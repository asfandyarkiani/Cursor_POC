using Core.Extensions;
using Core.SystemLayer.Exceptions;
using Core.SystemLayer.Middlewares;
using FsiCafmSystem.Attributes;
using FsiCafmSystem.ConfigModels;
using FsiCafmSystem.Constants;
using FsiCafmSystem.DTO.AtomicHandlerDTOs;
using FsiCafmSystem.DTO.DownstreamDTOs;
using FsiCafmSystem.Helper;
using FsiCafmSystem.Implementations.FsiCafm.AtomicHandlers;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Reflection;

namespace FsiCafmSystem.Middleware
{
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
                _logger.Info($"Authenticating for function: {context.FunctionDefinition.Name}");
                
                AuthenticateHandlerReqDTO request = new AuthenticateHandlerReqDTO
                {
                    Username = _appConfigs.FsiUsername ?? string.Empty,
                    Password = _appConfigs.FsiPassword ?? string.Empty
                };
                
                HttpResponseSnapshot authResponse = await _authenticateAtomicHandler.Handle(request);
                
                if (!authResponse.IsSuccessStatusCode)
                {
                    _logger.Error($"Authentication failed with status: {authResponse.StatusCode}");
                    throw new DownStreamApiFailureException(
                        statusCode: (HttpStatusCode)authResponse.StatusCode,
                        error: ErrorConstants.FSI_AUTHENT_0001,
                        errorDetails: new List<string> { $"Login API returned {authResponse.StatusCode}. Response: {authResponse.Content}" },
                        stepName: "CustomAuthenticationMiddleware / Invoke");
                }
                
                AuthenticateApiResDTO? authAPIRes = SOAPHelper.DeserializeSoapResponse<AuthenticateApiResDTO>(authResponse.Content!);
                
                if (authAPIRes == null || string.IsNullOrEmpty(authAPIRes.SessionId))
                {
                    _logger.Error("Authentication succeeded but SessionId is null or empty");
                    throw new DownStreamApiFailureException(
                        statusCode: HttpStatusCode.InternalServerError,
                        error: ErrorConstants.FSI_AUTHENT_0002,
                        errorDetails: new List<string> { "Expected SessionId but received null or empty" },
                        stepName: "CustomAuthenticationMiddleware / Invoke");
                }
                
                sessionId = authAPIRes.SessionId;
                RequestContext.SetSessionId(sessionId);
                
                _logger.Info($"Authentication successful. SessionId: {sessionId.Substring(0, Math.Min(8, sessionId.Length))}...");
                
                await next(context);
            }
            finally
            {
                if (!string.IsNullOrEmpty(sessionId))
                {
                    try
                    {
                        _logger.Info("Logging out from FSI CAFM session");
                        
                        LogoutHandlerReqDTO logoutRequest = new LogoutHandlerReqDTO
                        {
                            SessionId = sessionId
                        };
                        
                        HttpResponseSnapshot logoutResponse = await _logoutAtomicHandler.Handle(logoutRequest);
                        
                        if (logoutResponse.IsSuccessStatusCode)
                        {
                            _logger.Info("Logout successful");
                        }
                        else
                        {
                            _logger.Warn($"Logout failed with status: {logoutResponse.StatusCode}");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "Error during logout");
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
            string typeName = entryPoint.Substring(0, lastDot);
            string methodName = entryPoint.Substring(lastDot + 1);
            
            MethodInfo? method = Type.GetType(typeName)?.GetMethod(methodName);
            return method?.GetCustomAttribute<CustomAuthenticationAttribute>() != null;
        }
    }
}
