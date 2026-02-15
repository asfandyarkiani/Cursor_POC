using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SystemLayer.Application.Configuration;
using SystemLayer.Application.DTOs;
using SystemLayer.Application.Interfaces;

namespace SystemLayer.Infrastructure.Services;

public class CafmService : ICafmService
{
    private readonly ICafmClient _cafmClient;
    private readonly ICafmAuthenticationService _authService;
    private readonly ICafmXmlBuilder _xmlBuilder;
    private readonly ICafmXmlParser _xmlParser;
    private readonly ILogger<CafmService> _logger;
    private readonly CafmConfiguration _config;

    public CafmService(
        ICafmClient cafmClient,
        ICafmAuthenticationService authService,
        ICafmXmlBuilder xmlBuilder,
        ICafmXmlParser xmlParser,
        ILogger<CafmService> logger,
        IOptions<CafmConfiguration> config)
    {
        _cafmClient = cafmClient ?? throw new ArgumentNullException(nameof(cafmClient));
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _xmlBuilder = xmlBuilder ?? throw new ArgumentNullException(nameof(xmlBuilder));
        _xmlParser = xmlParser ?? throw new ArgumentNullException(nameof(xmlParser));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _config = config.Value ?? throw new ArgumentNullException(nameof(config));
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        return await _authService.LoginAsync(request, cancellationToken);
    }

    public async Task<LogoutResponseDto> LogoutAsync(LogoutRequestDto request, CancellationToken cancellationToken = default)
    {
        return await _authService.LogoutAsync(request.SessionToken!, cancellationToken);
    }

    public async Task<CreateWorkOrderResponseDto> CreateWorkOrderAsync(CreateWorkOrderRequestDto request, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithSessionAsync(async sessionToken =>
        {
            _logger.LogInformation("Creating work order: {WorkOrderNumber}", request.WorkOrderNumber);

            var xmlRequest = _xmlBuilder.BuildCreateWorkOrderRequest(request, sessionToken);
            var xmlResponse = await _cafmClient.SendSoapRequestAsync(
                _config.Soap.CreateWorkOrderAction,
                xmlRequest,
                sessionToken,
                cancellationToken);

            var response = _xmlParser.ParseCreateWorkOrderResponse(xmlResponse);

            if (response.Success)
            {
                _logger.LogInformation("Work order created successfully: {WorkOrderId}", response.WorkOrderId);
            }
            else
            {
                _logger.LogWarning("Work order creation failed: {Errors}", 
                    string.Join(", ", response.Errors ?? new List<string>()));
            }

            return response;
        }, cancellationToken);
    }

    public async Task<GetBreakdownTaskResponseDto> GetBreakdownTaskAsync(GetBreakdownTaskRequestDto request, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithSessionAsync(async sessionToken =>
        {
            _logger.LogInformation("Getting breakdown task: {TaskId}", request.TaskId);

            var xmlRequest = _xmlBuilder.BuildGetBreakdownTaskRequest(request, sessionToken);
            var xmlResponse = await _cafmClient.SendSoapRequestAsync(
                _config.Soap.GetBreakdownTaskAction,
                xmlRequest,
                sessionToken,
                cancellationToken);

            var response = _xmlParser.ParseGetBreakdownTaskResponse(xmlResponse);

            if (response.Success)
            {
                _logger.LogInformation("Breakdown task retrieved successfully: {TaskId}", response.TaskId);
            }
            else
            {
                _logger.LogWarning("Breakdown task retrieval failed: {Errors}", 
                    string.Join(", ", response.Errors ?? new List<string>()));
            }

            return response;
        }, cancellationToken);
    }

    public async Task<GetLocationResponseDto> GetLocationAsync(GetLocationRequestDto request, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithSessionAsync(async sessionToken =>
        {
            _logger.LogInformation("Getting location: {LocationId}", request.LocationId);

            var xmlRequest = _xmlBuilder.BuildGetLocationRequest(request, sessionToken);
            var xmlResponse = await _cafmClient.SendSoapRequestAsync(
                _config.Soap.GetLocationAction,
                xmlRequest,
                sessionToken,
                cancellationToken);

            var response = _xmlParser.ParseGetLocationResponse(xmlResponse);

            if (response.Success)
            {
                _logger.LogInformation("Location retrieved successfully: {LocationId}", response.LocationId);
            }
            else
            {
                _logger.LogWarning("Location retrieval failed: {Errors}", 
                    string.Join(", ", response.Errors ?? new List<string>()));
            }

            return response;
        }, cancellationToken);
    }

