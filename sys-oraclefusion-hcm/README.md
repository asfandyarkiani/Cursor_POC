# Oracle Fusion HCM System Layer

**Purpose:** System Layer API for Oracle Fusion HCM absence/leave management operations  
**SOR:** Oracle Fusion HCM (REST API)  
**Domain:** Human Resources - Leave Management  
**Architecture:** API-Led Architecture - System Layer

---

## Overview

This System Layer provides atomic operations for interacting with Oracle Fusion HCM, specifically for absence/leave record creation. It insulates Process Layer from Oracle Fusion HCM API complexity and provides reusable "Lego block" operations.

---

## Architecture

### Layer Responsibilities

**System Layer (This Project):**
- Unlock data from Oracle Fusion HCM
- Handle Oracle Fusion-specific authentication (Basic Auth)
- Transform Oracle Fusion API responses to standardized format
- Insulate consumers from Oracle Fusion API changes

**Process Layer (Consumer):**
- Orchestrate leave creation workflow
- Aggregate data from multiple System APIs (D365, Oracle Fusion, etc.)
- Implement business domain logic
- Handle cross-SOR orchestration

**Experience Layer:**
- Reconfigure data for UI/UX needs
- Mobile, Web, IoT specific formatting

---

## API Operations

### 1. Create Absence

**Endpoint:** `POST /api/hcm/absence/create`  
**Function:** `CreateAbsence`  
**Purpose:** Create absence/leave record in Oracle Fusion HCM

**Request (from Process Layer):**
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

**Response (to Process Layer):**
```json
{
  "message": "Absence created successfully in Oracle Fusion HCM.",
  "errorCode": null,
  "data": {
    "personAbsenceEntryId": 300100123456789,
    "absenceType": "Sick Leave",
    "startDate": "2024-03-24",
    "endDate": "2024-03-25",
    "absenceStatusCd": "SUBMITTED",
    "approvalStatusCd": "APPROVED"
  },
  "errorDetails": null,
  "isDownStreamError": false,
  "isPartialSuccess": false
}
```

**Oracle Fusion HCM API Call:**
- **Method:** POST
- **Endpoint:** `/hcmRestApi/resources/11.13.18.05/absences`
- **Authentication:** Basic Auth (username/password from KeyVault)
- **Field Mappings:**
  - `employeeNumber` → `personNumber`
  - `absenceStatusCode` → `absenceStatusCd`
  - `approvalStatusCode` → `approvalStatusCd`

---

## Sequence Diagram

```
Process Layer                System Layer                  Oracle Fusion HCM
     |                            |                                |
     |--POST /api/hcm/absence/--->|                                |
     |   create                   |                                |
     |   (CreateAbsenceReqDTO)    |                                |
     |                            |                                |
     |                    [CreateAbsenceAPI]                        |
     |                            |                                |
     |                    Validate Request                         |
     |                    (ValidateAPIRequestParameters)           |
     |                            |                                |
     |                    Delegate to Service                      |
     |                            |                                |
     |                    [AbsenceMgmtService]                     |
     |                            |                                |
     |                    Delegate to Handler                      |
     |                            |                                |
     |                    [CreateAbsenceHandler]                   |
     |                            |                                |
     |                    Transform DTO                            |
     |                    (CreateAbsenceHandlerReqDTO)             |
     |                            |                                |
     |                    Call Atomic Handler                      |
     |                            |                                |
     |                    [CreateAbsenceAtomicHandler]             |
     |                            |                                |
     |                    Get Credentials (KeyVault)               |
     |                            |                                |
     |                    Build Request Body                       |
     |                    (Map field names)                        |
     |                            |                                |
     |                    Build URL                                |
     |                            |                                |
     |                            |--POST /hcmRestApi/resources--->|
     |                            |   /11.13.18.05/absences        |
     |                            |   (Basic Auth)                 |
     |                            |   {                            |
     |                            |     personNumber: 9000604,     |
     |                            |     absenceType: "Sick Leave", |
     |                            |     absenceStatusCd: "SUBMITTED"|
     |                            |     ...                        |
     |                            |   }                            |
     |                            |                                |
     |                            |<--HTTP 200/201 (Success)-------|
     |                            |   {                            |
     |                            |     personAbsenceEntryId: ..., |
     |                            |     absenceType: "Sick Leave", |
     |                            |     ...                        |
     |                            |   }                            |
     |                            |                                |
     |                    Return HttpResponseSnapshot              |
     |                            |                                |
     |                    Check IsSuccessStatusCode                |
     |                            |                                |
     |                    IF Success:                              |
     |                      Deserialize ApiResDTO                  |
     |                      Map to ResDTO                          |
     |                      Return BaseResponseDTO                 |
     |                            |                                |
     |                    IF Error:                                |
     |                      Throw DownStreamApiFailureException    |
     |                            |                                |
     |<--BaseResponseDTO----------|                                |
     |   (CreateAbsenceResDTO)    |                                |
     |                            |                                |

[Middleware Layer]
ExecutionTimingMiddleware: Tracks total execution time
ExceptionHandlerMiddleware: Normalizes exceptions to BaseResponseDTO
```

