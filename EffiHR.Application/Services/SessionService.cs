//using EffiHR.Application.Interfaces;
//using EffiHR.Core.DTOs.Auth;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace EffiHR.Application.Services
//{
//    public class SessionService : ISessionService
//    {
//        private readonly ICacheService _cacheService;  // Sử dụng cache service (Redis)
//        private readonly TimeSpan _sessionTimeout = TimeSpan.FromHours(3);

//        public SessionService(ICacheService cacheService)
//        {
//            _cacheService = cacheService;
//        }

//        public async Task StoreSessionAsync(UserSession session)
//        {
//            await _cacheService.SetAsync(session.SessionId, session, _sessionTimeout);
//        }

//        public async Task<List<UserSession>> GetSessionsByUserAsync(string userId)
//        {
//            var userSessionKeys = await _cacheService.GetAsync<List<string>>($"user-sessions-{userId}")
//                                  ?? new List<string>();

//            var userSessions = new List<UserSession>();

//            foreach (var sessionId in userSessionKeys)
//            {
//                var session = await _cacheService.GetAsync<UserSession>(sessionId);
//                if (session != null)
//                {
//                    userSessions.Add(session);
//                }
//            }

//            return userSessions;
//        }

//        public async Task InvalidateSessionAsync(string sessionId)
//        {
//            await _cacheService.RemoveAsync(sessionId);
//        }

//        public async Task InvalidateAllSessionsExceptCurrentAsync(string userId, string currentSessionId)
//        {
//            var userSessionKeys = await _cacheService.GetAsync<List<string>>($"user-sessions-{userId}")
//                                 ?? new List<string>();

//            foreach (var sessionId in userSessionKeys.ToList())
//            {
//                if (sessionId != currentSessionId)
//                {
//                    await _cacheService.RemoveAsync(sessionId);  // Invalidate the session
//                    userSessionKeys.Remove(sessionId);  // Remove from the list
//                }
//            }

//            // Update the session key list with only the current session
//            await _cacheService.SetAsync($"user-sessions-{userId}", userSessionKeys, _sessionTimeout);
//        }
//    }
//}

//using EffiHR.Application.Interfaces;
//using EffiHR.Core.DTOs.Auth;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;

//namespace EffiHR.Application.Services
//{
//    public class SessionService : ISessionService
//    {
//        private readonly ICacheService _cacheService;  // Sử dụng Redis để quản lý cache
//        private readonly TimeSpan _sessionTimeout = TimeSpan.FromHours(3);  // Thời gian hết hạn của phiên

//        public SessionService(ICacheService cacheService)
//        {
//            _cacheService = cacheService;
//        }

//        public async Task StoreSessionAsync(UserSession session)
//        {
//            // Lấy danh sách các phiên của người dùng (nếu đã tồn tại)
//            var userSessions = await _cacheService.GetAsync<List<string>>($"user-sessions-{session.UserId}")
//                               ?? new List<string>();

//            // Thêm sessionId mới vào danh sách
//            userSessions.Add(session.SessionId);

//            // Lưu lại danh sách sessionId vào cache với thời gian hết hạn
//            await _cacheService.SetAsync($"user-sessions-{session.UserId}", userSessions, session.ExpiresAt - DateTime.UtcNow);

//            // Lưu phiên cụ thể vào cache với sessionId, và thời gian hết hạn giống với JWT token
//            await _cacheService.SetAsync(session.SessionId, session, session.ExpiresAt - DateTime.UtcNow);
//        }

//        public async Task<List<UserSession>> GetSessionsByUserAsync(string userId)
//        {
//            // Lấy danh sách các ID phiên đăng nhập của người dùng
//            var userSessionKeys = await _cacheService.GetAsync<List<string>>($"user-sessions-{userId}")
//                                  ?? new List<string>();

//            var userSessions = new List<UserSession>();

//            foreach (var sessionId in userSessionKeys)
//            {
//                var session = await _cacheService.GetAsync<UserSession>(sessionId);
//                if (session != null)
//                {
//                    userSessions.Add(session);
//                }
//            }

//            return userSessions;
//        }

//        public async Task InvalidateSessionAsync(string sessionId)
//        {
//            // Vô hiệu hóa một phiên đăng nhập
//            var session = await _cacheService.GetAsync<UserSession>(sessionId);
//            if (session != null)
//            {
//                var userSessionKeys = await _cacheService.GetAsync<List<string>>($"user-sessions-{session.UserId}")
//                                      ?? new List<string>();

