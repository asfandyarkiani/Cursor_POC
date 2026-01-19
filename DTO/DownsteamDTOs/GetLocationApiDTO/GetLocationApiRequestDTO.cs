using System.Xml.Serialization;

namespace FacilitiesMgmtSystem.DTO.DownsteamDTOs.GetLocationApiDTO;

/// <summary>
/// Downstream API request DTO for Get Location SOAP operation.
/// </summary>
[XmlRoot("LocationRequest", Namespace = "http://tempuri.org/")]
public class GetLocationApiRequestDTO
{
    [XmlElement("ContractId")]
    public string? ContractId { get; set; }

    [XmlElement("LocationId")]
    public string? LocationId { get; set; }

    [XmlElement("LocationCode")]
    public string? LocationCode { get; set; }

    [XmlElement("BuildingId")]
    public string? BuildingId { get; set; }

    [XmlElement("FloorId")]
    public string? FloorId { get; set; }

    [XmlElement("IncludeHierarchy")]
    public bool IncludeHierarchy { get; set; }
}
