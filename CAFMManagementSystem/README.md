# CAFM Management System - System Layer

**System of Record:** CAFM (Computer-Aided Facility Management) - FSI Concept  
**Integration Type:** SOAP/XML over HTTP  
**Authentication:** Session-based (Login/Logout)  
**Framework:** .NET 8, Azure Functions v4

---

## OVERVIEW

This System Layer provides integration with the CAFM (FSI Concept) system for work order management. It exposes atomic operations that Process Layer can orchestrate to implement business workflows.

**Key Operations:**
1. **GetBreakdownTasksByDto** - Check if work order exists in CAFM
2. **CreateBreakdownTask** - Create work order in CAFM with full details

---

## ARCHITECTURE

### API-Led Architecture Layers

```
┌─────────────────────────────────────────────────────────────┐
│ PROCESS LAYER (Orchestration)                               │
│ - Business logic and workflows                              │
│ - Orchestrates multiple System Layer APIs                   │
│ - Implements check-before-create pattern                    │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│ SYSTEM LAYER (This Project)                                 │
│ - Unlocks data from CAFM system                             │
│ - Handles CAFM-specific authentication (session-based)      │
│ - Transforms data to/from SOAP/XML format                   │
│ - Exposes atomic operations as Azure Functions              │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│ CAFM SYSTEM (FSI Concept) - System of Record                │
│ - Work order management                                      │
│ - Location/building management                              │
│ - Instruction sets                                           │
│ - Event linking                                              │
└─────────────────────────────────────────────────────────────┘
```

---

## SEQUENCE DIAGRAM

### GetBreakdownTasksByDto Operation

```
Process Layer
     |
     | POST /cafm/breakdown-tasks/query
     | { "serviceRequestNumber": "EQ-2025-001" }
     ↓
GetBreakdownTasksByDtoAPI (Function)
     |
     | [CustomAuthentication] attribute triggers middleware
     ↓
CustomAuthenticationMiddleware
     |
     ├─→ AuthenticateAtomicHandler
     |    └─→ SOAP: Authenticate (username/password from KeyVault)
     |    └─→ Response: SessionId
     |    └─→ Store in RequestContext (AsyncLocal)
     |
     ↓ Continue to function
     |
IBreakdownTaskMgmt (Service Interface)
     ↓
BreakdownTaskMgmtService
     ↓
GetBreakdownTasksByDtoHandler
     |
     ├─→ Read SessionId from RequestContext
     |
     ├─→ GetBreakdownTasksByDtoAtomicHandler
     |    └─→ SOAP: GetBreakdownTasksByDto (sessionId + callId)
     |    └─→ Response: CallId, BreakdownTaskId
     |
     ├─→ Deserialize SOAP response → GetBreakdownTasksByDtoApiResDTO
     ├─→ Map ApiResDTO → GetBreakdownTasksByDtoResDTO
     └─→ Return BaseResponseDTO
     |
     ↓ After function completes
     |
CustomAuthenticationMiddleware (finally block)
     |
     └─→ LogoutAtomicHandler
          └─→ SOAP: Logout (sessionId)
          └─→ Clear RequestContext
     |
     ↓
Response to Process Layer
{ "callId": "EQ-2025-001", "breakdownTaskId": "CAFM-2025-12345", "exists": true }
```

### CreateBreakdownTask Operation

