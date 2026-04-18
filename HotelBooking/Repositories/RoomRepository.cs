using HotelBooking.Data;
using HotelBooking.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Repositories;

public class RoomRepository : Repository<Room>, IRoomRepository
{
    public RoomRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Room>> GetAvailableRoomsAsync(DateTime checkIn, DateTime checkOut)
    {
        var overlappingReservations = await _context.Reservations
            .Where(r => r.Status != ReservationStatus.Cancelled &&
                       ((r.CheckInDate < checkOut && r.CheckOutDate > checkIn)))
            .Select(r => r.RoomId)
            .Distinct()
            .ToListAsync();

        return await _context.Rooms
            .Where(r => r.IsAvailable && !overlappingReservations.Contains(r.Id))
            .ToListAsync();
    }
}