namespace Event_Management_System.API.Domain.DTOs.EventCenter
{
    public class AddEventCentreAvailabilityDto
    {
        public Guid EventCentreId { get; set; }
        public DayOfWeek Day { get; set; }
        public TimeSpan OpenTime { get; set; }
        public TimeSpan CloseTime { get; set; }
    }
}
