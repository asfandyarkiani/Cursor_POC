using Core.DTOs;
using Core.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SysNetworkMgmt.Abstractions;
using SysNetworkMgmt.DTO.HandlerDTOs.NetworkTestDTO;

namespace SysNetworkMgmt.Functions
{
    /// <summary>
    /// Azure Function for Network Test operation.
    /// Provides a health check endpoint to verify network connectivity.
    /// </summary>
    public class NetworkTestAPI
    {
        private readonly ILogger<NetworkTestAPI> _logger;
        private readonly INetworkMgmt _networkMgmt;

        /// <summary>
        /// Initializes a new instance of the NetworkTestAPI class.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        /// <param name="networkMgmt">The network management service.</param>
        public NetworkTestAPI(
            ILogger<NetworkTestAPI> logger,
            INetworkMgmt networkMgmt)
        {
            _logger = logger;
            _networkMgmt = networkMgmt;
        }

        /// <summary>
        /// Executes a network connectivity test.
        /// This is a simple health check endpoint that returns a success message.
        /// </summary>
        /// <param name="req">The HTTP request.</param>
        /// <param name="context">The function execution context.</param>
        /// <returns>A BaseResponseDTO containing the test result.</returns>
        [Function("NetworkTest")]
        public async Task<BaseResponseDTO> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "network/test")] HttpRequest req,
            FunctionContext context)
        {
            _logger.Info("HTTP trigger received for Network Test.");

            // Create request DTO - no body required for health check
            var request = new NetworkTestReqDTO
            {
                RequestId = req.Headers["X-Request-Id"].FirstOrDefault()
            };

            // Validate request parameters (no mandatory params for health check)
            request.ValidateAPIRequestParameters();

            // Delegate to service
            BaseResponseDTO result = await _networkMgmt.NetworkTest(request);

            return result;
        }
    }
}
