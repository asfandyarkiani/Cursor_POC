using CAFMSystemLayer.DTO.HandlerDTOs.CreateWorkOrderDTO;
using Core.DTOs;

namespace CAFMSystemLayer.Abstractions
{
    /// <summary>
    /// Work Order Management interface for CAFM system operations.
    /// Defines contract for creating work orders in CAFM from external systems.
    /// </summary>
    public interface IWorkOrderMgmt
    {
        /// <summary>
        /// Creates work orders in CAFM system.
        /// Orchestrates location lookup, instruction set lookup, task existence check, and task creation.
        /// </summary>
        /// <param name="request">Work order creation request with array of work orders</param>
        /// <returns>BaseResponseDTO with creation results</returns>
        Task<BaseResponseDTO> CreateWorkOrder(CreateWorkOrderReqDTO request);
    }
}
