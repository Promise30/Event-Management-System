using System.ComponentModel;

namespace Event_Management_System.API.Domain.Enums
{
    public enum NotificationType
    {
        // Bookings
        [Description("Booking Submitted")]
        BookingSubmitted,
        [Description("Booking Confirmed")]
        BookingConfirmed,
        [Description("Booking Cancelled")]
        BookingCancelled,
        [Description("Booking Payment Failed")]
        BookingPaymentFailed,
        [Description("Booking Rejected")]
        BookingRejected,

        // Tickets
        [Description("Ticket Purchase Confirmed")]
        TicketPurchaseConfirmed,
        [Description("Ticket Purchase Failed")]
        TicketCancelledRefunded,
        [Description("Ticket Purchase Cancelled")]
        TicketPurchaseCancelled,

        // Events
        [Description("Event Details Updated")]
        EventDetailsUpdated,
        [Description("Event Cancelled")]
        EventCancelled,
        [Description("Event Reminder")]
        EventReminder,

        // Organizer
        [Description("Organizer Request Approved")]
        OrganizerRequestApproved,
        [Description("Organizer Request Rejected")]
        OrganizerRequestRejected,


        // Payments
        [Description("Payment Successful")]
        PaymentSuccessful,
        [Description("Payment Failed")]
        PaymentFailed,
        [Description("Refund Processed")]
        RefundProcessed,

        // Authentication
        [Description("Password Reset")]
        PasswordReset,
        [Description("Email Verification")]
        EmailVerification,
        [Description("Account Verification")]
        AccountVerification,
        [Description("Password Changed")]
        PasswordChanged,
        [Description("Two-Factor Authentication Enabled")]
        TwoFactorEnabled,
        [Description("Two-Factor Authentication Disabled")]
        TwoFactorDisabled,
         [Description("Two-Factor Authentication Code")]
         TwoFactorCode,

    }
}
