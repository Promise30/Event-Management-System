namespace Event_Management_System.API.Domain.DTOs.Event
{
    public class CreateEventDto
    {
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string? EventFlyer { get; set; }
        public int NumberOfAttendees { get; set; }
        public Guid BookingId { get; set; }

    }
}
