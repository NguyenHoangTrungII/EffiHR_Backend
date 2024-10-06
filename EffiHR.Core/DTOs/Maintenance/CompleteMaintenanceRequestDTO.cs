using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EffiHR.Core.DTOs.Maintenance
{
    public class CompleteMaintenanceRequestDTO
    {
        public Guid RequestId { get; set; } // ID của yêu cầu bảo trì
        public string TechnicianId { get; set; } // ID của kỹ thuật viên
    }

}
