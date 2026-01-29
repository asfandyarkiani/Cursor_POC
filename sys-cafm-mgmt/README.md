# sys-cafm-mgmt - CAFM System Layer API

System Layer API for CAFM (Computer-Aided Facility Management) operations. This service provides integration with the FSI Evolution CAFM system for work order management in the Facilities domain.

## Overview

This System Layer implements the "Create Work Order" functionality, enabling EQ+ system to create breakdown tasks in CAFM via SOAP API calls to FSI Evolution services.

## API Sequence Diagram

```
                    Create Work Order from EQ+ to CAFM
+--------+     +------------------+     +------------------+     +--------+
| Client |     | CreateWorkOrder  |     | CreateWorkOrder  |     |  CAFM  |
| (EQ+)  |     |    Function      |     |    Handler       |     |  FSI   |
+--------+     +------------------+     +------------------+     +--------+
    |                 |                        |                     |
    | POST /workorders|                        |                     |
    |---------------->|                        |                     |
    |                 |                        |                     |
    |                 | CreateWorkOrderAsync() |                     |
    |                 |----------------------->|                     |
    |                 |                        |                     |
    |                 |                        | 1. SOAP Authenticate|
    |                 |                        |-------------------->|
    |                 |                        |   (SessionId)       |
    |                 |                        |<--------------------|
    |                 |                        |                     |
    |                 |                        | 2. GetLocationsByDto|
    |                 |                        |-------------------->|
    |                 |                        |   (BuildingId,      |
    |                 |                        |    LocationId)      |
    |                 |                        |<--------------------|
    |                 |                        |                     |
    |                 |                        | 3. GetInstructionSets
    |                 |                        |-------------------->|
    |                 |                        |   (CategoryId,      |
    |                 |                        |    DisciplineId,    |
    |                 |                        |    InstructionId,   |
    |                 |                        |    PriorityId)      |
    |                 |                        |<--------------------|
    |                 |                        |                     |
    |                 |                        | 4. CreateBreakdown  |
    |                 |                        |    Task             |
    |                 |                        |-------------------->|
    |                 |                        |   (TaskId)          |
    |                 |                        |<--------------------|
    |                 |                        |                     |
    |                 |                        | 5. SOAP Logout      |
    |                 |                        |-------------------->|
    |                 |                        |                     |
    |                 |      Response          |<--------------------|
    |                 |<-----------------------|                     |
    |   JSON Response |                        |                     |
    |<----------------|                        |                     |
    |                 |                        |                     |
```

## Project Structure

```
sys-cafm-mgmt/
├── Abstractions/
│   └── IWorkOrderMgmt.cs              # Interface for work order operations
├── ConfigModels/
│   └── AppConfigs.cs                  # Configuration model classes
├── DTOs/
│   ├── Requests/
│   │   └── CreateWorkOrderRequestDto.cs
│   ├── Responses/
│   │   └── CreateWorkOrderResponseDto.cs
│   └── Downstream/
│       ├── AuthenticateRequestDto.cs
│       ├── AuthenticateResponseDto.cs
│       ├── LogoutRequestDto.cs
│       ├── GetLocationsByDtoRequestDto.cs
│       ├── GetLocationsByDtoResponseDto.cs
│       ├── GetInstructionSetsByDtoRequestDto.cs
│       ├── GetInstructionSetsByDtoResponseDto.cs
│       ├── CreateBreakdownTaskRequestDto.cs
│       └── CreateBreakdownTaskResponseDto.cs
├── Functions/
│   └── CreateWorkOrderFunction.cs     # Azure Function HTTP endpoint
├── Implementations/
│   └── Fsi/
│       ├── AtomicHandlers/
│       │   ├── AuthenticateAtomicHandler.cs
│       │   ├── LogoutAtomicHandler.cs
│       │   ├── GetLocationsByDtoAtomicHandler.cs
│       │   ├── GetInstructionSetsByDtoAtomicHandler.cs
│       │   └── CreateBreakdownTaskAtomicHandler.cs
│       ├── Handlers/
│       │   └── CreateWorkOrderHandler.cs
│       └── Services/
│           └── WorkOrderService.cs
├── Middleware/
│   ├── FsiSessionContext.cs
│   └── RequiresFsiAuthenticationAttribute.cs
├── Program.cs
├── appsettings.json
├── appsettings.{env}.json             # Environment-specific configs
└── README.md
```

## API Endpoints

### Create Work Order

**Endpoint:** `POST /api/workorders`

**Request Body:**
```json
{
  "workOrder": [
    {
      "reporterName": "John Doe",
      "reporterEmail": "john.doe@example.com",
      "reporterPhoneNumber": "+971501234567",
      "description": "AC not working in meeting room",
      "serviceRequestNumber": "SR-2024-001",
      "propertyName": "Building A",
      "unitCode": "UNIT-101",
      "categoryName": "HVAC",
      "subCategory": "Air Conditioning",
      "technician": "",
      "sourceOrgId": "ORG-001",
      "ticketDetails": {
        "status": "Open",
        "subStatus": "",
        "priority": "High",
        "scheduledDate": "2024-01-15",
        "scheduledTimeStart": "09:00:00",
        "scheduledTimeEnd": "17:00:00",
        "recurrence": "",
        "oldCAFMSRnumber": "",
        "raisedDateUtc": "2024-01-14T10:30:00Z"
      }
    }
  ]
}
```

