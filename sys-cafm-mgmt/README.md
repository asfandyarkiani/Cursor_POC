# CAFM System Layer API

## Overview

This System Layer API provides integration with the FSI Evolution CAFM (Computer-Aided Facility Management) system. It exposes work order creation operations for the Process Layer to consume.

**System of Record:** FSI Evolution CAFM  
**Authentication:** Session-based (SOAP)  
**Protocol:** SOAP/XML over HTTP  
**Architecture:** API-Led Architecture - System Layer

---

## API Operations

### CreateWorkOrder

**Endpoint:** `POST /api/workorder/create`

**Purpose:** Creates work orders in CAFM system from EQ+ service requests.

**Authentication:** Session-based (handled by middleware)

**Request Body:**
```json
{
  "workOrders": [
    {
      "reporterName": "John Doe",
      "reporterEmail": "john.doe@example.com",
      "reporterPhoneNumber": "+971501234567",
      "description": "Air conditioning not working",
      "serviceRequestNumber": "SR-2025-001",
      "propertyName": "Building A",
      "unitCode": "A-101",
      "categoryName": "HVAC",
      "subCategory": "AC Repair",
      "technician": "Tech-001",
      "sourceOrgId": "ORG-001",
      "ticketDetails": {
        "status": "Open",
        "subStatus": "Pending",
        "priority": "High",
        "scheduledDate": "2025-01-27",
        "scheduledTimeStart": "09:00",
        "scheduledTimeEnd": "17:00",
        "recurrence": "N",
        "oldCAFMSRnumber": "",
        "raisedDateUtc": "2025-01-26T10:00:00Z"
      }
    }
  ]
}
```

**Response:**
```json
{
  "message": "Work order created successfully in CAFM system.",
  "errorCode": null,
  "data": {
    "workOrders": [
      {
        "cafmSRNumber": "CAFM-12345",
        "sourceSRNumber": "SR-2025-001",
        "sourceOrgId": "ORG-001",
        "status": "Success",
        "message": "Work order created successfully in CAFM"
      }
    ]
  },
  "errorDetails": null,
  "isDownStreamError": false
}
```

---

## Architecture

### Component Flow

```
Process Layer
     ↓
CreateWorkOrderAPI (Azure Function)
     ↓
IWorkOrderMgmt (Interface)
     ↓
WorkOrderMgmtService (Service)
     ↓
CreateWorkOrderHandler (Handler)
     ↓
[Atomic Handlers] → CAFM SOAP API
     ├─→ GetLocationsByDtoAtomicHandler
     ├─→ GetInstructionSetsByDtoAtomicHandler
     ├─→ GetBreakdownTasksByDtoAtomicHandler (check-before-create)
     ├─→ CreateBreakdownTaskAtomicHandler
     └─→ CreateEventAtomicHandler (conditional - if recurrence = Y)
```

### Middleware Pipeline

```
Request
  ↓
ExecutionTimingMiddleware (tracks total time)
  ↓
ExceptionHandlerMiddleware (normalizes exceptions)
  ↓
CustomAuthenticationMiddleware (CAFM login/logout)
  ├─→ Login (AuthenticateAtomicHandler) → Get SessionId
  ├─→ Execute Function
  └─→ Logout (LogoutAtomicHandler) → Cleanup [finally block]
  ↓
Response
```

---

## Sequence Diagram

```
Process Layer                System Layer                                CAFM API
     |                            |                                          |
     |--CreateWorkOrder---------->|                                          |
     |                            |                                          |
     |                            |--[Middleware: Login]-------------------->|
     |                            |<-[SessionId]----------------------------|
     |                            |                                          |
     |                            |--For Each Work Order:                    |
     |                            |                                          |
     |                            |  1. GetBreakdownTasksByDto (check)----->|
     |                            |  <-[CallId or empty]---------------------|
     |                            |                                          |
     |                            |  IF CallId empty (task doesn't exist):   |
     |                            |     Skip creation, continue              |
     |                            |                                          |
     |                            |  IF CallId not empty (task exists):      |
     |                            |                                          |
     |                            |  2. GetLocationsByDto------------------->|
     |                            |  <-[BuildingId, LocationId]--------------|
     |                            |                                          |
     |                            |  3. GetInstructionSetsByDto------------->|
     |                            |  <-[CategoryId, DisciplineId, etc.]------|
     |                            |                                          |
     |                            |  4. CreateBreakdownTask----------------->|
     |                            |  <-[TaskId]------------------------------|
     |                            |                                          |
     |                            |  IF Recurrence = Y:                      |
     |                            |     5. CreateEvent---------------------->|
     |                            |     <-[EventId]--------------------------|
     |                            |                                          |
     |                            |--[Middleware: Logout]------------------->|
     |                            |<-[Success]-------------------------------|
     |                            |                                          |
     |<-[Results Array]-----------|                                          |
     |                            |                                          |
```

---

## Configuration

### Required Configuration (appsettings.json)

