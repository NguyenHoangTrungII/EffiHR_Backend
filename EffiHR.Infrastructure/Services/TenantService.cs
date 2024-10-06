using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace EffiHR.Infrastructure.Services
{
    public class TenantService
    {
        private readonly Dictionary<string, string> _tenantConnectionStrings;

        public TenantService(IConfiguration configuration)
        {
            // Lấy chuỗi kết nối của tenant từ file cấu hình
            _tenantConnectionStrings = configuration.GetSection("TenantConnectionStrings").Get<Dictionary<string, string>>();
        }

        public string GetConnectionString(string tenantId)
        {
            if (_tenantConnectionStrings.TryGetValue(tenantId, out var connectionString))
            {
                return connectionString;
            }
            throw new Exception("Tenant not found");
        }

        public bool HasTenant(string tenantId)
        {
            return _tenantConnectionStrings.ContainsKey(tenantId);
        }
    }
}
