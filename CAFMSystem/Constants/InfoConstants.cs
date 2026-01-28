namespace CAFMSystem.Constants
{
    /// <summary>
    /// Info/success message constants for CAFM System Layer operations.
    /// </summary>
    public class InfoConstants
    {
        // General
        public const string SUCCESS = "Operation completed successfully.";

        // Authentication
        public const string LOGIN_SUCCESS = "Successfully authenticated with CAFM system.";
        public const string LOGOUT_SUCCESS = "Successfully logged out from CAFM system.";

        // GetBreakdownTasksByDto
        public const string GET_TASKS_SUCCESS = "Successfully retrieved breakdown tasks from CAFM.";
        public const string TASK_EXISTS = "Breakdown task already exists in CAFM.";
        public const string TASK_NOT_EXISTS = "No existing breakdown task found in CAFM.";

        // GetLocationsByDto
        public const string GET_LOCATIONS_SUCCESS = "Successfully retrieved locations from CAFM.";

        // GetInstructionSetsByDto
        public const string GET_INSTRUCTIONS_SUCCESS = "Successfully retrieved instruction sets from CAFM.";

        // CreateBreakdownTask
        public const string CREATE_TASK_SUCCESS = "Breakdown task created successfully in CAFM.";
        public const string CREATE_TASK_PARTIAL = "Breakdown task created with partial success (some fields may not be populated).";

        // CreateEvent
        public const string CREATE_EVENT_SUCCESS = "Recurring event created and linked successfully in CAFM.";

        // RequestContext Keys
        public const string SESSION_ID = "SessionId";
    }
}
