using Event_Management_System.API.Domain.Enums;
using Event_Management_System.API.Helpers;

namespace Event_Management_System.Tests.NotificationTests
{
    [TestFixture]
    public class EmailTemplateBuilderTests
    {
        #region BookingConfirmed Tests

        [Test]
        public void Build_BookingConfirmed_ReturnsCorrectSubject()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "EventCentreName", "Grand Hall" },
                { "BookingDate", "2025-03-15" },
                { "BookingReference", "BK-12345" }
            };

            // Act
            var (subject, body) = EmailTemplateBuilder.Build(NotificationType.BookingConfirmed, "John", data);

            // Assert
            Assert.That(subject, Is.EqualTo("Booking Confirmed - Eventify"));
            Assert.That(body, Does.Contain("John"));
            Assert.That(body, Does.Contain("Grand Hall"));
            Assert.That(body, Does.Contain("2025-03-15"));
            Assert.That(body, Does.Contain("BK-12345"));
        }

        #endregion

        #region BookingCancelled Tests

        [Test]
        public void Build_BookingCancelled_ReturnsCorrectContent()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "EventCentreName", "Conference Room A" },
                { "Reason", "Venue unavailable" }
            };

            // Act
            var (subject, body) = EmailTemplateBuilder.Build(NotificationType.BookingCancelled, "Alice", data);

            // Assert
            Assert.That(subject, Is.EqualTo("Booking Cancelled - Eventify"));
            Assert.That(body, Does.Contain("Alice"));
            Assert.That(body, Does.Contain("Conference Room A"));
            Assert.That(body, Does.Contain("Venue unavailable"));
        }

        #endregion

        #region TicketPurchaseConfirmed Tests

        [Test]
        public void Build_TicketPurchaseConfirmed_ReturnsCorrectContent()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "EventName", "Music Festival" },
                { "TicketType", "VIP" },
                { "Quantity", "2" },
                { "TotalAmount", "$200.00" }
            };

            // Act
            var (subject, body) = EmailTemplateBuilder.Build(NotificationType.TicketPurchaseConfirmed, "Bob", data);

            // Assert
            Assert.That(subject, Is.EqualTo("Ticket Purchase Confirmed - Eventify"));
            Assert.That(body, Does.Contain("Bob"));
            Assert.That(body, Does.Contain("Music Festival"));
            Assert.That(body, Does.Contain("VIP"));
            Assert.That(body, Does.Contain("2"));
            Assert.That(body, Does.Contain("$200.00"));
        }

        #endregion

        #region EventReminder Tests

        [Test]
        public void Build_EventReminder_ReturnsCorrectContent()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "EventName", "Tech Conference" },
                { "EventDate", "2025-06-01" },
                { "Venue", "Convention Center" }
            };

            // Act
            var (subject, body) = EmailTemplateBuilder.Build(NotificationType.EventReminder, "Carol", data);

            // Assert
            Assert.That(subject, Is.EqualTo("Event Reminder - Eventify"));
            Assert.That(body, Does.Contain("Carol"));
            Assert.That(body, Does.Contain("Tech Conference"));
            Assert.That(body, Does.Contain("2025-06-01"));
            Assert.That(body, Does.Contain("Convention Center"));
        }

        #endregion

        #region PasswordReset Tests

        [Test]
        public void Build_PasswordReset_ReturnsCorrectContent()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "ResetLink", "https://eventify.com/reset?token=abc123" }
            };

            // Act
            var (subject, body) = EmailTemplateBuilder.Build(NotificationType.PasswordReset, "Dave", data);

            // Assert
            Assert.That(subject, Is.EqualTo("Password Reset - Eventify"));
            Assert.That(body, Does.Contain("Dave"));
            Assert.That(body, Does.Contain("https://eventify.com/reset?token=abc123"));
            Assert.That(body, Does.Contain("Reset Password"));
        }

        #endregion

        #region AccountVerification Tests

        [Test]
        public void Build_AccountVerification_ReturnsCorrectContent()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "VerificationLink", "https://eventify.com/verify?token=xyz789" }
            };

            // Act
            var (subject, body) = EmailTemplateBuilder.Build(NotificationType.AccountVerification, "Eve", data);

            // Assert
            Assert.That(subject, Is.EqualTo("Verify Your Account - Eventify"));
            Assert.That(body, Does.Contain("Eve"));
            Assert.That(body, Does.Contain("https://eventify.com/verify?token=xyz789"));
            Assert.That(body, Does.Contain("Verify Email"));
        }

        #endregion

        #region PaymentSuccessful Tests

        [Test]
        public void Build_PaymentSuccessful_ReturnsCorrectContent()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "Amount", "$50.00" },
                { "Reference", "PAY-001" }
            };

            // Act
            var (subject, body) = EmailTemplateBuilder.Build(NotificationType.PaymentSuccessful, "Frank", data);

            // Assert
            Assert.That(subject, Is.EqualTo("Payment Successful - Eventify"));
            Assert.That(body, Does.Contain("Frank"));
            Assert.That(body, Does.Contain("$50.00"));
            Assert.That(body, Does.Contain("PAY-001"));
        }

        #endregion

        #region OrganizerRequestApproved Tests

        [Test]
        public void Build_OrganizerRequestApproved_ReturnsCorrectContent()
        {
            // Arrange
            var data = new Dictionary<string, string>();

            // Act
            var (subject, body) = EmailTemplateBuilder.Build(NotificationType.OrganizerRequestApproved, "Grace", data);

            // Assert
            Assert.That(subject, Is.EqualTo("Organizer Request Approved - Eventify"));
            Assert.That(body, Does.Contain("Grace"));
            Assert.That(body, Does.Contain("approved"));
        }

        #endregion

        #region Default/Missing Data Tests

        [Test]
        public void Build_WithEmptyData_UsesDefaultValues()
        {
            // Arrange
            var data = new Dictionary<string, string>();

            // Act
            var (subject, body) = EmailTemplateBuilder.Build(NotificationType.BookingConfirmed, "Test", data);

            // Assert
            Assert.That(subject, Is.Not.Empty);
            Assert.That(body, Does.Contain("Test"));
            Assert.That(body, Does.Contain("your event centre")); // default value
        }

        [Test]
        public void Build_AllTypes_ContainHtmlLayout()
        {
            // Arrange
            var allTypes = Enum.GetValues<NotificationType>();
            var data = new Dictionary<string, string>();

            foreach (var type in allTypes)
            {
                // Act
                var (subject, body) = EmailTemplateBuilder.Build(type, "User", data);

                // Assert
                Assert.That(subject, Is.Not.Null.And.Not.Empty, $"Subject should not be empty for {type}");
                Assert.That(body, Does.Contain("<!DOCTYPE html>"), $"Body should contain HTML for {type}");
                Assert.That(body, Does.Contain("Eventify"), $"Body should contain branding for {type}");
                Assert.That(body, Does.Contain("User"), $"Body should contain recipient name for {type}");
            }
        }

        [Test]
        public void Build_RefundProcessed_ReturnsCorrectContent()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "RefundAmount", "$25.00" },
                { "Reference", "REF-001" }
            };

            // Act
            var (subject, body) = EmailTemplateBuilder.Build(NotificationType.RefundProcessed, "Hank", data);

            // Assert
            Assert.That(subject, Is.EqualTo("Refund Processed - Eventify"));
            Assert.That(body, Does.Contain("Hank"));
            Assert.That(body, Does.Contain("$25.00"));
            Assert.That(body, Does.Contain("REF-001"));
        }

        [Test]
        public void Build_EventCancelled_ReturnsCorrectContent()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "EventName", "Annual Gala" },
                { "Reason", "Severe weather warning" }
            };

            // Act
            var (subject, body) = EmailTemplateBuilder.Build(NotificationType.EventCancelled, "Ivy", data);

            // Assert
            Assert.That(subject, Is.EqualTo("Event Cancelled - Eventify"));
            Assert.That(body, Does.Contain("Ivy"));
            Assert.That(body, Does.Contain("Annual Gala"));
            Assert.That(body, Does.Contain("Severe weather warning"));
        }

        #endregion
    }
}
