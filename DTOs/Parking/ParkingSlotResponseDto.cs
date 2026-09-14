namespace Event_and_parking_reservation_system.DTOs.Parking
{
    public class ParkingSlotResponseDto
    {
        public int Id { get; set; }

        public int EventId { get; set; }

        public string SlotNumber { get; set; } = string.Empty;

        public string? Zone { get; set; }

        public decimal Fee { get; set; }

        public string Status { get; set; } = string.Empty;
    }
}