using Olsson.GET.Accessors.EntityFramework;
using Olsson.GET.Accessors.FileIO;
using Olsson.GET.Common.DataContracts.Runs;
using Olsson.GET.Common.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Serilog;
using Model = Olsson.GET.Common.DataContracts.Models.Model;
using RunStatus = Olsson.GET.Accessors.EntityFramework.RunStatus;

namespace Olsson.GET.Engines.ModelInputOutputEngines
{
    internal interface IListFileOutputSubEngine
    {
        (List<RunResultDetails> OutputResults, OutputDataInvalidException Exception) GenerateListFileOutput(IModelFileAccessor modflowFileAccessor, List<StressPeriod> stressPeriods, int outputVolumeUnitID, bool isDifferential);
    }

    public class ListFileOutputSubEngine : IListFileOutputSubEngine
    {
        public ListFileOutputSubEngine(Model model)
        {
            Model = model;
        }
        private static readonly ILogger Logger = Logging.GetLogger<ListFileOutputSubEngine>();
        private Model Model { get; }
        public (List<RunResultDetails> OutputResults, OutputDataInvalidException Exception) GenerateListFileOutput(IModelFileAccessor modflowFileAccessor, List<StressPeriod> stressPeriods, int outputVolumeUnitID, bool isDifferential)
        {
            var outputVolumeUnitEnum = VolumeUnit.AllLookupDictionary[outputVolumeUnitID].ToEnum;
            Logger.Information("Generating list file output.");
            var outputResults = new List<RunResultDetails>();
            var asrDataMap = modflowFileAccessor.GetAsrDataNameMap();

            var listFileOutputText = ReadListFileOutputText(modflowFileAccessor.GetListFileOutputFileLines());
            OutputDataInvalidException exception = null;

            if (asrDataMap.Any())
            {
                var processRunListFileResults = ProcessListFile(modflowFileAccessor.GetRunListFileLines(), stressPeriods, asrDataMap, outputVolumeUnitEnum);
                if (processRunListFileResults.Exception != null) exception = processRunListFileResults.Exception;

                List<DataSeries> asrOutputData;

                if (isDifferential)
                {
                    var processBaselineListFileResults = ProcessListFile(modflowFileAccessor.GetBaselineListFileLines(), stressPeriods, asrDataMap, outputVolumeUnitEnum);
                    asrOutputData = CalculateAsrOutputDeltas(processBaselineListFileResults.Item1, processRunListFileResults.Item1);
                }
                else
                {
                    asrOutputData = CalculateAsrOutputData(processRunListFileResults.Item1);
                }

                AddObservedData(modflowFileAccessor, asrOutputData, stressPeriods, outputVolumeUnitEnum, isDifferential);

                var asrDataRunResultDetails = new RunResultDetails
                {
                    RunResultName = "Water Budget",
                    ResultSets = new List<RunResultSet>
                    {
                        new RunResultSet
                        {
                            Name = "Rate",
                            DataType = VolumeUnit.ToType(outputVolumeUnitEnum).VolumeUnitDisplayName,
                            DisplayType = RunResultDisplayType.LineChart,
                            DataSeries = asrOutputData
                        }
                    }
                };

                asrDataRunResultDetails.ResultSets.Add(ModflowModelInputOutputEngine.CalculateCumulativeFromMonthly(asrDataRunResultDetails, outputVolumeUnitEnum));

                outputResults.Add(asrDataRunResultDetails);
            }

            outputResults.Add(new RunResultDetails
            {
                RunResultName = "List File Output",
                ResultSets = new List<RunResultSet>
                {
                    new RunResultSet
                    {
                        DisplayType = RunResultDisplayType.Text,
                        Name="List File Output",
                        TextDisplay = new TextDisplay
                        {
                            FileName = "ListFile.txt",
                            Text = listFileOutputText
                        }
                    }
                }
            });

            Logger.Information("List file output results generated.");
            return (outputResults, exception);
        }

