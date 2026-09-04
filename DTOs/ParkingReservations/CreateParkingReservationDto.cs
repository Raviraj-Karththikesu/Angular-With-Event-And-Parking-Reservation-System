using System.ComponentModel.DataAnnotations;

namespace Event_and_parking_reservation_system.DTOs.ParkingReservations;

public class CreateParkingReservationDto
{
    [Required]
    public int BookingId { get; set; }

    [Required]
    public int ParkingSlotId { get; set; }
}