using Alma.Core.Entities;
using System.Linq.Expressions;

namespace Alma.Core.Data
{
    public interface IUpdate<T>
        where T : Entity
    {
        IUpdate<T> Where(Expression<Func<T, bool>> expression);

        IUpdate<T> Set<TValue>(Expression<Func<T, TValue>> expression, TValue value);

        bool HasChanges();

        public Task<T?> ExecuteAsync(bool ignoreMatchedCount = false);
    }
}