using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace SimpleCore.Data.Contexts
{
    public class SimpleCosmosContext : SimpleDbContext
    {
        public SimpleCosmosContext(ILogger<SimpleCosmosContext> logger, IMemoryCache memoryCache, DbContextOptions options) 
            : base(logger, memoryCache, options)
        {

        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    }
}