using Core.Extensions;
using AGI.SysD365DriverLateLoginMgmt.Helper;
using Microsoft.Extensions.Logging;

namespace AGI.SysD365DriverLateLoginMgmt.Implementations.D365.AtomicHandlers;

/// <summary>
/// Atomic Handler for logging out from D365 (clearing cached token)
/// INTERNAL USE ONLY - Used by D365AuthenticationMiddleware
/// Note: D365 OAuth2 tokens are stateless, so logout just clears the cached token
/// </summary>
public class LogoutD365AtomicHandler
{
    private readonly RequestContext _requestContext;
    private readonly ILogger<LogoutD365AtomicHandler> _logger;

    public LogoutD365AtomicHandler(
        RequestContext requestContext,
        ILogger<LogoutD365AtomicHandler> logger)
    {
        _requestContext = requestContext;
        _logger = logger;
    }

    /// <summary>
    /// Clears the cached D365 authentication token
    /// </summary>
    public void Logout()
    {
        _logger.Info("Clearing D365 authentication token from cache");
        _requestContext.Clear();
        _logger.Info("D365 authentication token cleared successfully");
    }
}
