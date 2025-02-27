using CsvHelper;
using Microsoft.SqlServer.Types;
using Newtonsoft.Json;
using Olsson.GET.Common.DataContracts.Runs;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper.Configuration;

namespace Olsson.GET.Accessors.FileIO
{

    internal class FixedWidthFileFormatter : BaseFileFormatter, IFileFormatter
    {
        public FixedWidthFileFormatter(ModelFileAccessor accessor) : base(accessor)
        {

        }

        public string ParseWelFileHeaderData(string line)
        {
            return line.Substring(10);
        }

        public (int RowCount, int Option, int? AlternateWellRowCount) ParseStressPeriodHeaderData(string line)
        {
            var rowCountStr = line.Substring(0, 10);
            var optionStr = line.Substring(10, 10);
            var alternateRowCountStr = "";
            if (line.Length >= 30)
            {
                alternateRowCountStr = line.Substring(20, 10);
            }

            int? alternateRowCount = null;
            if (int.TryParse(alternateRowCountStr.Trim(), out var tryAlternateRowCount))
            {
                alternateRowCount = tryAlternateRowCount;
            }
            return (int.Parse(rowCountStr.Trim()), int.Parse(optionStr.Trim()), alternateRowCount);
        }

        public void WriteLocationRateRow(StreamWriter sw, LocationRate locationRate)
        {
            var values = DecomposeStructuredKey(locationRate.Location);
            sw.WriteLine(
                $"{values.Layer.ToString().PadLeft(10)}{values.Row.ToString().PadLeft(10)}{values.Column.ToString().PadLeft(10)}{GetFixedWidthOutputValue(locationRate.Rate, 10)}");

        }

        public (string Location, double Rate) ParseLocationRateData(string line)
        {
            //#########0#########0#########0#######0.0
            var data = GetFixedWidthData(line, true, 10, 10, 10, 10);
            var location = BuildStructuredKey(int.Parse(data[0]), int.Parse(data[1]), int.Parse(data[2]));
            var rate = double.Parse(data[3]);

            return (location, rate);
        }

        public void WriteStressPeriodHeaderRow(TextWriter tw, int wellCount, int flag, int? alternateWellCount)
        {
            var alternateWellCountString = "";
            if (alternateWellCount != null)
            {
                alternateWellCountString = alternateWellCount.ToString().PadLeft(10);
            }
            tw.WriteLine($"{wellCount.ToString().PadLeft(10)}{flag.ToString().PadLeft(10)}{alternateWellCountString}");
        }

        public void WriteTotalHeaderRow(TextWriter tw, int count, string headerValue)
        {
            tw.WriteLine($"{count.ToString().PadLeft(10)}{headerValue}");
        }

        public Dictionary<string, (SqlGeography Geography, List<LocationPumpingProportion> LocationPumpingProportions)> CreateLocationPositionMap(TextReader fileData)
        {
            return CreateLocationPositionMap<StructuredLocationMapPositionRecord>(fileData, a => BuildStructuredKey(a.Layer, a.Row, a.Col), GetLocationPumpingProportions);
        }

        public double GetDryLocationIndicator(string fileLine)
        {
            return double.Parse(GetFixedWidthData(fileLine, true, 10, 16)[1]);
        }

        public IEnumerable<MapOutputData> GetRecordMapOutputValues(BinaryReader br, uint stressPeriod, uint timeStep)
        {
            var colCount = br.ReadUInt32();
            var rowCount = br.ReadUInt32();
            var layer = br.ReadUInt32(); //layer number
            for (var row = 1; row <= rowCount; row++)
            {
                for (var col = 1; col <= colCount; col++)
                {
                    double value;
                    if (base.Accessor.Model.IsDoubleSizeHeatMapOutput)
                    {
                        value = br.ReadDouble();
                    }
                    else
                    {
                        value = br.ReadSingle();
                    }

                    yield return new MapOutputData
                    {
                        Location = BuildStructuredKey(Convert.ToInt32(layer), row, col),
                        StressPeriod = Convert.ToInt32(stressPeriod),
                        TimeStep = Convert.ToInt32(timeStep),
                        Value = value
                    };
                }
            }
        }