```json
{
  "AppConfigs": {
    "BaseUrl": "https://<environment>-cafm.fsi.co.uk",
    "LoginUrl": "https://<environment>-cafm.fsi.co.uk/services/evolution/04/09",
    "LogoutUrl": "https://<environment>-cafm.fsi.co.uk/services/evolution/04/09",
    "GetLocationsByDtoUrl": "https://<environment>-cafm.fsi.co.uk/services/evolution/04/09",
    "GetInstructionSetsByDtoUrl": "https://<environment>-cafm.fsi.co.uk/services/evolution/04/09",
    "CreateBreakdownTaskUrl": "https://<environment>-cafm.fsi.co.uk/services/evolution/04/09",
    "GetBreakdownTasksByDtoUrl": "https://<environment>-cafm.fsi.co.uk/services/evolution/04/09",
    "CreateEventUrl": "https://<environment>-cafm.fsi.co.uk/services/evolution/04/09",
    "LoginSoapAction": "http://www.fsi.co.uk/services/evolution/04/09/IEvolutionService/Authenticate",
    "LogoutSoapAction": "http://www.fsi.co.uk/services/evolution/04/09/IEvolutionService/LogOut",
    "GetLocationsByDtoSoapAction": "http://www.fsi.co.uk/services/evolution/04/09/IEvolutionService/GetLocationsByDto",
    "GetInstructionSetsByDtoSoapAction": "http://www.fsi.co.uk/services/evolution/04/09/IEvolutionService/GetInstructionSetsByDto",
    "CreateBreakdownTaskSoapAction": "http://www.fsi.co.uk/services/evolution/04/09/IEvolutionService/CreateBreakdownTask",
    "GetBreakdownTasksByDtoSoapAction": "http://www.fsi.co.uk/services/evolution/04/09/IEvolutionService/GetBreakdownTasksByDto",
    "CreateEventSoapAction": "http://www.fsi.co.uk/services/evolution/04/09/IEvolutionService/CreateEvent",
    "Username": "<cafm-username>",
    "Password": "<cafm-password>",
    "TimeoutSeconds": 50,
    "RetryCount": 0
  },
  "KeyVault": {
    "Url": "https://<keyvault-name>.vault.azure.net/",
    "Secrets": {
      "Password": "CAFMPassword",
      "Username": "CAFMUsername"
    }
  },
  "RedisCache": {
    "ConnectionString": "<redis-connection-string>",
    "InstanceName": "CAFMSystem:"
  }
}
```

### Environment Variables

- `ENVIRONMENT` or `ASPNETCORE_ENVIRONMENT`: Environment name (dev, qa, prod)
- `APPLICATIONINSIGHTS_CONNECTION_STRING`: Application Insights connection string

### Secrets (Azure Key Vault)

- `CAFMUsername`: CAFM system username
- `CAFMPassword`: CAFM system password

---

## Local Development

### Prerequisites

- .NET 8 SDK
- Azure Functions Core Tools v4
- Azure Storage Emulator or Azurite
- Redis (for caching)

### Setup

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd sys-cafm-mgmt
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Configure local settings**
   
   Create `local.settings.json`:
   ```json
   {
     "IsEncrypted": false,
     "Values": {
       "AzureWebJobsStorage": "UseDevelopmentStorage=true",
       "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
       "ENVIRONMENT": "dev",
       "APPLICATIONINSIGHTS_CONNECTION_STRING": "<your-app-insights-connection>"
     }
   }
   ```

4. **Update appsettings.dev.json**
   - Set CAFM endpoint URLs
   - Set CAFM credentials (or use KeyVault)
   - Set Redis connection string

5. **Run locally**
   ```bash
   func start
   ```

### Testing

**Test CreateWorkOrder API:**
```bash
curl -X POST http://localhost:7071/api/workorder/create \
  -H "Content-Type: application/json" \
  -d '{
    "workOrders": [{
      "serviceRequestNumber": "SR-TEST-001",
      "unitCode": "A-101",
      "subCategory": "AC Repair",
      "description": "Test work order",
      "reporterName": "Test User",
      "reporterEmail": "test@example.com",
      "reporterPhoneNumber": "+971501234567",
      "sourceOrgId": "ORG-001",
      "ticketDetails": {
        "raisedDateUtc": "2025-01-26T10:00:00Z",
        "status": "Open",
        "recurrence": "N"
      }
    }]
  }'
```

---

## Business Logic

### Check-Before-Create Pattern

The handler implements a check-before-create pattern:

1. **GetBreakdownTasksByDto** checks if task exists (by serviceRequestNumber)
2. **If CallId is empty:** Task doesn't exist → Skip creation
3. **If CallId has value:** Task exists → Proceed with creation

### Lookup Orchestration

The handler orchestrates internal lookups (same SOR):

1. **GetLocationsByDto:** Retrieves BuildingId and LocationId by UnitCode
2. **GetInstructionSetsByDto:** Retrieves CategoryId, DisciplineId, PriorityId, InstructionId by SubCategory
3. **CreateBreakdownTask:** Aggregates all lookup results and creates task

### Conditional Event Creation

If work order has `recurrence = "Y"`:
- **CreateEvent** is called after CreateBreakdownTask
- Links event to the created task using TaskId
- Event creation failure does NOT fail the work order (task already created)