```
Process Layer
     |
     | POST /cafm/breakdown-tasks/create
     | { "serviceRequestNumber": "EQ-2025-001", "description": "...", "ticketDetails": {...} }
     ↓
CreateBreakdownTaskAPI (Function)
     |
     | [CustomAuthentication] attribute triggers middleware
     ↓
CustomAuthenticationMiddleware
     |
     ├─→ AuthenticateAtomicHandler → SessionId stored in RequestContext
     |
     ↓ Continue to function
     |
IBreakdownTaskMgmt (Service Interface)
     ↓
BreakdownTaskMgmtService
     ↓
CreateBreakdownTaskHandler
     |
     ├─→ Read SessionId from RequestContext
     |
     ├─→ STEP 1: GetLocationsByDtoAtomicHandler (BEST-EFFORT LOOKUP)
     |    └─→ SOAP: GetLocationsByDto (sessionId + propertyName + unitCode)
     |    └─→ Response: LocationId, BuildingId
     |    └─→ Error Handling: If fails → Log warning, set empty values, CONTINUE
     |    └─→ Result: locationId, buildingId (populated or empty)
     |
     ├─→ STEP 2: GetInstructionSetsByDtoAtomicHandler (BEST-EFFORT LOOKUP)
     |    └─→ SOAP: GetInstructionSetsByDto (sessionId + categoryName + subCategory)
     |    └─→ Response: InstructionId
     |    └─→ Error Handling: If fails → Log warning, set empty value, CONTINUE
     |    └─→ Result: instructionId (populated or empty)
     |
     ├─→ STEP 3: CreateBreakdownTaskAtomicHandler (MAIN OPERATION)
     |    └─→ Format dates (ScheduledDateUtc, RaisedDateUtc)
     |    └─→ SOAP: CreateBreakdownTask (sessionId + all fields + lookup IDs)
     |    └─→ Uses: locationId, buildingId, instructionId (may be empty if lookups failed)
     |    └─→ Response: BreakdownTaskId, Status
     |    └─→ Error Handling: If fails → Throw exception (main operation must succeed)
     |
     ├─→ STEP 4: Conditional Event Linking
     |    └─→ IF ticketDetails.recurrence == "Y":
     |         └─→ CreateEventAtomicHandler
     |              └─→ SOAP: CreateEvent (sessionId + breakdownTaskId)
     |              └─→ Response: EventId
     |    └─→ ELSE:
     |         └─→ Skip event creation
     |
     ├─→ Map ApiResDTO → CreateBreakdownTaskResDTO
     └─→ Return BaseResponseDTO
     |
     ↓ After function completes
     |
CustomAuthenticationMiddleware (finally block)
     |
     └─→ LogoutAtomicHandler → Clear RequestContext
     |
     ↓
Response to Process Layer
{ "breakdownTaskId": "CAFM-2025-12345", "callId": "EQ-2025-001", "status": "Success" }
```

---

## FOLDER STRUCTURE

