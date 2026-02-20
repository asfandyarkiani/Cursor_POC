# Oracle Fusion HCM System Layer

**Project:** sys-oraclefusion-hcm  
**Purpose:** System Layer API for Oracle Fusion HCM leave/absence management  
**Architecture:** API-Led Architecture (System Layer)  
**Target:** .NET 8, Azure Functions v4

---

## Overview

This System Layer provides a standardized API interface to Oracle Fusion HCM for leave/absence operations. It insulates consumers from the complexity of Oracle Fusion HCM REST API and provides a reusable "Lego block" for Process Layer orchestration.

**Downstream System:** Oracle Fusion HCM (REST API)  
**Authentication:** Basic Auth (credentials per request)  
**Base URL:** https://iaaxey-{env}.fa.ocs.oraclecloud.com:443  
**Endpoint:** /hcmRestApi/resources/11.13.18.05/absences

---

## Architecture

### Call Flow

```
Process Layer
    ↓ HTTP POST /api/hcm/leave/create
CreateLeaveAPI (Azure Function)
    ↓ inject ILeaveMgmt
LeaveMgmtService (Service)
    ↓ inject CreateLeaveHandler
CreateLeaveHandler (Handler)
    ↓ inject CreateLeaveAtomicHandler
CreateLeaveAtomicHandler (Atomic Handler)
    ↓ HTTP POST with Basic Auth
Oracle Fusion HCM REST API
```

### Sequence Diagram

```
┌──────────────┐       ┌──────────────┐       ┌──────────────┐       ┌──────────────┐       ┌──────────────┐       ┌──────────────┐
│ Process Layer│       │CreateLeaveAPI│       │LeaveMgmtSvc  │       │CreateLeave   │       │CreateLeave   │       │Oracle Fusion │
│              │       │  (Function)  │       │  (Service)   │       │   Handler    │       │AtomicHandler │       │     HCM      │
└──────┬───────┘       └──────┬───────┘       └──────┬───────┘       └──────┬───────┘       └──────┬───────┘       └──────┬───────┘
       │                      │                      │                      │                      │                      │
       │ POST /api/hcm/       │                      │                      │                      │                      │
       │ leave/create         │                      │                      │                      │                      │
       │ CreateLeaveReqDTO    │                      │                      │                      │                      │
       ├─────────────────────>│                      │                      │                      │                      │
       │                      │                      │                      │                      │                      │
       │                      │ Validate Request     │                      │                      │                      │
       │                      │ (ValidateAPI         │                      │                      │                      │
       │                      │  RequestParameters)  │                      │                      │                      │
       │                      │                      │                      │                      │                      │
       │                      │ CreateLeave(request) │                      │                      │                      │
       │                      ├─────────────────────>│                      │                      │                      │
       │                      │                      │                      │                      │                      │
       │                      │                      │ HandleAsync(request) │                      │                      │
       │                      │                      ├─────────────────────>│                      │                      │
       │                      │                      │                      │                      │                      │
       │                      │                      │                      │ CreateLeaveIn        │                      │
       │                      │                      │                      │ Downstream()         │                      │
       │                      │                      │                      │ - Map ReqDTO to      │                      │
       │                      │                      │                      │   HandlerReqDTO      │                      │
       │                      │                      │                      │ - Transform field    │                      │
       │                      │                      │                      │   names              │                      │
       │                      │                      │                      │                      │                      │
       │                      │                      │                      │ Handle(atomicReq)    │                      │
       │                      │                      │                      ├─────────────────────>│                      │
       │                      │                      │                      │                      │                      │
       │                      │                      │                      │                      │ Read AppConfigs      │
       │                      │                      │                      │                      │ - Username           │
       │                      │                      │                      │                      │ - Password           │
       │                      │                      │                      │                      │ - BaseApiUrl         │
       │                      │                      │                      │                      │ - ResourcePath       │
       │                      │                      │                      │                      │                      │
       │                      │                      │                      │                      │ MapDtoToRequestBody()│
       │                      │                      │                      │                      │ - personNumber       │
       │                      │                      │                      │                      │ - absenceType        │
       │                      │                      │                      │                      │ - employer           │
       │                      │                      │                      │                      │ - startDate          │
       │                      │                      │                      │                      │ - endDate            │
       │                      │                      │                      │                      │ - absenceStatusCd    │
       │                      │                      │                      │                      │ - approvalStatusCd   │
       │                      │                      │                      │                      │ - startDateDuration  │
       │                      │                      │                      │                      │ - endDateDuration    │
       │                      │                      │                      │                      │                      │
       │                      │                      │                      │                      │ POST /absences       │
       │                      │                      │                      │                      │ Basic Auth           │
       │                      │                      │                      │                      ├─────────────────────>│
       │                      │                      │                      │                      │                      │
       │                      │                      │                      │                      │ HTTP 200/201         │
       │                      │                      │                      │                      │ CreateLeaveApiResDTO │
       │                      │                      │                      │                      │<─────────────────────┤
       │                      │                      │                      │                      │                      │
       │                      │                      │                      │ HttpResponseSnapshot │                      │
       │                      │                      │                      │<─────────────────────┤                      │
       │                      │                      │                      │                      │                      │
       │                      │                      │                      │ Check IsSuccess      │                      │
       │                      │                      │                      │ StatusCode           │                      │
       │                      │                      │                      │                      │                      │
       │                      │                      │                      │ Deserialize          │                      │
       │                      │                      │                      │ ApiResDTO            │                      │
       │                      │                      │                      │                      │                      │
       │                      │                      │                      │ Map ApiResDTO        │                      │
       │                      │                      │                      │ to ResDTO            │                      │
       │                      │                      │                      │                      │                      │
       │                      │                      │ BaseResponseDTO      │                      │                      │
       │                      │                      │<─────────────────────┤                      │                      │
       │                      │                      │                      │                      │                      │
       │                      │ BaseResponseDTO      │                      │                      │                      │
       │                      │<─────────────────────┤                      │                      │                      │
       │                      │                      │                      │                      │                      │
       │ BaseResponseDTO      │                      │                      │                      │                      │
       │<─────────────────────┤                      │                      │                      │                      │
       │                      │                      │                      │                      │                      │
```

