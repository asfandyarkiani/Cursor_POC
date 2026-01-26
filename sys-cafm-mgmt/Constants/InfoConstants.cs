namespace CAFMSystem.Constants
{
    public class InfoConstants
    {
        public const string SUCCESS = "Operation completed successfully.";
        public const string OPERATION_SUCCESS = "Operation completed successfully.";
        
        // Authentication
        public const string LOGIN_SUCCESS = "Login to CAFM system successful.";
        public const string LOGOUT_SUCCESS = "Logout from CAFM system successful.";
        public const string SESSION_CREATED = "CAFM session created successfully.";
        
        // Work Order operations
        public const string CREATE_WORK_ORDER_SUCCESS = "Work order created successfully in CAFM system.";
        public const string GET_LOCATION_SUCCESS = "Location details retrieved successfully from CAFM system.";
        public const string GET_INSTRUCTION_SET_SUCCESS = "Instruction set retrieved successfully from CAFM system.";
        public const string CREATE_BREAKDOWN_TASK_SUCCESS = "Breakdown task created successfully in CAFM system.";
        public const string GET_BREAKDOWN_TASK_SUCCESS = "Breakdown task retrieved successfully from CAFM system.";
        public const string CREATE_EVENT_SUCCESS = "Event created successfully in CAFM system.";
        
        // Context keys
        public const string SESSION_ID = "SessionId";
        public const string CORRELATION_ID = "CorrelationId";
    }
}
