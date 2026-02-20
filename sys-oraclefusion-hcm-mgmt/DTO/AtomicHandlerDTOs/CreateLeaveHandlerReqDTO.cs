using Core.Exceptions;
using Core.SystemLayer.DTOs;

namespace OracleFusionHcmMgmt.DTO.AtomicHandlerDTOs
{
    /// <summary>
    /// Request DTO for Create Leave Atomic Handler.
    /// Transforms D365 field names to Oracle Fusion HCM field names.
    /// </summary>
    public class CreateLeaveHandlerReqDTO : IDownStreamRequestDTO
    {
        // Oracle Fusion HCM field names (transformed from D365)
        public string PersonNumber { get; set; } = string.Empty;
        public string AbsenceType { get; set; } = string.Empty;
        public string Employer { get; set; } = string.Empty;
        public string StartDate { get; set; } = string.Empty;
        public string EndDate { get; set; } = string.Empty;
        public string AbsenceStatusCd { get; set; } = string.Empty;
        public string ApprovalStatusCd { get; set; } = string.Empty;
        public int StartDateDuration { get; set; }
        public int EndDateDuration { get; set; }
        
        // Authentication (from AppConfigs)
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        
        public void ValidateDownStreamRequestParameters()
        {
            List<string> errors = new List<string>();
            
            if (string.IsNullOrWhiteSpace(PersonNumber))
            {
                errors.Add("PersonNumber is required.");
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
            
            if (string.IsNullOrWhiteSpace(AbsenceStatusCd))
            {
                errors.Add("AbsenceStatusCd is required.");
            }
            
            if (string.IsNullOrWhiteSpace(ApprovalStatusCd))
            {
                errors.Add("ApprovalStatusCd is required.");
            }
            
            if (StartDateDuration <= 0)
            {
                errors.Add("StartDateDuration must be greater than 0.");
            }
            
            if (EndDateDuration <= 0)
            {
                errors.Add("EndDateDuration must be greater than 0.");
            }
            
            if (string.IsNullOrWhiteSpace(Username))
            {
                errors.Add("Username is required for Oracle Fusion authentication.");
            }
            
            if (string.IsNullOrWhiteSpace(Password))
            {
                errors.Add("Password is required for Oracle Fusion authentication.");
            }
            
            if (errors.Count > 0)
            {
                throw new RequestValidationFailureException(
                    errorDetails: errors,
                    stepName: "CreateLeaveHandlerReqDTO.cs / Executing ValidateDownStreamRequestParameters"
                );
            }
        }
    }
}
