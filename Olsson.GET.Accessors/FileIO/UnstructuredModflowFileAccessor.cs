using System;
using Olsson.GET.Common.DataContracts.Models;

namespace Olsson.GET.Accessors.FileIO
{
    internal class UnstructuredModflowFileAccessor : ModelFileAccessor
    {
        public UnstructuredModflowFileAccessor(Model model) : base(model)
        {
        
        }

        private sealed class UnstructuredProportionMapper : LocationProportionMapper
        {
            public UnstructuredProportionMapper()
                : base(new[] { "node" })
            {
                Map(m => m.Location).Name("node");
            }
        }

        protected override Type LocationProportionMapperType => typeof(UnstructuredProportionMapper);
        protected override string DisFileKey => UnstructuredDisFileKey;
        protected override int NumberOfStressPeriodsColumnInDisFileIndex => 4;
        protected override int FlowToAquiferColumnInOutputIndex => 4;
        protected override int SegmentNumberColumnInOutputIndex => 1;
        protected override int ReachNumberColumnInOutputIndex => 2;                  
    }

    internal class UnstructuredLocationMapPositionRecord
    {
        public string Node { get; set; }
        public string WellPumpingNodes { get; set; }
    }

    internal class UnstructuredLocationZone
    {
        public string Node { get; set; }
        public string Zone { get; set; }
    }
}
