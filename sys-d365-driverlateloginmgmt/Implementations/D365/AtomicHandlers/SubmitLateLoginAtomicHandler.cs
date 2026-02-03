using Core.Exceptions;
using Core.Extensions;
using Core.Middlewares;
using Core.SystemLayer.DTOs;
using Core.SystemLayer.Handlers;
using Core.SystemLayer.Middlewares;
using AGI.SysD365DriverLateLoginMgmt.ConfigModels;
using AGI.SysD365DriverLateLoginMgmt.Constants;
using AGI.SysD365DriverLateLoginMgmt.DTO.AtomicHandlerDTOs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace AGI.SysD365DriverLateLoginMgmt.Implementations.D365.AtomicHandlers;

/// <summary>
/// Atomic Handler for submitting late login request to D365
/// Makes single HTTP POST call to D365 late login API
/// </summary>
public class SubmitLateLoginAtomicHandler : IAtomicHandler<HttpResponseSnapshot>
{
    private readonly CustomRestClient _restClient;
    private readonly AppConfigs _appConfigs;
    private readonly ILogger<SubmitLateLoginAtomicHandler> _logger;

    public SubmitLateLoginAtomicHandler(
        CustomRestClient restClient,
        IOptions<AppConfigs> options,
        ILogger<SubmitLateLoginAtomicHandler> logger)
    {
        _restClient = restClient;
        _appConfigs = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// Handles the atomic operation of submitting late login request to D365
    /// </summary>
    /// <param name="downStreamRequestDTO">Request DTO containing late login details</param>
    /// <returns>HTTP response snapshot from D365 API</returns>
    public async Task<HttpResponseSnapshot> Handle(IDownStreamRequestDTO downStreamRequestDTO)
    {
        SubmitLateLoginHandlerReqDTO requestDTO = downStreamRequestDTO as SubmitLateLoginHandlerReqDTO 
            ?? throw new ArgumentException("Invalid DTO type - expected SubmitLateLoginHandlerReqDTO");

        _logger.Info($"Starting SubmitLateLogin for DriverId: {requestDTO.DriverId}");

        // Validate request parameters
        requestDTO.ValidateDownStreamRequestParameters();

        // Build D365 API URL
        string fullApiUrl = $"{_appConfigs.D365Config.BaseUrl}/{_appConfigs.D365Config.LateLoginResourcePath}";
        _logger.Info($"D365 API URL: {fullApiUrl}");

        // Build request body (D365 expects params object)
        // Note: requestDateTime and companyCode are optional in D365 contract
        object d365RequestBody = new
        {
            @params = new
            {
                driverId = requestDTO.DriverId,
                requestDateTime = requestDTO.RequestDateTime ?? string.Empty,
                companyCode = requestDTO.CompanyCode ?? string.Empty,
                reasonCode = requestDTO.ReasonCode ?? string.Empty,
                remarks = requestDTO.Remarks ?? string.Empty,
                RequestNo = requestDTO.RequestNo ?? string.Empty
            }
        };

        string requestBodyJson = JsonSerializer.Serialize(d365RequestBody);
        _logger.Info($"Request body: {requestBodyJson}");

        // Build custom headers with Authorization token
        Dictionary<string, string> customHeaders = new Dictionary<string, string>
        {
            { "Authorization", requestDTO.AuthorizationToken }
        };

        // Call D365 late login API
        HttpResponseSnapshot d365Response = await _restClient.ExecuteCustomRestRequestAsync(
            operationName: "SubmitLateLogin",
            apiUrl: fullApiUrl,
            httpMethod: HttpMethod.Post,
            contentFactory: () => CustomRestClient.CreateContentWithHeaders(
                CustomRestClient.CreateJsonContent(requestBodyJson, System.Text.Encoding.UTF8, "application/json"),
                customHeaders
            )
        );

        _logger.Info($"SubmitLateLogin completed - Status: {d365Response.StatusCode}");

        return d365Response;
    }
}
