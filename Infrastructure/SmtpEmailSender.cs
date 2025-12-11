using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace Sentara.Api.Infrastructure
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly EmailSettings _settings;

        public SmtpEmailSender(IOptions<EmailSettings> options)
        {
            _settings = options.Value;
        }

        public async Task SendAsync(
            string to,
            string subject,
            string htmlBody,
            IEnumerable<EmailAttachment>? attachments = null)
        {
            using var message = new MailMessage();
            message.From = new MailAddress(_settings.From);
            message.To.Add(to);
            message.Subject = subject;
            message.Body = htmlBody;
            message.IsBodyHtml = true;

            // 🔹 Attach files if provided
            if (attachments != null)
            {
                foreach (var att in attachments)
                {
                    if (!string.IsNullOrWhiteSpace(att.FilePath) && File.Exists(att.FilePath))
                    {
                        var attachment = new Attachment(att.FilePath, att.ContentType);
                        if (!string.IsNullOrWhiteSpace(att.FileName))
                        {
                            attachment.Name = att.FileName;
                        }

                        message.Attachments.Add(attachment);
                    }
                }
            }

            using var client = new SmtpClient(_settings.SmtpHost, _settings.SmtpPort)
            {
                EnableSsl = _settings.EnableSsl,
                Credentials = new NetworkCredential(_settings.User, _settings.Password)
            };

            await client.SendMailAsync(message);
        }
    }
}

