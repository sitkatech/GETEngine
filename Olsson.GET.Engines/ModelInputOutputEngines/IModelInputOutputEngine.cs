using Olsson.GET.Common.DataContracts.Runs;

namespace Olsson.GET.Engines.ModelInputOutputEngines
{
    public interface IModelInputOutputEngine
    {
        void GenerateInputFiles(Run run);
        void GenerateOutputFiles(Run run);
    }
}