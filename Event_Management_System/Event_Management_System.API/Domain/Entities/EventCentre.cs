namespace Event_Management_System.API.Domain.Entities
{
    public class EventCentre : BaseEntity<Guid>
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string Location { get; set; } = null!;
        public int Capacity { get; set; }
        //public bool AvailableOnWeekends { get; set; }
        public bool IsActive { get; set; }

        /// <summary>
        /// Price per day for booking this event centre. 0 means free.
        /// </summary>
        public decimal PricePerDay { get; set; }

        public ICollection<EventCentreAvailability> Availabilities { get; set; } = new List<EventCentreAvailability>();
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
