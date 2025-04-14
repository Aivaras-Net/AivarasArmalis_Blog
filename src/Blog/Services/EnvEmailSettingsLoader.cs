using DotNetEnv;

namespace Blog.Services
{
    public class EnvEmailSettingsLoader
    {
        /// <summary>
        /// Loads email settings from .env file
        /// </summary>
        /// <returns>EmailSettings object populated from environment variables</returns>
        public static EmailSettings LoadEmailSettings()
        {
            // Load .env file into environment variables
            Env.Load();

            return new EmailSettings
            {
                SmtpServer = Environment.GetEnvironmentVariable("EMAIL_SMTP_SERVER") ?? "smtp.gmail.com",
                SmtpPort = int.TryParse(Environment.GetEnvironmentVariable("EMAIL_SMTP_PORT"), out int port) ? port : 587,
                SmtpUsername = Environment.GetEnvironmentVariable("EMAIL_SMTP_USERNAME") ?? "",
                SmtpPassword = Environment.GetEnvironmentVariable("EMAIL_SMTP_PASSWORD") ?? "",
                EnableSsl = bool.TryParse(Environment.GetEnvironmentVariable("EMAIL_ENABLE_SSL"), out bool enableSsl) ? enableSsl : true,
                SenderEmail = Environment.GetEnvironmentVariable("EMAIL_SENDER_EMAIL") ?? "",
                SenderName = Environment.GetEnvironmentVariable("EMAIL_SENDER_NAME") ?? "Blog Application"
            };
        }
    }
}