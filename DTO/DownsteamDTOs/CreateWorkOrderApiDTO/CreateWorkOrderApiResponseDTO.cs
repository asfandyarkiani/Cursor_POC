using System.Xml.Serialization;

namespace FacilitiesMgmtSystem.DTO.DownsteamDTOs.CreateWorkOrderApiDTO;

/// <summary>
/// Downstream API response DTO for Create Work Order SOAP operation.
/// Represents the data deserialized from the SOAP response body.
/// </summary>
[XmlRoot("CreateWorkOrderResponse", Namespace = "http://tempuri.org/")]
public class CreateWorkOrderApiResponseDTO
{
    [XmlElement("Result")]
    public CreateWorkOrderResultDTO? Result { get; set; }
}

/// <summary>
/// Result element from Create Work Order SOAP response.
/// </summary>
public class CreateWorkOrderResultDTO
{
    [XmlElement("Success")]
    public bool Success { get; set; }

    [XmlElement("Message")]
    public string? Message { get; set; }

    [XmlElement("WorkOrderId")]
    public string? WorkOrderId { get; set; }

    [XmlElement("WorkOrderNumber")]
    public string? WorkOrderNumber { get; set; }

    [XmlElement("Status")]
    public string? Status { get; set; }

    [XmlElement("CreatedDate")]
    public string? CreatedDate { get; set; }

    [XmlElement("CreatedBy")]
    public string? CreatedBy { get; set; }

    [XmlElement("ErrorCode")]
    public string? ErrorCode { get; set; }

    [XmlElement("ErrorMessage")]
    public string? ErrorMessage { get; set; }
}
