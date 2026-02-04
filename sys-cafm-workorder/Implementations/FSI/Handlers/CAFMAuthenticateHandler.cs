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
/// Handler for CAFM Authenticate operation.
/// Maps API request to downstream request and invokes the atomic handler.
/// </summary>
public class CAFMAuthenticateHandler : IBaseHandler<AuthenticateRequestDto>
{
    private readonly CAFMAuthenticateAtomicHandler _atomicHandler;
    private readonly AppConfigs _appConfigs;
    private readonly ILogger<CAFMAuthenticateHandler> _logger;

    public CAFMAuthenticateHandler(
        CAFMAuthenticateAtomicHandler atomicHandler,
        IOptions<AppConfigs> appConfigs,
        ILogger<CAFMAuthenticateHandler> logger)
    {
        _atomicHandler = atomicHandler;
        _appConfigs = appConfigs.Value;
        _logger = logger;
    }

    public async Task<BaseResponseDTO> HandleAsync(AuthenticateRequestDto requestDTO)
    {
        _logger.Info("Processing CAFM Authentication request");

        // Map API request to downstream request
        var downstreamRequest = new CAFMAuthenticateDownstreamRequestDto
        {
            LoginName = string.IsNullOrWhiteSpace(requestDTO.LoginName) 
                ? _appConfigs.CAFM.Username 
                : requestDTO.LoginName,
            Password = string.IsNullOrWhiteSpace(requestDTO.Password) 
                ? _appConfigs.CAFM.Password 
                : requestDTO.Password,
            Url = _appConfigs.CAFM.GetFullUrl(),
            SoapAction = _appConfigs.CAFM.SoapActions.Authenticate
        };

        // Call atomic handler
        var downstreamResponse = await _atomicHandler.Handle(downstreamRequest);

        // Map downstream response to API response
        var apiResponse = new AuthenticateResponseDto
        {
            SessionId = downstreamResponse.SessionId,
            IsAuthenticated = downstreamResponse.IsSuccess
        };

        _logger.Info($"CAFM Authentication completed. IsAuthenticated: {apiResponse.IsAuthenticated}");

        return new BaseResponseDTO(
            message: "CAFM authentication successful",
            errorCode: string.Empty,
            data: apiResponse);
    }
}
