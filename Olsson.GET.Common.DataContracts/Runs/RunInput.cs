using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Olsson.GET.Common.DataContracts.Runs
{
    public class RunCanalInput
    {
        public int Month { get; set; }

        public int Year { get; set; }

        public List<FeatureValue> Values { get; set; }
    }

    public class RunWellInput
    {
        public int Month { get; set; }

        public int Year { get; set; }

        public bool ManuallyAdded { get; set; }

        public List<FeatureWithLocationValue> Values { get; set; }
    }

    public class RunZoneInput
    {
        public string ZoneNumber { get; set; }

        public string ZoneName { get; set; }

        public double Adjustment { get; set; }
    }

    public class RunWellParticleInput
    {
        public string Name { get; set; }

        public double Lat { get; set; }

        public double Lng { get; set; }

        public int ParticleCount { get; set; }

        public bool ManuallyAdded { get; set; }
    }

    public class RunCanalInputParseResult
    {
        public bool Success { get; set; }

        public List<RunCanalInput> RunInputs { get; set; }

        public List<string> Errors { get; set; }
    }

    public class RunWellInputParseResult
    {
        public bool Success { get; set; }

        public List<RunWellInput> RunInputs { get; set; }

        public List<string> Errors { get; set; }
    }

    public class RunWellParticleInputParseResult
    {
        public bool Success { get; set; }

        public List<RunWellParticleInput> RunInputs { get; set; }

        public List<string> Errors { get; set; }
    }

    public class FeatureValue
    {
        public string FeatureName { get; set; }

        public double Value { get; set; }
    }

    public class FeatureWithLocationValue : FeatureValue
    {
        public double Lat { get; set; }

        public double Lng { get; set; }

    }

    public class PivotedRunWellInput
    {
        public string Name { get; set; }

        public double Lat { get; set; }

        public double Lng { get; set; }

        public bool ManuallyAdded { get; set; }

        public double AverageValue { get; set; }

        public List<StressPeriodValue> StressPeriodValues { get; set; }
    }

    public class StressPeriodValue
    {
        public int Month { get; set; }

        public int Year { get; set; }

        public double Value { get; set; }
    }
}
