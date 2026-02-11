# sys-d365-driverlateloginmgmt

**System Layer API for D365 Driver Late Login Management**

## Overview

This System Layer project provides APIs for managing driver late login requests in Microsoft Dynamics 365 Finance & Operations (D365). It follows the API-Led Architecture principles, specifically implementing the System Layer that unlocks data from D365 and insulates consumers from the complexity of the underlying system.

**Domain:** Automotive / KABI Taxi  
**SOR:** Microsoft Dynamics 365 Finance & Operations (D365)  
**Business Function:** Driver Late Login Request Management

## Architecture

This project follows the **API-Led Architecture** with three layers:
- **System Layer (this project):** Unlocks data from D365 and provides atomic operations
- **Process Layer:** Orchestrates multiple System APIs and implements business logic
- **Experience Layer:** Reconfigures data for specific end-user needs

### System Layer Responsibilities

1. Direct interaction with D365 F&O
2. OAuth2 authentication with Azure AD
3. Data transformation between Process Layer format and D365 format
4. Error handling and retry logic
5. Insulating consumers from D365 complexity

## Project Structure

```
sys-d365-driverlateloginmgmt/
├── Abstractions/                              # Service interfaces
│   └── IDriverLateLoginMgmt.cs
├── Implementations/D365/                      # D365-specific implementations
│   ├── Services/
│   │   └── DriverLateLoginMgmtService.cs
│   ├── Handlers/
│   │   └── SubmitDriverLateLoginHandler.cs
│   └── AtomicHandlers/
│       ├── AuthenticateD365AtomicHandler.cs
│       ├── SubmitLateLoginAtomicHandler.cs
│       └── LogoutD365AtomicHandler.cs
├── DTO/                                       # Data Transfer Objects
│   ├── SubmitDriverLateLoginDTO/
│   │   ├── SubmitDriverLateLoginReqDTO.cs
│   │   └── SubmitDriverLateLoginResDTO.cs
│   ├── AtomicHandlerDTOs/
│   │   ├── SubmitLateLoginHandlerReqDTO.cs
│   │   └── AuthenticationRequestDTO.cs
│   └── DownstreamDTOs/
│       ├── SubmitLateLoginApiResDTO.cs
│       └── AuthenticationResponseDTO.cs
├── Functions/                                 # Azure Functions (HTTP entry points)
│   └── SubmitDriverLateLoginRequestAPI.cs
├── ConfigModels/                              # Configuration models
│   └── AppConfigs.cs
├── Constants/                                 # Error and info constants
│   ├── ErrorConstants.cs
│   └── InfoConstants.cs
├── Helpers/                                   # Helper utilities
│   ├── KeyVaultReader.cs
│   └── RequestContext.cs
├── Attributes/                                # Custom attributes
│   └── D365AuthenticationAttribute.cs
├── Middleware/                                # Custom middleware
│   └── D365AuthenticationMiddleware.cs
├── Program.cs                                 # Application entry point
├── host.json                                  # Azure Functions configuration
├── appsettings.json                           # Configuration (placeholder)
├── appsettings.dev.json                       # Development configuration
├── appsettings.qa.json                        # QA configuration
└── appsettings.prod.json                      # Production configuration
```

## API Flow

```
┌─────────────────────────────────────────────────────────────────────────┐
│ Process Layer / Experience Layer                                        │
└────────────────────────┬────────────────────────────────────────────────┘
                         │
                         │ HTTP POST /driver/latelogin
                         │ Body: { DriverId, RequestDateTime, CompanyCode, ... }
                         ▼
┌─────────────────────────────────────────────────────────────────────────┐
│ Azure Function: SubmitDriverLateLoginRequestAPI                        │
│ [D365Authentication]                                                    │
└────────────────────────┬────────────────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────────────────────┐
│ Middleware: D365AuthenticationMiddleware                               │
│ - Checks if token exists and is valid                                  │
│ - If expired/missing: calls AuthenticateD365AtomicHandler              │
│ - Caches token in RequestContext                                       │
└────────────────────────┬────────────────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────────────────────┐
│ Service: DriverLateLoginMgmtService (IDriverLateLoginMgmt)             │
└────────────────────────┬────────────────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────────────────────┐
│ Handler: SubmitDriverLateLoginHandler                                  │
│ - Orchestrates atomic operations                                       │
│ - Validates D365 response                                              │
│ - Maps D365 response to System Layer format                            │
└────────────────────────┬────────────────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────────────────────┐
│ Atomic Handler: SubmitLateLoginAtomicHandler                           │
│ - Builds D365 request body                                             │
│ - Adds Authorization header (Bearer token)                             │
│ - HTTP POST to D365 late login API                                     │
└────────────────────────┬────────────────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────────────────────┐
│ Microsoft Dynamics 365 F&O                                              │
│ POST /api/services/.../lateloginrequest                                │
│ Response: { Messages: [{ success, message, ... }] }                    │
└─────────────────────────────────────────────────────────────────────────┘
```

## Authentication Flow

The project uses OAuth2 Bearer token authentication with Azure AD:

