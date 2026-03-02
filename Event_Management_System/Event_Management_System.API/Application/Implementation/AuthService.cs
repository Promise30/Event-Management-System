using Event_Management_System.API.Application.Interfaces;
using Event_Management_System.API.Domain.DTOs;
using Event_Management_System.API.Domain.DTOs.User;
using Event_Management_System.API.Domain.Entities;
using Event_Management_System.API.Domain.Enums;
using Event_Management_System.API.Helpers;
using Event_Management_System.API.Infrastructures;
using Hangfire;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Text;
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
        private readonly INotificationService _notificationService;
        public AuthService(UserManager<ApplicationUser> userManager, ILogger<AuthService> logger, IConfiguration configuration, IHttpContextAccessor httpContextAccessor, ITokenService tokenService, RoleManager<IdentityRole<Guid>> roleManager, ApplicationDbContext dbContext, INotificationService notificationService = null)
        {
            _userManager = userManager;
            _logger = logger;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _tokenService = tokenService;
            _roleManager = roleManager;
            _dbContext = dbContext;
            _notificationService = notificationService;
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

            // Send email verification notification
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            _logger.LogInformation($" ----> Email verification token generated for user '{user.Email}': {token}");
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            var request = new NotificationRequest
            {
                Channels = new[] { NotificationChannel.Email },
                Type = NotificationType.EmailVerification,
                RecipientEmail = user.Email,
                RecipientName = $"{user.FirstName} {user.LastName}",
                Data = new Dictionary<string, string>
                {
                    { "FirstName", user.FirstName },
                    { "VerificationLink", $"https://yourapp.com/verify-email?token={encodedToken}&email={user.Email}" }
                }
            };

            BackgroundJob.Enqueue(() => _notificationService.SendAsync(request));

            return APIResponse<object>.Create(HttpStatusCode.OK, "User registered successfully", null);

        }
        public async Task<APIResponse<object>> LoginUserAsync(LoginDto loginUser)
        {
                var user = await _userManager.FindByEmailAsync(loginUser.Email);
                if (user == null)
                {
                _logger.LogInformation($" ----> User with email {loginUser.Email} does not exist in the system");

                return APIResponse<object>.Create(HttpStatusCode.NotFound, "User not found", null, new Error { Message = "User not found" });

                }
                if (!user.EmailConfirmed)
                {
                    _logger.LogInformation($" ---> User with email {loginUser.Email} has not been verified");
                    return APIResponse<object>.Create(HttpStatusCode.BadRequest, "User email has not been verified", null);
                }
            
            if (user.TwoFactorEnabled)
                {
                    var twoFactorToken = await GenerateOTPForTwoFactor(user);
                    if (string.IsNullOrEmpty(twoFactorToken))
                    {
                        _logger.LogError($"Failed to generate 2FA token for user {loginUser.Email}");
                        return APIResponse<object>.Create(HttpStatusCode.BadRequest, "Failed to generate 2FA token", null);
                    }
                    _logger.LogInformation($"2FA token generated for user '{loginUser.Email}': {twoFactorToken}");

                    // Two factor code notification
                    var userTokenRequest = new NotificationRequest
                    {
                        Channels = new[] { NotificationChannel.Email },
                        Type = NotificationType.TwoFactorCode,
                        RecipientEmail = loginUser.Email,
                        RecipientName = $"{user.FirstName} {user.LastName}",
                        Data = new Dictionary<string, string>
                        {
                            { "FirstName", user.FirstName },
                            { "TwoFactorToken", twoFactorToken },
                            { "DatePublished", DateTime.UtcNow.ToString("o") }
                        }
                    };
                    BackgroundJob.Enqueue(() => _notificationService.SendAsync(userTokenRequest));

                    return APIResponse<object>.Create(HttpStatusCode.OK, "2FA token generated successfully", null);
                }

                var result = await _userManager.CheckPasswordAsync(user, loginUser.Password);
                if (!result)
                {
                    _logger.LogInformation($"User with email {loginUser.Email} entered an incorrect password");
                    return APIResponse<object>.Create(HttpStatusCode.BadRequest, "Incorrect password", null);
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
        public async Task<APIResponse<object>> ChangePasswordAsync(ChangePasswordDto changePassword)
        {
            var user = await _userManager.FindByEmailAsync(changePassword.Email);
            if (user == null)
            {
                _logger.LogInformation($"--> User with email {changePassword.Email} does not exist in the system");
                return APIResponse<object>.Create(HttpStatusCode.NotFound, "User not found", null);
            }
            var result = await _userManager.ChangePasswordAsync(user, changePassword.CurrentPassword, changePassword.NewPassword);
            if (!result.Succeeded)
            {
                _logger.LogError($"--> An error occurred when trying to change user password: {result.Errors}");
                return APIResponse<object>.Create(HttpStatusCode.BadRequest, "An error occurred when trying to change user password", null);
            }
            return APIResponse<object>.Create(HttpStatusCode.OK, "User password changed successfully", null);

        }
        public async Task<APIResponse<object>> DeleteUserAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                _logger.LogInformation($"--> User with email {email} does not exist in the system");
                return APIResponse<object>.Create(HttpStatusCode.NotFound, "User not found", null);
            }
            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                _logger.LogError($"--> An error occurred when trying to delete user: {result.Errors}");
                return APIResponse<object>.Create(HttpStatusCode.BadRequest, "An error occurred when trying to delete user", null);
            }
            return APIResponse<object>.Create(HttpStatusCode.OK, "User deleted successfully", null);

        }
        public async Task<APIResponse<object>> Disable2FA(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                _logger.LogInformation($"--> User with email {email} does not exist in the system");
                return APIResponse<object>.Create(HttpStatusCode.NotFound, "User not found", null);
            }
            var result = await _userManager.SetTwoFactorEnabledAsync(user, false);
            if (!result.Succeeded)
            {
                _logger.LogError($"--> An error occurred when trying to disable 2FA: {result.Errors.FirstOrDefault()!.Description}");
                return APIResponse<object>.Create(HttpStatusCode.BadRequest, "An error occurred when trying to disable 2FA", null);
            }
                var user2FADisableNotification = new NotificationRequest
                {
                    Channels = new[] { NotificationChannel.Email },
                    Type = NotificationType.TwoFactorDisabled,
                    RecipientEmail = email,
                    RecipientName = $"{user.FirstName} {user.LastName}",
                    Data = new Dictionary<string, string>
                {
                    { "FirstName", user.FirstName },
                    { "DatePublished", DateTime.UtcNow.ToString("o") }
                }
                };
                BackgroundJob.Enqueue(() => _notificationService.SendAsync(user2FADisableNotification));
            return APIResponse<object>.Create(HttpStatusCode.OK, "2FA disabled successfully", null);
        }
        public async Task<APIResponse<object>> Enable2FA(string email)
        {

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                _logger.LogInformation($"--> User with email {email} does not exist in the system");
                return APIResponse<object>.Create(HttpStatusCode.NotFound, "User not found", null);
            }
            var result = await _userManager.SetTwoFactorEnabledAsync(user, true);
            if (!result.Succeeded)
            {
                _logger.LogError($"--> An error occurred when trying to enable 2FA: {result.Errors.FirstOrDefault()?.Description}");
                return APIResponse<object>.Create(HttpStatusCode.BadRequest, "An error occurred when trying to enable 2FA", null);
            }
            var user2FAEnableNotification = new NotificationRequest
            {
                Channels = new[] { NotificationChannel.Email },
                Type = NotificationType.TwoFactorEnabled,
                RecipientEmail = email,
                RecipientName = $"{user.FirstName} {user.LastName}",
                Data = new Dictionary<string, string>
             {
                 { "FirstName", user.FirstName },
                 { "DatePublished", DateTime.UtcNow.ToString("o") }
             }
            };
            BackgroundJob.Enqueue(() => _notificationService.SendAsync(user2FAEnableNotification));
            return APIResponse<object>.Create(HttpStatusCode.OK, "2FA enabled successfully", null);

        }
        public async Task<APIResponse<object>> ForgotPasswordAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                _logger.LogInformation("--> Forgot password requested for non-existent email: {Email}", email);
                return APIResponse<object>.Create(HttpStatusCode.OK,
                    "If an account exists with that email, a password reset link has been sent.", null);
            }
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            _logger.LogInformation($"--> Forgot password token generated for user '{email}': {token}");

            var request = new NotificationRequest
            {
                Channels = new[] { NotificationChannel.Email },
                Type = NotificationType.PasswordReset,
                RecipientEmail = user.Email,
                RecipientName = $"{user.FirstName} {user.LastName}",
                Data = new Dictionary<string, string>
            {
                { "FirstName", user.FirstName },
                { "ResetLink", $"https://yourapp.com/reset-password?token={encodedToken}&email={user.Email}" }
            }
            };
            BackgroundJob.Enqueue(() => _notificationService.SendAsync(request));
            //return APIResponse<object>.Create(HttpStatusCode.OK, "If an account exists with that email, a reset link has been sent.", null);
            return APIResponse<object>.Create(HttpStatusCode.OK, "Password reset token generated successfully", new { Token = token });
        }
        public async Task<APIResponse<object>> GetUserRolesAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                _logger.LogInformation($"--> User with email {email} does not exist in the system");
                return APIResponse<object>.Create(HttpStatusCode.NotFound, "User not found", null);
            }
            var roles = await _userManager.GetRolesAsync(user);
            _logger.LogInformation($"--> User with email {email} has {roles.Count} roles");
            return APIResponse<object>.Create(HttpStatusCode.OK, "User roles retrieved successfully", new { Roles = roles });

        }
        public async Task<APIResponse<object>> ResendEmailVerificationAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    _logger.LogInformation("--> Resend verification requested for non-existent email: {Email}", email);
                    return APIResponse<object>.Create(HttpStatusCode.OK,
                        "If an account exists with that email, a verification link has been sent.", null);
                }

                if (user.EmailConfirmed)
                {
                    _logger.LogInformation("--> Resend verification requested for already verified email: {Email}", email);
                    return APIResponse<object>.Create(HttpStatusCode.BadRequest,
                        "This email has already been verified.", null);
                }

                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

                var request = new NotificationRequest
                {
                    Channels = new[] { NotificationChannel.Email },
                    Type = NotificationType.EmailVerification,
                    RecipientEmail = user.Email,
                    RecipientName = $"{user.FirstName} {user.LastName}",
                    Data = new Dictionary<string, string>
                    {
                        { "FirstName", user.FirstName },
                        { "VerificationLink", $"https://yourapp.com/verify-email?token={encodedToken}&email={user.Email}" }
                    }
                };

                BackgroundJob.Enqueue(() => _notificationService.SendAsync(request));

                _logger.LogInformation("--> Email verification link sent for user {Email}", user.Email);
                return APIResponse<object>.Create(HttpStatusCode.OK,
                    "If an account exists with that email, a verification link has been sent.", null);
            
        }
        public async Task<APIResponse<object>> ResetPasswordAsync(ResetPasswordDto resetPassword)
        {
            var user = await _userManager.FindByEmailAsync(resetPassword.Email);
            if (user == null)
            {
                _logger.LogInformation($"User with email {resetPassword.Email} does not exist in the system");
                return APIResponse<object>.Create(HttpStatusCode.NotFound, "User not found", null);
            }
            var result = await _userManager.ResetPasswordAsync(user, resetPassword.Token, resetPassword.Password);
            if (!result.Succeeded)
            {
                _logger.LogError($"An error occurred when trying to reset user password: {result.Errors.FirstOrDefault().Description}");
                return APIResponse<object>.Create(HttpStatusCode.BadRequest, "An error occurred when trying to reset user password", null);
            }

            // Password reset event
            var passwordUpdateNotification = new NotificationRequest
            {
                Channels = new[] { NotificationChannel.Email },
                Type = NotificationType.PasswordChanged,
                RecipientEmail = resetPassword.Email,
                RecipientName = $"{user.FirstName} {user.LastName}",
                Data = new Dictionary<string, string>
                {
                    { "FirstName", user.FirstName },
                    { "DatePublished", DateTime.UtcNow.ToString("o") }
                }
            };
            BackgroundJob.Enqueue(() => _notificationService.SendAsync(passwordUpdateNotification));
            return APIResponse<object>.Create(HttpStatusCode.OK, "User password reset successfully", null);

        }
        public async Task<APIResponse<object>> UpdateUserAsync(UpdateUserDto updateUser)
        {
                var user = await _userManager.FindByEmailAsync(updateUser.Email);
                if (user == null)
                {
                    _logger.LogInformation($"User with email {updateUser.Email} does not exist in the system");
                    return APIResponse<object>.Create(HttpStatusCode.NotFound, "User not found", null);
                }
                user.FirstName = updateUser.FirstName;
                user.LastName = updateUser.LastName;
                user.PhoneNumber = updateUser.PhoneNumber;
                user.ModifiedDate = DateTimeOffset.UtcNow;
                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    _logger.LogError($"An error occurred when trying to update user: {result.Errors.FirstOrDefault()!.Description}");
                    return APIResponse<object>.Create(HttpStatusCode.BadRequest, "An error occurred when trying to update user", null);
                }
                return APIResponse<object>.Create(HttpStatusCode.OK, "User updated successfully", null);
        }
             public async Task<APIResponse<TokenDto>> Verify2FA(string email, string token)
        {
            
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    _logger.LogInformation($"User with email {email} does not exist in the system");
                    return APIResponse<TokenDto>.Create(HttpStatusCode.NotFound, "User not found", null);
                }
                var result = await _userManager.VerifyTwoFactorTokenAsync(user, "Email", token);
                if (!result)
                {
                    _logger.LogError($"An error occurred when trying to verify 2FA: {result}");
                    return APIResponse<TokenDto>.Create(HttpStatusCode.BadRequest, "An error occurred when trying to verify 2FA", null);
                }

                var userToken = await _tokenService.CreateToken(false, user);
                _logger.LogInformation("Token generated for user {email}: {token}", user.Email, token);
                return APIResponse<TokenDto>.Create(HttpStatusCode.OK, "2FA verified successfully", userToken);
            }
            
        public async Task<APIResponse<object>> VerifyEmailAsync(string email, string token)
        {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    _logger.LogInformation($"User with email {email} does not exist in the system");
                    return APIResponse<object>.Create(HttpStatusCode.NotFound, "User not found", null);
                }
                if (user.EmailConfirmed)
                {
                    _logger.LogInformation($"User with email {email} has already been verified");
                    return APIResponse<object>.Create(HttpStatusCode.BadRequest, "User email has already been verified", null);
                }
                var decodedTokenBytes = WebEncoders.Base64UrlDecode(token);
                var decodedToken = Encoding.UTF8.GetString(decodedTokenBytes);
                    var result = await _userManager.ConfirmEmailAsync(user, decodedToken);
                    if (!result.Succeeded)
                    {
                        _logger.LogError($"An error occurred when trying to verify user email: {result.Errors.FirstOrDefault()!.Description}");
                        return APIResponse<object>.Create(HttpStatusCode.BadRequest, "An error occurred when trying to verify user email", null);
                }


                var accountVerificationNotification = new NotificationRequest
                {
                    Channels = new[] { NotificationChannel.Email },
                    Type = NotificationType.AccountVerification,
                    RecipientEmail = email,
                    RecipientName = $"{user.FirstName} {user.LastName}",
                    Data = new Dictionary<string, string>
                    {
                        { "FirstName", user.FirstName },
                        { "DatePublished", DateTime.UtcNow.ToString("o") }
                    }
                };

                BackgroundJob.Enqueue(() => _notificationService.SendAsync(accountVerificationNotification));
                return APIResponse<object>.Create(HttpStatusCode.OK, "User email verified successfully", null);
           
        }

        #region
        private async Task<string> GenerateOTPForTwoFactor(ApplicationUser user)
        {
            var providers = await _userManager.GetValidTwoFactorProvidersAsync(user);
            if (!providers.Contains("Email"))
            {
                throw new Exception("Email is not a valid two factor provider");
            }
            var token = await _userManager.GenerateTwoFactorTokenAsync(user, "Email");
            return token;


        }
        #endregion
    }
}
    
