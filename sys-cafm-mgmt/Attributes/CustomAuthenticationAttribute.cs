namespace sys_cafm_mgmt.Attributes
{
    /// <summary>
    /// Marks Azure Functions requiring session-based authentication to CAFM system.
    /// CustomAuthenticationMiddleware handles login/logout lifecycle.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class CustomAuthenticationAttribute : Attribute
    {
    }
}
