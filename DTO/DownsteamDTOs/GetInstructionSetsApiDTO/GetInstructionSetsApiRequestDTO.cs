using System.Xml.Serialization;

namespace FacilitiesMgmtSystem.DTO.DownsteamDTOs.GetInstructionSetsApiDTO;

/// <summary>
/// Downstream API request DTO for Get Instruction Sets SOAP operation.
/// </summary>
[XmlRoot("InstructionSetsRequest", Namespace = "http://tempuri.org/")]
public class GetInstructionSetsApiRequestDTO
{
    [XmlElement("ContractId")]
    public string? ContractId { get; set; }

    [XmlElement("InstructionSetId")]
    public string? InstructionSetId { get; set; }

    [XmlElement("CategoryId")]
    public string? CategoryId { get; set; }

    [XmlElement("AssetTypeId")]
    public string? AssetTypeId { get; set; }

    [XmlElement("IncludeSteps")]
    public bool IncludeSteps { get; set; }
}
