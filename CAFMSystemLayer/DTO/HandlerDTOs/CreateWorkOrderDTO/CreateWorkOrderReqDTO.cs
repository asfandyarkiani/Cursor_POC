using Core.Exceptions;
using Core.SystemLayer.DTOs;

namespace CAFMSystemLayer.DTO.HandlerDTOs.CreateWorkOrderDTO
{
    public class CreateWorkOrderReqDTO : IRequestSysDTO
    {
        public List<WorkOrderItemDTO> WorkOrders { get; set; } = new List<WorkOrderItemDTO>();
        
        public void ValidateAPIRequestParameters()
        {
            List<string> errors = new List<string>();
            
            if (WorkOrders == null || WorkOrders.Count == 0)
                errors.Add("WorkOrders array cannot be empty");
            else
            {
                for (int i = 0; i < WorkOrders.Count; i++)
                {
                    WorkOrderItemDTO item = WorkOrders[i];
                    
                    if (string.IsNullOrWhiteSpace(item.ServiceRequestNumber))
                        errors.Add($"WorkOrder[{i}].ServiceRequestNumber is required");
                        
                    if (string.IsNullOrWhiteSpace(item.PropertyName))
                        errors.Add($"WorkOrder[{i}].PropertyName is required");
                        
                    if (string.IsNullOrWhiteSpace(item.UnitCode))
                        errors.Add($"WorkOrder[{i}].UnitCode is required");
                        
                    if (string.IsNullOrWhiteSpace(item.CategoryName))
                        errors.Add($"WorkOrder[{i}].CategoryName is required");
                        
                    if (string.IsNullOrWhiteSpace(item.SubCategory))
                        errors.Add($"WorkOrder[{i}].SubCategory is required");
                        
                    if (string.IsNullOrWhiteSpace(item.Description))
                        errors.Add($"WorkOrder[{i}].Description is required");
                }
            }
            
            if (errors.Count > 0)
                throw new RequestValidationFailureException() { ErrorProperties = errors };
        }
    }
    
    public class WorkOrderItemDTO
    {
        public string ReporterName { get; set; } = string.Empty;
        public string ReporterEmail { get; set; } = string.Empty;
        public string ReporterPhoneNumber { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ServiceRequestNumber { get; set; } = string.Empty;
        public string PropertyName { get; set; } = string.Empty;
        public string UnitCode { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string SubCategory { get; set; } = string.Empty;
        public string? Technician { get; set; }
        public string? SourceOrgId { get; set; }
        public TicketDetailsDTO? TicketDetails { get; set; }
    }
    
    public class TicketDetailsDTO
    {
        public string Status { get; set; } = string.Empty;
        public string SubStatus { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string ScheduledDate { get; set; } = string.Empty;
        public string ScheduledTimeStart { get; set; } = string.Empty;
        public string ScheduledTimeEnd { get; set; } = string.Empty;
    }
}
