namespace Alma.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CacheableAttribute : Attribute
    {
        public string Key { get; }

        public int SlidingExpiration { get; }

        public CacheableAttribute(string key, int slidingExpiration = 0)
        {
            Key = key;
            SlidingExpiration = slidingExpiration;
        }
    }
}