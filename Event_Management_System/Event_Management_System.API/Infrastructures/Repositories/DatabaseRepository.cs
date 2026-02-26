using Event_Management_System.API.Domain.Entities;
using Event_Management_System.API.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Linq.Expressions;

namespace Event_Management_System.API.Infrastructures.Repositories
{
    public class DatabaseRepository<T, Tkey> : IDatabaseRepository<T, Tkey> where T : BaseEntity<Tkey>
    {
        private readonly ILogger<DatabaseRepository<T, Tkey>> _logger;

        private readonly ApplicationDbContext _context;

        private readonly DbSet<T> _dbSet;

        public Tkey ID { get; set; }

        public DatabaseRepository(ApplicationDbContext context, ILogger<DatabaseRepository<T, Tkey>> logger)
        {
            _logger = logger;
            _context = context;
            _dbSet = context.Set<T>();
        }
        public async Task<PagedList<T>> GetAllPaginatedAsync(RequestParameters parameter, Expression<Func<T, bool>> filter = null)
        {
            return await PagedList<T>.ToPagedList(_dbSet.AsNoTracking(), filter, parameter);
        }

        public async Task<PagedList<T>> GetAllPaginatedAsync(RequestParameters parameter, SortParameters? sortParams, Expression<Func<T, bool>> filter = null)
        {
            IQueryable<T> source = _dbSet.AsNoTracking();
            if (sortParams != null)
            {
                if (sortParams.SortType == SortingType.Ascending)
                {
                    source = source.OrderBy((T x) => x.CreatedDate);
                }

                source = source.OrderByDescending((T x) => x.CreatedDate);
            }
            else
            {
                source = source.OrderByDescending((T x) => x.CreatedDate);
            }

            return await PagedList<T>.ToPagedList(source, filter, parameter);
        }

        //public async Task<PagedList<T>> GetAllPaginatedAsync(RequestParameters parameter, List<T> entity, Expression<Func<T, bool>> filter = null)
        //{
        //    return await PagedList<T>.ToPagedListAsync(entity, parameter);
        //}
        public async Task<PagedList<T>> GetAllPaginatedAsync(IQueryable<T> query, RequestParameters parameter, Expression<Func<T, bool>> filter = null)
        {
            return await PagedList<T>.ToPagedList(query.AsNoTracking(), filter, parameter);
        }

        public async Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>> filter, params Expression<Func<T, object>>[] includeProperties)
        {
            IQueryable<T> source = _dbSet.AsQueryable();
            if (filter != null)
            {
                source = source.Where(filter);
            }

            foreach (Expression<Func<T, object>> navigationPropertyPath in includeProperties)
            {
                source = source.Include(navigationPropertyPath);
            }

            return await source.AsNoTracking().ToListAsync();
        }

    }
}
