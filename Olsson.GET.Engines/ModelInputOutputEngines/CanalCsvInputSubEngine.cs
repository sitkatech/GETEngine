using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Newtonsoft.Json;
using Olsson.GET.Accessors.EntityFramework;
using Olsson.GET.Accessors.FileIO;
using Olsson.GET.Common.DataContracts.Runs;
using Olsson.GET.Common.Shared;
using Olsson.GET.Common.Utilities;
using Model = Olsson.GET.Common.DataContracts.Models.Model;
using Run = Olsson.GET.Common.DataContracts.Runs.Run;
[assembly: InternalsVisibleTo("Olsson.GET.Tests.EngineTests.ModelInputOutputEngines")]

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
            return JsonConvert.DeserializeObject<List<RunCanalInput>>(Encoding.UTF8.GetString(fileAccessor.GetFile(StorageLocations.ParsedInputFilePathForRun(run.FileStorageLocator), ConfigurationHelper.AppSettings.BlobStorageModelDataFolder).Result));
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

                        var newVolumeUnitEnum = VolumeUnit.AllLookupDictionary[modflowFileAccessor.Model.ExpectedOutputVolumeUnitID].ToEnum;
                        var expectedVolumeUnitPerDayValue = UnitConversion.ConvertVolume(val, VolumeUnit.AllLookupDictionary[run.InputVolumeUnitID].ToEnum, newVolumeUnitEnum) / daysInMonth;
                        foreach (var proportion in modflowFileAccessor.GetLocationProportions(featureValue.FeatureName))
                        {
                            var ratesToUpdate = proportion.IsClnWell ? stressPeriod.ClnLocationRates : stressPeriod.LocationRates;
                            ratesToUpdate.Insert(0, new LocationRate { Location = proportion.Location, Rate = proportion.Proportion * expectedVolumeUnitPerDayValue });
                        }
                    }
                }
            }
            return existingFlows;
        }
    }
}
