using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EffiHR.Application.Interfaces
{
    public interface IAuthService
    {
        Task<bool> SeedRolesAsync();
        //Task<bool> RegisterAsync(RegisterModel registerModel);
        //Task<> LoginAsync(LoginModel loginModel);
        //Task<bool> MakeAdminAsync(UpdatePermission updatePermission);
        //Task<bool> MakeOwnerAsync(UpdatePermission updatePermission);

        //Task<bool> ForgotPassword(string email);

        //Task<bool> ResetPassword(ResetPasswordRequest request);


    }
}
