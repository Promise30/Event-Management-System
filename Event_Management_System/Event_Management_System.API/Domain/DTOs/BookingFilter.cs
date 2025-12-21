using Event_Management_System.API.Domain.Enums;

namespace Event_Management_System.API.Domain.DTOs
{
    public class BookingFilter
    {
        public BookingStatus? BookingStatus { get; set; }
        public Guid? EventCentreId { get; set; } 
        public DateTimeOffset? StartDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }

    }
}
