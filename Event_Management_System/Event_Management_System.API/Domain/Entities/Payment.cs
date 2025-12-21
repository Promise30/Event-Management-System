using Event_Management_System.API.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Event_Management_System.API.Domain.Entities
{
    public class Payment
    {
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// User who initiated the payment
        /// </summary>
        public Guid UserId { get; set; }
        public ApplicationUser User { get; set; }

        /// <summary>
        /// Payment provider (Flutterwave, Paystack)
        /// </summary>
        public PaymentProvider Provider { get; set; }

        /// <summary>
        /// Internal transaction reference
        /// </summary>
        public string TransactionReference { get; set; }

        /// <summary>
        /// Provider's transaction reference
        /// </summary>
        public string ProviderReference { get; set; }

        /// <summary>
        /// Payment type (Booking or Ticket)
        /// </summary>
        public int PaymentType { get; set; } // 1=Booking, 2=Ticket

        /// <summary>
        /// Reference to Booking or Ticket
        /// </summary>
        public Guid ReferenceId { get; set; }

        /// <summary>
        /// Payment amount
        /// </summary>
        public decimal Amount { get; set; }

        public string Currency { get; set; } = "NGN";

        /// <summary>
        /// Current payment status
        /// </summary>
        public PaymentStatus Status { get; set; }

        /// <summary>
        /// Payment URL for customer
        /// </summary>
        public string PaymentUrl { get; set; }

        public string Description { get; set; }

        /// <summary>
        /// Customer email
        /// </summary>
        public string CustomerEmail { get; set; }

        /// <summary>
        /// When payment was confirmed
        /// </summary>
        public DateTimeOffset? PaidAt { get; set; }

        /// <summary>
        /// Additional metadata (JSON)
        /// </summary>
        public string Metadata { get; set; }

        public DateTimeOffset CreatedDate { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
    }
}
