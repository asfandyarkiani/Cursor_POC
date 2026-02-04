# System Layer: Oracle Fusion HCM - Leave Management

**Project:** sys-oraclefusionhcm-mgmt  
**System of Record:** Oracle Fusion HCM  
**Business Domain:** Human Resource Management (HCM) - Leaves  
**Framework:** .NET 8, Azure Functions v4 (Isolated Worker)  

---

## Overview

This System Layer project provides integration with Oracle Fusion HCM for leave/absence management operations. It exposes HTTP endpoints that Process Layer or D365 can call to create leave records in Oracle Fusion HCM.

### Key Features

- ✅ Create leave/absence records in Oracle Fusion HCM
- ✅ Transform D365 leave requests to Oracle HCM format
- ✅ Handle HTTP status codes and error responses
- ✅ Support gzip-compressed error responses
- ✅ Basic Authentication with Oracle Fusion HCM
- ✅ Retry and circuit breaker policies with Polly
- ✅ Comprehensive logging with Application Insights
- ✅ Environment-specific configuration (DEV, QA, PROD)

---

## Architecture

### API-Led Architecture Layers

This project implements the **System Layer** of the API-Led Architecture:

- **System Layer (This Project):** Unlocks data from Oracle Fusion HCM and insulates consumers from SOR complexity
- **Process Layer (Not Implemented):** Will orchestrate business logic and call System Layer APIs
- **Experience Layer (Not Implemented):** Will reconfigure data for specific end-user needs

### Component Structure

```
Azure Function (CreateLeaveAPI)
    ↓
Service (LeaveMgmtService) - Abstraction boundary
    ↓
Handler (CreateLeaveHandler) - Orchestrates transformation
    ↓
Atomic Handler (CreateLeaveAtomicHandler) - Single HTTP POST to Oracle HCM
```

### Middleware Pipeline

```
ExecutionTimingMiddleware → ExceptionHandlerMiddleware → Function
```

---

## API Endpoints

### POST /api/leave/create

