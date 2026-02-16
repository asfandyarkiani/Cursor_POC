# Oracle Fusion HCM - Leave Management System Layer

## Overview

This System Layer API provides integration with Oracle Fusion HCM for leave management operations. It unlocks leave data from Oracle Fusion HCM and insulates consumers from the complexity of the underlying Oracle Fusion API.

**Domain:** Human Resource Management (HCM)  
**System of Record:** Oracle Fusion HCM  
**Purpose:** Create leave absence entries in Oracle Fusion HCM

## Architecture

This project follows the **API-Led Architecture** principles:

- **System Layer:** Unlocks data from Oracle Fusion HCM (this project)
- **Process Layer:** Orchestrates business logic and multiple System APIs (consumer)
- **Experience Layer:** Reconfigures data for end-user needs (consumer)

## Project Structure

```
OracleFusionHCM.LeaveManagement/
├── Abstractions/                    # Interfaces at ROOT
│   └── ILeaveMgmt.cs               # Leave Management interface
├── ConfigModels/                    # Configuration classes
│   └── AppConfigs.cs               # App configuration with IConfigValidator
├── Constants/                       # Static constants
│   ├── ErrorConstants.cs           # Error codes (OFC_LEVCRT_*)
│   ├── InfoConstants.cs            # Success messages
│   └── OperationNames.cs           # Operation name constants
├── DTO/                            # Data Transfer Objects
│   ├── CreateLeaveDTO/             # API-level DTOs
│   │   ├── CreateLeaveReqDTO.cs   # Request from D365 (IRequestSysDTO)
│   │   └── CreateLeaveResDTO.cs   # Response to D365
│   ├── AtomicHandlerDTOs/          # Atomic Handler DTOs (FLAT)
│   │   └── CreateLeaveHandlerReqDTO.cs  # Oracle Fusion format (IDownStreamRequestDTO)
│   └── DownstreamDTOs/             # External API responses
│       └── CreateLeaveApiResDTO.cs # Oracle Fusion response
├── Functions/                       # Azure Functions (HTTP entry points)
│   └── CreateLeaveAPI.cs           # [Function("CreateLeave")]
├── Implementations/OracleFusion/    # Vendor-specific implementations
│   ├── Services/                   # Service implementations
│   │   └── LeaveMgmtService.cs    # Implements ILeaveMgmt
│   ├── Handlers/                   # Orchestration handlers
│   │   └── CreateLeaveHandler.cs  # Orchestrates Atomic Handler
│   └── AtomicHandlers/             # Single-operation handlers (FLAT)
│       └── CreateLeaveAtomicHandler.cs  # Single HTTP POST
├── Program.cs                       # DI and middleware configuration
├── host.json                        # Azure Functions runtime config
├── appsettings.json                # Placeholder config (CI/CD replaces)
├── appsettings.dev.json            # Development config
├── appsettings.qa.json             # QA config
└── appsettings.prod.json           # Production config
```

## API Endpoints

### Create Leave

**Endpoint:** `POST /api/leave/create`  
**Function Name:** `CreateLeave`  
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

**Success Response (HTTP 200):**
```json
{
  "message": "Leave entry created successfully in Oracle Fusion HCM",
  "errorCode": null,
  "data": {
    "status": "success",
    "message": "Data successfully sent to Oracle Fusion",
    "personAbsenceEntryId": 12345,
    "success": "true"
  }
}
```

**Error Response (HTTP 400/500):**
```json
{
  "message": "Failed to create leave entry in Oracle Fusion HCM",
  "errorCode": "OFC_LEVCRT_0001",
  "data": null,
  "errorDetails": {
    "errorCode": "OFC_LEVCRT_0001",
    "message": "Failed to create leave entry in Oracle Fusion HCM",
    "details": ["Response: {error details}"]
  }
}
```

## Sequence Diagram