        private void AddObservedData(IModelFileAccessor modflowFileAccessor, List<DataSeries> result, List<StressPeriod> stressPeriods, VolumeUnitEnum outputVolumeType, bool isDifferential)
        {
            var observedData = modflowFileAccessor.GetObservedZoneBudget(isDifferential);

            if (observedData == null)
            {
                Logger.Debug("Observed data is not present -- skipping adding it to chart.");
                return;
            }

            if (observedData.Any(x => x.Period > Model.NumberOfStressPeriods))
            {
                Logger.Debug($"Observed data has a period outside the model duration.");
                throw new OutputDataInvalidException("Too many stress periods in observed data.", RunStatus.InvalidOutput.RunStatusID);
            }

            var groupedItems = observedData.GroupBy(x => x.BudgetItemSeriesName);
            foreach (var dataSeries in groupedItems)
            {
                result.Add(new DataSeries
                {
                    Name = dataSeries.Key,
                    IsDefaultDisplayed = dataSeries.Any(a => a.ValueInAcreFeet.IsNotEqual(0)),
                    DataPoints = dataSeries.Select(x =>
                    {
                        var date = Model.StartDateTime.AddMonths(x.Period - 1);
                        var volume = UnitConversion.ConvertVolume(x.ValueInAcreFeet, VolumeUnitEnum.AcreFeet, outputVolumeType);
                        var value = UnitConversion.CalculateVolumePerTimeStep(volume, stressPeriods[x.Period - 1]);

                        return new RunResultSetDataPoint
                        {
                            Date = date,
                            Value = value,
                        };
                    }).OrderBy(a => a.Date).ToList(),
                    IsObserved = true
                });
            }
        }

        private List<DataSeries> CalculateAsrOutputDeltas(List<DataSeries> baselineData, List<DataSeries> runData)
        {
            var result = new List<DataSeries>();
            foreach (var dataSeries in runData.OrderBy(a => a.Name.ToUpper()).Select(a => new { RunItem = a, BaselineItem = baselineData.First(b => b.Name == a.Name) }))
            {
                result.Add(new DataSeries
                {
                    Name = dataSeries.RunItem.Name,
                    IsDefaultDisplayed = dataSeries.RunItem.DataPoints.Any(a => a.Value.IsNotEqual(0)),
                    DataPoints = dataSeries.RunItem.DataPoints.Select(a => new RunResultSetDataPoint
                    {
                        Date = a.Date,
                        Value = dataSeries.BaselineItem.DataPoints.First(b => b.Date == a.Date).Value - a.Value
                    }).OrderBy(a => a.Date).ToList(),
                    IsObserved = false
                });
            }
            return result;
        }

        private List<DataSeries> CalculateAsrOutputData(List<DataSeries> runData)
        {
            var result = new List<DataSeries>();
            foreach (var dataSeries in runData.OrderBy(a => a.Name.ToUpper()))
            {
                result.Add(new DataSeries
                {
                    Name = dataSeries.Name,
                    IsDefaultDisplayed = dataSeries.DataPoints.Any(a => a.Value.IsNotEqual(0)),
                    DataPoints = dataSeries.DataPoints.Select(a => new RunResultSetDataPoint
                    {
                        Date = a.Date,
                        Value = dataSeries.DataPoints.First(b => b.Date == a.Date).Value
                    }).OrderBy(a => a.Date).ToList(),
                    IsObserved = false
                });
            }
            return result;
        }

        private const string PercentDiscrepancyBudgetValueName = "PERCENT DISCREPANCY";
        private const string TotalInputValueName = "TOTAL IN";