//                userSessionKeys.Remove(sessionId);
//                await _cacheService.SetAsync($"user-sessions-{session.UserId}", userSessionKeys, _sessionTimeout);
//            }

//            await _cacheService.RemoveAsync(sessionId);  // Xóa phiên đăng nhập khỏi cache
//        }

//        public async Task InvalidateAllSessionsExceptCurrentAsync(string userId, string currentSessionId)
//        {
//            // Lấy danh sách các ID phiên của người dùng
//            var userSessionKeys = await _cacheService.GetAsync<List<string>>($"user-sessions-{userId}")
//                                  ?? new List<string>();

//            foreach (var sessionId in userSessionKeys.ToList())
//            {
//                if (sessionId != currentSessionId)
//                {
//                    await _cacheService.RemoveAsync(sessionId);  // Vô hiệu hóa phiên
//                    userSessionKeys.Remove(sessionId);  // Xóa khỏi danh sách
//                }
//            }

//            // Cập nhật danh sách phiên chỉ còn lại phiên hiện tại
//            await _cacheService.SetAsync($"user-sessions-{userId}", userSessionKeys, _sessionTimeout);
//        }
//    }
//}


using EffiHR.Application.Interfaces;
using EffiHR.Core.DTOs.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EffiHR.Application.Services
{
    public class SessionService : ISessionService
    {
        private readonly ICacheService _cacheService;  // Sử dụng Redis để quản lý cache
        private readonly TimeSpan _sessionTimeout = TimeSpan.FromHours(3);  // Thời gian hết hạn của phiên

        public SessionService(ICacheService cacheService)
        {
            _cacheService = cacheService;
        }

        public async Task StoreSessionAsync(UserSession session)
        {
            // Lấy danh sách các phiên của người dùng (nếu đã tồn tại)
            var userSessions = await _cacheService.GetAsync<List<string>>($"user-sessions-{session.UserId}")
                               ?? new List<string>();

            // Thêm sessionId mới vào danh sách
            userSessions.Add(session.SessionId);

            // Lưu lại danh sách sessionId vào cache với thời gian hết hạn
            await _cacheService.SetAsync($"user-sessions-{session.UserId}", userSessions, _sessionTimeout);

            // Lưu phiên cụ thể vào cache với sessionId, và thời gian hết hạn giống với JWT token
            await _cacheService.SetAsync(session.SessionId, session, session.ExpiresAt - DateTime.UtcNow);
        }

        public async Task<List<UserSession>> GetSessionsByUserAsync(string userId)
        {
            // Lấy danh sách các ID phiên đăng nhập của người dùng
            var userSessionKeys = await _cacheService.GetAsync<List<string>>($"user-sessions-{userId}")
                                  ?? new List<string>();

            var userSessions = new List<UserSession>();

            foreach (var sessionId in userSessionKeys)
            {
                var session = await _cacheService.GetAsync<UserSession>(sessionId);
                if (session != null)
                {
                    userSessions.Add(session);
                }
            }

            return userSessions;
        }

        public async Task InvalidateSessionAsync(string sessionId)
        {
            // Vô hiệu hóa một phiên đăng nhập
            var session = await _cacheService.GetAsync<UserSession>(sessionId);
            if (session != null)
            {
                var userSessionKeys = await _cacheService.GetAsync<List<string>>($"user-sessions-{session.UserId}")
                                      ?? new List<string>();

                userSessionKeys.Remove(sessionId);
                await _cacheService.SetAsync($"user-sessions-{session.UserId}", userSessionKeys, _sessionTimeout);
            }

            await _cacheService.RemoveAsync(sessionId);  // Xóa phiên đăng nhập khỏi cache
        }

        public async Task InvalidateAllSessionsExceptCurrentAsync(string userId, string currentSessionId)
        {
            // Lấy danh sách các ID phiên của người dùng
            var userSessionKeys = await _cacheService.GetAsync<List<string>>($"user-sessions-{userId}")
                                  ?? new List<string>();

            foreach (var sessionId in userSessionKeys.ToList())
            {
                if (sessionId != currentSessionId)
                {
                    await _cacheService.RemoveAsync(sessionId);  // Vô hiệu hóa phiên
                    userSessionKeys.Remove(sessionId);  // Xóa khỏi danh sách
                }
            }

            // Cập nhật danh sách phiên chỉ còn lại phiên hiện tại
            await _cacheService.SetAsync($"user-sessions-{userId}", userSessionKeys, _sessionTimeout);
        }
    }
}

