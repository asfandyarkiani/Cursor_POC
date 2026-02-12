using Core.DTOs;
using Core.Extensions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using sys_cafm_workorder.Abstractions;
using sys_cafm_workorder.DTOs.Api.CAFM;
using System.Net;
using System.Text.Json;

namespace sys_cafm_workorder.Functions.CAFM;

/// <summary>
/// Azure Function for CAFM Authentication.
/// POST /api/cafm/authenticate
/// </summary>
public class CAFMAuthenticateFunction
{
    private readonly ICAFMAuthenticationService _authService;
    private readonly ILogger<CAFMAuthenticateFunction> _logger;

    public CAFMAuthenticateFunction(
        ICAFMAuthenticationService authService,
        ILogger<CAFMAuthenticateFunction> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [Function("CAFMAuthenticate")]
    public async Task<BaseResponseDTO> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "cafm/authenticate")] HttpRequestData req,
        FunctionContext context)
    {
        _logger.Info("CAFMAuthenticate function triggered");

        // Store HttpResponseData in context for middleware
        var response = req.CreateResponse(HttpStatusCode.OK);
        context.Items[typeof(HttpResponseData).Name] = response;

        // Read request body (optional - can use config defaults)
        var requestDto = await req.ReadBodyAsync<AuthenticateRequestDto>() 
            ?? new AuthenticateRequestDto();

        // Call service
        return await _authService.AuthenticateAsync(requestDto);
    }
}
