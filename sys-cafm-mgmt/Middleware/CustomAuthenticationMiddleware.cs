using Core.Extensions;
using Core.SystemLayer.Exceptions;
using Core.SystemLayer.Middlewares;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;
using sys_cafm_mgmt.Attributes;
using sys_cafm_mgmt.Constants;
using sys_cafm_mgmt.DTO.AtomicHandlerDTOs;
using sys_cafm_mgmt.Helper;
using sys_cafm_mgmt.Implementations.CAFM.AtomicHandlers;
using System.Reflection;

namespace sys_cafm_mgmt.Middleware
{
    public class CustomAuthenticationMiddleware : IFunctionsWorkerMiddleware
    {
        private readonly ILogger<CustomAuthenticationMiddleware> _logger;
        private readonly AuthenticateAtomicHandler _authenticateAtomicHandler;
        private readonly LogoutAtomicHandler _logoutAtomicHandler;

        public CustomAuthenticationMiddleware(
            ILogger<CustomAuthenticationMiddleware> logger,
            AuthenticateAtomicHandler authenticateAtomicHandler,
            LogoutAtomicHandler logoutAtomicHandler)
        {
            _logger = logger;
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
                _logger.Info("CustomAuthenticationMiddleware: Authenticating to CAFM system");
                
                AuthenticateHandlerReqDTO authenticationRequest = new AuthenticateHandlerReqDTO();
                HttpResponseSnapshot authenticationResponse = await _authenticateAtomicHandler.Handle(authenticationRequest);
                
                if (!authenticationResponse.IsSuccessStatusCode)
                {
                    _logger.Error($"Authentication failed with status: {authenticationResponse.StatusCode}");
                    throw new DownStreamApiFailureException(
                        statusCode: (System.Net.HttpStatusCode)authenticationResponse.StatusCode,
                        error: ErrorConstants.CAF_AUTHEN_0001,
                        errorDetails: [$"Authentication failed. Status: {authenticationResponse.StatusCode}. Response: {authenticationResponse.Content}"],
                        stepName: "CustomAuthenticationMiddleware.cs / Invoke");
                }
                
                AuthenticateApiResDTO? authenticationApiResponse = SOAPHelper.DeserializeSoapResponse<AuthenticateApiResDTO>(authenticationResponse.Content!);
                sessionId = authenticationApiResponse?.Envelope?.Body?.AuthenticateResponse?.AuthenticateResult?.SessionId ?? string.Empty;
                
                if (string.IsNullOrEmpty(sessionId))
                {
                    _logger.Error("Authentication returned empty SessionId");
                    throw new DownStreamApiFailureException(
                        statusCode: System.Net.HttpStatusCode.InternalServerError,
                        error: ErrorConstants.CAF_AUTHEN_0002,
                        errorDetails: ["CAFM authentication returned empty SessionId"],
                        stepName: "CustomAuthenticationMiddleware.cs / Invoke");
                }
                
                RequestContext.SetSessionId(sessionId);
                _logger.Info("Authentication successful, SessionId stored");
                
                await next(context);
            }
            finally
            {
                if (!string.IsNullOrEmpty(sessionId))
                {
                    _logger.Info("CustomAuthenticationMiddleware: Logging out from CAFM system");
                    
                    LogoutHandlerReqDTO logoutRequest = new LogoutHandlerReqDTO { SessionId = sessionId };
                    await _logoutAtomicHandler.Handle(logoutRequest);
                }
                
                RequestContext.Clear();
            }
        }

        private bool ShouldApplyAuthentication(FunctionContext context)
        {
            string entryPoint = context.FunctionDefinition.EntryPoint;
            int lastDotIndex = entryPoint.LastIndexOf('.');
            string typeName = entryPoint.Substring(0, lastDotIndex);
            string methodName = entryPoint.Substring(lastDotIndex + 1);
            
            MethodInfo? method = Type.GetType(typeName)?.GetMethod(methodName);
            return method?.GetCustomAttribute<CustomAuthenticationAttribute>() != null;
        }
    }
}
