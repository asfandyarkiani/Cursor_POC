using Core.DTOs;
using Core.Extensions;
using Core.SystemLayer.Handlers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SysNetworkMgmt.ConfigModels;
using SysNetworkMgmt.Constants;
using SysNetworkMgmt.DTO.HandlerDTOs.NetworkTestDTO;

namespace SysNetworkMgmt.Implementations.Network.Handlers
{
    /// <summary>
    /// Handler for Network Test operations.
    /// This handler returns a successful test response indicating system health.
    /// Since the Boomi process doesn't make external API calls (just returns a message),
    /// this handler directly returns the success response without calling atomic handlers.
    /// </summary>
    public class NetworkTestHandler : IBaseHandler<NetworkTestReqDTO>
    {
        private readonly ILogger<NetworkTestHandler> _logger;
        private readonly AppConfigs _appConfigs;

        /// <summary>
        /// Initializes a new instance of the NetworkTestHandler.
        /// </summary>
        /// <param name="logger">Logger for logging operations.</param>
        /// <param name="options">Application configuration options.</param>
        public NetworkTestHandler(
            ILogger<NetworkTestHandler> logger,
            IOptions<AppConfigs> options)
        {
            _logger = logger;
            _appConfigs = options.Value;
        }

        /// <summary>
        /// Handles the network test request and returns a success response.
        /// This operation mirrors the Boomi process which simply returns "Test is successful!!!"
        /// without making any external API calls.
        /// </summary>
        /// <param name="requestDTO">The network test request DTO.</param>
        /// <returns>A BaseResponseDTO with the test result.</returns>
        public async Task<BaseResponseDTO> HandleAsync(NetworkTestReqDTO requestDTO)
        {
            _logger.Info("[System Layer]-Initiating Network Test");

            // Create the response DTO with success message
            // This mirrors the Boomi process: Start → Message("Test is successful!!!") → Return Documents
            var responseData = NetworkTestResDTO.MapSuccess(
                message: InfoConstants.NETWORK_TEST_SUCCESS,
                correlationId: requestDTO.CorrelationId,
                version: _appConfigs.Version,
                environment: _appConfigs.Environment
            );

            _logger.Info("[System Layer]-Network Test completed successfully");

            return await Task.FromResult(new BaseResponseDTO(
                message: InfoConstants.NETWORK_TEST_SUCCESS,
                data: responseData,
                errorCode: null
            ));
        }
    }
}