Creates a new leave/absence record in Oracle Fusion HCM.

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
  "status": "success",
  "message": "Data successfully sent to Oracle Fusion",
  "personAbsenceEntryId": 123456,
  "success": "true"
}
```

**Error Response (HTTP 400/500):**
```json
{
  "status": "failure",
  "message": "OFH_LVEMGT_0002: Invalid leave request data",
  "personAbsenceEntryId": null,
  "success": "false"
}
```

---

## Configuration

### Environment Variables

Configuration is loaded from `appsettings.{environment}.json` based on `AZURE_FUNCTIONS_ENVIRONMENT`.

**Required Configuration:**

```json
{
  "OracleFusionHCM": {
    "BaseUrl": "https://iaaxey-dev3.fa.ocs.oraclecloud.com:443",
    "AbsencesResourcePath": "hcmRestApi/resources/11.13.18.05/absences",
    "Username": "INTEGRATION.USER@al-ghurair.com",
    "Password": "YOUR_PASSWORD_HERE",
    "TimeoutSeconds": 30
  },
  "HttpClient": {
    "MaxRetryAttempts": 3,
    "RetryDelaySeconds": 2,
    "CircuitBreakerFailureThreshold": 5,
    "CircuitBreakerDurationSeconds": 30
  }
}
```

### Secrets Management

**⚠️ IMPORTANT:** Never commit passwords or secrets to source control.

**Recommended Approach:**
1. Store secrets in Azure Key Vault
2. Reference Key Vault secrets in Azure Function App Configuration
3. Use Managed Identity for Key Vault access

**Example:**
```bash
# Azure Key Vault secret reference
@Microsoft.KeyVault(SecretUri=https://your-keyvault.vault.azure.net/secrets/OracleFusionHCM-Password/)
```

---

## Development Setup

### Prerequisites

- .NET 8 SDK
- Azure Functions Core Tools v4
- Visual Studio 2022 or VS Code with Azure Functions extension

### Local Development

1. **Clone Repository:**
   ```bash
   git clone <repository-url>
   cd sys-oraclefusionhcm-mgmt
   ```

2. **Restore Dependencies:**
   ```bash
   dotnet restore
   ```

3. **Configure Local Settings:**
   Create `local.settings.json` (not committed to source control):
   ```json
   {
     "IsEncrypted": false,
     "Values": {
       "AzureWebJobsStorage": "UseDevelopmentStorage=true",
       "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
       "AZURE_FUNCTIONS_ENVIRONMENT": "Development"
     }
   }
   ```

4. **Update appsettings.dev.json:**
   Replace TODO placeholders with actual values.

5. **Run Locally:**
   ```bash
   func start
   ```

6. **Test Endpoint:**
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

---

## Deployment

### Azure Function App Deployment

1. **Create Azure Function App:**
   ```bash
   az functionapp create \
     --name sys-oraclefusionhcm-mgmt-dev \
     --resource-group rg-systemlayer-dev \
     --consumption-plan-location eastus \
     --runtime dotnet-isolated \
     --runtime-version 8 \
     --functions-version 4 \
     --storage-account stsystemlayerdev
   ```

2. **Configure Application Settings:**
   ```bash
   az functionapp config appsettings set \
     --name sys-oraclefusionhcm-mgmt-dev \
     --resource-group rg-systemlayer-dev \
     --settings \
       "AZURE_FUNCTIONS_ENVIRONMENT=Development" \
       "OracleFusionHCM__BaseUrl=https://iaaxey-dev3.fa.ocs.oraclecloud.com:443" \
       "OracleFusionHCM__Username=INTEGRATION.USER@al-ghurair.com" \
       "OracleFusionHCM__Password=@Microsoft.KeyVault(...)"
   ```

3. **Deploy Function App:**
   ```bash
   func azure functionapp publish sys-oraclefusionhcm-mgmt-dev
   ```

### CI/CD Pipeline

GitHub Actions workflow (`.github/workflows/dotnet-ci.yml`) automatically:
1. Builds the project on push to branch
2. Runs tests (if present)
3. Publishes artifacts
4. Deploys to Azure (if configured)

---

## Error Codes

All error codes follow the format: `OFH_LVEMGT_NNNN`

| Error Code | Description |
|------------|-------------|
| OFH_LVEMGT_0001 | Failed to create leave in Oracle Fusion HCM |
| OFH_LVEMGT_0002 | Invalid leave request data |
| OFH_LVEMGT_0003 | Oracle Fusion HCM API returned error |
| OFH_LVEMGT_0004 | Failed to deserialize Oracle Fusion HCM response |
| OFH_LVEMGT_0005 | Missing required configuration for Oracle Fusion HCM |
| OFH_LVEMGT_0006 | Oracle Fusion HCM authentication failed |
| OFH_LVEMGT_0007 | Oracle Fusion HCM connection timeout |
| OFH_LVEMGT_0008 | Invalid employee number |
| OFH_LVEMGT_0009 | Invalid date range for leave |
| OFH_LVEMGT_0010 | Missing person absence entry ID in response |

---

## Monitoring and Logging

### Application Insights

The project is configured with Application Insights for monitoring and logging.

**Key Metrics:**
- Request count and duration
- Failure rate and error details
- Dependency calls (Oracle Fusion HCM API)
- Custom events and traces

**Log Levels:**
- **Debug:** Request/response payloads (DEV only)
- **Information:** Operation start/completion
- **Warning:** Validation failures, non-critical errors
- **Error:** API failures, exceptions

### Viewing Logs

**Azure Portal:**
1. Navigate to Function App → Monitoring → Logs
2. Query Application Insights with KQL

**Example Query:**
```kql
traces
| where timestamp > ago(1h)
| where message contains "OFH_LVEMGT"
| order by timestamp desc
```

---

## Testing

### Unit Tests (TODO)

Create unit tests for:
- DTO validation logic
- Handler transformation logic
- Atomic Handler HTTP call logic

### Integration Tests (TODO)

Create integration tests for:
- End-to-end API flow
- Oracle Fusion HCM API integration
- Error handling scenarios

### Manual Testing

Use Postman or curl to test the API endpoint:

**Postman Collection:** (TODO - Create Postman collection)

**curl Example:**
```bash
curl -X POST https://sys-oraclefusionhcm-mgmt-dev.azurewebsites.net/api/leave/create \
  -H "Content-Type: application/json" \
  -H "x-functions-key: YOUR_FUNCTION_KEY" \
  -d @test-request.json
```

---

## Troubleshooting

### Common Issues

**Issue:** HTTP 401 Unauthorized from Oracle Fusion HCM
- **Cause:** Invalid credentials or expired password
- **Solution:** Verify username/password in configuration, check Oracle HCM user account

**Issue:** HTTP 500 Internal Server Error
- **Cause:** Configuration missing or invalid
- **Solution:** Check Application Insights logs for error details, verify all required configuration present

**Issue:** Timeout calling Oracle Fusion HCM
- **Cause:** Network issues or Oracle HCM unavailable
- **Solution:** Check Oracle HCM status, verify network connectivity, increase timeout in configuration

**Issue:** Gzip decompression error
- **Cause:** Oracle HCM returned gzip-compressed error response
- **Solution:** Check logs for decompressed error message, verify Oracle HCM error details

---

## Architecture Decisions

### 1. Single Azure Function

**Decision:** Create 1 Azure Function (CreateLeaveAPI) instead of multiple functions.

**Rationale:**
- No business decision logic present in Boomi process
- All decision points are technical concerns (HTTP status, gzip, exceptions)
- Process Layer will orchestrate business logic, not System Layer

### 2. No Authentication Middleware

**Decision:** Use Basic Authentication per-request instead of session/token middleware.

**Rationale:**
- Oracle Fusion HCM uses Basic Authentication (username/password per request)
- No session or token management required
- Credentials added directly in Atomic Handler HTTP headers

### 3. No Email Notifications

**Decision:** Do NOT implement email notification functionality.

**Rationale:**
- Email notifications are cross-cutting concerns, not System Layer responsibility
- System Layer throws exceptions; middleware handles error responses
- Email notifications (if needed) should be handled by Process Layer or monitoring infrastructure

### 4. Handler Orchestration

**Decision:** Handler orchestrates internal Atomic Handler with simple if/else for technical decisions.

**Rationale:**
- All decision points are same-SOR technical concerns (HTTP status checking, response decompression)
- No cross-SOR business decisions
- Handler orchestrates: transformation → HTTP call → response handling

### 5. Framework Extensions

**Decision:** Use Framework extension `ReadBodyAsync<T>()` for request deserialization.

**Rationale:**
- Framework provides optimized extension methods
- Avoids manual deserialization code
- Consistent with System Layer standards

---

## Contributing

### Code Standards

- Follow System Layer Rules (`.cursor/rules/System-Layer-Rules.mdc`)
- Use explicit types (NO `var` keyword)
- Use descriptive variable names
- Add comprehensive logging
- Add error handling with specific error codes
- Add XML documentation comments

### Pull Request Process

1. Create feature branch from `main`
2. Implement changes following System Layer Rules
3. Update documentation (README, code comments)
4. Commit with descriptive messages
5. Push to remote and create Pull Request
6. Wait for CI/CD pipeline to pass
7. Request code review
8. Merge after approval

---

## License

Copyright © 2024 Al Ghurair Investment LLC. All rights reserved.

---

## Contact

**Integration Team:** BoomiIntegrationTeam@al-ghurair.com

**Support:** For issues or questions, contact the Integration Team or create an issue in the repository.
