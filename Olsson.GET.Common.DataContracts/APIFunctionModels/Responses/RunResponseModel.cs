using Olsson.GET.Common.DataContracts.Runs;

namespace Olsson.GET.Common.DataContracts.APIFunctionModels
{
    public class RunResponseModel
    {
        public int RunId { get; set; }

        public RunStatus RunStatus {
            get;
            set;
        }

        public string Message { get; set; }
    }
}
