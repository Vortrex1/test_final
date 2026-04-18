using HotelBooking.Services;
using Microsoft.AspNetCore.Mvc;
using HotelBooking.Models;

namespace HotelBooking.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RoomsController : ControllerBase
{
    private readonly RoomService _roomService;

    public RoomsController(RoomService roomService)
    {
        _roomService = roomService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllRooms([FromQuery] RoomType? type, [FromQuery] bool? available, [FromQuery] DateTime? checkIn, [FromQuery] DateTime? checkOut)
    {
        var rooms = await _roomService.GetAllRoomsAsync();

        // Filter by type
        if (type.HasValue)
        {
            rooms = rooms.Where(r => r.Type == type.Value);
        }

        // Filter by availability
        if (available.HasValue && available.Value && checkIn.HasValue && checkOut.HasValue)
        {
            rooms = await _roomService.GetAvailableRoomsAsync(checkIn.Value, checkOut.Value);
        }
        else if (available.HasValue && available.Value)
        {
            rooms = rooms.Where(r => r.IsAvailable);
        }

        return Ok(rooms);
    }

    [HttpGet("available")]
    public async Task<IActionResult> GetAvailableRooms([FromQuery] DateTime checkIn, [FromQuery] DateTime checkOut)
    {
        if (checkIn.Kind == DateTimeKind.Unspecified)
        {
            checkIn = DateTime.SpecifyKind(checkIn, DateTimeKind.Utc);
        }

        if (checkOut.Kind == DateTimeKind.Unspecified)
        {
            checkOut = DateTime.SpecifyKind(checkOut, DateTimeKind.Utc);
        }

        var rooms = await _roomService.GetAvailableRoomsAsync(checkIn, checkOut);
        return Ok(rooms);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetRoomById(int id)
    {
        var room = await _roomService.GetRoomByIdAsync(id);
        if (room == null)
        {
            return NotFound();
        }
        return Ok(room);
    }
}