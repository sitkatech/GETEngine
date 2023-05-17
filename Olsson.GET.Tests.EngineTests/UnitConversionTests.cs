using Microsoft.VisualStudio.TestTools.UnitTesting;
using Olsson.GET.Accessors.EntityFramework;
using Olsson.GET.Engines.ModelInputOutputEngines;

namespace Olsson.GET.Tests.EngineTests
{
    [TestClass]
    public class UnitConversionTests
    {
        [DataRow(1, VolumeUnitEnum.AcreFeet, VolumeUnitEnum.AcreFeet, 30, 1)]
        [DataRow(1, VolumeUnitEnum.CubicFeet, VolumeUnitEnum.AcreFeet, 30, 6.88705e-4)]
        [DataRow(1, VolumeUnitEnum.CubicMeter, VolumeUnitEnum.AcreFeet, 30, 0.0243214)]
        [DataRow(1, VolumeUnitEnum.CubicYard, VolumeUnitEnum.AcreFeet, 30, 0.0185950)]
        [DataRow(1, VolumeUnitEnum.Gallon, VolumeUnitEnum.AcreFeet, 30, 0.132576)]
        [DataRow(1, VolumeUnitEnum.GallonsInMillions, VolumeUnitEnum.AcreFeet, 30, 92.0665)]

        [DataRow(1, VolumeUnitEnum.AcreFeet, VolumeUnitEnum.AcreFeet, 28, 1)]
        [DataRow(1, VolumeUnitEnum.CubicFeet, VolumeUnitEnum.AcreFeet, 28, 6.42792e-4)]
        [DataRow(1, VolumeUnitEnum.CubicMeter, VolumeUnitEnum.AcreFeet, 28, 0.0227000)]
        [DataRow(1, VolumeUnitEnum.CubicYard, VolumeUnitEnum.AcreFeet, 28, 0.0173554)]
        [DataRow(1, VolumeUnitEnum.Gallon, VolumeUnitEnum.AcreFeet, 28, 0.123737)]
        [DataRow(1, VolumeUnitEnum.GallonsInMillions, VolumeUnitEnum.AcreFeet, 28, 85.9287)]

        [DataRow(1, VolumeUnitEnum.CubicFeet, VolumeUnitEnum.CubicFeet, 30, 1)]
        [DataRow(1, VolumeUnitEnum.AcreFeet, VolumeUnitEnum.CubicFeet, 30, 1452.00)]
        [DataRow(1, VolumeUnitEnum.AcreFeet, VolumeUnitEnum.CubicFeet, 28, 1555.71)]
        [DataRow(1, VolumeUnitEnum.CubicMeter, VolumeUnitEnum.CubicFeet, 30, 35.3147)]
        [DataRow(1, VolumeUnitEnum.CubicYard, VolumeUnitEnum.CubicFeet, 30, 27)]
        [DataRow(1, VolumeUnitEnum.Gallon, VolumeUnitEnum.CubicFeet, 30, 192.500)]
        [DataRow(1, VolumeUnitEnum.GallonsInMillions, VolumeUnitEnum.CubicFeet, 30, 133681)]

        [DataRow(1, VolumeUnitEnum.CubicMeter, VolumeUnitEnum.CubicMeter, 30, 1)]
        [DataRow(1, VolumeUnitEnum.AcreFeet, VolumeUnitEnum.CubicMeter, 30, 41.1161)]
        [DataRow(1, VolumeUnitEnum.AcreFeet, VolumeUnitEnum.CubicMeter, 28, 44.0529)]
        [DataRow(1, VolumeUnitEnum.CubicFeet, VolumeUnitEnum.CubicMeter, 30, 0.0283168)]
        [DataRow(1, VolumeUnitEnum.CubicYard, VolumeUnitEnum.CubicMeter, 30, 0.764555)]
        [DataRow(1, VolumeUnitEnum.Gallon, VolumeUnitEnum.CubicMeter, 30, 5.45099)]
        [DataRow(1, VolumeUnitEnum.GallonsInMillions, VolumeUnitEnum.CubicMeter, 30, 3785.41)]

