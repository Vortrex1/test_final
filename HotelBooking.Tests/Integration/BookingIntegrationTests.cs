using System;
using System.Net;
using System.Net.Http.Json;
using HotelBooking.Controllers;
using HotelBooking.Data;
using HotelBooking.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace HotelBooking.Tests.Integration;

public class BookingIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public BookingIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Replace DbContext with in-memory for testing
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDb");
                });
            });
        });
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetAvailableRooms_ReturnsAvailableRooms()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();

        // Seed data
        var room1 = new Room { Number = "101", Type = RoomType.Single, PricePerNight = 100, Floor = 1, IsAvailable = true };
        var room2 = new Room { Number = "102", Type = RoomType.Double, PricePerNight = 150, Floor = 1, IsAvailable = true };
        db.Rooms.AddRange(room1, room2);
        await db.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync("/api/rooms/available?checkIn=2024-01-01&checkOut=2024-01-03");

        // Assert
        response.EnsureSuccessStatusCode();
        var rooms = await response.Content.ReadFromJsonAsync<List<Room>>();
        Assert.Equal(2, rooms.Count);
    }

    [Fact]
    public async Task CreateReservation_PreventsDoubleBooking()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();

        // Seed data
        var room = new Room { Number = "101", Type = RoomType.Single, PricePerNight = 100, Floor = 1, IsAvailable = true };
        var guest = new Guest { FirstName = "John", LastName = "Doe", Email = "john@example.com", Phone = "123456789", PassportNumber = "ABC123" };
        db.Rooms.Add(room);
        db.Guests.Add(guest);
        await db.SaveChangesAsync();

        var request1 = new CreateReservationRequest
        {
            RoomId = room.Id,
            GuestId = guest.Id,
            CheckInDate = new DateTime(2024, 1, 1),
            CheckOutDate = new DateTime(2024, 1, 3)
        };

        // Act: Create first reservation
        var response1 = await _client.PostAsJsonAsync("/api/reservations", request1);
        response1.EnsureSuccessStatusCode();

        // Act: Try to create overlapping reservation
        var request2 = new CreateReservationRequest
        {
            RoomId = room.Id,
            GuestId = guest.Id,
            CheckInDate = new DateTime(2024, 1, 2),
            CheckOutDate = new DateTime(2024, 1, 4)
        };
        var response2 = await _client.PostAsJsonAsync("/api/reservations", request2);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response2.StatusCode);
    }

    [Fact]
    public async Task FullBookingFlow_WorksCorrectly()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();

        // Seed data
        var room = new Room { Number = "101", Type = RoomType.Single, PricePerNight = 100, Floor = 1, IsAvailable = true };
        var guest = new Guest { FirstName = "John", LastName = "Doe", Email = "john@example.com", Phone = "123456789", PassportNumber = "ABC123" };
        db.Rooms.Add(room);
        db.Guests.Add(guest);
        await db.SaveChangesAsync();

        var createRequest = new CreateReservationRequest
        {
            RoomId = room.Id,
            GuestId = guest.Id,
            CheckInDate = DateTime.Today,
            CheckOutDate = DateTime.Today.AddDays(2)
        };

        // Act: Create reservation
        var createResponse = await _client.PostAsJsonAsync("/api/reservations", createRequest);
        createResponse.EnsureSuccessStatusCode();
        var reservation = await createResponse.Content.ReadFromJsonAsync<Reservation>();

        // Act: Check-in
        var checkInResponse = await _client.PatchAsync($"/api/reservations/{reservation.Id}/checkin", null);
        Assert.Equal(HttpStatusCode.NoContent, checkInResponse.StatusCode);

        // Act: Check-out
        var checkOutResponse = await _client.PatchAsync($"/api/reservations/{reservation.Id}/checkout", null);
        Assert.Equal(HttpStatusCode.NoContent, checkOutResponse.StatusCode);

        // Assert: Verify status
        var getResponse = await _client.GetAsync($"/api/reservations/{reservation.Id}");
        getResponse.EnsureSuccessStatusCode();
        var updatedReservation = await getResponse.Content.ReadFromJsonAsync<Reservation>();
        Assert.Equal(ReservationStatus.CheckedOut, updatedReservation.Status);
    }
}