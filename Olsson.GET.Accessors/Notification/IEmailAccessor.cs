using System.Collections.Generic;
using System.Threading.Tasks;

namespace Olsson.GET.Accessors.Notification
{
    public interface IEmailAccessor
    {
        Task SendEmail(string[] to, string from, string templateId, Dictionary<string, string> placeholders);
    }
}
