using Blog.Models;
using Microsoft.AspNetCore.Identity;

namespace Blog.Services
{
    public interface IRoleService
    {
        IEnumerable<IdentityRole> GetAllRoles();
        Task<List<UserRolesViewModel>> GetAllUserRolesAsync();
        Task<ApplicationUser?> GetUserByIdAsync(string userId);
        Task<List<ManageUserRolesViewModel>> GetUserRolesViewModelAsync(string userId);
        Task<(bool Succeeded, bool AdminRoleChanged)> UpdateUserRolesAsync(string userId, string currentUserId, List<ManageUserRolesViewModel> model);
    }

    public class RoleService : IRoleService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<RoleService> _logger;

        public RoleService(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<RoleService> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        public IEnumerable<IdentityRole> GetAllRoles()
        {
            return _roleManager.Roles.ToList();
        }

        public async Task<List<UserRolesViewModel>> GetAllUserRolesAsync()
        {
            var users = _userManager.Users.ToList();
            var userRolesViewModel = new List<UserRolesViewModel>();

            foreach (var user in users)
            {
                var userRoles = await _userManager.GetRolesAsync(user);
                var viewModel = new UserRolesViewModel
                {
                    UserId = user.Id,
                    Email = user.Email ?? string.Empty,
                    FullName = $"{user.FirstName} {user.LastName}",
                    Roles = userRoles.ToList()
                };
                userRolesViewModel.Add(viewModel);
            }

            return userRolesViewModel;
        }

        public async Task<ApplicationUser?> GetUserByIdAsync(string userId)
        {
            return await _userManager.FindByIdAsync(userId);
        }

        public async Task<List<ManageUserRolesViewModel>> GetUserRolesViewModelAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return new List<ManageUserRolesViewModel>();
            }

            var model = new List<ManageUserRolesViewModel>();
            var roles = _roleManager.Roles.ToList();
            var userRoles = await _userManager.GetRolesAsync(user);

            foreach (var role in roles)
            {
                var userRolesViewModel = new ManageUserRolesViewModel
                {
                    RoleId = role.Id,
                    RoleName = role.Name ?? string.Empty,
                    Selected = userRoles.Contains(role.Name ?? string.Empty)
                };
                model.Add(userRolesViewModel);
            }

            return model;
        }

        public async Task<(bool Succeeded, bool AdminRoleChanged)> UpdateUserRolesAsync(string userId, string currentUserId, List<ManageUserRolesViewModel> model)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return (false, false);
            }

            bool isCurrentUserAdmin = currentUserId == userId;
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
                _logger.LogWarning("Failed to remove roles from user {UserId}", userId);
                return (false, adminRoleChanged);
            }

            var selectedRoles = model.Where(x => x.Selected).Select(y => y.RoleName).ToList();
            var addResult = await _userManager.AddToRolesAsync(user, selectedRoles);

            return (addResult.Succeeded, adminRoleChanged);
        }
    }
}