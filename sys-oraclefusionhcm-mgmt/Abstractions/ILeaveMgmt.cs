using sys_oraclefusionhcm_mgmt.DTO.CreateLeaveDTO;

namespace sys_oraclefusionhcm_mgmt.Abstractions;

/// <summary>
/// Interface for Leave Management operations in Oracle Fusion HCM
/// Declares all operations the business needs for managing leave/absence
/// </summary>
public interface ILeaveMgmt
{
    /// <summary>
    /// Creates a leave absence entry in Oracle Fusion HCM
    /// </summary>
    /// <param name="requestDto">Leave creation request</param>
    /// <returns>Leave creation response with PersonAbsenceEntryId</returns>
    Task<CreateLeaveResDTO> CreateLeaveAsync(CreateLeaveReqDTO requestDto);
}
