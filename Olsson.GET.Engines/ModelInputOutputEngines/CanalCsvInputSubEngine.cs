using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Olsson.GET.Accessors.EntityFramework;
using Olsson.GET.Accessors.FileIO;
using Olsson.GET.Common.DataContracts.Runs;
using Olsson.GET.Common.Shared;
using Olsson.GET.Common.Utilities;
using Olsson.GET.Common.Shared.Enums;
using Model = Olsson.GET.Common.DataContracts.Models.Model;
using Run = Olsson.GET.Common.DataContracts.Runs.Run;

namespace Olsson.GET.Engines.ModelInputOutputEngines
{
    internal interface ICanalCsvInputSubEngine
    {
        StressPeriodsLocationRates UpdateFlowInputs(IModelFileAccessor modflowFileAccessor, IBlobFileAccessor fileAccessor, StressPeriodsLocationRates existingFlows, Run run);
    }
    internal class CanalCsvInputSubEngine : ICanalCsvInputSubEngine
    {
        public CanalCsvInputSubEngine(Model model)
        {
            Model = model;
        }
        private Model Model { get; }

        private List<RunCanalInput> GetInputFileData(IBlobFileAccessor fileAccessor, Run run)
        {
            return JsonConvert.DeserializeObject<List<RunCanalInput>>(Encoding.UTF8.GetString(fileAccessor.GetFile(StorageLocations.ParsedInputFilePathForRun(run.FileStorageLocator), ConfigurationHelper.AppSettings.BlobStorageModelDataFolder)));
        }

        public StressPeriodsLocationRates UpdateFlowInputs(IModelFileAccessor modflowFileAccessor, IBlobFileAccessor fileAccessor, StressPeriodsLocationRates existingFlows, Run run)
        {

            var inputFlowStressPeriods = GetInputFileData(fileAccessor, run);
            foreach (var inputFlows in inputFlowStressPeriods)
            {
                var nonZeroFeatureValues = inputFlows.Values.Where(a => a.Value.IsNotEqual(0)).ToList();
                if (nonZeroFeatureValues.Any())
                {
                    var stressPeriod = Utilities.GetStressPeriod(inputFlows.Year, inputFlows.Month, Model, existingFlows.StressPeriods);

                    var daysInMonth = DateTime.DaysInMonth(inputFlows.Year, inputFlows.Month);

                    foreach (var featureValue in nonZeroFeatureValues)
                    {
                        var val = featureValue.Value;
                        if (run.Scenario.ShouldSwitchSign)
                        {
                            val *= -1.0;
                        }
                        var cubicFeetPerDayValue = UnitConversion.ConvertVolume(val, VolumeUnit.AllLookupDictionary[run.InputVolumeUnitID].ToEnum, VolumeUnitEnum.CubicFeet) / daysInMonth;
                        foreach (var proportion in modflowFileAccessor.GetLocationProportions(featureValue.FeatureName))
                        {
                            var ratesToUpdate = proportion.IsClnWell ? stressPeriod.ClnLocationRates : stressPeriod.LocationRates;
                            ratesToUpdate.Insert(0, new LocationRate { Location = proportion.Location, Rate = proportion.Proportion * cubicFeetPerDayValue });
                        }
                    }
                }
            }
            return existingFlows;
        }
    }
}
