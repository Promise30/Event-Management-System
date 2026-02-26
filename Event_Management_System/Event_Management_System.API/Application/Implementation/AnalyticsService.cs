using Event_Management_System.API.Application.Interfaces;
using Event_Management_System.API.Domain.DTOs.Analytics;
using Event_Management_System.API.Domain.Enums;
using Event_Management_System.API.Helpers;
using Event_Management_System.API.Infrastructures;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Net;

namespace Event_Management_System.API.Application.Implementation
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AnalyticsService> _logger;

        public AnalyticsService(ApplicationDbContext context, ILogger<AnalyticsService> logger)
        {
            _context = context;
            _logger = logger;
        }

        #region Revenue & Bookings Analytics

        public async Task<APIResponse<RevenueSummaryDto>> GetRevenueSummaryAsync(DateTime? from, DateTime? to)
        {
            var query = _context.Payments
                .Where(p => p.Status == PaymentStatus.Successful && p.PaidAt.HasValue);

            if (from.HasValue)
            {
                var fromOffset = new DateTimeOffset(from.Value, TimeSpan.Zero);
                query = query.Where(p => p.PaidAt!.Value >= fromOffset);
            }

            if (to.HasValue)
            {
                var toOffset = new DateTimeOffset(to.Value, TimeSpan.Zero);
                query = query.Where(p => p.PaidAt!.Value <= toOffset);
            }

            var payments = await query
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    TotalRevenue = g.Sum(p => p.Amount),
                    TotalPayments = g.Count(),
                    BookingRevenue = g.Where(p => p.PaymentType == (int)PaymentType.Booking).Sum(p => p.Amount),
                    TicketRevenue = g.Where(p => p.PaymentType == (int)PaymentType.Ticket).Sum(p => p.Amount)
                })
                .FirstOrDefaultAsync();

            var result = new RevenueSummaryDto
            {
                TotalRevenue = payments?.TotalRevenue ?? 0,
                TotalPayments = payments?.TotalPayments ?? 0,
                BookingRevenue = payments?.BookingRevenue ?? 0,
                TicketRevenue = payments?.TicketRevenue ?? 0,
                From = from,
                To = to
            };

            return APIResponse<RevenueSummaryDto>.Create(HttpStatusCode.OK, "Revenue summary retrieved successfully.", result);
        }

        public async Task<APIResponse<List<RevenueByEventDto>>> GetRevenueByEventAsync()
        {
            // Ticket revenue per event: Payment -> Ticket -> TicketType -> Event
            var ticketRevenue = await _context.Payments
                .Where(p => p.Status == PaymentStatus.Successful && p.PaymentType == (int)PaymentType.Ticket)
                .Join(_context.Tickets,
                    p => p.ReferenceId,
                    t => t.Id,
                    (p, t) => new { p.Amount, t.TicketTypeId })
                .Join(_context.TicketTypes,
                    pt => pt.TicketTypeId,
                    tt => tt.Id,
                    (pt, tt) => new { pt.Amount, tt.EventId })
                .Join(_context.Events,
                    ptt => ptt.EventId,
                    e => e.Id,
                    (ptt, e) => new { ptt.Amount, e.Id, e.Title })
                .GroupBy(x => new { x.Id, x.Title })
                .Select(g => new RevenueByEventDto
                {
                    EventId = g.Key.Id,
                    EventTitle = g.Key.Title,
                    TotalRevenue = g.Sum(x => x.Amount),
                    TicketsSold = g.Count()
                })
                .OrderByDescending(r => r.TotalRevenue)
                .ToListAsync();

            return APIResponse<List<RevenueByEventDto>>.Create(HttpStatusCode.OK, "Revenue by event retrieved successfully.", ticketRevenue);
        }

        public async Task<APIResponse<List<RevenueByPeriodDto>>> GetRevenueByPeriodAsync(string groupBy)
        {
            var payments = await _context.Payments
                .Where(p => p.Status == PaymentStatus.Successful && p.PaidAt.HasValue)
                .Select(p => new { p.Amount, PaidAt = p.PaidAt!.Value })
                .ToListAsync();

            var grouped = groupBy?.ToLowerInvariant() switch
            {
                "week" => payments
                    .GroupBy(p => CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(
                        p.PaidAt.DateTime, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday))
                    .Select(g => new RevenueByPeriodDto
                    {
                        Period = $"{g.First().PaidAt.Year}-W{g.Key:D2}",
                        TotalRevenue = g.Sum(p => p.Amount),
                        TransactionCount = g.Count()
                    })
                    .OrderBy(r => r.Period)
                    .ToList(),
                "month" => payments
                    .GroupBy(p => new { p.PaidAt.Year, p.PaidAt.Month })
                    .Select(g => new RevenueByPeriodDto
                    {
                        Period = $"{g.Key.Year}-{g.Key.Month:D2}",
                        TotalRevenue = g.Sum(p => p.Amount),
                        TransactionCount = g.Count()
                    })
                    .OrderBy(r => r.Period)
                    .ToList(),
                _ => payments // default to day
                    .GroupBy(p => p.PaidAt.Date)
                    .Select(g => new RevenueByPeriodDto
                    {
                        Period = g.Key.ToString("yyyy-MM-dd"),
                        TotalRevenue = g.Sum(p => p.Amount),
                        TransactionCount = g.Count()
                    })
                    .OrderBy(r => r.Period)
                    .ToList()
            };

            return APIResponse<List<RevenueByPeriodDto>>.Create(HttpStatusCode.OK, $"Revenue by {groupBy ?? "day"} retrieved successfully.", grouped);
        }

        public async Task<APIResponse<BookingSummaryDto>> GetBookingSummaryAsync(DateTime? from, DateTime? to)
        {
            var query = _context.Bookings.AsQueryable();

            if (from.HasValue)
            {
                var fromOffset = new DateTimeOffset(from.Value, TimeSpan.Zero);
                query = query.Where(b => b.CreatedDate >= fromOffset);
            }
            if (to.HasValue)
            {
                var toOffset = new DateTimeOffset(to.Value, TimeSpan.Zero);
                query = query.Where(b => b.CreatedDate <= toOffset);
            }

            var bookings = await query
                .GroupBy(_ => 1)
                .Select(g => new BookingSummaryDto
                {
                    TotalBookings = g.Count(),
                    Confirmed = g.Count(b => b.BookingStatus == BookingStatus.Confirmed),
                    PendingPayment = g.Count(b => b.BookingStatus == BookingStatus.PendingPayment),
                    PendingApproval = g.Count(b => b.BookingStatus == BookingStatus.PendingApproval),
                    Cancelled = g.Count(b => b.BookingStatus == BookingStatus.Cancelled),
                    Rejected = g.Count(b => b.BookingStatus == BookingStatus.Rejected),
                    Expired = g.Count(b => b.BookingStatus == BookingStatus.Expired),
                    Submitted = g.Count(b => b.BookingStatus == BookingStatus.Submitted)
                })
                .FirstOrDefaultAsync();

            var result = bookings ?? new BookingSummaryDto();

            return APIResponse<BookingSummaryDto>.Create(HttpStatusCode.OK, "Booking summary retrieved successfully.", result);
        }

        public async Task<APIResponse<List<BookingsByEventDto>>> GetBookingsByEventAsync()
        {
            var result = await _context.Bookings
                .Include(b => b.EventCentre)
                .GroupBy(b => new { b.EventCentreId, b.EventCentre.Name })
                .Select(g => new BookingsByEventDto
                {
                    EventCentreId = g.Key.EventCentreId,
                    EventCentreName = g.Key.Name,
                    TotalBookings = g.Count(),
                    Confirmed = g.Count(b => b.BookingStatus == BookingStatus.Confirmed),
                    Cancelled = g.Count(b => b.BookingStatus == BookingStatus.Cancelled)
                })
                .OrderByDescending(b => b.TotalBookings)
                .ToListAsync();

            return APIResponse<List<BookingsByEventDto>>.Create(HttpStatusCode.OK, "Bookings by event centre retrieved successfully.", result);
        }

        public async Task<APIResponse<BookingConversionRateDto>> GetBookingConversionRateAsync()
        {
            var totalBookings = await _context.Bookings.CountAsync();
            var confirmedBookings = await _context.Bookings.CountAsync(b => b.BookingStatus == BookingStatus.Confirmed);

            var totalTickets = await _context.Tickets.CountAsync();
            var confirmedTickets = await _context.Tickets.CountAsync(t =>
                t.Status == TicketStatus.Confirmed || t.Status == TicketStatus.Active || t.Status == TicketStatus.Used);

            var result = new BookingConversionRateDto
            {
                TotalBookings = totalBookings,
                ConfirmedBookings = confirmedBookings,
                ConversionRate = totalBookings > 0 ? Math.Round((double)confirmedBookings / totalBookings * 100, 2) : 0,
                TotalTicketReservations = totalTickets,
                ConfirmedTickets = confirmedTickets,
                TicketConversionRate = totalTickets > 0 ? Math.Round((double)confirmedTickets / totalTickets * 100, 2) : 0
            };

            return APIResponse<BookingConversionRateDto>.Create(HttpStatusCode.OK, "Booking conversion rate retrieved successfully.", result);
        }

        #endregion

        #region Ticket Analytics

        public async Task<APIResponse<TicketsSoldDto>> GetTicketsSoldAsync(Guid? eventId, DateTime? from, DateTime? to)
        {
            var query = _context.Tickets
                .Include(t => t.TicketType)
                .Where(t => t.Status == TicketStatus.Confirmed || t.Status == TicketStatus.Active || t.Status == TicketStatus.Used);

            if (eventId.HasValue)
                query = query.Where(t => t.TicketType.EventId == eventId.Value);
            if (from.HasValue)
            {
                var fromOffset = new DateTimeOffset(from.Value, TimeSpan.Zero);
                query = query.Where(t => t.CreatedDate >= fromOffset);
            }
            if (to.HasValue)
            {
                var toOffset = new DateTimeOffset(to.Value, TimeSpan.Zero);
                query = query.Where(t => t.CreatedDate <= toOffset);
            }

            var tickets = await query.ToListAsync();

            var totalSold = tickets.Count;
            var totalRevenue = tickets.Sum(t => t.TicketType.Price);

            var result = new TicketsSoldDto
            {
                TotalTicketsSold = totalSold,
                TotalRevenue = totalRevenue,
                From = from,
                To = to,
                EventId = eventId
            };

            return APIResponse<TicketsSoldDto>.Create(HttpStatusCode.OK, "Tickets sold data retrieved successfully.", result);
        }

        public async Task<APIResponse<List<TicketsByTypeDto>>> GetTicketsByTypeAsync(Guid? eventId)
        {
            var query = _context.TicketTypes
                .Include(tt => tt.Event)
                .AsQueryable();

            if (eventId.HasValue)
                query = query.Where(tt => tt.EventId == eventId.Value);

            var result = await query
                .Select(tt => new TicketsByTypeDto
                {
                    TicketTypeId = tt.Id,
                    TicketTypeName = tt.Name,
                    EventId = tt.EventId,
                    EventTitle = tt.Event.Title,
                    Price = tt.Price,
                    Sold = tt.SoldCount,
                    Revenue = tt.SoldCount * tt.Price
                })
                .OrderByDescending(t => t.Revenue)
                .ToListAsync();

            return APIResponse<List<TicketsByTypeDto>>.Create(HttpStatusCode.OK, "Tickets by type retrieved successfully.", result);
        }

        public async Task<APIResponse<List<TicketAvailabilityDto>>> GetTicketAvailabilityAsync(Guid? eventId)
        {
            var query = _context.Events
                .Include(e => e.TicketTypes)
                .AsQueryable();

            if (eventId.HasValue)
                query = query.Where(e => e.Id == eventId.Value);

            var events = await query.ToListAsync();

            var result = events.Select(e =>
            {
                var totalCapacity = e.TicketTypes.Sum(tt => tt.Capacity);
                var totalSold = e.TicketTypes.Sum(tt => tt.SoldCount);

                return new TicketAvailabilityDto
                {
                    EventId = e.Id,
                    EventTitle = e.Title,
                    TotalCapacity = totalCapacity,
                    TotalSold = totalSold,
                    Remaining = totalCapacity - totalSold,
                    SoldPercentage = totalCapacity > 0 ? Math.Round((double)totalSold / totalCapacity * 100, 2) : 0,
                    TicketTypes = e.TicketTypes.Select(tt => new TicketTypeAvailabilityDto
                    {
                        TicketTypeId = tt.Id,
                        Name = tt.Name,
                        Capacity = tt.Capacity,
                        Sold = tt.SoldCount,
                        Remaining = tt.Capacity - tt.SoldCount,
                        Price = tt.Price
                    }).ToList()
                };
            }).ToList();

            return APIResponse<List<TicketAvailabilityDto>>.Create(HttpStatusCode.OK, "Ticket availability retrieved successfully.", result);
        }

        #endregion

        #region Event Analytics

        public async Task<APIResponse<List<PopularEventDto>>> GetPopularEventsAsync(PopularEventsRequestParameter requestParameter)
        {
            // Cap top value between 1 and 100
            requestParameter.Top = Math.Clamp(requestParameter.Top > 0 ? requestParameter.Top : 10, 1, 100);

            var query = _context.Events
                                .Include(e => e.TicketTypes)
                                .Include(e => e.Booking)
                                .AsQueryable();

            // Apply date range filter if provided
            if (requestParameter.StartDate.HasValue)
                query = query.Where(e => e.StartTime >= requestParameter.StartDate.Value);

            if (requestParameter.EndDate.HasValue)
                query = query.Where(e => e.StartTime <= requestParameter.EndDate.Value.Date.AddDays(1).AddTicks(-1)); // end of day

            var projected = query.Select(e => new PopularEventDto
            {
                EventId = e.Id,
                Title = e.Title,
                TotalTicketsSold = e.TicketTypes.Sum(tt => tt.SoldCount),
                TotalRevenue = e.TicketTypes.Sum(tt => tt.SoldCount * tt.Price),
                TotalBookings =1
            });
            projected = requestParameter.SortBy?.ToLowerInvariant() switch
            {
                "revenue" => projected.OrderByDescending(e => e.TotalRevenue),
                "bookings" => projected.OrderByDescending(e => e.TotalBookings),
                _ => projected.OrderByDescending(e => e.TotalTicketsSold)
            };

            var result = await projected.Take(requestParameter.Top).ToListAsync();

            return APIResponse<List<PopularEventDto>>.Create(HttpStatusCode.OK, "Popular events retrieved successfully.", result);
        }

        public async Task<APIResponse<List<UpcomingLowAvailabilityEventDto>>> GetUpcomingLowAvailabilityEventsAsync(double threshold, DateTime? startDate, DateTime? endDate)
        {
            var effectiveThreshold = Math.Clamp(threshold > 0 ? threshold : 80, 1, 100);
            var now = DateTime.UtcNow;

            var query = _context.Events
                .Where(e => e.StartTime > now);

            if (startDate.HasValue)
                query = query.Where(e => e.StartTime >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(e => e.StartTime <= endDate.Value.Date.AddDays(1).AddTicks(-1));

            var result = await query
                .Select(e => new
                {
                    e.Id,
                    e.Title,
                    e.StartTime,
                    TotalCapacity = e.TicketTypes.Sum(tt => tt.Capacity),
                    TotalSold = e.TicketTypes.Sum(tt => tt.SoldCount)
                })
                .Where(e => e.TotalCapacity > 0)
                .Where(e => (double)e.TotalSold / e.TotalCapacity * 100 >= effectiveThreshold)
                .OrderByDescending(e => (double)e.TotalSold / e.TotalCapacity)
                .Select(e => new UpcomingLowAvailabilityEventDto
                {
                    EventId = e.Id,
                    Title = e.Title,
                    StartTime = e.StartTime,
                    TotalCapacity = e.TotalCapacity,
                    TotalSold = e.TotalSold,
                    Remaining = e.TotalCapacity - e.TotalSold,
                    SoldPercentage = Math.Round((double)e.TotalSold / e.TotalCapacity * 100, 2)
                })
                .ToListAsync();

            return APIResponse<List<UpcomingLowAvailabilityEventDto>>.Create(
                HttpStatusCode.OK,
                $"Upcoming events with {effectiveThreshold}%+ tickets sold retrieved successfully.",
                result);
        }

        public async Task<APIResponse<List<EventPerformanceDto>>> GetEventPerformanceAsync(Guid? eventId)
        {
            var query = _context.Events
                .Include(e => e.TicketTypes)
                .AsQueryable();

            if (eventId.HasValue)
                query = query.Where(e => e.Id == eventId.Value);

            var events = await query.ToListAsync();

            var result = events.Select(e =>
            {
                var totalCapacity = e.TicketTypes.Sum(tt => tt.Capacity);
                var totalSold = e.TicketTypes.Sum(tt => tt.SoldCount);
                var totalRevenue = e.TicketTypes.Sum(tt => tt.SoldCount * tt.Price);

                return new EventPerformanceDto
                {
                    EventId = e.Id,
                    Title = e.Title,
                    StartTime = e.StartTime,
                    EndTime = e.EndTime,
                    TotalCapacity = totalCapacity,
                    TotalSold = totalSold,
                    TotalRevenue = totalRevenue,
                    SoldPercentage = totalCapacity > 0 ? Math.Round((double)totalSold / totalCapacity * 100, 2) : 0
                };
            })
            .OrderByDescending(e => e.TotalRevenue)
            .ToList();

            return APIResponse<List<EventPerformanceDto>>.Create(HttpStatusCode.OK, "Event performance data retrieved successfully.", result);
        }

        #endregion

        #region Event Centre Analytics

        public async Task<APIResponse<List<EventCentreUtilizationDto>>> GetEventCentreUtilizationAsync()
        {
            var result = await _context.EventCentres
                .Include(ec => ec.Bookings)
                .Select(ec => new EventCentreUtilizationDto
                {
                    EventCentreId = ec.Id,
                    Name = ec.Name,
                    Location = ec.Location,
                    TotalBookings = ec.Bookings.Count,
                    ConfirmedBookings = ec.Bookings.Count(b => b.BookingStatus == BookingStatus.Confirmed),
                    UtilizationRate = ec.Bookings.Count > 0
                        ? Math.Round((double)ec.Bookings.Count(b => b.BookingStatus == BookingStatus.Confirmed) / ec.Bookings.Count * 100, 2)
                        : 0
                })
                .OrderByDescending(ec => ec.ConfirmedBookings)
                .ToListAsync();

            return APIResponse<List<EventCentreUtilizationDto>>.Create(HttpStatusCode.OK, "Event centre utilization retrieved successfully.", result);
        }

        public async Task<APIResponse<List<EventCentreRevenueDto>>> GetEventCentreRevenueAsync()
        {
            // Revenue from booking payments associated with each event centre
            var centresWithBookings = await _context.EventCentres
                .Include(ec => ec.Bookings)
                .Select(ec => new
                {
                    ec.Id,
                    ec.Name,
                    ec.Location,
                    ec.PricePerDay,
                    ConfirmedBookingIds = ec.Bookings
                        .Where(b => b.BookingStatus == BookingStatus.Confirmed)
                        .Select(b => b.Id)
                        .ToList(),
                    ConfirmedCount = ec.Bookings.Count(b => b.BookingStatus == BookingStatus.Confirmed)
                })
                .ToListAsync();

            var allConfirmedBookingIds = centresWithBookings.SelectMany(c => c.ConfirmedBookingIds).ToList();

            var revenueByBooking = await _context.Payments
                .Where(p => p.Status == PaymentStatus.Successful
                    && p.PaymentType == (int)PaymentType.Booking
                    && allConfirmedBookingIds.Contains(p.ReferenceId))
                .GroupBy(p => p.ReferenceId)
                .Select(g => new { BookingId = g.Key, Revenue = g.Sum(p => p.Amount) })
                .ToDictionaryAsync(x => x.BookingId, x => x.Revenue);

            var result = centresWithBookings.Select(ec => new EventCentreRevenueDto
            {
                EventCentreId = ec.Id,
                Name = ec.Name,
                Location = ec.Location,
                PricePerDay = ec.PricePerDay,
                TotalConfirmedBookings = ec.ConfirmedCount,
                TotalRevenue = ec.ConfirmedBookingIds.Sum(bid => revenueByBooking.GetValueOrDefault(bid, 0))
            })
            .OrderByDescending(ec => ec.TotalRevenue)
            .ToList();

            return APIResponse<List<EventCentreRevenueDto>>.Create(HttpStatusCode.OK, "Event centre revenue retrieved successfully.", result);
        }

        public async Task<APIResponse<List<EventCentreAvailabilityTrendDto>>> GetEventCentreAvailabilityTrendsAsync(string groupBy)
        {
            var bookings = await _context.Bookings
                .Include(b => b.EventCentre)
                .Where(b => b.BookingStatus == BookingStatus.Confirmed)
                .ToListAsync();

            var centres = bookings
                .GroupBy(b => new { b.EventCentreId, b.EventCentre.Name })
                .Select(g =>
                {
                    var periods = groupBy?.ToLowerInvariant() switch
                    {
                        "month" => g
                            .GroupBy(b => $"{b.BookedFrom.Year}-{b.BookedFrom.Month:D2}")
                            .Select(pg => new PeakPeriodDto { Period = pg.Key, BookingCount = pg.Count() })
                            .OrderByDescending(p => p.BookingCount)
                            .ToList(),
                        "dayofweek" => g
                            .GroupBy(b => b.BookedFrom.DayOfWeek.ToString())
                            .Select(pg => new PeakPeriodDto { Period = pg.Key, BookingCount = pg.Count() })
                            .OrderByDescending(p => p.BookingCount)
                            .ToList(),
                        _ => g // default to month
                            .GroupBy(b => $"{b.BookedFrom.Year}-{b.BookedFrom.Month:D2}")
                            .Select(pg => new PeakPeriodDto { Period = pg.Key, BookingCount = pg.Count() })
                            .OrderByDescending(p => p.BookingCount)
                            .ToList()
                    };

                    return new EventCentreAvailabilityTrendDto
                    {
                        EventCentreId = g.Key.EventCentreId,
                        Name = g.Key.Name,
                        PeakPeriods = periods
                    };
                })
                .ToList();

            return APIResponse<List<EventCentreAvailabilityTrendDto>>.Create(HttpStatusCode.OK,
                "Event centre availability trends retrieved successfully.", centres);
        }

        #endregion

        #region Organizer Analytics

        public async Task<APIResponse<List<OrganizerPerformanceDto>>> GetOrganizerPerformanceAsync(Guid? organizerId)
        {
            var query = _context.Bookings
                .Include(b => b.Organizer)
                .AsQueryable();

            if (organizerId.HasValue)
                query = query.Where(b => b.OrganizerId == organizerId.Value);

            var organizers = await query
                .GroupBy(b => new { b.OrganizerId, b.Organizer.FirstName, b.Organizer.LastName, b.Organizer.Email })
                .Select(g => new
                {
                    g.Key.OrganizerId,
                    Name = g.Key.FirstName + " " + g.Key.LastName,
                    Email = g.Key.Email!,
                    TotalBookings = g.Count(),
                    ConfirmedBookings = g.Count(b => b.BookingStatus == BookingStatus.Confirmed),
                    ConfirmedBookingIds = g
                        .Where(b => b.BookingStatus == BookingStatus.Confirmed)
                        .Select(b => b.Id)
                        .ToList()
                })
                .ToListAsync();

            // Get all confirmed booking IDs to query events
            var allConfirmedBookingIds = organizers.SelectMany(o => o.ConfirmedBookingIds).ToList();

            // Get events per organizer (Event has a BookingId)
            var eventsPerBooking = await _context.Events
                .Where(e => allConfirmedBookingIds.Contains(e.BookingId))
                .GroupBy(e => e.BookingId)
                .Select(g => new { BookingId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.BookingId, x => x.Count);

            // Get revenue per booking
            var revenueByBooking = await _context.Payments
                .Where(p => p.Status == PaymentStatus.Successful
                    && p.PaymentType == (int)PaymentType.Booking
                    && allConfirmedBookingIds.Contains(p.ReferenceId))
                .GroupBy(p => p.ReferenceId)
                .Select(g => new { BookingId = g.Key, Revenue = g.Sum(p => p.Amount) })
                .ToDictionaryAsync(x => x.BookingId, x => x.Revenue);

            var result = organizers.Select(o => new OrganizerPerformanceDto
            {
                OrganizerId = o.OrganizerId,
                OrganizerName = o.Name,
                Email = o.Email,
                TotalBookings = o.TotalBookings,
                ConfirmedBookings = o.ConfirmedBookings,
                TotalEventsHosted = o.ConfirmedBookingIds.Sum(bid => eventsPerBooking.GetValueOrDefault(bid, 0)),
                TotalRevenue = o.ConfirmedBookingIds.Sum(bid => revenueByBooking.GetValueOrDefault(bid, 0))
            })
            .OrderByDescending(o => o.TotalRevenue)
            .ToList();

            return APIResponse<List<OrganizerPerformanceDto>>.Create(HttpStatusCode.OK, "Organizer performance data retrieved successfully.", result);
        }

        public async Task<APIResponse<List<OrganizerEventsHostedDto>>> GetOrganizerEventsHostedAsync(Guid? organizerId)
        {
            var query = _context.Bookings
                .Include(b => b.Organizer)
                .Where(b => b.BookingStatus == BookingStatus.Confirmed)
                .AsQueryable();

            if (organizerId.HasValue)
                query = query.Where(b => b.OrganizerId == organizerId.Value);

            var confirmedBookings = await query.ToListAsync();

            var bookingIds = confirmedBookings.Select(b => b.Id).ToList();

            var events = await _context.Events
                .Where(e => bookingIds.Contains(e.BookingId))
                .ToListAsync();

            var result = confirmedBookings
                .GroupBy(b => new { b.OrganizerId, b.Organizer.FirstName, b.Organizer.LastName })
                .Select(g =>
                {
                    var orgBookingIds = g.Select(b => b.Id).ToList();
                    var orgEvents = events.Where(e => orgBookingIds.Contains(e.BookingId)).ToList();

                    return new OrganizerEventsHostedDto
                    {
                        OrganizerId = g.Key.OrganizerId,
                        OrganizerName = $"{g.Key.FirstName} {g.Key.LastName}",
                        TotalEventsHosted = orgEvents.Count,
                        Events = orgEvents.Select(e => new OrganizerEventSummaryDto
                        {
                            EventId = e.Id,
                            Title = e.Title,
                            StartTime = e.StartTime
                        }).ToList()
                    };
                })
                .OrderByDescending(o => o.TotalEventsHosted)
                .ToList();

            return APIResponse<List<OrganizerEventsHostedDto>>.Create(HttpStatusCode.OK, "Organizer events hosted retrieved successfully.", result);
        }

        public async Task<APIResponse<List<TopOrganizerDto>>> GetTopOrganizersAsync(int top)
        {
            var effectiveTop = top > 0 ? top : 10;

            var performanceResult = await GetOrganizerPerformanceAsync(null);
            var performances = performanceResult.Data ?? new List<OrganizerPerformanceDto>();

            var result = performances
                .OrderByDescending(o => o.TotalRevenue)
                .ThenByDescending(o => o.TotalEventsHosted)
                .Take(effectiveTop)
                .Select((o, index) => new TopOrganizerDto
                {
                    Rank = index + 1,
                    OrganizerId = o.OrganizerId,
                    OrganizerName = o.OrganizerName,
                    Email = o.Email,
                    TotalRevenue = o.TotalRevenue,
                    TotalBookings = o.TotalBookings,
                    TotalEventsHosted = o.TotalEventsHosted
                })
                .ToList();

            return APIResponse<List<TopOrganizerDto>>.Create(HttpStatusCode.OK, "Top organizers retrieved successfully.", result);
        }

        #endregion
    }
}
