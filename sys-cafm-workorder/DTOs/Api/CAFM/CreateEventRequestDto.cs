using Core.SystemLayer.DTOs;

namespace sys_cafm_workorder.DTOs.Api.CAFM;

/// <summary>
/// Request DTO for creating an event (linking task) in CAFM.
/// </summary>
public class CreateEventRequestDto : IRequestSysDTO
{
    /// <summary>
    /// CAFM Session ID for authentication.
    /// </summary>
    public string SessionId { get; set; } = string.Empty;

    /// <summary>
    /// Task ID to link the event to.
    /// </summary>
    public string TaskId { get; set; } = string.Empty;

    /// <summary>
    /// Event type code.
    /// </summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>
    /// Event description or notes.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Date of the event (UTC).
    /// </summary>
    public DateTime? EventDateUtc { get; set; }

    public void ValidateAPIRequestParameters()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(SessionId))
            errors.Add("SessionId is required.");

        if (string.IsNullOrWhiteSpace(TaskId))
            errors.Add("TaskId is required.");

        if (errors.Count > 0)
        {
            throw new Core.Exceptions.RequestValidationFailureException(
                errorDetails: errors,
                stepName: nameof(CreateEventRequestDto));
        }
    }
}
