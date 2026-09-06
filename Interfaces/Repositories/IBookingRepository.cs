using Event_and_parking_reservation_system.Models;
using Microsoft.EntityFrameworkCore.Storage;

namespace Event_and_parking_reservation_system.Interfaces.Repositories
{
    public interface IBookingRepository
    {
        Task<Customer?> GetCustomerAsync(int customerId);
        Task<Models.Event?> GetEventAsync(int eventId);
        Task<List<Seat>> GetSeatsAsync(IEnumerable<int> seatIds);
        Task<ParkingSlot?> GetParkingSlotAsync(int parkingSlotId);

        Task<Booking?> GetByIdAsync(int bookingId);
        Task<List<Booking>> GetByCustomerAsync(int customerId);
        Task<List<Booking>> GetByEventAsync(int eventId);
        Task<List<Booking>> GetExpiredPendingAsync(DateTime utcNow);
        Task<bool> BookingNumberExistsAsync(string bookingNumber);

        Task<List<BookingSeat>> GetBookingSeatsAsync(int bookingId);
        Task<ParkingReservation?> GetActiveParkingAsync(int bookingId);

        Task AddBookingAsync(Booking booking);
        Task AddBookingSeatsAsync(IEnumerable<BookingSeat> bookingSeats);
        Task AddParkingReservationAsync(ParkingReservation reservation);

        Task<IDbContextTransaction> BeginTransactionAsync();
        Task<int> SaveChangesAsync();
    }
}
