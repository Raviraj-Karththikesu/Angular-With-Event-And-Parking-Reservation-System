using System.Security.Cryptography;
using System.Text;
using Event_and_parking_reservation_system.Enums;
using Event_and_parking_reservation_system.Exceptions;
using Event_and_parking_reservation_system.Interfaces.Repositories;
using Event_and_parking_reservation_system.Interfaces.Services;
using Event_and_parking_reservation_system.Models;
using Event_and_parking_reservation_system.Options;
using Event_and_parking_reservation_system.Services;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;

namespace EventParking.Tests
{
    public class PasswordResetServiceTests
    {
        private readonly Mock<ICustomerRepository>
            _customerRepositoryMock;

        private readonly Mock<IEmailService>
            _emailServiceMock;

        private readonly Mock<IPasswordHasher<Customer>>
            _passwordHasherMock;

        private readonly PasswordResetService
            _passwordResetService;

        public PasswordResetServiceTests()
        {
            _customerRepositoryMock =
                new Mock<ICustomerRepository>();

            _emailServiceMock =
                new Mock<IEmailService>();

            _passwordHasherMock =
                new Mock<IPasswordHasher<Customer>>();

            EmailSettings emailSettings =
                new EmailSettings
                {
                    FrontendBaseUrl =
                        "https://frontend.test",
                    PasswordResetTokenExpiryMinutes = 60
                };

            _passwordResetService =
                new PasswordResetService(
                    _customerRepositoryMock.Object,
                    _emailServiceMock.Object,
                    _passwordHasherMock.Object,
                    Microsoft.Extensions.Options.Options
                        .Create(emailSettings)
                );
        }

        [Fact]
        public async Task RequestPasswordResetAsync_WhenCustomerDoesNotExist_DoesNothing()
        {
            _customerRepositoryMock
                .Setup(repository =>
                    repository.GetByEmailAsync(
                        "unknown@example.com"
                    ))
                .ReturnsAsync((Customer?)null);

            await _passwordResetService
                .RequestPasswordResetAsync(
                    " UNKNOWN@EXAMPLE.COM "
                );

            _customerRepositoryMock.Verify(
                repository =>
                    repository.GetByEmailAsync(
                        "unknown@example.com"
                    ),
                Times.Once
            );

            _customerRepositoryMock.Verify(
                repository =>
                    repository.SaveChangesAsync(),
                Times.Never
            );

            _emailServiceMock.Verify(
                service =>
                    service.SendEmailAsync(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<string>()
                    ),
                Times.Never
            );
        }

        [Fact]
        public async Task RequestPasswordResetAsync_WhenCustomerExists_CreatesTokenAndSendsEmail()
        {
            Customer customer = CreateCustomer();

            DateTime beforeTest = DateTime.UtcNow;
            string? sentMessage = null;

            _customerRepositoryMock
                .Setup(repository =>
                    repository.GetByEmailAsync(
                        "customer@example.com"
                    ))
                .ReturnsAsync(customer);

            _customerRepositoryMock
                .Setup(repository =>
                    repository.SaveChangesAsync())
                .ReturnsAsync(true);

            _emailServiceMock
                .Setup(service =>
                    service.SendEmailAsync(
                        customer.Email,
                        "Reset your password",
                        It.IsAny<string>()
                    ))
                .Callback<string, string, string>(
                    (recipient, subject, message) =>
                    {
                        sentMessage = message;
                    }
                )
                .Returns(Task.CompletedTask);

            await _passwordResetService
                .RequestPasswordResetAsync(
                    " CUSTOMER@EXAMPLE.COM "
                );

            DateTime afterTest = DateTime.UtcNow;

            Assert.False(
                string.IsNullOrWhiteSpace(
                    customer.PasswordResetTokenHash
                )
            );

            Assert.Equal(
                64,
                customer.PasswordResetTokenHash!.Length
            );

            Assert.NotNull(
                customer.PasswordResetTokenExpiresAt
            );

            Assert.InRange(
                customer.PasswordResetTokenExpiresAt!.Value,
                beforeTest.AddMinutes(60),
                afterTest.AddMinutes(60)
            );

            Assert.NotNull(customer.UpdatedAt);
            Assert.NotNull(sentMessage);

            Assert.Contains(
                "https://frontend.test/reset-password?token=",
                sentMessage!
            );

            _customerRepositoryMock.Verify(
                repository =>
                    repository.Update(customer),
                Times.Once
            );

            _customerRepositoryMock.Verify(
                repository =>
                    repository.SaveChangesAsync(),
                Times.Once
            );

            _emailServiceMock.Verify(
                service =>
                    service.SendEmailAsync(
                        customer.Email,
                        "Reset your password",
                        It.IsAny<string>()
                    ),
                Times.Once
            );
        }

