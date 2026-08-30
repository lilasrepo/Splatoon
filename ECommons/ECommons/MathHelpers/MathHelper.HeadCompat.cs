using System.Numerics;

namespace ECommons.MathHelpers;

/// <summary>
/// Gap-fill for the walk-back ECommons this tree pins.
///
/// Splatoon's render engines call MathHelper.SwapYZ (18 sites across DirectX11Renderer and
/// ImGuiLegacyRenderer) after this refresh; the pinned ECommons revision predates it. Copied from a
/// newer ECommons (JP/AutoDuty/ECommons, MathHelpers/MathHelper.cs) rather than reinvented -- the
/// axis order matters and getting it backwards would draw everything in the wrong place while still
/// compiling. Declared as its own static class rather than a partial of MathHelper, because the pinned
/// MathHelper is not declared partial and editing the vendored file would be lost on the next refresh.
/// Extension-method lookup does not care which class it lives in, so  still resolves;
/// the handful of  static-style call sites are rewritten to .
/// Drop this file if the ECommons pin is advanced past the revision that added it.
/// </summary>
public static class MathHelperHeadCompat
{
    public static Vector3 SwapYZ(this Vector3 v)
    {
        return new(v.X, v.Z, v.Y);
    }
}
