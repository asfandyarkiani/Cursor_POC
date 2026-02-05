using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SystemLayer.Application.Configuration;
using SystemLayer.Application.DTOs;
using SystemLayer.Application.Interfaces;

namespace SystemLayer.Infrastructure.Services;

public class CafmAuthenticationService : ICafmAuthenticationService
{
    private readonly ICafmClient _cafmClient;
    private readonly ICafmXmlBuilder _xmlBuilder;
    private readonly ICafmXmlParser _xmlParser;
    private readonly ILogger<CafmAuthenticationService> _logger;
    private readonly CafmConfiguration _config;

    public CafmAuthenticationService(
        ICafmClient cafmClient,
        ICafmXmlBuilder xmlBuilder,
        ICafmXmlParser xmlParser,
        ILogger<CafmAuthenticationService> logger,
        IOptions<CafmConfiguration> config)
    {
        _cafmClient = cafmClient ?? throw new ArgumentNullException(nameof(cafmClient));
        _xmlBuilder = xmlBuilder ?? throw new ArgumentNullException(nameof(xmlBuilder));
        _xmlParser = xmlParser ?? throw new ArgumentNullException(nameof(xmlParser));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _config = config.Value ?? throw new ArgumentNullException(nameof(config));
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Attempting CAFM login for user: {Username}", request.Username);

            var xmlRequest = _xmlBuilder.BuildLoginRequest(request);
            var xmlResponse = await _cafmClient.SendSoapRequestAsync(
                _config.Soap.LoginAction, 
                xmlRequest, 
                cancellationToken: cancellationToken);

            var response = _xmlParser.ParseLoginResponse(xmlResponse);

            if (response.Success)
            {
                _logger.LogInformation("CAFM login successful for user: {Username}", request.Username);
            }
            else
            {
                _logger.LogWarning("CAFM login failed for user: {Username}. Errors: {Errors}", 
                    request.Username, string.Join(", ", response.Errors ?? new List<string>()));
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during CAFM login for user: {Username}", request.Username);
            return new LoginResponseDto
            {
                Success = false,
                Errors = new List<string> { $"Login failed: {ex.Message}" }
            };
        }
    }

    public async Task<LogoutResponseDto> LogoutAsync(string sessionToken, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Attempting CAFM logout");

            var xmlRequest = _xmlBuilder.BuildLogoutRequest(sessionToken);
            var xmlResponse = await _cafmClient.SendSoapRequestAsync(
                _config.Soap.LogoutAction, 
                xmlRequest, 
                sessionToken, 
                cancellationToken);

            var response = _xmlParser.ParseLogoutResponse(xmlResponse);

            if (response.Success)
            {
                _logger.LogInformation("CAFM logout successful");
            }
            else
            {
                _logger.LogWarning("CAFM logout failed. Errors: {Errors}", 
                    string.Join(", ", response.Errors ?? new List<string>()));
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during CAFM logout");
            return new LogoutResponseDto
            {
                Success = false,
                Errors = new List<string> { $"Logout failed: {ex.Message}" }
            };
        }
    }
}