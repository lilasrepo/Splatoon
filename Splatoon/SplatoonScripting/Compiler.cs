using ECommons.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System.Reflection;
using System.Runtime.Loader;
using System.Text.RegularExpressions;

namespace Splatoon.SplatoonScripting;

internal class Compiler
{
    internal static Assembly Load(byte[] assembly, byte[] pdb)
    {
        PluginLog.Debug($"Beginning assembly load");
        // porting-note: HEAD reflects into the plugin's "loader" field and casts it as AssemblyLoadContext.
        // In API12 the field type is Dalamud.Plugin.Internal.Loader.PluginLoader, NOT an AssemblyLoadContext —
        // the cast throws. Walk-back path (used here) uses the AssemblyLoadContext that DalamudReflector
        // already returns as the 2nd tuple element, which works on both API15 and API12.
        if(DalamudReflector.TryGetLocalPlugin(out var instance, out var context, out var type))
        {
            using var stream = new MemoryStream(assembly);
            using var streamPdb = new MemoryStream(pdb);
            try
            {
                return context.LoadFromStream(stream, streamPdb);
            }
            catch(Exception e)
            {
                e.LogDuo();
            }
        }
        return null;
    }

    internal static (byte[] Assembly, byte[] Pdb)? Compile(string sourceCode, string identity, string path = null)
    {
        using var peStream = new MemoryStream();
        using var pdbStream = new MemoryStream();
        var code = GenerateCode(sourceCode, identity);
        var result = code.Emit(peStream, pdbStream);

        if(!result.Success)
        {
            var ns = ScriptingProcessor.ExtractNamespaceFromCode(sourceCode);
            var cls = ScriptingProcessor.ExtractClassFromCode(sourceCode);
            //var updatePath = $"https://github.com/PunishXIV/Splatoon/raw/main/SplatoonScripts/{ns.ReplaceFirst("SplatoonScriptsOfficial.","").Replace("_", " ").Replace(".", "/")}/{cls.Replace("_", " ")}.cs";
            var updateName = $"{ns}@{cls}";
            PluginLog.Warning($"Compilation done with error ({identity}, {updateName})");

            if(ScriptingProcessor.ForceUpdate != null)
            {
                ScriptingProcessor.ForceUpdate.Add(updateName);
                PluginLog.Warning($"An attempt to update {updateName} will be made if it will be found in the update list");
            }
            var failures = result.Diagnostics.Where(diagnostic => diagnostic.IsWarningAsError || diagnostic.Severity == DiagnosticSeverity.Error);

            foreach(var diagnostic in failures)
            {
                PluginLog.Warning($"{diagnostic.Id}: {diagnostic.GetMessage()}");
            }
            Svc.Framework.RunOnFrameworkThread(() =>
            {
                if(P == null || P.Disposed || P.ScriptUpdateWindow == null) return; // porting-note(api12): runs on the compiler background thread via .Wait(); guard against disposal race (P/P.ScriptUpdateWindow null mid-compile NREs, rethrown wrapped by .Wait())
                P.ScriptUpdateWindow.FailedScripts_Add(path);
            }).Wait();

            return null;
        }

        PluginLog.Debug("Compilation done without any error.");

        peStream.Seek(0, SeekOrigin.Begin);
        pdbStream.Seek(0, SeekOrigin.Begin);

        return (peStream.ToArray(), pdbStream.ToArray());
    }

    private static CSharpCompilation GenerateCode(string sourceCode, string identity = "Script")
    {
        // porting-note: TC API12 build - community scripts are written against API15. Rewrite the API15
        // surface (Bindings.ImGui, ObjectId, IStatus, LegacyPlayer) to API12 equivalents before compile.
        sourceCode = PatchScriptForApi12(sourceCode);

        var codeString = SourceText.From(sourceCode);
        var options = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview);

        var parsedSyntaxTree = SyntaxFactory.ParseSyntaxTree(codeString, options);
        var refs = ReferenceCache.ReferenceList;
        //PluginLog.Information($"References: {references.Select(x => x.Display).Join(", ")}");

