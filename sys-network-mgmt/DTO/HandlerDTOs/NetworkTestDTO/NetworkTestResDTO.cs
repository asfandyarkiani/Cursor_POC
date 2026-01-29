namespace SysNetworkMgmt.DTO.HandlerDTOs.NetworkTestDTO
{
    /// <summary>
    /// Response DTO for Network Test operation.
    /// Contains the result of the network connectivity test.
    /// </summary>
    public class NetworkTestResDTO
    {
        /// <summary>
        /// The test result message.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Indicates whether the test was successful.
        /// </summary>
        public bool IsSuccessful { get; set; }

        /// <summary>
        /// Timestamp when the test was executed.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Correlation ID for request tracking.
        /// </summary>
        public string? CorrelationId { get; set; }

        /// <summary>
        /// Application version information.
        /// </summary>
        public string? Version { get; set; }

        /// <summary>
        /// Environment name.
        /// </summary>
        public string? Environment { get; set; }

        /// <summary>
        /// Maps to a successful NetworkTestResDTO.
        /// </summary>
        /// <param name="message">The success message.</param>
        /// <param name="correlationId">Optional correlation ID.</param>
        /// <param name="version">Optional version information.</param>
        /// <param name="environment">Optional environment name.</param>
        /// <returns>A new NetworkTestResDTO with success status.</returns>
        public static NetworkTestResDTO MapSuccess(
            string message,
            string? correlationId = null,
            string? version = null,
            string? environment = null)
        {
            return new NetworkTestResDTO
            {
                Message = message,
                IsSuccessful = true,
                Timestamp = DateTime.UtcNow,
                CorrelationId = correlationId,
                Version = version,
                Environment = environment
            };
        }
    }
}
