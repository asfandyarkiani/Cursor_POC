namespace FsiCafmSystem.Constants
{
    public class InfoConstants
    {
        public const string SUCCESS = "Operation completed successfully.";
        public const string OPERATION_SUCCESS = "Operation completed successfully.";
        
        // Authentication
        public const string LOGIN_SUCCESS = "Login to FSI CAFM successful.";
        public const string LOGOUT_SUCCESS = "Logout from FSI CAFM successful.";
        public const string SESSION_CREATED = "FSI CAFM session created successfully.";
        
        // Work Order Operations
        public const string CREATE_WORK_ORDER_SUCCESS = "Work order created successfully in FSI CAFM.";
        public const string GET_LOCATION_SUCCESS = "Location details retrieved successfully.";
        public const string GET_INSTRUCTION_SETS_SUCCESS = "Instruction sets retrieved successfully.";
        public const string GET_BREAKDOWN_TASKS_SUCCESS = "Breakdown tasks retrieved successfully.";
        public const string CREATE_BREAKDOWN_TASK_SUCCESS = "Breakdown task created successfully.";
        public const string CREATE_EVENT_SUCCESS = "Event created and linked successfully.";
        
        // Email Operations
        public const string SEND_EMAIL_SUCCESS = "Email sent successfully.";
        
        // Context Keys
        public const string SESSION_ID = "SessionId";
        public const string CORRELATION_ID = "CorrelationId";
    }
}
