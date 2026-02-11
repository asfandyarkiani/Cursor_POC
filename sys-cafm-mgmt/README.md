# CAFM System Layer - FSI Evolution Integration

## Overview

This System Layer provides integration with the CAFM (FSI Evolution) system for facilities management operations. It exposes Azure Functions that Process Layer can call to interact with CAFM's SOAP-based APIs.

**System of Record:** CAFM (FSI Evolution)  
**Integration Type:** SOAP/XML  
**Authentication:** Session-based (Login/Logout managed by middleware)  
**Vendor:** FSI

---

## Architecture

### API-Led Architecture Layers

This project implements the **System Layer** which:
- Unlocks data from CAFM (FSI Evolution) system
- Insulates consumers from SOAP/XML complexity
- Provides RESTful Azure Functions that Process Layer can call
- Manages session-based authentication via middleware

### Component Flow

```
Azure Function (HTTP Entry Point)
    ↓
Service (ICAFMMgmt → CAFMMgmtService)
    ↓
Handler (Orchestrates atomic operations)
    ↓
Atomic Handler (Single SOAP API call)
    ↓
CustomSoapClient (HTTP client with timing)
    ↓
CAFM FSI Evolution SOAP API
```

---

## API Sequence Diagram

```
Process Layer → System Layer Function
                      |
                      ├─→ [Middleware: CAFMAuthenticationMiddleware]
                      |    ├─→ AuthenticateCAFMAtomicHandler
                      |    |    └─→ SOAP: Authenticate (username, password)
                      |    |         └─→ Response: SessionId
                      |    |         └─→ Store in RequestContext.SessionId
                      |    |
                      |    └─→ Continue to Function execution...
                      |
                      ├─→ Function: GetLocationsByDto
                      |    └─→ Service: CAFMMgmtService.GetLocationsByDto
                      |         └─→ Handler: GetLocationsByDtoHandler
                      |              └─→ Atomic: GetLocationsByDtoAtomicHandler
                      |                   └─→ SOAP: GetLocationsByDto (SessionId, UnitCode)
                      |                        └─→ Response: BuildingId, LocationId
                      |
                      ├─→ Function: GetInstructionSetsByDto
                      |    └─→ Service: CAFMMgmtService.GetInstructionSetsByDto
                      |         └─→ Handler: GetInstructionSetsByDtoHandler
                      |              └─→ Atomic: GetInstructionSetsByDtoAtomicHandler
                      |                   └─→ SOAP: GetInstructionSetsByDto (SessionId, SubCategory)
                      |                        └─→ Response: CategoryId, DisciplineId, PriorityId, InstructionId
                      |
                      ├─→ Function: GetBreakdownTasksByDto
                      |    └─→ Service: CAFMMgmtService.GetBreakdownTasksByDto
                      |         └─→ Handler: GetBreakdownTasksByDtoHandler
                      |              └─→ Atomic: GetBreakdownTasksByDtoAtomicHandler
                      |                   └─→ SOAP: GetBreakdownTasksByDto (SessionId, ServiceRequestNumber)
                      |                        └─→ Response: CallId, TaskId (if exists)
                      |
                      ├─→ Function: CreateBreakdownTask
                      |    └─→ Service: CAFMMgmtService.CreateBreakdownTask
                      |         └─→ Handler: CreateBreakdownTaskHandler
                      |              └─→ Atomic: CreateBreakdownTaskAtomicHandler
                      |                   └─→ SOAP: CreateBreakdownTask (SessionId, all task details)
                      |                        └─→ Response: TaskId, TaskNumber
                      |
                      ├─→ Function: CreateEvent
                      |    └─→ Service: CAFMMgmtService.CreateEvent
                      |         └─→ Handler: CreateEventHandler
                      |              └─→ Atomic: CreateEventAtomicHandler
                      |                   └─→ SOAP: CreateEvent (SessionId, TaskId, Comments)
                      |                        └─→ Response: EventId
                      |
                      └─→ [Middleware: CAFMAuthenticationMiddleware - Finally Block]
                           └─→ LogoutCAFMAtomicHandler
                                └─→ SOAP: LogOut (SessionId)
                                └─→ Clear RequestContext.SessionId
```

---

## Azure Functions (Exposed APIs)

