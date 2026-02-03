namespace AGI.SysD365DriverLateLoginMgmt.Attributes;

/// <summary>
/// Attribute to mark Azure Functions that require D365 authentication
/// Used by D365AuthenticationMiddleware to determine if authentication is needed
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class D365AuthenticationAttribute : Attribute
{
    /// <summary>
    /// Indicates whether authentication is required for this function
    /// Default: true
    /// </summary>
    public bool IsRequired { get; set; } = true;

    public D365AuthenticationAttribute()
    {
    }

    public D365AuthenticationAttribute(bool isRequired)
    {
        IsRequired = isRequired;
    }
}
