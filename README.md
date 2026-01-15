# Facilities Management System (sys-facilities-mgmt)

Azure Functions v4 .NET 8 implementation for CAFM/MRI System Layer integrations.

## Project Structure

```
/workspace
├── src/
│   └── FacilitiesMgmtSystem/
│       ├── Attributes/             # Custom attributes (e.g., MRIAuthentication)
│       ├── ConfigModels/           # Configuration model classes
│       ├── Constants/              # Error and info constants
│       ├── Core/                   # Core exceptions
│       │   ├── Exceptions/
│       │   └── System/Exceptions/
│       ├── DTO/                    # Data Transfer Objects
│       │   ├── NetworkTestDTO/     # API DTOs for NetworkTest
│       │   └── DownsteamDTOs/      # Downstream API DTOs
│       ├── Functions/              # Azure Functions (HTTP triggers)
│       ├── Helper/                 # Utility classes (SOAP, XML, HTTP helpers)
│       ├── Implementations/
│       │   └── MRI/
│       │       ├── Handlers/       # Business logic handlers
│       │       └── AtomicHandlers/ # Single-responsibility downstream handlers
│       ├── Middlewares/            # Function middleware
│       └── SoapEnvelopes/          # SOAP XML templates (embedded resources)
├── tests/
│   └── FacilitiesMgmtSystem.Tests/
└── FacilitiesMgmtSystem.sln
```

## Available Endpoints

### NetworkTest (Health Check)
- **Endpoint**: `POST /api/NetworkTest` or `GET /api/NetworkTest`
- **Description**: Network connectivity test / health check endpoint
- **Returns**: "Test is successful!!!" message with timestamp
- **Authentication**: None (public endpoint)

## Configuration

### Required Configuration Values

The following configuration values must be set in `appsettings.{environment}.json` files:

| Key | Description | Example |
|-----|-------------|---------|
| `MriBaseUrl` | MRI SOAP API base URL | `https://mri.example.com/soap` |
| `MriLoginSoapAction` | MRI Login SOAP action URL | `http://mri.example.com/Login` |
| `MriLogoutSoapAction` | MRI Logout SOAP action URL | `http://mri.example.com/Logout` |
| `MriUsername` | MRI service account username | (Retrieve from Key Vault) |
| `MriPassword` | MRI service account password | (Retrieve from Key Vault) |
| `MriContractId` | MRI Contract ID for CAFM | (Environment-specific) |
| `HttpTimeoutSeconds` | HTTP client timeout | `30` |
| `MaxRetryAttempts` | Max retry attempts | `3` |

### Secrets Management

**Important**: MRI credentials (`MriUsername`, `MriPassword`) should NOT be stored in configuration files. Use:
- Azure Key Vault (recommended for production)
- Environment variables for local development
- Managed Identity for Azure deployments

### Environment Detection

The application detects the environment using:
1. `ENVIRONMENT` environment variable
2. `ASPNETCORE_ENVIRONMENT` environment variable
3. Defaults to `dev` if neither is set

## Local Development

### Prerequisites

- .NET 8 SDK
- Azure Functions Core Tools v4
- Azure Storage Emulator or Azurite (for local development)

### Running Locally

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd workspace
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Configure local settings**
   
   Update `src/FacilitiesMgmtSystem/local.settings.json`:
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

4. **Build the project**
   ```bash
   dotnet build
   ```

5. **Run the functions**
   ```bash
   cd src/FacilitiesMgmtSystem
   func start
   ```

6. **Test the NetworkTest endpoint**
   ```bash
   curl http://localhost:7071/api/NetworkTest
   ```

### Running Tests

```bash
dotnet test
```

## Architecture

### Layered Architecture

```
Functions → MRI Handlers → Atomic Handlers → SOAP/HTTP Calls
```

1. **Functions**: HTTP-triggered Azure Functions that receive requests
2. **MRI Handlers**: Orchestrate business logic, transform between API and downstream DTOs
3. **Atomic Handlers**: Single-responsibility handlers for individual downstream operations
4. **SOAP/HTTP Calls**: Use `CustomSoapClient` and `CustomHTTPClient` with Polly resilience

### Middleware Pipeline

```
ExecutionTimingMiddleware → ExceptionHandlerMiddleware → MRIAuthenticationMiddleware
```

- **ExecutionTimingMiddleware**: Logs execution time for all function invocations
- **ExceptionHandlerMiddleware**: Catches and normalizes exceptions into `BaseResponseDTO`
- **MRIAuthenticationMiddleware**: Handles MRI session management for functions with `[MRIAuthentication]` attribute

### Authentication

For endpoints requiring MRI session:
1. Decorate the function with `[MRIAuthentication]` attribute
2. Middleware automatically handles login before function execution
3. Session ID is available via `context.Items[InfoConstants.SESSION_ID]`
4. Middleware automatically handles logout in `finally` block

## Adding New Features

### Adding a New Endpoint

1. Create DTOs in `DTO/<FeatureName>DTO/`
2. Create handler in `Implementations/MRI/Handlers/<FeatureName>MRIHandler.cs`
3. Create atomic handler (if needed) in `Implementations/MRI/AtomicHandlers/<FeatureName>AtomicHandler/`
4. Create function in `Functions/<FeatureName>API.cs`
5. Register handler in `Program.cs`
6. Add SOAP envelope (if needed) in `SoapEnvelopes/` and register as embedded resource
7. Add unit tests

### Adding SOAP Envelopes

1. Create XML file in `SoapEnvelopes/<OperationName>.xml`
2. Register as embedded resource in `FacilitiesMgmtSystem.csproj`:
   ```xml
   <ItemGroup>
     <EmbeddedResource Include="SoapEnvelopes\<OperationName>.xml" />
   </ItemGroup>
   ```
3. Load in atomic handler using `CustomSoapClient.LoadEnvelopeTemplate("SoapEnvelopes.<OperationName>.xml")`

## Error Handling

### Exception Types

| Exception | Usage |
|-----------|-------|
| `RequestValidationFailureException` | Request validation errors |
| `DownStreamApiFailureException` | Downstream API failures |
| `ApiException` | General API errors |
| `BaseException` | Application errors |

### Error Response Format

All errors are normalized to `BaseResponseDTO`:
```json
{
  "success": false,
  "message": "Error description",
  "errors": ["INVALID_REQ_PAYLOAD"]
}
```

## CI/CD

The project includes a GitHub Actions workflow (`.github/workflows/dotnet-ci.yml`) that:
- Restores dependencies
- Builds the solution
- Runs all unit tests

## License

Proprietary - Al Ghurair Investment LLC
