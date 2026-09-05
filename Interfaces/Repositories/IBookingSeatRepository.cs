using Event_and_parking_reservation_system.Models;

namespace Event_and_parking_reservation_system.Interfaces.Repositories;

public interface IBookingSeatRepository
{
    Task<List<BookingSeat>> GetByBookingIdAsync(int bookingId);

    Task<bool> HasActiveSeatAsync(int seatId);

    Task AddRangeAsync(IEnumerable<BookingSeat> bookingSeats);

    Task SaveChangesAsync();
}