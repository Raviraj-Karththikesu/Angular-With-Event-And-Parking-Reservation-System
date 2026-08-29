using Event_and_parking_reservation_system.DTOs.Customers;

namespace Event_and_parking_reservation_system.Interfaces.Services
{
    public interface ICustomerService
    {
        Task<CustomerResponseDto> RegisterAsync(
            RegisterCustomerDto registerCustomerDto
        );

        Task<CustomerResponseDto?> GetByIdAsync(int customerId);
    }
}