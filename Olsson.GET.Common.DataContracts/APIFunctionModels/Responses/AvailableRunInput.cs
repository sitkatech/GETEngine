using System.Collections.Generic;

namespace Olsson.GET.Common.DataContracts.APIFunctionModels
{
    public class AvailableRunInput
    {
        public string FileName { get; set; }
        public List<string> AvailableFileTypes { get; set; }
    }
}