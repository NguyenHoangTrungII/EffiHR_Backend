using EffiHR.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EffiHR.Application.Interfaces
{
    public interface IRoomService
    {
        Task<IEnumerable<Room>> GetAvailableRoomsAsync(); // Thêm phương thức này

        //Task<IEnumerable<Room>> GetAllRoomsAsync(); // Lấy tất cả phòng
        //Task<Room> GetRoomByIdAsync(Guid id); // Lấy phòng theo ID
        //Task<Room> CreateRoomAsync(Room room); // Thêm phòng mới
        //Task<Room> UpdateRoomAsync(Room room); // Cập nhật phòng
        //Task<bool> DeleteRoomAsync(Guid id); // Xóa phòng
    }
}
