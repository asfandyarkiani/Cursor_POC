using System.Xml.Serialization;

namespace FacilitiesMgmtSystem.DTO.DownsteamDTOs.CreateWorkOrderApiDTO;

/// <summary>
/// Downstream API request DTO for Create Work Order SOAP operation.
/// Represents the data to be serialized into the SOAP request body.
/// </summary>
[XmlRoot("WorkOrderRequest", Namespace = "http://tempuri.org/")]
public class CreateWorkOrderApiRequestDTO
{
    [XmlElement("ContractId")]
    public string? ContractId { get; set; }

    [XmlElement("Description")]
    public string? Description { get; set; }

    [XmlElement("Priority")]
    public string? Priority { get; set; }

    [XmlElement("LocationId")]
    public string? LocationId { get; set; }

    [XmlElement("AssetId")]
    public string? AssetId { get; set; }

    [XmlElement("RequestedBy")]
    public string? RequestedBy { get; set; }

    [XmlElement("RequestedDate")]
    public string? RequestedDate { get; set; }

    [XmlElement("DueDate")]
    public string? DueDate { get; set; }

    [XmlElement("WorkOrderType")]
    public string? WorkOrderType { get; set; }

    [XmlElement("CategoryId")]
    public string? CategoryId { get; set; }

    [XmlElement("SubCategoryId")]
    public string? SubCategoryId { get; set; }

    [XmlElement("AssignedTo")]
    public string? AssignedTo { get; set; }

    [XmlElement("Notes")]
    public string? Notes { get; set; }

    [XmlElement("ExternalReference")]
    public string? ExternalReference { get; set; }
}
