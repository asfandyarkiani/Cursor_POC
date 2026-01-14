using Microsoft.AspNetCore.Mvc;
using SystemLayer.Application.DTOs;
using SystemLayer.Application.Interfaces;

namespace SystemLayer.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CafmController : ControllerBase
{
    private readonly ICafmService _cafmService;
    private readonly ILogger<CafmController> _logger;

    public CafmController(ICafmService cafmService, ILogger<CafmController> logger)
    {
        _cafmService = cafmService ?? throw new ArgumentNullException(nameof(cafmService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Login to CAFM system
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        if (request == null)
            return BadRequest("Login request is required");

        try
        {
            var response = await _cafmService.LoginAsync(request, cancellationToken);
            
            if (response.Success)
                return Ok(response);
            
            return BadRequest(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing login request");
            return StatusCode(500, new LoginResponseDto
            {
                Success = false,
                Errors = new List<string> { "Internal server error occurred" }
            });
        }
    }

    /// <summary>
    /// Logout from CAFM system
    /// </summary>
    [HttpPost("logout")]
    public async Task<ActionResult<LogoutResponseDto>> Logout([FromBody] LogoutRequestDto request, CancellationToken cancellationToken = default)
    {
        if (request == null)
            return BadRequest("Logout request is required");

        try
        {
            var response = await _cafmService.LogoutAsync(request, cancellationToken);
            
            if (response.Success)
                return Ok(response);
            
            return BadRequest(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing logout request");
            return StatusCode(500, new LogoutResponseDto
            {
                Success = false,
                Errors = new List<string> { "Internal server error occurred" }
            });
        }
    }

    /// <summary>
    /// Create a work order in CAFM system
    /// </summary>
    [HttpPost("work-orders")]
    public async Task<ActionResult<CreateWorkOrderResponseDto>> CreateWorkOrder([FromBody] CreateWorkOrderRequestDto request, CancellationToken cancellationToken = default)
    {
        if (request == null)
            return BadRequest("Work order request is required");

        try
        {
            var response = await _cafmService.CreateWorkOrderAsync(request, cancellationToken);
            
            if (response.Success)
                return Ok(response);
            
            return BadRequest(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating work order");
            return StatusCode(500, new CreateWorkOrderResponseDto
            {
                Success = false,
                Errors = new List<string> { "Internal server error occurred" }
            });
        }
    }

    /// <summary>
    /// Get breakdown task information from CAFM system
    /// </summary>
    [HttpGet("breakdown-tasks")]
    public async Task<ActionResult<GetBreakdownTaskResponseDto>> GetBreakdownTask([FromQuery] string? taskId, [FromQuery] string? assetId, [FromQuery] string? locationId, CancellationToken cancellationToken = default)
    {
        var request = new GetBreakdownTaskRequestDto
        {
            TaskId = taskId,
            AssetId = assetId,
            LocationId = locationId
        };

        try
        {
            var response = await _cafmService.GetBreakdownTaskAsync(request, cancellationToken);
            
            if (response.Success)
                return Ok(response);
            
            return BadRequest(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting breakdown task");
            return StatusCode(500, new GetBreakdownTaskResponseDto
            {
                Success = false,
                Errors = new List<string> { "Internal server error occurred" }
            });
        }
    }

    /// <summary>
    /// Get location information from CAFM system
    /// </summary>
    [HttpGet("locations")]
    public async Task<ActionResult<GetLocationResponseDto>> GetLocation([FromQuery] string? locationId, [FromQuery] string? locationCode, [FromQuery] string? buildingId, CancellationToken cancellationToken = default)
    {
        var request = new GetLocationRequestDto
        {
            LocationId = locationId,
            LocationCode = locationCode,
            BuildingId = buildingId
        };

        try
        {
            var response = await _cafmService.GetLocationAsync(request, cancellationToken);
            
            if (response.Success)
                return Ok(response);
            
            return BadRequest(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting location");
            return StatusCode(500, new GetLocationResponseDto
            {
                Success = false,
                Errors = new List<string> { "Internal server error occurred" }
            });
        }
    }

    /// <summary>
    /// Get instruction sets from CAFM system
    /// </summary>
    [HttpGet("instruction-sets")]
    public async Task<ActionResult<GetInstructionSetsResponseDto>> GetInstructionSets([FromQuery] string? assetId, [FromQuery] string? workType, [FromQuery] string? locationId, CancellationToken cancellationToken = default)
    {
        var request = new GetInstructionSetsRequestDto
        {
            AssetId = assetId,
            WorkType = workType,
            LocationId = locationId
        };

        try
        {
            var response = await _cafmService.GetInstructionSetsAsync(request, cancellationToken);
            
            if (response.Success)
                return Ok(response);
            
            return BadRequest(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting instruction sets");
            return StatusCode(500, new GetInstructionSetsResponseDto
            {
                Success = false,
                Errors = new List<string> { "Internal server error occurred" }
            });
        }
    }
}