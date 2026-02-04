using Core.DTOs;
using Core.Exceptions;
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
    /// HTTP entry point for the System Layer network connectivity test.
    /// This function mirrors the Boomi "Network Test" process which returns "Test is successful!!!"
    /// </summary>
    public class NetworkTestAPI
    {
        private readonly ILogger<NetworkTestAPI> _logger;
        private readonly INetworkMgmt _networkMgmt;

        /// <summary>
        /// Initializes a new instance of the NetworkTestAPI.
        /// </summary>
        /// <param name="logger">Logger for logging operations.</param>
        /// <param name="networkMgmt">Network management service interface.</param>
        public NetworkTestAPI(
            ILogger<NetworkTestAPI> logger,
            INetworkMgmt networkMgmt)
        {
            _logger = logger;
            _networkMgmt = networkMgmt;
        }

        /// <summary>
        /// HTTP trigger function for network connectivity test.
        /// Supports both GET and POST methods for flexibility.
        /// Route: /api/network/test
        /// </summary>
        /// <param name="req">The HTTP request.</param>
        /// <returns>A BaseResponseDTO with the test result.</returns>
        [Function("NetworkTest")]
        public async Task<BaseResponseDTO> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "network/test")] HttpRequest req)
        {
            _logger.Info("HTTP trigger received for Network Test.");

            // Create request DTO
            // Since the Boomi process has inputType: "none", we create a minimal request DTO
            NetworkTestReqDTO request;

            // Try to read body if POST with content, otherwise create default
            if (req.Method.Equals("POST", StringComparison.OrdinalIgnoreCase) && req.ContentLength > 0)
            {
                try
                {
                    request = await req.ReadBodyAsync<NetworkTestReqDTO>() ?? NetworkTestReqDTO.Create();
                }
                catch (NoRequestBodyException)
                {
                    // For network test, empty body is acceptable - create default request
                    request = NetworkTestReqDTO.Create();
                }
            }
            else
            {
                // For GET requests or POST without body, create default request
                request = NetworkTestReqDTO.Create();
            }

            // Validate request parameters (no-op for this simple operation)
            request.ValidateAPIRequestParameters();

            // Delegate to service
            BaseResponseDTO result = await _networkMgmt.NetworkTest(request);

            return result;
        }
    }
}
