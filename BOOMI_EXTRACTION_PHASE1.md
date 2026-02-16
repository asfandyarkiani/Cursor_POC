# BOOMI EXTRACTION PHASE 1: HCM Leave Create Process Analysis

**Process Name:** HCM_Leave Create  
**Process ID:** ca69f858-785f-4565-ba1f-b4edc6cca05b  
**Description:** This Process will sync the Leave data between D365 and Oracle HCM  
**Analysis Date:** 2026-02-16  
**Analyst:** Cloud Agent

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

### 1.1 Main Process Operations

| Operation ID | Operation Name | Type | Sub-Type | Purpose |
|--------------|----------------|------|----------|---------|
| 8f709c2b-e63f-4d5f-9374-2932ed70415d | Create Leave Oracle Fusion OP | connector-action | wss | Entry point - Web Service Server Listen operation |
| 6e8920fd-af5a-430b-a1d9-9fde7ac29a12 | Leave Oracle Fusion Create | connector-action | http | HTTP POST to Oracle Fusion HCM API |
| af07502a-fafd-4976-a691-45d51a33b549 | Email w Attachment | connector-action | mail | Send email with attachment (subprocess) |
| 15a72a21-9b57-49a1-a8ed-d70367146644 | Email W/O Attachment | connector-action | mail | Send email without attachment (subprocess) |

### 1.2 Connections

| Connection ID | Connection Name | Type | URL/Host |
|---------------|-----------------|------|----------|
| aa1fcb29-d146-4425-9ea6-b9698090f60e | Oracle Fusion | http | https://iaaxey-dev3.fa.ocs.oraclecloud.com:443 |
| 00eae79b-2303-4215-8067-dcc299e42697 | Office 365 Email | mail | smtp-mail.outlook.com:587 |

### 1.3 Profiles

| Profile ID | Profile Name | Type | Usage |
|------------|--------------|------|-------|
| febfa3e1-f719-4ee8-ba57-cdae34137ab3 | D365 Leave Create JSON Profile | profile.json | Request profile for entry operation |
| a94fa205-c740-40a5-9fda-3d018611135a | HCM Leave Create JSON Profile | profile.json | Oracle Fusion request payload |
| 316175c7-0e45-4869-9ac6-5f9d69882a62 | Oracle Fusion Leave Response JSON Profile | profile.json | Oracle Fusion response |
| f4ca3a70-114a-4601-bad8-44a3eb20e2c0 | Leave D365 Response | profile.json | Response profile for entry operation |
| 23d7a2e9-5cb0-4e9c-9e4b-3154834bad0d | Dummy FF Profile | profile.flatfile | Dummy profile for error mapping |

### 1.4 Maps

| Map ID | Map Name | From Profile | To Profile | Purpose |
|--------|----------|--------------|------------|---------|
| c426b4d6-2aff-450e-b43b-59956c4dbc96 | Leave Create Map | D365 Leave Create | HCM Leave Create | Transform D365 to Oracle Fusion format |
| e4fd3f59-edb5-43a1-aeae-143b600a064e | Oracle Fusion Leave Response Map | Oracle Fusion Response | Leave D365 Response | Transform Oracle response to D365 response (success) |
| f46b845a-7d75-41b5-b0ad-c41a6a8e9b12 | Leave Error Map | Dummy FF Profile | Leave D365 Response | Transform error to D365 response |

---

## 2. Input Structure Analysis (Step 1a)

### ✅ Step 1a Completion Status: COMPLETE

**Entry Point Operation:** Create Leave Oracle Fusion OP (8f709c2b-e63f-4d5f-9374-2932ed70415d)

### 2.1 Request Profile Structure

- **Profile ID:** febfa3e1-f719-4ee8-ba57-cdae34137ab3
- **Profile Name:** D365 Leave Create JSON Profile
- **Profile Type:** profile.json
- **Root Structure:** Root/Object
- **Array Detection:** ❌ NO - Single object structure
- **Input Type:** singlejson
- **Document Processing Behavior:** Single document processing

### 2.2 Input Format

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

### 2.3 Document Processing Behavior

- **Boomi Processing:** Single document execution (inputType: singlejson)
- **Azure Function Requirement:** Accept single leave request object
- **Implementation Pattern:** Process single leave request, return response

### 2.4 Complete Field Inventory

| # | Field Name | Field Path | Data Type | Required | Mappable | Notes |
|---|------------|------------|-----------|----------|----------|-------|
| 1 | employeeNumber | Root/Object/employeeNumber | number | Yes | Yes | Employee identifier from D365 |
| 2 | absenceType | Root/Object/absenceType | character | Yes | Yes | Type of leave (e.g., Sick Leave) |
| 3 | employer | Root/Object/employer | character | Yes | Yes | Employer name |
| 4 | startDate | Root/Object/startDate | character | Yes | Yes | Leave start date (YYYY-MM-DD) |
| 5 | endDate | Root/Object/endDate | character | Yes | Yes | Leave end date (YYYY-MM-DD) |
| 6 | absenceStatusCode | Root/Object/absenceStatusCode | character | Yes | Yes | Status code (e.g., SUBMITTED) |
| 7 | approvalStatusCode | Root/Object/approvalStatusCode | character | Yes | Yes | Approval status (e.g., APPROVED) |
| 8 | startDateDuration | Root/Object/startDateDuration | number | Yes | Yes | Duration for start date (days) |
| 9 | endDateDuration | Root/Object/endDateDuration | number | Yes | Yes | Duration for end date (days) |

**Total Fields:** 9

---

## 3. Response Structure Analysis (Step 1b)

### ✅ Step 1b Completion Status: COMPLETE

**Response Profile:** Leave D365 Response (f4ca3a70-114a-4601-bad8-44a3eb20e2c0)

### 3.1 Response Profile Structure

- **Profile ID:** f4ca3a70-114a-4601-bad8-44a3eb20e2c0
- **Profile Name:** Leave D365 Response
- **Profile Type:** profile.json
- **Root Structure:** leaveResponse/Object
- **Array Detection:** ❌ NO - Single object response

### 3.2 Response Format

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

### 3.3 Response Field Inventory

| # | Field Name | Field Path | Data Type | Mappable | Source |
|---|------------|------------|-----------|----------|--------|
| 1 | status | leaveResponse/Object/status | character | Yes | Map default or error |
| 2 | message | leaveResponse/Object/message | character | Yes | Map default or error message |
| 3 | personAbsenceEntryId | leaveResponse/Object/personAbsenceEntryId | number | Yes | Oracle Fusion response |
| 4 | success | leaveResponse/Object/success | character | Yes | Map default (true/false) |

**Total Fields:** 4

---

## 4. Operation Response Analysis (Step 1c)

### ✅ Step 1c Completion Status: COMPLETE

### 4.1 Operation: Leave Oracle Fusion Create (6e8920fd-af5a-430b-a1d9-9fde7ac29a12)

**Operation Type:** HTTP POST  
**Response Profile:** Oracle Fusion Leave Response JSON Profile (316175c7-0e45-4869-9ac6-5f9d69882a62)

#### 4.1.1 Response Structure

The Oracle Fusion API returns a comprehensive leave entry object with 79+ fields. Key fields include:

| Field Name | Data Type | Purpose |
|------------|-----------|---------|
| personAbsenceEntryId | number | Unique identifier for the created leave entry |
| absenceStatusCd | character | Status code of the absence |
| approvalStatusCd | character | Approval status code |
| personNumber | character | Employee person number |
| absenceType | character | Type of absence |
| startDate | character | Start date of absence |
| endDate | character | End date of absence |
| startDateDuration | number | Duration for start date |
| endDateDuration | number | Duration for end date |

#### 4.1.2 Extracted Fields

**Extracted by shape34 (map: Oracle Fusion Leave Response Map):**
- **Field:** personAbsenceEntryId
- **Written to:** Response profile field (leaveResponse/Object/personAbsenceEntryId)
- **Purpose:** Return the created leave entry ID to D365

#### 4.1.3 Response Headers Captured

**Header:** Content-Encoding  
**Captured to:** dynamicdocument.DDP_RespHeader  
**Purpose:** Detect if response is gzip compressed

#### 4.1.4 Data Consumers

**shape2 (Decision: HTTP Status 20 check):**
- Reads: meta.base.applicationstatuscode (track property from HTTP response)
- Purpose: Check if HTTP status is 20* (success)

**shape44 (Decision: Check Response Content Type):**
- Reads: dynamicdocument.DDP_RespHeader
- Purpose: Check if response is gzip compressed

**shape39, shape46 (Document Properties: error msg):**
- Reads: meta.base.applicationstatusmessage (track property)
- Writes to: process.DPP_ErrorMessage
- Purpose: Capture error message on failure

#### 4.1.5 Business Logic Implications

1. **Operation must execute first** before any decision can check HTTP status
2. **HTTP status check (shape2)** determines success vs error path
3. **Content-Encoding check (shape44)** determines if decompression is needed
4. **Error message extraction** happens only on error paths

---

## 5. Map Analysis (Step 1d)

### ✅ Step 1d Completion Status: COMPLETE

### 5.1 Map Inventory

| Map ID | Map Name | Type | From Profile | To Profile |
|--------|----------|------|--------------|------------|
| c426b4d6-2aff-450e-b43b-59956c4dbc96 | Leave Create Map | Request Transform | D365 Leave Create | HCM Leave Create |
| e4fd3f59-edb5-43a1-aeae-143b600a064e | Oracle Fusion Leave Response Map | Response Transform | Oracle Fusion Response | Leave D365 Response |
| f46b845a-7d75-41b5-b0ad-c41a6a8e9b12 | Leave Error Map | Error Transform | Dummy FF Profile | Leave D365 Response |