### 1. GetLocationsByDto
**Route:** `POST /api/cafm/locations`  
**Purpose:** Get location details by unit code/barcode  
**Request:**
```json
{
  "unitCode": "UNIT-001"
}
```
**Response:**
```json
{
  "message": "Location details retrieved successfully from CAFM system.",
  "errorCode": null,
  "data": {
    "buildingId": 123,
    "locationId": 456,
    "primaryKeyId": 789,
    "barCode": "UNIT-001",
    "locationName": "Building A - Floor 1",
    "areaId": 10,
    "areaName": "North Wing"
  }
}
```

### 2. GetInstructionSetsByDto
**Route:** `POST /api/cafm/instructionsets`  
**Purpose:** Get instruction set details by subcategory  
**Request:**
```json
{
  "subCategory": "Electrical Maintenance"
}
```
**Response:**
```json
{
  "message": "Instruction set details retrieved successfully from CAFM system.",
  "errorCode": null,
  "data": {
    "instructionId": 100,
    "categoryId": 5,
    "disciplineId": 8,
    "priorityId": 2,
    "description": "Electrical Maintenance"
  }
}
```

### 3. GetBreakdownTasksByDto
**Route:** `POST /api/cafm/breakdowntasks`  
**Purpose:** Get existing breakdown tasks by service request number  
**Request:**
```json
{
  "serviceRequestNumber": "SR-2025-001"
}
```
**Response:**
```json
{
  "message": "Breakdown task details retrieved successfully from CAFM system.",
  "errorCode": null,
  "data": {
    "callId": "SR-2025-001",
    "taskId": 5001,
    "primaryKeyId": 6001,
    "taskNumber": "TASK-001",
    "longDescription": "Repair electrical outlet",
    "tasks": [...]
  }
}
```

### 4. CreateBreakdownTask
**Route:** `POST /api/cafm/breakdowntask`  
**Purpose:** Create breakdown task in CAFM system  
**Request:**
```json
{
  "serviceRequestNumber": "SR-2025-001",
  "reporterName": "John Doe",
  "reporterEmail": "john.doe@example.com",
  "reporterPhoneNumber": "+971501234567",
  "description": "Electrical outlet not working",
  "buildingId": "123",
  "locationId": "456",
  "categoryId": "5",
  "disciplineId": "8",
  "priorityId": "2",
  "instructionId": "100",
  "contractId": "CONTRACT-001",
  "callerSourceId": "EQ+",
  "scheduledDateUtc": "2025-02-25T11:05:41.0208713Z",
  "raisedDateUtc": "2025-02-24T10:30:00.0208713Z"
}
```
**Response:**
```json
{
  "message": "Breakdown task created successfully in CAFM system.",
  "errorCode": null,
  "data": {
    "taskId": 5001,
    "primaryKeyId": 6001,
    "filterQueryId": 7001,
    "taskNumber": "TASK-001",
    "callId": "SR-2025-001"
  }
}
```

### 5. CreateEvent
**Route:** `POST /api/cafm/event`  
**Purpose:** Create event and link to task (for recurring tasks)  
**Request:**
```json
{
  "taskId": "5001",
  "comments": "Recurring maintenance task"
}
```
**Response:**
```json
{
  "message": "Event created and linked successfully in CAFM system.",
  "errorCode": null,
  "data": {
    "eventId": 8001,
    "primaryKeyId": 9001,
    "eventNumber": "EVT-001"
  }
}
```

---

## Authentication

### Session-Based Authentication (Middleware)

This System Layer uses **session-based authentication** managed by `CAFMAuthenticationMiddleware`:

1. **Before Function Execution:**
   - Middleware calls `AuthenticateCAFMAtomicHandler`
   - Sends SOAP Authenticate request with username/password
   - Extracts SessionId from response
   - Stores SessionId in `RequestContext` (AsyncLocal storage)

2. **During Function Execution:**
   - All Handlers retrieve SessionId from `RequestContext.GetSessionId()`
   - SessionId is included in all SOAP requests

3. **After Function Execution (Finally Block):**
   - Middleware calls `LogoutCAFMAtomicHandler`
   - Sends SOAP LogOut request with SessionId
   - Clears `RequestContext`

**Benefits:**
- No manual login/logout in each function
- Guaranteed logout even on exceptions
- Thread-safe session management via AsyncLocal

---

## Configuration

### Required Configuration (appsettings.{env}.json)

