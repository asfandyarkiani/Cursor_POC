namespace FsiCafmSystem.Attributes
{
    /// <summary>
    /// Marks Azure Functions requiring session-based authentication with FSI CAFM.
    /// CustomAuthenticationMiddleware handles login/logout lifecycle.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class CustomAuthenticationAttribute : Attribute
    {
    }
}
