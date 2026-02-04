using Core.DTOs;
using AGI.SysD365DriverLateLoginMgmt.DTO.SubmitDriverLateLoginDTO;

namespace AGI.SysD365DriverLateLoginMgmt.Abstractions;

/// <summary>
/// Interface for Driver Late Login Management operations
/// Defines operations for managing driver late login requests in D365
/// </summary>
public interface IDriverLateLoginMgmt
{
    /// <summary>
    /// Submits a driver late login request to D365
    /// </summary>
    /// <param name="request">Late login request details</param>
    /// <returns>Base response DTO with late login result</returns>
    Task<BaseResponseDTO> SubmitDriverLateLoginRequest(SubmitDriverLateLoginReqDTO request);
}