```json
{
  "AppConfigs": {
    "BaseApiUrl": "https://{env}-cafm.fsi.co.uk/api",
    "LoginUrl": "https://{env}-cafm.fsi.co.uk/api/services/evolution/04/09",
    "LogoutUrl": "https://{env}-cafm.fsi.co.uk/api/services/evolution/04/09",
    "GetLocationsByDtoUrl": "https://{env}-cafm.fsi.co.uk/api/services/evolution/04/09",
    "GetInstructionSetsByDtoUrl": "https://{env}-cafm.fsi.co.uk/api/services/evolution/04/09",
    "GetBreakdownTasksByDtoUrl": "https://{env}-cafm.fsi.co.uk/api/services/evolution/04/09",
    "CreateBreakdownTaskUrl": "https://{env}-cafm.fsi.co.uk/api/services/evolution/04/09",
    "CreateEventUrl": "https://{env}-cafm.fsi.co.uk/api/services/evolution/04/09",
    "LoginSoapAction": "http://www.fsi.co.uk/services/evolution/04/09/IEvolutionService/Authenticate",
    "LogoutSoapAction": "http://www.fsi.co.uk/services/evolution/04/09/IEvolutionService/LogOut",
    "GetLocationsByDtoSoapAction": "http://www.fsi.co.uk/services/evolution/04/09/IEvolutionService/GetLocationsByDto",
    "GetInstructionSetsByDtoSoapAction": "http://www.fsi.co.uk/services/evolution/04/09/IEvolutionService/GetInstructionSetsByDto",
    "GetBreakdownTasksByDtoSoapAction": "http://www.fsi.co.uk/services/evolution/04/09/IEvolutionService/GetBreakdownTasksByDto",
    "CreateBreakdownTaskSoapAction": "http://www.fsi.co.uk/services/evolution/04/09/IEvolutionService/CreateBreakdownTask",
    "CreateEventSoapAction": "http://www.fsi.co.uk/services/evolution/04/09/IEvolutionService/CreateEvent",
    "Username": "TODO_USERNAME",
    "Password": "",
    "TimeoutSeconds": 50,
    "RetryCount": 0,
    "ProjectNamespace": "CAFMSystem"
  },
  "KeyVault": {
    "Url": "https://{env}-keyvault.vault.azure.net/",
    "Secrets": {
      "Password": "CAFMPassword",
      "Username": "CAFMUsername"
    }
  },
  "HttpClientPolicy": {
    "RetryCount": 0,
    "TimeoutSeconds": 60
  }
}
```

### Azure KeyVault Secrets

Store sensitive credentials in Azure KeyVault:
- **CAFMPassword** - CAFM system password
- **CAFMUsername** - CAFM system username (optional, can use appsettings)

---

## Local Development Setup

### Prerequisites
- .NET 8 SDK
- Azure Functions Core Tools v4
- Azure Storage Emulator or Azurite
- Access to CAFM FSI Evolution system (dev/qa environment)

### Steps

1. **Clone Repository**
   ```bash
   git clone <repository-url>
   cd sys-cafm-mgmt
   ```

2. **Restore Dependencies**
   ```bash
   dotnet restore
   ```

3. **Configure Local Settings**
   
   Create `local.settings.json`:
   ```json
   {
     "IsEncrypted": false,
     "Values": {
       "AzureWebJobsStorage": "UseDevelopmentStorage=true",
       "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
       "ENVIRONMENT": "dev",
       "APPLICATIONINSIGHTS_CONNECTION_STRING": ""
     }
   }
   ```

4. **Update Configuration**
   
   Edit `appsettings.dev.json`:
   - Set correct CAFM API URLs (BaseApiUrl, LoginUrl, etc.)
   - Set SOAP Action URLs
   - Set Username (or leave empty to use KeyVault)
   - Password should be retrieved from KeyVault

5. **Run Locally**
   ```bash
   func start
   ```

6. **Test Endpoints**
   ```bash
   # Get Locations
   curl -X POST http://localhost:7071/api/cafm/locations \
     -H "Content-Type: application/json" \
     -d '{"unitCode": "UNIT-001"}'
   
   # Get Instruction Sets
   curl -X POST http://localhost:7071/api/cafm/instructionsets \
     -H "Content-Type: application/json" \
     -d '{"subCategory": "Electrical Maintenance"}'
   
   # Get Breakdown Tasks
   curl -X POST http://localhost:7071/api/cafm/breakdowntasks \
     -H "Content-Type: application/json" \
     -d '{"serviceRequestNumber": "SR-2025-001"}'
   
   # Create Breakdown Task
   curl -X POST http://localhost:7071/api/cafm/breakdowntask \
     -H "Content-Type: application/json" \
     -d '{
       "serviceRequestNumber": "SR-2025-001",
       "reporterName": "John Doe",
       "reporterEmail": "john.doe@example.com",
       "description": "Electrical outlet not working",
       "buildingId": "123",
       "locationId": "456",
       "categoryId": "5",
       "disciplineId": "8",
       "priorityId": "2",
       "instructionId": "100",
       "raisedDateUtc": "2025-02-24T10:30:00.0208713Z"
     }'
   
   # Create Event
   curl -X POST http://localhost:7071/api/cafm/event \
     -H "Content-Type: application/json" \
     -d '{
       "taskId": "5001",
       "comments": "Recurring maintenance task"
     }'
   ```

