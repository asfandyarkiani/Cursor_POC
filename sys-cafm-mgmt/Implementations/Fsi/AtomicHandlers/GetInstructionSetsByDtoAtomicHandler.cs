using Core.Extensions;
using Core.Middlewares;
using Core.SystemLayer.Handlers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SysCafmMgmt.ConfigModels;
using SysCafmMgmt.DTOs.Downstream;
using System.Net;
using System.Text;
using System.Xml.Linq;

namespace SysCafmMgmt.Implementations.Fsi.AtomicHandlers
{
    /// <summary>
    /// Atomic handler for FSI GetInstructionSetsByDto SOAP operation
    /// </summary>
    public class GetInstructionSetsByDtoAtomicHandler : IAtomicHandler<GetInstructionSetsByDtoResponseDto>
    {
        private readonly CustomHTTPClient _httpClient;
        private readonly ILogger<GetInstructionSetsByDtoAtomicHandler> _logger;
        private readonly AppConfigs _config;

        private const string StepName = "FSI.GetInstructionSetsByDto";

        public GetInstructionSetsByDtoAtomicHandler(
            CustomHTTPClient httpClient,
            ILogger<GetInstructionSetsByDtoAtomicHandler> logger,
            IOptions<AppConfigs> config)
        {
            _httpClient = httpClient;
            _logger = logger;
            _config = config.Value;
        }

        public async Task<GetInstructionSetsByDtoResponseDto> Handle(Core.SystemLayer.DTOs.IDownStreamRequestDTO downStreamRequestDTO)
        {
            var request = downStreamRequestDTO as GetInstructionSetsByDtoRequestDto
                ?? throw new ArgumentException($"Expected {nameof(GetInstructionSetsByDtoRequestDto)}", nameof(downStreamRequestDTO));

            request.ValidateDownStreamRequestParameters();

            _logger.Info($"Getting instruction sets from CAFM FSI for category: {request.CategoryName}");

            var url = $"{_config.Cafm.BaseUrl}{_config.Cafm.BreakdownTaskResourcePath}";
            var soapEnvelope = request.ToSoapEnvelope();

            var headers = new List<Tuple<string, string>>
            {
                new("SOAPAction", _config.Cafm.SoapActionGetInstructionSets),
                new("Content-Type", "text/xml; charset=utf-8")
            };

            var response = await _httpClient.SendAsync(
                HttpMethod.Post,
                url,
                () => new StringContent(soapEnvelope, Encoding.UTF8, "text/xml"),
                headers
            );

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.Error($"FSI GetInstructionSetsByDto failed with status {response.StatusCode}: {errorContent}");

                throw new Core.SystemLayer.Exceptions.DownStreamApiFailureException(
                    message: "CAFM FSI GetInstructionSetsByDto failed",
                    errorCode: "CAFM_GET_INSTRUCTIONS_FAILED",
                    statusCode: response.StatusCode,
                    errorDetails: new List<string> { errorContent },
                    stepName: StepName
                );
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var result = ParseGetInstructionSetsResponse(responseContent);

            _logger.Info($"Retrieved {result.InstructionSets?.Count ?? 0} instruction sets from CAFM FSI");
            return result;
        }

        private GetInstructionSetsByDtoResponseDto ParseGetInstructionSetsResponse(string soapResponse)
        {
            try
            {
                var doc = XDocument.Parse(soapResponse);
                XNamespace ns1 = "http://schemas.xmlsoap.org/soap/envelope/";
                XNamespace ns2 = "http://www.fsi.co.uk/services/evolution/04/09";
                XNamespace ns3 = "http://schemas.datacontract.org/2004/07/Fsi.Platform.Contracts.ServiceModel";

                var body = doc.Descendants(ns1 + "Body").FirstOrDefault();
                var getInstructionsResponse = body?.Descendants(ns2 + "GetInstructionSetsByDtoResponse").FirstOrDefault();
                var getInstructionsResult = getInstructionsResponse?.Descendants(ns2 + "GetInstructionSetsByDtoResult").FirstOrDefault();
                var instructionSets = getInstructionsResult?.Descendants(ns3 + "InstructionSetDto");

                var result = new GetInstructionSetsByDtoResponseDto
                {
                    InstructionSets = instructionSets?.Select(ins => new InstructionSetDto
                    {
                        InstructionId = ins.Descendants(ns3 + "Id").FirstOrDefault()?.Value,
                        Code = ins.Descendants(ns3 + "Code").FirstOrDefault()?.Value,
                        Description = ins.Descendants(ns3 + "Description").FirstOrDefault()?.Value,
                        CategoryId = ins.Descendants(ns3 + "CategoryId").FirstOrDefault()?.Value,
                        DisciplineId = ins.Descendants(ns3 + "DisciplineId").FirstOrDefault()?.Value,
                        PriorityId = ins.Descendants(ns3 + "PriorityId").FirstOrDefault()?.Value
                    }).ToList() ?? new List<InstructionSetDto>()
                };

                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to parse GetInstructionSetsByDto response: {ex.Message}");
                return new GetInstructionSetsByDtoResponseDto { InstructionSets = new List<InstructionSetDto>() };
            }
        }
    }
}