```
┌─────────┐         ┌──────────────┐         ┌─────────────────┐         ┌──────────────────┐         ┌─────────────────────┐         ┌──────────────────┐
│  D365   │         │ CreateLeave  │         │  LeaveMgmt      │         │  CreateLeave     │         │  CreateLeave        │         │  Oracle Fusion   │
│ (Client)│         │     API      │         │   Service       │         │    Handler       │         │  AtomicHandler      │         │      HCM         │
└────┬────┘         └──────┬───────┘         └────────┬────────┘         └────────┬─────────┘         └──────────┬──────────┘         └────────┬─────────┘
     │                     │                          │                           │                              │                            │
     │ POST /api/leave/    │                          │                           │                              │                            │
     │ create              │                          │                           │                              │                            │
     ├────────────────────>│                          │                           │                              │                            │
     │                     │                          │                           │                              │                            │
     │                     │ 1. ReadBodyAsync         │                           │                              │                            │
     │                     │    <CreateLeaveReqDTO>   │                           │                              │                            │
     │                     │                          │                           │                              │                            │
     │                     │ 2. Validate()            │                           │                              │                            │
     │                     │                          │                           │                              │                            │
     │                     │ 3. CreateLeave(request)  │                           │                              │                            │
     │                     ├─────────────────────────>│                           │                              │                            │
     │                     │                          │                           │                              │                            │
     │                     │                          │ 4. HandleAsync(request)   │                              │                            │
     │                     │                          ├──────────────────────────>│                              │                            │
     │                     │                          │                           │                              │                            │
     │                     │                          │                           │ 5. Transform D365 -> Oracle  │                            │
     │                     │                          │                           │    - employeeNumber ->       │                            │
     │                     │                          │                           │      personNumber            │                            │
     │                     │                          │                           │    - absenceStatusCode ->    │                            │
     │                     │                          │                           │      absenceStatusCd         │                            │
     │                     │                          │                           │                              │                            │
     │                     │                          │                           │ 6. Handle(atomicRequest)     │                            │
     │                     │                          │                           ├─────────────────────────────>│                            │
     │                     │                          │                           │                              │                            │
     │                     │                          │                           │                              │ 7. Build request body      │
     │                     │                          │                           │                              │    (Oracle Fusion format)  │
     │                     │                          │                           │                              │                            │
     │                     │                          │                           │                              │ 8. Create Basic Auth       │
     │                     │                          │                           │                              │    header                  │
     │                     │                          │                           │                              │                            │
     │                     │                          │                           │                              │ 9. POST /hcmRestApi/       │
     │                     │                          │                           │                              │    resources/11.13.18.05/  │
     │                     │                          │                           │                              │    absences                │
     │                     │                          │                           │                              ├───────────────────────────>│
     │                     │                          │                           │                              │                            │
     │                     │                          │                           │                              │ 10. Response (HTTP 201)    │
     │                     │                          │                           │                              │     personAbsenceEntryId   │
     │                     │                          │                           │                              │<───────────────────────────┤
     │                     │                          │                           │                              │                            │
     │                     │                          │                           │ 11. HttpResponseSnapshot     │                            │
     │                     │                          │                           │<─────────────────────────────┤                            │
     │                     │                          │                           │                              │                            │
     │                     │                          │                           │ 12. Check IsSuccessStatusCode│                            │
     │                     │                          │                           │                              │                            │
     │                     │                          │                           │ 13. Deserialize ApiResDTO    │                            │
     │                     │                          │                           │                              │                            │
     │                     │                          │                           │ 14. Map ApiResDTO -> ResDTO  │                            │
     │                     │                          │                           │                              │                            │
     │                     │                          │ 15. BaseResponseDTO       │                              │                            │
     │                     │                          │<──────────────────────────┤                              │                            │
     │                     │                          │                           │                              │                            │
     │                     │ 16. BaseResponseDTO      │                           │                              │                            │
     │                     │<─────────────────────────┤                           │                              │                            │
     │                     │                          │                           │                              │                            │
     │ 17. Response        │                          │                           │                              │                            │
     │     (HTTP 200)      │                          │                           │                              │                            │
     │<────────────────────┤                          │                           │                              │                            │
     │                     │                          │                           │                              │                            │
```

## Component Flow

```
Function → Service → Handler → Atomic Handler → External API
   ↓          ↓          ↓            ↓              ↓
CreateLeaveAPI → LeaveMgmtService → CreateLeaveHandler → CreateLeaveAtomicHandler → Oracle Fusion HCM
```

## Authentication

**Pattern:** Credentials-per-request (NO middleware)

- **Type:** Basic Auth
- **Username:** Configured in `AppConfigs.OracleFusionUsername`
- **Password:** Configured in `AppConfigs.OracleFusionPassword` (retrieve from KeyVault)
- **Header:** `Authorization: Basic {base64(username:password)}`
- **Location:** Added in Atomic Handler (NOT middleware)

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

### Environment-Specific Configuration

- **appsettings.json:** Placeholder (CI/CD replaces with environment-specific)
- **appsettings.dev.json:** Development environment
- **appsettings.qa.json:** QA environment
- **appsettings.prod.json:** Production environment

**CRITICAL:** All environment files have identical structure (same keys), only values differ.

## Middleware

**Registered Middleware (STRICT ORDER):**

1. **ExecutionTimingMiddleware** (FIRST) - Tracks request timing
2. **ExceptionHandlerMiddleware** (SECOND) - Normalizes exceptions to BaseResponseDTO

**NO CustomAuthenticationMiddleware** - Basic Auth credentials added per request in Atomic Handler

## Error Codes

| Error Code | Message | HTTP Status |
|---|---|---|
| OFC_LEVCRT_0001 | Failed to create leave entry in Oracle Fusion HCM | 400/500 |
| OFC_LEVCRT_0002 | Oracle Fusion HCM returned empty response body | 500 |
| OFC_LEVCRT_0003 | Oracle Fusion HCM authentication failed | 401 |
| OFC_LEVCRT_0004 | Invalid leave data provided | 400 |

**Format:** `AAA_AAAAAA_DDDD`
- AAA: OFC (Oracle Fusion Cloud)
- AAAAAA: LEVCRT (Leave Create)
- DDDD: 0001, 0002, etc.

## Field Mappings

### D365 → Oracle Fusion Transformation

