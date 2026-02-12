namespace SysNetworkMgmt.DTO.HandlerDTOs.NetworkTestDTO
{
    /// <summary>
    /// Response DTO for Network Test operation.
    /// Contains the result of the network connectivity test.
    /// </summary>
    public class NetworkTestResDTO
    {
        /// <summary>
        /// Gets or sets the test result message.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the timestamp of the test.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the environment name.
        /// </summary>
        public string Environment { get; set; } = string.Empty;

        /// <summary>
        /// Maps the internal response to the response DTO.
        /// </summary>
        /// <param name="message">The success message.</param>
        /// <param name="environment">The current environment name.</param>
        /// <returns>A NetworkTestResDTO with mapped values.</returns>
        public static NetworkTestResDTO Map(string message, string environment)
        {
            return new NetworkTestResDTO
            {
                Message = message,
                Timestamp = DateTime.UtcNow,
                Environment = environment
            };
        }
    }
}
