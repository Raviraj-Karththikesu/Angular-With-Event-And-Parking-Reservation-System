using Event_and_parking_reservation_system.Models;

namespace EventParking.Api.Models;

public class Venue
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public int TotalCapacity { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public ICollection<Event> Events { get; set; } = new List<Event>();
}