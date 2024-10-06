using EffiHR.Application.Data;
using EffiHR.Application.Interfaces;
using EffiHR.Application.Wrappers;
using EffiHR.Core.DTOs.Maintenance;
using EffiHR.Domain.Entities;
using EffiHR.Domain.Models;
using EffiHR.Infrastructure.Messaging;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace EffiHR.Application.Services
{
    public class MaintenanceService : IMaintenanceService
    {
        //private readonly RabbitMQProducer _rabbitMQProducer;
        private readonly IModel _channel;
        private readonly IMaintenanceQueue _maintenanceQueue;
        private readonly ApplicationDbContext _context;
        private int lastAssignedTechnicianIndex = -1;  // Lưu trữ vị trí kỹ thuật viên cuối cùng đã được phân công

        public MaintenanceService(IMaintenanceQueue maintenanceQueue, ApplicationDbContext context, RabbitMQProducer rabbitMQProducer, IModel channel = null)
        {
            _maintenanceQueue = maintenanceQueue;
            _context = context;
            _channel = channel;
        }

        public async Task<ApiResponse<MaintenanceRequestDTO>> AssignTechnicianAsync(MaintenanceRequestDTO requestDto)
        {
            

            // Get the IDs of technicians from the UserRoles table
            var technicianRoleId = await _context.Roles
                .Where(r => r.Name == "Technician")
                .Select(r => r.Id)
                .FirstOrDefaultAsync();

            if (technicianRoleId == null)
            {
                // Handle case where technician role does not exist
                throw new Exception("Technician role does not exist.");
            }

            // Fetch active technicians who are associated with the "Technician" role
            var technicians = await _context.UserRoles
                .Where(ur => ur.RoleId == technicianRoleId) // Lọc theo vai trò là "Technician"
                .Select(ur => ur.UserId) // Lấy UserId
                .Join(_context.Users.Where(u => u.LockoutEnabled), // Kết nối với bảng Users
                      ur => ur,
                      u => u.Id,
                      (ur, u) => new { u.Id, u.UserName }) // Chọn thông tin cần thiết
                .Where(t => !_context.MaintenanceRequests
                    .Any(req => req.TechnicianId == t.Id && req.Status != "Completed")) // Lọc kỹ thuật viên không có yêu cầu nào chưa hoàn thành
                .OrderBy(t => t.Id) // Sắp xếp theo Id
                .ToListAsync(); // Chuyển đổi sang danh sách không đồng bộ


            if (technicians?.Count == 0 )
            {
                // Không có kỹ thuật viên rảnh, đưa vào hàng đợi
                await _maintenanceQueue.QueueMaintenanceRequestAsync(requestDto);
                return new ApiResponse<MaintenanceRequestDTO>("No available technicians; request has been queued.")
                {
                    Succeeded = false,
                    Data = requestDto // Trả về dữ liệu gốc nếu cần
                };
            }

            // Kiểm tra kỹ thuật viên từ vị trí cuối cùng đã phân công
            for (int i = 0; i < technicians.Count; i++)
            {
                lastAssignedTechnicianIndex = (lastAssignedTechnicianIndex + 1) % technicians.Count;
                var technicianToAssign = technicians[lastAssignedTechnicianIndex];

                // Kiểm tra nếu kỹ thuật viên này có bất kỳ nhiệm vụ nào chưa hoàn thành
                bool hasPendingTasks = await _context.MaintenanceRequests
                    .AnyAsync(req => req.TechnicianId == technicianToAssign.Id && req.Status != "Completed");

                if (!hasPendingTasks )
                {
                    //maintenanceRequestEntity.TechnicianId = technicianToAssign.Id;
                    //maintenanceRequestEntity.Status = "In Progress";
                    var request = new MaintenanceRequest
                    {
                        CustomerId = requestDto.CustomerId,
                        TechnicianId = technicianToAssign.Id,
                        Status = requestDto.Status,
                        PriorityLevel = requestDto.PriorityLevel,
                        CreatedAt = requestDto.CreatedAt,
                        CustomerFeedback = requestDto.CustomerFeedback,
                        CategoryId = requestDto.CategoryId,
                        RoomId = requestDto.RoomId,
                    };

                    _context.MaintenanceRequests.Add(request);
                    await _context.SaveChangesAsync();
                    return new ApiResponse<MaintenanceRequestDTO>(requestDto, "Technician assigned successfully.");
                }
            }

            // Nếu không tìm thấy kỹ thuật viên nào rảnh, đưa vào hàng đợi
            
            await _maintenanceQueue.QueueMaintenanceRequestAsync(requestDto);

            return new ApiResponse<MaintenanceRequestDTO>("All technicians are busy; request has been queued.")
            {
                Succeeded = false,
                Data = requestDto // Trả về dữ liệu gốc nếu cần
            };
        }

        public async Task<ApiResponse<CompleteMaintenanceRequestDTO>> CompleteMaintenanceAsync(CompleteMaintenanceRequestDTO requestDto)
        {
            // Kiểm tra yêu cầu
            var request = await _context.MaintenanceRequests.FindAsync(requestDto.RequestId);
            if (request == null)
            {
                return new ApiResponse<CompleteMaintenanceRequestDTO>("Maintenance request not found.");
            }

            // Gán hoàn thành cho yêu cầu
            request.Status = "Completed"; // Cập nhật trạng thái
            request.TechnicianId = requestDto.TechnicianId; // Gán kỹ thuật viên

            //try
            //{
            //    await _context.SaveChangesAsync(); // Lưu thay đổi
            //    return new ApiResponse<CompleteMaintenanceRequestDTO>(requestDto, "Maintenance request completed successfully.");


            //}
            //catch (Exception ex)
            //{
            //    return new ApiResponse<CompleteMaintenanceRequestDTO>($"Error completing request: {ex.Message}");
            //}

            try
            {



                // Gọi hàm để gửi thông điệp tới completion_queue
                await _maintenanceQueue.SendToCompletionQueueAsync(requestDto);

                await _context.SaveChangesAsync(); // Lưu thay đổi


                return new ApiResponse<CompleteMaintenanceRequestDTO>(requestDto, "Maintenance request completed successfully.");
            }
            catch (Exception ex)
            {
                return new ApiResponse<CompleteMaintenanceRequestDTO>($"Error completing request: {ex.Message}");
            }
        }


        public async Task<ApiResponse<MaintenanceRequestDTO>> QueueMaintenanceRequestAsync(MaintenanceRequestDTO requestDto)
        {
            // Đưa yêu cầu vào hàng đợi RabbitMQ
            await _maintenanceQueue.QueueMaintenanceRequestAsync(requestDto);

            return new ApiResponse<MaintenanceRequestDTO>("Request has been queued for technician assignment.")
            {
                Succeeded = true,
                Data = requestDto
            };
        }


        //public async Task AssignTechnicianFromQueueAsync(MaintenanceRequestDTO requestDto)
        //{
        //    // Get the IDs of technicians from the UserRoles table
        //    var technicianRoleId = await _context.Roles
        //        .Where(r => r.Name == "Technician")
        //        .Select(r => r.Id)
        //        .FirstOrDefaultAsync();

        //    if (technicianRoleId == null)
        //    {
        //        throw new Exception("Technician role does not exist.");
        //    }

        //    // Fetch active technicians who are associated with the "Technician" role
        //    var technicians = await _context.UserRoles
        //        .Where(ur => ur.RoleId == technicianRoleId)
        //        .Select(ur => ur.UserId)
        //        .Join(_context.Users.Where(u => u.LockoutEnabled),
        //              ur => ur,
        //              u => u.Id,
        //              (ur, u) => new { u.Id, u.UserName })
        //        .Where(t => !_context.MaintenanceRequests
        //            .Any(req => req.TechnicianId == t.Id && req.Status != "Completed"))
        //        .OrderBy(t => t.Id)
        //        .ToListAsync();

        //    if (technicians.Count == 0)
        //    {
        //        // Nếu không có kỹ thuật viên rảnh, có thể xử lý theo cách khác hoặc giữ lại trong hàng đợi
        //        //Console.WriteLine("No available technicians.");
        //        //return;

        //        await _maintenanceQueue.SendToQueueWithTTLAsync("maintenance_queue_with_ttl", requestDto);

        //        return;


        //    }

        //    // Kiểm tra kỹ thuật viên từ vị trí cuối cùng đã phân công
        //    for (int i = 0; i < technicians.Count; i++)
        //    {
        //        lastAssignedTechnicianIndex = (lastAssignedTechnicianIndex + 1) % technicians.Count;
        //        var technicianToAssign = technicians[lastAssignedTechnicianIndex];

        //        // Kiểm tra nếu kỹ thuật viên này có bất kỳ nhiệm vụ nào chưa hoàn thành
        //        bool hasPendingTasks = await _context.MaintenanceRequests
        //            .AnyAsync(req => req.TechnicianId == technicianToAssign.Id && req.Status != "Completed");

        //        if (!hasPendingTasks)
        //        {
        //            // Gán kỹ thuật viên cho yêu cầu và cập nhật vào database
        //            var request = new MaintenanceRequest
        //            {
        //                CustomerId = requestDto.CustomerId,
        //                TechnicianId = technicianToAssign.Id,
        //                Status = "In Progress",
        //                PriorityLevel = requestDto.PriorityLevel,
        //                CreatedAt = requestDto.CreatedAt,
        //                CustomerFeedback = requestDto.CustomerFeedback,
        //                CategoryId = requestDto.CategoryId,
        //                RoomId = requestDto.RoomId,
        //            };

        //            _context.MaintenanceRequests.Add(request);
        //            await _context.SaveChangesAsync();

        //            Console.WriteLine($"Technician {technicianToAssign.UserName} assigned to request.");
        //            return;
        //        }
        //    }

        //    // Nếu không tìm thấy kỹ thuật viên nào rảnh, yêu cầu có thể vẫn nằm trong hàng đợi
        //    Console.WriteLine("All technicians are busy.");
        //}


        public async Task AssignTechnicianFromQueueAsync(MaintenanceRequestDTO requestDto)
        {
            // Get the IDs of technicians from the UserRoles table
            var technicianRoleId = await _context.Roles
                .Where(r => r.Name == "Technician")
                .Select(r => r.Id)
                .FirstOrDefaultAsync();

            if (technicianRoleId == null)
            {
                throw new Exception("Technician role does not exist.");
            }

            // Fetch active technicians who are associated with the "Technician" role
            var technicians = await _context.UserRoles
                .Where(ur => ur.RoleId == technicianRoleId)
                .Select(ur => ur.UserId)
                .Join(_context.Users.Where(u => u.LockoutEnabled),
                      ur => ur,
                      u => u.Id,
                      (ur, u) => new { u.Id, u.UserName })
                .Where(t => !_context.MaintenanceRequests
                    .Any(req => req.TechnicianId == t.Id && req.Status != "Completed"))
                .OrderBy(t => t.Id)
                .ToListAsync();

            if (technicians.Count == 0)
            {
                // Nếu không có kỹ thuật viên rảnh, gửi yêu cầu đến retry queue
                //_logger.LogInformation("No available technicians. Sending request to retry queue.");
                await _maintenanceQueue.SendToRetryQueueAsync(requestDto);
                return;
            }

            // Kiểm tra kỹ thuật viên từ vị trí cuối cùng đã phân công
            for (int i = 0; i < technicians.Count; i++)
            {
                lastAssignedTechnicianIndex = (lastAssignedTechnicianIndex + 1) % technicians.Count;
                var technicianToAssign = technicians[lastAssignedTechnicianIndex];

                // Kiểm tra nếu kỹ thuật viên này có bất kỳ nhiệm vụ nào chưa hoàn thành
                bool hasPendingTasks = await _context.MaintenanceRequests
                    .AnyAsync(req => req.TechnicianId == technicianToAssign.Id && req.Status != "Completed");

                if (!hasPendingTasks)
                {
                    // Gán kỹ thuật viên cho yêu cầu và cập nhật vào database
                    var request = new MaintenanceRequest
                    {
                        CustomerId = requestDto.CustomerId,
                        TechnicianId = technicianToAssign.Id,
                        Status = "In Progress",
                        PriorityLevel = requestDto.PriorityLevel,
                        CreatedAt = requestDto.CreatedAt,
                        CustomerFeedback = requestDto.CustomerFeedback,
                        CategoryId = requestDto.CategoryId,
                        RoomId = requestDto.RoomId,
                    };

                    _context.MaintenanceRequests.Add(request);
                    await _context.SaveChangesAsync();

                    Console.WriteLine($"Technician {technicianToAssign.UserName} assigned to request.");
                    return;
                }
            }

            // Nếu không tìm thấy kỹ thuật viên nào rảnh, yêu cầu sẽ được chuyển đến retry queue
            Console.WriteLine("All technicians are busy.");
        }



    }


}
