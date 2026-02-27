using Event_Management_System.API.Domain.Enums;

namespace Event_Management_System.API.Helpers
{
    public static class EmailTemplateBuilder
    {
        public static (string Subject, string Body) Build(NotificationType type, string recipientName, Dictionary<string, string> data)
        {
            return type switch
            {
                // Bookings
                NotificationType.BookingSubmitted => BuildBookingSubmitted(recipientName, data),
                NotificationType.BookingConfirmed => BuildBookingConfirmed(recipientName, data),
                NotificationType.BookingCancelled => BuildBookingCancelled(recipientName, data),
                NotificationType.BookingPaymentFailed => BuildBookingPaymentFailed(recipientName, data),
                NotificationType.BookingRejected => BuildBookingRejected(recipientName, data),

                // Tickets
                NotificationType.TicketPurchaseConfirmed => BuildTicketPurchaseConfirmed(recipientName, data),
                NotificationType.TicketCancelledRefunded => BuildTicketCancelledRefunded(recipientName, data),
                NotificationType.TicketPurchaseCancelled => BuildTicketPurchaseCancelled(recipientName, data),

                // Events
                NotificationType.EventDetailsUpdated => BuildEventDetailsUpdated(recipientName, data),
                NotificationType.EventCancelled => BuildEventCancelled(recipientName, data),
                NotificationType.EventReminder => BuildEventReminder(recipientName, data),

                // Organizer
                NotificationType.OrganizerRequestApproved => BuildOrganizerRequestApproved(recipientName, data),
                NotificationType.OrganizerRequestRejected => BuildOrganizerRequestRejected(recipientName, data),

                // Payments
                NotificationType.PaymentSuccessful => BuildPaymentSuccessful(recipientName, data),
                NotificationType.PaymentFailed => BuildPaymentFailed(recipientName, data),
                NotificationType.RefundProcessed => BuildRefundProcessed(recipientName, data),

                // Authentication
                NotificationType.PasswordReset => BuildPasswordReset(recipientName, data),
                NotificationType.EmailVerification => BuildEmailVerification(recipientName, data),
                NotificationType.PasswordChanged => BuildPasswordChanged(recipientName, data),
                NotificationType.AccountVerification => BuildAccountVerification(recipientName, data),
                NotificationType.TwoFactorEnabled => BuildTwoFactorEnabled(recipientName, data),
                NotificationType.TwoFactorDisabled => BuildTwoFactorDisabled(recipientName, data),
                NotificationType.TwoFactorCode => BuildTwoFactorCode(recipientName, data),

                _ => ($"Notification from Eventify", WrapInLayout(recipientName, $"<p>You have a new notification.</p>"))
            };
        }

        // --- Bookings ---

        private static (string, string) BuildBookingSubmitted(string name, Dictionary<string, string> data)
        {
            var eventCentre = data.GetValueOrDefault("EventCentreName", "your event centre");
            var bookingDate = data.GetValueOrDefault("BookingDate", "N/A");
            var bookingReference = data.GetValueOrDefault("BookingReference", "N/A");

            return ("Booking Submitted - Eventify", WrapInLayout(name,
                $"""
                <p>Your booking has been successfully submitted and is pending admin approval.</p>
                <table style="width:100%;border-collapse:collapse;margin:16px 0;">
                    <tr><td style="padding:8px;font-weight:bold;">Event Centre:</td><td style="padding:8px;">{eventCentre}</td></tr>
                    <tr><td style="padding:8px;font-weight:bold;">Booking Date:</td><td style="padding:8px;">{bookingDate}</td></tr>
                    <tr><td style="padding:8px;font-weight:bold;">Reference:</td><td style="padding:8px;">{bookingReference}</td></tr>
                </table>
                <p>You will receive a confirmation email once your booking has been reviewed and approved by our team.</p>
                <p>Thank you for choosing Eventify.</p>
                """));
        }

