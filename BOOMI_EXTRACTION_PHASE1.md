# BOOMI EXTRACTION PHASE 1 - HCM Leave Create Process

**Process Name:** HCM_Leave Create  
**Process ID:** ca69f858-785f-4565-ba1f-b4edc6cca05b  
**Description:** This Process will sync the Leave data between D365 and Oracle HCM  
**Analysis Date:** 2026-02-13

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
12. [Execution Order (Step 9)](#12-execution-order-step-9)
13. [Sequence Diagram (Step 10)](#13-sequence-diagram-step-10)
14. [Subprocess Analysis](#14-subprocess-analysis)
15. [Critical Patterns Identified](#15-critical-patterns-identified)
16. [Validation Checklist](#16-validation-checklist)
17. [System Layer Identification](#17-system-layer-identification)
18. [Function Exposure Decision Table](#18-function-exposure-decision-table)

---

## 1. Operations Inventory

### Main Process Operations

| Operation ID | Operation Name | Type | Sub-Type | Purpose |
|---|---|---|---|---|
| 8f709c2b-e63f-4d5f-9374-2932ed70415d | Create Leave Oracle Fusion OP | connector-action | wss | Web Service Server (Entry Point) - Listens for leave creation requests |
| 6e8920fd-af5a-430b-a1d9-9fde7ac29a12 | Leave Oracle Fusion Create | connector-action | http | HTTP POST to Oracle Fusion HCM to create absence record |
| af07502a-fafd-4976-a691-45d51a33b549 | Email w Attachment | connector-action | mail | Send email with attachment |
| 15a72a21-9b57-49a1-a8ed-d70367146644 | Email W/O Attachment | connector-action | mail | Send email without attachment |

### Subprocess Operations

| Subprocess ID | Subprocess Name | Operations |
|---|---|---|
| a85945c5-3004-42b9-80b1-104f465cd1fb | (Sub) Office 365 Email | Email w Attachment, Email W/O Attachment |

### Connections

| Connection ID | Connection Name | Type | Purpose |
|---|---|---|---|
| aa1fcb29-d146-4425-9ea6-b9698090f60e | Oracle Fusion | http | Oracle Fusion HCM REST API connection (Basic Auth) |
| 00eae79b-2303-4215-8067-dcc299e42697 | Office 365 Email | mail | Office 365 SMTP email connection |

### Profiles

| Profile ID | Profile Name | Type | Purpose |
|---|---|---|---|
| febfa3e1-f719-4ee8-ba57-cdae34137ab3 | D365 Leave Create JSON Profile | profile.json | Request profile (input from D365) |
| a94fa205-c740-40a5-9fda-3d018611135a | HCM Leave Create JSON Profile | profile.json | Oracle Fusion request profile |
| 316175c7-0e45-4869-9ac6-5f9d69882a62 | Oracle Fusion Leave Response JSON Profile | profile.json | Oracle Fusion response profile |
| f4ca3a70-114a-4601-bad8-44a3eb20e2c0 | Leave D365 Response | profile.json | Response profile (output to D365) |
| 23d7a2e9-5cb0-4e9c-9e4b-3154834bad0d | Dummy FF Profile | profile.flatfile | Dummy flat file profile for error mapping |

### Maps

| Map ID | Map Name | From Profile | To Profile | Purpose |
|---|---|---|---|
| c426b4d6-2aff-450e-b43b-59956c4dbc96 | Leave Create Map | D365 Leave Create JSON Profile | HCM Leave Create JSON Profile | Transform D365 request to Oracle Fusion format |
| e4fd3f59-edb5-43a1-aeae-143b600a064e | Oracle Fusion Leave Response Map | Oracle Fusion Leave Response JSON Profile | Leave D365 Response | Transform Oracle Fusion response to D365 response format |
| f46b845a-7d75-41b5-b0ad-c41a6a8e9b12 | Leave Error Map | Dummy FF Profile | Leave D365 Response | Map error messages to D365 response format |

---

## 2. Process Properties Analysis

### Property WRITES (Steps 2-3)

| Property Name | Property ID | Written By Shape(s) | Source Type | Value |
|---|---|---|---|---|
| DPP_Process_Name | process.DPP_Process_Name | shape38 | execution | Process Name |
| DPP_AtomName | process.DPP_AtomName | shape38 | execution | Atom Name |
| DPP_Payload | process.DPP_Payload | shape38 | current | Current document (input payload) |
| DPP_ExecutionID | process.DPP_ExecutionID | shape38 | execution | Execution Id |
| DPP_File_Name | process.DPP_File_Name | shape38 | concatenation | Process Name + timestamp + .txt |
| DPP_Subject | process.DPP_Subject | shape38 | concatenation | Atom Name + " (" + Process Name + " ) has errors to report" |
| To_Email | process.To_Email | shape38 | defined parameter | PP_HCM_LeaveCreate_Properties.To_Email |
| DPP_HasAttachment | process.DPP_HasAttachment | shape38 | defined parameter | PP_HCM_LeaveCreate_Properties.DPP_HasAttachment |
| DPP_ErrorMessage | process.DPP_ErrorMessage | shape19, shape39, shape46 | track | Error message from try/catch or HTTP response |
| dynamicdocument.URL | dynamicdocument.URL | shape8 | defined parameter | PP_HCM_LeaveCreate_Properties.Resource_Path |

### Property READS

| Property Name | Read By Shape(s) | Usage |
|---|---|---|
| process.DPP_ErrorMessage | Subprocess (shape21), Map (f46b845a) | Error message passed to email subprocess and error response map |
| process.DPP_Process_Name | Subprocess (shape21) | Used in email body |
| process.DPP_AtomName | Subprocess (shape21) | Used in email body |
| process.DPP_ExecutionID | Subprocess (shape21) | Used in email body |
| process.DPP_Payload | Subprocess (shape21) | Used as email attachment |
| process.DPP_File_Name | Subprocess (shape21) | Email attachment filename |
| process.DPP_Subject | Subprocess (shape21) | Email subject |
| process.To_Email | Subprocess (shape21) | Email recipient |
| process.DPP_HasAttachment | Subprocess (shape21) | Determines if email has attachment |
| dynamicdocument.URL | shape33 (HTTP operation) | Dynamic URL path for Oracle Fusion API call |
| dynamicdocument.DDP_RespHeader | shape44 (Decision) | Response header Content-Encoding check |

---

## 3. Input Structure Analysis (Step 1a)

### Request Profile Structure

**Profile ID:** febfa3e1-f719-4ee8-ba57-cdae34137ab3  
**Profile Name:** D365 Leave Create JSON Profile  
**Profile Type:** profile.json  
**Entry Operation:** Create Leave Oracle Fusion OP (8f709c2b-e63f-4d5f-9374-2932ed70415d)  
**Input Type:** singlejson  
**Root Structure:** Root/Object

### Array Detection
**Array Detection:** ❌ NO - This is a single object structure, not an array

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

- **Boomi Processing:** Single JSON document processed as one execution
- **Azure Function Requirement:** Must accept single leave object
- **Implementation Pattern:** Process single leave request, return single response
- **Session Management:** One session per execution

### Field Mapping Table

| Boomi Field Path | Boomi Field Name | Data Type | Required | Azure DTO Property | Notes |
|---|---|---|---|---|---|
| Root/Object/employeeNumber | employeeNumber | number | Yes | EmployeeNumber | Employee identifier from D365 |
| Root/Object/absenceType | absenceType | character | Yes | AbsenceType | Type of leave (Sick Leave, Annual Leave, etc.) |
| Root/Object/employer | employer | character | Yes | Employer | Employer name |
| Root/Object/startDate | startDate | character | Yes | StartDate | Leave start date (YYYY-MM-DD format) |
| Root/Object/endDate | endDate | character | Yes | EndDate | Leave end date (YYYY-MM-DD format) |
| Root/Object/absenceStatusCode | absenceStatusCode | character | Yes | AbsenceStatusCode | Status code (SUBMITTED, etc.) |
| Root/Object/approvalStatusCode | approvalStatusCode | character | Yes | ApprovalStatusCode | Approval status (APPROVED, etc.) |
| Root/Object/startDateDuration | startDateDuration | number | Yes | StartDateDuration | Duration for start date (typically 1 for full day) |
| Root/Object/endDateDuration | endDateDuration | number | Yes | EndDateDuration | Duration for end date (typically 1 for full day) |

**Total Fields:** 9

---

## 4. Response Structure Analysis (Step 1b)

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

### Response Field Mapping Table

| Boomi Field Path | Boomi Field Name | Data Type | Azure DTO Property | Notes |
|---|---|---|---|---|
| leaveResponse/Object/status | status | character | Status | "success" or "failure" |
| leaveResponse/Object/message | message | character | Message | Success or error message |
| leaveResponse/Object/personAbsenceEntryId | personAbsenceEntryId | number | PersonAbsenceEntryId | Oracle Fusion absence entry ID |
| leaveResponse/Object/success | success | character | Success | "true" or "false" |

**Total Fields:** 4

---

## 5. Operation Response Analysis (Step 1c)

### Operation: Leave Oracle Fusion Create (6e8920fd-af5a-430b-a1d9-9fde7ac29a12)

**Operation Type:** HTTP POST  
**Response Profile ID:** 316175c7-0e45-4869-9ac6-5f9d69882a62 (Oracle Fusion Leave Response JSON Profile)  
**Response Structure:** Complex JSON with 80+ fields

#### Key Response Fields Extracted

| Field Name | Field Path | Data Type | Extracted By | Written To Property | Consumers |
|---|---|---|---|---|---|
| personAbsenceEntryId | Root/Object/personAbsenceEntryId | number | shape34 (map) | N/A (mapped directly to response) | shape35 (Return Documents) |
| meta.base.applicationstatuscode | Track property | character | shape2 (decision) | N/A (track property) | shape2 (Decision - HTTP Status 20 check) |
| meta.base.applicationstatusmessage | Track property | character | shape39, shape46 | process.DPP_ErrorMessage | shape40, shape47 (Error maps) |
| dynamicdocument.DDP_RespHeader | Response header | character | shape33 (operation) | dynamicdocument.DDP_RespHeader | shape44 (Decision - Content-Encoding check) |

#### Response Structure Summary

The Oracle Fusion response contains extensive leave/absence information:
- **Core Fields:** personAbsenceEntryId, absenceCaseId, absenceTypeId, personId
- **Status Fields:** absenceStatusCd, approvalStatusCd, processingStatus
- **Date Fields:** startDate, endDate, startDateTime, endDateTime, creationDate, lastUpdateDate
- **Duration Fields:** duration, startDateDuration, endDateDuration
- **Reference Fields:** personNumber, absenceType, employer, assignmentId
- **Links Array:** Contains HATEOAS links (up to 21 array elements)

#### Business Logic Implications

1. **HTTP Status Check (shape2):**
   - Checks `meta.base.applicationstatuscode` against pattern "20*"
   - TRUE path: Success (HTTP 200-299) → Map response and return success
   - FALSE path: Error (HTTP 400+) → Check response encoding and return error

2. **Response Encoding Check (shape44):**
   - Checks if `dynamicdocument.DDP_RespHeader` equals "gzip"
   - TRUE path: Decompress gzip response → Extract error message
   - FALSE path: Extract error message directly

3. **Data Flow:**
   - Oracle Fusion operation → HTTP status check → Success/Error routing
   - Success: Map `personAbsenceEntryId` to D365 response
   - Error: Extract error message → Map to D365 error response

---

## 6. Map Analysis (Step 1d)

### Map 1: Leave Create Map (c426b4d6-2aff-450e-b43b-59956c4dbc96)

**From Profile:** febfa3e1-f719-4ee8-ba57-cdae34137ab3 (D365 Leave Create JSON Profile)  
**To Profile:** a94fa205-c740-40a5-9fda-3d018611135a (HCM Leave Create JSON Profile)  
**Type:** Request transformation map (D365 → Oracle Fusion)

#### Field Mappings

| Source Field | Source Path | Target Field | Target Path | Transformation |
|---|---|---|---|---|
| employeeNumber | Root/Object/employeeNumber | personNumber | Root/Object/personNumber | Direct mapping (number) |
| absenceType | Root/Object/absenceType | absenceType | Root/Object/absenceType | Direct mapping |
| employer | Root/Object/employer | employer | Root/Object/employer | Direct mapping |
| startDate | Root/Object/startDate | startDate | Root/Object/startDate | Direct mapping |
| endDate | Root/Object/endDate | endDate | Root/Object/endDate | Direct mapping |
| absenceStatusCode | Root/Object/absenceStatusCode | absenceStatusCd | Root/Object/absenceStatusCd | Direct mapping (field name change) |
| approvalStatusCode | Root/Object/approvalStatusCode | approvalStatusCd | Root/Object/approvalStatusCd | Direct mapping (field name change) |
| startDateDuration | Root/Object/startDateDuration | startDateDuration | Root/Object/startDateDuration | Direct mapping (number) |
| endDateDuration | Root/Object/endDateDuration | endDateDuration | Root/Object/endDateDuration | Direct mapping (number) |

#### Profile vs Map Comparison

| D365 Profile Field Name | Oracle Fusion Profile Field Name | Discrepancy? |
|---|---|---|
| employeeNumber | personNumber | ❌ DIFFERENT - Field name changed |
| absenceStatusCode | absenceStatusCd | ❌ DIFFERENT - Field name abbreviated |
| approvalStatusCode | approvalStatusCd | ❌ DIFFERENT - Field name abbreviated |
| absenceType | absenceType | ✅ Match |
| employer | employer | ✅ Match |
| startDate | startDate | ✅ Match |
| endDate | endDate | ✅ Match |
| startDateDuration | startDateDuration | ✅ Match |
| endDateDuration | endDateDuration | ✅ Match |

**CRITICAL RULE:** Map field names are AUTHORITATIVE. Use map field names in Oracle Fusion API request.

---

### Map 2: Oracle Fusion Leave Response Map (e4fd3f59-edb5-43a1-aeae-143b600a064e)

**From Profile:** 316175c7-0e45-4869-9ac6-5f9d69882a62 (Oracle Fusion Leave Response JSON Profile)  
**To Profile:** f4ca3a70-114a-4601-bad8-44a3eb20e2c0 (Leave D365 Response)  
**Type:** Success response transformation map (Oracle Fusion → D365)

#### Field Mappings

| Source Field | Source Path | Target Field | Target Path | Transformation |
|---|---|---|---|---|
| personAbsenceEntryId | Root/Object/personAbsenceEntryId | personAbsenceEntryId | leaveResponse/Object/personAbsenceEntryId | Direct mapping (number) |
| N/A (default) | N/A | status | leaveResponse/Object/status | Default value: "success" |
| N/A (default) | N/A | message | leaveResponse/Object/message | Default value: "Data successfully sent to Oracle Fusion" |
| N/A (default) | N/A | success | leaveResponse/Object/success | Default value: "true" |

#### Default Values

| Target Field | Default Value | Purpose |
|---|---|---|
| status | "success" | Indicates successful processing |
| message | "Data successfully sent to Oracle Fusion" | Success message |
| success | "true" | Boolean success flag |

---

### Map 3: Leave Error Map (f46b845a-7d75-41b5-b0ad-c41a6a8e9b12)

**From Profile:** 23d7a2e9-5cb0-4e9c-9e4b-3154834bad0d (Dummy FF Profile)  
**To Profile:** f4ca3a70-114a-4601-bad8-44a3eb20e2c0 (Leave D365 Response)  
**Type:** Error response transformation map

#### Field Mappings

| Source Field | Source Type | Target Field | Target Path | Transformation |
|---|---|---|---|---|
| DPP_ErrorMessage | function (PropertyGet) | message | leaveResponse/Object/message | Get process property DPP_ErrorMessage |
| N/A (default) | N/A | status | leaveResponse/Object/status | Default value: "failure" |
| N/A (default) | N/A | success | leaveResponse/Object/success | Default value: "false" |

#### Function Step: PropertyGet

**Function Key:** 1  
**Function Type:** PropertyGet  
**Inputs:**
- Property Name: "DPP_ErrorMessage"
- Default Value: (empty)

**Output:**
- Result: Error message from process property

#### Default Values

| Target Field | Default Value | Purpose |
|---|---|---|
| status | "failure" | Indicates failed processing |
| success | "false" | Boolean failure flag |

**CRITICAL RULE:** Error map retrieves error message from process property `DPP_ErrorMessage` which is populated by:
- shape19: Try/Catch error message (`meta.base.catcherrorsmessage`)
- shape39: HTTP error response message (`meta.base.applicationstatusmessage`)
- shape46: Decompressed HTTP error response message

---

## 7. HTTP Status Codes and Return Path Responses (Step 1e)

### Return Path 1: Success Response (shape35)

**Return Label:** "Success Response"  
**Return Shape ID:** shape35  
**HTTP Status Code:** 200  
**Decision Conditions Leading to Return:**
- shape2 (HTTP Status 20 check): TRUE path (HTTP status code matches "20*")

#### Populated Response Fields

| Field Name | Field Path | Source | Populated By |
|---|---|---|---|
| status | leaveResponse/Object/status | default value | Map e4fd3f59 |
| message | leaveResponse/Object/message | default value | Map e4fd3f59 |
| personAbsenceEntryId | leaveResponse/Object/personAbsenceEntryId | operation_response | Oracle Fusion Create operation (shape33) |
| success | leaveResponse/Object/success | default value | Map e4fd3f59 |

#### Response JSON Example (Success - HTTP 200)

```json
{
  "status": "success",
  "message": "Data successfully sent to Oracle Fusion",
  "personAbsenceEntryId": 300000123456789,
  "success": "true"
}
```

---

### Return Path 2: Error Response - Try/Catch Error (shape43)

**Return Label:** "Error Response"  
**Return Shape ID:** shape43  
**HTTP Status Code:** 500  
**Decision Conditions Leading to Return:**
- shape17 (Try/Catch): CATCH path (exception thrown during processing)
- shape20 (Branch): Path 2

#### Populated Response Fields

| Field Name | Field Path | Source | Populated By |
|---|---|---|---|
| status | leaveResponse/Object/status | default value | Map f46b845a |
| message | leaveResponse/Object/message | process_property | process.DPP_ErrorMessage (from shape19) |
| success | leaveResponse/Object/success | default value | Map f46b845a |

#### Response JSON Example (Error - HTTP 500)

```json
{
  "status": "failure",
  "message": "Error during transformation: Invalid date format",
  "success": "false"
}
```

---

### Return Path 3: Error Response - HTTP Non-200 with GZIP (shape48)

**Return Label:** "Error Response"  
**Return Shape ID:** shape48  
**HTTP Status Code:** 400/500 (depends on Oracle Fusion error)  
**Decision Conditions Leading to Return:**
- shape2 (HTTP Status 20 check): FALSE path (HTTP status code does NOT match "20*")
- shape44 (Check Response Content Type): TRUE path (Content-Encoding equals "gzip")

#### Populated Response Fields

| Field Name | Field Path | Source | Populated By |
|---|---|---|---|
| status | leaveResponse/Object/status | default value | Map f46b845a |
| message | leaveResponse/Object/message | process_property | process.DPP_ErrorMessage (from shape46, after decompression) |
| success | leaveResponse/Object/success | default value | Map f46b845a |

#### Response JSON Example (Error - HTTP 400)

```json
{
  "status": "failure",
  "message": "Oracle Fusion Error: Invalid absence type code",
  "success": "false"
}
```

---

### Return Path 4: Error Response - HTTP Non-200 without GZIP (shape36)

**Return Label:** "Error Response"  
**Return Shape ID:** shape36  
**HTTP Status Code:** 400/500 (depends on Oracle Fusion error)  
**Decision Conditions Leading to Return:**
- shape2 (HTTP Status 20 check): FALSE path (HTTP status code does NOT match "20*")
- shape44 (Check Response Content Type): FALSE path (Content-Encoding does NOT equal "gzip")

#### Populated Response Fields

| Field Name | Field Path | Source | Populated By |
|---|---|---|---|
| status | leaveResponse/Object/status | default value | Map f46b845a |
| message | leaveResponse/Object/message | process_property | process.DPP_ErrorMessage (from shape39) |
| success | leaveResponse/Object/success | default value | Map f46b845a |

#### Response JSON Example (Error - HTTP 400)

```json
{
  "status": "failure",
  "message": "Oracle Fusion Error: Employee not found",
  "success": "false"
}
```

---

### Downstream Operations HTTP Status Codes

| Operation Name | Operation ID | Expected Success Codes | Error Codes | Error Handling |
|---|---|---|---|---|
| Leave Oracle Fusion Create | 6e8920fd-af5a-430b-a1d9-9fde7ac29a12 | 200, 201 | 400, 401, 404, 500 | Return error response with error message |
| Email w Attachment (subprocess) | af07502a-fafd-4976-a691-45d51a33b549 | N/A (email) | N/A | Throw exception on error (subprocess try/catch) |
| Email W/O Attachment (subprocess) | 15a72a21-9b57-49a1-a8ed-d70367146644 | N/A (email) | N/A | Throw exception on error (subprocess try/catch) |

---

## 8. Data Dependency Graph (Step 4)

### Dependency Graph

#### Property: process.DPP_Process_Name
- **Written By:** shape38 (Input_details)
- **Read By:** Subprocess shape21 (via message shape11, shape23)
- **Dependency:** shape38 → Subprocess shape21

#### Property: process.DPP_AtomName
- **Written By:** shape38 (Input_details)
- **Read By:** Subprocess shape21 (via message shape11, shape23)
- **Dependency:** shape38 → Subprocess shape21

#### Property: process.DPP_Payload
- **Written By:** shape38 (Input_details)
- **Read By:** Subprocess shape21 (via message shape15)
- **Dependency:** shape38 → Subprocess shape21

#### Property: process.DPP_ExecutionID
- **Written By:** shape38 (Input_details)
- **Read By:** Subprocess shape21 (via message shape11, shape23)
- **Dependency:** shape38 → Subprocess shape21

#### Property: process.DPP_File_Name
- **Written By:** shape38 (Input_details)
- **Read By:** Subprocess shape21 (via shape6, shape20)
- **Dependency:** shape38 → Subprocess shape21

#### Property: process.DPP_Subject
- **Written By:** shape38 (Input_details)
- **Read By:** Subprocess shape21 (via shape6, shape20)
- **Dependency:** shape38 → Subprocess shape21

#### Property: process.To_Email
- **Written By:** shape38 (Input_details)
- **Read By:** Subprocess shape21 (via shape6, shape20)
- **Dependency:** shape38 → Subprocess shape21

#### Property: process.DPP_HasAttachment
- **Written By:** shape38 (Input_details)
- **Read By:** Subprocess shape21 (via shape4 decision)
- **Dependency:** shape38 → Subprocess shape21

#### Property: process.DPP_ErrorMessage
- **Written By:** shape19 (ErrorMsg - try/catch), shape39 (error msg - HTTP error), shape46 (error msg - HTTP error gzip)
- **Read By:** shape21 (Subprocess), Map f46b845a (Error map)
- **Dependencies:**
  - shape19 → shape21 (Subprocess)
  - shape39 → shape40 (Error map)
  - shape46 → shape47 (Error map)

#### Property: dynamicdocument.URL
- **Written By:** shape8 (set URL)
- **Read By:** shape33 (HTTP operation - Oracle Fusion Create)
- **Dependency:** shape8 → shape33

#### Property: dynamicdocument.DDP_RespHeader
- **Written By:** shape33 (HTTP operation - response header mapping)
- **Read By:** shape44 (Decision - Check Response Content Type)
- **Dependency:** shape33 → shape44

### Dependency Chains

1. **Main Success Flow:**
   - shape38 (Input_details) → shape29 (Map) → shape8 (set URL) → shape33 (HTTP operation) → shape2 (Decision) → shape34 (Map) → shape35 (Return)

2. **Error Flow - Try/Catch:**
   - shape38 (Input_details) → shape17 (Try/Catch) → shape20 (Branch) → shape19 (ErrorMsg) → shape21 (Subprocess) OR shape41 (Error map) → shape43 (Return)

3. **Error Flow - HTTP Non-200:**
   - shape33 (HTTP operation) → shape2 (Decision - FALSE) → shape44 (Decision) → shape39/shape46 (ErrorMsg) → shape40/shape47 (Error map) → shape36/shape48 (Return)

### Property Summary

**Properties that create dependencies:**
- All properties written by shape38 are required by subprocess shape21 (error notification)
- `dynamicdocument.URL` must be set before HTTP operation
- `process.DPP_ErrorMessage` must be set before error mapping
- `dynamicdocument.DDP_RespHeader` must be set before response encoding check

**Independent operations:**
- shape38 (Input_details) has no dependencies (reads from execution context)
- shape29 (Map) depends only on input document
- shape8 (set URL) depends only on defined process property

---

## 9. Control Flow Graph (Step 5)

### Control Flow Map

| From Shape | Shape Type | To Shape(s) | Identifier | Notes |
|---|---|---|---|---|
| shape1 (start) | start | shape38 | default | Entry point |
| shape38 (Input_details) | documentproperties | shape17 | default | Set process properties |
| shape17 (Try/Catch) | catcherrors | shape29 (Try), shape20 (Catch) | default, error | Error handling wrapper |
| shape29 (Map) | map | shape8 | default | Transform D365 to Oracle Fusion format |
| shape8 (set URL) | documentproperties | shape49 | default | Set dynamic URL |
| shape49 (Notify) | notify | shape33 | default | Log payload |
| shape33 (HTTP operation) | connectoraction | shape2 | default | Call Oracle Fusion API |
| shape2 (HTTP Status 20 check) | decision | shape34 (TRUE), shape44 (FALSE) | true, false | Check HTTP status code |
| shape34 (Map) | map | shape35 | default | Map success response |
| shape35 (Success Response) | returndocuments | (none) | N/A | Return success |
| shape44 (Check Response Content Type) | decision | shape45 (TRUE), shape39 (FALSE) | true, false | Check if response is gzipped |
| shape45 (Decompress) | dataprocess | shape46 | default | Decompress gzip response |
| shape46 (error msg) | documentproperties | shape47 | default | Extract error message |
| shape47 (Error map) | map | shape48 | default | Map error response |
| shape48 (Error Response) | returndocuments | (none) | N/A | Return error |
| shape39 (error msg) | documentproperties | shape40 | default | Extract error message |
| shape40 (Error map) | map | shape36 | default | Map error response |
| shape36 (Error Response) | returndocuments | (none) | N/A | Return error |
| shape20 (Branch) | branch | shape19 (Path 1), shape41 (Path 2) | 1, 2 | Error handling branch |
| shape19 (ErrorMsg) | documentproperties | shape21 | default | Extract try/catch error |
| shape21 (Subprocess) | processcall | (none) | N/A | Send error email (terminates) |
| shape41 (Error map) | map | shape43 | default | Map error response |
| shape43 (Error Response) | returndocuments | (none) | N/A | Return error |

### Connection Summary

- **Total Shapes:** 22 (main process)
- **Total Connections:** 21
- **Shapes with Multiple Outgoing Connections:**
  - shape17 (Try/Catch): 2 connections (default, error)
  - shape2 (Decision): 2 connections (true, false)
  - shape44 (Decision): 2 connections (true, false)
  - shape20 (Branch): 2 connections (1, 2)

### Reverse Flow Mapping (Step 6)

| Target Shape | Incoming From Shape(s) | Convergence Point? |
|---|---|---|
| shape38 | shape1 | No |
| shape17 | shape38 | No |
| shape29 | shape17 | No |
| shape8 | shape29 | No |
| shape49 | shape8 | No |
| shape33 | shape49 | No |
| shape2 | shape33 | No |
| shape34 | shape2 | No |
| shape35 | shape34 | No (terminal) |
| shape44 | shape2 | No |
| shape45 | shape44 | No |
| shape46 | shape45 | No |
| shape47 | shape46 | No |
| shape48 | shape47 | No (terminal) |
| shape39 | shape44 | No |
| shape40 | shape39 | No |
| shape36 | shape40 | No (terminal) |
| shape20 | shape17 | No |
| shape19 | shape20 | No |
| shape21 | shape19 | No (terminal) |
| shape41 | shape20 | No |
| shape43 | shape41 | No (terminal) |

**Convergence Points:** None - All paths lead to different terminal shapes (Return Documents or Subprocess)

---

## 10. Decision Shape Analysis (Step 7)

### ✅ Decision Data Source Analysis - MANDATORY

**✅ Decision data sources identified: YES**  
**✅ Decision types classified: YES**  
**✅ Execution order verified: YES**  
**✅ All decision paths traced: YES**  
**✅ Decision patterns identified: YES**  
**✅ Paths traced to termination: YES**

### Decision 1: HTTP Status 20 check (shape2)

**Shape ID:** shape2  
**Comparison Type:** wildcard  
**Comparison:** `meta.base.applicationstatuscode` wildcard matches "20*"

#### Data Source Analysis
- **Data Source:** TRACK_PROPERTY (meta.base.applicationstatuscode)
- **Data Source Type:** RESPONSE (from Oracle Fusion HTTP operation)
- **Decision Type:** POST_OPERATION
- **Actual Execution Order:** Operation shape33 → Response → Decision shape2 → Route based on HTTP status

**PROOF:** The track property `meta.base.applicationstatuscode` is populated by the HTTP operation (shape33) response. This is a standard Boomi track property that captures the HTTP status code from the connector response.

#### Decision Values
- **Value 1:** `meta.base.applicationstatuscode` (Track property - HTTP status code)
- **Value 2:** "20*" (Static value - wildcard pattern for 200-299)

#### TRUE Path (HTTP Success)
- **To Shape:** shape34 (Map - Oracle Fusion Leave Response Map)
- **Termination:** shape35 (Return Documents - Success Response) [SUCCESS]
- **Path:** shape2 → shape34 → shape35
- **HTTP Status Code:** 200

#### FALSE Path (HTTP Error)
- **To Shape:** shape44 (Decision - Check Response Content Type)
- **Termination:** shape36 or shape48 (Return Documents - Error Response) [ERROR]
- **Path:** shape2 → shape44 → [shape39 → shape40 → shape36] OR [shape45 → shape46 → shape47 → shape48]
- **HTTP Status Code:** 400/500 (depends on Oracle Fusion error)

#### Pattern Type
**Error Check (Success vs Failure)** - Checks HTTP response status to determine if Oracle Fusion API call succeeded

#### Early Exit
**No** - Both paths lead to return documents, but different responses (success vs error)

#### Convergence Point
**None** - Paths do not rejoin; each terminates at different Return Documents shapes

---

### Decision 2: Check Response Content Type (shape44)

**Shape ID:** shape44  
**Comparison Type:** equals  
**Comparison:** `dynamicdocument.DDP_RespHeader` equals "gzip"

#### Data Source Analysis
- **Data Source:** TRACK_PROPERTY (dynamicdocument.DDP_RespHeader)
- **Data Source Type:** RESPONSE (from Oracle Fusion HTTP operation - response header)
- **Decision Type:** POST_OPERATION
- **Actual Execution Order:** Operation shape33 → Response Header Mapping → Decision shape44 → Route based on encoding

**PROOF:** The dynamic document property `DDP_RespHeader` is populated by the HTTP operation (shape33) via response header mapping configuration. The operation maps the "Content-Encoding" response header to this property.

#### Decision Values
- **Value 1:** `dynamicdocument.DDP_RespHeader` (Track property - Content-Encoding header)
- **Value 2:** "gzip" (Static value)

#### TRUE Path (Response is GZIP compressed)
- **To Shape:** shape45 (Data Process - Decompress GZIP)
- **Termination:** shape48 (Return Documents - Error Response) [ERROR]
- **Path:** shape44 → shape45 → shape46 → shape47 → shape48
- **HTTP Status Code:** 400/500

#### FALSE Path (Response is NOT GZIP compressed)
- **To Shape:** shape39 (Document Properties - error msg)
- **Termination:** shape36 (Return Documents - Error Response) [ERROR]
- **Path:** shape44 → shape39 → shape40 → shape36
- **HTTP Status Code:** 400/500

#### Pattern Type
**Conditional Logic (Optional Processing)** - Conditionally decompresses response if it's gzipped before extracting error message

#### Early Exit
**No** - Both paths lead to error return documents

#### Convergence Point
**None** - Paths do not rejoin; each terminates at different Return Documents shapes

---

### Decision Summary

| Decision | Data Source | Decision Type | Pattern | Early Exit | Convergence |
|---|---|---|---|---|---|
| shape2 (HTTP Status 20 check) | TRACK_PROPERTY (Response) | POST_OPERATION | Error Check | No | None |
| shape44 (Check Response Content Type) | TRACK_PROPERTY (Response Header) | POST_OPERATION | Conditional Logic | No | None |

**All decisions are POST_OPERATION** - They check data from the Oracle Fusion HTTP operation response, so they execute AFTER the operation completes.

---

## 11. Branch Shape Analysis (Step 8)

### ✅ Branch Classification - MANDATORY

**✅ Classification completed: YES**  
**✅ Assumption check: NO (analyzed dependencies)**  
**✅ Properties extracted: YES**  
**✅ Dependency graph built: YES**  
**✅ Topological sort applied: N/A (no dependencies between paths)**

### Branch 1: Error Handling Branch (shape20)

**Shape ID:** shape20  
**Number of Paths:** 2  
**Location:** Catch path of Try/Catch (shape17)

#### Path Properties Analysis

**Path 1 (shape20 → shape19 → shape21):**
- **Properties READ:**
  - process.DPP_ErrorMessage (read by subprocess via message shapes)
  - process.DPP_Process_Name (read by subprocess)
  - process.DPP_AtomName (read by subprocess)
  - process.DPP_ExecutionID (read by subprocess)
  - process.DPP_Payload (read by subprocess)
  - process.DPP_File_Name (read by subprocess)
  - process.DPP_Subject (read by subprocess)
  - process.To_Email (read by subprocess)
  - process.DPP_HasAttachment (read by subprocess)
- **Properties WRITTEN:**
  - process.DPP_ErrorMessage (written by shape19 from `meta.base.catcherrorsmessage`)

**Path 2 (shape20 → shape41 → shape43):**
- **Properties READ:**
  - process.DPP_ErrorMessage (read by map f46b845a via PropertyGet function)
- **Properties WRITTEN:** None

#### Dependency Graph

**Dependencies between paths:**
- Path 2 reads `process.DPP_ErrorMessage`
- Path 1 writes `process.DPP_ErrorMessage`
- **Therefore:** Path 1 MUST execute BEFORE Path 2

**Dependency Chain:**
```
Path 1 (shape19 writes DPP_ErrorMessage) → Path 2 (shape41 reads DPP_ErrorMessage)
```

#### Classification

**Classification:** SEQUENTIAL

**PROOF:** Path 2 depends on Path 1 because:
1. shape19 (in Path 1) writes `process.DPP_ErrorMessage` from `meta.base.catcherrorsmessage`
2. shape41 (in Path 2) uses map f46b845a which reads `process.DPP_ErrorMessage` via PropertyGet function
3. Data dependency: Path 2 reads property that Path 1 writes → SEQUENTIAL execution required

#### Topological Sort Order

**Execution Order:** Path 1 → Path 2

**Reasoning:** Path 1 must execute first to populate `process.DPP_ErrorMessage`, which Path 2 requires for error response mapping.

#### Path Termination

- **Path 1 Terminal:** shape21 (ProcessCall - Subprocess) - Sends error email, terminates process
- **Path 2 Terminal:** shape43 (Return Documents - Error Response) - Returns error response to caller

#### Convergence Points

**Convergence Points:** None - Paths do not rejoin; each terminates independently

#### Execution Continuation

**Execution Continues From:** None - Both paths are terminal (either subprocess call or return documents)

---

### Branch Summary

| Branch | Shape ID | Num Paths | Classification | Dependency Order | Convergence |
|---|---|---|---|---|---|
| Error Handling Branch | shape20 | 2 | SEQUENTIAL | Path 1 → Path 2 | None |

**CRITICAL:** This branch is SEQUENTIAL due to data dependency (`process.DPP_ErrorMessage`), not due to API calls. Path 1 must execute before Path 2 to ensure error message is available for error response mapping.

---

## 12. Execution Order (Step 9)

### ✅ Execution Order Self-Checks - MANDATORY

**✅ Business logic verified FIRST: YES**  
**✅ Operation analysis complete: YES**  
**✅ Business logic execution order identified: YES**  
**✅ Data dependencies checked FIRST: YES**  
**✅ Operation response analysis used: YES** (Reference: Section 5)  
**✅ Decision analysis used: YES** (Reference: Section 10)  
**✅ Dependency graph used: YES** (Reference: Section 8)  
**✅ Branch analysis used: YES** (Reference: Section 11)  
**✅ Property dependency verification: YES**  
**✅ Topological sort applied: YES** (for branch shape20)

### Business Logic Flow (Step 0 - MUST BE FIRST)

#### Operation: Create Leave Oracle Fusion OP (8f709c2b-e63f-4d5f-9374-2932ed70415d)
- **Purpose:** Entry point - Web Service Server that receives leave creation requests from D365
- **Outputs:** Input document (leave request JSON)
- **Dependent Operations:** All downstream operations depend on this input
- **Business Flow:** Receives leave request → Triggers process execution

#### Operation: Leave Oracle Fusion Create (6e8920fd-af5a-430b-a1d9-9fde7ac29a12)
- **Purpose:** Create absence record in Oracle Fusion HCM via REST API
- **Outputs:**
  - HTTP status code (`meta.base.applicationstatuscode`)
  - Response body (Oracle Fusion absence response)
  - Response header Content-Encoding (`dynamicdocument.DDP_RespHeader`)
- **Dependent Operations:**
  - shape2 (Decision - HTTP Status check) depends on HTTP status code
  - shape44 (Decision - Content-Encoding check) depends on response header
  - shape34 (Map - success response) depends on response body
- **Business Flow:** This operation MUST execute FIRST (after input processing) because all subsequent decisions and mappings depend on its response

#### Subprocess: (Sub) Office 365 Email (a85945c5-3004-42b9-80b1-104f465cd1fb)
- **Purpose:** Send error notification email when processing fails
- **Outputs:** None (email sent)
- **Dependent Operations:** None (terminal operation)
- **Business Flow:** Only executes on error (try/catch or error return path)

### Actual Execution Order (Based on Business Logic and Data Dependencies)

**Based on dependency graph in Step 4, decision analysis in Step 7, control flow graph in Step 5, branch analysis in Step 8...**

#### Main Success Flow

1. **shape1 (START)** - Entry point
2. **shape38 (Input_details)** - Set process properties from execution context
   - WRITES: DPP_Process_Name, DPP_AtomName, DPP_Payload, DPP_ExecutionID, DPP_File_Name, DPP_Subject, To_Email, DPP_HasAttachment
3. **shape17 (Try/Catch)** - Error handling wrapper [TRY PATH]
4. **shape29 (Map - Leave Create Map)** - Transform D365 request to Oracle Fusion format
   - Maps: employeeNumber → personNumber, absenceStatusCode → absenceStatusCd, etc.
5. **shape8 (set URL)** - Set dynamic URL for Oracle Fusion API
   - WRITES: dynamicdocument.URL
   - READS: Defined parameter Resource_Path
6. **shape49 (Notify)** - Log payload (INFO level)
7. **shape33 (HTTP operation - Leave Oracle Fusion Create)** - Call Oracle Fusion REST API
   - READS: dynamicdocument.URL
   - WRITES: meta.base.applicationstatuscode, dynamicdocument.DDP_RespHeader
   - HTTP: POST to Oracle Fusion HCM
8. **shape2 (Decision - HTTP Status 20 check)** - Check if HTTP status is 20x
   - READS: meta.base.applicationstatuscode
   - **IF TRUE (HTTP 200-299):**
     9. **shape34 (Map - Oracle Fusion Leave Response Map)** - Map success response
        - Maps: personAbsenceEntryId, default values (status="success", message, success="true")
     10. **shape35 (Return Documents - Success Response)** - Return success [HTTP 200]
   - **IF FALSE (HTTP 400+):**
     9. **shape44 (Decision - Check Response Content Type)** - Check if response is gzipped
        - READS: dynamicdocument.DDP_RespHeader
        - **IF TRUE (Content-Encoding = "gzip"):**
          10. **shape45 (Data Process - Decompress)** - Decompress gzip response
          11. **shape46 (error msg)** - Extract error message
              - WRITES: process.DPP_ErrorMessage
              - READS: meta.base.applicationstatusmessage
          12. **shape47 (Map - Leave Error Map)** - Map error response
              - READS: process.DPP_ErrorMessage (via PropertyGet function)
          13. **shape48 (Return Documents - Error Response)** - Return error [HTTP 400/500]
        - **IF FALSE (Content-Encoding ≠ "gzip"):**
          10. **shape39 (error msg)** - Extract error message
              - WRITES: process.DPP_ErrorMessage
              - READS: meta.base.applicationstatusmessage
          11. **shape40 (Map - Leave Error Map)** - Map error response
              - READS: process.DPP_ErrorMessage (via PropertyGet function)
          12. **shape36 (Return Documents - Error Response)** - Return error [HTTP 400/500]

#### Error Flow (Try/Catch)

1. **shape1 (START)** - Entry point
2. **shape38 (Input_details)** - Set process properties
3. **shape17 (Try/Catch)** - Error handling wrapper [CATCH PATH]
4. **shape20 (Branch)** - Error handling branch [SEQUENTIAL - 2 paths]
   - **Path 1 (MUST EXECUTE FIRST):**
     5. **shape19 (ErrorMsg)** - Extract try/catch error message
        - WRITES: process.DPP_ErrorMessage
        - READS: meta.base.catcherrorsmessage
     6. **shape21 (ProcessCall - Subprocess)** - Send error email
        - READS: All DPP_* properties
        - Subprocess executes: Email w Attachment OR Email W/O Attachment (based on DPP_HasAttachment)
        - [TERMINATES PROCESS]
   - **Path 2 (EXECUTES AFTER Path 1):**
     5. **shape41 (Map - Leave Error Map)** - Map error response
        - READS: process.DPP_ErrorMessage (via PropertyGet function)
        - **DEPENDENCY:** Requires Path 1 to execute first to populate DPP_ErrorMessage
     6. **shape43 (Return Documents - Error Response)** - Return error [HTTP 500]

### Dependency Verification

**All property reads happen after property writes:**
- ✅ shape8 (set URL) writes `dynamicdocument.URL` → shape33 (HTTP operation) reads it
- ✅ shape33 (HTTP operation) writes `meta.base.applicationstatuscode` → shape2 (Decision) reads it
- ✅ shape33 (HTTP operation) writes `dynamicdocument.DDP_RespHeader` → shape44 (Decision) reads it
- ✅ shape19 (ErrorMsg) writes `process.DPP_ErrorMessage` → shape21 (Subprocess) and shape41 (Map) read it
- ✅ shape39 (error msg) writes `process.DPP_ErrorMessage` → shape40 (Map) reads it
- ✅ shape46 (error msg) writes `process.DPP_ErrorMessage` → shape47 (Map) reads it
- ✅ shape38 (Input_details) writes all DPP_* properties → shape21 (Subprocess) reads them

**Topological Sort Applied:**
- ✅ Branch shape20: Path 1 → Path 2 (Path 1 writes DPP_ErrorMessage, Path 2 reads it)

---

## 13. Sequence Diagram (Step 10)

**📋 NOTE:** This sequence diagram shows the technical flow. Detailed request/response JSON examples are documented in Section 7 (HTTP Status Codes and Return Path Responses).

**Based on dependency graph in Step 4, decision analysis in Step 7, control flow graph in Step 5, branch analysis in Step 8, execution order in Step 9...**

```
START (shape1)
 |
 ├─→ shape38: Input_details (Document Properties)
 |   └─→ WRITES: [process.DPP_Process_Name, process.DPP_AtomName, process.DPP_Payload, 
 |                 process.DPP_ExecutionID, process.DPP_File_Name, process.DPP_Subject,
 |                 process.To_Email, process.DPP_HasAttachment]
 |   └─→ READS: [Execution context properties]
 |
 ├─→ shape17: Try/Catch Error Handler
 |   |
 |   ├─→ TRY PATH:
 |   |   |
 |   |   ├─→ shape29: Leave Create Map (Map)
 |   |   |   └─→ READS: [Input document]
 |   |   |   └─→ Transform: D365 format → Oracle Fusion format
 |   |   |   └─→ Field mappings: employeeNumber→personNumber, absenceStatusCode→absenceStatusCd, etc.
 |   |   |
 |   |   ├─→ shape8: set URL (Document Properties)
 |   |   |   └─→ WRITES: [dynamicdocument.URL]
 |   |   |   └─→ READS: [PP_HCM_LeaveCreate_Properties.Resource_Path]
 |   |   |
 |   |   ├─→ shape49: Notify (Notify)
 |   |   |   └─→ READS: [Current document]
 |   |   |   └─→ Log level: INFO
 |   |   |
 |   |   ├─→ shape33: Leave Oracle Fusion Create (Downstream - HTTP POST)
 |   |   |   └─→ READS: [dynamicdocument.URL]
 |   |   |   └─→ WRITES: [meta.base.applicationstatuscode, dynamicdocument.DDP_RespHeader]
 |   |   |   └─→ HTTP: [Expected: 200/201, Error: 400/401/404/500]
 |   |   |   └─→ Connection: Oracle Fusion (aa1fcb29-d146-4425-9ea6-b9698090f60e)
 |   |   |   └─→ URL: https://iaaxey-dev3.fa.ocs.oraclecloud.com:443/{Resource_Path}
 |   |   |   └─→ Method: POST
 |   |   |   └─→ Auth: Basic Authentication
 |   |   |
 |   |   ├─→ shape2: Decision - HTTP Status 20 check
 |   |   |   └─→ READS: [meta.base.applicationstatuscode]
 |   |   |   └─→ Condition: applicationstatuscode wildcard "20*"?
 |   |   |   |
 |   |   |   ├─→ IF TRUE (HTTP 200-299 - Success):
 |   |   |   |   |
 |   |   |   |   ├─→ shape34: Oracle Fusion Leave Response Map (Map)
 |   |   |   |   |   └─→ READS: [Oracle Fusion response - personAbsenceEntryId]
 |   |   |   |   |   └─→ Default values: status="success", message="Data successfully sent to Oracle Fusion", success="true"
 |   |   |   |   |
 |   |   |   |   └─→ shape35: Return Documents [HTTP: 200] [SUCCESS]
 |   |   |   |       └─→ Response: { "status": "success", "message": "...", "personAbsenceEntryId": 123, "success": "true" }
 |   |   |   |
 |   |   |   └─→ IF FALSE (HTTP 400+ - Error):
 |   |   |       |
 |   |   |       ├─→ shape44: Decision - Check Response Content Type
 |   |   |           └─→ READS: [dynamicdocument.DDP_RespHeader]
 |   |   |           └─→ Condition: DDP_RespHeader equals "gzip"?
 |   |   |           |
 |   |   |           ├─→ IF TRUE (Response is GZIP compressed):
 |   |   |           |   |
 |   |   |           |   ├─→ shape45: Decompress GZIP (Data Process - Groovy Script)
 |   |   |           |   |   └─→ Script: GZIPInputStream decompression
 |   |   |           |   |
 |   |   |           |   ├─→ shape46: error msg (Document Properties)
 |   |   |           |   |   └─→ WRITES: [process.DPP_ErrorMessage]
 |   |   |           |   |   └─→ READS: [meta.base.applicationstatusmessage]
 |   |   |           |   |
 |   |   |           |   ├─→ shape47: Leave Error Map (Map)
 |   |   |           |   |   └─→ READS: [process.DPP_ErrorMessage via PropertyGet function]
 |   |   |           |   |   └─→ Default values: status="failure", success="false"
 |   |   |           |   |
 |   |   |           |   └─→ shape48: Return Documents [HTTP: 400/500] [ERROR]
 |   |   |           |       └─→ Response: { "status": "failure", "message": "...", "success": "false" }
 |   |   |           |
 |   |   |           └─→ IF FALSE (Response is NOT GZIP compressed):
 |   |   |               |
 |   |   |               ├─→ shape39: error msg (Document Properties)
 |   |   |               |   └─→ WRITES: [process.DPP_ErrorMessage]
 |   |   |               |   └─→ READS: [meta.base.applicationstatusmessage]
 |   |   |               |
 |   |   |               ├─→ shape40: Leave Error Map (Map)
 |   |   |               |   └─→ READS: [process.DPP_ErrorMessage via PropertyGet function]
 |   |   |               |   └─→ Default values: status="failure", success="false"
 |   |   |               |
 |   |   |               └─→ shape36: Return Documents [HTTP: 400/500] [ERROR]
 |   |   |                   └─→ Response: { "status": "failure", "message": "...", "success": "false" }
 |   |
 |   └─→ CATCH PATH (Exception thrown during processing):
 |       |
 |       ├─→ shape20: Branch (2 paths - SEQUENTIAL execution)
 |           |
 |           ├─→ PATH 1 (MUST EXECUTE FIRST):
 |           |   |
 |           |   ├─→ shape19: ErrorMsg (Document Properties)
 |           |   |   └─→ WRITES: [process.DPP_ErrorMessage]
 |           |   |   └─→ READS: [meta.base.catcherrorsmessage]
 |           |   |
 |           |   └─→ shape21: ProcessCall - (Sub) Office 365 Email (Subprocess)
 |           |       └─→ READS: [process.DPP_ErrorMessage, process.DPP_Process_Name, process.DPP_AtomName,
 |           |                   process.DPP_ExecutionID, process.DPP_Payload, process.DPP_File_Name,
 |           |                   process.DPP_Subject, process.To_Email, process.DPP_HasAttachment]
 |           |       └─→ SUBPROCESS INTERNAL FLOW:
 |           |           |
 |           |           ├─→ START (shape1)
 |           |           |
 |           |           ├─→ shape2: Try/Catch Error Handler (Subprocess)
 |           |           |   |
 |           |           |   ├─→ TRY PATH:
 |           |           |   |   |
 |           |           |   |   ├─→ shape4: Decision - Attachment_Check
 |           |           |   |   |   └─→ READS: [process.DPP_HasAttachment]
 |           |           |   |   |   └─→ Condition: DPP_HasAttachment equals "Y"?
 |           |           |   |   |   |
 |           |           |   |   |   ├─→ IF TRUE (Has Attachment):
 |           |           |   |   |   |   |
 |           |           |   |   |   |   ├─→ shape11: Mail_Body (Message)
 |           |           |   |   |   |   |   └─→ Build HTML email body with execution details
 |           |           |   |   |   |   |   └─→ READS: [process.DPP_Process_Name, process.DPP_AtomName,
 |           |           |   |   |   |   |              process.DPP_ExecutionID, process.DPP_ErrorMessage]
 |           |           |   |   |   |   |
 |           |           |   |   |   |   ├─→ shape14: set_MailBody (Document Properties)
 |           |           |   |   |   |   |   └─→ WRITES: [process.DPP_MailBody]
 |           |           |   |   |   |   |   └─→ READS: [Current document]
 |           |           |   |   |   |   |
 |           |           |   |   |   |   ├─→ shape15: payload (Message)
 |           |           |   |   |   |   |   └─→ READS: [process.DPP_Payload]
 |           |           |   |   |   |   |
 |           |           |   |   |   |   ├─→ shape6: set_Mail_Properties (Document Properties)
 |           |           |   |   |   |   |   └─→ WRITES: [connector.mail.fromAddress, connector.mail.toAddress,
 |           |           |   |   |   |   |              connector.mail.subject, connector.mail.body, connector.mail.filename]
 |           |           |   |   |   |   |   └─→ READS: [PP_Office365_Email.From_Email, process.To_Email, 
 |           |           |   |   |   |   |              PP_Office365_Email.Environment, process.DPP_Subject,
 |           |           |   |   |   |   |              process.DPP_MailBody, process.DPP_File_Name]
 |           |           |   |   |   |   |
 |           |           |   |   |   |   ├─→ shape3: Email w Attachment (Downstream - Mail Send)
 |           |           |   |   |   |   |   └─→ READS: [connector.mail.* properties]
 |           |           |   |   |   |   |   └─→ Connection: Office 365 Email (00eae79b-2303-4215-8067-dcc299e42697)
 |           |           |   |   |   |   |
 |           |           |   |   |   |   └─→ shape5: Stop (continue=true) [SUCCESS RETURN]
 |           |           |   |   |   |
 |           |           |   |   |   └─→ IF FALSE (No Attachment):
 |           |           |   |   |       |
 |           |           |   |   |       ├─→ shape23: Mail_Body (Message)
 |           |           |   |   |       |   └─→ Build HTML email body (same as shape11)
 |           |           |   |   |       |
 |           |           |   |   |       ├─→ shape22: set_MailBody (Document Properties)
 |           |           |   |   |       |   └─→ WRITES: [process.DPP_MailBody]
 |           |           |   |   |       |
 |           |           |   |   |       ├─→ shape20: set_Mail_Properties (Document Properties)
 |           |           |   |   |       |   └─→ WRITES: [connector.mail.* properties]
 |           |           |   |   |       |
 |           |           |   |   |       ├─→ shape7: Email W/O Attachment (Downstream - Mail Send)
 |           |           |   |   |       |   └─→ READS: [connector.mail.* properties]
 |           |           |   |   |       |
 |           |           |   |   |       └─→ shape9: Stop (continue=true) [SUCCESS RETURN]
 |           |           |   |
 |           |           |   └─→ CATCH PATH:
 |           |           |       └─→ shape10: Exception (Throw exception with try/catch message) [ERROR RETURN]
 |           |           |
 |           |           └─→ END SUBPROCESS
 |           |       └─→ [TERMINATES MAIN PROCESS - No return to main process after subprocess]
 |           |
 |           └─→ PATH 2 (EXECUTES AFTER PATH 1):
 |               |
 |               ├─→ shape41: Leave Error Map (Map)
 |               |   └─→ READS: [process.DPP_ErrorMessage via PropertyGet function]
 |               |   └─→ DEPENDENCY: Requires PATH 1 to execute first (writes DPP_ErrorMessage)
 |               |   └─→ Default values: status="failure", success="false"
 |               |
 |               └─→ shape43: Return Documents [HTTP: 500] [ERROR]
 |                   └─→ Response: { "status": "failure", "message": "...", "success": "false" }
```

**Note:** Detailed request/response JSON examples for all operations and return paths are documented in Section 7 (HTTP Status Codes and Return Path Responses).

---

## 14. Subprocess Analysis

### Subprocess: (Sub) Office 365 Email (a85945c5-3004-42b9-80b1-104f465cd1fb)

**Purpose:** Send error notification email via Office 365 SMTP

#### Internal Flow

1. **START (shape1)** - Subprocess entry point
2. **shape2 (Try/Catch)** - Error handling wrapper
   - **TRY PATH:**
     3. **shape4 (Decision - Attachment_Check)** - Check if email should have attachment
        - Condition: `process.DPP_HasAttachment` equals "Y"
        - **IF TRUE (Has Attachment):**
          4. **shape11 (Message)** - Build HTML email body with execution details
          5. **shape14 (Document Properties)** - Set mail body property
          6. **shape15 (Message)** - Get payload for attachment
          7. **shape6 (Document Properties)** - Set mail properties (from, to, subject, body, filename)
          8. **shape3 (Email w Attachment)** - Send email with attachment
          9. **shape5 (Stop - continue=true)** - Success return
        - **IF FALSE (No Attachment):**
          4. **shape23 (Message)** - Build HTML email body
          5. **shape22 (Document Properties)** - Set mail body property
          6. **shape20 (Document Properties)** - Set mail properties (from, to, subject, body)
          7. **shape7 (Email W/O Attachment)** - Send email without attachment
          8. **shape9 (Stop - continue=true)** - Success return
   - **CATCH PATH:**
     3. **shape10 (Exception)** - Throw exception with try/catch message

#### Return Paths

| Return Label | Return Type | Condition | Main Process Mapping |
|---|---|---|---|
| SUCCESS | Stop (continue=true) | Email sent successfully | N/A (subprocess terminates main process) |
| ERROR | Exception | Email send failed | N/A (exception thrown) |

#### Properties Written by Subprocess

| Property Name | Written By | Value |
|---|---|---|
| process.DPP_MailBody | shape14, shape22 | HTML email body |
| connector.mail.fromAddress | shape6, shape20 | From email address |
| connector.mail.toAddress | shape6, shape20 | To email address |
| connector.mail.subject | shape6, shape20 | Email subject |
| connector.mail.body | shape6, shape20 | Email body |
| connector.mail.filename | shape6 | Attachment filename (only if has attachment) |

#### Properties Read by Subprocess (from Main Process)

| Property Name | Read By | Usage |
|---|---|---|
| process.DPP_HasAttachment | shape4 | Determine if email has attachment |
| process.DPP_Process_Name | shape11, shape23 | Email body - process name |
| process.DPP_AtomName | shape11, shape23 | Email body - atom name |
| process.DPP_ExecutionID | shape11, shape23 | Email body - execution ID |
| process.DPP_ErrorMessage | shape11, shape23 | Email body - error details |
| process.DPP_Payload | shape15 | Email attachment content |
| process.DPP_File_Name | shape6 | Email attachment filename |
| process.DPP_Subject | shape6, shape20 | Email subject |
| process.To_Email | shape6, shape20 | Email recipient |

#### Email Body Template

```html
<!DOCTYPE html>
<html>
  <head>
    <meta http-equiv="Content-Type" content="text/html" charset="us-ascii" />
    <meta name="viewport" content="width=device-width" />
  </head>
  <body>
    <pre>
      <font color="black">
        <h1 style="font-size:15px;"><u>Execution Details:</u></h1>
        <table border="1">
          <tr>
            <th scope="row"><b><font size="2">Process Name</font></b></th>
            <td>{DPP_Process_Name}</td>
          </tr>
          <tr>
            <th scope="row"><b><font size="2">Environment</font></b></th>
            <td>{DPP_AtomName}</td>
          </tr>
          <tr>
            <th scope="row"><b><font size="2">Execution ID</font></b></th>
            <td>{DPP_ExecutionID}</td>
          </tr>
          <tr>
            <th scope="row"><b><font size="2">Error Details</font></b></th>
            <td>{DPP_ErrorMessage}</td>
          </tr>
        </table>
      </font>
    </pre>
    <text>P.S: This is system generated email.</text>
  </body>
</html>
```

#### Subprocess Execution Pattern

**CRITICAL:** The subprocess call (shape21) in the main process does NOT return to the main process. It terminates the main process execution after sending the error email. This is indicated by:
- `abort="true"` in the processcall configuration
- No return path mapping in the main process
- No dragpoints from shape21 (terminal shape)

**Business Logic:**
1. When an exception occurs in the try/catch (shape17), the process branches (shape20)
2. Path 1 sends error email via subprocess and terminates
3. Path 2 returns error response to caller
4. Both paths execute sequentially (Path 1 first, then Path 2)

---

## 15. Critical Patterns Identified

### Pattern 1: Try/Catch Error Handling with Dual Error Path

**Pattern Type:** Error handling with notification and response

**Components:**
- shape17 (Try/Catch)
- shape20 (Branch - 2 paths)
- shape19 → shape21 (Error notification via email)
- shape41 → shape43 (Error response to caller)

**Execution:**
1. Try block wraps main processing logic
2. On exception, catch block activates
3. Branch splits into 2 sequential paths:
   - Path 1: Send error notification email (terminates process)
   - Path 2: Return error response to caller
4. Both paths execute sequentially (Path 1 → Path 2)

**Business Logic:**
- Ensures error notification is sent to support team
- Provides error response to calling system (D365)
- Dual error handling: notification + response

---

### Pattern 2: HTTP Status Code Check with Response Encoding Handling

**Pattern Type:** Conditional error processing based on response encoding

**Components:**
- shape2 (Decision - HTTP Status 20 check)
- shape44 (Decision - Check Response Content Type)
- shape45 (Data Process - Decompress GZIP)

**Execution:**
1. Check HTTP status code (20x = success, 4xx/5xx = error)
2. On error, check if response is GZIP compressed
3. If compressed, decompress before extracting error message
4. If not compressed, extract error message directly

**Business Logic:**
- Oracle Fusion may return compressed error responses
- Must decompress before reading error message
- Ensures error messages are properly extracted regardless of encoding

---

### Pattern 3: Sequential Branch with Data Dependency

**Pattern Type:** Sequential branch execution due to data dependency

**Components:**
- shape20 (Branch - 2 paths)
- Path 1: shape19 (writes DPP_ErrorMessage)
- Path 2: shape41 (reads DPP_ErrorMessage)

**Execution:**
1. Path 1 MUST execute first (writes DPP_ErrorMessage)
2. Path 2 executes after Path 1 (reads DPP_ErrorMessage)
3. Topological sort enforces execution order

**Business Logic:**
- Error message must be captured before mapping error response
- Data dependency ensures correct execution order
- Both paths serve different purposes: notification vs response

---

### Pattern 4: Dynamic URL Configuration

**Pattern Type:** Dynamic endpoint configuration via process properties

**Components:**
- shape8 (set URL)
- Defined process property: Resource_Path
- shape33 (HTTP operation)

**Execution:**
1. Read Resource_Path from defined process property
2. Set dynamicdocument.URL
3. HTTP operation uses dynamic URL

**Business Logic:**
- Allows environment-specific configuration (DEV/QA/PROD)
- Resource path can be changed without modifying process
- Supports versioning of Oracle Fusion API endpoints

---

### Pattern 5: Subprocess with Conditional Logic (Attachment Check)

**Pattern Type:** Conditional subprocess execution based on process property

**Components:**
- Subprocess shape4 (Decision - Attachment_Check)
- shape3 (Email w Attachment)
- shape7 (Email W/O Attachment)

**Execution:**
1. Check DPP_HasAttachment property
2. If "Y", send email with attachment (payload + error details)
3. If "N", send email without attachment (error details only)

**Business Logic:**
- Flexible email notification (with/without payload)
- Reduces email size when payload not needed
- Controlled via process property configuration

---

## 16. Validation Checklist

### Data Dependencies
- [x] All property WRITES identified (Section 2)
- [x] All property READS identified (Section 2)
- [x] Dependency graph built (Section 8)
- [x] Execution order satisfies all dependencies (Section 12)

### Decision Analysis
- [x] ALL decision shapes inventoried (Section 10)
- [x] BOTH TRUE and FALSE paths traced to termination (Section 10)
- [x] Pattern type identified for each decision (Section 10)
- [x] Early exits identified and documented (Section 10)
- [x] Convergence points identified (Section 10)
- [x] **Decision data source analysis complete** (Section 10)
- [x] **Decision types classified** (Section 10)
- [x] **Actual execution order verified** (Section 10)

### Branch Analysis
- [x] Each branch classified as parallel or sequential (Section 11)
- [x] **SELF-CHECK:** Did I check for API calls in branch paths? (Answer: YES)
- [x] **SELF-CHECK:** Did I classify or assume? (Answer: Classified based on data dependencies)
- [x] If sequential: dependency_order built using topological sort (Section 11)
- [x] Each path traced to terminal point (Section 11)
- [x] Convergence points identified (Section 11)
- [x] Execution continuation point determined (Section 11)

### Sequence Diagram
- [x] Format follows required structure (Section 13)
- [x] Each operation shows READS and WRITES (Section 13)
- [x] Decisions show both TRUE and FALSE paths (Section 13)
- [x] **CROSS-VALIDATION:** Sequence diagram matches control flow graph from Step 5 (Section 9)
- [x] **CROSS-VALIDATION:** Execution order matches dependency graph from Step 4 (Section 8)
- [x] Early exits marked [ERROR] (Section 13)
- [x] Conditional execution marked (Section 13)
- [x] Subprocess internal flows documented (Section 13, 14)
- [x] Subprocess return paths mapped to main process (Section 14)

### Subprocess Analysis
- [x] ALL subprocesses analyzed (Section 14)
- [x] Return paths identified (Section 14)
- [x] Return path labels mapped to main process shapes (Section 14)
- [x] Properties written by subprocess documented (Section 14)
- [x] Properties read by subprocess from main process documented (Section 14)

### Property Extraction Completeness
- [x] All property patterns searched (Section 2)
- [x] Message parameters checked for process properties (Section 2)
- [x] Operation headers/path parameters checked (Section 2)
- [x] Decision track properties identified (Section 10)
- [x] Document properties that read other properties identified (Section 2)

### Input/Output Structure Analysis (CONTRACT VERIFICATION)
- [x] Entry point operation identified (Section 3)
- [x] Request profile identified and loaded (Section 3)
- [x] Request profile structure analyzed (Section 3)
- [x] Array vs single object detected (Section 3)
- [x] ALL request fields extracted (Section 3)
- [x] Request field paths documented (Section 3)
- [x] Request field mapping table generated (Section 3)
- [x] Response profile identified and loaded (Section 4)
- [x] Response profile structure analyzed (Section 4)
- [x] ALL response fields extracted (Section 4)
- [x] Response field mapping table generated (Section 4)
- [x] Document processing behavior determined (Section 3)
- [x] Input/Output structure documented in Phase 1 document (Sections 3 & 4)

### HTTP Status Codes and Return Path Responses
- [x] Section 7 (HTTP Status Codes and Return Path Responses - Step 1e) present
- [x] All return paths documented with HTTP status codes (Section 7)
- [x] Response JSON examples provided for each return path (Section 7)
- [x] Populated fields documented for each return path (Section 7)
- [x] Decision conditions leading to each return documented (Section 7)
- [x] Downstream operation HTTP status codes documented (Section 7)
- [x] Error handling strategy documented for downstream operations (Section 7)

### Map Analysis
- [x] ALL map files identified and loaded (Section 6)
- [x] Field mappings extracted from each map (Section 6)
- [x] Profile vs map field name discrepancies documented (Section 6)
- [x] Map field names marked as AUTHORITATIVE (Section 6)
- [x] Scripting functions analyzed (Section 6)
- [x] Static values identified and documented (Section 6)
- [x] Process property mappings documented (Section 6)
- [x] Map Analysis documented in Phase 1 document (Section 6)

---

## 17. System Layer Identification

### Downstream Systems

#### System 1: Oracle Fusion HCM

**System Type:** Oracle Fusion Human Capital Management (Cloud ERP)  
**Connection:** aa1fcb29-d146-4425-9ea6-b9698090f60e (Oracle Fusion)  
**Protocol:** HTTP REST API  
**Authentication:** Basic Authentication  
**Base URL:** https://iaaxey-dev3.fa.ocs.oraclecloud.com:443

**Operations:**
- **Leave Oracle Fusion Create** (6e8920fd-af5a-430b-a1d9-9fde7ac29a12)
  - **Method:** POST
  - **Endpoint:** {Base URL}/{Resource_Path}
  - **Resource Path:** hcmRestApi/resources/11.13.18.05/absences
  - **Purpose:** Create absence/leave record in Oracle Fusion HCM
  - **Request Format:** JSON
  - **Response Format:** JSON (may be GZIP compressed)
  - **Expected Status Codes:** 200, 201
  - **Error Status Codes:** 400, 401, 404, 500

**System Layer API Requirements:**
- **API Name:** OracleFusionHcmAbsenceApi
- **Method:** CreateAbsence
- **Input DTO:** OracleFusionAbsenceRequest
- **Output DTO:** OracleFusionAbsenceResponse
- **Error Handling:** Return error details from Oracle Fusion response
- **Retry Logic:** Not implemented in Boomi (should be added in Azure)
- **Timeout:** Configurable (connectTimeout, readTimeout in connection settings)

---

#### System 2: Office 365 Email (SMTP)

**System Type:** Email notification system  
**Connection:** 00eae79b-2303-4215-8067-dcc299e42697 (Office 365 Email)  
**Protocol:** SMTP  
**Authentication:** SMTP AUTH  
**Server:** smtp-mail.outlook.com:587  
**TLS:** Enabled

**Operations:**
- **Email w Attachment** (af07502a-fafd-4976-a691-45d51a33b549)
  - **Purpose:** Send error notification email with payload attachment
  - **Content Type:** HTML body, plain text attachment
  - **Disposition:** attachment
- **Email W/O Attachment** (15a72a21-9b57-49a1-a8ed-d70367146644)
  - **Purpose:** Send error notification email without attachment
  - **Content Type:** Plain text body, HTML data
  - **Disposition:** inline

**System Layer API Requirements:**
- **API Name:** EmailNotificationApi
- **Method:** SendErrorNotification
- **Input DTO:** ErrorNotificationRequest (subject, body, recipients, attachment)
- **Output DTO:** EmailSendResponse (success/failure)
- **Error Handling:** Throw exception on email send failure
- **Retry Logic:** Not implemented (email is best-effort)

---

### System Layer API Summary

| System | API Name | Method | Protocol | Authentication |
|---|---|---|---|---|
| Oracle Fusion HCM | OracleFusionHcmAbsenceApi | CreateAbsence | HTTP REST | Basic Auth |
| Office 365 Email | EmailNotificationApi | SendErrorNotification | SMTP | SMTP AUTH |

---

## 18. Function Exposure Decision Table

### Process Layer Function

**Function Name:** CreateLeaveInOracleFusion  
**HTTP Method:** POST  
**Route:** /api/hcm/leave/create  
**Purpose:** Create leave/absence record in Oracle Fusion HCM from D365 request

**Expose as Azure Function?** ✅ YES

**Reasoning:**
1. **Entry Point:** This is a Web Service Server (wss) operation, indicating it's an exposed API endpoint
2. **Business Process:** Orchestrates leave creation from D365 to Oracle Fusion
3. **Error Handling:** Comprehensive error handling with notification and response
4. **External Integration:** Integrates two external systems (D365 → Oracle Fusion)
5. **Stateless:** Single request-response pattern, no state management required

**Process Layer Responsibilities:**
- Validate input from D365
- Transform D365 leave request to Oracle Fusion format
- Call Oracle Fusion HCM API (via System Layer)
- Handle errors and return appropriate response
- Send error notifications on failure

**System Layer Dependencies:**
- OracleFusionHcmAbsenceApi.CreateAbsence
- EmailNotificationApi.SendErrorNotification (on error only)

---

### Subprocess Functions

**Subprocess Name:** (Sub) Office 365 Email  
**Purpose:** Send error notification email

**Expose as Azure Function?** ❌ NO

**Reasoning:**
1. **Internal Utility:** This is a reusable subprocess for error notification, not a business process
2. **Called by Parent:** Only called by main process on error, not exposed externally
3. **No Direct Entry Point:** No Web Service Server operation (not an API endpoint)
4. **Shared Logic:** Common email notification logic used by multiple processes

**Implementation:**
- Implement as a **shared utility class/service** within the Azure Functions project
- **Class Name:** EmailNotificationService
- **Method:** SendErrorNotification(errorDetails, hasAttachment)
- Called by Process Layer function when error occurs

---

### Function Exposure Summary

| Function Name | Type | Expose as Azure Function? | Reason |
|---|---|---|---|
| CreateLeaveInOracleFusion | Process Layer | ✅ YES | Entry point, business process orchestration |
| (Sub) Office 365 Email | Utility Subprocess | ❌ NO | Internal utility, implement as shared service |

**CRITICAL RULE:** Only expose Process Layer entry points as Azure Functions. Subprocesses and utility logic should be implemented as shared services/classes within the Azure Functions project.

---

## Summary

This Phase 1 extraction document provides a comprehensive analysis of the HCM Leave Create Boomi process, including:

1. **Complete Operations Inventory** - All operations, connections, profiles, and maps identified
2. **Data Flow Analysis** - Property writes/reads, dependencies, and execution order
3. **Input/Output Contract Verification** - Request/response structures and field mappings
4. **Decision and Branch Analysis** - All decision paths traced and classified
5. **Subprocess Analysis** - Internal flow and integration with main process
6. **Error Handling Patterns** - Try/catch, HTTP status checks, and error notification
7. **System Layer Identification** - Oracle Fusion HCM and Office 365 Email APIs
8. **Function Exposure Decision** - Process Layer function identified for Azure Functions migration

**Key Findings:**
- **Single Entry Point:** Web Service Server (wss) operation receives leave requests from D365
- **Downstream System:** Oracle Fusion HCM REST API for absence creation
- **Error Handling:** Dual error path (notification + response) with sequential branch execution
- **Response Encoding:** Handles GZIP compressed error responses from Oracle Fusion
- **Email Notification:** Subprocess sends error notifications with/without payload attachment
- **Data Dependencies:** All property dependencies identified and validated
- **Execution Order:** Sequential execution with clear data dependency chains

**Ready for Phase 2:** Code generation can proceed with confidence that all business logic, data flows, and integration points have been accurately extracted and documented.

---

**Document Status:** ✅ COMPLETE - All mandatory sections present and validated  
**Next Phase:** Phase 2 - Azure Functions code generation
