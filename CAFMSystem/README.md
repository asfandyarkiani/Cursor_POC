# CAFM System Layer - Work Order Management

**System of Record:** CAFM (FSI - Facilities System International)  
**Integration Type:** SOAP/XML over HTTP  
**Authentication:** Session-based (Login → SessionId → Logout)  
**Framework:** .NET 8, Azure Functions v4

---

## OVERVIEW

This System Layer provides integration with CAFM (FSI) for work order management operations in the Facilities domain.

**Key Operations:**
1. **GetBreakdownTasksByDto** - Check if work order already exists in CAFM
2. **CreateBreakdownTask** - Create new breakdown task/work order in CAFM
3. **CreateEvent** - Link recurring event to existing breakdown task

---

## QUICK START

### Check if Task Exists

```bash
curl -X POST https://[base-url]/api/cafm/breakdown-tasks/check \
  -H "Content-Type: application/json" \
  -d '{
    "serviceRequestNumber": "EQ-2025-001234"
  }'
```

**Response:**
```json
{
  "message": "Successfully retrieved breakdown tasks from CAFM.",
  "data": {
    "tasks": [],
    "taskExists": false,
    "existingTaskId": ""
  }
}
```

### Create Breakdown Task

```bash
curl -X POST https://[base-url]/api/cafm/breakdown-tasks/create \
  -H "Content-Type: application/json" \
  -d '{
    "reporterName": "John Doe",
    "reporterEmail": "john.doe@example.com",
    "reporterPhoneNumber": "+971-50-123-4567",
    "description": "Air conditioning not working",
    "serviceRequestNumber": "EQ-2025-001234",
    "propertyName": "Al Ghurair Tower",
    "unitCode": "UNIT-301",
    "categoryName": "HVAC",
    "subCategory": "Air Conditioning",
    "sourceOrgId": "ORG-001",
    "priority": "High",
    "scheduledDate": "2025-02-25",
    "scheduledTimeStart": "11:05:41",
    "raisedDateUtc": "2025-02-24T10:30:00Z",
    "recurrence": "N"
  }'
```

**Response:**
```json
{
  "message": "Breakdown task created successfully in CAFM.",
  "data": {
    "taskId": "CAFM-2025-12345",
    "status": "Created",
    "serviceRequestNumber": "EQ-2025-001234",
    "sourceOrgId": "ORG-001"
  }
}
```

### Create Recurring Event

```bash
curl -X POST https://[base-url]/api/cafm/events/create \
  -H "Content-Type: application/json" \
  -d '{
    "taskId": "CAFM-2025-12345",
    "eventType": "Recurring"
  }'
```

---

## API REFERENCE

### GetBreakdownTasksByDto

**Route:** `POST /cafm/breakdown-tasks/check`  
**Purpose:** Check if breakdown task already exists in CAFM based on service request number

**Request:**
```json
{
  "serviceRequestNumber": "string (required)"
}
```

**Response:**
```json
{
  "message": "string",
  "data": {
    "tasks": [
      {
        "taskId": "string",
        "callId": "string",
        "status": "string"
      }
    ],
    "taskExists": "boolean",
    "existingTaskId": "string"
  }
}
```

### CreateBreakdownTask

**Route:** `POST /cafm/breakdown-tasks/create`  
**Purpose:** Create new breakdown task/work order in CAFM with automatic lookups

**Request:** See Quick Start section for complete request structure

**Response:**
```json
{
  "message": "string",
  "data": {
    "taskId": "string",
    "status": "string",
    "serviceRequestNumber": "string",
    "sourceOrgId": "string"
  }
}
```

**Notes:**
- Automatically performs best-effort lookups for locations and instructions
- Lookups may fail without stopping task creation (CAFM validates required fields)
- Date formatting: Combines scheduledDate + scheduledTimeStart into ISO format with .0208713Z suffix

### CreateEvent

**Route:** `POST /cafm/events/create`  
**Purpose:** Link recurring event to existing breakdown task

**Request:**
```json
{
  "taskId": "string (required)",
  "eventType": "string (default: Recurring)"
}
```

**For complete technical details, see:** BOOMI_EXTRACTION_PHASE1.md Section 13

---

## AUTHENTICATION

**Method:** Session-based authentication with CustomAuthenticationMiddleware

**How it works:**
1. Middleware intercepts functions marked with `[CustomAuthentication]` attribute
2. Retrieves FSI username/password from Azure KeyVault
3. Calls CAFM Login API to obtain SessionId
4. Stores SessionId in RequestContext (AsyncLocal - thread-safe)
5. Function executes with SessionId available
6. Middleware logs out from CAFM in finally block (always executes)

**SessionId Lifecycle:**
- Login → Store in RequestContext → Use in all operations → Logout

**Reference:** BOOMI_EXTRACTION_PHASE1.md Section 15.1 for authentication pattern details

---

## CONFIGURATION

### Required Settings (appsettings.{env}.json)

