using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;
using AGI.SystemLayer.CAFM.Attributes;
using AGI.SystemLayer.CAFM.Implementations.FSI.AtomicHandlers;
using System.Reflection;

namespace AGI.SystemLayer.CAFM.Middlewares;

/// <summary>
/// Middleware to handle automatic CAFM authentication (login/logout) for functions
/// decorated with the CAFMAuthenticationAttribute.
/// 
/// This middleware:
/// 1. Checks if the function has the CAFMAuthenticationAttribute
/// 2. Performs login before the function executes
/// 3. Stores the session ID in the function context
/// 4. Performs logout after the function completes (if AutoLogout is true)
/// </summary>
public class CAFMAuthenticationMiddleware : IFunctionsWorkerMiddleware
{
    private readonly ILogger<CAFMAuthenticationMiddleware> _logger;
    private readonly LoginAtomicHandler _loginHandler;
    private readonly LogoutAtomicHandler _logoutHandler;

    public CAFMAuthenticationMiddleware(
        ILogger<CAFMAuthenticationMiddleware> logger,
        LoginAtomicHandler loginHandler,
        LogoutAtomicHandler logoutHandler)
    {
        _logger = logger;
        _loginHandler = loginHandler;
        _logoutHandler = logoutHandler;
    }

    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        var functionName = context.FunctionDefinition.Name;
        var targetMethod = GetTargetMethod(context);

        if (targetMethod == null)
        {
            await next(context);
            return;
        }

        var authAttribute = targetMethod.GetCustomAttribute<CAFMAuthenticationAttribute>();

        if (authAttribute == null)
        {
            // No authentication required
            await next(context);
            return;
        }

        string? sessionId = null;

        try
        {
            // Perform login
            _logger.LogInformation("CAFM Authentication: Logging in for function {FunctionName}", functionName);
            var loginResponse = await _loginHandler.AuthenticateAsync(CancellationToken.None);

            if (!loginResponse.IsSuccess || string.IsNullOrEmpty(loginResponse.SessionId))
            {
                _logger.LogError("CAFM Authentication: Login failed for function {FunctionName}. Error: {Error}",
                    functionName, loginResponse.ErrorMessage);
                throw new UnauthorizedAccessException($"CAFM login failed: {loginResponse.ErrorMessage}");
            }

            sessionId = loginResponse.SessionId;
            _logger.LogInformation("CAFM Authentication: Login successful for function {FunctionName}. SessionId: {SessionId}",
                functionName, sessionId);

            // Store session ID in context for use by handlers
            context.Items["CAFMSessionId"] = sessionId;

            // Execute the function
            await next(context);
        }
        finally
        {
            // Perform logout if configured and session exists
            if (authAttribute.AutoLogout && !string.IsNullOrEmpty(sessionId))
            {
                try
                {
                    _logger.LogInformation("CAFM Authentication: Logging out for function {FunctionName}. SessionId: {SessionId}",
                        functionName, sessionId);
                    await _logoutHandler.LogoutAsync(sessionId, CancellationToken.None);
                    _logger.LogInformation("CAFM Authentication: Logout successful for function {FunctionName}", functionName);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "CAFM Authentication: Logout failed for function {FunctionName}. SessionId: {SessionId}",
                        functionName, sessionId);
                    // Don't throw - logout failure shouldn't fail the request
                }
            }
        }
    }

    private static MethodInfo? GetTargetMethod(FunctionContext context)
    {
        var entryPoint = context.FunctionDefinition.EntryPoint;
        var parts = entryPoint.Split('.');
        if (parts.Length < 2) return null;

        var typeName = string.Join(".", parts.Take(parts.Length - 1));
        var methodName = parts.Last();

        var assembly = Assembly.GetExecutingAssembly();
        var type = assembly.GetType(typeName);

        return type?.GetMethod(methodName);
    }
}