        private List<(string, Regex)> CreateBudgetValueRegexs(List<AsrDataMap> asrDataMap)
        {
            return asrDataMap.Select(a => (a.Name, CreateBudgetValueRegex(a.Key)))
                             .Concat(new List<(string, Regex)>
                                     {
                                         (PercentDiscrepancyBudgetValueName, CreateBudgetValueRegex(PercentDiscrepancyBudgetValueName)),
                                         (TotalInputValueName, CreateBudgetValueRegex(TotalInputValueName))
                                     })
                             .ToList();
        }
        private static Regex CreateBudgetValueRegex(string budgetValueName)
        {

            return new Regex($@"^(\s*({budgetValueName}\s*=\s*(?<Value>(\-?\d*\.?\d*((E[\+\-]\d+)?))))){{2}}.*$", RegexOptions.Compiled);
        }

        private Regex CreateHeaderRegex()
        {
            return new Regex($@"^\s*(?<Value>.+) FOR ENTIRE MODEL AT END OF TIME STEP\s*(\d+).*STRESS PERIOD\s*(\d+)$", RegexOptions.Compiled);
        }

        private static (string Name, double CumulativeValue, double TimeStepValue)? GetBudgetValue(string line, List<(string Name, Regex Regex)> budgetValueRegExes)
        {
            foreach (var regEx in budgetValueRegExes)
            {
                var match = regEx.Regex.Match(line);
                if (match.Success)
                {
                    return (regEx.Name, double.Parse(match.Groups["Value"].Captures[0].Value), double.Parse(match.Groups["Value"].Captures[1].Value));
                }
            }
            return null;
        }

        private static string GetHeaderValue(string line, Regex regex)
        {
            var match = regex.Match(line);
            if (match.Success)
            {
                return match.Groups["Value"].Captures[0].Value;
            }

            return null;
        }

        private string ReadListFileOutputText(IEnumerable<string> fileLines)
        {
            var fileText = new StringBuilder();

            foreach (var fileLine in fileLines)
            {
                fileText.AppendLine(fileLine);
            }

            return fileText.ToString();
        }

