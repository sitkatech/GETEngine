using System;

namespace Olsson.GET.Common.DataContracts.APIFunctionModels
{
    public class RunSummaryReponseModel
    {
        public int RunId { get; set; }

        public string RunName { get; set; }

        public DateTime CreatedDate { get; set; }

        public string Status { get; set; }

        public int UserId { get; set; }
    }
}
