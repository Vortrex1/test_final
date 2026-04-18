using AutoFixture;
using AutoFixture.Xunit2;
using HotelBooking.Models;
using HotelBooking.Repositories;
using HotelBooking.Services;
using Moq;
using Xunit;

namespace HotelBooking.Tests.Unit;

public class ReservationServiceTests
{
    private readonly Mock<IReservationRepository> _reservationRepositoryMock;
    private readonly Mock<IRoomRepository> _roomRepositoryMock;
    private readonly Mock<IGuestRepository> _guestRepositoryMock;
    private readonly ReservationService _service;
    private readonly Fixture _fixture;

    public ReservationServiceTests()
    {
        _reservationRepositoryMock = new Mock<IReservationRepository>();
        _roomRepositoryMock = new Mock<IRoomRepository>();
        _guestRepositoryMock = new Mock<IGuestRepository>();
        _service = new ReservationService(
            _reservationRepositoryMock.Object,
            _roomRepositoryMock.Object,
            _guestRepositoryMock.Object);
        _fixture = new Fixture();
    }

    [Theory, AutoData]
    public async Task CreateReservationAsync_ShouldCalculateTotalPriceCorrectly(int roomId, int guestId, DateTime checkIn, decimal pricePerNight)
    {
        // Arrange
        checkIn = checkIn.Date; // Ensure date only
        var checkOut = checkIn.AddDays(3); // 3 nights
        var expectedTotalPrice = 3 * pricePerNight;

        var room = new Room { Id = roomId, PricePerNight = pricePerNight, IsAvailable = true };
        _roomRepositoryMock.Setup(r => r.GetByIdAsync(roomId)).ReturnsAsync(room);
        _reservationRepositoryMock.Setup(r => r.HasOverlappingReservationAsync(roomId, checkIn, checkOut, null)).ReturnsAsync(false);
        _reservationRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Reservation>())).Returns(Task.CompletedTask);

        // Act
        var result = await _service.CreateReservationAsync(roomId, guestId, checkIn, checkOut);

        // Assert
        Assert.Equal(expectedTotalPrice, result.TotalPrice);
        Assert.Equal(ReservationStatus.Confirmed, result.Status);
    }

    [Theory, AutoData]
    public async Task CreateReservationAsync_ShouldThrowWhenCheckOutBeforeCheckIn(int roomId, int guestId, DateTime checkIn)
    {
        // Arrange
        var checkOut = checkIn.AddDays(-1); // Invalid

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.CreateReservationAsync(roomId, guestId, checkIn, checkOut));
        Assert.Contains("Check-out date must be after check-in date", exception.Message);
    }

    [Theory, AutoData]
    public async Task CreateReservationAsync_ShouldThrowWhenRoomNotAvailable(int roomId, int guestId, DateTime checkIn)
    {
        // Arrange
        var checkOut = checkIn.AddDays(2);
        _reservationRepositoryMock.Setup(r => r.HasOverlappingReservationAsync(roomId, checkIn, checkOut, null)).ReturnsAsync(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.CreateReservationAsync(roomId, guestId, checkIn, checkOut));
        Assert.Contains("not available", exception.Message);
    }

    [Theory, AutoData]
    public async Task CheckInAsync_ShouldThrowWhenNotOnCheckInDate(int reservationId, DateTime checkIn)
    {
        // Arrange
        checkIn = DateTime.Today.AddDays(1); // Not today
        var reservation = new Reservation { Id = reservationId, CheckInDate = checkIn, Status = ReservationStatus.Confirmed };
        _reservationRepositoryMock.Setup(r => r.GetByIdAsync(reservationId)).ReturnsAsync(reservation);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.CheckInAsync(reservationId));
        Assert.Contains("only allowed on the check-in date", exception.Message);
    }

    [Theory, AutoData]
    public async Task CheckInAsync_ShouldUpdateStatusToCheckedIn(int reservationId)
    {
        // Arrange
        var checkIn = DateTime.Today;
        var reservation = new Reservation { Id = reservationId, CheckInDate = checkIn, Status = ReservationStatus.Confirmed };
        _reservationRepositoryMock.Setup(r => r.GetByIdAsync(reservationId)).ReturnsAsync(reservation);
        _reservationRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Reservation>())).Returns(Task.CompletedTask);

        // Act
        await _service.CheckInAsync(reservationId);

        // Assert
        Assert.Equal(ReservationStatus.CheckedIn, reservation.Status);
    }

    [Theory, AutoData]
    public async Task CancelReservationAsync_ShouldThrowAfterCheckIn(int reservationId)
    {
        // Arrange
        var reservation = new Reservation { Id = reservationId, Status = ReservationStatus.CheckedIn };
        _reservationRepositoryMock.Setup(r => r.GetByIdAsync(reservationId)).ReturnsAsync(reservation);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.CancelReservationAsync(reservationId));
        Assert.Contains("after check-in", exception.Message);
    }

    [Theory, AutoData]
    public async Task CheckOutAsync_ShouldUpdateStatusToCheckedOut(int reservationId)
    {
        // Arrange
        var reservation = new Reservation { Id = reservationId, Status = ReservationStatus.CheckedIn };
        _reservationRepositoryMock.Setup(r => r.GetByIdAsync(reservationId)).ReturnsAsync(reservation);
        _reservationRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Reservation>())).Returns(Task.CompletedTask);

        // Act
        await _service.CheckOutAsync(reservationId);

        // Assert
        Assert.Equal(ReservationStatus.CheckedOut, reservation.Status);
    }
}