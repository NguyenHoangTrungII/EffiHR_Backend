using EffiHR.Core.DTOs.Maintenance;
using EffiHR.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EffiHR.Application.Interfaces
{
    public interface IMaintenanceQueue
    {
        Task QueueMaintenanceRequestAsync(MaintenanceRequestDTO request);
        Task SendToQueueWithTTLAsync(string queuename, MaintenanceRequestDTO request);

        Task SendToRetryQueueAsync(MaintenanceRequestDTO message);

        Task SendToCompletionQueueAsync(CompleteMaintenanceRequestDTO requestDto);


    }
}
