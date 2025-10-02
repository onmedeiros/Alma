namespace Alma.Core.Caching
{
    public class CacheResult<T>
    {
        public T? Value { get; set; }
        public bool IsCached { get; set; }

        public static CacheResult<T> FromValue(T? value) =>
            new CacheResult<T>
            {
                Value = value,
                IsCached = true
            };

        public static CacheResult<T> NotCached() =>
            new CacheResult<T>
            {
                Value = default,
                IsCached = false
            };
    }
}