### 5.2 Map: Leave Create Map (c426b4d6-2aff-450e-b43b-59956c4dbc96)

**Purpose:** Transform D365 leave request to Oracle Fusion HCM format

**From Profile:** febfa3e1-f719-4ee8-ba57-cdae34137ab3 (D365 Leave Create JSON Profile)  
**To Profile:** a94fa205-c740-40a5-9fda-3d018611135a (HCM Leave Create JSON Profile)  
**Type:** JSON to JSON transformation

#### 5.2.1 Field Mappings

| Source Field | Source Type | Target Field | Profile Match | Discrepancy? |
|--------------|-------------|--------------|---------------|--------------|
| employeeNumber | profile | personNumber | employeeNumber → personNumber | ❌ DIFFERENT |
| absenceType | profile | absenceType | absenceType | ✅ Match |
| employer | profile | employer | employer | ✅ Match |
| startDate | profile | startDate | startDate | ✅ Match |
| endDate | profile | endDate | endDate | ✅ Match |
| absenceStatusCode | profile | absenceStatusCd | absenceStatusCode → absenceStatusCd | ❌ DIFFERENT |
| approvalStatusCode | profile | approvalStatusCd | approvalStatusCode → approvalStatusCd | ❌ DIFFERENT |
| startDateDuration | profile | startDateDuration | startDateDuration | ✅ Match |
| endDateDuration | profile | endDateDuration | endDateDuration | ✅ Match |

#### 5.2.2 Profile vs Map Comparison

| D365 Field Name | Oracle Field Name (ACTUAL) | Authority | Use in Request |
|-----------------|----------------------------|-----------|----------------|
| employeeNumber | personNumber | ✅ MAP | personNumber |
| absenceStatusCode | absenceStatusCd | ✅ MAP | absenceStatusCd |
| approvalStatusCode | approvalStatusCd | ✅ MAP | approvalStatusCd |

**CRITICAL RULE:** Map field names are AUTHORITATIVE. The target profile uses shortened field names (e.g., `absenceStatusCd` instead of `absenceStatusCode`).

#### 5.2.3 Scripting Functions

**No scripting functions** - Direct field-to-field mapping only.

### 5.3 Map: Oracle Fusion Leave Response Map (e4fd3f59-edb5-43a1-aeae-143b600a064e)

**Purpose:** Transform Oracle Fusion response to D365 response format (success scenario)

**From Profile:** 316175c7-0e45-4869-9ac6-5f9d69882a62 (Oracle Fusion Leave Response)  
**To Profile:** f4ca3a70-114a-4601-bad8-44a3eb20e2c0 (Leave D365 Response)

#### 5.3.1 Field Mappings

| Source Field | Source Type | Target Field | Notes |
|--------------|-------------|--------------|-------|
| personAbsenceEntryId | profile | personAbsenceEntryId | ID of created leave entry |

#### 5.3.2 Default Values

| Target Field | Default Value | Purpose |
|--------------|---------------|---------|
| status | "success" | Indicate successful processing |
| message | "Data successfully sent to Oracle Fusion" | Success message |
| success | "true" | Boolean success flag |

### 5.4 Map: Leave Error Map (f46b845a-7d75-41b5-b0ad-c41a6a8e9b12)

**Purpose:** Transform error information to D365 response format (error scenario)

**From Profile:** 23d7a2e9-5cb0-4e9c-9e4b-3154834bad0d (Dummy FF Profile)  
**To Profile:** f4ca3a70-114a-4601-bad8-44a3eb20e2c0 (Leave D365 Response)

#### 5.4.1 Field Mappings

| Source Field | Source Type | Target Field | Notes |
|--------------|-------------|--------------|-------|
| DPP_ErrorMessage | function (PropertyGet) | message | Error message from process property |

#### 5.4.2 Default Values

| Target Field | Default Value | Purpose |
|--------------|---------------|---------|
| status | "failure" | Indicate failed processing |
| success | "false" | Boolean failure flag |

#### 5.4.3 Scripting Functions

**Function 1: Get Dynamic Process Property**
- **Type:** PropertyGet
- **Input:** Property Name = "DPP_ErrorMessage"
- **Output:** Result (error message)
- **Purpose:** Retrieve error message stored in process property

---

## 6. HTTP Status Codes and Return Path Responses (Step 1e)

### ✅ Step 1e Completion Status: COMPLETE

### 6.1 Return Path Inventory

| Return Shape ID | Return Label | HTTP Status | Decision Conditions | Error/Success Code |
|-----------------|--------------|-------------|---------------------|-------------------|
| shape35 | Success Response | 200 | HTTP Status 20* = TRUE | SUCCESS |
| shape36 | Error Response | 400 | HTTP Status 20* = FALSE, Content-Encoding != gzip | ERROR |
| shape43 | Error Response | 400 | Try/Catch error (Path 2) | ERROR |
| shape48 | Error Response | 400 | HTTP Status 20* = FALSE, Content-Encoding = gzip | ERROR |

### 6.2 Return Path 1: Success Response (shape35)

**Return Label:** Success Response  
**HTTP Status Code:** 200  
**Path:** shape1 → shape38 → shape17 (Try) → shape29 → shape8 → shape49 → shape33 → shape2 (TRUE) → shape34 → shape35

**Decision Conditions:**
- Decision shape2: meta.base.applicationstatuscode matches "20*" → TRUE path

**Populated Response Fields:**

| Field Name | Field Path | Source | Populated By |
|------------|------------|--------|--------------|
| status | leaveResponse/Object/status | map_default | shape34 (map e4fd3f59) |
| message | leaveResponse/Object/message | map_default | shape34 (map e4fd3f59) |
| personAbsenceEntryId | leaveResponse/Object/personAbsenceEntryId | operation_response | shape33 → shape34 (map e4fd3f59) |
| success | leaveResponse/Object/success | map_default | shape34 (map e4fd3f59) |

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

### 6.3 Return Path 2: Error Response - Try/Catch (shape43)

**Return Label:** Error Response  
**HTTP Status Code:** 400  
**Path:** shape1 → shape38 → shape17 (Catch) → shape20 (Branch Path 2) → shape41 → shape43

**Decision Conditions:**
- Try/Catch error occurred in shape17

**Populated Response Fields:**

| Field Name | Field Path | Source | Populated By |
|------------|------------|--------|--------------|
| status | leaveResponse/Object/status | map_default | shape41 (map f46b845a) |
| message | leaveResponse/Object/message | process_property | shape41 (map f46b845a) - DPP_ErrorMessage |
| success | leaveResponse/Object/success | map_default | shape41 (map f46b845a) |

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

### 6.4 Return Path 3: Error Response - HTTP Non-20*, Not Gzip (shape36)

**Return Label:** Error Response  
**HTTP Status Code:** 400  
**Path:** shape1 → shape38 → shape17 (Try) → shape29 → shape8 → shape49 → shape33 → shape2 (FALSE) → shape44 (FALSE) → shape39 → shape40 → shape36

**Decision Conditions:**
- Decision shape2: meta.base.applicationstatuscode does NOT match "20*" → FALSE path
- Decision shape44: dynamicdocument.DDP_RespHeader != "gzip" → FALSE path

**Populated Response Fields:**

| Field Name | Field Path | Source | Populated By |
|------------|------------|--------|--------------|
| status | leaveResponse/Object/status | map_default | shape40 (map f46b845a) |
| message | leaveResponse/Object/message | process_property | shape40 (map f46b845a) - DPP_ErrorMessage |
| success | leaveResponse/Object/success | map_default | shape40 (map f46b845a) |

**Response JSON Example:**

```json
{
  "leaveResponse": {
    "status": "failure",
    "message": "HTTP 400 Bad Request: [error details from Oracle]",
    "success": "false"
  }
}
```

### 6.5 Return Path 4: Error Response - HTTP Non-20*, Gzip (shape48)

**Return Label:** Error Response  
**HTTP Status Code:** 400  
**Path:** shape1 → shape38 → shape17 (Try) → shape29 → shape8 → shape49 → shape33 → shape2 (FALSE) → shape44 (TRUE) → shape45 → shape46 → shape47 → shape48

**Decision Conditions:**
- Decision shape2: meta.base.applicationstatuscode does NOT match "20*" → FALSE path
- Decision shape44: dynamicdocument.DDP_RespHeader = "gzip" → TRUE path

**Populated Response Fields:**

| Field Name | Field Path | Source | Populated By |
|------------|------------|--------|--------------|
| status | leaveResponse/Object/status | map_default | shape47 (map f46b845a) |
| message | leaveResponse/Object/message | process_property | shape47 (map f46b845a) - DPP_ErrorMessage (after decompression) |
| success | leaveResponse/Object/success | map_default | shape47 (map f46b845a) |

**Response JSON Example:**

```json
{
  "leaveResponse": {
    "status": "failure",
    "message": "HTTP 500 Internal Server Error: [decompressed error details]",
    "success": "false"
  }
}
```

### 6.6 Downstream Operations HTTP Status Codes

| Operation Name | Operation ID | Expected Success Codes | Error Codes | Error Handling |
|----------------|--------------|------------------------|-------------|----------------|
| Leave Oracle Fusion Create | 6e8920fd-af5a-430b-a1d9-9fde7ac29a12 | 200, 201 | 400, 401, 404, 500 | Return error response with error message |

