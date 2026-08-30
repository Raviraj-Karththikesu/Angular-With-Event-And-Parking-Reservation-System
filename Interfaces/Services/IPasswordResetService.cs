namespace Event_and_parking_reservation_system
    .Interfaces.Services
{
    public interface IPasswordResetService
    {
        Task RequestPasswordResetAsync(
            string email
        );

        Task ResetPasswordAsync(
            string token,
            string newPassword
        );
    }
}