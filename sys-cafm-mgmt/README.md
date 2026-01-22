# System Layer: CAFM Management (sys-cafm-mgmt)

## Overview

This System Layer repository provides integration with the **FSI CAFM (Computer-Aided Facility Management)** system. It exposes Azure Functions as HTTP endpoints to enable Process Layer and Experience Layer to interact with CAFM for work order management, location lookups, and task management.

## System of Record (SOR)

- **SOR Name**: FSI CAFM
- **Vendor**: FSI (Facilities Solutions International)
- **Protocol**: SOAP over HTTP
- **Base URL**: 
  - DEV: `https://devcafm.agfacilities.com`
  - QA: `https://qacafm.agfacilities.com`
  - STG: `https://stgcafm.agfacilities.com`
  - PROD: `https://prodcafm.agfacilities.com`

## Architecture

This repository follows the **API-Led Architecture** pattern for System Layer:

```
Azure Functions (HTTP Entry Points)
    ↓
Services (ICafmMgmt)
    ↓
Handlers (Orchestration)
    ↓
Atomic Handlers (Single SOAP Operation)
    ↓
FSI CAFM SOAP APIs
```

### Key Components

1. **Functions**: HTTP-triggered Azure Functions exposing System Layer endpoints
2. **Services**: `CafmMgmtService` implements `ICafmMgmt` interface
3. **Handlers**: `CafmWorkOrderHandler` orchestrates multiple atomic operations
4. **Atomic Handlers**: Each handles a single SOAP operation (Login, Logout, GetLocations, etc.)
5. **DTOs**: 
   - API DTOs: Request/Response for Azure Functions
   - DownStream DTOs: SOAP Request/Response for CAFM operations

## API Endpoints

### 1. Create Work Order
- **Endpoint**: `POST /api/cafm/workorders`
- **Description**: Creates a work order in CAFM system
- **Request Body**:
```json
{
  "reporterName": "John Doe",
  "reporterEmail": "john.doe@example.com",
  "reporterPhoneNumber": "+971501234567",
  "description": "AC not working in office",
  "serviceRequestNumber": "SR-2025-001",
  "propertyName": "Al Ghurair Tower",
  "unitCode": "UNIT-101",
  "categoryName": "HVAC",
  "subCategory": "Air Conditioning",
  "technician": "Tech-001",
  "sourceOrgId": "ORG-001",
  "ticketDetails": {
    "status": "Open",
    "subStatus": "Pending Assignment",
    "priority": "High",
    "scheduledDate": "2025-01-25",
    "scheduledTimeStart": "09:00",
    "scheduledTimeEnd": "17:00",
    "recurrence": "None",
    "raisedDateUtc": "2025-01-22T10:00:00Z"
  }
}
```

### 2. Get Locations
- **Endpoint**: `POST /api/cafm/locations`
- **Description**: Retrieves locations from CAFM
- **Request Body**:
```json
{
  "locationCode": "LOC-001",
  "propertyName": "Al Ghurair Tower"
}
```

### 3. Get Instruction Sets
- **Endpoint**: `POST /api/cafm/instructionsets`
- **Description**: Retrieves instruction sets from CAFM
- **Request Body**:
```json
{
  "instructionSetCode": "INST-001",
  "categoryName": "HVAC"
}
```

### 4. Get Breakdown Tasks
- **Endpoint**: `POST /api/cafm/breakdowntasks`
- **Description**: Retrieves breakdown tasks from CAFM
- **Request Body**:
```json
{
  "breakdownTaskCode": "BT-001",
  "categoryName": "HVAC"
}
```

### 5. Create Event
- **Endpoint**: `POST /api/cafm/events`
- **Description**: Creates an event/links a task in CAFM
- **Request Body**:
```json
{
  "eventType": "WorkOrder",
  "description": "Scheduled maintenance",
  "locationId": "LOC-001",
  "priority": "Medium",
  "scheduledDate": "2025-01-25",
  "taskId": "TASK-001"
}
```

### 6. Login
- **Endpoint**: `POST /api/cafm/login`
- **Description**: Authenticates with CAFM and returns session ID

### 7. Logout
- **Endpoint**: `POST /api/cafm/logout`
- **Description**: Terminates CAFM session
- **Request Body**:
```json
{
  "sessionId": "SESSION-ID-HERE"
}
```

## API Sequence Diagram

