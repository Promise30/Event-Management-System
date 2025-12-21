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
    public class TicketService : ITicketService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        public readonly ILogger<TicketService> _logger;
        private readonly IDatabaseRepository<Ticket, Guid> _databaseRepository;
        public TicketService(ApplicationDbContext dbContext, IMapper mapper, UserManager<ApplicationUser> userManager, ILogger<TicketService> logger, IDatabaseRepository<Ticket, Guid> databaseRepository)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _userManager = userManager;
            _logger = logger;
            _databaseRepository = databaseRepository;
        }
        public async Task<APIResponse<TicketDto>> GetTicketByIdAsync(Guid ticketId)
        {
            var ticket = await _dbContext.Tickets.Include(t => t.TicketType).FirstOrDefaultAsync(t => t.Id == ticketId);
            if (ticket == null)
            {
                return APIResponse<TicketDto>.Create(HttpStatusCode.NotFound, "Ticket not found", null, new Error { Message = "Ticket not found" });
            }
            var ticketDto = _mapper.Map<TicketDto>(ticket);
            //    new TicketDto
            //{
            //    Id = ticket.Id,
            //    AttendeeId = ticket.AttendeeId,
            //    TicketType = ticket.TicketType.Name,
            //    TicketNumber = ticket.TicketNumber,
            //    TicketStatus = ticket.Status.GetEnumDescription(),
            //    DateCreated = ticket.CreatedDate
            //};
            return APIResponse<TicketDto>.Create(HttpStatusCode.OK, "Ticket record retrieved successfully", ticketDto);
        }

        public async Task<APIResponse<PagedResponse<List<TicketDto>>>> GetAllTicketsAsync(RequestParameters requestParameters, SortParameters sortParameters, string? searchTerm, Guid eventId)
        {
            var eventEntity = await _dbContext.Events.FirstOrDefaultAsync(e => e.Id == eventId);
            if (eventEntity == null)
            {
                return APIResponse<PagedResponse<List<TicketDto>>>.Create(HttpStatusCode.NotFound, "Event not found", null, new Error { Message = "Event not found" });
            }
            var ticketsQuery = _dbContext.Tickets.Include(t => t.TicketType).Where(t => t.TicketType.EventId == eventId).AsQueryable();
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.Trim().ToLower();
                ticketsQuery = ticketsQuery.Where(t => t.TicketNumber.ToLower().Contains(searchTerm) || t.TicketType.Name.ToLower().Contains(searchTerm));
            }
            if (sortParameters != null && !string.IsNullOrEmpty(sortParameters.SortMember))
            {
                switch (sortParameters.SortMember.ToLower())
                {
                    case "ticketnumber":
                        ticketsQuery = sortParameters.SortType.GetEnumDescription() == "desc" ? ticketsQuery.OrderByDescending(t => t.TicketNumber) : ticketsQuery.OrderBy(t => t.TicketNumber);
                        break;
                    case "tickettype":
                        ticketsQuery = sortParameters.SortType.GetEnumDescription() == "desc" ? ticketsQuery.OrderByDescending(t => t.TicketType.Name) : ticketsQuery.OrderBy(t => t.TicketType.Name);
                        break;
                    case "datecreated":
                        ticketsQuery = sortParameters.SortType.GetEnumDescription() == "desc" ? ticketsQuery.OrderByDescending(t => t.CreatedDate) : ticketsQuery.OrderBy(t => t.CreatedDate);
                        break;
                    default:
                        ticketsQuery = ticketsQuery.OrderByDescending(t => t.CreatedDate);
                        break;
                }
            }
            else
            {
                ticketsQuery = ticketsQuery.OrderByDescending(t => t.CreatedDate);
            }
            var tickets = await _databaseRepository.GetAllPaginatedAsync(requestParameters, ticketsQuery.ToList());
            if (tickets == null || !tickets.Data.Any())
            {
                return APIResponse<PagedResponse<List<TicketDto>>>.Create(HttpStatusCode.OK, "No tickets found for the provided event", new PagedResponse<List<TicketDto>>());
            }
            var ticketList = _mapper.Map<List<TicketDto>>(tickets);
            //    tickets.Data.Select(t => new TicketDto
            //{
            //    Id = t.Id,
            //    AttendeeId = t.AttendeeId,
            //    TicketType = t.TicketType.Name,
            //    TicketStatus = t.Status.GetEnumDescription(),
            //    DateCreated = t.CreatedDate,
            //    TicketNumber = t.TicketNumber
            //}).ToList();
            var result = new PagedResponse<List<TicketDto>>
            {
                Data = ticketList,
                NextPage = tickets.MetaData.HasNext ? tickets.MetaData.CurrentPage + 1 : null,
                PreviousPage = tickets.MetaData.HasPrevious ? tickets.MetaData.CurrentPage - 1 : null,
                TotalPages = tickets.MetaData.TotalPages,
                PageNumber = tickets.MetaData.CurrentPage,
                PageSize = tickets.MetaData.PageSize,
                TotalNoItems = tickets.MetaData.TotalCount
            };
            return APIResponse<PagedResponse<List<TicketDto>>>.Create(HttpStatusCode.OK, "Tickets record retrieved successfully", result);
        }

        public async Task<APIResponse<List<TicketDto>>> GetTicketsByAttendeeIdAsync(Guid attendeeId)
        {
            var attendee = await _userManager.FindByIdAsync(attendeeId.ToString());
            if (attendee == null)
            {
                return APIResponse<List<TicketDto>>.Create(HttpStatusCode.NotFound, "Attendee not found", null, new Error { Message = "Attendee not found" });
            }
            var tickets = await _dbContext.Tickets.Include(t => t.TicketType).Where(t => t.AttendeeId == attendeeId).ToListAsync();
            if (tickets == null || !tickets.Any())
            {
                return APIResponse<List<TicketDto>>.Create(HttpStatusCode.OK, "No tickets found for the provided attendee", new List<TicketDto>());
            }
            var ticketList = _mapper.Map<List<TicketDto>>(tickets);
            return APIResponse<List<TicketDto>>.Create(HttpStatusCode.OK, "Tickets record retrieved successfully", ticketList);
        }

        public async Task<APIResponse<object>> CancelTicketAsync(Guid userId, Guid ticketId)
        {
            
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return APIResponse<object>.Create(HttpStatusCode.NotFound, "User not found", null, new Error { Message = "User not found" });
            }
            var ticket = await _dbContext.Tickets.FirstOrDefaultAsync(t => t.Id == ticketId);
            if (ticket == null)
            {
                return APIResponse<object>.Create(HttpStatusCode.NotFound, "Ticket not found", null, new Error { Message = "Ticket not found" });
            }
            if (ticket.Status == TicketStatus.Cancelled)
            {
                return APIResponse<object>.Create(HttpStatusCode.BadRequest, "Ticket is already cancelled", null, new Error { Message = "Ticket is already cancelled" });
            }
            ticket.Status = TicketStatus.Cancelled;
            ticket.ModifiedDate = DateTimeOffset.UtcNow;
            _dbContext.Tickets.Update(ticket);

            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                ActionType = ActionType.Delete,
                ObjectId = ticket.Id,
                UserId = user.Id,
                Description = $"Ticket {ticket.TicketNumber} cancelled by user {user.UserName} at {DateTimeOffset.UtcNow}",
                CreatedDate = DateTimeOffset.UtcNow,
                ModifiedDate = DateTimeOffset.UtcNow
            };
            _dbContext.AuditLogs.Add(auditLog);
            await _dbContext.SaveChangesAsync();

            return APIResponse<object>.Create(HttpStatusCode.NoContent, "Ticket cancelled successfully", null);
        }

        public async Task<APIResponse<object>> CreateTicketAsync(Guid userId, CreateTicketDto createTicketDto)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return APIResponse<object>.Create(HttpStatusCode.NotFound, "User not found", null, new Error { Message = "User not found" });
            }
            var ticketType = await _dbContext.TicketTypes.Include(tt => tt.Event).FirstOrDefaultAsync(tt => tt.Id == createTicketDto.TicketTypeId && tt.EventId == createTicketDto.EventId);
            if (ticketType == null)
            {
                return APIResponse<object>.Create(HttpStatusCode.NotFound, "Ticket type not found for the provided event", null, new Error { Message = "Ticket type not found for the provided event" });
            }
            // check if there are still tickets available
            var issuedTicketsCount = await _dbContext.Tickets.CountAsync(t => t.TicketTypeId == ticketType.Id && t.Status == TicketStatus.Active);
            if (issuedTicketsCount >= ticketType.Capacity)
            {
                return APIResponse<object>.Create(HttpStatusCode.BadRequest, "No tickets available for the selected ticket type", null, new Error { Message = "No tickets available for the selected ticket type" });
            }
            // check if user already has a ticket for the event
            var existingTicket = await _dbContext.Tickets.Include(t => t.TicketType).FirstOrDefaultAsync(t => t.AttendeeId == user.Id && t.TicketType.EventId == createTicketDto.EventId && t.Status == TicketStatus.Active);
            if (existingTicket != null)
            {
                return APIResponse<object>.Create(HttpStatusCode.BadRequest, "User already has an active ticket for this event", null, new Error { Message = "User already has an active ticket for this event" });
            }

            var ticket = new Ticket
            {
                Id = Guid.NewGuid(),
                AttendeeId = user.Id,
                TicketTypeId = ticketType.Id,
                TicketNumber = $"TCKT-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}",
                Status = ticketType.Price > 0 ? TicketStatus.Reserved : TicketStatus.Active,
                CreatedDate = DateTimeOffset.UtcNow,
                ModifiedDate = DateTimeOffset.UtcNow
            };
            await _dbContext.Tickets.AddAsync(ticket);
            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                ActionType = ActionType.Create,
                ObjectId = ticket.Id,
                UserId = user.Id,
                Description = $"Ticket {ticket.TicketNumber} created by user {user.UserName}",
                CreatedDate = DateTimeOffset.UtcNow,
                ModifiedDate = DateTimeOffset.UtcNow
            };
            _dbContext.AuditLogs.Add(auditLog);
            await _dbContext.SaveChangesAsync();

            if(ticketType.Price > 0)
            {
                // Simulate payment processing for paid tickets
                // In real scenario, integrate with payment gateway
            /*// Initiate payment
            var transactionRequest = new TransactionInitializeRequest
            {
                Email = checkoutDto.Email,
                Reference = Guid.NewGuid().ToString(),
                CallbackUrl = string.Concat(_configuration["PayStack:CallbackUrl"], "/api/order/verify"),
                AmountInKobo = (int)(totalAmount * 100),
                Currency = "NGN",
                Channels = new[] { checkoutDto.PaymentMethod },
            };

            var transactionResponse = _payStackApi.Transactions.Initialize(transactionRequest);

            if (!transactionResponse.Status)
            {
                return APIResponse<object>.Create(HttpStatusCode.InternalServerError, "An error occurred while trying to initiate payment", null);
            }

            order.PaymentReference = transactionResponse.Data.Reference;
            await _orderRepository.CreateAsync(order);
            await _orderRepository.SaveChangesAsync();

            return APIResponse<object>.Create(HttpStatusCode.OK, "Successful", new PaymentInitializationResponse
            {
                AuthorizationUrl = transactionResponse.Data.AuthorizationUrl,
                PaymentReference = transactionResponse.Data.Reference
            });
             */
            ticket.Status = TicketStatus.Active;
            ticket.PaymentReference = $"PAY-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
            ticket.ModifiedDate = DateTimeOffset.UtcNow;
            
        }
        return APIResponse<object>.Create(HttpStatusCode.Created, "Ticket created successfully", null);
        }
    }
}
