using EffiHR.Application.Data;
using EffiHR.Application.Interfaces;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using EffiHR.Core.DTOs.Maintenance;
using EffiHR.Domain.Models;

namespace EffiHR.Infrastructure.Services
{
    public class MaintenanceRequestConsumer : IMaintenanceRequestConsumer
    {
        private readonly ApplicationDbContext _context;

        public MaintenanceRequestConsumer(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task ConsumeAsync(string rabbitMqHost)
        {
            var factory = new ConnectionFactory() { HostName = rabbitMqHost }; // Hoặc config khác từ Cloud
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "maintenance_queue",
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += async (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var requestDto = JsonConvert.DeserializeObject<MaintenanceRequestDTO>(message);

                    // Thực hiện xử lý lưu vào database
                    var maintenanceRequest = new MaintenanceRequest
                    {
                        CustomerId = requestDto.CustomerId,
                        TechnicianId = null, // Kỹ thuật viên chưa được phân
                        Status = "Pending",
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.MaintenanceRequests.Add(maintenanceRequest);
                    await _context.SaveChangesAsync();
                };

                channel.BasicConsume(queue: "maintenance_queue",
                                     autoAck: true,
                                     consumer: consumer);

                // Có thể sleep hoặc chạy liên tục để lắng nghe queue
            }
        }
    }
}