        var id = $"SplatoonScript-{identity}-{Guid.NewGuid()}";
        PluginLog.Debug($"Assembly name: {id}");
        return CSharpCompilation.Create(id,
            new[] { parsedSyntaxTree },
            references: refs,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,
                optimizationLevel: OptimizationLevel.Release,
                assemblyIdentityComparer: DesktopAssemblyIdentityComparer.Default,
                allowUnsafe: true));
    }

    // porting-note: rewrites HEAD-style (API15) script source to compile against TC API12 references.
    // Invoked from GenerateCode before SyntaxTree parsing. Only string-level surgery - no Roslyn analysis.
    private static string PatchScriptForApi12(string src)
    {
        // Namespace renames (using directives)
        // porting-note(api13): the Bindings.ImGui -> ImGuiNET rewrites that used to sit here
        // are REMOVED. At API13 the TC runtime ships Dalamud.Bindings.ImGui natively, so
        // community scripts already name it correctly; rewriting it back to ImGuiNET makes
        // every ImGui-using script fail to compile AT RUNTIME. Invisible to dotnet build,
        // which is why it outlived the api13 sweep.
        // porting-note(api13, 2026-09-03): the OTHER direction is still needed. 41 of the operator's on-disk
        // scripts pre-date upstream's Bindings.ImGui move and say `using ImGuiNET;` -- api13 has no
        // ImGui.NET.dll, so they fail CS0246/CS0103 at runtime (measured 2026-09-03: 1070 `ImGui` misses).
        // Same rewrite plugin_update.py applies to plugin source at api13.
        src = Regex.Replace(src, @"\busing\s+ImGuiNET\s*;", "using Dalamud.Bindings.ImGui;");
        src = src.Replace("ImGuiNET.", "Dalamud.Bindings.ImGui.");
        // ECommons 3.2.1.15 keeps IPlayerCharacter.GetJob() in GameHelpers.LegacyPlayer; older scripts
        // call it with only `using ECommons.GameHelpers;`. Add the namespace only when needed -- importing
        // both unconditionally makes GetNameWithWorld ambiguous.
        if(src.Contains(".GetJob()") && !Regex.IsMatch(src, @"\busing\s+ECommons\.GameHelpers\.LegacyPlayer\s*;"))
            src = new Regex(@"\busing\s+ECommons\.GameHelpers\s*;").Replace(src, "using ECommons.GameHelpers;" + System.Environment.NewLine + "using ECommons.GameHelpers.LegacyPlayer;", 1);

        // porting-note(api13, 2026-09-02): the LegacyPlayer / CSExtensions rewrites that used to sit here are
        // REMOVED too -- the vendored ECommons is 3.2.1.15 now, which ships both namespaces, so scripts
        // compile against the surface they were written for.

        // IPlayerCharacter / IBattleNpc / IGameObject .ObjectId -> .EntityId, BUT preserve FFXIVClientStructs
        // GameObjectId struct field accesses (GameObjectId has .ObjectId, not .EntityId — renaming breaks compile):
        //  (a) .TargetId.ObjectId / .<X>Id.ObjectId            — struct field whose owner name ends in Id
        //  (b) MarkingController.Markers[N].ObjectId           — indexer returns a GameObjectId struct
        //  (c) v.ObjectId where v.ObjectId.GetObject() exists  — v is a GameObjectId local (e.g. `var x = markers[i]`);
        //                                                         you call GetObject() on the uint id, never on an IGameObject
        // Also skip entirely when a script defines its own `public uint/ulong ObjectId;` field (renaming would break it).
        var scriptDefinesObjectIdField = Regex.IsMatch(src, @"\bpublic\s+(uint|ulong)\s+ObjectId\b");
        if(!scriptDefinesObjectIdField)
        {
            src = Regex.Replace(src, @"(\.[A-Z][A-Za-z0-9_]*Id)\.ObjectId\b", "$1__PROTECTED__OBJECTID__"); // (a)
            src = Regex.Replace(src, @"(Markers\[[^\]]+\])\.ObjectId\b", "$1__PROTECTED__OBJECTID__");       // (b)
            // (c) runs after (a)/(b): any bare `v.ObjectId.GetObject()` still present means v is a GameObjectId,
            //     so protect every .ObjectId on that variable (matches MatchCollection are taken against the pre-loop src).
            foreach(Match m in Regex.Matches(src, @"\b(\w+)\.ObjectId\.GetObject\(\)"))
            {
                var v = m.Groups[1].Value;
                src = Regex.Replace(src, @"\b" + Regex.Escape(v) + @"\.ObjectId\b", v + "__PROTECTED__OBJECTID__");
            }
            src = Regex.Replace(src, @"\.ObjectId\b", ".EntityId");
            src = src.Replace("__PROTECTED__OBJECTID__", ".ObjectId");
        }

        // IStatus -> Dalamud Status (chrome class)
        src = Regex.Replace(src, @"\bIStatus\b", "Dalamud.Game.ClientState.Statuses.Status");

        // FFXIVClientStructs MarkingController.Markers[X] returns GameObjectId struct in API12,
        // but HEAD scripts compare it directly to uint EntityId — requires .ObjectId deref.
        src = Regex.Replace(src, @"(->Markers\[[^\]]+\])\s*==\s*", "$1.ObjectId == ");
        src = Regex.Replace(src, @"\s*==\s*(\w[\w\.]*->Markers\[[^\]]+\])", " == $1.ObjectId");

        return src;
    }
}