        [Fact]
        public async Task RequestPasswordResetAsync_WhenSaveFails_ThrowsAppException()
        {
            Customer customer = CreateCustomer();

            _customerRepositoryMock
                .Setup(repository =>
                    repository.GetByEmailAsync(
                        "customer@example.com"
                    ))
                .ReturnsAsync(customer);

            _customerRepositoryMock
                .Setup(repository =>
                    repository.SaveChangesAsync())
                .ReturnsAsync(false);

            AppException exception =
                await Assert.ThrowsAsync<AppException>(
                    () => _passwordResetService
                        .RequestPasswordResetAsync(
                            "customer@example.com"
                        )
                );

            Assert.Equal(
                "Unable to create the password reset token.",
                exception.Message
            );

            _emailServiceMock.Verify(
                service =>
                    service.SendEmailAsync(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<string>()
                    ),
                Times.Never
            );
        }

        [Fact]
        public async Task ResetPasswordAsync_WhenTokenIsEmpty_ThrowsAppException()
        {
            AppException exception =
                await Assert.ThrowsAsync<AppException>(
                    () => _passwordResetService
                        .ResetPasswordAsync(
                            " ",
                            "NewPassword@123"
                        )
                );

            Assert.Equal(
                "Password reset token is required.",
                exception.Message
            );

            _customerRepositoryMock.Verify(
                repository =>
                    repository
                        .GetByPasswordResetTokenHashAsync(
                            It.IsAny<string>()
                        ),
                Times.Never
            );
        }

        [Fact]
        public async Task ResetPasswordAsync_WhenPasswordIsEmpty_ThrowsAppException()
        {
            AppException exception =
                await Assert.ThrowsAsync<AppException>(
                    () => _passwordResetService
                        .ResetPasswordAsync(
                            "valid-token",
                            " "
                        )
                );

            Assert.Equal(
                "New password is required.",
                exception.Message
            );

            _customerRepositoryMock.Verify(
                repository =>
                    repository
                        .GetByPasswordResetTokenHashAsync(
                            It.IsAny<string>()
                        ),
                Times.Never
            );
        }

        [Fact]
        public async Task ResetPasswordAsync_WhenTokenIsInvalid_ThrowsAppException()
        {
            const string token = "invalid-token";

            string tokenHash = HashToken(token);

            _customerRepositoryMock
                .Setup(repository =>
                    repository
                        .GetByPasswordResetTokenHashAsync(
                            tokenHash
                        ))
                .ReturnsAsync((Customer?)null);

            AppException exception =
                await Assert.ThrowsAsync<AppException>(
                    () => _passwordResetService
                        .ResetPasswordAsync(
                            token,
                            "NewPassword@123"
                        )
                );

            Assert.Equal(
                "Invalid or expired password reset token.",
                exception.Message
            );

            _customerRepositoryMock.Verify(
                repository =>
                    repository.SaveChangesAsync(),
                Times.Never
            );
        }

        [Fact]
        public async Task ResetPasswordAsync_WhenTokenIsExpired_ThrowsAppException()
        {
            const string token = "expired-token";

            string tokenHash = HashToken(token);

            Customer customer = CreateCustomer();

            customer.PasswordResetTokenHash =
                tokenHash;

            customer.PasswordResetTokenExpiresAt =
                DateTime.UtcNow.AddMinutes(-10);

            _customerRepositoryMock
                .Setup(repository =>
                    repository
                        .GetByPasswordResetTokenHashAsync(
                            tokenHash
                        ))
                .ReturnsAsync(customer);

            AppException exception =
                await Assert.ThrowsAsync<AppException>(
                    () => _passwordResetService
                        .ResetPasswordAsync(
                            token,
                            "NewPassword@123"
                        )
                );

            Assert.Equal(
                "Invalid or expired password reset token.",
                exception.Message
            );

            Assert.Equal(
                "old-hashed-password",
                customer.PasswordHash
            );

            _passwordHasherMock.Verify(
                hasher =>
                    hasher.HashPassword(
                        It.IsAny<Customer>(),
                        It.IsAny<string>()
                    ),
                Times.Never
            );

            _customerRepositoryMock.Verify(
                repository =>
                    repository.SaveChangesAsync(),
                Times.Never
            );
        }

