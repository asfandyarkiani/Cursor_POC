using System.Xml.Serialization;

namespace FacilitiesMgmtSystem.DTO.DownsteamDTOs.GetInstructionSetsApiDTO;

/// <summary>
/// Downstream API response DTO for Get Instruction Sets SOAP operation.
/// </summary>
[XmlRoot("GetInstructionSetsResponse", Namespace = "http://tempuri.org/")]
public class GetInstructionSetsApiResponseDTO
{
    [XmlElement("Result")]
    public GetInstructionSetsResultDTO? Result { get; set; }
}

/// <summary>
/// Result element from Get Instruction Sets SOAP response.
/// </summary>
public class GetInstructionSetsResultDTO
{
    [XmlElement("Success")]
    public bool Success { get; set; }

    [XmlElement("Message")]
    public string? Message { get; set; }

    [XmlArray("InstructionSets")]
    [XmlArrayItem("InstructionSet")]
    public List<InstructionSetApiDTO>? InstructionSets { get; set; }

    [XmlElement("ErrorCode")]
    public string? ErrorCode { get; set; }

    [XmlElement("ErrorMessage")]
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Instruction set data from SOAP response.
/// </summary>
public class InstructionSetApiDTO
{
    [XmlElement("InstructionSetId")]
    public string? InstructionSetId { get; set; }

    [XmlElement("Name")]
    public string? Name { get; set; }

    [XmlElement("Description")]
    public string? Description { get; set; }

    [XmlElement("CategoryId")]
    public string? CategoryId { get; set; }

    [XmlElement("CategoryName")]
    public string? CategoryName { get; set; }

    [XmlElement("AssetTypeId")]
    public string? AssetTypeId { get; set; }

    [XmlElement("AssetTypeName")]
    public string? AssetTypeName { get; set; }

    [XmlElement("Version")]
    public string? Version { get; set; }

    [XmlElement("Status")]
    public string? Status { get; set; }

    [XmlElement("EstimatedDurationMinutes")]
    public int? EstimatedDurationMinutes { get; set; }

    [XmlElement("RequiredSkillLevel")]
    public string? RequiredSkillLevel { get; set; }

    [XmlArray("Steps")]
    [XmlArrayItem("Step")]
    public List<InstructionStepApiDTO>? Steps { get; set; }
}

/// <summary>
/// Instruction step data from SOAP response.
/// </summary>
public class InstructionStepApiDTO
{
    [XmlElement("StepNumber")]
    public int StepNumber { get; set; }

    [XmlElement("Title")]
    public string? Title { get; set; }

    [XmlElement("Description")]
    public string? Description { get; set; }

    [XmlElement("EstimatedMinutes")]
    public int? EstimatedMinutes { get; set; }

    [XmlElement("SafetyNotes")]
    public string? SafetyNotes { get; set; }

    [XmlArray("RequiredTools")]
    [XmlArrayItem("Tool")]
    public List<string>? RequiredTools { get; set; }

    [XmlElement("IsMandatory")]
    public bool IsMandatory { get; set; }
}
