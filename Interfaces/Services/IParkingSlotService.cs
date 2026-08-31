using Event_and_parking_reservation_system.DTOs.Parking;

namespace Event_and_parking_reservation_system.Interfaces.Services
{
    public interface IParkingSlotService
    {
        Task<List<ParkingSlotResponseDto>> GetParkingLayoutAsync(int eventId);

        Task<List<ParkingSlotResponseDto>> GenerateParkingLayoutAsync(
            int eventId,
            GenerateParkingLayoutDto dto);

        Task<ParkingSlotResponseDto> UpdateParkingSlotAsync(
            int eventId,
            int parkingSlotId,
            UpdateParkingSlotDto dto);

        Task DeleteParkingSlotAsync(
            int eventId,
            int parkingSlotId);
    }
}