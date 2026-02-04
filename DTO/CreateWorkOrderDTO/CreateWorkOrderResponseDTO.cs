namespace FacilitiesMgmtSystem.DTO.CreateWorkOrderDTO;

/// <summary>
/// Response DTO for Create Work Order API operation.
/// </summary>
public class CreateWorkOrderResponseDTO : BaseResponseDTO
{
    /// <summary>
    /// The created work order data.
    /// </summary>
    public WorkOrderData? WorkOrder { get; set; }
}

/// <summary>
/// Work order data returned from Create Work Order operation.
/// </summary>
public class WorkOrderData
{
    /// <summary>
    /// Unique identifier of the created work order.
    /// </summary>
    public string? WorkOrderId { get; set; }

    /// <summary>
    /// Work order number for display/reference.
    /// </summary>
    public string? WorkOrderNumber { get; set; }

    /// <summary>
    /// Status of the work order after creation.
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// Timestamp when the work order was created.
    /// </summary>
    public string? CreatedDate { get; set; }

    /// <summary>
    /// User who created the work order.
    /// </summary>
    public string? CreatedBy { get; set; }
}
