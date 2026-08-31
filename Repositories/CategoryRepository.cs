using Event_and_parking_reservation_system.Data;
using Event_and_parking_reservation_system.Interfaces.Repositories;
using Event_and_parking_reservation_system.Models;
using Microsoft.EntityFrameworkCore;

namespace Event_and_parking_reservation_system.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly AppDbContext _context;

        public CategoryRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<EventCategory>> GetAllAsync()
        {
            return await _context.EventCategories
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<EventCategory?> GetByIdAsync(int id)
        {
            return await _context.EventCategories
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<EventCategory?> GetByNameAsync(string name)
        {
            return await _context.EventCategories
                .FirstOrDefaultAsync(c => c.Name == name);
        }

        public async Task AddAsync(EventCategory category)
        {
            await _context.EventCategories.AddAsync(category);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(EventCategory category)
        {
            _context.EventCategories.Update(category);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(EventCategory category)
        {
            _context.EventCategories.Remove(category);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> HasEventsAsync(int categoryId)
        {
            return await _context.Events
                .AnyAsync(e => e.EventCategoryId == categoryId);
        }
    }
}