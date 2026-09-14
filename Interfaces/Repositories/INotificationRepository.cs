using Event_and_parking_reservation_system.Models;

namespace Event_and_parking_reservation_system.Interfaces.Repositories
{
    public interface INotificationRepository
    {
        Task AddAsync(Notification notification);
        Task<Notification?> GetByIdAsync(int notificationId);
        Task<List<Notification>> GetByCustomerAsync(int customerId);
        Task<int> SaveChangesAsync();
    }
}
