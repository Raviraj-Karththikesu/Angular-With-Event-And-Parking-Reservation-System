using System.Security.Cryptography;
using System.Text;
using Event_and_parking_reservation_system.Enums;
using Event_and_parking_reservation_system.Exceptions;
using Event_and_parking_reservation_system.Interfaces.Repositories;
using Event_and_parking_reservation_system.Interfaces.Services;
using Event_and_parking_reservation_system.Models;
using Event_and_parking_reservation_system.Options;
using Event_and_parking_reservation_system.Services;
using Moq;
using Xunit;

namespace EventParking.Tests
{
    public class EmailVerificationServiceTests
    {
        private readonly Mock<ICustomerRepository>
            _customerRepositoryMock;

        private readonly Mock<IEmailService>
            _emailServiceMock;

        private readonly EmailVerificationService
            _emailVerificationService;

        public EmailVerificationServiceTests()
        {
            _customerRepositoryMock =
                new Mock<ICustomerRepository>();

            _emailServiceMock =
                new Mock<IEmailService>();

            EmailSettings emailSettings =
                new EmailSettings
                {
                    FrontendBaseUrl =
                        "https://frontend.test",
                    VerificationTokenExpiryHours = 24
                };

            _emailVerificationService =
                new EmailVerificationService(
                    _customerRepositoryMock.Object,
                    _emailServiceMock.Object,
                    Microsoft.Extensions.Options.Options
                        .Create(emailSettings)
                );
        }

