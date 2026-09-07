using Event_and_parking_reservation_system.DTOs.Dashboards;

namespace Event_and_parking_reservation_system.Interfaces.Repositories
{
    public interface IDashboardRepository
    {
        Task<AdminDashboardDto> GetAdminSummaryAsync();
        Task<CustomerDashboardDto> GetCustomerSummaryAsync(int customerId);
    }
}