---

## API Specification

### CreateLeave API

**Endpoint:** `POST /api/hcm/leave/create`  
**Function Name:** CreateLeave  
**Authorization:** Anonymous (Process Layer provides auth context)

**Request DTO:** CreateLeaveReqDTO
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

**Response DTO:** CreateLeaveResDTO (wrapped in BaseResponseDTO)
```json
{
  "message": "Leave created successfully in Oracle Fusion HCM.",
  "errorCode": null,
  "data": {
    "personAbsenceEntryId": 300100123456789,
    "status": "SUBMITTED"
  },
  "errorDetails": null,
  "isDownStreamError": false
}
```

**Error Response:**
```json
{
  "message": "Failed to create leave in Oracle Fusion HCM.",
  "errorCode": "OFH_LEVCRT_0001",
  "data": null,
  "errorDetails": {
    "errorCode": "OFH_LEVCRT_0001",
    "message": "Failed to create leave in Oracle Fusion HCM.",
    "details": ["Status 400. Response: {error details}"]
  },
  "isDownStreamError": true
}
```

---

## Field Mappings

### D365 → Oracle Fusion HCM

| D365 Field | Oracle Fusion Field | Transformation |
|---|---|---|
| employeeNumber | personNumber | Direct mapping |
| absenceType | absenceType | Direct mapping |
| employer | employer | Direct mapping |
| startDate | startDate | Direct mapping (YYYY-MM-DD) |
| endDate | endDate | Direct mapping (YYYY-MM-DD) |
| absenceStatusCode | absenceStatusCd | Field name change |
| approvalStatusCode | approvalStatusCd | Field name change |
| startDateDuration | startDateDuration | Direct mapping |
| endDateDuration | endDateDuration | Direct mapping |

---

## Configuration

### Environment Variables

**Required:**
- `ENVIRONMENT` or `ASPNETCORE_ENVIRONMENT` (dev/qa/prod)

### AppConfigs (appsettings.json)

```json
{
  "AppConfigs": {
    "BaseApiUrl": "https://iaaxey-{env}.fa.ocs.oraclecloud.com:443",
    "ResourcePath": "hcmRestApi/resources/11.13.18.05/absences",
    "Username": "{oracle_fusion_username}",
    "Password": "",
    "TimeoutSeconds": 50,
    "RetryCount": 0
  }
}
```

### KeyVault Secrets

**Required Secrets:**
- `OracleFusionHcmPassword` - Oracle Fusion HCM Basic Auth password

### Redis Cache

```json
{
  "RedisCache": {
    "ConnectionString": "{redis_connection_string}",
    "InstanceName": "OracleFusionHcm:"
  }
}
```

---

## Error Codes

| Error Code | Message | HTTP Status |
|---|---|---|
| OFH_LEVCRT_0001 | Failed to create leave in Oracle Fusion HCM. | Varies (from API) |
| OFH_LEVCRT_0002 | Oracle Fusion HCM returned empty response body. | 500 |
| OFH_LEVCRT_0003 | Failed to deserialize Oracle Fusion HCM response. | 500 |

---

## Deployment

### Prerequisites

1. .NET 8 SDK
2. Azure Functions Core Tools v4
3. Azure subscription with:
   - Azure Functions App
   - Azure KeyVault
   - Azure Redis Cache
   - Application Insights

