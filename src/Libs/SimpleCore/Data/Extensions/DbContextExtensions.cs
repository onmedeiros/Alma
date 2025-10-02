using Microsoft.EntityFrameworkCore;
using Alma.Core.Entities;
using Alma.Core.Types;

namespace SimpleCore.Data.Extensions
{
    public static class DbContextExtensions
    {
        public static async Task<TEntity> Delete<TEntity>(this DbContext context, string id)
           where TEntity : Entity
        {
            var entity = await context.Set<TEntity>().FindAsync(id);

            if (entity == null)
                throw new Exception("The Entity cannot be found.");

            entity.Deleted = true;

            await context.SaveChangesAsync();

            return entity;
        }

        public static IQueryable<TEntity> AsQueryable<TEntity>(this DbContext context, bool tracking = false)
            where TEntity : Entity
        {
            var query = context.Set<TEntity>().Where(x => !x.Deleted);

            if (tracking)
                return query.AsTracking();

            return query.AsNoTracking();
        }

        public static Task<PagedList<TEntity>> Get<TEntity>(this DbContext context, bool tracking = false) where TEntity : Entity
        {
            return context.Get<TEntity>(null, tracking);
        }

        public static Task<PagedList<TEntity>> Get<TEntity>(this DbContext context, IQueryable<TEntity>? query, bool tracking = false) where TEntity : Entity
        {
            return context.Get(1, 0, query, tracking);
        }

        public static Task<PagedList<TEntity>> Get<TEntity>(this DbContext context, int pageIndex, int pageSize, bool tracking = false) where TEntity : Entity
        {
            return context.Get<TEntity>(pageIndex, pageSize, null, tracking);
        }

        public static async Task<PagedList<TEntity>> Get<TEntity>(this DbContext context, int pageIndex, int pageSize, IQueryable<TEntity>? query, bool tracking = false)
            where TEntity : Entity
        {
            query ??= context.AsQueryable<TEntity>(tracking);

            // Count
            var count = await query.CountAsync();

            // Create result
            pageIndex = pageIndex == 0 ? 1 : pageIndex;
            pageSize = pageSize == 0 || pageSize == int.MaxValue ? count : pageSize;

            var result = new PagedList<TEntity>
            {
                TotalCount = count,
                PageIndex = pageIndex,
                PageSize = pageSize
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

        public static async Task<PagedList<TEntity>> ToPagedList<TEntity>(this IQueryable<TEntity> query, int pageIndex, int pageSize)
        {
            // Count
            var count = await query.CountAsync();

            // Create result
            pageIndex = pageIndex == 0 ? 1 : pageIndex;
            pageSize = pageSize == 0 || pageSize == int.MaxValue ? count : pageSize;

            var result = new PagedList<TEntity>
            {
                TotalCount = count,
                PageIndex = pageIndex,
                PageSize = pageSize
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

        public static bool IsBeingTracked<TEntity>(this DbContext context, TEntity entity)
            where TEntity : Entity
        {
            return context.ChangeTracker.Entries<TEntity>().Any(x => x.Entity.Id == entity.Id);
        }

        //public static Task<TEntity?> EnsureTracking<TEntity>(this DbContext context, TEntity entity)
        //    where TEntity : Entity
        //{
        //    return context.FindAsync<TEntity>(entity.Id);
        //}
    }
}
