// porting-note: walk-back ECommons (~3.0.0.x) lacks several Checkbox overloads that Splatoon HEAD uses.
// This file adds the missing surface as no-frills wrappers around ImGui.Checkbox so the plugin compiles.

using Dalamud.Interface;
using Dalamud.Bindings.ImGui;
using System.Numerics;

namespace ECommons.ImGuiMethods;
public static unsafe partial class ImGuiEx
{
    public static bool Checkbox(string label, ref bool value)
    {
        return ImGui.Checkbox(label, ref value);
    }

    public static bool Checkbox(string label, ref bool value, bool inverted, bool dummy = false)
    {
        return ImGui.Checkbox(label, ref value);
    }

    public static bool Checkbox(string label, ref bool? value, bool inverted, bool dummy = false)
    {
        return Checkbox(label, ref value);
    }

    public static bool Checkbox(FontAwesomeIcon icon, string id, ref bool value)
    {
        ImGui.PushFont(UiBuilder.IconFont);
        var ret = ImGui.Checkbox($"{icon.ToIconString()}{id}", ref value);
        ImGui.PopFont();
        return ret;
    }

    public static bool Checkbox(FontAwesomeIcon icon, Vector4? c1, Vector4? c2, Vector4? c3, Vector4? c4, string id, ref bool value)
    {
        return Checkbox(icon, id, ref value);
    }
}
