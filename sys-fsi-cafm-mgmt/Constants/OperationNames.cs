namespace FsiCafmSystem.Constants
{
    public class OperationNames
    {
        // Authentication Operations
        public const string AUTHENTICATE = "AUTHENTICATE";
        public const string LOGOUT = "LOGOUT";
        
        // Work Order Operations
        public const string GET_LOCATIONS_BY_DTO = "GET_LOCATIONS_BY_DTO";
        public const string GET_INSTRUCTION_SETS_BY_DTO = "GET_INSTRUCTION_SETS_BY_DTO";
        public const string GET_BREAKDOWN_TASKS_BY_DTO = "GET_BREAKDOWN_TASKS_BY_DTO";
        public const string CREATE_BREAKDOWN_TASK = "CREATE_BREAKDOWN_TASK";
        public const string CREATE_EVENT = "CREATE_EVENT";
        
        // Email Operations
        public const string SEND_EMAIL = "SEND_EMAIL";
    }
}
