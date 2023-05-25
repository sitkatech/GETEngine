using Olsson.GET.Common.DataContracts.Models;
using Olsson.GET.Common.DataContracts.Users;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Olsson.GET.Common.DataContracts.Scenarios;

namespace Olsson.GET.Common.DataContracts.Runs
{
    [DataContract]
    public class Run
    {
        [DataMember]
        public int RunID { get; set; }

        [DataMember]
        public string RunName { get; set; }

        [DataMember]
        public string FileStorageLocator { get; set; }

        [DataMember]
        public int? ImageID { get; set; }

        [DataMember]
        public int ModelID { get; set; }

        [DataMember]
        public int ScenarioID { get; set; }

        [DataMember]
        public int UserID { get; set; }

        [DataMember]
        public int CustomerID { get; set; }

        [DataMember]
        public string InputFileName { get; set; }

        [DataMember]
        public int RunStatusID { get; set; }

        [DataMember]
        public bool IsDeleted { get; set; }

        [DataMember]
        public DateTime CreatedDate { get; set; }

        [DataMember]
        public DateTime? ProcessingStartDate { get; set; }

        [DataMember]
        public DateTime? ProcessingEndDate { get; set; }

        [DataMember]
        public bool ShouldCreateMaps { get; set; }

        [DataMember]
        public int RestartCount { get; set; }

        [DataMember]
        public string Output { get; set; }

        [DataMember]
        public Model Model { get; set; }

        [DataMember]
        public Scenario Scenario { get; set; }

        [DataMember]
        public User User { get; set; }

        [DataMember]
        public Image Image { get; set; }

        [DataMember]
        public RunStatus RunStatus { get; set; }

        [DataMember]
        public int InputVolumeUnitID { get; set; }

        [DataMember]
        public int OutputVolumeUnitID { get; set; }

        [DataMember]
        public bool IsDifferential { get; set; }

        [DataMember]
        public string RunDescription { get; set; }

        public List<RunResultListItem> AvailableRunResults { get; set; }

        public List<RunCanalInput> CanalRunInputs { get; set; }

        public List<RunWellInput> WellMapInputs { get; set; }

        public List<PivotedRunWellInput> PivotedWellMapInputs { get; set; }

        public List<RunZoneInput> RunZoneInputs { get; set; }

        public List<RunWellParticleInput> RunWellParticleInputs { get; set; }

        public List<RunBucket> RunBuckets { get; set; }
    }

    [DataContract]
    public class RunResultListItem
    {
        [DataMember]
        public int RunResultId { get; set; }

        [DataMember]
        public string RunResultName { get; set; }

        [DataMember]
        public string RunResultFileExtension { get; set; }
    }
}
