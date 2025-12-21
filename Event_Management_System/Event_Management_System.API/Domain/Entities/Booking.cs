using Event_Management_System.API.Domain.Enums;
using System.Diagnostics;

namespace Event_Management_System.API.Domain.Entities
{
    [DebuggerDisplay("Booking {Id}: {EventId} at {EventCentreId} from {BookedFrom} to {BookedTo}")]
    public class Booking : BaseEntity<Guid>
    {
        public Guid EventId { get; set; }
        public Event Event { get; set; } = null!;

        // Event centre reserved
        public Guid EventCentreId { get; set; }
        public EventCentre EventCentre { get; set; } = null!;

        public DateTime BookedFrom { get; set; }
        public DateTime BookedTo { get; set; }

        // Organizer who makes the booking
        public Guid OrganizerId { get; set; }
        public ApplicationUser Organizer { get; set; }
        public BookingStatus BookingStatus { get; set; } = BookingStatus.Submitted;
    }
}

// features of event centre
// pricw
// filter event centre by capacity, features and availablity
// eventcentre image url
