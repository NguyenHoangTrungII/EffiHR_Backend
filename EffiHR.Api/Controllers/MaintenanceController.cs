using EffiHR.Application.Interfaces;
using EffiHR.Application.Services;
using EffiHR.Core.DTOs.Maintenance;
using Microsoft.AspNetCore.Mvc;

namespace EffiHR.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MaintenanceController : ControllerBase
    {
        private readonly IMaintenanceQueue _maintenanceQueue;
        private readonly IMaintenanceService _maintenanceService;


        public MaintenanceController(IMaintenanceQueue maintenanceQueue, IMaintenanceService maintenanceService)
        { 
            _maintenanceQueue = maintenanceQueue;
            _maintenanceService = maintenanceService;
        }

        [HttpPost("request-maintenance")]
        public async Task<IActionResult> RequestMaintenance([FromBody] MaintenanceRequestDTO request)
        {
            await _maintenanceQueue.QueueMaintenanceRequestAsync(request);
            return Ok("Yêu cầu bảo trì đã được nhận và đang xử lý.");
        }

        [HttpPost("assign-technician")]
        public async Task<IActionResult> AssignTechnician([FromBody] MaintenanceRequestDTO requestDto)
        {
            if (requestDto == null)
            {
                return BadRequest("Invalid request data.");
            }

            try
            {
                // Gọi phương thức AssignTechnicianAsync và nhận phản hồi
                //var response = await _maintenanceService.AssignTechnicianAsync(requestDto);
                var response = await _maintenanceService.QueueMaintenanceRequestAsync(requestDto);

                // Kiểm tra xem yêu cầu có thành công không
                if (response.Succeeded)
                {
                    return Ok(response);
                }

                // Trả về thông báo không thành công nếu không gán được kỹ thuật viên
                return BadRequest(response.Message);
            }
            catch (Exception ex)
            {
                // Log the exception (optional)
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("complete-maintenance")]
        public async Task<IActionResult> CompleteMaintenance([FromBody] CompleteMaintenanceRequestDTO requestDto)
        {
            if (requestDto == null)
            {
                return BadRequest("Invalid request data.");
            }

            try
            {
                var response = await _maintenanceService.CompleteMaintenanceAsync(requestDto);
                if (response.Succeeded)
                {
                    return Ok(response);
                }

                return BadRequest(response.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


    }
}