        [Fact]
        public async Task SendVerificationEmailAsync_WhenAlreadyVerified_DoesNothing()
        {
            Customer customer =
                CreateCustomer(emailVerified: true);

            await _emailVerificationService
                .SendVerificationEmailAsync(customer);

            _customerRepositoryMock.Verify(
                repository =>
                    repository.Update(
                        It.IsAny<Customer>()
                    ),
                Times.Never
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
        public async Task SendVerificationEmailAsync_WhenUnverified_CreatesTokenAndSendsEmail()
        {
            Customer customer = CreateCustomer();

            DateTime beforeTest = DateTime.UtcNow;
            string? sentMessage = null;

            _customerRepositoryMock
                .Setup(repository =>
                    repository.SaveChangesAsync())
                .ReturnsAsync(true);

            _emailServiceMock
                .Setup(service =>
                    service.SendEmailAsync(
                        customer.Email,
                        "Verify your email address",
                        It.IsAny<string>()
                    ))
                .Callback<string, string, string>(
                    (recipient, subject, message) =>
                    {
                        sentMessage = message;
                    }
                )
                .Returns(Task.CompletedTask);

            await _emailVerificationService
                .SendVerificationEmailAsync(customer);

            DateTime afterTest = DateTime.UtcNow;

            Assert.False(customer.EmailVerified);

            Assert.False(
                string.IsNullOrWhiteSpace(
                    customer.EmailVerificationTokenHash
                )
            );

            Assert.Equal(
                64,
                customer.EmailVerificationTokenHash!.Length
            );

            Assert.NotNull(
                customer.EmailVerificationTokenExpiresAt
            );

            Assert.InRange(
                customer.EmailVerificationTokenExpiresAt!.Value,
                beforeTest.AddHours(24),
                afterTest.AddHours(24)
            );

            Assert.NotNull(customer.UpdatedAt);
            Assert.NotNull(sentMessage);

            Assert.Contains(
                "https://frontend.test/verify-email?token=",
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
                        "Verify your email address",
                        It.IsAny<string>()
                    ),
                Times.Once
            );
        }

        [Fact]
        public async Task SendVerificationEmailAsync_WhenSaveFails_ThrowsAppException()
        {
            Customer customer = CreateCustomer();

            _customerRepositoryMock
                .Setup(repository =>
                    repository.SaveChangesAsync())
                .ReturnsAsync(false);

            AppException exception =
                await Assert.ThrowsAsync<AppException>(
                    () => _emailVerificationService
                        .SendVerificationEmailAsync(
                            customer
                        )
                );

            Assert.Equal(
                "Unable to create the email verification token.",
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
        public async Task ResendVerificationEmailAsync_WhenCustomerExists_SendsNewEmail()
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
                .ReturnsAsync(true);

            _emailServiceMock
                .Setup(service =>
                    service.SendEmailAsync(
                        customer.Email,
                        "Verify your email address",
                        It.IsAny<string>()
                    ))
                .Returns(Task.CompletedTask);

            await _emailVerificationService
                .ResendVerificationEmailAsync(
                    " CUSTOMER@EXAMPLE.COM "
                );

            Assert.NotNull(
                customer.EmailVerificationTokenHash
            );

            _customerRepositoryMock.Verify(
                repository =>
                    repository.GetByEmailAsync(
                        "customer@example.com"
                    ),
                Times.Once
            );

            _emailServiceMock.Verify(
                service =>
                    service.SendEmailAsync(
                        customer.Email,
                        "Verify your email address",
                        It.IsAny<string>()
                    ),
                Times.Once
            );
        }

        [Fact]
        public async Task ResendVerificationEmailAsync_WhenCustomerDoesNotExist_DoesNothing()
        {
            _customerRepositoryMock
                .Setup(repository =>
                    repository.GetByEmailAsync(
                        "unknown@example.com"
                    ))
                .ReturnsAsync((Customer?)null);

            await _emailVerificationService
                .ResendVerificationEmailAsync(
                    "UNKNOWN@EXAMPLE.COM"
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
        public async Task VerifyEmailAsync_WhenTokenIsEmpty_ThrowsAppException()
        {
            AppException exception =
                await Assert.ThrowsAsync<AppException>(
                    () => _emailVerificationService
                        .VerifyEmailAsync(" ")
                );

            Assert.Equal(
                "Verification token is required.",
                exception.Message
            );

            _customerRepositoryMock.Verify(
                repository =>
                    repository
                        .GetByEmailVerificationTokenHashAsync(
                            It.IsAny<string>()
                        ),
                Times.Never
            );
        }

        [Fact]
        public async Task VerifyEmailAsync_WhenTokenIsInvalid_ThrowsAppException()
        {
            const string token = "invalid-token";

            string expectedHash = HashToken(token);

            _customerRepositoryMock
                .Setup(repository =>
                    repository
                        .GetByEmailVerificationTokenHashAsync(
                            expectedHash
                        ))
                .ReturnsAsync((Customer?)null);

            AppException exception =
                await Assert.ThrowsAsync<AppException>(
                    () => _emailVerificationService
                        .VerifyEmailAsync(token)
                );

            Assert.Equal(
                "Invalid or expired verification token.",
                exception.Message
            );

            _customerRepositoryMock.Verify(
                repository =>
                    repository
                        .GetByEmailVerificationTokenHashAsync(
                            expectedHash
                        ),
                Times.Once
            );

            _customerRepositoryMock.Verify(
                repository =>
                    repository.SaveChangesAsync(),
                Times.Never
            );
        }

        [Fact]
        public async Task VerifyEmailAsync_WhenTokenIsExpired_ThrowsAppException()
        {
            const string token = "expired-token";

            string tokenHash = HashToken(token);

            Customer customer = CreateCustomer();

            customer.EmailVerificationTokenHash =
                tokenHash;

            customer.EmailVerificationTokenExpiresAt =
                DateTime.UtcNow.AddMinutes(-10);

            _customerRepositoryMock
                .Setup(repository =>
                    repository
                        .GetByEmailVerificationTokenHashAsync(
                            tokenHash
                        ))
                .ReturnsAsync(customer);

            AppException exception =
                await Assert.ThrowsAsync<AppException>(
                    () => _emailVerificationService
                        .VerifyEmailAsync(token)
                );

            Assert.Equal(
                "Invalid or expired verification token.",
                exception.Message
            );

            Assert.False(customer.EmailVerified);

            _customerRepositoryMock.Verify(
                repository =>
                    repository.SaveChangesAsync(),
                Times.Never
            );
        }

        [Fact]
        public async Task VerifyEmailAsync_WhenTokenIsValid_VerifiesCustomer()
        {
            const string token = "valid-token";

            string tokenHash = HashToken(token);

            Customer customer = CreateCustomer();

            customer.EmailVerificationTokenHash =
                tokenHash;

            customer.EmailVerificationTokenExpiresAt =
                DateTime.UtcNow.AddHours(1);

            _customerRepositoryMock
                .Setup(repository =>
                    repository
                        .GetByEmailVerificationTokenHashAsync(
                            tokenHash
                        ))
                .ReturnsAsync(customer);

            _customerRepositoryMock
                .Setup(repository =>
                    repository.SaveChangesAsync())
                .ReturnsAsync(true);

            await _emailVerificationService
                .VerifyEmailAsync(token);

            Assert.True(customer.EmailVerified);

            Assert.Null(
                customer.EmailVerificationTokenHash
            );

            Assert.Null(
                customer.EmailVerificationTokenExpiresAt
            );

            Assert.NotNull(customer.UpdatedAt);

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

        private static Customer CreateCustomer(
            bool emailVerified = false)
        {
            return new Customer
            {
                Id = 1,
                FullName = "Test Customer",
                Email = "customer@example.com",
                PhoneNumber = "0771234567",
                PasswordHash = "hashed-password",
                Role = UserRole.Customer,
                Status = CustomerStatus.Active,
                EmailVerified = emailVerified,
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