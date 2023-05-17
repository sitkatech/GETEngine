using log4net;
using Newtonsoft.Json;
using Olsson.GET.Accessors.FileIO;
using Olsson.GET.Common.DataContracts.Runs;
using Olsson.GET.Common.Utilities;
using SharpKml.Base;
using SharpKml.Dom;
using SharpKml.Engine;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using GeoJSON.Net.Feature;
using Olsson.GET.Accessors.EntityFramework;
using static System.String;
using DataContractsModels = Olsson.GET.Common.DataContracts.Models;
using RunStatus = Olsson.GET.Accessors.EntityFramework.RunStatus;

namespace Olsson.GET.Engines.ModelInputOutputEngines
{
    internal interface ILocationMapOutputSubEngine
    {
        (RelatedResultDetails OutputResults, OutputDataInvalidException Exception) CreateWaterLevelHeatMapResults(IModelFileAccessor modflowFileAccessor, List<StressPeriod> stressPeriods, bool isDifferential);
        RelatedResultDetails CreateWaterLevelByZoneHeatMapResults(IModelFileAccessor modflowFileAccessor, List<StressPeriod> stressPeriods, bool isDifferential, string outputLocationGeoJSONRaw);
        RelatedResultDetails CreateDrawdownHeatMapResults(IModelFileAccessor modflowFileAccessor, List<StressPeriod> stressPeriods, bool isDifferential);
    }

    internal class LocationMapOutputSubEngine : ILocationMapOutputSubEngine
    {
        private const string KmlSchemaName = "heatmap_geojson";
        private const string KmlDocumentName = "heatmap";

        public LocationMapOutputSubEngine(DataContractsModels.Model model)
        {
            Model = model;
        }
        private static readonly ILog Logger = Logging.GetLogger(typeof(LocationMapOutputSubEngine));
        private DataContractsModels.Model Model { get; }
        public (RelatedResultDetails OutputResults, OutputDataInvalidException Exception) CreateWaterLevelHeatMapResults(IModelFileAccessor modflowFileAccessor, List<StressPeriod> stressPeriods, bool isDifferential)
        {
            Logger.Info("Creating Water Level Heat Map");

            if (!StressPeriodsAreValid(stressPeriods)) return (null, null);

            Logger.Debug("Getting location position map.");
            var locations = modflowFileAccessor.GetLocationPositionMap();
            if (!LocationsAreValid(locations)) return (null, null);

            var run = modflowFileAccessor.GetOutputMapData();
            if (!MapDataIsValid(run)) return (null, null);

            MapDataStats stats;
            List<Common.DataContracts.Runs.MapData> heatMap;
            if (isDifferential)
            {
                Logger.Info("Run is differential -- comparing to baseline.");

                var baseline = modflowFileAccessor.GetBaselineMapData();
                if (!MapDataIsValid(baseline)) return (null, null);

                string calculateColor(MapLocationState state, double value, double min, double max) => GetDifferentialColor(state, value, min, max, false);

                Logger.Debug("Creating heat map legend from map stats.");
                stats = CalculateDifferentialFlowStats(baseline, run, stressPeriods, locations);
                var legend = GetDifferentialLegendData(stats, calculateColor);

                Logger.Debug("Creating heat map colors and locations.");
                heatMap = CreateDifferentialFlowHeatMapData(baseline, run, modflowFileAccessor, stressPeriods, locations, stats, legend, calculateColor);
            }
            else
            {
                Logger.Info("Run is non-differential -- ignoring baseline data even if present.");

                string calculateColor(MapLocationState state, double value, double min, double max) => GetNonDifferentialColor(state, value, min, max);

                Logger.Debug("Creating heat map legend from map stats.");
                stats = CalculateMapStats(run, stressPeriods, locations);
                var legend = GetLegendData(stats, calculateColor);

                Logger.Debug("Creating heat map colors and locations.");
                heatMap = CreateHeatMapData(run, modflowFileAccessor, stressPeriods, locations, stats, legend, calculateColor);
            }

            var runResultDetails = heatMap.OrderBy(a => a.CurrentStressPeriod).Select(a => new RunResultDetails
            {
                RunResultName = GetStressPeriodLabel(a.CurrentStressPeriod),
                ResultSets = new List<RunResultSet>
                {
                    new RunResultSet
                    {
                        Name = "WellMap",
                        DisplayType = RunResultDisplayType.Map,
                        MapData = a
                    }
                }
            }).ToList();

            if (!runResultDetails.Any())
            {
                return (null, null);
            }

            Logger.Info("Heat map result calculated.");

            // We allow locations to have dry cells on non-differential runs, but it probably indicates a model issue on a differential run
            var invalidOutput = stats.HasRanDry && isDifferential;

            return (new RelatedResultDetails
            {
                SetName = isDifferential ? "Water Level Change" : "Water Level",
                RelatedResults = runResultDetails
            }, invalidOutput ? new OutputDataInvalidException("At least one location has run dry.", RunStatus.HasDryCells.RunStatusID) : null);
        }

