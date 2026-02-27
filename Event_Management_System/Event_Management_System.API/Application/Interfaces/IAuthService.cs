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
        Task<APIResponse<object>> VerifyEmailAsync(string email, string token);
        Task<APIResponse<object>> ForgotPasswordAsync(string email);
        Task<APIResponse<object>> ResetPasswordAsync(ResetPasswordDto resetPassword);
        Task<APIResponse<object>> ChangePasswordAsync(ChangePasswordDto changePassword);
        Task<APIResponse<object>> UpdateUserAsync(UpdateUserDto updateUser);
        Task<APIResponse<object>> DeleteUserAsync(string email);
        Task<APIResponse<object>> GetUserRolesAsync(string email);
        Task<APIResponse<object>> Enable2FA(string email);
        Task<APIResponse<object>> Disable2FA(string email);
        Task<APIResponse<TokenDto>> Verify2FA(string email, string token);
        Task<APIResponse<object>> ResendEmailVerificationAsync(string email);
    }
}
