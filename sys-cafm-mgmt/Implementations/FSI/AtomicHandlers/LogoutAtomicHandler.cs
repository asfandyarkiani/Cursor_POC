using Core.Extensions;
using Core.Middlewares;
using Core.SystemLayer.Handlers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SysCafmMgmt.ConfigModels;
using SysCafmMgmt.DTOs.Downstream;
using System.Text;

namespace SysCafmMgmt.Implementations.FSI.AtomicHandlers;

/// <summary>
/// Atomic handler for FSI LogOut SOAP operation
/// Single responsibility: End session
/// </summary>
public class LogoutAtomicHandler : IAtomicHandler<bool>
{
    private readonly CustomHTTPClient _httpClient;
    private readonly ILogger<LogoutAtomicHandler> _logger;
    private readonly FsiConfig _fsiConfig;
    private const string StepName = "FSI_Logout";

    public LogoutAtomicHandler(
        CustomHTTPClient httpClient,
        ILogger<LogoutAtomicHandler> logger,
        IOptions<AppConfigs> appConfigs)
    {
        _httpClient = httpClient;
        _logger = logger;
        _fsiConfig = appConfigs.Value.Fsi;
    }

    public async Task<bool> Handle(Core.SystemLayer.DTOs.IDownStreamRequestDTO downStreamRequestDTO)
    {
        var request = (FsiLogoutRequestDTO)downStreamRequestDTO;
        request.ValidateDownStreamRequestParameters();

        var url = $"{_fsiConfig.BaseUrl}{_fsiConfig.LogoutResourcePath}";
        var soapBody = request.ToSoapRequest();

        _logger.Info($"Calling FSI Logout at {url}");

        var headers = new List<Tuple<string, string>>
        {
            Tuple.Create("SOAPAction", _fsiConfig.SoapActionLogout),
            Tuple.Create("Content-Type", "text/xml; charset=utf-8")
        };

        try
        {
            var response = await _httpClient.SendAsync(
                HttpMethod.Post,
                url,
                () => new StringContent(soapBody, Encoding.UTF8, "text/xml"),
                headers);

            // Logout is best-effort, we don't fail the main operation if logout fails
            if (response.IsSuccessStatusCode)
            {
                _logger.Info("FSI Logout successful");
                return true;
            }
            else
            {
                _logger.Warn($"FSI Logout returned non-success status: {response.StatusCode}");
                return false;
            }
        }
        catch (Exception ex)
        {
            // Logout failures should not propagate
            _logger.Warn($"FSI Logout failed with exception: {ex.Message}");
            return false;
        }
    }
}
