using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;
using Olsson.GET.Engines.RunDataParse;
using FluentAssertions;
using Olsson.GET.Common.DataContracts.Models;

namespace Olsson.GET.Tests.EngineTests
{
    [TestClass]
    public class RunDataParseEngineTests
    {
        private Model _canalModel = new Model
        {
            CanalData = "Southside,30-mile,Cozad",
            StartDateTime = new DateTime(2011, 1, 1),
            NumberOfStressPeriods = 600
        };

        private RunDataParseEngine createRunDataParseEngine(Model model, bool incorrectLeapYear = false)
        {
            var result = new RunDataParseEngine();
            return result;
        }

        private Model _wellModel = new Model
        {
            StartDateTime = new DateTime(2000, 1, 1),
            NumberOfStressPeriods = 24
        };

        private Model _wellModelAnnual = new Model
        {
            StartDateTime = new DateTime(2000, 1, 1),
            NumberOfStressPeriods = 2,
            ModelStressPeriodCustomStartDates = new List<ModelStressPeriodCustomStartDate>
            {
                new ModelStressPeriodCustomStartDate()
                {
                    StressPeriod = 1,
                    StressPeriodStartDate = new DateTime(2000, 1, 1)
                }, 
                new ModelStressPeriodCustomStartDate()
                {
                    StressPeriodStartDate = new DateTime(2001, 1,  1),
                    StressPeriod = 2
                }
            }
        };

        [TestMethod]
        public void RunDataParseEngine_CanalParseHappyPath()
        {
            var data = File.ReadAllBytes("Files\\CanalData.csv");

            var engine = createRunDataParseEngine(_canalModel);

            var parseResult = engine.ParseCanalRunDataFromFile(data, _canalModel);

            parseResult.Should().NotBeNull();

            parseResult.Success.Should().BeTrue();

            parseResult.RunInputs.Count.Should().Be(600);
        }


        [TestMethod]
        public void RunDataParseEngine_CanalInvalidColumns()
        {
            var data = File.ReadAllBytes("Files\\CanalDataMissingColumn.csv");

            var engine = createRunDataParseEngine(_canalModel);

            var parseResult = engine.ParseCanalRunDataFromFile(data, _canalModel);

            parseResult.Should().NotBeNull();

            parseResult.Success.Should().BeFalse();

            parseResult.Errors.Count.Should().Be(1);
        }

        [TestMethod]
        public void RunDataParseEngine_CanalInvalidDataType()
        {
            var data = File.ReadAllBytes("Files\\CanalDataMissingBadData.csv");

            var engine = createRunDataParseEngine(_canalModel);

            var parseResult = engine.ParseCanalRunDataFromFile(data, _canalModel);

            parseResult.Should().NotBeNull();

            parseResult.Success.Should().BeFalse();

            parseResult.Errors.Count.Should().Be(1);
        }

        [TestMethod]
        public void RunDataParseEngine_DateTooEarly()
        {
            var data = File.ReadAllBytes("Files\\CanalData.csv");

            var engine = createRunDataParseEngine(_canalModel);

            _canalModel.StartDateTime = new DateTime(2011, 2, 1);

            var parseResult = engine.ParseCanalRunDataFromFile(data, _canalModel);

            parseResult.Should().NotBeNull();

            parseResult.Success.Should().BeFalse();

            parseResult.Errors.Count.Should().Be(1);
        }

        [TestMethod]
        public void RunDataParseEngine_DateTooLate()
        {
            var data = File.ReadAllBytes("Files\\CanalDataExtraDate.csv");

            var engine = createRunDataParseEngine(_canalModel);

            var parseResult = engine.ParseCanalRunDataFromFile(data, _canalModel);

            parseResult.Should().NotBeNull();

            parseResult.Success.Should().BeFalse();

            parseResult.Errors.Count.Should().Be(1);
        }

        [TestMethod]
        public void RunDataParseEngine_InvalidColumnName()
        {
            var data = File.ReadAllBytes("Files\\CanalData.csv");

            var engine = createRunDataParseEngine(_canalModel);

            _canalModel.CanalData = "30-mile,Cozad";

            var parseResult = engine.ParseCanalRunDataFromFile(data, _canalModel);

            parseResult.Should().NotBeNull();

            parseResult.Success.Should().BeFalse();

            parseResult.Errors.Count.Should().Be(1);

            parseResult.Errors.First().Should().Contain("Southside");
        }

