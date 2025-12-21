namespace Event_Management_System.API.Domain.DTOs
{
    public class AddEventCentreDto
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public string Location { get; set; }
        public int Capacity { get; set; }
    }
}
