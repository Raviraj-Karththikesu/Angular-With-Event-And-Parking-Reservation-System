using Event_and_parking_reservation_system.DTOs.Auth;
using Event_and_parking_reservation_system.Enums;
using Event_and_parking_reservation_system.Exceptions;
using Event_and_parking_reservation_system.Interfaces.Repositories;
using Event_and_parking_reservation_system.Interfaces.Services;
using Event_and_parking_reservation_system.Models;
using Event_and_parking_reservation_system.Services;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;

namespace EventParking.Tests
{
    public class AuthServiceTests
    {
        private readonly Mock<ICustomerRepository>
            _customerRepositoryMock;

        private readonly Mock<IPasswordHasher<Customer>>
            _passwordHasherMock;

        private readonly Mock<IJwtTokenService>
            _jwtTokenServiceMock;

        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _customerRepositoryMock =
                new Mock<ICustomerRepository>();

            _passwordHasherMock =
                new Mock<IPasswordHasher<Customer>>();

            _jwtTokenServiceMock =
                new Mock<IJwtTokenService>();

            _authService = new AuthService(
                _customerRepositoryMock.Object,
                _passwordHasherMock.Object,
                _jwtTokenServiceMock.Object
            );
        }

        [Fact]
        public async Task LoginAsync_WhenCredentialsAreValid_ReturnsAuthResponse()
        {
            LoginRequestDto loginRequest =
                new LoginRequestDto
                {
                    Email = " CUSTOMER@EXAMPLE.COM ",
                    Password = "Password@123"
                };

            Customer customer = CreateCustomer();

            DateTime expirationTime =
                DateTime.UtcNow.AddMinutes(60);

            _customerRepositoryMock
                .Setup(repository =>
                    repository.GetByEmailAsync(
                        "customer@example.com"
                    ))
                .ReturnsAsync(customer);

            _passwordHasherMock
                .Setup(hasher =>
                    hasher.VerifyHashedPassword(
                        customer,
                        customer.PasswordHash,
                        loginRequest.Password
                    ))
                .Returns(
                    PasswordVerificationResult.Success
                );

            _jwtTokenServiceMock
                .Setup(service =>
                    service.GenerateToken(customer))
                .Returns("test-jwt-token");

            _jwtTokenServiceMock
                .Setup(service =>
                    service.GetExpirationTime())
                .Returns(expirationTime);

            AuthResponseDto result =
                await _authService.LoginAsync(
                    loginRequest
                );

            Assert.Equal(1, result.CustomerId);
            Assert.Equal(
                "Test Customer",
                result.FullName
            );
            Assert.Equal(
                "customer@example.com",
                result.Email
            );
            Assert.Equal(
                UserRole.Customer.ToString(),
                result.Role
            );
            Assert.Equal(
                CustomerStatus.Active.ToString(),
                result.Status
            );
            Assert.True(result.EmailVerified);
            Assert.Equal(
                "test-jwt-token",
                result.AccessToken
            );
            Assert.Equal("Bearer", result.TokenType);
            Assert.Equal(
                expirationTime,
                result.ExpiresAt
            );

            _customerRepositoryMock.Verify(
                repository =>
                    repository.GetByEmailAsync(
                        "customer@example.com"
                    ),
                Times.Once
            );

            _jwtTokenServiceMock.Verify(
                service =>
                    service.GenerateToken(customer),
                Times.Once
            );
        }

        [Fact]
        public async Task LoginAsync_WhenEmailDoesNotExist_ThrowsAppException()
        {
            LoginRequestDto loginRequest =
                new LoginRequestDto
                {
                    Email = "UNKNOWN@EXAMPLE.COM",
                    Password = "Password@123"
                };

            _customerRepositoryMock
                .Setup(repository =>
                    repository.GetByEmailAsync(
                        "unknown@example.com"
                    ))
                .ReturnsAsync((Customer?)null);

            AppException exception =
                await Assert.ThrowsAsync<AppException>(
                    () => _authService.LoginAsync(
                        loginRequest
                    )
                );

            Assert.Equal(
                "Invalid email or password.",
                exception.Message
            );

            _passwordHasherMock.Verify(
                hasher =>
                    hasher.VerifyHashedPassword(
                        It.IsAny<Customer>(),
                        It.IsAny<string>(),
                        It.IsAny<string>()
                    ),
                Times.Never
            );

            _jwtTokenServiceMock.Verify(
                service =>
                    service.GenerateToken(
                        It.IsAny<Customer>()
                    ),
                Times.Never
            );
        }

