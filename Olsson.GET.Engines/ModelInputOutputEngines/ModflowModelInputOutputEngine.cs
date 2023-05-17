using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Olsson.GET.Accessors.FileIO;
using Olsson.GET.Common.DataContracts.Runs;
using Olsson.GET.Common.Utilities;
using Olsson.GET.Common.DataContracts.Models;
using Olsson.GET.Accessors.EntityFramework;
using Model = Olsson.GET.Common.DataContracts.Models.Model;
using Run = Olsson.GET.Common.DataContracts.Runs.Run;

namespace Olsson.GET.Engines.ModelInputOutputEngines
{
    internal class RelatedResultDetails
    {
        public string SetName { get; set; }
        public List<RunResultDetails> RelatedResults { get; set; }
    }

    internal enum MapLocationState
    {
        Normal,
        RanDry,
        WasDry,
        IsDry
    }

    public class ModflowModelInputOutputEngine : BaseInputOutputEngine, IModelInputOutputEngine
    {
        private static readonly ILog Logger = Logging.GetLogger(typeof(ModflowModelInputOutputEngine));

        public ModflowModelInputOutputEngine(Model model)
        {
            Model = model;
            AccessorFactory = new Accessors.AccessorFactory();
        }

        private Model Model { get; }

        private IPointsOfInterestOutputSubEngine _pointsOfInterestOutputSubEngine;
        internal IPointsOfInterestOutputSubEngine PointsOfInterestOutputSubEngine
        {
            get { return _pointsOfInterestOutputSubEngine ?? (_pointsOfInterestOutputSubEngine = new PointsOfInterestOutputSubEngine(Model)); }
            set { _pointsOfInterestOutputSubEngine = value; }
        }

        private IListFileOutputSubEngine _listFileOutputSubEngine;
        internal IListFileOutputSubEngine ListFileOutputSubEngine
        {
            get { return _listFileOutputSubEngine ?? (_listFileOutputSubEngine = new ListFileOutputSubEngine(Model)); }
            set { _listFileOutputSubEngine = value; }
        }

        private IImpactToBaseflowFileOutputSubEngine _impactToBaseflowFileOutputSubEngine;
        internal IImpactToBaseflowFileOutputSubEngine ImpactToBaseflowFileOutputSubEngine
        {
            get { return _impactToBaseflowFileOutputSubEngine ?? (_impactToBaseflowFileOutputSubEngine = new ImpactToBaseflowFileOutputSubEngine(Model)); }
            set { _impactToBaseflowFileOutputSubEngine = value; }
        }

        private ILocationMapOutputSubEngine _locationMapOutputSubEngine;
        internal ILocationMapOutputSubEngine LocationMapOutputSubEngine
        {
            get { return _locationMapOutputSubEngine ?? (_locationMapOutputSubEngine = new LocationMapOutputSubEngine(Model)); }
            set { _locationMapOutputSubEngine = value; }
        }

        private IZoneBudgetOutputSubEngine _zoneBudgetOutputSubEngine;
        internal IZoneBudgetOutputSubEngine ZoneBudgetOutputSubEngine
        {
            get { return _zoneBudgetOutputSubEngine ?? (_zoneBudgetOutputSubEngine = new ZoneBudgetOutputSubEngine(Model)); }
            set { _zoneBudgetOutputSubEngine = value; }
        }

        private ICanalCsvInputSubEngine _canalCsvInputSubEngine;
        internal ICanalCsvInputSubEngine CanalCsvInputSubEngine
        {
            get { return _canalCsvInputSubEngine ?? (_canalCsvInputSubEngine = new CanalCsvInputSubEngine(Model)); }
            set { _canalCsvInputSubEngine = value; }
        }

        private IAddWellMapInputSubEngine _addWellMapInputSubEngine;
        internal IAddWellMapInputSubEngine AddWellMapInputSubEngine
        {
            get { return _addWellMapInputSubEngine ?? (_addWellMapInputSubEngine = new AddWellMapInputSubEngine(Model)); }
            set { _addWellMapInputSubEngine = value; }
        }

        private IAdjustZoneInputSubEngine _adjustZoneInputSubEngine;
        internal IAdjustZoneInputSubEngine AdjustZoneInputSubEngine
        {
            get { return _adjustZoneInputSubEngine ?? (_adjustZoneInputSubEngine = new AdjustZoneInputSubEngine(Model)); }
            set { _adjustZoneInputSubEngine = value; }
        }

