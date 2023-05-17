using System.Collections.Generic;

namespace Olsson.GET.Common.DataContracts.Runs
{
    public class LocationRate
    {
        public string Location { get; set; }
        public double Rate { get; set; }
    }

    public class StressPeriodsLocationRates
    {
        public List<StressPeriodLocationRates> StressPeriods { get; set; }
        public string HeaderValue { get; set; }
        public List<string> Parameters { get; set; }
    }

    public class StressPeriodLocationRates
    {
        public List<LocationRate> LocationRates { get; set; }
        public int Flag { get; set; }
        public List<LocationRate> ClnLocationRates { get; set; }
    }
}