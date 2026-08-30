using Event_and_parking_reservation_system.Models;

namespace Event_and_parking_reservation_system.Interfaces.Repositories
{
    public interface ICustomerRepository
    {
        Task<List<Customer>> GetAllAsync();
        Task<Customer?> GetByIdAsync(int customerId);

        Task<Customer?> GetByEmailAsync(string email);

        Task<Customer?> GetByEmailVerificationTokenHashAsync(
    string tokenHash
);
        Task<Customer?> GetByPasswordResetTokenHashAsync(
    string tokenHash
);

        Task<bool> EmailExistsAsync(string email);

        Task<bool> PhoneNumberExistsAsync(string phoneNumber);

        Task AddAsync(Customer customer);

        void Update(Customer customer);

        Task<bool> SaveChangesAsync();


    }
}