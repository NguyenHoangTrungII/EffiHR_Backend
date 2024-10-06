using EffiHR.Application.Interfaces;
using EffiHR.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using EffiHR.Core.DTOs.Auth;
using EffiHR.Core.Constants;
using EffiHR.Application.Services;

namespace EffiHR.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IAuthService _authService;
        private readonly IConfiguration _configuration;
        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService;
        private readonly ISessionService _sessionService;

        public AuthController(IAuthService authService, UserManager<ApplicationUser> userManager, IConfiguration configuration, ITokenService tokenService, RoleManager<IdentityRole> roleManager, IEmailService emailService, ISessionService sessionService)
        {
            _authService = authService;
            _userManager = userManager;
            _configuration = configuration;
            _tokenService = tokenService;
            _roleManager = roleManager;
            _emailService = emailService;
            _sessionService = sessionService;
        }

        [HttpPost]
        [Route("seed-roles")]
        //[Authorize(Roles = "ADMIN, OWNER")]
        public async Task<IActionResult> SeedRoles()
        {
            var seerRoles = await _authService.SeedRolesAsync();
            return Ok(seerRoles);
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserRequest registerRequest)
        {
            // Kiểm tra xem role có hợp lệ không
            //if (!new[] { StaticUserRoles.ADMIN, StaticUserRoles.LANDLORD, StaticUserRoles.TENANT, StaticUserRoles.TECHNICIAN}.Contains(registerRequest.Role))
            //{
            //    return BadRequest(new { message = "Invalid role." });
            //}

            // Kiểm tra xem người dùng đã tồn tại chưa
            var userExists = await _userManager.FindByNameAsync(registerRequest.Username);
            if (userExists != null)
                return BadRequest(new { message = "User already exists!" });

            // Tạo người dùng mới
            var user = new ApplicationUser
            {
                UserName = registerRequest.Username,
                Email = registerRequest.Email,
                EmailConfirmed = true // Có thể thêm logic xác nhận email sau nếu cần
            };

            // Thêm người dùng vào hệ thống
            var result = await _userManager.CreateAsync(user, registerRequest.Password);
            if (!result.Succeeded)
            {
                return StatusCode(500, new { message = "User creation failed! Please check user details and try again." });
            }

            // Gán quyền (role) cho người dùng
            if (!await _roleManager.RoleExistsAsync(registerRequest.Role))
            {
                await _roleManager.CreateAsync(new IdentityRole(registerRequest.Role));
            }
            await _userManager.AddToRoleAsync(user, registerRequest.Role);

            return Ok(new { message = "User created successfully!" });
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            // Tìm kiếm người dùng theo username
            var user = await _userManager.FindByNameAsync(loginRequest.Username);
            if (user == null)
            {
                return Unauthorized(new { message = "Invalid username or password." });
            }

            // Kiểm tra mật khẩu
            var result = await _userManager.CheckPasswordAsync(user, loginRequest.Password);
            if (!result)
            {
                return Unauthorized(new { message = "Invalid username or password." });
            }

            // Tạo các claims cho JWT
            var authClaims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id),
        new Claim(ClaimTypes.Name, user.UserName),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

            // Thêm các quyền (roles) của người dùng vào claims
            var userRoles = await _userManager.GetRolesAsync(user);
            foreach (var role in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, role));
            }

            // Tạo JWT token
            var token = _tokenService.GenerateJwtToken(authClaims);

            // Lấy địa chỉ IP của người dùng (có thể tùy vào cấu hình của bạn)
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

            // Lấy thông tin thiết bị từ request nếu có
            var deviceInfo = Request.Headers["User-Agent"].ToString();

            // Tạo thông tin phiên (session) cho người dùng
            var userSession = new UserSession
            {
                SessionId = Guid.NewGuid().ToString(),  // Tạo SessionId ngẫu nhiên
                UserId = user.Id,
                JwtToken = new JwtSecurityTokenHandler().WriteToken(token),
                DeviceInfo = deviceInfo,   // Thông tin thiết bị từ User-Agent
                IPAddress = ipAddress,     // Địa chỉ IP của người dùng
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = token.ValidTo,  // Thời gian hết hạn của JWT token
                IsActive = true             // Phiên đang hoạt động
            };

            // Lưu phiên đăng nhập vào cache
            await _sessionService.StoreSessionAsync(userSession);

            // Trả về JWT token cho client
            return Ok(new
            {
                token = userSession.JwtToken,
                expiration = userSession.ExpiresAt
            });
        }


        [HttpPost]
        [Route("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return BadRequest(new { message = "User not found" });
            }

            // Tạo token đặt lại mật khẩu
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

            // Tạo URL đặt lại mật khẩu
            var resetUrl = $"{_configuration["ClientUrl"]}/reset-password?token={Uri.EscapeDataString(resetToken)}&email={user.Email}";

            // Gửi email
            //await _emailService.SendEmailAsync(user.Email, "Reset your password ", $"Click here to reset your password: <a href='{resetUrl}'>link</a>");
            await _emailService.SendEmailAsync(user.Email, "Reset your password ", $"'{resetToken}'");

            return Ok(new { message = "Password reset link has been sent to your email." });
        }


        // Chức năng Đổi mật khẩu
        [HttpPost]
        [Route("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest resetPasswordRequest)
        {
            // Tìm kiếm người dùng qua email
            var user = await _userManager.FindByEmailAsync(resetPasswordRequest.Email);
            if (user == null)
            {
                return BadRequest(new { message = "User not found." });
            }

            // Đặt lại mật khẩu sử dụng token
            var resetPassResult = await _userManager.ResetPasswordAsync(user, resetPasswordRequest.Token, resetPasswordRequest.NewPassword);
            if (!resetPassResult.Succeeded)
            {
                return BadRequest(new { message = "Error resetting password.", errors = resetPassResult.Errors });
            }

            return Ok(new { message = "Password has been reset successfully." });
        }

    }
}
