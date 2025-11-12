namespace Event_Management_System.API.Helpers
{
    public class PagedResponse<T>
    {
            public int TotalNoItems { get; set; }
            public int TotalPages { get; set; }
            public int PageNumber { get; set; }
            public int PageSize { get; set; }
            public int? NextPage { get; set; }
            public int? PreviousPage { get; set; }
            public T Data { get; set; }
        
    }
}
