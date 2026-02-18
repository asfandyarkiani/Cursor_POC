# BOOMI EXTRACTION PHASE 1: HCM Leave Create Process

**Process Name:** HCM_Leave Create  
**Process ID:** ca69f858-785f-4565-ba1f-b4edc6cca05b  
**Description:** This Process will sync the Leave data between D365 and Oracle HCM  
**Business Domain:** Human Resource Management (HCM)  
**Date Analyzed:** 2025-02-18

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
18. [Critical Patterns Identified](#18-critical-patterns-identified)
19. [Validation Checklist](#19-validation-checklist)

---

## 1. Operations Inventory

### Main Process Operations

| Operation ID | Operation Name | Type | Sub-Type | Purpose |
|---|---|---|---|---|
| 8f709c2b-e63f-4d5f-9374-2932ed70415d | Create Leave Oracle Fusion OP | connector-action | wss | Web Service Server Listen - Entry point for leave creation requests |
| 6e8920fd-af5a-430b-a1d9-9fde7ac29a12 | Leave Oracle Fusion Create | connector-action | http | HTTP POST to Oracle Fusion HCM to create leave absence |
| af07502a-fafd-4976-a691-45d51a33b549 | Email w Attachment | connector-action | mail | Send email with attachment (error notification) |
| 15a72a21-9b57-49a1-a8ed-d70367146644 | Email W/O Attachment | connector-action | mail | Send email without attachment (error notification) |

### Subprocess Operations

**Subprocess ID:** a85945c5-3004-42b9-80b1-104f465cd1fb  
**Subprocess Name:** (Sub) Office 365 Email

| Operation ID | Operation Name | Type | Sub-Type | Purpose |
|---|---|---|---|---|
| af07502a-fafd-4976-a691-45d51a33b549 | Email w Attachment | connector-action | mail | Send email with attachment |
| 15a72a21-9b57-49a1-a8ed-d70367146644 | Email W/O Attachment | connector-action | mail | Send email without attachment |

### Connections

| Connection ID | Connection Name | Type | URL/Host |
|---|---|---|---|
| aa1fcb29-d146-4425-9ea6-b9698090f60e | Oracle Fusion | http | https://iaaxey-dev3.fa.ocs.oraclecloud.com:443 |
| 00eae79b-2303-4215-8067-dcc299e42697 | Office 365 Email | mail | smtp-mail.outlook.com:587 |

---

## 2. Input Structure Analysis (Step 1a)

### ✅ Step 1a Validation Checkpoint
**Status:** COMPLETE

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

- **Boomi Processing:** Single document processing (inputType: singlejson, no array)
- **Azure Function Requirement:** Accept single leave request object
- **Implementation Pattern:** Process single leave request, create absence in Oracle Fusion HCM
- **Execution Pattern:** One execution per request
- **Session Management:** One session per execution

### Request Field Mapping

| Boomi Field Path | Boomi Field Name | Data Type | Required | Azure DTO Property | Notes |
|---|---|---|---|---|
| Root/Object/employeeNumber | employeeNumber | number | Yes | EmployeeNumber | Employee identifier in D365 |
| Root/Object/absenceType | absenceType | character | Yes | AbsenceType | Type of leave (e.g., "Sick Leave") |
| Root/Object/employer | employer | character | Yes | Employer | Employer name |
| Root/Object/startDate | startDate | character | Yes | StartDate | Leave start date (YYYY-MM-DD) |
| Root/Object/endDate | endDate | character | Yes | EndDate | Leave end date (YYYY-MM-DD) |
| Root/Object/absenceStatusCode | absenceStatusCode | character | Yes | AbsenceStatusCode | Status code (e.g., "SUBMITTED") |
| Root/Object/approvalStatusCode | approvalStatusCode | character | Yes | ApprovalStatusCode | Approval status (e.g., "APPROVED") |
| Root/Object/startDateDuration | startDateDuration | number | Yes | StartDateDuration | Duration for start date (days) |
| Root/Object/endDateDuration | endDateDuration | number | Yes | EndDateDuration | Duration for end date (days) |

**Total Fields:** 9 fields (all flat structure, no nested objects)

---

## 3. Response Structure Analysis (Step 1b)

### ✅ Step 1b Validation Checkpoint
**Status:** COMPLETE

### Response Profile Structure

**Profile ID:** f4ca3a70-114a-4601-bad8-44a3eb20e2c0  
**Profile Name:** Leave D365 Response  
**Profile Type:** profile.json  
**Root Structure:** leaveResponse/Object

### Response Format

```json
{
  "leaveResponse": {
    "status": "success",
    "message": "Data successfully sent to Oracle Fusion",
    "personAbsenceEntryId": 12345,
    "success": "true"
  }
}
```

### Response Field Mapping

| Boomi Field Path | Boomi Field Name | Data Type | Azure DTO Property | Notes |
|---|---|---|---|---|
| leaveResponse/Object/status | status | character | Status | "success" or "failure" |
| leaveResponse/Object/message | message | character | Message | Success/error message |
| leaveResponse/Object/personAbsenceEntryId | personAbsenceEntryId | number | PersonAbsenceEntryId | Oracle Fusion absence entry ID |
| leaveResponse/Object/success | success | character | Success | "true" or "false" |

**Total Fields:** 4 fields (all flat structure)

---

## 4. Operation Response Analysis (Step 1c)

### ✅ Step 1c Validation Checkpoint
**Status:** COMPLETE

### Operation Response Inventory

#### Operation: Leave Oracle Fusion Create (6e8920fd-af5a-430b-a1d9-9fde7ac29a12)

**Response Profile ID:** None (responseProfileType: NONE)  
**Response Type:** HTTP response with JSON body  
**Response Structure:** Oracle Fusion HCM REST API response

**Extracted Fields:**
- **Field:** HTTP Status Code
  - **Extracted By:** shape2 (decision shape)
  - **Written To Property:** meta.base.applicationstatuscode (track property)
  - **Consumers:** shape2 (HTTP Status 20 check decision)

- **Field:** HTTP Status Message
  - **Extracted By:** shape39, shape46 (documentproperties shapes)
  - **Written To Property:** process.DPP_ErrorMessage
  - **Consumers:** shape40, shape47 (error response maps), shape21 (email subprocess)

- **Field:** Response Header (Content-Encoding)
  - **Extracted By:** Operation configuration (responseHeaderMapping)
  - **Written To Property:** dynamicdocument.DDP_RespHeader
  - **Consumers:** shape44 (Check Response Content Type decision)

**Data Consumers:**
- **shape2 (Decision):** Checks meta.base.applicationstatuscode for HTTP 20* pattern
- **shape44 (Decision):** Checks dynamicdocument.DDP_RespHeader for "gzip" encoding
- **shape39, shape46:** Extract error messages to process.DPP_ErrorMessage
- **shape40, shape47, shape41:** Use process.DPP_ErrorMessage in error response maps

**Business Logic Implications:**
- **Operation 6e8920fd (HTTP POST)** MUST execute BEFORE **shape2 (decision)** because decision checks HTTP status code from operation response
- **Operation 6e8920fd** produces HTTP response → **shape2** checks status → Routes to success or error path
- **If HTTP status is NOT 20*:** Error path extracts error message → Maps error response → Returns error to caller

---

## 5. Map Analysis (Step 1d)

### ✅ Step 1d Validation Checkpoint
**Status:** COMPLETE

### SOAP Request Maps Inventory

**Note:** This process uses HTTP/REST operations, not SOAP. No SOAP request maps found.

### HTTP Request Maps Inventory

#### Map 1: Leave Create Map (c426b4d6-2aff-450e-b43b-59956c4dbc96)

**From Profile:** febfa3e1-f719-4ee8-ba57-cdae34137ab3 (D365 Leave Create JSON Profile)  
**To Profile:** a94fa205-c740-40a5-9fda-3d018611135a (HCM Leave Create JSON Profile)  
**Type:** Request Transformation Map (D365 → Oracle Fusion HCM)

**Field Mappings:**

| Source Field (D365) | Source Type | Target Field (Oracle HCM) | Profile Match? | Notes |
|---|---|---|---|---|
| employeeNumber | profile | personNumber | ❌ DIFFERENT | D365 uses employeeNumber, Oracle uses personNumber |
| absenceType | profile | absenceType | ✅ Match | Direct mapping |
| employer | profile | employer | ✅ Match | Direct mapping |
| startDate | profile | startDate | ✅ Match | Direct mapping |
| endDate | profile | endDate | ✅ Match | Direct mapping |
| absenceStatusCode | profile | absenceStatusCd | ❌ DIFFERENT | D365: absenceStatusCode, Oracle: absenceStatusCd |
| approvalStatusCode | profile | approvalStatusCd | ❌ DIFFERENT | D365: approvalStatusCode, Oracle: approvalStatusCd |
| startDateDuration | profile | startDateDuration | ✅ Match | Direct mapping |
| endDateDuration | profile | endDateDuration | ✅ Match | Direct mapping |

**Profile vs Map Discrepancies:**

| D365 Field Name | Oracle HCM Field Name (ACTUAL) | Authority | Use in HTTP Request |
|---|---|---|---|
| employeeNumber | personNumber | ✅ MAP | personNumber |
| absenceStatusCode | absenceStatusCd | ✅ MAP | absenceStatusCd |
| approvalStatusCode | approvalStatusCd | ✅ MAP | approvalStatusCd |

**CRITICAL RULE:** Map field names are AUTHORITATIVE. Use map field names in HTTP requests, NOT source profile field names.

#### Map 2: Oracle Fusion Leave Response Map (e4fd3f59-edb5-43a1-aeae-143b600a064e)

**From Profile:** 316175c7-0e45-4869-9ac6-5f9d69882a62 (Oracle Fusion Leave Response JSON Profile)  
**To Profile:** f4ca3a70-114a-4601-bad8-44a3eb20e2c0 (Leave D365 Response)  
**Type:** Response Transformation Map (Oracle Fusion HCM → D365)

**Field Mappings:**

| Source Field (Oracle) | Source Type | Target Field (D365 Response) | Notes |
|---|---|---|---|
| personAbsenceEntryId | profile | personAbsenceEntryId | Oracle Fusion absence entry ID |

**Default Values:**

| Target Field | Default Value | Notes |
|---|---|---|
| status | "success" | Hardcoded success status |
| message | "Data successfully sent to Oracle Fusion" | Hardcoded success message |
| success | "true" | Hardcoded success flag |

#### Map 3: Leave Error Map (f46b845a-7d75-41b5-b0ad-c41a6a8e9b12)

**From Profile:** 23d7a2e9-5cb0-4e9c-9e4b-3154834bad0d (Dummy FF Profile)  
**To Profile:** f4ca3a70-114a-4601-bad8-44a3eb20e2c0 (Leave D365 Response)  
**Type:** Error Response Transformation Map

**Field Mappings:**

| Source | Source Type | Target Field (D365 Response) | Notes |
|---|---|---|---|
| process.DPP_ErrorMessage | function (PropertyGet) | message | Error message from process property |

**Default Values:**

| Target Field | Default Value | Notes |
|---|---|---|
| status | "failure" | Hardcoded failure status |
| success | "false" | Hardcoded failure flag |

**Scripting Functions:** None

---

## 6. HTTP Status Codes and Return Path Responses (Step 1e)

### ✅ Step 1e Validation Checkpoint
**Status:** COMPLETE

### Return Path Inventory

#### Return Path 1: Success Response

**Return Label:** "Success Response"  
**Return Shape ID:** shape35  
**HTTP Status Code:** 200  
**Decision Conditions Leading to Return:**
- Decision shape2: meta.base.applicationstatuscode matches "20*" → TRUE path

**Populated Response Fields:**

| Field Name | Field Path | Source | Populated By |
|---|---|---|---|
| status | leaveResponse/Object/status | static (map default) | Map e4fd3f59 |
| message | leaveResponse/Object/message | static (map default) | Map e4fd3f59 |
| personAbsenceEntryId | leaveResponse/Object/personAbsenceEntryId | operation_response | Operation 6e8920fd (Oracle Fusion response) |
| success | leaveResponse/Object/success | static (map default) | Map e4fd3f59 |

**Response JSON Example:**

```json
{
  "leaveResponse": {
    "status": "success",
    "message": "Data successfully sent to Oracle Fusion",
    "personAbsenceEntryId": 300100558987654,
    "success": "true"
  }
}
```

#### Return Path 2: Error Response (HTTP Status NOT 20*, Content-Encoding = gzip)

**Return Label:** "Error Response"  
**Return Shape ID:** shape48  
**HTTP Status Code:** 400  
**Decision Conditions Leading to Return:**
- Decision shape2: meta.base.applicationstatuscode NOT matching "20*" → FALSE path
- Decision shape44: dynamicdocument.DDP_RespHeader equals "gzip" → TRUE path

**Populated Response Fields:**

| Field Name | Field Path | Source | Populated By |
|---|---|---|---|
| status | leaveResponse/Object/status | static (map default) | Map f46b845a |
| message | leaveResponse/Object/message | process_property | process.DPP_ErrorMessage (from HTTP response) |
| success | leaveResponse/Object/success | static (map default) | Map f46b845a |

**Response JSON Example:**

```json
{
  "leaveResponse": {
    "status": "failure",
    "message": "Oracle Fusion API error: Invalid absence type",
    "success": "false"
  }
}
```

#### Return Path 3: Error Response (HTTP Status NOT 20*, Content-Encoding NOT gzip)

**Return Label:** "Error Response"  
**Return Shape ID:** shape36  
**HTTP Status Code:** 400  
**Decision Conditions Leading to Return:**
- Decision shape2: meta.base.applicationstatuscode NOT matching "20*" → FALSE path
- Decision shape44: dynamicdocument.DDP_RespHeader NOT equals "gzip" → FALSE path

**Populated Response Fields:**

| Field Name | Field Path | Source | Populated By |
|---|---|---|---|
| status | leaveResponse/Object/status | static (map default) | Map f46b845a |
| message | leaveResponse/Object/message | process_property | process.DPP_ErrorMessage (from HTTP response) |
| success | leaveResponse/Object/success | static (map default) | Map f46b845a |

**Response JSON Example:**

```json
{
  "leaveResponse": {
    "status": "failure",
    "message": "Oracle Fusion API error: Employee not found",
    "success": "false"
  }
}
```

#### Return Path 4: Error Response (Try/Catch Exception)

**Return Label:** "Error Response"  
**Return Shape ID:** shape43  
**HTTP Status Code:** 500  
**Decision Conditions Leading to Return:**
- Try/Catch shape17: Exception thrown in Try block → Catch path

**Populated Response Fields:**

| Field Name | Field Path | Source | Populated By |
|---|---|---|---|
| status | leaveResponse/Object/status | static (map default) | Map f46b845a |
| message | leaveResponse/Object/message | process_property | process.DPP_ErrorMessage (from catch error message) |
| success | leaveResponse/Object/success | static (map default) | Map f46b845a |

**Response JSON Example:**

```json
{
  "leaveResponse": {
    "status": "failure",
    "message": "Connection timeout to Oracle Fusion HCM",
    "success": "false"
  }
}
```

### Downstream Operations HTTP Status Codes

| Operation Name | Expected Success Codes | Error Codes | Error Handling |
|---|---|---|---|
| Leave Oracle Fusion Create (6e8920fd) | 200, 201 | 400, 401, 404, 500 | Check HTTP status, extract error message, send error notification email |

---

## 7. Process Properties Analysis (Steps 2-3)

### ✅ Steps 2-3 Validation Checkpoint
**Status:** COMPLETE

### Property WRITES (Step 2)

| Property Name | Written By Shape(s) | Value Source | Notes |
|---|---|---|---|
| process.DPP_Process_Name | shape38 | execution property (Process Name) | Process name for logging/email |
| process.DPP_AtomName | shape38 | execution property (Atom Name) | Atom name for logging/email |
| process.DPP_Payload | shape38 | current document | Input payload for email attachment |
| process.DPP_ExecutionID | shape38 | execution property (Execution Id) | Execution ID for tracking |
| process.DPP_File_Name | shape38 | concatenation (Process Name + Timestamp + ".txt") | Email attachment filename |
| process.DPP_Subject | shape38 | concatenation (Atom Name + " (" + Process Name + " ) has errors to report") | Email subject line |
| process.To_Email | shape38 | defined parameter (PP_HCM_LeaveCreate_Properties.To_Email) | Email recipient address |
| process.DPP_HasAttachment | shape38 | defined parameter (PP_HCM_LeaveCreate_Properties.DPP_HasAttachment) | Flag for email attachment ("Y" or "N") |
| process.DPP_ErrorMessage | shape19, shape39, shape46 | track property (meta.base.catcherrorsmessage or meta.base.applicationstatusmessage) | Error message for email/response |
| dynamicdocument.URL | shape8 | defined parameter (PP_HCM_LeaveCreate_Properties.Resource_Path) | Oracle Fusion API resource path |

### Property READS (Step 3)

| Property Name | Read By Shape(s) | Usage | Notes |
|---|---|---|---|
| dynamicdocument.URL | shape33 (operation 6e8920fd) | HTTP request URL path | Oracle Fusion API endpoint |
| process.DPP_ErrorMessage | shape21 (subprocess), shape40, shape41, shape47 (maps) | Error message in email body and error response | Used in error handling |
| process.DPP_Process_Name | shape21 (subprocess) | Email body content | Process name in error notification |
| process.DPP_AtomName | shape21 (subprocess) | Email body content | Atom name in error notification |
| process.DPP_ExecutionID | shape21 (subprocess) | Email body content | Execution ID in error notification |
| process.DPP_Payload | shape21 (subprocess) | Email attachment content | Input payload attached to error email |
| process.DPP_File_Name | shape21 (subprocess) | Email attachment filename | Filename for error email attachment |
| process.DPP_Subject | shape21 (subprocess) | Email subject line | Subject for error notification email |
| process.To_Email | shape21 (subprocess) | Email recipient | To address for error notification |
| process.DPP_HasAttachment | shape21 (subprocess) | Email attachment flag | Determines if email has attachment |
| dynamicdocument.DDP_RespHeader | shape44 (decision) | Response header check | Checks for gzip encoding |

### Defined Process Properties

#### PP_HCM_LeaveCreate_Properties (e22c04db-7dfa-4b1b-9d88-77365b0fdb18)

| Property Key | Property Label | Type | Default Value | Notes |
|---|---|---|---|---|
| c2d1a1e8-4bcb-435b-b1d2-bb10a209bcc0 | Resource_Path | string | hcmRestApi/resources/11.13.18.05/absences | Oracle Fusion HCM API path |
| 71c90f6e-4f86-49f3-8aef-bae6668c73f9 | To_Email | string | BoomiIntegrationTeam@al-ghurair.com | Error notification recipient |
| a717867b-67f4-4a72-be2d-c99c73309fdb | DPP_HasAttachment | string | Y | Email attachment flag |

#### PP_Office365_Email (0feff13f-8a2c-438a-b3c7-1909e2a7f533)

| Property Key | Property Label | Type | Default Value | Notes |
|---|---|---|---|---|
| 804b6d40-eeee-4ac0-ab4b-94e1aca9f4b7 | From_Email | string | Boomi.Dev.failures@al-ghurair.com | Email sender address |
| d8fc7496-ec2c-4308-a4ca-e9f49c0c9500 | Environment | string | DEV Failure : | Environment prefix for email subject |

---

## 8. Data Dependency Graph (Step 4)

### ✅ Step 4 Validation Checkpoint
**Status:** COMPLETE

### Dependency Graph

#### Property: dynamicdocument.URL

**Writers:** shape8  
**Readers:** shape33 (operation 6e8920fd)  
**Dependency:** shape8 MUST execute BEFORE shape33

**Reasoning:** shape8 sets the Oracle Fusion API resource path from defined process property, which shape33 (HTTP operation) uses to construct the full URL for the HTTP POST request.

#### Property: process.DPP_ErrorMessage

**Writers:** shape19, shape39, shape46  
**Readers:** shape21 (subprocess), shape40, shape41, shape47 (maps)  
**Dependency:** shape19/shape39/shape46 MUST execute BEFORE shape21/shape40/shape41/shape47

**Reasoning:** Error message must be extracted from track properties (catch error message or HTTP status message) before being used in error response maps or email notification subprocess.

#### Property: dynamicdocument.DDP_RespHeader

**Writers:** Operation 6e8920fd (via responseHeaderMapping)  
**Readers:** shape44 (decision)  
**Dependency:** Operation 6e8920fd MUST execute BEFORE shape44

**Reasoning:** HTTP operation extracts Content-Encoding response header, which shape44 checks to determine if response is gzip-encoded.

#### Property: process.DPP_Process_Name, process.DPP_AtomName, process.DPP_ExecutionID, process.DPP_Payload, process.DPP_File_Name, process.DPP_Subject, process.To_Email, process.DPP_HasAttachment

**Writers:** shape38  
**Readers:** shape21 (subprocess)  
**Dependency:** shape38 MUST execute BEFORE shape21

**Reasoning:** shape38 sets all execution context properties (process name, atom name, execution ID, payload, email settings) that are used by the email notification subprocess.

### Dependency Chains

1. **Main Flow Chain:**
   - shape1 (start) → shape38 (set properties) → shape17 (try/catch) → shape29 (map) → shape8 (set URL) → shape49 (notify) → shape33 (HTTP POST) → shape2 (decision)

2. **Success Path Chain:**
   - shape2 (TRUE) → shape34 (map response) → shape35 (return success)

3. **Error Path Chain (gzip):**
   - shape2 (FALSE) → shape44 (check encoding) → shape45 (decompress) → shape46 (extract error) → shape47 (map error) → shape48 (return error)

4. **Error Path Chain (non-gzip):**
   - shape2 (FALSE) → shape44 (check encoding) → shape39 (extract error) → shape40 (map error) → shape36 (return error)

5. **Exception Path Chain:**
   - shape17 (catch) → shape20 (branch) → Path 1: shape19 (extract error) → shape21 (email subprocess) → (no return)
   - shape17 (catch) → shape20 (branch) → Path 2: shape41 (map error) → shape43 (return error)

### Property Summary

**Properties Creating Dependencies:**
- dynamicdocument.URL (shape8 → shape33)
- process.DPP_ErrorMessage (shape19/shape39/shape46 → shape21/shape40/shape41/shape47)
- dynamicdocument.DDP_RespHeader (operation 6e8920fd → shape44)
- All email-related properties (shape38 → shape21)

**Independent Operations:**
- shape1 (start) - No dependencies
- shape38 (set properties) - Only depends on execution context
- shape29 (map) - Only depends on input document

---

## 9. Control Flow Graph (Step 5)

### ✅ Step 5 Validation Checkpoint
**Status:** COMPLETE

### Control Flow Map

| From Shape | To Shape | Identifier | Text | Notes |
|---|---|---|---|---|
| shape1 (start) | shape38 | default | | Entry point |
| shape38 (documentproperties) | shape17 | default | | Set execution properties |
| shape17 (catcherrors) | shape29 | default | Try | Try block entry |
| shape17 (catcherrors) | shape20 | error | Catch | Catch block entry |
| shape29 (map) | shape8 | default | | Map D365 to Oracle format |
| shape8 (documentproperties) | shape49 | default | | Set URL property |
| shape49 (notify) | shape33 | default | | Log request |
| shape33 (connectoraction) | shape2 | default | | HTTP POST to Oracle Fusion |
| shape2 (decision) | shape34 | true | True | HTTP status 20* |
| shape2 (decision) | shape44 | false | False | HTTP status NOT 20* |
| shape34 (map) | shape35 | default | | Map success response |
| shape35 (returndocuments) | - | - | | Success return (terminal) |
| shape44 (decision) | shape45 | true | True | Content-Encoding = gzip |
| shape44 (decision) | shape39 | false | False | Content-Encoding NOT gzip |
| shape45 (dataprocess) | shape46 | default | | Decompress gzip response |
| shape46 (documentproperties) | shape47 | default | | Extract error message |
| shape47 (map) | shape48 | default | | Map error response |
| shape48 (returndocuments) | - | - | | Error return (terminal) |
| shape39 (documentproperties) | shape40 | default | | Extract error message |
| shape40 (map) | shape36 | default | | Map error response |
| shape36 (returndocuments) | - | - | | Error return (terminal) |
| shape20 (branch) | shape19 | 1 | 1 | Branch path 1 (email) |
| shape20 (branch) | shape41 | 2 | 2 | Branch path 2 (return error) |
| shape19 (documentproperties) | shape21 | default | | Extract catch error message |
| shape21 (processcall) | - | - | | Call email subprocess (terminal) |
| shape41 (map) | shape43 | default | | Map catch error response |
| shape43 (returndocuments) | - | - | | Error return (terminal) |

### Connection Summary

- **Total Shapes:** 21 shapes (main process)
- **Total Connections:** 20 connections
- **Shapes with Multiple Outgoing Connections:**
  - shape17 (catcherrors): 2 paths (try, catch)
  - shape2 (decision): 2 paths (true, false)
  - shape44 (decision): 2 paths (true, false)
  - shape20 (branch): 2 paths (1, 2)
- **Terminal Shapes:** shape35, shape36, shape43, shape48 (returndocuments), shape21 (processcall with no return)

### Reverse Flow Mapping (Step 6)

**Convergence Points:** None identified (all paths lead to different terminal points)

**Incoming Connections:**

| Target Shape | Source Shapes | Notes |
|---|---|---|
| shape38 | shape1 | Single entry point |
| shape17 | shape38 | Single source |
| shape29 | shape17 | Try path |
| shape8 | shape29 | Sequential |
| shape49 | shape8 | Sequential |
| shape33 | shape49 | Sequential |
| shape2 | shape33 | Sequential |
| shape34 | shape2 | TRUE path |
| shape44 | shape2 | FALSE path |
| shape35 | shape34 | Success terminal |
| shape45 | shape44 | TRUE path (gzip) |
| shape39 | shape44 | FALSE path (non-gzip) |
| shape46 | shape45 | Sequential |
| shape47 | shape46 | Sequential |
| shape48 | shape47 | Error terminal |
| shape40 | shape39 | Sequential |
| shape36 | shape40 | Error terminal |
| shape19 | shape20 | Branch path 1 |
| shape41 | shape20 | Branch path 2 |
| shape21 | shape19 | Email subprocess call |
| shape43 | shape41 | Error terminal |

---

## 10. Decision Shape Analysis (Step 7)

### ✅ Step 7 Validation Checkpoint
**Status:** COMPLETE

### Self-Check Answers

- ✅ **Decision data sources identified:** YES
- ✅ **Decision types classified:** YES
- ✅ **Execution order verified:** YES
- ✅ **All decision paths traced:** YES
- ✅ **Decision patterns identified:** YES
- ✅ **Paths traced to termination:** YES

### Decision Inventory

#### Decision 1: HTTP Status 20 check (shape2)

**Shape ID:** shape2  
**Comparison Type:** wildcard  
**Value 1:** meta.base.applicationstatuscode (track property)  
**Value 2:** "20*" (static)

**Data Source:** TRACK_PROPERTY (from operation 6e8920fd response)  
**Decision Type:** POST_OPERATION  
**Actual Execution Order:** Operation 6e8920fd (HTTP POST) → Response → Decision shape2 → Route to success or error path

**TRUE Path:**
- **Destination:** shape34 (map success response)
- **Termination:** shape35 (returndocuments - Success Response) [HTTP 200]
- **Pattern:** Success path - Map Oracle Fusion response to D365 format and return

**FALSE Path:**
- **Destination:** shape44 (Check Response Content Type decision)
- **Termination:** shape36 or shape48 (returndocuments - Error Response) [HTTP 400]
- **Pattern:** Error path - Extract error message, map error response, and return

**Pattern Type:** Error Check (Success vs Failure)  
**Convergence Point:** None (paths lead to different terminals)  
**Early Exit:** No (both paths return responses)

**Business Logic:** This decision checks if the Oracle Fusion HCM API call was successful (HTTP status 20*). If successful, map the response and return success. If failed, extract error details and return error response.

#### Decision 2: Check Response Content Type (shape44)

**Shape ID:** shape44  
**Comparison Type:** equals  
**Value 1:** dynamicdocument.DDP_RespHeader (dynamic document property)  
**Value 2:** "gzip" (static)

**Data Source:** TRACK_PROPERTY (from operation 6e8920fd response header)  
**Decision Type:** POST_OPERATION  
**Actual Execution Order:** Operation 6e8920fd (HTTP POST) → Response Header Extracted → Decision shape44 → Route based on encoding

**TRUE Path:**
- **Destination:** shape45 (dataprocess - decompress gzip)
- **Termination:** shape48 (returndocuments - Error Response) [HTTP 400]
- **Pattern:** Error handling with gzip decompression

**FALSE Path:**
- **Destination:** shape39 (extract error message)
- **Termination:** shape36 (returndocuments - Error Response) [HTTP 400]
- **Pattern:** Error handling without decompression

**Pattern Type:** Conditional Logic (Content-Encoding check)  
**Convergence Point:** None (both paths lead to error returns)  
**Early Exit:** No (both paths return error responses)

**Business Logic:** This decision checks if the Oracle Fusion error response is gzip-encoded. If yes, decompress before extracting error message. If no, extract error message directly.

### Decision Patterns Summary

1. **Error Check Pattern:** shape2 checks HTTP status to route to success or error path
2. **Conditional Logic Pattern:** shape44 checks response encoding to handle gzip compression

---

## 11. Branch Shape Analysis (Step 8)

### ✅ Step 8 Validation Checkpoint
**Status:** COMPLETE

### Self-Check Answers

- ✅ **Classification completed:** YES
- ✅ **Assumption check:** NO (analyzed dependencies)
- ✅ **Properties extracted:** YES
- ✅ **Dependency graph built:** YES
- ✅ **Topological sort applied:** YES (for sequential branch)

### Branch Shape: shape20 (Catch Error Handler)

**Shape ID:** shape20  
**Number of Paths:** 2  
**Location:** Catch block of shape17 (try/catch)

#### Path Properties Analysis

**Path 1 (shape19 → shape21):**
- **Reads:** process.DPP_ErrorMessage, process.DPP_Process_Name, process.DPP_AtomName, process.DPP_ExecutionID, process.DPP_Payload, process.DPP_File_Name, process.DPP_Subject, process.To_Email, process.DPP_HasAttachment
- **Writes:** None (subprocess call)
- **Operations:** shape19 (extract error), shape21 (email subprocess)
- **API Calls:** Yes (email operation in subprocess)

**Path 2 (shape41 → shape43):**
- **Reads:** process.DPP_ErrorMessage
- **Writes:** None
- **Operations:** shape41 (map error), shape43 (return error)
- **API Calls:** No

#### Dependency Graph

**Path 1 Dependencies:**
- Reads: process.DPP_ErrorMessage (written by shape19)
- Reads: All email properties (written by shape38, before try/catch)

**Path 2 Dependencies:**
- Reads: process.DPP_ErrorMessage (written by shape19 in Path 1 OR could be from shape38 context)

**Cross-Path Dependencies:**
- Path 2 does NOT depend on Path 1 (process.DPP_ErrorMessage is written by shape19 in Path 1, but Path 2 uses the same property)
- Both paths are independent after shape19 writes the error message

#### Classification

**Classification:** SEQUENTIAL

**Reasoning:**
1. **API Calls Present:** Path 1 contains email operation (API call) in subprocess
2. **Rule:** ALL API CALLS ARE SEQUENTIAL (BOOMI_EXTRACTION_RULES.mdc, Critical Principle 7)
3. **Even though paths have no data dependencies between them, the presence of API calls in Path 1 requires sequential execution**

#### Dependency Order (Topological Sort)

**Execution Order:** Path 1 → Path 2

**Reasoning:**
- Path 1 contains API call (email operation)
- Path 2 contains no API calls
- Sequential execution: Email notification sent first, then error response returned

#### Path Termination

**Path 1 Termination:** shape21 (processcall) - Subprocess call (no explicit return in main process)  
**Path 2 Termination:** shape43 (returndocuments - Error Response)

#### Convergence Points

**Convergence:** None (paths do not converge; they lead to different terminals)

#### Execution Continuation

**Execution Continues From:** None (both paths terminate independently)

**Summary:** Branch shape20 has 2 sequential paths due to API call in Path 1. Path 1 sends error notification email, Path 2 returns error response. Both paths execute sequentially, with email sent before error response returned.

---

## 12. Execution Order (Step 9)

### ✅ Step 9 Validation Checkpoint
**Status:** COMPLETE

### Self-Check Answers

- ✅ **Business logic verified FIRST:** YES
- ✅ **Operation analysis complete:** YES
- ✅ **Business logic execution order identified:** YES
- ✅ **Data dependencies checked FIRST:** YES
- ✅ **Operation response analysis used:** YES (referenced Step 1c)
- ✅ **Decision analysis used:** YES (referenced Step 7)
- ✅ **Dependency graph used:** YES (referenced Step 4)
- ✅ **Branch analysis used:** YES (referenced Step 8)
- ✅ **Property dependency verification:** YES
- ✅ **Topological sort applied:** YES

### Business Logic Flow (Step 0)

#### Operation 1: Create Leave Oracle Fusion OP (8f709c2b-e63f-4d5f-9374-2932ed70415d)

**Purpose:** Entry point - Web Service Server Listen for leave creation requests from D365  
**Outputs:**
- Input document (leave request JSON)
- Execution context properties (process name, atom name, execution ID)

**Dependent Operations:** All subsequent operations depend on this entry point  
**Business Flow:** Receives leave request from D365 → Triggers process execution

#### Operation 2: Leave Oracle Fusion Create (6e8920fd-af5a-430b-a1d9-9fde7ac29a12)

**Purpose:** Create leave absence in Oracle Fusion HCM via HTTP POST  
**Outputs:**
- HTTP response (success or error)
- HTTP status code (written to meta.base.applicationstatuscode)
- HTTP status message (written to meta.base.applicationstatusmessage)
- Response header Content-Encoding (written to dynamicdocument.DDP_RespHeader)

**Dependent Operations:**
- shape2 (decision) reads HTTP status code
- shape44 (decision) reads response header
- shape39, shape46 (extract error message) read HTTP status message

**Business Flow:** Operation 2 MUST execute FIRST (produces HTTP response data) → shape2 checks status → Routes to success or error path

#### Operation 3: Email w Attachment (af07502a-fafd-4976-a691-45d51a33b549) - Subprocess

**Purpose:** Send error notification email with attachment  
**Outputs:** Email sent to integration team

**Dependent Operations:** None (terminal operation in error path)  
**Business Flow:** Called only in catch block error path → Sends error notification → Process terminates

#### Operation 4: Email W/O Attachment (15a72a21-9b57-49a1-a8ed-d70367146644) - Subprocess

**Purpose:** Send error notification email without attachment  
**Outputs:** Email sent to integration team

**Dependent Operations:** None (terminal operation in error path)  
**Business Flow:** Called only in catch block error path → Sends error notification → Process terminates

### Execution Order List

Based on dependency graph (Step 4), decision analysis (Step 7), and branch analysis (Step 8):

#### Main Flow (Happy Path)

1. **shape1** (start) - Entry point
2. **shape38** (documentproperties) - Set execution context properties
   - WRITES: process.DPP_Process_Name, process.DPP_AtomName, process.DPP_Payload, process.DPP_ExecutionID, process.DPP_File_Name, process.DPP_Subject, process.To_Email, process.DPP_HasAttachment
3. **shape17** (catcherrors) - Try/Catch wrapper
4. **TRY BLOCK:**
   - **shape29** (map) - Map D365 leave request to Oracle Fusion format
   - **shape8** (documentproperties) - Set Oracle Fusion API URL
     - WRITES: dynamicdocument.URL
   - **shape49** (notify) - Log request (INFO level)
   - **shape33** (connectoraction) - HTTP POST to Oracle Fusion HCM
     - READS: dynamicdocument.URL
     - WRITES: meta.base.applicationstatuscode, meta.base.applicationstatusmessage, dynamicdocument.DDP_RespHeader
   - **shape2** (decision) - Check HTTP status code
     - READS: meta.base.applicationstatuscode
     - **IF TRUE (HTTP 20*):**
       - **shape34** (map) - Map Oracle Fusion response to D365 format
       - **shape35** (returndocuments) - Return success response [HTTP 200] [SUCCESS]
     - **IF FALSE (HTTP NOT 20*):**
       - **shape44** (decision) - Check response Content-Encoding
         - READS: dynamicdocument.DDP_RespHeader
         - **IF TRUE (gzip):**
           - **shape45** (dataprocess) - Decompress gzip response
           - **shape46** (documentproperties) - Extract error message
             - WRITES: process.DPP_ErrorMessage
           - **shape47** (map) - Map error response
             - READS: process.DPP_ErrorMessage
           - **shape48** (returndocuments) - Return error response [HTTP 400] [ERROR]
         - **IF FALSE (NOT gzip):**
           - **shape39** (documentproperties) - Extract error message
             - WRITES: process.DPP_ErrorMessage
           - **shape40** (map) - Map error response
             - READS: process.DPP_ErrorMessage
           - **shape36** (returndocuments) - Return error response [HTTP 400] [ERROR]

#### Exception Flow (Catch Block)

5. **CATCH BLOCK (if exception thrown in try block):**
   - **shape20** (branch) - Branch to parallel error handling
     - **SEQUENTIAL EXECUTION (API calls present):**
       - **Path 1 (Email Notification):**
         - **shape19** (documentproperties) - Extract catch error message
           - WRITES: process.DPP_ErrorMessage
         - **shape21** (processcall) - Call email subprocess
           - READS: process.DPP_ErrorMessage, process.DPP_Process_Name, process.DPP_AtomName, process.DPP_ExecutionID, process.DPP_Payload, process.DPP_File_Name, process.DPP_Subject, process.To_Email, process.DPP_HasAttachment
           - **Subprocess Internal Flow:** (See Section 14)
       - **Path 2 (Error Response):**
         - **shape41** (map) - Map catch error response
           - READS: process.DPP_ErrorMessage
         - **shape43** (returndocuments) - Return error response [HTTP 500] [ERROR]

### Dependency Verification

**Verified Dependencies:**

1. **shape8 → shape33:** shape8 writes dynamicdocument.URL → shape33 reads it for HTTP request
2. **shape33 → shape2:** shape33 writes meta.base.applicationstatuscode → shape2 reads it for decision
3. **shape33 → shape44:** shape33 writes dynamicdocument.DDP_RespHeader → shape44 reads it for decision
4. **shape39/shape46 → shape40/shape47:** Extract error message → Use in error response map
5. **shape38 → shape21:** shape38 writes all email properties → shape21 (subprocess) reads them
6. **shape19 → shape21/shape41:** shape19 writes process.DPP_ErrorMessage → shape21 and shape41 read it

**All property reads happen AFTER property writes:** ✅ VERIFIED

### Branch Execution Order

**Branch shape20 (Catch Error Handler):**
- **Classification:** SEQUENTIAL (API calls present in Path 1)
- **Execution Order:** Path 1 (Email) → Path 2 (Error Response)
- **Reasoning:** Path 1 contains email operation (API call), which requires sequential execution per BOOMI_EXTRACTION_RULES.mdc

---

## 13. Sequence Diagram (Step 10)

### ✅ Step 10 Validation Checkpoint
**Status:** COMPLETE

**Prerequisites Verified:**
- ✅ Step 4 (Data Dependency Graph) - COMPLETE and DOCUMENTED
- ✅ Step 5 (Control Flow Graph) - COMPLETE and DOCUMENTED
- ✅ Step 7 (Decision Analysis) - COMPLETE and DOCUMENTED
- ✅ Step 8 (Branch Analysis) - COMPLETE and DOCUMENTED
- ✅ Step 9 (Execution Order) - COMPLETE and DOCUMENTED

**Note:** Detailed request/response JSON examples are documented in Section 16 (Request/Response JSON Examples).

### Sequence Diagram

**Based on:**
- Dependency graph in Step 4
- Control flow graph in Step 5
- Decision analysis in Step 7
- Branch analysis in Step 8
- Execution order in Step 9

```
START (shape1 - Web Service Server Listen)
 |
 ├─→ shape38: Set Execution Context Properties
 |   └─→ WRITES: [process.DPP_Process_Name, process.DPP_AtomName, process.DPP_Payload, 
 |                 process.DPP_ExecutionID, process.DPP_File_Name, process.DPP_Subject, 
 |                 process.To_Email, process.DPP_HasAttachment]
 |
 ├─→ TRY/CATCH (shape17)
 |   |
 |   ├─→ TRY BLOCK:
 |   |   |
 |   |   ├─→ shape29: Map D365 to Oracle Fusion Format
 |   |   |   └─→ READS: [Input document]
 |   |   |   └─→ WRITES: [Mapped document]
 |   |   |
 |   |   ├─→ shape8: Set Oracle Fusion API URL
 |   |   |   └─→ READS: [Defined parameter: PP_HCM_LeaveCreate_Properties.Resource_Path]
 |   |   |   └─→ WRITES: [dynamicdocument.URL]
 |   |   |
 |   |   ├─→ shape49: Notify (Log Request)
 |   |   |   └─→ READS: [Current document]
 |   |   |   └─→ HTTP: [N/A - Logging only]
 |   |   |
 |   |   ├─→ shape33: Leave Oracle Fusion Create (Downstream)
 |   |   |   └─→ READS: [dynamicdocument.URL, Mapped document]
 |   |   |   └─→ WRITES: [meta.base.applicationstatuscode, meta.base.applicationstatusmessage, 
 |   |   |                 dynamicdocument.DDP_RespHeader]
 |   |   |   └─→ HTTP: [Expected: 200/201, Error: 400/401/404/500]
 |   |   |
 |   |   ├─→ Decision (shape2): HTTP Status 20* check
 |   |   |   └─→ READS: [meta.base.applicationstatuscode]
 |   |   |   |
 |   |   |   ├─→ IF TRUE (HTTP 20*) → Success Path:
 |   |   |   |   |
 |   |   |   |   ├─→ shape34: Map Oracle Fusion Response to D365 Format
 |   |   |   |   |   └─→ READS: [Oracle Fusion response]
 |   |   |   |   |   └─→ WRITES: [D365 success response]
 |   |   |   |   |
 |   |   |   |   └─→ shape35: Return Documents [HTTP: 200] [SUCCESS]
 |   |   |   |       └─→ Response: {"status": "success", "message": "Data successfully sent to Oracle Fusion", 
 |   |   |   |                     "personAbsenceEntryId": 300100558987654, "success": "true"}
 |   |   |   |
 |   |   |   └─→ IF FALSE (HTTP NOT 20*) → Error Path:
 |   |   |       |
 |   |   |       ├─→ Decision (shape44): Check Response Content-Encoding
 |   |   |       |   └─→ READS: [dynamicdocument.DDP_RespHeader]
 |   |   |       |   |
 |   |   |       |   ├─→ IF TRUE (gzip) → Decompress and Return Error:
 |   |   |       |   |   |
 |   |   |       |   |   ├─→ shape45: Decompress GZIP Response
 |   |   |       |   |   |   └─→ READS: [Gzip-encoded response]
 |   |   |       |   |   |   └─→ WRITES: [Decompressed response]
 |   |   |       |   |   |
 |   |   |       |   |   ├─→ shape46: Extract Error Message
 |   |   |       |   |   |   └─→ READS: [meta.base.applicationstatusmessage]
 |   |   |       |   |   |   └─→ WRITES: [process.DPP_ErrorMessage]
 |   |   |       |   |   |
 |   |   |       |   |   ├─→ shape47: Map Error Response
 |   |   |       |   |   |   └─→ READS: [process.DPP_ErrorMessage]
 |   |   |       |   |   |   └─→ WRITES: [D365 error response]
 |   |   |       |   |   |
 |   |   |       |   |   └─→ shape48: Return Documents [HTTP: 400] [ERROR]
 |   |   |       |   |       └─→ Response: {"status": "failure", "message": "Oracle Fusion API error: ...", 
 |   |   |       |   |                     "success": "false"}
 |   |   |       |   |
 |   |   |       |   └─→ IF FALSE (NOT gzip) → Return Error:
 |   |   |       |       |
 |   |   |       |       ├─→ shape39: Extract Error Message
 |   |   |       |       |   └─→ READS: [meta.base.applicationstatusmessage]
 |   |   |       |       |   └─→ WRITES: [process.DPP_ErrorMessage]
 |   |   |       |       |
 |   |   |       |       ├─→ shape40: Map Error Response
 |   |   |       |       |   └─→ READS: [process.DPP_ErrorMessage]
 |   |   |       |       |   └─→ WRITES: [D365 error response]
 |   |   |       |       |
 |   |   |       |       └─→ shape36: Return Documents [HTTP: 400] [ERROR]
 |   |   |       |           └─→ Response: {"status": "failure", "message": "Oracle Fusion API error: ...", 
 |   |   |       |                         "success": "false"}
 |   |
 |   └─→ CATCH BLOCK (if exception in try block):
 |       |
 |       ├─→ shape20: Branch (SEQUENTIAL - API calls present)
 |           |
 |           ├─→ Path 1 (Email Notification) → Executes FIRST:
 |           |   |
 |           |   ├─→ shape19: Extract Catch Error Message
 |           |   |   └─→ READS: [meta.base.catcherrorsmessage]
 |           |   |   └─→ WRITES: [process.DPP_ErrorMessage]
 |           |   |
 |           |   └─→ shape21: Call Email Subprocess (Downstream)
 |           |       └─→ READS: [process.DPP_ErrorMessage, process.DPP_Process_Name, 
 |           |                   process.DPP_AtomName, process.DPP_ExecutionID, process.DPP_Payload, 
 |           |                   process.DPP_File_Name, process.DPP_Subject, process.To_Email, 
 |           |                   process.DPP_HasAttachment]
 |           |       └─→ HTTP: [Expected: 200, Error: 500]
 |           |       └─→ SUBPROCESS INTERNAL FLOW: (See Section 14)
 |           |
 |           └─→ Path 2 (Error Response) → Executes SECOND:
 |               |
 |               ├─→ shape41: Map Catch Error Response
 |               |   └─→ READS: [process.DPP_ErrorMessage]
 |               |   └─→ WRITES: [D365 error response]
 |               |
 |               └─→ shape43: Return Documents [HTTP: 500] [ERROR]
 |                   └─→ Response: {"status": "failure", "message": "Connection timeout to Oracle Fusion HCM", 
 |                                 "success": "false"}
```

**Note:** Detailed request/response JSON examples for all operations and return paths are documented in Section 16 (Request/Response JSON Examples).

---

## 14. Subprocess Analysis (Step 7a)

### Subprocess: (Sub) Office 365 Email

**Subprocess ID:** a85945c5-3004-42b9-80b1-104f465cd1fb  
**Subprocess Name:** (Sub) Office 365 Email  
**Purpose:** Send error notification email with or without attachment

### Internal Flow

```
START (shape1 - Passthrough)
 |
 ├─→ shape2: Try/Catch
 |   |
 |   ├─→ TRY BLOCK:
 |   |   |
 |   |   ├─→ shape4: Decision - Attachment Check
 |   |   |   └─→ READS: [process.DPP_HasAttachment]
 |   |   |   |
 |   |   |   ├─→ IF TRUE (DPP_HasAttachment = "Y") → With Attachment:
 |   |   |   |   |
 |   |   |   |   ├─→ shape11: Build Email Body (HTML)
 |   |   |   |   |   └─→ READS: [process.DPP_Process_Name, process.DPP_AtomName, 
 |   |   |   |   |                 process.DPP_ExecutionID, process.DPP_ErrorMessage]
 |   |   |   |   |
 |   |   |   |   ├─→ shape14: Set Mail Body Property
 |   |   |   |   |   └─→ WRITES: [process.DPP_MailBody]
 |   |   |   |   |
 |   |   |   |   ├─→ shape15: Set Payload for Attachment
 |   |   |   |   |   └─→ READS: [process.DPP_Payload]
 |   |   |   |   |
 |   |   |   |   ├─→ shape6: Set Mail Properties
 |   |   |   |   |   └─→ READS: [Defined parameter: PP_Office365_Email.From_Email, 
 |   |   |   |   |                 process.To_Email, process.DPP_Subject, process.DPP_MailBody, 
 |   |   |   |   |                 process.DPP_File_Name]
 |   |   |   |   |   └─→ WRITES: [connector.mail.fromAddress, connector.mail.toAddress, 
 |   |   |   |   |                 connector.mail.subject, connector.mail.body, connector.mail.filename]
 |   |   |   |   |
 |   |   |   |   ├─→ shape3: Email w Attachment (Downstream)
 |   |   |   |   |   └─→ HTTP: [Expected: 200, Error: 500]
 |   |   |   |   |
 |   |   |   |   └─→ shape5: Stop (continue=true) [SUCCESS RETURN]
 |   |   |   |
 |   |   |   └─→ IF FALSE (DPP_HasAttachment = "N") → Without Attachment:
 |   |   |       |
 |   |   |       ├─→ shape23: Build Email Body (HTML)
 |   |   |       |   └─→ READS: [process.DPP_Process_Name, process.DPP_AtomName, 
 |   |   |       |                 process.DPP_ExecutionID, process.DPP_ErrorMessage]
 |   |   |       |
 |   |   |       ├─→ shape22: Set Mail Body Property
 |   |   |       |   └─→ WRITES: [process.DPP_MailBody]
 |   |   |       |
 |   |   |       ├─→ shape20: Set Mail Properties
 |   |   |       |   └─→ READS: [Defined parameter: PP_Office365_Email.From_Email, 
 |   |   |       |                 process.To_Email, process.DPP_Subject, process.DPP_MailBody]
 |   |   |       |   └─→ WRITES: [connector.mail.fromAddress, connector.mail.toAddress, 
 |   |   |       |                 connector.mail.subject, connector.mail.body]
 |   |   |       |
 |   |   |       ├─→ shape7: Email W/O Attachment (Downstream)
 |   |   |       |   └─→ HTTP: [Expected: 200, Error: 500]
 |   |   |       |
 |   |   |       └─→ shape9: Stop (continue=true) [SUCCESS RETURN]
 |   |
 |   └─→ CATCH BLOCK (if exception in try block):
 |       |
 |       └─→ shape10: Exception
 |           └─→ READS: [meta.base.catcherrorsmessage]
 |           └─→ Throws exception with error message [ERROR RETURN]
```

### Return Paths

| Return Label | Return Type | Condition | Main Process Mapping |
|---|---|---|---|
| SUCCESS | Stop (continue=true) | Email sent successfully (with or without attachment) | No explicit return path (subprocess completes) |
| ERROR | Exception | Email send failed | Exception thrown, caught by main process try/catch |

### Properties Written by Subprocess

| Property Name | Written By | Value |
|---|---|---|
| process.DPP_MailBody | shape14, shape22 | HTML email body with execution details |
| connector.mail.fromAddress | shape6, shape20 | From email address |
| connector.mail.toAddress | shape6, shape20 | To email address |
| connector.mail.subject | shape6, shape20 | Email subject line |
| connector.mail.body | shape6, shape20 | Email body content |
| connector.mail.filename | shape6 | Attachment filename (only if hasAttachment = Y) |

### Properties Read by Subprocess

| Property Name | Read From | Usage |
|---|---|---|
| process.DPP_HasAttachment | Main process (shape38) | Determines if email has attachment |
| process.DPP_Process_Name | Main process (shape38) | Included in email body |
| process.DPP_AtomName | Main process (shape38) | Included in email body |
| process.DPP_ExecutionID | Main process (shape38) | Included in email body |
| process.DPP_ErrorMessage | Main process (shape19) | Included in email body |
| process.To_Email | Main process (shape38) | Email recipient |
| process.DPP_Subject | Main process (shape38) | Email subject line |
| process.DPP_Payload | Main process (shape38) | Email attachment content |
| process.DPP_File_Name | Main process (shape38) | Email attachment filename |

---

## 15. System Layer Identification

### Downstream Systems

#### System 1: Oracle Fusion HCM

**System Name:** Oracle Fusion Human Capital Management  
**Connection:** Oracle Fusion (aa1fcb29-d146-4425-9ea6-b9698090f60e)  
**Protocol:** HTTP/REST  
**Base URL:** https://iaaxey-dev3.fa.ocs.oraclecloud.com:443  
**Authentication:** Basic Authentication  
**API Endpoint:** /hcmRestApi/resources/11.13.18.05/absences  
**Purpose:** Create leave absence entries in Oracle Fusion HCM

**Operations:**
- **Leave Oracle Fusion Create (6e8920fd-af5a-430b-a1d9-9fde7ac29a12):** HTTP POST to create absence

**Request Format:** JSON
**Response Format:** JSON

**System Layer Classification:** System Layer API (unlocks data from Oracle Fusion HCM)

#### System 2: Office 365 Email (SMTP)

**System Name:** Office 365 Email Service  
**Connection:** Office 365 Email (00eae79b-2303-4215-8067-dcc299e42697)  
**Protocol:** SMTP  
**Host:** smtp-mail.outlook.com:587  
**Authentication:** SMTP AUTH with TLS  
**Purpose:** Send error notification emails to integration team

**Operations:**
- **Email w Attachment (af07502a-fafd-4976-a691-45d51a33b549):** Send email with attachment
- **Email W/O Attachment (15a72a21-9b57-49a1-a8ed-d70367146644):** Send email without attachment

**System Layer Classification:** System Layer API (unlocks email notification capability)

### Process Layer Function

**Function Name:** HCM Leave Create  
**Business Domain:** Human Resource Management  
**Purpose:** Synchronize leave data between D365 and Oracle Fusion HCM  
**Orchestration:** Receives leave request from D365, transforms data, creates absence in Oracle Fusion HCM, handles errors with email notifications

**Process Layer Classification:** Process Layer API (orchestrates data between D365 and Oracle Fusion HCM)

---

## 16. Request/Response JSON Examples

### Process Layer Entry Point

#### Request JSON Example

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

#### Response JSON Examples

**Success Response (HTTP 200):**

```json
{
  "leaveResponse": {
    "status": "success",
    "message": "Data successfully sent to Oracle Fusion",
    "personAbsenceEntryId": 300100558987654,
    "success": "true"
  }
}
```

**Error Response - HTTP Error (HTTP 400):**

```json
{
  "leaveResponse": {
    "status": "failure",
    "message": "Oracle Fusion API error: Invalid absence type 'Sick Leave' for employee 9000604",
    "success": "false"
  }
}
```

**Error Response - Exception (HTTP 500):**

```json
{
  "leaveResponse": {
    "status": "failure",
    "message": "Connection timeout to Oracle Fusion HCM",
    "success": "false"
  }
}
```

### Downstream System Layer Calls

#### Operation: Leave Oracle Fusion Create (6e8920fd-af5a-430b-a1d9-9fde7ac29a12)

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

**Response JSON (Success - HTTP 200):**

```json
{
  "personAbsenceEntryId": 300100558987654,
  "absenceCaseId": "ABS-2024-001234",
  "absenceStatusCd": "SUBMITTED",
  "approvalStatusCd": "APPROVED",
  "personNumber": "9000604",
  "absenceType": "Sick Leave",
  "startDate": "2024-03-24",
  "endDate": "2024-03-25",
  "startDateDuration": 1,
  "endDateDuration": 1,
  "duration": 2,
  "createdBy": "INTEGRATION.USER@al-ghurair.com",
  "creationDate": "2024-03-24T10:30:00Z"
}
```

**Response JSON (Error - HTTP 400):**

```json
{
  "title": "Bad Request",
  "status": 400,
  "detail": "Invalid absence type 'Sick Leave' for employee 9000604. Valid absence types are: Annual Leave, Sick Leave - Certified, Emergency Leave.",
  "o:errorCode": "INVALID_ABSENCE_TYPE"
}
```

**Response JSON (Error - HTTP 404):**

```json
{
  "title": "Not Found",
  "status": 404,
  "detail": "Employee with personNumber 9000604 not found in Oracle Fusion HCM.",
  "o:errorCode": "EMPLOYEE_NOT_FOUND"
}
```

**Response JSON (Error - HTTP 500):**

```json
{
  "title": "Internal Server Error",
  "status": 500,
  "detail": "An internal error occurred while processing the absence creation request.",
  "o:errorCode": "INTERNAL_ERROR"
}
```

---

## 17. Function Exposure Decision Table

### ✅ Function Exposure Decision Checkpoint
**Status:** COMPLETE

| Function Name | Business Domain | Expose as Azure Function? | Reasoning | Alternative |
|---|---|---|---|---|
| HCM Leave Create | Human Resource Management | ✅ YES | **Primary integration endpoint** - Receives leave requests from D365 and orchestrates creation in Oracle Fusion HCM. This is a core business process that must be exposed as an HTTP-triggered Azure Function. | N/A - This is the main entry point |
| (Sub) Office 365 Email | Notification/Alerting | ❌ NO | **Internal utility subprocess** - Used only for error notifications within the main process. Should be implemented as an internal helper method/class, not a separate function. | Implement as `EmailNotificationService` class with `SendErrorNotificationAsync()` method |

**Function Explosion Prevention:**

- **Total Functions to Expose:** 1 (HCM Leave Create only)
- **Subprocesses as Internal Methods:** 1 ((Sub) Office 365 Email as helper class)

**Rationale:**

1. **HCM Leave Create** is the primary business process that external systems (D365) need to invoke. It must be exposed as an Azure Function with HTTP trigger.

2. **(Sub) Office 365 Email** is an internal utility used only for error notifications. Exposing it as a separate function would create unnecessary complexity and management overhead. It should be implemented as an internal service class that the main function calls when needed.

**Azure Function Architecture:**

```
Azure Function: HCM-Leave-Create (HTTP Trigger)
├── Process Layer: HCMLeaveCreateFunction.cs
├── System Layer: OracleFusionHCMService.cs
└── Utility: EmailNotificationService.cs (internal helper)
```

---

## 18. Critical Patterns Identified

### Pattern 1: Try/Catch Error Handling with Email Notification

**Identification:**
- Try/Catch shape (shape17) wraps main business logic
- Catch block branches to email notification and error response
- Email subprocess sends error details to integration team
- Error response returned to caller

**Execution Rule:**
- **Try block executes main business logic** (map, HTTP POST, check status)
- **If exception thrown:** Catch block executes
  - Branch path 1 (sequential): Send error notification email
  - Branch path 2 (sequential): Return error response to caller

**Sequence Format:**

```
TRY:
  ├─→ Map Request
  ├─→ Set URL
  ├─→ HTTP POST to Oracle Fusion (Downstream)
  └─→ Check HTTP Status
CATCH (if exception):
  ├─→ Extract Error Message
  ├─→ Branch (SEQUENTIAL - API calls present):
      ├─→ Path 1: Send Error Notification Email (Downstream) [Executes FIRST]
      └─→ Path 2: Return Error Response [Executes SECOND]
```

### Pattern 2: HTTP Status Code Check with Multiple Error Paths

**Identification:**
- Decision shape (shape2) checks HTTP status code from Oracle Fusion response
- TRUE path (HTTP 20*): Success - Map response and return
- FALSE path (HTTP NOT 20*): Error - Check encoding, extract error, return error response

**Execution Rule:**
- **HTTP operation MUST execute BEFORE decision** (produces status code)
- **Decision routes based on HTTP status:**
  - Success (20*): Map Oracle Fusion response → Return success
  - Error (NOT 20*): Check encoding → Extract error → Return error

**Sequence Format:**

```
HTTP POST to Oracle Fusion (Downstream)
 └─→ WRITES: [meta.base.applicationstatuscode]
 |
 ├─→ Decision: HTTP Status 20* check
 | └─→ READS: [meta.base.applicationstatuscode]
 | |
 | ├─→ IF TRUE (20*) → Map Success Response → Return Success [HTTP 200]
 | |
 | └─→ IF FALSE (NOT 20*) → Check Encoding → Extract Error → Return Error [HTTP 400]
```

### Pattern 3: Response Encoding Check with Conditional Decompression

**Identification:**
- Decision shape (shape44) checks response Content-Encoding header
- TRUE path (gzip): Decompress response before extracting error
- FALSE path (NOT gzip): Extract error directly

**Execution Rule:**
- **HTTP operation extracts response header** (Content-Encoding)
- **Decision routes based on encoding:**
  - gzip: Decompress → Extract error → Return error
  - NOT gzip: Extract error → Return error

**Sequence Format:**

```
HTTP POST to Oracle Fusion (Downstream)
 └─→ WRITES: [dynamicdocument.DDP_RespHeader]
 |
 ├─→ Decision: Check Response Content-Encoding
 | └─→ READS: [dynamicdocument.DDP_RespHeader]
 | |
 | ├─→ IF TRUE (gzip) → Decompress → Extract Error → Map Error → Return Error [HTTP 400]
 | |
 | └─→ IF FALSE (NOT gzip) → Extract Error → Map Error → Return Error [HTTP 400]
```

### Pattern 4: Sequential Branch Execution (API Calls Present)

**Identification:**
- Branch shape (shape20) in catch block with 2 paths
- Path 1 contains email operation (API call)
- Path 2 returns error response

**Execution Rule:**
- **Branch paths execute SEQUENTIALLY** (API calls present in Path 1)
- **Path 1 executes FIRST:** Send error notification email
- **Path 2 executes SECOND:** Return error response to caller

**Sequence Format:**

```
Branch (SEQUENTIAL - API calls present):
 |
 ├─→ Path 1 (Executes FIRST): Send Error Notification Email (Downstream)
 |   └─→ HTTP: [Expected: 200, Error: 500]
 |
 └─→ Path 2 (Executes SECOND): Map Error Response → Return Error [HTTP 500]
```

---

## 19. Validation Checklist

### ✅ Data Dependencies

- [x] All property WRITES identified (Section 7)
- [x] All property READS identified (Section 7)
- [x] Dependency graph built (Section 8)
- [x] Execution order satisfies all dependencies (no read-before-write) (Section 12)

### ✅ Decision Analysis

- [x] ALL decision shapes inventoried (Section 10)
- [x] BOTH TRUE and FALSE paths traced to termination (Section 10)
- [x] Pattern type identified for each decision (Section 10)
- [x] Early exits identified and documented (Section 10)
- [x] Convergence points identified (if paths rejoin) (Section 10)
- [x] Decision data source analysis documented (Section 10)
- [x] Decision types classified (PRE_FILTER or POST_OPERATION) (Section 10)
- [x] Actual execution order verified (Section 10)

### ✅ Branch Analysis

- [x] Each branch classified as parallel or sequential (Section 11)
- [x] API calls checked in branch paths (Section 11)
- [x] Classification: SEQUENTIAL (API calls present) (Section 11)
- [x] If sequential: dependency_order built using topological sort (Section 11)
- [x] Each path traced to terminal point (Section 11)
- [x] Convergence points identified (Section 11)
- [x] Execution continuation point determined (Section 11)

### ✅ Sequence Diagram

- [x] Format follows required structure (Operation → Decision → Operation) (Section 13)
- [x] Each operation shows READS and WRITES (Section 13)
- [x] Decisions show both TRUE and FALSE paths (Section 13)
- [x] Try/Catch patterns shown correctly (Section 13)
- [x] Cross-validation: Sequence diagram matches control flow graph from Step 5 (Section 13)
- [x] Cross-validation: Execution order matches dependency graph from Step 4 (Section 13)
- [x] Early exits marked [EARLY EXIT] (N/A - No early exits in this process)
- [x] Conditional execution marked [Only if condition X] (Section 13)
- [x] Subprocess internal flows documented (Section 14)
- [x] Subprocess return paths mapped to main process (Section 14)

### ✅ Subprocess Analysis

- [x] ALL subprocesses analyzed (internal flow traced) (Section 14)
- [x] Return paths identified (success and error) (Section 14)
- [x] Return path labels mapped to main process shapes (Section 14)
- [x] Properties written by subprocess documented (Section 14)
- [x] Properties read by subprocess from main process documented (Section 14)

### ✅ Edge Cases

- [x] Nested branches/decisions analyzed (N/A - No nested branches)
- [x] Loops identified (if any) with exit conditions (N/A - No loops)
- [x] Property chains traced (transitive dependencies) (Section 8)
- [x] Circular dependencies detected and resolved (None found)
- [x] Try/Catch error paths documented (Section 13)

### ✅ Property Extraction Completeness

- [x] All property patterns searched (${}, %%, {}) (Section 7)
- [x] Message parameters checked for process properties (Section 7)
- [x] Operation headers/path parameters checked (Section 7)
- [x] Decision track properties identified (meta.*) (Section 7)
- [x] Document properties that read other properties identified (Section 7)

### ✅ Input/Output Structure Analysis (CONTRACT VERIFICATION)

- [x] Entry point operation identified (Section 2)
- [x] Request profile identified and loaded (Section 2)
- [x] Request profile structure analyzed (JSON/XML) (Section 2)
- [x] Array vs single object detected (Section 2)
- [x] Array cardinality documented (minOccurs, maxOccurs) (N/A - Single object)
- [x] ALL request fields extracted (including nested structures) (Section 2)
- [x] Request field paths documented (full Boomi paths) (Section 2)
- [x] Request field mapping table generated (Boomi → Azure DTO) (Section 2)
- [x] Response profile identified and loaded (Section 3)
- [x] Response profile structure analyzed (Section 3)
- [x] ALL response fields extracted (Section 3)
- [x] Response field mapping table generated (Section 3)
- [x] Document processing behavior determined (splitting vs batch) (Section 2)
- [x] Input/Output structure documented in Phase 1 document (Sections 2 & 3)

### ✅ HTTP Status Codes and Return Path Responses

- [x] Section 6 (HTTP Status Codes and Return Path Responses - Step 1e) present
- [x] All return paths documented with HTTP status codes (Section 6)
- [x] Response JSON examples provided for each return path (Section 6)
- [x] Populated fields documented for each return path (source and populated by) (Section 6)
- [x] Decision conditions leading to each return documented (Section 6)
- [x] Error codes and success codes documented for each return path (Section 6)
- [x] Downstream operation HTTP status codes documented (expected success and error codes) (Section 6)
- [x] Error handling strategy documented for downstream operations (Section 6)

### ✅ Request/Response JSON Examples

- [x] Section 16 (Request/Response JSON Examples) present
- [x] Process Layer entry point request JSON example provided (Section 16)
- [x] Process Layer response JSON examples provided (all return paths) (Section 16)
- [x] Downstream System Layer request JSON examples provided (all operations) (Section 16)
- [x] Downstream System Layer response JSON examples provided (all operations - success and error scenarios) (Section 16)

### ✅ Map Analysis

- [x] ALL map files identified and loaded (Section 5)
- [x] HTTP request maps identified (maps to operation request profiles) (Section 5)
- [x] Field mappings extracted from each map (Section 5)
- [x] Profile vs map field name discrepancies documented (Section 5)
- [x] Map field names marked as AUTHORITATIVE for HTTP requests (Section 5)
- [x] Scripting functions analyzed (date formatting, concatenation, etc.) (Section 5)
- [x] Static values identified and documented (Section 5)
- [x] Process property mappings documented (Section 5)
- [x] Map Analysis documented in Phase 1 document (Section 5)

### ✅ Function Exposure Decision

- [x] Function Exposure Decision Table complete (Section 17)
- [x] Primary business process identified for exposure (Section 17)
- [x] Subprocesses classified as internal helpers (Section 17)
- [x] Function explosion prevented (Section 17)

### ✅ Self-Check Questions

**Step 7 (Decision Analysis):**
- [x] Did I identify data source for EVERY decision? (Answer: YES)
- [x] Did I classify each decision type? (Answer: YES)
- [x] Did I verify actual execution order for PRE-FILTER decisions? (Answer: YES)
- [x] Did I trace BOTH TRUE and FALSE paths for EVERY decision? (Answer: YES)
- [x] Did I identify the pattern for each decision? (Answer: YES)
- [x] Did I trace paths to termination? (Answer: YES)

**Step 8 (Branch Analysis):**
- [x] Did I classify each branch as parallel or sequential? (Answer: YES)
- [x] Did I assume branches are parallel? (Answer: NO - analyzed dependencies and API calls)
- [x] Did I extract properties read/written by each path? (Answer: YES)
- [x] Did I build dependency graph between paths? (Answer: YES)
- [x] Did I apply topological sort if sequential? (Answer: YES)

**Step 9 (Execution Order):**
- [x] Did I verify business logic FIRST before following dragpoints? (Answer: YES)
- [x] Did I identify what each operation does and what it produces? (Answer: YES)
- [x] Did I identify which operations MUST execute first based on business logic? (Answer: YES)
- [x] Did I check data dependencies FIRST before following dragpoints? (Answer: YES)
- [x] Did I use operation response analysis from Step 1c? (Answer: YES)
- [x] Did I use decision analysis from Step 7? (Answer: YES)
- [x] Did I use dependency graph from Step 4? (Answer: YES)
- [x] Did I use branch analysis from Step 8? (Answer: YES)
- [x] Did I verify all property reads happen after property writes? (Answer: YES)
- [x] Did I follow topological sort order for sequential branches? (Answer: YES)

### ✅ Final Validation

- [x] All Steps 1-10 completed in order
- [x] All validation checklists completed
- [x] All "NEVER ASSUME" self-checks answered YES
- [x] Sequence diagram cross-checked against JSON dragpoints
- [x] Execution order verified against dependency graph
- [x] Phase 1 document contains all mandatory sections
- [x] All self-check answers shown in Phase 1 document with YES answers

---

## Summary

**Phase 1 Extraction Status:** ✅ COMPLETE

**Process Summary:**
- **Process Name:** HCM_Leave Create
- **Business Domain:** Human Resource Management (HCM)
- **Purpose:** Synchronize leave data between D365 and Oracle Fusion HCM
- **Entry Point:** Web Service Server Listen (HTTP trigger)
- **Downstream Systems:** Oracle Fusion HCM (REST API), Office 365 Email (SMTP)
- **Error Handling:** Try/Catch with email notifications and error responses
- **Return Paths:** 4 return paths (1 success, 3 error scenarios)

**Key Findings:**
1. **Single object processing** - No array splitting required
2. **Sequential branch execution** - Branch in catch block executes sequentially due to API calls
3. **Multiple error paths** - HTTP errors, gzip-encoded errors, and exception errors handled separately
4. **Email notifications** - Error notifications sent to integration team via subprocess
5. **Field name transformations** - D365 field names differ from Oracle Fusion field names (e.g., employeeNumber → personNumber)

**Ready for Phase 2:** ✅ YES - All mandatory sections complete, all validation checkpoints passed

---

**Document Version:** 1.0  
**Created By:** AI Agent  
**Date:** 2025-02-18  
**Status:** Complete and Ready for Phase 2 Code Generation
