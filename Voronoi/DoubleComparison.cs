using System;

namespace Voronoi
{
    internal static class DoubleComparison
    {
        internal static bool IsEqual(double a, double b) => Math.Abs(a - b) < 0.00000000001;
    }
}
