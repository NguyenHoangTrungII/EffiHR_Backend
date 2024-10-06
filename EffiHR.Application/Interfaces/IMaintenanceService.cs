using EffiHR.Application.Wrappers;
using EffiHR.Core.DTOs.Maintenance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EffiHR.Application.Interfaces
{
    public interface IMaintenanceService
    {
        Task<ApiResponse<MaintenanceRequestDTO>> AssignTechnicianAsync(MaintenanceRequestDTO request);

        Task<ApiResponse<CompleteMaintenanceRequestDTO>> CompleteMaintenanceAsync(CompleteMaintenanceRequestDTO requestDto);

        Task AssignTechnicianFromQueueAsync(MaintenanceRequestDTO requestDto);

        Task<ApiResponse<MaintenanceRequestDTO>> QueueMaintenanceRequestAsync(MaintenanceRequestDTO requestDto);





    }

}
