using System.Drawing;
using System.Drawing.Imaging;

namespace Blog.Services
{
    public class InitialsProfileImageGenerator
    {
        private readonly IWebHostEnvironment _environment;
        private readonly Random _random = new Random();

        public InitialsProfileImageGenerator(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public string GenerateInitialsImage(string firstName, string lastName, string userId)
        {
            string dataFolder = Path.Combine(_environment.ContentRootPath, "data");
            Directory.CreateDirectory(dataFolder);

            string userImagesFolder = Path.Combine(dataFolder, "UserImages");
            Directory.CreateDirectory(userImagesFolder);

            string initialsFolder = Path.Combine(userImagesFolder, "initials");
            Directory.CreateDirectory(initialsFolder);

            string initials = GetInitials(firstName, lastName);

            string filename = $"initial_{userId}.png";
            string filePath = Path.Combine(initialsFolder, filename);
            string relativePath = Path.Combine("data", "UserImages", "initials", filename);

            using (Bitmap bitmap = new Bitmap(200, 200))
            {
                using (Graphics graphics = Graphics.FromImage(bitmap))
                {
                    Color backgroundColor = GetRandomPastelColor();

                    graphics.Clear(backgroundColor);

                    Font font = new Font("Arial", 72, FontStyle.Bold);
                    Brush textBrush = Brushes.White;

                    SizeF textSize = graphics.MeasureString(initials, font);
                    float x = (bitmap.Width - textSize.Width) / 2;
                    float y = (bitmap.Height - textSize.Height) / 2;

                    graphics.DrawString(initials, font, textBrush, x, y);

                    font.Dispose();
                }

                bitmap.Save(filePath, ImageFormat.Png);
            }

            return relativePath;
        }

        private string GetInitials(string firstName, string lastName)
        {
            string initials = "";

            if (!string.IsNullOrEmpty(firstName) && firstName.Length > 0)
            {
                initials += char.ToUpper(firstName[0]);
            }

            if (!string.IsNullOrEmpty(lastName) && lastName.Length > 0)
            {
                initials += char.ToUpper(lastName[0]);
            }

            if (string.IsNullOrEmpty(initials))
            {
                initials = "U";
            }

            return initials;
        }

        private Color GetRandomPastelColor()
        {
            int r = _random.Next(100, 200);
            int g = _random.Next(100, 200);
            int b = _random.Next(100, 200);

            return Color.FromArgb(r, g, b);
        }
    }
}