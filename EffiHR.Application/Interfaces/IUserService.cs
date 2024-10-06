using EffiHR.Core.DTOs;
using EffiHR.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using EffiHR.Core.DTOs.User;


namespace EffiHR.Application.Services
{
    public interface IUserService
    {
        Task<UserDTO> GetUserByIdAsync(Guid userId);
        Task<IdentityResult> CreateUserAsync(string userName, string email, string password, string role);
        Task<IdentityResult> AddRoleToUserAsync(string email, string role);
    }
}
