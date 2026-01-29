namespace CAFMSystemLayer.Helper
{
    /// <summary>
    /// Thread-safe storage for CAFM session information using AsyncLocal pattern.
    /// Used by CustomAuthenticationMiddleware to store SessionId across async operations.
    /// </summary>
    public static class RequestContext
    {
        private static readonly AsyncLocal<string?> _sessionId = new();
        
        public static string? SessionId
        {
            get => _sessionId.Value;
            private set => _sessionId.Value = value;
        }
        
        public static void SetSessionId(string sessionId)
        {
            SessionId = sessionId;
        }
        
        public static string? GetSessionId()
        {
            return SessionId;
        }
        
        public static void Clear()
        {
            SessionId = null;
        }
    }
}
