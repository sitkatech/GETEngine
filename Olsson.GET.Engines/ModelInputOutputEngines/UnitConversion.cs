using System;
using Olsson.GET.Accessors.EntityFramework;
using Olsson.GET.Common.DataContracts.Runs;

namespace Olsson.GET.Engines.ModelInputOutputEngines
{
    internal static class UnitConversion
    {
        private const double Million = 1e6;

        private const double CubicFeetPerAcreFoot = 43560;
        private const double CubicFeetPerCubicYard = 27;
        private const double CubicFeetPerCubicMeter = 35.314666721489;
        private const double CubicFeetPerUsGallon = 0.13368055555556;

        public static double CalculateVolumePerTimeStep(double volume, StressPeriod stressPeriod)
        {
            return volume * (stressPeriod.Days / stressPeriod.NumberOfTimeSteps);
        }

        public static double ConvertFlow(double value, int currentVolumeUnitID, int newVolumeUnitID, int daysInMonth)
        {
            var currentVolumeUnitEnum = VolumeUnit.AllLookupDictionary[currentVolumeUnitID].ToEnum;
            var newVolumeUnitEnum = VolumeUnit.AllLookupDictionary[newVolumeUnitID].ToEnum;
            double convertedVolume = ConvertVolume(value, currentVolumeUnitEnum, newVolumeUnitEnum);
            return convertedVolume * (GetDefaultFlowPeriod(newVolumeUnitEnum, daysInMonth).TotalMilliseconds / GetDefaultFlowPeriod(currentVolumeUnitEnum, daysInMonth).TotalMilliseconds);
        }

        public static double ConvertVolume(double value, VolumeUnitEnum currentVolumeUnitEnum, VolumeUnitEnum newVolumeUnitEnum)
        {
            var cubicFeet = ToCubicFeet(value, currentVolumeUnitEnum);
            switch (newVolumeUnitEnum)
            {
                case VolumeUnitEnum.AcreFeet:
                    return ToAcreFeet(cubicFeet);
                case VolumeUnitEnum.CubicFeet:
                    return ToCubicFeet(cubicFeet);
                case VolumeUnitEnum.CubicYard:
                    return ToCubicYards(cubicFeet);
                case VolumeUnitEnum.CubicMeter:
                    return ToCubicMeters(cubicFeet);
                case VolumeUnitEnum.Gallon:
                    return ToUsGallons(cubicFeet);
                case VolumeUnitEnum.GallonsInMillions:
                    return ToMillionUsGallons(cubicFeet);
                default:
                    throw new NotSupportedException($"No conversion exists for cubic feet to {newVolumeUnitEnum}");
            }
        }

        private static TimeSpan GetDefaultFlowPeriod(VolumeUnitEnum volumeUnitEnum, int daysInMonth)
        {
            switch (volumeUnitEnum)
            {
                case VolumeUnitEnum.AcreFeet:
                    return TimeSpan.FromDays(daysInMonth);
                case VolumeUnitEnum.CubicFeet:
                    return TimeSpan.FromDays(1);
                case VolumeUnitEnum.CubicYard:
                    return TimeSpan.FromDays(1);
                case VolumeUnitEnum.CubicMeter:
                    return TimeSpan.FromDays(1);
                case VolumeUnitEnum.Gallon:
                    return TimeSpan.FromMinutes(1);
                case VolumeUnitEnum.GallonsInMillions:
                    return TimeSpan.FromDays(1);
                default:
                    throw new NotSupportedException($"No conversion exists for cubic feet to {volumeUnitEnum}");
            }
        }
        private static double ToCubicFeet(double cubicFeet)
        {
            return cubicFeet;
        }
        private static double ToCubicMeters(double cubicFeet)
        {
            return cubicFeet * (1 / CubicFeetPerCubicMeter);
        }
        private static double ToAcreFeet(double cubicFeet)
        {
            return cubicFeet * (1 / CubicFeetPerAcreFoot);
        }
        private static double ToUsGallons(double cubicFeet)
        {
            return cubicFeet * (1 / CubicFeetPerUsGallon);
        }
        private static double ToMillionUsGallons(double cubicFeet)
        {
            return ToUsGallons(cubicFeet) / Million;
        }
        private static double ToCubicYards(double cubicFeet)
        {
            return cubicFeet * (1 / CubicFeetPerCubicYard);
        }
        private static double ToCubicFeet(double value, VolumeUnitEnum volumeUnitEnum)
        {
            switch (volumeUnitEnum)
            {
                case VolumeUnitEnum.AcreFeet:
                    return value * CubicFeetPerAcreFoot;
                case VolumeUnitEnum.CubicFeet:
                    return value;
                case VolumeUnitEnum.CubicYard:
                    return value * CubicFeetPerCubicYard;
                case VolumeUnitEnum.CubicMeter:
                    return value * CubicFeetPerCubicMeter;
                case VolumeUnitEnum.Gallon:
                    return value * CubicFeetPerUsGallon;
                case VolumeUnitEnum.GallonsInMillions:
                    return value * CubicFeetPerUsGallon * Million;
                default:
                    throw new NotSupportedException($"No conversion exists for volume type {volumeUnitEnum} to cubic feet");
            }
        }
    }
}