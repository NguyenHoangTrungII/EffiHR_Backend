using System;
using System.Text.Json;
using System.Threading.Tasks;
using EffiHR.Application.Interfaces;
using Microsoft.Extensions.Caching.Distributed;

namespace EffiHR.Infrastructure.Services
{
    public class RedisCacheService : ICacheService
    {
        private readonly IDistributedCache _cache;

        public RedisCacheService(IDistributedCache distributedCache)
        {
            _cache = distributedCache;
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan expiration)
        {
            // Thiết lập tùy chọn cho cache
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration
            };

            // Serialize đối tượng thành chuỗi JSON
            var serializedValue = JsonSerializer.Serialize(value);

            // Lưu giá trị vào Redis
            await _cache.SetStringAsync(key, serializedValue, options);
        }

        public async Task<T> GetAsync<T>(string key)
        {
            // Lấy chuỗi JSON từ Redis
            var serializedValue = await _cache.GetStringAsync(key);

            if (serializedValue == null)
            {
                return default(T); // Không tìm thấy dữ liệu trong cache
            }

            // Deserialize chuỗi JSON về đối tượng
            return JsonSerializer.Deserialize<T>(serializedValue);
        }

        public async Task RemoveAsync(string key)
        {
            // Xóa dữ liệu khỏi Redis
            await _cache.RemoveAsync(key);
        }
    }
}
