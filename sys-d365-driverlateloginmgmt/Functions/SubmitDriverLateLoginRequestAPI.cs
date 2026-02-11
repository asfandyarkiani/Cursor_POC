using Core.Constants;
using Core.DTOs;
using Core.Exceptions;
using Core.Extensions;
using AGI.SysD365DriverLateLoginMgmt.Abstractions;
using AGI.SysD365DriverLateLoginMgmt.Attributes;
using AGI.SysD365DriverLateLoginMgmt.DTO.SubmitDriverLateLoginDTO;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace AGI.SysD365DriverLateLoginMgmt.Functions;

/// <summary>
/// Azure Function for submitting driver late login request to D365
/// HTTP-triggered entry point for Process Layer
/// </summary>
public class SubmitDriverLateLoginRequestAPI
{
    private readonly ILogger<SubmitDriverLateLoginRequestAPI> _logger;
    private readonly IDriverLateLoginMgmt _driverLateLoginMgmt;

    public SubmitDriverLateLoginRequestAPI(
        ILogger<SubmitDriverLateLoginRequestAPI> logger,
        IDriverLateLoginMgmt driverLateLoginMgmt)
    {
        _logger = logger;
        _driverLateLoginMgmt = driverLateLoginMgmt;
    }

    /// <summary>
    /// HTTP trigger for submitting driver late login request
    /// </summary>
    /// <param name="req">HTTP request</param>
    /// <param name="context">Function context</param>
    /// <returns>Base response DTO with late login result</returns>
    [D365Authentication]
    [Function("SubmitDriverLateLoginRequest")]
    public async Task<BaseResponseDTO> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "driver/latelogin")] HttpRequest req,
        FunctionContext context)
    {
        _logger.Info("HTTP trigger received for Submit Driver Late Login Request");

        // Read request body
        SubmitDriverLateLoginReqDTO? lateLoginRequest = await req.ReadBodyAsync<SubmitDriverLateLoginReqDTO>();

        if (lateLoginRequest == null)
        {
            _logger.Error("Request body is null or invalid");
            throw new NoRequestBodyException(
                errorDetails: [ErrorCodes.REQ_BODY_MISSING_OR_EMPTY.Message],
                stepName: "SubmitDriverLateLoginRequestAPI.cs / Run"
            );
        }
        else
        {
            // Validate request parameters
            lateLoginRequest.ValidateAPIRequestParameters();

            _logger.Info($"Request validated successfully - DriverId: {lateLoginRequest.DriverId}, CompanyCode: {lateLoginRequest.CompanyCode}");

            // Delegate to service
            BaseResponseDTO result = await _driverLateLoginMgmt.SubmitDriverLateLoginRequest(lateLoginRequest);

            _logger.Info("Submit Driver Late Login Request completed successfully");

            return result;
        }
    }
}