---

## Error Handling

### Error Scenarios

**1. Missing Request Body**
- **Exception:** `NoRequestBodyException`
- **HTTP Status:** 400
- **Thrown By:** CreateAbsenceAPI.cs
- **Message:** "Request body is missing or empty"

**2. Validation Failure**
- **Exception:** `RequestValidationFailureException`
- **HTTP Status:** 400
- **Thrown By:** CreateAbsenceReqDTO.cs, CreateAbsenceHandlerReqDTO.cs
- **Message:** List of validation errors

**3. Oracle Fusion API Failure**
- **Exception:** `DownStreamApiFailureException`
- **HTTP Status:** Varies (from Oracle Fusion)
- **Thrown By:** CreateAbsenceHandler.cs
- **Error Code:** `OFH_ABSCRT_0001`
- **Message:** "Failed to create absence in Oracle Fusion HCM."

**4. Empty Response Body**
- **Exception:** `NoResponseBodyException`
- **HTTP Status:** 500
- **Thrown By:** CreateAbsenceHandler.cs
- **Message:** "Oracle Fusion HCM returned empty response body"

**5. Deserialization Failure**
- **Exception:** `DownStreamApiFailureException`
- **HTTP Status:** 500
- **Thrown By:** CreateAbsenceHandler.cs
- **Error Code:** `OFH_ABSCRT_0003`
- **Message:** "Failed to deserialize Oracle Fusion HCM response"

---

## Configuration

### Environment Variables

**ENVIRONMENT** or **ASPNETCORE_ENVIRONMENT**
- Values: `dev`, `qa`, `prod`
- Determines which appsettings file to load

### appsettings.json Structure

```json
{
  "AppConfigs": {
    "BaseApiUrl": "https://iaaxey-{env}.fa.ocs.oraclecloud.com:443",
    "AbsencesResourcePath": "hcmRestApi/resources/11.13.18.05/absences",
    "Username": "{env}_user",
    "Password": "",
    "TimeoutSeconds": 50,
    "RetryCount": 0
  },
  "KeyVault": {
    "Url": "https://{env}-keyvault.vault.azure.net/",
    "Secrets": {
      "Password": "OracleFusionHcmPassword"
    }
  },
  "RedisCache": {
    "ConnectionString": "{env}-redis-connection-string",
    "InstanceName": "OracleFusionHcm:"
  },
  "HttpClientPolicy": {
    "RetryCount": 0,
    "TimeoutSeconds": 60
  }
}
```

### Azure KeyVault Secrets

**Required Secrets:**
- `OracleFusionHcmPassword` - Oracle Fusion HCM password for Basic Auth

**Configuration:**
- Secrets retrieved at runtime via KeyVaultReader
- Uses DefaultAzureCredential (Managed Identity in Azure)
- Secret caching with thread-safe SemaphoreSlim

---

## Dependencies

### NuGet Packages
- Microsoft.Azure.Functions.Worker 1.21.0
- Microsoft.Azure.Functions.Worker.Sdk 1.17.0
- Microsoft.Azure.Functions.Worker.Extensions.Http 3.1.0
- Microsoft.Azure.Functions.Worker.Extensions.Http.AspNetCore 1.2.0
- Microsoft.ApplicationInsights.WorkerService 2.22.0
- Microsoft.Azure.Functions.Worker.ApplicationInsights 1.2.0
- Polly 8.3.1
- Azure.Identity 1.11.0
- Azure.Security.KeyVault.Secrets 4.6.0

### Framework References
- Framework/Core/Core/Core.csproj
- Framework/Cache/Cache.csproj

---

## Component Flow

```
CreateAbsenceAPI (Function)
    ↓ (inject IAbsenceMgmt)
AbsenceMgmtService (Service)
    ↓ (inject CreateAbsenceHandler)
CreateAbsenceHandler (Handler)
    ↓ (inject CreateAbsenceAtomicHandler)
CreateAbsenceAtomicHandler (Atomic Handler)
    ↓ (HTTP POST)
Oracle Fusion HCM API
```

---

## Field Mappings

### Request Transformation (D365 → Oracle Fusion)

