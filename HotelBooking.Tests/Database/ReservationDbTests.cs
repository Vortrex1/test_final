using System;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Networks;
using HotelBooking.Data;
using HotelBooking.Models;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Xunit;

namespace HotelBooking.Tests.Database;

public class ReservationDbTests : IAsyncLifetime
{
    private readonly INetwork _network;
    private readonly PostgreSqlContainer _dbContainer;
    private ApplicationDbContext _dbContext = null!;

    public string ConnectionString => _dbContainer.GetConnectionString();

    public ReservationDbTests()
    {
        _network = new NetworkBuilder()
            .WithName(Guid.NewGuid().ToString())
            .Build();

        _dbContainer = new PostgreSqlBuilder("postgres:16-alpine")
            .WithDatabase("testdb")
            .WithUsername("postgres")
            .WithPassword("postgres_password")
            .WithNetwork(_network)
            .WithNetworkAliases("db")
            .WithPortBinding(5432, true)
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _network.CreateAsync();
        await _dbContainer.StartAsync();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(_dbContainer.GetConnectionString())
            .Options;

        _dbContext = new ApplicationDbContext(options);
        await _dbContext.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await _dbContext.DisposeAsync();
        await _dbContainer.DisposeAsync();
        await _network.DeleteAsync();
    }

    [Fact]
    public async Task OverlappingReservations_CanBeQueried()
    {
        // Arrange
        var room = new Room { Number = "101", Type = RoomType.Single, PricePerNight = 100, Floor = 1, IsAvailable = true };
        var guest = new Guest { FirstName = "John", LastName = "Doe", Email = "john@example.com", Phone = "123", PassportNumber = "ABC1" };

        _dbContext!.Rooms.Add(room);
        _dbContext.Guests.Add(guest);
        await _dbContext.SaveChangesAsync();

        var reservation = new Reservation
        {
            RoomId = room.Id,
            GuestId = guest.Id,
            CheckInDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            CheckOutDate = new DateTime(2024, 1, 3, 0, 0, 0, DateTimeKind.Utc),
            TotalPrice = 200,
            Status = ReservationStatus.Confirmed
        };

        _dbContext.Reservations.Add(reservation);
        await _dbContext.SaveChangesAsync();

        // Act: query with an overlapping candidate period
        var queryStart = new DateTime(2024, 1, 2, 0, 0, 0, DateTimeKind.Utc);
        var queryEnd = new DateTime(2024, 1, 4, 0, 0, 0, DateTimeKind.Utc);

        var overlapping = await _dbContext.Reservations
            .Where(r => r.RoomId == room.Id &&
                       r.CheckInDate < queryEnd &&
                       r.CheckOutDate > queryStart)
            .ToListAsync();

        Assert.Single(overlapping);
        Assert.Equal(reservation.Id, overlapping[0].Id);
    }

    [Fact]
    public async Task OverlappingReservations_ShouldBeRejectedByDatabase()
    {
        // Arrange
        var room = new Room { Number = "101", Type = RoomType.Single, PricePerNight = 100, Floor = 1, IsAvailable = true };
        var guest1 = new Guest { FirstName = "John", LastName = "Doe", Email = "john@example.com", Phone = "123", PassportNumber = "ABC1" };
        var guest2 = new Guest { FirstName = "Jane", LastName = "Smith", Email = "jane@example.com", Phone = "456", PassportNumber = "DEF2" };

        _dbContext!.Rooms.Add(room);
        _dbContext.Guests.AddRange(guest1, guest2);
        await _dbContext.SaveChangesAsync();

        var reservation1 = new Reservation
        {
            RoomId = room.Id,
            GuestId = guest1.Id,
            CheckInDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            CheckOutDate = new DateTime(2024, 1, 3, 0, 0, 0, DateTimeKind.Utc),
            TotalPrice = 200,
            Status = ReservationStatus.Confirmed
        };

        var reservation2 = new Reservation
        {
            RoomId = room.Id,
            GuestId = guest2.Id,
            CheckInDate = new DateTime(2024, 1, 2, 0, 0, 0, DateTimeKind.Utc),
            CheckOutDate = new DateTime(2024, 1, 4, 0, 0, 0, DateTimeKind.Utc),
            TotalPrice = 200,
            Status = ReservationStatus.Confirmed
        };

        _dbContext.Reservations.Add(reservation1);
        await _dbContext.SaveChangesAsync();

        // Act / Assert
        _dbContext.Reservations.Add(reservation2);
        await Assert.ThrowsAsync<DbUpdateException>(() => _dbContext.SaveChangesAsync());
    }

