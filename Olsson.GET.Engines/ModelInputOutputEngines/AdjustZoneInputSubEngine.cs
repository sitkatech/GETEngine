using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Olsson.GET.Accessors.FileIO;
using Olsson.GET.Common.DataContracts.Models;
using Olsson.GET.Common.DataContracts.Runs;
using Olsson.GET.Common.Shared;
using Olsson.GET.Common.Utilities;

namespace Olsson.GET.Engines.ModelInputOutputEngines
{
    internal interface IAdjustZoneInputSubEngine
    {
        StressPeriodsLocationRates UpdateFlowInputs(IModelFileAccessor modflowFileAccessor, IBlobFileAccessor fileAccessor, StressPeriodsLocationRates existingFlows, Run run);
    }
    internal class AdjustZoneInputSubEngine : IAdjustZoneInputSubEngine
    {
        public AdjustZoneInputSubEngine(Model model)
        {
            Model = model;
        }
        private Model Model { get; }

        private RunZoneInput[] GetInputFileData(IBlobFileAccessor fileAccessor, Run run)
        {
            return JsonConvert.DeserializeObject<RunZoneInput[]>(Encoding.UTF8.GetString(fileAccessor.GetFile(StorageLocations.ParsedZoneInputFilePathForRun(run.FileStorageLocator), ConfigurationHelper.AppSettings.BlobStorageModelDataFolder)));
        }

        public StressPeriodsLocationRates UpdateFlowInputs(IModelFileAccessor modflowFileAccessor, IBlobFileAccessor fileAccessor, StressPeriodsLocationRates existingFlows, Run run)
        {
            var mapPointsInputs = GetInputFileData(fileAccessor, run);

            foreach(var item in existingFlows.StressPeriods.SelectMany(a => a.LocationRates))
            {
                var adjustment = modflowFileAccessor.GetInputLocationZones(item.Location).Select(a=>mapPointsInputs.FirstOrDefault(b=>b.ZoneNumber==a)).Where(a=>a!=null).Aggregate(1.0, (a, b)=>a*b.Adjustment);
                if (run.Scenario.ShouldSwitchSign)
                {
                    adjustment = 2 - adjustment;
                }
                item.Rate *= adjustment;
            }

            return existingFlows;
        }
    }
}