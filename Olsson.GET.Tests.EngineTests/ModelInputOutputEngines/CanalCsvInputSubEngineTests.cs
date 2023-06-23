using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Olsson.GET.Accessors.EntityFramework;
using Olsson.GET.Accessors.FileIO;
using Olsson.GET.Common.DataContracts.Runs;
using Olsson.GET.Common.Utilities;
using Telerik.JustMock;
using Telerik.JustMock.Helpers;
using Olsson.GET.Engines.ModelInputOutputEngines;
using Model = Olsson.GET.Common.DataContracts.Models.Model;
using ModelStressPeriodCustomStartDate = Olsson.GET.Common.DataContracts.Models.ModelStressPeriodCustomStartDate;
using Run = Olsson.GET.Common.DataContracts.Runs.Run;
using Scenario = Olsson.GET.Common.DataContracts.Scenarios.Scenario;

namespace Olsson.GET.Tests.EngineTests.ModelInputOutputEngines
{
    [TestClass]
    public class CanalCsvInputSubEngineTests
    {
        private readonly IModelFileAccessor _modflowFileAccessorMock = Mock.Create<IModelFileAccessor>(Behavior.Strict);
        private readonly IBlobFileAccessor _fileAccessorMock = Mock.Create<IBlobFileAccessor>(Behavior.Strict);

        private readonly Model _model = new Model
        {
            StartDateTime = new DateTime(2011, 1, 1),
            NumberOfStressPeriods = 1
        };

        private Run _run = new Run
        {
            FileStorageLocator = "fakeLocator",
            Scenario = new Scenario
            {
                ShouldSwitchSign = false
            },
            InputVolumeUnitID = VolumeUnit.AcreFeet.VolumeUnitID,
            OutputVolumeUnitID = VolumeUnit.AcreFeet.VolumeUnitID
        };

        private CanalCsvInputSubEngine CreateCanalCsvInputSubEngine()
        {
            var modflowFileAccessorFactory = Mock.Create<IModelFileAccessorFactory>();
            modflowFileAccessorFactory.Arrange(a => a.CreateModflowFileAccessor(Arg.IsAny<Model>()))
                .Returns(_modflowFileAccessorMock);

            _modflowFileAccessorMock.Arrange(a => a.GetAsrDataNameMap())
                .Returns(new List<AsrDataMap>());

            var sut = new CanalCsvInputSubEngine(_model);
            return sut;
        }

        [TestMethod]
        public void UpdateFlowInputs_BasicInput()
        {
            ConfigurationHelper.AppSettings.BlobStorageModelDataFolder = "fakeModelDataFolder";
            var existingFlows = new StressPeriodsLocationRates
            {
                StressPeriods = new List<StressPeriodLocationRates>
                {
                    new StressPeriodLocationRates
                    {
                        LocationRates = new List<LocationRate>
                        {
                            new LocationRate
                            {
                                Location = "1",
                                Rate = 1.23
                            }
                        }
                    }
                }
            };
            _modflowFileAccessorMock.Arrange(a => a.GetLocationProportions("FakeFeature"))
                .Returns(new List<LocationProportion>
                {
                    new LocationProportion {Location = "2", Proportion = 1}
                });
            _fileAccessorMock.Arrange(a => a.GetFile("fakeLocator/inputs/inputs.json", "fakeModelDataFolder"))
                .ReturnsAsync(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new List<RunCanalInput>
                {
                    new RunCanalInput
                    {
                        Month = 1,
                        Year = 2011,
                        Values = new List<FeatureValue>
                        {
                            new FeatureValue
                            {
                                FeatureName = "FakeFeature",
                                Value = .3456
                            }
                        }
                    }
                })));
            var sut = CreateCanalCsvInputSubEngine();
            var updateNodeRatesResult = sut.UpdateFlowInputs(_modflowFileAccessorMock, _fileAccessorMock, existingFlows, _run);

