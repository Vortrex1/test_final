using HotelBooking.Models;
using HotelBooking.Repositories;

namespace HotelBooking.Services;

public class RoomService
{
    private readonly IRoomRepository _roomRepository;

    public RoomService(IRoomRepository roomRepository)
    {
        _roomRepository = roomRepository;
    }

    public async Task<IEnumerable<Room>> GetAllRoomsAsync()
    {
        return await _roomRepository.GetAllAsync();
    }

    public async Task<Room?> GetRoomByIdAsync(int id)
    {
        return await _roomRepository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Room>> GetAvailableRoomsAsync(DateTime checkIn, DateTime checkOut)
    {
        if (checkIn.Kind == DateTimeKind.Unspecified)
        {
            checkIn = DateTime.SpecifyKind(checkIn, DateTimeKind.Utc);
        }

        if (checkOut.Kind == DateTimeKind.Unspecified)
        {
            checkOut = DateTime.SpecifyKind(checkOut, DateTimeKind.Utc);
        }

        return await _roomRepository.GetAvailableRoomsAsync(checkIn, checkOut);
    }

    public async Task AddRoomAsync(Room room)
    {
        await _roomRepository.AddAsync(room);
    }

    public async Task UpdateRoomAsync(Room room)
    {
        await _roomRepository.UpdateAsync(room);
    }

    public async Task DeleteRoomAsync(int id)
    {
        await _roomRepository.DeleteAsync(id);
    }
}