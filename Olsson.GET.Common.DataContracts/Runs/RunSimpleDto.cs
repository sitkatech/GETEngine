using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Olsson.GET.Common.DataContracts.Models;
using Olsson.GET.Common.DataContracts.Scenarios;
using Olsson.GET.Common.DataContracts.Users;

namespace Olsson.GET.Common.DataContracts.Runs
{
    public class RunSimpleDto
    {
        public int RunID { get; set; }

        public string RunName { get; set; }

        public string RunDescription { get; set; }

        public User User { get; set; }

        public int CustomerID { get; set; }

        public ModelSimpleDto Model { get; set; }

        public ScenarioSimpleDto Scenario { get; set; }

        public RunStatus RunStatus { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime? ProcessingStartDate { get; set; }

        public DateTime? ProcessingEndDate { get; set; }

        public List<RunBucket> RunBuckets { get; set; }
    }
}