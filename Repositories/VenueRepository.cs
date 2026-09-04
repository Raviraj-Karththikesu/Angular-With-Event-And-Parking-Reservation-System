using Event_and_parking_reservation_system.Data;
using Event_and_parking_reservation_system.Interfaces.Repositories;
using Event_and_parking_reservation_system.Models;
using Microsoft.EntityFrameworkCore;

namespace Event_and_parking_reservation_system.Repositories
{
    public class VenueRepository : IVenueRepository
    {
        private readonly AppDbContext _context;

        public VenueRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Venue>> GetAllAsync()
        {
            return await _context.Venues
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Venue?> GetByIdAsync(int id)
        {
            return await _context.Venues
                .FirstOrDefaultAsync(v => v.Id == id);
        }

        public async Task AddAsync(Venue venue)
        {
            await _context.Venues.AddAsync(venue);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Venue venue)
        {
            _context.Venues.Update(venue);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Venue venue)
        {
            _context.Venues.Remove(venue);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> HasUpcomingEventsAsync(int venueId)
        {
            var now = DateTime.UtcNow;

            return await _context.Events.AnyAsync(e =>
                e.VenueId == venueId &&
                e.StartDateTime >= now);
        }

        public async Task<bool> IsAvailableAsync(
            int venueId,
            DateOnly date,
            TimeOnly startTime,
            TimeOnly endTime,
            int? excludeEventId = null)
        {
            var requestedStart = date.ToDateTime(startTime);
            var requestedEnd = date.ToDateTime(endTime);

            return !await _context.Events.AnyAsync(e =>
                e.VenueId == venueId &&
                (!excludeEventId.HasValue || e.Id != excludeEventId.Value) &&
                requestedStart < e.EndDateTime &&
                requestedEnd > e.StartDateTime);
        }
    }
}