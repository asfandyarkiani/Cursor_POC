using Core.Exceptions;
using Core.SystemLayer.Exceptions;
using Core.SystemLayer.Middlewares;
using Microsoft.Extensions.Logging;
using sys_oraclefusionhcm_mgmt.Constants;
using sys_oraclefusionhcm_mgmt.DTO.AtomicHandlerDTOs;
using sys_oraclefusionhcm_mgmt.DTO.CreateLeaveDTO;
using sys_oraclefusionhcm_mgmt.DTO.DownstreamDTOs;
using sys_oraclefusionhcm_mgmt.Helpers;
using sys_oraclefusionhcm_mgmt.Implementations.OracleFusionHCM.AtomicHandlers;
using System.Net;

namespace sys_oraclefusionhcm_mgmt.Implementations.OracleFusionHCM.Handlers;

/// <summary>
/// Handler for CreateLeave operation
/// Orchestrates CreateLeaveAtomicHandler and handles response transformation
/// </summary>
public class CreateLeaveHandler
{
    private readonly ILogger<CreateLeaveHandler> _logger;
    private readonly CreateLeaveAtomicHandler _createLeaveAtomicHandler;
    
    public CreateLeaveHandler(
        ILogger<CreateLeaveHandler> logger,
        CreateLeaveAtomicHandler createLeaveAtomicHandler)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _createLeaveAtomicHandler = createLeaveAtomicHandler ?? throw new ArgumentNullException(nameof(createLeaveAtomicHandler));
    }
    
    /// <summary>
    /// Handles CreateLeave request
    /// </summary>
    public async Task<CreateLeaveResDTO> HandleAsync(CreateLeaveReqDTO requestDto)
    {
        _logger.LogInformation(InfoConstants.CREATE_LEAVE_REQUEST_RECEIVED);
        
        // Validate request
        if (!requestDto.IsValid())
        {
            _logger.LogError("Invalid CreateLeave request: {Request}", RestApiHelper.SerializeToJson(requestDto));
            throw new RequestValidationFailureException(ErrorConstants.OFH_CRLEAV_1002_MSG);
        }
        
        // Map API-level DTO to Atomic Handler DTO
        CreateLeaveHandlerReqDTO atomicHandlerRequest = CreateLeaveHandlerReqDTO.Map(requestDto);
        
        // Call Atomic Handler
        HttpResponseSnapshot downstreamApiResponse = await _createLeaveAtomicHandler.CreateLeaveAsync(atomicHandlerRequest);
        
        // Check HTTP status code
        if (!downstreamApiResponse.IsSuccessStatusCode)
        {
            _logger.LogError(
                "Oracle Fusion HCM API returned non-success status code: {StatusCode}. Response: {Response}",
                downstreamApiResponse.StatusCode,
                downstreamApiResponse.Content
            );
            
            string errorMessage = ExtractErrorMessage(downstreamApiResponse);
            
            throw new DownStreamApiFailureException(
                ErrorConstants.OFH_CRLEAV_1003,
                ErrorConstants.OFH_CRLEAV_1003_MSG,
                downstreamApiResponse.StatusCode,
                errorMessage
            );
        }
        
        // Deserialize response
        CreateLeaveApiResDTO? apiResponse = RestApiHelper.DeserializeJsonResponse<CreateLeaveApiResDTO>(downstreamApiResponse.Content!);
        
        if (apiResponse == null || apiResponse.PersonAbsenceEntryId == 0)
        {
            _logger.LogError("Failed to deserialize Oracle Fusion HCM API response or PersonAbsenceEntryId is missing");
            throw new DownStreamApiFailureException(
                ErrorConstants.OFH_CRLEAV_1004,
                ErrorConstants.OFH_CRLEAV_1004_MSG,
                downstreamApiResponse.StatusCode,
                "Invalid response structure"
            );
        }
        
        // Map to response DTO
        CreateLeaveResDTO responseDto = CreateLeaveResDTO.CreateSuccess(
            apiResponse.PersonAbsenceEntryId,
            InfoConstants.CREATE_LEAVE_SUCCESS
        );
        
        _logger.LogInformation(
            "Leave absence created successfully. PersonAbsenceEntryId: {PersonAbsenceEntryId}",
            apiResponse.PersonAbsenceEntryId
        );
        
        return responseDto;
    }
    
    /// <summary>
    /// Extracts error message from downstream API response
    /// </summary>
    private string ExtractErrorMessage(HttpResponseSnapshot responseSnapshot)
    {
        if (string.IsNullOrWhiteSpace(responseSnapshot.Content))
        {
            return $"HTTP {(int)responseSnapshot.StatusCode} - {responseSnapshot.StatusCode}";
        }
        
        try
        {
            // Try to parse as JSON error response
            // Oracle Fusion HCM typically returns error details in JSON format
            return responseSnapshot.Content;
        }
        catch
        {
            return responseSnapshot.Content;
        }
    }
}
