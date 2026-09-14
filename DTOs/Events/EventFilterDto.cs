namespace Event_and_parking_reservation_system.DTOs.Events
{
    public class EventFilterDto

    {
        public string? Search { get; set; }

        public DateTime? Date { get; set; }

        public int? VenueId { get; set; }

        public int? EventCategoryId { get; set; }
    }
}