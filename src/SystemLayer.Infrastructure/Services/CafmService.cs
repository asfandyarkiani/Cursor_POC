using Microsoft.Extensions.Logging;
using SystemLayer.Application.DTOs;
using SystemLayer.Application.Interfaces;
using SystemLayer.Infrastructure.Mapping;
using SystemLayer.Infrastructure.Models;

namespace SystemLayer.Infrastructure.Services;

/// <summary>
/// High-level CAFM service implementation
/// </summary>
public class CafmService : ICafmService
{
    private readonly ICafmClient _cafmClient;
    private readonly CafmMappingService _mappingService;
    private readonly ILogger<CafmService> _logger;
    
    public CafmService(
        ICafmClient cafmClient,
        CafmMappingService mappingService,
        ILogger<CafmService> logger)
    {
        _cafmClient = cafmClient;
        _mappingService = mappingService;
        _logger = logger;
    }
    
    /// <inheritdoc />
    public async Task<SystemLayerResult<CreateWorkOrderResponse>> CreateWorkOrderAsync(
        CreateWorkOrderRequest request,
        string correlationId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating work order {WorkOrderNumber} with correlation {CorrelationId}", 
            request.WorkOrderNumber, correlationId);
        
        try
        {
            // Validate request
            if (string.IsNullOrWhiteSpace(request.WorkOrderNumber))
            {
                return SystemLayerResult<CreateWorkOrderResponse>.Fail(
                    SystemLayerErrorCodes.MissingRequiredField, 
                    "Work order number is required", 
                    false, 
                    correlationId);
            }
            
            if (string.IsNullOrWhiteSpace(request.Description))
            {
                return SystemLayerResult<CreateWorkOrderResponse>.Fail(
                    SystemLayerErrorCodes.MissingRequiredField, 
                    "Description is required", 
                    false, 
                    correlationId);
            }
            
            if (string.IsNullOrWhiteSpace(request.LocationId))
            {
                return SystemLayerResult<CreateWorkOrderResponse>.Fail(
                    SystemLayerErrorCodes.MissingRequiredField, 
                    "Location ID is required", 
                    false, 
                    correlationId);
            }
            
            // Map request (session token will be added by the client)
            var cafmRequest = _mappingService.MapToCreateWorkOrderRequest(request, string.Empty, correlationId);
            
            // Execute with login/logout wrapper
            var cafmResponse = await _cafmClient.ExecuteWithSessionAsync<CafmCreateWorkOrderRequest, CafmCreateWorkOrderResponse>(
                "CreateWorkOrder",
                cafmRequest,
                correlationId,
                cancellationToken);
            
            // Map response
            var response = _mappingService.MapFromCreateWorkOrderResponse(cafmResponse, correlationId);
            
            if (!response.Success)
            {
                _logger.LogWarning("CAFM work order creation failed for {WorkOrderNumber}: {Message}. Correlation: {CorrelationId}", 
                    request.WorkOrderNumber, response.Message, correlationId);
                
                // Determine if error is retryable based on message/error code
                var isRetryable = IsRetryableError(response.Message, response.AdditionalData.GetValueOrDefault("ErrorCode")?.ToString());
                
                return SystemLayerResult<CreateWorkOrderResponse>.Fail(
                    SystemLayerErrorCodes.CafmServiceUnavailable,
                    response.Message ?? "Work order creation failed",
                    isRetryable,
                    correlationId);
            }
            
            _logger.LogInformation("Work order {WorkOrderNumber} created successfully with ID {WorkOrderId}. Correlation: {CorrelationId}", 
                request.WorkOrderNumber, response.WorkOrderId, correlationId);
            
            return SystemLayerResult<CreateWorkOrderResponse>.Ok(response, correlationId);
        }
        catch (TimeoutException ex)
        {
            _logger.LogError(ex, "Timeout creating work order {WorkOrderNumber} with correlation {CorrelationId}", 
                request.WorkOrderNumber, correlationId);
            
            return SystemLayerResult<CreateWorkOrderResponse>.Fail(
                SystemLayerErrorCodes.CafmTimeout,
                "Request timed out while creating work order",
                true,
                correlationId);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error creating work order {WorkOrderNumber} with correlation {CorrelationId}", 
                request.WorkOrderNumber, correlationId);
            
