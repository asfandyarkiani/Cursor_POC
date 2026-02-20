using Core.DTOs;
using OracleFusionHcmSystemLayer.DTO.CreateAbsenceDTO;

namespace OracleFusionHcmSystemLayer.Abstractions
{
    public interface IAbsenceMgmt
    {
        Task<BaseResponseDTO> CreateAbsence(CreateAbsenceReqDTO request);
    }
}
