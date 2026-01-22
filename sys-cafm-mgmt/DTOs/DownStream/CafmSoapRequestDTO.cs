using Core.SystemLayer.DTOs;

namespace SysCafmMgmt.DTOs.DownStream
{
    /// <summary>
    /// Base downstream request DTO for CAFM SOAP operations
    /// </summary>
    public class CafmSoapRequestDTO : IDownStreamRequestDTO
    {
        public string? SessionId { get; set; }
        public string? SoapAction { get; set; }
        public string? Endpoint { get; set; }
    }

    /// <summary>
    /// Downstream request DTO for CAFM Login SOAP operation
    /// </summary>
    public class LoginSoapRequestDTO : CafmSoapRequestDTO
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
    }

    /// <summary>
    /// Downstream request DTO for CAFM Logout SOAP operation
    /// </summary>
    public class LogoutSoapRequestDTO : CafmSoapRequestDTO
    {
        // SessionId inherited from base
    }

    /// <summary>
    /// Downstream request DTO for GetLocationsByDto SOAP operation
    /// </summary>
    public class GetLocationsByDtoSoapRequestDTO : CafmSoapRequestDTO
    {
        public string? LocationCode { get; set; }
        public string? PropertyName { get; set; }
    }

    /// <summary>
    /// Downstream request DTO for GetInstructionSetsByDto SOAP operation
    /// </summary>
    public class GetInstructionSetsByDtoSoapRequestDTO : CafmSoapRequestDTO
    {
        public string? InstructionSetCode { get; set; }
        public string? CategoryName { get; set; }
    }

    /// <summary>
    /// Downstream request DTO for GetBreakdownTasksByDto SOAP operation
    /// </summary>
    public class GetBreakdownTasksByDtoSoapRequestDTO : CafmSoapRequestDTO
    {
        public string? BreakdownTaskCode { get; set; }
        public string? CategoryName { get; set; }
    }

    /// <summary>
    /// Downstream request DTO for CreateEvent SOAP operation
    /// </summary>
    public class CreateEventSoapRequestDTO : CafmSoapRequestDTO
    {
        public string? EventType { get; set; }
        public string? Description { get; set; }
        public string? LocationId { get; set; }
        public string? Priority { get; set; }
        public string? ScheduledDate { get; set; }
        public string? TaskId { get; set; }
    }
}
