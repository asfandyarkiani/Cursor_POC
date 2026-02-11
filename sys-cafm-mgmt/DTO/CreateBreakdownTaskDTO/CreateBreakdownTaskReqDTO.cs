using Core.Exceptions;
using Core.SystemLayer.DTOs;

namespace sys_cafm_mgmt.DTO.CreateBreakdownTaskDTO
{
    public class CreateBreakdownTaskReqDTO : IRequestSysDTO
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
        public string Status { get; set; } = string.Empty;
        public string SubStatus { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string ScheduledDate { get; set; } = string.Empty;
        public string ScheduledTimeStart { get; set; } = string.Empty;
        public string ScheduledTimeEnd { get; set; } = string.Empty;
        public string Recurrence { get; set; } = string.Empty;
        public string OldCAFMSRNumber { get; set; } = string.Empty;
        public string RaisedDateUtc { get; set; } = string.Empty;

        public void ValidateAPIRequestParameters()
        {
            List<string> errors = new List<string>();
            
            if (string.IsNullOrWhiteSpace(ServiceRequestNumber))
                errors.Add("ServiceRequestNumber is required");
            
            if (string.IsNullOrWhiteSpace(Description))
                errors.Add("Description is required");
            
            if (string.IsNullOrWhiteSpace(PropertyName))
                errors.Add("PropertyName is required");
            
            if (string.IsNullOrWhiteSpace(UnitCode))
                errors.Add("UnitCode is required");
            
            if (string.IsNullOrWhiteSpace(CategoryName))
                errors.Add("CategoryName is required");
            
            if (errors.Count > 0)
                throw new RequestValidationFailureException(
                    errorDetails: errors,
                    stepName: "CreateBreakdownTaskReqDTO.cs / Executing ValidateAPIRequestParameters");
        }
    }
}
