# BOOMI EXTRACTION PHASE 1 - HCM Leave Create Process

**Process Name:** HCM_Leave Create  
**Process ID:** ca69f858-785f-4565-ba1f-b4edc6cca05b  
**Description:** This Process will sync the Leave data between D365 and Oracle HCM  
**Version:** 29  
**Last Modified:** 2024-11-04T08:54:39Z

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
16. [System Layer Identification](#16-system-layer-identification)
17. [Function Exposure Decision Table](#17-function-exposure-decision-table)
18. [Request/Response JSON Examples](#18-requestresponse-json-examples)
19. [Validation Checklist](#19-validation-checklist)

---

## 1. Operations Inventory

### 1.1 Main Process Operations

| Operation ID | Operation Name | Type | Sub-Type | Description |
|---|---|---|---|---|
| 8f709c2b-e63f-4d5f-9374-2932ed70415d | Create Leave Oracle Fusion OP | connector-action | wss | Web Service Server - Entry point for leave creation |
| 6e8920fd-af5a-430b-a1d9-9fde7ac29a12 | Leave Oracle Fusion Create | connector-action | http | HTTP POST to Oracle Fusion HCM REST API |
| 15a72a21-9b57-49a1-a8ed-d70367146644 | Email W/O Attachment | connector-action | mail | Email sending without attachment |
| af07502a-fafd-4976-a691-45d51a33b549 | Email w Attachment | connector-action | mail | Email sending with attachment |

### 1.2 Connections

| Connection ID | Connection Name | Type | Description |
|---|---|---|---|
| aa1fcb29-d146-4425-9ea6-b9698090f60e | Oracle Fusion | http | Oracle Fusion HCM connection (Basic Auth) |
| 00eae79b-2303-4215-8067-dcc299e42697 | Office 365 Email | mail | Office 365 SMTP email connection |

### 1.3 Maps

| Map ID | Map Name | From Profile | To Profile | Purpose |
|---|---|---|---|---|
| c426b4d6-2aff-450e-b43b-59956c4dbc96 | Leave Create Map | febfa3e1-f719-4ee8-ba57-cdae34137ab3 | a94fa205-c740-40a5-9fda-3d018611135a | Transform D365 request to Oracle format |
| e4fd3f59-edb5-43a1-aeae-143b600a064e | Oracle Fusion Leave Response Map | 316175c7-0e45-4869-9ac6-5f9d69882a62 | f4ca3a70-114a-4601-bad8-44a3eb20e2c0 | Transform Oracle response to D365 success response |
| f46b845a-7d75-41b5-b0ad-c41a6a8e9b12 | Leave Error Map | 23d7a2e9-5cb0-4e9c-9e4b-3154834bad0d | f4ca3a70-114a-4601-bad8-44a3eb20e2c0 | Transform errors to D365 error response |

### 1.4 Profiles

| Profile ID | Profile Name | Type | Purpose |
|---|---|---|---|---|
| febfa3e1-f719-4ee8-ba57-cdae34137ab3 | D365 Leave Create JSON Profile | profile.json | Request from D365 |
| a94fa205-c740-40a5-9fda-3d018611135a | HCM Leave Create JSON Profile | profile.json | Request to Oracle Fusion |
| 316175c7-0e45-4869-9ac6-5f9d69882a62 | Oracle Fusion Leave Response JSON Profile | profile.json | Response from Oracle Fusion |
| f4ca3a70-114a-4601-bad8-44a3eb20e2c0 | Leave D365 Response | profile.json | Response to D365 |
| 23d7a2e9-5cb0-4e9c-9e4b-3154834bad0d | Dummy FF Profile | profile.flatfile | Dummy flat file profile for error mapping |

### 1.5 Subprocess

| Subprocess ID | Subprocess Name | Purpose |
|---|---|---|
| a85945c5-3004-42b9-80b1-104f465cd1fb | (Sub) Office 365 Email | Email notification subprocess |

---

## 2. Process Properties Analysis

### 2.1 Property WRITES (Step 2)

| Property Name | Written By Shape(s) | Source Type | Description |
|---|---|---|---|
| process.DPP_Process_Name | shape38 | execution | Process Name from execution context |
| process.DPP_AtomName | shape38 | execution | Atom Name from execution context |
| process.DPP_Payload | shape38 | current | Current document (input payload) |
| process.DPP_ExecutionID | shape38 | execution | Execution ID from execution context |
| process.DPP_File_Name | shape38 | concatenation | Process Name + Timestamp + ".txt" |
| process.DPP_Subject | shape38 | concatenation | Atom Name + " (" + Process Name + " ) has errors to report" |
| process.To_Email | shape38 | definedparameter | Email recipients from process properties |
| process.DPP_HasAttachment | shape38 | definedparameter | Attachment flag (Y/N) |
| dynamicdocument.URL | shape8 | definedparameter | Resource path for Oracle Fusion API |
| process.DPP_ErrorMessage | shape19, shape39, shape46 | track | Error message from Try/Catch or HTTP response |

### 2.2 Property READS (Step 3)

| Property Name | Read By Shape(s) | Purpose |
|---|---|---|---|
| dynamicdocument.URL | shape33 (HTTP operation) | URL path parameter for Oracle Fusion API |
| dynamicdocument.DDP_RespHeader | shape44 (Decision) | Response header Content-Encoding check |
| process.DPP_ErrorMessage | map_f46b845a (Error map function) | Error message for response mapping |

### 2.3 Defined Process Properties

#### From PP_HCM_LeaveCreate_Properties (e22c04db-7dfa-4b1b-9d88-77365b0fdb18)

| Property Key | Property Name | Default Value | Description |
|---|---|---|---|
| c2d1a1e8-4bcb-435b-b1d2-bb10a209bcc0 | Resource_Path | hcmRestApi/resources/11.13.18.05/absences | Oracle Fusion API resource path |
| 71c90f6e-4f86-49f3-8aef-bae6668c73f9 | To_Email | BoomiIntegrationTeam@al-ghurair.com | Email recipients for error notifications |
| a717867b-67f4-4a72-be2d-c99c73309fdb | DPP_HasAttachment | Y | Email attachment flag |

#### From PP_Office365_Email (0feff13f-8a2c-438a-b3c7-1909e2a7f533)

| Property Key | Property Name | Default Value | Description |
|---|---|---|---|
| 804b6d40-eeee-4ac0-ab4b-94e1aca9f4b7 | From_Email | Boomi.Dev.failures@al-ghurair.com | Email sender address |
| 600acadb-ee02-4369-af85-ee70af380b6c | To_Email | Rajesh.Muppala@al-ghurair.com;mohan.jonnalagadda@al-ghurair.com | Email recipients |
| 2fa6ce9e-437a-44cc-b44f-5c7e61052f41 | HasAttachment | Y | Email attachment flag |
| 3ca9f307-cecb-4d1e-b9ec-007839509ed7 | EmailBody | (empty) | Email body content |
| d8fc7496-ec2c-4308-a4ca-e9f49c0c9500 | Environment | DEV Failure : | Environment prefix for email subject |

---

## 3. Input Structure Analysis (Step 1a)

✅ **Input structure analysis completed: YES**

### 3.1 Entry Point Operation

**Operation ID:** 8f709c2b-e63f-4d5f-9374-2932ed70415d  
**Operation Name:** Create Leave Oracle Fusion OP  
**Operation Type:** connector-action (wss - Web Services Server)  
**Action Type:** Listen  
**Request Profile ID:** febfa3e1-f719-4ee8-ba57-cdae34137ab3  
**Input Type:** singlejson  
**Output Type:** singlejson

### 3.2 Request Profile Structure

**Profile ID:** febfa3e1-f719-4ee8-ba57-cdae34137ab3  
**Profile Name:** D365 Leave Create JSON Profile  
**Profile Type:** profile.json  
**Strict Mode:** false

### 3.3 Input JSON Structure

**Root Structure:** Root/Object  
**Array Detection:** ❌ NO - Single object input  
**Array Cardinality:** N/A

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

### 3.4 Request Field Inventory

| Boomi Field Path | Boomi Field Name | Data Type | Required | Azure DTO Property | Notes |
|---|---|---|---|---|---|
| Root/Object/employeeNumber | employeeNumber | number | Yes | EmployeeNumber | Employee identifier |
| Root/Object/absenceType | absenceType | character | Yes | AbsenceType | Type of leave |
| Root/Object/employer | employer | character | Yes | Employer | Employer name |
| Root/Object/startDate | startDate | character | Yes | StartDate | Leave start date (YYYY-MM-DD) |
| Root/Object/endDate | endDate | character | Yes | EndDate | Leave end date (YYYY-MM-DD) |
| Root/Object/absenceStatusCode | absenceStatusCode | character | Yes | AbsenceStatusCode | Absence status code |
| Root/Object/approvalStatusCode | approvalStatusCode | character | Yes | ApprovalStatusCode | Approval status code |
| Root/Object/startDateDuration | startDateDuration | number | Yes | StartDateDuration | Duration for start date |
| Root/Object/endDateDuration | endDateDuration | number | Yes | EndDateDuration | Duration for end date |

**Total Fields:** 9

### 3.5 Document Processing Behavior

**Behavior:** single_document  
**Description:** Boomi processes single document per execution  
**Execution Pattern:** single_execution  
**Session Management:** one_session_per_execution  
**Azure Function Requirement:** Accept single leave request object

---

## 4. Response Structure Analysis (Step 1b)

✅ **Response structure analysis completed: YES**

### 4.1 Response Profile

**Profile ID:** f4ca3a70-114a-4601-bad8-44a3eb20e2c0  
**Profile Name:** Leave D365 Response  
**Profile Type:** profile.json

### 4.2 Response JSON Structure

**Root Structure:** leaveResponse/Object

```json
{
  "status": "success",
  "message": "Data successfully sent to Oracle Fusion",
  "personAbsenceEntryId": 12345,
  "success": "true"
}
```

### 4.3 Response Field Inventory

| Boomi Field Path | Boomi Field Name | Data Type | Azure DTO Property | Populated By |
|---|---|---|---|---|
| leaveResponse/Object/status | status | character | Status | Map default or error map |
| leaveResponse/Object/message | message | character | Message | Map default or error map |
| leaveResponse/Object/personAbsenceEntryId | personAbsenceEntryId | number | PersonAbsenceEntryId | Oracle Fusion response |
| leaveResponse/Object/success | success | character | Success | Map default or error map |

**Total Fields:** 4

---

## 5. Operation Response Analysis (Step 1c)

✅ **Operation response analysis completed: YES**

### 5.1 Operation: Leave Oracle Fusion Create (6e8920fd-af5a-430b-a1d9-9fde7ac29a12)

**Response Profile ID:** 316175c7-0e45-4869-9ac6-5f9d69882a62  
**Response Profile Name:** Oracle Fusion Leave Response JSON Profile  
**Response Type:** JSON

#### 5.1.1 Response Structure

The Oracle Fusion API returns a comprehensive absence entry object with 80+ fields including:

**Key Response Fields:**
- personAbsenceEntryId (number) - Primary identifier for created absence
- absenceType (character) - Type of absence
- personNumber (number) - Employee number
- startDate (character) - Leave start date
- endDate (character) - Leave end date
- absenceStatusCd (character) - Status code
- approvalStatusCd (character) - Approval status
- duration (number) - Total duration
- And 70+ additional fields (see profile 316175c7-0e45-4869-9ac6-5f9d69882a62)

#### 5.1.2 Extracted Fields

| Field Name | Extracted By | Written To Property | Used By |
|---|---|---|---|
| personAbsenceEntryId | shape34 (Oracle Response Map) | N/A - mapped directly | Success response mapping |
| (full response) | shape34 (Oracle Response Map) | N/A - used in mapping | shape35 (Return Documents) |

#### 5.1.3 Data Consumers

**Consumers:** shape34 (Oracle Fusion Leave Response Map)  
**Purpose:** Transform Oracle response to D365 success response format

**Dependency Chain:**
1. shape33 (HTTP POST to Oracle Fusion) → Executes API call
2. Oracle Fusion returns response with personAbsenceEntryId
3. shape34 (Map) → Transforms Oracle response to D365 format
4. shape35 (Return Documents) → Returns success response to caller

---

## 6. Map Analysis (Step 1d)

✅ **Map analysis completed: YES**

### 6.1 Map Inventory

| Map ID | Map Name | From Profile | To Profile | Type |
|---|---|---|---|---|
| c426b4d6-2aff-450e-b43b-59956c4dbc96 | Leave Create Map | febfa3e1 | a94fa205 | Request transformation |
| e4fd3f59-edb5-43a1-aeae-143b600a064e | Oracle Fusion Leave Response Map | 316175c7 | f4ca3a70 | Success response transformation |
| f46b845a-7d75-41b5-b0ad-c41a6a8e9b12 | Leave Error Map | 23d7a2e9 | f4ca3a70 | Error response transformation |

### 6.2 Map: Leave Create Map (c426b4d6-2aff-450e-b43b-59956c4dbc96)

**From Profile:** febfa3e1-f719-4ee8-ba57-cdae34137ab3 (D365 Leave Create JSON Profile)  
**To Profile:** a94fa205-c740-40a5-9fda-3d018611135a (HCM Leave Create JSON Profile)  
**Type:** HTTP Request transformation (D365 → Oracle Fusion)

#### Field Mappings

| Source Field (D365) | Source Type | Target Field (Oracle) | Profile Match | Notes |
|---|---|---|---|---|
| employeeNumber | profile | personNumber | ✅ Different name | Field renamed |
| absenceType | profile | absenceType | ✅ Match | Direct mapping |
| employer | profile | employer | ✅ Match | Direct mapping |
| startDate | profile | startDate | ✅ Match | Direct mapping |
| endDate | profile | endDate | ✅ Match | Direct mapping |
| absenceStatusCode | profile | absenceStatusCd | ✅ Different name | Field renamed (Code→Cd) |
| approvalStatusCode | profile | approvalStatusCd | ✅ Different name | Field renamed (Code→Cd) |
| startDateDuration | profile | startDateDuration | ✅ Match | Direct mapping |
| endDateDuration | profile | endDateDuration | ✅ Match | Direct mapping |

**Field Name Authority:** ✅ MAP field names are AUTHORITATIVE  
**Scripting Functions:** None  
**Static Values:** None  
**Process Property Mappings:** None

**CRITICAL RULE:** Use map field names (personNumber, absenceStatusCd, approvalStatusCd) in Oracle Fusion API calls, NOT profile field names.

### 6.3 Map: Oracle Fusion Leave Response Map (e4fd3f59-edb5-43a1-aeae-143b600a064e)

**From Profile:** 316175c7-0e45-4869-9ac6-5f9d69882a62 (Oracle Fusion Leave Response JSON Profile)  
**To Profile:** f4ca3a70-114a-4601-bad8-44a3eb20e2c0 (Leave D365 Response)  
**Type:** Success Response transformation (Oracle → D365)

#### Field Mappings

| Source Field (Oracle) | Source Type | Target Field (D365) | Notes |
|---|---|---|---|
| personAbsenceEntryId | profile | personAbsenceEntryId | Absence entry ID |
| (static) | default | status | Default: "success" |
| (static) | default | message | Default: "Data successfully sent to Oracle Fusion" |
| (static) | default | success | Default: "true" |

### 6.4 Map: Leave Error Map (f46b845a-7d75-41b5-b0ad-c41a6a8e9b12)

**From Profile:** 23d7a2e9-5cb0-4e9c-9e4b-3154834bad0d (Dummy FF Profile)  
**To Profile:** f4ca3a70-114a-4601-bad8-44a3eb20e2c0 (Leave D365 Response)  
**Type:** Error Response transformation

#### Field Mappings

| Source Field | Source Type | Target Field (D365) | Notes |
|---|---|---|---|
| DPP_ErrorMessage | function (PropertyGet) | message | Error message from process property |
| (static) | default | status | Default: "failure" |
| (static) | default | success | Default: "false" |

#### Scripting Functions

| Function Key | Type | Input | Output | Logic |
|---|---|---|---|---|
| 1 | PropertyGet | Property Name: "DPP_ErrorMessage" | Result | Get error message from process property |

---

## 7. HTTP Status Codes and Return Path Responses (Step 1e)

✅ **HTTP status codes and return paths documented: YES**

### 7.1 Return Path Inventory

| Return Shape ID | Return Label | HTTP Status Code | Decision Conditions | Response Type |
|---|---|---|---|---|
| shape35 | Success Response | 200 | HTTP Status 20* → TRUE | Success |
| shape36 | Error Response | 400 | HTTP Status 20* → FALSE, Content-Encoding != gzip | Error |
| shape43 | Error Response | 400 | Try/Catch Error | Error |
| shape48 | Error Response | 400 | HTTP Status 20* → FALSE, Content-Encoding = gzip | Error |

### 7.2 Return Path 1: Success Response (shape35)

**Return Label:** Success Response  
**Return Shape ID:** shape35  
**HTTP Status Code:** 200 (Success)  
**Decision Conditions:**
- Decision shape2 (HTTP Status 20* check) → TRUE path
- Oracle Fusion API returns 20x status code

**Populated Response Fields:**

| Field Name | Field Path | Source | Populated By |
|---|---|---|---|
| status | leaveResponse/Object/status | map_default | Oracle Response Map (default: "success") |
| message | leaveResponse/Object/message | map_default | Oracle Response Map (default: "Data successfully sent to Oracle Fusion") |
| personAbsenceEntryId | leaveResponse/Object/personAbsenceEntryId | operation_response | Oracle Fusion API response |
| success | leaveResponse/Object/success | map_default | Oracle Response Map (default: "true") |

**Response JSON Example:**

```json
{
  "status": "success",
  "message": "Data successfully sent to Oracle Fusion",
  "personAbsenceEntryId": 123456,
  "success": "true"
}
```

### 7.3 Return Path 2: HTTP Error Response (shape36)

**Return Label:** Error Response  
**Return Shape ID:** shape36  
**HTTP Status Code:** 400 (Bad Request)  
**Decision Conditions:**
- Decision shape2 (HTTP Status 20* check) → FALSE path
- Decision shape44 (Content-Encoding check) → FALSE path (not gzip)

**Populated Response Fields:**

| Field Name | Field Path | Source | Populated By |
|---|---|---|---|
| status | leaveResponse/Object/status | map_default | Error Map (default: "failure") |
| message | leaveResponse/Object/message | process_property | Error Map (from process.DPP_ErrorMessage) |
| success | leaveResponse/Object/success | map_default | Error Map (default: "false") |

**Response JSON Example:**

```json
{
  "status": "failure",
  "message": "Oracle Fusion API returned error: Invalid absence type",
  "success": "false"
}
```

### 7.4 Return Path 3: Try/Catch Error Response (shape43)

**Return Label:** Error Response  
**Return Shape ID:** shape43  
**HTTP Status Code:** 400 (Bad Request)  
**Decision Conditions:**
- Try/Catch block catches exception in shape17

**Populated Response Fields:**

| Field Name | Field Path | Source | Populated By |
|---|---|---|---|
| status | leaveResponse/Object/status | map_default | Error Map (default: "failure") |
| message | leaveResponse/Object/message | process_property | Error Map (from process.DPP_ErrorMessage via Try/Catch message) |
| success | leaveResponse/Object/success | map_default | Error Map (default: "false") |

**Response JSON Example:**

```json
{
  "status": "failure",
  "message": "Mapping error: Required field missing",
  "success": "false"
}
```

### 7.5 Return Path 4: Gzip Error Response (shape48)

**Return Label:** Error Response  
**Return Shape ID:** shape48  
**HTTP Status Code:** 400 (Bad Request)  
**Decision Conditions:**
- Decision shape2 (HTTP Status 20* check) → FALSE path
- Decision shape44 (Content-Encoding check) → TRUE path (is gzip)
- After decompression using Groovy script

**Populated Response Fields:**

| Field Name | Field Path | Source | Populated By |
|---|---|---|---|
| status | leaveResponse/Object/status | map_default | Error Map (default: "failure") |
| message | leaveResponse/Object/message | process_property | Error Map (from process.DPP_ErrorMessage after decompression) |
| success | leaveResponse/Object/success | map_default | Error Map (default: "false") |

**Response JSON Example:**

```json
{
  "status": "failure",
  "message": "Decompressed error: Oracle validation failed",
  "success": "false"
}
```

### 7.6 Downstream Operations HTTP Status Codes

| Operation Name | Expected Success Codes | Error Codes | Error Handling Strategy |
|---|---|---|---|
| Leave Oracle Fusion Create | 200, 201 | 400, 401, 404, 500 | Check status code with wildcard "20*", return error response if not matching |

---

## 8. Data Dependency Graph (Step 4)

✅ **Data dependency graph completed: YES**

### 8.1 Property Dependencies

#### Dependency Chain 1: URL Property

**Property:** dynamicdocument.URL

**Writers:**
- shape8 (set URL) - Writes from defined parameter Resource_Path

**Readers:**
- shape33 (Leave Oracle Fusion Create HTTP operation) - Uses URL as path parameter

**Dependency:** shape8 MUST execute BEFORE shape33

#### Dependency Chain 2: Error Message Property

**Property:** process.DPP_ErrorMessage

**Writers:**
- shape19 (ErrorMsg) - Writes from track property meta.base.catcherrorsmessage
- shape39 (error msg) - Writes from track property meta.base.applicationstatusmessage
- shape46 (error msg) - Writes from track property meta.base.applicationstatusmessage

**Readers:**
- map_f46b845a (Leave Error Map) - Function 1 reads DPP_ErrorMessage

**Dependencies:**
- shape19 → map_f46b845a (shape41 or shape40)
- shape39 → map_f46b845a (shape40)
- shape46 → map_f46b845a (shape47)

#### Dependency Chain 3: Email Properties

**Properties:** process.DPP_Process_Name, process.DPP_AtomName, process.DPP_Payload, process.DPP_ExecutionID, process.DPP_File_Name, process.DPP_Subject, process.To_Email, process.DPP_HasAttachment

**Writers:**
- shape38 (Input_details) - Writes all email-related properties

**Readers:**
- shape21 (Email subprocess) - Reads email properties

**Dependency:** shape38 MUST execute BEFORE shape21 (email subprocess)

#### Dependency Chain 4: Response Header Property

**Property:** dynamicdocument.DDP_RespHeader

**Writers:**
- shape33 (HTTP operation) - Response header mapping captures Content-Encoding

**Readers:**
- shape44 (Decision: Check Response Content Type) - Checks if Content-Encoding equals "gzip"

**Dependency:** shape33 MUST execute BEFORE shape44

### 8.2 Complete Dependency Summary

| Shape (Writer) | Property | Shape (Reader) | Reasoning |
|---|---|---|---|
| shape8 | dynamicdocument.URL | shape33 | HTTP operation needs URL path parameter |
| shape19 | process.DPP_ErrorMessage | shape41 (map) | Error map needs error message |
| shape39 | process.DPP_ErrorMessage | shape40 (map) | Error map needs error message |
| shape46 | process.DPP_ErrorMessage | shape47 (map) | Error map needs error message |
| shape38 | process.DPP_* (email props) | shape21 (subprocess) | Email subprocess needs email properties |
| shape33 | dynamicdocument.DDP_RespHeader | shape44 | Decision needs to check response header |

---

## 9. Control Flow Graph (Step 5)

✅ **Control flow graph completed: YES**

### 9.1 Shape-to-Shape Connections

| From Shape | Shape Type | To Shape(s) | Identifier | Description |
|---|---|---|---|---|
| shape1 | start | shape38 | default | Start to Input_details |
| shape38 | documentproperties | shape17 | default | Input_details to Try/Catch |
| shape17 | catcherrors | shape29 | default | Try path |
| shape17 | catcherrors | shape20 | error | Catch path |
| shape29 | map | shape8 | default | Map to set URL |
| shape8 | documentproperties | shape49 | default | set URL to Notify |
| shape49 | notify | shape33 | default | Notify to HTTP operation |
| shape33 | connectoraction | shape2 | default | HTTP operation to Decision |
| shape2 | decision | shape34 | true | HTTP Status 20* → TRUE |
| shape2 | decision | shape44 | false | HTTP Status 20* → FALSE |
| shape34 | map | shape35 | default | Success map to Success Response |
| shape35 | returndocuments | - | - | Success return (terminal) |
| shape44 | decision | shape45 | true | Content-Encoding = gzip → TRUE |
| shape44 | decision | shape39 | false | Content-Encoding = gzip → FALSE |
| shape45 | dataprocess | shape46 | default | Decompress to error msg |
| shape46 | documentproperties | shape47 | default | error msg to Error map |
| shape47 | map | shape48 | default | Error map to Error Response |
| shape48 | returndocuments | - | - | Error return (terminal) |
| shape39 | documentproperties | shape40 | default | error msg to Error map |
| shape40 | map | shape36 | default | Error map to Error Response |
| shape36 | returndocuments | - | - | Error return (terminal) |
| shape20 | branch | shape19 | 1 | Branch path 1 |
| shape20 | branch | shape41 | 2 | Branch path 2 |
| shape19 | documentproperties | shape21 | default | ErrorMsg to Email subprocess |
| shape21 | processcall | - | - | Email subprocess (terminal) |
| shape41 | map | shape43 | default | Error map to Error Response |
| shape43 | returndocuments | - | - | Error return (terminal) |

### 9.2 Connection Summary

**Total Shapes:** 19 (main process)  
**Total Connections:** 24  
**Shapes with Multiple Outgoing Connections:**
- shape17 (catcherrors) - 2 connections (Try, Catch)
- shape2 (decision) - 2 connections (True, False)
- shape44 (decision) - 2 connections (True, False)
- shape20 (branch) - 2 connections (Path 1, Path 2)

### 9.3 Reverse Flow Mapping (Step 6)

✅ **Reverse flow mapping completed: YES**

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

**Convergence Points:** None - All paths lead to distinct terminal points

---

## 10. Decision Shape Analysis (Step 7)

✅ **Decision shape analysis completed: YES**  
✅ **Decision data sources identified: YES**  
✅ **Decision types classified: YES**  
✅ **Execution order verified: YES**  
✅ **All decision paths traced: YES**  
✅ **Decision patterns identified: YES**  
✅ **Paths traced to termination: YES**

### 10.1 Decision Inventory

#### Decision 1: HTTP Status 20 check (shape2)

**Shape ID:** shape2  
**Comparison Type:** wildcard  
**Value 1:** meta.base.applicationstatuscode (track property)  
**Value 2:** "20*" (static)

**Data Source:** TRACK_PROPERTY (from HTTP operation response)  
**Decision Type:** POST_OPERATION (checks response data from Oracle Fusion API)  
**Actual Execution Order:** shape33 (HTTP POST to Oracle) → Response → shape2 (Decision) → Route based on status code

**TRUE Path:**
- **Destination:** shape34 (Oracle Fusion Leave Response Map)
- **Termination:** shape35 (Return Documents - Success Response)
- **Pattern:** Success path - Continue processing with successful response

**FALSE Path:**
- **Destination:** shape44 (Check Response Content Type Decision)
- **Termination:** Multiple (shape36 or shape48 - Error Response)
- **Pattern:** Error path - Further check for gzip compression, then return error

**Pattern:** Error Check (Success vs Failure)  
**Convergence Point:** None  
**Early Exit:** FALSE path leads to error return

**Business Logic:**
- If Oracle Fusion API returns 20x status (200, 201, etc.) → Success path
- If Oracle Fusion API returns non-20x status (400, 401, 404, 500) → Error path
- Success path maps Oracle response to D365 success format
- Error path checks if response is gzipped, decompresses if needed, then returns error

#### Decision 2: Check Response Content Type (shape44)

**Shape ID:** shape44  
**Comparison Type:** equals  
**Value 1:** dynamicdocument.DDP_RespHeader (track property)  
**Value 2:** "gzip" (static)

**Data Source:** TRACK_PROPERTY (from HTTP operation response header)  
**Decision Type:** POST_OPERATION (checks response header from Oracle Fusion API)  
**Actual Execution Order:** shape33 (HTTP POST to Oracle) → Response Header Captured → shape2 (FALSE) → shape44 (Decision) → Route based on Content-Encoding

**TRUE Path:**
- **Destination:** shape45 (Groovy decompression script)
- **Termination:** shape48 (Return Documents - Error Response)
- **Pattern:** Decompress gzipped error, extract message, return error

**FALSE Path:**
- **Destination:** shape39 (Extract error message)
- **Termination:** shape36 (Return Documents - Error Response)
- **Pattern:** Extract plain error message, return error

**Pattern:** Conditional Logic (Decompression if gzipped)  
**Convergence Point:** None  
**Early Exit:** Both paths lead to error return (different routes)

**Business Logic:**
- If Oracle Fusion API returns error response with Content-Encoding: gzip → Decompress first
- If Oracle Fusion API returns error response without gzip → Use error message directly
- Both paths eventually return error response to caller

### 10.2 Decision Pattern Summary

| Pattern Type | Decision(s) | Description |
|---|---|---|
| Error Check (Success vs Failure) | shape2 | Check HTTP status code to determine success or failure path |
| Conditional Logic (Optional Processing) | shape44 | Decompress gzipped error response if needed |

### 10.3 Decision Data Source Summary

| Decision | Data Source Type | Source Details | Input vs Response |
|---|---|---|---|
| shape2 | TRACK_PROPERTY | meta.base.applicationstatuscode | RESPONSE (from Oracle Fusion API HTTP status) |
| shape44 | TRACK_PROPERTY | dynamicdocument.DDP_RespHeader (Content-Encoding) | RESPONSE (from Oracle Fusion API response header) |

**CRITICAL:** Both decisions check RESPONSE data from Oracle Fusion API, not INPUT data. Therefore, both are POST-OPERATION decisions that execute AFTER the HTTP operation (shape33).

---

## 11. Branch Shape Analysis (Step 8)

✅ **Branch classification completed: YES**  
✅ **Assumption check: NO (analyzed dependencies)**  
✅ **Properties extracted: YES**  
✅ **Dependency graph built: YES**  
✅ **Topological sort applied: N/A (parallel paths)**

### 11.1 Branch: Error Handling Branch (shape20)

**Shape ID:** shape20  
**Number of Paths:** 2  
**Location:** Inside Catch block of Try/Catch (shape17)

#### 11.1.1 Path Analysis

**Path 1:**
- **Start Shape:** shape19 (ErrorMsg - documentproperties)
- **Terminal:** shape21 (Email subprocess call)
- **Shapes:** shape19 → shape21

**Path 2:**
- **Start Shape:** shape41 (Error map)
- **Terminal:** shape43 (Return Documents - Error Response)
- **Shapes:** shape41 → shape43

#### 11.1.2 Properties Analysis

**Path 1 Properties:**

**READS:**
- meta.base.catcherrorsmessage (track property - Try/Catch error message)
- process.DPP_Process_Name (read by email subprocess)
- process.DPP_AtomName (read by email subprocess)
- process.DPP_Payload (read by email subprocess)
- process.DPP_ExecutionID (read by email subprocess)
- process.DPP_File_Name (read by email subprocess)
- process.DPP_Subject (read by email subprocess)
- process.To_Email (read by email subprocess)
- process.DPP_HasAttachment (read by email subprocess)

**WRITES:**
- process.DPP_ErrorMessage (from meta.base.catcherrorsmessage)

**Path 2 Properties:**

**READS:**
- process.DPP_ErrorMessage (from map function - PropertyGet)

**WRITES:**
- None

#### 11.1.3 Dependency Graph

```
Path 1 (shape19) → WRITES process.DPP_ErrorMessage
Path 2 (shape41) → READS process.DPP_ErrorMessage

Dependency: Path 1 MUST execute BEFORE Path 2
```

#### 11.1.4 Classification

**Classification:** SEQUENTIAL

**Reasoning:**
1. Path 2 reads process.DPP_ErrorMessage which Path 1 writes
2. Data dependency exists: Path 2 depends on Path 1
3. Therefore, paths MUST execute sequentially, not in parallel

#### 11.1.5 Topological Sort Order

```
Dependency Order: Path 1 → Path 2

Execution Order:
1. Path 1: shape19 (Extract error message) → shape21 (Send email notification)
2. Path 2: shape41 (Map error to response) → shape43 (Return error response)
```

#### 11.1.6 Path Termination

**Path 1 Termination:**
- **Type:** processcall (subprocess execution)
- **Terminal Shape:** shape21 (Email subprocess)
- **Behavior:** Subprocess executes, then path ends (no explicit return)

**Path 2 Termination:**
- **Type:** return (Return Documents)
- **Terminal Shape:** shape43 (Error Response)
- **Behavior:** Returns error response to caller

#### 11.1.7 Convergence Points

**Convergence:** None  
**Execution Continues From:** None - Path 1 executes subprocess, Path 2 returns response

#### 11.1.8 API Call Detection

**Path 1 API Calls:**
- Email sending (inside subprocess shape21)

**Path 2 API Calls:**
- None

**🚨 CRITICAL:** Path 1 contains email API call (mail connector inside subprocess), so branch is SEQUENTIAL regardless of data dependencies.

---

## 12. Execution Order (Step 9)

✅ **Business logic verified FIRST: YES**  
✅ **Operation analysis complete: YES**  
✅ **Business logic execution order identified: YES**  
✅ **Data dependencies checked FIRST: YES**  
✅ **Operation response analysis used: YES** (Section 5)  
✅ **Decision analysis used: YES** (Section 10)  
✅ **Dependency graph used: YES** (Section 8)  
✅ **Branch analysis used: YES** (Section 11)  
✅ **Property dependency verification: YES**  
✅ **Topological sort applied: YES**

### 12.1 Business Logic Flow (Step 0 - MANDATORY FIRST)

#### Operation Analysis

**OperationA: Create Leave Oracle Fusion OP (shape1 - Entry Point)**
- **Purpose:** Receive leave creation request from D365 via Web Service
- **Outputs:** Input JSON payload
- **Dependent Operations:** All subsequent operations depend on input data
- **Business Flow:** Entry point that triggers entire leave creation process

**OperationB: Leave Oracle Fusion Create (shape33 - HTTP POST)**
- **Purpose:** Submit leave request to Oracle Fusion HCM REST API
- **Outputs:** 
  - HTTP response with personAbsenceEntryId (if successful)
  - HTTP status code (meta.base.applicationstatuscode)
  - Response headers including Content-Encoding (dynamicdocument.DDP_RespHeader)
- **Dependent Operations:** 
  - shape2 (Decision) - Checks HTTP status from this operation
  - shape44 (Decision) - Checks response header from this operation
  - shape34 (Map) - Transforms response from this operation
- **Business Flow:** Core operation that creates leave entry in Oracle Fusion; all downstream processing depends on its response

**OperationC: Email w Attachment (subprocess shape21 - Email Notification)**
- **Purpose:** Send email notification with error details (only triggered on errors)
- **Outputs:** Email sent to configured recipients
- **Dependent Operations:** None
- **Business Flow:** Notification operation that runs in parallel with error response return

#### Business Logic Execution Order

**Primary Flow: Leave Creation**
1. **Receive Request** (shape1) → Entry point receives leave request from D365
2. **Extract Metadata** (shape38) → Capture execution context (process name, execution ID, payload)
3. **Transform Request** (shape29) → Map D365 format to Oracle Fusion format
4. **Set API URL** (shape8) → Set Oracle Fusion API endpoint path
5. **Log Payload** (shape49) → Log transformed request for debugging
6. **Create Leave in Oracle** (shape33) → POST to Oracle Fusion HCM API
7. **Check HTTP Status** (shape2) → Determine if API call was successful
   - **If 20x (Success):**
     - **Transform Success Response** (shape34) → Map Oracle response to D365 format
     - **Return Success** (shape35) → Return success response to D365
   - **If Non-20x (Error):**
     - **Check Compression** (shape44) → Check if error response is gzipped
       - **If Gzipped:**
         - **Decompress** (shape45) → Decompress gzip response
         - **Extract Error** (shape46) → Extract error message
         - **Transform Error** (shape47) → Map error to D365 format
         - **Return Error** (shape48) → Return error response to D365
       - **If Not Gzipped:**
         - **Extract Error** (shape39) → Extract error message directly
         - **Transform Error** (shape40) → Map error to D365 format
         - **Return Error** (shape36) → Return error response to D365

**Error Flow: Exception Handling**
1. **Try/Catch Wrapper** (shape17) → Wraps transformation and API call
2. **If Exception Occurs:**
   - **Branch Execution** (shape20) → Two parallel paths
     - **Path 1 (Email Notification):**
       - **Extract Error** (shape19) → Get Try/Catch error message
       - **Send Email** (shape21) → Notify team via email subprocess
     - **Path 2 (Error Response):**
       - **Transform Error** (shape41) → Map error to D365 format
       - **Return Error** (shape43) → Return error response to D365

**CRITICAL DEPENDENCIES:**
1. **shape33 MUST execute BEFORE shape2** → Decision needs HTTP status from API response
2. **shape33 MUST execute BEFORE shape44** → Decision needs response header from API response
3. **shape8 MUST execute BEFORE shape33** → API call needs URL parameter
4. **shape29 MUST execute BEFORE shape33** → API call needs transformed request payload
5. **shape38 MUST execute BEFORE shape21** → Email subprocess needs execution metadata
6. **shape19 MUST execute BEFORE shape41** → Error map needs error message property

### 12.2 Complete Execution Order

**Based on dependency graph (Section 8), decision analysis (Section 10), and control flow graph (Section 9):**

```
START (shape1)
  ↓
Extract Input Details (shape38)
  ├─ WRITES: process.DPP_Process_Name, process.DPP_AtomName, process.DPP_Payload,
  │          process.DPP_ExecutionID, process.DPP_File_Name, process.DPP_Subject,
  │          process.To_Email, process.DPP_HasAttachment
  ↓
Try/Catch Wrapper (shape17)
  ├─ TRY PATH:
  │   ↓
  │  Transform Request (shape29 - Map: Leave Create Map)
  │   │  ├─ INPUT: D365 Leave request
  │   │  └─ OUTPUT: Oracle Fusion format request
  │   ↓
  │  Set URL (shape8)
  │   │  └─ WRITES: dynamicdocument.URL (from defined parameter Resource_Path)
  │   ↓
  │  Log Request (shape49 - Notify)
  │   │  └─ INFO level notification with current document
  │   ↓
  │  Create Leave in Oracle (shape33 - HTTP POST Operation)
  │   │  ├─ READS: dynamicdocument.URL
  │   │  ├─ HTTP Method: POST
  │   │  ├─ URL: Oracle Fusion HCM API + Resource_Path
  │   │  ├─ Request: Transformed Oracle Fusion format
  │   │  └─ WRITES: meta.base.applicationstatuscode, dynamicdocument.DDP_RespHeader
  │   ↓
  │  Decision: HTTP Status 20* check (shape2)
  │   │  └─ READS: meta.base.applicationstatuscode
  │   ├─ IF TRUE (20x status - Success):
  │   │   ↓
  │   │  Transform Success Response (shape34 - Map: Oracle Response Map)
  │   │   │  ├─ INPUT: Oracle Fusion response (personAbsenceEntryId)
  │   │   │  └─ OUTPUT: D365 success format
  │   │   ↓
  │   │  Return Success Response (shape35)
  │   │   └─ HTTP 200, Response: {status: "success", message: "...", personAbsenceEntryId: ..., success: "true"}
  │   │
  │   └─ IF FALSE (Non-20x status - Error):
  │       ↓
  │      Decision: Check Response Content Type (shape44)
  │       │  └─ READS: dynamicdocument.DDP_RespHeader
  │       ├─ IF TRUE (Content-Encoding = gzip):
  │       │   ↓
  │       │  Decompress Gzip (shape45 - Groovy Script)
  │       │   │  └─ Script: GZIPInputStream decompression
  │       │   ↓
  │       │  Extract Error Message (shape46)
  │       │   │  └─ WRITES: process.DPP_ErrorMessage (from meta.base.applicationstatusmessage)
  │       │   ↓
  │       │  Transform Error (shape47 - Map: Error Map)
  │       │   │  ├─ READS: process.DPP_ErrorMessage
  │       │   │  └─ OUTPUT: D365 error format
  │       │   ↓
  │       │  Return Error Response (shape48)
  │       │   └─ HTTP 400, Response: {status: "failure", message: "...", success: "false"}
  │       │
  │       └─ IF FALSE (Content-Encoding != gzip):
  │           ↓
  │          Extract Error Message (shape39)
  │           │  └─ WRITES: process.DPP_ErrorMessage (from meta.base.applicationstatusmessage)
  │           ↓
  │          Transform Error (shape40 - Map: Error Map)
  │           │  ├─ READS: process.DPP_ErrorMessage
  │           │  └─ OUTPUT: D365 error format
  │           ↓
  │          Return Error Response (shape36)
  │           └─ HTTP 400, Response: {status: "failure", message: "...", success: "false"}
  │
  └─ CATCH PATH (if exception in Try block):
      ↓
     Branch: Error Notification and Response (shape20)
      │  └─ SEQUENTIAL execution (data dependency + API calls)
      │
      ├─ Path 1: Email Notification
      │   ↓
      │  Extract Error Message (shape19)
      │   │  └─ WRITES: process.DPP_ErrorMessage (from meta.base.catcherrorsmessage)
      │   ↓
      │  Send Email Notification (shape21 - Subprocess Call)
      │   │  ├─ READS: process.DPP_Process_Name, process.DPP_AtomName, process.DPP_ExecutionID,
      │   │  │         process.DPP_ErrorMessage, process.DPP_Payload, process.To_Email,
      │   │  │         process.DPP_File_Name, process.DPP_Subject, process.DPP_HasAttachment
      │   │  └─ Subprocess: (Sub) Office 365 Email (sends email with error details)
      │
      └─ Path 2: Error Response (AFTER Path 1)
          ↓
         Transform Error (shape41 - Map: Error Map)
          │  ├─ READS: process.DPP_ErrorMessage (written by Path 1)
          │  └─ OUTPUT: D365 error format
          ↓
         Return Error Response (shape43)
          └─ HTTP 400, Response: {status: "failure", message: "...", success: "false"}
```

### 12.3 Execution Order Verification

**Property Dependency Verification:**

✅ **All property reads happen AFTER property writes:**
1. shape8 writes dynamicdocument.URL → shape33 reads it ✓
2. shape19 writes process.DPP_ErrorMessage → shape41 reads it ✓
3. shape39 writes process.DPP_ErrorMessage → shape40 reads it ✓
4. shape46 writes process.DPP_ErrorMessage → shape47 reads it ✓
5. shape38 writes email properties → shape21 reads them ✓
6. shape33 writes meta.base.applicationstatuscode → shape2 reads it ✓
7. shape33 writes dynamicdocument.DDP_RespHeader → shape44 reads it ✓

**Branch Execution Order Verification:**

✅ **Branch shape20 follows topological sort:**
1. Path 1 (shape19 → shape21) executes FIRST
2. Path 2 (shape41 → shape43) executes SECOND
3. Reason: Path 2 depends on process.DPP_ErrorMessage written by Path 1
4. Additional: Path 1 contains API call (email), so SEQUENTIAL execution required

**Decision Path Verification:**

✅ **Decision shape2 executes AFTER shape33:**
- shape33 produces HTTP status code
- shape2 checks HTTP status code
- Execution order: shape33 → shape2 ✓

✅ **Decision shape44 executes AFTER shape33:**
- shape33 produces response header
- shape44 checks Content-Encoding header
- Execution order: shape33 → shape2 → shape44 ✓

---

## 13. Sequence Diagram (Step 10)

✅ **Sequence diagram created: YES**  
✅ **References Step 4 (dependency graph): YES**  
✅ **References Step 5 (control flow graph): YES**  
✅ **References Step 7 (decision analysis): YES**  
✅ **References Step 8 (branch analysis): YES**  
✅ **References Step 9 (execution order): YES**

**Based on:**
- **Dependency graph in Step 4 (Section 8)** - Ensures all property reads happen after writes
- **Control flow graph in Step 5 (Section 9)** - Follows actual dragpoint connections
- **Decision analysis in Step 7 (Section 10)** - Includes both TRUE/FALSE paths with data source verification
- **Branch analysis in Step 8 (Section 11)** - Shows sequential execution with dependency reasoning
- **Execution order in Step 9 (Section 12)** - Follows business logic and data dependencies

**📋 NOTE:** Detailed request/response JSON examples are documented in Section 18 (Request/Response JSON Examples).

```
START (shape1 - Web Service Server Entry Point)
 │
 ├─→ Extract Input Details (shape38)
 │    └─→ WRITES: [process.DPP_Process_Name, process.DPP_AtomName, process.DPP_Payload,
 │                  process.DPP_ExecutionID, process.DPP_File_Name, process.DPP_Subject,
 │                  process.To_Email, process.DPP_HasAttachment]
 │
 ├─→ TRY/CATCH Wrapper (shape17)
 │    │
 │    ├─→ TRY PATH:
 │    │    │
 │    │    ├─→ Transform Request (shape29 - Map: Leave Create Map)
 │    │    │    └─→ Transform: D365 format → Oracle Fusion format
 │    │    │    └─→ Field mappings: employeeNumber→personNumber, absenceStatusCode→absenceStatusCd, etc.
 │    │    │
 │    │    ├─→ Set URL (shape8)
 │    │    │    └─→ WRITES: [dynamicdocument.URL]
 │    │    │    └─→ Source: Defined parameter Resource_Path = "hcmRestApi/resources/11.13.18.05/absences"
 │    │    │
 │    │    ├─→ Log Request (shape49 - Notify)
 │    │    │    └─→ INFO: Current document (transformed Oracle request)
 │    │    │
 │    │    ├─→ HTTP POST: Leave Oracle Fusion Create (shape33) (Downstream - System Layer)
 │    │    │    └─→ READS: [dynamicdocument.URL]
 │    │    │    └─→ WRITES: [meta.base.applicationstatuscode, dynamicdocument.DDP_RespHeader]
 │    │    │    └─→ HTTP: [Expected: 200/201, Error: 400/401/404/500]
 │    │    │    └─→ Method: POST
 │    │    │    └─→ URL: Oracle Fusion base URL + dynamicdocument.URL
 │    │    │    └─→ Auth: Basic (INTEGRATION.USER@al-ghurair.com)
 │    │    │    └─→ Request: Oracle Fusion leave creation JSON
 │    │    │    └─→ Response: Oracle Fusion absence entry object (80+ fields)
 │    │    │
 │    │    ├─→ Decision: HTTP Status 20* check (shape2)
 │    │    │    └─→ READS: [meta.base.applicationstatuscode]
 │    │    │    └─→ Comparison: wildcard match "20*"
 │    │    │    └─→ Data Source: TRACK_PROPERTY (from Oracle Fusion API response)
 │    │    │    └─→ Decision Type: POST_OPERATION
 │    │    │    │
 │    │    │    ├─→ IF TRUE (HTTP 20x - Success):
 │    │    │    │    │
 │    │    │    │    ├─→ Transform Success Response (shape34 - Map: Oracle Response Map)
 │    │    │    │    │    └─→ Transform: Oracle response → D365 success format
 │    │    │    │    │    └─→ Mappings: personAbsenceEntryId, status="success", message="...", success="true"
 │    │    │    │    │
 │    │    │    │    └─→ Return Documents: Success Response (shape35) [HTTP: 200] [SUCCESS]
 │    │    │    │         └─→ Response: {status: "success", message: "Data successfully sent to Oracle Fusion",
 │    │    │    │                        personAbsenceEntryId: <ID>, success: "true"}
 │    │    │    │
 │    │    │    └─→ IF FALSE (HTTP Non-20x - Error):
 │    │    │         │
 │    │    │         ├─→ Decision: Check Response Content Type (shape44)
 │    │    │         │    └─→ READS: [dynamicdocument.DDP_RespHeader]
 │    │    │         │    └─→ Comparison: equals "gzip"
 │    │    │         │    └─→ Data Source: TRACK_PROPERTY (from Oracle Fusion API response header)
 │    │    │         │    └─→ Decision Type: POST_OPERATION
 │    │    │         │    │
 │    │    │         │    ├─→ IF TRUE (gzip response):
 │    │    │         │    │    │
 │    │    │         │    │    ├─→ Decompress (shape45 - Groovy Script)
 │    │    │         │    │    │    └─→ Script: GZIPInputStream decompression
 │    │    │         │    │    │
 │    │    │         │    │    ├─→ Extract Error Message (shape46)
 │    │    │         │    │    │    └─→ WRITES: [process.DPP_ErrorMessage]
 │    │    │         │    │    │    └─→ Source: meta.base.applicationstatusmessage (after decompression)
 │    │    │         │    │    │
 │    │    │         │    │    ├─→ Transform Error (shape47 - Map: Error Map)
 │    │    │         │    │    │    └─→ READS: [process.DPP_ErrorMessage]
 │    │    │         │    │    │    └─→ Transform: Error → D365 error format
 │    │    │         │    │    │
 │    │    │         │    │    └─→ Return Documents: Error Response (shape48) [HTTP: 400] [ERROR]
 │    │    │         │    │         └─→ Response: {status: "failure", message: <error>, success: "false"}
 │    │    │         │    │
 │    │    │         │    └─→ IF FALSE (non-gzip response):
 │    │    │         │         │
 │    │    │         │         ├─→ Extract Error Message (shape39)
 │    │    │         │         │    └─→ WRITES: [process.DPP_ErrorMessage]
 │    │    │         │         │    └─→ Source: meta.base.applicationstatusmessage
 │    │    │         │         │
 │    │    │         │         ├─→ Transform Error (shape40 - Map: Error Map)
 │    │    │         │         │    └─→ READS: [process.DPP_ErrorMessage]
 │    │    │         │         │    └─→ Transform: Error → D365 error format
 │    │    │         │         │
 │    │    │         │         └─→ Return Documents: Error Response (shape36) [HTTP: 400] [ERROR]
 │    │    │         │              └─→ Response: {status: "failure", message: <error>, success: "false"}
 │    │    │
 │    └─→ CATCH PATH (if exception in Try block):
 │         │
 │         ├─→ Branch: Error Notification and Response (shape20)
 │         │    └─→ Classification: SEQUENTIAL (data dependency + API calls)
 │         │    └─→ Dependency: Path 1 MUST execute BEFORE Path 2
 │         │    └─→ Reason: Path 2 reads process.DPP_ErrorMessage written by Path 1
 │         │    └─→ Reason: Path 1 contains email API call (SEQUENTIAL required)
 │         │    │
 │         │    ├─→ Path 1: Email Notification (FIRST)
 │         │    │    │
 │         │    │    ├─→ Extract Error Message (shape19)
 │         │    │    │    └─→ WRITES: [process.DPP_ErrorMessage]
 │         │    │    │    └─→ Source: meta.base.catcherrorsmessage
 │         │    │    │
 │         │    │    └─→ Send Email Notification (shape21 - Subprocess Call)
 │         │    │         └─→ READS: [process.DPP_Process_Name, process.DPP_AtomName, process.DPP_ExecutionID,
 │         │    │                     process.DPP_ErrorMessage, process.DPP_Payload, process.To_Email,
 │         │    │                     process.DPP_File_Name, process.DPP_Subject, process.DPP_HasAttachment]
 │         │    │         └─→ Subprocess: (Sub) Office 365 Email
 │         │    │         └─→ Action: Send email with error details to BoomiIntegrationTeam@al-ghurair.com
 │         │    │         └─→ Attachment: Y (includes payload file)
 │         │    │
 │         │    └─→ Path 2: Error Response (SECOND - after Path 1 completes)
 │         │         │
 │         │         ├─→ Transform Error (shape41 - Map: Error Map)
 │         │         │    └─→ READS: [process.DPP_ErrorMessage] (written by Path 1)
 │         │         │    └─→ Transform: Error → D365 error format
 │         │         │
 │         │         └─→ Return Documents: Error Response (shape43) [HTTP: 400] [ERROR]
 │         │              └─→ Response: {status: "failure", message: <error>, success: "false"}
 │
 └─→ END

**Note:** Detailed request/response JSON examples for all operations and return paths are documented in Section 18 (Request/Response JSON Examples).
```

**Key Sequence Diagram Features:**

1. **Data Dependencies Enforced:**
   - shape8 writes dynamicdocument.URL → shape33 reads it
   - shape33 writes status/header → shape2/shape44 read them
   - shape19 writes process.DPP_ErrorMessage → shape41 reads it
   - shape38 writes email properties → shape21 reads them

2. **Decision Logic:**
   - shape2 checks HTTP status (POST-OPERATION decision on RESPONSE data)
   - shape44 checks Content-Encoding (POST-OPERATION decision on RESPONSE header)
   - Both decisions execute AFTER shape33 HTTP operation

3. **Branch Execution:**
   - shape20 branch executes SEQUENTIALLY (Path 1 → Path 2)
   - Dependency: Path 2 depends on process.DPP_ErrorMessage from Path 1
   - API calls: Path 1 contains email API call (SEQUENTIAL required)

4. **Return Paths:**
   - Success: shape35 [HTTP 200]
   - Error (non-gzip): shape36 [HTTP 400]
   - Error (Try/Catch): shape43 [HTTP 400]
   - Error (gzip): shape48 [HTTP 400]

---

## 14. Subprocess Analysis

### 14.1 Subprocess: (Sub) Office 365 Email (a85945c5-3004-42b9-80b1-104f465cd1fb)

**Subprocess ID:** a85945c5-3004-42b9-80b1-104f465cd1fb  
**Subprocess Name:** (Sub) Office 365 Email  
**Purpose:** Send email notifications with error details  
**Called By:** shape21 (main process)

#### 14.1.1 Internal Flow

```
START (shape1 - Subprocess entry)
 │
 ├─→ TRY/CATCH Wrapper (shape2)
 │    │
 │    ├─→ TRY PATH:
 │    │    │
 │    │    ├─→ Decision: Attachment_Check (shape4)
 │    │    │    └─→ Check: process.DPP_HasAttachment equals "Y"?
 │    │    │    │
 │    │    │    ├─→ IF TRUE (with attachment):
 │    │    │    │    │
 │    │    │    │    ├─→ Build Mail Body HTML (shape11)
 │    │    │    │    │    └─→ HTML template with execution details table
 │    │    │    │    │
 │    │    │    │    ├─→ Set Mail Body Property (shape14)
 │    │    │    │    │    └─→ WRITES: process.DPP_MailBody (from current document)
 │    │    │    │    │
 │    │    │    │    ├─→ Set Payload as Attachment (shape15)
 │    │    │    │    │    └─→ Message: {1} where {1} = process.DPP_Payload
 │    │    │    │    │
 │    │    │    │    ├─→ Set Mail Properties (shape6)
 │    │    │    │    │    └─→ WRITES: [connector.mail.fromAddress, connector.mail.toAddress,
 │    │    │    │    │                  connector.mail.subject, connector.mail.body, connector.mail.filename]
 │    │    │    │    │
 │    │    │    │    ├─→ Send Email with Attachment (shape3)
 │    │    │    │    │    └─→ Operation: Email w Attachment
 │    │    │    │    │    └─→ Connection: Office 365 Email
 │    │    │    │    │
 │    │    │    │    └─→ Stop (Continue) (shape5)
 │    │    │    │         └─→ Returns to main process
 │    │    │    │
 │    │    │    └─→ IF FALSE (without attachment):
 │    │    │         │
 │    │    │         ├─→ Build Mail Body HTML (shape23)
 │    │    │         │    └─→ HTML template with execution details table
 │    │    │         │
 │    │    │         ├─→ Set Mail Body Property (shape22)
 │    │    │         │    └─→ WRITES: process.DPP_MailBody (from current document)
 │    │    │         │
 │    │    │         ├─→ Set Mail Properties (shape20)
 │    │    │         │    └─→ WRITES: [connector.mail.fromAddress, connector.mail.toAddress,
 │    │    │         │                  connector.mail.subject, connector.mail.body]
 │    │    │         │
 │    │    │         ├─→ Send Email without Attachment (shape7)
 │    │    │         │    └─→ Operation: Email W/O Attachment
 │    │    │         │    └─→ Connection: Office 365 Email
 │    │    │         │
 │    │    │         └─→ Stop (Continue) (shape9)
 │    │    │              └─→ Returns to main process
 │    │    │
 │    └─→ CATCH PATH:
 │         │
 │         └─→ Throw Exception (shape10)
 │              └─→ Exception message: {1} where {1} = meta.base.catcherrorsmessage
```

#### 14.1.2 Return Paths

| Return Label | Termination Shape | Condition | Type |
|---|---|---|---|
| (Implicit Success) | shape5 (Stop continue=true) | Attachment = Y | Success |
| (Implicit Success) | shape9 (Stop continue=true) | Attachment = N | Success |
| (Exception) | shape10 (Exception) | Error in email sending | Error |

#### 14.1.3 Properties Read from Main Process

| Property Name | Used For |
|---|---|
| process.DPP_Process_Name | Email body (execution details) |
| process.DPP_AtomName | Email body (environment info) |
| process.DPP_ExecutionID | Email body (execution ID) |
| process.DPP_ErrorMessage | Email body (error details) |
| process.DPP_Payload | Email attachment (if HasAttachment = Y) |
| process.To_Email | Email recipient address |
| process.DPP_File_Name | Email attachment filename |
| process.DPP_Subject | Email subject line |
| process.DPP_HasAttachment | Decision flag (Y/N) |

#### 14.1.4 Properties Written by Subprocess

| Property Name | Written By | Value |
|---|---|---|
| process.DPP_MailBody | shape14 or shape22 | HTML email body with execution details |

#### 14.1.5 Subprocess Summary

**Purpose:** Send email notifications when errors occur in the main process  
**Execution:** Synchronous (wait=true in main process)  
**Return Behavior:** Returns to main process after email is sent  
**Error Handling:** Throws exception if email sending fails

---

## 15. Critical Patterns Identified

### 15.1 Pattern: Error Check (Success vs Failure)

**Location:** Decision shape2 (HTTP Status 20 check)

**Description:** Check HTTP status code from Oracle Fusion API to determine success or failure path

**Flow:**
1. Execute HTTP POST to Oracle Fusion API (shape33)
2. Check meta.base.applicationstatuscode (shape2)
3. If "20*" (200, 201, etc.) → Success path (transform and return success response)
4. If not "20*" → Error path (further check for gzip, then return error response)

**Business Logic:**
- Oracle Fusion API returns 20x for successful leave creation
- Oracle Fusion API returns 40x or 50x for errors (validation, auth, server errors)
- Success path maps Oracle response to D365 format with personAbsenceEntryId
- Error path extracts error message and maps to D365 error format

### 15.2 Pattern: Conditional Logic (Decompression if gzipped)

**Location:** Decision shape44 (Check Response Content Type)

**Description:** Check if error response is gzip-compressed, decompress if needed

**Flow:**
1. If Oracle API returns non-20x status, check Content-Encoding header (shape44)
2. If "gzip" → Decompress with Groovy script (shape45), extract error, return
3. If not "gzip" → Extract error directly, return

**Business Logic:**
- Oracle Fusion API may compress error responses with gzip
- Need to decompress to extract readable error message
- Both paths eventually return error response to D365

### 15.3 Pattern: Parallel Error Notification and Response

**Location:** Branch shape20 (in Try/Catch error handler)

**Description:** SEQUENTIAL execution of email notification and error response return

**Flow:**
1. If exception occurs in Try block, branch to two paths
2. Path 1 (FIRST): Extract error message, send email notification to team
3. Path 2 (SECOND): Transform error to D365 format, return error response

**Business Logic:**
- Team needs to be notified of errors for monitoring
- D365 caller needs error response to handle failure
- Path 1 MUST execute FIRST because Path 2 depends on process.DPP_ErrorMessage
- Email sending contains API call, so SEQUENTIAL execution required

**CRITICAL:** Despite appearing as parallel branch, execution is SEQUENTIAL due to:
1. Data dependency: Path 2 reads process.DPP_ErrorMessage written by Path 1
2. API call requirement: Path 1 contains email API call (no parallel API calls allowed)

### 15.4 Pattern: Try/Catch Error Handling

**Location:** Try/Catch wrapper shape17

**Description:** Wrap transformation and API call in Try/Catch to handle exceptions

**Flow:**
1. Try: Transform request, set URL, call Oracle API, handle response
2. Catch: Extract error message, send email notification, return error response

**Business Logic:**
- Mapping errors (missing fields, data type mismatches)
- Network errors (connection failures, timeouts)
- All errors are caught, logged via email, and returned as error response

---

## 16. System Layer Identification

### 16.1 Downstream Systems

| System Name | Connection | Operation | Protocol | Purpose |
|---|---|---|---|---|
| Oracle Fusion HCM | Oracle Fusion (aa1fcb29) | Leave Oracle Fusion Create | HTTP REST | Create leave/absence entries |
| Office 365 Email | Office 365 Email (00eae79b) | Email w Attachment, Email W/O Attachment | SMTP | Send error notification emails |

### 16.2 System Layer APIs Required

#### System API 1: Oracle Fusion HCM Leave Management

**API Name:** OracleFusionHcmLeaveSystemApi  
**Purpose:** Create leave/absence entries in Oracle Fusion HCM  
**Protocol:** HTTP REST  
**Authentication:** Basic Authentication  
**Base URL:** https://iaaxey-dev3.fa.ocs.oraclecloud.com:443  
**Resource Path:** /hcmRestApi/resources/11.13.18.05/absences  
**Method:** POST

**Request DTO:**
```csharp
public class OracleFusionLeaveRequest
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
public class OracleFusionLeaveResponse
{
    public int PersonAbsenceEntryId { get; set; }
    public string PersonNumber { get; set; }
    public string AbsenceType { get; set; }
    // ... additional 70+ fields
}
```

#### System API 2: Office 365 Email Notification

**API Name:** Office365EmailSystemApi  
**Purpose:** Send email notifications  
**Protocol:** SMTP  
**Authentication:** SMTP AUTH  
**Host:** smtp-mail.outlook.com  
**Port:** 587  
**TLS:** true

**Request DTO:**
```csharp
public class EmailRequest
{
    public string From { get; set; }
    public string To { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }
    public bool HasAttachment { get; set; }
    public string AttachmentFilename { get; set; }
    public string AttachmentContent { get; set; }
}
```

---

## 17. Function Exposure Decision Table

### 17.1 Process Layer Function Exposure

**Process Name:** HCM Leave Create  
**Business Domain:** Human Capital Management (HCM)  
**Integration Pattern:** Synchronous API (D365 → Oracle Fusion HCM)

| Criteria | Value | Decision |
|---|---|---|
| **Multiple Entry Points?** | No - Single Web Service entry point | ✅ Expose single Process Layer function |
| **Complex Orchestration?** | Yes - Transform, API call, error handling, email notification | ✅ Expose as Process Layer |
| **Multiple System APIs?** | Yes - Oracle Fusion HCM + Email | ✅ Encapsulate in Process Layer |
| **Business Logic?** | Yes - Error handling, response transformation, notification | ✅ Process Layer function required |
| **Reusability Across Projects?** | High - Leave creation is common HCM use case | ✅ Process Layer for reusability |

**DECISION:** ✅ **Expose ONE Process Layer Function**

**Function Name:** `CreateHcmLeaveAsync`  
**Signature:**
```csharp
Task<HcmLeaveResponse> CreateHcmLeaveAsync(HcmLeaveRequest request)
```

**Process Layer Responsibilities:**
1. Accept D365 leave request
2. Transform to Oracle Fusion format
3. Call Oracle Fusion HCM System API
4. Handle success/error responses
5. Send email notifications on errors
6. Return standardized response to D365

**System Layer Dependencies:**
1. `OracleFusionHcmLeaveSystemApi.CreateLeaveAsync()` - Create leave in Oracle
2. `Office365EmailSystemApi.SendEmailAsync()` - Send error notifications

### 17.2 System Layer Functions

#### System Function 1: Oracle Fusion HCM Leave Creation

**Function Name:** `CreateLeaveAsync`  
**API Name:** OracleFusionHcmLeaveSystemApi  
**Purpose:** Create leave/absence entry in Oracle Fusion HCM

**Signature:**
```csharp
Task<OracleFusionLeaveResponse> CreateLeaveAsync(OracleFusionLeaveRequest request)
```

**Responsibilities:**
- HTTP POST to Oracle Fusion HCM API
- Basic authentication
- Handle HTTP response (20x success, 40x/50x errors)
- Decompress gzipped responses
- Return Oracle response or throw exception

#### System Function 2: Office 365 Email Sending

**Function Name:** `SendEmailAsync`  
**API Name:** Office365EmailSystemApi  
**Purpose:** Send email notifications via Office 365 SMTP

**Signature:**
```csharp
Task SendEmailAsync(EmailRequest request)
```

**Responsibilities:**
- SMTP connection to Office 365
- SMTP authentication
- Send email with/without attachment
- Handle SMTP errors

### 17.3 Function Explosion Prevention

**Total Functions Exposed:**
- **Process Layer:** 1 function (`CreateHcmLeaveAsync`)
- **System Layer:** 2 functions (`CreateLeaveAsync`, `SendEmailAsync`)
- **Total:** 3 functions

**Prevented Function Explosion:**
- ❌ NOT exposing separate functions for: transform, error handling, decision logic
- ❌ NOT exposing subprocess as separate Process Layer function
- ✅ Encapsulated all orchestration in single Process Layer function
- ✅ System Layer functions are reusable across projects

**Justification:**
- Single business use case: Create leave in Oracle Fusion from D365
- All orchestration (transform, call API, handle errors, notify) is part of one business process
- System Layer functions (Oracle API, Email) are generic and reusable
- No risk of function explosion - clean API-Led architecture

---

## 18. Request/Response JSON Examples

### 18.1 Process Layer Entry Point

#### Request JSON Example (from D365)

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
  "status": "success",
  "message": "Data successfully sent to Oracle Fusion",
  "personAbsenceEntryId": 123456,
  "success": "true"
}
```

**Error Response - Validation Error (HTTP 400):**

```json
{
  "status": "failure",
  "message": "Oracle Fusion API returned error: Invalid absence type 'Sick Leave'. Valid values are: ...",
  "success": "false"
}
```

**Error Response - Mapping Error (HTTP 400):**

```json
{
  "status": "failure",
  "message": "Mapping error: Required field 'employeeNumber' is missing",
  "success": "false"
}
```

**Error Response - Network Error (HTTP 400):**

```json
{
  "status": "failure",
  "message": "Connection error: Unable to connect to Oracle Fusion HCM API",
  "success": "false"
}
```

### 18.2 Downstream System Layer Calls

#### System API 1: Oracle Fusion HCM Leave Creation

**Request JSON (to Oracle Fusion API):**

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

**Success Response (HTTP 200/201):**

```json
{
  "personAbsenceEntryId": 123456,
  "personNumber": 9000604,
  "absenceType": "Sick Leave",
  "employer": "Al Ghurair Investment LLC",
  "startDate": "2024-03-24",
  "endDate": "2024-03-25",
  "absenceStatusCd": "SUBMITTED",
  "approvalStatusCd": "APPROVED",
  "startDateDuration": 1,
  "endDateDuration": 1,
  "duration": 2,
  "absenceCaseId": 78910,
  "absenceEntryBasicFlag": true,
  "personId": 300000012345678,
  "createdBy": "INTEGRATION.USER",
  "creationDate": "2024-03-24T10:30:00Z",
  "lastUpdateDate": "2024-03-24T10:30:00Z",
  "lastUpdatedBy": "INTEGRATION.USER",
  "objectVersionNumber": 1
  // ... additional 60+ fields
}
```

**Error Response (HTTP 400):**

```json
{
  "title": "Bad Request",
  "status": 400,
  "detail": "Invalid absence type 'Sick Leave'. Valid values are: Annual Leave, Sick Leave Medical Certificate Required, Unpaid Leave",
  "o:errorCode": "VALIDATION_ERROR",
  "o:errorPath": "absenceType"
}
```

**Error Response (HTTP 401):**

```json
{
  "title": "Unauthorized",
  "status": 401,
  "detail": "Authentication credentials are missing or invalid"
}
```

**Error Response (HTTP 404):**

```json
{
  "title": "Not Found",
  "status": 404,
  "detail": "Employee with personNumber 9000604 not found"
}
```

**Error Response (HTTP 500):**

```json
{
  "title": "Internal Server Error",
  "status": 500,
  "detail": "An unexpected error occurred while processing your request"
}
```

#### System API 2: Office 365 Email Notification

**Email Request (constructed from process properties):**

```
From: Boomi.Dev.failures@al-ghurair.com
To: BoomiIntegrationTeam@al-ghurair.com
Subject: DEV Failure : Cloud-Prod (HCM_Leave Create) has errors to report
Body: 
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
            <td>HCM_Leave Create</td>
          </tr>
          <tr>
            <th scope="row"><b><font size="2">Environment</font></b></th>
            <td>Cloud-Prod</td>
          </tr>
          <tr>
            <th scope="row"><b><font size="2">Execution ID</font></b></th>
            <td>execution-abc-123-xyz</td>
          </tr>
          <tr>
            <th scope="row"><b><font size="2">Error Details</font></b></th>
            <td>Mapping error: Required field 'employeeNumber' is missing</td>
          </tr>
        </table>
      </font>
    </pre>
    <text>P.S: This is system generated email.</text>
  </body>
</html>

Attachment: HCM_Leave Create_2024-03-24T10:30:00.000Z.txt
Attachment Content: 
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

---

## 19. Validation Checklist

### 19.1 Data Dependencies

- [x] All property WRITES identified (Section 2.1)
- [x] All property READS identified (Section 2.2)
- [x] Dependency graph built (Section 8)
- [x] Execution order satisfies all dependencies (Section 12.3 - all reads after writes verified)

### 19.2 Decision Analysis

- [x] ALL decision shapes inventoried (Section 10.1 - 2 decisions)
- [x] BOTH TRUE and FALSE paths traced to termination (Section 10.1 - all paths traced)
- [x] Pattern type identified for each decision (Section 10.2 - Error Check, Conditional Logic)
- [x] Early exits identified and documented (Section 10.1 - FALSE paths lead to error returns)
- [x] Convergence points identified (Section 10.1 - None)
- [x] Decision data sources identified (Section 10.3 - Both TRACK_PROPERTY from Oracle response)
- [x] Decision types classified (Section 10.1 - Both POST_OPERATION)
- [x] Actual execution order verified (Section 10.1 - Both after shape33 HTTP operation)

### 19.3 Branch Analysis

- [x] Branch classified as parallel or sequential (Section 11.1.4 - SEQUENTIAL)
- [x] Self-check: Did not assume parallel (Section 11.1.4 - Analyzed dependencies)
- [x] If sequential: dependency_order built using topological sort (Section 11.1.5)
- [x] Each path traced to terminal point (Section 11.1.6)
- [x] Convergence points identified (Section 11.1.7 - None)
- [x] Execution continuation point determined (Section 11.1.7 - None)
- [x] API calls detected (Section 11.1.8 - Email API in Path 1, SEQUENTIAL required)

### 19.4 Sequence Diagram

- [x] Format follows required structure (Section 13 - Operation → Decision → Operation)
- [x] Each operation shows READS and WRITES (Section 13 - All operations documented)
- [x] Decisions show both TRUE and FALSE paths (Section 13 - Both paths shown)
- [x] Cross-validation: Sequence diagram matches control flow graph (Section 13 references Section 9)
- [x] Cross-validation: Execution order matches dependency graph (Section 13 references Section 8)
- [x] Early exits marked (Section 13 - [ERROR] markers on error returns)
- [x] Conditional execution marked (Section 13 - Decision conditions shown)
- [x] Subprocess internal flows documented (Section 14.1.1)
- [x] Subprocess return paths mapped to main process (Section 14.1.2)

### 19.5 Subprocess Analysis

- [x] ALL subprocesses analyzed (Section 14 - Office 365 Email subprocess)
- [x] Return paths identified (Section 14.1.2 - Success and Exception paths)
- [x] Return path labels mapped to main process shapes (Section 14.1.2)
- [x] Properties written by subprocess documented (Section 14.1.4)
- [x] Properties read by subprocess from main process documented (Section 14.1.3)

### 19.6 Edge Cases

- [x] Nested branches/decisions analyzed (None in this process)
- [x] Loops identified (None in this process)
- [x] Property chains traced (Section 8.1 - All chains documented)
- [x] Circular dependencies detected and resolved (None in this process)
- [x] Try/Catch error paths documented (Section 15.4 - Try/Catch pattern documented)

### 19.7 Property Extraction Completeness

- [x] All property patterns searched (Section 2.2 - track, process, definedparameter, execution)
- [x] Message parameters checked for process properties (Section 14.1.1 - Subprocess message shapes)
- [x] Operation headers/path parameters checked (Section 8.1 - dynamicdocument.URL and DDP_RespHeader)
- [x] Decision track properties identified (Section 10.1 - meta.base.applicationstatuscode, dynamicdocument.DDP_RespHeader)
- [x] Document properties that read other properties identified (Section 2.2)

### 19.8 Input/Output Structure Analysis (Contract Verification)

- [x] Entry point operation identified (Section 3.1)
- [x] Request profile identified and loaded (Section 3.2)
- [x] Request profile structure analyzed (Section 3.3 - JSON structure)
- [x] Array vs single object detected (Section 3.3 - Single object)
- [x] Array cardinality documented (Section 3.3 - N/A)
- [x] ALL request fields extracted (Section 3.4 - 9 fields)
- [x] Request field paths documented (Section 3.4 - Root/Object/fieldName)
- [x] Request field mapping table generated (Section 3.4 - Boomi → Azure DTO)
- [x] Response profile identified and loaded (Section 4.1)
- [x] Response profile structure analyzed (Section 4.2 - JSON structure)
- [x] ALL response fields extracted (Section 4.3 - 4 fields)
- [x] Response field mapping table generated (Section 4.3)
- [x] Document processing behavior determined (Section 3.5 - single_document)
- [x] Input/Output structure documented in Phase 1 document (Sections 3 & 4)

### 19.9 HTTP Status Codes and Return Path Responses

- [x] Section 7 (HTTP Status Codes and Return Path Responses - Step 1e) present (Section 7)
- [x] All return paths documented with HTTP status codes (Section 7.1 - 4 return paths)
- [x] Response JSON examples provided for each return path (Sections 7.2-7.5)
- [x] Populated fields documented for each return path (Sections 7.2-7.5 - source and populated by)
- [x] Decision conditions leading to each return documented (Sections 7.2-7.5)
- [x] Error codes and success codes documented for each return path (Sections 7.2-7.5)
- [x] Downstream operation HTTP status codes documented (Section 7.6 - Expected: 200/201, Error: 400/401/404/500)
- [x] Error handling strategy documented for downstream operations (Section 7.6 - Check status code wildcard "20*")

### 19.10 Request/Response JSON Examples

- [x] Section 18 (Request/Response JSON Examples) present (Section 18)
- [x] Process Layer entry point request JSON example provided (Section 18.1)
- [x] Process Layer response JSON examples provided (Section 18.1 - success and 3 error scenarios)
- [x] Downstream System Layer request JSON examples provided (Section 18.2 - Oracle API and Email)
- [x] Downstream System Layer response JSON examples provided (Section 18.2 - success and 4 error scenarios)

### 19.11 Map Analysis

- [x] ALL map files identified and loaded (Section 6.1 - 3 maps)
- [x] Request transformation map analyzed (Section 6.2 - Leave Create Map)
- [x] Field mappings extracted from each map (Sections 6.2-6.4)
- [x] Profile vs map field name discrepancies documented (Section 6.2 - employeeNumber→personNumber, etc.)
- [x] Map field names marked as AUTHORITATIVE (Section 6.2 - CRITICAL RULE documented)
- [x] Scripting functions analyzed (Section 6.4 - PropertyGet function in error map)
- [x] Static values identified and documented (Sections 6.3-6.4 - Default values in response maps)
- [x] Process property mappings documented (Section 6.4 - DPP_ErrorMessage)
- [x] Map Analysis documented in Phase 1 document (Section 6)

### 19.12 Self-Check Questions (MANDATORY)

1. ✅ Did I verify business logic FIRST before following dragpoints? **YES** (Section 12.1)
2. ✅ Did I identify what each operation does and what it produces? **YES** (Section 12.1)
3. ✅ Did I identify which operations MUST execute first based on business logic? **YES** (Section 12.1)
4. ✅ Did I check data dependencies FIRST before following dragpoints? **YES** (Section 8)
5. ✅ Did I use operation response analysis from Step 1c? **YES** (Section 5)
6. ✅ Did I use decision analysis from Step 7? **YES** (Section 10)
7. ✅ Did I use dependency graph from Step 4? **YES** (Section 8)
8. ✅ Did I use branch analysis from Step 8? **YES** (Section 11)
9. ✅ Did I verify all property reads happen after property writes? **YES** (Section 12.3)
10. ✅ Did I follow topological sort order for sequential branches? **YES** (Section 11.1.5)
11. ✅ Did I analyze ALL map files? **YES** (Section 6)
12. ✅ Did I identify SOAP request maps? **N/A** (No SOAP operations in this process)
13. ✅ Did I extract actual field names from maps? **YES** (Section 6)
14. ✅ Did I compare profile field names vs map field names? **YES** (Section 6.2)
15. ✅ Did I mark map field names as AUTHORITATIVE? **YES** (Section 6.2)
16. ✅ Did I analyze scripting functions in maps? **YES** (Section 6.4)
17. ✅ Did I extract element names from maps? **N/A** (JSON maps, not SOAP)
18. ✅ Did I verify namespace prefixes from message shapes? **N/A** (No SOAP operations)
19. ✅ Did I extract HTTP status codes for all return paths? **YES** (Section 7)
20. ✅ Did I document response JSON for each return path? **YES** (Section 7)
21. ✅ Did I document populated fields for each return path? **YES** (Section 7)
22. ✅ Did I extract HTTP status codes for downstream operations? **YES** (Section 7.6)
23. ✅ Did I create request/response JSON examples? **YES** (Section 18)

### 19.13 FINAL VALIDATION

**ALL MANDATORY SECTIONS PRESENT:**
- [x] Section 1: Operations Inventory
- [x] Section 2: Process Properties Analysis
- [x] Section 3: Input Structure Analysis (Step 1a)
- [x] Section 4: Response Structure Analysis (Step 1b)
- [x] Section 5: Operation Response Analysis (Step 1c)
- [x] Section 6: Map Analysis (Step 1d)
- [x] Section 7: HTTP Status Codes and Return Path Responses (Step 1e)
- [x] Section 8: Data Dependency Graph (Step 4)
- [x] Section 9: Control Flow Graph (Step 5)
- [x] Section 10: Decision Shape Analysis (Step 7)
- [x] Section 11: Branch Shape Analysis (Step 8)
- [x] Section 12: Execution Order (Step 9)
- [x] Section 13: Sequence Diagram (Step 10)
- [x] Section 14: Subprocess Analysis
- [x] Section 15: Critical Patterns Identified
- [x] Section 16: System Layer Identification
- [x] Section 17: Function Exposure Decision Table
- [x] Section 18: Request/Response JSON Examples
- [x] Section 19: Validation Checklist

**ALL SELF-CHECK QUESTIONS ANSWERED YES:** ✅

**PHASE 1 EXTRACTION COMPLETE:** ✅  
**Ready for Phase 2 (Code Generation):** ✅

---

**END OF BOOMI_EXTRACTION_PHASE1.md**
