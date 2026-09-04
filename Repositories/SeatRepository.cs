using Event_and_parking_reservation_system.Data;
using Event_and_parking_reservation_system.Interfaces.Repositories;
using Event_and_parking_reservation_system.Models;
using Microsoft.EntityFrameworkCore;

namespace Event_and_parking_reservation_system.Repositories
{
    public class SeatRepository : ISeatRepository
    {
        private readonly AppDbContext _context;

        public SeatRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Seat>> GetByEventIdAsync(int eventId)
        {
            return await _context.Seats
                .Where(s => s.EventId == eventId)
                .OrderBy(s => s.RowLabel)
                .ThenBy(s => s.SeatNumber)
                .ToListAsync();
        }

        public async Task<Seat?> GetByIdAsync(int seatId)
        {
            return await _context.Seats
                .Include(s => s.BookingSeats)
                .FirstOrDefaultAsync(s => s.Id == seatId);
        }

        public async Task<bool> SeatMapExistsAsync(int eventId)
        {
            return await _context.Seats
                .AnyAsync(s => s.EventId == eventId);
        }

        public async Task AddRangeAsync(IEnumerable<Seat> seats)
        {
            await _context.Seats.AddRangeAsync(seats);
        }

        public void Update(Seat seat)
        {
            _context.Seats.Update(seat);
        }

        public void Remove(Seat seat)
        {
            _context.Seats.Remove(seat);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}