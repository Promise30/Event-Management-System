using Event_Management_System.API.Application.Interfaces;
using Event_Management_System.API.Domain.DTOs.User;
using Event_Management_System.API.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Event_Management_System.API.Controllers
{
    /// <summary>
    /// Handles user authentication and account management operations
    /// </summary>
    [Authorize]
    [Route("auth")]
    [ApiController]
    public class AuthController : BaseController
    {
        private readonly IAuthService _authService;
        public AuthController(IHttpContextAccessor contextAccessor, IConfiguration configuration, IAuthService authService) : base(contextAccessor, configuration)
        {
            _authService = authService;
        }

        /// <summary>
        /// Register a new user account
        /// </summary>
        /// <param name="registerUser">User registration details including name, email, and password</param>
        /// <returns>The newly created user details</returns>
        /// <response code="201">User registered successfully</response>
        /// <response code="400">Invalid registration data or user already exists</response>
        /// <response code="500">An internal server error occurred</response>
        [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(APIResponse<UserDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto registerUser)
        {
            var response = await _authService.RegisterUserAsync(registerUser);
            return StatusCode((int)response.StatusCode, response);
        }

        /// <summary>
        /// Authenticate a user and return access and refresh tokens
        /// </summary>
        /// <param name="loginUser">Login credentials including email and password</param>
        /// <returns>JWT access token and refresh token</returns>
        /// <response code="200">Login successful</response>
        /// <response code="400">Invalid credentials</response>
        /// <response code="500">An internal server error occurred</response>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(APIResponse<TokenDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Login([FromBody] LoginDto loginUser)
        {
            var response = await _authService.LoginUserAsync(loginUser);
            return StatusCode((int)response.StatusCode, response);
        }

        /// <summary>
        /// Refresh an expired access token using a valid refresh token
        /// </summary>
        /// <param name="tokenDto">The expired access token and valid refresh token</param>
        /// <returns>New JWT access token and refresh token</returns>
        /// <response code="200">Token refreshed successfully</response>
        /// <response code="400">Invalid or expired tokens</response>
        /// <response code="500">An internal server error occurred</response>
        [HttpPost("refresh-token")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(APIResponse<TokenDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RefreshToken([FromBody] TokenDto tokenDto)
        {
            var response = await _authService.RefreshToken(tokenDto);
            return StatusCode((int)response.StatusCode, response);
        }

        /// <summary>
        /// Retrieve all registered users in the system
        /// </summary>
        /// <returns>A list of all users</returns>
        /// <response code="200">Users retrieved successfully</response>
        /// <response code="500">An internal server error occurred</response>
        [HttpGet("users")]
        [Authorize(Roles ="Administrator")]
        [ProducesResponseType(typeof(APIResponse<List<UserDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllUsers()
        {
            var response = await _authService.GetAllUsersAsync();
            return StatusCode((int)response.StatusCode, response);
        }

        /// <summary>
        /// Retrieve a user by their unique identifier
        /// </summary>
        /// <param name="email">The unique identifier of the user</param>
        /// <returns>The user details</returns>
        /// <response code="200">User retrieved successfully</response>
        /// <response code="404">User not found</response>
        /// <response code="500">An internal server error occurred</response>
        [HttpGet("users/{email}")]
        [ProducesResponseType(typeof(APIResponse<UserDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUserById(string email)
        {
            var response = await _authService.GetUserAsync(email);
            return StatusCode((int)response.StatusCode, response);
        }

        /// <summary>
        /// Delete a user by their unique identifier
        /// </summary>
        /// <param name="email">The unique identifier of the user</param>
        /// <returns>No content</returns>
        /// <response code="204">User deleted successfully</response>
        /// <response code="404">User not found</response>
        /// <response code="500">An internal server error occurred</response>
        [HttpDelete("users/{email}")]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteUser(string email)
        {
            var response = await _authService.DeleteUserAsync(email);
            return StatusCode((int)response.StatusCode, response);
        }

        /// <summary>
        /// Change the password of the logged-in user
        /// </summary>
        /// <param name="changePassword">Current password and new password details</param>
        /// <returns>No content</returns>
        /// <response code="200">Password changed successfully</response>
        /// <response code="400">Invalid password details</response>
        /// <response code="500">An internal server error occurred</response>
        [HttpPost("change-password")]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePassword)
        {
            var response = await _authService.ChangePasswordAsync(changePassword);
            return StatusCode((int)response.StatusCode, response);
        }

        /// <summary>
        /// Enable two-factor authentication (2FA) for a user
        /// </summary>
        /// <param name="email">The email of the user</param>
        /// <returns>No content</returns>
        /// <response code="200">2FA enabled successfully</response>
        /// <response code="400">Invalid request</response>
        /// <response code="500">An internal server error occurred</response>
        [HttpPost("2fa/enable")]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Enable2FA([FromQuery] string email)
        {
            var response = await _authService.Enable2FA(email);
            return StatusCode((int)response.StatusCode, response);
        }

        /// <summary>
        /// Disable two-factor authentication (2FA) for a user
        /// </summary>
        /// <param name="email">The email of the user</param>
        /// <returns>No content</returns>
        /// <response code="200">2FA disabled successfully</response>
        /// <response code="400">Invalid request</response>
        /// <response code="500">An internal server error occurred</response>
        [HttpPost("2fa/disable")]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Disable2FA([FromQuery] string email)
        {
            var response = await _authService.Disable2FA(email);
            return StatusCode((int)response.StatusCode, response);
        }

        /// <summary>
        /// Verify the two-factor authentication (2FA) code for a user
        /// </summary>
        /// <param name="email">The email of the user</param>
        /// <param name="token">The 2FA code</param>
        /// <returns>JWT access token and refresh token</returns>
        /// <response code="200">2FA verified successfully</response>
        /// <response code="400">Invalid 2FA code</response>
        /// <response code="500">An internal server error occurred</response>
        [HttpPost("2fa/verify")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(APIResponse<TokenDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Verify2FA([FromQuery] string email, [FromQuery] string token)
        {
            var response = await _authService.Verify2FA(email, token);
            return StatusCode((int)response.StatusCode, response);
        }

        /// <summary>
        /// Initiate the password reset process for a user
        /// </summary>
        /// <param name="email">The email of the user</param>
        /// <returns>No content</returns>
        /// <response code="200">Password reset initiated successfully</response>
        /// <response code="400">Invalid email</response>
        /// <response code="500">An internal server error occurred</response>
        [HttpPost("forgot-password")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ForgotPassword([FromQuery] string email)
        {
            var response = await _authService.ForgotPasswordAsync(email);
            return StatusCode((int)response.StatusCode, response);
        }

        /// <summary>
        /// Reset the password for a user
        /// </summary>
        /// <param name="resetPassword">Password reset information including email, token, and new password</param>
        /// <returns>No content</returns>
        /// <response code="200">Password reset successfully</response>
        /// <response code="400">Invalid password reset information</response>
        /// <response code="500">An internal server error occurred</response>
        [HttpPost("reset-password")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPassword)
        {
            var response = await _authService.ResetPasswordAsync(resetPassword);
            return StatusCode((int)response.StatusCode, response);
        }

        /// <summary>
        /// Retrieve the roles assigned to a user
        /// </summary>
        /// <param name="email">The email of the user</param>
        /// <returns>A list of roles</returns>
        /// <response code="200">Roles retrieved successfully</response>
        /// <response code="404">User not found</response>
        /// <response code="500">An internal server error occurred</response>
        [HttpGet("users/{email}/roles")]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUserRoles([FromRoute] string email)
        {
            var response = await _authService.GetUserRolesAsync(email);
            return StatusCode((int)response.StatusCode, response);
        }

        /// <summary>
        /// Resend the email verification link to a user
        /// </summary>
        /// <param name="email">The email of the user</param>
        /// <returns>No content</returns>
        /// <response code="200">Verification email resent successfully</response>
        /// <response code="400">Invalid email</response>
        /// <response code="500">An internal server error occurred</response>
        [HttpPost("users/resend-verification")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ResendEmailVerification([FromQuery] string email)
        {
            var response = await _authService.ResendEmailVerificationAsync(email);
            return StatusCode((int)response.StatusCode, response);
        }

        /// <summary>
        /// Verify the email address of a user
        /// </summary>
        /// <param name="email">The email of the user</param>
        /// <param name="token">The verification token</param>
        /// <returns>No content</returns>
        /// <response code="200">Email verified successfully</response>
        /// <response code="400">Invalid verification token</response>
        /// <response code="500">An internal server error occurred</response>
        [HttpGet("verify-email")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> VerifyEmail([FromQuery] string email, [FromQuery] string token)
        {
            var response = await _authService.VerifyEmailAsync(email, token);
            return StatusCode((int)response.StatusCode, response);
        }

        /// <summary>
        /// Update the details of a user
        /// </summary>
        /// <param name="updateUser">The new user details</param>
        /// <returns>No content</returns>
        /// <response code="200">User updated successfully</response>
        /// <response code="400">Invalid user details</response>
        /// <response code="500">An internal server error occurred</response>
        [HttpPut("users/update")]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUserDto updateUser)
        {
            var response = await _authService.UpdateUserAsync(updateUser);
            return StatusCode((int)response.StatusCode, response);
        }
    }
}
