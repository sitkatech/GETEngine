using System;

namespace Olsson.GET.Common.DataContracts.Runs
{
    public class IWFMBudgetGroundwaterPeriod
    {
        public DateTime Time { get; set; }
        public double Percolation { get; set; }
        public double BeginningStorage { get; set; }
        public double EndingStorage { get; set; }
        public double DeepPercolation { get; set; }
        public double GainFromStream { get; set; }
        public double GainFromLake { get; set; }
        public double BoundaryInflow { get; set; }
        public double Pumping { get; set; }
        public double OutflowToRootZone { get; set; }
    }
}