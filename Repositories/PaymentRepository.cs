using Event_and_parking_reservation_system.Data;
using Event_and_parking_reservation_system.Interfaces.Repositories;
using Event_and_parking_reservation_system.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Event_and_parking_reservation_system.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly AppDbContext _db;

        public PaymentRepository(AppDbContext db)
        {
            _db = db;
        }

        public Task<Booking?> GetBookingAsync(int bookingId) =>
            _db.Set<Booking>().FirstOrDefaultAsync(x => x.Id == bookingId);

        public Task<Payment?> GetByBookingAsync(int bookingId) =>
            _db.Set<Payment>().FirstOrDefaultAsync(x => x.BookingId == bookingId);

        public Task<Payment?> GetByIdAsync(int paymentId) =>
            _db.Set<Payment>()
                .Include(x => x.Booking)
                .FirstOrDefaultAsync(x => x.Id == paymentId);

        public Task<List<Payment>> GetByCustomerAsync(int customerId) =>
            _db.Set<Payment>()
                .Include(x => x.Booking)
                .Where(x => x.Booking.CustomerId == customerId)
                .OrderByDescending(x => x.PaidAt)
                .ToListAsync();

        public Task AddAsync(Payment payment) =>
            _db.Set<Payment>().AddAsync(payment).AsTask();

        public Task<IDbContextTransaction> BeginTransactionAsync() =>
            _db.Database.BeginTransactionAsync();

        public Task<int> SaveChangesAsync() =>
            _db.SaveChangesAsync();
    }
}
