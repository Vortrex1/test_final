using AutoFixture;
using Bogus;
using HotelBooking.Models;

namespace HotelBooking.Data;

public static class SeedData
{
    public static void Seed(ApplicationDbContext db)
    {
        const int roomCount = 250;
        const int guestCount = 10000;
        const int reservationCount = 10000;
        const int batchSize = 1000;

        var fixture = new Fixture();
        var faker = new Faker("en");

        var existingRooms = db.Rooms.Count();
        var existingGuests = db.Guests.Count();
        var existingReservations = db.Reservations.Count();

        var rooms = db.Rooms.ToList();
        if (existingRooms < roomCount)
        {
            var newRooms = Enumerable.Range(existingRooms + 1, roomCount - existingRooms)
                .Select(index => fixture.Build<Room>()
                    .With(r => r.Id, -index)
                    .With(r => r.Number, $"R{index:000}")
                    .With(r => r.Type, faker.PickRandom<RoomType>())
                    .With(r => r.PricePerNight, faker.Random.Decimal(60m, 450m))
                    .With(r => r.Floor, faker.Random.Number(1, 10))
                    .With(r => r.IsAvailable, true)
                    .Create())
                .ToList();

            db.Rooms.AddRange(newRooms);
            db.SaveChanges();
            rooms.AddRange(newRooms);
        }

        var guests = db.Guests.ToList();
        if (existingGuests < guestCount)
        {
            var missingGuestCount = guestCount - existingGuests;
            var guestBatch = new List<Guest>(batchSize);
            var nextGuestId = -1;

            for (var index = 0; index < missingGuestCount; index++)
            {
                var person = faker.Person;
                guestBatch.Add(fixture.Build<Guest>()
                    .With(g => g.Id, nextGuestId--)
                    .With(g => g.FirstName, person.FirstName)
                    .With(g => g.LastName, person.LastName)
                    .With(g => g.Email, person.Email)
                    .With(g => g.Phone, person.Phone)
                    .With(g => g.PassportNumber, faker.Random.Replace("??######"))
                    .Create());

                if (guestBatch.Count == batchSize)
                {
                    db.Guests.AddRange(guestBatch);
                    db.SaveChanges();
                    guests.AddRange(guestBatch);
                    guestBatch.Clear();
                }
            }

            if (guestBatch.Count > 0)
            {
                db.Guests.AddRange(guestBatch);
                db.SaveChanges();
                guests.AddRange(guestBatch);
            }
        }

        rooms = db.Rooms.ToList();
        guests = db.Guests.ToList();

        var roomIds = rooms.Select(r => r.Id).ToArray();
        var guestIds = guests.Select(g => g.Id).ToArray();
        var missingReservationCount = Math.Max(0, reservationCount - existingReservations);

        if (missingReservationCount > 0 && roomIds.Length > 0 && guestIds.Length > 0)
        {
            var reservationBatch = new List<Reservation>(batchSize);
            var nextReservationId = -1;
            var roomLastCheckouts = db.Reservations
                .Where(r => roomIds.Contains(r.RoomId))
                .GroupBy(r => r.RoomId)
                .ToDictionary(g => g.Key, g => g.Max(r => r.CheckOutDate));

            var reservationCountAdded = 0;
            var baseCheckIn = AsUtc(DateTime.UtcNow.Date);
            var roomIndex = 0;

            foreach (var roomId in roomIds)
            {
                if (reservationCountAdded >= missingReservationCount)
                {
                    break;
                }

                var currentCheckIn = roomLastCheckouts.TryGetValue(roomId, out var lastCheckOut)
                    ? AsUtc(lastCheckOut.AddDays(1))
                    : AsUtc(baseCheckIn.AddDays(roomIndex));

                while (reservationCountAdded < missingReservationCount)
                {
                    var stayLength = 1;
                    var checkOut = AsUtc(currentCheckIn.AddDays(stayLength));
                    var guestId = faker.PickRandom(guestIds);
                    var status = faker.PickRandom<ReservationStatus>();
                    var room = rooms.First(room => room.Id == roomId);
                    var totalPrice = room.PricePerNight * stayLength;

                    reservationBatch.Add(new Reservation
                    {
                        Id = nextReservationId--,
                        RoomId = roomId,
                        GuestId = guestId,
                        CheckInDate = currentCheckIn,
                        CheckOutDate = checkOut,
                        Status = status,
                        CreatedAt = AsUtc(faker.Date.Past(1, DateTime.UtcNow)),
                        TotalPrice = totalPrice,
                    });

                    reservationCountAdded++;
                    if (reservationBatch.Count == batchSize)
                    {
                        db.Reservations.AddRange(reservationBatch);
                        db.SaveChanges();
                        reservationBatch.Clear();
                    }

                    if (reservationCountAdded >= missingReservationCount)
                    {
                        break;
                    }

                    currentCheckIn = AsUtc(checkOut.AddDays(1));
                }

                roomIndex++;
            }

            if (reservationBatch.Any())
            {
                db.Reservations.AddRange(reservationBatch);
                db.SaveChanges();
            }
        }
    }

    private static DateTime AsUtc(DateTime value)
    {
        return value.Kind == DateTimeKind.Utc ? value : DateTime.SpecifyKind(value, DateTimeKind.Utc);
    }
}
