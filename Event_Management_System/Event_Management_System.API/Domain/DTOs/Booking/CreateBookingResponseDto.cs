namespace Event_Management_System.API.Domain.DTOs.Booking
{
    public class CreateBookingResponseDto
    {
        public Guid BookingId { get; set; }
        public Guid EventCentreId { get; set; }
        public string EventCentreName { get; set; } = null!;
        public DateTime BookedFrom { get; set; }
        public DateTime BookedTo { get; set; }
        public string BookingStatus { get; set; } = null!;
        public decimal TotalAmount { get; set; }
        public string? PaymentUrl { get; set; }
        public string? PaymentReference { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
    }
}
