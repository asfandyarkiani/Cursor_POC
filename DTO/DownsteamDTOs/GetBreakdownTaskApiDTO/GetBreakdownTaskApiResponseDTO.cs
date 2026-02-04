using System.Xml.Serialization;

namespace FacilitiesMgmtSystem.DTO.DownsteamDTOs.GetBreakdownTaskApiDTO;

/// <summary>
/// Downstream API response DTO for Get Breakdown Task SOAP operation.
/// </summary>
[XmlRoot("GetBreakdownTaskResponse", Namespace = "http://tempuri.org/")]
public class GetBreakdownTaskApiResponseDTO
{
    [XmlElement("Result")]
    public GetBreakdownTaskResultDTO? Result { get; set; }
}

/// <summary>
/// Result element from Get Breakdown Task SOAP response.
/// </summary>
public class GetBreakdownTaskResultDTO
{
    [XmlElement("Success")]
    public bool Success { get; set; }

    [XmlElement("Message")]
    public string? Message { get; set; }

    [XmlArray("Tasks")]
    [XmlArrayItem("Task")]
    public List<BreakdownTaskApiDTO>? Tasks { get; set; }

    [XmlElement("ErrorCode")]
    public string? ErrorCode { get; set; }

    [XmlElement("ErrorMessage")]
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Breakdown task data from SOAP response.
/// </summary>
public class BreakdownTaskApiDTO
{
    [XmlElement("TaskId")]
    public string? TaskId { get; set; }

    [XmlElement("TaskName")]
    public string? TaskName { get; set; }

    [XmlElement("Description")]
    public string? Description { get; set; }

    [XmlElement("WorkOrderId")]
    public string? WorkOrderId { get; set; }

    [XmlElement("Status")]
    public string? Status { get; set; }

    [XmlElement("Priority")]
    public string? Priority { get; set; }

    [XmlElement("EstimatedHours")]
    public decimal? EstimatedHours { get; set; }

    [XmlElement("ActualHours")]
    public decimal? ActualHours { get; set; }

    [XmlElement("AssignedTo")]
    public string? AssignedTo { get; set; }

    [XmlElement("ScheduledStartDate")]
    public string? ScheduledStartDate { get; set; }

    [XmlElement("ScheduledEndDate")]
    public string? ScheduledEndDate { get; set; }

    [XmlElement("SequenceNumber")]
    public int? SequenceNumber { get; set; }

    [XmlElement("InstructionSetId")]
    public string? InstructionSetId { get; set; }
}
