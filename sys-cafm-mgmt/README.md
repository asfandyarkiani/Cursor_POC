# sys-cafm-mgmt - CAFM System Layer

System Layer API for FSI CAFM (Facilities Management System) integration.

## Overview

This System Layer provides HTTP-triggered Azure Functions that expose CAFM operations to Process/Experience Layers following API-Led Architecture principles.

## Architecture

### API Flow Sequence Diagram

```
START (Process Layer Request)
 |
 ├─→ CustomAuthenticationMiddleware: Authenticate to CAFM
 |   └─→ AuthenticateAtomicHandler → CAFM SOAP API (Authenticate)
 |   └─→ WRITES: RequestContext.SessionId
 |   └─→ HTTP: [Expected: 200, Error: 401/500]
 |
 ├─→ CreateBreakdownTaskAPI (Azure Function)
 |   └─→ WorkOrderMgmtService
 |       └─→ CreateBreakdownTaskHandler
 |           ├─→ GetLocationsByDtoAtomicHandler → CAFM SOAP API (GetLocationsByDto)
 |           |   └─→ READS: RequestContext.SessionId
 |           |   └─→ HTTP: [Expected: 200, Error: 404/500]
 |           |
 |           ├─→ GetInstructionSetsByDtoAtomicHandler → CAFM SOAP API (GetInstructionSetsByDto)
 |           |   └─→ READS: RequestContext.SessionId
 |           |   └─→ HTTP: [Expected: 200, Error: 404/500]
 |           |
 |           └─→ CreateBreakdownTaskAtomicHandler → CAFM SOAP API (CreateBreakdownTask)
 |               └─→ READS: RequestContext.SessionId
 |               └─→ HTTP: [Expected: 200, Error: 400/500]
 |
 ├─→ CreateEventAPI (Azure Function)
 |   └─→ WorkOrderMgmtService
 |       └─→ CreateEventHandler
 |           └─→ CreateEventAtomicHandler → CAFM SOAP API (CreateEvent)
 |               └─→ READS: RequestContext.SessionId
 |               └─→ HTTP: [Expected: 200, Error: 400/500]
 |
 ├─→ SendEmailAPI (Azure Function)
 |   └─→ NotificationMgmtService
 |       └─→ SendEmailHandler
 |           └─→ SendEmailAtomicHandler → SMTP Server (Office 365)
 |               └─→ HTTP: [Expected: 200, Error: 401/500]
 |
 └─→ CustomAuthenticationMiddleware: Logout from CAFM (finally block)
     └─→ LogoutAtomicHandler → CAFM SOAP API (LogOut)
     └─→ READS: RequestContext.SessionId
     └─→ HTTP: [Expected: 200, Error: 500]
```

## Azure Functions Exposed

### 1. CreateBreakdownTask
- **Route:** `POST /api/cafm/breakdown-task`
- **Purpose:** Creates a breakdown task/work order in CAFM system
- **Authentication:** Session-based (middleware handles login/logout)
- **Operations:** Orchestrates GetLocationsByDto, GetInstructionSetsByDto, CreateBreakdownTask

### 2. CreateEvent
- **Route:** `POST /api/cafm/event`
- **Purpose:** Creates an event or links task in CAFM system
- **Authentication:** Session-based (middleware handles login/logout)

### 3. SendEmail
- **Route:** `POST /api/notification/email`
- **Purpose:** Sends email notifications via SMTP (Office 365)
- **Authentication:** None (uses SMTP credentials)

## Configuration Requirements

### appsettings.{env}.json

All environment files (dev/qa/prod) must have identical structure with environment-specific values:

```json
{
  "AppConfigs": {
    "BaseApiUrl": "https://{env}-cafm.example.com/api",
    "AuthUrl": "https://{env}-cafm.example.com/api/authenticate",
    "LogoutUrl": "https://{env}-cafm.example.com/api/logout",
    "CreateBreakdownTaskUrl": "https://{env}-cafm.example.com/api/CreateBreakdownTask",
    "GetLocationsByDtoUrl": "https://{env}-cafm.example.com/api/GetLocationsByDto",
    "GetInstructionSetsByDtoUrl": "https://{env}-cafm.example.com/api/GetInstructionSetsByDto",
    "GetBreakdownTasksByDtoUrl": "https://{env}-cafm.example.com/api/GetBreakdownTasksByDto",
    "CreateEventUrl": "https://{env}-cafm.example.com/api/CreateEvent",
    "SoapActionAuthenticate": "http://www.fsi.co.uk/services/evolution/04/09/Authenticate",
    "SoapActionLogout": "http://www.fsi.co.uk/services/evolution/04/09/LogOut",
    "SoapActionCreateBreakdownTask": "http://www.fsi.co.uk/services/evolution/04/09/CreateBreakdownTask",
    "SoapActionGetLocations": "http://www.fsi.co.uk/services/evolution/04/09/GetLocationsByDto",
    "SoapActionGetInstructions": "http://www.fsi.co.uk/services/evolution/04/09/GetInstructionSetsByDto",
    "SoapActionGetBreakdownTasks": "http://www.fsi.co.uk/services/evolution/04/09/GetBreakdownTasksByDto",
    "SoapActionCreateEvent": "http://www.fsi.co.uk/services/evolution/04/09/CreateEvent",
    "SmtpHost": "smtp.office365.com",
    "SmtpPort": 587,
    "SmtpUsername": "notifications@example.com",
    "TimeoutSeconds": 50,
    "RetryCount": 0,
    "ProjectNamespace": "sys_cafm_mgmt"
  },
  "KeyVault": {
    "Url": "https://{env}-keyvault.vault.azure.net/",
    "Secrets": {
      "FsiUsername": "FSI-Username",
      "FsiPassword": "FSI-Password",
      "SmtpPassword": "SMTP-Password"
    }
  },
  "RedisCache": {
    "ConnectionString": "{env}-redis-connection-string",
    "InstanceName": "sys-cafm-mgmt:"
  }
}
```

