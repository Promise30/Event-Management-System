namespace Event_Management_System.API.Domain.DTOs
{
    public class BookingDto
    {
        public Guid Id{ get; set; }
        public Guid EventCentreId { get; set; }
        public string EventCentreName { get; set; } = null!;
        public DateTime BookedFrom { get; set; }
        public DateTime BookedTo { get; set; }
        public Guid OrganizerId { get; set; }
        public string OrganizerName { get; set; } = null!;
        public string BookingStatus { get; set; } = null!;
        public DateTimeOffset CreatedDate { get; set; }

    }
}