        private static (string, string) BuildBookingConfirmed(string name, Dictionary<string, string> data)
        {
            var eventCentre = data.GetValueOrDefault("EventCentreName", "your event centre");
            var bookingDate = data.GetValueOrDefault("BookingDate", "N/A");
            var bookingReference = data.GetValueOrDefault("BookingReference", "N/A");

            return ("Booking Confirmed - Eventify", WrapInLayout(name,
                $"""
                <p>Your booking has been confirmed!</p>
                <table style="width:100%;border-collapse:collapse;margin:16px 0;">
                    <tr><td style="padding:8px;font-weight:bold;">Event Centre:</td><td style="padding:8px;">{eventCentre}</td></tr>
                    <tr><td style="padding:8px;font-weight:bold;">Booking Date:</td><td style="padding:8px;">{bookingDate}</td></tr>
                    <tr><td style="padding:8px;font-weight:bold;">Reference:</td><td style="padding:8px;">{bookingReference}</td></tr>
                </table>
                <p>Thank you for choosing Eventify.</p>
                """));
        }

        private static (string, string) BuildBookingCancelled(string name, Dictionary<string, string> data)
        {
            var eventCentre = data.GetValueOrDefault("EventCentreName", "your event centre");
            var reason = data.GetValueOrDefault("Reason", "No reason specified");

            return ("Booking Cancelled - Eventify", WrapInLayout(name,
                $"""
                <p>Your booking for <strong>{eventCentre}</strong> has been cancelled.</p>
                <p><strong>Reason:</strong> {reason}</p>
                <p>If you believe this is an error, please contact support.</p>
                """));
        }

        private static (string, string) BuildBookingPaymentFailed(string name, Dictionary<string, string> data)
        {
            var eventCentre = data.GetValueOrDefault("EventCentreName", "your event centre");
            var bookingReference = data.GetValueOrDefault("BookingReference", "N/A");

            return ("Booking Payment Failed - Eventify", WrapInLayout(name,
                $"""
                <p>Payment for your booking at <strong>{eventCentre}</strong> (Ref: {bookingReference}) has failed.</p>
                <p>Please try again or contact support for assistance.</p>
                """));
        }

        private static (string, string) BuildBookingRejected(string name, Dictionary<string, string> data)
        {
            var eventCentre = data.GetValueOrDefault("EventCentreName", "your event centre");
            var reason = data.GetValueOrDefault("Reason", "No reason specified");

            return ("Booking Rejected - Eventify", WrapInLayout(name,
                $"""
                <p>Your booking request for <strong>{eventCentre}</strong> has been rejected.</p>
                <p><strong>Reason:</strong> {reason}</p>
                <p>You may submit a new booking request or contact support.</p>
                """));
        }

        // --- Tickets ---

        private static (string, string) BuildTicketPurchaseConfirmed(string name, Dictionary<string, string> data)
        {
            var eventName = data.GetValueOrDefault("EventName", "the event");
            var ticketType = data.GetValueOrDefault("TicketType", "N/A");
            var quantity = data.GetValueOrDefault("Quantity", "1");
            var totalAmount = data.GetValueOrDefault("TotalAmount", "N/A");

            return ("Ticket Purchase Confirmed - Eventify", WrapInLayout(name,
                $"""
                <p>Your ticket purchase has been confirmed!</p>
                <table style="width:100%;border-collapse:collapse;margin:16px 0;">
                    <tr><td style="padding:8px;font-weight:bold;">Event:</td><td style="padding:8px;">{eventName}</td></tr>
                    <tr><td style="padding:8px;font-weight:bold;">Ticket Type:</td><td style="padding:8px;">{ticketType}</td></tr>
                    <tr><td style="padding:8px;font-weight:bold;">Quantity:</td><td style="padding:8px;">{quantity}</td></tr>
                    <tr><td style="padding:8px;font-weight:bold;">Total:</td><td style="padding:8px;">{totalAmount}</td></tr>
                </table>
                """));
        }

        private static (string, string) BuildTicketCancelledRefunded(string name, Dictionary<string, string> data)
        {
            var eventName = data.GetValueOrDefault("EventName", "the event");
            var refundAmount = data.GetValueOrDefault("RefundAmount", "N/A");

            return ("Ticket Cancelled & Refunded - Eventify", WrapInLayout(name,
                $"""
                <p>Your ticket for <strong>{eventName}</strong> has been cancelled and a refund of <strong>{refundAmount}</strong> has been processed.</p>
                <p>The refund may take 5-10 business days to appear in your account.</p>
                """));
        }

        private static (string, string) BuildTicketPurchaseCancelled(string name, Dictionary<string, string> data)
        {
            var eventName = data.GetValueOrDefault("EventName", "the event");

            return ("Ticket Purchase Cancelled - Eventify", WrapInLayout(name,
                $"""
                <p>Your ticket purchase for <strong>{eventName}</strong> has been cancelled.</p>
                <p>If you did not request this cancellation, please contact support immediately.</p>
                """));
        }

