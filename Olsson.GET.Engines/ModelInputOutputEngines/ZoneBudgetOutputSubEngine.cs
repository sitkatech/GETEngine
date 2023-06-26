using Microsoft.Extensions.Logging;
using Olsson.GET.Accessors.EntityFramework;
using Olsson.GET.Accessors.FileIO;
using Olsson.GET.Common.DataContracts.Runs;
using Olsson.GET.Common.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using Model = Olsson.GET.Common.DataContracts.Models.Model;
using RunStatus = Olsson.GET.Accessors.EntityFramework.RunStatus;

namespace Olsson.GET.Engines.ModelInputOutputEngines
{
    internal interface IZoneBudgetOutputSubEngine
    {
        List<RelatedResultDetails> CreateZoneBudgetOutputResults(IModelFileAccessor modflowFileAccessor, List<StressPeriod> stressPeriods, int outputVolumeUnitID, bool isDifferential);
    }
    internal class ZoneBudgetOutputSubEngine : IZoneBudgetOutputSubEngine
    {

        private static readonly ILogger Logger = Logging.GetLogger<ZoneBudgetOutputSubEngine>();

        public ZoneBudgetOutputSubEngine(Model model)
        {
            Model = model;
        }

        private Model Model { get; }
        public List<RelatedResultDetails> CreateZoneBudgetOutputResults(IModelFileAccessor modflowFileAccessor, List<StressPeriod> stressPeriods, int outputVolumeUnitID, bool isDifferential)
        {
            var outputVolumeUnitEnum = VolumeUnit.AllLookupDictionary[outputVolumeUnitID].ToEnum;
            var asrData = modflowFileAccessor.GetZoneBudgetAsrDataNameMap();
            if (asrData == null || !asrData.Any())
            {
                Logger.LogWarning("Not generating zone budget because no asr data found.");
                return new List<RelatedResultDetails>();
            }
            var asrDict = asrData.ToDictionary(a => a.Key, a => a.Name);

            var runData = modflowFileAccessor.GetRunZoneBudgetItems(asrData);
            if (runData == null)
            {
                Logger.LogWarning("Not generating zone budget because no run file found.");
                return new List<RelatedResultDetails>();
            }

            var byZoneDictionary = new Dictionary<string, Dictionary<string, Dictionary<DateTime, double>>>(); //dict[Zone][BudgetItem][Date]
            var byBudgetItemDictionary = new Dictionary<string, Dictionary<string, Dictionary<DateTime, double>>>(); //byBudgetItemDictionary[BudgetItem][Zone][Date]

            if (isDifferential)
            {
                var baselineData = modflowFileAccessor.GetBaselineZoneBudgetItems(asrData);
                if (baselineData == null)
                {
                    Logger.LogWarning("Not generating zone budget because no baseline file found.");
                    return new List<RelatedResultDetails>();
                }

                Logger.LogInformation("Run is differential -- comparing to baseline.");
                SummarizeDifferentialZoneBudgetData(modflowFileAccessor, stressPeriods, runData, baselineData, asrDict, byZoneDictionary, byBudgetItemDictionary, outputVolumeUnitEnum);
            }
            else
            {
                Logger.LogInformation("Run is non-differential -- ignoring baseline data even if present.");
                SummarizeNonDifferentialZoneBudgetData(modflowFileAccessor, stressPeriods, runData, asrDict, byZoneDictionary, byBudgetItemDictionary, outputVolumeUnitEnum);
            }

            AddObservedData(modflowFileAccessor, stressPeriods, byZoneDictionary, byBudgetItemDictionary, outputVolumeUnitEnum, isDifferential);

            var byZoneResults = BuildItemResults(byZoneDictionary, outputVolumeUnitEnum);
            var byBudgetItemResults = BuildItemResults(byBudgetItemDictionary, outputVolumeUnitEnum);

            AddCumulativeResults(byZoneResults, byBudgetItemResults, outputVolumeUnitEnum);

            return new List<RelatedResultDetails>
            {
                new RelatedResultDetails
                {
                    SetName = "Water Budget By Zone",
                    RelatedResults = byZoneResults
                },
                new RelatedResultDetails
                {
                    SetName = "Water Budget By Budget Item",
                    RelatedResults = byBudgetItemResults
                }
            };
        }

