using FacilitiesMgmtSystem.Constants;
using FacilitiesMgmtSystem.DTO;
using FacilitiesMgmtSystem.DTO.NetworkTestDTO;
using FacilitiesMgmtSystem.Helper;
using Microsoft.Extensions.Logging;

namespace FacilitiesMgmtSystem.Implementations.MRI.Handlers;

/// <summary>
/// Handler for Network Test operations.
/// This handler processes network connectivity test requests and returns success status.
/// </summary>
public class NetworkTestMRIHandler
{
    private readonly ILogger<NetworkTestMRIHandler> _logger;

    public NetworkTestMRIHandler(ILogger<NetworkTestMRIHandler> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Processes the network test request.
    /// </summary>
    /// <param name="request">The network test request DTO (optional).</param>
    /// <returns>A BaseResponseDTO with the test result.</returns>
    public Task<BaseResponseDTO> ProcessRequest(NetworkTestRequestDTO? request)
    {
        _logger.Info("Processing network test request. CorrelationId: {CorrelationId}", 
            request?.CorrelationId ?? "N/A");

        var response = new NetworkTestResponseDTO
        {
            Message = InfoConstants.NETWORK_TEST_SUCCESS,
            Timestamp = DateTime.UtcNow,
            CorrelationId = request?.CorrelationId
        };

        _logger.Info("Network test completed successfully.");

        return Task.FromResult(BaseResponseDTO.CreateSuccess(
            InfoConstants.NETWORK_TEST_SUCCESS, 
            response));
    }
}
