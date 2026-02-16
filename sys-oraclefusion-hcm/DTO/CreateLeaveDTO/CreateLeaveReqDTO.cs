using Core.Exceptions;
using Core.SystemLayer.DTOs;

namespace sys_oraclefusion_hcm.DTO.CreateLeaveDTO
{
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
            else if (!DateTime.TryParse(StartDate, out _))
                errors.Add("StartDate must be a valid date (YYYY-MM-DD)");

            if (string.IsNullOrWhiteSpace(EndDate))
                errors.Add("EndDate is required");
            else if (!DateTime.TryParse(EndDate, out _))
                errors.Add("EndDate must be a valid date (YYYY-MM-DD)");

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
