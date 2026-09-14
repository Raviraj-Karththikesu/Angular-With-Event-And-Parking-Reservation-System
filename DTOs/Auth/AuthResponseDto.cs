namespace Event_and_parking_reservation_system.DTOs.Auth
{
    public class AuthResponseDto
    {
        public int CustomerId { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public bool EmailVerified { get; set; }

        public string AccessToken { get; set; } = string.Empty;

        public string TokenType { get; set; } = "Bearer";

        public DateTime ExpiresAt { get; set; }
    }
}