**Error Handling Strategy:**
- **returnErrors: true** - Operation captures error responses instead of throwing exceptions
- **returnResponses: true** - Operation returns both success and error responses
- Error responses are checked via HTTP status code decision (shape2)
- Error messages are extracted from response body and returned to caller

---

## 7. Process Properties Analysis (Steps 2-3)

### ✅ Steps 2-3 Completion Status: COMPLETE

### 7.1 Property WRITES

| Property Name | Written By Shape(s) | Value Source | Purpose |
|---------------|---------------------|--------------|---------|
| process.DPP_Process_Name | shape38 | Execution Property (Process Name) | Store process name for error email |
| process.DPP_AtomName | shape38 | Execution Property (Atom Name) | Store atom name for error email |
| process.DPP_Payload | shape38 | Current document | Store input payload for error email |
| process.DPP_ExecutionID | shape38 | Execution Property (Execution Id) | Store execution ID for error email |
| process.DPP_File_Name | shape38 | Concatenation (Process Name + Timestamp + ".txt") | Generate filename for error email attachment |
| process.DPP_Subject | shape38 | Concatenation (Atom Name + " (" + Process Name + " ) has errors to report") | Generate email subject |
| process.To_Email | shape38 | Defined Property (PP_HCM_LeaveCreate_Properties.To_Email) | Email recipient for errors |
| process.DPP_HasAttachment | shape38 | Defined Property (PP_HCM_LeaveCreate_Properties.DPP_HasAttachment) | Flag for email attachment |
| process.DPP_ErrorMessage | shape19 | Track Property (meta.base.catcherrorsmessage) | Error message from Try/Catch |
| process.DPP_ErrorMessage | shape39 | Track Property (meta.base.applicationstatusmessage) | Error message from HTTP response (non-gzip) |
| process.DPP_ErrorMessage | shape46 | Current document (after decompression) | Error message from HTTP response (gzip) |
| dynamicdocument.URL | shape8 | Defined Property (PP_HCM_LeaveCreate_Properties.Resource_Path) | Oracle Fusion API resource path |

### 7.2 Property READS

| Property Name | Read By Shape(s) | Usage |
|---------------|------------------|-------|
| dynamicdocument.URL | shape33 (operation) | HTTP request URL path element |
| dynamicdocument.DDP_RespHeader | shape44 (decision) | Check if response is gzip compressed |
| process.DPP_ErrorMessage | map f46b845a (shapes 41, 40, 47) | Map error message to response |
| process.To_Email | Subprocess shape21 | Email recipient (passed to subprocess) |
| process.DPP_Subject | Subprocess shape21 | Email subject (passed to subprocess) |
| process.DPP_HasAttachment | Subprocess shape21 | Email attachment flag (passed to subprocess) |
| process.DPP_Process_Name | Subprocess shape21 | Process name (passed to subprocess) |
| process.DPP_AtomName | Subprocess shape21 | Atom name (passed to subprocess) |
| process.DPP_ExecutionID | Subprocess shape21 | Execution ID (passed to subprocess) |
| process.DPP_Payload | Subprocess shape21 | Input payload (passed to subprocess) |
| process.DPP_File_Name | Subprocess shape21 | Attachment filename (passed to subprocess) |

### 7.3 Defined Process Properties

#### 7.3.1 PP_HCM_LeaveCreate_Properties (e22c04db-7dfa-4b1b-9d88-77365b0fdb18)

| Property Key | Property Name | Default Value | Purpose |
|--------------|---------------|---------------|---------|
| c2d1a1e8-4bcb-435b-b1d2-bb10a209bcc0 | Resource_Path | hcmRestApi/resources/11.13.18.05/absences | Oracle Fusion API endpoint path |
| 71c90f6e-4f86-49f3-8aef-bae6668c73f9 | To_Email | BoomiIntegrationTeam@al-ghurair.com | Error notification recipient |
| a717867b-67f4-4a72-be2d-c99c73309fdb | DPP_HasAttachment | Y | Include attachment in error email |

#### 7.3.2 PP_Office365_Email (0feff13f-8a2c-438a-b3c7-1909e2a7f533) - Used by Subprocess

| Property Key | Property Name | Default Value | Purpose |
|--------------|---------------|---------------|---------|
| 804b6d40-eeee-4ac0-ab4b-94e1aca9f4b7 | From_Email | Boomi.Dev.failures@al-ghurair.com | Email sender address |
| 600acadb-ee02-4369-af85-ee70af380b6c | To_Email | [recipients] | Email recipient(s) |
| 2fa6ce9e-437a-44cc-b44f-5c7e61052f41 | HasAttachment | Y | Include attachment flag |
| 3ca9f307-cecb-4d1e-b9ec-007839509ed7 | EmailBody | [empty] | Email body content |
| d8fc7496-ec2c-4308-a4ca-e9f49c0c9500 | Environment | DEV Failure : | Environment prefix for subject |

---

## 8. Data Dependency Graph (Step 4)

### ✅ Step 4 Completion Status: COMPLETE

### 8.1 Dependency Graph

```
shape38 (Input_details) → shape17 (Try/Catch)
  ├─ WRITES: process.DPP_Process_Name
  ├─ WRITES: process.DPP_AtomName
  ├─ WRITES: process.DPP_Payload
  ├─ WRITES: process.DPP_ExecutionID
  ├─ WRITES: process.DPP_File_Name
  ├─ WRITES: process.DPP_Subject
  ├─ WRITES: process.To_Email
  └─ WRITES: process.DPP_HasAttachment

shape8 (set URL) → shape33 (HTTP operation)
  └─ WRITES: dynamicdocument.URL
  └─ READ BY: shape33 (operation path element)

shape33 (HTTP operation) → shape2 (Decision: HTTP Status 20 check)
  └─ PRODUCES: meta.base.applicationstatuscode (track property)
  └─ PRODUCES: dynamicdocument.DDP_RespHeader (response header)
  └─ READ BY: shape2 (decision)
  └─ READ BY: shape44 (decision)

shape19 (ErrorMsg) → shape21 (Subprocess)
  └─ WRITES: process.DPP_ErrorMessage
  └─ READ BY: Subprocess (email body)

shape39 (error msg) → shape40 (map) → shape36 (Return)
  └─ WRITES: process.DPP_ErrorMessage
  └─ READ BY: shape40 (map f46b845a)

shape46 (error msg) → shape47 (map) → shape48 (Return)
  └─ WRITES: process.DPP_ErrorMessage
  └─ READ BY: shape47 (map f46b845a)
```

### 8.2 Dependency Chains

**Chain 1: Input Properties Setup**
```
shape1 (START) → shape38 (Input_details) → shape17 (Try/Catch)
```
- shape38 MUST execute first to set up all error handling properties
- These properties are used by subprocess if error occurs

**Chain 2: HTTP Request Execution**
```
shape29 (map) → shape8 (set URL) → shape49 (notify) → shape33 (HTTP operation) → shape2 (Decision)
```
- shape8 MUST execute before shape33 (sets URL)
- shape33 MUST execute before shape2 (produces HTTP status)

**Chain 3: Error Path - Try/Catch**
```
shape17 (Catch) → shape20 (Branch) → shape19 (ErrorMsg) → shape21 (Subprocess)
```
- shape19 MUST execute before shape21 (sets error message)

**Chain 4: Error Path - HTTP Non-20*, Not Gzip**
```
shape2 (FALSE) → shape44 (FALSE) → shape39 (error msg) → shape40 (map) → shape36 (Return)
```
- shape39 MUST execute before shape40 (sets error message)

**Chain 5: Error Path - HTTP Non-20*, Gzip**
```
shape2 (FALSE) → shape44 (TRUE) → shape45 (decompress) → shape46 (error msg) → shape47 (map) → shape48 (Return)
```
- shape45 MUST execute before shape46 (decompresses response)
- shape46 MUST execute before shape47 (sets error message)

### 8.3 Independent Operations

**None** - All operations have dependencies or produce data consumed by subsequent operations.

---

## 9. Control Flow Graph (Step 5)

### ✅ Step 5 Completion Status: COMPLETE

### 9.1 Control Flow Map

```
shape1 (start) → shape38
shape38 (documentproperties) → shape17
shape17 (catcherrors):
  ├─ default → shape29 (Try path)
  └─ error → shape20 (Catch path)
shape29 (map) → shape8
shape8 (documentproperties) → shape49
shape49 (notify) → shape33
shape33 (connectoraction) → shape2
shape2 (decision):
  ├─ true → shape34 (HTTP 20* success)
  └─ false → shape44 (HTTP non-20* error)
shape34 (map) → shape35
shape35 (returndocuments) [TERMINAL]
shape44 (decision):
  ├─ true → shape45 (gzip response)
  └─ false → shape39 (non-gzip response)
shape45 (dataprocess) → shape46
shape46 (documentproperties) → shape47
shape47 (map) → shape48
shape48 (returndocuments) [TERMINAL]
shape39 (documentproperties) → shape40
shape40 (map) → shape36
shape36 (returndocuments) [TERMINAL]
shape20 (branch):
  ├─ 1 → shape19
  └─ 2 → shape41
shape19 (documentproperties) → shape21
shape21 (processcall) [TERMINAL - subprocess handles return]
shape41 (map) → shape43
shape43 (returndocuments) [TERMINAL]
```

### 9.2 Connection Summary

- **Total Shapes:** 20 (main process)
- **Total Connections:** 23
- **Decision Points:** 2 (shape2, shape44)
- **Branch Points:** 1 (shape20)
- **Terminal Points:** 4 (shape35, shape36, shape43, shape48)
- **Subprocess Calls:** 1 (shape21)

