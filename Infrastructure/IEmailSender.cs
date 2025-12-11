using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sentara.Api.Infrastructure
{
    public interface IEmailSender
    {
        Task SendAsync(
            string to,
            string subject,
            string htmlBody,
            IEnumerable<EmailAttachment>? attachments = null);
    }
}
