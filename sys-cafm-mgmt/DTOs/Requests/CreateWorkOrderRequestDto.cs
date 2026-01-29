using Core.SystemLayer.DTOs;
using System.Text.Json.Serialization;

namespace SysCafmMgmt.DTOs.Requests
{
    /// <summary>
    /// Request DTO for creating a work order from EQ+ to CAFM
    /// Based on Boomi profile: EQ+_CAFM_Create_Request
    /// </summary>
    public class CreateWorkOrderRequestDto : IRequestSysDTO
    {
        [JsonPropertyName("workOrder")]
        public List<WorkOrderItem>? WorkOrder { get; set; }

        public void ValidateAPIRequestParameters()
        {
            if (WorkOrder == null || WorkOrder.Count == 0)
            {
                throw new Core.Exceptions.RequestValidationFailureException(
                    "Work order items are required",
                    "VALIDATION_ERROR",
                    System.Net.HttpStatusCode.BadRequest,
                    stepName: "CreateWorkOrderRequestDto.ValidateAPIRequestParameters"
                );
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
                    "Service request number is required",
                    "VALIDATION_ERROR",
                    System.Net.HttpStatusCode.BadRequest,
                    stepName: "WorkOrderItem.Validate"
                );
            }

            if (string.IsNullOrWhiteSpace(PropertyName))
            {
                throw new Core.Exceptions.RequestValidationFailureException(
                    "Property name is required",
                    "VALIDATION_ERROR",
                    System.Net.HttpStatusCode.BadRequest,
                    stepName: "WorkOrderItem.Validate"
                );
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
        public string? OldCafmSrNumber { get; set; }

        [JsonPropertyName("raisedDateUtc")]
        public string? RaisedDateUtc { get; set; }
    }
}