---

## Error Handling

### Exception Types

| Exception | HTTP Status | When Thrown |
|---|---|---|
| `NoRequestBodyException` | 400 | Request body missing or empty |
| `RequestValidationFailureException` | 400 | Request validation fails |
| `NotFoundException` | 404 | Location or instruction set not found |
| `DownStreamApiFailureException` | Varies | CAFM API call fails |
| `BaseException` | 500 | SessionId not available |

### Error Response Format

```json
{
  "message": "Error message",
  "errorCode": "SYS_TSKCRT_0001",
  "data": null,
  "errorDetails": {
    "errorCode": "SYS_TSKCRT_0001",
    "message": "Failed to create breakdown task in CAFM system.",
    "details": ["Detailed error information"],
    "stepName": "CreateWorkOrderHandler.cs / HandleAsync / CreateBreakdownTask"
  },
  "isDownStreamError": true
}
```

---

## Performance

### Response Headers

- `SYSTotalTime`: Total execution time (milliseconds)
- `DSTimeBreakDown`: Breakdown of downstream operation times
  - Format: `AUTHENTICATE:245,GET_LOCATIONS_BY_DTO:1823,CREATE_BREAKDOWN_TASK:2100,LOGOUT:123`
- `DSAggregatedTime`: Sum of all downstream operation times

### Timeout Configuration

- **HTTP Client Timeout:** 50 seconds (hardcoded in CustomHTTPClient)
- **Polly Policy Timeout:** 60 seconds (configurable in appsettings.json)
- **Retry Count:** 0 (no retries by default - set in appsettings.json if needed)

---

## Dependencies

### Framework Libraries

- **Core Framework:** `/Framework/Core/Core/Core.csproj`
  - Base classes, interfaces, extensions, middleware
  - CustomHTTPClient, CustomRestClient
  - Exception handling, logging extensions
  
- **Cache Framework:** `/Framework/Cache/Cache.csproj`
  - Redis caching support
  - Caching interceptors and extensions

### NuGet Packages

- Microsoft.Azure.Functions.Worker (2.0.0)
- Microsoft.Azure.Functions.Worker.Extensions.Http (3.2.0)
- Microsoft.ApplicationInsights.WorkerService (2.22.0)
- Polly (8.6.1)
- Azure.Identity (1.14.2)
- Azure.Security.KeyVault.Secrets (4.8.0)
- StackExchange.Redis (2.8.58)

---

## TODOs

### Configuration TODOs

1. **ContractId:** Update `CreateBreakdownTaskHandlerReqDTO` to use actual ContractId from configuration
   - Current: Hardcoded as "TODO_CONTRACT_ID"
   - Action: Add ContractId to AppConfigs and retrieve from configuration

### Deployment TODOs

1. **Azure KeyVault:** Configure KeyVault secrets for CAFM credentials
   - CAFMUsername
   - CAFMPassword

2. **Redis Cache:** Configure Redis connection string for each environment

3. **Application Insights:** Configure Application Insights connection string

4. **CAFM Endpoints:** Verify and update CAFM endpoint URLs for each environment (dev, qa, prod)

5. **SOAPAction URLs:** Verify SOAPAction header values match CAFM WSDL

---

## Architecture Decisions

### Single Function Design

**Decision:** Create ONE Azure Function (CreateWorkOrderAPI) instead of multiple functions.

**Reasoning:**
- All operations target same SOR (CAFM FSI Evolution)
- GetLocationsByDto and GetInstructionSetsByDto are internal lookups (field extraction)
- GetBreakdownTasksByDto is internal check (existence verification)
- CreateEvent is conditional operation (only if recurrence = Y)
- Handler orchestrates internally with simple if/else and check-before-create patterns
- Per System Layer rules: Same SOR operations → Handler orchestrates internally

### Session-Based Authentication

**Decision:** Use middleware for authentication lifecycle.

**Reasoning:**
- CAFM requires session-based authentication (Login → SessionId → Operations → Logout)
- Middleware ensures login before function execution and logout in finally block
- Guarantees logout even on exception
- Cleaner code - no manual login/logout in each operation

### Check-Before-Create Pattern

**Decision:** Check task existence before creation.

**Reasoning:**
- Prevents duplicate task creation
- Boomi process implements this pattern (GetBreakdownTasksByDto → Decision → CreateBreakdownTask)
- If CallId is empty (task doesn't exist), skip creation
- If CallId has value (task exists), proceed with creation

---

## Monitoring

### Application Insights

- All operations logged with correlation IDs
- Performance metrics tracked (DSTimeBreakDown)
- Exceptions automatically captured
- Live metrics enabled (host.json)

### Logging Levels

- **Info:** Function entry/exit, major operation steps
- **Warn:** Recoverable errors (e.g., CreateEvent failure after task created)
- **Error:** Unrecoverable errors, exceptions

---

## Support

For issues or questions, contact the API Platform team.

**Repository:** sys-cafm-mgmt  
**System:** FSI Evolution CAFM  
**Domain:** Facilities Management  
**Layer:** System Layer (API-Led Architecture)
