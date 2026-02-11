# BOOMI EXTRACTION PHASE 1 - HCM Leave Create Process

**Process Name:** HCM_Leave Create  
**Process ID:** ca69f858-785f-4565-ba1f-b4edc6cca05b  
**Description:** This Process will sync the Leave data between D365 and Oracle HCM  
**Extraction Date:** 2026-02-11

---

## Table of Contents

1. [Operations Inventory](#1-operations-inventory)
2. [Input Structure Analysis (Step 1a)](#2-input-structure-analysis-step-1a)
3. [Response Structure Analysis (Step 1b)](#3-response-structure-analysis-step-1b)
4. [Operation Response Analysis (Step 1c)](#4-operation-response-analysis-step-1c)
5. [Map Analysis (Step 1d)](#5-map-analysis-step-1d)
6. [HTTP Status Codes and Return Path Responses (Step 1e)](#6-http-status-codes-and-return-path-responses-step-1e)
7. [Process Properties Analysis (Steps 2-3)](#7-process-properties-analysis-steps-2-3)
8. [Data Dependency Graph (Step 4)](#8-data-dependency-graph-step-4)
9. [Control Flow Graph (Step 5)](#9-control-flow-graph-step-5)
10. [Decision Shape Analysis (Step 7)](#10-decision-shape-analysis-step-7)
11. [Branch Shape Analysis (Step 8)](#11-branch-shape-analysis-step-8)
12. [Execution Order (Step 9)](#12-execution-order-step-9)
13. [Sequence Diagram (Step 10)](#13-sequence-diagram-step-10)
14. [Subprocess Analysis (Step 7a)](#14-subprocess-analysis-step-7a)
15. [System Layer Identification](#15-system-layer-identification)
16. [Request/Response JSON Examples](#16-requestresponse-json-examples)
17. [Function Exposure Decision Table](#17-function-exposure-decision-table)
18. [Validation Checklist](#18-validation-checklist)

---

## 1. Operations Inventory

### Main Process Operations

| Operation ID | Operation Name | Type | Sub-Type | Purpose |
|---|---|---|---|---|
| 8f709c2b-e63f-4d5f-9374-2932ed70415d | Create Leave Oracle Fusion OP | connector-action | wss | Web Service Server - Entry point for leave creation |
| 6e8920fd-af5a-430b-a1d9-9fde7ac29a12 | Leave Oracle Fusion Create | connector-action | http | HTTP POST to Oracle Fusion HCM API |
| af07502a-fafd-4976-a691-45d51a33b549 | Email w Attachment | connector-action | mail | Send email with attachment |
| 15a72a21-9b57-49a1-a8ed-d70367146644 | Email W/O Attachment | connector-action | mail | Send email without attachment |

### Connections

| Connection ID | Connection Name | Type | Base URL/Host |
|---|---|---|---|
| aa1fcb29-d146-4425-9ea6-b9698090f60e | Oracle Fusion | http | https://iaaxey-dev3.fa.ocs.oraclecloud.com:443 |
| 00eae79b-2303-4215-8067-dcc299e42697 | Office 365 Email | mail | smtp-mail.outlook.com:587 |

### Maps

| Map ID | Map Name | From Profile | To Profile | Purpose |
|---|---|---|---|---|
| c426b4d6-2aff-450e-b43b-59956c4dbc96 | Leave Create Map | D365 Leave Create JSON Profile | HCM Leave Create JSON Profile | Transform D365 request to Oracle Fusion format |
| e4fd3f59-edb5-43a1-aeae-143b600a064e | Oracle Fusion Leave Response Map | Oracle Fusion Leave Response JSON Profile | Leave D365 Response | Map Oracle Fusion response to D365 response |
| f46b845a-7d75-41b5-b0ad-c41a6a8e9b12 | Leave Error Map | Dummy FF Profile | Leave D365 Response | Map error messages to D365 error response |

### Profiles

| Profile ID | Profile Name | Type | Purpose |
|---|---|---|---|
| febfa3e1-f719-4ee8-ba57-cdae34137ab3 | D365 Leave Create JSON Profile | profile.json | Request profile for entry point |
| a94fa205-c740-40a5-9fda-3d018611135a | HCM Leave Create JSON Profile | profile.json | Request profile for Oracle Fusion API |
| 316175c7-0e45-4869-9ac6-5f9d69882a62 | Oracle Fusion Leave Response JSON Profile | profile.json | Response profile from Oracle Fusion API |
| f4ca3a70-114a-4601-bad8-44a3eb20e2c0 | Leave D365 Response | profile.json | Response profile for D365 |
| 23d7a2e9-5cb0-4e9c-9e4b-3154834bad0d | Dummy FF Profile | profile.flatfile | Dummy profile for error mapping |

### Subprocess

| Subprocess ID | Subprocess Name | Purpose |
|---|---|---|
| a85945c5-3004-42b9-80b1-104f465cd1fb | (Sub) Office 365 Email | Reusable email subprocess for sending notifications |

---

## 2. Input Structure Analysis (Step 1a)

### Request Profile Structure

**Profile ID:** febfa3e1-f719-4ee8-ba57-cdae34137ab3  
**Profile Name:** D365 Leave Create JSON Profile  
**Profile Type:** profile.json  
**Root Structure:** Root/Object  
**Array Detection:** ❌ NO - Single object structure  
**Input Type:** singlejson

### Input Format

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

### Document Processing Behavior

- **Boomi Processing:** Single document processing (inputType: singlejson)
- **Azure Function Requirement:** Must accept single leave request object
- **Implementation Pattern:** Process single leave request, return response

### Request Field Mapping

| Boomi Field Path | Boomi Field Name | Data Type | Required | Azure DTO Property | Notes |
|---|---|---|---|---|---|
| Root/Object/employeeNumber | employeeNumber | number | Yes | EmployeeNumber | Employee identifier |
| Root/Object/absenceType | absenceType | character | Yes | AbsenceType | Type of leave (Sick Leave, Annual Leave, etc.) |
| Root/Object/employer | employer | character | Yes | Employer | Employer name |
| Root/Object/startDate | startDate | character | Yes | StartDate | Leave start date (YYYY-MM-DD) |
| Root/Object/endDate | endDate | character | Yes | EndDate | Leave end date (YYYY-MM-DD) |
| Root/Object/absenceStatusCode | absenceStatusCode | character | Yes | AbsenceStatusCode | Status code (SUBMITTED, APPROVED, etc.) |
| Root/Object/approvalStatusCode | approvalStatusCode | character | Yes | ApprovalStatusCode | Approval status code |
| Root/Object/startDateDuration | startDateDuration | number | Yes | StartDateDuration | Duration for start date |
| Root/Object/endDateDuration | endDateDuration | number | Yes | EndDateDuration | Duration for end date |

**Total Fields:** 9 fields

---

## 3. Response Structure Analysis (Step 1b)

### Response Profile Structure

**Profile ID:** f4ca3a70-114a-4601-bad8-44a3eb20e2c0  
**Profile Name:** Leave D365 Response  
**Profile Type:** profile.json  
**Root Structure:** leaveResponse/Object

### Response Format

```json
{
  "status": "success",
  "message": "Data successfully sent to Oracle Fusion",
  "personAbsenceEntryId": 12345,
  "success": "true"
}
```

### Response Field Mapping

| Boomi Field Path | Boomi Field Name | Data Type | Azure DTO Property | Notes |
|---|---|---|---|---|
| leaveResponse/Object/status | status | character | Status | Success or failure status |
| leaveResponse/Object/message | message | character | Message | Response message |
| leaveResponse/Object/personAbsenceEntryId | personAbsenceEntryId | number | PersonAbsenceEntryId | Oracle Fusion absence entry ID |
| leaveResponse/Object/success | success | character | Success | Boolean string (true/false) |

**Total Fields:** 4 fields

---

## 4. Operation Response Analysis (Step 1c)

### Operation: Leave Oracle Fusion Create (6e8920fd-af5a-430b-a1d9-9fde7ac29a12)

**Operation Type:** HTTP POST  
**Response Profile ID:** None (responseProfileType: NONE)  
**Response Handling:** HTTP status code and response body captured via track properties

**Extracted Fields:**
- **meta.base.applicationstatuscode** - HTTP status code from Oracle Fusion API
  - Extracted by: Decision shape2 (HTTP Status 20 check)
  - Written to: Track property (used directly in decision)
  - Consumers: shape2 (Decision), shape44 (Decision)

- **meta.base.applicationstatusmessage** - HTTP response message
  - Extracted by: shape39 (error msg), shape46 (error msg)
  - Written to: process.DPP_ErrorMessage
  - Consumers: shape40 (error map), shape47 (error map)

- **dynamicdocument.DDP_RespHeader** - Content-Encoding header
  - Extracted by: HTTP operation response header mapping
  - Written to: Document property
  - Consumers: shape44 (Check Response Content Type decision)

**Business Logic Implications:**
- Oracle Fusion API call MUST execute BEFORE any decision checking HTTP status
- HTTP status code determines success vs error path
- Response content encoding (gzip) determines if decompression is needed

### Response Data Flow

```
Operation: Leave Oracle Fusion Create
  ↓ (produces)
HTTP Status Code (meta.base.applicationstatuscode)
  ↓ (consumed by)
Decision: HTTP Status 20 check (shape2)
  ↓ (routes to)
Success Path OR Error Path
```

---

## 5. Map Analysis (Step 1d)

### Map Inventory

| Map ID | Map Name | From Profile | To Profile | Type |
|---|---|---|---|
| c426b4d6-2aff-450e-b43b-59956c4dbc96 | Leave Create Map | febfa3e1-f719-4ee8-ba57-cdae34137ab3 | a94fa205-c740-40a5-9fda-3d018611135a | Request transformation |
| e4fd3f59-edb5-43a1-aeae-143b600a064e | Oracle Fusion Leave Response Map | 316175c7-0e45-4869-9ac6-5f9d69882a62 | f4ca3a70-114a-4601-bad8-44a3eb20e2c0 | Success response |
| f46b845a-7d75-41b5-b0ad-c41a6a8e9b12 | Leave Error Map | 23d7a2e9-5cb0-4e9c-9e4b-3154834bad0d | f4ca3a70-114a-4601-bad8-44a3eb20e2c0 | Error response |

### Map 1: Leave Create Map (c426b4d6-2aff-450e-b43b-59956c4dbc96)

**Purpose:** Transform D365 leave request to Oracle Fusion format

**Field Mappings:**

| Source Field | Source Type | Target Field | Transformation |
|---|---|---|---|
| employeeNumber | profile | personNumber | Direct mapping |
| absenceType | profile | absenceType | Direct mapping |
| employer | profile | employer | Direct mapping |
| startDate | profile | startDate | Direct mapping |
| endDate | profile | endDate | Direct mapping |
| absenceStatusCode | profile | absenceStatusCd | Field name change |
| approvalStatusCode | profile | approvalStatusCd | Field name change |
| startDateDuration | profile | startDateDuration | Direct mapping |
| endDateDuration | profile | endDateDuration | Direct mapping |

**Profile vs Map Comparison:**

| Profile Field Name (From) | Map Field Name (To) | Discrepancy? |
|---|---|---|
| absenceStatusCode | absenceStatusCd | ✅ Minor name change |
| approvalStatusCode | approvalStatusCd | ✅ Minor name change |

**CRITICAL RULE:** Map field names are AUTHORITATIVE. Use map field names in API requests.

### Map 2: Oracle Fusion Leave Response Map (e4fd3f59-edb5-43a1-aeae-143b600a064e)

**Purpose:** Map Oracle Fusion success response to D365 response format

**Field Mappings:**

| Source Field | Source Type | Target Field | Notes |
|---|---|---|---|
| personAbsenceEntryId | profile | personAbsenceEntryId | Direct mapping from Oracle response |
| (static) | default | status | Default value: "success" |
| (static) | default | message | Default value: "Data successfully sent to Oracle Fusion" |
| (static) | default | success | Default value: "true" |

### Map 3: Leave Error Map (f46b845a-7d75-41b5-b0ad-c41a6a8e9b12)

**Purpose:** Map error messages to D365 error response format

**Field Mappings:**

| Source Field | Source Type | Target Field | Notes |
|---|---|---|---|
| process.DPP_ErrorMessage | function (PropertyGet) | message | Error message from process property |
| (static) | default | status | Default value: "failure" |
| (static) | default | success | Default value: "false" |

**Scripting Functions:**

| Function | Type | Input | Output | Logic |
|---|---|---|---|---|
| Function 1 | PropertyGet | Property Name: "DPP_ErrorMessage" | Result | Retrieves error message from process property |

---

## 6. HTTP Status Codes and Return Path Responses (Step 1e)

### Return Path Inventory

| Return Label | Return Shape ID | HTTP Status Code | Decision Conditions | Type |
|---|---|---|---|---|
| Success Response | shape35 | 200 | HTTP Status 20* = TRUE | Success |
| Error Response | shape36 | 400 | HTTP Status 20* = FALSE AND Content-Encoding != gzip | Error |
| Error Response | shape43 | 500 | Try/Catch error path | Error |
| Error Response | shape48 | 400 | HTTP Status 20* = FALSE AND Content-Encoding = gzip | Error |

### Return Path 1: Success Response (shape35)

**Return Label:** Success Response  
**Return Shape ID:** shape35  
**HTTP Status Code:** 200  
**Decision Conditions Leading to Return:**
- shape2 (HTTP Status 20 check): meta.base.applicationstatuscode matches "20*" → TRUE path

**Populated Response Fields:**

| Field Name | Field Path | Source | Populated By |
|---|---|---|---|
| status | leaveResponse/Object/status | map default | Oracle Fusion Leave Response Map |
| message | leaveResponse/Object/message | map default | Oracle Fusion Leave Response Map |
| personAbsenceEntryId | leaveResponse/Object/personAbsenceEntryId | operation_response | Leave Oracle Fusion Create |
| success | leaveResponse/Object/success | map default | Oracle Fusion Leave Response Map |

**Response JSON Example:**

```json
{
  "status": "success",
  "message": "Data successfully sent to Oracle Fusion",
  "personAbsenceEntryId": 300100123456789,
  "success": "true"
}
```

### Return Path 2: Error Response - Non-Gzip (shape36)

**Return Label:** Error Response  
**Return Shape ID:** shape36  
**HTTP Status Code:** 400  
**Decision Conditions Leading to Return:**
- shape2 (HTTP Status 20 check): meta.base.applicationstatuscode NOT matches "20*" → FALSE path
- shape44 (Check Response Content Type): dynamicdocument.DDP_RespHeader != "gzip" → FALSE path

**Error Code:** HTTP_ERROR  
**Populated Response Fields:**

| Field Name | Field Path | Source | Populated By |
|---|---|---|---|
| status | leaveResponse/Object/status | map default | Leave Error Map |
| message | leaveResponse/Object/message | process_property | process.DPP_ErrorMessage |
| success | leaveResponse/Object/success | map default | Leave Error Map |

**Response JSON Example:**

```json
{
  "status": "failure",
  "message": "Oracle Fusion API returned error: Invalid absence type",
  "success": "false"
}
```

### Return Path 3: Error Response - Try/Catch (shape43)

**Return Label:** Error Response  
**Return Shape ID:** shape43  
**HTTP Status Code:** 500  
**Decision Conditions Leading to Return:**
- shape17 (Try/Catch): Exception caught in Try block → Catch path
- shape20 (Branch): Path 2 selected

**Error Code:** EXCEPTION_ERROR  
**Populated Response Fields:**

| Field Name | Field Path | Source | Populated By |
|---|---|---|---|
| status | leaveResponse/Object/status | map default | Leave Error Map |
| message | leaveResponse/Object/message | process_property | process.DPP_ErrorMessage (from meta.base.catcherrorsmessage) |
| success | leaveResponse/Object/success | map default | Leave Error Map |

**Response JSON Example:**

```json
{
  "status": "failure",
  "message": "Connection timeout to Oracle Fusion API",
  "success": "false"
}
```

### Return Path 4: Error Response - Gzip (shape48)

**Return Label:** Error Response  
**Return Shape ID:** shape48  
**HTTP Status Code:** 400  
**Decision Conditions Leading to Return:**
- shape2 (HTTP Status 20 check): meta.base.applicationstatuscode NOT matches "20*" → FALSE path
- shape44 (Check Response Content Type): dynamicdocument.DDP_RespHeader = "gzip" → TRUE path
- shape45 (Groovy script): Decompress gzip response

**Error Code:** HTTP_ERROR_GZIP  
**Populated Response Fields:**

| Field Name | Field Path | Source | Populated By |
|---|---|---|---|
| status | leaveResponse/Object/status | map default | Leave Error Map |
| message | leaveResponse/Object/message | process_property | process.DPP_ErrorMessage (decompressed) |
| success | leaveResponse/Object/success | map default | Leave Error Map |

**Response JSON Example:**

```json
{
  "status": "failure",
  "message": "Validation error: Employee not found in Oracle Fusion",
  "success": "false"
}
```

### Downstream Operations HTTP Status Codes

| Operation Name | Expected Success Codes | Error Codes | Error Handling |
|---|---|---|---|
| Leave Oracle Fusion Create | 200, 201 | 400, 401, 404, 500 | Check HTTP status code, route to error path if non-20* |

---

## 7. Process Properties Analysis (Steps 2-3)

### Process Properties WRITTEN

| Property Name | Property Type | Written By Shape(s) | Source | Purpose |
|---|---|---|---|---|
| process.DPP_Process_Name | Dynamic | shape38 | Execution property: Process Name | Store process name for email notification |
| process.DPP_AtomName | Dynamic | shape38 | Execution property: Atom Name | Store atom name for email notification |
| process.DPP_Payload | Dynamic | shape38 | Current document | Store input payload for email attachment |
| process.DPP_ExecutionID | Dynamic | shape38 | Execution property: Execution Id | Store execution ID for email notification |
| process.DPP_File_Name | Dynamic | shape38 | Concatenation: Process Name + Timestamp + ".txt" | Generate filename for email attachment |
| process.DPP_Subject | Dynamic | shape38 | Concatenation: Atom Name + " (" + Process Name + " ) has errors to report" | Generate email subject |
| process.To_Email | Dynamic | shape38 | Defined property: PP_HCM_LeaveCreate_Properties.To_Email | Email recipient address |
| process.DPP_HasAttachment | Dynamic | shape38 | Defined property: PP_HCM_LeaveCreate_Properties.DPP_HasAttachment | Flag for email attachment (Y/N) |
| process.DPP_ErrorMessage | Dynamic | shape19, shape39, shape46 | Track property: meta.base.catcherrorsmessage OR meta.base.applicationstatusmessage | Store error message for email and response |
| dynamicdocument.URL | Dynamic Document | shape8 | Defined property: PP_HCM_LeaveCreate_Properties.Resource_Path | Oracle Fusion API resource path |

### Process Properties READ

| Property Name | Read By Shape(s) | Usage |
|---|---|---|
| process.DPP_Process_Name | Subprocess shape21 (shape11, shape23) | Email body parameter |
| process.DPP_AtomName | Subprocess shape21 (shape11, shape23) | Email body parameter |
| process.DPP_Payload | Subprocess shape21 (shape15) | Email attachment content |
| process.DPP_ExecutionID | Subprocess shape21 (shape11, shape23) | Email body parameter |
| process.DPP_File_Name | Subprocess shape21 (shape6) | Email attachment filename |
| process.DPP_Subject | Subprocess shape21 (shape6, shape20) | Email subject |
| process.To_Email | Subprocess shape21 (shape6, shape20) | Email recipient |
| process.DPP_HasAttachment | Subprocess shape21 (shape4) | Decision: Send email with or without attachment |
| process.DPP_MailBody | Subprocess shape21 (shape6, shape20) | Email body content |
| process.DPP_ErrorMessage | shape40, shape41, shape47 | Error message mapping to response |
| dynamicdocument.URL | shape33 (HTTP operation) | Oracle Fusion API endpoint path |

### Defined Process Properties

**Component:** PP_HCM_LeaveCreate_Properties (e22c04db-7dfa-4b1b-9d88-77365b0fdb18)

| Property Key | Property Label | Type | Default Value | Purpose |
|---|---|---|---|---|
| c2d1a1e8-4bcb-435b-b1d2-bb10a209bcc0 | Resource_Path | string | hcmRestApi/resources/11.13.18.05/absences | Oracle Fusion API resource path |
| 71c90f6e-4f86-49f3-8aef-bae6668c73f9 | To_Email | string | BoomiIntegrationTeam@al-ghurair.com | Error notification email recipient |
| a717867b-67f4-4a72-be2d-c99c73309fdb | DPP_HasAttachment | string | Y | Flag to include attachment in error email |

**Component:** PP_Office365_Email (0feff13f-8a2c-438a-b3c7-1909e2a7f533)

| Property Key | Property Label | Type | Default Value | Purpose |
|---|---|---|---|---|
| 804b6d40-eeee-4ac0-ab4b-94e1aca9f4b7 | From_Email | string | Boomi.Dev.failures@al-ghurair.com | Email sender address |
| d8fc7496-ec2c-4308-a4ca-e9f49c0c9500 | Environment | string | DEV Failure : | Environment prefix for email subject |

---

## 8. Data Dependency Graph (Step 4)

### Dependency Graph

```
shape38 (Input_details)
  ↓ WRITES: process.DPP_Process_Name, process.DPP_AtomName, process.DPP_Payload, 
            process.DPP_ExecutionID, process.DPP_File_Name, process.DPP_Subject, 
            process.To_Email, process.DPP_HasAttachment
  ↓ (consumed by)
shape21 (Subprocess: Office 365 Email) - READS all above properties

shape8 (set URL)
  ↓ WRITES: dynamicdocument.URL
  ↓ (consumed by)
shape33 (Leave Oracle Fusion Create) - READS dynamicdocument.URL

shape19 (ErrorMsg - Catch path)
  ↓ WRITES: process.DPP_ErrorMessage (from meta.base.catcherrorsmessage)
  ↓ (consumed by)
shape21 (Subprocess) - READS process.DPP_ErrorMessage

shape39 (error msg - HTTP error path)
  ↓ WRITES: process.DPP_ErrorMessage (from meta.base.applicationstatusmessage)
  ↓ (consumed by)
shape40 (error map) - READS process.DPP_ErrorMessage

shape46 (error msg - Gzip error path)
  ↓ WRITES: process.DPP_ErrorMessage (from meta.base.applicationstatusmessage)
  ↓ (consumed by)
shape47 (error map) - READS process.DPP_ErrorMessage

shape33 (Leave Oracle Fusion Create)
  ↓ PRODUCES: meta.base.applicationstatuscode, meta.base.applicationstatusmessage, 
              dynamicdocument.DDP_RespHeader
  ↓ (consumed by)
shape2 (HTTP Status 20 check) - READS meta.base.applicationstatuscode
shape44 (Check Response Content Type) - READS dynamicdocument.DDP_RespHeader
```

### Dependency Chains

**Chain 1: URL Setup → API Call**
```
shape8 (set URL) → shape33 (HTTP POST to Oracle Fusion)
```

**Chain 2: Input Properties → Email Notification**
```
shape38 (Input_details) → shape21 (Subprocess: Email)
```

**Chain 3: API Call → Status Check → Response Routing**
```
shape33 (HTTP POST) → shape2 (HTTP Status 20 check) → Success/Error Path
```

**Chain 4: Error Capture → Error Response**
```
shape19/shape39/shape46 (Error capture) → shape40/shape41/shape47 (Error mapping) → Error Response
```

### Property Summary

**Properties Creating Dependencies:**
- `dynamicdocument.URL` - Required by HTTP operation
- `process.DPP_ErrorMessage` - Required by error mapping and email notification
- `process.DPP_*` (email properties) - Required by email subprocess
- `meta.base.applicationstatuscode` - Required by decision shapes for routing
- `dynamicdocument.DDP_RespHeader` - Required by content type decision

---

## 9. Control Flow Graph (Step 5)

### Control Flow Map

**🔍 SELF-CHECK:** Did I read dragpoints from JSON file? **YES** (Lines 58-67, 110-133, 175-184, etc. in process_root JSON)

| From Shape | Shape Type | To Shape | Identifier | Text | Notes |
|---|---|---|---|---|---|
| shape1 | start | shape38 | default | | Entry point |
| shape38 | documentproperties | shape17 | default | | Set input details |
| shape17 | catcherrors | shape29 | default | Try | Try block |
| shape17 | catcherrors | shape20 | error | Catch | Catch block |
| shape29 | map | shape8 | default | | Map to Oracle format |
| shape8 | documentproperties | shape49 | default | | Set URL |
| shape49 | notify | shape33 | default | | Log payload |
| shape33 | connectoraction | shape2 | default | | HTTP POST to Oracle |
| shape2 | decision | shape34 | true | True | HTTP status 20* |
| shape2 | decision | shape44 | false | False | HTTP status non-20* |
| shape34 | map | shape35 | default | | Map success response |
| shape35 | returndocuments | - | - | | Success return |
| shape44 | decision | shape45 | true | True | Content-Encoding = gzip |
| shape44 | decision | shape39 | false | False | Content-Encoding != gzip |
| shape45 | dataprocess | shape46 | default | | Decompress gzip |
| shape46 | documentproperties | shape47 | default | | Set error message |
| shape47 | map | shape48 | default | | Map error response |
| shape48 | returndocuments | - | - | | Error return (gzip) |
| shape39 | documentproperties | shape40 | default | | Set error message |
| shape40 | map | shape36 | default | | Map error response |
| shape36 | returndocuments | - | - | | Error return |
| shape20 | branch | shape19 | 1 | 1 | Branch path 1 |
| shape20 | branch | shape41 | 2 | 2 | Branch path 2 |
| shape19 | documentproperties | shape21 | default | | Set error message |
| shape21 | processcall | - | - | | Call email subprocess |
| shape41 | map | shape43 | default | | Map error response |
| shape43 | returndocuments | - | - | | Error return (catch) |

### Connection Summary

- **Total Shapes:** 20 main process shapes + 1 subprocess call
- **Total Connections:** 24 dragpoint connections
- **Shapes with Multiple Outgoing Connections:**
  - shape17 (catcherrors): 2 connections (Try, Catch)
  - shape2 (decision): 2 connections (True, False)
  - shape44 (decision): 2 connections (True, False)
  - shape20 (branch): 2 connections (Path 1, Path 2)

### Reverse Flow Mapping (Step 6)

**Convergence Points:** None identified (all paths lead to different return points)

| Target Shape | Incoming From Shapes | Notes |
|---|---|---|
| shape38 | shape1 | Single entry point |
| shape17 | shape38 | Single path |
| shape29 | shape17 | Try path only |
| shape20 | shape17 | Catch path only |
| shape21 | shape19 | Error notification path |

**No convergence points identified** - All decision and branch paths lead to separate return documents.

---

## 10. Decision Shape Analysis (Step 7)

### 🔍 SELF-CHECK (MANDATORY)

✅ **Decision data sources identified:** YES  
✅ **Decision types classified:** YES  
✅ **Execution order verified:** YES  
✅ **All decision paths traced:** YES  
✅ **Decision patterns identified:** YES  
✅ **Paths traced to termination:** YES

### Decision Inventory

#### Decision 1: HTTP Status 20 check (shape2)

**Shape ID:** shape2  
**Comparison Type:** wildcard  
**Value 1:** meta.base.applicationstatuscode (track property)  
**Value 2:** "20*" (static)

**Data Source:** TRACK_PROPERTY (meta.base.applicationstatuscode)  
**Decision Type:** POST_OPERATION (checks response from HTTP operation)  
**Actual Execution Order:** 
```
Operation: Leave Oracle Fusion Create (shape33) 
  → Produces: meta.base.applicationstatuscode 
  → Decision: HTTP Status 20 check (shape2) 
  → Routes to Success or Error path
```

**TRUE Path:**
- **Destination:** shape34 (Oracle Fusion Leave Response Map)
- **Termination:** shape35 (Success Response - Return Documents)
- **Type:** Success path - HTTP 200

**FALSE Path:**
- **Destination:** shape44 (Check Response Content Type)
- **Termination:** shape36 or shape48 (Error Response - Return Documents)
- **Type:** Error path - HTTP 400

**Pattern:** POST_OPERATION - Error Check (Success vs Failure based on HTTP status)  
**Convergence Point:** None (paths lead to different returns)  
**Early Exit:** No (both paths return documents)

**Business Logic:** If Oracle Fusion API returns HTTP status 20*, process success response. Otherwise, check if response is gzipped and process error response accordingly.

#### Decision 2: Check Response Content Type (shape44)

**Shape ID:** shape44  
**Comparison Type:** equals  
**Value 1:** dynamicdocument.DDP_RespHeader (track property)  
**Value 2:** "gzip" (static)

**Data Source:** TRACK_PROPERTY (dynamicdocument.DDP_RespHeader)  
**Decision Type:** POST_OPERATION (checks response header from HTTP operation)  
**Actual Execution Order:**
```
Operation: Leave Oracle Fusion Create (shape33) 
  → Produces: dynamicdocument.DDP_RespHeader 
  → Decision: HTTP Status 20 check (shape2) → FALSE 
  → Decision: Check Response Content Type (shape44) 
  → Routes to Gzip or Non-Gzip error path
```

**TRUE Path:**
- **Destination:** shape45 (Groovy script - decompress gzip)
- **Termination:** shape48 (Error Response - Return Documents)
- **Type:** Error path with gzip decompression - HTTP 400

**FALSE Path:**
- **Destination:** shape39 (set error message)
- **Termination:** shape36 (Error Response - Return Documents)
- **Type:** Error path without decompression - HTTP 400

**Pattern:** POST_OPERATION - Conditional Logic (Gzip vs Non-Gzip error handling)  
**Convergence Point:** None (paths lead to different returns)  
**Early Exit:** No (both paths return documents)

**Business Logic:** If Oracle Fusion API returns error response with gzip encoding, decompress before extracting error message. Otherwise, extract error message directly.

### Decision Patterns Summary

| Pattern Type | Decision Shapes | Description |
|---|---|---|
| Error Check (Success vs Failure) | shape2 | Check HTTP status code to determine success or error path |
| Conditional Logic (Error Handling) | shape44 | Check response encoding to determine decompression need |

### Decision Data Source Analysis

**Decision 1 (shape2):**
- **Data Source:** TRACK_PROPERTY (meta.base.applicationstatuscode)
- **Source Operation:** Leave Oracle Fusion Create (shape33)
- **Type:** POST_OPERATION
- **Proof:** HTTP status code is produced by HTTP operation (shape33) and consumed by decision (shape2)

**Decision 2 (shape44):**
- **Data Source:** TRACK_PROPERTY (dynamicdocument.DDP_RespHeader)
- **Source Operation:** Leave Oracle Fusion Create (shape33)
- **Type:** POST_OPERATION
- **Proof:** Response header is captured by HTTP operation (shape33) via responseHeaderMapping and consumed by decision (shape44)

---

## 11. Branch Shape Analysis (Step 8)

### 🔍 SELF-CHECK (MANDATORY)

✅ **Classification completed:** YES  
✅ **Assumption check:** NO (analyzed dependencies)  
✅ **Properties extracted:** YES  
✅ **Dependency graph built:** YES  
✅ **Topological sort applied:** N/A (no dependencies between paths)

### Branch Shape: shape20 (Error Notification Branch)

**Shape ID:** shape20  
**Number of Paths:** 2  
**Location:** Catch block of Try/Catch (shape17)

#### Path Properties Analysis

**Path 1 (shape19 → shape21):**
- **Reads:** 
  - meta.base.catcherrorsmessage (track property)
- **Writes:** 
  - process.DPP_ErrorMessage
- **Operations:** 
  - shape19: documentproperties (set error message)
  - shape21: processcall (email subprocess)
- **API Calls:** ❌ NO (subprocess calls email operation, but not direct API call)

**Path 2 (shape41 → shape43):**
- **Reads:** 
  - process.DPP_ErrorMessage (written by Path 1)
- **Writes:** None
- **Operations:** 
  - shape41: map (error map)
  - shape43: returndocuments
- **API Calls:** ❌ NO

#### Dependency Graph

```
Path 1 (shape19 → shape21)
  ↓ WRITES: process.DPP_ErrorMessage
  ↓ (consumed by)
Path 2 (shape41 → shape43) - READS process.DPP_ErrorMessage
```

**Dependency:** Path 2 depends on Path 1 (reads property written by Path 1)

#### Classification

**Classification:** SEQUENTIAL  
**Reason:** Path 2 reads process.DPP_ErrorMessage which is written by Path 1, creating a data dependency.

**Proof:**
- Path 1 (shape19) writes process.DPP_ErrorMessage from meta.base.catcherrorsmessage
- Path 2 (shape41) reads process.DPP_ErrorMessage via map function (PropertyGet)
- Therefore, Path 1 MUST execute BEFORE Path 2

#### Topological Sort Order

**Execution Order:** Path 1 → Path 2

**Reasoning:**
1. Path 1 captures error message and sends email notification
2. Path 2 maps error message to response format
3. Path 2 depends on error message from Path 1

#### Path Termination

**Path 1 Termination:** Subprocess call (shape21) - no explicit return in main process  
**Path 2 Termination:** shape43 (Return Documents - Error Response)

#### Convergence Points

**Convergence:** None - Path 1 calls subprocess, Path 2 returns documents

#### Execution Continuation

**Execution Continues From:** Path 2 (shape43) - Return Documents (process ends)

### Branch Analysis Summary

| Branch Shape | Classification | Dependency Order | Convergence | Continuation |
|---|---|---|---|---|
| shape20 | SEQUENTIAL | Path 1 → Path 2 | None | shape43 (Return) |

---

## 12. Execution Order (Step 9)

### 🔍 SELF-CHECK (MANDATORY)

✅ **Business logic verified FIRST:** YES  
✅ **Operation analysis complete:** YES  
✅ **Business logic execution order identified:** YES  
✅ **Data dependencies checked FIRST:** YES  
✅ **Operation response analysis used:** YES (Step 1c)  
✅ **Decision analysis used:** YES (Step 7)  
✅ **Dependency graph used:** YES (Step 4)  
✅ **Branch analysis used:** YES (Step 8)  
✅ **Property dependency verification:** YES  
✅ **Topological sort applied:** YES (for branch shape20)

### Business Logic Flow (Step 0 - MUST BE FIRST)

#### Operation Analysis

**Operation 1: Create Leave Oracle Fusion OP (8f709c2b-e63f-4d5f-9374-2932ed70415d)**
- **Purpose:** Web Service Server - Entry point for leave creation from D365
- **Outputs:** Input document (leave request)
- **Dependent Operations:** All downstream operations depend on this input
- **Business Flow:** This is the entry point, must execute first to receive leave request

**Operation 2: Leave Oracle Fusion Create (6e8920fd-af5a-430b-a1d9-9fde7ac29a12)**
- **Purpose:** HTTP POST to Oracle Fusion HCM API to create absence entry
- **Outputs:** 
  - meta.base.applicationstatuscode (HTTP status code)
  - meta.base.applicationstatusmessage (HTTP response message)
  - dynamicdocument.DDP_RespHeader (Content-Encoding header)
  - Response body (Oracle Fusion absence entry)
- **Dependent Operations:** Decision shapes (shape2, shape44) depend on HTTP status and headers
- **Business Flow:** Core operation that creates leave in Oracle Fusion, all subsequent logic depends on its response

**Operation 3: Email w Attachment / Email W/O Attachment (af07502a-fafd-4976-a691-45d51a33b549 / 15a72a21-9b57-49a1-a8ed-d70367146644)**
- **Purpose:** Send error notification email to integration team
- **Outputs:** None (side effect - email sent)
- **Dependent Operations:** None
- **Business Flow:** Error notification only, executed in catch block when errors occur

#### Business Logic Execution Order

**Primary Flow (Happy Path):**
1. **Receive leave request** from D365 (Entry point operation)
2. **Transform request** to Oracle Fusion format (Map)
3. **Set API endpoint URL** (Document property)
4. **Call Oracle Fusion API** to create absence entry (HTTP POST)
5. **Check HTTP status code** (Decision)
6. **Map success response** to D365 format (Map)
7. **Return success response** to D365 (Return Documents)

**Error Flow (HTTP Error):**
1. **Receive leave request** from D365 (Entry point operation)
2. **Transform request** to Oracle Fusion format (Map)
3. **Set API endpoint URL** (Document property)
4. **Call Oracle Fusion API** to create absence entry (HTTP POST)
5. **Check HTTP status code** (Decision) → Non-20* status
6. **Check response encoding** (Decision)
7. **Decompress response if gzipped** (Groovy script - conditional)
8. **Extract error message** (Document property)
9. **Map error response** to D365 format (Map)
10. **Return error response** to D365 (Return Documents)

**Exception Flow (Try/Catch):**
1. **Receive leave request** from D365 (Entry point operation)
2. **Exception occurs** during processing (e.g., connection timeout)
3. **Catch exception** (Try/Catch block)
4. **Branch execution:**
   - **Path 1:** Capture error message → Send email notification (Subprocess)
   - **Path 2:** Map error response → Return error response to D365
5. **Sequential execution:** Path 1 MUST execute BEFORE Path 2 (data dependency)

### Execution Order List

Based on dependency graph, decision analysis, and branch analysis:

**Main Flow:**
```
1. shape1 (START - Entry point)
2. shape38 (Input_details - Set process properties)
3. shape17 (Try/Catch - Error handling wrapper)
   
   TRY BLOCK:
   4. shape29 (Map - Transform to Oracle format)
   5. shape8 (set URL - Set API endpoint)
   6. shape49 (Notify - Log payload)
   7. shape33 (HTTP POST - Call Oracle Fusion API) [DOWNSTREAM]
   8. shape2 (Decision - HTTP Status 20 check)
   
      IF TRUE (HTTP 20*):
      9a. shape34 (Map - Success response)
      10a. shape35 (Return Documents - Success) [HTTP 200]
      
      IF FALSE (HTTP non-20*):
      9b. shape44 (Decision - Check Response Content Type)
      
         IF TRUE (Content-Encoding = gzip):
         10b1. shape45 (Groovy - Decompress gzip)
         11b1. shape46 (set error message)
         12b1. shape47 (Map - Error response)
         13b1. shape48 (Return Documents - Error) [HTTP 400]
         
         IF FALSE (Content-Encoding != gzip):
         10b2. shape39 (set error message)
         11b2. shape40 (Map - Error response)
         12b2. shape36 (Return Documents - Error) [HTTP 400]
   
   CATCH BLOCK:
   4. shape20 (Branch - Error notification)
      
      SEQUENTIAL EXECUTION (Path 1 → Path 2):
      Path 1:
      5a. shape19 (set error message)
      6a. shape21 (Subprocess - Email notification) [DOWNSTREAM]
      
      Path 2:
      5b. shape41 (Map - Error response)
      6b. shape43 (Return Documents - Error) [HTTP 500]
```

### Dependency Verification

**Dependency 1: URL Setup → HTTP Operation**
- shape8 writes dynamicdocument.URL
- shape33 reads dynamicdocument.URL
- **Verification:** ✅ shape8 executes BEFORE shape33

**Dependency 2: HTTP Operation → Status Check**
- shape33 produces meta.base.applicationstatuscode
- shape2 reads meta.base.applicationstatuscode
- **Verification:** ✅ shape33 executes BEFORE shape2

**Dependency 3: HTTP Operation → Content Type Check**
- shape33 produces dynamicdocument.DDP_RespHeader
- shape44 reads dynamicdocument.DDP_RespHeader
- **Verification:** ✅ shape33 executes BEFORE shape44

**Dependency 4: Error Capture → Error Mapping (Branch Path 1 → Path 2)**
- shape19 writes process.DPP_ErrorMessage
- shape41 reads process.DPP_ErrorMessage
- **Verification:** ✅ shape19 (Path 1) executes BEFORE shape41 (Path 2)

### Branch Execution Order

**Branch: shape20 (Error Notification)**
- **Classification:** SEQUENTIAL (data dependency)
- **Order:** Path 1 (shape19 → shape21) → Path 2 (shape41 → shape43)
- **Reasoning:** Path 2 reads process.DPP_ErrorMessage written by Path 1

### Decision Path Tracing

**Decision 1: shape2 (HTTP Status 20 check)**
- **TRUE Path:** shape34 → shape35 (Success Response)
- **FALSE Path:** shape44 → (shape45 → shape46 → shape47 → shape48) OR (shape39 → shape40 → shape36)
- **Convergence:** None (separate return points)

**Decision 2: shape44 (Check Response Content Type)**
- **TRUE Path:** shape45 → shape46 → shape47 → shape48 (Gzip error)
- **FALSE Path:** shape39 → shape40 → shape36 (Non-gzip error)
- **Convergence:** None (separate return points)

---

## 13. Sequence Diagram (Step 10)

**📋 NOTE:** This sequence diagram is based on the analysis in Steps 4, 5, 7, 8, and 9. Detailed request/response JSON examples are documented in Section 16.

**🔍 VALIDATION:**
- ✅ Based on dependency graph in Step 4
- ✅ Based on control flow graph in Step 5
- ✅ Based on decision analysis in Step 7
- ✅ Based on branch analysis in Step 8
- ✅ Based on execution order in Step 9

### Main Process Flow

```
START (shape1)
 |
 ├─→ shape38: Input_details (documentproperties)
 |   └─→ WRITES: process.DPP_Process_Name, process.DPP_AtomName, process.DPP_Payload,
 |                process.DPP_ExecutionID, process.DPP_File_Name, process.DPP_Subject,
 |                process.To_Email, process.DPP_HasAttachment
 |
 ├─→ shape17: Try/Catch (catcherrors)
 |   |
 |   ├─→ TRY PATH:
 |   |   |
 |   |   ├─→ shape29: Leave Create Map (map)
 |   |   |   └─→ Transform D365 request to Oracle Fusion format
 |   |   |
 |   |   ├─→ shape8: set URL (documentproperties)
 |   |   |   └─→ WRITES: dynamicdocument.URL
 |   |   |   └─→ READS: PP_HCM_LeaveCreate_Properties.Resource_Path
 |   |   |
 |   |   ├─→ shape49: Notify (notify)
 |   |   |   └─→ Log payload for debugging
 |   |   |
 |   |   ├─→ shape33: Leave Oracle Fusion Create (HTTP POST) [DOWNSTREAM]
 |   |   |   └─→ READS: dynamicdocument.URL
 |   |   |   └─→ WRITES: meta.base.applicationstatuscode, meta.base.applicationstatusmessage,
 |   |   |                dynamicdocument.DDP_RespHeader
 |   |   |   └─→ HTTP: [Expected: 200/201, Error: 400/401/404/500]
 |   |   |   └─→ Endpoint: https://iaaxey-dev3.fa.ocs.oraclecloud.com:443/hcmRestApi/resources/11.13.18.05/absences
 |   |   |
 |   |   ├─→ shape2: HTTP Status 20 check (decision)
 |   |   |   └─→ READS: meta.base.applicationstatuscode
 |   |   |   └─→ Comparison: meta.base.applicationstatuscode wildcard "20*"
 |   |   |   |
 |   |   |   ├─→ IF TRUE (HTTP 20*):
 |   |   |   |   |
 |   |   |   |   ├─→ shape34: Oracle Fusion Leave Response Map (map)
 |   |   |   |   |   └─→ Map Oracle response to D365 format
 |   |   |   |   |   └─→ Set defaults: status="success", message="Data successfully sent to Oracle Fusion", success="true"
 |   |   |   |   |
 |   |   |   |   └─→ shape35: Success Response (returndocuments) [HTTP 200] [SUCCESS]
 |   |   |   |       └─→ Response: { "status": "success", "message": "...", "personAbsenceEntryId": 123, "success": "true" }
 |   |   |   |
 |   |   |   └─→ IF FALSE (HTTP non-20*):
 |   |   |       |
 |   |   |       ├─→ shape44: Check Response Content Type (decision)
 |   |   |       |   └─→ READS: dynamicdocument.DDP_RespHeader
 |   |   |       |   └─→ Comparison: dynamicdocument.DDP_RespHeader equals "gzip"
 |   |   |       |   |
 |   |   |       |   ├─→ IF TRUE (Content-Encoding = gzip):
 |   |   |       |   |   |
 |   |   |       |   |   ├─→ shape45: Groovy Script (dataprocess)
 |   |   |       |   |   |   └─→ Decompress gzip response
 |   |   |       |   |   |
 |   |   |       |   |   ├─→ shape46: error msg (documentproperties)
 |   |   |       |   |   |   └─→ WRITES: process.DPP_ErrorMessage
 |   |   |       |   |   |   └─→ READS: meta.base.applicationstatusmessage (decompressed)
 |   |   |       |   |   |
 |   |   |       |   |   ├─→ shape47: Leave Error Map (map)
 |   |   |       |   |   |   └─→ READS: process.DPP_ErrorMessage
 |   |   |       |   |   |   └─→ Set defaults: status="failure", success="false"
 |   |   |       |   |   |
 |   |   |       |   |   └─→ shape48: Error Response (returndocuments) [HTTP 400] [ERROR]
 |   |   |       |   |       └─→ Response: { "status": "failure", "message": "...", "success": "false" }
 |   |   |       |   |
 |   |   |       |   └─→ IF FALSE (Content-Encoding != gzip):
 |   |   |       |       |
 |   |   |       |       ├─→ shape39: error msg (documentproperties)
 |   |   |       |       |   └─→ WRITES: process.DPP_ErrorMessage
 |   |   |       |       |   └─→ READS: meta.base.applicationstatusmessage
 |   |   |       |       |
 |   |   |       |       ├─→ shape40: Leave Error Map (map)
 |   |   |       |       |   └─→ READS: process.DPP_ErrorMessage
 |   |   |       |       |   └─→ Set defaults: status="failure", success="false"
 |   |   |       |       |
 |   |   |       |       └─→ shape36: Error Response (returndocuments) [HTTP 400] [ERROR]
 |   |   |       |           └─→ Response: { "status": "failure", "message": "...", "success": "false" }
 |   |   |
 |   |   └─→ [END TRY PATH]
 |   |
 |   └─→ CATCH PATH (Exception):
 |       |
 |       ├─→ shape20: Branch (branch) - SEQUENTIAL EXECUTION
 |       |   |
 |       |   ├─→ PATH 1 (Error Notification):
 |       |   |   |
 |       |   |   ├─→ shape19: ErrorMsg (documentproperties)
 |       |   |   |   └─→ WRITES: process.DPP_ErrorMessage
 |       |   |   |   └─→ READS: meta.base.catcherrorsmessage
 |       |   |   |
 |       |   |   └─→ shape21: Subprocess - Office 365 Email (processcall) [DOWNSTREAM]
 |       |   |       └─→ READS: process.DPP_Process_Name, process.DPP_AtomName, process.DPP_Payload,
 |       |   |                  process.DPP_ExecutionID, process.DPP_File_Name, process.DPP_Subject,
 |       |   |                  process.To_Email, process.DPP_HasAttachment, process.DPP_ErrorMessage
 |       |   |       └─→ Sends error notification email to integration team
 |       |   |       └─→ [See Subprocess Analysis for internal flow]
 |       |   |
 |       |   └─→ PATH 2 (Error Response) - EXECUTES AFTER PATH 1:
 |       |       |
 |       |       ├─→ shape41: Leave Error Map (map)
 |       |       |   └─→ READS: process.DPP_ErrorMessage (written by PATH 1)
 |       |       |   └─→ Set defaults: status="failure", success="false"
 |       |       |
 |       |       └─→ shape43: Error Response (returndocuments) [HTTP 500] [ERROR]
 |       |           └─→ Response: { "status": "failure", "message": "...", "success": "false" }
 |       |
 |       └─→ [END CATCH PATH]
 |
 └─→ [END PROCESS]
```

**Note:** Detailed request/response JSON examples for all operations and return paths are documented in Section 16 (Request/Response JSON Examples).

---

## 14. Subprocess Analysis (Step 7a)

### Subprocess: (Sub) Office 365 Email (a85945c5-3004-42b9-80b1-104f465cd1fb)

**Purpose:** Reusable email subprocess for sending error notifications with or without attachments

#### Internal Flow

```
START (shape1)
 |
 ├─→ shape2: Try/Catch (catcherrors)
 |   |
 |   ├─→ TRY PATH:
 |   |   |
 |   |   ├─→ shape4: Attachment_Check (decision)
 |   |   |   └─→ READS: process.DPP_HasAttachment
 |   |   |   └─→ Comparison: process.DPP_HasAttachment equals "Y"
 |   |   |   |
 |   |   |   ├─→ IF TRUE (Has Attachment):
 |   |   |   |   |
 |   |   |   |   ├─→ shape11: Mail_Body (message)
 |   |   |   |   |   └─→ READS: process.DPP_Process_Name, process.DPP_AtomName, 
 |   |   |   |   |              process.DPP_ExecutionID, process.DPP_ErrorMessage
 |   |   |   |   |   └─→ Build HTML email body with execution details
 |   |   |   |   |
 |   |   |   |   ├─→ shape14: set_MailBody (documentproperties)
 |   |   |   |   |   └─→ WRITES: process.DPP_MailBody
 |   |   |   |   |   └─→ READS: Current document (HTML body)
 |   |   |   |   |
 |   |   |   |   ├─→ shape15: payload (message)
 |   |   |   |   |   └─→ READS: process.DPP_Payload
 |   |   |   |   |   └─→ Set document to payload content (for attachment)
 |   |   |   |   |
 |   |   |   |   ├─→ shape6: set_Mail_Properties (documentproperties)
 |   |   |   |   |   └─→ WRITES: connector.mail.fromAddress, connector.mail.toAddress,
 |   |   |   |   |              connector.mail.subject, connector.mail.body, connector.mail.filename
 |   |   |   |   |   └─→ READS: PP_Office365_Email.From_Email, process.To_Email,
 |   |   |   |   |              PP_Office365_Email.Environment, process.DPP_Subject,
 |   |   |   |   |              process.DPP_MailBody, process.DPP_File_Name
 |   |   |   |   |
 |   |   |   |   ├─→ shape3: Email (mail) [DOWNSTREAM]
 |   |   |   |   |   └─→ Operation: Email w Attachment
 |   |   |   |   |   └─→ Send email with attachment
 |   |   |   |   |
 |   |   |   |   └─→ shape5: Stop (stop - continue=true) [SUCCESS RETURN]
 |   |   |   |
 |   |   |   └─→ IF FALSE (No Attachment):
 |   |   |       |
 |   |   |       ├─→ shape23: Mail_Body (message)
 |   |   |       |   └─→ READS: process.DPP_Process_Name, process.DPP_AtomName,
 |   |   |       |              process.DPP_ExecutionID, process.DPP_ErrorMessage
 |   |   |       |   └─→ Build HTML email body with execution details
 |   |   |       |
 |   |   |       ├─→ shape22: set_MailBody (documentproperties)
 |   |   |       |   └─→ WRITES: process.DPP_MailBody
 |   |   |       |   └─→ READS: Current document (HTML body)
 |   |   |       |
 |   |   |       ├─→ shape20: set_Mail_Properties (documentproperties)
 |   |   |       |   └─→ WRITES: connector.mail.fromAddress, connector.mail.toAddress,
 |   |   |       |              connector.mail.subject, connector.mail.body
 |   |   |       |   └─→ READS: PP_Office365_Email.From_Email, process.To_Email,
 |   |   |       |              PP_Office365_Email.Environment, process.DPP_Subject,
 |   |   |       |              process.DPP_MailBody
 |   |   |       |
 |   |   |       ├─→ shape7: Email (mail) [DOWNSTREAM]
 |   |   |       |   └─→ Operation: Email W/O Attachment
 |   |   |       |   └─→ Send email without attachment
 |   |   |       |
 |   |   |       └─→ shape9: Stop (stop - continue=true) [SUCCESS RETURN]
 |   |   |
 |   |   └─→ [END TRY PATH]
 |   |
 |   └─→ CATCH PATH (Exception):
 |       |
 |       └─→ shape10: Exception (exception)
 |           └─→ READS: meta.base.catcherrorsmessage
 |           └─→ Throw exception with error message [ERROR RETURN]
 |
 └─→ [END SUBPROCESS]
```

#### Return Paths

| Return Label | Return Type | Condition | Main Process Mapping |
|---|---|---|---|
| SUCCESS | Stop (continue=true) | Email sent successfully (with or without attachment) | Continue main process execution |
| ERROR | Exception | Email sending failed | Exception propagates to main process |

#### Properties Read by Subprocess

| Property Name | Usage |
|---|---|
| process.DPP_HasAttachment | Decision: Send email with or without attachment |
| process.DPP_Process_Name | Email body parameter |
| process.DPP_AtomName | Email body parameter |
| process.DPP_ExecutionID | Email body parameter |
| process.DPP_ErrorMessage | Email body parameter |
| process.DPP_Payload | Email attachment content |
| process.DPP_File_Name | Email attachment filename |
| process.To_Email | Email recipient |
| process.DPP_Subject | Email subject |
| PP_Office365_Email.From_Email | Email sender |
| PP_Office365_Email.Environment | Email subject prefix |

#### Properties Written by Subprocess

| Property Name | Value |
|---|---|
| process.DPP_MailBody | HTML email body content |
| connector.mail.fromAddress | Email sender address |
| connector.mail.toAddress | Email recipient address |
| connector.mail.subject | Email subject line |
| connector.mail.body | Email body content |
| connector.mail.filename | Email attachment filename (if applicable) |

---

## 15. System Layer Identification

### Third-Party Systems

| System Name | Type | Purpose | Operations |
|---|---|---|---|
| Oracle Fusion HCM | REST API | Human Capital Management - Absence Management | Leave Oracle Fusion Create (HTTP POST) |
| Office 365 Email | SMTP | Email notification service | Email w Attachment, Email W/O Attachment |

### System Layer APIs Required

#### System Layer API 1: Oracle Fusion HCM - Absence Management

**API Name:** OracleFusionHcmAbsenceApi  
**Base URL:** https://iaaxey-dev3.fa.ocs.oraclecloud.com:443  
**Authentication:** Basic Auth  
**Operations:**

1. **CreateAbsence**
   - **Method:** POST
   - **Endpoint:** /hcmRestApi/resources/11.13.18.05/absences
   - **Request Body:** JSON (HCM Leave Create JSON Profile)
   - **Response Body:** JSON (Oracle Fusion Leave Response JSON Profile)
   - **Success Status Codes:** 200, 201
   - **Error Status Codes:** 400, 401, 404, 500
   - **Special Handling:** Response may be gzip-encoded

#### System Layer API 2: Office 365 Email Service

**API Name:** Office365EmailApi  
**Host:** smtp-mail.outlook.com:587  
**Authentication:** SMTP AUTH with TLS  
**Operations:**

1. **SendEmailWithAttachment**
   - **Protocol:** SMTP
   - **From:** Configurable (default: Boomi.Dev.failures@al-ghurair.com)
   - **To:** Configurable (default: BoomiIntegrationTeam@al-ghurair.com)
   - **Subject:** Dynamic (process name + environment + error message)
   - **Body:** HTML format with execution details
   - **Attachment:** Text file with error payload

2. **SendEmailWithoutAttachment**
   - **Protocol:** SMTP
   - **From:** Configurable (default: Boomi.Dev.failures@al-ghurair.com)
   - **To:** Configurable (default: BoomiIntegrationTeam@al-ghurair.com)
   - **Subject:** Dynamic (process name + environment + error message)
   - **Body:** HTML format with execution details

---

## 16. Request/Response JSON Examples

### Process Layer Entry Point

**Request JSON Example:**

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

**Response JSON Examples:**

**Success Response (HTTP 200):**

```json
{
  "status": "success",
  "message": "Data successfully sent to Oracle Fusion",
  "personAbsenceEntryId": 300100123456789,
  "success": "true"
}
```

**Error Response - HTTP Error (HTTP 400):**

```json
{
  "status": "failure",
  "message": "Oracle Fusion API returned error: Invalid absence type 'Sick Leave' for employee 9000604",
  "success": "false"
}
```

**Error Response - Exception (HTTP 500):**

```json
{
  "status": "failure",
  "message": "Connection timeout to Oracle Fusion API after 30 seconds",
  "success": "false"
}
```

### Downstream System Layer Calls

#### Operation: Leave Oracle Fusion Create (HTTP POST)

**Request JSON:**

```json
{
  "personNumber": 9000604,
  "absenceType": "Sick Leave",
  "employer": "Al Ghurair Investment LLC",
  "startDate": "2024-03-24",
  "endDate": "2024-03-25",
  "absenceStatusCd": "SUBMITTED",
  "approvalStatusCd": "APPROVED",
  "startDateDuration": 1,
  "endDateDuration": 1
}
```

**Response JSON (Success - HTTP 200/201):**

```json
{
  "personAbsenceEntryId": 300100123456789,
  "absenceCaseId": "ABS-2024-001234",
  "absenceEntryBasicFlag": true,
  "absencePatternCd": "FULL_DAY",
  "absenceStatusCd": "SUBMITTED",
  "absenceTypeId": 300100000123456,
  "approvalStatusCd": "APPROVED",
  "personId": 300100987654321,
  "personNumber": "9000604",
  "absenceType": "Sick Leave",
  "employer": "Al Ghurair Investment LLC",
  "startDate": "2024-03-24",
  "endDate": "2024-03-25",
  "startDateDuration": 1,
  "endDateDuration": 1,
  "duration": 2,
  "unitOfMeasure": "DAYS",
  "createdBy": "INTEGRATION.USER",
  "creationDate": "2024-03-24T10:30:00Z",
  "lastUpdatedBy": "INTEGRATION.USER",
  "lastUpdateDate": "2024-03-24T10:30:00Z",
  "objectVersionNumber": 1,
  "links": [
    {
      "rel": "self",
      "href": "https://iaaxey-dev3.fa.ocs.oraclecloud.com:443/hcmRestApi/resources/11.13.18.05/absences/300100123456789",
      "name": "absences",
      "kind": "item"
    }
  ]
}
```

**Response JSON (Error - HTTP 400):**

```json
{
  "title": "Bad Request",
  "status": 400,
  "detail": "Invalid absence type 'Sick Leave' for employee 9000604. Absence type not configured for this employee's legal entity.",
  "o:errorCode": "VALIDATION_ERROR",
  "o:errorPath": "absenceType"
}
```

**Response JSON (Error - HTTP 401):**

```json
{
  "title": "Unauthorized",
  "status": 401,
  "detail": "Authentication failed. Invalid credentials provided.",
  "o:errorCode": "AUTH_ERROR"
}
```

**Response JSON (Error - HTTP 404):**

```json
{
  "title": "Not Found",
  "status": 404,
  "detail": "Employee with number 9000604 not found in Oracle Fusion HCM.",
  "o:errorCode": "EMPLOYEE_NOT_FOUND"
}
```

**Response JSON (Error - HTTP 500):**

```json
{
  "title": "Internal Server Error",
  "status": 500,
  "detail": "An unexpected error occurred while processing the absence request.",
  "o:errorCode": "INTERNAL_ERROR"
}
```

---

## 17. Function Exposure Decision Table

### Process Layer Function

| Function Name | HTTP Method | Route | Expose? | Reasoning |
|---|---|---|---|---|
| CreateLeave | POST | /api/hcm/leave/create | ✅ YES | **PRIMARY BUSINESS FUNCTION** - This is the main entry point for creating leave/absence entries in Oracle Fusion HCM from D365. This function orchestrates the leave creation process and handles error scenarios. |

**Justification for Exposure:**
- **Business Value:** Core integration function for synchronizing leave data between D365 and Oracle Fusion HCM
- **Orchestration:** Handles transformation, API call, error handling, and notifications
- **Error Handling:** Comprehensive error handling with multiple return paths
- **Reusability:** Can be called by multiple upstream systems (D365, other HR systems)
- **API-Led Architecture:** Fits Process Layer pattern - orchestrates System Layer calls and business logic

### System Layer Functions

| Function Name | HTTP Method | Route | Expose? | Reasoning |
|---|---|---|---|---|
| OracleFusionHcmAbsenceApi.CreateAbsence | POST | /api/systems/oracle-fusion-hcm/absences | ✅ YES | **SYSTEM LAYER API** - Direct integration with Oracle Fusion HCM Absence API. Should be exposed as System Layer function for reusability across multiple Process Layer functions. |
| Office365EmailApi.SendEmail | POST | /api/systems/email/send | ✅ YES | **SYSTEM LAYER API** - Reusable email notification service. Should be exposed as System Layer function for use by multiple Process Layer functions. |

**Function Exposure Summary:**
- **Total Functions Identified:** 3
- **Process Layer Functions to Expose:** 1 (CreateLeave)
- **System Layer Functions to Expose:** 2 (OracleFusionHcmAbsenceApi.CreateAbsence, Office365EmailApi.SendEmail)
- **Functions NOT to Expose:** 0

**Critical Note:** This process follows proper API-Led Architecture principles:
- Process Layer orchestrates business logic and error handling
- System Layer provides reusable APIs for Oracle Fusion HCM and Email services
- Clear separation of concerns between layers

---

## 18. Validation Checklist

### Data Dependencies
- [x] All property WRITES identified
- [x] All property READS identified
- [x] Dependency graph built
- [x] Execution order satisfies all dependencies (no read-before-write)

### Decision Analysis
- [x] ALL decision shapes inventoried (2 decisions)
- [x] BOTH TRUE and FALSE paths traced to termination
- [x] Pattern type identified for each decision
- [x] Early exits identified and documented (none - all paths return documents)
- [x] Convergence points identified (none)
- [x] Decision data sources identified (TRACK_PROPERTY)
- [x] Decision types classified (POST_OPERATION)
- [x] Actual execution order verified

### Branch Analysis
- [x] Each branch classified as parallel or sequential (1 branch - SEQUENTIAL)
- [x] **CRITICAL:** Branch contains NO API calls (email is in subprocess)
- [x] **SELF-CHECK:** Did I check for API calls in branch paths? **YES**
- [x] **SELF-CHECK:** Did I classify or assume? **Classified based on data dependencies**
- [x] Dependency order built using data dependency analysis
- [x] Each path traced to terminal point
- [x] Convergence points identified (none)
- [x] Execution continuation point determined

### Sequence Diagram
- [x] Format follows required structure (Operation → Decision → Operation)
- [x] Each operation shows READS and WRITES
- [x] Decisions show both TRUE and FALSE paths
- [x] **CROSS-VALIDATION:** Sequence diagram matches control flow graph from Step 5
- [x] **CROSS-VALIDATION:** Execution order matches dependency graph from Step 4
- [x] Early exits marked (none - all paths return documents)
- [x] Conditional execution marked
- [x] Subprocess internal flows documented
- [x] Subprocess return paths mapped to main process

### Subprocess Analysis
- [x] ALL subprocesses analyzed (1 subprocess - Office 365 Email)
- [x] Internal flow traced
- [x] Return paths identified (SUCCESS, ERROR)
- [x] Return path labels mapped to main process shapes
- [x] Properties written by subprocess documented
- [x] Properties read by subprocess from main process documented

### Edge Cases
- [x] Nested branches/decisions analyzed (none)
- [x] Loops identified (none)
- [x] Property chains traced (transitive dependencies)
- [x] Circular dependencies detected and resolved (none)
- [x] Try/Catch error paths documented

### Property Extraction Completeness
- [x] All property patterns searched (${}, %%, {})
- [x] Message parameters checked for process properties
- [x] Operation headers/path parameters checked
- [x] Decision track properties identified (meta.*)
- [x] Document properties that read other properties identified

### Input/Output Structure Analysis (CONTRACT VERIFICATION)
- [x] Entry point operation identified
- [x] Request profile identified and loaded
- [x] Request profile structure analyzed (JSON)
- [x] Array vs single object detected (single object)
- [x] ALL request fields extracted (9 fields)
- [x] Request field paths documented
- [x] Request field mapping table generated (Boomi → Azure DTO)
- [x] Response profile identified and loaded
- [x] Response profile structure analyzed
- [x] ALL response fields extracted (4 fields)
- [x] Response field mapping table generated
- [x] Document processing behavior determined (single document)
- [x] Input/Output structure documented in Phase 1 document

### HTTP Status Codes and Return Path Responses
- [x] All return paths documented with HTTP status codes (4 return paths)
- [x] Response JSON examples provided for each return path
- [x] Populated fields documented for each return path
- [x] Decision conditions leading to each return documented
- [x] Error codes and success codes documented
- [x] Downstream operation HTTP status codes documented

### Request/Response JSON Examples
- [x] Process Layer entry point request JSON example provided
- [x] Process Layer response JSON examples provided (all return paths)
- [x] Downstream System Layer request JSON examples provided
- [x] Downstream System Layer response JSON examples provided (success and error scenarios)

### Map Analysis
- [x] ALL map files identified and loaded (3 maps)
- [x] Field mappings extracted from each map
- [x] Profile vs map field name discrepancies documented (minor name changes)
- [x] Map field names marked as AUTHORITATIVE
- [x] Scripting functions analyzed (PropertyGet function in error map)
- [x] Static values identified and documented (default values in response maps)
- [x] Process property mappings documented
- [x] Map Analysis documented in Phase 1 document

### Function Exposure Decision Table
- [x] Function Exposure Decision Table complete
- [x] All functions identified (3 functions)
- [x] Exposure decisions made with reasoning
- [x] API-Led Architecture principles applied

---

## PHASE 1 COMPLETION SUMMARY

✅ **Phase 1 extraction complete**

**Summary:**
- **Process Type:** Leave creation integration between D365 and Oracle Fusion HCM
- **Complexity:** Medium - HTTP API integration with comprehensive error handling
- **Operations:** 4 operations (1 entry point, 1 HTTP API call, 2 email operations)
- **Decisions:** 2 decisions (HTTP status check, content encoding check)
- **Branches:** 1 branch (error notification - sequential execution)
- **Subprocesses:** 1 subprocess (Office 365 Email)
- **Return Paths:** 4 return paths (1 success, 3 error scenarios)
- **System Layer APIs Required:** 2 (Oracle Fusion HCM Absence API, Office 365 Email API)

**Key Findings:**
1. Process follows proper error handling patterns with Try/Catch and multiple error paths
2. HTTP status code checking determines success vs error routing
3. Special handling for gzip-encoded error responses
4. Email notification sent on exception errors (not HTTP errors)
5. Sequential branch execution required due to data dependency (error message property)
6. Comprehensive error response mapping for different error scenarios

**Ready for Phase 2:** ✅ YES - All mandatory sections complete, all self-checks passed
