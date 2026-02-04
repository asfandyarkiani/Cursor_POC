# Boomi Process Analysis: Create Work Order from EQ+ to CAFM

## Executive Summary
**Process Name:** Create Work Order from EQ+ to CAFM  
**System of Record (SOR):** CAFM (FSI Evolution CAFM System)  
**Integration Type:** SOAP/XML-based APIs  
**Authentication:** Session-based (Login/Logout)  
**Base URL:** https://devcafm.agfacilities.com

## Function Exposure Decision Table (MANDATORY)

| Operation | Independent Invoke? | Decision Before/After? | Same SOR? | Internal Lookup? | Conclusion | Reasoning |
|-----------|-------------------|----------------------|-----------|-----------------|------------|-----------|
| **Authenticate (FsiLogin)** | NO | None | N/A | N/A | **Atomic Handler (Middleware)** | Auth lifecycle handled by middleware, not exposed as Function |
| **Logout** | NO | None | N/A | N/A | **Atomic Handler (Middleware)** | Auth lifecycle handled by middleware, not exposed as Function |
| **GetLocationsByDto** | NO | None | YES (CAFM) | YES - lookup for CreateBreakdownTask | **Atomic Handler** | Internal lookup to get location ID for work order creation |
| **GetInstructionSetsByDto** | NO | None | YES (CAFM) | YES - lookup for CreateBreakdownTask | **Atomic Handler** | Internal lookup to get instruction set ID for work order creation |
| **GetBreakdownTasksByDto** | NO | YES (BEFORE) | YES (CAFM) | YES - check before create | **Atomic Handler** | Check if task exists before creating (same SOR check-before-create pattern) |
| **CreateBreakdownTask** | YES | YES (check-before-create) | YES (CAFM) | NO | **Azure Function** | Main operation - Process Layer calls to create work order. Handler orchestrates internal operations |
| **CreateEvent/LinkTask** | NO | None | YES (CAFM) | NO - part of create flow | **Atomic Handler** | Links event to created task (part of create workflow) |

**Decision Summary:**  
"I will create **1 Azure Function** for CAFM: **CreateWorkOrderAPI**. Because this is a same-SOR operation with check-before-create pattern and internal lookups. Per Rule 1066, business decisions → Process Layer when cross-SOR, but this is same-SOR with simple if/else (if task exists skip, if not exists create). Functions: CreateWorkOrderAPI orchestrates all CAFM operations internally. Internal: GetLocationsAtomicHandler, GetInstructionSetsAtomicHandler, GetBreakdownTasksAtomicHandler (check), CreateBreakdownTaskAtomicHandler, CreateEventLinkTaskAtomicHandler. Auth: Session-based middleware (AuthenticateAtomicHandler + LogoutAtomicHandler)."

## Operations Inventory

### 1. Authenticate (FsiLogin Subprocess)
- **Operation ID:** `operation_381a025b-f3b9-4597-9902-3be49715c978` (from subprocess)
- **Type:** SOAP
- **Method:** POST
- **Endpoint:** `/services/evolution/04/09`
- **Request:** Username, Password
- **Response:** SessionId
- **Purpose:** Establish authenticated session with CAFM system

### 2. GetLocationsByDto
- **Operation ID:** `442683cb-b984-499e-b7bb-075c826905aa`
- **Type:** SOAP
- **Method:** POST
- **Request Profile:** `589e623c-b91f-4d3c-a5aa-3c767033abc5`
- **Response Profile:** `3aa0f5c5-8c95-4023-aba9-9d78dd6ade96`
- **Purpose:** Get location details by property/unit code

### 3. GetInstructionSetsByDto
- **Operation ID:** `dc3b6b85-848d-471d-8c76-ed3b7dea0fbd`
- **Type:** SOAP
- **Method:** POST
- **Request Profile:** `589e623c-b91f-4d3c-a5aa-3c767033abc5`
- **Response Profile:** `5c2f13dd-3e51-4a7c-867b-c801aaa35562`
- **Purpose:** Get instruction set details by category/subcategory

