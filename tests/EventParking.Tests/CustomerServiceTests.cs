using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Event_and_parking_reservation_system.DTOs.Customers;
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
    public class CustomerServiceTests
    {
        private readonly Mock<ICustomerRepository>
            _customerRepositoryMock;

        private readonly Mock<IPasswordHasher<Customer>>
            _passwordHasherMock;

        private readonly Mock<IEmailVerificationService>
            _emailVerificationServiceMock;

        private readonly CustomerService _customerService;

        public CustomerServiceTests()
        {
            _customerRepositoryMock =
                new Mock<ICustomerRepository>();

            _passwordHasherMock =
                new Mock<IPasswordHasher<Customer>>();

            _emailVerificationServiceMock =
                new Mock<IEmailVerificationService>();

            _customerService = new CustomerService(
                _customerRepositoryMock.Object,
                _passwordHasherMock.Object,
                _emailVerificationServiceMock.Object
            );
        }

        [Fact]
        public async Task GetByIdAsync_WhenCustomerExists_ReturnsCustomerWithBookingSummary()
        {
            DateTime createdAt = DateTime.UtcNow;
            DateTime now = DateTime.UtcNow;

            Customer customer = new Customer
            {
                Id = 1,
                FullName = "Test Customer",
                Email = "customer@example.com",
                PhoneNumber = "0771234567",
                Role = UserRole.Customer,
                Status = CustomerStatus.Active,
                EmailVerified = true,
                CreatedAt = createdAt,

                Bookings = new List<Booking>
                {
                    new Booking
                    {
                        Status = BookingStatus.Pending,
                        Event = new Event
                        {
                            EndDateTime =
                                now.AddDays(2)
                        }
                    },

                    new Booking
                    {
                        Status = BookingStatus.Confirmed,
                        Event = new Event
                        {
                            EndDateTime =
                                now.AddDays(3)
                        }
                    },

                    new Booking
                    {
                        Status = BookingStatus.Confirmed,
                        Event = new Event
                        {
                            EndDateTime =
                                now.AddDays(-1)
                        }
                    },

                    new Booking
                    {
                        Status = BookingStatus.Cancelled,
                        Event = new Event
                        {
                            EndDateTime =
                                now.AddDays(5)
                        }
                    },

                    new Booking
                    {
                        Status = BookingStatus.Expired,
                        Event = new Event
                        {
                            EndDateTime =
                                now.AddDays(5)
                        }
                    }
                }
            };

            _customerRepositoryMock
                .Setup(repository =>
                    repository
                        .GetByIdWithBookingsAsync(1))
                .ReturnsAsync(customer);

            CustomerResponseDto? result =
                await _customerService
                    .GetByIdAsync(1);

            Assert.NotNull(result);

            Assert.Equal(
                1,
                result.CustomerId
            );

            Assert.Equal(
                "Test Customer",
                result.FullName
            );

            Assert.Equal(
                "customer@example.com",
                result.Email
            );

            Assert.Equal(
                "0771234567",
                result.PhoneNumber
            );

            Assert.True(result.EmailVerified);

            Assert.Equal(
                createdAt,
                result.CreatedAt
            );

            Assert.Equal(
                5,
                result.BookingSummary.TotalBookings
            );

            Assert.Equal(
                1,
                result.BookingSummary.PendingBookings
            );

            Assert.Equal(
                2,
                result.BookingSummary.ConfirmedBookings
            );

            Assert.Equal(
                1,
                result.BookingSummary.CancelledBookings
            );

            Assert.Equal(
                1,
                result.BookingSummary.ExpiredBookings
            );

            Assert.Equal(
                2,
                result.BookingSummary
                    .ActiveUpcomingBookings
            );

            _customerRepositoryMock.Verify(
                repository =>
                    repository
                        .GetByIdWithBookingsAsync(1),
                Times.Once
            );
        }

        [Fact]
        public async Task GetByIdAsync_WhenCustomerDoesNotExist_ReturnsNull()
        {
            const int customerId = 999;

            _customerRepositoryMock
                .Setup(repository =>
                    repository
                        .GetByIdWithBookingsAsync(
                            customerId
                        ))
                .ReturnsAsync((Customer?)null);

            CustomerResponseDto? result =
                await _customerService
                    .GetByIdAsync(customerId);

            Assert.Null(result);

            _customerRepositoryMock.Verify(
                repository =>
                    repository
                        .GetByIdWithBookingsAsync(
                            customerId
                        ),
                Times.Once
            );
        }

        [Fact]
        public async Task RegisterAsync_WhenEmailAlreadyExists_ThrowsConflictException()
        {
            RegisterCustomerDto registerDto =
                new RegisterCustomerDto
                {
                    FullName = "Existing Customer",
                    Email = "EXISTING@EXAMPLE.COM",
                    PhoneNumber = "0771234567",
                    Password = "Password@123"
                };

            _customerRepositoryMock
                .Setup(repository =>
                    repository.EmailExistsAsync(
                        "existing@example.com"
                    ))
                .ReturnsAsync(true);

            ConflictException exception =
                await Assert.ThrowsAsync<
                    ConflictException>(
                    () => _customerService
                        .RegisterAsync(registerDto)
                );

            Assert.Equal(
                "A customer with this email already exists.",
                exception.Message
            );

            _customerRepositoryMock.Verify(
                repository =>
                    repository.AddAsync(
                        It.IsAny<Customer>()
                    ),
                Times.Never
            );

            _customerRepositoryMock.Verify(
                repository =>
                    repository.SaveChangesAsync(),
                Times.Never
            );

            _emailVerificationServiceMock.Verify(
                service =>
                    service.SendVerificationEmailAsync(
                        It.IsAny<Customer>()
                    ),
                Times.Never
            );
        }

        [Fact]
        public async Task DeactivateAsync_WhenCustomerIsActive_DeactivatesCustomer()
        {
            Customer customer = new Customer
            {
                Id = 5,
                FullName = "Active Customer",
                Email = "active@example.com",
                Role = UserRole.Customer,
                Status = CustomerStatus.Active,
                EmailVerified = true,
                CreatedAt = DateTime.UtcNow
            };

            _customerRepositoryMock
                .Setup(repository =>
                    repository.GetByIdAsync(5))
                .ReturnsAsync(customer);

            _customerRepositoryMock
                .Setup(repository =>
                    repository
                        .HasActiveFutureBookingsAsync(
                            5,
                            It.IsAny<DateTime>()
                        ))
                .ReturnsAsync(false);

            _customerRepositoryMock
                .Setup(repository =>
                    repository.SaveChangesAsync())
                .ReturnsAsync(true);

            CustomerResponseDto result =
                await _customerService
                    .DeactivateAsync(5);

            Assert.Equal(
                CustomerStatus.Deactivated,
                customer.Status
            );

            Assert.Equal(
                CustomerStatus.Deactivated.ToString(),
                result.Status
            );

            Assert.NotNull(customer.UpdatedAt);

            _customerRepositoryMock.Verify(
                repository =>
                    repository.GetByIdAsync(5),
                Times.Once
            );

            _customerRepositoryMock.Verify(
                repository =>
                    repository
                        .HasActiveFutureBookingsAsync(
                            5,
                            It.IsAny<DateTime>()
                        ),
                Times.Once
            );

            _customerRepositoryMock.Verify(
                repository =>
                    repository.SaveChangesAsync(),
                Times.Once
            );
        }

        [Fact]
        public async Task DeactivateAsync_WhenActiveFutureBookingExists_ThrowsConflictException()
        {
            Customer customer = new Customer
            {
                Id = 5,
                FullName = "Booked Customer",
                Email = "booked@example.com",
                Role = UserRole.Customer,
                Status = CustomerStatus.Active,
                EmailVerified = true,
                CreatedAt = DateTime.UtcNow
            };

            _customerRepositoryMock
                .Setup(repository =>
                    repository.GetByIdAsync(5))
                .ReturnsAsync(customer);

            _customerRepositoryMock
                .Setup(repository =>
                    repository
                        .HasActiveFutureBookingsAsync(
                            5,
                            It.IsAny<DateTime>()
                        ))
                .ReturnsAsync(true);

            ConflictException exception =
                await Assert.ThrowsAsync<
                    ConflictException>(
                    () => _customerService
                        .DeactivateAsync(5)
                );

            Assert.Equal(
                "A customer with active or upcoming bookings cannot be deactivated.",
                exception.Message
            );

            Assert.Equal(
                CustomerStatus.Active,
                customer.Status
            );

            _customerRepositoryMock.Verify(
                repository =>
                    repository.SaveChangesAsync(),
                Times.Never
            );
        }

        [Fact]
        public async Task ReactivateAsync_WhenCustomerIsDeactivated_ReactivatesCustomer()
        {
            Customer customer = new Customer
            {
                Id = 5,
                FullName = "Deactivated Customer",
                Email = "deactivated@example.com",
                Role = UserRole.Customer,
                Status = CustomerStatus.Deactivated,
                EmailVerified = true,
                CreatedAt = DateTime.UtcNow
            };

            _customerRepositoryMock
                .Setup(repository =>
                    repository.GetByIdAsync(5))
                .ReturnsAsync(customer);

            _customerRepositoryMock
                .Setup(repository =>
                    repository.SaveChangesAsync())
                .ReturnsAsync(true);

            CustomerResponseDto result =
                await _customerService
                    .ReactivateAsync(5);

            Assert.Equal(
                CustomerStatus.Active,
                customer.Status
            );

            Assert.Equal(
                CustomerStatus.Active.ToString(),
                result.Status
            );

            Assert.NotNull(customer.UpdatedAt);

            _customerRepositoryMock.Verify(
                repository =>
                    repository.GetByIdAsync(5),
                Times.Once
            );

            _customerRepositoryMock.Verify(
                repository =>
                    repository.SaveChangesAsync(),
                Times.Once
            );
        }

        [Fact]
        public async Task DeactivateAsync_WhenTargetIsAdmin_ThrowsAppException()
        {
            Customer administrator = new Customer
            {
                Id = 1,
                FullName = "System Administrator",
                Email = "admin@example.com",
                Role = UserRole.Admin,
                Status = CustomerStatus.Active,
                EmailVerified = true,
                CreatedAt = DateTime.UtcNow
            };

            _customerRepositoryMock
                .Setup(repository =>
                    repository.GetByIdAsync(1))
                .ReturnsAsync(administrator);

            AppException exception =
                await Assert.ThrowsAsync<AppException>(
                    () => _customerService
                        .DeactivateAsync(1)
                );

            Assert.Equal(
                "Administrator accounts cannot be deactivated.",
                exception.Message
            );

            Assert.Equal(
                CustomerStatus.Active,
                administrator.Status
            );

            _customerRepositoryMock.Verify(
                repository =>
                    repository
                        .HasActiveFutureBookingsAsync(
                            It.IsAny<int>(),
                            It.IsAny<DateTime>()
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
        public async Task DeactivateAsync_WhenCustomerDoesNotExist_ThrowsNotFoundException()
        {
            const int customerId = 999;

            _customerRepositoryMock
                .Setup(repository =>
                    repository.GetByIdAsync(customerId))
                .ReturnsAsync((Customer?)null);

            NotFoundException exception =
                await Assert.ThrowsAsync<
                    NotFoundException>(
                    () => _customerService
                        .DeactivateAsync(customerId)
                );

            Assert.Equal(
                $"Customer with ID {customerId} was not found.",
                exception.Message
            );

            _customerRepositoryMock.Verify(
                repository =>
                    repository.GetByIdAsync(customerId),
                Times.Once
            );

            _customerRepositoryMock.Verify(
                repository =>
                    repository
                        .HasActiveFutureBookingsAsync(
                            It.IsAny<int>(),
                            It.IsAny<DateTime>()
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
        public async Task UpdateStatusAsync_WhenActiveFutureBookingExists_DoesNotBypassValidation()
        {
            const int customerId = 7;

            Customer customer = new Customer
            {
                Id = customerId,
                FullName = "Booked Customer",
                Email = "booked@example.com",
                Role = UserRole.Customer,
                Status = CustomerStatus.Active,
                EmailVerified = true,
                CreatedAt = DateTime.UtcNow
            };

            UpdateCustomerStatusDto statusDto =
                new UpdateCustomerStatusDto
                {
                    Status = "Deactivated"
                };

            _customerRepositoryMock
                .Setup(repository =>
                    repository.GetByIdAsync(
                        customerId
                    ))
                .ReturnsAsync(customer);

            _customerRepositoryMock
                .Setup(repository =>
                    repository
                        .HasActiveFutureBookingsAsync(
                            customerId,
                            It.IsAny<DateTime>()
                        ))
                .ReturnsAsync(true);

            ConflictException exception =
                await Assert.ThrowsAsync<
                    ConflictException>(
                    () => _customerService
                        .UpdateStatusAsync(
                            customerId,
                            statusDto
                        )
                );

            Assert.Equal(
                "A customer with active or upcoming bookings cannot be deactivated.",
                exception.Message
            );

            Assert.Equal(
                CustomerStatus.Active,
                customer.Status
            );

            _customerRepositoryMock.Verify(
                repository =>
                    repository.SaveChangesAsync(),
                Times.Never
            );
        }

        [Fact]
        public async Task RegisterAsync_WhenDataIsValid_CreatesCustomer()
        {
            RegisterCustomerDto registerDto =
                new RegisterCustomerDto
                {
                    FullName = "New Customer",
                    Email = "NEW@EXAMPLE.COM",
                    PhoneNumber = "0777654321",
                    Password = "Password@123"
                };

            _customerRepositoryMock
                .Setup(repository =>
                    repository.EmailExistsAsync(
                        "new@example.com"
                    ))
                .ReturnsAsync(false);

            _customerRepositoryMock
                .Setup(repository =>
                    repository.PhoneNumberExistsAsync(
                        "0777654321"
                    ))
                .ReturnsAsync(false);

            _passwordHasherMock
                .Setup(hasher =>
                    hasher.HashPassword(
                        It.IsAny<Customer>(),
                        "Password@123"
                    ))
                .Returns("hashed-password");

            _customerRepositoryMock
                .Setup(repository =>
                    repository.AddAsync(
                        It.IsAny<Customer>()
                    ))
                .Callback<Customer>(customer =>
                {
                    customer.Id = 10;
                })
                .Returns(Task.CompletedTask);

            _customerRepositoryMock
                .Setup(repository =>
                    repository.SaveChangesAsync())
                .ReturnsAsync(true);

            _emailVerificationServiceMock
                .Setup(service =>
                    service.SendVerificationEmailAsync(
                        It.IsAny<Customer>()
                    ))
                .Returns(Task.CompletedTask);

            CustomerResponseDto result =
                await _customerService
                    .RegisterAsync(registerDto);

            Assert.Equal(
                10,
                result.CustomerId
            );

            Assert.Equal(
                "New Customer",
                result.FullName
            );

            Assert.Equal(
                "new@example.com",
                result.Email
            );

            Assert.Equal(
                "0777654321",
                result.PhoneNumber
            );

            Assert.False(result.EmailVerified);

            Assert.Equal(
                0,
                result.BookingSummary.TotalBookings
            );

            _customerRepositoryMock.Verify(
                repository =>
                    repository.AddAsync(
                        It.Is<Customer>(customer =>
                            customer.Email ==
                                "new@example.com"
                            &&
                            customer.PasswordHash ==
                                "hashed-password"
                            &&
                            customer.EmailVerified ==
                                false
                        )
                    ),
                Times.Once
            );

            _customerRepositoryMock.Verify(
                repository =>
                    repository.SaveChangesAsync(),
                Times.Once
            );

            _emailVerificationServiceMock.Verify(
                service =>
                    service.SendVerificationEmailAsync(
                        It.Is<Customer>(customer =>
                            customer.Id == 10
                        )
                    ),
                Times.Once
            );
        }
    }
}