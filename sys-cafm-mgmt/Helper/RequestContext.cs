using System.Threading;

namespace CAFMSystem.Helper
{
    public static class RequestContext
    {
        private static readonly AsyncLocal<string?> _sessionId = new AsyncLocal<string?>();
        private static readonly AsyncLocal<string?> _authToken = new AsyncLocal<string?>();

        public static string? SessionId
        {
            get => _sessionId.Value;
            private set => _sessionId.Value = value;
        }

        public static string? Token
        {
            get => _authToken.Value;
            private set => _authToken.Value = value;
        }

        public static void SetSessionId(string sessionId)
        {
            SessionId = sessionId;
        }

        public static string? GetSessionId()
        {
            return SessionId;
        }

        public static void SetToken(string token)
        {
            Token = token;
        }

        public static string? GetToken()
        {
            return Token;
        }

        public static void Clear()
        {
            SessionId = null;
            Token = null;
        }
    }
}
