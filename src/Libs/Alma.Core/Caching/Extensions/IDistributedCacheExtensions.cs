using Alma.Core.Caching;
using Microsoft.Extensions.Caching.Distributed;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Alma.Core.Caching.Extensions
{
    public static class IDistributedCacheExtensions
    {
        private static JsonSerializerOptions serializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = null,
            WriteIndented = true,
            AllowTrailingCommas = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public static Task SetAsync<T>(this IDistributedCache cache, string key, T value, DistributedCacheEntryOptions options)
        {
            var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(value, serializerOptions));
            return cache.SetAsync(key, bytes, options);
        }

        public static Task SetAsync<T>(this IDistributedCache cache, string key, T value)
        {
            return SetAsync(cache, key, value, new DistributedCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(30))
                .SetAbsoluteExpiration(TimeSpan.FromHours(1)));
        }

        public static async Task<CacheResult<T>> GetAsync<T>(this IDistributedCache cache, string key)
        {
            var bytes = await cache.GetAsync(key);

            if (bytes == null)
                return CacheResult<T>.NotCached();

            return CacheResult<T>.FromValue(JsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(bytes), serializerOptions));
        }

        public static async Task<T?> GetOrSetAsync<T>(this IDistributedCache cache, string key, Func<Task<T>> factory, DistributedCacheEntryOptions? options)
        {
            options ??= new DistributedCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(30))
                .SetAbsoluteExpiration(TimeSpan.FromHours(1));

            var result = await cache.GetAsync<T?>(key);

            if (!result.IsCached)
            {
                var cached = await factory();
                await cache.SetAsync(key, cached, options);

                return cached;
            }

            return result.Value;
        }

        public static Task<T?> GetOrSetAsync<T>(this IDistributedCache cache, string key, DistributedCacheEntryOptions? options, Func<Task<T>> factory)
        {
            return GetOrSetAsync(cache, key, factory, options);
        }
    }
}