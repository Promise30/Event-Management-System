using Event_Management_System.API.Application.Implementation;
using Event_Management_System.API.Application.Interfaces;
using Event_Management_System.API.Domain.DTOs;
using Event_Management_System.API.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;

namespace Event_Management_System.Tests.NotificationTests
{
    [TestFixture]
    public class NotificationServiceTests
    {
        private NotificationService _notificationService;
        private Mock<ILogger<NotificationService>> _loggerMock;
        private Mock<INotificationChannel> _emailChannelMock;
        private Mock<INotificationChannel> _smsChannelMock;
        private List<INotificationChannel> _channels;

        [SetUp]
        public void Setup()
        {
            _loggerMock = new Mock<ILogger<NotificationService>>();

            _emailChannelMock = new Mock<INotificationChannel>();
            _emailChannelMock.Setup(c => c.ChannelType).Returns(NotificationChannel.Email);

            _smsChannelMock = new Mock<INotificationChannel>();
            _smsChannelMock.Setup(c => c.ChannelType).Returns(NotificationChannel.SMS);

            _channels = [_emailChannelMock.Object, _smsChannelMock.Object];

            _notificationService = new NotificationService(_loggerMock.Object, _channels);
        }

        #region SendAsync Tests

        [Test]
        public async Task SendAsync_WithEmailChannel_SendsOnlyToEmailChannel()
        {
            // Arrange
            var request = new NotificationRequest
            {
                RecipientEmail = "test@example.com",
                RecipientName = "Test User",
                Type = NotificationType.BookingConfirmed,
                Channels = [NotificationChannel.Email],
                Data = new Dictionary<string, string>
                {
                    { "EventCentreName", "Test Centre" },
                    { "BookingDate", "2025-01-15" },
                    { "BookingReference", "BK-001" }
                }
            };

            // Act
            await _notificationService.SendAsync(request);

            // Assert
            _emailChannelMock.Verify(c => c.SendAsync(request, It.IsAny<CancellationToken>()), Times.Once);
            _smsChannelMock.Verify(c => c.SendAsync(It.IsAny<NotificationRequest>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task SendAsync_WithSmsChannel_SendsOnlyToSmsChannel()
        {
            // Arrange
            var request = new NotificationRequest
            {
                RecipientEmail = "test@example.com",
                RecipientName = "Test User",
                RecipientPhone = "+1234567890",
                Type = NotificationType.EventReminder,
                Channels = [NotificationChannel.SMS],
                Data = new Dictionary<string, string>
                {
                    { "EventName", "Test Event" },
                    { "EventDate", "2025-02-01" }
                }
            };

            // Act
            await _notificationService.SendAsync(request);

            // Assert
            _smsChannelMock.Verify(c => c.SendAsync(request, It.IsAny<CancellationToken>()), Times.Once);
            _emailChannelMock.Verify(c => c.SendAsync(It.IsAny<NotificationRequest>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task SendAsync_WithMultipleChannels_SendsToAllSpecifiedChannels()
        {
            // Arrange
            var request = new NotificationRequest
            {
                RecipientEmail = "test@example.com",
                RecipientName = "Test User",
                RecipientPhone = "+1234567890",
                Type = NotificationType.TicketPurchaseConfirmed,
                Channels = [NotificationChannel.Email, NotificationChannel.SMS],
                Data = new Dictionary<string, string>
                {
                    { "EventName", "Test Event" },
                    { "TicketType", "VIP" },
                    { "Quantity", "2" },
                    { "TotalAmount", "$100.00" }
                }
            };

            // Act
            await _notificationService.SendAsync(request);

            // Assert
            _emailChannelMock.Verify(c => c.SendAsync(request, It.IsAny<CancellationToken>()), Times.Once);
            _smsChannelMock.Verify(c => c.SendAsync(request, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task SendAsync_WithNoChannelsSpecified_SendsToAllRegisteredChannels()
        {
            // Arrange
            var request = new NotificationRequest
            {
                RecipientEmail = "test@example.com",
                RecipientName = "Test User",
                Type = NotificationType.PaymentSuccessful,
                Channels = null!,
                Data = new Dictionary<string, string>
                {
                    { "Amount", "$50.00" },
                    { "Reference", "PAY-001" }
                }
            };

            // Act
            await _notificationService.SendAsync(request);

            // Assert
            _emailChannelMock.Verify(c => c.SendAsync(request, It.IsAny<CancellationToken>()), Times.Once);
            _smsChannelMock.Verify(c => c.SendAsync(request, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task SendAsync_WithEmptyChannelsArray_SendsToAllRegisteredChannels()
        {
            // Arrange
            var request = new NotificationRequest
            {
                RecipientEmail = "test@example.com",
                RecipientName = "Test User",
                Type = NotificationType.AccountVerification,
                Channels = [],
                Data = new Dictionary<string, string>
                {
                    { "VerificationLink", "https://example.com/verify" }
                }
            };

            // Act
            await _notificationService.SendAsync(request);

            // Assert
            _emailChannelMock.Verify(c => c.SendAsync(request, It.IsAny<CancellationToken>()), Times.Once);
            _smsChannelMock.Verify(c => c.SendAsync(request, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task SendAsync_WhenChannelThrowsException_ContinuesToNextChannel()
        {
            // Arrange
            _emailChannelMock
                .Setup(c => c.SendAsync(It.IsAny<NotificationRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("SMTP server unavailable"));

            var request = new NotificationRequest
            {
                RecipientEmail = "test@example.com",
                RecipientName = "Test User",
                Type = NotificationType.BookingConfirmed,
                Channels = [NotificationChannel.Email, NotificationChannel.SMS],
                Data = new Dictionary<string, string>()
            };

            // Act
            await _notificationService.SendAsync(request);

            // Assert - SMS should still be called even though Email failed
            _emailChannelMock.Verify(c => c.SendAsync(request, It.IsAny<CancellationToken>()), Times.Once);
            _smsChannelMock.Verify(c => c.SendAsync(request, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task SendAsync_WhenChannelThrowsException_LogsError()
        {
            // Arrange
            var exception = new InvalidOperationException("SMTP server unavailable");
            _emailChannelMock
                .Setup(c => c.SendAsync(It.IsAny<NotificationRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(exception);

            var request = new NotificationRequest
            {
                RecipientEmail = "test@example.com",
                RecipientName = "Test User",
                Type = NotificationType.BookingCancelled,
                Channels = [NotificationChannel.Email],
                Data = new Dictionary<string, string>()
            };

            // Act
            await _notificationService.SendAsync(request);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    exception,
                    It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
                Times.Once);
        }

        [Test]
        public async Task SendAsync_WhenAllChannelsFail_DoesNotThrow()
        {
            // Arrange
            _emailChannelMock
                .Setup(c => c.SendAsync(It.IsAny<NotificationRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Email failed"));

            _smsChannelMock
                .Setup(c => c.SendAsync(It.IsAny<NotificationRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("SMS failed"));

            var request = new NotificationRequest
            {
                RecipientEmail = "test@example.com",
                RecipientName = "Test User",
                Type = NotificationType.PaymentFailed,
                Channels = [NotificationChannel.Email, NotificationChannel.SMS],
                Data = new Dictionary<string, string>()
            };

            // Act & Assert - should not throw
            Assert.DoesNotThrowAsync(async () => await _notificationService.SendAsync(request));
        }

        [Test]
        public async Task SendAsync_WithNoRegisteredChannels_CompletesWithoutError()
        {
            // Arrange
            var emptyService = new NotificationService(
                _loggerMock.Object,
                Enumerable.Empty<INotificationChannel>());

            var request = new NotificationRequest
            {
                RecipientEmail = "test@example.com",
                RecipientName = "Test User",
                Type = NotificationType.EventCancelled,
                Channels = [NotificationChannel.Email],
                Data = new Dictionary<string, string>()
            };

            // Act & Assert
            Assert.DoesNotThrowAsync(async () => await emptyService.SendAsync(request));
        }

        [Test]
        public async Task SendAsync_WithDifferentNotificationTypes_SendsSuccessfully()
        {
            // Arrange & Act & Assert for each notification type
            var notificationTypes = new[]
            {
                NotificationType.BookingConfirmed,
                NotificationType.BookingCancelled,
                NotificationType.TicketPurchaseConfirmed,
                NotificationType.EventReminder,
                NotificationType.PaymentSuccessful,
                NotificationType.PasswordReset,
                NotificationType.AccountVerification,
                NotificationType.OrganizerRequestApproved,
                NotificationType.RefundProcessed
            };

            foreach (var type in notificationTypes)
            {
                var request = new NotificationRequest
                {
                    RecipientEmail = "test@example.com",
                    RecipientName = "Test User",
                    Type = type,
                    Channels = [NotificationChannel.Email],
                    Data = new Dictionary<string, string>()
                };

                await _notificationService.SendAsync(request);
            }

            // Assert - email channel should have been called once per notification type
            _emailChannelMock.Verify(
                c => c.SendAsync(It.IsAny<NotificationRequest>(), It.IsAny<CancellationToken>()),
                Times.Exactly(notificationTypes.Length));
        }

        #endregion
    }
}
