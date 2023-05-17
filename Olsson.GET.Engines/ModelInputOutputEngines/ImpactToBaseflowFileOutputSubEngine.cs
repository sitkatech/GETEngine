using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using log4net;
using Olsson.GET.Accessors.EntityFramework;
using Olsson.GET.Accessors.FileIO;
using Olsson.GET.Common.DataContracts.Runs;
using Olsson.GET.Common.Utilities;
using Model = Olsson.GET.Common.DataContracts.Models.Model;
using RunStatus = Olsson.GET.Accessors.EntityFramework.RunStatus;

namespace Olsson.GET.Engines.ModelInputOutputEngines
{
    internal interface IImpactToBaseflowFileOutputSubEngine
    {
        List<RunResultDetails> CalculateImpactToBaseflow(IModelFileAccessor modflowFileAccessor, List<StressPeriod> stressPeriods, int outputVolumeUnitID, bool isDifferentialRun);
    }

    internal class ImpactToBaseflowFileOutputSubEngine : IImpactToBaseflowFileOutputSubEngine
    {
        public ImpactToBaseflowFileOutputSubEngine(Model model)
        {
            Model = model;
        }
        private static readonly ILog Logger = Logging.GetLogger(typeof(ListFileOutputSubEngine));
        private Model Model { get; }

        public List<RunResultDetails> CalculateImpactToBaseflow(IModelFileAccessor modflowFileAccessor, List<StressPeriod> stressPeriods, int outputVolumeUnitID, bool isDifferentialRun)
        {
            var outputVolumeUnitEnum = VolumeUnit.AllLookupDictionary[outputVolumeUnitID].ToEnum;
            Logger.Info("Calculating impact to baseflow.");

            if (stressPeriods == null || !stressPeriods.Any())
            {
                Logger.Warn("Not generating impact to baseflow because no stress period data found.");
                return new List<RunResultDetails>();
            }

            RunResultDetails result;
            if (isDifferentialRun)
            {
                Logger.Info("Run is differential -- comparing to baseline.");
                result = CalculateDifferentialImpactToBaseflow(modflowFileAccessor, stressPeriods, outputVolumeUnitEnum);
            }
            else
            {
                Logger.Info("Run is non-differential -- ignoring baseline data even if present.");
                result = CalculateNonDifferentialImpactToBaseflow(modflowFileAccessor, stressPeriods, outputVolumeUnitEnum);
            }

            if (result == null) return new List<RunResultDetails>();

            result.ResultSets.Add(ModflowModelInputOutputEngine.CalculateCumulativeFromMonthly(result, outputVolumeUnitEnum));

            AddObservedData(modflowFileAccessor, result, outputVolumeUnitEnum, isDifferentialRun);

            foreach (var dataPoint in result.ResultSets.SelectMany(a => a.DataSeries).SelectMany(c => c.DataPoints))
            {
                dataPoint.Value = Math.Round(dataPoint.Value, 9);
            }

            Logger.Info("Impact to baseflow calculated.");
            return new List<RunResultDetails>() { result };
        }

        private void AddObservedData(IModelFileAccessor modflowFileAccessor, RunResultDetails result, VolumeUnitEnum outputVolumeUnitEnum, bool isDifferential)
        {
            var observedData = modflowFileAccessor.GetObservedImpactToBaseflow(isDifferential);
            if (observedData == null)
            {
                Logger.Debug("Observed data is not present -- skipping adding it to chart.");
                return;
            }

            var lines = observedData.GroupBy(x => x.DataSeriesName);
            foreach (var line in lines)
            {
                if (line.Any(x => x.Period > Model.NumberOfStressPeriods))
                {
                    Logger.Debug($"{line.Key} has data from a period outside the model duration.");
                    throw new OutputDataInvalidException("Too many stress periods in observed data.", RunStatus.InvalidOutput.RunStatusID);
                }

                Logger.Info($"{line.Key} successfully parsed for observed data. Should be set to true");
                result.ResultSets.First().DataSeries.Add(new DataSeries
                {
                    Name = line.Key,
                    IsDefaultDisplayed = true,
                    DataPoints = line.Select(a => CalculateObservedDataPoint(a, outputVolumeUnitEnum)).ToList(),
                    IsObserved = true,
                    TestProperty = "This is fake and just for testing Baseflow observed"
                });
            }
        }