            Assert.IsNotNull(updateNodeRatesResult);
            Assert.AreEqual(1, updateNodeRatesResult.StressPeriods.Count);
            Assert.AreEqual(2, updateNodeRatesResult.StressPeriods[0].LocationRates.Count);
            Assert.AreEqual("2", updateNodeRatesResult.StressPeriods[0].LocationRates[0].Location);
            TestUtilities.AssertAreEqualWithCalculatedDelta(485.622, updateNodeRatesResult.StressPeriods[0].LocationRates[0].Rate);
            Assert.AreEqual("1", updateNodeRatesResult.StressPeriods[0].LocationRates[1].Location);
            TestUtilities.AssertAreEqualWithCalculatedDelta(1.23, updateNodeRatesResult.StressPeriods[0].LocationRates[1].Rate);
        }

        [TestMethod]
        public void UpdateFlowInputs_LeapYear()
        {
            ConfigurationHelper.AppSettings.BlobStorageModelDataFolder = "fakeModelDataFolder";
            _model.StartDateTime = new DateTime(2012, 2, 1);

            var existingFlows = new StressPeriodsLocationRates
            {
                StressPeriods = new List<StressPeriodLocationRates>
                {
                    new StressPeriodLocationRates
                    {
                        LocationRates = new List<LocationRate>
                        {
                            new LocationRate
                            {
                                Location = "1",
                                Rate = 1.23
                            }
                        }
                    }
                }
            };
            _modflowFileAccessorMock.Arrange(a => a.GetLocationProportions("FakeFeature"))
                .Returns(new List<LocationProportion>
                {
                    new LocationProportion {Location = "2", Proportion = 1}
                });
            _fileAccessorMock.Arrange(a => a.GetFile("fakeLocator/inputs/inputs.json", "fakeModelDataFolder"))
                .ReturnsAsync(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new List<RunCanalInput>
                {
                    new RunCanalInput
                    {
                        Month = 2,
                        Year = 2012,
                        Values = new List<FeatureValue>
                        {
                            new FeatureValue
                            {
                                FeatureName = "FakeFeature",
                                Value = .3456
                            }
                        }
                    }
                })));
            var sut = CreateCanalCsvInputSubEngine();
            var updateNodeRatesResult = sut.UpdateFlowInputs(_modflowFileAccessorMock, _fileAccessorMock, existingFlows, _run);

            Assert.IsNotNull(updateNodeRatesResult);
            Assert.AreEqual(1, updateNodeRatesResult.StressPeriods.Count);
            Assert.AreEqual(2, updateNodeRatesResult.StressPeriods[0].LocationRates.Count);
            Assert.AreEqual("2", updateNodeRatesResult.StressPeriods[0].LocationRates[0].Location);
            TestUtilities.AssertAreEqualWithCalculatedDelta(519.113, updateNodeRatesResult.StressPeriods[0].LocationRates[0].Rate);
            Assert.AreEqual("1", updateNodeRatesResult.StressPeriods[0].LocationRates[1].Location);
            TestUtilities.AssertAreEqualWithCalculatedDelta(1.23, updateNodeRatesResult.StressPeriods[0].LocationRates[1].Rate);
        }

        [TestMethod]
        public void UpdateFlowInputs_OverOneYear()
        {
            ConfigurationHelper.AppSettings.BlobStorageModelDataFolder = "fakeModelDataFolder";
            var existingFlows = new StressPeriodsLocationRates
            {
                StressPeriods = new List<StressPeriodLocationRates>
                    {
                        new StressPeriodLocationRates
                        {
                            LocationRates = new List<LocationRate>
                            {
                                new LocationRate
                                {
                                    Location = "1",
                                    Rate = 1.23
                                }
                            }
                        },
                        new StressPeriodLocationRates
                        {
                            LocationRates = new List<LocationRate>
                            {
                                new LocationRate
                                {
                                    Location = "1",
                                    Rate = 1.23
                                }
                            }
                        },
                        new StressPeriodLocationRates
                        {
                            LocationRates = new List<LocationRate>
                            {
                                new LocationRate
                                {
                                    Location = "1",
                                    Rate = 1.23
                                }
                            }
                        },
                        new StressPeriodLocationRates
                        {
                            LocationRates = new List<LocationRate>
                            {
                                new LocationRate
                                {
                                    Location = "1",
                                    Rate = 1.23
                                }
                            }
                        },
                        new StressPeriodLocationRates
                        {
                            LocationRates = new List<LocationRate>
                            {
                                new LocationRate
                                {
                                    Location = "1",
                                    Rate = 1.23
                                }
                            }
                        },
                        new StressPeriodLocationRates
                        {
                            LocationRates = new List<LocationRate>
                            {
                                new LocationRate
                                {
                                    Location = "1",
                                    Rate = 1.23
                                }
                            }
                        },
                        new StressPeriodLocationRates
                        {
                            LocationRates = new List<LocationRate>
                            {
                                new LocationRate
                                {
                                    Location = "1",
                                    Rate = 1.23
                                }
                            }
                        },
                        new StressPeriodLocationRates
                        {
                            LocationRates = new List<LocationRate>
                            {
                                new LocationRate
                                {
                                    Location = "1",
                                    Rate = 1.23
                                }
                            }
                        },
                        new StressPeriodLocationRates
                        {
                            LocationRates = new List<LocationRate>
                            {
                                new LocationRate
                                {
                                    Location = "1",
                                    Rate = 1.23
                                }
                            }
                        },
                        new StressPeriodLocationRates
                        {
                            LocationRates = new List<LocationRate>
                            {
                                new LocationRate
                                {
                                    Location = "1",
                                    Rate = 1.23
                                }
                            }
                        },
                        new StressPeriodLocationRates
                        {
                            LocationRates = new List<LocationRate>
                            {
                                new LocationRate
                                {
                                    Location = "1",
                                    Rate = 1.23
                                }
                            }
                        },
                        new StressPeriodLocationRates
                        {
                            LocationRates = new List<LocationRate>
                            {
                                new LocationRate
                                {
                                    Location = "1",
                                    Rate = 1.23
                                }
                            }
                        },
                        new StressPeriodLocationRates
                        {
                            LocationRates = new List<LocationRate>
                            {
                                new LocationRate
                                {
                                    Location = "1",
                                    Rate = 1.23
                                }
                            }
                        }
                    }
            };
            _modflowFileAccessorMock.Arrange(a => a.GetLocationProportions("FakeFeature"))
                .Returns(new List<LocationProportion>
                {
                    new LocationProportion {Location = "2", Proportion = 1}
                });
            _fileAccessorMock.Arrange(a => a.GetFile("fakeLocator/inputs/inputs.json", "fakeModelDataFolder"))
                .ReturnsAsync(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new List<RunCanalInput>
                {
                    new RunCanalInput
                    {
                        Month = 1,
                        Year = 2012,
                        Values = new List<FeatureValue>
                        {
                            new FeatureValue
                            {
                                FeatureName = "FakeFeature",
                                Value = .3456
                            }
                        }
                    }
                })));
            var sut = CreateCanalCsvInputSubEngine();
            var updateNodeRatesResult = sut.UpdateFlowInputs(_modflowFileAccessorMock, _fileAccessorMock, existingFlows, _run);

            Assert.IsNotNull(updateNodeRatesResult);
            Assert.AreEqual(13, updateNodeRatesResult.StressPeriods.Count);
            Assert.AreEqual(2, updateNodeRatesResult.StressPeriods[12].LocationRates.Count);
            Assert.AreEqual("2", updateNodeRatesResult.StressPeriods[12].LocationRates[0].Location);
            TestUtilities.AssertAreEqualWithCalculatedDelta(485.622, updateNodeRatesResult.StressPeriods[12].LocationRates[0].Rate);
            Assert.AreEqual("1", updateNodeRatesResult.StressPeriods[12].LocationRates[1].Location);
            TestUtilities.AssertAreEqualWithCalculatedDelta(1.23, updateNodeRatesResult.StressPeriods[12].LocationRates[1].Rate);
        }

        [TestMethod]
        public void UpdateFlowInputs_DateTooLate_ZeroValue()
        {
            ConfigurationHelper.AppSettings.BlobStorageModelDataFolder = "fakeModelDataFolder";
            var existingFlows = new StressPeriodsLocationRates
            {
                StressPeriods = new List<StressPeriodLocationRates>
                    {
                        new StressPeriodLocationRates
                        {
                            LocationRates = new List<LocationRate>
                            {
                                new LocationRate
                                {
                                    Location = "1",
                                    Rate = 1.23
                                }
                            }
                        }
                    }
            };
            _modflowFileAccessorMock.Arrange(a => a.GetLocationProportions("FakeFeature"))
                .Returns(new List<LocationProportion>
                {
                    new LocationProportion {Location = "2", Proportion = 1}
                });
            _fileAccessorMock.Arrange(a => a.GetFile("fakeLocator/inputs/inputs.json", "fakeModelDataFolder"))
                .ReturnsAsync(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new List<Olsson.GET.Common.DataContracts.Runs.RunCanalInput>
                {
                    new RunCanalInput
                    {
                        Month = 2,
                        Year = 2011,
                        Values = new List<FeatureValue>
                        {
                            new FeatureValue
                            {
                                FeatureName = "FakeFeature",
                                Value = 0
                            }
                        }
                    }
                })));
            var sut = CreateCanalCsvInputSubEngine();
            var updateNodeRatesResult = sut.UpdateFlowInputs(_modflowFileAccessorMock, _fileAccessorMock, existingFlows, _run);

            Assert.IsNotNull(updateNodeRatesResult);
            Assert.AreEqual(1, updateNodeRatesResult.StressPeriods.Count);
            Assert.AreEqual(1, updateNodeRatesResult.StressPeriods[0].LocationRates.Count);
            Assert.AreEqual("1", updateNodeRatesResult.StressPeriods[0].LocationRates[0].Location);
            TestUtilities.AssertAreEqualWithCalculatedDelta(1.23, updateNodeRatesResult.StressPeriods[0].LocationRates[0].Rate);
        }

        [TestMethod]
        public void UpdateFlowInputs_DateTooLate_NonZeroValue()
        {
            ConfigurationHelper.AppSettings.BlobStorageModelDataFolder = "fakeModelDataFolder";
            var existingFlows = new StressPeriodsLocationRates
            {
                StressPeriods = new List<StressPeriodLocationRates>
                    {
                        new StressPeriodLocationRates
                        {
                            LocationRates = new List<LocationRate>
                            {
                                new LocationRate
                                {
                                    Location = "1",
                                    Rate = 1.23
                                }
                            }
                        }
                    }
            };
            _modflowFileAccessorMock.Arrange(a => a.GetLocationProportions("FakeFeature"))
                .Returns(new List<LocationProportion>
                {
                    new LocationProportion {Location = "2", Proportion = 1}
                });
            _fileAccessorMock.Arrange(a => a.GetFile("fakeLocator/inputs/inputs.json", "fakeModelDataFolder"))
                .ReturnsAsync(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new List<RunCanalInput>
                {
                    new RunCanalInput
                    {
                        Month = 2,
                        Year = 2011,
                        Values = new List<FeatureValue>
                        {
                            new FeatureValue
                            {
                                FeatureName = "FakeFeature",
                                Value = 0.0001
                            }
                        }
                    }
                })));
            var sut = CreateCanalCsvInputSubEngine();
            Assert.ThrowsException<InputDataInvalidException>(() => sut.UpdateFlowInputs(_modflowFileAccessorMock, _fileAccessorMock, existingFlows, _run));
        }

        [TestMethod]
        public void UpdateFlowInputs_DateTooEarly_ZeroValue()
        {
            ConfigurationHelper.AppSettings.BlobStorageModelDataFolder = "fakeModelDataFolder";
            var existingFlows = new StressPeriodsLocationRates
            {
                StressPeriods = new List<StressPeriodLocationRates>
                    {
                        new StressPeriodLocationRates
                        {
                            LocationRates = new List<LocationRate>
                            {
                                new LocationRate
                                {
                                    Location = "1",
                                    Rate = 1.23
                                }
                            }
                        }
                    }
            };
            _modflowFileAccessorMock.Arrange(a => a.GetLocationProportions("FakeFeature"))
                .Returns(new List<LocationProportion>
                {
                    new LocationProportion {Location = "2", Proportion = 1}
                });
            _fileAccessorMock.Arrange(a => a.GetFile("fakeLocator/inputs/inputs.json", "fakeModelDataFolder"))
                .ReturnsAsync(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new List<RunCanalInput>
                {
                    new RunCanalInput
                    {
                        Month = 12,
                        Year = 2010,
                        Values = new List<FeatureValue>
                        {
                            new FeatureValue
                            {
                                FeatureName = "FakeFeature",
                                Value = 0
                            }
                        }
                    }
                })));
            var sut = CreateCanalCsvInputSubEngine();
            var updateNodeRatesResult = sut.UpdateFlowInputs(_modflowFileAccessorMock, _fileAccessorMock, existingFlows, _run);

            Assert.IsNotNull(updateNodeRatesResult);
            Assert.AreEqual(1, updateNodeRatesResult.StressPeriods.Count);
            Assert.AreEqual(1, updateNodeRatesResult.StressPeriods[0].LocationRates.Count);
            Assert.AreEqual("1", updateNodeRatesResult.StressPeriods[0].LocationRates[0].Location);
            Assert.AreEqual(1.23, updateNodeRatesResult.StressPeriods[0].LocationRates[0].Rate, .001);
        }

        [TestMethod]
        public void UpdateFlowInputs_DateTooEarly_NonZeroValue()
        {
            ConfigurationHelper.AppSettings.BlobStorageModelDataFolder = "fakeModelDataFolder";
            var existingFlows = new StressPeriodsLocationRates
            {
                StressPeriods = new List<StressPeriodLocationRates>
                    {
                        new StressPeriodLocationRates
                        {
                            LocationRates = new List<LocationRate>
                            {
                                new LocationRate
                                {
                                    Location = "1",
                                    Rate = 1.23
                                }
                            }
                        }
                    }
            };
            _modflowFileAccessorMock.Arrange(a => a.GetLocationProportions("FakeFeature"))
                .Returns(new List<LocationProportion>
                {
                    new LocationProportion {Location = "2", Proportion = 1}
                });
            _fileAccessorMock.Arrange(a => a.GetFile("fakeLocator/inputs/inputs.json", "fakeModelDataFolder"))
                .ReturnsAsync(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new List<RunCanalInput>
                {
                    new RunCanalInput
                    {
                        Month = 12,
                        Year = 2010,
                        Values = new List<FeatureValue>
                        {
                            new FeatureValue
                            {
                                FeatureName = "FakeFeature",
                                Value = 0.0001
                            }
                        }
                    }
                })));
            var sut = CreateCanalCsvInputSubEngine();
            Assert.ThrowsException<InputDataInvalidException>(() => sut.UpdateFlowInputs(_modflowFileAccessorMock, _fileAccessorMock, existingFlows, _run));
        }

        [TestMethod]
        public void UpdateFlowInputs_ZeroInput()
        {
            ConfigurationHelper.AppSettings.BlobStorageModelDataFolder = "fakeModelDataFolder";
            var existingFlows = new StressPeriodsLocationRates
            {
                StressPeriods = new List<StressPeriodLocationRates>
                    {
                        new StressPeriodLocationRates
                        {
                            LocationRates = new List<LocationRate>
                            {
                                new LocationRate
                                {
                                    Location = "1",
                                    Rate = 1.23
                                }
                            }
                        }
                    }
            };
            _modflowFileAccessorMock.Arrange(a => a.GetLocationProportions("FakeFeature"))
                .Returns(new List<LocationProportion>
                {
                    new LocationProportion {Location = "2", Proportion = 1}
                });
            _fileAccessorMock.Arrange(a => a.GetFile("fakeLocator/inputs/inputs.json", "fakeModelDataFolder"))
                .ReturnsAsync(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new List<RunCanalInput>
                {
                    new RunCanalInput
                    {
                        Month = 1,
                        Year = 2011,
                        Values = new List<FeatureValue>
                        {
                            new FeatureValue
                            {
                                FeatureName = "FakeFeature",
                                Value = 0
                            }
                        }
                    }
                })));
            var sut = CreateCanalCsvInputSubEngine();
            var updateNodeRatesResult = sut.UpdateFlowInputs(_modflowFileAccessorMock, _fileAccessorMock, existingFlows, _run);

            Assert.IsNotNull(updateNodeRatesResult);
            Assert.AreEqual(1, updateNodeRatesResult.StressPeriods.Count);
            Assert.AreEqual(1, updateNodeRatesResult.StressPeriods[0].LocationRates.Count);
            Assert.AreEqual("1", updateNodeRatesResult.StressPeriods[0].LocationRates[0].Location);
            Assert.AreEqual(1.23, updateNodeRatesResult.StressPeriods[0].LocationRates[0].Rate, .001);
        }

        [TestMethod]
        public void UpdateFlowInputs_SameNode()
        {
            ConfigurationHelper.AppSettings.BlobStorageModelDataFolder = "fakeModelDataFolder";
            var existingFlows = new StressPeriodsLocationRates
            {
                StressPeriods = new List<StressPeriodLocationRates>
                    {
                        new StressPeriodLocationRates
                        {
                            LocationRates = new List<LocationRate>
                            {
                                new LocationRate
                                {
                                    Location = "1",
                                    Rate = 1.23
                                }
                            }
                        }
                    }
            };
            _modflowFileAccessorMock.Arrange(a => a.GetLocationProportions("FakeFeature"))
                .Returns(new List<LocationProportion>
                {
                    new LocationProportion {Location = "1", Proportion = 1}
                });
            _fileAccessorMock.Arrange(a => a.GetFile("fakeLocator/inputs/inputs.json", "fakeModelDataFolder"))
                .ReturnsAsync(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new List<RunCanalInput>
                {
                    new RunCanalInput
                    {
                        Month = 1,
                        Year = 2011,
                        Values = new List<FeatureValue>
                        {
                            new FeatureValue
                            {
                                FeatureName = "FakeFeature",
                                Value = .3456
                            }
                        }
                    }
                })));
            var sut = CreateCanalCsvInputSubEngine();
            var updateNodeRatesResult = sut.UpdateFlowInputs(_modflowFileAccessorMock, _fileAccessorMock, existingFlows, _run);

            Assert.IsNotNull(updateNodeRatesResult);
            Assert.AreEqual(1, updateNodeRatesResult.StressPeriods.Count);
            Assert.AreEqual(2, updateNodeRatesResult.StressPeriods[0].LocationRates.Count);
            Assert.AreEqual("1", updateNodeRatesResult.StressPeriods[0].LocationRates[0].Location);
            TestUtilities.AssertAreEqualWithCalculatedDelta(485.622, updateNodeRatesResult.StressPeriods[0].LocationRates[0].Rate);
            Assert.AreEqual("1", updateNodeRatesResult.StressPeriods[0].LocationRates[1].Location);
            TestUtilities.AssertAreEqualWithCalculatedDelta(1.23, updateNodeRatesResult.StressPeriods[0].LocationRates[1].Rate);
        }

        [TestMethod]
        public void UpdateFlowInputs_MultipleDates()
        {
            ConfigurationHelper.AppSettings.BlobStorageModelDataFolder = "fakeModelDataFolder";
            var existingFlows = new StressPeriodsLocationRates
            {
                StressPeriods = new List<StressPeriodLocationRates>
                    {
                        new StressPeriodLocationRates
                        {
                            LocationRates = new List<LocationRate>
                            {
                                new LocationRate
                                {
                                    Location = "1",
                                    Rate = 1.23
                                }
                            }
                        },
                        new StressPeriodLocationRates
                        {
                            LocationRates = new List<LocationRate>
                            {
                                new LocationRate
                                {
                                    Location = "1",
                                    Rate = 2.34
                                }
                            }
                        }
                    }
            };
            _modflowFileAccessorMock.Arrange(a => a.GetLocationProportions("FakeFeature"))
                .Returns(new List<LocationProportion>
                {
                    new LocationProportion {Location = "2", Proportion = 1}
                });
            _fileAccessorMock.Arrange(a => a.GetFile("fakeLocator/inputs/inputs.json", "fakeModelDataFolder"))
                .ReturnsAsync(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new List<RunCanalInput>
                {
                    new RunCanalInput
                    {
                        Month = 1,
                        Year = 2011,
                        Values = new List<FeatureValue>
                        {
                            new FeatureValue
                            {
                                FeatureName = "FakeFeature",
                                Value = .3456
                            }
                        }
                    },
                    new RunCanalInput
                    {
                        Month = 2,
                        Year = 2011,
                        Values = new List<FeatureValue>
                        {
                            new FeatureValue
                            {
                                FeatureName = "FakeFeature",
                                Value = .1122
                            }
                        }
                    }
                })));
            var sut = CreateCanalCsvInputSubEngine();
            var updateNodeRatesResult = sut.UpdateFlowInputs(_modflowFileAccessorMock, _fileAccessorMock, existingFlows, _run);

            Assert.IsNotNull(updateNodeRatesResult);
            Assert.AreEqual(2, updateNodeRatesResult.StressPeriods.Count);

            Assert.AreEqual(2, updateNodeRatesResult.StressPeriods[0].LocationRates.Count);
            Assert.AreEqual("2", updateNodeRatesResult.StressPeriods[0].LocationRates[0].Location);
            TestUtilities.AssertAreEqualWithCalculatedDelta(485.622, updateNodeRatesResult.StressPeriods[0].LocationRates[0].Rate);
            Assert.AreEqual("1", updateNodeRatesResult.StressPeriods[0].LocationRates[1].Location);
            TestUtilities.AssertAreEqualWithCalculatedDelta(1.23, updateNodeRatesResult.StressPeriods[0].LocationRates[1].Rate);

            Assert.AreEqual(2, updateNodeRatesResult.StressPeriods[1].LocationRates.Count);
            Assert.AreEqual("2", updateNodeRatesResult.StressPeriods[1].LocationRates[0].Location);
            TestUtilities.AssertAreEqualWithCalculatedDelta(174.551, updateNodeRatesResult.StressPeriods[1].LocationRates[0].Rate);
            Assert.AreEqual("1", updateNodeRatesResult.StressPeriods[1].LocationRates[1].Location);
            TestUtilities.AssertAreEqualWithCalculatedDelta(2.34, updateNodeRatesResult.StressPeriods[1].LocationRates[1].Rate);
        }

        [TestMethod]
        public void UpdateFlowInputs_MultipleDates_OverTwoYears()
        {
            ConfigurationHelper.AppSettings.BlobStorageModelDataFolder = "fakeModelDataFolder";
            _model.ModelStressPeriodCustomStartDates = new List<ModelStressPeriodCustomStartDate>
            {
                new ModelStressPeriodCustomStartDate()
                {
                    StressPeriod = 1,
                    StressPeriodStartDate = new DateTime(2011, 1,  1)
                }, 
                new ModelStressPeriodCustomStartDate()
                {
                    StressPeriod = 2,
                    StressPeriodStartDate = new DateTime(2012, 1, 1 )
                }
            };
            var existingFlows = new StressPeriodsLocationRates
            {
                StressPeriods = new List<StressPeriodLocationRates>
                    {
                        new StressPeriodLocationRates
                        {
                            LocationRates = new List<LocationRate>
                            {
                                new LocationRate
                                {
                                    Location = "1",
                                    Rate = 1.23
                                }
                            }
                        },
                        new StressPeriodLocationRates
                        {
                            LocationRates = new List<LocationRate>
                            {
                                new LocationRate
                                {
                                    Location = "1",
                                    Rate = 2.34
                                }
                            }
                        }
                    }
            };
            _modflowFileAccessorMock.Arrange(a => a.GetLocationProportions("FakeFeature"))
                .Returns(new List<LocationProportion>
                {
                    new LocationProportion {Location = "2", Proportion = 1}
                });
            _fileAccessorMock.Arrange(a => a.GetFile("fakeLocator/inputs/inputs.json", "fakeModelDataFolder"))
                .ReturnsAsync(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new List<RunCanalInput>
                {
                    new RunCanalInput
                    {
                        Month = 1,
                        Year = 2011,
                        Values = new List<FeatureValue>
                        {
                            new FeatureValue
                            {
                                FeatureName = "FakeFeature",
                                Value = .3456
                            }
                        }
                    },
                    new RunCanalInput
                    {
                        Month = 1,
                        Year = 2012,
                        Values = new List<FeatureValue>
                        {
                            new FeatureValue
                            {
                                FeatureName = "FakeFeature",
                                Value = .1122
                            }
                        }
                    }
                })));
            var sut = CreateCanalCsvInputSubEngine();
            var updateNodeRatesResult = sut.UpdateFlowInputs(_modflowFileAccessorMock, _fileAccessorMock, existingFlows, _run);

            Assert.IsNotNull(updateNodeRatesResult);
            Assert.AreEqual(2, updateNodeRatesResult.StressPeriods.Count);

            Assert.AreEqual(2, updateNodeRatesResult.StressPeriods[0].LocationRates.Count);
            Assert.AreEqual("2", updateNodeRatesResult.StressPeriods[0].LocationRates[0].Location);
            TestUtilities.AssertAreEqualWithCalculatedDelta(485.622, updateNodeRatesResult.StressPeriods[0].LocationRates[0].Rate);
            Assert.AreEqual("1", updateNodeRatesResult.StressPeriods[0].LocationRates[1].Location);
            TestUtilities.AssertAreEqualWithCalculatedDelta(1.23, updateNodeRatesResult.StressPeriods[0].LocationRates[1].Rate);

            Assert.AreEqual(2, updateNodeRatesResult.StressPeriods[1].LocationRates.Count);
            Assert.AreEqual("2", updateNodeRatesResult.StressPeriods[1].LocationRates[0].Location);
            TestUtilities.AssertAreEqualWithCalculatedDelta(157.659, updateNodeRatesResult.StressPeriods[1].LocationRates[0].Rate);
            Assert.AreEqual("1", updateNodeRatesResult.StressPeriods[1].LocationRates[1].Location);
            TestUtilities.AssertAreEqualWithCalculatedDelta(2.34, updateNodeRatesResult.StressPeriods[1].LocationRates[1].Rate);
        }

        [TestMethod]
        public void UpdateFlowInputs_ProportionedCanalInput()
        {
            ConfigurationHelper.AppSettings.BlobStorageModelDataFolder = "fakeModelDataFolder";
            var existingFlows = new StressPeriodsLocationRates
            {
                StressPeriods = new List<StressPeriodLocationRates>
                    {
                        new StressPeriodLocationRates
                        {
                            LocationRates = new List<LocationRate>
                            {
                                new LocationRate
                                {
                                    Location = "1",
                                    Rate = 1.23
                                }
                            }
                        }
                    }
            };
            _modflowFileAccessorMock.Arrange(a => a.GetLocationProportions("FakeFeature"))
                .Returns(new List<LocationProportion>
                {
                    new LocationProportion {Location = "2", Proportion = .75},
                    new LocationProportion {Location = "3", Proportion = .25}
                });
            _fileAccessorMock.Arrange(a => a.GetFile("fakeLocator/inputs/inputs.json", "fakeModelDataFolder"))
                .ReturnsAsync(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new List<RunCanalInput>
                {
                    new RunCanalInput
                    {
                        Month = 1,
                        Year = 2011,
                        Values = new List<FeatureValue>
                        {
                            new FeatureValue
                            {
                                FeatureName = "FakeFeature",
                                Value = .3456
                            }
                        }
                    }
                })));
            var sut = CreateCanalCsvInputSubEngine();
            var updateNodeRatesResult = sut.UpdateFlowInputs(_modflowFileAccessorMock, _fileAccessorMock, existingFlows, _run);

            Assert.IsNotNull(updateNodeRatesResult);
            Assert.AreEqual(1, updateNodeRatesResult.StressPeriods.Count);
            Assert.AreEqual(3, updateNodeRatesResult.StressPeriods[0].LocationRates.Count);
            Assert.AreEqual("3", updateNodeRatesResult.StressPeriods[0].LocationRates[0].Location);
            TestUtilities.AssertAreEqualWithCalculatedDelta(121.406, updateNodeRatesResult.StressPeriods[0].LocationRates[0].Rate);
            Assert.AreEqual("2", updateNodeRatesResult.StressPeriods[0].LocationRates[1].Location);
            TestUtilities.AssertAreEqualWithCalculatedDelta(364.216, updateNodeRatesResult.StressPeriods[0].LocationRates[1].Rate);
            Assert.AreEqual("1", updateNodeRatesResult.StressPeriods[0].LocationRates[2].Location);
            Assert.AreEqual(1.23, updateNodeRatesResult.StressPeriods[0].LocationRates[2].Rate, .001);
        }

        [TestMethod]
        public void UpdateFlowInputs_IsClnWell()
        {
            ConfigurationHelper.AppSettings.BlobStorageModelDataFolder = "fakeModelDataFolder";
            var existingFlows = new StressPeriodsLocationRates
            {
                StressPeriods = new List<StressPeriodLocationRates>
                {
                    new StressPeriodLocationRates
                    {
                        ClnLocationRates = new List<LocationRate>
                        {
                            new LocationRate
                            {
                                Location = "1",
                                Rate = 1.23
                            }
                        },
                        LocationRates = new List<LocationRate>()
                    }
                }
            };
            _modflowFileAccessorMock.Arrange(a => a.GetLocationProportions("FakeFeature"))
                .Returns(new List<LocationProportion>
                {
                    new LocationProportion {Location = "2", Proportion = 1, IsClnWell = true}
                });
            _fileAccessorMock.Arrange(a => a.GetFile("fakeLocator/inputs/inputs.json", "fakeModelDataFolder"))
                .ReturnsAsync(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new List<RunCanalInput>
                {
                    new RunCanalInput
                    {
                        Month = 1,
                        Year = 2011,
                        Values = new List<FeatureValue>
                        {
                            new FeatureValue
                            {
                                FeatureName = "FakeFeature",
                                Value = .3456
                            }
                        }
                    }
                })));
            var sut = CreateCanalCsvInputSubEngine();
            var updateNodeRatesResult = sut.UpdateFlowInputs(_modflowFileAccessorMock, _fileAccessorMock, existingFlows, _run);

            Assert.IsNotNull(updateNodeRatesResult);
            Assert.AreEqual(1, updateNodeRatesResult.StressPeriods.Count);
            Assert.AreEqual(2, updateNodeRatesResult.StressPeriods[0].ClnLocationRates.Count);
            Assert.AreEqual("2", updateNodeRatesResult.StressPeriods[0].ClnLocationRates[0].Location);
            TestUtilities.AssertAreEqualWithCalculatedDelta(485.622, updateNodeRatesResult.StressPeriods[0].ClnLocationRates[0].Rate);
            Assert.AreEqual("1", updateNodeRatesResult.StressPeriods[0].ClnLocationRates[1].Location);
            TestUtilities.AssertAreEqualWithCalculatedDelta(1.23, updateNodeRatesResult.StressPeriods[0].ClnLocationRates[1].Rate);

            Assert.AreEqual(0, updateNodeRatesResult.StressPeriods[0].LocationRates.Count);
        }

        [TestMethod]
        public void UpdateFlowInputs_BasicInput_ShouldSwitchSign()
        {
            ConfigurationHelper.AppSettings.BlobStorageModelDataFolder = "fakeModelDataFolder";
            var existingFlows = new StressPeriodsLocationRates
            {
                StressPeriods = new List<StressPeriodLocationRates>
                {
                    new StressPeriodLocationRates
                    {
                        LocationRates = new List<LocationRate>
                        {
                            new LocationRate
                            {
                                Location = "1",
                                Rate = 1.23
                            }
                        }
                    }
                }
            };
            _modflowFileAccessorMock.Arrange(a => a.GetLocationProportions("FakeFeature"))
                .Returns(new List<LocationProportion>
                {
                    new LocationProportion {Location = "2", Proportion = 1}
                });
            _fileAccessorMock.Arrange(a => a.GetFile("fakeLocator/inputs/inputs.json", "fakeModelDataFolder"))
                .ReturnsAsync(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new List<RunCanalInput>
                {
                    new RunCanalInput
                    {
                        Month = 1,
                        Year = 2011,
                        Values = new List<FeatureValue>
                        {
                            new FeatureValue
                            {
                                FeatureName = "FakeFeature",
                                Value = .3456
                            }
                        }
                    }
                })));

            _run.Scenario.ShouldSwitchSign = true;

            var sut = CreateCanalCsvInputSubEngine();
            var updateNodeRatesResult = sut.UpdateFlowInputs(_modflowFileAccessorMock, _fileAccessorMock, existingFlows, _run);

            Assert.IsNotNull(updateNodeRatesResult);
            Assert.AreEqual(1, updateNodeRatesResult.StressPeriods.Count);
            Assert.AreEqual(2, updateNodeRatesResult.StressPeriods[0].LocationRates.Count);
            Assert.AreEqual("2", updateNodeRatesResult.StressPeriods[0].LocationRates[0].Location);
            TestUtilities.AssertAreEqualWithCalculatedDelta(-485.622, updateNodeRatesResult.StressPeriods[0].LocationRates[0].Rate);
            Assert.AreEqual("1", updateNodeRatesResult.StressPeriods[0].LocationRates[1].Location);
            TestUtilities.AssertAreEqualWithCalculatedDelta(1.23, updateNodeRatesResult.StressPeriods[0].LocationRates[1].Rate);
        }
    }
}