### 4. GetBreakdownTasksByDto
- **Operation ID:** `c52c74c2-95e3-4cba-990e-3ce4746836a2`
- **Type:** SOAP
- **Method:** POST
- **Request Profile:** `004838f5-51d7-4438-a693-aa82bdef7181`
- **Response Profile:** `1570c9d2-0588-410d-ad3d-bdc7d5c0ec9a`
- **Purpose:** Check if breakdown task already exists

### 5. CreateBreakdownTask
- **Operation ID:** `33dac20f-ea09-471c-91c3-91b39bc3b172`
- **Type:** SOAP
- **Method:** POST
- **Request Profile:** `362c3ec8-053c-4694-8a26-cdb931e6a411`
- **Response Profile:** `dbcca2ef-55cc-48e0-9329-1e8db4ada0c8`
- **Purpose:** Create new breakdown task/work order

### 6. CreateEvent/LinkTask
- **Operation ID:** `52166afd-a020-4de9-b49e-55400f1c0a7a`
- **Type:** SOAP
- **Method:** POST
- **Request Profile:** `23f4cc6e-f46c-47fe-ad9d-6dc191adefb9`
- **Response Profile:** `449782a0-4b04-4a7a-aa5c-aa9265fd2614`
- **Purpose:** Create event and link to breakdown task

### 7. Logout
- **Operation ID:** From FsiLogout subprocess
- **Type:** SOAP
- **Method:** POST
- **Purpose:** Terminate authenticated session

## Input Structure Analysis (Step 1a - MANDATORY)

### Request Profile Structure
- **Profile ID:** `af096014-313f-4565-9091-2bdd56eb46df`
- **Profile Name:** EQ+_CAFM_Create_Request
- **Profile Type:** profile.json
- **Root Structure:** Root/Object/workOrder/Array/ArrayElement1/Object/...
- **Array Detection:** ✅ YES - workOrder is an array
- **Array Cardinality:**
  - minOccurs: 0
  - maxOccurs: -1 (unlimited)
- **Input Type:** singlejson

### Input Format (JSON)
```json
{
  "workOrder": [
    {
      "reporterName": "string",
      "reporterEmail": "string",
      "reporterPhoneNumber": "string",
      "description": "string",
      "serviceRequestNumber": "string",
      "propertyName": "string",
      "unitCode": "string",
      "categoryName": "string",
      "subCategory": "string",
      "technician": "string",
      "sourceOrgId": "string",
      "ticketDetails": {
        "status": "string",
        "subStatus": "string",
        "priority": "string",
        "scheduledDate": "string",
        "scheduledTimeStart": "string",
        "scheduledTimeEnd": "string"
      }
    }
  ]
}
```

### Document Processing Behavior
- **Boomi Processing:** Each array element triggers separate process execution (inputType="singlejson" with array)
- **Azure Function Requirement:** Must accept array and process each element
- **Implementation Pattern:** Loop through array, process each work order, aggregate results

### Field Mapping Analysis

