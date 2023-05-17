using System;
using Olsson.GET.Accessors.EntityFramework;
using Run = Olsson.GET.Common.DataContracts.Runs.Run;

namespace Olsson.GET.Engines.ModelInputOutputEngines
{
    public class ModelInputOutputEngineFactory : IModelInputOutputEngineFactory
    {
        public IModelInputOutputEngine CreateModelInputOutputEngine(Run run)
        {
            switch ((ModelEngineTypeEnum)run.Model.ModelEngineTypeID)
            {
                case ModelEngineTypeEnum.Modpath:
                    return new ModpathModelInputOutputEngine();
                case ModelEngineTypeEnum.Modflow:
                case ModelEngineTypeEnum.Modflow6:
                    return new ModflowModelInputOutputEngine(run.Model);
                case ModelEngineTypeEnum.IWFM:
                    return new IWFMModelInputOutputEngine(run.Model);
                default:
                    throw new ArgumentOutOfRangeException($"Unknown Model Engine Type {run.Model.ModelEngineTypeID}");
            }
        }
    }
}