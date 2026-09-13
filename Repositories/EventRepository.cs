using Event_and_parking_reservation_system.Data;
using Event_and_parking_reservation_system.DTOs.Events;
using Event_and_parking_reservation_system.Interfaces.Repositories;
using Event_and_parking_reservation_system.Models;
using Microsoft.EntityFrameworkCore;

namespace Event_and_parking_reservation_system.Repositories
{
    public class EventRepository : IEventRepository
    {
        private readonly AppDbContext _context;

        public EventRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Event>> GetAllAsync(
            EventFilterDto filter)
        {
            var query = _context.Events
                .Include(e => e.Venue)
                .Include(e => e.EventCategory)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var search = filter.Search.Trim();

                query = query.Where(e =>
                    e.Name.Contains(search));
            }

            if (filter.Date.HasValue)
            {
                var date = filter.Date.Value.Date;

                query = query.Where(e =>
                    e.StartDateTime.Date == date);
            }

            if (filter.VenueId.HasValue)
            {
                query = query.Where(e =>
                    e.VenueId == filter.VenueId.Value);
            }

            if (filter.EventCategoryId.HasValue)
            {
                query = query.Where(e =>
                    e.EventCategoryId ==
                    filter.EventCategoryId.Value);
            }

            return await query
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Event?> GetByIdAsync(int id)
        {
            return await _context.Events
                .Include(e => e.Venue)
                .Include(e => e.EventCategory)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task AddAsync(Event eventEntity)
        {
            await _context.Events.AddAsync(eventEntity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Event eventEntity)
        {
            _context.Events.Update(eventEntity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Event eventEntity)
        {
            _context.Events.Remove(eventEntity);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> HasOverlapAsync(
            int venueId,
            DateTime startDateTime,
            DateTime endDateTime,
            int? excludeEventId = null)
        {
            return await _context.Events.AnyAsync(e =>
                e.VenueId == venueId &&
                (!excludeEventId.HasValue ||
                 e.Id != excludeEventId.Value) &&
                startDateTime < e.EndDateTime &&
                endDateTime > e.StartDateTime);
        }

        public async Task<int> GetBookedSeatCountAsync(
            int eventId)
        {
            return await _context.BookingSeats
                .CountAsync(bs =>
                    bs.Seat.EventId == eventId);
        }

        public async Task<bool> HasBookingsAsync(
            int eventId)
        {
            return await _context.Bookings
                .AnyAsync(b =>
                    b.EventId == eventId);
        }

        public async Task DeleteWithResourcesAsync(
            Event eventEntity)
        {
            await using var transaction =
                await _context.Database
                    .BeginTransactionAsync();

            try
            {
                var seats = await _context.Seats
                    .Where(s =>
                        s.EventId == eventEntity.Id)
                    .ToListAsync();

                var parkingSlots =
                    await _context.ParkingSlots
                        .Where(p =>
                            p.EventId == eventEntity.Id)
                        .ToListAsync();

                _context.Seats.RemoveRange(seats);

                _context.ParkingSlots
                    .RemoveRange(parkingSlots);

                _context.Events.Remove(eventEntity);

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}