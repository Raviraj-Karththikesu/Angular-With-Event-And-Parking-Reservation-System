using Event_and_parking_reservation_system.DTOs.Events;

namespace Event_and_parking_reservation_system.Interfaces.Services
{
    public interface IEventService
    {
        Task<IEnumerable<EventResponseDto>> GetAllAsync(EventFilterDto filter);

        Task<EventResponseDto> GetByIdAsync(int id);

        Task<EventResponseDto> CreateAsync(CreateEventDto dto);

        Task<EventResponseDto> UpdateAsync(
            int id,
            UpdateEventDto dto);

        Task DeleteAsync(int id);
    }
}