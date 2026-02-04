# sys-cafm-workorder

System Layer API for CAFM (FSI) Work Order Management operations.

## Overview

This Azure Functions project provides System Layer APIs for interacting with the CAFM (Computer-Aided Facilities Management) system. It implements atomic operations that can be composed by Process Layer APIs for complex workflows like "Create Work Order from EQ+ to CAFM".

## Architecture

```
┌─────────────────────────────────────────────────────────────────────┐
│                        Azure Functions                               │
│  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐ ┌─────────────┐   │
│  │ Authenticate│ │   Logout    │ │GetBreakdown │ │GetLocations │   │
│  │  Function   │ │  Function   │ │   Tasks     │ │  Function   │   │
│  └──────┬──────┘ └──────┬──────┘ └──────┬──────┘ └──────┬──────┘   │
│         │               │               │               │           │
│  ┌──────┴───────────────┴───────────────┴───────────────┴──────┐   │
│  │                        Services                              │   │
│  │  ICAFMAuthenticationService  │  ICAFMWorkOrderService        │   │
│  └──────────────────────────────┴───────────────────────────────┘   │
│         │                                       │                   │
│  ┌──────┴───────────────────────────────────────┴──────────────┐   │
│  │                        Handlers                              │   │
│  │  CAFMAuthenticateHandler, CAFMLogoutHandler,                 │   │
│  │  CAFMGetBreakdownTasksHandler, etc.                          │   │
│  └──────────────────────────────────────────────────────────────┘   │
│         │                                                           │
│  ┌──────┴──────────────────────────────────────────────────────┐   │
│  │                    Atomic Handlers                           │   │
│  │  CAFMAuthenticateAtomicHandler, CAFMLogoutAtomicHandler,     │   │
│  │  CAFMGetBreakdownTasksAtomicHandler, etc.                    │   │
│  └──────────────────────────────────────────────────────────────┘   │
│         │                                                           │
│  ┌──────┴──────────────────────────────────────────────────────┐   │
│  │                    CustomHTTPClient                          │   │
│  │  (SOAP/HTTP calls to CAFM with Polly retry policies)         │   │
│  └──────────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────┘
                              │
                              ▼
                    ┌─────────────────┐
                    │   CAFM (FSI)    │
                    │  SOAP Services  │
                    └─────────────────┘
```

## API Endpoints

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/api/cafm/authenticate` | POST | Authenticate with CAFM, returns session ID |
| `/api/cafm/logout` | POST | Logout from CAFM, terminates session |
| `/api/cafm/breakdowntasks` | POST | Get breakdown tasks by caller source ID |
| `/api/cafm/locations` | POST | Get locations by property name |
| `/api/cafm/instructionsets` | POST | Get instruction sets by category |
| `/api/cafm/breakdowntasks/create` | POST | Create a new breakdown task |
| `/api/cafm/events/create` | POST | Create an event (link to task) |

## Sequence Diagrams

### Authentication Flow

```
┌──────────┐          ┌───────────────┐          ┌────────────────┐          ┌──────┐
│  Client  │          │ Azure Function│          │ AtomicHandler  │          │ CAFM │
└────┬─────┘          └───────┬───────┘          └───────┬────────┘          └──┬───┘
     │                        │                          │                      │
     │ POST /cafm/authenticate│                          │                      │
     │───────────────────────>│                          │                      │
     │                        │                          │                      │
     │                        │ Build SOAP Request       │                      │
     │                        │─────────────────────────>│                      │
     │                        │                          │                      │
     │                        │                          │ POST (SOAP XML)      │
     │                        │                          │─────────────────────>│
     │                        │                          │                      │
     │                        │                          │ <SessionId>          │
     │                        │                          │<─────────────────────│
     │                        │                          │                      │
     │                        │ AuthenticateResponseDto  │                      │
     │                        │<─────────────────────────│                      │
     │                        │                          │                      │
     │  { sessionId: "..." }  │                          │                      │
     │<───────────────────────│                          │                      │
     │                        │                          │                      │
```

### Get Breakdown Tasks Flow

```
┌──────────┐          ┌───────────────┐          ┌────────────────┐          ┌──────┐
│  Client  │          │ Azure Function│          │ AtomicHandler  │          │ CAFM │
└────┬─────┘          └───────┬───────┘          └───────┬────────┘          └──┬───┘
     │                        │                          │                      │
     │ POST /cafm/breakdowntasks                         │                      │
     │ { sessionId, callerSourceId }                     │                      │
     │───────────────────────>│                          │                      │
     │                        │                          │                      │
     │                        │ Build SOAP Request       │                      │
     │                        │─────────────────────────>│                      │
     │                        │                          │                      │
     │                        │                          │ POST (SOAP XML)      │
     │                        │                          │ GetBreakdownTasksByDto
     │                        │                          │─────────────────────>│
     │                        │                          │                      │
     │                        │                          │ <BreakdownTaskDto[]> │
     │                        │                          │<─────────────────────│
     │                        │                          │                      │
     │                        │ GetBreakdownTasksResponse│                      │
     │                        │<─────────────────────────│                      │
     │                        │                          │                      │
     │  { tasks: [...] }      │                          │                      │
     │<───────────────────────│                          │                      │
     │                        │                          │                      │
