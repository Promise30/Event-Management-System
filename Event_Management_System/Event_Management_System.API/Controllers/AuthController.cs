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
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status500InternalServerError)]
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
        [ProducesResponseType(typeof(APIResponse<List<UserDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllUsers()
        {
            var response = await _authService.GetAllUsersAsync();
            return StatusCode((int)response.StatusCode, response);
        }
    }
}