### 9.3 Reverse Flow Mapping (Step 6)

**Convergence Points:** None - All paths lead to distinct terminal points (return documents)

**Incoming Connections:**

| Shape ID | Incoming From | Connection Type |
|----------|---------------|-----------------|
| shape38 | shape1 | default |
| shape17 | shape38 | default |
| shape29 | shape17 | default (Try) |
| shape20 | shape17 | error (Catch) |
| shape8 | shape29 | default |
| shape49 | shape8 | default |
| shape33 | shape49 | default |
| shape2 | shape33 | default |
| shape34 | shape2 | true |
| shape44 | shape2 | false |
| shape35 | shape34 | default |
| shape45 | shape44 | true |
| shape39 | shape44 | false |
| shape46 | shape45 | default |
| shape47 | shape46 | default |
| shape48 | shape47 | default |
| shape40 | shape39 | default |
| shape36 | shape40 | default |
| shape19 | shape20 | 1 |
| shape41 | shape20 | 2 |
| shape21 | shape19 | default |
| shape43 | shape41 | default |

---

## 10. Decision Shape Analysis (Step 7)

### ✅ Step 7 Completion Status: COMPLETE

### 10.1 Self-Check Results

- ✅ **Decision data sources identified:** YES
- ✅ **Decision types classified:** YES
- ✅ **Execution order verified:** YES
- ✅ **All decision paths traced:** YES
- ✅ **Decision patterns identified:** YES
- ✅ **Paths traced to termination:** YES

### 10.2 Decision Inventory

#### Decision 1: shape2 - HTTP Status 20 check

**Shape ID:** shape2  
**User Label:** HTTP Status 20 check  
**Comparison:** wildcard  
**Value 1:** meta.base.applicationstatuscode (track property)  
**Value 2:** "20*" (static)

**Data Source:** TRACK_PROPERTY (from HTTP operation response)  
**Decision Type:** POST_OPERATION  
**Actual Execution Order:** Operation shape33 (HTTP) → Response → Decision shape2 → Route

**TRUE Path:**
- **Destination:** shape34 (map: Oracle Fusion Leave Response Map)
- **Termination:** shape35 (returndocuments: Success Response)
- **Type:** return

**FALSE Path:**
- **Destination:** shape44 (decision: Check Response Content Type)
- **Termination:** shape36 or shape48 (returndocuments: Error Response)
- **Type:** return (after nested decision)

**Pattern:** Error Check (Success vs Failure)  
**Convergence Point:** None  
**Early Exit:** Both paths terminate (return documents)

**Business Logic:**
- If HTTP status is 20* (200, 201, etc.) → Success path → Map response → Return success
- If HTTP status is NOT 20* (400, 401, 404, 500, etc.) → Error path → Check if gzip → Extract error → Return error

#### Decision 2: shape44 - Check Response Content Type

**Shape ID:** shape44  
**User Label:** Check Response Content Type  
**Comparison:** equals  
**Value 1:** dynamicdocument.DDP_RespHeader (document property)  
**Value 2:** "gzip" (static)

**Data Source:** PROCESS_PROPERTY (from HTTP operation response header)  
**Decision Type:** POST_OPERATION  
**Actual Execution Order:** Operation shape33 (HTTP) → Response Header Captured → Decision shape44 → Route

**TRUE Path:**
- **Destination:** shape45 (dataprocess: decompress gzip)
- **Termination:** shape48 (returndocuments: Error Response)
- **Type:** return

**FALSE Path:**
- **Destination:** shape39 (documentproperties: error msg)
- **Termination:** shape36 (returndocuments: Error Response)
- **Type:** return

**Pattern:** Conditional Logic (Optional Processing - decompress if needed)  
**Convergence Point:** None  
**Early Exit:** Both paths terminate (return documents)

**Business Logic:**
- If Content-Encoding header is "gzip" → Decompress response → Extract error → Return error
- If Content-Encoding header is NOT "gzip" → Extract error directly → Return error

### 10.3 Decision Patterns Summary

| Pattern | Decision(s) | Description |
|---------|-------------|-------------|
| Error Check (Success vs Failure) | shape2 | Check HTTP status to determine success or error path |
| Conditional Logic (Optional Processing) | shape44 | Decompress response only if gzip encoded |

---

## 11. Branch Shape Analysis (Step 8)

### ✅ Step 8 Completion Status: COMPLETE

### 11.1 Self-Check Results

- ✅ **Classification completed:** YES
- ✅ **Assumption check:** NO (analyzed dependencies)
- ✅ **Properties extracted:** YES
- ✅ **Dependency graph built:** YES
- ✅ **Topological sort applied:** YES (for sequential branch)

### 11.2 Branch: shape20 (Error Handling Branch)

**Shape ID:** shape20  
**Number of Paths:** 2  
**Location:** Catch path of Try/Catch (shape17)

#### 11.2.1 Path Analysis

**Path 1:** shape20 → shape19 (ErrorMsg) → shape21 (Subprocess: Email)  
**Path 2:** shape20 → shape41 (map: Error Map) → shape43 (Return: Error Response)

#### 11.2.2 Properties Analysis

**Path 1 Properties:**
- **READS:** None (uses properties already set by shape38)
- **WRITES:** process.DPP_ErrorMessage (shape19)

**Path 2 Properties:**
- **READS:** process.DPP_ErrorMessage (shape41 - map function)
- **WRITES:** None

#### 11.2.3 Dependency Graph

```
Path 1 (shape19) → Path 2 (shape41)
  └─ Path 1 WRITES process.DPP_ErrorMessage
  └─ Path 2 READS process.DPP_ErrorMessage
  └─ Therefore: Path 1 MUST execute BEFORE Path 2
```

#### 11.2.4 Classification

**Classification:** SEQUENTIAL

**Reasoning:**
1. Path 2 reads `process.DPP_ErrorMessage` which Path 1 writes
2. Data dependency exists: Path 2 depends on Path 1
3. Although no API calls, data dependency makes execution sequential

#### 11.2.5 Topological Sort Order

```
Execution Order: Path 1 → Path 2
  1. Path 1: shape19 → shape21 (subprocess)
  2. Path 2: shape41 → shape43 (return)
```

#### 11.2.6 Path Termination

- **Path 1:** shape21 (processcall) - Subprocess handles email sending, then process ends
- **Path 2:** shape43 (returndocuments) - Returns error response to caller

#### 11.2.7 Convergence Points

**None** - Each path terminates independently

#### 11.2.8 Execution Continuation

**Execution continues from:** None (both paths are terminal)

**Business Logic:**
- Path 1 sends error notification email with payload attachment
- Path 2 returns error response to caller
- Both paths execute sequentially to ensure error is logged and returned

---

## 12. Execution Order (Step 9)

### ✅ Step 9 Completion Status: COMPLETE

### 12.1 Self-Check Results

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

### 12.2 Business Logic Flow (Step 0)

#### 12.2.1 Operation Analysis

**Operation 1: Create Leave Oracle Fusion OP (shape1)**
- **Purpose:** Entry point - Receive leave request from D365
- **Produces:** Input document (leave request JSON)
- **Dependent Operations:** All subsequent operations
- **Business Flow:** Receives leave request → Triggers process execution

**Operation 2: Leave Oracle Fusion Create (shape33)**
- **Purpose:** Create leave entry in Oracle Fusion HCM via REST API
- **Produces:** 
  - HTTP response (Oracle leave entry object)
  - meta.base.applicationstatuscode (HTTP status code)
  - dynamicdocument.DDP_RespHeader (Content-Encoding header)
- **Dependent Operations:** 
  - shape2 (decision - checks HTTP status)
  - shape44 (decision - checks Content-Encoding)
  - shape34 (map - transforms success response)
  - shape39, shape46 (extract error message)
- **Business Flow:** Sends leave request to Oracle → Receives response → Status determines next action

**Operation 3: Office 365 Email (Subprocess shape21)**
- **Purpose:** Send error notification email to support team
- **Produces:** Email sent (no return value to main process)
- **Dependent Operations:** None (terminal operation)
- **Business Flow:** Sends error notification with payload attachment → Process ends

#### 12.2.2 Actual Business Flow

1. **Receive leave request from D365** (shape1)
2. **Set up error handling properties** (shape38) - MUST execute first for error scenarios
3. **Try/Catch wrapper** (shape17) - Protects against unexpected errors
4. **Transform request to Oracle format** (shape29)
5. **Set Oracle API URL** (shape8) - MUST execute before HTTP call
6. **Log request** (shape49)
7. **Call Oracle Fusion API** (shape33) - MUST execute before checking status
8. **Check HTTP status** (shape2) - Depends on shape33 response
   - **If success (20*):** Transform response → Return success
   - **If error (non-20*):** Check if gzip → Extract error → Return error
9. **On Try/Catch error:** Send email notification → Return error

### 12.3 Execution Order List

#### 12.3.1 Main Success Path

```
1. shape1 (start) - Entry point
2. shape38 (Input_details) - Set up error handling properties
3. shape17 (catcherrors) - Try/Catch wrapper
4. shape29 (map) - Transform D365 to Oracle format
5. shape8 (set URL) - Set Oracle API resource path
6. shape49 (notify) - Log request payload
7. shape33 (HTTP operation) - Call Oracle Fusion Create Leave API
8. shape2 (decision) - Check HTTP status = 20*
   └─ TRUE path:
9. shape34 (map) - Transform Oracle response to D365 format
10. shape35 (returndocuments) - Return success response [HTTP 200]
```

