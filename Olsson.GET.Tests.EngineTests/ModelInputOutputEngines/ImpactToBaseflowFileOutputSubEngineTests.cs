using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Olsson.GET.Accessors.EntityFramework;
using Olsson.GET.Accessors.FileIO;
using Olsson.GET.Common.DataContracts.Runs;
using Olsson.GET.Engines.ModelInputOutputEngines;
using Telerik.JustMock;
using Telerik.JustMock.Helpers;
using Model = Olsson.GET.Common.DataContracts.Models.Model;
using ModelStressPeriodCustomStartDate = Olsson.GET.Common.DataContracts.Models.ModelStressPeriodCustomStartDate;

namespace Olsson.GET.Tests.EngineTests.ModelInputOutputEngines
{
    [TestClass]
    public class ImpactToBaseflowFileOutputSubEngineTests
    {
        private List<StressPeriod> _getStressPeriodDataResult = new List<StressPeriod>
        {
            new StressPeriod
            {
                Days = 31,
                NumberOfTimeSteps = 1
            },
            new StressPeriod
            {
                Days = 28,
                NumberOfTimeSteps = 1
            }
        };

        private List<StressPeriod> _getStressPeriodAnnualDataResult = new List<StressPeriod>
        {
            new StressPeriod
            {
                Days = 31,
                NumberOfTimeSteps = 1
            },
            new StressPeriod
            {
                Days = 31,
                NumberOfTimeSteps = 1
            }
        };

        private readonly Model _model = new Model
        {
            StartDateTime = new DateTime(2011, 1, 1),
            NumberOfStressPeriods = 2,
            AllowablePercentDiscrepancy = 1.0
        };

        private readonly Model _modelAnnual = new Model
        {
            StartDateTime = new DateTime(2011, 1, 1),
            NumberOfStressPeriods = 2,
            AllowablePercentDiscrepancy = 1.0,
            ModelStressPeriodCustomStartDates = new List<ModelStressPeriodCustomStartDate>
            {
                new ModelStressPeriodCustomStartDate()
                {
                    StressPeriod =1 ,
                    StressPeriodStartDate = new DateTime(2011, 1,  1)
                },
                new ModelStressPeriodCustomStartDate()
                {
                    StressPeriod = 2,
                    StressPeriodStartDate = new DateTime(2012, 1,  1)
                }
            }
        };

        [TestMethod]
        public void CalculateImpactToBaseflow_BasicFlow()
        {
            _modflowFileAccessorMock.Arrange(a => a.GetAllZones())
                .Returns(new List<string> { "A" });
            _modflowFileAccessorMock.Arrange(a => a.GetFriendlyInputZoneName(Arg.AnyString))
                .Returns("");
            _modflowFileAccessorMock.Arrange(a => a.GetNumberOfSegmentReaches())
                .Returns(1);
            _modflowFileAccessorMock.Arrange(a => a.GetBaselineData())
                .Returns(new List<OutputData>
                {
                    new OutputData{ SegmentNumber = 1, ReachNumber = 1, FlowToAquifer = 123.45},
                    new OutputData{ SegmentNumber = 1, ReachNumber = 2, FlowToAquifer = 234.56}
                });
            _modflowFileAccessorMock.Arrange(a => a.GetOutputData())
                .Returns(new List<OutputData>
                {
                    new OutputData{ SegmentNumber = 1, ReachNumber = 1, FlowToAquifer = 111.11},
                    new OutputData{ SegmentNumber = 1, ReachNumber = 2, FlowToAquifer = 333.33}
                });
            _modflowFileAccessorMock.Arrange(a => a.GetSegmentReachZones(Arg.AnyInt, Arg.AnyInt))
                .Returns(new List<string> { "A" });

            _modflowFileAccessorMock.Arrange(a => a.GetObservedImpactToBaseflow(true)).Returns((IEnumerable<ObservedImpactToBaseflow>)null);

            var sut = CreateImpactToBaseflowFileOutputSubEngine();

            var result = sut.CalculateImpactToBaseflow(_modflowFileAccessorMock, _getStressPeriodDataResult, VolumeUnit.AcreFeet.VolumeUnitID, true);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            TestImpactToBaseflowResult(result[0], new List<ExpectedResultData>
            {
                new ExpectedResultData {Month = 1, Year = 2011, Value = 0.008781971},
                new ExpectedResultData {Month = 2, Year = 2011, Value = -0.063488961},
            }, "Impacts to Baseflow", 1, 2);
        }

