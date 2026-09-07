using Event_and_parking_reservation_system.DTOs.Notifications;

namespace Event_and_parking_reservation_system.Interfaces.Services
{
    public interface INotificationService
    {
        Task CreateInternalAsync(int customerId, string typeName, string title, string message);
        Task<List<NotificationResponseDto>> GetCustomerNotificationsAsync(int requesterCustomerId, bool isAdmin, int customerId);
        Task<NotificationResponseDto> MarkReadAsync(int requesterCustomerId, bool isAdmin, int notificationId);
    }
}