```
┌─────────────┐         ┌──────────────┐         ┌─────────────┐         ┌──────────────┐
│ Process/    │         │ sys-cafm-mgmt│         │   CAFM      │         │    Email     │
│ Experience  │         │ (System Layer)│         │   System    │         │   Service    │
│   Layer     │         │               │         │   (FSI)     │         │  (Office365) │
└──────┬──────┘         └───────┬──────┘         └──────┬──────┘         └──────┬───────┘
       │                        │                        │                        │
       │ POST /workorders       │                        │                        │
       │───────────────────────>│                        │                        │
       │                        │                        │                        │
       │                        │ SOAP: Authenticate     │                        │
       │                        │───────────────────────>│                        │
       │                        │                        │                        │
       │                        │ <SessionId>            │                        │
       │                        │<───────────────────────│                        │
       │                        │                        │                        │
       │                        │ SOAP: GetLocationsByDto│                        │
       │                        │───────────────────────>│                        │
       │                        │                        │                        │
       │                        │ <Locations>            │                        │
       │                        │<───────────────────────│                        │
       │                        │                        │                        │
       │                        │ SOAP: GetInstructionSets│                       │
       │                        │───────────────────────>│                        │
       │                        │                        │                        │
       │                        │ <InstructionSets>      │                        │
       │                        │<───────────────────────│                        │
       │                        │                        │                        │
       │                        │ SOAP: GetBreakdownTasks│                        │
       │                        │───────────────────────>│                        │
       │                        │                        │                        │
       │                        │ <BreakdownTasks>       │                        │
       │                        │<───────────────────────│                        │
       │                        │                        │                        │
       │                        │ SOAP: CreateEvent      │                        │
       │                        │───────────────────────>│                        │
       │                        │                        │                        │
       │                        │ <EventId, EventNumber> │                        │
       │                        │<───────────────────────│                        │
       │                        │                        │                        │
       │                        │ SOAP: LogOut           │                        │
       │                        │───────────────────────>│                        │
       │                        │                        │                        │
       │                        │ <Success>              │                        │
       │                        │<───────────────────────│                        │
       │                        │                        │                        │
       │ <WorkOrderResponse>    │                        │                        │
       │<───────────────────────│                        │                        │
       │                        │                        │                        │
       │                        │ (On Error)             │                        │
       │                        │ SMTP: Send Email       │                        │
       │                        │────────────────────────┼───────────────────────>│
       │                        │                        │                        │
       │                        │                        │    <Email Sent>        │
       │                        │<───────────────────────┼────────────────────────│
       │                        │                        │                        │
```

## Configuration

### Required Settings

All configuration is managed through `appsettings.{environment}.json` files:

```json
{
  "AppConfigs": {
    "CafmSettings": {
      "BaseUrl": "https://devcafm.agfacilities.com",
      "Username": "YOUR_CAFM_USERNAME",
      "Password": "YOUR_CAFM_PASSWORD",
      "LoginResourcePath": "/FSIWebServices/EvolutionService.asmx",
      "TimeoutSeconds": 60
    },
    "EmailSettings": {
      "SmtpHost": "smtp-mail.outlook.com",
      "SmtpPort": 587,
      "FromEmail": "notifications@al-ghurair.com",
      "Username": "YOUR_EMAIL_USERNAME",
      "Password": "YOUR_EMAIL_PASSWORD",
      "EnableTls": true
    }
  }
}
```

### Environment-Specific Configuration

- **appsettings.dev.json**: Development environment
- **appsettings.qa.json**: QA environment
- **appsettings.stg.json**: Staging environment
- **appsettings.prod.json**: Production environment
- **appsettings.dr.json**: Disaster Recovery environment

### Secrets Management

**IMPORTANT**: Never commit actual credentials to source control. Use:
- Azure Key Vault for production secrets
- Local secrets manager for development
- Environment variables for CI/CD pipelines

Replace all `TODO_*` placeholders in appsettings files with actual values from secure sources.

## Local Development Setup

### Prerequisites

- .NET 8 SDK
- Azure Functions Core Tools v4
- Visual Studio 2022 or VS Code with Azure Functions extension

### Steps

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd sys-cafm-mgmt
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Update configuration**
   - Copy `appsettings.json` to `appsettings.Development.json`
   - Update CAFM credentials and email settings
   - Ensure Framework projects are referenced correctly

