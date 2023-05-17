using System;

namespace Olsson.GET.Common.DataContracts.Models
{
    public class ModelStressPeriodCustomStartDate
    {
        public int ModelStressPeriodCustomStartDateID { get; set; }
        public int ModelID { get; set; }
        public int StressPeriod { get; set; }
        public DateTime StressPeriodStartDate { get; set; }
    }
}