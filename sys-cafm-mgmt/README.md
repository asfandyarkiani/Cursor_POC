# CAFM System Layer

**System of Record:** CAFM (FSI Evolution)  
**Integration Type:** SOAP/XML  
**Authentication:** Session-based (Login/Logout)  
**Framework:** .NET 8, Azure Functions v4

---

## OVERVIEW

This System Layer provides integration with CAFM (FSI Evolution) for work order management operations.

**Key Operations:**
1. **GetBreakdownTasksByDto** - Check if work order exists
2. **CreateBreakdownTask** - Create work order with location and instruction lookups
3. **CreateEvent** - Create recurring event for work order

---

## QUICK START

### Check if Work Order Exists

```bash
curl -X POST https://your-function-app.azurewebsites.net/api/cafm/breakdown-tasks/check \
  -H "Content-Type: application/json" \
  -d '{
    "serviceRequestNumber": "EQ-2025-001"
  }'
```

**Response:**

```json
{
  "message": "Breakdown tasks retrieved successfully.",
  "data": {
    "breakdownTaskId": "12345",
    "serviceRequestNumber": "EQ-2025-001",
    "taskExists": true
  }
}
```

### Create Work Order

```bash
curl -X POST https://your-function-app.azurewebsites.net/api/cafm/breakdown-tasks/create \
  -H "Content-Type: application/json" \
  -d '{
    "reporterName": "John Doe",
    "reporterEmail": "john.doe@example.com",
    "reporterPhoneNumber": "+971-50-1234567",
    "description": "Air conditioning not working",
    "serviceRequestNumber": "EQ-2025-001",
    "propertyName": "Building A",
    "unitCode": "A-101",
    "categoryName": "HVAC",
    "subCategory": "Air Conditioning",
    "technician": "Tech Team 1",
    "sourceOrgId": "ORG-123",
    "ticketDetails": {
      "status": "Open",
      "priority": "High",
      "scheduledDate": "2025-01-30",
      "scheduledTimeStart": "09:00",
      "recurrence": "N",
      "raisedDateUtc": "2025-01-28T10:30:00"
    }
  }'
```

**Response:**

```json
{
  "message": "Breakdown task created successfully.",
  "data": {
    "cafmSRNumber": "CAFM-2025-12345",
    "sourceSRNumber": "EQ-2025-001",
    "sourceOrgId": "ORG-123",
    "status": "Success",
    "message": "Work order created successfully"
  }
}
```

### Create Recurring Event

```bash
curl -X POST https://your-function-app.azurewebsites.net/api/cafm/events/create \
  -H "Content-Type: application/json" \
  -d '{
    "breakdownTaskId": "12345"
  }'
```

**Response:**

```json
{
  "message": "Event created successfully.",
  "data": {
    "eventId": "67890",
    "breakdownTaskId": "12345",
    "status": "Success",
    "message": "Event created successfully"
  }
}
```

---

## API REFERENCE

### GetBreakdownTasksByDto

**Route:** `POST /api/cafm/breakdown-tasks/check`  
**Purpose:** Check if work order already exists in CAFM

**Request:**

```json
{
  "serviceRequestNumber": "string"
}
```

**Response:**

```json
{
  "message": "string",
  "data": {
    "breakdownTaskId": "string",
    "serviceRequestNumber": "string",
    "taskExists": boolean
  }
}
```

**For complete technical details, see:** BOOMI_EXTRACTION_PHASE1.md Section 13

### CreateBreakdownTask

**Route:** `POST /api/cafm/breakdown-tasks/create`  
**Purpose:** Create work order in CAFM (orchestrates internal lookups)

**Request:** See Quick Start example above

**Response:** See Quick Start example above

**Internal Operations:**
- STEP 1: GetLocationsByDto (best-effort lookup - continues with empty if fails)
- STEP 2: GetInstructionSetsByDto (best-effort lookup - continues with empty if fails)
- STEP 3: CreateBreakdownTask (main operation - throws exception if fails)

**For complete technical details, see:** BOOMI_EXTRACTION_PHASE1.md Section 13.2

### CreateEvent

**Route:** `POST /api/cafm/events/create`  
**Purpose:** Create recurring event for work order

**Request:**

```json
{
  "breakdownTaskId": "string"
}
```

**Response:** See Quick Start example above

**For complete technical details, see:** BOOMI_EXTRACTION_PHASE1.md Section 13

---

## AUTHENTICATION

**Type:** Session-based (Login/Logout)

