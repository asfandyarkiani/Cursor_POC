using System.Reflection;

namespace Cache.DTO
{
    public interface ICacheable
    {

        /// <summary>
        /// Retrieves the properties and their values that make up the unique cache key for the object.
        /// </summary>
        /// <returns>
        /// An <see cref="IDictionary{TKey, TValue}"/> 
        /// </returns>
        
        public IDictionary<PropertyInfo, object> GetCacheKey();
    }
}
