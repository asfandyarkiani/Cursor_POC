using Core.DTOs;
using Core.Extensions;
using AGI.SysD365DriverLateLoginMgmt.Abstractions;
using AGI.SysD365DriverLateLoginMgmt.DTO.SubmitDriverLateLoginDTO;
using AGI.SysD365DriverLateLoginMgmt.Implementations.D365.Handlers;
using Microsoft.Extensions.Logging;

namespace AGI.SysD365DriverLateLoginMgmt.Implementations.D365.Services;

/// <summary>
/// Service for Driver Late Login Management operations
/// Implements IDriverLateLoginMgmt interface
/// Delegates to Handlers for business operations
/// </summary>
public class DriverLateLoginMgmtService : IDriverLateLoginMgmt
{
    private readonly ILogger<DriverLateLoginMgmtService> _logger;
    private readonly SubmitDriverLateLoginHandler _submitDriverLateLoginHandler;

    public DriverLateLoginMgmtService(
        ILogger<DriverLateLoginMgmtService> logger,
        SubmitDriverLateLoginHandler submitDriverLateLoginHandler)
    {
        _logger = logger;
        _submitDriverLateLoginHandler = submitDriverLateLoginHandler;
    }

    /// <summary>
    /// Submits a driver late login request to D365
    /// </summary>
    /// <param name="request">Late login request details</param>
    /// <returns>Base response DTO with late login result</returns>
    public async Task<BaseResponseDTO> SubmitDriverLateLoginRequest(SubmitDriverLateLoginReqDTO request)
    {
        _logger.Info("DriverLateLoginMgmtService.SubmitDriverLateLoginRequest called");
        return await _submitDriverLateLoginHandler.HandleAsync(request);
    }
}
