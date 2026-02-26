using AutoMapper;
using Event_Management_System.API.Domain.DTOs.Booking;
using Event_Management_System.API.Domain.DTOs.Event;
using Event_Management_System.API.Domain.DTOs.EventCenter;
using Event_Management_System.API.Domain.DTOs.Ticket;
using Event_Management_System.API.Domain.DTOs.TicketType;
using Event_Management_System.API.Domain.Entities;

namespace Event_Management_System.API.Helpers
{
    public class Mappings : Profile
    {
        public Mappings()
        {
            CreateMap<EventCentre, EventCentreDto>().ReverseMap();
            CreateMap<EventCentreAvailability, EventCentreAvailabilityDto>()
                .ForMember(eca => eca.Day, opt => opt.MapFrom(src => src.Day.GetEnumDescription()));
            CreateMap<Event, EventDto>().ReverseMap();
            CreateMap<Booking, BookingDto>()
                .ForMember(b => b.EventCentreName, opt => opt.MapFrom(src => src.EventCentre.Name))
                .ForMember(b => b.BookingStatus, opt => opt.MapFrom(src => src.BookingStatus.GetEnumDescription()))
                .ForMember(b => b.OrganizerName, opt => opt.MapFrom(src => (src.Organizer.FirstName + " " + src.Organizer.LastName)));
            CreateMap<TicketType, TicketTypeDto>()
                .ForMember(dto => dto.DateCreated, opt => opt.MapFrom(src => src.CreatedDate));
            CreateMap<Ticket, TicketDto>()
                .ForMember(t => t.TicketType, opt => opt.MapFrom(src => src.TicketType.Name))
                .ForMember(t => t.TicketStatus, opt => opt.MapFrom(src=> src.Status.GetEnumDescription()))
                .ForMember(t => t.DateCreated, opt => opt.MapFrom(src => src.CreatedDate));
        
        }
    }
}