**Success Response (200):**
```json
{
  "message": "Work order created successfully",
  "errorCode": "SUCCESS",
  "data": {
    "workOrder": [
      {
        "cafmSRNumber": "12345",
        "sourceSRNumber": "SR-2024-001",
        "sourceOrgId": "ORG-001",
        "success": true,
        "message": "Work order created successfully"
      }
    ]
  },
  "errorDetails": null,
  "isDownStreamError": false,
  "isPartialSuccess": false
}
```

**Error Response (4xx/5xx):**
```json
{
  "message": "Error description",
  "errorCode": "ERROR_CODE",
  "data": null,
  "errorDetails": {
    "errors": [
      {
        "stepName": "FSI.CreateBreakdownTask",
        "stepError": "Detailed error message"
      }
    ]
  },
  "isDownStreamError": true,
  "isPartialSuccess": false
}
```

## Configuration

### Required Configuration Settings

The following settings must be configured in `appsettings.{env}.json`:

| Setting | Description | Required |
|---------|-------------|----------|
| `AppConfigs:Cafm:BaseUrl` | CAFM FSI base URL | Yes |
| `AppConfigs:Cafm:Username` | FSI service account username | Yes |
| `AppConfigs:Cafm:Password` | FSI service account password | Yes |
| `AppConfigs:Cafm:DefaultContractId` | Default contract ID for tasks | Yes |
| `AppConfigs:Cafm:CallerSourceId` | Caller source identifier | Yes |

### Environment URLs

| Environment | Base URL |
|-------------|----------|
| dev | https://devcafm.agfacilities.com |
| qa | https://qacafm.agfacilities.com |
| stg | https://stgcafm.agfacilities.com |
| prod | https://cafm.agfacilities.com |
| dr | https://drcafm.agfacilities.com |

### Secure Configuration (TODO)

**Important:** The following credentials should be stored securely and not in configuration files:

- `AppConfigs:Cafm:Username` - TODO: Retrieve from Azure Key Vault
- `AppConfigs:Cafm:Password` - TODO: Retrieve from Azure Key Vault

## Running Locally

### Prerequisites

- .NET 8.0 SDK
- Azure Functions Core Tools v4
- Azure Storage Emulator (or Azurite)

### Steps

1. Clone the repository
2. Navigate to the project directory:
   ```bash
   cd sys-cafm-mgmt
   ```
3. Restore dependencies:
   ```bash
   dotnet restore
   ```
4. Set environment in `local.settings.json`:
   ```json
   {
     "Values": {
       "Environment": "dev"
     }
   }
   ```
5. Configure credentials in `appsettings.dev.json` (for local testing only)
6. Run the function app:
   ```bash
   func start
   ```

### Testing the API

```bash
curl -X POST http://localhost:7071/api/workorders \
  -H "Content-Type: application/json" \
  -d '{
    "workOrder": [{
      "serviceRequestNumber": "TEST-001",
      "propertyName": "Test Building",
      "description": "Test work order"
    }]
  }'
```

## SOAP Operations

This System Layer communicates with CAFM FSI using SOAP over HTTP:

| Operation | SOAP Action | Description |
|-----------|-------------|-------------|
| Authenticate | `IIntegrationService/Authenticate` | Login to obtain SessionId |
| LogOut | `IIntegrationService/LogOut` | End session |
| GetLocationsByDto | `IIntegrationService/GetLocationsByDto` | Lookup building/location IDs |
| GetInstructionSetsByDto | `IIntegrationService/GetInstructionSetsByDto` | Lookup category/instruction IDs |
| CreateBreakdownTask | `IIntegrationService/CreateBreakdownTask` | Create work order |

## Error Handling

All errors are handled by the `ExceptionHandlerMiddleware` and return standardized `BaseResponseDTO`:

- **Validation Errors (400)**: Missing or invalid request parameters
- **Authentication Errors (401)**: CAFM FSI authentication failures
- **Downstream Errors (5xx)**: FSI service failures
- **Internal Errors (500)**: Unexpected application errors

## Dependencies

This project references the shared Framework:
- `Core` - Base exceptions, DTOs, HTTP client, logging extensions

## Architecture Notes

This implementation follows API-Led Architecture principles:
- **System Layer Only**: This repository handles direct SOR (System of Record) integration
- **No Business Logic**: Business orchestration belongs in Process Layer
- **One Interface Per Entity**: `IWorkOrderMgmt` defines all work order operations
- **Vendor-Specific Implementations**: FSI-specific logic in `Implementations/Fsi/`
