using Event_and_parking_reservation_system.DTOs.Auth;
using Event_and_parking_reservation_system.Enums;
using Event_and_parking_reservation_system.Exceptions;
using Event_and_parking_reservation_system.Interfaces.Repositories;
using Event_and_parking_reservation_system.Interfaces.Services;
using Event_and_parking_reservation_system.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace Event_and_parking_reservation_system.Services
{
    public class AuthService : IAuthService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IPasswordHasher<Customer> _passwordHasher;
        private readonly IJwtTokenService _jwtTokenService;

        public AuthService(
            ICustomerRepository customerRepository,
            IPasswordHasher<Customer> passwordHasher,
            IJwtTokenService jwtTokenService)
        {
            _customerRepository = customerRepository;
            _passwordHasher = passwordHasher;
            _jwtTokenService = jwtTokenService;
        }

        public async Task<AuthResponseDto> LoginAsync(
            LoginRequestDto loginRequestDto)
        {
            string normalizedEmail =
                loginRequestDto.Email.Trim().ToLowerInvariant();

            Customer? customer =
                await _customerRepository.GetByEmailAsync(
                    normalizedEmail
                );

            if (customer is null)
            {
                throw new AppException(
                    "Invalid email or password.",
                    StatusCodes.Status401Unauthorized
                );
            }

            PasswordVerificationResult verificationResult =
                _passwordHasher.VerifyHashedPassword(
                    customer,
                    customer.PasswordHash,
                    loginRequestDto.Password
                );

            if (verificationResult ==
                PasswordVerificationResult.Failed)
            {
                throw new AppException(
                    "Invalid email or password.",
                    StatusCodes.Status401Unauthorized
                );
            }

            if (customer.Status != CustomerStatus.Active)
            {
                throw new AppException(
                    "This customer account is not active.",
                    StatusCodes.Status403Forbidden
                );
            }

            if (!customer.EmailVerified)
            {
                throw new AppException(
                    "Please verify your email address before logging in.",
                    StatusCodes.Status403Forbidden
                );
            }

            if (verificationResult ==
                PasswordVerificationResult.SuccessRehashNeeded)
            {
                customer.PasswordHash =
                    _passwordHasher.HashPassword(
                        customer,
                        loginRequestDto.Password
                    );

                customer.UpdatedAt = DateTime.UtcNow;

                _customerRepository.Update(customer);

                await _customerRepository.SaveChangesAsync();
            }

            string accessToken =
                _jwtTokenService.GenerateToken(customer);

            DateTime expiresAt =
                _jwtTokenService.GetExpirationTime();

            return new AuthResponseDto
            {
                CustomerId = customer.Id,
                FullName = customer.FullName,
                Email = customer.Email,
                Role = customer.Role.ToString(),
                Status = customer.Status.ToString(),
                EmailVerified = customer.EmailVerified,
                AccessToken = accessToken,
                TokenType = "Bearer",
                ExpiresAt = expiresAt
            };
        }
    }
}