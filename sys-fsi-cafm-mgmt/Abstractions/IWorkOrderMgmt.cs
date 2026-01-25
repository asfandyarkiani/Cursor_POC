using Core.DTOs;
using FsiCafmSystem.DTO.HandlerDTOs.CreateWorkOrderDTO;

namespace FsiCafmSystem.Abstractions
{
    /// <summary>
    /// Work order management interface for FSI CAFM system operations.
    /// </summary>
    public interface IWorkOrderMgmt
    {
        /// <summary>
        /// Creates work orders in FSI CAFM system.
        /// </summary>
        /// <param name="request">Work order creation request</param>
        /// <returns>Work order creation response with task IDs</returns>
        Task<BaseResponseDTO> CreateWorkOrder(CreateWorkOrderReqDTO request);
    }
}
