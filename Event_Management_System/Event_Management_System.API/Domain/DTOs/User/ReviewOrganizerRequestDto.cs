namespace Event_Management_System.API.Domain.DTOs.User
{
    public class ReviewOrganizerRequestDto
    {
        /// <summary>
        /// Optional admin note for the approval/rejection
        /// </summary>
        public string? AdminNote { get; set; }
    }
}