```
┌─────────────────────────────────────────────────────────────────────────┐
│ D365AuthenticationMiddleware                                           │
└────────────────────────┬────────────────────────────────────────────────┘
                         │
                         │ Check: Token exists and not expired?
                         │
                ┌────────┴────────┐
                │                 │
               NO                YES
                │                 │
                ▼                 ▼
┌───────────────────────┐  ┌──────────────────┐
│ AuthenticateD365      │  │ Use cached token │
│ AtomicHandler         │  └──────────────────┘
└───────────┬───────────┘
            │
            │ POST /oauth2/token
            │ Body: grant_type, client_id, client_secret, resource
            ▼
┌─────────────────────────────────────────────────────────────────────────┐
│ Azure AD Token Endpoint                                                 │
│ Response: { access_token, expires_on, ... }                            │
└────────────────────────┬────────────────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────────────────────┐
│ RequestContext (AsyncLocal)                                             │
│ - AuthorizationToken: "Bearer <access_token>"                          │
│ - TokenExpiresOn: <unix_timestamp>                                     │
│ - IsTokenValid: true                                                   │
└─────────────────────────────────────────────────────────────────────────┘
```

## API Endpoints

### Submit Driver Late Login Request

**Endpoint:** `POST /driver/latelogin`  
**Authentication:** OAuth2 Bearer token (handled by middleware)  
**Content-Type:** application/json

**Request Body:**
```json
{
  "DriverId": "12345",
  "RequestDateTime": "2024-01-15T08:30:00Z",
  "CompanyCode": "AGI",
  "ReasonCode": "TRAFFIC",
  "Remarks": "Heavy traffic on Sheikh Zayed Road",
  "RequestNo": "REQ-001"
}
```

**Response (Success):**
```json
{
  "Message": "Late login request submitted successfully",
  "Data": {
    "Success": true,
    "Message": "Late login request created successfully",
    "Reference": "REF-12345",
    "InputReference": "INPUT-12345"
  },
  "ErrorCode": null,
  "ErrorDetails": null
}
```

**Response (Error):**
```json
{
  "Message": "Failed to submit late login request",
  "Data": null,
  "ErrorCode": "D365_LATLOG_0001",
  "ErrorDetails": [
    "Status: 400",
    "Response: { error details }"
  ]
}
```

## Configuration

### Environment Variables

The following configuration is required in appsettings.{env}.json:

```json
{
  "D365Config": {
    "BaseUrl": "https://agifd-{env}.cloudax.uae.dynamics.com",
    "LateLoginResourcePath": "api/services/AGIMobileAppGroup/AGIMobileAppCarsTaxiIntegrationServices/lateloginrequest",
    "TokenUrl": "https://login.microsoftonline.com/{tenant-id}/oauth2/token",
    "ClientId": "{client-id}",
    "ClientSecret": "{client-secret}",
    "Resource": "https://agifd-{env}.cloudax.uae.dynamics.com",
    "GrantType": "client_credentials",
    "TokenCacheDurationMinutes": 50
  },
  "KeyVaultConfig": {
    "KeyVaultUrl": "https://agi-keyvault-{env}.vault.azure.net/",
    "UseKeyVault": true
  },
  "ApplicationInsights": {
    "ConnectionString": "InstrumentationKey={instrumentation-key}"
  }
}
```

### Azure Key Vault Secrets

If `KeyVaultConfig.UseKeyVault` is true, the following secrets should be stored in Azure Key Vault:
- `D365-ClientSecret` - OAuth2 client secret for D365 authentication

## Middleware

### ExecutionTimingMiddleware (Framework)
- Tracks execution time for each request
- Initializes timing breakdown header

### ExceptionHandlerMiddleware (Framework)
- Catches all exceptions
- Normalizes to BaseResponseDTO
- Returns appropriate HTTP status codes

### D365AuthenticationMiddleware (Custom)
- Handles OAuth2 token authentication
- Retrieves token from Azure AD
- Caches token with expiration tracking
- Automatically refreshes expired tokens

## Error Handling

All errors are handled by the ExceptionHandlerMiddleware and returned as BaseResponseDTO:

| Exception Type | HTTP Status | Description |
|---------------|-------------|-------------|
| RequestValidationFailureException | 400 | Invalid request parameters |
| NoRequestBodyException | 400 | Missing request body |
| NotFoundException | 404 | Resource not found |
| BusinessCaseFailureException | 409/422 | Business rule violation |
| DownStreamApiFailureException | Varies | D365 API failure |
| NoResponseBodyException | 500 | Empty D365 response |

## Logging

The project uses structured logging with Application Insights:

```csharp
_logger.Info("Operation started");
_logger.Error(exception, "Operation failed");
_logger.Warn("Warning message");
```

All logs are sent to:
- Console (local development)
- Application Insights (all environments)
- File system (fileLoggingMode: always)

## Dependencies

### Framework Projects
- **AGI.ApiEcoSys.Core** - Core framework with base classes, interfaces, middleware
- **AGI.ApiEcoSys.Cache** - Caching framework (not used in this project)