        private void AddObservedData(IModelFileAccessor modflowFileAccessor, List<StressPeriod> stressPeriods, Dictionary<string, Dictionary<string, Dictionary<DateTime, double>>> byZoneDictionary, Dictionary<string, Dictionary<string, Dictionary<DateTime, double>>> byBudgetItemDictionary, VolumeUnitEnum outputVolumeUnitEnum, bool isDifferential)
        {
            var observedData = modflowFileAccessor.GetObservedZoneBudget(isDifferential);
            if (observedData == null)
            {
                Logger.LogDebug("Observed data is not present -- skipping adding it to chart.");
                return;
            }

            if (observedData.Any(x => x.Period > Model.NumberOfStressPeriods))
            {
                Logger.LogDebug($"Observed data has a period outside the model duration.");
                throw new OutputDataInvalidException("Too many stress periods in observed data.", RunStatus.InvalidOutput.RunStatusID);
            }

            foreach (var observedDataItem in observedData)
            {

                var date = Model.StartDateTime.AddMonths(observedDataItem.Period - 1);
                var stressPeriod = stressPeriods[observedDataItem.Period - 1];
                var volume = UnitConversion.ConvertVolume(observedDataItem.ValueInAcreFeet, VolumeUnitEnum.AcreFeet, outputVolumeUnitEnum);
                var timeStepVolume = UnitConversion.CalculateVolumePerTimeStep(volume, stressPeriod);
                AddValue(byZoneDictionary, observedDataItem.ZoneSeriesName, observedDataItem.BudgetItemSeriesName, date, timeStepVolume);
                AddValue(byBudgetItemDictionary, observedDataItem.BudgetItemSeriesName, observedDataItem.ZoneSeriesName, date, timeStepVolume);
            }
        }

        private static void AddCumulativeResults(List<RunResultDetails> byZoneResults, List<RunResultDetails> byBudgetItemResults, VolumeUnitEnum outputVolumeUnitEnum)
        {
            foreach (var result in byZoneResults.Concat(byBudgetItemResults))
            {
                result.ResultSets.Add(ModflowModelInputOutputEngine.CalculateCumulativeFromMonthly(result, outputVolumeUnitEnum));
            }
        }

        private static List<RunResultDetails> BuildItemResults(Dictionary<string, Dictionary<string, Dictionary<DateTime, double>>> byZoneDictionary, VolumeUnitEnum outputVolumeUnitEnum)
        {
            return byZoneDictionary.OrderBy(a => a.Key).Select(a => new RunResultDetails
            {
                RunResultName = a.Key,
                ResultSets = new List<RunResultSet>
                {
                    new RunResultSet
                    {
                        DataType = VolumeUnit.ToType(outputVolumeUnitEnum).VolumeUnitDisplayName,
                        DisplayType = RunResultDisplayType.LineChart,
                        Name = "Rate",
                        DataSeries = a.Value.OrderBy(b=>b.Key).Select(b => new DataSeries
                        {
                            Name = b.Key,
                            IsDefaultDisplayed = b.Value.Any(c => c.Value.IsNotEqual(0)),
                            DataPoints = b.Value.OrderBy(c=>c.Key).Select(c => new RunResultSetDataPoint
                            {
                                Date = c.Key,
                                Value = c.Value
                            }).ToList(),
                            IsObserved = false
                        }).ToList()
                    }
                }
            }).ToList();
        }

