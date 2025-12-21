using Event_Management_System.API.Domain.Enums;

namespace Event_Management_System.API.Domain.Entities
{
    public class AuditLog : BaseEntity<Guid>
    {
        public Guid UserId { get; set; }
        public Guid ObjectId { get; set; }
        public ActionType ActionType { get; set; }
        public string? Description { get; set; }
    }
}
