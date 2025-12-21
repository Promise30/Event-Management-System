using Event_Management_System.API.Application;
using Event_Management_System.API.Domain.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Event_Management_System.API.Controllers
{
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
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody]RegisterUserDto registerUser)
        {
            var response = await _authService.RegisterUserAsync(registerUser);
            return StatusCode((int)response.StatusCode, response);
        }
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDto loginUser)
        {
            var response = await _authService.LoginUserAsync(loginUser);
            return StatusCode((int)response.StatusCode, response);
        }
        [HttpPost("refresh-token")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshToken([FromBody] TokenDto tokenDto)
        {
            var response = await _authService.RefreshToken(tokenDto);
            return StatusCode((int)response.StatusCode, response);
        }
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var response = await _authService.GetAllUsersAsync();
            return StatusCode((int)response.StatusCode, response);
        }
    }
}
