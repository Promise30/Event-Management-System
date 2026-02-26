namespace Event_Management_System.API.Domain.DTOs.EventCenter
{
    public class AddEventCentreDto
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public string Location { get; set; }
        public int Capacity { get; set; }

        /// <summary>
        /// Price per day for booking this event centre. 0 means free.
        /// </summary>
        public decimal PricePerDay { get; set; }
    }
}
