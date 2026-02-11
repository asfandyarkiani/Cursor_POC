namespace CAFMSystem.Attributes
{
    /// <summary>
    /// Marks Azure Functions requiring CAFM session-based authentication.
    /// CAFMAuthenticationMiddleware handles login/logout lifecycle.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class CAFMAuthenticationAttribute : Attribute
    {
    }
}
