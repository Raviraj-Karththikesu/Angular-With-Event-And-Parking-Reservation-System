using System.ComponentModel.DataAnnotations;

namespace Event_and_parking_reservation_system.DTOs.Customers;

public class UpdateCustomerDto
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string FullName { get; set; } = string.Empty;

    [Phone]
    [StringLength(20)]
    public string? PhoneNumber { get; set; }
}