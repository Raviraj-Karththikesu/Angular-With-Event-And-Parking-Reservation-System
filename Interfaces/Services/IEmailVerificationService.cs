using Event_and_parking_reservation_system.Models;

namespace Event_and_parking_reservation_system.Interfaces.Services
{
    public interface IEmailVerificationService
    {
        Task SendVerificationEmailAsync(Customer customer);

        Task ResendVerificationEmailAsync(string email);

        Task VerifyEmailAsync(string token);
    }
}