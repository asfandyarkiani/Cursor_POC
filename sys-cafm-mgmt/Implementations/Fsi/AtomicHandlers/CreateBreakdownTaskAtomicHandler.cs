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
    /// Atomic handler for FSI CreateBreakdownTask SOAP operation
    /// </summary>
    public class CreateBreakdownTaskAtomicHandler : IAtomicHandler<CreateBreakdownTaskResponseDto>
    {
        private readonly CustomHTTPClient _httpClient;
        private readonly ILogger<CreateBreakdownTaskAtomicHandler> _logger;
        private readonly AppConfigs _config;

        private const string StepName = "FSI.CreateBreakdownTask";

        public CreateBreakdownTaskAtomicHandler(
            CustomHTTPClient httpClient,
            ILogger<CreateBreakdownTaskAtomicHandler> logger,
            IOptions<AppConfigs> config)
        {
            _httpClient = httpClient;
            _logger = logger;
            _config = config.Value;
        }

        public async Task<CreateBreakdownTaskResponseDto> Handle(Core.SystemLayer.DTOs.IDownStreamRequestDTO downStreamRequestDTO)
        {
            var request = downStreamRequestDTO as CreateBreakdownTaskRequestDto
                ?? throw new ArgumentException($"Expected {nameof(CreateBreakdownTaskRequestDto)}", nameof(downStreamRequestDTO));

            request.ValidateDownStreamRequestParameters();

            _logger.Info($"Creating breakdown task in CAFM FSI for CallId: {request.CallId}");

            var url = $"{_config.Cafm.BaseUrl}{_config.Cafm.BreakdownTaskResourcePath}";
            var soapEnvelope = request.ToSoapEnvelope();

            var headers = new List<Tuple<string, string>>
            {
                new("SOAPAction", _config.Cafm.SoapActionCreateBreakdownTask),
                new("Content-Type", "text/xml; charset=utf-8")
            };

            var response = await _httpClient.SendAsync(
                HttpMethod.Post,
                url,
                () => new StringContent(soapEnvelope, Encoding.UTF8, "text/xml"),
                headers
            );

            var responseContent = await response.Content.ReadAsStringAsync();

            // Check HTTP status code for 2xx range (same as Boomi's 20* check)
            if (!response.IsSuccessStatusCode)
            {
                _logger.Error($"FSI CreateBreakdownTask failed with status {response.StatusCode}: {responseContent}");

                throw new Core.SystemLayer.Exceptions.DownStreamApiFailureException(
                    message: "CAFM FSI CreateBreakdownTask failed",
                    errorCode: "CAFM_CREATE_TASK_FAILED",
                    statusCode: response.StatusCode,
                    errorDetails: new List<string> { responseContent },
                    stepName: StepName
                );
            }

            var result = ParseCreateBreakdownTaskResponse(responseContent);

            if (!result.IsSuccess)
            {
                _logger.Error($"FSI CreateBreakdownTask returned without TaskId. Response: {responseContent}");

                throw new Core.SystemLayer.Exceptions.DownStreamApiFailureException(
                    message: "CAFM FSI CreateBreakdownTask did not return a TaskId",
                    errorCode: "CAFM_CREATE_TASK_NO_ID",
                    statusCode: HttpStatusCode.InternalServerError,
                    errorDetails: new List<string> { responseContent },
                    stepName: StepName
                );
            }

            _logger.Info($"Successfully created breakdown task in CAFM FSI. TaskId: {result.TaskId}");
            return result;
        }

        private CreateBreakdownTaskResponseDto ParseCreateBreakdownTaskResponse(string soapResponse)
        {
            try
            {
                var doc = XDocument.Parse(soapResponse);
                XNamespace ns1 = "http://schemas.xmlsoap.org/soap/envelope/";
                XNamespace ns2 = "http://www.fsi.co.uk/services/evolution/04/09";
                XNamespace ns3 = "http://schemas.datacontract.org/2004/07/Fsi.Platform.Contracts.ServiceModel";
                XNamespace ns4 = "http://schemas.datacontract.org/2004/07/Fsi.Platform.Contracts.Entities.ServiceModel";

                var body = doc.Descendants(ns1 + "Body").FirstOrDefault();
                var createResponse = body?.Descendants(ns2 + "CreateBreakdownTaskResponse").FirstOrDefault();
                var createResult = createResponse?.Descendants(ns2 + "CreateBreakdownTaskResult").FirstOrDefault();

                // Try multiple possible element names for TaskId
                var taskId = createResult?.Descendants(ns3 + "TaskId").FirstOrDefault()?.Value
                    ?? createResult?.Descendants(ns4 + "TaskId").FirstOrDefault()?.Value
                    ?? createResult?.Descendants("TaskId").FirstOrDefault()?.Value;

                var operationResult = createResult?.Descendants(ns4 + "OperationResult").FirstOrDefault()?.Value
                    ?? createResult?.Descendants(ns3 + "OperationResult").FirstOrDefault()?.Value;

                return new CreateBreakdownTaskResponseDto
                {
                    TaskId = taskId,
                    OperationResult = operationResult
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to parse CreateBreakdownTask response: {ex.Message}");
                return new CreateBreakdownTaskResponseDto();
            }
        }
    }
}
