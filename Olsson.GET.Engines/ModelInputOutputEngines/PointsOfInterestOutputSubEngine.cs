using log4net;
using Olsson.GET.Accessors.FileIO;
using Olsson.GET.Common.DataContracts.Runs;
using Olsson.GET.Common.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Model = Olsson.GET.Common.DataContracts.Models.Model;
using RunStatus = Olsson.GET.Accessors.EntityFramework.RunStatus;

namespace Olsson.GET.Engines.ModelInputOutputEngines
{
    internal interface IPointsOfInterestOutputSubEngine
    {
        List<RunResultDetails> GeneratePointsOfInterestGraphOutput(IModelFileAccessor modflowFileAccessor, List<StressPeriod> stressPeriods, int currResultId, bool isDifferential);
    }

    internal class PointsOfInterestOutputSubEngine : IPointsOfInterestOutputSubEngine
    {
        private static readonly ILogger Logger = Logging.GetLogger<ListFileOutputSubEngine>();
        public PointsOfInterestOutputSubEngine(Model model)
        {
            Model = model;
        }

        private Model Model { get; }

        public List<RunResultDetails> GeneratePointsOfInterestGraphOutput(IModelFileAccessor modflowFileAccessor, List<StressPeriod> stressPeriods, int currResultId, bool isDifferential)
        {
            Logger.LogInformation("Generating points of interest output.");

            if (stressPeriods == null || !stressPeriods.Any())
            {
                Logger.LogWarning("Not generating points of interest output because no stress period data found.");
                return new List<RunResultDetails>(); ;
            }

            var pointsOfInterest = modflowFileAccessor.GetPointsOfInterest();
            if (pointsOfInterest == null)
            {
                Logger.LogWarning("Not generating points of interest output because no points of interest file found.");
                return new List<RunResultDetails>(); ;
            }

            var cells = GetLocationCellDictionary(pointsOfInterest.ToList(), modflowFileAccessor);

            var result = CreateRunResultSetWithPointsOfInterest(pointsOfInterest.Select(x => x.Name).ToList());

            if (isDifferential)
            {
                Logger.LogInformation("Run is differential -- comparing to baseline.");
                AddFlowDeltas(result, modflowFileAccessor.GetBaselineMapData(), modflowFileAccessor.GetOutputMapData(), stressPeriods, cells);
            }
            else
            {
                Logger.LogInformation("Run is non-differential -- ignoring baseline data even if present.");
                AddFlowData(result, modflowFileAccessor.GetOutputMapData(), stressPeriods, cells);
            }

            AddObservedData(result, modflowFileAccessor, isDifferential);

            var resultDetails = new RunResultDetails
            {
                RunResultName = "Points of Interest",
                ResultSets = new List<RunResultSet>(),
            };
            resultDetails.ResultSets.Add(result);

            Logger.LogInformation("List file output results generated.");
            return new List<RunResultDetails>() { resultDetails };
        }

        private Dictionary<string, List<string>> GetLocationCellDictionary(List<PointOfInterest> pointsOfInterest, IModelFileAccessor modflowFileAccessor)
        {
            var cells = new Dictionary<string, List<string>>();
            foreach (var point in pointsOfInterest)
            {
                var cellLocation = modflowFileAccessor.FindLocationCell(point.Coordinate.Lat, point.Coordinate.Lng).Location.Replace(' ', '|');

                if (cells.ContainsKey(cellLocation))
                {
                    cells[cellLocation].Add(point.Name);
                }
                else
                {
                    cells.Add(cellLocation, new List<string> { point.Name });
                }
            }

            return cells;
        }

        private void AddObservedData(RunResultSet result, IModelFileAccessor modflowFileAccessor, bool isDifferential)
        {
            var observedData = modflowFileAccessor.GetObservedPointsOfInterest(isDifferential);
            if (observedData == null)
            {
                Logger.LogDebug("Observed data is not present -- skipping adding it to chart.");
                return;
            }

            foreach (var observedLocation in observedData.GroupBy(x => x.LocationSeriesName))
            {
                if (observedLocation.Any(x => x.Period > Model.NumberOfStressPeriods))
                {
                    Logger.LogDebug($"{observedLocation.Key} has data from a period outside the model duration.");
                    throw new OutputDataInvalidException("Too many stress periods in observed data.", RunStatus.InvalidOutput.RunStatusID);
                }

                result.DataSeries.Add(new DataSeries
                {
                    Name = observedLocation.Key,
                    IsDefaultDisplayed = false,
                    DataPoints = observedLocation.Select(CalculateObservedDataPoint).ToList(),
                    IsObserved = true
                });
            }
        }

        private RunResultSet CreateRunResultSetWithPointsOfInterest(List<string> pointsOfInterest)
        {
            var result = new RunResultSet()
            {
                Name = "Points of Interest",
                DisplayType = RunResultDisplayType.LineChart,
                DataSeries = new List<DataSeries>(),
                DataType = "Elevation (feet)",
            };

            foreach (var point in pointsOfInterest)
            {
                result.DataSeries.Add(new DataSeries()
                {
                    Name = point,
                    IsDefaultDisplayed = false,
                    DataPoints = new List<RunResultSetDataPoint>(),
                    IsObserved = false
                });
            }

            return result;
        }

