using System.ComponentModel.DataAnnotations;

namespace HotelBooking.Models;

public class Room
{
    public int Id { get; set; }

    [Required]
    public string Number { get; set; } = string.Empty;

    [Required]
    public RoomType Type { get; set; }

    [Range(0, double.MaxValue)]
    public decimal PricePerNight { get; set; }

    public int Floor { get; set; }

    public bool IsAvailable { get; set; } = true;
}