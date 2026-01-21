using Core.DTOs;
using SysNetworkMgmt.DTO.HandlerDTOs.NetworkTestDTO;

namespace SysNetworkMgmt.Abstractions
{
    /// <summary>
    /// Interface for Network Management operations in the System Layer.
    /// Provides contracts for network-related operations such as connectivity testing.
    /// </summary>
    public interface INetworkMgmt
    {
        /// <summary>
        /// Executes a network connectivity test.
        /// </summary>
        /// <param name="request">The network test request DTO.</param>
        /// <returns>A BaseResponseDTO containing the test result.</returns>
        Task<BaseResponseDTO> NetworkTest(NetworkTestReqDTO request);
    }
}