        // --- Events ---

        private static (string, string) BuildEventDetailsUpdated(string name, Dictionary<string, string> data)
        {
            var eventName = data.GetValueOrDefault("EventName", "the event");
            var changes = data.GetValueOrDefault("Changes", "Some event details have been updated.");

            return ("Event Details Updated - Eventify", WrapInLayout(name,
                $"""
                <p>The details for <strong>{eventName}</strong> have been updated.</p>
                <p>{changes}</p>
                <p>Please review the updated event information.</p>
                """));
        }

        private static (string, string) BuildEventCancelled(string name, Dictionary<string, string> data)
        {
            var eventName = data.GetValueOrDefault("EventName", "the event");
            var reason = data.GetValueOrDefault("Reason", "No reason specified");

            return ("Event Cancelled - Eventify", WrapInLayout(name,
                $"""
                <p>We regret to inform you that <strong>{eventName}</strong> has been cancelled.</p>
                <p><strong>Reason:</strong> {reason}</p>
                <p>If you purchased tickets, a refund will be processed automatically.</p>
                """));
        }

        private static (string, string) BuildEventReminder(string name, Dictionary<string, string> data)
        {
            var eventName = data.GetValueOrDefault("EventName", "your event");
            var eventDate = data.GetValueOrDefault("EventDate", "soon");
            var venue = data.GetValueOrDefault("Venue", "N/A");

            return ("Event Reminder - Eventify", WrapInLayout(name,
                $"""
                <p>This is a reminder that <strong>{eventName}</strong> is coming up on <strong>{eventDate}</strong>.</p>
                <p><strong>Venue:</strong> {venue}</p>
                <p>We look forward to seeing you there!</p>
                """));
        }

        // --- Organizer ---

        private static (string, string) BuildOrganizerRequestApproved(string name, Dictionary<string, string> data)
        {
            return ("Organizer Request Approved - Eventify", WrapInLayout(name,
                """
                <p>Congratulations! Your request to become an organizer has been <strong>approved</strong>.</p>
                <p>You can now create and manage events on the Eventify platform.</p>
                """));
        }

        private static (string, string) BuildOrganizerRequestRejected(string name, Dictionary<string, string> data)
        {
            var reason = data.GetValueOrDefault("Reason", "No reason specified");

            return ("Organizer Request Rejected - Eventify", WrapInLayout(name,
                $"""
                <p>Unfortunately, your request to become an organizer has been <strong>rejected</strong>.</p>
                <p><strong>Reason:</strong> {reason}</p>
                <p>You may resubmit your application after addressing the concerns.</p>
                """));
        }

        // --- Payments ---

        private static (string, string) BuildPaymentSuccessful(string name, Dictionary<string, string> data)
        {
            var amount = data.GetValueOrDefault("Amount", "N/A");
            var reference = data.GetValueOrDefault("Reference", "N/A");

            return ("Payment Successful - Eventify", WrapInLayout(name,
                $"""
                <p>Your payment of <strong>{amount}</strong> has been processed successfully.</p>
                <p><strong>Reference:</strong> {reference}</p>
                <p>Thank you for your payment.</p>
                """));
        }

        private static (string, string) BuildPaymentFailed(string name, Dictionary<string, string> data)
        {
            var amount = data.GetValueOrDefault("Amount", "N/A");
            var reason = data.GetValueOrDefault("Reason", "Unknown error");

            return ("Payment Failed - Eventify", WrapInLayout(name,
                $"""
                <p>Your payment of <strong>{amount}</strong> could not be processed.</p>
                <p><strong>Reason:</strong> {reason}</p>
                <p>Please try again or use a different payment method.</p>
                """));
        }

        private static (string, string) BuildRefundProcessed(string name, Dictionary<string, string> data)
        {
            var amount = data.GetValueOrDefault("RefundAmount", "N/A");
            var reference = data.GetValueOrDefault("Reference", "N/A");

            return ("Refund Processed - Eventify", WrapInLayout(name,
                $"""
                <p>A refund of <strong>{amount}</strong> has been processed for reference <strong>{reference}</strong>.</p>
                <p>The refund may take 5-10 business days to appear in your account.</p>
                """));
        }

        // --- Authentication ---

