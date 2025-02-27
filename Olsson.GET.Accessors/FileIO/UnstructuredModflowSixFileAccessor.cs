using CsvHelper;
using CsvHelper.Configuration;
using Olsson.GET.Common.DataContracts.Models;
using Olsson.GET.Common.DataContracts.Runs;
using Olsson.GET.Common.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Olsson.GET.Accessors.FileIO
{
    internal class UnstructuredModflowSixFileAccessor : ModflowSixFileAccessor
    {
        public UnstructuredModflowSixFileAccessor(Model model) : base(model)
        {

        }

        private sealed class UnstructuredModflowSixProportionMapper : LocationProportionMapper
        {
            public UnstructuredModflowSixProportionMapper()
                : base(new[] { "node" })
            {
                {
                    Map(m => m.Location).Name("node");
                }
            }
        }

        protected override string DisFileKey => StructuredDisFileKey;
        protected override Type LocationProportionMapperType => typeof(UnstructuredModflowSixProportionMapper);
        protected override int NumberOfStressPeriodsColumnInDisFileIndex => 3;
        protected override int FlowToAquiferColumnInOutputIndex => 6;
        protected override int SegmentNumberColumnInOutputIndex => 3;
        protected override int ReachNumberColumnInOutputIndex => 4;


        public override List<StressPeriod> GetStressPeriodData()
        {
            var tdisFileName = GetFileName(StressPeriodFileKey);
            if (!FileExists(tdisFileName))
            {
                return new List<StressPeriod>();
            }

            var fileLines = GetModelFileLines(tdisFileName);
            var result = new List<StressPeriod>();

            using (var fileLineEnumerator = fileLines.GetEnumerator())
            {
                while (fileLineEnumerator.MoveNext())
                {
                    if (fileLineEnumerator.Current.IndexOf("END PERIODDATA", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        break;
                    }

                    if (fileLineEnumerator.Current.IndexOf("BEGIN PERIODDATA", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        AddStressPeriodData(fileLineEnumerator, result);
                    }
                }

                return result;
            }
        }

        private void AddStressPeriodData(IEnumerator<string> fileLineEnumerator, List<StressPeriod> stressPeriods)
        {
            while (fileLineEnumerator.MoveNext())
            {
                if (fileLineEnumerator.Current.IndexOf("END PERIODDATA", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    break;
                }

                var data = fileLineEnumerator.Current.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (data[0].StartsWith("#"))
                {
                    continue;
                }

                stressPeriods.Add(new StressPeriod
                {
                    Days = double.Parse(data[0]),
                    NumberOfTimeSteps = int.Parse(data[1])
                });
            }
        }

        private void AddRateData(IEnumerator<string> fileLineEnumerator, List<LocationRate> rates, List<string> wellList)
        {
            var encounteredLocations = new List<string>();

            while (fileLineEnumerator.MoveNext())
            {
                if (fileLineEnumerator.Current.IndexOf("end period", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    break;
                }

                var data = FileFormatter.ParseLocationRateData(fileLineEnumerator.Current);

                if (!wellList.Contains(data.Location))
                {
                    wellList.Add(data.Location);
                }

                encounteredLocations.Add(data.Location);

                rates.Add(new LocationRate
                {
                    Location = data.Location,
                    Rate = data.Rate
                });
            }

            rates.AddRange(wellList.Except(encounteredLocations).Select(x => new LocationRate
            {
                Location = x,
                Rate = 0.0
            }));
        }

        public override IEnumerable<MapOutputData> GetBaselineMapData()
        {
            return GetMapOutputData(BaselineMapFileName);
        }

        public override IEnumerable<MapOutputData> GetOutputMapData()
        {
            return GetMapOutputData(Model.MapRunFileName);
        }

        private IEnumerable<MapOutputData> GetMapOutputData(string fileName)
        {
            using (var bs = new BufferedStream(GetFileStream(fileName)))
            using (var br = new BinaryReader(bs, System.Text.Encoding.ASCII))
            {
                // For each stress period, time step, and layer for which data are saved to the binary output file, the following two records are written:
                // Record 1: KSTP,KPER,PERTIM,TOTIM,TEXT,NODES,1,1
                // Record 2: (DATA(N),N=1,MAXBOUND)

                while (br.PeekChar() != -1)
                {
                    // KSTP
                    var timeStep = br.ReadUInt32();
                    // KPER
                    var stressPeriod = br.ReadUInt32();

                    // skip pertim
                    br.ReadDouble();
                    // skip totim
                    br.ReadDouble();

                    var headerText = new string(br.ReadChars(16));
                    if (!ModflowSixValidMapFileHeaders.Contains(headerText))
                    {
                        throw new Exception($"Invalid map file record header {headerText}");
                    }

                    var isOutputHeader = ModflowSixMapOutputHeaderRecordsToReturn.Contains(headerText);
                    foreach (var result in GetRecordMapOutputValues(br, stressPeriod, timeStep))
                    {
                        if (isOutputHeader)
                        {
                            if (result.Value != null && Math.Abs(result.Value.Value) >= 1.0E30)
                            {
                                result.Value = null;
                            }
                            yield return result;
                        }
                    }
                }
            }
        }

        private IEnumerable<MapOutputData> GetRecordMapOutputValues(BinaryReader br, uint stressPeriod, uint timeStep)
        {
            // nodes
            var nodes = br.ReadInt32();

            // skip 1, 1
            br.ReadUInt32();
            br.ReadUInt32();

            var result = new List<MapOutputData>();
            for (var node = 1; node <= nodes; node++)
            {
                double value = br.ReadDouble();
                yield return new MapOutputData
                {
                    Location = node.ToString(),
                    StressPeriod = Convert.ToInt32(stressPeriod),
                    TimeStep = Convert.ToInt32(timeStep),
                    Value = value
                };
            }
        }

        private static Stream GetFileStream(string fileName)
        {
            var path = Path.Combine(ConfigurationHelper.AppSettings.ModflowDataFolder, fileName);
            return new FileStream(path, FileMode.Open, FileAccess.Read);
        }

        protected override Dictionary<Tuple<int, int>, List<string>> CreateSegmentReachZoneDictionary()
        {
            var result = new Dictionary<Tuple<int, int>, List<string>>();
            if (FileExists(ZonesFileName))
            {
                var csvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                    TrimOptions = TrimOptions.Trim
                };
                using var csvReader = new CsvReader(GetFileData(ZonesFileName), csvConfiguration);
                csvReader.Context.RegisterClassMap<SegmentReachZoneItemMapper>();
                csvReader.Read();
                csvReader.ReadHeader();
                while (csvReader.Read())
                {
                    var record = csvReader.GetRecord<SegmentReachZone>();
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

        public override List<string> GetSegmentReachZones(int segment, int reach)
        {
            var key = Tuple.Create(segment, 0);
            if (SegmentReachZoneDictionaryLazy.Value.ContainsKey(key) && SegmentReachZoneDictionaryLazy.Value[key] != null)
            {
                return SegmentReachZoneDictionaryLazy.Value[key];
            }
            return new List<string>();
        }

        private class SegmentReachZoneItemMapper : ClassMap<SegmentReachZone>
        {
            public SegmentReachZoneItemMapper()
            {
                Map(m => m.Seg).Name("Seg");
                Map(m => m.Zone).Name("Zone");
                var allMappedColumns = new[] { "Seg", "Zone" };
            }
        }
    }
}
