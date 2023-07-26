using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.SqlServer.Types;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using Newtonsoft.Json;
using Olsson.GET.Accessors.ExtensionMethods;
using Olsson.GET.Common.DataContracts.Models;
using Olsson.GET.Common.DataContracts.Runs;
using Olsson.GET.Common.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Olsson.GET.Accessors.FileIO
{
    public enum FileFormat
    {
        Undefined,
        FixedWidth,
        Delimited,
        ModflowSixStructured,
        ModflowSixUnstructured
    }
    public enum FileStructure
    {
        Undefined,
        Structured,
        Unstructured,
        ModflowSixStructured,
        ModflowSixUnstructured
    }

    public abstract class ModelFileAccessor : IModelFileAccessor
    {
        protected ModelFileAccessor(Model model)
        {
            Model = model;
            ModflowFileNamesLazy = new Lazy<Dictionary<string, List<string>>>(CreateModflowFileNamesDictionary);
            SegmentReachZoneDictionaryLazy = new Lazy<Dictionary<Tuple<int, int>, List<string>>>(CreateSegmentReachZoneDictionary);
            FriendlyInputZoneNamesDictionaryLazy = new Lazy<Dictionary<string, string>>(CreateFriendlyInputZoneNamesDictionary);
            LocationFlowProportionsLazy = new Lazy<Dictionary<string, List<LocationProportion>>>(CreateLocationFlowProportionsDictionary);
            LocationMapPositionsLazy = new Lazy<Dictionary<string, (SqlGeography Geography, List<LocationPumpingProportion> LocationPumpingProportions)>>(CreateLocationPositionMap);
            InputLocationZoneDictionaryLazy = new Lazy<Dictionary<string, List<string>>>(CreateInputLocationZoneDictionary);
            OutputLocationZoneDictionaryLazy = new Lazy<Dictionary<string, List<string>>>(CreateOutputLocationZoneDictionary);
            DryLocationIndicatorLazy = new Lazy<double>(GetDryLocationIndicator);
            FriendlyZoneBudgetNamesDictionaryLazy = new Lazy<Dictionary<string, string>>(CreateFriendlyZoneBudgetNamesDictionary);
            FileFormatter = new AccessorFactory().CreateAccessor<IModelFileAccessorFactory>().CreateFileFormatterAccessor(this);
        }

        private const string SfrFileKey = "SFR";
        protected const string UnstructuredDisFileKey = "DISU";
        protected const string StructuredDisFileKey = "DIS";
        protected const string UnstructuredModFlow6DisFileKey = "DISU6";
        protected const string StructuredModFlow6DisFileKey = "DIS6";
        private const string WelFileKey = "WEL";
        private const string ListFileKey = "LIST";
        private const string LpfFileKey = "LPF";
        private const string UpwFileKey = "UPW";
        private const string BcfFileKey = "BCF";
        private const string HufFileKey = "HUF2";

        protected const string BaselineListFileName = "Baseline.lst";
        private const string BaselineFileName = "Baseline.dat";
        private const string ObservedBaseFlowFileName = "Observed.ImpactToBaseflow.NonDifferential.csv";
        private const string ObservedBaseFlowDifferentialFileName = "Observed.ImpactToBaseflow.Differential.csv";
        private const string ObservedZoneBudgetFileName = "Observed.ZoneBudget.NonDifferential.csv";
        private const string ObservedZoneBudgetDifferentialFileName = "Observed.ZoneBudget.Differential.csv";
        private const string ObservedPointsOfInterestFileName = "Observed.PointsOfInterest.NonDifferential.csv";
        private const string ObservedPointsOfInterestDifferentialFileName = "Observed.PointsOfInterest.Differential.csv";
        protected const string BaselineMapFileName = "Baseline.hds";
        protected const string BaselineZoneBudgetFileName = "BaselineZoneBudget.csv";
        private const string AsrDataNameMapFileName = "AsrDataNameMap.csv";
        private const string ZoneBudgetAsrDataNameMapFileName = "AsrDataNameMap.ZoneBudget.csv";
        protected const string ZonesFileName = "SegRchZones.csv";
        private static readonly string[] FriendlyInputZoneNamesPossibleFileNames = {
            "InputZoneNames.csv", "ZoneNames.csv"
        };
        private const string FriendlyZoneBudgetNamesFileName = "ZoneBudgetNames.csv";
        private const string LocationFlowProportionsFileName = "LocationFlowProportions.csv";
        private const string LocationMapCoordinatesFileName = "LocationMapCoordinates.csv";
        private static readonly string[] LocationZonePossibleFileNames = {"InputLocationZones.csv", "LocationZones.csv"};
        private const string OutputLocationZoneFileName = "OutputLocationZones.csv";
        private const string RunZoneBudgetFileName = "Scenario.2.csv";
        private const string PointsOfInterestFileName = "PointsOfInterest.csv";
        private const string SettingsFileName = "settings.txt";

        protected IFileFormatter FileFormatter { get; set; }

        public ModelSettings GetSettings()
        {
            var settingsFilePath = System.IO.Path.Combine(ConfigurationHelper.AppSettings.ModflowDataFolder, SettingsFileName);

            if (File.Exists(settingsFilePath))
            {
                return JsonConvert.DeserializeObject<ModelSettings>(File.ReadAllText(settingsFilePath));
            }

            return null;
        }

        private static ILogger Logger = Logging.GetLogger<ModelFileAccessor>();
        internal static bool IsStructuredFile(Model model)
        {
            var namFileName = model.ModelExecutables.OrderBy(x => x.RunOrder).First().Arguments;
            var files = CreateModflowFileNamesDictionary(namFileName)
                .Union(ModflowSixFileAccessor.CreateModflowFileNamesDictionary(namFileName));
            foreach (var file in files)
            {
                if (file.Key == UnstructuredDisFileKey || file.Key == UnstructuredModFlow6DisFileKey)
                {
                    return false;
                }
                if (file.Key == StructuredDisFileKey || file.Key == StructuredModFlow6DisFileKey)
                {
                    return true;
                }
            }
            throw new Exception("Unable to determine if this is a structured or unstructured model.");
        }

        internal FileFormat FileFormat
        {
            get
            {
                var settings = GetSettings();
                if (settings != null)
                {
                    return settings.FileFormat;
                }

                //defaults
                if (FileStructure == FileStructure.Structured)
                {
                    return FileFormat.FixedWidth;
                }
                else if (FileStructure == FileStructure.Unstructured)
                {
                    return FileFormat.Delimited;
                }
                else if (FileStructure == FileStructure.ModflowSixStructured)
                {
                    return FileFormat.ModflowSixStructured;
                }
                else if (FileStructure == FileStructure.ModflowSixUnstructured)
                {
                    return FileFormat.ModflowSixUnstructured;
                }

                throw new Exception("Unsupported file format type");
            }
        }

        internal FileStructure FileStructure
        {
            get
            {
                if (this.GetType() == typeof(StructuredModflowFileAccessor))
                {
                    return FileStructure.Structured;
                }
                else if (this.GetType() == typeof(UnstructuredModflowFileAccessor))
                {
                    return FileStructure.Unstructured;
                }
                else if (this.GetType() == typeof(StructuredModflowSixFileAccessor))
                {
                    return FileStructure.ModflowSixStructured;
                }
                else if (this.GetType() == typeof(UnstructuredModflowSixFileAccessor))
                {
                    return FileStructure.ModflowSixUnstructured;
                }

                throw new Exception("Unsupported file structure type");
            }
        }

        protected Lazy<Dictionary<string, List<string>>> ModflowFileNamesLazy { get; }
        protected Lazy<Dictionary<Tuple<int, int>, List<string>>> SegmentReachZoneDictionaryLazy { get; }
        private Lazy<Dictionary<string, List<LocationProportion>>> LocationFlowProportionsLazy { get; }
        private Lazy<Dictionary<string, string>> FriendlyInputZoneNamesDictionaryLazy { get; }
        private Lazy<Dictionary<string, string>> FriendlyZoneBudgetNamesDictionaryLazy { get; }
        private static readonly Regex FileNameRegEx = new Regex(@"^\s*(?<key>(\S+?))(6?)\s.*?(?<fileName>\S+)$"); //(6?)
        private Lazy<Dictionary<string, (SqlGeography Geography, List<LocationPumpingProportion> LocationPumpingProportions)>> LocationMapPositionsLazy { get; }
        protected string GetFileName(string key)
        {
            if (ModflowFileNamesLazy.Value.ContainsKey(key))
            {
                return ModflowFileNamesLazy.Value[key].FirstOrDefault();
            }
            return null;
        }
        protected Lazy<Dictionary<string, List<string>>> InputLocationZoneDictionaryLazy { get; }
        public Lazy<Dictionary<string, List<string>>> OutputLocationZoneDictionaryLazy { get; set; }
        protected Lazy<double> DryLocationIndicatorLazy { get; }

        private class FriendlyZoneName
        {
            public string ZoneNumber { get; set; }
            public string ZoneName { get; set; }
        }

        internal class SegmentReachZone
        {
            public int Seg { get; set; }
            public int Rch { get; set; }
            public string Zone { get; set; }
        }

        public virtual List<string> GetSegmentReachZones(int segment, int reach)
        {
            var key = Tuple.Create(segment, reach);
            if (SegmentReachZoneDictionaryLazy.Value.ContainsKey(key) && SegmentReachZoneDictionaryLazy.Value[key] != null)
            {
                return SegmentReachZoneDictionaryLazy.Value[key];
            }
            return new List<string>();
        }

        public List<string> GetAllZones()
        {
            return SegmentReachZoneDictionaryLazy.Value.Select(a => a.Value).SelectMany(a => a).Distinct().ToList();
        }

        protected virtual Dictionary<Tuple<int, int>, List<string>> CreateSegmentReachZoneDictionary()
        {
            var result = new Dictionary<Tuple<int, int>, List<string>>();
            if (FileExists(ZonesFileName))
            {
                var reader = new CsvReader(GetFileData(ZonesFileName));
                reader.Read();
                reader.ReadHeader();
                while (reader.Read())
                {
                    var record = reader.GetRecord<SegmentReachZone>();
                    var key = Tuple.Create(record.Seg, record.Rch);
                    if (!result.ContainsKey(key))
                    {
                        result[key] = new List<string>();
                    }
                    if (!result[key].Contains(record.Zone))
                    {
                        result[key].Add(record.Zone);
                    }
                }
            }
            return result;
        }

        private Dictionary<string, string> CreateFriendlyInputZoneNamesDictionary()
        {
            var fileName = "";
            foreach (var file in FriendlyInputZoneNamesPossibleFileNames)
            {
                if (FileExists(file))
                {
                    fileName = file;
                    break;
                }
            }

            return !string.IsNullOrEmpty(fileName) ? CreateFriendlyZoneNamesDictionary(fileName) : new Dictionary<string, string>();
        }

        private Dictionary<string, string> CreateFriendlyZoneNamesDictionary(string fileName)
        {
            var result = new Dictionary<string, string>();
            if (FileExists(fileName))
            {
                var reader = new CsvReader(GetFileData(fileName));
                reader.Read();
                reader.ReadHeader();
                while (reader.Read())
                {
                    var record = reader.GetRecord<FriendlyZoneName>();
                    result[record.ZoneNumber] = record.ZoneName;
                }
            }
            return result;
        }

        private Dictionary<string, string> CreateFriendlyZoneBudgetNamesDictionary()
        {
            return CreateFriendlyZoneNamesDictionary(FriendlyZoneBudgetNamesFileName);
        }

        private class CaseInsensitiveStringComparer : IEqualityComparer<string>
        {
            public bool Equals(string x, string y)
            {
                return string.Equals(x, y, StringComparison.OrdinalIgnoreCase);
            }

            public int GetHashCode(string obj)
            {
                return obj?.ToUpper().GetHashCode() ?? 0;
            }
        }

        protected abstract string DisFileKey { get; }

        protected abstract Type LocationProportionMapperType { get; }

        private Dictionary<string, List<LocationProportion>> CreateLocationFlowProportionsDictionary()
        {
            var result = new Dictionary<string, List<LocationProportion>>(new CaseInsensitiveStringComparer());
            using (var reader = new CsvReader(GetFileData(LocationFlowProportionsFileName)))
            {
                reader.Configuration.RegisterClassMap(LocationProportionMapperType);
                reader.Read();
                reader.ReadHeader();
                while (reader.Read())
                {
                    var record = reader.GetRecord<LocationProportionData>();
                    foreach (var proportion in record.Proportions.Where(a => a.Proportion != 0.0))
                    {
                        if (!result.ContainsKey(proportion.FeatureName))
                        {
                            result[proportion.FeatureName] = new List<LocationProportion>();
                        }
                        result[proportion.FeatureName].Add(new LocationProportion
                        {
                            Location = record.Location,
                            Proportion = proportion.Proportion,
                            IsClnWell = record.IsClnWell
                        });
                    }
                }
                return result;
            }
        }

        internal class LocationProportionMapper : ClassMap<LocationProportionData>
        {
            public LocationProportionMapper(string[] mappedColumns)
            {
                Map(m => m.IsClnWell).ConvertUsing(r =>
                {
                    var row = (CsvHelper.CsvReader)r;
                    if (row.Context.HeaderRecord.Any(a => string.Equals("CLN", a)))
                    {
                        return row.GetField("CLN") == "1";
                    }
                    return false;
                });
                var allMappedColumns = mappedColumns.Concat(new[] { "CLN" });
                //Adding a column here? add it to the mappedColumns array below
                Map(m => m.Proportions).ConvertUsing(r =>
                {
                    var row = (CsvHelper.CsvReader)r;

                    //any column outside our expected values is treated as canal
                    //wish we could programatically check which columns are already mapped, couldn't figure it out
                    string[] columnsInFileNotMapped = row.Context.HeaderRecord.Where(f => !allMappedColumns.Contains(f)).ToArray();

                    var values = new List<LocationProportionValue>();

                    foreach (var feature in columnsInFileNotMapped)
                    {
                        //if we have a value and it's parsable to an int add it.
                        if (row.TryGetField(feature, out double value))
                        {
                            values.Add(new LocationProportionValue()
                            {
                                FeatureName = feature,
                                Proportion = value,
                            });
                        }
                        else
                        {
                            if (!string.IsNullOrWhiteSpace(row.GetField(feature))) // not null, not an int, blow up
                            {
                                throw new CsvHelperException(r.Context, $"Error on Row {row}: Unable to read value for column {feature}.");
                            }
                        }
                    }

                    return values;
                });
            }
        }

        internal class LocationProportionData
        {
            public string Location { get; set; }
            public bool IsClnWell { get; set; }
            public List<LocationProportionValue> Proportions { get; set; }
        }

        internal class LocationProportionValue
        {
            public string FeatureName { get; set; }
            public double Proportion { get; set; }
        }

        protected virtual Dictionary<string, List<string>> CreateModflowFileNamesDictionary()
        {
            var result = new Dictionary<string, List<string>>();
            var fileLines = GetModelFileLines(Model.ModelExecutables.OrderBy(x => x.RunOrder).First().Arguments);

            foreach (var fileLine in fileLines)
            {
                var match = FileNameRegEx.Match(fileLine);
                if (match.Success)
                {
                    var key = match.Groups["key"].Value;
                    var fileName = match.Groups["fileName"].Value;
                    if (!result.ContainsKey(key))
                    {
                        result.Add(key, new List<string>());
                    }
                    result[key].Add(fileName);
                }
            }

            return result;
        }

        protected static Dictionary<string, List<string>> CreateModflowFileNamesDictionary(string namFileName)
        {
            var result = new Dictionary<string, List<string>>();
            var fileLines = GetModelFileLines(namFileName);

            foreach (var fileLine in fileLines)
            {
                var match = FileNameRegEx.Match(fileLine);
                if (match.Success)
                {
                    var key = match.Groups["key"].Value;
                    var fileName = match.Groups["fileName"].Value;
                    if (!result.ContainsKey(key))
                    {
                        result.Add(key, new List<string>());
                    }
                    result[key].Add(fileName);
                }
            }

            return result;
        }

        public virtual int GetNumberOfSegmentReaches()
        {
            var lines = GetModelFileLines(GetFileName(SfrFileKey));
            int i = 0;
            bool result = int.TryParse(GetColumnLineData(lines.First())[0], out i);
            if (result == true)
            {
                return Math.Abs(int.Parse(GetColumnLineData(lines.First())[0]));
            }
            else
            {
                return Math.Abs(int.Parse(GetColumnLineData(lines.Skip(1).First())[0]));
            }
        }

        protected abstract int NumberOfStressPeriodsColumnInDisFileIndex { get; }
        protected abstract int FlowToAquiferColumnInOutputIndex { get; }
        protected abstract int SegmentNumberColumnInOutputIndex { get; }
        protected abstract int ReachNumberColumnInOutputIndex { get; }

        private const int NumberOfDaysInStressPeriodColumnInDisFileIndex = 0;
        private const int NumberOfTimeStepsInStressPeriodColumnInDisFileIndex = 1;

        public virtual List<StressPeriod> GetStressPeriodData()
        {
            var disFileName = GetFileName(DisFileKey);
            if (!FileExists(disFileName))
            {
                return new List<StressPeriod>();
            }
            var lines = GetModelFileLines(disFileName);
            int? numberOfStressPeriods = null;
            var buffer = new Queue<string>();
            foreach (var line in lines)
            {
                if (numberOfStressPeriods == null)
                {
                    //this is the first line
                    var splitFirstLine = GetColumnLineData(line);
                    numberOfStressPeriods = int.Parse(splitFirstLine[NumberOfStressPeriodsColumnInDisFileIndex]);
                }
                else if (!string.IsNullOrWhiteSpace(line))
                {
                    if (buffer.Count >= numberOfStressPeriods.Value)
                        buffer.Dequeue();

                    buffer.Enqueue(line.Trim());
                }
            }
            var stressDataLines = buffer.ToArray();
            if (stressDataLines.Length < numberOfStressPeriods)
            {
                throw new Exception("Not enough stress period data lines.");
            }
            if (stressDataLines.Length > numberOfStressPeriods)
            {
                throw new Exception("Too many stress period data lines.");
            }
            return buffer.ToArray()
                .Select(x => GetColumnLineData(x))
                .Select(a => new StressPeriod
                {
                    Days = double.Parse(a[NumberOfDaysInStressPeriodColumnInDisFileIndex]),
                    NumberOfTimeSteps = int.Parse(a[NumberOfTimeStepsInStressPeriodColumnInDisFileIndex])
                }).ToList();
        }

        protected string[] GetColumnLineData(string line, char separator = ' ')
        {
            return line.Trim().Split(new[] { separator }, StringSplitOptions.RemoveEmptyEntries);
        }

        protected static IEnumerable<string> GetModelFileLines(string fileName)
        {
            return System.IO.File.ReadLines(System.IO.Path.Combine(ConfigurationHelper.AppSettings.ModflowDataFolder, fileName)).SkipWhile(a => a.StartsWith("#"));
        }

        protected static string GetModelFile(string fileName)
        {
            return System.IO.File.ReadAllText(System.IO.Path.Combine(ConfigurationHelper.AppSettings.ModflowDataFolder, fileName));
        }

        public virtual IEnumerable<OutputData> GetOutputData()
        {
            return GetProcessedData(Model.RunFileName);
        }

        public virtual IEnumerable<OutputData> GetBaselineData()
        {
            return GetProcessedData(BaselineFileName);
        }

        public IEnumerable<ObservedImpactToBaseflow> GetObservedImpactToBaseflow(bool isDifferential)
        {
            var fileName = isDifferential ? ObservedBaseFlowDifferentialFileName : ObservedBaseFlowFileName;
            if (string.IsNullOrWhiteSpace(fileName) || !FileExists(fileName))
            {
                return null;
            }
            return GetModelFileLines(fileName).Select(GetObservedImpactToBaseFlowData).Where(a => a != null);
        }

        public IEnumerable<ObservedZoneBudgetData> GetObservedZoneBudget(bool isDifferential)
        {
            var fileName = isDifferential ? ObservedZoneBudgetDifferentialFileName : ObservedZoneBudgetFileName;
            if (string.IsNullOrWhiteSpace(fileName) || !FileExists(fileName))
            {
                return null;
            }
            return GetModelFileLines(fileName).Select(GetObservedZoneBudgetData).Where(a => a != null);
        }

        public IEnumerable<ObservedPointOfInterest> GetObservedPointsOfInterest(bool isDifferential)
        {
            var fileName = isDifferential ? ObservedPointsOfInterestDifferentialFileName : ObservedPointsOfInterestFileName;
            if (string.IsNullOrWhiteSpace(fileName) || !FileExists(fileName))
            {
                return null;
            }
            return GetModelFileLines(fileName).Select(GetObservedPointOfInterestData).Where(a => a != null);
        }

        private IEnumerable<OutputData> GetProcessedData(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName) || !FileExists(fileName) || !Model.BaseflowTableProcessingConfiguration.ReachColumnNum.HasValue)
            {
                return null;
            }

            var regexForBaseflowTableIndicator = new Regex($@"{Model.BaseflowTableProcessingConfiguration.BaseflowTableIndicatorRegexPattern}");

            var fileLines = GetModelFileLines(fileName);
            var result = new List<OutputData>();

            using (var fileLineEnumerator = fileLines.GetEnumerator())
            {
                while (fileLineEnumerator.MoveNext())
                {
                    var match = regexForBaseflowTableIndicator.Match(fileLineEnumerator.Current);
                    if (match.Success)
                    {
                        AddLocationFlowRate(fileLineEnumerator, result);
                    }
                }

                return result;
            }
        }

        internal TextReader GetFileData(string fileName)
        {
            return new StreamReader(GetFileStream(fileName));
        }

        private Stream GetFileStream(string fileName)
        {
            var path = System.IO.Path.Combine(ConfigurationHelper.AppSettings.ModflowDataFolder, fileName);
            return new System.IO.FileStream(path, FileMode.Open, FileAccess.Read);
        }

        internal bool FileExists(string fileName)
        {
            var path = System.IO.Path.Combine(ConfigurationHelper.AppSettings.ModflowDataFolder, fileName);
            return System.IO.File.Exists(path);
        }
        private void AddLocationFlowRate(IEnumerator<string> fileLineEnumerator, List<OutputData> outputData)
        {
            for (var i = 0; i < 4; i++)
            {
                fileLineEnumerator.MoveNext();
            }

            var segmentColumnNum = Model.BaseflowTableProcessingConfiguration.SegmentColumnNum - 1;
            var flowToAquiferColumnNum = Model.BaseflowTableProcessingConfiguration.FlowToAquiferColumnNum - 1;
            var reachColumnNum = Model.BaseflowTableProcessingConfiguration.ReachColumnNum.Value - 1;

            while (fileLineEnumerator.MoveNext())
            {
                if (string.IsNullOrWhiteSpace(fileLineEnumerator.Current))
                {
                    break;
                }

                var data = fileLineEnumerator.Current.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                outputData.Add(new OutputData
                {
                    ReachNumber = int.Parse(data[reachColumnNum]),
                    SegmentNumber = int.Parse(data[segmentColumnNum]),
                    FlowToAquifer = double.Parse(data[flowToAquiferColumnNum])
                });
            }
        }

        private ObservedImpactToBaseflow GetObservedImpactToBaseFlowData(string line)
        {
            var numberOfColumns = 3;
            var dataSeriesNameColumn = 0;
            var periodColumn = 1;
            var valueColumn = 2;

            // Using pipe delimiter so that the data series may include commas and spaces if desired
            var columns = GetColumnLineData(line, '|');
            if (columns.Length != numberOfColumns)
            {
                return null;
            }
            if (!int.TryParse(columns[periodColumn], out var period))
            {
                return null;
            }
            if (!double.TryParse(columns[valueColumn], out var value))
            {
                return null;
            }
            return new ObservedImpactToBaseflow
            {
                DataSeriesName = columns[dataSeriesNameColumn],
                Period = period,
                FlowToAquiferInAcreFeet = value,
            };
        }

        private ObservedZoneBudgetData GetObservedZoneBudgetData(string line)
        {
            var numberOfColumns = 4;
            var zoneColumn = 0;
            var budgetItemColumn = 1;
            var periodColumn = 2;
            var valueColumn = 3;

            // Using pipe delimiter so that the data series may include commas and spaces if desired
            var columns = GetColumnLineData(line, '|');
            if (columns.Length != numberOfColumns)
            {
                return null;
            }
            if (!int.TryParse(columns[periodColumn], out var period))
            {
                return null;
            }
            if (!double.TryParse(columns[valueColumn], out var value))
            {
                return null;
            }
            return new ObservedZoneBudgetData
            {
                BudgetItemSeriesName = columns[budgetItemColumn],
                ZoneSeriesName = columns[zoneColumn],
                Period = period,
                ValueInAcreFeet = value,
            };
        }

        private ObservedPointOfInterest GetObservedPointOfInterestData(string line)
        {
            var numberOfColumns = 3;
            var locationColumn = 0;
            var periodColumn = 1;
            var valueColumn = 2;

            // Using pipe delimiter so that the data series may include commas and spaces if desired
            var columns = GetColumnLineData(line, '|');
            if (columns.Length != numberOfColumns)
            {
                return null;
            }
            if (!int.TryParse(columns[periodColumn], out var period))
            {
                return null;
            }
            if (!double.TryParse(columns[valueColumn], out var value))
            {
                return null;
            }
            return new ObservedPointOfInterest
            {
                LocationSeriesName = columns[locationColumn],
                Period = period,
                ValueInCubicFeet = value,
            };
        }

        public virtual StressPeriodsLocationRates GetLocationRates()
        {
            var fileLines = GetModelFileLines(GetFileName(WelFileKey));
            using (var fileLineEnumerator = fileLines.GetEnumerator())
            {
                var result = new StressPeriodsLocationRates
                {
                    Parameters = new List<string>(),
                    StressPeriods = new List<StressPeriodLocationRates>()
                };

                fileLineEnumerator.MoveNext();
                var currentline = fileLineEnumerator.Current;
                if (currentline.StartsWith("PARAMETER"))
                {
                    result.Parameters.Add(currentline);
                    //we will eventually need to be able to handle actual parameters but we don't have enough information about them yet
                    fileLineEnumerator.MoveNext();
                    currentline = fileLineEnumerator.Current;
                }

                result.HeaderValue = ParseWelFileHeaderData(currentline);

                while (fileLineEnumerator.MoveNext())
                {
                    var currStressPeriodRates = new StressPeriodLocationRates
                    {
                        LocationRates = new List<LocationRate>()
                    };
                    var stressPeriodHeaderData = ParseStressPeriodHeaderData(fileLineEnumerator.Current);
                    AddRateData(fileLineEnumerator, currStressPeriodRates.LocationRates, stressPeriodHeaderData.RowCount);
                    if (stressPeriodHeaderData.AlternateWellRowCount != null)
                    {
                        currStressPeriodRates.ClnLocationRates = new List<LocationRate>();
                        AddRateData(fileLineEnumerator, currStressPeriodRates.ClnLocationRates, stressPeriodHeaderData.AlternateWellRowCount.Value);
                    }

                    result.StressPeriods.Add(currStressPeriodRates);
                }
                return result;
            }
        }

        private void AddRateData(IEnumerator<string> fileLineEnumerator, List<LocationRate> rates, int rowCount)
        {
            for (var i = 0; i < rowCount; i++)
            {
                fileLineEnumerator.MoveNext();
                var data = ParseLocationRateData(fileLineEnumerator.Current);
                rates.Add(new LocationRate
                {
                    Location = data.Location,
                    Rate = data.Rate
                });
            }
        }

        private string ParseWelFileHeaderData(string line)
        {
            return FileFormatter.ParseWelFileHeaderData(line);
        }

        private (string Location, double Rate) ParseLocationRateData(string line)
        {
            return FileFormatter.ParseLocationRateData(line);
        }

        private (int RowCount, int Option, int? AlternateWellRowCount) ParseStressPeriodHeaderData(string line)
        {
            return FileFormatter.ParseStressPeriodHeaderData(line);
        }

        public virtual void UpdateLocationRates(StressPeriodsLocationRates stressPeriods)
        {
            var welFileFullPath = System.IO.Path.Combine(ConfigurationHelper.AppSettings.ModflowDataFolder, GetFileName(WelFileKey));
            using (var sw = new StreamWriter(new FileStream(welFileFullPath, FileMode.Create, FileAccess.Write)))
            {
                if (stressPeriods.Parameters?.Any() == true)
                {
                    foreach (var param in stressPeriods.Parameters)
                    {
                        sw.WriteLine(param);
                    }
                }
                if (stressPeriods.StressPeriods == null)
                {
                    stressPeriods.StressPeriods = new List<StressPeriodLocationRates>();
                }
                var maxCount = stressPeriods.StressPeriods.Select(a => (a.LocationRates?.Count ?? 0) + (a.ClnLocationRates?.Count ?? 0)).Max();
                WriteTotalHeaderRow(sw, maxCount, stressPeriods.HeaderValue);

                foreach (var stressPeriod in stressPeriods.StressPeriods)
                {
                    if (stressPeriod.LocationRates == null)
                    {
                        stressPeriod.LocationRates = new List<LocationRate>();
                    }
                    WriteStressPeriodHeaderRow(sw, stressPeriod.LocationRates.Count, stressPeriod.Flag, stressPeriod.ClnLocationRates?.Count);
                    foreach (var locationRate in stressPeriod.LocationRates)
                    {
                        WriteLocationRateRow(sw, locationRate);
                    }
                    if (stressPeriod.ClnLocationRates != null)
                    {
                        foreach (var locationRate in stressPeriod.ClnLocationRates)
                        {
                            WriteLocationRateRow(sw, locationRate);
                        }
                    }
                }
            }
        }

        protected void WriteLocationRateRow(StreamWriter sw, LocationRate locationRate)
        {
            FileFormatter.WriteLocationRateRow(sw, locationRate);
        }

        protected void WriteStressPeriodHeaderRow(TextWriter tw, int wellCount, int flag, int? alternateWellCount)
        {
            FileFormatter.WriteStressPeriodHeaderRow(tw, wellCount, flag, alternateWellCount);
        }

        protected void WriteTotalHeaderRow(TextWriter tw, int count, string headerValue)
        {
            FileFormatter.WriteTotalHeaderRow(tw, count, headerValue);
        }

        public List<LocationProportion> GetLocationProportions(string feature)
        {
            if (!LocationFlowProportionsLazy.Value.ContainsKey(feature) || LocationFlowProportionsLazy.Value[feature] == null)
            {
                return new List<LocationProportion>();
            }
            return LocationFlowProportionsLazy.Value[feature];
        }

        public virtual IEnumerable<string> GetRunListFileLines()
        {
            return GetModelFileLines(GetFileName(ListFileKey));
        }

        public virtual IEnumerable<string> GetListFileOutputFileLines()
        {
            return GetModelFileLines(GetFileName(ListFileKey));
        }

        public string GetFriendlyInputZoneName(string zoneKey)
        {
            if (FriendlyInputZoneNamesDictionaryLazy.Value.ContainsKey(zoneKey) && FriendlyInputZoneNamesDictionaryLazy.Value[zoneKey] != null)
            {
                return FriendlyInputZoneNamesDictionaryLazy.Value[zoneKey];
            }
            return "";
        }

        public string GetFriendlyZoneBudgetName(string zoneKey)
        {
            if (FriendlyZoneBudgetNamesDictionaryLazy.Value.ContainsKey(zoneKey) && FriendlyZoneBudgetNamesDictionaryLazy.Value[zoneKey] != null)
            {
                return FriendlyZoneBudgetNamesDictionaryLazy.Value[zoneKey];
            }
            return "";
        }

        public List<string> GetLocationPositionMap()
        {
            return LocationMapPositionsLazy.Value.Keys.ToList();
        }

        private Dictionary<string, (SqlGeography Geography, List<LocationPumpingProportion> LocationPumpingProportions)> CreateLocationPositionMap()
        {
            if (!FileExists(LocationMapCoordinatesFileName))
            {
                return null;
            }
            return CreateLocationPositionMap(GetFileData(LocationMapCoordinatesFileName));
        }

        private Dictionary<string, (SqlGeography Geography, List<LocationPumpingProportion> LocationPumpingProportions)> CreateLocationPositionMap(TextReader fileData)
        {
            return FileFormatter.CreateLocationPositionMap(fileData);
        }

        public virtual IEnumerable<MapOutputData> GetBaselineMapData()
        {
            return GetMapOutputData(BaselineMapFileName);
        }

        public virtual IEnumerable<MapOutputData> GetOutputMapData()
        {
            if (string.IsNullOrWhiteSpace(Model.MapRunFileName) || !FileExists(Model.MapRunFileName))
            {
                return null;
            }
            return GetMapOutputData(Model.MapRunFileName);
        }

        public IEnumerable<MapOutputData> GetDrawdownMapData()
        {
            if (string.IsNullOrWhiteSpace(Model.MapDrawdownFileName) || !FileExists(Model.MapDrawdownFileName))
            {
                return null;
            }
            return GetMapOutputData(Model.MapDrawdownFileName);
        }

        protected static readonly string[] ValidMapFileHeaders =
        {
            "           HEADU",
            "            HEAD",
            "       DRAWDOWNU",
            "        DRAWDOWN",
            "       CLN HEADS",
            "    CLN DRAWDOWN"
        };

        protected static readonly string[] MapOutputHeaderRecordsToReturn =
        {
            "           HEADU",
            "            HEAD",
            "        DRAWDOWN",
        };

        protected static readonly string[] ModflowSixValidMapFileHeaders =
        {
            "HEADU           ",
            "HEAD            ",
            "DRAWDOWNU       ",
            "DRAWDOWN        ",
            "CLN HEADS       ",
            "CLN DRAWDOWN    "
        };

        protected static readonly string[] ModflowSixMapOutputHeaderRecordsToReturn =
        {
            "HEADU           ",
            "HEAD            "
        };

        private IEnumerable<MapOutputData> GetMapOutputData(string fileName)
        {
            using (var bs = new BufferedStream(GetFileStream(fileName), 4194304))
            using (var br = new BinaryReader(bs, System.Text.Encoding.ASCII))
            {
                while (br.PeekChar() != -1)
                {
                    var timeStep = br.ReadUInt32();
                    var stressPeriod = br.ReadUInt32();

                    if (Model.IsDoubleSizeHeatMapOutput)
                    {
                        br.ReadDouble();//time value for current stress period
                        br.ReadDouble();//total simulation time
                    }
                    else
                    {
                        br.ReadSingle();//time value for current stress period
                        br.ReadSingle();//total simulation time
                    }

                    var headerText = new string(br.ReadChars(16));
                    if (!ValidMapFileHeaders.Contains(headerText))
                    {
                        throw new Exception($"Invalid map file record header {headerText}");
                    }
                    var isOutputHeader = MapOutputHeaderRecordsToReturn.Contains(headerText);
                    foreach (var result in GetRecordMapOutputValues(br, stressPeriod, timeStep))
                    {
                        if (isOutputHeader)
                        {
                            if (result.Value != null && result.Value.Value.IsEqual(DryLocationIndicatorLazy.Value, GetDryLocationIndicatorTolerance()))
                            {
                                result.Value = null;
                            }
                            yield return result;
                        }
                    }
                }
            }
        }

        private double GetDryLocationIndicator()
        {
            if (!string.IsNullOrWhiteSpace(GetFileName(LpfFileKey)))
            {
                return GetDryLocationIndicator(GetModelFileLines(GetFileName(LpfFileKey)).First());
            }
            else if (!string.IsNullOrWhiteSpace(GetFileName(UpwFileKey)))
            {
                return GetDryLocationIndicator(GetModelFileLines(GetFileName(UpwFileKey)).First());
            }
            else if (!string.IsNullOrWhiteSpace(GetFileName(BcfFileKey)))
            {
                return GetDryLocationIndicator(GetModelFileLines(GetFileName(BcfFileKey)).First());
            }
            else if (!string.IsNullOrWhiteSpace(GetFileName(HufFileKey)))
            {
                return GetDryLocationIndicator(GetModelFileLines(GetFileName(HufFileKey)).First());
            }

            throw new Exception("No file for dry indicator");
        }

        private double GetDryLocationIndicator(string fileLine)
        {
            return FileFormatter.GetDryLocationIndicator(fileLine);
        }

        protected double? _dryLocationIndicatorTolerance;
        protected double GetDryLocationIndicatorTolerance()
        {
            return _dryLocationIndicatorTolerance ?? (_dryLocationIndicatorTolerance = Math.Abs(DryLocationIndicatorLazy.Value / 1e+7)).Value;
        }

        private IEnumerable<MapOutputData> GetRecordMapOutputValues(BinaryReader br, uint stressPeriod, uint timeStep)
        {
            return FileFormatter.GetRecordMapOutputValues(br, stressPeriod, timeStep);
        }

        public IEnumerable<string> GetBaselineListFileLines()
        {
            return GetModelFileLines(BaselineListFileName);
        }

        public List<AsrDataMap> GetAsrDataNameMap()
        {
            return GetAsrDataNameMap(AsrDataNameMapFileName) ?? new List<AsrDataMap>();
        }

        public List<AsrDataMap> GetZoneBudgetAsrDataNameMap()
        {
            var zoneBudgetAsrDataNameMap = GetAsrDataNameMap(ZoneBudgetAsrDataNameMapFileName);
            if (zoneBudgetAsrDataNameMap == null)
            {
                zoneBudgetAsrDataNameMap = GetAsrDataNameMap();
            }
            return zoneBudgetAsrDataNameMap;
        }

        private List<AsrDataMap> GetAsrDataNameMap(string fileName)
        {
            if (!FileExists(fileName))
            {
                return null;
            }

            var result = new List<AsrDataMap>();
            var reader = new CsvReader(GetFileData(fileName));
            reader.Read();
            reader.ReadHeader();
            while (reader.Read())
            {
                result.Add(reader.GetRecord<AsrDataMap>());
            }

            return result;
        }

        public virtual IEnumerable<ZoneBudgetItem> GetBaselineZoneBudgetItems(List<AsrDataMap> asrData)
        {
            return GetZoneBudgetItems(BaselineZoneBudgetFileName, asrData);
        }

        public virtual IEnumerable<ZoneBudgetItem> GetRunZoneBudgetItems(List<AsrDataMap> asrData)
        {
            return GetZoneBudgetItems(RunZoneBudgetFileName, asrData);
        }

        public virtual IEnumerable<ZoneBudgetItem> GetZoneBudgetItems(string fileName, List<AsrDataMap> asrData)
        {
            if (!FileExists(fileName))
            {
                return null;
            }
            return ReadZoneBudgetItemsFile(fileName);
        }

        protected virtual IEnumerable<ZoneBudgetItem> ReadZoneBudgetItemsFile(string fileName)
        {
            var reader = new CsvReader(GetFileData(fileName));
            reader.Configuration.RegisterClassMap<ZoneBudgetItemMapper>();
            reader.Configuration.TrimOptions = TrimOptions.Trim;
            reader.Read();
            reader.ReadHeader();
            while (reader.Read())
            {
                yield return reader.GetRecord<ZoneBudgetItem>();
            }
        }

        private class ZoneBudgetItemMapper : ClassMap<ZoneBudgetItem>
        {
            public ZoneBudgetItemMapper()
            {
                Map(m => m.Period).Name("PERIOD");
                Map(m => m.Step).Name("STEP");
                Map(m => m.Zone).Name("ZONE");
                var allMappedColumns = new[] { "PERIOD", "STEP", "ZONE" };
                //Adding a column here? add it to the mappedColumns array below
                Map(m => m.Values).ConvertUsing(r =>
                {
                    var row = (CsvHelper.CsvReader)r;
                    //any column outside our expected values is treated as canal
                    //wish we could programatically check which columns are already mapped, couldn't figure it out
                    var values = new List<ZoneBudgetValue>();

                    for (var i = 0; i < row.Context.HeaderRecord.Length; i++)
                    {
                        var header = row.Context.HeaderRecord[i];
                        if (!allMappedColumns.Contains(header.ToUpper()) && double.TryParse(row.GetField(i), out var value))
                        {
                            values.Add(new ZoneBudgetValue
                            {
                                Key = header,
                                Value = value
                            });
                        }
                    }

                    return values;
                });
            }
        }

        public IEnumerable<PointOfInterest> GetPointsOfInterest()
        {
            if (!FileExists(PointsOfInterestFileName))
            {
                return null;
            }
            return ReadPointsOfInterestFile(PointsOfInterestFileName);
        }

        private IEnumerable<PointOfInterest> ReadPointsOfInterestFile(string filename)
        {
            var reader = new CsvReader(GetFileData(filename));
            reader.Configuration.RegisterClassMap<PointOfInterestItemMapper>();
            reader.Configuration.TrimOptions = TrimOptions.Trim;
            reader.Configuration.Delimiter = "|";
            reader.Read();
            reader.ReadHeader();
            while (reader.Read())
            {
                yield return reader.GetRecord<PointOfInterest>();
            }
        }

        private class PointOfInterestItemMapper : ClassMap<PointOfInterest>
        {
            public PointOfInterestItemMapper()
            {
                Map(m => m.Coordinate.Lat).Name("Latitude");
                Map(m => m.Coordinate.Lng).Name("Longitude");
                Map(m => m.Name).Name("Name");
            }
        }

        public Model Model { get; }

        private SqlGeography UnionGeographies(IEnumerable<SqlGeography> geographies)
        {
            SqlGeography unionedGeographies = null;
            foreach (var dbGeography in geographies)
            {
                unionedGeographies = unionedGeographies == null ? dbGeography : unionedGeographies.STUnion(dbGeography);
            }
            return unionedGeographies;
        }

        public string ReduceMapCells(int stressPeriod, List<MapLocationsPositionCellColor> colorsList)
        {
            var features = new FeatureCollection();

            foreach (var color in colorsList.Where(a => a.Locations.Any()))
            {
                Logger.LogDebug($"Unioning geographies [{stressPeriod}][{color.Color}]");
                var geographies = UnionAllGeographies(color);

                Logger.LogDebug($"Buffering geographies [{stressPeriod}][{color.Color}]");
                var geography = geographies.BufferWithTolerance(0.15, 0.5, false);

                Logger.LogDebug($"Reducing geographies [{stressPeriod}][{color.Color}]");
                geography = geography.Reduce(0.5).MakeValid();

                Logger.LogDebug($"Creating Geography Feature [{stressPeriod}][{color.Color}]");
                features.Add(CreateGeographicFeature(stressPeriod, geography, color));
            }

            var reduceMapCells = features.Serialize();
            return reduceMapCells;
        }

        private static Feature CreateGeographicFeature(int stressPeriod, SqlGeography geography, MapLocationsPositionCellColor color)
        {
            var feature = SqlGeographyToFeature(geography);
            feature.Attributes = new AttributesTable(new Dictionary<string, object>());
            feature.Attributes.Add("color", color.Color);
            feature.Attributes.Add("stressPeriod", stressPeriod);
            if (feature.Geometry is GeometryCollection collection)
            {
                feature.Geometry = new GeometryCollection(collection.Geometries.Where(a => !(a is Point)).ToArray());
            }

            return feature;
        }

        private static Feature SqlGeographyToFeature(SqlGeography geography)
        {
            // Create a WKT reader
            WKTReader reader = new WKTReader();

            // Parse the WKT string and create a geometry object
            var wkt = new string(geography.AsTextZM().Value);
            Geometry geometry = reader.Read(wkt);
            
            Feature feature = new Feature(){Geometry = geometry};

            return feature;
        }

        private SqlGeography UnionAllGeographies(MapLocationsPositionCellColor color)
        {
            //Unioning two geographies at a time was faster than just looping and joining by quite a bit.
            //Unioning in larger batches was a bit slower.
            var geographies = color.Locations.Select(a => LocationMapPositionsLazy.Value[a].Geography).ToList();
            while (geographies.Count > 1)
            {
                geographies = geographies.Batch(2).Select(UnionGeographies).ToList();
            }
            return geographies.First();
        }

        public string CreateSerializedFeatureCollectionFromWaterLevelChangeByZoneMapData(List<WaterLevelChangeByZoneMapData> results)
        {
            var features = new FeatureCollection();

            results.ForEach(x =>
            {
                var geography = CreateGeography(x.Bounds.Select(y => y.Lat).AsEnumerable(),
                    x.Bounds.Select(y => y.Lng).AsEnumerable()).Reduce(0.5).MakeValid();
                features.Add(CreateGeographicFeature(geography, x));
            });

            return features.Serialize();
        }

        private static Feature CreateGeographicFeature(SqlGeography geography, WaterLevelChangeByZoneMapData zone)
        {
            var feature = SqlGeographyToFeature(geography);
            feature.Attributes = new AttributesTable(new Dictionary<string, object>());
            feature.Attributes.Add("color", zone.Color);
            feature.Attributes.Add("zoneName", zone.ZoneName);

            if (feature.Geometry is GeometryCollection collection)
            {
                feature.Geometry = new GeometryCollection(collection.Geometries.Where(a => !(a is Point)).ToArray());
            }

            return feature;
        }

        public SqlGeography CreateGeography(IEnumerable<double> lats, IEnumerable<double> longs)
        {
            var points = lats.Zip(longs, (lat, lng) => new { lat, lng });
            var stringPoints = points.Select(a => $"{Math.Round(a.lng, 7)} {Math.Round(a.lat, 7)}");
            return SqlGeography.Parse($"POLYGON(({string.Join(", ", stringPoints)}))");//"POLYGON((-100 40, -110 40, -110 30, -100 40))"
        }

        public List<LocationPumpingProportion> FindWellLocations(double lat, double lng)
        {
            foreach (var item in LocationMapPositionsLazy.Value)
            {
                var geography = item.Value.Geography;
                if (geography.STIntersects(SqlGeography.Point(lat, lng, geography.STSrid.Value)))
                {
                    return item.Value.LocationPumpingProportions;
                }
            }
            return new List<LocationPumpingProportion>();
        }

        public LocationWithBounds FindLocationCell(double lat, double lng)
        {
            foreach (var item in LocationMapPositionsLazy.Value)
            {
                var geography = item.Value.Geography;
                if (geography.STIntersects(SqlGeography.Point(lat, lng, geography.STSrid.Value)))
                {
                    List<Common.DataContracts.Models.Coordinate> bounds = new List<Common.DataContracts.Models.Coordinate>();
                    for (int i = 1; i <= geography.STNumPoints(); i++)
                    {
                        var p = geography.STPointN(i);
                        bounds.Add(new Common.DataContracts.Models.Coordinate()
                        {
                            Lat = (double)p.Lat,
                            Lng = (double)p.Long
                        });
                    }

                    return new LocationWithBounds()
                    {
                        Location = item.Key.Replace('|', ' '),
                        BoundCoordinates = bounds,
                    };
                }
            }
            return null;
        }

        public List<Common.DataContracts.Models.Coordinate> FindCellBounds(string key)
        {
            if (LocationMapPositionsLazy.Value.ContainsKey(key))
            {
                var geo = LocationMapPositionsLazy.Value.FirstOrDefault(l => l.Key == key);

                List<Common.DataContracts.Models.Coordinate> bounds = new List<Common.DataContracts.Models.Coordinate>();

                for (int i = 1; i <= geo.Value.Geography.STNumPoints(); i++)
                {
                    var p = geo.Value.Geography.STPointN(i);
                    bounds.Add(new Common.DataContracts.Models.Coordinate()
                    {
                        Lat = (double)p.Lat,
                        Lng = (double)p.Long
                    });
                }

                return bounds;
            }

            return null;
        }

        private Dictionary<string, List<string>> CreateOutputLocationZoneDictionary()
        {
            var result = new Dictionary<string, List<string>>();

            if (FileExists(OutputLocationZoneFileName))
            {
                GetLocationZonesFromFile(OutputLocationZoneFileName, result);
            }
            return result;
        }

        private Dictionary<string, List<string>> CreateInputLocationZoneDictionary()
        {
            var result = new Dictionary<string, List<string>>();
            var fileName = "";

            //MP 1/27/2021 We introduced a new file name convention, but don't want 
            //to force the user to go through and update all the files that were previously
            //named differently. The first one will always be the newer convention, but we can
            //search for the others if need be.
            foreach (var file in LocationZonePossibleFileNames)
            {
                if (FileExists(file))
                {
                    fileName = file;
                    break;
                }
            }


            if (!string.IsNullOrEmpty(fileName))
            {
                GetLocationZonesFromFile(fileName, result);
            }
            return result;
        }

        private void GetLocationZonesFromFile(string fileName, Dictionary<string, List<string>> result)
        {
            var reader = new CsvReader(GetFileData(fileName));
            reader.Read();
            reader.ReadHeader();
            foreach (var record in GetLocationZoneData(reader))
            {
                if (!result.ContainsKey(record.Location))
                {
                    result[record.Location] = new List<string>();
                }

                if (!result[record.Location].Contains(record.Zone))
                {
                    result[record.Location].Add(record.Zone);
                }
            }
        }

        private IEnumerable<(string Location, string Zone)> GetLocationZoneData(CsvReader reader)
        {
            return FileFormatter.GetLocationZoneData(reader);
        }

        public List<string> GetOutputLocationZones(string location)
        {
            if (OutputLocationZoneDictionaryLazy.Value.ContainsKey(location) && OutputLocationZoneDictionaryLazy.Value[location] != null)
            {
                return OutputLocationZoneDictionaryLazy.Value[location];
            }
            return new List<string>();
        }

        public bool OutputLocationZonesExists()
        {
            return FileExists(OutputLocationZoneFileName);
        }

        public List<string> GetInputLocationZones(string location)
        {
            if (InputLocationZoneDictionaryLazy.Value.ContainsKey(location) && InputLocationZoneDictionaryLazy.Value[location] != null)
            {
                return InputLocationZoneDictionaryLazy.Value[location];
            }
            return new List<string>();
        }

        public string GetModpathListFileContent(string listFileName)
        {
            return GetModelFile(listFileName);
        }

        public string GetModpathListFileName(string simFileName)
        {
            //listfile name is the 2nd line ofthe simFile
            var simData = GetModelFileLines(simFileName);

            if (simData.Count() < 2)
            {
                throw new Exception("Could not get list file name from simulation file");
            }

            return simData.ElementAt(1);
        }

        public string GetModpathLocationFileName(string simFileName)
        {
            //listfile name is the 2nd line ofthe simFile
            var simData = GetModelFileLines(simFileName);

            // last, non empty row
            var lastLine = simData.Last(data => !string.IsNullOrWhiteSpace(data)).Trim();

            if (lastLine != null && lastLine.StartsWith("external "))
            {
                lastLine = lastLine.Replace("external ", "");
            }

            return lastLine;
        }

        public string GetModpathTimeSeriesFileName(string simFileName)
        {
            //listfile name is the 2nd line ofthe simFile
            var simData = GetModelFileLines(simFileName);

            if (simData.Count() < 6)
            {
                throw new Exception("Could not get list file name from simulation file");
            }

            return simData.ElementAt(5);
        }

        public void WriteLocationFile(string fileName, string data)
        {
            Logger.LogInformation($"Writing location file at {ConfigurationHelper.AppSettings.ModflowDataFolder} with filename {fileName} ");
            File.WriteAllText(Path.Combine(ConfigurationHelper.AppSettings.ModflowDataFolder, fileName), data);
        }

        public List<ModpathTimeSeries> GetModpathTimeSeriesResult(string fileName)
        {
            var result = new List<ModpathTimeSeries>();
            var rawFileLines = GetModelFileLines(fileName);

            //skip the first 3 lines that are the header
            foreach (var line in rawFileLines.Skip(3))
            {
                result.Add(new ModpathTimeSeries()
                {
                    TimePointIndex = int.Parse(line.Substring(0, 8)),
                    CumulativeTimeStep = int.Parse(line.Substring(8, 8)),
                    TrackingTime = double.Parse(line.Substring(16, 18)),
                    SequenceNumber = int.Parse(line.Substring(34, 10)),
                    ParticleGroup = int.Parse(line.Substring(44, 5)),
                    ParticleId = int.Parse(line.Substring(49, 10)),
                    CellNumber = int.Parse(line.Substring(59, 10)),
                    LocalX = double.Parse(line.Substring(69, 18)),
                    LocalY = double.Parse(line.Substring(87, 18)),
                    LocalZ = double.Parse(line.Substring(105, 18)),
                    GlobalX = double.Parse(line.Substring(123, 18)),
                    GlobalY = double.Parse(line.Substring(141, 18)),
                    GlobalZ = double.Parse(line.Substring(159, 18)),
                    Layer = int.Parse(line.Substring(177, 10))
                });
            }

            return result;
        }
    }
}
