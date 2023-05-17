using System;

namespace Olsson.GET.Common.DataContracts.Files
{
    public class FileModel
    {
        public string Name { get; set; }

        public string Path { get; set; }

        public DateTime ModDate { get; set; }
    }
}
