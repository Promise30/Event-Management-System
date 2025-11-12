using Event_Management_System.API.Domain.DTOs;
using Event_Management_System.API.Domain.Entities;
using Event_Management_System.API.Helpers;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Linq.Expressions;

namespace Event_Management_System.API.Infrastructures.Repositories
{
    public interface IDatabaseRepository<T, Tkey> where T : BaseEntity<Tkey>
    {
        Task<PagedList<T>> GetAllPaginatedAsync(RequestParameters parameter, Expression<Func<T, bool>> filter = null);

        Task<PagedList<T>> GetAllPaginatedAsync(RequestParameters parameter, SortParameters sortParams, Expression<Func<T, bool>> filter = null);
        Task<PagedList<T>> GetAllPaginatedAsync(RequestParameters parameter, List<T> entity, Expression<Func<T, bool>> filter = null);


    }
}
