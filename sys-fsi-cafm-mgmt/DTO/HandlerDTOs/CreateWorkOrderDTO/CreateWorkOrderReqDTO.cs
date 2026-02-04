using Core.Exceptions;
using Core.SystemLayer.DTOs;

namespace FsiCafmSystem.DTO.HandlerDTOs.CreateWorkOrderDTO
{
    public class CreateWorkOrderReqDTO : IRequestSysDTO
    {
        public List<WorkOrderItemDTO> WorkOrders { get; set; } = new List<WorkOrderItemDTO>();
        
        public void ValidateAPIRequestParameters()
        {
            List<string> errors = new List<string>();
            
            if (WorkOrders == null || WorkOrders.Count == 0)
            {
                errors.Add("WorkOrders array is required and cannot be empty.");
            }
            else
            {
                for (int i = 0; i < WorkOrders.Count; i++)
                {
                    WorkOrderItemDTO item = WorkOrders[i];
                    
                    if (string.IsNullOrWhiteSpace(item.ServiceRequestNumber))
                    {
                        errors.Add($"WorkOrders[{i}].ServiceRequestNumber is required.");
                    }
                    
                    if (string.IsNullOrWhiteSpace(item.Description))
                    {
                        errors.Add($"WorkOrders[{i}].Description is required.");
                    }
                    
                    if (string.IsNullOrWhiteSpace(item.UnitCode))
                    {
                        errors.Add($"WorkOrders[{i}].UnitCode is required.");
                    }
                    
                    if (string.IsNullOrWhiteSpace(item.SubCategory))
                    {
                        errors.Add($"WorkOrders[{i}].SubCategory is required.");
                    }
                    
                    if (item.TicketDetails == null)
                    {
                        errors.Add($"WorkOrders[{i}].TicketDetails is required.");
                    }
                    else
                    {
                        if (string.IsNullOrWhiteSpace(item.TicketDetails.RaisedDateUtc))
                        {
                            errors.Add($"WorkOrders[{i}].TicketDetails.RaisedDateUtc is required.");
                        }
                    }
                }
            }
            
            if (errors.Any())
            {
                throw new RequestValidationFailureException
                {
                    ErrorProperties = errors
                };
            }
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
        public string Technician { get; set; } = string.Empty;
        public string SourceOrgId { get; set; } = string.Empty;
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
        public string Recurrence { get; set; } = string.Empty;
        public string OldCAFMSRNumber { get; set; } = string.Empty;
        public string RaisedDateUtc { get; set; } = string.Empty;
    }
}