| D365 Field | Oracle Fusion Field | Transformation |
|---|---|---|
| employeeNumber | personNumber | Field name change |
| absenceType | absenceType | Direct mapping |
| employer | employer | Direct mapping |
| startDate | startDate | Direct mapping |
| endDate | endDate | Direct mapping |
| absenceStatusCode | absenceStatusCd | Field name change (Code → Cd) |
| approvalStatusCode | approvalStatusCd | Field name change (Code → Cd) |
| startDateDuration | startDateDuration | Direct mapping |
| endDateDuration | endDateDuration | Direct mapping |

**Note:** Map field names are AUTHORITATIVE (from Boomi Map Analysis Step 1d)

---

## Deployment

### Prerequisites
1. Azure Function App configured
2. Azure KeyVault with required secrets
3. Redis Cache instance
4. Application Insights instance
5. Managed Identity enabled for KeyVault access

### Environment Configuration
1. Set `ENVIRONMENT` or `ASPNETCORE_ENVIRONMENT` variable
2. Configure Application Insights connection string
3. Configure Redis connection string
4. Add Oracle Fusion HCM password to KeyVault

### CI/CD Pipeline
- Build validation via GitHub Actions
- Automated deployment to Azure Function App
- Environment-specific configuration via appsettings.{env}.json

---

## Testing

### Unit Testing
- Test DTOs validation logic
- Test field mapping transformations
- Test error handling scenarios

### Integration Testing
- Test Oracle Fusion HCM API connectivity
- Test KeyVault secret retrieval
- Test end-to-end absence creation flow

### Manual Testing
```bash
curl -X POST https://{function-app}.azurewebsites.net/api/hcm/absence/create \
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

---

## Monitoring

### Application Insights
- Request/response logging
- Exception tracking
- Performance metrics
- Live metrics stream

### Logging Levels
- **Info:** Function entry, operation start/completion
- **Error:** API failures, exceptions
- **Warn:** Retries, recoverable errors
- **Debug:** Detailed execution flow

### Performance Tracking
- ExecutionTimingMiddleware tracks total execution time
- CustomRestClient tracks individual operation timing
- Metrics available in Application Insights

---

## Maintenance

### Adding New Operations
1. Create new *ReqDTO and *ResDTO in `DTO/<Operation>DTO/`
2. Create *HandlerReqDTO in `DTO/AtomicHandlerDTOs/`
3. Create *ApiResDTO in `DTO/DownstreamDTOs/`
4. Create Atomic Handler in `Implementations/OracleFusion/AtomicHandlers/`
5. Create Handler in `Implementations/OracleFusion/Handlers/`
6. Add method to `IAbsenceMgmt` interface
7. Implement method in `AbsenceMgmtService`
8. Create Azure Function in `Functions/`
9. Register components in `Program.cs`

### Updating Configuration
- Update `AppConfigs.cs` for new configuration properties
- Add properties to all appsettings.{env}.json files
- Update validation logic in `validate()` method

### Error Codes
- Add new error constants to `ErrorConstants.cs`
- Follow format: `OFH_<OPERATION>_<NUMBER>`
- Example: `OFH_ABSUPD_0001` for absence update errors

---

## Support

### Troubleshooting

**Issue:** "Failed to create absence in Oracle Fusion HCM"
- Check Oracle Fusion HCM API availability
- Verify credentials in KeyVault
- Check network connectivity
- Review Application Insights logs

**Issue:** "KeyVault secret not found"
- Verify secret name: `OracleFusionHcmPassword`
- Check Managed Identity permissions
- Verify KeyVault URL in appsettings

**Issue:** "Request validation failed"
- Review validation error details in response
- Check required fields are provided
- Verify date format (YYYY-MM-DD)

### Logs Location
- Application Insights: Azure Portal
- Console logs: Azure Function App logs
- File logs: Azure Function App file system

---

## Architecture Compliance

This System Layer implementation follows:
- ✅ System-Layer-Rules.mdc (100% compliance)
- ✅ API-Led Architecture principles
- ✅ .NET 8 Azure Functions v4 Isolated Worker Model
- ✅ Framework/Core and Framework/Cache patterns
- ✅ Folder structure conventions
- ✅ Naming conventions
- ✅ Exception handling patterns
- ✅ Configuration management patterns
- ✅ Logging patterns

**Compliance Report:** See `RULEBOOK_COMPLIANCE_REPORT.md` for detailed audit

---

## Contact

**Integration Team:** BoomiIntegrationTeam@al-ghurair.com  
**Domain:** Human Resources  
**SOR:** Oracle Fusion HCM

---

**Generated by:** Cloud Agent 2 (System Layer Code Generation)  
**Date:** 2026-02-16  
**Based on:** BOOMI_EXTRACTION_PHASE1.md (HCM Leave Create process)
