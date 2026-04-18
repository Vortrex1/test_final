using HotelBooking.Data;
using HotelBooking.Models;

namespace HotelBooking.Repositories;

public class GuestRepository : Repository<Guest>, IGuestRepository
{
    public GuestRepository(ApplicationDbContext context) : base(context)
    {
    }
}