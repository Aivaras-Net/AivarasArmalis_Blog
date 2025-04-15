using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Blog.Models;
using Microsoft.AspNetCore.Authorization;

namespace Blog.Controllers
{
    [Authorize(Roles = "Admin")]
    public class RolesController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public RolesController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var roles = _roleManager.Roles.ToList();
            return View(roles);
        }

        [HttpGet]
        public async Task<IActionResult> ManageUsers()
        {
            var users = _userManager.Users.ToList();
            var userRolesViewModel = new List<UserRolesViewModel>();

            foreach (var user in users)
            {
                var userRoles = await _userManager.GetRolesAsync(user);
                var viewModel = new UserRolesViewModel
                {
                    UserId = user.Id,
                    Email = user.Email,
                    FullName = $"{user.FirstName} {user.LastName}",
                    Roles = userRoles.ToList()
                };
                userRolesViewModel.Add(viewModel);
            }

            return View(userRolesViewModel);
        }

        [HttpGet]
        public async Task<IActionResult> ManageUserRoles(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var model = new List<ManageUserRolesViewModel>();
            var roles = _roleManager.Roles.ToList();
            var userRoles = await _userManager.GetRolesAsync(user);

            var currentUser = await _userManager.GetUserAsync(User);
            bool isCurrentUser = currentUser?.Id == userId;

            foreach (var role in roles)
            {
                var userRolesViewModel = new ManageUserRolesViewModel
                {
                    RoleId = role.Id,
                    RoleName = role.Name,
                    Selected = userRoles.Contains(role.Name)
                };
                model.Add(userRolesViewModel);
            }

            ViewBag.UserName = $"{user.FirstName} {user.LastName}";
            ViewBag.UserId = userId;
            ViewBag.IsCurrentUser = isCurrentUser;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateUserRoles(string userId, List<ManageUserRolesViewModel> model)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            bool isCurrentUserAdmin = currentUser?.Id == userId;
            bool adminRoleChanged = false;

            if (isCurrentUserAdmin)
            {
                var adminRole = await _roleManager.FindByNameAsync("Admin");
                if (adminRole != null)
                {
                    var adminRoleInModel = model.FirstOrDefault(r => r.RoleName == "Admin");

                    if (adminRoleInModel != null && !adminRoleInModel.Selected)
                    {
                        adminRoleInModel.Selected = true;
                        adminRoleChanged = true;
                    }
                }
            }

            var roles = await _userManager.GetRolesAsync(user);

            var removeResult = await _userManager.RemoveFromRolesAsync(user, roles);
            if (!removeResult.Succeeded)
            {
                TempData["Error"] = "Failed to remove user roles.";
                return RedirectToAction(nameof(ManageUsers));
            }

            var selectedRoles = model.Where(x => x.Selected).Select(y => y.RoleName).ToList();
            var addResult = await _userManager.AddToRolesAsync(user, selectedRoles);

            if (addResult.Succeeded)
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
                TempData["Error"] = "Failed to add user to roles.";
            }

            return RedirectToAction(nameof(ManageUsers));
        }
    }
}