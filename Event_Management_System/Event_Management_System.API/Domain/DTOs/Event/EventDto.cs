namespace Event_Management_System.API.Domain.DTOs.Event
{
    public class EventDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int Capacity { get; set; }
        public Guid BookingId { get; set; }

    }
}
