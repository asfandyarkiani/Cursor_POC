namespace SysCafmMgmt.DTOs.DownStream
{
    /// <summary>
    /// Base downstream response DTO for CAFM SOAP operations
    /// </summary>
    public class CafmSoapResponseDTO
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public string? RawResponse { get; set; }
    }

    /// <summary>
    /// Downstream response DTO for CAFM Login SOAP operation
    /// </summary>
    public class LoginSoapResponseDTO : CafmSoapResponseDTO
    {
        public string? SessionId { get; set; }
    }

    /// <summary>
    /// Downstream response DTO for CAFM Logout SOAP operation
    /// </summary>
    public class LogoutSoapResponseDTO : CafmSoapResponseDTO
    {
        public string? Message { get; set; }
    }

    /// <summary>
    /// Downstream response DTO for GetLocationsByDto SOAP operation
    /// </summary>
    public class GetLocationsByDtoSoapResponseDTO : CafmSoapResponseDTO
    {
        public List<LocationSoapDTO>? Locations { get; set; }
    }

    public class LocationSoapDTO
    {
        public string? LocationId { get; set; }
        public string? LocationCode { get; set; }
        public string? LocationName { get; set; }
        public string? PropertyName { get; set; }
        public string? BuildingName { get; set; }
        public string? FloorName { get; set; }
    }

    /// <summary>
    /// Downstream response DTO for GetInstructionSetsByDto SOAP operation
    /// </summary>
    public class GetInstructionSetsByDtoSoapResponseDTO : CafmSoapResponseDTO
    {
        public List<InstructionSetSoapDTO>? InstructionSets { get; set; }
    }

    public class InstructionSetSoapDTO
    {
        public string? InstructionSetId { get; set; }
        public string? InstructionSetCode { get; set; }
        public string? InstructionSetName { get; set; }
        public string? CategoryName { get; set; }
        public string? Description { get; set; }
    }

    /// <summary>
    /// Downstream response DTO for GetBreakdownTasksByDto SOAP operation
    /// </summary>
    public class GetBreakdownTasksByDtoSoapResponseDTO : CafmSoapResponseDTO
    {
        public List<BreakdownTaskSoapDTO>? BreakdownTasks { get; set; }
    }

    public class BreakdownTaskSoapDTO
    {
        public string? BreakdownTaskId { get; set; }
        public string? BreakdownTaskCode { get; set; }
        public string? BreakdownTaskName { get; set; }
        public string? CategoryName { get; set; }
        public string? Description { get; set; }
    }

    /// <summary>
    /// Downstream response DTO for CreateEvent SOAP operation
    /// </summary>
    public class CreateEventSoapResponseDTO : CafmSoapResponseDTO
    {
        public string? EventId { get; set; }
        public string? EventNumber { get; set; }
    }
}
