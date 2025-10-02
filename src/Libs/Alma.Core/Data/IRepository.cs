using Alma.Core.Entities;
using Alma.Core.Types;
using System.Linq.Expressions;

namespace Alma.Core.Data
{
    public interface IRepository<T>
        where T : Entity
    {
        #region Operations

        Task InsertAsync(T entity);

        Task UpdateAsync(T entity);

        IUpdate<T> BeginUpdate();

        Task<T?> DeleteAsync(string id, bool hard = false);

        #endregion

        #region Queries

        IQueryable<T> AsQueryable();

        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);

        Task<T> GetByIdAsync(string id);

        Task<T?> GetOneAsync(Expression<Func<T, bool>> predicate);

        Task<List<T>> GetAllAsync();

        Task<List<T>> ToListAsync(IQueryable<T>? query = null);

        Task<PagedList<T>> GetPagedAsync(int page, int pageSize, IQueryable<T>? query = null);

        #endregion
    }
}