        [DataRow(1, VolumeUnitEnum.CubicYard, VolumeUnitEnum.CubicYard, 30, 1)]
        [DataRow(1, VolumeUnitEnum.AcreFeet, VolumeUnitEnum.CubicYard, 30, 53.7778)]
        [DataRow(1, VolumeUnitEnum.AcreFeet, VolumeUnitEnum.CubicYard, 28, 57.6190)]
        [DataRow(1, VolumeUnitEnum.CubicMeter, VolumeUnitEnum.CubicYard, 30, 1.30795)]
        [DataRow(1, VolumeUnitEnum.CubicFeet, VolumeUnitEnum.CubicYard, 30, 0.037037)]
        [DataRow(1, VolumeUnitEnum.Gallon, VolumeUnitEnum.CubicYard, 30, 7.12963)]
        [DataRow(1, VolumeUnitEnum.GallonsInMillions, VolumeUnitEnum.CubicYard, 30, 4951.13)]

        [DataRow(1, VolumeUnitEnum.Gallon, VolumeUnitEnum.Gallon, 30, 1)]
        [DataRow(1, VolumeUnitEnum.AcreFeet, VolumeUnitEnum.Gallon, 30, 7.54286)]
        [DataRow(1, VolumeUnitEnum.AcreFeet, VolumeUnitEnum.Gallon, 28, 8.08163)]
        [DataRow(1, VolumeUnitEnum.CubicMeter, VolumeUnitEnum.Gallon, 30, 0.183453)]
        [DataRow(1, VolumeUnitEnum.CubicFeet, VolumeUnitEnum.Gallon, 30, 0.00519481)]
        [DataRow(1, VolumeUnitEnum.CubicYard, VolumeUnitEnum.Gallon, 30, 0.140260)]
        [DataRow(1, VolumeUnitEnum.GallonsInMillions, VolumeUnitEnum.Gallon, 30, 694.444)]

        [DataRow(1, VolumeUnitEnum.GallonsInMillions, VolumeUnitEnum.GallonsInMillions, 30, 1)]
        [DataRow(1, VolumeUnitEnum.AcreFeet, VolumeUnitEnum.GallonsInMillions, 30, 0.0108617)]
        [DataRow(1, VolumeUnitEnum.AcreFeet, VolumeUnitEnum.GallonsInMillions, 28, 0.0116376)]
        [DataRow(1, VolumeUnitEnum.CubicMeter, VolumeUnitEnum.GallonsInMillions, 30, 0.000264172)]
        [DataRow(1, VolumeUnitEnum.CubicFeet, VolumeUnitEnum.GallonsInMillions, 30, 0.00000748052)]
        [DataRow(1, VolumeUnitEnum.CubicYard, VolumeUnitEnum.GallonsInMillions, 30, 0.000201974)]
        [DataRow(1, VolumeUnitEnum.Gallon, VolumeUnitEnum.GallonsInMillions, 30, 0.00144000)]

        [DataRow(-3.123456, VolumeUnitEnum.CubicFeet, VolumeUnitEnum.CubicMeter, 30, -0.088446424389)]
        [DataRow(0, VolumeUnitEnum.CubicFeet, VolumeUnitEnum.AcreFeet, 30, 0)]

        [DataTestMethod]
        public void ConvertFlowTestToFifthSignificantNumber(double currentValue, VolumeUnitEnum currentVolumeUnitEnum, VolumeUnitEnum newVolumeUnitEnum, int daysInMonth, double expectedValue)
        {
            var convertedValue = UnitConversion.ConvertFlow(currentValue, (int) currentVolumeUnitEnum, (int) newVolumeUnitEnum, daysInMonth);

            TestUtilities.AssertAreEqualWithCalculatedDelta(expectedValue, convertedValue);
        }

        [DataRow(1, VolumeUnitEnum.AcreFeet, VolumeUnitEnum.AcreFeet, 1)]
        [DataRow(1, VolumeUnitEnum.CubicFeet, VolumeUnitEnum.AcreFeet, 2.2957e-5)]
        [DataRow(1, VolumeUnitEnum.CubicMeter, VolumeUnitEnum.AcreFeet, 0.000810714)]
        [DataRow(1, VolumeUnitEnum.CubicYard, VolumeUnitEnum.AcreFeet, 0.000619836)]
        [DataRow(1, VolumeUnitEnum.Gallon, VolumeUnitEnum.AcreFeet, 3.0689e-6)]
        [DataRow(1, VolumeUnitEnum.GallonsInMillions, VolumeUnitEnum.AcreFeet, 3.0688)]