        public IEnumerable<(string Location, string Zone)> GetLocationZoneData(CsvReader reader)
        {
            while (reader.Read())
            {
                var record = reader.GetRecord<StructuredLocationZone>();
                yield return (BuildStructuredKey(record.Layer, record.Row, record.Col), record.Zone);
            }
        }

        private string[] GetFixedWidthData(string line, bool trim, params int[] widths)
        {
            if (line.Length < widths.Sum())
                throw new ArgumentException("Not enough characters to build fixed width data.");

            string[] ret = new string[widths.Length];
            char[] c = line.ToCharArray();
            int startPos = 0;
            for (int i = 0; i < widths.Length; i++)
            {
                int width = widths[i];
                ret[i] = new string(c.Skip(startPos).Take(width).ToArray());
                if (trim)
                {
                    ret[i] = ret[i].Trim();
                }
                startPos += width;
            }
            return ret;
        }
    }

    internal class DelimitedFileFormatter : BaseFileFormatter, IFileFormatter
    {
        public DelimitedFileFormatter(ModelFileAccessor accessor) : base(accessor)
        {

        }
        private string[] GetColumnLineData(string line)
        {
            return line.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        }

        public string ParseWelFileHeaderData(string line)
        {
            return line.Substring(line.IndexOf(" ") + 1);
        }

        public (int RowCount, int Option, int? AlternateWellRowCount) ParseStressPeriodHeaderData(string line)
        {
            var data = GetColumnLineData(line);
            int? alternateRowCount = null;
            if (data.Length > 2 && int.TryParse(data[2].Trim(), out var tryAlternateRowCount))
            {
                alternateRowCount = tryAlternateRowCount;
            }

            if (data.Length <= 1)
            {
                throw new Exception($"Invalid Header row {line}");
            }

            return (int.Parse(data[0]), int.Parse(data[1]), alternateRowCount);
        }

        public void WriteLocationRateRow(StreamWriter sw, LocationRate locationRate)
        {
            if (Accessor.FileStructure == FileStructure.Unstructured)
            {
                sw.WriteLine($"{locationRate.Location} {locationRate.Rate:e6}");
            }
            else if (Accessor.FileStructure == FileStructure.Structured)
            {
                var values = DecomposeStructuredKey(locationRate.Location);
                sw.WriteLine($"{values.Layer} {values.Row} {values.Column} {locationRate.Rate}");
            }
        }

        public (string Location, double Rate) ParseLocationRateData(string line)
        {
            if (Accessor.FileStructure == FileStructure.Unstructured)
            {
                //0 0.0
                var data = GetColumnLineData(line);
                var location = data[0];
                var rate = double.Parse(data[1]);

                return (location, rate);
            }
            else if (Accessor.FileStructure == FileStructure.Structured)
            {
                //0 0 0 0.0
                var data = GetColumnLineData(line);
                var location = BuildStructuredKey(int.Parse(data[0]), int.Parse(data[1]), int.Parse(data[2]));
                var rate = double.Parse(data[3]);

                return (location, rate);
            }
            else if (Accessor.FileStructure == FileStructure.ModflowSixUnstructured)
            {
                //0 0.0
                var data = GetColumnLineData(line);
                var location = data[0];
                var rate = double.Parse(data[1]);

                return (location, rate);
            }
            else if (Accessor.FileStructure == FileStructure.ModflowSixStructured)
            {
                //0 0 0 0.0
                var data = GetColumnLineData(line);
                var location = BuildStructuredKey(int.Parse(data[0]), int.Parse(data[1]), int.Parse(data[2]));
                var rate = double.Parse(data[3]);

                return (location, rate);
            }

            throw new Exception("Unsupported file structure");
        }

        public void WriteStressPeriodHeaderRow(TextWriter tw, int wellCount, int flag, int? alternateWellCount)
        {
            var alternateWellCountString = "";
            if (alternateWellCount != null)
            {
                alternateWellCountString = $" {alternateWellCount}";
            }

            tw.WriteLine($"{wellCount} {flag}{alternateWellCountString}");
        }

        public void WriteTotalHeaderRow(TextWriter tw, int count, string headerValue)
        {
            tw.WriteLine($"{count} {headerValue}");
        }