    [Fact]
    public async Task CancelledReservation_AllowsOverlappingBooking()
    {
        // Arrange
        var room = new Room { Number = "101", Type = RoomType.Single, PricePerNight = 100, Floor = 1, IsAvailable = true };
        var guest = new Guest { FirstName = "John", LastName = "Doe", Email = "john@example.com", Phone = "123", PassportNumber = "ABC1" };
        var guest2 = new Guest { FirstName = "Jane", LastName = "Smith", Email = "jane@example.com", Phone = "456", PassportNumber = "DEF2" };

        _dbContext!.Rooms.Add(room);
        _dbContext.Guests.AddRange(guest, guest2);
        await _dbContext.SaveChangesAsync();

        var cancelledReservation = new Reservation
        {
            RoomId = room.Id,
            GuestId = guest.Id,
            CheckInDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            CheckOutDate = new DateTime(2024, 1, 3, 0, 0, 0, DateTimeKind.Utc),
            TotalPrice = 200,
            Status = ReservationStatus.Cancelled
        };

        var overlappingReservation = new Reservation
        {
            RoomId = room.Id,
            GuestId = guest2.Id,
            CheckInDate = new DateTime(2024, 1, 2, 0, 0, 0, DateTimeKind.Utc),
            CheckOutDate = new DateTime(2024, 1, 4, 0, 0, 0, DateTimeKind.Utc),
            TotalPrice = 200,
            Status = ReservationStatus.Confirmed
        };

        _dbContext.Reservations.Add(cancelledReservation);
        await _dbContext.SaveChangesAsync();

        // Act
        _dbContext.Reservations.Add(overlappingReservation);
        await _dbContext.SaveChangesAsync();

        // Assert
        var saved = await _dbContext.Reservations
            .Where(r => r.RoomId == room.Id)
            .ToListAsync();
        Assert.Equal(2, saved.Count);
        Assert.Contains(saved, r => r.Status == ReservationStatus.Cancelled);
        Assert.Contains(saved, r => r.Status == ReservationStatus.Confirmed);
    }

    [Fact]
    public async Task GuestReservationHistory_ShouldTrackAllBookings()
    {
        // Arrange
        var room1 = new Room { Number = "101", Type = RoomType.Single, PricePerNight = 100, Floor = 1, IsAvailable = true };
        var room2 = new Room { Number = "102", Type = RoomType.Double, PricePerNight = 150, Floor = 1, IsAvailable = true };
        var guest = new Guest { FirstName = "John", LastName = "Doe", Email = "john@example.com", Phone = "123", PassportNumber = "ABC1" };

        _dbContext.Rooms.AddRange(room1, room2);
        _dbContext.Guests.Add(guest);
        await _dbContext.SaveChangesAsync();

        var reservation1 = new Reservation
        {
            RoomId = room1.Id,
            GuestId = guest.Id,
            CheckInDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            CheckOutDate = new DateTime(2024, 1, 2, 0, 0, 0, DateTimeKind.Utc),
            TotalPrice = 100,
            Status = ReservationStatus.CheckedOut
        };

        var reservation2 = new Reservation
        {
            RoomId = room2.Id,
            GuestId = guest.Id,
            CheckInDate = new DateTime(2024, 2, 1, 0, 0, 0, DateTimeKind.Utc),
            CheckOutDate = new DateTime(2024, 2, 3, 0, 0, 0, DateTimeKind.Utc),
            TotalPrice = 300,
            Status = ReservationStatus.Confirmed
        };

        _dbContext.Reservations.AddRange(reservation1, reservation2);
        await _dbContext.SaveChangesAsync();

        // Act
        var history = await _dbContext.Reservations
            .Where(r => r.GuestId == guest.Id)
            .Include(r => r.Room)
            .ToListAsync();

        // Assert
        Assert.Equal(2, history.Count);
        Assert.Contains(history, r => r.Room.Number == "101" && r.Status == ReservationStatus.CheckedOut);
        Assert.Contains(history, r => r.Room.Number == "102" && r.Status == ReservationStatus.Confirmed);
    }

    [Fact]
    public async Task RoomStatusTracking_ShouldReflectAvailability()
    {
        // Arrange
        var room = new Room { Number = "101", Type = RoomType.Single, PricePerNight = 100, Floor = 1, IsAvailable = true };
        var guest = new Guest { FirstName = "John", LastName = "Doe", Email = "john@example.com", Phone = "123", PassportNumber = "ABC1" };

        _dbContext.Rooms.Add(room);
        _dbContext.Guests.Add(guest);
        await _dbContext.SaveChangesAsync();

        // Act: Check available rooms
        var availableBefore = await _dbContext.Rooms
            .Where(r => r.IsAvailable)
            .ToListAsync();
        Assert.Contains(availableBefore, r => r.Number == "101");

        // Create reservation
        var reservation = new Reservation
        {
            RoomId = room.Id,
            GuestId = guest.Id,
            CheckInDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            CheckOutDate = new DateTime(2024, 1, 3, 0, 0, 0, DateTimeKind.Utc),
            TotalPrice = 200,
            Status = ReservationStatus.Confirmed
        };
        _dbContext.Reservations.Add(reservation);
        await _dbContext.SaveChangesAsync();

        // Check overlapping availability
        var availableAfter = await _dbContext.Rooms
            .Where(r => !_dbContext.Reservations
                .Where(res => res.Status != ReservationStatus.Cancelled &&
                             ((res.CheckInDate < new DateTime(2024, 1, 2, 0, 0, 0, DateTimeKind.Utc) && res.CheckOutDate > new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc))))
                .Select(res => res.RoomId)
                .Contains(r.Id))
            .ToListAsync();

        // Assert: Room should not be available during reservation period
        Assert.DoesNotContain(availableAfter, r => r.Number == "101");
    }
}