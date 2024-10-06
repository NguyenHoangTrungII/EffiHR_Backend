using EffiHR.Application.Interfaces;
using EffiHR.Core.DTOs.Maintenance;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Text;
using System.Threading.Tasks;

namespace EffiHR.Infrastructure.Messaging
{
    public class RabbitMQProducer : IMaintenanceQueue, IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public RabbitMQProducer()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.QueueDeclare(queue: "maintenance_queue",
                                  durable: true,
                                  exclusive: false,
                                  autoDelete: false,
                                  arguments: null);
        }

        public async Task QueueMaintenanceRequestAsync(MaintenanceRequestDTO requestDto)
        {
            var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(requestDto));

            // Thực hiện publish với async/await để xử lý tốt hơn trong môi trường bất đồng bộ
            await Task.Run(() =>
            {
                _channel.BasicPublish(exchange: "",
                                      routingKey: "maintenance_queue",
                                      basicProperties: null,
                                      body: body);
            });
        }

        public async Task SendToQueueWithTTLAsync(string queueName, MaintenanceRequestDTO message)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            // Không khai báo lại queue ở đây, chỉ gửi message
            var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;

            // Gửi thông điệp vào hàng đợi mà không cần khai báo queue
            channel.BasicPublish(exchange: "maintenance_exchange",
                                 routingKey: "maintenance_request",
                                 basicProperties: properties,
                                 body: body);

            Console.WriteLine($"Message sent to queue {queueName}.");
        }


        public async Task SendToRetryQueueAsync(MaintenanceRequestDTO message)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            // Chỉ gửi message vào retry queue mà không khai báo lại queue
            var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

            // Gửi thông điệp vào hàng đợi retry
            channel.BasicPublish(exchange: "retry_exchange",
                                 routingKey: "maintenance_request",
                                 basicProperties: null,
                                 body: body);

            Console.WriteLine($"Message sent to retry queue.");
        }

        public async Task SendToCompletionQueueAsync(CompleteMaintenanceRequestDTO requestDto)
        {
            // Sử dụng một kết nối và kênh riêng để gửi thông điệp
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            // Gửi thông điệp lên completion_queue
            var completionMessage = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(requestDto));

            var properties = channel.CreateBasicProperties();
            properties.Persistent = true; // Nếu bạn muốn đảm bảo thông điệp không bị mất

            channel.BasicPublish(exchange: "",
                                 routingKey: "completion_queue",
                                 basicProperties: properties,
                                 body: completionMessage);

            Console.WriteLine("Completion message sent.");
        }





        // Implement IDisposable để giải phóng kết nối khi không dùng nữa
        public void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
        }
    }
}