            return SystemLayerResult<CreateWorkOrderResponse>.Fail(
                SystemLayerErrorCodes.CafmConnectionFailed,
                "Failed to connect to CAFM system",
                true,
                correlationId);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("SOAP fault") || ex.Message.Contains("failed"))
        {
            _logger.LogError(ex, "CAFM SOAP fault creating work order {WorkOrderNumber} with correlation {CorrelationId}", 
                request.WorkOrderNumber, correlationId);
            
            return SystemLayerResult<CreateWorkOrderResponse>.Fail(
                SystemLayerErrorCodes.CafmSoapFault,
                ex.Message,
                false,
                correlationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error creating work order {WorkOrderNumber} with correlation {CorrelationId}", 
                request.WorkOrderNumber, correlationId);
            
            return SystemLayerResult<CreateWorkOrderResponse>.Fail(
                SystemLayerErrorCodes.InternalError,
                "An unexpected error occurred while creating work order",
                false,
                correlationId);
        }
    }
    
    /// <inheritdoc />
    public async Task<SystemLayerResult<LocationResponse>> GetLocationAsync(
        GetLocationRequest request,
        string correlationId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting location {LocationId} with correlation {CorrelationId}", 
            request.LocationId, correlationId);
        
        try
        {
            // Validate request
            if (string.IsNullOrWhiteSpace(request.LocationId))
            {
                return SystemLayerResult<LocationResponse>.Fail(
                    SystemLayerErrorCodes.MissingRequiredField,
                    "Location ID is required",
                    false,
                    correlationId);
            }
            
            // Map request (session token will be added by the client)
            var cafmRequest = _mappingService.MapToGetLocationRequest(request, string.Empty, correlationId);
            
            // Execute with login/logout wrapper
            var cafmResponse = await _cafmClient.ExecuteWithSessionAsync<CafmGetLocationRequest, CafmGetLocationResponse>(
                "GetLocation",
                cafmRequest,
                correlationId,
                cancellationToken);
            
            if (!cafmResponse.Success)
            {
                _logger.LogWarning("CAFM location retrieval failed for {LocationId}: {Message}. Correlation: {CorrelationId}", 
                    request.LocationId, cafmResponse.Message, correlationId);
                
                var errorCode = cafmResponse.ErrorCode == "NOT_FOUND" ? 
                    SystemLayerErrorCodes.LocationNotFound : 
                    SystemLayerErrorCodes.CafmServiceUnavailable;
                
                return SystemLayerResult<LocationResponse>.Fail(
                    errorCode,
                    cafmResponse.Message ?? "Location retrieval failed",
                    IsRetryableError(cafmResponse.Message, cafmResponse.ErrorCode),
                    correlationId);
            }
            
            // Map response
            var response = _mappingService.MapFromGetLocationResponse(cafmResponse, correlationId);
            
            _logger.LogInformation("Location {LocationId} retrieved successfully. Correlation: {CorrelationId}", 
                request.LocationId, correlationId);
            
            return SystemLayerResult<LocationResponse>.Ok(response, correlationId);
        }
        catch (TimeoutException ex)
        {
            _logger.LogError(ex, "Timeout getting location {LocationId} with correlation {CorrelationId}", 
                request.LocationId, correlationId);
            
            return SystemLayerResult<LocationResponse>.Fail(
                SystemLayerErrorCodes.CafmTimeout,
                "Request timed out while retrieving location",
                true,
                correlationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting location {LocationId} with correlation {CorrelationId}", 
                request.LocationId, correlationId);
            
            return SystemLayerResult<LocationResponse>.Fail(
                SystemLayerErrorCodes.InternalError,
                "An unexpected error occurred while retrieving location",
                false,
                correlationId);
        }
    }
    
    /// <inheritdoc />
    public async Task<SystemLayerResult<BreakdownTaskResponse>> GetBreakdownTaskAsync(
        GetBreakdownTaskRequest request,
        string correlationId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting breakdown task {TaskId} with correlation {CorrelationId}", 
            request.TaskId, correlationId);
        
        try
        {
            // Validate request
            if (string.IsNullOrWhiteSpace(request.TaskId))
            {
                return SystemLayerResult<BreakdownTaskResponse>.Fail(
                    SystemLayerErrorCodes.MissingRequiredField,
                    "Task ID is required",
                    false,
                    correlationId);
            }
            
            // Map request (session token will be added by the client)
            var cafmRequest = _mappingService.MapToGetBreakdownTaskRequest(request, string.Empty, correlationId);
            
            // Execute with login/logout wrapper
            var cafmResponse = await _cafmClient.ExecuteWithSessionAsync<CafmGetBreakdownTaskRequest, CafmGetBreakdownTaskResponse>(
                "GetBreakdownTask",
                cafmRequest,
                correlationId,
                cancellationToken);
            
            if (!cafmResponse.Success)
            {
                _logger.LogWarning("CAFM breakdown task retrieval failed for {TaskId}: {Message}. Correlation: {CorrelationId}", 
                    request.TaskId, cafmResponse.Message, correlationId);
                
                var errorCode = cafmResponse.ErrorCode == "NOT_FOUND" ? 
                    SystemLayerErrorCodes.TaskNotFound : 
                    SystemLayerErrorCodes.CafmServiceUnavailable;
                
                return SystemLayerResult<BreakdownTaskResponse>.Fail(
                    errorCode,
                    cafmResponse.Message ?? "Breakdown task retrieval failed",
                    IsRetryableError(cafmResponse.Message, cafmResponse.ErrorCode),
                    correlationId);
            }
            
            // Map response
            var response = _mappingService.MapFromGetBreakdownTaskResponse(cafmResponse, correlationId);
            
            _logger.LogInformation("Breakdown task {TaskId} retrieved successfully. Correlation: {CorrelationId}", 
                request.TaskId, correlationId);
            
            return SystemLayerResult<BreakdownTaskResponse>.Ok(response, correlationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting breakdown task {TaskId} with correlation {CorrelationId}", 
                request.TaskId, correlationId);
            
            return SystemLayerResult<BreakdownTaskResponse>.Fail(
                SystemLayerErrorCodes.InternalError,
                "An unexpected error occurred while retrieving breakdown task",
                false,
                correlationId);
        }
    }
    
    /// <inheritdoc />
    public async Task<SystemLayerResult<GetInstructionSetsResponse>> GetInstructionSetsAsync(
        GetInstructionSetsRequest request,
        string correlationId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting instruction sets with correlation {CorrelationId}", correlationId);
        
        try
        {
            // Map request (session token will be added by the client)
            var cafmRequest = _mappingService.MapToGetInstructionSetsRequest(request, string.Empty, correlationId);
            
            // Execute with login/logout wrapper
            var cafmResponse = await _cafmClient.ExecuteWithSessionAsync<CafmGetInstructionSetsRequest, CafmGetInstructionSetsResponse>(
                "GetInstructionSets",
                cafmRequest,
                correlationId,
                cancellationToken);
            
            if (!cafmResponse.Success)
            {
                _logger.LogWarning("CAFM instruction sets retrieval failed: {Message}. Correlation: {CorrelationId}", 
                    cafmResponse.Message, correlationId);
                
                return SystemLayerResult<GetInstructionSetsResponse>.Fail(
                    SystemLayerErrorCodes.CafmServiceUnavailable,
                    cafmResponse.Message ?? "Instruction sets retrieval failed",
                    IsRetryableError(cafmResponse.Message, cafmResponse.ErrorCode),
                    correlationId);
            }
            
            // Map response
            var response = _mappingService.MapFromGetInstructionSetsResponse(cafmResponse, correlationId);
            
            _logger.LogInformation("Retrieved {Count} instruction sets successfully. Correlation: {CorrelationId}", 
                response.InstructionSets.Count, correlationId);
            
            return SystemLayerResult<GetInstructionSetsResponse>.Ok(response, correlationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting instruction sets with correlation {CorrelationId}", correlationId);
            
            return SystemLayerResult<GetInstructionSetsResponse>.Fail(
                SystemLayerErrorCodes.InternalError,
                "An unexpected error occurred while retrieving instruction sets",
                false,
                correlationId);
        }
    }
    
    private static bool IsRetryableError(string? message, string? errorCode)
    {
        if (string.IsNullOrEmpty(message) && string.IsNullOrEmpty(errorCode))
            return false;
        
        var retryableMessages = new[]
        {
            "timeout", "connection", "network", "unavailable", "busy", "overloaded"
        };
        
        var retryableCodes = new[]
        {
            "TIMEOUT", "CONNECTION_FAILED", "SERVICE_UNAVAILABLE", "BUSY", "OVERLOADED"
        };
        
        var messageToCheck = message?.ToLowerInvariant() ?? string.Empty;
        var codeToCheck = errorCode?.ToUpperInvariant() ?? string.Empty;
        
        return retryableMessages.Any(rm => messageToCheck.Contains(rm)) ||
               retryableCodes.Any(rc => codeToCheck.Contains(rc));
    }
}