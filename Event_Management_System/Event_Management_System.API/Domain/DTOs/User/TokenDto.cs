namespace Event_Management_System.API.Domain.DTOs.User
{
    public class TokenDto
    {
        public string AccessToken { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
    }
}
