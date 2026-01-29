using System;

namespace CAFMSystem.Attributes
{
    /// <summary>
    /// Marks Azure Functions requiring session-based authentication with CAFM.
    /// CustomAuthenticationMiddleware handles login/logout lifecycle.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class CustomAuthenticationAttribute : Attribute
    {
    }
}
