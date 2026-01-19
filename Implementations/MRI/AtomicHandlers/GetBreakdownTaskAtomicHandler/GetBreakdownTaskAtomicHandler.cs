using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using FacilitiesMgmtSystem.ConfigModels;
using FacilitiesMgmtSystem.Constants;
using FacilitiesMgmtSystem.Core.Exceptions;
using FacilitiesMgmtSystem.DTO.DownsteamDTOs.GetBreakdownTaskApiDTO;
using FacilitiesMgmtSystem.Helper;

namespace FacilitiesMgmtSystem.Implementations.MRI.AtomicHandlers.GetBreakdownTaskAtomicHandler;

/// <summary>
/// Atomic handler for Get Breakdown Task SOAP operation.
/// Single responsibility: build SOAP request, call endpoint, deserialize response.
/// </summary>
public class GetBreakdownTaskAtomicHandler
{
    private readonly ILogger<GetBreakdownTaskAtomicHandler> _logger;
    private readonly CustomSoapClient _soapClient;
    private readonly IOptions<AppConfigs> _appConfigs;

    public GetBreakdownTaskAtomicHandler(
        ILogger<GetBreakdownTaskAtomicHandler> logger,
        CustomSoapClient soapClient,
        IOptions<AppConfigs> appConfigs)
    {
        _logger = logger;
        _soapClient = soapClient;
        _appConfigs = appConfigs;
    }

    /// <summary>
    /// Executes the Get Breakdown Task SOAP call.
    /// </summary>
    /// <param name="sessionId">MRI session ID.</param>
    /// <param name="request">Breakdown task request data.</param>
    /// <returns>The SOAP response DTO.</returns>
    public async Task<GetBreakdownTaskApiResponseDTO> ExecuteAsync(
        string sessionId,
        GetBreakdownTaskApiRequestDTO request)
    {
        _logger.Info(InfoConstants.DOWNSTREAM_CALL_STARTED, "GetBreakdownTask");

        var config = _appConfigs.Value.MRI;
        
        // Load and populate SOAP envelope
        var envelope = SOAPHelper.LoadSoapEnvelope(InfoConstants.SOAP_ENVELOPE_GET_BREAKDOWN_TASK);
        envelope = PopulateSoapEnvelope(envelope, sessionId, request, config);

        // Make SOAP call
        var url = config.BaseUrl + config.GetBreakdownTaskEndpoint;
        var soapAction = config.SoapActionNamespace + InfoConstants.SOAP_ACTION_GET_BREAKDOWN_TASK;
        
        var response = await _soapClient.SendSoapRequestAsync(url, envelope, soapAction);

        // Handle SOAP fault
        if (response.IsSoapFault)
        {
            _logger.Error("GetBreakdownTask SOAP fault: {FaultMessage}", response.SoapFaultMessage);
            throw new DownStreamApiFailureException(response.SoapFaultMessage ?? ErrorConstants.SOAP_FAULT_RECEIVED)
            {
                ErrorProperties = [ErrorConstants.GET_BREAKDOWN_TASK_FAILED]
            };
        }

        // Handle HTTP error
        if (!response.IsSuccessStatusCode)
        {
            _logger.Error("GetBreakdownTask HTTP error: {StatusCode}", response.StatusCode);
            throw new DownStreamApiFailureException(
                ErrorConstants.GET_BREAKDOWN_TASK_FAILED, 
                response.StatusCode, 
                response.Content);
        }

        // Deserialize response
        var result = DeserializeResponse(response.Content);
        
        _logger.Info(InfoConstants.DOWNSTREAM_CALL_COMPLETED, "GetBreakdownTask");
        return result;
    }

    private string PopulateSoapEnvelope(
        string envelope, 
        string sessionId, 
        GetBreakdownTaskApiRequestDTO request,
        MRIConfig config)
    {
        var values = new Dictionary<string, string?>
        {
            ["SessionId"] = sessionId,
            ["ContractId"] = request.ContractId ?? config.ContractId,
            ["TaskId"] = request.TaskId ?? string.Empty,
            ["WorkOrderId"] = request.WorkOrderId ?? string.Empty,
            ["IncludeDetails"] = request.IncludeDetails.ToString().ToLower()
        };

        return XMLHelper.ReplacePlaceholders(envelope, values);
    }

    private GetBreakdownTaskApiResponseDTO DeserializeResponse(string? content)
    {
        if (string.IsNullOrEmpty(content))
        {
            throw new DownStreamApiFailureException(ErrorConstants.SOAP_RESPONSE_DESERIALIZATION_FAILED)
            {
                ErrorProperties = [ErrorConstants.GET_BREAKDOWN_TASK_FAILED]
            };
        }

        var result = SOAPHelper.DeserializeSoapResponse<GetBreakdownTaskApiResponseDTO>(
            content, 
            "GetBreakdownTaskResponse",
            _appConfigs.Value.MRI.ServiceNamespace);

        if (result == null)
        {
            throw new DownStreamApiFailureException(ErrorConstants.SOAP_RESPONSE_DESERIALIZATION_FAILED)
            {
                ErrorProperties = [ErrorConstants.GET_BREAKDOWN_TASK_FAILED]
            };
        }

        return result;
    }
}
