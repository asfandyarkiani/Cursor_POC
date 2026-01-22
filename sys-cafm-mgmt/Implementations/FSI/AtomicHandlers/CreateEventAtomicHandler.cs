using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Xml.Linq;
using AGI.SystemLayer.CAFM.ConfigModels;
using AGI.SystemLayer.CAFM.DTOs.Downstream;
using Core.Middlewares;
using Core.SystemLayer.Handlers;

namespace AGI.SystemLayer.CAFM.Implementations.FSI.AtomicHandlers;

/// <summary>
/// Atomic Handler for CreateEvent/Link task SOAP operation.
/// Creates an event or links tasks in CAFM.
/// </summary>
public class CreateEventAtomicHandler : IAtomicHandler
{
    private readonly CustomHTTPClient _httpClient;
    private readonly AppConfigs _config;
    private readonly ILogger<CreateEventAtomicHandler> _logger;

    public CreateEventAtomicHandler(
        CustomHTTPClient httpClient,
        IOptions<AppConfigs> config,
        ILogger<CreateEventAtomicHandler> logger)
    {
        _httpClient = httpClient;
        _config = config.Value;
        _logger = logger;
    }

    public async Task<CreateEventResponseDTO> CreateEventAsync(
        CreateEventRequestDTO request,
        CancellationToken cancellationToken)
    {
        try
        {
            var soapRequest = BuildCreateEventSoapRequest(request);
            var url = $"{_config.CAFMSettings.BaseUrl}{_config.CAFMSettings.CreateEventResourcePath}";

            _logger.LogInformation("CAFM CreateEvent: Sending request to {Url}", url);

            var response = await _httpClient.PostAsync(
                url,
                new StringContent(soapRequest, System.Text.Encoding.UTF8, "text/xml"),
                new Dictionary<string, string>
                {
                    { "SOAPAction", _config.CAFMSettings.CreateEventSoapAction }
                },
                cancellationToken);

            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var eventId = ParseEventIdFromResponse(responseContent);

            if (string.IsNullOrEmpty(eventId))
            {
                _logger.LogError("CAFM CreateEvent: Failed to parse EventId from response");
                return new CreateEventResponseDTO
                {
                    IsSuccess = false,
                    ErrorMessage = "Failed to parse EventId from CreateEvent response"
                };
            }

            _logger.LogInformation("CAFM CreateEvent: Event created successfully. EventId: {EventId}", eventId);

            return new CreateEventResponseDTO
            {
                IsSuccess = true,
                EventId = eventId
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CAFM CreateEvent: Exception during CreateEvent");
            return new CreateEventResponseDTO
            {
                IsSuccess = false,
                ErrorMessage = $"Exception during CreateEvent: {ex.Message}"
            };
        }
    }

    private string BuildCreateEventSoapRequest(CreateEventRequestDTO request)
    {
        var details = request.EventDetails;

        return $@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/""
                  xmlns:ns=""http://www.fsi.co.uk/services/evolution/04/09""
                  xmlns:fsi1=""http://schemas.datacontract.org/2004/07/Fsi.Concept.Contracts.Entities.ServiceModel"">
    <soapenv:Header/>
    <soapenv:Body>
        <ns:CreateEvent>
            <ns:sessionId>{System.Security.SecurityElement.Escape(request.SessionId)}</ns:sessionId>
            <ns:eventDto>
                <fsi1:TaskId>{System.Security.SecurityElement.Escape(details.TaskId ?? "")}</fsi1:TaskId>
                <fsi1:EventType>{System.Security.SecurityElement.Escape(details.EventType ?? "")}</fsi1:EventType>
                <fsi1:EventDescription>{System.Security.SecurityElement.Escape(details.EventDescription ?? "")}</fsi1:EventDescription>
                <fsi1:LinkedTaskId>{System.Security.SecurityElement.Escape(details.LinkedTaskId ?? "")}</fsi1:LinkedTaskId>
            </ns:eventDto>
        </ns:CreateEvent>
    </soapenv:Body>
</soapenv:Envelope>";
    }

    private string? ParseEventIdFromResponse(string soapResponse)
    {
        try
        {
            var doc = XDocument.Parse(soapResponse);
            XNamespace ns = "http://www.fsi.co.uk/services/evolution/04/09";

            var eventIdElement = doc.Descendants(ns + "EventId").FirstOrDefault();
            return eventIdElement?.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CAFM CreateEvent: Error parsing EventId from SOAP response");
            return null;
        }
    }
}
