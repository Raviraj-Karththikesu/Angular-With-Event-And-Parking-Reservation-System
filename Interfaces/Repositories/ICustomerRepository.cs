using Event_and_parking_reservation_system.Models;

namespace Event_and_parking_reservation_system.Interfaces.Repositories
{
    public interface ICustomerRepository
    {
        Task<Customer?> GetByIdAsync(int customerId);

        Task<Customer?> GetByEmailAsync(string email);

        Task<bool> EmailExistsAsync(string email);

        Task<bool> PhoneNumberExistsAsync(string phoneNumber);

        Task AddAsync(Customer customer);

        void Update(Customer customer);

        Task<bool> SaveChangesAsync();
    }
}