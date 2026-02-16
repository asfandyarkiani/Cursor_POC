# System Layer: Oracle Fusion HCM - Leave Management

## Overview

This System Layer API provides access to Oracle Fusion HCM Leave Management operations, specifically for creating leave absence entries.

**Domain:** Human Resource Management (HCM)  
**System of Record:** Oracle Fusion HCM Cloud  
**Authentication:** Basic Auth (credentials-per-request)

## Architecture

This project follows the **API-Led Architecture** System Layer pattern:

```
Azure Function (HTTP Entry)
    ↓
Service (ILeaveMgmt)
    ↓
Handler (CreateLeaveHandler)
    ↓
Atomic Handler (CreateLeaveAtomicHandler)
    ↓
Oracle Fusion HCM REST API
```

## Operations

### Create Leave

**Function:** `CreateLeaveAPI`  
**Route:** `POST /api/hcm/leave/create`  
**Purpose:** Create leave absence entry in Oracle Fusion HCM

**Request:**
```json
{
  "employeeNumber": 9000604,
  "absenceType": "Sick Leave",
  "employer": "Al Ghurair Investment LLC",
  "startDate": "2024-03-24",
  "endDate": "2024-03-25",
  "absenceStatusCode": "SUBMITTED",
  "approvalStatusCode": "APPROVED",
  "startDateDuration": 1,
  "endDateDuration": 1
}
```

**Response (Success):**
```json
{
  "message": "Leave created successfully in Oracle Fusion HCM.",
  "errorCode": null,
  "data": {
    "personAbsenceEntryId": 12345
  }
}
```

**Response (Error):**
```json
{
  "message": "Failed to create leave in Oracle Fusion HCM",
  "errorCode": "OFH_LEVCRT_0001",
  "errorDetails": ["Oracle Fusion HCM returned status 400. Response: ..."]
}
```

## Sequence Diagram

```
┌─────────────┐      ┌──────────────┐      ┌──────────────┐      ┌─────────────────┐      ┌──────────────────┐
│ Process     │      │ CreateLeave  │      │ LeaveMgmt    │      │ CreateLeave     │      │ CreateLeave      │
│ Layer       │      │ API          │      │ Service      │      │ Handler         │      │ AtomicHandler    │
│ (Caller)    │      │ (Function)   │      │              │      │                 │      │                  │
└──────┬──────┘      └──────┬───────┘      └──────┬───────┘      └────────┬────────┘      └────────┬─────────┘
       │                    │                     │                       │                        │
       │ POST /api/hcm/     │                     │                       │                        │
       │ leave/create       │                     │                       │                        │
       ├───────────────────>│                     │                       │                        │
       │                    │                     │                       │                        │
       │                    │ ReadBodyAsync<>     │                       │                        │
       │                    │ Validate()          │                       │                        │
       │                    │                     │                       │                        │
       │                    │ CreateLeave()       │                       │                        │
       │                    ├────────────────────>│                       │                        │
       │                    │                     │                       │                        │
       │                    │                     │ HandleAsync()         │                        │
       │                    │                     ├──────────────────────>│                        │
       │                    │                     │                       │                        │
       │                    │                     │                       │ Handle()               │
       │                    │                     │                       ├───────────────────────>│
       │                    │                     │                       │                        │
       │                    │                     │                       │   Map DTO              │
       │                    │                     │                       │   (employeeNumber→     │
       │                    │                     │                       │    personNumber)       │
       │                    │                     │                       │                        │
       │                    │                     │                       │   POST /absences       │
       │                    │                     │                       │   (Basic Auth)         │
       │                    │                     │                       │   ┌──────────────────┐ │
       │                    │                     │                       │   │ Oracle Fusion    │ │
       │                    │                     │                       │───┤ HCM REST API     │ │
       │                    │                     │                       │<──│                  │ │
       │                    │                     │                       │   └──────────────────┘ │
       │                    │                     │                       │                        │
       │                    │                     │                       │ HttpResponseSnapshot   │
       │                    │                     │                       │<───────────────────────┤
       │                    │                     │                       │                        │
       │                    │                     │  Check IsSuccessStatusCode                     │
       │                    │                     │  Deserialize ApiResDTO                         │
       │                    │                     │  Map to ResDTO                                 │
       │                    │                     │                       │                        │
       │                    │                     │ BaseResponseDTO       │                        │
       │                    │                     │<──────────────────────┤                        │
       │                    │                     │                       │                        │
       │                    │ BaseResponseDTO     │                       │                        │
       │                    │<────────────────────┤                       │                        │
       │                    │                     │                       │                        │
       │ BaseResponseDTO    │                     │                       │                        │
       │<───────────────────┤                     │                       │                        │
       │                    │                     │                       │                        │
```

