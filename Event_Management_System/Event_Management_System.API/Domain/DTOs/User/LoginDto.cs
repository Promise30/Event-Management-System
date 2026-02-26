using System.ComponentModel.DataAnnotations;

namespace Event_Management_System.API.Domain.DTOs.User
{
    public class LoginDto
    {
        [Required(ErrorMessage = "Email field is required")]
        public string Email { get; set; } = null!;
        [Required(ErrorMessage = "Password field is required")]
        public string Password { get; set; } = null!;
    }
}
