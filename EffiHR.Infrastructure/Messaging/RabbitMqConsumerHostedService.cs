using EffiHR.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace EffiHR.Infrastructure.Messaging
{
    public class RabbitMqConsumerHostedService : IHostedService
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public RabbitMqConsumerHostedService(IConfiguration configuration, IServiceScopeFactory serviceScopeFactory)
        {
            _configuration = configuration;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var maintenanceRequestConsumer = scope.ServiceProvider.GetRequiredService<IMaintenanceRequestConsumer>();
                var rabbitMqHost = _configuration["RabbitMQ:HostName"];

                // Bắt đầu lắng nghe hàng đợi từ RabbitMQ
                await maintenanceRequestConsumer.ConsumeAsync(rabbitMqHost);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            // Cleanup nếu cần thiết
            return Task.CompletedTask;
        }
    }
}
