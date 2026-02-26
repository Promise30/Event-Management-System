using Event_Management_System.API.Domain.DTOs.User;
using Event_Management_System.API.Helpers;

namespace Event_Management_System.API.Application.Interfaces
{
    public interface IAuthService
    {
        Task<APIResponse<IEnumerable<UserDto>>> GetAllUsersAsync();
        Task<APIResponse<UserDto>> GetUserAsync(string email);
        Task<APIResponse<object>> LoginUserAsync(LoginDto loginUser);
        Task<APIResponse<TokenDto>> RefreshToken(TokenDto tokenDto);
        Task<APIResponse<object>> RegisterUserAsync(RegisterUserDto registerUser);
    }
}
