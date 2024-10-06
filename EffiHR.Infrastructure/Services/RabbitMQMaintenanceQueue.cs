//using EffiHR.Core.DTOs.Maintenance;
//using EffiHR.Application.Interfaces;
//using RabbitMQ.Client;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Text.Json;

//namespace EffiHR.Infrastructure.Services
//{
//    public class RabbitMQMaintenanceQueue : IMaintenanceQueue
//    {
//        private readonly string _hostname;
//        private readonly string _queueName;

//        public RabbitMQMaintenanceQueue(string hostname, string queueName)
//        {
//            _hostname = hostname;
//            _queueName = queueName;
//        }

//        public async Task QueueMaintenanceRequestAsync(MaintenanceRequestDTO request)
//        {
//            var factory = new ConnectionFactory() { HostName = _hostname };
//            using (var connection = factory.CreateConnection())
//            using (var channel = connection.CreateModel())
//            {
//                channel.QueueDeclare(queue: _queueName,
//                                     durable: false,
//                                     exclusive: false,
//                                     autoDelete: false,
//                                     arguments: null);

//                var message = JsonSerializer.Serialize(request);
//                var body = Encoding.UTF8.GetBytes(message);

//                channel.BasicPublish(exchange: "",
//                                     routingKey: _queueName,
//                                     basicProperties: null,
//                                     body: body);

//                Console.WriteLine(" [x] Sent {0}", message);
//            }
//        }
//    }
//}
