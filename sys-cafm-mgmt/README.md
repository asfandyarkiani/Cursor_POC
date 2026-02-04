# sys-cafm-mgmt - CAFM System Layer API

## Overview

This is the **System Layer API** for CAFM (Computer-Aided Facility Management) system of record. It provides a standardized interface to interact with the FSI CAFM system, following the API-Led Architecture principles.

**System of Record (SOR):** FSI CAFM (Facilities Management System)  
**Vendor:** FSI  
**Architecture Layer:** System Layer  
**Target Framework:** .NET 8  
**Hosting:** Azure Functions v4

## Purpose

The CAFM System Layer API:
- Unlocks data from the CAFM system of record
- Insulates consumers from the complexity of SOAP-based FSI CAFM APIs
- Provides RESTful HTTP endpoints for work order management
- Handles authentication and session management automatically
- Ensures reusability across multiple Process and Experience Layer APIs

## Architecture

This System Layer follows a layered architecture pattern:

```
Azure Functions (HTTP Entry Points)
        ↓
    Services (Abstraction Boundary)
        ↓
    Handlers (Orchestration Layer)
        ↓
Atomic Handlers (Single SOAP Operations)
        ↓
    CAFM SOAP APIs
```

### Components

1. **Functions/** - HTTP-triggered Azure Functions (entry points)
2. **Abstractions/** - Interface definitions (ICAFMMgmt)
3. **Implementations/FSI/Services/** - Service implementations
4. **Implementations/FSI/Handlers/** - Orchestration handlers
5. **Implementations/FSI/AtomicHandlers/** - Single SOAP operation handlers
6. **DTOs/API/** - Public API DTOs
7. **DTOs/Downstream/** - CAFM-specific DTOs
8. **Middlewares/** - Authentication and cross-cutting concerns
9. **ConfigModels/** - Configuration models

## API Operations

### 1. Create Work Order

**Endpoint:** `POST /api/cafm/workorders`

Creates work orders in CAFM from external systems (e.g., EQ+).

**Request Body:**
```json
{
  "workOrder": {
    "serviceRequests": [
      {
        "serviceRequestNumber": "EQ-12345",
        "sourceOrgId": "EQ_PLUS",
        "unitCode": "BLD-01-FL-02-ROOM-301",
        "description": "Air conditioning not working",
        "priority": "High",
        "category": "HVAC",
        "subCategory": "AC Repair",
        "requestedBy": "John Doe",
        "contactPhone": "+971-50-123-4567",
        "notes": "Urgent - Meeting room"
      }
    ]
  }
}
```

**Response:**
```json
{
  "success": true,
  "message": "Successfully created 1 work order(s)",
  "workOrder": {
    "items": [
      {
        "cafmSRNumber": "CAFM-WO-98765",
        "sourceSRNumber": "EQ-12345",
        "sourceOrgId": "EQ_PLUS"
      }
    ]
  }
}
```

## Sequence Diagram

```
┌─────────┐     ┌──────────────┐     ┌──────────────┐     ┌─────────────┐     ┌──────────┐
│ Client  │     │   Azure      │     │    CAFM      │     │   Handler   │     │   CAFM   │
│ (EQ+)   │     │   Function   │     │ Auth Middleware│   │             │     │   SOAP   │
└────┬────┘     └──────┬───────┘     └──────┬───────┘     └──────┬──────┘     └────┬─────┘
     │                 │                     │                    │                  │
     │ POST /workorders│                     │                    │                  │
     ├────────────────>│                     │                    │                  │
     │                 │                     │                    │                  │
     │                 │  Check Auth Attr   │                    │                  │
     │                 ├────────────────────>│                    │                  │
     │                 │                     │                    │                  │
     │                 │                     │  Login (Authenticate)                 │
     │                 │                     ├───────────────────────────────────────>│
     │                 │                     │                    │                  │
     │                 │                     │<───────────────────────────────────────┤
     │                 │                     │  SessionId         │                  │
     │                 │                     │                    │                  │
     │                 │  Store SessionId   │                    │                  │
     │                 │<────────────────────┤                    │                  │
     │                 │                     │                    │                  │
     │                 │  Invoke Function   │                    │                  │
     │                 ├────────────────────────────────────────>│                  │
     │                 │                     │                    │                  │
     │                 │                     │                    │ GetLocationsByDto│
     │                 │                     │                    ├─────────────────>│
     │                 │                     │                    │<─────────────────┤
     │                 │                     │                    │  Location Data   │
     │                 │                     │                    │                  │
     │                 │                     │                    │ GetInstructionSets│
     │                 │                     │                    ├─────────────────>│
     │                 │                     │                    │<─────────────────┤
     │                 │                     │                    │ Instruction Sets │
     │                 │                     │                    │                  │
     │                 │                     │                    │CreateBreakdownTask│
     │                 │                     │                    ├─────────────────>│
     │                 │                     │                    │<─────────────────┤
     │                 │                     │                    │  TaskId          │
     │                 │                     │                    │                  │
     │                 │  Return Response   │                    │                  │
     │                 │<────────────────────────────────────────┤                  │
     │                 │                     │                    │                  │
     │                 │                     │  Logout (Auto)     │                  │
     │                 │                     ├───────────────────────────────────────>│
     │                 │                     │                    │                  │
     │  Response       │                     │                    │                  │
     │<────────────────┤                     │                    │                  │
     │                 │                     │                    │                  │
```

## Configuration

### Required Settings

Configuration is managed through `appsettings.{env}.json` files for each environment (dev, qa, stg, prod, dr).

**AppConfigs.CAFMSettings:**
- `BaseUrl` - CAFM base URL (e.g., https://devcafm.agfacilities.com)
- `Username` - CAFM authentication username (TODO: Store in Azure Key Vault)
- `Password` - CAFM authentication password (TODO: Store in Azure Key Vault)
- `LoginResourcePath` - Resource path for login endpoint
- `LoginSoapAction` - SOAP action for login
- `LogoutResourcePath` - Resource path for logout endpoint
- `LogoutSoapAction` - SOAP action for logout
- `GetLocationsResourcePath` - Resource path for GetLocationsByDto
- `GetLocationsSoapAction` - SOAP action for GetLocationsByDto
- `GetInstructionSetsResourcePath` - Resource path for GetInstructionSetsByDto
- `GetInstructionSetsSoapAction` - SOAP action for GetInstructionSetsByDto
- `GetBreakdownTasksResourcePath` - Resource path for GetBreakdownTasksByDto
- `GetBreakdownTasksSoapAction` - SOAP action for GetBreakdownTasksByDto
- `CreateBreakdownTaskResourcePath` - Resource path for CreateBreakdownTask
- `CreateBreakdownTaskSoapAction` - SOAP action for CreateBreakdownTask
- `CreateEventResourcePath` - Resource path for CreateEvent
- `CreateEventSoapAction` - SOAP action for CreateEvent
- `ConnectionTimeoutSeconds` - HTTP connection timeout (default: 30)
- `ReadTimeoutSeconds` - HTTP read timeout (default: 60)
- `SessionCacheExpirationMinutes` - Session cache expiration (default: 30)

### Environment Detection

The application detects the environment using the `Environment` environment variable:
- `local` - Local development
- `dev` - Development environment
- `qa` - QA environment
- `stg` - Staging environment
- `prod` - Production environment
- `dr` - Disaster Recovery environment

## Authentication & Session Management

Authentication is handled automatically by the `CAFMAuthenticationMiddleware`. Functions decorated with `[CAFMAuthentication]` attribute will:

1. Automatically login to CAFM before execution
2. Store the session ID in the function context
3. Make the session ID available to handlers
4. Automatically logout after execution (if `AutoLogout = true`)

**Example:**
```csharp
[Function("CreateWorkOrder")]
[CAFMAuthentication(AutoLogout = true)]
public async Task<HttpResponseData> CreateWorkOrder(...)
{
    // Session management is automatic
    // No manual login/logout required
}
```

## Local Development

### Prerequisites
- .NET 8 SDK
- Azure Functions Core Tools v4
- Visual Studio 2022 or VS Code with Azure Functions extension

### Running Locally

1. Update `local.settings.json` with appropriate values:
```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "Environment": "local"
  }
}
```

2. Update `appsettings.json` or `appsettings.local.json` with CAFM credentials (for local testing only)

3. Run the function:
```bash
func start
```

4. Test the endpoint:
```bash
curl -X POST http://localhost:7071/api/cafm/workorders \
  -H "Content-Type: application/json" \
  -d @sample-request.json
```

## Dependencies

### Framework References
- **AGI.ApiEcoSys.Core** - Core framework with base classes, extensions, and middleware
- **AGI.ApiEcoSys.Cache** - Caching framework (for future session caching)

### NuGet Packages
- Microsoft.Azure.Functions.Worker (v2.0.0)
- Microsoft.Azure.Functions.Worker.Extensions.Http (v4.0.0)
- Polly (v8.6.1) - Resilience and retry policies
- System.ServiceModel.Http (v8.0.0) - SOAP client support
- System.ServiceModel.Primitives (v8.0.0) - SOAP primitives

## Error Handling

All exceptions are handled by the `ExceptionHandlerMiddleware` from the Core framework. Errors are normalized and returned in a consistent format:

```json
{
  "success": false,
  "message": "Error message",
  "errorCode": "ERROR_CODE",
  "timestamp": "2026-01-22T10:30:00Z"
}
```

## Logging

Logging is handled through `ILogger<T>` and follows these conventions:
- **Information** - Normal operation flow
- **Warning** - Recoverable issues (e.g., logout failure)
- **Error** - Errors that affect functionality

All logs include contextual information such as:
- Function name
- Session ID
- Service request numbers
- Operation details

## Security Considerations

1. **Credentials Storage**: CAFM credentials should be stored in Azure Key Vault, not in configuration files
2. **Session Management**: Sessions are automatically managed and cleaned up
3. **HTTPS Only**: All communication with CAFM uses HTTPS
4. **Function-Level Authorization**: Functions use `AuthorizationLevel.Function` requiring function keys

## Future Enhancements

- [ ] Implement session caching using Redis to reduce login calls
- [ ] Add support for bulk work order creation
- [ ] Implement work order status tracking
- [ ] Add support for work order updates and cancellations
- [ ] Implement comprehensive error recovery mechanisms
- [ ] Add support for file attachments
- [ ] Implement audit logging

## Support

For issues or questions, contact the API Ecosystem team or create an issue in the repository.

## License

Copyright © 2026 Al Ghurair Investment LLC. All rights reserved.
