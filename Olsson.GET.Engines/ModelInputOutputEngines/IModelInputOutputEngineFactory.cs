using Olsson.GET.Common.DataContracts.Runs;

namespace Olsson.GET.Engines.ModelInputOutputEngines
{
    public interface IModelInputOutputEngineFactory
    {
        IModelInputOutputEngine CreateModelInputOutputEngine(Run run);
    }
}
