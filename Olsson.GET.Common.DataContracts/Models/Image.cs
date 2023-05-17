namespace Olsson.GET.Common.DataContracts.Models
{
    public class Image
    {
        public int ImageID { get; set; }

        public string ImageName { get; set; }

        public string Server { get; set; }

        public bool IsLinux { get; set; }

        public int? CpuCoreCount { get; set; }

        public decimal? Memory { get; set; }
    }
}
