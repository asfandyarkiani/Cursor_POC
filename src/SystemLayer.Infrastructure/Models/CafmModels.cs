using System.Xml.Serialization;

namespace SystemLayer.Infrastructure.Models;

/// <summary>
/// CAFM SOAP request/response models
/// </summary>

[XmlRoot("LoginRequest", Namespace = "http://cafm.mri.com/services")]
public class CafmLoginRequest
{
    [XmlElement("Username")]
    public string Username { get; set; } = string.Empty;
    
    [XmlElement("Password")]
    public string Password { get; set; } = string.Empty;
    
    [XmlElement("Database")]
    public string Database { get; set; } = string.Empty;
}

[XmlRoot("LoginResponse", Namespace = "http://cafm.mri.com/services")]
public class CafmLoginResponse
{
    [XmlElement("SessionToken")]
    public string SessionToken { get; set; } = string.Empty;
    
    [XmlElement("Success")]
    public bool Success { get; set; }
    
    [XmlElement("Message")]
    public string? Message { get; set; }
}

[XmlRoot("LogoutRequest", Namespace = "http://cafm.mri.com/services")]
public class CafmLogoutRequest
{
    [XmlElement("SessionToken")]
    public string SessionToken { get; set; } = string.Empty;
}

[XmlRoot("LogoutResponse", Namespace = "http://cafm.mri.com/services")]
public class CafmLogoutResponse
{
    [XmlElement("Success")]
    public bool Success { get; set; }
    
    [XmlElement("Message")]
    public string? Message { get; set; }
}

[XmlRoot("CreateWorkOrderRequest", Namespace = "http://cafm.mri.com/services")]
public class CafmCreateWorkOrderRequest
{
    [XmlElement("SessionToken")]
    public string SessionToken { get; set; } = string.Empty;
    
    [XmlElement("WorkOrderNumber")]
    public string WorkOrderNumber { get; set; } = string.Empty;
    
    [XmlElement("Description")]
    public string Description { get; set; } = string.Empty;
    
    [XmlElement("LocationId")]
    public string LocationId { get; set; } = string.Empty;
    
    [XmlElement("Priority")]
    public string? Priority { get; set; }
    
    [XmlElement("AssignedTo")]
    public string? AssignedTo { get; set; }
    
    [XmlElement("ScheduledDate")]
    public DateTime? ScheduledDate { get; set; }
    
    [XmlElement("InstructionSetId")]
    public string? InstructionSetId { get; set; }
    
    [XmlArray("CustomFields")]
    [XmlArrayItem("Field")]
    public List<CafmCustomField> CustomFields { get; set; } = new();
}

[XmlRoot("CreateWorkOrderResponse", Namespace = "http://cafm.mri.com/services")]
public class CafmCreateWorkOrderResponse
{
    [XmlElement("Success")]
    public bool Success { get; set; }
    
    [XmlElement("WorkOrderId")]
    public string? WorkOrderId { get; set; }
    
    [XmlElement("WorkOrderNumber")]
    public string? WorkOrderNumber { get; set; }
    
    [XmlElement("Message")]
    public string? Message { get; set; }
    
    [XmlElement("ErrorCode")]
    public string? ErrorCode { get; set; }
}

[XmlRoot("GetLocationRequest", Namespace = "http://cafm.mri.com/services")]
public class CafmGetLocationRequest
{
    [XmlElement("SessionToken")]
    public string SessionToken { get; set; } = string.Empty;
    
    [XmlElement("LocationId")]
    public string LocationId { get; set; } = string.Empty;
    
    [XmlElement("IncludeHierarchy")]
    public bool IncludeHierarchy { get; set; }
}

[XmlRoot("GetLocationResponse", Namespace = "http://cafm.mri.com/services")]
public class CafmGetLocationResponse
{
    [XmlElement("Success")]
    public bool Success { get; set; }
    
    [XmlElement("Location")]
    public CafmLocation? Location { get; set; }
    
    [XmlElement("Message")]
    public string? Message { get; set; }
    
    [XmlElement("ErrorCode")]
    public string? ErrorCode { get; set; }
}

[XmlRoot("GetBreakdownTaskRequest", Namespace = "http://cafm.mri.com/services")]
public class CafmGetBreakdownTaskRequest
{
    [XmlElement("SessionToken")]
    public string SessionToken { get; set; } = string.Empty;
    
    [XmlElement("TaskId")]
    public string TaskId { get; set; } = string.Empty;
    
    [XmlElement("IncludeDetails")]
    public bool IncludeDetails { get; set; }
}

[XmlRoot("GetBreakdownTaskResponse", Namespace = "http://cafm.mri.com/services")]
public class CafmGetBreakdownTaskResponse
{
    [XmlElement("Success")]
    public bool Success { get; set; }
    
    [XmlElement("Task")]
    public CafmBreakdownTask? Task { get; set; }
    
    [XmlElement("Message")]
    public string? Message { get; set; }
    