**AppConfigs:**
- `BaseApiUrl`: CAFM base API URL
- `LoginUrl`, `LogoutUrl`: Authentication endpoints
- `CreateBreakdownTaskUrl`, `GetBreakdownTasksByDtoUrl`: Operation endpoints
- `*SoapAction`: SOAP action headers for each operation
- `ContractId`, `CallerSourceId`: CAFM-specific identifiers
- `TimeoutSeconds`, `RetryCount`: HTTP client settings

**KeyVault:**
- `Url`: Azure KeyVault URL
- `Secrets`: Secret name mappings (FSI_Username, FSI_Password)

**RedisCache:**
- `ConnectionString`: Redis connection string
- `InstanceName`: Cache key prefix

### Secrets Management

All sensitive credentials stored in Azure KeyVault:
- FSI_Username: CAFM username
- FSI_Password: CAFM password

**Never commit secrets to appsettings.json** (leave empty, retrieve from KeyVault at runtime)

---

## DEPLOYMENT

### Environment Files

- `appsettings.json`: Placeholder (CI/CD replaces with environment-specific)
- `appsettings.dev.json`: Development environment
- `appsettings.qa.json`: QA environment
- `appsettings.prod.json`: Production environment

**CI/CD Pipeline:** Copies content from `appsettings.{env}.json` → `appsettings.json` during deployment

### Azure Resources Required

1. **Azure Function App** (.NET 8, Isolated Worker)
2. **Azure KeyVault** (for FSI credentials)
3. **Azure Redis Cache** (for caching)
4. **Application Insights** (for monitoring)

### Deployment Steps

1. Configure Azure KeyVault with FSI credentials
2. Update appsettings.{env}.json with environment-specific URLs
3. Deploy Function App via CI/CD pipeline
4. Verify Application Insights connection
5. Test endpoints with sample requests

---

## ERROR HANDLING

### Common Errors

**FSI_AUTHEN_0001:** Authentication to CAFM system failed
- **Cause:** Invalid credentials or CAFM service unavailable
- **Action:** Verify KeyVault secrets, check CAFM service status

**FSI_SESSIO_0001:** SessionId not found in request context
- **Cause:** Middleware not applied or authentication failed
- **Action:** Ensure function has `[CustomAuthentication]` attribute

**FSI_TSKCRT_0001:** Failed to create breakdown task in CAFM
- **Cause:** Invalid request data or CAFM validation failure
- **Action:** Check request fields, verify required fields populated

**FSI_TSKGET_0001:** Failed to retrieve breakdown tasks from CAFM
- **Cause:** CAFM service error or invalid CallId
- **Action:** Verify service request number format

### Best-Effort Lookup Pattern

**GetLocationsByDto** and **GetInstructionSetsByDto** use best-effort pattern:
- If lookup fails → Log warning, set empty value, CONTINUE
- CreateBreakdownTask proceeds with empty lookup values
- CAFM system validates required fields (not System Layer)

**Benefits:**
- Resilient: Lookup failures don't stop task creation
- Validation at right place: CAFM validates required fields
- Accurate errors: Error from CAFM (not generic lookup error)

**Reference:** BOOMI_EXTRACTION_PHASE1.md Section 15.3 for best-effort lookup pattern details

---

## MONITORING

### Application Insights

**Metrics tracked:**
- Request duration (ExecutionTimingMiddleware)
- Downstream API timing breakdown (DSTimeBreakDown header)
- Exception rates and types
- Success/failure rates per operation

**Custom Dimensions:**
- Operation name (AUTHENTICATE, CREATE_BREAKDOWN_TASK, etc.)
- SessionId (first 8 characters for correlation)
- ServiceRequestNumber (for tracing)

### Logs

**Log Levels:**
- **Info:** Function entry, operation start/completion, major milestones
- **Warn:** Best-effort lookup failures, recoverable errors
- **Error:** Authentication failures, main operation failures, exceptions

**Search Queries:**
- Authentication failures: `traces | where message contains "authentication failed"`
- Task creation: `traces | where message contains "CreateBreakdownTask"`
- Lookup warnings: `traces | where severityLevel == 2 and message contains "lookup"`

---

## SUPPORT

### Technical Documentation

**Complete technical specification:** BOOMI_EXTRACTION_PHASE1.md
- Section 13.1: Operation classification with error handling strategy
- Section 13.2: Complete sequence diagram with all operations
- Section 17: Request/response JSON examples for all operations

### Architecture

**Folder Structure:** See System-Layer-Rules.mdc for complete folder structure rules

**Component Flow:**
```
Function → Service (IWorkOrderMgmt) → Handler → Atomic Handler → CAFM SOAP API
```

**Middleware Order:**
1. ExecutionTimingMiddleware (tracks total time)
2. ExceptionHandlerMiddleware (normalizes exceptions)
3. CustomAuthenticationMiddleware (login/logout lifecycle)

### Contact

For questions or issues, refer to:
- BOOMI_EXTRACTION_PHASE1.md (complete technical details)
- System-Layer-Rules.mdc (architecture rules)
- Framework documentation (Core and Cache libraries)

---

**Version:** 1.0  
**Last Updated:** 2026-01-28
