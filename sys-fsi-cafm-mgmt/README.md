# FSI CAFM System Layer

System Layer API for FSI (Facilities System International) CAFM integration.

## Overview

This System Layer provides Azure Functions for interacting with the FSI CAFM (Computer-Aided Facilities Management) system. It handles work order creation, location lookups, instruction set management, and email notifications.

## Architecture

**Pattern:** API-Led Architecture - System Layer  
**Framework:** .NET 8, Azure Functions v4 (Isolated Worker)  
**Integration:** SOAP/XML APIs + SMTP Email  
**Authentication:** Session-based (Login/Logout with middleware)

## Azure Functions

### 1. CreateWorkOrder
**Endpoint:** `POST /api/workorder/create`  
**Purpose:** Creates work orders in FSI CAFM system  
**Authentication:** Required (CustomAuthenticationMiddleware)

**Request Body:**
```json
{
  "workOrders": [
    {
      "reporterName": "John Doe",
      "reporterEmail": "john.doe@example.com",
      "reporterPhoneNumber": "+1234567890",
      "description": "AC not working in office",
      "serviceRequestNumber": "SR-2025-001",
      "propertyName": "Building A",
      "unitCode": "UNIT-123",
      "categoryName": "HVAC",
      "subCategory": "Air Conditioning",
      "technician": "Tech-001",
      "sourceOrgId": "ORG-001",
      "ticketDetails": {
        "status": "Open",
        "subStatus": "Pending",
        "priority": "High",
        "scheduledDate": "2025-01-26",
        "scheduledTimeStart": "09:00",
        "scheduledTimeEnd": "17:00",
        "recurrence": "N",
        "oldCAFMSRNumber": "",
        "raisedDateUtc": "2025-01-25T10:30:00Z"
      }
    }
  ]
}
```

**Response:**
```json
{
  "message": "Work order created successfully in FSI CAFM.",
  "errorCode": null,
  "data": {
    "results": [
      {
        "serviceRequestNumber": "SR-2025-001",
        "taskId": "TASK-12345",
        "success": true,
        "message": "Work order created successfully"
      }
    ],
    "successCount": 1,
    "failureCount": 0
  },
  "errorDetails": null,
  "isDownStreamError": false,
  "isPartialSuccess": false
}
```

### 2. SendEmail
**Endpoint:** `POST /api/email/send`  
**Purpose:** Sends email notifications via SMTP  
**Authentication:** Not required

**Request Body:**
```json
{
  "toAddress": "recipient@example.com",
  "subject": "Work Order Notification",
  "body": "<html><body><h1>Work Order Created</h1><p>Details...</p></body></html>",
  "hasAttachment": false,
  "attachmentContent": null,
  "attachmentFileName": null
}
```

**Response:**
```json
{
  "message": "Email sent successfully.",
  "errorCode": null,
  "data": {
    "emailSent": true,
    "message": "Email sent successfully"
  },
  "errorDetails": null,
  "isDownStreamError": false,
  "isPartialSuccess": false
}
```

## Sequence Diagram

### CreateWorkOrder API Flow

