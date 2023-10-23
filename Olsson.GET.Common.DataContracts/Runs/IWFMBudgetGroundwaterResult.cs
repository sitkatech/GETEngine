using System.Collections.Generic;

namespace Olsson.GET.Common.DataContracts.Runs
{
    public class IWFMBudgetGroundwaterResult
    {
        public double StorageArea { get; set; }
        public List<IWFMBudgetGroundwaterPeriod> Periods { get; set; }
        public List<IWFMBudgetGroundwaterPeriod> BaselinePeriods { get; set; }
    }
}