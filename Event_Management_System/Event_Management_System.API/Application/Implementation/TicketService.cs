using AutoMapper;
using Event_Management_System.API.Application.Interfaces;
using Event_Management_System.API.Application.Payments;
using Event_Management_System.API.Domain.DTOs.Payment;
using Event_Management_System.API.Domain.DTOs.Ticket;
using Event_Management_System.API.Domain.Entities;
using Event_Management_System.API.Domain.Enums;
using Event_Management_System.API.Helpers;
using Event_Management_System.API.Infrastructures;
using Event_Management_System.API.Infrastructures.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Net;

namespace Event_Management_System.API.Application.Implementation
{
    public class TicketService : ITicketService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<TicketService> _logger;
        private readonly IDatabaseRepository<Ticket, Guid> _databaseRepository;
        private readonly IPaymentService _paymentService;

        public TicketService(
            ApplicationDbContext dbContext,
            IMapper mapper,
            UserManager<ApplicationUser> userManager,
            ILogger<TicketService> logger,
            IDatabaseRepository<Ticket, Guid> databaseRepository,
            IPaymentService paymentService)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _userManager = userManager;
            _logger = logger;
            _databaseRepository = databaseRepository;
            _paymentService = paymentService;
        }
        public async Task<APIResponse<TicketDto>> GetTicketByIdAsync(Guid ticketId)
        {
            var ticket = await _dbContext.Tickets.Include(t => t.TicketType).FirstOrDefaultAsync(t => t.Id == ticketId);
            if (ticket == null)
            {
                return APIResponse<TicketDto>.Create(HttpStatusCode.NotFound, "Ticket not found", null, new Error { Message = "Ticket not found" });
            }
            var ticketDto = _mapper.Map<TicketDto>(ticket);
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
            var tickets = await _databaseRepository.GetAllPaginatedAsync(ticketsQuery, requestParameters);
            if (tickets == null || !tickets.Data.Any())
            {
                return APIResponse<PagedResponse<List<TicketDto>>>.Create(HttpStatusCode.OK, "No tickets found for the provided event", new PagedResponse<List<TicketDto>>());
            }
            var ticketList = _mapper.Map<List<TicketDto>>(tickets.Data);
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
            var ticket = await _dbContext.Tickets
        .Include(t => t.TicketType)
        .ThenInclude(tt => tt.Event)
        .FirstOrDefaultAsync(t => t.Id == ticketId);
            if (ticket == null)
            {
                return APIResponse<object>.Create(HttpStatusCode.NotFound, "Ticket not found", null, new Error { Message = "Ticket not found" });
            }
            if(ticket.AttendeeId != userId)
            {
                return APIResponse<object>.Create(HttpStatusCode.Unauthorized, "You are not authorized to cancel this ticket", null, new Error { Message = "You are not authorized to cancel this ticket" });
            }
            if (ticket.Status == TicketStatus.Cancelled)
            {
                return APIResponse<object>.Create(HttpStatusCode.BadRequest, "Ticket is already cancelled", null, new Error { Message = "Ticket is already cancelled" });
            }
            // Event already happened
            if (ticket.TicketType.Event.EndTime < DateTimeOffset.UtcNow)
            {
                return APIResponse<object>.Create(
                    HttpStatusCode.BadRequest,
                    "Event has already occurred. Ticket cannot be cancelled.",
                    null,
                    new Error { Message = "Event already occurred" });
            }
            //// Cancellation window (example: 24 hours before event)
            //if (ticket.TicketType.Event.StartDate.AddHours(-24) < DateTimeOffset.UtcNow)
            //{
            //    return APIResponse<object>.Create(
            //        HttpStatusCode.BadRequest,
            //        "Ticket can no longer be cancelled within 24 hours of event start.",
            //        null,
            //        new Error { Message = "Cancellation window closed" });
            //}
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                ticket.Status = TicketStatus.Cancelled;

                ticket.ModifiedDate = DateTimeOffset.UtcNow;

                // Restore capacity (guard against negative SoldCount)
                await _dbContext.Database.ExecuteSqlInterpolatedAsync($@"
                    UPDATE TicketTypes
                    SET SoldCount = SoldCount - 1
                    WHERE Id = {ticket.TicketTypeId}
                    AND SoldCount > 0");

                var auditLog = new AuditLog
                {
                    Id = Guid.NewGuid(),
                    ActionType = ActionType.Delete,
                    ObjectId = ticket.Id,
                    UserId = userId,
                    Description = $"Ticket {ticket.TicketNumber} cancelled by user {userId} at {DateTimeOffset.UtcNow}",
                    CreatedDate = DateTimeOffset.UtcNow,
                    ModifiedDate = DateTimeOffset.UtcNow
                };
                _dbContext.AuditLogs.Add(auditLog);
                await _dbContext.SaveChangesAsync();

                await transaction.CommitAsync();

                return APIResponse<object>.Create(HttpStatusCode.NoContent, "Ticket cancelled successfully", null);
            }
            catch(Exception ex)
            {
                await transaction.RollbackAsync();

                _logger.LogError(ex, "Error cancelling ticket {TicketId}", ticketId);

                return APIResponse<object>.Create(
                    HttpStatusCode.InternalServerError,
                    "An error occurred while cancelling the ticket",
                    null,
                    new Error { Message = "An unexpected error occurred." });
            }
            }

    //    public async Task<APIResponse<object>> CreateTicketAsync(Guid userId, CreateTicketDto createTicketDto)
    //    {
    //        var user = await _userManager.FindByIdAsync(userId.ToString());
    //        if (user == null)
    //        {
    //            return APIResponse<object>.Create(HttpStatusCode.NotFound, "User not found", null, new Error { Message = "User not found" });
    //        }
    //        using var transaction = await _dbContext.Database.BeginTransactionAsync();

    //        try
    //        {
    //            var ticketType = await _dbContext.TicketTypes.Include(tt => tt.Event).FirstOrDefaultAsync(tt => tt.Id == createTicketDto.TicketTypeId && tt.EventId == createTicketDto.EventId);
    //            if (ticketType == null)
    //            {
    //                return APIResponse<object>.Create(HttpStatusCode.NotFound, "Ticket type not found for the provided event", null, new Error { Message = "Ticket type not found for the provided event" });
    //            }
    //            // check if there are still tickets available
    //            var issuedTicketsCount = await _dbContext.Tickets.CountAsync(t => t.TicketTypeId == ticketType.Id && t.Status != TicketStatus.Cancelled);
    //            if (issuedTicketsCount >= ticketType.Capacity)
    //            {
    //                return APIResponse<object>.Create(HttpStatusCode.BadRequest, "No tickets available for the selected ticket type", null, new Error { Message = "No tickets available for the selected ticket type" });
    //            }
    //            // check if user already has a ticket for the event (active or reserved)
    //            var existingTicket = await _dbContext.Tickets
    //                .Include(t => t.TicketType)
    //                .FirstOrDefaultAsync(t => t.AttendeeId == user.Id
    //                    && t.TicketType.EventId == createTicketDto.EventId
    //                    && (t.Status == TicketStatus.Active || t.Status == TicketStatus.Reserved));
    //            if (existingTicket != null)
    //            {
    //                return APIResponse<object>.Create(HttpStatusCode.BadRequest, "User already has an active or reserved ticket for this event", null, new Error { Message = "User already has an active or reserved ticket for this event" });
    //            }

    //            var ticket = new Ticket
    //            {
    //                Id = Guid.NewGuid(),
    //                AttendeeId = user.Id,
    //                TicketTypeId = ticketType.Id,
    //                TicketNumber = $"TCKT-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}",
    //                Status = ticketType.Price > 0 ? TicketStatus.Reserved : TicketStatus.Active,
    //                CreatedDate = DateTimeOffset.UtcNow,
    //                ModifiedDate = DateTimeOffset.UtcNow
    //            };
    //            await _dbContext.Tickets.AddAsync(ticket);
    //            var auditLog = new AuditLog
    //            {
    //                Id = Guid.NewGuid(),
    //                ActionType = ActionType.Create,
    //                ObjectId = ticket.Id,
    //                UserId = user.Id,
    //                Description = $"Ticket {ticket.TicketNumber} created by user {user.UserName}",
    //                CreatedDate = DateTimeOffset.UtcNow,
    //                ModifiedDate = DateTimeOffset.UtcNow
    //            };
    //            _dbContext.AuditLogs.Add(auditLog);
    //            await _dbContext.SaveChangesAsync();

    //            // For paid tickets, initiate Paystack payment
    //            if (ticketType.Price > 0)
    //            {
    //                var initiatePaymentDto = new InitiatePaymentDto
    //                {
    //                    UserId = user.Id,
    //                    Email = user.Email!,
    //                    FirstName = user.FirstName,
    //                    LastName = user.LastName,
    //                    PhoneNumber = user.PhoneNumber ?? string.Empty,
    //                    Amount = ticketType.Price,
    //                    Currency = "NGN",
    //                    Description = $"Payment for {ticketType.Name} ticket - {ticketType.Event.Title}",
    //                    PaymentType = PaymentType.Ticket,
    //                    ReferenceId = ticket.Id,
    //                    Metadata = new Dictionary<string, string>
    //                {
    //                    { "ticket_id", ticket.Id.ToString() },
    //                    { "ticket_number", ticket.TicketNumber },
    //                    { "event_id", ticketType.EventId.ToString() },
    //                    { "ticket_type", ticketType.Name }
    //                }
    //                };

    //                var paymentResult = await _paymentService.InitializePaymentAsync(initiatePaymentDto);

    //                if (paymentResult.StatusCode != HttpStatusCode.OK)
    //                {
    //                    _logger.LogError("Payment initialization failed for ticket {TicketId}", ticket.Id);

    //                    await transaction.RollbackAsync();

    //                    return APIResponse<object>.Create(
    //                       HttpStatusCode.InternalServerError,
    //                       "An error occurred while initiating payment for the ticket",
    //                       null,
    //                       new Error { Message = "Payment initialization failed. Ticket reservation has been cancelled." });
    //                }
                
    //            await transaction.CommitAsync();


    //            return APIResponse<object>.Create(HttpStatusCode.Created, "Ticket reserved. Please complete payment to confirm your ticket.", new
    //            {
    //                TicketId = ticket.Id,
    //                TicketNumber = ticket.TicketNumber,
    //                TicketStatus = ticket.Status.GetEnumDescription(),
    //                Amount = ticketType.Price,
    //                PaymentUrl = paymentResult.Data.PaymentUrl,
    //                TransactionReference = paymentResult.Data.TransactionReference
    //            });
    //        }

    //        // Free ticket - already active
    //        return APIResponse<object>.Create(HttpStatusCode.Created, "Ticket created successfully", new
    //        {
    //            TicketId = ticket.Id,
    //            TicketNumber = ticket.TicketNumber,
    //            TicketStatus = ticket.Status.GetEnumDescription()
    //        });
    //    }
    //         catch (Exception ex)
    //        {
    //            _logger.LogError(ex, "An error occurred while creating ticket for user {UserId}", userId);
    //            await transaction.RollbackAsync();
    //            return APIResponse<object>.Create(HttpStatusCode.InternalServerError, "An error occurred while creating the ticket", null, new Error { Message = "An error occurred while creating the ticket" });
    //    }
    //}
    
        public async Task<APIResponse<CreateTicketResponseDto>> CreateTicketAsync(Guid userId, CreateTicketDto createTicketDto)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return APIResponse<CreateTicketResponseDto>.Create(
                    HttpStatusCode.NotFound,
                    "User not found",
                    null,
                    new Error { Message = "User not found" });
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var ticketType = await _dbContext.TicketTypes
                    .Include(tt => tt.Event)
                    .FirstOrDefaultAsync(tt =>
                        tt.Id == createTicketDto.TicketTypeId &&
                        tt.EventId == createTicketDto.EventId);

                if (ticketType == null)
                {
                    return APIResponse<CreateTicketResponseDto>.Create(
                        HttpStatusCode.NotFound,
                        "Ticket type not found for the provided event",
                        null,
                        new Error { Message = "Ticket type not found for the provided event" });
                }

                // Atomic capacity update (Concurrency safe)
                var rowsAffected = await _dbContext.Database.ExecuteSqlInterpolatedAsync($@"
                    
                    UPDATE TicketTypes
                    SET SoldCount = SoldCount + 1
                    WHERE Id = {ticketType.Id} 
                    AND SoldCount < Capacity");

                if(rowsAffected == 0)
                {
                    return APIResponse<CreateTicketResponseDto>.Create(
                        HttpStatusCode.BadRequest,
                        "No tickets available for the selected ticket type",
                        null,
                        new Error { Message = "No tickets available for the selected ticket type" });
                }

                // Check if user already has a ticket for the event (active or reserved)
                var existingTicket = await _dbContext.Tickets
                    .Include(t => t.TicketType)
                    .FirstOrDefaultAsync(t =>
                        t.AttendeeId == user.Id &&
                        t.TicketType.EventId == createTicketDto.EventId &&
                        (t.Status == TicketStatus.Active || t.Status == TicketStatus.Reserved));

                if (existingTicket != null)
                {
                    // rollback sold count
                    await _dbContext.Database.ExecuteSqlInterpolatedAsync($@"
                        UPDATE TicketTypes
                        SET SoldCount = SoldCount - 1
                        WHERE Id = {ticketType.Id}");

                    return APIResponse<CreateTicketResponseDto>.Create(
                        HttpStatusCode.BadRequest,
                        "User already has an active or reserved ticket for this event",
                        null,
                        new Error { Message = "User already has an active or reserved ticket for this event" });
                }

                var now = DateTimeOffset.UtcNow;
                var isPaidTicket = ticketType.Price > 0;
                var ticket = new Ticket
                {
                    Id = Guid.NewGuid(),
                    AttendeeId = user.Id,
                    TicketTypeId = ticketType.Id,
                    TicketNumber = $"TCKT-{Guid.NewGuid().ToString()[..8].ToUpper()}",
                    Status = ticketType.Price > 0 ? TicketStatus.Reserved : TicketStatus.Active,
                    ReservationExpiresAt = isPaidTicket ? now.AddMinutes(15) : null,
                    CreatedDate = now,
                    ModifiedDate = now
                };

                await _dbContext.Tickets.AddAsync(ticket);

                var auditLog = new AuditLog
                {
                    Id = Guid.NewGuid(),
                    ActionType = ActionType.Create,
                    ObjectId = ticket.Id,
                    UserId = user.Id,
                    Description = $"Ticket {ticket.TicketNumber} created by user {user.UserName}",
                    CreatedDate = now,
                    ModifiedDate = now
                };

                await _dbContext.AuditLogs.AddAsync(auditLog);
                await _dbContext.SaveChangesAsync();

                // For paid tickets, initiate payment
                if (isPaidTicket)
                {
                    var initiatePaymentDto = new InitiatePaymentDto
                    {
                        UserId = user.Id,
                        Email = user.Email!,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        PhoneNumber = user.PhoneNumber ?? string.Empty,
                        Amount = ticketType.Price,
                        Currency = "NGN",
                        Description = $"Payment for {ticketType.Name} ticket - {ticketType.Event.Title}",
                        PaymentType = PaymentType.Ticket,
                        ReferenceId = ticket.Id,
                        Metadata = new Dictionary<string, string>
                {
                    { "ticket_id", ticket.Id.ToString() },
                    { "ticket_number", ticket.TicketNumber },
                    { "event_id", ticketType.EventId.ToString() },
                    { "ticket_type", ticketType.Name }
                }
                    };

                    var paymentResult = await _paymentService.InitializePaymentAsync(initiatePaymentDto);

                    if (paymentResult.StatusCode != (int)HttpStatusCode.OK || paymentResult.Data == null)
                    {
                        _logger.LogError("Payment initialization failed for ticket {TicketId}", ticket.Id);

                        // rollback sold count
                        await _dbContext.Database.ExecuteSqlInterpolatedAsync($@"
                            UPDATE TicketTypes
                            SET SoldCount = SoldCount - 1
                            WHERE Id = {ticketType.Id}");

                        await transaction.RollbackAsync();

                        return APIResponse<CreateTicketResponseDto>.Create(
                            HttpStatusCode.InternalServerError,
                            "An error occurred while initiating payment for the ticket",
                            null,
                            new Error
                            {
                                Message = "Payment initialization failed. Ticket reservation has been cancelled."
                            });
                    }

                    await transaction.CommitAsync();

                    var response = new CreateTicketResponseDto
                    {
                        TicketId = ticket.Id,
                        TicketStatus = ticket.Status.GetEnumDescription(),
                        TicketNumber = ticket.TicketNumber,
                        Amount = ticketType.Price,
                        PaymentReference = paymentResult.Data.TransactionReference,
                        PaymentUrl = paymentResult.Data.PaymentUrl,
                        DateCreated = ticket.CreatedDate
                    };
                    return APIResponse<CreateTicketResponseDto>.Create(
                        HttpStatusCode.Created,
                        "Ticket reserved. Please complete payment to confirm your ticket.", response);
                }

                // Free ticket
                await transaction.CommitAsync();

                var result = new CreateTicketResponseDto
                {
                    TicketId = ticket.Id,
                    TicketStatus = ticket.Status.GetEnumDescription(),
                    TicketNumber = ticket.TicketNumber,
                    Amount = ticketType.Price,
                    DateCreated = ticket.CreatedDate
                };
                return APIResponse<CreateTicketResponseDto>.Create(
                    HttpStatusCode.Created,
                    "Ticket created successfully",
                    result);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                _logger.LogError(ex, "Error occurred while creating ticket for user {UserId}", userId);

                return APIResponse<CreateTicketResponseDto>.Create(
                    HttpStatusCode.InternalServerError,
                    "An error occurred while creating the ticket",
                    null,
                    new Error { Message = "An unexpected error occurred." });
            }
        }
    }
}
