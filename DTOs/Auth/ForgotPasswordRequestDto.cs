using System.ComponentModel.DataAnnotations;

namespace Event_and_parking_reservation_system.DTOs.Auth
{
    public class ForgotPasswordRequestDto
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(
            ErrorMessage = "Enter a valid email address."
        )]
        public string Email { get; set; } = string.Empty;
    }
}