        public RelatedResultDetails CreateWaterLevelByZoneHeatMapResults(IModelFileAccessor modflowFileAccessor, List<StressPeriod> stressPeriods, bool isDifferential, string outputZoneGeoJSONRaw)
        {
            var title = isDifferential ? "Water Level Change By Zone" : "Water Level By Zone";

            Logger.Info($"Creating {title} Heat Map");

            if (!modflowFileAccessor.OutputLocationZonesExists() || IsNullOrWhiteSpace(outputZoneGeoJSONRaw))
            {
                Logger.Warn($"Not generating {title} map because Output Zone information is missing.");
                return null;
            }

            Logger.Debug("Getting location position map.");
            var locations = modflowFileAccessor.GetLocationPositionMap();
            if (!LocationsAreValid(locations)) return null;
            var sortedLocations = new HashSet<string>(locations);

            if (!StressPeriodsAreValid(stressPeriods)) return null;

            //For this map, we only care about the last stress period
            var stressPeriod = stressPeriods[stressPeriods.Count - 1];
            var maxTimeStep = stressPeriod.NumberOfTimeSteps;

            var run = modflowFileAccessor.GetOutputMapData();
            if (!MapDataIsValidForWaterLevelByZone(run, stressPeriods.Count)) return null;

            var finalStressPeriodTimeStepMapData = run
                .Where(x => x.StressPeriod == stressPeriods.Count && x.TimeStep == maxTimeStep && sortedLocations.Contains(x.Location)).AsEnumerable();

            var outputZones = JsonConvert.DeserializeObject<DataContractsModels.Zone[]>(outputZoneGeoJSONRaw);
            MapDataStats stats;
            Common.DataContracts.Runs.MapData heatMap  =  new Common.DataContracts.Runs.MapData();
            List<LegendItem> legend;
            List<WaterLevelChangeByZone> dataForZones;

            string calculateColor(MapLocationState state, double value, double min, double max) => isDifferential ?
                GetDifferentialColor(state, value, min, max, false) :
                GetNonDifferentialColor(state, value, min, max);

            if (isDifferential)
            {
                Logger.Info("Run is differential -- comparing to baseline.");

                var baseline = modflowFileAccessor.GetBaselineMapData();
                if (!MapDataIsValidForWaterLevelByZone(baseline, stressPeriods.Count)) return null;

                var finalStressPeriodTimeStepBaselineData = baseline
                    .Where(x => x.StressPeriod == stressPeriods.Count && x.TimeStep == maxTimeStep && sortedLocations.Contains(x.Location)).AsEnumerable();

                dataForZones = GetWaterLevelChangeByZonesFromMapOutputData(modflowFileAccessor,
                    finalStressPeriodTimeStepBaselineData, finalStressPeriodTimeStepMapData, outputZones);

                if (dataForZones.Count != outputZones.Length
                    || outputZones.Any(x => dataForZones.All(y => x.Name != y.ZoneName))
                    || outputZones.Any(x => x.ZoneNumber != dataForZones.Single(y => y.ZoneName == x.Name).ZoneNumber))
                {
                    Logger.Warn($"Not returning {title} map because the zones found in the OutputZoneGeoJSON and the Zones from the OutputZones.csv do not match.");
                    return null;
                }

                var absoluteMeans = dataForZones.Select(x => Math.Abs(x.Mean)).ToList();
                var maxMean = GetNearestValueInSameOrderOfMagnitude(absoluteMeans.Max(), true);
                var minMean = GetNearestValueInSameOrderOfMagnitude(absoluteMeans.Min(), false);

                stats = new MapDataStats()
                {
                    MaximumValue = maxMean,
                    MinimumValue = minMean
                };

                legend = GetDifferentialLegendData(stats, calculateColor);
            }
            else
            {
                dataForZones = GetWaterLevelByZonesFromMapOutputData(modflowFileAccessor, finalStressPeriodTimeStepMapData, outputZones);

                if (dataForZones.Count != outputZones.Length
                    || outputZones.Any(x => dataForZones.All(y => x.Name != y.ZoneName))
                    || outputZones.Any(x => x.ZoneNumber != dataForZones.Single(y => y.ZoneName == x.Name).ZoneNumber))
                {
                    Logger.Warn($"Not returning {title} map because the zones found in the OutputZoneGeoJSON and the Zones from the OutputZones.csv do not match.");
                    return null;
                }

                var maxValue = dataForZones.Select(x => x.Mean).Max();
                var minValue = dataForZones.Select(x => x.Mean).Min();

                stats = new MapDataStats()
                {
                    MaximumValue = GetNearestValueInSameOrderOfMagnitude(maxValue, maxValue > 0),
                    MinimumValue = GetNearestValueInSameOrderOfMagnitude(minValue, minValue < 0)
                };

                legend = GetLegendData(stats, calculateColor);
            }

            var waterLevelByZoneMapData = new List<WaterLevelChangeByZoneMapData>();

            dataForZones.ForEach(x =>
            {
                var color = calculateColor(MapLocationState.Normal,
                    dataForZones.First(y => y.ZoneName == x.ZoneName).Mean, stats.MinimumValue, stats.MaximumValue);

                waterLevelByZoneMapData.Add(new WaterLevelChangeByZoneMapData()
                {
                    Bounds = outputZones.Single(y => y.ZoneNumber == x.ZoneNumber).Bounds,
                    Color = color ?? "",
                    Mean = x.Mean,
                    ZoneName = x.ZoneName
                });
                
            });

            heatMap.MapPoints =
                modflowFileAccessor.CreateSerializedFeatureCollectionFromWaterLevelChangeByZoneMapData(
                    waterLevelByZoneMapData);
            heatMap.Legend = legend;
            heatMap.KmlString = !IsNullOrEmpty(heatMap.MapPoints) ? GetWaterLevelByZoneKmlString(heatMap.MapPoints, legend, waterLevelByZoneMapData, isDifferential) : null;
            heatMap.ContainsKmlFile = !IsNullOrEmpty(heatMap.MapPoints);

            var runResultDetails = new List<RunResultDetails>
            {
                new RunResultDetails
                {
                    RunResultName = title,
                    ResultSets = new List<RunResultSet>
                    {
                        new RunResultSet
                        {
                            Name = title,
                            DisplayType = RunResultDisplayType.Map,
                            MapData = heatMap,
                            WaterLevelChangeByZones = dataForZones
                        }
                    }
                }
            };

            Logger.Info("Creating Water Level By Zone Heat Map created.");

            return new RelatedResultDetails
            {
                SetName = title,
                RelatedResults = runResultDetails
            };
        }

        private double GetNearestValueInSameOrderOfMagnitude(double value, bool roundUp)
        {
            if (value == 0)
            {
                return value;
            }

            var initialDigits = Math.Floor(Math.Log10(value));
            var digits = initialDigits;

            if (initialDigits < 1)
            {
                value *= Math.Pow(10, Math.Abs(initialDigits) + 1);
                digits = 2;
            }

            var unit = Math.Pow(10, digits);

            var valueToReturn = (roundUp ? Math.Ceiling(value / unit) : Math.Floor(value / unit)) * unit;

            return initialDigits >= 1 ? valueToReturn : valueToReturn * Math.Pow(10, initialDigits - 1);
        }

