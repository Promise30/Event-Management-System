namespace Event_Management_System.API.Domain.DTOs.User
{
    public class CreateOrganizerRequestDto
    {
        /// <summary>
        /// Optional reason/motivation for becoming an organizer
        /// </summary>
        public string? Reason { get; set; }
    }
}
