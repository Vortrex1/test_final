using HotelBooking.Models;
using HotelBooking.Repositories;

namespace HotelBooking.Services;

public class GuestService
{
    private readonly IGuestRepository _guestRepository;

    public GuestService(IGuestRepository guestRepository)
    {
        _guestRepository = guestRepository;
    }

    public async Task<IEnumerable<Guest>> GetAllGuestsAsync()
    {
        return await _guestRepository.GetAllAsync();
    }

    public async Task<Guest?> GetGuestByIdAsync(int id)
    {
        return await _guestRepository.GetByIdAsync(id);
    }

    public async Task AddGuestAsync(Guest guest)
    {
        await _guestRepository.AddAsync(guest);
    }

    public async Task UpdateGuestAsync(Guest guest)
    {
        await _guestRepository.UpdateAsync(guest);
    }

    public async Task DeleteGuestAsync(int id)
    {
        await _guestRepository.DeleteAsync(id);
    }
}