4. **Build the project**
   ```bash
   dotnet build
   ```

5. **Run locally**
   ```bash
   func start
   ```

6. **Test endpoints**
   ```bash
   curl -X POST http://localhost:7071/api/cafm/login \
     -H "Content-Type: application/json"
   ```

## Dependencies

### Framework Projects (Project References)

- **Core Framework**: `/Framework/Core/Core/Core.csproj`
  - Provides base classes, middleware, exception handling, and HTTP client
- **Cache Framework**: `/Framework/Cache/Cache.csproj`
  - Provides caching capabilities (if needed for session management)

### NuGet Packages

- `Microsoft.Azure.Functions.Worker` - Azure Functions runtime
- `Microsoft.Extensions.Http.Polly` - HTTP resilience policies
- `System.ServiceModel.Http` - SOAP client support
- `System.ServiceModel.Primitives` - SOAP primitives

## SOAP Operations

### FSI CAFM SOAP Actions

1. **Authenticate**: `http://www.fsi.co.uk/services/evolution/04/09/Authenticate`
2. **LogOut**: `http://www.fsi.co.uk/services/evolution/04/09/LogOut`
3. **GetLocationsByDto**: `http://www.fsi.co.uk/services/evolution/04/09/GetLocationsByDto`
4. **GetInstructionSetsByDto**: `http://www.fsi.co.uk/services/evolution/04/09/GetInstructionSetsByDto`
5. **GetBreakdownTasksByDto**: `http://www.fsi.co.uk/services/evolution/04/09/GetBreakdownTasksByDto`
6. **CreateEvent**: `http://www.fsi.co.uk/services/evolution/04/09/CreateEvent`

### SOAP Envelope Structure

All SOAP requests follow this structure:

```xml
<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" 
                  xmlns:ns="http://www.fsi.co.uk/services/evolution/04/09">
   <soapenv:Header/>
   <soapenv:Body>
      <ns:OperationName>
         <ns:sessionId>SESSION_ID</ns:sessionId>
         <!-- Operation-specific parameters -->
      </ns:OperationName>
   </soapenv:Body>
</soapenv:Envelope>
```

## Error Handling

All errors are handled by the `ExceptionHandlerMiddleware` and return standardized responses:

```json
{
  "statusCode": 500,
  "message": "Error message",
  "data": {
    "error": "Detailed error information"
  }
}
```

## Logging

Logging is configured using Microsoft.Extensions.Logging with the following levels:
- **Debug**: Detailed diagnostic information
- **Info**: General informational messages
- **Warning**: Warnings and non-critical issues
- **Error**: Error events that might still allow the application to continue
- **Critical**: Critical failures requiring immediate attention

## Testing

### Manual Testing

Use tools like Postman or curl to test endpoints:

```bash
# Test Create Work Order
curl -X POST http://localhost:7071/api/cafm/workorders \
  -H "Content-Type: application/json" \
  -d @test-data/create-workorder-request.json
```

### Integration Testing

TODO: Add integration tests using xUnit or NUnit

## Deployment

### Azure Function App Deployment

1. **Build and publish**
   ```bash
   dotnet publish -c Release -o ./publish
   ```

2. **Deploy to Azure**
   ```bash
   func azure functionapp publish <function-app-name>
   ```

3. **Configure application settings** in Azure Portal or via Azure CLI

## Troubleshooting

### Common Issues

1. **SOAP Authentication Failure**
   - Verify CAFM credentials in appsettings
   - Check network connectivity to CAFM endpoint
   - Ensure SOAP action headers are correct

2. **Timeout Errors**
   - Increase `TimeoutSeconds` in CafmSettings
   - Check CAFM system performance

3. **Session Expired**
   - CAFM sessions have limited lifetime
   - Implement session caching if needed

## TODOs

- [ ] Update SOAP envelope structures based on actual CAFM WSDL
- [ ] Update response parsing logic based on actual CAFM responses
- [ ] Implement session caching for performance optimization
- [ ] Add comprehensive unit and integration tests
- [ ] Implement retry logic for transient failures
- [ ] Add Application Insights telemetry
- [ ] Document CAFM-specific business rules and validations

## Support

For issues or questions, contact:
- **Team**: Integration Team
- **Repository**: sys-cafm-mgmt
- **SOR**: FSI CAFM

## License

Internal use only - Al Ghurair Investment LLC
