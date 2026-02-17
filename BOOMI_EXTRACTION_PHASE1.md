# BOOMI EXTRACTION PHASE 1 - HCM Leave Create Process

**Process Name:** HCM_Leave Create  
**Process ID:** ca69f858-785f-4565-ba1f-b4edc6cca05b  
**Description:** This Process will sync the Leave data between D365 and Oracle HCM  
**Business Domain:** Human Resource Management (HCM)  
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
11. [Branch Shape Analysis (Step 8)](#11-branch-shape-analysis-step-8)
12. [Execution Order (Step 9)](#12-execution-order-step-9)
13. [Sequence Diagram (Step 10)](#13-sequence-diagram-step-10)
14. [Subprocess Analysis (Step 7a)](#14-subprocess-analysis-step-7a)
15. [System Layer Identification](#15-system-layer-identification)
16. [Critical Patterns Identified](#16-critical-patterns-identified)
17. [Validation Checklist](#17-validation-checklist)
18. [Function Exposure Decision Table](#18-function-exposure-decision-table)

---

## 1. Operations Inventory

### Main Process Operations

| Operation ID | Operation Name | Type | Sub-Type | Purpose |
|---|---|---|---|---|
| 8f709c2b-e63f-4d5f-9374-2932ed70415d | Create Leave Oracle Fusion OP | connector-action | wss | Entry point - Web Service Server Listen operation |
| 6e8920fd-af5a-430b-a1d9-9fde7ac29a12 | Leave Oracle Fusion Create | connector-action | http | HTTP POST to Oracle Fusion HCM REST API |
| af07502a-fafd-4976-a691-45d51a33b549 | Email w Attachment | connector-action | mail | Send email with attachment (error notification) |
| 15a72a21-9b57-49a1-a8ed-d70367146644 | Email W/O Attachment | connector-action | mail | Send email without attachment (error notification) |

### Subprocess Operations

| Subprocess ID | Subprocess Name | Purpose |
|---|---|---|
| a85945c5-3004-42b9-80b1-104f465cd1fb | (Sub) Office 365 Email | Email notification subprocess for error handling |

### Connections

| Connection ID | Connection Name | Type | Purpose |
|---|---|---|---|
| aa1fcb29-d146-4425-9ea6-b9698090f60e | Oracle Fusion | http | Connection to Oracle Fusion HCM Cloud |
| 00eae79b-2303-4215-8067-dcc299e42697 | Office 365 Email | mail | SMTP connection for email notifications |

### Profiles

| Profile ID | Profile Name | Type | Purpose |
|---|---|---|---|
| febfa3e1-f719-4ee8-ba57-cdae34137ab3 | D365 Leave Create JSON Profile | profile.json | Request profile (from D365) |
| a94fa205-c740-40a5-9fda-3d018611135a | HCM Leave Create JSON Profile | profile.json | Oracle Fusion request profile |
| 316175c7-0e45-4869-9ac6-5f9d69882a62 | Oracle Fusion Leave Response JSON Profile | profile.json | Oracle Fusion response profile |
| f4ca3a70-114a-4601-bad8-44a3eb20e2c0 | Leave D365 Response | profile.json | D365 response profile |
| 23d7a2e9-5cb0-4e9c-9e4b-3154834bad0d | Dummy FF Profile | profile.flatfile | Dummy profile for error mapping |

### Maps

| Map ID | Map Name | From Profile | To Profile | Purpose |
|---|---|---|---|---|
| c426b4d6-2aff-450e-b43b-59956c4dbc96 | Leave Create Map | febfa3e1 | a94fa205 | Transform D365 request to Oracle Fusion format |
| e4fd3f59-edb5-43a1-aeae-143b600a064e | Oracle Fusion Leave Response Map | 316175c7 | f4ca3a70 | Transform Oracle Fusion response to D365 response |
| f46b845a-7d75-41b5-b0ad-c41a6a8e9b12 | Leave Error Map | 23d7a2e9 | f4ca3a70 | Map error messages to D365 response format |

---

## 2. Input Structure Analysis (Step 1a)

### ✅ MANDATORY STEP - CONTRACT VERIFICATION

**Entry Point Operation:** Create Leave Oracle Fusion OP (8f709c2b-e63f-4d5f-9374-2932ed70415d)  
**Operation Type:** Web Services Server (wss) - Listen  
**Input Type:** singlejson  
**Request Profile ID:** febfa3e1-f719-4ee8-ba57-cdae34137ab3  
**Request Profile Name:** D365 Leave Create JSON Profile  
**Profile Type:** profile.json

### Request Profile Structure

**Root Element:** Root  
**Array Detection:** ❌ NO - Single object structure  
**Array Cardinality:** N/A (single object)

### Input Format (JSON Structure)

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

### Request Field Mapping

| Boomi Field Path | Boomi Field Name | Data Type | Required | Azure DTO Property | Notes |
|---|---|---|---|---|---|
| Root/Object/employeeNumber | employeeNumber | number | Yes | EmployeeNumber | Used for tracking |
| Root/Object/absenceType | absenceType | character | Yes | AbsenceType | Leave type |
| Root/Object/employer | employer | character | Yes | Employer | Employer name |
| Root/Object/startDate | startDate | character | Yes | StartDate | Leave start date |
| Root/Object/endDate | endDate | character | Yes | EndDate | Leave end date |
| Root/Object/absenceStatusCode | absenceStatusCode | character | Yes | AbsenceStatusCode | Status code |
| Root/Object/approvalStatusCode | approvalStatusCode | character | Yes | ApprovalStatusCode | Approval status |
| Root/Object/startDateDuration | startDateDuration | number | Yes | StartDateDuration | Duration in days |
| Root/Object/endDateDuration | endDateDuration | number | Yes | EndDateDuration | Duration in days |

### Document Processing Behavior

**Behavior:** Single document processing  
**Description:** Boomi processes single JSON document per execution  
**Execution Pattern:** Single execution per request  
**Session Management:** One session per execution  
**Azure Function Requirement:** Accept single LeaveCreateRequest object (not array)

### Validation Checklist

- [x] Request profile identified from entry operation
- [x] Profile structure analyzed (JSON)
- [x] Array vs single object detected (Single object)
- [x] Array cardinality documented (N/A)
- [x] ALL fields extracted (9 fields)
- [x] Field paths documented (full Boomi paths)
- [x] Field mapping table generated (Boomi → Azure DTO)
- [x] Document processing behavior determined (Single document)
- [x] Input structure documented in Phase 1 document

---

## 3. Response Structure Analysis (Step 1b)

### ✅ MANDATORY STEP - CONTRACT VERIFICATION

**Response Profile ID:** f4ca3a70-114a-4601-bad8-44a3eb20e2c0  
**Response Profile Name:** Leave D365 Response  
**Profile Type:** profile.json

### Response Profile Structure

**Root Element:** leaveResponse  
**Array Detection:** ❌ NO - Single object structure

### Response Format (JSON Structure)

```json
{
  "status": "success",
  "message": "Data successfully sent to Oracle Fusion",
  "personAbsenceEntryId": 123456,
  "success": "true"
}
```

### Response Field Mapping

| Boomi Field Path | Boomi Field Name | Data Type | Azure DTO Property | Notes |
|---|---|---|---|---|
| leaveResponse/Object/status | status | character | Status | Success or failure status |
| leaveResponse/Object/message | message | character | Message | Response message |
| leaveResponse/Object/personAbsenceEntryId | personAbsenceEntryId | number | PersonAbsenceEntryId | Oracle Fusion absence entry ID |
| leaveResponse/Object/success | success | character | Success | Boolean flag as string |

### Validation Checklist

- [x] Response profile identified from entry operation
- [x] Response structure analyzed (JSON)
- [x] ALL fields extracted (4 fields)
- [x] Field paths documented
- [x] Response field mapping table generated

---

## 4. Operation Response Analysis (Step 1c)

### ✅ MANDATORY STEP - Required before Step 7

### Operation: Leave Oracle Fusion Create (6e8920fd-af5a-430b-a1d9-9fde7ac29a12)

**Response Profile ID:** None (HTTP operation with no explicit response profile)  
**Response Type:** JSON (application/json)  
**Response Content:** Oracle Fusion HCM REST API response

**Extracted Fields:**
- Response body captured as current document
- HTTP status code captured in track property: `meta.base.applicationstatuscode`
- Response header `Content-Encoding` captured in dynamic property: `dynamicdocument.DDP_RespHeader`

**Data Consumers:**
1. **shape2 (Decision: HTTP Status 20 check)** - Reads `meta.base.applicationstatuscode`
   - **Dependency Chain:** shape33 (HTTP POST) → Writes `meta.base.applicationstatuscode` → shape2 reads it
   - **Business Logic:** Decision checks if HTTP status is 20* (success)

2. **shape44 (Decision: Check Response Content Type)** - Reads `dynamicdocument.DDP_RespHeader`
   - **Dependency Chain:** shape33 (HTTP POST) → Writes `dynamicdocument.DDP_RespHeader` → shape44 reads it
   - **Business Logic:** Decision checks if response is gzip compressed

3. **shape34 (Map: Oracle Fusion Leave Response Map)** - Reads response body
   - **Dependency Chain:** shape33 (HTTP POST) → Produces response → shape34 maps it
   - **Business Logic:** Maps Oracle Fusion response to D365 response format

### Business Logic Implications

**Critical Execution Order:**
1. **shape33 (HTTP POST to Oracle Fusion)** MUST execute FIRST
2. **shape2 (HTTP Status Check)** executes AFTER shape33 (checks response status)
3. **shape44 (Content-Type Check)** executes AFTER shape2 (on FALSE path - non-20* status)
4. **shape34 (Response Mapping)** executes AFTER shape2 (on TRUE path - 20* status)

**Proof of Dependencies:**
- shape2 decision checks `meta.base.applicationstatuscode` which is produced by shape33
- shape44 decision checks `dynamicdocument.DDP_RespHeader` which is produced by shape33
- shape34 map transforms response body which is produced by shape33

### Validation Checklist

- [x] Operation response structure identified
- [x] Extracted fields documented (HTTP status, Content-Encoding header)
- [x] Data consumers identified (shape2, shape44, shape34)
- [x] Dependency chains documented with proof
- [x] Business logic implications documented

---

## 5. Map Analysis (Step 1d)

### ✅ MANDATORY STEP - Required before Phase 2

### Map Inventory

| Map ID | Map Name | From Profile | To Profile | Type |
|---|---|---|---|---|
| c426b4d6-2aff-450e-b43b-59956c4dbc96 | Leave Create Map | febfa3e1 | a94fa205 | Request transformation |
| e4fd3f59-edb5-43a1-aeae-143b600a064e | Oracle Fusion Leave Response Map | 316175c7 | f4ca3a70 | Response transformation |
| f46b845a-7d75-41b5-b0ad-c41a6a8e9b12 | Leave Error Map | 23d7a2e9 | f4ca3a70 | Error transformation |

### Map 1: Leave Create Map (c426b4d6-2aff-450e-b43b-59956c4dbc96)

**From Profile:** febfa3e1-f719-4ee8-ba57-cdae34137ab3 (D365 Leave Create JSON Profile)  
**To Profile:** a94fa205-c740-40a5-9fda-3d018611135a (HCM Leave Create JSON Profile)  
**Type:** JSON to JSON transformation

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

**Profile vs Map Field Name Comparison:**

| Profile Field Name | Map Field Name (ACTUAL) | Authority | Use in Request |
|---|---|---|---|
| employeeNumber | personNumber | ✅ MAP | personNumber |
| absenceStatusCode | absenceStatusCd | ✅ MAP | absenceStatusCd |
| approvalStatusCode | approvalStatusCd | ✅ MAP | approvalStatusCd |

**CRITICAL RULE:** Map field names are AUTHORITATIVE. Use map field names in request JSON, NOT profile field names.

**Scripting Functions:** None

**Static Values:** None

### Map 2: Oracle Fusion Leave Response Map (e4fd3f59-edb5-43a1-aeae-143b600a064e)

**From Profile:** 316175c7-0e45-4869-9ac6-5f9d69882a62 (Oracle Fusion Leave Response JSON Profile)  
**To Profile:** f4ca3a70-114a-4601-bad8-44a3eb20e2c0 (Leave D365 Response)  
**Type:** JSON to JSON transformation

**Field Mappings:**

| Source Field | Source Path | Target Field | Target Path | Transformation |
|---|---|---|---|---|
| personAbsenceEntryId | Root/Object/personAbsenceEntryId | personAbsenceEntryId | leaveResponse/Object/personAbsenceEntryId | Direct mapping |

**Default Values:**

| Target Field | Default Value |
|---|---|
| status | "success" |
| message | "Data successfully sent to Oracle Fusion" |
| success | "true" |

### Map 3: Leave Error Map (f46b845a-7d75-41b5-b0ad-c41a6a8e9b12)

**From Profile:** 23d7a2e9-5cb0-4e9c-9e4b-3154834bad0d (Dummy FF Profile)  
**To Profile:** f4ca3a70-114a-4601-bad8-44a3eb20e2c0 (Leave D365 Response)  
**Type:** Error mapping with process property

**Field Mappings:**

| Source Type | Source | Target Field | Target Path | Transformation |
|---|---|---|---|---|
| function (PropertyGet) | process.DPP_ErrorMessage | message | leaveResponse/Object/message | Get error message from process property |

**Default Values:**

| Target Field | Default Value |
|---|---|
| status | "failure" |
| success | "false" |

**Scripting Functions:**

| Function Key | Type | Input | Output | Logic |
|---|---|---|---|---|
| 1 | PropertyGet | Property Name: "DPP_ErrorMessage" | Result | Retrieves error message from process property |

### Validation Checklist

- [x] All map files identified and loaded (3 maps)
- [x] Field mappings extracted from each map
- [x] Profile vs map field name discrepancies documented
- [x] Map field names marked as AUTHORITATIVE
- [x] Scripting functions analyzed (PropertyGet in error map)
- [x] Static values identified and documented (default values in response maps)
- [x] Process property mappings documented (DPP_ErrorMessage)
- [x] Map Analysis documented in Phase 1 document

---

## 6. HTTP Status Codes and Return Path Responses (Step 1e)

### ✅ MANDATORY STEP - Required before Phase 2

### Return Path Analysis

#### Return Path 1: Success Response

**Return Label:** "Success Response"  
**Return Shape ID:** shape35  
**HTTP Status Code:** 200  
**Decision Conditions Leading to Return:**
- shape2 (HTTP Status 20 check) → TRUE path (HTTP status is 20*)

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
  "personAbsenceEntryId": 123456,
  "success": "true"
}
```

#### Return Path 2: Error Response (Try/Catch Error)

**Return Label:** "Error Response"  
**Return Shape ID:** shape43  
**HTTP Status Code:** 400  
**Decision Conditions Leading to Return:**
- shape17 (Try/Catch) → CATCH path (exception occurred)
- shape20 (Branch) → Path 2

**Error Code:** N/A (error message in DPP_ErrorMessage)

**Populated Response Fields:**

| Field Name | Field Path | Source | Populated By |
|---|---|---|---|
| status | leaveResponse/Object/status | static (map default) | "failure" |
| message | leaveResponse/Object/message | process_property | process.DPP_ErrorMessage |
| success | leaveResponse/Object/success | static (map default) | "false" |

**Response JSON Example:**

```json
{
  "status": "failure",
  "message": "[Error message from catch block]",
  "success": "false"
}
```

#### Return Path 3: Error Response (HTTP Non-20* Status, Non-Gzip)

**Return Label:** "Error Response"  
**Return Shape ID:** shape36  
**HTTP Status Code:** 400  
**Decision Conditions Leading to Return:**
- shape2 (HTTP Status 20 check) → FALSE path (HTTP status is NOT 20*)
- shape44 (Check Response Content Type) → FALSE path (Content-Encoding is NOT gzip)

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
  "message": "[HTTP error message from Oracle Fusion]",
  "success": "false"
}
```

#### Return Path 4: Error Response (HTTP Non-20* Status, Gzip Compressed)

**Return Label:** "Error Response"  
**Return Shape ID:** shape48  
**HTTP Status Code:** 400  
**Decision Conditions Leading to Return:**
- shape2 (HTTP Status 20 check) → FALSE path (HTTP status is NOT 20*)
- shape44 (Check Response Content Type) → TRUE path (Content-Encoding is gzip)
- shape45 (Data Process: Decompress gzip) → Decompresses response
- shape46 (Document Properties: error msg) → Extracts error message

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
  "message": "[Decompressed HTTP error message from Oracle Fusion]",
  "success": "false"
}
```

### Downstream Operations HTTP Status Codes

| Operation Name | Expected Success Codes | Error Codes | Error Handling |
|---|---|---|---|
| Leave Oracle Fusion Create | 200, 201, 202 | 400, 401, 404, 500 | Return error response to caller |

### Validation Checklist

- [x] All return paths documented with HTTP status codes (4 return paths)
- [x] Response JSON examples provided for each return path
- [x] Populated fields documented for each return path
- [x] Decision conditions leading to each return documented
- [x] Error codes and success codes documented
- [x] Downstream operation HTTP status codes documented

---

## 7. Process Properties Analysis (Steps 2-3)

### Process Properties WRITTEN (Step 2)

| Property Name | Written By Shape(s) | Value Source | Purpose |
|---|---|---|---|
| process.DPP_Process_Name | shape38 | Execution property: Process Name | Store process name for logging |
| process.DPP_AtomName | shape38 | Execution property: Atom Name | Store atom name for logging |
| process.DPP_Payload | shape38 | Current document | Store input payload for error notification |
| process.DPP_ExecutionID | shape38 | Execution property: Execution Id | Store execution ID for logging |
| process.DPP_File_Name | shape38 | Concatenation: Process Name + Timestamp + ".txt" | Generate file name for email attachment |
| process.DPP_Subject | shape38 | Concatenation: Atom Name + " (" + Process Name + " ) has errors to report" | Generate email subject |
| process.To_Email | shape38 | Defined property: PP_HCM_LeaveCreate_Properties.To_Email | Email recipient for error notifications |
| process.DPP_HasAttachment | shape38 | Defined property: PP_HCM_LeaveCreate_Properties.DPP_HasAttachment | Flag to indicate if email has attachment |
| dynamicdocument.URL | shape8 | Defined property: PP_HCM_LeaveCreate_Properties.Resource_Path | Oracle Fusion REST API resource path |
| process.DPP_ErrorMessage | shape19 | Track property: meta.base.catcherrorsmessage | Error message from try/catch block |
| process.DPP_ErrorMessage | shape39 | Track property: meta.base.applicationstatusmessage | Error message from HTTP response |
| process.DPP_ErrorMessage | shape46 | Current document (after gzip decompression) | Error message from decompressed HTTP response |

### Process Properties READ (Step 3)

| Property Name | Read By Shape(s) | Usage |
|---|---|---|
| dynamicdocument.URL | shape33 (HTTP operation) | Used as URL path parameter for Oracle Fusion API call |
| process.DPP_ErrorMessage | shape41 (Map: Leave Error Map) | Used as source for error message in response |
| process.To_Email | Subprocess shape21 | Used as email recipient in error notification subprocess |
| process.DPP_Subject | Subprocess shape21 | Used as email subject in error notification subprocess |
| process.DPP_HasAttachment | Subprocess shape21 | Used to determine if email has attachment |
| process.DPP_Process_Name | Subprocess shape21 | Used in email body for error details |
| process.DPP_AtomName | Subprocess shape21 | Used in email body for error details |
| process.DPP_ExecutionID | Subprocess shape21 | Used in email body for error details |
| process.DPP_Payload | Subprocess shape21 | Used as email attachment content |
| process.DPP_File_Name | Subprocess shape21 | Used as email attachment file name |
| process.DPP_MailBody | Subprocess shape21 | Used as email body content |

### Defined Process Properties

#### PP_HCM_LeaveCreate_Properties (e22c04db-7dfa-4b1b-9d88-77365b0fdb18)

| Property Key | Property Label | Type | Default Value |
|---|---|---|---|
| c2d1a1e8-4bcb-435b-b1d2-bb10a209bcc0 | Resource_Path | string | "hcmRestApi/resources/11.13.18.05/absences" |
| 71c90f6e-4f86-49f3-8aef-bae6668c73f9 | To_Email | string | "BoomiIntegrationTeam@al-ghurair.com" |
| a717867b-67f4-4a72-be2d-c99c73309fdb | DPP_HasAttachment | string | "Y" |

#### PP_Office365_Email (0feff13f-8a2c-438a-b3c7-1909e2a7f533)

| Property Key | Property Label | Type | Default Value |
|---|---|---|---|
| 804b6d40-eeee-4ac0-ab4b-94e1aca9f4b7 | From_Email | string | "Boomi.Dev.failures@al-ghurair.com" |
| 600acadb-ee02-4369-af85-ee70af380b6c | To_Email | string | "Rajesh.Muppala@al-ghurair.com;mohan.jonnalagadda@al-ghurair.com" |
| 2fa6ce9e-437a-44cc-b44f-5c7e61052f41 | HasAttachment | string | "Y" |
| 3ca9f307-cecb-4d1e-b9ec-007839509ed7 | EmailBody | string | "" |
| d8fc7496-ec2c-4308-a4ca-e9f49c0c9500 | Environment | string | "DEV Failure :" |

### Validation Checklist

- [x] All properties WRITTEN documented (12 properties)
- [x] All properties READ documented (11 properties)
- [x] Property sources documented (execution properties, defined properties, track properties)
- [x] Property usage documented

---

## 8. Data Dependency Graph (Step 4)

### ✅ MANDATORY STEP - Required before Step 10

### Property Dependencies

#### Dependency Chain 1: URL Configuration

**Dependency:** shape8 → shape33

- **Writer:** shape8 (Document Properties: set URL)
  - **Writes:** `dynamicdocument.URL`
  - **Value:** Defined property `PP_HCM_LeaveCreate_Properties.Resource_Path`
- **Reader:** shape33 (HTTP operation: Leave Oracle Fusion Create)
  - **Reads:** `dynamicdocument.URL`
  - **Usage:** Used as URL path parameter for HTTP POST

**Execution Order:** shape8 MUST execute BEFORE shape33

#### Dependency Chain 2: Error Message (Try/Catch)

**Dependency:** shape19 → shape41 → shape43

- **Writer:** shape19 (Document Properties: ErrorMsg)
  - **Writes:** `process.DPP_ErrorMessage`
  - **Value:** Track property `meta.base.catcherrorsmessage`
- **Reader:** shape41 (Map: Leave Error Map)
  - **Reads:** `process.DPP_ErrorMessage` (via function PropertyGet)
  - **Usage:** Maps error message to response
- **Terminal:** shape43 (Return Documents: Error Response)

**Execution Order:** shape19 MUST execute BEFORE shape41

#### Dependency Chain 3: Error Message (HTTP Error, Non-Gzip)

**Dependency:** shape39 → shape40 → shape36

- **Writer:** shape39 (Document Properties: error msg)
  - **Writes:** `process.DPP_ErrorMessage`
  - **Value:** Track property `meta.base.applicationstatusmessage`
- **Reader:** shape40 (Map: Leave Error Map)
  - **Reads:** `process.DPP_ErrorMessage` (via function PropertyGet)
  - **Usage:** Maps error message to response
- **Terminal:** shape36 (Return Documents: Error Response)

**Execution Order:** shape39 MUST execute BEFORE shape40

#### Dependency Chain 4: Error Message (HTTP Error, Gzip)

**Dependency:** shape46 → shape47 → shape48

- **Writer:** shape46 (Document Properties: error msg)
  - **Writes:** `process.DPP_ErrorMessage`
  - **Value:** Current document (after gzip decompression by shape45)
- **Reader:** shape47 (Map: Leave Error Map)
  - **Reads:** `process.DPP_ErrorMessage` (via function PropertyGet)
  - **Usage:** Maps error message to response
- **Terminal:** shape48 (Return Documents: Error Response)

**Execution Order:** shape46 MUST execute BEFORE shape47

#### Dependency Chain 5: Error Notification Properties

**Dependency:** shape38 → shape21 (subprocess)

- **Writer:** shape38 (Document Properties: Input_details)
  - **Writes:** Multiple properties:
    - `process.DPP_Process_Name`
    - `process.DPP_AtomName`
    - `process.DPP_Payload`
    - `process.DPP_ExecutionID`
    - `process.DPP_File_Name`
    - `process.DPP_Subject`
    - `process.To_Email`
    - `process.DPP_HasAttachment`
- **Reader:** shape21 (ProcessCall: (Sub) Office 365 Email)
  - **Reads:** All properties written by shape38
  - **Usage:** Used in email notification subprocess

**Execution Order:** shape38 MUST execute BEFORE shape21

### Independent Operations

The following operations have NO data dependencies and could theoretically execute in parallel (but are sequential due to control flow):

- shape29 (Map: Leave Create Map) - Transforms input, no property dependencies
- shape34 (Map: Oracle Fusion Leave Response Map) - Transforms response, no property dependencies

### Dependency Summary

**Total Dependencies:** 5 dependency chains  
**Properties Creating Dependencies:** 9 properties  
**Critical Path:** shape38 → shape8 → shape29 → shape33 → shape2 → shape34 → shape35

### Validation Checklist

- [x] Dependency graph built
- [x] All property dependencies documented
- [x] Dependency chains shown with proof
- [x] Independent operations identified
- [x] Critical path identified

---

## 9. Control Flow Graph (Step 5)

### ✅ MANDATORY STEP - Required for flow understanding

### Control Flow Map

| From Shape | To Shape(s) | Identifier | Description |
|---|---|---|---|
| shape1 (Start) | shape38 | default | Entry point to Input_details |
| shape38 (Input_details) | shape17 | default | To Try/Catch block |
| shape17 (Try/Catch) | shape29 | default (Try) | Try path to mapping |
| shape17 (Try/Catch) | shape20 | error (Catch) | Catch path to branch |
| shape29 (Map) | shape8 | default | To set URL |
| shape8 (set URL) | shape49 | default | To notify |
| shape49 (Notify) | shape33 | default | To HTTP operation |
| shape33 (HTTP POST) | shape2 | default | To HTTP status check |
| shape2 (HTTP Status Check) | shape34 | true | Success path (20*) |
| shape2 (HTTP Status Check) | shape44 | false | Error path (non-20*) |
| shape34 (Response Map) | shape35 | default | To success return |
| shape35 (Success Return) | - | - | Terminal (success) |
| shape44 (Content-Type Check) | shape45 | true | Gzip path |
| shape44 (Content-Type Check) | shape39 | false | Non-gzip path |
| shape45 (Decompress) | shape46 | default | To error msg extraction |
| shape46 (Error msg) | shape47 | default | To error map |
| shape47 (Error Map) | shape48 | default | To error return |
| shape48 (Error Return) | - | - | Terminal (error) |
| shape39 (Error msg) | shape40 | default | To error map |
| shape40 (Error Map) | shape36 | default | To error return |
| shape36 (Error Return) | - | - | Terminal (error) |
| shape20 (Branch) | shape19 | 1 | Branch path 1 |
| shape20 (Branch) | shape41 | 2 | Branch path 2 |
| shape19 (ErrorMsg) | shape21 | default | To subprocess |
| shape21 (Subprocess) | - | - | Terminal (subprocess handles email) |
| shape41 (Error Map) | shape43 | default | To error return |
| shape43 (Error Return) | - | - | Terminal (error) |

### Connection Summary

- **Total Shapes:** 20 shapes
- **Total Connections:** 22 connections
- **Shapes with Multiple Outgoing Connections:**
  - shape17 (Try/Catch): 2 paths (default, error)
  - shape2 (Decision): 2 paths (true, false)
  - shape44 (Decision): 2 paths (true, false)
  - shape20 (Branch): 2 paths (1, 2)
- **Terminal Shapes:** 5 return/stop shapes (shape35, shape36, shape43, shape48, shape21)

### Convergence Points

**No convergence points identified.** All paths lead to different terminal shapes (return documents or subprocess).

### Validation Checklist

- [x] All dragpoint connections extracted
- [x] All toShape references documented
- [x] All identifiers (true/false/default/error) extracted
- [x] Control flow graph is complete
- [x] Convergence points identified (none)

---

## 10. Decision Shape Analysis (Step 7)

### ✅ MANDATORY STEP - BLOCKING - Required before Step 10

### Self-Check Answers (MANDATORY)

- ✅ **Decision data sources identified:** YES
- ✅ **Decision types classified:** YES
- ✅ **Execution order verified:** YES
- ✅ **All decision paths traced:** YES
- ✅ **Decision patterns identified:** YES
- ✅ **Paths traced to termination:** YES

### Decision 1: HTTP Status 20 check (shape2)

**Shape ID:** shape2  
**Comparison Type:** wildcard  
**Comparison:** `meta.base.applicationstatuscode` wildcard matches "20*"

**Data Source Analysis:**
- **Value 1:** Track property `meta.base.applicationstatuscode`
- **Value 2:** Static value "20*"
- **Data Source:** TRACK_PROPERTY (HTTP response status code)
- **Decision Type:** POST-OPERATION (checks response from HTTP operation)
- **Actual Execution Order:** shape33 (HTTP POST) → Response → shape2 (Decision) → Route based on status

**TRUE Path:**
- **To Shape:** shape34 (Map: Oracle Fusion Leave Response Map)
- **Termination:** shape35 (Return Documents: Success Response)
- **Path Type:** Success path - maps response and returns success

**FALSE Path:**
- **To Shape:** shape44 (Decision: Check Response Content Type)
- **Termination:** Multiple terminals (shape36, shape48) depending on content type
- **Path Type:** Error path - handles HTTP errors

**Pattern:** Error Check (Success vs Failure)  
**Convergence Point:** None (paths lead to different terminals)  
**Early Exit:** No (both paths return documents)

### Decision 2: Check Response Content Type (shape44)

**Shape ID:** shape44  
**Comparison Type:** equals  
**Comparison:** `dynamicdocument.DDP_RespHeader` equals "gzip"

**Data Source Analysis:**
- **Value 1:** Dynamic document property `dynamicdocument.DDP_RespHeader`
- **Value 2:** Static value "gzip"
- **Data Source:** TRACK_PROPERTY (HTTP response header Content-Encoding)
- **Decision Type:** POST-OPERATION (checks response header from HTTP operation)
- **Actual Execution Order:** shape33 (HTTP POST) → Response → shape2 (FALSE) → shape44 (Decision) → Route based on encoding

**TRUE Path:**
- **To Shape:** shape45 (Data Process: Decompress gzip)
- **Termination:** shape48 (Return Documents: Error Response)
- **Path Type:** Gzip decompression path - decompresses response, extracts error, returns error

**FALSE Path:**
- **To Shape:** shape39 (Document Properties: error msg)
- **Termination:** shape36 (Return Documents: Error Response)
- **Path Type:** Direct error path - extracts error message, returns error

**Pattern:** Conditional Logic (Optional Processing) - decides if decompression is needed  
**Convergence Point:** None (both paths lead to different error return shapes)  
**Early Exit:** No (both paths return documents)

### Decision Summary

| Decision | Data Source | Type | Pattern | Early Exit |
|---|---|---|---|---|
| shape2 (HTTP Status 20 check) | TRACK_PROPERTY | POST-OPERATION | Error Check | No |
| shape44 (Check Response Content Type) | TRACK_PROPERTY | POST-OPERATION | Conditional Logic | No |

### Validation Checklist

- [x] All decision shapes inventoried (2 decisions)
- [x] BOTH TRUE and FALSE paths traced to termination
- [x] Pattern type identified for each decision
- [x] Early exits identified (none)
- [x] Convergence points identified (none)
- [x] Data source analysis completed for all decisions
- [x] Decision types classified (both POST-OPERATION)
- [x] Actual execution order verified

---

## 11. Branch Shape Analysis (Step 8)

### ✅ MANDATORY STEP - BLOCKING - Required before Step 10

### Self-Check Answers (MANDATORY)

- ✅ **Classification completed:** YES
- ✅ **Assumption check:** NO (analyzed dependencies)
- ✅ **Properties extracted:** YES
- ✅ **Dependency graph built:** YES
- ✅ **Topological sort applied:** N/A (no dependencies between paths)

### Branch 1: Error Handling Branch (shape20)

**Shape ID:** shape20  
**Number of Paths:** 2  
**Location:** Inside Try/Catch error handler

#### Path Properties Analysis

**Path 1 (shape20 → shape19):**
- **Reads:** Track property `meta.base.catcherrorsmessage`
- **Writes:** `process.DPP_ErrorMessage`
- **Operations:** Document properties, subprocess call (email notification)

**Path 2 (shape20 → shape41):**
- **Reads:** `process.DPP_ErrorMessage` (via map function)
- **Writes:** None
- **Operations:** Map (error mapping), return documents

#### Dependency Graph

**Dependency Analysis:**
- Path 2 reads `process.DPP_ErrorMessage`
- Path 1 writes `process.DPP_ErrorMessage`
- **Dependency:** Path 2 depends on Path 1

**Dependency Graph:**
```
Path 1 (shape19) → Writes process.DPP_ErrorMessage
                 ↓
Path 2 (shape41) → Reads process.DPP_ErrorMessage
```

#### Classification

**Classification:** SEQUENTIAL  
**Reason:** Path 2 depends on Path 1 (reads property written by Path 1)

**API Call Detection:** No API calls in branch paths

**Proof:**
- Path 1 writes `process.DPP_ErrorMessage` in shape19
- Path 2 reads `process.DPP_ErrorMessage` in shape41 (via map function PropertyGet)
- Therefore, Path 1 MUST execute BEFORE Path 2

#### Topological Sort Order

**Execution Order:** Path 1 → Path 2

**Reasoning:**
1. Path 1 executes first: shape19 writes `process.DPP_ErrorMessage`
2. Path 2 executes second: shape41 reads `process.DPP_ErrorMessage`

#### Path Termination

**Path 1 Termination:** shape21 (ProcessCall: subprocess) - Subprocess handles email notification  
**Path 2 Termination:** shape43 (Return Documents: Error Response)

#### Convergence Points

**Convergence:** None - paths lead to different terminals

#### Execution Continuation

**Execution Continues From:** None - each path terminates independently

### Branch Analysis Summary

| Branch | Paths | Classification | Reason | Execution Order |
|---|---|---|---|---|
| shape20 | 2 | SEQUENTIAL | Path 2 depends on Path 1 (property dependency) | Path 1 → Path 2 |

### Validation Checklist

- [x] Branch shape identified
- [x] Properties extracted for each path
- [x] Dependency graph built
- [x] Classification completed (SEQUENTIAL)
- [x] Proof provided (property dependency)
- [x] Topological sort order documented
- [x] Path termination identified
- [x] Convergence points identified (none)

---

## 12. Execution Order (Step 9)

### ✅ MANDATORY STEP - BLOCKING - Required before Step 10

### Self-Check Answers (MANDATORY)

- ✅ **Business logic verified FIRST:** YES
- ✅ **Operation analysis complete:** YES
- ✅ **Business logic execution order identified:** YES
- ✅ **Data dependencies checked FIRST:** YES
- ✅ **Operation response analysis used:** YES (Step 1c)
- ✅ **Decision analysis used:** YES (Step 7)
- ✅ **Dependency graph used:** YES (Step 4)
- ✅ **Branch analysis used:** YES (Step 8)
- ✅ **Property dependency verification:** YES
- ✅ **Topological sort applied:** YES (for branch shape20)

### Business Logic Flow (Step 0 - MANDATORY FIRST)

#### Operation Analysis

**Operation 1: Create Leave Oracle Fusion OP (8f709c2b-e63f-4d5f-9374-2932ed70415d)**
- **Purpose:** Entry point - Receives leave request from D365
- **Outputs:** Input document (leave request JSON)
- **Dependent Operations:** All downstream operations depend on this input
- **Business Flow:** Receives leave request → Validates → Transforms → Sends to Oracle Fusion

**Operation 2: Leave Oracle Fusion Create (6e8920fd-af5a-430b-a1d9-9fde7ac29a12)**
- **Purpose:** HTTP POST to Oracle Fusion HCM REST API to create absence entry
- **Outputs:** 
  - HTTP response body (Oracle Fusion response JSON)
  - `meta.base.applicationstatuscode` (HTTP status code)
  - `dynamicdocument.DDP_RespHeader` (Content-Encoding header)
- **Dependent Operations:** 
  - shape2 (HTTP Status Check) - reads HTTP status code
  - shape44 (Content-Type Check) - reads Content-Encoding header
  - shape34 (Response Map) - reads response body
- **Business Flow:** Sends leave request to Oracle Fusion → Receives response → Routes based on status

**Operation 3: (Sub) Office 365 Email (a85945c5-3004-42b9-80b1-104f465cd1fb)**
- **Purpose:** Send error notification email
- **Outputs:** Email sent
- **Dependent Operations:** None (terminal operation)
- **Business Flow:** Formats error email → Sends email → Terminates

#### Actual Business Flow

1. **Receive Request** (shape1 → shape38)
   - Entry point receives leave request from D365
   - Extract execution details (process name, atom name, execution ID, payload)

2. **Try Block Execution** (shape17 → shape29 → shape8 → shape49 → shape33)
   - Map input from D365 format to Oracle Fusion format
   - Set Oracle Fusion API URL
   - Log request payload
   - Send HTTP POST to Oracle Fusion HCM API

3. **Response Evaluation** (shape2)
   - Check HTTP status code
   - If 20* (success) → Continue to success path
   - If non-20* (error) → Continue to error path

4. **Success Path** (shape34 → shape35)
   - Map Oracle Fusion response to D365 response format
   - Return success response to D365

5. **Error Path - Non-20* Status** (shape44 → shape39/shape45 → shape40/shape47 → shape36/shape48)
   - Check if response is gzip compressed
   - If gzip → Decompress → Extract error message
   - If not gzip → Extract error message directly
   - Map error to D365 response format
   - Return error response to D365

6. **Catch Block Execution** (shape20 → shape19 → shape21 → shape41 → shape43)
   - Path 1: Extract error message → Send error notification email (subprocess)
   - Path 2: Map error to D365 response format → Return error response

### Execution Order (Based on Business Logic and Dependencies)

#### Main Flow (Try Block)

1. **shape1** (Start) - Entry point
2. **shape38** (Document Properties: Input_details) - Extract execution details
3. **shape17** (Try/Catch) - Begin try block
4. **shape29** (Map: Leave Create Map) - Transform D365 request to Oracle Fusion format
5. **shape8** (Document Properties: set URL) - Set Oracle Fusion API URL
6. **shape49** (Notify) - Log request payload
7. **shape33** (HTTP POST: Leave Oracle Fusion Create) - Send request to Oracle Fusion
8. **shape2** (Decision: HTTP Status 20 check) - Check HTTP status code

#### Success Path (TRUE from shape2)

9. **shape34** (Map: Oracle Fusion Leave Response Map) - Map response to D365 format
10. **shape35** (Return Documents: Success Response) - Return success [HTTP 200]

#### Error Path - Non-Gzip (FALSE from shape2, FALSE from shape44)

9. **shape44** (Decision: Check Response Content Type) - Check if gzip compressed
10. **shape39** (Document Properties: error msg) - Extract error message from HTTP response
11. **shape40** (Map: Leave Error Map) - Map error to D365 response format
12. **shape36** (Return Documents: Error Response) - Return error [HTTP 400]

#### Error Path - Gzip (FALSE from shape2, TRUE from shape44)

9. **shape44** (Decision: Check Response Content Type) - Check if gzip compressed
10. **shape45** (Data Process: Decompress gzip) - Decompress gzip response
11. **shape46** (Document Properties: error msg) - Extract error message from decompressed response
12. **shape47** (Map: Leave Error Map) - Map error to D365 response format
13. **shape48** (Return Documents: Error Response) - Return error [HTTP 400]

#### Catch Block (Error from shape17)

9. **shape20** (Branch) - Branch to two paths
10. **Path 1:**
    - **shape19** (Document Properties: ErrorMsg) - Extract error message from catch block
    - **shape21** (ProcessCall: (Sub) Office 365 Email) - Send error notification email
11. **Path 2:**
    - **shape41** (Map: Leave Error Map) - Map error to D365 response format
    - **shape43** (Return Documents: Error Response) - Return error [HTTP 400]

**Note:** Path 1 and Path 2 execute SEQUENTIALLY (Path 1 → Path 2) because Path 2 depends on `process.DPP_ErrorMessage` written by Path 1.

### Dependency Verification

**Reference to Step 4 (Data Dependency Graph):**

1. **shape8 → shape33:**
   - shape8 writes `dynamicdocument.URL`
   - shape33 reads `dynamicdocument.URL`
   - **Proof:** shape8 MUST execute BEFORE shape33

2. **shape19 → shape41:**
   - shape19 writes `process.DPP_ErrorMessage`
   - shape41 reads `process.DPP_ErrorMessage`
   - **Proof:** shape19 MUST execute BEFORE shape41

3. **shape39 → shape40:**
   - shape39 writes `process.DPP_ErrorMessage`
   - shape40 reads `process.DPP_ErrorMessage`
   - **Proof:** shape39 MUST execute BEFORE shape40

4. **shape46 → shape47:**
   - shape46 writes `process.DPP_ErrorMessage`
   - shape47 reads `process.DPP_ErrorMessage`
   - **Proof:** shape46 MUST execute BEFORE shape47

5. **shape38 → shape21:**
   - shape38 writes multiple properties (DPP_Process_Name, DPP_AtomName, etc.)
   - shape21 reads these properties
   - **Proof:** shape38 MUST execute BEFORE shape21

### Branch Execution Order

**Reference to Step 8 (Branch Analysis):**

**Branch shape20:**
- **Classification:** SEQUENTIAL
- **Reason:** Path 2 depends on Path 1 (property dependency)
- **Execution Order:** Path 1 (shape19 → shape21) → Path 2 (shape41 → shape43)

### Decision Path Tracing

**Reference to Step 7 (Decision Analysis):**

**Decision shape2 (HTTP Status 20 check):**
- **TRUE Path:** shape34 → shape35 (Success response)
- **FALSE Path:** shape44 → ... → shape36/shape48 (Error response)

**Decision shape44 (Check Response Content Type):**
- **TRUE Path:** shape45 → shape46 → shape47 → shape48 (Gzip decompression + error response)
- **FALSE Path:** shape39 → shape40 → shape36 (Direct error response)

### Validation Checklist

- [x] Business logic flow documented
- [x] Operation analysis complete
- [x] Actual business flow documented
- [x] Execution order list complete
- [x] Dependency verification complete (references Step 4)
- [x] Branch execution order documented (references Step 8)
- [x] Decision path tracing complete (references Step 7)
- [x] All self-check answers shown with YES

---

## 13. Sequence Diagram (Step 10)

### ✅ MANDATORY STEP - BLOCKING - Only after Steps 4, 5, 7, 8, 9

**📋 NOTE:** Detailed request/response JSON examples are documented in:
- **Section 6: HTTP Status Codes and Return Path Responses** - For response JSON with populated fields for return paths

### Pre-Creation Validation

- [x] Step 4 (Data Dependency Graph) - COMPLETE and DOCUMENTED
- [x] Step 5 (Control Flow Graph) - COMPLETE and DOCUMENTED
- [x] Step 7 (Decision Analysis) - COMPLETE and DOCUMENTED
- [x] Step 8 (Branch Analysis) - COMPLETE and DOCUMENTED
- [x] Step 9 (Execution Order) - COMPLETE and DOCUMENTED

### Sequence Diagram

**Based on dependency graph in Step 4, decision analysis in Step 7, control flow graph in Step 5, branch analysis in Step 8, and execution order in Step 9.**

```
START (shape1)
 |
 ├─→ Document Properties: Input_details (shape38)
 |   └─→ WRITES: [process.DPP_Process_Name, process.DPP_AtomName, process.DPP_Payload,
 |                 process.DPP_ExecutionID, process.DPP_File_Name, process.DPP_Subject,
 |                 process.To_Email, process.DPP_HasAttachment]
 |
 ├─→ Try/Catch (shape17)
 |   |
 |   ├─→ TRY PATH:
 |   |   |
 |   |   ├─→ Map: Leave Create Map (shape29)
 |   |   |   └─→ Transform D365 request to Oracle Fusion format
 |   |   |   └─→ Field mappings: employeeNumber → personNumber, absenceStatusCode → absenceStatusCd
 |   |   |
 |   |   ├─→ Document Properties: set URL (shape8)
 |   |   |   └─→ WRITES: [dynamicdocument.URL]
 |   |   |   └─→ VALUE: "hcmRestApi/resources/11.13.18.05/absences"
 |   |   |
 |   |   ├─→ Notify (shape49)
 |   |   |   └─→ Log request payload
 |   |   |
 |   |   ├─→ HTTP POST: Leave Oracle Fusion Create (shape33) [Downstream]
 |   |   |   └─→ READS: [dynamicdocument.URL]
 |   |   |   └─→ WRITES: [meta.base.applicationstatuscode, dynamicdocument.DDP_RespHeader, response body]
 |   |   |   └─→ HTTP: [Expected: 200/201/202, Error: 400/401/404/500]
 |   |   |   └─→ URL: https://iaaxey-dev3.fa.ocs.oraclecloud.com:443/hcmRestApi/resources/11.13.18.05/absences
 |   |   |   └─→ Method: POST
 |   |   |   └─→ Auth: Basic Authentication
 |   |   |
 |   |   ├─→ Decision: HTTP Status 20 check (shape2)
 |   |   |   └─→ READS: [meta.base.applicationstatuscode]
 |   |   |   └─→ Condition: HTTP status matches "20*"?
 |   |   |   |
 |   |   |   ├─→ IF TRUE (HTTP 20*):
 |   |   |   |   |
 |   |   |   |   ├─→ Map: Oracle Fusion Leave Response Map (shape34)
 |   |   |   |   |   └─→ Transform Oracle Fusion response to D365 format
 |   |   |   |   |   └─→ Field mappings: personAbsenceEntryId → personAbsenceEntryId
 |   |   |   |   |   └─→ Default values: status="success", message="Data successfully sent to Oracle Fusion", success="true"
 |   |   |   |   |
 |   |   |   |   └─→ Return Documents: Success Response (shape35) [HTTP: 200] [SUCCESS]
 |   |   |   |       └─→ Response: {"status":"success","message":"Data successfully sent to Oracle Fusion","personAbsenceEntryId":123456,"success":"true"}
 |   |   |   |
 |   |   |   └─→ IF FALSE (HTTP non-20*):
 |   |   |       |
 |   |   |       ├─→ Decision: Check Response Content Type (shape44)
 |   |   |       |   └─→ READS: [dynamicdocument.DDP_RespHeader]
 |   |   |       |   └─→ Condition: Content-Encoding equals "gzip"?
 |   |   |       |   |
 |   |   |       |   ├─→ IF TRUE (gzip):
 |   |   |       |   |   |
 |   |   |       |   |   ├─→ Data Process: Decompress gzip (shape45)
 |   |   |       |   |   |   └─→ Decompress gzip response using Groovy script
 |   |   |       |   |   |
 |   |   |       |   |   ├─→ Document Properties: error msg (shape46)
 |   |   |       |   |   |   └─→ WRITES: [process.DPP_ErrorMessage]
 |   |   |       |   |   |   └─→ VALUE: Current document (decompressed error message)
 |   |   |       |   |   |
 |   |   |       |   |   ├─→ Map: Leave Error Map (shape47)
 |   |   |       |   |   |   └─→ READS: [process.DPP_ErrorMessage]
 |   |   |       |   |   |   └─→ Default values: status="failure", success="false"
 |   |   |       |   |   |
 |   |   |       |   |   └─→ Return Documents: Error Response (shape48) [HTTP: 400] [ERROR]
 |   |   |       |   |       └─→ Response: {"status":"failure","message":"[Decompressed error]","success":"false"}
 |   |   |       |   |
 |   |   |       |   └─→ IF FALSE (not gzip):
 |   |   |       |       |
 |   |   |       |       ├─→ Document Properties: error msg (shape39)
 |   |   |       |       |   └─→ WRITES: [process.DPP_ErrorMessage]
 |   |   |       |       |   └─→ VALUE: meta.base.applicationstatusmessage
 |   |   |       |       |
 |   |   |       |       ├─→ Map: Leave Error Map (shape40)
 |   |   |       |       |   └─→ READS: [process.DPP_ErrorMessage]
 |   |   |       |       |   └─→ Default values: status="failure", success="false"
 |   |   |       |       |
 |   |   |       |       └─→ Return Documents: Error Response (shape36) [HTTP: 400] [ERROR]
 |   |   |       |           └─→ Response: {"status":"failure","message":"[HTTP error]","success":"false"}
 |   |   |
 |   └─→ CATCH PATH (Exception occurred):
 |       |
 |       ├─→ Branch (shape20) [SEQUENTIAL - Path 2 depends on Path 1]
 |           |
 |           ├─→ PATH 1 (Email Notification):
 |           |   |
 |           |   ├─→ Document Properties: ErrorMsg (shape19)
 |           |   |   └─→ WRITES: [process.DPP_ErrorMessage]
 |           |   |   └─→ VALUE: meta.base.catcherrorsmessage
 |           |   |
 |           |   └─→ ProcessCall: (Sub) Office 365 Email (shape21) [Subprocess]
 |           |       └─→ READS: [process.DPP_Process_Name, process.DPP_AtomName, process.DPP_Payload,
 |           |                   process.DPP_ExecutionID, process.DPP_File_Name, process.DPP_Subject,
 |           |                   process.To_Email, process.DPP_HasAttachment, process.DPP_ErrorMessage]
 |           |       └─→ Sends error notification email with attachment
 |           |       └─→ SUBPROCESS INTERNAL FLOW: See Section 14
 |           |
 |           └─→ PATH 2 (Error Response) [Executes AFTER Path 1]:
 |               |
 |               ├─→ Map: Leave Error Map (shape41)
 |               |   └─→ READS: [process.DPP_ErrorMessage]
 |               |   └─→ Default values: status="failure", success="false"
 |               |
 |               └─→ Return Documents: Error Response (shape43) [HTTP: 400] [ERROR]
 |                   └─→ Response: {"status":"failure","message":"[Catch error]","success":"false"}
```

### Critical Notes

1. **Sequential Branch Execution:** Branch shape20 paths execute SEQUENTIALLY (Path 1 → Path 2) due to property dependency.
2. **HTTP Status Check:** Decision shape2 routes based on HTTP status code from Oracle Fusion API.
3. **Gzip Decompression:** Decision shape44 handles gzip-compressed error responses.
4. **Error Notification:** Subprocess shape21 sends email notification on catch errors.
5. **Multiple Error Returns:** Process has 4 different error return paths (shape36, shape43, shape48, and subprocess).

---

## 14. Subprocess Analysis (Step 7a)

### Subprocess: (Sub) Office 365 Email (a85945c5-3004-42b9-80b1-104f465cd1fb)

**Purpose:** Send error notification email with or without attachment

### Internal Flow

```
START (shape1)
 |
 ├─→ Try/Catch (shape2)
 |   |
 |   ├─→ TRY PATH:
 |   |   |
 |   |   ├─→ Decision: Attachment_Check (shape4)
 |   |   |   └─→ READS: [process.DPP_HasAttachment]
 |   |   |   └─→ Condition: DPP_HasAttachment equals "Y"?
 |   |   |   |
 |   |   |   ├─→ IF TRUE (with attachment):
 |   |   |   |   |
 |   |   |   |   ├─→ Message: Mail_Body (shape11)
 |   |   |   |   |   └─→ READS: [process.DPP_Process_Name, process.DPP_AtomName,
 |   |   |   |   |                process.DPP_ExecutionID, process.DPP_ErrorMessage]
 |   |   |   |   |   └─→ Generates HTML email body with error details
 |   |   |   |   |
 |   |   |   |   ├─→ Document Properties: set_MailBody (shape14)
 |   |   |   |   |   └─→ WRITES: [process.DPP_MailBody]
 |   |   |   |   |   └─→ VALUE: Current document (HTML email body)
 |   |   |   |   |
 |   |   |   |   ├─→ Message: payload (shape15)
 |   |   |   |   |   └─→ READS: [process.DPP_Payload]
 |   |   |   |   |   └─→ Sets payload as attachment content
 |   |   |   |   |
 |   |   |   |   ├─→ Document Properties: set_Mail_Properties (shape6)
 |   |   |   |   |   └─→ WRITES: [connector.mail.fromAddress, connector.mail.toAddress,
 |   |   |   |   |                connector.mail.subject, connector.mail.body,
 |   |   |   |   |                connector.mail.filename]
 |   |   |   |   |   └─→ Sets email properties from process properties
 |   |   |   |   |
 |   |   |   |   ├─→ Email: Email w Attachment (shape3) [Downstream]
 |   |   |   |   |   └─→ READS: [connector.mail.fromAddress, connector.mail.toAddress,
 |   |   |   |   |                connector.mail.subject, connector.mail.body,
 |   |   |   |   |                connector.mail.filename]
 |   |   |   |   |   └─→ Sends email with attachment via Office 365 SMTP
 |   |   |   |   |
 |   |   |   |   └─→ Stop (shape5) [continue=true] [SUCCESS RETURN]
 |   |   |   |
 |   |   |   └─→ IF FALSE (without attachment):
 |   |   |       |
 |   |   |       ├─→ Message: Mail_Body (shape23)
 |   |   |       |   └─→ READS: [process.DPP_Process_Name, process.DPP_AtomName,
 |   |   |       |                process.DPP_ExecutionID, process.DPP_ErrorMessage]
 |   |   |       |   └─→ Generates HTML email body with error details
 |   |   |       |
 |   |   |       ├─→ Document Properties: set_MailBody (shape22)
 |   |   |       |   └─→ WRITES: [process.DPP_MailBody]
 |   |   |       |   └─→ VALUE: Current document (HTML email body)
 |   |   |       |
 |   |   |       ├─→ Document Properties: set_Mail_Properties (shape20)
 |   |   |       |   └─→ WRITES: [connector.mail.fromAddress, connector.mail.toAddress,
 |   |   |       |                connector.mail.subject, connector.mail.body]
 |   |   |       |   └─→ Sets email properties from process properties
 |   |   |       |
 |   |   |       ├─→ Email: Email W/O Attachment (shape7) [Downstream]
 |   |   |       |   └─→ READS: [connector.mail.fromAddress, connector.mail.toAddress,
 |   |   |       |                connector.mail.subject, connector.mail.body]
 |   |   |       |   └─→ Sends email without attachment via Office 365 SMTP
 |   |   |       |
 |   |   |       └─→ Stop (shape9) [continue=true] [SUCCESS RETURN]
 |   |
 |   └─→ CATCH PATH (Exception occurred):
 |       |
 |       └─→ Exception (shape10)
 |           └─→ READS: [meta.base.catcherrorsmessage]
 |           └─→ Throws exception with error message [ERROR RETURN]
```

### Return Paths

| Return Label | Return Type | Condition | Main Process Mapping |
|---|---|---|---|
| SUCCESS RETURN | Stop (continue=true) | Email sent successfully | N/A (implicit success) |
| ERROR RETURN | Exception | Email send failed | N/A (exception propagates) |

### Properties Written by Subprocess

| Property Name | Written By | Value |
|---|---|---|
| process.DPP_MailBody | shape14, shape22 | HTML email body |
| connector.mail.fromAddress | shape6, shape20 | From email address |
| connector.mail.toAddress | shape6, shape20 | To email address |
| connector.mail.subject | shape6, shape20 | Email subject |
| connector.mail.body | shape6, shape20 | Email body |
| connector.mail.filename | shape6 | Attachment filename |

### Properties Read by Subprocess

| Property Name | Read By | Usage |
|---|---|---|
| process.DPP_HasAttachment | shape4 | Determines if email has attachment |
| process.DPP_Process_Name | shape11, shape23 | Used in email body |
| process.DPP_AtomName | shape11, shape23 | Used in email body |
| process.DPP_ExecutionID | shape11, shape23 | Used in email body |
| process.DPP_ErrorMessage | shape11, shape23 | Used in email body |
| process.DPP_Payload | shape15 | Used as email attachment content |
| process.To_Email | shape6, shape20 | Email recipient |
| process.DPP_Subject | shape6, shape20 | Email subject |
| process.DPP_File_Name | shape6 | Attachment filename |

### Validation Checklist

- [x] Subprocess internal flow traced
- [x] Return paths identified (success and error)
- [x] Properties written by subprocess documented
- [x] Properties read by subprocess documented

---

## 15. System Layer Identification

### Third-Party Systems

| System Name | Type | Purpose | Connection | Operations |
|---|---|---|---|---|
| Oracle Fusion HCM Cloud | SaaS | Human Capital Management system | Oracle Fusion (HTTP) | Create absence entries |
| Office 365 Email | SaaS | Email notification service | Office 365 Email (SMTP) | Send error notifications |

### System Layer APIs Required

#### 1. Oracle Fusion HCM System Layer API

**API Name:** OracleFusionHcmSystemLayer  
**Purpose:** Create absence entries in Oracle Fusion HCM  
**Type:** HTTP Client  
**Base URL:** https://iaaxey-dev3.fa.ocs.oraclecloud.com:443  
**Authentication:** Basic Authentication

**Operations:**

| Operation | HTTP Method | Endpoint | Request | Response |
|---|---|---|---|---|
| CreateAbsence | POST | /hcmRestApi/resources/11.13.18.05/absences | HCM Leave Create JSON | Oracle Fusion Leave Response JSON |

**Request DTO:**
```csharp
public class CreateAbsenceRequest
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
```

**Response DTO:**
```csharp
public class CreateAbsenceResponse
{
    public int PersonAbsenceEntryId { get; set; }
    public string AbsenceStatusCd { get; set; }
    public string ApprovalStatusCd { get; set; }
    // ... additional fields from Oracle Fusion response
}
```

#### 2. Email Notification System Layer API

**API Name:** EmailNotificationSystemLayer  
**Purpose:** Send email notifications via Office 365  
**Type:** SMTP Client  
**SMTP Server:** smtp-mail.outlook.com:587  
**Authentication:** SMTP AUTH with TLS

**Operations:**

| Operation | Protocol | Purpose |
|---|---|---|
| SendEmailWithAttachment | SMTP | Send error notification email with attachment |
| SendEmailWithoutAttachment | SMTP | Send error notification email without attachment |

**Request DTO:**
```csharp
public class SendEmailRequest
{
    public string From { get; set; }
    public string To { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }
    public bool HasAttachment { get; set; }
    public string AttachmentFileName { get; set; }
    public string AttachmentContent { get; set; }
}
```

---

## 16. Critical Patterns Identified

### Pattern 1: Error Handling with Try/Catch

**Pattern Type:** Exception Handling  
**Location:** shape17 (Try/Catch)

**Description:**
- Try block executes main business logic (map, HTTP POST, response handling)
- Catch block handles exceptions and sends error notification email
- Catch block has 2 sequential paths: email notification + error response

**Implementation:**
- Azure Function should use try/catch block
- Catch block should send email notification and return error response
- Error response should include error message from exception

### Pattern 2: HTTP Status Code Routing

**Pattern Type:** Conditional Logic (Error Check)  
**Location:** shape2 (HTTP Status 20 check)

**Description:**
- Decision checks HTTP status code from Oracle Fusion API
- If 20* (success) → Map response and return success
- If non-20* (error) → Extract error message and return error

**Implementation:**
- Azure Function should check HTTP status code after API call
- Route to success or error path based on status code
- Success path: Map response to D365 format
- Error path: Extract error message and return error response

### Pattern 3: Gzip Decompression

**Pattern Type:** Conditional Processing  
**Location:** shape44 (Check Response Content Type), shape45 (Decompress gzip)

**Description:**
- Decision checks if HTTP response is gzip compressed
- If gzip → Decompress response before extracting error message
- If not gzip → Extract error message directly

**Implementation:**
- Azure Function should check Content-Encoding header
- If "gzip" → Decompress response using GZipStream
- Extract error message from decompressed or direct response

### Pattern 4: Sequential Branch Execution

**Pattern Type:** Sequential Processing with Dependencies  
**Location:** shape20 (Branch)

**Description:**
- Branch has 2 paths in catch block
- Path 1: Send error notification email
- Path 2: Return error response
- Path 2 depends on Path 1 (reads property written by Path 1)

**Implementation:**
- Azure Function should execute paths sequentially
- Path 1 writes error message to variable
- Path 2 reads error message from variable
- Both paths execute (not mutually exclusive)

### Pattern 5: Subprocess Call for Email Notification

**Pattern Type:** Subprocess Invocation  
**Location:** shape21 (ProcessCall: (Sub) Office 365 Email)

**Description:**
- Main process calls subprocess to send error notification email
- Subprocess has internal decision logic (with/without attachment)
- Subprocess returns implicitly (Stop with continue=true)

**Implementation:**
- Azure Function should call separate method/function for email notification
- Email function should have logic to send with/without attachment
- Email function should return success/failure status

---

## 17. Validation Checklist

### Data Dependencies
- [x] All property WRITES identified (12 properties)
- [x] All property READS identified (11 properties)
- [x] Dependency graph built (5 dependency chains)
- [x] Execution order satisfies all dependencies (no read-before-write)

### Decision Analysis
- [x] ALL decision shapes inventoried (2 decisions)
- [x] BOTH TRUE and FALSE paths traced to termination
- [x] Pattern type identified for each decision
- [x] Early exits identified (none)
- [x] Convergence points identified (none)
- [x] Data source analysis completed for all decisions
- [x] Decision types classified (both POST-OPERATION)
- [x] Actual execution order verified

### Branch Analysis
- [x] Each branch classified as parallel or sequential (1 branch: SEQUENTIAL)
- [x] API call detection completed (no API calls in branch)
- [x] Classification verified (not assumed)
- [x] If sequential: dependency_order built using topological sort
- [x] Each path traced to terminal point
- [x] Convergence points identified (none)
- [x] Execution continuation point determined (none)

### Sequence Diagram
- [x] Format follows required structure
- [x] Each operation shows READS and WRITES
- [x] Decisions show both TRUE and FALSE paths
- [x] Sequential branch execution documented
- [x] Subprocess internal flows documented
- [x] Subprocess return paths mapped to main process
- [x] HTTP status codes documented for all operations
- [x] Error paths documented with HTTP codes

### Subprocess Analysis
- [x] Subprocess analyzed (internal flow traced)
- [x] Return paths identified (success and error)
- [x] Return path labels mapped to main process shapes
- [x] Properties written by subprocess documented
- [x] Properties read by subprocess from main process documented

### Input/Output Structure Analysis
- [x] Entry point operation identified
- [x] Request profile identified and loaded
- [x] Request profile structure analyzed (JSON)
- [x] Array vs single object detected (single object)
- [x] ALL request fields extracted (9 fields)
- [x] Request field paths documented
- [x] Request field mapping table generated
- [x] Response profile identified and loaded
- [x] Response profile structure analyzed (JSON)
- [x] ALL response fields extracted (4 fields)
- [x] Response field mapping table generated
- [x] Document processing behavior determined (single document)

### Map Analysis
- [x] ALL map files identified and loaded (3 maps)
- [x] Field mappings extracted from each map
- [x] Profile vs map field name discrepancies documented
- [x] Map field names marked as AUTHORITATIVE
- [x] Scripting functions analyzed (PropertyGet in error map)
- [x] Static values identified and documented
- [x] Process property mappings documented

### HTTP Status Codes and Return Path Responses
- [x] All return paths documented with HTTP status codes (4 return paths)
- [x] Response JSON examples provided for each return path
- [x] Populated fields documented for each return path
- [x] Decision conditions leading to each return documented
- [x] Downstream operation HTTP status codes documented

---

## 18. Function Exposure Decision Table

### ✅ MANDATORY - Prevents Function Explosion

**Purpose:** Determine which operations should be exposed as separate Azure Functions vs. internal methods.

| Operation/Process | Expose as Function? | Reasoning | Implementation |
|---|---|---|---|
| HCM_Leave Create (Main Process) | ✅ YES | Entry point from D365, needs HTTP trigger | **Process Layer Function:** `CreateLeaveProcessLayer` |
| Leave Oracle Fusion Create (HTTP POST) | ❌ NO | Internal System Layer call | **System Layer Method:** Called by Process Layer |
| (Sub) Office 365 Email | ❌ NO | Internal subprocess for error notification | **Internal Method:** Called by Process Layer on error |
| Email w Attachment | ❌ NO | Internal SMTP operation | **System Layer Method:** Called by email notification method |
| Email W/O Attachment | ❌ NO | Internal SMTP operation | **System Layer Method:** Called by email notification method |

### Function Architecture

#### Process Layer Function

**Function Name:** `CreateLeaveProcessLayer`  
**Trigger:** HTTP POST  
**Route:** `/api/hcm/leave/create`  
**Purpose:** Orchestrate leave creation process

**Responsibilities:**
1. Receive leave request from D365
2. Validate input
3. Transform D365 request to Oracle Fusion format
4. Call System Layer to create absence in Oracle Fusion
5. Handle HTTP response (success/error)
6. Handle gzip decompression if needed
7. Send error notification email on exceptions
8. Return response to D365

**System Layer Dependencies:**
- `OracleFusionHcmSystemLayer.CreateAbsence()` - Create absence in Oracle Fusion
- `EmailNotificationSystemLayer.SendEmail()` - Send error notification email

#### System Layer Methods

**Method 1:** `OracleFusionHcmSystemLayer.CreateAbsence()`  
**Purpose:** HTTP POST to Oracle Fusion HCM API  
**Returns:** CreateAbsenceResponse or throws exception

**Method 2:** `EmailNotificationSystemLayer.SendEmail()`  
**Purpose:** Send email via Office 365 SMTP  
**Returns:** Success/failure status

### Validation Checklist

- [x] All operations evaluated for function exposure
- [x] Function explosion prevented (only 1 Process Layer function)
- [x] System Layer operations identified as internal methods
- [x] Subprocess identified as internal method
- [x] Function architecture documented

---

## Summary

This Phase 1 extraction document provides a comprehensive analysis of the HCM Leave Create Boomi process. The process receives leave requests from D365, transforms them to Oracle Fusion format, sends them to Oracle Fusion HCM Cloud via REST API, handles success/error responses, and sends error notification emails on failures.

**Key Findings:**
- **1 Process Layer Function:** CreateLeaveProcessLayer (HTTP trigger)
- **2 System Layer APIs:** Oracle Fusion HCM, Email Notification
- **4 Return Paths:** 1 success, 3 error paths
- **2 Decisions:** HTTP status check, gzip compression check
- **1 Sequential Branch:** Error handling with email notification + error response
- **1 Subprocess:** Email notification with/without attachment
- **No Arrays:** Single document processing (not array)
- **No Convergence:** All paths lead to different terminals

**Critical Implementation Notes:**
1. Map field names are AUTHORITATIVE (use `personNumber`, `absenceStatusCd`, `approvalStatusCd`)
2. Branch shape20 paths execute SEQUENTIALLY (Path 1 → Path 2)
3. Gzip decompression required for some error responses
4. Error notification email sent on catch errors (with attachment)
5. HTTP status code routing determines success/error path

**Ready for Phase 2:** Code generation can proceed based on this extraction.

---

**Document Status:** ✅ COMPLETE - All mandatory sections documented with self-checks answered YES

**Next Steps:** Proceed to Phase 2 (Code Generation) to create Azure Functions implementation.
