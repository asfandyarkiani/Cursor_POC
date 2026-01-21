
using Core.Attributes;

namespace Cache.Enums
{
    /// <summary>
    /// Specifies the type of pattern matching to use when searching Redis keys.
    /// This enum can be used to determine whether a key search should match 
    /// keys that contain a substring, start with a prefix, or end with a suffix.
    /// </summary>
    public enum KeyMatchType
    {
        /// <summary>
        /// Match keys that contain the specified substring anywhere in the key.
        /// Example: "user:123" contains "123".
        /// </summary>
        [StringValue("Contains")]
        Contains,

        /// <summary>
        /// Match keys that start with the specified prefix.
        /// Example: "user:123" starts with "user:".
        /// </summary>
        [StringValue("StartsWith")]
        StartsWith,

        /// <summary>
        /// Match keys that end with the specified suffix.
        /// Example: "user:123" ends with "123".
        /// </summary>
        [StringValue("EndsWith")]
        EndsWith
    }
}