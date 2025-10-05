using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Alma.Core.Types;

namespace SimpleCore.Data.Mongo.Extensions
{
    public static class MongoQueryableExtensions
    {
        public static async Task<PagedList<T>> ToPagedListAsync<T>(this IQueryable<T> query, int pageIndex, int pageSize)
        {
            // Count
            var count = await query.CountAsync();

            // Create result
            pageIndex = pageIndex == 0 ? 1 : pageIndex;
            pageSize = pageSize == 0 || pageSize == int.MaxValue ? count : pageSize;

            var result = new PagedList<T>
            {
                PageIndex = pageIndex,
                PageSize = pageSize,
                TotalCount = count
            };

            // Return if count = 0
            if (count == 0) return result;

            // Paginate if pageSize != count
            if (pageSize != count)
                query = query.Skip((pageIndex - 1) * pageSize).Take(pageSize);

            // Return result
            result.AddRange(await query.ToListAsync());

            return result;
        }
    }
}
