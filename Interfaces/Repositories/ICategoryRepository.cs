using Event_and_parking_reservation_system.Models;

namespace Event_and_parking_reservation_system.Interfaces.Repositories
{
    public interface ICategoryRepository
    {
        Task<IEnumerable<EventCategory>> GetAllAsync();

        Task<EventCategory?> GetByIdAsync(int id);

        Task<EventCategory?> GetByNameAsync(string name);

        Task AddAsync(EventCategory category);

        Task UpdateAsync(EventCategory category);

        Task DeleteAsync(EventCategory category);

        Task<bool> HasEventsAsync(int categoryId);
    }
}