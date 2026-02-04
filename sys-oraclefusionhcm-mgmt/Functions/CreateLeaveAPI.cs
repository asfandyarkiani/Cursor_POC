using Core.Extensions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using sys_oraclefusionhcm_mgmt.Abstractions;
using sys_oraclefusionhcm_mgmt.Constants;
using sys_oraclefusionhcm_mgmt.DTO.CreateLeaveDTO;
using sys_oraclefusionhcm_mgmt.Helpers;
using System.Net;

namespace sys_oraclefusionhcm_mgmt.Functions;

/// <summary>
/// Azure Function for creating leave absence in Oracle Fusion HCM
/// HTTP-triggered function exposed to Process Layer
/// </summary>
public class CreateLeaveAPI
{
    private readonly ILogger<CreateLeaveAPI> _logger;
    private readonly ILeaveMgmt _leaveMgmtService;
    
    public CreateLeaveAPI(
        ILogger<CreateLeaveAPI> logger,
        ILeaveMgmt leaveMgmtService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _leaveMgmtService = leaveMgmtService ?? throw new ArgumentNullException(nameof(leaveMgmtService));
    }
    
    /// <summary>
    /// Creates leave absence in Oracle Fusion HCM
    /// </summary>
    /// <param name="req">HTTP request</param>
    /// <param name="executionContext">Function execution context</param>
    /// <returns>HTTP response with CreateLeaveResDTO</returns>
    [Function("CreateLeaveAPI")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "leave")] HttpRequestData req,
        FunctionContext executionContext)
    {
        _logger.LogInformation("CreateLeaveAPI function triggered");
        
        try
        {
            // Read request body
            string requestBody = await req.ReadAsStringAsync() ?? string.Empty;
            
            if (string.IsNullOrWhiteSpace(requestBody))
            {
                _logger.LogError("Request body is empty");
                HttpResponseData badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                CreateLeaveResDTO errorResponse = CreateLeaveResDTO.CreateFailure("Request body is empty");
                await badRequestResponse.WriteAsJsonAsync(errorResponse);
                return badRequestResponse;
            }
            
            // Deserialize request
            CreateLeaveReqDTO? requestDto = RestApiHelper.DeserializeJsonResponse<CreateLeaveReqDTO>(requestBody);
            
            if (requestDto == null)
            {
                _logger.LogError("Failed to deserialize request body");
                HttpResponseData badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                CreateLeaveResDTO errorResponse = CreateLeaveResDTO.CreateFailure("Invalid request format");
                await badRequestResponse.WriteAsJsonAsync(errorResponse);
                return badRequestResponse;
            }
            
            // Call service
            CreateLeaveResDTO responseDto = await _leaveMgmtService.CreateLeaveAsync(requestDto);
            
            // Create success response
            HttpResponseData response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(responseDto);
            
            _logger.LogInformation("CreateLeaveAPI function completed successfully");
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in CreateLeaveAPI function");
            
            // Create error response
            HttpResponseData errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            CreateLeaveResDTO errorDto = CreateLeaveResDTO.CreateFailure(
                $"Internal server error: {ex.Message}"
            );
            await errorResponse.WriteAsJsonAsync(errorDto);
            return errorResponse;
        }
    }
}
