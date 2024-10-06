//using EffiHR.Core.DTOs.Maintenance;
//using Microsoft.Extensions.Logging;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Microsoft.Extensions.Hosting;
//using EffiHR.Application.Interfaces;


//namespace EffiHR.Application.Services
//{
//    public class MaintenanceBackgroundService : BackgroundService
//    {
//        private readonly IMaintenanceQueue _maintenanceQueue;
//        private readonly ILogger<MaintenanceBackgroundService> _logger;

//        public MaintenanceBackgroundService(IMaintenanceQueue maintenanceQueue, ILogger<MaintenanceBackgroundService> logger)
//        {
//            _maintenanceQueue = maintenanceQueue;
//            _logger = logger;
//        }

//        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//        {
//            _logger.LogInformation("Maintenance background service is starting.");

//            while (!stoppingToken.IsCancellationRequested)
//            {
//                var request = await _maintenanceQueue.DequeueAsync(stoppingToken);

//                if (request != null)
//                {
//                    try
//                    {
//                        await ProcessMaintenanceRequest(request);
//                    }
//                    catch (Exception ex)
//                    {
//                        _logger.LogError(ex, "Error occurred while processing maintenance request.");
//                    }
//                }

//                await Task.Delay(1000, stoppingToken);
//            }

//            _logger.LogInformation("Maintenance background service is stopping.");
//        }

//        private async Task ProcessMaintenanceRequest(MaintenanceRequestDTO request)
//        {
//            // Logic xử lý yêu cầu bảo trì
//            _logger.LogInformation($"Processing maintenance request for room {request.RoomId}");
//            await Task.CompletedTask;
//        }
//    }

//}
