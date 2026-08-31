using Event_and_parking_reservation_system.Data;
using Event_and_parking_reservation_system.DTOs.Parking;
using Event_and_parking_reservation_system.Enums;
using Event_and_parking_reservation_system.Interfaces.Repositories;
using Event_and_parking_reservation_system.Interfaces.Services;
using Event_and_parking_reservation_system.Models;
using Microsoft.EntityFrameworkCore;

namespace Event_and_parking_reservation_system.Services
{
    public class ParkingSlotService : IParkingSlotService
    {
        private readonly IParkingSlotRepository _parkingSlotRepository;
        private readonly AppDbContext _context;

        public ParkingSlotService(
            IParkingSlotRepository parkingSlotRepository,
            AppDbContext context)
        {
            _parkingSlotRepository = parkingSlotRepository;
            _context = context;
        }

        public async Task<List<ParkingSlotResponseDto>> GetParkingLayoutAsync(int eventId)
        {
            var eventExists = await _context.Events
                .AnyAsync(e => e.Id == eventId);

            if (!eventExists)
                throw new KeyNotFoundException("Event not found.");

            var parkingSlots =
                await _parkingSlotRepository.GetByEventIdAsync(eventId);

            return parkingSlots
                .Select(MapToResponse)
                .ToList();
        }

        public async Task<List<ParkingSlotResponseDto>> GenerateParkingLayoutAsync(
            int eventId,
            GenerateParkingLayoutDto dto)
        {
            var eventEntity = await _context.Events
                .FirstOrDefaultAsync(e => e.Id == eventId);

            if (eventEntity == null)
                throw new KeyNotFoundException("Event not found.");

            if (await _parkingSlotRepository.ParkingLayoutExistsAsync(eventId))
                throw new InvalidOperationException(
                    "Parking layout already exists for this event.");

            var parkingSlots = new List<ParkingSlot>();

            for (int i = 1; i <= dto.TotalSlots; i++)
            {
                parkingSlots.Add(new ParkingSlot
                {
                    EventId = eventId,
                    SlotNumber = $"P{i}",
                    Zone = dto.Zone,
                    Fee = dto.Fee > 0
                        ? dto.Fee
                        : eventEntity.ParkingFee,
                    Status = ParkingSlotStatus.Available
                });
            }

            await _parkingSlotRepository.AddRangeAsync(parkingSlots);
            await _parkingSlotRepository.SaveChangesAsync();

            return parkingSlots
                .Select(MapToResponse)
                .ToList();
        }

        public async Task<ParkingSlotResponseDto> UpdateParkingSlotAsync(
            int eventId,
            int parkingSlotId,
            UpdateParkingSlotDto dto)
        {
            var parkingSlot =
                await _parkingSlotRepository.GetByIdAsync(parkingSlotId);

            if (parkingSlot == null || parkingSlot.EventId != eventId)
                throw new KeyNotFoundException("Parking slot not found.");

            if (parkingSlot.ParkingReservations.Any(r => r.IsActive))
                throw new InvalidOperationException(
                    "A parking slot with an active reservation cannot be edited.");

            parkingSlot.SlotNumber = dto.SlotNumber;
            parkingSlot.Zone = dto.Zone;
            parkingSlot.Fee = dto.Fee;

            _parkingSlotRepository.Update(parkingSlot);
            await _parkingSlotRepository.SaveChangesAsync();

            return MapToResponse(parkingSlot);
        }

        public async Task DeleteParkingSlotAsync(
            int eventId,
            int parkingSlotId)
        {
            var parkingSlot =
                await _parkingSlotRepository.GetByIdAsync(parkingSlotId);

            if (parkingSlot == null || parkingSlot.EventId != eventId)
                throw new KeyNotFoundException("Parking slot not found.");

            if (parkingSlot.ParkingReservations.Any(r => r.IsActive))
                throw new InvalidOperationException(
                    "A parking slot with an active reservation cannot be deleted.");

            _parkingSlotRepository.Remove(parkingSlot);
            await _parkingSlotRepository.SaveChangesAsync();
        }

        private static ParkingSlotResponseDto MapToResponse(
            ParkingSlot parkingSlot)
        {
            return new ParkingSlotResponseDto
            {
                Id = parkingSlot.Id,
                EventId = parkingSlot.EventId,
                SlotNumber = parkingSlot.SlotNumber,
                Zone = parkingSlot.Zone,
                Fee = parkingSlot.Fee,
                Status = parkingSlot.Status.ToString()
            };
        }
    }
}