using Dalamud.Bindings.ImGui;
using System;
using System.Numerics;

namespace ECommons.ImGuiMethods;

/// <summary>
/// Gap-fill for the walk-back ECommons this tree pins (3.0.0.7-era).
///
/// Splatoon's upstream moved its style editors onto ImGuiEx.DragFloat / ImGuiEx.DragInt, which that
/// ECommons revision does not have — the refresh then failed with CS0117 "'ImGuiEx' does not contain
/// a definition for 'DragFloat'" at eight call sites. These four overloads are copied verbatim from a
/// newer ECommons (JP/AutoDuty/ECommons, ImGuiMethods/ImGuiEx/Inputs.cs lines 90-193) rather than
/// reimplemented, so the nullable-with-checkbox behaviour matches what upstream's call sites expect.
///
/// They depend only on members the walk-back already has (ImGuiEx.Checkbox, ImGuiEx.TextV) plus
/// ImU8String, which exists at api13 because the reference set ships Dalamud.Bindings.ImGui.
/// Drop this file if the ECommons pin is ever advanced past the revision that introduced them.
/// The pinned ImGuiEx.Checkbox has no  parameter, so that argument is dropped here.
/// Behaviour-neutral for this tree: every Splatoon call site leaves  at its default
/// true, so the checkbox was never going to be greyed out anyway.
/// </summary>
public static partial class ImGuiEx
{
    public static bool DragInt(float width, string id, ref int? nullableValueRef, float vSpeed = 1.0f, int vMin = 0, int vMax = 0, ImU8String format = default, ImGuiSliderFlags flags = ImGuiSliderFlags.None, bool enabled = true, int defaultValue = default, bool isLabelPrefix = false, bool showCheckbox = true)
    {
        var ret = false;
        var checkboxId = $"##{id}_checkbox";
        var b = nullableValueRef != null;
        if(isLabelPrefix && !id.StartsWith("##"))
        {
            if(!enabled) ImGui.PushStyleVar(ImGuiStyleVar.Alpha, 0.6f);
            ImGuiEx.TextV(id);
            if(!enabled) ImGui.PopStyleVar();
            ImGui.SameLine();
        }
        if(showCheckbox)
        {
            if(ImGuiEx.Checkbox(checkboxId, ref b))
            {
                nullableValueRef = b ? defaultValue : null;
                ret = true;
            }
            ImGui.SameLine();
        }
        var value = nullableValueRef ?? defaultValue;
        if(ImGuiEx.DragInt(width, "##dragfloat_" + id, ref value, enabled ? vSpeed : 0, vMin, vMax, format, flags, enabled && b))
        {
            nullableValueRef = value;
            ret = true;
        }
        if(!isLabelPrefix && !id.StartsWith("##"))
        {
            ImGui.SameLine();
            if(!enabled) ImGui.PushStyleVar(ImGuiStyleVar.Alpha, 0.6f);
            ImGuiEx.TextV(id);
            if(!enabled) ImGui.PopStyleVar();
        }
        return ret;
    }

    public static bool DragInt(float width, string id, ref int valueRef, float vSpeed = 1.0f, int vMin = 0, int vMax = 0, ImU8String format = default, ImGuiSliderFlags flags = ImGuiSliderFlags.None, bool enabled = true)
    {
        if(!enabled) ImGui.PushStyleVar(ImGuiStyleVar.Alpha, 0.6f);
        var ret = false;
        var value = valueRef;
        if(width != 0) ImGui.SetNextItemWidth(width);
        if(ImGui.DragInt(id, ref value, vSpeed, vMin, vMax, format, flags) && enabled)
        {
            valueRef = value;
            ret = true;
        }
        if(!enabled) ImGui.PopStyleVar();
        return ret;
    }

    public static bool DragFloat(float width, string id, ref float? nullableValueRef, float vSpeed = 1.0f, float vMin = 0.0f, float vMax = 0.0f, ImU8String format = default, ImGuiSliderFlags flags = ImGuiSliderFlags.None, bool enabled = true, float defaultValue = default, bool isLabelPrefix = false, bool showCheckbox = true)
    {
        var ret = false;
        var checkboxId = $"##{id}_checkbox";
        var b = nullableValueRef != null;
        if(isLabelPrefix && !id.StartsWith("##"))
        {
            if(!enabled) ImGui.PushStyleVar(ImGuiStyleVar.Alpha, 0.6f);
            ImGuiEx.TextV(id);
            if(!enabled) ImGui.PopStyleVar();
            ImGui.SameLine();
        }
        if(showCheckbox)
        {
            if(ImGuiEx.Checkbox(checkboxId, ref b))
            {
                nullableValueRef = b ? defaultValue : null;
                ret = true;
            }
            ImGui.SameLine();
        }
        var value = nullableValueRef ?? defaultValue;
        if(ImGuiEx.DragFloat(width, "##dragfloat_" + id, ref value, enabled?vSpeed:0, vMin, vMax, format, flags, enabled && b))
        {
            nullableValueRef = value;
            ret = true;
        }
        if(!isLabelPrefix && !id.StartsWith("##"))
        {
            ImGui.SameLine();
            if(!enabled) ImGui.PushStyleVar(ImGuiStyleVar.Alpha, 0.6f);
            ImGuiEx.TextV(id);
            if(!enabled) ImGui.PopStyleVar();
        }
        return ret;
    }