        [TestMethod]
        public void RunDataParseEngine_NoValues()
        {
            var data = File.ReadAllBytes("Files\\CanalDataNoValues.csv");

            var engine = createRunDataParseEngine(_canalModel);

            _canalModel.CanalData = null;

            var parseResult = engine.ParseCanalRunDataFromFile(data, _canalModel);

            parseResult.Should().NotBeNull();

            parseResult.Success.Should().BeTrue();
        }

        [TestMethod]
        public void RunDataParseEngine_WellParseHappyPath()
        {
            var data = File.ReadAllBytes("Files\\WellData.csv");

            var engine = createRunDataParseEngine(_wellModel);

            var parseResult = engine.ParseWellRunDataFromFile(data, _wellModel);

            parseResult.Should().NotBeNull();

            parseResult.Success.Should().BeTrue();

            parseResult.RunInputs.Count.Should().Be(24);

            parseResult.RunInputs.First().Values[0].Lat = 40.8705;
            parseResult.RunInputs.First().Values[0].Lng = -100.0121;

            parseResult.RunInputs.First().Values[1].Lat = 40.7853;
            parseResult.RunInputs.First().Values[1].Lng = -99.9709;

            parseResult.RunInputs.First().Values[2].Lat = 40.8176;
            parseResult.RunInputs.First().Values[2].Lng = -100.1453;
        }

        [TestMethod]
        public void RunDataParseEngine_WellParseHappyPath_Annual()
        {
            var data = File.ReadAllBytes("Files\\WellDataAnnual.csv");

            var engine = createRunDataParseEngine(_wellModel);

            var parseResult = engine.ParseWellRunDataFromFile(data, _wellModelAnnual);

            parseResult.Should().NotBeNull();

            parseResult.Success.Should().BeTrue();

            parseResult.RunInputs.Count.Should().Be(2);

            parseResult.RunInputs.First().Values[0].Lat = 40.8705;
            parseResult.RunInputs.First().Values[0].Lng = -100.0121;

            parseResult.RunInputs.First().Values[1].Lat = 40.7853;
            parseResult.RunInputs.First().Values[1].Lng = -99.9709;
        }

        [TestMethod]
        public void RunDataParseEngine_WellParseDateTooEarly()
        {
            var data = File.ReadAllBytes("Files\\WellData.csv");

            var engine = createRunDataParseEngine(_wellModel);

            _wellModel.StartDateTime = new DateTime(2000, 2, 1);

            var parseResult = engine.ParseWellRunDataFromFile(data, _wellModel);

            parseResult.Should().NotBeNull();

            parseResult.Success.Should().BeFalse();

            parseResult.Errors.Count.Should().Be(1);
        }

        [TestMethod]
        public void RunDataParseEngine_WellParseDateTooLate()
        {
            var data = File.ReadAllBytes("Files\\WellDataExtraDate.csv");

            var engine = createRunDataParseEngine(_wellModel);

            var parseResult = engine.ParseWellRunDataFromFile(data, _wellModel);

            parseResult.Should().NotBeNull();

            parseResult.Success.Should().BeFalse();

            parseResult.Errors.Count.Should().Be(1);
        }

        [TestMethod]
        public void RunDataParseEngine_WellParseDateTooLateAnnual()
        {
            var data = File.ReadAllBytes("Files\\WellDataAnnualExtraDate.csv");

            var engine = createRunDataParseEngine(_wellModelAnnual);

            var parseResult = engine.ParseWellRunDataFromFile(data, _wellModel);

            parseResult.Should().NotBeNull();

            parseResult.Success.Should().BeFalse();

            parseResult.Errors.Count.Should().Be(1);
        }

        [TestMethod]
        public void RunDataParseEngine_WellBadDate()
        {
            var data = File.ReadAllBytes("Files\\WellDataBadDate.csv");

            var engine = createRunDataParseEngine(_wellModel);

            var parseResult = engine.ParseWellRunDataFromFile(data, _wellModel);

            parseResult.Should().NotBeNull();

            parseResult.Success.Should().BeFalse();

            parseResult.Errors.Count.Should().Be(1);

            parseResult.Errors.First().Should().Contain("xxxxxx");
        }
    }
}
