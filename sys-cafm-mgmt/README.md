# sys-cafm-mgmt - CAFM System Layer API

System Layer API for CAFM (Computer-Aided Facility Management) integration with FSI Evolution platform.

## Overview

This System Layer API provides work order management capabilities for the CAFM system. It abstracts the FSI CAFM SOAP web services and exposes RESTful HTTP endpoints for Process Layer consumption.

## API Endpoints

### POST /api/workorders
Creates a new work order in the CAFM system.

**Request Body:**
```json
{
  "workOrder": [
    {
      "reporterName": "John Doe",
      "reporterEmail": "john.doe@example.com",
      "reporterPhoneNumber": "+971501234567",
      "description": "AC not working in the conference room",
      "serviceRequestNumber": "SR-2025-001",
      "propertyName": "Building A",
      "unitCode": "BLD-A-101",
      "categoryName": "HVAC",
      "subCategory": "Air Conditioning",
      "technician": "",
      "sourceOrgId": "ORG001",
      "ticketDetails": {
        "status": "Open",
        "subStatus": "",
        "priority": "High",
        "scheduledDate": "2025-01-25",
        "scheduledTimeStart": "09:00:00",
        "scheduledTimeEnd": "17:00:00",
        "recurrence": "N",
        "oldCAFMSRnumber": "",
        "raisedDateUtc": "2025-01-22T10:00:00Z"
      }
    }
  ]
}
```

**Response:**
```json
{
  "message": "Work order created successfully",
  "errorCode": "",
  "data": {
    "workOrderId": "12345",
    "taskNumber": "BDET-2025-00001",
    "sourceServiceRequestNumber": "SR-2025-001",
    "status": "Created",
    "message": "Work order created successfully"
  },
  "errorDetails": null,
  "isDownStreamError": false,
  "isPartialSuccess": false
}
```

## API Call Sequence

```
┌─────────────────────────────────────────────────────────────────────────────────┐
│                        Create Work Order - API Sequence                         │
└─────────────────────────────────────────────────────────────────────────────────┘

  Client                Azure Function               FSI CAFM (SOAP)
    │                        │                             │
    │  POST /api/workorders  │                             │
    │───────────────────────>│                             │
    │                        │                             │
    │                        │  1. Authenticate            │
    │                        │  ─────────────────────────> │
    │                        │     (Login SOAP)            │
    │                        │  <───────────────────────── │
    │                        │     SessionId               │
    │                        │                             │
    │                        │  2a. GetLocationsByDto      │
    │                        │  ─────────────────────────> │
    │                        │     (UnitCode/BarCode)      │
    │                        │  <───────────────────────── │
    │                        │     BuildingId, LocationId  │
    │                        │                             │
    │                        │  2b. GetInstructionSetsByDto│ (parallel)
    │                        │  ─────────────────────────> │
    │                        │     (SubCategory)           │
    │                        │  <───────────────────────── │
    │                        │     CategoryId, DisciplineId│
    │                        │     PriorityId, InstructionId
    │                        │                             │
    │                        │  3. CreateBreakdownTask     │
    │                        │  ─────────────────────────> │
    │                        │     (All mapped fields)     │
    │                        │  <───────────────────────── │
    │                        │     TaskId, TaskNumber      │
    │                        │                             │
    │                        │  4. LogOut (best-effort)    │
    │                        │  ─────────────────────────> │
    │                        │     (SessionId)             │
    │                        │  <───────────────────────── │
    │                        │                             │
    │  Response (JSON)       │                             │
    │<───────────────────────│                             │
    │                        │                             │
```

## FSI CAFM SOAP Operations

| Operation | SOAP Action | Description |
|-----------|-------------|-------------|
| Authenticate | `IBreakdownAdmin/Authenticate` | Login to FSI and get session token |
| LogOut | `IBreakdownAdmin/LogOut` | End FSI session |
| GetLocationsByDto | `ILocation/GetLocationsByDto` | Get location details by barcode |
| GetInstructionSetsByDto | `IBreakdownAdmin/GetInstructionSetsByDto` | Get instruction set by subcategory |
| CreateBreakdownTask | `IBreakdownAdmin/CreateBreakdownTask` | Create a breakdown task (work order) |
| GetBreakdownTasksByDto | `IBreakdownAdmin/GetBreakdownTasksByDto` | Get task details by task number |

## Configuration

### Required Configuration (appsettings.{env}.json)

