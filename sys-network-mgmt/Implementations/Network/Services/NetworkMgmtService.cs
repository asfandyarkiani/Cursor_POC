using Core.DTOs;
using Core.Extensions;
using Microsoft.Extensions.Logging;
using SysNetworkMgmt.Abstractions;
using SysNetworkMgmt.DTO.HandlerDTOs.NetworkTestDTO;
using SysNetworkMgmt.Implementations.Network.Handlers;

namespace SysNetworkMgmt.Implementations.Network.Services
{
    /// <summary>
    /// Service implementation for Network Management operations.
    /// Delegates operations to appropriate handlers.
    /// </summary>
    public class NetworkMgmtService : INetworkMgmt
    {
        private readonly ILogger<NetworkMgmtService> _logger;
        private readonly NetworkTestHandler _networkTestHandler;

        /// <summary>
        /// Initializes a new instance of the NetworkMgmtService.
        /// </summary>
        /// <param name="logger">Logger for logging operations.</param>
        /// <param name="networkTestHandler">Handler for network test operations.</param>
        public NetworkMgmtService(
            ILogger<NetworkMgmtService> logger,
            NetworkTestHandler networkTestHandler)
        {
            _logger = logger;
            _networkTestHandler = networkTestHandler;
        }

        /// <summary>
        /// Performs a network connectivity test by delegating to the handler.
        /// </summary>
        /// <param name="request">The network test request DTO.</param>
        /// <returns>A BaseResponseDTO containing the test result.</returns>
        public async Task<BaseResponseDTO> NetworkTest(NetworkTestReqDTO request)
        {
            _logger.Info("NetworkMgmtService.NetworkTest called");

            try
            {
                return await _networkTestHandler.HandleAsync(request);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to execute network test");
                throw;
            }
        }
    }
}
