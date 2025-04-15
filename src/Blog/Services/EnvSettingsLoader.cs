using DotNetEnv;

namespace Blog.Services
{
    public class EnvSettingsLoader
    {
        /// <summary>
        /// Loads environment variables from .env file
        /// </summary>
        private static void LoadEnvironmentVariables()
        {
            Env.Load();
        }

        /// <summary>
        /// Loads email settings from .env file
        /// </summary>
        /// <returns>EmailSettings object populated from environment variables</returns>
        public static EmailSettings LoadEmailSettings()
        {
            LoadEnvironmentVariables();

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

        /// <summary>
        /// Loads admin user settings from .env file
        /// </summary>
        /// <returns>AdminSettings object populated from environment variables</returns>
        public static AdminSettings LoadAdminSettings()
        {
            LoadEnvironmentVariables();

            return new AdminSettings
            {
                Email = Environment.GetEnvironmentVariable("ADMIN_EMAIL") ?? "",
                Password = Environment.GetEnvironmentVariable("ADMIN_PASSWORD") ?? ""
            };
        }
    }

    public class AdminSettings
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}