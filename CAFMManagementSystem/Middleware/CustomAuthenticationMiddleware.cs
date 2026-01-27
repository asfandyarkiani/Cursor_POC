using CAFMManagementSystem.Attributes;
using CAFMManagementSystem.ConfigModels;
using CAFMManagementSystem.Constants;
using CAFMManagementSystem.DTO.AtomicHandlerDTOs;
using CAFMManagementSystem.DTO.DownstreamDTOs;
using CAFMManagementSystem.Helper;
using CAFMManagementSystem.Implementations.FSIConcept.AtomicHandlers;
using Core.Exceptions;
using Core.Extensions;
using Core.SystemLayer.Exceptions;
using Core.SystemLayer.Middlewares;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Reflection;

namespace CAFMManagementSystem.Middleware
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
                _logger.Info("CustomAuthenticationMiddleware: Authenticating to CAFM");
                
                AuthenticateHandlerReqDTO authRequest = new AuthenticateHandlerReqDTO
                {
                    Username = string.Empty, // Will be fetched from KeyVault in AtomicHandler
                    Password = string.Empty  // Will be fetched from KeyVault in AtomicHandler
                };
                
                HttpResponseSnapshot authResponse = await _authenticateAtomicHandler.Handle(authRequest);
                
                if (!authResponse.IsSuccessStatusCode)
                {
                    _logger.Error($"CAFM authentication failed: {authResponse.StatusCode}");
                    throw new DownStreamApiFailureException(
                        statusCode: (HttpStatusCode)authResponse.StatusCode,
                        error: ErrorConstants.CAF_AUTHEN_0001,
                        errorDetails: [$"CAFM authentication failed. Status: {authResponse.StatusCode}. Response: {authResponse.Content}"],
                        stepName: "CustomAuthenticationMiddleware.cs / Invoke"
                    );
                }
                
                AuthenticateApiResDTO? authApiResponse = SOAPHelper.DeserializeSoapResponse<AuthenticateApiResDTO>(authResponse.Content!);
                sessionId = authApiResponse?.SessionId ?? string.Empty;
                
                if (string.IsNullOrEmpty(sessionId))
                {
                    _logger.Error("CAFM authentication returned empty SessionId");
                    throw new BaseException(ErrorConstants.CAF_AUTHEN_0002);
                }
                
                RequestContext.SetSessionId(sessionId);
                _logger.Info($"CAFM authentication successful. SessionId stored.");
                
                await next(context);
            }
            finally
            {
                if (!string.IsNullOrEmpty(sessionId))
                {
                    _logger.Info("CustomAuthenticationMiddleware: Logging out from CAFM");
                    
                    try
                    {
                        LogoutHandlerReqDTO logoutRequest = new LogoutHandlerReqDTO
                        {
                            SessionId = sessionId
                        };
                        
                        await _logoutAtomicHandler.Handle(logoutRequest);
                        _logger.Info("CAFM logout successful");
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "CAFM logout failed (non-critical)");
                    }
                    
                    RequestContext.Clear();
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
