using Event_Management_System.API.Application.Interfaces;
using Event_Management_System.API.Domain.DTOs.User;
using Event_Management_System.API.Domain.Entities;
using Event_Management_System.API.Domain.Enums;
using Event_Management_System.API.Helpers;
using Event_Management_System.API.Infrastructures;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Net;
using Event_Management_System.API.Domain.DTOs;
using Hangfire;

namespace Event_Management_System.API.Application.Implementation
{
    public class OrganizerRequestService : IOrganizerRequestService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly ILogger<OrganizerRequestService> _logger;
        private readonly INotificationService _notificationService;

        public OrganizerRequestService(
            ApplicationDbContext dbContext,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole<Guid>> roleManager,
            ILogger<OrganizerRequestService> logger,
            INotificationService notificationService)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
            _notificationService = notificationService;
        }

        /// <inheritdoc/>
        public async Task<APIResponse<OrganizerRequestDto>> SubmitRequestAsync(Guid userId, CreateOrganizerRequestDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return APIResponse<OrganizerRequestDto>.Create(HttpStatusCode.NotFound, "User not found", null, new Error { Message = "User not found" });

            // Ensure the user currently has the "User" role
            var isUser = await _userManager.IsInRoleAsync(user, "User");
            if (!isUser)
            {
                // User is already an Organizer or Admin
                var roles = await _userManager.GetRolesAsync(user);
                return APIResponse<OrganizerRequestDto>.Create(
                    HttpStatusCode.BadRequest,
                    $"Only users with the 'User' role can request to become an Organizer. Your current role: {string.Join(", ", roles)}",
                    null,
                    new Error { Message = "User is not eligible to submit an organizer request" });
            }

            // Check if the user already has a pending request
            var existingRequest = await _dbContext.OrganizerRequests
                .FirstOrDefaultAsync(r => r.UserId == userId && r.Status == OrganizerRequestStatus.Pending);

            if (existingRequest != null)
                return APIResponse<OrganizerRequestDto>.Create(
                    HttpStatusCode.BadRequest,
                    "You already have a pending organizer request",
                    null,
                    new Error { Message = "A pending organizer request already exists for this user" });

            var request = new OrganizerRequest
            {
                UserId = userId,
                Reason = dto.Reason,
                Status = OrganizerRequestStatus.Pending,
                CreatedDate = DateTimeOffset.UtcNow,
                ModifiedDate = DateTimeOffset.UtcNow
            };

            _dbContext.OrganizerRequests.Add(request);

            var auditLog = new AuditLog
            {
                UserId = userId,
                ObjectId = request.Id,
                ActionType = ActionType.Create,
                Description = $"User {user.Email} submitted an organizer request at {DateTimeOffset.UtcNow}",
                CreatedDate = DateTimeOffset.UtcNow,
                ModifiedDate = DateTimeOffset.UtcNow
            };
            _dbContext.AuditLogs.Add(auditLog);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("User {Email} submitted an organizer request {RequestId}", user.Email, request.Id);

            var response = MapToDto(request, user);
            return APIResponse<OrganizerRequestDto>.Create(HttpStatusCode.Created, "Organizer request submitted successfully", response);
        }

        /// <inheritdoc/>
        public async Task<APIResponse<IEnumerable<OrganizerRequestDto>>> GetPendingRequestsAsync()
        {
            var requests = await _dbContext.OrganizerRequests
                .Include(r => r.User)
                .Where(r => r.Status == OrganizerRequestStatus.Pending)
                .OrderByDescending(r => r.CreatedDate)
                .ToListAsync();

            var response = requests.Select(r => MapToDto(r, r.User)).ToList();

            return APIResponse<IEnumerable<OrganizerRequestDto>>.Create(
                HttpStatusCode.OK,
                "Pending organizer requests retrieved successfully",
                response);
        }

        /// <inheritdoc/>
        public async Task<APIResponse<OrganizerRequestDto>> ApproveRequestAsync(Guid adminUserId, Guid requestId, ReviewOrganizerRequestDto dto)
        {
            var request = await _dbContext.OrganizerRequests
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == requestId);

            if (request == null)
                return APIResponse<OrganizerRequestDto>.Create(HttpStatusCode.NotFound, "Organizer request not found", null, new Error { Message = "Organizer request not found" });

            if (request.Status != OrganizerRequestStatus.Pending)
                return APIResponse<OrganizerRequestDto>.Create(
                    HttpStatusCode.BadRequest,
                    $"This request has already been {request.Status.GetEnumDescription().ToLower()}",
                    null,
                    new Error { Message = "Request is not in a pending state" });

            var user = request.User;

            // Ensure the Organizer role exists
            if (!await _roleManager.RoleExistsAsync("Organizer"))
            {
                await _roleManager.CreateAsync(new IdentityRole<Guid>("Organizer"));
            }

            // Remove the User role and add the Organizer role
            var removeResult = await _userManager.RemoveFromRoleAsync(user, "User");
            if (!removeResult.Succeeded)
            {
                _logger.LogError("Failed to remove User role from {Email}: {Errors}", user.Email, string.Join(", ", removeResult.Errors.Select(e => e.Description)));
            }

            var addResult = await _userManager.AddToRoleAsync(user, "Organizer");
            if (!addResult.Succeeded)
            {
                _logger.LogError("Failed to add Organizer role to {Email}: {Errors}", user.Email, string.Join(", ", addResult.Errors.Select(e => e.Description)));
                return APIResponse<OrganizerRequestDto>.Create(
                    HttpStatusCode.InternalServerError,
                    "Failed to assign Organizer role",
                    null,
                    new Error { Message = string.Join(", ", addResult.Errors.Select(e => e.Description)) });
            }

            // Update the request
            request.Status = OrganizerRequestStatus.Approved;
            request.ReviewedByAdminId = adminUserId;
            request.AdminNote = dto.AdminNote;
            request.ReviewedAt = DateTimeOffset.UtcNow;
            request.ModifiedDate = DateTimeOffset.UtcNow;

            var auditLog = new AuditLog
            {
                UserId = adminUserId,
                ObjectId = request.Id,
                ActionType = ActionType.Update,
                Description = $"Admin approved organizer request for user {user.Email} at {DateTimeOffset.UtcNow}",
                CreatedDate = DateTimeOffset.UtcNow,
                ModifiedDate = DateTimeOffset.UtcNow
            };
            _dbContext.AuditLogs.Add(auditLog);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Admin {AdminId} approved organizer request {RequestId} for user {Email}", adminUserId, requestId, user.Email);

            // Send organizer request approval notification
            var approvalNotification = new NotificationRequest
            {
                Channels = new[] { NotificationChannel.Email },
                Type = NotificationType.OrganizerRequestApproved,
                RecipientEmail = user.Email,
                RecipientName = $"{user.FirstName} {user.LastName}",
                Data = new Dictionary<string, string>
                {
                    { "FirstName", user.FirstName }
                }
            };
            BackgroundJob.Enqueue(() => _notificationService.SendAsync(approvalNotification));

            var response = MapToDto(request, user);
            return APIResponse<OrganizerRequestDto>.Create(HttpStatusCode.OK, "Organizer request approved successfully", response);
        }

        /// <inheritdoc/>
        public async Task<APIResponse<OrganizerRequestDto>> RejectRequestAsync(Guid adminUserId, Guid requestId, ReviewOrganizerRequestDto dto)
        {
            var request = await _dbContext.OrganizerRequests
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == requestId);

            if (request == null)
                return APIResponse<OrganizerRequestDto>.Create(HttpStatusCode.NotFound, "Organizer request not found", null, new Error { Message = "Organizer request not found" });

            if (request.Status != OrganizerRequestStatus.Pending)
                return APIResponse<OrganizerRequestDto>.Create(
                    HttpStatusCode.BadRequest,
                    $"This request has already been {request.Status.GetEnumDescription().ToLower()}",
                    null,
                    new Error { Message = "Request is not in a pending state" });

            // Update the request
            request.Status = OrganizerRequestStatus.Rejected;
            request.ReviewedByAdminId = adminUserId;
            request.AdminNote = dto.AdminNote;
            request.ReviewedAt = DateTimeOffset.UtcNow;
            request.ModifiedDate = DateTimeOffset.UtcNow;

            var auditLog = new AuditLog
            {
                UserId = adminUserId,
                ObjectId = request.Id,
                ActionType = ActionType.Update,
                Description = $"Admin rejected organizer request for user {request.User.Email} at {DateTimeOffset.UtcNow}",
                CreatedDate = DateTimeOffset.UtcNow,
                ModifiedDate = DateTimeOffset.UtcNow
            };
            _dbContext.AuditLogs.Add(auditLog);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Admin {AdminId} rejected organizer request {RequestId} for user {Email}", adminUserId, requestId, request.User.Email);

            // Send organizer request rejection notification
            var rejectionNotification = new NotificationRequest
            {
                Channels = new[] { NotificationChannel.Email },
                Type = NotificationType.OrganizerRequestRejected,
                RecipientEmail = request.User.Email,
                RecipientName = $"{request.User.FirstName} {request.User.LastName}",
                Data = new Dictionary<string, string>
                {
                    { "FirstName", request.User.FirstName },
                    { "Reason", dto.AdminNote ?? "Your request did not meet the requirements" }
                }
            };
            BackgroundJob.Enqueue(() => _notificationService.SendAsync(rejectionNotification));

            var response = MapToDto(request, request.User);
            return APIResponse<OrganizerRequestDto>.Create(HttpStatusCode.OK, "Organizer request rejected", response);
        }

        #region Private helpers

        private static OrganizerRequestDto MapToDto(OrganizerRequest request, ApplicationUser user)
        {
            return new OrganizerRequestDto
            {
                Id = request.Id,
                UserId = request.UserId,
                UserName = $"{user.FirstName} {user.LastName}",
                UserEmail = user.Email ?? string.Empty,
                Reason = request.Reason,
                Status = request.Status.GetEnumDescription(),
                AdminNote = request.AdminNote,
                ReviewedByAdminId = request.ReviewedByAdminId,
                ReviewedAt = request.ReviewedAt,
                CreatedDate = request.CreatedDate
            };
        }

        #endregion
    }
}