        [TestMethod]
        public void CalculateImpactToBaseflow_BasicFlowAnnual()
        {
            _modflowFileAccessorMock.Arrange(a => a.GetAllZones())
                .Returns(new List<string> { "A" });
            _modflowFileAccessorMock.Arrange(a => a.GetFriendlyInputZoneName(Arg.AnyString))
                .Returns("");
            _modflowFileAccessorMock.Arrange(a => a.GetNumberOfSegmentReaches())
                .Returns(1);
            _modflowFileAccessorMock.Arrange(a => a.GetBaselineData())
                .Returns(new List<OutputData>
                {
                    new OutputData{ SegmentNumber = 1, ReachNumber = 1, FlowToAquifer = 123.45},
                    new OutputData{ SegmentNumber = 1, ReachNumber = 2, FlowToAquifer = 234.56}
                });
            _modflowFileAccessorMock.Arrange(a => a.GetOutputData())
                .Returns(new List<OutputData>
                {
                    new OutputData{ SegmentNumber = 1, ReachNumber = 1, FlowToAquifer = 111.11},
                    new OutputData{ SegmentNumber = 1, ReachNumber = 2, FlowToAquifer = 333.33}
                });
            _modflowFileAccessorMock.Arrange(a => a.GetSegmentReachZones(Arg.AnyInt, Arg.AnyInt))
                .Returns(new List<string> { "A" });

            _modflowFileAccessorMock.Arrange(a => a.GetObservedImpactToBaseflow(true)).Returns((IEnumerable<ObservedImpactToBaseflow>)null);

            var sut = CreateImpactToBaseflowFileOutputSubEngineAnnual();

            var result = sut.CalculateImpactToBaseflow(_modflowFileAccessorMock, _getStressPeriodAnnualDataResult, VolumeUnit.AcreFeet.VolumeUnitID, true);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            TestImpactToBaseflowResult(result[0], new List<ExpectedResultData>
            {
                new ExpectedResultData {Month = 1, Year = 2011, Value = 0.008781971},
                new ExpectedResultData {Month = 1, Year = 2012, Value = -0.070290863},
            }, "Impacts to Baseflow", 1, 2);
        }