### Build & Deploy

```bash
# Restore packages
dotnet restore

# Build
dotnet build --configuration Release

# Publish
dotnet publish --configuration Release --output ./publish

# Deploy to Azure (using Azure CLI)
func azure functionapp publish {function-app-name}
```

### Configuration Steps

1. **KeyVault Setup:**
   - Create secret: `OracleFusionHcmPassword`
   - Grant Function App Managed Identity access

2. **Application Settings:**
   - Set `ENVIRONMENT` variable (dev/qa/prod)
   - Configure `APPLICATIONINSIGHTS_CONNECTION_STRING`

3. **Redis Cache:**
   - Create Azure Redis Cache instance
   - Configure connection string in KeyVault or App Settings

---

## Testing

### Local Testing

```bash
# Start Azure Functions locally
func start
```

### Test Request (cURL)

```bash
curl -X POST http://localhost:7071/api/hcm/leave/create \
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

### Expected Response

```json
{
  "message": "Leave created successfully in Oracle Fusion HCM.",
  "errorCode": null,
  "data": {
    "personAbsenceEntryId": 300100123456789,
    "status": "SUBMITTED"
  }
}
```

---

## Project Structure

```
sys-oraclefusion-hcm/
├── Abstractions/
│   └── ILeaveMgmt.cs                    # Service interface
├── Implementations/OracleFusion/
│   ├── Services/
│   │   └── LeaveMgmtService.cs          # Service implementation
│   ├── Handlers/
│   │   └── CreateLeaveHandler.cs        # Orchestration handler
│   └── AtomicHandlers/
│       └── CreateLeaveAtomicHandler.cs  # Single API call handler
├── DTO/
│   ├── CreateLeaveDTO/
│   │   ├── CreateLeaveReqDTO.cs         # API request DTO
│   │   └── CreateLeaveResDTO.cs         # API response DTO
│   ├── AtomicHandlerDTOs/
│   │   └── CreateLeaveHandlerReqDTO.cs  # Atomic handler DTO
│   └── DownstreamDTOs/
│       └── CreateLeaveApiResDTO.cs      # Oracle Fusion response DTO
├── Functions/
│   └── CreateLeaveAPI.cs                # Azure Function entry point
├── ConfigModels/
│   ├── AppConfigs.cs                    # Application configuration
│   └── KeyVaultConfigs.cs               # KeyVault configuration
├── Constants/
│   ├── ErrorConstants.cs                # Error code constants
│   ├── InfoConstants.cs                 # Success message constants
│   └── OperationNames.cs                # Operation name constants
├── Program.cs                           # DI registration & startup
├── host.json                            # Azure Functions host config
├── appsettings.json                     # Configuration (placeholder)
├── appsettings.dev.json                 # Development config
├── appsettings.qa.json                  # QA config
├── appsettings.prod.json                # Production config
├── OracleFusionHcm.csproj               # Project file
└── OracleFusionHcm.sln                  # Solution file
```

---

## Key Features

✅ **Standardized API Interface:** Consistent request/response DTOs  
✅ **Error Handling:** Framework-based exception handling with meaningful error codes  
✅ **Validation:** Comprehensive request validation with detailed error messages  
✅ **Logging:** Structured logging with Application Insights integration  
✅ **Performance Tracking:** Execution timing middleware  
✅ **Resilience:** Polly retry and timeout policies  
✅ **Configuration:** Environment-specific configuration with validation  
✅ **Security:** Basic Auth with KeyVault integration for password storage  
✅ **Caching:** Redis cache support (Framework integration)

---

## Dependencies

### Framework Projects
- **Core** (../Framework/Core/Core/Core.csproj)
- **Cache** (../Framework/Cache/Cache.csproj)

### NuGet Packages
- Microsoft.Azure.Functions.Worker (2.0.0)
- Microsoft.Azure.Functions.Worker.Sdk (2.0.0)
- Microsoft.Azure.Functions.Worker.Extensions.Http (3.2.0)
- Microsoft.ApplicationInsights.WorkerService (2.22.0)
- Polly (8.6.1)
- Azure.Identity (1.13.1)
- Azure.Security.KeyVault.Secrets (4.8.0)

---

## Compliance

This implementation is **100% compliant** with:
- System-Layer-Rules.mdc (44/44 applicable rules)
- API-Led Architecture principles
- Enterprise architecture standards

See `RULEBOOK_COMPLIANCE_REPORT.md` for detailed compliance audit.

---

## Support

**Integration Team:** BoomiIntegrationTeam@al-ghurair.com  
**Architecture:** API-Led Architecture (System Layer)  
**Framework Version:** .NET 8, Azure Functions v4

---

**End of README.md**
