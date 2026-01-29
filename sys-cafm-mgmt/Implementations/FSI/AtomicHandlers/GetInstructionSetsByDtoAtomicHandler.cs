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
    /// Atomic handler for GetInstructionSetsByDto SOAP operation
    /// </summary>
    public class GetInstructionSetsByDtoAtomicHandler : IAtomicHandler<GetInstructionSetsByDtoSoapResponseDTO>
    {
        private readonly CustomHTTPClient _httpClient;
        private readonly ILogger<GetInstructionSetsByDtoAtomicHandler> _logger;
        private readonly AppConfigs _appConfigs;

        public GetInstructionSetsByDtoAtomicHandler(
            CustomHTTPClient httpClient,
            ILogger<GetInstructionSetsByDtoAtomicHandler> logger,
            IOptions<AppConfigs> appConfigs)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _appConfigs = appConfigs?.Value ?? throw new ArgumentNullException(nameof(appConfigs));
        }

        public async Task<GetInstructionSetsByDtoSoapResponseDTO> Handle(Core.SystemLayer.DTOs.IDownStreamRequestDTO downStreamRequestDTO)
        {
            if (downStreamRequestDTO is not GetInstructionSetsByDtoSoapRequestDTO request)
            {
                throw new ArgumentException("Invalid request DTO type", nameof(downStreamRequestDTO));
            }

            _logger.Info($"[GetInstructionSetsByDtoAtomicHandler] Getting instruction sets");

            try
            {
                string soapEnvelope = BuildSoapEnvelope(request);
                string url = $"{_appConfigs.CafmSettings.BaseUrl}{_appConfigs.CafmSettings.LoginResourcePath}";
                
                var headers = new List<Tuple<string, string>>
                {
                    new Tuple<string, string>("SOAPAction", _appConfigs.CafmSettings.GetInstructionSetsByDtoSoapAction),
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
                        $"GetInstructionSets failed: {response.StatusCode}",
                        "CAFM_GET_INSTRUCTION_SETS_FAILED",
                        response.StatusCode,
                        new List<string> { responseBody },
                        "GetInstructionSetsByDtoAtomicHandler",
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
                _logger.Error(ex, $"[GetInstructionSetsByDtoAtomicHandler] Exception: {ex.Message}");
                throw new HTTPBaseException(
                    $"GetInstructionSets exception: {ex.Message}",
                    "CAFM_GET_INSTRUCTION_SETS_EXCEPTION",
                    HttpStatusCode.InternalServerError,
                    new List<string> { ex.ToString() },
                    "GetInstructionSetsByDtoAtomicHandler",
                    true
                );
            }
        }

        private string BuildSoapEnvelope(GetInstructionSetsByDtoSoapRequestDTO request)
        {
            // TODO: Update based on actual CAFM WSDL
            return $@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:ns=""http://www.fsi.co.uk/services/evolution/04/09"">
   <soapenv:Header/>
   <soapenv:Body>
      <ns:GetInstructionSetsByDto>
         <ns:sessionId>{System.Security.SecurityElement.Escape(request.SessionId ?? string.Empty)}</ns:sessionId>
         <ns:instructionSetCode>{System.Security.SecurityElement.Escape(request.InstructionSetCode ?? string.Empty)}</ns:instructionSetCode>
         <ns:categoryName>{System.Security.SecurityElement.Escape(request.CategoryName ?? string.Empty)}</ns:categoryName>
      </ns:GetInstructionSetsByDto>
   </soapenv:Body>
</soapenv:Envelope>";
        }

        private GetInstructionSetsByDtoSoapResponseDTO ParseResponse(string soapResponse)
        {
            try
            {
                // TODO: Update parsing based on actual CAFM response
                XDocument doc = XDocument.Parse(soapResponse);
                XNamespace ns = "http://www.fsi.co.uk/services/evolution/04/09";

                var instructionSets = new List<InstructionSetSoapDTO>();
                var elements = doc.Descendants(ns + "InstructionSet");
                
                foreach (var element in elements)
                {
                    instructionSets.Add(new InstructionSetSoapDTO
                    {
                        InstructionSetId = element.Element(ns + "InstructionSetId")?.Value,
                        InstructionSetCode = element.Element(ns + "InstructionSetCode")?.Value,
                        InstructionSetName = element.Element(ns + "InstructionSetName")?.Value,
                        CategoryName = element.Element(ns + "CategoryName")?.Value,
                        Description = element.Element(ns + "Description")?.Value
                    });
                }

                return new GetInstructionSetsByDtoSoapResponseDTO
                {
                    Success = true,
                    InstructionSets = instructionSets,
                    RawResponse = soapResponse
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"[GetInstructionSetsByDtoAtomicHandler] Parse failed: {ex.Message}");
                return new GetInstructionSetsByDtoSoapResponseDTO
                {
                    Success = false,
                    ErrorMessage = $"Parse failed: {ex.Message}",
                    RawResponse = soapResponse
                };
            }
        }
    }
}
