using Event_and_parking_reservation_system.Enums;

namespace Event_and_parking_reservation_system.Models;

public class Notification
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public NotificationType Type { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public bool IsRead { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ReadAt { get; set; }

    public Customer Customer { get; set; } = null!;
}