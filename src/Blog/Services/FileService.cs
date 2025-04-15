namespace Blog.Services
{
    public class FileService
    {
        private readonly IWebHostEnvironment _environment;

        public FileService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<string> SaveProfilePictureAsync(IFormFile file, string userId)
        {
            if (file == null || file.Length == 0)
            {
                return string.Empty;
            }

            string dataFolder = Path.Combine(_environment.ContentRootPath, "data");
            Directory.CreateDirectory(dataFolder);

            string uploadsFolder = Path.Combine(dataFolder, "UserImages");
            Directory.CreateDirectory(uploadsFolder);

            string uniqueFileName = $"{userId}_{DateTime.Now:yyyyMMddHHmmss}{Path.GetExtension(file.FileName)}";
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return Path.Combine("data", "UserImages", uniqueFileName);
        }

        public void DeleteProfilePicture(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return;
            }

            string fullPath = Path.Combine(_environment.ContentRootPath, filePath);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
    }
}