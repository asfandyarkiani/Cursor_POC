namespace SysCafmMgmt.Middleware
{
    /// <summary>
    /// Context holder for FSI session management
    /// Used to share session state across atomic handlers within a single request
    /// </summary>
    public class FsiSessionContext
    {
        /// <summary>
        /// Current FSI session ID obtained from authentication
        /// </summary>
        public string? SessionId { get; set; }

        /// <summary>
        /// Indicates if the session has been authenticated
        /// </summary>
        public bool IsAuthenticated => !string.IsNullOrWhiteSpace(SessionId);

        /// <summary>
        /// Timestamp when the session was created
        /// </summary>
        public DateTime? SessionCreatedAt { get; set; }

        /// <summary>
        /// Marks the session as authenticated with the given session ID
        /// </summary>
        public void SetSession(string sessionId)
        {
            SessionId = sessionId;
            SessionCreatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Clears the session
        /// </summary>
        public void ClearSession()
        {
            SessionId = null;
            SessionCreatedAt = null;
        }
    }
}
