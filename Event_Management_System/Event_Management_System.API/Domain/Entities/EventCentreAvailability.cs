namespace Event_Management_System.API.Domain.Entities
{
    public class EventCentreAvailability : BaseEntity<Guid>
    {
        public Guid EventCentreId { get; set; }
        public EventCentre EventCentre { get; set; }
        public DayOfWeek Day {  get; set; } 
        public TimeSpan OpenTime { get; set; }
        public TimeSpan CloseTime { get; set; }
    }
}