        public void GenerateInputFiles(Run run)
        {
            var modflowFileAccessor = AccessorFactory.CreateAccessor<IModelFileAccessorFactory>().CreateModflowFileAccessor(Model);
            var fileAccessor = AccessorFactory.CreateAccessor<IBlobFileAccessor>();
            var existingFlows = modflowFileAccessor.GetLocationRates();
            StressPeriodsLocationRates updatedFlows;
            if (run.Scenario.InputControlType == InputControlType.WellMap)
            {
                updatedFlows = AddWellMapInputSubEngine.UpdateFlowInputs(modflowFileAccessor, fileAccessor, existingFlows, run);
            }
            else if (run.Scenario.InputControlType == InputControlType.ZoneMap)
            {
                updatedFlows = AdjustZoneInputSubEngine.UpdateFlowInputs(modflowFileAccessor, fileAccessor, existingFlows, run);
            }
            else
            {
                updatedFlows = CanalCsvInputSubEngine.UpdateFlowInputs(modflowFileAccessor, fileAccessor, existingFlows, run);
            }
            modflowFileAccessor.UpdateLocationRates(updatedFlows);
        }

        public void GenerateOutputFiles(Run run)
        {
            var currResultId = 0;

            var fileAccessor = AccessorFactory.CreateAccessor<IBlobFileAccessor>();
            var modflowFileAccessor = AccessorFactory.CreateAccessor<IModelFileAccessorFactory>().CreateModflowFileAccessor(Model);

            var stressPeriods = modflowFileAccessor.GetStressPeriodData();
            if (stressPeriods == null || !stressPeriods.Any())
            {
                throw new Exception("Unable to find stress period data.");
            }

            try
            {
                CreateImpactsToBaseFlow(modflowFileAccessor, fileAccessor, run, stressPeriods, ref currResultId);
                CreatePointsOfInterestGraph(modflowFileAccessor, fileAccessor, run, stressPeriods, ref currResultId);
                CreateWaterLevelByZoneHeatMap(modflowFileAccessor, fileAccessor, run, stressPeriods, ref currResultId);
                CreateDrawdownHeatMap(modflowFileAccessor, fileAccessor, run, stressPeriods, ref currResultId);
                //This last result can throw an error that will set the run's status to 'Completed with Dry Cells', but will cause the exiting of the try block
                //Could be worth updating so that the function just updates the status, but at the very least ensure that this is the last function that runs in this block.
                CreateWaterLevelHeatMap(modflowFileAccessor, fileAccessor, run, stressPeriods, ref currResultId);
            }
            finally
            {
                try
                {
                    CreateZoneBudget(modflowFileAccessor, fileAccessor, run, stressPeriods, ref currResultId);
                }
                finally
                {
                    CreateListFile(modflowFileAccessor, fileAccessor, run, stressPeriods, currResultId);
                }
            }
        }

        private void CreatePointsOfInterestGraph(IModelFileAccessor modflowFileAccessor, IBlobFileAccessor fileAccessor, Run run, List<StressPeriod> stressPeriods, ref int currResultId)
        {
            var pointsOfInterestResults = PointsOfInterestOutputSubEngine.GeneratePointsOfInterestGraphOutput(modflowFileAccessor, stressPeriods, currResultId, run.IsDifferential);
            if (pointsOfInterestResults.Count != 0)
            {
                WriteRunResults(run, pointsOfInterestResults, ref currResultId, fileAccessor);
            }
        }

        private void CreateListFile(IModelFileAccessor modflowFileAccessor, IBlobFileAccessor fileAccessor, Run run, List<StressPeriod> stressPeriods, int currResultId)
        {
            var listFileData = ListFileOutputSubEngine.GenerateListFileOutput(modflowFileAccessor, stressPeriods, run.OutputVolumeUnitID, run.IsDifferential);
            if (listFileData.OutputResults != null)
            {
                WriteRunResults(run, listFileData.OutputResults, ref currResultId, fileAccessor);
            }
            if (listFileData.Exception != null)
            {
                throw listFileData.Exception;
            }
        }

        private void CreateZoneBudget(IModelFileAccessor modflowFileAccessor, IBlobFileAccessor fileAccessor, Run run, List<StressPeriod> stressPeriods, ref int currResultId)
        {
            var zoneBudgetResults = ZoneBudgetOutputSubEngine.CreateZoneBudgetOutputResults(modflowFileAccessor, stressPeriods, run.OutputVolumeUnitID, run.IsDifferential);
            if (zoneBudgetResults != null)
            {
                foreach (var result in zoneBudgetResults)
                {
                    WriteRelatedResults(run, result, fileAccessor, ref currResultId);
                }
            }
        }

        private void CreateWaterLevelHeatMap(IModelFileAccessor modflowFileAccessor, IBlobFileAccessor fileAccessor, Run run, List<StressPeriod> stressPeriods, ref int currResultId)
        {
            if (run.ShouldCreateMaps)
            {
                var heatMapResults = LocationMapOutputSubEngine.CreateWaterLevelHeatMapResults(modflowFileAccessor, stressPeriods, run.IsDifferential);
                if (heatMapResults.OutputResults != null)
                {
                    WriteRelatedMapResults(run, heatMapResults.OutputResults, fileAccessor, ref currResultId);
                }
                if (heatMapResults.Exception != null)
                {
                    throw heatMapResults.Exception;
                }
            }
        }