Based on Boomi map_c426b4d6 (Leave Create Map):

| D365 Field | Oracle Fusion Field | Notes |
|---|---|---|
| employeeNumber | personNumber | Field name change |
| absenceStatusCode | absenceStatusCd | Code → Cd |
| approvalStatusCode | approvalStatusCd | Code → Cd |
| absenceType | absenceType | Same |
| employer | employer | Same |
| startDate | startDate | Same |
| endDate | endDate | Same |
| startDateDuration | startDateDuration | Same |
| endDateDuration | endDateDuration | Same |

## Dependencies

### Framework References

- **Core Framework:** `/Framework/Core/Core/Core.csproj`
- **Cache Framework:** `/Framework/Cache/Cache.csproj`

### NuGet Packages

- Microsoft.Azure.Functions.Worker (1.21.0)
- Microsoft.Azure.Functions.Worker.Sdk (1.17.0)
- Microsoft.Azure.Functions.Worker.Extensions.Http (3.1.0)
- Microsoft.ApplicationInsights.WorkerService (2.22.0)
- Polly (8.3.1)

## Deployment

### Prerequisites

1. Azure Function App (v4, .NET 8)
2. Application Insights
3. Azure KeyVault (for Oracle Fusion password)
4. Environment variables configured

### Environment Variables

- `ENVIRONMENT` or `ASPNETCORE_ENVIRONMENT`: dev/qa/prod
- `APPLICATIONINSIGHTS_CONNECTION_STRING`: Application Insights connection

### Configuration Steps

1. Deploy Azure Function App
2. Configure Application Settings:
   - `AppConfigs:OracleFusionBaseUrl`
   - `AppConfigs:OracleFusionUsername`
   - `AppConfigs:OracleFusionPassword` (from KeyVault)
   - `AppConfigs:LeaveResourcePath`
3. Enable Application Insights
4. Configure CORS if needed

## Testing

### Local Testing

```bash
# Restore dependencies
dotnet restore

# Build project
dotnet build

# Run locally (requires local.settings.json)
func start
```

### Sample Request (cURL)

```bash
curl -X POST https://<function-app>.azurewebsites.net/api/leave/create \
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

## Monitoring

### Application Insights Metrics

- **Request Duration:** Tracked by ExecutionTimingMiddleware
- **Success Rate:** HTTP 200 responses
- **Error Rate:** HTTP 400/500 responses
- **Downstream Timing:** `DSTimeBreakDown` header shows Oracle Fusion API timing

### Log Queries

```kusto
// Failed leave creation requests
traces
| where message contains "Oracle Fusion HCM API failed"
| project timestamp, message, severityLevel

// Successful leave creation
traces
| where message contains "Completed Create Leave"
| project timestamp, message
```

## Troubleshooting

### Common Issues

1. **401 Unauthorized**
   - Check Oracle Fusion username/password in AppConfigs
   - Verify credentials in KeyVault
   - Ensure Basic Auth header is correctly formatted

2. **400 Bad Request**
   - Validate request body matches required schema
   - Check field name transformations (employeeNumber → personNumber)
   - Verify date format (YYYY-MM-DD)

3. **500 Internal Server Error**
   - Check Oracle Fusion API availability
   - Verify resource path: `hcmRestApi/resources/11.13.18.05/absences`
   - Check Application Insights for detailed error logs

## Design Decisions

### Function Exposure Decision

Based on Function Exposure Decision Table (BOOMI_EXTRACTION_PHASE1.md Section 18):

| Operation | Independent Invoke? | Decision Before/After? | Same SOR? | Internal Lookup? | Conclusion | Reasoning |
|---|---|---|---|---|---|---|
| Create Leave | YES | NO | N/A | NO | **Azure Function** | Complete business operation Process Layer needs |

**Result:** 1 Azure Function (`CreateLeave`)

**Reasoning:**
- Process Layer can invoke independently (synchronous request-response)
- No decision shapes before/after (only error handling)
- Complete business operation (create leave in Oracle Fusion)
- NOT an internal lookup or field extraction

### Authentication Pattern

**Pattern:** Credentials-per-request (NO middleware)

**Reasoning:**
- Oracle Fusion uses Basic Auth
- No session/token lifecycle
- Credentials sent with every request
- NO Login/Logout operations needed

**Implementation:**
- Username/Password configured in AppConfigs
- Basic Auth header created in Atomic Handler
- NO CustomAuthenticationMiddleware
- NO AuthenticateAtomicHandler/LogoutAtomicHandler

## Compliance

This implementation follows:

- **System-Layer-Rules.mdc:** All architecture rules
- **API-Led Architecture:** System Layer principles
- **Azure Functions v4:** .NET 8 Isolated Worker Model
- **Framework Integration:** Core and Cache framework references

## Related Documentation

- **BOOMI_EXTRACTION_PHASE1.md:** Complete Boomi process analysis
- **System-Layer-Rules.mdc:** Architecture rules and patterns
- **Framework/Core:** Core framework documentation
- **Framework/Cache:** Cache framework documentation

## Support

For issues or questions, contact the integration team.
