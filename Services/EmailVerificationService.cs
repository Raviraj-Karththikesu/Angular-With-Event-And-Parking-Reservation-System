using System.Security.Cryptography;
using System.Text;
using Event_and_parking_reservation_system.Exceptions;
using Event_and_parking_reservation_system.Interfaces.Repositories;
using Event_and_parking_reservation_system.Interfaces.Services;
using Event_and_parking_reservation_system.Models;
using Event_and_parking_reservation_system.Options;
using Microsoft.Extensions.Options;

namespace Event_and_parking_reservation_system.Services
{
    public class EmailVerificationService : IEmailVerificationService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IEmailService _emailService;
        private readonly EmailSettings _emailSettings;

        public EmailVerificationService(
            ICustomerRepository customerRepository,
            IEmailService emailService,
            IOptions<EmailSettings> emailOptions)
        {
            _customerRepository = customerRepository;
            _emailService = emailService;
            _emailSettings = emailOptions.Value;
        }

        public async Task SendVerificationEmailAsync(Customer customer)
        {
            if (customer.EmailVerified)
            {
                return;
            }

            
            string rawToken = GenerateSecureToken();

            
            string tokenHash = HashToken(rawToken);

            customer.EmailVerificationTokenHash = tokenHash;
            customer.EmailVerificationTokenExpiresAt =
                DateTime.UtcNow.AddHours(
                    _emailSettings.VerificationTokenExpiryHours
                );

            customer.UpdatedAt = DateTime.UtcNow;

            _customerRepository.Update(customer);

            bool saved =
                await _customerRepository.SaveChangesAsync();

            if (!saved)
            {
                throw new AppException(
                    "Unable to create the email verification token.",
                    StatusCodes.Status500InternalServerError
                );
            }

            string verificationLink =
                $"{_emailSettings.FrontendBaseUrl.TrimEnd('/')}" +
                $"/verify-email?token={Uri.EscapeDataString(rawToken)}";

            string message =
                "<h2>Email Verification</h2>" +
                "<p>Thank you for registering.</p>" +
                "<p>Use the following link to verify your email address:</p>" +
                $"<p><a href=\"{verificationLink}\">" +
                "Verify Email Address</a></p>" +
                $"<p>This link expires in " +
                $"{_emailSettings.VerificationTokenExpiryHours} hours.</p>" +
                "<p>If you did not create this account, " +
                "you can ignore this message.</p>";

            await _emailService.SendEmailAsync(
                customer.Email,
                "Verify your email address",
                message
            );
        }

        public async Task ResendVerificationEmailAsync(string email)
        {
            string normalizedEmail = email.Trim().ToLowerInvariant();

            Customer? customer =
                await _customerRepository.GetByEmailAsync(
                    normalizedEmail
                );

            
            if (customer is null || customer.EmailVerified)
            {
                return;
            }

            
            await SendVerificationEmailAsync(customer);
        }

        public async Task VerifyEmailAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new AppException(
                    "Verification token is required.",
                    StatusCodes.Status400BadRequest
                );
            }

            string tokenHash = HashToken(token.Trim());

            Customer? customer =
                await _customerRepository
                    .GetByEmailVerificationTokenHashAsync(
                        tokenHash
                    );

            if (customer is null)
            {
                throw new AppException(
                    "Invalid or expired verification token.",
                    StatusCodes.Status400BadRequest
                );
            }

            if (customer.EmailVerificationTokenExpiresAt is null ||
                customer.EmailVerificationTokenExpiresAt <= DateTime.UtcNow)
            {
                throw new AppException(
                    "Invalid or expired verification token.",
                    StatusCodes.Status400BadRequest
                );
            }

            customer.EmailVerified = true;

            
            customer.EmailVerificationTokenHash = null;
            customer.EmailVerificationTokenExpiresAt = null;
            customer.UpdatedAt = DateTime.UtcNow;

            _customerRepository.Update(customer);

            bool saved =
                await _customerRepository.SaveChangesAsync();

            if (!saved)
            {
                throw new AppException(
                    "Email verification failed.",
                    StatusCodes.Status500InternalServerError
                );
            }
        }

        private static string GenerateSecureToken()
        {
            byte[] tokenBytes =
                RandomNumberGenerator.GetBytes(32);

            return Convert.ToHexString(tokenBytes);
        }

        private static string HashToken(string token)
        {
            byte[] tokenBytes =
                Encoding.UTF8.GetBytes(token);

            byte[] hashBytes =
                SHA256.HashData(tokenBytes);

            return Convert.ToHexString(hashBytes);
        }
    }
}