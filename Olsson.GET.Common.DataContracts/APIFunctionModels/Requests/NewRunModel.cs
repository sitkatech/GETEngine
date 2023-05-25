using Olsson.GET.Common.DataContracts.Runs;
using System.Collections.Generic;

namespace Olsson.GET.Common.DataContracts.APIFunctionModels
{
    public class NewRunModel
    {
        public string Name { get; set; }

        public int? CustomerId { get; set; }

        public int? UserId { get; set; }

        public int? ModelId { get; set; }

        public int? ScenarioId { get; set; }

        public bool CreateMaps { get; set; }

        public bool? IsDifferential { get; set; }

        public string Description { get; set; }

        public int InputVolumeType { get; set; }

        public int OutputVolumeType { get; set; }

        public List<RunCanalInput> RunCanalInputs { get; set; }

        public List<PivotedRunWellInput> PivotedRunWellInputs { get; set; }

        public List<RunZoneInput> RunZoneInputs { get; set; }

        public List<RunWellParticleInput> RunWellParticleInputs { get; set; }
    }

    public class InputFile
    {
        public string FileName { get; set; }

        public byte[] FileContent { get; set; }
    }
}