| Boomi Field Path | Boomi Field Name | Data Type | Required | Azure DTO Property | Notes |
|---|---|-----|----|----|---|
| Root/Object/workOrder/Array/ArrayElement1/Object/reporterName | reporterName | character | No | ReporterName | Reporter contact info |
| Root/Object/workOrder/Array/ArrayElement1/Object/reporterEmail | reporterEmail | character | No | ReporterEmail | Reporter email |
| Root/Object/workOrder/Array/ArrayElement1/Object/reporterPhoneNumber | reporterPhoneNumber | character | No | ReporterPhoneNumber | Reporter phone |
| Root/Object/workOrder/Array/ArrayElement1/Object/description | description | character | No | Description | Work order description |
| Root/Object/workOrder/Array/ArrayElement1/Object/serviceRequestNumber | serviceRequestNumber | character | No | ServiceRequestNumber | EQ+ ticket number |
| Root/Object/workOrder/Array/ArrayElement1/Object/propertyName | propertyName | character | No | PropertyName | Property location |
| Root/Object/workOrder/Array/ArrayElement1/Object/unitCode | unitCode | character | No | UnitCode | Unit/apartment code |
| Root/Object/workOrder/Array/ArrayElement1/Object/categoryName | categoryName | character | No | CategoryName | Work category |
| Root/Object/workOrder/Array/ArrayElement1/Object/subCategory | subCategory | character | No | SubCategory | Work subcategory |
| Root/Object/workOrder/Array/ArrayElement1/Object/technician | technician | character | No | Technician | Assigned technician |
| Root/Object/workOrder/Array/ArrayElement1/Object/sourceOrgId | sourceOrgId | character | No | SourceOrgId | Source organization |
| Root/Object/workOrder/Array/ArrayElement1/Object/ticketDetails/Object/status | status | character | No | Status | Ticket status |
| Root/Object/workOrder/Array/ArrayElement1/Object/ticketDetails/Object/subStatus | subStatus | character | No | SubStatus | Ticket substatus |
| Root/Object/workOrder/Array/ArrayElement1/Object/ticketDetails/Object/priority | priority | character | No | Priority | Priority level |
| Root/Object/workOrder/Array/ArrayElement1/Object/ticketDetails/Object/scheduledDate | scheduledDate | character | No | ScheduledDate | Scheduled date |
| Root/Object/workOrder/Array/ArrayElement1/Object/ticketDetails/Object/scheduledTimeStart | scheduledTimeStart | character | No | ScheduledTimeStart | Start time |
| Root/Object/workOrder/Array/ArrayElement1/Object/ticketDetails/Object/scheduledTimeEnd | scheduledTimeEnd | character | No | ScheduledTimeEnd | End time |

## Execution Sequence (Step 9 - MANDATORY)

### Business Logic Flow (Step 0)
1. **Authenticate** → Establishes session → Produces SessionId → All subsequent operations need SessionId
2. **GetLocationsByDto** → Retrieves location → Produces LocationId → CreateBreakdownTask needs LocationId
3. **GetInstructionSetsByDto** → Retrieves instruction set → Produces InstructionSetId → CreateBreakdownTask needs InstructionSetId
4. **GetBreakdownTasksByDto** → Checks if task exists → If exists, skip creation → If not exists, proceed to create
5. **CreateBreakdownTask** → Creates work order → Produces TaskId → CreateEvent needs TaskId
6. **CreateEvent/LinkTask** → Links event to task → Uses TaskId from previous step
7. **Logout** → Terminates session

### Actual Execution Order
```
START
 ├─→ Middleware: Authenticate (produces SessionId)
 |
 ├─→ FOR EACH work order in array:
 |   ├─→ GetLocationsAtomicHandler (reads: propertyName, unitCode | writes: LocationId)
 |   ├─→ GetInstructionSetsAtomicHandler (reads: categoryName, subCategory | writes: InstructionSetId)
 |   ├─→ GetBreakdownTasksAtomicHandler (reads: serviceRequestNumber | checks if exists)
 |   ├─→ IF task NOT exists:
 |   |   ├─→ CreateBreakdownTaskAtomicHandler (reads: LocationId, InstructionSetId, all work order fields | writes: TaskId)
 |   |   └─→ CreateEventLinkTaskAtomicHandler (reads: TaskId | links event)
 |   └─→ IF task exists: Skip creation
 |
 └─→ Middleware: Logout (finally block - always executes)
```

### Data Dependencies
- **SessionId** (from Authenticate) → ALL operations
- **LocationId** (from GetLocations) → CreateBreakdownTask
- **InstructionSetId** (from GetInstructionSets) → CreateBreakdownTask
- **TaskId** (from CreateBreakdownTask) → CreateEvent/LinkTask

## Handler Orchestration Rules

### Same-SOR Business Decisions → Handler Orchestrates
✅ **This process qualifies for Handler orchestration:**
- All operations are same SOR (CAFM)
- Simple check-before-create pattern (if task exists skip, if not exists create)
- Internal lookups (GetLocations, GetInstructionSets) for enrichment
- Simple sequential calls (fixed sequence, no complex iteration beyond array processing)

