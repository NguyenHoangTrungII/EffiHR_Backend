using Microsoft.AspNetCore.Identity;
using EffiHR.Application.Interfaces;
using EffiHR.Infrastructure.Mappings; 
using System.Threading.Tasks;
using EffiHR.Application.Services;
using EffiHR.Infrastructure.Data;
using EffiHR.Core.DTOs.User;



namespace EffiHR.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserService(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<UserDTO> GetUserByIdAsync(Guid userId)
        {
             ApplicationUser user = await _userManager.FindByIdAsync(userId.ToString());

            if (user == null) return null;

            return new UserDTO
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
            };

        }

        public async Task<IdentityResult> CreateUserAsync(string userName, string email, string password, string role)
        {

            var user = new ApplicationUser
            {
                UserName = userName,
                Email = email,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                return result;
            }

            //if (!await _roleManager.RoleExistsAsync(role))
            //{
            //    await _roleManager.CreateAsync(new IdentityRole(role));
            //}

            //result = await _userManager.AddToRoleAsync(user, role);
            return result;
        }

        public async Task<IdentityResult> AddRoleToUserAsync(string email, string role)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "User not found." });
            }

            if (!await _roleManager.RoleExistsAsync(role))
            {
                return IdentityResult.Failed(new IdentityError { Description = "Role does not exist." });
            }

            return await _userManager.AddToRoleAsync(user, role);
        }
    }
}


