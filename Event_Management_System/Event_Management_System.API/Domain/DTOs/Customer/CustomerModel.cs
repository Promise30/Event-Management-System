namespace Event_Management_System.API.Domain.DTOs.Customer
{
    public class Address
    {
        public string city { get; set; }
        public string country { get; set; }
        public string line1 { get; set; }
        public string line2 { get; set; }
        public string postal_code { get; set; }
        public string state { get; set; }
    }

    public class Name
    {
        public string first { get; set; }
        public string middle { get; set; }
        public string last { get; set; }
    }

    public class Phone
    {
        public string country_code { get; set; }
        public string number { get; set; }
    }
    public class Meta
    {
    }

}