---

## Project Structure

```
sys-cafm-mgmt/
├── Abstractions/
│   └── ICAFMMgmt.cs                    # Service interface
├── Implementations/FSI/
│   ├── Services/
│   │   └── CAFMMgmtService.cs          # Service implementation
│   ├── Handlers/
│   │   ├── GetLocationsByDtoHandler.cs
│   │   ├── GetInstructionSetsByDtoHandler.cs
│   │   ├── GetBreakdownTasksByDtoHandler.cs
│   │   ├── CreateBreakdownTaskHandler.cs
│   │   └── CreateEventHandler.cs
│   └── AtomicHandlers/
│       ├── AuthenticateCAFMAtomicHandler.cs      # Middleware internal
│       ├── LogoutCAFMAtomicHandler.cs            # Middleware internal
│       ├── GetLocationsByDtoAtomicHandler.cs
│       ├── GetInstructionSetsByDtoAtomicHandler.cs
│       ├── GetBreakdownTasksByDtoAtomicHandler.cs
│       ├── CreateBreakdownTaskAtomicHandler.cs
│       └── CreateEventAtomicHandler.cs
├── DTO/
│   ├── HandlerDTOs/                    # API request/response DTOs
│   │   ├── GetLocationsByDtoDTO/
│   │   ├── GetInstructionSetsByDtoDTO/
│   │   ├── GetBreakdownTasksByDtoDTO/
│   │   ├── CreateBreakdownTaskDTO/
│   │   └── CreateEventDTO/
│   ├── AtomicHandlerDTOs/              # Atomic handler DTOs
│   │   ├── AuthenticateCAFMHandlerReqDTO.cs
│   │   ├── LogoutCAFMHandlerReqDTO.cs
│   │   ├── GetLocationsByDtoHandlerReqDTO.cs
│   │   ├── GetInstructionSetsByDtoHandlerReqDTO.cs
│   │   ├── GetBreakdownTasksByDtoHandlerReqDTO.cs
│   │   ├── CreateBreakdownTaskHandlerReqDTO.cs
│   │   └── CreateEventHandlerReqDTO.cs
│   └── DownstreamDTOs/                 # CAFM API response DTOs
│       ├── AuthenticateCAFMApiResDTO.cs
│       ├── GetLocationsByDtoApiResDTO.cs
│       ├── GetInstructionSetsByDtoApiResDTO.cs
│       ├── GetBreakdownTasksByDtoApiResDTO.cs
│       ├── CreateBreakdownTaskApiResDTO.cs
│       └── CreateEventApiResDTO.cs
├── Functions/                          # Azure Functions
│   ├── GetLocationsByDtoAPI.cs
│   ├── GetInstructionSetsByDtoAPI.cs
│   ├── GetBreakdownTasksByDtoAPI.cs
│   ├── CreateBreakdownTaskAPI.cs
│   └── CreateEventAPI.cs
├── ConfigModels/
│   ├── AppConfigs.cs
│   └── KeyVaultConfigs.cs
├── Constants/
│   ├── ErrorConstants.cs
│   ├── InfoConstants.cs
│   └── OperationNames.cs
├── Helper/
│   ├── CustomSoapClient.cs             # SOAP HTTP client
│   ├── SOAPHelper.cs                   # SOAP utilities
│   └── RequestContext.cs               # AsyncLocal session storage
├── Middleware/
│   └── CAFMAuthenticationMiddleware.cs # Session lifecycle management
├── Attributes/
│   └── CAFMAuthenticationAttribute.cs  # Function attribute
├── SoapEnvelopes/                      # SOAP XML templates
│   ├── Authenticate.xml
│   ├── Logout.xml
│   ├── GetLocationsByDto.xml
│   ├── GetInstructionSetsByDto.xml
│   ├── GetBreakdownTasksByDto.xml
│   ├── CreateBreakdownTask.xml
│   └── CreateEvent.xml
├── Program.cs                          # DI & middleware registration
├── host.json
├── appsettings.json
├── appsettings.dev.json
├── appsettings.qa.json
└── appsettings.prod.json
```

