namespace Event_and_parking_reservation_system.DTOs.ParkingReservations;

public class ParkingReservationResponseDto
{
    public int Id { get; set; }

    public int BookingId { get; set; }

    public int ParkingSlotId { get; set; }

    public string SlotNumber { get; set; } = string.Empty;

    public string? Zone { get; set; }

    public decimal FeeAtReservation { get; set; }

    public bool IsActive { get; set; }

    public DateTime ReservedAt { get; set; }

    public DateTime? ReleasedAt { get; set; }
}