    public static bool DragFloat(float width, string id, ref float valueRef, float vSpeed = 1.0f, float vMin = 0.0f, float vMax = 0.0f, ImU8String format = default, ImGuiSliderFlags flags = ImGuiSliderFlags.None, bool enabled = true)
    {
        if(!enabled) ImGui.PushStyleVar(ImGuiStyleVar.Alpha, 0.6f);
        var ret = false;
        var value = valueRef;
        if(width != 0) ImGui.SetNextItemWidth(width);
        if(ImGui.DragFloat(id, ref value, vSpeed, vMin, vMax, format, flags) && enabled)
        {
            valueRef = value;
            ret = true;
        }
        if(!enabled) ImGui.PopStyleVar();
        return ret;
    }


    public static bool ColorEdit4(float width, string id, ref uint? nullableValueRef, ImGuiColorEditFlags flags = ImGuiColorEditFlags.NoInputs, bool enabled = true, uint defaultValue = default, bool isLabelPrefix = false, bool showCheckbox = true)
    {
        var ret = false;
        Vector4? value = nullableValueRef?.ToVector4();
        if(ColorEdit4(width, id, ref value, flags, enabled, defaultValue.ToVector4(), isLabelPrefix, showCheckbox))
        {
            ret = true;
            nullableValueRef = value?.ToUint();
        }
        return ret;
    }

    public static bool ColorEdit4(float width, string id, ref Vector4? nullableValueRef, ImGuiColorEditFlags flags = ImGuiColorEditFlags.NoInputs, bool enabled = true, Vector4 defaultValue = default, bool isLabelPrefix = false, bool showCheckbox = true)
    {
        var ret = false;
        var checkboxId = $"##{id}_checkbox";
        var b = nullableValueRef != null;
        if(isLabelPrefix && !id.StartsWith("##"))
        {
            if(!enabled) ImGui.PushStyleVar(ImGuiStyleVar.Alpha, 0.6f);
            ImGuiEx.TextV(id);
            if(!enabled) ImGui.PopStyleVar();
            ImGui.SameLine();
        }
        if(showCheckbox)
        {
            if(ImGuiEx.Checkbox(checkboxId, ref b))
            {
                nullableValueRef = b ? defaultValue : null;
                ret = true;
            }
            ImGui.SameLine();
        }
        var value = nullableValueRef ?? defaultValue;
        if(width != 0) ImGui.SetNextItemWidth(width);
        if(ImGuiEx.ColorEdit4(width, "##coloredit4_" + id, ref value, flags, enabled && b))
        {
            nullableValueRef = value;
            ret = true;
        }
        if(!isLabelPrefix && !id.StartsWith("##"))
        {
            ImGui.SameLine();
            if(!enabled) ImGui.PushStyleVar(ImGuiStyleVar.Alpha, 0.6f);
            ImGuiEx.TextV(id);
            if(!enabled) ImGui.PopStyleVar();
        }
        return ret;
    }

    public static bool ColorEdit4(float width, string id, ref uint valueRef, ImGuiColorEditFlags flags = ImGuiColorEditFlags.NoInputs, bool enabled = true)
    {
        var ret = false;
        var value = valueRef.ToVector4();
        if(ColorEdit4(width, id, ref value, flags, enabled))
        {
            ret = true;
            valueRef = value.ToUint();
        }
        return ret;
    }

    public static bool ColorEdit4(float width, string id, ref Vector4 valueRef, ImGuiColorEditFlags flags = ImGuiColorEditFlags.NoInputs, bool enabled = true)
    {
        if(!enabled) ImGui.PushStyleVar(ImGuiStyleVar.Alpha, 0.6f);
        ImGui.SetNextItemWidth(width);
        var ret = false;
        var value = valueRef;
        if(ImGui.ColorEdit4(id, ref value, flags) && enabled)
        {
            valueRef = value;
            ret = true;
        }
        if(!enabled) ImGui.PopStyleVar();
        return ret;
    }
}