```
Client
  |
  | POST /api/workorder/create
  |
  v
Azure Function: CreateWorkOrderAPI
  |
  | [CustomAuthentication] Middleware
  |
  v
CustomAuthenticationMiddleware
  |
  | 1. Authenticate
  |
  v
AuthenticateAtomicHandler
  |
  | SOAP: Authenticate(username, password)
  |
  v
FSI CAFM API
  |
  | Response: SessionId
  |
  v
RequestContext.SetSessionId(sessionId)
  |
  v
WorkOrderMgmtService
  |
  v
CreateWorkOrderHandler
  |
  | FOR EACH work order:
  |
  | 2. Check if task exists (check-before-create)
  |
  v
GetBreakdownTasksByDtoAtomicHandler
  |
  | SOAP: GetBreakdownTasksByDto(sessionId, serviceRequestNumber)
  |
  v
FSI CAFM API
  |
  | Response: CallId (empty if not exists)
  |
  v
Handler Decision: CallId empty?
  |
  ├─→ YES (not exists) → Continue to create
  |
  └─→ NO (exists) → Skip creation, return existing TaskId
  |
  | 3. Get location details (parallel)
  |
  v
GetLocationsByDtoAtomicHandler
  |
  | SOAP: GetLocationsByDto(sessionId, unitCode)
  |
  v
FSI CAFM API
  |
  | Response: BuildingId, LocationId
  |
  | 4. Get instruction sets (parallel)
  |
  v
GetInstructionSetsByDtoAtomicHandler
  |
  | SOAP: GetInstructionSetsByDto(sessionId, subCategory)
  |
  v
FSI CAFM API
  |
  | Response: CategoryId, DisciplineId, PriorityId, InstructionId
  |
  | 5. Create breakdown task (aggregates data from steps 3 & 4)
  |
  v
CreateBreakdownTaskAtomicHandler
  |
  | SOAP: CreateBreakdownTask(sessionId, dto with all IDs)
  |
  v
FSI CAFM API
  |
  | Response: TaskId
  |
  | 6. If recurring, create event and link
  |
  v
Handler Decision: recurrence == "Y"?
  |
  ├─→ YES → CreateEventAtomicHandler
  |          |
  |          | SOAP: CreateEvent(sessionId, taskId, comments)
  |          |
  |          v
  |        FSI CAFM API
  |
  └─→ NO → Skip event creation
  |
  | END FOR EACH
  |
  v
Response: List of TaskIds
  |
  v
CustomAuthenticationMiddleware (finally block)
  |
  | 7. Logout
  |
  v
LogoutAtomicHandler
  |
  | SOAP: LogOut(sessionId)
  |
  v
FSI CAFM API
  |
  v
RequestContext.Clear()
  |
  v
Client (Response)
```

### SendEmail API Flow

```
Client
  |
  | POST /api/email/send
  |
  v
Azure Function: SendEmailAPI
  |
  v
EmailMgmtService
  |
  v
SendEmailHandler
  |
  v
SendEmailAtomicHandler
  |
  | SMTP: Send email (with/without attachment)
  |
  v
SMTP Server (Office 365)
  |
  v
Client (Response)
```

## Configuration

### Required Configuration (appsettings.{env}.json)

