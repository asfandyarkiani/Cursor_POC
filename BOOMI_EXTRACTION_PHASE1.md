# BOOMI EXTRACTION PHASE 1 - HCM Leave Create Process

**Process Name:** HCM_Leave Create  
**Process ID:** ca69f858-785f-4565-ba1f-b4edc6cca05b  
**Description:** This Process will sync the Leave data between D365 and Oracle HCM  
**Analysis Date:** 2026-02-20

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
15. [System Layer Identification](#15-system-layer-identification)
16. [Function Exposure Decision Table](#16-function-exposure-decision-table)
17. [Validation Checklist](#17-validation-checklist)

---

## 1. Operations Inventory

### Main Process Operations

| Operation ID | Operation Name | Type | Sub-Type | Purpose |
|--------------|----------------|------|----------|---------|
| 8f709c2b-e63f-4d5f-9374-2932ed70415d | Create Leave Oracle Fusion OP | connector-action | wss | Web Service Server - Entry point for leave creation requests |
| 6e8920fd-af5a-430b-a1d9-9fde7ac29a12 | Leave Oracle Fusion Create | connector-action | http | HTTP POST to Oracle Fusion HCM to create leave absence |
| af07502a-fafd-4976-a691-45d51a33b549 | Email w Attachment | connector-action | mail | Send email with attachment for error notifications |
| 15a72a21-9b57-49a1-a8ed-d70367146644 | Email W/O Attachment | connector-action | mail | Send email without attachment for error notifications |

### Subprocess Operations

| Subprocess ID | Subprocess Name | Purpose |
|---------------|-----------------|---------|
| a85945c5-3004-42b9-80b1-104f465cd1fb | (Sub) Office 365 Email | Email notification subprocess for error handling |

### Maps

| Map ID | Map Name | From Profile | To Profile | Purpose |
|--------|----------|--------------|------------|---------|
| c426b4d6-2aff-450e-b43b-59956c4dbc96 | Leave Create Map | D365 Leave Create JSON Profile | HCM Leave Create JSON Profile | Transform D365 request to Oracle Fusion format |
| e4fd3f59-edb5-43a1-aeae-143b600a064e | Oracle Fusion Leave Response Map | Oracle Fusion Leave Response JSON Profile | Leave D365 Response | Transform Oracle Fusion response to D365 response format |
| f46b845a-7d75-41b5-b0ad-c41a6a8e9b12 | Leave Error Map | Dummy FF Profile | Leave D365 Response | Transform error messages to D365 error response format |

### Connections

| Connection ID | Connection Name | Type | Purpose |
|---------------|-----------------|------|---------|
| aa1fcb29-d146-4425-9ea6-b9698090f60e | Oracle Fusion | http | Connection to Oracle Fusion HCM REST API |
| 00eae79b-2303-4215-8067-dcc299e42697 | Office 365 Email | mail | Connection to Office 365 SMTP for email notifications |

---

## 2. Input Structure Analysis (Step 1a)

### ✅ VALIDATION CHECKPOINT: Step 1a Complete

**Request Profile Structure**

- **Profile ID:** febfa3e1-f719-4ee8-ba57-cdae34137ab3
- **Profile Name:** D365 Leave Create JSON Profile
- **Profile Type:** profile.json
- **Root Structure:** Root/Object
- **Array Detection:** ❌ NO - Single object structure
- **Input Type:** singlejson

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

- **Boomi Processing:** Single document processing (inputType: singlejson)
- **Azure Function Requirement:** Must accept single leave request object
- **Implementation Pattern:** Process single leave request, return response

### Request Field Mapping

| Boomi Field Path | Boomi Field Name | Data Type | Required | Azure DTO Property | Notes |
|------------------|------------------|-----------|----------|-------------------|-------|
| Root/Object/employeeNumber | employeeNumber | number | Yes | EmployeeNumber | Employee identifier |
| Root/Object/absenceType | absenceType | character | Yes | AbsenceType | Type of leave (e.g., Sick Leave) |
| Root/Object/employer | employer | character | Yes | Employer | Employer name |
| Root/Object/startDate | startDate | character | Yes | StartDate | Leave start date (YYYY-MM-DD) |
| Root/Object/endDate | endDate | character | Yes | EndDate | Leave end date (YYYY-MM-DD) |
| Root/Object/absenceStatusCode | absenceStatusCode | character | Yes | AbsenceStatusCode | Status code (e.g., SUBMITTED) |
| Root/Object/approvalStatusCode | approvalStatusCode | character | Yes | ApprovalStatusCode | Approval status (e.g., APPROVED) |
| Root/Object/startDateDuration | startDateDuration | number | Yes | StartDateDuration | Duration for start date (1 = full day) |
| Root/Object/endDateDuration | endDateDuration | number | Yes | EndDateDuration | Duration for end date (1 = full day) |

**Total Fields:** 9

---

## 3. Response Structure Analysis (Step 1b)

### ✅ VALIDATION CHECKPOINT: Step 1b Complete

**Response Profile Structure**

- **Profile ID:** f4ca3a70-114a-4601-bad8-44a3eb20e2c0
- **Profile Name:** Leave D365 Response
- **Profile Type:** profile.json
- **Root Structure:** leaveResponse/Object
- **Array Detection:** ❌ NO - Single object structure

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
|------------------|------------------|-----------|-------------------|-------|
| leaveResponse/Object/status | status | character | Status | Status of the operation (success/failure) |
| leaveResponse/Object/message | message | character | Message | Descriptive message |
| leaveResponse/Object/personAbsenceEntryId | personAbsenceEntryId | number | PersonAbsenceEntryId | Oracle Fusion absence entry ID (on success) |
| leaveResponse/Object/success | success | character | Success | Boolean flag as string (true/false) |

**Total Fields:** 4

---

## 4. Operation Response Analysis (Step 1c)

### ✅ VALIDATION CHECKPOINT: Step 1c Complete

### Operation: Leave Oracle Fusion Create (6e8920fd-af5a-430b-a1d9-9fde7ac29a12)

**Operation Type:** HTTP POST  
**Response Profile ID:** None (responseProfileType: NONE)  
**Response Handling:** Raw response returned, no profile mapping

**Response Headers Captured:**
- **Header:** Content-Encoding
- **Target Property:** dynamicdocument.DDP_RespHeader
- **Purpose:** Check if response is gzip compressed

**Extracted Fields:**
- No explicit profile-based extraction
- Response body is passed through for mapping

**Data Consumers:**
- **shape2 (Decision):** Checks HTTP status code (meta.base.applicationstatuscode) to determine success/failure
- **shape44 (Decision):** Checks Content-Encoding header to determine if decompression is needed
- **shape34 (Map):** Maps successful response to D365 response format
- **shape39 (Document Properties):** Extracts error message on failure
- **shape46 (Document Properties):** Extracts error message on gzip failure

**Business Logic Implications:**
- HTTP status code check (20*) determines success vs failure path
- Success path (HTTP 20x) → Map response → Return success
- Failure path (HTTP non-20x) → Check compression → Extract error → Return error

---

## 5. Map Analysis (Step 1d)

### ✅ VALIDATION CHECKPOINT: Step 1d Complete

### Map Inventory

| Map ID | Map Name | From Profile | To Profile | Type |
|--------|----------|--------------|------------|------|
| c426b4d6-2aff-450e-b43b-59956c4dbc96 | Leave Create Map | febfa3e1-f719-4ee8-ba57-cdae34137ab3 | a94fa205-c740-40a5-9fda-3d018611135a | Request Transformation |
| e4fd3f59-edb5-43a1-aeae-143b600a064e | Oracle Fusion Leave Response Map | 316175c7-0e45-4869-9ac6-5f9d69882a62 | f4ca3a70-114a-4601-bad8-44a3eb20e2c0 | Success Response Transformation |
| f46b845a-7d75-41b5-b0ad-c41a6a8e9b12 | Leave Error Map | 23d7a2e9-5cb0-4e9c-9e4b-3154834bad0d | f4ca3a70-114a-4601-bad8-44a3eb20e2c0 | Error Response Transformation |

### Map 1: Leave Create Map (c426b4d6-2aff-450e-b43b-59956c4dbc96)

**From Profile:** febfa3e1-f719-4ee8-ba57-cdae34137ab3 (D365 Leave Create JSON Profile)  
**To Profile:** a94fa205-c740-40a5-9fda-3d018611135a (HCM Leave Create JSON Profile)  
**Type:** Request Transformation (D365 → Oracle Fusion)

**Field Mappings:**

| Source Field | Source Type | Target Field | Profile Field Name | Discrepancy? |
|--------------|-------------|--------------|-------------------|--------------|
| employeeNumber | profile | personNumber | personNumber | ✅ Match (field name change) |
| absenceType | profile | absenceType | absenceType | ✅ Match |
| employer | profile | employer | employer | ✅ Match |
| startDate | profile | startDate | startDate | ✅ Match |
| endDate | profile | endDate | endDate | ✅ Match |
| absenceStatusCode | profile | absenceStatusCd | absenceStatusCd | ✅ Match (field name change) |
| approvalStatusCode | profile | approvalStatusCd | approvalStatusCd | ✅ Match (field name change) |
| startDateDuration | profile | startDateDuration | startDateDuration | ✅ Match |
| endDateDuration | profile | endDateDuration | endDateDuration | ✅ Match |

**Scripting Functions:** None

**Profile vs Map Comparison:**
- Field name transformations: employeeNumber → personNumber, absenceStatusCode → absenceStatusCd, approvalStatusCode → approvalStatusCd
- All transformations are simple field renames, no complex logic

### Map 2: Oracle Fusion Leave Response Map (e4fd3f59-edb5-43a1-aeae-143b600a064e)

**From Profile:** 316175c7-0e45-4869-9ac6-5f9d69882a62 (Oracle Fusion Leave Response JSON Profile)  
**To Profile:** f4ca3a70-114a-4601-bad8-44a3eb20e2c0 (Leave D365 Response)  
**Type:** Success Response Transformation (Oracle Fusion → D365)

**Field Mappings:**

| Source Field | Source Type | Target Field | Notes |
|--------------|-------------|--------------|-------|
| personAbsenceEntryId | profile | personAbsenceEntryId | Oracle Fusion absence entry ID |

**Default Values:**

| Target Field | Default Value | Purpose |
|--------------|---------------|---------|
| status | "success" | Indicates successful operation |
| message | "Data successfully sent to Oracle Fusion" | Success message |
| success | "true" | Boolean flag for success |

### Map 3: Leave Error Map (f46b845a-7d75-41b5-b0ad-c41a6a8e9b12)

**From Profile:** 23d7a2e9-5cb0-4e9c-9e4b-3154834bad0d (Dummy FF Profile)  
**To Profile:** f4ca3a70-114a-4601-bad8-44a3eb20e2c0 (Leave D365 Response)  
**Type:** Error Response Transformation

**Field Mappings:**

| Source Field | Source Type | Target Field | Notes |
|--------------|-------------|--------------|-------|
| DPP_ErrorMessage | function (PropertyGet) | message | Error message from process property |

**Function: PropertyGet**
- **Function Key:** 1
- **Type:** PropertyGet
- **Input:** Property Name = "DPP_ErrorMessage"
- **Output:** Result (error message)
- **Purpose:** Retrieve error message from process property

**Default Values:**

| Target Field | Default Value | Purpose |
|--------------|---------------|---------|
| status | "failure" | Indicates failed operation |
| success | "false" | Boolean flag for failure |

**CRITICAL RULE:** Map field names are AUTHORITATIVE. Use map field names in transformations.

---

## 6. HTTP Status Codes and Return Path Responses (Step 1e)

### ✅ VALIDATION CHECKPOINT: Step 1e Complete

### Return Path Inventory

| Return Label | Return Shape ID | HTTP Status Code | Decision Conditions | Type |
|--------------|-----------------|------------------|---------------------|------|
| Success Response | shape35 | 200 | HTTP Status 20* check → TRUE | Success |
| Error Response | shape36 | 400 | HTTP Status 20* check → FALSE, Content-Encoding != gzip | Error |
| Error Response | shape43 | 400 | Try/Catch error path | Error |
| Error Response | shape48 | 400 | HTTP Status 20* check → FALSE, Content-Encoding == gzip | Error |

### Return Path 1: Success Response (shape35)

**Return Label:** "Success Response"  
**Return Shape ID:** shape35  
**HTTP Status Code:** 200  
**Decision Conditions Leading to Return:**
- shape2 (Decision): meta.base.applicationstatuscode wildcard matches "20*" → TRUE path

**Populated Response Fields:**

| Field Name | Field Path | Source | Populated By |
|------------|-----------|--------|--------------|
| personAbsenceEntryId | leaveResponse/Object/personAbsenceEntryId | operation_response | Leave Oracle Fusion Create (shape33) |
| status | leaveResponse/Object/status | static (map default) | Oracle Fusion Leave Response Map (shape34) |
| message | leaveResponse/Object/message | static (map default) | Oracle Fusion Leave Response Map (shape34) |
| success | leaveResponse/Object/success | static (map default) | Oracle Fusion Leave Response Map (shape34) |

**Response JSON Example:**

```json
{
  "status": "success",
  "message": "Data successfully sent to Oracle Fusion",
  "personAbsenceEntryId": 300000123456789,
  "success": "true"
}
```

### Return Path 2: Error Response - Non-Gzip (shape36)

**Return Label:** "Error Response"  
**Return Shape ID:** shape36  
**HTTP Status Code:** 400  
**Decision Conditions Leading to Return:**
- shape2 (Decision): meta.base.applicationstatuscode wildcard matches "20*" → FALSE path
- shape44 (Decision): dynamicdocument.DDP_RespHeader equals "gzip" → FALSE path

**Error Code:** N/A (HTTP error)  
**Populated Response Fields:**

| Field Name | Field Path | Source | Populated By |
|------------|-----------|--------|--------------|
| message | leaveResponse/Object/message | process_property | shape39 (DPP_ErrorMessage from meta.base.applicationstatusmessage) |
| status | leaveResponse/Object/status | static (map default) | Leave Error Map (shape40) |
| success | leaveResponse/Object/success | static (map default) | Leave Error Map (shape40) |

**Response JSON Example:**

```json
{
  "status": "failure",
  "message": "HTTP 400: Bad Request - Invalid absence type",
  "success": "false"
}
```

### Return Path 3: Error Response - Try/Catch (shape43)

**Return Label:** "Error Response"  
**Return Shape ID:** shape43  
**HTTP Status Code:** 400  
**Decision Conditions Leading to Return:**
- shape17 (Try/Catch): Error caught in Try block → Catch path
- shape20 (Branch): Path 2

**Error Code:** N/A (Exception)  
**Populated Response Fields:**

| Field Name | Field Path | Source | Populated By |
|------------|-----------|--------|--------------|
| message | leaveResponse/Object/message | process_property | shape19 (DPP_ErrorMessage from meta.base.catcherrorsmessage) |
| status | leaveResponse/Object/status | static (map default) | Leave Error Map (shape41) |
| success | leaveResponse/Object/success | static (map default) | Leave Error Map (shape41) |

**Response JSON Example:**

```json
{
  "status": "failure",
  "message": "Connection timeout to Oracle Fusion HCM",
  "success": "false"
}
```

### Return Path 4: Error Response - Gzip Decompression (shape48)

**Return Label:** "Error Response"  
**Return Shape ID:** shape48  
**HTTP Status Code:** 400  
**Decision Conditions Leading to Return:**
- shape2 (Decision): meta.base.applicationstatuscode wildcard matches "20*" → FALSE path
- shape44 (Decision): dynamicdocument.DDP_RespHeader equals "gzip" → TRUE path

**Error Code:** N/A (Gzip error)  
**Populated Response Fields:**

| Field Name | Field Path | Source | Populated By |
|------------|-----------|--------|--------------|
| message | leaveResponse/Object/message | process_property | shape46 (DPP_ErrorMessage from current document after decompression) |
| status | leaveResponse/Object/status | static (map default) | Leave Error Map (shape47) |
| success | leaveResponse/Object/success | static (map default) | Leave Error Map (shape47) |

**Response JSON Example:**

```json
{
  "status": "failure",
  "message": "HTTP 500: Internal Server Error - Oracle Fusion service unavailable",
  "success": "false"
}
```

### Downstream Operations HTTP Status Codes

| Operation Name | Expected Success Codes | Error Codes | Error Handling |
|----------------|----------------------|-------------|----------------|
| Leave Oracle Fusion Create | 200, 201, 202 (20*) | 400, 401, 403, 404, 500 | Return error response with message |

---

## 7. Process Properties Analysis (Steps 2-3)

### ✅ VALIDATION CHECKPOINT: Steps 2-3 Complete

### Property WRITES (Step 2)

| Property Name | Written By Shape(s) | Source | Purpose |
|---------------|---------------------|--------|---------|
| process.DPP_Process_Name | shape38 | Execution Property (Process Name) | Capture process name for logging/email |
| process.DPP_AtomName | shape38 | Execution Property (Atom Name) | Capture atom name for logging/email |
| process.DPP_Payload | shape38 | Current Document | Capture input payload for error reporting |
| process.DPP_ExecutionID | shape38 | Execution Property (Execution Id) | Capture execution ID for tracking |
| process.DPP_File_Name | shape38 | Concatenation (Process Name + Timestamp + ".txt") | Generate filename for email attachment |
| process.DPP_Subject | shape38 | Concatenation (Atom Name + " (" + Process Name + " ) has errors to report") | Generate email subject |
| process.To_Email | shape38 | Defined Process Property (PP_HCM_LeaveCreate_Properties.To_Email) | Email recipients for error notifications |
| process.DPP_HasAttachment | shape38 | Defined Process Property (PP_HCM_LeaveCreate_Properties.DPP_HasAttachment) | Flag to determine if email should have attachment |
| dynamicdocument.URL | shape8 | Defined Process Property (PP_HCM_LeaveCreate_Properties.Resource_Path) | Oracle Fusion REST API resource path |
| process.DPP_ErrorMessage | shape19 | Track Property (meta.base.catcherrorsmessage) | Capture error message from Try/Catch |
| process.DPP_ErrorMessage | shape39 | Track Property (meta.base.applicationstatusmessage) | Capture error message from HTTP response |
| process.DPP_ErrorMessage | shape46 | Current Document (after decompression) | Capture error message from gzip response |

### Property READS (Step 3)

| Property Name | Read By Shape(s) | Usage | Purpose |
|---------------|------------------|-------|---------|
| process.DPP_HasAttachment | shape4 (Decision in subprocess) | Decision comparison | Determine email attachment requirement |
| process.To_Email | shape6 (Document Properties in subprocess) | Mail - To Address | Set email recipient |
| process.DPP_Subject | shape6 (Document Properties in subprocess) | Mail - Subject | Set email subject |
| process.DPP_MailBody | shape6 (Document Properties in subprocess) | Mail - Body | Set email body content |
| process.DPP_File_Name | shape6 (Document Properties in subprocess) | Mail - File Name | Set attachment filename |
| process.DPP_Process_Name | shape11 (Message in subprocess) | Email body parameter | Display process name in error email |
| process.DPP_AtomName | shape11 (Message in subprocess) | Email body parameter | Display atom name in error email |
| process.DPP_ExecutionID | shape11 (Message in subprocess) | Email body parameter | Display execution ID in error email |
| process.DPP_ErrorMessage | shape11 (Message in subprocess) | Email body parameter | Display error message in error email |
| process.DPP_Payload | shape15 (Message in subprocess) | Message content | Pass payload as email attachment |
| dynamicdocument.URL | shape33 (HTTP Operation) | Path element | Set Oracle Fusion API endpoint URL |
| process.DPP_ErrorMessage | Map f46b845a (Function PropertyGet) | Map function input | Retrieve error message for error response |

### Defined Process Properties

**Component: PP_HCM_LeaveCreate_Properties (e22c04db-7dfa-4b1b-9d88-77365b0fdb18)**

| Property Key | Property Label | Type | Default Value | Purpose |
|--------------|----------------|------|---------------|---------|
| c2d1a1e8-4bcb-435b-b1d2-bb10a209bcc0 | Resource_Path | string | "hcmRestApi/resources/11.13.18.05/absences" | Oracle Fusion HCM REST API resource path |
| 71c90f6e-4f86-49f3-8aef-bae6668c73f9 | To_Email | string | "BoomiIntegrationTeam@al-ghurair.com" | Email recipients for error notifications |
| a717867b-67f4-4a72-be2d-c99c73309fdb | DPP_HasAttachment | string | "Y" | Flag to include attachment in error emails |

**Component: PP_Office365_Email (0feff13f-8a2c-438a-b3c7-1909e2a7f533)**

| Property Key | Property Label | Type | Default Value | Purpose |
|--------------|----------------|------|---------------|---------|
| 804b6d40-eeee-4ac0-ab4b-94e1aca9f4b7 | From_Email | string | "Boomi.Dev.failures@al-ghurair.com" | Sender email address |
| 600acadb-ee02-4369-af85-ee70af380b6c | To_Email | string | "Rajesh.Muppala@al-ghurair.com;mohan.jonnalagadda@al-ghurair.com" | Default email recipients (overridden by main process) |
| 2fa6ce9e-437a-44cc-b44f-5c7e61052f41 | HasAttachment | string | "Y" | Default attachment flag (overridden by main process) |
| 3ca9f307-cecb-4d1e-b9ec-007839509ed7 | EmailBody | string | "" | Email body content (not used) |
| d8fc7496-ec2c-4308-a4ca-e9f49c0c9500 | Environment | string | "DEV Failure :" | Environment prefix for email subject |

---

## 8. Data Dependency Graph (Step 4)

### ✅ VALIDATION CHECKPOINT: Step 4 Complete

### Dependency Graph

**Property: dynamicdocument.URL**
- **WRITTEN BY:** shape8 (Document Properties)
- **READ BY:** shape33 (HTTP Operation - Leave Oracle Fusion Create)
- **DEPENDENCY:** shape8 must execute BEFORE shape33

**Property: process.DPP_ErrorMessage**
- **WRITTEN BY:** shape19 (Document Properties - Try/Catch error), shape39 (Document Properties - HTTP error), shape46 (Document Properties - Gzip error)
- **READ BY:** shape11 (Message in subprocess), Map f46b845a (Function PropertyGet)
- **DEPENDENCY:** shape19/shape39/shape46 must execute BEFORE subprocess shape11 and error map

**Property: process.DPP_Process_Name**
- **WRITTEN BY:** shape38 (Document Properties)
- **READ BY:** shape11 (Message in subprocess)
- **DEPENDENCY:** shape38 must execute BEFORE subprocess shape11

**Property: process.DPP_AtomName**
- **WRITTEN BY:** shape38 (Document Properties)
- **READ BY:** shape11 (Message in subprocess)
- **DEPENDENCY:** shape38 must execute BEFORE subprocess shape11

**Property: process.DPP_Payload**
- **WRITTEN BY:** shape38 (Document Properties)
- **READ BY:** shape15 (Message in subprocess)
- **DEPENDENCY:** shape38 must execute BEFORE subprocess shape15

**Property: process.DPP_ExecutionID**
- **WRITTEN BY:** shape38 (Document Properties)
- **READ BY:** shape11 (Message in subprocess)
- **DEPENDENCY:** shape38 must execute BEFORE subprocess shape11

**Property: process.DPP_File_Name**
- **WRITTEN BY:** shape38 (Document Properties)
- **READ BY:** shape6 (Document Properties in subprocess)
- **DEPENDENCY:** shape38 must execute BEFORE subprocess shape6

**Property: process.DPP_Subject**
- **WRITTEN BY:** shape38 (Document Properties)
- **READ BY:** shape6 (Document Properties in subprocess)
- **DEPENDENCY:** shape38 must execute BEFORE subprocess shape6

**Property: process.To_Email**
- **WRITTEN BY:** shape38 (Document Properties)
- **READ BY:** shape6 (Document Properties in subprocess)
- **DEPENDENCY:** shape38 must execute BEFORE subprocess shape6

**Property: process.DPP_HasAttachment**
- **WRITTEN BY:** shape38 (Document Properties)
- **READ BY:** shape4 (Decision in subprocess)
- **DEPENDENCY:** shape38 must execute BEFORE subprocess shape4

**Property: process.DPP_MailBody**
- **WRITTEN BY:** shape14 (Document Properties in subprocess), shape22 (Document Properties in subprocess)
- **READ BY:** shape6 (Document Properties in subprocess), shape20 (Document Properties in subprocess)
- **DEPENDENCY:** shape14/shape22 must execute BEFORE shape6/shape20

### Dependency Chains

**Main Process Chain:**
1. shape1 (Start) → shape38 (Set properties) → shape17 (Try/Catch)
2. shape17 (Try) → shape29 (Map) → shape8 (Set URL) → shape49 (Notify) → shape33 (HTTP Call) → shape2 (Decision)
3. shape2 (Decision TRUE) → shape34 (Map) → shape35 (Return Success)
4. shape2 (Decision FALSE) → shape44 (Decision)
5. shape44 (Decision TRUE - gzip) → shape45 (Decompress) → shape46 (Set Error) → shape47 (Map) → shape48 (Return Error)
6. shape44 (Decision FALSE - no gzip) → shape39 (Set Error) → shape40 (Map) → shape36 (Return Error)
7. shape17 (Catch) → shape20 (Branch)
8. shape20 (Branch Path 1) → shape19 (Set Error) → shape21 (Subprocess Email)
9. shape20 (Branch Path 2) → shape41 (Map) → shape43 (Return Error)

**Subprocess Chain:**
1. shape1 (Start) → shape2 (Try/Catch)
2. shape2 (Try) → shape4 (Decision)
3. shape4 (Decision TRUE - has attachment) → shape11 (Mail Body) → shape14 (Set MailBody) → shape15 (Payload) → shape6 (Set Mail Properties) → shape3 (Send Email) → shape5 (Stop)
4. shape4 (Decision FALSE - no attachment) → shape23 (Mail Body) → shape22 (Set MailBody) → shape20 (Set Mail Properties) → shape7 (Send Email) → shape9 (Stop)
5. shape2 (Catch) → shape10 (Exception)

### Independent Operations

- None (all operations are part of sequential dependency chains)

---

## 9. Control Flow Graph (Step 5)

### ✅ VALIDATION CHECKPOINT: Step 5 Complete

### Main Process Control Flow

| From Shape | To Shape | Identifier | Text | Notes |
|------------|----------|------------|------|-------|
| shape1 (start) | shape38 | default | | Entry point |
| shape38 (documentproperties) | shape17 | default | | Set input properties |
| shape17 (catcherrors) | shape29 | default | Try | Try block start |
| shape17 (catcherrors) | shape20 | error | Catch | Catch block start |
| shape29 (map) | shape8 | default | | Transform request |
| shape8 (documentproperties) | shape49 | default | | Set URL |
| shape49 (notify) | shape33 | default | | Log request |
| shape33 (connectoraction) | shape2 | default | | HTTP call to Oracle Fusion |
| shape2 (decision) | shape34 | true | True | HTTP status 20* |
| shape2 (decision) | shape44 | false | False | HTTP status non-20* |
| shape34 (map) | shape35 | default | | Map success response |
| shape35 (returndocuments) | - | - | | Return success |
| shape44 (decision) | shape45 | true | True | Content-Encoding is gzip |
| shape44 (decision) | shape39 | false | False | Content-Encoding is not gzip |
| shape45 (dataprocess) | shape46 | default | | Decompress gzip |
| shape46 (documentproperties) | shape47 | default | | Set error message |
| shape47 (map) | shape48 | default | | Map error response |
| shape48 (returndocuments) | - | - | | Return error |
| shape39 (documentproperties) | shape40 | default | | Set error message |
| shape40 (map) | shape36 | default | | Map error response |
| shape36 (returndocuments) | - | - | | Return error |
| shape20 (branch) | shape19 | 1 | 1 | Branch path 1 - Email notification |
| shape20 (branch) | shape41 | 2 | 2 | Branch path 2 - Return error |
| shape19 (documentproperties) | shape21 | default | | Set error message |
| shape21 (processcall) | - | - | | Call email subprocess |
| shape41 (map) | shape43 | default | | Map error response |
| shape43 (returndocuments) | - | - | | Return error |

### Subprocess Control Flow

| From Shape | To Shape | Identifier | Text | Notes |
|------------|----------|------------|------|-------|
| shape1 (start) | shape2 | default | | Subprocess entry |
| shape2 (catcherrors) | shape4 | default | Try | Try block start |
| shape2 (catcherrors) | shape10 | error | Catch | Catch block start |
| shape4 (decision) | shape11 | true | True | Has attachment |
| shape4 (decision) | shape23 | false | False | No attachment |
| shape11 (message) | shape14 | default | | Build email body |
| shape14 (documentproperties) | shape15 | default | | Set mail body property |
| shape15 (message) | shape6 | default | | Set payload |
| shape6 (documentproperties) | shape3 | default | | Set mail properties |
| shape3 (connectoraction) | shape5 | default | | Send email with attachment |
| shape5 (stop) | - | - | | Subprocess success return |
| shape23 (message) | shape22 | default | | Build email body |
| shape22 (documentproperties) | shape20 | default | | Set mail body property |
| shape20 (documentproperties) | shape7 | default | | Set mail properties |
| shape7 (connectoraction) | shape9 | default | | Send email without attachment |
| shape9 (stop) | - | - | | Subprocess success return |
| shape10 (exception) | - | - | | Subprocess exception |

### Convergence Points

**Main Process:**
- None (all paths terminate at return documents)

**Subprocess:**
- None (all paths terminate at stop or exception)

---

## 10. Decision Shape Analysis (Step 7)

### ✅ VALIDATION CHECKPOINT: Step 7 Complete

### Self-Check Results

- ✅ **Decision data sources identified:** YES
- ✅ **Decision types classified:** YES
- ✅ **Execution order verified:** YES
- ✅ **All decision paths traced:** YES
- ✅ **Decision patterns identified:** YES
- ✅ **Paths traced to termination:** YES

### Decision 1: HTTP Status 20 check (shape2)

**Shape ID:** shape2  
**Comparison:** wildcard  
**Value 1:** meta.base.applicationstatuscode (Track Property)  
**Value 2:** "20*" (Static)

**Data Source:** TRACK_PROPERTY (HTTP response status code from operation shape33)  
**Decision Type:** POST_OPERATION (checks response data from HTTP call)  
**Actual Execution Order:** Operation shape33 (HTTP Call) → Response → Decision shape2 → Route to success/error path

**TRUE Path:**
- **Destination:** shape34 (Map - Oracle Fusion Leave Response Map)
- **Termination:** shape35 (Return Documents - Success Response)
- **Type:** Success path - maps response and returns success

**FALSE Path:**
- **Destination:** shape44 (Decision - Check Response Content Type)
- **Termination:** shape36 or shape48 (Return Documents - Error Response)
- **Type:** Error path - checks compression and returns error

**Pattern:** Error Check (Success vs Failure based on HTTP status code)  
**Convergence Point:** None (paths terminate independently)  
**Early Exit:** No (both paths return documents)

**Business Logic:**
- If HTTP status code matches 20* (200, 201, 202, etc.) → Success path
- If HTTP status code is non-20* (400, 401, 404, 500, etc.) → Error path
- Success path maps Oracle Fusion response to D365 response format
- Error path checks if response is compressed and handles accordingly

### Decision 2: Check Response Content Type (shape44)

**Shape ID:** shape44  
**Comparison:** equals  
**Value 1:** dynamicdocument.DDP_RespHeader (Track Property)  
**Value 2:** "gzip" (Static)

**Data Source:** TRACK_PROPERTY (Content-Encoding header from HTTP response)  
**Decision Type:** POST_OPERATION (checks response header from HTTP call)  
**Actual Execution Order:** Operation shape33 (HTTP Call) → Response Header Captured → Decision shape2 (FALSE) → Decision shape44 → Route to decompression/error path

**TRUE Path:**
- **Destination:** shape45 (Data Process - Groovy script to decompress gzip)
- **Termination:** shape48 (Return Documents - Error Response)
- **Type:** Decompression path - decompresses gzip response, extracts error, returns error

**FALSE Path:**
- **Destination:** shape39 (Document Properties - Extract error message)
- **Termination:** shape36 (Return Documents - Error Response)
- **Type:** Direct error path - extracts error message, returns error

**Pattern:** Conditional Logic (Handle compressed vs uncompressed error responses)  
**Convergence Point:** None (both paths terminate at return documents)  
**Early Exit:** No (both paths return documents)

**Business Logic:**
- If Content-Encoding header is "gzip" → Decompress response first, then extract error
- If Content-Encoding header is not "gzip" → Extract error directly
- Both paths ultimately return error response to caller

### Decision 3: Attachment_Check (shape4 in subprocess)

**Shape ID:** shape4 (in subprocess a85945c5-3004-42b9-80b1-104f465cd1fb)  
**Comparison:** equals  
**Value 1:** process.DPP_HasAttachment (Process Property)  
**Value 2:** "Y" (Static)

**Data Source:** PROCESS_PROPERTY (set by main process shape38)  
**Decision Type:** PRE_FILTER (checks input parameter to determine email format)  
**Actual Execution Order:** Main process sets DPP_HasAttachment → Subprocess reads property → Decision shape4 → Route to attachment/no-attachment path

**TRUE Path:**
- **Destination:** shape11 (Message - Build email body with attachment)
- **Termination:** shape5 (Stop - continue=true, subprocess success return)
- **Type:** Email with attachment path

**FALSE Path:**
- **Destination:** shape23 (Message - Build email body without attachment)
- **Termination:** shape9 (Stop - continue=true, subprocess success return)
- **Type:** Email without attachment path

**Pattern:** Conditional Logic (Optional Processing based on configuration)  
**Convergence Point:** None (both paths terminate at stop)  
**Early Exit:** No (both paths complete successfully)

**Business Logic:**
- If DPP_HasAttachment is "Y" → Send email with payload as attachment
- If DPP_HasAttachment is not "Y" → Send email without attachment
- Both paths send error notification email, only difference is attachment

---

## 11. Subprocess Analysis (Step 7a)

### ✅ VALIDATION CHECKPOINT: Step 7a Complete

### Subprocess: (Sub) Office 365 Email (a85945c5-3004-42b9-80b1-104f465cd1fb)

**Subprocess ID:** a85945c5-3004-42b9-80b1-104f465cd1fb  
**Subprocess Name:** (Sub) Office 365 Email  
**Purpose:** Send error notification emails via Office 365 SMTP

### Internal Flow

1. **START** (shape1) → Entry point
2. **Try/Catch** (shape2) → Error handling wrapper
3. **Try Path:**
   - **Decision** (shape4) → Check if attachment is required (DPP_HasAttachment == "Y")
   - **TRUE Path (Has Attachment):**
     - shape11 (Message) → Build HTML email body with error details
     - shape14 (Document Properties) → Set DPP_MailBody from email body
     - shape15 (Message) → Set payload (DPP_Payload) as attachment content
     - shape6 (Document Properties) → Set mail properties (From, To, Subject, Body, Filename)
     - shape3 (Connector Action) → Send email with attachment (operation af07502a)
     - shape5 (Stop continue=true) → Success return
   - **FALSE Path (No Attachment):**
     - shape23 (Message) → Build HTML email body with error details
     - shape22 (Document Properties) → Set DPP_MailBody from email body
     - shape20 (Document Properties) → Set mail properties (From, To, Subject, Body)
     - shape7 (Connector Action) → Send email without attachment (operation 15a72a21)
     - shape9 (Stop continue=true) → Success return
4. **Catch Path:**
   - shape10 (Exception) → Throw exception with error message

### Return Paths

| Return Label | Shape ID | Type | Condition |
|--------------|----------|------|-----------|
| SUCCESS | shape5, shape9 | Stop (continue=true) | Email sent successfully (both attachment paths) |
| EXCEPTION | shape10 | Exception | Error during email sending |

### Main Process Return Mapping

The subprocess is called by shape21 (ProcessCall) in the main process. Since the subprocess uses Stop(continue=true) for success, the main process continues execution after the subprocess completes. There are no explicit return path mappings in the ProcessCall configuration, indicating the subprocess always returns to the same point in the main process.

### Properties Written by Subprocess

| Property Name | Written By | Purpose |
|---------------|------------|---------|
| process.DPP_MailBody | shape14, shape22 | Store email body content |
| connector.mail.fromAddress | shape6, shape20 | Set email sender |
| connector.mail.toAddress | shape6, shape20 | Set email recipients |
| connector.mail.subject | shape6, shape20 | Set email subject |
| connector.mail.body | shape6, shape20 | Set email body |
| connector.mail.filename | shape6 | Set attachment filename (only in attachment path) |

### Properties Read by Subprocess

| Property Name | Read By | Purpose |
|---------------|---------|---------|
| process.DPP_HasAttachment | shape4 | Determine if attachment is required |
| process.To_Email | shape6, shape20 | Email recipients |
| process.DPP_Subject | shape6, shape20 | Email subject |
| process.DPP_File_Name | shape6 | Attachment filename |
| process.DPP_Process_Name | shape11, shape23 | Display in email body |
| process.DPP_AtomName | shape11, shape23 | Display in email body |
| process.DPP_ExecutionID | shape11, shape23 | Display in email body |
| process.DPP_ErrorMessage | shape11, shape23 | Display in email body |
| process.DPP_Payload | shape15 | Attachment content |
| process.DPP_MailBody | shape6, shape20 | Email body content |

### Error Paths

- **Try/Catch Error:** If any error occurs during email sending, shape10 (Exception) throws an exception with the error message from meta.base.catcherrorsmessage

---

## 12. Branch Shape Analysis (Step 8)

### ✅ VALIDATION CHECKPOINT: Step 8 Complete

### Self-Check Results

- ✅ **Classification completed:** YES
- ✅ **Assumption check:** NO (analyzed dependencies)
- ✅ **Properties extracted:** YES
- ✅ **Dependency graph built:** YES
- ✅ **Topological sort applied:** YES (for sequential branch)

### Branch: Error Handling Branch (shape20)

**Shape ID:** shape20  
**Number of Paths:** 2  
**Location:** Main process, Catch block of Try/Catch (shape17)

### Step 1: Properties Analysis

**Path 1 (shape19 → shape21):**
- **Properties READ:** 
  - process.DPP_Process_Name (by subprocess shape11)
  - process.DPP_AtomName (by subprocess shape11)
  - process.DPP_Payload (by subprocess shape15)
  - process.DPP_ExecutionID (by subprocess shape11)
  - process.DPP_File_Name (by subprocess shape6)
  - process.DPP_Subject (by subprocess shape6)
  - process.To_Email (by subprocess shape6)
  - process.DPP_HasAttachment (by subprocess shape4)
  - process.DPP_ErrorMessage (by subprocess shape11)
- **Properties WRITTEN:**
  - process.DPP_ErrorMessage (by shape19 from meta.base.catcherrorsmessage)
  - process.DPP_MailBody (by subprocess shape14/shape22)
  - connector.mail.* properties (by subprocess shape6/shape20)

**Path 2 (shape41 → shape43):**
- **Properties READ:**
  - process.DPP_ErrorMessage (by map f46b845a function PropertyGet)
- **Properties WRITTEN:** None

### Step 2: Dependency Graph

**Dependencies:**
- Path 2 reads process.DPP_ErrorMessage
- Path 1 writes process.DPP_ErrorMessage (shape19)
- **DEPENDENCY:** Path 1 must execute BEFORE Path 2 (Path 2 depends on Path 1)

### Step 3: Classification

**Classification:** SEQUENTIAL

**Reasoning:**
1. Path 2 reads process.DPP_ErrorMessage which Path 1 writes
2. Data dependency exists: Path 2 depends on Path 1
3. Path 1 contains API call (email operation in subprocess)
4. **🚨 CRITICAL RULE:** ALL API CALLS ARE SEQUENTIAL - Path 1 contains email operation (API call), therefore classification is ALWAYS SEQUENTIAL

### Step 4: Topological Sort Order

**Execution Order:** Path 1 → Path 2

**Reasoning:**
1. Path 1 writes process.DPP_ErrorMessage (shape19)
2. Path 2 reads process.DPP_ErrorMessage (map function)
3. Path 1 must execute first to populate the error message
4. Path 1 contains API call (email operation), which must execute sequentially
5. Topological sort: Path 1 has no dependencies, Path 2 depends on Path 1

### Step 5: Path Termination

**Path 1 Termination:**
- **Shape:** shape21 (ProcessCall - Subprocess Email)
- **Type:** Subprocess call (returns to main process after completion)
- **Note:** Subprocess terminates at shape5/shape9 (Stop continue=true) or shape10 (Exception)

**Path 2 Termination:**
- **Shape:** shape43 (Return Documents - Error Response)
- **Type:** Return Documents (process termination)

### Step 6: Convergence Points

**Convergence:** None (Path 1 calls subprocess and returns, Path 2 terminates process)

### Step 7: Execution Continuation

**Execution Continues From:** Path 2 terminates the process (Return Documents), so no continuation

### Step 8: Complete Analysis

**Branch Analysis Summary:**
- **Shape ID:** shape20
- **Number of Paths:** 2
- **Classification:** SEQUENTIAL
- **Dependency Order:** Path 1 → Path 2
- **Path 1 Terminal:** Subprocess call (shape21)
- **Path 2 Terminal:** Return Documents (shape43)
- **Convergence Points:** None
- **Execution Continues From:** None (process terminates)

**Business Logic:**
- Path 1: Send error notification email to support team (subprocess call)
- Path 2: Return error response to caller
- Sequential execution ensures email is sent before returning error response
- Error message is captured in Path 1 and used in Path 2 for response mapping

---

## 13. Execution Order (Step 9)

### ✅ VALIDATION CHECKPOINT: Step 9 Complete

### Self-Check Results

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

### Business Logic Flow (Step 0 - MUST BE FIRST)

**Operation 1: Create Leave Oracle Fusion OP (8f709c2b-e63f-4d5f-9374-2932ed70415d)**
- **Purpose:** Web Service Server - Entry point for leave creation requests from D365
- **Outputs:** Input document (leave request JSON)
- **Dependent Operations:** All subsequent operations depend on this input
- **Business Flow:** This operation MUST execute FIRST (entry point, produces input data for all operations)

**Operation 2: Leave Oracle Fusion Create (6e8920fd-af5a-430b-a1d9-9fde7ac29a12)**
- **Purpose:** HTTP POST to Oracle Fusion HCM to create leave absence
- **Outputs:** 
  - HTTP response body (Oracle Fusion leave response JSON)
  - HTTP status code (meta.base.applicationstatuscode)
  - Response headers (Content-Encoding → dynamicdocument.DDP_RespHeader)
- **Dependent Operations:** 
  - Decision shape2 (checks HTTP status code)
  - Decision shape44 (checks Content-Encoding header)
  - Map shape34 (maps success response)
  - Document Properties shape39/shape46 (extract error messages)
- **Business Flow:** This operation MUST execute AFTER request transformation and URL setup, BEFORE response processing

**Operation 3: Email w Attachment (af07502a-fafd-4976-a691-45d51a33b549)**
- **Purpose:** Send error notification email with payload attachment
- **Outputs:** Email sent confirmation
- **Dependent Operations:** None
- **Business Flow:** This operation executes in subprocess when DPP_HasAttachment is "Y" and error occurs

**Operation 4: Email W/O Attachment (15a72a21-9b57-49a1-a8ed-d70367146644)**
- **Purpose:** Send error notification email without attachment
- **Outputs:** Email sent confirmation
- **Dependent Operations:** None
- **Business Flow:** This operation executes in subprocess when DPP_HasAttachment is not "Y" and error occurs

### Actual Business Flow

**Main Success Flow:**
1. **Receive Request** (shape1) → Entry point receives leave request from D365
2. **Set Input Properties** (shape38) → Capture execution metadata (process name, atom name, execution ID, payload, etc.)
3. **Transform Request** (shape29) → Map D365 request to Oracle Fusion format (employeeNumber → personNumber, etc.)
4. **Set URL** (shape8) → Set Oracle Fusion API endpoint URL from defined property
5. **Log Request** (shape49) → Notify/log the request payload
6. **Call Oracle Fusion** (shape33) → HTTP POST to Oracle Fusion HCM to create leave absence
7. **Check HTTP Status** (shape2) → Verify if HTTP status code is 20* (success)
8. **Map Success Response** (shape34) → Transform Oracle Fusion response to D365 response format
9. **Return Success** (shape35) → Return success response to D365 with personAbsenceEntryId

**Error Flow 1: HTTP Non-20* Status (No Gzip)**
1. Steps 1-6 (same as success flow)
2. **Check HTTP Status** (shape2) → HTTP status code is non-20* (FALSE path)
3. **Check Content-Encoding** (shape44) → Content-Encoding is not "gzip" (FALSE path)
4. **Extract Error Message** (shape39) → Set DPP_ErrorMessage from meta.base.applicationstatusmessage
5. **Map Error Response** (shape40) → Transform error message to D365 error response format
6. **Return Error** (shape36) → Return error response to D365

**Error Flow 2: HTTP Non-20* Status (Gzip Compressed)**
1. Steps 1-6 (same as success flow)
2. **Check HTTP Status** (shape2) → HTTP status code is non-20* (FALSE path)
3. **Check Content-Encoding** (shape44) → Content-Encoding is "gzip" (TRUE path)
4. **Decompress Response** (shape45) → Groovy script to decompress gzip response
5. **Extract Error Message** (shape46) → Set DPP_ErrorMessage from decompressed response
6. **Map Error Response** (shape47) → Transform error message to D365 error response format
7. **Return Error** (shape48) → Return error response to D365

**Error Flow 3: Try/Catch Exception**
1. **Receive Request** (shape1) → Entry point receives leave request from D365
2. **Set Input Properties** (shape38) → Capture execution metadata
3. **Try/Catch** (shape17) → Catch any exception during processing
4. **Exception Caught** → Error occurs (connection timeout, transformation error, etc.)
5. **Branch** (shape20) → Split into two sequential paths
6. **Path 1: Send Email Notification**
   - **Extract Error Message** (shape19) → Set DPP_ErrorMessage from meta.base.catcherrorsmessage
   - **Call Email Subprocess** (shape21) → Send error notification email to support team
7. **Path 2: Return Error Response**
   - **Map Error Response** (shape41) → Transform error message to D365 error response format
   - **Return Error** (shape43) → Return error response to D365

### Execution Order List

**Main Process:**

1. shape1 (START) - Entry point
2. shape38 (Document Properties) - Set input properties
3. shape17 (Try/Catch) - Error handling wrapper
4. **TRY PATH:**
   - shape29 (Map) - Transform D365 request to Oracle Fusion format
   - shape8 (Document Properties) - Set URL
   - shape49 (Notify) - Log request
   - shape33 (Connector Action) - HTTP POST to Oracle Fusion
   - shape2 (Decision) - Check HTTP status code
   - **IF TRUE (HTTP 20*):**
     - shape34 (Map) - Map success response
     - shape35 (Return Documents) - Return success [HTTP 200]
   - **IF FALSE (HTTP non-20*):**
     - shape44 (Decision) - Check Content-Encoding
     - **IF TRUE (gzip):**
       - shape45 (Data Process) - Decompress gzip
       - shape46 (Document Properties) - Extract error message
       - shape47 (Map) - Map error response
       - shape48 (Return Documents) - Return error [HTTP 400]
     - **IF FALSE (no gzip):**
       - shape39 (Document Properties) - Extract error message
       - shape40 (Map) - Map error response
       - shape36 (Return Documents) - Return error [HTTP 400]
5. **CATCH PATH:**
   - shape20 (Branch) - SEQUENTIAL execution
   - **Path 1 (Email Notification):**
     - shape19 (Document Properties) - Extract error message
     - shape21 (Process Call) - Call email subprocess
   - **Path 2 (Return Error):**
     - shape41 (Map) - Map error response
     - shape43 (Return Documents) - Return error [HTTP 400]

**Subprocess: (Sub) Office 365 Email**

1. shape1 (START) - Subprocess entry
2. shape2 (Try/Catch) - Error handling wrapper
3. **TRY PATH:**
   - shape4 (Decision) - Check if attachment is required
   - **IF TRUE (has attachment):**
     - shape11 (Message) - Build email body
     - shape14 (Document Properties) - Set DPP_MailBody
     - shape15 (Message) - Set payload
     - shape6 (Document Properties) - Set mail properties
     - shape3 (Connector Action) - Send email with attachment
     - shape5 (Stop continue=true) - Success return
   - **IF FALSE (no attachment):**
     - shape23 (Message) - Build email body
     - shape22 (Document Properties) - Set DPP_MailBody
     - shape20 (Document Properties) - Set mail properties
     - shape7 (Connector Action) - Send email without attachment
     - shape9 (Stop continue=true) - Success return
4. **CATCH PATH:**
   - shape10 (Exception) - Throw exception

### Dependency Verification

**Reference to Step 4 (Data Dependency Graph):**

All data dependencies are satisfied in the execution order:

1. **dynamicdocument.URL:** shape8 writes → shape33 reads ✅
2. **process.DPP_ErrorMessage:** shape19/shape39/shape46 write → shape11/map reads ✅
3. **process.DPP_Process_Name:** shape38 writes → shape11 reads ✅
4. **process.DPP_AtomName:** shape38 writes → shape11 reads ✅
5. **process.DPP_Payload:** shape38 writes → shape15 reads ✅
6. **process.DPP_ExecutionID:** shape38 writes → shape11 reads ✅
7. **process.DPP_File_Name:** shape38 writes → shape6 reads ✅
8. **process.DPP_Subject:** shape38 writes → shape6 reads ✅
9. **process.To_Email:** shape38 writes → shape6 reads ✅
10. **process.DPP_HasAttachment:** shape38 writes → shape4 reads ✅
11. **process.DPP_MailBody:** shape14/shape22 write → shape6/shape20 read ✅

**PROOF:** All property reads happen AFTER property writes, ensuring correct execution order.

### Branch Execution Order

**Reference to Step 8 (Branch Analysis):**

**Branch shape20 (Error Handling Branch):**
- **Classification:** SEQUENTIAL
- **Execution Order:** Path 1 (Email Notification) → Path 2 (Return Error)
- **Reasoning:** 
  - Path 2 reads process.DPP_ErrorMessage which Path 1 writes
  - Path 1 contains API call (email operation), which must execute sequentially
  - Topological sort confirms Path 1 must execute before Path 2

### Decision Path Tracing

**Reference to Step 7 (Decision Analysis):**

**Decision shape2 (HTTP Status 20 check):**
- **TRUE Path:** shape34 → shape35 (Return Success)
- **FALSE Path:** shape44 → [shape45 → shape46 → shape47 → shape48] OR [shape39 → shape40 → shape36] (Return Error)
- **Convergence:** None (paths terminate independently)

**Decision shape44 (Check Response Content Type):**
- **TRUE Path:** shape45 → shape46 → shape47 → shape48 (Decompress and Return Error)
- **FALSE Path:** shape39 → shape40 → shape36 (Return Error)
- **Convergence:** None (paths terminate independently)

**Decision shape4 (Attachment_Check in subprocess):**
- **TRUE Path:** shape11 → shape14 → shape15 → shape6 → shape3 → shape5 (Send Email with Attachment)
- **FALSE Path:** shape23 → shape22 → shape20 → shape7 → shape9 (Send Email without Attachment)
- **Convergence:** None (paths terminate independently)

---

## 14. Sequence Diagram (Step 10)

### ✅ VALIDATION CHECKPOINT: Step 10 Complete

**📋 NOTE:** Detailed request/response JSON examples are documented in:
- **Section 6: HTTP Status Codes and Return Path Responses** - For response JSON with populated fields for return paths
- **Section 2 & 3: Input/Response Structure Analysis** - For detailed request/response JSON examples

### Sequence Diagram

**Based on dependency graph in Step 4, decision analysis in Step 7, control flow graph in Step 5, branch analysis in Step 8, and execution order in Step 9.**

```
START (shape1)
 |
 ├─→ shape38: Set Input Properties (Document Properties)
 |   └─→ WRITES: [process.DPP_Process_Name, process.DPP_AtomName, process.DPP_Payload, 
 |                 process.DPP_ExecutionID, process.DPP_File_Name, process.DPP_Subject, 
 |                 process.To_Email, process.DPP_HasAttachment]
 |
 ├─→ shape17: Try/Catch (Error Handling Wrapper)
 |   |
 |   ├─→ TRY PATH:
 |   |   |
 |   |   ├─→ shape29: Transform Request (Map - Leave Create Map)
 |   |   |   └─→ READS: [Input document]
 |   |   |   └─→ WRITES: [Transformed document]
 |   |   |   └─→ Transformation: employeeNumber → personNumber, absenceStatusCode → absenceStatusCd, etc.
 |   |   |
 |   |   ├─→ shape8: Set URL (Document Properties)
 |   |   |   └─→ READS: [PP_HCM_LeaveCreate_Properties.Resource_Path]
 |   |   |   └─→ WRITES: [dynamicdocument.URL]
 |   |   |
 |   |   ├─→ shape49: Log Request (Notify)
 |   |   |   └─→ READS: [Current document]
 |   |   |
 |   |   ├─→ shape33: Leave Oracle Fusion Create (Downstream HTTP POST)
 |   |   |   └─→ READS: [dynamicdocument.URL, transformed document]
 |   |   |   └─→ WRITES: [HTTP response, meta.base.applicationstatuscode, dynamicdocument.DDP_RespHeader]
 |   |   |   └─→ HTTP: [Expected: 200/201/202, Error: 400/401/403/404/500]
 |   |   |   └─→ Endpoint: POST {dynamicdocument.URL}
 |   |   |
 |   |   ├─→ shape2: Decision - HTTP Status 20 check
 |   |   |   └─→ READS: [meta.base.applicationstatuscode]
 |   |   |   └─→ Condition: meta.base.applicationstatuscode wildcard matches "20*"?
 |   |   |   |
 |   |   |   ├─→ IF TRUE (HTTP 20*) → SUCCESS PATH:
 |   |   |   |   |
 |   |   |   |   ├─→ shape34: Map Success Response (Map - Oracle Fusion Leave Response Map)
 |   |   |   |   |   └─→ READS: [HTTP response body]
 |   |   |   |   |   └─→ WRITES: [D365 response document]
 |   |   |   |   |   └─→ Transformation: personAbsenceEntryId → personAbsenceEntryId, 
 |   |   |   |   |                      + defaults (status="success", message="Data successfully sent to Oracle Fusion", success="true")
 |   |   |   |   |
 |   |   |   |   └─→ shape35: Return Documents [HTTP: 200] [Success] [SUCCESS]
 |   |   |   |       └─→ Response: { "status": "success", "message": "Data successfully sent to Oracle Fusion", 
 |   |   |   |                       "personAbsenceEntryId": 300000123456789, "success": "true" }
 |   |   |   |
 |   |   |   └─→ IF FALSE (HTTP non-20*) → ERROR PATH:
 |   |   |       |
 |   |   |       ├─→ shape44: Decision - Check Response Content Type
 |   |   |           └─→ READS: [dynamicdocument.DDP_RespHeader]
 |   |   |           └─→ Condition: dynamicdocument.DDP_RespHeader equals "gzip"?
 |   |   |           |
 |   |   |           ├─→ IF TRUE (gzip) → GZIP ERROR PATH:
 |   |   |           |   |
 |   |   |           |   ├─→ shape45: Decompress Gzip (Data Process - Groovy Script)
 |   |   |           |   |   └─→ READS: [HTTP response body (gzip compressed)]
 |   |   |           |   |   └─→ WRITES: [Decompressed response body]
 |   |   |           |   |   └─→ Script: GZIPInputStream decompression
 |   |   |           |   |
 |   |   |           |   ├─→ shape46: Extract Error Message (Document Properties)
 |   |   |           |   |   └─→ READS: [Current document (decompressed)]
 |   |   |           |   |   └─→ WRITES: [process.DPP_ErrorMessage]
 |   |   |           |   |
 |   |   |           |   ├─→ shape47: Map Error Response (Map - Leave Error Map)
 |   |   |           |   |   └─→ READS: [process.DPP_ErrorMessage]
 |   |   |           |   |   └─→ WRITES: [D365 error response document]
 |   |   |           |   |   └─→ Transformation: DPP_ErrorMessage → message, 
 |   |   |           |   |                      + defaults (status="failure", success="false")
 |   |   |           |   |
 |   |   |           |   └─→ shape48: Return Documents [HTTP: 400] [Error] [EARLY EXIT]
 |   |   |           |       └─→ Response: { "status": "failure", "message": "HTTP 500: Internal Server Error - Oracle Fusion service unavailable", 
 |   |   |           |                       "success": "false" }
 |   |   |           |
 |   |   |           └─→ IF FALSE (no gzip) → DIRECT ERROR PATH:
 |   |   |               |
 |   |   |               ├─→ shape39: Extract Error Message (Document Properties)
 |   |   |               |   └─→ READS: [meta.base.applicationstatusmessage]
 |   |   |               |   └─→ WRITES: [process.DPP_ErrorMessage]
 |   |   |               |
 |   |   |               ├─→ shape40: Map Error Response (Map - Leave Error Map)
 |   |   |               |   └─→ READS: [process.DPP_ErrorMessage]
 |   |   |               |   └─→ WRITES: [D365 error response document]
 |   |   |               |   └─→ Transformation: DPP_ErrorMessage → message, 
 |   |   |               |                      + defaults (status="failure", success="false")
 |   |   |               |
 |   |   |               └─→ shape36: Return Documents [HTTP: 400] [Error] [EARLY EXIT]
 |   |   |                   └─→ Response: { "status": "failure", "message": "HTTP 400: Bad Request - Invalid absence type", 
 |   |   |                                   "success": "false" }
 |   |
 |   └─→ CATCH PATH (Exception):
 |       |
 |       ├─→ shape20: Branch (SEQUENTIAL - API call in Path 1)
 |           |
 |           ├─→ Path 1 (Email Notification) → EXECUTES FIRST:
 |           |   |
 |           |   ├─→ shape19: Extract Error Message (Document Properties)
 |           |   |   └─→ READS: [meta.base.catcherrorsmessage]
 |           |   |   └─→ WRITES: [process.DPP_ErrorMessage]
 |           |   |
 |           |   └─→ shape21: Call Email Subprocess (Process Call)
 |           |       └─→ READS: [process.DPP_Process_Name, process.DPP_AtomName, process.DPP_Payload, 
 |           |                   process.DPP_ExecutionID, process.DPP_File_Name, process.DPP_Subject, 
 |           |                   process.To_Email, process.DPP_HasAttachment, process.DPP_ErrorMessage]
 |           |       └─→ SUBPROCESS: (Sub) Office 365 Email
 |           |           |
 |           |           ├─→ START (shape1)
 |           |           |
 |           |           ├─→ shape2: Try/Catch (Error Handling Wrapper)
 |           |           |   |
 |           |           |   ├─→ TRY PATH:
 |           |           |   |   |
 |           |           |   |   ├─→ shape4: Decision - Attachment_Check
 |           |           |   |   |   └─→ READS: [process.DPP_HasAttachment]
 |           |           |   |   |   └─→ Condition: process.DPP_HasAttachment equals "Y"?
 |           |           |   |   |   |
 |           |           |   |   |   ├─→ IF TRUE (has attachment) → WITH ATTACHMENT PATH:
 |           |           |   |   |   |   |
 |           |           |   |   |   |   ├─→ shape11: Build Email Body (Message)
 |           |           |   |   |   |   |   └─→ READS: [process.DPP_Process_Name, process.DPP_AtomName, 
 |           |           |   |   |   |   |                 process.DPP_ExecutionID, process.DPP_ErrorMessage]
 |           |           |   |   |   |   |   └─→ WRITES: [HTML email body document]
 |           |           |   |   |   |   |
 |           |           |   |   |   |   ├─→ shape14: Set Mail Body Property (Document Properties)
 |           |           |   |   |   |   |   └─→ READS: [Current document (email body)]
 |           |           |   |   |   |   |   └─→ WRITES: [process.DPP_MailBody]
 |           |           |   |   |   |   |
 |           |           |   |   |   |   ├─→ shape15: Set Payload (Message)
 |           |           |   |   |   |   |   └─→ READS: [process.DPP_Payload]
 |           |           |   |   |   |   |   └─→ WRITES: [Payload document for attachment]
 |           |           |   |   |   |   |
 |           |           |   |   |   |   ├─→ shape6: Set Mail Properties (Document Properties)
 |           |           |   |   |   |   |   └─→ READS: [PP_Office365_Email.From_Email, process.To_Email, 
 |           |           |   |   |   |   |                 PP_Office365_Email.Environment, process.DPP_Subject, 
 |           |           |   |   |   |   |                 process.DPP_MailBody, process.DPP_File_Name]
 |           |           |   |   |   |   |   └─→ WRITES: [connector.mail.fromAddress, connector.mail.toAddress, 
 |           |           |   |   |   |   |                  connector.mail.subject, connector.mail.body, connector.mail.filename]
 |           |           |   |   |   |   |
 |           |           |   |   |   |   ├─→ shape3: Email w Attachment (Downstream Mail Send)
 |           |           |   |   |   |   |   └─→ READS: [connector.mail.*, payload document]
 |           |           |   |   |   |   |   └─→ WRITES: [Email sent confirmation]
 |           |           |   |   |   |   |   └─→ HTTP: [Expected: 250 (SMTP success), Error: SMTP errors]
 |           |           |   |   |   |   |
 |           |           |   |   |   |   └─→ shape5: Stop (continue=true) [Subprocess Success Return]
 |           |           |   |   |   |
 |           |           |   |   |   └─→ IF FALSE (no attachment) → WITHOUT ATTACHMENT PATH:
 |           |           |   |   |       |
 |           |           |   |   |       ├─→ shape23: Build Email Body (Message)
 |           |           |   |   |       |   └─→ READS: [process.DPP_Process_Name, process.DPP_AtomName, 
 |           |           |   |   |       |                 process.DPP_ExecutionID, process.DPP_ErrorMessage]
 |           |           |   |   |       |   └─→ WRITES: [HTML email body document]
 |           |           |   |   |       |
 |           |           |   |   |       ├─→ shape22: Set Mail Body Property (Document Properties)
 |           |           |   |   |       |   └─→ READS: [Current document (email body)]
 |           |           |   |   |       |   └─→ WRITES: [process.DPP_MailBody]
 |           |           |   |   |       |
 |           |           |   |   |       ├─→ shape20: Set Mail Properties (Document Properties)
 |           |           |   |   |       |   └─→ READS: [PP_Office365_Email.From_Email, process.To_Email, 
 |           |           |   |   |       |                 PP_Office365_Email.Environment, process.DPP_Subject, 
 |           |           |   |   |       |                 process.DPP_MailBody]
 |           |           |   |   |       |   └─→ WRITES: [connector.mail.fromAddress, connector.mail.toAddress, 
 |           |           |   |   |       |                  connector.mail.subject, connector.mail.body]
 |           |           |   |   |       |
 |           |           |   |   |       ├─→ shape7: Email W/O Attachment (Downstream Mail Send)
 |           |           |   |   |       |   └─→ READS: [connector.mail.*]
 |           |           |   |   |       |   └─→ WRITES: [Email sent confirmation]
 |           |           |   |   |       |   └─→ HTTP: [Expected: 250 (SMTP success), Error: SMTP errors]
 |           |           |   |   |       |
 |           |           |   |   |       └─→ shape9: Stop (continue=true) [Subprocess Success Return]
 |           |           |   |
 |           |           |   └─→ CATCH PATH (Exception):
 |           |           |       |
 |           |           |       └─→ shape10: Exception (Throw Exception)
 |           |           |           └─→ READS: [meta.base.catcherrorsmessage]
 |           |           |           └─→ WRITES: [Exception thrown]
 |           |           |
 |           |           └─→ END SUBPROCESS
 |           |
 |           └─→ Path 2 (Return Error) → EXECUTES SECOND (depends on Path 1 DPP_ErrorMessage):
 |               |
 |               ├─→ shape41: Map Error Response (Map - Leave Error Map)
 |               |   └─→ READS: [process.DPP_ErrorMessage]
 |               |   └─→ WRITES: [D365 error response document]
 |               |   └─→ Transformation: DPP_ErrorMessage → message, 
 |               |                      + defaults (status="failure", success="false")
 |               |
 |               └─→ shape43: Return Documents [HTTP: 400] [Error] [EARLY EXIT]
 |                   └─→ Response: { "status": "failure", "message": "Connection timeout to Oracle Fusion HCM", 
 |                                   "success": "false" }
 |
 └─→ END
```

**Note:** Detailed request/response JSON examples for all operations and return paths are documented in Section 6 (HTTP Status Codes and Return Path Responses) and Sections 2 & 3 (Input/Response Structure Analysis).

---

## 15. System Layer Identification

### Third-Party Systems

| System Name | System Type | Purpose | Operations |
|-------------|-------------|---------|------------|
| Oracle Fusion HCM | Cloud ERP (Oracle) | Human Capital Management - Leave/Absence Management | Leave Oracle Fusion Create (HTTP POST) |
| Office 365 SMTP | Email Service (Microsoft) | Email notifications for error reporting | Email w Attachment, Email W/O Attachment |
| D365 (Dynamics 365) | Cloud ERP (Microsoft) | Source system for leave requests | Entry point (Web Service Server) |

### System Layer APIs Required

**1. Oracle Fusion HCM System API**
- **Purpose:** Create leave absence entries in Oracle Fusion HCM
- **Endpoint:** POST /hcmRestApi/resources/11.13.18.05/absences
- **Authentication:** Basic Auth
- **Request Format:** JSON
- **Response Format:** JSON
- **Error Handling:** HTTP status codes (400, 401, 403, 404, 500), gzip compression support

**2. Office 365 SMTP System API**
- **Purpose:** Send error notification emails
- **Protocol:** SMTP (Office 365)
- **Authentication:** SMTP AUTH
- **Features:** Attachment support, HTML email body
- **Error Handling:** SMTP error codes

---

## 16. Function Exposure Decision Table

### ✅ VALIDATION CHECKPOINT: Function Exposure Decision Table Complete

**🚨 CRITICAL: This table prevents function explosion by ensuring only necessary functions are exposed.**

| Function Name | Expose as Azure Function? | Reasoning | Alternative |
|---------------|---------------------------|-----------|-------------|
| HCM Leave Create | ✅ YES | **PRIMARY FUNCTION** - Main business process for creating leave absences in Oracle Fusion HCM from D365 requests. This is the entry point for the integration. | N/A - This is the core business function |
| Office 365 Email Notification | ❌ NO | **INTERNAL UTILITY** - Error notification subprocess used only within the main process. Not a standalone business function. | Keep as internal helper method/service called by main function |
| Oracle Fusion Leave Create API Call | ❌ NO | **SYSTEM LAYER** - HTTP client call to Oracle Fusion. Should be encapsulated in System Layer API, not exposed as separate function. | Implement as System Layer API client (OracleFusionHcmClient) |
| Email Send (with/without attachment) | ❌ NO | **SYSTEM LAYER** - SMTP client call to Office 365. Should be encapsulated in System Layer API, not exposed as separate function. | Implement as System Layer API client (Office365EmailClient) |
| Request Transformation (D365 → Oracle) | ❌ NO | **INTERNAL LOGIC** - Data mapping logic within the main function. Not a standalone business function. | Implement as internal mapping/transformation logic in main function |
| Response Transformation (Oracle → D365) | ❌ NO | **INTERNAL LOGIC** - Data mapping logic within the main function. Not a standalone business function. | Implement as internal mapping/transformation logic in main function |
| Error Response Mapping | ❌ NO | **INTERNAL LOGIC** - Error handling logic within the main function. Not a standalone business function. | Implement as internal error handling logic in main function |
| Gzip Decompression | ❌ NO | **INTERNAL UTILITY** - Response decompression logic within the main function. Not a standalone business function. | Implement as internal utility method in main function |

### Function Exposure Summary

**EXPOSE AS AZURE FUNCTIONS:** 1
- HCM Leave Create (Main Process Layer Function)

**DO NOT EXPOSE:** 7
- All other components are internal logic, utilities, or System Layer API calls

### Rationale

**Why only 1 function?**
- The Boomi process has a single entry point (Web Service Server operation)
- All other operations are internal processing steps or downstream API calls
- Following API-Led Architecture principles: Process Layer exposes business functions, System Layer encapsulates downstream systems
- Prevents function explosion by keeping internal logic and System Layer calls as implementation details

**Architecture Pattern:**
```
Process Layer Function: HCM Leave Create
  ├─ Internal Logic: Request transformation, response mapping, error handling
  ├─ System Layer API: OracleFusionHcmClient (Oracle Fusion HCM API calls)
  └─ System Layer API: Office365EmailClient (Email notification calls)
```

---

## 17. Validation Checklist

### ✅ ALL VALIDATION CHECKPOINTS COMPLETE

### Data Dependencies
- [x] All property WRITES identified (Step 2)
- [x] All property READS identified (Step 3)
- [x] Dependency graph built (Step 4)
- [x] Execution order satisfies all dependencies (no read-before-write)

### Decision Analysis
- [x] ALL decision shapes inventoried (Step 7)
- [x] BOTH TRUE and FALSE paths traced to termination
- [x] Pattern type identified for each decision
- [x] Early exits identified and documented
- [x] Convergence points identified (if paths rejoin)
- [x] Decision data sources identified (INPUT vs RESPONSE vs PROCESS_PROPERTY)
- [x] Decision types classified (PRE_FILTER vs POST_OPERATION)
- [x] Actual execution order verified

### Branch Analysis
- [x] Each branch classified as parallel or sequential (Step 8)
- [x] API call check performed (branch contains API calls → SEQUENTIAL)
- [x] If sequential: dependency_order built using topological sort
- [x] Each path traced to terminal point
- [x] Convergence points identified
- [x] Execution continuation point determined

### Sequence Diagram
- [x] Format follows required structure (Step 10)
- [x] Each operation shows READS and WRITES
- [x] Decisions show both TRUE and FALSE paths
- [x] HTTP status codes documented for all operations
- [x] Early exits marked [EARLY EXIT]
- [x] Conditional execution marked appropriately
- [x] Subprocess internal flows documented
- [x] Sequence diagram references all prior steps (4, 5, 7, 8, 9)

### Subprocess Analysis
- [x] ALL subprocesses analyzed (internal flow traced) (Step 7a)
- [x] Return paths identified (success and error)
- [x] Return path labels mapped to main process shapes
- [x] Properties written by subprocess documented
- [x] Properties read by subprocess from main process documented

### Edge Cases
- [x] Nested branches/decisions analyzed
- [x] Loops identified (if any) with exit conditions (None in this process)
- [x] Property chains traced (transitive dependencies)
- [x] Circular dependencies detected and resolved (None in this process)
- [x] Try/Catch error paths documented

### Property Extraction Completeness
- [x] All property patterns searched (${}, %%, {})
- [x] Message parameters checked for process properties
- [x] Operation headers/path parameters checked
- [x] Decision track properties identified (meta.*)
- [x] Document properties that read other properties identified

### Input/Output Structure Analysis (CONTRACT VERIFICATION)
- [x] Entry point operation identified (Step 1a)
- [x] Request profile identified and loaded
- [x] Request profile structure analyzed (JSON/XML)
- [x] Array vs single object detected
- [x] ALL request fields extracted
- [x] Request field paths documented
- [x] Request field mapping table generated (Boomi → Azure DTO)
- [x] Response profile identified and loaded (Step 1b)
- [x] Response profile structure analyzed
- [x] ALL response fields extracted
- [x] Response field mapping table generated
- [x] Document processing behavior determined
- [x] Input/Output structure documented in Phase 1 document

### HTTP Status Codes and Return Path Responses
- [x] Section 6 (HTTP Status Codes and Return Path Responses - Step 1e) present
- [x] All return paths documented with HTTP status codes
- [x] Response JSON examples provided for each return path
- [x] Populated fields documented for each return path
- [x] Decision conditions leading to each return documented
- [x] Error codes and success codes documented for each return path
- [x] Downstream operation HTTP status codes documented

### Map Analysis
- [x] ALL map files identified and loaded (Step 1d)
- [x] Field mappings extracted from each map
- [x] Profile vs map field name discrepancies documented
- [x] Map field names marked as AUTHORITATIVE
- [x] Scripting functions analyzed (None in this process)
- [x] Static values identified and documented
- [x] Process property mappings documented
- [x] Map Analysis documented in Phase 1 document

### Function Exposure Decision Table
- [x] Function Exposure Decision Table complete
- [x] All potential functions evaluated
- [x] Only necessary functions marked for exposure
- [x] Rationale provided for each decision
- [x] Function explosion prevented

### Self-Check Questions (MANDATORY)

1. ❓ Did I analyze ALL map files? **YES** ✅
2. ❓ Did I identify SOAP request maps? **N/A** (No SOAP operations, only HTTP REST)
3. ❓ Did I extract actual field names from maps? **YES** ✅
4. ❓ Did I compare profile field names vs map field names? **YES** ✅
5. ❓ Did I mark map field names as AUTHORITATIVE? **YES** ✅
6. ❓ Did I analyze scripting functions in maps? **YES** (None found) ✅
7. ❓ Did I extract HTTP status codes for all return paths? **YES** ✅
8. ❓ Did I document response JSON for each return path? **YES** ✅
9. ❓ Did I document populated fields for each return path? **YES** ✅
10. ❓ Did I extract HTTP status codes for downstream operations? **YES** ✅
11. ❓ Did I create Function Exposure Decision Table? **YES** ✅
12. ❓ Did I prevent function explosion? **YES** ✅

### Final Validation

**Phase 1 Extraction Status:** ✅ COMPLETE

All mandatory sections are present and complete:
- [x] Operations Inventory
- [x] Input Structure Analysis (Step 1a)
- [x] Response Structure Analysis (Step 1b)
- [x] Operation Response Analysis (Step 1c)
- [x] Map Analysis (Step 1d)
- [x] HTTP Status Codes and Return Path Responses (Step 1e)
- [x] Process Properties Analysis (Steps 2-3)
- [x] Data Dependency Graph (Step 4)
- [x] Control Flow Graph (Step 5)
- [x] Decision Shape Analysis (Step 7)
- [x] Subprocess Analysis (Step 7a)
- [x] Branch Shape Analysis (Step 8)
- [x] Execution Order (Step 9)
- [x] Sequence Diagram (Step 10)
- [x] System Layer Identification
- [x] Function Exposure Decision Table
- [x] Validation Checklist

**Ready for Phase 2:** ✅ YES

---

**END OF PHASE 1 EXTRACTION DOCUMENT**