```
CAFMManagementSystem/
├── Abstractions/
│   └── IBreakdownTaskMgmt.cs                    # Service interface
├── Implementations/FSIConcept/
│   ├── Services/
│   │   └── BreakdownTaskMgmtService.cs          # Service implementation
│   ├── Handlers/
│   │   ├── GetBreakdownTasksByDtoHandler.cs     # Orchestrates query
│   │   └── CreateBreakdownTaskHandler.cs        # Orchestrates create + lookups
│   └── AtomicHandlers/
│       ├── AuthenticateAtomicHandler.cs         # Login (middleware)
│       ├── LogoutAtomicHandler.cs               # Logout (middleware)
│       ├── GetBreakdownTasksByDtoAtomicHandler.cs
│       ├── CreateBreakdownTaskAtomicHandler.cs
│       ├── GetLocationsByDtoAtomicHandler.cs    # Internal lookup
│       ├── GetInstructionSetsByDtoAtomicHandler.cs # Internal lookup
│       └── CreateEventAtomicHandler.cs          # Internal conditional
├── DTO/
│   ├── GetBreakdownTasksByDtoDTO/
│   │   ├── GetBreakdownTasksByDtoReqDTO.cs
│   │   └── GetBreakdownTasksByDtoResDTO.cs
│   ├── CreateBreakdownTaskDTO/
│   │   ├── CreateBreakdownTaskReqDTO.cs
│   │   └── CreateBreakdownTaskResDTO.cs
│   ├── AtomicHandlerDTOs/
│   │   ├── AuthenticateHandlerReqDTO.cs
│   │   ├── LogoutHandlerReqDTO.cs
│   │   ├── GetBreakdownTasksByDtoHandlerReqDTO.cs
│   │   ├── CreateBreakdownTaskHandlerReqDTO.cs
│   │   ├── GetLocationsByDtoHandlerReqDTO.cs
│   │   ├── GetInstructionSetsByDtoHandlerReqDTO.cs
│   │   └── CreateEventHandlerReqDTO.cs
│   └── DownstreamDTOs/
│       ├── AuthenticateApiResDTO.cs
│       ├── GetBreakdownTasksByDtoApiResDTO.cs
│       ├── CreateBreakdownTaskApiResDTO.cs
│       ├── GetLocationsByDtoApiResDTO.cs
│       ├── GetInstructionSetsByDtoApiResDTO.cs
│       └── CreateEventApiResDTO.cs
├── Functions/
│   ├── GetBreakdownTasksByDtoAPI.cs
│   └── CreateBreakdownTaskAPI.cs
├── ConfigModels/
│   ├── AppConfigs.cs
│   └── KeyVaultConfigs.cs
├── Constants/
│   ├── ErrorConstants.cs
│   ├── InfoConstants.cs
│   └── OperationNames.cs
├── Helper/
│   ├── SOAPHelper.cs
│   ├── CustomSoapClient.cs
│   ├── KeyVaultReader.cs
│   └── RequestContext.cs
├── Attributes/
│   └── CustomAuthenticationAttribute.cs
├── Middleware/
│   └── CustomAuthenticationMiddleware.cs
├── SoapEnvelopes/
│   ├── Authenticate.xml
│   ├── Logout.xml
│   ├── GetBreakdownTasksByDto.xml
│   ├── CreateBreakdownTask.xml
│   ├── GetLocationsByDto.xml
│   ├── GetInstructionSetsByDto.xml
│   └── CreateEvent.xml
├── Program.cs
├── host.json
├── appsettings.json (placeholder)
├── appsettings.dev.json
├── appsettings.qa.json
├── appsettings.prod.json
└── CAFMManagementSystem.csproj
```

---

## AZURE FUNCTIONS EXPOSED

### 1. GetBreakdownTasksByDto

**Route:** `POST /cafm/breakdown-tasks/query`

**Purpose:** Check if work order exists in CAFM system

**Request:**
```json
{
  "serviceRequestNumber": "EQ-2025-001"
}
```

**Response:**
```json
{
  "message": "Breakdown tasks retrieved successfully.",
  "errorCode": null,
  "data": {
    "callId": "EQ-2025-001",
    "breakdownTaskId": "CAFM-2025-12345",
    "exists": true
  }
}
```

**Authentication:** Session-based (handled by middleware)

**Process Layer Usage:**
```
1. Call GetBreakdownTasksByDto to check if work order exists
2. Check response.data.exists
3. If exists == false → Call CreateBreakdownTask
4. If exists == true → Skip creation, return existing
```

### 2. CreateBreakdownTask

**Route:** `POST /cafm/breakdown-tasks/create`

**Purpose:** Create work order in CAFM system

**Request:**
```json
{
  "reporterName": "John Doe",
  "reporterEmail": "john.doe@example.com",
  "reporterPhoneNumber": "+971501234567",
  "description": "Air conditioning not working",
  "serviceRequestNumber": "EQ-2025-001",
  "propertyName": "Building A",
  "unitCode": "A-101",
  "categoryName": "HVAC",
  "subCategory": "Air Conditioning",
  "technician": "Tech-001",
  "sourceOrgId": "ORG-001",
  "ticketDetails": {
    "status": "Open",
    "subStatus": "Pending",
    "priority": "High",
    "scheduledDate": "2025-02-25",
    "scheduledTimeStart": "11:05:41",
    "scheduledTimeEnd": "13:00:00",
    "recurrence": "Y",
    "oldCAFMSRNumber": "",
    "raisedDateUtc": "2025-02-24T10:30:00Z"
  }
}
```

