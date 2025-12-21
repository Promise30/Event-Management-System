namespace Event_Management_System.API.Domain.DTOs
{
    public class EventCentreDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public int Capacity { get; set; }
        public DateTimeOffset CreatedDate{ get; set; }
        public ICollection<EventCentreAvailabilityDto> Availabilities { get; set; } = new List<EventCentreAvailabilityDto>();

    }
}
