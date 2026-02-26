using Event_Management_System.API.Domain.Enums;

namespace Event_Management_System.API.Domain.Entities
{
    public class OrganizerRequest : BaseEntity<Guid>
    {
        /// <summary>
        /// The user requesting to become an Organizer
        /// </summary>
        public Guid UserId { get; set; }
        public ApplicationUser User { get; set; } = null!;

        /// <summary>
        /// Optional reason/motivation from the user
        /// </summary>
        public string? Reason { get; set; }

        /// <summary>
        /// Current status of the request
        /// </summary>
        public OrganizerRequestStatus Status { get; set; } = OrganizerRequestStatus.Pending;

        /// <summary>
        /// Admin who processed the request (null while pending)
        /// </summary>
        public Guid? ReviewedByAdminId { get; set; }
        public ApplicationUser? ReviewedByAdmin { get; set; }

        /// <summary>
        /// Optional admin note when approving/rejecting
        /// </summary>
        public string? AdminNote { get; set; }

        /// <summary>
        /// When the request was reviewed
        /// </summary>
        public DateTimeOffset? ReviewedAt { get; set; }
    }
}
