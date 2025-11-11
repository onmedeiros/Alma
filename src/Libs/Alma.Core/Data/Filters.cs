using Alma.Core.Entities;
using System.Linq.Expressions;

namespace Alma.Core.Data
{
    /// <summary>
    /// Generic filters class for building complex queries with Where conditions and OrderBy clauses.
    /// </summary>
    /// <typeparam name="T">The entity type to filter</typeparam>
    /// <example>
    /// <code>
    /// var filters = new Filters&lt;FlowDefinition&gt;()
    ///     .Where(x => x.Name.Contains("test"))
    ///     .Where(x => x.Discriminator == "org123")
    ///     .OrderBy(x => x.Name)
    ///     .OrderByDescending(x => x.CreatedAt);
    /// 
    /// var result = await repository.GetPagedAsync(1, 10, filters);
    /// </code>
    /// </example>
    public class Filters<T>
        where T : class
    {
        private ICollection<Expression<Func<T, bool>>> _conditions = [];
        private ICollection<IFilterOrder<T>> _orderBys = [];

        public IEnumerable<Expression<Func<T, bool>>> Conditions => _conditions;
        public IEnumerable<IFilterOrder<T>> OrderBys => _orderBys;

        /// <summary>
        /// Adds a Where condition to the filter.
        /// </summary>
        /// <param name="condition">The predicate expression to filter by</param>
        /// <returns>The current Filters instance for method chaining</returns>
        public Filters<T> Where(Expression<Func<T, bool>> condition)
        {
            _conditions.Add(condition);
            return this;
        }

        /// <summary>
        /// Adds an OrderBy clause to the filter.
        /// </summary>
        /// <typeparam name="TKey">The type of the property to order by</typeparam>
        /// <param name="keySelector">The property selector expression</param>
        /// <param name="descending">Whether to sort in descending order</param>
        /// <returns>The current Filters instance for method chaining</returns>
        public Filters<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelector, bool descending = false)
        {
            _orderBys.Add(new FilterOrder<T, TKey>
            {
                KeySelector = keySelector,
                Descending = descending
            });
            return this;
        }

        /// <summary>
        /// Adds an OrderByDescending clause to the filter.
        /// </summary>
        /// <typeparam name="TKey">The type of the property to order by</typeparam>
        /// <param name="keySelector">The property selector expression</param>
        /// <returns>The current Filters instance for method chaining</returns>
        public Filters<T> OrderByDescending<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            return OrderBy(keySelector, descending: true);
        }

        /// <summary>
        /// Applies all filters and ordering to the provided query.
        /// </summary>
        /// <param name="query">The IQueryable to apply filters to</param>
        /// <returns>The filtered and ordered query</returns>
        public IQueryable<T> Apply(IQueryable<T> query)
        {
            // Apply Where conditions
            foreach (var condition in _conditions)
            {
                query = query.Where(condition);
            }

            // Apply OrderBy
            if (_orderBys.Any())
            {
                IOrderedQueryable<T>? orderedQuery = null;
                bool isFirst = true;

                foreach (var order in _orderBys)
                {
                    if (isFirst)
                    {
                        orderedQuery = order.ApplyOrdering(query);
                        isFirst = false;
                    }
                    else
                    {
                        orderedQuery = order.ApplyThenBy(orderedQuery!);
                    }
                }

                query = orderedQuery!;
            }

            return query;
        }
    }
}