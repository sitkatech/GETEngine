using System;

namespace Olsson.GET.Common.Utilities
{
    public static class DoubleExtensions
    {
        public static bool IsEqual(this double value, double comparison, double tolerance = .0000001)
        {
            return Math.Abs(value - comparison) < tolerance;
        }

        public static bool IsNotEqual(this double value, double comparison, double tolerance = .0000001)
        {
            return !value.IsEqual(comparison);
        }
    }
}