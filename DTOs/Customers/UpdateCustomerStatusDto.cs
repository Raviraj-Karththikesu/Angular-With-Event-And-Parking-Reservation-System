using System.ComponentModel.DataAnnotations;

namespace Event_and_parking_reservation_system
    .DTOs.Customers
{
    public class UpdateCustomerStatusDto
    {
        [Required(
            ErrorMessage = "Customer status is required."
        )]
        [RegularExpression(
            "^(Active|Deactivated)$",
            ErrorMessage =
                "Status must be Active or Deactivated."
        )]
        public string Status { get; set; } =
            string.Empty;
    }
}