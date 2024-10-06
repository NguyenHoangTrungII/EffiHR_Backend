using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using EffiHR.Core.DTOs.Maintenance;
using EffiHR.Application.Services;
using Microsoft.Extensions.Logging;

namespace EffiHR.Infrastructure.Messaging
{
    public class RabbitMQConsumer : BackgroundService
    {
        private readonly ILogger<RabbitMQConsumer> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public RabbitMQConsumer(IServiceScopeFactory scopeFactory, ILogger<RabbitMQConsumer> logger)
        {
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        //protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        //{
        //    _logger.LogInformation("RabbitMQConsumer is starting.");

        //    try
        //    {
        //        var factory = new ConnectionFactory() { HostName = "localhost" };
        //        _logger.LogInformation("Connecting to RabbitMQ...");

        //        using var connection = factory.CreateConnection();
        //        using var channel = connection.CreateModel();
        //        _logger.LogInformation("RabbitMQ connection established.");

        //        // Declare exchange
        //        channel.ExchangeDeclare(exchange: "maintenance_exchange", type: ExchangeType.Direct);
        //        _logger.LogInformation("Exchange 'maintenance_exchange' declared.");

        //        // Declare queue and bind to exchange
        //        channel.QueueDeclare(queue: "maintenance_queue",
        //                             durable: true,
        //                             exclusive: false,
        //                             autoDelete: false,
        //                             arguments: null);
        //        _logger.LogInformation("Queue 'maintenance_queue' declared.");

        //        channel.QueueBind(queue: "maintenance_queue",
        //                          exchange: "maintenance_exchange",
        //                          routingKey: "maintenance_request");
        //        _logger.LogInformation("Queue bound to exchange with routing key 'maintenance_request'.");

        //        var consumer = new EventingBasicConsumer(channel);

        //        consumer.Received += async (model, ea) =>
        //        {
        //            var body = ea.Body.ToArray();
        //            var message = Encoding.UTF8.GetString(body);
        //            _logger.LogInformation("Received message: {Message}", message);

        //            try
        //            {
        //                var requestDto = JsonConvert.DeserializeObject<MaintenanceRequestDTO>(message);
        //                _logger.LogInformation("Deserialized message to DTO: {RequestDto}", requestDto);

        //                using (var scope = _scopeFactory.CreateScope())
        //                {
        //                    var maintenanceService = scope.ServiceProvider.GetRequiredService<MaintenanceService>();
        //                    await maintenanceService.AssignTechnicianFromQueueAsync(requestDto);
        //                }

        //                channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
        //                _logger.LogInformation("Message processed and acknowledged.");
        //            }
        //            catch (Exception ex)
        //            {
        //                _logger.LogError($"Error processing message: {ex.Message}. Message will be requeued.");
        //                // Nack the message to requeue it
        //                channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
        //            }
        //        };

        //        _logger.LogInformation("Registering consumer to listen for messages...");
        //        channel.BasicConsume(queue: "maintenance_queue", autoAck: false, consumer: consumer);
        //        _logger.LogInformation("Consumer is listening for messages.");

        //        // Keep the background service running
        //        await Task.Delay(Timeout.Infinite, stoppingToken);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError($"RabbitMQConsumer encountered an error: {ex.Message}");
        //    }
        //}



        //        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        //        {
        //            _logger.LogInformation("RabbitMQConsumer is starting.");

        //            try
        //            {
        //                var factory = new ConnectionFactory() { HostName = "localhost" };
        //                _logger.LogInformation("Connecting to RabbitMQ...");

        //                using var connection = factory.CreateConnection();
        //                using var channel = connection.CreateModel();
        //                _logger.LogInformation("RabbitMQ connection established.");

        //                // Declare exchange
        //                channel.ExchangeDeclare(exchange: "maintenance_exchange", type: ExchangeType.Direct);
        //                _logger.LogInformation("Exchange 'maintenance_exchange' declared.");

        //                // Declare maintenance_queue
        //                channel.QueueDeclare(queue: "maintenance_queue",
        //                                     durable: true,
        //                                     exclusive: false,
        //                                     autoDelete: false,
        //                                     arguments: null);
        //                _logger.LogInformation("Queue 'maintenance_queue' declared.");

        //                // Declare maintenance_queue_with_ttl with TTL
        //                var ttl = 60000; // TTL in milliseconds (ví dụ: 60 giây)
        //                var arguments = new Dictionary<string, object>
        //{
        //    { "x-message-ttl", ttl }, // Thiết lập TTL cho message
        //    { "x-dead-letter-exchange", "maintenance_exchange" }, // Đảm bảo giá trị này khớp với giá trị đã khai báo trước đó
        //    { "x-dead-letter-routing-key", "maintenance_request" } // Routing key cho DLX
        //};

        //                channel.QueueDeclare(queue: "maintenance_queue_with_ttl",
        //                                     durable: true,
        //                                     exclusive: false,
        //                                     autoDelete: false,
        //                                     arguments: arguments);
        //                _logger.LogInformation("Queue 'maintenance_queue_with_ttl' declared with TTL.");

        //                // Bind maintenance_queue to maintenance_exchange
        //                channel.QueueBind(queue: "maintenance_queue",
        //                                  exchange: "maintenance_exchange",
        //                                  routingKey: "maintenance_request");
        //                _logger.LogInformation("Queue bound to exchange with routing key 'maintenance_request'.");

        //                // Bind maintenance_queue_with_ttl to maintenance_exchange (nếu cần)
        //                channel.QueueBind(queue: "maintenance_queue_with_ttl",
        //                                  exchange: "maintenance_exchange",
        //                                  routingKey: "maintenance_request");
        //                _logger.LogInformation("Queue 'maintenance_queue_with_ttl' bound to exchange with routing key 'maintenance_request'.");

        //                var consumer = new EventingBasicConsumer(channel);

        //                consumer.Received += async (model, ea) =>
        //                {
        //                    var body = ea.Body.ToArray();
        //                    var message = Encoding.UTF8.GetString(body);
        //                    _logger.LogInformation("Received message: {Message}", message);

        //                    try
        //                    {
        //                        var requestDto = JsonConvert.DeserializeObject<MaintenanceRequestDTO>(message);
        //                        _logger.LogInformation("Deserialized message to DTO: {RequestDto}", requestDto);

        //                        using (var scope = _scopeFactory.CreateScope())
        //                        {
        //                            var maintenanceService = scope.ServiceProvider.GetRequiredService<MaintenanceService>();
        //                            await maintenanceService.AssignTechnicianFromQueueAsync(requestDto);
        //                        }

        //                        channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
        //                        _logger.LogInformation("Message processed and acknowledged.");
        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        _logger.LogError($"Error processing message: {ex.Message}. Message will be requeued.");
        //                        // Nack the message to requeue it
        //                        channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
        //                    }
        //                };

        //                _logger.LogInformation("Registering consumer to listen for messages...");
        //                channel.BasicConsume(queue: "maintenance_queue", autoAck: false, consumer: consumer);
        //                _logger.LogInformation("Consumer is listening for messages.");

        //                // Keep the background service running
        //                await Task.Delay(Timeout.Infinite, stoppingToken);
        //            }
        //            catch (Exception ex)
        //            {
        //                _logger.LogError($"RabbitMQConsumer encountered an error: {ex.Message}");
        //            }
        //        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("RabbitMQConsumer is starting.");

            try
            {
                var factory = new ConnectionFactory() { HostName = "localhost" };
                _logger.LogInformation("Connecting to RabbitMQ...");

                using var connection = factory.CreateConnection();
                using var channel = connection.CreateModel();
                _logger.LogInformation("RabbitMQ connection established.");

                // Declare exchanges and queues
                channel.ExchangeDeclare(exchange: "maintenance_exchange", type: ExchangeType.Direct);
                channel.ExchangeDeclare(exchange: "retry_exchange", type: ExchangeType.Direct);

                // Declare main queue
                channel.QueueDeclare(queue: "maintenance_queue", durable: true, exclusive: false, autoDelete: false);
                _logger.LogInformation("Queue 'maintenance_queue' declared.");

                // Declare retry queue with TTL
                var ttl = 1800000; // TTL in milliseconds
                var arguments = new Dictionary<string, object>
        {
            { "x-message-ttl", ttl }, // Set TTL for messages
            { "x-dead-letter-exchange", "maintenance_exchange" }, // Set dead-letter exchange
            { "x-dead-letter-routing-key", "maintenance_request" } // Routing key for DLX
        };
                channel.QueueDeclare(queue: "retry_queue", durable: true, exclusive: false, autoDelete: false, arguments: arguments);
                _logger.LogInformation("Queue 'retry_queue' declared with TTL.");

                // Bind queues to exchanges
                channel.QueueBind(queue: "maintenance_queue", exchange: "maintenance_exchange", routingKey: "maintenance_request");
                _logger.LogInformation("Queue bound to exchange 'maintenance_exchange'.");

                channel.QueueBind(queue: "retry_queue", exchange: "retry_exchange", routingKey: "maintenance_request");
                _logger.LogInformation("Queue bound to exchange 'retry_exchange'.");

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += async (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    _logger.LogInformation("Received message: {Message}", message);

                    try
                    {
                        var requestDto = JsonConvert.DeserializeObject<MaintenanceRequestDTO>(message);
                        _logger.LogInformation("Deserialized message to DTO: {RequestDto}", requestDto);

                        using (var scope = _scopeFactory.CreateScope())
                        {
                            var maintenanceService = scope.ServiceProvider.GetRequiredService<MaintenanceService>();
                            await maintenanceService.AssignTechnicianFromQueueAsync(requestDto);
                        }

                        channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                        _logger.LogInformation("Message processed and acknowledged.");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error processing message: {ex.Message}. Message will be sent to retry queue.");
                        // Publish to retry queue
                        var bodyToRetry = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
                        channel.BasicPublish(exchange: "retry_exchange", routingKey: "maintenance_request", basicProperties: null, body: bodyToRetry);

                        // Nack the message to not requeue
                        channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: false);
                    }
                };

                _logger.LogInformation("Registering consumer to listen for messages...");
                channel.BasicConsume(queue: "maintenance_queue", autoAck: false, consumer: consumer);
                _logger.LogInformation("Consumer is listening for messages.");


                // Consumer for CompletionQueue (lắng nghe sự kiện hoàn thành)
                // Declare completion queue
                channel.QueueDeclare(queue: "completion_queue", durable: true, exclusive: false, autoDelete: false);
                _logger.LogInformation("Queue 'completion_queue' declared.");

                var completionConsumer = new EventingBasicConsumer(channel);
                completionConsumer.Received += async (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    //if (message == "TechnicianAvailable")
                    //{
                        _logger.LogInformation("Technician available, checking retry_queue...");
                        await MoveRequestsFromRetryToMaintenanceQueueAsync(channel);
                    //}
                };

                channel.BasicConsume(queue: "completion_queue", autoAck: true, consumer: completionConsumer);




                // Keep the background service running
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError($"RabbitMQConsumer encountered an error: {ex.Message}");
            }
        }

