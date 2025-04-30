using Blog.Models;
using Microsoft.AspNetCore.Identity;

namespace Blog.Services.Articles
{
    public interface IAccountService
    {
        Task<SignInResult> LoginAsync(string email, string password, bool rememberMe);
        Task<(IdentityResult Result, ApplicationUser? User)> RegisterUserAsync(RegisterViewModel model);
        Task<bool> UpdateProfilePictureAsync(ApplicationUser user, UpdateProfilePictureViewModel model);
        Task<IdentityResult> UpdateEmailAsync(ApplicationUser user, string newEmail);
        Task<IdentityResult> UpdateNameAsync(ApplicationUser user, string firstName, string lastName);
        Task<IdentityResult> ChangePasswordAsync(ApplicationUser user, string currentPassword, string newPassword);
        Task<string> GeneratePasswordResetTokenAsync(ApplicationUser user);
        Task<IdentityResult> ResetPasswordAsync(ApplicationUser user, string token, string newPassword);
        Task<bool> SendPasswordResetEmailAsync(ApplicationUser user, string callbackUrl);
        Task LogoutAsync();
    }

    public class AccountService : IAccountService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly FileService _fileService;
        private readonly InitialsProfileImageGenerator _initialsGenerator;
        private readonly IEmailSender _emailSender;
        private readonly TemplateHelper _templateHelper;
        private readonly ILogger<AccountService> _logger;

        public AccountService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            FileService fileService,
            InitialsProfileImageGenerator initialsGenerator,
            IEmailSender emailSender,
            TemplateHelper templateHelper,
            ILogger<AccountService> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _fileService = fileService;
            _initialsGenerator = initialsGenerator;
            _emailSender = emailSender;
            _templateHelper = templateHelper;
            _logger = logger;
        }

        public async Task<SignInResult> LoginAsync(string email, string password, bool rememberMe)
        {
            return await _signInManager.PasswordSignInAsync(email, password, rememberMe, lockoutOnFailure: true);
        }

        public async Task<(IdentityResult Result, ApplicationUser? User)> RegisterUserAsync(RegisterViewModel model)
        {
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                string profilePath = _initialsGenerator.GenerateInitialsImage(user.FirstName, user.LastName, user.Id);
                user.ProfilePicturePath = profilePath;
                await _userManager.UpdateAsync(user);

                await _userManager.AddToRoleAsync(user, WebConstants.UserRoleName);
                await _signInManager.SignInAsync(user, isPersistent: false);

                return (result, user);
            }

            return (result, null);
        }

        public async Task<bool> UpdateProfilePictureAsync(ApplicationUser user, UpdateProfilePictureViewModel model)
        {
            try
            {
                string? oldProfilePicturePath = user.ProfilePicturePath;
                bool hadCustomPicture = !string.IsNullOrEmpty(oldProfilePicturePath) &&
                                       !oldProfilePicturePath.Contains(WebConstants.InitialsImagePath);

                if (model.RemoveProfilePicture && hadCustomPicture)
                {
                    _fileService.DeleteProfilePicture(oldProfilePicturePath!);
                    user.ProfilePicturePath = _initialsGenerator.GenerateInitialsImage(
                        user.FirstName, user.LastName, user.Id);
                }
                else if (model.ProfilePicture != null && model.ProfilePicture.Length > 0)
                {
                    if (hadCustomPicture)
                    {
                        _fileService.DeleteProfilePicture(oldProfilePicturePath!);
                    }

                    user.ProfilePicturePath = await _fileService.SaveProfilePictureAsync(
                        model.ProfilePicture, user.Id);
                }
                else
                {
                    return false;
                }

                var result = await _userManager.UpdateAsync(user);
                return result.Succeeded;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, WebConstants.LogProfilePictureUpdateError, user.Id);
                return false;
            }
        }

        public async Task<IdentityResult> UpdateEmailAsync(ApplicationUser user, string newEmail)
        {
            if (user.Email == newEmail)
            {
                return IdentityResult.Success;
            }

            user.Email = newEmail;
            user.UserName = newEmail;
            user.NormalizedEmail = newEmail.ToUpper();
            user.NormalizedUserName = newEmail.ToUpper();

            return await _userManager.UpdateAsync(user);
        }

        public async Task<IdentityResult> UpdateNameAsync(ApplicationUser user, string firstName, string lastName)
        {
            bool nameChanged = user.FirstName != firstName || user.LastName != lastName;

            if (!nameChanged)
            {
                return IdentityResult.Success;
            }

            user.FirstName = firstName;
            user.LastName = lastName;

            // Update profile picture if it's an initials image
            if (!string.IsNullOrEmpty(user.ProfilePicturePath) &&
                user.ProfilePicturePath.Contains(WebConstants.InitialsImagePath))
            {
                string oldPath = user.ProfilePicturePath;
                user.ProfilePicturePath = _initialsGenerator.GenerateInitialsImage(firstName, lastName, user.Id);

                if (oldPath != user.ProfilePicturePath)
                {
                    _fileService.DeleteProfilePicture(oldPath);
                }
            }

            return await _userManager.UpdateAsync(user);
        }

        public async Task<IdentityResult> ChangePasswordAsync(ApplicationUser user, string currentPassword, string newPassword)
        {
            return await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        }

        public async Task<string> GeneratePasswordResetTokenAsync(ApplicationUser user)
        {
            return await _userManager.GeneratePasswordResetTokenAsync(user);
        }

        public async Task<IdentityResult> ResetPasswordAsync(ApplicationUser user, string token, string newPassword)
        {
            return await _userManager.ResetPasswordAsync(user, token, newPassword);
        }

        public async Task<bool> SendPasswordResetEmailAsync(ApplicationUser user, string callbackUrl)
        {
            try
            {
                var replacements = new Dictionary<string, string>
                {
                    { "UserName", $"{user.FirstName} {user.LastName}" },
                    { "ResetLink", callbackUrl }
                };

                string emailBody = await _templateHelper.GetTemplateAsync("PasswordReset.html", replacements);

                await _emailSender.SendEmailAsync(
                    user.Email ?? string.Empty,
                    WebConstants.PasswordResetEmailSubject,
                    emailBody);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, WebConstants.LogPasswordResetEmailFailed, user.Email);
                return false;
            }
        }

        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }
    }
}