        private List<WaterLevelChangeByZone> GetWaterLevelChangeByZonesFromMapOutputData(
            IModelFileAccessor modflowFileAccessor, IEnumerable<MapOutputData> baselineOutputData,
            IEnumerable<MapOutputData> mapOutputData, DataContractsModels.Zone[] outputZones)
        {
            Dictionary<string, List<double>> zoneAndValues = new Dictionary<string, List<double>>();
            using (var runEnumerator = mapOutputData.GetEnumerator())
            {
                foreach (var baselineData in baselineOutputData)
                {
                    if (!runEnumerator.MoveNext() || runEnumerator.Current == null)
                    {
                        throw new OutputDataInvalidException("Not enough rows in map output data.", RunStatus.InvalidOutput.RunStatusID);
                    }
                    var runData = runEnumerator.Current;

                    if (baselineData.StressPeriod != runData.StressPeriod || baselineData.TimeStep != runData.TimeStep || baselineData.Location != runData.Location)
                    {
                        throw new OutputDataInvalidException("Mismatched map output data.", RunStatus.InvalidOutput.RunStatusID);
                    }

                    //The runs value is dry or the baseline is dry, so exclude that from our calculations
                    if (!runData.Value.HasValue || !baselineData.Value.HasValue)
                    {
                        continue;
                    }

                    var zones = modflowFileAccessor.GetOutputLocationZones(baselineData.Location);

                    //If we have a location that isn't within a zone, don't add it.
                    //Otherwise it'll go into a grab bag of unzoned-locations and will give misleading results
                    if (zones.Count == 0)
                    {
                        continue;
                    }

                    foreach (var zone in zones)
                    {
                        if (!zoneAndValues.ContainsKey(zone))
                        {
                            zoneAndValues[zone] = new List<double>();
                        }

                        zoneAndValues[zone].Add(runData.Value.Value - baselineData.Value.Value);
                    }
                }
            }

            return zoneAndValues.Select(x =>
            {
                var absoluteValues = x.Value.Select(y => Math.Abs(y)).ToList();
                var max = absoluteValues.Max();
                var maxTolerance = Math.Abs(max * 0.00001);
                var min = absoluteValues.Min();
                var minTolerance = Math.Abs(min * 0.00001);

                return new WaterLevelChangeByZone
                {
                    ZoneName = outputZones.SingleOrDefault(y => y.ZoneNumber == x.Key)?.Name,
                    ZoneNumber = x.Key,
                    Maximum = Math.Round(x.Value.First(y => Math.Abs(Math.Abs(y) - max) <= maxTolerance), 2),
                    Minimum = Math.Round(x.Value.First(y => Math.Abs(Math.Abs(y) - min) <= minTolerance), 2),
                    Mean = Math.Round(x.Value.Average(), 2)
                };
            }).OrderBy(x => x.ZoneName).ToList();
        }

        private List<WaterLevelChangeByZone> GetWaterLevelByZonesFromMapOutputData(
            IModelFileAccessor modflowFileAccessor,
            IEnumerable<MapOutputData> mapOutputData, DataContractsModels.Zone[] outputZones)
        {
            Dictionary<string, List<double>> zoneAndValues = new Dictionary<string, List<double>>();

            foreach (var mapData in mapOutputData)
            {
                //This indicates a dry cell
                if (!mapData.Value.HasValue)
                {
                    continue;
                }

                var zones = modflowFileAccessor.GetOutputLocationZones(mapData.Location);

                //If we have a location that isn't within a zone, don't add it.
                //Otherwise it'll go into a grab bag of unzoned-locations and will give misleading results
                if (zones.Count == 0)
                {
                    continue;
                }

                foreach (var zone in zones)
                {
                    if (!zoneAndValues.ContainsKey(zone))
                    {
                        zoneAndValues[zone] = new List<double>();
                    }

                    zoneAndValues[zone].Add(mapData.Value.Value);
                }
            }

            return zoneAndValues.Select(x =>
            {
                var zoneName = outputZones.SingleOrDefault(y => y.ZoneNumber == x.Key)?.Name;

                return new WaterLevelChangeByZone
                {
                    ZoneName = zoneName,
                    ZoneNumber = x.Key,
                    Maximum = Math.Round(x.Value.Max(), 2),
                    Minimum = Math.Round(x.Value.Min(), 2),
                    Mean = Math.Round(x.Value.Average(), 2)
                };
            }).OrderBy(x => x.ZoneName).ToList();
        }

        public RelatedResultDetails CreateDrawdownHeatMapResults(IModelFileAccessor modflowFileAccessor, List<StressPeriod> stressPeriods, bool isDifferential)
        {
            Logger.Info("Creating Drawdown Heat Map");

            if (isDifferential)
            {
                Logger.Warn("Not generating heat map because run is differential.");
                return null;
            }

            if (!StressPeriodsAreValid(stressPeriods)) return null;

            Logger.Debug("Getting location position map.");
            var locations = modflowFileAccessor.GetLocationPositionMap();
            if (!LocationsAreValid(locations)) return (null);

            var drawdown = modflowFileAccessor.GetDrawdownMapData();
            if (!MapDataIsValid(drawdown)) return null;

            string calculateColor(MapLocationState state, double value, double min, double max) => GetDifferentialColor(state, value, min, max, true);

            Logger.Debug("Creating heat map legend from map stats.");
            var stats = CalculateMapStats(drawdown, stressPeriods, locations);
            var legend = GetDifferentialLegendData(stats, calculateColor);

            Logger.Debug("Creating heat map colors and locations.");
            var heatMap = CreateHeatMapData(drawdown, modflowFileAccessor, stressPeriods, locations, stats, legend, calculateColor);

            var runResultDetails = heatMap.OrderBy(a => a.CurrentStressPeriod).Select(a => new RunResultDetails
            {
                RunResultName = GetStressPeriodLabel(a.CurrentStressPeriod),
                ResultSets = new List<RunResultSet>
                {
                    new RunResultSet
                    {
                        Name = "WellMap",
                        DisplayType = RunResultDisplayType.Map,
                        MapData = a
                    }
                }
            }).ToList();

            if (!runResultDetails.Any())
            {
                return null;
            }

            Logger.Info("Heat map result calculated.");

            return (new RelatedResultDetails
            {
                SetName = "Drawdown",
                RelatedResults = runResultDetails
            });
        }