```

### Create Breakdown Task Flow

```
┌──────────┐          ┌───────────────┐          ┌────────────────┐          ┌──────┐
│  Client  │          │ Azure Function│          │ AtomicHandler  │          │ CAFM │
└────┬─────┘          └───────┬───────┘          └───────┬────────┘          └──┬───┘
     │                        │                          │                      │
     │ POST /cafm/breakdowntasks/create                  │                      │
     │ { sessionId, buildingId, locationId, ...}         │                      │
     │───────────────────────>│                          │                      │
     │                        │                          │                      │
     │                        │ Validate & Map Request   │                      │
     │                        │─────────────────────────>│                      │
     │                        │                          │                      │
     │                        │                          │ POST (SOAP XML)      │
     │                        │                          │ CreateBreakdownTask  │
     │                        │                          │─────────────────────>│
     │                        │                          │                      │
     │                        │                          │ <TaskId>             │
     │                        │                          │<─────────────────────│
     │                        │                          │                      │
     │                        │ CreateBreakdownTaskResponse                     │
     │                        │<─────────────────────────│                      │
     │                        │                          │                      │
     │  { taskId: "..." }     │                          │                      │
     │<───────────────────────│                          │                      │
     │                        │                          │                      │
```

## Configuration

### Required Settings

Update `appsettings.json` or environment-specific files:

```json
{
  "AppConfigs": {
    "CAFM": {
      "BaseUrl": "https://devcafm.agfacilities.com",
      "WebServicesPath": "/WebServices/evolution",
      "Username": "TODO: Retrieve from Key Vault",
      "Password": "TODO: Retrieve from Key Vault",
      "ContractId": "TODO: Set Contract ID per environment",
      "SoapActions": {
        "Authenticate": "http://www.fsi.co.uk/services/evolution/04/09/Authenticate",
        "Logout": "http://www.fsi.co.uk/services/evolution/04/09/Logout",
        "GetBreakdownTasksByDto": "http://www.fsi.co.uk/services/evolution/04/09/GetBreakdownTasksByDto",
        "GetLocationsByDto": "http://www.fsi.co.uk/services/evolution/04/09/GetLocationsByDto",
        "GetInstructionSetsByDto": "http://www.fsi.co.uk/services/evolution/04/09/GetInstructionSetsByDto",
        "CreateBreakdownTask": "http://www.fsi.co.uk/services/evolution/04/09/CreateBreakdownTask",
        "CreateEvent": "http://www.fsi.co.uk/services/evolution/04/09/CreateEvent"
      }
    }
  }
}
```

### Environment Variables

For production, sensitive values should be stored in Azure Key Vault:

- `CAFM__Username` - FSI Login username
- `CAFM__Password` - FSI Login password
- `CAFM__ContractId` - Contract ID for work order creation

## Running Locally

1. Ensure you have the .NET 8 SDK installed
2. Install Azure Functions Core Tools v4
3. Update `local.settings.json` with required values
4. Run:

```bash
cd sys-cafm-workorder
dotnet restore
func start
```

## Project Structure

```
sys-cafm-workorder/
├── Abstractions/                    # Service interfaces
│   ├── ICAFMAuthenticationService.cs
│   └── ICAFMWorkOrderService.cs
├── ConfigModels/                    # Configuration classes
│   └── AppConfigs.cs
├── DTOs/
│   ├── Api/CAFM/                    # API request/response DTOs
│   │   ├── AuthenticateRequestDto.cs
│   │   ├── AuthenticateResponseDto.cs
│   │   └── ...
│   └── Downstream/CAFM/             # SOAP request/response DTOs
│       ├── CAFMAuthenticateDownstreamRequestDto.cs
│       └── ...
├── Functions/CAFM/                  # Azure Function entry points
│   ├── CAFMAuthenticateFunction.cs
│   └── ...
├── Implementations/FSI/
│   ├── AtomicHandlers/              # Direct SOAP call handlers
│   ├── Handlers/                    # Business logic handlers
│   └── Services/                    # Service implementations
├── appsettings.json
├── appsettings.Development.json
├── host.json
├── local.settings.json
├── Program.cs
└── sys-cafm-workorder.csproj
```

## Dependencies

- **Framework/Core** - Shared core functionality including CustomHTTPClient, exceptions, extensions
- **Framework/Cache** - Caching infrastructure (available but not used in this implementation)

## Error Handling

All errors are handled by the `ExceptionHandlerMiddleware` from the Core framework:

- `DownStreamApiFailureException` - CAFM service errors
- `RequestValidationFailureException` - Invalid request parameters
- `NoRequestBodyException` - Missing request body

## Notes

- This is a **System Layer** implementation only
- No orchestration logic is included (that belongs in Process Layer)
- Each API is atomic and stateless (except for session management)
- Session IDs must be passed by the caller (typically from Process Layer)
