using Core.SystemLayer.DTOs;

namespace AGI.SysD365DriverLateLoginMgmt.DTO.AtomicHandlerDTOs;

/// <summary>
/// Request DTO for AuthenticateAtomicHandler
/// Used internally for D365 OAuth2 authentication
/// </summary>
public class AuthenticationRequestDTO : IDownStreamRequestDTO
{
    /// <summary>
    /// OAuth2 grant type (e.g., "client_credentials")
    /// </summary>
    public string GrantType { get; set; } = string.Empty;

    /// <summary>
    /// OAuth2 client ID
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// OAuth2 client secret
    /// </summary>
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>
    /// Resource URL (D365 base URL)
    /// </summary>
    public string Resource { get; set; } = string.Empty;

    /// <summary>
    /// Validates the downstream request parameters
    /// </summary>
    public void ValidateDownStreamRequestParameters()
    {
        List<string> errors = new List<string>();

        if (string.IsNullOrWhiteSpace(GrantType))
        {
            errors.Add("GrantType is required");
        }

        if (string.IsNullOrWhiteSpace(ClientId))
        {
            errors.Add("ClientId is required");
        }

        if (string.IsNullOrWhiteSpace(ClientSecret))
        {
            errors.Add("ClientSecret is required");
        }

        if (string.IsNullOrWhiteSpace(Resource))
        {
            errors.Add("Resource is required");
        }

        if (errors.Count > 0)
        {
            throw new Core.Exceptions.RequestValidationFailureException(
                errorDetails: errors,
                stepName: "AuthenticationRequestDTO.cs / ValidateDownStreamRequestParameters"
            );
        }
    }
}
