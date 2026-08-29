using Event_and_parking_reservation_system.DTOs.Auth;

namespace Event_and_parking_reservation_system.Interfaces.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto> LoginAsync(
            LoginRequestDto loginRequestDto
        );
    }
}