namespace Olsson.GET.Common.DataContracts.Runs
{
    public class ObservedImpactToBaseflow
    {
        public string DataSeriesName { get; set; }
        public int Period { get; set; }
        public double FlowToAquiferInAcreFeet { get; set; }
    }

    public class ObservedZoneBudgetData
    {
        public string ZoneSeriesName { get; set; }
        public string BudgetItemSeriesName { get; set; }
        public int Period { get; set; }
        public double ValueInAcreFeet { get; set; }
    }

    public class ObservedPointOfInterest
    {
        public string LocationSeriesName { get; set; }
        public int Period { get; set; }
        public double ValueInCubicFeet { get; set; }
    }
}