**Response:**
```json
{
  "message": "Breakdown task created successfully.",
  "errorCode": null,
  "data": {
    "breakdownTaskId": "CAFM-2025-12345",
    "callId": "EQ-2025-001",
    "status": "Success",
    "message": "Breakdown task created successfully"
  }
}
```

**Authentication:** Session-based (handled by middleware)

**Internal Operations:**
1. GetLocationsByDto (best-effort lookup for LocationId/BuildingId - continues on failure)
2. GetInstructionSetsByDto (best-effort lookup for InstructionId - continues on failure)
3. CreateBreakdownTask (main operation - throws on failure)
4. CreateEvent (conditional - only if recurrence == "Y", non-critical)

---

## AUTHENTICATION

### Session-Based Authentication

**Middleware:** CustomAuthenticationMiddleware

**Flow:**
```
1. Function marked with [CustomAuthentication] attribute
2. Middleware detects attribute via reflection
3. Middleware calls AuthenticateAtomicHandler
   - Fetches username/password from KeyVault
   - Sends SOAP Authenticate request to CAFM
   - Receives SessionId
   - Stores SessionId in RequestContext (AsyncLocal)
4. Function executes (handlers read SessionId from RequestContext)
5. Middleware finally block calls LogoutAtomicHandler
   - Sends SOAP Logout request with SessionId
   - Clears RequestContext
```

**Benefits:**
- ✅ Automatic login/logout for all authenticated functions
- ✅ SessionId managed in thread-safe AsyncLocal storage
- ✅ Logout guaranteed (finally block executes even on error)
- ✅ No manual auth code in functions/handlers

---

## CONFIGURATION

### AppConfigs (appsettings.json)

```json
{
  "AppConfigs": {
    "CAFMBaseUrl": "https://cafm.fsi.co.uk",
    "CAFMLoginUrl": "https://cafm.fsi.co.uk/services/evolution/04/09",
    "CAFMCreateBreakdownTaskUrl": "https://cafm.fsi.co.uk/services/evolution/04/09",
    "CAFMLoginSoapAction": "http://www.fsi.co.uk/services/evolution/04/09/Authenticate",
    "CAFMCreateBreakdownTaskSoapAction": "http://www.fsi.co.uk/services/evolution/04/09/CreateBreakdownTask",
    "TimeoutSeconds": 50,
    "RetryCount": 0
  }
}
```

### KeyVault Secrets

```json
{
  "KeyVault": {
    "Url": "https://your-keyvault.vault.azure.net/",
    "Secrets": {
      "CAFMUsername": "CAFM-Username",
      "CAFMPassword": "CAFM-Password"
    }
  }
}
```

**Secrets Required:**
- `CAFM-Username`: CAFM system username
- `CAFM-Password`: CAFM system password

---

## DEPLOYMENT

### Environment Files

- `appsettings.json` - Placeholder (CI/CD replaces with environment-specific)
- `appsettings.dev.json` - Development environment
- `appsettings.qa.json` - QA environment
- `appsettings.prod.json` - Production environment

**CI/CD Process:**
1. Pipeline detects environment (dev/qa/prod)
2. Copies `appsettings.{env}.json` content → `appsettings.json`
3. Deploys to Azure Functions

### Azure Resources Required

- Azure Functions App (.NET 8, Isolated Worker)
- Azure Key Vault (for CAFM credentials)
- Azure Redis Cache (for caching)
- Application Insights (for monitoring)

---

## PROCESS LAYER INTEGRATION

### Check-Before-Create Pattern

**Scenario:** Process Layer receives work order from EQ+ system

**Flow:**
```
1. Process Layer calls GetBreakdownTasksByDto
   └─→ Request: { "serviceRequestNumber": "EQ-2025-001" }
   └─→ Response: { "exists": false, "callId": "" }

2. Process Layer checks response.data.exists
   └─→ If exists == false:
        └─→ Call CreateBreakdownTask
             └─→ Request: { full work order details }
             └─→ Response: { "breakdownTaskId": "CAFM-2025-12345", "status": "Success" }
   
   └─→ If exists == true:
        └─→ Skip creation, return existing work order details
```