#### 12.3.2 Error Path 1: Try/Catch Error

```
1. shape1 (start)
2. shape38 (Input_details)
3. shape17 (catcherrors) - Error caught
   └─ Catch path:
4. shape20 (branch) - 2-path branch (SEQUENTIAL)
   └─ Path 1:
5. shape19 (ErrorMsg) - Extract error message from Try/Catch
6. shape21 (processcall) - Send error notification email [SUBPROCESS]
   └─ Path 2:
7. shape41 (map) - Map error to D365 response format
8. shape43 (returndocuments) - Return error response [HTTP 400]
```

#### 12.3.3 Error Path 2: HTTP Non-20*, Not Gzip

```
1-7. [Same as success path through shape33]
8. shape2 (decision) - Check HTTP status = 20*
   └─ FALSE path:
9. shape44 (decision) - Check Content-Encoding = gzip
   └─ FALSE path:
10. shape39 (error msg) - Extract error message from HTTP response
11. shape40 (map) - Map error to D365 response format
12. shape36 (returndocuments) - Return error response [HTTP 400]
```

#### 12.3.4 Error Path 3: HTTP Non-20*, Gzip

```
1-7. [Same as success path through shape33]
8. shape2 (decision) - Check HTTP status = 20*
   └─ FALSE path:
9. shape44 (decision) - Check Content-Encoding = gzip
   └─ TRUE path:
10. shape45 (dataprocess) - Decompress gzip response
11. shape46 (error msg) - Extract error message from decompressed response
12. shape47 (map) - Map error to D365 response format
13. shape48 (returndocuments) - Return error response [HTTP 400]
```

### 12.4 Dependency Verification

**Dependency Chain 1: URL Setup**
```
shape8 (set URL) → shape33 (HTTP operation)
  └─ shape8 WRITES dynamicdocument.URL
  └─ shape33 READS dynamicdocument.URL
  └─ ✅ Verified: shape8 executes before shape33
```

**Dependency Chain 2: HTTP Status Check**
```
shape33 (HTTP operation) → shape2 (decision)
  └─ shape33 PRODUCES meta.base.applicationstatuscode
  └─ shape2 READS meta.base.applicationstatuscode
  └─ ✅ Verified: shape33 executes before shape2
```

**Dependency Chain 3: Content-Encoding Check**
```
shape33 (HTTP operation) → shape44 (decision)
  └─ shape33 PRODUCES dynamicdocument.DDP_RespHeader
  └─ shape44 READS dynamicdocument.DDP_RespHeader
  └─ ✅ Verified: shape33 executes before shape44
```

**Dependency Chain 4: Error Message (Try/Catch)**
```
shape19 (ErrorMsg) → shape41 (map)
  └─ shape19 WRITES process.DPP_ErrorMessage
  └─ shape41 READS process.DPP_ErrorMessage (via map function)
  └─ ✅ Verified: shape19 executes before shape41 (sequential branch)
```

**Dependency Chain 5: Error Message (HTTP Error, Not Gzip)**
```
shape39 (error msg) → shape40 (map)
  └─ shape39 WRITES process.DPP_ErrorMessage
  └─ shape40 READS process.DPP_ErrorMessage (via map function)
  └─ ✅ Verified: shape39 executes before shape40
```

**Dependency Chain 6: Error Message (HTTP Error, Gzip)**
```
shape45 (decompress) → shape46 (error msg) → shape47 (map)
  └─ shape45 decompresses response
  └─ shape46 WRITES process.DPP_ErrorMessage
  └─ shape47 READS process.DPP_ErrorMessage (via map function)
  └─ ✅ Verified: shape45 → shape46 → shape47 execute in order
```

### 12.5 Branch Execution Order

**Branch shape20 (Error Handling):**
- **Classification:** SEQUENTIAL (from Step 8)
- **Execution Order:** Path 1 → Path 2
  1. Path 1: shape19 → shape21 (send email)
  2. Path 2: shape41 → shape43 (return error)
- **Reasoning:** Path 2 depends on process.DPP_ErrorMessage written by Path 1

---

## 13. Sequence Diagram (Step 10)

### ✅ Step 10 Completion Status: COMPLETE

**📋 NOTE:** This sequence diagram is based on:
- **Step 4:** Data Dependency Graph
- **Step 5:** Control Flow Graph
- **Step 7:** Decision Shape Analysis
- **Step 8:** Branch Shape Analysis
- **Step 9:** Execution Order

**📋 NOTE:** Detailed request/response JSON examples are documented in:
- **Section 6:** HTTP Status Codes and Return Path Responses
- **Section 2:** Input Structure Analysis (request format)
- **Section 3:** Response Structure Analysis (response format)

### 13.1 Main Success Flow

```
START (shape1: Web Service Server Listen)
 |
 ├─→ shape38: Input_details (Document Properties)
 |   └─→ WRITES: process.DPP_Process_Name, process.DPP_AtomName, process.DPP_Payload,
 |                process.DPP_ExecutionID, process.DPP_File_Name, process.DPP_Subject,
 |                process.To_Email, process.DPP_HasAttachment
 |
 ├─→ shape17: Try/Catch (Catch Errors)
 |   ├─→ TRY PATH (default):
 |   |
 |   ├─→ shape29: Map - Leave Create Map (Transform)
 |   |   └─→ READS: Input document (D365 format)
 |   |   └─→ WRITES: Transformed document (Oracle format)
 |   |
 |   ├─→ shape8: set URL (Document Properties)
 |   |   └─→ WRITES: dynamicdocument.URL = "hcmRestApi/resources/11.13.18.05/absences"
 |   |
 |   ├─→ shape49: Notify (Log)
 |   |   └─→ READS: Current document (Oracle format)
 |   |   └─→ LOG: INFO level
 |   |
 |   ├─→ shape33: Leave Oracle Fusion Create (Downstream HTTP POST)
 |   |   └─→ READS: dynamicdocument.URL
 |   |   └─→ WRITES: HTTP response, meta.base.applicationstatuscode, 
 |   |               dynamicdocument.DDP_RespHeader (Content-Encoding)
 |   |   └─→ HTTP: [Expected: 200/201, Error: 400/401/404/500]
 |   |   └─→ Connection: Oracle Fusion (https://iaaxey-dev3.fa.ocs.oraclecloud.com:443)
 |   |   └─→ Auth: Basic Authentication
 |   |
 |   ├─→ shape2: Decision - HTTP Status 20 check
 |   |   └─→ READS: meta.base.applicationstatuscode
 |   |   └─→ CONDITION: applicationstatuscode matches "20*"?
 |   |
 |   |   ├─→ IF TRUE (HTTP 20* - Success):
 |   |   |
 |   |   ├─→ shape34: Map - Oracle Fusion Leave Response Map (Transform)
 |   |   |   └─→ READS: Oracle response (personAbsenceEntryId)
 |   |   |   └─→ WRITES: D365 response format
 |   |   |   └─→ DEFAULTS: status="success", message="Data successfully sent to Oracle Fusion", 
 |   |   |                 success="true"
 |   |   |
 |   |   └─→ shape35: Return Documents [HTTP: 200] [SUCCESS]
 |   |       └─→ Response: { "leaveResponse": { "status": "success", 
 |   |                       "message": "Data successfully sent to Oracle Fusion",
 |   |                       "personAbsenceEntryId": 300000123456789, "success": "true" } }
 |   |
 |   |   └─→ IF FALSE (HTTP non-20* - Error):
 |   |   |
 |   |   ├─→ shape44: Decision - Check Response Content Type
 |   |   |   └─→ READS: dynamicdocument.DDP_RespHeader
 |   |   |   └─→ CONDITION: DDP_RespHeader equals "gzip"?
 |   |   |
 |   |   |   ├─→ IF TRUE (Gzip Compressed):
 |   |   |   |
 |   |   |   ├─→ shape45: Custom Scripting (Decompress)
 |   |   |   |   └─→ READS: Current document (gzip compressed)
 |   |   |   |   └─→ WRITES: Decompressed document
 |   |   |   |   └─→ Script: GZIPInputStream decompression (Groovy)
 |   |   |   |
 |   |   |   ├─→ shape46: error msg (Document Properties)
 |   |   |   |   └─→ READS: Current document (decompressed error)
 |   |   |   |   └─→ WRITES: process.DPP_ErrorMessage
 |   |   |   |
 |   |   |   ├─→ shape47: Map - Leave Error Map (Transform)
 |   |   |   |   └─→ READS: process.DPP_ErrorMessage (via PropertyGet function)
 |   |   |   |   └─→ WRITES: D365 error response format
 |   |   |   |   └─→ DEFAULTS: status="failure", success="false"
 |   |   |   |
 |   |   |   └─→ shape48: Return Documents [HTTP: 400] [ERROR] [EARLY EXIT]
 |   |   |       └─→ Response: { "leaveResponse": { "status": "failure",
 |   |   |                       "message": "[decompressed error details]", "success": "false" } }
 |   |   |
 |   |   |   └─→ IF FALSE (Not Gzip):
 |   |   |   |
 |   |   |   ├─→ shape39: error msg (Document Properties)
 |   |   |   |   └─→ READS: meta.base.applicationstatusmessage
 |   |   |   |   └─→ WRITES: process.DPP_ErrorMessage
 |   |   |   |
 |   |   |   ├─→ shape40: Map - Leave Error Map (Transform)
 |   |   |   |   └─→ READS: process.DPP_ErrorMessage (via PropertyGet function)
 |   |   |   |   └─→ WRITES: D365 error response format
 |   |   |   |   └─→ DEFAULTS: status="failure", success="false"
 |   |   |   |
 |   |   |   └─→ shape36: Return Documents [HTTP: 400] [ERROR] [EARLY EXIT]
 |   |   |       └─→ Response: { "leaveResponse": { "status": "failure",
 |   |   |                       "message": "[error details from Oracle]", "success": "false" } }
 |   |
 |   └─→ CATCH PATH (error):
 |   |
 |   ├─→ shape20: Branch (2 paths - SEQUENTIAL)
 |   |   └─→ Classification: SEQUENTIAL (Path 2 depends on Path 1)
 |   |
 |   |   ├─→ PATH 1 (Branch identifier: 1):
 |   |   |
 |   |   ├─→ shape19: ErrorMsg (Document Properties)
 |   |   |   └─→ READS: meta.base.catcherrorsmessage
 |   |   |   └─→ WRITES: process.DPP_ErrorMessage
 |   |   |
 |   |   └─→ shape21: ProcessCall - (Sub) Office 365 Email [SUBPROCESS]
 |   |       └─→ READS: process.To_Email, process.DPP_Subject, process.DPP_HasAttachment,
 |   |                  process.DPP_Process_Name, process.DPP_AtomName, process.DPP_ExecutionID,
 |   |                  process.DPP_ErrorMessage, process.DPP_Payload, process.DPP_File_Name
 |   |       └─→ ACTION: Send error notification email with payload attachment
 |   |       └─→ [See Section 14 for subprocess internal flow]
 |   |
 |   |   └─→ PATH 2 (Branch identifier: 2):
 |   |   |
 |   |   ├─→ shape41: Map - Leave Error Map (Transform)
 |   |   |   └─→ READS: process.DPP_ErrorMessage (via PropertyGet function)
 |   |   |   └─→ WRITES: D365 error response format
 |   |   |   └─→ DEFAULTS: status="failure", success="false"
 |   |   |
 |   |   └─→ shape43: Return Documents [HTTP: 400] [ERROR] [EARLY EXIT]
 |   |       └─→ Response: { "leaveResponse": { "status": "failure",
 |   |                       "message": "[Try/Catch error message]", "success": "false" } }
```

