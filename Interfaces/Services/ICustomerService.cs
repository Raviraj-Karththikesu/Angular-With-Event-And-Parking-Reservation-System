using Event_and_parking_reservation_system.DTOs.Customers;

namespace Event_and_parking_reservation_system.Interfaces.Services
{
    public interface ICustomerService
    {
        Task<CustomerResponseDto> RegisterAsync(
            RegisterCustomerDto registerCustomerDto
        );

        Task<CustomerResponseDto?> GetByIdAsync(int customerId);

        Task<List<CustomerListItemDto>> GetAllAsync();

        Task<CustomerResponseDto> UpdateStatusAsync(
            int customerId,
            UpdateCustomerStatusDto updateStatusDto
        );

        Task<CustomerResponseDto> UpdateProfileAsync(
    int customerId,
    UpdateCustomerProfileDto updateProfileDto
);

        Task<List<CustomerListItemDto>> SearchAsync(
    string? search
);
    }
}