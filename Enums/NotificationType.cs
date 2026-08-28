namespace Event_and_parking_reservation_system.Models;

public enum NotificationType
{
    BookingCreated = 1,
    PaymentCompleted = 2,
    BookingCancelled = 3,
    BookingExpired = 4,
    EventUpdated = 5,
    EventReminder = 6
}