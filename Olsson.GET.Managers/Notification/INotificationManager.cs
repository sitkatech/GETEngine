using System.ServiceModel;
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
