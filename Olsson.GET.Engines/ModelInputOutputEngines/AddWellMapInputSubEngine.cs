using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Olsson.GET.Accessors.FileIO;
using Olsson.GET.Common.DataContracts.Runs;
using Olsson.GET.Common.Shared;
using Olsson.GET.Common.Utilities;
using Model = Olsson.GET.Common.DataContracts.Models.Model;
using Run = Olsson.GET.Common.DataContracts.Runs.Run;

namespace Olsson.GET.Engines.ModelInputOutputEngines
{
    internal interface IAddWellMapInputSubEngine
    {
        Task<StressPeriodsLocationRates> UpdateFlowInputs(IModelFileAccessor modflowFileAccessor, IBlobFileAccessor fileAccessor, StressPeriodsLocationRates existingFlows, Run run);
    }

    public class AddWellMapInputSubEngine : IAddWellMapInputSubEngine
    {
        public AddWellMapInputSubEngine(Model model)
        {
            Model = model;
        }
        private Model Model { get; }

        private async Task<List<RunWellInput>> GetInputFileData(IBlobFileAccessor fileAccessor, Run run)
        {
            var bytes = await fileAccessor.GetFile(StorageLocations.ParsedWellInputFilePathForRun(run.FileStorageLocator), ConfigurationHelper.AppSettings.BlobStorageModelDataFolder);
            return JsonConvert.DeserializeObject<List<RunWellInput>>(Encoding.UTF8.GetString(bytes));
        }

        public async Task<StressPeriodsLocationRates> UpdateFlowInputs(IModelFileAccessor modflowFileAccessor, IBlobFileAccessor fileAccessor, StressPeriodsLocationRates existingFlows, Run run)
        {
            var mapPointsInputs = await GetInputFileData(fileAccessor, run);
            var groupedData = mapPointsInputs.SelectMany(a => a.Values.Where(b => b.Value.IsNotEqual(0)).Select(b => new { Date = (a.Year, a.Month), Value = b })).GroupBy(a => (a.Value.Lat, a.Value.Lng));
            foreach (var latLngData in groupedData)
            {
                var wellLocations = modflowFileAccessor.FindWellLocations(latLngData.Key.Item1, latLngData.Key.Item2);
                foreach (var latLngValue in latLngData)
                {
                    var val = latLngValue.Value.Value;
                    if (!run.Scenario.ShouldSwitchSign) //this seems backwards but add a well should go in as a negative.  Thus is we shouldn't switch sign we want to multiply by -1, otherwise leave it.
                    {
                        val *= -1.0;
                    }
                    var daysInMonth = DateTime.DaysInMonth(latLngValue.Date.Item1, latLngValue.Date.Item2);
                    var stressPeriod = Utilities.GetStressPeriod(latLngValue.Date.Item1, latLngValue.Date.Item2, Model, existingFlows.StressPeriods);
                    var modelExpectedVolumeUnitID = modflowFileAccessor.Model.OutputVolumeUnitID;
                    var flowInExpectedVolumeUnitPerDay = UnitConversion.ConvertFlow(val, run.InputVolumeUnitID, modelExpectedVolumeUnitID, daysInMonth);

                    foreach (var locationPumpingProportion in wellLocations)
                    {
                        stressPeriod.LocationRates.Insert(0, new LocationRate { Location = locationPumpingProportion.Location, Rate = flowInExpectedVolumeUnitPerDay * locationPumpingProportion.Proportion });
                    }
                }
            }

            return existingFlows;
        }
    }
}