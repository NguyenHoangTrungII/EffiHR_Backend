//using EffiHR.Application.Interfaces;
//using EffiHR.Core.DTOs.Maintenance;
//using EffiHR.Domain.Models;
//using System.Threading.Channels;

//public class MaintenanceQueue : IMaintenanceQueue
//{
//    private readonly Channel<MaintenanceRequestDTO> _queue;

//    public MaintenanceQueue()
//    {
//        _queue = Channel.CreateUnbounded<MaintenanceRequestDTO>();
//    }

//    public async Task QueueMaintenanceRequestAsync(MaintenanceRequestDTO request)
//    {
//        await _queue.Writer.WriteAsync(request);
//    }

//    public async Task<MaintenanceRequestDTO> DequeueAsync(CancellationToken cancellationToken)
//    {
//        var request = await _queue.Reader.ReadAsync(cancellationToken);
//        return request;
//    }
//}