using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;
using Olsson.GET.Common.DataContracts.Models;

namespace Olsson.GET.Common.DataContracts.Runs
{
    [DataContract]
    public class RunResultDetails
    {
        [DataMember]
        public int RunResultId { get; set; }

        [DataMember]
        public string RunResultName { get; set; }

        public string BaseResultTypeName { get; set; }

        [DataMember]
        public List<RunResultSet> ResultSets { get; set; }

        [DataMember]
        public List<ResultOption> RelatedResultOptions { get; set; }

        [DataMember]
        public string Version { get; set; }
    }

    [DataContract]
    public class RunResultSet
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public RunResultDisplayType DisplayType { get; set; }

        [DataMember]
        public string DataType { get; set; }

        [DataMember]
        public List<DataSeries> DataSeries { get; set; }

        [DataMember]
        public TextDisplay TextDisplay { get; set; }

        [DataMember]
        public MapData MapData { get; set; }

        [DataMember]
        public List<WaterLevelChangeByZone> WaterLevelChangeByZones { get; set; }
    }

    public enum RunResultDisplayType
    {
        LineChart = 0,
        StackedBarChart = 1,
        Text = 2,
        Map = 3
    }

    [DataContract]
    public class TextDisplay
    {
        [DataMember]
        public string Text { get; set; }

        [DataMember]
        public string FileName { get; set; }
    }

    [DataContract]
    public class DataSeries
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public bool IsDefaultDisplayed { get; set; }

        [DataMember]
        public List<RunResultSetDataPoint> DataPoints { get; set; }
        [DataMember]
        public bool IsObserved { get; set; }
        [DataMember]
        public string TestProperty { get; set; }
    }

    [DataContract]
    public class RunResultSetDataPoint
    {
        [DataMember]
        public DateTime Date { get; set; }

        [DataMember]
        public double Value { get; set; }
    }

    [DataContract]
    public class MapData
    {
        [DataMember]
        public int CurrentStressPeriod { get; set; }

        [DataMember]
        public List<ResultOption> AvailableStressPeriods { get; set; }

        [DataMember]
        public string MapPoints { get; set; }

        [DataMember]
        public List<LegendItem> Legend { get; set; }

        [DataMember]
        public bool ContainsKmlFile { get; set; }

        [DataMember]
        public string KmlString { get; set; }
    }

    [DataContract]
    public class LegendItem
    {
        [DataMember]
        public string IncreaseColor { get; set; }

        [DataMember]
        public string DecreaseColor { get; set; }

        [DataMember]
        public double Value { get; set; }

        [DataMember]
        public string Text { get; set; }
    }

    [DataContract]
    public class MapCell
    {
        [DataMember]
        public string Color { get; set; }

        [DataMember]
        public string WellKnownText { get; set; }
    }

    public class MapLocationsPositionCellColor
    {
        public List<string> Locations { get; set; }
        public string Color { get; set; }
    }

    [DataContract]
    public class GeographicPoint
    {
        [DataMember]
        public double Latitude { get; set; }

        [DataMember]
        public double Longitude { get; set; }
    }

    [DataContract]
    public class ResultOption
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string Label { get; set; }
    }

    [DataContract]
    public class WaterLevelChangeByZone
    {
        [DataMember]
        public string ZoneName { get; set; }
        [DataMember]
        public string ZoneNumber { get; set; }
        [DataMember]
        public double Maximum { get; set; }
        [DataMember]
        public double Minimum { get; set; }
        [DataMember]
        public double Mean { get; set; }
    }

    [DataContract]
    public class WaterLevelChangeByZoneMapData
    {
        [DataMember]
        public string ZoneName { get; set; }
        [DataMember]
        public Coordinate[] Bounds { get; set; }
        [DataMember]
        public double Mean { get; set; }
        [DataMember]
        public string Color { get; set; }
    }
}
