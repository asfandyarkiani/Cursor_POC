namespace CAFMSystemLayer.Attributes
{
    /// <summary>
    /// Marks Azure Functions requiring CAFM session-based authentication.
    /// CustomAuthenticationMiddleware handles login/logout lifecycle.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class CustomAuthenticationAttribute : Attribute
    {
    }
}
