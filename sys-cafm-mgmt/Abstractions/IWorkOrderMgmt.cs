using Core.DTOs;
using SysCafmMgmt.DTOs.Requests;

namespace SysCafmMgmt.Abstractions
{
    /// <summary>
    /// Interface for Work Order management operations against CAFM (FSI) system
    /// This interface defines all work order related operations for the Facilities domain
    /// </summary>
    public interface IWorkOrderMgmt
    {
        /// <summary>
        /// Creates a work order in CAFM system from EQ+ request
        /// </summary>
        /// <param name="request">Work order creation request</param>
        /// <returns>Base response with work order creation result</returns>
        Task<BaseResponseDTO> CreateWorkOrderAsync(CreateWorkOrderRequestDto request);
    }
}