        private void SummarizeDifferentialZoneBudgetData(IModelFileAccessor modflowFileAccessor, List<StressPeriod> stressPeriods, IEnumerable<ZoneBudgetItem> runData, IEnumerable<ZoneBudgetItem> baselineData, Dictionary<string, string> asrDict, Dictionary<string, Dictionary<string, Dictionary<DateTime, double>>> byZoneDictionary, Dictionary<string, Dictionary<string, Dictionary<DateTime, double>>> byBudgetItemDictionary, VolumeUnitEnum outputVolumeUnitEnum)
        {
            using (var runEnum = runData.GetEnumerator())
            {
                foreach (var baselineItem in baselineData)
                {
                    if (!runEnum.MoveNext() || runEnum.Current == null)
                    {
                        throw new Exception("Mismatched number of baseline and run rows.");
                    }
                    var runItem = runEnum.Current;
                    if (runItem.Period != baselineItem.Period || runItem.Step != baselineItem.Step || runItem.Zone != baselineItem.Zone)
                    {
                        throw new Exception($"Baseline and run records don't match [{baselineItem.Period}-{baselineItem.Step}-{baselineItem.Zone}] [{runItem.Period}-{runItem.Step}-{runItem.Zone}]");
                    }

                    var date = Model.ModelStressPeriodCustomStartDates != null && Model.ModelStressPeriodCustomStartDates.Any() ? Model.ModelStressPeriodCustomStartDates[baselineItem.Period - 1].StressPeriodStartDate : Model.StartDateTime;

                    if (date == Model.StartDateTime)
                    {
                        date = Model.StartDateTime.AddMonths(baselineItem.Period - 1);
                    }
                    
                    var stressPeriod = stressPeriods[baselineItem.Period - 1];
                    var zoneValue = GetFriendlyZoneBudgetName(modflowFileAccessor, baselineItem.Zone);

                    Logger.LogDebug($"@ [{baselineItem.Period}-{baselineItem.Step}-{baselineItem.Zone}] - Processing baseline budget items [{string.Join(",", baselineItem.Values.Select(a => a.Key))}]  Found run budget data items [{string.Join(",", runItem.Values.Select(a => a.Key))}]");

                    var baselineBudgetData = GetValues(baselineItem, asrDict).GroupBy(a => a.Key).Where(a => a.Count() == 2)
                        .Select(a => new { BudgetItemName = a.Key, Value = a.First().Value - a.Last().Value }).ToList();
                    var runBudgetData = GetValues(runItem, asrDict).GroupBy(a => a.Key).Where(a => a.Count() == 2)
                        .Select(a => new { BudgetItemName = a.Key, Value = a.First().Value - a.Last().Value }).ToList();

                    Logger.LogDebug($"@ [{baselineItem.Period}-{baselineItem.Step}-{baselineItem.Zone}] - Found baseline budget data items [{string.Join(",", baselineBudgetData.Select(a=>a.BudgetItemName))}]  Found run budget data items [{string.Join(",", runBudgetData.Select(a => a.BudgetItemName))}]");

                    foreach (var baselineBudgetDataItem in baselineBudgetData)
                    {
                        var runBudgetDataItem = runBudgetData.FirstOrDefault(a => baselineBudgetDataItem.BudgetItemName == a.BudgetItemName);
                        if(runBudgetDataItem == null)
                        {
                            throw new Exception($"Unable to find budget item [{baselineBudgetDataItem.BudgetItemName}] @ [{baselineItem.Period}-{baselineItem.Step}-{baselineItem.Zone}] in run results [{string.Join(",", runBudgetData.Select(a=>a.BudgetItemName))}].");
                        }
                        var volume = UnitConversion.ConvertVolume(baselineBudgetDataItem.Value - runBudgetDataItem.Value, VolumeUnitEnum.CubicFeet, outputVolumeUnitEnum);
                        var timeStepVolume = UnitConversion.CalculateVolumePerTimeStep(volume, stressPeriod);
                        AddValue(byZoneDictionary, zoneValue, baselineBudgetDataItem.BudgetItemName, date, timeStepVolume);
                        AddValue(byBudgetItemDictionary, baselineBudgetDataItem.BudgetItemName, zoneValue, date, timeStepVolume);
                    }
                }
            }
        }

