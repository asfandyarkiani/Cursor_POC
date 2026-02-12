using Core.DTOs;
using Core.Extensions;
using Core.SystemLayer.Handlers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using sys_cafm_workorder.ConfigModels;
using sys_cafm_workorder.DTOs.Api.CAFM;
using sys_cafm_workorder.DTOs.Downstream.CAFM;
using sys_cafm_workorder.Implementations.FSI.AtomicHandlers;

namespace sys_cafm_workorder.Implementations.FSI.Handlers;

/// <summary>
/// Handler for CAFM Logout operation.
/// Maps API request to downstream request and invokes the atomic handler.
/// </summary>
public class CAFMLogoutHandler : IBaseHandler<LogoutRequestDto>
{
    private readonly CAFMLogoutAtomicHandler _atomicHandler;
    private readonly AppConfigs _appConfigs;
    private readonly ILogger<CAFMLogoutHandler> _logger;

    public CAFMLogoutHandler(
        CAFMLogoutAtomicHandler atomicHandler,
        IOptions<AppConfigs> appConfigs,
        ILogger<CAFMLogoutHandler> logger)
    {
        _atomicHandler = atomicHandler;
        _appConfigs = appConfigs.Value;
        _logger = logger;
    }

    public async Task<BaseResponseDTO> HandleAsync(LogoutRequestDto requestDTO)
    {
        _logger.Info($"Processing CAFM Logout request for session");

        // Validate request
        requestDTO.ValidateAPIRequestParameters();

        // Map API request to downstream request
        var downstreamRequest = new CAFMLogoutDownstreamRequestDto
        {
            SessionId = requestDTO.SessionId,
            Url = _appConfigs.CAFM.GetFullUrl(),
            SoapAction = _appConfigs.CAFM.SoapActions.Logout
        };

        // Call atomic handler
        var success = await _atomicHandler.Handle(downstreamRequest);

        _logger.Info($"CAFM Logout completed. Success: {success}");

        return new BaseResponseDTO(
            message: "CAFM logout successful",
            errorCode: string.Empty,
            data: new { Success = success });
    }
}
