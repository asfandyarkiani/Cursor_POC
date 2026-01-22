namespace AGI.SystemLayer.CAFM.Attributes;

/// <summary>
/// Attribute to mark Azure Functions that require CAFM authentication.
/// When applied, the CAFMAuthenticationMiddleware will automatically handle
/// login/logout and session management.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class CAFMAuthenticationAttribute : Attribute
{
    /// <summary>
    /// Whether to automatically logout after the operation completes
    /// </summary>
    public bool AutoLogout { get; set; } = true;
}