        private MapDataStats CalculateDifferentialFlowStats(IEnumerable<MapOutputData> baseline, IEnumerable<MapOutputData> run, List<StressPeriod> stressPeriods, List<string> locations)
        {
            Logger.Debug("Calculating heat map flow change.");

            var result = new MapDataStats
            {
                HasRanDry = false,
                HasWasDry = false,
                MaximumValue = double.MinValue,
                MinimumValue = double.MaxValue,
            };

            var sortedLocations = new HashSet<string>(locations);

            using (var runEnumerator = run.GetEnumerator())
            {
                foreach (var baselineData in baseline)
                {
                    if (!runEnumerator.MoveNext() || runEnumerator.Current == null)
                    {
                        throw new OutputDataInvalidException("Not enough rows in map output data.", RunStatus.InvalidOutput.RunStatusID);
                    }
                    var runData = runEnumerator.Current;
                    if (baselineData.StressPeriod != runData.StressPeriod || baselineData.TimeStep != runData.TimeStep || baselineData.Location != runData.Location)
                    {
                        throw new OutputDataInvalidException("Mismatched map output data.", RunStatus.InvalidOutput.RunStatusID);
                    }

                    if (baselineData.StressPeriod > stressPeriods.Count)
                    {
                        throw new OutputDataInvalidException("Stress period not found.", RunStatus.InvalidOutput.RunStatusID);
                    }

                    var stressPeriod = stressPeriods[baselineData.StressPeriod - 1];
                    if (stressPeriod.NumberOfTimeSteps == baselineData.TimeStep && sortedLocations.Contains(baselineData.Location))
                    {
                        UpdateFlowDeltaStats(runData, baselineData, result);
                    }
                }
            }

            return result;
        }

        private MapDataStats CalculateMapStats(IEnumerable<MapOutputData> mapOutput, List<StressPeriod> stressPeriods, List<string> locations)
        {
            Logger.Debug("Calculating non-differential heat map stats.");

            var result = new MapDataStats
            {
                HasRanDry = false,
                HasWasDry = false,
                MaximumValue = double.MinValue,
                MinimumValue = double.MaxValue,
            };

            var sortedLocations = new HashSet<string>(locations);

            foreach (var mapData in mapOutput)
            {
                if (mapData.StressPeriod > stressPeriods.Count)
                {
                    throw new OutputDataInvalidException("Stress period not found.", RunStatus.InvalidOutput.RunStatusID);
                }

                var stressPeriod = stressPeriods[mapData.StressPeriod - 1];
                if (stressPeriod.NumberOfTimeSteps == mapData.TimeStep && sortedLocations.Contains(mapData.Location))
                {
                    UpdateMapDataStats(mapData, result);
                }
            }

            return result;
        }

        private List<Common.DataContracts.Runs.MapData> CreateDifferentialFlowHeatMapData(IEnumerable<MapOutputData> baseline, IEnumerable<MapOutputData> run, IModelFileAccessor modflowFileAccessor, List<StressPeriod> stressPeriods, List<string> locations, MapDataStats flowDataStats, List<LegendItem> legend, Func<MapLocationState, double, double, double, string> calculateColor)
        {
            var sortedLocations = new HashSet<string>(locations);

            var stressPeriodIndex = 0;
            var stressPeriod = stressPeriods[stressPeriodIndex];
            var stressPeriodFlowData = new List<MapLocationPositionCellValue>();

            var result = new ConcurrentBag<Common.DataContracts.Runs.MapData>();

            using (var runEnumerator = run.GetEnumerator())
            {
                foreach (var baselineData in baseline)
                {
                    runEnumerator.MoveNext();
                    var runData = runEnumerator.Current;

                    // Since the run data's stress period is not zero indexed, this condition happens if we've advanced to the next stress period
                    if (runData.StressPeriod - 1 == stressPeriodIndex + 1)
                    {
                        // If we reach run data that is for the next stress period, normalize and add that period.
                        result.Add(NormalizeNodes(modflowFileAccessor, stressPeriodIndex, stressPeriodFlowData));

                        stressPeriodIndex++;
                        stressPeriod = stressPeriods[stressPeriodIndex];
                        stressPeriodFlowData = new List<MapLocationPositionCellValue>();
                    }

                    if (baselineData.StressPeriod - 1 != stressPeriodIndex || runData.StressPeriod - 1 != stressPeriodIndex)
                    {
                        throw new OutputDataInvalidException("Data is out of order.", RunStatus.InvalidOutput.RunStatusID);
                    }

                    if (sortedLocations.Contains(baselineData.Location))
                    {
                        AddFlowDelta(runData, baselineData, stressPeriodFlowData, flowDataStats, calculateColor);
                    }
                }
            }

            //Because of logic above, we will always miss grabbing the final set of data if there is any
            //So, if we do have data upon exiting the loop, we need to add it
            if (stressPeriodFlowData.Count > 0)
            {
                result.Add(NormalizeNodes(modflowFileAccessor, stressPeriodIndex, stressPeriodFlowData));
            }

            foreach (var map in result)
            {
                map.Legend = legend;
                map.KmlString = !IsNullOrEmpty(map.MapPoints) ? GetKmlString(map.MapPoints, legend, GetStressPeriodLabel(map.CurrentStressPeriod)) : null;
            }

            return result.OrderBy(a => a.CurrentStressPeriod).ToList();
        }

