# Oracle Fusion HCM System Layer

## Overview

This System Layer provides integration with Oracle Fusion HCM (Human Capital Management) system for absence management operations. It follows API-Led Architecture principles to unlock data from Oracle Fusion HCM and insulate consumers from the complexity of the underlying system.

## Business Domain

**Domain:** Human Resource (HCM)  
**System of Record:** Oracle Fusion HCM  
**Purpose:** Create and manage employee absence entries

## Architecture

This project follows the System Layer architecture pattern:

```
Azure Function (HTTP Entry Point)
    ↓
Service (IAbsenceMgmt → AbsenceMgmtService)
    ↓
Handler (CreateAbsenceHandler)
    ↓
Atomic Handler (CreateAbsenceAtomicHandler)
    ↓
Oracle Fusion HCM REST API
```

### Components

- **Functions/**: HTTP-triggered Azure Functions (Process Layer entry points)
- **Abstractions/**: Service interfaces (IAbsenceMgmt)
- **Services/**: Service implementations (AbsenceMgmtService)
- **Handlers/**: Operation orchestration (CreateAbsenceHandler)
- **AtomicHandlers/**: Single external API calls (CreateAbsenceAtomicHandler)
- **DTO/**: Data transfer objects (Request, Response, Handler, API response)
- **ConfigModels/**: Configuration classes (AppConfigs, KeyVaultConfigs)
- **Constants/**: Error codes, info messages, operation names
- **Helper/**: Utility classes (KeyVaultReader, RestApiHelper)

## API Endpoints

### Create Absence

**Endpoint:** `POST /api/hcm/absences`  
**Function Name:** CreateAbsence  
**Purpose:** Create a new absence entry in Oracle Fusion HCM

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
  "message": "Absence entry created successfully in Oracle Fusion HCM",
  "errorCode": null,
  "data": {
    "status": "success",
    "message": "Data successfully sent to Oracle Fusion",
    "personAbsenceEntryId": 300000123456789,
    "success": "true"
  },
  "errorDetails": null,
  "isDownStreamError": false
}
```

**Error Response (HTTP 400/500):**
```json
{
  "message": "Failed to create absence entry in Oracle Fusion HCM",
  "errorCode": "OFH_ABSCRT_0001",
  "data": null,
  "errorDetails": ["Oracle Fusion HCM returned status 400. Response: ..."],
  "isDownStreamError": true
}
```

## Configuration

### Environment Variables

Configuration is managed through environment-specific appsettings files:

- `appsettings.json` - Placeholder (replaced by CI/CD)
- `appsettings.dev.json` - Development environment
- `appsettings.qa.json` - QA environment
- `appsettings.prod.json` - Production environment

### AppConfigs

```json
{
  "AppConfigs": {
    "OracleFusionBaseUrl": "https://iaaxey-dev3.fa.ocs.oraclecloud.com:443",
    "OracleFusionAbsencesResourcePath": "hcmRestApi/resources/11.13.18.05/absences",
    "TimeoutSeconds": 50,
    "RetryCount": 0
  }
}
```

### KeyVault Configuration

Credentials are stored securely in Azure KeyVault:

```json
{
  "KeyVault": {
    "Url": "https://dev-keyvault.vault.azure.net/",
    "Secrets": {
      "OracleFusionUsername": "OracleFusionHCMUsername",
      "OracleFusionPassword": "OracleFusionHCMPassword"
    }
  }
}
```

**Required Secrets in KeyVault:**
- `OracleFusionHCMUsername` - Oracle Fusion HCM username
- `OracleFusionHCMPassword` - Oracle Fusion HCM password

### Redis Cache Configuration

```json
{
  "RedisCache": {
    "ConnectionString": "your-redis-connection-string",
    "InstanceName": "OracleFusionHCM:"
  }
}
```

## Authentication

**Pattern:** Credentials-per-request (Basic Authentication)

- Credentials retrieved from Azure KeyVault on each request
- Basic Auth header added in CreateAbsenceAtomicHandler
- No middleware authentication (no session/token lifecycle)
- Secure credential storage in KeyVault

## Error Handling

### Error Codes

All error codes follow System Layer format: `AAA_AAAAAA_DDDD`

- `OFH_ABSCRT_0001` - Failed to create absence entry
- `OFH_ABSCRT_0002` - Oracle Fusion returned error response
- `OFH_ABSCRT_0003` - Failed to decompress gzip response
- `OFH_ABSCRT_0004` - No response body received
- `OFH_AUTHEN_0001` - Failed to retrieve credentials from KeyVault

### Exception Flow

```
Atomic Handler (returns HttpResponseSnapshot)
    ↓
Handler (checks status, throws DownStreamApiFailureException)
    ↓
Middleware (catches, normalizes to BaseResponseDTO)
    ↓
Client (receives BaseResponseDTO with error details)
```

## Sequence Diagram

```
Process Layer
    |
    | POST /api/hcm/absences
    | (CreateAbsenceReqDTO)
    ↓
CreateAbsenceAPI (Function)
    |
    | Validate request
    | Delegate to service
    ↓
IAbsenceMgmt (Interface)
    ↓
AbsenceMgmtService
    |
    | Delegate to handler
    ↓
CreateAbsenceHandler
    |
    | Transform API DTO → Atomic DTO
    | (employeeNumber → personNumber)
    | (absenceStatusCode → absenceStatusCd)
    ↓
CreateAbsenceAtomicHandler
    |
    | Get credentials from KeyVault
    | Build request body
    | POST to Oracle Fusion HCM
    ↓
Oracle Fusion HCM API
    |
    | POST /hcmRestApi/resources/11.13.18.05/absences
    | Basic Auth
    ↓
Response (HTTP 200/400/500)
    ↓
CreateAbsenceAtomicHandler
    |
    | Return HttpResponseSnapshot
    ↓
CreateAbsenceHandler
    |
    | Check IsSuccessStatusCode
    | If success: Deserialize → Map → Return
    | If error: Throw DownStreamApiFailureException
    ↓
ExceptionHandlerMiddleware
    |
    | Normalize to BaseResponseDTO
    ↓
Process Layer
    |
    | Receive BaseResponseDTO
```

## Field Mapping

### Request Transformation (D365 → Oracle Fusion)

| D365 Field | Oracle Fusion Field | Transformation |
|---|---|---|
| employeeNumber | personNumber | ToString() |
| absenceStatusCode | absenceStatusCd | Direct mapping |
| approvalStatusCode | approvalStatusCd | Direct mapping |
| absenceType | absenceType | Direct mapping |
| employer | employer | Direct mapping |
| startDate | startDate | Direct mapping |
| endDate | endDate | Direct mapping |
| startDateDuration | startDateDuration | Direct mapping |
| endDateDuration | endDateDuration | Direct mapping |

**Note:** Field name mappings are based on Boomi map analysis (Step 1d) - map field names are AUTHORITATIVE.

### Response Transformation (Oracle Fusion → D365)

| Oracle Fusion Field | D365 Field | Transformation |
|---|---|---|
| personAbsenceEntryId | personAbsenceEntryId | Direct mapping |
| (static) | status | "success" or "failure" |
| (static) | message | Success/error message |
| (static) | success | "true" or "false" |

## Dependencies

### Framework References

- **Framework/Core** - Core framework (CustomHTTPClient, CustomRestClient, exceptions, extensions)
- **Framework/Cache** - Caching framework (Redis integration)

### NuGet Packages

- Microsoft.Azure.Functions.Worker (1.21.0)
- Microsoft.Azure.Functions.Worker.Sdk (1.17.0)
- Microsoft.Azure.Functions.Worker.Extensions.Http (3.1.0)
- Microsoft.ApplicationInsights.WorkerService (2.22.0)
- Polly (8.3.1)
- Azure.Identity (1.11.0)
- Azure.Security.KeyVault.Secrets (4.6.0)

## Development

### Build

```bash
dotnet restore
dotnet build
```

### Run Locally

```bash
func start
```

### Test Endpoint

```bash
curl -X POST http://localhost:7071/api/hcm/absences \
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

### Azure Function App Settings

Required application settings in Azure:

- `ENVIRONMENT` or `ASPNETCORE_ENVIRONMENT` - Environment name (dev/qa/prod)
- `APPLICATIONINSIGHTS_CONNECTION_STRING` - Application Insights connection string
- KeyVault URL and secrets configured in appsettings.{env}.json

### CI/CD Pipeline

- Pipeline replaces `appsettings.json` with environment-specific file
- Deploys to Azure Function App
- Configures Application Insights and KeyVault access

## Monitoring

### Application Insights

- Live metrics enabled in host.json
- Request tracking with ExecutionTimingMiddleware
- Performance breakdown in ResponseHeaders.DSTimeBreakDown
- Exception tracking with ExceptionHandlerMiddleware

### Logging

All logs use Core Framework LoggerExtensions:

- `_logger.Info()` - Function entry, major operations
- `_logger.Error()` - Errors with exception details
- `_logger.Warn()` - Warnings
- `_logger.Debug()` - Detailed debugging

## Compliance

This implementation follows:

- ✅ System Layer Rules (.cursor/rules/System-Layer-Rules.mdc)
- ✅ API-Led Architecture principles
- ✅ .NET 8 Azure Functions v4 isolated worker model
- ✅ Framework Core and Cache integration
- ✅ Credentials-per-request authentication pattern
- ✅ Proper error handling and exception normalization
- ✅ Non-breaking, additive changes

## Source Process

**Boomi Process:** HCM_Leave Create  
**Process ID:** ca69f858-785f-4565-ba1f-b4edc6cca05b  
**Analysis Document:** BOOMI_EXTRACTION_PHASE1.md

## Contact

For questions or issues, contact the Boomi Integration Team.
