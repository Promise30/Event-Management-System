namespace Event_Management_System.API.Domain.DTOs
{
    public class CreateBookingDto
    {
        public Guid EventCentreId { get; set; }
        public DateTime BookedFrom { get; set; }
        public DateTime BookedTo { get; set; }

    }
}
