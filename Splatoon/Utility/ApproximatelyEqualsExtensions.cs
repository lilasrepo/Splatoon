using System;
using System.Numerics;

namespace Splatoon.Utility;

// porting-note: HEAD relies on ECommons' ApproximatelyEquals helpers; walk-back ECommons
// (substituted into TC_forward/Splatoon/ECommons/) lacks them. Re-introduce the surface
// inline so script files and Splatoon proper compile under API12.
public static class ApproximatelyEqualsExtensions
{
    public static bool ApproximatelyEquals(this float a, float b, float tolerance) => Math.Abs(a - b) <= tolerance;
    public static bool ApproximatelyEquals(this Vector3 a, Vector3 b, float tolerance) => Vector3.Distance(a, b) <= tolerance;
}
