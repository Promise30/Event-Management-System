using System.ComponentModel.DataAnnotations;

namespace Event_Management_System.API.Domain.DTOs.User
{
    public class RegisterUserDto
    {
        [Required(ErrorMessage = "FirstName field is required")]
        public string FirstName { get; set; } = null!;
        [Required(ErrorMessage = "LastName field is required")]
        public string LastName { get; set; } = null!;
        [Required(ErrorMessage = "Email field is required")]
        public string Email { get; set; } = null!;
        [Required(ErrorMessage = "Password field is required")]
        public string Password { get; set; } = null!;
        [Required(ErrorMessage = "Username field is required")]
        public string UserName { get; set; } = null!;
    }
}