        public async Task MoveRequestsFromRetryToMaintenanceQueueAsync(IModel channel)
        {
            var result = channel.BasicGet(queue: "retry_queue", autoAck: false);

            if (result != null)
            {
                try
                {
                    // Publish message vào maintenance_queue
                    channel.BasicPublish(
                        exchange: "maintenance_exchange",
                        routingKey: "maintenance_request",
                        basicProperties: result.BasicProperties,
                        body: result.Body.ToArray()
                    );

                    _logger.LogInformation("Request moved from retry_queue to maintenance_queue.");

                    // Acknowledge message nếu publish thành công
                    channel.BasicAck(deliveryTag: result.DeliveryTag, multiple: false);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to move message from retry_queue to maintenance_queue: {ex.Message}");

                    // Nếu có lỗi, không acknowledge message, để nó được xử lý lại
                }
            }
            else
            {
                _logger.LogInformation("No message available in retry_queue.");
            }

        }



    }
}





//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Hosting;
//using RabbitMQ.Client.Events;
//using RabbitMQ.Client;
//using System;
//using System.Collections.Generic;
//using System.Text;
//using System.Threading.Tasks;
//using Newtonsoft.Json;
//using EffiHR.Core.DTOs.Maintenance;
//using EffiHR.Application.Services;
//using Microsoft.Extensions.Logging;

