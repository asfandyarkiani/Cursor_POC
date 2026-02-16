using Core.DTOs;
using OracleFusionHCM.LeaveManagement.DTO.CreateLeaveDTO;

namespace OracleFusionHCM.LeaveManagement.Abstractions
{
    /// <summary>
    /// Interface for Leave Management operations in Oracle Fusion HCM
    /// Defines contract for leave-related operations
    /// </summary>
    public interface ILeaveMgmt
    {
        /// <summary>
        /// Creates a leave entry in Oracle Fusion HCM
        /// </summary>
        /// <param name="request">Leave creation request from D365</param>
        /// <returns>Response with PersonAbsenceEntryId and status</returns>
        Task<BaseResponseDTO> CreateLeave(CreateLeaveReqDTO request);
    }
}
