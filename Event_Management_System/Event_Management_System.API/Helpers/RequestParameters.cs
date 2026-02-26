namespace Event_Management_System.API.Helpers
{
    public class RequestParameters
    {
        private int _pageSize = 10;

        private const int MaxPageSize = 50;

        public int PageNumber { get; set; } = 1;


        public int PageSize
        {
            get
            {
                return _pageSize;
            }
            set
            {
                _pageSize = value > 50 ? 50 : value;
            }
        }
    }
}
