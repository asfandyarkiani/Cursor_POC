using Core.Extensions;
using Core.Middlewares;
using Core.SystemLayer.Handlers;
using Microsoft.Extensions.Logging;
using SysCafmMgmt.ConfigModels;
using SysCafmMgmt.DTOs.DownStream;
using System.Net;
using System.Text;
using System.Xml.Linq;
using Microsoft.Extensions.Options;
using Core.Exceptions;

namespace SysCafmMgmt.Implementations.FSI.AtomicHandlers
{
    /// <summary>
    /// Atomic handler for CreateEvent SOAP operation
    /// Creates an event/work order in FSI CAFM system
    /// </summary>
    public class CreateEventAtomicHandler : IAtomicHandler<CreateEventSoapResponseDTO>
    {
        private readonly CustomHTTPClient _httpClient;
        private readonly ILogger<CreateEventAtomicHandler> _logger;
        private readonly AppConfigs _appConfigs;

        public CreateEventAtomicHandler(
            CustomHTTPClient httpClient,
            ILogger<CreateEventAtomicHandler> logger,
            IOptions<AppConfigs> appConfigs)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _appConfigs = appConfigs?.Value ?? throw new ArgumentNullException(nameof(appConfigs));
        }

        public async Task<CreateEventSoapResponseDTO> Handle(Core.SystemLayer.DTOs.IDownStreamRequestDTO downStreamRequestDTO)
        {
            if (downStreamRequestDTO is not CreateEventSoapRequestDTO request)
            {
                throw new ArgumentException("Invalid request DTO type", nameof(downStreamRequestDTO));
            }

            _logger.Info($"[CreateEventAtomicHandler] Creating event in CAFM");

            try
            {
                string soapEnvelope = BuildSoapEnvelope(request);
                string url = $"{_appConfigs.CafmSettings.BaseUrl}{_appConfigs.CafmSettings.LoginResourcePath}";
                
                var headers = new List<Tuple<string, string>>
                {
                    new Tuple<string, string>("SOAPAction", _appConfigs.CafmSettings.CreateEventSoapAction),
                    new Tuple<string, string>("Content-Type", "text/xml; charset=utf-8")
                };

                HttpResponseMessage response = await _httpClient.SendAsync(
                    HttpMethod.Post,
                    url,
                    () => new StringContent(soapEnvelope, Encoding.UTF8, "text/xml"),
                    headers
                );

                string responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new HTTPBaseException(
                        $"CreateEvent failed: {response.StatusCode}",
                        "CAFM_CREATE_EVENT_FAILED",
                        response.StatusCode,
                        new List<string> { responseBody },
                        "CreateEventAtomicHandler",
                        true
                    );
                }

                return ParseResponse(responseBody);
            }
            catch (HTTPBaseException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"[CreateEventAtomicHandler] Exception: {ex.Message}");
                throw new HTTPBaseException(
                    $"CreateEvent exception: {ex.Message}",
                    "CAFM_CREATE_EVENT_EXCEPTION",
                    HttpStatusCode.InternalServerError,
                    new List<string> { ex.ToString() },
                    "CreateEventAtomicHandler",
                    true
                );
            }
        }

        private string BuildSoapEnvelope(CreateEventSoapRequestDTO request)
        {
            // TODO: Update based on actual CAFM WSDL
            return $@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:ns=""http://www.fsi.co.uk/services/evolution/04/09"">
   <soapenv:Header/>
   <soapenv:Body>
      <ns:CreateEvent>
         <ns:sessionId>{System.Security.SecurityElement.Escape(request.SessionId ?? string.Empty)}</ns:sessionId>
         <ns:eventType>{System.Security.SecurityElement.Escape(request.EventType ?? string.Empty)}</ns:eventType>
         <ns:description>{System.Security.SecurityElement.Escape(request.Description ?? string.Empty)}</ns:description>
         <ns:locationId>{System.Security.SecurityElement.Escape(request.LocationId ?? string.Empty)}</ns:locationId>
         <ns:priority>{System.Security.SecurityElement.Escape(request.Priority ?? string.Empty)}</ns:priority>
         <ns:scheduledDate>{System.Security.SecurityElement.Escape(request.ScheduledDate ?? string.Empty)}</ns:scheduledDate>
         <ns:taskId>{System.Security.SecurityElement.Escape(request.TaskId ?? string.Empty)}</ns:taskId>
      </ns:CreateEvent>
   </soapenv:Body>
</soapenv:Envelope>";
        }

        private CreateEventSoapResponseDTO ParseResponse(string soapResponse)
        {
            try
            {
                // TODO: Update parsing based on actual CAFM response
                XDocument doc = XDocument.Parse(soapResponse);
                XNamespace ns = "http://www.fsi.co.uk/services/evolution/04/09";

                var eventIdElement = doc.Descendants(ns + "EventId").FirstOrDefault();
                var eventNumberElement = doc.Descendants(ns + "EventNumber").FirstOrDefault();

                return new CreateEventSoapResponseDTO
                {
                    Success = true,
                    EventId = eventIdElement?.Value,
                    EventNumber = eventNumberElement?.Value,
                    RawResponse = soapResponse
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"[CreateEventAtomicHandler] Parse failed: {ex.Message}");
                return new CreateEventSoapResponseDTO
                {
                    Success = false,
                    ErrorMessage = $"Parse failed: {ex.Message}",
                    RawResponse = soapResponse
                };
            }
        }
    }
}
