using Event_and_parking_reservation_system.Interfaces.Services;
using Event_and_parking_reservation_system.Options;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Event_and_parking_reservation_system.Services
{
    public sealed class SmtpEmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;

        public SmtpEmailService(
            IOptions<EmailSettings> emailOptions)
        {
            _emailSettings = emailOptions.Value;
        }

        public async Task SendEmailAsync(
            string recipientEmail,
            string subject,
            string htmlMessage)
        {
            ValidateSettings();

            MimeMessage message = new();

            message.From.Add(
                new MailboxAddress(
                    _emailSettings.FromName,
                    _emailSettings.FromAddress
                )
            );

            message.To.Add(
                MailboxAddress.Parse(recipientEmail)
            );

            message.Subject = subject;

            BodyBuilder bodyBuilder = new()
            {
                HtmlBody = htmlMessage
            };

            message.Body = bodyBuilder.ToMessageBody();

            using SmtpClient client = new();

            await client.ConnectAsync(
                _emailSettings.SmtpHost,
                _emailSettings.SmtpPort,
                SecureSocketOptions.StartTls
            );

            await client.AuthenticateAsync(
                _emailSettings.Username,
                _emailSettings.Password
            );

            await client.SendAsync(message);

            await client.DisconnectAsync(true);
        }

        private void ValidateSettings()
        {
            if (string.IsNullOrWhiteSpace(
                    _emailSettings.FromAddress) ||
                string.IsNullOrWhiteSpace(
                    _emailSettings.Username) ||
                string.IsNullOrWhiteSpace(
                    _emailSettings.Password))
            {
                throw new InvalidOperationException(
                    "SMTP email settings are not configured."
                );
            }
        }
    }
}