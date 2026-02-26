namespace Event_Management_System.API.Domain.Enums
{
    public enum BookingStatus
    {
        Submitted = 0,        // Free center booking, awaiting admin approval
        PendingPayment = 1,   // Paid center, payment initiated but not completed
        PendingApproval = 2,  // Payment confirmed, now awaiting admin approval
        Confirmed = 3,        // Admin approved
        Rejected = 4,         // Admin rejected
        Cancelled = 5,        // User cancelled
        Expired = 6           // Payment window lapsed, booking discarded
    }
}

