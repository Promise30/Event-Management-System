using Event_Management_System.API.Application.Interfaces;
using Event_Management_System.API.Domain.DTOs.User;
using Event_Management_System.API.Domain.Entities;
using Event_Management_System.API.Domain.Enums;
using Event_Management_System.API.Helpers;
using Event_Management_System.API.Infrastructures;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text.Json;

namespace Event_Management_System.API.Application.Implementation
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly ILogger<AuthService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITokenService _tokenService;
        private readonly ApplicationDbContext _dbContext;
        public AuthService(UserManager<ApplicationUser> userManager, ILogger<AuthService> logger, IConfiguration configuration, IHttpContextAccessor httpContextAccessor, ITokenService tokenService, RoleManager<IdentityRole<Guid>> roleManager, ApplicationDbContext dbContext)
        {
            _userManager = userManager;
            _logger = logger;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _tokenService = tokenService;
            _roleManager = roleManager;
            _dbContext = dbContext;
        }
        public async Task<APIResponse<object>> RegisterUserAsync(RegisterUserDto registerUser)
        {
            var user = new ApplicationUser
            {
                FirstName = registerUser.FirstName,
                LastName = registerUser.LastName,
                UserName = registerUser.UserName,
                NormalizedUserName = registerUser.UserName.ToUpper(),
                NormalizedEmail = registerUser.Email.ToUpper(),
                Email = registerUser.Email,
                CreatedDate = DateTimeOffset.UtcNow,
                ModifiedDate = DateTimeOffset.UtcNow
            };

            var result = await _userManager.CreateAsync(user, registerUser.Password);
            if (!result.Succeeded)
            {
                _logger.LogError($" -----> An error occurred when trying to register new user: {result.Errors.FirstOrDefault()!.Description}");
                return APIResponse<object>.Create(HttpStatusCode.BadRequest, "An error occurred when trying to register new user", null, new Error { Message = "An error occurred when trying to register new user" });
            }

            // Assign the "User" role to the newly registered user
            if (!await _roleManager.RoleExistsAsync("User"))
            {
                await _roleManager.CreateAsync(new IdentityRole<Guid>("User"));
            }
            await _userManager.AddToRoleAsync(user, "User");
            _logger.LogInformation($" ----> New user registered with email {user.Email}");

            // Create a log entry
            var auditLog = new AuditLog
            {
                UserId = user.Id,
                ActionType = ActionType.Create,
                ObjectId = user.Id,
                Description = $"New user registered with email {user.Email}",
                CreatedDate = DateTimeOffset.UtcNow,
                ModifiedDate = DateTimeOffset.UtcNow
            };
            _dbContext.AuditLogs.Add(auditLog);
            await _dbContext.SaveChangesAsync();

            return APIResponse<object>.Create(HttpStatusCode.OK, "User registered successfully", null);

        }
        public async Task<APIResponse<object>> LoginUserAsync(LoginDto loginUser)
        {
            var user = await _userManager.FindByEmailAsync(loginUser.Email);
            if (user == null)
            {
                _logger.LogInformation($" ----> User with email {loginUser.Email} does not exist in the system");
                return APIResponse<object>.Create(HttpStatusCode.NotFound, "User not found", null, new Error { Message= "User not found"});
            }
            var result = await _userManager.CheckPasswordAsync(user, loginUser.Password);
            if (!result)
            {
                _logger.LogInformation($" ----> User with email {loginUser.Email} entered an incorrect password");
                return APIResponse<object>.Create(HttpStatusCode.BadRequest, "Request unsuccessful", null, new Error { Message= "Request unsuccessful"});
            }

            var token = await _tokenService.CreateToken(true, user);
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
            };
            _logger.LogInformation("Token generated for user {email}: {token}", user.Email, JsonSerializer.Serialize(token, options));

            var response = new TokenDto
            {
                AccessToken = token.AccessToken,
                RefreshToken = token.RefreshToken
            };
            return APIResponse<object>.Create(HttpStatusCode.OK, "User logged in successfully", response);

        }

        public async Task<APIResponse<IEnumerable<UserDto>>> GetAllUsersAsync()
        {
            // get the users from the database along with their roles and deserialize into UserDto class
            var users = await _userManager.Users.ToListAsync();
            var userDtos = new List<UserDto>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var userDto = new UserDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    UserName = user.UserName,
                    Email = user.Email,
                    Role = roles.FirstOrDefault() ?? string.Empty,
                    DateCreated = user.CreatedDate,
                    DateUpdated = user.ModifiedDate
                };
                userDtos.Add(userDto);
            }
            _logger.LogInformation($"--> {userDtos.Count} users retrieved from the system");

            return APIResponse<IEnumerable<UserDto>>.Create(HttpStatusCode.OK, "Users retrieved successfully", userDtos);

        }

        public async Task<APIResponse<UserDto>> GetUserAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                _logger.LogInformation($"--> User with email {email} does not exist in the system");
                return APIResponse<UserDto>.Create(HttpStatusCode.NotFound, "User not found", null);
            }
            var userDto = new UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                UserName = user.UserName,
                Email = user.Email,
                DateCreated = user.CreatedDate,
                DateUpdated = user.ModifiedDate
            };
            _logger.LogInformation($"--> User with email {email} found in the system");
            return APIResponse<UserDto>.Create(HttpStatusCode.OK, "User retrieved successfully", userDto);
        }
        public async Task<APIResponse<TokenDto>> RefreshToken(TokenDto tokenDto)
        {
            var principal = _tokenService.GetPrincipalFromExpiredToken(tokenDto.AccessToken);
            var user = await _userManager.FindByNameAsync(principal.Identity.Name);
            if (user == null || user.RefreshToken != tokenDto.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
                return APIResponse<TokenDto>.Create(HttpStatusCode.BadRequest, "Invalid request. The tokenDto has some invalid values.", null);
            var newToken = await _tokenService.CreateToken(populateExp: false, user);
            return APIResponse<TokenDto>.Create(HttpStatusCode.OK, "Request Successful", newToken);
        }
    }
}
