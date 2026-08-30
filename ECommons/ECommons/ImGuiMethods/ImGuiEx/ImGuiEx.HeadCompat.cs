// porting-note: walk-back ECommons (~3.0.0.x) lacks several ImGuiEx helpers that Splatoon HEAD uses.
// This file adds the missing surface as no-frills wrappers / no-ops so the plugin compiles.

using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Bindings.ImGui;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace ECommons.ImGuiMethods;
public static unsafe partial class ImGuiEx
{
    // --- ButtonCheckbox(noColor:) — drop the named arg, fall through to standard color path. ---
    public static bool ButtonCheckbox(string name, ref bool value, bool noColor, Vector4? color = null)
    {
        return ButtonCheckbox(name, ref value, noColor ? (Vector4?)null : color);
    }

    // --- CollectionButtonCheckbox FontAwesomeIcon overload + Vector4 color slot 4. ---
    public static bool CollectionButtonCheckbox<T>(FontAwesomeIcon icon, T value, ICollection<T> collection, Vector4 color, bool smallButton = false, bool inverted = false)
    {
        ImGui.PushFont(UiBuilder.IconFont);
        var ret = CollectionButtonCheckbox(icon.ToIconString(), value, collection, color, smallButton, inverted);
        ImGui.PopFont();
        return ret;
    }

    // --- Stubs for table/dragdrop helpers that walk-back doesn't provide. ---
    public static void SimpleTableTextColumns(params string[] cols)
    {
        foreach(var c in cols)
        {
            ImGui.TableNextColumn();
            ImGui.TextUnformatted(c ?? string.Empty);
        }
    }

    public static void DragDropRepopulateClass(string id, string current, Action<string> setter)
    {
        // TODO(api12): HEAD uses this for inter-language string drag-drop reseed. No-op in walk-back.
    }

    // --- HelpMarker(string, EColor (Vector4), FontAwesomeIcon-as-string) overload. ---
    // HEAD uses ImGuiEx.HelpMarker(string, EColor, FontAwesomeIcon.X.ToIconString()) — already covered by walk-back's
    // (string, Vector4?, string, bool) overload via implicit conversion. No new overload needed.
}
