using AutoMapper;
using Event_Management_System.API.Application;
using Event_Management_System.API.Domain.DTOs;
using Event_Management_System.API.Domain.Entities;
using Event_Management_System.API.Infrastructures;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;

namespace Event_Management_System.Tests.EventCentreTests
{
    [TestFixture]
    public class EventCentreTests
    {
        private EventCentreService _eventCentreService;
        private Mock<ILogger<EventCentreService>> _loggerMock;
        private ApplicationDbContext _dbContext;
        private Mock<UserManager<ApplicationUser>> _userManagerMock;
        private Mock<IMapper> _mapperMock;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"EventCentreTests-{Guid.NewGuid()}")
                .Options;
            _dbContext = new ApplicationDbContext(options);
            _loggerMock = new Mock<ILogger<EventCentreService>>();
            var store = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
            _mapperMock = new Mock<IMapper>();
            _eventCentreService = new EventCentreService(
                _loggerMock.Object,
                _dbContext,
                _userManagerMock.Object,
                _mapperMock.Object
            );  
        }

        [TearDown]
        public void TearDown()
        {
            _dbContext.Dispose();
        }

        #region AddEventCentre Tests
        [Test]
        public async Task AddEventCentre_WithValidData_ReturnsCreatedEntity()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new ApplicationUser { Id = userId, UserName = "organizer@test.com" };

            var addEventCentreDto = new AddEventCentreDto
            {
                Name = "Test Event Centre",
                Location = "Test Location",
                Capacity = 100,
                Description = "Test Description"
            };
            _userManagerMock
                .Setup(um => um.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            var mappedDto = new EventCentreDto 
            { 
                Name = addEventCentreDto.Name, 
                Location = addEventCentreDto.Location, 
                Capacity = addEventCentreDto.Capacity, 
                Description = addEventCentreDto.Description 
            };

            _mapperMock
                .Setup(m => m.Map<EventCentreDto>(It.IsAny<EventCentre>()))
                .Returns(mappedDto);

            // Act
            var response = await _eventCentreService.AddEventCentre(userId, addEventCentreDto);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(response.StatusMessage, Is.EqualTo("Request successful"));
            Assert.That(response.Data, Is.Not.Null);
            var eventCentre = response.Data as EventCentreDto;
            Assert.That(eventCentre.Name, Is.EqualTo(addEventCentreDto.Name));
            Assert.That(await _dbContext.EventCentres.CountAsync(), Is.EqualTo(1));
            Assert.That(await _dbContext.AuditLogs.CountAsync(), Is.EqualTo(1));
        }

        [Test]
        public async Task AddEventCentre_WithInvalidUserId_ReturnsBadRequest()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var addEventCentreDto = new AddEventCentreDto
            {
                Name = "Test Event Centre",
                Location = "Test Location",
                Capacity = 100,
                Description = "Test Description"
            };
            _userManagerMock
              .Setup(um => um.FindByIdAsync(userId.ToString()))
              .ReturnsAsync((ApplicationUser)null!);

            // Act
            var response = await _eventCentreService.AddEventCentre(userId, addEventCentreDto);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(response.StatusMessage, Is.EqualTo("User does not exist"));
            Assert.That(response.Data, Is.Null);
            Assert.That(await _dbContext.EventCentres.CountAsync(), Is.EqualTo(0));
            Assert.That(await _dbContext.AuditLogs.CountAsync(), Is.EqualTo(0));
        }
        #endregion

        #region GetEventCentreById Tests
        [Test]
        public async Task GetEventCentreById_WithValidId_ReturnsEventCentre()
        {
            // Arrange
            var eventCentreId = Guid.NewGuid();
            var eventCentre = new EventCentre
            {
                Id = eventCentreId,
                Name = "Test Event Centre",
                Location = "Test Location",
                Capacity = 100,
                Description = "Test Description",
                IsActive = true
            };
            await _dbContext.EventCentres.AddAsync(eventCentre);
            await _dbContext.SaveChangesAsync();

            var mappedDto = new EventCentreDto
            {
                Id = eventCentreId,
                Name = eventCentre.Name,
                Location = eventCentre.Location,
                Capacity = eventCentre.Capacity,
                Description = eventCentre.Description
            };

            _mapperMock
                .Setup(m => m.Map<EventCentreDto>(It.IsAny<EventCentre>()))
                .Returns(mappedDto);

            // Act
            var response = await _eventCentreService.GetEventCentreById(eventCentreId);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.StatusMessage, Is.EqualTo("Request successful"));
            Assert.That(response.Data, Is.Not.Null);
            Assert.That(response.Data.Id, Is.EqualTo(eventCentreId));
            Assert.That(response.Data.Name, Is.EqualTo(eventCentre.Name));
        }

        [Test]
        public async Task GetEventCentreById_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var eventCentreId = Guid.NewGuid();

            // Act
            var response = await _eventCentreService.GetEventCentreById(eventCentreId);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(response.StatusMessage, Is.EqualTo("Event center does not exist"));
            Assert.That(response.Data, Is.Null);
        }
        #endregion

        #region GetAllEventCentresAsync Tests
        [Test]
        public async Task GetAllEventCentresAsync_WithActiveEventCentres_ReturnsPagedList()
        {
            // Arrange
            var eventCentres = new List<EventCentre>
            {
                new EventCentre { Id = Guid.NewGuid(), Name = "Centre 1", Location = "Location 1", Capacity = 100, IsActive = true, CreatedDate = DateTimeOffset.UtcNow.AddDays(-2) },
                new EventCentre { Id = Guid.NewGuid(), Name = "Centre 2", Location = "Location 2", Capacity = 200, IsActive = true, CreatedDate = DateTimeOffset.UtcNow.AddDays(-1) },
                new EventCentre { Id = Guid.NewGuid(), Name = "Centre 3", Location = "Location 3", Capacity = 300, IsActive = false, CreatedDate = DateTimeOffset.UtcNow }
            };
            await _dbContext.EventCentres.AddRangeAsync(eventCentres);
            await _dbContext.SaveChangesAsync();

            var requestParameters = new RequestParameters { PageNumber = 1, PageSize = 10 };

            _mapperMock
                .Setup(m => m.Map<List<EventCentreDto>>(It.IsAny<List<EventCentre>>()))
                .Returns((List<EventCentre> source) => source.Select(ec => new EventCentreDto
                {
                    Id = ec.Id,
                    Name = ec.Name,
                    Location = ec.Location,
                    Capacity = ec.Capacity
                }).ToList());

            // Act
            var response = await _eventCentreService.GetAllEventCentresAsync(requestParameters);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.StatusMessage, Is.EqualTo("Request successful"));
            Assert.That(response.Data, Is.Not.Null);
            Assert.That(response.Data.Data.Count, Is.EqualTo(2)); // Only active ones
            Assert.That(response.Data.MetaData.TotalCount, Is.EqualTo(2));
        }

        [Test]
        public async Task GetAllEventCentresAsync_WithNoEventCentres_ReturnsEmptyList()
        {
            // Arrange
            var requestParameters = new RequestParameters { PageNumber = 1, PageSize = 10 };

            // Act
            var response = await _eventCentreService.GetAllEventCentresAsync(requestParameters);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.StatusMessage, Is.EqualTo("No event centres found"));
            Assert.That(response.Data.Data.Count, Is.EqualTo(0));
        }
        #endregion

        #region UpdateEventCentreAsync Tests
        [Test]
        public async Task UpdateEventCentreAsync_WithValidData_UpdatesEventCentre()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new ApplicationUser { Id = userId, UserName = "organizer@test.com" };
            var eventCentreId = Guid.NewGuid();
            var eventCentre = new EventCentre
            {
                Id = eventCentreId,
                Name = "Old Name",
                Location = "Old Location",
                Capacity = 100,
                Description = "Old Description",
                IsActive = true
            };
            await _dbContext.EventCentres.AddAsync(eventCentre);
            await _dbContext.SaveChangesAsync();

            _userManagerMock
                .Setup(um => um.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            var updateDto = new AddEventCentreDto
            {
                Name = "New Name",
                Location = "New Location",
                Capacity = 200,
                Description = "New Description"
            };

            // Act
            var response = await _eventCentreService.UpdateEventCentreAsync(userId, eventCentreId, updateDto);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
            Assert.That(response.StatusMessage, Is.EqualTo("Request Successful"));
            
            var updatedEventCentre = await _dbContext.EventCentres.FindAsync(eventCentreId);
            Assert.That(updatedEventCentre!.Name, Is.EqualTo("New Name"));
            Assert.That(updatedEventCentre.Location, Is.EqualTo("New Location"));
            Assert.That(updatedEventCentre.Capacity, Is.EqualTo(200));
            Assert.That(await _dbContext.AuditLogs.CountAsync(), Is.EqualTo(1));
        }

        [Test]
        public async Task UpdateEventCentreAsync_WithInvalidUser_ReturnsBadRequest()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var eventCentreId = Guid.NewGuid();
            var updateDto = new AddEventCentreDto
            {
                Name = "New Name",
                Location = "New Location",
                Capacity = 200
            };

            _userManagerMock
                .Setup(um => um.FindByIdAsync(userId.ToString()))
                .ReturnsAsync((ApplicationUser)null!);

            // Act
            var response = await _eventCentreService.UpdateEventCentreAsync(userId, eventCentreId, updateDto);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(response.StatusMessage, Is.EqualTo("User does not exist"));
        }

        [Test]
        public async Task UpdateEventCentreAsync_WithInvalidEventCentreId_ReturnsNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new ApplicationUser { Id = userId, UserName = "organizer@test.com" };
            var eventCentreId = Guid.NewGuid();
            var updateDto = new AddEventCentreDto
            {
                Name = "New Name",
                Location = "New Location",
                Capacity = 200
            };

            _userManagerMock
                .Setup(um => um.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            // Act
            var response = await _eventCentreService.UpdateEventCentreAsync(userId, eventCentreId, updateDto);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(response.StatusMessage, Is.EqualTo("Event centre does not exist"));
        }
        #endregion

        #region DeleteEventCentre Tests
        [Test]
        public async Task DeleteEventCentre_WithValidId_SoftDeletesEventCentre()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new ApplicationUser { Id = userId, UserName = "organizer@test.com" };
            var eventCentreId = Guid.NewGuid();
            var eventCentre = new EventCentre
            {
                Id = eventCentreId,
                Name = "Test Centre",
                Location = "Test Location",
                Capacity = 100,
                IsActive = true
            };
            await _dbContext.EventCentres.AddAsync(eventCentre);
            await _dbContext.SaveChangesAsync();

            _userManagerMock
                .Setup(um => um.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            // Act
            var response = await _eventCentreService.DeleteEventCentre(userId, eventCentreId);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
            Assert.That(response.StatusMessage, Is.EqualTo("Request Successful"));
            
            var deletedEventCentre = await _dbContext.EventCentres.FindAsync(eventCentreId);
            Assert.That(deletedEventCentre!.IsActive, Is.False);
            Assert.That(await _dbContext.AuditLogs.CountAsync(), Is.EqualTo(1));
        }

        [Test]
        public async Task DeleteEventCentre_WithInvalidUser_ReturnsBadRequest()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var eventCentreId = Guid.NewGuid();

            _userManagerMock
                .Setup(um => um.FindByIdAsync(userId.ToString()))
                .ReturnsAsync((ApplicationUser)null!);

            // Act
            var response = await _eventCentreService.DeleteEventCentre(userId, eventCentreId);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(response.StatusMessage, Is.EqualTo("User does not exist"));
        }

        [Test]
        public async Task DeleteEventCentre_WithInvalidEventCentreId_ReturnsNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new ApplicationUser { Id = userId, UserName = "organizer@test.com" };
            var eventCentreId = Guid.NewGuid();

            _userManagerMock
                .Setup(um => um.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            // Act
            var response = await _eventCentreService.DeleteEventCentre(userId, eventCentreId);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(response.StatusMessage, Is.EqualTo("Event centre does not exist"));
        }
        #endregion

        #region ReactivateEventCentre Tests
        [Test]
        public async Task ReactivateEventCentre_WithValidId_ReactivatesEventCentre()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new ApplicationUser { Id = userId, UserName = "organizer@test.com" };
            var eventCentreId = Guid.NewGuid();
            var eventCentre = new EventCentre
            {
                Id = eventCentreId,
                Name = "Test Centre",
                Location = "Test Location",
                Capacity = 100,
                IsActive = false
            };
            await _dbContext.EventCentres.AddAsync(eventCentre);
            await _dbContext.SaveChangesAsync();

            _userManagerMock
                .Setup(um => um.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            // Act
            var response = await _eventCentreService.ReactivateEventCentre(userId, eventCentreId);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
            Assert.That(response.StatusMessage, Is.EqualTo("Request Successful"));
            
            var reactivatedEventCentre = await _dbContext.EventCentres.FindAsync(eventCentreId);
            Assert.That(reactivatedEventCentre!.IsActive, Is.True);
            Assert.That(await _dbContext.AuditLogs.CountAsync(), Is.EqualTo(1));
        }

        [Test]
        public async Task ReactivateEventCentre_WithInvalidUser_ReturnsBadRequest()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var eventCentreId = Guid.NewGuid();

            _userManagerMock
                .Setup(um => um.FindByIdAsync(userId.ToString()))
                .ReturnsAsync((ApplicationUser)null!);

            // Act
            var response = await _eventCentreService.ReactivateEventCentre(userId, eventCentreId);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(response.StatusMessage, Is.EqualTo("User does not exist"));
        }

        [Test]
        public async Task ReactivateEventCentre_WithInvalidEventCentreId_ReturnsNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new ApplicationUser { Id = userId, UserName = "organizer@test.com" };
            var eventCentreId = Guid.NewGuid();

            _userManagerMock
                .Setup(um => um.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            // Act
            var response = await _eventCentreService.ReactivateEventCentre(userId, eventCentreId);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(response.StatusMessage, Is.EqualTo("Event center does not exist"));
        }
        #endregion

        #region AddEventCentreAvailability Tests
        [Test]
        public async Task AddEventCentreAvailability_WithValidData_AddsAvailability()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new ApplicationUser { Id = userId, UserName = "organizer@test.com" };
            var eventCentreId = Guid.NewGuid();
            var eventCentre = new EventCentre
            {
                Id = eventCentreId,
                Name = "Test Centre",
                Location = "Test Location",
                Capacity = 100,
                IsActive = true
            };
            await _dbContext.EventCentres.AddAsync(eventCentre);
            await _dbContext.SaveChangesAsync();

            _userManagerMock
                .Setup(um => um.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            var availabilityDto = new AddEventCentreAvailabilityDto
            {
                EventCentreId = eventCentreId,
                Day = DayOfWeek.Monday,
                OpenTime = new TimeSpan(9, 0, 0),
                CloseTime = new TimeSpan(17, 0, 0)
            };

            var mappedDto = new EventCentreAvailabilityDto
            {
                Id = Guid.NewGuid(),
                Day = "Monday",
                OpenTime = availabilityDto.OpenTime,
                CloseTime = availabilityDto.CloseTime
            };

            _mapperMock
                .Setup(m => m.Map<EventCentreAvailabilityDto>(It.IsAny<EventCentreAvailability>()))
                .Returns(mappedDto);

            // Act
            var response = await _eventCentreService.AddEventCentreAvailability(userId, availabilityDto);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(response.StatusMessage, Is.EqualTo("Request successful"));
            Assert.That(await _dbContext.Availabilities.CountAsync(), Is.EqualTo(1));
            Assert.That(await _dbContext.AuditLogs.CountAsync(), Is.EqualTo(1));
        }

        [Test]
        public async Task AddEventCentreAvailability_WithInvalidUser_ReturnsBadRequest()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var availabilityDto = new AddEventCentreAvailabilityDto
            {
                EventCentreId = Guid.NewGuid(),
                Day = DayOfWeek.Monday,
                OpenTime = new TimeSpan(9, 0, 0),
                CloseTime = new TimeSpan(17, 0, 0)
            };

            _userManagerMock
                .Setup(um => um.FindByIdAsync(userId.ToString()))
                .ReturnsAsync((ApplicationUser)null!);

            // Act
            var response = await _eventCentreService.AddEventCentreAvailability(userId, availabilityDto);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(response.StatusMessage, Is.EqualTo("User does not exist"));
        }

        [Test]
        public async Task AddEventCentreAvailability_WithInvalidEventCentre_ReturnsNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new ApplicationUser { Id = userId, UserName = "organizer@test.com" };
            var availabilityDto = new AddEventCentreAvailabilityDto
            {
                EventCentreId = Guid.NewGuid(),
                Day = DayOfWeek.Monday,
                OpenTime = new TimeSpan(9, 0, 0),
                CloseTime = new TimeSpan(17, 0, 0)
            };

            _userManagerMock
                .Setup(um => um.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            // Act
            var response = await _eventCentreService.AddEventCentreAvailability(userId, availabilityDto);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(response.StatusMessage, Is.EqualTo("Event centre does not exist"));
        }

        [Test]
        public async Task AddEventCentreAvailability_WithDuplicateAvailability_ReturnsBadRequest()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new ApplicationUser { Id = userId, UserName = "organizer@test.com" };
            var eventCentreId = Guid.NewGuid();
            var eventCentre = new EventCentre
            {
                Id = eventCentreId,
                Name = "Test Centre",
                Location = "Test Location",
                Capacity = 100,
                IsActive = true
            };
            var existingAvailability = new EventCentreAvailability
            {
                EventCentreId = eventCentreId,
                Day = DayOfWeek.Monday,
                OpenTime = new TimeSpan(9, 0, 0),
                CloseTime = new TimeSpan(17, 0, 0)
            };
            await _dbContext.EventCentres.AddAsync(eventCentre);
            await _dbContext.Availabilities.AddAsync(existingAvailability);
            await _dbContext.SaveChangesAsync();

            _userManagerMock
                .Setup(um => um.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            var availabilityDto = new AddEventCentreAvailabilityDto
            {
                EventCentreId = eventCentreId,
                Day = DayOfWeek.Monday,
                OpenTime = new TimeSpan(9, 0, 0),
                CloseTime = new TimeSpan(17, 0, 0)
            };

            // Act
            var response = await _eventCentreService.AddEventCentreAvailability(userId, availabilityDto);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(response.StatusMessage, Is.EqualTo("The specified availability already exists."));
        }

        [Test]
        public async Task AddEventCentreAvailability_WithOverlappingAvailability_ReturnsBadRequest()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new ApplicationUser { Id = userId, UserName = "organizer@test.com" };
            var eventCentreId = Guid.NewGuid();
            var eventCentre = new EventCentre
            {
                Id = eventCentreId,
                Name = "Test Centre",
                Location = "Test Location",
                Capacity = 100,
                IsActive = true
            };
            var existingAvailability = new EventCentreAvailability
            {
                EventCentreId = eventCentreId,
                Day = DayOfWeek.Monday,
                OpenTime = new TimeSpan(9, 0, 0),
                CloseTime = new TimeSpan(17, 0, 0)
            };
            await _dbContext.EventCentres.AddAsync(eventCentre);
            await _dbContext.Availabilities.AddAsync(existingAvailability);
            await _dbContext.SaveChangesAsync();

            _userManagerMock
                .Setup(um => um.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            var availabilityDto = new AddEventCentreAvailabilityDto
            {
                EventCentreId = eventCentreId,
                Day = DayOfWeek.Monday,
                OpenTime = new TimeSpan(10, 0, 0),  // Overlaps with 9:00-17:00
                CloseTime = new TimeSpan(18, 0, 0)
            };

            // Act
            var response = await _eventCentreService.AddEventCentreAvailability(userId, availabilityDto);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(response.StatusMessage, Is.EqualTo("The specified availability overlaps with an existing availability."));
        }
        #endregion

        #region UpdateEventCentreAvailability Tests
        [Test]
        public async Task UpdateEventCentreAvailability_WithValidData_UpdatesAvailability()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new ApplicationUser { Id = userId, UserName = "organizer@test.com" };
            var eventCentreId = Guid.NewGuid();
            var availabilityId = Guid.NewGuid();
            
            var eventCentre = new EventCentre
            {
                Id = eventCentreId,
                Name = "Test Centre",
                Location = "Test Location",
                Capacity = 100,
                IsActive = true
            };
            var availability = new EventCentreAvailability
            {
                Id = availabilityId,
                EventCentreId = eventCentreId,
                Day = DayOfWeek.Monday,
                OpenTime = new TimeSpan(9, 0, 0),
                CloseTime = new TimeSpan(17, 0, 0)
            };
            await _dbContext.EventCentres.AddAsync(eventCentre);
            await _dbContext.Availabilities.AddAsync(availability);
            await _dbContext.SaveChangesAsync();

            _userManagerMock
                .Setup(um => um.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            var updateDto = new UpdateEventCentreAvailabilityDto
            {
                Day = DayOfWeek.Tuesday,
                OpenTime = new TimeSpan(10, 0, 0),
                CloseTime = new TimeSpan(18, 0, 0)
            };

            // Act
            var response = await _eventCentreService.UpdateEventCentreAvailability(userId, availabilityId, updateDto);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
            Assert.That(response.StatusMessage, Is.EqualTo("Request Successful"));
            
            var updatedAvailability = await _dbContext.Availabilities.FindAsync(availabilityId);
            Assert.That(updatedAvailability!.Day, Is.EqualTo(DayOfWeek.Tuesday));
            Assert.That(updatedAvailability.OpenTime, Is.EqualTo(new TimeSpan(10, 0, 0)));
            Assert.That(updatedAvailability.CloseTime, Is.EqualTo(new TimeSpan(18, 0, 0)));
            Assert.That(await _dbContext.AuditLogs.CountAsync(), Is.EqualTo(1));
        }

        [Test]
        public async Task UpdateEventCentreAvailability_WithInvalidUser_ReturnsBadRequest()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var availabilityId = Guid.NewGuid();
            var updateDto = new UpdateEventCentreAvailabilityDto
            {
                Day = DayOfWeek.Tuesday,
                OpenTime = new TimeSpan(10, 0, 0),
                CloseTime = new TimeSpan(18, 0, 0)
            };

            _userManagerMock
                .Setup(um => um.FindByIdAsync(userId.ToString()))
                .ReturnsAsync((ApplicationUser)null!);

            // Act
            var response = await _eventCentreService.UpdateEventCentreAvailability(userId, availabilityId, updateDto);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(response.StatusMessage, Is.EqualTo("User does not exist"));
        }

        [Test]
        public async Task UpdateEventCentreAvailability_WithInvalidAvailabilityId_ReturnsNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new ApplicationUser { Id = userId, UserName = "organizer@test.com" };
            var availabilityId = Guid.NewGuid();
            var updateDto = new UpdateEventCentreAvailabilityDto
            {
                Day = DayOfWeek.Tuesday,
                OpenTime = new TimeSpan(10, 0, 0),
                CloseTime = new TimeSpan(18, 0, 0)
            };

            _userManagerMock
                .Setup(um => um.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            // Act
            var response = await _eventCentreService.UpdateEventCentreAvailability(userId, availabilityId, updateDto);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(response.StatusMessage, Is.EqualTo("Availability does not exist"));
        }
        #endregion

        #region DeleteEventCentreAvailability Tests
        [Test]
        public async Task DeleteEventCentreAvailability_WithValidId_DeletesAvailability()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new ApplicationUser { Id = userId, UserName = "organizer@test.com" };
            var eventCentreId = Guid.NewGuid();
            var availabilityId = Guid.NewGuid();
            
            var eventCentre = new EventCentre
            {
                Id = eventCentreId,
                Name = "Test Centre",
                Location = "Test Location",
                Capacity = 100,
                IsActive = true
            };
            var availability = new EventCentreAvailability
            {
                Id = availabilityId,
                EventCentreId = eventCentreId,
                Day = DayOfWeek.Monday,
                OpenTime = new TimeSpan(9, 0, 0),
                CloseTime = new TimeSpan(17, 0, 0)
            };
            await _dbContext.EventCentres.AddAsync(eventCentre);
            await _dbContext.Availabilities.AddAsync(availability);
            await _dbContext.SaveChangesAsync();

            _userManagerMock
                .Setup(um => um.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            // Act
            var response = await _eventCentreService.DeleteEventCentreAvailability(userId, availabilityId);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
            Assert.That(response.StatusMessage, Is.EqualTo("Request Successful"));
            Assert.That(await _dbContext.Availabilities.CountAsync(), Is.EqualTo(0));
            Assert.That(await _dbContext.AuditLogs.CountAsync(), Is.EqualTo(1));
        }

        [Test]
        public async Task DeleteEventCentreAvailability_WithInvalidUser_ReturnsBadRequest()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var availabilityId = Guid.NewGuid();

            _userManagerMock
                .Setup(um => um.FindByIdAsync(userId.ToString()))
                .ReturnsAsync((ApplicationUser)null!);

            // Act
            var response = await _eventCentreService.DeleteEventCentreAvailability(userId, availabilityId);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(response.StatusMessage, Is.EqualTo("User does not exist"));
        }

        [Test]
        public async Task DeleteEventCentreAvailability_WithInvalidAvailabilityId_ReturnsNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new ApplicationUser { Id = userId, UserName = "organizer@test.com" };
            var availabilityId = Guid.NewGuid();

            _userManagerMock
                .Setup(um => um.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            // Act
            var response = await _eventCentreService.DeleteEventCentreAvailability(userId, availabilityId);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(response.StatusMessage, Is.EqualTo("Availability does not exist"));
        }
        #endregion
    }
}
