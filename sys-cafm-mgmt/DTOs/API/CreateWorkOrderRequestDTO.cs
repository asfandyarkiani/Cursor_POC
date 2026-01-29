using Core.DTOs;

namespace AGI.SystemLayer.CAFM.DTOs.API;

/// <summary>
/// Request DTO for creating work orders in CAFM from external systems (e.g., EQ+)
/// </summary>
public class CreateWorkOrderRequestDTO : IRequestBaseDTO
{
    public WorkOrderData? WorkOrder { get; set; }
}

public class WorkOrderData
{
    public List<ServiceRequest>? ServiceRequests { get; set; }
}

public class ServiceRequest
{
    /// <summary>
    /// Service Request Number from source system (e.g., EQ+)
    /// </summary>
    public string? ServiceRequestNumber { get; set; }

    /// <summary>
    /// Source Organization ID
    /// </summary>
    public string? SourceOrgId { get; set; }

    /// <summary>
    /// Unit/Location code (barcode)
    /// </summary>
    public string? UnitCode { get; set; }

    /// <summary>
    /// Description of the work order
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Priority of the work order
    /// </summary>
    public string? Priority { get; set; }

    /// <summary>
    /// Category of the work order
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Sub-category of the work order
    /// </summary>
    public string? SubCategory { get; set; }

    /// <summary>
    /// Requested by
    /// </summary>
    public string? RequestedBy { get; set; }

    /// <summary>
    /// Contact phone number
    /// </summary>
    public string? ContactPhone { get; set; }

    /// <summary>
    /// Additional notes
    /// </summary>
    public string? Notes { get; set; }
}
