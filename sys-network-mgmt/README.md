# sys-network-mgmt

System Layer API for Network Management operations. This service provides network connectivity and health check functionality for the API-Led Architecture.

## Overview

This System Layer implementation is derived from the Boomi "Network Test" process. It exposes a simple health check endpoint that returns a success message indicating the system is operational.

### Source Boomi Process

- **Process Name:** Network Test
- **Folder:** Al Ghurair Investment LLC/Network
- **Operation:** Execute (Listen)
- **Input:** None
- **Output:** Single data (text/plain)
- **Message:** "Test is successful!!!"

## Architecture

### API-Led Architecture Layer

This is a **System Layer** API that:
- Unlocks/exposes network connectivity testing capabilities
- Insulates consumers from underlying system complexity
- Provides a reusable "Lego block" for health checking

### Component Flow

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                           SYSTEM LAYER                                       │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  ┌──────────────────┐    ┌───────────────────┐    ┌─────────────────────┐   │
│  │  NetworkTestAPI  │───▶│ NetworkMgmtService│───▶│ NetworkTestHandler  │   │
│  │  (Azure Function)│    │    (Service)      │    │     (Handler)       │   │
│  └──────────────────┘    └───────────────────┘    └─────────────────────┘   │
│         │                         │                        │                 │
│         │                         │                        │                 │
│         ▼                         ▼                        ▼                 │
│   HTTP Trigger              Delegates to            Returns Success         │
│   (GET/POST)                  Handler                  Response             │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

### Sequence Diagram

```
┌────────┐     ┌───────────────┐     ┌──────────────────┐     ┌─────────────────┐
│ Client │     │ NetworkTestAPI│     │NetworkMgmtService│     │NetworkTestHandler│
└───┬────┘     └───────┬───────┘     └────────┬─────────┘     └────────┬────────┘
    │                  │                      │                        │
    │ HTTP GET/POST    │                      │                        │
    │ /api/network/test│                      │                        │
    │─────────────────▶│                      │                        │
    │                  │                      │                        │
    │                  │ NetworkTest(reqDTO)  │                        │
    │                  │─────────────────────▶│                        │
    │                  │                      │                        │
    │                  │                      │ HandleAsync(reqDTO)    │
    │                  │                      │───────────────────────▶│
    │                  │                      │                        │
    │                  │                      │                        │ Generate
    │                  │                      │                        │ Response
    │                  │                      │                        │
    │                  │                      │    BaseResponseDTO     │
    │                  │                      │◀───────────────────────│
    │                  │                      │                        │
    │                  │    BaseResponseDTO   │                        │
    │                  │◀─────────────────────│                        │
    │                  │                      │                        │
    │ BaseResponseDTO  │                      │                        │
    │ "Test is         │                      │                        │
    │  successful!!!"  │                      │                        │
    │◀─────────────────│                      │                        │
    │                  │                      │                        │
```

## Folder Structure

```
sys-network-mgmt/
├── Abstractions/
│   └── INetworkMgmt.cs                    # Service interface
├── ConfigModels/
│   └── AppConfigs.cs                      # Application configuration
├── Constants/
│   ├── ErrorConstants.cs                  # Error codes and messages
│   └── InfoConstants.cs                   # Success/info messages
├── DTO/
│   └── HandlerDTOs/
│       └── NetworkTestDTO/
│           ├── NetworkTestReqDTO.cs       # Request DTO
│           └── NetworkTestResDTO.cs       # Response DTO
├── Functions/
│   └── NetworkTestAPI.cs                  # Azure Function entry point
├── Implementations/
│   └── Network/
│       ├── Handlers/
│       │   └── NetworkTestHandler.cs      # Business handler
│       └── Services/
│           └── NetworkMgmtService.cs      # Service implementation
├── appsettings.json                       # Base configuration
├── appsettings.dev.json                   # Development configuration
├── appsettings.qa.json                    # QA configuration
├── appsettings.prod.json                  # Production configuration
├── host.json                              # Azure Functions host configuration
├── local.settings.json                    # Local development settings
├── Program.cs                             # Application entry point & DI
├── sys-network-mgmt.csproj               # Project file
├── sys-network-mgmt.sln                  # Solution file
└── README.md                              # This file
```

