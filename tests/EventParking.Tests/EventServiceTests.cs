using Event_and_parking_reservation_system.DTOs.Events;
using Event_and_parking_reservation_system.Exceptions;
using Event_and_parking_reservation_system.Interfaces.Repositories;
using Event_and_parking_reservation_system.Models;
using Event_and_parking_reservation_system.Services;
using Moq;
using Xunit;

namespace EventParking.Tests
{
    public class EventServiceTests
    {
        private readonly Mock<IEventRepository> _eventRepositoryMock;
        private readonly Mock<IVenueRepository> _venueRepositoryMock;
        private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
        private readonly EventService _eventService;

        public EventServiceTests()
        {
            _eventRepositoryMock = new Mock<IEventRepository>();
            _venueRepositoryMock = new Mock<IVenueRepository>();
            _categoryRepositoryMock = new Mock<ICategoryRepository>();

            _eventService = new EventService(
                _eventRepositoryMock.Object,
                _venueRepositoryMock.Object,
                _categoryRepositoryMock.Object);
        }

        private static Venue CreateVenue()
        {
            return new Venue
            {
                Id = 1,
                Name = "Jaffna Hall",
                Address = "Jaffna",
                TotalCapacity = 500
            };
        }

        private static EventCategory CreateCategory()
        {
            return new EventCategory
            {
                Id = 1,
                Name = "Technology",
                Description = "Technology events"
            };
        }

        private static Event CreateEvent()
        {
            return new Event
            {
                Id = 1,
                Name = "Jaffna Tech Expo",
                Description = "Technology exhibition",
                VenueId = 1,
                EventCategoryId = 1,
                StartDateTime = new DateTime(2026, 9, 20, 10, 0, 0),
                EndDateTime = new DateTime(2026, 9, 20, 14, 0, 0),
                TicketPrice = 1500,
                ParkingFee = 300,
                Capacity = 300,
                Venue = CreateVenue(),
                EventCategory = CreateCategory()
            };
        }

        private static CreateEventDto CreateValidCreateDto()
        {
            return new CreateEventDto
            {
                Name = "  Jaffna Tech Expo  ",
                Description = "  Technology exhibition  ",
                VenueId = 1,
                EventCategoryId = 1,
                StartDateTime = new DateTime(2026, 9, 20, 10, 0, 0),
                EndDateTime = new DateTime(2026, 9, 20, 14, 0, 0),
                TicketPrice = 1500,
                ParkingFee = 300,
                Capacity = 300
            };
        }

        private static UpdateEventDto CreateValidUpdateDto()
        {
            return new UpdateEventDto
            {
                Name = "Updated Tech Expo",
                Description = "Updated exhibition",
                VenueId = 1,
                EventCategoryId = 1,
                StartDateTime = new DateTime(2026, 9, 21, 10, 0, 0),
                EndDateTime = new DateTime(2026, 9, 21, 15, 0, 0),
                TicketPrice = 1800,
                ParkingFee = 350,
                Capacity = 350
            };
        }

