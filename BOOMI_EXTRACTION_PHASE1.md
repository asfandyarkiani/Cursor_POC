# BOOMI EXTRACTION PHASE 1 - HCM Leave Create Process

**Process Name:** HCM_Leave Create  
**Process ID:** ca69f858-785f-4565-ba1f-b4edc6cca05b  
**Description:** This Process will sync the Leave data between D365 and Oracle HCM  
**Business Domain:** Human Resource Management (HCM)  
**Extraction Date:** 2026-02-12

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
15. [System Layer Identification](#15-system-layer-identification)
16. [Request/Response JSON Examples](#16-requestresponse-json-examples)
17. [Function Exposure Decision Table](#17-function-exposure-decision-table)

---

## 1. Operations Inventory

### Main Process Operations

| Operation ID | Operation Name | Type | Sub-Type | Purpose |
|---|---|---|---|---|
| 8f709c2b-e63f-4d5f-9374-2932ed70415d | Create Leave Oracle Fusion OP | connector-action | wss | Web Service Server - Entry point for D365 leave requests |
| 6e8920fd-af5a-430b-a1d9-9fde7ac29a12 | Leave Oracle Fusion Create | connector-action | http | HTTP POST to Oracle Fusion HCM to create leave |
| af07502a-fafd-4976-a691-45d51a33b549 | Email w Attachment | connector-action | mail | Send email with attachment |
| 15a72a21-9b57-49a1-a8ed-d70367146644 | Email W/O Attachment | connector-action | mail | Send email without attachment |

### Connections

| Connection ID | Connection Name | Type | Purpose |
|---|---|---|---|
| aa1fcb29-d146-4425-9ea6-b9698090f60e | Oracle Fusion | http | Connection to Oracle Fusion HCM (https://iaaxey-dev3.fa.ocs.oraclecloud.com:443) |
| 00eae79b-2303-4215-8067-dcc299e42697 | Office 365 Email | mail | SMTP connection for email notifications (smtp-mail.outlook.com:587) |

### Profiles

| Profile ID | Profile Name | Type | Purpose |
|---|---|---|---|
| febfa3e1-f719-4ee8-ba57-cdae34137ab3 | D365 Leave Create JSON Profile | profile.json | Request profile - D365 leave data structure |
| a94fa205-c740-40a5-9fda-3d018611135a | HCM Leave Create JSON Profile | profile.json | Oracle Fusion HCM leave request structure |
| 316175c7-0e45-4869-9ac6-5f9d69882a62 | Oracle Fusion Leave Response JSON Profile | profile.json | Oracle Fusion HCM leave response structure |
| f4ca3a70-114a-4601-bad8-44a3eb20e2c0 | Leave D365 Response | profile.json | Response profile - D365 response structure |
| 23d7a2e9-5cb0-4e9c-9e4b-3154834bad0d | Dummy FF Profile | profile.flatfile | Dummy flat file profile for error mapping |

### Maps

| Map ID | Map Name | From Profile | To Profile | Purpose |
|---|---|---|---|---|
| c426b4d6-2aff-450e-b43b-59956c4dbc96 | Leave Create Map | D365 Leave Create JSON Profile | HCM Leave Create JSON Profile | Transform D365 format to Oracle Fusion format |
| e4fd3f59-edb5-43a1-aeae-143b600a064e | Oracle Fusion Leave Response Map | Oracle Fusion Leave Response JSON Profile | Leave D365 Response | Transform Oracle Fusion response to D365 response format |
| f46b845a-7d75-41b5-b0ad-c41a6a8e9b12 | Leave Error Map | Dummy FF Profile | Leave D365 Response | Map error messages to D365 response format |

---

## 2. Process Properties Analysis

### Property WRITES

| Property Name | Written By Shape(s) | Source Type | Purpose |
|---|---|---|---|
| process.DPP_Process_Name | shape38 | execution | Stores process name for logging/email |
| process.DPP_AtomName | shape38 | execution | Stores atom name for logging/email |
| process.DPP_Payload | shape38 | current | Stores current document (input payload) |
| process.DPP_ExecutionID | shape38 | execution | Stores execution ID for tracking |
| process.DPP_File_Name | shape38 | execution + date + static | Constructs filename: ProcessName + timestamp + .txt |
| process.DPP_Subject | shape38 | execution + static | Constructs email subject: AtomName (ProcessName) has errors to report |
| process.To_Email | shape38 | definedparameter | Email recipient from process properties |
| process.DPP_HasAttachment | shape38 | definedparameter | Flag indicating if email should have attachment |
| process.DPP_ErrorMessage | shape19, shape39, shape46 | track | Error message from try/catch or HTTP response |
| dynamicdocument.URL | shape8 | definedparameter | Dynamic URL for Oracle Fusion API call |

### Property READS

| Property Name | Read By Shape(s) | Usage |
|---|---|---|---|
| process.DPP_Process_Name | shape11 (subprocess), shape23 (subprocess) | Email body content |
| process.DPP_AtomName | shape11 (subprocess), shape23 (subprocess) | Email body content |
| process.DPP_ExecutionID | shape11 (subprocess), shape23 (subprocess) | Email body content |
| process.DPP_ErrorMessage | shape11 (subprocess), shape23 (subprocess), map_f46b845a | Error details in email/response |
| process.DPP_Payload | shape15 (subprocess) | Attachment content for email |
| process.DPP_File_Name | shape6 (subprocess) | Email attachment filename |
| process.To_Email | shape6 (subprocess), shape20 (subprocess) | Email recipient |
| process.DPP_Subject | shape6 (subprocess), shape20 (subprocess) | Email subject |
| process.DPP_MailBody | shape6 (subprocess), shape20 (subprocess) | Email body content |
| process.DPP_HasAttachment | shape4 (subprocess) | Decision to send email with/without attachment |
| dynamicdocument.URL | shape33 (operation) | HTTP request URL |

### Defined Process Properties

**Component: PP_HCM_LeaveCreate_Properties (e22c04db-7dfa-4b1b-9d88-77365b0fdb18)**

| Property Key | Property Label | Default Value | Purpose |
|---|---|---|---|
| c2d1a1e8-4bcb-435b-b1d2-bb10a209bcc0 | Resource_Path | hcmRestApi/resources/11.13.18.05/absences | Oracle Fusion HCM API resource path |
| 71c90f6e-4f86-49f3-8aef-bae6668c73f9 | To_Email | BoomiIntegrationTeam@al-ghurair.com | Error notification email recipient |
| a717867b-67f4-4a72-be2d-c99c73309fdb | DPP_HasAttachment | Y | Flag to include attachment in error emails |

**Component: PP_Office365_Email (0feff13f-8a2c-438a-b3c7-1909e2a7f533)**

| Property Key | Property Label | Default Value | Purpose |
|---|---|---|---|
| 804b6d40-eeee-4ac0-ab4b-94e1aca9f4b7 | From_Email | Boomi.Dev.failures@al-ghurair.com | Email sender address |
| d8fc7496-ec2c-4308-a4ca-e9f49c0c9500 | Environment | DEV Failure : | Environment prefix for email subject |

---

## 3. Input Structure Analysis (Step 1a)

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

- **Boomi Processing:** Single JSON document processed as single execution
- **Azure Function Requirement:** Must accept single leave request object
- **Implementation Pattern:** Process single leave request, call Oracle Fusion API, return response

### Request Field Mapping

| Boomi Field Path | Boomi Field Name | Data Type | Required | Azure DTO Property | Notes |
|---|---|---|---|---|---|
| Root/Object/employeeNumber | employeeNumber | number | Yes | EmployeeNumber | Employee identifier from D365 |
| Root/Object/absenceType | absenceType | character | Yes | AbsenceType | Type of leave (Sick Leave, Annual Leave, etc.) |
| Root/Object/employer | employer | character | Yes | Employer | Employer name |
| Root/Object/startDate | startDate | character | Yes | StartDate | Leave start date (YYYY-MM-DD) |
| Root/Object/endDate | endDate | character | Yes | EndDate | Leave end date (YYYY-MM-DD) |
| Root/Object/absenceStatusCode | absenceStatusCode | character | Yes | AbsenceStatusCode | Status code (SUBMITTED, APPROVED, etc.) |
| Root/Object/approvalStatusCode | approvalStatusCode | character | Yes | ApprovalStatusCode | Approval status code |
| Root/Object/startDateDuration | startDateDuration | number | Yes | StartDateDuration | Duration for start date (1 = full day) |
| Root/Object/endDateDuration | endDateDuration | number | Yes | EndDateDuration | Duration for end date (1 = full day) |

**Total Fields:** 9 fields (all required)

**✅ Step 1a Complete: Input structure analyzed and documented**

---

## 4. Response Structure Analysis (Step 1b)

### Response Profile Structure

**Profile ID:** f4ca3a70-114a-4601-bad8-44a3eb20e2c0  
**Profile Name:** Leave D365 Response  
**Profile Type:** profile.json  
**Root Structure:** leaveResponse/Object  
**Array Detection:** ❌ NO - Single object structure

### Response Format

```json
{
  "leaveResponse": {
    "status": "success",
    "message": "Data successfully sent to Oracle Fusion",
    "personAbsenceEntryId": 300000123456789,
    "success": "true"
  }
}
```

### Response Field Mapping

| Boomi Field Path | Boomi Field Name | Data Type | Azure DTO Property | Notes |
|---|---|---|---|---|
| leaveResponse/Object/status | status | character | Status | Success or failure status |
| leaveResponse/Object/message | message | character | Message | Success/error message |
| leaveResponse/Object/personAbsenceEntryId | personAbsenceEntryId | number | PersonAbsenceEntryId | Oracle Fusion leave entry ID (returned on success) |
| leaveResponse/Object/success | success | character | Success | Boolean flag as string ("true"/"false") |

**Total Fields:** 4 fields

**✅ Step 1b Complete: Response structure analyzed and documented**

---

## 5. Operation Response Analysis (Step 1c)

### Operation: Leave Oracle Fusion Create (6e8920fd-af5a-430b-a1d9-9fde7ac29a12)

**Operation Type:** HTTP POST  
**Response Profile:** NONE (responseProfileType: NONE)  
**Response Handling:** returnResponses: true, returnErrors: true  

**Response Header Mapping:**
- Header: Content-Encoding → Property: dynamicdocument.DDP_RespHeader

**Extracted Fields:**
- **meta.base.applicationstatuscode** (track property) - HTTP status code from response
  - Extracted by: Decision shape2 (checks for 20* status)
  - Written to: Track property (not process property)
  - Consumers: shape2 (decision), shape44 (decision)

- **meta.base.applicationstatusmessage** (track property) - HTTP response message
  - Extracted by: shape39, shape46 (documentproperties)
  - Written to: process.DPP_ErrorMessage
  - Consumers: shape11, shape23 (email body), map_f46b845a (error map)

- **dynamicdocument.DDP_RespHeader** (document property) - Content-Encoding header
  - Extracted by: Operation response header mapping
  - Consumers: shape44 (decision - checks if "gzip")

**Business Logic Implications:**

1. **Operation: Leave Oracle Fusion Create** produces:
   - HTTP status code (meta.base.applicationstatuscode)
   - HTTP response message (meta.base.applicationstatusmessage)
   - Response body (JSON from Oracle Fusion)
   - Content-Encoding header (dynamicdocument.DDP_RespHeader)

2. **Decision shape2** checks HTTP status code:
   - TRUE path (20*): Success - map response and return success
   - FALSE path (non-20*): Error - check if gzipped, extract error, return error

3. **Decision shape44** checks Content-Encoding:
   - TRUE path (gzip): Decompress response, extract error message
   - FALSE path (not gzip): Extract error message directly

**Execution Order Implications:**
- shape33 (Leave Oracle Fusion Create) MUST execute BEFORE shape2 (HTTP Status decision)
- shape2 MUST execute BEFORE shape34 (success map) or shape44 (error check)
- shape44 MUST execute BEFORE shape45 (decompress) or shape39 (extract error)

**✅ Step 1c Complete: Operation response analysis documented**

---

## 6. Map Analysis (Step 1d)

### Map Inventory

| Map ID | Map Name | From Profile | To Profile | Type |
|---|---|---|---|
| c426b4d6-2aff-450e-b43b-59956c4dbc96 | Leave Create Map | febfa3e1 (D365) | a94fa205 (Oracle Fusion) | Request transformation |
| e4fd3f59-edb5-43a1-aeae-143b600a064e | Oracle Fusion Leave Response Map | 316175c7 (Oracle Fusion Response) | f4ca3a70 (D365 Response) | Success response |
| f46b845a-7d75-41b5-b0ad-c41a6a8e9b12 | Leave Error Map | 23d7a2e9 (Dummy) | f4ca3a70 (D365 Response) | Error response |

### Map 1: Leave Create Map (c426b4d6-2aff-450e-b43b-59956c4dbc96)

**From Profile:** febfa3e1-f719-4ee8-ba57-cdae34137ab3 (D365 Leave Create JSON Profile)  
**To Profile:** a94fa205-c740-40a5-9fda-3d018611135a (HCM Leave Create JSON Profile)  
**Type:** JSON to JSON transformation for Oracle Fusion API request

**Field Mappings:**

| Source Field | Source Type | Target Field | Profile Match? | Notes |
|---|---|---|---|---|
| employeeNumber | profile | personNumber | ❌ DIFFERENT | Field name changed |
| absenceType | profile | absenceType | ✅ MATCH | Direct mapping |
| employer | profile | employer | ✅ MATCH | Direct mapping |
| startDate | profile | startDate | ✅ MATCH | Direct mapping |
| endDate | profile | endDate | ✅ MATCH | Direct mapping |
| absenceStatusCode | profile | absenceStatusCd | ❌ DIFFERENT | Field name shortened |
| approvalStatusCode | profile | approvalStatusCd | ❌ DIFFERENT | Field name shortened |
| startDateDuration | profile | startDateDuration | ✅ MATCH | Direct mapping |
| endDateDuration | profile | endDateDuration | ✅ MATCH | Direct mapping |

**Profile vs Map Discrepancies:**

| D365 Profile Field | Oracle Fusion Profile Field | Authority | Use in Request |
|---|---|---|---|
| employeeNumber | personNumber | ✅ MAP | personNumber |
| absenceStatusCode | absenceStatusCd | ✅ MAP | absenceStatusCd |
| approvalStatusCode | approvalStatusCd | ✅ MAP | approvalStatusCd |

**Scripting Functions:** None

**CRITICAL RULE:** Map field names are AUTHORITATIVE. Use map field names in Oracle Fusion API request.

### Map 2: Oracle Fusion Leave Response Map (e4fd3f59-edb5-43a1-aeae-143b600a064e)

**From Profile:** 316175c7-0e45-4869-9ac6-5f9d69882a62 (Oracle Fusion Leave Response JSON Profile)  
**To Profile:** f4ca3a70-114a-4601-bad8-44a3eb20e2c0 (Leave D365 Response)  
**Type:** Success response transformation

**Field Mappings:**

| Source Field | Source Type | Target Field | Notes |
|---|---|---|---|
| personAbsenceEntryId | profile | personAbsenceEntryId | Oracle Fusion leave entry ID |

**Default Values:**

| Target Field | Default Value | Purpose |
|---|---|---|
| status | "success" | Indicates successful processing |
| message | "Data successfully sent to Oracle Fusion" | Success message |
| success | "true" | Boolean success flag |

### Map 3: Leave Error Map (f46b845a-7d75-41b5-b0ad-c41a6a8e9b12)

**From Profile:** 23d7a2e9-5cb0-4e9c-9e4b-3154834bad0d (Dummy FF Profile)  
**To Profile:** f4ca3a70-114a-4601-bad8-44a3eb20e2c0 (Leave D365 Response)  
**Type:** Error response transformation

**Field Mappings:**

| Source Field | Source Type | Target Field | Notes |
|---|---|---|---|
| process.DPP_ErrorMessage | function (PropertyGet) | message | Error message from process property |

**Function Analysis:**

| Function | Type | Input | Output | Logic |
|---|---|---|---|---|
| Function 1 | PropertyGet | Property Name: "DPP_ErrorMessage" | Result | Retrieves error message from process property |

**Default Values:**

| Target Field | Default Value | Purpose |
|---|---|---|
| status | "failure" | Indicates failed processing |
| success | "false" | Boolean failure flag |

**✅ Step 1d Complete: Map analysis documented**

---

## 7. HTTP Status Codes and Return Path Responses (Step 1e)

### Return Path Analysis

#### Return Path 1: Success Response (shape35)

**Return Label:** "Success Response"  
**Return Shape ID:** shape35  
**HTTP Status Code:** 200  
**Decision Conditions Leading to Return:**
- Decision shape2: meta.base.applicationstatuscode matches "20*" → TRUE path

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
  "leaveResponse": {
    "status": "success",
    "message": "Data successfully sent to Oracle Fusion",
    "personAbsenceEntryId": 300000123456789,
    "success": "true"
  }
}
```

#### Return Path 2: Error Response - Non-20* Status, Not Gzipped (shape36)

**Return Label:** "Error Response"  
**Return Shape ID:** shape36  
**HTTP Status Code:** 400 (or original error status from Oracle Fusion)  
**Decision Conditions Leading to Return:**
- Decision shape2: meta.base.applicationstatuscode does NOT match "20*" → FALSE path
- Decision shape44: dynamicdocument.DDP_RespHeader does NOT equal "gzip" → FALSE path

**Populated Response Fields:**

| Field Name | Field Path | Source | Populated By |
|---|---|---|---|
| status | leaveResponse/Object/status | map default | Leave Error Map |
| message | leaveResponse/Object/message | process_property | process.DPP_ErrorMessage (from meta.base.applicationstatusmessage) |
| success | leaveResponse/Object/success | map default | Leave Error Map |

**Response JSON Example:**

```json
{
  "leaveResponse": {
    "status": "failure",
    "message": "Invalid absence type provided",
    "success": "false"
  }
}
```

#### Return Path 3: Error Response - Non-20* Status, Gzipped (shape48)

**Return Label:** "Error Response"  
**Return Shape ID:** shape48  
**HTTP Status Code:** 400 (or original error status from Oracle Fusion)  
**Decision Conditions Leading to Return:**
- Decision shape2: meta.base.applicationstatuscode does NOT match "20*" → FALSE path
- Decision shape44: dynamicdocument.DDP_RespHeader equals "gzip" → TRUE path

**Populated Response Fields:**

| Field Name | Field Path | Source | Populated By |
|---|---|---|---|
| status | leaveResponse/Object/status | map default | Leave Error Map |
| message | leaveResponse/Object/message | process_property | process.DPP_ErrorMessage (from decompressed response) |
| success | leaveResponse/Object/success | map default | Leave Error Map |

**Response JSON Example:**

```json
{
  "leaveResponse": {
    "status": "failure",
    "message": "Employee not found in Oracle Fusion HCM",
    "success": "false"
  }
}
```

#### Return Path 4: Error Response - Try/Catch Exception (shape43)

**Return Label:** "Error Response"  
**Return Shape ID:** shape43  
**HTTP Status Code:** 500  
**Decision Conditions Leading to Return:**
- Try/Catch shape17: Exception caught in Try block → Catch path

**Populated Response Fields:**

| Field Name | Field Path | Source | Populated By |
|---|---|---|---|
| status | leaveResponse/Object/status | map default | Leave Error Map |
| message | leaveResponse/Object/message | process_property | process.DPP_ErrorMessage (from meta.base.catcherrorsmessage) |
| success | leaveResponse/Object/success | map default | Leave Error Map |

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
| Leave Oracle Fusion Create | 200, 201, 202 | 400, 401, 403, 404, 500, 503 | Return error response with message |

**✅ Step 1e Complete: HTTP status codes and return path responses documented**

---

## 8. Data Dependency Graph (Step 4)

### Dependency Graph

**Property: dynamicdocument.URL**
- **WRITTEN BY:** shape8 (set URL)
  - Source: Defined parameter (Resource_Path)
- **READ BY:** shape33 (Leave Oracle Fusion Create operation)
- **DEPENDENCY:** shape8 MUST execute BEFORE shape33

**Property: process.DPP_Process_Name**
- **WRITTEN BY:** shape38 (Input_details)
  - Source: Execution property (Process Name)
- **READ BY:** shape11, shape23 (subprocess email body)
- **DEPENDENCY:** shape38 MUST execute BEFORE subprocess shape11/shape23

**Property: process.DPP_AtomName**
- **WRITTEN BY:** shape38 (Input_details)
  - Source: Execution property (Atom Name)
- **READ BY:** shape11, shape23 (subprocess email body)
- **DEPENDENCY:** shape38 MUST execute BEFORE subprocess shape11/shape23

**Property: process.DPP_Payload**
- **WRITTEN BY:** shape38 (Input_details)
  - Source: Current document
- **READ BY:** shape15 (subprocess - payload message for attachment)
- **DEPENDENCY:** shape38 MUST execute BEFORE subprocess shape15

**Property: process.DPP_ExecutionID**
- **WRITTEN BY:** shape38 (Input_details)
  - Source: Execution property (Execution Id)
- **READ BY:** shape11, shape23 (subprocess email body)
- **DEPENDENCY:** shape38 MUST execute BEFORE subprocess shape11/shape23

**Property: process.DPP_File_Name**
- **WRITTEN BY:** shape38 (Input_details)
  - Source: Execution property + date + static (.txt)
- **READ BY:** shape6 (subprocess - email attachment filename)
- **DEPENDENCY:** shape38 MUST execute BEFORE subprocess shape6

**Property: process.DPP_Subject**
- **WRITTEN BY:** shape38 (Input_details)
  - Source: Execution property + static text
- **READ BY:** shape6, shape20 (subprocess - email subject)
- **DEPENDENCY:** shape38 MUST execute BEFORE subprocess shape6/shape20

**Property: process.To_Email**
- **WRITTEN BY:** shape38 (Input_details)
  - Source: Defined parameter (To_Email)
- **READ BY:** shape6, shape20 (subprocess - email recipient)
- **DEPENDENCY:** shape38 MUST execute BEFORE subprocess shape6/shape20

**Property: process.DPP_HasAttachment**
- **WRITTEN BY:** shape38 (Input_details)
  - Source: Defined parameter (DPP_HasAttachment)
- **READ BY:** shape4 (subprocess - decision for attachment)
- **DEPENDENCY:** shape38 MUST execute BEFORE subprocess shape4

**Property: process.DPP_ErrorMessage**
- **WRITTEN BY:** shape19 (catch error path), shape39 (error path - non-gzip), shape46 (error path - gzip)
  - Source: Track property (meta.base.catcherrorsmessage or meta.base.applicationstatusmessage)
- **READ BY:** shape11, shape23 (subprocess email body), map_f46b845a (error map)
- **DEPENDENCY:** shape19/shape39/shape46 MUST execute BEFORE subprocess or error map

**Property: process.DPP_MailBody**
- **WRITTEN BY:** shape14, shape22 (subprocess - set mail body)
  - Source: Current document (HTML email body)
- **READ BY:** shape6, shape20 (subprocess - email body)
- **DEPENDENCY:** shape14/shape22 MUST execute BEFORE shape6/shape20

### Dependency Chains

1. **Main Process → Oracle Fusion API Call:**
   - shape38 (set properties) → shape8 (set URL) → shape49 (notify) → shape33 (API call)

2. **API Call → Success Path:**
   - shape33 (API call) → shape2 (check status 20*) → shape34 (map response) → shape35 (return success)

3. **API Call → Error Path (Non-Gzip):**
   - shape33 (API call) → shape2 (check status non-20*) → shape44 (check gzip) → shape39 (extract error) → shape40 (error map) → shape36 (return error)

4. **API Call → Error Path (Gzip):**
   - shape33 (API call) → shape2 (check status non-20*) → shape44 (check gzip) → shape45 (decompress) → shape46 (extract error) → shape47 (error map) → shape48 (return error)

5. **Try/Catch → Error Path:**
   - shape17 (try/catch) → shape20 (branch) → shape19 (extract error) → shape21 (email subprocess) → shape41 (error map) → shape43 (return error)

6. **Subprocess Email (With Attachment):**
   - shape38 (set properties) → shape4 (check attachment) → shape11 (mail body) → shape14 (set mail body) → shape15 (payload) → shape6 (set mail properties) → shape3 (send email)

7. **Subprocess Email (Without Attachment):**
   - shape38 (set properties) → shape4 (check attachment) → shape23 (mail body) → shape22 (set mail body) → shape20 (set mail properties) → shape7 (send email)

### Independent Operations

None - All operations have dependencies through process properties or control flow.

**✅ Step 4 Complete: Data dependency graph documented**

---

## 9. Control Flow Graph (Step 5)

### Control Flow Map

| From Shape | Shape Type | To Shape(s) | Identifier | Notes |
|---|---|---|---|---|
| shape1 | start | shape38 | default | Entry point (Web Service Server) |
| shape38 | documentproperties | shape17 | default | Set input details → Try/Catch |
| shape17 | catcherrors | shape29 (Try), shape20 (Catch) | default, error | Try/Catch block |
| shape29 | map | shape8 | default | Map D365 to Oracle Fusion format |
| shape8 | documentproperties | shape49 | default | Set dynamic URL |
| shape49 | notify | shape33 | default | Log request → Call Oracle Fusion API |
| shape33 | connectoraction | shape2 | default | API call → Check HTTP status |
| shape2 | decision | shape34 (True), shape44 (False) | true, false | Check HTTP status 20* |
| shape34 | map | shape35 | default | Map success response |
| shape35 | returndocuments | - | - | Return success response (terminal) |
| shape44 | decision | shape45 (True), shape39 (False) | true, false | Check if response is gzipped |
| shape45 | dataprocess | shape46 | default | Decompress gzipped response |
| shape46 | documentproperties | shape47 | default | Extract error message |
| shape47 | map | shape48 | default | Map error response |
| shape48 | returndocuments | - | - | Return error response (terminal) |
| shape39 | documentproperties | shape40 | default | Extract error message (non-gzip) |
| shape40 | map | shape36 | default | Map error response |
| shape36 | returndocuments | - | - | Return error response (terminal) |
| shape20 | branch | shape19 (Path 1), shape41 (Path 2) | 1, 2 | Branch on catch error |
| shape19 | documentproperties | shape21 | default | Extract error message |
| shape21 | processcall | - | - | Call email subprocess (terminal - subprocess handles return) |
| shape41 | map | shape43 | default | Map error response |
| shape43 | returndocuments | - | - | Return error response (terminal) |

### Subprocess Control Flow (a85945c5-3004-42b9-80b1-104f465cd1fb)

| From Shape | Shape Type | To Shape(s) | Identifier | Notes |
|---|---|---|---|---|
| shape1 | start | shape2 | default | Subprocess entry point |
| shape2 | catcherrors | shape4 (Try), shape10 (Catch) | default, error | Try/Catch block |
| shape4 | decision | shape11 (True), shape23 (False) | true, false | Check if attachment required |
| shape11 | message | shape14 | default | Create HTML email body (with attachment) |
| shape14 | documentproperties | shape15 | default | Set mail body property |
| shape15 | message | shape6 | default | Set payload as attachment |
| shape6 | documentproperties | shape3 | default | Set mail properties (from, to, subject, body, filename) |
| shape3 | connectoraction | shape5 | default | Send email with attachment |
| shape5 | stop | - | - | Stop with continue=true (success return) |
| shape23 | message | shape22 | default | Create HTML email body (no attachment) |
| shape22 | documentproperties | shape20 | default | Set mail body property |
| shape20 | documentproperties | shape7 | default | Set mail properties (from, to, subject, body) |
| shape7 | connectoraction | shape9 | default | Send email without attachment |
| shape9 | stop | - | - | Stop with continue=true (success return) |
| shape10 | exception | - | - | Throw exception (terminal) |

### Connection Summary

- **Total Shapes (Main Process):** 20 shapes
- **Total Shapes (Subprocess):** 13 shapes
- **Total Connections (Main Process):** 19 connections
- **Total Connections (Subprocess):** 12 connections
- **Shapes with Multiple Outgoing Connections:**
  - shape17 (catcherrors): 2 paths (try, catch)
  - shape2 (decision): 2 paths (true, false)
  - shape44 (decision): 2 paths (true, false)
  - shape20 (branch): 2 paths (1, 2)
  - shape4 (decision - subprocess): 2 paths (true, false)
  - shape2 (catcherrors - subprocess): 2 paths (try, catch)

**✅ Step 5 Complete: Control flow graph documented**

### Reverse Flow Mapping (Step 6)

**Convergence Points:** None identified - All paths lead to terminal shapes (return documents or stop)

**Terminal Shapes:**
- shape35 (Success Response)
- shape36 (Error Response - non-gzip)
- shape48 (Error Response - gzip)
- shape43 (Error Response - catch)
- shape5 (Subprocess - email sent with attachment)
- shape9 (Subprocess - email sent without attachment)
- shape10 (Subprocess - exception thrown)

**✅ Step 6 Complete: Reverse flow mapping documented**

---

## 10. Decision Shape Analysis (Step 7)

### Decision Data Source Analysis (MANDATORY)

**✅ Decision data sources identified: YES**  
**✅ Decision types classified: YES**  
**✅ Execution order verified: YES**  
**✅ All decision paths traced: YES**  
**✅ Decision patterns identified: YES**  
**✅ Paths traced to termination: YES**

### Decision Inventory

#### Decision 1: shape2 - HTTP Status 20 check

**Shape ID:** shape2  
**Comparison:** wildcard  
**Value 1:** meta.base.applicationstatuscode (track property)  
**Value 2:** "20*" (static)  
**Data Source:** TRACK_PROPERTY (from operation response)  
**Decision Type:** POST_OPERATION  
**Actual Execution Order:** Operation (shape33) → Response → Decision (shape2) → Route to success/error path

**TRUE Path:**
- **Destination:** shape34 (map success response)
- **Termination:** shape35 (Return Documents - Success Response)
- **Type:** Success path - map Oracle Fusion response to D365 format

**FALSE Path:**
- **Destination:** shape44 (check if response is gzipped)
- **Termination:** shape36 or shape48 (Return Documents - Error Response)
- **Type:** Error path - check compression, extract error, return error

**Pattern:** Error Check (Success vs Failure)  
**Convergence Point:** None (paths terminate separately)  
**Early Exit:** Both paths terminate (TRUE returns success, FALSE returns error)

**Business Logic:**
- If Oracle Fusion API returns HTTP 20* (200, 201, 202, etc.) → Success
- If Oracle Fusion API returns non-20* (400, 401, 404, 500, etc.) → Error

#### Decision 2: shape44 - Check Response Content Type

**Shape ID:** shape44  
**Comparison:** equals  
**Value 1:** dynamicdocument.DDP_RespHeader (document property)  
**Value 2:** "gzip" (static)  
**Data Source:** DOCUMENT_PROPERTY (from operation response header)  
**Decision Type:** POST_OPERATION  
**Actual Execution Order:** Operation (shape33) → Response Headers → Decision (shape44) → Route to decompress/extract

**TRUE Path:**
- **Destination:** shape45 (decompress gzipped response)
- **Termination:** shape48 (Return Documents - Error Response)
- **Type:** Decompress → Extract error → Map error → Return error

**FALSE Path:**
- **Destination:** shape39 (extract error message directly)
- **Termination:** shape36 (Return Documents - Error Response)
- **Type:** Extract error → Map error → Return error

**Pattern:** Conditional Logic (Optional Processing)  
**Convergence Point:** None (both paths lead to error return, but different shapes)  
**Early Exit:** Both paths terminate with error response

**Business Logic:**
- If Oracle Fusion error response is gzipped → Decompress first, then extract error
- If Oracle Fusion error response is not gzipped → Extract error directly

#### Decision 3: shape4 (Subprocess) - Attachment_Check

**Shape ID:** shape4 (in subprocess a85945c5-3004-42b9-80b1-104f465cd1fb)  
**Comparison:** equals  
**Value 1:** process.DPP_HasAttachment (process property)  
**Value 2:** "Y" (static)  
**Data Source:** PROCESS_PROPERTY (set by main process)  
**Decision Type:** PRE_FILTER  
**Actual Execution Order:** Main process sets property → Subprocess reads property → Decision routes

**TRUE Path:**
- **Destination:** shape11 (create email body with attachment)
- **Termination:** shape5 (Stop - continue=true, success return)
- **Type:** Send email with attachment

**FALSE Path:**
- **Destination:** shape23 (create email body without attachment)
- **Termination:** shape9 (Stop - continue=true, success return)
- **Type:** Send email without attachment

**Pattern:** Conditional Logic (Optional Processing)  
**Convergence Point:** None (both paths terminate with stop)  
**Early Exit:** None (both paths complete successfully)

**Business Logic:**
- If attachment required (Y) → Create email with payload attachment
- If attachment not required (N) → Create email without attachment

### Decision Patterns Summary

| Pattern Type | Decision Shapes | Description |
|---|---|---|
| Error Check (Success vs Failure) | shape2 | Check HTTP status code to route to success or error path |
| Conditional Logic (Optional Processing) | shape44, shape4 | Check condition to determine processing path (decompress, attachment) |

**✅ Step 7 Complete: Decision shape analysis documented with data source analysis**

---

## 11. Branch Shape Analysis (Step 8)

### Branch Shape Analysis

**✅ Classification completed: YES**  
**✅ Assumption check: NO (analyzed dependencies)**  
**✅ Properties extracted: YES**  
**✅ Dependency graph built: YES**  
**✅ Topological sort applied: N/A (no dependencies between paths)**

#### Branch 1: shape20 (Catch Error Branch)

**Shape ID:** shape20  
**Number of Paths:** 2  
**Location:** Catch block of main process Try/Catch

**Path Properties Analysis:**

**Path 1 (shape19):**
- **READS:** meta.base.catcherrorsmessage (track property)
- **WRITES:** process.DPP_ErrorMessage
- **Operations:** documentproperties (extract error) → processcall (email subprocess)

**Path 2 (shape41):**
- **READS:** None (uses dummy profile)
- **WRITES:** None
- **Operations:** map (error map) → returndocuments (return error)

**Dependency Graph:**
- No dependencies between Path 1 and Path 2
- Path 1 reads track property (available from catch)
- Path 2 uses dummy profile (no data dependencies)

**Classification:** PARALLEL  
**Reasoning:** No data dependencies between paths, no API calls in paths

**API Call Detection:** ❌ NO API calls in branch paths

**Path Termination:**
- Path 1: Subprocess (email notification) - subprocess returns to main process (implicit)
- Path 2: shape43 (Return Documents - Error Response)

**Convergence Points:** None (paths terminate independently)

**Execution Continues From:** None (both paths terminate)

**Business Logic:**
- Path 1: Send error notification email to support team
- Path 2: Return error response to D365
- Both paths execute in parallel (email notification + error response)

**✅ Branch Analysis Complete**

**Self-Check Results:**
- ✅ Classification completed: YES (PARALLEL)
- ✅ Assumption check: NO (analyzed dependencies AND API calls)
- ✅ Properties extracted: YES (listed above)
- ✅ Dependency graph built: YES (no dependencies found)
- ✅ Topological sort applied: N/A (parallel execution, no ordering required)

---

## 12. Execution Order (Step 9)

### Business Logic Verification (Step 0 - MUST DO FIRST)

**✅ Business logic verified FIRST: YES**  
**✅ Operation analysis complete: YES**  
**✅ Business logic execution order identified: YES**  
**✅ Data dependencies checked FIRST: YES**  
**✅ Operation response analysis used: YES** (Reference: Section 5)  
**✅ Decision analysis used: YES** (Reference: Section 10)  
**✅ Dependency graph used: YES** (Reference: Section 8)  
**✅ Branch analysis used: YES** (Reference: Section 11)  
**✅ Property dependency verification: YES** (All reads happen after writes)  
**✅ Topological sort applied: N/A** (no sequential branches)

### Business Logic Flow

#### Operation 1: Create Leave Oracle Fusion OP (8f709c2b)

**Purpose:** Web Service Server - Entry point for D365 leave requests  
**Outputs:** Input document (leave request from D365)  
**Dependent Operations:** All subsequent operations depend on this entry point  
**Business Flow:** Receives leave request from D365 → Triggers process execution

#### Operation 2: Leave Oracle Fusion Create (6e8920fd)

**Purpose:** HTTP POST to Oracle Fusion HCM to create leave absence entry  
**Outputs:**
- HTTP status code (meta.base.applicationstatuscode)
- HTTP response message (meta.base.applicationstatusmessage)
- Response body (Oracle Fusion leave response JSON)
- Content-Encoding header (dynamicdocument.DDP_RespHeader)

**Dependent Operations:**
- shape2 (decision) - checks HTTP status code
- shape34 (map) - maps success response
- shape44 (decision) - checks if response is gzipped
- shape39/shape46 (documentproperties) - extract error message

**Business Flow:**
- Operation MUST execute FIRST (produces data needed by all subsequent operations)
- Response determines success or error path
- HTTP status code determines routing (20* = success, non-20* = error)

**Business Pattern:** API Integration - Create leave absence in Oracle Fusion HCM

#### Operation 3: Email w Attachment (af07502a) / Email W/O Attachment (15a72a21)

**Purpose:** Send error notification email to support team  
**Outputs:** Email sent (no data returned to process)  
**Dependent Operations:** None (terminal operation in subprocess)  
**Business Flow:**
- Executes ONLY on error (catch path)
- Sends email with error details to support team
- Runs in parallel with error response return

**Business Pattern:** Error Notification - Alert support team of failures

### Execution Order Analysis

**Based on dependency graph (Section 8), control flow (Section 9), decision analysis (Section 10), and branch analysis (Section 11):**

#### Main Process Execution Order

1. **START** (shape1) - Web Service Server entry point
2. **shape38** (Input_details) - Set process properties (name, atom, payload, execution ID, etc.)
   - WRITES: process.DPP_Process_Name, process.DPP_AtomName, process.DPP_Payload, process.DPP_ExecutionID, process.DPP_File_Name, process.DPP_Subject, process.To_Email, process.DPP_HasAttachment
3. **shape17** (Try/Catch) - Begin error handling block

#### Try Path (Success Flow)

4. **shape29** (Map) - Transform D365 leave request to Oracle Fusion format
   - READS: Input document (D365 leave request)
   - WRITES: Transformed document (Oracle Fusion format)
5. **shape8** (set URL) - Set dynamic URL for Oracle Fusion API
   - READS: Defined parameter (Resource_Path)
   - WRITES: dynamicdocument.URL
6. **shape49** (Notify) - Log request payload
   - READS: Current document
7. **shape33** (Leave Oracle Fusion Create) - HTTP POST to Oracle Fusion HCM
   - READS: dynamicdocument.URL, transformed document
   - WRITES: Response (HTTP status, message, body, headers)
8. **shape2** (Decision) - Check HTTP status code (20*)
   - READS: meta.base.applicationstatuscode

#### Success Path (HTTP 20*)

9. **shape34** (Map) - Map Oracle Fusion response to D365 format
   - READS: Oracle Fusion response
   - WRITES: D365 success response
10. **shape35** (Return Documents) - Return success response [HTTP: 200] [SUCCESS]

#### Error Path (HTTP non-20*)

9. **shape44** (Decision) - Check if response is gzipped
   - READS: dynamicdocument.DDP_RespHeader

#### Error Path - Gzipped Response

10. **shape45** (Decompress) - Decompress gzipped response
    - READS: Gzipped response
    - WRITES: Decompressed response
11. **shape46** (Extract Error) - Extract error message
    - READS: meta.base.applicationstatusmessage
    - WRITES: process.DPP_ErrorMessage
12. **shape47** (Map) - Map error to D365 format
    - READS: process.DPP_ErrorMessage
    - WRITES: D365 error response
13. **shape48** (Return Documents) - Return error response [HTTP: 400] [ERROR]

#### Error Path - Non-Gzipped Response

10. **shape39** (Extract Error) - Extract error message
    - READS: meta.base.applicationstatusmessage
    - WRITES: process.DPP_ErrorMessage
11. **shape40** (Map) - Map error to D365 format
    - READS: process.DPP_ErrorMessage
    - WRITES: D365 error response
12. **shape36** (Return Documents) - Return error response [HTTP: 400] [ERROR]

#### Catch Path (Exception)

4. **shape20** (Branch) - Branch into parallel paths

**PARALLEL_START**

**Path 1: Email Notification**
5. **shape19** (Extract Error) - Extract catch error message
   - READS: meta.base.catcherrorsmessage
   - WRITES: process.DPP_ErrorMessage
6. **shape21** (ProcessCall) - Call email subprocess
   - READS: process.DPP_Process_Name, process.DPP_AtomName, process.DPP_Payload, process.DPP_ExecutionID, process.DPP_File_Name, process.DPP_Subject, process.To_Email, process.DPP_HasAttachment, process.DPP_ErrorMessage
   - Subprocess executes (see Subprocess Execution Order below)

**Path 2: Error Response**
5. **shape41** (Map) - Map error to D365 format
   - READS: Dummy profile (no actual data)
   - WRITES: D365 error response
6. **shape43** (Return Documents) - Return error response [HTTP: 500] [ERROR]

**PARALLEL_END**

### Subprocess Execution Order (a85945c5-3004-42b9-80b1-104f465cd1fb)

1. **START** (shape1) - Subprocess entry point
2. **shape2** (Try/Catch) - Begin error handling block

#### Try Path

3. **shape4** (Decision) - Check if attachment required (process.DPP_HasAttachment equals "Y")
   - READS: process.DPP_HasAttachment

#### With Attachment Path

4. **shape11** (Message) - Create HTML email body with error details
   - READS: process.DPP_Process_Name, process.DPP_AtomName, process.DPP_ExecutionID, process.DPP_ErrorMessage
   - WRITES: HTML email body
5. **shape14** (Set Mail Body) - Store email body in property
   - READS: Current document (HTML body)
   - WRITES: process.DPP_MailBody
6. **shape15** (Payload) - Set payload as attachment
   - READS: process.DPP_Payload
   - WRITES: Attachment document
7. **shape6** (Set Mail Properties) - Set email properties (from, to, subject, body, filename)
   - READS: Defined parameters (From_Email, Environment), process properties (To_Email, DPP_Subject, DPP_MailBody, DPP_File_Name)
   - WRITES: connector.mail.fromAddress, connector.mail.toAddress, connector.mail.subject, connector.mail.body, connector.mail.filename
8. **shape3** (Send Email) - Send email with attachment
   - READS: Mail properties, attachment document
9. **shape5** (Stop) - Stop with continue=true [SUCCESS]

#### Without Attachment Path

4. **shape23** (Message) - Create HTML email body with error details
   - READS: process.DPP_Process_Name, process.DPP_AtomName, process.DPP_ExecutionID, process.DPP_ErrorMessage
   - WRITES: HTML email body
5. **shape22** (Set Mail Body) - Store email body in property
   - READS: Current document (HTML body)
   - WRITES: process.DPP_MailBody
6. **shape20** (Set Mail Properties) - Set email properties (from, to, subject, body)
   - READS: Defined parameters (From_Email, Environment), process properties (To_Email, DPP_Subject, DPP_MailBody)
   - WRITES: connector.mail.fromAddress, connector.mail.toAddress, connector.mail.subject, connector.mail.body
7. **shape7** (Send Email) - Send email without attachment
   - READS: Mail properties
8. **shape9** (Stop) - Stop with continue=true [SUCCESS]

#### Catch Path

3. **shape10** (Exception) - Throw exception [ERROR]

### Dependency Verification

**All property reads happen after property writes:**

✅ dynamicdocument.URL: Written by shape8 → Read by shape33  
✅ process.DPP_Process_Name: Written by shape38 → Read by shape11/shape23  
✅ process.DPP_AtomName: Written by shape38 → Read by shape11/shape23  
✅ process.DPP_Payload: Written by shape38 → Read by shape15  
✅ process.DPP_ExecutionID: Written by shape38 → Read by shape11/shape23  
✅ process.DPP_File_Name: Written by shape38 → Read by shape6  
✅ process.DPP_Subject: Written by shape38 → Read by shape6/shape20  
✅ process.To_Email: Written by shape38 → Read by shape6/shape20  
✅ process.DPP_HasAttachment: Written by shape38 → Read by shape4  
✅ process.DPP_ErrorMessage: Written by shape19/shape39/shape46 → Read by shape11/shape23/map_f46b845a  
✅ process.DPP_MailBody: Written by shape14/shape22 → Read by shape6/shape20

**✅ Step 9 Complete: Execution order documented with business logic verification**

---

## 13. Sequence Diagram (Step 10)

**📋 NOTE:** This diagram shows the technical execution flow. Detailed request/response JSON examples are documented in Section 16.

**Based on dependency graph in Step 4, control flow graph in Step 5, decision analysis in Step 7, branch analysis in Step 8, and execution order in Step 9.**

```
START (Web Service Server - Entry Point)
 |
 ├─→ shape38: Set Input Details (documentproperties)
 |   └─→ WRITES: [process.DPP_Process_Name, process.DPP_AtomName, process.DPP_Payload, 
 |                 process.DPP_ExecutionID, process.DPP_File_Name, process.DPP_Subject, 
 |                 process.To_Email, process.DPP_HasAttachment]
 |
 ├─→ shape17: Try/Catch Block (catcherrors)
 |   |
 |   ├─→ TRY PATH:
 |   |   |
 |   |   ├─→ shape29: Transform D365 to Oracle Fusion Format (map)
 |   |   |   └─→ READS: [Input document (D365 leave request)]
 |   |   |   └─→ WRITES: [Transformed document (Oracle Fusion format)]
 |   |   |
 |   |   ├─→ shape8: Set Dynamic URL (documentproperties)
 |   |   |   └─→ READS: [Defined parameter: Resource_Path]
 |   |   |   └─→ WRITES: [dynamicdocument.URL]
 |   |   |
 |   |   ├─→ shape49: Log Request Payload (notify)
 |   |   |   └─→ READS: [Current document]
 |   |   |
 |   |   ├─→ shape33: Leave Oracle Fusion Create (Downstream - HTTP POST)
 |   |   |   └─→ READS: [dynamicdocument.URL, Transformed document]
 |   |   |   └─→ WRITES: [meta.base.applicationstatuscode, meta.base.applicationstatusmessage, 
 |   |   |                 Response body, dynamicdocument.DDP_RespHeader]
 |   |   |   └─→ HTTP: [Expected: 200/201/202, Error: 400/401/403/404/500/503]
 |   |   |
 |   |   ├─→ Decision (shape2): Check HTTP Status Code (meta.base.applicationstatuscode wildcard "20*")
 |   |   |   |
 |   |   |   ├─→ IF TRUE (HTTP 20*) → SUCCESS PATH:
 |   |   |   |   |
 |   |   |   |   ├─→ shape34: Map Oracle Fusion Response to D365 Format (map)
 |   |   |   |   |   └─→ READS: [Oracle Fusion response]
 |   |   |   |   |   └─→ WRITES: [D365 success response]
 |   |   |   |   |
 |   |   |   |   └─→ shape35: Return Documents [HTTP: 200] [SUCCESS]
 |   |   |   |       └─→ Response: {"status": "success", "message": "Data successfully sent to Oracle Fusion", 
 |   |   |   |                      "personAbsenceEntryId": 300000123456789, "success": "true"}
 |   |   |   |
 |   |   |   └─→ IF FALSE (HTTP non-20*) → ERROR PATH:
 |   |   |       |
 |   |   |       ├─→ Decision (shape44): Check Response Content Type (dynamicdocument.DDP_RespHeader equals "gzip")
 |   |   |           |
 |   |   |           ├─→ IF TRUE (Gzipped) → DECOMPRESS PATH:
 |   |   |           |   |
 |   |   |           |   ├─→ shape45: Decompress Gzipped Response (dataprocess)
 |   |   |           |   |   └─→ READS: [Gzipped response]
 |   |   |           |   |   └─→ WRITES: [Decompressed response]
 |   |   |           |   |
 |   |   |           |   ├─→ shape46: Extract Error Message (documentproperties)
 |   |   |           |   |   └─→ READS: [meta.base.applicationstatusmessage]
 |   |   |           |   |   └─→ WRITES: [process.DPP_ErrorMessage]
 |   |   |           |   |
 |   |   |           |   ├─→ shape47: Map Error to D365 Format (map)
 |   |   |           |   |   └─→ READS: [process.DPP_ErrorMessage]
 |   |   |           |   |   └─→ WRITES: [D365 error response]
 |   |   |           |   |
 |   |   |           |   └─→ shape48: Return Documents [HTTP: 400] [ERROR]
 |   |   |           |       └─→ Response: {"status": "failure", "message": "Error message from Oracle Fusion", 
 |   |   |           |                      "success": "false"}
 |   |   |           |
 |   |   |           └─→ IF FALSE (Not Gzipped) → DIRECT EXTRACT PATH:
 |   |   |               |
 |   |   |               ├─→ shape39: Extract Error Message (documentproperties)
 |   |   |               |   └─→ READS: [meta.base.applicationstatusmessage]
 |   |   |               |   └─→ WRITES: [process.DPP_ErrorMessage]
 |   |   |               |
 |   |   |               ├─→ shape40: Map Error to D365 Format (map)
 |   |   |               |   └─→ READS: [process.DPP_ErrorMessage]
 |   |   |               |   └─→ WRITES: [D365 error response]
 |   |   |               |
 |   |   |               └─→ shape36: Return Documents [HTTP: 400] [ERROR]
 |   |   |                   └─→ Response: {"status": "failure", "message": "Error message from Oracle Fusion", 
 |   |   |                                  "success": "false"}
 |   |
 |   └─→ CATCH PATH (Exception):
 |       |
 |       ├─→ shape20: Branch into Parallel Paths (branch)
 |           |
 |           ├─→ [PARALLEL_START]
 |           |
 |           ├─→ PATH 1: Email Notification
 |           |   |
 |           |   ├─→ shape19: Extract Catch Error Message (documentproperties)
 |           |   |   └─→ READS: [meta.base.catcherrorsmessage]
 |           |   |   └─→ WRITES: [process.DPP_ErrorMessage]
 |           |   |
 |           |   └─→ shape21: Call Email Subprocess (processcall)
 |           |       └─→ READS: [process.DPP_Process_Name, process.DPP_AtomName, process.DPP_Payload, 
 |           |                   process.DPP_ExecutionID, process.DPP_File_Name, process.DPP_Subject, 
 |           |                   process.To_Email, process.DPP_HasAttachment, process.DPP_ErrorMessage]
 |           |       |
 |           |       └─→ SUBPROCESS: Office 365 Email (a85945c5-3004-42b9-80b1-104f465cd1fb)
 |           |           |
 |           |           ├─→ START (Subprocess Entry)
 |           |           |
 |           |           ├─→ shape2: Try/Catch Block (catcherrors)
 |           |           |   |
 |           |           |   ├─→ TRY PATH:
 |           |           |   |   |
 |           |           |   |   ├─→ Decision (shape4): Check Attachment Required (process.DPP_HasAttachment equals "Y")
 |           |           |   |       |
 |           |           |   |       ├─→ IF TRUE (With Attachment):
 |           |           |   |       |   |
 |           |           |   |       |   ├─→ shape11: Create HTML Email Body (message)
 |           |           |   |       |   |   └─→ READS: [process.DPP_Process_Name, process.DPP_AtomName, 
 |           |           |   |       |   |                 process.DPP_ExecutionID, process.DPP_ErrorMessage]
 |           |           |   |       |   |
 |           |           |   |       |   ├─→ shape14: Set Mail Body Property (documentproperties)
 |           |           |   |       |   |   └─→ READS: [Current document (HTML body)]
 |           |           |   |       |   |   └─→ WRITES: [process.DPP_MailBody]
 |           |           |   |       |   |
 |           |           |   |       |   ├─→ shape15: Set Payload as Attachment (message)
 |           |           |   |       |   |   └─→ READS: [process.DPP_Payload]
 |           |           |   |       |   |
 |           |           |   |       |   ├─→ shape6: Set Mail Properties (documentproperties)
 |           |           |   |       |   |   └─→ READS: [Defined: From_Email, Environment; Process: To_Email, 
 |           |           |   |       |   |                 DPP_Subject, DPP_MailBody, DPP_File_Name]
 |           |           |   |       |   |   └─→ WRITES: [connector.mail.fromAddress, connector.mail.toAddress, 
 |           |           |   |       |   |                 connector.mail.subject, connector.mail.body, 
 |           |           |   |       |   |                 connector.mail.filename]
 |           |           |   |       |   |
 |           |           |   |       |   ├─→ shape3: Send Email with Attachment (Downstream - SMTP)
 |           |           |   |       |   |   └─→ READS: [Mail properties, Attachment document]
 |           |           |   |       |   |   └─→ HTTP: [Expected: 250, Error: 4xx/5xx]
 |           |           |   |       |   |
 |           |           |   |       |   └─→ shape5: Stop (continue=true) [SUCCESS]
 |           |           |   |       |
 |           |           |   |       └─→ IF FALSE (Without Attachment):
 |           |           |   |           |
 |           |           |   |           ├─→ shape23: Create HTML Email Body (message)
 |           |           |   |           |   └─→ READS: [process.DPP_Process_Name, process.DPP_AtomName, 
 |           |           |   |           |                 process.DPP_ExecutionID, process.DPP_ErrorMessage]
 |           |           |   |           |
 |           |           |   |           ├─→ shape22: Set Mail Body Property (documentproperties)
 |           |           |   |           |   └─→ READS: [Current document (HTML body)]
 |           |           |   |           |   └─→ WRITES: [process.DPP_MailBody]
 |           |           |   |           |
 |           |           |   |           ├─→ shape20: Set Mail Properties (documentproperties)
 |           |           |   |           |   └─→ READS: [Defined: From_Email, Environment; Process: To_Email, 
 |           |           |   |           |                 DPP_Subject, DPP_MailBody]
 |           |           |   |           |   └─→ WRITES: [connector.mail.fromAddress, connector.mail.toAddress, 
 |           |           |   |           |                 connector.mail.subject, connector.mail.body]
 |           |           |   |           |
 |           |           |   |           ├─→ shape7: Send Email without Attachment (Downstream - SMTP)
 |           |           |   |           |   └─→ READS: [Mail properties]
 |           |           |   |           |   └─→ HTTP: [Expected: 250, Error: 4xx/5xx]
 |           |           |   |           |
 |           |           |   |           └─→ shape9: Stop (continue=true) [SUCCESS]
 |           |           |   |
 |           |           |   └─→ CATCH PATH:
 |           |           |       |
 |           |           |       └─→ shape10: Throw Exception [ERROR]
 |           |           |
 |           |           └─→ END SUBPROCESS
 |           |
 |           ├─→ PATH 2: Error Response
 |           |   |
 |           |   ├─→ shape41: Map Error to D365 Format (map)
 |           |   |   └─→ READS: [Dummy profile]
 |           |   |   └─→ WRITES: [D365 error response]
 |           |   |
 |           |   └─→ shape43: Return Documents [HTTP: 500] [ERROR]
 |           |       └─→ Response: {"status": "failure", "message": "Exception message", 
 |           |                      "success": "false"}
 |           |
 |           └─→ [PARALLEL_END]
```

**Note:** Detailed request/response JSON examples for all operations and return paths are documented in Section 16 (Request/Response JSON Examples).

**✅ Step 10 Complete: Sequence diagram created with references to prior analysis steps**

---

## 14. Subprocess Analysis

### Subprocess: Office 365 Email (a85945c5-3004-42b9-80b1-104f465cd1fb)

**Subprocess ID:** a85945c5-3004-42b9-80b1-104f465cd1fb  
**Subprocess Name:** (Sub) Office 365 Email  
**Purpose:** Send error notification email to support team with optional attachment

### Internal Flow

1. **START** (shape1) - Subprocess entry point
2. **Try/Catch** (shape2) - Error handling for email operations
3. **Decision** (shape4) - Check if attachment required (process.DPP_HasAttachment equals "Y")
4. **With Attachment Path:**
   - shape11: Create HTML email body with error details
   - shape14: Set mail body property
   - shape15: Set payload as attachment
   - shape6: Set mail properties (from, to, subject, body, filename)
   - shape3: Send email with attachment (SMTP)
   - shape5: Stop (continue=true) - Success return
5. **Without Attachment Path:**
   - shape23: Create HTML email body with error details
   - shape22: Set mail body property
   - shape20: Set mail properties (from, to, subject, body)
   - shape7: Send email without attachment (SMTP)
   - shape9: Stop (continue=true) - Success return
6. **Catch Path:**
   - shape10: Throw exception - Error return

### Return Paths

| Return Label | Return Type | Condition | Main Process Mapping |
|---|---|---|---|
| (Implicit Success) | Stop (continue=true) | Email sent successfully | Continue main process execution |
| (Exception) | Exception thrown | Email send failed | Exception propagates to main process |

### Properties Written by Subprocess

| Property Name | Written By | Purpose |
|---|---|---|
| process.DPP_MailBody | shape14, shape22 | Store HTML email body |
| connector.mail.fromAddress | shape6, shape20 | Email sender address |
| connector.mail.toAddress | shape6, shape20 | Email recipient address |
| connector.mail.subject | shape6, shape20 | Email subject line |
| connector.mail.body | shape6, shape20 | Email body content |
| connector.mail.filename | shape6 | Email attachment filename (with attachment only) |

### Properties Read by Subprocess (from Main Process)

| Property Name | Read By | Purpose |
|---|---|---|
| process.DPP_HasAttachment | shape4 | Determine if attachment required |
| process.DPP_Process_Name | shape11, shape23 | Process name for email body |
| process.DPP_AtomName | shape11, shape23 | Atom name for email body |
| process.DPP_ExecutionID | shape11, shape23 | Execution ID for email body |
| process.DPP_ErrorMessage | shape11, shape23 | Error details for email body |
| process.DPP_Payload | shape15 | Payload for email attachment |
| process.DPP_File_Name | shape6 | Attachment filename |
| process.DPP_Subject | shape6, shape20 | Email subject |
| process.To_Email | shape6, shape20 | Email recipient |

### Error Paths

- **Success:** Email sent successfully → Stop (continue=true) → Main process continues
- **Exception:** Email send failed → Exception thrown → Propagates to main process catch block

**✅ Subprocess Analysis Complete**

---

## 15. System Layer Identification

### Third-Party Systems

#### System 1: Oracle Fusion HCM

**System Type:** Cloud ERP - Human Capital Management  
**Purpose:** Manage employee leave/absence data  
**Connection:** HTTP REST API  
**Base URL:** https://iaaxey-dev3.fa.ocs.oraclecloud.com:443  
**API Endpoint:** /hcmRestApi/resources/11.13.18.05/absences  
**Authentication:** Basic Authentication  
**Operations:**
- Create Leave Absence Entry (POST)

**Request Format:** JSON  
**Response Format:** JSON

**System Layer API Required:** YES  
**API Name:** OracleFusionHcmSystemApi  
**API Purpose:** Encapsulate Oracle Fusion HCM leave management operations

#### System 2: Office 365 Email (SMTP)

**System Type:** Email Service  
**Purpose:** Send error notification emails  
**Connection:** SMTP  
**Host:** smtp-mail.outlook.com  
**Port:** 587  
**Security:** TLS  
**Authentication:** SMTP AUTH  
**Operations:**
- Send Email with Attachment
- Send Email without Attachment

**System Layer API Required:** NO  
**Reason:** Email notifications are operational/monitoring concerns, not business data integration. Azure Functions can use built-in email services or SendGrid.

### System Layer APIs to Create

| System Layer API | Target System | Operations | Priority |
|---|---|---|---|
| OracleFusionHcmSystemApi | Oracle Fusion HCM | CreateLeaveAbsence | HIGH |

**✅ System Layer Identification Complete**

---

## 16. Request/Response JSON Examples

### Process Layer Entry Point

#### Request JSON Example

**Endpoint:** POST /api/HcmLeaveCreate  
**Content-Type:** application/json

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
    "personAbsenceEntryId": 300000123456789,
    "success": "true"
  }
}
```

**Error Response - Validation Error (HTTP 400):**

```json
{
  "leaveResponse": {
    "status": "failure",
    "message": "Invalid absence type provided",
    "success": "false"
  }
}
```

**Error Response - Employee Not Found (HTTP 404):**

```json
{
  "leaveResponse": {
    "status": "failure",
    "message": "Employee number 9000604 not found in Oracle Fusion HCM",
    "success": "false"
  }
}
```

**Error Response - System Error (HTTP 500):**

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

#### Operation: Leave Oracle Fusion Create (Oracle Fusion HCM)

**System Layer API:** OracleFusionHcmSystemApi.CreateLeaveAbsence

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
  "personAbsenceEntryId": 300000123456789,
  "absenceCaseId": "ABS-2024-001234",
  "absenceStatusCd": "SUBMITTED",
  "approvalStatusCd": "APPROVED",
  "personNumber": "9000604",
  "absenceType": "Sick Leave",
  "employer": "Al Ghurair Investment LLC",
  "startDate": "2024-03-24",
  "endDate": "2024-03-25",
  "startDateDuration": 1,
  "endDateDuration": 1,
  "duration": 2,
  "createdBy": "INTEGRATION.USER",
  "creationDate": "2024-03-24T10:30:00Z"
}
```

**Response JSON (Error - HTTP 400):**

```json
{
  "title": "Bad Request",
  "status": 400,
  "detail": "Invalid absence type provided. Valid values are: Sick Leave, Annual Leave, Maternity Leave, Paternity Leave",
  "o:errorCode": "VALIDATION_ERROR",
  "o:errorPath": "absenceType"
}
```

**Response JSON (Error - HTTP 404):**

```json
{
  "title": "Not Found",
  "status": 404,
  "detail": "Employee with person number 9000604 not found",
  "o:errorCode": "PERSON_NOT_FOUND",
  "o:errorPath": "personNumber"
}
```

**Response JSON (Error - HTTP 500):**

```json
{
  "title": "Internal Server Error",
  "status": 500,
  "detail": "An unexpected error occurred while processing the request",
  "o:errorCode": "INTERNAL_ERROR"
}
```

**✅ Request/Response JSON Examples Complete**

---

## 17. Function Exposure Decision Table

### Function Exposure Analysis

**Purpose:** Determine which Azure Functions should be exposed and prevent function explosion.

| Function Name | Expose? | Reasoning | Layer | HTTP Trigger? |
|---|---|---|---|---|
| HcmLeaveCreate | ✅ YES | Primary business process - Create leave absence from D365 to Oracle Fusion | Process Layer | YES (POST) |
| OracleFusionHcmCreateLeaveAbsence | ❌ NO | System Layer API - Called internally by Process Layer | System Layer | NO (Internal only) |
| SendErrorNotificationEmail | ❌ NO | Internal error handling - Not a business process | Utility/Helper | NO (Internal only) |

### Exposed Functions Summary

**Total Functions to Expose:** 1

#### Function 1: HcmLeaveCreate (Process Layer)

**HTTP Trigger:** POST /api/HcmLeaveCreate  
**Purpose:** Sync leave data from D365 to Oracle Fusion HCM  
**Input:** D365 leave request (JSON)  
**Output:** Leave creation response (JSON)  
**Authentication:** API Key / OAuth (to be determined)  
**Business Value:** Enables D365 to create leave absences in Oracle Fusion HCM

**Rationale for Exposure:**
- Primary business integration process
- Called by external system (D365)
- Represents a complete business workflow
- Requires HTTP trigger for external access

### Internal Functions (Not Exposed)

#### Function 2: OracleFusionHcmCreateLeaveAbsence (System Layer)

**Purpose:** System Layer API to encapsulate Oracle Fusion HCM leave operations  
**Called By:** HcmLeaveCreate (Process Layer)  
**Rationale for Non-Exposure:**
- System Layer API - should not be directly accessible
- Encapsulates third-party system complexity
- Called internally by Process Layer only

#### Function 3: SendErrorNotificationEmail (Utility)

**Purpose:** Send error notification emails to support team  
**Called By:** HcmLeaveCreate (on error)  
**Rationale for Non-Exposure:**
- Utility/helper function for error handling
- Not a business process
- Internal operational concern

### Function Explosion Prevention

**Guideline:** Only expose Process Layer functions that represent complete business workflows and require external access.

**Rules Applied:**
1. ✅ Expose Process Layer functions with HTTP triggers for external systems
2. ❌ Do NOT expose System Layer APIs (internal use only)
3. ❌ Do NOT expose utility/helper functions (error handling, logging, etc.)
4. ✅ Consolidate related operations into single Process Layer function when possible

**Result:** 1 exposed function (HcmLeaveCreate) instead of 3+ separate functions, preventing function explosion.

**✅ Function Exposure Decision Table Complete**

---

## Summary

### Extraction Validation Checklist

**Phase 1 Completion:**

- [x] Operations Inventory - Complete
- [x] Input Structure Analysis (Step 1a) - Complete
- [x] Response Structure Analysis (Step 1b) - Complete
- [x] Operation Response Analysis (Step 1c) - Complete
- [x] Map Analysis (Step 1d) - Complete
- [x] HTTP Status Codes and Return Paths (Step 1e) - Complete
- [x] Process Properties Analysis (Steps 2-3) - Complete
- [x] Data Dependency Graph (Step 4) - Complete
- [x] Control Flow Graph (Step 5) - Complete
- [x] Reverse Flow Mapping (Step 6) - Complete
- [x] Decision Shape Analysis (Step 7) - Complete with self-checks
- [x] Subprocess Analysis (Step 7a) - Complete
- [x] Branch Shape Analysis (Step 8) - Complete with self-checks
- [x] Execution Order (Step 9) - Complete with business logic verification
- [x] Sequence Diagram (Step 10) - Complete with references to prior steps
- [x] System Layer Identification - Complete
- [x] Request/Response JSON Examples - Complete
- [x] Function Exposure Decision Table - Complete

**Self-Check Validation:**

- [x] All Step 7 self-checks answered: YES
- [x] All Step 8 self-checks answered: YES
- [x] All Step 9 self-checks answered: YES
- [x] Sequence diagram references Steps 4, 5, 7, 8, 9: YES
- [x] All property reads happen after writes: YES
- [x] All decision paths traced to termination: YES
- [x] Business logic verified before execution order: YES

**✅ PHASE 1 EXTRACTION COMPLETE - Ready for Phase 2 (Code Generation)**

---

**Document End**
