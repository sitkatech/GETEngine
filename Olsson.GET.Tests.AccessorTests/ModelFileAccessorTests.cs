using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Olsson.GET.Accessors.FileIO;
using Olsson.GET.Common.DataContracts.Runs;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.SqlServer;
using System.IO;
using System.Linq;
using Olsson.GET.Accessors.EntityFramework;
using BaseflowTableProcessingConfiguration = Olsson.GET.Common.DataContracts.Models.BaseflowTableProcessingConfiguration;
using Model = Olsson.GET.Common.DataContracts.Models.Model;
using ModelExecutable = Olsson.GET.Common.DataContracts.Models.ModelExecutable;

namespace Olsson.GET.Tests.AccessorTests
{
    [TestClass]
    public class ModelFileAccessorFactoryTests
    {
        [TestMethod]
        public void CreateModflowFileAccessor_IsStructured_IsModFlow6()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = $@"ModflowTestFiles\CreateModflowFileAccessor\StructuredModFlow6";

            var result = new ModelFileAccessorFactory().CreateModflowFileAccessor(new Model
            {
                ModelEngineTypeID = (int)ModelEngineTypeEnum.Modflow6,
                ModelGridTypeID = (int)ModelGridTypeEnum.Structured,
                ModelExecutables = new List<ModelExecutable>
                {
                    new ModelExecutable
                    {
                        Arguments = "mfsim.nam",
                    }
                }
            });
            Assert.IsInstanceOfType(result, typeof(StructuredModflowSixFileAccessor));
        }

        [TestMethod]
        public void CreateModflowFileAccessor_IsStructured_IsModFlow2005()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = $@"ModflowTestFiles\CreateModflowFileAccessor\StructuredModFlow2005";

