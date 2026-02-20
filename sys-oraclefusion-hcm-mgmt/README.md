# Oracle Fusion HCM System Layer

System Layer API for Oracle Fusion HCM Leave Management integration.

## Purpose

Unlocks leave/absence data from Oracle Fusion HCM and insulates consumers from the complexity of the underlying Oracle Fusion REST API.

## Architecture

This System Layer follows API-Led Architecture principles:
- **System Layer**: Provides atomic operations for Oracle Fusion HCM
- **Process Layer**: Orchestrates business logic and calls this System Layer
- **Experience Layer**: Consumes Process Layer APIs

## Operations

### Create Leave
- **Function**: `CreateLeave`
- **Route**: `POST /api/leave/create`
- **Purpose**: Creates leave absence entry in Oracle Fusion HCM
- **Input**: D365 leave request format
- **Output**: Oracle Fusion personAbsenceEntryId

## Sequence Diagram

```
┌─────────────┐         ┌──────────────┐         ┌─────────────┐         ┌──────────────┐         ┌─────────────────────┐
│ Process     │         │ CreateLeave  │         │ LeaveMgmt   │         │ CreateLeave │         │ Oracle Fusion HCM   │
│ Layer       │         │ API          │         │ Service     │         │ Handler     │         │ REST API            │
└──────┬──────┘         └──────┬───────┘         └──────┬──────┘         └──────┬──────┘         └──────────┬──────────┘
       │                       │                        │                        │                           │
       │ POST /api/leave/create│                        │                        │                           │
       │ (D365 format)         │                        │                        │                           │
       ├──────────────────────>│                        │                        │                           │
       │                       │                        │                        │                           │
       │                       │ Validate Request       │                        │                           │
       │                       │ (ValidateAPIRequestParameters)                  │                           │
       │                       │                        │                        │                           │
       │                       │ CreateLeave(request)   │                        │                           │
       │                       ├───────────────────────>│                        │                           │
       │                       │                        │                        │                           │
       │                       │                        │ HandleAsync(request)   │                           │
       │                       │                        ├───────────────────────>│                           │
       │                       │                        │                        │                           │
       │                       │                        │                        │ Transform D365 → Oracle   │
       │                       │                        │                        │ (employeeNumber →         │
       │                       │                        │                        │  personNumber, etc.)      │
       │                       │                        │                        │                           │
       │                       │                        │                        │ CreateLeaveAtomicHandler  │
       │                       │                        │                        │ .Handle(atomicRequest)    │
       │                       │                        │                        │                           │
       │                       │                        │                        │ POST /absences            │
       │                       │                        │                        │ (Oracle format)           │
       │                       │                        │                        ├──────────────────────────>│
       │                       │                        │                        │                           │
       │                       │                        │                        │ HTTP 201 Created          │
       │                       │                        │                        │ {personAbsenceEntryId}    │
       │                       │                        │                        │<──────────────────────────┤
       │                       │                        │                        │                           │
       │                       │                        │ HttpResponseSnapshot   │                           │
       │                       │                        │<───────────────────────┤                           │
       │                       │                        │                        │                           │
       │                       │                        │ Check IsSuccessStatusCode                          │
       │                       │                        │ Deserialize ApiResDTO  │                           │
       │                       │                        │ Map to ResDTO          │                           │
       │                       │                        │                        │                           │
       │                       │ BaseResponseDTO        │                        │                           │
       │                       │<───────────────────────┤                        │                           │
       │                       │                        │                        │                           │
       │ BaseResponseDTO       │                        │                        │                           │
       │ (D365 format)         │                        │                        │                           │
       │<──────────────────────┤                        │                        │                           │
       │                       │                        │                        │                           │
```

## Error Handling

The System Layer handles Oracle Fusion HCM errors and normalizes them to BaseResponseDTO:
- HTTP 20x: Success response with personAbsenceEntryId
- HTTP 4xx/5xx: Error response with message
- Connection errors: Caught by ExceptionHandlerMiddleware

## Configuration

### AppConfigs
- `OracleFusionBaseUrl`: Oracle Fusion HCM base URL
- `OracleFusionResourcePath`: REST API resource path (hcmRestApi/resources/11.13.18.05/absences)
- `OracleFusionUsername`: Basic auth username
- `OracleFusionPassword`: Basic auth password (from KeyVault)

### Environment Files
- `appsettings.json`: Placeholder (CI/CD replaces)
- `appsettings.dev.json`: Development environment
- `appsettings.qa.json`: QA environment
- `appsettings.prod.json`: Production environment

## Authentication

Uses HTTP Basic Authentication with credentials from AppConfigs/KeyVault.

## Technology Stack

- .NET 8
- Azure Functions v4 (Isolated Worker Model)
- Polly (Retry/Timeout policies)
- Azure KeyVault (Secret management)
- Redis Cache (Performance optimization)

## Dependencies

- Framework/Core: Core framework components
- Framework/Cache: Caching infrastructure
