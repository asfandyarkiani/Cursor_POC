using System.Xml.Serialization;

namespace FacilitiesMgmtSystem.DTO.DownsteamDTOs.GetBreakdownTaskApiDTO;

/// <summary>
/// Downstream API request DTO for Get Breakdown Task SOAP operation.
/// </summary>
[XmlRoot("BreakdownTaskRequest", Namespace = "http://tempuri.org/")]
public class GetBreakdownTaskApiRequestDTO
{
    [XmlElement("ContractId")]
    public string? ContractId { get; set; }

    [XmlElement("TaskId")]
    public string? TaskId { get; set; }

    [XmlElement("WorkOrderId")]
    public string? WorkOrderId { get; set; }

    [XmlElement("IncludeDetails")]
    public bool IncludeDetails { get; set; }
}