            var result = new ModelFileAccessorFactory().CreateModflowFileAccessor(new Model
            {
                ModelEngineTypeID = (int) ModelEngineTypeEnum.Modflow,
                ModelGridTypeID = (int) ModelGridTypeEnum.Structured,
                ModelExecutables = new List<ModelExecutable>
                {
                    new ModelExecutable
                    {
                        Arguments = "test.nam",
                    }
                }
            });
            Assert.IsInstanceOfType(result, typeof(StructuredModflowFileAccessor));
        }

        [TestMethod]
        public void CreateModflowFileAccessor_IsUnstructured_IsModFlow6()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = $@"ModflowTestFiles\CreateModflowFileAccessor\UnstructuredModFlow6";

            var result = new ModelFileAccessorFactory().CreateModflowFileAccessor(new Model
            {
                ModelEngineTypeID = (int)ModelEngineTypeEnum.Modflow6,
                ModelGridTypeID = (int)ModelGridTypeEnum.Unstructured,
                ModelExecutables = new List<ModelExecutable>
                {
                    new ModelExecutable
                    {
                        Arguments = "mfsim.nam",
                    }
                }
            });
            Assert.IsInstanceOfType(result, typeof(UnstructuredModflowSixFileAccessor));
        }

        [TestMethod]
        public void CreateModflowFileAccessor_IsUnstructured_IsModFlow2005()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = $@"ModflowTestFiles\CreateModflowFileAccessor\UnstructuredModFlow2005";

            var result = new ModelFileAccessorFactory().CreateModflowFileAccessor(new Model
            {
                ModelEngineTypeID = (int)ModelEngineTypeEnum.Modflow,
                ModelGridTypeID = (int)ModelGridTypeEnum.Unstructured,
                ModelExecutables = new List<ModelExecutable>
                {
                    new ModelExecutable
                    {
                        Arguments = "test.nam",
                    }
                }
            });
            Assert.IsInstanceOfType(result, typeof(UnstructuredModflowFileAccessor));
        }
    }

    [TestClass]
    public class StructuredModflowFileAccessorTests : ModelFileAccessorTests
    {
        internal override ModelFileAccessor CreateModelFileAccessor(Model model)
        {
            return new Olsson.GET.Accessors.FileIO.StructuredModflowFileAccessor(model);
        }

        internal override string ModflowFileType => "Structured";
        internal override BaseflowTableProcessingConfiguration BaseflowTableProcessingConfiguration => new BaseflowTableProcessingConfiguration()
        {
            BaseflowTableProcessingConfigurationID = 1,
            BaseflowTableIndicatorRegexPattern = @"^\s+STREAM LISTING\s+PERIOD\s+[0-9]+\s+STEP\s+[0-9]+$",
            SegmentColumnNum = 4,
            FlowToAquiferColumnNum = 7,
            ReachColumnNum = 5
        };

        [TestMethod]
        public void UpdateLocationRates_BasicTest()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = $@"ModflowTestFiles\{ModflowFileType}\UpdateLocationRates\BasicTest";
            var sut = CreateModelFileAccessor(new Model
            {
                ModelExecutables = new List<ModelExecutable>
                {
                    new ModelExecutable
                    {
                        Arguments = "Name.nam",
                    }
                }
            });

            sut.UpdateLocationRates(new StressPeriodsLocationRates
            {
                HeaderValue = "        50",
                StressPeriods = new List<StressPeriodLocationRates>
                {
                    new StressPeriodLocationRates
                    {
                        Flag = 0,
                        LocationRates = new List<LocationRate>
                        {
                            new LocationRate{Location = "11|111|1111", Rate = 123.45},
                            new LocationRate{Location = "22|222|2222", Rate = 456.12}
                        }
                    },
                    new StressPeriodLocationRates
                    {
                        Flag = 0,
                        LocationRates = new List<LocationRate>()
                    },
                    new StressPeriodLocationRates
                    {
                        Flag = 0,
                        LocationRates = new List<LocationRate>
                        {
                            new LocationRate{Location = "11|111|1111", Rate = 222.33},
                            new LocationRate{Location = "22|222|2222", Rate = 1.11},
                            new LocationRate{Location = "3|3|3", Rate = 33.22},
                            new LocationRate{Location = "2|2|2", Rate=0},
                            new LocationRate{Location = "2|2|2", Rate=-999999999},
                            new LocationRate{Location = "2|2|2", Rate=-1000000000},
                            new LocationRate{Location = "2|2|2", Rate=-999999999.0},
                            new LocationRate{Location = "2|2|2", Rate=-999999999.9},
                            new LocationRate{Location = "2|2|2", Rate=9999999999},
                            new LocationRate{Location = "2|2|2", Rate=10000000000},
                            new LocationRate{Location = "2|2|2", Rate=9999999999.0},
                            new LocationRate{Location = "2|2|2", Rate=9999999999.9},
                        }
                    }
                }

            });
            using (var fileLineEnumerator = File.ReadLines($@"ModflowTestFiles\{ModflowFileType}\UpdateLocationRates\BasicTest\Well.WEL").GetEnumerator())
            {
                //header
                Assert.IsTrue(fileLineEnumerator.MoveNext());
                Assert.AreEqual("        12        50", fileLineEnumerator.Current);

                //stress period 1
                Assert.IsTrue(fileLineEnumerator.MoveNext());
                Assert.AreEqual("         2         0", fileLineEnumerator.Current);

                Assert.IsTrue(fileLineEnumerator.MoveNext());
                Assert.AreEqual("        11       111      1111123.450000", fileLineEnumerator.Current);

                Assert.IsTrue(fileLineEnumerator.MoveNext());
                Assert.AreEqual("        22       222      2222456.120000", fileLineEnumerator.Current);

                //stress period 2
                Assert.IsTrue(fileLineEnumerator.MoveNext());
                Assert.AreEqual("         0         0", fileLineEnumerator.Current);

                //stress period 3
                Assert.IsTrue(fileLineEnumerator.MoveNext());
                Assert.AreEqual("        12         0", fileLineEnumerator.Current);

                Assert.IsTrue(fileLineEnumerator.MoveNext());
                Assert.AreEqual("        11       111      1111222.330000", fileLineEnumerator.Current);

                Assert.IsTrue(fileLineEnumerator.MoveNext());
                Assert.AreEqual("        22       222      22221.11000000", fileLineEnumerator.Current);

                Assert.IsTrue(fileLineEnumerator.MoveNext());
                Assert.AreEqual("         3         3         333.2200000", fileLineEnumerator.Current);

                Assert.IsTrue(fileLineEnumerator.MoveNext());
                Assert.AreEqual("         2         2         20.00000000", fileLineEnumerator.Current);

                Assert.IsTrue(fileLineEnumerator.MoveNext());
                Assert.AreEqual("         2         2         2-999999999", fileLineEnumerator.Current);

                Assert.IsTrue(fileLineEnumerator.MoveNext());
                Assert.AreEqual("         2         2         2-1.00e+009", fileLineEnumerator.Current);

                Assert.IsTrue(fileLineEnumerator.MoveNext());
                Assert.AreEqual("         2         2         2-999999999", fileLineEnumerator.Current);

                Assert.IsTrue(fileLineEnumerator.MoveNext());
                Assert.AreEqual("         2         2         2-1.00e+009", fileLineEnumerator.Current);

                Assert.IsTrue(fileLineEnumerator.MoveNext());
                Assert.AreEqual("         2         2         29999999999", fileLineEnumerator.Current);

                Assert.IsTrue(fileLineEnumerator.MoveNext());
                Assert.AreEqual("         2         2         21.000e+010", fileLineEnumerator.Current);

                Assert.IsTrue(fileLineEnumerator.MoveNext());
                Assert.AreEqual("         2         2         29999999999", fileLineEnumerator.Current);

                Assert.IsTrue(fileLineEnumerator.MoveNext());
                Assert.AreEqual("         2         2         21.000e+010", fileLineEnumerator.Current);
            }
        }

        [TestMethod]
        public void UpdateLocationRates_HasClnWellGroup()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = $@"ModflowTestFiles\{ModflowFileType}\UpdateLocationRates\BasicTest";
            var sut = CreateModelFileAccessor(new Model
            {
                ModelExecutables = new List<ModelExecutable>
                {
                    new ModelExecutable
                    {
                        Arguments = "Name.nam",
                    }
                }
            });

            sut.UpdateLocationRates(new StressPeriodsLocationRates
            {
                HeaderValue = "        50",
                StressPeriods = new List<StressPeriodLocationRates>
                {
                    new StressPeriodLocationRates
                    {
                        Flag = 0,
                        LocationRates = new List<LocationRate>
                        {
                            new LocationRate{Location = "11|111|1111", Rate = 123.45},
                            new LocationRate{Location = "22|222|2222", Rate = 456.12}
                        },
                        ClnLocationRates = new List<LocationRate>
                        {
                            new LocationRate{Location = "33|333|3333", Rate = 987.65}
                        }
                    }
                }

            });
            using (var fileLineEnumerator = File.ReadLines($@"ModflowTestFiles\{ModflowFileType}\UpdateLocationRates\BasicTest\Well.WEL").GetEnumerator())
            {
                //header
                Assert.IsTrue(fileLineEnumerator.MoveNext());
                Assert.AreEqual("         3        50", fileLineEnumerator.Current);

                //stress period 1
                Assert.IsTrue(fileLineEnumerator.MoveNext());
                Assert.AreEqual("         2         0         1", fileLineEnumerator.Current);

                Assert.IsTrue(fileLineEnumerator.MoveNext());
                Assert.AreEqual("        11       111      1111123.450000", fileLineEnumerator.Current);

                Assert.IsTrue(fileLineEnumerator.MoveNext());
                Assert.AreEqual("        22       222      2222456.120000", fileLineEnumerator.Current);

                Assert.IsTrue(fileLineEnumerator.MoveNext());
                Assert.AreEqual("        33       333      3333987.650000", fileLineEnumerator.Current);
            }
        }

        [TestMethod]
        public void GetLocationRates_BasicTest()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = $@"ModflowTestFiles\{ModflowFileType}\GetLocationRates\BasicTest";
            var sut = CreateModelFileAccessor(new Model
            {
                ModelExecutables = new List<ModelExecutable>
                {
                    new ModelExecutable
                    {
                        Arguments = "Name.nam",
                    }
                }
            });

            var result = sut.GetLocationRates();
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Parameters.Count);
            Assert.AreEqual(2, result.StressPeriods.Count);
            Assert.AreEqual("        50", result.HeaderValue);

            Assert.AreEqual(2, result.StressPeriods[0].LocationRates.Count);
            Assert.AreEqual(0, result.StressPeriods[0].Flag);
            Assert.IsNull(result.StressPeriods[0].ClnLocationRates);
            Assert.AreEqual("11|111|1111", result.StressPeriods[0].LocationRates[0].Location);
            Assert.AreEqual(8.1506e+02, result.StressPeriods[0].LocationRates[0].Rate);
            Assert.AreEqual("22|222|2222", result.StressPeriods[0].LocationRates[1].Location);
            Assert.AreEqual(5.7547e+02, result.StressPeriods[0].LocationRates[1].Rate);

            Assert.AreEqual(1, result.StressPeriods[1].LocationRates.Count);
            Assert.AreEqual(0, result.StressPeriods[1].Flag);
            Assert.IsNull(result.StressPeriods[1].ClnLocationRates);
            Assert.AreEqual("11|111|1111", result.StressPeriods[1].LocationRates[0].Location);
            Assert.AreEqual(4.7291e+03, result.StressPeriods[1].LocationRates[0].Rate);
        }

        [TestMethod]
        public void GetLocationRates_MultipleWellGroups()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = $@"ModflowTestFiles\{ModflowFileType}\GetLocationRates\MultipleWellGroups";
            var sut = CreateModelFileAccessor(new Model
            {
                ModelExecutables = new List<ModelExecutable>
                {
                    new ModelExecutable
                    {
                        Arguments = "Name.nam",
                    }
                }
            });

            var result = sut.GetLocationRates();
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Parameters.Count);
            Assert.AreEqual(2, result.StressPeriods.Count);
            Assert.AreEqual("        50", result.HeaderValue);

            //stress period 1
            Assert.AreEqual(0, result.StressPeriods[0].Flag);
            Assert.AreEqual(2, result.StressPeriods[0].LocationRates.Count);
            Assert.AreEqual("11|111|1111", result.StressPeriods[0].LocationRates[0].Location);
            Assert.AreEqual(8.1506e+02, result.StressPeriods[0].LocationRates[0].Rate);
            Assert.AreEqual("22|222|2222", result.StressPeriods[0].LocationRates[1].Location);
            Assert.AreEqual(5.7547e+02, result.StressPeriods[0].LocationRates[1].Rate);

            Assert.AreEqual(2, result.StressPeriods[0].ClnLocationRates.Count);
            Assert.AreEqual("31|111|1111", result.StressPeriods[0].ClnLocationRates[0].Location);
            Assert.AreEqual(4.1506e+02, result.StressPeriods[0].ClnLocationRates[0].Rate);
            Assert.AreEqual("32|222|2222", result.StressPeriods[0].ClnLocationRates[1].Location);
            Assert.AreEqual(2.7547e+02, result.StressPeriods[0].ClnLocationRates[1].Rate);

            //stress period 2
            Assert.AreEqual(0, result.StressPeriods[1].Flag);
            Assert.AreEqual(1, result.StressPeriods[1].LocationRates.Count);
            Assert.AreEqual("11|111|1111", result.StressPeriods[1].LocationRates[0].Location);
            Assert.AreEqual(4.7291e+03, result.StressPeriods[1].LocationRates[0].Rate);

            Assert.AreEqual(2, result.StressPeriods[1].ClnLocationRates.Count);
            Assert.AreEqual("31|111|1111", result.StressPeriods[1].ClnLocationRates[0].Location);
            Assert.AreEqual(8.7291e+03, result.StressPeriods[1].ClnLocationRates[0].Rate);
            Assert.AreEqual("32|111|1111", result.StressPeriods[1].ClnLocationRates[1].Location);
            Assert.AreEqual(9.7291e+03, result.StressPeriods[1].ClnLocationRates[1].Rate);
        }

        [TestMethod]
        public void ReduceMapCells_HugeTest()
        {
            //this is for testing large amounts of data only - it should not normally be uncommented except for dev testing
            //ConfigurationManager.AppSettings["ModflowDataFolder"] = @"ModflowTestFiles\Structured\ReduceMapCells\HugeTest";
            //var sut = CreateModelFileAccessor(new Model());

            //var colors = Enumerable.Range(1, 5).Select(a => a.ToString()).ToList();
            //var rnd = new Random(10051981);
            //var locations = Enumerable.Range(1, 5001).SelectMany(a => Enumerable.Range(1, 65).Select(b=> new { row = b, col = a })).OrderBy(a=> rnd.NextDouble()).ToList();

            //var input = new List<MapLocationsPositionCellColor>();
            //var count = 0;
            //var itemsPerColor = locations.Count / colors.Count;
            //foreach (var color in colors)
            //{
            //    input.Add(new MapLocationsPositionCellColor { Color = color, Locations = locations.Skip(count * itemsPerColor).Take(itemsPerColor).Select(a => $"1|{a.row}|{a.col}").ToList() });
            //    count++;
            //}

            //var result = sut.ReduceMapCells(1, input).ToList();
        }

        internal override string GetLocationValue(string location)
        {
            return $"{location}|{location}|{location}";
        }

        [TestMethod]
        public void IsStructuredFile()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = $@"ModflowTestFiles\IsStructuredFile\StructuredModFlow2005";

            var result = ModelFileAccessor.IsStructuredFile(new Model
            {
                ModelExecutables = new List<ModelExecutable>
                {
                    new ModelExecutable
                    {
                        Arguments = "test.nam",
                    }
                }
            });
            Assert.IsTrue(result);
        }
    }

    [TestClass]
    public class UnstructuredModflowFileAccessorTests : ModelFileAccessorTests
    {
        internal override ModelFileAccessor CreateModelFileAccessor(Model model)
        {
            return new Olsson.GET.Accessors.FileIO.UnstructuredModflowFileAccessor(model);
        }

        internal override string ModflowFileType => "Unstructured";
        internal override BaseflowTableProcessingConfiguration BaseflowTableProcessingConfiguration => new BaseflowTableProcessingConfiguration()
        {
            BaseflowTableProcessingConfigurationID = 1,
            BaseflowTableIndicatorRegexPattern = @"^\s+STREAM LISTING\s+PERIOD\s+[0-9]+\s+STEP\s+[0-9]+$",
            SegmentColumnNum = 2,
            FlowToAquiferColumnNum = 5,
            ReachColumnNum = 3
        };

        [TestMethod]
        public void UpdateLocationRates_BasicTest()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = $@"ModflowTestFiles\{ModflowFileType}\UpdateLocationRates\BasicTest";
            var sut = CreateModelFileAccessor(new Model
            {
                ModelExecutables = new List<ModelExecutable>
                {
                    new ModelExecutable
                    {
                        Arguments = "Name.nam",
                    }
                }
            });

            sut.UpdateLocationRates(new StressPeriodsLocationRates
            {
                HeaderValue = "50",
                StressPeriods = new List<StressPeriodLocationRates>
                {
                    new StressPeriodLocationRates
                    {
                        Flag = 0,
                        LocationRates = new List<LocationRate>
                        {
                            new LocationRate{Location = "111", Rate = 123.45},
                            new LocationRate{Location = "222", Rate = 456.12}
                        }
                    },
                    new StressPeriodLocationRates
                    {
                        Flag = 0,
                        LocationRates = new List<LocationRate>()
                    },
                    new StressPeriodLocationRates
                    {
                        Flag = 0,
                        LocationRates = new List<LocationRate>
                        {
                            new LocationRate{Location = "111", Rate = 222.33},
                            new LocationRate{Location = "222", Rate = 1.11},
                            new LocationRate{Location = "2", Rate = 33.22}
                        }
                    }
                }
            });
            using (var fileLineEnumerator = File.ReadLines($@"ModflowTestFiles\{ModflowFileType}\UpdateLocationRates\BasicTest\Well.WEL").GetEnumerator())
            {
                //header
                Assert.IsTrue(fileLineEnumerator.MoveNext());
                Assert.AreEqual("3 50", fileLineEnumerator.Current);

                //stress period 1
                Assert.IsTrue(fileLineEnumerator.MoveNext());
                Assert.AreEqual("2 0", fileLineEnumerator.Current);

                Assert.IsTrue(fileLineEnumerator.MoveNext());
                Assert.AreEqual("111 1.234500e+002", fileLineEnumerator.Current);

                Assert.IsTrue(fileLineEnumerator.MoveNext());
                Assert.AreEqual("222 4.561200e+002", fileLineEnumerator.Current);

                //stress period 2
                Assert.IsTrue(fileLineEnumerator.MoveNext());
                Assert.AreEqual("0 0", fileLineEnumerator.Current);

                //stress period 3
                Assert.IsTrue(fileLineEnumerator.MoveNext());
                Assert.AreEqual("3 0", fileLineEnumerator.Current);

                Assert.IsTrue(fileLineEnumerator.MoveNext());
                Assert.AreEqual("111 2.223300e+002", fileLineEnumerator.Current);

                Assert.IsTrue(fileLineEnumerator.MoveNext());
                Assert.AreEqual("222 1.110000e+000", fileLineEnumerator.Current);

                Assert.IsTrue(fileLineEnumerator.MoveNext());
                Assert.AreEqual("2 3.322000e+001", fileLineEnumerator.Current);
            }
        }

        [TestMethod]
        public void UpdateLocationRates_HasClnWellGroup()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = $@"ModflowTestFiles\{ModflowFileType}\UpdateLocationRates\BasicTest";
            var sut = CreateModelFileAccessor(new Model
            {
                ModelExecutables = new List<ModelExecutable>
                {
                    new ModelExecutable
                    {
                        Arguments = "Name.nam",
                    }
                }
            });

            sut.UpdateLocationRates(new StressPeriodsLocationRates
            {
                HeaderValue = "50",
                StressPeriods = new List<StressPeriodLocationRates>
                {
                    new StressPeriodLocationRates
                    {
                        Flag = 0,
                        LocationRates = new List<LocationRate>
                        {
                            new LocationRate {Location = "111", Rate = 123.45},
                            new LocationRate {Location = "222", Rate = 456.12}
                        },
                        ClnLocationRates = new List<LocationRate>
                        {
                            new LocationRate {Location = "333", Rate = 987.65}
                        }
                    }
                }

            });
            using (var fileLineEnumerator = File.ReadLines($@"ModflowTestFiles\{ModflowFileType}\UpdateLocationRates\BasicTest\Well.WEL").GetEnumerator())
            {
                //header
                Assert.IsTrue(fileLineEnumerator.MoveNext());
                Assert.AreEqual("3 50", fileLineEnumerator.Current);

                //stress period 1
                Assert.IsTrue(fileLineEnumerator.MoveNext());
                Assert.AreEqual("2 0 1", fileLineEnumerator.Current);

                Assert.IsTrue(fileLineEnumerator.MoveNext());
                Assert.AreEqual("111 1.234500e+002", fileLineEnumerator.Current);

                Assert.IsTrue(fileLineEnumerator.MoveNext());
                Assert.AreEqual("222 4.561200e+002", fileLineEnumerator.Current);

                Assert.IsTrue(fileLineEnumerator.MoveNext());
                Assert.AreEqual("333 9.876500e+002", fileLineEnumerator.Current);
            }
        }

        [TestMethod]
        public void GetLocationRates_BasicTest()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = $@"ModflowTestFiles\{ModflowFileType}\GetLocationRates\BasicTest";
            var sut = CreateModelFileAccessor(new Model
            {
                ModelExecutables = new List<ModelExecutable>
                {
                    new ModelExecutable
                    {
                        Arguments = "Name.nam",
                    }
                }
            });

            var result = sut.GetLocationRates();
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Parameters.Count);
            Assert.AreEqual(2, result.StressPeriods.Count);
            Assert.AreEqual("50", result.HeaderValue);

            Assert.AreEqual(2, result.StressPeriods[0].LocationRates.Count);
            Assert.AreEqual("11", result.StressPeriods[0].LocationRates[0].Location);
            Assert.AreEqual(8.150616e+02, result.StressPeriods[0].LocationRates[0].Rate);
            Assert.AreEqual("12", result.StressPeriods[0].LocationRates[1].Location);
            Assert.AreEqual(5.754755e+02, result.StressPeriods[0].LocationRates[1].Rate);
            Assert.IsNull(result.StressPeriods[0].ClnLocationRates);

            Assert.AreEqual(1, result.StressPeriods[1].LocationRates.Count);
            Assert.AreEqual("11", result.StressPeriods[1].LocationRates[0].Location);
            Assert.AreEqual(4.729106e+03, result.StressPeriods[1].LocationRates[0].Rate);
            Assert.IsNull(result.StressPeriods[1].ClnLocationRates);
        }

        [TestMethod]
        public void GetLocationRates_MultipleWellGroups()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = $@"ModflowTestFiles\{ModflowFileType}\GetLocationRates\MultipleWellGroups";
            var sut = CreateModelFileAccessor(new Model
            {
                ModelExecutables = new List<ModelExecutable>
                {
                    new ModelExecutable
                    {
                        Arguments = "Name.nam",
                    }
                }
            });

            var result = sut.GetLocationRates();
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Parameters.Count);
            Assert.AreEqual(2, result.StressPeriods.Count);
            Assert.AreEqual("50 AUTOFLOWREDUCE  IUNITAFR 233", result.HeaderValue);

            //stress period 1
            Assert.AreEqual(2, result.StressPeriods[0].LocationRates.Count);
            Assert.AreEqual("11", result.StressPeriods[0].LocationRates[0].Location);
            Assert.AreEqual(8.150616e+02, result.StressPeriods[0].LocationRates[0].Rate);
            Assert.AreEqual("12", result.StressPeriods[0].LocationRates[1].Location);
            Assert.AreEqual(5.754755e+02, result.StressPeriods[0].LocationRates[1].Rate);

            Assert.AreEqual(2, result.StressPeriods[0].ClnLocationRates.Count);
            Assert.AreEqual("21", result.StressPeriods[0].ClnLocationRates[0].Location);
            Assert.AreEqual(4.150616e+02, result.StressPeriods[0].ClnLocationRates[0].Rate);
            Assert.AreEqual("22", result.StressPeriods[0].ClnLocationRates[1].Location);
            Assert.AreEqual(2.754755e+02, result.StressPeriods[0].ClnLocationRates[1].Rate);

            //stress period 2
            Assert.AreEqual(1, result.StressPeriods[1].LocationRates.Count);
            Assert.AreEqual("11", result.StressPeriods[1].LocationRates[0].Location);
            Assert.AreEqual(4.729106e+03, result.StressPeriods[1].LocationRates[0].Rate);

            Assert.AreEqual(2, result.StressPeriods[1].ClnLocationRates.Count);
            Assert.AreEqual("21", result.StressPeriods[1].ClnLocationRates[0].Location);
            Assert.AreEqual(8.729106e+03, result.StressPeriods[1].ClnLocationRates[0].Rate);
            Assert.AreEqual("22", result.StressPeriods[1].ClnLocationRates[1].Location);
            Assert.AreEqual(9.729106e+03, result.StressPeriods[1].ClnLocationRates[1].Rate);
        }

        internal override string GetLocationValue(string location)
        {
            return location;
        }

        [TestMethod]
        public void IsStructuredFile()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = $@"ModflowTestFiles\IsStructuredFile\UnstructuredModFlow2005";

            var result = ModelFileAccessor.IsStructuredFile(new Model
            {
                ModelExecutables = new List<ModelExecutable>
                {
                    new ModelExecutable
                    {
                        Arguments = "test.nam",
                    }
                }
            });
            Assert.IsFalse(result);
        }
    }

    [TestClass]
    public class StructuredModflowSixFileAccessorTests
    {
        internal ModelFileAccessor CreateModelFileAccessor(Model model)
        {
            return new Olsson.GET.Accessors.FileIO.StructuredModflowSixFileAccessor(model);
        }

        internal string ModflowFileType => "ModflowSixStructured";
        internal BaseflowTableProcessingConfiguration BaseflowTableProcessingConfiguration => new BaseflowTableProcessingConfiguration()
        {
            BaseflowTableProcessingConfigurationID = 1,
            BaseflowTableIndicatorRegexPattern = @"^\s+SFR \(SFR-\d+\) FLOWS\s+PERIOD\s+[0-9]+\s+STEP\s+[0-9]+$",
            SegmentColumnNum = 1,
            FlowToAquiferColumnNum = 7,
            ReachColumnNum = null
        };

        [TestMethod]
        public void GetSegmentReachZones_BasicTest()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = $@"ModflowTestFiles\{ModflowFileType}\GetSegmentReachZones\BasicTest";
            var sut = CreateModelFileAccessor(new Model());

            CollectionAssert.AreEquivalent(new[] { "1", "2" }, sut.GetSegmentReachZones(1, 0));
            CollectionAssert.AreEquivalent(new[] { "1" }, sut.GetSegmentReachZones(2, 0));
            CollectionAssert.AreEquivalent(new[] { "1", "2" }, sut.GetSegmentReachZones(3, 0));
            CollectionAssert.AreEquivalent(new[] { "1" }, sut.GetSegmentReachZones(4, 0));
            CollectionAssert.AreEquivalent(new string[0], sut.GetSegmentReachZones(5, 0));
        }

        [TestMethod]
        public void GetLocationRates_BasicTest()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = $@"ModflowTestFiles\{ModflowFileType}\GetLocationRates\BasicTest";
            var sut = CreateModelFileAccessor(new Model
            {
                ModelExecutables = new List<ModelExecutable>
                {
                    new ModelExecutable
                    {
                        Arguments = "mfsim.nam",
                    }
                }
            });

            var result = sut.GetLocationRates();
            Assert.IsNotNull(result);

            Assert.AreEqual(2, result.Parameters.Count);
            Assert.AreEqual("BOUNDNAMES", result.Parameters[0]);
            Assert.AreEqual("SAVE_FLOW", result.Parameters[1]);

            Assert.AreEqual(11, result.StressPeriods.Count);
            Assert.AreEqual("10", result.HeaderValue);

            Assert.AreEqual(10, result.StressPeriods[0].LocationRates.Count);
            Assert.AreEqual(0, result.StressPeriods[0].Flag);
            Assert.IsNull(result.StressPeriods[0].ClnLocationRates);
            Assert.AreEqual("1|6|4", result.StressPeriods[0].LocationRates[0].Location);
            Assert.AreEqual(-10.00, result.StressPeriods[0].LocationRates[0].Rate);
            Assert.AreEqual("1|6|5", result.StressPeriods[0].LocationRates[1].Location);
            Assert.AreEqual(-10.00, result.StressPeriods[0].LocationRates[1].Rate);

            Assert.AreEqual(0, result.StressPeriods[1].LocationRates.Count);
            Assert.IsNull(result.StressPeriods[1].ClnLocationRates);

            Assert.AreEqual(6, result.StressPeriods[2].LocationRates.Count);
            Assert.IsNull(result.StressPeriods[2].ClnLocationRates);
            Assert.AreEqual("1|6|4", result.StressPeriods[2].LocationRates[0].Location);
            Assert.AreEqual(-20.00, result.StressPeriods[2].LocationRates[0].Rate);

            Assert.AreEqual(6, result.StressPeriods[3].LocationRates.Count);
            Assert.IsNull(result.StressPeriods[3].ClnLocationRates);
            Assert.AreEqual("1|6|4", result.StressPeriods[3].LocationRates[0].Location);
            Assert.AreEqual(-20.00, result.StressPeriods[3].LocationRates[0].Rate);

            Assert.AreEqual(6, result.StressPeriods[9].LocationRates.Count);
            Assert.IsNull(result.StressPeriods[9].ClnLocationRates);
            Assert.AreEqual("1|6|4", result.StressPeriods[9].LocationRates[0].Location);
            Assert.AreEqual(-20.00, result.StressPeriods[9].LocationRates[0].Rate);

            Assert.AreEqual(10, result.StressPeriods[10].LocationRates.Count);
            Assert.IsNull(result.StressPeriods[10].ClnLocationRates);
            Assert.AreEqual("1|6|4", result.StressPeriods[10].LocationRates[0].Location);
            Assert.AreEqual(-30.00, result.StressPeriods[10].LocationRates[0].Rate);
        }

        [TestMethod]
        public void GetLocationRates_HasNoOptions()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = $@"ModflowTestFiles\{ModflowFileType}\GetLocationRates\HasNoOptions";
            var sut = CreateModelFileAccessor(new Model
            {
                ModelExecutables = new List<ModelExecutable>
                {
                    new ModelExecutable
                    {
                        Arguments = "mfsim.nam",
                    }
                }
            });

            var result = sut.GetLocationRates();
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Parameters.Count);

            Assert.AreEqual(11, result.StressPeriods.Count);
            Assert.AreEqual("10", result.HeaderValue);
        }

        [TestMethod]
        public void GetLocationRates_HasEmptyOptions()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = $@"ModflowTestFiles\{ModflowFileType}\GetLocationRates\HasEmptyOptions";
            var sut = CreateModelFileAccessor(new Model
            {
                ModelExecutables = new List<ModelExecutable>
                {
                    new ModelExecutable
                    {
                        Arguments = "mfsim.nam",
                    }
                }
            });

            var result = sut.GetLocationRates();
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Parameters.Count);

            Assert.AreEqual(11, result.StressPeriods.Count);
            Assert.AreEqual("10", result.HeaderValue);
        }

        [TestMethod]
        public void GetLocationRates_DifferentCasedHeaders()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = $@"ModflowTestFiles\{ModflowFileType}\GetLocationRates\DifferentCasedHeaders";
            var sut = CreateModelFileAccessor(new Model
            {
                ModelExecutables = new List<ModelExecutable>
                {
                    new ModelExecutable
                    {
                        Arguments = "mfsim.nam",
                    }
                }
            });

            var result = sut.GetLocationRates();
            Assert.IsNotNull(result);

            Assert.AreEqual(1, result.Parameters.Count);
            Assert.AreEqual("BOUNDNAMES", result.Parameters[0]);

            Assert.AreEqual(11, result.StressPeriods.Count);
            Assert.AreEqual("10", result.HeaderValue);

            Assert.AreEqual(10, result.StressPeriods[0].LocationRates.Count);
            Assert.AreEqual(0, result.StressPeriods[0].Flag);
            Assert.IsNull(result.StressPeriods[0].ClnLocationRates);
            Assert.AreEqual("1|6|4", result.StressPeriods[0].LocationRates[0].Location);
            Assert.AreEqual(-10.00, result.StressPeriods[0].LocationRates[0].Rate);
            Assert.AreEqual("1|6|5", result.StressPeriods[0].LocationRates[1].Location);
            Assert.AreEqual(-10.00, result.StressPeriods[0].LocationRates[1].Rate);

            Assert.AreEqual(0, result.StressPeriods[1].LocationRates.Count);
            Assert.IsNull(result.StressPeriods[1].ClnLocationRates);

            Assert.AreEqual(6, result.StressPeriods[2].LocationRates.Count);
            Assert.IsNull(result.StressPeriods[2].ClnLocationRates);
            Assert.AreEqual("1|6|4", result.StressPeriods[2].LocationRates[0].Location);
            Assert.AreEqual(-20.00, result.StressPeriods[2].LocationRates[0].Rate);

            Assert.AreEqual(6, result.StressPeriods[3].LocationRates.Count);
            Assert.IsNull(result.StressPeriods[3].ClnLocationRates);
            Assert.AreEqual("1|6|4", result.StressPeriods[3].LocationRates[0].Location);
            Assert.AreEqual(-20.00, result.StressPeriods[3].LocationRates[0].Rate);

            Assert.AreEqual(6, result.StressPeriods[9].LocationRates.Count);
            Assert.IsNull(result.StressPeriods[9].ClnLocationRates);
            Assert.AreEqual("1|6|4", result.StressPeriods[9].LocationRates[0].Location);
            Assert.AreEqual(-20.00, result.StressPeriods[9].LocationRates[0].Rate);

            Assert.AreEqual(10, result.StressPeriods[10].LocationRates.Count);
            Assert.IsNull(result.StressPeriods[10].ClnLocationRates);
            Assert.AreEqual("1|6|4", result.StressPeriods[10].LocationRates[0].Location);
            Assert.AreEqual(-30.00, result.StressPeriods[10].LocationRates[0].Rate);
        }

        [TestMethod]
        public void GetZoneBudgetRates_BasicTest()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = $@"ModflowTestFiles\{ModflowFileType}\ZoneBudgetItems\BasicTest";
            List<AsrDataMap> asrDataMap = new List<AsrDataMap>
            {
                new AsrDataMap{Key = "STO-SS", Name = "SS"},
                new AsrDataMap{Key = "STO-SY", Name = "SY"},
                new AsrDataMap{Key = "WEL", Name = "Wells"},
                new AsrDataMap{Key = "GHB", Name = "Head Dep Bounds"},
                new AsrDataMap{Key = "RCH", Name = "Recharge"},
                new AsrDataMap{Key = "EVT", Name = "ET"},
                new AsrDataMap{Key = "SFR", Name = "Streams"}
            };

            var sut = CreateModelFileAccessor(new Model
            {
                ModelExecutables = new List<ModelExecutable>
                {
                    new ModelExecutable
                    {
                        Arguments = "mfsim.nam",
                    }
                }
            });

            var result = sut.GetZoneBudgetItems("zbud.csv", asrDataMap).ToList();
            Assert.IsNotNull(result);
            Assert.AreEqual(12, result.Count);
            Assert.AreEqual(14, result[0].Values.Count);
            Assert.AreEqual("STO-SS", result[0].Values[0].Key);
            Assert.AreEqual(0.92131548435465827, result[0].Values[0].Value);
            Assert.AreEqual("STO-SS", result[0].Values[1].Key);
            Assert.AreEqual(0.024912084961899391, result[0].Values[1].Value);
            Assert.AreEqual("STO-SY", result[0].Values[2].Key);
            Assert.AreEqual(127.22596187282829, result[0].Values[2].Value);
            Assert.AreEqual("STO-SY", result[0].Values[3].Key);
            Assert.AreEqual(3.2242495683397081, result[0].Values[3].Value);
        }

        [TestMethod]
        public void GetZoneBudgetRates_MultiplePackageNames()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = $@"ModflowTestFiles\{ModflowFileType}\ZoneBudgetItems\MultiplePackageNames";
            List<AsrDataMap> asrDataMap = new List<AsrDataMap>
            {
                new AsrDataMap{Key = "STO-SS", Name = "SS"},
                new AsrDataMap{Key = "STO-SY", Name = "SY"},
                new AsrDataMap{Key = "WEL", Name = "Wells"},
                new AsrDataMap{Key = "GHB", Name = "Head Dep Bounds"},
                new AsrDataMap{Key = "RCH", Name = "Recharge"},
                new AsrDataMap{Key = "EVT", Name = "ET"},
                new AsrDataMap{Key = "SFR", Name = "Streams"}
            };

            var sut = CreateModelFileAccessor(new Model
            {
                ModelExecutables = new List<ModelExecutable>
                {
                    new ModelExecutable
                    {
                        Arguments = "mfsim.nam",
                    }
                }
            });

            var result = sut.GetZoneBudgetItems("zbud.csv", asrDataMap).ToList();
            Assert.IsNotNull(result);
            Assert.AreEqual(12, result.Count);
            Assert.AreEqual(14, result[0].Values.Count);
            Assert.AreEqual("STO-SS", result[0].Values[0].Key);
            Assert.AreEqual(0.90749878019365815, result[0].Values[0].Value);
            Assert.AreEqual("STO-SS", result[0].Values[1].Key);
            Assert.AreEqual(0.28130213450567448, result[0].Values[1].Value);
            Assert.AreEqual("STO-SY", result[0].Values[2].Key);
            Assert.AreEqual(126.2173798693843, result[0].Values[2].Value);
            Assert.AreEqual("STO-SY", result[0].Values[3].Key);
            Assert.AreEqual(21.94312731273838, result[0].Values[3].Value);
            Assert.AreEqual("WEL", result[0].Values[4].Key);
            Assert.AreEqual(20, result[0].Values[4].Value);
            Assert.AreEqual("WEL", result[0].Values[5].Key);
            Assert.AreEqual(80, result[0].Values[5].Value);
        }

        [TestMethod]
        public void GetStressPeriodData_BasicTest()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = $@"ModflowTestFiles\{ModflowFileType}\GetStressPeriodData";
            var sut = CreateModelFileAccessor(new Model
            {
                ModelExecutables = new List<ModelExecutable>
                {
                    new ModelExecutable
                    {
                        Arguments = "mfsim.nam",
                    }
                }
            });

            var result = sut.GetStressPeriodData();
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(50, result[0].NumberOfTimeSteps);
            Assert.AreEqual(50, result[1].NumberOfTimeSteps);
            Assert.AreEqual(1577880000.0, result[0].Days);
            Assert.AreEqual(1577880000.0, result[1].Days);
        }

        [TestMethod]
        public void GetZoneBudgetAsrDataNameMap()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = $@"ModflowTestFiles\{ModflowFileType}\GetAsrDataNameMap\BasicTest";

            var sut = CreateModelFileAccessor(new Model
            {
                ModelExecutables = new List<ModelExecutable>
                {
                    new ModelExecutable
                    {
                        Arguments = "mfsim.nam",
                    }
                }
            });

            var result = sut.GetZoneBudgetAsrDataNameMap();
            Assert.AreEqual(7, result.Count);
            Assert.AreEqual("STO-SS", result[0].Key);
            Assert.AreEqual("SS", result[0].Name);
            Assert.AreEqual("STO-SY", result[1].Key);
            Assert.AreEqual("SY", result[1].Name);
            Assert.AreEqual("WEL", result[2].Key);
            Assert.AreEqual("Wells", result[2].Name);
            Assert.AreEqual("GHB", result[3].Key);
            Assert.AreEqual("Head Dep Bounds", result[3].Name);
        }

        [TestMethod]
        public void GetBaselineMapData()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = $@"ModflowTestFiles\{ModflowFileType}\GetMapData";
            var sut = CreateModelFileAccessor(new Model
            {
                ModelExecutables = new List<ModelExecutable>
                {
                    new ModelExecutable
                    {
                        Arguments = "mfsim.nam",
                    }
                }
            });

            var result = sut.GetBaselineMapData().ToList();
            Assert.IsNotNull(result);
            Assert.AreEqual(600, result.Count);
            Assert.AreEqual("1|1|1", result[0].Location);
            Assert.AreEqual(1, result[0].StressPeriod);
            Assert.AreEqual(1, result[0].TimeStep);
            Assert.AreEqual(1092.8999997953272, result[0].Value);
            Assert.AreEqual("1|1|2", result[1].Location);
            Assert.AreEqual(1, result[1].StressPeriod);
            Assert.AreEqual(1, result[1].TimeStep);
            Assert.AreEqual(1089.4000000291328, result[1].Value);
            Assert.AreEqual("1|1|3", result[2].Location);
            Assert.AreEqual(1, result[2].StressPeriod);
            Assert.AreEqual(1, result[2].TimeStep);
            Assert.AreEqual(null, result[2].Value);
            Assert.AreEqual("1|10|10", result[99].Location);
            Assert.AreEqual(1, result[99].StressPeriod);
            Assert.AreEqual(1, result[99].TimeStep);
            Assert.AreEqual(1041.5999999901853, result[99].Value);
        }

        [TestMethod]
        public void UpdateLocationRates_BasicTest()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = $@"ModflowTestFiles\{ModflowFileType}\UpdateLocationRates\BasicTest";
            var sut = CreateModelFileAccessor(new Model
            {
                ModelExecutables = new List<ModelExecutable>
                {
                    new ModelExecutable
                    {
                        Arguments = "mfsim.nam",
                    }
                }
            });

            sut.UpdateLocationRates(new StressPeriodsLocationRates
            {
                HeaderValue = "        50",
                StressPeriods = new List<StressPeriodLocationRates>
                {
                    new StressPeriodLocationRates
                    {
                        Flag = 0,
                        LocationRates = new List<LocationRate>
                        {
                            new LocationRate{Location = "11|111|1111", Rate = 123.45},
                            new LocationRate{Location = "22|222|2222", Rate = 456.12}
                        }
                    },
                    new StressPeriodLocationRates
                    {
                        Flag = 0,
                        LocationRates = new List<LocationRate>()
                    },
                    new StressPeriodLocationRates
                    {
                        Flag = 0,
                        LocationRates = new List<LocationRate>
                        {
                            new LocationRate{Location = "11|111|1111", Rate = 222.33},
                            new LocationRate{Location = "22|222|2222", Rate = 1.11},
                            new LocationRate{Location = "3|3|3", Rate = 33.22},
                            new LocationRate{Location = "2|2|2", Rate=0},
                            new LocationRate{Location = "2|2|2", Rate=-999999999},
                            new LocationRate{Location = "2|2|2", Rate=-1000000000},
                            new LocationRate{Location = "2|2|2", Rate=-999999999.0},
                            new LocationRate{Location = "2|2|2", Rate=-999999999.9},
                            new LocationRate{Location = "2|2|2", Rate=9999999999},
                            new LocationRate{Location = "2|2|2", Rate=10000000000},
                            new LocationRate{Location = "2|2|2", Rate=9999999999.0},
                            new LocationRate{Location = "2|2|2", Rate=9999999999.9},
                        }
                    }
                },
                Parameters = new List<string>
                {
                    "BOUNDNAMES",
                    "PRINT_INPUT",
                    "PRINT_FLOWS",
                    "SAVE_FLOWS"
                }
            });
            using (var fileLineEnumerator = File.ReadLines($@"ModflowTestFiles\{ModflowFileType}\UpdateLocationRates\BasicTest\test1tr.wel").GetEnumerator())
            {
                //options header
                Assert.IsTrue(fileLineEnumerator.MoveNext());
                Assert.AreEqual("begin options", fileLineEnumerator.Current);
                Assert.IsTrue(fileLineEnumerator.MoveNext());
                Assert.AreEqual("  BOUNDNAMES", fileLineEnumerator.Current);
                Assert.IsTrue(fileLineEnumerator.MoveNext());
                Assert.AreEqual("  PRINT_INPUT", fileLineEnumerator.Current);
                Assert.IsTrue(fileLineEnumerator.MoveNext());
                Assert.AreEqual("  PRINT_FLOWS", fileLineEnumerator.Current);
                Assert.IsTrue(fileLineEnumerator.MoveNext());
                Assert.AreEqual("  SAVE_FLOWS", fileLineEnumerator.Current);
                Assert.IsTrue(fileLineEnumerator.MoveNext());
                Assert.AreEqual("end options", fileLineEnumerator.Current);
                Assert.IsTrue(fileLineEnumerator.MoveNext());
                Assert.AreEqual("", fileLineEnumerator.Current);

                //dimensions header
                Assert.IsTrue(fileLineEnumerator.MoveNext());
                Assert.AreEqual("begin dimensions", fileLineEnumerator.Current);
                Assert.IsTrue(fileLineEnumerator.MoveNext());
                Assert.AreEqual("  MAXBOUND 12", fileLineEnumerator.Current);
                Assert.IsTrue(fileLineEnumerator.MoveNext());
                Assert.AreEqual("end dimensions", fileLineEnumerator.Current);
                Assert.IsTrue(fileLineEnumerator.MoveNext());
                Assert.AreEqual("", fileLineEnumerator.Current);

                //stress period 1
                Assert.IsTrue(fileLineEnumerator.MoveNext());
                Assert.AreEqual("begin period 1", fileLineEnumerator.Current);
                Assert.IsTrue(fileLineEnumerator.MoveNext());
                Assert.AreEqual("        11       111      1111     123.45", fileLineEnumerator.Current);
                Assert.IsTrue(fileLineEnumerator.MoveNext());
                Assert.AreEqual("        22       222      2222     456.12", fileLineEnumerator.Current);
                Assert.IsTrue(fileLineEnumerator.MoveNext());
                Assert.AreEqual("end period", fileLineEnumerator.Current);
                Assert.IsTrue(fileLineEnumerator.MoveNext());
                Assert.AreEqual("", fileLineEnumerator.Current);

                //stress period 2
                Assert.IsTrue(fileLineEnumerator.MoveNext());
                Assert.AreEqual("begin period 2", fileLineEnumerator.Current);
                Assert.IsTrue(fileLineEnumerator.MoveNext());
                Assert.AreEqual("end period", fileLineEnumerator.Current);
                Assert.IsTrue(fileLineEnumerator.MoveNext());
                Assert.AreEqual("", fileLineEnumerator.Current);

                //stress period 3
                Assert.IsTrue(fileLineEnumerator.MoveNext());
                Assert.AreEqual("begin period 3", fileLineEnumerator.Current);
                Assert.IsTrue(fileLineEnumerator.MoveNext());
                Assert.AreEqual("        11       111      1111     222.33", fileLineEnumerator.Current);
                Assert.IsTrue(fileLineEnumerator.MoveNext());
                Assert.AreEqual("        22       222      2222       1.11", fileLineEnumerator.Current);
                Assert.IsTrue(fileLineEnumerator.MoveNext());
                Assert.AreEqual("         3         3         3      33.22", fileLineEnumerator.Current);
                Assert.IsTrue(fileLineEnumerator.MoveNext());
                Assert.AreEqual("         2         2         2          0", fileLineEnumerator.Current);
                Assert.IsTrue(fileLineEnumerator.MoveNext());
                Assert.AreEqual("         2         2         2 -999999999", fileLineEnumerator.Current);
                Assert.IsTrue(fileLineEnumerator.MoveNext());
                Assert.AreEqual("         2         2         2 -1000000000", fileLineEnumerator.Current);
                Assert.IsTrue(fileLineEnumerator.MoveNext());
                Assert.AreEqual("         2         2         2 -999999999", fileLineEnumerator.Current);
                Assert.IsTrue(fileLineEnumerator.MoveNext());
                Assert.AreEqual("         2         2         2 -999999999.9", fileLineEnumerator.Current);
                Assert.IsTrue(fileLineEnumerator.MoveNext());
                Assert.AreEqual("         2         2         2 9999999999", fileLineEnumerator.Current);
                Assert.IsTrue(fileLineEnumerator.MoveNext());
                Assert.AreEqual("         2         2         2 10000000000", fileLineEnumerator.Current);
                Assert.IsTrue(fileLineEnumerator.MoveNext());
                Assert.AreEqual("         2         2         2 9999999999", fileLineEnumerator.Current);
                Assert.IsTrue(fileLineEnumerator.MoveNext());
                Assert.AreEqual("         2         2         2 9999999999.9", fileLineEnumerator.Current);
                Assert.IsTrue(fileLineEnumerator.MoveNext());
                Assert.AreEqual("end period", fileLineEnumerator.Current);
            }
        }

        internal string GetLocationValue(string location)
        {
            return location;
        }

        [TestMethod]
        public void GetBaselineFlowData()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = $@"ModflowTestFiles\{ModflowFileType}\GetAquiferFlowData";

            var baseflowTableProcessingConfiguration = new BaseflowTableProcessingConfiguration()
            {
                BaseflowTableProcessingConfigurationID = 1,
                BaseflowTableIndicatorRegexPattern = @"^\s+SFR \(SFR-\d+\) FLOWS\s+PERIOD\s+[0-9]+\s+STEP\s+[0-9]+$",
                SegmentColumnNum = 1,
                FlowToAquiferColumnNum = 7,
                ReachColumnNum = null
            };
            var sut = CreateModelFileAccessor(new Model
            {
                ModelExecutables = new List<ModelExecutable>
                {
                    new ModelExecutable
                    {
                        Arguments = "mfsim.nam",
                    }
                }
                ,
                BaseflowTableProcessingConfiguration = baseflowTableProcessingConfiguration});

            var result = sut.GetBaselineData().ToList();
            Assert.IsNotNull(result);
            Assert.AreEqual(144, result.Count);
            Assert.AreEqual(1, result[0].SegmentNumber);
            Assert.AreEqual(0, result[0].ReachNumber);
            Assert.AreEqual(0.7426, result[0].FlowToAquifer);
            Assert.AreEqual(2, result[1].SegmentNumber);
            Assert.AreEqual(0, result[1].ReachNumber);
            Assert.AreEqual(2.136, result[1].FlowToAquifer);
            Assert.AreEqual(12, result[11].SegmentNumber);
            Assert.AreEqual(0, result[11].ReachNumber);
            Assert.AreEqual(-0.6849E-01, result[11].FlowToAquifer);
        }

        [TestMethod]
        public void GetOutputDataReachNameColumnAndNewIndicatorRegex()
        {
            //Currently no unstructured version is available to us
            if (ModflowFileType != "ModflowSixStructured")
            {
                return;
            }

            ConfigurationManager.AppSettings["ModflowDataFolder"] = $@"ModflowTestFiles\{ModflowFileType}\GetAquiferFlowData";

            var baseflowTableProcessingConfiguration = new BaseflowTableProcessingConfiguration()
            {
                BaseflowTableProcessingConfigurationID = 1,
                BaseflowTableIndicatorRegexPattern = @"^\s+SFR \(STREAMS_SFR\) FLOWS\s+PERIOD\s+[0-9]+\s+STEP\s+[0-9]+$",
                SegmentColumnNum = 2,
                FlowToAquiferColumnNum = 8,
                ReachColumnNum = null
            };
            var sut = CreateModelFileAccessor(new Model
            {
                ModelExecutables = new List<ModelExecutable>
                {
                    new ModelExecutable
                    {
                        Arguments = "mfsim.nam",
                    }
                }
                ,
                ListFileName = "Baseline_Reach_Name_Column.lst", BaseflowTableProcessingConfiguration = baseflowTableProcessingConfiguration });

            var result = sut.GetOutputData().ToList();
            Assert.IsNotNull(result);
            Assert.AreEqual(72, result.Count);
            Assert.AreEqual(1, result[0].SegmentNumber);
            Assert.AreEqual(0, result[0].ReachNumber);
            Assert.AreEqual(0.0000, result[0].FlowToAquifer);
            Assert.AreEqual(3, result[2].SegmentNumber);
            Assert.AreEqual(0, result[2].ReachNumber);
            Assert.AreEqual(-949.3, result[2].FlowToAquifer);
            Assert.AreEqual(1, result[36].SegmentNumber);
            Assert.AreEqual(0, result[36].ReachNumber);
            Assert.AreEqual(628.3, result[36].FlowToAquifer);
        }

        [TestMethod]
        public void GetOutputDataNewFormat02192021()
        {
            //Currently no unstructured version is available to us
            if (ModflowFileType != "ModflowSixStructured")
            {
                return;
            }

            ConfigurationManager.AppSettings["ModflowDataFolder"] = $@"ModflowTestFiles\{ModflowFileType}\GetAquiferFlowData";

            var baseflowTableProcessingConfiguration = new BaseflowTableProcessingConfiguration()
            {
                BaseflowTableProcessingConfigurationID = 1,
                BaseflowTableIndicatorRegexPattern = @"^\s+SFR-\d+ PACKAGE - SUMMARY OF FLOWS FOR EACH CONTROL VOLUME\s+PERIOD\s+[0-9]+\s+STEP\s+[0-9]+$",
                SegmentColumnNum = 1,
                FlowToAquiferColumnNum = 5,
                ReachColumnNum = null
            };
            var sut = CreateModelFileAccessor(new Model
            {
                ModelExecutables = new List<ModelExecutable>
                {
                    new ModelExecutable
                    {
                        Arguments = "mfsim.nam",
                    }
                }
                ,
                ListFileName = "Baseline_New_Format_02_19_2021.lst", BaseflowTableProcessingConfiguration = baseflowTableProcessingConfiguration });

            var result = sut.GetOutputData().ToList();
            Assert.IsNotNull(result);
            Assert.AreEqual(144, result.Count);
            Assert.AreEqual(1, result[0].SegmentNumber);
            Assert.AreEqual(0, result[0].ReachNumber);
            Assert.AreEqual(48654, result[0].FlowToAquifer);
            Assert.AreEqual(3, result[2].SegmentNumber);
            Assert.AreEqual(0, result[2].ReachNumber);
            Assert.AreEqual(10877, result[2].FlowToAquifer);
            Assert.AreEqual(1, result[36].SegmentNumber);
            Assert.AreEqual(0, result[36].ReachNumber);
            Assert.AreEqual(43009, result[36].FlowToAquifer);
        }

        [TestMethod]
        public void GetOutputFlowData()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = $@"ModflowTestFiles\{ModflowFileType}\GetAquiferFlowData";

            var baseflowTableProcessingConfiguration = new BaseflowTableProcessingConfiguration()
            {
                BaseflowTableProcessingConfigurationID = 1,
                BaseflowTableIndicatorRegexPattern = @"^\s+SFR \(SFR-\d+\) FLOWS\s+PERIOD\s+[0-9]+\s+STEP\s+[0-9]+$",
                SegmentColumnNum = 1,
                FlowToAquiferColumnNum = 7,
                ReachColumnNum = null
            };

            var sut = CreateModelFileAccessor(new Model
            {
                ModelExecutables = new List<ModelExecutable>
                {
                    new ModelExecutable
                    {
                        Arguments = "mfsim.nam",
                    }
                }
                ,
                ListFileName = "test1tr.lst", BaseflowTableProcessingConfiguration = baseflowTableProcessingConfiguration});

            var result = sut.GetOutputData().ToList();
            Assert.IsNotNull(result);
            Assert.AreEqual(720, result.Count);
            Assert.AreEqual(1, result[0].SegmentNumber);
            Assert.AreEqual(0, result[0].ReachNumber);
            Assert.AreEqual(0.7754, result[0].FlowToAquifer);
            Assert.AreEqual(2, result[1].SegmentNumber);
            Assert.AreEqual(0, result[1].ReachNumber);
            Assert.AreEqual(2.144, result[1].FlowToAquifer);
            Assert.AreEqual(36, result[35].SegmentNumber);
            Assert.AreEqual(0, result[35].ReachNumber);
            Assert.AreEqual(-0.4128E-01, result[35].FlowToAquifer);
        }

        public void GetOutputFlowDataSecondKnownConfigurationOfLstFile()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = $@"ModflowTestFiles\{ModflowFileType}\GetAquiferFlowData";

            var baseflowTableProcessingConfiguration = new BaseflowTableProcessingConfiguration()
            {
                BaseflowTableProcessingConfigurationID = 1,
                BaseflowTableIndicatorRegexPattern = @"^\s+SFR \(SFR-\d+\) FLOWS\s+PERIOD\s+[0-9]+\s+STEP\s+[0-9]+$",
                SegmentColumnNum = 1,
                FlowToAquiferColumnNum = 7,
                ReachColumnNum = null
            };

            var sut = CreateModelFileAccessor(new Model
            {
                ModelExecutables = new List<ModelExecutable>
                {
                    new ModelExecutable
                    {
                        Arguments = "mfsim.nam",
                    }
                }
                ,
                ListFileName = "test1tr.lst", BaseflowTableProcessingConfiguration = baseflowTableProcessingConfiguration });

            var result = sut.GetOutputData().ToList();
            Assert.IsNotNull(result);
            Assert.AreEqual(720, result.Count);
            Assert.AreEqual(1, result[0].SegmentNumber);
            Assert.AreEqual(0, result[0].ReachNumber);
            Assert.AreEqual(0.7754, result[0].FlowToAquifer);
            Assert.AreEqual(2, result[1].SegmentNumber);
            Assert.AreEqual(0, result[1].ReachNumber);
            Assert.AreEqual(2.144, result[1].FlowToAquifer);
            Assert.AreEqual(36, result[35].SegmentNumber);
            Assert.AreEqual(0, result[35].ReachNumber);
            Assert.AreEqual(-0.4128E-01, result[35].FlowToAquifer);
        }

        [TestMethod]
        public void IsStructuredFile()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = $@"ModflowTestFiles\IsStructuredFile\StructuredModFlow6";

            var result = ModelFileAccessor.IsStructuredFile(new Model
            {
                ModelExecutables = new List<ModelExecutable>
                {
                    new ModelExecutable
                    {
                        Arguments = "mfsim.nam",
                    }
                }
            });
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void GetRunListFileLines()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = $@"ModflowTestFiles\{ModflowFileType}\GetRunListFileLines";
            var sut = CreateModelFileAccessor(new Model { ListFileName = "mfsim.lst" });
            var result = sut.GetRunListFileLines().ToList();
            Assert.IsNotNull(result);
            Assert.AreEqual(215, result.Count);
        }

        [TestMethod]
        public void GetNumberOfSegmentReaches_BasicTest()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = @"ModflowTestFiles\ModflowSixStructured\GetNumberOfSegmentReaches\BasicTest";
            var sut = CreateModelFileAccessor(new Model
            {
                ModelExecutables = new List<ModelExecutable>
                {
                    new ModelExecutable
                    {
                        Arguments = "mfsim.nam",
                    }
                }
            });

            Assert.AreEqual(36, sut.GetNumberOfSegmentReaches());
        }

        [TestMethod]
        public void GetNumberOfSegmentReaches_ExtraSpaces()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = @"ModflowTestFiles\ModflowSixStructured\GetNumberOfSegmentReaches\ExtraSpaces";
            var sut = CreateModelFileAccessor(new Model
            {
                ModelExecutables = new List<ModelExecutable>
                {
                    new ModelExecutable
                    {
                        Arguments = "mfsim.nam",
                    }
                }
            });

            Assert.AreEqual(36, sut.GetNumberOfSegmentReaches());
        }
    }

    [TestClass]
    public class UnstructuredModflowSixFileAccessorTests
    {
        internal ModelFileAccessor CreateModelFileAccessor(Model model)
        {
            return new Olsson.GET.Accessors.FileIO.UnstructuredModflowSixFileAccessor(model);
        }

        internal string ModflowFileType => "ModflowSixUnstructured";
        internal BaseflowTableProcessingConfiguration BaseflowTableProcessingConfiguration => new BaseflowTableProcessingConfiguration()
        {
            BaseflowTableProcessingConfigurationID = 1,
            BaseflowTableIndicatorRegexPattern = @"^\s+SFR \(SFR-\d+\) FLOWS\s+PERIOD\s+[0-9]+\s+STEP\s+[0-9]+$",
            SegmentColumnNum = 1,
            FlowToAquiferColumnNum = 7,
            ReachColumnNum = null
        };

        [TestMethod]
        public void GetLocationRates_BasicTest()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = $@"ModflowTestFiles\{ModflowFileType}\GetLocationRates\BasicTest";
            var sut = CreateModelFileAccessor(new Model
            {
                ModelExecutables = new List<ModelExecutable>
                {
                    new ModelExecutable
                    {
                        Arguments = "mfsim.nam",
                    }
                }
            });

            var result = sut.GetLocationRates();
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Parameters.Count);
            Assert.AreEqual("BOUNDNAMES", result.Parameters[0]);
            Assert.AreEqual("SAVE_FLOW", result.Parameters[1]);
            Assert.AreEqual(6, result.StressPeriods.Count);
            Assert.AreEqual("2", result.HeaderValue);

            Assert.AreEqual(2, result.StressPeriods[0].LocationRates.Count);
            Assert.AreEqual("20", result.StressPeriods[0].LocationRates[0].Location);
            Assert.AreEqual(-10.00, result.StressPeriods[0].LocationRates[0].Rate);
            Assert.AreEqual("30", result.StressPeriods[0].LocationRates[1].Location);
            Assert.AreEqual(-10.00, result.StressPeriods[0].LocationRates[1].Rate);
            Assert.IsNull(result.StressPeriods[0].ClnLocationRates);

            Assert.AreEqual(2, result.StressPeriods[1].LocationRates.Count);
            Assert.AreEqual(-10.00, result.StressPeriods[1].LocationRates[0].Rate);
            Assert.AreEqual(-10.00, result.StressPeriods[1].LocationRates[1].Rate);
            Assert.IsNull(result.StressPeriods[1].ClnLocationRates);

            Assert.AreEqual(0, result.StressPeriods[2].LocationRates.Count);
            Assert.IsNull(result.StressPeriods[2].ClnLocationRates);

            Assert.AreEqual(2, result.StressPeriods[4].LocationRates.Count);
            Assert.AreEqual("20", result.StressPeriods[4].LocationRates[0].Location);
            Assert.AreEqual(-15.00, result.StressPeriods[4].LocationRates[0].Rate);
            Assert.IsNull(result.StressPeriods[4].ClnLocationRates);

            Assert.AreEqual(1, result.StressPeriods[5].LocationRates.Count);
            Assert.AreEqual("20", result.StressPeriods[5].LocationRates[0].Location);
            Assert.AreEqual(-5.00, result.StressPeriods[5].LocationRates[0].Rate);
            Assert.IsNull(result.StressPeriods[4].ClnLocationRates);
        }

        [TestMethod]
        public void GetLocationRates_HasNoOptions()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = $@"ModflowTestFiles\{ModflowFileType}\GetLocationRates\HasNoOptions";
            var sut = CreateModelFileAccessor(new Model
            {
                ModelExecutables = new List<ModelExecutable>
                {
                    new ModelExecutable
                    {
                        Arguments = "mfsim.nam",
                    }
                }
            });

            var result = sut.GetLocationRates();
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Parameters.Count);
            Assert.AreEqual(6, result.StressPeriods.Count);
            Assert.AreEqual("2", result.HeaderValue);
        }

        [TestMethod]
        public void GetLocationRates_HasEmptyOptions()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = $@"ModflowTestFiles\{ModflowFileType}\GetLocationRates\HasEmptyOptions";
            var sut = CreateModelFileAccessor(new Model
            {
                ModelExecutables = new List<ModelExecutable>
                {
                    new ModelExecutable
                    {
                        Arguments = "mfsim.nam",
                    }
                }
            });

            var result = sut.GetLocationRates();
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Parameters.Count);
            Assert.AreEqual(6, result.StressPeriods.Count);
            Assert.AreEqual("2", result.HeaderValue);
        }

        [TestMethod]
        public void GetLocationRates_DifferentCasedHeaders()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = $@"ModflowTestFiles\{ModflowFileType}\GetLocationRates\DifferentCasedHeaders";
            var sut = CreateModelFileAccessor(new Model
            {
                ModelExecutables = new List<ModelExecutable>
                {
                    new ModelExecutable
                    {
                        Arguments = "mfsim.nam",
                    }
                }
            });

            var result = sut.GetLocationRates();
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Parameters.Count);
            Assert.AreEqual("BOUNDNAMES", result.Parameters[0]);
            Assert.AreEqual(6, result.StressPeriods.Count);
            Assert.AreEqual("2", result.HeaderValue);
        }

        [TestMethod]
        public void UpdateLocationRates_BasicTest()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = $@"ModflowTestFiles\{ModflowFileType}\UpdateLocationRates\BasicTest";
            var sut = CreateModelFileAccessor(new Model
            {
                ModelExecutables = new List<ModelExecutable>
                {
                    new ModelExecutable
                    {
                        Arguments = "mfsim.nam",
                    }
                }
            });

            sut.UpdateLocationRates(new StressPeriodsLocationRates
            {
                HeaderValue = "50",
                StressPeriods = new List<StressPeriodLocationRates>
                {
                    new StressPeriodLocationRates
                    {
                        Flag = 0,
                        LocationRates = new List<LocationRate>
                        {
                            new LocationRate{Location = "111", Rate = 123.45},
                            new LocationRate{Location = "222", Rate = 456.12}
                        }
                    },
                    new StressPeriodLocationRates
                    {
                        Flag = 0,
                        LocationRates = new List<LocationRate>()
                    },
                    new StressPeriodLocationRates
                    {
                        Flag = 0,
                        LocationRates = new List<LocationRate>
                        {
                            new LocationRate{Location = "111", Rate = 222.33},
                            new LocationRate{Location = "222", Rate = 1.11},
                            new LocationRate{Location = "2", Rate = 33.22}
                        }
                    }
                },
                Parameters = new List<string>
                {
                    "BOUNDNAMES",
                    "SAVE_FLOWS"
                }
            });
            using (var fileLineEnumerator = File.ReadLines($@"ModflowTestFiles\{ModflowFileType}\UpdateLocationRates\BasicTest\flow.wel").GetEnumerator())
            {
                //options header
                Assert.IsTrue(fileLineEnumerator.MoveNext());
                Assert.AreEqual("begin options", fileLineEnumerator.Current);
                Assert.IsTrue(fileLineEnumerator.MoveNext());
                Assert.AreEqual("  BOUNDNAMES", fileLineEnumerator.Current);
                Assert.IsTrue(fileLineEnumerator.MoveNext());
                Assert.AreEqual("  SAVE_FLOWS", fileLineEnumerator.Current);
                Assert.IsTrue(fileLineEnumerator.MoveNext());
                Assert.AreEqual("end options", fileLineEnumerator.Current);
                Assert.IsTrue(fileLineEnumerator.MoveNext());
                Assert.AreEqual("", fileLineEnumerator.Current);

                //dimensions header
                Assert.IsTrue(fileLineEnumerator.MoveNext());
                Assert.AreEqual("begin dimensions", fileLineEnumerator.Current);
                Assert.IsTrue(fileLineEnumerator.MoveNext());
                Assert.AreEqual("  MAXBOUND 3", fileLineEnumerator.Current);
                Assert.IsTrue(fileLineEnumerator.MoveNext());
                Assert.AreEqual("end dimensions", fileLineEnumerator.Current);
                Assert.IsTrue(fileLineEnumerator.MoveNext());
                Assert.AreEqual("", fileLineEnumerator.Current);

                //stress period 1
                Assert.IsTrue(fileLineEnumerator.MoveNext());
                Assert.AreEqual("begin period 1", fileLineEnumerator.Current);
                Assert.IsTrue(fileLineEnumerator.MoveNext());
                Assert.AreEqual("        111     123.45", fileLineEnumerator.Current);
                Assert.IsTrue(fileLineEnumerator.MoveNext());
                Assert.AreEqual("        222     456.12", fileLineEnumerator.Current);
                Assert.IsTrue(fileLineEnumerator.MoveNext());
                Assert.AreEqual("end period", fileLineEnumerator.Current);
                Assert.IsTrue(fileLineEnumerator.MoveNext());
                Assert.AreEqual("", fileLineEnumerator.Current);

                //stress period 2
                Assert.IsTrue(fileLineEnumerator.MoveNext());
                Assert.AreEqual("begin period 2", fileLineEnumerator.Current);
                Assert.IsTrue(fileLineEnumerator.MoveNext());
                Assert.AreEqual("end period", fileLineEnumerator.Current);
                Assert.IsTrue(fileLineEnumerator.MoveNext());
                Assert.AreEqual("", fileLineEnumerator.Current);

                //stress period 3
                Assert.IsTrue(fileLineEnumerator.MoveNext());
                Assert.AreEqual("begin period 3", fileLineEnumerator.Current);
                Assert.IsTrue(fileLineEnumerator.MoveNext());
                Assert.AreEqual("        111     222.33", fileLineEnumerator.Current);
                Assert.IsTrue(fileLineEnumerator.MoveNext());
                Assert.AreEqual("        222       1.11", fileLineEnumerator.Current);
                Assert.IsTrue(fileLineEnumerator.MoveNext());
                Assert.AreEqual("          2      33.22", fileLineEnumerator.Current);
                Assert.IsTrue(fileLineEnumerator.MoveNext());
                Assert.AreEqual("end period", fileLineEnumerator.Current);
            }
        }

        [TestMethod]
        public void GetStressPeriodData_BasicTest()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = $@"ModflowTestFiles\{ModflowFileType}\GetStressPeriodData";
            var sut = CreateModelFileAccessor(new Model
            {
                ModelExecutables = new List<ModelExecutable>
                {
                    new ModelExecutable
                    {
                        Arguments = "mfsim.nam",
                    }
                }
            });

            var result = sut.GetStressPeriodData();
            Assert.IsNotNull(result);
            Assert.AreEqual(4, result.Count);
            Assert.AreEqual(1, result[0].NumberOfTimeSteps);
            Assert.AreEqual(1.0, result[0].Days);
            Assert.AreEqual(1, result[1].NumberOfTimeSteps);
            Assert.AreEqual(1.0, result[1].Days);
            Assert.AreEqual(1, result[2].NumberOfTimeSteps);
            Assert.AreEqual(1.0, result[2].Days);
            Assert.AreEqual(1, result[3].NumberOfTimeSteps);
            Assert.AreEqual(1.0, result[3].Days);
        }

        [TestMethod]
        public void GetZoneBudgetRates_BasicTest()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = $@"ModflowTestFiles\{ModflowFileType}\ZoneBudgetItems\BasicTest";
            List<AsrDataMap> asrDataMap = new List<AsrDataMap>
            {
                new AsrDataMap{Key = "STO-SS", Name = "SS"},
                new AsrDataMap{Key = "STO-SY", Name = "SY"},
                new AsrDataMap{Key = "WEL", Name = "Wells"},
                new AsrDataMap{Key = "GHB", Name = "Head Dep Bounds"},
                new AsrDataMap{Key = "RCH", Name = "Recharge"},
                new AsrDataMap{Key = "EVT", Name = "ET"},
                new AsrDataMap{Key = "SFR", Name = "Streams"}
            };

            var sut = CreateModelFileAccessor(new Model
            {
                ModelExecutables = new List<ModelExecutable>
                {
                    new ModelExecutable
                    {
                        Arguments = "mfsim.nam",
                    }
                }
            });

            var result = sut.GetZoneBudgetItems("zbud.csv", asrDataMap).ToList();
            Assert.IsNotNull(result);
            Assert.AreEqual(result.Count, 12);
            Assert.AreEqual(result[0].Values.Count, 14);
            Assert.AreEqual(result[0].Values[0].Key, "STO-SS");
            Assert.AreEqual(result[0].Values[0].Value, 0.92131548435465827);
            Assert.AreEqual(result[0].Values[1].Key, "STO-SS");
            Assert.AreEqual(result[0].Values[1].Value, 0.024912084961899391);
            Assert.AreEqual(result[0].Values[2].Key, "STO-SY");
            Assert.AreEqual(result[0].Values[2].Value, 127.22596187282829);
            Assert.AreEqual(result[0].Values[3].Key, "STO-SY");
            Assert.AreEqual(result[0].Values[3].Value, 3.2242495683397081);
        }

        [TestMethod]
        public void GetZoneBudgetAsrDataNameMap()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = $@"ModflowTestFiles\{ModflowFileType}\GetAsrDataNameMap\BasicTest";

            var sut = CreateModelFileAccessor(new Model());

            var result = sut.GetZoneBudgetAsrDataNameMap();
            Assert.AreEqual(7, result.Count);
            Assert.AreEqual("STO-SS", result[0].Key);
            Assert.AreEqual("SS", result[0].Name);
            Assert.AreEqual("STO-SY", result[1].Key);
            Assert.AreEqual("SY", result[1].Name);
            Assert.AreEqual("WEL", result[2].Key);
            Assert.AreEqual("Wells", result[2].Name);
            Assert.AreEqual("GHB", result[3].Key);
            Assert.AreEqual("Head Dep Bounds", result[3].Name);
        }

        [TestMethod]
        public void GetBaselineMapData()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = $@"ModflowTestFiles\{ModflowFileType}\GetMapData";
            var sut = CreateModelFileAccessor(new Model
            {
                ModelExecutables = new List<ModelExecutable>
                {
                    new ModelExecutable
                    {
                        Arguments = "mfsim.nam",
                    }
                }
            });

            var result = sut.GetBaselineMapData().ToList();
            Assert.IsNotNull(result);
            Assert.AreEqual(121, result.Count);
            Assert.AreEqual("1", result[0].Location);
            Assert.AreEqual(1, result[0].StressPeriod);
            Assert.AreEqual(1, result[0].TimeStep);
            Assert.AreEqual(1.0, result[0].Value);
            Assert.AreEqual("2", result[1].Location);
            Assert.AreEqual(1, result[1].StressPeriod);
            Assert.AreEqual(1, result[1].TimeStep);
            Assert.AreEqual(0.81196711271340682, result[1].Value);
            Assert.AreEqual("3", result[2].Location);
            Assert.AreEqual(1, result[2].StressPeriod);
            Assert.AreEqual(1, result[2].TimeStep);
            Assert.AreEqual(0.62355986618721915, result[2].Value);
            Assert.AreEqual("4", result[3].Location);
            Assert.AreEqual(1, result[3].StressPeriod);
            Assert.AreEqual(1, result[3].TimeStep);
            Assert.AreEqual(0.42868323043809409, result[3].Value);
        }

        internal string GetLocationValue(string location)
        {
            return location;
        }

        [TestMethod]
        public void GetBaselineFlowData()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = $@"ModflowTestFiles\{ModflowFileType}\GetAquiferFlowData";

            var sut = CreateModelFileAccessor(new Model
            {
                ModelExecutables = new List<ModelExecutable>
                {
                    new ModelExecutable
                    {
                        Arguments = "mfsim.nam",
                    }
                }
                ,
                BaseflowTableProcessingConfiguration = BaseflowTableProcessingConfiguration});

            var result = sut.GetBaselineData().ToList();
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(1, result[0].SegmentNumber);
            Assert.AreEqual(0, result[0].ReachNumber);
            Assert.AreEqual(-4.924, result[0].FlowToAquifer);
            Assert.AreEqual(2, result[1].SegmentNumber);
            Assert.AreEqual(0, result[1].ReachNumber);
            Assert.AreEqual(-7.376, result[1].FlowToAquifer);
        }

        [TestMethod]
        public void GetOutputFlowData()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = $@"ModflowTestFiles\{ModflowFileType}\GetAquiferFlowData";

            var sut = CreateModelFileAccessor(new Model
            {
                ModelExecutables = new List<ModelExecutable>
                {
                    new ModelExecutable
                    {
                        Arguments = "mfsim.nam",
                    }
                }
                ,
                ListFileName = "flow.lst", BaseflowTableProcessingConfiguration = BaseflowTableProcessingConfiguration});

            var result = sut.GetOutputData().ToList();
            Assert.IsNotNull(result);
            Assert.AreEqual(5, result.Count);
            Assert.AreEqual(1, result[0].SegmentNumber);
            Assert.AreEqual(0, result[0].ReachNumber);
            Assert.AreEqual(-4.924, result[0].FlowToAquifer);
            Assert.AreEqual(2, result[1].SegmentNumber);
            Assert.AreEqual(0, result[1].ReachNumber);
            Assert.AreEqual(-7.376, result[1].FlowToAquifer);
            Assert.AreEqual(5, result[4].SegmentNumber);
            Assert.AreEqual(0, result[4].ReachNumber);
            Assert.AreEqual(-0.7528E-01, result[4].FlowToAquifer);
        }

        [TestMethod]
        public void IsStructuredFile()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = $@"ModflowTestFiles\IsStructuredFile\UnstructuredModFlow6";

            var result = ModelFileAccessor.IsStructuredFile(new Model
            {
                ModelExecutables = new List<ModelExecutable>
                {
                    new ModelExecutable
                    {
                        Arguments = "mfsim.nam",
                    }
                }
            });
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void GetRunListFileLines()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = $@"ModflowTestFiles\{ModflowFileType}\GetRunListFileLines";
            var sut = CreateModelFileAccessor(new Model { ListFileName = "mfsim.lst" });
            var result = sut.GetRunListFileLines().ToList();
            Assert.IsNotNull(result);
            Assert.AreEqual(168, result.Count);
        }

        [TestMethod]
        public void GetNumberOfSegmentReaches_BasicTest()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = @"ModflowTestFiles\ModflowSixUnstructured\GetNumberOfSegmentReaches\BasicTest";
            var sut = CreateModelFileAccessor(new Model
            {
                ModelExecutables = new List<ModelExecutable>
                {
                    new ModelExecutable
                    {
                        Arguments = "mfsim.nam",
                    }
                }
            });

            Assert.AreEqual(2, sut.GetNumberOfSegmentReaches());
        }

        [TestMethod]
        public void GetNumberOfSegmentReaches_ExtraSpaces()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = @"ModflowTestFiles\ModflowSixUnstructured\GetNumberOfSegmentReaches\ExtraSpaces";
            var sut = CreateModelFileAccessor(new Model
            {
                ModelExecutables = new List<ModelExecutable>
                {
                    new ModelExecutable
                    {
                        Arguments = "mfsim.nam",
                    }
                }
            });

            Assert.AreEqual(2, sut.GetNumberOfSegmentReaches());
        }
    }

    public abstract class ModelFileAccessorTests
    {
        internal abstract ModelFileAccessor CreateModelFileAccessor(Model model);
        internal abstract string ModflowFileType { get; }
        internal abstract BaseflowTableProcessingConfiguration  BaseflowTableProcessingConfiguration { get; }
        internal abstract string GetLocationValue(string location);

        private const string SIMULATION_FILE_NAME = "COHYST.mpsim";

        [TestInitialize]
        public void InitTypes()
        {
            SqlServerTypes.Utilities.LoadNativeAssemblies(AppDomain.CurrentDomain.BaseDirectory);
            SqlProviderServices.SqlServerTypesAssemblyName = "Microsoft.SqlServer.Types, Version=14.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91";
        }

        [TestMethod]
        public void GetSegmentReachZones_BasicTest()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = @"ModflowTestFiles\GetSegmentReachZones\BasicTest";
            var sut = CreateModelFileAccessor(new Model());

            CollectionAssert.AreEquivalent(new[] { "1" }, sut.GetSegmentReachZones(1, 1));
            CollectionAssert.AreEquivalent(new[] { "2" }, sut.GetSegmentReachZones(1, 2));
            CollectionAssert.AreEquivalent(new[] { "1" }, sut.GetSegmentReachZones(2, 1));
            CollectionAssert.AreEquivalent(new[] { "1" }, sut.GetSegmentReachZones(2, 2));
            CollectionAssert.AreEquivalent(new[] { "1", "2" }, sut.GetSegmentReachZones(3, 1));
            CollectionAssert.AreEquivalent(new string[0], sut.GetSegmentReachZones(3, 2));
            CollectionAssert.AreEquivalent(new[] { "1" }, sut.GetSegmentReachZones(4, 1));
        }

        [TestMethod]
        public void GetSegmentReachZones_NoFile()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = @"ModflowTestFiles\GetSegmentReachZones\DoesntExist";
            var sut = CreateModelFileAccessor(new Model());

            CollectionAssert.AreEquivalent(new string[0], sut.GetSegmentReachZones(1, 1));
        }

        [TestMethod]
        public void GetFriendlyZoneName_BasicTest()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = @"ModflowTestFiles\GetFriendlyZoneName\BasicTest";
            var sut = CreateModelFileAccessor(new Model());

            Assert.AreEqual("Zone A", sut.GetFriendlyInputZoneName("1"));
            Assert.AreEqual("Zone B", sut.GetFriendlyInputZoneName("2"));
            Assert.AreEqual("", sut.GetFriendlyInputZoneName("3"));
            Assert.AreEqual("", sut.GetFriendlyInputZoneName(""));
        }

        [TestMethod]
        public void GetFriendlyZoneName_NoFile()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = @"ModflowTestFiles\GetFriendlyZoneName\DoesntExist";
            var sut = CreateModelFileAccessor(new Model());

            Assert.AreEqual("", sut.GetFriendlyInputZoneName("1"));
            Assert.AreEqual("", sut.GetFriendlyInputZoneName("2"));
            Assert.AreEqual("", sut.GetFriendlyInputZoneName("3"));
            Assert.AreEqual("", sut.GetFriendlyInputZoneName(""));
        }

        [TestMethod]
        public void GetFriendlyZoneBudgetName_BasicTest()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = @"ModflowTestFiles\GetFriendlyZoneBudgetName\BasicTest";
            var sut = CreateModelFileAccessor(new Model());

            Assert.AreEqual("Zone A", sut.GetFriendlyZoneBudgetName("1"));
            Assert.AreEqual("Zone B", sut.GetFriendlyZoneBudgetName("2"));
            Assert.AreEqual("", sut.GetFriendlyZoneBudgetName("3"));
            Assert.AreEqual("", sut.GetFriendlyZoneBudgetName(""));
        }

        [TestMethod]
        public void GetFriendlyZoneBudgetName_NoFile()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = @"ModflowTestFiles\GetFriendlyZoneBudgetName\DoesntExist";
            var sut = CreateModelFileAccessor(new Model());

            Assert.AreEqual("", sut.GetFriendlyZoneBudgetName("1"));
            Assert.AreEqual("", sut.GetFriendlyZoneBudgetName("2"));
            Assert.AreEqual("", sut.GetFriendlyZoneBudgetName("3"));
            Assert.AreEqual("", sut.GetFriendlyZoneBudgetName(""));
        }

        [TestMethod]
        public void GetAllZones_BasicTest()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = @"ModflowTestFiles\GetSegmentReachZones\BasicTest";
            var sut = CreateModelFileAccessor(new Model());

            CollectionAssert.AreEquivalent(new[] { "1", "2" }, sut.GetAllZones());
        }

        [TestMethod]
        public void GetAllZones_NoFile()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = @"ModflowTestFiles\GetSegmentReachZones\DoesntExist";
            var sut = CreateModelFileAccessor(new Model());

            CollectionAssert.AreEquivalent(new string[0], sut.GetAllZones());
        }

        [TestMethod]
        public void GetNumberOfSegmentReaches_BasicTest()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = @"ModflowTestFiles\GetNumberOfSegmentReaches\BasicTest";
            var sut = CreateModelFileAccessor(new Model
            {
                ModelExecutables = new List<ModelExecutable>
                {
                    new ModelExecutable
                    {
                        Arguments = "test.nam",
                    }
                }
            });

            Assert.AreEqual(1234, sut.GetNumberOfSegmentReaches());
        }

        [TestMethod]
        public void GetStressPeriodData_BasicTest()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = $@"ModflowTestFiles\{ModflowFileType}\GetStressPeriodData\BasicTest";
            var sut = CreateModelFileAccessor(new Model
            {
                ModelExecutables = new List<ModelExecutable>
                {
                    new ModelExecutable
                    {
                        Arguments = "test.nam",
                    }
                }
            });

            var result = sut.GetStressPeriodData();
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(31.0, result[0].Days);
            Assert.AreEqual(2, result[0].NumberOfTimeSteps);
            Assert.AreEqual(28.0, result[1].Days);
            Assert.AreEqual(2, result[1].NumberOfTimeSteps);
        }

        [TestMethod]
        public void GetStressPeriodData_NotEnoughData()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = $@"ModflowTestFiles\{ModflowFileType}\GetStressPeriodData\NotEnoughData";
            var sut = CreateModelFileAccessor(new Model
            {
                ModelExecutables = new List<ModelExecutable>
                {
                    new ModelExecutable
                    {
                        Arguments = "test.nam",
                    }
                }
            });

            Assert.ThrowsException<Exception>(() => sut.GetStressPeriodData());
        }

        [TestMethod]
        public void GetStressPeriodData_NoFile()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = $@"ModflowTestFiles\{ModflowFileType}\GetStressPeriodData\NoFile";
            var sut = CreateModelFileAccessor(new Model
            {
                ModelExecutables = new List<ModelExecutable>
                {
                    new ModelExecutable
                    {
                        Arguments = "test.nam",
                    }
                }
            });

            var result = sut.GetStressPeriodData();
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void GetOutputData_BasicTest()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = $@"ModflowTestFiles\{ModflowFileType}\GetOutputData\BasicTest";

            var sut = CreateModelFileAccessor(new Model
            {
                RunFileName = "BasicTest.dat",
                BaseflowTableProcessingConfiguration = BaseflowTableProcessingConfiguration,
                ModelExecutables = new List<ModelExecutable>
                {
                    new ModelExecutable
                    {
                        Arguments = "test.nam",
                    }
                }
            });

            var result = sut.GetOutputData().ToList();
            Assert.AreEqual(2, result.Count);

            Assert.AreEqual(1, result[0].SegmentNumber);
            Assert.AreEqual(8, result[0].ReachNumber);
            Assert.AreEqual(-299.19, result[0].FlowToAquifer);

            Assert.AreEqual(97, result[1].SegmentNumber);
            Assert.AreEqual(86, result[1].ReachNumber);
            Assert.AreEqual(5751.3, result[1].FlowToAquifer);
        }

        [TestMethod]
        public void GetOutputData_NoFile()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = $@"ModflowTestFiles\{ModflowFileType}\GetOutputData\NoFile";
            var sut = CreateModelFileAccessor(new Model
            {
                RunFileName = "BasicTest.dat",
                ModelExecutables = new List<ModelExecutable>
                {
                    new ModelExecutable
                    {
                        Arguments = "test.nam",
                    }
                }
            });

            var result = sut.GetOutputData();
            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetOutputData_RunFileName_Null()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = $@"ModflowTestFiles\{ModflowFileType}\GetOutputData\BasicTest";
            var sut = CreateModelFileAccessor(new Model
            {
                RunFileName = null,
                ModelExecutables = new List<ModelExecutable>
                {
                    new ModelExecutable
                    {
                        Arguments = "test.nam",
                    }
                }
            });

            var result = sut.GetOutputData();
            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetOutputData_RunFileName_Empty()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = $@"ModflowTestFiles\{ModflowFileType}\GetOutputData\BasicTest";
            var sut = CreateModelFileAccessor(new Model
            {
                RunFileName = "",
                ModelExecutables = new List<ModelExecutable>
                {
                    new ModelExecutable
                    {
                        Arguments = "test.nam",
                    }
                }
            });

            var result = sut.GetOutputData();
            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetOutputData_RunFileName_Whitespace()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = $@"ModflowTestFiles\{ModflowFileType}\GetOutputData\BasicTest";
            var sut = CreateModelFileAccessor(new Model
            {
                RunFileName = " ",
                ModelExecutables = new List<ModelExecutable>
                {
                    new ModelExecutable
                    {
                        Arguments = "test.nam",
                    }
                }
            });

            var result = sut.GetOutputData();
            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetBaselineData_BasicTest()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = $@"ModflowTestFiles\{ModflowFileType}\GetBaselineData\BasicTest";

            var sut = CreateModelFileAccessor(new Model
            {
                ModelExecutables = new List<ModelExecutable>
                {
                    new ModelExecutable
                    {
                        Arguments = "test.nam",
                    }
                },
                BaseflowTableProcessingConfiguration = BaseflowTableProcessingConfiguration
            });

            var result = sut.GetBaselineData().ToList();
            Assert.AreEqual(2, result.Count);

            Assert.AreEqual(1, result[0].SegmentNumber);
            Assert.AreEqual(8, result[0].ReachNumber);
            Assert.AreEqual(-299.19, result[0].FlowToAquifer);

            Assert.AreEqual(97, result[1].SegmentNumber);
            Assert.AreEqual(86, result[1].ReachNumber);
            Assert.AreEqual(5751.3, result[1].FlowToAquifer);
        }

        [TestMethod]
        public void GetBaselineData_NoFile()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = $@"ModflowTestFiles\{ModflowFileType}\GetBaselineData\NoFile";
            var sut = CreateModelFileAccessor(new Model
            {
                ModelExecutables = new List<ModelExecutable>
                {
                    new ModelExecutable
                    {
                        Arguments = "test.nam",
                    }
                }
            });

            var result = sut.GetBaselineData();
            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetLocationProportions_FeatureNotInHeader()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = $@"ModflowTestFiles\{ModflowFileType}\GetLocationProportions\HasSingleValue";
            var sut = CreateModelFileAccessor(new Model
            {
                ModelExecutables = new List<ModelExecutable>
                {
                    new ModelExecutable
                    {
                        Arguments = "name.nam",
                    }
                }
            });

            var canal1 = sut.GetLocationProportions("ThisDoesntExist");
            Assert.AreEqual(0, canal1.Count);
        }

        [TestMethod]
        public void GetLocationPositionMap_BasicTest()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = $@"ModflowTestFiles\{ModflowFileType}\GetLocationPositionMap\BasicTest";
            var sut = CreateModelFileAccessor(new Model());

            var result = sut.GetLocationPositionMap();
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);

            Assert.AreEqual(GetLocationValue("1"), result[0]);
            Assert.AreEqual(GetLocationValue("2"), result[1]);
        }

        [TestMethod]
        public void GetLocationPositionMap_IncludesSinglePumpingWellDataWithoutProportion()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = $@"ModflowTestFiles\{ModflowFileType}\GetLocationPositionMap\IncludesSinglePumpingWellDataWithoutProportion";
            var sut = CreateModelFileAccessor(new Model());

            var result = sut.GetLocationPositionMap();
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);

            Assert.AreEqual(GetLocationValue("1"), result[0]);
            Assert.AreEqual(GetLocationValue("2"), result[1]);
        }

        [TestMethod]
        public void GetLocationPositionMap_IncludesSinglePumpingWellDataWithProportion()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = $@"ModflowTestFiles\{ModflowFileType}\GetLocationPositionMap\IncludesSinglePumpingWellDataWithProportion";
            var sut = CreateModelFileAccessor(new Model());

            var result = sut.GetLocationPositionMap();
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);

            Assert.AreEqual(GetLocationValue("1"), result[0]);
            Assert.AreEqual(GetLocationValue("2"), result[1]);
        }

        [TestMethod]
        public void GetLocationPositionMap_IncludesMultiplePumpingWellDataWithoutProportion()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = $@"ModflowTestFiles\{ModflowFileType}\GetLocationPositionMap\IncludesMultiplePumpingWellDataWithoutProportion";
            var sut = CreateModelFileAccessor(new Model());

            var result = sut.GetLocationPositionMap();
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);

            Assert.AreEqual(GetLocationValue("1"), result[0]);
            Assert.AreEqual(GetLocationValue("2"), result[1]);
        }

        [TestMethod]
        public void GetLocationPositionMap_IncludesMultiplePumpingWellDataWithProportion()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = $@"ModflowTestFiles\{ModflowFileType}\GetLocationPositionMap\IncludesMultiplePumpingWellDataWithProportion";
            var sut = CreateModelFileAccessor(new Model());

            var result = sut.GetLocationPositionMap();
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);

            Assert.AreEqual(GetLocationValue("1"), result[0]);
            Assert.AreEqual(GetLocationValue("2"), result[1]);
        }

        public static List<(double Latitude, double Longitude)> PointsInFirstWellFileGeometry()
        {
            return Enumerable.Range(1, 8).SelectMany(a => Enumerable.Range(1, 8).Select(b => (Math.Round(a * .001 + 41.01, 3), Math.Round(b * .001 + -100.02, 3)))).ToList();
        }

        [TestMethod]
        public void FindWellLocations_NoPumpingWellData_PointPresent()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = $@"ModflowTestFiles\{ModflowFileType}\FindWellLocations\NoPumpingWellData";
            var sut = CreateModelFileAccessor(new Model());

            foreach (var point in PointsInFirstWellFileGeometry())
            {
                var result = sut.FindWellLocations(point.Latitude, point.Longitude);
                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Count);

                Assert.AreEqual(GetLocationValue("1"), result[0].Location);
                Assert.AreEqual(1.0, result[0].Proportion);
            }
        }

        [TestMethod]
        public void FindWellLocations_IncludesSinglePumpingWellDataWithProportion_PointPresent()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = $@"ModflowTestFiles\{ModflowFileType}\FindWellLocations\IncludesSinglePumpingWellDataWithProportion";
            var sut = CreateModelFileAccessor(new Model());

            foreach (var point in PointsInFirstWellFileGeometry())
            {
                var result = sut.FindWellLocations(point.Latitude, point.Longitude);
                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Count);

                Assert.AreEqual(GetLocationValue("3"), result[0].Location);
                Assert.AreEqual(1.0, result[0].Proportion);
            }
        }

        [TestMethod]
        public void FindWellLocations_IncludesSinglePumpingWellDataWithoutProportion_PointPresent()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = $@"ModflowTestFiles\{ModflowFileType}\FindWellLocations\IncludesSinglePumpingWellDataWithoutProportion";
            var sut = CreateModelFileAccessor(new Model());

            foreach (var point in PointsInFirstWellFileGeometry())
            {
                var result = sut.FindWellLocations(point.Latitude, point.Longitude);
                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Count);

                Assert.AreEqual(GetLocationValue("3"), result[0].Location);
                Assert.AreEqual(1.0, result[0].Proportion);
            }
        }

        [TestMethod]
        public void FindWellLocations_IncludesMultiplePumpingWellDataWithProportion_PointPresent()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = $@"ModflowTestFiles\{ModflowFileType}\FindWellLocations\IncludesMultiplePumpingWellDataWithProportion";
            var sut = CreateModelFileAccessor(new Model());

            foreach (var point in PointsInFirstWellFileGeometry())
            {
                var result = sut.FindWellLocations(point.Latitude, point.Longitude);
                Assert.IsNotNull(result);
                Assert.AreEqual(2, result.Count);

                Assert.AreEqual(GetLocationValue("3"), result[0].Location);
                Assert.AreEqual(0.75, result[0].Proportion);
                Assert.AreEqual(GetLocationValue("5"), result[1].Location);
                Assert.AreEqual(0.25, result[1].Proportion);
            }
        }

        [TestMethod]
        public void FindWellLocations_IncludesMultiplePumpingWellDataWithoutProportion_PointPresent()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = $@"ModflowTestFiles\{ModflowFileType}\FindWellLocations\IncludesMultiplePumpingWellDataWithoutProportion";
            var sut = CreateModelFileAccessor(new Model());

            foreach (var point in PointsInFirstWellFileGeometry())
            {
                var result = sut.FindWellLocations(point.Latitude, point.Longitude);
                Assert.IsNotNull(result);
                Assert.AreEqual(2, result.Count);

                Assert.AreEqual(GetLocationValue("3"), result[0].Location);
                Assert.AreEqual(0.5, result[0].Proportion);
                Assert.AreEqual(GetLocationValue("5"), result[1].Location);
                Assert.AreEqual(0.5, result[1].Proportion);
            }
        }

        [TestMethod]
        public void GetLocationZones_FileNotFound()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = $@"ModflowTestFiles\{ModflowFileType}\GetLocationZones\FileNotFound";
            var sut = CreateModelFileAccessor(new Model());

            var result = sut.GetInputLocationZones(GetLocationValue("1"));

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void GetLocationZones_NodeNotInFile()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = $@"ModflowTestFiles\{ModflowFileType}\GetLocationZones\NodeNotInFile";
            var sut = CreateModelFileAccessor(new Model());

            var result = sut.GetInputLocationZones(GetLocationValue("1"));

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void GetLocationZones_SingleZoneFound()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = $@"ModflowTestFiles\{ModflowFileType}\GetLocationZones\SingleZoneFound";
            var sut = CreateModelFileAccessor(new Model());

            var result = sut.GetInputLocationZones(GetLocationValue("1"));

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("1", result[0]);
        }

        [TestMethod]
        public void GetLocationZones_MultipleZonesFound()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = $@"ModflowTestFiles\{ModflowFileType}\GetLocationZones\MultipleZonesFound";
            var sut = CreateModelFileAccessor(new Model());

            var result = sut.GetInputLocationZones(GetLocationValue("1"));

            Assert.IsNotNull(result);
            CollectionAssert.AreEquivalent(new[] { "1", "2" }, result);
        }

        [TestMethod]
        public void GetLocationZones_MultipleZonesFound_NewFileNameInputLocationZones()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = $@"ModflowTestFiles\{ModflowFileType}\GetLocationZones\InputLocationZonesCsv";
            var sut = CreateModelFileAccessor(new Model());

            var result = sut.GetInputLocationZones(GetLocationValue("1"));

            Assert.IsNotNull(result);
            CollectionAssert.AreEquivalent(new[] { "1", "2" }, result);
        }

        [TestMethod]
        public void GetOutputLocationZones_Valid()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = $@"ModflowTestFiles\{ModflowFileType}\GetOutputLocationZones\Valid";
            var sut = CreateModelFileAccessor(new Model());

            var result = sut.GetOutputLocationZones(GetLocationValue("1"));

            Assert.IsNotNull(result);
            CollectionAssert.AreEquivalent(new[] { "1", "2" }, result);
        }

        [TestMethod]
        public void GetLocationProportions_HasSingleValue()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = $@"ModflowTestFiles\{ModflowFileType}\GetLocationProportions\HasSingleValue";
            var sut = CreateModelFileAccessor(new Model
            {
                ModelExecutables = new List<ModelExecutable>
                {
                    new ModelExecutable
                    {
                        Arguments = "name.nam",
                    }
                }
            });

            var canal1 = sut.GetLocationProportions("Canal1");
            Assert.AreEqual(1, canal1.Count);
            Assert.AreEqual(GetLocationValue("1"), canal1[0].Location);
            Assert.AreEqual(1.0, canal1[0].Proportion);
            Assert.IsFalse(canal1[0].IsClnWell);

            var canal2 = sut.GetLocationProportions("Canal2");
            Assert.AreEqual(0, canal2.Count);
        }

        [TestMethod]
        public void GetLocationProportions_HasMultipleValues()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = $@"ModflowTestFiles\{ModflowFileType}\GetLocationProportions\HasMultipleValues";
            var sut = CreateModelFileAccessor(new Model
            {
                ModelExecutables = new List<ModelExecutable>
                {
                    new ModelExecutable
                    {
                        Arguments = "name.nam",
                    }
                }
            });

            var canal1 = sut.GetLocationProportions("Canal1");
            Assert.AreEqual(2, canal1.Count);
            var location1 = canal1.Single(a => a.Location == GetLocationValue("1"));
            Assert.AreEqual(.75, location1.Proportion);
            Assert.IsFalse(location1.IsClnWell);
            var location2 = canal1.Single(a => a.Location == GetLocationValue("2"));
            Assert.AreEqual(.25, location2.Proportion);
            Assert.IsFalse(location2.IsClnWell);

            var canal2 = sut.GetLocationProportions("Canal2");
            Assert.AreEqual(0.0, canal2.Count);
        }

        [TestMethod]
        public void GetLocationProportions_DifferentCasedFeatureName()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = $@"ModflowTestFiles\{ModflowFileType}\GetLocationProportions\HasSingleValue";
            var sut = CreateModelFileAccessor(new Model
            {
                ModelExecutables = new List<ModelExecutable>
                {
                    new ModelExecutable
                    {
                        Arguments = "name.nam",
                    }
                }
            });

            var canal1 = sut.GetLocationProportions("canal1");
            Assert.AreEqual(1, canal1.Count);
            Assert.AreEqual(GetLocationValue("1"), canal1[0].Location);
            Assert.AreEqual(1.0, canal1[0].Proportion);
            Assert.IsFalse(canal1[0].IsClnWell);
        }

        [TestMethod]
        public void GetLocationProportions_HasClnColumn_IsFalse()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = $@"ModflowTestFiles\{ModflowFileType}\GetLocationProportions\ClnColumnFalse";
            var sut = CreateModelFileAccessor(new Model
            {
                ModelExecutables = new List<ModelExecutable>
                {
                    new ModelExecutable
                    {
                        Arguments = "name.nam",
                    }
                }
            });

            var canal1 = sut.GetLocationProportions("canal1");
            Assert.AreEqual(1, canal1.Count);
            Assert.AreEqual(GetLocationValue("1"), canal1[0].Location);
            Assert.AreEqual(1.0, canal1[0].Proportion);
            Assert.IsFalse(canal1[0].IsClnWell);
        }

        [TestMethod]
        public void GetLocationProportions_HasClnColumn_IsTrue()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = $@"ModflowTestFiles\{ModflowFileType}\GetLocationProportions\ClnColumnTrue";
            var sut = CreateModelFileAccessor(new Model
            {
                ModelExecutables = new List<ModelExecutable>
                {
                    new ModelExecutable
                    {
                        Arguments = "name.nam",
                    }
                }
            });

            var canal1 = sut.GetLocationProportions("canal1");
            Assert.AreEqual(1, canal1.Count);
            Assert.AreEqual(GetLocationValue("1"), canal1[0].Location);
            Assert.AreEqual(1.0, canal1[0].Proportion);
            Assert.IsTrue(canal1[0].IsClnWell);
        }

        [TestMethod]
        public void GetZoneBudgetAsrDataNameMap_EmptyZoneSpecificFile()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = @"ModflowTestFiles\GetZoneBudgetAsrDataNameMap\EmptyZoneSpecificFile";

            var sut = CreateModelFileAccessor(new Model());

            var result = sut.GetZoneBudgetAsrDataNameMap();
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void GetZoneBudgetAsrDataNameMap_HasZoneSpecificData()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = @"ModflowTestFiles\GetZoneBudgetAsrDataNameMap\HasZoneSpecificData";

            var sut = CreateModelFileAccessor(new Model());

            var result = sut.GetZoneBudgetAsrDataNameMap();
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("ZoneKey1", result[0].Key);
            Assert.AreEqual("ZoneName1", result[0].Name);
            Assert.AreEqual("ZoneKey2", result[1].Key);
            Assert.AreEqual("ZoneName2", result[1].Name);
        }
        [TestMethod]
        public void GetZoneBudgetAsrDataNameMap_MissingZoneSpecificFile()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = @"ModflowTestFiles\GetZoneBudgetAsrDataNameMap\MissingZoneSpecificFile";

            var sut = CreateModelFileAccessor(new Model());

            var result = sut.GetZoneBudgetAsrDataNameMap();
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("Key1", result[0].Key);
            Assert.AreEqual("Name1", result[0].Name);
            Assert.AreEqual("Key2", result[1].Key);
            Assert.AreEqual("Name2", result[1].Name);
        }
        [TestMethod]
        public void GetZoneBudgetAsrDataNameMap_NoAsrDataAtAll()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = @"ModflowTestFiles\GetZoneBudgetAsrDataNameMap\NoAsrDataAtAll";

            var sut = CreateModelFileAccessor(new Model());

            var result = sut.GetZoneBudgetAsrDataNameMap();
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void GetBaselineZoneBudgetItems_NoFile()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = @"ModflowTestFiles\GetBaselineZoneBudgetItems\NoFile";
            List<AsrDataMap> asrDataMap = new List<AsrDataMap>
            {
                new AsrDataMap{Key = "KeyA", Name = "NameA"},
                new AsrDataMap{Key = "KeyC", Name = "NameC"},
                new AsrDataMap{Key = "KeyB", Name = "NameB"},
                new AsrDataMap{Key = "KeyD", Name = "NameD"}
            };

            var sut = CreateModelFileAccessor(new Model());

            var result = sut.GetBaselineZoneBudgetItems(asrDataMap);
            Assert.IsNull(result);
        }
        [TestMethod]
        public void GetBaselineZoneBudgetItems_NoData()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = @"ModflowTestFiles\GetBaselineZoneBudgetItems\NoData";
            List<AsrDataMap> asrDataMap = new List<AsrDataMap>
            {
                new AsrDataMap{Key = "KeyA", Name = "NameA"},
                new AsrDataMap{Key = "KeyC", Name = "NameC"},
                new AsrDataMap{Key = "KeyB", Name = "NameB"},
                new AsrDataMap{Key = "KeyD", Name = "NameD"}
            };

            var sut = CreateModelFileAccessor(new Model());

            var result = sut.GetBaselineZoneBudgetItems(asrDataMap).ToList();
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void GetBaselineZoneBudgetItems_HasRecords()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = @"ModflowTestFiles\GetBaselineZoneBudgetItems\HasRecords";
            List<AsrDataMap> asrDataMap = new List<AsrDataMap>
            {
                new AsrDataMap{Key = "KeyA", Name = "NameA"},
                new AsrDataMap{Key = "KeyC", Name = "NameC"},
                new AsrDataMap{Key = "KeyB", Name = "NameB"},
                new AsrDataMap{Key = "KeyD", Name = "NameD"}
            };

            var sut = CreateModelFileAccessor(new Model());

            var result = sut.GetBaselineZoneBudgetItems(asrDataMap).ToList();
            Assert.AreEqual(2, result.Count);

            Assert.AreEqual(1, result[0].Period);
            Assert.AreEqual(2, result[0].Step);
            Assert.AreEqual("3", result[0].Zone);
            Assert.AreEqual(6, result[0].Values.Count);
            Assert.AreEqual("STORAGE", result[0].Values[0].Key);
            Assert.AreEqual(11.11, result[0].Values[0].Value, .01);
            Assert.AreEqual("RIVER LEAKAGE", result[0].Values[1].Key);
            Assert.AreEqual(22.22, result[0].Values[1].Value, .01);
            Assert.AreEqual("Total IN", result[0].Values[2].Key);
            Assert.AreEqual(33.33, result[0].Values[2].Value, .01);
            Assert.AreEqual("STORAGE", result[0].Values[3].Key);
            Assert.AreEqual(44.44, result[0].Values[3].Value, .01);
            Assert.AreEqual("RIVER LEAKAGE", result[0].Values[4].Key);
            Assert.AreEqual(55.55, result[0].Values[4].Value, .01);
            Assert.AreEqual("Total Out", result[0].Values[5].Key);
            Assert.AreEqual(66.66, result[0].Values[5].Value, .01);

            Assert.AreEqual(4, result[1].Period);
            Assert.AreEqual(5, result[1].Step);
            Assert.AreEqual("6", result[1].Zone);
            Assert.AreEqual(6, result[1].Values.Count);
            Assert.AreEqual("STORAGE", result[1].Values[0].Key);
            Assert.AreEqual(12.34, result[1].Values[0].Value, .01);
            Assert.AreEqual("RIVER LEAKAGE", result[1].Values[1].Key);
            Assert.AreEqual(23.45, result[1].Values[1].Value, .01);
            Assert.AreEqual("Total IN", result[1].Values[2].Key);
            Assert.AreEqual(34.56, result[1].Values[2].Value, .01);
            Assert.AreEqual("STORAGE", result[1].Values[3].Key);
            Assert.AreEqual(45.67, result[1].Values[3].Value, .01);
            Assert.AreEqual("RIVER LEAKAGE", result[1].Values[4].Key);
            Assert.AreEqual(56.78, result[1].Values[4].Value, .01);
            Assert.AreEqual("Total Out", result[1].Values[5].Key);
            Assert.AreEqual(67.89, result[1].Values[5].Value, .01);
        }

        [TestMethod]
        public void GetBaselineZoneBudgetItems_HasUndefinedValue()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = @"ModflowTestFiles\GetBaselineZoneBudgetItems\HasUndefinedValue";
            List<AsrDataMap> asrDataMap = new List<AsrDataMap>
            {
                new AsrDataMap{Key = "KeyA", Name = "NameA"},
                new AsrDataMap{Key = "KeyC", Name = "NameC"},
                new AsrDataMap{Key = "KeyB", Name = "NameB"},
                new AsrDataMap{Key = "KeyD", Name = "NameD"}
            };

            var sut = CreateModelFileAccessor(new Model());

            var result = sut.GetBaselineZoneBudgetItems(asrDataMap).ToList();
            Assert.AreEqual(1, result.Count);

            Assert.AreEqual(1, result[0].Period);
            Assert.AreEqual(2, result[0].Step);
            Assert.AreEqual("3", result[0].Zone);
            Assert.AreEqual(6, result[0].Values.Count);
            Assert.AreEqual("STORAGE", result[0].Values[0].Key);
            Assert.AreEqual(11.11, result[0].Values[0].Value, .01);
            Assert.AreEqual("RIVER LEAKAGE", result[0].Values[1].Key);
            Assert.AreEqual(22.22, result[0].Values[1].Value, .01);
            Assert.AreEqual("Total IN", result[0].Values[2].Key);
            Assert.AreEqual(33.33, result[0].Values[2].Value, .01);
            Assert.AreEqual("STORAGE", result[0].Values[3].Key);
            Assert.AreEqual(44.44, result[0].Values[3].Value, .01);
            Assert.AreEqual("RIVER LEAKAGE", result[0].Values[4].Key);
            Assert.AreEqual(55.55, result[0].Values[4].Value, .01);
            Assert.AreEqual("Total Out", result[0].Values[5].Key);
            Assert.AreEqual(66.66, result[0].Values[5].Value, .01);
        }

        [TestMethod]
        public void GetRunZoneBudgetItems_NoFile()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = @"ModflowTestFiles\GetRunZoneBudgetItems\NoFile";
            List<AsrDataMap> asrDataMap = new List<AsrDataMap>
            {
                new AsrDataMap{Key = "KeyA", Name = "NameA"},
                new AsrDataMap{Key = "KeyC", Name = "NameC"},
                new AsrDataMap{Key = "KeyB", Name = "NameB"},
                new AsrDataMap{Key = "KeyD", Name = "NameD"}
            };

            var sut = CreateModelFileAccessor(new Model());

            var result = sut.GetRunZoneBudgetItems(asrDataMap);
            Assert.IsNull(result);
        }
        [TestMethod]
        public void GetRunZoneBudgetItems_NoData()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = @"ModflowTestFiles\GetRunZoneBudgetItems\NoData";
            List<AsrDataMap> asrDataMap = new List<AsrDataMap>
            {
                new AsrDataMap{Key = "KeyA", Name = "NameA"},
                new AsrDataMap{Key = "KeyC", Name = "NameC"},
                new AsrDataMap{Key = "KeyB", Name = "NameB"},
                new AsrDataMap{Key = "KeyD", Name = "NameD"}
            };

            var sut = CreateModelFileAccessor(new Model());

            var result = sut.GetRunZoneBudgetItems(asrDataMap).ToList();
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void GetRunZoneBudgetItems_HasRecords()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = @"ModflowTestFiles\GetRunZoneBudgetItems\HasRecords";
            List<AsrDataMap> asrDataMap = new List<AsrDataMap>
            {
                new AsrDataMap{Key = "KeyA", Name = "NameA"},
                new AsrDataMap{Key = "KeyC", Name = "NameC"},
                new AsrDataMap{Key = "KeyB", Name = "NameB"},
                new AsrDataMap{Key = "KeyD", Name = "NameD"}
            };

            var sut = CreateModelFileAccessor(new Model());

            var result = sut.GetRunZoneBudgetItems(asrDataMap).ToList();
            Assert.AreEqual(2, result.Count);

            Assert.AreEqual(1, result[0].Period);
            Assert.AreEqual(2, result[0].Step);
            Assert.AreEqual("3", result[0].Zone);
            Assert.AreEqual(6, result[0].Values.Count);
            Assert.AreEqual("STORAGE", result[0].Values[0].Key);
            Assert.AreEqual(11.11, result[0].Values[0].Value, .01);
            Assert.AreEqual("RIVER LEAKAGE", result[0].Values[1].Key);
            Assert.AreEqual(22.22, result[0].Values[1].Value, .01);
            Assert.AreEqual("Total IN", result[0].Values[2].Key);
            Assert.AreEqual(33.33, result[0].Values[2].Value, .01);
            Assert.AreEqual("STORAGE", result[0].Values[3].Key);
            Assert.AreEqual(44.44, result[0].Values[3].Value, .01);
            Assert.AreEqual("RIVER LEAKAGE", result[0].Values[4].Key);
            Assert.AreEqual(55.55, result[0].Values[4].Value, .01);
            Assert.AreEqual("Total Out", result[0].Values[5].Key);
            Assert.AreEqual(66.66, result[0].Values[5].Value, .01);

            Assert.AreEqual(4, result[1].Period);
            Assert.AreEqual(5, result[1].Step);
            Assert.AreEqual("6", result[1].Zone);
            Assert.AreEqual(6, result[1].Values.Count);
            Assert.AreEqual("STORAGE", result[1].Values[0].Key);
            Assert.AreEqual(12.34, result[1].Values[0].Value, .01);
            Assert.AreEqual("RIVER LEAKAGE", result[1].Values[1].Key);
            Assert.AreEqual(23.45, result[1].Values[1].Value, .01);
            Assert.AreEqual("Total IN", result[1].Values[2].Key);
            Assert.AreEqual(34.56, result[1].Values[2].Value, .01);
            Assert.AreEqual("STORAGE", result[1].Values[3].Key);
            Assert.AreEqual(45.67, result[1].Values[3].Value, .01);
            Assert.AreEqual("RIVER LEAKAGE", result[1].Values[4].Key);
            Assert.AreEqual(56.78, result[1].Values[4].Value, .01);
            Assert.AreEqual("Total Out", result[1].Values[5].Key);
            Assert.AreEqual(67.89, result[1].Values[5].Value, .01);
        }

        [TestMethod]
        public void GetRunZoneBudgetItems_HasUndefinedValue()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = @"ModflowTestFiles\GetRunZoneBudgetItems\HasUndefinedValue";
            List<AsrDataMap> asrDataMap = new List<AsrDataMap>
            {
                new AsrDataMap{Key = "KeyA", Name = "NameA"},
                new AsrDataMap{Key = "KeyC", Name = "NameC"},
                new AsrDataMap{Key = "KeyB", Name = "NameB"},
                new AsrDataMap{Key = "KeyD", Name = "NameD"}
            };

            var sut = CreateModelFileAccessor(new Model());

            var result = sut.GetRunZoneBudgetItems(asrDataMap).ToList();
            Assert.AreEqual(1, result.Count);

            Assert.AreEqual(1, result[0].Period);
            Assert.AreEqual(2, result[0].Step);
            Assert.AreEqual("3", result[0].Zone);
            Assert.AreEqual(6, result[0].Values.Count);
            Assert.AreEqual("STORAGE", result[0].Values[0].Key);
            Assert.AreEqual(11.11, result[0].Values[0].Value, .01);
            Assert.AreEqual("RIVER LEAKAGE", result[0].Values[1].Key);
            Assert.AreEqual(22.22, result[0].Values[1].Value, .01);
            Assert.AreEqual("Total IN", result[0].Values[2].Key);
            Assert.AreEqual(33.33, result[0].Values[2].Value, .01);
            Assert.AreEqual("STORAGE", result[0].Values[3].Key);
            Assert.AreEqual(44.44, result[0].Values[3].Value, .01);
            Assert.AreEqual("RIVER LEAKAGE", result[0].Values[4].Key);
            Assert.AreEqual(55.55, result[0].Values[4].Value, .01);
            Assert.AreEqual("Total Out", result[0].Values[5].Key);
            Assert.AreEqual(66.66, result[0].Values[5].Value, .01);
        }

        [TestMethod]
        public void ModpathFileNames_Find()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = @"ModflowTestFiles\Modpath";

            var sut = CreateModelFileAccessor(new Model());

            sut.GetModpathListFileName(SIMULATION_FILE_NAME).Should().Be("COHYST2010_28b_14_28.mplist");
            sut.GetModpathTimeSeriesFileName(SIMULATION_FILE_NAME).Should().Be("COHYST2010_28b_14_28.timeseries7");
            sut.GetModpathLocationFileName(SIMULATION_FILE_NAME).Should().Be("COHYST2010_28b_14_28.sloc");
        }

        [TestMethod]
        public void ModpathTimeSeries()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = @"ModflowTestFiles\Modpath";

            var sut = CreateModelFileAccessor(new Model());

            var ts = sut.GetModpathTimeSeriesResult("COHYST2010_28b_14_28.timeseries7");

            ts.Count.Should().Be(408);

            ts.First().TimePointIndex.Should().Be(0);
            ts.First().CumulativeTimeStep.Should().Be(1);
            ts.First().TrackingTime.Should().Be(0);
            ts.First().SequenceNumber.Should().Be(1);
            ts.First().ParticleGroup.Should().Be(1);
            ts.First().ParticleId.Should().Be(1);
            ts.First().CellNumber.Should().Be(93563);
            ts.First().LocalX.Should().Be(0.2231);
            ts.First().LocalY.Should().Be(0.8599);
            ts.First().LocalZ.Should().Be(0.5);
            ts.First().GlobalX.Should().Be(850668.984);
            ts.First().GlobalY.Should().Be(237230.136);
            ts.First().GlobalZ.Should().Be(2017.697813);
            ts.First().Layer.Should().Be(1);
        }

        [TestMethod]
        public void ReduceMapCells_BasicTest()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = $@"ModflowTestFiles\{ModflowFileType}\FindWellLocations\NoPumpingWellData";
            var sut = CreateModelFileAccessor(new Model());

            var result = sut.ReduceMapCells(1, new List<MapLocationsPositionCellColor>
            {
                new MapLocationsPositionCellColor
                {
                    Color = "red",
                    Locations = new List<string> { GetLocationValue("1") }
                }
            });

            result.Should().NotBeNullOrEmpty();
        }

        [TestMethod]
        public void GetObservedImpactToBaseflow_NonDifferential()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = $@"ModflowTestFiles\GetObservedImpactToBaseflow\NonDifferential";
            var sut = CreateModelFileAccessor(new Model()
            {
                StartDateTime = new DateTime(2020, 5, 1)
            });

            var result = sut.GetObservedImpactToBaseflow(false).ToList();
            Assert.AreEqual(2, result.Count);

            Assert.AreEqual("Observed", result[0].DataSeriesName);
            Assert.AreEqual(1, result[0].Period);
            Assert.AreEqual(-299.19, result[0].FlowToAquiferInAcreFeet);

            Assert.AreEqual("Observed", result[1].DataSeriesName);
            Assert.AreEqual(2, result[1].Period);
            Assert.AreEqual(-99.19, result[1].FlowToAquiferInAcreFeet);
        }

        [TestMethod]
        public void GetObservedImpactToBaseflow_Differential()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = $@"ModflowTestFiles\GetObservedImpactToBaseflow\Differential";
            var sut = CreateModelFileAccessor(new Model()
            {
                StartDateTime = new DateTime(2020, 5, 1)
            });

            var result = sut.GetObservedImpactToBaseflow(true).ToList();
            Assert.AreEqual(2, result.Count);

            Assert.AreEqual("Observed", result[0].DataSeriesName);
            Assert.AreEqual(3, result[0].Period);
            Assert.AreEqual(-1, result[0].FlowToAquiferInAcreFeet);

            Assert.AreEqual("Observed", result[1].DataSeriesName);
            Assert.AreEqual(5, result[1].Period);
            Assert.AreEqual(4, result[1].FlowToAquiferInAcreFeet);
        }

        [TestMethod]
        public void GetObservedImpactToBaseflow_MultiWordDataSeries()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = $@"ModflowTestFiles\GetObservedImpactToBaseflow\MultiWordDataSeries";
            var sut = CreateModelFileAccessor(new Model()
            {
                StartDateTime = new DateTime(2020, 5, 1)
            });

            var result = sut.GetObservedImpactToBaseflow(false).ToList();
            Assert.AreEqual(2, result.Count);

            Assert.AreEqual("Observed Zone A", result[0].DataSeriesName);
            Assert.AreEqual(1, result[0].Period);
            Assert.AreEqual(-299.19, result[0].FlowToAquiferInAcreFeet);

            Assert.AreEqual("Observed Zone A", result[1].DataSeriesName);
            Assert.AreEqual(2, result[1].Period);
            Assert.AreEqual(-99.19, result[1].FlowToAquiferInAcreFeet);
        }

        [TestMethod]
        public void GetObservedImpactToBaseflow_NoFile()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = $@"ModflowTestFiles\GetObservedImpactToBaseflow\DoesntExist";
            var sut = CreateModelFileAccessor(new Model());

            var result = sut.GetObservedImpactToBaseflow(false);
            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetObservedZoneBudget_NonDifferential()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = $@"ModflowTestFiles\GetObservedZoneBudget\NonDifferential";
            var sut = CreateModelFileAccessor(new Model()
            {
                StartDateTime = new DateTime(2020, 5, 1)
            });

            var result = sut.GetObservedZoneBudget(false).ToList();
            Assert.AreEqual(2, result.Count);

            Assert.AreEqual("Recharge - Observed", result[0].BudgetItemSeriesName);
            Assert.AreEqual("Zone A - Observed", result[0].ZoneSeriesName);
            Assert.AreEqual(1, result[0].Period);
            Assert.AreEqual(-299.19, result[0].ValueInAcreFeet);

            Assert.AreEqual("Recharge - Observed", result[1].BudgetItemSeriesName);
            Assert.AreEqual("Zone A - Observed", result[1].ZoneSeriesName);
            Assert.AreEqual(2, result[1].Period);
            Assert.AreEqual(-99.19, result[1].ValueInAcreFeet);
        }

        [TestMethod]
        public void GetObservedZoneBudget_Differential()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = $@"ModflowTestFiles\GetObservedZoneBudget\Differential";
            var sut = CreateModelFileAccessor(new Model()
            {
                StartDateTime = new DateTime(2020, 5, 1)
            });

            var result = sut.GetObservedZoneBudget(true).ToList();
            Assert.AreEqual(2, result.Count);

            Assert.AreEqual("Recharge - Observed", result[0].BudgetItemSeriesName);
            Assert.AreEqual("Zone A - Observed", result[0].ZoneSeriesName);
            Assert.AreEqual(3, result[0].Period);
            Assert.AreEqual(-1, result[0].ValueInAcreFeet);

            Assert.AreEqual("Recharge - Observed", result[1].BudgetItemSeriesName);
            Assert.AreEqual("Zone A - Observed", result[1].ZoneSeriesName);
            Assert.AreEqual(5, result[1].Period);
            Assert.AreEqual(4, result[1].ValueInAcreFeet);
        }

        [TestMethod]
        public void GetObservedZoneBudget_NoFile()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = $@"ModflowTestFiles\GetObservedZoneBudget\DoesntExist";
            var sut = CreateModelFileAccessor(new Model());

            var result = sut.GetObservedZoneBudget(false);
            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetObservedPointsOfInterest_NonDifferential()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = $@"ModflowTestFiles\GetObservedPointsOfInterest\NonDifferential";
            var sut = CreateModelFileAccessor(new Model()
            {
                StartDateTime = new DateTime(2020, 5, 1)
            });

            var result = sut.GetObservedPointsOfInterest(false).ToList();
            Assert.AreEqual(2, result.Count);

            Assert.AreEqual("POI A - Observed", result[0].LocationSeriesName);
            Assert.AreEqual(1, result[0].Period);
            Assert.AreEqual(-299.19, result[0].ValueInCubicFeet);

            Assert.AreEqual("POI A - Observed", result[0].LocationSeriesName);
            Assert.AreEqual(2, result[1].Period);
            Assert.AreEqual(-99.19, result[1].ValueInCubicFeet);
        }

        [TestMethod]
        public void GetObservedPointsOfInterest_Differential()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = $@"ModflowTestFiles\GetObservedPointsOfInterest\Differential";
            var sut = CreateModelFileAccessor(new Model()
            {
                StartDateTime = new DateTime(2020, 5, 1)
            });

            var result = sut.GetObservedPointsOfInterest(true).ToList();
            Assert.AreEqual(2, result.Count);

            Assert.AreEqual("POI A - Observed", result[0].LocationSeriesName);
            Assert.AreEqual(3, result[0].Period);
            Assert.AreEqual(-1, result[0].ValueInCubicFeet);

            Assert.AreEqual("POI A - Observed", result[1].LocationSeriesName);
            Assert.AreEqual(5, result[1].Period);
            Assert.AreEqual(4, result[1].ValueInCubicFeet);
        }

        [TestMethod]
        public void GetObservedPointsOfInterest_NoFile()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = $@"ModflowTestFiles\GetObservedPointsOfInterest\DoesntExist";
            var sut = CreateModelFileAccessor(new Model());

            var result = sut.GetObservedPointsOfInterest(false);
            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetPointsOfInterest()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = $@"ModflowTestFiles\GetPointsOfInterest\BasicTest";
            var sut = CreateModelFileAccessor(new Model());

            var result = sut.GetPointsOfInterest().ToList();
            Assert.AreEqual(2, result.Count);

            Assert.AreEqual("POI 1", result[0].Name);
            Assert.AreEqual(40.850501, result[0].Coordinate.Lat);
            Assert.AreEqual(-100.15596, result[0].Coordinate.Lng);

            Assert.AreEqual("POI 2", result[1].Name);
            Assert.AreEqual(40.785155, result[1].Coordinate.Lat);
            Assert.AreEqual(-100.03202, result[1].Coordinate.Lng);
        }

        [TestMethod]
        public void GetPointsOfInterest_NoFile()
        {
            ConfigurationManager.AppSettings["ModflowDataFolder"] = $@"ModflowTestFiles\GetPointsOfInterest\DoesntExist";
            var sut = CreateModelFileAccessor(new Model());

            var result = sut.GetPointsOfInterest();
            Assert.IsNull(result);
        }
    }
}
