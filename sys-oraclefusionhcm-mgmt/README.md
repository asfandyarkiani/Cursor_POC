# Oracle Fusion HCM System Layer

## Overview

This System Layer provides integration with Oracle Fusion HCM (Human Capital Management) for leave management operations.

**System of Record:** Oracle Fusion HCM  
**Integration Type:** REST API (JSON)  
**Authentication:** Basic Authentication (credentials-per-request)

## Architecture

This project follows the API-Led Architecture System Layer pattern:

```
Azure Function (CreateLeaveAPI)
    ↓
Service (LeaveMgmtService) - implements ILeaveMgmt
    ↓
Handler (CreateLeaveHandler) - orchestrates atomic operations
    ↓
Atomic Handler (CreateLeaveAtomicHandler) - single REST API call
    ↓
Oracle Fusion HCM REST API
```

## Operations

### CreateLeave

**Purpose:** Create leave absence entry in Oracle Fusion HCM

**Endpoint:** `POST /api/leave/create`

**Request:**
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
  "message": "Leave absence entry created successfully in Oracle Fusion HCM.",
  "errorCode": null,
  "data": {
    "personAbsenceEntryId": 123456,
    "personNumber": "9000604",
    "absenceType": "Sick Leave",
    "employer": "Al Ghurair Investment LLC",
    "startDate": "2024-03-24",
    "endDate": "2024-03-25",
    "absenceStatusCd": "SUBMITTED",
    "approvalStatusCd": "APPROVED",
    "startDateDuration": 1,
    "endDateDuration": 1
  },
  "errorDetails": null,
  "isDownStreamError": false,
  "isPartialSuccess": false
}
```

**Response (Error):**
```json
{
  "message": "Failed to create leave absence entry in Oracle Fusion HCM.",
  "errorCode": "HCM_LEVCRT_0001",
  "data": null,
  "errorDetails": {
    "errors": [
      {
        "stepName": "CreateLeaveHandler.cs / HandleAsync",
        "stepError": "Oracle Fusion HCM API returned status 400. Response: [error details]"
      }
    ]
  },
  "isDownStreamError": true,
  "isPartialSuccess": false
}
```

## Configuration

### AppConfigs (appsettings.json)

```json
{
  "AppConfigs": {
    "OracleFusionBaseUrl": "https://iaaxey-dev3.fa.ocs.oraclecloud.com:443",
    "OracleFusionUsername": "INTEGRATION.USER@al-ghurair.com",
    "OracleFusionPassword": "",
    "LeaveResourcePath": "hcmRestApi/resources/11.13.18.05/absences",
    "TimeoutSeconds": 50,
    "RetryCount": 0
  }
}
```

### KeyVault Secrets

- **OracleFusionPassword:** Password for Oracle Fusion HCM Basic Authentication

### Environment Variables

- **ENVIRONMENT** or **ASPNETCORE_ENVIRONMENT:** Environment name (dev, qa, prod)

## Sequence Diagram

```
START
 |
 ├─→ CreateLeaveAPI (Azure Function)
 |   └─→ Validate request
 |   └─→ Call ILeaveMgmt.CreateLeave()
 |
 ├─→ LeaveMgmtService
 |   └─→ Delegate to CreateLeaveHandler
 |
 ├─→ CreateLeaveHandler
 |   └─→ Transform request to atomic DTO
 |   └─→ Call CreateLeaveAtomicHandler
 |   └─→ Check response status
 |   └─→ IF SUCCESS:
 |   |   └─→ Deserialize API response
 |   |   └─→ Map to CreateLeaveResDTO
 |   |   └─→ Return BaseResponseDTO (success)
 |   └─→ IF ERROR:
 |       └─→ Throw DownStreamApiFailureException
 |
 ├─→ CreateLeaveAtomicHandler
 |   └─→ Read credentials from AppConfigs/KeyVault
 |   └─→ Build API URL
 |   └─→ Map DTO to request body
 |   └─→ Execute REST API call (POST)
 |   └─→ Return HttpResponseSnapshot
 |
 └─→ Oracle Fusion HCM REST API
     └─→ POST /hcmRestApi/resources/11.13.18.05/absences
     └─→ Return leave absence entry with personAbsenceEntryId
```

## Error Handling

### Error Constants

| Error Code | Message | HTTP Status |
|---|---|---|
| HCM_LEVCRT_0001 | Failed to create leave absence entry in Oracle Fusion HCM. | Varies (from API) |
| HCM_LEVCRT_0002 | Oracle Fusion HCM API returned empty response body. | 500 |
| HCM_LEVCRT_0003 | Failed to decompress GZIP response from Oracle Fusion HCM. | 500 |

### Exception Flow

```
Atomic Handler → Returns HttpResponseSnapshot (never throws on HTTP errors)
    ↓
