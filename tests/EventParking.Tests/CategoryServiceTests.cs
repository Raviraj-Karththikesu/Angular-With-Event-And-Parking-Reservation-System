using Event_and_parking_reservation_system.DTOs.Categories;
using Event_and_parking_reservation_system.Exceptions;
using Event_and_parking_reservation_system.Interfaces.Repositories;
using Event_and_parking_reservation_system.Models;
using Event_and_parking_reservation_system.Services;
using Moq;
using Xunit;

namespace EventParking.Tests
{
    public class CategoryServiceTests
    {
        private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
        private readonly CategoryService _categoryService;

        public CategoryServiceTests()
        {
            _categoryRepositoryMock = new Mock<ICategoryRepository>();
            _categoryService = new CategoryService(_categoryRepositoryMock.Object);
        }

        [Fact]
        public async Task GetAllAsync_WhenCategoriesExist_ReturnsCategories()
        {
            var categories = new List<EventCategory>
            {
                new EventCategory
                {
                    Id = 1,
                    Name = "Concert",
                    Description = "Music events"
                },
                new EventCategory
                {
                    Id = 2,
                    Name = "Workshop",
                    Description = "Training events"
                }
            };

            _categoryRepositoryMock
                .Setup(repository => repository.GetAllAsync())
                .ReturnsAsync(categories);

            var result =
                (await _categoryService.GetAllAsync())
                .ToList();

            Assert.Equal(2, result.Count);
            Assert.Equal("Concert", result[0].Name);
            Assert.Equal("Workshop", result[1].Name);

            _categoryRepositoryMock.Verify(
                repository => repository.GetAllAsync(),
                Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_WhenCategoryExists_ReturnsCategory()
        {
            var category = new EventCategory
            {
                Id = 1,
                Name = "Concert",
                Description = "Music events"
            };

            _categoryRepositoryMock
                .Setup(repository => repository.GetByIdAsync(1))
                .ReturnsAsync(category);

            var result =
                await _categoryService.GetByIdAsync(1);

            Assert.Equal(1, result.Id);
            Assert.Equal("Concert", result.Name);
            Assert.Equal("Music events", result.Description);
        }

        [Fact]
        public async Task GetByIdAsync_WhenCategoryDoesNotExist_ThrowsNotFoundException()
        {
            _categoryRepositoryMock
                .Setup(repository => repository.GetByIdAsync(999))
                .ReturnsAsync((EventCategory?)null);

            var exception =
                await Assert.ThrowsAsync<NotFoundException>(
                    () => _categoryService.GetByIdAsync(999));

            Assert.Equal(
                "Category was not found.",
                exception.Message);
        }

        [Fact]
        public async Task CreateAsync_WhenDataIsValid_CreatesCategory()
        {
            var dto = new CreateCategoryDto
            {
                Name = "  Concert  ",
                Description = "  Music events  "
            };

            _categoryRepositoryMock
                .Setup(repository => repository.GetByNameAsync("Concert"))
                .ReturnsAsync((EventCategory?)null);

            _categoryRepositoryMock
                .Setup(repository =>
                    repository.AddAsync(It.IsAny<EventCategory>()))
                .Returns(Task.CompletedTask);

            var result =
                await _categoryService.CreateAsync(dto);

            Assert.Equal("Concert", result.Name);
            Assert.Equal("Music events", result.Description);

            _categoryRepositoryMock.Verify(
                repository =>
                    repository.AddAsync(
                        It.Is<EventCategory>(category =>
                            category.Name == "Concert" &&
                            category.Description == "Music events")),
                Times.Once);
        }

        [Fact]
        public async Task CreateAsync_WhenCategoryNameAlreadyExists_ThrowsConflictException()
        {
            var existingCategory = new EventCategory
            {
                Id = 1,
                Name = "Concert"
            };

            _categoryRepositoryMock
                .Setup(repository => repository.GetByNameAsync("Concert"))
                .ReturnsAsync(existingCategory);

            var dto = new CreateCategoryDto
            {
                Name = "Concert",
                Description = "Duplicate category"
            };

            var exception =
                await Assert.ThrowsAsync<ConflictException>(
                    () => _categoryService.CreateAsync(dto));

            Assert.Equal(
                "A category with this name already exists.",
                exception.Message);

            _categoryRepositoryMock.Verify(
                repository =>
                    repository.AddAsync(It.IsAny<EventCategory>()),
                Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_WhenCategoryExists_UpdatesCategory()
        {
            var category = new EventCategory
            {
                Id = 1,
                Name = "Old Name",
                Description = "Old description"
            };

            _categoryRepositoryMock
                .Setup(repository => repository.GetByIdAsync(1))
                .ReturnsAsync(category);

            _categoryRepositoryMock
                .Setup(repository => repository.GetByNameAsync("Concert"))
                .ReturnsAsync((EventCategory?)null);

            _categoryRepositoryMock
                .Setup(repository =>
                    repository.UpdateAsync(It.IsAny<EventCategory>()))
                .Returns(Task.CompletedTask);

            var dto = new UpdateCategoryDto
            {
                Name = "Concert",
                Description = "Updated music events"
            };

            var result =
                await _categoryService.UpdateAsync(1, dto);

            Assert.Equal("Concert", result.Name);
            Assert.Equal("Updated music events", result.Description);

            _categoryRepositoryMock.Verify(
                repository => repository.UpdateAsync(category),
                Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WhenCategoryDoesNotExist_ThrowsNotFoundException()
        {
            _categoryRepositoryMock
                .Setup(repository => repository.GetByIdAsync(999))
                .ReturnsAsync((EventCategory?)null);

            var dto = new UpdateCategoryDto
            {
                Name = "Concert",
                Description = "Music events"
            };

            await Assert.ThrowsAsync<NotFoundException>(
                () => _categoryService.UpdateAsync(999, dto));

            _categoryRepositoryMock.Verify(
                repository =>
                    repository.UpdateAsync(It.IsAny<EventCategory>()),
                Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_WhenAnotherCategoryHasSameName_ThrowsConflictException()
        {
            var category = new EventCategory
            {
                Id = 1,
                Name = "Workshop"
            };

            var existingCategory = new EventCategory
            {
                Id = 2,
                Name = "Concert"
            };

            _categoryRepositoryMock
                .Setup(repository => repository.GetByIdAsync(1))
                .ReturnsAsync(category);

            _categoryRepositoryMock
                .Setup(repository => repository.GetByNameAsync("Concert"))
                .ReturnsAsync(existingCategory);

            var dto = new UpdateCategoryDto
            {
                Name = "Concert",
                Description = "Duplicate name"
            };

            var exception =
                await Assert.ThrowsAsync<ConflictException>(
                    () => _categoryService.UpdateAsync(1, dto));

            Assert.Equal(
                "A category with this name already exists.",
                exception.Message);

            _categoryRepositoryMock.Verify(
                repository =>
                    repository.UpdateAsync(It.IsAny<EventCategory>()),
                Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_WhenCategoryHasNoEvents_DeletesCategory()
        {
            var category = new EventCategory
            {
                Id = 1,
                Name = "Workshop"
            };

            _categoryRepositoryMock
                .Setup(repository => repository.GetByIdAsync(1))
                .ReturnsAsync(category);

            _categoryRepositoryMock
                .Setup(repository => repository.HasEventsAsync(1))
                .ReturnsAsync(false);

            _categoryRepositoryMock
                .Setup(repository =>
                    repository.DeleteAsync(category))
                .Returns(Task.CompletedTask);

            await _categoryService.DeleteAsync(1);

            _categoryRepositoryMock.Verify(
                repository => repository.DeleteAsync(category),
                Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_WhenCategoryHasEvents_ThrowsConflictException()
        {
            var category = new EventCategory
            {
                Id = 1,
                Name = "Concert"
            };

            _categoryRepositoryMock
                .Setup(repository => repository.GetByIdAsync(1))
                .ReturnsAsync(category);

            _categoryRepositoryMock
                .Setup(repository => repository.HasEventsAsync(1))
                .ReturnsAsync(true);

            var exception =
                await Assert.ThrowsAsync<ConflictException>(
                    () => _categoryService.DeleteAsync(1));

            Assert.Equal(
                "Category cannot be deleted because it is assigned to existing events.",
                exception.Message);

            _categoryRepositoryMock.Verify(
                repository =>
                    repository.DeleteAsync(It.IsAny<EventCategory>()),
                Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_WhenCategoryDoesNotExist_ThrowsNotFoundException()
        {
            _categoryRepositoryMock
                .Setup(repository => repository.GetByIdAsync(999))
                .ReturnsAsync((EventCategory?)null);

            await Assert.ThrowsAsync<NotFoundException>(
                () => _categoryService.DeleteAsync(999));

            _categoryRepositoryMock.Verify(
                repository =>
                    repository.DeleteAsync(It.IsAny<EventCategory>()),
                Times.Never);
        }
    }
}