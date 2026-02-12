using Core.DTOs;
using OracleFusionHCMSystemLayer.DTO.CreateAbsenceDTO;

namespace OracleFusionHCMSystemLayer.Abstractions
{
    /// <summary>
    /// Abstraction for Oracle Fusion HCM Absence Management operations
    /// Defines contract for absence-related operations that Process Layer can invoke
    /// </summary>
    public interface IAbsenceMgmt
    {
        /// <summary>
        /// Create a new absence entry in Oracle Fusion HCM
        /// </summary>
        /// <param name="request">Absence creation request with employee details and leave information</param>
        /// <returns>BaseResponseDTO containing PersonAbsenceEntryId and success status</returns>
        Task<BaseResponseDTO> CreateAbsence(CreateAbsenceReqDTO request);
    }
}
