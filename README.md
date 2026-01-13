# CAFM Integration System Layer (.NET)

A clean, enterprise-grade System Layer for integrating with CAFM/MRI systems via SOAP/XML, built with .NET 8 and following clean architecture principles.

## Overview

This System Layer provides a robust integration interface between Process Layer components and CAFM (Computer-Aided Facility Management) systems. It handles all SOAP/XML communication, authentication, error handling, and data transformation while maintaining clean separation of concerns.

## Architecture

The solution follows clean architecture principles with clear layer separation:

```
┌─────────────────────────────────────────────────────────────┐
│                    Process Layer                             │
│              (Not included in this solution)                │
└─────────────────────────┬───────────────────────────────────┘
                          │ HTTP/REST API Calls
                          ▼
┌─────────────────────────────────────────────────────────────┐
│                  System Layer API                           │
│  • Controllers (CafmController)                             │
│  • Dependency Injection Configuration                       │
│  • Health Checks & Observability                           │
└─────────────────────────┬───────────────────────────────────┘
                          │
                          ▼
┌─────────────────────────────────────────────────────────────┐
│                 Application Layer                           │
│  • Interfaces (ICafmService, ICafmClient)                  │
│  • DTOs (Request/Response models)                          │
│  • Error Definitions                                       │
└─────────────────────────┬───────────────────────────────────┘
                          │
                          ▼
┌─────────────────────────────────────────────────────────────┐
│               Infrastructure Layer                          │
│  • CAFM Client Implementation                              │
│  • SOAP Message Builders                                   │
│  • DTO Mapping Services                                    │
│  • Resilience Policies (Polly)                            │
│  • Configuration Management                                │
└─────────────────────────┬───────────────────────────────────┘
                          │ SOAP/XML over HTTP
                          ▼
┌─────────────────────────────────────────────────────────────┐
│                   CAFM/MRI System                          │
│              (External SOAP Web Service)                   │
└─────────────────────────────────────────────────────────────┘
```

## Features

### Core Functionality
- **Work Order Management**: Create work orders in CAFM system
- **Location Services**: Retrieve location information and hierarchies
- **Breakdown Tasks**: Access breakdown task definitions and details
- **Instruction Sets**: Fetch maintenance instruction sets and procedures

### Technical Features
- **Clean Architecture**: Proper separation of concerns with Application and Infrastructure layers
- **SOAP/XML Integration**: Robust SOAP message building and parsing
- **Authentication Flow**: Automatic login/logout wrapper for all operations
- **Resilience Patterns**: Retry policies, circuit breakers, and timeout handling via Polly
- **Error Handling**: Comprehensive error classification and mapping
- **Configuration Management**: Options pattern with Azure Key Vault support
- **Observability**: Structured logging with correlation IDs
- **Testing**: Comprehensive unit test coverage (49 tests)

## Project Structure

```
SystemLayer/
├── src/
│   ├── SystemLayer.Api/              # Web API controllers and startup
│   │   ├── Controllers/
│   │   │   └── CafmController.cs     # REST API endpoints
│   │   ├── Program.cs                # Application startup
│   │   └── appsettings.json          # Configuration
│   │
│   ├── SystemLayer.Application/      # Application layer contracts
│   │   ├── DTOs/                     # Data transfer objects
│   │   │   ├── ProcessLayerDTOs.cs   # Request/response models
│   │   │   └── SystemLayerErrors.cs  # Error handling models
│   │   └── Interfaces/               # Service contracts
│   │       ├── ICafmService.cs       # High-level CAFM operations
│   │       └── ICafmClient.cs        # Low-level CAFM client
│   │
│   └── SystemLayer.Infrastructure/   # Infrastructure implementations
│       ├── Configuration/            # Configuration options
│       ├── DependencyInjection/      # Service registration
│       ├── Mapping/                  # DTO mapping services
│       ├── Models/                   # CAFM SOAP models
│       ├── Services/                 # Service implementations
│       └── Soap/                     # SOAP message handling
│
└── tests/
    └── SystemLayer.Tests/            # Unit tests
        ├── Configuration/            # Configuration validation tests
        ├── DTOs/                     # DTO and error handling tests
        ├── Mapping/                  # Mapping service tests
        ├── Services/                 # Service logic tests
        └── Soap/                     # SOAP message builder tests
```

