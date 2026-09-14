using System.Security.Cryptography;
using System.Text;
using Event_and_parking_reservation_system.Exceptions;
using Event_and_parking_reservation_system.Interfaces.Repositories;
using Event_and_parking_reservation_system.Interfaces.Services;
using Event_and_parking_reservation_system.Models;
using Event_and_parking_reservation_system.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Event_and_parking_reservation_system.Services
{
    public class PasswordResetService :
        IPasswordResetService
    {
        private readonly ICustomerRepository
            _customerRepository;

        private readonly IEmailService _emailService;

        private readonly IPasswordHasher<Customer>
            _passwordHasher;

        private readonly EmailSettings _emailSettings;

        public PasswordResetService(
            ICustomerRepository customerRepository,
            IEmailService emailService,
            IPasswordHasher<Customer> passwordHasher,
            IOptions<EmailSettings> emailOptions)
        {
            _customerRepository = customerRepository;
            _emailService = emailService;
            _passwordHasher = passwordHasher;
            _emailSettings = emailOptions.Value;
        }

        public async Task RequestPasswordResetAsync(
            string email)
        {
            string normalizedEmail =
                email.Trim().ToLowerInvariant();

            Customer? customer =
                await _customerRepository.GetByEmailAsync(
                    normalizedEmail
                );

            if (customer is null)
            {
                return;
            }

            string rawToken = GenerateSecureToken();
            string tokenHash = HashToken(rawToken);

            customer.PasswordResetTokenHash =
                tokenHash;

            customer.PasswordResetTokenExpiresAt =
                DateTime.UtcNow.AddMinutes(
                    _emailSettings
                        .PasswordResetTokenExpiryMinutes
                );

            customer.UpdatedAt = DateTime.UtcNow;

            _customerRepository.Update(customer);

            bool saved =
                await _customerRepository
                    .SaveChangesAsync();

            if (!saved)
            {
                throw new AppException(
                    "Unable to create the password reset token.",
                    StatusCodes.Status500InternalServerError
                );
            }

            string resetLink =
                $"{_emailSettings.FrontendBaseUrl.TrimEnd('/')}" +
                $"/reset-password?token=" +
                $"{Uri.EscapeDataString(rawToken)}";

            string message =
                "<h2>Password Reset</h2>" +
                "<p>A password reset was requested " +
                "for your account.</p>" +
                "<p>Use the following link to reset " +
                "your password:</p>" +
                $"<p><a href=\"{resetLink}\">" +
                "Reset Password</a></p>" +
                $"<p>This link expires in " +
                $"{_emailSettings.PasswordResetTokenExpiryMinutes} " +
                "minutes.</p>" +
                "<p>If you did not request a password reset, " +
                "you can ignore this message.</p>";

            await _emailService.SendEmailAsync(
                customer.Email,
                "Reset your password",
                message
            );
        }

        public async Task ResetPasswordAsync(
            string token,
            string newPassword)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new AppException(
                    "Password reset token is required.",
                    StatusCodes.Status400BadRequest
                );
            }

            if (string.IsNullOrWhiteSpace(newPassword))
            {
                throw new AppException(
                    "New password is required.",
                    StatusCodes.Status400BadRequest
                );
            }

            string tokenHash =
                HashToken(token.Trim());

            Customer? customer =
                await _customerRepository
                    .GetByPasswordResetTokenHashAsync(
                        tokenHash
                    );

            if (customer is null)
            {
                throw new AppException(
                    "Invalid or expired password reset token.",
                    StatusCodes.Status400BadRequest
                );
            }

            if (customer.PasswordResetTokenExpiresAt is null ||
                customer.PasswordResetTokenExpiresAt <=
                DateTime.UtcNow)
            {
                throw new AppException(
                    "Invalid or expired password reset token.",
                    StatusCodes.Status400BadRequest
                );
            }

            customer.PasswordHash =
                _passwordHasher.HashPassword(
                    customer,
                    newPassword
                );

            customer.PasswordResetTokenHash = null;
            customer.PasswordResetTokenExpiresAt = null;
            customer.UpdatedAt = DateTime.UtcNow;

            _customerRepository.Update(customer);

            bool saved =
                await _customerRepository
                    .SaveChangesAsync();

            if (!saved)
            {
                throw new AppException(
                    "Password reset failed.",
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