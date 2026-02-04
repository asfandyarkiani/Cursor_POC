using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using FacilitiesMgmtSystem.ConfigModels;
using FacilitiesMgmtSystem.Constants;
using FacilitiesMgmtSystem.Core.Exceptions;
using FacilitiesMgmtSystem.DTO.DownsteamDTOs.GetLocationApiDTO;
using FacilitiesMgmtSystem.Helper;

namespace FacilitiesMgmtSystem.Implementations.MRI.AtomicHandlers.GetLocationAtomicHandler;

/// <summary>
/// Atomic handler for Get Location SOAP operation.
/// Single responsibility: build SOAP request, call endpoint, deserialize response.
/// </summary>
public class GetLocationAtomicHandler
{
    private readonly ILogger<GetLocationAtomicHandler> _logger;
    private readonly CustomSoapClient _soapClient;
    private readonly IOptions<AppConfigs> _appConfigs;

    public GetLocationAtomicHandler(
        ILogger<GetLocationAtomicHandler> logger,
        CustomSoapClient soapClient,
        IOptions<AppConfigs> appConfigs)
    {
        _logger = logger;
        _soapClient = soapClient;
        _appConfigs = appConfigs;
    }

    /// <summary>
    /// Executes the Get Location SOAP call.
    /// </summary>
    /// <param name="sessionId">MRI session ID.</param>
    /// <param name="request">Location request data.</param>
    /// <returns>The SOAP response DTO.</returns>
    public async Task<GetLocationApiResponseDTO> ExecuteAsync(
        string sessionId,
        GetLocationApiRequestDTO request)
    {
        _logger.Info(InfoConstants.DOWNSTREAM_CALL_STARTED, "GetLocation");

        var config = _appConfigs.Value.MRI;
        
        // Load and populate SOAP envelope
        var envelope = SOAPHelper.LoadSoapEnvelope(InfoConstants.SOAP_ENVELOPE_GET_LOCATION);
        envelope = PopulateSoapEnvelope(envelope, sessionId, request, config);

        // Make SOAP call
        var url = config.BaseUrl + config.GetLocationEndpoint;
        var soapAction = config.SoapActionNamespace + InfoConstants.SOAP_ACTION_GET_LOCATION;
        
        var response = await _soapClient.SendSoapRequestAsync(url, envelope, soapAction);

        // Handle SOAP fault
        if (response.IsSoapFault)
        {
            _logger.Error("GetLocation SOAP fault: {FaultMessage}", response.SoapFaultMessage);
            throw new DownStreamApiFailureException(response.SoapFaultMessage ?? ErrorConstants.SOAP_FAULT_RECEIVED)
            {
                ErrorProperties = [ErrorConstants.GET_LOCATION_FAILED]
            };
        }

        // Handle HTTP error
        if (!response.IsSuccessStatusCode)
        {
            _logger.Error("GetLocation HTTP error: {StatusCode}", response.StatusCode);
            throw new DownStreamApiFailureException(
                ErrorConstants.GET_LOCATION_FAILED, 
                response.StatusCode, 
                response.Content);
        }

        // Deserialize response
        var result = DeserializeResponse(response.Content);
        
        _logger.Info(InfoConstants.DOWNSTREAM_CALL_COMPLETED, "GetLocation");
        return result;
    }

    private string PopulateSoapEnvelope(
        string envelope, 
        string sessionId, 
        GetLocationApiRequestDTO request,
        MRIConfig config)
    {
        var values = new Dictionary<string, string?>
        {
            ["SessionId"] = sessionId,
            ["ContractId"] = request.ContractId ?? config.ContractId,
            ["LocationId"] = request.LocationId ?? string.Empty,
            ["LocationCode"] = request.LocationCode ?? string.Empty,
            ["BuildingId"] = request.BuildingId ?? string.Empty,
            ["FloorId"] = request.FloorId ?? string.Empty,
            ["IncludeHierarchy"] = request.IncludeHierarchy.ToString().ToLower()
        };

        return XMLHelper.ReplacePlaceholders(envelope, values);
    }

    private GetLocationApiResponseDTO DeserializeResponse(string? content)
    {
        if (string.IsNullOrEmpty(content))
        {
            throw new DownStreamApiFailureException(ErrorConstants.SOAP_RESPONSE_DESERIALIZATION_FAILED)
            {
                ErrorProperties = [ErrorConstants.GET_LOCATION_FAILED]
            };
        }

        var result = SOAPHelper.DeserializeSoapResponse<GetLocationApiResponseDTO>(
            content, 
            "GetLocationResponse",
            _appConfigs.Value.MRI.ServiceNamespace);

        if (result == null)
        {
            throw new DownStreamApiFailureException(ErrorConstants.SOAP_RESPONSE_DESERIALIZATION_FAILED)
            {
                ErrorProperties = [ErrorConstants.GET_LOCATION_FAILED]
            };
        }

        return result;
    }
}
