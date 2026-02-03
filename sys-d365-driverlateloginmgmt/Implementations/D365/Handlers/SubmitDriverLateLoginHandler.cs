using Core.DTOs;
using Core.Extensions;
using Core.SystemLayer.Exceptions;
using Core.SystemLayer.Handlers;
using Core.SystemLayer.Middlewares;
using AGI.SysD365DriverLateLoginMgmt.Constants;
using AGI.SysD365DriverLateLoginMgmt.DTO.AtomicHandlerDTOs;
using AGI.SysD365DriverLateLoginMgmt.DTO.DownstreamDTOs;
using AGI.SysD365DriverLateLoginMgmt.DTO.SubmitDriverLateLoginDTO;
using AGI.SysD365DriverLateLoginMgmt.Helper;
using AGI.SysD365DriverLateLoginMgmt.Implementations.D365.AtomicHandlers;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace AGI.SysD365DriverLateLoginMgmt.Implementations.D365.Handlers;

/// <summary>
/// Handler for submitting driver late login request to D365
/// Orchestrates SubmitLateLoginAtomicHandler
/// </summary>
public class SubmitDriverLateLoginHandler : IBaseHandler<SubmitDriverLateLoginReqDTO>
{
    private readonly ILogger<SubmitDriverLateLoginHandler> _logger;
    private readonly SubmitLateLoginAtomicHandler _submitLateLoginAtomicHandler;
    private readonly RequestContext _requestContext;

    public SubmitDriverLateLoginHandler(
        ILogger<SubmitDriverLateLoginHandler> logger,
        SubmitLateLoginAtomicHandler submitLateLoginAtomicHandler,
        RequestContext requestContext)
    {
        _logger = logger;
        _submitLateLoginAtomicHandler = submitLateLoginAtomicHandler;
        _requestContext = requestContext;
    }

    /// <summary>
    /// Handles the submit driver late login request
    /// </summary>
    /// <param name="requestDTO">Request DTO from Process Layer</param>
    /// <returns>Base response DTO with late login result</returns>
    public async Task<BaseResponseDTO> HandleAsync(SubmitDriverLateLoginReqDTO requestDTO)
    {
        _logger.Info("[System Layer]-Initiating Submit Driver Late Login");
        _logger.Info($"DriverId: {requestDTO.DriverId}, CompanyCode: {requestDTO.CompanyCode}");

        // Call atomic handler to submit late login request to D365
        HttpResponseSnapshot d365Response = await SubmitLateLoginToDownstream(requestDTO);

        if (!d365Response.IsSuccessStatusCode)
        {
            _logger.Error($"D365 late login API call failed - Status: {d365Response.StatusCode}, Response: {d365Response.Content}");
            throw new DownStreamApiFailureException(
                statusCode: (HttpStatusCode)d365Response.StatusCode,
                error: ErrorConstants.D365_LATLOG_0001,
                errorDetails: [$"Status: {d365Response.StatusCode}", $"Response: {d365Response.Content}"],
                stepName: "SubmitDriverLateLoginHandler.cs / HandleAsync"
            );
        }
        else
        {
            // Deserialize D365 API response
            SubmitLateLoginApiResDTO? d365ApiResponse = JsonSerializer.Deserialize<SubmitLateLoginApiResDTO>(
                d365Response.Content ?? "{}",
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            if (d365ApiResponse == null || d365ApiResponse.Messages == null || d365ApiResponse.Messages.Count == 0)
            {
                _logger.Error("D365 API response is null or empty");
                throw new DownStreamApiFailureException(
                    statusCode: HttpStatusCode.InternalServerError,
                    error: ErrorConstants.D365_NETWRK_0003,
                    errorDetails: ["D365 API returned empty or invalid response"],
                    stepName: "SubmitDriverLateLoginHandler.cs / HandleAsync"
                );
            }
            else
            {
                D365MessageDTO d365Message = d365ApiResponse.Messages[0];

                // Check if D365 returned success
                bool isSuccess = d365Message.Success?.ToLower() == "true";

                if (!isSuccess)
                {
                    _logger.Warn($"D365 late login request failed - Message: {d365Message.Message}");
                    throw new DownStreamApiFailureException(
                        statusCode: HttpStatusCode.BadRequest,
                        error: ErrorConstants.D365_LATLOG_0002,
                        errorDetails: [$"D365 Error: {d365Message.Message}"],
                        stepName: "SubmitDriverLateLoginHandler.cs / HandleAsync"
                    );
                }
                else
                {
                    _logger.Info("[System Layer]-Completed Submit Driver Late Login");

                    // Map D365 response to System Layer response DTO
                    SubmitDriverLateLoginResDTO responseDTO = SubmitDriverLateLoginResDTO.CreateSuccessResponse(
                        message: d365Message.Message,
                        reference: d365Message.Reference,
                        inputReference: d365Message.InputReference
                    );

                    return new BaseResponseDTO(
                        message: InfoConstants.LATE_LOGIN_REQUEST_SUCCESS,
                        data: responseDTO,
                        errorCode: null
                    );
                }
            }
        }
    }

    /// <summary>
    /// Calls the atomic handler to submit late login request to D365
    /// </summary>
    /// <param name="request">Request DTO from Process Layer</param>
    /// <returns>HTTP response snapshot from D365 API</returns>
    private async Task<HttpResponseSnapshot> SubmitLateLoginToDownstream(SubmitDriverLateLoginReqDTO request)
    {
        // Build atomic handler request DTO
        SubmitLateLoginHandlerReqDTO atomicHandlerRequest = new SubmitLateLoginHandlerReqDTO
        {
            DriverId = request.DriverId,
            RequestDateTime = request.RequestDateTime,
            CompanyCode = request.CompanyCode,
            ReasonCode = request.ReasonCode,
            Remarks = request.Remarks,
            RequestNo = request.RequestNo,
            AuthorizationToken = _requestContext.AuthorizationToken ?? string.Empty
        };

        return await _submitLateLoginAtomicHandler.Handle(atomicHandlerRequest);
    }
}