//namespace EffiHR.Infrastructure.Messaging
//{
//    public class RabbitMQConsumer : BackgroundService
//    {
//        private readonly ILogger<RabbitMQConsumer> _logger;
//        private readonly IServiceScopeFactory _scopeFactory;

//        public RabbitMQConsumer(IServiceScopeFactory scopeFactory, ILogger<RabbitMQConsumer> logger)
//        {
//            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
//            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
//        }

//        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//        {
//            _logger.LogInformation("RabbitMQConsumer is starting.");

//            try
//            {
//                var factory = new ConnectionFactory() { HostName = "localhost" };
//                _logger.LogInformation("Connecting to RabbitMQ...");

//                using var connection = factory.CreateConnection();
//                using var channel = connection.CreateModel();
//                _logger.LogInformation("RabbitMQ connection established.");

//                // Declare exchange for maintenance requests
//                channel.ExchangeDeclare(exchange: "maintenance_exchange", type: ExchangeType.Direct);
//                _logger.LogInformation("Exchange 'maintenance_exchange' declared.");

//                // Declare main queue with TTL and DLX
//                channel.QueueDeclare(queue: "maintenance_queue_with_ttl", // Tạo hàng đợi mới với TTL
//                      durable: true,
//                      exclusive: false,
//                      autoDelete: false,
//                      arguments: new Dictionary<string, object>
//                      {
//                         { "x-message-ttl", 60000 }, // TTL là 60 giây
//                         { "x-dead-letter-exchange", "dlx_exchange" }
//                      });

