using CAFMSystem.Attributes;
using CAFMSystem.ConfigModels;
using CAFMSystem.Constants;
using CAFMSystem.DTO.AtomicHandlerDTOs;
using CAFMSystem.Helper;
using CAFMSystem.Implementations.FSI.AtomicHandlers;
using Core.Extensions;
using Core.SystemLayer.Exceptions;
using Core.SystemLayer.Middlewares;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Reflection;

namespace CAFMSystem.Middleware
{
    public class CAFMAuthenticationMiddleware : IFunctionsWorkerMiddleware
    {
        private readonly ILogger<CAFMAuthenticationMiddleware> _logger;
        private readonly AppConfigs _appConfigs;
        private readonly AuthenticateCAFMAtomicHandler _authenticateAtomicHandler;
        private readonly LogoutCAFMAtomicHandler _logoutAtomicHandler;

        public CAFMAuthenticationMiddleware(
            ILogger<CAFMAuthenticationMiddleware> logger,
            IOptions<AppConfigs> options,
            AuthenticateCAFMAtomicHandler authenticateAtomicHandler,
            LogoutCAFMAtomicHandler logoutAtomicHandler)
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
                _logger.Info("CAFMAuthenticationMiddleware: Initiating CAFM login");
                
                AuthenticateCAFMHandlerReqDTO request = new AuthenticateCAFMHandlerReqDTO
                {
                    Username = _appConfigs.Username ?? string.Empty,
                    Password = _appConfigs.Password ?? string.Empty
                };
                
                HttpResponseSnapshot authResponse = await _authenticateAtomicHandler.Handle(request);
                
                if (!authResponse.IsSuccessStatusCode)
                {
                    _logger.Error($"CAFM authentication failed with status: {authResponse.StatusCode}");
                    throw new DownStreamApiFailureException(
                        statusCode: (HttpStatusCode)authResponse.StatusCode,
                        error: ErrorConstants.SYS_AUTHENT_0001,
                        errorDetails: [$"Login API returned {authResponse.StatusCode}. Response: {authResponse.Content}"],
                        stepName: "CAFMAuthenticationMiddleware / Invoke");
                }
                
                AuthenticateCAFMApiResDTO? authAPIRes = SOAPHelper.DeserializeSoapResponse<AuthenticateCAFMApiResDTO>(authResponse.Content!);
                
                sessionId = authAPIRes?.Envelope?.Body?.AuthenticateResponse?.AuthenticateResult?.SessionId ?? string.Empty;
                
                if (string.IsNullOrEmpty(sessionId))
                {
                    _logger.Error("CAFM authentication succeeded but no SessionId returned");
                    throw new DownStreamApiFailureException(
                        statusCode: HttpStatusCode.InternalServerError,
                        error: ErrorConstants.SYS_AUTHENT_0002,
                        errorDetails: ["Expected SessionId but received null or empty"],
                        stepName: "CAFMAuthenticationMiddleware / Invoke");
                }
                
                RequestContext.SetSessionId(sessionId);
                _logger.Info($"CAFM authentication successful. SessionId obtained.");
                
                await next(context);
            }
            finally
            {
                if (!string.IsNullOrEmpty(sessionId))
                {
                    try
                    {
                        _logger.Info("CAFMAuthenticationMiddleware: Initiating CAFM logout");
                        
                        LogoutCAFMHandlerReqDTO logoutRequest = new LogoutCAFMHandlerReqDTO
                        {
                            SessionId = sessionId
                        };
                        
                        await _logoutAtomicHandler.Handle(logoutRequest);
                        
                        _logger.Info("CAFM logout completed");
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "CAFM logout failed but continuing execution");
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
            return method?.GetCustomAttribute<CAFMAuthenticationAttribute>() != null;
        }
    }
}
