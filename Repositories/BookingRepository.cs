using Event_and_parking_reservation_system.Data;
using Event_and_parking_reservation_system.Enums;
using Event_and_parking_reservation_system.Interfaces.Repositories;
using Event_and_parking_reservation_system.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Event_and_parking_reservation_system.Repositories
{
    public class BookingRepository : IBookingRepository
    {
        private readonly AppDbContext _db;

        public BookingRepository(AppDbContext db)
        {
            _db = db;
        }

        public Task<Customer?> GetCustomerAsync(int customerId) =>
            _db.Set<Customer>().FirstOrDefaultAsync(x => x.Id == customerId);

        public Task<Models.Event?> GetEventAsync(int eventId) =>
            _db.Set<Models.Event>().FirstOrDefaultAsync(x => x.Id == eventId);

        public Task<List<Seat>> GetSeatsAsync(IEnumerable<int> seatIds)
        {
            List<int> ids = seatIds.Distinct().ToList();
            return _db.Set<Seat>()
                .Where(x => ids.Contains(x.Id))
                .ToListAsync();
        }

        public Task<ParkingSlot?> GetParkingSlotAsync(int parkingSlotId) =>
            _db.Set<ParkingSlot>().FirstOrDefaultAsync(x => x.Id == parkingSlotId);

        public Task<Booking?> GetByIdAsync(int bookingId) =>
            _db.Set<Booking>().FirstOrDefaultAsync(x => x.Id == bookingId);

        public Task<List<Booking>> GetByCustomerAsync(int customerId) =>
            _db.Set<Booking>()
                .Where(x => x.CustomerId == customerId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

        public Task<List<Booking>> GetByEventAsync(int eventId) =>
            _db.Set<Booking>()
                .Where(x => x.EventId == eventId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

        public Task<List<Booking>> GetExpiredPendingAsync(DateTime utcNow)
        {
            BookingStatus pending = Enum.Parse<BookingStatus>("Pending", true);

            return _db.Set<Booking>()
                .Where(x =>
                    x.Status == pending &&
                    x.HoldExpiresAt != null &&
                    x.HoldExpiresAt <= utcNow)
                .ToListAsync();
        }

        public Task<bool> BookingNumberExistsAsync(string bookingNumber) =>
            _db.Set<Booking>().AnyAsync(x => x.BookingNumber == bookingNumber);

        public Task<List<BookingSeat>> GetBookingSeatsAsync(int bookingId) =>
            _db.Set<BookingSeat>()
                .Include(x => x.Seat)
                .Where(x => x.BookingId == bookingId)
                .ToListAsync();

        public Task<ParkingReservation?> GetActiveParkingAsync(int bookingId) =>
            _db.Set<ParkingReservation>()
                .Include(x => x.ParkingSlot)
                .FirstOrDefaultAsync(x => x.BookingId == bookingId && x.IsActive);

        public Task AddBookingAsync(Booking booking) =>
            _db.Set<Booking>().AddAsync(booking).AsTask();

        public Task AddBookingSeatsAsync(IEnumerable<BookingSeat> bookingSeats) =>
            _db.Set<BookingSeat>().AddRangeAsync(bookingSeats);

        public Task AddParkingReservationAsync(ParkingReservation reservation) =>
            _db.Set<ParkingReservation>().AddAsync(reservation).AsTask();

        public Task<IDbContextTransaction> BeginTransactionAsync() =>
            _db.Database.BeginTransactionAsync();

        public Task<int> SaveChangesAsync() =>
            _db.SaveChangesAsync();
    }
}
