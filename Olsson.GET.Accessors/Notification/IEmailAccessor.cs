using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Olsson.GET.Accessors.Notification
{
    public interface IEmailAccessor
    {
        Task SendEmail(string[] to, string from, string templateId, Dictionary<string, string> placeholders);
    }
}