## API Endpoints

### Network Test

Tests network connectivity and system health.

**Endpoint:** `GET/POST /api/network/test`

**Request:**
- Method: GET or POST
- Body (optional for POST):
```json
{
  "correlationId": "optional-correlation-id"
}
```

**Response:**
```json
{
  "message": "Test is successful!!!",
  "errorCode": null,
  "data": {
    "message": "Test is successful!!!",
    "isSuccessful": true,
    "timestamp": "2025-01-21T12:00:00Z",
    "correlationId": "guid-here",
    "version": "1.0.0",
    "environment": "dev"
  },
  "errorDetails": null,
  "isDownStreamError": false,
  "isPartialSuccess": false
}
```

## Configuration

### AppConfigs Section

| Property | Description | Required |
|----------|-------------|----------|
| Environment | Environment name (dev/qa/stg/prod/dr) | Yes |
| ApplicationName | Application identifier | Yes |
| Version | Application version | No |

### Environment Variables

| Variable | Description |
|----------|-------------|
| AzureWebJobsStorage | Azure Storage connection string |
| FUNCTIONS_WORKER_RUNTIME | Must be `dotnet-isolated` |
| ASPNETCORE_ENVIRONMENT | Environment name |

## Running Locally

### Prerequisites

- .NET 8.0 SDK
- Azure Functions Core Tools v4
- Azure Storage Emulator (or Azurite)

### Steps

1. Clone the repository:
```bash
git clone <repository-url>
cd sys-network-mgmt
```

2. Restore dependencies:
```bash
dotnet restore
```

3. Build the project:
```bash
dotnet build
```

4. Start Azure Storage Emulator or Azurite

5. Run the function:
```bash
func start
```

6. Test the endpoint:
```bash
# GET request
curl http://localhost:7071/api/network/test

# POST request
curl -X POST http://localhost:7071/api/network/test \
  -H "Content-Type: application/json" \
  -d '{"correlationId": "test-123"}'
```

## Dependencies

### Framework References
- **Core Framework** (`Framework/Core/Core`): Base classes, DTOs, exceptions, extensions
- **Cache Framework** (`Framework/Cache`): Caching capabilities (optional)

### NuGet Packages
- Microsoft.Azure.Functions.Worker (v2.0.0)
- Microsoft.Azure.Functions.Worker.Extensions.Http (v3.2.0)
- Microsoft.Azure.Functions.Worker.Extensions.Http.AspNetCore (v2.0.0)
- Microsoft.Azure.Functions.Worker.Sdk (v2.0.0)
- Microsoft.ApplicationInsights.WorkerService (v2.23.0)
- Microsoft.Azure.Functions.Worker.ApplicationInsights (v2.0.0)

## Error Handling

Errors are handled by the `ExceptionHandlerMiddleware` from the Core Framework. All exceptions are normalized to `BaseResponseDTO` format.

| Error Code | Message | HTTP Status |
|------------|---------|-------------|
| NET_SYS_0001 | Network test operation failed | 500 |
| NET_SYS_0002 | The network service is currently unavailable | 503 |
| NET_SYS_0003 | Request validation failed | 400 |

## Middleware Pipeline

The middleware pipeline is configured in `Program.cs`:

1. **ExecutionTimingMiddleware**: Tracks execution time and initializes response headers
2. **ExceptionHandlerMiddleware**: Catches and normalizes exceptions to BaseResponseDTO

## Notes

- This implementation does not require authentication (no `[CustomAuthentication]` attribute)
- No downstream API calls are made (simple health check)
- The response mirrors the original Boomi process output: "Test is successful!!!"

## License

Proprietary - Al Ghurair Investment LLC
