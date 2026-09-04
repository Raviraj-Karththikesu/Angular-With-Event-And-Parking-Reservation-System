using Event_and_parking_reservation_system.DTOs.Venues;

namespace Event_and_parking_reservation_system.Interfaces.Services
{
    public interface IVenueService
    {
        Task<IEnumerable<VenueResponseDto>> GetAllAsync();

        Task<VenueResponseDto> GetByIdAsync(int id);

        Task<VenueResponseDto> CreateAsync(CreateVenueDto dto);

        Task<VenueResponseDto> UpdateAsync(int id, UpdateVenueDto dto);

        Task DeleteAsync(int id);

        Task<IEnumerable<VenueAvailabilityResponseDto>> CheckAvailabilityAsync(
            VenueAvailabilityQueryDto query);
    }
}