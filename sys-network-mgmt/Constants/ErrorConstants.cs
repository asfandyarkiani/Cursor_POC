namespace SysNetworkMgmt.Constants
{
    /// <summary>
    /// Error constants for the Network Management System Layer.
    /// </summary>
    public static class ErrorConstants
    {
        /// <summary>
        /// Error when network test fails.
        /// </summary>
        public static readonly (string ErrorCode, string Message) NETWORK_TEST_FAILURE =
            ("NET_SYS_0001", "Network test operation failed.");

        /// <summary>
        /// Error when service is unavailable.
        /// </summary>
        public static readonly (string ErrorCode, string Message) SERVICE_UNAVAILABLE =
            ("NET_SYS_0002", "The network service is currently unavailable.");

        /// <summary>
        /// Error when request validation fails.
        /// </summary>
        public static readonly (string ErrorCode, string Message) VALIDATION_FAILURE =
            ("NET_SYS_0003", "Request validation failed.");
    }
}
