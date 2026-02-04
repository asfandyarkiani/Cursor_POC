# Buddy App Notification Service - System Layer

## Overview

This System Layer API provides push notification management for the Buddy App (Automotive domain). It exposes operations to send push notifications to drivers via a microservice API.

## Architecture

This project follows the **API-Led Architecture** principles:

- **System Layer**: Unlocks data from Buddy App Notification Microservice
- **Insulates consumers** from the complexity of the underlying microservice
- **Reusable "Lego block"** for Process Layer orchestration

## System of Record (SOR)

- **SOR Name**: Buddy App Notification Service (Microservice API)
- **Base URL**: Configurable per environment
  - DEV: `http://shared-ms.agp-dev.com`
  - QA: `http://shared-ms.agp-qa.com`
  - PROD: `http://shared-ms.agp-prod.com`
- **Resource Path**: `ms/comm/transaction`
- **Authentication**: None

## Operations

### SendPushNotificationAPI

**Route**: `POST /api/notifications`

**Description**: Sends push notifications to Buddy App drivers

**Request Headers**:
- `organization-unit` (optional): Organization unit identifier
- `bussiness-unit` (optional): Business unit identifier
- `channel` (optional): Channel identifier
- `accept-language` (optional): Language preference
- `source` (optional): Source system identifier

**Request Body**:
```json
{
  "modes": [
    {
      "type": "push",
      "provider": "fcm"
    }
  ],
  "data": {
    "driverId": "12345",
    "title": "Notification Title",
    "message": "Notification message body",
    "data": {
      "foo": "custom value 1",
      "bar": "custom value 2"
    }
  }
}
```

**Response**:
```json
{
  "status": "success",
  "message": "Notification sent successfully"
}
```

**Error Response**:
```json
{
  "status": "failure",
  "message": "Error message describing the failure"
}
```

## Project Structure

```
sys-buddyapp-notificationservice-mgmt/
├── Abstractions/                          # Interfaces (INotificationMgmt)
├── Implementations/BuddyAppMicroservice/  # Vendor-specific implementation
│   ├── Services/                          # NotificationMgmtService
│   ├── Handlers/                          # SendPushNotificationHandler
│   └── AtomicHandlers/                    # SendPushNotificationAtomicHandler
├── DTO/
│   ├── SendPushNotificationDTO/           # API-level DTOs
│   ├── AtomicHandlerDTOs/                 # Handler request DTOs
│   └── DownstreamDTOs/                    # Microservice response DTOs
├── Functions/                             # Azure Functions (SendPushNotificationAPI)
├── ConfigModels/                          # AppConfigs
├── Constants/                             # ErrorConstants, InfoConstants
├── Helpers/                               # RestApiHelper
├── Program.cs                             # DI registration and middleware configuration
├── host.json                              # Azure Functions configuration
└── appsettings.{env}.json                 # Environment-specific configuration
```

## Configuration

Configuration is managed through `appsettings.{env}.json` files:

```json
{
  "AppConfigs": {
    "MicroserviceBaseUrl": "http://shared-ms.agp-dev.com",
    "ResourcePath": "ms/comm/transaction",
    "TimeoutSeconds": 30,
    "RetryCount": 3,
    "RetryDelaySeconds": 2
  }
}
```

## Middleware

The following middleware is registered in order:

1. **ExecutionTimingMiddleware**: Tracks execution time
2. **ExceptionHandlerMiddleware**: Handles exceptions and returns standardized error responses

## Error Codes

Error codes follow the format: `SYS_NTFSVC_DDDD`

- **SYS**: System Layer
- **NTFSVC**: NotificationService (6 characters)
- **DDDD**: 4-digit error code

### Error Code Ranges

- **1000-1999**: Validation errors
- **2000-2999**: Downstream API errors
- **3000-3999**: Configuration errors
- **9000-9999**: General errors

## Dependencies

- **.NET 8.0**: Target framework
- **Azure Functions v4**: Hosting model
- **Framework/Core**: Shared framework for middleware, exceptions, extensions
- **Polly**: Retry and circuit breaker policies

## Development

### Build

```bash
dotnet build
```

### Run Locally

```bash
func start
```

### Test

```bash
curl -X POST http://localhost:7071/api/notifications \
  -H "Content-Type: application/json" \
  -H "organization-unit: ORG001" \
  -H "bussiness-unit: BU001" \
  -H "channel: mobile" \
  -H "accept-language: en-US" \
  -H "source: D365" \
  -d '{
    "modes": [{"type": "push", "provider": "fcm"}],
    "data": {
      "driverId": "12345",
      "title": "Test Notification",
      "message": "This is a test notification"
    }
  }'
```

## Deployment

This project is deployed as an Azure Function App. The CI/CD pipeline automatically deploys to the appropriate environment based on the branch.

## Boomi Migration

This System Layer API was migrated from the following Boomi process:

- **Process Name**: Push Notifications
- **Process ID**: 50751707-15fe-4c27-a814-426385d7e8aa
- **Business Domain**: Automotive / Buddy APP 2.0 / Notifications
- **Extraction Document**: See `BOOMI_EXTRACTION_PHASE1.md` for complete analysis

## Sequence Diagram

```
Caller (Process Layer) -> SendPushNotificationAPI
  -> NotificationMgmtService
    -> SendPushNotificationHandler
      -> SendPushNotificationAtomicHandler
        -> Buddy App Microservice (HTTP POST)
      <- HTTP Response
    <- Process Response (20x/40x/5xx)
  <- SendPushNotificationResDTO
<- HTTP 200 OK (JSON)
```

## Response Processing Logic

The handler implements the following decision logic (from Boomi analysis):

1. **Status Code 20x (Success)**:
   - Check if `failed` array has error messages
   - If error message present → Return failure
   - If error message empty → Return success

2. **Status Code 40x (Client Error)**:
   - Extract error from `message` array
   - Throw `DownStreamApiFailureException`

3. **Status Code 5xx or Other**:
   - Extract generic error message
   - Throw `DownStreamApiFailureException`

## Contact

For questions or issues, please contact the Integration Team.
