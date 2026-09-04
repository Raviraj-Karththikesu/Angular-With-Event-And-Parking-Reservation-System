using Event_and_parking_reservation_system.Models;

namespace Event_and_parking_reservation_system.Interfaces.Repositories
{
    public interface ISeatRepository
    {
        Task<List<Seat>> GetByEventIdAsync(int eventId);

        Task<Seat?> GetByIdAsync(int seatId);

        Task<bool> SeatMapExistsAsync(int eventId);

        Task AddRangeAsync(IEnumerable<Seat> seats);

        void Update(Seat seat);

        void Remove(Seat seat);

        Task SaveChangesAsync();
    }
}