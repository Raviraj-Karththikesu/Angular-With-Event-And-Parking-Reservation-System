namespace Event_and_parking_reservation_system.Interfaces.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(
            string recipientEmail,
            string subject,
            string htmlMessage
        );
    }
}