Handler → Checks IsSuccessStatusCode → Throws DownStreamApiFailureException if failed
    ↓
Middleware (ExceptionHandlerMiddleware) → Catches exception → Returns BaseResponseDTO
    ↓
Client → Receives normalized error response
```

## Middleware

This project uses the following middleware (in order):

1. **ExecutionTimingMiddleware** - Tracks request execution time
2. **ExceptionHandlerMiddleware** - Normalizes exceptions to BaseResponseDTO

**Note:** No custom authentication middleware is used. Oracle Fusion HCM uses credentials-per-request (Basic Auth).

## Dependencies

### Framework References

- **Core** - Framework/Core/Core/Core.csproj
- **Cache** - Framework/Cache/Cache.csproj

### NuGet Packages

- Microsoft.Azure.Functions.Worker (v1.23.0)
- Microsoft.Azure.Functions.Worker.Sdk (v1.18.0)
- Microsoft.Azure.Functions.Worker.Extensions.Http (v3.2.0)
- Microsoft.ApplicationInsights.WorkerService (v2.22.0)
- Azure.Identity (v1.13.1)
- Azure.Security.KeyVault.Secrets (v4.7.0)
- Polly (v8.5.0)
- StackExchange.Redis (v2.8.16)

## Project Structure

```
sys-oraclefusionhcm-mgmt/
├── Abstractions/
│   └── ILeaveMgmt.cs
├── ConfigModels/
│   ├── AppConfigs.cs
│   └── KeyVaultConfigs.cs
├── Constants/
│   ├── ErrorConstants.cs
│   ├── InfoConstants.cs
│   └── OperationNames.cs
├── DTO/
│   ├── CreateLeaveDTO/
│   │   ├── CreateLeaveReqDTO.cs
│   │   └── CreateLeaveResDTO.cs
│   ├── AtomicHandlerDTOs/
│   │   └── CreateLeaveHandlerReqDTO.cs
│   └── DownstreamDTOs/
│       └── CreateLeaveApiResDTO.cs
├── Functions/
│   └── CreateLeaveAPI.cs
├── Helper/
│   └── KeyVaultReader.cs
├── Implementations/
│   └── OracleFusionHCM/
│       ├── AtomicHandlers/
│       │   └── CreateLeaveAtomicHandler.cs
│       ├── Handlers/
│       │   └── CreateLeaveHandler.cs
│       └── Services/
│           └── LeaveMgmtService.cs
├── Program.cs
├── host.json
├── appsettings.json
├── appsettings.dev.json
├── appsettings.qa.json
├── appsettings.prod.json
└── sys-oraclefusionhcm-mgmt.csproj
```

## Build & Run

### Prerequisites

- .NET 8 SDK
- Azure Functions Core Tools v4
- Access to Azure KeyVault (for production secrets)
- Redis instance (for caching)

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
curl -X POST http://localhost:7071/api/leave/create \
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

This project is designed to be deployed as an Azure Function App with the following configuration:

- **Runtime:** .NET 8 Isolated Worker
- **Azure Functions Version:** v4
- **Environment Variables:** ENVIRONMENT, APPLICATIONINSIGHTS_CONNECTION_STRING
- **KeyVault:** Configured via AppConfigs.KeyVault.Url
- **Redis:** Configured via AppConfigs.RedisCache.ConnectionString

## Notes

- All sensitive credentials (passwords) are stored in Azure KeyVault
- Basic Authentication credentials are sent with each request (credentials-per-request pattern)
- No session or token management required
- Response timing is tracked via ExecutionTimingMiddleware
- All exceptions are normalized to BaseResponseDTO via ExceptionHandlerMiddleware

## Migration from Boomi

This System Layer was migrated from the following Boomi process:

- **Boomi Process:** HCM_Leave Create (ca69f858-785f-4565-ba1f-b4edc6cca05b)
- **Boomi Operation:** Leave Oracle Fusion Create (6e8920fd-af5a-430b-a1d9-9fde7ac29a12)
- **Original Flow:** D365 → Boomi → Oracle Fusion HCM
- **New Flow:** Process Layer → System Layer (CreateLeaveAPI) → Oracle Fusion HCM

**Key Differences:**
- Boomi handled error email notifications internally (subprocess)
- Azure Function focuses only on Oracle Fusion HCM API interaction
- Error notifications should be handled by Process Layer or separate Email System Layer
- GZIP decompression handling removed (modern HTTP clients handle this automatically)
