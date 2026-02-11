# CAFM System Layer

## Overview
System Layer API for CAFM (Computer-Aided Facility Management) system integration. Provides work order management operations for creating breakdown tasks from external systems (EQ+).

**System of Record:** FSI Evolution CAFM  
**Integration Type:** SOAP/XML-based APIs  
**Authentication:** Session-based (Login/Logout middleware)  
**Base URL:** https://devcafm.agfacilities.com

## Architecture

### API-Led Architecture Layers
- **System Layer (This Project):** Unlocks data from CAFM system, handles SOAP operations, manages authentication lifecycle
- **Process Layer:** Orchestrates business logic, aggregates data from multiple System APIs
- **Experience Layer:** Formats data for specific channels (Mobile, Web, IoT)

### Components
```
Function → Service → Handler → Atomic Handler → CAFM SOAP API
   ↓
Middleware (Auth: Login → Execute → Logout)
```

## API Endpoints

### Create Work Order
**Endpoint:** `POST /api/cafm/workorder/create`  
**Authentication:** Session-based (handled by middleware)  
**Description:** Creates work orders in CAFM system from EQ+ service requests

**Request Body:**
```json
{
  "workOrders": [
    {
      "reporterName": "John Doe",
      "reporterEmail": "john.doe@example.com",
      "reporterPhoneNumber": "+971501234567",
      "description": "AC not working in unit",
      "serviceRequestNumber": "EQ-2024-001234",
      "propertyName": "Tower A",
      "unitCode": "A-101",
      "categoryName": "HVAC",
      "subCategory": "Air Conditioning",
      "technician": "Tech-001",
      "sourceOrgId": "ORG-001",
      "ticketDetails": {
        "status": "Open",
        "subStatus": "Pending Assignment",
        "priority": "High",
        "scheduledDate": "2024-01-26",
        "scheduledTimeStart": "09:00",
        "scheduledTimeEnd": "12:00"
      }
    }
  ]
}
```

**Response:**
```json
{
  "message": "Work order created successfully in CAFM.",
  "errorCode": null,
  "data": {
    "results": [
      {
        "serviceRequestNumber": "EQ-2024-001234",
        "status": "Success",
        "taskId": "CAFM-TASK-12345",
        "locationId": "LOC-001",
        "instructionSetId": "INS-001",
        "eventId": "EVT-001",
        "message": "Work order created successfully",
        "alreadyExists": false
      }
    ],
    "totalProcessed": 1,
    "successCount": 1,
    "failureCount": 0,
    "skippedCount": 0
  }
}
```

## Sequence Diagram

