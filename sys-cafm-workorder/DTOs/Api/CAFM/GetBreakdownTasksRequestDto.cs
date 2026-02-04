using Core.SystemLayer.DTOs;

namespace sys_cafm_workorder.DTOs.Api.CAFM;

/// <summary>
/// Request DTO for getting breakdown tasks from CAFM.
/// </summary>
public class GetBreakdownTasksRequestDto : IRequestSysDTO
{
    /// <summary>
    /// CAFM Session ID for authentication.
    /// </summary>
    public string SessionId { get; set; } = string.Empty;

    /// <summary>
    /// The caller source ID to filter breakdown tasks.
    /// This typically maps to the service request number from the source system (e.g., EQ+).
    /// </summary>
    public string CallerSourceId { get; set; } = string.Empty;

    public void ValidateAPIRequestParameters()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(SessionId))
            errors.Add("SessionId is required.");

        if (string.IsNullOrWhiteSpace(CallerSourceId))
            errors.Add("CallerSourceId is required.");

        if (errors.Count > 0)
        {
            throw new Core.Exceptions.RequestValidationFailureException(
                errorDetails: errors,
                stepName: nameof(GetBreakdownTasksRequestDto));
        }
    }
}
