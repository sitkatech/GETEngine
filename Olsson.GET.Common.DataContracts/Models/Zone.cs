namespace Olsson.GET.Common.DataContracts.Models
{
    public class Zone
    {
        public string ZoneNumber { get; set; }

        public string Name { get; set; }

        public Coordinate[] Bounds { get; set; }
    }
}
