namespace Event_and_parking_reservation_system.DTOs.Venues
{
    public class VenueAvailabilityResponseDto
    {
        public int VenueId { get; set; }

        public string VenueName { get; set; } = string.Empty;

        public bool IsAvailable { get; set; }
    }
}