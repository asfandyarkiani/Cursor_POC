using Core.Exceptions;
using Core.SystemLayer.DTOs;

namespace OracleFusionHCM.LeaveManagement.DTO.CreateLeaveDTO
{
    /// <summary>
    /// Request DTO for Create Leave API (from D365)
    /// Maps to D365 Leave Create JSON Profile (febfa3e1-f719-4ee8-ba57-cdae34137ab3)
    /// </summary>
    public class CreateLeaveReqDTO : IRequestSysDTO
    {
        public int EmployeeNumber { get; set; }
        public string AbsenceType { get; set; } = string.Empty;
        public string Employer { get; set; } = string.Empty;
        public string StartDate { get; set; } = string.Empty;
        public string EndDate { get; set; } = string.Empty;
        public string AbsenceStatusCode { get; set; } = string.Empty;
        public string ApprovalStatusCode { get; set; } = string.Empty;
        public int StartDateDuration { get; set; }
        public int EndDateDuration { get; set; }
        
        public void ValidateAPIRequestParameters()
        {
            List<string> errors = new List<string>();
            
            if (EmployeeNumber <= 0)
                errors.Add("EmployeeNumber is required and must be greater than 0");
            
            if (string.IsNullOrWhiteSpace(AbsenceType))
                errors.Add("AbsenceType is required");
            
            if (string.IsNullOrWhiteSpace(Employer))
                errors.Add("Employer is required");
            
            if (string.IsNullOrWhiteSpace(StartDate))
                errors.Add("StartDate is required");
            
            if (string.IsNullOrWhiteSpace(EndDate))
                errors.Add("EndDate is required");
            
            if (string.IsNullOrWhiteSpace(AbsenceStatusCode))
                errors.Add("AbsenceStatusCode is required");
            
            if (string.IsNullOrWhiteSpace(ApprovalStatusCode))
                errors.Add("ApprovalStatusCode is required");
            
            if (StartDateDuration <= 0)
                errors.Add("StartDateDuration must be greater than 0");
            
            if (EndDateDuration <= 0)
                errors.Add("EndDateDuration must be greater than 0");
            
            if (errors.Count > 0)
                throw new RequestValidationFailureException(
                    errorDetails: errors,
                    stepName: "CreateLeaveReqDTO.cs / Executing ValidateAPIRequestParameters"
                );
        }
    }
}
