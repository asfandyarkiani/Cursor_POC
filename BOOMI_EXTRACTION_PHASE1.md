# BOOMI EXTRACTION PHASE 1 DOCUMENT
## HCM Leave Create Process

**Process Name:** HCM_Leave Create  
**Process ID:** ca69f858-785f-4565-ba1f-b4edc6cca05b  
**Description:** This Process will sync the Leave data between D365 and Oracle HCM  
**Date:** 2026-02-11  
**Version:** 29

---

## TABLE OF CONTENTS

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
11. [Subprocess Analysis (Step 7a)](#11-subprocess-analysis-step-7a)
12. [Branch Shape Analysis (Step 8)](#12-branch-shape-analysis-step-8)
13. [Execution Order (Step 9)](#13-execution-order-step-9)
14. [Sequence Diagram (Step 10)](#14-sequence-diagram-step-10)
15. [System Layer Identification](#15-system-layer-identification)
16. [Request/Response JSON Examples](#16-requestresponse-json-examples)
17. [Function Exposure Decision Table](#17-function-exposure-decision-table)
18. [Validation Checklist](#18-validation-checklist)

---

## 1. OPERATIONS INVENTORY

### 1.1 Main Process Operations

| Operation ID | Operation Name | Type | Sub-Type | Purpose |
|--------------|----------------|------|----------|---------|
| 8f709c2b-e63f-4d5f-9374-2932ed70415d | Create Leave Oracle Fusion OP | connector-action | wss (Web Service Server) | Entry point - Receive leave data from D365 |
| 6e8920fd-af5a-430b-a1d9-9fde7ac29a12 | Leave Oracle Fusion Create | connector-action | http | POST leave data to Oracle Fusion HCM REST API |

### 1.2 Subprocess Operations (Office 365 Email Subprocess)

| Operation ID | Operation Name | Type | Sub-Type | Purpose |
|--------------|----------------|------|----------|---------|
| af07502a-fafd-4976-a691-45d51a33b549 | Email w Attachment | connector-action | mail | Send email with attachment |
| 15a72a21-9b57-49a1-a8ed-d70367146644 | Email W/O Attachment | connector-action | mail | Send email without attachment |

### 1.3 Connections

| Connection ID | Connection Name | Type | Purpose |
|---------------|-----------------|------|---------|
| aa1fcb29-d146-4425-9ea6-b9698090f60e | Oracle Fusion | http | Connection to Oracle Fusion HCM (URL: https://iaaxey-dev3.fa.ocs.oraclecloud.com:443) |
| 00eae79b-2303-4215-8067-dcc299e42697 | Office 365 Email | mail | SMTP connection for sending emails (smtp-mail.outlook.com:587) |

### 1.4 Profiles

| Profile ID | Profile Name | Type | Purpose |
|------------|--------------|------|---------|
| febfa3e1-f719-4ee8-ba57-cdae34137ab3 | D365 Leave Create JSON Profile | profile.json | Request profile - Input from D365 |
| a94fa205-c740-40a5-9fda-3d018611135a | HCM Leave Create JSON Profile | profile.json | Oracle Fusion request payload |
| 316175c7-0e45-4869-9ac6-5f9d69882a62 | Oracle Fusion Leave Response JSON Profile | profile.json | Oracle Fusion API response |
| f4ca3a70-114a-4601-bad8-44a3eb20e2c0 | Leave D365 Response | profile.json | Response profile sent back to D365 |
| 23d7a2e9-5cb0-4e9c-9e4b-3154834bad0d | Dummy FF Profile | profile.flatfile | Dummy profile for error mapping |

### 1.5 Maps

| Map ID | Map Name | From Profile | To Profile | Purpose |
|--------|----------|--------------|------------|---------|
| c426b4d6-2aff-450e-b43b-59956c4dbc96 | Leave Create Map | D365 Leave Create JSON Profile | HCM Leave Create JSON Profile | Transform D365 input to Oracle Fusion format |
| e4fd3f59-edb5-43a1-aeae-143b600a064e | Oracle Fusion Leave Response Map | Oracle Fusion Leave Response JSON Profile | Leave D365 Response | Map Oracle Fusion success response to D365 response format |
| f46b845a-7d75-41b5-b0ad-c41a6a8e9b12 | Leave Error Map | Dummy FF Profile | Leave D365 Response | Map error message to D365 error response format |

---

## 2. PROCESS PROPERTIES ANALYSIS

### 2.1 Process Properties WRITTEN (Steps 2-3)

| Property ID | Property Name | Written By Shape(s) | Source |
|-------------|---------------|---------------------|--------|
| process.DPP_Process_Name | Dynamic Process Property - DPP_Process_Name | shape38 (Input_details) | Execution property: Process Name |
| process.DPP_AtomName | Dynamic Process Property - DPP_AtomName | shape38 (Input_details) | Execution property: Atom Name |
| process.DPP_Payload | Dynamic Process Property - DPP_Payload | shape38 (Input_details) | Current document (input payload) |
| process.DPP_ExecutionID | Dynamic Process Property - DPP_ExecutionID | shape38 (Input_details) | Execution property: Execution Id |
| process.DPP_File_Name | Dynamic Process Property - DPP_File_Name | shape38 (Input_details) | Concatenation of Process Name + Current Date + ".txt" |
| process.DPP_Subject | Dynamic Process Property - DPP_Subject | shape38 (Input_details) | Concatenation: Atom Name + " (" + Process Name + " ) has errors to report" |
| process.To_Email | Dynamic Process Property - To_Email | shape38 (Input_details) | Defined Process Property from PP_HCM_LeaveCreate_Properties |
| process.DPP_HasAttachment | Dynamic Process Property - DPP_HasAttachment | shape38 (Input_details) | Defined Process Property from PP_HCM_LeaveCreate_Properties (value: "Y") |
| process.DPP_ErrorMessage | Dynamic Process Property - DPP_ErrorMessage | shape19 (ErrorMsg), shape39 (error msg), shape46 (error msg) | Track property: meta.base.catcherrorsmessage or meta.base.applicationstatusmessage |
| dynamicdocument.URL | Dynamic Document Property - URL | shape8 (set URL) | Defined Process Property: Resource_Path = "hcmRestApi/resources/11.13.18.05/absences" |
| dynamicdocument.DDP_RespHeader | Dynamic Document Property - DDP_RespHeader | (Auto-set by HTTP operation) | HTTP response header: Content-Encoding |

### 2.2 Process Properties READ

| Property ID | Property Name | Read By Shape(s) | Usage |
|-------------|---------------|------------------|-------|
| process.DPP_ErrorMessage | Dynamic Process Property - DPP_ErrorMessage | Error mapping (map_f46b845a) | Used in error response mapping |
| process.To_Email | To_Email | Subprocess shapes (shape6, shape20) | Email recipient for error notifications |
| process.DPP_Subject | DPP_Subject | Subprocess shapes (shape6, shape20) | Email subject for error notifications |
| process.DPP_MailBody | DPP_MailBody | Subprocess shapes (shape6, shape20) | Email body content |
| process.DPP_HasAttachment | DPP_HasAttachment | Subprocess shape4 (Attachment_Check decision) | Determines whether to send email with or without attachment |
| process.DPP_Process_Name | DPP_Process_Name | Subprocess message shapes (shape11, shape23) | Used in email body template |
| process.DPP_AtomName | DPP_AtomName | Subprocess message shapes (shape11, shape23) | Used in email body template |
| process.DPP_ExecutionID | DPP_ExecutionID | Subprocess message shapes (shape11, shape23) | Used in email body template |
| process.DPP_Payload | DPP_Payload | Subprocess message shape15 | Used as email attachment content |
| process.DPP_File_Name | DPP_File_Name | Subprocess shape6, shape20 | Email attachment filename |

### 2.3 Defined Process Properties

#### From PP_HCM_LeaveCreate_Properties (e22c04db-7dfa-4b1b-9d88-77365b0fdb18)

| Property Key | Property Label | Default Value | Purpose |
|--------------|----------------|---------------|---------|
| c2d1a1e8-4bcb-435b-b1d2-bb10a209bcc0 | Resource_Path | hcmRestApi/resources/11.13.18.05/absences | Oracle Fusion REST API endpoint path |
| 71c90f6e-4f86-49f3-8aef-bae6668c73f9 | To_Email | BoomiIntegrationTeam@al-ghurair.com | Email recipient for error notifications |
| a717867b-67f4-4a72-be2d-c99c73309fdb | DPP_HasAttachment | Y | Flag indicating whether email should have attachment |

#### From PP_Office365_Email (0feff13f-8a2c-438a-b3c7-1909e2a7f533)

| Property Key | Property Label | Default Value | Purpose |
|--------------|----------------|---------------|---------|
| 804b6d40-eeee-4ac0-ab4b-94e1aca9f4b7 | From_Email | Boomi.Dev.failures@al-ghurair.com | Email sender address |
| d8fc7496-ec2c-4308-a4ca-e9f49c0c9500 | Environment | DEV Failure : | Environment prefix for email subject |

---

## 3. INPUT STRUCTURE ANALYSIS (Step 1a)

### 3.1 Entry Point Operation

**Operation ID:** 8f709c2b-e63f-4d5f-9374-2932ed70415d  
**Operation Name:** Create Leave Oracle Fusion OP  
**Operation Type:** connector-action (Web Services Server)  
**Sub-Type:** wss  
**Request Profile ID:** febfa3e1-f719-4ee8-ba57-cdae34137ab3  
**Request Profile Name:** D365 Leave Create JSON Profile  
**Input Type:** singlejson  
**Output Type:** singlejson

### 3.2 Request Profile Structure

**Profile Type:** profile.json  
**Root Element:** Root  
**Structure Path:** Root/Object  
**Is Array:** NO - Single object structure  
**Strict Mode:** false

### 3.3 Request Profile Fields

| Field Name | Key | Data Type | Mappable | Required | Description |
|------------|-----|-----------|----------|----------|-------------|
| employeeNumber | 3 | number | Yes | Yes | Employee identification number |
| absenceType | 4 | character | Yes | Yes | Type of leave/absence (e.g., "Sick Leave") |
| employer | 5 | character | Yes | Yes | Employer name (e.g., "Al Ghurair Investment LLC") |
| startDate | 6 | character | Yes | Yes | Leave start date (format: "YYYY-MM-DD") |
| endDate | 7 | character | Yes | Yes | Leave end date (format: "YYYY-MM-DD") |
| absenceStatusCode | 8 | character | Yes | Yes | Status code of the absence (e.g., "SUBMITTED") |
| approvalStatusCode | 9 | character | Yes | Yes | Approval status code (e.g., "APPROVED") |
| startDateDuration | 10 | number | Yes | Yes | Duration for start date (e.g., 1 = full day) |
| endDateDuration | 11 | number | Yes | Yes | Duration for end date (e.g., 1 = full day) |

### 3.4 Document Processing Behavior

**Input Type:** singlejson  
**Behavior:** Single Document Processing  
**Execution Pattern:** Single execution per request  
**Session Management:** One session per execution

**Azure Function Requirement:** 
- Process Layer must accept a single leave request object (not an array)
- Each request triggers one execution
- DTOs should be designed for single object, not collections

### 3.5 Input Structure JSON Example

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

### 3.6 Tracked Fields

**Tracked Field:** employeeNumber (hr_employee_id)  
**Source:** Profile element key 3 (Root/Object/employeeNumber)  
**Purpose:** Used for process tracking and monitoring

---

## 4. RESPONSE STRUCTURE ANALYSIS (Step 1b)

### 4.1 Response Profile

**Profile ID:** f4ca3a70-114a-4601-bad8-44a3eb20e2c0  
**Profile Name:** Leave D365 Response  
**Profile Type:** profile.json  
**Root Element:** leaveResponse

### 4.2 Response Profile Structure

**Structure Path:** leaveResponse/Object  
**Is Array:** NO - Single object  

### 4.3 Response Profile Fields

| Field Name | Key | Data Type | Mappable | Description |
|------------|-----|-----------|----------|-------------|
| status | 4 | character | Yes | Response status ("success" or "failure") |
| message | 5 | character | Yes | Response message |
| personAbsenceEntryId | 6 | number | Yes | Oracle Fusion generated absence entry ID (only populated on success) |
| success | 7 | character | Yes | Boolean flag as string ("true" or "false") |

### 4.4 Response JSON Examples

#### Success Response:
```json
{
  "status": "success",
  "message": "Data successfully sent to Oracle Fusion",
  "personAbsenceEntryId": 123456,
  "success": "true"
}
```

#### Error Response:
```json
{
  "status": "failure",
  "message": "[Error message from Oracle Fusion or Try/Catch]",
  "success": "false"
}
```

---

## 5. OPERATION RESPONSE ANALYSIS (Step 1c)

### 5.1 Oracle Fusion Leave Create Operation (6e8920fd-af5a-430b-a1d9-9fde7ac29a12)

**Operation Name:** Leave Oracle Fusion Create  
**Operation Type:** HTTP POST  
**Has Response Profile:** NO (responseProfileType: NONE)  
**Return Errors:** true  
**Return Responses:** true

**Response Handling:**
- Response is NOT mapped to a structured profile
- Instead, response is evaluated via:
  - **Track Property:** meta.base.applicationstatuscode (HTTP status code)
  - **Track Property:** meta.base.applicationstatusmessage (HTTP response message)
  - **Document Property:** dynamicdocument.DDP_RespHeader (Content-Encoding header)

**Extracted Data:**
- HTTP Status Code → Used in Decision shape2 (wildcard comparison "20*")
- Content-Encoding Header → Used in Decision shape44 (equals "gzip")
- Error Messages → Extracted by shapes 19, 39, 46 and written to process.DPP_ErrorMessage

**Data Consumers:**
- Decision shape2: Checks if HTTP status code matches "20*" (success)
- Decision shape44: Checks if Content-Encoding header equals "gzip"
- Map shapes (shape40, shape41, shape47): Use process.DPP_ErrorMessage in error response mapping

### 5.2 Business Logic Implications

**Operation: Leave Oracle Fusion Create (shape33)**
- **Produces:** HTTP status code, response message, response headers
- **Consumers:** 
  - Decision shape2 (checks status code)
  - Decision shape44 (checks response encoding)
  - Error mapping shapes (use error message)
- **Business Logic:** Operation MUST execute before any decision or error handling can occur

**Execution Order:**
1. HTTP POST to Oracle Fusion (shape33)
2. Evaluate HTTP Status Code (shape2)
3. If success (20*): Map success response
4. If failure (non-20*): Check if response is gzipped, decompress if needed, map error response

---

## 6. MAP ANALYSIS (Step 1d)

### 6.1 Map Inventory

| Map ID | Map Name | Type | From Profile | To Profile |
|--------|----------|------|--------------|------------|
| c426b4d6-2aff-450e-b43b-59956c4dbc96 | Leave Create Map | Request Transformation | D365 Leave Create JSON Profile (febfa3e1) | HCM Leave Create JSON Profile (a94fa205) |
| e4fd3f59-edb5-43a1-aeae-143b600a064e | Oracle Fusion Leave Response Map | Success Response | Oracle Fusion Leave Response JSON Profile (316175c7) | Leave D365 Response (f4ca3a70) |
| f46b845a-7d75-41b5-b0ad-c41a6a8e9b12 | Leave Error Map | Error Response | Dummy FF Profile (23d7a2e9) | Leave D365 Response (f4ca3a70) |

### 6.2 Map: Leave Create Map (c426b4d6-2aff-450e-b43b-59956c4dbc96)

**Purpose:** Transform D365 input format to Oracle Fusion HCM REST API format

**From Profile:** febfa3e1-f719-4ee8-ba57-cdae34137ab3 (D365 Leave Create JSON Profile)  
**To Profile:** a94fa205-c740-40a5-9fda-3d018611135a (HCM Leave Create JSON Profile)

#### Field Mappings:

| From Field (D365) | From Key | To Field (Oracle Fusion) | To Key | Mapping Type | Notes |
|-------------------|----------|--------------------------|--------|--------------|-------|
| employeeNumber | 3 | personNumber | 3 | Direct | Employee ID mapping |
| absenceType | 4 | absenceType | 4 | Direct | Leave type (e.g., "Sick Leave") |
| employer | 5 | employer | 5 | Direct | Employer name |
| startDate | 6 | startDate | 6 | Direct | Leave start date |
| endDate | 7 | endDate | 7 | Direct | Leave end date |
| absenceStatusCode | 8 | absenceStatusCd | 8 | Direct | Status code transformation |
| approvalStatusCode | 9 | approvalStatusCd | 9 | Direct | Approval status code transformation |
| startDateDuration | 10 | startDateDuration | 10 | Direct | Start date duration |
| endDateDuration | 11 | endDateDuration | 11 | Direct | End date duration |

#### Profile vs Map Field Name Comparison:

| D365 Profile Field | Oracle Fusion Profile Field | Change Type |
|--------------------|----------------------------|-------------|
| employeeNumber | personNumber | **Field Name Change** |
| absenceStatusCode | absenceStatusCd | **Field Name Abbreviation** |
| approvalStatusCode | approvalStatusCd | **Field Name Abbreviation** |

**Authority:** Map field names are AUTHORITATIVE for Oracle Fusion API request

**Transformation Logic:**
- All mappings are direct field-to-field (no scripting functions)
- No data type transformations required (numbers remain numbers, strings remain strings)
- Field name changes handled by map structure

### 6.3 Map: Oracle Fusion Leave Response Map (e4fd3f59-edb5-43a1-aeae-143b600a064e)

**Purpose:** Map Oracle Fusion success response to D365 response format

**From Profile:** 316175c7-0e45-4869-9ac6-5f9d69882a62 (Oracle Fusion Leave Response JSON Profile)  
**To Profile:** f4ca3a70-114a-4601-bad8-44a3eb20e2c0 (Leave D365 Response)

#### Field Mappings:

| From Field (Oracle Fusion) | From Key | To Field (D365 Response) | To Key | Mapping Type |
|----------------------------|----------|--------------------------|--------|--------------|
| personAbsenceEntryId | 3 | personAbsenceEntryId | 6 | Direct mapping from Oracle Fusion response |

#### Default Values:

| Target Field | Target Key | Default Value | Purpose |
|--------------|------------|---------------|---------|
| status | 4 | "success" | Indicates successful processing |
| message | 5 | "Data successfully sent to Oracle Fusion" | Success message |
| success | 7 | "true" | Boolean flag indicating success |

**Transformation Logic:**
- Only personAbsenceEntryId is extracted from Oracle Fusion response
- All other fields (status, message, success) are hardcoded default values
- This map is only used when HTTP status code is 20* (success path)

### 6.4 Map: Leave Error Map (f46b845a-7d75-41b5-b0ad-c41a6a8e9b12)

**Purpose:** Map error messages to D365 error response format

**From Profile:** 23d7a2e9-5cb0-4e9c-9e4b-3154834bad0d (Dummy FF Profile - not actually used)  
**To Profile:** f4ca3a70-114a-4601-bad8-44a3eb20e2c0 (Leave D365 Response)

#### Field Mappings:

| From Source | Function | To Field (D365 Response) | To Key | Mapping Type |
|-------------|----------|--------------------------|--------|--------------|
| process.DPP_ErrorMessage | PropertyGet (Function 1) | message | 5 | Process property retrieval |

#### Function Analysis:

**Function 1: Get Dynamic Process Property**
- **Type:** PropertyGet
- **Category:** ProcessProperty
- **Input 1:** Property Name = "DPP_ErrorMessage"
- **Input 2:** Default Value = (empty)
- **Output:** Result (key 3) → Mapped to message field

**Purpose:** Retrieves the error message that was captured by earlier shapes (shape19, shape39, or shape46) from Try/Catch or HTTP error response

#### Default Values:

| Target Field | Target Key | Default Value | Purpose |
|--------------|------------|---------------|---------|
| status | 4 | "failure" | Indicates failed processing |
| success | 7 | "false" | Boolean flag indicating failure |

**Transformation Logic:**
- Error message is retrieved from process property DPP_ErrorMessage
- Status and success fields are hardcoded to indicate failure
- personAbsenceEntryId field is not populated (only populated on success)

### 6.5 Critical Rules for Azure Migration

✅ **Map Field Names are AUTHORITATIVE**
- Use mapped field names (not profile technical names) in DTOs
- Example: Use "personNumber" (not "employeeNumber") when calling Oracle Fusion

✅ **NO SOAP Envelopes in This Process**
- This process uses REST API (JSON), not SOAP
- No namespace prefixes or SOAP-specific formatting required

✅ **Process Property Usage**
- Error map depends on process property DPP_ErrorMessage being set before mapping
- Azure implementation must ensure error capture logic sets this property equivalent

---

## 7. HTTP STATUS CODES AND RETURN PATH RESPONSES (Step 1e)

### 7.1 Return Path Analysis

#### Return Path 1: Success Response (shape35)

**Return Label:** "Success Response"  
**Return Shape ID:** shape35  
**HTTP Status Code:** 200  
**Path to Return:** shape1 → shape38 → shape17 (Try) → shape29 (Map) → shape8 (set URL) → shape49 (Notify) → shape33 (Oracle Fusion HTTP POST) → shape2 (Decision TRUE) → shape34 (Map) → shape35

**Decision Conditions Leading to Return:**
- Decision shape2: meta.base.applicationstatuscode wildcard matches "20*" → TRUE path

**Populated Response Fields:**

| Field Name | Field Path | Source | Populated By |
|------------|------------|--------|--------------|
| status | leaveResponse/Object/status | map default | map_e4fd3f59 (Oracle Fusion Leave Response Map) |
| message | leaveResponse/Object/message | map default | map_e4fd3f59 (Default: "Data successfully sent to Oracle Fusion") |
| personAbsenceEntryId | leaveResponse/Object/personAbsenceEntryId | operation_response | shape33 (Oracle Fusion HTTP response), mapped by map_e4fd3f59 |
| success | leaveResponse/Object/success | map default | map_e4fd3f59 (Default: "true") |

**Response JSON Example:**
```json
{
  "status": "success",
  "message": "Data successfully sent to Oracle Fusion",
  "personAbsenceEntryId": 300100561427173,
  "success": "true"
}
```

**Success Code:** N/A (implicit success)

---

#### Return Path 2: Error Response - HTTP Non-2xx (shape36)

**Return Label:** "Error Response"  
**Return Shape ID:** shape36  
**HTTP Status Code:** 400 (or original HTTP error code from Oracle Fusion)  
**Path to Return:** shape1 → ... → shape33 (Oracle Fusion HTTP POST) → shape2 (Decision FALSE) → shape44 (Check Content-Encoding Decision FALSE) → shape39 (extract error msg) → shape40 (Error Map) → shape36

**Decision Conditions Leading to Return:**
- Decision shape2: meta.base.applicationstatuscode does NOT match "20*" → FALSE path
- Decision shape44: dynamicdocument.DDP_RespHeader does NOT equal "gzip" → FALSE path

**Populated Response Fields:**

| Field Name | Field Path | Source | Populated By |
|------------|------------|--------|--------------|
| status | leaveResponse/Object/status | map default | map_f46b845a (Leave Error Map) |
| message | leaveResponse/Object/message | process_property | shape39 writes process.DPP_ErrorMessage from meta.base.applicationstatusmessage, then map_f46b845a retrieves it |
| success | leaveResponse/Object/success | map default | map_f46b845a (Default: "false") |

**Response JSON Example:**
```json
{
  "status": "failure",
  "message": "The operation can't be completed because the person number provided for the worker doesn't exist. (HRX-1548607)",
  "success": "false"
}
```

**Error Code:** HRX-* (Oracle Fusion error codes) or HTTP status message

---

#### Return Path 3: Error Response - HTTP Non-2xx with GZIP Compression (shape48)

**Return Label:** "Error Response"  
**Return Shape ID:** shape48  
**HTTP Status Code:** 400 (or original HTTP error code from Oracle Fusion)  
**Path to Return:** shape1 → ... → shape33 (Oracle Fusion HTTP POST) → shape2 (Decision FALSE) → shape44 (Check Content-Encoding Decision TRUE) → shape45 (Decompress GZIP) → shape46 (extract error msg) → shape47 (Error Map) → shape48

**Decision Conditions Leading to Return:**
- Decision shape2: meta.base.applicationstatuscode does NOT match "20*" → FALSE path
- Decision shape44: dynamicdocument.DDP_RespHeader equals "gzip" → TRUE path

**Populated Response Fields:**

| Field Name | Field Path | Source | Populated By |
|------------|------------|--------|--------------|
| status | leaveResponse/Object/status | map default | map_f46b845a (Leave Error Map) |
| message | leaveResponse/Object/message | process_property | shape46 writes process.DPP_ErrorMessage from meta.base.applicationstatusmessage (after GZIP decompression), then map_f46b845a retrieves it |
| success | leaveResponse/Object/success | map default | map_f46b845a (Default: "false") |

**Response JSON Example:**
```json
{
  "status": "failure",
  "message": "[Decompressed error message from Oracle Fusion]",
  "success": "false"
}
```

**Error Code:** Oracle Fusion error codes (after GZIP decompression)

---

#### Return Path 4: Try/Catch Error Response (shape43)

**Return Label:** "Error Response"  
**Return Shape ID:** shape43  
**HTTP Status Code:** 500 (Internal Server Error)  
**Path to Return:** shape1 → shape38 → shape17 (Catch) → shape20 (Branch Path 2) → shape41 (Error Map) → shape43

**Decision Conditions Leading to Return:**
- Try/Catch: Any exception thrown within the Try block (shape17) → Catch path

**Populated Response Fields:**

| Field Name | Field Path | Source | Populated By |
|------------|------------|--------|--------------|
| status | leaveResponse/Object/status | map default | map_f46b845a (Leave Error Map) |
| message | leaveResponse/Object/message | process_property | shape19 writes process.DPP_ErrorMessage from meta.base.catcherrorsmessage, then map_f46b845a retrieves it |
| success | leaveResponse/Object/success | map default | map_f46b845a (Default: "false") |

**Response JSON Example:**
```json
{
  "status": "failure",
  "message": "Connection timeout to Oracle Fusion",
  "success": "false"
}
```

**Error Code:** System error messages (connection errors, timeouts, mapping errors, etc.)

---

### 7.2 Downstream Operation HTTP Status Codes

#### Operation: Leave Oracle Fusion Create (shape33)

**Operation Name:** Leave Oracle Fusion Create  
**Operation Type:** HTTP POST  
**Endpoint:** {dynamicdocument.URL} (hcmRestApi/resources/11.13.18.05/absences)

**Expected Success Status Codes:**
- 200 (OK)
- 201 (Created)
- Pattern: Any 20* status code

**Error Status Codes:**
- 400 (Bad Request) - Invalid request data
- 401 (Unauthorized) - Authentication failure
- 404 (Not Found) - Invalid endpoint or resource
- 500 (Internal Server Error) - Oracle Fusion system error

**Error Handling Strategy:**
- Return errors to caller (returnErrors: true)
- Return responses to caller (returnResponses: true)
- Error response may be GZIP compressed (handled by shape44 decision)
- All errors are mapped to standard error response format before returning to D365

---

### 7.3 Email Notification Behavior (No Direct Return)

**Note:** Email subprocess (shape21) is triggered on Try/Catch error (Catch path, Branch Path 1) but does NOT return a response to the caller. After email is sent, the process terminates without returning to the main process flow. The error response to D365 is returned via Branch Path 2 (shape43).

**Email Subprocess Flow:**
- shape17 (Catch) → shape20 (Branch) → **Path 1:** shape19 (extract error) → shape21 (Email subprocess) → (subprocess ends, no return)
- shape17 (Catch) → shape20 (Branch) → **Path 2:** shape41 (Error Map) → shape43 (Return Error Response to D365)

**HTTP Status Code for Email Path:** N/A (subprocess does not return HTTP response to caller, only sends email)

---

## 8. DATA DEPENDENCY GRAPH (Step 4)

### 8.1 Property Dependencies

✅ **Data dependencies checked FIRST before following dragpoints**

#### 8.1.1 Dependencies from shape38 (Input_details)

**Shape:** shape38 (documentproperties - Input_details)  
**Properties Written:**
- process.DPP_Process_Name
- process.DPP_AtomName
- process.DPP_Payload
- process.DPP_ExecutionID
- process.DPP_File_Name
- process.DPP_Subject
- process.To_Email
- process.DPP_HasAttachment

**Dependent Shapes (Readers):**
- **Subprocess shape4** (Decision: Attachment_Check) reads process.DPP_HasAttachment
- **Subprocess shape11** (Message: Mail_Body) reads process.DPP_Process_Name, process.DPP_AtomName, process.DPP_ExecutionID, process.DPP_ErrorMessage
- **Subprocess shape23** (Message: Mail_Body) reads process.DPP_Process_Name, process.DPP_AtomName, process.DPP_ExecutionID, process.DPP_ErrorMessage
- **Subprocess shape15** (Message: payload) reads process.DPP_Payload
- **Subprocess shape6** (set_Mail_Properties) reads process.To_Email, process.DPP_Subject, process.DPP_MailBody, process.DPP_File_Name
- **Subprocess shape20** (set_Mail_Properties) reads process.To_Email, process.DPP_Subject, process.DPP_MailBody

**Dependency Chain:**
```
shape38 (writes properties) 
  → Subprocess shapes (read properties)
```

**Business Logic:** shape38 MUST execute before subprocess is called (shape21) to ensure all email-related properties are populated.

---

#### 8.1.2 Dependencies from shape8 (set URL)

**Shape:** shape8 (documentproperties - set URL)  
**Properties Written:**
- dynamicdocument.URL

**Dependent Shapes (Readers):**
- **shape33** (HTTP operation) reads dynamicdocument.URL to construct full Oracle Fusion REST API URL

**Dependency Chain:**
```
shape8 (writes dynamicdocument.URL) 
  → shape33 (reads dynamicdocument.URL for HTTP request)
```

**Business Logic:** shape8 MUST execute before shape33 to set the dynamic URL for the HTTP POST operation.

---

#### 8.1.3 Dependencies from shape19 (ErrorMsg)

**Shape:** shape19 (documentproperties - ErrorMsg)  
**Properties Written:**
- process.DPP_ErrorMessage (from meta.base.catcherrorsmessage)

**Dependent Shapes (Readers):**
- **Subprocess shape11** (Message) reads process.DPP_ErrorMessage for email body
- **Subprocess shape23** (Message) reads process.DPP_ErrorMessage for email body
- **map_f46b845a** (Leave Error Map) reads process.DPP_ErrorMessage for error response mapping

**Dependency Chain:**
```
shape19 (writes process.DPP_ErrorMessage from Try/Catch error) 
  → shape21 (subprocess - uses error message in email)
  → shape41 (error map - uses error message in response)
```

**Business Logic:** shape19 MUST execute before subprocess (shape21) and error mapping (shape41) to capture error message from Try/Catch.

---

#### 8.1.4 Dependencies from shape39 (error msg)

**Shape:** shape39 (documentproperties - error msg)  
**Properties Written:**
- process.DPP_ErrorMessage (from meta.base.applicationstatusmessage)

**Dependent Shapes (Readers):**
- **map_f46b845a** (Leave Error Map, via shape40) reads process.DPP_ErrorMessage

**Dependency Chain:**
```
shape39 (writes process.DPP_ErrorMessage from HTTP error) 
  → shape40 (error map - uses error message in response) 
  → shape36 (return error response)
```

**Business Logic:** shape39 MUST execute before shape40 to capture HTTP error message for error response.

---

#### 8.1.5 Dependencies from shape46 (error msg)

**Shape:** shape46 (documentproperties - error msg)  
**Properties Written:**
- process.DPP_ErrorMessage (from meta.base.applicationstatusmessage, after GZIP decompression)

**Dependent Shapes (Readers):**
- **map_f46b845a** (Leave Error Map, via shape47) reads process.DPP_ErrorMessage

**Dependency Chain:**
```
shape46 (writes process.DPP_ErrorMessage from decompressed HTTP error) 
  → shape47 (error map - uses error message in response) 
  → shape48 (return error response)
```

**Business Logic:** shape46 MUST execute before shape47 to capture decompressed HTTP error message for error response.

---

### 8.2 Dependency Summary Table

| Writer Shape | Property Written | Reader Shape(s) | Dependency Reason |
|--------------|------------------|-----------------|-------------------|
| shape38 | process.DPP_Process_Name | Subprocess shapes 11, 23 | Email body template requires process name |
| shape38 | process.DPP_AtomName | Subprocess shapes 11, 23 | Email body template requires atom name |
| shape38 | process.DPP_Payload | Subprocess shape15 | Email attachment content |
| shape38 | process.DPP_ExecutionID | Subprocess shapes 11, 23 | Email body template requires execution ID |
| shape38 | process.DPP_File_Name | Subprocess shapes 6, 20 | Email attachment filename |
| shape38 | process.DPP_Subject | Subprocess shapes 6, 20 | Email subject line |
| shape38 | process.To_Email | Subprocess shapes 6, 20 | Email recipient address |
| shape38 | process.DPP_HasAttachment | Subprocess shape4 | Decision: Send email with or without attachment |
| shape8 | dynamicdocument.URL | shape33 | HTTP POST operation requires dynamic URL |
| shape19 | process.DPP_ErrorMessage | Subprocess shapes 11, 23, map_f46b845a via shape41 | Error message for email and error response |
| shape39 | process.DPP_ErrorMessage | map_f46b845a via shape40 | HTTP error message for error response |
| shape46 | process.DPP_ErrorMessage | map_f46b845a via shape47 | Decompressed HTTP error message for error response |

---

### 8.3 Independent Operations (No Dependencies)

The following operations have NO data dependencies (do not read process properties):
- **shape29** (Map: Leave Create Map) - Uses only input document fields
- **shape33** (HTTP POST) - Uses only mapped document and dynamicdocument.URL (already set by shape8)
- **shape34** (Map: Oracle Fusion Leave Response Map) - Uses only HTTP response document

These operations execute in their natural flow order but are not blocked by property dependencies.

---

## 9. CONTROL FLOW GRAPH (Step 5)

### 9.1 Main Process Control Flow

✅ **Control flow extracted from dragpoints in JSON**

| From Shape | Shape Type | To Shape | Dragpoint Identifier | Dragpoint Text |
|------------|------------|----------|----------------------|----------------|
| shape1 (start) | start | shape38 | (default) | |
| shape38 (Input_details) | documentproperties | shape17 | (default) | |
| shape17 (Try/Catch) | catcherrors | shape29 | default | "Try" |
| shape17 (Try/Catch) | catcherrors | shape20 | error | "Catch" |
| shape29 (Map) | map | shape8 | (default) | |
| shape8 (set URL) | documentproperties | shape49 | (default) | |
| shape49 (Notify) | notify | shape33 | (default) | |
| shape33 (Oracle Fusion HTTP POST) | connectoraction | shape2 | (default) | |
| shape2 (HTTP Status 20 check) | decision | shape34 | true | "True" |
| shape2 (HTTP Status 20 check) | decision | shape44 | false | "False" |
| shape34 (Map success response) | map | shape35 | (default) | |
| shape35 (Success Response) | returndocuments | (none) | (terminal) | |
| shape44 (Check Response Content Type) | decision | shape45 | true | "True" |
| shape44 (Check Response Content Type) | decision | shape39 | false | "False" |
| shape45 (Decompress GZIP) | dataprocess | shape46 | (default) | |
| shape46 (error msg) | documentproperties | shape47 | (default) | |
| shape47 (Error Map) | map | shape48 | (default) | |
| shape48 (Error Response) | returndocuments | (none) | (terminal) | |
| shape39 (error msg) | documentproperties | shape40 | (default) | |
| shape40 (Error Map) | map | shape36 | (default) | |
| shape36 (Error Response) | returndocuments | (none) | (terminal) | |
| shape20 (Branch - Catch path) | branch | shape19 | 1 | "1" |
| shape20 (Branch - Catch path) | branch | shape41 | 2 | "2" |
| shape19 (ErrorMsg) | documentproperties | shape21 | (default) | |
| shape21 (Email subprocess) | processcall | (none) | (subprocess terminates) | |
| shape41 (Error Map) | map | shape43 | (default) | |
| shape43 (Error Response) | returndocuments | (none) | (terminal) | |

### 9.2 Subprocess Control Flow (Office 365 Email Subprocess)

| From Shape | Shape Type | To Shape | Dragpoint Identifier | Dragpoint Text |
|------------|------------|----------|----------------------|----------------|
| shape1 (start) | start | shape2 | (default) | |
| shape2 (Try/Catch) | catcherrors | shape4 | default | "Try" |
| shape2 (Try/Catch) | catcherrors | shape10 | error | "Catch" |
| shape4 (Attachment_Check decision) | decision | shape11 | true | "True" |
| shape4 (Attachment_Check decision) | decision | shape23 | false | "False" |
| shape11 (Mail_Body message) | message | shape14 | (default) | |
| shape14 (set_MailBody) | documentproperties | shape15 | (default) | |
| shape15 (payload message) | message | shape6 | (default) | |
| shape6 (set_Mail_Properties) | documentproperties | shape3 | (default) | |
| shape3 (Email w attachment) | connectoraction | shape5 | (default) | |
| shape5 (Stop continue) | stop | (none) | (subprocess success) | |
| shape23 (Mail_Body message) | message | shape22 | (default) | |
| shape22 (set_MailBody) | documentproperties | shape20 | (default) | |
| shape20 (set_Mail_Properties) | documentproperties | shape7 | (default) | |
| shape7 (Email w/o attachment) | connectoraction | shape9 | (default) | |
| shape9 (Stop continue) | stop | (none) | (subprocess success) | |
| shape10 (Exception) | exception | (none) | (subprocess throws exception) | |

### 9.3 Reverse Flow Mapping (Convergence Points)

**Convergence Point Analysis:**

**Main Process:**
- **No convergence points** - All decision branches lead to terminal return shapes without rejoining

**Subprocess:**
- **No convergence points** - Decision branches (with/without attachment) lead to separate email operations and stop shapes without rejoining

**Termination Points:**
- shape35 (Success Response)
- shape36 (Error Response - HTTP error, no GZIP)
- shape48 (Error Response - HTTP error, GZIP compressed)
- shape43 (Error Response - Try/Catch error)
- Subprocess shape5 (Stop continue - Email with attachment)
- Subprocess shape9 (Stop continue - Email without attachment)
- Subprocess shape10 (Exception - Email error)

---

## 10. DECISION SHAPE ANALYSIS (Step 7)

### 10.1 Decision Data Source Analysis

✅ **Decision data sources identified: YES**  
✅ **Decision types classified: YES**  
✅ **Execution order verified: YES**  
✅ **All decision paths traced: YES**  
✅ **Decision patterns identified: YES**  
✅ **Paths traced to termination: YES**

### 10.2 Decision Inventory

#### Decision 1: shape2 - HTTP Status 20 check

**Shape ID:** shape2  
**User Label:** "HTTP Status 20 check"  
**Comparison Type:** wildcard  
**Decision Values:**
- **Value 1:** meta.base.applicationstatuscode (track property - HTTP status code from Oracle Fusion response)
- **Value 2:** "20*" (static value - pattern matching any 2xx HTTP status code)

**Data Source:** TRACK_PROPERTY (meta.base.applicationstatuscode)  
**Data Source Type:** RESPONSE (HTTP status code from Oracle Fusion API)  
**Decision Type:** POST-OPERATION (checks response from shape33 HTTP operation)  
**Actual Execution Order:** 
```
shape33 (Oracle Fusion HTTP POST) 
  → HTTP Response (status code captured in track property)
  → shape2 (Decision: Check if status code matches 20*)
  → TRUE path: Success response mapping
  → FALSE path: Error handling
```

**TRUE Path:**
- **Destination:** shape34 (Map: Oracle Fusion Leave Response Map)
- **Termination:** shape35 (Return Documents - Success Response)
- **Termination Type:** return (terminal)

**FALSE Path:**
- **Destination:** shape44 (Decision: Check Response Content Type)
- **Termination:** shape36 (Return Documents - Error Response) OR shape48 (Return Documents - Error Response after GZIP decompression)
- **Termination Type:** return (terminal)

**Pattern:** Error Check (Success vs Failure based on HTTP status code)  
**Early Exit:** No (both paths eventually return, but different responses)  
**Convergence Point:** None (paths terminate at different return shapes)

**Business Logic:** This decision determines success or failure based on Oracle Fusion API response. HTTP 2xx codes indicate success, all other codes indicate failure. This is a POST-OPERATION decision because it checks the response from the downstream API call.

---

#### Decision 2: shape44 - Check Response Content Type

**Shape ID:** shape44  
**User Label:** "Check Response Content Type"  
**Comparison Type:** equals  
**Decision Values:**
- **Value 1:** dynamicdocument.DDP_RespHeader (document property - Content-Encoding HTTP response header)
- **Value 2:** "gzip" (static value)

**Data Source:** TRACK_PROPERTY (dynamicdocument.DDP_RespHeader, set by HTTP operation response header mapping)  
**Data Source Type:** RESPONSE (HTTP response header from Oracle Fusion API)  
**Decision Type:** POST-OPERATION (checks response header from shape33 HTTP operation)  
**Actual Execution Order:**
```
shape33 (Oracle Fusion HTTP POST) 
  → HTTP Response (Content-Encoding header captured in document property DDP_RespHeader)
  → shape2 (Decision: HTTP status check) → FALSE path (error)
  → shape44 (Decision: Check if Content-Encoding equals "gzip")
  → TRUE path: Decompress GZIP before error mapping
  → FALSE path: Error mapping without decompression
```

**TRUE Path (GZIP Compressed):**
- **Destination:** shape45 (Data Process: Custom Scripting - GZIP decompression)
- **Termination:** shape48 (Return Documents - Error Response)
- **Termination Type:** return (terminal)

**FALSE Path (Not GZIP Compressed):**
- **Destination:** shape39 (documentproperties: extract error message)
- **Termination:** shape36 (Return Documents - Error Response)
- **Termination Type:** return (terminal)

**Pattern:** Conditional Logic (Optional Processing - decompress if needed)  
**Early Exit:** No (both paths eventually return error responses)  
**Convergence Point:** None (paths terminate at different return shapes but with same logical outcome - error response)

**Business Logic:** Oracle Fusion API may return error responses with GZIP compression. This decision checks if the response is compressed and, if so, decompresses it before extracting the error message. This ensures error messages are readable regardless of compression.

---

#### Decision 3: shape4 (Subprocess) - Attachment_Check

**Shape ID:** shape4 (in subprocess a85945c5-3004-42b9-80b1-104f465cd1fb)  
**User Label:** "Attachment_Check"  
**Comparison Type:** equals  
**Decision Values:**
- **Value 1:** process.DPP_HasAttachment (process property)
- **Value 2:** "Y" (static value)

**Data Source:** PROCESS_PROPERTY (process.DPP_HasAttachment)  
**Data Source Type:** INPUT (set by main process shape38 from defined process property)  
**Decision Type:** PRE-FILTER (checks input configuration, not response data)  
**Actual Execution Order:**
```
Main process shape38 (writes process.DPP_HasAttachment = "Y")
  → Subprocess called (shape21)
  → Subprocess shape4 (Decision: Check if DPP_HasAttachment equals "Y")
  → TRUE path: Send email with attachment
  → FALSE path: Send email without attachment
```

**TRUE Path (Has Attachment = "Y"):**
- **Destination:** shape11 (Message: Mail_Body with attachment)
- **Termination:** shape5 (Stop continue - subprocess success)
- **Termination Type:** stop with continue=true (subprocess returns successfully)

**FALSE Path (Has Attachment ≠ "Y"):**
- **Destination:** shape23 (Message: Mail_Body without attachment)
- **Termination:** shape9 (Stop continue - subprocess success)
- **Termination Type:** stop with continue=true (subprocess returns successfully)

**Pattern:** Conditional Logic (Optional Processing - attachment handling)  
**Early Exit:** No (both paths successfully send email and return)  
**Convergence Point:** None (paths terminate at different stop shapes but with same logical outcome - email sent)

**Business Logic:** This decision determines whether to send email with or without attachment based on process configuration. In this specific process, attachment is always enabled (DPP_HasAttachment = "Y"), so email will include the error payload as attachment.

---

### 10.3 Decision Pattern Summary

| Decision | Pattern Type | Data Source | Execution Type | Early Exit |
|----------|--------------|-------------|----------------|------------|
| shape2 (HTTP Status 20 check) | Error Check (Success vs Failure) | RESPONSE (HTTP status code) | POST-OPERATION | No |
| shape44 (Check Response Content Type) | Conditional Logic (Optional Processing) | RESPONSE (HTTP header) | POST-OPERATION | No |
| shape4 (Attachment_Check) | Conditional Logic (Optional Processing) | INPUT (process property) | PRE-FILTER | No |

---

## 11. SUBPROCESS ANALYSIS (Step 7a)

### 11.1 Subprocess: (Sub) Office 365 Email

**Subprocess ID:** a85945c5-3004-42b9-80b1-104f465cd1fb  
**Subprocess Name:** (Sub) Office 365 Email  
**Called By:** shape21 (processcall) in main process

### 11.2 Subprocess Internal Flow

**Entry Point:** shape1 (start - passthrough action)

**Flow:**
```
START (shape1)
  → Try/Catch (shape2)
    → [TRY PATH]
      → Decision: Attachment_Check (shape4)
        → [TRUE PATH - With Attachment]
          → Build Email Body HTML (shape11)
          → Set MailBody Property (shape14)
          → Set Payload Message (shape15)
          → Set Mail Properties (shape6)
          → Send Email w Attachment (shape3)
          → Stop Continue (shape5) [SUCCESS RETURN]
        → [FALSE PATH - Without Attachment]
          → Build Email Body HTML (shape23)
          → Set MailBody Property (shape22)
          → Set Mail Properties (shape20)
          → Send Email W/O Attachment (shape7)
          → Stop Continue (shape9) [SUCCESS RETURN]
    → [CATCH PATH]
      → Exception: Throw Error (shape10) [ERROR RETURN]
```

### 11.3 Return Paths

#### Return Path 1: SUCCESS (Implicit - Stop Continue)

**Return Label:** (No explicit label - Stop with continue=true)  
**Return Shapes:** shape5 (with attachment path) OR shape9 (without attachment path)  
**Return Type:** Stop (continue=true)  
**Mapped To:** Main process continues normally after subprocess (no explicit return path mapping)  
**Condition:** Email sent successfully

**Note:** Since the main process shape21 (processcall) has `returnpaths: ""` (empty), the subprocess returns control to the main process after successful completion without any conditional routing.

#### Return Path 2: ERROR (Exception)

**Return Label:** (Exception thrown)  
**Return Shape:** shape10  
**Return Type:** Exception  
**Mapped To:** Main process may catch this exception if wrapped in Try/Catch (but shape21 is NOT within a Try/Catch in main process for this path)  
**Condition:** Any error during email sending (SMTP errors, authentication failures, etc.)

**Note:** Since shape21 is on the Catch path (Branch Path 1) and is NOT wrapped in a Try/Catch, if the subprocess throws an exception, it will propagate up and potentially terminate the main process execution. However, Branch Path 2 (shape41 → shape43) will still execute and return error response to D365.

### 11.4 Properties Written by Subprocess

| Property ID | Property Name | Written By Shape | Source |
|-------------|---------------|------------------|--------|
| process.DPP_MailBody | Dynamic Process Property - DPP_MailBody | shape14, shape22 | Current document (HTML email body built by shape11 or shape23) |
| connector.mail.fromAddress | Mail - From Address | shape6, shape20 | Defined Process Property: From_Email |
| connector.mail.toAddress | Mail - To Address | shape6, shape20 | Process Property: To_Email (passed from main process) |
| connector.mail.subject | Mail - Subject | shape6, shape20 | Concatenation: Environment + DPP_Subject |
| connector.mail.body | Mail - Body | shape6, shape20 | Process Property: DPP_MailBody |
| connector.mail.filename | Mail - File Name | shape6 | Process Property: DPP_File_Name (only for with-attachment path) |

### 11.5 Properties Read by Subprocess (from Main Process)

| Property ID | Property Name | Read By Shape | Usage |
|-------------|---------------|---------------|-------|
| process.DPP_HasAttachment | DPP_HasAttachment | shape4 (Decision) | Determines whether to send email with or without attachment |
| process.To_Email | To_Email | shape6, shape20 | Email recipient address |
| process.DPP_Subject | DPP_Subject | shape6, shape20 | Email subject line |
| process.DPP_Process_Name | DPP_Process_Name | shape11, shape23 (Message) | Used in email body HTML template |
| process.DPP_AtomName | DPP_AtomName | shape11, shape23 (Message) | Used in email body HTML template |
| process.DPP_ExecutionID | DPP_ExecutionID | shape11, shape23 (Message) | Used in email body HTML template |
| process.DPP_ErrorMessage | DPP_ErrorMessage | shape11, shape23 (Message) | Error details displayed in email body |
| process.DPP_Payload | DPP_Payload | shape15 (Message) | Original input payload included as email attachment |
| process.DPP_File_Name | DPP_File_Name | shape6 | Email attachment filename |

### 11.6 Email Body HTML Template

**Template Structure:**
```html
<!DOCTYPE html>
<html>
  <head>
    <meta http-equiv="Content-Type" content="text/html" charset="us-ascii" />
    <meta name="viewport" content="width=device-width" />
  <body>
    <pre>
      <font color="black">
        <h1 style="font-size:15px;"><u>Execution Details:</u></h1>
        <table border="1">
          <tr>
            <th scope="row"><b><font size="2">Process Name</font></b></th>
            <td>{1}</td> <!-- DPP_Process_Name -->
          </tr>
          <tr>
            <th scope="row"><b><font size="2">Environment</font></b></th>
            <td>{2}</td> <!-- DPP_AtomName -->
          </tr>
          <tr>
            <th scope="row"><b><font size="2">Execution ID</font></b></th>
            <td>{3}</td> <!-- DPP_ExecutionID -->
          </tr>
          <tr>
            <th scope="row"><b><font size="2">Error Details</font></b></th>
            <td>{4}</td> <!-- DPP_ErrorMessage -->
          </tr>
        </table>
      </font>
    </pre>
    <text>P.S: This is system generated email.</text>
    <tr></tr><tr></tr>
  </body>
</html>
```

**Template Parameters:**
1. {1} = process.DPP_Process_Name (e.g., "HCM_Leave Create")
2. {2} = process.DPP_AtomName (e.g., "Al Ghurair Dev Atom")
3. {3} = process.DPP_ExecutionID (e.g., "execution-abc123")
4. {4} = process.DPP_ErrorMessage (e.g., "Connection timeout to Oracle Fusion")

**Email Subject:** `{Environment} {DPP_Subject}`  
Example: "DEV Failure : Al Ghurair Dev Atom ( HCM_Leave Create ) has errors to report"

**Email Attachment (if DPP_HasAttachment = "Y"):**
- **Filename:** {DPP_File_Name} (e.g., "HCM_Leave Create2024-04-29T12:30:45.123Z.txt")
- **Content:** {DPP_Payload} (Original input JSON from D365)

---

## 12. BRANCH SHAPE ANALYSIS (Step 8)

### 12.1 Branch Shape: shape20 (Catch Error Path)

**Shape ID:** shape20  
**Shape Type:** branch  
**Location:** Main process - Catch path of Try/Catch (shape17)  
**Number of Paths:** 2

✅ **Classification completed: YES**  
✅ **Assumption check: NO (analyzed dependencies)**  
✅ **Properties extracted: YES**  
✅ **Dependency graph shown: YES**  
✅ **Topological sort applied: YES (SEQUENTIAL classification)**

### 12.2 Properties Analysis (Step 1)

#### Path 1 (Branch Identifier "1"):

**Start Shape:** shape19 (documentproperties - ErrorMsg)  
**Path Flow:** shape19 → shape21 (subprocess)

**Properties READ:**
- process.DPP_ErrorMessage (read by subprocess for email body)
- process.To_Email (read by subprocess)
- process.DPP_Subject (read by subprocess)
- process.DPP_Process_Name (read by subprocess)
- process.DPP_AtomName (read by subprocess)
- process.DPP_ExecutionID (read by subprocess)
- process.DPP_Payload (read by subprocess)
- process.DPP_File_Name (read by subprocess)
- process.DPP_HasAttachment (read by subprocess)

**Properties WRITTEN:**
- process.DPP_ErrorMessage (written by shape19 from meta.base.catcherrorsmessage)
- (Subprocess writes additional mail-related connector properties)

**JSON Proof:** 
- shape19 writes process.DPP_ErrorMessage (line 246 in process JSON: `propertyId: "process.DPP_ErrorMessage"`)
- Subprocess shapes read these properties (subprocess JSON lines 154-157, 269-276, 309-316, etc.)

#### Path 2 (Branch Identifier "2"):

**Start Shape:** shape41 (map - Leave Error Map)  
**Path Flow:** shape41 → shape43 (Return Documents - Error Response)

**Properties READ:**
- process.DPP_ErrorMessage (read by map_f46b845a function PropertyGet)

**Properties WRITTEN:**
- None (only reads existing property)

**JSON Proof:**
- shape41 uses map_f46b845a which has PropertyGet function (map JSON line 51: `type: "PropertyGet"`)
- PropertyGet reads "DPP_ErrorMessage" (map JSON line 63: `default: "DPP_ErrorMessage"`)

### 12.3 Dependency Graph (Step 2)

**Analysis:**
- Path 1 writes process.DPP_ErrorMessage (via shape19)
- Path 2 reads process.DPP_ErrorMessage (via shape41 map)

**Dependency:**
```
Path 1 (shape19) writes process.DPP_ErrorMessage
  ⬇ (dependency)
Path 2 (shape41) reads process.DPP_ErrorMessage
```

**Reasoning:** Path 2 reads process.DPP_ErrorMessage which Path 1 writes. However, since shape19 writes this property BEFORE the branch split, both paths can access it. There is NO dependency between the paths themselves - both paths read the same pre-existing property.

**Corrected Dependency Analysis:**
- shape19 (writes process.DPP_ErrorMessage) executes BEFORE branch shape20
- Both branch paths can independently read this property
- **No cross-path dependencies exist**

### 12.4 API Call Detection

**Path 1:**
- shape21 (subprocess) → Subprocess contains:
  - shape3 (Email w attachment) - connectoraction type "mail" ✅ **API CALL**
  - shape7 (Email W/O Attachment) - connectoraction type "mail" ✅ **API CALL**

**Path 2:**
- shape41 (map) - No API calls ❌
- shape43 (returndocuments) - No API calls ❌

**API Call Analysis:** Path 1 contains API calls (SMTP email sending operations)

### 12.5 Classification (Step 3)

🚨 **CRITICAL RULE: ALL API CALLS ARE SEQUENTIAL**

Since **Path 1 contains API calls** (email operations), the classification is:

**Classification:** ✅ **SEQUENTIAL**

**Reasoning:**
1. ❌ Path 1 contains API calls → ALWAYS SEQUENTIAL (per critical rule #7)
2. Even though there are no data dependencies between paths, API calls mandate sequential execution
3. Azure Functions migration: Email sending (Path 1) and error response return (Path 2) must execute sequentially

### 12.6 Topological Sort Order (Step 4)

**Execution Order:** Path 1 → Path 2

**Reasoning:**
1. Visual dragpoint order: Path 1 (identifier "1") precedes Path 2 (identifier "2")
2. No data dependencies between paths (shape19 writes property before branch)
3. API call presence in Path 1 mandates sequential execution
4. Logical order: Send email notification (Path 1), then return error response to caller (Path 2)

**JSON Proof:**
- Branch shape20 dragpoints (process JSON lines 294-316):
  - dragpoint identifier "1" → toShape: "shape19" (Path 1)
  - dragpoint identifier "2" → toShape: "shape41" (Path 2)

### 12.7 Path Termination (Step 5)

**Path 1 Termination:**
- **Terminal Shape:** Subprocess terminates (shape5 or shape9 in subprocess - Stop continue=true)
- **Termination Type:** Subprocess success return (no explicit return to main process)
- **Note:** Subprocess does not return control to main process in a way that continues main process flow

**Path 2 Termination:**
- **Terminal Shape:** shape43 (returndocuments - Error Response)
- **Termination Type:** Return Documents (terminal)

### 12.8 Convergence Points (Step 6)

**Convergence:** None

**Analysis:**
- Path 1 terminates in subprocess (email sent)
- Path 2 terminates in return documents (error response to D365)
- Paths do NOT rejoin - they execute independently and terminate separately

**Execution Continues From:** N/A (both paths terminate without convergence)

---

## 13. EXECUTION ORDER (Step 9)

### 13.1 Business Logic Flow (Step 0 - MANDATORY FIRST)

✅ **Business logic verified FIRST: YES**  
✅ **Operation analysis complete: YES**  
✅ **Business logic execution order identified: YES**  
✅ **Operation response analysis used: YES** (Reference: Section 5)  
✅ **Decision analysis used: YES** (Reference: Section 10)  
✅ **Dependency graph used: YES** (Reference: Section 8)  
✅ **Branch analysis used: YES** (Reference: Section 12)  
✅ **Property dependency verification: YES**  
✅ **Topological sort applied: YES**

#### Operation Analysis:

**Operation 1: Create Leave Oracle Fusion OP (shape1 operation 8f709c2b)**
- **Purpose:** Web Service Server - Receives leave data from D365
- **Outputs:** Input document (leave request JSON)
- **Dependent Operations:** All subsequent operations (entire process depends on input)
- **Business Flow:** This is the entry point - MUST execute FIRST

**Operation 2: Leave Oracle Fusion Create (shape33 operation 6e8920fd)**
- **Purpose:** HTTP POST - Sends leave data to Oracle Fusion HCM REST API
- **Outputs:** 
  - HTTP status code (meta.base.applicationstatuscode)
  - HTTP response message (meta.base.applicationstatusmessage)
  - HTTP response headers (dynamicdocument.DDP_RespHeader)
  - Oracle Fusion response body (may include personAbsenceEntryId)
- **Dependent Operations:** 
  - Decision shape2 (checks HTTP status code)
  - Decision shape44 (checks response encoding)
  - Success mapping shape34 (if HTTP 2xx)
  - Error mapping shapes 40, 47 (if HTTP non-2xx)
- **Business Flow:** HTTP POST MUST execute BEFORE success/error handling decisions

**Subprocess Operation: Email w Attachment / W/O Attachment (shape3/shape7 operations af07502a/15a72a21)**
- **Purpose:** Send error notification email via SMTP
- **Outputs:** None (side effect - email sent)
- **Dependent Operations:** None
- **Business Flow:** Email is sent as notification only, does not affect main process response

#### Business Flow Summary:

1. **Receive Input** (shape1) → Produces leave request data → MUST execute FIRST
2. **Set Properties** (shape38) → Produces error handling and email properties → MUST execute before error handling and subprocess
3. **Transform Data** (shape29) → Produces Oracle Fusion request format → MUST execute before HTTP POST
4. **Set URL** (shape8) → Produces dynamic URL property → MUST execute before HTTP POST
5. **HTTP POST** (shape33) → Produces HTTP response (status, headers, body) → MUST execute before success/error decisions
6. **Evaluate Response** (shape2) → Determines success or failure → MUST execute after HTTP POST
7. **Handle Success** (shape34, shape35) → Maps and returns success response → Only if HTTP 2xx
8. **Handle Error** (shape44, shape39/shape46, shape40/shape47, shape36/shape48) → Maps and returns error response → Only if HTTP non-2xx
9. **Handle Exception** (shape19, shape20, shape21/shape41, shape43) → Sends email and returns error response → Only if Try/Catch error

### 13.2 Actual Execution Order

✅ **Data dependencies checked FIRST: YES**  
✅ **Execution order derived from dependency graph: YES**

#### Main Process Execution Order:

```
START (shape1 - Web Service Server Trigger)
  ↓
SET PROPERTIES (shape38 - Input_details)
  → WRITES: process.DPP_Process_Name, process.DPP_AtomName, process.DPP_Payload,
            process.DPP_ExecutionID, process.DPP_File_Name, process.DPP_Subject,
            process.To_Email, process.DPP_HasAttachment
  ↓
TRY/CATCH (shape17)
  ↓
  [TRY PATH - Normal Flow]
    ↓
    MAP (shape29 - Leave Create Map)
      → Transforms: D365 input → Oracle Fusion request format
    ↓
    SET URL (shape8 - set URL)
      → WRITES: dynamicdocument.URL = "hcmRestApi/resources/11.13.18.05/absences"
    ↓
    NOTIFY (shape49 - Log payload for debugging)
    ↓
    HTTP POST (shape33 - Leave Oracle Fusion Create)
      → READS: dynamicdocument.URL (dependency on shape8)
      → POSTS: Transformed leave data to Oracle Fusion HCM
      → PRODUCES: HTTP status code, response message, response headers
    ↓
    DECISION (shape2 - HTTP Status 20 check)
      → READS: meta.base.applicationstatuscode (dependency on shape33 response)
      ↓
      [TRUE PATH - HTTP 2xx Success]
        ↓
        MAP (shape34 - Oracle Fusion Leave Response Map)
          → Transforms: Oracle Fusion response → D365 success response
          → Extracts: personAbsenceEntryId
          → Sets defaults: status="success", message="Data successfully sent...", success="true"
        ↓
        RETURN DOCUMENTS (shape35 - Success Response) [HTTP 200]
          → Response: {"status":"success","message":"...","personAbsenceEntryId":123456,"success":"true"}
      ↓
      [FALSE PATH - HTTP Non-2xx Error]
        ↓
        DECISION (shape44 - Check Response Content Type)
          → READS: dynamicdocument.DDP_RespHeader (dependency on shape33 response header)
          ↓
          [TRUE PATH - GZIP Compressed Response]
            ↓
            DATA PROCESS (shape45 - Decompress GZIP)
              → Decompresses: GZIP response body
            ↓
            SET PROPERTY (shape46 - error msg)
              → WRITES: process.DPP_ErrorMessage (from decompressed response)
            ↓
            MAP (shape47 - Leave Error Map)
              → READS: process.DPP_ErrorMessage (dependency on shape46)
              → Transforms: Error message → D365 error response
              → Sets defaults: status="failure", success="false"
            ↓
            RETURN DOCUMENTS (shape48 - Error Response) [HTTP 400]
              → Response: {"status":"failure","message":"[error]","success":"false"}
          ↓
          [FALSE PATH - Not GZIP Compressed]
            ↓
            SET PROPERTY (shape39 - error msg)
              → WRITES: process.DPP_ErrorMessage (from raw response)
            ↓
            MAP (shape40 - Leave Error Map)
              → READS: process.DPP_ErrorMessage (dependency on shape39)
              → Transforms: Error message → D365 error response
              → Sets defaults: status="failure", success="false"
            ↓
            RETURN DOCUMENTS (shape36 - Error Response) [HTTP 400]
              → Response: {"status":"failure","message":"[error]","success":"false"}
  ↓
  [CATCH PATH - Exception/Error Handling]
    ↓
    BRANCH (shape20 - Split into 2 sequential paths)
      ↓
      [BRANCH PATH 1 - SEQUENTIAL] (Send Email Notification)
        ↓
        SET PROPERTY (shape19 - ErrorMsg)
          → WRITES: process.DPP_ErrorMessage (from meta.base.catcherrorsmessage)
        ↓
        SUBPROCESS CALL (shape21 - Office 365 Email)
          → READS: process.DPP_ErrorMessage, process.To_Email, process.DPP_Subject,
                   process.DPP_Process_Name, process.DPP_AtomName, process.DPP_ExecutionID,
                   process.DPP_Payload, process.DPP_File_Name, process.DPP_HasAttachment
          → [SUBPROCESS INTERNAL FLOW - See Section 13.3]
          → Result: Email sent (success or exception)
      ↓
      [BRANCH PATH 2 - SEQUENTIAL] (Return Error Response to D365)
        ↓
        MAP (shape41 - Leave Error Map)
          → READS: process.DPP_ErrorMessage (dependency on shape19)
          → Transforms: Error message → D365 error response
          → Sets defaults: status="failure", success="false"
        ↓
        RETURN DOCUMENTS (shape43 - Error Response) [HTTP 500]
          → Response: {"status":"failure","message":"[error]","success":"false"}
```

#### Dependency Verification:

| Operation | Reads Property | Written By | Verified |
|-----------|----------------|------------|----------|
| shape33 (HTTP POST) | dynamicdocument.URL | shape8 (set URL) | ✅ shape8 executes before shape33 |
| shape2 (Decision) | meta.base.applicationstatuscode | shape33 (HTTP response) | ✅ shape33 executes before shape2 |
| shape44 (Decision) | dynamicdocument.DDP_RespHeader | shape33 (HTTP response) | ✅ shape33 executes before shape44 |
| shape40 (Error Map) | process.DPP_ErrorMessage | shape39 (error msg) | ✅ shape39 executes before shape40 |
| shape47 (Error Map) | process.DPP_ErrorMessage | shape46 (error msg) | ✅ shape46 executes before shape47 |
| shape41 (Error Map) | process.DPP_ErrorMessage | shape19 (ErrorMsg) | ✅ shape19 executes before shape41 |
| Subprocess (all reads) | Multiple properties | shape38 (Input_details) | ✅ shape38 executes before subprocess call |

**All property dependencies satisfied in execution order ✅**

### 13.3 Subprocess Execution Order (Office 365 Email)

```
SUBPROCESS START (shape1)
  ↓
TRY/CATCH (shape2)
  ↓
  [TRY PATH]
    ↓
    DECISION (shape4 - Attachment_Check)
      → READS: process.DPP_HasAttachment
      ↓
      [TRUE PATH - With Attachment (Default)]
        ↓
        MESSAGE (shape11 - Mail_Body)
          → Builds HTML email body with error details
          → READS: process.DPP_Process_Name, process.DPP_AtomName,
                   process.DPP_ExecutionID, process.DPP_ErrorMessage
        ↓
        SET PROPERTY (shape14 - set_MailBody)
          → WRITES: process.DPP_MailBody (from shape11 message)
        ↓
        MESSAGE (shape15 - payload)
          → READS: process.DPP_Payload (for attachment content)
        ↓
        SET PROPERTIES (shape6 - set_Mail_Properties)
          → WRITES: connector.mail.fromAddress (from defined property From_Email)
          → WRITES: connector.mail.toAddress (from process.To_Email)
          → WRITES: connector.mail.subject (from defined property Environment + process.DPP_Subject)
          → WRITES: connector.mail.body (from process.DPP_MailBody)
          → WRITES: connector.mail.filename (from process.DPP_File_Name)
        ↓
        EMAIL (shape3 - Email w Attachment)
          → Sends email via Office 365 SMTP with attachment
        ↓
        STOP CONTINUE (shape5 - Subprocess Success)
      ↓
      [FALSE PATH - Without Attachment]
        ↓
        MESSAGE (shape23 - Mail_Body)
          → Builds HTML email body with error details
          → READS: process.DPP_Process_Name, process.DPP_AtomName,
                   process.DPP_ExecutionID, process.DPP_ErrorMessage
        ↓
        SET PROPERTY (shape22 - set_MailBody)
          → WRITES: process.DPP_MailBody (from shape23 message)
        ↓
        SET PROPERTIES (shape20 - set_Mail_Properties)
          → WRITES: connector.mail.fromAddress, connector.mail.toAddress,
                   connector.mail.subject, connector.mail.body
        ↓
        EMAIL (shape7 - Email W/O Attachment)
          → Sends email via Office 365 SMTP without attachment
        ↓
        STOP CONTINUE (shape9 - Subprocess Success)
  ↓
  [CATCH PATH - Email Error]
    ↓
    EXCEPTION (shape10 - Throw exception to main process)
```

---

## 14. SEQUENCE DIAGRAM (Step 10)

**📋 NOTE:** Detailed request/response JSON examples are documented in Section 16 (Request/Response JSON Examples).

**Based on:**
- Data Dependency Graph (Section 8)
- Control Flow Graph (Section 9)
- Decision Analysis (Section 10)
- Branch Analysis (Section 12)
- Execution Order (Section 13)

```
START (Web Service Server - Receive Leave Request from D365)
  |
  ├─→ shape38: Set Input Properties (Document Properties)
  |   └─→ READS: Execution properties (Process Name, Atom Name, Execution ID), Current document (input payload)
  |   └─→ WRITES: process.DPP_Process_Name, process.DPP_AtomName, process.DPP_Payload,
  |                process.DPP_ExecutionID, process.DPP_File_Name, process.DPP_Subject,
  |                process.To_Email, process.DPP_HasAttachment
  |
  ├─→ TRY/CATCH (shape17)
  |   |
  |   ├─→ [TRY PATH - Normal Flow]
  |   |   |
  |   |   ├─→ shape29: Leave Create Map (Transform)
  |   |   |   └─→ Transform: D365 format → Oracle Fusion format
  |   |   |   └─→ Field mappings: employeeNumber→personNumber, absenceStatusCode→absenceStatusCd, etc.
  |   |   |
  |   |   ├─→ shape8: Set URL (Document Properties)
  |   |   |   └─→ WRITES: dynamicdocument.URL = "hcmRestApi/resources/11.13.18.05/absences"
  |   |   |
  |   |   ├─→ shape49: Notify (Log payload for debugging)
  |   |   |
  |   |   ├─→ shape33: Leave Oracle Fusion Create (Downstream) ⚡ HTTP POST
  |   |   |   └─→ READS: dynamicdocument.URL
  |   |   |   └─→ WRITES: meta.base.applicationstatuscode, meta.base.applicationstatusmessage,
  |   |   |                dynamicdocument.DDP_RespHeader (Content-Encoding)
  |   |   |   └─→ HTTP: [Expected: 200/201, Error: 400/401/404/500]
  |   |   |   └─→ Endpoint: POST https://iaaxey-dev3.fa.ocs.oraclecloud.com:443/hcmRestApi/resources/11.13.18.05/absences
  |   |   |   └─→ Auth: Basic Authentication
  |   |   |
  |   |   ├─→ shape2: Decision - HTTP Status 20 check
  |   |   |   └─→ READS: meta.base.applicationstatuscode
  |   |   |   └─→ Condition: applicationstatuscode wildcard matches "20*"?
  |   |   |   |
  |   |   |   ├─→ IF TRUE (HTTP 2xx - Success) → shape34: Oracle Fusion Leave Response Map
  |   |   |   |   └─→ READS: Oracle Fusion response (personAbsenceEntryId)
  |   |   |   |   └─→ Transform: Oracle Fusion response → D365 success response
  |   |   |   |   └─→ Default values: status="success", message="Data successfully sent to Oracle Fusion", success="true"
  |   |   |   |   |
  |   |   |   |   └─→ shape35: Return Documents [HTTP: 200] [SUCCESS] [EARLY EXIT]
  |   |   |   |       └─→ Response: {"status":"success","message":"...","personAbsenceEntryId":123456,"success":"true"}
  |   |   |   |
  |   |   |   └─→ IF FALSE (HTTP Non-2xx - Error) → shape44: Decision - Check Response Content Type
  |   |   |       └─→ READS: dynamicdocument.DDP_RespHeader
  |   |   |       └─→ Condition: DDP_RespHeader equals "gzip"?
  |   |   |       |
  |   |   |       ├─→ IF TRUE (GZIP Compressed) → shape45: Decompress GZIP (Custom Scripting)
  |   |   |       |   └─→ Groovy script: GZIPInputStream decompression
  |   |   |       |   |
  |   |   |       |   └─→ shape46: Set Error Message (Document Properties)
  |   |   |       |       └─→ WRITES: process.DPP_ErrorMessage (from decompressed applicationstatusmessage)
  |   |   |       |       |
  |   |   |       |       └─→ shape47: Leave Error Map
  |   |   |       |           └─→ READS: process.DPP_ErrorMessage (via PropertyGet function)
  |   |   |       |           └─→ Default values: status="failure", success="false"
  |   |   |       |           |
  |   |   |       |           └─→ shape48: Return Documents [HTTP: 400] [ERROR] [EARLY EXIT]
  |   |   |       |               └─→ Response: {"status":"failure","message":"[error]","success":"false"}
  |   |   |       |
  |   |   |       └─→ IF FALSE (Not GZIP) → shape39: Set Error Message (Document Properties)
  |   |   |           └─→ WRITES: process.DPP_ErrorMessage (from applicationstatusmessage)
  |   |   |           |
  |   |   |           └─→ shape40: Leave Error Map
  |   |   |               └─→ READS: process.DPP_ErrorMessage (via PropertyGet function)
  |   |   |               └─→ Default values: status="failure", success="false"
  |   |   |               |
  |   |   |               └─→ shape36: Return Documents [HTTP: 400] [ERROR] [EARLY EXIT]
  |   |   |                   └─→ Response: {"status":"failure","message":"[error]","success":"false"}
  |   |
  |   └─→ [CATCH PATH - Exception Handling]
  |       |
  |       └─→ shape20: Branch (2 Sequential Paths) ⚠️ SEQUENTIAL (API calls in Path 1)
  |           |
  |           ├─→ [BRANCH PATH 1 - Email Notification] (Sequential execution)
  |           |   |
  |           |   ├─→ shape19: Set Error Message (Document Properties)
  |           |   |   └─→ WRITES: process.DPP_ErrorMessage (from meta.base.catcherrorsmessage)
  |           |   |
  |           |   └─→ shape21: Call Subprocess - Office 365 Email ⚡ API CALL (SMTP)
  |           |       └─→ READS: process.DPP_ErrorMessage, process.To_Email, process.DPP_Subject,
  |           |                  process.DPP_Process_Name, process.DPP_AtomName, process.DPP_ExecutionID,
  |           |                  process.DPP_Payload, process.DPP_File_Name, process.DPP_HasAttachment
  |           |       |
  |           |       └─→ SUBPROCESS INTERNAL FLOW:
  |           |           |
  |           |           ├─→ Subprocess shape4: Decision - Attachment_Check
  |           |           |   └─→ READS: process.DPP_HasAttachment
  |           |           |   └─→ Condition: DPP_HasAttachment equals "Y"?
  |           |           |   |
  |           |           |   ├─→ IF TRUE (With Attachment) [DEFAULT PATH]
  |           |           |   |   |
  |           |           |   |   ├─→ Subprocess shape11: Build Email Body HTML
  |           |           |   |   |   └─→ READS: process.DPP_Process_Name, process.DPP_AtomName,
  |           |           |   |   |                process.DPP_ExecutionID, process.DPP_ErrorMessage
  |           |           |   |   |   └─→ Template: HTML table with execution details
  |           |           |   |   |
  |           |           |   |   ├─→ Subprocess shape14: Set MailBody Property
  |           |           |   |   |   └─→ WRITES: process.DPP_MailBody (from shape11 HTML)
  |           |           |   |   |
  |           |           |   |   ├─→ Subprocess shape15: Set Payload Message
  |           |           |   |   |   └─→ READS: process.DPP_Payload (for attachment)
  |           |           |   |   |
  |           |           |   |   ├─→ Subprocess shape6: Set Mail Properties
  |           |           |   |   |   └─→ WRITES: connector.mail.fromAddress, connector.mail.toAddress,
  |           |           |   |   |                connector.mail.subject, connector.mail.body, connector.mail.filename
  |           |           |   |   |
  |           |           |   |   ├─→ Subprocess shape3: Send Email w Attachment ⚡ SMTP API CALL
  |           |           |   |   |   └─→ Connection: Office 365 SMTP (smtp-mail.outlook.com:587)
  |           |           |   |   |   └─→ Attachment: process.DPP_Payload (original input JSON)
  |           |           |   |   |
  |           |           |   |   └─→ Subprocess shape5: Stop Continue [SUBPROCESS SUCCESS]
  |           |           |   |
  |           |           |   └─→ IF FALSE (Without Attachment)
  |           |           |       |
  |           |           |       ├─→ Subprocess shape23: Build Email Body HTML
  |           |           |       |   └─→ READS: process.DPP_Process_Name, process.DPP_AtomName,
  |           |           |       |                process.DPP_ExecutionID, process.DPP_ErrorMessage
  |           |           |       |
  |           |           |       ├─→ Subprocess shape22: Set MailBody Property
  |           |           |       |   └─→ WRITES: process.DPP_MailBody
  |           |           |       |
  |           |           |       ├─→ Subprocess shape20: Set Mail Properties
  |           |           |       |   └─→ WRITES: connector.mail properties
  |           |           |       |
  |           |           |       ├─→ Subprocess shape7: Send Email W/O Attachment ⚡ SMTP API CALL
  |           |           |       |   └─→ Connection: Office 365 SMTP
  |           |           |       |
  |           |           |       └─→ Subprocess shape9: Stop Continue [SUBPROCESS SUCCESS]
  |           |
  |           └─→ [BRANCH PATH 2 - Return Error Response] (Sequential execution after Path 1)
  |               |
  |               └─→ shape41: Leave Error Map
  |                   └─→ READS: process.DPP_ErrorMessage (from shape19)
  |                   └─→ Default values: status="failure", success="false"
  |                   |
  |                   └─→ shape43: Return Documents [HTTP: 500] [ERROR] [EARLY EXIT]
  |                       └─→ Response: {"status":"failure","message":"[error]","success":"false"}
```

### Notes:
- ⚡ **API CALL (Downstream):** Indicates external system call (Oracle Fusion REST API, SMTP email)
- ⚠️ **SEQUENTIAL:** Branch paths MUST execute sequentially (API calls in Path 1)
- **[EARLY EXIT]:** Indicates return path that terminates process execution
- All property dependencies are satisfied (verified in Section 13.2)

---

## 15. SYSTEM LAYER IDENTIFICATION

### 15.1 Third-Party Systems

| System Name | Type | Purpose | Connection Method |
|-------------|------|---------|-------------------|
| Oracle Fusion HCM | SaaS (Oracle Cloud) | Human Capital Management system - Manages employee leave/absence data | REST API (HTTPS) |
| Microsoft Office 365 Email | SaaS (Microsoft Cloud) | Email service for sending error notifications | SMTP (TLS) |
| Microsoft Dynamics 365 (D365) | SaaS (Microsoft Cloud) | Source system - Sends leave request data | Web Service (SOAP/REST) - Caller of this Boomi process |

### 15.2 System Layer Operations Required (for Azure Migration)

#### System Layer 1: Oracle Fusion HCM Leave Service

**Service Name:** OracleFusionHcmLeaveService  
**Purpose:** Create leave/absence entry in Oracle Fusion HCM  
**Operation:** CreateLeave

**Endpoint:**
- **Base URL:** https://iaaxey-dev3.fa.ocs.oraclecloud.com:443
- **Resource Path:** /hcmRestApi/resources/11.13.18.05/absences
- **HTTP Method:** POST
- **Content-Type:** application/json

**Authentication:**
- **Type:** Basic Authentication
- **Username:** INTEGRATION.USER@al-ghurair.com
- **Password:** [Encrypted - stored in connection]

**Request Schema:** (Section 3.3 - HCM Leave Create JSON Profile)
```typescript
{
  personNumber: number,
  absenceType: string,
  employer: string,
  startDate: string,  // YYYY-MM-DD
  endDate: string,    // YYYY-MM-DD
  absenceStatusCd: string,
  approvalStatusCd: string,
  startDateDuration: number,
  endDateDuration: number
}
```

**Response Schema:** (Section 5.1 - Oracle Fusion Leave Response JSON Profile)
- **Success Response (HTTP 200/201):** Contains personAbsenceEntryId and many other fields (see profile_316175c7)
- **Error Response (HTTP 400/4xx/5xx):** Error message (may be GZIP compressed)

**Error Handling:**
- Check HTTP status code (2xx = success, non-2xx = error)
- Check Content-Encoding header (if "gzip", decompress response)
- Return standardized error response to caller

---

#### System Layer 2: Office 365 Email Service

**Service Name:** Office365EmailService  
**Purpose:** Send error notification emails  
**Operation:** SendEmail

**Endpoint:**
- **SMTP Host:** smtp-mail.outlook.com
- **Port:** 587
- **Encryption:** TLS (STARTTLS)

**Authentication:**
- **Type:** SMTP Authentication
- **Username:** Boomi.Dev.failures@al-ghurair.com
- **Password:** [Encrypted - stored in connection]

**Request Schema:**
```typescript
{
  from: string,            // Sender email address
  to: string,              // Recipient email address(es)
  subject: string,         // Email subject
  body: string,            // HTML email body
  hasAttachment: boolean,  // Whether to include attachment
  attachmentFilename?: string,  // Attachment filename (if hasAttachment=true)
  attachmentContent?: string    // Attachment content (if hasAttachment=true)
}
```

**Email Body Template:** (See Section 11.6)
- HTML table with execution details (Process Name, Environment, Execution ID, Error Details)

**Error Handling:**
- Throw exception if email sending fails (SMTP errors, authentication failures)
- Subprocess may return exception to caller (but in this process, exception is not caught in main process)

---

### 15.3 System Layer Design Recommendations

#### Recommendation 1: Separate System Layer Functions

Create independent System Layer Azure Functions for each external system:

1. **OracleFusionHcmLeaveFunction** (System Layer)
   - Encapsulates Oracle Fusion HCM REST API
   - Handles authentication, request/response transformation, error handling
   - Reusable for other Oracle Fusion HCM integrations

2. **Office365EmailFunction** (System Layer)
   - Encapsulates Office 365 SMTP email sending
   - Handles authentication, HTML email composition, attachment handling
   - Reusable for other email notification scenarios

#### Recommendation 2: Process Layer Orchestration

Create a Process Layer Azure Function that orchestrates the business logic:

**HcmLeaveCreateProcessFunction** (Process Layer)
- Receives leave request from D365
- Transforms input to Oracle Fusion format
- Calls OracleFusionHcmLeaveFunction (System Layer)
- Evaluates response (success/error)
- On error: Calls Office365EmailFunction (System Layer) to send notification
- Returns standardized response to D365

#### Recommendation 3: Error Handling Strategy

- **Process Layer:** Orchestrates error handling logic (decisions, branching)
- **System Layer:** Throws exceptions with detailed error information
- **Process Layer:** Catches exceptions, transforms to standardized error response
- **System Layer (Email):** Send error notification as side effect (fire-and-forget)

---

## 16. REQUEST/RESPONSE JSON EXAMPLES

### 16.1 Process Layer Entry Point

#### Request JSON Example (from D365):

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

#### Response JSON Examples (to D365):

**Success Response (HTTP 200):**
```json
{
  "status": "success",
  "message": "Data successfully sent to Oracle Fusion",
  "personAbsenceEntryId": 300100561427173,
  "success": "true"
}
```

**Error Response - Oracle Fusion Validation Error (HTTP 400):**
```json
{
  "status": "failure",
  "message": "The operation can't be completed because the person number provided for the worker doesn't exist. (HRX-1548607)",
  "success": "false"
}
```

**Error Response - Connection Timeout (HTTP 500):**
```json
{
  "status": "failure",
  "message": "Connection timeout to Oracle Fusion HCM API",
  "success": "false"
}
```

---

### 16.2 System Layer: Oracle Fusion HCM Leave Service

#### Request JSON Example (to Oracle Fusion):

**Endpoint:** POST https://iaaxey-dev3.fa.ocs.oraclecloud.com:443/hcmRestApi/resources/11.13.18.05/absences

**Headers:**
```
Content-Type: application/json
Authorization: Basic [Base64 encoded credentials]
```

**Body:**
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

#### Response JSON Examples (from Oracle Fusion):

**Success Response (HTTP 200/201):**
```json
{
  "absenceCaseId": null,
  "absenceEntryBasicFlag": true,
  "absencePatternCd": null,
  "absenceStatusCd": "SUBMITTED",
  "absenceTypeId": 300100011070041,
  "absenceTypeReasonId": null,
  "agreementId": null,
  "approvalStatusCd": "APPROVED",
  "authStatusUpdateDate": null,
  "bandDtlId": null,
  "blockedLeaveCandidate": null,
  "certificationAuthFlag": null,
  "childEventTypeCd": null,
  "comments": null,
  "conditionStartDate": null,
  "confirmedDate": null,
  "consumedByAgreement": null,
  "createdBy": "INTEGRATION.USER@AL-GHURAIR.COM",
  "creationDate": "2024-04-30T06:59:41.062+00:00",
  "diseaseCode": null,
  "duration": 2,
  "employeeShiftFlag": false,
  "endDate": "2024-03-25",
  "endDateDuration": 1,
  "endDateTime": null,
  "endTime": null,
  "establishmentDate": null,
  "frequency": null,
  "initialReportById": null,
  "initialTimelyNotifyFlag": null,
  "lastUpdateDate": "2024-04-30T06:59:41.062+00:00",
  "lastUpdateLogin": null,
  "lastUpdatedBy": "INTEGRATION.USER@AL-GHURAIR.COM",
  "lateNotifyFlag": null,
  "legalEntityId": 300100000104019,
  "legislationCode": "AE",
  "legislativeDataGroupId": 300100000104036,
  "notificationDate": "2024-04-30",
  "objectVersionNumber": 1,
  "openEndedFlag": false,
  "overridden": null,
  "personAbsenceEntryId": 300100561427173,
  "periodOfIncapToWorkFlag": null,
  "periodOfServiceId": 300100561346123,
  "personId": 300100561346040,
  "plannedEndDate": null,
  "processingStatus": null,
  "projectId": null,
  "singleDayFlag": false,
  "source": "PER",
  "splCondition": null,
  "startDate": "2024-03-24",
  "startDateDuration": 1,
  "startDateTime": null,
  "startTime": null,
  "submittedDate": "2024-04-30",
  "timelinessOverrideDate": null,
  "unitOfMeasure": "D",
  "userMode": null,
  "personNumber": "9000604",
  "absenceType": "Sick Leave",
  "employer": "Al Ghurair Investment LLC",
  "absenceReason": null,
  "absenceDispStatus": "Submitted",
  "assignmentId": null,
  "dataSecurityPersonId": 300100561346040,
  "effectiveEndDate": null,
  "effectiveStartDate": "2024-03-24",
  "ObjectVersionNumber": 1,
  "agreementName": null,
  "paymentDetail": null,
  "assignmentName": null,
  "assignmentNumber": null,
  "unitOfMeasureMeaning": "Days",
  "formattedDuration": "2 D",
  "absenceDispStatusMeaning": "Submitted",
  "absenceUpdatableFlag": "Y",
  "ApprovalDatetime": null,
  "allowAssignmentSelectionFlag": true,
  "links": [
    {
      "rel": "self",
      "href": "https://iaaxey-dev3.fa.ocs.oraclecloud.com:443/hcmRestApi/resources/11.13.18.05/absences/300100561427173",
      "name": "absences",
      "kind": "item",
      "properties": {
        "changeIndicator": "ACED0005737200136A6176612E7574696C2E41727261794C6973747881D21D99C7619D03000149000473697A65787000000001770400000001737200116A6176612E6C616E672E496E746567657212E2A0A4F781873802000149000576616C7565787200106A6176612E6C616E672E4E756D62657286AC951D0B94E08B02000078700000000178"
      }
    },
    {
      "rel": "canonical",
      "href": "https://iaaxey-dev3.fa.ocs.oraclecloud.com:443/hcmRestApi/resources/11.13.18.05/absences/300100561427173",
      "name": "absences",
      "kind": "item"
    },
    {
      "rel": "child",
      "href": "https://iaaxey-dev3.fa.ocs.oraclecloud.com:443/hcmRestApi/resources/11.13.18.05/absences/300100561427173/child/absenceDetails",
      "name": "absenceDetails",
      "kind": "collection"
    }
  ]
}
```

**Error Response (HTTP 400) - Not GZIP Compressed:**
```json
{
  "title": "Bad Request",
  "detail": "The operation can't be completed because the person number provided for the worker doesn't exist. (HRX-1548607)",
  "o:errorCode": "HRX-1548607",
  "status": "400"
}
```

**Error Response (HTTP 400) - GZIP Compressed:**
```
Content-Encoding: gzip
[GZIP compressed binary data]

After decompression:
{
  "title": "Bad Request",
  "detail": "Invalid absence type provided for the legal employer. (HRX-1548608)",
  "o:errorCode": "HRX-1548608",
  "status": "400"
}
```

---

### 16.3 System Layer: Office 365 Email Service

#### Request Example (SMTP Email):

**Email Properties:**
```json
{
  "from": "Boomi.Dev.failures@al-ghurair.com",
  "to": "BoomiIntegrationTeam@al-ghurair.com",
  "subject": "DEV Failure : Al Ghurair Dev Atom ( HCM_Leave Create ) has errors to report",
  "bodyContentType": "text/html",
  "body": "<!DOCTYPE html><html><head>...</head><body>...</body></html>",
  "hasAttachment": true,
  "attachmentFilename": "HCM_Leave Create2024-04-30T06:59:45.123Z.txt",
  "attachmentContent": "{\"employeeNumber\":9000604,\"absenceType\":\"Sick Leave\",...}",
  "attachmentContentType": "text/plain",
  "attachmentDisposition": "attachment"
}
```

**Email Body HTML:**
```html
<!DOCTYPE html>
<html>
  <head>
    <meta http-equiv="Content-Type" content="text/html" charset="us-ascii" />
    <meta name="viewport" content="width=device-width" />
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
            <td>Al Ghurair Dev Atom</td>
          </tr>
          <tr>
            <th scope="row"><b><font size="2">Execution ID</font></b></th>
            <td>execution-abc123-def456</td>
          </tr>
          <tr>
            <th scope="row"><b><font size="2">Error Details</font></b></th>
            <td>Connection timeout to Oracle Fusion HCM API</td>
          </tr>
        </table>
      </font>
    </pre>
    <text>P.S: This is system generated email.</text>
    <tr></tr><tr></tr>
  </body>
</html>
```

#### Response:
- **Success:** SMTP 250 OK (email accepted for delivery)
- **Error:** SMTP error code (e.g., 535 Authentication failed, 550 Mailbox unavailable)

---

## 17. FUNCTION EXPOSURE DECISION TABLE

### 17.1 Process Layer Function

**Function Name:** HcmLeaveCreateProcessFunction  
**Layer:** Process Layer  
**Expose as API:** ✅ **YES**

**Rationale:**
- This is the main orchestration function that receives requests from D365
- Encapsulates business logic (transformation, error handling, decision logic)
- Acts as facade for downstream System Layer calls
- Provides standardized response format to D365

**API Specification:**
- **HTTP Method:** POST
- **Endpoint:** /api/hcm/leave/create
- **Content-Type:** application/json
- **Request Schema:** D365 Leave Create JSON (Section 3.5)
- **Response Schema:** Leave D365 Response JSON (Section 4.4)
- **Authentication:** API Key / OAuth2 (to be determined based on D365 integration requirements)

---

### 17.2 System Layer Function: Oracle Fusion HCM Leave Service

**Function Name:** OracleFusionHcmLeaveFunction  
**Layer:** System Layer  
**Expose as API:** ⚠️ **CONDITIONAL**

**Rationale:**
- Encapsulates Oracle Fusion HCM REST API operations
- Reusable for other Oracle Fusion HCM integrations (not just leave creation)
- **DECISION:**
  - ✅ **EXPOSE** if other processes/applications need direct access to Oracle Fusion HCM leave operations
  - ❌ **DO NOT EXPOSE** if this is only used by HcmLeaveCreateProcessFunction and no other consumers

**Recommendation:** **DO NOT EXPOSE** as public API initially. Keep as internal System Layer function called only by Process Layer. Can expose later if reusability requirements emerge.

**If Exposed:**
- **HTTP Method:** POST
- **Endpoint:** /api/system/oraclefusion/hcm/leave/create
- **Content-Type:** application/json
- **Request Schema:** HCM Leave Create JSON (Oracle Fusion format)
- **Response Schema:** Oracle Fusion Leave Response JSON
- **Authentication:** Internal API Key (not exposed to external consumers)

---

### 17.3 System Layer Function: Office 365 Email Service

**Function Name:** Office365EmailFunction  
**Layer:** System Layer  
**Expose as API:** ⚠️ **CONDITIONAL**

**Rationale:**
- Generic email sending service
- Highly reusable across many processes (error notifications, business alerts, reports)
- **DECISION:**
  - ✅ **EXPOSE** if organization wants a centralized email service for all integrations
  - ❌ **DO NOT EXPOSE** if email functionality is only for internal use by Process Layer functions

**Recommendation:** **EXPOSE** as internal shared service API. Many processes will need email notifications, so a centralized email service reduces duplication.

**If Exposed:**
- **HTTP Method:** POST
- **Endpoint:** /api/system/email/send
- **Content-Type:** application/json
- **Request Schema:** Email request schema (from, to, subject, body, attachments)
- **Response Schema:** Email send result (success/failure)
- **Authentication:** Internal API Key (not exposed to external consumers)

---

### 17.4 Function Explosion Prevention

**Total Functions Created:** 3
- 1 Process Layer function (public API)
- 2 System Layer functions (internal)

**Prevention Measures:**
- ✅ No unnecessary functions created
- ✅ System Layer functions are reusable (Oracle Fusion HCM, Email)
- ✅ Clear separation of concerns (orchestration vs system operations)
- ✅ Function exposure is justified by reusability and business requirements

**Future Reusability:**
- **Oracle Fusion HCM Leave Function:** Can be extended to support other leave operations (update, delete, query)
- **Office 365 Email Function:** Can be used by all processes requiring email notifications
- **Process Layer Function:** Single-purpose orchestration (not reusable, but that's correct for Process Layer)

---

## 18. VALIDATION CHECKLIST

### 18.1 Data Dependencies

- [x] All property WRITES identified (Section 2.1)
- [x] All property READS identified (Section 2.2)
- [x] Dependency graph built (Section 8)
- [x] Execution order satisfies all dependencies (Section 13.2 - All verified ✅)

### 18.2 Decision Analysis

- [x] ALL decision shapes inventoried (3 decisions total)
- [x] BOTH TRUE and FALSE paths traced to termination for each decision
- [x] Pattern type identified for each decision (Error Check, Conditional Logic)
- [x] Early exits identified and documented (All return paths are early exits)
- [x] Convergence points identified (None - all paths terminate separately)
- [x] Data source analysis complete (RESPONSE, PROCESS_PROPERTY)
- [x] Decision type classification complete (POST-OPERATION, PRE-FILTER)
- [x] Actual execution order verified (Operations execute before decisions)

### 18.3 Branch Analysis

- [x] Branch classified as SEQUENTIAL (API calls in Path 1)
- [x] API call detection performed (Subprocess contains SMTP email operations)
- [x] Property extraction complete for each path
- [x] Dependency graph built (shape19 writes, both paths can read)
- [x] Topological sort applied (Path 1 → Path 2 sequential order)
- [x] Each path traced to terminal point (Subprocess success, Return Documents)
- [x] Convergence points identified (None)
- [x] Execution continuation point determined (N/A - paths terminate separately)

### 18.4 Sequence Diagram

- [x] Format follows required structure (Section 14)
- [x] Each operation shows READS and WRITES
- [x] Decisions show both TRUE and FALSE paths
- [x] Early exits marked [EARLY EXIT]
- [x] Conditional execution marked where applicable
- [x] Subprocess internal flows documented
- [x] Subprocess return paths mapped to main process
- [x] API calls marked with ⚡ (Downstream)
- [x] Sequential execution marked with ⚠️ SEQUENTIAL

### 18.5 Subprocess Analysis

- [x] Subprocess analyzed (Office 365 Email subprocess)
- [x] Internal flow traced (Try/Catch → Decision → Email paths)
- [x] Return paths identified (Success: Stop continue, Error: Exception)
- [x] Return path labels mapped to main process (Implicit return, no explicit mapping)
- [x] Properties written by subprocess documented (Section 11.4)
- [x] Properties read by subprocess from main process documented (Section 11.5)

### 18.6 Edge Cases

- [x] Nested branches/decisions analyzed (Decision within Try/Catch)
- [x] Loops identified (None in this process)
- [x] Property chains traced (shape38 → subprocess, shape19 → error mapping)
- [x] Circular dependencies detected (None)
- [x] Try/Catch error paths documented (Catch path splits into email + error response)

### 18.7 Property Extraction Completeness

- [x] All property patterns searched (${}, %%, {} patterns)
- [x] Message parameters checked for process properties (Subprocess messages)
- [x] Operation headers/path parameters checked (dynamicdocument.URL for HTTP operation)
- [x] Decision track properties identified (meta.base.applicationstatuscode, meta.base.applicationstatusmessage)
- [x] Document properties that read other properties identified (Connector mail properties)

### 18.8 Input/Output Structure Analysis (CONTRACT VERIFICATION)

- [x] Entry point operation identified (Create Leave Oracle Fusion OP - shape1)
- [x] Request profile identified and loaded (D365 Leave Create JSON Profile)
- [x] Request profile structure analyzed (Single object, 9 fields)
- [x] Array vs single object detected (Single object - no array)
- [x] Request field paths documented (Root/Object/fieldName)
- [x] Request field mapping table generated (Section 3.3)
- [x] Response profile identified and loaded (Leave D365 Response)
- [x] Response profile structure analyzed (Single object, 4 fields)
- [x] Response field mapping table generated (Section 4.3)
- [x] Document processing behavior determined (Single document processing)
- [x] Input/Output structure documented in Phase 1 document (Sections 3-4)

### 18.9 HTTP Status Codes and Return Path Responses

- [x] All return paths documented with HTTP status codes (4 return paths total)
- [x] Response JSON examples provided for each return path (Success, Error paths)
- [x] Populated fields documented for each return path (Source and populated by)
- [x] Decision conditions leading to each return documented (HTTP status check, GZIP check)
- [x] Error codes and success codes documented (Oracle Fusion error codes)
- [x] Downstream operation HTTP status codes documented (Oracle Fusion API: 200/201 success, 400/401/404/500 error)
- [x] Error handling strategy documented (Return errors, check status code, decompress if GZIP)

### 18.10 Request/Response JSON Examples

- [x] Process Layer request JSON example provided (D365 format)
- [x] Process Layer response JSON examples provided (Success, Error responses)
- [x] System Layer request JSON examples provided (Oracle Fusion format)
- [x] System Layer response JSON examples provided (Oracle Fusion success, error, GZIP responses)
- [x] Email request example provided (SMTP parameters)

### 18.11 Map Analysis

- [x] ALL map files identified and loaded (3 maps total)
- [x] Field mappings extracted from each map (Direct mappings documented)
- [x] Profile vs map field name discrepancies documented (employeeNumber→personNumber, etc.)
- [x] Map field names marked as AUTHORITATIVE (Yes - use Oracle Fusion field names from map)
- [x] Scripting functions analyzed (None - all direct mappings)
- [x] Static values identified and documented (Default values in success/error maps)
- [x] Process property mappings documented (Error map uses PropertyGet function)
- [x] Map Analysis documented in Phase 1 document (Section 6)

### 18.12 Final Validation

✅ **ALL SELF-CHECK QUESTIONS ANSWERED WITH YES**

**Phase 1 Document Completeness:**
- [x] Section 1: Operations Inventory ✅
- [x] Section 2: Process Properties Analysis ✅
- [x] Section 3: Input Structure Analysis (Step 1a) ✅
- [x] Section 4: Response Structure Analysis (Step 1b) ✅
- [x] Section 5: Operation Response Analysis (Step 1c) ✅
- [x] Section 6: Map Analysis (Step 1d) ✅
- [x] Section 7: HTTP Status Codes and Return Path Responses (Step 1e) ✅
- [x] Section 8: Data Dependency Graph (Step 4) ✅
- [x] Section 9: Control Flow Graph (Step 5) ✅
- [x] Section 10: Decision Shape Analysis (Step 7) ✅
- [x] Section 11: Subprocess Analysis (Step 7a) ✅
- [x] Section 12: Branch Shape Analysis (Step 8) ✅
- [x] Section 13: Execution Order (Step 9) ✅
- [x] Section 14: Sequence Diagram (Step 10) ✅
- [x] Section 15: System Layer Identification ✅
- [x] Section 16: Request/Response JSON Examples ✅
- [x] Section 17: Function Exposure Decision Table ✅
- [x] Section 18: Validation Checklist ✅

**Extraction Complete:** ✅  
**Ready for Phase 2 Code Generation:** ✅

---

## END OF PHASE 1 EXTRACTION DOCUMENT

**Next Step:** Proceed to Phase 2 - Azure Functions Code Generation

**Summary:**
- Process Name: HCM Leave Create
- Purpose: Sync leave data from D365 to Oracle Fusion HCM
- Architecture: Process Layer (orchestration) + System Layer (Oracle Fusion HCM API, Email service)
- Complexity: Medium (Try/Catch, decisions, subprocess, GZIP handling)
- Total Functions Required: 3 (1 Process Layer, 2 System Layer)
- All mandatory extraction steps completed and validated ✅
