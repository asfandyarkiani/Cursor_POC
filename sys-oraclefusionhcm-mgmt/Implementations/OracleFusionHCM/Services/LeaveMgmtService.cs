using Microsoft.Extensions.Logging;
using sys_oraclefusionhcm_mgmt.Abstractions;
using sys_oraclefusionhcm_mgmt.DTO.CreateLeaveDTO;
using sys_oraclefusionhcm_mgmt.Implementations.OracleFusionHCM.Handlers;

namespace sys_oraclefusionhcm_mgmt.Implementations.OracleFusionHCM.Services;

/// <summary>
/// Service implementation for Leave Management operations
/// Delegates to Handlers for business logic
/// </summary>
public class LeaveMgmtService : ILeaveMgmt
{
    private readonly ILogger<LeaveMgmtService> _logger;
    private readonly CreateLeaveHandler _createLeaveHandler;
    
    public LeaveMgmtService(
        ILogger<LeaveMgmtService> logger,
        CreateLeaveHandler createLeaveHandler)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _createLeaveHandler = createLeaveHandler ?? throw new ArgumentNullException(nameof(createLeaveHandler));
    }
    
    /// <summary>
    /// Creates a leave absence entry in Oracle Fusion HCM
    /// </summary>
    public async Task<CreateLeaveResDTO> CreateLeaveAsync(CreateLeaveReqDTO requestDto)
    {
        _logger.LogDebug("LeaveMgmtService.CreateLeaveAsync called");
        return await _createLeaveHandler.HandleAsync(requestDto);
    }
}
