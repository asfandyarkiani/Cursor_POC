using Core.SystemLayer.DTOs;

namespace SysNetworkMgmt.DTO.HandlerDTOs.NetworkTestDTO
{
    /// <summary>
    /// Request DTO for Network Test operation.
    /// This is a simple health check operation that doesn't require input parameters.
    /// </summary>
    public class NetworkTestReqDTO : IRequestSysDTO
    {
        /// <summary>
        /// Optional correlation ID for request tracking.
        /// </summary>
        public string? CorrelationId { get; set; }

        /// <summary>
        /// Validates the API request parameters.
        /// Since this is a simple health check with no required parameters, validation always passes.
        /// </summary>
        public void ValidateAPIRequestParameters()
        {
            // No required parameters for network test operation
            // Validation passes by default
        }

        /// <summary>
        /// Creates a new NetworkTestReqDTO with an optional correlation ID.
        /// </summary>
        /// <param name="correlationId">Optional correlation ID for tracking.</param>
        /// <returns>A new NetworkTestReqDTO instance.</returns>
        public static NetworkTestReqDTO Create(string? correlationId = null)
        {
            return new NetworkTestReqDTO
            {
                CorrelationId = correlationId ?? Guid.NewGuid().ToString()
            };
        }
    }
}
