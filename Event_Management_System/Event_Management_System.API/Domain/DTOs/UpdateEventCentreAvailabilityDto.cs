namespace Event_Management_System.API.Domain.DTOs
{
    public class UpdateEventCentreAvailabilityDto
    {
        public DayOfWeek Day { get; set; }
        public TimeSpan OpenTime { get; set; }
        public TimeSpan CloseTime { get; set; }
    }
}
