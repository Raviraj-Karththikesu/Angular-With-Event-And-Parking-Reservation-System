using Event_and_parking_reservation_system.DTOs.Notifications;
using Event_and_parking_reservation_system.Enums;
using Event_and_parking_reservation_system.Exceptions;
using Event_and_parking_reservation_system.Interfaces.Repositories;
using Event_and_parking_reservation_system.Interfaces.Services;
using Event_and_parking_reservation_system.Models;

namespace Event_and_parking_reservation_system.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _repository;

        public NotificationService(INotificationRepository repository)
        {
            _repository = repository;
        }

        public async Task CreateInternalAsync(
            int customerId,
            string typeName,
            string title,
            string message)
        {
            NotificationType type = ResolveType(typeName);

            Notification notification = new()
            {
                CustomerId = customerId,
                Type = type,
                Title = title,
                Message = message,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            await _repository.AddAsync(notification);
            await _repository.SaveChangesAsync();
        }

        public async Task<List<NotificationResponseDto>>
            GetCustomerNotificationsAsync(
                int requesterCustomerId,
                bool isAdmin,
                int customerId)
        {
            if (!isAdmin && requesterCustomerId != customerId)
                throw new AppException(
                    "You can only view your own notifications.",
                    StatusCodes.Status403Forbidden);

            List<Notification> notifications =
                await _repository.GetByCustomerAsync(customerId);

            return notifications.Select(Map).ToList();
        }

        public async Task<NotificationResponseDto> MarkReadAsync(
            int requesterCustomerId,
            bool isAdmin,
            int notificationId)
        {
            Notification? notification =
                await _repository.GetByIdAsync(notificationId);

            if (notification is null)
                throw new NotFoundException("Notification was not found.");

            if (!isAdmin && notification.CustomerId != requesterCustomerId)
                throw new AppException(
                    "You can only update your own notifications.",
                    StatusCodes.Status403Forbidden);

            notification.IsRead = true;
            await _repository.SaveChangesAsync();

            return Map(notification);
        }

        private static NotificationType ResolveType(string preferredName)
        {
            if (Enum.TryParse(
                preferredName,
                true,
                out NotificationType exact))
                return exact;

            string[] fallbacks =
            {
                "BookingConfirmed",
                "BookingUpdated",
                "System"
            };

            foreach (string name in fallbacks)
            {
                if (Enum.TryParse(name, true, out NotificationType parsed))
                    return parsed;
            }

            return default;
        }

        private static NotificationResponseDto Map(Notification notification) =>
            new()
            {
                NotificationId = notification.Id,
                CustomerId = notification.CustomerId,
                Type = notification.Type.ToString(),
                Title = notification.Title,
                Message = notification.Message,
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt
            };
    }
}
