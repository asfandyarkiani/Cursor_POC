using AlGhurair.Core.Extensions;
using AlGhurair.Core.SystemLayer.DTOs;
using AlGhurair.SystemLayer.OracleFusionHCM.Abstractions;
using AlGhurair.SystemLayer.OracleFusionHCM.Constants;
using AlGhurair.SystemLayer.OracleFusionHCM.DTO.CreateLeaveDTO;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace AlGhurair.SystemLayer.OracleFusionHCM.Functions;

/// <summary>
/// Azure Function for creating leave in Oracle Fusion HCM
/// HTTP entry point exposed to Process Layer or D365
/// </summary>
public class CreateLeaveAPI
{
    private readonly ILeaveMgmt _leaveMgmt;
    private readonly ILogger<CreateLeaveAPI> _logger;

    public CreateLeaveAPI(
        ILeaveMgmt leaveMgmt,
        ILogger<CreateLeaveAPI> logger)
    {
        _leaveMgmt = leaveMgmt ?? throw new ArgumentNullException(nameof(leaveMgmt));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// HTTP POST endpoint for creating leave in Oracle Fusion HCM
    /// Route: /api/leave/create
    /// </summary>
    [Function("CreateLeaveAPI")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "leave/create")] HttpRequestData req)
    {
        _logger.LogInformation("CreateLeaveAPI: Request received");

        try
        {
            // Deserialize request using Framework extension method
            CreateLeaveReqDTO request = await req.ReadBodyAsync<CreateLeaveReqDTO>();

            _logger.LogInformation($"CreateLeaveAPI: Processing leave creation for employee {request.EmployeeNumber}");

            // Call service to create leave
            CreateLeaveResDTO response = await _leaveMgmt.CreateLeaveAsync(request);

            // Create HTTP response
            HttpResponseData httpResponse;
            
            if (response.Success == "true")
            {
                httpResponse = req.CreateResponse(HttpStatusCode.OK);
                _logger.LogInformation($"CreateLeaveAPI: Leave created successfully. PersonAbsenceEntryId: {response.PersonAbsenceEntryId}");
            }
            else
            {
                httpResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                _logger.LogWarning($"CreateLeaveAPI: Leave creation failed. Message: {response.Message}");
            }

            await httpResponse.WriteAsJsonAsync(response);

            return httpResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{ErrorConstants.OFH_LVEMGT_0001}: Unexpected error in CreateLeaveAPI");

            HttpResponseData errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            
            CreateLeaveResDTO errorDto = new()
            {
                Status = "failure",
                Message = $"{ErrorConstants.OFH_LVEMGT_0001}: {ex.Message}",
                PersonAbsenceEntryId = null,
                Success = "false"
            };

            await errorResponse.WriteAsJsonAsync(errorDto);

            return errorResponse;
        }
    }
}
