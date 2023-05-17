using System.Collections.Generic;

namespace Olsson.GET.Common.DataContracts.Runs
{
    public class RunFilter
    {
        public RunFilter()
        {
            RunStatusIDs = new List<int>();
        }

        public string NameSearch { get; set; }

        public List<int> RunStatusIDs { get; set; }

        public int? UserID { get; set; }

        public int? ModelID { get; set; }

        public int? ScenarioID { get; set; }
    }
}