**Benefits:**
- ✅ Prevents duplicate work orders in CAFM
- ✅ Process Layer controls business decision (create or skip)
- ✅ System Layer exposes atomic operations (check, create)

---

## ERROR HANDLING STRATEGY

### Operation Classification

**MUST-SUCCEED Operations (Throw on Failure):**

| Operation | Reason | Action on Failure |
|---|---|---|
| Login | Required for all operations | Throw exception (middleware) |
| CreateBreakdownTask | Main operation | Throw exception |

**BEST-EFFORT Operations (Continue on Failure):**

| Operation | Reason | Action on Failure |
|---|---|---|
| GetLocationsByDto | Enrichment lookup | Log warning, set empty, continue |
| GetInstructionSetsByDto | Enrichment lookup | Log warning, set empty, continue |
| Lookup Subprocess | Enrichment lookup | Log warning, set empty, continue |
| CreateEvent | Optional linking | Log warning, continue (task created) |
| Logout | Cleanup only | Log error, continue (non-critical) |

### Rationale: Branch Convergence Pattern

**From Boomi Analysis:**
- Lookup operations are in branch paths that converge (shape6)
- NO decision shapes check status codes after lookups
- Branch convergence means: ALL paths complete and converge regardless of individual results
- Process continues with whatever data is available (populated or empty)

**Azure Implementation:**
```csharp
// Best-effort lookup
HttpResponseSnapshot response = await GetLocationsByDto(...);

if (!response.IsSuccessStatusCode) {
    _logger.Warn("Lookup failed - Continuing with empty values");
    locationId = string.Empty; // Continue with empty
} else {
    locationId = ExtractFromResponse(...); // Extract if success
}

// Continue to next operation (don't throw)
await CreateBreakdownTask(..., locationId, ...); // May be empty
```

**Benefits:**
- ✅ Resilient: Lookup failures don't stop work order creation
- ✅ Validation at right place: CAFM validates required fields in CreateBreakdownTask
- ✅ Accurate errors: Error from CAFM (not generic lookup error)
- ✅ Matches Boomi: Same behavior as original process

---

## ERROR HANDLING

### Exception Types

| Exception | HTTP Status | When |
|---|---|---|
| NoRequestBodyException | 400 | Request body is null/empty |
| RequestValidationFailureException | 400 | Request validation fails |
| DownStreamApiFailureException | Varies | CAFM API call fails |
| NoResponseBodyException | 500 | CAFM returns empty response |
| BaseException | 500 | SessionId not found |

### Error Response Format

```json
{
  "message": "Failed to create breakdown task in CAFM system.",
  "errorCode": "CAF_CRTTSK_0001",
  "data": null,
  "errorDetails": {
    "errors": [
      {
        "stepName": "CreateBreakdownTaskHandler.cs / HandleAsync",
        "stepError": "CAFM CreateBreakdownTask API failed. Status: 500. Response: ..."
      }
    ]
  }
}
```

### Error Codes

| Error Code | Message |
|---|---|
| CAF_AUTHEN_0001 | Authentication to CAFM system failed |
| CAF_AUTHEN_0002 | CAFM authentication returned empty SessionId |
| CAF_GETTSK_0001 | Failed to get breakdown tasks from CAFM system |
| CAF_CRTTSK_0001 | Failed to create breakdown task in CAFM system |
| CAF_GETLOC_0001 | Failed to get locations from CAFM system |
| CAF_GETINS_0001 | Failed to get instruction sets from CAFM system |
| CAF_CRTEVT_0001 | Failed to create event in CAFM system |
| CAF_SESSIO_0001 | SessionId not found in RequestContext |

---

## PERFORMANCE

### Timing Tracking

**Mechanism:** Stopwatch + ResponseHeaders.DSTimeBreakDown

