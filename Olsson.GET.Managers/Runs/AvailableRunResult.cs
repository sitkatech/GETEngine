using System.Collections.Generic;

namespace Olsson.GET.Managers.Runs
{
    public class AvailableRunResult
    {
        public string FileName { get; set; }
        public List<string> AvailableSubTypes { get; set; }
        public List<string> AvailableFileTypes { get; set; }
    }
}