        [Fact]
        public async Task LoginAsync_WhenPasswordIsInvalid_ThrowsAppException()
        {
            LoginRequestDto loginRequest =
                new LoginRequestDto
                {
                    Email = "customer@example.com",
                    Password = "WrongPassword"
                };

            Customer customer = CreateCustomer();

            _customerRepositoryMock
                .Setup(repository =>
                    repository.GetByEmailAsync(
                        "customer@example.com"
                    ))
                .ReturnsAsync(customer);

            _passwordHasherMock
                .Setup(hasher =>
                    hasher.VerifyHashedPassword(
                        customer,
                        customer.PasswordHash,
                        loginRequest.Password
                    ))
                .Returns(
                    PasswordVerificationResult.Failed
                );

            AppException exception =
                await Assert.ThrowsAsync<AppException>(
                    () => _authService.LoginAsync(
                        loginRequest
                    )
                );

            Assert.Equal(
                "Invalid email or password.",
                exception.Message
            );

            _jwtTokenServiceMock.Verify(
                service =>
                    service.GenerateToken(
                        It.IsAny<Customer>()
                    ),
                Times.Never
            );
        }

        [Fact]
        public async Task LoginAsync_WhenCustomerIsDeactivated_ThrowsAppException()
        {
            LoginRequestDto loginRequest =
                new LoginRequestDto
                {
                    Email = "customer@example.com",
                    Password = "Password@123"
                };

            Customer customer = CreateCustomer();

            customer.Status =
                CustomerStatus.Deactivated;

            _customerRepositoryMock
                .Setup(repository =>
                    repository.GetByEmailAsync(
                        "customer@example.com"
                    ))
                .ReturnsAsync(customer);

            _passwordHasherMock
                .Setup(hasher =>
                    hasher.VerifyHashedPassword(
                        customer,
                        customer.PasswordHash,
                        loginRequest.Password
                    ))
                .Returns(
                    PasswordVerificationResult.Success
                );

            AppException exception =
                await Assert.ThrowsAsync<AppException>(
                    () => _authService.LoginAsync(
                        loginRequest
                    )
                );

            Assert.Equal(
                "This customer account is not active.",
                exception.Message
            );

            _jwtTokenServiceMock.Verify(
                service =>
                    service.GenerateToken(
                        It.IsAny<Customer>()
                    ),
                Times.Never
            );
        }

        [Fact]
        public async Task LoginAsync_WhenEmailIsNotVerified_ThrowsAppException()
        {
            LoginRequestDto loginRequest =
                new LoginRequestDto
                {
                    Email = "customer@example.com",
                    Password = "Password@123"
                };

            Customer customer = CreateCustomer();

            customer.EmailVerified = false;

            _customerRepositoryMock
                .Setup(repository =>
                    repository.GetByEmailAsync(
                        "customer@example.com"
                    ))
                .ReturnsAsync(customer);

            _passwordHasherMock
                .Setup(hasher =>
                    hasher.VerifyHashedPassword(
                        customer,
                        customer.PasswordHash,
                        loginRequest.Password
                    ))
                .Returns(
                    PasswordVerificationResult.Success
                );

            AppException exception =
                await Assert.ThrowsAsync<AppException>(
                    () => _authService.LoginAsync(
                        loginRequest
                    )
                );

            Assert.Equal(
                "Please verify your email address before logging in.",
                exception.Message
            );

            _jwtTokenServiceMock.Verify(
                service =>
                    service.GenerateToken(
                        It.IsAny<Customer>()
                    ),
                Times.Never
            );
        }

        [Fact]
        public async Task LoginAsync_WhenPasswordNeedsRehash_UpdatesPasswordHash()
        {
            LoginRequestDto loginRequest =
                new LoginRequestDto
                {
                    Email = "customer@example.com",
                    Password = "Password@123"
                };

            Customer customer = CreateCustomer();

            _customerRepositoryMock
                .Setup(repository =>
                    repository.GetByEmailAsync(
                        "customer@example.com"
                    ))
                .ReturnsAsync(customer);

            _passwordHasherMock
                .Setup(hasher =>
                    hasher.VerifyHashedPassword(
                        customer,
                        customer.PasswordHash,
                        loginRequest.Password
                    ))
                .Returns(
                    PasswordVerificationResult
                        .SuccessRehashNeeded
                );

            _passwordHasherMock
                .Setup(hasher =>
                    hasher.HashPassword(
                        customer,
                        loginRequest.Password
                    ))
                .Returns("new-hashed-password");

            _customerRepositoryMock
                .Setup(repository =>
                    repository.SaveChangesAsync())
                .ReturnsAsync(true);

            _jwtTokenServiceMock
                .Setup(service =>
                    service.GenerateToken(customer))
                .Returns("test-jwt-token");

            _jwtTokenServiceMock
                .Setup(service =>
                    service.GetExpirationTime())
                .Returns(
                    DateTime.UtcNow.AddMinutes(60)
                );

            AuthResponseDto result =
                await _authService.LoginAsync(
                    loginRequest
                );

            Assert.Equal(
                "new-hashed-password",
                customer.PasswordHash
            );

            Assert.NotNull(customer.UpdatedAt);

            Assert.Equal(
                "test-jwt-token",
                result.AccessToken
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
                PasswordHash = "existing-hashed-password",
                Role = UserRole.Customer,
                Status = CustomerStatus.Active,
                EmailVerified = true,
                CreatedAt = DateTime.UtcNow
            };
        }
    }
}