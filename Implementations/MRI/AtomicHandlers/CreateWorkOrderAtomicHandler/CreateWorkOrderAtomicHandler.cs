using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using FacilitiesMgmtSystem.ConfigModels;
using FacilitiesMgmtSystem.Constants;
using FacilitiesMgmtSystem.Core.Exceptions;
using FacilitiesMgmtSystem.DTO.DownsteamDTOs.CreateWorkOrderApiDTO;
using FacilitiesMgmtSystem.Helper;

namespace FacilitiesMgmtSystem.Implementations.MRI.AtomicHandlers.CreateWorkOrderAtomicHandler;

/// <summary>
/// Atomic handler for Create Work Order SOAP operation.
/// Single responsibility: build SOAP request, call endpoint, deserialize response.
/// </summary>
public class CreateWorkOrderAtomicHandler
{
    private readonly ILogger<CreateWorkOrderAtomicHandler> _logger;
    private readonly CustomSoapClient _soapClient;
    private readonly IOptions<AppConfigs> _appConfigs;

    public CreateWorkOrderAtomicHandler(
        ILogger<CreateWorkOrderAtomicHandler> logger,
        CustomSoapClient soapClient,
        IOptions<AppConfigs> appConfigs)
    {
        _logger = logger;
        _soapClient = soapClient;
        _appConfigs = appConfigs;
    }

    /// <summary>
    /// Executes the Create Work Order SOAP call.
    /// </summary>
    /// <param name="sessionId">MRI session ID.</param>
    /// <param name="request">Work order request data.</param>
    /// <returns>The SOAP response DTO.</returns>
    public async Task<CreateWorkOrderApiResponseDTO> ExecuteAsync(
        string sessionId,
        CreateWorkOrderApiRequestDTO request)
    {
        _logger.Info(InfoConstants.DOWNSTREAM_CALL_STARTED, "CreateWorkOrder");

        var config = _appConfigs.Value.MRI;
        
        // Load and populate SOAP envelope
        var envelope = SOAPHelper.LoadSoapEnvelope(InfoConstants.SOAP_ENVELOPE_CREATE_WORKORDER);
        envelope = PopulateSoapEnvelope(envelope, sessionId, request, config);

        // Make SOAP call
        var url = config.BaseUrl + config.CreateWorkOrderEndpoint;
        var soapAction = config.SoapActionNamespace + InfoConstants.SOAP_ACTION_CREATE_WORKORDER;
        
        var response = await _soapClient.SendSoapRequestAsync(url, envelope, soapAction);

        // Handle SOAP fault
        if (response.IsSoapFault)
        {
            _logger.Error("CreateWorkOrder SOAP fault: {FaultMessage}", response.SoapFaultMessage);
            throw new DownStreamApiFailureException(response.SoapFaultMessage ?? ErrorConstants.SOAP_FAULT_RECEIVED)
            {
                ErrorProperties = [ErrorConstants.CREATE_WORKORDER_FAILED]
            };
        }

        // Handle HTTP error
        if (!response.IsSuccessStatusCode)
        {
            _logger.Error("CreateWorkOrder HTTP error: {StatusCode}", response.StatusCode);
            throw new DownStreamApiFailureException(
                ErrorConstants.CREATE_WORKORDER_FAILED, 
                response.StatusCode, 
                response.Content);
        }

        // Deserialize response
        var result = DeserializeResponse(response.Content);
        
        _logger.Info(InfoConstants.DOWNSTREAM_CALL_COMPLETED, "CreateWorkOrder");
        return result;
    }

    private string PopulateSoapEnvelope(
        string envelope, 
        string sessionId, 
        CreateWorkOrderApiRequestDTO request,
        MRIConfig config)
    {
        var values = new Dictionary<string, string?>
        {
            ["SessionId"] = sessionId,
            ["ContractId"] = request.ContractId ?? config.ContractId,
            ["Description"] = request.Description,
            ["Priority"] = request.Priority ?? "Medium",
            ["LocationId"] = request.LocationId,
            ["AssetId"] = request.AssetId ?? string.Empty,
            ["RequestedBy"] = request.RequestedBy ?? string.Empty,
            ["RequestedDate"] = request.RequestedDate ?? DateTime.UtcNow.ToString("o"),
            ["DueDate"] = request.DueDate ?? string.Empty,
            ["WorkOrderType"] = request.WorkOrderType ?? "Corrective",
            ["CategoryId"] = request.CategoryId ?? string.Empty,
            ["SubCategoryId"] = request.SubCategoryId ?? string.Empty,
            ["AssignedTo"] = request.AssignedTo ?? string.Empty,
            ["Notes"] = request.Notes ?? string.Empty,
            ["ExternalReference"] = request.ExternalReference ?? string.Empty
        };

        return XMLHelper.ReplacePlaceholders(envelope, values);
    }

    private CreateWorkOrderApiResponseDTO DeserializeResponse(string? content)
    {
        if (string.IsNullOrEmpty(content))
        {
            throw new DownStreamApiFailureException(ErrorConstants.SOAP_RESPONSE_DESERIALIZATION_FAILED)
            {
                ErrorProperties = [ErrorConstants.CREATE_WORKORDER_FAILED]
            };
        }

        var result = SOAPHelper.DeserializeSoapResponse<CreateWorkOrderApiResponseDTO>(
            content, 
            "CreateWorkOrderResponse",
            _appConfigs.Value.MRI.ServiceNamespace);

        if (result == null)
        {
            throw new DownStreamApiFailureException(ErrorConstants.SOAP_RESPONSE_DESERIALIZATION_FAILED)
            {
                ErrorProperties = [ErrorConstants.CREATE_WORKORDER_FAILED]
            };
        }

        return result;
    }
}
