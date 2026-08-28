using Event_and_parking_reservation_system.Models;

namespace Event_and_parking_reservation_system.Models;

public class EventCategory
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public ICollection<Event> Events { get; set; } = new List<Event>();
}