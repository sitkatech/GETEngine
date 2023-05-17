using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Olsson.GET.Tests.EngineTests
{
    static class TestUtilities
    {
        private static double DetermineTestingDelta(double expected, int significantDigits = 5)
        {
            if (expected == 0)
            {
                return Math.Pow(10, 1 - (significantDigits - 1));
            }
            var expectedDigits = Math.Floor(Math.Log10(Math.Abs(expected)));
            return Math.Pow(10, expectedDigits - (significantDigits - 1));
        }

        public static void AssertAreEqualWithCalculatedDelta(double expected, double actual, int significantDigits = 5)
        {
            Assert.AreEqual(expected, actual, DetermineTestingDelta(expected, significantDigits));
        }
    }
}
