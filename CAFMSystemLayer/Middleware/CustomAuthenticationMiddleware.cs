using CAFMSystemLayer.Attributes;
using CAFMSystemLayer.ConfigModels;
using CAFMSystemLayer.Constants;
using CAFMSystemLayer.DTO.AtomicHandlerDTOs;
using CAFMSystemLayer.DTO.DownstreamDTOs;
using CAFMSystemLayer.Helper;
using CAFMSystemLayer.Implementations.CAFM.AtomicHandlers;
using Core.Extensions;
using Core.SystemLayer.Exceptions;
using Core.SystemLayer.Middlewares;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Reflection;

namespace CAFMSystemLayer.Middleware
{
    /// <summary>
    /// Custom authentication middleware for CAFM session-based authentication.
    /// Handles login before function execution and logout in finally block.
    /// </summary>
    public class CustomAuthenticationMiddleware : IFunctionsWorkerMiddleware
    {
        private readonly ILogger<CustomAuthenticationMiddleware> _logger;
        private readonly AppConfigs _appConfigs;
        private readonly AuthenticateAtomicHandler _authenticateAtomicHandler;
        private readonly LogoutAtomicHandler _logoutAtomicHandler;
        
        public CustomAuthenticationMiddleware(
            ILogger<CustomAuthenticationMiddleware> logger,
            IOptions<AppConfigs> options,
            AuthenticateAtomicHandler authenticateAtomicHandler,
            LogoutAtomicHandler logoutAtomicHandler)
        {
            _logger = logger;
            _appConfigs = options.Value;
            _authenticateAtomicHandler = authenticateAtomicHandler;
            _logoutAtomicHandler = logoutAtomicHandler;
        }
        
        public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            if (!ShouldApplyAuthentication(context))
            {
                await next(context);
                return;
            }
            
            string sessionId = string.Empty;
            
            try
            {
                _logger.Info("CustomAuthenticationMiddleware: Authenticating with CAFM");
                
                AuthenticationRequestDTO authRequest = new AuthenticationRequestDTO
                {
                    Username = _appConfigs.CAFMUsername,
                    Password = _appConfigs.CAFMPassword
                };
                
                HttpResponseSnapshot authResponse = await _authenticateAtomicHandler.Handle(authRequest);
                
                if (!authResponse.IsSuccessStatusCode)
                {
                    _logger.Error($"CAFM authentication failed: {authResponse.StatusCode}");
                    throw new DownStreamApiFailureException(
                        statusCode: System.Net.HttpStatusCode.Unauthorized,
                        error: ErrorConstants.CAFM_AUTHENT_0001,
                        errorDetails: [$"Authentication failed. Status: {authResponse.StatusCode}. Response: {authResponse.Content}"],
                        stepName: "CustomAuthenticationMiddleware.cs / Invoke"
                    );
                }
                
                AuthenticationResponseDTO? authData = SOAPHelper.DeserializeSoapResponse<AuthenticationResponseDTO>(authResponse.Content!);
                
                sessionId = authData?.SessionId ?? string.Empty;
                
                if (string.IsNullOrEmpty(sessionId))
                {
                    _logger.Error("Authentication succeeded but SessionId is null or empty");
                    throw new DownStreamApiFailureException(
                        statusCode: System.Net.HttpStatusCode.Unauthorized,
                        error: ErrorConstants.CAFM_AUTHENT_0002,
                        errorDetails: ["SessionId not returned from authentication"],
                        stepName: "CustomAuthenticationMiddleware.cs / Invoke"
                    );
                }
                
                RequestContext.SetSessionId(sessionId);
                _logger.Info($"CAFM authentication successful, SessionId stored");
                
                await next(context);
            }
            finally
            {
                if (!string.IsNullOrEmpty(sessionId))
                {
                    try
                    {
                        _logger.Info("CustomAuthenticationMiddleware: Logging out from CAFM");
                        
                        LogoutRequestDTO logoutRequest = new LogoutRequestDTO
                        {
                            SessionId = sessionId
                        };
                        
                        HttpResponseSnapshot logoutResponse = await _logoutAtomicHandler.Handle(logoutRequest);
                        
                        if (!logoutResponse.IsSuccessStatusCode)
                        {
                            _logger.Warn($"CAFM logout failed: {logoutResponse.StatusCode}");
                        }
                        else
                        {
                            _logger.Info("CAFM logout successful");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "Error during CAFM logout");
                    }
                    finally
                    {
                        RequestContext.Clear();
                    }
                }
            }
        }
        
        private bool ShouldApplyAuthentication(FunctionContext context)
        {
            string entryPoint = context.FunctionDefinition.EntryPoint;
            int lastDot = entryPoint.LastIndexOf('.');
            
            if (lastDot == -1)
                return false;
                
            string typeName = entryPoint.Substring(0, lastDot);
            string methodName = entryPoint.Substring(lastDot + 1);
            
            Type? type = Type.GetType(typeName);
            MethodInfo? method = type?.GetMethod(methodName);
            
            return method?.GetCustomAttribute<CustomAuthenticationAttribute>() != null;
        }
    }
}