### 13.2 Execution Flow Summary

**Success Path:**
1. Receive leave request from D365
2. Set up error handling properties
3. Transform request to Oracle format
4. Set Oracle API URL
5. Log request
6. Call Oracle Fusion Create Leave API
7. Check HTTP status = 20*
8. Transform Oracle response to D365 format
9. Return success response [HTTP 200]

**Error Path 1 (Try/Catch Error):**
1. Receive leave request from D365
2. Set up error handling properties
3. Error occurs during processing
4. Extract error message from Try/Catch
5. Send error notification email (Path 1)
6. Map error to D365 response format (Path 2)
7. Return error response [HTTP 400]

**Error Path 2 (HTTP Non-20*, Not Gzip):**
1-6. [Same as success path]
7. Check HTTP status ≠ 20*
8. Check Content-Encoding ≠ gzip
9. Extract error message from HTTP response
10. Map error to D365 response format
11. Return error response [HTTP 400]

**Error Path 3 (HTTP Non-20*, Gzip):**
1-6. [Same as success path]
7. Check HTTP status ≠ 20*
8. Check Content-Encoding = gzip
9. Decompress gzip response
10. Extract error message from decompressed response
11. Map error to D365 response format
12. Return error response [HTTP 400]

---

## 14. Subprocess Analysis (Step 7a)

### 14.1 Subprocess: (Sub) Office 365 Email (a85945c5-3004-42b9-80b1-104f465cd1fb)

**Purpose:** Send error notification email via Office 365 SMTP

**Called By:** shape21 (processcall) in main process Catch path

**Abort on Error:** true  
**Wait for Completion:** true

### 14.2 Subprocess Internal Flow

```
START (shape1: passthrough)
 |
 ├─→ shape2: Try/Catch (Catch Errors)
 |   ├─→ TRY PATH (default):
 |   |
 |   ├─→ shape4: Decision - Attachment_Check
 |   |   └─→ READS: process.DPP_HasAttachment
 |   |   └─→ CONDITION: DPP_HasAttachment equals "Y"?
 |   |
 |   |   ├─→ IF TRUE (Has Attachment):
 |   |   |
 |   |   ├─→ shape11: Mail_Body (Message)
 |   |   |   └─→ READS: process.DPP_Process_Name, process.DPP_AtomName, 
 |   |   |              process.DPP_ExecutionID, process.DPP_ErrorMessage
 |   |   |   └─→ WRITES: HTML email body with execution details table
 |   |   |
 |   |   ├─→ shape14: set_MailBody (Document Properties)
 |   |   |   └─→ READS: Current document (HTML body)
 |   |   |   └─→ WRITES: process.DPP_MailBody
 |   |   |
 |   |   ├─→ shape15: payload (Message)
 |   |   |   └─→ READS: process.DPP_Payload
 |   |   |   └─→ WRITES: Payload as attachment content
 |   |   |
 |   |   ├─→ shape6: set_Mail_Properties (Document Properties)
 |   |   |   └─→ WRITES: connector.mail.fromAddress (from PP_Office365_Email.From_Email)
 |   |   |   └─→ WRITES: connector.mail.toAddress (from process.To_Email)
 |   |   |   └─→ WRITES: connector.mail.subject (Environment + process.DPP_Subject)
 |   |   |   └─→ WRITES: connector.mail.body (from process.DPP_MailBody)
 |   |   |   └─→ WRITES: connector.mail.filename (from process.DPP_File_Name)
 |   |   |
 |   |   ├─→ shape3: Email w Attachment (Downstream Mail Send)
 |   |   |   └─→ READS: connector.mail.* properties
 |   |   |   └─→ ACTION: Send email with attachment
 |   |   |   └─→ Connection: Office 365 Email (smtp-mail.outlook.com:587)
 |   |   |   └─→ bodyContentType: text/plain, dataContentType: text/html, disposition: attachment
 |   |   |
 |   |   └─→ shape5: Stop (continue=true) [SUCCESS RETURN]
 |   |
 |   |   └─→ IF FALSE (No Attachment):
 |   |   |
 |   |   ├─→ shape23: Mail_Body (Message)
 |   |   |   └─→ READS: process.DPP_Process_Name, process.DPP_AtomName,
 |   |   |              process.DPP_ExecutionID, process.DPP_ErrorMessage
 |   |   |   └─→ WRITES: HTML email body with execution details table
 |   |   |
 |   |   ├─→ shape22: set_MailBody (Document Properties)
 |   |   |   └─→ READS: Current document (HTML body)
 |   |   |   └─→ WRITES: process.DPP_MailBody
 |   |   |
 |   |   ├─→ shape20: set_Mail_Properties (Document Properties)
 |   |   |   └─→ WRITES: connector.mail.fromAddress (from PP_Office365_Email.From_Email)
 |   |   |   └─→ WRITES: connector.mail.toAddress (from process.To_Email)
 |   |   |   └─→ WRITES: connector.mail.subject (Environment + process.DPP_Subject)
 |   |   |   └─→ WRITES: connector.mail.body (from process.DPP_MailBody)
 |   |   |
 |   |   ├─→ shape7: Email W/O Attachment (Downstream Mail Send)
 |   |   |   └─→ READS: connector.mail.* properties
 |   |   |   └─→ ACTION: Send email without attachment
 |   |   |   └─→ Connection: Office 365 Email (smtp-mail.outlook.com:587)
 |   |   |   └─→ bodyContentType: text/plain, dataContentType: text/html, disposition: inline
 |   |   |
 |   |   └─→ shape9: Stop (continue=true) [SUCCESS RETURN]
 |   |
 |   └─→ CATCH PATH (error):
 |   |
 |   └─→ shape10: Exception (Throw)
 |       └─→ READS: meta.base.catcherrorsmessage
 |       └─→ ACTION: Throw exception with error message
 |       └─→ stopsingledoc: true
```

### 14.3 Subprocess Return Paths

**Success Return (Stop with continue=true):**
- **Label:** Implicit success (no explicit return label)
- **Shapes:** shape5 (with attachment), shape9 (without attachment)
- **Main Process Handling:** Continues to next shape after processcall (none - terminal)

**Error Return (Exception):**
- **Label:** Exception thrown
- **Shape:** shape10
- **Main Process Handling:** Caught by main process Try/Catch (already in Catch path)

### 14.4 Subprocess Properties

**Properties Read (from Main Process):**
- process.DPP_HasAttachment
- process.DPP_Process_Name
- process.DPP_AtomName
- process.DPP_ExecutionID
- process.DPP_ErrorMessage
- process.DPP_Payload
- process.To_Email
- process.DPP_Subject
- process.DPP_File_Name

**Properties Written (Internal):**
- process.DPP_MailBody
- connector.mail.fromAddress
- connector.mail.toAddress
- connector.mail.subject
- connector.mail.body
- connector.mail.filename (only if attachment)

**Properties Available to Main Process After Return:**
- None (subprocess does not write properties used by main process)

### 14.5 Subprocess Business Logic

**Purpose:** Send error notification email to support team

**Logic:**
1. Check if attachment flag is "Y"
2. Build HTML email body with execution details (process name, environment, execution ID, error message)
3. If attachment:
   - Set email body as HTML
   - Set payload as attachment content
   - Set attachment filename
   - Send email with attachment
4. If no attachment:
   - Set email body as HTML
   - Send email without attachment
5. On error: Throw exception

