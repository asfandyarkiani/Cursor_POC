using Core.DTOs;
using SysNetworkMgmt.DTO.HandlerDTOs.NetworkTestDTO;

namespace SysNetworkMgmt.Abstractions
{
    /// <summary>
    /// Interface for Network Management operations in the System Layer.
    /// Provides network connectivity and health check operations.
    /// </summary>
    public interface INetworkMgmt
    {
        /// <summary>
        /// Performs a network connectivity test.
        /// This is a simple health check operation that verifies the system is operational.
        /// </summary>
        /// <param name="request">The network test request DTO.</param>
        /// <returns>A BaseResponseDTO containing the test result.</returns>
        Task<BaseResponseDTO> NetworkTest(NetworkTestReqDTO request);
    }
}
