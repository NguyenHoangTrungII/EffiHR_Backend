using EffiHR.Application.Interfaces;
using EffiHR.Core.Constants;
using EffiHR.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EffiHR.Infrastructure.Services
{
    public class AuthService: IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;


        public AuthService(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }

        public async Task<bool> SeedRolesAsync()
        {
            bool isAdminRoleExists = await _roleManager.RoleExistsAsync(StaticUserRoles.ADMIN);
            bool isRentalManagerRoleExists = await _roleManager.RoleExistsAsync(StaticUserRoles.RENTAL_MANAGER);
            bool isTenantRoleExists = await _roleManager.RoleExistsAsync(StaticUserRoles.TENANT);
            bool isLandLordRoleExists = await _roleManager.RoleExistsAsync(StaticUserRoles.LANDLORD);
            bool isTechnicianRoleExists = await _roleManager.RoleExistsAsync(StaticUserRoles.TECHNICIAN);



            if (isRentalManagerRoleExists && isAdminRoleExists && isTenantRoleExists && isLandLordRoleExists && isTechnicianRoleExists)
                return false;


            await _roleManager.CreateAsync(new IdentityRole(StaticUserRoles.RENTAL_MANAGER));
            await _roleManager.CreateAsync(new IdentityRole(StaticUserRoles.ADMIN));
            await _roleManager.CreateAsync(new IdentityRole(StaticUserRoles.TENANT));
            await _roleManager.CreateAsync(new IdentityRole(StaticUserRoles.LANDLORD));
            await _roleManager.CreateAsync(new IdentityRole(StaticUserRoles.TECHNICIAN));


            return true;

        }
    }
}
