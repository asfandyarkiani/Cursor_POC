namespace sys_cafm_mgmt.Helper
{
    public static class RequestContext
    {
        private static readonly AsyncLocal<string?> _sessionId = new AsyncLocal<string?>();
        private static readonly AsyncLocal<string?> _username = new AsyncLocal<string?>();
        private static readonly AsyncLocal<string?> _password = new AsyncLocal<string?>();

        public static string? SessionId
        {
            get => _sessionId.Value;
            private set => _sessionId.Value = value;
        }

        public static string? Username
        {
            get => _username.Value;
            private set => _username.Value = value;
        }

        public static string? Password
        {
            get => _password.Value;
            private set => _password.Value = value;
        }

        public static void SetSessionId(string sessionId)
        {
            SessionId = sessionId;
        }

        public static string? GetSessionId()
        {
            return SessionId;
        }

        public static void SetUsername(string username)
        {
            Username = username;
        }

        public static string? GetUsername()
        {
            return Username;
        }

        public static void SetPassword(string password)
        {
            Password = password;
        }

        public static string? GetPassword()
        {
            return Password;
        }

        public static void Clear()
        {
            SessionId = null;
            Username = null;
            Password = null;
        }
    }
}