**How It Works:**
1. CustomAuthenticationMiddleware intercepts all Function calls marked with [CustomAuthentication]
2. BEFORE function execution: Retrieve credentials from KeyVault → Call CAFM Login → Store SessionId
3. EXECUTE function: All operations use SessionId from RequestContext
4. AFTER function execution (finally): Call CAFM Logout → Clear RequestContext

**Credentials:**
- FSI_Username: Stored in Azure KeyVault
- FSI_Password: Stored in Azure KeyVault

**For complete authentication flow, see:** BOOMI_EXTRACTION_PHASE1.md Section 20.5

---

## CONFIGURATION

### AppConfigs (appsettings.json)

**Required Settings:**
- `BaseApiUrl`: CAFM base URL
- `AuthUrl`: CAFM authentication endpoint
- `LogoutUrl`: CAFM logout endpoint
- `CreateBreakdownTaskUrl`: CAFM create breakdown task endpoint
- `GetBreakdownTasksByDtoUrl`: CAFM get breakdown tasks endpoint
- `CreateEventUrl`: CAFM create event endpoint
- `TimeoutSeconds`: HTTP timeout (default: 50)
- `RetryCount`: Retry count (default: 0)

### KeyVault Secrets

**Required Secrets:**
- `FSI-Username`: CAFM username
- `FSI-Password`: CAFM password

### Redis Cache

**Required Settings:**
- `ConnectionString`: Redis connection string
- `InstanceName`: Cache key prefix (default: "CAFMSystem:")

---

## DEPLOYMENT

### Environment Files

- `appsettings.json` - Placeholder (replaced by CI/CD)
- `appsettings.dev.json` - Development environment
- `appsettings.qa.json` - QA environment
- `appsettings.prod.json` - Production environment

**CI/CD Pipeline:** Copies content from `appsettings.{env}.json` to `appsettings.json` during deployment

### Azure Resources

**Required:**
- Azure Function App (.NET 8, v4)
- Azure KeyVault (FSI credentials)
- Azure Redis Cache (session caching)
- Application Insights (monitoring)

---

## ERROR HANDLING

### Common Errors

**Authentication Failed (HTTP 401):**
- **Cause:** Invalid FSI credentials or CAFM authentication service unavailable
- **Action:** Check KeyVault secrets (FSI-Username, FSI-Password)

**Work Order Creation Failed (HTTP 400):**
- **Cause:** Missing required fields or validation error from CAFM
- **Action:** Check request payload, ensure all required fields present

**Lookup Failures (Best-Effort):**
- **Behavior:** GetLocationsByDto and GetInstructionSetsByDto failures don't stop work order creation
- **Reason:** CAFM validates required fields (better error messages from SOR)
- **Action:** Check CAFM logs for validation errors

**For complete error handling strategy, see:** BOOMI_EXTRACTION_PHASE1.md Section 13.1 and Section 20.6

---

## MONITORING

### Application Insights

**Metrics:**
- Request duration (SYSTotalTime)
- Downstream operation timing (DSTimeBreakDown)
- Success/failure rates
- Exception tracking

**Logs:**
- Function entry/exit
- Operation start/completion
- Error details with step names

### Live Metrics

**Enabled:** Yes (host.json configuration)  
**Access:** Azure Portal → Application Insights → Live Metrics

---

## SUPPORT

### Technical Documentation

**Complete Technical Specification:**
- **BOOMI_EXTRACTION_PHASE1.md** - Complete Boomi extraction with operation classification, error handling, and sequence diagrams

**Architecture Rules:**
- **System-Layer-Rules.mdc** - System Layer architecture patterns and coding guidelines
- **BOOMI_EXTRACTION_RULES.mdc** - Boomi extraction methodology

### Key Sections

- **Section 13.1:** Operation Classification Table (error handling for each operation)
- **Section 13.2:** Enhanced Sequence Diagram (complete execution flow)
- **Section 20:** Process Layer ↔ System Layer Orchestration Diagram

---

## NOTES

**Best-Effort Lookup Pattern:**
- GetLocationsByDto and GetInstructionSetsByDto are best-effort lookups
- If they fail, work order creation continues with empty values
- CAFM system validates required fields and returns appropriate errors
- This matches original Boomi behavior (branch convergence pattern)

**Date Formatting:**
- ScheduledDateUtc: Combines scheduledDate + scheduledTimeStart, formats to ISO with .0208713Z suffix
- RaisedDateUtc: Formats to ISO with .0208713Z suffix

**Session Management:**
- SessionId stored in RequestContext (AsyncLocal pattern)
- Thread-safe across concurrent requests
- Automatically cleared after logout

---

**END OF README**
