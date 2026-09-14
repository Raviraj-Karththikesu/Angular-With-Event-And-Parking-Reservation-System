using Event_and_parking_reservation_system.Models;

namespace Event_and_parking_reservation_system.Interfaces.Repositories
{
    public interface IVenueRepository
    {
        Task<IEnumerable<Venue>> GetAllAsync();

        Task<Venue?> GetByIdAsync(int id);

        Task AddAsync(Venue venue);

        Task UpdateAsync(Venue venue);

        Task DeleteAsync(Venue venue);

        Task<bool> HasUpcomingEventsAsync(int venueId);

        Task<bool> IsAvailableAsync(
            int venueId,
            DateOnly date,
            TimeOnly startTime,
            TimeOnly endTime,
            int? excludeEventId = null);
    }
}