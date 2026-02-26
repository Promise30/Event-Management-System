using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Event_Management_System.API.Helpers
{
    public class PagedList<T>
    {
        public MetaData MetaData { get; set; }

        public List<T> Data { get; set; }

        public PagedList(List<T> items, int count, int pageNumber, int pageSize)
        {
            MetaData = new MetaData
            {
                TotalCount = count,
                PageSize = pageSize,
                CurrentPage = pageNumber,
                TotalPages = (int)Math.Ceiling((double)count / (double)pageSize)
            };
            Data = items;
        }

        public static async Task<PagedList<T>> ToPagedList(IQueryable<T> source, Expression<Func<T, bool>> filter, RequestParameters parameter)
        {
            if (filter != null)
            {
                source = source.Where(filter);
            }

            return new PagedList<T>(count: await source.CountAsync(), items: await source.Skip((parameter.PageNumber - 1) * parameter.PageSize).Take(parameter.PageSize).ToListAsync(), pageNumber: parameter.PageNumber, pageSize: parameter.PageSize);
        }

        public static async Task<PagedList<T>> ToPagedListAsync<T>(List<T> source, RequestParameters parameter)
        {
            int count = source.Count;
            return await Task.FromResult(new PagedList<T>(source.Skip((parameter.PageNumber - 1) * parameter.PageSize).Take(parameter.PageSize).ToList(), count, parameter.PageNumber, parameter.PageSize));
        }
    }
}
