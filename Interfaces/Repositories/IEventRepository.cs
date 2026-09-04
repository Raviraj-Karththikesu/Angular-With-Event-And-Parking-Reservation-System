using Event_and_parking_reservation_system.DTOs.Events;
using Event_and_parking_reservation_system.Models;

namespace Event_and_parking_reservation_system.Interfaces.Repositories
{
    public interface IEventRepository
    {
        Task<IEnumerable<Event>> GetAllAsync(EventFilterDto filter);

        Task<Event?> GetByIdAsync(int id);

        Task AddAsync(Event eventEntity);

        Task UpdateAsync(Event eventEntity);

        Task DeleteAsync(Event eventEntity);

        Task<bool> HasOverlapAsync(
            int venueId,
            DateTime startDateTime,
            DateTime endDateTime,
            int? excludeEventId = null);

        Task<int> GetBookedSeatCountAsync(int eventId);
    }
}