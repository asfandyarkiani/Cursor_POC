namespace FacilitiesMgmtSystem.Attributes;

/// <summary>
/// Attribute to mark Azure Functions that require MRI authentication.
/// When applied, the MRIAuthenticationMiddleware will automatically handle
/// login before execution and logout after execution.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class MRIAuthenticationAttribute : Attribute
{
    /// <summary>
    /// Indicates whether the session should be automatically logged out after the function completes.
    /// Default is true.
    /// </summary>
    public bool AutoLogout { get; set; } = true;
}
