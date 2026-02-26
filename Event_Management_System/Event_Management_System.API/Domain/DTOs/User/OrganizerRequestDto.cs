namespace Event_Management_System.API.Domain.DTOs.User
{
    public class OrganizerRequestDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; } = null!;
        public string UserEmail { get; set; } = null!;
        public string? Reason { get; set; }
        public string Status { get; set; } = null!;
        public string? AdminNote { get; set; }
        public Guid? ReviewedByAdminId { get; set; }
        public DateTimeOffset? ReviewedAt { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
    }
}