```json
{
  "AppConfigs": {
    "Fsi": {
      "BaseUrl": "https://cafm.domain.com",
      "Username": "<from Azure Key Vault>",
      "Password": "<from Azure Key Vault>",
      "ContractId": "<environment-specific contract ID>",
      "CallerSourceId": "EQ+"
    }
  }
}
```

### Secrets (Azure Key Vault)
The following secrets should be stored in Azure Key Vault:
- `FSI-Username` - FSI CAFM login username
- `FSI-Password` - FSI CAFM login password

## Project Structure

```
sys-cafm-mgmt/
├── Functions/                          # Azure Function HTTP triggers
│   └── CreateWorkOrderFunction.cs
├── Abstractions/                       # Interface definitions
│   └── IWorkOrderMgmt.cs
├── Implementations/
│   └── FSI/                           # FSI CAFM vendor implementation
│       ├── Services/
│       │   └── WorkOrderService.cs    # Service abstraction
│       ├── Handlers/
│       │   └── CreateWorkOrderHandler.cs  # Orchestration handler
│       └── AtomicHandlers/            # Single SOAP operation handlers
│           ├── AuthenticateAtomicHandler.cs
│           ├── LogoutAtomicHandler.cs
│           ├── GetLocationsByDtoAtomicHandler.cs
│           ├── GetInstructionSetsByDtoAtomicHandler.cs
│           ├── CreateBreakdownTaskAtomicHandler.cs
│           └── GetBreakdownTasksByDtoAtomicHandler.cs
├── DTOs/
│   ├── API/                           # API request/response DTOs
│   │   ├── CreateWorkOrderRequestDTO.cs
│   │   └── CreateWorkOrderResponseDTO.cs
│   └── Downstream/                    # FSI SOAP DTOs
│       ├── FsiAuthenticateRequestDTO.cs
│       ├── FsiLogoutRequestDTO.cs
│       ├── FsiGetLocationsByDtoRequestDTO.cs
│       ├── FsiGetInstructionSetsByDtoRequestDTO.cs
│       ├── FsiCreateBreakdownTaskRequestDTO.cs
│       └── FsiGetBreakdownTasksByDtoRequestDTO.cs
├── ConfigModels/
│   └── AppConfigs.cs
├── appsettings.json                   # Base configuration
├── appsettings.{env}.json             # Environment-specific configs
├── Program.cs                         # Host configuration & DI
└── host.json                          # Azure Functions host config
```

## Architecture Layers

1. **Azure Functions** - HTTP entry points exposing System Layer operations
2. **Services** - Abstraction boundaries that delegate to handlers
3. **Handlers** - Orchestrate multiple atomic handlers for same-SOR operations
4. **Atomic Handlers** - Single external SOAP call per handler

## Running Locally

1. Ensure you have .NET 8 SDK installed
2. Configure `local.settings.json` with development settings
3. Set up Azurite for local storage emulation
4. Run:
   ```bash
   cd sys-cafm-mgmt
   dotnet restore
   dotnet build
   func start
   ```

## Dependencies

- **Core Framework** - Shared framework from `/Framework/Core/`
- **Microsoft.Azure.Functions.Worker** - Azure Functions isolated worker
- **Polly** - Resilience and transient fault handling

## Error Handling

All exceptions are handled by `ExceptionHandlerMiddleware` which normalizes errors into `BaseResponseDTO` format:
- `DownStreamApiFailureException` - FSI CAFM API failures
- `RequestValidationFailureException` - Input validation errors
- `NoRequestBodyException` - Missing request body
- Other exceptions are wrapped as internal errors

## Data Mapping (EQ+ to CAFM)

| EQ+ Field | CAFM Field | Source |
|-----------|------------|--------|
| reporterName | ReporterName | Direct mapping |
| reporterEmail | BDET_EMAIL | Direct mapping |
| reporterPhoneNumber | Phone | Direct mapping |
| serviceRequestNumber | CallId | Direct mapping |
| description | LongDescription | Direct mapping |
| unitCode | BarCode → BuildingId, LocationId | Via GetLocationsByDto |
| subCategory | IN_DESCRIPTION → CategoryId, DisciplineId, PriorityId, InstructionId | Via GetInstructionSetsByDto |
| scheduledDate + scheduledTimeStart | ScheduledDateUtc | Formatted |
| raisedDateUtc | RaisedDateUtc | Formatted |
| - | ContractId | From config |
| - | BDET_CALLER_SOURCE_ID | From config |