        private static (string, string) BuildPasswordReset(string name, Dictionary<string, string> data)
        {
            var resetLink = data.GetValueOrDefault("ResetLink", "#");

            return ("Password Reset - Eventify", WrapInLayout(name,
                $"""
                <p>You requested a password reset. Click the button below to reset your password:</p>
                <p style="text-align:center;margin:24px 0;">
                    <a href="{resetLink}" style="background-color:#4CAF50;color:white;padding:12px 24px;text-decoration:none;border-radius:4px;font-weight:bold;">Reset Password</a>
                </p>
                <p>If you did not request this, please ignore this email.</p>
                <p style="font-size:12px;color:#888;">This link will expire in 24 hours.</p>
                """));
        }

        private static (string, string) BuildEmailVerification(string name, Dictionary<string, string> data)
        {
            var verificationLink = data.GetValueOrDefault("VerificationLink", "#");

            return ("Verify Your Account - Eventify", WrapInLayout(name,
                $"""
                <p>Welcome to Eventify! Please verify your email address by clicking the button below:</p>
                <p style="text-align:center;margin:24px 0;">
                    <a href="{verificationLink}" style="background-color:#4CAF50;color:white;padding:12px 24px;text-decoration:none;border-radius:4px;font-weight:bold;">Verify Email</a>
                </p>
                <p>If you did not create an account, please ignore this email.</p>
                """));
        }

        // Account Verification - success message that their account has been verified
        private static (string, string) BuildAccountVerification(string name, Dictionary<string, string> data)
        {
            return ("Account Verified - Eventify", WrapInLayout(name,
                """
                <p>Your account has been successfully verified! You can now log in and start using Eventify.</p>
                <p>Thank you for joining our community!</p>
                """));
        }

        // Password Changed
        private static (string, string) BuildPasswordChanged(string name, Dictionary<string, string> data)
        {
            return ("Password Changed - Eventify", WrapInLayout(name,
                """
                <p>Your password has been changed successfully.</p>
                <p>If you did not make this change, please contact support immediately.</p>
                """));
        }

        // Two-Factor Authentication Enabled
        private static (string, string) BuildTwoFactorEnabled(string name, Dictionary<string, string> data)
        {
            return ("Two-Factor Authentication Enabled - Eventify", WrapInLayout(name,
                """
                <p>Two-factor authentication has been enabled on your account for added security.</p>
                <p>If you did not enable this, please contact support immediately.</p>
                """));
        }

        // Two-Factor Authentication Disabled
        private static (string, string) BuildTwoFactorDisabled(string name, Dictionary<string, string> data)
        {
            return ("Two-Factor Authentication Disabled - Eventify", WrapInLayout(name,
                """
                <p>Two-factor authentication has been disabled on your account.</p>
                <p>If you did not disable this, please contact support immediately.</p>
                """));
        }

        // Two-Factor Authentication Code
        private static (string, string) BuildTwoFactorCode(string name, Dictionary<string, string> data)
        {
            var code = data.GetValueOrDefault("Code", "N/A");
            return ("Your Two-Factor Authentication Code - Eventify", WrapInLayout(name,
                $"""
                <p>Your two-factor authentication code is:</p>
                <p style="text-align:center;font-size:24px;font-weight:bold;margin:24px 0;">{data.GetValueOrDefault("TwoFactorToken", "N/A")}</p>
                <p>This code will expire in 10 minutes.</p>
                """));
        }


        private static string WrapInLayout(string recipientName, string content)
        {
            return $"""
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset="utf-8" />
                <meta name="viewport" content="width=device-width, initial-scale=1.0" />
            </head>
            <body style="font-family:Arial,Helvetica,sans-serif;margin:0;padding:0;background-color:#f4f4f4;">
                <div style="max-width:600px;margin:20px auto;background-color:#ffffff;border-radius:8px;overflow:hidden;box-shadow:0 2px 8px rgba(0,0,0,0.1);">
                    <div style="background-color:#4CAF50;padding:20px;text-align:center;">
                        <h1 style="color:#ffffff;margin:0;font-size:24px;">Eventify</h1>
                    </div>
                    <div style="padding:24px;">
                        <p>Hi {recipientName},</p>
                        {content}
                    </div>
                    <div style="background-color:#f9f9f9;padding:16px;text-align:center;font-size:12px;color:#888;">
                        <p>&copy; {DateTime.UtcNow.Year} Eventify. All rights reserved.</p>
                    </div>
                </div>
            </body>
            </html>
            """;
        }
    }
}
