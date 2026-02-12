using Core.Exceptions;
using Core.SystemLayer.DTOs;

namespace OracleFusionHCMSystemLayer.DTO.AtomicHandlerDTOs
{
    public class CreateAbsenceHandlerReqDTO : IDownStreamRequestDTO
    {
        // Oracle Fusion HCM field names (from map analysis - AUTHORITATIVE)
        public string PersonNumber { get; set; } = string.Empty;
        public string AbsenceType { get; set; } = string.Empty;
        public string Employer { get; set; } = string.Empty;
        public string StartDate { get; set; } = string.Empty;
        public string EndDate { get; set; } = string.Empty;
        public string AbsenceStatusCd { get; set; } = string.Empty;
        public string ApprovalStatusCd { get; set; } = string.Empty;
        public int StartDateDuration { get; set; }
        public int EndDateDuration { get; set; }

        // Credentials (from KeyVault)
        public string? Username { get; set; }
        public string? Password { get; set; }

        public void ValidateDownStreamRequestParameters()
        {
            List<string> errors = new List<string>();

            if (string.IsNullOrWhiteSpace(PersonNumber))
                errors.Add("PersonNumber is required");

            if (string.IsNullOrWhiteSpace(AbsenceType))
                errors.Add("AbsenceType is required");

            if (string.IsNullOrWhiteSpace(Employer))
                errors.Add("Employer is required");

            if (string.IsNullOrWhiteSpace(StartDate))
                errors.Add("StartDate is required");

            if (string.IsNullOrWhiteSpace(EndDate))
                errors.Add("EndDate is required");

            if (string.IsNullOrWhiteSpace(AbsenceStatusCd))
                errors.Add("AbsenceStatusCd is required");

            if (string.IsNullOrWhiteSpace(ApprovalStatusCd))
                errors.Add("ApprovalStatusCd is required");

            if (errors.Count > 0)
                throw new RequestValidationFailureException(
                    errorDetails: errors,
                    stepName: "CreateAbsenceHandlerReqDTO.cs / Executing ValidateDownStreamRequestParameters"
                );
        }
    }
}
