namespace AGI.Enterprise.Automotive.BuddyApp.NotificationService.Mgmt.DTO.DownstreamDTOs;

/// <summary>
/// Response DTO for 400 Bad Request errors from Buddy App microservice
/// Maps to Boomi profile: BuddyApp Status400 Response (2d956ec0-4920-47bf-b215-0cbcdc6ef206)
/// </summary>
public class BuddyAppStatus400ResDTO
{
    /// <summary>
    /// Array of error messages
    /// </summary>
    public List<string>? Message { get; set; }

    /// <summary>
    /// Error type
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// HTTP status code
    /// </summary>
    public int? StatusCode { get; set; }
}