**Email Content:**
- **From:** Boomi.Dev.failures@al-ghurair.com
- **To:** BoomiIntegrationTeam@al-ghurair.com (from main process property)
- **Subject:** [Environment] [Atom Name] ([Process Name]) has errors to report
- **Body:** HTML table with execution details
- **Attachment:** Input payload (if DPP_HasAttachment = "Y")

---

## 15. System Layer Identification

### 15.1 Downstream Systems

| System Name | Type | Purpose | Connection Details |
|-------------|------|---------|-------------------|
| Oracle Fusion HCM | REST API | Create leave entries in Oracle HCM | https://iaaxey-dev3.fa.ocs.oraclecloud.com:443 |
| Office 365 Email | SMTP | Send error notification emails | smtp-mail.outlook.com:587 |

### 15.2 System Layer Operations

#### 15.2.1 Oracle Fusion HCM - Create Leave

**Operation:** Leave Oracle Fusion Create (6e8920fd-af5a-430b-a1d9-9fde7ac29a12)  
**Method:** HTTP POST  
**Endpoint:** {base_url}/{Resource_Path}  
**Full URL:** https://iaaxey-dev3.fa.ocs.oraclecloud.com:443/hcmRestApi/resources/11.13.18.05/absences  
**Authentication:** Basic Authentication  
**Content-Type:** application/json

**Request Payload:**
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
  "personAbsenceEntryId": 300000123456789,
  "absenceStatusCd": "SUBMITTED",
  "approvalStatusCd": "APPROVED",
  "personNumber": "9000604",
  "absenceType": "Sick Leave",
  "startDate": "2024-03-24",
  "endDate": "2024-03-25",
  "startDateDuration": 1,
  "endDateDuration": 1,
  ... [79+ additional fields]
}
```

**Error Response (HTTP 400/401/404/500):**
```json
{
  "errorCode": "VALIDATION_ERROR",
  "errorMessage": "Invalid absence type",
  "details": [...]
}
```

#### 15.2.2 Office 365 Email - Send Notification

**Operation:** Email w Attachment (af07502a-fafd-4976-a691-45d51a33b549) OR Email W/O Attachment (15a72a21-9b57-49a1-a8ed-d70367146644)  
**Protocol:** SMTP  
**Server:** smtp-mail.outlook.com:587  
**Authentication:** SMTP AUTH  
**TLS:** Enabled

**Email Details:**
- **From:** Boomi.Dev.failures@al-ghurair.com
- **To:** BoomiIntegrationTeam@al-ghurair.com
- **Subject:** DEV Failure : [Atom Name] ([Process Name]) has errors to report
- **Body:** HTML table with execution details
- **Attachment:** Input payload (if applicable)

---

## 16. Critical Patterns Identified

### 16.1 Pattern 1: Try/Catch Error Handling with Email Notification

**Identification:**
- Try/Catch wrapper (shape17) around main processing logic
- Catch path branches to send email notification AND return error response
- Sequential branch execution ensures email is sent before returning

**Implementation:**
```
Try:
  - Transform request
  - Call Oracle API
  - Check status
  - Return response
Catch:
  - Extract error message
  - Branch (Sequential):
    - Path 1: Send email notification
    - Path 2: Return error response
```

**Business Logic:**
- Any unexpected error during processing is caught
- Error notification is sent to support team with payload attachment
- Error response is returned to caller
- Both actions happen sequentially

### 16.2 Pattern 2: HTTP Status-Based Routing

**Identification:**
- Decision shape2 checks HTTP status code using wildcard match "20*"
- TRUE path (20*) → Success processing
- FALSE path (non-20*) → Error processing with nested decision for gzip handling

**Implementation:**
```
HTTP Operation → Check Status
  ├─ If 20* → Transform success response → Return success
  └─ If non-20* → Check if gzip
      ├─ If gzip → Decompress → Extract error → Return error
      └─ If not gzip → Extract error → Return error
```

**Business Logic:**
- HTTP 200/201 indicates successful leave creation
- HTTP 400/401/404/500 indicates error
- Error responses may be gzip compressed (need decompression)
- Error message is extracted and returned to caller

### 16.3 Pattern 3: Conditional Gzip Decompression

**Identification:**
- Decision shape44 checks Content-Encoding header
- TRUE path (gzip) → Decompress using Groovy script → Extract error
- FALSE path (not gzip) → Extract error directly

**Implementation:**
```
Check Content-Encoding header
  ├─ If "gzip" → Decompress (GZIPInputStream) → Extract error
  └─ If not "gzip" → Extract error directly
```

**Business Logic:**
- Oracle Fusion may return gzip-compressed error responses
- Decompression is only performed when Content-Encoding header indicates gzip
- Groovy script uses GZIPInputStream to decompress response
- Error message is then extracted from decompressed content

### 16.4 Pattern 4: Property-Based Configuration

**Identification:**
- Defined Process Properties (PP_HCM_LeaveCreate_Properties) store configuration values
- Properties are read and used to set dynamic document properties
- Allows environment-specific configuration without code changes

**Implementation:**
```
Defined Properties:
  - Resource_Path: Oracle API endpoint path
  - To_Email: Error notification recipient
  - DPP_HasAttachment: Email attachment flag

Usage:
  - shape8: Set URL from Resource_Path
  - shape38: Set To_Email from defined property
  - shape38: Set DPP_HasAttachment from defined property
```

**Business Logic:**
- Configuration values are externalized to process properties
- Different values can be set for DEV/QA/PROD environments
- No code changes needed for environment-specific configuration

### 16.5 Pattern 5: Sequential Branch for Error Handling

**Identification:**
- Branch shape20 has 2 paths with data dependency
- Path 1 writes process.DPP_ErrorMessage
- Path 2 reads process.DPP_ErrorMessage
- Topological sort determines Path 1 → Path 2 execution order

**Implementation:**
```
Branch (Sequential):
  Path 1: Extract error message → Send email notification
  Path 2: Map error message → Return error response
