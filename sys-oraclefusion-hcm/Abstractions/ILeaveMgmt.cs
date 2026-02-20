using Core.DTOs;
using OracleFusionHcm.DTO.CreateLeaveDTO;

namespace OracleFusionHcm.Abstractions
{
    public interface ILeaveMgmt
    {
        Task<BaseResponseDTO> CreateLeave(CreateLeaveReqDTO request);
    }
}