        private void SummarizeNonDifferentialZoneBudgetData(IModelFileAccessor modflowFileAccessor, List<StressPeriod> stressPeriods, IEnumerable<ZoneBudgetItem> runData, Dictionary<string, string> asrDict, Dictionary<string, Dictionary<string, Dictionary<DateTime, double>>> byZoneDictionary, Dictionary<string, Dictionary<string, Dictionary<DateTime, double>>> byBudgetItemDictionary, VolumeUnitEnum outputVolumeUnitEnum)
        {
            foreach (var runDataItem in runData)
            {
                var date = Model.ModelStressPeriodCustomStartDates != null && Model.ModelStressPeriodCustomStartDates.Any() ? Model.ModelStressPeriodCustomStartDates[runDataItem.Period - 1].StressPeriodStartDate : Model.StartDateTime;

                if (date == Model.StartDateTime)
                {
                    date = Model.StartDateTime.AddMonths(runDataItem.Period - 1);
                }
                var stressPeriod = stressPeriods[runDataItem.Period - 1];
                var zoneValue = GetFriendlyZoneBudgetName(modflowFileAccessor, runDataItem.Zone);

                var runBudgetData = GetValues(runDataItem, asrDict).GroupBy(a => a.Key).Where(a => a.Count() == 2)
                    .Select(a => new { BudgetItemName = a.Key, Value = a.First().Value - a.Last().Value }).ToList();
                foreach (var runBudgetDataItem in runBudgetData)
                {
                    var volume = UnitConversion.ConvertVolume(runBudgetDataItem.Value, VolumeUnitEnum.CubicFeet, outputVolumeUnitEnum);
                    var timeStepVolume = UnitConversion.CalculateVolumePerTimeStep(volume, stressPeriod);
                    AddValue(byZoneDictionary, zoneValue, runBudgetDataItem.BudgetItemName, date, timeStepVolume);
                    AddValue(byBudgetItemDictionary, runBudgetDataItem.BudgetItemName, zoneValue, date, timeStepVolume);
                }
            }
        }

        private static IEnumerable<ZoneBudgetValue> GetValues(ZoneBudgetItem budgetItem, Dictionary<string, string> asrDataMap)
        {
            foreach (var value in budgetItem.Values)
            {
                if (asrDataMap.ContainsKey(value.Key))
                {
                    yield return new ZoneBudgetValue
                    {
                        Key = asrDataMap[value.Key],
                        Value = value.Value
                    };
                }
            }
        }

        private static string GetFriendlyZoneBudgetName(IModelFileAccessor modflowFileAccessor, string zoneKey)
        {
            var friendlyName = modflowFileAccessor.GetFriendlyZoneBudgetName(zoneKey);
            if (!string.IsNullOrWhiteSpace(friendlyName))
            {
                return friendlyName;
            }
            return $"Zone {zoneKey}";
        }

        private static void AddValue(Dictionary<string, Dictionary<string, Dictionary<DateTime, double>>> dict, string key1, string key2, DateTime key3, double value)
        {
            if (!dict.ContainsKey(key1))
            {
                dict[key1] = new Dictionary<string, Dictionary<DateTime, double>>();
            }
            if (!dict[key1].ContainsKey(key2))
            {
                dict[key1][key2] = new Dictionary<DateTime, double>();
            }
            if (!dict[key1].ContainsKey(key2))
            {
                dict[key1][key2] = new Dictionary<DateTime, double>();
            }
            if (!dict[key1][key2].ContainsKey(key3))
            {
                dict[key1][key2][key3] = 0;
            }
            dict[key1][key2][key3] += value;
        }

        private class DateValue
        {
            public DateTime Date { get; set; }
            public double Value { get; set; }
        }
    }
}
