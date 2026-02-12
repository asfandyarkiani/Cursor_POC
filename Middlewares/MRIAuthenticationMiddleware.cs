using System.Reflection;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using FacilitiesMgmtSystem.Attributes;
using FacilitiesMgmtSystem.ConfigModels;
using FacilitiesMgmtSystem.Constants;
using FacilitiesMgmtSystem.Core.Exceptions;
using FacilitiesMgmtSystem.Helper;

namespace FacilitiesMgmtSystem.Middlewares;

/// <summary>
/// Middleware that handles MRI authentication for functions decorated with [MRIAuthentication] attribute.
/// Automatically logs in before function execution and logs out after completion.
/// </summary>
public class MRIAuthenticationMiddleware : IFunctionsWorkerMiddleware
{
    private readonly ILogger<MRIAuthenticationMiddleware> _logger;
    private readonly CustomSoapClient _soapClient;
    private readonly IOptions<AppConfigs> _appConfigs;

    public MRIAuthenticationMiddleware(
        ILogger<MRIAuthenticationMiddleware> logger,
        CustomSoapClient soapClient,
        IOptions<AppConfigs> appConfigs)
    {
        _logger = logger;
        _soapClient = soapClient;
        _appConfigs = appConfigs;
    }

    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        var mriAuthAttribute = GetMRIAuthenticationAttribute(context);
        
        if (mriAuthAttribute == null)
        {
            // No MRI authentication required, proceed normally
            await next(context);
            return;
        }

        string? sessionId = null;
        
        try
        {
            // Login to MRI
            _logger.Info("Initiating MRI login for function: {FunctionName}", context.FunctionDefinition.Name);
            sessionId = await PerformMRILogin();
            
            if (string.IsNullOrEmpty(sessionId))
            {
                throw new BaseException(ErrorConstants.MRI_LOGIN_FAILED);
            }

            // Store session ID in context for use by the function
            context.Items[InfoConstants.SESSION_ID] = sessionId;
            _logger.Info("MRI login successful. Session ID stored in context.");

            // Execute the function
            await next(context);
        }
        finally
        {
            // Logout from MRI if we have a session and auto-logout is enabled
            if (!string.IsNullOrEmpty(sessionId) && mriAuthAttribute.AutoLogout)
            {
                try
                {
                    _logger.Info("Initiating MRI logout for session.");
                    await PerformMRILogout(sessionId);
                    _logger.Info("MRI logout successful.");
                }
                catch (Exception ex)
                {
                    // Log logout failure but don't throw - the main operation may have succeeded
                    _logger.Warn("MRI logout failed: {Message}", ex.Message);
                }
            }
        }
    }

    private MRIAuthenticationAttribute? GetMRIAuthenticationAttribute(FunctionContext context)
    {
        // Get the target method from the function definition
        var entryPoint = context.FunctionDefinition.EntryPoint;
        var assemblyPath = context.FunctionDefinition.PathToAssembly;

        try
        {
            var assembly = Assembly.LoadFrom(assemblyPath);
            var typeName = entryPoint.Substring(0, entryPoint.LastIndexOf('.'));
            var methodName = entryPoint.Substring(entryPoint.LastIndexOf('.') + 1);

            var type = assembly.GetType(typeName);
            if (type == null) return null;

            var method = type.GetMethod(methodName);
            return method?.GetCustomAttribute<MRIAuthenticationAttribute>();
        }
        catch
        {
            return null;
        }
    }

    private async Task<string?> PerformMRILogin()
    {
        var config = _appConfigs.Value.MRI;
        
        // Load SOAP envelope template
        var envelope = SOAPHelper.LoadSoapEnvelope(InfoConstants.SOAP_ENVELOPE_LOGIN);
        
        // Replace placeholders with actual values
        envelope = envelope
            .Replace("{{Username}}", config.Username)
            .Replace("{{Password}}", config.Password)
            .Replace("{{ContractId}}", config.ContractId);

        var response = await _soapClient.SendSoapRequestAsync(
            config.BaseUrl + config.LoginEndpoint,
            envelope,
            InfoConstants.SOAP_ACTION_LOGIN);

        // Parse session ID from response
        var sessionId = SOAPHelper.ExtractValueFromXml(response.Content ?? string.Empty, "SessionId");
        return sessionId;
    }

    private async Task PerformMRILogout(string sessionId)
    {
        var config = _appConfigs.Value.MRI;
        
        // Load SOAP envelope template
        var envelope = SOAPHelper.LoadSoapEnvelope(InfoConstants.SOAP_ENVELOPE_LOGOUT);
        
        // Replace placeholders with actual values
        envelope = envelope.Replace("{{SessionId}}", sessionId);

        await _soapClient.SendSoapRequestAsync(
            config.BaseUrl + config.LogoutEndpoint,
            envelope,
            InfoConstants.SOAP_ACTION_LOGOUT);
    }
}
