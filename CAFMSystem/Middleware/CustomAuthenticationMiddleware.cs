using CAFMSystem.Attributes;
using CAFMSystem.ConfigModels;
using CAFMSystem.Constants;
using CAFMSystem.DTO.AtomicHandlerDTOs;
using CAFMSystem.DTO.DownstreamDTOs;
using CAFMSystem.Helper;
using CAFMSystem.Implementations.FSI.AtomicHandlers;
using Core.Exceptions;
using Core.Extensions;
using Core.SystemLayer.Exceptions;
using Core.SystemLayer.Middlewares;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CAFMSystem.Middleware
{
    /// <summary>
    /// Custom authentication middleware for CAFM session-based authentication.
    /// Handles login (obtain SessionId) and logout lifecycle for functions marked with [CustomAuthentication].
    /// </summary>
    public class CustomAuthenticationMiddleware : IFunctionsWorkerMiddleware
    {
        private readonly ILogger<CustomAuthenticationMiddleware> _logger;
        private readonly AppConfigs _appConfigs;
        private readonly AuthenticateAtomicHandler _authenticateAtomicHandler;
        private readonly LogoutAtomicHandler _logoutAtomicHandler;
        private readonly KeyVaultReader _keyVaultReader;

        public CustomAuthenticationMiddleware(
            ILogger<CustomAuthenticationMiddleware> logger,
            IOptions<AppConfigs> options,
            AuthenticateAtomicHandler authenticateAtomicHandler,
            LogoutAtomicHandler logoutAtomicHandler,
            KeyVaultReader keyVaultReader)
        {
            _logger = logger;
            _appConfigs = options.Value;
            _authenticateAtomicHandler = authenticateAtomicHandler;
            _logoutAtomicHandler = logoutAtomicHandler;
            _keyVaultReader = keyVaultReader;
        }

        public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            // Check if function requires authentication
            if (!ShouldApplyAuthentication(context))
            {
                await next(context);
                return;
            }
            else
            {
                string sessionId = string.Empty;

                try
                {
                    // Step 1: Authenticate and get SessionId
                    _logger.Info("CustomAuthenticationMiddleware: Authenticating with CAFM");

                    Dictionary<string, string> authSecrets = await _keyVaultReader.GetAuthSecretsAsync();

                    AuthenticationRequestDTO authRequest = new AuthenticationRequestDTO
                    {
                        Username = authSecrets.GetValueOrDefault("Username", string.Empty),
                        Password = authSecrets.GetValueOrDefault("Password", string.Empty)
                    };

                    HttpResponseSnapshot authResponse = await _authenticateAtomicHandler.Handle(authRequest);

                    if (!authResponse.IsSuccessStatusCode)
                    {
                        _logger.Error($"CAFM authentication failed with status: {authResponse.StatusCode}");
                        throw new DownStreamApiFailureException(
                            statusCode: (HttpStatusCode)authResponse.StatusCode,
                            error: ErrorConstants.FSI_AUTHEN_0001,
                            errorDetails: new List<string> { $"CAFM Login API returned {authResponse.StatusCode}. Response: {authResponse.Content}" },
                            stepName: "CustomAuthenticationMiddleware.cs / Invoke"
                        );
                    }
                    else
                    {
                        // Extract SessionId from SOAP response
                        AuthenticationResponseDTO? authApiResponse = DeserializeAuthenticationResponse(authResponse.Content!);

                        sessionId = authApiResponse?.SessionId ?? string.Empty;

                        if (string.IsNullOrEmpty(sessionId))
                        {
                            throw new BaseException(ErrorConstants.FSI_AUTHEN_0002);
                        }
                        else
                        {
                            // Store SessionId in RequestContext
                            RequestContext.SetSessionId(sessionId);
                            _logger.Info($"CAFM authentication successful - SessionId obtained");

                            // Step 2: Execute function
                            await next(context);
                        }
                    }
                }
                finally
                {
                    // Step 3: Logout (always execute, even on exception)
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
                                _logger.Error($"CAFM logout failed with status: {logoutResponse.StatusCode} - Continuing anyway");
                            }
                            else
                            {
                                _logger.Info("CAFM logout successful");
                            }
                        }
                        catch (Exception logoutEx)
                        {
                            _logger.Error(logoutEx, "Exception during CAFM logout - Continuing anyway");
                        }
                        finally
                        {
                            // Clear RequestContext
                            RequestContext.Clear();
                        }
                    }
                    else
                    {
                        // No session to logout
                    }
                }
            }
        }

        private bool ShouldApplyAuthentication(FunctionContext context)
        {
            string entryPoint = context.FunctionDefinition.EntryPoint;
            int lastDot = entryPoint.LastIndexOf('.');

            if (lastDot < 0)
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

        private AuthenticationResponseDTO? DeserializeAuthenticationResponse(string xmlContent)
        {
            try
            {
                XDocument xdoc = XDocument.Parse(xmlContent);
                XNamespace soapNs = "http://schemas.xmlsoap.org/soap/envelope/";
                XNamespace ns = "http://www.fsi.co.uk/services/evolution/04/09";

                XElement? body = xdoc.Root?.Element(soapNs + "Body");
                XElement? response = body?.Element(ns + "AuthenticateResponse");
                XElement? result = response?.Element(ns + "AuthenticateResult");
                XElement? sessionIdElement = result?.Element(ns + "SessionId");

                if (sessionIdElement == null)
                {
                    return null;
                }
                else
                {
                    return new AuthenticationResponseDTO
                    {
                        SessionId = sessionIdElement.Value
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to deserialize authentication SOAP response");
                return null;
            }
        }
    }
}
