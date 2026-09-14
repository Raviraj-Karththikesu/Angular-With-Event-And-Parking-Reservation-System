using Event_and_parking_reservation_system.Models;
using Microsoft.EntityFrameworkCore.Storage;

namespace Event_and_parking_reservation_system.Interfaces.Repositories
{
    public interface IPaymentRepository
    {
        Task<Booking?> GetBookingAsync(int bookingId);
        Task<Payment?> GetByBookingAsync(int bookingId);
        Task<Payment?> GetByIdAsync(int paymentId);
        Task<List<Payment>> GetByCustomerAsync(int customerId);
        Task AddAsync(Payment payment);
        Task<IDbContextTransaction> BeginTransactionAsync();
        Task<int> SaveChangesAsync();
    }
}
