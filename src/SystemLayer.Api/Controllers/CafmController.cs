using Microsoft.AspNetCore.Mvc;
using SystemLayer.Application.DTOs;
using SystemLayer.Application.Interfaces;

namespace SystemLayer.Api.Controllers;

/// <summary>
/// Controller for CAFM integration operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class CafmController : ControllerBase
{
    private readonly ICafmService _cafmService;
    private readonly ILogger<CafmController> _logger;
    
    public CafmController(ICafmService cafmService, ILogger<CafmController> logger)
    {
        _cafmService = cafmService;
        _logger = logger;
    }
    
    /// <summary>
    /// Creates a work order in the CAFM system
    /// </summary>
    /// <param name="request">Work order creation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Work order creation result</returns>
    [HttpPost("work-orders")]
    [ProducesResponseType(typeof(SystemLayerResult<CreateWorkOrderResponse>), 200)]
    [ProducesResponseType(typeof(SystemLayerResult<CreateWorkOrderResponse>), 400)]
    [ProducesResponseType(typeof(SystemLayerResult<CreateWorkOrderResponse>), 500)]
    public async Task<ActionResult<SystemLayerResult<CreateWorkOrderResponse>>> CreateWorkOrder(
        [FromBody] CreateWorkOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        var correlationId = GetOrGenerateCorrelationId();
        
        _logger.LogInformation("Received create work order request for {WorkOrderNumber} with correlation {CorrelationId}", 
            request.WorkOrderNumber, correlationId);
        
        try
        {
            var result = await _cafmService.CreateWorkOrderAsync(request, correlationId, cancellationToken);
            
            if (result.Success)
            {
                return Ok(result);
            }
            
            // Return appropriate HTTP status based on error type
            return result.Error?.ErrorCode switch
            {
                var code when code.StartsWith("SL_VAL_") => BadRequest(result),
                var code when code.StartsWith("SL_BIZ_") => BadRequest(result),
                _ => StatusCode(500, result)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in CreateWorkOrder with correlation {CorrelationId}", correlationId);
            
            var errorResult = SystemLayerResult<CreateWorkOrderResponse>.Fail(
                SystemLayerErrorCodes.InternalError,
                "An unexpected error occurred",
                false,
                correlationId);
            
            return StatusCode(500, errorResult);
        }
    }
    
    /// <summary>
    /// Retrieves location information from the CAFM system
    /// </summary>
    /// <param name="locationId">Location ID to retrieve</param>
    /// <param name="includeHierarchy">Whether to include location hierarchy</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Location information</returns>
    [HttpGet("locations/{locationId}")]
    [ProducesResponseType(typeof(SystemLayerResult<LocationResponse>), 200)]
    [ProducesResponseType(typeof(SystemLayerResult<LocationResponse>), 400)]
    [ProducesResponseType(typeof(SystemLayerResult<LocationResponse>), 404)]
    [ProducesResponseType(typeof(SystemLayerResult<LocationResponse>), 500)]
    public async Task<ActionResult<SystemLayerResult<LocationResponse>>> GetLocation(
        [FromRoute] string locationId,
        [FromQuery] bool includeHierarchy = false,
        CancellationToken cancellationToken = default)
    {
        var correlationId = GetOrGenerateCorrelationId();
        
        _logger.LogInformation("Received get location request for {LocationId} with correlation {CorrelationId}", 
            locationId, correlationId);
        
        try
        {
            var request = new GetLocationRequest
            {
                LocationId = locationId,
                IncludeHierarchy = includeHierarchy
            };
            
            var result = await _cafmService.GetLocationAsync(request, correlationId, cancellationToken);
            
            if (result.Success)
            {
                return Ok(result);
            }
            
            // Return appropriate HTTP status based on error type
            return result.Error?.ErrorCode switch
            {
                SystemLayerErrorCodes.LocationNotFound => NotFound(result),
                var code when code.StartsWith("SL_VAL_") => BadRequest(result),
                _ => StatusCode(500, result)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in GetLocation with correlation {CorrelationId}", correlationId);
            
            var errorResult = SystemLayerResult<LocationResponse>.Fail(
                SystemLayerErrorCodes.InternalError,
                "An unexpected error occurred",
                false,
                correlationId);
            
            return StatusCode(500, errorResult);
        }
    }
    
    /// <summary>
    /// Retrieves breakdown task information from the CAFM system
    /// </summary>
    /// <param name="taskId">Task ID to retrieve</param>
    /// <param name="includeDetails">Whether to include task details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Breakdown task information</returns>
    [HttpGet("breakdown-tasks/{taskId}")]
    [ProducesResponseType(typeof(SystemLayerResult<BreakdownTaskResponse>), 200)]
    [ProducesResponseType(typeof(SystemLayerResult<BreakdownTaskResponse>), 400)]
    [ProducesResponseType(typeof(SystemLayerResult<BreakdownTaskResponse>), 404)]
    [ProducesResponseType(typeof(SystemLayerResult<BreakdownTaskResponse>), 500)]
    public async Task<ActionResult<SystemLayerResult<BreakdownTaskResponse>>> GetBreakdownTask(
        [FromRoute] string taskId,
        [FromQuery] bool includeDetails = true,
        CancellationToken cancellationToken = default)
    {
        var correlationId = GetOrGenerateCorrelationId();
        
        _logger.LogInformation("Received get breakdown task request for {TaskId} with correlation {CorrelationId}", 
            taskId, correlationId);
        
        try
        {
            var request = new GetBreakdownTaskRequest
            {
                TaskId = taskId,
                IncludeDetails = includeDetails
            };
            
            var result = await _cafmService.GetBreakdownTaskAsync(request, correlationId, cancellationToken);
            
            if (result.Success)
            {
                return Ok(result);
            }
            
            // Return appropriate HTTP status based on error type
            return result.Error?.ErrorCode switch
            {
                SystemLayerErrorCodes.TaskNotFound => NotFound(result),
                var code when code.StartsWith("SL_VAL_") => BadRequest(result),
                _ => StatusCode(500, result)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in GetBreakdownTask with correlation {CorrelationId}", correlationId);
            
            var errorResult = SystemLayerResult<BreakdownTaskResponse>.Fail(
                SystemLayerErrorCodes.InternalError,
                "An unexpected error occurred",
                false,
                correlationId);
            
            return StatusCode(500, errorResult);
        }
    }
    
    /// <summary>
    /// Retrieves instruction sets from the CAFM system
    /// </summary>
    /// <param name="categoryFilter">Optional category filter</param>
    /// <param name="assetTypeFilter">Optional asset type filter</param>
    /// <param name="maxResults">Maximum number of results to return</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Instruction sets</returns>
    [HttpGet("instruction-sets")]
    [ProducesResponseType(typeof(SystemLayerResult<GetInstructionSetsResponse>), 200)]
    [ProducesResponseType(typeof(SystemLayerResult<GetInstructionSetsResponse>), 400)]
    [ProducesResponseType(typeof(SystemLayerResult<GetInstructionSetsResponse>), 500)]
    public async Task<ActionResult<SystemLayerResult<GetInstructionSetsResponse>>> GetInstructionSets(
        [FromQuery] string? categoryFilter = null,
        [FromQuery] string? assetTypeFilter = null,
        [FromQuery] int maxResults = 100,
        CancellationToken cancellationToken = default)
    {
        var correlationId = GetOrGenerateCorrelationId();
        
        _logger.LogInformation("Received get instruction sets request with correlation {CorrelationId}", correlationId);
        
        try
        {
            var request = new GetInstructionSetsRequest
            {
                CategoryFilter = categoryFilter,
                AssetTypeFilter = assetTypeFilter,
                MaxResults = maxResults
            };
            
            var result = await _cafmService.GetInstructionSetsAsync(request, correlationId, cancellationToken);
            
            if (result.Success)
            {
                return Ok(result);
            }
            
            // Return appropriate HTTP status based on error type
            return result.Error?.ErrorCode switch
            {
                var code when code.StartsWith("SL_VAL_") => BadRequest(result),
                _ => StatusCode(500, result)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in GetInstructionSets with correlation {CorrelationId}", correlationId);
            
            var errorResult = SystemLayerResult<GetInstructionSetsResponse>.Fail(
                SystemLayerErrorCodes.InternalError,
                "An unexpected error occurred",
                false,
                correlationId);
            
            return StatusCode(500, errorResult);
        }
    }
    
    /// <summary>
    /// Health check endpoint
    /// </summary>
    /// <returns>Health status</returns>
    [HttpGet("health")]
    [ProducesResponseType(typeof(object), 200)]
    public ActionResult<object> HealthCheck()
    {
        return Ok(new
        {
            Status = "Healthy",
            Timestamp = DateTime.UtcNow,
            Service = "SystemLayer.Api"
        });
    }
    
    private string GetOrGenerateCorrelationId()
    {
        // Try to get correlation ID from headers first
        if (Request.Headers.TryGetValue("X-Correlation-ID", out var correlationId) && 
            !string.IsNullOrEmpty(correlationId))
        {
            return correlationId.ToString();
        }
        
        // Generate new correlation ID
        return Guid.NewGuid().ToString();
    }
}