```
┌─────────┐     ┌──────────────┐     ┌─────────────────┐     ┌───────────────┐     ┌──────────┐
│ Process │     │   System     │     │    Handler      │     │    Atomic     │     │   CAFM   │
│  Layer  │     │   Function   │     │                 │     │   Handlers    │     │  System  │
└────┬────┘     └──────┬───────┘     └────────┬────────┘     └───────┬───────┘     └─────┬────┘
     │                 │                      │                      │                   │
     │  POST /api/     │                      │                      │                   │
     │  cafm/workorder/│                      │                      │                   │
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

## Configuration

### Required Configuration (appsettings.{env}.json)

```json
{
  "AppConfigs": {
    "ASPNETCORE_ENVIRONMENT": "dev",
    "CAFMBaseUrl": "https://devcafm.agfacilities.com",
    "CAFMAuthEndpoint": "/services/evolution/04/09",
    "CAFMUsername": "<CAFM_USERNAME>",
    "CAFMPassword": "<CAFM_PASSWORD>",
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

### Configuration Parameters

| Parameter | Description | Required | Default |
|-----------|-------------|----------|---------|
| CAFMBaseUrl | CAFM system base URL | Yes | - |
| CAFMAuthEndpoint | Authentication endpoint path | Yes | /services/evolution/04/09 |
| CAFMUsername | CAFM login username | Yes | - |
| CAFMPassword | CAFM login password | Yes | - |
| TimeoutSeconds | HTTP request timeout | No | 50 |
| RetryCount | Number of retry attempts | No | 0 |
| ProjectNamespace | Namespace for embedded resources | Yes | CAFMSystemLayer |

### Environment-Specific Files
- `appsettings.dev.json` - Development environment
- `appsettings.qa.json` - QA/Testing environment
- `appsettings.prod.json` - Production environment

**Note:** All environment files must have identical structure (same keys), only values differ.

## Local Development

### Prerequisites
- .NET 8 SDK
- Azure Functions Core Tools v4
- Visual Studio 2022 or VS Code with Azure Functions extension

### Setup Steps

1. **Clone Repository**
   ```bash
   git clone <repository-url>
   cd CAFMSystemLayer
   ```

2. **Configure Settings**
   - Update `appsettings.dev.json` with CAFM credentials
   - Set `CAFMUsername` and `CAFMPassword`
   - Verify `CAFMBaseUrl` points to correct environment

3. **Restore Dependencies**
   ```bash
   dotnet restore
   ```

4. **Build Project**
   ```bash
   dotnet build
   ```

5. **Run Locally**
   ```bash
   func start
   ```

6. **Test Endpoint**
   ```bash
   curl -X POST http://localhost:7071/api/cafm/workorder/create \
     -H "Content-Type: application/json" \
     -d @test-request.json
   ```

## Operations Flow

### Handler Orchestration (Same-SOR Pattern)
The `CreateWorkOrderHandler` orchestrates all CAFM operations internally:

1. **Get Location** - Retrieves location ID from property/unit code
2. **Get Instruction Set** - Retrieves instruction set ID from category/subcategory
3. **Check Task Exists** - Verifies if work order already exists
4. **Create Task** (if not exists) - Creates breakdown task in CAFM
5. **Link Event** - Creates event and links to task

### Authentication Lifecycle
- **Login:** Middleware authenticates before function execution
- **SessionId:** Stored in RequestContext (AsyncLocal<T>) for thread-safe access
- **Logout:** Executed in finally block (always runs, even on error)

### Error Handling
All exceptions are normalized by `ExceptionHandlerMiddleware` to `BaseResponseDTO`:
- **400:** Validation errors, missing request body
- **404:** Location/instruction set not found
- **409:** Task already exists (skipped, not error)
- **500:** CAFM API failures, authentication errors

## Error Codes

| Error Code | Message | HTTP Status |
|------------|---------|-------------|
| CAFM_AUTHENT_0001 | Authentication to CAFM system failed | 401 |
| CAFM_AUTHENT_0002 | Authentication succeeded but no SessionId returned | 500 |
| CAFM_LOGOUT_0001 | Failed to logout from CAFM session | 500 |
| CAFM_LOCGET_0001 | Failed to get location details from CAFM | 500 |
| CAFM_LOCGET_0002 | Location not found for given property/unit code | 404 |
| CAFM_INSGET_0001 | Failed to get instruction sets from CAFM | 500 |
| CAFM_INSGET_0002 | Instruction set not found for given category | 404 |
| CAFM_TSKCHK_0001 | Failed to check existing tasks in CAFM | 500 |
| CAFM_TSKCRT_0001 | Failed to create breakdown task in CAFM | 500 |
| CAFM_TSKCRT_0002 | Breakdown task already exists in CAFM | 409 |
| CAFM_EVTCRT_0001 | Failed to create event/link task in CAFM | 500 |

## Performance Tracking

All SOAP operations are tracked with timing metrics:
- **ResponseHeaders.DSTimeBreakDown:** Comma-separated operation timings
- **Format:** `AUTHENTICATE:245,GET_LOCATIONS:1823,CREATE_BREAKDOWN_TASK:3421`
- **Aggregated Time:** Auto-calculated sum of all operations

## Testing

### Sample Request
```json
{
  "workOrders": [
    {
      "reporterName": "John Doe",
      "reporterEmail": "john.doe@example.com",
      "reporterPhoneNumber": "+971501234567",
      "description": "AC not working in unit A-101",
      "serviceRequestNumber": "EQ-2024-001234",
      "propertyName": "Tower A",
      "unitCode": "A-101",
      "categoryName": "HVAC",
      "subCategory": "Air Conditioning",
      "technician": "Tech-001",
      "sourceOrgId": "ORG-001",
      "ticketDetails": {
        "status": "Open",
        "subStatus": "Pending Assignment",
        "priority": "High",
        "scheduledDate": "2024-01-26",
        "scheduledTimeStart": "09:00",
        "scheduledTimeEnd": "12:00"
      }
    }
  ]
}
```

### Expected Response
```json
{
  "message": "Work order created successfully in CAFM.",
  "errorCode": null,
  "data": {
    "results": [
      {
        "serviceRequestNumber": "EQ-2024-001234",
        "status": "Success",
        "taskId": "CAFM-TASK-12345",
        "locationId": "LOC-001",
        "instructionSetId": "INS-001",
        "eventId": "EVT-001",
        "message": "Work order created successfully",
        "alreadyExists": false
      }
    ],
    "totalProcessed": 1,
    "successCount": 1,
    "failureCount": 0,
    "skippedCount": 0
  }
}
```

## Middleware

### ExecutionTimingMiddleware
- **Order:** 1 (FIRST)
- **Purpose:** Tracks total execution time and initializes timing breakdown

### ExceptionHandlerMiddleware
- **Order:** 2 (SECOND)
- **Purpose:** Catches all exceptions and normalizes to BaseResponseDTO

### CustomAuthenticationMiddleware
- **Order:** 3 (THIRD)
- **Purpose:** Handles CAFM session lifecycle (login/logout)
- **Pattern:** Login → Execute Function → Logout (finally block)
- **Storage:** RequestContext (AsyncLocal<T>) for thread-safe SessionId

## SOAP Operations

### Authentication
- **Operation:** Authenticate
- **Endpoint:** /services/evolution/04/09
- **SOAPAction:** http://www.fsi.co.uk/services/evolution/04/09/Authenticate
- **Request:** Username, Password
- **Response:** SessionId

### Get Locations
- **Operation:** GetLocationsByDto
- **Purpose:** Retrieve location ID from property/unit code
- **Request:** SessionId, PropertyName, UnitCode
- **Response:** LocationId, LocationName

### Get Instruction Sets
- **Operation:** GetInstructionSetsByDto
- **Purpose:** Retrieve instruction set ID from category/subcategory
- **Request:** SessionId, CategoryName, SubCategory
- **Response:** InstructionSetId, InstructionSetName

### Check Task Exists
- **Operation:** GetBreakdownTasksByDto
- **Purpose:** Check if breakdown task already exists
- **Request:** SessionId, ServiceRequestNumber
- **Response:** TaskId (if exists), TaskStatus

### Create Breakdown Task
- **Operation:** CreateBreakdownTask
- **Purpose:** Create new breakdown task/work order
- **Request:** SessionId, LocationId, InstructionSetId, Description, ServiceRequestNumber, Reporter details, Ticket details
- **Response:** TaskId, Success

### Create Event/Link Task
- **Operation:** CreateEvent
- **Purpose:** Create event and link to breakdown task
- **Request:** SessionId, TaskId, EventType, Description
- **Response:** EventId, Success

### Logout
- **Operation:** Logout
- **Purpose:** Terminate CAFM session
- **Request:** SessionId
- **Response:** Success

## Dependencies

### Framework References
- **Core Framework:** `/Framework/Core/Core/Core.csproj`
  - CustomHTTPClient, ExecutionTimingMiddleware, ExceptionHandlerMiddleware
  - LoggerExtensions, HttpResponseSnapshot, BaseResponseDTO
  - IRequestSysDTO, IDownStreamRequestDTO, IAtomicHandler, IBaseHandler
  
- **Cache Framework:** `/Framework/Cache/Cache.csproj`
  - Redis caching support (optional)

### NuGet Packages
- Microsoft.Azure.Functions.Worker (1.23.0)
- Microsoft.Azure.Functions.Worker.Extensions.Http (3.2.0)
- Microsoft.ApplicationInsights.WorkerService (2.22.0)
- Polly (8.5.0)
- Azure.Identity (1.13.1)
- Azure.Security.KeyVault.Secrets (4.7.0)

## Deployment

### Azure Function App Settings
Configure the following application settings in Azure Portal:

```
ASPNETCORE_ENVIRONMENT=prod
AppConfigs__CAFMBaseUrl=https://prodcafm.agfacilities.com
AppConfigs__CAFMAuthEndpoint=/services/evolution/04/09
AppConfigs__CAFMUsername=<from-keyvault>
AppConfigs__CAFMPassword=<from-keyvault>
AppConfigs__TimeoutSeconds=50
AppConfigs__RetryCount=0
AppConfigs__ProjectNamespace=CAFMSystemLayer
APPLICATIONINSIGHTS_CONNECTION_STRING=<connection-string>
```

### CI/CD Pipeline
The `.github/workflows/dotnet-ci.yml` handles:
- Build and test
- Environment-specific configuration replacement
- Deployment to Azure Function App

## Troubleshooting

### Authentication Failures
- **Error:** CAFM_AUTHENT_0001
- **Check:** Verify CAFMUsername and CAFMPassword in configuration
- **Check:** Ensure CAFMBaseUrl is accessible
- **Check:** Verify network connectivity to CAFM system

### Location Not Found
- **Error:** CAFM_LOCGET_0002
- **Check:** Verify PropertyName and UnitCode are valid in CAFM
- **Check:** Ensure location exists in CAFM system
- **Solution:** Create location in CAFM or correct property/unit codes

### Instruction Set Not Found
- **Error:** CAFM_INSGET_0002
- **Check:** Verify CategoryName and SubCategory are valid in CAFM
- **Check:** Ensure instruction set exists for category/subcategory
- **Solution:** Create instruction set in CAFM or correct category names

### Task Already Exists
- **Status:** Skipped (not error)
- **Behavior:** Returns existing TaskId, increments skippedCount
- **Message:** "Task already exists in CAFM"

## Architecture Decisions

### Why 1 Azure Function (Not 7)?
Following System Layer rules and Function Exposure Decision Table:
- **Same SOR:** All operations are CAFM system (same SOR)
- **Check-Before-Create:** Simple if/else pattern (if task exists skip, if not create)
- **Internal Lookups:** GetLocations and GetInstructionSets are field extraction for CreateTask
- **Handler Orchestration:** CreateWorkOrderHandler orchestrates all atomic operations internally
- **Result:** 1 Function with 5 Atomic Handlers (plus 2 auth handlers for middleware)

### Why Session-Based Middleware?
- **Lifecycle:** Login → Execute → Logout pattern
- **Efficiency:** Single login per request (not per operation)
- **Reliability:** Logout in finally block (always executes)
- **Thread-Safety:** RequestContext with AsyncLocal<T> storage

### Why SOAP?
- **CAFM System:** FSI Evolution uses SOAP/XML APIs
- **Templates:** SOAP envelopes stored as embedded resources
- **CustomSoapClient:** Wraps CustomHTTPClient with SOAP-specific logic
- **Performance:** Timing tracked for each SOAP operation

## Support

For issues or questions:
1. Check error codes in Constants/ErrorConstants.cs
2. Review logs in Application Insights
3. Verify configuration in appsettings.{env}.json
4. Contact: Facilities Integration Team

---

**Version:** 1.0  
**Last Updated:** 2025-01-26  
**Maintained By:** System Integration Team
