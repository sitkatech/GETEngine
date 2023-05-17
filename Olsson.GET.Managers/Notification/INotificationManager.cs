using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Olsson.GET.Managers.Notification
{
    [ServiceContract]
    public interface INotificationManager
    {
        [OperationContract]
        Task SendPasswordResetEmail(string toAddress, string resetLink);

        [OperationContract]
        Task SendRunCompletedEmail(int runId, string errorMessage);
    }
}
