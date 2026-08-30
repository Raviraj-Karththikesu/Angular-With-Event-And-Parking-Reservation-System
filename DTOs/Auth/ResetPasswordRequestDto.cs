using System.ComponentModel.DataAnnotations;

namespace Event_and_parking_reservation_system.DTOs.Auth
{
    public class ResetPasswordRequestDto
    {
        [Required(ErrorMessage = "Reset token is required.")]
        public string Token { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password is required.")]
        [StringLength(
            100,
            MinimumLength = 8,
            ErrorMessage =
                "Password must contain at least 8 characters."
        )]
        public string NewPassword { get; set; } =
            string.Empty;

        [Required(
            ErrorMessage = "Password confirmation is required."
        )]
        [Compare(
            nameof(NewPassword),
            ErrorMessage = "Passwords do not match."
        )]
        public string ConfirmPassword { get; set; } =
            string.Empty;
    }
}