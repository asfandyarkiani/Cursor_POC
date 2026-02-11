using CAFMSystem.Attributes;
using CAFMSystem.ConfigModels;
using CAFMSystem.Constants;
using CAFMSystem.DTO.AtomicHandlerDTOs;
using CAFMSystem.DTO.DownstreamDTOs;
using CAFMSystem.Helper;
using CAFMSystem.Implementations.CAFM.AtomicHandlers;
using Core.Extensions;
using Core.SystemLayer.Exceptions;
using Core.SystemLayer.Middlewares;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace CAFMSystem.Middleware
{
    public class CustomAuthenticationMiddleware : IFunctionsWorkerMiddleware
    {
        private readonly ILogger<CustomAuthenticationMiddleware> _logger;
        private readonly AppConfigs _appConfigs;
        private readonly KeyVaultReader _keyVaultReader;
        private readonly AuthenticateAtomicHandler _authenticateAtomicHandler;
        private readonly LogoutAtomicHandler _logoutAtomicHandler;

        public CustomAuthenticationMiddleware(
            ILogger<CustomAuthenticationMiddleware> logger,
            IOptions<AppConfigs> options,
            KeyVaultReader keyVaultReader,
            AuthenticateAtomicHandler authenticateAtomicHandler,
            LogoutAtomicHandler logoutAtomicHandler)
        {
            _logger = logger;
            _appConfigs = options.Value;
            _keyVaultReader = keyVaultReader;
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
                _logger.Info($"CustomAuthenticationMiddleware: Authenticating for function {context.FunctionDefinition.Name}");
                
                System.Collections.Generic.Dictionary<string, string> secrets = await _keyVaultReader.GetAuthSecretsAsync();
                
                string username = secrets.GetValueOrDefault("FSI_Username") ?? string.Empty;
                string password = secrets.GetValueOrDefault("FSI_Password") ?? string.Empty;
                
                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                {
                    _logger.Error("FSI credentials not found in KeyVault");
                    throw new InvalidOperationException("FSI credentials are required for authentication");
                }
                else
                {
                    AuthenticationHandlerReqDTO authRequest = new AuthenticationHandlerReqDTO
                    {
                        Username = username,
                        Password = password
                    };
                    
                    HttpResponseSnapshot authResponse = await _authenticateAtomicHandler.Handle(authRequest);
                    
                    if (!authResponse.IsSuccessStatusCode)
                    {
                        _logger.Error($"Authentication failed with status: {authResponse.StatusCode}");
                        throw new DownStreamApiFailureException(
                            statusCode: (HttpStatusCode)authResponse.StatusCode,
                            error: ErrorConstants.CAF_AUTHEN_0001,
                            errorDetails: new string[] { $"CAFM authentication failed. Status: {authResponse.StatusCode}. Response: {authResponse.Content}" },
                            stepName: "CustomAuthenticationMiddleware.cs / Invoke");
                    }
                    else
                    {
                        AuthenticationApiResDTO? authAPIRes = SOAPHelper.DeserializeSoapResponse<AuthenticationApiResDTO>(authResponse.Content!);
                        
                        sessionId = authAPIRes?.SessionId ?? string.Empty;
                        
                        if (string.IsNullOrWhiteSpace(sessionId))
                        {
                            _logger.Error("CAFM authentication returned empty SessionId");
                            throw new NoResponseBodyException(
                                errorDetails: new string[] { "CAFM authentication succeeded but returned empty SessionId" },
                                stepName: "CustomAuthenticationMiddleware.cs / Invoke");
                        }
                        else
                        {
                            RequestContext.SetSessionId(sessionId);
                            _logger.Info($"Authentication successful - SessionId stored in RequestContext");
                            
                            await next(context);
                        }
                    }
                }
            }
            finally
            {
                if (!string.IsNullOrWhiteSpace(sessionId))
                {
                    try
                    {
                        _logger.Info("CustomAuthenticationMiddleware: Logging out");
                        
                        LogoutHandlerReqDTO logoutRequest = new LogoutHandlerReqDTO
                        {
                            SessionId = sessionId
                        };
                        
                        await _logoutAtomicHandler.Handle(logoutRequest);
                        
                        _logger.Info("Logout completed successfully");
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "Logout failed - Session may remain active");
                    }
                    finally
                    {
                        RequestContext.Clear();
                    }
                }
                else
                {
                    _logger.Debug("No SessionId to logout");
                }
            }
        }

        private bool ShouldApplyAuthentication(FunctionContext context)
        {
            string entryPoint = context.FunctionDefinition.EntryPoint;
            int lastDot = entryPoint.LastIndexOf('.');
            
            if (lastDot == -1)
            {
                return false;
            }
            else
            {
                string typeName = entryPoint.Substring(0, lastDot);
                string methodName = entryPoint.Substring(lastDot + 1);
                
                Type? type = Type.GetType(typeName);
                MethodInfo? method = type?.GetMethod(methodName);
                
                return method?.GetCustomAttribute<CustomAuthenticationAttribute>() != null;
            }
        }
    }
}