        private List<Common.DataContracts.Runs.MapData> CreateHeatMapData(IEnumerable<MapOutputData> mapOutput, IModelFileAccessor modflowFileAccessor, List<StressPeriod> stressPeriods, List<string> locations, MapDataStats mapDataStats, List<LegendItem> legend, Func<MapLocationState, double, double, double, string> calculateColor)
        {
            var sortedLocations = new HashSet<string>(locations);

            var stressPeriodIndex = 0;
            var stressPeriod = stressPeriods[stressPeriodIndex];
            var stressPeriodFlowData = new List<MapLocationPositionCellValue>();

            var result = new ConcurrentBag<Common.DataContracts.Runs.MapData>();

            foreach (var mapData in mapOutput)
            {
                // Since the map data's stress period is not zero indexed, this condition happens if we've advanced to the next stress period
                if (mapData.StressPeriod - 1 == stressPeriodIndex + 1)
                {
                    // If we reach map data that is for the next stress period, normalize and add that period.
                    result.Add(NormalizeNodes(modflowFileAccessor, stressPeriodIndex, stressPeriodFlowData));

                    stressPeriodIndex++;
                    stressPeriod = stressPeriods[stressPeriodIndex];
                    stressPeriodFlowData = new List<MapLocationPositionCellValue>();
                }

                if (mapData.StressPeriod - 1 != stressPeriodIndex)
                {
                    throw new OutputDataInvalidException("Data is out of order.", RunStatus.InvalidOutput.RunStatusID);
                }

                if (mapData.TimeStep == stressPeriod.NumberOfTimeSteps && sortedLocations.Contains(mapData.Location))
                {
                    AddMapData(mapData, stressPeriodFlowData, mapDataStats, calculateColor);
                }
            }

            //Because of logic above, we will always miss grabbing the final set of data if there is any
            //So, if we do have data upon exiting the loop, we need to add it
            if (stressPeriodFlowData.Count > 0)
            {
                result.Add(NormalizeNodes(modflowFileAccessor, stressPeriodIndex, stressPeriodFlowData));
            }

            foreach (var map in result)
            {
                map.Legend = legend;
                map.KmlString = !IsNullOrEmpty(map.MapPoints) ? GetKmlString(map.MapPoints, legend, GetStressPeriodLabel(map.CurrentStressPeriod)) : null;
            }

            return result.OrderBy(a => a.CurrentStressPeriod).ToList();
        }

        private Common.DataContracts.Runs.MapData NormalizeNodes(IModelFileAccessor modflowFileAccessor, int stressPeriod, List<MapLocationPositionCellValue> values)
        {
            var resultMapData = new Common.DataContracts.Runs.MapData
            {
                CurrentStressPeriod = stressPeriod
            };
            var mapCells = CalculateMapCellLocations(modflowFileAccessor, stressPeriod, values.Where(a => !IsNullOrWhiteSpace(a.Color)).ToList());
            resultMapData.MapPoints = mapCells;
            return resultMapData;
        }

        internal List<LegendItem> GetDifferentialLegendData(MapDataStats mapStats, Func<MapLocationState, double, double, double, string> calculateColor)
        {
            List<LegendItem> legend;
            var largestAbsoluteValue = Math.Max(Math.Abs(mapStats.MaximumValue), Math.Abs(mapStats.MinimumValue));

            if (largestAbsoluteValue == 0)
            {
                legend = new List<LegendItem>();
            }
            else
            {
                legend = LegendDataPointsValues.OrderByDescending(a => a).Select(a => largestAbsoluteValue * a)
                    .Select(a => new LegendItem
                    {
                        IncreaseColor = calculateColor(MapLocationState.Normal, a, mapStats.MinimumValue, mapStats.MaximumValue),
                        DecreaseColor = calculateColor(MapLocationState.Normal, -1.0 * a, mapStats.MinimumValue, mapStats.MaximumValue),
                        Value = a
                    })
                    .ToList();
            }
            if (mapStats.HasRanDry)
            {
                legend.Add(new LegendItem
                {
                    IncreaseColor = calculateColor(MapLocationState.RanDry, 0, mapStats.MinimumValue, mapStats.MaximumValue),
                    Text = "Ran Dry"
                });
            }
            if (mapStats.HasWasDry)
            {
                legend.Add(new LegendItem
                {
                    IncreaseColor = calculateColor(MapLocationState.WasDry, 0, mapStats.MinimumValue, mapStats.MaximumValue),
                    Text = "Was Dry"
                });
            }
            if (mapStats.HasIsDry)
            {
                legend.Add(new LegendItem
                {
                    IncreaseColor = calculateColor(MapLocationState.WasDry, 0, mapStats.MinimumValue, mapStats.MaximumValue),
                    Text = "Is Dry"
                });
            }
            if (legend.Any())
            {
                return legend;
            }
            return null;
        }

        internal List<LegendItem> GetLegendData(MapDataStats mapStats, Func<MapLocationState, double, double, double, string> calculateColor)
        {
            List<LegendItem> legend;
            if (mapStats.MaximumValue == double.MinValue && mapStats.MinimumValue == double.MaxValue)
            {
                legend = new List<LegendItem>();
            }
            else
            {
                var range = mapStats.MaximumValue - mapStats.MinimumValue;
                legend = LegendDataPointsValues.OrderByDescending(a => a).Select(a => (range * a) + mapStats.MinimumValue)
                    .Select(a => new LegendItem
                    {
                        IncreaseColor = calculateColor(MapLocationState.Normal, a, mapStats.MinimumValue, mapStats.MaximumValue),
                        Value = a
                    })
                    .ToList();
            }
            if (mapStats.HasIsDry)
            {
                legend.Add(new LegendItem
                {
                    IncreaseColor = calculateColor(MapLocationState.IsDry, 0, mapStats.MinimumValue, mapStats.MaximumValue),
                    Text = "Is Dry"
                });
            }
            if (legend.Any())
            {
                return legend;
            }
            return null;
        }

        private static void UpdateFlowDeltaStats(MapOutputData runData, MapOutputData baselineData, MapDataStats result)
        {
            if (runData.Value == null && baselineData.Value == null)
            {
                //when both are null that is the equivilant of no change
            }
            else if (runData.Value != null && baselineData.Value != null)
            {
                var difference = runData.Value.Value - baselineData.Value.Value;
                result.MaximumValue = Math.Max(result.MaximumValue, difference);
                result.MinimumValue = Math.Min(result.MinimumValue, difference);
            }
            else if (runData.Value == null)
            {
                //a location has run dry
                result.HasRanDry = true;
            }
            else
            {
                //a baseline location that was dry now has a value
                result.HasWasDry = true;
            }
        }

