using Event_and_parking_reservation_system.DTOs.Customers;
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
        private readonly ICustomerRepository _customerRepository;
        private readonly IPasswordHasher<Customer> _passwordHasher;
        private readonly IEmailVerificationService
            _emailVerificationService;

        public CustomerService(
            ICustomerRepository customerRepository,
            IPasswordHasher<Customer> passwordHasher,
            IEmailVerificationService emailVerificationService)
        {
            _customerRepository = customerRepository;
            _passwordHasher = passwordHasher;
            _emailVerificationService = emailVerificationService;
        }

        public async Task<CustomerResponseDto> RegisterAsync(
            RegisterCustomerDto registerCustomerDto)
        {
            string normalizedEmail =
                registerCustomerDto.Email
                    .Trim()
                    .ToLowerInvariant();

            bool emailExists =
                await _customerRepository.EmailExistsAsync(
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
                    registerCustomerDto.PhoneNumber.Trim();

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
                    registerCustomerDto.FullName.Trim(),

                Email = normalizedEmail,

                PhoneNumber = normalizedPhoneNumber,

                EmailVerified = false,

                CreatedAt = DateTime.UtcNow
            };

            customer.PasswordHash =
                _passwordHasher.HashPassword(
                    customer,
                    registerCustomerDto.Password
                );

            await _customerRepository.AddAsync(customer);

            bool saved =
                await _customerRepository.SaveChangesAsync();

            if (!saved)
            {
                throw new AppException(
                    "Customer registration failed.",
                    StatusCodes.Status500InternalServerError
                );
            }

            await _emailVerificationService
                .SendVerificationEmailAsync(customer);

            return MapToResponseDto(customer);
        }

        public async Task<CustomerResponseDto?> GetByIdAsync(
            int customerId)
        {
            Customer? customer =
                await _customerRepository.GetByIdAsync(
                    customerId
                );

            if (customer is null)
            {
                return null;
            }

            return MapToResponseDto(customer);
        }

        private static CustomerResponseDto MapToResponseDto(
            Customer customer)
        {
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
                UpdatedAt = customer.UpdatedAt
            };
        }
    }
}