using Event_and_parking_reservation_system.DTOs.Seats;

namespace Event_and_parking_reservation_system.Interfaces.Services
{
    public interface ISeatService
    {
        Task<List<SeatResponseDto>> GetSeatMapAsync(int eventId);

        Task<List<SeatResponseDto>> GenerateSeatMapAsync(
            int eventId,
            GenerateSeatMapDto dto);

        Task<SeatResponseDto> UpdateSeatAsync(
            int eventId,
            int seatId,
            UpdateSeatDto dto);

        Task DeleteSeatAsync(
            int eventId,
            int seatId);
    }
}