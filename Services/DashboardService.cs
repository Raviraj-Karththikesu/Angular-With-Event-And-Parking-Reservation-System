using Event_and_parking_reservation_system.DTOs.Dashboards;
using Event_and_parking_reservation_system.Exceptions;
using Event_and_parking_reservation_system.Interfaces.Repositories;
using Event_and_parking_reservation_system.Interfaces.Services;

namespace Event_and_parking_reservation_system.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IDashboardRepository _repository;

        public DashboardService(IDashboardRepository repository)
        {
            _repository = repository;
        }

        public Task<AdminDashboardDto> GetAdminSummaryAsync() =>
            _repository.GetAdminSummaryAsync();

        public Task<CustomerDashboardDto> GetCustomerSummaryAsync(
            int requesterCustomerId,
            bool isAdmin,
            int customerId)
        {
            if (!isAdmin && requesterCustomerId != customerId)
                throw new AppException(
                    "You can only view your own dashboard.",
                    StatusCodes.Status403Forbidden);

            return _repository.GetCustomerSummaryAsync(customerId);
        }
    }
}
