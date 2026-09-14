using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Event_and_parking_reservation_system.Interfaces.Services;
using Event_and_parking_reservation_system.Models;
using Event_and_parking_reservation_system.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Event_and_parking_reservation_system.Services
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly JwtSettings _jwtSettings;

        public JwtTokenService(
            IOptions<JwtSettings> jwtOptions)
        {
            _jwtSettings = jwtOptions.Value;
        }

        public string GenerateToken(Customer customer)
        {
            if (string.IsNullOrWhiteSpace(_jwtSettings.Key))
            {
                throw new InvalidOperationException(
                    "JWT secret key is not configured."
                );
            }

            var claims = new List<Claim>
            {
                new Claim(
                    JwtRegisteredClaimNames.Sub,
                    customer.Id.ToString()
                ),

                new Claim(
                    JwtRegisteredClaimNames.Email,
                    customer.Email
                ),

                new Claim(
                    JwtRegisteredClaimNames.Jti,
                    Guid.NewGuid().ToString()
                ),

                new Claim(
                    ClaimTypes.NameIdentifier,
                    customer.Id.ToString()
                ),

                new Claim(
                    ClaimTypes.Name,
                    customer.FullName
                ),

                new Claim(
                    ClaimTypes.Role,
                    customer.Role.ToString()
                )
            };

            var securityKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_jwtSettings.Key)
            );

            var credentials = new SigningCredentials(
                securityKey,
                SecurityAlgorithms.HmacSha256
            );

            DateTime expiresAt = GetExpirationTime();

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: expiresAt,
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler()
                .WriteToken(token);
        }

        public DateTime GetExpirationTime()
        {
            return DateTime.UtcNow.AddMinutes(
                _jwtSettings.ExpiryMinutes
            );
        }
    }
}