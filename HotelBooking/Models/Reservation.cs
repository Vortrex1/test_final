using System.ComponentModel.DataAnnotations;

namespace HotelBooking.Models;

public class Reservation
{
    public int Id { get; set; }

    [Required]
    public int RoomId { get; set; }

    [Required]
    public int GuestId { get; set; }

    [Required]
    public DateTime CheckInDate { get; set; }

    [Required]
    public DateTime CheckOutDate { get; set; }

    [Range(0, double.MaxValue)]
    public decimal TotalPrice { get; set; }

    [Required]
    public ReservationStatus Status { get; set; } = ReservationStatus.Confirmed;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Room? Room { get; set; }
    public Guest? Guest { get; set; }
}