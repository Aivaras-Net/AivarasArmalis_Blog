using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Blog.Models;
using Microsoft.AspNetCore.Authorization;
using Blog.Services;

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
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);

                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(model);
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
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
                ExistingProfilePicturePath = user.ProfilePicturePath
            };

            return View(model);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var email = await _userManager.GetEmailAsync(user);
            if (model.Email != email)
            {
                var setEmailResult = await _userManager.SetEmailAsync(user, model.Email);
                if (!setEmailResult.Succeeded)
                {
                    foreach (var error in setEmailResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View(model);
                }
            }

            bool nameChanged = user.FirstName != model.FirstName || user.LastName != model.LastName;
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;

            if (model.RemoveProfilePicture && !string.IsNullOrEmpty(user.ProfilePicturePath))
            {
                if (!user.ProfilePicturePath.Contains("data/UserImages/initials/"))
                {
                    _fileService.DeleteProfilePicture(user.ProfilePicturePath);
                }

                string profilePath = _initialsGenerator.GenerateInitialsImage(user.FirstName, user.LastName, user.Id);
                user.ProfilePicturePath = profilePath;
            }
            else if (model.ProfilePicture != null && model.ProfilePicture.Length > 0)
            {
                if (!string.IsNullOrEmpty(user.ProfilePicturePath) && !user.ProfilePicturePath.Contains("data/UserImages/initials/"))
                {
                    _fileService.DeleteProfilePicture(user.ProfilePicturePath);
                }

                string profilePicturePath = await _fileService.SaveProfilePictureAsync(model.ProfilePicture, user.Id);
                user.ProfilePicturePath = profilePicturePath;
            }
            else if (nameChanged && (string.IsNullOrEmpty(user.ProfilePicturePath) || user.ProfilePicturePath.Contains("data/UserImages/initials/")))
            {
                string profilePath = _initialsGenerator.GenerateInitialsImage(user.FirstName, user.LastName, user.Id);
                user.ProfilePicturePath = profilePath;
            }

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Unexpected error when trying to update profile.");
                return View(model);
            }

            await _signInManager.RefreshSignInAsync(user);
            model.StatusMessage = "Your profile has been updated";
            model.ExistingProfilePicturePath = user.ProfilePicturePath;
            return View(model);
        }

        [HttpGet]
        [Authorize]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var changePasswordResult = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
            if (!changePasswordResult.Succeeded)
            {
                foreach (var error in changePasswordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }

            await _signInManager.RefreshSignInAsync(user);
            return RedirectToAction(nameof(Profile));
        }
    }
}