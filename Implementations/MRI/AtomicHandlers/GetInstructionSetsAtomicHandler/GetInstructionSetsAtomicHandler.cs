using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using FacilitiesMgmtSystem.ConfigModels;
using FacilitiesMgmtSystem.Constants;
using FacilitiesMgmtSystem.Core.Exceptions;
using FacilitiesMgmtSystem.DTO.DownsteamDTOs.GetInstructionSetsApiDTO;
using FacilitiesMgmtSystem.Helper;

namespace FacilitiesMgmtSystem.Implementations.MRI.AtomicHandlers.GetInstructionSetsAtomicHandler;

/// <summary>
/// Atomic handler for Get Instruction Sets SOAP operation.
/// Single responsibility: build SOAP request, call endpoint, deserialize response.
/// </summary>
public class GetInstructionSetsAtomicHandler
{
    private readonly ILogger<GetInstructionSetsAtomicHandler> _logger;
    private readonly CustomSoapClient _soapClient;
    private readonly IOptions<AppConfigs> _appConfigs;

    public GetInstructionSetsAtomicHandler(
        ILogger<GetInstructionSetsAtomicHandler> logger,
        CustomSoapClient soapClient,
        IOptions<AppConfigs> appConfigs)
    {
        _logger = logger;
        _soapClient = soapClient;
        _appConfigs = appConfigs;
    }

    /// <summary>
    /// Executes the Get Instruction Sets SOAP call.
    /// </summary>
    /// <param name="sessionId">MRI session ID.</param>
    /// <param name="request">Instruction sets request data.</param>
    /// <returns>The SOAP response DTO.</returns>
    public async Task<GetInstructionSetsApiResponseDTO> ExecuteAsync(
        string sessionId,
        GetInstructionSetsApiRequestDTO request)
    {
        _logger.Info(InfoConstants.DOWNSTREAM_CALL_STARTED, "GetInstructionSets");

        var config = _appConfigs.Value.MRI;
        
        // Load and populate SOAP envelope
        var envelope = SOAPHelper.LoadSoapEnvelope(InfoConstants.SOAP_ENVELOPE_GET_INSTRUCTION_SETS);
        envelope = PopulateSoapEnvelope(envelope, sessionId, request, config);

        // Make SOAP call
        var url = config.BaseUrl + config.GetInstructionSetsEndpoint;
        var soapAction = config.SoapActionNamespace + InfoConstants.SOAP_ACTION_GET_INSTRUCTION_SETS;
        
        var response = await _soapClient.SendSoapRequestAsync(url, envelope, soapAction);

        // Handle SOAP fault
        if (response.IsSoapFault)
        {
            _logger.Error("GetInstructionSets SOAP fault: {FaultMessage}", response.SoapFaultMessage);
            throw new DownStreamApiFailureException(response.SoapFaultMessage ?? ErrorConstants.SOAP_FAULT_RECEIVED)
            {
                ErrorProperties = [ErrorConstants.GET_INSTRUCTION_SETS_FAILED]
            };
        }

        // Handle HTTP error
        if (!response.IsSuccessStatusCode)
        {
            _logger.Error("GetInstructionSets HTTP error: {StatusCode}", response.StatusCode);
            throw new DownStreamApiFailureException(
                ErrorConstants.GET_INSTRUCTION_SETS_FAILED, 
                response.StatusCode, 
                response.Content);
        }

        // Deserialize response
        var result = DeserializeResponse(response.Content);
        
        _logger.Info(InfoConstants.DOWNSTREAM_CALL_COMPLETED, "GetInstructionSets");
        return result;
    }

    private string PopulateSoapEnvelope(
        string envelope, 
        string sessionId, 
        GetInstructionSetsApiRequestDTO request,
        MRIConfig config)
    {
        var values = new Dictionary<string, string?>
        {
            ["SessionId"] = sessionId,
            ["ContractId"] = request.ContractId ?? config.ContractId,
            ["InstructionSetId"] = request.InstructionSetId ?? string.Empty,
            ["CategoryId"] = request.CategoryId ?? string.Empty,
            ["AssetTypeId"] = request.AssetTypeId ?? string.Empty,
            ["IncludeSteps"] = request.IncludeSteps.ToString().ToLower()
        };

        return XMLHelper.ReplacePlaceholders(envelope, values);
    }

    private GetInstructionSetsApiResponseDTO DeserializeResponse(string? content)
    {
        if (string.IsNullOrEmpty(content))
        {
            throw new DownStreamApiFailureException(ErrorConstants.SOAP_RESPONSE_DESERIALIZATION_FAILED)
            {
                ErrorProperties = [ErrorConstants.GET_INSTRUCTION_SETS_FAILED]
            };
        }

        var result = SOAPHelper.DeserializeSoapResponse<GetInstructionSetsApiResponseDTO>(
            content, 
            "GetInstructionSetsResponse",
            _appConfigs.Value.MRI.ServiceNamespace);

        if (result == null)
        {
            throw new DownStreamApiFailureException(ErrorConstants.SOAP_RESPONSE_DESERIALIZATION_FAILED)
            {
                ErrorProperties = [ErrorConstants.GET_INSTRUCTION_SETS_FAILED]
            };
        }

        return result;
    }
}