        private void CreateWaterLevelByZoneHeatMap(IModelFileAccessor modflowFileAccessor, IBlobFileAccessor fileAccessor, Run run, List<StressPeriod> stressPeriods, ref int currResultId)
        {
            if (run.ShouldCreateMaps)
            {
                var waterLevelByZoneResults = LocationMapOutputSubEngine.CreateWaterLevelByZoneHeatMapResults(modflowFileAccessor, stressPeriods, run.IsDifferential, run.Model.OutputZoneData);
                if (waterLevelByZoneResults != null)
                {
                    WriteRelatedMapResults(run, waterLevelByZoneResults, fileAccessor, ref currResultId);
                }
                
            }
        }

        private void CreateDrawdownHeatMap(IModelFileAccessor modflowFileAccessor, IBlobFileAccessor fileAccessor, Run run, List<StressPeriod> stressPeriods, ref int currResultId)
        {
            if (run.ShouldCreateMaps)
            {
                var heatMapResults = LocationMapOutputSubEngine.CreateDrawdownHeatMapResults(modflowFileAccessor, stressPeriods, run.IsDifferential);
                if (heatMapResults != null)
                {
                    WriteRelatedMapResults(run, heatMapResults, fileAccessor, ref currResultId);
                }
            }
        }

        private void CreateImpactsToBaseFlow(IModelFileAccessor modflowFileAccessor, IBlobFileAccessor fileAccessor, Run run, List<StressPeriod> stressPeriods, ref int currResultId)
        {
            if (run.Model.BaseflowTableProcessingConfigurationID.HasValue)
            {
                var baseFlowResults = ImpactToBaseflowFileOutputSubEngine.CalculateImpactToBaseflow(modflowFileAccessor, stressPeriods, run.OutputVolumeUnitID, run.IsDifferential);
                if (baseFlowResults.Count != 0)
                {
                    WriteRunResults(run, baseFlowResults, ref currResultId, fileAccessor);
                }
            }
        }

        private static void WriteRunResults(Run run, List<RunResultDetails> impactToBaseflowData, ref int currResultId, IBlobFileAccessor fileAccessor)
        {
            foreach (var result in impactToBaseflowData)
            {
                result.RunResultId = ++currResultId;
                WriteOutputFile(run, fileAccessor, result);
            }
        }

        private static void WriteRelatedMapResults(Run run, RelatedResultDetails results, IBlobFileAccessor fileAccessor, ref int currResultId)
        {
            WriteRelatedResults(run, results, fileAccessor, (a, b) => a.ResultSets[0].MapData.AvailableStressPeriods = b, ref currResultId);
        }

        private static void WriteRelatedResults(Run run, RelatedResultDetails results, IBlobFileAccessor fileAccessor, ref int currResultId)
        {
            WriteRelatedResults(run, results, fileAccessor, (a, b) => a.RelatedResultOptions = b, ref currResultId);
        }

        private static void WriteRelatedResults(Run run, RelatedResultDetails results, IBlobFileAccessor fileAccessor, Action<RunResultDetails, List<ResultOption>> setRelatedResultsAction, ref int currResultId)
        {
            if (results?.RelatedResults != null)
            {
                foreach (var result in results.RelatedResults)
                {
                    result.RunResultId = ++currResultId;
                }
                var isFirst = true;
                foreach (var result in results.RelatedResults)
                {
                    setRelatedResultsAction(result, results.RelatedResults.Select(a => new ResultOption { Label = a.RunResultName, Id = a.RunResultId }).ToList());
                    if (isFirst)
                    {
                        WriteOutputFile(run, fileAccessor, result, false, results.SetName);
                        isFirst = false;
                    }
                    else
                    {
                        WriteOutputFile(run, fileAccessor, result, true, result.RunResultName);
                    }

                    if (result.ResultSets != null &&
                           result.ResultSets.Count > 0 &&
                           result.ResultSets[0].MapData != null &&
                           !string.IsNullOrEmpty(result.ResultSets[0].MapData.KmlString))
                    {
                        WriteKmlFile(run, fileAccessor, result, true, result.RunResultName);
                    }
                }
            }
        }

        internal static RunResultSet CalculateCumulativeFromMonthly(RunResultDetails item, VolumeUnitEnum outputVolumeUnitEnum)
        {
            return new RunResultSet
            {
                Name = "Cumulative",
                DataType =  VolumeUnit.ToType(outputVolumeUnitEnum).VolumeUnitDisplayName,
                DataSeries = item.ResultSets[0].DataSeries.Select(a =>
                {
                    var depletion = 0.0;
                    return new DataSeries
                    {
                        Name = a.Name,
                        IsDefaultDisplayed = a.IsDefaultDisplayed,
                        DataPoints = a.DataPoints.Select(b => new RunResultSetDataPoint
                        {
                            Date = b.Date,
                            Value = depletion += b.Value
                        }).ToList(),
                        IsObserved = false
                    };
                }).ToList()
            };
        }
    }
}