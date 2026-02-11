using AlGhurair.SystemLayer.OracleFusionHCM.DTO.CreateLeaveDTO;

namespace AlGhurair.SystemLayer.OracleFusionHCM.Abstractions;

/// <summary>
/// Interface for Leave Management operations in Oracle Fusion HCM
/// Declares all operations the business needs for managing leaves
/// </summary>
public interface ILeaveMgmt
{
    /// <summary>
    /// Creates a new leave/absence record in Oracle Fusion HCM
    /// </summary>
    /// <param name="request">Leave creation request from D365 or Process Layer</param>
    /// <returns>Leave creation response with PersonAbsenceEntryId</returns>
    Task<CreateLeaveResDTO> CreateLeaveAsync(CreateLeaveReqDTO request);
}