```

**Business Logic:**
- Error message must be extracted before it can be used
- Email notification is sent first (logging/alerting)
- Error response is returned second (caller notification)
- Sequential execution ensures both actions complete

---

## 17. Validation Checklist

### 17.1 Data Dependencies

- ✅ All property WRITES identified (Section 7.1)
- ✅ All property READS identified (Section 7.2)
- ✅ Dependency graph built (Section 8)
- ✅ Execution order satisfies all dependencies (Section 12)
- ✅ No read-before-write violations

### 17.2 Decision Analysis

- ✅ ALL decision shapes inventoried (Section 10)
- ✅ BOTH TRUE and FALSE paths traced to termination (Section 10.2)
- ✅ Pattern type identified for each decision (Section 10.3)
- ✅ Early exits identified and documented (Section 10.2)
- ✅ Convergence points identified (None - all paths terminate)
- ✅ Decision data sources identified (Section 10.2)
- ✅ Decision types classified (POST_OPERATION for both)

### 17.3 Branch Analysis

- ✅ Each branch classified as parallel or sequential (Section 11)
- ✅ Classification based on analysis, not assumption (Section 11.1)
- ✅ If sequential: dependency_order built using topological sort (Section 11.2.5)
- ✅ Each path traced to terminal point (Section 11.2.6)
- ✅ Convergence points identified (None)
- ✅ Execution continuation point determined (None - terminal)
- ✅ API calls checked (No API calls in branch paths)

### 17.4 Sequence Diagram

- ✅ Format follows required structure (Section 13)
- ✅ Each operation shows READS and WRITES (Section 13.1)
- ✅ Decisions show both TRUE and FALSE paths (Section 13.1)
- ✅ Early exits marked [EARLY EXIT] (Section 13.1)
- ✅ Conditional execution marked appropriately (Section 13.1)
- ✅ Subprocess internal flows documented (Section 14)
- ✅ Sequence diagram matches control flow graph (Section 9)
- ✅ Execution order matches dependency graph (Section 8)
- ✅ HTTP status codes documented (Section 6)

### 17.5 Subprocess Analysis

- ✅ ALL subprocesses analyzed (Section 14)
- ✅ Internal flow traced (Section 14.2)
- ✅ Return paths identified (Section 14.3)
- ✅ Return path labels mapped to main process shapes (Section 14.3)
- ✅ Properties written by subprocess documented (Section 14.4)
- ✅ Properties read by subprocess from main process documented (Section 14.4)

### 17.6 Input/Output Structure Analysis

- ✅ Entry point operation identified (Section 2)
- ✅ Request profile identified and loaded (Section 2.1)
- ✅ Request profile structure analyzed (Section 2.2)
- ✅ Array vs single object detected (Section 2.1 - Single object)
- ✅ ALL request fields extracted (Section 2.4)
- ✅ Request field paths documented (Section 2.4)
- ✅ Request field mapping table generated (Section 2.4)
- ✅ Response profile identified and loaded (Section 3.1)
- ✅ Response profile structure analyzed (Section 3.2)
- ✅ ALL response fields extracted (Section 3.3)
- ✅ Response field mapping table generated (Section 3.3)
- ✅ Document processing behavior determined (Section 2.3)

### 17.7 HTTP Status Codes and Return Path Responses

- ✅ All return paths documented with HTTP status codes (Section 6.1)
- ✅ Response JSON examples provided for each return path (Section 6.2-6.5)
- ✅ Populated fields documented for each return path (Section 6.2-6.5)
- ✅ Decision conditions leading to each return documented (Section 6.2-6.5)
- ✅ Error codes and success codes documented (Section 6.2-6.5)
- ✅ Downstream operation HTTP status codes documented (Section 6.6)
- ✅ Error handling strategy documented (Section 6.6)

### 17.8 Map Analysis

- ✅ ALL map files identified and loaded (Section 5.1)
- ✅ Field mappings extracted from each map (Section 5.2-5.4)
- ✅ Profile vs map field name discrepancies documented (Section 5.2.2)
- ✅ Map field names marked as AUTHORITATIVE (Section 5.2.2)
- ✅ Scripting functions analyzed (Section 5.4.3)
- ✅ Static values identified and documented (Section 5.3.2, 5.4.2)
- ✅ Process property mappings documented (Section 5.4.1)

---

## 18. Function Exposure Decision Table

### 18.1 Purpose

This table determines which functions should be exposed at the Process Layer vs System Layer to prevent function explosion and maintain proper API-Led Architecture boundaries.

### 18.2 Decision Criteria

| Criterion | Process Layer | System Layer |
|-----------|---------------|--------------|
| **Business Logic** | Orchestration, transformation, routing | Direct system interaction |
| **Reusability** | Process-specific orchestration | Reusable across processes |
| **Complexity** | Complex multi-step workflows | Simple CRUD operations |
| **Error Handling** | Business error handling | Technical error handling |
| **Data Transformation** | Business-level transformation | System-level transformation |

### 18.3 Function Exposure Analysis

| Function Name | Current Layer | Expose As | Reasoning |
|---------------|---------------|-----------|-----------|
| **HCM Leave Create (Main Process)** | Process Layer | ✅ Process Layer API | Orchestrates leave creation with error handling, email notifications, and response transformation. Business-level orchestration. |
| **Oracle Fusion Create Leave** | System Layer | ✅ System Layer API | Direct interaction with Oracle Fusion HCM REST API. Reusable for other leave-related processes. Simple POST operation. |
| **Office 365 Email Notification** | System Layer (Subprocess) | ✅ System Layer API | Reusable email notification service. Can be used by multiple processes for error notifications. |

### 18.4 Recommended API Structure

#### 18.4.1 Process Layer API

**API Name:** HCM Leave Create Process API  
**Endpoint:** POST /api/process/hcm/leaves  
**Purpose:** Orchestrate leave creation from D365 to Oracle Fusion with error handling

**Request:**
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

**Response:**
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

**Responsibilities:**
- Receive leave request from D365
- Transform request to Oracle Fusion format
- Call System Layer Oracle Fusion API
- Handle HTTP status codes (success/error)
- Handle gzip decompression if needed
- Transform response to D365 format
- Send error notification email on failure
- Return standardized response

#### 18.4.2 System Layer API 1: Oracle Fusion Leave Management

**API Name:** Oracle Fusion Leave Management System API  
**Endpoint:** POST /api/system/oracle-fusion/leaves  
**Purpose:** Direct interaction with Oracle Fusion HCM REST API for leave operations

**Request:**
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

**Response:**
```json
{
  "personAbsenceEntryId": 300000123456789,
  "absenceStatusCd": "SUBMITTED",
  "approvalStatusCd": "APPROVED",
  "personNumber": "9000604",
  "absenceType": "Sick Leave",
  "startDate": "2024-03-24",
  "endDate": "2024-03-25",
  "startDateDuration": 1,
  "endDateDuration": 1,
  ... [additional Oracle fields]
}
```

**Responsibilities:**
- Authenticate with Oracle Fusion (Basic Auth)
- Make HTTP POST request to Oracle Fusion absences endpoint
- Return raw Oracle Fusion response
- Handle Oracle-specific errors (HTTP 400/401/404/500)
- No business logic or transformation

#### 18.4.3 System Layer API 2: Email Notification Service

**API Name:** Email Notification System API  
**Endpoint:** POST /api/system/email/notifications  
**Purpose:** Send email notifications via Office 365 SMTP

**Request:**
```json
{
  "from": "Boomi.Dev.failures@al-ghurair.com",
  "to": "BoomiIntegrationTeam@al-ghurair.com",
  "subject": "DEV Failure : [Process Name] has errors to report",
  "body": "<html>...</html>",
  "hasAttachment": true,
  "attachmentContent": "{...payload...}",
  "attachmentFilename": "HCM_Leave_Create_2024-01-15T10:30:00.000Z.txt"
}
```

**Response:**
```json
{
  "success": true,
  "messageId": "abc123",
  "timestamp": "2024-01-15T10:30:00Z"
}
```

**Responsibilities:**
- Authenticate with Office 365 SMTP
- Send email with or without attachment
- Return send status
- Handle SMTP errors
- No business logic

### 18.5 Function Exposure Summary

| Layer | Function Count | Functions |
|-------|----------------|-----------|
| **Process Layer** | 1 | HCM Leave Create Process API |
| **System Layer** | 2 | Oracle Fusion Leave Management API, Email Notification API |
| **Total APIs** | 3 | Prevents function explosion by proper layering |

### 18.6 Benefits of This Structure

1. **Reusability:** System Layer APIs can be used by other processes (e.g., Leave Update, Leave Delete)
2. **Separation of Concerns:** Business logic in Process Layer, system interaction in System Layer
3. **Maintainability:** Changes to Oracle Fusion API only affect System Layer
4. **Testability:** Each layer can be tested independently
5. **Scalability:** System Layer APIs can be scaled independently based on usage
6. **Function Explosion Prevention:** Only 3 APIs instead of exposing every Boomi shape as a function

---

## 19. Summary and Recommendations

### 19.1 Process Summary

**Process Name:** HCM Leave Create  
**Purpose:** Synchronize leave data from D365 to Oracle Fusion HCM  
**Complexity:** Medium  
**Integration Pattern:** Process Layer orchestration with System Layer API calls

**Key Characteristics:**
- Single document processing (no batch)
- Synchronous HTTP POST to Oracle Fusion
- Comprehensive error handling (Try/Catch + HTTP status checks)
- Error notification via email with payload attachment
- Gzip decompression support for error responses
- Standardized response format for D365

### 19.2 Execution Flow Summary

1. **Input:** Receive leave request from D365 (Web Service Server Listen)
2. **Setup:** Initialize error handling properties
3. **Transform:** Map D365 format to Oracle Fusion format
4. **Call:** HTTP POST to Oracle Fusion HCM API
5. **Check:** Verify HTTP status code (20* = success, non-20* = error)
6. **Success Path:** Transform Oracle response → Return success to D365
7. **Error Path:** Extract error message (decompress if gzip) → Return error to D365
8. **Exception Path:** Send email notification → Return error to D365

### 19.3 Critical Dependencies

1. **shape8 → shape33:** URL must be set before HTTP call
2. **shape33 → shape2:** HTTP operation must complete before status check
3. **shape33 → shape44:** HTTP operation must complete before Content-Encoding check
4. **shape19 → shape41:** Error message must be extracted before mapping (sequential branch)
5. **shape38 → shape21:** Error handling properties must be set before subprocess call

### 19.4 Recommendations for Azure Migration

#### 19.4.1 Architecture

1. **Process Layer Function:** Create Azure Function for HCM Leave Create Process
   - HTTP Trigger (POST)
   - Orchestrates leave creation workflow
   - Handles error scenarios and notifications

2. **System Layer Function 1:** Create Azure Function for Oracle Fusion Leave Management
   - HTTP Trigger (POST)
   - Direct interaction with Oracle Fusion API
   - Reusable for other leave operations

3. **System Layer Function 2:** Create Azure Function for Email Notification Service
   - HTTP Trigger (POST)
   - Send emails via Office 365 SMTP or Microsoft Graph API
   - Reusable for all error notifications

#### 19.4.2 Error Handling

1. **Try/Catch:** Implement try/catch blocks in Azure Function
2. **HTTP Status Checks:** Use HttpClient status code checks
3. **Gzip Decompression:** Use GZipStream for decompression
4. **Error Logging:** Use Application Insights for error tracking
5. **Email Notifications:** Use Microsoft Graph API or SMTP client

#### 19.4.3 Configuration

1. **App Settings:** Store Oracle Fusion URL, API path, email recipients
2. **Key Vault:** Store Oracle Fusion credentials, email credentials
3. **Environment Variables:** Store environment-specific settings (DEV/QA/PROD)

#### 19.4.4 Testing

1. **Unit Tests:** Test each function independently
2. **Integration Tests:** Test Process Layer → System Layer integration
3. **Error Scenario Tests:** Test all error paths (Try/Catch, HTTP errors, gzip)
4. **Performance Tests:** Test response times and throughput

### 19.5 Phase 1 Completion Confirmation

✅ **Phase 1 Extraction Complete**

All mandatory sections completed:
- ✅ Operations Inventory
- ✅ Input Structure Analysis (Step 1a)
- ✅ Response Structure Analysis (Step 1b)
- ✅ Operation Response Analysis (Step 1c)
- ✅ Map Analysis (Step 1d)
- ✅ HTTP Status Codes and Return Path Responses (Step 1e)
- ✅ Process Properties Analysis (Steps 2-3)
- ✅ Data Dependency Graph (Step 4)
- ✅ Control Flow Graph (Step 5)
- ✅ Decision Shape Analysis (Step 7)
- ✅ Branch Shape Analysis (Step 8)
- ✅ Execution Order (Step 9)
- ✅ Sequence Diagram (Step 10)
- ✅ Subprocess Analysis (Step 7a)
- ✅ System Layer Identification
- ✅ Critical Patterns Identified
- ✅ Validation Checklist
- ✅ Function Exposure Decision Table

**Ready for Phase 2: Code Generation**

---

**End of BOOMI_EXTRACTION_PHASE1.md**
