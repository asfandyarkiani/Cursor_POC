using Core.Exceptions;
using Core.SystemLayer.DTOs;

namespace OracleFusionHcmMgmt.DTO.CreateLeaveDTO
{
    /// <summary>
    /// Request DTO for Create Leave API.
    /// Receives leave request from Process Layer (D365 format).
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
            {
                errors.Add("EmployeeNumber must be greater than 0.");
            }
            
            if (string.IsNullOrWhiteSpace(AbsenceType))
            {
                errors.Add("AbsenceType is required.");
            }
            
            if (string.IsNullOrWhiteSpace(Employer))
            {
                errors.Add("Employer is required.");
            }
            
            if (string.IsNullOrWhiteSpace(StartDate))
            {
                errors.Add("StartDate is required.");
            }
            
            if (string.IsNullOrWhiteSpace(EndDate))
            {
                errors.Add("EndDate is required.");
            }
            
            if (string.IsNullOrWhiteSpace(AbsenceStatusCode))
            {
                errors.Add("AbsenceStatusCode is required.");
            }
            
            if (string.IsNullOrWhiteSpace(ApprovalStatusCode))
            {
                errors.Add("ApprovalStatusCode is required.");
            }
            
            if (StartDateDuration <= 0)
            {
                errors.Add("StartDateDuration must be greater than 0.");
            }
            
            if (EndDateDuration <= 0)
            {
                errors.Add("EndDateDuration must be greater than 0.");
            }
            
            if (errors.Count > 0)
            {
                throw new RequestValidationFailureException(
                    errorDetails: errors,
                    stepName: "CreateLeaveReqDTO.cs / Executing ValidateAPIRequestParameters"
                );
            }
        }
    }
}
