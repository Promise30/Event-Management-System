namespace Event_Management_System.API.Domain.DTOs.User
{
    public class ChangePasswordDto
    {
        public string Email { get; set; }
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
    }
}
