using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Olsson.GET.Common.DataContracts.Runs;
using CsvHelper.TypeConversion;
using Olsson.GET.Common.Utilities;
using CsvHelper;
using System.IO;
using CsvHelper.Configuration;
using log4net;
using Olsson.GET.Accessors.Models;
using Olsson.GET.Common.DataContracts.Models;
using System.Data;
using Olsson.GET.Accessors.FileIO;

namespace Olsson.GET.Engines.RunDataParse
{
    public class RunDataParseEngine : BaseEngine, IRunDataParseEngine
    {
        private static readonly ILog Logger = Logging.GetLogger(typeof(RunDataParseEngine));

        public RunCanalInputParseResult ParseCanalRunDataFromFile(byte[] data, Model model)
        {
            Logger.Info("Parsing raw survey data");

            List<RunCanalInput> records = null;
            List<string> errors = new List<string>();
            try
            {
                using (TextReader tr = new StreamReader(new MemoryStream(data), Encoding.UTF8))
                {
                    using (var reader = new CsvReader(tr))
                    {
                        reader.Configuration.RegisterClassMap<CanalRunDataMapper>();

                        reader.Configuration.TrimOptions = TrimOptions.Trim;

                        records = reader.GetRecords<RunCanalInput>().ToList();
                    }

                    var canalNames = model.CanalData?.Split(',').Where(a => !string.IsNullOrWhiteSpace(a)).Select(a => a.Trim()).ToList() ?? new List<string>();
                    var isFirst = true;
                    for (var i = 0; i < records.Count; i++)
                    {
                        var record = records[i];
                        if (isFirst)
                        {
                            var invalidColumnNames = record.Values.Where(a => !canalNames.Contains(a.FeatureName.Trim()));
                            foreach (var invalidColumnName in invalidColumnNames)
                            {
                                errors.Add($"Invalid Column Name - Record #{i + 1} {invalidColumnName.FeatureName}");
                            }
                            isFirst = false;
                        }
                        ValidateDates(model, record.Year, record.Month, errors, i);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                errors.Add($"Error when parsing data: {ex.Message}.");
            }

            return new RunCanalInputParseResult()
            {
                RunInputs = records,
                Success = errors.Count == 0,
                Errors = errors,
            };
        }

        private static void ValidateDates(Model model, int year, int month, List<string> errors, int i)
        {
            var date = new DateTime(year, month, 1);
            var lastStressPeriodStartDate =
                model.ModelStressPeriodCustomStartDates != null && model.ModelStressPeriodCustomStartDates.Any()
                    ? model.ModelStressPeriodCustomStartDates[model.NumberOfStressPeriods - 1].StressPeriodStartDate
                    : model.StartDateTime.AddMonths(model.NumberOfStressPeriods - 1);
            if (date < model.StartDateTime)
            {
                errors.Add($"Invalid Date - Before Start Date: Record #{i + 1} - Start Date: {model.StartDateTime.Month}/{model.StartDateTime.Year} - Record Date: {month}/{year} ");
            }
            if (date > lastStressPeriodStartDate)
            {
                errors.Add($"Invalid Date - After End Date: Record #{i + 1} - End Date: {lastStressPeriodStartDate.Month}/{lastStressPeriodStartDate.Year} - Record Date: {month}/{year} ");
            }
        }

        public byte[] CanalRunDataToCsv(List<RunCanalInput> data)
        {
            using (var memoryStream = new MemoryStream())
            using (var streamWriter = new StreamWriter(memoryStream))
            using (var csvWriter = new CsvWriter(streamWriter))
            {
                //header row
                csvWriter.WriteField("Month");
                csvWriter.WriteField("Year");

                var fields = data.First().Values.Select(v => v.FeatureName).ToList();

                foreach (var field in fields)
                {
                    csvWriter.WriteField(field);
                }

                csvWriter.NextRecord();

                //rows
                foreach (var record in data)
                {
                    csvWriter.WriteField(record.Month);
                    csvWriter.WriteField(record.Year);

                    foreach (var field in fields)
                    {
                        csvWriter.WriteField(record.Values.First(v => v.FeatureName == field).Value);
                    }

                    csvWriter.NextRecord();
                }

                streamWriter.Flush();
                return memoryStream.ToArray();
            }

        }

        public RunWellInputParseResult ParseWellRunDataFromFile(byte[] data, Model model)
        {
            Logger.Info("Parsing raw survey data");

            List<RunWellInput> records = null;
            List<string> errors = new List<string>();
            
            try
            {
                using (TextReader tr = new StreamReader(new MemoryStream(data), Encoding.UTF8))
                {
                    using (var reader = new CsvReader(tr))
                    {
                        reader.Configuration.RegisterClassMap<WellRunDataMapper>();
                        reader.Configuration.TrimOptions = TrimOptions.Trim;

                        records = new List<RunWellInput>();

                        //header
                        reader.Read();
                        reader.ReadHeader();

                        ////////////////////////////////////////////////////////////////sb 12/11/2018 BEGIN identify duplicate wells          
                        //well
                        var wells = new List<string>();
                        var x = 1;
                        while (reader.TryGetField<string>(x, out var well))
                        {
                            wells.Add(well);
                            x++;
                        }

                        var duplicate = wells
                            .GroupBy(z => z)
                            .Where(g => g.Count() > 1)
                            .Select(g => g.Key);
                        foreach (var d in duplicate)
                        {
                            if (!String.IsNullOrEmpty(d.ToString()))
                            {
                                throw new Exception("Duplicate Wells found: " + (d) + ", please update the csv file");
                            }
                        }
                        ////////////////////////////////////////////////////////////sb 12/11/2018 end

                        //lat
                        reader.Read();
                        var lats = new List<double>();
                        var i = 1;
                        while (reader.TryGetField<double>(i, out var lat))
                        {
                            lats.Add(lat);
                            i++;
                        }

                        //lng
                        reader.Read();
                        var lngs = new List<double>();
                        i = 1;
                        while (reader.TryGetField<double>(i, out var lng))
                        {
                            lngs.Add(lng);
                            i++;
                        }

                        while (reader.Read())
                        {
                            records.Add(reader.GetRecord<RunWellInput>());
                        }

                        //populate the lat/long
                        foreach (var record in records)
                        {
                            var recordIndex = 0;
                            foreach (var well in record.Values)
                            {
                                well.Lat = lats[recordIndex];
                                well.Lng = lngs[recordIndex];
                                recordIndex++;
                            }
                        }
                    }
                }

                for (var i = 0; i < records.Count; i++)
                {
                    var record = records[i];
                    ValidateDates(model, record.Year, record.Month, errors, i);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                errors.Add($"Error when parsing data: {ex.Message}.");
            }


            return new RunWellInputParseResult()
            {
                RunInputs = records,
                Success = errors.Count == 0,
                Errors = errors,
            };
        }

        public byte[] WellRunDataToCsv(List<RunWellInput> data)
        {
            using (var memoryStream = new MemoryStream())
            using (var streamWriter = new StreamWriter(memoryStream))
            using (var csvWriter = new CsvWriter(streamWriter))
            {
                //header row
                csvWriter.WriteField("Date");

                var wells = data.First().Values.Select(sp => sp.FeatureName).Distinct().ToList();

                foreach (var field in wells)
                {
                    csvWriter.WriteField(field);
                }

                //Lat row
                csvWriter.NextRecord();
                csvWriter.WriteField(""); //empty row under date
                foreach (var field in wells)
                {
                    csvWriter.WriteField(data.First().Values.First(v => v.FeatureName == field).Lat);
                }

                //Lng row
                csvWriter.NextRecord();
                csvWriter.WriteField(""); //empty row under date
                foreach (var field in wells)
                {
                    csvWriter.WriteField(data.First().Values.First(v => v.FeatureName == field).Lng);
                }

                csvWriter.NextRecord();

                //rows
                foreach (var record in data)
                {
                    csvWriter.WriteField(new DateTime(record.Year, record.Month, 1).ToString("MM/dd/yyyy"));

                    foreach (var well in wells)
                    {
                        csvWriter.WriteField(record.Values.FirstOrDefault(r => r.FeatureName == well)?.Value);
                    }

                    csvWriter.NextRecord();
                }

                streamWriter.Flush();
                return memoryStream.ToArray();
            }
        }

        public byte[] WellParticleRunDataToCsv(List<RunWellParticleInput> data)
        {
            using (var memoryStream = new MemoryStream())
            using (var streamWriter = new StreamWriter(memoryStream))
            using (var csvWriter = new CsvWriter(streamWriter))
            {
                //header row
                csvWriter.WriteField("Name");
                csvWriter.WriteField("Latitude");
                csvWriter.WriteField("Longitude");
                csvWriter.WriteField("Particle Count");


                csvWriter.NextRecord();

                //rows
                foreach (var record in data)
                {
                    csvWriter.WriteField(record.Name);
                    csvWriter.WriteField(record.Lat);
                    csvWriter.WriteField(record.Lng);
                    csvWriter.WriteField(record.ParticleCount);

                    csvWriter.NextRecord();
                }

                streamWriter.Flush();
                return memoryStream.ToArray();
            }
        }

        public RunWellParticleInputParseResult ParseWellParticleRunDataFromFile(byte[] data, Model model)
        {
            Logger.Info("Parsing raw survey data");

            List<RunWellParticleInput> records = null;
            List<string> errors = new List<string>();

            try
            {
                using (TextReader tr = new StreamReader(new MemoryStream(data), Encoding.UTF8))
                {
                    using (var reader = new CsvReader(tr))
                    {
                        reader.Configuration.RegisterClassMap<RunWellParticleDataMapper>();

                        reader.Configuration.TrimOptions = TrimOptions.Trim;

                        records = reader.GetRecords<RunWellParticleInput>().ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                errors.Add($"Error when parsing data: {ex.Message}.");
            }

            return new RunWellParticleInputParseResult()
            {
                RunInputs = records,
                Success = errors.Count == 0,
                Errors = errors,
            };
        }
    }
}
