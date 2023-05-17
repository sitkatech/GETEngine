using Olsson.GET.Common.Utilities;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Olsson.GET.Accessors.Notification
{
    public class EmailAccessor : IEmailAccessor
    {
        public async Task SendEmail(string[] to, string from, string templateId, Dictionary<string, string> placeholders)
        {
            var client = new SendGridClient(ConfigurationHelper.AppSettings.SendGridApiKey);

            var myMessage = new SendGridMessage();

            myMessage.From = new EmailAddress(from);

            foreach (var recipient in to)
            {
                myMessage.AddTo(recipient);
            }

            myMessage.SetTemplateId(templateId);

            foreach (var value in placeholders)
            {
                myMessage.AddSubstitution(value.Key, value.Value);
            }

            var result = await client.SendEmailAsync(myMessage);

            return;
        }
    }
}
