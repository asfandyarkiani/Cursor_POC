# BOOMI EXTRACTION PHASE 1 - HCM Leave Create Process

**Process Name:** HCM_Leave Create  
**Process ID:** ca69f858-785f-4565-ba1f-b4edc6cca05b  
**Description:** This Process will sync the Leave data between D365 and Oracle HCM  
**Domain:** Human Resource Management (HCM)  
**Date Extracted:** 2026-02-12

---

## Table of Contents

1. [Operations Inventory](#1-operations-inventory)
2. [Process Properties Analysis](#2-process-properties-analysis)
3. [Input Structure Analysis (Step 1a)](#3-input-structure-analysis-step-1a)
4. [Response Structure Analysis (Step 1b)](#4-response-structure-analysis-step-1b)
5. [Operation Response Analysis (Step 1c)](#5-operation-response-analysis-step-1c)
6. [Map Analysis (Step 1d)](#6-map-analysis-step-1d)
7. [HTTP Status Codes and Return Path Responses (Step 1e)](#7-http-status-codes-and-return-path-responses-step-1e)
8. [Data Dependency Graph (Step 4)](#8-data-dependency-graph-step-4)
9. [Control Flow Graph (Step 5)](#9-control-flow-graph-step-5)
10. [Decision Shape Analysis (Step 7)](#10-decision-shape-analysis-step-7)
11. [Branch Shape Analysis (Step 8)](#11-branch-shape-analysis-step-8)
12. [Subprocess Analysis (Step 7a)](#12-subprocess-analysis-step-7a)
13. [Execution Order (Step 9)](#13-execution-order-step-9)
14. [Sequence Diagram (Step 10)](#14-sequence-diagram-step-10)
15. [System Layer Identification](#15-system-layer-identification)
16. [Critical Patterns Identified](#16-critical-patterns-identified)
17. [Function Exposure Decision Table](#17-function-exposure-decision-table)
18. [Validation Checklist](#18-validation-checklist)

---

## 1. Operations Inventory

### 1.1 Main Process Operations

| Operation ID | Operation Name | Type | Sub-Type | Purpose |
|---|---|---|---|---|
| 8f709c2b-e63f-4d5f-9374-2932ed70415d | Create Leave Oracle Fusion OP | connector-action | wss | Web Service Server - Entry point for incoming leave requests from D365 |
| 6e8920fd-af5a-430b-a1d9-9fde7ac29a12 | Leave Oracle Fusion Create | connector-action | http | HTTP POST to Oracle Fusion HCM to create leave/absence record |
| af07502a-fafd-4976-a691-45d51a33b549 | Email w Attachment | connector-action | mail | Send email with attachment (error notification) |
| 15a72a21-9b57-49a1-a8ed-d70367146644 | Email W/O Attachment | connector-action | mail | Send email without attachment (error notification) |

### 1.2 Subprocess Operations

**Subprocess:** (Sub) Office 365 Email (a85945c5-3004-42b9-80b1-104f465cd1fb)

| Operation ID | Operation Name | Type | Sub-Type | Purpose |
|---|---|---|---|---|
| af07502a-fafd-4976-a691-45d51a33b549 | Email w Attachment | connector-action | mail | Send email with attachment |
| 15a72a21-9b57-49a1-a8ed-d70367146644 | Email W/O Attachment | connector-action | mail | Send email without attachment |

### 1.3 Connections

| Connection ID | Connection Name | Type | URL/Host |
|---|---|---|---|
| aa1fcb29-d146-4425-9ea6-b9698090f60e | Oracle Fusion | http | https://iaaxey-dev3.fa.ocs.oraclecloud.com:443 |
| 00eae79b-2303-4215-8067-dcc299e42697 | Office 365 Email | mail | smtp-mail.outlook.com:587 |

### 1.4 Profiles

| Profile ID | Profile Name | Type | Purpose |
|---|---|---|---|
| febfa3e1-f719-4ee8-ba57-cdae34137ab3 | D365 Leave Create JSON Profile | profile.json | Request profile - Input from D365 |
| a94fa205-c740-40a5-9fda-3d018611135a | HCM Leave Create JSON Profile | profile.json | Oracle Fusion request payload |
| 316175c7-0e45-4869-9ac6-5f9d69882a62 | Oracle Fusion Leave Response JSON Profile | profile.json | Oracle Fusion response |
| f4ca3a70-114a-4601-bad8-44a3eb20e2c0 | Leave D365 Response | profile.json | Response profile - Output to D365 |
| 23d7a2e9-5cb0-4e9c-9e4b-3154834bad0d | Dummy FF Profile | profile.flatfile | Dummy flat file profile for error mapping |

### 1.5 Maps

| Map ID | Map Name | Type | From Profile | To Profile | Purpose |
|---|---|---|---|---|---|
| c426b4d6-2aff-450e-b43b-59956c4dbc96 | Leave Create Map | transform.map | febfa3e1 (D365) | a94fa205 (HCM) | Transform D365 request to Oracle Fusion format |
| e4fd3f59-edb5-43a1-aeae-143b600a064e | Oracle Fusion Leave Response Map | transform.map | 316175c7 (HCM Response) | f4ca3a70 (D365 Response) | Transform Oracle Fusion response to D365 format |
| f46b845a-7d75-41b5-b0ad-c41a6a8e9b12 | Leave Error Map | transform.map | 23d7a2e9 (Dummy) | f4ca3a70 (D365 Response) | Map error messages to D365 response format |

---

## 2. Process Properties Analysis

### 2.1 Property WRITES (Step 2)

| Property Name | Written By Shape(s) | Source | Purpose |
|---|---|---|---|
| process.DPP_Process_Name | shape38 | Execution Property (Process Name) | Store process name for logging/email |
| process.DPP_AtomName | shape38 | Execution Property (Atom Name) | Store atom name for logging/email |
| process.DPP_Payload | shape38 | Current Document | Store incoming payload for error attachment |
| process.DPP_ExecutionID | shape38 | Execution Property (Execution Id) | Store execution ID for tracking |
| process.DPP_File_Name | shape38 | Process Name + Timestamp + ".txt" | Generate filename for error attachment |
| process.DPP_Subject | shape38 | Atom Name + " (" + Process Name + " ) has errors to report" | Generate email subject |
| process.To_Email | shape38 | Defined Property (PP_HCM_LeaveCreate_Properties.To_Email) | Email recipient for errors |
| process.DPP_HasAttachment | shape38 | Defined Property (PP_HCM_LeaveCreate_Properties.DPP_HasAttachment) | Flag to determine if email has attachment |
| dynamicdocument.URL | shape8 | Defined Property (PP_HCM_LeaveCreate_Properties.Resource_Path) | Oracle Fusion API resource path |
| process.DPP_ErrorMessage | shape19 | Track Property (meta.base.catcherrorsmessage) | Error message from try/catch block |
| process.DPP_ErrorMessage | shape39 | Track Property (meta.base.applicationstatusmessage) | Error message from HTTP response (non-20x) |
| process.DPP_ErrorMessage | shape46 | Current Document (meta.base.applicationstatusmessage) | Error message from gzip decompression path |

### 2.2 Property READS (Step 3)

| Property Name | Read By Shape(s) | Purpose |
|---|---|---|---|
| dynamicdocument.URL | shape33 (operation 6e8920fd) | URL path element for HTTP POST to Oracle Fusion |
| process.DPP_Process_Name | shape11, shape23 (subprocess message shapes) | Display in error email body |
| process.DPP_AtomName | shape11, shape23 (subprocess message shapes) | Display in error email body |
| process.DPP_ExecutionID | shape11, shape23 (subprocess message shapes) | Display in error email body |
| process.DPP_ErrorMessage | shape11, shape23 (subprocess message shapes), map f46b845a | Display in error email body and response |
| process.To_Email | shape6, shape20 (subprocess) | Email recipient address |
| process.DPP_Subject | shape6, shape20 (subprocess) | Email subject line |
| process.DPP_MailBody | shape6, shape20 (subprocess) | Email body content |
| process.DPP_Payload | shape15 (subprocess message) | Attach payload to error email |
| process.DPP_File_Name | shape6 (subprocess) | Filename for email attachment |
| process.DPP_HasAttachment | shape4 (subprocess decision) | Determine email operation (with/without attachment) |

---

## 3. Input Structure Analysis (Step 1a)

### 3.1 Request Profile Structure

**Profile ID:** febfa3e1-f719-4ee8-ba57-cdae34137ab3  
**Profile Name:** D365 Leave Create JSON Profile  
**Profile Type:** profile.json  
**Root Structure:** Root/Object  
**Array Detection:** ❌ NO - Single object  
**Input Type:** singlejson

### 3.2 Input Format

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

### 3.3 Document Processing Behavior

- **Boomi Processing:** Single document per execution (inputType = "singlejson")
- **Azure Function Requirement:** Accept single leave request object
- **Implementation Pattern:** Process single leave request, return success/error response

### 3.4 Request Field Mapping

| Boomi Field Path | Boomi Field Name | Data Type | Required | Azure DTO Property | Notes |
|---|---|---|---|---|
| Root/Object/employeeNumber | employeeNumber | number | Yes | EmployeeNumber | Employee ID from D365 |
| Root/Object/absenceType | absenceType | character | Yes | AbsenceType | Type of leave (Sick Leave, Annual Leave, etc.) |
| Root/Object/employer | employer | character | Yes | Employer | Employer name |
| Root/Object/startDate | startDate | character | Yes | StartDate | Leave start date (YYYY-MM-DD) |
| Root/Object/endDate | endDate | character | Yes | EndDate | Leave end date (YYYY-MM-DD) |
| Root/Object/absenceStatusCode | absenceStatusCode | character | Yes | AbsenceStatusCode | Status code (SUBMITTED, APPROVED, etc.) |
| Root/Object/approvalStatusCode | approvalStatusCode | character | Yes | ApprovalStatusCode | Approval status code |
| Root/Object/startDateDuration | startDateDuration | number | Yes | StartDateDuration | Duration in days for start date |
| Root/Object/endDateDuration | endDateDuration | number | Yes | EndDateDuration | Duration in days for end date |

**Total Fields:** 9 fields (all flat, no nested structures)

---

## 4. Response Structure Analysis (Step 1b)

### 4.1 Response Profile Structure

**Profile ID:** f4ca3a70-114a-4601-bad8-44a3eb20e2c0  
**Profile Name:** Leave D365 Response  
**Profile Type:** profile.json  
**Root Structure:** leaveResponse/Object  
**Array Detection:** ❌ NO - Single object

### 4.2 Response Format

**Success Response:**
```json
{
  "status": "success",
  "message": "Data successfully sent to Oracle Fusion",
  "personAbsenceEntryId": 12345,
  "success": "true"
}
```

**Error Response:**
```json
{
  "status": "failure",
  "message": "<error message from process.DPP_ErrorMessage>",
  "personAbsenceEntryId": null,
  "success": "false"
}
```

### 4.3 Response Field Mapping

| Boomi Field Path | Boomi Field Name | Data Type | Azure DTO Property | Notes |
|---|---|---|---|---|
| leaveResponse/Object/status | status | character | Status | "success" or "failure" |
| leaveResponse/Object/message | message | character | Message | Success message or error details |
| leaveResponse/Object/personAbsenceEntryId | personAbsenceEntryId | number | PersonAbsenceEntryId | Oracle Fusion absence entry ID (success only) |
| leaveResponse/Object/success | success | character | Success | "true" or "false" |

**Total Fields:** 4 fields (all flat, no nested structures)

---

## 5. Operation Response Analysis (Step 1c)

### 5.1 Operation: Leave Oracle Fusion Create (6e8920fd-af5a-430b-a1d9-9fde7ac29a12)

**Operation Type:** HTTP POST  
**Response Profile ID:** NONE (responseProfileType = "NONE")  
**Response Handling:** Returns raw HTTP response

**Extracted Fields:**
- **Track Property:** `meta.base.applicationstatuscode` - HTTP status code
- **Track Property:** `meta.base.applicationstatusmessage` - HTTP response message
- **Document Property:** `dynamicdocument.DDP_RespHeader` - Content-Encoding header value

**Consumers:**
1. **shape2 (Decision)** - Checks HTTP status code (20* wildcard)
2. **shape44 (Decision)** - Checks Content-Encoding header for gzip
3. **shape39, shape46 (Document Properties)** - Extract error message from response

**Business Logic Implications:**
- HTTP POST to Oracle Fusion MUST execute FIRST
- Decision shapes check response status to determine success/error path
- Success path (20x status) → Map response to D365 format
- Error path (non-20x status) → Extract error message, send email notification

### 5.2 Data Flow Chain

```
Operation (shape33) → HTTP POST to Oracle Fusion
  ↓
  ├─ WRITES: meta.base.applicationstatuscode (HTTP status)
  ├─ WRITES: meta.base.applicationstatusmessage (HTTP message)
  └─ WRITES: dynamicdocument.DDP_RespHeader (Content-Encoding header)
  ↓
Decision (shape2) → READS: meta.base.applicationstatuscode
  ↓
  ├─ TRUE (20*) → Success path → Map response → Return success
  └─ FALSE (non-20*) → Error path → Decision (shape44) → Check gzip encoding
      ↓
      ├─ TRUE (gzip) → Decompress → Extract error → Map error → Return error
      └─ FALSE → Extract error → Map error → Return error
```

---

## 6. Map Analysis (Step 1d)

### 6.1 Map Inventory

| Map ID | Map Name | From Profile | To Profile | Type |
|---|---|---|---|
| c426b4d6 | Leave Create Map | febfa3e1 (D365) | a94fa205 (HCM) | Request transformation |
| e4fd3f59 | Oracle Fusion Leave Response Map | 316175c7 (HCM Response) | f4ca3a70 (D365 Response) | Success response transformation |
| f46b845a | Leave Error Map | 23d7a2e9 (Dummy) | f4ca3a70 (D365 Response) | Error response transformation |

### 6.2 Map: Leave Create Map (c426b4d6)

**Purpose:** Transform D365 leave request to Oracle Fusion HCM format

**Field Mappings:**

| Source Field (D365) | Target Field (HCM) | Transformation | Notes |
|---|---|---|---|
| employeeNumber | personNumber | Direct mapping | Employee ID |
| absenceType | absenceType | Direct mapping | Leave type |
| employer | employer | Direct mapping | Employer name |
| startDate | startDate | Direct mapping | Start date |
| endDate | endDate | Direct mapping | End date |
| absenceStatusCode | absenceStatusCd | Direct mapping | Status code (field name change) |
| approvalStatusCode | approvalStatusCd | Direct mapping | Approval status (field name change) |
| startDateDuration | startDateDuration | Direct mapping | Start duration |
| endDateDuration | endDateDuration | Direct mapping | End duration |

**Profile vs Map Field Name Comparison:**

| D365 Field | HCM Profile Field | Match? | Notes |
|---|---|---|---|
| absenceStatusCode | absenceStatusCd | ✅ Match | Consistent naming |
| approvalStatusCode | approvalStatusCd | ✅ Match | Consistent naming |

**Scripting Functions:** None

**Authority:** Map field names are AUTHORITATIVE for Oracle Fusion API request

### 6.3 Map: Oracle Fusion Leave Response Map (e4fd3f59)

**Purpose:** Transform Oracle Fusion response to D365 response format

**Field Mappings:**

| Source Field (HCM Response) | Target Field (D365 Response) | Transformation | Notes |
|---|---|---|---|
| personAbsenceEntryId | personAbsenceEntryId | Direct mapping | Absence entry ID from Oracle |
| (default) | status | Static value: "success" | Success indicator |
| (default) | message | Static value: "Data successfully sent to Oracle Fusion" | Success message |
| (default) | success | Static value: "true" | Boolean success flag |

**Default Values:**
- `status` = "success"
- `message` = "Data successfully sent to Oracle Fusion"
- `success` = "true"

### 6.4 Map: Leave Error Map (f46b845a)

**Purpose:** Transform error information to D365 response format

**Field Mappings:**

| Source Field | Target Field (D365 Response) | Transformation | Notes |
|---|---|---|---|
| process.DPP_ErrorMessage (function) | message | Get Process Property | Error message from catch block or HTTP response |
| (default) | status | Static value: "failure" | Failure indicator |
| (default) | success | Static value: "false" | Boolean failure flag |

**Default Values:**
- `status` = "failure"
- `success` = "false"

**Scripting Functions:**

| Function | Type | Input | Output | Logic |
|---|---|---|---|---|
| Function 1 | PropertyGet | Property Name: "DPP_ErrorMessage" | Result | Retrieve error message from process property |

---

## 7. HTTP Status Codes and Return Path Responses (Step 1e)

### 7.1 Return Path Inventory

| Return Label | Return Shape ID | HTTP Status Code | Decision Conditions | Response Type |
|---|---|---|---|---|
| Success Response | shape35 | 200 | HTTP status 20* (TRUE path) | Success |
| Error Response | shape36 | 400 | HTTP status non-20* (FALSE path) → gzip check (FALSE path) | Error |
| Error Response | shape43 | 400 | Try/Catch error (Catch path) | Error |
| Error Response | shape48 | 400 | HTTP status non-20* (FALSE path) → gzip check (TRUE path) → decompress | Error |

### 7.2 Return Path 1: Success Response (shape35)

**Return Label:** "Success Response"  
**Return Shape ID:** shape35  
**HTTP Status Code:** 200  
**Decision Conditions Leading to Return:**
- shape2 (Decision): meta.base.applicationstatuscode matches "20*" → TRUE path

**Populated Response Fields:**

| Field Name | Field Path | Source | Populated By |
|---|---|---|---|
| status | leaveResponse/Object/status | static (map default) | map e4fd3f59 |
| message | leaveResponse/Object/message | static (map default) | map e4fd3f59 |
| personAbsenceEntryId | leaveResponse/Object/personAbsenceEntryId | operation_response | Oracle Fusion response (shape33) |
| success | leaveResponse/Object/success | static (map default) | map e4fd3f59 |

**Response JSON Example:**
```json
{
  "status": "success",
  "message": "Data successfully sent to Oracle Fusion",
  "personAbsenceEntryId": 300100123456789,
  "success": "true"
}
```

### 7.3 Return Path 2: Error Response - Try/Catch (shape43)

**Return Label:** "Error Response"  
**Return Shape ID:** shape43  
**HTTP Status Code:** 400  
**Decision Conditions Leading to Return:**
- shape17 (Try/Catch): Error occurred in Try block → Catch path
- shape20 (Branch): Path 2

**Populated Response Fields:**

| Field Name | Field Path | Source | Populated By |
|---|---|---|---|
| status | leaveResponse/Object/status | static (map default) | map f46b845a |
| message | leaveResponse/Object/message | process_property | process.DPP_ErrorMessage (from meta.base.catcherrorsmessage) |
| personAbsenceEntryId | leaveResponse/Object/personAbsenceEntryId | null | Not populated |
| success | leaveResponse/Object/success | static (map default) | map f46b845a |

**Response JSON Example:**
```json
{
  "status": "failure",
  "message": "Connection timeout to Oracle Fusion HCM API",
  "personAbsenceEntryId": null,
  "success": "false"
}
```

### 7.4 Return Path 3: Error Response - HTTP Non-20x, No Gzip (shape36)

**Return Label:** "Error Response"  
**Return Shape ID:** shape36  
**HTTP Status Code:** 400  
**Decision Conditions Leading to Return:**
- shape2 (Decision): meta.base.applicationstatuscode does NOT match "20*" → FALSE path
- shape44 (Decision): dynamicdocument.DDP_RespHeader does NOT equal "gzip" → FALSE path

**Populated Response Fields:**

| Field Name | Field Path | Source | Populated By |
|---|---|---|---|
| status | leaveResponse/Object/status | static (map default) | map f46b845a |
| message | leaveResponse/Object/message | process_property | process.DPP_ErrorMessage (from meta.base.applicationstatusmessage) |
| personAbsenceEntryId | leaveResponse/Object/personAbsenceEntryId | null | Not populated |
| success | leaveResponse/Object/success | static (map default) | map f46b845a |

**Response JSON Example:**
```json
{
  "status": "failure",
  "message": "Invalid absence type provided",
  "personAbsenceEntryId": null,
  "success": "false"
}
```

### 7.5 Return Path 4: Error Response - HTTP Non-20x, Gzip Decompressed (shape48)

**Return Label:** "Error Response"  
**Return Shape ID:** shape48  
**HTTP Status Code:** 400  
**Decision Conditions Leading to Return:**
- shape2 (Decision): meta.base.applicationstatuscode does NOT match "20*" → FALSE path
- shape44 (Decision): dynamicdocument.DDP_RespHeader equals "gzip" → TRUE path
- shape45 (Data Process): Decompress gzip response

**Populated Response Fields:**

| Field Name | Field Path | Source | Populated By |
|---|---|---|---|
| status | leaveResponse/Object/status | static (map default) | map f46b845a |
| message | leaveResponse/Object/message | process_property | process.DPP_ErrorMessage (from decompressed response) |
| personAbsenceEntryId | leaveResponse/Object/personAbsenceEntryId | null | Not populated |
| success | leaveResponse/Object/success | static (map default) | map f46b845a |

**Response JSON Example:**
```json
{
  "status": "failure",
  "message": "Employee not found in Oracle HCM system",
  "personAbsenceEntryId": null,
  "success": "false"
}
```

### 7.6 Downstream Operations HTTP Status Codes

| Operation Name | Expected Success Codes | Error Codes | Error Handling |
|---|---|---|---|
| Leave Oracle Fusion Create (shape33) | 200, 201, 202 | 400, 401, 403, 404, 500, 503 | Check status code, extract error message, return error response |

---

## 8. Data Dependency Graph (Step 4)

### 8.1 Dependency Graph

**Property: dynamicdocument.URL**
- **WRITTEN BY:** shape8 (Document Properties)
- **READ BY:** shape33 (HTTP POST operation)
- **DEPENDENCY:** shape8 MUST execute BEFORE shape33

**Property: process.DPP_ErrorMessage**
- **WRITTEN BY:** shape19, shape39, shape46 (Document Properties - error paths)
- **READ BY:** shape11, shape23 (subprocess message shapes), map f46b845a
- **DEPENDENCY:** Error extraction shapes MUST execute BEFORE error email/response mapping

**Property: process.DPP_Process_Name, process.DPP_AtomName, process.DPP_ExecutionID, process.DPP_Payload, process.DPP_File_Name, process.DPP_Subject, process.To_Email, process.DPP_HasAttachment**
- **WRITTEN BY:** shape38 (Document Properties - Input_details)
- **READ BY:** shape11, shape23, shape4, shape6, shape15, shape20 (subprocess shapes)
- **DEPENDENCY:** shape38 MUST execute BEFORE subprocess call (shape21)

**Property: process.DPP_MailBody**
- **WRITTEN BY:** shape14, shape22 (subprocess - set_MailBody)
- **READ BY:** shape6, shape20 (subprocess - set_Mail_Properties)
- **DEPENDENCY:** Message shapes (shape11, shape23) → set_MailBody (shape14, shape22) → set_Mail_Properties (shape6, shape20) → Email operation (shape3, shape7)

### 8.2 Dependency Chains

**Chain 1: Main Process Flow**
```
shape38 (Input_details) → shape17 (Try/Catch) → shape29 (Map) → shape8 (set URL) → shape49 (Notify) → shape33 (HTTP POST) → shape2 (Decision)
```

**Chain 2: Success Path**
```
shape2 (TRUE) → shape34 (Map response) → shape35 (Return success)
```

**Chain 3: Error Path - Try/Catch**
```
shape17 (Catch) → shape20 (Branch) → [shape19 (ErrorMsg) → shape21 (Subprocess)] OR [shape41 (Map error) → shape43 (Return error)]
```

**Chain 4: Error Path - HTTP Non-20x**
```
shape2 (FALSE) → shape44 (Check gzip) → [shape39 (error msg) → shape40 (Map error) → shape36 (Return error)] OR [shape45 (Decompress) → shape46 (error msg) → shape47 (Map error) → shape48 (Return error)]
```

**Chain 5: Subprocess Email Flow**
```
shape21 (ProcessCall) → subprocess shape1 (Start) → shape2 (Try/Catch) → shape4 (Decision) → [shape11 (Mail_Body) → shape14 (set_MailBody) → shape15 (payload) → shape6 (set_Mail_Properties) → shape3 (Email w/ attachment)] OR [shape23 (Mail_Body) → shape22 (set_MailBody) → shape20 (set_Mail_Properties) → shape7 (Email w/o attachment)]
```

### 8.3 Property Summary

**Properties Creating Dependencies:**
- `dynamicdocument.URL` - Required for HTTP operation
- `process.DPP_ErrorMessage` - Required for error response mapping
- `process.DPP_Process_Name`, `process.DPP_AtomName`, `process.DPP_ExecutionID` - Required for error email
- `process.DPP_Payload`, `process.DPP_File_Name` - Required for email attachment
- `process.DPP_Subject`, `process.To_Email` - Required for email properties
- `process.DPP_HasAttachment` - Required for email operation selection
- `process.DPP_MailBody` - Required for email body content

---

## 9. Control Flow Graph (Step 5)

### 9.1 Main Process Control Flow

| From Shape | To Shape | Identifier | Description |
|---|---|---|---|
| shape1 (start) | shape38 | default | Start → Input_details |
| shape38 | shape17 | default | Input_details → Try/Catch |
| shape17 | shape29 | default (Try) | Try/Catch → Map (Try path) |
| shape17 | shape20 | error (Catch) | Try/Catch → Branch (Catch path) |
| shape29 | shape8 | default | Map → set URL |
| shape8 | shape49 | default | set URL → Notify |
| shape49 | shape33 | default | Notify → HTTP POST |
| shape33 | shape2 | default | HTTP POST → HTTP Status 20 check |
| shape2 | shape34 | true | Decision TRUE → Map response |
| shape2 | shape44 | false | Decision FALSE → Check Response Content Type |
| shape34 | shape35 | default | Map response → Return success |
| shape44 | shape45 | true | Gzip TRUE → Decompress |
| shape44 | shape39 | false | Gzip FALSE → error msg |
| shape45 | shape46 | default | Decompress → error msg |
| shape46 | shape47 | default | error msg → Map error |
| shape47 | shape48 | default | Map error → Return error |
| shape39 | shape40 | default | error msg → Map error |
| shape40 | shape36 | default | Map error → Return error |
| shape20 | shape19 | 1 (Branch path 1) | Branch → ErrorMsg |
| shape20 | shape41 | 2 (Branch path 2) | Branch → Map error |
| shape19 | shape21 | default | ErrorMsg → Subprocess (Email) |
| shape41 | shape43 | default | Map error → Return error |

**Total Shapes:** 19 main process shapes  
**Total Connections:** 21 dragpoint connections  
**Branches:** 1 (shape20 - 2 paths)  
**Decisions:** 2 (shape2, shape44)  
**Try/Catch:** 1 (shape17)

### 9.2 Subprocess Control Flow

**Subprocess:** (Sub) Office 365 Email (a85945c5-3004-42b9-80b1-104f465cd1fb)

| From Shape | To Shape | Identifier | Description |
|---|---|---|---|
| shape1 (start) | shape2 | default | Start → Try/Catch |
| shape2 | shape4 | default (Try) | Try/Catch → Attachment_Check |
| shape2 | shape10 | error (Catch) | Try/Catch → Exception |
| shape4 | shape11 | true | Attachment TRUE → Mail_Body (with attachment) |
| shape4 | shape23 | false | Attachment FALSE → Mail_Body (without attachment) |
| shape11 | shape14 | default | Mail_Body → set_MailBody |
| shape14 | shape15 | default | set_MailBody → payload |
| shape15 | shape6 | default | payload → set_Mail_Properties |
| shape6 | shape3 | default | set_Mail_Properties → Email w/ attachment |
| shape3 | shape5 | default | Email → Stop (continue=true) |
| shape23 | shape22 | default | Mail_Body → set_MailBody |
| shape22 | shape20 | default | set_MailBody → set_Mail_Properties |
| shape20 | shape7 | default | set_Mail_Properties → Email w/o attachment |
| shape7 | shape9 | default | Email → Stop (continue=true) |

**Total Shapes:** 14 subprocess shapes  
**Total Connections:** 14 dragpoint connections  
**Decisions:** 1 (shape4 - Attachment check)  
**Try/Catch:** 1 (shape2)

### 9.3 Reverse Flow Mapping (Step 6)

**Convergence Points (shapes reached by multiple paths):**

Main Process:
- **None** - No convergence points in main process (all paths lead to different return shapes)

Subprocess:
- **None** - No convergence points in subprocess (decision paths lead to different email operations)

---

## 10. Decision Shape Analysis (Step 7)

### 10.1 Self-Check Results

✅ **Decision data sources identified:** YES  
✅ **Decision types classified:** YES  
✅ **Execution order verified:** YES  
✅ **All decision paths traced:** YES  
✅ **Decision patterns identified:** YES  
✅ **Paths traced to termination:** YES

### 10.2 Decision Inventory

#### Decision 1: HTTP Status 20 check (shape2)

**Shape ID:** shape2  
**Comparison:** wildcard  
**Value 1:** meta.base.applicationstatuscode (Track Property)  
**Value 2:** "20*" (Static)

**Data Source:** TRACK_PROPERTY (HTTP response status code from operation)  
**Decision Type:** POST_OPERATION (checks response data from HTTP operation)  
**Actual Execution Order:** Operation (shape33) → HTTP Response → Decision (shape2) → Route based on status

**TRUE Path:**
- **Destination:** shape34 (Map response)
- **Termination:** shape35 (Return Documents - Success Response)
- **Type:** Success path

**FALSE Path:**
- **Destination:** shape44 (Check Response Content Type)
- **Termination:** shape36 or shape48 (Return Documents - Error Response)
- **Type:** Error path

**Pattern:** Error Check (Success vs Failure based on HTTP status)  
**Convergence Point:** None (paths lead to different returns)  
**Early Exit:** No (both paths return documents)

**Business Logic:**
- If HTTP status is 20x (200, 201, 202, etc.) → Success path → Map Oracle Fusion response to D365 format → Return success
- If HTTP status is non-20x (400, 401, 404, 500, etc.) → Error path → Check if response is gzip compressed → Extract error message → Return error

#### Decision 2: Check Response Content Type (shape44)

**Shape ID:** shape44  
**Comparison:** equals  
**Value 1:** dynamicdocument.DDP_RespHeader (Track Property - Content-Encoding header)  
**Value 2:** "gzip" (Static)

**Data Source:** TRACK_PROPERTY (HTTP response header from operation)  
**Decision Type:** POST_OPERATION (checks response header from HTTP operation)  
**Actual Execution Order:** Operation (shape33) → HTTP Response → Decision (shape2) → FALSE → Decision (shape44) → Route based on encoding

**TRUE Path:**
- **Destination:** shape45 (Data Process - Decompress gzip)
- **Termination:** shape48 (Return Documents - Error Response)
- **Type:** Error path with gzip decompression

**FALSE Path:**
- **Destination:** shape39 (Document Properties - error msg)
- **Termination:** shape36 (Return Documents - Error Response)
- **Type:** Error path without decompression

**Pattern:** Conditional Logic (Handle gzip-compressed error responses)  
**Convergence Point:** None (paths lead to different return shapes)  
**Early Exit:** No (both paths return documents)

**Business Logic:**
- If Content-Encoding header is "gzip" → Decompress response → Extract error message → Return error
- If Content-Encoding header is not "gzip" → Extract error message directly → Return error

#### Decision 3: Attachment_Check (subprocess shape4)

**Shape ID:** shape4 (subprocess)  
**Comparison:** equals  
**Value 1:** process.DPP_HasAttachment (Process Property)  
**Value 2:** "Y" (Static)

**Data Source:** PROCESS_PROPERTY (set by main process)  
**Decision Type:** PRE_FILTER (checks input property to route to appropriate email operation)  
**Actual Execution Order:** Decision (shape4) → Route to email operation (with or without attachment)

**TRUE Path:**
- **Destination:** shape11 (Message - Mail_Body with attachment)
- **Termination:** shape5 (Stop - continue=true)
- **Type:** Email with attachment path

**FALSE Path:**
- **Destination:** shape23 (Message - Mail_Body without attachment)
- **Termination:** shape9 (Stop - continue=true)
- **Type:** Email without attachment path

**Pattern:** Conditional Routing (Select email operation based on attachment flag)  
**Convergence Point:** None (paths lead to different email operations)  
**Early Exit:** No (both paths complete successfully)

**Business Logic:**
- If DPP_HasAttachment = "Y" → Build email with attachment (payload file) → Send email
- If DPP_HasAttachment ≠ "Y" → Build email without attachment → Send email

---

## 11. Branch Shape Analysis (Step 8)

### 11.1 Self-Check Results

✅ **Classification completed:** YES  
✅ **Assumption check:** NO (analyzed dependencies)  
✅ **Properties extracted:** YES  
✅ **Dependency graph built:** YES  
✅ **Topological sort applied:** YES (for sequential branch)

### 11.2 Branch Shape: Error Handling Branch (shape20)

**Shape ID:** shape20  
**Number of Paths:** 2  
**Location:** Try/Catch error path (Catch block)

#### Step 1: Properties Analysis

**Path 1 (shape20 → shape19 → shape21):**
- **READS:** None
- **WRITES:** process.DPP_ErrorMessage (shape19)

**Path 2 (shape20 → shape41 → shape43):**
- **READS:** process.DPP_ErrorMessage (map f46b845a via function)
- **WRITES:** None

#### Step 2: Dependency Graph

```
Path 1 → Path 2
```

**Reasoning:** Path 1 writes `process.DPP_ErrorMessage` which Path 2 reads (via map f46b845a). Therefore, Path 2 depends on Path 1.

#### Step 3: Classification

**Classification:** SEQUENTIAL

**Proof:** Path 2 reads `process.DPP_ErrorMessage` which Path 1 writes. Data dependency exists between paths.

**API Call Detection:** Path 1 contains subprocess call (shape21) which may include email operations (API calls). This further confirms SEQUENTIAL classification.

#### Step 4: Topological Sort Order

**Execution Order:** Path 1 → Path 2

**Dependency Chain:**
1. Path 1: shape19 (Extract error message) → shape21 (Send error email via subprocess)
2. Path 2: shape41 (Map error using process.DPP_ErrorMessage) → shape43 (Return error response)

#### Step 5: Path Termination

**Path 1 Termination:** Subprocess returns (implicit success)  
**Path 2 Termination:** shape43 (Return Documents - Error Response)

#### Step 6: Convergence Points

**Convergence:** None - Paths execute sequentially, Path 2 terminates with return

#### Step 7: Execution Continuation

**Execution Continues From:** None - Path 2 terminates process with return document

#### Step 8: Complete Analysis

**Branch Analysis Summary:**
- **Classification:** SEQUENTIAL
- **Dependency Order:** Path 1 → Path 2
- **Path 1 Purpose:** Send error notification email to support team
- **Path 2 Purpose:** Return error response to D365
- **Business Logic:** First notify support team of error, then return error response to caller

---

## 12. Subprocess Analysis (Step 7a)

### 12.1 Subprocess: (Sub) Office 365 Email (a85945c5-3004-42b9-80b1-104f465cd1fb)

**Subprocess ID:** a85945c5-3004-42b9-80b1-104f465cd1fb  
**Subprocess Name:** (Sub) Office 365 Email  
**Called By:** shape21 (ProcessCall) in main process

#### Internal Flow

**Start → Try/Catch → Decision (Attachment Check) → [Email with Attachment] OR [Email without Attachment] → Stop (continue=true)**

**Detailed Flow:**
1. **shape1 (Start)** - Subprocess entry point
2. **shape2 (Try/Catch)** - Error handling wrapper
3. **shape4 (Decision)** - Check process.DPP_HasAttachment = "Y"
   - **TRUE:** shape11 → shape14 → shape15 → shape6 → shape3 (Email w/ attachment) → shape5 (Stop)
   - **FALSE:** shape23 → shape22 → shape20 → shape7 (Email w/o attachment) → shape9 (Stop)
4. **shape10 (Exception)** - Catch path (throws exception if email fails)

#### Return Paths

**Return Path 1: Success (Implicit)**
- **Label:** SUCCESS (Stop with continue=true)
- **Shape ID:** shape5 or shape9
- **Path:** Email operation completes successfully → Stop (continue=true)
- **Main Process Mapping:** No explicit return label, subprocess completes normally

**Return Path 2: Error (Exception)**
- **Label:** ERROR (Exception thrown)
- **Shape ID:** shape10
- **Path:** Try/Catch error → Exception shape
- **Main Process Mapping:** Exception propagates to main process (caught by main Try/Catch if present)

#### Properties Written by Subprocess

- `process.DPP_MailBody` (shape14, shape22) - Email body content
- `connector.mail.fromAddress` (shape6, shape20) - Email from address
- `connector.mail.toAddress` (shape6, shape20) - Email to address
- `connector.mail.subject` (shape6, shape20) - Email subject
- `connector.mail.body` (shape6, shape20) - Email body
- `connector.mail.filename` (shape6) - Attachment filename (only for path with attachment)

#### Properties Read by Subprocess (from Main Process)

- `process.DPP_HasAttachment` - Determines email operation (with/without attachment)
- `process.DPP_Process_Name` - Used in email body
- `process.DPP_AtomName` - Used in email body
- `process.DPP_ExecutionID` - Used in email body
- `process.DPP_ErrorMessage` - Used in email body
- `process.DPP_Payload` - Attached to email (if DPP_HasAttachment = "Y")
- `process.DPP_File_Name` - Attachment filename
- `process.DPP_Subject` - Email subject
- `process.To_Email` - Email recipient

#### Subprocess Business Logic

**Purpose:** Send error notification email to support team with execution details

**Execution Pattern:**
1. Check if email should have attachment (payload file)
2. Build HTML email body with execution details (process name, atom, execution ID, error message)
3. If attachment required: Attach payload file with generated filename
4. Set email properties (from, to, subject, body)
5. Send email via Office 365 SMTP
6. Return success (Stop with continue=true)

**Error Handling:**
- If email operation fails → Exception thrown → Propagates to main process
- Main process can catch exception or let it fail the entire process

---

## 13. Execution Order (Step 9)

### 13.1 Self-Check Results

✅ **Business logic verified FIRST:** YES  
✅ **Operation analysis complete:** YES  
✅ **Business logic execution order identified:** YES  
✅ **Data dependencies checked FIRST:** YES  
✅ **Operation response analysis used:** YES (Step 1c)  
✅ **Decision analysis used:** YES (Step 7)  
✅ **Dependency graph used:** YES (Step 4)  
✅ **Branch analysis used:** YES (Step 8)  
✅ **Property dependency verification:** YES  
✅ **Topological sort applied:** YES

### 13.2 Business Logic Flow (Step 0)

#### Operation 1: Create Leave Oracle Fusion OP (8f709c2b - Entry Point)

**Purpose:** Web Service Server - Receive incoming leave request from D365

**What it does:** Listens for incoming JSON requests, triggers process execution

**What it produces:**
- Input document (leave request JSON)
- Tracking field: hr_employee_id (employeeNumber)

**Dependent Operations:** All subsequent operations depend on this entry point

**Business Flow:** Entry point MUST execute FIRST to receive request and trigger process

#### Operation 2: Leave Oracle Fusion Create (6e8920fd - Downstream API Call)

**Purpose:** HTTP POST to Oracle Fusion HCM API to create absence/leave record

**What it does:** Sends transformed leave request to Oracle Fusion HCM REST API

**What it produces:**
- HTTP response with status code (meta.base.applicationstatuscode)
- HTTP response message (meta.base.applicationstatusmessage)
- Content-Encoding header (dynamicdocument.DDP_RespHeader)
- Oracle Fusion response body (personAbsenceEntryId and other fields)

**Dependent Operations:**
- Decision (shape2) - Checks HTTP status code
- Decision (shape44) - Checks Content-Encoding header
- Map (shape34) - Maps success response
- Document Properties (shape39, shape46) - Extract error message

**Business Flow:** HTTP POST MUST execute BEFORE decisions check response status

#### Operation 3: Email w Attachment (af07502a - Subprocess)

**Purpose:** Send error notification email with payload attachment

**What it does:** Sends email to support team with error details and payload file

**What it produces:** Email sent (no return value to process)

**Dependent Operations:** None (terminal operation in error notification path)

**Business Flow:** Executes in error path BEFORE returning error response to D365

#### Operation 4: Email W/O Attachment (15a72a21 - Subprocess)

**Purpose:** Send error notification email without attachment

**What it does:** Sends email to support team with error details only

**What it produces:** Email sent (no return value to process)

**Dependent Operations:** None (terminal operation in error notification path)

**Business Flow:** Executes in error path BEFORE returning error response to D365

### 13.3 Actual Execution Order

**REASONING:** This section documents the WHY behind the execution order. The actual sequence diagram is in Step 10.

#### Main Process Execution Order

**Phase 1: Initialization and Input Processing**
1. **shape1 (Start)** - Process triggered by incoming request
2. **shape38 (Input_details)** - Extract execution properties and set process properties
   - **WRITES:** process.DPP_Process_Name, process.DPP_AtomName, process.DPP_Payload, process.DPP_ExecutionID, process.DPP_File_Name, process.DPP_Subject, process.To_Email, process.DPP_HasAttachment
   - **DEPENDENCY:** MUST execute BEFORE subprocess call (shape21) because subprocess reads these properties

**Phase 2: Try Block - Main Processing**
3. **shape17 (Try/Catch)** - Error handling wrapper
4. **shape29 (Map)** - Transform D365 request to Oracle Fusion format
   - **MAP:** c426b4d6 (Leave Create Map)
   - **TRANSFORMATION:** D365 fields → Oracle Fusion HCM fields
5. **shape8 (set URL)** - Set dynamic URL for HTTP operation
   - **WRITES:** dynamicdocument.URL
   - **DEPENDENCY:** MUST execute BEFORE shape33 (HTTP POST) because operation reads this property
6. **shape49 (Notify)** - Log current document (for debugging)
7. **shape33 (HTTP POST)** - Send leave request to Oracle Fusion HCM
   - **OPERATION:** 6e8920fd (Leave Oracle Fusion Create)
   - **WRITES:** meta.base.applicationstatuscode, meta.base.applicationstatusmessage, dynamicdocument.DDP_RespHeader
   - **DEPENDENCY:** MUST execute BEFORE decisions (shape2, shape44) because they read response properties

**Phase 3: Success Path (HTTP 20x)**
8. **shape2 (Decision)** - Check HTTP status code = "20*"
   - **READS:** meta.base.applicationstatuscode
   - **TRUE PATH:** Success (shape34)
   - **FALSE PATH:** Error (shape44)
9. **shape34 (Map)** - Transform Oracle Fusion response to D365 format
   - **MAP:** e4fd3f59 (Oracle Fusion Leave Response Map)
   - **TRANSFORMATION:** Oracle Fusion response → D365 response (success)
10. **shape35 (Return)** - Return success response to D365
    - **HTTP STATUS:** 200
    - **RESPONSE:** {"status": "success", "message": "Data successfully sent to Oracle Fusion", "personAbsenceEntryId": <id>, "success": "true"}

**Phase 4: Error Path (HTTP Non-20x)**
8. **shape2 (Decision)** - Check HTTP status code = "20*"
   - **FALSE PATH:** Error (shape44)
9. **shape44 (Decision)** - Check Content-Encoding = "gzip"
   - **READS:** dynamicdocument.DDP_RespHeader
   - **TRUE PATH:** Gzip decompression (shape45)
   - **FALSE PATH:** Direct error extraction (shape39)

**Phase 4a: Error Path - No Gzip**
10. **shape39 (error msg)** - Extract error message from HTTP response
    - **WRITES:** process.DPP_ErrorMessage
    - **SOURCE:** meta.base.applicationstatusmessage
11. **shape40 (Map)** - Map error to D365 response format
    - **MAP:** f46b845a (Leave Error Map)
    - **READS:** process.DPP_ErrorMessage
12. **shape36 (Return)** - Return error response to D365
    - **HTTP STATUS:** 400
    - **RESPONSE:** {"status": "failure", "message": "<error>", "personAbsenceEntryId": null, "success": "false"}

**Phase 4b: Error Path - Gzip Decompression**
10. **shape45 (Decompress)** - Decompress gzip-encoded error response
    - **GROOVY SCRIPT:** GZIPInputStream decompression
11. **shape46 (error msg)** - Extract error message from decompressed response
    - **WRITES:** process.DPP_ErrorMessage
    - **SOURCE:** Current document (decompressed)
12. **shape47 (Map)** - Map error to D365 response format
    - **MAP:** f46b845a (Leave Error Map)
    - **READS:** process.DPP_ErrorMessage
13. **shape48 (Return)** - Return error response to D365
    - **HTTP STATUS:** 400
    - **RESPONSE:** {"status": "failure", "message": "<error>", "personAbsenceEntryId": null, "success": "false"}

**Phase 5: Catch Block - Exception Handling**
3. **shape17 (Try/Catch)** - Catch path triggered by exception
4. **shape20 (Branch)** - Sequential branch with 2 paths
   - **CLASSIFICATION:** SEQUENTIAL (Path 2 depends on Path 1)
   - **EXECUTION ORDER:** Path 1 → Path 2

**Path 1: Send Error Email**
5. **shape19 (ErrorMsg)** - Extract error message from catch block
   - **WRITES:** process.DPP_ErrorMessage
   - **SOURCE:** meta.base.catcherrorsmessage
6. **shape21 (Subprocess)** - Call email subprocess
   - **SUBPROCESS:** a85945c5 (Office 365 Email)
   - **READS:** process.DPP_Process_Name, process.DPP_AtomName, process.DPP_ExecutionID, process.DPP_ErrorMessage, process.DPP_Payload, process.DPP_File_Name, process.DPP_Subject, process.To_Email, process.DPP_HasAttachment
   - **PURPOSE:** Send error notification email to support team

**Path 2: Return Error Response**
7. **shape41 (Map)** - Map error to D365 response format
   - **MAP:** f46b845a (Leave Error Map)
   - **READS:** process.DPP_ErrorMessage (written by shape19 in Path 1)
   - **DEPENDENCY:** MUST execute AFTER Path 1 (shape19) because it reads process.DPP_ErrorMessage
8. **shape43 (Return)** - Return error response to D365
   - **HTTP STATUS:** 400
   - **RESPONSE:** {"status": "failure", "message": "<error>", "personAbsenceEntryId": null, "success": "false"}

#### Subprocess Execution Order (Office 365 Email)

**Subprocess Entry:** shape21 (ProcessCall) in main process

1. **shape1 (Start)** - Subprocess entry point
2. **shape2 (Try/Catch)** - Error handling wrapper
3. **shape4 (Decision)** - Check process.DPP_HasAttachment = "Y"
   - **READS:** process.DPP_HasAttachment
   - **TRUE PATH:** Email with attachment (shape11)
   - **FALSE PATH:** Email without attachment (shape23)

**Path A: Email with Attachment**
4. **shape11 (Mail_Body)** - Build HTML email body
   - **READS:** process.DPP_Process_Name, process.DPP_AtomName, process.DPP_ExecutionID, process.DPP_ErrorMessage
   - **OUTPUT:** HTML email template with execution details
5. **shape14 (set_MailBody)** - Store email body in process property
   - **WRITES:** process.DPP_MailBody
6. **shape15 (payload)** - Retrieve payload for attachment
   - **READS:** process.DPP_Payload
7. **shape6 (set_Mail_Properties)** - Set email properties
   - **WRITES:** connector.mail.fromAddress, connector.mail.toAddress, connector.mail.subject, connector.mail.body, connector.mail.filename
   - **READS:** process.To_Email, process.DPP_Subject, process.DPP_MailBody, process.DPP_File_Name
8. **shape3 (Email)** - Send email with attachment
   - **OPERATION:** af07502a (Email w Attachment)
9. **shape5 (Stop)** - Return success (continue=true)

**Path B: Email without Attachment**
4. **shape23 (Mail_Body)** - Build HTML email body
   - **READS:** process.DPP_Process_Name, process.DPP_AtomName, process.DPP_ExecutionID, process.DPP_ErrorMessage
   - **OUTPUT:** HTML email template with execution details
5. **shape22 (set_MailBody)** - Store email body in process property
   - **WRITES:** process.DPP_MailBody
6. **shape20 (set_Mail_Properties)** - Set email properties
   - **WRITES:** connector.mail.fromAddress, connector.mail.toAddress, connector.mail.subject, connector.mail.body
   - **READS:** process.To_Email, process.DPP_Subject, process.DPP_MailBody
7. **shape7 (Email)** - Send email without attachment
   - **OPERATION:** 15a72a21 (Email W/O Attachment)
8. **shape9 (Stop)** - Return success (continue=true)

**Catch Path: Email Exception**
3. **shape10 (Exception)** - Throw exception if email fails
   - **READS:** meta.base.catcherrorsmessage
   - **ACTION:** Propagate exception to main process

### 13.4 Dependency Verification

**Reference to Step 4 (Data Dependency Graph):**

✅ **Verified:** shape38 executes BEFORE shape21 (subprocess)
- **Property Chain:** shape38 writes process properties → shape21 (subprocess) reads process properties

✅ **Verified:** shape8 executes BEFORE shape33 (HTTP POST)
- **Property Chain:** shape8 writes dynamicdocument.URL → shape33 reads dynamicdocument.URL

✅ **Verified:** shape33 executes BEFORE shape2 (Decision)
- **Property Chain:** shape33 writes meta.base.applicationstatuscode → shape2 reads meta.base.applicationstatuscode

✅ **Verified:** shape19 (Path 1) executes BEFORE shape41 (Path 2)
- **Property Chain:** shape19 writes process.DPP_ErrorMessage → shape41 reads process.DPP_ErrorMessage

✅ **Verified:** All property reads happen AFTER property writes
- No read-before-write violations detected

### 13.5 Branch Execution Order

**Reference to Step 8 (Branch Analysis):**

**Branch shape20 (Error handling branch):**
- **Classification:** SEQUENTIAL
- **Topological Sort Order:** Path 1 → Path 2
- **Reasoning:** Path 2 reads process.DPP_ErrorMessage which Path 1 writes

**Execution:**
1. **Path 1:** shape19 (Extract error) → shape21 (Send email)
2. **Path 2:** shape41 (Map error) → shape43 (Return error)

---

## 14. Sequence Diagram (Step 10)

**📋 NOTE:** This section provides a VISUAL diagram of the execution flow. Detailed reasoning and analysis are documented in Step 9 (Execution Order).

**References:**
- Based on dependency graph in Step 4
- Based on decision analysis in Step 7
- Based on control flow graph in Step 5
- Based on branch analysis in Step 8
- Based on execution order in Step 9

### 14.1 Main Process Flow

```
START (shape1)
 |
 ├─→ shape38: Input_details (Document Properties)
 |   └─→ WRITES: [process.DPP_Process_Name, process.DPP_AtomName, process.DPP_Payload, 
 |                 process.DPP_ExecutionID, process.DPP_File_Name, process.DPP_Subject, 
 |                 process.To_Email, process.DPP_HasAttachment]
 |
 ├─→ shape17: Try/Catch (Error Handling Wrapper)
 |   |
 |   ├─→ TRY PATH:
 |   |   |
 |   |   ├─→ shape29: Map (Leave Create Map) - Transform D365 → Oracle Fusion
 |   |   |   └─→ MAP: c426b4d6 (D365 Leave Create → HCM Leave Create)
 |   |   |
 |   |   ├─→ shape8: set URL (Document Properties)
 |   |   |   └─→ WRITES: [dynamicdocument.URL]
 |   |   |   └─→ READS: [PP_HCM_LeaveCreate_Properties.Resource_Path]
 |   |   |
 |   |   ├─→ shape49: Notify (Log current document)
 |   |   |   └─→ READS: [current document]
 |   |   |
 |   |   ├─→ shape33: Leave Oracle Fusion Create (Downstream API Call)
 |   |   |   └─→ OPERATION: HTTP POST to Oracle Fusion HCM
 |   |   |   └─→ READS: [dynamicdocument.URL, current document]
 |   |   |   └─→ WRITES: [meta.base.applicationstatuscode, meta.base.applicationstatusmessage, 
 |   |   |                dynamicdocument.DDP_RespHeader]
 |   |   |   └─→ HTTP: [Expected: 200/201/202, Error: 400/401/403/404/500/503]
 |   |   |
 |   |   ├─→ shape2: Decision - HTTP Status 20 check
 |   |   |   └─→ READS: [meta.base.applicationstatuscode]
 |   |   |   └─→ COMPARISON: wildcard match "20*"
 |   |   |   |
 |   |   |   ├─→ IF TRUE (HTTP 20x - Success):
 |   |   |   |   |
 |   |   |   |   ├─→ shape34: Map (Oracle Fusion Leave Response Map)
 |   |   |   |   |   └─→ MAP: e4fd3f59 (HCM Response → D365 Response)
 |   |   |   |   |   └─→ MAPS: personAbsenceEntryId from Oracle response
 |   |   |   |   |   └─→ DEFAULTS: status="success", message="Data successfully sent to Oracle Fusion", success="true"
 |   |   |   |   |
 |   |   |   |   └─→ shape35: Return Documents [HTTP: 200] [SUCCESS]
 |   |   |   |       └─→ RESPONSE: {"status": "success", "message": "Data successfully sent to Oracle Fusion", 
 |   |   |   |                      "personAbsenceEntryId": <id>, "success": "true"}
 |   |   |   |
 |   |   |   └─→ IF FALSE (HTTP Non-20x - Error):
 |   |   |       |
 |   |   |       ├─→ shape44: Decision - Check Response Content Type
 |   |   |           └─→ READS: [dynamicdocument.DDP_RespHeader]
 |   |   |           └─→ COMPARISON: equals "gzip"
 |   |   |           |
 |   |   |           ├─→ IF TRUE (Gzip compressed error):
 |   |   |           |   |
 |   |   |           |   ├─→ shape45: Data Process - Decompress gzip
 |   |   |           |   |   └─→ GROOVY: GZIPInputStream decompression
 |   |   |           |   |
 |   |   |           |   ├─→ shape46: error msg (Document Properties)
 |   |   |           |   |   └─→ WRITES: [process.DPP_ErrorMessage]
 |   |   |           |   |   └─→ READS: [current document (decompressed)]
 |   |   |           |   |
 |   |   |           |   ├─→ shape47: Map (Leave Error Map)
 |   |   |           |   |   └─→ MAP: f46b845a (Error → D365 Response)
 |   |   |           |   |   └─→ READS: [process.DPP_ErrorMessage]
 |   |   |           |   |   └─→ DEFAULTS: status="failure", success="false"
 |   |   |           |   |
 |   |   |           |   └─→ shape48: Return Documents [HTTP: 400] [ERROR - Gzip] [EARLY EXIT]
 |   |   |           |       └─→ RESPONSE: {"status": "failure", "message": "<error>", 
 |   |   |           |                      "personAbsenceEntryId": null, "success": "false"}
 |   |   |           |
 |   |   |           └─→ IF FALSE (No gzip compression):
 |   |   |               |
 |   |   |               ├─→ shape39: error msg (Document Properties)
 |   |   |               |   └─→ WRITES: [process.DPP_ErrorMessage]
 |   |   |               |   └─→ READS: [meta.base.applicationstatusmessage]
 |   |   |               |
 |   |   |               ├─→ shape40: Map (Leave Error Map)
 |   |   |               |   └─→ MAP: f46b845a (Error → D365 Response)
 |   |   |               |   └─→ READS: [process.DPP_ErrorMessage]
 |   |   |               |   └─→ DEFAULTS: status="failure", success="false"
 |   |   |               |
 |   |   |               └─→ shape36: Return Documents [HTTP: 400] [ERROR - No Gzip] [EARLY EXIT]
 |   |   |                   └─→ RESPONSE: {"status": "failure", "message": "<error>", 
 |   |   |                                  "personAbsenceEntryId": null, "success": "false"}
 |   |
 |   └─→ CATCH PATH (Exception in Try block):
 |       |
 |       ├─→ shape20: Branch (2 paths - SEQUENTIAL)
 |           └─→ CLASSIFICATION: SEQUENTIAL (Path 2 depends on Path 1)
 |           └─→ EXECUTION ORDER: Path 1 → Path 2
 |           |
 |           ├─→ PATH 1 (Error Notification):
 |           |   |
 |           |   ├─→ shape19: ErrorMsg (Document Properties)
 |           |   |   └─→ WRITES: [process.DPP_ErrorMessage]
 |           |   |   └─→ READS: [meta.base.catcherrorsmessage]
 |           |   |
 |           |   └─→ shape21: ProcessCall - (Sub) Office 365 Email
 |           |       └─→ SUBPROCESS: a85945c5 (Office 365 Email)
 |           |       └─→ READS: [process.DPP_Process_Name, process.DPP_AtomName, process.DPP_ExecutionID, 
 |           |                   process.DPP_ErrorMessage, process.DPP_Payload, process.DPP_File_Name, 
 |           |                   process.DPP_Subject, process.To_Email, process.DPP_HasAttachment]
 |           |       └─→ PURPOSE: Send error notification email to support team
 |           |       └─→ [See Subprocess Flow below]
 |           |
 |           └─→ PATH 2 (Error Response):
 |               └─→ DEPENDENCY: Executes AFTER Path 1 (reads process.DPP_ErrorMessage from Path 1)
 |               |
 |               ├─→ shape41: Map (Leave Error Map)
 |               |   └─→ MAP: f46b845a (Error → D365 Response)
 |               |   └─→ READS: [process.DPP_ErrorMessage] (written by shape19 in Path 1)
 |               |   └─→ DEFAULTS: status="failure", success="false"
 |               |
 |               └─→ shape43: Return Documents [HTTP: 400] [ERROR - Try/Catch] [EARLY EXIT]
 |                   └─→ RESPONSE: {"status": "failure", "message": "<error>", 
 |                                  "personAbsenceEntryId": null, "success": "false"}
```

### 14.2 Subprocess Flow: (Sub) Office 365 Email

```
SUBPROCESS START (shape1)
 |
 ├─→ shape2: Try/Catch (Error Handling Wrapper)
 |   |
 |   ├─→ TRY PATH:
 |   |   |
 |   |   ├─→ shape4: Decision - Attachment_Check
 |   |       └─→ READS: [process.DPP_HasAttachment]
 |   |       └─→ COMPARISON: equals "Y"
 |   |       |
 |   |       ├─→ IF TRUE (Email with Attachment):
 |   |       |   |
 |   |       |   ├─→ shape11: Mail_Body (Message)
 |   |       |   |   └─→ READS: [process.DPP_Process_Name, process.DPP_AtomName, 
 |   |       |   |              process.DPP_ExecutionID, process.DPP_ErrorMessage]
 |   |       |   |   └─→ OUTPUT: HTML email template with execution details
 |   |       |   |
 |   |       |   ├─→ shape14: set_MailBody (Document Properties)
 |   |       |   |   └─→ WRITES: [process.DPP_MailBody]
 |   |       |   |   └─→ READS: [current document (HTML email)]
 |   |       |   |
 |   |       |   ├─→ shape15: payload (Message)
 |   |       |   |   └─→ READS: [process.DPP_Payload]
 |   |       |   |   └─→ OUTPUT: Payload document for attachment
 |   |       |   |
 |   |       |   ├─→ shape6: set_Mail_Properties (Document Properties)
 |   |       |   |   └─→ WRITES: [connector.mail.fromAddress, connector.mail.toAddress, 
 |   |       |   |              connector.mail.subject, connector.mail.body, connector.mail.filename]
 |   |       |   |   └─→ READS: [PP_Office365_Email.From_Email, process.To_Email, 
 |   |       |   |              PP_Office365_Email.Environment, process.DPP_Subject, 
 |   |       |   |              process.DPP_MailBody, process.DPP_File_Name]
 |   |       |   |
 |   |       |   ├─→ shape3: Email w Attachment (Downstream Email Operation)
 |   |       |   |   └─→ OPERATION: af07502a (Email w Attachment)
 |   |       |   |   └─→ READS: [connector.mail.* properties, current document (payload)]
 |   |       |   |   └─→ CONNECTION: Office 365 Email (smtp-mail.outlook.com:587)
 |   |       |   |
 |   |       |   └─→ shape5: Stop (continue=true) [SUCCESS RETURN]
 |   |       |
 |   |       └─→ IF FALSE (Email without Attachment):
 |   |           |
 |   |           ├─→ shape23: Mail_Body (Message)
 |   |           |   └─→ READS: [process.DPP_Process_Name, process.DPP_AtomName, 
 |   |           |              process.DPP_ExecutionID, process.DPP_ErrorMessage]
 |   |           |   └─→ OUTPUT: HTML email template with execution details
 |   |           |
 |   |           ├─→ shape22: set_MailBody (Document Properties)
 |   |           |   └─→ WRITES: [process.DPP_MailBody]
 |   |           |   └─→ READS: [current document (HTML email)]
 |   |           |
 |   |           ├─→ shape20: set_Mail_Properties (Document Properties)
 |   |           |   └─→ WRITES: [connector.mail.fromAddress, connector.mail.toAddress, 
 |   |           |              connector.mail.subject, connector.mail.body]
 |   |           |   └─→ READS: [PP_Office365_Email.From_Email, process.To_Email, 
 |   |           |              PP_Office365_Email.Environment, process.DPP_Subject, 
 |   |           |              process.DPP_MailBody]
 |   |           |
 |   |           ├─→ shape7: Email W/O Attachment (Downstream Email Operation)
 |   |           |   └─→ OPERATION: 15a72a21 (Email W/O Attachment)
 |   |           |   └─→ READS: [connector.mail.* properties]
 |   |           |   └─→ CONNECTION: Office 365 Email (smtp-mail.outlook.com:587)
 |   |           |
 |   |           └─→ shape9: Stop (continue=true) [SUCCESS RETURN]
 |   |
 |   └─→ CATCH PATH (Email Exception):
 |       |
 |       └─→ shape10: Exception (Throw exception)
 |           └─→ READS: [meta.base.catcherrorsmessage]
 |           └─→ ACTION: Propagate exception to main process
```

**Note:** Detailed request/response JSON examples are documented in Section 7 (HTTP Status Codes and Return Path Responses).

---

## 15. System Layer Identification

### 15.1 Third-Party Systems

| System Name | Type | Connection | Operations | Purpose |
|---|---|---|---|---|
| Oracle Fusion HCM | SaaS (Cloud ERP) | Oracle Fusion (aa1fcb29) | Leave Oracle Fusion Create (6e8920fd) | Create absence/leave records in Oracle HCM |
| Microsoft D365 | SaaS (Cloud ERP) | Web Service Server (8f709c2b) | Create Leave Oracle Fusion OP (8f709c2b) | Source system sending leave requests |
| Office 365 Email | SaaS (Email) | Office 365 Email (00eae79b) | Email w Attachment (af07502a), Email W/O Attachment (15a72a21) | Error notification emails to support team |

### 15.2 System Layer APIs Required

**System Layer API 1: Oracle Fusion HCM - Leave Management**
- **Purpose:** Create absence/leave records in Oracle Fusion HCM
- **Endpoint:** POST https://iaaxey-dev3.fa.ocs.oraclecloud.com:443/hcmRestApi/resources/11.13.18.05/absences
- **Authentication:** Basic Auth (INTEGRATION.USER@al-ghurair.com)
- **Request Format:** JSON
- **Response Format:** JSON
- **Operations:**
  - Create Leave/Absence Entry

**System Layer API 2: Office 365 Email - SMTP**
- **Purpose:** Send error notification emails
- **Endpoint:** SMTP smtp-mail.outlook.com:587
- **Authentication:** SMTP AUTH (Boomi.Dev.failures@al-ghurair.com)
- **Request Format:** Email (HTML body, optional attachment)
- **Response Format:** SMTP response codes
- **Operations:**
  - Send Email with Attachment
  - Send Email without Attachment

### 15.3 Process Layer API

**Process Layer API: HCM Leave Create**
- **Purpose:** Orchestrate leave creation between D365 and Oracle Fusion HCM
- **Entry Point:** Web Service Server (Create Leave Oracle Fusion OP)
- **Request Format:** JSON (D365 Leave Create JSON Profile)
- **Response Format:** JSON (Leave D365 Response)
- **Business Logic:**
  1. Receive leave request from D365
  2. Transform D365 format to Oracle Fusion format
  3. Call Oracle Fusion HCM API to create leave
  4. Check HTTP response status
  5. If success: Return success response with personAbsenceEntryId
  6. If error: Send error notification email, return error response
  7. Handle exceptions: Send error notification email, return error response

---

## 16. Critical Patterns Identified

### 16.1 Pattern 1: Error Notification with Sequential Branch

**Pattern Type:** Error Handling with Notification

**Identification:**
- Try/Catch block (shape17) catches exceptions
- Branch shape (shape20) with 2 sequential paths
- Path 1: Send error notification email (shape19 → shape21)
- Path 2: Return error response (shape41 → shape43)

**Execution Rule:**
- Path 1 MUST execute BEFORE Path 2 (data dependency: process.DPP_ErrorMessage)
- Error email sent to support team BEFORE returning error to caller
- Ensures support team is notified of errors even if response delivery fails

**Sequence Format:**
```
Try/Catch (shape17) → CATCH PATH
 |
 ├─→ Branch (shape20) - SEQUENTIAL
 |   |
 |   ├─→ PATH 1: Error Notification
 |   |   ├─→ shape19: Extract error message
 |   |   |   └─→ WRITES: process.DPP_ErrorMessage
 |   |   └─→ shape21: Send email (subprocess)
 |   |       └─→ READS: process.DPP_ErrorMessage
 |   |
 |   └─→ PATH 2: Error Response (depends on Path 1)
 |       ├─→ shape41: Map error
 |       |   └─→ READS: process.DPP_ErrorMessage (from Path 1)
 |       └─→ shape43: Return error [HTTP: 400] [EARLY EXIT]
```

### 16.2 Pattern 2: HTTP Response Status Check with Gzip Handling

**Pattern Type:** Conditional Error Handling

**Identification:**
- Decision shape (shape2) checks HTTP status code (20* wildcard)
- FALSE path leads to another decision (shape44) checking Content-Encoding header
- Gzip-compressed error responses are decompressed before error extraction
- Non-gzip error responses are extracted directly

**Execution Rule:**
- HTTP operation MUST execute BEFORE status check
- If status is 20x: Success path (map response, return success)
- If status is non-20x: Check gzip encoding
  - If gzip: Decompress → Extract error → Return error
  - If not gzip: Extract error → Return error

**Sequence Format:**
```
shape33: HTTP POST to Oracle Fusion
 |
 ├─→ shape2: Decision - HTTP Status 20 check
 |   |
 |   ├─→ IF TRUE (20x): Success path
 |   |   └─→ shape34: Map response → shape35: Return success [HTTP: 200]
 |   |
 |   └─→ IF FALSE (non-20x): Error path
 |       |
 |       ├─→ shape44: Decision - Check gzip encoding
 |           |
 |           ├─→ IF TRUE (gzip):
 |           |   └─→ shape45: Decompress → shape46: Extract error → shape47: Map error → shape48: Return error [HTTP: 400]
 |           |
 |           └─→ IF FALSE (no gzip):
 |               └─→ shape39: Extract error → shape40: Map error → shape36: Return error [HTTP: 400]
```

### 16.3 Pattern 3: Subprocess with Conditional Email Operation

**Pattern Type:** Conditional Routing in Subprocess

**Identification:**
- Subprocess (a85945c5) called from main process
- Decision shape (shape4) checks attachment flag
- TRUE path: Email with attachment (includes payload file)
- FALSE path: Email without attachment (HTML body only)

**Execution Rule:**
- Main process MUST set process properties BEFORE calling subprocess
- Subprocess checks DPP_HasAttachment flag
- If "Y": Build email with attachment → Send via Email w Attachment operation
- If not "Y": Build email without attachment → Send via Email W/O Attachment operation

**Sequence Format:**
```
Main Process:
 ├─→ shape38: Set process properties (including DPP_HasAttachment)
 |
 └─→ shape21: Call subprocess (Office 365 Email)
     |
     Subprocess:
     ├─→ shape4: Decision - Check DPP_HasAttachment = "Y"
     |   |
     |   ├─→ IF TRUE: Email with attachment path
     |   |   └─→ shape11: Build email → shape14: Set body → shape15: Get payload → 
     |   |       shape6: Set properties → shape3: Send email w/ attachment → shape5: Stop
     |   |
     |   └─→ IF FALSE: Email without attachment path
     |       └─→ shape23: Build email → shape22: Set body → shape20: Set properties → 
     |           shape7: Send email w/o attachment → shape9: Stop
```

### 16.4 Pattern 4: Dynamic URL Construction

**Pattern Type:** Dynamic Property Assignment

**Identification:**
- Document Properties shape (shape8) sets dynamicdocument.URL
- URL value comes from Defined Process Property (PP_HCM_LeaveCreate_Properties.Resource_Path)
- HTTP operation (shape33) reads dynamicdocument.URL as path element

**Execution Rule:**
- Document Properties shape MUST execute BEFORE HTTP operation
- URL is constructed dynamically from process property
- Allows environment-specific configuration (DEV/QA/PROD)

**Sequence Format:**
```
shape8: set URL (Document Properties)
 └─→ WRITES: dynamicdocument.URL
 └─→ READS: PP_HCM_LeaveCreate_Properties.Resource_Path
 |
 ↓
shape33: HTTP POST
 └─→ READS: dynamicdocument.URL
 └─→ OPERATION: POST to Oracle Fusion HCM
```

---

## 17. Function Exposure Decision Table

### 17.1 Process Layer Function

| Function Name | Expose as Azure Function? | Reason | Implementation |
|---|---|---|---|
| HCM_Leave_Create | ✅ YES | Main process orchestration - Entry point from D365 | HTTP-triggered Azure Function |

**Function Details:**
- **Trigger:** HTTP POST
- **Input:** D365 Leave Create JSON (single leave request)
- **Output:** Leave D365 Response JSON (success/error)
- **Business Logic:** Orchestrate leave creation between D365 and Oracle Fusion HCM
- **Error Handling:** Send email notification on errors, return error response

### 17.2 System Layer Functions

| Function Name | Expose as Azure Function? | Reason | Implementation |
|---|---|---|---|
| Oracle_Fusion_HCM_Leave_Create | ✅ YES | System Layer API for Oracle Fusion HCM | HTTP client call to Oracle Fusion API |
| Office365_Email_Send | ✅ YES | System Layer API for email notifications | SMTP client or Microsoft Graph API |

**System Layer Function 1: Oracle_Fusion_HCM_Leave_Create**
- **Purpose:** Create absence/leave record in Oracle Fusion HCM
- **Trigger:** Called by Process Layer function
- **Input:** HCM Leave Create JSON (Oracle Fusion format)
- **Output:** Oracle Fusion Leave Response JSON
- **Implementation:** HTTP client with Basic Auth

**System Layer Function 2: Office365_Email_Send**
- **Purpose:** Send error notification emails
- **Trigger:** Called by Process Layer function (error paths)
- **Input:** Email details (to, subject, body, optional attachment)
- **Output:** Email send status
- **Implementation:** SMTP client or Microsoft Graph API

### 17.3 Subprocess Functions

| Function Name | Expose as Azure Function? | Reason | Implementation |
|---|---|---|---|
| Office365_Email_Subprocess | ❌ NO | Internal helper - Not exposed | Inline logic in Process Layer function |

**Reasoning:** Subprocess logic is simple conditional routing (with/without attachment). This should be implemented as inline logic within the Process Layer function, not as a separate Azure Function.

### 17.4 Function Explosion Prevention

**Decision:** Create 1 Process Layer function + 2 System Layer functions = **3 Azure Functions total**

**Rationale:**
1. **Process Layer:** 1 function for main orchestration (HCM_Leave_Create)
2. **System Layer:** 2 functions for external system integration (Oracle Fusion HCM, Office 365 Email)
3. **Subprocess:** Inline logic (not separate function)

**Benefits:**
- Clear separation of concerns (Process vs System layers)
- Reusable System Layer functions (can be called by other processes)
- Avoid function explosion (no unnecessary functions)
- Simplified deployment and maintenance

---

## 18. Validation Checklist

### 18.1 Data Dependencies

- [x] All property WRITES identified (Section 2.1)
- [x] All property READS identified (Section 2.2)
- [x] Dependency graph built (Section 8)
- [x] Execution order satisfies all dependencies (Section 13)
- [x] No read-before-write violations detected

### 18.2 Decision Analysis

- [x] ALL decision shapes inventoried (Section 10.2)
- [x] BOTH TRUE and FALSE paths traced to termination (Section 10.2)
- [x] Pattern type identified for each decision (Section 10.2)
- [x] Early exits identified and documented (Section 7 - Return paths)
- [x] Convergence points identified (Section 9.3 - None found)
- [x] Decision data sources identified (INPUT/RESPONSE/PROCESS_PROPERTY/TRACK_PROPERTY)
- [x] Decision types classified (PRE_FILTER/POST_OPERATION)
- [x] Actual execution order verified

### 18.3 Branch Analysis

- [x] Each branch classified as parallel or sequential (Section 11.2)
- [x] API call detection performed (Section 11.2)
- [x] Classification based on analysis, not assumption (Section 11.1 - Self-check)
- [x] If sequential: dependency_order built using topological sort (Section 11.2)
- [x] Each path traced to terminal point (Section 11.2)
- [x] Convergence points identified (Section 11.2)
- [x] Execution continuation point determined (Section 11.2)

### 18.4 Sequence Diagram

- [x] Format follows required structure (Section 14)
- [x] Each operation shows READS and WRITES (Section 14)
- [x] Decisions show both TRUE and FALSE paths (Section 14)
- [x] Sequence diagram matches control flow graph from Step 5 (Section 9)
- [x] Execution order matches dependency graph from Step 4 (Section 8)
- [x] Early exits marked [EARLY EXIT] (Section 14)
- [x] Conditional execution marked appropriately (Section 14)
- [x] Subprocess internal flows documented (Section 14.2)
- [x] Subprocess return paths mapped to main process (Section 12)
- [x] Sequence diagram references all prior steps (Section 14)

### 18.5 Subprocess Analysis

- [x] ALL subprocesses analyzed (Section 12)
- [x] Internal flow traced (Section 12.1)
- [x] Return paths identified (Section 12.1)
- [x] Return path labels mapped to main process shapes (Section 12.1)
- [x] Properties written by subprocess documented (Section 12.1)
- [x] Properties read by subprocess from main process documented (Section 12.1)

### 18.6 Edge Cases

- [x] Nested branches/decisions analyzed (None found in this process)
- [x] Loops identified (None found in this process)
- [x] Property chains traced (Section 8.2)
- [x] Circular dependencies detected and resolved (None found)
- [x] Try/Catch error paths documented (Section 13.3)

### 18.7 Property Extraction Completeness

- [x] All property patterns searched (${}, %%, {}) (Section 2)
- [x] Message parameters checked for process properties (Section 2)
- [x] Operation headers/path parameters checked (Section 2)
- [x] Decision track properties identified (meta.*) (Section 10)
- [x] Document properties that read other properties identified (Section 2)

### 18.8 Input/Output Structure Analysis

- [x] Entry point operation identified (Section 1.1)
- [x] Request profile identified and loaded (Section 3.1)
- [x] Request profile structure analyzed (JSON) (Section 3.2)
- [x] Array vs single object detected (Section 3.1 - Single object)
- [x] Array cardinality documented (N/A - Single object)
- [x] ALL request fields extracted (Section 3.4 - 9 fields)
- [x] Request field paths documented (Section 3.4)
- [x] Request field mapping table generated (Section 3.4)
- [x] Response profile identified and loaded (Section 4.1)
- [x] Response profile structure analyzed (Section 4.2)
- [x] ALL response fields extracted (Section 4.3 - 4 fields)
- [x] Response field mapping table generated (Section 4.3)
- [x] Document processing behavior determined (Section 3.3 - Single document)
- [x] Input/Output structure documented in Phase 1 document (Sections 3 & 4)

### 18.9 HTTP Status Codes and Return Path Responses

- [x] Section 7 (HTTP Status Codes and Return Path Responses - Step 1e) present
- [x] All return paths documented with HTTP status codes (Section 7.1)
- [x] Response JSON examples provided for each return path (Section 7.2-7.5)
- [x] Populated fields documented for each return path (Section 7.2-7.5)
- [x] Decision conditions leading to each return documented (Section 7.2-7.5)
- [x] Error codes and success codes documented (Section 7.2-7.5)
- [x] Downstream operation HTTP status codes documented (Section 7.6)
- [x] Error handling strategy documented (Section 7.6)

### 18.10 Map Analysis

- [x] ALL map files identified and loaded (Section 6.1)
- [x] Field mappings extracted from each map (Section 6.2-6.4)
- [x] Profile vs map field name discrepancies documented (Section 6.2)
- [x] Map field names marked as AUTHORITATIVE (Section 6.2)
- [x] Scripting functions analyzed (Section 6.4 - PropertyGet function)
- [x] Static values identified and documented (Section 6.3-6.4)
- [x] Process property mappings documented (Section 6.4)
- [x] Map Analysis documented in Phase 1 document (Section 6)

### 18.11 Function Exposure Decision Table

- [x] Function Exposure Decision Table present (Section 17)
- [x] Process Layer functions identified (Section 17.1)
- [x] System Layer functions identified (Section 17.2)
- [x] Subprocess functions analyzed (Section 17.3)
- [x] Function explosion prevention applied (Section 17.4)
- [x] Total function count reasonable (3 Azure Functions)

### 18.12 Final Validation

- [x] All Steps 1-10 completed in order
- [x] All validation checklists completed
- [x] All "NEVER ASSUME" self-checks answered YES
- [x] Sequence diagram cross-checked against JSON dragpoints
- [x] Execution order verified against dependency graph
- [x] Phase 1 document is complete and ready for Phase 2

---

## Summary

**Process Complexity:** Medium  
**Total Shapes:** 33 (19 main process + 14 subprocess)  
**Decisions:** 3 (2 main process + 1 subprocess)  
**Branches:** 1 (sequential)  
**Try/Catch Blocks:** 2 (1 main process + 1 subprocess)  
**Subprocesses:** 1 (Office 365 Email)  
**Return Paths:** 4 (1 success + 3 error paths)  
**Downstream API Calls:** 1 (Oracle Fusion HCM)  
**Email Notifications:** 2 operations (with/without attachment)

**Key Business Logic:**
1. Receive leave request from D365
2. Transform to Oracle Fusion format
3. POST to Oracle Fusion HCM API
4. Check HTTP response status
5. If success: Return success with personAbsenceEntryId
6. If error: Send email notification, return error response
7. Handle exceptions: Send email notification, return error response

**Critical Patterns:**
1. Error notification with sequential branch
2. HTTP response status check with gzip handling
3. Subprocess with conditional email operation
4. Dynamic URL construction

**Azure Functions Required:** 3
1. Process Layer: HCM_Leave_Create (HTTP-triggered)
2. System Layer: Oracle_Fusion_HCM_Leave_Create (HTTP client)
3. System Layer: Office365_Email_Send (SMTP/Graph API)

---

**Phase 1 Extraction: COMPLETE ✅**

All mandatory sections completed. Ready for Phase 2 (Code Generation).
