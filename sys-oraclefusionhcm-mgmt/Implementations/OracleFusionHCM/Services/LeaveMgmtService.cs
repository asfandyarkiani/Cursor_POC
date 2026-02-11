using AlGhurair.Core.SystemLayer.Handlers;
using AlGhurair.SystemLayer.OracleFusionHCM.Abstractions;
using AlGhurair.SystemLayer.OracleFusionHCM.DTO.CreateLeaveDTO;
using Microsoft.Extensions.Logging;

namespace AlGhurair.SystemLayer.OracleFusionHCM.Implementations.OracleFusionHCM.Services;

/// <summary>
/// Service implementation for Leave Management operations in Oracle Fusion HCM
/// Abstraction boundary that delegates to Handlers
/// </summary>
public class LeaveMgmtService : ILeaveMgmt
{
    private readonly IBaseHandler<CreateLeaveReqDTO, CreateLeaveResDTO> _createLeaveHandler;
    private readonly ILogger<LeaveMgmtService> _logger;

    public LeaveMgmtService(
        IBaseHandler<CreateLeaveReqDTO, CreateLeaveResDTO> createLeaveHandler,
        ILogger<LeaveMgmtService> logger)
    {
        _createLeaveHandler = createLeaveHandler ?? throw new ArgumentNullException(nameof(createLeaveHandler));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Creates a new leave/absence record in Oracle Fusion HCM
    /// Delegates to CreateLeaveHandler
    /// </summary>
    public async Task<CreateLeaveResDTO> CreateLeaveAsync(CreateLeaveReqDTO request)
    {
        _logger.LogInformation("LeaveMgmtService: CreateLeaveAsync called");
        
        CreateLeaveResDTO response = await _createLeaveHandler.HandleAsync(request);
        
        _logger.LogInformation($"LeaveMgmtService: CreateLeaveAsync completed with status: {response.Status}");
        
        return response;
    }
}
