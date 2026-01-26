namespace CAFMSystem.Middleware
{
    /// <summary>
    /// Thread-safe storage for CAFM session context using AsyncLocal pattern.
    /// Stores SessionId for the current request scope.
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
