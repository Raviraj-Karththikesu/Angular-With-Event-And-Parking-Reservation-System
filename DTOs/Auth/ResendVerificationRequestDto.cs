using System.ComponentModel.DataAnnotations;

namespace Event_and_parking_reservation_system.DTOs.Auth;

public class ResendVerificationRequestDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}