using Core.SystemLayer.DTOs;

namespace sys_cafm_workorder.DTOs.Api.CAFM;

/// <summary>
/// Request DTO for getting locations from CAFM.
/// </summary>
public class GetLocationsRequestDto : IRequestSysDTO
{
    /// <summary>
    /// CAFM Session ID for authentication.
    /// </summary>
    public string SessionId { get; set; } = string.Empty;

    /// <summary>
    /// Property name to search for locations.
    /// </summary>
    public string PropertyName { get; set; } = string.Empty;

    /// <summary>
    /// Unit code within the property.
    /// </summary>
    public string UnitCode { get; set; } = string.Empty;

    public void ValidateAPIRequestParameters()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(SessionId))
            errors.Add("SessionId is required.");

        if (string.IsNullOrWhiteSpace(PropertyName))
            errors.Add("PropertyName is required.");

        if (errors.Count > 0)
        {
            throw new Core.Exceptions.RequestValidationFailureException(
                errorDetails: errors,
                stepName: nameof(GetLocationsRequestDto));
        }
    }
}
