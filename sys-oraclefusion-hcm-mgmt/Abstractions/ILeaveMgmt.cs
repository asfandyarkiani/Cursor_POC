using Core.DTOs;
using OracleFusionHcmMgmt.DTO.CreateLeaveDTO;

namespace OracleFusionHcmMgmt.Abstractions
{
    /// <summary>
    /// Service abstraction for Leave Management operations in Oracle Fusion HCM.
    /// Defines contract for leave-related operations.
    /// </summary>
    public interface ILeaveMgmt
    {
        /// <summary>
        /// Creates a leave absence entry in Oracle Fusion HCM.
        /// </summary>
        /// <param name="request">Leave creation request from Process Layer</param>
        /// <returns>Leave creation result with personAbsenceEntryId</returns>
        Task<BaseResponseDTO> CreateLeave(CreateLeaveReqDTO request);
    }
}
