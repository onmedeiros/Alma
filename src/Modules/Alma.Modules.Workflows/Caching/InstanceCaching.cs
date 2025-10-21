using Microsoft.Extensions.Caching.Distributed;

namespace Alma.Modules.Workflows.Caching
{
    public static class InstanceCaching
    {
        public static readonly DistributedCacheEntryOptions Options
            = new DistributedCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(30))
            .SetAbsoluteExpiration(TimeSpan.FromHours(1));

        public const string INSTANCE_NAME_BY_ID_ORGANIZATION = "Alma.Modules.Workflows.Instance.ById-{0}-Organization-{1}";
    }
}
