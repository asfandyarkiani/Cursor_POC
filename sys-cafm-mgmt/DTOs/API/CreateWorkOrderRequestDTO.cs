using Core.SystemLayer.DTOs;
using System.Text.Json.Serialization;

namespace SysCafmMgmt.DTOs.API;

/// <summary>
/// Request DTO for Create Work Order API endpoint
/// </summary>
public class CreateWorkOrderRequestDTO : IRequestSysDTO
{
    [JsonPropertyName("workOrder")]
    public List<WorkOrderItem>? WorkOrder { get; set; }

    public void ValidateAPIRequestParameters()
    {
        if (WorkOrder == null || WorkOrder.Count == 0)
        {
            throw new Core.Exceptions.RequestValidationFailureException(
                "WorkOrder array is required and cannot be empty",
                "VALIDATION_ERROR",
                stepName: nameof(CreateWorkOrderRequestDTO));
        }

        foreach (var item in WorkOrder)
        {
            item.Validate();
        }
    }
}

public class WorkOrderItem
{
    [JsonPropertyName("reporterName")]
    public string? ReporterName { get; set; }

    [JsonPropertyName("reporterEmail")]
    public string? ReporterEmail { get; set; }

    [JsonPropertyName("reporterPhoneNumber")]
    public string? ReporterPhoneNumber { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("serviceRequestNumber")]
    public string? ServiceRequestNumber { get; set; }

    [JsonPropertyName("propertyName")]
    public string? PropertyName { get; set; }

    [JsonPropertyName("unitCode")]
    public string? UnitCode { get; set; }

    [JsonPropertyName("categoryName")]
    public string? CategoryName { get; set; }

    [JsonPropertyName("subCategory")]
    public string? SubCategory { get; set; }

    [JsonPropertyName("technician")]
    public string? Technician { get; set; }

    [JsonPropertyName("sourceOrgId")]
    public string? SourceOrgId { get; set; }

    [JsonPropertyName("ticketDetails")]
    public TicketDetails? TicketDetails { get; set; }

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(ServiceRequestNumber))
        {
            throw new Core.Exceptions.RequestValidationFailureException(
                "ServiceRequestNumber is required",
                "VALIDATION_ERROR",
                stepName: nameof(WorkOrderItem));
        }

        if (string.IsNullOrWhiteSpace(UnitCode))
        {
            throw new Core.Exceptions.RequestValidationFailureException(
                "UnitCode is required",
                "VALIDATION_ERROR",
                stepName: nameof(WorkOrderItem));
        }

        if (string.IsNullOrWhiteSpace(SubCategory))
        {
            throw new Core.Exceptions.RequestValidationFailureException(
                "SubCategory is required",
                "VALIDATION_ERROR",
                stepName: nameof(WorkOrderItem));
        }
    }
}

public class TicketDetails
{
    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("subStatus")]
    public string? SubStatus { get; set; }

    [JsonPropertyName("priority")]
    public string? Priority { get; set; }

    [JsonPropertyName("scheduledDate")]
    public string? ScheduledDate { get; set; }

    [JsonPropertyName("scheduledTimeStart")]
    public string? ScheduledTimeStart { get; set; }

    [JsonPropertyName("scheduledTimeEnd")]
    public string? ScheduledTimeEnd { get; set; }

    [JsonPropertyName("recurrence")]
    public string? Recurrence { get; set; }

    [JsonPropertyName("oldCAFMSRnumber")]
    public string? OldCAFMSRNumber { get; set; }

    [JsonPropertyName("raisedDateUtc")]
    public string? RaisedDateUtc { get; set; }
}
