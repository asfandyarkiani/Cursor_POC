using System.Xml.Serialization;

namespace FacilitiesMgmtSystem.DTO.DownsteamDTOs.GetLocationApiDTO;

/// <summary>
/// Downstream API response DTO for Get Location SOAP operation.
/// </summary>
[XmlRoot("GetLocationResponse", Namespace = "http://tempuri.org/")]
public class GetLocationApiResponseDTO
{
    [XmlElement("Result")]
    public GetLocationResultDTO? Result { get; set; }
}

/// <summary>
/// Result element from Get Location SOAP response.
/// </summary>
public class GetLocationResultDTO
{
    [XmlElement("Success")]
    public bool Success { get; set; }

    [XmlElement("Message")]
    public string? Message { get; set; }

    [XmlArray("Locations")]
    [XmlArrayItem("Location")]
    public List<LocationApiDTO>? Locations { get; set; }

    [XmlElement("ErrorCode")]
    public string? ErrorCode { get; set; }

    [XmlElement("ErrorMessage")]
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Location data from SOAP response.
/// </summary>
public class LocationApiDTO
{
    [XmlElement("LocationId")]
    public string? LocationId { get; set; }

    [XmlElement("LocationCode")]
    public string? LocationCode { get; set; }

    [XmlElement("Name")]
    public string? Name { get; set; }

    [XmlElement("Description")]
    public string? Description { get; set; }

    [XmlElement("LocationType")]
    public string? LocationType { get; set; }

    [XmlElement("BuildingId")]
    public string? BuildingId { get; set; }

    [XmlElement("BuildingName")]
    public string? BuildingName { get; set; }

    [XmlElement("FloorId")]
    public string? FloorId { get; set; }

    [XmlElement("FloorName")]
    public string? FloorName { get; set; }

    [XmlElement("ParentLocationId")]
    public string? ParentLocationId { get; set; }

    [XmlElement("HierarchyPath")]
    public string? HierarchyPath { get; set; }

    [XmlElement("Area")]
    public decimal? Area { get; set; }

    [XmlElement("Capacity")]
    public int? Capacity { get; set; }

    [XmlElement("Status")]
    public string? Status { get; set; }
}
