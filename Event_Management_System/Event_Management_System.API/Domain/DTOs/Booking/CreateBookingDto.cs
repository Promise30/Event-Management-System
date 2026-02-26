namespace Event_Management_System.API.Domain.DTOs.Booking
{
    public class CreateBookingDto
    {
        public Guid EventCentreId { get; set; }
        public DateTime BookedFrom { get; set; }
        public DateTime BookedTo { get; set; }

    }
}
