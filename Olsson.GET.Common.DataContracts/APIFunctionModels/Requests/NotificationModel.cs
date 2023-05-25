using System;

namespace Olsson.GET.Common.DataContracts.APIFunctionModels
{
    public class NotificationModel
    {
        public int? RunId { get; set; }

        public Exception Exception { get; set; }

        public bool IsSystemFailure { get; set; }
    }
}
