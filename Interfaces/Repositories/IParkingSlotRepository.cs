using Event_and_parking_reservation_system.Models;

namespace Event_and_parking_reservation_system.Interfaces.Repositories
{
    public interface IParkingSlotRepository
    {
        Task<List<ParkingSlot>> GetByEventIdAsync(int eventId);

        Task<ParkingSlot?> GetByIdAsync(int parkingSlotId);

        Task<bool> ParkingLayoutExistsAsync(int eventId);

        Task AddRangeAsync(IEnumerable<ParkingSlot> parkingSlots);

        void Update(ParkingSlot parkingSlot);

        void Remove(ParkingSlot parkingSlot);

        Task SaveChangesAsync();
    }
}