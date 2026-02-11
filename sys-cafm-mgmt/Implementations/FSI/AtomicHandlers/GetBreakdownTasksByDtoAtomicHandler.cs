using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Xml.Linq;
using AGI.SystemLayer.CAFM.ConfigModels;
using AGI.SystemLayer.CAFM.DTOs.Downstream;
using Core.Middlewares;
using Core.SystemLayer.Handlers;

namespace AGI.SystemLayer.CAFM.Implementations.FSI.AtomicHandlers;

/// <summary>
/// Atomic Handler for GetBreakdownTasksByDto SOAP operation.
/// Retrieves breakdown task information from CAFM.
/// </summary>
public class GetBreakdownTasksByDtoAtomicHandler : IAtomicHandler
{
    private readonly CustomHTTPClient _httpClient;
    private readonly AppConfigs _config;
    private readonly ILogger<GetBreakdownTasksByDtoAtomicHandler> _logger;

    public GetBreakdownTasksByDtoAtomicHandler(
        CustomHTTPClient httpClient,
        IOptions<AppConfigs> config,
        ILogger<GetBreakdownTasksByDtoAtomicHandler> logger)
    {
        _httpClient = httpClient;
        _config = config.Value;
        _logger = logger;
    }

    public async Task<GetBreakdownTasksByDtoResponseDTO> GetBreakdownTasksAsync(
        GetBreakdownTasksByDtoRequestDTO request,
        CancellationToken cancellationToken)
    {
        try
        {
            var soapRequest = BuildGetBreakdownTasksSoapRequest(request.SessionId, request.TaskId, request.TaskCode);
            var url = $"{_config.CAFMSettings.BaseUrl}{_config.CAFMSettings.GetBreakdownTasksResourcePath}";

            _logger.LogInformation("CAFM GetBreakdownTasks: Sending request to {Url}", url);

            var response = await _httpClient.PostAsync(
                url,
                new StringContent(soapRequest, System.Text.Encoding.UTF8, "text/xml"),
                new Dictionary<string, string>
                {
                    { "SOAPAction", _config.CAFMSettings.GetBreakdownTasksSoapAction }
                },
                cancellationToken);

            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var tasks = ParseBreakdownTasksFromResponse(responseContent);

            _logger.LogInformation("CAFM GetBreakdownTasks: Retrieved {Count} breakdown tasks", tasks.Count);

            return new GetBreakdownTasksByDtoResponseDTO
            {
                BreakdownTasks = tasks
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CAFM GetBreakdownTasks: Exception during GetBreakdownTasksByDto");
            throw;
        }
    }

    private string BuildGetBreakdownTasksSoapRequest(string sessionId, string? taskId, string? taskCode)
    {
        var taskIdElement = string.IsNullOrEmpty(taskId)
            ? ""
            : $"<fsi1:TaskId>{System.Security.SecurityElement.Escape(taskId)}</fsi1:TaskId>";

        var taskCodeElement = string.IsNullOrEmpty(taskCode)
            ? ""
            : $"<fsi1:TaskCode>{System.Security.SecurityElement.Escape(taskCode)}</fsi1:TaskCode>";

        return $@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/""
                  xmlns:ns=""http://www.fsi.co.uk/services/evolution/04/09""
                  xmlns:fsi1=""http://schemas.datacontract.org/2004/07/Fsi.Concept.Contracts.Entities.ServiceModel"">
    <soapenv:Header/>
    <soapenv:Body>
        <ns:GetBreakdownTasksByDto>
            <ns:sessionId>{System.Security.SecurityElement.Escape(sessionId)}</ns:sessionId>
            <ns:breakdownTaskDto>
                {taskIdElement}
                {taskCodeElement}
            </ns:breakdownTaskDto>
        </ns:GetBreakdownTasksByDto>
    </soapenv:Body>
</soapenv:Envelope>";
    }

    private List<BreakdownTaskData> ParseBreakdownTasksFromResponse(string soapResponse)
    {
        var tasks = new List<BreakdownTaskData>();

        try
        {
            var doc = XDocument.Parse(soapResponse);
            XNamespace fsi1 = "http://schemas.datacontract.org/2004/07/Fsi.Concept.Contracts.Entities.ServiceModel";

            var taskElements = doc.Descendants(fsi1 + "BreakdownTaskDto");

            foreach (var element in taskElements)
            {
                tasks.Add(new BreakdownTaskData
                {
                    TaskId = element.Element(fsi1 + "TaskId")?.Value,
                    TaskCode = element.Element(fsi1 + "TaskCode")?.Value,
                    TaskName = element.Element(fsi1 + "TaskName")?.Value,
                    Description = element.Element(fsi1 + "Description")?.Value,
                    Status = element.Element(fsi1 + "Status")?.Value,
                    Priority = element.Element(fsi1 + "Priority")?.Value
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CAFM GetBreakdownTasks: Error parsing breakdown tasks from SOAP response");
        }

        return tasks;
    }
}
