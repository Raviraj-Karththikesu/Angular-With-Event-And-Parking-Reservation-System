using Event_and_parking_reservation_system.Data;
using Event_and_parking_reservation_system.Interfaces.Repositories;
using Event_and_parking_reservation_system.Models;
using Microsoft.EntityFrameworkCore;

namespace Event_and_parking_reservation_system.Repositories
{
    public class ParkingSlotRepository : IParkingSlotRepository
    {
        private readonly AppDbContext _context;

        public ParkingSlotRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<ParkingSlot>> GetByEventIdAsync(int eventId)
        {
            return await _context.ParkingSlots
                .Where(p => p.EventId == eventId)
                .OrderBy(p => p.Zone)
                .ThenBy(p => p.SlotNumber)
                .ToListAsync();
        }

        public async Task<ParkingSlot?> GetByIdAsync(int parkingSlotId)
        {
            return await _context.ParkingSlots
                .Include(p => p.ParkingReservations)
                .FirstOrDefaultAsync(p => p.Id == parkingSlotId);
        }

        public async Task<bool> ParkingLayoutExistsAsync(int eventId)
        {
            return await _context.ParkingSlots
                .AnyAsync(p => p.EventId == eventId);
        }

        public async Task AddRangeAsync(IEnumerable<ParkingSlot> parkingSlots)
        {
            await _context.ParkingSlots.AddRangeAsync(parkingSlots);
        }

        public void Update(ParkingSlot parkingSlot)
        {
            _context.ParkingSlots.Update(parkingSlot);
        }

        public void Remove(ParkingSlot parkingSlot)
        {
            _context.ParkingSlots.Remove(parkingSlot);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}