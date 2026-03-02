using AutoMapper;
using Event_Management_System.API.Application.Implementation;
using Event_Management_System.API.Application.Interfaces;
using Event_Management_System.API.Application.Payments;
using Event_Management_System.API.Domain.Entities;
using Event_Management_System.API.Infrastructures;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Event_Management_System.Tests.BookingTests
{
    [TestFixture]
    public class BookingServiceTests
    {
        private BookingService _bookingService;
        private Mock<ILogger<BookingService>> _loggerMock;
        private ApplicationDbContext _dbContext;
        private Mock<UserManager<ApplicationUser>> _userManagerMock;
        private Mock<IMapper> _mapperMock;
        private Mock<IPaymentService> _paymentServiceMock;
        private Mock<INotificationService> _notificationServiceMock;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "EventManagementTestDb")
                .Options;
            _dbContext = new ApplicationDbContext(options);
            _loggerMock = new Mock<ILogger<BookingService>>();
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(userStoreMock.Object, null, null, null, null, null, null, null, null);
            _mapperMock = new Mock<IMapper>();
            _paymentServiceMock = new Mock<IPaymentService>();
            _notificationServiceMock = new Mock<INotificationService>();
            _bookingService = new BookingService(_dbContext, _loggerMock.Object, _mapperMock.Object, _userManagerMock.Object, _paymentServiceMock.Object, _notificationServiceMock.Object);
        }
        [TearDown]
        public void TearDown()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }

    }
}