### Azure KeyVault Secrets

The following secrets must be configured in Azure KeyVault:

1. **FSI-Username** - CAFM system username
2. **FSI-Password** - CAFM system password
3. **SMTP-Password** - Office 365 SMTP password

### Environment Variables

Set `ENVIRONMENT` or `ASPNETCORE_ENVIRONMENT` to one of: `dev`, `qa`, `prod`

## Local Development

### Prerequisites

- .NET 8 SDK
- Azure Functions Core Tools v4
- Redis (for caching)
- Azure KeyVault access (or local secrets)

### Setup

1. Clone the repository
2. Update `appsettings.dev.json` with development values
3. Configure Azure KeyVault secrets or use local secrets
4. Restore dependencies:
   ```bash
   dotnet restore
   ```

5. Build the project:
   ```bash
   dotnet build
   ```

6. Run locally:
   ```bash
   func start
   ```

## Dependencies

- **Framework/Core** - Core framework library (project reference)
- **Framework/Cache** - Caching framework library (project reference)
- Microsoft.Azure.Functions.Worker v1.23.0
- Azure.Identity v1.13.1
- Azure.Security.KeyVault.Secrets v4.7.0
- Polly v8.5.0
- Castle.Core v5.1.1

## Middleware

### CustomAuthenticationMiddleware

Handles session-based authentication lifecycle:
- **Before Function:** Authenticates to CAFM, retrieves SessionId, stores in RequestContext
- **After Function (finally):** Logs out from CAFM using SessionId
- **Applied to:** Functions decorated with `[CustomAuthentication]` attribute

### ExecutionTimingMiddleware

Tracks request execution time and downstream API call timings.

### ExceptionHandlerMiddleware

Normalizes all exceptions to BaseResponseDTO format.

## Error Handling

All exceptions are normalized by ExceptionHandlerMiddleware and returned as:

```json
{
  "message": "Error message",
  "errorCode": "CAF_TSKCRT_0001",
  "data": null,
  "errorDetails": {
    "errors": [
      {
        "stepName": "CreateBreakdownTaskHandler.cs / HandleAsync",
        "stepError": "Failed to create breakdown task..."
      }
    ]
  }
}
```

## Testing

### Sample CreateBreakdownTask Request

```json
{
  "reporterName": "John Doe",
  "reporterEmail": "john.doe@example.com",
  "reporterPhoneNumber": "+971501234567",
  "description": "Air conditioning not working in office",
  "serviceRequestNumber": "SR-2024-001",
  "propertyName": "Building A",
  "unitCode": "UNIT-101",
  "categoryName": "HVAC",
  "subCategory": "Air Conditioning",
  "technician": "Tech Team",
  "sourceOrgId": "ORG-001",
  "status": "Open",
  "subStatus": "New",
  "priority": "High",
  "scheduledDate": "2024-01-15",
  "scheduledTimeStart": "09:00",
  "scheduledTimeEnd": "17:00",
  "recurrence": "None",
  "oldCAFMSRNumber": "",
  "raisedDateUtc": "2024-01-10T08:30:00Z"
}
```

### Sample CreateEvent Request

```json
{
  "breakdownTaskId": 12345,
  "eventDescription": "Task assigned to technician",
  "serviceRequestNumber": "SR-2024-001"
}
```

### Sample SendEmail Request

```json
{
  "to": "recipient@example.com",
  "subject": "Work Order Created",
  "body": "<html><body><h1>Work Order SR-2024-001 Created</h1></body></html>",
  "attachmentFileName": "workorder.txt",
  "attachmentContent": "base64-encoded-content"
}
```

## Deployment

The project follows standard Azure Functions deployment:

1. Build the project
2. Publish to Azure Functions
3. Configure Application Settings in Azure Portal
4. Ensure KeyVault access is granted to Function App managed identity

## Notes

- All SOAP operations require session-based authentication (handled by middleware)
- Email operations use SMTP directly (no session required)
- Middleware automatically handles login/logout lifecycle for CAFM operations
- All downstream API timings are tracked in `DSTimeBreakDown` response header
