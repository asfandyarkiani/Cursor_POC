using Core.DTOs;

namespace sys_oraclefusionhcm_mgmt.DTO.CreateLeaveDTO;

/// <summary>
/// Response DTO for CreateLeave API
/// Represents the output payload returned to Process Layer after creating leave absence
/// </summary>
public class CreateLeaveResDTO : BaseResponseDTO
{
    /// <summary>
    /// Status of the operation ("success" or "failure")
    /// </summary>
    public string Status { get; set; } = string.Empty;
    
    /// <summary>
    /// Message describing the result
    /// </summary>
    public string Message { get; set; } = string.Empty;
    
    /// <summary>
    /// Person absence entry ID returned by Oracle Fusion HCM (null if operation failed)
    /// </summary>
    public long? PersonAbsenceEntryId { get; set; }
    
    /// <summary>
    /// Success flag indicating if operation completed successfully
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Creates a success response
    /// </summary>
    public static CreateLeaveResDTO CreateSuccess(long personAbsenceEntryId, string message = "Leave absence created successfully in Oracle Fusion HCM.")
    {
        return new CreateLeaveResDTO
        {
            Status = "success",
            Message = message,
            PersonAbsenceEntryId = personAbsenceEntryId,
            Success = true
        };
    }
    
    /// <summary>
    /// Creates a failure response
    /// </summary>
    public static CreateLeaveResDTO CreateFailure(string errorMessage)
    {
        return new CreateLeaveResDTO
        {
            Status = "failure",
            Message = errorMessage,
            PersonAbsenceEntryId = null,
            Success = false
        };
    }
}
