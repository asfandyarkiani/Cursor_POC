namespace SysCafmMgmt.Middleware
{
    /// <summary>
    /// Attribute to mark functions or handlers that require FSI authentication
    /// When applied, the authentication middleware will ensure a valid session exists
    /// before executing the handler, and will logout after completion
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class RequiresFsiAuthenticationAttribute : Attribute
    {
        /// <summary>
        /// If true, logout will be called automatically after the operation completes
        /// Default is true
        /// </summary>
        public bool AutoLogout { get; set; } = true;

        /// <summary>
        /// If true, authentication failures will throw exceptions
        /// If false, the handler will be called without authentication
        /// Default is true
        /// </summary>
        public bool ThrowOnAuthFailure { get; set; } = true;
    }
}
