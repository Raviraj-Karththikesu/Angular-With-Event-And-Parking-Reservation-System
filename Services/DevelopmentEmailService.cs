using Event_and_parking_reservation_system.Interfaces.Services;

namespace Event_and_parking_reservation_system.Services
{
    public class DevelopmentEmailService : IEmailService
    {
        private readonly ILogger<DevelopmentEmailService> _logger;

        public DevelopmentEmailService(
            ILogger<DevelopmentEmailService> logger)
        {
            _logger = logger;
        }

        public Task SendEmailAsync(
            string recipientEmail,
            string subject,
            string htmlMessage)
        {
            _logger.LogInformation(
                "----- DEVELOPMENT EMAIL -----\n" +
                "To: {RecipientEmail}\n" +
                "Subject: {Subject}\n" +
                "Message:\n{HtmlMessage}\n" +
                "-----------------------------",
                recipientEmail,
                subject,
                htmlMessage
            );

            return Task.CompletedTask;
        }
    }
}