        private static void UpdateMapDataStats(MapOutputData runData, MapDataStats result)
        {
            if (runData.Value != null)
            {
                result.MaximumValue = Math.Max(result.MaximumValue, runData.Value.Value);
                result.MinimumValue = Math.Min(result.MinimumValue, runData.Value.Value);
            }
            else
            {
                // We'll show any dry cells as "Is Dry" -- not differentiating ran dry from already was dry
                result.HasIsDry = true;
            }
        }

        private static void AddFlowDelta(MapOutputData runData, MapOutputData baselineData, List<MapLocationPositionCellValue> result, MapDataStats stats, Func<MapLocationState, double, double, double, string> calculateColor)
        {
            double difference;
            MapLocationState state;
            if (runData.Value == null && baselineData.Value == null)
            {
                difference = 0; //when both are null that is the equivilant of no change
                state = MapLocationState.Normal;
            }
            else if (runData.Value != null && baselineData.Value != null)
            {
                difference = runData.Value.Value - baselineData.Value.Value;
                state = MapLocationState.Normal;
            }
            else if (runData.Value == null)
            {
                //a location has run dry
                difference = 0;
                state = MapLocationState.RanDry;
            }
            else
            {
                //a baseline location that was dry now has a value
                difference = 0;
                state = MapLocationState.WasDry;
            }

            bool includeNode = difference.IsNotEqual(0) || state != MapLocationState.Normal;
            if (includeNode)
            {
                result.Add(new MapLocationPositionCellValue
                {
                    Location = baselineData.Location,
                    Value = difference,
                    Color = calculateColor(state, difference, stats.MinimumValue, stats.MaximumValue),
                    State = state
                });
            }
        }

        private static void AddMapData(MapOutputData mapOutput, List<MapLocationPositionCellValue> result, MapDataStats stats, Func<MapLocationState, double, double, double, string> calculateColor)
        {
            MapLocationState state;
            if (mapOutput.Value != null)
            {
                state = MapLocationState.Normal;
            }
            else
            {
                // We'll show any dry cells as "Is Dry" -- not differentiating ran dry from already was dry
                state = MapLocationState.IsDry;
            }

            var value = mapOutput.Value ?? 0;
            result.Add(new MapLocationPositionCellValue
            {
                Location = mapOutput.Location,
                Value = value,
                Color = calculateColor(state, value, stats.MinimumValue, stats.MaximumValue),
                State = state
            });
        }

        internal class MapDataStats
        {
            public double MaximumValue { get; set; }
            public double MinimumValue { get; set; }
            public bool HasRanDry { get; set; }
            public bool HasWasDry { get; set; }
            public bool HasIsDry { get; set; }
        }

        internal class MapData
        {
            // Values may be deltas or actuals depending on whether the run is differential or not
            public Dictionary<int, List<MapLocationPositionCellValue>> Values { get; set; }
        }

        internal class MapLocationPositionCellValue
        {
            public string Location { get; set; }
            public double Value { get; set; }
            public string Color { get; set; }
            public MapLocationState State { get; set; }
        }

        private const double ColorRoundingFactor = .1;

        private const double MinimumRatio = .01;

        private const float MaxColorScaleFactor = 0f; //-2 is all black, -1 is half way to black, 0 is the base color, 1 is half way to white, 2 is all white

        private const float MinColorScaleFactor = 1.6f; //-2 is all black, -1 is half way to black, 0 is the base color, 1 is half way to white, 2 is all white

        private const string ZeroValueColor = null; //"#00FF00";

        private static readonly System.Drawing.Color NegativeColor = System.Drawing.Color.FromArgb(0, 255, 0, 0);

        private static readonly System.Drawing.Color PostitiveColor = System.Drawing.Color.FromArgb(0, 0, 0, 255);

        private static readonly System.Drawing.Color DrawdownNegativeColor = System.Drawing.Color.FromArgb(0, 0, 0, 255);

        private static readonly System.Drawing.Color DrawdownPostitiveColor = System.Drawing.Color.FromArgb(0, 255, 0, 0);

        private const string RanDryColor = "#000000";

        private const string WasDryColor = "#00FF00";

        private const string IsDryColor = "#00FF00";

        private static readonly double[] LegendDataPointsValues = { .01, .1, .2, .3, .4, .5, .6, .7, .8, .9, 1 };

        internal static string GetDifferentialColor(MapLocationState state, double value, double min, double max, bool useDrawDownColors)
        {
            var maximumAbsoluteValue = Math.Max(Math.Abs(min), Math.Abs(max));
            if (state == MapLocationState.RanDry)
            {
                return RanDryColor;
            }
            if (state == MapLocationState.WasDry)
            {
                return WasDryColor;
            }
            if (maximumAbsoluteValue.IsEqual(0.0))
            {
                return ZeroValueColor;
            }

            var ratio = Math.Abs(value / maximumAbsoluteValue);
            if (ratio < MinimumRatio)
            {
                return ZeroValueColor;
            }
            var roundedRatio = (float)(Math.Round(ratio / ColorRoundingFactor, MidpointRounding.AwayFromZero) * ColorRoundingFactor);
            if (roundedRatio > 1)
            {
                roundedRatio = 1;
            }
            var valueColorScale = ((1.0f - roundedRatio) * (MinColorScaleFactor - MaxColorScaleFactor)) + MaxColorScaleFactor;

            Color color;
            if (useDrawDownColors)
            {
                color = value < 0 ? DrawdownNegativeColor : DrawdownPostitiveColor;
            }
            else
            {
                color = value < 0 ? NegativeColor : PostitiveColor;
            }

            return Intern(System.Drawing.ColorTranslator.ToHtml(ControlPaint.Light(color, valueColorScale)));
        }

