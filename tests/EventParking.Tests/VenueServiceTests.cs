using Event_and_parking_reservation_system.DTOs.Venues;
using Event_and_parking_reservation_system.Exceptions;
using Event_and_parking_reservation_system.Interfaces.Repositories;
using Event_and_parking_reservation_system.Models;
using Event_and_parking_reservation_system.Services;
using Moq;
using Xunit;

namespace EventParking.Tests
{
    public class VenueServiceTests
    {
        private readonly Mock<IVenueRepository> _venueRepositoryMock;
        private readonly VenueService _venueService;

        public VenueServiceTests()
        {
            _venueRepositoryMock = new Mock<IVenueRepository>();
            _venueService = new VenueService(_venueRepositoryMock.Object);
        }

        [Fact]
        public async Task GetAllAsync_WhenVenuesExist_ReturnsVenues()
        {
            var venues = new List<Venue>
            {
                new Venue
                {
                    Id = 1,
                    Name = "Jaffna Hall",
                    Address = "Jaffna",
                    TotalCapacity = 500
                },
                new Venue
                {
                    Id = 2,
                    Name = "Colombo Hall",
                    Address = "Colombo",
                    TotalCapacity = 1000
                }
            };

            _venueRepositoryMock
                .Setup(repository => repository.GetAllAsync())
                .ReturnsAsync(venues);

            var result =
                (await _venueService.GetAllAsync())
                .ToList();

            Assert.Equal(2, result.Count);
            Assert.Equal("Jaffna Hall", result[0].Name);
            Assert.Equal("Colombo Hall", result[1].Name);

            _venueRepositoryMock.Verify(
                repository => repository.GetAllAsync(),
                Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_WhenVenueExists_ReturnsVenue()
        {
            var venue = new Venue
            {
                Id = 1,
                Name = "Jaffna Hall",
                Address = "Jaffna",
                TotalCapacity = 500
            };

            _venueRepositoryMock
                .Setup(repository => repository.GetByIdAsync(1))
                .ReturnsAsync(venue);

            var result =
                await _venueService.GetByIdAsync(1);

            Assert.Equal(1, result.Id);
            Assert.Equal("Jaffna Hall", result.Name);
            Assert.Equal("Jaffna", result.Address);
            Assert.Equal(500, result.TotalCapacity);
        }

        [Fact]
        public async Task GetByIdAsync_WhenVenueDoesNotExist_ThrowsNotFoundException()
        {
            _venueRepositoryMock
                .Setup(repository => repository.GetByIdAsync(999))
                .ReturnsAsync((Venue?)null);

            var exception =
                await Assert.ThrowsAsync<NotFoundException>(
                    () => _venueService.GetByIdAsync(999));

            Assert.Equal(
                "Venue was not found.",
                exception.Message);
        }

        [Fact]
        public async Task CreateAsync_WhenDataIsValid_CreatesVenue()
        {
            var dto = new CreateVenueDto
            {
                Name = "  Jaffna Hall  ",
                Address = "  Jaffna  ",
                TotalCapacity = 500
            };

            _venueRepositoryMock
                .Setup(repository =>
                    repository.AddAsync(It.IsAny<Venue>()))
                .Returns(Task.CompletedTask);

            var result =
                await _venueService.CreateAsync(dto);

            Assert.Equal("Jaffna Hall", result.Name);
            Assert.Equal("Jaffna", result.Address);
            Assert.Equal(500, result.TotalCapacity);

            _venueRepositoryMock.Verify(
                repository =>
                    repository.AddAsync(
                        It.Is<Venue>(venue =>
                            venue.Name == "Jaffna Hall" &&
                            venue.Address == "Jaffna" &&
                            venue.TotalCapacity == 500)),
                Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WhenVenueExists_UpdatesVenue()
        {
            var venue = new Venue
            {
                Id = 1,
                Name = "Old Hall",
                Address = "Old Address",
                TotalCapacity = 300
            };

            _venueRepositoryMock
                .Setup(repository => repository.GetByIdAsync(1))
                .ReturnsAsync(venue);

            _venueRepositoryMock
                .Setup(repository =>
                    repository.UpdateAsync(It.IsAny<Venue>()))
                .Returns(Task.CompletedTask);

            var dto = new UpdateVenueDto
            {
                Name = "Updated Hall",
                Address = "Jaffna",
                TotalCapacity = 600
            };

            var result =
                await _venueService.UpdateAsync(1, dto);

            Assert.Equal("Updated Hall", result.Name);
            Assert.Equal("Jaffna", result.Address);
            Assert.Equal(600, result.TotalCapacity);

            _venueRepositoryMock.Verify(
                repository =>
                    repository.UpdateAsync(venue),
                Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WhenVenueDoesNotExist_ThrowsNotFoundException()
        {
            _venueRepositoryMock
                .Setup(repository => repository.GetByIdAsync(999))
                .ReturnsAsync((Venue?)null);

            var dto = new UpdateVenueDto
            {
                Name = "Updated Hall",
                Address = "Jaffna",
                TotalCapacity = 500
            };

            await Assert.ThrowsAsync<NotFoundException>(
                () => _venueService.UpdateAsync(999, dto));

            _venueRepositoryMock.Verify(
                repository =>
                    repository.UpdateAsync(
                        It.IsAny<Venue>()),
                Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_WhenVenueHasNoUpcomingEvents_DeletesVenue()
        {
            var venue = new Venue
            {
                Id = 1,
                Name = "Jaffna Hall",
                Address = "Jaffna",
                TotalCapacity = 500
            };

            _venueRepositoryMock
                .Setup(repository => repository.GetByIdAsync(1))
                .ReturnsAsync(venue);

            _venueRepositoryMock
                .Setup(repository =>
                    repository.HasUpcomingEventsAsync(1))
                .ReturnsAsync(false);

            _venueRepositoryMock
                .Setup(repository =>
                    repository.DeleteAsync(venue))
                .Returns(Task.CompletedTask);

            await _venueService.DeleteAsync(1);

            _venueRepositoryMock.Verify(
                repository =>
                    repository.DeleteAsync(venue),
                Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_WhenVenueHasUpcomingEvents_ThrowsConflictException()
        {
            var venue = new Venue
            {
                Id = 1,
                Name = "Jaffna Hall",
                Address = "Jaffna",
                TotalCapacity = 500
            };

            _venueRepositoryMock
                .Setup(repository => repository.GetByIdAsync(1))
                .ReturnsAsync(venue);

            _venueRepositoryMock
                .Setup(repository =>
                    repository.HasUpcomingEventsAsync(1))
                .ReturnsAsync(true);

            var exception =
                await Assert.ThrowsAsync<ConflictException>(
                    () => _venueService.DeleteAsync(1));

            Assert.Equal(
                "Venue cannot be deleted because it has upcoming events.",
                exception.Message);

            _venueRepositoryMock.Verify(
                repository =>
                    repository.DeleteAsync(
                        It.IsAny<Venue>()),
                Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_WhenVenueDoesNotExist_ThrowsNotFoundException()
        {
            _venueRepositoryMock
                .Setup(repository => repository.GetByIdAsync(999))
                .ReturnsAsync((Venue?)null);

            await Assert.ThrowsAsync<NotFoundException>(
                () => _venueService.DeleteAsync(999));

            _venueRepositoryMock.Verify(
                repository =>
                    repository.DeleteAsync(
                        It.IsAny<Venue>()),
                Times.Never);
        }

        [Fact]
        public async Task CheckAvailabilityAsync_WhenVenueIsAvailable_ReturnsAvailable()
        {
            var venues = new List<Venue>
            {
                new Venue
                {
                    Id = 1,
                    Name = "Jaffna Hall",
                    Address = "Jaffna",
                    TotalCapacity = 500
                }
            };

            _venueRepositoryMock
                .Setup(repository =>
                    repository.GetAllAsync())
                .ReturnsAsync(venues);

            _venueRepositoryMock
                .Setup(repository =>
                    repository.IsAvailableAsync(
                        1,
                        It.IsAny<DateOnly>(),
                        It.IsAny<TimeOnly>(),
                        It.IsAny<TimeOnly>(),
                        null))
                .ReturnsAsync(true);

            var query = new VenueAvailabilityQueryDto
            {
                VenueId = 1,
                Date = new DateOnly(2026, 9, 20),
                StartTime = new TimeOnly(10, 0),
                EndTime = new TimeOnly(12, 0)
            };

            var result =
                (await _venueService
                    .CheckAvailabilityAsync(query))
                .ToList();

            Assert.Single(result);
            Assert.Equal(1, result[0].VenueId);
            Assert.Equal(
                "Jaffna Hall",
                result[0].VenueName);
            Assert.True(result[0].IsAvailable);

            _venueRepositoryMock.Verify(
                repository =>
                    repository.IsAvailableAsync(
                        1,
                        query.Date,
                        query.StartTime,
                        query.EndTime,
                        null),
                Times.Once);
        }

        [Fact]
        public async Task CheckAvailabilityAsync_WhenEndTimeIsBeforeStartTime_ThrowsArgumentException()
        {
            var query = new VenueAvailabilityQueryDto
            {
                Date = new DateOnly(2026, 9, 20),
                StartTime = new TimeOnly(12, 0),
                EndTime = new TimeOnly(10, 0)
            };

            var exception =
                await Assert.ThrowsAsync<ArgumentException>(
                    () =>
                        _venueService
                            .CheckAvailabilityAsync(query));

            Assert.Equal(
                "End time must be later than start time.",
                exception.Message);

            _venueRepositoryMock.Verify(
                repository =>
                    repository.GetAllAsync(),
                Times.Never);
        }
    }
}