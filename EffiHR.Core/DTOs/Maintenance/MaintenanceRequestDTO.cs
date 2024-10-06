using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EffiHR.Core.DTOs.Maintenance
{
    public class MaintenanceRequestDTO
    {
        public Guid RoomId { get; set; }
        public string Description { get; set; }
        public string? TechnicianId { get; set; }
        public string CustomerId { get; set; }
        public Guid CategoryId { get; set; }
        public int PriorityLevel { get; set; }
        public string Status { get; set; }

        public DateTime CreatedAt { get; set; }
        public string CustomerFeedback { get; set; }
        
    }

}
