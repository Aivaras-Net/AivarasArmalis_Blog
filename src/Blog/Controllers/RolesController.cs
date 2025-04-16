using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Blog.Models;
using Microsoft.AspNetCore.Authorization;
using Blog.Services;

namespace Blog.Controllers
{
    [Authorize(Roles = "Admin")]
    public class RolesController : Controller
    {
        private readonly IRoleService _roleService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<RolesController> _logger;

        public RolesController(
            IRoleService roleService,
            UserManager<ApplicationUser> userManager,
            ILogger<RolesController> logger)
        {
            _roleService = roleService;
            _userManager = userManager;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var roles = _roleService.GetAllRoles();
            return View(roles);
        }

        [HttpGet]
        public async Task<IActionResult> ManageUsers()
        {
            var userRolesViewModel = await _roleService.GetAllUserRolesAsync();
            return View(userRolesViewModel);
        }

        [HttpGet]
        public async Task<IActionResult> ManageUserRoles(string userId)
        {
            var user = await _roleService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var model = await _roleService.GetUserRolesViewModelAsync(userId);
            var currentUser = await _userManager.GetUserAsync(User);
            bool isCurrentUser = currentUser?.Id == userId;

            ViewBag.UserName = $"{user.FirstName} {user.LastName}";
            ViewBag.UserId = userId;
            ViewBag.IsCurrentUser = isCurrentUser;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateUserRoles(string userId, List<ManageUserRolesViewModel> model)
        {
            var user = await _roleService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            var currentUserId = currentUser?.Id ?? string.Empty;

            var (succeeded, adminRoleChanged) = await _roleService.UpdateUserRolesAsync(userId, currentUserId, model);

            if (succeeded)
            {
                if (adminRoleChanged)
                {
                    TempData["Success"] = "You cannot remove your own Admin role. Other roles updated successfully.";
                }
                else
                {
                    TempData["Success"] = "Roles updated successfully.";
                }
            }
            else
            {
                TempData["Error"] = "Failed to update user roles.";
                _logger.LogError("Failed to update roles for user {UserId}", userId);
            }

            return RedirectToAction(nameof(ManageUsers));
        }
    }
}