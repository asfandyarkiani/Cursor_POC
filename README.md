# CAFM System Layer

A .NET 8 microservice that provides integration with CAFM (Computer-Aided Facility Management) systems via SOAP/XML. This System Layer handles authentication, work order management, and facility data operations with built-in resilience patterns.

## Architecture

The solution follows Clean Architecture principles with clear separation of concerns:

```
src/
├── SystemLayer.Api/           # Web API controllers and configuration
├── SystemLayer.Application/   # Business interfaces and DTOs
└── SystemLayer.Infrastructure/ # CAFM client, XML processing, and external integrations

tests/
└── SystemLayer.Tests/         # Unit tests
```

## Features

### Core Operations
- **Authentication**: Login/logout with session management
- **Work Orders**: Create work orders with full metadata
- **Breakdown Tasks**: Retrieve task information for maintenance
- **Locations**: Get facility location details
- **Instruction Sets**: Fetch maintenance instruction sets

### Enterprise Features
- **Resilience**: Retry policies with exponential backoff and circuit breaker
- **Observability**: Structured logging with correlation IDs
- **Configuration**: Strongly-typed configuration with validation
- **Security**: Azure Key Vault integration for production secrets
- **Health Checks**: Built-in health monitoring endpoints

## Getting Started

### Prerequisites
- .NET 8 SDK
- Visual Studio 2022 or VS Code

### Local Development

1. **Clone and restore packages**:
   ```bash
   git clone <repository-url>
   cd SystemLayer
   dotnet restore
   ```

2. **Configure CAFM settings** in `src/SystemLayer.Api/appsettings.Development.json`:
   ```json
   {
     "Cafm": {
       "BaseUrl": "https://your-cafm-server.com/soap/endpoint",
       "Username": "your_cafm_username",
       "Password": "your_cafm_password",
       "Database": "your_cafm_database"
     }
   }
   ```

3. **Run the application**:
   ```bash
   cd src/SystemLayer.Api
   dotnet run
   ```

4. **Access Swagger UI**: Navigate to `https://localhost:7001/swagger`

### Configuration

#### Required Settings
```json
{
  "Cafm": {
    "BaseUrl": "https://cafm-server.com/soap",     // CAFM SOAP endpoint
    "Username": "cafm_user",                        // CAFM username
    "Password": "cafm_password",                    // CAFM password (use Key Vault in production)
    "Database": "cafm_database",                    // CAFM database name
    "TimeoutSeconds": 30                            // HTTP timeout
  }
}
```

#### Optional Resilience Settings
```json
{
  "Cafm": {
    "Retry": {
      "MaxAttempts": 3,
      "BaseDelaySeconds": 2,
      "MaxDelaySeconds": 30
    },
    "CircuitBreaker": {
      "FailureThreshold": 5,
      "DurationOfBreakSeconds": 30
    }
  }
}
```

#### Production Configuration with Azure Key Vault
```json
{
  "KeyVault": {
    "Url": "https://your-keyvault.vault.azure.net/"
  },
  "Cafm": {
    "BaseUrl": "https://prod-cafm-server.com/soap",
    "Username": "prod_cafm_user",
    // Password will be loaded from Key Vault secret "Cafm--Password"
    "Database": "prod_cafm_database"
  }
}
```

## API Endpoints

### Authentication
- `POST /api/cafm/login` - Login to CAFM system
- `POST /api/cafm/logout` - Logout from CAFM system

### Work Orders
- `POST /api/cafm/work-orders` - Create a new work order

### Facility Data
- `GET /api/cafm/breakdown-tasks` - Get breakdown task information
- `GET /api/cafm/locations` - Get location details
- `GET /api/cafm/instruction-sets` - Get maintenance instruction sets

### Health & Monitoring
- `GET /health` - Health check endpoint

## Example Usage

### Creating a Work Order
```json
POST /api/cafm/work-orders
{
  "workOrderNumber": "WO-2024-001",
  "description": "HVAC maintenance required",
  "priority": "High",
  "locationId": "LOC-001",
  "assetId": "HVAC-001",
  "requestedBy": "John Doe",
  "requestedDate": "2024-01-15T10:30:00Z",
  "workType": "Preventive",
  "status": "Open",
  "breakdownTaskId": "BT-001",
  "instructionSetIds": ["IS-001", "IS-002"]
}
```

### Getting Location Information
```http
GET /api/cafm/locations?locationId=LOC-001&buildingId=BLD-001
```

## Testing

### Run Unit Tests
```bash
dotnet test
```

### Run with Coverage
```bash
dotnet test --collect:"XPlat Code Coverage"
```

## Deployment

### Docker
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "SystemLayer.Api.dll"]
```

### Azure App Service
1. Configure Application Settings with CAFM connection details
2. Set up Key Vault integration for secrets
3. Configure Application Insights for monitoring

## Security Considerations

### Production Checklist
- [ ] Store CAFM credentials in Azure Key Vault
- [ ] Enable HTTPS only
- [ ] Configure proper CORS policies
- [ ] Set up authentication/authorization if needed
- [ ] Enable Application Insights monitoring
- [ ] Configure proper logging levels (avoid logging sensitive data)

### Sensitive Data Handling
- Passwords are masked in logs
- Session tokens are not logged
- XML requests/responses can be logged at Debug level (review for sensitive data)

## Troubleshooting

### Common Issues

1. **CAFM Connection Timeout**
   - Check `Cafm:BaseUrl` configuration
   - Verify network connectivity to CAFM server
   - Increase `Cafm:TimeoutSeconds` if needed

2. **Authentication Failures**
   - Verify `Cafm:Username`, `Cafm:Password`, and `Cafm:Database`
   - Check CAFM server logs for authentication errors

3. **SOAP Parsing Errors**
   - Enable Debug logging to see raw XML responses
   - Check if CAFM server response format has changed

### Logging
- Set log level to `Debug` for detailed XML request/response logging
- Use structured logging with correlation IDs for request tracing
- Monitor Application Insights for production issues

## Contributing

1. Follow Clean Architecture principles
2. Add unit tests for new features
3. Update documentation for configuration changes
4. Use conventional commit messages

## Support

For issues related to:
- **CAFM Integration**: Check CAFM server documentation and contact CAFM administrator
- **System Layer Code**: Create an issue in this repository
- **Infrastructure/Deployment**: Contact DevOps team