---

## Key Design Decisions

### 1. Authentication Approach
**Decision:** Session-based authentication with middleware  
**Rationale:**
- CAFM requires Login → Operations → Logout sequence
- Middleware ensures login before every function execution
- Finally block guarantees logout even on exceptions
- No manual authentication code in functions

### 2. Function Exposure
**Functions Created:** 5 Azure Functions  
**Rationale:**
- Each function represents an operation Process Layer can call independently
- Login/Logout are NOT functions (middleware-managed, internal Atomic Handlers)
- GetLocationsByDto, GetInstructionSetsByDto, GetBreakdownTasksByDto are lookup operations
- CreateBreakdownTask is the main create operation
- CreateEvent is conditional operation for recurring tasks

### 3. Same-SOR Orchestration
**Decision:** All operations are separate functions (NO Handler orchestration)  
**Rationale:**
- Process Layer orchestrates the complete workflow:
  - Calls GetLocationsByDto to get location IDs
  - Calls GetInstructionSetsByDto to get instruction set IDs
  - Calls GetBreakdownTasksByDto to check if task exists
  - If task doesn't exist, calls CreateBreakdownTask with enriched data
  - If recurring, calls CreateEvent to link event to task
- System Layer provides atomic operations as "Lego blocks"
- Business logic (check-before-create, recurrence handling) belongs in Process Layer

### 4. Data Transformation
**Decision:** Process Layer handles date formatting and data enrichment  
**Rationale:**
- Boomi process has JavaScript functions for date formatting (ScheduledDateUtc, RaisedDateUtc)
- This business logic belongs in Process Layer
- System Layer accepts pre-formatted dates as strings

---

## Error Handling

### Exception Types

| Exception | HTTP Status | When Thrown |
|---|---|---|
| NoRequestBodyException | 400 | Request body is null/empty |
| RequestValidationFailureException | 400 | DTO validation fails |
| NotFoundException | 404 | Location/InstructionSet not found |
| DownStreamApiFailureException | Varies | CAFM API call fails |
| NoResponseBodyException | 500 | API returns success but no data |

### Error Response Format

```json
{
  "message": "Failed to get location details from CAFM system.",
  "errorCode": "SYS_LOCGET_0001",
  "data": null,
  "errorDetails": {
    "errorCode": "SYS_LOCGET_0001",
    "message": "Failed to get location details from CAFM system.",
    "details": [
      "GetLocationsByDto API failed. Status: 500. Response: ..."
    ],
    "stepName": "GetLocationsByDtoHandler.cs / HandleAsync"
  }
}
```

---

## Dependencies

### Framework Libraries
- **Core Framework:** `/Framework/Core/Core/Core.csproj`
  - CustomHTTPClient, ExecutionTimingMiddleware, ExceptionHandlerMiddleware
  - BaseResponseDTO, HttpResponseSnapshot
  - Logger extensions, HTTP extensions
- **Cache Framework:** `/Framework/Cache/Cache.csproj`
  - Redis caching support (optional)

### NuGet Packages
- Microsoft.Azure.Functions.Worker (1.21.0)
- Microsoft.Azure.Functions.Worker.Extensions.Http (3.1.0)
- Microsoft.ApplicationInsights.WorkerService (2.22.0)
- Polly (8.3.1) - Retry/timeout policies
- Azure.Identity (1.11.0) - KeyVault authentication
- Azure.Security.KeyVault.Secrets (4.6.0)
- StackExchange.Redis (2.7.33) - Redis caching

---

## Deployment

### Azure Resources Required
- Azure Function App (.NET 8, Isolated Worker)
- Azure Application Insights
- Azure Key Vault (for credentials)
- Azure Redis Cache (optional, for caching)

### Environment Variables
- `ENVIRONMENT` or `ASPNETCORE_ENVIRONMENT` - Environment name (dev/qa/prod)
- `APPLICATIONINSIGHTS_CONNECTION_STRING` - Application Insights connection

### CI/CD Pipeline
- Build: `dotnet build`
- Test: `dotnet test` (if tests exist)
- Publish: `dotnet publish -c Release -o ./output`
- Deploy: Azure Functions deployment

---

## Monitoring

### Application Insights
- Request/response tracking
- Performance metrics via `ExecutionTimingMiddleware`
- Downstream API timing via `ResponseHeaders.DSTimeBreakDown`
- Exception tracking via `ExceptionHandlerMiddleware`

