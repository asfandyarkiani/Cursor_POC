using Core.Exceptions;
using Core.Extensions;
using Core.Middlewares;
using Core.SystemLayer.Handlers;
using Core.SystemLayer.Middlewares;
using AGI.SysD365DriverLateLoginMgmt.ConfigModels;
using AGI.SysD365DriverLateLoginMgmt.Constants;
using AGI.SysD365DriverLateLoginMgmt.DTO.AtomicHandlerDTOs;
using AGI.SysD365DriverLateLoginMgmt.DTO.DownstreamDTOs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace AGI.SysD365DriverLateLoginMgmt.Implementations.D365.AtomicHandlers;

/// <summary>
/// Atomic Handler for authenticating with D365 via OAuth2
/// Retrieves access token from Azure AD token endpoint
/// INTERNAL USE ONLY - Used by D365AuthenticationMiddleware
/// </summary>
public class AuthenticateD365AtomicHandler
{
    private readonly CustomRestClient _restClient;
    private readonly AppConfigs _appConfigs;
    private readonly ILogger<AuthenticateD365AtomicHandler> _logger;

    public AuthenticateD365AtomicHandler(
        CustomRestClient restClient,
        IOptions<AppConfigs> options,
        ILogger<AuthenticateD365AtomicHandler> logger)
    {
        _restClient = restClient;
        _appConfigs = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// Authenticates with D365 and retrieves OAuth2 access token
    /// </summary>
    /// <returns>Authentication response with access token</returns>
    public async Task<AuthenticationResponseDTO> AuthenticateAsync()
    {
        _logger.Info("Starting D365 authentication - retrieving OAuth2 token");

        try
        {
            // Build token request body (application/x-www-form-urlencoded)
            string tokenRequestBody = $"grant_type={_appConfigs.D365Config.GrantType}" +
                                     $"&client_id={_appConfigs.D365Config.ClientId}" +
                                     $"&client_secret={_appConfigs.D365Config.ClientSecret}" +
                                     $"&resource={_appConfigs.D365Config.Resource}";

            _logger.Info($"Calling Azure AD token endpoint: {_appConfigs.D365Config.TokenUrl}");

            // Call Azure AD token endpoint
            HttpResponseSnapshot tokenResponse = await _restClient.ExecuteCustomRestRequestAsync(
                operationName: "AuthenticateD365",
                apiUrl: _appConfigs.D365Config.TokenUrl,
                httpMethod: HttpMethod.Post,
                contentFactory: () => new StringContent(tokenRequestBody, System.Text.Encoding.UTF8, "application/x-www-form-urlencoded")
            );

            _logger.Info($"D365 authentication completed - Status: {tokenResponse.StatusCode}");

            if (!tokenResponse.IsSuccessStatusCode)
            {
                _logger.Error($"D365 authentication failed - Status: {tokenResponse.StatusCode}, Response: {tokenResponse.Content}");
                throw new RequestValidationFailureException(
                    errorDetails: [ErrorConstants.D365_AUTHEN_0001.Message, $"Status: {tokenResponse.StatusCode}", $"Response: {tokenResponse.Content}"],
                    stepName: "AuthenticateD365AtomicHandler.cs / AuthenticateAsync"
                );
            }
            else
            {
                // Deserialize token response
                AuthenticationResponseDTO? authResponse = JsonSerializer.Deserialize<AuthenticationResponseDTO>(tokenResponse.Content ?? "{}");

                if (authResponse == null || string.IsNullOrWhiteSpace(authResponse.AccessToken))
                {
                    _logger.Error("D365 authentication response is null or missing access token");
                    throw new RequestValidationFailureException(
                        errorDetails: [ErrorConstants.D365_AUTHEN_0002.Message, "Access token is missing in response"],
                        stepName: "AuthenticateD365AtomicHandler.cs / AuthenticateAsync"
                    );
                }
                else
                {
                    _logger.Info("D365 authentication successful - access token retrieved");
                    return authResponse;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Exception occurred during D365 authentication");
            throw;
        }
    }
}
