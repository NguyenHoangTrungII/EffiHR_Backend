using EffiHR.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using EffiHR.Core.DTOs.Auth;
using Microsoft.AspNetCore.Authorization;

namespace EffiHR.Api.Controllers
{
    [ApiController]
    [Route("api/sessions")]
    [Authorize]
    public class SessionController : ControllerBase
    {
        private readonly ISessionService _sessionService;

        public SessionController(ISessionService sessionService)
        {
            _sessionService = sessionService;
        }

        [HttpGet]
        [Route("active-sessions")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> GetActiveSessions()
        {
            var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();  
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var sessions = await _sessionService.GetSessionsByUserAsync(userId);
            return Ok(sessions);
        }

        [HttpPost]
        [Route("invalidate-session")]
        public async Task<IActionResult> InvalidateSession([FromBody] InvalidateSessionRequest request)
        {
            await _sessionService.InvalidateSessionAsync(request.SessionId);
            return Ok(new { message = "Session invalidated successfully" });
        }

        [HttpPost]
        [Route("invalidate-all-sessions-except-current")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> InvalidateAllSessionsExceptCurrent([FromBody] InvalidateAllSessionsExceptCurrentRequest request)
        {
            // Get userId from the user's token
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest(new { message = "UserId not found." });
            }

            // Get all sessions of the user
            var userSessions = await _sessionService.GetSessionsByUserAsync(userId);

            if (userSessions == null || !userSessions.Any())
            {
                return Ok(new { message = "No active sessions to invalidate." });
            }

            // Invalidate all sessions except the current one
            foreach (var session in userSessions)
            {
                if (session.SessionId != request.CurrentSessionId)
                {
                    await _sessionService.InvalidateSessionAsync(session.SessionId);
                }
            }

            return Ok(new { message = "All other sessions have been invalidated." });
        }


        [HttpPost]
        [Route("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest logoutRequest)
        {
            await _sessionService.InvalidateSessionAsync(logoutRequest.SessionId);

            return Ok(new { message = "Đã đăng xuất thành công." });
        }

    }

}
