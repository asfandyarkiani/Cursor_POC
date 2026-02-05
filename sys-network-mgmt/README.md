# sys-network-mgmt

System Layer API for Network Management operations. This API provides network connectivity testing and health check functionality.

## Overview

This System Layer API is part of the API-Led Architecture implementation for enterprise integration systems. It belongs to the **System Layer** which unlocks data from core systems of record and insulates users from the complexity of underlying data sources.

### Domain
- **Business Domain:** Network
- **SOR (System of Record):** Network Infrastructure

## API Operations

### Network Test (Health Check)

A simple health check endpoint to verify network connectivity and API availability.

- **Endpoint:** `GET/POST /api/network/test`
- **Authentication:** None required (Anonymous)
- **Request Body:** None required
- **Response:** JSON containing success message, timestamp, and environment

## Architecture

### Sequence Diagram (ASCII)

```
┌─────────┐     ┌─────────────────┐     ┌───────────────────┐     ┌──────────────────┐
│ Process │     │  NetworkTestAPI │     │ NetworkMgmtService│     │ NetworkTestHandler│
│  Layer  │     │   (Function)    │     │     (Service)     │     │    (Handler)      │
└────┬────┘     └────────┬────────┘     └─────────┬─────────┘     └─────────┬────────┘
     │                   │                        │                         │
     │  GET /api/network/test                     │                         │
     │─────────────────>│                         │                         │
     │                   │                        │                         │
     │                   │  NetworkTest(request)  │                         │
     │                   │──────────────────────>│                         │
     │                   │                        │                         │
     │                   │                        │  HandleAsync(request)   │
     │                   │                        │────────────────────────>│
     │                   │                        │                         │
     │                   │                        │                         │ ┌─────────────┐
     │                   │                        │                         │ │ Return      │
     │                   │                        │                         │ │ "Test is    │
     │                   │                        │                         │ │ successful!"│
     │                   │                        │                         │ └──────┬──────┘
     │                   │                        │                         │        │
     │                   │                        │       BaseResponseDTO   │<───────┘
     │                   │                        │<────────────────────────│
     │                   │                        │                         │
     │                   │    BaseResponseDTO     │                         │
     │                   │<───────────────────────│                         │
     │                   │                        │                         │
     │    BaseResponseDTO                         │                         │
     │<──────────────────│                        │                         │
     │                   │                        │                         │
```

### Component Flow

```
NetworkTestAPI (Function)
         │
         ▼
  INetworkMgmt (Interface)
         │
         ▼
NetworkMgmtService (Service)
         │
         ▼
NetworkTestHandler (Handler)
         │
         ▼
    [No External API Call - Simple Health Check]
```

## Folder Structure

```
sys-network-mgmt/
├── Abstractions/                           # Interface definitions
│   └── INetworkMgmt.cs
├── ConfigModels/                           # Configuration models
│   └── AppConfigs.cs
├── Constants/                              # Error and Info constants
│   ├── ErrorConstants.cs
│   └── InfoConstants.cs
├── DTO/
│   ├── HandlerDTOs/
│   │   └── NetworkTestDTO/
│   │       ├── NetworkTestReqDTO.cs
│   │       └── NetworkTestResDTO.cs
│   ├── AtomicHandlerDTOs/                  # (Empty - no downstream calls)
│   └── DownstreamDTOs/                     # (Empty - no downstream calls)
├── Functions/                              # Azure Functions (HTTP entry points)
│   └── NetworkTestAPI.cs
├── Implementations/Network/
│   ├── Services/
│   │   └── NetworkMgmtService.cs
│   ├── Handlers/
│   │   └── NetworkTestHandler.cs
│   └── AtomicHandlers/                     # (Empty - no downstream calls)
├── appsettings.json
├── appsettings.dev.json
├── appsettings.qa.json
├── appsettings.prod.json
├── host.json
├── local.settings.json
├── Program.cs
├── sys-network-mgmt.csproj
├── sys-network-mgmt.sln
└── README.md
```

## Configuration

### Required Configuration (appsettings.json)

```json
{
  "AppConfigs": {
    "AppName": "sys-network-mgmt",
    "Environment": "dev|qa|stg|prod"
  }
}
```

### Environment Variables

| Variable | Description | Required |
|----------|-------------|----------|
| `ASPNETCORE_ENVIRONMENT` | Environment name (Development, Production) | No (defaults to Production) |
| `AzureWebJobsStorage` | Azure Storage connection string | Yes (for Azure deployment) |
| `FUNCTIONS_WORKER_RUNTIME` | Must be `dotnet-isolated` | Yes |

## Running Locally

### Prerequisites

- .NET 8.0 SDK
- Azure Functions Core Tools v4
- Azure Storage Emulator or Azurite (for local development)

### Steps

1. **Clone the repository:**
   ```bash
   git clone <repository-url>
   cd sys-network-mgmt
   ```

2. **Restore dependencies:**
   ```bash
   dotnet restore
   ```

3. **Build the project:**
   ```bash
   dotnet build
   ```

4. **Run locally:**
   ```bash
   func start
   ```
   
   Or using .NET:
   ```bash
   dotnet run
   ```

5. **Test the endpoint:**
   ```bash
   curl http://localhost:7071/api/network/test
   ```

### Expected Response

```json
{
  "message": "Test is successful!!!",
  "errorCode": null,
  "data": {
    "message": "Test is successful!!!",
    "timestamp": "2025-01-21T12:00:00Z",
    "environment": "dev"
  },
  "errorDetails": null,
  "isDownStreamError": false,
  "isPartialSuccess": false
}
```

## Deployment

### Azure Functions

1. Create an Azure Functions App (Consumption or Premium plan)
2. Configure application settings with required environment variables
3. Deploy using:
   - Azure DevOps pipelines
   - GitHub Actions
   - Azure CLI: `func azure functionapp publish <app-name>`

## Dependencies

### Framework References
- `Core` - Core framework library (from `/Framework/Core/Core/`)
- `Cache` - Cache framework library (from `/Framework/Cache/`)

### NuGet Packages
- Microsoft.Azure.Functions.Worker
- Microsoft.Azure.Functions.Worker.Extensions.Http
- Microsoft.Azure.Functions.Worker.Extensions.Http.AspNetCore
- Microsoft.ApplicationInsights.WorkerService
- Microsoft.Azure.Functions.Worker.ApplicationInsights

## API Led Architecture Notes

This System Layer API follows the API-Led Architecture principles:

- **System Layer (This API):** Provides raw access to network infrastructure data/operations
- **Process Layer:** Would orchestrate multiple System APIs to create business processes
- **Experience Layer:** Would transform data for specific UI/UX needs

### Boomi Process Source

This implementation is derived from the Boomi process:
- **Process Name:** Network Test
- **Component ID:** 446fc0bf-4f22-4ddc-a5cf-a11e091bceec
- **Folder:** Al Ghurair Investment LLC/Network

The original Boomi process was a simple WSS (Web Services Server) listener that returns a success message, which has been translated to this Azure Functions-based System Layer API.

## Support

For issues or questions, please contact the integration team.
