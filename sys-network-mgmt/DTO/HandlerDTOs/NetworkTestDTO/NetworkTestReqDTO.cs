using Core.SystemLayer.DTOs;

namespace SysNetworkMgmt.DTO.HandlerDTOs.NetworkTestDTO
{
    /// <summary>
    /// Request DTO for Network Test operation.
    /// This is a simple health check endpoint that doesn't require input parameters.
    /// </summary>
    public class NetworkTestReqDTO : IRequestSysDTO
    {
        /// <summary>
        /// Optional request identifier for tracking purposes.
        /// </summary>
        public string? RequestId { get; set; }

        /// <summary>
        /// Validates the API request parameters.
        /// For network test, no mandatory parameters are required.
        /// </summary>
        public void ValidateAPIRequestParameters()
        {
            // No mandatory parameters for health check endpoint
            // Request is always valid
        }
    }
}
