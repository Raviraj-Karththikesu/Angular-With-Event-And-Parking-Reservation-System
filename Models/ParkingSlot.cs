using Event_and_parking_reservation_system.Models;
using Event_and_parking_reservation_system.Enums;

namespace Event_and_parking_reservation_system.Models;

public class ParkingSlot
{
    public int Id { get; set; }

    public int EventId { get; set; }

    public string SlotNumber { get; set; } = string.Empty;

    public string? Zone { get; set; }

    public decimal Fee { get; set; }

    public ParkingSlotStatus Status { get; set; }
        = ParkingSlotStatus.Available;

    public Event Event { get; set; } = null!;

    public ICollection<ParkingReservation> ParkingReservations { get; set; }
        = new List<ParkingReservation>();
}