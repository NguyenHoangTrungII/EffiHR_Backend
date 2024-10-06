using EffiHR.Application.Interfaces;
using EffiHR.Domain.Entities;
using EffiHR.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EffiHR.Application.Services
{
    public class RoomService : IRoomService
    {
        private readonly IGenericRepository _repository;
        private readonly IMultiProcess _multiProcess; // Thêm vào interface để sử dụng

        public RoomService(IGenericRepository repository, IMultiProcess multiProcess)
        {
            _repository = repository;
            _multiProcess = multiProcess; // Khởi tạo multiProcess
        }

        // Phương thức lấy danh sách phòng trống
        public async Task<IEnumerable<Room>> GetAvailableRoomsAsync()
        {
            var allRooms = await _repository.GetAllAsync<Room>().ToListAsync();
            var availableRooms = new ConcurrentBag<Room>();

            // Sử dụng MultiProcess để kiểm tra trạng thái phòng
            await _multiProcess.ExecuteHandler(1, allRooms.Count, async (skip, pageSize, threadIndex) =>
            {
                var roomsToCheck = allRooms.Skip(skip).Take(pageSize);

                foreach (var room in roomsToCheck)
                {
                    if (IsRoomAvailable(room))
                    {
                        availableRooms.Add(room);
                    }
                }
            });

            return availableRooms.ToList(); // Trả về danh sách phòng trống
        }

        // Kiểm tra trạng thái phòng
        private bool IsRoomAvailable(Room room)
        {
            // Kiểm tra trạng thái phòng (giả sử đã định nghĩa trạng thái)
            return room.Status == Room.RoomStatus.Available;
        }
    }
}