        [Fact]
        public async Task ResetPasswordAsync_WhenTokenIsValid_UpdatesPassword()
        {
            const string token = "valid-token";
            const string newPassword =
                "NewPassword@123";

            string tokenHash = HashToken(token);

            Customer customer = CreateCustomer();

            customer.PasswordResetTokenHash =
                tokenHash;

            customer.PasswordResetTokenExpiresAt =
                DateTime.UtcNow.AddMinutes(30);

            _customerRepositoryMock
                .Setup(repository =>
                    repository
                        .GetByPasswordResetTokenHashAsync(
                            tokenHash
                        ))
                .ReturnsAsync(customer);

            _passwordHasherMock
                .Setup(hasher =>
                    hasher.HashPassword(
                        customer,
                        newPassword
                    ))
                .Returns("new-hashed-password");

            _customerRepositoryMock
                .Setup(repository =>
                    repository.SaveChangesAsync())
                .ReturnsAsync(true);

            await _passwordResetService
                .ResetPasswordAsync(
                    token,
                    newPassword
                );

            Assert.Equal(
                "new-hashed-password",
                customer.PasswordHash
            );

            Assert.Null(
                customer.PasswordResetTokenHash
            );

            Assert.Null(
                customer.PasswordResetTokenExpiresAt
            );

            Assert.NotNull(customer.UpdatedAt);

            _passwordHasherMock.Verify(
                hasher =>
                    hasher.HashPassword(
                        customer,
                        newPassword
                    ),
                Times.Once
            );

            _customerRepositoryMock.Verify(
                repository =>
                    repository.Update(customer),
                Times.Once
            );

            _customerRepositoryMock.Verify(
                repository =>
                    repository.SaveChangesAsync(),
                Times.Once
            );
        }

        [Fact]
        public async Task ResetPasswordAsync_WhenSaveFails_ThrowsAppException()
        {
            const string token = "valid-token";
            const string newPassword =
                "NewPassword@123";

            string tokenHash = HashToken(token);

            Customer customer = CreateCustomer();

            customer.PasswordResetTokenHash =
                tokenHash;

            customer.PasswordResetTokenExpiresAt =
                DateTime.UtcNow.AddMinutes(30);

            _customerRepositoryMock
                .Setup(repository =>
                    repository
                        .GetByPasswordResetTokenHashAsync(
                            tokenHash
                        ))
                .ReturnsAsync(customer);

            _passwordHasherMock
                .Setup(hasher =>
                    hasher.HashPassword(
                        customer,
                        newPassword
                    ))
                .Returns("new-hashed-password");

            _customerRepositoryMock
                .Setup(repository =>
                    repository.SaveChangesAsync())
                .ReturnsAsync(false);

            AppException exception =
                await Assert.ThrowsAsync<AppException>(
                    () => _passwordResetService
                        .ResetPasswordAsync(
                            token,
                            newPassword
                        )
                );

            Assert.Equal(
                "Password reset failed.",
                exception.Message
            );

            _customerRepositoryMock.Verify(
                repository =>
                    repository.Update(customer),
                Times.Once
            );

            _customerRepositoryMock.Verify(
                repository =>
                    repository.SaveChangesAsync(),
                Times.Once
            );
        }

        private static Customer CreateCustomer()
        {
            return new Customer
            {
                Id = 1,
                FullName = "Test Customer",
                Email = "customer@example.com",
                PhoneNumber = "0771234567",
                PasswordHash = "old-hashed-password",
                Role = UserRole.Customer,
                Status = CustomerStatus.Active,
                EmailVerified = true,
                CreatedAt = DateTime.UtcNow
            };
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