### Logging
- All operations log start/completion
- Errors logged with full context
- SOAP envelope creation logged at Debug level
- Uses Core Framework logger extensions (`.Info()`, `.Error()`, `.Warn()`, `.Debug()`)

---

## Process Layer Integration

### Typical Process Layer Workflow

```csharp
// Process Layer orchestrates System Layer calls

// 1. Get location details
var locationRes = await _cafmSystemAbstraction.GetLocationsByDto(new GetLocationsByDtoReqDTO { UnitCode = "UNIT-001" });
var locationData = locationRes.Data as GetLocationsByDtoResDTO;

// 2. Get instruction set details
var instructionRes = await _cafmSystemAbstraction.GetInstructionSetsByDto(new GetInstructionSetsByDtoReqDTO { SubCategory = "Electrical" });
var instructionData = instructionRes.Data as GetInstructionSetsByDtoResDTO;

// 3. Check if task already exists
var existingTaskRes = await _cafmSystemAbstraction.GetBreakdownTasksByDto(new GetBreakdownTasksByDtoReqDTO { ServiceRequestNumber = "SR-001" });
var existingTask = existingTaskRes.Data as GetBreakdownTasksByDtoResDTO;

if (!string.IsNullOrEmpty(existingTask.CallId))
{
    // Task already exists, skip creation
    return new BaseResponseDTO("Task already exists", "SYS_TSKCRT_0003", existingTask);
}

// 4. Create breakdown task with enriched data
var createTaskReq = new CreateBreakdownTaskReqDTO
{
    ServiceRequestNumber = "SR-001",
    BuildingId = locationData.BuildingId.ToString(),
    LocationId = locationData.LocationId.ToString(),
    CategoryId = instructionData.CategoryId.ToString(),
    DisciplineId = instructionData.DisciplineId.ToString(),
    PriorityId = instructionData.PriorityId.ToString(),
    InstructionId = instructionData.InstructionId.ToString(),
    // ... other fields
};

var createTaskRes = await _cafmSystemAbstraction.CreateBreakdownTask(createTaskReq);
var taskData = createTaskRes.Data as CreateBreakdownTaskResDTO;

// 5. If recurring, create event
if (isRecurring)
{
    var createEventReq = new CreateEventReqDTO
    {
        TaskId = taskData.TaskId.ToString(),
        Comments = "Recurring maintenance task"
    };
    
    await _cafmSystemAbstraction.CreateEvent(createEventReq);
}
```

---

## Troubleshooting

### Common Issues

**Issue:** Authentication fails with "SessionId not found"  
**Solution:** Ensure `[CAFMAuthentication]` attribute is applied to function

**Issue:** SOAP envelope template not found  
**Solution:** Verify .csproj has `<EmbeddedResource Include="SoapEnvelopes\*.xml" />` and rebuild

**Issue:** CAFM API returns 500 error  
**Solution:** Check SOAP envelope format, verify all required fields are populated

**Issue:** Timeout errors  
**Solution:** Increase `TimeoutSeconds` in appsettings or `HttpClientPolicy:TimeoutSeconds`

**Issue:** Deserialization fails  
**Solution:** Verify SOAP response structure matches ApiResDTO classes, check XML namespaces

---

## TODO Items

### Configuration
- [ ] Update BaseApiUrl with actual CAFM environment URLs (dev/qa/prod)
- [ ] Update all operation URLs (LoginUrl, LogoutUrl, etc.)
- [ ] Verify SOAP Action URLs match CAFM API requirements
- [ ] Store CAFM credentials in Azure KeyVault
- [ ] Update KeyVault URL for each environment

### Testing
- [ ] Test authentication flow (login/logout)
- [ ] Test each function with sample data
- [ ] Verify SOAP envelope formats with CAFM team
- [ ] Test error scenarios (invalid credentials, not found, etc.)
- [ ] Load testing for concurrent requests

### Documentation
- [ ] Document CAFM API contract details
- [ ] Add Postman collection for testing
- [ ] Document field mappings (EQ+ → CAFM)
- [ ] Add troubleshooting guide for common CAFM errors

---

## Support

For issues or questions:
- Review CAFM FSI Evolution API documentation
- Check Application Insights for error details
- Review Boomi process analysis: `/workspace/PHASE1_BOOMI_ANALYSIS.md`
- Contact CAFM system administrator for API access issues

---

**Version:** 1.0  
**Last Updated:** 2025-01-23  
**Boomi Process:** Create Work Order from EQ+ to CAFM
