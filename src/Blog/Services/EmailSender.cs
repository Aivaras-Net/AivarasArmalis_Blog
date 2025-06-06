using System.Net;
using System.Net.Mail;
using Blog.Models;

namespace Blog.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(
            EmailSettings emailSettings,
            ILogger<EmailSender> logger)
        {
            _emailSettings = emailSettings;
            _logger = logger;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                {
                    return;
                }

                _logger.LogInformation(WebConstants.LogSendingEmail, email);

                using (var message = new MailMessage())
                {
                    message.From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName);
                    message.Subject = subject;
                    message.Body = htmlMessage;
                    message.IsBodyHtml = true;
                    message.To.Add(email);

                    using (var client = new SmtpClient(_emailSettings.SmtpServer))
                    {
                        client.Port = _emailSettings.SmtpPort;
                        client.Credentials = new NetworkCredential(_emailSettings.SmtpUsername, _emailSettings.SmtpPassword);
                        client.EnableSsl = _emailSettings.EnableSsl;

                        await client.SendMailAsync(message);
                    }
                }

                _logger.LogInformation(WebConstants.LogEmailSent, email);
            }
            catch (Exception ex)
            {
                _logger.LogError(WebConstants.LogEmailSendingFailed, ex.Message);
            }
        }
    }

    public class EmailSettings
    {
        public string SmtpServer { get; set; } = string.Empty;
        public int SmtpPort { get; set; }
        public string SmtpUsername { get; set; } = string.Empty;
        public string SmtpPassword { get; set; } = string.Empty;
        public bool EnableSsl { get; set; }
        public string SenderEmail { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
    }
}