## Getting Started

### Prerequisites

- .NET 8.0 SDK
- Visual Studio 2022 or VS Code
- Access to CAFM/MRI system (for integration testing)

### Local Development Setup

1. **Clone and Build**
   ```bash
   git clone <repository-url>
   cd SystemLayer
   dotnet restore
   dotnet build
   ```

2. **Configure CAFM Connection**
   
   Update `src/SystemLayer.Api/appsettings.Development.json`:
   ```json
   {
     "Cafm": {
       "BaseUrl": "https://your-cafm-server.com",
       "ServicePath": "/services/CafmService.asmx",
       "Username": "your-integration-user",
       "Password": "your-password",
       "Database": "your-cafm-database",
       "EnableDetailedLogging": true
     }
   }
   ```

3. **Run the Application**
   ```bash
   dotnet run --project src/SystemLayer.Api
   ```

4. **Access Swagger UI**
   
   Navigate to `http://localhost:5000` to explore the API documentation.

### Production Configuration

For production deployments, use Azure Key Vault for sensitive configuration:

1. **Enable Key Vault**
   ```json
   {
     "KeyVault": {
       "Enabled": true,
       "VaultUrl": "https://your-keyvault.vault.azure.net/",
       "ClientId": "your-managed-identity-client-id"
     }
   }
   ```

2. **Store Secrets in Key Vault**
   - `Cafm--Password`: CAFM system password
   - `Cafm--Username`: CAFM system username (if needed)

3. **Configure Managed Identity**
   
   Ensure your Azure App Service or Container has a managed identity with access to the Key Vault.

## API Endpoints

### Work Orders
- `POST /api/cafm/work-orders` - Create a new work order
- Request body example:
  ```json
  {
    "workOrderNumber": "WO-12345",
    "description": "Replace HVAC filter",
    "locationId": "LOC-001",
    "priority": "High",
    "assignedTo": "John Doe",
    "scheduledDate": "2024-01-15T10:00:00Z"
  }
  ```

### Locations
- `GET /api/cafm/locations/{locationId}` - Get location details
- Query parameters:
  - `includeHierarchy`: Include parent/child location relationships

### Breakdown Tasks
- `GET /api/cafm/breakdown-tasks/{taskId}` - Get breakdown task details
- Query parameters:
  - `includeDetails`: Include detailed task information

### Instruction Sets
- `GET /api/cafm/instruction-sets` - Get instruction sets
- Query parameters:
  - `categoryFilter`: Filter by category
  - `assetTypeFilter`: Filter by asset type
  - `maxResults`: Maximum number of results (default: 100)

### Health Check
- `GET /api/cafm/health` - System health status

## Configuration Reference

### CAFM Configuration
```json
{
  "Cafm": {
    "BaseUrl": "https://cafm-server.com",           // CAFM server URL
    "ServicePath": "/services/CafmService.asmx",    // SOAP service path
    "Username": "integration_user",                  // CAFM username
    "Password": "secure_password",                   // CAFM password (use Key Vault)
    "Database": "CAFM_PROD",                        // CAFM database name
    "TimeoutSeconds": 30,                           // Request timeout
    "MaxRetryAttempts": 3,                          // Retry attempts
    "BaseDelayMs": 1000,                           // Base retry delay
    "CircuitBreakerThreshold": 5,                   // Circuit breaker threshold
    "CircuitBreakerDurationSeconds": 30,            // Circuit breaker duration
    "EnableDetailedLogging": false                   // Enable detailed SOAP logging
  }
}
```

