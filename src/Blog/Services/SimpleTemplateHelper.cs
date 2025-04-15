namespace Blog.Services
{
    /// <summary>
    /// A template helper that reads HTML files and replaces placeholders.
    /// Uses {{placeholder}} syntax.
    /// </summary>
    public class TemplateHelper
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public TemplateHelper(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        /// <summary>
        /// Gets a template from the Templates folder in the project root and replaces placeholders.
        /// </summary>
        /// <param name="templateName">The filename (with extension) in the Templates folder</param>
        /// <param name="replacements">Dictionary of placeholders and their values</param>
        /// <returns>The processed template string</returns>
        public async Task<string> GetTemplateAsync(string templateName, Dictionary<string, string> replacements)
        {
            try
            {
                string templatePath = Path.Combine(_webHostEnvironment.ContentRootPath, "Templates", templateName);

                if (!File.Exists(templatePath))
                {
                    throw new FileNotFoundException($"Template file {templateName} not found at {templatePath}");
                }

                string templateContent = await File.ReadAllTextAsync(templatePath);

                if (!replacements.ContainsKey("CurrentYear"))
                {
                    replacements["CurrentYear"] = DateTime.Now.Year.ToString();
                }

                foreach (var replacement in replacements)
                {
                    templateContent = templateContent.Replace($"{{{{{replacement.Key}}}}}", replacement.Value);
                }

                return templateContent;
            }
            catch (Exception ex)
            {
                return $"<p>There was an error processing the email template: {ex.Message}</p>";
            }
        }
    }
}