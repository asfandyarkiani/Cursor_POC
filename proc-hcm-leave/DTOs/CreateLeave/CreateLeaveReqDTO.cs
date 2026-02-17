using Core.Exceptions;
using Core.ProcessLayer.DTOs;
using ProcHcmLeave.Domains.HumanResource;

namespace ProcHcmLeave.DTOs.CreateLeave
{
    public class CreateLeaveReqDTO : IRequestBaseDTO, IRequestPopulatorDTO<Leave>
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

        public void Validate()
        {
            List<string> errors = new List<string>();
            
            if (EmployeeNumber <= 0)
            {
                errors.Add($"{nameof(EmployeeNumber)} is required and must be greater than 0.");
            }
            
            if (string.IsNullOrWhiteSpace(AbsenceType))
            {
                errors.Add($"{nameof(AbsenceType)} is required.");
            }
            
            if (string.IsNullOrWhiteSpace(Employer))
            {
                errors.Add($"{nameof(Employer)} is required.");
            }
            
            if (string.IsNullOrWhiteSpace(StartDate))
            {
                errors.Add($"{nameof(StartDate)} is required.");
            }
            
            if (string.IsNullOrWhiteSpace(EndDate))
            {
                errors.Add($"{nameof(EndDate)} is required.");
            }
            
            if (string.IsNullOrWhiteSpace(AbsenceStatusCode))
            {
                errors.Add($"{nameof(AbsenceStatusCode)} is required.");
            }
            
            if (string.IsNullOrWhiteSpace(ApprovalStatusCode))
            {
                errors.Add($"{nameof(ApprovalStatusCode)} is required.");
            }
            
            if (StartDateDuration <= 0)
            {
                errors.Add($"{nameof(StartDateDuration)} must be greater than 0.");
            }
            
            if (EndDateDuration <= 0)
            {
                errors.Add($"{nameof(EndDateDuration)} must be greater than 0.");
            }
            
            if (errors.Any())
            {
                throw new RequestValidationFailureException(
                    errorDetails: errors,
                    stepName: "CreateLeaveReqDTO.cs / Executing Validate"
                );
            }
        }

        public void Populate(Leave domain)
        {
            domain.EmployeeNumber = this.EmployeeNumber;
            domain.AbsenceType = this.AbsenceType;
            domain.Employer = this.Employer;
            domain.StartDate = this.StartDate;
            domain.EndDate = this.EndDate;
            domain.AbsenceStatusCode = this.AbsenceStatusCode;
            domain.ApprovalStatusCode = this.ApprovalStatusCode;
            domain.StartDateDuration = this.StartDateDuration;
            domain.EndDateDuration = this.EndDateDuration;
        }
    }
}
