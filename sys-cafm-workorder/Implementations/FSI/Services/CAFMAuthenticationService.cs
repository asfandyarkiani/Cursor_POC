using Core.DTOs;
using Core.Extensions;
using Microsoft.Extensions.Logging;
using sys_cafm_workorder.Abstractions;
using sys_cafm_workorder.DTOs.Api.CAFM;
using sys_cafm_workorder.Implementations.FSI.Handlers;

namespace sys_cafm_workorder.Implementations.FSI.Services;

/// <summary>
/// Service implementation for CAFM Authentication operations.
/// Provides a clean interface for authentication-related operations.
/// </summary>
public class CAFMAuthenticationService : ICAFMAuthenticationService
{
    private readonly CAFMAuthenticateHandler _authenticateHandler;
    private readonly CAFMLogoutHandler _logoutHandler;
    private readonly ILogger<CAFMAuthenticationService> _logger;

    public CAFMAuthenticationService(
        CAFMAuthenticateHandler authenticateHandler,
        CAFMLogoutHandler logoutHandler,
        ILogger<CAFMAuthenticationService> logger)
    {
        _authenticateHandler = authenticateHandler;
        _logoutHandler = logoutHandler;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<BaseResponseDTO> AuthenticateAsync(AuthenticateRequestDto request)
    {
        _logger.Info("CAFMAuthenticationService: Processing authentication request");
        return await _authenticateHandler.HandleAsync(request);
    }

    /// <inheritdoc/>
    public async Task<BaseResponseDTO> LogoutAsync(LogoutRequestDto request)
    {
        _logger.Info("CAFMAuthenticationService: Processing logout request");
        return await _logoutHandler.HandleAsync(request);
    }
}