### NuGet Packages
- Microsoft.Azure.Functions.Worker (2.0.0)
- Microsoft.Azure.Functions.Worker.Sdk (2.0.0)
- Microsoft.Azure.Functions.Worker.Extensions.Http (4.0.0)
- Microsoft.ApplicationInsights.WorkerService (2.22.0)
- Azure.Identity (1.14.1)
- Azure.Security.KeyVault.Secrets (4.8.0)
- Polly (8.6.1)

## Development

### Prerequisites
- .NET 8 SDK
- Azure Functions Core Tools v4
- Visual Studio 2022 or VS Code with Azure Functions extension

### Local Development

1. Clone the repository
2. Update `appsettings.dev.json` with your D365 credentials
3. Run the project:
   ```bash
   cd sys-d365-driverlateloginmgmt
   func start
   ```

### Testing

Test the API using curl or Postman:

```bash
curl -X POST http://localhost:7071/api/driver/latelogin \
  -H "Content-Type: application/json" \
  -d '{
    "DriverId": "12345",
    "RequestDateTime": "2024-01-15T08:30:00Z",
    "CompanyCode": "AGI",
    "ReasonCode": "TRAFFIC",
    "Remarks": "Heavy traffic"
  }'
```

## Deployment

### CI/CD Pipeline

The project uses GitHub Actions for CI/CD:
- **Build:** Compiles the project and runs tests
- **Deploy:** Deploys to Azure Functions

### Environment Configuration

The CI/CD pipeline replaces `appsettings.json` with the appropriate environment-specific file:
- **Development:** appsettings.dev.json
- **QA:** appsettings.qa.json
- **Production:** appsettings.prod.json

## Security

### Authentication
- OAuth2 Bearer token authentication with Azure AD
- Token caching with automatic refresh
- Tokens expire after 60 minutes (cached for 50 minutes with 5-minute buffer)

### Secrets Management
- Client secrets stored in Azure Key Vault
- Managed Identity for Key Vault access
- No secrets in code or configuration files

## Monitoring

### Application Insights
- Request/response logging
- Exception tracking
- Performance metrics
- Custom events

### Health Checks
- Function availability
- D365 connectivity
- Azure AD authentication

## Error Codes

| Error Code | Description |
|-----------|-------------|
| D365_AUTHEN_0001 | Failed to authenticate with D365 token endpoint |
| D365_AUTHEN_0002 | Invalid or expired D365 authentication token |
| D365_AUTHEN_0003 | Missing required authentication configuration |
| D365_LATLOG_0001 | Failed to submit late login request to D365 |
| D365_LATLOG_0002 | D365 late login API returned error response |
| D365_LATLOG_0003 | Invalid late login request data |
| D365_LATLOG_0004 | Missing required late login parameters |
| D365_CONFIG_0001 | Missing or invalid D365 configuration |
| D365_CONFIG_0002 | Missing D365 BaseUrl configuration |
| D365_CONFIG_0003 | Missing D365 LateLoginResourcePath configuration |
| D365_NETWRK_0001 | Network timeout while calling D365 API |
| D365_NETWRK_0002 | D365 API returned HTTP error status code |
| D365_NETWRK_0003 | Failed to deserialize D365 API response |
| D365_VALIDN_0001 | Invalid driver ID format |
| D365_VALIDN_0002 | Invalid late login date/time format |
| D365_VALIDN_0003 | Invalid company code |

## Sequence Diagram

```
Mobile App → Process Layer → System Layer (this) → D365 F&O

1. Process Layer sends HTTP POST to /driver/latelogin
2. D365AuthenticationMiddleware checks token:
   - If expired/missing: calls AuthenticateD365AtomicHandler
   - If valid: uses cached token
3. SubmitDriverLateLoginRequestAPI validates request
4. DriverLateLoginMgmtService delegates to Handler
5. SubmitDriverLateLoginHandler orchestrates:
   - Calls SubmitLateLoginAtomicHandler
   - Validates D365 response
   - Maps response to System Layer format
6. SubmitLateLoginAtomicHandler:
   - Builds D365 request body
   - Adds Authorization header
   - HTTP POST to D365 API
7. D365 F&O processes late login request
8. Response flows back through layers
9. Process Layer receives BaseResponseDTO
```

## Boomi Process Migration

This System Layer project was migrated from the following Boomi process:
- **Process Name:** Late_Login
- **Process ID:** 62132bc2-8e9b-4132-bf0c-c7108d8310bf
- **Folder:** Al Ghurair Investment LLC/Automotive/Buddy APP 2.0/LateLogin

### Migration Documentation
- **Phase 1 Extraction:** BOOMI_EXTRACTION_PHASE1.md
- **Phase 3 Compliance:** RULEBOOK_COMPLIANCE_REPORT.md

## Support

For issues or questions, contact:
- **Integration Team:** BoomiIntegrationTeam@al-ghurair.com
- **Repository:** https://github.com/asfandyarkiani/System_Layer_Agent

## License

Copyright © 2026 Al Ghurair Investment LLC. All rights reserved.
