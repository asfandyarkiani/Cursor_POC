using Core.SystemLayer.DTOs;

namespace SysCafmMgmt.DTOs.API
{
    /// <summary>
    /// Base request DTO for CAFM System Layer operations
    /// </summary>
    public class CafmRequestDTO : IRequestSysDTO
    {
        public string? SessionId { get; set; }
    }

    /// <summary>
    /// Request DTO for creating a work order in CAFM
    /// </summary>
    public class CreateWorkOrderRequestDTO : CafmRequestDTO
    {
        public string? ReporterName { get; set; }
        public string? ReporterEmail { get; set; }
        public string? ReporterPhoneNumber { get; set; }
        public string? Description { get; set; }
        public string? ServiceRequestNumber { get; set; }
        public string? PropertyName { get; set; }
        public string? UnitCode { get; set; }
        public string? CategoryName { get; set; }
        public string? SubCategory { get; set; }
        public string? Technician { get; set; }
        public string? SourceOrgId { get; set; }
        public TicketDetails? TicketDetails { get; set; }
    }

    public class TicketDetails
    {
        public string? Status { get; set; }
        public string? SubStatus { get; set; }
        public string? Priority { get; set; }
        public string? ScheduledDate { get; set; }
        public string? ScheduledTimeStart { get; set; }
        public string? ScheduledTimeEnd { get; set; }
        public string? Recurrence { get; set; }
        public string? OldCAFMSRNumber { get; set; }
        public string? RaisedDateUtc { get; set; }
    }

    /// <summary>
    /// Request DTO for getting locations from CAFM
    /// </summary>
    public class GetLocationsRequestDTO : CafmRequestDTO
    {
        public string? LocationCode { get; set; }
        public string? PropertyName { get; set; }
    }

    /// <summary>
    /// Request DTO for getting instruction sets from CAFM
    /// </summary>
    public class GetInstructionSetsRequestDTO : CafmRequestDTO
    {
        public string? InstructionSetCode { get; set; }
        public string? CategoryName { get; set; }
    }

    /// <summary>
    /// Request DTO for getting breakdown tasks from CAFM
    /// </summary>
    public class GetBreakdownTasksRequestDTO : CafmRequestDTO
    {
        public string? BreakdownTaskCode { get; set; }
        public string? CategoryName { get; set; }
    }

    /// <summary>
    /// Request DTO for creating an event/linking task in CAFM
    /// </summary>
    public class CreateEventRequestDTO : CafmRequestDTO
    {
        public string? EventType { get; set; }
        public string? Description { get; set; }
        public string? LocationId { get; set; }
        public string? Priority { get; set; }
        public string? ScheduledDate { get; set; }
        public string? TaskId { get; set; }
    }
}
