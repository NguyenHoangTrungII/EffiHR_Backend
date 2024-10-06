using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EffiHR.Core.Constants
{
    public static class StaticUserRoles
    {
        // Roles hiện tại
        public const string ADMIN = "ADMIN";                // Quản trị viên
        public const string LANDLORD = "LANDLORD";          // Chủ nhà
        public const string TENANT = "TENANT";              // Người thuê nhà
        public const string TECHNICIAN = "TECHNICIAN ";     // Kỹ thuật viên


        // Roles tương lai (khi mở rộng hệ thống)
        public const string MAINTENANCE_WORKER = "MAINTENANCE_WORKER"; // Nhân viên bảo trì
        public const string RENTAL_MANAGER = "RENTAL_MANAGER";         // Quản lý phòng trọ
        public const string ACCOUNTANT = "ACCOUNTANT";                 // Nhân viên kế toán
    }
}

