using CAFMSystem.DTO.HandlerDTOs.CreateWorkOrderDTO;
using Core.DTOs;

namespace CAFMSystem.Abstractions
{
    /// <summary>
    /// Interface for Work Order management operations in CAFM system.
    /// Defines contract for Process Layer to interact with CAFM work order operations.
    /// </summary>
    public interface IWorkOrderMgmt
    {
        /// <summary>
        /// Creates work orders in CAFM system.
        /// Orchestrates location lookup, instruction set lookup, task creation, and optional event creation.
        /// </summary>
        /// <param name="request">Work order creation request with array of work orders</param>
        /// <returns>Base response with work order creation results</returns>
        Task<BaseResponseDTO> CreateWorkOrder(CreateWorkOrderReqDTO request);
    }
}
