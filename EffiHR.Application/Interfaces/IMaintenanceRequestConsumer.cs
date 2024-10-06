using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EffiHR.Application.Interfaces
{
    public interface IMaintenanceRequestConsumer
    {
        Task ConsumeAsync(string rabbitMqHost);
    }
}
