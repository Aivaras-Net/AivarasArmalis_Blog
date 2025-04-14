using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Blog.Models;
using Microsoft.AspNetCore.Authorization;
using Blog.Services;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace Blog.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly FileService _fileService;
        private readonly InitialsProfileImageGenerator _initialsGenerator;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            FileService fileService,
            InitialsProfileImageGenerator initialsGenerator)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _fileService = fileService;
            _initialsGenerator = initialsGenerator;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);

                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }

                if (result.RequiresTwoFactor)
                {
                    return RedirectToAction("LoginWith2fa", new { ReturnUrl = Url.Action("Index", "Home"), model.RememberMe });
                }
                if (result.IsLockedOut)
                {
                    return View("Lockout");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return View(model);
                }
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
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

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var model = new ProfileViewModel
            {
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                ExistingProfilePicturePath = user.ProfilePicturePath,
                StatusMessage = TempData["StatusMessage"] as string ?? string.Empty,
                OldPassword = "",
                NewPassword = "",
                ConfirmPassword = ""
            };

            if (TempData["PasswordChangeErrors"] is List<string> passwordErrors)
            {
                foreach (var error in passwordErrors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }
            }
            if (TempData["EmailUpdateErrors"] is List<string> emailErrors)
            {
                foreach (var error in emailErrors) ModelState.AddModelError("Email", error);
            }
            if (TempData["NameUpdateErrors"] is List<string> nameErrors)
            {
                foreach (var error in nameErrors) ModelState.AddModelError("Name", error);
            }

            return View(model);
        }

        [HttpPost("Account/UpdateProfilePicture")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfilePicture(UpdateProfilePictureViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            bool updateRequired = false;
            string? oldProfilePicturePath = user.ProfilePicturePath;
            bool hadCustomPicture = !string.IsNullOrEmpty(oldProfilePicturePath) && !oldProfilePicturePath.Contains("data/UserImages/initials/");

            if (model.RemoveProfilePicture && hadCustomPicture)
            {
                _fileService.DeleteProfilePicture(oldProfilePicturePath!);
                user.ProfilePicturePath = _initialsGenerator.GenerateInitialsImage(user.FirstName, user.LastName, user.Id);
                updateRequired = true;
            }
            else if (model.ProfilePicture != null && model.ProfilePicture.Length > 0)
            {
                if (hadCustomPicture)
                {
                    _fileService.DeleteProfilePicture(oldProfilePicturePath!);
                }

                user.ProfilePicturePath = await _fileService.SaveProfilePictureAsync(model.ProfilePicture, user.Id);
                updateRequired = true;
            }

            if (updateRequired)
            {
                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    TempData["StatusMessage"] = "Error updating profile picture.";
                }
                else
                {
                    await _signInManager.RefreshSignInAsync(user);
                    TempData["StatusMessage"] = "Profile picture updated successfully.";
                }
            }
            else
            {
                TempData["StatusMessage"] = "No changes detected for profile picture.";
            }

            return RedirectToAction(nameof(Profile));
        }

        [HttpPost("Account/UpdateEmail")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateEmail(UpdateEmailViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                TempData["EmailUpdateErrors"] = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                TempData["StatusMessage"] = "Email update failed. Please check the errors.";
                return RedirectToAction(nameof(Profile));
            }

            var currentEmail = await _userManager.GetEmailAsync(user);
            if (model.Email != currentEmail)
            {
                if (user.UserName == currentEmail)
                {
                    var setUserNameResult = await _userManager.SetUserNameAsync(user, model.Email);
                    if (!setUserNameResult.Succeeded)
                    {
                        TempData["StatusMessage"] = $"Error updating username: {string.Join(", ", setUserNameResult.Errors.Select(e => e.Description))}";
                        return RedirectToAction(nameof(Profile));
                    }
                }

                var setEmailResult = await _userManager.SetEmailAsync(user, model.Email);
                if (!setEmailResult.Succeeded)
                {
                    TempData["StatusMessage"] = $"Error updating email: {string.Join(", ", setEmailResult.Errors.Select(e => e.Description))}";
                    return RedirectToAction(nameof(Profile));
                }

                await _signInManager.RefreshSignInAsync(user);
                TempData["StatusMessage"] = "Email updated successfully.";
            }
            else
            {
                TempData["StatusMessage"] = "No changes detected for email.";
            }

            return RedirectToAction(nameof(Profile));
        }

        [HttpPost("Account/UpdateName")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateName(UpdateNameViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                TempData["NameUpdateErrors"] = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                TempData["StatusMessage"] = "Name update failed. Please check the errors.";
                return RedirectToAction(nameof(Profile));
            }

            bool nameChanged = user.FirstName != model.FirstName || user.LastName != model.LastName;

            if (nameChanged)
            {
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;

                bool isInitialsPicture = string.IsNullOrEmpty(user.ProfilePicturePath) || user.ProfilePicturePath.Contains("data/UserImages/initials/");
                if (isInitialsPicture)
                {
                    user.ProfilePicturePath = _initialsGenerator.GenerateInitialsImage(user.FirstName, user.LastName, user.Id);
                }

                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    TempData["StatusMessage"] = $"Error updating name: {string.Join(", ", updateResult.Errors.Select(e => e.Description))}";
                    return RedirectToAction(nameof(Profile));
                }

                await _signInManager.RefreshSignInAsync(user);
                TempData["StatusMessage"] = "Name updated successfully.";
            }
            else
            {
                TempData["StatusMessage"] = "No changes detected for name.";
            }

            return RedirectToAction(nameof(Profile));
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                TempData["PasswordChangeErrors"] = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                TempData["StatusMessage"] = "Password change failed. Please check the errors.";
                return RedirectToAction(nameof(Profile));
            }

            var changePasswordResult = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
            if (!changePasswordResult.Succeeded)
            {
                TempData["PasswordChangeErrors"] = changePasswordResult.Errors.Select(e => e.Description).ToList();
                TempData["StatusMessage"] = "Password change failed. Please check the errors.";
                return RedirectToAction(nameof(Profile));
            }

            await _signInManager.RefreshSignInAsync(user);
            TempData["StatusMessage"] = "Your password has been changed.";
            return RedirectToAction(nameof(Profile));
        }
    }
}