        private RunResultDetails CalculateDifferentialImpactToBaseflow(IModelFileAccessor modflowFileAccessor, List<StressPeriod> stressPeriods, VolumeUnitEnum outputVolumeUnitEnum)
        {
            var baselineData = modflowFileAccessor.GetBaselineData();
            if (baselineData == null)
            {
                Logger.Warn("Not generating impact to baseflow because no baseflow data found.");
                return null;
            }

            var runData = modflowFileAccessor.GetOutputData();
            if (runData == null)
            {
                Logger.Warn("Not generating impact to baseflow because no run data found.");
                return null;
            }

            var numberOfSegmentReaches = modflowFileAccessor.GetNumberOfSegmentReaches();

            Logger.Debug("Calculating baseflow baseline data.");
            var baselinePoints = CalculateDataPoints(baselineData, stressPeriods, numberOfSegmentReaches, modflowFileAccessor, outputVolumeUnitEnum);

            Logger.Debug("Calculating run baseline data.");
            var runPoints = CalculateDataPoints(runData, stressPeriods, numberOfSegmentReaches, modflowFileAccessor, outputVolumeUnitEnum);

            Logger.Debug("Calculating run results.");
            return CreateRunResultDetails(modflowFileAccessor, runPoints, outputVolumeUnitEnum, baselinePoints);
        }

        private RunResultDetails CalculateNonDifferentialImpactToBaseflow(IModelFileAccessor modflowFileAccessor, List<StressPeriod> stressPeriods, VolumeUnitEnum outputVolumeUnitEnum)
        {
            var runData = modflowFileAccessor.GetOutputData();
            if (runData == null)
            {
                Logger.Warn("Not generating impact to baseflow because no run data found.");
                return null;
            }

            var numberOfSegmentReaches = modflowFileAccessor.GetNumberOfSegmentReaches();

            Logger.Debug("Calculating run baseline data.");
            var runPoints = CalculateDataPoints(runData, stressPeriods, numberOfSegmentReaches, modflowFileAccessor, outputVolumeUnitEnum);

            Logger.Debug("Calculating run results.");
            return CreateRunResultDetails(modflowFileAccessor, runPoints, outputVolumeUnitEnum);
        }

        private Dictionary<string, List<RunResultSetDataPoint>> CalculateDataPoints(IEnumerable<OutputData> data, List<StressPeriod> stressPeriods, int numberOfSegmentReaches, IModelFileAccessor modflowFileAccessor, VolumeUnitEnum outputVolumeUnitEnum)
        {
            var uniqueZoneList = modflowFileAccessor.GetAllZones();
            using (var outputData = data.GetEnumerator())
            {
                var result = new Dictionary<string, List<RunResultSetDataPoint>>();
                foreach (var zone in uniqueZoneList)
                {
                    result[zone] = new List<RunResultSetDataPoint>();
                }
                result[""] = new List<RunResultSetDataPoint>();

                for (var k = 0; k < stressPeriods.Count; k++)
                {
                    var currDate = Model.ModelStressPeriodCustomStartDates != null && Model.ModelStressPeriodCustomStartDates.Any() ? Model.ModelStressPeriodCustomStartDates[k].StressPeriodStartDate : Model.StartDateTime.AddMonths(k);

                    var dataPoints = new Dictionary<string, RunResultSetDataPoint>();
                    foreach (var zone in result.Keys)
                    {
                        dataPoints[zone] = new RunResultSetDataPoint { Date = currDate };
                    }

                    for (var i = 0; i < stressPeriods[k].NumberOfTimeSteps; i++)
                    {
                        for (var j = 0; j < numberOfSegmentReaches; j++)
                        {
                            CalculateDataPoint(outputData, stressPeriods[k], dataPoints, modflowFileAccessor, outputVolumeUnitEnum);
                        }
                    }

                    foreach (var dataPoint in dataPoints)
                    {
                        result[dataPoint.Key].Add(dataPoint.Value);
                    }
                }
                if (outputData.MoveNext())
                {
                    throw new OutputDataInvalidException("Too many output data rows.", RunStatus.InvalidOutput.RunStatusID);
                }
                return result;
            }
        }

