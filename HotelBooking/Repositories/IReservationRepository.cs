using HotelBooking.Models;

namespace HotelBooking.Repositories;

public interface IReservationRepository : IRepository<Reservation>
{
    Task<IEnumerable<Reservation>> GetReservationsByGuestIdAsync(int guestId);
    Task<bool> HasOverlappingReservationAsync(int roomId, DateTime checkIn, DateTime checkOut, int? excludeReservationId = null);
}