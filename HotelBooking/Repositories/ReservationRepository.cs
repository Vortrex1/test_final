using HotelBooking.Data;
using HotelBooking.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Repositories;

public class ReservationRepository : Repository<Reservation>, IReservationRepository
{
    public ReservationRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Reservation>> GetReservationsByGuestIdAsync(int guestId)
    {
        return await _context.Reservations
            .Where(r => r.GuestId == guestId)
            .Include(r => r.Room)
            .Include(r => r.Guest)
            .ToListAsync();
    }

    public async Task<bool> HasOverlappingReservationAsync(int roomId, DateTime checkIn, DateTime checkOut, int? excludeReservationId = null)
    {
        var query = _context.Reservations
            .Where(r => r.RoomId == roomId &&
                       r.Status != ReservationStatus.Cancelled &&
                       ((r.CheckInDate < checkOut && r.CheckOutDate > checkIn)));

        if (excludeReservationId.HasValue)
        {
            query = query.Where(r => r.Id != excludeReservationId.Value);
        }

        return await query.AnyAsync();
    }
}