        private static void CalculateDataPoint(IEnumerator<OutputData> outputData, StressPeriod stressPeriod, Dictionary<string, RunResultSetDataPoint> dataPoints, IModelFileAccessor modflowFileAccessor, VolumeUnitEnum outputVolumeUnitEnum)
        {
            if (!outputData.MoveNext() || outputData.Current == null)
            {
                throw new OutputDataInvalidException("Not enough output data rows.", RunStatus.InvalidOutput.RunStatusID);
            }

            var volumeFlow = UnitConversion.ConvertVolume(outputData.Current.FlowToAquifer, VolumeUnitEnum.CubicFeet, outputVolumeUnitEnum);
            var flowPerTimeStep = UnitConversion.CalculateVolumePerTimeStep(volumeFlow, stressPeriod);
            dataPoints[""].Value += flowPerTimeStep;
            foreach (var zone in modflowFileAccessor.GetSegmentReachZones(outputData.Current.SegmentNumber, outputData.Current.ReachNumber))
            {
                dataPoints[zone].Value += flowPerTimeStep;
            }
        }

        private static RunResultDetails CreateRunResultDetails(IModelFileAccessor modflowFileAccessor, Dictionary<string, List<RunResultSetDataPoint>> dataSets, VolumeUnitEnum volumeUnitEnum, Dictionary<string, List<RunResultSetDataPoint>> baseline = null)
        {
            var result = new RunResultDetails
            {
                RunResultName = baseline != null ? "Impacts to Baseflow" : "Baseflow",
                ResultSets = new List<RunResultSet>()
            };
            var series = new List<DataSeries>();

            foreach (var dataSet in dataSets.OrderBy(ds => ds.Key))
            {
                var seriesName = string.Empty;
                var isDefaultDisplay = false;
                if (!string.IsNullOrWhiteSpace(dataSet.Key))
                {
                    var friendlyZoneName = modflowFileAccessor.GetFriendlyInputZoneName(dataSet.Key);
                    seriesName = string.IsNullOrWhiteSpace(friendlyZoneName) ? $"Zone {dataSet.Key}" : friendlyZoneName;
                }
                else
                {
                    seriesName = "Total";
                    isDefaultDisplay = true;
                }

                series.Add(new DataSeries
                {
                    Name = seriesName,
                    IsDefaultDisplayed = isDefaultDisplay,
                    DataPoints = dataSet.Value.Select(a => baseline != null ? CalculateDifferentialDataPoint(a, baseline, dataSet) : a).ToList(),
                    IsObserved = false,
                    TestProperty = "This is fake and just for testing Baseflow non-observed"
                });
            }

            result.ResultSets.Add(new RunResultSet
            {
                DisplayType = RunResultDisplayType.LineChart,
                Name = "Rate",
                DataType = VolumeUnit.ToType(volumeUnitEnum).VolumeUnitDisplayName,
                DataSeries = series
            });

            return result;
        }

        private static RunResultSetDataPoint CalculateDifferentialDataPoint(RunResultSetDataPoint point, Dictionary<string, List<RunResultSetDataPoint>> baseline, KeyValuePair<string, List<RunResultSetDataPoint>> dataSet)
        {
            var baselineValue = baseline[dataSet.Key].Single(b => b.Date == point.Date);
            return new RunResultSetDataPoint
            {
                Date = point.Date,
                Value =
                    baselineValue.Value -
                    point.Value //modflow gives us the change to the aquifier and we want the change to the stream so we use baseline minus calculated to get the change to the stream
            };
        }

        private RunResultSetDataPoint CalculateObservedDataPoint(ObservedImpactToBaseflow point, VolumeUnitEnum volumeUnitEnum)
        {
            var convertedValue = UnitConversion.ConvertVolume(point.FlowToAquiferInAcreFeet, VolumeUnitEnum.AcreFeet, volumeUnitEnum);
            var stressPeriodDate = Model.ModelStressPeriodCustomStartDates != null && Model.ModelStressPeriodCustomStartDates.Any() ? Model.ModelStressPeriodCustomStartDates[point.Period - 1].StressPeriodStartDate : Model.StartDateTime;

            if (stressPeriodDate == Model.StartDateTime)
            {
                stressPeriodDate = stressPeriodDate.AddMonths(point.Period - 1);
            }

            return new RunResultSetDataPoint
            {
                Date = stressPeriodDate,
                Value = convertedValue
            };
        }
    }
}