//                channel.QueueBind(queue: "maintenance_queue_with_ttl",
//                                  exchange: "maintenance_exchange",
//                                  routingKey: "maintenance_request_with_ttl");
//                _logger.LogInformation("Queue bound to exchange with routing key 'maintenance_request'.");

//                // Declare Dead Letter Exchange (DLX) and queue
//                channel.ExchangeDeclare(exchange: "dlx_exchange", type: ExchangeType.Direct);
//                channel.QueueDeclare(queue: "dlx_queue",
//                                     durable: true,
//                                     exclusive: false,
//                                     autoDelete: false,
//                                     arguments: null);
//                channel.QueueBind(queue: "dlx_queue", exchange: "dlx_exchange", routingKey: "maintenance_dlx");
//                _logger.LogInformation("Dead Letter Exchange and queue declared.");

//                var consumer = new EventingBasicConsumer(channel);

//                consumer.Received += async (model, ea) =>
//                {
//                    var body = ea.Body.ToArray();
//                    var message = Encoding.UTF8.GetString(body);
//                    _logger.LogInformation("Received message: {Message}", message);

//                    try
//                    {
//                        var requestDto = JsonConvert.DeserializeObject<MaintenanceRequestDTO>(message);
//                        _logger.LogInformation("Deserialized message to DTO: {RequestDto}", requestDto);

//                        using (var scope = _scopeFactory.CreateScope())
//                        {
//                            var maintenanceService = scope.ServiceProvider.GetRequiredService<MaintenanceService>();
//                            await maintenanceService.AssignTechnicianFromQueueAsync(requestDto);
//                        }

//                        channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
//                        _logger.LogInformation("Message processed and acknowledged.");
//                    }
//                    catch (Exception ex)
//                    {
//                        _logger.LogError($"Error processing message: {ex.Message}. Message will be requeued.");
//                        // Nack the message to requeue it
//                        channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
//                    }
//                };

//                _logger.LogInformation("Registering consumer to listen for messages...");
//                channel.BasicConsume(queue: "maintenance_queue_with_ttl", autoAck: false, consumer: consumer);
//                _logger.LogInformation("Consumer is listening for messages.");

//                // DLX consumer to retry messages
//                var dlxConsumer = new EventingBasicConsumer(channel);
//                dlxConsumer.Received += (model, ea) =>
//                {
//                    var body = ea.Body.ToArray();
//                    var message = Encoding.UTF8.GetString(body);
//                    _logger.LogInformation("Retrying message from DLX: {Message}", message);

//                    // Publish the message back to the main queue
//                    channel.BasicPublish(exchange: "maintenance_exchange", routingKey: "maintenance_request", basicProperties: null, body: body);

//                    // Acknowledge that the message has been processed in DLX
//                    channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
//                };

//                // Register the DLX consumer
//                channel.BasicConsume(queue: "dlx_queue", autoAck: false, consumer: dlxConsumer);
//                _logger.LogInformation("DLX consumer is listening for messages.");

//                // Keep the background service running
//                await Task.Delay(Timeout.Infinite, stoppingToken);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError($"RabbitMQConsumer encountered an error: {ex.Message}");
//            }
//        }
//    }
//}

