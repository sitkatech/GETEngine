using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Olsson.GET.Tests.EngineTests
{
    static class TestUtilities
    {
        public static double DetermineTestingDelta(double value1, double value2, int significantDigits = 5)
        {
            var expectedDigits = Math.Floor(Math.Log10(Math.Abs(value1)));
            var actualDigits = Math.Floor(Math.Log10(Math.Abs(value2)));
            return Math.Pow(10, Math.Min(expectedDigits, actualDigits) - (significantDigits - 1));
        }

        public static void AssertAreEqualWithCalculatedDelta(double expected, double actual, int significantDigits = 5)
        {
            Assert.AreEqual(expected, actual, DetermineTestingDelta(expected, actual, significantDigits));
        }
    }
}
