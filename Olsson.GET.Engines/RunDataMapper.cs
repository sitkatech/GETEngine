using Olsson.GET.Common.DataContracts.Runs;
using System;
using System.Collections.Generic;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;

namespace Olsson.GET.Engines
{
    public sealed class CanalRunDataMapper : ClassMap<RunCanalInput>
    {
        public CanalRunDataMapper()
        {
            //Adding a column here? add it to the mappedColumns array below
            Map(m => m.Month).Name("Month");
            Map(m => m.Year).Name("Year");
            Map(m => m.Values).Convert(r =>
            {
                var row = r.Row;
                //any column outside our expected values is treated as canal
                //wish we could programatically check which columns are already mapped, couldn't figure it out
                string[] mappedColumns = { "Year", "Month" };
                var columnsInFileNotMapped = row.HeaderRecord == null ? new string[]{} :  row.HeaderRecord.Where(f => !mappedColumns.Contains(f)).ToArray();

                var values = new List<FeatureValue>();

                foreach (var feature in columnsInFileNotMapped)
                {
                    //if we have a value and parseable to an int add it.
                    if (row.TryGetField(feature, out double value))
                    {
                        values.Add(new FeatureValue()
                        {
                            FeatureName = feature,
                            Value = value,
                        });
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(row.GetField(feature))) // not null, not an int, blow up
                        {
                            throw new CsvHelperException(row.Context, $"Error on Row {row}: Unable to read value for column {feature}.");
                        }
                    }
                }

                return values;
            });
        }
    }

    public sealed class WellRunDataMapper : ClassMap<RunWellInput>
    {
        public WellRunDataMapper()
        {
            //Adding a column here? add it to the mappedColumns array below
            Map(m => m.Month).Convert(row =>
            {
                if (row.Row.TryGetField("Date", out DateTime value))
                {
                    return value.Month;
                }

                throw new CsvHelperException(row.Row.Context, $"Error on Row {row.Row}: Unable to read date {row.Row.GetField("Date")}.");
            });
            Map(m => m.Year).Convert(row =>
            {
                if (row.Row.TryGetField("Date", out DateTime value))
                {
                    return value.Year;
                }
                else
                {
                    throw new CsvHelperException(row.Row.Context, $"Error on Row {row.Row}: Unable to read date {row.Row.GetField("Date")}.");
                }
            });

            Map(m => m.Values).Convert(r =>
            {
                var row = r.Row;
                //any column outside our expected values is treated as canal
                //wish we could programatically check which columns are already mapped, couldn't figure it out
                string[] mappedColumns = { "Date" };
                var columnsInFileNotMapped = row.HeaderRecord == null ? new string[]{} : row.HeaderRecord.Where(f => !mappedColumns.Contains(f)).ToArray();

                var values = new List<FeatureWithLocationValue>();

                foreach (var feature in columnsInFileNotMapped)
                {
                    //if we have a value and parseable to an int add it.
                    if (row.TryGetField(feature, out double value))
                    {
                        values.Add(new FeatureWithLocationValue()
                        {
                            Value = value,
                            FeatureName = feature,
                            Lng = 0,
                            Lat = 0
                        });
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(row.GetField(feature))) // not null, not an int, blow up
                        {
                            throw new CsvHelperException(row.Context, $"Error on Row {row}: Unable to read value for column {feature}.");
                        }
                    }
                }

                return values;
            });
        }
    }

    public sealed class RunWellParticleDataMapper : ClassMap<RunWellParticleInput>
    {
        public RunWellParticleDataMapper()
        {
            Map(m => m.Name).Name("Name");
            Map(m => m.Lat).Name("Latitude");
            Map(m => m.Lng).Name("Longitude");
            Map(m => m.ParticleCount).Convert(row =>
            {
                if (row.Row.TryGetField("Particle Count", out int value))
                {
                    if (value is <= 0 or > 32)
                    {
                        throw new CsvHelperException(row.Row.Context, $"Error on Row {row.Row}: Particle Count must be between 1 and 32.");
                    }
                    return value;
                }
                else
                {
                    throw new CsvHelperException(row.Row.Context, $"Error on Row {row.Row}: Unable to read value for column Particle Count.");
                }
            });
        }
    }
}
