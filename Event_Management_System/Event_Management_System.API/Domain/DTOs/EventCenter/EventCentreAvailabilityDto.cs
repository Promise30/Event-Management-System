namespace Event_Management_System.API.Domain.DTOs.EventCenter
{
    public class EventCentreAvailabilityDto
    {
        public Guid Id { get; set; }
        public Guid EventCentreId { get; set; }
        public string Day { get; set; }
        public TimeSpan OpenTime { get; set; }
        public TimeSpan CloseTime { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
    }
}