## Field Mappings

The System Layer transforms field names from D365 format to Oracle Fusion format:

| D365 Field | Oracle Fusion Field | Transformation |
|---|---|---|
| employeeNumber | personNumber | int → string |
| absenceStatusCode | absenceStatusCd | Field name change |
| approvalStatusCode | approvalStatusCd | Field name change |
| absenceType | absenceType | Direct mapping |
| employer | employer | Direct mapping |
| startDate | startDate | Direct mapping |
| endDate | endDate | Direct mapping |
| startDateDuration | startDateDuration | Direct mapping |
| endDateDuration | endDateDuration | Direct mapping |

## Configuration

### AppConfigs

| Property | Description | Example |
|---|---|---|
| OracleFusionBaseUrl | Base URL for Oracle Fusion HCM API | https://iaaxey-dev3.fa.ocs.oraclecloud.com:443 |
| OracleFusionResourcePath | Resource path for absences endpoint | hcmRestApi/resources/11.13.18.05/absences |
| OracleFusionUsername | Basic Auth username | INTEGRATION.USER@al-ghurair.com |
| OracleFusionPassword | Basic Auth password (from KeyVault) | [SECRET] |
| TimeoutSeconds | HTTP client timeout | 50 |
| RetryCount | Retry count for 5xx errors | 0 |

### Environment Files

- `appsettings.json` - Placeholder (CI/CD replaces)
- `appsettings.dev.json` - Development environment
- `appsettings.qa.json` - QA environment
- `appsettings.prod.json` - Production environment

## Error Codes

| Error Code | Message | HTTP Status |
|---|---|---|
| OFH_LEVCRT_0001 | Failed to create leave in Oracle Fusion HCM | 400/500 |
| OFH_LEVCRT_0002 | Oracle Fusion HCM returned empty response | 500 |
| OFH_LEVCRT_0003 | Oracle Fusion HCM authentication failed | 401 |
| OFH_LEVCRT_0004 | Invalid leave data provided | 400 |

## Project Structure

```
sys-oraclefusion-hcm/
├── Abstractions/
│   └── ILeaveMgmt.cs
├── ConfigModels/
│   └── AppConfigs.cs
├── Constants/
│   ├── ErrorConstants.cs
│   ├── InfoConstants.cs
│   └── OperationNames.cs
├── DTO/
│   ├── CreateLeaveDTO/
│   │   ├── CreateLeaveReqDTO.cs
│   │   └── CreateLeaveResDTO.cs
│   ├── AtomicHandlerDTOs/
│   │   └── CreateLeaveHandlerReqDTO.cs
│   └── DownstreamDTOs/
│       └── CreateLeaveApiResDTO.cs
├── Functions/
│   └── CreateLeaveAPI.cs
├── Helper/
│   └── RestApiHelper.cs
├── Implementations/
│   └── OracleFusionHCM/
│       ├── Services/
│       │   └── LeaveMgmtService.cs
│       ├── Handlers/
│       │   └── CreateLeaveHandler.cs
│       └── AtomicHandlers/
│           └── CreateLeaveAtomicHandler.cs
├── Program.cs
├── host.json
├── appsettings.json
├── appsettings.dev.json
├── appsettings.qa.json
└── appsettings.prod.json
```

## Dependencies

- .NET 8.0
- Azure Functions v4 (Isolated Worker Model)
- Framework/Core (project reference)
- Framework/Cache (project reference)
- Polly (resilience policies)
- Application Insights (telemetry)

## Notes

- **Authentication:** Basic Auth credentials-per-request (no middleware required)
- **No Custom Middleware:** Uses only ExecutionTiming and ExceptionHandler from Framework
- **Single Operation:** Only Create Leave operation (as per Boomi process analysis)
- **No SOAP:** REST/JSON only (no SOAP envelopes or CustomSoapClient)
- **Error Handling:** All exceptions normalized by ExceptionHandlerMiddleware
- **Additive Changes:** Non-breaking, follows System Layer architectural patterns
