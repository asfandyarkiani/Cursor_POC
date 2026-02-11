# Oracle Fusion HCM System Layer - Leave Management

## Overview

This System Layer repository provides API access to Oracle Fusion HCM for Leave/Absence Management operations. It follows the API-Led Architecture principles, exposing reusable "Lego block" APIs that Process Layer can orchestrate.

## Architecture

### System of Record (SOR)
- **Name:** Oracle Fusion HCM
- **Type:** REST API (JSON)
- **Base URL:** `https://iaaxey-{env}.fa.ocs.oraclecloud.com:443`
- **Authentication:** Basic Authentication (per-request credentials)

### Entity Management
- **Entity:** Leave/Absence Management
- **Interface:** `ILeaveMgmt`
- **Service:** `LeaveMgmtService`
- **Vendor:** OracleFusionHCM

## API Endpoints

### CreateLeaveAPI

**Endpoint:** `POST /api/leave`

**Purpose:** Creates a leave absence entry in Oracle Fusion HCM

**Request Body:**
```json
{
  "employeeNumber": 9000604,
  "absenceType": "Sick Leave",
  "employer": "Al Ghurair Investment LLC",
  "startDate": "2024-03-24",
  "endDate": "2024-03-25",
  "absenceStatusCode": "SUBMITTED",
  "approvalStatusCode": "APPROVED",
  "startDateDuration": 1,
  "endDateDuration": 1
}
```

**Response (Success):**
```json
{
  "status": "success",
  "message": "Leave absence created successfully in Oracle Fusion HCM.",
  "personAbsenceEntryId": 123456,
  "success": true
}
```

**Response (Failure):**
```json
{
  "status": "failure",
  "message": "Error message details",
  "personAbsenceEntryId": null,
  "success": false
}
```

## Project Structure

```
sys-oraclefusionhcm-mgmt/
├── Abstractions/                          # Interfaces (ILeaveMgmt)
├── Implementations/OracleFusionHCM/       # Vendor-specific implementation
│   ├── Services/                          # LeaveMgmtService
│   ├── Handlers/                          # CreateLeaveHandler
│   └── AtomicHandlers/                    # CreateLeaveAtomicHandler
├── DTO/
│   ├── CreateLeaveDTO/                    # API-level DTOs
│   ├── AtomicHandlerDTOs/                 # Atomic Handler DTOs
│   └── DownstreamDTOs/                    # Oracle Fusion HCM response DTOs
├── Functions/                             # Azure Functions (CreateLeaveAPI)
├── ConfigModels/                          # AppConfigs
├── Constants/                             # ErrorConstants, InfoConstants
├── Helpers/                               # RestApiHelper
├── Program.cs                             # DI and middleware registration
├── host.json                              # Azure Functions host configuration
└── appsettings.{env}.json                 # Environment-specific configuration
```

## Configuration

### Environment Variables

Configuration is loaded from `appsettings.{env}.json` files:

- **dev:** Development environment
- **qa:** QA environment
- **prod:** Production environment

### Required Configuration

```json
{
  "OracleFusionHCM": {
    "BaseUrl": "https://iaaxey-dev3.fa.ocs.oraclecloud.com:443",
    "ResourcePath": "hcmRestApi/resources/11.13.18.05/absences",
    "Username": "INTEGRATION.USER@al-ghurair.com",
    "Password": "TODO_SECURE_PASSWORD_FROM_KEYVAULT",
    "ConnectTimeout": 30000,
    "ReadTimeout": 60000
  }
}
```

## Middleware

Middleware is registered in the following order (per System-Layer-Rules.mdc):

1. **ExecutionTimingMiddleware** (FIRST) - Tracks execution time
2. **ExceptionHandlerMiddleware** (SECOND) - Handles exceptions and normalizes errors

No `CustomAuthenticationMiddleware` is needed because Oracle Fusion HCM uses Basic Authentication with per-request credentials.

