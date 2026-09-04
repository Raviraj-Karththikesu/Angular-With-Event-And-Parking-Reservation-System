using Event_and_parking_reservation_system.DTOs.Categories;

namespace Event_and_parking_reservation_system.Interfaces.Services
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryResponseDto>> GetAllAsync();

        Task<CategoryResponseDto> GetByIdAsync(int id);

        Task<CategoryResponseDto> CreateAsync(CreateCategoryDto dto);

        Task<CategoryResponseDto> UpdateAsync(
            int id,
            UpdateCategoryDto dto);

        Task DeleteAsync(int id);
    }
}