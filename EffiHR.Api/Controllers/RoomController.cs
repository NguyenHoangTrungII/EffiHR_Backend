using EffiHR.Application.Interfaces;
using EffiHR.Application.Services;
using EffiHR.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace EffiHR.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoomController : Controller
    {

        private readonly IRoomService _roomService;

        public RoomController(IRoomService roomService)
        {
            _roomService = roomService;
        }



        // GET: api/Room/available
        [HttpGet("available")]
        public async Task<ActionResult<IEnumerable<Room>>> GetAvailableRooms()
        {
            var availableRooms = await _roomService.GetAvailableRoomsAsync();
            return Ok(availableRooms);
        }

    }
}
