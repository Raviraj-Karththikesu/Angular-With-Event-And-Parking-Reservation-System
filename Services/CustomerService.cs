using Event_and_parking_reservation_system.DTOs.Customers;
using Event_and_parking_reservation_system.Enums;
using Event_and_parking_reservation_system.Exceptions;
using Event_and_parking_reservation_system.Interfaces.Repositories;
using Event_and_parking_reservation_system.Interfaces.Services;
using Event_and_parking_reservation_system.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace Event_and_parking_reservation_system.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository
            _customerRepository;

        private readonly IPasswordHasher<Customer>
            _passwordHasher;

        private readonly IEmailVerificationService
            _emailVerificationService;

        public CustomerService(
            ICustomerRepository customerRepository,
            IPasswordHasher<Customer> passwordHasher,
            IEmailVerificationService emailVerificationService)
        {
            _customerRepository = customerRepository;
            _passwordHasher = passwordHasher;
            _emailVerificationService =
                emailVerificationService;
        }

        public async Task<CustomerResponseDto> RegisterAsync(
            RegisterCustomerDto registerCustomerDto)
        {
            string normalizedEmail =
                registerCustomerDto.Email
                    .Trim()
                    .ToLowerInvariant();

            bool emailExists =
                await _customerRepository
                    .EmailExistsAsync(
                        normalizedEmail
                    );

            if (emailExists)
            {
                throw new ConflictException(
                    "A customer with this email already exists."
                );
            }

            string? normalizedPhoneNumber = null;

            if (!string.IsNullOrWhiteSpace(
                registerCustomerDto.PhoneNumber))
            {
                normalizedPhoneNumber =
                    registerCustomerDto
                        .PhoneNumber
                        .Trim();

                bool phoneNumberExists =
                    await _customerRepository
                        .PhoneNumberExistsAsync(
                            normalizedPhoneNumber
                        );

                if (phoneNumberExists)
                {
                    throw new ConflictException(
                        "A customer with this phone number already exists."
                    );
                }
            }

            Customer customer = new Customer
            {
                FullName =
                    registerCustomerDto
                        .FullName
                        .Trim(),

                Email = normalizedEmail,

                PhoneNumber =
                    normalizedPhoneNumber,

                EmailVerified = false,

                CreatedAt = DateTime.UtcNow
            };

            customer.PasswordHash =
                _passwordHasher.HashPassword(
                    customer,
                    registerCustomerDto.Password
                );

            await _customerRepository.AddAsync(
                customer
            );

            bool saved =
                await _customerRepository
                    .SaveChangesAsync();

            if (!saved)
            {
                throw new AppException(
                    "Customer registration failed.",
                    StatusCodes
                        .Status500InternalServerError
                );
            }

            await _emailVerificationService
                .SendVerificationEmailAsync(
                    customer
                );

            return MapToResponseDto(customer);
        }

        public async Task<CustomerResponseDto?>
    GetByIdAsync(int customerId)
        {
            Customer? customer =
                await _customerRepository
                    .GetByIdWithBookingsAsync(
                        customerId
                    );

            if (customer is null)
            {
                return null;
            }

            return MapToResponseDto(customer);
        }
        public async Task<List<CustomerListItemDto>>
            GetAllAsync()
        {
            List<Customer> customers =
                await _customerRepository
                    .GetAllAsync();

            return customers
                .Select(customer =>
                    new CustomerListItemDto
                    {
                        CustomerId = customer.Id,

                        FullName =
                            customer.FullName,

                        Email = customer.Email,

                        PhoneNumber =
                            customer.PhoneNumber,

                        Role =
                            customer.Role.ToString(),

                        Status =
                            customer.Status.ToString(),

                        EmailVerified =
                            customer.EmailVerified,

                        CreatedAt =
                            customer.CreatedAt
                    })
                .ToList();
        }

        public async Task<CustomerResponseDto>
    UpdateStatusAsync(
        int customerId,
        UpdateCustomerStatusDto updateStatusDto)
        {
            bool validStatus =
                Enum.TryParse(
                    updateStatusDto.Status,
                    true,
                    out CustomerStatus customerStatus
                );

            if (!validStatus ||
                !Enum.IsDefined(
                    typeof(CustomerStatus),
                    customerStatus
                ))
            {
                throw new AppException(
                    "Status must be Active or Deactivated.",
                    StatusCodes.Status400BadRequest
                );
            }

            if (customerStatus ==
                CustomerStatus.Deactivated)
            {
                return await DeactivateAsync(customerId);
            }

            return await ReactivateAsync(customerId);
        }

        public async Task<CustomerResponseDto>
    UpdateProfileAsync(
        int customerId,
        UpdateCustomerProfileDto updateProfileDto)
        {
            Customer? customer =
                await _customerRepository.GetByIdAsync(
                    customerId
                );

            if (customer is null)
            {
                throw new NotFoundException(
                    "Customer was not found."
                );
            }

            string? normalizedPhoneNumber =
                string.IsNullOrWhiteSpace(
                    updateProfileDto.PhoneNumber
                )
                    ? null
                    : updateProfileDto.PhoneNumber.Trim();

            bool phoneNumberChanged =
                !string.Equals(
                    customer.PhoneNumber,
                    normalizedPhoneNumber,
                    StringComparison.Ordinal
                );

            if (phoneNumberChanged &&
                normalizedPhoneNumber is not null)
            {
                bool phoneNumberExists =
                    await _customerRepository
                        .PhoneNumberExistsAsync(
                            normalizedPhoneNumber
                        );

                if (phoneNumberExists)
                {
                    throw new ConflictException(
                        "A customer with this phone number already exists."
                    );
                }
            }

            customer.FullName =
                updateProfileDto.FullName.Trim();

            customer.PhoneNumber =
                normalizedPhoneNumber;

            customer.UpdatedAt = DateTime.UtcNow;

            _customerRepository.Update(customer);

            bool saved =
                await _customerRepository
                    .SaveChangesAsync();

            if (!saved)
            {
                throw new AppException(
                    "Customer profile update failed.",
                    StatusCodes.Status500InternalServerError
                );
            }

            return MapToResponseDto(customer);
        }

        private static CustomerResponseDto
    MapToResponseDto(Customer customer)
        {
            DateTime utcNow = DateTime.UtcNow;

            return new CustomerResponseDto
            {
                CustomerId = customer.Id,

                FullName = customer.FullName,

                Email = customer.Email,

                PhoneNumber = customer.PhoneNumber,

                Role = customer.Role.ToString(),

                Status = customer.Status.ToString(),

                EmailVerified = customer.EmailVerified,

                CreatedAt = customer.CreatedAt,

                UpdatedAt = customer.UpdatedAt,

                BookingSummary =
                    new CustomerBookingSummaryDto
                    {
                        TotalBookings =
                            customer.Bookings.Count,

                        PendingBookings =
                            customer.Bookings.Count(
                                booking =>
                                    booking.Status ==
                                    BookingStatus.Pending
                            ),

                        ConfirmedBookings =
                            customer.Bookings.Count(
                                booking =>
                                    booking.Status ==
                                    BookingStatus.Confirmed
                            ),

                        CancelledBookings =
                            customer.Bookings.Count(
                                booking =>
                                    booking.Status ==
                                    BookingStatus.Cancelled
                            ),

                        ExpiredBookings =
                            customer.Bookings.Count(
                                booking =>
                                    booking.Status ==
                                    BookingStatus.Expired
                            ),

                        ActiveUpcomingBookings =
                            customer.Bookings.Count(
                                booking =>
                                    (
                                        booking.Status ==
                                            BookingStatus.Pending
                                        ||
                                        booking.Status ==
                                            BookingStatus.Confirmed
                                    )
                                    &&
                                    booking.Event is not null
                                    &&
                                    booking.Event.EndDateTime >
                                        utcNow
                            )
                    }
            };
        }

        public async Task<List<CustomerListItemDto>>
    SearchAsync(string? search)
        {
            List<Customer> customers =
                await _customerRepository.SearchAsync(
                    search
                );

            return customers
                .Select(customer =>
                    new CustomerListItemDto
                    {
                        CustomerId = customer.Id,

                        FullName = customer.FullName,

                        Email = customer.Email,

                        PhoneNumber =
                            customer.PhoneNumber,

                        Role =
                            customer.Role.ToString(),

                        Status =
                            customer.Status.ToString(),

                        EmailVerified =
                            customer.EmailVerified,

                        CreatedAt =
                            customer.CreatedAt
                    })
                .ToList();
        }

        public async Task<CustomerResponseDto> DeactivateAsync(
     int customerId)
        {
            Customer? customer =
                await _customerRepository.GetByIdAsync(
                    customerId
                );

            if (customer is null)
            {
                throw new NotFoundException(
                    $"Customer with ID {customerId} was not found."
                );
            }

            if (customer.Role == UserRole.Admin)
            {
                throw new AppException(
                    "Administrator accounts cannot be deactivated.",
                    StatusCodes.Status403Forbidden
                );
            }

            if (customer.Status ==
                CustomerStatus.Deactivated)
            {
                return MapToResponseDto(customer);
            }

            DateTime utcNow = DateTime.UtcNow;

            bool hasActiveFutureBookings =
                await _customerRepository
                    .HasActiveFutureBookingsAsync(
                        customerId,
                        utcNow
                    );

            if (hasActiveFutureBookings)
            {
                throw new ConflictException(
                    "A customer with active or upcoming bookings cannot be deactivated."
                );
            }

            customer.Status =
                CustomerStatus.Deactivated;

            customer.UpdatedAt = utcNow;

            bool saved =
                await _customerRepository
                    .SaveChangesAsync();

            if (!saved)
            {
                throw new AppException(
                    "Customer deactivation failed.",
                    StatusCodes.Status500InternalServerError
                );
            }

            return MapToResponseDto(customer);
        }

        public async Task<CustomerResponseDto> ReactivateAsync(
            int customerId)
        {
            Customer? customer =
                await _customerRepository.GetByIdAsync(customerId);

            if (customer is null)
            {
                throw new NotFoundException(
                    $"Customer with ID {customerId} was not found."
                );
            }

            if (customer.Role == UserRole.Admin)
            {
                throw new AppException(
                    "Administrator account status cannot be changed here.",
                    StatusCodes.Status403Forbidden
                );
            }

            if (customer.Status == CustomerStatus.Active)
            {
                return MapToResponseDto(customer);
            }

            customer.Status = CustomerStatus.Active;
            customer.UpdatedAt = DateTime.UtcNow;

            bool saved =
                await _customerRepository.SaveChangesAsync();

            if (!saved)
            {
                throw new AppException(
                    "Customer reactivation failed.",
                    StatusCodes.Status500InternalServerError
                );
            }

            return MapToResponseDto(customer);
        }


    }
}