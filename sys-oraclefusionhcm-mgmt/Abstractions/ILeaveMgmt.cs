using Core.DTOs;
using OracleFusionHCMSystem.DTO.CreateLeaveDTO;

namespace OracleFusionHCMSystem.Abstractions
{
    public interface ILeaveMgmt
    {
        Task<BaseResponseDTO> CreateLeave(CreateLeaveReqDTO request);
    }
}
