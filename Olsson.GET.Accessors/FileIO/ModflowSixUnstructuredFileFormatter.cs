using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CsvHelper;
using Microsoft.SqlServer.Types;
using Olsson.GET.Common.DataContracts.Runs;

namespace Olsson.GET.Accessors.FileIO
{
    internal class ModflowSixUnstructuredFileFormatter : BaseFileFormatter, IFileFormatter
    {
        public ModflowSixUnstructuredFileFormatter(ModelFileAccessor accessor) : base(accessor)
        {

        }

        public string ParseWelFileHeaderData(string line)
        {
            return line.Substring(11);
        }

        public (string Location, double Rate) ParseLocationRateData(string line)
        {
            var data = line.Split().Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
            return (data[0], double.Parse(data[1]));
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

        public void WriteTotalHeaderRow(TextWriter tw, int count, string headerValue)
        {
            tw.Write($"begin dimensions\n  MAXBOUND {count}\nend dimensions\n\n");
        }

        public void WriteStressPeriodHeaderRow(TextWriter tw, int periodIndex, int flag, int? alternateWellCount)
        {
            tw.WriteLine($"begin period {periodIndex}");
        }

        public void WriteLocationRateRow(StreamWriter sw, LocationRate locationRate)
        {
            var line = BuildFixedWidthData(new string[] { locationRate.Location, locationRate.Rate.ToString() },
                new int[] { 11, 11 });
            sw.WriteLine(line);
        }

        public Dictionary<string, (SqlGeography Geography, List<LocationPumpingProportion> LocationPumpingProportions)> CreateLocationPositionMap(TextReader fileData)
        {
            return CreateLocationPositionMap<UnstructuredLocationMapPositionRecord>(fileData, a => a.Node,
                GetLocationPumpingProportions);
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
                var record = reader.GetRecord<UnstructuredLocationZone>();
                yield return (record.Node, record.Zone);
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

        private static string BuildFixedWidthData(string[] values, int[] widthParameters)
        {
            var sb = new StringBuilder();

            for (var i = 0; i < values.Length; i++)
            {
                if (values[i].Length < widthParameters[i])
                {
                    sb.Append(values[i].PadLeft(widthParameters[i]));
                }
                else
                {
                    sb.Append(values[i].PadLeft(values[i].Length + 1));
                }
            }

            return sb.ToString();
        }
    }
}