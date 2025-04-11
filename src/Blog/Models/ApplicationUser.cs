using Microsoft.AspNetCore.Identity;

namespace Blog.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string ProfilePicturePath { get; set; } = string.Empty;
    }
}