    [XmlElement("ErrorCode")]
    public string? ErrorCode { get; set; }
}

[XmlRoot("GetInstructionSetsRequest", Namespace = "http://cafm.mri.com/services")]
public class CafmGetInstructionSetsRequest
{
    [XmlElement("SessionToken")]
    public string SessionToken { get; set; } = string.Empty;
    
    [XmlElement("CategoryFilter")]
    public string? CategoryFilter { get; set; }
    
    [XmlElement("AssetTypeFilter")]
    public string? AssetTypeFilter { get; set; }
    
    [XmlElement("MaxResults")]
    public int MaxResults { get; set; } = 100;
}

[XmlRoot("GetInstructionSetsResponse", Namespace = "http://cafm.mri.com/services")]
public class CafmGetInstructionSetsResponse
{
    [XmlElement("Success")]
    public bool Success { get; set; }
    
    [XmlArray("InstructionSets")]
    [XmlArrayItem("InstructionSet")]
    public List<CafmInstructionSet> InstructionSets { get; set; } = new();
    
    [XmlElement("TotalCount")]
    public int TotalCount { get; set; }
    
    [XmlElement("HasMore")]
    public bool HasMore { get; set; }
    
    [XmlElement("Message")]
    public string? Message { get; set; }
    
    [XmlElement("ErrorCode")]
    public string? ErrorCode { get; set; }
}

// Supporting models
public class CafmCustomField
{
    [XmlElement("Name")]
    public string Name { get; set; } = string.Empty;
    
    [XmlElement("Value")]
    public string Value { get; set; } = string.Empty;
}

public class CafmLocation
{
    [XmlElement("LocationId")]
    public string LocationId { get; set; } = string.Empty;
    
    [XmlElement("LocationName")]
    public string LocationName { get; set; } = string.Empty;
    
    [XmlElement("ParentLocationId")]
    public string? ParentLocationId { get; set; }
    
    [XmlElement("LocationType")]
    public string? LocationType { get; set; }
    
    [XmlElement("Status")]
    public string? Status { get; set; }
    
    [XmlArray("Properties")]
    [XmlArrayItem("Property")]
    public List<CafmProperty> Properties { get; set; } = new();
}

public class CafmBreakdownTask
{
    [XmlElement("TaskId")]
    public string TaskId { get; set; } = string.Empty;
    
    [XmlElement("TaskName")]
    public string TaskName { get; set; } = string.Empty;
    
    [XmlElement("Description")]
    public string? Description { get; set; }
    
    [XmlElement("Status")]
    public string? Status { get; set; }
    
    [XmlElement("EstimatedDurationMinutes")]
    public int? EstimatedDurationMinutes { get; set; }
    
    [XmlArray("RequiredSkills")]
    [XmlArrayItem("Skill")]
    public List<string> RequiredSkills { get; set; } = new();
    
    [XmlArray("TaskDetails")]
    [XmlArrayItem("Detail")]
    public List<CafmProperty> TaskDetails { get; set; } = new();
}

public class CafmInstructionSet
{
    [XmlElement("InstructionSetId")]
    public string InstructionSetId { get; set; } = string.Empty;
    
    [XmlElement("Name")]
    public string Name { get; set; } = string.Empty;
    
    [XmlElement("Category")]
    public string? Category { get; set; }
    
    [XmlElement("AssetType")]
    public string? AssetType { get; set; }
    
    [XmlArray("Steps")]
    [XmlArrayItem("Step")]
    public List<CafmInstructionStep> Steps { get; set; } = new();
    
    [XmlArray("Metadata")]
    [XmlArrayItem("MetadataItem")]
    public List<CafmProperty> Metadata { get; set; } = new();
}

public class CafmInstructionStep
{
    [XmlElement("StepNumber")]
    public int StepNumber { get; set; }
    
    [XmlElement("Description")]
    public string Description { get; set; } = string.Empty;
    
    [XmlElement("EstimatedDurationMinutes")]
    public int? EstimatedDurationMinutes { get; set; }
    
    [XmlArray("RequiredTools")]
    [XmlArrayItem("Tool")]
    public List<string> RequiredTools { get; set; } = new();
    
    [XmlArray("StepData")]
    [XmlArrayItem("Data")]
    public List<CafmProperty> StepData { get; set; } = new();
}

public class CafmProperty
{
    [XmlElement("Name")]
    public string Name { get; set; } = string.Empty;
    
    [XmlElement("Value")]
    public string Value { get; set; } = string.Empty;
    
    [XmlElement("Type")]
    public string? Type { get; set; }
}

// SOAP Fault model
[XmlRoot("Fault", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
public class SoapFault
{
    [XmlElement("faultcode")]
    public string FaultCode { get; set; } = string.Empty;
    
    [XmlElement("faultstring")]
    public string FaultString { get; set; } = string.Empty;
    
    [XmlElement("detail")]
    public string? Detail { get; set; }
}