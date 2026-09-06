using System.Security.Cryptography;
using Event_and_parking_reservation_system.DTOs.Bookings;
using Event_and_parking_reservation_system.Enums;
using Event_and_parking_reservation_system.Exceptions;
using Event_and_parking_reservation_system.Interfaces.Repositories;
using Event_and_parking_reservation_system.Interfaces.Services;
using Event_and_parking_reservation_system.Models;
using Event_and_parking_reservation_system.Options;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Options;

namespace Event_and_parking_reservation_system.Services
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _repository;
       // private readonly INotificationService _notifications;
        private readonly BookingSettings _settings;

        public BookingService(
            IBookingRepository repository,
           // INotificationService notifications,
            IOptions<BookingSettings> settings)
        {
            _repository = repository;
           // _notifications = notifications;
            _settings = settings.Value;
        }

        public async Task<BookingResponseDto> CreateAsync(
            int customerId,
            CreateBookingDto dto)
        {
            if (dto.SeatIds is null || dto.SeatIds.Count == 0)
                throw new AppException(
                    "A booking must contain at least one seat.",
                    StatusCodes.Status400BadRequest);

            List<int> seatIds = dto.SeatIds.Distinct().ToList();

            if (seatIds.Count != dto.SeatIds.Count)
                throw new AppException(
                    "The same seat cannot be selected twice.",
                    StatusCodes.Status400BadRequest);

            Customer? customer = await _repository.GetCustomerAsync(customerId);
            if (customer is null)
                throw new NotFoundException("Customer was not found.");

            if (!customer.EmailVerified)
                throw new AppException(
                    "Please verify your email address before creating a booking.",
                    StatusCodes.Status403Forbidden);

            Models.Event? eventEntity = await _repository.GetEventAsync(dto.EventId);
            if (eventEntity is null)
                throw new NotFoundException("Event was not found.");

            List<Seat> seats = await _repository.GetSeatsAsync(seatIds);
            if (seats.Count != seatIds.Count)
                throw new NotFoundException("One or more selected seats were not found.");

            if (seats.Any(x => x.EventId != dto.EventId))
                throw new AppException(
                    "Every selected seat must belong to the selected event.",
                    StatusCodes.Status400BadRequest);

            SeatStatus availableSeat = Enum.Parse<SeatStatus>("Available", true);
            List<int> unavailableSeatIds = seats
                .Where(x => x.Status != availableSeat)
                .Select(x => x.Id)
                .ToList();

            if (unavailableSeatIds.Count > 0)
                throw new ConflictException(
                    $"Seats already taken: {string.Join(", ", unavailableSeatIds)}");

            ParkingSlot? parkingSlot = null;

            if (dto.ParkingSlotId.HasValue)
            {
                parkingSlot = await _repository.GetParkingSlotAsync(dto.ParkingSlotId.Value);

                if (parkingSlot is null)
                    throw new NotFoundException("Parking slot was not found.");

                if (parkingSlot.EventId != dto.EventId)
                    throw new AppException(
                        "The parking slot must belong to the selected event.",
                        StatusCodes.Status400BadRequest);

                ParkingSlotStatus availableParking =
                    Enum.Parse<ParkingSlotStatus>("Available", true);

                if (parkingSlot.Status != availableParking)
                    throw new ConflictException("The selected parking slot is already occupied.");
            }

            if (_settings.HoldMinutes <= 0)
                throw new AppException(
                    "Booking hold time is not configured.",
                    StatusCodes.Status500InternalServerError);

            int holdMinutes = _settings.HoldMinutes;

            DateTime now = DateTime.UtcNow;

            Booking booking = new()
            {
                BookingNumber = await GenerateBookingNumberAsync(),
                CustomerId = customerId,
                EventId = dto.EventId,
                Status = Enum.Parse<BookingStatus>("Pending", true),
                TotalAmount = seats.Sum(x => x.Price) + (parkingSlot?.Fee ?? 0m),
                HoldExpiresAt = now.AddMinutes(holdMinutes),
                CreatedAt = now,
                UpdatedAt = now
            };

            await using IDbContextTransaction transaction =
                await _repository.BeginTransactionAsync();

            try
            {
                await _repository.AddBookingAsync(booking);
                await _repository.SaveChangesAsync();

                List<BookingSeat> bookingSeats = seats
                    .Select(seat => new BookingSeat
                    {
                        BookingId = booking.Id,
                        SeatId = seat.Id,
                        PriceAtBooking = seat.Price
                    })
                    .ToList();

                await _repository.AddBookingSeatsAsync(bookingSeats);

                // The current schema uses Available/Booked.
                // A Pending booking uses Booked as the temporary hold state.
                SeatStatus bookedSeat = Enum.Parse<SeatStatus>("Booked", true);
                foreach (Seat seat in seats)
                    seat.Status = bookedSeat;

                if (parkingSlot is not null)
                {
                    ParkingReservation reservation = new()
                    {
                        BookingId = booking.Id,
                        ParkingSlotId = parkingSlot.Id,
                        FeeAtReservation = parkingSlot.Fee,
                        ReservedAt = now,
                        ReleasedAt = null
                    };

                    await _repository.AddParkingReservationAsync(reservation);

                    parkingSlot.Status = ParkingSlotStatus.Held;
                }

                await _repository.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            return await MapAsync(booking);
        }

        public async Task<BookingResponseDto> GetByIdAsync(
            int requesterCustomerId,
            bool isAdmin,
            int bookingId)
        {
            Booking booking = await RequireBookingAsync(bookingId);
            EnsureOwnerOrAdmin(requesterCustomerId, isAdmin, booking);
            return await MapAsync(booking);
        }

        public async Task<List<BookingResponseDto>> GetCustomerBookingsAsync(
            int requesterCustomerId,
            bool isAdmin,
            int customerId)
        {
            if (!isAdmin && requesterCustomerId != customerId)
                throw new AppException(
                    "You can only view your own bookings.",
                    StatusCodes.Status403Forbidden);

            List<Booking> bookings = await _repository.GetByCustomerAsync(customerId);
            List<BookingResponseDto> response = new();

            foreach (Booking booking in bookings)
                response.Add(await MapAsync(booking));

            return response;
        }

        public async Task<List<BookingResponseDto>> GetEventBookingsAsync(int eventId)
        {
            List<Booking> bookings = await _repository.GetByEventAsync(eventId);
            List<BookingResponseDto> response = new();

            foreach (Booking booking in bookings)
                response.Add(await MapAsync(booking));

            return response;
        }

        public async Task<HoldStatusDto> GetHoldStatusAsync(
            int requesterCustomerId,
            bool isAdmin,
            int bookingId)
        {
            Booking booking = await RequireBookingAsync(bookingId);
            EnsureOwnerOrAdmin(requesterCustomerId, isAdmin, booking);

            BookingStatus pending = Enum.Parse<BookingStatus>("Pending", true);
            bool expired =
                booking.Status == pending &&
                booking.HoldExpiresAt.HasValue &&
                booking.HoldExpiresAt.Value <= DateTime.UtcNow;

            if (expired)
            {
                await ExpireOneAsync(booking);
                booking = await RequireBookingAsync(bookingId);
            }

            return new HoldStatusDto
            {
                BookingId = booking.Id,
                BookingNumber = booking.BookingNumber,
                Status = booking.Status.ToString(),
                HoldExpiresAt = booking.HoldExpiresAt,
                RemainingSeconds = RemainingSeconds(booking),
                IsExpired = booking.Status.ToString()
                    .Equals("Expired", StringComparison.OrdinalIgnoreCase)
            };
        }

        public async Task CancelAsync(
            int requesterCustomerId,
            bool isAdmin,
            int bookingId)
        {
            Booking booking = await RequireBookingAsync(bookingId);
            EnsureOwnerOrAdmin(requesterCustomerId, isAdmin, booking);

            if (booking.Status.ToString().Equals(
                "Cancelled",
                StringComparison.OrdinalIgnoreCase))
                return;

            if (booking.Status.ToString().Equals(
                "Expired",
                StringComparison.OrdinalIgnoreCase))
                return;

            await ReleaseReservationAsync(
                booking,
                Enum.Parse<BookingStatus>("Cancelled", true));

           //await _notifications.CreateInternalAsync(
           //     booking.CustomerId,
           //     "BookingCancelled",
           //     "Booking cancelled",
           //     $"Booking {booking.BookingNumber} was cancelled.");
        }

        public async Task<int> ExpirePendingBookingsAsync()
        {
            List<Booking> expired =
                await _repository.GetExpiredPendingAsync(DateTime.UtcNow);

            foreach (Booking booking in expired)
                await ExpireOneAsync(booking);

            return expired.Count;
        }

        private async Task ExpireOneAsync(Booking booking)
        {
            await ReleaseReservationAsync(
                booking,
                Enum.Parse<BookingStatus>("Expired", true));
        }

        private async Task ReleaseReservationAsync(
            Booking booking,
            BookingStatus finalStatus)
        {
            await using IDbContextTransaction transaction =
                await _repository.BeginTransactionAsync();

            try
            {
                List<BookingSeat> bookingSeats =
                    await _repository.GetBookingSeatsAsync(booking.Id);

                SeatStatus availableSeat =
                    Enum.Parse<SeatStatus>("Available", true);

                foreach (BookingSeat bookingSeat in bookingSeats)
                    bookingSeat.Seat.Status = availableSeat;

                ParkingReservation? parking =
                    await _repository.GetActiveParkingAsync(booking.Id);

                if (parking is not null)
                {
                    parking.ReleasedAt = DateTime.UtcNow;
                    parking.ParkingSlot.Status =
                        Enum.Parse<ParkingSlotStatus>("Available", true);
                }

                booking.Status = finalStatus;
                booking.HoldExpiresAt = null;
                booking.UpdatedAt = DateTime.UtcNow;

                await _repository.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task<Booking> RequireBookingAsync(int bookingId)
        {
            Booking? booking = await _repository.GetByIdAsync(bookingId);
            return booking ?? throw new NotFoundException("Booking was not found.");
        }

        private static void EnsureOwnerOrAdmin(
            int requesterCustomerId,
            bool isAdmin,
            Booking booking)
        {
            if (!isAdmin && booking.CustomerId != requesterCustomerId)
                throw new AppException(
                    "You can only access your own booking.",
                    StatusCodes.Status403Forbidden);
        }

        private async Task<BookingResponseDto> MapAsync(Booking booking)
        {
            List<BookingSeat> seats =
                await _repository.GetBookingSeatsAsync(booking.Id);

            ParkingReservation? parking =
                await _repository.GetActiveParkingAsync(booking.Id);

            return new BookingResponseDto
            {
                BookingId = booking.Id,
                BookingNumber = booking.BookingNumber,
                CustomerId = booking.CustomerId,
                EventId = booking.EventId,
                Status = booking.Status.ToString(),
                TotalAmount = booking.TotalAmount,
                HoldExpiresAt = booking.HoldExpiresAt,
                RemainingHoldSeconds = RemainingSeconds(booking),
                SeatIds = seats.Select(x => x.SeatId).ToList(),
                ParkingSlotId = parking?.ParkingSlotId,
                CreatedAt = booking.CreatedAt,
                UpdatedAt = booking.UpdatedAt
            };
        }

        private static int RemainingSeconds(Booking booking)
        {
            if (!booking.HoldExpiresAt.HasValue)
                return 0;

            double seconds =
                (booking.HoldExpiresAt.Value - DateTime.UtcNow).TotalSeconds;

            return Math.Max(0, (int)Math.Ceiling(seconds));
        }

        private async Task<string> GenerateBookingNumberAsync()
        {
            for (int attempt = 0; attempt < 10; attempt++)
            {
                int number = RandomNumberGenerator.GetInt32(0, 1_000_000);
                string bookingNumber =
                    $"BKG-{DateTime.UtcNow:yyyy}-{number:D6}";

                if (!await _repository.BookingNumberExistsAsync(bookingNumber))
                    return bookingNumber;
            }

            throw new AppException(
                "Could not generate a unique booking number.",
                StatusCodes.Status500InternalServerError);
        }
    }
}
