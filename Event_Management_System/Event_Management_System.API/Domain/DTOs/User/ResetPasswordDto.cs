using System.ComponentModel.DataAnnotations;

namespace Event_Management_System.API.Domain.DTOs.User
{
    public class ResetPasswordDto
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string Token { get; set; }
        [Required]
        public string Password { get; set; }

    }
}
