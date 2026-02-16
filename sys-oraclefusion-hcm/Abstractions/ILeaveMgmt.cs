using Core.DTOs;
using sys_oraclefusion_hcm.DTO.CreateLeaveDTO;

namespace sys_oraclefusion_hcm.Abstractions
{
    /// <summary>
    /// Interface for Leave Management operations in Oracle Fusion HCM
    /// </summary>
    public interface ILeaveMgmt
    {
        /// <summary>
        /// Creates a leave absence entry in Oracle Fusion HCM
        /// </summary>
        /// <param name="request">Leave creation request with employee and leave details</param>
        /// <returns>BaseResponseDTO with PersonAbsenceEntryId</returns>
        Task<BaseResponseDTO> CreateLeave(CreateLeaveReqDTO request);
    }
}
