using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using Microsoft.SqlServer.Types;
using Newtonsoft.Json;
using Olsson.GET.Common.DataContracts.Models;
using Olsson.GET.Common.DataContracts.Runs;

namespace Olsson.GET.Accessors.FileIO
{
    internal class StructuredModflowFileAccessor : ModelFileAccessor
    {
        public StructuredModflowFileAccessor(Model model) : base(model)
        {

        }

        private sealed class StructuredProportionMapper : LocationProportionMapper
        {
            public StructuredProportionMapper()
                : base(new[] { "layer", "row", "col" })
            {
                //Adding a column here? Add it to the mappedColumns array below
                Map(m => m.Location).ConvertUsing(r =>
                {
                    var row = (CsvHelper.CsvReader)r;
                    return BuildStructuredKey(row.GetField<int>("layer"), row.GetField<int>("row"), row.GetField<int>("col"));
                });
            }
        }

        private static string BuildStructuredKey(int layer, int row, int column)
        {
            return $"{layer}|{row}|{column}";
        }

        protected override string DisFileKey => StructuredDisFileKey;
        protected override Type LocationProportionMapperType => typeof(StructuredProportionMapper);
        protected override int NumberOfStressPeriodsColumnInDisFileIndex => 3;
        protected override int FlowToAquiferColumnInOutputIndex => 6;
        protected override int SegmentNumberColumnInOutputIndex => 3;
        protected override int ReachNumberColumnInOutputIndex => 4;
    }
    internal class StructuredLocationMapPositionRecord
    {
        public int Layer { get; set; }
        public int Row { get; set; }
        public int Col { get; set; }
        public string WellPumpingNodes { get; set; }
    }

    internal class StructuredLocationPumpingProportion
    {
        public int Layer { get; set; }
        public int Row { get; set; }
        public int Col { get; set; }
        public double Proportion { get; set; }
    }

    internal class StructuredLocationZone
    {
        public int Layer { get; set; }
        public int Row { get; set; }
        public int Col { get; set; }
        public string Zone { get; set; }
    }

}
