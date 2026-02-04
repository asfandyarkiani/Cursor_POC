using Core.SystemLayer.DTOs;

namespace sys_oraclefusionhcm_mgmt.DTO.AtomicHandlerDTOs;

/// <summary>
/// Request DTO for CreateLeaveAtomicHandler
/// Represents the payload sent to Oracle Fusion HCM absences API
/// </summary>
public class CreateLeaveHandlerReqDTO : IDownStreamRequestDTO
{
    /// <summary>
    /// Person number (mapped from EmployeeNumber)
    /// </summary>
    public string PersonNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Absence type
    /// </summary>
    public string AbsenceType { get; set; } = string.Empty;
    
    /// <summary>
    /// Employer name
    /// </summary>
    public string Employer { get; set; } = string.Empty;
    
    /// <summary>
    /// Start date (format: YYYY-MM-DD)
    /// </summary>
    public string StartDate { get; set; } = string.Empty;
    
    /// <summary>
    /// End date (format: YYYY-MM-DD)
    /// </summary>
    public string EndDate { get; set; } = string.Empty;
    
    /// <summary>
    /// Absence status code (mapped from AbsenceStatusCode)
    /// </summary>
    public string AbsenceStatusCd { get; set; } = string.Empty;
    
    /// <summary>
    /// Approval status code (mapped from ApprovalStatusCode)
    /// </summary>
    public string ApprovalStatusCd { get; set; } = string.Empty;
    
    /// <summary>
    /// Start date duration
    /// </summary>
    public decimal StartDateDuration { get; set; }
    
    /// <summary>
    /// End date duration
    /// </summary>
    public decimal EndDateDuration { get; set; }
    
    /// <summary>
    /// Maps CreateLeaveReqDTO to CreateLeaveHandlerReqDTO
    /// </summary>
    public static CreateLeaveHandlerReqDTO Map(CreateLeaveDTO.CreateLeaveReqDTO requestDto)
    {
        return new CreateLeaveHandlerReqDTO
        {
            PersonNumber = requestDto.EmployeeNumber.ToString(),
            AbsenceType = requestDto.AbsenceType,
            Employer = requestDto.Employer,
            StartDate = requestDto.StartDate,
            EndDate = requestDto.EndDate,
            AbsenceStatusCd = requestDto.AbsenceStatusCode,
            ApprovalStatusCd = requestDto.ApprovalStatusCode,
            StartDateDuration = requestDto.StartDateDuration,
            EndDateDuration = requestDto.EndDateDuration
        };
    }
}
