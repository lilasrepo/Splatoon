using ECommons.Hooks;
using Lumina.Excel.Sheets;
using Splatoon.Modules;
using Splatoon.SplatoonScripting;

namespace Splatoon.Memory;

internal static unsafe class DirectorUpdateProcessor
{
    // porting-note: walk-back ECommons DirectorUpdate.Init expects 7-param Action; HEAD signature has 9.
    // Drop a8/a9 — TC 7.1 director update hook doesn't pass them. ScriptingProcessor.OnDirectorUpdate
    // 9-arg overload is invoked with default zeros for the missing tail.
    internal static void ProcessDirectorUpdate(long a1, long a2, DirectorUpdateCategory a3, uint a4, uint a5, int a6, int a7)
    {
        if(P.Config.Logging)
        {
            var text = $"Director Update: {a3:X}, {a4:X8}, {a5:X8}, {a6:X8}, {a7:X8}";
            Logger.Log(text);
            PluginLog.Verbose(text);
            P.LogWindow.Log(text);
        }
        PhaseUpdater.UpdateFromDirector(a3);
        EmulatedCombatTimer.OnDirectorUpdate(a3);
        ScriptingProcessor.OnDirectorUpdate(a3);
        ScriptingProcessor.OnDirectorUpdate((nint)a1, (uint)a2, a3, a4, a5, a6, a7, 0, 0);
    }
}
