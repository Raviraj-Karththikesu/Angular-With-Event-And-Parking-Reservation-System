namespace Event_and_parking_reservation_system.Models;

public class ParkingReservation
{
    public int Id { get; set; }

    public int BookingId { get; set; }

    public int ParkingSlotId { get; set; }

    public decimal FeeAtReservation { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime ReservedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ReleasedAt { get; set; }

    public Booking Booking { get; set; } = null!;

    public ParkingSlot ParkingSlot { get; set; } = null!;
}