        public Dictionary<string, (SqlGeography Geography, List<LocationPumpingProportion> LocationPumpingProportions)> CreateLocationPositionMap(TextReader fileData)
        {
            if (Accessor.FileStructure == FileStructure.Unstructured)
            {
                return CreateLocationPositionMap<UnstructuredLocationMapPositionRecord>(fileData, a => a.Node,
                    GetLocationPumpingProportions);
            }
            else if (Accessor.FileStructure == FileStructure.Structured)
            {
                return CreateLocationPositionMap<StructuredLocationMapPositionRecord>(fileData, a => BuildStructuredKey(a.Layer, a.Row, a.Col), GetLocationPumpingProportions);
            }

            throw new Exception("Unsupported file structure");
        }

        public double GetDryLocationIndicator(string fileLine)
        {
            return double.Parse(GetColumnLineData(fileLine)[1]);
        }

        public IEnumerable<MapOutputData> GetRecordMapOutputValues(BinaryReader br, uint stressPeriod, uint timeStep)
        {
            if (Accessor.FileStructure == FileStructure.Unstructured)
            {
                var startingCellNumber = br.ReadUInt32();
                var endingCellNumber = br.ReadUInt32();
                br.ReadUInt32(); //layer number
                for (var z = startingCellNumber; z <= endingCellNumber; z++)
                {
                    var value = br.ReadSingle();

                    yield return new MapOutputData
                    {
                        Location = z.ToString(),
                        StressPeriod = Convert.ToInt32(stressPeriod),
                        TimeStep = Convert.ToInt32(timeStep),
                        Value = value
                    };
                }
            }
            else if (Accessor.FileStructure == FileStructure.Structured)
            {
                var colCount = br.ReadUInt32();
                var rowCount = br.ReadUInt32();
                var layer = br.ReadUInt32(); //layer number
                for (var row = 1; row <= rowCount; row++)
                {
                    for (var col = 1; col <= colCount; col++)
                    {
                        double value;
                        if (base.Accessor.Model.IsDoubleSizeHeatMapOutput)
                        {
                            value = br.ReadDouble();
                        }
                        else
                        {
                            value = br.ReadSingle();
                        }

                        yield return new MapOutputData
                        {
                            Location = BuildStructuredKey(Convert.ToInt32(layer), row, col),
                            StressPeriod = Convert.ToInt32(stressPeriod),
                            TimeStep = Convert.ToInt32(timeStep),
                            Value = value
                        };
                    }
                }
            }
        }

        public IEnumerable<(string Location, string Zone)> GetLocationZoneData(CsvReader reader)
        {
            if (Accessor.FileStructure == FileStructure.Unstructured)
            {
                while (reader.Read())
                {
                    var record = reader.GetRecord<UnstructuredLocationZone>();
                    yield return (record.Node, record.Zone);
                }
            }
            else if (Accessor.FileStructure == FileStructure.Structured)
            {
                while (reader.Read())
                {
                    var record = reader.GetRecord<StructuredLocationZone>();
                    yield return (BuildStructuredKey(record.Layer, record.Row, record.Col), record.Zone);
                }
            }
        }
    }
    
    internal abstract class BaseFileFormatter
    {
        protected BaseFileFormatter(ModelFileAccessor accessor)
        {
            Accessor = accessor;

            //System doesn't support this, no valid models for now either
            if (Accessor.FileFormat == FileFormat.FixedWidth && Accessor.FileStructure == FileStructure.Unstructured)
            {
                throw new Exception("Fixed Width Unstructured model not supported");
            }
        }

        protected ModelFileAccessor Accessor { get; set; }

