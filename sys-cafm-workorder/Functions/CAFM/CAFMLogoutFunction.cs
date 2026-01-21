using Core.DTOs;
using Core.Extensions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using sys_cafm_workorder.Abstractions;
using sys_cafm_workorder.DTOs.Api.CAFM;
using System.Net;

namespace sys_cafm_workorder.Functions.CAFM;

/// <summary>
/// Azure Function for CAFM Logout.
/// POST /api/cafm/logout
/// </summary>
public class CAFMLogoutFunction
{
    private readonly ICAFMAuthenticationService _authService;
    private readonly ILogger<CAFMLogoutFunction> _logger;

    public CAFMLogoutFunction(
        ICAFMAuthenticationService authService,
        ILogger<CAFMLogoutFunction> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [Function("CAFMLogout")]
    public async Task<BaseResponseDTO> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "cafm/logout")] HttpRequestData req,
        FunctionContext context)
    {
        _logger.Info("CAFMLogout function triggered");

        // Store HttpResponseData in context for middleware
        var response = req.CreateResponse(HttpStatusCode.OK);
        context.Items[typeof(HttpResponseData).Name] = response;

        // Read request body
        var requestDto = await req.ReadBodyAsync<LogoutRequestDto>();

        if (requestDto == null)
        {
            throw new Core.Exceptions.NoRequestBodyException(
                null,
                new List<string> { "Request body is required" },
                nameof(CAFMLogoutFunction));
        }

        // Call service
        return await _authService.LogoutAsync(requestDto);
    }
}
