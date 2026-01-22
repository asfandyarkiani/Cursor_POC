namespace SysCafmMgmt.DTOs.API
{
    /// <summary>
    /// Response DTO for CAFM login operation
    /// </summary>
    public class LoginResponseDTO
    {
        public string? SessionId { get; set; }
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// Response DTO for CAFM logout operation
    /// </summary>
    public class LogoutResponseDTO
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
    }

    /// <summary>
    /// Response DTO for creating work order in CAFM
    /// </summary>
    public class CreateWorkOrderResponseDTO
    {
        public string? WorkOrderId { get; set; }
        public string? WorkOrderNumber { get; set; }
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// Response DTO for getting locations from CAFM
    /// </summary>
    public class GetLocationsResponseDTO
    {
        public List<LocationDTO>? Locations { get; set; }
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class LocationDTO
    {
        public string? LocationId { get; set; }
        public string? LocationCode { get; set; }
        public string? LocationName { get; set; }
        public string? PropertyName { get; set; }
        public string? BuildingName { get; set; }
        public string? FloorName { get; set; }
    }

    /// <summary>
    /// Response DTO for getting instruction sets from CAFM
    /// </summary>
    public class GetInstructionSetsResponseDTO
    {
        public List<InstructionSetDTO>? InstructionSets { get; set; }
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class InstructionSetDTO
    {
        public string? InstructionSetId { get; set; }
        public string? InstructionSetCode { get; set; }
        public string? InstructionSetName { get; set; }
        public string? CategoryName { get; set; }
        public string? Description { get; set; }
    }

    /// <summary>
    /// Response DTO for getting breakdown tasks from CAFM
    /// </summary>
    public class GetBreakdownTasksResponseDTO
    {
        public List<BreakdownTaskDTO>? BreakdownTasks { get; set; }
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class BreakdownTaskDTO
    {
        public string? BreakdownTaskId { get; set; }
        public string? BreakdownTaskCode { get; set; }
        public string? BreakdownTaskName { get; set; }
        public string? CategoryName { get; set; }
        public string? Description { get; set; }
    }

    /// <summary>
    /// Response DTO for creating event in CAFM
    /// </summary>
    public class CreateEventResponseDTO
    {
        public string? EventId { get; set; }
        public string? EventNumber { get; set; }
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
