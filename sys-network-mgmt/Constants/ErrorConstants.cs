namespace SysNetworkMgmt.Constants
{
    /// <summary>
    /// Error constants for sys-network-mgmt System Layer API.
    /// </summary>
    public static class ErrorConstants
    {
        /// <summary>
        /// Error when network test operation fails.
        /// </summary>
        public static readonly (string ErrorCode, string Message) NETWORK_TEST_FAILURE =
            ("SYS_NET_0001", "Network test operation failed.");

        /// <summary>
        /// Error when request validation fails.
        /// </summary>
        public static readonly (string ErrorCode, string Message) SYS_VAL_0001 =
            ("SYS_NET_VAL_0001", "Request validation failed.");
    }
}
