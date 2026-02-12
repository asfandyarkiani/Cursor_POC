namespace FacilitiesMgmtSystem.DTO.CreateWorkOrderDTO;

/// <summary>
/// Request DTO for Create Work Order API operation.
/// </summary>
public class CreateWorkOrderRequestDTO : BaseRequestDTO
{
    /// <summary>
    /// Description of the work order.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Priority level of the work order (e.g., "High", "Medium", "Low").
    /// </summary>
    public string? Priority { get; set; }

    /// <summary>
    /// ID of the location where work is to be performed.
    /// </summary>
    public string? LocationId { get; set; }

    /// <summary>
    /// ID of the asset associated with the work order.
    /// </summary>
    public string? AssetId { get; set; }

    /// <summary>
    /// Name or ID of the person requesting the work.
    /// </summary>
    public string? RequestedBy { get; set; }

    /// <summary>
    /// Date when the work was requested (ISO 8601 format).
    /// </summary>
    public string? RequestedDate { get; set; }

    /// <summary>
    /// Due date for work completion (ISO 8601 format).
    /// </summary>
    public string? DueDate { get; set; }

    /// <summary>
    /// Type of work order (e.g., "Corrective", "Preventive", "Emergency").
    /// </summary>
    public string? WorkOrderType { get; set; }

    /// <summary>
    /// Category ID for the work order.
    /// </summary>
    public string? CategoryId { get; set; }

    /// <summary>
    /// Sub-category ID for the work order.
    /// </summary>
    public string? SubCategoryId { get; set; }

    /// <summary>
    /// ID or name of the person/team assigned to the work.
    /// </summary>
    public string? AssignedTo { get; set; }

    /// <summary>
    /// Additional notes for the work order.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// External reference number (e.g., from calling system).
    /// </summary>
    public string? ExternalReference { get; set; }

    /// <inheritdoc/>
    public override void ValidateAPIRequestParameters()
    {
        base.ValidateAPIRequestParameters();
        ValidateRequired(Description, nameof(Description));
        ValidateRequired(LocationId, nameof(LocationId));
    }
}
