using Event_and_parking_reservation_system.Models;

namespace Event_and_parking_reservation_system.Interfaces.Services
{
    public interface IJwtTokenService
    {
        string GenerateToken(Customer customer);

        DateTime GetExpirationTime();
    }
}