### Handler Implementation Pattern
```
CreateWorkOrderHandler:
 1. FOR EACH work order in request array:
    a. Call GetLocationsAtomicHandler (internal lookup)
    b. Call GetInstructionSetsAtomicHandler (internal lookup)
    c. Call GetBreakdownTasksAtomicHandler (check if exists)
    d. IF task NOT exists (simple if/else - same SOR):
       - Call CreateBreakdownTaskAtomicHandler
       - Call CreateEventLinkTaskAtomicHandler
    e. IF task exists:
       - Skip creation, return existing task info
 2. Aggregate results
 3. Return response
```

## System Layer Components to Implement

### 1. Middleware Components (MANDATORY - Session-based Auth)
- **CustomAuthenticationAttribute** - Marks functions requiring auth
- **CustomAuthenticationMiddleware** - Handles login/logout lifecycle
- **AuthenticateAtomicHandler** - Calls CAFM authenticate API
- **LogoutAtomicHandler** - Calls CAFM logout API
- **RequestContext** - AsyncLocal storage for SessionId

### 2. SOAP Infrastructure
- **CustomSoapClient** - HTTP client wrapper for SOAP operations
- **SOAPHelper** - Utility methods for SOAP envelope operations
- **SoapEnvelopes/** folder with XML templates:
  - Authenticate.xml
  - Logout.xml
  - GetLocationsByDto.xml
  - GetInstructionSetsByDto.xml
  - GetBreakdownTasksByDto.xml
  - CreateBreakdownTask.xml
  - CreateEventLinkTask.xml

### 3. DTOs
**HandlerDTOs/CreateWorkOrderDTO/**
- CreateWorkOrderReqDTO (implements IRequestSysDTO)
- CreateWorkOrderResDTO (with static Map() method)

**AtomicHandlerDTOs/** (FLAT structure)
- AuthenticationRequestDTO (implements IDownStreamRequestDTO)
- LogoutRequestDTO (implements IDownStreamRequestDTO)
- GetLocationsHandlerReqDTO (implements IDownStreamRequestDTO)
- GetInstructionSetsHandlerReqDTO (implements IDownStreamRequestDTO)
- GetBreakdownTasksHandlerReqDTO (implements IDownStreamRequestDTO)
- CreateBreakdownTaskHandlerReqDTO (implements IDownStreamRequestDTO)
- CreateEventLinkTaskHandlerReqDTO (implements IDownStreamRequestDTO)

**DownstreamDTOs/** (ALL *ApiResDTO)
- AuthenticationResponseDTO
- LogoutResponseDTO
- GetLocationsApiResDTO
- GetInstructionSetsApiResDTO
- GetBreakdownTasksApiResDTO
- CreateBreakdownTaskApiResDTO
- CreateEventLinkTaskApiResDTO

### 4. Atomic Handlers (Implementations/CAFM/AtomicHandlers/)
- AuthenticateAtomicHandler
- LogoutAtomicHandler
- GetLocationsAtomicHandler
- GetInstructionSetsAtomicHandler
- GetBreakdownTasksAtomicHandler
- CreateBreakdownTaskAtomicHandler
- CreateEventLinkTaskAtomicHandler

### 5. Handler (Implementations/CAFM/Handlers/)
- CreateWorkOrderHandler (orchestrates all atomic handlers)

### 6. Service (Implementations/CAFM/Services/)
- WorkOrderMgmtService (implements IWorkOrderMgmt)

### 7. Abstraction (Abstractions/)
- IWorkOrderMgmt interface

### 8. Function (Functions/)
- CreateWorkOrderAPI

### 9. Configuration
- **ConfigModels/AppConfigs.cs** - Add CAFM config
- **Constants/ErrorConstants.cs** - CAFM error codes
- **Constants/InfoConstants.cs** - Success messages
- **Constants/OperationNames.cs** - Operation name constants

### 10. Program.cs
- Register all middleware (ExecutionTiming → Exception → CustomAuth)
- Register all services, handlers, atomic handlers
- Configure SOAP client

## Error Codes (CAFM System)

```csharp
// Format: CAFM_OPERATION_NUMBER (4-7-4 format)
public static readonly (string ErrorCode, string Message) CAFM_AUTHENT_0001 = ("CAFM_AUTHENT_0001", "Authentication to CAFM system failed.");
public static readonly (string ErrorCode, string Message) CAFM_AUTHENT_0002 = ("CAFM_AUTHENT_0002", "Authentication succeeded but no SessionId returned.");
public static readonly (string ErrorCode, string Message) CAFM_LOGOUT_0001 = ("CAFM_LOGOUT_0001", "Failed to logout from CAFM session.");
public static readonly (string ErrorCode, string Message) CAFM_LOCGET_0001 = ("CAFM_LOCGET_0001", "Failed to get location details from CAFM.");
public static readonly (string ErrorCode, string Message) CAFM_LOCGET_0002 = ("CAFM_LOCGET_0002", "Location not found for given property/unit code.");
public static readonly (string ErrorCode, string Message) CAFM_INSGET_0001 = ("CAFM_INSGET_0001", "Failed to get instruction sets from CAFM.");
public static readonly (string ErrorCode, string Message) CAFM_INSGET_0002 = ("CAFM_INSGET_0002", "Instruction set not found for given category.");
public static readonly (string ErrorCode, string Message) CAFM_TSKCHK_0001 = ("CAFM_TSKCHK_0001", "Failed to check existing tasks in CAFM.");
public static readonly (string ErrorCode, string Message) CAFM_TSKCRT_0001 = ("CAFM_TSKCRT_0001", "Failed to create breakdown task in CAFM.");
public static readonly (string ErrorCode, string Message) CAFM_TSKCRT_0002 = ("CAFM_TSKCRT_0002", "Breakdown task already exists in CAFM.");
public static readonly (string ErrorCode, string Message) CAFM_EVTCRT_0001 = ("CAFM_EVTCRT_0001", "Failed to create event/link task in CAFM.");
```

## Sequence Diagram (ASCII)

```
┌─────────┐     ┌──────────────┐     ┌─────────────────┐     ┌───────────────┐     ┌──────────┐
│ Process │     │   System     │     │    Handler      │     │    Atomic     │     │   CAFM   │
│  Layer  │     │   Function   │     │                 │     │   Handlers    │     │  System  │
└────┬────┘     └──────┬───────┘     └────────┬────────┘     └───────┬───────┘     └─────┬────┘
     │                 │                      │                      │                   │
     │  POST /api/     │                      │                      │                   │
     │  workorder/     │                      │                      │                   │
     │  create         │                      │                      │                   │
     ├────────────────>│                      │                      │                   │
     │                 │                      │                      │                   │
     │                 │ [Middleware: Auth]   │                      │                   │
     │                 │──────────────────────┼─────────────────────>│ Authenticate     │
     │                 │                      │                      ├──────────────────>│
     │                 │                      │                      │<──────────────────┤
     │                 │                      │                      │  SessionId        │
     │                 │                      │                      │                   │
     │                 │ HandleAsync()        │                      │                   │
     │                 ├─────────────────────>│                      │                   │
     │                 │                      │                      │                   │
     │                 │                      │ FOR EACH work order: │                   │
     │                 │                      │                      │                   │
     │                 │                      │ GetLocations         │                   │
     │                 │                      ├─────────────────────>│ GetLocationsByDto│
     │                 │                      │                      ├──────────────────>│
     │                 │                      │                      │<──────────────────┤
     │                 │                      │<─────────────────────┤  LocationId       │
     │                 │                      │                      │                   │
     │                 │                      │ GetInstructionSets   │                   │
     │                 │                      ├─────────────────────>│ GetInstructionSetsByDto
     │                 │                      │                      ├──────────────────>│
     │                 │                      │                      │<──────────────────┤
     │                 │                      │<─────────────────────┤  InstructionSetId │
     │                 │                      │                      │                   │
     │                 │                      │ CheckTaskExists      │                   │
     │                 │                      ├─────────────────────>│ GetBreakdownTasksByDto
     │                 │                      │                      ├──────────────────>│
     │                 │                      │                      │<──────────────────┤
     │                 │                      │<─────────────────────┤  Task exists?     │
     │                 │                      │                      │                   │
     │                 │                      │ IF NOT exists:       │                   │
     │                 │                      │ CreateTask           │                   │
     │                 │                      ├─────────────────────>│ CreateBreakdownTask
     │                 │                      │                      ├──────────────────>│
     │                 │                      │                      │<──────────────────┤
     │                 │                      │<─────────────────────┤  TaskId           │
     │                 │                      │                      │                   │
     │                 │                      │ LinkEvent            │                   │
     │                 │                      ├─────────────────────>│ CreateEventLinkTask
     │                 │                      │                      ├──────────────────>│
     │                 │                      │                      │<──────────────────┤
     │                 │                      │<─────────────────────┤  Success          │
     │                 │                      │                      │                   │
     │                 │<─────────────────────┤                      │                   │
     │                 │  BaseResponseDTO     │                      │                   │
     │                 │                      │                      │                   │
     │                 │ [Middleware: Logout] │                      │                   │
     │                 │──────────────────────┼─────────────────────>│ Logout            │
     │                 │                      │                      ├──────────────────>│
     │                 │                      │                      │<──────────────────┤
     │                 │                      │                      │  Success          │
     │<────────────────┤                      │                      │                   │
     │  Response       │                      │                      │                   │
     │                 │                      │                      │                   │
```

## Configuration Requirements

### appsettings.json Structure
```json
{
  "AppConfigs": {
    "ASPNETCORE_ENVIRONMENT": "dev",
    "CAFMBaseUrl": "https://devcafm.agfacilities.com",
    "CAFMAuthEndpoint": "/services/evolution/04/09",
    "CAFMUsername": "",
    "CAFMPassword": "",
    "TimeoutSeconds": 50,
    "RetryCount": 0,
    "ProjectNamespace": "CAFMSystemLayer"
  },
  "HttpClientPolicy": {
    "RetryCount": 0,
    "TimeoutSeconds": 60
  },
  "Logging": {
    "LogLevel": {
      "Default": "Debug"
    }
  }
}
```

## Implementation Notes

1. **SOAP Envelope Templates:** All SOAP envelopes must be stored in `SoapEnvelopes/` folder and registered as embedded resources in .csproj
2. **Middleware Registration Order:** ExecutionTimingMiddleware (FIRST) → ExceptionHandlerMiddleware (SECOND) → CustomAuthenticationMiddleware (THIRD)
3. **SessionId Management:** Use RequestContext (AsyncLocal<T>) for thread-safe session storage, NEVER use FunctionContext.Items
4. **Handler Orchestration:** CreateWorkOrderHandler orchestrates all atomic handlers internally (same-SOR pattern)
5. **Array Processing:** Azure Function accepts array, Handler loops through each work order
6. **Check-Before-Create:** Handler implements simple if/else logic (if task exists skip, if not exists create)
7. **Error Handling:** All exceptions normalized by ExceptionHandlerMiddleware to BaseResponseDTO
8. **Performance Timing:** CustomSoapClient MUST implement timing tracking (Stopwatch + ResponseHeaders.DSTimeBreakDown)

## Next Steps

1. Create folder structure
2. Implement middleware components (auth)
3. Create SOAP infrastructure (CustomSoapClient, SOAPHelper, envelope templates)
4. Implement DTOs (all layers)
5. Implement Atomic Handlers
6. Implement Handler (orchestration)
7. Implement Service and Interface
8. Implement Azure Function
9. Configure AppConfigs, Constants, appsettings
10. Update Program.cs with all registrations
11. Test compilation
12. Create README with usage instructions

---
**Document Version:** 1.0  
**Created:** 2025-01-26  
**Purpose:** Comprehensive analysis of Boomi process for System Layer implementation
