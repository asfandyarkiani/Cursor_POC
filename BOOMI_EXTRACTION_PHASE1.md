# BOOMI EXTRACTION PHASE 1 - HCM Leave Create Process

**Process Name:** HCM_Leave Create  
**Process ID:** ca69f858-785f-4565-ba1f-b4edc6cca05b  
**Description:** This Process will sync the Leave data between D365 and Oracle HCM  
**Business Domain:** Human Resource (HCM)  
**Date Analyzed:** 2026-02-12

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
15. [Critical Patterns Identified](#15-critical-patterns-identified)
16. [System Layer Identification](#16-system-layer-identification)
17. [Function Exposure Decision Table](#17-function-exposure-decision-table)
18. [Validation Checklist](#18-validation-checklist)

---

## 1. Operations Inventory

### Main Process Operations

| Operation ID | Operation Name | Type | Sub-Type | Purpose |
|---|---|---|---|---|
| 8f709c2b-e63f-4d5f-9374-2932ed70415d | Create Leave Oracle Fusion OP | connector-action | wss | Entry point - Web Service Server Listen |
| 6e8920fd-af5a-430b-a1d9-9fde7ac29a12 | Leave Oracle Fusion Create | connector-action | http | HTTP POST to Oracle Fusion HCM |
| af07502a-fafd-4976-a691-45d51a33b549 | Email w Attachment | connector-action | mail | Send email with attachment |
| 15a72a21-9b57-49a1-a8ed-d70367146644 | Email W/O Attachment | connector-action | mail | Send email without attachment |

### Subprocess Operations (Email Subprocess)

| Subprocess ID | Subprocess Name | Operations |
|---|---|---|
| a85945c5-3004-42b9-80b1-104f465cd1fb | (Sub) Office 365 Email | Email w Attachment, Email W/O Attachment |

### Connections

| Connection ID | Connection Name | Type | Target System |
|---|---|---|---|
| aa1fcb29-d146-4425-9ea6-b9698090f60e | Oracle Fusion | http | Oracle Fusion HCM (https://iaaxey-dev3.fa.ocs.oraclecloud.com:443) |
| 00eae79b-2303-4215-8067-dcc299e42697 | Office 365 Email | mail | Office 365 SMTP (smtp-mail.outlook.com:587) |

---

## 2. Input Structure Analysis (Step 1a)

### Entry Point Operation

**Operation ID:** 8f709c2b-e63f-4d5f-9374-2932ed70415d  
**Operation Name:** Create Leave Oracle Fusion OP  
**Operation Type:** Web Services Server Listen (wss)  
**Input Type:** singlejson  
**Request Profile ID:** febfa3e1-f719-4ee8-ba57-cdae34137ab3  
**Request Profile Name:** D365 Leave Create JSON Profile

### Request Profile Structure

**Profile Type:** profile.json  
**Root Structure:** Root/Object  
**Array Detection:** ❌ NO - Single object structure  
**Input Type:** singlejson

### Input JSON Structure

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
- **Implementation Pattern:** Process single leave request, return single response
- **Session Management:** One session per execution

### Request Field Mapping

| Boomi Field Path | Boomi Field Name | Data Type | Required | Azure DTO Property | Notes |
|---|---|---|---|---|---|
| Root/Object/employeeNumber | employeeNumber | number | Yes | EmployeeNumber | Tracked field (hr_employee_id) |
| Root/Object/absenceType | absenceType | character | Yes | AbsenceType | Leave type (Sick Leave, Annual Leave, etc.) |
| Root/Object/employer | employer | character | Yes | Employer | Employer name |
| Root/Object/startDate | startDate | character | Yes | StartDate | Leave start date (YYYY-MM-DD) |
| Root/Object/endDate | endDate | character | Yes | EndDate | Leave end date (YYYY-MM-DD) |
| Root/Object/absenceStatusCode | absenceStatusCode | character | Yes | AbsenceStatusCode | Status code (SUBMITTED, APPROVED, etc.) |
| Root/Object/approvalStatusCode | approvalStatusCode | character | Yes | ApprovalStatusCode | Approval status |
| Root/Object/startDateDuration | startDateDuration | number | Yes | StartDateDuration | Duration for start date (1 = full day) |
| Root/Object/endDateDuration | endDateDuration | number | Yes | EndDateDuration | Duration for end date (1 = full day) |

**Total Fields:** 9 fields (all required)

---

## 3. Response Structure Analysis (Step 1b)

### Response Profile

**Response Profile ID:** febfa3e1-f719-4ee8-ba57-cdae34137ab3 (same as request - bidirectional)  
**Response Profile Name:** D365 Leave Create JSON Profile  
**Profile Type:** profile.json  
**Output Type:** singlejson

### Actual Response Profile Used

**Response Profile ID:** f4ca3a70-114a-4601-bad8-44a3eb20e2c0  
**Response Profile Name:** Leave D365 Response  
**Profile Type:** profile.json

### Response JSON Structure

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

**Total Fields:** 4 fields

---

## 4. Operation Response Analysis (Step 1c)

### Operation: Leave Oracle Fusion Create (6e8920fd-af5a-430b-a1d9-9fde7ac29a12)

**Response Profile ID:** None (NONE - responseProfileType: NONE)  
**Response Type:** Raw JSON response from Oracle Fusion  
**Response Structure:** Oracle Fusion HCM absence entry response

**Expected Response Fields (from profile 316175c7-0e45-4869-9ac6-5f9d69882a62):**
- personAbsenceEntryId (number) - Primary identifier
- absenceStatusCd (character)
- approvalStatusCd (character)
- startDate (character)
- endDate (character)
- absenceType (character)
- employer (character)
- personNumber (character)
- And 70+ additional Oracle Fusion fields

**Extracted Fields:**
- **personAbsenceEntryId** extracted by map shape34 (map_e4fd3f59-edb5-43a1-aeae-143b600a064e)
- Written to response profile field: leaveResponse/Object/personAbsenceEntryId

**Consumers:**
- shape34 (Map: Oracle Fusion Leave Response Map) - maps Oracle response to D365 response format
- shape35 (Return Documents) - returns success response to caller

**Business Logic Implications:**
- Oracle Fusion Create operation MUST execute successfully before response mapping
- HTTP status code check (shape2) determines success/error path
- Response contains personAbsenceEntryId which is returned to D365

### Response Header Mapping

**Operation Configuration:**
- Response header "Content-Encoding" mapped to document property "DDP_RespHeader"
- Used to detect gzip compression (checked by shape44 decision)

---

## 5. Map Analysis (Step 1d)

### Map Inventory

| Map ID | Map Name | From Profile | To Profile | Type |
|---|---|---|---|---|
| c426b4d6-2aff-450e-b43b-59956c4dbc96 | Leave Create Map | febfa3e1-f719-4ee8-ba57-cdae34137ab3 | a94fa205-c740-40a5-9fda-3d018611135a | Request transformation |
| e4fd3f59-edb5-43a1-aeae-143b600a064e | Oracle Fusion Leave Response Map | 316175c7-0e45-4869-9ac6-5f9d69882a62 | f4ca3a70-114a-4601-bad8-44a3eb20e2c0 | Response transformation |
| f46b845a-7d75-41b5-b0ad-c41a6a8e9b12 | Leave Error Map | 23d7a2e9-5cb0-4e9c-9e4b-3154834bad0d | f4ca3a70-114a-4601-bad8-44a3eb20e2c0 | Error transformation |

### Map 1: Leave Create Map (c426b4d6-2aff-450e-b43b-59956c4dbc96)

**Purpose:** Transform D365 request to Oracle Fusion request format

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

| Profile Field Name | Map Field Name (ACTUAL) | Authority | Use in Request |
|---|---|---|---|
| employeeNumber | personNumber | ✅ MAP | personNumber |
| absenceStatusCode | absenceStatusCd | ✅ MAP | absenceStatusCd |
| approvalStatusCode | approvalStatusCd | ✅ MAP | approvalStatusCd |

**CRITICAL RULE:** Map field names are AUTHORITATIVE. Use map field names in Oracle Fusion requests.

### Map 2: Oracle Fusion Leave Response Map (e4fd3f59-edb5-43a1-aeae-143b600a064e)

**Purpose:** Transform Oracle Fusion response to D365 response format

**Field Mappings:**

| Source Field | Source Type | Target Field | Transformation |
|---|---|---|---|
| personAbsenceEntryId | profile | personAbsenceEntryId | Direct mapping |
| (static) | default | status | Default: "success" |
| (static) | default | message | Default: "Data successfully sent to Oracle Fusion" |
| (static) | default | success | Default: "true" |

**Default Values:**
- status: "success"
- message: "Data successfully sent to Oracle Fusion"
- success: "true"

### Map 3: Leave Error Map (f46b845a-7d75-41b5-b0ad-c41a6a8e9b12)

**Purpose:** Transform error information to D365 error response format

**Field Mappings:**

| Source Field | Source Type | Target Field | Transformation |
|---|---|---|---|
| DPP_ErrorMessage | function (PropertyGet) | message | Get process property |
| (static) | default | status | Default: "failure" |
| (static) | default | success | Default: "false" |

**Function Analysis:**

**Function 1: Get Dynamic Process Property**
- Type: PropertyGet
- Input: Property Name = "DPP_ErrorMessage"
- Output: Result (error message text)
- Purpose: Retrieve error message from process property

**Default Values:**
- status: "failure"
- success: "false"

**Scripting Functions:** None

---

## 6. HTTP Status Codes and Return Path Responses (Step 1e)

### Return Path 1: Success Response (shape35)

**Return Label:** "Success Response"  
**Return Shape ID:** shape35  
**HTTP Status Code:** 200  
**Decision Conditions Leading to Return:**
- shape2 (HTTP Status 20 check): meta.base.applicationstatuscode matches "20*" → TRUE path

**Populated Response Fields:**

| Field Name | Field Path | Source | Populated By |
|---|---|---|---|
| personAbsenceEntryId | leaveResponse/Object/personAbsenceEntryId | operation_response | Oracle Fusion Create (shape33) |
| status | leaveResponse/Object/status | static (map default) | Map shape34 |
| message | leaveResponse/Object/message | static (map default) | Map shape34 |
| success | leaveResponse/Object/success | static (map default) | Map shape34 |

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

### Return Path 2: Error Response - Try/Catch Error (shape43)

**Return Label:** "Error Response"  
**Return Shape ID:** shape43  
**HTTP Status Code:** 400  
**Decision Conditions Leading to Return:**
- shape17 (Try/Catch): Error in try block → Catch path
- shape20 (Branch): Path 2 (map error response)

**Error Code:** PROCESS_ERROR  
**Populated Response Fields:**

| Field Name | Field Path | Source | Populated By |
|---|---|---|---|
| message | leaveResponse/Object/message | process_property | DPP_ErrorMessage (shape19) |
| status | leaveResponse/Object/status | static (map default) | Map shape41 |
| success | leaveResponse/Object/success | static (map default) | Map shape41 |

**Response JSON Example:**

```json
{
  "leaveResponse": {
    "status": "failure",
    "message": "Error occurred during processing: [actual error message]",
    "success": "false"
  }
}
```

### Return Path 3: Error Response - HTTP Non-20x with GZIP (shape48)

**Return Label:** "Error Response"  
**Return Shape ID:** shape48  
**HTTP Status Code:** 400  
**Decision Conditions Leading to Return:**
- shape2 (HTTP Status 20 check): meta.base.applicationstatuscode does NOT match "20*" → FALSE path
- shape44 (Check Response Content Type): DDP_RespHeader equals "gzip" → TRUE path
- shape45 (Data Process): Decompress gzip response
- shape46 (Document Properties): Extract error message from decompressed response

**Error Code:** ORACLE_FUSION_ERROR  
**Populated Response Fields:**

| Field Name | Field Path | Source | Populated By |
|---|---|---|---|
| message | leaveResponse/Object/message | current document (decompressed) | shape46 |
| status | leaveResponse/Object/status | static (map default) | Map shape47 |
| success | leaveResponse/Object/success | static (map default) | Map shape47 |

**Response JSON Example:**

```json
{
  "leaveResponse": {
    "status": "failure",
    "message": "[Oracle Fusion error message from decompressed response]",
    "success": "false"
  }
}
```

### Return Path 4: Error Response - HTTP Non-20x without GZIP (shape36)

**Return Label:** "Error Response"  
**Return Shape ID:** shape36  
**HTTP Status Code:** 400  
**Decision Conditions Leading to Return:**
- shape2 (HTTP Status 20 check): meta.base.applicationstatuscode does NOT match "20*" → FALSE path
- shape44 (Check Response Content Type): DDP_RespHeader does NOT equal "gzip" → FALSE path
- shape39 (Document Properties): Extract error message from response

**Error Code:** ORACLE_FUSION_ERROR  
**Populated Response Fields:**

| Field Name | Field Path | Source | Populated By |
|---|---|---|---|
| message | leaveResponse/Object/message | track property | meta.base.applicationstatusmessage (shape39) |
| status | leaveResponse/Object/status | static (map default) | Map shape40 |
| success | leaveResponse/Object/success | static (map default) | Map shape40 |

**Response JSON Example:**

```json
{
  "leaveResponse": {
    "status": "failure",
    "message": "[Oracle Fusion error message]",
    "success": "false"
  }
}
```

### Downstream Operations HTTP Status Codes

| Operation Name | Expected Success Codes | Error Codes | Error Handling |
|---|---|---|---|
| Leave Oracle Fusion Create | 200, 201, 202 (20*) | 400, 401, 403, 404, 500 | Check HTTP status, handle gzip compression, return error |
| Email w Attachment | N/A (mail) | N/A | Throw exception on error (subprocess) |
| Email W/O Attachment | N/A (mail) | N/A | Throw exception on error (subprocess) |

---

## 7. Process Properties Analysis (Steps 2-3)

### Property WRITES (Step 2)

| Property Name | Written By Shape(s) | Value Source | Purpose |
|---|---|---|---|
| process.DPP_Process_Name | shape38 | Execution property: Process Name | Store process name for logging |
| process.DPP_AtomName | shape38 | Execution property: Atom Name | Store atom name for logging |
| process.DPP_Payload | shape38 | Current document | Store input payload for error reporting |
| process.DPP_ExecutionID | shape38 | Execution property: Execution Id | Store execution ID for tracking |
| process.DPP_File_Name | shape38 | Concatenation: Process Name + Timestamp + ".txt" | Generate filename for email attachment |
| process.DPP_Subject | shape38 | Concatenation: Atom Name + " (" + Process Name + " ) has errors to report" | Generate email subject |
| process.To_Email | shape38 | Defined property: PP_HCM_LeaveCreate_Properties.To_Email | Email recipient(s) |
| process.DPP_HasAttachment | shape38 | Defined property: PP_HCM_LeaveCreate_Properties.DPP_HasAttachment | Flag for email attachment ("Y" or "N") |
| dynamicdocument.URL | shape8 | Defined property: PP_HCM_LeaveCreate_Properties.Resource_Path | Oracle Fusion REST API resource path |
| process.DPP_ErrorMessage | shape19 | Track property: meta.base.catcherrorsmessage | Store try/catch error message |
| process.DPP_ErrorMessage | shape39 | Track property: meta.base.applicationstatusmessage | Store HTTP error message |
| process.DPP_ErrorMessage | shape46 | Current document (decompressed) | Store decompressed error message |

### Property READS (Step 3)

| Property Name | Read By Shape(s) | Usage |
|---|---|---|
| dynamicdocument.URL | shape33 (operation) | HTTP request URL path element |
| process.DPP_HasAttachment | subprocess shape4 (decision) | Determine email attachment requirement |
| process.To_Email | subprocess shape6, shape20 | Email recipient address |
| process.DPP_Subject | subprocess shape6, shape20 | Email subject line |
| process.DPP_MailBody | subprocess shape6, shape20 | Email body content |
| process.DPP_File_Name | subprocess shape6 | Email attachment filename |
| process.DPP_Process_Name | subprocess shape11, shape23 | Process name in email body |
| process.DPP_AtomName | subprocess shape11, shape23 | Atom name in email body |
| process.DPP_ExecutionID | subprocess shape11, shape23 | Execution ID in email body |
| process.DPP_ErrorMessage | subprocess shape11, shape23, map function | Error message in email body / response |
| process.DPP_Payload | subprocess shape15 | Payload content for email attachment |
| dynamicdocument.DDP_RespHeader | shape44 (decision) | Check response content encoding |

### Defined Process Properties

**Component: PP_HCM_LeaveCreate_Properties (e22c04db-7dfa-4b1b-9d88-77365b0fdb18)**

| Property Key | Property Name | Type | Default Value |
|---|---|---|---|
| c2d1a1e8-4bcb-435b-b1d2-bb10a209bcc0 | Resource_Path | string | hcmRestApi/resources/11.13.18.05/absences |
| 71c90f6e-4f86-49f3-8aef-bae6668c73f9 | To_Email | string | BoomiIntegrationTeam@al-ghurair.com |
| a717867b-67f4-4a72-be2d-c99c73309fdb | DPP_HasAttachment | string | Y |

**Component: PP_Office365_Email (0feff13f-8a2c-438a-b3c7-1909e2a7f533)**

| Property Key | Property Name | Type | Default Value |
|---|---|---|---|
| 804b6d40-eeee-4ac0-ab4b-94e1aca9f4b7 | From_Email | string | Boomi.Dev.failures@al-ghurair.com |
| 600acadb-ee02-4369-af85-ee70af380b6c | To_Email | string | Rajesh.Muppala@al-ghurair.com;mohan.jonnalagadda@al-ghurair.com |
| 2fa6ce9e-437a-44cc-b44f-5c7e61052f41 | HasAttachment | string | Y |
| 3ca9f307-cecb-4d1e-b9ec-007839509ed7 | EmailBody | string | (empty) |
| d8fc7496-ec2c-4308-a4ca-e9f49c0c9500 | Environment | string | DEV Failure : |

---

## 8. Data Dependency Graph (Step 4)

### Dependency Chains

**Chain 1: Input Properties Setup**
```
shape1 (START) 
  → shape38 (Input_details - WRITES: DPP_Process_Name, DPP_AtomName, DPP_Payload, DPP_ExecutionID, DPP_File_Name, DPP_Subject, To_Email, DPP_HasAttachment)
  → shape17 (Try/Catch)
```

**Chain 2: Main Processing Path**
```
shape38 (WRITES: DPP_*)
  → shape17 (Try/Catch)
  → shape29 (Map: Leave Create Map)
  → shape8 (set URL - WRITES: dynamicdocument.URL)
  → shape49 (Notify - READS: current document)
  → shape33 (Leave Oracle Fusion Create - READS: dynamicdocument.URL)
  → shape2 (HTTP Status 20 check - READS: meta.base.applicationstatuscode)
```

**Chain 3: Success Path**
```
shape2 (TRUE path)
  → shape34 (Map: Oracle Fusion Leave Response Map)
  → shape35 (Return Documents - Success Response)
```

**Chain 4: Error Path - HTTP Non-20x**
```
shape2 (FALSE path)
  → shape44 (Check Response Content Type - READS: dynamicdocument.DDP_RespHeader)
  → shape44 (TRUE path - gzip)
    → shape45 (Decompress gzip)
    → shape46 (error msg - WRITES: DPP_ErrorMessage)
    → shape47 (Map: Leave Error Map - READS: DPP_ErrorMessage)
    → shape48 (Return Documents - Error Response)
  → shape44 (FALSE path - no gzip)
    → shape39 (error msg - WRITES: DPP_ErrorMessage)
    → shape40 (Map: Leave Error Map - READS: DPP_ErrorMessage)
    → shape36 (Return Documents - Error Response)
```

**Chain 5: Error Path - Try/Catch**
```
shape17 (Catch path)
  → shape20 (Branch)
  → Path 1:
    → shape19 (ErrorMsg - WRITES: DPP_ErrorMessage)
    → shape21 (ProcessCall: Email Subprocess - READS: DPP_Process_Name, DPP_AtomName, DPP_ExecutionID, DPP_ErrorMessage, DPP_Payload, DPP_File_Name, DPP_Subject, To_Email, DPP_HasAttachment)
  → Path 2:
    → shape41 (Map: Leave Error Map - READS: DPP_ErrorMessage)
    → shape43 (Return Documents - Error Response)
```

### Property Summary

**Properties Creating Dependencies:**

1. **dynamicdocument.URL**
   - Written by: shape8
   - Read by: shape33 (operation)
   - Dependency: shape8 MUST execute before shape33

2. **process.DPP_ErrorMessage**
   - Written by: shape19, shape39, shape46
   - Read by: shape21 (subprocess), map functions (shape41, shape40, shape47)
   - Dependency: Error capture shapes MUST execute before error response mapping

3. **process.DPP_Process_Name, DPP_AtomName, DPP_ExecutionID, DPP_Payload, DPP_File_Name, DPP_Subject, To_Email, DPP_HasAttachment**
   - Written by: shape38
   - Read by: shape21 (subprocess)
   - Dependency: shape38 MUST execute before subprocess call

4. **dynamicdocument.DDP_RespHeader**
   - Written by: shape33 (operation response header mapping)
   - Read by: shape44 (decision)
   - Dependency: shape33 MUST execute before shape44

---

## 9. Control Flow Graph (Step 5)

### Main Process Control Flow

| From Shape | To Shape | Identifier | Description |
|---|---|---|---|
| shape1 (START) | shape38 | default | Start to Input_details |
| shape38 (Input_details) | shape17 | default | Input_details to Try/Catch |
| shape17 (Try/Catch) | shape29 | default (Try) | Try path to Map |
| shape17 (Try/Catch) | shape20 | error (Catch) | Catch path to Branch |
| shape29 (Map) | shape8 | default | Map to set URL |
| shape8 (set URL) | shape49 | default | set URL to Notify |
| shape49 (Notify) | shape33 | default | Notify to Oracle Fusion Create |
| shape33 (Oracle Fusion Create) | shape2 | default | Oracle Fusion Create to HTTP Status check |
| shape2 (HTTP Status 20 check) | shape34 | true | TRUE path to success map |
| shape2 (HTTP Status 20 check) | shape44 | false | FALSE path to content type check |
| shape34 (Map) | shape35 | default | Map to Success Response |
| shape44 (Check Response Content Type) | shape45 | true | TRUE path to decompress |
| shape44 (Check Response Content Type) | shape39 | false | FALSE path to error msg |
| shape45 (Decompress) | shape46 | default | Decompress to error msg |
| shape46 (error msg) | shape47 | default | error msg to Map |
| shape47 (Map) | shape48 | default | Map to Error Response |
| shape39 (error msg) | shape40 | default | error msg to Map |
| shape40 (Map) | shape36 | default | Map to Error Response |
| shape20 (Branch) | shape19 | 1 | Branch path 1 to ErrorMsg |
| shape20 (Branch) | shape41 | 2 | Branch path 2 to Map |
| shape19 (ErrorMsg) | shape21 | default | ErrorMsg to ProcessCall |
| shape41 (Map) | shape43 | default | Map to Error Response |

### Subprocess Control Flow (Email Subprocess)

| From Shape | To Shape | Identifier | Description |
|---|---|---|---|
| shape1 (START) | shape2 | default | Start to Try/Catch |
| shape2 (Try/Catch) | shape4 | default (Try) | Try path to Attachment_Check |
| shape2 (Try/Catch) | shape10 | error (Catch) | Catch path to Exception |
| shape4 (Attachment_Check) | shape11 | true | TRUE path to Mail_Body (with attachment) |
| shape4 (Attachment_Check) | shape23 | false | FALSE path to Mail_Body (no attachment) |
| shape11 (Mail_Body) | shape14 | default | Mail_Body to set_MailBody |
| shape14 (set_MailBody) | shape15 | default | set_MailBody to payload |
| shape15 (payload) | shape6 | default | payload to set_Mail_Properties |
| shape6 (set_Mail_Properties) | shape3 | default | set_Mail_Properties to Email |
| shape3 (Email) | shape5 | default | Email to Stop |
| shape23 (Mail_Body) | shape22 | default | Mail_Body to set_MailBody |
| shape22 (set_MailBody) | shape20 | default | set_MailBody to set_Mail_Properties |
| shape20 (set_Mail_Properties) | shape7 | default | set_Mail_Properties to Email |
| shape7 (Email) | shape9 | default | Email to Stop |

**Connection Summary:**
- Total shapes in main process: 19
- Total connections in main process: 21
- Shapes with multiple outgoing connections: 3 (shape17 - Try/Catch, shape2 - Decision, shape44 - Decision, shape20 - Branch)
- Total shapes in subprocess: 13
- Total connections in subprocess: 11
- Shapes with multiple outgoing connections in subprocess: 2 (shape2 - Try/Catch, shape4 - Decision)

### Reverse Flow Mapping (Step 6)

**Convergence Points:**

None identified in main process - all paths lead to distinct return documents.

**Subprocess Convergence:**

None identified - attachment vs no-attachment paths are independent and both lead to Stop shapes.

---

## 10. Decision Shape Analysis (Step 7)

### ✅ Self-Check Results

- ✅ **Decision data sources identified:** YES
- ✅ **Decision types classified:** YES
- ✅ **Execution order verified:** YES
- ✅ **All decision paths traced:** YES
- ✅ **Decision patterns identified:** YES
- ✅ **Paths traced to termination:** YES

### Decision 1: HTTP Status 20 check (shape2)

**Shape ID:** shape2  
**Comparison Type:** wildcard  
**Value 1:** meta.base.applicationstatuscode (track property)  
**Value 2:** "20*" (static)

**Data Source:** TRACK_PROPERTY (meta.base.applicationstatuscode)  
**Decision Type:** POST_OPERATION  
**Actual Execution Order:** Operation shape33 (Oracle Fusion Create) → Response → Decision shape2 → Route based on HTTP status

**TRUE Path:**
- **Destination:** shape34 (Map: Oracle Fusion Leave Response Map)
- **Termination:** shape35 (Return Documents - Success Response)
- **Type:** return
- **Path:** shape2 → shape34 → shape35

**FALSE Path:**
- **Destination:** shape44 (Check Response Content Type)
- **Termination:** shape36 or shape48 (Return Documents - Error Response)
- **Type:** return (after nested decision)
- **Path:** shape2 → shape44 → [shape45 → shape46 → shape47 → shape48] OR [shape39 → shape40 → shape36]

**Pattern:** Error Check (Success vs Failure based on HTTP status)  
**Convergence Point:** None (paths terminate at different return shapes)  
**Early Exit:** Both paths lead to return (TRUE = success return, FALSE = error return)

**Business Logic:**
- Checks HTTP response status code from Oracle Fusion API call
- If status code matches 20* (200, 201, 202, etc.) → Success path
- If status code does NOT match 20* → Error path (check for gzip compression)

### Decision 2: Check Response Content Type (shape44)

**Shape ID:** shape44  
**Comparison Type:** equals  
**Value 1:** dynamicdocument.DDP_RespHeader (track property)  
**Value 2:** "gzip" (static)

**Data Source:** TRACK_PROPERTY (dynamicdocument.DDP_RespHeader - response header from operation)  
**Decision Type:** POST_OPERATION  
**Actual Execution Order:** Operation shape33 (Oracle Fusion Create) → Response headers captured → Decision shape44 → Route based on content encoding

**TRUE Path:**
- **Destination:** shape45 (Data Process - Decompress gzip)
- **Termination:** shape48 (Return Documents - Error Response)
- **Type:** return
- **Path:** shape44 → shape45 → shape46 → shape47 → shape48

**FALSE Path:**
- **Destination:** shape39 (Document Properties - error msg)
- **Termination:** shape36 (Return Documents - Error Response)
- **Type:** return
- **Path:** shape44 → shape39 → shape40 → shape36

**Pattern:** Conditional Logic (Handle gzip compression)  
**Convergence Point:** None (both paths terminate at different return shapes)  
**Early Exit:** Both paths lead to return (error responses)

**Business Logic:**
- Checks if Oracle Fusion error response is gzip compressed
- If compressed (Content-Encoding: gzip) → Decompress before extracting error message
- If not compressed → Extract error message directly

### Decision 3: Attachment_Check (subprocess shape4)

**Shape ID:** shape4 (in subprocess a85945c5-3004-42b9-80b1-104f465cd1fb)  
**Comparison Type:** equals  
**Value 1:** process.DPP_HasAttachment (process property)  
**Value 2:** "Y" (static)

**Data Source:** PROCESS_PROPERTY (DPP_HasAttachment)  
**Decision Type:** PRE_FILTER  
**Actual Execution Order:** Decision shape4 → Route to email operation based on attachment flag

**TRUE Path:**
- **Destination:** shape11 (Message - Mail_Body with attachment)
- **Termination:** shape5 (Stop - continue=true)
- **Type:** stop (continue)
- **Path:** shape4 → shape11 → shape14 → shape15 → shape6 → shape3 → shape5

**FALSE Path:**
- **Destination:** shape23 (Message - Mail_Body without attachment)
- **Termination:** shape9 (Stop - continue=true)
- **Type:** stop (continue)
- **Path:** shape4 → shape23 → shape22 → shape20 → shape7 → shape9

**Pattern:** Conditional Logic (Optional Processing - with/without attachment)  
**Convergence Point:** None (both paths lead to Stop shapes)  
**Early Exit:** No (both paths complete successfully)

**Business Logic:**
- Checks if email should include attachment
- If "Y" → Send email with attachment (operation af07502a)
- If not "Y" → Send email without attachment (operation 15a72a21)

---

## 11. Subprocess Analysis (Step 7a)

### Subprocess: (Sub) Office 365 Email (a85945c5-3004-42b9-80b1-104f465cd1fb)

**Purpose:** Send error notification email via Office 365 SMTP

**Internal Flow:**

1. **START** (shape1)
2. **Try/Catch** (shape2)
   - **Try Path:**
     - **Attachment_Check Decision** (shape4)
       - **TRUE Path (with attachment):**
         - shape11: Build email body HTML with error details
         - shape14: Store email body in DPP_MailBody
         - shape15: Get payload for attachment
         - shape6: Set mail properties (from, to, subject, body, filename)
         - shape3: Send email with attachment (operation af07502a)
         - shape5: Stop (continue=true) - SUCCESS RETURN
       - **FALSE Path (without attachment):**
         - shape23: Build email body HTML with error details
         - shape22: Store email body in DPP_MailBody
         - shape20: Set mail properties (from, to, subject, body)
         - shape7: Send email without attachment (operation 15a72a21)
         - shape9: Stop (continue=true) - SUCCESS RETURN
   - **Catch Path:**
     - shape10: Throw exception with error message - ERROR RETURN

**Return Paths:**

| Return Label | Termination Shape | Type | Condition |
|---|---|---|---|
| (implicit success) | shape5 (Stop continue=true) | success | Email sent successfully (with attachment) |
| (implicit success) | shape9 (Stop continue=true) | success | Email sent successfully (without attachment) |
| (exception) | shape10 (Exception) | error | Email send failed |

**Main Process Mapping:**

| Subprocess Return | Main Process Next Shape |
|---|---|
| Success (Stop continue=true) | (subprocess completes, main process ends) |
| Exception | (exception propagates to main process) |

**Properties Written by Subprocess:**

- process.DPP_MailBody (written by shape14 or shape22)
- connector.mail.fromAddress (written by shape6 or shape20)
- connector.mail.toAddress (written by shape6 or shape20)
- connector.mail.subject (written by shape6 or shape20)
- connector.mail.body (written by shape6 or shape20)
- connector.mail.filename (written by shape6)

**Properties Read by Subprocess (from main process):**

- process.DPP_Process_Name
- process.DPP_AtomName
- process.DPP_ExecutionID
- process.DPP_ErrorMessage
- process.DPP_Payload
- process.DPP_File_Name
- process.DPP_Subject
- process.To_Email
- process.DPP_HasAttachment
- PP_Office365_Email.From_Email (defined property)
- PP_Office365_Email.Environment (defined property)

**Business Logic:**
- Subprocess is called only on error (try/catch error path)
- Sends notification email to support team with error details
- Includes execution context (process name, atom, execution ID)
- Optionally attaches payload for debugging

---

## 12. Branch Shape Analysis (Step 8)

### ✅ Self-Check Results

- ✅ **Classification completed:** YES
- ✅ **Assumption check:** NO (analyzed dependencies)
- ✅ **Properties extracted:** YES
- ✅ **Dependency graph built:** YES
- ✅ **Topological sort applied:** YES (for sequential branch)

### Branch 1: Error Handling Branch (shape20)

**Shape ID:** shape20  
**Number of Paths:** 2  
**Location:** Try/Catch error path

**Path Properties Analysis:**

**Path 1 (shape20 → shape19):**
- **READS:** meta.base.catcherrorsmessage (track property)
- **WRITES:** process.DPP_ErrorMessage

**Path 2 (shape20 → shape41):**
- **READS:** process.DPP_ErrorMessage (via map function)
- **WRITES:** None (maps to response profile)

**Dependency Graph:**

```
Path 1 (shape19) → Path 2 (shape41)
Reason: Path 2 reads process.DPP_ErrorMessage which Path 1 writes
```

**Classification:** SEQUENTIAL

**Reasoning:**
- Path 2 depends on Path 1 (reads property written by Path 1)
- Path 1 MUST execute before Path 2

**Dependency Order (Topological Sort):**
1. Path 1: shape19 (ErrorMsg) → shape21 (ProcessCall: Email Subprocess)
2. Path 2: shape41 (Map: Leave Error Map) → shape43 (Return Documents)

**Path Termination:**

| Path | Terminal Shape | Type |
|---|---|---|
| Path 1 | shape21 (ProcessCall) | subprocess call (then process ends) |
| Path 2 | shape43 (Return Documents) | return |

**Convergence Points:** None (paths terminate independently)

**Execution Continues From:** None (both paths terminate)

**Business Logic:**
- Path 1: Send error notification email to support team
- Path 2: Return error response to caller
- Both paths execute sequentially to ensure error is logged and reported

---

## 13. Execution Order (Step 9)

### ✅ Self-Check Results

- ✅ **Business logic verified FIRST:** YES
- ✅ **Operation analysis complete:** YES
- ✅ **Business logic execution order identified:** YES
- ✅ **Data dependencies checked FIRST:** YES
- ✅ **Operation response analysis used:** YES (Step 1c)
- ✅ **Decision analysis used:** YES (Step 7)
- ✅ **Dependency graph used:** YES (Step 4)
- ✅ **Branch analysis used:** YES (Step 8)
- ✅ **Property dependency verification:** YES
- ✅ **Topological sort applied:** YES

### Business Logic Flow (Step 0)

**Operation 1: Create Leave Oracle Fusion OP (8f709c2b-e63f-4d5f-9374-2932ed70415d)**
- **Purpose:** Entry point - Receives leave request from D365
- **Outputs:** Input document (leave request JSON)
- **Dependent Operations:** All subsequent operations depend on input
- **Business Flow:** Entry point MUST execute FIRST (receives request from D365)

**Operation 2: Leave Oracle Fusion Create (6e8920fd-af5a-430b-a1d9-9fde7ac29a12)**
- **Purpose:** Create absence entry in Oracle Fusion HCM
- **Outputs:** 
  - HTTP response (Oracle Fusion absence entry)
  - Response headers (Content-Encoding)
  - Track properties (meta.base.applicationstatuscode, meta.base.applicationstatusmessage)
- **Dependent Operations:** 
  - shape2 (HTTP Status check) - reads meta.base.applicationstatuscode
  - shape44 (Content Type check) - reads dynamicdocument.DDP_RespHeader
  - shape34 (Response map) - reads response document
- **Business Flow:** Oracle Fusion Create MUST execute BEFORE status check and response mapping

**Operation 3: Email w Attachment (af07502a-fafd-4976-a691-45d51a33b549)**
- **Purpose:** Send error notification email with attachment
- **Outputs:** Email sent (no response data)
- **Dependent Operations:** None
- **Business Flow:** Executes only on error, after error message is captured

**Operation 4: Email W/O Attachment (15a72a21-9b57-49a1-a8ed-d70367146644)**
- **Purpose:** Send error notification email without attachment
- **Outputs:** Email sent (no response data)
- **Dependent Operations:** None
- **Business Flow:** Executes only on error, after error message is captured

### Execution Order (Detailed)

**Main Flow:**

1. **START** (shape1)
2. **Input_details** (shape38) - WRITES: DPP_Process_Name, DPP_AtomName, DPP_Payload, DPP_ExecutionID, DPP_File_Name, DPP_Subject, To_Email, DPP_HasAttachment
3. **Try/Catch** (shape17)
   - **TRY PATH:**
     4. **Map: Leave Create Map** (shape29) - Transform D365 request to Oracle Fusion format
     5. **set URL** (shape8) - WRITES: dynamicdocument.URL
     6. **Notify** (shape49) - Log request payload (INFO level)
     7. **Leave Oracle Fusion Create** (shape33) - HTTP POST to Oracle Fusion - READS: dynamicdocument.URL
     8. **HTTP Status 20 check** (shape2) - READS: meta.base.applicationstatuscode
        - **IF TRUE (20*):** → Success Path
          9. **Map: Oracle Fusion Leave Response Map** (shape34)
          10. **Return Documents: Success Response** (shape35) [HTTP: 200] [SUCCESS]
        - **IF FALSE (non-20*):** → Error Path
          9. **Check Response Content Type** (shape44) - READS: dynamicdocument.DDP_RespHeader
             - **IF TRUE (gzip):**
               10. **Data Process: Decompress gzip** (shape45)
               11. **error msg** (shape46) - WRITES: DPP_ErrorMessage
               12. **Map: Leave Error Map** (shape47) - READS: DPP_ErrorMessage
               13. **Return Documents: Error Response** (shape48) [HTTP: 400] [EARLY EXIT]
             - **IF FALSE (no gzip):**
               10. **error msg** (shape39) - WRITES: DPP_ErrorMessage (from meta.base.applicationstatusmessage)
               11. **Map: Leave Error Map** (shape40) - READS: DPP_ErrorMessage
               12. **Return Documents: Error Response** (shape36) [HTTP: 400] [EARLY EXIT]
   - **CATCH PATH:**
     4. **Branch** (shape20) - 2 paths (SEQUENTIAL)
        - **Path 1 (FIRST):**
          5. **ErrorMsg** (shape19) - WRITES: DPP_ErrorMessage (from meta.base.catcherrorsmessage)
          6. **ProcessCall: Email Subprocess** (shape21) - READS: DPP_Process_Name, DPP_AtomName, DPP_ExecutionID, DPP_ErrorMessage, DPP_Payload, DPP_File_Name, DPP_Subject, To_Email, DPP_HasAttachment
             - **SUBPROCESS INTERNAL FLOW:**
               - START → Try/Catch → Attachment_Check Decision
                 - IF TRUE (DPP_HasAttachment = "Y"):
                   - Mail_Body → set_MailBody → payload → set_Mail_Properties → Email w Attachment → Stop (continue=true)
                 - IF FALSE (DPP_HasAttachment ≠ "Y"):
                   - Mail_Body → set_MailBody → set_Mail_Properties → Email W/O Attachment → Stop (continue=true)
                 - CATCH: Exception (throw error)
        - **Path 2 (SECOND):**
          5. **Map: Leave Error Map** (shape41) - READS: DPP_ErrorMessage
          6. **Return Documents: Error Response** (shape43) [HTTP: 400] [EARLY EXIT]

### Dependency Verification

**Verified Dependencies:**

1. ✅ shape8 (set URL) MUST execute BEFORE shape33 (Oracle Fusion Create)
   - Reason: shape33 reads dynamicdocument.URL which shape8 writes

2. ✅ shape33 (Oracle Fusion Create) MUST execute BEFORE shape2 (HTTP Status check)
   - Reason: shape2 reads meta.base.applicationstatuscode which shape33 produces

3. ✅ shape33 (Oracle Fusion Create) MUST execute BEFORE shape44 (Content Type check)
   - Reason: shape44 reads dynamicdocument.DDP_RespHeader which shape33 produces

4. ✅ shape19 (ErrorMsg) MUST execute BEFORE shape21 (ProcessCall) and shape41 (Map)
   - Reason: Both read process.DPP_ErrorMessage which shape19 writes

5. ✅ shape38 (Input_details) MUST execute BEFORE shape21 (ProcessCall)
   - Reason: subprocess reads DPP_Process_Name, DPP_AtomName, DPP_ExecutionID, etc. which shape38 writes

6. ✅ Branch Path 1 (shape19 → shape21) MUST execute BEFORE Path 2 (shape41 → shape43)
   - Reason: Path 2 reads process.DPP_ErrorMessage which Path 1 writes

**All property reads happen after property writes:** ✅ VERIFIED

---

## 14. Sequence Diagram (Step 10)

**📋 NOTE:** This diagram shows the technical execution flow. Detailed request/response JSON examples are documented in Section 6 (HTTP Status Codes and Return Path Responses).

**Based on:**
- Dependency graph in Step 4 (Section 8)
- Decision analysis in Step 7 (Section 10)
- Control flow graph in Step 5 (Section 9)
- Branch analysis in Step 8 (Section 12)
- Execution order in Step 9 (Section 13)

```
START
 |
 ├─→ shape38: Input_details (Document Properties)
 |   └─→ WRITES: [process.DPP_Process_Name, process.DPP_AtomName, process.DPP_Payload, 
 |                 process.DPP_ExecutionID, process.DPP_File_Name, process.DPP_Subject, 
 |                 process.To_Email, process.DPP_HasAttachment]
 |
 ├─→ shape17: Try/Catch
 |   |
 |   ├─→ TRY PATH:
 |   |   |
 |   |   ├─→ shape29: Map - Leave Create Map
 |   |   |   └─→ Transform: D365 request → Oracle Fusion request format
 |   |   |   └─→ Field mappings: employeeNumber→personNumber, absenceStatusCode→absenceStatusCd, etc.
 |   |   |
 |   |   ├─→ shape8: set URL (Document Properties)
 |   |   |   └─→ WRITES: [dynamicdocument.URL = "hcmRestApi/resources/11.13.18.05/absences"]
 |   |   |
 |   |   ├─→ shape49: Notify (Log request payload)
 |   |   |   └─→ READS: [current document]
 |   |   |   └─→ Log Level: INFO
 |   |   |
 |   |   ├─→ shape33: Leave Oracle Fusion Create (Downstream - HTTP POST)
 |   |   |   └─→ READS: [dynamicdocument.URL]
 |   |   |   └─→ WRITES: [meta.base.applicationstatuscode, meta.base.applicationstatusmessage, 
 |   |   |                 dynamicdocument.DDP_RespHeader (Content-Encoding header)]
 |   |   |   └─→ HTTP: [Expected: 200/201/202 (20*), Error: 400/401/403/404/500]
 |   |   |   └─→ Target: Oracle Fusion HCM (https://iaaxey-dev3.fa.ocs.oraclecloud.com:443)
 |   |   |   └─→ Method: POST
 |   |   |   └─→ Content-Type: application/json
 |   |   |   └─→ Auth: Basic Authentication
 |   |   |
 |   |   ├─→ shape2: Decision - HTTP Status 20 check
 |   |   |   └─→ READS: [meta.base.applicationstatuscode]
 |   |   |   └─→ Condition: meta.base.applicationstatuscode matches "20*"?
 |   |   |   |
 |   |   |   ├─→ IF TRUE (20*): → Success Path
 |   |   |   |   |
 |   |   |   |   ├─→ shape34: Map - Oracle Fusion Leave Response Map
 |   |   |   |   |   └─→ Transform: Oracle Fusion response → D365 response format
 |   |   |   |   |   └─→ Extract: personAbsenceEntryId
 |   |   |   |   |   └─→ Set defaults: status="success", message="Data successfully sent to Oracle Fusion", success="true"
 |   |   |   |   |
 |   |   |   |   └─→ shape35: Return Documents [HTTP: 200] [SUCCESS]
 |   |   |   |       └─→ Response: {"leaveResponse": {"status": "success", "message": "Data successfully sent to Oracle Fusion", 
 |   |   |   |                      "personAbsenceEntryId": 300000123456789, "success": "true"}}
 |   |   |   |
 |   |   |   └─→ IF FALSE (non-20*): → Error Path
 |   |   |       |
 |   |   |       ├─→ shape44: Decision - Check Response Content Type
 |   |   |       |   └─→ READS: [dynamicdocument.DDP_RespHeader]
 |   |   |       |   └─→ Condition: DDP_RespHeader equals "gzip"?
 |   |   |       |   |
 |   |   |       |   ├─→ IF TRUE (gzip): → Decompress and extract error
 |   |   |       |   |   |
 |   |   |       |   |   ├─→ shape45: Data Process - Decompress gzip
 |   |   |       |   |   |   └─→ Script: GZIPInputStream decompression (Groovy)
 |   |   |       |   |   |
 |   |   |       |   |   ├─→ shape46: error msg (Document Properties)
 |   |   |       |   |   |   └─→ READS: [current document (decompressed)]
 |   |   |       |   |   |   └─→ WRITES: [process.DPP_ErrorMessage]
 |   |   |       |   |   |
 |   |   |       |   |   ├─→ shape47: Map - Leave Error Map
 |   |   |       |   |   |   └─→ READS: [process.DPP_ErrorMessage]
 |   |   |       |   |   |   └─→ Set defaults: status="failure", success="false"
 |   |   |       |   |   |
 |   |   |       |   |   └─→ shape48: Return Documents [HTTP: 400] [EARLY EXIT]
 |   |   |       |   |       └─→ Response: {"leaveResponse": {"status": "failure", "message": "[error from decompressed response]", "success": "false"}}
 |   |   |       |   |
 |   |   |       |   └─→ IF FALSE (no gzip): → Extract error directly
 |   |   |       |       |
 |   |   |       |       ├─→ shape39: error msg (Document Properties)
 |   |   |       |       |   └─→ READS: [meta.base.applicationstatusmessage]
 |   |   |       |       |   └─→ WRITES: [process.DPP_ErrorMessage]
 |   |   |       |       |
 |   |   |       |       ├─→ shape40: Map - Leave Error Map
 |   |   |       |       |   └─→ READS: [process.DPP_ErrorMessage]
 |   |   |       |       |   └─→ Set defaults: status="failure", success="false"
 |   |   |       |       |
 |   |   |       |       └─→ shape36: Return Documents [HTTP: 400] [EARLY EXIT]
 |   |   |       |           └─→ Response: {"leaveResponse": {"status": "failure", "message": "[Oracle Fusion error message]", "success": "false"}}
 |   |
 |   └─→ CATCH PATH: → Error notification and response
 |       |
 |       ├─→ shape20: Branch (2 paths - SEQUENTIAL)
 |           |
 |           ├─→ Path 1 (FIRST): → Send error notification email
 |           |   |
 |           |   ├─→ shape19: ErrorMsg (Document Properties)
 |           |   |   └─→ READS: [meta.base.catcherrorsmessage]
 |           |   |   └─→ WRITES: [process.DPP_ErrorMessage]
 |           |   |
 |           |   └─→ shape21: ProcessCall - Email Subprocess
 |           |       └─→ READS: [process.DPP_Process_Name, process.DPP_AtomName, process.DPP_ExecutionID, 
 |           |                   process.DPP_ErrorMessage, process.DPP_Payload, process.DPP_File_Name, 
 |           |                   process.DPP_Subject, process.To_Email, process.DPP_HasAttachment]
 |           |       |
 |           |       └─→ SUBPROCESS: (Sub) Office 365 Email
 |           |           |
 |           |           ├─→ START
 |           |           |
 |           |           ├─→ Try/Catch
 |           |           |   |
 |           |           |   ├─→ TRY PATH:
 |           |           |   |   |
 |           |           |   |   ├─→ Decision: Attachment_Check
 |           |           |   |   |   └─→ READS: [process.DPP_HasAttachment]
 |           |           |   |   |   └─→ Condition: DPP_HasAttachment equals "Y"?
 |           |           |   |   |   |
 |           |           |   |   |   ├─→ IF TRUE (Y): → Send email with attachment
 |           |           |   |   |   |   |
 |           |           |   |   |   |   ├─→ Mail_Body (Message) - Build HTML email body
 |           |           |   |   |   |   |   └─→ READS: [process.DPP_Process_Name, process.DPP_AtomName, 
 |           |           |   |   |   |   |               process.DPP_ExecutionID, process.DPP_ErrorMessage]
 |           |           |   |   |   |   |
 |           |           |   |   |   |   ├─→ set_MailBody (Document Properties)
 |           |           |   |   |   |   |   └─→ WRITES: [process.DPP_MailBody]
 |           |           |   |   |   |   |
 |           |           |   |   |   |   ├─→ payload (Message)
 |           |           |   |   |   |   |   └─→ READS: [process.DPP_Payload]
 |           |           |   |   |   |   |
 |           |           |   |   |   |   ├─→ set_Mail_Properties (Document Properties)
 |           |           |   |   |   |   |   └─→ WRITES: [connector.mail.fromAddress, connector.mail.toAddress, 
 |           |           |   |   |   |   |                 connector.mail.subject, connector.mail.body, connector.mail.filename]
 |           |           |   |   |   |   |   └─→ READS: [PP_Office365_Email.From_Email, process.To_Email, 
 |           |           |   |   |   |   |               PP_Office365_Email.Environment, process.DPP_Subject, 
 |           |           |   |   |   |   |               process.DPP_MailBody, process.DPP_File_Name]
 |           |           |   |   |   |   |
 |           |           |   |   |   |   ├─→ Email w Attachment (Downstream - Mail)
 |           |           |   |   |   |   |   └─→ Target: Office 365 SMTP (smtp-mail.outlook.com:587)
 |           |           |   |   |   |   |   └─→ Auth: SMTP AUTH
 |           |           |   |   |   |   |   └─→ Body Content Type: text/html
 |           |           |   |   |   |   |   └─→ Attachment: Payload as text file
 |           |           |   |   |   |   |
 |           |           |   |   |   |   └─→ Stop (continue=true) [SUCCESS RETURN]
 |           |           |   |   |   |
 |           |           |   |   |   └─→ IF FALSE (not Y): → Send email without attachment
 |           |           |   |   |       |
 |           |           |   |   |       ├─→ Mail_Body (Message) - Build HTML email body
 |           |           |   |   |       |   └─→ READS: [process.DPP_Process_Name, process.DPP_AtomName, 
 |           |           |   |   |       |               process.DPP_ExecutionID, process.DPP_ErrorMessage]
 |           |           |   |   |       |
 |           |           |   |   |       ├─→ set_MailBody (Document Properties)
 |           |           |   |   |       |   └─→ WRITES: [process.DPP_MailBody]
 |           |           |   |   |       |
 |           |           |   |   |       ├─→ set_Mail_Properties (Document Properties)
 |           |           |   |   |       |   └─→ WRITES: [connector.mail.fromAddress, connector.mail.toAddress, 
 |           |           |   |   |       |                 connector.mail.subject, connector.mail.body]
 |           |           |   |   |       |   └─→ READS: [PP_Office365_Email.From_Email, process.To_Email, 
 |           |           |   |   |       |               PP_Office365_Email.Environment, process.DPP_Subject, 
 |           |           |   |   |       |               process.DPP_MailBody]
 |           |           |   |   |       |
 |           |           |   |   |       ├─→ Email W/O Attachment (Downstream - Mail)
 |           |           |   |   |       |   └─→ Target: Office 365 SMTP (smtp-mail.outlook.com:587)
 |           |           |   |   |       |   └─→ Auth: SMTP AUTH
 |           |           |   |   |       |   └─→ Body Content Type: text/plain
 |           |           |   |   |       |
 |           |           |   |   |       └─→ Stop (continue=true) [SUCCESS RETURN]
 |           |           |   |
 |           |           |   └─→ CATCH PATH:
 |           |           |       └─→ Exception (throw error) [ERROR RETURN]
 |           |           |
 |           |           └─→ END SUBPROCESS
 |           |
 |           └─→ Path 2 (SECOND): → Return error response
 |               |
 |               ├─→ shape41: Map - Leave Error Map
 |               |   └─→ READS: [process.DPP_ErrorMessage]
 |               |   └─→ Set defaults: status="failure", success="false"
 |               |
 |               └─→ shape43: Return Documents [HTTP: 400] [EARLY EXIT]
 |                   └─→ Response: {"leaveResponse": {"status": "failure", "message": "[try/catch error message]", "success": "false"}}
```

**Note:** Detailed request/response JSON examples for all operations and return paths are documented in Section 6 (HTTP Status Codes and Return Path Responses).

---

## 15. Critical Patterns Identified

### Pattern 1: Error Check with Multiple Return Paths

**Identification:**
- Decision shape2 checks HTTP status code from Oracle Fusion API
- TRUE path (20*) → Success response
- FALSE path (non-20*) → Error response with nested decision for gzip handling

**Execution Rule:**
- Oracle Fusion Create operation MUST execute BEFORE status check
- If status is 20* → Map success response and return
- If status is non-20* → Check for gzip compression, extract error message, map error response, and return

**Pattern Type:** Error Check (Success vs Failure)

### Pattern 2: Conditional Error Handling (Gzip Compression)

**Identification:**
- Decision shape44 checks if error response is gzip compressed
- TRUE path → Decompress response before extracting error message
- FALSE path → Extract error message directly

**Execution Rule:**
- HTTP status check MUST execute BEFORE content type check
- If Content-Encoding is gzip → Decompress using Groovy script, then extract error
- If not gzip → Extract error message from track property

**Pattern Type:** Conditional Logic (Optional Processing)

### Pattern 3: Sequential Branch for Error Notification and Response

**Identification:**
- Branch shape20 has 2 paths with data dependency
- Path 1 writes DPP_ErrorMessage and sends email notification
- Path 2 reads DPP_ErrorMessage and returns error response

**Execution Rule:**
- Path 1 MUST execute BEFORE Path 2 (topological sort)
- Path 1 captures error, sends notification email
- Path 2 maps error to response format and returns to caller

**Pattern Type:** Sequential Branch (Data Dependency)

### Pattern 4: Subprocess with Conditional Execution (Email with/without Attachment)

**Identification:**
- Subprocess decision checks DPP_HasAttachment flag
- TRUE path → Send email with payload attachment
- FALSE path → Send email without attachment

**Execution Rule:**
- Decision routes to appropriate email operation
- Both paths lead to success (Stop continue=true)
- Error path throws exception

**Pattern Type:** Conditional Logic (Optional Processing)

### Pattern 5: Try/Catch with Error Notification

**Identification:**
- Try/Catch wraps main processing logic
- Catch path sends error notification email and returns error response

**Execution Rule:**
- Try path executes main business logic
- Catch path captures error, sends notification, returns error response
- Email subprocess is called only on error

**Pattern Type:** Try/Catch Error Handling

---

## 16. System Layer Identification

### Third-Party Systems

| System Name | Type | Connection | Operations | Purpose |
|---|---|---|---|---|
| Oracle Fusion HCM | REST API | Oracle Fusion (aa1fcb29) | Leave Oracle Fusion Create | Create absence entries in Oracle HCM |
| Office 365 Email | SMTP | Office 365 Email (00eae79b) | Email w Attachment, Email W/O Attachment | Send error notification emails |

### System Layer APIs Required

**1. Oracle Fusion HCM System Layer API**

- **API Name:** OracleFusionHCMAbsenceApi
- **Purpose:** Create absence entries in Oracle Fusion HCM
- **Endpoint:** POST /hcmRestApi/resources/11.13.18.05/absences
- **Base URL:** https://iaaxey-dev3.fa.ocs.oraclecloud.com:443
- **Authentication:** Basic Authentication
- **Request Format:** JSON
- **Response Format:** JSON
- **Operations:**
  - CreateAbsence: Create new absence entry

**2. Office 365 Email System Layer API**

- **API Name:** Office365EmailApi
- **Purpose:** Send email notifications via Office 365 SMTP
- **Endpoint:** SMTP (smtp-mail.outlook.com:587)
- **Authentication:** SMTP AUTH
- **Operations:**
  - SendEmailWithAttachment: Send email with attachment
  - SendEmailWithoutAttachment: Send email without attachment

---

## 17. Function Exposure Decision Table

### Process Layer Function

| Attribute | Value |
|---|---|
| **Function Name** | CreateLeaveInOracleFusion |
| **HTTP Method** | POST |
| **Route** | /api/hcm/leaves |
| **Purpose** | Sync leave data from D365 to Oracle Fusion HCM |
| **Expose as HTTP Function?** | ✅ YES |
| **Reasoning** | Entry point is Web Service Server (wss) - designed for external HTTP calls from D365 |

**Function Signature:**
```csharp
[FunctionName("CreateLeaveInOracleFusion")]
public async Task<IActionResult> CreateLeaveInOracleFusion(
    [HttpTrigger(AuthorizationLevel.Function, "post", Route = "hcm/leaves")] HttpRequest req,
    ILogger log)
```

**Request DTO:**
```csharp
public class LeaveCreateRequest
{
    public int EmployeeNumber { get; set; }
    public string AbsenceType { get; set; }
    public string Employer { get; set; }
    public string StartDate { get; set; }
    public string EndDate { get; set; }
    public string AbsenceStatusCode { get; set; }
    public string ApprovalStatusCode { get; set; }
    public int StartDateDuration { get; set; }
    public int EndDateDuration { get; set; }
}
```

**Response DTO:**
```csharp
public class LeaveCreateResponse
{
    public LeaveResponseData LeaveResponse { get; set; }
}

public class LeaveResponseData
{
    public string Status { get; set; }
    public string Message { get; set; }
    public long? PersonAbsenceEntryId { get; set; }
    public string Success { get; set; }
}
```

### System Layer Functions

| System | Function Name | Expose as HTTP Function? | Reasoning |
|---|---|---|---|
| Oracle Fusion HCM | CreateAbsenceInOracleFusion | ❌ NO | System Layer - called by Process Layer only |
| Office 365 Email | SendErrorNotificationEmail | ❌ NO | System Layer - called by Process Layer only |

**Validation:**
- ✅ Only 1 Process Layer function exposed (CreateLeaveInOracleFusion)
- ✅ System Layer functions are internal (not exposed)
- ✅ No function explosion - single entry point for leave creation

---

## 18. Validation Checklist

### Data Dependencies
- ✅ All property WRITES identified (12 properties)
- ✅ All property READS identified (12 properties)
- ✅ Dependency graph built (documented in Section 8)
- ✅ Execution order satisfies all dependencies (no read-before-write)

### Decision Analysis
- ✅ ALL decision shapes inventoried (3 decisions: 2 main process, 1 subprocess)
- ✅ BOTH TRUE and FALSE paths traced to termination
- ✅ Pattern type identified for each decision
- ✅ Early exits identified and documented (3 error return paths)
- ✅ Convergence points identified (none - all paths terminate independently)
- ✅ Decision data sources identified (TRACK_PROPERTY, PROCESS_PROPERTY)
- ✅ Decision types classified (POST_OPERATION, PRE_FILTER)
- ✅ Actual execution order verified

### Branch Analysis
- ✅ Each branch classified as parallel or sequential (1 branch: SEQUENTIAL)
- ✅ **SELF-CHECK:** Did I check for API calls in branch paths? (Answer: YES - no API calls in branch)
- ✅ **SELF-CHECK:** Did I classify or assume? (Answer: Classified based on data dependencies)
- ✅ If sequential: dependency_order built using topological sort (Path 1 → Path 2)
- ✅ Each path traced to terminal point
- ✅ Convergence points identified (none)
- ✅ Execution continuation point determined (none - paths terminate)

### Sequence Diagram
- ✅ Format follows required structure (Operation → Decision → Operation)
- ✅ Each operation shows READS and WRITES
- ✅ Decisions show both TRUE and FALSE paths
- ✅ **CRITICAL:** Check-before-create patterns shown correctly (N/A - no check-before-create)
- ✅ **SELF-CHECK:** Did I verify check happens BEFORE create? (Answer: N/A)
- ✅ **CROSS-VALIDATION:** Sequence diagram matches control flow graph from Step 5
- ✅ **CROSS-VALIDATION:** Execution order matches dependency graph from Step 4
- ✅ Early exits marked [EARLY EXIT] (3 error returns)
- ✅ Conditional execution marked [Only if condition X] (gzip decompression)
- ✅ Subprocess internal flows documented (email subprocess)
- ✅ Subprocess return paths mapped to main process

### Subprocess Analysis
- ✅ ALL subprocesses analyzed (1 subprocess: Email)
- ✅ Internal flow traced (START → Try/Catch → Decision → Email operations → Stop)
- ✅ Return paths identified (success: Stop continue=true, error: Exception)
- ✅ Return path labels mapped to main process shapes
- ✅ Properties written by subprocess documented (DPP_MailBody, connector.mail.*)
- ✅ Properties read by subprocess from main process documented (8 properties)

### Edge Cases
- ✅ Nested branches/decisions analyzed (shape44 nested in shape2 FALSE path)
- ✅ Loops identified (none)
- ✅ Property chains traced (DPP_ErrorMessage chain)
- ✅ Circular dependencies detected and resolved (none)
- ✅ Try/Catch error paths documented

### Property Extraction Completeness
- ✅ All property patterns searched (${}, %%, {})
- ✅ Message parameters checked for process properties
- ✅ Operation headers/path parameters checked
- ✅ Decision track properties identified (meta.base.*)
- ✅ Document properties that read other properties identified

### Input/Output Structure Analysis (CONTRACT VERIFICATION)
- ✅ Entry point operation identified (Create Leave Oracle Fusion OP)
- ✅ Request profile identified and loaded (D365 Leave Create JSON Profile)
- ✅ Request profile structure analyzed (JSON)
- ✅ Array vs single object detected (single object)
- ✅ Array cardinality documented (N/A - single object)
- ✅ ALL request fields extracted (9 fields)
- ✅ Request field paths documented
- ✅ Request field mapping table generated (Boomi → Azure DTO)
- ✅ Response profile identified and loaded (Leave D365 Response)
- ✅ Response profile structure analyzed (JSON)
- ✅ ALL response fields extracted (4 fields)
- ✅ Response field mapping table generated
- ✅ Document processing behavior determined (single document)
- ✅ Input/Output structure documented in Phase 1 document (Sections 2 & 3)

### HTTP Status Codes and Return Path Responses
- ✅ Section 6 (HTTP Status Codes and Return Path Responses - Step 1e) present
- ✅ All return paths documented with HTTP status codes (4 return paths)
- ✅ Response JSON examples provided for each return path
- ✅ Populated fields documented for each return path (source and populated by)
- ✅ Decision conditions leading to each return documented
- ✅ Error codes and success codes documented for each return path
- ✅ Downstream operation HTTP status codes documented (expected success and error codes)
- ✅ Error handling strategy documented for downstream operations

### Map Analysis
- ✅ ALL map files identified and loaded (3 maps)
- ✅ SOAP request maps identified (N/A - HTTP JSON, not SOAP)
- ✅ Field mappings extracted from each map
- ✅ Profile vs map field name discrepancies documented (absenceStatusCode vs absenceStatusCd)
- ✅ Map field names marked as AUTHORITATIVE for requests
- ✅ Scripting functions analyzed (1 function: PropertyGet in error map)
- ✅ Static values identified and documented (default values in response maps)
- ✅ Process property mappings documented (DPP_ErrorMessage)
- ✅ Map Analysis documented in Phase 1 document (Section 5)

### Self-Check Questions (MANDATORY)

**Step 1a (Input Structure Analysis):**
- ✅ Did I complete input structure analysis? (Answer: YES)

**Step 1b (Response Structure Analysis):**
- ✅ Did I complete response structure analysis? (Answer: YES)

**Step 1c (Operation Response Analysis):**
- ✅ Did I analyze operation responses? (Answer: YES)

**Step 1d (Map Analysis):**
- ✅ Did I analyze ALL maps? (Answer: YES - 3 maps)
- ✅ Did I identify field mappings? (Answer: YES)
- ✅ Did I compare profile vs map field names? (Answer: YES)
- ✅ Did I mark map field names as AUTHORITATIVE? (Answer: YES)

**Step 1e (HTTP Status Codes and Return Paths):**
- ✅ Did I extract HTTP status codes for all return paths? (Answer: YES - 4 paths)
- ✅ Did I document response JSON for each return path? (Answer: YES)
- ✅ Did I document populated fields for each return path? (Answer: YES)
- ✅ Did I extract HTTP status codes for downstream operations? (Answer: YES)

**Step 7 (Decision Analysis):**
- ✅ Did I identify data source for EVERY decision? (Answer: YES)
- ✅ Did I classify each decision type? (Answer: YES)
- ✅ Did I verify actual execution order for decisions? (Answer: YES)
- ✅ Did I trace BOTH TRUE and FALSE paths for EVERY decision? (Answer: YES)
- ✅ Did I identify the pattern for each decision? (Answer: YES)
- ✅ Did I trace paths to termination? (Answer: YES)

**Step 8 (Branch Analysis):**
- ✅ Did I classify each branch as parallel or sequential? (Answer: YES - SEQUENTIAL)
- ✅ Did I assume branches are parallel? (Answer: NO - analyzed dependencies)
- ✅ Did I extract properties read/written by each path? (Answer: YES)
- ✅ Did I build dependency graph between paths? (Answer: YES)
- ✅ Did I apply topological sort if sequential? (Answer: YES)

**Step 9 (Execution Order):**
- ✅ Did I verify business logic FIRST before following dragpoints? (Answer: YES)
- ✅ Did I identify what each operation does and what it produces? (Answer: YES)
- ✅ Did I identify which operations MUST execute first based on business logic? (Answer: YES)
- ✅ Did I check data dependencies FIRST before following dragpoints? (Answer: YES)
- ✅ Did I use operation response analysis from Step 1c? (Answer: YES)
- ✅ Did I use decision analysis from Step 7? (Answer: YES)
- ✅ Did I use dependency graph from Step 4? (Answer: YES)
- ✅ Did I use branch analysis from Step 8? (Answer: YES)
- ✅ Did I verify all property reads happen after property writes? (Answer: YES)
- ✅ Did I follow topological sort order for sequential branches? (Answer: YES)

**Final Validation:**
- ✅ All Steps 1-10 completed in order
- ✅ All validation checklists completed
- ✅ All "NEVER ASSUME" self-checks answered YES
- ✅ Sequence diagram cross-checked against JSON dragpoints
- ✅ Execution order verified against dependency graph
- ✅ Function Exposure Decision Table complete

---

**PHASE 1 EXTRACTION COMPLETE ✅**

**Summary:**
- Process analyzed: HCM_Leave Create
- Operations identified: 4 (1 entry point, 1 HTTP, 2 mail)
- Subprocess analyzed: 1 (Email notification)
- Decisions analyzed: 3 (2 main process, 1 subprocess)
- Branches analyzed: 1 (sequential)
- Return paths: 4 (1 success, 3 error)
- System Layer APIs identified: 2 (Oracle Fusion HCM, Office 365 Email)
- Process Layer functions to expose: 1 (CreateLeaveInOracleFusion)

**Ready for Phase 2: Code Generation**
