using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Xml.Linq;
using AGI.SystemLayer.CAFM.ConfigModels;
using AGI.SystemLayer.CAFM.DTOs.Downstream;
using Core.Middlewares;
using Core.SystemLayer.Handlers;

namespace AGI.SystemLayer.CAFM.Implementations.FSI.AtomicHandlers;

/// <summary>
/// Atomic Handler for GetInstructionSetsByDto SOAP operation.
/// Retrieves instruction set information from CAFM.
/// </summary>
public class GetInstructionSetsByDtoAtomicHandler : IAtomicHandler
{
    private readonly CustomHTTPClient _httpClient;
    private readonly AppConfigs _config;
    private readonly ILogger<GetInstructionSetsByDtoAtomicHandler> _logger;

    public GetInstructionSetsByDtoAtomicHandler(
        CustomHTTPClient httpClient,
        IOptions<AppConfigs> config,
        ILogger<GetInstructionSetsByDtoAtomicHandler> logger)
    {
        _httpClient = httpClient;
        _config = config.Value;
        _logger = logger;
    }

    public async Task<GetInstructionSetsByDtoResponseDTO> GetInstructionSetsAsync(
        GetInstructionSetsByDtoRequestDTO request,
        CancellationToken cancellationToken)
    {
        try
        {
            var soapRequest = BuildGetInstructionSetsSoapRequest(request.SessionId, request.InstructionSetCode);
            var url = $"{_config.CAFMSettings.BaseUrl}{_config.CAFMSettings.GetInstructionSetsResourcePath}";

            _logger.LogInformation("CAFM GetInstructionSets: Sending request to {Url}", url);

            var response = await _httpClient.PostAsync(
                url,
                new StringContent(soapRequest, System.Text.Encoding.UTF8, "text/xml"),
                new Dictionary<string, string>
                {
                    { "SOAPAction", _config.CAFMSettings.GetInstructionSetsSoapAction }
                },
                cancellationToken);

            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var instructionSets = ParseInstructionSetsFromResponse(responseContent);

            _logger.LogInformation("CAFM GetInstructionSets: Retrieved {Count} instruction sets", instructionSets.Count);

            return new GetInstructionSetsByDtoResponseDTO
            {
                InstructionSets = instructionSets
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CAFM GetInstructionSets: Exception during GetInstructionSetsByDto");
            throw;
        }
    }

    private string BuildGetInstructionSetsSoapRequest(string sessionId, string? instructionSetCode)
    {
        var codeElement = string.IsNullOrEmpty(instructionSetCode)
            ? ""
            : $"<fsi1:InstructionSetCode>{System.Security.SecurityElement.Escape(instructionSetCode)}</fsi1:InstructionSetCode>";

        return $@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/""
                  xmlns:ns=""http://www.fsi.co.uk/services/evolution/04/09""
                  xmlns:fsi1=""http://schemas.datacontract.org/2004/07/Fsi.Concept.Contracts.Entities.ServiceModel"">
    <soapenv:Header/>
    <soapenv:Body>
        <ns:GetInstructionSetsByDto>
            <ns:sessionId>{System.Security.SecurityElement.Escape(sessionId)}</ns:sessionId>
            <ns:instructionSetDto>
                {codeElement}
            </ns:instructionSetDto>
        </ns:GetInstructionSetsByDto>
    </soapenv:Body>
</soapenv:Envelope>";
    }

    private List<InstructionSetData> ParseInstructionSetsFromResponse(string soapResponse)
    {
        var instructionSets = new List<InstructionSetData>();

        try
        {
            var doc = XDocument.Parse(soapResponse);
            XNamespace fsi1 = "http://schemas.datacontract.org/2004/07/Fsi.Concept.Contracts.Entities.ServiceModel";

            var instructionSetElements = doc.Descendants(fsi1 + "InstructionSetDto");

            foreach (var element in instructionSetElements)
            {
                instructionSets.Add(new InstructionSetData
                {
                    InstructionSetId = element.Element(fsi1 + "InstructionSetId")?.Value,
                    InstructionSetCode = element.Element(fsi1 + "InstructionSetCode")?.Value,
                    InstructionSetName = element.Element(fsi1 + "InstructionSetName")?.Value,
                    Description = element.Element(fsi1 + "Description")?.Value
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CAFM GetInstructionSets: Error parsing instruction sets from SOAP response");
        }

        return instructionSets;
    }
}