        private (List<DataSeries>, OutputDataInvalidException Exception) ProcessListFile(IEnumerable<string> fileLines, List<StressPeriod> stressPeriods, List<AsrDataMap> asrDataMap, VolumeUnitEnum outputVolumeType)
        {
            OutputDataInvalidException badOutputDataException = null;
            var percentDiscrepancyLineCount = 0;
            var budgetValueRegExes = CreateBudgetValueRegexs(asrDataMap);
            var asrValues = new List<(string Name, double CumulativeValue, double TimeStepValue)>();
            var tempAsrValues = new List<(string Name, double CumulativeValue, double TimeStepValue)>();

            var headerRegex = CreateHeaderRegex();
            var continueToNextHeader = true;

            foreach (var fileLine in fileLines)
            {
                var headerMatch = GetHeaderValue(fileLine, headerRegex);

                if (headerMatch != null)
                {
                    continueToNextHeader = (headerMatch != "VOLUME BUDGET" && headerMatch != "VOLUMETRIC BUDGET");
                }
                else if (!continueToNextHeader)
                {
                    var budgetValueMatch = GetBudgetValue(fileLine, budgetValueRegExes);
                    if (budgetValueMatch != null)
                    {
                        //We've reached the end of a volume budget chart for a time step
                        //This should have only the 'OUT' values in tempAsrValues, which we'll sum because there could be multiple
                        if (budgetValueMatch.Value.Name == PercentDiscrepancyBudgetValueName)
                        {
                            percentDiscrepancyLineCount++;
                            badOutputDataException = badOutputDataException ??
                                                     ValidatePercentDiscrepancy(budgetValueMatch.Value.CumulativeValue, "Cumulative") ??
                                                     ValidatePercentDiscrepancy(budgetValueMatch.Value.TimeStepValue, "Step");
                            asrValues.AddRange(SumValuesForBudgetValue(tempAsrValues));
                            tempAsrValues.Clear();
                        }
                        //We've reached the end of the 'IN' values for a Volume Budget Chart
                        //This should have only the  'IN' values in tempAsrValues, which we'll sum because there could be multiple
                        else if (budgetValueMatch.Value.Name == TotalInputValueName)
                        {
                            asrValues.AddRange(SumValuesForBudgetValue(tempAsrValues));
                            tempAsrValues.Clear();
                        }
                        else
                        {
                            tempAsrValues.Add(budgetValueMatch.Value);
                        }
                    }

                }
            }
            var expectedPercentDiscrepancyLineCount = stressPeriods.Sum(a => a.NumberOfTimeSteps);
            if (percentDiscrepancyLineCount != expectedPercentDiscrepancyLineCount)
            {
                var message = $"The number of percent discrepancy lines in the list file, {percentDiscrepancyLineCount}, does not match the number of time steps, {expectedPercentDiscrepancyLineCount}.";
                Logger.Warning(message);
                badOutputDataException = badOutputDataException ?? new OutputDataInvalidException(message, RunStatus.InvalidOutput.RunStatusID);
            }

            var series = new List<DataSeries>();
            foreach (var asrType in asrValues.GroupBy(a => a.Name))
            {
                var currSeries = new DataSeries
                {
                    Name = asrType.Key,
                    IsDefaultDisplayed = true,
                    DataPoints = new List<RunResultSetDataPoint>(),
                    IsObserved = false
                };
                //Batching two because values should be stored with an output immediately following an input, then we get their delta
                using (var typeEnumerator = asrType.Batch(2).Select(a => a.ToList()).Select(FindTimeStepDelta).GetEnumerator())
                {
                    
                    for (var i = 0; i < stressPeriods.Count; i ++)
                    {
                        var stressPeriodStartDate = Model.ModelStressPeriodCustomStartDates != null && Model.ModelStressPeriodCustomStartDates.Any() ? Model.ModelStressPeriodCustomStartDates[i].StressPeriodStartDate : Model.StartDateTime.AddMonths(i);
                        var value = 0.0;
                        for (var j = 0; j < stressPeriods[i].NumberOfTimeSteps; j++)
                        {
                            typeEnumerator.MoveNext();
                            var volume = UnitConversion.ConvertVolume(typeEnumerator.Current, VolumeUnitEnum.CubicFeet, outputVolumeType);
                            value += UnitConversion.CalculateVolumePerTimeStep(volume, stressPeriods[i]);
                        }
                        currSeries.DataPoints.Add(new RunResultSetDataPoint
                        {
                            Date = stressPeriodStartDate,
                            Value = value
                        });
                    }
                }
                series.Add(currSeries);
            }

            return (series, badOutputDataException);
        }

        private static List<(string Name, double CumulativeValue, double TimeStepValue)> SumValuesForBudgetValue(
            List<(string Name, double CumulativeValue, double TimeStepValue)> listToSum)
        {
            return listToSum.GroupBy(x => x.Name)
                .Select(x =>
                    {
                        return (x.Key, x.Sum(y => y.CumulativeValue), x.Sum(y => y.TimeStepValue));
                    })
                .ToList();
        }

        private static double FindTimeStepDelta(List<(string Name, double CumulativeValue, double TimeStepValue)> a)
        {
            return a[0].TimeStepValue - a[1].TimeStepValue;
        }

        private OutputDataInvalidException ValidatePercentDiscrepancy(double percentDiscrepancyValue, string discrepancyDescription)
        {
            if (Model.AllowablePercentDiscrepancy == null)
            {
                //null AllowablePercentDiscrepancy means that we do not need to verify the percent discrepancy
                return null;
            }
            if (Math.Abs(percentDiscrepancyValue) > Model.AllowablePercentDiscrepancy)
            {
                var message = $"{discrepancyDescription} percent discrepancy, {percentDiscrepancyValue}, is greater than the allowable amount {Model.AllowablePercentDiscrepancy}.";
                Logger.Warning(message);
                return new OutputDataInvalidException(message, RunStatus.InvalidOutput.RunStatusID);
            }
            return null;
        }
    }
}