        [DataRow(1, VolumeUnitEnum.CubicFeet, VolumeUnitEnum.CubicFeet, 1)]
        [DataRow(1, VolumeUnitEnum.AcreFeet, VolumeUnitEnum.CubicFeet, 43559.9)]
        [DataRow(1, VolumeUnitEnum.CubicMeter, VolumeUnitEnum.CubicFeet, 35.3147)]
        [DataRow(1, VolumeUnitEnum.CubicYard, VolumeUnitEnum.CubicFeet, 27)]
        [DataRow(1, VolumeUnitEnum.Gallon, VolumeUnitEnum.CubicFeet, 0.133681)]
        [DataRow(1, VolumeUnitEnum.GallonsInMillions, VolumeUnitEnum.CubicFeet, 133681)]

        [DataRow(1, VolumeUnitEnum.CubicMeter, VolumeUnitEnum.CubicMeter, 1)]
        [DataRow(1, VolumeUnitEnum.AcreFeet, VolumeUnitEnum.CubicMeter, 1233.48)]
        [DataRow(1, VolumeUnitEnum.CubicFeet, VolumeUnitEnum.CubicMeter, 0.0283168)]
        [DataRow(1, VolumeUnitEnum.CubicYard, VolumeUnitEnum.CubicMeter, 0.764555)]
        [DataRow(1, VolumeUnitEnum.Gallon, VolumeUnitEnum.CubicMeter, 0.00378541)]
        [DataRow(1, VolumeUnitEnum.GallonsInMillions, VolumeUnitEnum.CubicMeter, 3785.41)]

        [DataRow(1, VolumeUnitEnum.CubicYard, VolumeUnitEnum.CubicYard, 1)]
        [DataRow(1, VolumeUnitEnum.AcreFeet, VolumeUnitEnum.CubicYard, 1613.33)]
        [DataRow(1, VolumeUnitEnum.CubicMeter, VolumeUnitEnum.CubicYard, 1.30795)]
        [DataRow(1, VolumeUnitEnum.CubicFeet, VolumeUnitEnum.CubicYard, 0.037037)]
        [DataRow(1, VolumeUnitEnum.Gallon, VolumeUnitEnum.CubicYard, 0.00495113)]
        [DataRow(1, VolumeUnitEnum.GallonsInMillions, VolumeUnitEnum.CubicYard, 4951.13)]

        [DataRow(1, VolumeUnitEnum.Gallon, VolumeUnitEnum.Gallon, 1)]
        [DataRow(1, VolumeUnitEnum.AcreFeet, VolumeUnitEnum.Gallon, 325851)]
        [DataRow(1, VolumeUnitEnum.CubicMeter, VolumeUnitEnum.Gallon, 264.172)]
        [DataRow(1, VolumeUnitEnum.CubicFeet, VolumeUnitEnum.Gallon, 7.48052)]
        [DataRow(1, VolumeUnitEnum.CubicYard, VolumeUnitEnum.Gallon, 201.974)]
        [DataRow(1, VolumeUnitEnum.GallonsInMillions, VolumeUnitEnum.Gallon, 1e6)]

        [DataRow(1, VolumeUnitEnum.GallonsInMillions, VolumeUnitEnum.GallonsInMillions, 1)]
        [DataRow(1, VolumeUnitEnum.AcreFeet, VolumeUnitEnum.GallonsInMillions, .325851)]
        [DataRow(1, VolumeUnitEnum.CubicMeter, VolumeUnitEnum.GallonsInMillions, .000264172)]
        [DataRow(1, VolumeUnitEnum.CubicFeet, VolumeUnitEnum.GallonsInMillions, .00000748052)]
        [DataRow(1, VolumeUnitEnum.CubicYard, VolumeUnitEnum.GallonsInMillions, .000201974)]
        [DataRow(1, VolumeUnitEnum.Gallon, VolumeUnitEnum.GallonsInMillions, 0.000001)]

        [DataRow(-3.123456, VolumeUnitEnum.CubicFeet, VolumeUnitEnum.CubicMeter, -0.088446424389)]
        [DataRow(0, VolumeUnitEnum.CubicFeet, VolumeUnitEnum.AcreFeet, 0)]

        [DataTestMethod]
        public void ConvertVolumeTestToFifthSignificantNumber(double currentValue, VolumeUnitEnum currentVolumeUnitEnum, VolumeUnitEnum newVolumeUnitEnum, double expectedValue)
        {
            var convertedValue = UnitConversion.ConvertVolume(currentValue, currentVolumeUnitEnum, newVolumeUnitEnum);

            TestUtilities.AssertAreEqualWithCalculatedDelta(expectedValue, convertedValue);
        }
    }
}
