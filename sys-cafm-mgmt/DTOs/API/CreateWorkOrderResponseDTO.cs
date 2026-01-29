using Core.DTOs;

namespace AGI.SystemLayer.CAFM.DTOs.API;

/// <summary>
/// Response DTO for work order creation
/// </summary>
public class CreateWorkOrderResponseDTO : BaseResponseDTO
{
    public WorkOrderResult? WorkOrder { get; set; }
}

public class WorkOrderResult
{
    public List<WorkOrderItem>? Items { get; set; }
}

public class WorkOrderItem
{
    /// <summary>
    /// CAFM-generated Service Request Number (Task ID)
    /// </summary>
    public string? CafmSRNumber { get; set; }

    /// <summary>
    /// Source Service Request Number (from EQ+ or other system)
    /// </summary>
    public string? SourceSRNumber { get; set; }

    /// <summary>
    /// Source Organization ID
    /// </summary>
    public string? SourceOrgId { get; set; }
}
