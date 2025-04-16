using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Blog.Models;
using Microsoft.AspNetCore.Authorization;
using Blog.Services;
using System.Text.Encodings.Web;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using System.Runtime.Versioning;

namespace Blog.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAccountService _accountService;
        private readonly IValidationService _validationService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            IAccountService accountService,
            IValidationService validationService,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _accountService = accountService;
            _validationService = validationService;
            _logger = logger;
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
                var result = await _accountService.LoginAsync(model.Email, model.Password, model.RememberMe);

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
        [SupportedOSPlatform("windows")]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var (result, user) = await _accountService.RegisterUserAsync(model);

                if (result.Succeeded)
                {
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
            await _accountService.LogoutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [Authorize]
        [SupportedOSPlatform("windows")]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var model = new ProfileViewModel
            {
                Email = user.Email ?? string.Empty,
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
        [SupportedOSPlatform("windows")]
        public async Task<IActionResult> UpdateProfilePicture(UpdateProfilePictureViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var result = await _accountService.UpdateProfilePictureAsync(user, model);

            if (result)
            {
                TempData["StatusMessage"] = "Your profile picture has been updated";
            }
            else
            {
                TempData["StatusMessage"] = "Error changing profile picture";
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

            if (!_validationService.ValidateUserEmailUpdate(model.Email, ModelState))
            {
                TempData["EmailUpdateErrors"] = ModelState["Email"]?.Errors.Select(e => e.ErrorMessage).ToList();
                return RedirectToAction(nameof(Profile));
            }

            var result = await _accountService.UpdateEmailAsync(user, model.Email);

            if (result.Succeeded)
            {
                TempData["StatusMessage"] = "Your email has been updated";
            }
            else
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                TempData["EmailUpdateErrors"] = errors;
            }

            return RedirectToAction(nameof(Profile));
        }

        [HttpPost("Account/UpdateName")]
        [Authorize]
        [ValidateAntiForgeryToken]
        [SupportedOSPlatform("windows")]
        public async Task<IActionResult> UpdateName(UpdateNameViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!_validationService.ValidateUserNameUpdate(model.FirstName, model.LastName, ModelState))
            {
                var errors = new List<string>();
                if (ModelState.ContainsKey("FirstName"))
                    errors.AddRange(ModelState["FirstName"].Errors.Select(e => e.ErrorMessage));
                if (ModelState.ContainsKey("LastName"))
                    errors.AddRange(ModelState["LastName"].Errors.Select(e => e.ErrorMessage));

                TempData["NameUpdateErrors"] = errors;
                return RedirectToAction(nameof(Profile));
            }

            var result = await _accountService.UpdateNameAsync(user, model.FirstName, model.LastName);

            if (result.Succeeded)
            {
                TempData["StatusMessage"] = "Your name has been updated";
            }
            else
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                TempData["NameUpdateErrors"] = errors;
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
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                TempData["PasswordChangeErrors"] = errors;
                return RedirectToAction(nameof(Profile));
            }

            var result = await _accountService.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);

            if (result.Succeeded)
            {
                TempData["StatusMessage"] = "Your password has been changed";
            }
            else
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                TempData["PasswordChangeErrors"] = errors;
            }

            return RedirectToAction(nameof(Profile));
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    return RedirectToAction(nameof(ForgotPasswordConfirmation));
                }

                var code = await _accountService.GeneratePasswordResetTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                var callbackUrl = Url.Page(
                    "/Account/ResetPassword",
                    null,
                    new { area = "", code, userId = user.Id },
                    Request.Scheme);

                bool emailSent = await _accountService.SendPasswordResetEmailAsync(user, HtmlEncoder.Default.Encode(callbackUrl));

                if (emailSent)
                {
                    return RedirectToAction(nameof(ForgotPasswordConfirmation));
                }
                else
                {
                    ModelState.AddModelError("", "There was an error sending the password reset email. Please try again.");
                    return View(model);
                }
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword(string? code = null, string? userId = null)
        {
            if (code == null || userId == null)
            {
                ModelState.AddModelError("", "Invalid password reset code or user ID.");
                return View();
            }

            var model = new ResetPasswordViewModel
            {
                Code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code)),
                UserId = userId
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!_validationService.ValidatePasswordReset(model, ModelState))
            {
                return View(model);
            }

            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }

            var result = await _accountService.ResetPasswordAsync(user, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }
    }
}