        [Fact]
        public async Task GetAllAsync_WhenEventsExist_ReturnsEvents()
        {
            var filter = new EventFilterDto();

            var events = new List<Event>
            {
                CreateEvent()
            };

            _eventRepositoryMock
                .Setup(repository => repository.GetAllAsync(filter))
                .ReturnsAsync(events);

            var result =
                (await _eventService.GetAllAsync(filter))
                .ToList();

            Assert.Single(result);
            Assert.Equal("Jaffna Tech Expo", result[0].Name);
            Assert.Equal("Jaffna Hall", result[0].VenueName);
            Assert.Equal("Technology", result[0].CategoryName);

            _eventRepositoryMock.Verify(
                repository => repository.GetAllAsync(filter),
                Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_WhenEventExists_ReturnsEvent()
        {
            var eventEntity = CreateEvent();

            _eventRepositoryMock
                .Setup(repository => repository.GetByIdAsync(1))
                .ReturnsAsync(eventEntity);

            var result =
                await _eventService.GetByIdAsync(1);

            Assert.Equal(1, result.Id);
            Assert.Equal("Jaffna Tech Expo", result.Name);
            Assert.Equal(1, result.VenueId);
            Assert.Equal(1, result.EventCategoryId);
            Assert.Equal(300, result.Capacity);
        }

        [Fact]
        public async Task GetByIdAsync_WhenEventDoesNotExist_ThrowsNotFoundException()
        {
            _eventRepositoryMock
                .Setup(repository => repository.GetByIdAsync(999))
                .ReturnsAsync((Event?)null);

            var exception =
                await Assert.ThrowsAsync<NotFoundException>(
                    () => _eventService.GetByIdAsync(999));

            Assert.Equal(
                "Event was not found.",
                exception.Message);
        }

        [Fact]
        public async Task CreateAsync_WhenDataIsValid_CreatesEvent()
        {
            var venue = CreateVenue();
            var category = CreateCategory();
            var dto = CreateValidCreateDto();

            _venueRepositoryMock
                .Setup(repository => repository.GetByIdAsync(1))
                .ReturnsAsync(venue);

            _categoryRepositoryMock
                .Setup(repository => repository.GetByIdAsync(1))
                .ReturnsAsync(category);

            _eventRepositoryMock
                .Setup(repository =>
                    repository.HasOverlapAsync(
                        1,
                        dto.StartDateTime,
                        dto.EndDateTime,
                        null))
                .ReturnsAsync(false);

            _eventRepositoryMock
                .Setup(repository =>
                    repository.AddAsync(It.IsAny<Event>()))
                .Returns(Task.CompletedTask);

            var result =
                await _eventService.CreateAsync(dto);

            Assert.Equal("Jaffna Tech Expo", result.Name);
            Assert.Equal("Technology exhibition", result.Description);
            Assert.Equal("Jaffna Hall", result.VenueName);
            Assert.Equal("Technology", result.CategoryName);
            Assert.Equal(300, result.Capacity);

            _eventRepositoryMock.Verify(
                repository =>
                    repository.AddAsync(
                        It.Is<Event>(eventEntity =>
                            eventEntity.Name == "Jaffna Tech Expo" &&
                            eventEntity.VenueId == 1 &&
                            eventEntity.EventCategoryId == 1 &&
                            eventEntity.Capacity == 300)),
                Times.Once);
        }

        [Fact]
        public async Task CreateAsync_WhenEndDateIsBeforeStartDate_ThrowsArgumentException()
        {
            var dto = CreateValidCreateDto();

            dto.StartDateTime =
                new DateTime(2026, 9, 20, 14, 0, 0);

            dto.EndDateTime =
                new DateTime(2026, 9, 20, 10, 0, 0);

            var exception =
                await Assert.ThrowsAsync<ArgumentException>(
                    () => _eventService.CreateAsync(dto));

            Assert.Equal(
                "End date/time must be later than start date/time.",
                exception.Message);

            _venueRepositoryMock.Verify(
                repository =>
                    repository.GetByIdAsync(It.IsAny<int>()),
                Times.Never);
        }

        [Fact]
        public async Task CreateAsync_WhenVenueDoesNotExist_ThrowsNotFoundException()
        {
            var dto = CreateValidCreateDto();

            _venueRepositoryMock
                .Setup(repository => repository.GetByIdAsync(1))
                .ReturnsAsync((Venue?)null);

            var exception =
                await Assert.ThrowsAsync<NotFoundException>(
                    () => _eventService.CreateAsync(dto));

            Assert.Equal(
                "Venue was not found.",
                exception.Message);

            _eventRepositoryMock.Verify(
                repository =>
                    repository.AddAsync(It.IsAny<Event>()),
                Times.Never);
        }

        [Fact]
        public async Task CreateAsync_WhenCategoryDoesNotExist_ThrowsNotFoundException()
        {
            var dto = CreateValidCreateDto();

            _venueRepositoryMock
                .Setup(repository => repository.GetByIdAsync(1))
                .ReturnsAsync(CreateVenue());

            _categoryRepositoryMock
                .Setup(repository => repository.GetByIdAsync(1))
                .ReturnsAsync((EventCategory?)null);

            var exception =
                await Assert.ThrowsAsync<NotFoundException>(
                    () => _eventService.CreateAsync(dto));

            Assert.Equal(
                "Category was not found.",
                exception.Message);

            _eventRepositoryMock.Verify(
                repository =>
                    repository.AddAsync(It.IsAny<Event>()),
                Times.Never);
        }

        [Fact]
        public async Task CreateAsync_WhenCapacityExceedsVenueCapacity_ThrowsConflictException()
        {
            var dto = CreateValidCreateDto();
            dto.Capacity = 600;

            _venueRepositoryMock
                .Setup(repository => repository.GetByIdAsync(1))
                .ReturnsAsync(CreateVenue());

            _categoryRepositoryMock
                .Setup(repository => repository.GetByIdAsync(1))
                .ReturnsAsync(CreateCategory());

            var exception =
                await Assert.ThrowsAsync<ConflictException>(
                    () => _eventService.CreateAsync(dto));

            Assert.Equal(
                "Event capacity cannot exceed venue capacity.",
                exception.Message);

            _eventRepositoryMock.Verify(
                repository =>
                    repository.AddAsync(It.IsAny<Event>()),
                Times.Never);
        }

        [Fact]
        public async Task CreateAsync_WhenVenueHasOverlap_ThrowsConflictException()
        {
            var dto = CreateValidCreateDto();

            _venueRepositoryMock
                .Setup(repository => repository.GetByIdAsync(1))
                .ReturnsAsync(CreateVenue());

            _categoryRepositoryMock
                .Setup(repository => repository.GetByIdAsync(1))
                .ReturnsAsync(CreateCategory());

            _eventRepositoryMock
                .Setup(repository =>
                    repository.HasOverlapAsync(
                        1,
                        dto.StartDateTime,
                        dto.EndDateTime,
                        null))
                .ReturnsAsync(true);

            var exception =
                await Assert.ThrowsAsync<ConflictException>(
                    () => _eventService.CreateAsync(dto));

            Assert.Equal(
                "The selected venue already has an overlapping event.",
                exception.Message);

            _eventRepositoryMock.Verify(
                repository =>
                    repository.AddAsync(It.IsAny<Event>()),
                Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_WhenDataIsValid_UpdatesEvent()
        {
            var eventEntity = CreateEvent();
            var dto = CreateValidUpdateDto();

            _eventRepositoryMock
                .Setup(repository => repository.GetByIdAsync(1))
                .ReturnsAsync(eventEntity);

            _venueRepositoryMock
                .Setup(repository => repository.GetByIdAsync(1))
                .ReturnsAsync(CreateVenue());

            _categoryRepositoryMock
                .Setup(repository => repository.GetByIdAsync(1))
                .ReturnsAsync(CreateCategory());

            _eventRepositoryMock
                .Setup(repository =>
                    repository.GetBookedSeatCountAsync(1))
                .ReturnsAsync(100);

            _eventRepositoryMock
                .Setup(repository =>
                    repository.HasOverlapAsync(
                        1,
                        dto.StartDateTime,
                        dto.EndDateTime,
                        1))
                .ReturnsAsync(false);

            _eventRepositoryMock
                .Setup(repository =>
                    repository.UpdateAsync(eventEntity))
                .Returns(Task.CompletedTask);

            var result =
                await _eventService.UpdateAsync(1, dto);

            Assert.Equal("Updated Tech Expo", result.Name);
            Assert.Equal("Updated exhibition", result.Description);
            Assert.Equal(1800, result.TicketPrice);
            Assert.Equal(350, result.ParkingFee);
            Assert.Equal(350, result.Capacity);

            _eventRepositoryMock.Verify(
                repository =>
                    repository.UpdateAsync(eventEntity),
                Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WhenEventDoesNotExist_ThrowsNotFoundException()
        {
            var dto = CreateValidUpdateDto();

            _eventRepositoryMock
                .Setup(repository => repository.GetByIdAsync(999))
                .ReturnsAsync((Event?)null);

            var exception =
                await Assert.ThrowsAsync<NotFoundException>(
                    () => _eventService.UpdateAsync(999, dto));

            Assert.Equal(
                "Event was not found.",
                exception.Message);
        }

        [Fact]
        public async Task UpdateAsync_WhenCapacityExceedsVenueCapacity_ThrowsConflictException()
        {
            var dto = CreateValidUpdateDto();
            dto.Capacity = 600;

            _eventRepositoryMock
                .Setup(repository => repository.GetByIdAsync(1))
                .ReturnsAsync(CreateEvent());

            _venueRepositoryMock
                .Setup(repository => repository.GetByIdAsync(1))
                .ReturnsAsync(CreateVenue());

            _categoryRepositoryMock
                .Setup(repository => repository.GetByIdAsync(1))
                .ReturnsAsync(CreateCategory());

            var exception =
                await Assert.ThrowsAsync<ConflictException>(
                    () => _eventService.UpdateAsync(1, dto));

            Assert.Equal(
                "Event capacity cannot exceed venue capacity.",
                exception.Message);
        }

        [Fact]
        public async Task UpdateAsync_WhenCapacityIsLowerThanBookedSeats_ThrowsConflictException()
        {
            var dto = CreateValidUpdateDto();
            dto.Capacity = 100;

            _eventRepositoryMock
                .Setup(repository => repository.GetByIdAsync(1))
                .ReturnsAsync(CreateEvent());

            _venueRepositoryMock
                .Setup(repository => repository.GetByIdAsync(1))
                .ReturnsAsync(CreateVenue());

            _categoryRepositoryMock
                .Setup(repository => repository.GetByIdAsync(1))
                .ReturnsAsync(CreateCategory());

            _eventRepositoryMock
                .Setup(repository =>
                    repository.GetBookedSeatCountAsync(1))
                .ReturnsAsync(150);

            var exception =
                await Assert.ThrowsAsync<ConflictException>(
                    () => _eventService.UpdateAsync(1, dto));

            Assert.Equal(
                "Event capacity cannot be lower than the number of booked seats.",
                exception.Message);

            _eventRepositoryMock.Verify(
                repository =>
                    repository.UpdateAsync(It.IsAny<Event>()),
                Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_WhenVenueHasOverlap_ThrowsConflictException()
        {
            var dto = CreateValidUpdateDto();

            _eventRepositoryMock
                .Setup(repository => repository.GetByIdAsync(1))
                .ReturnsAsync(CreateEvent());

            _venueRepositoryMock
                .Setup(repository => repository.GetByIdAsync(1))
                .ReturnsAsync(CreateVenue());

            _categoryRepositoryMock
                .Setup(repository => repository.GetByIdAsync(1))
                .ReturnsAsync(CreateCategory());

            _eventRepositoryMock
                .Setup(repository =>
                    repository.GetBookedSeatCountAsync(1))
                .ReturnsAsync(100);

            _eventRepositoryMock
                .Setup(repository =>
                    repository.HasOverlapAsync(
                        1,
                        dto.StartDateTime,
                        dto.EndDateTime,
                        1))
                .ReturnsAsync(true);

            var exception =
                await Assert.ThrowsAsync<ConflictException>(
                    () => _eventService.UpdateAsync(1, dto));

            Assert.Equal(
                "The selected venue already has an overlapping event.",
                exception.Message);

            _eventRepositoryMock.Verify(
                repository =>
                    repository.UpdateAsync(It.IsAny<Event>()),
                Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_WhenEventExists_DeletesEvent()
        {
            var eventEntity = CreateEvent();

            _eventRepositoryMock
                .Setup(repository => repository.GetByIdAsync(1))
                .ReturnsAsync(eventEntity);

            _eventRepositoryMock
                .Setup(repository =>
                    repository.DeleteAsync(eventEntity))
                .Returns(Task.CompletedTask);

            await _eventService.DeleteAsync(1);

            _eventRepositoryMock.Verify(
                repository =>
                    repository.DeleteAsync(eventEntity),
                Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_WhenEventDoesNotExist_ThrowsNotFoundException()
        {
            _eventRepositoryMock
                .Setup(repository => repository.GetByIdAsync(999))
                .ReturnsAsync((Event?)null);

            var exception =
                await Assert.ThrowsAsync<NotFoundException>(
                    () => _eventService.DeleteAsync(999));

            Assert.Equal(
                "Event was not found.",
                exception.Message);

            _eventRepositoryMock.Verify(
                repository =>
                    repository.DeleteAsync(It.IsAny<Event>()),
                Times.Never);
        }
    }
}