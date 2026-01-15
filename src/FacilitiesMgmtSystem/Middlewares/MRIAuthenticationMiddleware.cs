using System.Reflection;
using FacilitiesMgmtSystem.Attributes;
using FacilitiesMgmtSystem.ConfigModels;
using FacilitiesMgmtSystem.Constants;
using FacilitiesMgmtSystem.Core.Exceptions;
using FacilitiesMgmtSystem.Helper;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FacilitiesMgmtSystem.Middlewares;

/// <summary>
/// Middleware to handle MRI authentication for functions decorated with [MRIAuthentication].
/// </summary>
public class MRIAuthenticationMiddleware : IFunctionsWorkerMiddleware
{
    private readonly ILogger<MRIAuthenticationMiddleware> _logger;
    private readonly AppConfigs _appConfigs;

    public MRIAuthenticationMiddleware(
        ILogger<MRIAuthenticationMiddleware> logger,
        IOptions<AppConfigs> appConfigs)
    {
        _logger = logger;
        _appConfigs = appConfigs.Value;
    }

    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        var requiresAuthentication = RequiresMRIAuthentication(context);
        string? sessionId = null;

        if (requiresAuthentication)
        {
            try
            {
                _logger.Info("MRI authentication required for function {FunctionName}.", 
                    context.FunctionDefinition.Name);
                
                // TODO: Implement actual MRI login using CustomSoapClient
                // This would call the MRI Login SOAP endpoint and obtain a session ID
                sessionId = await AuthenticateWithMRI();
                
                if (string.IsNullOrEmpty(sessionId))
                {
                    throw new BaseException(ErrorConstants.MRI_AUTHENTICATION_FAILED);
                }

                context.Items[InfoConstants.SESSION_ID] = sessionId;
                _logger.Info("MRI authentication successful. Session established.");
            }
            catch (Exception ex) when (ex is not BaseException)
            {
                _logger.Error(ex, "MRI authentication failed.");
                throw new BaseException(ErrorConstants.MRI_AUTHENTICATION_FAILED, ex);
            }
        }

        try
        {
            await next(context);
        }
        finally
        {
            if (requiresAuthentication && !string.IsNullOrEmpty(sessionId))
            {
                try
                {
                    _logger.Info("Logging out from MRI session.");
                    // TODO: Implement actual MRI logout using CustomSoapClient
                    await LogoutFromMRI(sessionId);
                }
                catch (Exception ex)
                {
                    _logger.Warn("MRI logout failed: {Message}", ex.Message);
                    // Don't throw on logout failure - the main operation has completed
                }
            }
        }
    }

    private static bool RequiresMRIAuthentication(FunctionContext context)
    {
        var targetMethod = context.FunctionDefinition.Name;
        
        // Check if the function method has the MRIAuthentication attribute
        var entryPoint = context.FunctionDefinition.EntryPoint;
        var lastDot = entryPoint.LastIndexOf('.');
        if (lastDot > 0)
        {
            var typeName = entryPoint.Substring(0, lastDot);
            var methodName = entryPoint.Substring(lastDot + 1);
            
            var type = Assembly.GetExecutingAssembly().GetType(typeName);
            var method = type?.GetMethod(methodName);
            
            return method?.GetCustomAttribute<MRIAuthenticationAttribute>() != null;
        }
        
        return false;
    }

    private async Task<string?> AuthenticateWithMRI()
    {
        // TODO: Implement actual MRI login
        // This should use CustomSoapClient to call the MRI Login SOAP endpoint
        // For now, return a placeholder to allow the infrastructure to work
        await Task.CompletedTask;
        
        // In production, this would be something like:
        // var loginRequest = new MRILoginRequest { ... };
        // var response = await _soapClient.SendAsync<MRILoginResponse>("Login", loginRequest);
        // return response.SessionId;
        
        return $"MRI_SESSION_{Guid.NewGuid():N}";
    }

    private async Task LogoutFromMRI(string sessionId)
    {
        // TODO: Implement actual MRI logout
        // This should use CustomSoapClient to call the MRI Logout SOAP endpoint
        await Task.CompletedTask;
        
        _logger.Debug("MRI session {SessionId} logged out.", sessionId);
    }
}
