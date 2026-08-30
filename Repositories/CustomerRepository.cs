using Event_and_parking_reservation_system.Data;
using Event_and_parking_reservation_system.Interfaces.Repositories;
using Event_and_parking_reservation_system.Models;
using Microsoft.EntityFrameworkCore;

namespace Event_and_parking_reservation_system.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly AppDbContext _context;

        public CustomerRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Customer?> GetByIdAsync(int customerId)
        {
            return await _context.Customers
                .AsNoTracking()
                .FirstOrDefaultAsync(customer =>
                    customer.Id == customerId);
        }

        public async Task<Customer?> GetByEmailAsync(string email)
        {
            string normalizedEmail = email.Trim().ToLower();

            return await _context.Customers
                .AsNoTracking()
                .FirstOrDefaultAsync(customer =>
                    customer.Email.ToLower() == normalizedEmail);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            string normalizedEmail = email.Trim().ToLower();

            return await _context.Customers
                .AnyAsync(customer =>
                    customer.Email.ToLower() == normalizedEmail);
        }

        public async Task<bool> PhoneNumberExistsAsync(string phoneNumber)
        {
            string normalizedPhoneNumber = phoneNumber.Trim();

            return await _context.Customers
                .AnyAsync(customer =>
                    customer.PhoneNumber == normalizedPhoneNumber);
        }

        public async Task AddAsync(Customer customer)
        {
            await _context.Customers.AddAsync(customer);
        }

        public void Update(Customer customer)
        {
            _context.Customers.Update(customer);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<Customer?>
    GetByEmailVerificationTokenHashAsync(string tokenHash)
        {
            return await _context.Customers
                .FirstOrDefaultAsync(customer =>
                    customer.EmailVerificationTokenHash == tokenHash
                );
        }

        public async Task<Customer?>
    GetByPasswordResetTokenHashAsync(
        string tokenHash)
        {
            return await _context.Customers
                .FirstOrDefaultAsync(customer =>
                    customer.PasswordResetTokenHash ==
                    tokenHash
                );
        }

        public async Task<List<Customer>> GetAllAsync()
        {
            return await _context.Customers
                .AsNoTracking()
                .OrderByDescending(customer =>
                    customer.CreatedAt
                )
                .ToListAsync();
        }
    }
}