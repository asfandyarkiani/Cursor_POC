# BOOMI EXTRACTION PHASE 1 - HCM Leave Create Process

**Process Name:** HCM_Leave Create  
**Process ID:** ca69f858-785f-4565-ba1f-b4edc6cca05b  
**Description:** This Process will sync the Leave data between D365 and Oracle HCM  
**Analysis Date:** 2026-02-11

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
15. [Critical Patterns Identified](#15-critical-patterns-identified)
16. [System Layer Identification](#16-system-layer-identification)
17. [Validation Checklist](#17-validation-checklist)
18. [Function Exposure Decision Table](#18-function-exposure-decision-table)

---

## 1. Operations Inventory

### Main Process Operations

| Operation ID | Operation Name | Type | SubType | Purpose |
|---|---|---|---|---|
| 8f709c2b-e63f-4d5f-9374-2932ed70415d | Create Leave Oracle Fusion OP | connector-action | wss | Web Service Server - Entry point for D365 leave requests |
| 6e8920fd-af5a-430b-a1d9-9fde7ac29a12 | Leave Oracle Fusion Create | connector-action | http | HTTP POST to Oracle Fusion HCM API |

### Subprocess Operations (a85945c5-3004-42b9-80b1-104f465cd1fb)

| Operation ID | Operation Name | Type | SubType | Purpose |
|---|---|---|---|---|
| af07502a-fafd-4976-a691-45d51a33b549 | Email w Attachment | connector-action | mail | Send email with attachment |
| 15a72a21-9b57-49a1-a8ed-d70367146644 | Email W/O Attachment | connector-action | mail | Send email without attachment |

### Connections

| Connection ID | Connection Name | Type | Purpose |
|---|---|---|---|
| aa1fcb29-d146-4425-9ea6-b9698090f60e | Oracle Fusion | http | Oracle Fusion HCM Cloud connection (Basic Auth) |
| 00eae79b-2303-4215-8067-dcc299e42697 | Office 365 Email | mail | Office 365 SMTP for error notifications |

---

## 2. Input Structure Analysis (Step 1a)

### Request Profile Structure

**Profile ID:** febfa3e1-f719-4ee8-ba57-cdae34137ab3  
**Profile Name:** D365 Leave Create JSON Profile  
**Profile Type:** profile.json  
**Input Type:** singlejson  
**Root Structure:** Root/Object

### Array Detection
**isArray:** ❌ NO - Single object structure  
**Array Cardinality:** N/A

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
- **Azure Function Requirement:** Accept single leave request object
- **Implementation Pattern:** Process single leave request, return response
- **Session Management:** One session per execution

### Request Field Mapping

| Boomi Field Path | Boomi Field Name | Data Type | Required | Azure DTO Property | Notes |
|---|---|---|---|---|
| Root/Object/employeeNumber | employeeNumber | number | Yes | EmployeeNumber | Used for tracking |
| Root/Object/absenceType | absenceType | character | Yes | AbsenceType | Leave type (Sick Leave, Annual Leave, etc.) |
| Root/Object/employer | employer | character | Yes | Employer | Legal entity name |
| Root/Object/startDate | startDate | character | Yes | StartDate | Leave start date (YYYY-MM-DD) |
| Root/Object/endDate | endDate | character | Yes | EndDate | Leave end date (YYYY-MM-DD) |
| Root/Object/absenceStatusCode | absenceStatusCode | character | Yes | AbsenceStatusCode | Status (SUBMITTED, APPROVED) |
| Root/Object/approvalStatusCode | approvalStatusCode | character | Yes | ApprovalStatusCode | Approval status |
| Root/Object/startDateDuration | startDateDuration | number | Yes | StartDateDuration | Duration in days for start date |
| Root/Object/endDateDuration | endDateDuration | number | Yes | EndDateDuration | Duration in days for end date |

**Total Fields:** 9 fields (all mappable, no nested structures)

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
| leaveResponse/Object/status | status | character | Status | "success" or "failure" |
| leaveResponse/Object/message | message | character | Message | Success/error message |
| leaveResponse/Object/personAbsenceEntryId | personAbsenceEntryId | number | PersonAbsenceEntryId | Oracle Fusion absence entry ID |
| leaveResponse/Object/success | success | character | Success | "true" or "false" |

**Total Fields:** 4 fields (all mappable, no nested structures)

---

## 4. Operation Response Analysis (Step 1c)

### Operation: Leave Oracle Fusion Create (6e8920fd-af5a-430b-a1d9-9fde7ac29a12)

**Response Profile:** None (responseProfileType: NONE)  
**Response Type:** JSON (dataContentType: application/json)  
**Return Errors:** true  
**Return Responses:** true

**Response Header Mapping:**
- Header: Content-Encoding
- Target Property: DDP_RespHeader (dynamicdocument.DDP_RespHeader)

**Extracted Fields:**
- HTTP Status Code → meta.base.applicationstatuscode (track property)
- HTTP Status Message → meta.base.applicationstatusmessage (track property)
- Response Body → Current document (for mapping)
- Content-Encoding Header → dynamicdocument.DDP_RespHeader

**Consumers:**
1. **shape2 (Decision):** Checks meta.base.applicationstatuscode for "20*" pattern
2. **shape44 (Decision):** Checks dynamicdocument.DDP_RespHeader equals "gzip"
3. **shape39 (Document Properties):** Extracts meta.base.applicationstatusmessage to process.DPP_ErrorMessage
4. **shape46 (Document Properties):** Extracts meta.base.applicationstatusmessage to process.DPP_ErrorMessage

**Business Logic Implications:**
- HTTP operation MUST execute before decision shapes can evaluate response status
- Response status determines success path (TRUE: map response, FALSE: check compression and handle error)
- Error message extraction happens only on non-2xx responses

---

## 5. Map Analysis (Step 1d)

### Map Inventory

| Map ID | Map Name | From Profile | To Profile | Type |
|---|---|---|---|---|
| c426b4d6-2aff-450e-b43b-59956c4dbc96 | Leave Create Map | febfa3e1-f719-4ee8-ba57-cdae34137ab3 | a94fa205-c740-40a5-9fda-3d018611135a | Request transformation |
| e4fd3f59-edb5-43a1-aeae-143b600a064e | Oracle Fusion Leave Response Map | 316175c7-0e45-4869-9ac6-5f9d69882a62 | f4ca3a70-114a-4601-bad8-44a3eb20e2c0 | Success response |
| f46b845a-7d75-41b5-b0ad-c41a6a8e9b12 | Leave Error Map | 23d7a2e9-5cb0-4e9c-9e4b-3154834bad0d | f4ca3a70-114a-4601-bad8-44a3eb20e2c0 | Error response |

### Map 1: Leave Create Map (c426b4d6-2aff-450e-b43b-59956c4dbc96)

**Purpose:** Transform D365 request to Oracle Fusion HCM format

**Field Mappings:**

| Source Field | Source Path | Target Field | Target Path | Transformation |
|---|---|---|---|---|
| employeeNumber | Root/Object/employeeNumber | personNumber | Root/Object/personNumber | Direct mapping |
| absenceType | Root/Object/absenceType | absenceType | Root/Object/absenceType | Direct mapping |
| employer | Root/Object/employer | employer | Root/Object/employer | Direct mapping |
| startDate | Root/Object/startDate | startDate | Root/Object/startDate | Direct mapping |
| endDate | Root/Object/endDate | endDate | Root/Object/endDate | Direct mapping |
| absenceStatusCode | Root/Object/absenceStatusCode | absenceStatusCd | Root/Object/absenceStatusCd | Field name change |
| approvalStatusCode | Root/Object/approvalStatusCode | approvalStatusCd | Root/Object/approvalStatusCd | Field name change |
| startDateDuration | Root/Object/startDateDuration | startDateDuration | Root/Object/startDateDuration | Direct mapping |
| endDateDuration | Root/Object/endDateDuration | endDateDuration | Root/Object/endDateDuration | Direct mapping |

**Profile vs Map Comparison:**

| Profile Field Name | Map Field Name (ACTUAL) | Authority | Use in Request |
|---|---|---|---|
| absenceStatusCode | absenceStatusCd | ✅ MAP | absenceStatusCd |
| approvalStatusCode | approvalStatusCd | ✅ MAP | approvalStatusCd |
| employeeNumber | personNumber | ✅ MAP | personNumber |

**Scripting Functions:** None

**Static Values:** None

**CRITICAL RULE:** Map field names are AUTHORITATIVE for Oracle Fusion API request.

### Map 2: Oracle Fusion Leave Response Map (e4fd3f59-edb5-43a1-aeae-143b600a064e)

**Purpose:** Map Oracle Fusion success response to D365 response format

**Field Mappings:**

| Source Field | Source Path | Target Field | Target Path | Transformation |
|---|---|---|---|---|
| personAbsenceEntryId | Root/Object/personAbsenceEntryId | personAbsenceEntryId | leaveResponse/Object/personAbsenceEntryId | Direct mapping |

**Default Values:**

| Target Field | Default Value | Purpose |
|---|---|---|
| status | "success" | Indicates successful processing |
| message | "Data successfully sent to Oracle Fusion" | Success message |
| success | "true" | Boolean success indicator |

### Map 3: Leave Error Map (f46b845a-7d75-41b5-b0ad-c41a6a8e9b12)

**Purpose:** Map error details to D365 error response format

**Field Mappings:**

| Source Field | Source Type | Target Field | Target Path | Transformation |
|---|---|---|---|---|
| DPP_ErrorMessage | Process Property (function) | message | leaveResponse/Object/message | Get process property |

**Function Step 1: Get Dynamic Process Property**
- **Type:** PropertyGet
- **Input:** Property Name = "DPP_ErrorMessage"
- **Output:** Result (mapped to message field)
- **Purpose:** Extract error message from process property

**Default Values:**

| Target Field | Default Value | Purpose |
|---|---|---|
| status | "failure" | Indicates failed processing |
| success | "false" | Boolean failure indicator |

**CRITICAL RULE:** Error message is retrieved from process.DPP_ErrorMessage property, which is populated by:
- Try/Catch error handler (meta.base.catcherrorsmessage)
- HTTP operation error response (meta.base.applicationstatusmessage)

---

## 6. HTTP Status Codes and Return Path Responses (Step 1e)

### Return Path Inventory

| Return Shape ID | Return Label | HTTP Status Code | Path Type | Conditions |
|---|---|---|---|---|
| shape35 | Success Response | 200 | Success | HTTP status 20* AND no compression |
| shape36 | Error Response | 400 | Error | HTTP status NOT 20* AND Content-Encoding NOT gzip |
| shape43 | Error Response | 500 | Error | Try/Catch error |
| shape48 | Error Response | 400 | Error | HTTP status NOT 20* AND Content-Encoding = gzip |

### Return Path 1: Success Response (shape35)

**Return Shape ID:** shape35  
**Return Label:** "Success Response"  
**HTTP Status Code:** 200  
**Decision Conditions Leading to Return:**
- shape2 (Decision): meta.base.applicationstatuscode matches "20*" → TRUE path

**Populated Response Fields:**

| Field Name | Field Path | Source | Populated By |
|---|---|---|---|
| status | leaveResponse/Object/status | static (map default) | "success" |
| message | leaveResponse/Object/message | static (map default) | "Data successfully sent to Oracle Fusion" |
| personAbsenceEntryId | leaveResponse/Object/personAbsenceEntryId | operation_response | Oracle Fusion API response |
| success | leaveResponse/Object/success | static (map default) | "true" |

**Response JSON Example:**

```json
{
  "status": "success",
  "message": "Data successfully sent to Oracle Fusion",
  "personAbsenceEntryId": 300100551234567,
  "success": "true"
}
```

### Return Path 2: Error Response - Non-2xx Status, No Compression (shape36)

**Return Shape ID:** shape36  
**Return Label:** "Error Response"  
**HTTP Status Code:** 400  
**Decision Conditions Leading to Return:**
- shape2 (Decision): meta.base.applicationstatuscode NOT matches "20*" → FALSE path
- shape44 (Decision): dynamicdocument.DDP_RespHeader NOT equals "gzip" → FALSE path

**Populated Response Fields:**

| Field Name | Field Path | Source | Populated By |
|---|---|---|---|
| status | leaveResponse/Object/status | static (map default) | "failure" |
| message | leaveResponse/Object/message | process_property | process.DPP_ErrorMessage (from meta.base.applicationstatusmessage) |
| success | leaveResponse/Object/success | static (map default) | "false" |

**Response JSON Example:**

```json
{
  "status": "failure",
  "message": "Invalid absence type provided",
  "success": "false"
}
```

### Return Path 3: Error Response - Try/Catch (shape43)

**Return Shape ID:** shape43  
**Return Label:** "Error Response"  
**HTTP Status Code:** 500  
**Decision Conditions Leading to Return:**
- shape17 (Try/Catch): Exception caught in Try block → Catch path

**Populated Response Fields:**

| Field Name | Field Path | Source | Populated By |
|---|---|---|---|
| status | leaveResponse/Object/status | static (map default) | "failure" |
| message | leaveResponse/Object/message | process_property | process.DPP_ErrorMessage (from meta.base.catcherrorsmessage) |
| success | leaveResponse/Object/success | static (map default) | "false" |

**Response JSON Example:**

```json
{
  "status": "failure",
  "message": "Connection timeout to Oracle Fusion HCM",
  "success": "false"
}
```

### Return Path 4: Error Response - Compressed Error Response (shape48)

**Return Shape ID:** shape48  
**Return Label:** "Error Response"  
**HTTP Status Code:** 400  
**Decision Conditions Leading to Return:**
- shape2 (Decision): meta.base.applicationstatuscode NOT matches "20*" → FALSE path
- shape44 (Decision): dynamicdocument.DDP_RespHeader equals "gzip" → TRUE path
- shape45 (Data Process): Decompress gzip response

**Populated Response Fields:**

| Field Name | Field Path | Source | Populated By |
|---|---|---|---|
| status | leaveResponse/Object/status | static (map default) | "failure" |
| message | leaveResponse/Object/message | process_property | process.DPP_ErrorMessage (from decompressed response) |
| success | leaveResponse/Object/success | static (map default) | "false" |

**Response JSON Example:**

```json
{
  "status": "failure",
  "message": "Employee not found in Oracle Fusion HCM",
  "success": "false"
}
```

### Downstream Operations HTTP Status Codes

| Operation Name | Expected Success Codes | Error Codes | Error Handling |
|---|---|---|---|
| Leave Oracle Fusion Create | 200, 201, 202 | 400, 401, 404, 500, 503 | Return error response with message |

---

## 7. Process Properties Analysis (Steps 2-3)

### Property WRITES

| Property Name | Written By Shape(s) | Source | Purpose |
|---|---|---|---|
| process.DPP_Process_Name | shape38 | execution.Process Name | Store process name for error email |
| process.DPP_AtomName | shape38 | execution.Atom Name | Store atom name for error email |
| process.DPP_Payload | shape38 | current document | Store input payload for error email attachment |
| process.DPP_ExecutionID | shape38 | execution.Execution Id | Store execution ID for error email |
| process.DPP_File_Name | shape38 | concatenation (Process Name + Timestamp + ".txt") | Generate attachment filename |
| process.DPP_Subject | shape38 | concatenation (Atom Name + " (" + Process Name + " ) has errors to report") | Generate email subject |
| process.To_Email | shape38 | defined property (PP_HCM_LeaveCreate_Properties.To_Email) | Email recipient |
| process.DPP_HasAttachment | shape38 | defined property (PP_HCM_LeaveCreate_Properties.DPP_HasAttachment) | Attachment flag ("Y"/"N") |
| dynamicdocument.URL | shape8 | defined property (PP_HCM_LeaveCreate_Properties.Resource_Path) | Oracle Fusion API resource path |
| process.DPP_ErrorMessage | shape19 | meta.base.catcherrorsmessage | Error message from Try/Catch |
| process.DPP_ErrorMessage | shape39 | meta.base.applicationstatusmessage | Error message from HTTP response |
| process.DPP_ErrorMessage | shape46 | meta.base.applicationstatusmessage (current) | Error message from decompressed response |

### Property READS

| Property Name | Read By Shape(s) | Purpose |
|---|---|---|---|
| dynamicdocument.URL | shape33 (operation) | Oracle Fusion API endpoint URL |
| process.DPP_Process_Name | shape21 (subprocess) | Pass to email subprocess |
| process.DPP_AtomName | shape21 (subprocess) | Pass to email subprocess |
| process.DPP_Payload | shape21 (subprocess) | Pass to email subprocess |
| process.DPP_ExecutionID | shape21 (subprocess) | Pass to email subprocess |
| process.DPP_File_Name | shape21 (subprocess) | Pass to email subprocess |
| process.DPP_Subject | shape21 (subprocess) | Pass to email subprocess |
| process.To_Email | shape21 (subprocess) | Pass to email subprocess |
| process.DPP_HasAttachment | shape21 (subprocess) | Pass to email subprocess |
| process.DPP_ErrorMessage | shape21 (subprocess) | Pass to email subprocess |

### Defined Process Properties

#### PP_HCM_LeaveCreate_Properties (e22c04db-7dfa-4b1b-9d88-77365b0fdb18)

| Property Key | Property Name | Default Value | Purpose |
|---|---|---|---|
| c2d1a1e8-4bcb-435b-b1d2-bb10a209bcc0 | Resource_Path | "hcmRestApi/resources/11.13.18.05/absences" | Oracle Fusion HCM API path |
| 71c90f6e-4f86-49f3-8aef-bae6668c73f9 | To_Email | "BoomiIntegrationTeam@al-ghurair.com" | Error notification recipient |
| a717867b-67f4-4a72-be2d-c99c73309fdb | DPP_HasAttachment | "Y" | Include payload as attachment |

#### PP_Office365_Email (0feff13f-8a2c-438a-b3c7-1909e2a7f533)

| Property Key | Property Name | Default Value | Purpose |
|---|---|---|---|
| 804b6d40-eeee-4ac0-ab4b-94e1aca9f4b7 | From_Email | "Boomi.Dev.failures@al-ghurair.com" | Email sender address |
| d8fc7496-ec2c-4308-a4ca-e9f49c0c9500 | Environment | "DEV Failure :" | Environment prefix for email subject |

---

## 8. Data Dependency Graph (Step 4)

### Dependency Graph

```
shape38 (Input_details) 
  ├─→ WRITES: process.DPP_Process_Name, process.DPP_AtomName, process.DPP_Payload, 
  │            process.DPP_ExecutionID, process.DPP_File_Name, process.DPP_Subject,
  │            process.To_Email, process.DPP_HasAttachment
  │
  └─→ MUST EXECUTE BEFORE: shape21 (subprocess call - reads all these properties)

shape8 (set URL)
  ├─→ WRITES: dynamicdocument.URL
  │
  └─→ MUST EXECUTE BEFORE: shape33 (HTTP operation - reads URL)

shape33 (Leave Oracle Fusion Create)
  ├─→ WRITES: meta.base.applicationstatuscode, meta.base.applicationstatusmessage,
  │            dynamicdocument.DDP_RespHeader, current document (response)
  │
  └─→ MUST EXECUTE BEFORE: shape2 (decision - reads applicationstatuscode)
                           shape44 (decision - reads DDP_RespHeader)
                           shape39 (extract error - reads applicationstatusmessage)
                           shape46 (extract error - reads applicationstatusmessage)

shape19 (ErrorMsg - Try/Catch path)
  ├─→ WRITES: process.DPP_ErrorMessage
  │
  └─→ MUST EXECUTE BEFORE: shape21 (subprocess - reads DPP_ErrorMessage)

shape39 (error msg - HTTP error path)
  ├─→ WRITES: process.DPP_ErrorMessage
  │
  └─→ MUST EXECUTE BEFORE: shape40 (map - reads DPP_ErrorMessage via function)

shape46 (error msg - compressed error path)
  ├─→ WRITES: process.DPP_ErrorMessage
  │
  └─→ MUST EXECUTE BEFORE: shape47 (map - reads DPP_ErrorMessage via function)
```

### Dependency Chains

1. **Main Success Flow:**
   - shape38 (set properties) → shape8 (set URL) → shape33 (HTTP call) → shape2 (check status) → shape34 (map response) → shape35 (return)

2. **Try/Catch Error Flow:**
   - shape38 (set properties) → shape17 (try/catch) → shape20 (branch) → shape19 (extract error) → shape21 (email subprocess)

3. **HTTP Error Flow (uncompressed):**
   - shape33 (HTTP call) → shape2 (check status FALSE) → shape44 (check compression FALSE) → shape39 (extract error) → shape40 (map error) → shape36 (return)

4. **HTTP Error Flow (compressed):**
   - shape33 (HTTP call) → shape2 (check status FALSE) → shape44 (check compression TRUE) → shape45 (decompress) → shape46 (extract error) → shape47 (map error) → shape48 (return)

### Property Summary

**Properties that create dependencies:**
- dynamicdocument.URL (written by shape8, read by shape33)
- meta.base.applicationstatuscode (written by shape33, read by shape2)
- dynamicdocument.DDP_RespHeader (written by shape33, read by shape44)
- meta.base.applicationstatusmessage (written by shape33, read by shape39, shape46)
- meta.base.catcherrorsmessage (written by shape17, read by shape19)
- process.DPP_ErrorMessage (written by shape19/shape39/shape46, read by shape21/map functions)
- All execution properties (written by shape38, read by shape21 subprocess)

---

## 9. Control Flow Graph (Step 5)

### Control Flow Map

| From Shape | To Shape(s) | Identifier | Description |
|---|---|---|---|
| shape1 (start) | shape38 | default | Entry point |
| shape38 (Input_details) | shape17 | default | Set execution properties |
| shape17 (catcherrors) | shape29 | default (Try) | Try block |
| shape17 (catcherrors) | shape20 | error (Catch) | Catch block |
| shape29 (map) | shape8 | default | Transform request |
| shape8 (set URL) | shape49 | default | Set Oracle Fusion URL |
| shape49 (notify) | shape33 | default | Log request |
| shape33 (HTTP operation) | shape2 | default | Call Oracle Fusion API |
| shape2 (decision) | shape34 | true | HTTP 20x success |
| shape2 (decision) | shape44 | false | HTTP non-20x error |
| shape34 (map) | shape35 | default | Map success response |
| shape35 (returndocuments) | - | - | Success return (terminal) |
| shape44 (decision) | shape45 | true | Response is gzipped |
| shape44 (decision) | shape39 | false | Response not gzipped |
| shape45 (dataprocess) | shape46 | default | Decompress response |
| shape46 (error msg) | shape47 | default | Extract error from decompressed |
| shape47 (map) | shape48 | default | Map error response |
| shape48 (returndocuments) | - | - | Error return (terminal) |
| shape39 (error msg) | shape40 | default | Extract error message |
| shape40 (map) | shape36 | default | Map error response |
| shape36 (returndocuments) | - | - | Error return (terminal) |
| shape20 (branch) | shape19 | 1 | Branch path 1: Extract error |
| shape20 (branch) | shape41 | 2 | Branch path 2: Map error |
| shape19 (ErrorMsg) | shape21 | default | Set error property |
| shape21 (processcall) | - | - | Call email subprocess (terminal) |
| shape41 (map) | shape43 | default | Map error response |
| shape43 (returndocuments) | - | - | Error return (terminal) |

### Connection Summary

- **Total Shapes:** 23 shapes (main process)
- **Total Connections:** 22 connections
- **Shapes with Multiple Outgoing Connections:**
  - shape17 (Try/Catch): 2 paths (default, error)
  - shape2 (Decision): 2 paths (true, false)
  - shape44 (Decision): 2 paths (true, false)
  - shape20 (Branch): 2 paths (1, 2)

### Reverse Flow Mapping (Step 6)

**Convergence Points:** None identified (all paths lead to different terminal points)

**Incoming Connections:**

| Target Shape | Source Shape(s) | Connection Type |
|---|---|---|
| shape38 | shape1 | Single entry |
| shape17 | shape38 | Single entry |
| shape29 | shape17 | Try path |
| shape20 | shape17 | Catch path |
| shape35 | shape34 | Success terminal |
| shape36 | shape40 | Error terminal |
| shape43 | shape41 | Error terminal |
| shape48 | shape47 | Error terminal |

---

## 10. Decision Shape Analysis (Step 7)

### ✅ Decision data sources identified: YES
### ✅ Decision types classified: YES
### ✅ Execution order verified: YES
### ✅ All decision paths traced: YES
### ✅ Decision patterns identified: YES
### ✅ Paths traced to termination: YES

### Decision Inventory

#### Decision 1: shape2 - HTTP Status 20 check

**Shape ID:** shape2  
**Comparison:** wildcard  
**Value 1:** meta.base.applicationstatuscode (track property)  
**Value 2:** "20*" (static)

**Data Source:** TRACK_PROPERTY (from HTTP operation response)  
**Decision Type:** POST_OPERATION  
**Actual Execution Order:** Operation shape33 → Response → Decision shape2 → Route based on status

**TRUE Path:**
- **Destination:** shape34 (map success response)
- **Termination:** shape35 (Return Documents - Success Response)
- **Type:** Success path

**FALSE Path:**
- **Destination:** shape44 (check compression)
- **Termination:** shape36 or shape48 (Return Documents - Error Response)
- **Type:** Error path

**Pattern:** Error Check (Success vs Failure)  
**Convergence Point:** None (paths terminate separately)  
**Early Exit:** No (both paths return)

**Business Logic:**
- If HTTP status is 20x (200, 201, 202, etc.) → Success → Map Oracle Fusion response → Return success
- If HTTP status is NOT 20x (400, 401, 404, 500, etc.) → Error → Check compression → Extract error → Return failure

#### Decision 2: shape44 - Check Response Content Type

**Shape ID:** shape44  
**Comparison:** equals  
**Value 1:** dynamicdocument.DDP_RespHeader (track property)  
**Value 2:** "gzip" (static)

**Data Source:** TRACK_PROPERTY (from HTTP operation response header)  
**Decision Type:** POST_OPERATION  
**Actual Execution Order:** Operation shape33 → Response → Decision shape2 (FALSE) → Decision shape44 → Route based on compression

**TRUE Path:**
- **Destination:** shape45 (decompress gzip)
- **Termination:** shape48 (Return Documents - Error Response)
- **Type:** Error path with decompression

**FALSE Path:**
- **Destination:** shape39 (extract error message)
- **Termination:** shape36 (Return Documents - Error Response)
- **Type:** Error path without decompression

**Pattern:** Conditional Logic (Optional Processing)  
**Convergence Point:** None (paths terminate separately)  
**Early Exit:** No (both paths return)

**Business Logic:**
- If Content-Encoding header is "gzip" → Decompress response → Extract error → Return failure
- If Content-Encoding header is NOT "gzip" → Extract error directly → Return failure

### Decision Patterns Summary

1. **Error Check Pattern (shape2):** Determines success vs failure based on HTTP status code
2. **Conditional Processing Pattern (shape44):** Handles compressed vs uncompressed error responses

---

## 11. Branch Shape Analysis (Step 8)

### ✅ Classification completed: YES
### ✅ Assumption check: NO (analyzed dependencies)
### ✅ Properties extracted: YES
### ✅ Dependency graph built: YES
### ✅ Topological sort applied: N/A (parallel execution)

### Branch Shape: shape20 (Error Handling Branch)

**Shape ID:** shape20  
**Number of Paths:** 2  
**Location:** Try/Catch error path

#### Step 1: Properties Analysis

**Path 1 (shape19 → shape21):**
- **READS:** meta.base.catcherrorsmessage (track property)
- **WRITES:** process.DPP_ErrorMessage

**Path 2 (shape41 → shape43):**
- **READS:** process.DPP_ErrorMessage (via map function)
- **WRITES:** None (maps to response profile)

#### Step 2: Dependency Graph

```
Path 1 → Path 2
```

**Reasoning:** Path 2 reads process.DPP_ErrorMessage which Path 1 writes, therefore Path 2 depends on Path 1.

#### Step 3: Classification

**Classification:** PARALLEL (no API calls, but has data dependency)

**PROOF:** 
- Path 1 contains no API calls (only documentproperties and processcall)
- Path 2 contains no API calls (only map and returndocuments)
- However, Path 2 depends on Path 1 for process.DPP_ErrorMessage property

**Actual Execution:** SEQUENTIAL due to data dependency

#### Step 4: Topological Sort Order

**Execution Order:** Path 1 → Path 2

**Reasoning:** Path 1 must execute first to populate process.DPP_ErrorMessage, which Path 2 needs for error mapping.

#### Step 5: Path Termination

- **Path 1:** Terminates at shape21 (processcall - email subprocess)
- **Path 2:** Terminates at shape43 (returndocuments - Error Response)

#### Step 6: Convergence Points

**Convergence Points:** None (paths terminate independently)

#### Step 7: Execution Continuation

**Execution Continues From:** None (both paths are terminal)

#### Step 8: Complete Analysis

```
Branch: shape20
├─ Path 1: shape19 → shape21
│  ├─ Writes: process.DPP_ErrorMessage
│  └─ Terminates: Email subprocess call
│
└─ Path 2: shape41 → shape43
   ├─ Reads: process.DPP_ErrorMessage
   └─ Terminates: Error response return
```

**Note:** While this branch appears to have 2 paths, they execute in parallel conceptually but Path 2's map function reads the error message that Path 1 sets. In practice, both paths execute (email notification AND error response return).

---

## 12. Execution Order (Step 9)

### ✅ Business logic verified FIRST: YES
### ✅ Operation analysis complete: YES
### ✅ Business logic execution order identified: YES
### ✅ Data dependencies checked FIRST: YES
### ✅ Operation response analysis used: YES (Section 4)
### ✅ Decision analysis used: YES (Section 10)
### ✅ Dependency graph used: YES (Section 8)
### ✅ Branch analysis used: YES (Section 11)
### ✅ Property dependency verification: YES
### ✅ Topological sort applied: N/A (no sequential branches with API calls)

### Business Logic Flow (Step 0)

#### Operation: Create Leave Oracle Fusion OP (8f709c2b-e63f-4d5f-9374-2932ed70415d)

**Purpose:** Web Service Server entry point - receives leave request from D365  
**Outputs:** Input document (leave request JSON)  
**Dependent Operations:** None (entry point)  
**Business Flow:** Receives leave request → Triggers process execution

#### Operation: Leave Oracle Fusion Create (6e8920fd-af5a-430b-a1d9-9fde7ac29a12)

**Purpose:** HTTP POST to Oracle Fusion HCM API - creates absence record  
**Outputs:** 
- HTTP response body (absence entry details)
- meta.base.applicationstatuscode (HTTP status code)
- meta.base.applicationstatusmessage (HTTP status message)
- dynamicdocument.DDP_RespHeader (Content-Encoding header)

**Dependent Operations:** 
- shape2 (Decision) - checks HTTP status code
- shape44 (Decision) - checks Content-Encoding header
- shape34 (Map) - maps success response
- shape39/shape46 (Extract error) - extracts error message

**Business Flow:** 
1. Receive transformed leave request from map
2. POST to Oracle Fusion HCM API endpoint
3. Return HTTP response (success or error)
4. Response determines next action (success mapping or error handling)

**Operations that MUST execute first:** None (depends on input transformation)

**Operations that execute after:** 
- shape2 (Decision) - evaluates HTTP status code from this operation
- shape34 (Map) - processes success response from this operation
- shape39/shape46 (Extract error) - processes error response from this operation

### Execution Order List

Based on dependency graph, control flow, and business logic:

```
1. START (shape1)
   └─→ Entry point (Web Service Server Listen)

2. shape38 (Input_details)
   └─→ WRITES: All execution properties for error handling
   └─→ Purpose: Capture execution context for error notifications

3. TRY BLOCK START (shape17)
   
   4. shape29 (map - Leave Create Map)
      └─→ Transform D365 request to Oracle Fusion format
      └─→ Field transformations: employeeNumber→personNumber, etc.
   
   5. shape8 (set URL)
      └─→ WRITES: dynamicdocument.URL
      └─→ Purpose: Set Oracle Fusion API endpoint
   
   6. shape49 (notify)
      └─→ Log request payload (INFO level)
   
   7. shape33 (Leave Oracle Fusion Create) [DOWNSTREAM]
      └─→ READS: dynamicdocument.URL
      └─→ WRITES: meta.base.applicationstatuscode, meta.base.applicationstatusmessage, dynamicdocument.DDP_RespHeader
      └─→ HTTP: [Expected: 200/201/202, Error: 400/401/404/500/503]
      └─→ Purpose: POST leave request to Oracle Fusion HCM API
   
   8. shape2 (Decision: HTTP Status 20 check)
      └─→ READS: meta.base.applicationstatuscode
      └─→ Condition: applicationstatuscode matches "20*"?
      
      IF TRUE (HTTP 20x success):
         9a. shape34 (map - Oracle Fusion Leave Response Map)
             └─→ Map Oracle Fusion response to D365 response format
             └─→ Extract: personAbsenceEntryId
             └─→ Set defaults: status="success", message="Data successfully sent to Oracle Fusion", success="true"
         
         10a. shape35 (Return Documents) [SUCCESS] [HTTP: 200]
              └─→ Response: { status: "success", message: "...", personAbsenceEntryId: 123, success: "true" }
      
      IF FALSE (HTTP non-20x error):
         9b. shape44 (Decision: Check Response Content Type)
             └─→ READS: dynamicdocument.DDP_RespHeader
             └─→ Condition: DDP_RespHeader equals "gzip"?
             
             IF TRUE (Response is gzipped):
                10b. shape45 (dataprocess - Decompress)
                     └─→ Groovy script: Decompress gzip response using GZIPInputStream
                
                11b. shape46 (error msg)
                     └─→ WRITES: process.DPP_ErrorMessage
                     └─→ READS: meta.base.applicationstatusmessage (from decompressed response)
                
                12b. shape47 (map - Leave Error Map)
                     └─→ READS: process.DPP_ErrorMessage (via function)
                     └─→ Set: status="failure", message=DPP_ErrorMessage, success="false"
                
                13b. shape48 (Return Documents) [ERROR] [HTTP: 400]
                     └─→ Response: { status: "failure", message: "...", success: "false" }
             
             IF FALSE (Response not gzipped):
                10c. shape39 (error msg)
                     └─→ WRITES: process.DPP_ErrorMessage
                     └─→ READS: meta.base.applicationstatusmessage
                
                11c. shape40 (map - Leave Error Map)
                     └─→ READS: process.DPP_ErrorMessage (via function)
                     └─→ Set: status="failure", message=DPP_ErrorMessage, success="false"
                
                12c. shape36 (Return Documents) [ERROR] [HTTP: 400]
                     └─→ Response: { status: "failure", message: "...", success: "false" }

CATCH BLOCK (if exception in Try block):
   
   9d. shape20 (branch - Error Handling)
       └─→ 2 parallel paths (both execute)
       
       Path 1:
          10d. shape19 (ErrorMsg)
               └─→ WRITES: process.DPP_ErrorMessage
               └─→ READS: meta.base.catcherrorsmessage
          
          11d. shape21 (processcall - Email Subprocess)
               └─→ READS: All execution properties
               └─→ Purpose: Send error notification email with payload attachment
               └─→ Subprocess: (Sub) Office 365 Email
       
       Path 2:
          10e. shape41 (map - Leave Error Map)
               └─→ READS: process.DPP_ErrorMessage (via function)
               └─→ Set: status="failure", message=DPP_ErrorMessage, success="false"
          
          11e. shape43 (Return Documents) [ERROR] [HTTP: 500]
               └─→ Response: { status: "failure", message: "...", success: "false" }
```

### Dependency Verification

**All property reads happen after property writes:**

1. ✅ dynamicdocument.URL: Written by shape8 → Read by shape33
2. ✅ meta.base.applicationstatuscode: Written by shape33 → Read by shape2
3. ✅ dynamicdocument.DDP_RespHeader: Written by shape33 → Read by shape44
4. ✅ meta.base.applicationstatusmessage: Written by shape33 → Read by shape39, shape46
5. ✅ meta.base.catcherrorsmessage: Written by shape17 (catch) → Read by shape19
6. ✅ process.DPP_ErrorMessage: Written by shape19/shape39/shape46 → Read by map functions
7. ✅ All execution properties: Written by shape38 → Read by shape21 (subprocess)

### Branch Execution Order

**Branch shape20 (Error Handling):**
- **Classification:** Parallel execution (no API calls)
- **Actual Execution:** Both paths execute simultaneously
  - Path 1: Send error notification email
  - Path 2: Return error response to caller

---

## 13. Sequence Diagram (Step 10)

**📋 NOTE:** This diagram is based on:
- **Dependency graph** in Section 8
- **Decision analysis** in Section 10
- **Control flow graph** in Section 9
- **Branch analysis** in Section 11
- **Execution order** in Section 12

**📋 NOTE:** Detailed request/response JSON examples are documented in Section 6 (HTTP Status Codes and Return Path Responses).

```
START (Web Service Server - Listen for D365 Leave Request)
 |
 ├─→ shape38: Input_details (Document Properties)
 |   └─→ WRITES: [process.DPP_Process_Name, process.DPP_AtomName, process.DPP_Payload, 
 |                 process.DPP_ExecutionID, process.DPP_File_Name, process.DPP_Subject,
 |                 process.To_Email, process.DPP_HasAttachment]
 |   └─→ Purpose: Capture execution context for error handling
 |
 ├─→ TRY BLOCK (shape17: catcherrors)
 |   |
 |   ├─→ shape29: Leave Create Map
 |   |   └─→ Transform: D365 format → Oracle Fusion format
 |   |   └─→ Mappings: employeeNumber→personNumber, absenceStatusCode→absenceStatusCd, etc.
 |   |
 |   ├─→ shape8: set URL (Document Properties)
 |   |   └─→ WRITES: [dynamicdocument.URL]
 |   |   └─→ Value: "hcmRestApi/resources/11.13.18.05/absences"
 |   |
 |   ├─→ shape49: notify (Log request)
 |   |   └─→ Log level: INFO
 |   |   └─→ Message: Request payload
 |   |
 |   ├─→ shape33: Leave Oracle Fusion Create (Downstream)
 |   |   └─→ READS: [dynamicdocument.URL]
 |   |   └─→ WRITES: [meta.base.applicationstatuscode, meta.base.applicationstatusmessage, 
 |   |                dynamicdocument.DDP_RespHeader, current document (response)]
 |   |   └─→ HTTP: POST to Oracle Fusion HCM API
 |   |   └─→ Expected: 200/201/202, Error: 400/401/404/500/503
 |   |   └─→ Connection: Oracle Fusion (Basic Auth)
 |   |   └─→ Request: { personNumber, absenceType, employer, startDate, endDate, 
 |   |                  absenceStatusCd, approvalStatusCd, startDateDuration, endDateDuration }
 |   |
 |   ├─→ shape2: Decision - HTTP Status 20 check
 |   |   └─→ READS: [meta.base.applicationstatuscode]
 |   |   └─→ Condition: applicationstatuscode matches "20*"?
 |   |   |
 |   |   ├─→ IF TRUE (HTTP 20x success):
 |   |   |   |
 |   |   |   ├─→ shape34: Oracle Fusion Leave Response Map
 |   |   |   |   └─→ READS: [current document (Oracle Fusion response)]
 |   |   |   |   └─→ Mapping: personAbsenceEntryId → personAbsenceEntryId
 |   |   |   |   └─→ Defaults: status="success", message="Data successfully sent to Oracle Fusion", success="true"
 |   |   |   |
 |   |   |   └─→ shape35: Return Documents [HTTP: 200] [SUCCESS]
 |   |   |       └─→ Response: { status: "success", message: "Data successfully sent to Oracle Fusion", 
 |   |   |                      personAbsenceEntryId: 300100551234567, success: "true" }
 |   |   |
 |   |   └─→ IF FALSE (HTTP non-20x error):
 |   |       |
 |   |       ├─→ shape44: Decision - Check Response Content Type
 |   |           └─→ READS: [dynamicdocument.DDP_RespHeader]
 |   |           └─→ Condition: DDP_RespHeader equals "gzip"?
 |   |           |
 |   |           ├─→ IF TRUE (Response is gzipped):
 |   |           |   |
 |   |           |   ├─→ shape45: Decompress (Data Process)
 |   |           |   |   └─→ Groovy Script: GZIPInputStream decompression
 |   |           |   |
 |   |           |   ├─→ shape46: error msg (Document Properties)
 |   |           |   |   └─→ WRITES: [process.DPP_ErrorMessage]
 |   |           |   |   └─→ READS: [meta.base.applicationstatusmessage (from decompressed response)]
 |   |           |   |
 |   |           |   ├─→ shape47: Leave Error Map
 |   |           |   |   └─→ READS: [process.DPP_ErrorMessage (via PropertyGet function)]
 |   |           |   |   └─→ Defaults: status="failure", success="false"
 |   |           |   |   └─→ Mapping: DPP_ErrorMessage → message
 |   |           |   |
 |   |           |   └─→ shape48: Return Documents [HTTP: 400] [ERROR]
 |   |           |       └─→ Response: { status: "failure", message: "Employee not found in Oracle Fusion HCM", 
 |   |           |                      success: "false" }
 |   |           |
 |   |           └─→ IF FALSE (Response not gzipped):
 |   |               |
 |   |               ├─→ shape39: error msg (Document Properties)
 |   |               |   └─→ WRITES: [process.DPP_ErrorMessage]
 |   |               |   └─→ READS: [meta.base.applicationstatusmessage]
 |   |               |
 |   |               ├─→ shape40: Leave Error Map
 |   |               |   └─→ READS: [process.DPP_ErrorMessage (via PropertyGet function)]
 |   |               |   └─→ Defaults: status="failure", success="false"
 |   |               |   └─→ Mapping: DPP_ErrorMessage → message
 |   |               |
 |   |               └─→ shape36: Return Documents [HTTP: 400] [ERROR]
 |   |                   └─→ Response: { status: "failure", message: "Invalid absence type provided", 
 |   |                                  success: "false" }
 |
 └─→ CATCH BLOCK (if exception in Try block):
     |
     ├─→ shape20: branch (Error Handling)
         └─→ 2 parallel paths (both execute):
         |
         ├─→ Path 1: Error Notification
         |   |
         |   ├─→ shape19: ErrorMsg (Document Properties)
         |   |   └─→ WRITES: [process.DPP_ErrorMessage]
         |   |   └─→ READS: [meta.base.catcherrorsmessage]
         |   |
         |   └─→ shape21: processcall - (Sub) Office 365 Email
         |       └─→ READS: [process.DPP_Process_Name, process.DPP_AtomName, process.DPP_Payload,
         |                   process.DPP_ExecutionID, process.DPP_File_Name, process.DPP_Subject,
         |                   process.To_Email, process.DPP_HasAttachment, process.DPP_ErrorMessage]
         |       └─→ Purpose: Send error notification email with payload attachment
         |       └─→ Subprocess: See Section 14 for internal flow
         |
         └─→ Path 2: Error Response
             |
             ├─→ shape41: Leave Error Map
             |   └─→ READS: [process.DPP_ErrorMessage (via PropertyGet function)]
             |   └─→ Defaults: status="failure", success="false"
             |   └─→ Mapping: DPP_ErrorMessage → message
             |
             └─→ shape43: Return Documents [HTTP: 500] [ERROR]
                 └─→ Response: { status: "failure", message: "Connection timeout to Oracle Fusion HCM", 
                                success: "false" }
```

**Note:** Detailed request/response JSON examples for all operations and return paths are documented in Section 6 (HTTP Status Codes and Return Path Responses).

---

## 14. Subprocess Analysis (Step 7a)

### Subprocess: (Sub) Office 365 Email (a85945c5-3004-42b9-80b1-104f465cd1fb)

**Subprocess ID:** a85945c5-3004-42b9-80b1-104f465cd1fb  
**Subprocess Name:** (Sub) Office 365 Email  
**Purpose:** Send error notification emails via Office 365 SMTP

### Internal Flow

```
START (Subprocess entry - receives error details from main process)
 |
 ├─→ shape2: catcherrors (Try/Catch)
 |   |
 |   ├─→ TRY BLOCK:
 |   |   |
 |   |   ├─→ shape4: Decision - Attachment_Check
 |   |       └─→ READS: [process.DPP_HasAttachment]
 |   |       └─→ Condition: DPP_HasAttachment equals "Y"?
 |   |       |
 |   |       ├─→ IF TRUE (Has attachment):
 |   |       |   |
 |   |       |   ├─→ shape11: Mail_Body (Message)
 |   |       |   |   └─→ Build HTML email body with execution details
 |   |       |   |   └─→ READS: [process.DPP_Process_Name, process.DPP_AtomName, 
 |   |       |   |                process.DPP_ExecutionID, process.DPP_ErrorMessage]
 |   |       |   |
 |   |       |   ├─→ shape14: set_MailBody (Document Properties)
 |   |       |   |   └─→ WRITES: [process.DPP_MailBody]
 |   |       |   |   └─→ READS: [current document (HTML email body)]
 |   |       |   |
 |   |       |   ├─→ shape15: payload (Message)
 |   |       |   |   └─→ READS: [process.DPP_Payload]
 |   |       |   |   └─→ Purpose: Restore original payload for attachment
 |   |       |   |
 |   |       |   ├─→ shape6: set_Mail_Properties (Document Properties)
 |   |       |   |   └─→ WRITES: [connector.mail.fromAddress, connector.mail.toAddress, 
 |   |       |   |                connector.mail.subject, connector.mail.body, connector.mail.filename]
 |   |       |   |   └─→ READS: [defined properties, process properties]
 |   |       |   |
 |   |       |   ├─→ shape3: Email (Connector Action - Email w Attachment)
 |   |       |   |   └─→ READS: [connector.mail.* properties]
 |   |       |   |   └─→ Connection: Office 365 Email (SMTP)
 |   |       |   |   └─→ Operation: Send email with attachment
 |   |       |   |
 |   |       |   └─→ shape5: Stop (continue=true) [SUCCESS RETURN]
 |   |       |
 |   |       └─→ IF FALSE (No attachment):
 |   |           |
 |   |           ├─→ shape23: Mail_Body (Message)
 |   |           |   └─→ Build HTML email body with execution details
 |   |           |   └─→ READS: [process.DPP_Process_Name, process.DPP_AtomName, 
 |   |           |                process.DPP_ExecutionID, process.DPP_ErrorMessage]
 |   |           |
 |   |           ├─→ shape22: set_MailBody (Document Properties)
 |   |           |   └─→ WRITES: [process.DPP_MailBody]
 |   |           |   └─→ READS: [current document (HTML email body)]
 |   |           |
 |   |           ├─→ shape20: set_Mail_Properties (Document Properties)
 |   |           |   └─→ WRITES: [connector.mail.fromAddress, connector.mail.toAddress, 
 |   |           |                connector.mail.subject, connector.mail.body]
 |   |           |   └─→ READS: [defined properties, process properties]
 |   |           |
 |   |           ├─→ shape7: Email (Connector Action - Email W/O Attachment)
 |   |           |   └─→ READS: [connector.mail.* properties]
 |   |           |   └─→ Connection: Office 365 Email (SMTP)
 |   |           |   └─→ Operation: Send email without attachment
 |   |           |
 |   |           └─→ shape9: Stop (continue=true) [SUCCESS RETURN]
 |   |
 |   └─→ CATCH BLOCK (if exception in Try block):
 |       |
 |       └─→ shape10: exception
 |           └─→ READS: [meta.base.catcherrorsmessage]
 |           └─→ Throw exception with error message
 |           └─→ [ERROR RETURN]
```

### Return Paths

| Return Label | Return Type | Condition | Main Process Mapping |
|---|---|---|---|
| SUCCESS | Stop (continue=true) | Email sent successfully (with or without attachment) | Continue execution (subprocess completes) |
| ERROR | Exception | Email sending failed | Exception propagated to main process |

### Properties Written by Subprocess

| Property Name | Written By | Purpose |
|---|---|---|
| process.DPP_MailBody | shape14, shape22 | Store HTML email body |
| connector.mail.fromAddress | shape6, shape20 | Set email sender |
| connector.mail.toAddress | shape6, shape20 | Set email recipient |
| connector.mail.subject | shape6, shape20 | Set email subject |
| connector.mail.body | shape6, shape20 | Set email body content |
| connector.mail.filename | shape6 | Set attachment filename (only with attachment) |

### Properties Read by Subprocess (from Main Process)

| Property Name | Read By | Purpose |
|---|---|---|
| process.DPP_HasAttachment | shape4 | Determine if attachment should be included |
| process.DPP_Process_Name | shape11, shape23 | Include in email body |
| process.DPP_AtomName | shape11, shape23 | Include in email body |
| process.DPP_ExecutionID | shape11, shape23 | Include in email body |
| process.DPP_ErrorMessage | shape11, shape23 | Include error details in email body |
| process.DPP_Payload | shape15 | Attach original payload to email |
| process.DPP_File_Name | shape6 | Set attachment filename |
| process.DPP_Subject | shape6, shape20 | Set email subject |
| process.To_Email | shape6, shape20 | Set email recipient |

### Subprocess Operations

#### Operation: Email w Attachment (af07502a-fafd-4976-a691-45d51a33b549)

**Type:** Mail Send  
**Body Content Type:** text/html  
**Data Content Type:** text/plain  
**Disposition:** attachment  
**Purpose:** Send HTML email with payload as text attachment

#### Operation: Email W/O Attachment (15a72a21-9b57-49a1-a8ed-d70367146644)

**Type:** Mail Send  
**Body Content Type:** text/plain  
**Data Content Type:** text/html  
**Disposition:** inline  
**Purpose:** Send HTML email without attachment

---

## 15. Critical Patterns Identified

### Pattern 1: Try/Catch Error Handling

**Identification:**
- Try/Catch wrapper (shape17) around entire business logic
- Catch path branches to dual error handling (email notification + error response)

**Execution Rule:**
- Try block executes main business logic (transform → HTTP call → response handling)
- If any exception occurs → Catch block executes
- Catch block performs dual actions:
  - Path 1: Send error notification email with payload attachment
  - Path 2: Return error response to caller

**Implementation:**
```
TRY:
  - Transform request
  - Call Oracle Fusion API
  - Handle response (success or error)
CATCH:
  - Extract error message
  - Send email notification (async)
  - Return error response (sync)
```

### Pattern 2: HTTP Status Code Decision Tree

**Identification:**
- Decision shape2 checks HTTP status code pattern "20*"
- TRUE path → Success response mapping
- FALSE path → Error handling with compression check

**Execution Rule:**
- HTTP operation MUST execute before decision
- Decision evaluates response status code
- Success path (20x) → Map Oracle Fusion response → Return success
- Error path (non-20x) → Check compression → Extract error → Return failure

**Implementation:**
```
HTTP Call → Check Status Code
  ├─→ 20x: Map success response → Return 200
  └─→ Non-20x: Check compression
      ├─→ gzip: Decompress → Extract error → Return 400
      └─→ no gzip: Extract error → Return 400
```

### Pattern 3: Conditional Compression Handling

**Identification:**
- Decision shape44 checks Content-Encoding header for "gzip"
- TRUE path → Decompress response using Groovy script → Extract error
- FALSE path → Extract error directly

**Execution Rule:**
- Only executed on HTTP error responses (non-20x)
- Handles Oracle Fusion API's conditional gzip compression
- Decompression uses GZIPInputStream in Groovy

**Implementation:**
```groovy
// Groovy decompression script (shape45)
import java.util.zip.GZIPInputStream
0.upto(dataContext.getDataCount()-1) {
    dataContext.storeStream(new GZIPInputStream(dataContext.getStream(it)), dataContext.getProperties(it))
}
```

### Pattern 4: Dual Error Response (Email + Return)

**Identification:**
- Branch shape20 in Catch block with 2 parallel paths
- Path 1: Email notification subprocess
- Path 2: Error response return

**Execution Rule:**
- Both paths execute in parallel
- Path 1 sends email notification asynchronously
- Path 2 returns error response synchronously
- Ensures both notification and response occur

**Implementation:**
```
CATCH:
  ├─→ Path 1: Extract error → Call email subprocess → Notify team
  └─→ Path 2: Map error → Return error response → Notify caller
```

### Pattern 5: Property-Based Configuration

**Identification:**
- Defined process properties for environment-specific configuration
- Properties set at deployment time (Resource_Path, To_Email, etc.)
- Properties read at runtime for dynamic behavior

**Execution Rule:**
- Configuration properties loaded from defined property components
- Runtime properties set from execution context
- Enables environment-specific behavior without code changes

**Implementation:**
```
Defined Properties:
  - Resource_Path: Oracle Fusion API endpoint
  - To_Email: Error notification recipient
  - DPP_HasAttachment: Include payload in email

Runtime Properties:
  - DPP_Process_Name: Current process name
  - DPP_ExecutionID: Current execution ID
  - DPP_ErrorMessage: Error message for response
```

---

## 16. System Layer Identification

### Third-Party Systems

#### System 1: Oracle Fusion HCM Cloud

**System Type:** Cloud-based Human Capital Management (HCM) system  
**Connection:** Oracle Fusion (aa1fcb29-d146-4425-9ea6-b9698090f60e)  
**Protocol:** HTTPS with Basic Authentication  
**Base URL:** https://iaaxey-dev3.fa.ocs.oraclecloud.com:443  
**API Endpoint:** /hcmRestApi/resources/11.13.18.05/absences  
**Authentication:** Basic Auth (INTEGRATION.USER@al-ghurair.com)

**Operations:**
- **POST /absences:** Create absence record (leave request)

**Request Format:**
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

**Response Format (Success):**
```json
{
  "personAbsenceEntryId": 300100551234567,
  "absenceStatusCd": "SUBMITTED",
  "approvalStatusCd": "APPROVED",
  // ... additional fields ...
}
```

**Response Format (Error):**
```json
{
  "title": "Invalid Request",
  "detail": "Employee not found in Oracle Fusion HCM",
  "status": 404
}
```

**Expected Status Codes:**
- Success: 200, 201, 202
- Error: 400 (Bad Request), 401 (Unauthorized), 404 (Not Found), 500 (Internal Server Error), 503 (Service Unavailable)

**Error Handling:**
- Non-20x responses trigger error path
- Error message extracted from response body
- Compressed responses (gzip) are decompressed before error extraction

#### System 2: Office 365 Email (SMTP)

**System Type:** Email notification system  
**Connection:** Office 365 Email (00eae79b-2303-4215-8067-dcc299e42697)  
**Protocol:** SMTP with TLS  
**Host:** smtp-mail.outlook.com  
**Port:** 587  
**Authentication:** SMTP AUTH (Boomi.Dev.failures@al-ghurair.com)

**Operations:**
- **Send Email with Attachment:** Send error notification with payload
- **Send Email without Attachment:** Send error notification without payload

**Email Format:**
- **From:** Boomi.Dev.failures@al-ghurair.com
- **To:** BoomiIntegrationTeam@al-ghurair.com (configurable)
- **Subject:** [Environment] [Atom Name] ([Process Name]) has errors to report
- **Body:** HTML table with execution details (Process Name, Environment, Execution ID, Error Details)
- **Attachment:** Original payload as .txt file (optional)

**Expected Behavior:**
- Emails sent asynchronously (does not block main process)
- Failures in email sending throw exceptions (handled by subprocess Try/Catch)

### System Layer API Requirements

#### Oracle Fusion HCM System Layer API

**API Name:** OracleFusionHcmAbsenceApi  
**Purpose:** Create absence records in Oracle Fusion HCM  
**Layer:** System Layer

**Methods:**
- **CreateAbsence(request):** POST /absences
  - Input: AbsenceRequest DTO
  - Output: AbsenceResponse DTO
  - Handles: Authentication, error handling, response parsing

**DTOs:**
```csharp
// Request DTO
public class OracleFusionAbsenceRequest
{
    public int PersonNumber { get; set; }
    public string AbsenceType { get; set; }
    public string Employer { get; set; }
    public string StartDate { get; set; }
    public string EndDate { get; set; }
    public string AbsenceStatusCd { get; set; }
    public string ApprovalStatusCd { get; set; }
    public int StartDateDuration { get; set; }
    public int EndDateDuration { get; set; }
}

// Response DTO
public class OracleFusionAbsenceResponse
{
    public long PersonAbsenceEntryId { get; set; }
    public string AbsenceStatusCd { get; set; }
    public string ApprovalStatusCd { get; set; }
    // ... additional fields ...
}
```

#### Email Notification System Layer API

**API Name:** EmailNotificationApi  
**Purpose:** Send error notification emails  
**Layer:** System Layer (reusable)

**Methods:**
- **SendErrorNotification(details):** Send email with error details
  - Input: ErrorNotificationRequest DTO
  - Output: void (fire-and-forget)
  - Handles: SMTP connection, email formatting, attachment handling

**DTOs:**
```csharp
// Request DTO
public class ErrorNotificationRequest
{
    public string ProcessName { get; set; }
    public string Environment { get; set; }
    public string ExecutionId { get; set; }
    public string ErrorMessage { get; set; }
    public string Payload { get; set; }
    public bool IncludeAttachment { get; set; }
    public string ToEmail { get; set; }
}
```

---

## 17. Validation Checklist

### Data Dependencies
- [x] All property WRITES identified (Section 7)
- [x] All property READS identified (Section 7)
- [x] Dependency graph built (Section 8)
- [x] Execution order satisfies all dependencies (Section 12)

### Decision Analysis
- [x] ALL decision shapes inventoried (Section 10)
- [x] BOTH TRUE and FALSE paths traced to termination (Section 10)
- [x] Pattern type identified for each decision (Section 10)
- [x] Early exits identified and documented (Section 10)
- [x] Convergence points identified (Section 10)
- [x] Decision data source analysis documented (Section 10)
- [x] Decision type classification (PRE_FILTER vs POST_OPERATION) (Section 10)
- [x] Actual execution order verified (Section 10)

### Branch Analysis
- [x] Each branch classified as parallel or sequential (Section 11)
- [x] API calls checked in branch paths (Section 11)
- [x] Dependency order built using topological sort (Section 11)
- [x] Each path traced to terminal point (Section 11)
- [x] Convergence points identified (Section 11)
- [x] Execution continuation point determined (Section 11)

### Sequence Diagram
- [x] Format follows required structure (Section 13)
- [x] Each operation shows READS and WRITES (Section 13)
- [x] Decisions show both TRUE and FALSE paths (Section 13)
- [x] Sequence diagram matches control flow graph (Section 9)
- [x] Execution order matches dependency graph (Section 8)
- [x] References to prior analysis sections included (Section 13)

### Subprocess Analysis
- [x] ALL subprocesses analyzed (Section 14)
- [x] Internal flow traced (Section 14)
- [x] Return paths identified (Section 14)
- [x] Properties written by subprocess documented (Section 14)
- [x] Properties read by subprocess documented (Section 14)

### Input/Output Structure Analysis
- [x] Entry point operation identified (Section 2)
- [x] Request profile structure analyzed (Section 2)
- [x] Array vs single object detected (Section 2)
- [x] ALL request fields extracted (Section 2)
- [x] Request field mapping table generated (Section 2)
- [x] Response profile structure analyzed (Section 3)
- [x] ALL response fields extracted (Section 3)
- [x] Response field mapping table generated (Section 3)
- [x] Document processing behavior determined (Section 2)

### HTTP Status Codes and Return Path Responses
- [x] All return paths documented with HTTP status codes (Section 6)
- [x] Response JSON examples provided for each return path (Section 6)
- [x] Populated fields documented for each return path (Section 6)
- [x] Decision conditions leading to each return documented (Section 6)
- [x] Downstream operation HTTP status codes documented (Section 6)

### Map Analysis
- [x] ALL map files identified and loaded (Section 5)
- [x] Field mappings extracted from each map (Section 5)
- [x] Profile vs map field name discrepancies documented (Section 5)
- [x] Map field names marked as AUTHORITATIVE (Section 5)
- [x] Scripting functions analyzed (Section 5)
- [x] Static values identified and documented (Section 5)

### Operation Response Analysis
- [x] Operation response structures analyzed (Section 4)
- [x] Extracted fields documented (Section 4)
- [x] Data consumers identified (Section 4)
- [x] Business logic implications documented (Section 4)

---

## 18. Function Exposure Decision Table

### Purpose
This table determines which functions should be exposed as Azure Functions and which should be internal helper methods, preventing function explosion and ensuring proper API design.

### Decision Criteria
- **Expose as Azure Function:** External trigger, independent business capability, reusable across processes
- **Internal Helper Method:** Process-specific logic, called only within process, no external trigger

### Function Exposure Analysis

| Component | Component Type | Expose as Function? | Rationale | Implementation |
|---|---|---|---|---|
| HCM_Leave Create (Main Process) | Process | ✅ YES | External trigger from D365, independent business capability | **Azure Function:** `CreateLeaveFunction` (HTTP Trigger) |
| Leave Create Map | Map | ❌ NO | Internal transformation, called only within process | **Internal:** Helper method in CreateLeaveFunction |
| Oracle Fusion Leave Response Map | Map | ❌ NO | Internal transformation, called only within process | **Internal:** Helper method in CreateLeaveFunction |
| Leave Error Map | Map | ❌ NO | Internal transformation, called only within process | **Internal:** Helper method in CreateLeaveFunction |
| Leave Oracle Fusion Create (HTTP Operation) | Operation | ❌ NO | System Layer API call, not a function | **System Layer API:** OracleFusionHcmAbsenceApi.CreateAbsence() |
| (Sub) Office 365 Email | Subprocess | ❌ NO | Reusable utility, but not externally triggered | **System Layer API:** EmailNotificationApi.SendErrorNotification() |
| Email w Attachment | Operation | ❌ NO | System Layer API call, not a function | **System Layer API:** EmailNotificationApi (internal method) |
| Email W/O Attachment | Operation | ❌ NO | System Layer API call, not a function | **System Layer API:** EmailNotificationApi (internal method) |

### Function Exposure Summary

**Functions to Create:**
1. **CreateLeaveFunction (Process Layer)**
   - **Trigger:** HTTP (Web Service Server)
   - **Input:** D365 Leave Request JSON
   - **Output:** Leave Response JSON (success/failure)
   - **Purpose:** Receive leave request from D365, create absence in Oracle Fusion HCM, return response
   - **Route:** POST /api/hcm/leave/create

**System Layer APIs to Create:**
1. **OracleFusionHcmAbsenceApi (System Layer)**
   - **Purpose:** Interact with Oracle Fusion HCM API
   - **Methods:** CreateAbsence(request)
   - **Reusable:** Yes (can be used by other HCM processes)

2. **EmailNotificationApi (System Layer)**
   - **Purpose:** Send error notification emails
   - **Methods:** SendErrorNotification(details)
   - **Reusable:** Yes (can be used by any process for error notifications)

**Internal Helper Methods (within CreateLeaveFunction):**
- TransformD365ToOracleFusion(d365Request) → OracleFusionAbsenceRequest
- MapOracleFusionResponse(oracleResponse) → LeaveResponse
- MapErrorResponse(errorMessage) → LeaveResponse
- HandleCompressedResponse(compressedData) → string

### Function Explosion Prevention

**❌ AVOID:**
- Creating separate Azure Functions for each map
- Creating separate Azure Functions for each operation
- Creating separate Azure Functions for each subprocess

**✅ CORRECT:**
- Single Azure Function for entire process (CreateLeaveFunction)
- System Layer APIs for external system interactions
- Internal helper methods for transformations and mappings

### Architecture Summary

```
┌─────────────────────────────────────────────────────────────┐
│ PROCESS LAYER (Azure Functions)                            │
├─────────────────────────────────────────────────────────────┤
│ CreateLeaveFunction (HTTP Trigger)                          │
│   ├─ TransformD365ToOracleFusion() [Internal]              │
│   ├─ OracleFusionHcmAbsenceApi.CreateAbsence() [System]    │
│   ├─ MapOracleFusionResponse() [Internal]                  │
│   ├─ MapErrorResponse() [Internal]                         │
│   └─ EmailNotificationApi.SendErrorNotification() [System] │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│ SYSTEM LAYER (Reusable APIs)                               │
├─────────────────────────────────────────────────────────────┤
│ OracleFusionHcmAbsenceApi                                   │
│   └─ CreateAbsence(request) → response                     │
│                                                             │
│ EmailNotificationApi                                        │
│   └─ SendErrorNotification(details) → void                 │
└─────────────────────────────────────────────────────────────┘
```

---

## PHASE 1 COMPLETION SUMMARY

### ✅ All Mandatory Sections Complete

1. ✅ Operations Inventory (Section 1)
2. ✅ Input Structure Analysis - Step 1a (Section 2)
3. ✅ Response Structure Analysis - Step 1b (Section 3)
4. ✅ Operation Response Analysis - Step 1c (Section 4)
5. ✅ Map Analysis - Step 1d (Section 5)
6. ✅ HTTP Status Codes and Return Path Responses - Step 1e (Section 6)
7. ✅ Process Properties Analysis - Steps 2-3 (Section 7)
8. ✅ Data Dependency Graph - Step 4 (Section 8)
9. ✅ Control Flow Graph - Step 5 (Section 9)
10. ✅ Decision Shape Analysis - Step 7 (Section 10)
11. ✅ Branch Shape Analysis - Step 8 (Section 11)
12. ✅ Execution Order - Step 9 (Section 12)
13. ✅ Sequence Diagram - Step 10 (Section 13)
14. ✅ Subprocess Analysis - Step 7a (Section 14)
15. ✅ Critical Patterns Identified (Section 15)
16. ✅ System Layer Identification (Section 16)
17. ✅ Validation Checklist (Section 17)
18. ✅ Function Exposure Decision Table (Section 18)

### ✅ All Self-Check Questions Answered with YES

**Step 1a (Input Structure Analysis):**
- ✅ Request profile identified from entry operation
- ✅ Profile structure analyzed (JSON)
- ✅ Array vs single object detected (single object)
- ✅ ALL fields extracted
- ✅ Field mapping table generated

**Step 1b (Response Structure Analysis):**
- ✅ Response profile identified
- ✅ Response structure analyzed
- ✅ Response field mapping table generated

**Step 1c (Operation Response Analysis):**
- ✅ Operation response structures analyzed
- ✅ Extracted fields documented
- ✅ Data consumers identified
- ✅ Business logic implications documented

**Step 1d (Map Analysis):**
- ✅ ALL map files analyzed
- ✅ Field mappings extracted
- ✅ Profile vs map discrepancies documented
- ✅ Map field names marked as AUTHORITATIVE

**Step 1e (HTTP Status Codes and Return Paths):**
- ✅ All return paths documented with HTTP status codes
- ✅ Response JSON examples provided
- ✅ Populated fields documented
- ✅ Downstream operation HTTP codes documented

**Step 7 (Decision Analysis):**
- ✅ Decision data sources identified: YES
- ✅ Decision types classified: YES
- ✅ Execution order verified: YES
- ✅ All decision paths traced: YES
- ✅ Decision patterns identified: YES
- ✅ Paths traced to termination: YES

**Step 8 (Branch Analysis):**
- ✅ Classification completed: YES
- ✅ Assumption check: NO (analyzed dependencies)
- ✅ Properties extracted: YES
- ✅ Dependency graph built: YES
- ✅ Topological sort applied: N/A (parallel execution)

**Step 9 (Execution Order):**
- ✅ Business logic verified FIRST: YES
- ✅ Operation analysis complete: YES
- ✅ Business logic execution order identified: YES
- ✅ Data dependencies checked FIRST: YES
- ✅ Operation response analysis used: YES
- ✅ Decision analysis used: YES
- ✅ Dependency graph used: YES
- ✅ Branch analysis used: YES
- ✅ Property dependency verification: YES

### Process Summary

**Process Name:** HCM_Leave Create  
**Purpose:** Sync leave data between D365 and Oracle HCM  
**Entry Point:** Web Service Server (D365 leave request)  
**Main Operation:** HTTP POST to Oracle Fusion HCM API  
**Error Handling:** Try/Catch with dual error response (email notification + error return)  
**Response Handling:** Status code-based routing with compression handling  
**Subprocess:** Email notification for errors

**Key Characteristics:**
- Single document processing (no array splitting)
- HTTP status code-based decision tree
- Conditional gzip decompression
- Dual error handling (notification + response)
- Property-based configuration

**System Layer APIs Required:**
1. OracleFusionHcmAbsenceApi (Oracle Fusion HCM integration)
2. EmailNotificationApi (Error notification emails)

**Azure Functions to Create:**
1. CreateLeaveFunction (Process Layer - HTTP Trigger)

---

**PHASE 1 EXTRACTION COMPLETE - READY FOR PHASE 2 (CODE GENERATION)**
