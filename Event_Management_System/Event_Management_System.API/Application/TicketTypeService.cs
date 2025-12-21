using AutoMapper;
using Event_Management_System.API.Domain.DTOs;
using Event_Management_System.API.Domain.Entities;
using Event_Management_System.API.Domain.Enums;
using Event_Management_System.API.Helpers;
using Event_Management_System.API.Infrastructures;
using Event_Management_System.API.Infrastructures.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Event_Management_System.API.Application
{
    public class TicketTypeService : ITicketTypeService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        public readonly ILogger<TicketTypeService> _logger;
        private readonly IDatabaseRepository<TicketType, Guid> _databaseRepository;
        public TicketTypeService(ApplicationDbContext dbContext, IMapper mapper, UserManager<ApplicationUser> userManager, ILogger<TicketTypeService> logger, IDatabaseRepository<TicketType, Guid> databaseRepository)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _userManager = userManager;
            _logger = logger;
            _databaseRepository = databaseRepository;
        }

        public async Task<APIResponse<object>> CreateTicketTypeAsync(Guid userId, CreateTicketTypeDto createTicketTypeDto)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return APIResponse<object>.Create(HttpStatusCode.NotFound, "User not found", null, new Error { Message = "User not found" });
            }
            // ✅ Step 1: Validate Event exists and load its Booking
            var eventEntity = await _dbContext.Events
                .Include(e => e.Booking)
                    .ThenInclude(b => b.EventCentre)
                .Include(e => e.TicketTypes)
                .FirstOrDefaultAsync(e => e.Id == createTicketTypeDto.EventId);

            // var eventEntity = await _dbContext.Events.Include(b => b.Booking).FirstOrDefaultAsync(e => e.Equals(createTicketTypeDto.EventId));
            if (eventEntity == null)
            {
                return APIResponse<object>.Create(HttpStatusCode.NotFound, "Event not found", null, new Error { Message = "Event not found" });
            }
            // Validate that the event has an associated booking
            if (eventEntity.Booking == null)
                return APIResponse<object>.Create(HttpStatusCode.BadRequest, "Cannot create tickets for an event without a venue booking", null, new Error { Message = "Event must have a confirmed booking before creating ticket types" });
            if (eventEntity.Booking.BookingStatus != BookingStatus.Confirmed)
                return APIResponse<object>.Create(HttpStatusCode.BadRequest,
                    $"Cannot create tickets. Booking status is '{eventEntity.Booking.BookingStatus.GetEnumDescription()}'. Only confirmed bookings can have ticket types.",
                    null,
                    new Error { Message = $"Booking must be confirmed before creating tickets. Current status: {eventEntity.Booking.BookingStatus.GetEnumDescription()}" });


            // check if user is the organizer of the event
            if (eventEntity.Booking.OrganizerId != userId)
            {
                return APIResponse<object>.Create(HttpStatusCode.Forbidden, "You are not authorized to create ticket types for this event", null, new Error { Message = "You are not authorized to create ticket types for this event" });
            }

            // check if ticket type with the same name already exists for the event
            var existingTicketType = await _dbContext.TicketTypes.FirstOrDefaultAsync(tt => tt.EventId == createTicketTypeDto.EventId && tt.Name.ToLower() == createTicketTypeDto.Name.ToLower());
            if (existingTicketType != null)
            {
                return APIResponse<object>.Create(HttpStatusCode.Conflict, "Ticket type with the same name already exists for this event", null, new Error { Message = "Ticket type with the same name already exists for this event" });
            }

            var ticketTypeToCreate = new TicketType
            {
                Name = createTicketTypeDto.Name,
                Description = createTicketTypeDto.Description,
                Price = createTicketTypeDto.Price,
                Capacity = createTicketTypeDto.Capacity,
                IsActive = createTicketTypeDto.IsActive,
                EventId = createTicketTypeDto.EventId,
                CreatedDate = DateTimeOffset.UtcNow,
                ModifiedDate = DateTimeOffset.UtcNow,
            };
            _dbContext.TicketTypes.Add(ticketTypeToCreate);

            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ActionType = ActionType.Create,
                ObjectId = ticketTypeToCreate.Id,
                CreatedDate = DateTimeOffset.UtcNow,
                ModifiedDate = DateTimeOffset.UtcNow,
                Description = $"Ticket Type '{ticketTypeToCreate.Name}' created for Event '{eventEntity.Title}' by User '{user.UserName}'"
            };
            _dbContext.AuditLogs.Add(auditLog);
            await _dbContext.SaveChangesAsync();
            var ticketTypeDto = _mapper.Map<TicketTypeDto>(ticketTypeToCreate);
            return APIResponse<object>.Create(HttpStatusCode.Created, "Ticket type created successfully", ticketTypeDto);
        }

        public async Task<APIResponse<PagedResponse<List<TicketTypeDto>>>> GetAllTicketTypesAsync(RequestParameters requestParameters, SortParameters? sortParameters, string? searchTerm, Guid eventId)
        {
            var eventEntity = await _dbContext.Events.FirstOrDefaultAsync(e => e.Id == eventId);
            if (eventEntity == null)
            {
                return APIResponse<PagedResponse<List<TicketTypeDto>>>.Create(HttpStatusCode.NotFound, "Event not found", null, new Error { Message = "Event not found" });
            }
            var ticketTypesQuery = _dbContext.TicketTypes.Where(tt => tt.EventId == eventId).AsQueryable();
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.Trim().ToLower();
                ticketTypesQuery = ticketTypesQuery.Where(tt => tt.Name.ToLower().Contains(searchTerm) || (tt.Description != null && tt.Description.ToLower().Contains(searchTerm)));
            }
            if (sortParameters != null && !string.IsNullOrEmpty(sortParameters.SortMember))
            {
                switch (sortParameters.SortMember.ToLower())
                {
                    case "name":
                        ticketTypesQuery = sortParameters.SortType.GetEnumDescription() == "desc" ? ticketTypesQuery.OrderByDescending(tt => tt.Name) : ticketTypesQuery.OrderBy(tt => tt.Name);
                        break;
                    case "price":
                        ticketTypesQuery = sortParameters.SortType.GetEnumDescription() == "desc" ? ticketTypesQuery.OrderByDescending(tt => tt.Price) : ticketTypesQuery.OrderBy(tt => tt.Price);
                        break;
                    case "capacity":
                        ticketTypesQuery = sortParameters.SortType.GetEnumDescription() == "desc" ? ticketTypesQuery.OrderByDescending(tt => tt.Capacity) : ticketTypesQuery.OrderBy(tt => tt.Capacity);
                        break;
                    default:
                        ticketTypesQuery = ticketTypesQuery.OrderBy(tt => tt.Name);
                        break;
                }
            }
            else
            {
                ticketTypesQuery = ticketTypesQuery.OrderBy(tt => tt.Name);
            }
            var ticketTypes = await _databaseRepository.GetAllPaginatedAsync(requestParameters, ticketTypesQuery.ToList());
            if (ticketTypes == null || !ticketTypes.Data.Any())
            {
                return APIResponse<PagedResponse<List<TicketTypeDto>>>.Create(HttpStatusCode.OK, "No ticket types found for the provided event", new PagedResponse<List<TicketTypeDto>>());
            }

            var ticketTypesList = ticketTypes.Data.Select(tt => new TicketTypeDto
            {
                Id = tt.Id,
                Name = tt.Name,
                Description = tt.Description,
                Capacity = tt.Capacity,
                EventId = tt.EventId,
                IsActive = tt.IsActive,
                Price = tt.Price,
                DateCreated = tt.CreatedDate
            }).ToList();
            var result = new PagedResponse<List<TicketTypeDto>>
            {
                Data = ticketTypesList,
                NextPage = ticketTypes.MetaData.HasNext ? ticketTypes.MetaData.CurrentPage + 1 : null,
                PreviousPage = ticketTypes.MetaData.HasPrevious ? ticketTypes.MetaData.CurrentPage - 1 : null,
                TotalPages = ticketTypes.MetaData.TotalPages,
                PageNumber = ticketTypes.MetaData.CurrentPage,
                PageSize = ticketTypes.MetaData.PageSize,
                TotalNoItems = ticketTypes.MetaData.TotalCount
            };
            return APIResponse<PagedResponse<List<TicketTypeDto>>>.Create(HttpStatusCode.OK, "Ticket types record retrieved successfully", result);
        }

        public async Task<APIResponse<object>> DeleteTicketTypeAsync(Guid userId, Guid ticketTypeId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return APIResponse<object>.Create(HttpStatusCode.NotFound, "User not found", null, new Error { Message = "User not found" });

            }
            var ticketType = await _dbContext.TicketTypes.Include(tt => tt.Event).ThenInclude(b => b.Booking).FirstOrDefaultAsync(tt => tt.Id == ticketTypeId);
            if (ticketType == null)
            {
                return APIResponse<object>.Create(HttpStatusCode.NotFound, "Ticket type not found", null, new Error { Message = "Ticket type not found" });
            }

            if (ticketType.Event.Booking.OrganizerId != userId)
            {
                return APIResponse<object>.Create(HttpStatusCode.Forbidden, "You are not authorized to delete this ticket type", null, new Error { Message = "You are not authorized to delete this ticket type" });
            }
            var associatedTickets = await _dbContext.Tickets.AnyAsync(t => t.TicketTypeId == ticketTypeId);
            if (associatedTickets)
            {
                return APIResponse<object>.Create(HttpStatusCode.Conflict, "Cannot delete ticket type with associated tickets", null, new Error { Message = "Cannot delete ticket type with associated tickets" });
            }
            _dbContext.TicketTypes.Remove(ticketType);
            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ActionType = ActionType.Delete,
                ObjectId = ticketType.Id,
                CreatedDate = DateTimeOffset.UtcNow,
                ModifiedDate = DateTimeOffset.UtcNow,
                Description = $"Ticket Type '{ticketType.Name}' for Event '{ticketType.Event.Title}' deleted by User '{user.UserName}'"
            };
            _dbContext.AuditLogs.Add(auditLog);
            await _dbContext.SaveChangesAsync();
            return APIResponse<object>.Create(HttpStatusCode.OK, "Ticket type deleted successfully", null);


        }

        public async Task<APIResponse<object>> UpdateTicketTypeAsync(Guid userId, Guid ticketTypeId, CreateTicketTypeDto createTicketTypeDto)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return APIResponse<object>.Create(HttpStatusCode.NotFound, "User not found", null, new Error { Message = "User not found" });
            }
            var ticketType = await _dbContext.TicketTypes.Include(tt => tt.Event).ThenInclude(b => b.Booking).FirstOrDefaultAsync(tt => tt.Id == ticketTypeId);
            if (ticketType == null)
            {
                return APIResponse<object>.Create(HttpStatusCode.NotFound, "Ticket type not found", null, new Error { Message = "Ticket type not found" });
            }
            if (ticketType.Event.Booking.OrganizerId != userId)
            {
                return APIResponse<object>.Create(HttpStatusCode.Forbidden, "You are not authorized to update this ticket type", null, new Error { Message = "You are not authorized to update this ticket type" });
            }
            // check if user is the organizer of the event
            if (ticketType.Event.Booking.OrganizerId != userId)
            {
                return APIResponse<object>.Create(HttpStatusCode.Forbidden, "You are not authorized to update ticket types for this event", null, new Error { Message = "You are not authorized to update ticket types for this event" });
            }
            // check if ticket type with the same name already exists for the event
            var existingTicketType = await _dbContext.TicketTypes.FirstOrDefaultAsync(tt => tt.EventId == createTicketTypeDto.EventId && tt.Name.ToLower() == createTicketTypeDto.Name.ToLower() && tt.Id != ticketTypeId);
            if (existingTicketType != null)
            {
                return APIResponse<object>.Create(HttpStatusCode.Conflict, "Ticket type with the same name already exists for this event", null, new Error { Message = "Ticket type with the same name already exists for this event" });
            }
            ticketType.Name = createTicketTypeDto.Name;
            ticketType.Description = createTicketTypeDto.Description;
            ticketType.Capacity = createTicketTypeDto.Capacity;
            ticketType.IsActive = createTicketTypeDto.IsActive;
            ticketType.Price = createTicketTypeDto.Price;
            ticketType.ModifiedDate = DateTimeOffset.UtcNow;

            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ActionType = ActionType.Update,
                ObjectId = ticketType.Id,
                CreatedDate = DateTimeOffset.UtcNow,
                ModifiedDate = DateTimeOffset.UtcNow,
                Description = $"Ticket Type '{ticketType.Name}' for Event '{ticketType.Event.Title}' updated by User '{user.UserName}'"
            };
            _dbContext.AuditLogs.Add(auditLog);
            await _dbContext.SaveChangesAsync();
            return APIResponse<object>.Create(HttpStatusCode.NoContent, "Ticket type updated successfully", null);
        }

        public async Task<APIResponse<TicketTypeDto>> GetTicketTypeByIdAsync(Guid ticketTypeId)
        {
            var ticketType = await _dbContext.TicketTypes.FirstOrDefaultAsync(tt => tt.Id == ticketTypeId);
            if (ticketType == null)
            {
                return APIResponse<TicketTypeDto>.Create(HttpStatusCode.NotFound, "Ticket type not found", null, new Error { Message = "Ticket type not found" });
            }
            var ticketTypeDto = _mapper.Map<TicketTypeDto>(ticketType);
            return APIResponse<TicketTypeDto>.Create(HttpStatusCode.OK, "Ticket type record retrieved successfully", ticketTypeDto);
        }

       
    }
}
