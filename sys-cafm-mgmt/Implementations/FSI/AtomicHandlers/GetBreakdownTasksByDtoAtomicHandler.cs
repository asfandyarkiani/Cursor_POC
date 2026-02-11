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
    /// Atomic handler for GetBreakdownTasksByDto SOAP operation
    /// </summary>
    public class GetBreakdownTasksByDtoAtomicHandler : IAtomicHandler<GetBreakdownTasksByDtoSoapResponseDTO>
    {
        private readonly CustomHTTPClient _httpClient;
        private readonly ILogger<GetBreakdownTasksByDtoAtomicHandler> _logger;
        private readonly AppConfigs _appConfigs;

        public GetBreakdownTasksByDtoAtomicHandler(
            CustomHTTPClient httpClient,
            ILogger<GetBreakdownTasksByDtoAtomicHandler> logger,
            IOptions<AppConfigs> appConfigs)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _appConfigs = appConfigs?.Value ?? throw new ArgumentNullException(nameof(appConfigs));
        }

        public async Task<GetBreakdownTasksByDtoSoapResponseDTO> Handle(Core.SystemLayer.DTOs.IDownStreamRequestDTO downStreamRequestDTO)
        {
            if (downStreamRequestDTO is not GetBreakdownTasksByDtoSoapRequestDTO request)
            {
                throw new ArgumentException("Invalid request DTO type", nameof(downStreamRequestDTO));
            }

            _logger.Info($"[GetBreakdownTasksByDtoAtomicHandler] Getting breakdown tasks");

            try
            {
                string soapEnvelope = BuildSoapEnvelope(request);
                string url = $"{_appConfigs.CafmSettings.BaseUrl}{_appConfigs.CafmSettings.LoginResourcePath}";
                
                var headers = new List<Tuple<string, string>>
                {
                    new Tuple<string, string>("SOAPAction", _appConfigs.CafmSettings.GetBreakdownTasksByDtoSoapAction),
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
                        $"GetBreakdownTasks failed: {response.StatusCode}",
                        "CAFM_GET_BREAKDOWN_TASKS_FAILED",
                        response.StatusCode,
                        new List<string> { responseBody },
                        "GetBreakdownTasksByDtoAtomicHandler",
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
                _logger.Error(ex, $"[GetBreakdownTasksByDtoAtomicHandler] Exception: {ex.Message}");
                throw new HTTPBaseException(
                    $"GetBreakdownTasks exception: {ex.Message}",
                    "CAFM_GET_BREAKDOWN_TASKS_EXCEPTION",
                    HttpStatusCode.InternalServerError,
                    new List<string> { ex.ToString() },
                    "GetBreakdownTasksByDtoAtomicHandler",
                    true
                );
            }
        }

        private string BuildSoapEnvelope(GetBreakdownTasksByDtoSoapRequestDTO request)
        {
            // TODO: Update based on actual CAFM WSDL
            return $@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:ns=""http://www.fsi.co.uk/services/evolution/04/09"">
   <soapenv:Header/>
   <soapenv:Body>
      <ns:GetBreakdownTasksByDto>
         <ns:sessionId>{System.Security.SecurityElement.Escape(request.SessionId ?? string.Empty)}</ns:sessionId>
         <ns:breakdownTaskCode>{System.Security.SecurityElement.Escape(request.BreakdownTaskCode ?? string.Empty)}</ns:breakdownTaskCode>
         <ns:categoryName>{System.Security.SecurityElement.Escape(request.CategoryName ?? string.Empty)}</ns:categoryName>
      </ns:GetBreakdownTasksByDto>
   </soapenv:Body>
</soapenv:Envelope>";
        }

        private GetBreakdownTasksByDtoSoapResponseDTO ParseResponse(string soapResponse)
        {
            try
            {
                // TODO: Update parsing based on actual CAFM response
                XDocument doc = XDocument.Parse(soapResponse);
                XNamespace ns = "http://www.fsi.co.uk/services/evolution/04/09";

                var tasks = new List<BreakdownTaskSoapDTO>();
                var elements = doc.Descendants(ns + "BreakdownTask");
                
                foreach (var element in elements)
                {
                    tasks.Add(new BreakdownTaskSoapDTO
                    {
                        BreakdownTaskId = element.Element(ns + "BreakdownTaskId")?.Value,
                        BreakdownTaskCode = element.Element(ns + "BreakdownTaskCode")?.Value,
                        BreakdownTaskName = element.Element(ns + "BreakdownTaskName")?.Value,
                        CategoryName = element.Element(ns + "CategoryName")?.Value,
                        Description = element.Element(ns + "Description")?.Value
                    });
                }

                return new GetBreakdownTasksByDtoSoapResponseDTO
                {
                    Success = true,
                    BreakdownTasks = tasks,
                    RawResponse = soapResponse
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"[GetBreakdownTasksByDtoAtomicHandler] Parse failed: {ex.Message}");
                return new GetBreakdownTasksByDtoSoapResponseDTO
                {
                    Success = false,
                    ErrorMessage = $"Parse failed: {ex.Message}",
                    RawResponse = soapResponse
                };
            }
        }
    }
}
