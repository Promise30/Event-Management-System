namespace Event_Management_System.API.Helpers
{
    public class SortParameters
    {
        public SortingType SortType { get; set; } = SortingType.Descending;


        public string SortMember { get; set; }
    }
}
