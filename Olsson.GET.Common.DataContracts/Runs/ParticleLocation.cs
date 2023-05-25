using Olsson.GET.Common.DataContracts.Models;
using System.Collections.Generic;

namespace Olsson.GET.Common.DataContracts.Runs
{
    public class ParticleLocation
    {
        public int PaticleId { get; set; }

        public List<ParticleTimeLocations> TimeLocations { get; set; }
    }

    public class ParticleTimeLocations
    {
        public double TimeSinceBeginingOfModelInDays { get; set; }

        public Coordinate Coordinate { get; set; }
    }
}