    public async Task<GetInstructionSetsResponseDto> GetInstructionSetsAsync(GetInstructionSetsRequestDto request, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithSessionAsync(async sessionToken =>
        {
            _logger.LogInformation("Getting instruction sets for asset: {AssetId}", request.AssetId);

            var xmlRequest = _xmlBuilder.BuildGetInstructionSetsRequest(request, sessionToken);
            var xmlResponse = await _cafmClient.SendSoapRequestAsync(
                _config.Soap.GetInstructionSetsAction,
                xmlRequest,
                sessionToken,
                cancellationToken);

            var response = _xmlParser.ParseGetInstructionSetsResponse(xmlResponse);

            if (response.Success)
            {
                _logger.LogInformation("Instruction sets retrieved successfully. Count: {Count}", 
                    response.InstructionSets?.Count ?? 0);
            }
            else
            {
                _logger.LogWarning("Instruction sets retrieval failed: {Errors}", 
                    string.Join(", ", response.Errors ?? new List<string>()));
            }

            return response;
        }, cancellationToken);
    }

    private async Task<T> ExecuteWithSessionAsync<T>(Func<string, Task<T>> action, CancellationToken cancellationToken = default)
        where T : class, new()
    {
        string? sessionToken = null;
        
        try
        {
            // Step 1: Login
            var loginRequest = new LoginRequestDto
            {
                Username = _config.Username,
                Password = _config.Password,
                Database = _config.Database
            };

            var loginResponse = await _authService.LoginAsync(loginRequest, cancellationToken);
            if (!loginResponse.Success || string.IsNullOrEmpty(loginResponse.SessionToken))
            {
                _logger.LogError("Failed to login to CAFM: {Errors}", 
                    string.Join(", ", loginResponse.Errors ?? new List<string>()));
                
                // Return a failed response of type T
                var failedResponse = new T();
                if (failedResponse is CreateWorkOrderResponseDto createWoResponse)
                {
                    createWoResponse.Success = false;
                    createWoResponse.Errors = loginResponse.Errors;
                }
                else if (failedResponse is GetBreakdownTaskResponseDto breakdownResponse)
                {
                    breakdownResponse.Success = false;
                    breakdownResponse.Errors = loginResponse.Errors;
                }
                else if (failedResponse is GetLocationResponseDto locationResponse)
                {
                    locationResponse.Success = false;
                    locationResponse.Errors = loginResponse.Errors;
                }
                else if (failedResponse is GetInstructionSetsResponseDto instructionResponse)
                {
                    instructionResponse.Success = false;
                    instructionResponse.Errors = loginResponse.Errors;
                }
                
                return failedResponse;
            }

            sessionToken = loginResponse.SessionToken;

            // Step 2: Execute the action
            return await action(sessionToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing CAFM operation");
            
            // Return a failed response of type T
            var failedResponse = new T();
            var errorMessage = $"Operation failed: {ex.Message}";
            
            if (failedResponse is CreateWorkOrderResponseDto createWoResponse)
            {
                createWoResponse.Success = false;
                createWoResponse.Errors = new List<string> { errorMessage };
            }
            else if (failedResponse is GetBreakdownTaskResponseDto breakdownResponse)
            {
                breakdownResponse.Success = false;
                breakdownResponse.Errors = new List<string> { errorMessage };
            }
            else if (failedResponse is GetLocationResponseDto locationResponse)
            {
                locationResponse.Success = false;
                locationResponse.Errors = new List<string> { errorMessage };
            }
            else if (failedResponse is GetInstructionSetsResponseDto instructionResponse)
            {
                instructionResponse.Success = false;
                instructionResponse.Errors = new List<string> { errorMessage };
            }
            
            return failedResponse;
        }
        finally
        {
            // Step 3: Always logout (even if action failed)
            if (!string.IsNullOrEmpty(sessionToken))
            {
                try
                {
                    await _authService.LogoutAsync(sessionToken, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to logout from CAFM session");
                    // Don't throw here as the main operation might have succeeded
                }
            }
        }
    }
}