        [TestMethod]
        public void CalculateImpactToBaseflow_NoStressPeriodData()
        {
            _modflowFileAccessorMock.Arrange(a => a.GetAllZones())
                .Returns(new List<string> { "A" });
            _modflowFileAccessorMock.Arrange(a => a.GetFriendlyInputZoneName(Arg.AnyString))
                .Returns("");
            _modflowFileAccessorMock.Arrange(a => a.GetNumberOfSegmentReaches())
                .Returns(1);
            _modflowFileAccessorMock.Arrange(a => a.GetBaselineData())
                .Returns(new List<OutputData>
                {
                    new OutputData{ SegmentNumber = 1, ReachNumber = 1, FlowToAquifer = 123.45},
                    new OutputData{ SegmentNumber = 1, ReachNumber = 2, FlowToAquifer = 234.56}
                });
            _modflowFileAccessorMock.Arrange(a => a.GetOutputData())
                .Returns(new List<OutputData>
                {
                    new OutputData{ SegmentNumber = 1, ReachNumber = 1, FlowToAquifer = 111.11},
                    new OutputData{ SegmentNumber = 1, ReachNumber = 2, FlowToAquifer = 333.33}
                });
            _modflowFileAccessorMock.Arrange(a => a.GetSegmentReachZones(Arg.AnyInt, Arg.AnyInt))
                .Returns(new List<string> { "A" });
            _modflowFileAccessorMock.Arrange(a => a.GetObservedImpactToBaseflow(true)).Returns((IEnumerable<ObservedImpactToBaseflow>)null);

            var sut = CreateImpactToBaseflowFileOutputSubEngine();

            var result = sut.CalculateImpactToBaseflow(_modflowFileAccessorMock, new List<StressPeriod>(), VolumeUnit.AcreFeet.VolumeUnitID, true);

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void CalculateImpactToBaseflow_NoBaselineData()
        {
            _modflowFileAccessorMock.Arrange(a => a.GetAllZones())
                .Returns(new List<string>());
            _modflowFileAccessorMock.Arrange(a => a.GetNumberOfSegmentReaches())
                .Returns(1);
            _modflowFileAccessorMock.Arrange(a => a.GetBaselineData())
                .Returns((List<OutputData>)null);
            _modflowFileAccessorMock.Arrange(a => a.GetOutputData())
                .Returns(new List<OutputData>
                {
                    new OutputData{ SegmentNumber = 1, ReachNumber = 1, FlowToAquifer = 111.11},
                    new OutputData{ SegmentNumber = 1, ReachNumber = 2, FlowToAquifer = 333.33}
                });
            _modflowFileAccessorMock.Arrange(a => a.GetObservedImpactToBaseflow(true)).Returns((IEnumerable<ObservedImpactToBaseflow>)null);

            var sut = CreateImpactToBaseflowFileOutputSubEngine();

            var result = sut.CalculateImpactToBaseflow(_modflowFileAccessorMock, _getStressPeriodDataResult, VolumeUnit.AcreFeet.VolumeUnitID, true);

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void CalculateImpactToBaseflow_NoRunData()
        {
            _modflowFileAccessorMock.Arrange(a => a.GetAllZones())
                .Returns(new List<string>());
            _modflowFileAccessorMock.Arrange(a => a.GetNumberOfSegmentReaches())
                .Returns(1);
            _modflowFileAccessorMock.Arrange(a => a.GetBaselineData())
                .Returns(new List<OutputData>
                {
                    new OutputData{ SegmentNumber = 1, ReachNumber = 1, FlowToAquifer = 111.11},
                    new OutputData{ SegmentNumber = 1, ReachNumber = 2, FlowToAquifer = 333.33}
                });
            _modflowFileAccessorMock.Arrange(a => a.GetOutputData())
                .Returns((List<OutputData>)null);
            _modflowFileAccessorMock.Arrange(a => a.GetSegmentReachZones(Arg.AnyInt, Arg.AnyInt))
                .Returns(new List<string>());
            _modflowFileAccessorMock.Arrange(a => a.GetObservedImpactToBaseflow(true)).Returns((IEnumerable<ObservedImpactToBaseflow>)null);

            var sut = CreateImpactToBaseflowFileOutputSubEngine();

            var result = sut.CalculateImpactToBaseflow(_modflowFileAccessorMock, _getStressPeriodDataResult, VolumeUnit.AcreFeet.VolumeUnitID, true);

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void CalculateImpactToBaseflow_NoZones()
        {
            _modflowFileAccessorMock.Arrange(a => a.GetAllZones())
                .Returns(new List<string>());
            _modflowFileAccessorMock.Arrange(a => a.GetNumberOfSegmentReaches())
                .Returns(1);
            _modflowFileAccessorMock.Arrange(a => a.GetBaselineData())
                .Returns(new List<OutputData>
                {
                    new OutputData{ SegmentNumber = 1, ReachNumber = 1, FlowToAquifer = 123.45},
                    new OutputData{ SegmentNumber = 1, ReachNumber = 2, FlowToAquifer = 234.56}
                });
            _modflowFileAccessorMock.Arrange(a => a.GetOutputData())
                .Returns(new List<OutputData>
                {
                    new OutputData{ SegmentNumber = 1, ReachNumber = 1, FlowToAquifer = 111.11},
                    new OutputData{ SegmentNumber = 1, ReachNumber = 2, FlowToAquifer = 333.33}
                });
            _modflowFileAccessorMock.Arrange(a => a.GetSegmentReachZones(Arg.AnyInt, Arg.AnyInt))
                .Returns(new List<string>());
            _modflowFileAccessorMock.Arrange(a => a.GetObservedImpactToBaseflow(true)).Returns((IEnumerable<ObservedImpactToBaseflow>)null);

            var sut = CreateImpactToBaseflowFileOutputSubEngine();

            var result = sut.CalculateImpactToBaseflow(_modflowFileAccessorMock, _getStressPeriodDataResult, VolumeUnit.AcreFeet.VolumeUnitID, true);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            TestImpactToBaseflowResult(result[0], new List<ExpectedResultData>
            {
                new ExpectedResultData {Month = 1, Year = 2011, Value = 0.008781971},
                new ExpectedResultData {Month = 2, Year = 2011, Value = -0.063488961},
            }, "Impacts to Baseflow", 1, 1);
        }

        [TestMethod]
        public void CalculateImpactToBaseflow_MultipleSegmentReaches()
        {
            _modflowFileAccessorMock.Arrange(a => a.GetAllZones())
                .Returns(new List<string>());
            _modflowFileAccessorMock.Arrange(a => a.GetNumberOfSegmentReaches())
                .Returns(3);
            _modflowFileAccessorMock.Arrange(a => a.GetBaselineData())
                .Returns(new List<OutputData>
                {
                    new OutputData{ SegmentNumber = 1, ReachNumber = 1, FlowToAquifer = 123.45},
                    new OutputData{ SegmentNumber = 1, ReachNumber = 2, FlowToAquifer = 234.56},
                    new OutputData{ SegmentNumber = 1, ReachNumber = 3, FlowToAquifer = 345.67},
                    new OutputData{ SegmentNumber = 1, ReachNumber = 1, FlowToAquifer = 456.78},
                    new OutputData{ SegmentNumber = 1, ReachNumber = 2, FlowToAquifer = 567.89},
                    new OutputData{ SegmentNumber = 1, ReachNumber = 3, FlowToAquifer = 678.90}
                });
            _modflowFileAccessorMock.Arrange(a => a.GetOutputData())
                .Returns(new List<OutputData>
                {
                    new OutputData{ SegmentNumber = 1, ReachNumber = 1, FlowToAquifer = 121.21},
                    new OutputData{ SegmentNumber = 1, ReachNumber = 2, FlowToAquifer = 222.22},
                    new OutputData{ SegmentNumber = 1, ReachNumber = 3, FlowToAquifer = 333.44},
                    new OutputData{ SegmentNumber = 1, ReachNumber = 1, FlowToAquifer = 212.12},
                    new OutputData{ SegmentNumber = 1, ReachNumber = 2, FlowToAquifer = 444.22},
                    new OutputData{ SegmentNumber = 1, ReachNumber = 3, FlowToAquifer = 111.11}
                });
            _modflowFileAccessorMock.Arrange(a => a.GetSegmentReachZones(Arg.AnyInt, Arg.AnyInt))
                .Returns(new List<string>());
            _modflowFileAccessorMock.Arrange(a => a.GetObservedImpactToBaseflow(true)).Returns((IEnumerable<ObservedImpactToBaseflow>)null);

            var sut = CreateImpactToBaseflowFileOutputSubEngine();

            var result = sut.CalculateImpactToBaseflow(_modflowFileAccessorMock, _getStressPeriodDataResult, VolumeUnit.AcreFeet.VolumeUnitID, true);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            TestImpactToBaseflowResult(result[0], new List<ExpectedResultData>
            {
                new ExpectedResultData {Month = 1, Year = 2011, Value = 0.019079792},
                new ExpectedResultData {Month = 2, Year = 2011, Value = 0.601734192},
            }, "Impacts to Baseflow", 1, 1);
        }

        [TestMethod]
        public void CalculateImpactToBaseflow_NotEnoughOutputData()
        {
            _modflowFileAccessorMock.Arrange(a => a.GetAllZones())
                .Returns(new List<string> { "A" });
            _modflowFileAccessorMock.Arrange(a => a.GetFriendlyInputZoneName(Arg.AnyString))
                .Returns("");
            _modflowFileAccessorMock.Arrange(a => a.GetNumberOfSegmentReaches())
                .Returns(1);
            _modflowFileAccessorMock.Arrange(a => a.GetBaselineData())
                .Returns(new List<OutputData>
                {
                    new OutputData{ SegmentNumber = 1, ReachNumber = 1, FlowToAquifer = 123.45},
                    new OutputData{ SegmentNumber = 1, ReachNumber = 2, FlowToAquifer = 234.56}
                });
            _modflowFileAccessorMock.Arrange(a => a.GetOutputData())
                .Returns(new List<OutputData>
                {
                    new OutputData{ SegmentNumber = 1, ReachNumber = 1, FlowToAquifer = 111.11}
                });
            _modflowFileAccessorMock.Arrange(a => a.GetSegmentReachZones(Arg.AnyInt, Arg.AnyInt))
                .Returns(new List<string> { "A" });

            _modflowFileAccessorMock.Arrange(a => a.GetObservedImpactToBaseflow(true)).Returns((IEnumerable<ObservedImpactToBaseflow>)null);

            var sut = CreateImpactToBaseflowFileOutputSubEngine();

            Assert.ThrowsException<OutputDataInvalidException>(() => sut.CalculateImpactToBaseflow(_modflowFileAccessorMock, _getStressPeriodDataResult, VolumeUnit.AcreFeet.VolumeUnitID, true));
        }

        [TestMethod]
        public void CalculateImpactToBaseflow_NotEnoughBaselineData()
        {
            _modflowFileAccessorMock.Arrange(a => a.GetAllZones())
                .Returns(new List<string> { "A" });
            _modflowFileAccessorMock.Arrange(a => a.GetFriendlyInputZoneName(Arg.AnyString))
                .Returns("");
            _modflowFileAccessorMock.Arrange(a => a.GetNumberOfSegmentReaches())
                .Returns(1);
            _modflowFileAccessorMock.Arrange(a => a.GetBaselineData())
                .Returns(new List<OutputData>
                {
                    new OutputData{ SegmentNumber = 1, ReachNumber = 1, FlowToAquifer = 123.45}
                });
            _modflowFileAccessorMock.Arrange(a => a.GetOutputData())
                .Returns(new List<OutputData>
                {
                    new OutputData{ SegmentNumber = 1, ReachNumber = 1, FlowToAquifer = 111.11},
                    new OutputData{ SegmentNumber = 1, ReachNumber = 2, FlowToAquifer = 333.33}
                });
            _modflowFileAccessorMock.Arrange(a => a.GetSegmentReachZones(Arg.AnyInt, Arg.AnyInt))
                .Returns(new List<string> { "A" });

            _modflowFileAccessorMock.Arrange(a => a.GetObservedImpactToBaseflow(true)).Returns((IEnumerable<ObservedImpactToBaseflow>)null);

            var sut = CreateImpactToBaseflowFileOutputSubEngine();
            Assert.ThrowsException<OutputDataInvalidException>(() => sut.CalculateImpactToBaseflow(_modflowFileAccessorMock, _getStressPeriodDataResult, VolumeUnit.AcreFeet.VolumeUnitID, true));
        }

        [TestMethod]
        public void CalculateImpactToBaseflow_TooMuchBaselineData()
        {
            _modflowFileAccessorMock.Arrange(a => a.GetAllZones())
                .Returns(new List<string> { "A" });
            _modflowFileAccessorMock.Arrange(a => a.GetFriendlyInputZoneName(Arg.AnyString))
                .Returns("");
            _modflowFileAccessorMock.Arrange(a => a.GetNumberOfSegmentReaches())
                .Returns(1);
            _modflowFileAccessorMock.Arrange(a => a.GetBaselineData())
                .Returns(new List<OutputData>
                {
                    new OutputData{ SegmentNumber = 1, ReachNumber = 1, FlowToAquifer = 123.45},
                    new OutputData{ SegmentNumber = 1, ReachNumber = 1, FlowToAquifer = 123.45},
                    new OutputData{ SegmentNumber = 1, ReachNumber = 1, FlowToAquifer = 123.45}
                });
            _modflowFileAccessorMock.Arrange(a => a.GetOutputData())
                .Returns(new List<OutputData>
                {
                    new OutputData{ SegmentNumber = 1, ReachNumber = 1, FlowToAquifer = 111.11},
                    new OutputData{ SegmentNumber = 1, ReachNumber = 2, FlowToAquifer = 333.33}
                });
            _modflowFileAccessorMock.Arrange(a => a.GetSegmentReachZones(Arg.AnyInt, Arg.AnyInt))
                .Returns(new List<string> { "A" });

            _modflowFileAccessorMock.Arrange(a => a.GetObservedImpactToBaseflow(true)).Returns((IEnumerable<ObservedImpactToBaseflow>)null);

            var sut = CreateImpactToBaseflowFileOutputSubEngine();
            Assert.ThrowsException<OutputDataInvalidException>(() => sut.CalculateImpactToBaseflow(_modflowFileAccessorMock, _getStressPeriodDataResult, VolumeUnit.AcreFeet.VolumeUnitID, true));
        }

        [TestMethod]
        public void CalculateImpactToBaseflow_TooMuchOutputData()
        {
            _modflowFileAccessorMock.Arrange(a => a.GetAllZones())
                .Returns(new List<string> { "A" });
            _modflowFileAccessorMock.Arrange(a => a.GetFriendlyInputZoneName(Arg.AnyString))
                .Returns("");
            _modflowFileAccessorMock.Arrange(a => a.GetNumberOfSegmentReaches())
                .Returns(1);
            _modflowFileAccessorMock.Arrange(a => a.GetBaselineData())
                .Returns(new List<OutputData>
                {
                    new OutputData{ SegmentNumber = 1, ReachNumber = 1, FlowToAquifer = 123.45},
                    new OutputData{ SegmentNumber = 1, ReachNumber = 1, FlowToAquifer = 123.45}
                });
            _modflowFileAccessorMock.Arrange(a => a.GetOutputData())
                .Returns(new List<OutputData>
                {
                    new OutputData{ SegmentNumber = 1, ReachNumber = 1, FlowToAquifer = 111.11},
                    new OutputData{ SegmentNumber = 1, ReachNumber = 2, FlowToAquifer = 333.33},
                    new OutputData{ SegmentNumber = 1, ReachNumber = 2, FlowToAquifer = 333.33}
                });
            _modflowFileAccessorMock.Arrange(a => a.GetSegmentReachZones(Arg.AnyInt, Arg.AnyInt))
                .Returns(new List<string> { "A" });

            _modflowFileAccessorMock.Arrange(a => a.GetObservedImpactToBaseflow(true)).Returns((IEnumerable<ObservedImpactToBaseflow>)null);

            var sut = CreateImpactToBaseflowFileOutputSubEngine();
            Assert.ThrowsException<OutputDataInvalidException>(() => sut.CalculateImpactToBaseflow(_modflowFileAccessorMock, _getStressPeriodDataResult, VolumeUnit.AcreFeet.VolumeUnitID, true));
        }

        [TestMethod]
        public void CalculateImpactToBaseflow_MultipleZones()
        {
            _modflowFileAccessorMock.Arrange(a => a.GetAllZones())
                .Returns(new List<string> { "A", "B", "C" });
            _modflowFileAccessorMock.Arrange(a => a.GetFriendlyInputZoneName(Arg.AnyString))
                .Returns("");
            _modflowFileAccessorMock.Arrange(a => a.GetNumberOfSegmentReaches())
                .Returns(1);
            _modflowFileAccessorMock.Arrange(a => a.GetBaselineData())
                .Returns(new List<OutputData>
                {
                    new OutputData{ SegmentNumber = 1, ReachNumber = 1, FlowToAquifer = 123.45},
                    new OutputData{ SegmentNumber = 1, ReachNumber = 2, FlowToAquifer = 234.56}
                });
            _modflowFileAccessorMock.Arrange(a => a.GetOutputData())
                .Returns(new List<OutputData>
                {
                    new OutputData{ SegmentNumber = 1, ReachNumber = 1, FlowToAquifer = 111.11},
                    new OutputData{ SegmentNumber = 1, ReachNumber = 2, FlowToAquifer = 333.33}
                });
            _modflowFileAccessorMock.Arrange(a => a.GetSegmentReachZones(1, 1))
                .Returns(new List<string> { "A", "C" });
            _modflowFileAccessorMock.Arrange(a => a.GetSegmentReachZones(1, 2))
                .Returns(new List<string> { "B", "C" });

            _modflowFileAccessorMock.Arrange(a => a.GetObservedImpactToBaseflow(true)).Returns((IEnumerable<ObservedImpactToBaseflow>)null);

            var sut = CreateImpactToBaseflowFileOutputSubEngine();

            var result = sut.CalculateImpactToBaseflow(_modflowFileAccessorMock, _getStressPeriodDataResult, VolumeUnit.AcreFeet.VolumeUnitID, true);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            TestImpactToBaseflowResult(result[0], new List<ExpectedResultData>
            {
                new ExpectedResultData {Month = 1, Year = 2011, Value = 0.008781971},
                new ExpectedResultData {Month = 2, Year = 2011, Value = -0.063488961},
            }, "Impacts to Baseflow", 1, 4);
        }

        [TestMethod]
        public void CalculateImpactToBaseflow_MultipleZones_FriendlyNames()
        {
            _modflowFileAccessorMock.Arrange(a => a.GetAllZones())
                .Returns(new List<string> { "A", "B", "C" });
            _modflowFileAccessorMock.Arrange(a => a.GetFriendlyInputZoneName("A"))
                .Returns("Fake Friendly AAA");
            _modflowFileAccessorMock.Arrange(a => a.GetFriendlyInputZoneName("B"))
                .Returns("Fake Friendly BBB");
            _modflowFileAccessorMock.Arrange(a => a.GetFriendlyInputZoneName("C"))
                .Returns("Fake Friendly CCC");
            _modflowFileAccessorMock.Arrange(a => a.GetNumberOfSegmentReaches())
                .Returns(1);
            _modflowFileAccessorMock.Arrange(a => a.GetBaselineData())
                .Returns(new List<OutputData>
                {
                    new OutputData{ SegmentNumber = 1, ReachNumber = 1, FlowToAquifer = 123.45},
                    new OutputData{ SegmentNumber = 1, ReachNumber = 2, FlowToAquifer = 234.56}
                });
            _modflowFileAccessorMock.Arrange(a => a.GetOutputData())
                .Returns(new List<OutputData>
                {
                    new OutputData{ SegmentNumber = 1, ReachNumber = 1, FlowToAquifer = 111.11},
                    new OutputData{ SegmentNumber = 1, ReachNumber = 2, FlowToAquifer = 333.33}
                });
            _modflowFileAccessorMock.Arrange(a => a.GetSegmentReachZones(1, 1))
                .Returns(new List<string> { "A", "C" });
            _modflowFileAccessorMock.Arrange(a => a.GetSegmentReachZones(1, 2))
                .Returns(new List<string> { "B", "C" });

            _modflowFileAccessorMock.Arrange(a => a.GetObservedImpactToBaseflow(true)).Returns((IEnumerable<ObservedImpactToBaseflow>)null);

            var sut = CreateImpactToBaseflowFileOutputSubEngine();

            var result = sut.CalculateImpactToBaseflow(_modflowFileAccessorMock, _getStressPeriodDataResult, VolumeUnit.AcreFeet.VolumeUnitID, true);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            TestImpactToBaseflowResult(result[0], new List<ExpectedResultData>
            {
                new ExpectedResultData {Month = 1, Year = 2011, Value = 0.008781971},
                new ExpectedResultData {Month = 2, Year = 2011, Value = -0.063488961},
            }, "Impacts to Baseflow", 1, 4);
        }

        [TestMethod]
        public void CalculateImpactToBaseflow_NonDifferential()
        {
            _modflowFileAccessorMock.Arrange(a => a.GetAllZones())
                .Returns(new List<string> { "A" });
            _modflowFileAccessorMock.Arrange(a => a.GetFriendlyInputZoneName(Arg.AnyString))
                .Returns("");
            _modflowFileAccessorMock.Arrange(a => a.GetNumberOfSegmentReaches())
                .Returns(1);
            _modflowFileAccessorMock.Arrange(a => a.GetOutputData())
                .Returns(new List<OutputData>
                {
                    new OutputData{ SegmentNumber = 1, ReachNumber = 1, FlowToAquifer = 111.11},
                    new OutputData{ SegmentNumber = 1, ReachNumber = 2, FlowToAquifer = 333.33}
                });
            _modflowFileAccessorMock.Arrange(a => a.GetSegmentReachZones(Arg.AnyInt, Arg.AnyInt))
                .Returns(new List<string> { "A" });
            _modflowFileAccessorMock.Arrange(a => a.GetObservedImpactToBaseflow(false)).Returns((IEnumerable<ObservedImpactToBaseflow>)null);

            var sut = CreateImpactToBaseflowFileOutputSubEngine();

            var result = sut.CalculateImpactToBaseflow(_modflowFileAccessorMock, _getStressPeriodDataResult, VolumeUnit.AcreFeet.VolumeUnitID, false);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            TestImpactToBaseflowResult(result[0], new List<ExpectedResultData>
            {
                new ExpectedResultData {Month = 1, Year = 2011, Value = 0.079072773},
                new ExpectedResultData {Month = 2, Year = 2011, Value = 0.214261708},
            }, "Baseflow", 1, 2);
        }

        [TestMethod]
        public void CalculateImpactToBaseflow_NonDifferential_NoRunData()
        {
            _modflowFileAccessorMock.Arrange(a => a.GetAllZones())
                .Returns(new List<string> { "A" });
            _modflowFileAccessorMock.Arrange(a => a.GetFriendlyInputZoneName(Arg.AnyString))
                .Returns("");
            _modflowFileAccessorMock.Arrange(a => a.GetNumberOfSegmentReaches())
                .Returns(1);
            _modflowFileAccessorMock.Arrange(a => a.GetOutputData())
                .Returns((List<OutputData>)null);
            _modflowFileAccessorMock.Arrange(a => a.GetSegmentReachZones(Arg.AnyInt, Arg.AnyInt))
                .Returns(new List<string> { "A" });
            _modflowFileAccessorMock.Arrange(a => a.GetObservedImpactToBaseflow(true)).Returns((IEnumerable<ObservedImpactToBaseflow>)null);

            var sut = CreateImpactToBaseflowFileOutputSubEngine();

            var result = sut.CalculateImpactToBaseflow(_modflowFileAccessorMock, _getStressPeriodDataResult, VolumeUnit.AcreFeet.VolumeUnitID, false);

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void CalculateImpactToBaseflow_ObservedData()
        {
            _modflowFileAccessorMock.Arrange(a => a.GetAllZones())
                .Returns(new List<string> { "A" });
            _modflowFileAccessorMock.Arrange(a => a.GetFriendlyInputZoneName(Arg.AnyString))
                .Returns("");
            _modflowFileAccessorMock.Arrange(a => a.GetNumberOfSegmentReaches())
                .Returns(1);
            _modflowFileAccessorMock.Arrange(a => a.GetBaselineData())
                .Returns(new List<OutputData>
                {
                    new OutputData{ SegmentNumber = 1, ReachNumber = 1, FlowToAquifer = 123.45},
                    new OutputData{ SegmentNumber = 1, ReachNumber = 2, FlowToAquifer = 234.56}
                });
            _modflowFileAccessorMock.Arrange(a => a.GetOutputData())
                .Returns(new List<OutputData>
                {
                    new OutputData{ SegmentNumber = 1, ReachNumber = 1, FlowToAquifer = 111.11},
                    new OutputData{ SegmentNumber = 1, ReachNumber = 2, FlowToAquifer = 333.33}
                });
            _modflowFileAccessorMock.Arrange(a => a.GetSegmentReachZones(Arg.AnyInt, Arg.AnyInt))
                .Returns(new List<string> { "A" });
            _modflowFileAccessorMock.Arrange(a => a.GetObservedImpactToBaseflow(true)).Returns(new List<ObservedImpactToBaseflow>
            {
                new ObservedImpactToBaseflow { DataSeriesName = "Observed", Period = 1, FlowToAquiferInAcreFeet = 0.00863}
            });

            var sut = CreateImpactToBaseflowFileOutputSubEngine();

            var result = sut.CalculateImpactToBaseflow(_modflowFileAccessorMock, _getStressPeriodDataResult, VolumeUnit.AcreFeet.VolumeUnitID, true);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            TestImpactToBaseflowResult(result[0], new List<ExpectedResultData>
            {
                new ExpectedResultData {Month = 1, Year = 2011, Value = 0.008781971},
                new ExpectedResultData {Month = 2, Year = 2011, Value = -0.063488961},
            }, "Impacts to Baseflow", 1, 3);

            result[0].ResultSets[0].DataSeries[2].Name.Should().Be("Observed");
            result[0].ResultSets[0].DataSeries[2].DataPoints.Count.Should().Be(1);
            result[0].ResultSets[0].DataSeries[2].DataPoints.First().Date.Should().Be(new DateTime(2011, 1, 1));
            result[0].ResultSets[0].DataSeries[2].DataPoints.First().Value.Should().Be(0.00863);
        }

        [TestMethod]
        public void CalculateImpactToBaseflow_TooMuchObservedData()
        {
            _modflowFileAccessorMock.Arrange(a => a.GetAllZones())
               .Returns(new List<string> { "A" });
            _modflowFileAccessorMock.Arrange(a => a.GetFriendlyInputZoneName(Arg.AnyString))
                .Returns("");
            _modflowFileAccessorMock.Arrange(a => a.GetNumberOfSegmentReaches())
                .Returns(1);
            _modflowFileAccessorMock.Arrange(a => a.GetBaselineData())
                .Returns(new List<OutputData>
                {
                    new OutputData{ SegmentNumber = 1, ReachNumber = 1, FlowToAquifer = 123.45},
                    new OutputData{ SegmentNumber = 1, ReachNumber = 2, FlowToAquifer = 234.56}
                });
            _modflowFileAccessorMock.Arrange(a => a.GetOutputData())
                .Returns(new List<OutputData>
                {
                    new OutputData{ SegmentNumber = 1, ReachNumber = 1, FlowToAquifer = 111.11},
                    new OutputData{ SegmentNumber = 1, ReachNumber = 2, FlowToAquifer = 333.33}
                });
            _modflowFileAccessorMock.Arrange(a => a.GetSegmentReachZones(Arg.AnyInt, Arg.AnyInt))
                .Returns(new List<string> { "A" });
            _modflowFileAccessorMock.Arrange(a => a.GetObservedImpactToBaseflow(true)).Returns(new List<ObservedImpactToBaseflow>
            {
                new ObservedImpactToBaseflow { DataSeriesName = "Observed", Period = 1, FlowToAquiferInAcreFeet = 0.00863},
                new ObservedImpactToBaseflow { DataSeriesName = "Observed", Period = 2, FlowToAquiferInAcreFeet = 0.00863},
                new ObservedImpactToBaseflow { DataSeriesName = "Observed", Period = 3, FlowToAquiferInAcreFeet = 0.00863}
            });

            var sut = CreateImpactToBaseflowFileOutputSubEngine();
            Action action = () => sut.CalculateImpactToBaseflow(_modflowFileAccessorMock, _getStressPeriodDataResult, VolumeUnit.AcreFeet.VolumeUnitID, true);
            action.Should().Throw<OutputDataInvalidException>();
        }


        [TestMethod]
        public void CalculateImpactToBaseflow_ConvertObservedVolume()
        {
            _modflowFileAccessorMock.Arrange(a => a.GetAllZones())
                .Returns(new List<string> { "A" });
            _modflowFileAccessorMock.Arrange(a => a.GetFriendlyInputZoneName(Arg.AnyString))
                .Returns("");
            _modflowFileAccessorMock.Arrange(a => a.GetNumberOfSegmentReaches())
                .Returns(1);
            _modflowFileAccessorMock.Arrange(a => a.GetBaselineData())
                .Returns(new List<OutputData>
                {
                    new OutputData{ SegmentNumber = 1, ReachNumber = 1, FlowToAquifer = 123.45},
                    new OutputData{ SegmentNumber = 1, ReachNumber = 2, FlowToAquifer = 234.56}
                });
            _modflowFileAccessorMock.Arrange(a => a.GetOutputData())
                .Returns(new List<OutputData>
                {
                    new OutputData{ SegmentNumber = 1, ReachNumber = 1, FlowToAquifer = 111.11},
                    new OutputData{ SegmentNumber = 1, ReachNumber = 2, FlowToAquifer = 333.33}
                });
            _modflowFileAccessorMock.Arrange(a => a.GetSegmentReachZones(Arg.AnyInt, Arg.AnyInt))
                .Returns(new List<string> { "A" });
            _modflowFileAccessorMock.Arrange(a => a.GetObservedImpactToBaseflow(true)).Returns(new List<ObservedImpactToBaseflow>
            {
                new ObservedImpactToBaseflow { DataSeriesName = "Observed", Period = 1, FlowToAquiferInAcreFeet = 0.00863}
            });

            var sut = CreateImpactToBaseflowFileOutputSubEngine();

            var result = sut.CalculateImpactToBaseflow(_modflowFileAccessorMock, _getStressPeriodDataResult, VolumeUnit.CubicMeter.VolumeUnitID, true);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);

            result[0].ResultSets[0].DataSeries[2].Name.Should().Be("Observed");
            result[0].ResultSets[0].DataSeries[2].DataPoints.Count.Should().Be(1);
            result[0].ResultSets[0].DataSeries[2].DataPoints.First().Date.Should().Be(new DateTime(2011, 1, 1));
            result[0].ResultSets[0].DataSeries[2].DataPoints.First().Value.Should().Be(10.644948258);
        }

        [TestMethod]
        public void CalculateImpactToBaseflow_ModFlowSixStructuredModel()
        {
            _modflowFileAccessorMock.Arrange(a => a.GetAllZones())
                .Returns(new List<string> { "A" });
            _modflowFileAccessorMock.Arrange(a => a.GetFriendlyInputZoneName(Arg.AnyString))
                .Returns("");
            _modflowFileAccessorMock.Arrange(a => a.GetNumberOfSegmentReaches())
                .Returns(1);
            _modflowFileAccessorMock.Arrange(a => a.GetBaselineData())
                .Returns(new List<OutputData>
                {
                    new OutputData{ SegmentNumber = 1, ReachNumber = 1, FlowToAquifer = 123.45},
                    new OutputData{ SegmentNumber = 1, ReachNumber = 2, FlowToAquifer = 234.56}
                });
            _modflowFileAccessorMock.Arrange(a => a.GetOutputData())
                .Returns(new List<OutputData>
                {
                    new OutputData{ SegmentNumber = 1, ReachNumber = 1, FlowToAquifer = 111.11},
                    new OutputData{ SegmentNumber = 1, ReachNumber = 2, FlowToAquifer = 333.33}
                });
            _modflowFileAccessorMock.Arrange(a => a.GetSegmentReachZones(Arg.AnyInt, Arg.AnyInt))
                .Returns(new List<string> { "A" });

            _modflowFileAccessorMock.Arrange(a => a.GetObservedImpactToBaseflow(Arg.AnyBool))
                .Returns((List<ObservedImpactToBaseflow>)null);

            var sut = CreateImpactToBaseflowFileOutputSubEngine();

            var result = sut.CalculateImpactToBaseflow(_modflowFileAccessorMock, _getStressPeriodDataResult, VolumeUnit.AcreFeet.VolumeUnitID, true);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);

            result[0].ResultSets[0].DataSeries[0].Name.Should().Be("Total");
            result[0].ResultSets[0].DataSeries[0].DataPoints.Count.Should().Be(2);
            result[0].ResultSets[0].DataSeries[0].DataPoints.First().Date.Should().Be(new DateTime(2011, 1, 1));
            result[0].ResultSets[0].DataSeries[0].DataPoints.First().Value.Should().Be(0.00878191);
        }

        private class ExpectedResultData
        {
            public int Year { get; set; }
            public int Month { get; set; }
            public double Value { get; set; }
        }

        private void TestImpactToBaseflowResult(RunResultDetails totalResult, List<ExpectedResultData> expectedResultData, string expectedTitle, int expectedRunResultId, int expectedSeries)
        {
            totalResult.Should().NotBeNull();
            totalResult.RunResultName.Should().Be(expectedTitle);
            totalResult.ResultSets.Should().NotBeNull().And.Subject.Count().Should().Be(2);
            totalResult.ResultSets[0].Name.Should().Be("Rate");
            totalResult.ResultSets[0].DataType.Should().Be("Acre-Feet");
            totalResult.ResultSets[0].DisplayType.Should().Be(RunResultDisplayType.LineChart);
            totalResult.ResultSets[0].DataSeries.Should().NotBeNull().And.Subject.Count().Should().Be(expectedSeries);
            totalResult.ResultSets[0].DataSeries[0].DataPoints.Should().NotBeNull().And.Subject.Count().Should().Be(expectedResultData.Count);
            for (var i = 0; i < expectedResultData.Count; i++)
            {
                var date = new DateTime(expectedResultData[i].Year, expectedResultData[i].Month, 1);
                if (date.Year % 2 != 0 || date.Month != 2)
                {
                    totalResult.ResultSets[0].DataSeries[0].DataPoints[i].Date.Should().Be(date);
                    var expectedValue = expectedResultData[i].Value;
                    var actualValue = totalResult.ResultSets[0].DataSeries[0].DataPoints[i].Value;
                    TestUtilities.AssertAreEqualWithCalculatedDelta(expectedValue, actualValue);
                }
            }
        }

        private readonly IModelFileAccessor _modflowFileAccessorMock = Mock.Create<IModelFileAccessor>(Behavior.Strict);

        private ImpactToBaseflowFileOutputSubEngine CreateImpactToBaseflowFileOutputSubEngine()
        {
            return new ImpactToBaseflowFileOutputSubEngine(_model);
        }

        private ImpactToBaseflowFileOutputSubEngine CreateImpactToBaseflowFileOutputSubEngineAnnual()
        {
            return new ImpactToBaseflowFileOutputSubEngine(_modelAnnual);
        }
    }
}
