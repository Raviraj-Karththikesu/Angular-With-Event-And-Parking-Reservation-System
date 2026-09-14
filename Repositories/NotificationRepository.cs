using Event_and_parking_reservation_system.Data;
using Event_and_parking_reservation_system.Interfaces.Repositories;
using Event_and_parking_reservation_system.Models;
using Microsoft.EntityFrameworkCore;

namespace Event_and_parking_reservation_system.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly AppDbContext _db;

        public NotificationRepository(AppDbContext db)
        {
            _db = db;
        }

        public Task AddAsync(Notification notification) =>
            _db.Set<Notification>().AddAsync(notification).AsTask();

        public Task<Notification?> GetByIdAsync(int notificationId) =>
            _db.Set<Notification>().FirstOrDefaultAsync(x => x.Id == notificationId);

        public Task<List<Notification>> GetByCustomerAsync(int customerId) =>
            _db.Set<Notification>()
                .Where(x => x.CustomerId == customerId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

        public Task<int> SaveChangesAsync() =>
            _db.SaveChangesAsync();
    }
}
