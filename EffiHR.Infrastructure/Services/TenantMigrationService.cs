using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using EffiHR.Application.Data;

namespace EffiHR.Application.Services
{
    public class TenantMigrationService
    {
        private readonly List<string> _tenantConnectionStrings;

        public TenantMigrationService(List<string> tenantConnectionStrings)
        {
            _tenantConnectionStrings = tenantConnectionStrings;
        }

        public void MigrateDatabases()
        {
            foreach (var connectionString in _tenantConnectionStrings)
            {
                // Sử dụng DbContextOptionsBuilder để tạo DbContextOptions từ chuỗi kết nối
                var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
                optionsBuilder.UseSqlServer(connectionString);  // Giả định bạn đang dùng SQL Server

                using (var context = new ApplicationDbContext(optionsBuilder.Options))
                {
                    context.Database.Migrate();  // Áp dụng migration cho từng tenant
                }
            }
        }
    }
}
