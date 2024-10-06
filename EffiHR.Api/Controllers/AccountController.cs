//using Microsoft.AspNetCore.Mvc;
//using EffiHR.Application.Interfaces;
//using EffiHR.Domain.Entities;

//namespace EffiHR.API.Controllers
//{
//    [ApiController]
//    [Route("api/[controller]")]
//    public class AccountController : ControllerBase
//    {
//        private readonly IUserService _userService;

//        public AccountController(IUserService userService)
//        {
//            _userService = userService;
//        }

//        [HttpPost("register")]
//        public async Task<IActionResult> Register([FromBody] RegisterModel model)
//        {
//            var user = new User
//            {
//                Username = model.Username,
//                Email = model.Email
//            };

//            var result = await _userService.CreateUserAsync(user, model.Password);

//            if (result)
//            {
//                return Ok(new { Message = "User registered successfully" });
//            }

//            return BadRequest("User registration failed");
//        }

//        [HttpGet("{id}")]
//        public async Task<IActionResult> GetUser(Guid id)
//        {
//            var user = await _userService.GetUserByIdAsync(id);
//            if (user == null)
//                return NotFound();

//            return Ok(user);
//        }
//    }

//    public class RegisterModel
//    {
//        public string Username { get; set; }
//        public string Email { get; set; }
//        public string Password { get; set; }
//    }

//}

using EffiHR.Application.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using EffiHR.Application.Interfaces;
using Microsoft.AspNetCore.Identity;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    private readonly ITokenService _tokenService;




    public UserController(IUserService userService, ITokenService tokenService)
    {
        _userService = userService;
        _tokenService = tokenService;
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateUser(string userName, string email, string password, string role)
    {

        var result = await _userService.CreateUserAsync(userName, email, password, role);
        if (result !=null)
        {
            return Ok("User created successfully");
        }
        return BadRequest(result.Errors);
    }

    // Action để chuyển hướng người dùng đến Google để đăng nhập
    [HttpGet("signin-google")]
    public IActionResult SignInWithGoogle()
    {
        var properties = new AuthenticationProperties
        {
            RedirectUri = Url.Action("GoogleResponse") // Sau khi đăng nhập, Google sẽ chuyển về đây
        };
        return Challenge(properties, GoogleDefaults.AuthenticationScheme); // Bắt đầu quy trình đăng nhập với Google
    }

    // Xử lý phản hồi từ Google
    [HttpGet("google-response")]
    public async Task<IActionResult> GoogleResponse()
    {
        var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
        if (!result.Succeeded)
        {
            return Unauthorized();
        }

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, result.Principal.FindFirst(ClaimTypes.NameIdentifier).Value),
            new Claim(JwtRegisteredClaimNames.UniqueName, result.Principal.FindFirst(ClaimTypes.Name).Value),
        };

        var jwtToken = _tokenService.GenerateJwtToken(claims); // Sử dụng TokenService để tạo JWT

        return Ok(new
        {
            Token = jwtToken
        });
    }

}

