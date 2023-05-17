using CsvHelper;
using CsvHelper.Configuration;
using log4net;
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
    internal abstract class ModflowSixFileAccessor : ModelFileAccessor
    {
        protected const string StressPeriodFileKey = "TDIS6";
        private const string SfrFileKey = "SFR6";
        private const string WelFileKey = "WEL6";

        private const string RunZoneBudgetFileName = "zbud.csv";
        protected const string ListFileOutputFileName = "mfsim.lst";

        private static readonly Regex ModFlowSixNamFileRegex = new Regex(@"^\s+GWF6\s+?(?<namefile>\S+)\s+?(?<modelname>\S+)$");
        private static readonly Regex ModFlowSixFileNameRegex = new Regex(@"^\s+(?<key>(\S+?))\s+?(?<fileName>\S+)$");
        private static readonly Regex SfrReachNumber = new Regex(@"^\s+NREACHES\s+(?<reachCount>\d+)\s*$", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);
        
        private static readonly ILog Logger = Logging.GetLogger(typeof(ModflowSixFileAccessor));

        protected ModflowSixFileAccessor(Model model) : base(model)
        {

        }

        protected override Dictionary<string, List<string>> CreateModflowFileNamesDictionary()
        {
            // todo: need to revisit this, but right now the NamFileName is the ModelExecutables ordered by RunOrder first
            return CreateModflowFileNamesDictionary(Model.ModelExecutables.OrderBy(x => x.RunOrder).First().Arguments);
        }

        internal static new Dictionary<string, List<string>> CreateModflowFileNamesDictionary(string namFileName)
        {
            var result = new Dictionary<string, List<string>>();
            var fileLines = GetModelFileLines(namFileName);

            using (var fileLineEnumerator = fileLines.GetEnumerator())
            {
                while (fileLineEnumerator.MoveNext())
                {
                    if (fileLineEnumerator.Current.IndexOf("BEGIN TIMING", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        ParseTimingFileName(fileLineEnumerator, result);
                    }

                    var match = ModFlowSixNamFileRegex.Match(fileLineEnumerator.Current);
                    if (match.Success)
                    {
                        var nameFileLines = GetModelFileLines(match.Groups["namefile"].Value);
                        foreach (var nameFileLine in nameFileLines)
                        {
                            var file = ModFlowSixFileNameRegex.Match(nameFileLine);
                            if (file.Success)
                            {
                                var key = file.Groups["key"].Value;
                                var fileName = file.Groups["fileName"].Value;
                                if (!result.ContainsKey(key))
                                {
                                    result.Add(key, new List<string>());
                                }
                                result[key].Add(fileName);
                            }
                        }
                    }
                }
            }

            return result;
        }

        private static void ParseTimingFileName(IEnumerator<string> fileLineEnumerator, Dictionary<string, List<string>> fileDictionary)
        {
            while (fileLineEnumerator.MoveNext())
            {
                if (fileLineEnumerator.Current.IndexOf("END TIMING", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    break;
                }

                var data = fileLineEnumerator.Current.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                var key = data[0];
                var fileName = data[1];

                if (!fileDictionary.ContainsKey(key))
                {
                    fileDictionary.Add(key, new List<string>());
                }
                fileDictionary[key].Add(fileName);
            }
        }

        public override int GetNumberOfSegmentReaches()
        {
            var fileLines = GetModelFileLines(GetFileName(SfrFileKey));
            using (var fileLineEnumerator = fileLines.GetEnumerator())
            {
                while (fileLineEnumerator.MoveNext())
                {
                    var match = SfrReachNumber.Match(fileLineEnumerator.Current);
                    if (match.Success)
                    {
                        return int.Parse(match.Groups["reachCount"].Value);
                    }
                }
            }

            throw new Exception("Encountered an error while parsing segment flow reaches: Invalid SFR file, cannot reach NREACHES");
        }

        public override IEnumerable<OutputData> GetOutputData()
        {
            return GetProcessedData(Model.ListFileName);
        }

        public override IEnumerable<OutputData> GetBaselineData()
        {
            return GetProcessedData(BaselineListFileName);
        }

        private IEnumerable<OutputData> GetProcessedData(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName) || !FileExists(fileName))
            {
                return null;
            }

            var regexForBaseflowTableIndicator = new Regex($@"{Model.BaseflowTableProcessingConfiguration.BaseflowTableIndicatorRegexPattern}");

            var fileLines = GetModelFileLines(fileName);
            var result = new List<OutputData>();

            var numSegmentReaches = GetNumberOfSegmentReaches();

            using (var fileLineEnumerator = fileLines.GetEnumerator())
            {
                while (fileLineEnumerator.MoveNext())
                {
                    var match = regexForBaseflowTableIndicator.Match(fileLineEnumerator.Current);
                    if (match.Success)
                    {
                        AddLocationFlowRate(fileLineEnumerator, result, numSegmentReaches);
                    }
                }

                return result;
            }
        }

        private void AddLocationFlowRate(IEnumerator<string> fileLineEnumerator, List<OutputData> outputData,
            int numSegmentReaches)
        {
            for (var i = 0; i < 4; i++)
            {
                fileLineEnumerator.MoveNext();
            }

            var segmentColumnNum = Model.BaseflowTableProcessingConfiguration.SegmentColumnNum - 1;
            var flowToAquiferColumnNum = Model.BaseflowTableProcessingConfiguration.FlowToAquiferColumnNum - 1;

            var currentSegmentNum = 0;

            while (fileLineEnumerator.MoveNext() && currentSegmentNum < numSegmentReaches)
            {
                if (string.IsNullOrWhiteSpace(fileLineEnumerator.Current))
                {
                    break;
                }

                var data = fileLineEnumerator.Current.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                currentSegmentNum = int.Parse(data[segmentColumnNum]);
                // For the sake of analysis, we display the negative of MF6's output to the user (FlowToAquifer is multiplied by -1)
                outputData.Add(new OutputData
                {
                    //No reach and Segment in Modflow6, only value we need is being set in SegmentNumber
                    ReachNumber = 0,
                    SegmentNumber = currentSegmentNum,
                    FlowToAquifer = double.Parse(data[flowToAquiferColumnNum]) * -1
                });
            }
        }

        public override IEnumerable<string> GetListFileOutputFileLines()
        {
            return GetModelFileLines(ListFileOutputFileName);
        }

        public override IEnumerable<string> GetRunListFileLines()
        {
            return GetModelFileLines(Model.ListFileName);
        }

        public override IEnumerable<ZoneBudgetItem> GetBaselineZoneBudgetItems(List<AsrDataMap> asrData)
        {
            return GetZoneBudgetItems(BaselineZoneBudgetFileName, asrData);
        }

        public override IEnumerable<ZoneBudgetItem> GetRunZoneBudgetItems(List<AsrDataMap> asrData)
        {
            return GetZoneBudgetItems(RunZoneBudgetFileName, asrData);
        }

        public override IEnumerable<ZoneBudgetItem> GetZoneBudgetItems(string fileName, List<AsrDataMap> asrData)
        {
            if (!FileExists(fileName))
            {
                return null;
            }

            var result = ReadZoneBudgetItemsFile(fileName).ToList();
            var loggedASRValues = new HashSet<string>();

            for (var i = 0; i < result.Count; i++)
            {
                var item = result[i];
                var mappedValues = new List<ZoneBudgetValue>();

                foreach (var asrItem in asrData)
                {
                    var mappedItemsIn = item.Values.Where(x => Contains(asrItem.Key + "-IN", x.Key, StringComparison.OrdinalIgnoreCase)).ToList();
                    var mappedItemsOut = item.Values.Where(x => Contains(asrItem.Key + "-OUT", x.Key, StringComparison.OrdinalIgnoreCase)).ToList();

                    if (mappedItemsIn.Any() && mappedItemsOut.Any())
                    {
                        // The zone budget output engine expects 'in' and 'out' to exist in the list with the same key, with 'in' appearing first
                        // We map here to preserve backwards compatibility with modflow 2005
                        mappedValues.Add(new ZoneBudgetValue()
                        {
                            Key = asrItem.Key,
                            Value = mappedItemsIn.Sum(x => x.Value)
                        });

                        mappedValues.Add(new ZoneBudgetValue()
                        {
                            Key = asrItem.Key,
                            Value = mappedItemsOut.Sum(x => x.Value)
                        });
                    }
                    else if (!loggedASRValues.Contains(asrItem.Key))
                    {
                        Logger.Debug($"@ [{item.Period}-{item.Step}-{item.Zone}] - Unable to find ASR item key [{asrItem.Key}].  Available values - [{string.Join(",", item.Values.Select(a => a.Key))}]");
                        loggedASRValues.Add(asrItem.Key);
                    }
                }

                item.Values = mappedValues;
            }

            return result;
        }

        private static bool Contains(string subset, string set, StringComparison comp)
        {
            return subset != null && set != null && set.IndexOf(subset, comp) >= 0;
        }

        protected override IEnumerable<ZoneBudgetItem> ReadZoneBudgetItemsFile(string fileName)
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

        public override StressPeriodsLocationRates GetLocationRates()
        {
            var fileLines = GetModelFileLines(GetFileName(WelFileKey)).ToList();
            var result = new StressPeriodsLocationRates
            {
                Parameters = new List<string>(),
                StressPeriods = new List<StressPeriodLocationRates>()
            };

            using (var fileLineEnumerator = fileLines.GetEnumerator())
            {
                while (fileLineEnumerator.MoveNext())
                {
                    if (fileLineEnumerator.Current.Equals("begin options", StringComparison.OrdinalIgnoreCase))
                    {
                        while (fileLineEnumerator.MoveNext() && !fileLineEnumerator.Current.Equals("end options", StringComparison.OrdinalIgnoreCase))
                        {
                            var currOption = fileLineEnumerator.Current?.Trim();
                            if (!string.IsNullOrEmpty(currOption))
                            {
                                result.Parameters.Add(currOption);
                            }
                        }
                        continue;
                    }

                    if (fileLineEnumerator.Current.Equals("begin dimensions", StringComparison.OrdinalIgnoreCase))
                    {
                        fileLineEnumerator.MoveNext();
                        result.HeaderValue = FileFormatter.ParseWelFileHeaderData(fileLineEnumerator.Current);
                        continue;
                    }

                    if (fileLineEnumerator.Current.IndexOf("begin period", StringComparison.OrdinalIgnoreCase) < 0)
                    {
                        continue;
                    }

                    // If no stress period is specified (e.g. The previous period is 3, and the current stress period is 5), fill the intervening stress periods with identical location rates
                    var currStressPeriod = int.Parse(fileLineEnumerator.Current.Substring(13));
                    if (result.StressPeriods.Count != currStressPeriod - 1)
                    {
                        for (var i = result.StressPeriods.Count; i < currStressPeriod - 1; i++)
                        {
                            result.StressPeriods.Add(result.StressPeriods[i - 1]);
                        }
                    }

                    var currStressPeriodRates = new StressPeriodLocationRates
                    {
                        LocationRates = new List<LocationRate>()
                    };

                    AddRateData(fileLineEnumerator, currStressPeriodRates.LocationRates);

                    result.StressPeriods.Add(currStressPeriodRates);
                }
                return result;
            }
        }

        /// <summary>
        /// This reads the well rate date from the begin period end period blocks
        /// </summary>
        /// <param name="fileLineEnumerator"></param>
        /// <param name="rates"></param>
        private void AddRateData(IEnumerator<string> fileLineEnumerator, List<LocationRate> rates)
        {
            while (fileLineEnumerator.MoveNext())
            {
                if (fileLineEnumerator.Current.IndexOf("end period", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    break;
                }

                var data = FileFormatter.ParseLocationRateData(fileLineEnumerator.Current);
                rates.Add(new LocationRate
                {
                    Location = data.Location,
                    Rate = data.Rate
                });
            }
        }

        public override void UpdateLocationRates(StressPeriodsLocationRates stressPeriods)
        {
            var welFileFullPath = Path.Combine(ConfigurationHelper.AppSettings.ModflowDataFolder, GetFileName(WelFileKey));
            using (var sw = new StreamWriter(new FileStream(welFileFullPath, FileMode.Create, FileAccess.Write)))
            {
                if (stressPeriods.StressPeriods == null)
                {
                    stressPeriods.StressPeriods = new List<StressPeriodLocationRates>();
                }

                sw.WriteLine("begin options");
                foreach (var parameter in stressPeriods.Parameters)
                {
                    sw.WriteLine("  " + parameter);
                }
                sw.WriteLine("end options\n");

                var maxCount = stressPeriods.StressPeriods.Select(a => (a.LocationRates?.Count ?? 0) + (a.ClnLocationRates?.Count ?? 0)).Max();
                WriteTotalHeaderRow(sw, maxCount, stressPeriods.HeaderValue);

                for (var i = 0; i < stressPeriods.StressPeriods.Count; i++)
                {
                    if (stressPeriods.StressPeriods[i].LocationRates == null)
                    {
                        stressPeriods.StressPeriods[i].LocationRates = new List<LocationRate>();
                    }

                    sw.WriteLine($"begin period {i + 1}");

                    foreach (var locationRate in stressPeriods.StressPeriods[i].LocationRates)
                    {
                        WriteLocationRateRow(sw, locationRate);
                    }

                    if (stressPeriods.StressPeriods[i].ClnLocationRates != null)
                    {
                        foreach (var locationRate in stressPeriods.StressPeriods[i].ClnLocationRates)
                        {
                            WriteLocationRateRow(sw, locationRate);
                        }
                    }

                    sw.WriteLine("end period\n");
                }
            }
        }

        private class ZoneBudgetItemMapper : ClassMap<ZoneBudgetItem>
        {
            public ZoneBudgetItemMapper()
            {
                Map(m => m.Step).Name("kstp");
                Map(m => m.Period).Name("kper");
                Map(m => m.Zone).Name("zone");
                var allMappedColumns = new[] { "kstp", "kper", "zone" };
                var loggedFoundHeaders = false;
                Map(m => m.Values).ConvertUsing(r =>
                {
                    var row = (CsvHelper.CsvReader)r;
                    var values = new List<ZoneBudgetValue>();

                    if (!loggedFoundHeaders)
                    {
                        Logger.Debug($"Found zone budget item headers [{string.Join(",", row.Context.HeaderRecord)}]");
                        loggedFoundHeaders = true;
                    }
                    for (var i = 0; i < row.Context.HeaderRecord.Length; i++)
                    {
                        var header = row.Context.HeaderRecord[i];
                        if (!allMappedColumns.Contains(header.ToLower()) && double.TryParse(row.GetField(i), out var value))
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
    }
}
