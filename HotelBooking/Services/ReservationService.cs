using HotelBooking.Models;
using HotelBooking.Repositories;

namespace HotelBooking.Services;

public class ReservationService
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IRoomRepository _roomRepository;
    private readonly IGuestRepository _guestRepository;

    public ReservationService(
        IReservationRepository reservationRepository,
        IRoomRepository roomRepository,
        IGuestRepository guestRepository)
    {
        _reservationRepository = reservationRepository;
        _roomRepository = roomRepository;
        _guestRepository = guestRepository;
    }

    public async Task<Reservation> CreateReservationAsync(int roomId, int guestId, DateTime checkIn, DateTime checkOut)
    {
        // Validate dates
        if (checkOut <= checkIn)
        {
            throw new ArgumentException("Check-out date must be after check-in date.");
        }

        // Check room availability
        var hasOverlap = await _reservationRepository.HasOverlappingReservationAsync(roomId, checkIn, checkOut);
        if (hasOverlap)
        {
            throw new InvalidOperationException("Room is not available for the selected dates.");
        }

        // Get room for pricing
        var room = await _roomRepository.GetByIdAsync(roomId);
        if (room == null || !room.IsAvailable)
        {
            throw new InvalidOperationException("Room not found or not available.");
        }

        // Calculate total price
        var nights = (checkOut - checkIn).Days;
        var totalPrice = nights * room.PricePerNight;

        var reservation = new Reservation
        {
            RoomId = roomId,
            GuestId = guestId,
            CheckInDate = checkIn,
            CheckOutDate = checkOut,
            TotalPrice = totalPrice,
            Status = ReservationStatus.Confirmed
        };

        await _reservationRepository.AddAsync(reservation);
        return reservation;
    }

    public async Task CheckInAsync(int reservationId)
    {
        var reservation = await _reservationRepository.GetByIdAsync(reservationId);
        if (reservation == null)
        {
            throw new KeyNotFoundException("Reservation not found.");
        }

        if (reservation.Status != ReservationStatus.Confirmed)
        {
            throw new InvalidOperationException("Can only check-in confirmed reservations.");
        }

        if (reservation.CheckInDate.Date != DateTime.Today)
        {
            throw new InvalidOperationException("Check-in is only allowed on the check-in date.");
        }

        reservation.Status = ReservationStatus.CheckedIn;
        await _reservationRepository.UpdateAsync(reservation);
    }

    public async Task CheckOutAsync(int reservationId)
    {
        var reservation = await _reservationRepository.GetByIdAsync(reservationId);
        if (reservation == null)
        {
            throw new KeyNotFoundException("Reservation not found.");
        }

        if (reservation.Status != ReservationStatus.CheckedIn)
        {
            throw new InvalidOperationException("Can only check-out checked-in reservations.");
        }

        reservation.Status = ReservationStatus.CheckedOut;
        await _reservationRepository.UpdateAsync(reservation);
    }

    public async Task CancelReservationAsync(int reservationId)
    {
        var reservation = await _reservationRepository.GetByIdAsync(reservationId);
        if (reservation == null)
        {
            throw new KeyNotFoundException("Reservation not found.");
        }

        if (reservation.Status == ReservationStatus.CheckedIn || reservation.Status == ReservationStatus.CheckedOut)
        {
            throw new InvalidOperationException("Cannot cancel reservation after check-in.");
        }

        reservation.Status = ReservationStatus.Cancelled;
        await _reservationRepository.UpdateAsync(reservation);
    }

    public async Task<Reservation?> GetReservationByIdAsync(int id)
    {
        return await _reservationRepository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Reservation>> GetReservationsByGuestIdAsync(int guestId)
    {
        return await _reservationRepository.GetReservationsByGuestIdAsync(guestId);
    }
}