### Resilience Configuration
```json
{
  "Resilience": {
    "Retry": {
      "MaxAttempts": 3,                             // Maximum retry attempts
      "BaseDelayMs": 1000,                          // Base delay between retries
      "MaxDelayMs": 10000                           // Maximum delay between retries
    },
    "CircuitBreaker": {
      "FailureThreshold": 5,                        // Failures before opening circuit
      "DurationSeconds": 30,                        // Circuit open duration
      "MinimumThroughput": 10                       // Minimum requests for circuit activation
    },
    "Timeout": {
      "TimeoutSeconds": 30                          // Request timeout
    }
  }
}
```

## Error Handling

The System Layer provides comprehensive error classification:

### Error Categories
- **Authentication Errors** (`SL_AUTH_*`): Login/session issues
- **CAFM System Errors** (`SL_CAFM_*`): CAFM connectivity and SOAP faults
- **Validation Errors** (`SL_VAL_*`): Request validation failures
- **Business Logic Errors** (`SL_BIZ_*`): Business rule violations
- **System Errors** (`SL_SYS_*`): Internal system issues

### Error Response Format
```json
{
  "success": false,
  "data": null,
  "error": {
    "errorCode": "SL_CAFM_001",
    "message": "Failed to connect to CAFM system",
    "isRetryable": true,
    "timestamp": "2024-01-15T10:30:00Z",
    "correlationId": "abc123-def456"
  },
  "correlationId": "abc123-def456"
}
```

## Testing

### Run Unit Tests
```bash
dotnet test
```

### Test Coverage
The solution includes 49 unit tests covering:
- DTO mapping and transformations
- SOAP message building and parsing
- Service layer business logic
- Error handling scenarios
- Configuration validation

### Integration Testing
For integration testing with actual CAFM systems:

1. Configure test CAFM environment in `appsettings.Testing.json`
2. Create integration test project
3. Use test data that won't affect production systems

## Monitoring and Observability

### Logging
- Structured logging with correlation IDs
- Configurable log levels per component
- Sensitive data masking in logs

### Health Checks
- Basic health endpoint at `/health`
- Can be extended with CAFM connectivity checks

### Metrics
Ready for integration with:
- Application Insights
- Prometheus/Grafana
- Custom metrics collection

## Security Considerations

### Authentication
- CAFM credentials stored in Azure Key Vault
- Managed Identity for Key Vault access
- No hardcoded credentials in code

### Network Security
- HTTPS-only communication
- Configurable timeouts and rate limiting
- Circuit breaker protection against cascading failures

### Data Protection
- Sensitive data masking in logs
- Secure credential handling
- Input validation and sanitization

## Deployment

### Azure App Service
1. Create App Service with .NET 8 runtime
2. Configure Managed Identity
3. Set up Key Vault access
4. Deploy using Azure DevOps or GitHub Actions

### Docker Container
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0
COPY . /app
WORKDIR /app
EXPOSE 80
ENTRYPOINT ["dotnet", "SystemLayer.Api.dll"]
```

### Kubernetes
Use the provided Kubernetes manifests in the `k8s/` directory (if available).

## Troubleshooting

### Common Issues

1. **CAFM Connection Failures**
   - Verify CAFM server URL and credentials
   - Check network connectivity
   - Review CAFM server logs

2. **Authentication Issues**
   - Validate CAFM username/password
   - Check CAFM user permissions
   - Verify database access

3. **SOAP Parsing Errors**
   - Enable detailed logging
   - Check CAFM SOAP response format
   - Validate XML schema compatibility

4. **Performance Issues**
   - Adjust timeout settings
   - Review retry policy configuration
   - Monitor circuit breaker status

### Debugging
Enable detailed logging in development:
```json
{
  "Logging": {
    "LogLevel": {
      "SystemLayer": "Debug",
      "System.Net.Http.HttpClient": "Information"
    }
  },
  "Cafm": {
    "EnableDetailedLogging": true
  }
}
```

## Contributing

1. Follow clean architecture principles
2. Maintain comprehensive test coverage
3. Use structured logging with correlation IDs
4. Update documentation for new features
5. Follow .NET coding standards and conventions

## License

[Specify your license here]

## Support

For technical support or questions:
- Create an issue in the repository
- Contact the development team
- Review the troubleshooting section above