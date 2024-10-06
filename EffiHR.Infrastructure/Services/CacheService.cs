//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using StackExchange.Redis;
//using EffiHR.Application.Interfaces;
//using Newtonsoft.Json;

//namespace EffiHR.Infrastructure.Services
//{
//    public class CacheService : ICacheService
//    {
//        private readonly IDatabase _cache;

//        public CacheService(IConnectionMultiplexer redis)
//        {
//            _cache = redis.GetDatabase();
//        }

//        public async Task SetAsync<T>(string key, T value, TimeSpan expiration)
//        {
//            var serializedValue = JsonConvert.SerializeObject(value);
//            await _cache.StringSetAsync(key, serializedValue, expiration);
//        }

//        public async Task<T> GetAsync<T>(string key)
//        {
//            var value = await _cache.StringGetAsync(key);
//            if (value.IsNullOrEmpty)
//            {
//                return default;
//            }
//            return JsonConvert.DeserializeObject<T>(value);
//        }

//        public async Task RemoveAsync(string key)
//        {
//            await _cache.KeyDeleteAsync(key);
//        }
//    }
//}

using System;
using System.Threading.Tasks;
using EffiHR.Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace EffiHR.Infrastructure.Services
{
    public class CacheService : ICacheService
    {
        private readonly IMemoryCache _cache;

        public CacheService(IMemoryCache memoryCache)
        {
            _cache = memoryCache;
        }

        public Task SetAsync<T>(string key, T value, TimeSpan expiration)
        {
            // Đặt dữ liệu vào cache với thời gian hết hạn
            _cache.Set(key, value, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration
            });

            return Task.CompletedTask;
        }

        public Task<T> GetAsync<T>(string key)
        {
            // Lấy dữ liệu từ cache
            _cache.TryGetValue(key, out T value);
            return Task.FromResult(value);
        }

        public Task RemoveAsync(string key)
        {
            // Xóa dữ liệu khỏi cache
            _cache.Remove(key);
            return Task.CompletedTask;
        }
    }
}



