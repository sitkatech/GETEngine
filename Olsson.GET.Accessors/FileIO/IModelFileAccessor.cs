using System.Collections.Generic;
using Olsson.GET.Common.DataContracts.Models;
using Olsson.GET.Common.DataContracts.Runs;

namespace Olsson.GET.Accessors.FileIO
{
    public interface IModelFileAccessor
    {
        List<StressPeriod> GetStressPeriodData();
        int GetNumberOfSegmentReaches();
        IEnumerable<OutputData> GetOutputData();
        IEnumerable<OutputData> GetBaselineData();
        List<string> GetSegmentReachZones(int segment, int reach);
        List<string> GetAllZones();
        StressPeriodsLocationRates GetLocationRates();
        void UpdateLocationRates(StressPeriodsLocationRates stressPeriods);
        List<LocationProportion> GetLocationProportions(string feature);
        IEnumerable<string> GetListFileOutputFileLines();
        IEnumerable<string> GetRunListFileLines();
        IEnumerable<string> GetBaselineListFileLines();
        string GetFriendlyInputZoneName(string zoneKey);
        string GetFriendlyZoneBudgetName(string zoneKey);
        List<string> GetLocationPositionMap();
        IEnumerable<MapOutputData> GetBaselineMapData();
        IEnumerable<MapOutputData> GetOutputMapData();
        IEnumerable<MapOutputData> GetDrawdownMapData();
        List<AsrDataMap> GetAsrDataNameMap();
        List<AsrDataMap> GetZoneBudgetAsrDataNameMap();
        string ReduceMapCells(int stressPeriod, List<MapLocationsPositionCellColor> colorsList);
        string CreateSerializedFeatureCollectionFromWaterLevelChangeByZoneMapData(List<WaterLevelChangeByZoneMapData> results);
        List<LocationPumpingProportion> FindWellLocations(double lat, double lng);
        LocationWithBounds FindLocationCell(double lat, double lng);
        List<Coordinate> FindCellBounds(string key);
        List<string> GetInputLocationZones(string location);
        List<string> GetOutputLocationZones(string location);
        bool OutputLocationZonesExists();
        IEnumerable<ZoneBudgetItem> GetBaselineZoneBudgetItems(List<AsrDataMap> asrData);
        IEnumerable<ZoneBudgetItem> GetRunZoneBudgetItems(List<AsrDataMap> asrData);
        IEnumerable<PointOfInterest> GetPointsOfInterest();
        string GetModpathListFileContent(string listFileName);
        string GetModpathListFileName(string simFileName);
        string GetModpathLocationFileName(string simFileName);
        string GetModpathTimeSeriesFileName(string simFileName);
        ModelSettings GetSettings();
        void WriteLocationFile(string fileName, string data);
        List<ModpathTimeSeries> GetModpathTimeSeriesResult(string fileName);
        IEnumerable<ObservedImpactToBaseflow> GetObservedImpactToBaseflow(bool isDifferential);
        IEnumerable<ObservedZoneBudgetData> GetObservedZoneBudget(bool isDifferential);
        IEnumerable<ObservedPointOfInterest> GetObservedPointsOfInterest(bool isDifferential);
    }
}