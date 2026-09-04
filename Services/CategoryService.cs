using Event_and_parking_reservation_system.DTOs.Categories;
using Event_and_parking_reservation_system.Exceptions;
using Event_and_parking_reservation_system.Interfaces.Repositories;
using Event_and_parking_reservation_system.Interfaces.Services;
using Event_and_parking_reservation_system.Models;

namespace Event_and_parking_reservation_system.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<IEnumerable<CategoryResponseDto>> GetAllAsync()
        {
            var categories = await _categoryRepository.GetAllAsync();

            return categories.Select(MapToResponseDto);
        }

        public async Task<CategoryResponseDto> GetByIdAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);

            if (category is null)
            {
                throw new NotFoundException("Category was not found.");
            }

            return MapToResponseDto(category);
        }

        public async Task<CategoryResponseDto> CreateAsync(
            CreateCategoryDto dto)
        {
            var name = dto.Name.Trim();

            var existingCategory =
                await _categoryRepository.GetByNameAsync(name);

            if (existingCategory is not null)
            {
                throw new ConflictException(
                    "A category with this name already exists.");
            }

            var category = new EventCategory
            {
                Name = name,
                Description = string.IsNullOrWhiteSpace(dto.Description)
                    ? null
                    : dto.Description.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            await _categoryRepository.AddAsync(category);

            return MapToResponseDto(category);
        }

        public async Task<CategoryResponseDto> UpdateAsync(
            int id,
            UpdateCategoryDto dto)
        {
            var category = await _categoryRepository.GetByIdAsync(id);

            if (category is null)
            {
                throw new NotFoundException("Category was not found.");
            }

            var name = dto.Name.Trim();

            var existingCategory =
                await _categoryRepository.GetByNameAsync(name);

            if (existingCategory is not null &&
                existingCategory.Id != id)
            {
                throw new ConflictException(
                    "A category with this name already exists.");
            }

            category.Name = name;
            category.Description =
                string.IsNullOrWhiteSpace(dto.Description)
                    ? null
                    : dto.Description.Trim();

            category.UpdatedAt = DateTime.UtcNow;

            await _categoryRepository.UpdateAsync(category);

            return MapToResponseDto(category);
        }

        public async Task DeleteAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);

            if (category is null)
            {
                throw new NotFoundException("Category was not found.");
            }

            var hasEvents =
                await _categoryRepository.HasEventsAsync(id);

            if (hasEvents)
            {
                throw new ConflictException(
                    "Category cannot be deleted because it is assigned to existing events.");
            }

            await _categoryRepository.DeleteAsync(category);
        }

        private static CategoryResponseDto MapToResponseDto(
            EventCategory category)
        {
            return new CategoryResponseDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt
            };
        }
    }
}