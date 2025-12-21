
namespace Event_Management_System.API.Domain.DTOs.Customer
{
    public class CustomerRequestDto
    {
        public Address address { get; set; }
        public Name name { get; set; }
        public Phone phone { get; set; }
        public string email { get; set; }
    }
}
/*
--header 'Authorization: Bearer {{YOUR_ACCESS_TOKEN}}' \
--header 'Content-Type: application/json' \
--header 'X-Trace-Id: {{YOUR_UNIQUE_TRACE_ID}}' 
*/