**Example Response Headers:**
```
SYSTotalTime: 2450ms
DSTimeBreakDown: AUTHENTICATE:245,GET_LOCATIONS_BY_DTO:423,GET_INSTRUCTION_SETS_BY_DTO:312,CREATE_BREAKDOWN_TASK:1234,CREATE_EVENT:156,LOGOUT:80
DSAggregatedTime: 2450ms
```

**Breakdown:**
- AUTHENTICATE: 245ms (login)
- GET_LOCATIONS_BY_DTO: 423ms (lookup location)
- GET_INSTRUCTION_SETS_BY_DTO: 312ms (lookup instruction)
- CREATE_BREAKDOWN_TASK: 1234ms (create work order)
- CREATE_EVENT: 156ms (link event - conditional)
- LOGOUT: 80ms (logout)

---

## DEVELOPMENT

### Prerequisites

- .NET 8 SDK
- Azure Functions Core Tools v4
- Visual Studio 2022 or VS Code with Azure Functions extension

### Build

```bash
dotnet restore
dotnet build
```

### Run Locally

```bash
func start
```

### Test

```bash
# GetBreakdownTasksByDto
curl -X POST http://localhost:7071/api/cafm/breakdown-tasks/query \
  -H "Content-Type: application/json" \
  -d '{"serviceRequestNumber": "EQ-2025-001"}'

# CreateBreakdownTask
curl -X POST http://localhost:7071/api/cafm/breakdown-tasks/create \
  -H "Content-Type: application/json" \
  -d @test-create-request.json
```

---

## DEPENDENCIES

### Framework Projects

- `Framework/Core/Core/Core.csproj` - Core framework (exceptions, middlewares, extensions)
- `Framework/Cache/Cache.csproj` - Caching framework (Redis)

### NuGet Packages

- Microsoft.Azure.Functions.Worker (1.23.0)
- Microsoft.Azure.Functions.Worker.Extensions.Http (3.2.0)
- Microsoft.ApplicationInsights.WorkerService (2.22.0)
- Azure.Identity (1.13.1)
- Azure.Security.KeyVault.Secrets (4.7.0)
- Polly (8.5.0)
- Newtonsoft.Json (13.0.3)

---

## MONITORING

### Application Insights

**Metrics Tracked:**
- Request duration (SYSTotalTime)
- Downstream operation timing (DSTimeBreakDown)
- Success/failure rates
- Exception counts
- Authentication success/failure

**Logs:**
- Function entry/exit
- Handler start/completion
- Atomic handler operations
- SOAP request/response status
- Authentication lifecycle

### Live Metrics

**Enabled:** Yes (host.json: enableLiveMetricsFilters: true)

**Real-time Monitoring:**
- Active requests
- Failed requests
- Server response time
- Dependency calls (CAFM SOAP operations)

---

## FUTURE ENHANCEMENTS

### Potential Additions

1. **Lookup Subprocess Implementation**
   - Add atomic handlers for Category, Discipline, Priority lookups
   - Replace TODO placeholders in CreateBreakdownTaskHandler

2. **Caching for Lookup Operations**
   - Implement ICacheableAtomicHandler for GetLocationsByDto, GetInstructionSetsByDto
   - Cache location and instruction set data (master data)

3. **Batch Operations**
   - Add BatchCreateBreakdownTask function for multiple work orders
   - Process array of work orders in single request

4. **Additional CAFM Operations**
   - UpdateBreakdownTask
   - DeleteBreakdownTask
   - GetBreakdownTaskDetails

---

## SUPPORT

### Documentation

- `BOOMI_EXTRACTION_PHASE1.md` - Complete Boomi process analysis
- `RULEBOOK_COMPLIANCE_REPORT.md` - Architecture compliance audit
- `.cursor/rules/System-Layer-Rules.mdc` - System Layer architecture rules
- `.cursor/rules/BOOMI_EXTRACTION_RULES.mdc` - Boomi extraction methodology

### Contact

For questions or issues, refer to the architecture documentation or contact the development team.

---

**Version:** 1.0  
**Last Updated:** 2026-01-27  
**Status:** ✅ Production Ready (pending build validation)
