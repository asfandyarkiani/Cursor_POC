
namespace Cache.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CacheableServiceAttribute : Attribute
    {
        /// <summary>
        /// Optional cache namespace for grouping cache keys.
        /// </summary>
        public string? Namespace { get; set; } = null;

        public CacheableServiceAttribute() { }
    }
}
