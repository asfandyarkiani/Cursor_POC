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
    /// Handler for Network Test operation.
    /// This is a simple health check that returns a success message.
    /// No downstream API calls are required for this operation.
    /// </summary>
    public class NetworkTestHandler : IBaseHandler<NetworkTestReqDTO>
    {
        private readonly ILogger<NetworkTestHandler> _logger;
        private readonly AppConfigs _appConfigs;

        /// <summary>
        /// Initializes a new instance of the NetworkTestHandler class.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        /// <param name="appConfigs">The application configuration options.</param>
        public NetworkTestHandler(
            ILogger<NetworkTestHandler> logger,
            IOptions<AppConfigs> appConfigs)
        {
            _logger = logger;
            _appConfigs = appConfigs.Value;
        }

        /// <inheritdoc/>
        public async Task<BaseResponseDTO> HandleAsync(NetworkTestReqDTO request)
        {
            _logger.Info("[System Layer]-Initiating Network Test");

            // This is a simple health check operation
            // No downstream API calls are required
            // Return the success message directly

            var responseData = NetworkTestResDTO.Map(
                message: InfoConstants.NETWORK_TEST_SUCCESS,
                environment: _appConfigs.Environment
            );

            _logger.Info("[System Layer]-Network Test Completed Successfully");

            return await Task.FromResult(new BaseResponseDTO(
                message: InfoConstants.NETWORK_TEST_SUCCESS,
                data: responseData,
                errorCode: null!
            ));
        }
    }
}
