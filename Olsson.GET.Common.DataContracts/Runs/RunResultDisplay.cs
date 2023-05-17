using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Olsson.GET.Common.DataContracts.Runs
{
    public class RunResultDisplay
    {
        public int RunResultId { get; set; }
        public string Name { get; set; }
        public string FileStorageLocator { get; set; }
    }
}
