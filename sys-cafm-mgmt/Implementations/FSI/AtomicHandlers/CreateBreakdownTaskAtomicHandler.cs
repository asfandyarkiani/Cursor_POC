using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Xml.Linq;
using AGI.SystemLayer.CAFM.ConfigModels;
using AGI.SystemLayer.CAFM.DTOs.Downstream;
using Core.Middlewares;
using Core.SystemLayer.Handlers;

namespace AGI.SystemLayer.CAFM.Implementations.FSI.AtomicHandlers;

/// <summary>
/// Atomic Handler for CreateBreakdownTask SOAP operation.
/// Creates a new breakdown task (work order) in CAFM.
/// </summary>
public class CreateBreakdownTaskAtomicHandler : IAtomicHandler
{
    private readonly CustomHTTPClient _httpClient;
    private readonly AppConfigs _config;
    private readonly ILogger<CreateBreakdownTaskAtomicHandler> _logger;

    public CreateBreakdownTaskAtomicHandler(
        CustomHTTPClient httpClient,
        IOptions<AppConfigs> config,
        ILogger<CreateBreakdownTaskAtomicHandler> logger)
    {
        _httpClient = httpClient;
        _config = config.Value;
        _logger = logger;
    }

    public async Task<CreateBreakdownTaskResponseDTO> CreateBreakdownTaskAsync(
        CreateBreakdownTaskRequestDTO request,
        CancellationToken cancellationToken)
    {
        try
        {
            var soapRequest = BuildCreateBreakdownTaskSoapRequest(request);
            var url = $"{_config.CAFMSettings.BaseUrl}{_config.CAFMSettings.CreateBreakdownTaskResourcePath}";

            _logger.LogInformation("CAFM CreateBreakdownTask: Sending request to {Url}", url);

            var response = await _httpClient.PostAsync(
                url,
                new StringContent(soapRequest, System.Text.Encoding.UTF8, "text/xml"),
                new Dictionary<string, string>
                {
                    { "SOAPAction", _config.CAFMSettings.CreateBreakdownTaskSoapAction }
                },
                cancellationToken);

            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var taskId = ParseTaskIdFromResponse(responseContent);

            if (string.IsNullOrEmpty(taskId))
            {
                _logger.LogError("CAFM CreateBreakdownTask: Failed to parse TaskId from response");
                return new CreateBreakdownTaskResponseDTO
                {
                    IsSuccess = false,
                    ErrorMessage = "Failed to parse TaskId from CreateBreakdownTask response"
                };
            }

            _logger.LogInformation("CAFM CreateBreakdownTask: Task created successfully. TaskId: {TaskId}", taskId);

            return new CreateBreakdownTaskResponseDTO
            {
                IsSuccess = true,
                TaskId = taskId
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CAFM CreateBreakdownTask: Exception during CreateBreakdownTask");
            return new CreateBreakdownTaskResponseDTO
            {
                IsSuccess = false,
                ErrorMessage = $"Exception during CreateBreakdownTask: {ex.Message}"
            };
        }
    }

    private string BuildCreateBreakdownTaskSoapRequest(CreateBreakdownTaskRequestDTO request)
    {
        var details = request.TaskDetails;

        return $@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/""
                  xmlns:ns=""http://www.fsi.co.uk/services/evolution/04/09""
                  xmlns:fsi1=""http://schemas.datacontract.org/2004/07/Fsi.Concept.Contracts.Entities.ServiceModel"">
    <soapenv:Header/>
    <soapenv:Body>
        <ns:CreateBreakdownTask>
            <ns:sessionId>{System.Security.SecurityElement.Escape(request.SessionId)}</ns:sessionId>
            <ns:breakdownTaskDto>
                <fsi1:LocationId>{System.Security.SecurityElement.Escape(details.LocationId ?? "")}</fsi1:LocationId>
                <fsi1:Description>{System.Security.SecurityElement.Escape(details.Description ?? "")}</fsi1:Description>
                <fsi1:Priority>{System.Security.SecurityElement.Escape(details.Priority ?? "")}</fsi1:Priority>
                <fsi1:Category>{System.Security.SecurityElement.Escape(details.Category ?? "")}</fsi1:Category>
                <fsi1:SubCategory>{System.Security.SecurityElement.Escape(details.SubCategory ?? "")}</fsi1:SubCategory>
                <fsi1:RequestedBy>{System.Security.SecurityElement.Escape(details.RequestedBy ?? "")}</fsi1:RequestedBy>
                <fsi1:ContactPhone>{System.Security.SecurityElement.Escape(details.ContactPhone ?? "")}</fsi1:ContactPhone>
                <fsi1:Notes>{System.Security.SecurityElement.Escape(details.Notes ?? "")}</fsi1:Notes>
                <fsi1:InstructionSetId>{System.Security.SecurityElement.Escape(details.InstructionSetId ?? "")}</fsi1:InstructionSetId>
            </ns:breakdownTaskDto>
        </ns:CreateBreakdownTask>
    </soapenv:Body>
</soapenv:Envelope>";
    }

    private string? ParseTaskIdFromResponse(string soapResponse)
    {
        try
        {
            var doc = XDocument.Parse(soapResponse);
            XNamespace ns = "http://www.fsi.co.uk/services/evolution/04/09";

            var taskIdElement = doc.Descendants(ns + "TaskId").FirstOrDefault();
            return taskIdElement?.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CAFM CreateBreakdownTask: Error parsing TaskId from SOAP response");
            return null;
        }
    }
}
