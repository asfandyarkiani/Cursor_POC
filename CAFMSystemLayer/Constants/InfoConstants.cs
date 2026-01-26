namespace CAFMSystemLayer.Constants
{
    public class InfoConstants
    {
        public const string SUCCESS = "Operation completed successfully.";
        public const string OPERATION_SUCCESS = "Operation completed successfully.";
        
        // Authentication
        public const string LOGIN_SUCCESS = "Login to CAFM successful.";
        public const string LOGOUT_SUCCESS = "Logout from CAFM successful.";
        public const string SESSION_CREATED = "CAFM session created successfully.";
        
        // Work Order operations
        public const string CREATE_WORK_ORDER_SUCCESS = "Work order created successfully in CAFM.";
        public const string GET_LOCATION_SUCCESS = "Location details retrieved successfully from CAFM.";
        public const string GET_INSTRUCTION_SET_SUCCESS = "Instruction set retrieved successfully from CAFM.";
        public const string CHECK_TASK_SUCCESS = "Task existence check completed successfully.";
        public const string LINK_EVENT_SUCCESS = "Event linked to task successfully in CAFM.";
        
        // Context keys
        public const string SESSION_ID = "SessionId";
        public const string CORRELATION_ID = "CorrelationId";
    }
}