**FSI CAFM Configuration:**
- `BaseUrl`: FSI CAFM base URL (e.g., https://fsi-cafm.example.com)
- `LoginResourcePath`: Login endpoint path
- `LogoutResourcePath`: Logout endpoint path
- `GetLocationsByDtoResourcePath`: Get locations endpoint path
- `GetInstructionSetsByDtoResourcePath`: Get instruction sets endpoint path
- `GetBreakdownTasksByDtoResourcePath`: Get breakdown tasks endpoint path
- `CreateBreakdownTaskResourcePath`: Create breakdown task endpoint path
- `CreateEventResourcePath`: Create event endpoint path
- `LoginSoapAction`: SOAP action for login
- `LogoutSoapAction`: SOAP action for logout
- `GetLocationsByDtoSoapAction`: SOAP action for get locations
- `GetInstructionSetsByDtoSoapAction`: SOAP action for get instruction sets
- `GetBreakdownTasksByDtoSoapAction`: SOAP action for get breakdown tasks
- `CreateBreakdownTaskSoapAction`: SOAP action for create breakdown task
- `CreateEventSoapAction`: SOAP action for create event

**SMTP Configuration:**
- `SmtpHost`: SMTP server host (e.g., smtp.office365.com)
- `SmtpPort`: SMTP server port (default: 587)
- `SmtpUseSsl`: Use SSL/TLS (default: true)
- `SmtpFromEmail`: From email address

**Authentication (from KeyVault):**
- `FsiUsername`: FSI CAFM username
- `FsiPassword`: FSI CAFM password (retrieved from KeyVault)
- `SmtpUsername`: SMTP username
- `SmtpPassword`: SMTP password (retrieved from KeyVault)

### Azure KeyVault Secrets

Store sensitive credentials in Azure KeyVault:

1. **FsiCafmPassword** - FSI CAFM system password
2. **SmtpPassword** - SMTP server password

Update `KeyVault.Url` in appsettings with your KeyVault URL.

### Environment Variables

Set `ENVIRONMENT` or `ASPNETCORE_ENVIRONMENT` to load environment-specific configuration:
- `dev` - Development
- `qa` - QA/Testing
- `prod` - Production

## Local Development

### Prerequisites
- .NET 8 SDK
- Azure Functions Core Tools v4
- Azure Storage Emulator or Azurite

### Setup

1. **Clone repository:**
   ```bash
   git clone <repository-url>
   cd sys-fsi-cafm-mgmt
   ```

2. **Restore dependencies:**
   ```bash
   dotnet restore
   ```

3. **Configure local settings:**
   
   Update `appsettings.dev.json` with your development environment values:
   - FSI CAFM base URL and resource paths
   - SMTP configuration
   - KeyVault URL (or use local secrets)

4. **Set environment variable:**
   ```bash
   export ENVIRONMENT=dev
   ```

5. **Run locally:**
   ```bash
   dotnet build
   func start
   ```

### Testing Endpoints

**Test CreateWorkOrder:**
```bash
curl -X POST http://localhost:7071/api/workorder/create \
  -H "Content-Type: application/json" \
  -d '{
    "workOrders": [{
      "serviceRequestNumber": "SR-TEST-001",
      "description": "Test work order",
      "unitCode": "UNIT-001",
      "subCategory": "Maintenance",
      "ticketDetails": {
        "raisedDateUtc": "2025-01-25T10:00:00Z"
      }
    }]
  }'
```

**Test SendEmail:**
```bash
curl -X POST http://localhost:7071/api/email/send \
  -H "Content-Type: application/json" \
  -d '{
    "toAddress": "test@example.com",
    "subject": "Test Email",
    "body": "<html><body>Test</body></html>",
    "hasAttachment": false
  }'
```

## Architecture Details

### Components

**Functions (Entry Points):**
- `CreateWorkOrderAPI.cs` - HTTP trigger for work order creation
- `SendEmailAPI.cs` - HTTP trigger for email sending

**Services (Abstractions):**
- `IWorkOrderMgmt` / `WorkOrderMgmtService` - Work order operations
- `IEmailMgmt` / `EmailMgmtService` - Email operations

**Handlers (Orchestration):**
- `CreateWorkOrderHandler` - Orchestrates work order creation flow
- `SendEmailHandler` - Handles email sending

**Atomic Handlers (Single Operations):**
- `AuthenticateAtomicHandler` - SOAP login
- `LogoutAtomicHandler` - SOAP logout
- `GetLocationsByDtoAtomicHandler` - Get location details
- `GetInstructionSetsByDtoAtomicHandler` - Get instruction sets
- `GetBreakdownTasksByDtoAtomicHandler` - Check task existence
- `CreateBreakdownTaskAtomicHandler` - Create breakdown task
- `CreateEventAtomicHandler` - Create event and link
- `SendEmailAtomicHandler` - Send SMTP email

**Middleware:**
- `ExecutionTimingMiddleware` - Performance tracking
- `ExceptionHandlerMiddleware` - Exception normalization
- `CustomAuthenticationMiddleware` - Session management (login/logout)

**Helpers:**
- `CustomSoapClient` - SOAP HTTP client wrapper
- `SOAPHelper` - SOAP envelope utilities
- `CustomSmtpClient` - SMTP client wrapper
- `KeyVaultReader` - Azure KeyVault integration

### Business Logic

**Check-Before-Create Pattern:**
The handler implements a check-before-create pattern to avoid duplicate work orders:
1. Call `GetBreakdownTasksByDto` to check if task exists
2. If task exists (CallId not empty), skip creation and return existing TaskId
3. If task doesn't exist (CallId empty), proceed with creation

**Parallel Lookups:**
Location and instruction set lookups can be executed in parallel as they have no dependencies on each other. Both must complete before creating the breakdown task.

**Recurring Task Handling:**
If the work order has `recurrence = "Y"`, the handler creates an event and links it to the task after task creation.

**Session Management:**
The middleware automatically handles login before the function executes and logout in the finally block, ensuring cleanup even on errors.

## Error Handling

All exceptions are caught by `ExceptionHandlerMiddleware` and normalized to `BaseResponseDTO` format with appropriate HTTP status codes:

- `400` - Validation errors, missing request body
- `404` - Resource not found (location, instruction set)
- `409/422` - Business rule violations
- `500` - Downstream API failures, server errors

## Performance Tracking

`ExecutionTimingMiddleware` tracks total execution time and `ResponseHeaders.DSTimeBreakDown` tracks individual operation timings:

```
SYSTotalTime: 5234ms
DSTimeBreakDown: AUTHENTICATE:245,GET_LOCATIONS_BY_DTO:823,GET_INSTRUCTION_SETS_BY_DTO:756,CREATE_BREAKDOWN_TASK:1823,LOGOUT:187
DSAggregatedTime: 3834ms
```

## Dependencies

**Framework Libraries:**
- `Core` - Base framework (exceptions, DTOs, extensions, middlewares)
- `Cache` - Caching framework (optional, not currently used)

**NuGet Packages:**
- `Microsoft.Azure.Functions.Worker` - Azure Functions runtime
- `Polly` - Resilience and retry policies
- `StackExchange.Redis` - Redis caching (optional)
- `Azure.Identity` / `Azure.Security.KeyVault.Secrets` - KeyVault integration
- `MailKit` - SMTP email sending

## Deployment

### CI/CD Pipeline

The project includes GitHub Actions workflow (`.github/workflows/dotnet-ci.yml`) for automated build and deployment.

### Environment-Specific Configuration

The CI/CD pipeline replaces `appsettings.json` content with environment-specific files during deployment:
- Development: `appsettings.dev.json`
- QA: `appsettings.qa.json`
- Production: `appsettings.prod.json`

### Azure Configuration

**Application Settings:**
- `ENVIRONMENT` or `ASPNETCORE_ENVIRONMENT` - Environment name
- `APPLICATIONINSIGHTS_CONNECTION_STRING` - Application Insights connection

**KeyVault Integration:**
- Configure managed identity for the Function App
- Grant Function App access to KeyVault secrets
- Update `KeyVault.Url` in appsettings

## Troubleshooting

### Common Issues

**1. "SessionId not found in context"**
- Ensure `[CustomAuthentication]` attribute is applied to the function
- Verify middleware is registered in correct order in `Program.cs`

**2. "SOAP template not found"**
- Verify SOAP envelope XML files are marked as `EmbeddedResource` in `.csproj`
- Check resource name format: `FsiCafmSystem.SoapEnvelopes.<FileName>.xml`
- Rebuild the project

**3. "Authentication failed"**
- Verify FSI CAFM credentials in KeyVault
- Check BaseUrl and LoginResourcePath configuration
- Verify LoginSoapAction matches FSI CAFM requirements

**4. "Location not found"**
- Verify UnitCode exists in FSI CAFM system
- Check GetLocationsByDto SOAP action and resource path

**5. "Email send failed"**
- Verify SMTP configuration (host, port, SSL)
- Check SMTP credentials in KeyVault
- Ensure FromEmail is authorized to send

## Support

For issues or questions, contact the integration team.

## License

Internal use only - Al Ghurair Investment LLC
