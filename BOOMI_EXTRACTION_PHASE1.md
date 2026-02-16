# BOOMI EXTRACTION PHASE 1 - HCM Leave Create Process

**Process Name:** HCM_Leave Create  
**Process ID:** ca69f858-785f-4565-ba1f-b4edc6cca05b  
**Version:** 29  
**Description:** This Process will sync the Leave data between D365 and Oracle HCM  
**Folder Path:** Al Ghurair Investment LLC/Corporate/Fusion-HCM/Leaves

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
11. [Subprocess Analysis (Step 7a)](#11-subprocess-analysis-step-7a)
12. [Branch Shape Analysis (Step 8)](#12-branch-shape-analysis-step-8)
13. [Execution Order (Step 9)](#13-execution-order-step-9)
14. [Sequence Diagram (Step 10)](#14-sequence-diagram-step-10)
15. [Function Exposure Decision Table](#15-function-exposure-decision-table)
16. [Critical Patterns Identified](#16-critical-patterns-identified)
17. [System Layer Identification](#17-system-layer-identification)
18. [Validation Checklist](#18-validation-checklist)

---

## 1. Operations Inventory

### Main Process Operations

| Operation ID | Operation Name | Type | Sub-Type | Description |
|---|---|---|---|---|
| 8f709c2b-e63f-4d5f-9374-2932ed70415d | Create Leave Oracle Fusion OP | connector-action | wss | Entry point: Web Service Server Listen operation |
| 6e8920fd-af5a-430b-a1d9-9fde7ac29a12 | Leave Oracle Fusion Create | connector-action | http | HTTP POST operation to Oracle Fusion HCM |
| af07502a-fafd-4976-a691-45d51a33b549 | Email w Attachment | connector-action | mail | Send email with attachment |
| 15a72a21-9b57-49a1-a8ed-d70367146644 | Email W/O Attachment | connector-action | mail | Send email without attachment |

### Subprocess Operations

**Subprocess ID:** a85945c5-3004-42b9-80b1-104f465cd1fb  
**Subprocess Name:** (Sub) Office 365 Email

| Operation ID | Operation Name | Type | Sub-Type | Description |
|---|---|---|---|---|
| af07502a-fafd-4976-a691-45d51a33b549 | Email w Attachment | connector-action | mail | Send email with attachment (used in subprocess) |
| 15a72a21-9b57-49a1-a8ed-d70367146644 | Email W/O Attachment | connector-action | mail | Send email without attachment (used in subprocess) |

### Connections

| Connection ID | Type | Description |
|---|---|---|
| aa1fcb29-d146-4425-9ea6-b9698090f60e | HTTP | Oracle Fusion HCM connection |
| 00eae79b-2303-4215-8067-dcc299e42697 | Mail | Office 365 Email SMTP connection |

---

## 2. Input Structure Analysis (Step 1a)

### ✅ VALIDATION CHECKPOINT: Step 1a Complete

**🔍 SELF-CHECK ANSWERS:**
- ✅ Request profile identified from entry operation: **YES**
- ✅ Profile structure analyzed (JSON or XML): **YES** (JSON)
- ✅ Array vs single object detected: **YES** (Single Object)
- ✅ Array cardinality documented: **YES** (N/A - single object)
- ✅ ALL fields extracted (including nested): **YES**
- ✅ Field paths documented (full Boomi paths): **YES**
- ✅ Field mapping table generated (Boomi → Azure DTO): **YES**
- ✅ Document processing behavior determined: **YES**
- ✅ Input structure documented in Phase 1 document: **YES**

### Request Profile Structure

- **Profile ID:** febfa3e1-f719-4ee8-ba57-cdae34137ab3
- **Profile Name:** D365 Leave Create JSON Profile
- **Profile Type:** profile.json
- **Root Structure:** Root/Object
- **Array Detection:** ❌ NO - Single object structure
- **Input Type:** singlejson
- **Tracked Field:** employeeNumber (hr_employee_id)

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

- **Boomi Processing:** Single document processing (inputType="singlejson")
- **Azure Function Requirement:** Must accept single object (not an array)
- **Implementation Pattern:** Process single leave request per execution
- **Session Management:** One session per execution

### Field Mapping Analysis

| Boomi Field Path | Boomi Field Name | Data Type | Required | Azure DTO Property | Notes |
|---|---|---|---|---|---|
| Root/Object/employeeNumber | employeeNumber | number | Yes | EmployeeNumber | Tracked field |
| Root/Object/absenceType | absenceType | character | No | AbsenceType | Leave type (e.g., "Sick Leave") |
| Root/Object/employer | employer | character | No | Employer | Organization name |
| Root/Object/startDate | startDate | character | No | StartDate | Leave start date (ISO format) |
| Root/Object/endDate | endDate | character | No | EndDate | Leave end date (ISO format) |
| Root/Object/absenceStatusCode | absenceStatusCode | character | No | AbsenceStatusCode | Leave status (e.g., "SUBMITTED") |
| Root/Object/approvalStatusCode | approvalStatusCode | character | No | ApprovalStatusCode | Approval status (e.g., "APPROVED") |
| Root/Object/startDateDuration | startDateDuration | number | No | StartDateDuration | Duration for start date (days) |
| Root/Object/endDateDuration | endDateDuration | number | No | EndDateDuration | Duration for end date (days) |

### Total Fields Count

- **Total Input Fields:** 9 fields
- **Required Fields:** 1 (employeeNumber)
- **Optional Fields:** 8
- **Nested Structures:** None

---

## 3. Response Structure Analysis (Step 1b)

### ✅ VALIDATION CHECKPOINT: Step 1b Complete

**🔍 SELF-CHECK ANSWERS:**
- ✅ Response profile identified: **YES**
- ✅ Response structure analyzed: **YES**
- ✅ Response field mapping table generated: **YES**

### Response Profile Structure

- **Profile ID:** f4ca3a70-114a-4601-bad8-44a3eb20e2c0
- **Profile Name:** Leave D365 Response
- **Profile Type:** profile.json
- **Root Structure:** leaveResponse/Object

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
| leaveResponse/Object/personAbsenceEntryId | personAbsenceEntryId | number | PersonAbsenceEntryId | Oracle Fusion leave entry ID |
| leaveResponse/Object/success | success | character | Success | "true" or "false" |

### Total Fields Count

- **Total Response Fields:** 4 fields
- **All fields are mapped from Oracle Fusion response or static values**

---

## 4. Operation Response Analysis (Step 1c)

### ✅ VALIDATION CHECKPOINT: Step 1c Complete

**🔍 SELF-CHECK ANSWERS:**
- ✅ Operation response structures identified: **YES**
- ✅ Extracted fields documented: **YES**
- ✅ Data consumers identified: **YES**
- ✅ Business logic implications documented: **YES**

### Operation Response Inventory

#### Operation: Leave Oracle Fusion Create (6e8920fd-af5a-430b-a1d9-9fde7ac29a12)

**Response Profile:** 316175c7-0e45-4869-9ac6-5f9d69882a62 (Oracle Fusion Leave Response JSON Profile)

**Response Structure:** Complex Oracle Fusion response with 80+ fields including:
- personAbsenceEntryId (key field)
- absenceStatusCd
- approvalStatusCd
- startDate, endDate
- duration fields
- links array (nested structure)
- Many other Oracle-specific fields

**Extracted Fields:**

| Field Name | Extracted By | Written To Property | Consumer |
|---|---|---|---|
| personAbsenceEntryId | shape34 (Map) | (mapped to response) | Response mapping |
| HTTP Status Code | shape33 (HTTP operation) | meta.base.applicationstatuscode | shape2 (Decision) |
| HTTP Status Message | shape33 (HTTP operation) | meta.base.applicationstatusmessage | shape39 (Document Properties) |
| Response Header (Content-Encoding) | shape33 (HTTP operation) | dynamicdocument.DDP_RespHeader | shape44 (Decision) |

**Data Consumers:**

1. **Decision shape2 (HTTP Status 20 check):**
   - Checks: meta.base.applicationstatuscode equals "20*"
   - Data Source: RESPONSE from shape33 (Oracle Fusion HTTP call)
   - Decision Type: POST-OPERATION

2. **Decision shape44 (Check Response Content Type):**
   - Checks: dynamicdocument.DDP_RespHeader equals "gzip"
   - Data Source: RESPONSE from shape33 (Oracle Fusion HTTP call)
   - Decision Type: POST-OPERATION

**Business Logic Implications:**

**🚨 CRITICAL:** shape33 (Oracle Fusion Create) MUST execute BEFORE decisions shape2 and shape44 because they check the HTTP response status and headers from the Oracle call.

**Dependency Chain:**
```
shape33 (Oracle Fusion HTTP POST) 
  → Produces: HTTP Status Code, Status Message, Response Headers
  → shape2 (Decision: HTTP Status 20 check) reads meta.base.applicationstatuscode
  → shape44 (Decision: Check Content Type) reads dynamicdocument.DDP_RespHeader
```

**Execution Order Proof:**
- shape33 executes the HTTP POST to Oracle Fusion
- Oracle Fusion returns response with status code and headers
- shape2 checks if HTTP status is 20* (success)
- shape44 checks if response is gzip-encoded
- Based on these checks, process routes to success or error paths

---

## 5. Map Analysis (Step 1d)

### ✅ VALIDATION CHECKPOINT: Step 1d Complete

**🔍 SELF-CHECK ANSWERS:**
- ✅ ALL map files analyzed: **YES**
- ✅ SOAP request maps identified: **NO** (No SOAP operations, only HTTP/REST)
- ✅ Field mappings extracted: **YES**
- ✅ Profile vs map field name comparison: **YES**
- ✅ Scripting functions analyzed: **YES**

### Map Inventory

| Map ID | Map Name | From Profile | To Profile | Type | Usage |
|---|---|---|---|---|---|
| c426b4d6-2aff-450e-b43b-59956c4dbc96 | Leave Create Map | febfa3e1-f719-4ee8-ba57-cdae34137ab3 | a94fa205-c740-40a5-9fda-3d018611135a | Data Transformation | Maps D365 request to Oracle Fusion format |
| e4fd3f59-edb5-43a1-aeae-143b600a064e | Oracle Fusion Leave Response Map | 316175c7-0e45-4869-9ac6-5f9d69882a62 | f4ca3a70-114a-4601-bad8-44a3eb20e2c0 | Response Mapping | Maps Oracle response to D365 response |
| f46b845a-7d75-41b5-b0ad-c41a6a8e9b12 | Leave Error Map | 23d7a2e9-5cb0-4e9c-9e4b-3154834bad0d | f4ca3a70-114a-4601-bad8-44a3eb20e2c0 | Error Mapping | Maps error messages to D365 response |

### Map 1: Leave Create Map (c426b4d6-2aff-450e-b43b-59956c4dbc96)

**From Profile:** febfa3e1-f719-4ee8-ba57-cdae34137ab3 (D365 Leave Create JSON Profile)  
**To Profile:** a94fa205-c740-40a5-9fda-3d018611135a (HCM Leave Create JSON Profile)  
**Type:** REST/HTTP Request Transformation (NOT SOAP)

**Field Mappings:**

| Source Field | Source Path | Target Field | Target Path | Transformation |
|---|---|---|---|---|
| employeeNumber | Root/Object/employeeNumber | personNumber | Root/Object/personNumber | Direct mapping |
| absenceType | Root/Object/absenceType | absenceType | Root/Object/absenceType | Direct mapping |
| employer | Root/Object/employer | employer | Root/Object/employer | Direct mapping |
| startDate | Root/Object/startDate | startDate | Root/Object/startDate | Direct mapping |
| endDate | Root/Object/endDate | endDate | Root/Object/endDate | Direct mapping |
| absenceStatusCode | Root/Object/absenceStatusCode | absenceStatusCd | Root/Object/absenceStatusCd | ⚠️ Field name change |
| approvalStatusCode | Root/Object/approvalStatusCode | approvalStatusCd | Root/Object/approvalStatusCd | ⚠️ Field name change |
| startDateDuration | Root/Object/startDateDuration | startDateDuration | Root/Object/startDateDuration | Direct mapping |
| endDateDuration | Root/Object/endDateDuration | endDateDuration | Root/Object/endDateDuration | Direct mapping |

**Profile vs Map Field Name Comparison:**

| Source Profile Field | Target Profile Field | Discrepancy? | Authority | Use in HTTP Request |
|---|---|---|---|---|
| absenceStatusCode | absenceStatusCd | ✅ DIFFERENT | MAP | absenceStatusCd |
| approvalStatusCode | approvalStatusCd | ✅ DIFFERENT | MAP | approvalStatusCd |

**Scripting Functions:** None (direct field mappings only)

**HTTP Request Format:** This map prepares data for HTTP POST to Oracle Fusion HCM REST API.

### Map 2: Oracle Fusion Leave Response Map (e4fd3f59-edb5-43a1-aeae-143b600a064e)

**From Profile:** 316175c7-0e45-4869-9ac6-5f9d69882a62 (Oracle Fusion Leave Response)  
**To Profile:** f4ca3a70-114a-4601-bad8-44a3eb20e2c0 (Leave D365 Response)  
**Type:** Success Response Mapping

**Field Mappings:**

| Source Field | Source Path | Target Field | Target Path | Transformation |
|---|---|---|---|---|
| personAbsenceEntryId | Root/Object/personAbsenceEntryId | personAbsenceEntryId | leaveResponse/Object/personAbsenceEntryId | Direct mapping |

**Default Values (Static):**

| Target Field | Default Value | Purpose |
|---|---|---|
| status | "success" | Indicates successful processing |
| message | "Data successfully sent to Oracle Fusion" | Success message |
| success | "true" | Boolean success flag |

### Map 3: Leave Error Map (f46b845a-7d75-41b5-b0ad-c41a6a8e9b12)

**From Profile:** 23d7a2e9-5cb0-4e9c-9e4b-3154834bad0d (Dummy FF Profile)  
**To Profile:** f4ca3a70-114a-4601-bad8-44a3eb20e2c0 (Leave D365 Response)  
**Type:** Error Response Mapping

**Scripting Functions:**

| Function | Type | Input | Output | Logic |
|---|---|---|---|---|
| Function 1 | PropertyGet | Property Name: "DPP_ErrorMessage" | Result | Retrieves error message from process property |

**Field Mappings:**

| Source | Source Type | Target Field | Target Path | Transformation |
|---|---|---|---|---|
| Function 1 (DPP_ErrorMessage) | Process Property | message | leaveResponse/Object/message | Error message from property |

**Default Values (Static):**

| Target Field | Default Value | Purpose |
|---|---|---|
| status | "failure" | Indicates failed processing |
| success | "false" | Boolean failure flag |

**🚨 CRITICAL RULE:** This process does NOT use SOAP. It uses HTTP/REST API calls to Oracle Fusion HCM. Field name transformations are handled by standard map transformations, not SOAP envelope mappings.

---

## 6. HTTP Status Codes and Return Path Responses (Step 1e)

### ✅ VALIDATION CHECKPOINT: Step 1e Complete

**🔍 SELF-CHECK ANSWERS:**
- ✅ HTTP status codes extracted for all return paths: **YES**
- ✅ Response JSON documented for each return path: **YES**
- ✅ Populated fields documented for each return path: **YES**
- ✅ Downstream operation HTTP status codes documented: **YES**

### Return Path 1: Success Response (shape35)

**Return Label:** "Success Response"  
**Return Shape ID:** shape35  
**HTTP Status Code:** 200  
**Decision Conditions Leading to Return:**
- shape2 (Decision: HTTP Status 20 check) → TRUE path (HTTP status is 20*)

**Populated Response Fields:**

| Field Name | Field Path | Source | Populated By |
|---|---|---|---|
| status | leaveResponse/Object/status | static_value | Map: e4fd3f59 (default) |
| message | leaveResponse/Object/message | static_value | Map: e4fd3f59 (default: "Data successfully sent to Oracle Fusion") |
| personAbsenceEntryId | leaveResponse/Object/personAbsenceEntryId | operation_response | shape33 (Oracle Fusion Create) |
| success | leaveResponse/Object/success | static_value | Map: e4fd3f59 (default: "true") |

**Response JSON Example:**

```json
{
  "status": "success",
  "message": "Data successfully sent to Oracle Fusion",
  "personAbsenceEntryId": 300100582725789,
  "success": "true"
}
```

**Success Code:** N/A (implicit success)

---

### Return Path 2: Error Response - Try/Catch Error (shape43)

**Return Label:** "Error Response"  
**Return Shape ID:** shape43  
**HTTP Status Code:** 400  
**Decision Conditions Leading to Return:**
- shape17 (Catch Errors) → CATCH path (error occurred during processing)

**Populated Response Fields:**

| Field Name | Field Path | Source | Populated By |
|---|---|---|---|
| status | leaveResponse/Object/status | static_value | Map: f46b845a (default: "failure") |
| message | leaveResponse/Object/message | process_property | process.DPP_ErrorMessage (from catch error) |
| success | leaveResponse/Object/success | static_value | Map: f46b845a (default: "false") |

**Response JSON Example:**

```json
{
  "status": "failure",
  "message": "[Actual error message from exception]",
  "success": "false"
}
```

**Error Code:** Implicit (catch all errors)

---

### Return Path 3: Error Response - HTTP Non-20* Status (shape36)

**Return Label:** "Error Response"  
**Return Shape ID:** shape36  
**HTTP Status Code:** 400  
**Decision Conditions Leading to Return:**
- shape2 (Decision: HTTP Status 20 check) → FALSE path → shape44 (Check Content Type) → FALSE path

**Populated Response Fields:**

| Field Name | Field Path | Source | Populated By |
|---|---|---|---|
| status | leaveResponse/Object/status | static_value | Map: f46b845a (default: "failure") |
| message | leaveResponse/Object/message | process_property | process.DPP_ErrorMessage (from HTTP response message) |
| success | leaveResponse/Object/success | static_value | Map: f46b845a (default: "false") |

**Response JSON Example:**

```json
{
  "status": "failure",
  "message": "[HTTP error message from Oracle Fusion]",
  "success": "false"
}
```

**Error Code:** HTTP non-20* status from Oracle Fusion

---

### Return Path 4: Error Response - Gzip Content Encoding (shape48)

**Return Label:** "Error Response"  
**Return Shape ID:** shape48  
**HTTP Status Code:** 400  
**Decision Conditions Leading to Return:**
- shape2 (Decision: HTTP Status 20 check) → FALSE path → shape44 (Check Content Type) → TRUE path (gzip encoded)

**Populated Response Fields:**

| Field Name | Field Path | Source | Populated By |
|---|---|---|---|
| status | leaveResponse/Object/status | static_value | Map: f46b845a (default: "failure") |
| message | leaveResponse/Object/message | current_document | After gzip decompression (shape45) |
| success | leaveResponse/Object/success | static_value | Map: f46b845a (default: "false") |

**Response JSON Example:**

```json
{
  "status": "failure",
  "message": "[Decompressed error message from Oracle Fusion]",
  "success": "false"
}
```

**Error Code:** HTTP non-20* status with gzip encoding

---

### Downstream Operations HTTP Status Codes

#### Operation: Leave Oracle Fusion Create (shape33)

**Operation ID:** 6e8920fd-af5a-430b-a1d9-9fde7ac29a12  
**Method:** POST  
**Expected Success Codes:** 200, 201  
**Error Codes:** 400 (Bad Request), 404 (Not Found), 500 (Internal Server Error)  
**Error Handling:** returnErrors="true" - Errors are returned as documents, not thrown as exceptions

**HTTP Headers:**
- Content-Type: application/json
- Response Header Mapping: Content-Encoding → dynamicdocument.DDP_RespHeader

**Path Elements:**
- URL: Dynamic (from process property dynamicdocument.URL)

---

## 7. Process Properties Analysis (Steps 2-3)

### ✅ VALIDATION CHECKPOINT: Steps 2-3 Complete

### Properties WRITTEN (Step 2)

| Property Name | Property Type | Written By Shape | Source | Value/Description |
|---|---|---|---|---|
| process.DPP_Process_Name | Dynamic Process Property | shape38 | Execution Property | Process Name |
| process.DPP_AtomName | Dynamic Process Property | shape38 | Execution Property | Atom Name (environment) |
| process.DPP_Payload | Dynamic Process Property | shape38 | Current Document | Input payload |
| process.DPP_ExecutionID | Dynamic Process Property | shape38 | Execution Property | Execution ID |
| process.DPP_File_Name | Dynamic Process Property | shape38 | Concatenation | Process Name + Timestamp + ".txt" |
| process.DPP_Subject | Dynamic Process Property | shape38 | Concatenation | Email subject for errors |
| process.To_Email | Dynamic Process Property | shape38 | Defined Parameter | Email recipient from component e22c04db |
| process.DPP_HasAttachment | Dynamic Process Property | shape38 | Defined Parameter | "Y" or "N" from component e22c04db |
| dynamicdocument.URL | Dynamic Document Property | shape8 | Defined Parameter | Oracle Fusion API URL (Resource_Path from component e22c04db) |
| process.DPP_ErrorMessage | Dynamic Process Property | shape19 | Track Property | Error message from catch block (meta.base.catcherrorsmessage) |
| process.DPP_ErrorMessage | Dynamic Process Property | shape39 | Track Property | HTTP error message (meta.base.applicationstatusmessage) |
| process.DPP_ErrorMessage | Dynamic Process Property | shape46 | Current Document | Error message after gzip decompression |
| dynamicdocument.DDP_RespHeader | Dynamic Document Property | shape33 | HTTP Response Header | Content-Encoding header value |

### Defined Process Properties (Component e22c04db-7dfa-4b1b-9d88-77365b0fdb18)

**Component Name:** PP_HCM_LeaveCreate_Properties

| Property Key | Property Label | Type | Default Value | Description |
|---|---|---|---|---|
| c2d1a1e8-4bcb-435b-b1d2-bb10a209bcc0 | Resource_Path | string | "hcmRestApi/resources/11.13.18.05/absences" | Oracle Fusion API resource path |
| 71c90f6e-4f86-49f3-8aef-bae6668c73f9 | To_Email | string | "BoomiIntegrationTeam@al-ghurair.com" | Error notification email recipients |
| a717867b-67f4-4a72-be2d-c99c73309fdb | DPP_HasAttachment | string | "Y" | Whether error email has attachment (Y/N) |

### Properties READ (Step 3)

| Property Name | Read By Shape | Usage |
|---|---|---|
| process.DPP_Payload | shape15 (subprocess) | Message parameter for email attachment |
| process.DPP_ErrorMessage | shape11 (subprocess), shape23 (subprocess) | Message parameter for email body |
| process.DPP_Process_Name | shape11 (subprocess), shape23 (subprocess) | Message parameter for email body |
| process.DPP_AtomName | shape11 (subprocess), shape23 (subprocess) | Message parameter for email body |
| process.DPP_ExecutionID | shape11 (subprocess), shape23 (subprocess) | Message parameter for email body |
| process.DPP_File_Name | shape6 (subprocess), shape20 (subprocess) | Mail file name property |
| process.To_Email | shape6 (subprocess), shape20 (subprocess) | Mail to address property |
| process.DPP_Subject | shape6 (subprocess), shape20 (subprocess) | Mail subject property |
| process.DPP_MailBody | shape6 (subprocess), shape20 (subprocess) | Mail body property |
| process.DPP_HasAttachment | shape4 (subprocess) | Decision check for attachment |
| dynamicdocument.URL | shape33 | HTTP path element (Oracle Fusion URL) |
| dynamicdocument.DDP_RespHeader | shape44 | Decision check for gzip encoding |
| meta.base.applicationstatuscode | shape2 | Decision check for HTTP 20* status |

### Subprocess Properties (Component 0feff13f-8a2c-438a-b3c7-1909e2a7f533)

**Component Name:** PP_Office365_Email

| Property Key | Property Label | Type | Default Value | Description |
|---|---|---|---|---|
| 804b6d40-eeee-4ac0-ab4b-94e1aca9f4b7 | From_Email | string | "Boomi.Dev.failures@al-ghurair.com" | Email from address |
| 600acadb-ee02-4369-af85-ee70af380b6c | To_Email | string | "Rajesh.Muppala@al-ghurair.com;mohan.jonnalagadda@al-ghurair.com" | Default email recipients (overridden) |
| 2fa6ce9e-437a-44cc-b44f-5c7e61052f41 | HasAttachment | string | "Y" | Whether email has attachment |
| 3ca9f307-cecb-4d1e-b9ec-007839509ed7 | EmailBody | string | (empty) | Email body template |
| d8fc7496-ec2c-4308-a4ca-e9f49c0c9500 | Environment | string | "DEV Failure :" | Environment prefix for email subject |

---

## 8. Data Dependency Graph (Step 4)

### ✅ VALIDATION CHECKPOINT: Step 4 Complete

**🔍 SELF-CHECK ANSWERS:**
- ✅ Dependency graph built: **YES**
- ✅ Dependency chains documented: **YES**
- ✅ Property summary created: **YES**

### Dependency Graph

**Format:** Writer → Property → Reader(s)

#### Property: dynamicdocument.URL

**Writers:** shape8  
**Readers:** shape33  
**Dependency:** shape8 MUST execute BEFORE shape33

**Reasoning:** shape8 writes the Oracle Fusion API URL to dynamicdocument.URL, which shape33 reads to make the HTTP POST request.

---

#### Property: process.DPP_ErrorMessage

**Writers:** shape19, shape39, shape46  
**Readers:** shape11 (subprocess), shape23 (subprocess), Map f46b845a  

**Dependencies:**
- shape19 MUST execute BEFORE subprocess shape21 (error email path 1)
- shape39 MUST execute BEFORE shape40 (error email path 2)
- shape46 MUST execute BEFORE shape47 (error email path 3)

**Reasoning:** Error messages are captured from different error paths and used in error response mapping.

---

#### Property: meta.base.applicationstatuscode

**Writers:** shape33 (HTTP operation - automatic)  
**Readers:** shape2  
**Dependency:** shape33 MUST execute BEFORE shape2

**Reasoning:** shape2 decision checks HTTP status code from shape33's Oracle Fusion API call.

---

#### Property: dynamicdocument.DDP_RespHeader

**Writers:** shape33 (HTTP operation - response header mapping)  
**Readers:** shape44  
**Dependency:** shape33 MUST execute BEFORE shape44

**Reasoning:** shape44 decision checks Content-Encoding header from shape33's Oracle Fusion API response.

---

#### Property: process.DPP_Process_Name, process.DPP_AtomName, process.DPP_Payload, process.DPP_ExecutionID, process.DPP_File_Name, process.DPP_Subject, process.To_Email, process.DPP_HasAttachment

**Writers:** shape38  
**Readers:** Various shapes in subprocess (shape4, shape6, shape11, shape15, shape20, shape23)  
**Dependency:** shape38 MUST execute BEFORE any subprocess calls

**Reasoning:** shape38 sets up all required properties for error email notifications before the subprocess can use them.

---

### Dependency Chains

**Chain 1: Main Success Flow**
```
shape38 (Set properties) 
  → shape29 (Map input) 
  → shape8 (Set URL) 
  → shape33 (Oracle Fusion HTTP POST) 
  → shape2 (Check HTTP status) 
  → shape34 (Map response) 
  → shape35 (Return success)
```

**Chain 2: Try/Catch Error Flow**
```
shape38 (Set properties) 
  → shape17 (Try/Catch) 
  → CATCH path 
  → shape19 (Capture error message) 
  → shape21 (Email subprocess)
```

**Chain 3: HTTP Non-20* Error Flow (Non-Gzip)**
```
shape33 (Oracle Fusion HTTP POST) 
  → shape2 (Check HTTP status - FALSE) 
  → shape44 (Check gzip - FALSE) 
  → shape39 (Capture error message) 
  → shape40 (Map error) 
  → shape36 (Return error)
```

**Chain 4: HTTP Non-20* Error Flow (Gzip)**
```
shape33 (Oracle Fusion HTTP POST) 
  → shape2 (Check HTTP status - FALSE) 
  → shape44 (Check gzip - TRUE) 
  → shape45 (Decompress gzip) 
  → shape46 (Capture error message) 
  → shape47 (Map error) 
  → shape48 (Return error)
```

### Independent Operations

**None** - All operations have dependencies on earlier shapes (property writes or control flow).

### Property Summary

**Total Properties Creating Dependencies:** 13

**Critical Properties:**
1. **dynamicdocument.URL** - Required for Oracle Fusion API call
2. **meta.base.applicationstatuscode** - Required for success/error routing
3. **dynamicdocument.DDP_RespHeader** - Required for gzip detection
4. **process.DPP_ErrorMessage** - Required for error response mapping

---

## 9. Control Flow Graph (Step 5)

### ✅ VALIDATION CHECKPOINT: Step 5 Complete

**🔍 SELF-CHECK ANSWERS:**
- ✅ Control flow map created: **YES**
- ✅ All dragpoint connections extracted: **YES**
- ✅ Decision TRUE/FALSE paths shown: **YES**
- ✅ Branch paths shown: **YES**
- ✅ Subprocess return paths shown: **YES**

### Control Flow Map

#### Main Process Flow

| From Shape | Shape Type | To Shape | Identifier | Text/Label |
|---|---|---|---|---|
| shape1 | start | shape38 | default | Start |
| shape38 | documentproperties | shape17 | default | Input_details |
| shape17 | catcherrors | shape29 | default | Try |
| shape17 | catcherrors | shape20 | error | Catch |
| shape29 | map | shape8 | default | Leave Create Map |
| shape8 | documentproperties | shape49 | default | set URL |
| shape49 | notify | shape33 | default | Notify payload |
| shape33 | connectoraction | shape2 | default | Oracle Fusion Create |
| shape2 | decision | shape34 | true | True (HTTP 20*) |
| shape2 | decision | shape44 | false | False (Non-20*) |
| shape34 | map | shape35 | default | Success response map |
| shape35 | returndocuments | (terminal) | - | Success Response |
| shape44 | decision | shape45 | true | True (gzip) |
| shape44 | decision | shape39 | false | False (not gzip) |
| shape45 | dataprocess | shape46 | default | Decompress gzip |
| shape46 | documentproperties | shape47 | default | error msg |
| shape47 | map | shape48 | default | Error map |
| shape48 | returndocuments | (terminal) | - | Error Response |
| shape39 | documentproperties | shape40 | default | error msg |
| shape40 | map | shape36 | default | Error map |
| shape36 | returndocuments | (terminal) | - | Error Response |
| shape20 | branch | shape19 | 1 | Branch path 1 |
| shape20 | branch | shape41 | 2 | Branch path 2 |
| shape19 | documentproperties | shape21 | default | ErrorMsg |
| shape21 | processcall | (subprocess) | - | Email subprocess |
| shape41 | map | shape43 | default | Error map |
| shape43 | returndocuments | (terminal) | - | Error Response |

#### Subprocess Flow (a85945c5-3004-42b9-80b1-104f465cd1fb)

| From Shape | Shape Type | To Shape | Identifier | Text/Label |
|---|---|---|---|---|
| shape1 | start | shape2 | default | Email content |
| shape2 | catcherrors | shape4 | default | Try |
| shape2 | catcherrors | shape10 | error | Catch |
| shape4 | decision | shape11 | true | True (has attachment) |
| shape4 | decision | shape23 | false | False (no attachment) |
| shape11 | message | shape14 | default | Mail_Body |
| shape14 | documentproperties | shape15 | default | set_MailBody |
| shape15 | message | shape6 | default | payload |
| shape6 | documentproperties | shape3 | default | set_Mail_Properties |
| shape3 | connectoraction | shape5 | default | Email w Attachment |
| shape5 | stop | (terminal) | - | Stop (continue=true) |
| shape23 | message | shape22 | default | Mail_Body |
| shape22 | documentproperties | shape20 | default | set_MailBody |
| shape20 | documentproperties | shape7 | default | set_Mail_Properties |
| shape7 | connectoraction | shape9 | default | Email W/O Attachment |
| shape9 | stop | (terminal) | - | Stop (continue=true) |
| shape10 | exception | (terminal) | - | Throw exception |

### Connection Summary

**Main Process:**
- Total Shapes: 18 shapes (excluding subprocess)
- Total Connections: 24 dragpoints
- Decisions: 2 (shape2, shape44)
- Branches: 1 (shape20 - 2 paths)
- Return Documents: 4 (shape35, shape36, shape43, shape48)
- Subprocess Calls: 1 (shape21)

**Subprocess:**
- Total Shapes: 13 shapes
- Total Connections: 14 dragpoints
- Decisions: 1 (shape4)
- Stop Shapes: 2 (shape5, shape9 - both continue=true)
- Exception: 1 (shape10)

### Reverse Flow Mapping (Step 6)

**Convergence Points:** None identified (all paths lead to distinct return documents)

**Multiple Incoming Connections:**

| Target Shape | Incoming From | Context |
|---|---|---|
| (None identified) | - | All flows terminate at distinct return documents |

---

## 10. Decision Shape Analysis (Step 7)

### ✅ VALIDATION CHECKPOINT: Step 7 Complete

**🔍 SELF-CHECK MANDATORY ANSWERS:**

✅ **Decision data sources identified: YES**  
✅ **Decision types classified: YES**  
✅ **Execution order verified: YES**  
✅ **All decision paths traced: YES**  
✅ **Decision patterns identified: YES**  
✅ **Paths traced to termination: YES**

### Decision Inventory

#### Decision 1: shape2 - HTTP Status 20 check

**Shape ID:** shape2  
**User Label:** "HTTP Status 20 check"  
**Comparison Type:** wildcard  
**Value 1:** meta.base.applicationstatuscode (track property)  
**Value 2:** "20*" (static)

**🚨 CRITICAL: Data Source Analysis**

**Data Source:** TRACK_PROPERTY (meta.base.applicationstatuscode)  
**Data Source Type:** RESPONSE from shape33 (Oracle Fusion HTTP POST)  
**Decision Type:** POST-OPERATION  
**Actual Execution Order:** 
```
shape33 (Oracle Fusion HTTP POST) 
  → Oracle Fusion returns HTTP response with status code 
  → meta.base.applicationstatuscode set automatically 
  → shape2 (Decision) checks status code 
  → Route to success or error path
```

**PROOF:** meta.base.applicationstatuscode is a track property that is automatically populated by HTTP connector actions. shape2 checks this property to determine if the Oracle Fusion API call was successful (HTTP 20*) or failed (HTTP 40*, 50*).

**TRUE Path:**
- **Destination:** shape34
- **Termination:** shape35 (Return Documents - "Success Response")
- **Flow:** Map Oracle response → Return success response (HTTP 200)

**FALSE Path:**
- **Destination:** shape44
- **Termination:** Multiple (shape36, shape48 depending on gzip check)
- **Flow:** Check if error response is gzip-encoded → Map error → Return error response (HTTP 400)

**Pattern Type:** Error Check (Success vs Failure based on HTTP status)

**Convergence:** No convergence (paths terminate at different return documents)

**Early Exit:** No (both paths eventually return documents)

**Business Logic:**
- If Oracle Fusion returns HTTP 20* (success) → Map successful response → Return to caller
- If Oracle Fusion returns HTTP 40*/50* (error) → Check encoding → Map error → Return error to caller

---

#### Decision 2: shape44 - Check Response Content Type

**Shape ID:** shape44  
**User Label:** "Check Response Content Type"  
**Comparison Type:** equals  
**Value 1:** dynamicdocument.DDP_RespHeader (track property)  
**Value 2:** "gzip" (static)

**🚨 CRITICAL: Data Source Analysis**

**Data Source:** TRACK_PROPERTY (dynamicdocument.DDP_RespHeader)  
**Data Source Type:** RESPONSE from shape33 (Oracle Fusion HTTP POST - HTTP header mapping)  
**Decision Type:** POST-OPERATION  
**Actual Execution Order:** 
```
shape33 (Oracle Fusion HTTP POST) 
  → Oracle Fusion returns HTTP response with Content-Encoding header 
  → dynamicdocument.DDP_RespHeader set from response header mapping 
  → shape44 (Decision) checks if Content-Encoding equals "gzip" 
  → Route to decompress or direct error mapping
```

**PROOF:** dynamicdocument.DDP_RespHeader is mapped from the HTTP response header "Content-Encoding" in shape33 (operation 6e8920fd). shape44 checks if the error response from Oracle Fusion is gzip-compressed.

**TRUE Path:**
- **Destination:** shape45
- **Termination:** shape48 (Return Documents - "Error Response")
- **Flow:** Decompress gzip → Extract error message → Map error → Return error response (HTTP 400)

**FALSE Path:**
- **Destination:** shape39
- **Termination:** shape36 (Return Documents - "Error Response")
- **Flow:** Extract error message → Map error → Return error response (HTTP 400)

**Pattern Type:** Conditional Logic (Handle compressed vs uncompressed error responses)

**Convergence:** No convergence (paths terminate at different return documents)

**Early Exit:** No (both paths eventually return documents)

**Business Logic:**
- If error response is gzip-encoded → Decompress first → Extract error → Return to caller
- If error response is not encoded → Extract error directly → Return to caller

---

### Subprocess Decision Analysis

#### Decision 3: shape4 (subprocess) - Attachment_Check

**Shape ID:** shape4  
**User Label:** "Attachment_Check"  
**Comparison Type:** equals  
**Value 1:** process.DPP_HasAttachment (process property)  
**Value 2:** "Y" (static)

**🚨 CRITICAL: Data Source Analysis**

**Data Source:** PROCESS_PROPERTY (process.DPP_HasAttachment)  
**Data Source Type:** INPUT (set by main process shape38 from defined parameter)  
**Decision Type:** PRE-FILTER  
**Actual Execution Order:** 
```
Main process shape38 writes process.DPP_HasAttachment 
  → Subprocess shape4 reads process.DPP_HasAttachment 
  → Decision routes to attachment or non-attachment email path
```

**PROOF:** process.DPP_HasAttachment is written by shape38 in the main process before the subprocess is called. This is an input-based routing decision.

**TRUE Path:**
- **Destination:** shape11
- **Termination:** shape5 (Stop - continue=true)
- **Flow:** Build email body with attachment → Set mail properties → Send email with attachment → Return

**FALSE Path:**
- **Destination:** shape23
- **Termination:** shape9 (Stop - continue=true)
- **Flow:** Build email body → Set mail properties → Send email without attachment → Return

**Pattern Type:** Conditional Logic (Route based on attachment requirement)

**Convergence:** No convergence (both paths lead to Stop with continue=true)

**Early Exit:** No (both paths complete successfully)

**Business Logic:**
- If attachment required (Y) → Include payload as email attachment
- If no attachment (N) → Send email with body only

---

### Decision Patterns Summary

**Pattern 1: Error Check (shape2)**
- Type: POST-OPERATION
- Purpose: Route based on HTTP status code from Oracle Fusion
- Paths: Success (HTTP 20*) vs Error (HTTP 40*/50*)

**Pattern 2: Conditional Error Handling (shape44)**
- Type: POST-OPERATION
- Purpose: Handle gzip-encoded error responses
- Paths: Decompress gzip vs Direct error mapping

**Pattern 3: Conditional Routing (shape4 - subprocess)**
- Type: PRE-FILTER
- Purpose: Route email notification based on attachment requirement
- Paths: Email with attachment vs Email without attachment

---

## 11. Subprocess Analysis (Step 7a)

### ✅ VALIDATION CHECKPOINT: Step 7a Complete

### Subprocess: (Sub) Office 365 Email

**Subprocess ID:** a85945c5-3004-42b9-80b1-104f465cd1fb  
**Subprocess Name:** (Sub) Office 365 Email  
**Called By:** shape21 (processcall) in main process

### Internal Flow Analysis

**Purpose:** Send error notification emails via Office 365 SMTP

**Entry Point:** shape1 (start - passthrough)

**Flow Paths:**

**Path 1: Email with Attachment (HasAttachment = Y)**
```
shape1 (Start) 
  → shape2 (Try/Catch) 
  → shape4 (Decision: DPP_HasAttachment equals "Y") 
  → TRUE path 
  → shape11 (Message: Build HTML email body) 
  → shape14 (Document Properties: Set DPP_MailBody from current document) 
  → shape15 (Message: Attach payload) 
  → shape6 (Document Properties: Set mail properties - from, to, subject, body, filename) 
  → shape3 (Connector Action: Send email with attachment - operation af07502a) 
  → shape5 (Stop: continue=true) [SUCCESS RETURN]
```

**Path 2: Email without Attachment (HasAttachment = N)**
```
shape1 (Start) 
  → shape2 (Try/Catch) 
  → shape4 (Decision: DPP_HasAttachment equals "Y") 
  → FALSE path 
  → shape23 (Message: Build HTML email body) 
  → shape22 (Document Properties: Set DPP_MailBody from current document) 
  → shape20 (Document Properties: Set mail properties - from, to, subject, body) 
  → shape7 (Connector Action: Send email without attachment - operation 15a72a21) 
  → shape9 (Stop: continue=true) [SUCCESS RETURN]
```

**Path 3: Error Path (Any error during email sending)**
```
shape1 (Start) 
  → shape2 (Try/Catch) 
  → ERROR caught 
  → shape10 (Exception: Throw exception with catch error message) [ERROR RETURN]
```

### Return Paths

| Return Label | Return Type | Termination Shape | Main Process Mapping | Description |
|---|---|---|---|---|
| (Implicit Success) | Stop (continue=true) | shape5, shape9 | No explicit return path | Email sent successfully |
| (Exception) | Exception thrown | shape10 | No explicit return path | Email sending failed |

**Note:** The processcall in shape21 has no explicit returnpaths configuration, meaning it uses implicit return behavior (success = continue, exception = abort).

### Properties Written by Subprocess

| Property Name | Written By | Source | Description |
|---|---|---|---|
| process.DPP_MailBody | shape14, shape22 | Current document | HTML email body content |
| connector.mail.fromAddress | shape6, shape20 | Defined parameter | Email from address |
| connector.mail.toAddress | shape6, shape20 | Process property | Email to address |
| connector.mail.subject | shape6, shape20 | Process property + Defined parameter | Email subject with environment prefix |
| connector.mail.body | shape6, shape20 | Process property | Email body content |
| connector.mail.filename | shape6 | Process property | Attachment filename |

### Properties Read by Subprocess (from Main Process)

| Property Name | Read By | Usage |
|---|---|---|
| process.DPP_HasAttachment | shape4 | Decision: Check if attachment required |
| process.DPP_Process_Name | shape11, shape23 | Message parameter: Process name in email |
| process.DPP_AtomName | shape11, shape23 | Message parameter: Environment name in email |
| process.DPP_ExecutionID | shape11, shape23 | Message parameter: Execution ID in email |
| process.DPP_ErrorMessage | shape11, shape23 | Message parameter: Error details in email |
| process.DPP_Payload | shape15 | Message parameter: Original payload as attachment |
| process.To_Email | shape6, shape20 | Mail property: Email recipients |
| process.DPP_Subject | shape6, shape20 | Mail property: Email subject |
| process.DPP_File_Name | shape6 | Mail property: Attachment filename |

### Email Template (HTML)

The subprocess builds an HTML email with the following structure:

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

### Business Logic

**Purpose:** Notify operations team when leave creation fails

**Conditions for Subprocess Invocation:**
1. Try/Catch error occurs (shape17 → CATCH → shape21)
2. Main process captures error details in process.DPP_ErrorMessage
3. Subprocess sends formatted HTML email with error details

**Success Criteria:** Email sent successfully (Stop with continue=true)

**Error Handling:** If email sending fails, exception is thrown back to main process (would abort execution)

---

## 12. Branch Shape Analysis (Step 8)

### ✅ VALIDATION CHECKPOINT: Step 8 Complete

**🔍 SELF-CHECK MANDATORY ANSWERS:**

✅ **Classification completed: YES**  
✅ **Assumption check: NO (analyzed dependencies)**  
✅ **Properties extracted: YES**  
✅ **Dependency graph built: YES**  
✅ **Topological sort applied: YES (for sequential branch)**

### Branch 1: shape20 - Error Handler Branch

**Shape ID:** shape20  
**User Label:** (no label)  
**Location:** Catch error block (after shape17 CATCH path)  
**Number of Paths:** 2

#### Step 1: Properties Analysis

**Path 1 (Branch identifier "1"):**
- **Shapes:** shape19 → shape21
- **Properties READ:**
  - None (shape19 reads track property meta.base.catcherrorsmessage)
- **Properties WRITTEN:**
  - process.DPP_ErrorMessage (written by shape19)

**Path 2 (Branch identifier "2"):**
- **Shapes:** shape41 → shape43
- **Properties READ:**
  - process.DPP_ErrorMessage (read by map f46b845a in shape41)
- **Properties WRITTEN:**
  - None

#### Step 2: Dependency Graph

```
Path 1 writes: process.DPP_ErrorMessage
Path 2 reads: process.DPP_ErrorMessage

Therefore: Path 1 DEPENDS ON → Path 2 reads what Path 1 writes
```

**Dependency:** Path 2 depends on Path 1

#### Step 3: Classification

**🚨 CRITICAL: API Call Detection**

**Path 1 Contains API Calls?**
- shape21 is a processcall (subprocess invocation)
- Subprocess contains email operations (SMTP connector - shape3, shape7)
- **YES - Contains API calls (Email SMTP operations)**

**Path 2 Contains API Calls?**
- shape41 is a map operation (no API call)
- shape43 is a returndocuments (no API call)
- **NO - No API calls**

**🚨 CRITICAL RULE: ALL API CALLS ARE SEQUENTIAL**

**Classification: SEQUENTIAL**

**Reasoning:**
1. Path 1 contains API calls (email operations via subprocess)
2. Even though only Path 1 has API calls, the paths are sequential because Path 2 depends on Path 1's output
3. Path 2 reads process.DPP_ErrorMessage which is written by Path 1
4. **PROOF:** Path 1 must execute FIRST to set process.DPP_ErrorMessage, then Path 2 can use it for error mapping

**Wait, this doesn't make logical sense!** Let me re-analyze...

**🔍 CORRECTING ANALYSIS:**

Looking at the actual flow:
- Path 1: shape19 → shape21 (Email subprocess - notify about error, then subprocess terminates)
- Path 2: shape41 → shape43 (Map error → Return error response)

These are **PARALLEL** paths because:
- Path 1: Sends error notification email (side effect, subprocess handles it)
- Path 2: Returns error response to caller
- Both paths execute independently from the branch
- Path 2 does NOT depend on Path 1 completing (both use process.DPP_ErrorMessage which is already set before the branch)

**RE-CLASSIFICATION: PARALLEL**

**Reasoning:**
1. process.DPP_ErrorMessage is written by shape19 (in Path 1)
2. BUT shape19 is IN Path 1, and Path 2 needs this property
3. Actually, let me trace this more carefully...

**🔍 CAREFUL TRACE:**

Branch shape20 has two paths:
- dragpoint 1: toShape="shape19"
- dragpoint 2: toShape="shape41"

So:
- **Path 1:** shape19 (set DPP_ErrorMessage) → shape21 (subprocess)
- **Path 2:** shape41 (map error using DPP_ErrorMessage) → shape43 (return error)

Path 2 reads DPP_ErrorMessage, but where is it written before the branch?

Looking at the catch path: shape17 (CATCH) → shape20 (branch)

So DPP_ErrorMessage is NOT set before the branch. It's set IN path 1 (shape19).

**Therefore:**
- Path 1 writes process.DPP_ErrorMessage (shape19)
- Path 2 reads process.DPP_ErrorMessage (shape41/map)
- **Path 2 depends on Path 1**

**FINAL CLASSIFICATION: SEQUENTIAL**

#### Step 4: Topological Sort Order

**Dependencies:**
- Path 1 → Path 2 (Path 2 depends on Path 1)

**Topological Sort Order:**
1. Path 1 (shape19 → shape21)
2. Path 2 (shape41 → shape43)

**Execution Order:**
```
Path 1: shape19 (Write DPP_ErrorMessage) → shape21 (Email subprocess)
  ↓
Path 2: shape41 (Map error using DPP_ErrorMessage) → shape43 (Return error)
```

#### Step 5: Path Termination

**Path 1 Termination:**
- Terminal Shape: shape21 (processcall - subprocess)
- Subprocess terminates with Stop (continue=true) or Exception
- Main process continues after subprocess returns

**Path 2 Termination:**
- Terminal Shape: shape43 (returndocuments)
- Type: Return Documents - "Error Response"

#### Step 6: Convergence Points

**Convergence:** None (Path 2 terminates with return documents)

#### Step 7: Execution Continuation

**Execution Continues From:** None (Path 2 returns documents, ending execution)

**🚨 WAIT - LOGICAL ERROR!**

If Path 2 returns documents (shape43), then the process ends there. Path 1 cannot execute after Path 2.

Let me re-examine the branch logic...

**🔍 RE-EXAMINING BRANCH SHAPE20:**

Looking at dragpoints:
- Identifier "1" → toShape="shape19"
- Identifier "2" → toShape="shape41"

In Boomi, a branch shape executes ALL paths in parallel (or in sequence if dependencies exist).

But this creates a logical issue:
- Path 1: shape19 → shape21 (subprocess that sends email)
- Path 2: shape41 → shape43 (return error response)

If both execute:
- Path 1 sends email notification
- Path 2 returns error response

These should execute IN PARALLEL because:
- They serve different purposes (notification vs response)
- Both use the same error message

**BUT** shape19 writes DPP_ErrorMessage in Path 1, and Path 2 needs it.

**🚨 CRITICAL INSIGHT:**

Actually, looking more carefully at shape19:
- shape19 reads meta.base.catcherrorsmessage (track property from catch block)
- shape19 writes to process.DPP_ErrorMessage

And the branch comes AFTER the catch error shape17.

So when the catch block is triggered, meta.base.catcherrorsmessage is automatically set.

**Let me check shape41 (Path 2):**

shape41 is a map (f46b845a) that uses:
- Function 1: PropertyGet - reads "DPP_ErrorMessage"
- Maps it to leaveResponse/Object/message

So Path 2 reads process.DPP_ErrorMessage.

**CONCLUSION:**
- Path 1 writes process.DPP_ErrorMessage (shape19)
- Path 2 reads process.DPP_ErrorMessage (shape41 via map function)
- **Path 2 DEPENDS ON Path 1**
- **Classification: SEQUENTIAL**
- **Order: Path 1 BEFORE Path 2**

**But this still seems wrong logically!** Why would we send an email BEFORE returning the error response?

Let me check if there's a convergence or if these are truly separate terminating paths...

**🔍 FINAL ANALYSIS:**

Looking at the structure:
- Path 1 terminates at shape21 (subprocess call, but main process continues? No explicit return path)
- Path 2 terminates at shape43 (return documents - TERMINAL)

**Ah! I see the issue:**

The subprocess (shape21) has no explicit return paths defined:
```json
"returnpaths": ""
```

This means the subprocess executes, and when it completes (Stop with continue=true), the main process continues.

But there's no dragpoint FROM shape21 in the main process. So after the subprocess completes, what happens?

**Answer:** The subprocess completes, but since shape21 has no dragpoints in the main process flow, execution stops after the subprocess.

**So the actual behavior is:**

**Branch shape20 executes:**
- **Path 1:** shape19 (set error message) → shape21 (send email) → (subprocess completes) → (no continuation)
- **Path 2:** shape41 (map error) → shape43 (return error response) → (terminal)

**Given that both paths execute in parallel and there's no continuation from Path 1, the correct classification is:**

**CLASSIFICATION: PARALLEL** (with dependency order)

**But dependencies exist, so we need topological sort:**

**Topological Sort Order:**
1. Path 1 (sets DPP_ErrorMessage needed by Path 2)
2. Path 2 (uses DPP_ErrorMessage)

**ACTUALLY - this is SEQUENTIAL due to data dependency!**

**FINAL FINAL CLASSIFICATION: SEQUENTIAL**

**Order: Path 1 → Path 2**

**Execution:**
1. shape19 writes process.DPP_ErrorMessage
2. shape21 sends email notification (subprocess)
3. shape41 maps error using process.DPP_ErrorMessage
4. shape43 returns error response

#### Step 8: Documentation

**Branch Analysis: shape20**

**Classification:** SEQUENTIAL  
**Number of Paths:** 2  
**Dependency Order:** Path 1 → Path 2

**Properties Analysis:**
- Path 1 writes: process.DPP_ErrorMessage
- Path 2 reads: process.DPP_ErrorMessage

**Dependency Graph:**
```
Path 1 (shape19) writes process.DPP_ErrorMessage
  ↓
Path 2 (shape41) reads process.DPP_ErrorMessage via map function
```

**PROOF:** Map f46b845a (used in shape41) contains Function 1 (PropertyGet) that reads "DPP_ErrorMessage". This property is written by shape19 in Path 1. Therefore, Path 1 must execute BEFORE Path 2.

**Topological Sort:**
1. Path 1: shape19 → shape21
2. Path 2: shape41 → shape43

**Path Terminations:**
- Path 1: subprocess call (continues implicitly)
- Path 2: returndocuments (terminal)

**Convergence:** None

**Execution Continues From:** None (Path 2 terminates)

---

## 13. Execution Order (Step 9)

### ✅ VALIDATION CHECKPOINT: Step 9 Complete

**🔍 SELF-CHECK MANDATORY ANSWERS:**

✅ **Business logic verified FIRST: YES**  
✅ **Operation analysis complete: YES** (See Step 1c)  
✅ **Business logic execution order identified: YES**  
✅ **Data dependencies checked FIRST: YES** (See Step 4)  
✅ **Operation response analysis used: YES** (Referenced Step 1c)  
✅ **Decision analysis used: YES** (Referenced Step 7)  
✅ **Dependency graph used: YES** (Referenced Step 4)  
✅ **Branch analysis used: YES** (Referenced Step 8)  
✅ **Property dependency verification: YES**  
✅ **Topological sort applied: YES** (Branch shape20)

### Business Logic Flow (Step 0 - MUST BE FIRST)

#### Operation Analysis

**Operation 1: Create Leave Oracle Fusion OP (8f709c2b-e63f-4d5f-9374-2932ed70415d)**
- **Purpose:** Authentication/Entry Point - Web Service Server Listen operation
- **Outputs:** Receives JSON request from D365, makes it available to process
- **Dependent Operations:** All subsequent operations (provides input data)
- **Business Flow:** Entry point that receives leave creation requests from D365

**Operation 2: Leave Oracle Fusion Create (6e8920fd-af5a-430b-a1d9-9fde7ac29a12)**
- **Purpose:** Create Leave Entry - HTTP POST to Oracle Fusion HCM REST API
- **Outputs:** 
  - HTTP Status Code (meta.base.applicationstatuscode)
  - HTTP Status Message (meta.base.applicationstatusmessage)
  - Response Headers (dynamicdocument.DDP_RespHeader)
  - Oracle Fusion response (316175c7 profile) with personAbsenceEntryId
- **Dependent Operations:** Decisions shape2, shape44 depend on this operation's response
- **Business Flow:** shape33 MUST execute FIRST (creates leave in Oracle), then decisions check response

**Operation 3: Email w Attachment (af07502a) / Email W/O Attachment (15a72a21)**
- **Purpose:** Error Notification - Send email alerts when leave creation fails
- **Outputs:** Email sent (side effect, no data returned to main process)
- **Dependent Operations:** None (notification only)
- **Business Flow:** Sends email with error details when process fails

#### Actual Business Flow

**Main Success Path:**
1. Receive leave request from D365 (shape1 - Web Service Listen)
2. Set process properties for tracking and error handling (shape38)
3. Transform D365 format to Oracle Fusion format (shape29 - Map)
4. Set Oracle Fusion API URL (shape8)
5. **POST leave data to Oracle Fusion** (shape33) ← **CRITICAL OPERATION**
6. Check if Oracle returned HTTP 20* status (shape2 decision)
7. If success: Map Oracle response to D365 response (shape34)
8. Return success response to D365 (shape35)

**Error Path 1: Try/Catch Error (before Oracle call)**
1. Error occurs during mapping or setup (shape17 CATCH)
2. Branch to parallel error handling (shape20)
3. **Path 1:** Capture error message (shape19) → Send email notification (shape21)
4. **Path 2:** Map error message (shape41) → Return error response to D365 (shape43)

**Error Path 2: HTTP Non-20* Status (Oracle call failed)**
1. Oracle Fusion returns HTTP 40* or 50* error (shape33)
2. Decision detects non-20* status (shape2 FALSE)
3. Check if error response is gzip-encoded (shape44)
4. **If NOT gzip:** Capture error message (shape39) → Map error (shape40) → Return error to D365 (shape36)
5. **If gzip:** Decompress (shape45) → Capture error (shape46) → Map error (shape47) → Return error to D365 (shape48)

**Operations that MUST Execute First:**
- shape38 (Set properties) MUST execute BEFORE shape29 (map needs input)
- shape8 (Set URL) MUST execute BEFORE shape33 (HTTP POST needs URL)
- shape33 (Oracle POST) MUST execute BEFORE shape2 (decision needs HTTP status)
- shape33 (Oracle POST) MUST execute BEFORE shape44 (decision needs response headers)
- shape19 (Set error message) MUST execute BEFORE shape21 (email needs error message)
- shape19 (Set error message) MUST execute BEFORE shape41 (map needs error message)

### Execution Order (Following Business Logic and Data Dependencies)

**References:**
- **Step 4 (Data Dependency Graph):** Dependencies verified
- **Step 7 (Decision Analysis):** Decision execution order verified  
- **Step 8 (Branch Analysis):** Branch topological sort verified

#### Main Success Flow

```
1. shape1 (START - Web Service Listen)
   ↓
2. shape38 (Document Properties: Set all process properties)
   WRITES: process.DPP_Process_Name, process.DPP_AtomName, process.DPP_Payload, 
           process.DPP_ExecutionID, process.DPP_File_Name, process.DPP_Subject, 
           process.To_Email, process.DPP_HasAttachment
   ↓
3. shape17 (Catch Errors - TRY PATH)
   ↓
4. shape29 (Map: Leave Create Map)
   Transform: D365 format → Oracle Fusion format
   ↓
5. shape8 (Document Properties: Set URL)
   WRITES: dynamicdocument.URL
   ↓
6. shape49 (Notify: Log payload)
   ↓
7. shape33 (Connector Action: Oracle Fusion Create)
   READS: dynamicdocument.URL
   WRITES: meta.base.applicationstatuscode, dynamicdocument.DDP_RespHeader
   HTTP POST to Oracle Fusion HCM API
   ↓
8. shape2 (Decision: HTTP Status 20 check)
   READS: meta.base.applicationstatuscode
   CHECKS: Does status match "20*"?
   ↓
   IF TRUE (HTTP 20* - Success):
   ↓
9. shape34 (Map: Oracle Fusion Leave Response Map)
   Transform: Oracle response → D365 response format
   ↓
10. shape35 (Return Documents: Success Response)
    HTTP 200
    TERMINAL
```

#### Error Flow 1: Try/Catch Error (shape17 CATCH path)

```
shape17 (Catch Errors - CATCH PATH)
   ↓
shape20 (Branch: 2-path branch - SEQUENTIAL)
   ↓
   SEQUENTIAL ORDER (Path 1 → Path 2):
   ↓
   PATH 1:
   ↓
   shape19 (Document Properties: ErrorMsg)
   READS: meta.base.catcherrorsmessage
   WRITES: process.DPP_ErrorMessage
   ↓
   shape21 (Process Call: Email subprocess)
   SUBPROCESS: (Sub) Office 365 Email
   [Subprocess sends error notification email]
   ↓
   PATH 2:
   ↓
   shape41 (Map: Leave Error Map)
   READS: process.DPP_ErrorMessage (via function)
   Transform: Error → D365 response format
   ↓
   shape43 (Return Documents: Error Response)
   HTTP 400
   TERMINAL
```

#### Error Flow 2: HTTP Non-20* Status, Non-Gzip (shape2 FALSE → shape44 FALSE)

```
shape2 (Decision: HTTP Status 20 check)
   ↓
   IF FALSE (HTTP 40*/50* - Error):
   ↓
shape44 (Decision: Check Response Content Type)
   READS: dynamicdocument.DDP_RespHeader
   CHECKS: Does header equal "gzip"?
   ↓
   IF FALSE (Not gzip):
   ↓
shape39 (Document Properties: error msg)
   READS: meta.base.applicationstatusmessage
   WRITES: process.DPP_ErrorMessage
   ↓
shape40 (Map: Leave Error Map)
   READS: process.DPP_ErrorMessage (via function)
   Transform: Error → D365 response format
   ↓
shape36 (Return Documents: Error Response)
   HTTP 400
   TERMINAL
```

#### Error Flow 3: HTTP Non-20* Status, Gzip (shape2 FALSE → shape44 TRUE)

```
shape2 (Decision: HTTP Status 20 check)
   ↓
   IF FALSE (HTTP 40*/50* - Error):
   ↓
shape44 (Decision: Check Response Content Type)
   READS: dynamicdocument.DDP_RespHeader
   CHECKS: Does header equal "gzip"?
   ↓
   IF TRUE (Gzip encoded):
   ↓
shape45 (Data Process: Custom Scripting - Decompress gzip)
   Groovy script: Decompress GZIPInputStream
   ↓
shape46 (Document Properties: error msg)
   READS: Current document (decompressed error)
   WRITES: process.DPP_ErrorMessage
   ↓
shape47 (Map: Leave Error Map)
   READS: process.DPP_ErrorMessage (via function)
   Transform: Error → D365 response format
   ↓
shape48 (Return Documents: Error Response)
   HTTP 400
   TERMINAL
```

### Dependency Verification

**From Step 4 (Data Dependency Graph):**

✅ **Verified:** shape8 writes dynamicdocument.URL → shape33 reads it  
✅ **Verified:** shape33 writes meta.base.applicationstatuscode → shape2 reads it  
✅ **Verified:** shape33 writes dynamicdocument.DDP_RespHeader → shape44 reads it  
✅ **Verified:** shape19 writes process.DPP_ErrorMessage → shape41 reads it (via map function)  
✅ **Verified:** shape39 writes process.DPP_ErrorMessage → shape40 reads it (via map function)  
✅ **Verified:** shape46 writes process.DPP_ErrorMessage → shape47 reads it (via map function)  
✅ **Verified:** shape38 writes all subprocess properties → subprocess reads them

**All property dependencies are satisfied in the execution order above.**

---

## 14. Sequence Diagram (Step 10)

### ✅ VALIDATION CHECKPOINT: Step 10 Prerequisites

**PRE-CREATION VALIDATION:**

✅ **Step 4 (Data Dependency Graph):** COMPLETE and DOCUMENTED (Section 8)  
✅ **Step 5 (Control Flow Graph):** COMPLETE and DOCUMENTED (Section 9)  
✅ **Step 7 (Decision Analysis):** COMPLETE and DOCUMENTED (Section 10)  
✅ **Step 8 (Branch Analysis):** COMPLETE and DOCUMENTED (Section 12)  
✅ **Step 9 (Execution Order):** COMPLETE and DOCUMENTED (Section 13)  
✅ **All self-check answers from Steps 7-9:** Shown with YES answers

### Sequence Diagram

**📋 NOTE:** Detailed request/response JSON examples are documented in:
- **Section 6: HTTP Status Codes and Return Path Responses** - For response JSON with populated fields for return paths
- **Section 2: Input Structure Analysis** - For detailed request JSON structure
- **Section 3: Response Structure Analysis** - For detailed response JSON structure

**References:**
- Based on dependency graph in Step 4 (Section 8)
- Based on decision analysis in Step 7 (Section 10)
- Based on control flow graph in Step 5 (Section 9)
- Based on branch analysis in Step 8 (Section 12)
- Based on execution order in Step 9 (Section 13)

```
START (shape1: Web Service Listen - Entry Point)
 |
 ├─→ shape38: Set Process Properties (Document Properties)
 |   └─→ WRITES: [process.DPP_Process_Name, process.DPP_AtomName, process.DPP_Payload,
 |                 process.DPP_ExecutionID, process.DPP_File_Name, process.DPP_Subject,
 |                 process.To_Email, process.DPP_HasAttachment]
 |
 ├─→ TRY/CATCH Block (shape17)
 |   |
 |   ├─→ TRY PATH:
 |   |   |
 |   |   ├─→ shape29: Leave Create Map
 |   |   |   └─→ Transform: D365 format → Oracle Fusion format
 |   |   |
 |   |   ├─→ shape8: Set URL (Document Properties)
 |   |   |   └─→ WRITES: [dynamicdocument.URL]
 |   |   |   └─→ READS: [Defined Parameter: Resource_Path]
 |   |   |
 |   |   ├─→ shape49: Notify Payload (Log)
 |   |   |
 |   |   ├─→ shape33: Leave Oracle Fusion Create (Downstream - HTTP POST)
 |   |   |   └─→ READS: [dynamicdocument.URL]
 |   |   |   └─→ WRITES: [meta.base.applicationstatuscode, meta.base.applicationstatusmessage,
 |   |   |                 dynamicdocument.DDP_RespHeader]
 |   |   |   └─→ HTTP: [Expected: 200/201, Error: 400/404/500]
 |   |   |   └─→ Method: POST
 |   |   |   └─→ Endpoint: Oracle Fusion HCM /absences API
 |   |   |
 |   |   ├─→ shape2: Decision - HTTP Status 20 check
 |   |   |   └─→ READS: [meta.base.applicationstatuscode]
 |   |   |   └─→ CONDITION: Check if HTTP status matches "20*"
 |   |   |   └─→ DATA SOURCE: RESPONSE from shape33 (POST-OPERATION)
 |   |   |   |
 |   |   |   ├─→ IF TRUE (HTTP 20* - Success):
 |   |   |   |   |
 |   |   |   |   ├─→ shape34: Oracle Fusion Leave Response Map
 |   |   |   |   |   └─→ Transform: Oracle response → D365 response
 |   |   |   |   |   └─→ Map personAbsenceEntryId + set defaults (status, message, success)
 |   |   |   |   |
 |   |   |   |   └─→ shape35: Return Documents [HTTP: 200] [SUCCESS]
 |   |   |   |       └─→ Response: {"status":"success", "message":"Data successfully sent to Oracle Fusion",
 |   |   |   |                     "personAbsenceEntryId":300100582725789, "success":"true"}
 |   |   |   |
 |   |   |   └─→ IF FALSE (HTTP 40*/50* - Error):
 |   |   |       |
 |   |   |       ├─→ shape44: Decision - Check Response Content Type
 |   |   |       |   └─→ READS: [dynamicdocument.DDP_RespHeader]
 |   |   |       |   └─→ CONDITION: Check if Content-Encoding equals "gzip"
 |   |   |       |   └─→ DATA SOURCE: RESPONSE from shape33 (POST-OPERATION)
 |   |   |       |   |
 |   |   |       |   ├─→ IF TRUE (Gzip encoded):
 |   |   |       |   |   |
 |   |   |       |   |   ├─→ shape45: Decompress Gzip (Data Process - Groovy)
 |   |   |       |   |   |   └─→ Script: GZIPInputStream decompression
 |   |   |       |   |   |
 |   |   |       |   |   ├─→ shape46: Capture Error Message (Document Properties)
 |   |   |       |   |   |   └─→ READS: [Current document - decompressed error]
 |   |   |       |   |   |   └─→ WRITES: [process.DPP_ErrorMessage]
 |   |   |       |   |   |
 |   |   |       |   |   ├─→ shape47: Leave Error Map
 |   |   |       |   |   |   └─→ READS: [process.DPP_ErrorMessage via function]
 |   |   |       |   |   |   └─→ Transform: Error → D365 response (status="failure", success="false")
 |   |   |       |   |   |
 |   |   |       |   |   └─→ shape48: Return Documents [HTTP: 400] [EARLY EXIT]
 |   |   |       |   |       └─→ Response: {"status":"failure", "message":"[decompressed error]", "success":"false"}
 |   |   |       |   |
 |   |   |       |   └─→ IF FALSE (Not gzip):
 |   |   |       |       |
 |   |   |       |       ├─→ shape39: Capture Error Message (Document Properties)
 |   |   |       |       |   └─→ READS: [meta.base.applicationstatusmessage]
 |   |   |       |       |   └─→ WRITES: [process.DPP_ErrorMessage]
 |   |   |       |       |
 |   |   |       |       ├─→ shape40: Leave Error Map
 |   |   |       |       |   └─→ READS: [process.DPP_ErrorMessage via function]
 |   |   |       |       |   └─→ Transform: Error → D365 response (status="failure", success="false")
 |   |   |       |       |
 |   |   |       |       └─→ shape36: Return Documents [HTTP: 400] [EARLY EXIT]
 |   |   |       |           └─→ Response: {"status":"failure", "message":"[HTTP error]", "success":"false"}
 |   |   |
 |   |   [End of TRY PATH]
 |   |
 |   └─→ CATCH PATH (Error during TRY block):
 |       |
 |       ├─→ shape20: Branch (2-path SEQUENTIAL branch)
 |       |   └─→ Classification: SEQUENTIAL (Path 2 depends on Path 1)
 |       |   └─→ Topological Order: Path 1 → Path 2
 |       |   |
 |       |   ├─→ PATH 1 (Sequential - Executes FIRST):
 |       |   |   |
 |       |   |   ├─→ shape19: Capture Error Message (Document Properties)
 |       |   |   |   └─→ READS: [meta.base.catcherrorsmessage]
 |       |   |   |   └─→ WRITES: [process.DPP_ErrorMessage]
 |       |   |   |
 |       |   |   └─→ shape21: Process Call - Email Subprocess
 |       |   |       └─→ SUBPROCESS: (Sub) Office 365 Email (a85945c5-3004-42b9-80b1-104f465cd1fb)
 |       |   |       └─→ Purpose: Send error notification email
 |       |   |       |
 |       |   |       └─→ SUBPROCESS INTERNAL FLOW:
 |       |   |           |
 |       |   |           ├─→ START (Subprocess Entry)
 |       |   |           |
 |       |   |           ├─→ TRY/CATCH (subprocess shape2)
 |       |   |           |   |
 |       |   |           |   ├─→ TRY PATH:
 |       |   |           |   |   |
 |       |   |           |   |   ├─→ Decision: Attachment_Check (subprocess shape4)
 |       |   |           |   |   |   └─→ READS: [process.DPP_HasAttachment]
 |       |   |           |   |   |   └─→ CONDITION: equals "Y"?
 |       |   |           |   |   |   └─→ DATA SOURCE: INPUT (PRE-FILTER)
 |       |   |           |   |   |   |
 |       |   |           |   |   |   ├─→ IF TRUE (With Attachment):
 |       |   |           |   |   |   |   |
 |       |   |           |   |   |   |   ├─→ Build HTML Email Body (subprocess shape11)
 |       |   |           |   |   |   |   |   └─→ READS: [process.DPP_Process_Name, process.DPP_AtomName,
 |       |   |           |   |   |   |   |             process.DPP_ExecutionID, process.DPP_ErrorMessage]
 |       |   |           |   |   |   |   |
 |       |   |           |   |   |   |   ├─→ Set Mail Body Property (subprocess shape14)
 |       |   |           |   |   |   |   |   └─→ WRITES: [process.DPP_MailBody]
 |       |   |           |   |   |   |   |
 |       |   |           |   |   |   |   ├─→ Attach Payload (subprocess shape15)
 |       |   |           |   |   |   |   |   └─→ READS: [process.DPP_Payload]
 |       |   |           |   |   |   |   |
 |       |   |           |   |   |   |   ├─→ Set Mail Properties (subprocess shape6)
 |       |   |           |   |   |   |   |   └─→ WRITES: [connector.mail.fromAddress, connector.mail.toAddress,
 |       |   |           |   |   |   |   |               connector.mail.subject, connector.mail.body, connector.mail.filename]
 |       |   |           |   |   |   |   |   └─→ READS: [process.To_Email, process.DPP_Subject, process.DPP_MailBody,
 |       |   |           |   |   |   |   |             process.DPP_File_Name, Defined Parameters]
 |       |   |           |   |   |   |   |
 |       |   |           |   |   |   |   ├─→ Email w Attachment (subprocess shape3 - Downstream SMTP)
 |       |   |           |   |   |   |   |   └─→ HTTP: [Expected: 250 (SMTP), Error: 500+]
 |       |   |           |   |   |   |   |
 |       |   |           |   |   |   |   └─→ Stop (continue=true) [SUCCESS RETURN]
 |       |   |           |   |   |   |
 |       |   |           |   |   |   └─→ IF FALSE (No Attachment):
 |       |   |           |   |   |       |
 |       |   |           |   |   |       ├─→ Build HTML Email Body (subprocess shape23)
 |       |   |           |   |   |       |   └─→ READS: [process.DPP_Process_Name, process.DPP_AtomName,
 |       |   |           |   |   |       |             process.DPP_ExecutionID, process.DPP_ErrorMessage]
 |       |   |           |   |   |       |
 |       |   |           |   |   |       ├─→ Set Mail Body Property (subprocess shape22)
 |       |   |           |   |   |       |   └─→ WRITES: [process.DPP_MailBody]
 |       |   |           |   |   |       |
 |       |   |           |   |   |       ├─→ Set Mail Properties (subprocess shape20)
 |       |   |           |   |   |       |   └─→ WRITES: [connector.mail.fromAddress, connector.mail.toAddress,
 |       |   |           |   |   |       |               connector.mail.subject, connector.mail.body]
 |       |   |           |   |   |       |   └─→ READS: [process.To_Email, process.DPP_Subject, process.DPP_MailBody,
 |       |   |           |   |   |       |             Defined Parameters]
 |       |   |           |   |   |       |
 |       |   |           |   |   |       ├─→ Email W/O Attachment (subprocess shape7 - Downstream SMTP)
 |       |   |           |   |   |       |   └─→ HTTP: [Expected: 250 (SMTP), Error: 500+]
 |       |   |           |   |   |       |
 |       |   |           |   |   |       └─→ Stop (continue=true) [SUCCESS RETURN]
 |       |   |           |   |
 |       |   |           |   └─→ CATCH PATH (Email error):
 |       |   |           |       |
 |       |   |           |       └─→ Exception (subprocess shape10)
 |       |   |           |           └─→ READS: [meta.base.catcherrorsmessage]
 |       |   |           |           └─→ Throw exception [ERROR RETURN]
 |       |   |           |
 |       |   |           └─→ END SUBPROCESS
 |       |   |
 |       |   └─→ PATH 2 (Sequential - Executes AFTER Path 1):
 |       |       |
 |       |       ├─→ shape41: Leave Error Map
 |       |       |   └─→ READS: [process.DPP_ErrorMessage via function]
 |       |       |   └─→ Transform: Error → D365 response (status="failure", success="false")
 |       |       |
 |       |       └─→ shape43: Return Documents [HTTP: 400] [EARLY EXIT]
 |       |           └─→ Response: {"status":"failure", "message":"[catch error]", "success":"false"}
 |       |
 |       [End of CATCH PATH]
 |
 [END]
```

**Note:** Detailed request/response JSON examples for all operations and return paths are documented in:
- Section 2: Input Structure Analysis (Request JSON)
- Section 3: Response Structure Analysis (Response JSON)
- Section 6: HTTP Status Codes and Return Path Responses (All return path responses)

---

## 15. Function Exposure Decision Table

### ✅ VALIDATION CHECKPOINT: Function Exposure Decision Table Complete

**Purpose:** Determine which Boomi process functions should be exposed as Azure Functions to prevent function explosion and maintain API-Led Architecture principles.

| Boomi Process/Subprocess | Function Type | Expose as Azure Function? | Reasoning | Recommended Approach |
|---|---|---|---|---|
| **HCM_Leave Create** (Main Process) | Process Layer | ✅ **YES** | Entry point for D365 → Oracle Fusion leave creation integration. Orchestrates business logic. | **Process Layer Function:** `CreateLeave` |
| **(Sub) Office 365 Email** (Subprocess) | Utility Subprocess | ❌ **NO** | Generic email notification utility, reusable across processes. Not business-specific. | **System Layer Utility:** Email notification service (shared library or separate utility function) |
| **Leave Create Map** (Map c426b4d6) | Data Transformation | ❌ **NO** | Internal transformation (D365 → Oracle format). Part of Process Layer logic. | **Inline Transformation:** Within `CreateLeave` function (AutoMapper or manual mapping) |
| **Oracle Fusion Leave Response Map** (Map e4fd3f59) | Data Transformation | ❌ **NO** | Internal transformation (Oracle → D365 format). Part of Process Layer logic. | **Inline Transformation:** Within `CreateLeave` function |
| **Leave Error Map** (Map f46b845a) | Data Transformation | ❌ **NO** | Error response transformation. Part of Process Layer logic. | **Inline Transformation:** Within `CreateLeave` function |
| **Oracle Fusion Create** (Operation 6e8920fd) | System Layer API | ✅ **YES** (as System Layer) | Downstream API call to Oracle Fusion HCM. Should be abstracted as System Layer. | **System Layer Function:** `OracleFusionHcm_CreateAbsence` (separate function) |

### Function Exposure Summary

**Process Layer (Experience/Process):**
1. **CreateLeave** (Azure Function)
   - **Type:** HTTP Trigger
   - **Route:** `POST /api/hcm/leaves`
   - **Purpose:** Receive leave request from D365, orchestrate leave creation in Oracle Fusion
   - **Responsibilities:**
     - Input validation
     - Data transformation (D365 → Oracle format)
     - Call System Layer (`OracleFusionHcm_CreateAbsence`)
     - Error handling and error response mapping
     - Response transformation (Oracle → D365 format)
   - **Does NOT expose:** Email notification (handled internally or via separate utility)

**System Layer:**
2. **OracleFusionHcm_CreateAbsence** (Azure Function)
   - **Type:** HTTP Trigger (internal) or Class Library
   - **Route:** `POST /api/system/oracle-fusion-hcm/absences` (if exposed) OR internal library
   - **Purpose:** Abstract Oracle Fusion HCM REST API for absence creation
   - **Responsibilities:**
     - Oracle Fusion API authentication
     - HTTP POST to Oracle Fusion `/absences` endpoint
     - Handle Oracle-specific errors and HTTP status codes
     - Return standardized response (success/error)
   - **Reusability:** Can be used by other Process Layer functions that need to create absences

**Utility/Shared Services (Not Exposed as Functions):**
3. **Email Notification Service**
   - **Type:** Shared Library or Internal Utility Function
   - **Purpose:** Send error notification emails via Office 365 SMTP
   - **Responsibilities:**
     - Build HTML email templates
     - Send email with/without attachments
     - Handle SMTP errors
   - **Reusability:** Used by multiple Process Layer functions for error notifications
   - **Implementation:** Shared .NET library (e.g., `AlGhurair.Common.Notifications`)

### Architecture Alignment

**API-Led Architecture Layers:**

**Experience Layer:** (Not applicable in this process - D365 is the consumer)

**Process Layer:**
- **Function:** `CreateLeave`
- **Responsibility:** Orchestrate leave creation business logic
- **Consumers:** D365 (external system)
- **Dependencies:** System Layer (Oracle Fusion HCM)

**System Layer:**
- **Function:** `OracleFusionHcm_CreateAbsence`
- **Responsibility:** Unlock Oracle Fusion HCM absence data
- **Consumers:** Process Layer functions
- **Dependencies:** Oracle Fusion HCM REST API

**Shared Utilities:**
- **Service:** Email Notification Service
- **Responsibility:** Generic email sending capability
- **Consumers:** Process Layer functions (for error notifications)
- **Dependencies:** Office 365 SMTP

### Benefits of This Approach

1. **Prevents Function Explosion:** Only 2 functions exposed (Process + System), not 5+ functions for every map/subprocess
2. **Separation of Concerns:** Clear boundaries between Process and System layers
3. **Reusability:** System Layer function (`OracleFusionHcm_CreateAbsence`) can be reused by other processes
4. **Maintainability:** Email notification is a shared library, not duplicated across functions
5. **Scalability:** System Layer can be scaled independently based on Oracle API load
6. **Testability:** Each layer can be unit tested independently

### Migration Recommendation

**Phase 1: System Layer**
- Implement `OracleFusionHcm_CreateAbsence` function
- Abstract Oracle Fusion HCM API authentication and error handling
- Expose as internal API or shared library

**Phase 2: Process Layer**
- Implement `CreateLeave` function
- Integrate with System Layer function
- Implement data transformations inline (D365 ↔ Oracle)
- Integrate email notification library for error handling

**Phase 3: Shared Utilities**
- Create `AlGhurair.Common.Notifications` library
- Implement email sending logic
- Reference in Process Layer functions

---

## 16. Critical Patterns Identified

### Pattern 1: POST-OPERATION Error Check

**Identification:**
- Decision shape2 checks HTTP status code from Oracle Fusion API call (shape33)
- Decision shape44 checks response headers from Oracle Fusion API call (shape33)

**Execution Rule:**
- **Oracle Fusion API call (shape33) MUST execute BEFORE both decisions**
- Decisions check RESPONSE data, not INPUT data

**Sequence:**
```
shape33 (Oracle Fusion HTTP POST)
  → Oracle returns response with status code and headers
  → shape2 checks if HTTP status is 20* (success)
  → shape44 checks if response is gzip-encoded (if error)
  → Route to success or error paths based on checks
```

**Azure Migration:**
- Use try/catch blocks around Oracle Fusion HTTP client call
- Check `HttpResponseMessage.StatusCode`
- Check `HttpResponseMessage.Content.Headers.ContentEncoding`
- Route based on status code and content encoding

---

### Pattern 2: Sequential Branch with Data Dependency

**Identification:**
- Branch shape20 has 2 paths
- Path 1 writes process.DPP_ErrorMessage (shape19)
- Path 2 reads process.DPP_ErrorMessage (shape41 via map function)

**Execution Rule:**
- **Path 1 MUST execute BEFORE Path 2**
- Topological sort order: Path 1 → Path 2

**Sequence:**
```
Branch shape20 (SEQUENTIAL)
  ↓
  Path 1: shape19 (Write DPP_ErrorMessage) → shape21 (Email subprocess)
  ↓
  Path 2: shape41 (Read DPP_ErrorMessage) → shape43 (Return error response)
```

**Azure Migration:**
- Execute error notification (Path 1) first
- Then execute error response mapping (Path 2)
- Use sequential async/await pattern

---

### Pattern 3: Conditional Error Response Decompression

**Identification:**
- Decision shape44 checks if error response is gzip-encoded
- TRUE path: Decompress gzip before extracting error message
- FALSE path: Extract error message directly

**Execution Rule:**
- If `Content-Encoding: gzip` → Decompress → Extract error → Map → Return
- If not gzip → Extract error → Map → Return

**Sequence:**
```
IF dynamicdocument.DDP_RespHeader equals "gzip":
  → shape45 (Decompress gzip)
  → shape46 (Capture error message)
  → shape47 (Map error)
  → shape48 (Return error)
ELSE:
  → shape39 (Capture error message)
  → shape40 (Map error)
  → shape36 (Return error)
```

**Azure Migration:**
- Check `HttpResponseMessage.Content.Headers.ContentEncoding`
- If gzip: Use `GZipStream` to decompress response
- Extract error message from decompressed content
- Map to error response DTO

---

### Pattern 4: Subprocess for Reusable Notification

**Identification:**
- Subprocess `(Sub) Office 365 Email` is called from error paths
- Subprocess handles email notification with/without attachments
- Decision in subprocess routes based on attachment requirement

**Execution Rule:**
- Main process sets all required properties before calling subprocess
- Subprocess reads properties and sends email
- Subprocess returns success (Stop continue=true) or throws exception

**Sequence:**
```
Main Process:
  → Set properties (DPP_ErrorMessage, To_Email, DPP_HasAttachment, etc.)
  → Call subprocess (shape21)
  
Subprocess:
  → Read DPP_HasAttachment
  → IF "Y": Build email with attachment → Send
  → IF "N": Build email without attachment → Send
  → Return success (Stop continue=true)
```

**Azure Migration:**
- Create shared `EmailNotificationService` library
- Method: `SendErrorNotificationAsync(errorDetails, hasAttachment, payload)`
- Call from Process Layer function error handlers
- Use async SMTP client

---

### Pattern 5: Try/Catch with Branch Error Handling

**Identification:**
- Try/Catch block (shape17) wraps main processing logic
- CATCH path uses branch (shape20) for parallel error notification and response

**Execution Rule:**
- TRY path: Normal processing (map → HTTP POST → check status → return)
- CATCH path: Capture error → Branch → [Path 1: Email notification, Path 2: Error response]

**Sequence:**
```
TRY:
  → Normal processing flow
CATCH:
  → Branch (shape20)
    → Path 1: Capture error → Send email notification
    → Path 2: Map error → Return error response
```

**Azure Migration:**
- Use try/catch blocks in C#
- Catch block: 
  - Capture exception message
  - Call email notification service (async, don't wait)
  - Map exception to error response DTO
  - Return error response with HTTP 400

---

### Pattern 6: Multi-Path Error Handling

**Identification:**
- Process has 4 distinct error return paths:
  1. Try/Catch error (shape43)
  2. HTTP non-20* without gzip (shape36)
  3. HTTP non-20* with gzip (shape48)
  4. (Implicit) Subprocess email error (exception thrown)

**Execution Rule:**
- All error paths converge on same response structure (Leave D365 Response profile)
- All error paths set `status="failure"` and `success="false"`
- Error message varies based on error source

**Azure Migration:**
- Create unified error response DTO
- Use exception handling middleware or filter
- Map all exception types to consistent error response
- Log error details separately (Application Insights)

---

## 17. System Layer Identification

### Third-Party Systems Identified

#### System 1: Oracle Fusion HCM

**System Name:** Oracle Fusion Human Capital Management  
**Type:** Cloud SaaS - Oracle Fusion HCM  
**Integration Type:** REST API (HTTP)  
**Authentication:** Basic Authentication (configured in connection)  

**Operations:**
- **Create Absence Entry:** POST to `/hcmRestApi/resources/11.13.18.05/absences`
  - **Purpose:** Create employee leave/absence records in Oracle Fusion HCM
  - **Request Format:** JSON
  - **Response Format:** JSON (complex Oracle Fusion response with 80+ fields)
  - **Expected Status:** 200, 201
  - **Error Status:** 400, 404, 500

**System Layer Function Recommendation:**
- **Function Name:** `OracleFusionHcm_CreateAbsence`
- **System Layer API Route:** `POST /api/system/oracle-fusion-hcm/absences`
- **Responsibility:** Abstract Oracle Fusion HCM absence creation API

**Connection Details:**
- **Connection ID:** aa1fcb29-d146-4425-9ea6-b9698090f60e
- **Base URL:** (configured in connection - environment-specific)
- **Headers:**
  - Content-Type: application/json
  - Authorization: Basic (from connection credentials)
- **Response Header Handling:** Content-Encoding mapping for gzip detection

---

#### System 2: Office 365 SMTP (Email)

**System Name:** Office 365 Exchange Online (SMTP)  
**Type:** Cloud SaaS - Microsoft Office 365  
**Integration Type:** SMTP (Mail Connector)  
**Authentication:** SMTP AUTH (username/password configured in connection)  

**Operations:**
- **Send Email with Attachment:** SMTP send operation
  - **Purpose:** Send error notification emails with payload attachment
  - **From:** Boomi.Dev.failures@al-ghurair.com
  - **To:** BoomiIntegrationTeam@al-ghurair.com (configurable)
  - **Body:** HTML email with error details
  - **Attachment:** Original payload as .txt file

- **Send Email without Attachment:** SMTP send operation
  - **Purpose:** Send error notification emails (body only)
  - **From:** Boomi.Dev.failures@al-ghurair.com
  - **To:** BoomiIntegrationTeam@al-ghurair.com (configurable)
  - **Body:** HTML email with error details

**System Layer Function Recommendation:**
- **NOT a System Layer Function** - Should be a shared library/utility
- **Library Name:** `AlGhurair.Common.Notifications`
- **Class:** `EmailNotificationService`
- **Methods:**
  - `SendErrorNotificationAsync(errorDetails, hasAttachment, payload)`
  - `SendHtmlEmailAsync(to, subject, body, attachment)`

**Connection Details:**
- **Connection ID:** 00eae79b-2303-4215-8067-dcc299e42697
- **SMTP Server:** (configured in connection - likely smtp.office365.com)
- **Port:** (configured - likely 587 for TLS)
- **Use SSL:** Yes
- **Use SMTP AUTH:** Yes

---

### System Layer API Catalog

| System | API/Service | Method | Endpoint | System Layer Function | Purpose |
|---|---|---|---|---|---|
| Oracle Fusion HCM | Absences API | POST | /hcmRestApi/resources/11.13.18.05/absences | `OracleFusionHcm_CreateAbsence` | Create employee absence/leave entry |
| Office 365 | SMTP | SMTP | smtp.office365.com:587 | (Shared Library) | Send email notifications |

---

### Integration Summary

**Process Layer Function:** `CreateLeave`
- **Consumes:** D365 request (JSON)
- **Calls System Layer:** `OracleFusionHcm_CreateAbsence`
- **Uses Shared Library:** `EmailNotificationService` (for error notifications)
- **Returns:** D365 response (JSON)

**System Layer Function:** `OracleFusionHcm_CreateAbsence`
- **Consumes:** Oracle Fusion HCM request (JSON)
- **Calls External API:** Oracle Fusion HCM REST API
- **Returns:** Standardized response (personAbsenceEntryId + status)

**Shared Library:** `EmailNotificationService`
- **Consumes:** Error details, email configuration
- **Calls External Service:** Office 365 SMTP
- **Returns:** Success/failure (async, no wait)

---

## 18. Validation Checklist

### ✅ Data Dependencies

- [x] All property WRITES identified (Section 7)
- [x] All property READS identified (Section 7)
- [x] Dependency graph built (Section 8)
- [x] Execution order satisfies all dependencies (Section 13)
- [x] No read-before-write violations

### ✅ Decision Analysis

- [x] ALL decision shapes inventoried (Section 10)
- [x] BOTH TRUE and FALSE paths traced to termination (Section 10)
- [x] Pattern type identified for each decision (Section 10)
- [x] Early exits identified and documented (Section 10)
- [x] Convergence points identified (Section 10)
- [x] Data source analysis complete (INPUT vs RESPONSE vs PROCESS_PROPERTY) (Section 10)
- [x] Decision types classified (PRE-FILTER vs POST-OPERATION) (Section 10)
- [x] Actual execution order verified (Section 10)

### ✅ Branch Analysis

- [x] Each branch classified as parallel or sequential (Section 12)
- [x] API call detection performed (Section 12)
- [x] Classification NOT assumed (analyzed dependencies) (Section 12)
- [x] If sequential: dependency_order built using topological sort (Section 12)
- [x] Each path traced to terminal point (Section 12)
- [x] Convergence points identified (Section 12)
- [x] Execution continuation point determined (Section 12)

### ✅ Sequence Diagram

- [x] Format follows required structure (Section 14)
- [x] Each operation shows READS and WRITES (Section 14)
- [x] Decisions show both TRUE and FALSE paths (Section 14)
- [x] Sequence diagram matches control flow graph from Step 5 (Section 14)
- [x] Execution order matches dependency graph from Step 4 (Section 14)
- [x] Early exits marked [EARLY EXIT] (Section 14)
- [x] Subprocess internal flows documented (Section 14)
- [x] HTTP status codes documented for all return paths (Section 6, 14)
- [x] References to Step 4, 5, 7, 8, 9 included (Section 14)

### ✅ Subprocess Analysis

- [x] ALL subprocesses analyzed (Section 11)
- [x] Internal flow traced (Section 11)
- [x] Return paths identified (Section 11)
- [x] Return path labels mapped to main process shapes (Section 11)
- [x] Properties written by subprocess documented (Section 11)
- [x] Properties read by subprocess from main process documented (Section 11)

### ✅ Input/Output Structure Analysis

- [x] Entry point operation identified (Section 2)
- [x] Request profile identified and loaded (Section 2)
- [x] Request profile structure analyzed (JSON) (Section 2)
- [x] Array vs single object detected (single object) (Section 2)
- [x] Array cardinality documented (N/A) (Section 2)
- [x] ALL request fields extracted (9 fields) (Section 2)
- [x] Request field paths documented (Section 2)
- [x] Request field mapping table generated (Section 2)
- [x] Response profile identified and loaded (Section 3)
- [x] Response profile structure analyzed (Section 3)
- [x] ALL response fields extracted (4 fields) (Section 3)
- [x] Response field mapping table generated (Section 3)
- [x] Document processing behavior determined (Section 2)
- [x] Input/Output structure documented (Sections 2, 3)

### ✅ HTTP Status Codes and Return Path Responses

- [x] All return paths documented with HTTP status codes (Section 6)
- [x] Response JSON examples provided for each return path (Section 6)
- [x] Populated fields documented for each return path (Section 6)
- [x] Decision conditions leading to each return documented (Section 6)
- [x] Error codes and success codes documented (Section 6)
- [x] Downstream operation HTTP status codes documented (Section 6)

### ✅ Map Analysis

- [x] ALL map files identified and loaded (Section 5)
- [x] Field mappings extracted from each map (Section 5)
- [x] Profile vs map field name discrepancies documented (Section 5)
- [x] Scripting functions analyzed (Section 5)
- [x] Static values identified and documented (Section 5)
- [x] Process property mappings documented (Section 5)
- [x] SOAP detection performed (N/A - HTTP/REST only) (Section 5)

### ✅ Self-Check Questions (All Steps)

**Step 7 (Decision Analysis):**
- [x] Decision data sources identified: **YES**
- [x] Decision types classified: **YES**
- [x] Execution order verified: **YES**
- [x] All decision paths traced: **YES**
- [x] Decision patterns identified: **YES**
- [x] Paths traced to termination: **YES**

**Step 8 (Branch Analysis):**
- [x] Classification completed: **YES**
- [x] Assumption check: **NO** (analyzed dependencies)
- [x] Properties extracted: **YES**
- [x] Dependency graph built: **YES**
- [x] Topological sort applied: **YES**

**Step 9 (Execution Order):**
- [x] Business logic verified FIRST: **YES**
- [x] Operation analysis complete: **YES**
- [x] Business logic execution order identified: **YES**
- [x] Data dependencies checked FIRST: **YES**
- [x] Operation response analysis used: **YES**
- [x] Decision analysis used: **YES**
- [x] Dependency graph used: **YES**
- [x] Branch analysis used: **YES**
- [x] Property dependency verification: **YES**
- [x] Topological sort applied: **YES**

### ✅ Phase 1 Document Structure

- [x] Section 1: Operations Inventory - COMPLETE
- [x] Section 2: Input Structure Analysis (Step 1a) - COMPLETE
- [x] Section 3: Response Structure Analysis (Step 1b) - COMPLETE
- [x] Section 4: Operation Response Analysis (Step 1c) - COMPLETE
- [x] Section 5: Map Analysis (Step 1d) - COMPLETE
- [x] Section 6: HTTP Status Codes and Return Path Responses (Step 1e) - COMPLETE
- [x] Section 7: Process Properties Analysis (Steps 2-3) - COMPLETE
- [x] Section 8: Data Dependency Graph (Step 4) - COMPLETE
- [x] Section 9: Control Flow Graph (Step 5) - COMPLETE
- [x] Section 10: Decision Shape Analysis (Step 7) - COMPLETE
- [x] Section 11: Subprocess Analysis (Step 7a) - COMPLETE
- [x] Section 12: Branch Shape Analysis (Step 8) - COMPLETE
- [x] Section 13: Execution Order (Step 9) - COMPLETE
- [x] Section 14: Sequence Diagram (Step 10) - COMPLETE
- [x] Section 15: Function Exposure Decision Table - COMPLETE
- [x] Section 16: Critical Patterns Identified - COMPLETE
- [x] Section 17: System Layer Identification - COMPLETE
- [x] Section 18: Validation Checklist - COMPLETE

### ✅ Mandatory Sections Verification

- [x] All mandatory sections from BOOMI_EXTRACTION_RULES.mdc are present
- [x] All self-check answers documented with YES
- [x] All references between sections documented
- [x] All dependencies verified

### ✅ Final Gate

**Before declaring extraction complete:**

- [x] All Steps 1a-1e, 2-10 completed in order
- [x] All validation checklists completed
- [x] All "NEVER ASSUME" self-checks answered YES
- [x] Sequence diagram cross-checked against JSON dragpoints
- [x] Execution order verified against dependency graph
- [x] Function Exposure Decision Table complete

---

## 🎯 EXTRACTION COMPLETE

**Status:** ✅ **PHASE 1 EXTRACTION COMPLETE**

**Summary:**
- **Process:** HCM_Leave Create
- **Operations:** 4 operations (1 entry point, 1 HTTP downstream, 2 email operations)
- **Subprocess:** 1 subprocess (Email notification)
- **Maps:** 3 maps (request transformation, success response, error response)
- **Decisions:** 3 decisions (2 main process, 1 subprocess)
- **Branches:** 1 branch (sequential, error handling)
- **Return Paths:** 4 return paths (1 success, 3 error scenarios)
- **System Layer APIs:** 1 (Oracle Fusion HCM)
- **Shared Services:** 1 (Email notification)

**Next Steps:**
- ✅ Phase 1 document committed
- ⏭️ Ready for Phase 2: Code Generation (NOT in scope for this task)

**Recommended Azure Functions:**
1. **Process Layer:** `CreateLeave` (main entry point)
2. **System Layer:** `OracleFusionHcm_CreateAbsence` (Oracle Fusion abstraction)
3. **Shared Library:** `AlGhurair.Common.Notifications` (email service)

---

**Document Prepared By:** AI Agent (Cloud Agent)  
**Date:** 2026-02-16  
**Boomi Process Version:** 29  
**Extraction Rules Version:** 2.3
