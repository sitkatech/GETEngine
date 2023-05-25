namespace Olsson.GET.Common.DataContracts.Runs
{
    public class ModpathTimeSeries
    {
        public int TimePointIndex { get; set; }

        public int CumulativeTimeStep { get; set; }

        /// <summary>
        /// Days from begining of model
        /// </summary>
        public double TrackingTime { get; set; }

        public int SequenceNumber { get; set; }

        public int ParticleGroup { get; set; }

        public int ParticleId { get; set; }

        public int CellNumber { get; set; }

        public int Layer { get; set; }

        public double LocalX { get; set; }

        public double LocalY { get; set; }

        public double LocalZ { get; set; }

        public double GlobalX { get; set; }

        public double GlobalY { get; set; }

        public double GlobalZ { get; set; }

    }
}
