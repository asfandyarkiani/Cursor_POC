namespace FacilitiesMgmtSystem.Attributes;

/// <summary>
/// Attribute to mark functions that require MRI authentication.
/// When applied, the MRIAuthenticationMiddleware will authenticate the session
/// and provide the SessionId via the FunctionContext.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class MRIAuthenticationAttribute : Attribute
{
}
