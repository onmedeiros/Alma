using System.Linq.Expressions;

namespace Alma.Core.Data
{
    public interface IFilterOrder<T> where T : class
    {
        bool Descending { get; }
        IOrderedQueryable<T> ApplyOrdering(IQueryable<T> query);
        IOrderedQueryable<T> ApplyThenBy(IOrderedQueryable<T> query);
    }

    public class FilterOrder<TEntity, TKey> : IFilterOrder<TEntity>
        where TEntity : class
    {
        public required Expression<Func<TEntity, TKey>> KeySelector { get; set; }
        public bool Descending { get; set; }

        public IOrderedQueryable<TEntity> ApplyOrdering(IQueryable<TEntity> query)
        {
            return Descending
                ? query.OrderByDescending(KeySelector)
                : query.OrderBy(KeySelector);
        }

        public IOrderedQueryable<TEntity> ApplyThenBy(IOrderedQueryable<TEntity> query)
        {
            return Descending
                ? query.ThenByDescending(KeySelector)
                : query.ThenBy(KeySelector);
        }
    }
}