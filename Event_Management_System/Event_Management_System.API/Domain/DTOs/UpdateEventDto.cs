namespace Event_Management_System.API.Domain.DTOs
{
    public class UpdateEventDto
    {
        public string Title { get; set; }
        public string? Description { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string? EventFlyer { get; set; }
        public int NumberOfAttnedees { get; set; }
        public Guid BookingId { get; set; }
    }
}