        protected Dictionary<string, (SqlGeography Geography, List<LocationPumpingProportion> LocationPumpingProportions)> CreateLocationPositionMap<T>(TextReader fileData, Func<T, string> getKeyFunc, Func<T, List<LocationPumpingProportion>> getWellPumpingFunc)
        {
            var result = new Dictionary<string, (SqlGeography Geography, List<LocationPumpingProportion> LocationPumpingProportions)>();
            var csvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null
            };
            using var csvReader = new CsvReader(fileData, csvConfiguration);
            csvReader.Read();
            csvReader.ReadHeader();
            var startingLatLongIndex = csvReader.HeaderRecord?.Length ?? 0;
            while (csvReader.Read())
            {
                var record = csvReader.GetRecord<T>();
                var latLngs = new List<double>();
                var currIndex = startingLatLongIndex;
                while (csvReader.TryGetField<string>(currIndex++, out var strValue) && double.TryParse(strValue, out var value))
                {
                    latLngs.Add(value);
                }
                var longs = latLngs.Where((x, i) => i % 2 == 0).ToList();
                var lats = latLngs.Where((x, i) => (i + 1) % 2 == 0).ToList();
                var key = getKeyFunc(record);
                result[key] = (CreateGeography(lats, longs), GetLocationPumpingProportions(key, getWellPumpingFunc(record)));
            }

            return result;
        }

        private SqlGeography CreateGeography(IEnumerable<double> lats, IEnumerable<double> longs)
        {
            var points = lats.Zip(longs, (lat, lng) => new { lat, lng });
            var stringPoints = points.Select(a => $"{Math.Round(a.lng, 7)} {Math.Round(a.lat, 7)}");
            return SqlGeography.Parse($"POLYGON(({string.Join(", ", stringPoints)}))");//"POLYGON((-100 40, -110 40, -110 30, -100 40))"
        }

        protected static List<LocationPumpingProportion> GetLocationPumpingProportions(string key, List<LocationPumpingProportion> wellPumpingNodes)
        {
            if (wellPumpingNodes == null || wellPumpingNodes.Count == 0)
            {
                wellPumpingNodes = new List<LocationPumpingProportion>
                {
                    new LocationPumpingProportion
                    {
                        Location = key,
                        Proportion = 1
                    }
                };
            }
            if (wellPumpingNodes.All(a => a.Proportion == 0.0))
            {
                foreach (var locationPumpingProportion in wellPumpingNodes)
                {
                    locationPumpingProportion.Proportion = 1.0 / wellPumpingNodes.Count;
                }
            }
            return wellPumpingNodes;
        }

        protected static string BuildStructuredKey(int layer, int row, int column)
        {
            return $"{layer}|{row}|{column}";
        }

        protected static (int Layer, int Row, int Column) DecomposeStructuredKey(string value)
        {
            var parts = value.Split('|');
            return (int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]));
        }

        protected string GetFixedWidthOutputValue(double rate, int maxCharacters)
        {
            var maxDigits = maxCharacters;
            if (rate < 0)
            {
                maxDigits--;
            }
            if (Math.Abs(rate % 1) >= (Double.Epsilon * 100))
            {
                maxDigits--;
            }
            double minScientificNotationValue = Math.Pow(10, maxDigits);
            if (Math.Abs(rate) >= minScientificNotationValue)
            {
                if (rate < 0)
                {
                    return rate.ToString("e" + (maxCharacters - 8));
                }
                return rate.ToString("e" + (maxCharacters - 7));
            }
            return rate.ToString("f" + maxCharacters).PadLeft(maxCharacters).Substring(0, maxCharacters);
        }

        protected static List<LocationPumpingProportion> GetLocationPumpingProportions(StructuredLocationMapPositionRecord record)
        {
            List<LocationPumpingProportion> wellPumpingNodes = null;
            if (!string.IsNullOrWhiteSpace(record.WellPumpingNodes))
            {
                var structuredWellPumpingNodes = JsonConvert.DeserializeObject<List<StructuredLocationPumpingProportion>>(record.WellPumpingNodes);
                wellPumpingNodes = structuredWellPumpingNodes.Select(a => new LocationPumpingProportion
                {
                    Location = BuildStructuredKey(a.Layer, a.Row, a.Col),
                    Proportion = a.Proportion
                }).ToList();
            }
            return wellPumpingNodes;
        }

        protected static List<LocationPumpingProportion> GetLocationPumpingProportions(UnstructuredLocationMapPositionRecord record)
        {
            List<LocationPumpingProportion> wellPumpingNodes = null;
            if (!string.IsNullOrWhiteSpace(record.WellPumpingNodes))
            {
                wellPumpingNodes = JsonConvert.DeserializeObject<List<LocationPumpingProportion>>(record.WellPumpingNodes);
            }
            return wellPumpingNodes;
        }
    }
}
