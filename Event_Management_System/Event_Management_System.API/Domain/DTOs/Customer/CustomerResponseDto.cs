namespace Event_Management_System.API.Domain.DTOs.Customer
{
    public class CustomerResponseDto
    {
            public string id { get; set; }
            public Address address { get; set; }
            public string email { get; set; }
            public Name name { get; set; }
            public Phone phone { get; set; }
            public Meta meta { get; set; }
            public string created_datetime { get; set; }
    }
}