## Error Codes

Error codes follow the format: `OFH_XXXXXX_DDDD`

- `OFH` = Oracle Fusion HCM (3 chars)
- `XXXXXX` = Operation identifier (6 chars)
- `DDDD` = Error number (4 digits)

### CreateLeave API Errors (1000-1999)

| Error Code | Message |
|-----------|---------|
| OFH_CRLEAV_1001 | Failed to create leave absence in Oracle Fusion HCM. Downstream API returned error. |
| OFH_CRLEAV_1002 | Invalid request payload for CreateLeave API. Required fields are missing or invalid. |
| OFH_CRLEAV_1003 | Oracle Fusion HCM API returned non-success HTTP status code. |
| OFH_CRLEAV_1004 | Failed to deserialize Oracle Fusion HCM API response. |

### General System Layer Errors (9000-9999)

| Error Code | Message |
|-----------|---------|
| OFH_SYSTEM_9001 | Unexpected error occurred in Oracle Fusion HCM System Layer. |
| OFH_SYSTEM_9002 | Configuration error: Oracle Fusion HCM connection settings are missing or invalid. |
| OFH_SYSTEM_9003 | Authentication failed: Invalid credentials for Oracle Fusion HCM API. |

## Sequence Diagram

```
┌─────────────┐
│ Process     │
│ Layer       │
└──────┬──────┘
       │
       │ POST /api/leave (CreateLeaveReqDTO)
       ▼
┌──────────────────────────────────────────────────────────┐
│ CreateLeaveAPI (Azure Function)                          │
├──────────────────────────────────────────────────────────┤
│ 1. Validate request                                      │
│ 2. Call LeaveMgmtService.CreateLeaveAsync()             │
│    └─> CreateLeaveHandler.HandleAsync()                 │
│        ├─> Validate request DTO                         │
│        ├─> Map to CreateLeaveHandlerReqDTO              │
│        └─> CreateLeaveAtomicHandler.CreateLeaveAsync()  │
│            ├─> Validate configuration                   │
│            ├─> Build full URL                           │
│            ├─> Add Basic Auth header                    │
│            ├─> POST to Oracle Fusion HCM API            │
│            │   URL: {BaseUrl}/{ResourcePath}            │
│            │   Body: {personNumber, absenceType, ...}   │
│            └─> Return HttpResponseSnapshot              │
│        ├─> Check HTTP status code                       │
│        ├─> Deserialize CreateLeaveApiResDTO             │
│        └─> Map to CreateLeaveResDTO                     │
│ 3. Return success/failure response                      │
└──────────────────────────────────────────────────────────┘
       │
       │ Response: CreateLeaveResDTO
       ▼
┌──────────────┐
│ Process      │
│ Layer        │
└──────────────┘
```

## Development

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
curl -X POST http://localhost:7071/api/leave \
  -H "Content-Type: application/json" \
  -d '{
    "employeeNumber": 9000604,
    "absenceType": "Sick Leave",
    "employer": "Al Ghurair Investment LLC",
    "startDate": "2024-03-24",
    "endDate": "2024-03-25",
    "absenceStatusCode": "SUBMITTED",
    "approvalStatusCode": "APPROVED",
    "startDateDuration": 1,
    "endDateDuration": 1
  }'
```

## Deployment

This System Layer is deployed as an Azure Function App. CI/CD pipeline automatically deploys to:

- **DEV:** Development environment
- **QA:** QA environment
- **PROD:** Production environment

## Dependencies

- **Framework/Core:** Core framework with base classes, interfaces, and middleware
- **Microsoft.Azure.Functions.Worker:** Azure Functions v4 runtime
- **Polly:** Resilience and transient-fault-handling library
- **System.Text.Json:** JSON serialization/deserialization

## License

Internal use only - Al Ghurair Investment LLC

## Contact

For questions or support, contact the Integration Team.