        internal static string GetNonDifferentialColor(MapLocationState state, double value, double min, double max)
        {
            if (state == MapLocationState.IsDry)
            {
                return IsDryColor;
            }
            if (max.IsEqual(0.0) && min.IsEqual(0.0))
            {
                return ZeroValueColor;
            }

            var range = max - min;
            var valueAboveMin = value - min;

            var ratio = Math.Abs(valueAboveMin / range);
            if (ratio < MinimumRatio)
            {
                return ZeroValueColor;
            }
            var roundedRatio = (float)(Math.Round(ratio / ColorRoundingFactor, MidpointRounding.AwayFromZero) * ColorRoundingFactor);
            if (roundedRatio > 1)
            {
                roundedRatio = 1;
            }
            var valueColorScale = ((1.0f - roundedRatio) * (MinColorScaleFactor - MaxColorScaleFactor)) + MaxColorScaleFactor;
            return Intern(System.Drawing.ColorTranslator.ToHtml(ControlPaint.Light(PostitiveColor, valueColorScale)));
        }

        private string CalculateMapCellLocations(IModelFileAccessor modflowFileAccessor, int stressPeriod, List<MapLocationPositionCellValue> calculatedCellValues)
        {
            var grouped = calculatedCellValues.GroupBy(a => a.Color).Select(a => new MapLocationsPositionCellColor
            {
                Color = a.Key,
                Locations = a.Select(b => b.Location).ToList()
            }).ToList();

            return modflowFileAccessor.ReduceMapCells(stressPeriod, grouped);
        }

        private string GetStressPeriodLabel(int currentStressPeriod)
        {
            if (Model.ModelStressPeriodCustomStartDates != null && Model.ModelStressPeriodCustomStartDates.Any())
            {
                return Model.ModelStressPeriodCustomStartDates[currentStressPeriod].StressPeriodStartDate.ToString("MMMM yyyy");
            }
            
            return Model.StartDateTime.AddMonths(currentStressPeriod).ToString("MMMM yyyy");
        }

        #region KML
        public string GetKmlString(string mapPointsStr, List<LegendItem> legend, string name)
        {
            GeoJSON.Net.Feature.FeatureCollection mapPoints;

            try
            {
                mapPoints = JsonConvert.DeserializeObject<GeoJSON.Net.Feature.FeatureCollection>(mapPointsStr);
            }
            catch (Exception)
            {
                return null;
            }

            var document = new Document();
            document.Id = KmlDocumentName;
            document.Name = name;
            document.AddSchema(GetSchema());

            foreach (var feature in mapPoints.Features)
            {
                var hexColor = feature.Properties["color"].ToString();

                var waterDrawValue = Math.Round(GetWaterDrawValue(hexColor, legend), 6).ToString();

                var placemarkDescription = $"There was a water level change of {waterDrawValue} ft in this area.";
                var placemarkName = $"{waterDrawValue} ft";

                if (feature.Geometry.Type == GeoJSON.Net.GeoJSONObjectType.Polygon)
                {
                    var polygon = feature.Geometry as GeoJSON.Net.Geometry.Polygon;

                    document.AddFeature(CreatePlacemark(polygon, hexColor, feature.Properties, placemarkName, placemarkDescription, false));
                }
                else if (feature.Geometry.Type == GeoJSON.Net.GeoJSONObjectType.MultiPolygon)
                {
                    var multiPolygon = feature.Geometry as GeoJSON.Net.Geometry.MultiPolygon;

                    foreach (var polygon in multiPolygon.Coordinates)
                    {
                        document.AddFeature(CreatePlacemark(polygon, hexColor, feature.Properties, placemarkName, placemarkDescription, false));
                    }
                }
            }

            var kml = new Kml();
            kml.Feature = document;

            KmlFile kmlFile = KmlFile.Create(kml, true);

            var serializer = new Serializer();
            serializer.Serialize(kmlFile.Root);
            return serializer.Xml;
        }

        public string GetWaterLevelByZoneKmlString(string mapPointsStr, List<LegendItem> legend,
            List<WaterLevelChangeByZoneMapData> waterLevelByZoneMapData, bool isDifferential)
        {
            GeoJSON.Net.Feature.FeatureCollection mapPoints;

            try
            {
                mapPoints = JsonConvert.DeserializeObject<GeoJSON.Net.Feature.FeatureCollection>(mapPointsStr);
            }
            catch (Exception)
            {
                return null;
            }

            var document = new Document();
            document.Id = KmlDocumentName;
            document.Name = "Water Level By Zone";
            document.AddSchema(GetWaterLevelByZoneSchema());

            foreach (var feature in mapPoints.Features)
            {
                var hexColor = feature.Properties["color"].ToString();

                var zoneName = feature.Properties["zoneName"].ToString();

                var waterDrawValue = Math.Round(waterLevelByZoneMapData.Single(x => x.ZoneName == zoneName).Mean, 6).ToString();

                var placemarkDescription = $"The mean water level {(isDifferential ? "change" : "")} in {zoneName} was {waterDrawValue} ft.";
                var placemarkName = zoneName;

                if (feature.Geometry.Type == GeoJSON.Net.GeoJSONObjectType.Polygon)
                {
                    var polygon = feature.Geometry as GeoJSON.Net.Geometry.Polygon;

                    document.AddFeature(CreatePlacemark(polygon, hexColor, feature.Properties, placemarkName, placemarkDescription, true));
                }
                else if (feature.Geometry.Type == GeoJSON.Net.GeoJSONObjectType.MultiPolygon)
                {
                    var multiPolygon = feature.Geometry as GeoJSON.Net.Geometry.MultiPolygon;

                    foreach (var polygon in multiPolygon.Coordinates)
                    {
                        document.AddFeature(CreatePlacemark(polygon, hexColor, feature.Properties, placemarkName, placemarkDescription, true));
                    }
                }
            }

            var kml = new Kml();
            kml.Feature = document;

            KmlFile kmlFile = KmlFile.Create(kml, true);

            var serializer = new Serializer();
            serializer.Serialize(kmlFile.Root);
            return serializer.Xml;
        }

