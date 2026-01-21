namespace Cache.Key
{
    public class CacheKeySettings
    {
        /// <summary>
        /// If not defined, then the key will be cached till memory of cache hits its max size
        /// </summary>
        public TimeSpan? TimeToLive { get; set; }
    }
}
