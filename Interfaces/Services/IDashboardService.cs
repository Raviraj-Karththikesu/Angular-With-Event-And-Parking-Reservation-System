using Event_and_parking_reservation_system.DTOs.Dashboards;

namespace Event_and_parking_reservation_system.Interfaces.Services
{
    public interface IDashboardService
    {
        Task<AdminDashboardDto> GetAdminSummaryAsync();
        Task<CustomerDashboardDto> GetCustomerSummaryAsync(int requesterCustomerId, bool isAdmin, int customerId);
    }
}
