using HotelBooking.Models;

namespace HotelBooking.Repositories;

public interface IRoomRepository : IRepository<Room>
{
    Task<IEnumerable<Room>> GetAvailableRoomsAsync(DateTime checkIn, DateTime checkOut);
}