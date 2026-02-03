using AGI.ApiEcoSys.Core.Extensions;
using AGI.SysD365DriverLateLoginMgmt.Attributes;
using AGI.SysD365DriverLateLoginMgmt.Helper;
using AGI.SysD365DriverLateLoginMgmt.Implementations.D365.AtomicHandlers;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace AGI.SysD365DriverLateLoginMgmt.Middleware;

/// <summary>
/// Middleware for handling D365 OAuth2 authentication
/// Retrieves and caches authentication token for D365 API calls
/// </summary>
public class D365AuthenticationMiddleware : IFunctionsWorkerMiddleware
{
    private readonly ILogger<D365AuthenticationMiddleware> _logger;
    private readonly RequestContext _requestContext;
    private readonly AuthenticateD365AtomicHandler _authenticateHandler;

    public D365AuthenticationMiddleware(
        ILogger<D365AuthenticationMiddleware> logger,
        RequestContext requestContext,
        AuthenticateD365AtomicHandler authenticateHandler)
    {
        _logger = logger;
        _requestContext = requestContext;
        _authenticateHandler = authenticateHandler;
    }

    public async Task Invoke(FunctionContext functionContext, FunctionExecutionDelegate next)
    {
        string functionName = functionContext.FunctionDefinition.Name;

        try
        {
            // Check if function requires authentication
            MethodInfo? methodInfo = GetFunctionMethodInfo(functionContext);
            if (methodInfo == null)
            {
                _logger.LogWarning("Could not retrieve method info for function: {FunctionName}", functionName);
                await next(functionContext);
                return;
            }

            D365AuthenticationAttribute? authAttribute = methodInfo.GetCustomAttribute<D365AuthenticationAttribute>();
            if (authAttribute == null || !authAttribute.IsRequired)
            {
                _logger.LogInformation("Function {FunctionName} does not require D365 authentication", functionName);
                await next(functionContext);
                return;
            }

            _logger.LogInformation("Function {FunctionName} requires D365 authentication", functionName);

            // Check if token exists and is valid
            if (!string.IsNullOrWhiteSpace(_requestContext.AuthorizationToken) && !_requestContext.IsTokenExpired())
            {
                _logger.LogInformation("Using cached D365 authentication token for function: {FunctionName}", functionName);
            }
            else
            {
                _logger.LogInformation("Retrieving new D365 authentication token for function: {FunctionName}", functionName);

                // Retrieve new token
                DTO.DownstreamDTOs.AuthenticationResponseDTO authenticationResponse = await _authenticateHandler.AuthenticateAsync();

                // Store token in request context
                _requestContext.AuthorizationToken = $"Bearer {authenticationResponse.AccessToken}";
                _requestContext.IsTokenValid = true;

                // Parse and store token expiration
                if (long.TryParse(authenticationResponse.ExpiresOn, out long expiresOn))
                {
                    _requestContext.TokenExpiresOn = expiresOn;
                    _logger.LogInformation("D365 authentication token retrieved and cached. Expires on: {ExpiresOn}", expiresOn);
                }
                else
                {
                    _logger.LogWarning("Could not parse token expiration timestamp: {ExpiresOn}", authenticationResponse.ExpiresOn);
                }
            }

            // Proceed to next middleware/function
            await next(functionContext);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in D365AuthenticationMiddleware for function: {FunctionName}", functionName);
            throw;
        }
    }

    /// <summary>
    /// Gets the MethodInfo for the function being executed
    /// </summary>
    /// <param name="functionContext">Function context</param>
    /// <returns>MethodInfo or null</returns>
    private MethodInfo? GetFunctionMethodInfo(FunctionContext functionContext)
    {
        try
        {
            string functionName = functionContext.FunctionDefinition.Name;
            string? entryPoint = functionContext.FunctionDefinition.EntryPoint;

            if (string.IsNullOrWhiteSpace(entryPoint))
            {
                return null;
            }

            // EntryPoint format: "Namespace.ClassName.MethodName"
            string[] entryPointParts = entryPoint.Split('.');
            if (entryPointParts.Length < 2)
            {
                return null;
            }

            string methodName = entryPointParts[^1];
            string className = entryPointParts[^2];

            // Get all types in current assembly
            Assembly currentAssembly = Assembly.GetExecutingAssembly();
            Type? functionType = currentAssembly.GetTypes()
                .FirstOrDefault(t => t.Name == className);

            if (functionType == null)
            {
                return null;
            }

            // Get the method
            MethodInfo? methodInfo = functionType.GetMethod(methodName);
            return methodInfo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting method info for function: {FunctionName}", functionContext.FunctionDefinition.Name);
            return null;
        }
    }
}