        private List<List<Vector>> CreateVectorListFromPolygon(GeoJSON.Net.Geometry.Polygon polygon)
        {
            var vectorsList = new List<List<Vector>>();

            foreach (var coordinates in polygon.Coordinates)
            {
                var vectors = ConvertCoordinatesToVectors(coordinates.Coordinates.ToList());

                vectorsList.Add(vectors);
            }

            return vectorsList;
        }

        private Style CreateStyle(string hexColor, bool hasOutline)
        {
            var style = new Style();

            style.Polygon = new PolygonStyle();
            style.Polygon.Fill = true;
            style.Polygon.Outline = hasOutline;
            style.Polygon.ColorMode = ColorMode.Normal;
            style.Polygon.Color = GetSharpKmlColorFromHex(hexColor);

            return style;
        }

        private Placemark CreatePlacemark(GeoJSON.Net.Geometry.Polygon initialPolygon, string hexColor, IDictionary<string, object> properties, string placemarkName, string placemarkDescription, bool addOutline )
        {
            var style = CreateStyle(hexColor, addOutline);

            var polygon = CreatePolygon(CreateVectorListFromPolygon(initialPolygon));

            Placemark placemark = new Placemark();
            placemark.Description = new Description { Text = placemarkDescription };
            placemark.Name = placemarkName;
            placemark.Geometry = polygon;
            placemark.AddStyle(style);

            var extendedData = new ExtendedData();
            extendedData.AddSchemaData(GetSchemaData(properties));
            placemark.ExtendedData = extendedData;

            return placemark;
        }

        private Polygon CreatePolygon(List<List<Vector>> vectors)
        {
            OuterBoundary outerBoundary = new OuterBoundary();
            outerBoundary.LinearRing = new LinearRing();
            outerBoundary.LinearRing.Coordinates = CreateCoordinateCollection(vectors[0]);

            var polygon = new SharpKml.Dom.Polygon();
            polygon.OuterBoundary = outerBoundary;
            //polygon.Extrude = true;           

            if (vectors.Count > 1)
            {
                for (var i = 1; i < vectors.Count; i++)
                {
                    InnerBoundary innerBoundary = new InnerBoundary();
                    innerBoundary.LinearRing = new LinearRing();
                    innerBoundary.LinearRing.Coordinates = CreateCoordinateCollection(vectors[i]);

                    polygon.AddInnerBoundary(innerBoundary);
                }
            }

            return polygon;
        }

        private CoordinateCollection CreateCoordinateCollection(List<Vector> vectors)
        {
            var collection = new CoordinateCollection();

            foreach (var vector in vectors)
            {
                collection.Add(vector);
            }

            return collection;
        }

        private Color32 GetSharpKmlColorFromHex(string hex)
        {
            if (IsNullOrWhiteSpace(hex))
            {
                return new Color32(0,0,0,0);
            }

            var color = ColorTranslator.FromHtml(hex);

            return new Color32(191, color.B, color.G, color.R);
        }

        private List<Vector> ConvertCoordinatesToVectors(List<GeoJSON.Net.Geometry.IPosition> coordinates)
        {
            var vectors = new List<Vector>();

            foreach (var c in coordinates)
            {
                vectors.Add(new Vector(c.Latitude, c.Longitude, c.Altitude ?? 0));
            }

            return vectors;
        }

        private Schema GetSchema()
        {
            var schema = new Schema
            {
                Name = KmlSchemaName,
                Id = KmlSchemaName
            };

            schema.AddField(new SimpleField { Name = "color", FieldType = "string" });
            schema.AddField(new SimpleField { Name = "stressPeriod", FieldType = "int" });

            return schema;
        }
        private Schema GetWaterLevelByZoneSchema()
        {
            var schema = new Schema
            {
                Name = KmlSchemaName,
                Id = KmlSchemaName
            };

            schema.AddField(new SimpleField { Name = "color", FieldType = "string" });
            schema.AddField(new SimpleField { Name = "zoneName", FieldType = "string" });

            return schema;
        }

        private SchemaData GetSchemaData(IDictionary<string, object> properties)
        {
            var schemaData = new SchemaData { SchemaUrl = new Uri($"#{KmlSchemaName}", UriKind.Relative) };

            foreach (var prop in properties)
            {
                schemaData.AddData(new SimpleData { Name = prop.Key, Text = prop.Value.ToString() });
            }

            return schemaData;
        }

        private double GetWaterDrawValue(string featureColor, List<LegendItem> legend)
        {
            var value = legend.FirstOrDefault(x => String.Equals(x.IncreaseColor, featureColor, StringComparison.InvariantCultureIgnoreCase))?.Value;
            if (value == null)
            {
                value = legend.FirstOrDefault(x => String.Equals(x.DecreaseColor, featureColor, StringComparison.InvariantCultureIgnoreCase))?.Value * -1;
            }

            return value ?? 0;
        }
        #endregion

        #region Validation helpers
        private bool StressPeriodsAreValid(List<StressPeriod> stressPeriods)
        {
            if (stressPeriods == null || !stressPeriods.Any())
            {
                Logger.Warn("Not generating heat map because no stress period data found.");
                return false;
            }
            return true;
        }

        private bool LocationsAreValid(List<string> locations)
        {
            if (locations == null || !locations.Any())
            {
                Logger.Debug("Not generating heat map because location position map was null or empty.");
                return false;
            }
            return true;
        }

        private bool MapDataIsValid(IEnumerable<MapOutputData> mapOutput)
        {
            if (mapOutput == null)
            {
                Logger.Debug("Not generating heat map because map output was null.");
                return false;
            }
            return true;
        }

        private bool MapDataIsValidForWaterLevelByZone(IEnumerable<MapOutputData> mapOutput, int numStressPeriods)
        {
            if (mapOutput == null)
            {
                Logger.Debug("Not generating heat map because map output was null.");
                return false;
            }

            if (mapOutput.Any(x => x.StressPeriod > numStressPeriods))
            {
                Logger.Debug("Not generating heat map because one or more map output cells have a greater number of stress periods than what the model has returned as its highest stress period");
                return false;
            }

            return true;
        }
        #endregion
    }
}
