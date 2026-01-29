# sys-facilities-mgmt - CAFM/MRI System Layer Integration

Azure Functions v4 (.NET 8) implementation for CAFM/MRI (SOAP over HTTP) System Layer operations.

## Overview

This project provides System Layer integrations to CAFM/MRI services through HTTP-triggered Azure Functions. It follows a layered architecture:

```
Functions → MRI Handlers → Atomic Handlers → SOAP/HTTP calls
```

## Implemented Operations

| Operation | Function Name | Route | Description |
|-----------|---------------|-------|-------------|
| **CreateWorkOrder** | `CreateWorkOrder` | `POST /api/workorder/create` | Creates a new work order in CAFM/MRI |
| **GetBreakdownTask** | `GetBreakdownTask` | `POST /api/task/breakdown` | Retrieves breakdown tasks for a work order |
| **GetLocation** | `GetLocation` | `POST /api/location/get` | Retrieves location information |
| **GetInstructionSets** | `GetInstructionSets` | `POST /api/instructionsets/get` | Retrieves instruction sets for tasks |

## Project Structure

```
FacilitiesMgmtSystem/
├── Functions/                      # HTTP-triggered Azure Functions
├── Implementations/
│   └── MRI/
│       ├── Handlers/              # Business logic orchestration
│       └── AtomicHandlers/        # Single-responsibility SOAP operations
├── DTO/
│   ├── <FeatureName>DTO/          # API request/response DTOs
│   └── DownsteamDTOs/             # SOAP request/response DTOs
├── SoapEnvelopes/                 # SOAP XML templates (embedded resources)
├── Middlewares/                   # Function middleware (auth, error handling)
├── Attributes/                    # Custom attributes
├── Helper/                        # Utility classes (SOAP, XML, HTTP)
├── Constants/                     # Error and info constants
├── ConfigModels/                  # Configuration models
└── Core/Exceptions/               # Domain exceptions
```

## Prerequisites

- .NET 8.0 SDK
- Azure Functions Core Tools v4
- Azure Storage Emulator (for local development)

## Required Configuration

### Environment Variables

The following environment variables are required:

| Variable | Description |
|----------|-------------|
| `ENVIRONMENT` | Environment name (dev, qa, stg, prod, dr) |
| `AzureWebJobsStorage` | Azure Storage connection string |

### Application Settings (appsettings.{env}.json)

```json
{
  "MRI": {
    "BaseUrl": "<MRI SOAP Service Base URL>",
    "LoginEndpoint": "/api/soap/Login",
    "LogoutEndpoint": "/api/soap/Logout",
    "CreateWorkOrderEndpoint": "/api/soap/WorkOrder/Create",
    "GetBreakdownTaskEndpoint": "/api/soap/Task/GetBreakdown",
    "GetLocationEndpoint": "/api/soap/Location/Get",
    "GetInstructionSetsEndpoint": "/api/soap/InstructionSets/Get",
    "Username": "<MRI Username - TODO: Store in Key Vault>",
    "Password": "<MRI Password - TODO: Store in Key Vault>",
    "ContractId": "<MRI Contract ID - TODO: Store in Key Vault>",
    "SoapActionNamespace": "http://tempuri.org/",
    "ServiceNamespace": "http://tempuri.org/"
  },
  "HttpClient": {
    "TimeoutSeconds": 30,
    "RetryCount": 3,
    "RetryDelaySeconds": 2
  }
}
```

### Secrets Management

**Important:** The following values must be stored securely (e.g., Azure Key Vault):
- `MRI:Username`
- `MRI:Password`
- `MRI:ContractId`

TODO: Update appsettings files to reference Key Vault secrets using the appropriate secret reference syntax for your deployment model.

## Running Locally

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd sys-facilities-mgmt
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Configure local.settings.json**
   ```json
   {
     "IsEncrypted": false,
     "Values": {
       "AzureWebJobsStorage": "UseDevelopmentStorage=true",
       "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
       "ENVIRONMENT": "dev"
     }
   }
   ```

4. **Configure appsettings.dev.json**
   Update the MRI configuration with your development environment values.

5. **Start the Azure Storage Emulator**
   ```bash
   # Using Azurite
   azurite --silent --location ./azurite-data
   ```

6. **Run the function app**
   ```bash
   func start
   ```

   The functions will be available at:
   - `http://localhost:7071/api/workorder/create`
   - `http://localhost:7071/api/task/breakdown`
   - `http://localhost:7071/api/location/get`
   - `http://localhost:7071/api/instructionsets/get`

## Testing

### Run Unit Tests
```bash
dotnet test
```

### Sample Request - Create Work Order
```bash
curl -X POST http://localhost:7071/api/workorder/create \
  -H "Content-Type: application/json" \
  -d '{
    "description": "Repair broken HVAC unit",
    "priority": "High",
    "locationId": "LOC-001",
    "workOrderType": "Corrective",
    "requestedBy": "John Doe"
  }'
```

### Sample Request - Get Location
```bash
curl -X POST http://localhost:7071/api/location/get \
  -H "Content-Type: application/json" \
  -d '{
    "locationId": "LOC-001",
    "includeHierarchy": true
  }'
```

## Architecture

### Middleware Pipeline
1. **ExecutionTimingMiddleware** - Tracks function execution time
2. **ExceptionHandlerMiddleware** - Normalizes error responses
3. **MRIAuthenticationMiddleware** - Handles MRI session login/logout

### Authentication
Functions decorated with `[MRIAuthentication]` attribute automatically:
- Login to MRI before function execution
- Store session ID in function context
- Logout from MRI after function completion (in finally block)

### Error Handling
- `RequestValidationFailureException` - Validation errors (400 Bad Request)
- `DownStreamApiFailureException` - SOAP/API failures (502 Bad Gateway)
- `ApiException` - General API errors (configurable status code)
- `BaseException` - Application errors (500 Internal Server Error)

## Development Guidelines

See `.cursor/rules/system-layer-rules.mdc` for detailed coding standards and conventions.

### Key Points
- Place Functions in `Functions/`
- Place business logic in `Implementations/MRI/Handlers/`
- Place SOAP operations in `Implementations/MRI/AtomicHandlers/`
- Use SOAP envelope templates from `SoapEnvelopes/`
- Register new handlers in `Program.cs`
- Use existing Polly policies for HTTP resilience

## Deployment

The project is configured for Azure Functions v4 isolated worker process. Deploy using:

```bash
# Build for release
dotnet build -c Release

# Publish
dotnet publish -c Release -o ./publish

# Deploy to Azure (using Azure CLI)
az functionapp deployment source config-zip \
  -g <resource-group> \
  -n <function-app-name> \
  --src ./publish.zip
```

## CI/CD

GitHub Actions workflow is configured in `.github/workflows/dotnet-ci.yml` for:
- Build verification
- Unit test execution

## License

Proprietary - Internal use only.
