using Core.Exceptions;
using Core.SystemLayer.DTOs;

namespace OracleFusionHCMSystem.DTO.AtomicHandlerDTOs
{
    public class CreateLeaveHandlerReqDTO : IDownStreamRequestDTO
    {
        public int PersonNumber { get; set; }
        public string AbsenceType { get; set; } = string.Empty;
        public string Employer { get; set; } = string.Empty;
        public string StartDate { get; set; } = string.Empty;
        public string EndDate { get; set; } = string.Empty;
        public string AbsenceStatusCd { get; set; } = string.Empty;
        public string ApprovalStatusCd { get; set; } = string.Empty;
        public int StartDateDuration { get; set; }
        public int EndDateDuration { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        public void ValidateDownStreamRequestParameters()
        {
            List<string> errors = new List<string>();

            if (PersonNumber <= 0)
                errors.Add("PersonNumber is required and must be greater than 0.");

            if (string.IsNullOrWhiteSpace(AbsenceType))
                errors.Add("AbsenceType is required.");

            if (string.IsNullOrWhiteSpace(Employer))
                errors.Add("Employer is required.");

            if (string.IsNullOrWhiteSpace(StartDate))
                errors.Add("StartDate is required.");

            if (string.IsNullOrWhiteSpace(EndDate))
                errors.Add("EndDate is required.");

            if (string.IsNullOrWhiteSpace(AbsenceStatusCd))
                errors.Add("AbsenceStatusCd is required.");

            if (string.IsNullOrWhiteSpace(ApprovalStatusCd))
                errors.Add("ApprovalStatusCd is required.");

            if (StartDateDuration <= 0)
                errors.Add("StartDateDuration must be greater than 0.");

            if (EndDateDuration <= 0)
                errors.Add("EndDateDuration must be greater than 0.");

            if (string.IsNullOrWhiteSpace(Username))
                errors.Add("Username is required.");

            if (string.IsNullOrWhiteSpace(Password))
                errors.Add("Password is required.");

            if (errors.Count > 0)
                throw new RequestValidationFailureException(
                    errorDetails: errors,
                    stepName: "CreateLeaveHandlerReqDTO.cs / Executing ValidateDownStreamRequestParameters"
                );
        }
    }
}