        private void AddFlowDeltas(RunResultSet result, IEnumerable<MapOutputData> baseline, IEnumerable<MapOutputData> run, List<StressPeriod> stressPeriods, Dictionary<string, List<string>> locations)
        {
            using (var runEnumerator = run.GetEnumerator())
            {
                foreach (var baselineData in baseline)
                {
                    if (!runEnumerator.MoveNext() || runEnumerator.Current == null)
                    {
                        throw new OutputDataInvalidException("Not enough rows in map output data.", RunStatus.InvalidOutput.RunStatusID);
                    }

                    var runData = runEnumerator.Current;
                    var stressPeriod = stressPeriods[baselineData.StressPeriod - 1];

                    if (baselineData.StressPeriod != runData.StressPeriod || baselineData.TimeStep != runData.TimeStep || baselineData.Location != runData.Location)
                    {
                        throw new OutputDataInvalidException("Mismatched map output data.", RunStatus.InvalidOutput.RunStatusID);
                    }

                    if (baselineData.StressPeriod > stressPeriods.Count)
                    {
                        throw new OutputDataInvalidException("Stress period not found.", RunStatus.InvalidOutput.RunStatusID);
                    }

                    if (stressPeriod.NumberOfTimeSteps != baselineData.TimeStep ||
                        !locations.ContainsKey(baselineData.Location)) continue;

                    var pointsOfInterest = locations[baselineData.Location];
                    var stressPeriodDate = Model.ModelStressPeriodCustomStartDates != null && Model.ModelStressPeriodCustomStartDates.Any() ? Model.ModelStressPeriodCustomStartDates[baselineData.StressPeriod - 1].StressPeriodStartDate : Model.StartDateTime;

                    if (stressPeriodDate == Model.StartDateTime)
                    {
                        stressPeriodDate = stressPeriodDate.AddMonths(baselineData.StressPeriod - 1);
                    }

                    foreach (var pointOfInterest in pointsOfInterest)
                    {
                        AddDelta(runData, baselineData, stressPeriodDate, result.DataSeries.SingleOrDefault(x => x.Name == pointOfInterest));
                    }
                }
            }
        }


        private void AddFlowData(RunResultSet result, IEnumerable<MapOutputData> run, List<StressPeriod> stressPeriods, Dictionary<string, List<string>> locations)
        {
            foreach (var runData in run)
            {
                var stressPeriod = stressPeriods[runData.StressPeriod - 1];

                if (runData.StressPeriod > stressPeriods.Count)
                {
                    throw new OutputDataInvalidException("Stress period not found.", RunStatus.InvalidOutput.RunStatusID);
                }

                if (stressPeriod.NumberOfTimeSteps != runData.TimeStep ||
                    !locations.ContainsKey(runData.Location)) continue;

                var stressPeriodDate = Model.ModelStressPeriodCustomStartDates != null && Model.ModelStressPeriodCustomStartDates.Any() ? Model.ModelStressPeriodCustomStartDates[runData.StressPeriod - 1].StressPeriodStartDate : Model.StartDateTime;

                if (stressPeriodDate == Model.StartDateTime)
                {
                    stressPeriodDate = stressPeriodDate.AddMonths(runData.StressPeriod - 1);
                }
                var pointOfInterests = locations[runData.Location];
                foreach (var pointOfInterest in pointOfInterests)
                {
                    AddData(runData, stressPeriodDate, result.DataSeries.SingleOrDefault(x => x.Name == pointOfInterest));
                }
            }
        }


        private static void AddDelta(MapOutputData runData, MapOutputData baselineData, DateTime date, DataSeries result)
        {
            double difference = 0;

            if (runData.Value != null && baselineData.Value != null)
            {
                difference = runData.Value.Value - baselineData.Value.Value;
            }

            result.DataPoints.Add(new RunResultSetDataPoint()
            {
                Date = date,
                Value = difference
            });
        }

        private static void AddData(MapOutputData runData, DateTime date, DataSeries result)
        {
            result.DataPoints.Add(new RunResultSetDataPoint()
            {
                Date = date,
                Value = runData.Value ?? 0
            });
        }

        private RunResultSetDataPoint CalculateObservedDataPoint(ObservedPointOfInterest point)
        {
            var stressPeriodDate = Model.ModelStressPeriodCustomStartDates != null && Model.ModelStressPeriodCustomStartDates.Any() ? Model.ModelStressPeriodCustomStartDates[point.Period - 1].StressPeriodStartDate : Model.StartDateTime;

            if (stressPeriodDate == Model.StartDateTime)
            {
                stressPeriodDate = stressPeriodDate.AddMonths(point.Period - 1);
            }

            return new RunResultSetDataPoint
            {
                Date = stressPeriodDate,
                Value = point.ValueInCubicFeet
            };
        }

    }
}
