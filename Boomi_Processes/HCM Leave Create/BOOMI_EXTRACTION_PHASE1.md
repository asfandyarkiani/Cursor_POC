# BOOMI EXTRACTION PHASE 1: HCM Leave Create Process

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
14. [Subprocess Analysis (Step 7a)](#14-subprocess-analysis-step-7a)
15. [System Layer Identification](#15-system-layer-identification)
16. [Request/Response JSON Examples](#16-requestresponse-json-examples)
17. [Function Exposure Decision Table](#17-function-exposure-decision-table)

---

## 1. Operations Inventory

### Main Process Operations

| Operation ID | Operation Name | Type | Sub-Type | Purpose |
|--------------|----------------|------|----------|---------|
| 8f709c2b-e63f-4d5f-9374-2932ed70415d | Create Leave Oracle Fusion OP | connector-action | wss | Entry point - WebServices Server Listen |
| 6e8920fd-af5a-430b-a1d9-9fde7ac29a12 | Leave Oracle Fusion Create | connector-action | http | HTTP POST to Oracle Fusion |
| af07502a-fafd-4976-a691-45d51a33b549 | Email w Attachment | connector-action | mail | Send email with attachment |
| 15a72a21-9b57-49a1-a8ed-d70367146644 | Email W/O Attachment | connector-action | mail | Send email without attachment |

### Subprocess Operations

**Subprocess ID:** a85945c5-3004-42b9-80b1-104f465cd1fb  
**Subprocess Name:** (Sub) Office 365 Email

| Operation ID | Operation Name | Type | Sub-Type | Purpose |
|--------------|----------------|------|----------|---------|
| af07502a-fafd-4976-a691-45d51a33b549 | Email w Attachment | connector-action | mail | Send email with attachment |
| 15a72a21-9b57-49a1-a8ed-d70367146644 | Email W/O Attachment | connector-action | mail | Send email without attachment |

### Connections

| Connection ID | Connection Name | Type | URL/Host |
|---------------|-----------------|------|----------|
| aa1fcb29-d146-4425-9ea6-b9698090f60e | Oracle Fusion | http | https://iaaxey-dev3.fa.ocs.oraclecloud.com:443 |
| 00eae79b-2303-4215-8067-dcc299e42697 | Office 365 Email | mail | smtp-mail.outlook.com:587 |

---

## 2. Process Properties Analysis

### Steps 2-3: Property WRITES and READS

#### Properties WRITTEN (Step 2)

| Property Name | Property ID | Written By Shape(s) | Source Type | Value/Source |
|---------------|-------------|---------------------|-------------|--------------|
| DPP_Process_Name | process.DPP_Process_Name | shape38 | execution | Process Name |
| DPP_AtomName | process.DPP_AtomName | shape38 | execution | Atom Name |
| DPP_Payload | process.DPP_Payload | shape38 | current | Current document |
| DPP_ExecutionID | process.DPP_ExecutionID | shape38 | execution | Execution Id |
| DPP_File_Name | process.DPP_File_Name | shape38 | concatenated | Process Name + Date + ".txt" |
| DPP_Subject | process.DPP_Subject | shape38 | concatenated | AtomName + " (" + ProcessName + ") has errors to report" |
| To_Email | process.To_Email | shape38 | defined_parameter | From PP_HCM_LeaveCreate_Properties |
| DPP_HasAttachment | process.DPP_HasAttachment | shape38 | defined_parameter | From PP_HCM_LeaveCreate_Properties |
| DPP_ErrorMessage | process.DPP_ErrorMessage | shape19, shape39, shape46 | track | meta.base.catcherrorsmessage / meta.base.applicationstatusmessage |
| URL | dynamicdocument.URL | shape8 | defined_parameter | Resource_Path from PP_HCM_LeaveCreate_Properties |

**Subprocess Properties WRITTEN:**

| Property Name | Property ID | Written By Shape(s) | Source Type | Value/Source |
|---------------|-------------|---------------------|-------------|--------------|
| DPP_MailBody | process.DPP_MailBody | shape14, shape22 | current | Email body HTML content |

#### Properties READ (Step 3)

| Property Name | Property ID | Read By Shape(s) | Usage Context |
|---------------|-------------|------------------|---------------|
| DPP_Process_Name | process.DPP_Process_Name | shape11 (subprocess), shape23 (subprocess) | Message parameter in email body |
| DPP_AtomName | process.DPP_AtomName | shape11 (subprocess), shape23 (subprocess) | Message parameter in email body |
| DPP_ExecutionID | process.DPP_ExecutionID | shape11 (subprocess), shape23 (subprocess) | Message parameter in email body |
| DPP_ErrorMessage | process.DPP_ErrorMessage | shape11 (subprocess), shape23 (subprocess) | Message parameter in email body |
| DPP_Payload | process.DPP_Payload | shape15 (subprocess) | Message parameter for attachment |
| DPP_File_Name | process.DPP_File_Name | shape6 (subprocess), shape20 (subprocess) | connector.mail.filename |
| To_Email | process.To_Email | shape6 (subprocess), shape20 (subprocess) | connector.mail.toAddress |
| DPP_Subject | process.DPP_Subject | shape6 (subprocess), shape20 (subprocess) | connector.mail.subject |
| DPP_MailBody | process.DPP_MailBody | shape6 (subprocess), shape20 (subprocess) | connector.mail.body |
| DPP_HasAttachment | process.DPP_HasAttachment | shape4 (subprocess) | Decision check for Y/N |

---

## 3. Input Structure Analysis (Step 1a)

✅ **MANDATORY STEP - CONTRACT VERIFICATION**

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

- **Boomi Processing:** Single document processing
- **Azure Function Requirement:** Must accept single leave request object
- **Implementation Pattern:** Process single leave request, return response
- **Execution Pattern:** single_execution
- **Session Management:** one_session_per_execution

### Complete Field Inventory

| Boomi Field Path | Boomi Field Name | Data Type | Required | Azure DTO Property | Notes |
|------------------|------------------|-----------|----------|-------------------|-------|
| Root/Object/employeeNumber | employeeNumber | number | No | EmployeeNumber | Employee identifier (tracked field) |
| Root/Object/absenceType | absenceType | character | No | AbsenceType | Leave type (Sick Leave, Annual Leave, etc.) |
| Root/Object/employer | employer | character | No | Employer | Employer name |
| Root/Object/startDate | startDate | character | No | StartDate | Leave start date (YYYY-MM-DD format) |
| Root/Object/endDate | endDate | character | No | EndDate | Leave end date (YYYY-MM-DD format) |
| Root/Object/absenceStatusCode | absenceStatusCode | character | No | AbsenceStatusCode | Absence status (SUBMITTED) |
| Root/Object/approvalStatusCode | approvalStatusCode | character | No | ApprovalStatusCode | Approval status (APPROVED) |
| Root/Object/startDateDuration | startDateDuration | number | No | StartDateDuration | Duration for start date (days) |
| Root/Object/endDateDuration | endDateDuration | number | No | EndDateDuration | Duration for end date (days) |

**Total Fields:** 9 fields (all flat structure, no nested objects)

### Array Cardinality

**Array Status:** Not applicable (single object, not an array)

---

## 4. Response Structure Analysis (Step 1b)

✅ **MANDATORY STEP - CONTRACT VERIFICATION**

### Response Profile Structure

**Profile ID:** f4ca3a70-114a-4601-bad8-44a3eb20e2c0  
**Profile Name:** Leave D365 Response  
**Profile Type:** profile.json  
**Root Structure:** leaveResponse/Object  
**Array Detection:** ❌ NO - Single object structure

### Response Format

```json
{
  "status": "success",
  "message": "Data successfully sent to Oracle Fusion",
  "personAbsenceEntryId": 123456,
  "success": "true"
}
```

### Complete Response Field Inventory

| Boomi Field Path | Boomi Field Name | Data Type | Azure DTO Property | Notes |
|------------------|------------------|-----------|-------------------|-------|
| leaveResponse/Object/status | status | character | Status | "success" or "failure" |
| leaveResponse/Object/message | message | character | Message | Response message or error details |
| leaveResponse/Object/personAbsenceEntryId | personAbsenceEntryId | number | PersonAbsenceEntryId | Oracle Fusion leave entry ID (from success response) |
| leaveResponse/Object/success | success | character | Success | "true" or "false" |

**Total Fields:** 4 fields (all flat structure, no nested objects)

---

## 5. Operation Response Analysis (Step 1c)

✅ **MANDATORY STEP - REQUIRED BEFORE STEP 7**

### Operation: Leave Oracle Fusion Create (6e8920fd-af5a-430b-a1d9-9fde7ac29a12)

**Response Profile ID:** None (responseProfileType: NONE)  
**Response Type:** application/json  
**Response Content Type Handling:** Content-Encoding header mapped to DDP_RespHeader

**Response Structure:** Oracle Fusion returns JSON response from absences API

**Extracted Fields:**
- **DDP_RespHeader:** Extracted via responseHeaderMapping from Content-Encoding header
  - Written to: dynamicdocument.DDP_RespHeader
  - Extracted by: HTTP operation configuration (shape33)
  - Used by: Decision shape44 (checks if equals "gzip")

**Data Consumers:**
- **shape44 (Decision):** Checks DDP_RespHeader to determine if response is gzip compressed
  - TRUE path → shape45 (decompress with Groovy script)
  - FALSE path → shape39 (extract error message)

**Business Logic Implications:**
- Oracle Fusion Create operation MUST execute BEFORE decision shape44
- Decision shape44 checks RESPONSE header (DDP_RespHeader), not INPUT data
- Decision type: POST-OPERATION (checks response data from HTTP call)

**Oracle Fusion Response Fields (from profile 316175c7-0e45-4869-9ac6-5f9d69882a62):**

The Oracle Fusion API returns extensive leave data including:
- personAbsenceEntryId (primary key)
- absenceStatusCd, approvalStatusCd
- startDate, endDate, duration fields
- personId, personNumber
- Audit fields (createdBy, creationDate, lastUpdatedBy, lastUpdateDate)
- Many other Oracle-specific fields (70+ fields total)

**Key Response Field Extracted:**
- **personAbsenceEntryId:** Mapped to D365 response (via map e4fd3f59-edb5-43a1-aeae-143b600a064e)

---

## 6. Map Analysis (Step 1d)

✅ **MANDATORY STEP - REQUIRED BEFORE PHASE 2**

### Map Inventory

| Map ID | Map Name | From Profile | To Profile | Type |
|--------|----------|--------------|------------|------|
| c426b4d6-2aff-450e-b43b-59956c4dbc96 | Leave Create Map | febfa3e1-f719-4ee8-ba57-cdae34137ab3 (D365 Leave Create) | a94fa205-c740-40a5-9fda-3d018611135a (HCM Leave Create) | Request transformation |
| e4fd3f59-edb5-43a1-aeae-143b600a064e | Oracle Fusion Leave Response Map | 316175c7-0e45-4869-9ac6-5f9d69882a62 (Oracle Fusion Response) | f4ca3a70-114a-4601-bad8-44a3eb20e2c0 (Leave D365 Response) | Success response transformation |
| f46b845a-7d75-41b5-b0ad-c41a6a8e9b12 | Leave Error Map | 23d7a2e9-5cb0-4e9c-9e4b-3154834bad0d (Dummy FF Profile) | f4ca3a70-114a-4601-bad8-44a3eb20e2c0 (Leave D365 Response) | Error response transformation |

### Map 1: Leave Create Map (c426b4d6-2aff-450e-b43b-59956c4dbc96)

**From Profile:** febfa3e1-f719-4ee8-ba57-cdae34137ab3 (D365 Leave Create JSON Profile)  
**To Profile:** a94fa205-c740-40a5-9fda-3d018611135a (HCM Leave Create JSON Profile)  
**Type:** Request transformation (D365 → Oracle Fusion format)

**Field Mappings:**

| Source Field (D365) | Source Path | Target Field (Oracle) | Target Path | Transformation |
|---------------------|-------------|----------------------|-------------|----------------|
| employeeNumber | Root/Object/employeeNumber | personNumber | Root/Object/personNumber | Direct mapping |
| absenceType | Root/Object/absenceType | absenceType | Root/Object/absenceType | Direct mapping |
| employer | Root/Object/employer | employer | Root/Object/employer | Direct mapping |
| startDate | Root/Object/startDate | startDate | Root/Object/startDate | Direct mapping |
| endDate | Root/Object/endDate | endDate | Root/Object/endDate | Direct mapping |
| absenceStatusCode | Root/Object/absenceStatusCode | absenceStatusCd | Root/Object/absenceStatusCd | Field name change |
| approvalStatusCode | Root/Object/approvalStatusCode | approvalStatusCd | Root/Object/approvalStatusCd | Field name change |
| startDateDuration | Root/Object/startDateDuration | startDateDuration | Root/Object/startDateDuration | Direct mapping |
| endDateDuration | Root/Object/endDateDuration | endDateDuration | Root/Object/endDateDuration | Direct mapping |

**Profile vs Map Field Name Discrepancies:**

| Source Field Name | Target Profile Field Name | Authority | Notes |
|-------------------|---------------------------|-----------|-------|
| absenceStatusCode | absenceStatusCd | ✅ MAP | Field name shortened in Oracle format |
| approvalStatusCode | approvalStatusCd | ✅ MAP | Field name shortened in Oracle format |

**Scripting Functions:** None

**Static Values:** None

**CRITICAL RULE:** This is a JSON-to-JSON transformation. All field names are used as-is for Oracle Fusion REST API.

### Map 2: Oracle Fusion Leave Response Map (e4fd3f59-edb5-43a1-aeae-143b600a064e)

**From Profile:** 316175c7-0e45-4869-9ac6-5f9d69882a62 (Oracle Fusion Leave Response JSON Profile)  
**To Profile:** f4ca3a70-114a-4601-bad8-44a3eb20e2c0 (Leave D365 Response)  
**Type:** Success response transformation (Oracle Fusion → D365 format)

**Field Mappings:**

| Source Field (Oracle) | Source Path | Target Field (D365) | Target Path | Transformation |
|-----------------------|-------------|---------------------|-------------|----------------|
| personAbsenceEntryId | Root/Object/personAbsenceEntryId | personAbsenceEntryId | leaveResponse/Object/personAbsenceEntryId | Direct mapping |

**Default Values:**

| Target Field | Default Value | Purpose |
|--------------|---------------|---------|
| status | "success" | Indicates successful processing |
| message | "Data successfully sent to Oracle Fusion" | Success message |
| success | "true" | Boolean success flag |

**CRITICAL:** This map only extracts the key Oracle response field (personAbsenceEntryId) and adds success indicators.

### Map 3: Leave Error Map (f46b845a-7d75-41b5-b0ad-c41a6a8e9b12)

**From Profile:** 23d7a2e9-5cb0-4e9c-9e4b-3154834bad0d (Dummy FF Profile)  
**To Profile:** f4ca3a70-114a-4601-bad8-44a3eb20e2c0 (Leave D365 Response)  
**Type:** Error response transformation

**Field Mappings:**

| Source | Source Type | Target Field (D365) | Target Path | Transformation |
|--------|-------------|---------------------|-------------|----------------|
| process.DPP_ErrorMessage | function (PropertyGet) | message | leaveResponse/Object/message | Get error message from process property |

**Scripting Functions:**

| Function Key | Function Type | Input | Output | Logic |
|--------------|---------------|-------|--------|-------|
| 1 | PropertyGet | Property Name: "DPP_ErrorMessage" | Result | Get Dynamic Process Property DPP_ErrorMessage |

**Default Values:**

| Target Field | Default Value | Purpose |
|--------------|---------------|---------|
| status | "failure" | Indicates failed processing |
| success | "false" | Boolean failure flag |

**CRITICAL:** This map retrieves the error message stored in process.DPP_ErrorMessage and creates a failure response.

---

## 7. HTTP Status Codes and Return Path Responses (Step 1e)

✅ **MANDATORY STEP - REQUIRED BEFORE PHASE 2**

### Return Path 1: Success Response (shape35)

**Return Label:** "Success Response"  
**Return Shape ID:** shape35  
**HTTP Status Code:** 200  
**Decision Conditions Leading to Return:**
- Decision shape2: meta.base.applicationstatuscode matches "20*" (wildcard) → TRUE path

**Populated Response Fields:**

| Field Name | Field Path | Source | Populated By | Value |
|----------|------------|--------|--------------|-------|
| personAbsenceEntryId | leaveResponse/Object/personAbsenceEntryId | operation_response | shape33 (Oracle Fusion Create) → shape34 (map) | Oracle response ID |
| status | leaveResponse/Object/status | static (map default) | shape34 (map e4fd3f59) | "success" |
| message | leaveResponse/Object/message | static (map default) | shape34 (map e4fd3f59) | "Data successfully sent to Oracle Fusion" |
| success | leaveResponse/Object/success | static (map default) | shape34 (map e4fd3f59) | "true" |

**Response JSON Example:**

```json
{
  "status": "success",
  "message": "Data successfully sent to Oracle Fusion",
  "personAbsenceEntryId": 123456,
  "success": "true"
}
```

### Return Path 2: Error Response - Try/Catch Error (shape43)

**Return Label:** "Error Response"  
**Return Shape ID:** shape43  
**HTTP Status Code:** 500  
**Decision Conditions Leading to Return:**
- Try/Catch block (shape17) → Catch path (shape20 branch path 2)

**Error Code:** N/A  
**Populated Response Fields:**

| Field Name | Field Path | Source | Populated By | Value |
|----------|------------|--------|--------------|-------|
| status | leaveResponse/Object/status | static (map default) | shape41 (map f46b845a) | "failure" |
| message | leaveResponse/Object/message | process_property | shape19 → process.DPP_ErrorMessage → shape41 (map) | Error message from Try/Catch |
| success | leaveResponse/Object/success | static (map default) | shape41 (map f46b845a) | "false" |

**Response JSON Example:**

```json
{
  "status": "failure",
  "message": "Error details from exception: [actual error message]",
  "success": "false"
}
```

### Return Path 3: Error Response - HTTP Non-20x (shape36)

**Return Label:** "Error Response"  
**Return Shape ID:** shape36  
**HTTP Status Code:** 400 (or matching Oracle error code)  
**Decision Conditions Leading to Return:**
- Decision shape2: meta.base.applicationstatuscode does NOT match "20*" → FALSE path
- Decision shape44: DDP_RespHeader does NOT equal "gzip" → FALSE path

**Populated Response Fields:**

| Field Name | Field Path | Source | Populated By | Value |
|----------|------------|--------|--------------|-------|
| status | leaveResponse/Object/status | static (map default) | shape40 (map f46b845a) | "failure" |
| message | leaveResponse/Object/message | process_property | shape39 → process.DPP_ErrorMessage (from meta.base.applicationstatusmessage) → shape40 (map) | Oracle API error message |
| success | leaveResponse/Object/success | static (map default) | shape40 (map f46b845a) | "false" |

**Response JSON Example:**

```json
{
  "status": "failure",
  "message": "Oracle Fusion API error: [actual error message from Oracle]",
  "success": "false"
}
```

### Return Path 4: Error Response - HTTP Non-20x with GZIP (shape48)

**Return Label:** "Error Response"  
**Return Shape ID:** shape48  
**HTTP Status Code:** 400 (or matching Oracle error code)  
**Decision Conditions Leading to Return:**
- Decision shape2: meta.base.applicationstatuscode does NOT match "20*" → FALSE path
- Decision shape44: DDP_RespHeader equals "gzip" → TRUE path

**Populated Response Fields:**

| Field Name | Field Path | Source | Populated By | Value |
|----------|------------|--------|--------------|-------|
| status | leaveResponse/Object/status | static (map default) | shape47 (map f46b845a) | "failure" |
| message | leaveResponse/Object/message | process_property | shape45 (decompress) → shape46 → process.DPP_ErrorMessage (from meta.base.applicationstatusmessage) → shape47 (map) | Decompressed Oracle API error message |
| success | leaveResponse/Object/success | static (map default) | shape47 (map f46b845a) | "false" |

**Response JSON Example:**

```json
{
  "status": "failure",
  "message": "Oracle Fusion API error (decompressed): [actual error message from Oracle]",
  "success": "false"
}
```

### Downstream Operations HTTP Status Codes

| Operation Name | Operation ID | Expected Success Codes | Error Codes | Error Handling |
|----------------|--------------|----------------------|-------------|----------------|
| Leave Oracle Fusion Create | 6e8920fd-af5a-430b-a1d9-9fde7ac29a12 | 20* (200, 201, etc.) | 400, 401, 404, 500 | Return error response with message from Oracle |

**Note:** Oracle Fusion returns errors with status codes and may use gzip compression for responses.

---

## 8. Data Dependency Graph (Step 4)

✅ **MANDATORY STEP - REQUIRED BEFORE STEP 10**

### Dependency Chains

**Property Dependency Chain 1: Error Handling (Try/Catch)**

```
shape17 (Catch Errors)
  └─→ Catch path → shape19 (writes process.DPP_ErrorMessage from meta.base.catcherrorsmessage)
      └─→ shape21 (Subprocess call)
      └─→ shape20 (Branch) → shape41 (map reads process.DPP_ErrorMessage via function)
          └─→ shape43 (Return error response)
```

**Property Dependency Chain 2: HTTP Error Handling (Non-20x, Non-GZIP)**

```
shape33 (HTTP operation)
  └─→ shape2 (Decision checks meta.base.applicationstatuscode)
      └─→ FALSE path → shape44 (Decision checks dynamicdocument.DDP_RespHeader)
          └─→ FALSE path → shape39 (writes process.DPP_ErrorMessage from meta.base.applicationstatusmessage)
              └─→ shape40 (map reads process.DPP_ErrorMessage via function)
                  └─→ shape36 (Return error response)
```

**Property Dependency Chain 3: HTTP Error Handling (Non-20x, GZIP)**

```
shape33 (HTTP operation)
  └─→ shape2 (Decision checks meta.base.applicationstatuscode)
      └─→ FALSE path → shape44 (Decision checks dynamicdocument.DDP_RespHeader)
          └─→ TRUE path → shape45 (Decompress GZIP)
              └─→ shape46 (writes process.DPP_ErrorMessage from meta.base.applicationstatusmessage)
                  └─→ shape47 (map reads process.DPP_ErrorMessage via function)
                      └─→ shape48 (Return error response)
```

**Property Dependency Chain 4: Input Properties (for Email subprocess)**

```
shape38 (writes multiple properties: DPP_Process_Name, DPP_AtomName, DPP_Payload, DPP_ExecutionID, DPP_File_Name, DPP_Subject, To_Email, DPP_HasAttachment)
  └─→ shape17 (Try/Catch) → Catch path → shape20 (Branch)
      └─→ shape21 (Subprocess reads all email-related properties)
          └─→ Subprocess shape4 (reads DPP_HasAttachment)
          └─→ Subprocess shape11, shape23 (read DPP_Process_Name, DPP_AtomName, DPP_ExecutionID, DPP_ErrorMessage)
          └─→ Subprocess shape15 (reads DPP_Payload)
          └─→ Subprocess shape6, shape20 (read To_Email, DPP_Subject, DPP_MailBody, DPP_File_Name)
```

**Property Dependency Chain 5: URL Configuration**

```
shape8 (writes dynamicdocument.URL from defined parameter Resource_Path)
  └─→ shape49 (Notify - logs payload)
      └─→ shape33 (HTTP operation uses dynamicdocument.URL)
```

### Dependency Summary

| Property Name | Writer Shapes | Reader Shapes | Critical Path |
|---------------|---------------|---------------|---------------|
| process.DPP_ErrorMessage | shape19, shape39, shape46 | map f46b845a (shapes 40, 41, 47) | Error handling - must write before map reads |
| process.DPP_Process_Name | shape38 | subprocess shapes 11, 23 | Email notification |
| process.DPP_AtomName | shape38 | subprocess shapes 11, 23 | Email notification |
| process.DPP_Payload | shape38 | subprocess shape15 | Email attachment |
| process.DPP_ExecutionID | shape38 | subprocess shapes 11, 23 | Email notification |
| process.DPP_File_Name | shape38 | subprocess shapes 6, 20 | Email attachment filename |
| process.DPP_Subject | shape38 | subprocess shapes 6, 20 | Email subject |
| process.To_Email | shape38 | subprocess shapes 6, 20 | Email recipient |
| process.DPP_HasAttachment | shape38 | subprocess shape4 | Email attachment decision |
| dynamicdocument.URL | shape8 | shape33 | HTTP endpoint path |
| dynamicdocument.DDP_RespHeader | shape33 (HTTP response header mapping) | shape44 | GZIP detection decision |

**Independent Operations:**
- shape1 (Start)
- shape38 (Input_details - writes properties from execution/input)
- shape28 (Test message - disconnected, toShape="unset")

---

## 9. Control Flow Graph (Step 5)

✅ **MANDATORY STEP**

### Control Flow Map

| Source Shape | Shape Type | Target Shape(s) | Identifier | Description |
|--------------|------------|-----------------|------------|-------------|
| shape1 | start | shape38 | default | Entry point |
| shape38 | documentproperties | shape17 | default | Set input properties → Try/Catch |
| shape17 | catcherrors | shape29 (try), shape20 (catch) | default, error | Try/Catch split |
| shape29 | map | shape8 | default | Map request data |
| shape8 | documentproperties | shape49 | default | Set URL |
| shape49 | notify | shape33 | default | Log → HTTP call |
| shape33 | connectoraction | shape2 | default | HTTP call → Decision |
| shape2 | decision | shape34 (true), shape44 (false) | true, false | HTTP status check |
| shape34 | map | shape35 | default | Success map → Return |
| shape35 | returndocuments | - | - | Success return (terminal) |
| shape44 | decision | shape45 (true), shape39 (false) | true, false | GZIP check |
| shape45 | dataprocess | shape46 | default | Decompress GZIP |
| shape46 | documentproperties | shape47 | default | Extract error msg |
| shape47 | map | shape48 | default | Error map → Return |
| shape48 | returndocuments | - | - | Error return (terminal) |
| shape39 | documentproperties | shape40 | default | Extract error msg |
| shape40 | map | shape36 | default | Error map → Return |
| shape36 | returndocuments | - | - | Error return (terminal) |
| shape20 | branch | shape19 (path1), shape41 (path2) | 1, 2 | Error branch |
| shape19 | documentproperties | shape21 | default | Extract catch error |
| shape21 | processcall | - | - | Email subprocess (terminal) |
| shape41 | map | shape43 | default | Error map → Return |
| shape43 | returndocuments | - | - | Error return (terminal) |
| shape28 | message | unset | - | Test message (disconnected) |

### Reverse Flow Mapping (Step 6)

**Convergence Points:**

None identified. All paths lead to distinct terminal shapes (returndocuments).

**Incoming Connections Summary:**

| Target Shape | Source Shapes | Type |
|--------------|---------------|------|
| shape38 | shape1 | Start connection |
| shape17 | shape38 | Try/Catch entry |
| shape29 | shape17 | Try path |
| shape20 | shape17 | Catch path |
| shape34 | shape2 | Success path from HTTP status decision |
| shape44 | shape2 | Failure path from HTTP status decision |
| shape45 | shape44 | GZIP detected |
| shape39 | shape44 | No GZIP |

**Terminal Shapes (no outgoing connections):**
- shape35 (Success Response)
- shape36 (Error Response)
- shape43 (Error Response)
- shape48 (Error Response)
- shape21 (Subprocess - email notification, returns to main)

---

## 10. Decision Shape Analysis (Step 7)

✅ **MANDATORY STEP - BLOCKING - REQUIRED BEFORE STEP 10**

### Self-Check Results

✅ **Decision data sources identified: YES**  
✅ **Decision types classified: YES**  
✅ **Execution order verified: YES**  
✅ **All decision paths traced: YES**  
✅ **Decision patterns identified: YES**  
✅ **Paths traced to termination: YES**

### Decision 1: HTTP Status 20 check (shape2)

**Shape ID:** shape2  
**Comparison:** wildcard  
**Value 1:** meta.base.applicationstatuscode (track property)  
**Value 2:** "20*" (static)

**Data Source:** TRACK_PROPERTY (meta.base.applicationstatuscode from HTTP response)  
**Decision Type:** POST-OPERATION  
**Actual Execution Order:** shape33 (HTTP operation) → Response → shape2 (Decision) → Route based on status

**TRUE Path:**
- **Destination:** shape34 (map)
- **Termination:** shape34 → shape35 (returndocuments "Success Response")
- **Type:** return (SUCCESS)

**FALSE Path:**
- **Destination:** shape44 (decision)
- **Termination:** shape44 → branches to shape45/shape39 paths → both lead to error returns (shape48 or shape36)
- **Type:** return (ERROR)

**Pattern:** Error Check (Success vs Failure)  
**Description:** Checks HTTP status code from Oracle Fusion API call. If 20x (success), map response and return success. Otherwise, handle error (check for GZIP compression).

**Convergence Point:** None (paths diverge to different returns)  
**Early Exit:** FALSE path leads to error return

**Proof of Data Source:**
- JSON Reference: shape2 decision configuration, decisionvalue[0].valueType = "track", trackparameter.propertyId = "meta.base.applicationstatuscode"
- This is a TRACK property from the HTTP connector response (shape33)

**Business Logic:** Oracle Fusion Create API must execute FIRST, then check response status to determine success/failure path.

### Decision 2: Check Response Content Type (shape44)

**Shape ID:** shape44  
**Comparison:** equals  
**Value 1:** dynamicdocument.DDP_RespHeader (track property)  
**Value 2:** "gzip" (static)

**Data Source:** TRACK_PROPERTY (dynamicdocument.DDP_RespHeader from HTTP response header mapping)  
**Decision Type:** POST-OPERATION  
**Actual Execution Order:** shape33 (HTTP operation) → Response header extracted → shape2 (Decision FALSE path) → shape44 (Decision) → Route based on compression

**TRUE Path:**
- **Destination:** shape45 (dataprocess - decompress GZIP)
- **Termination:** shape45 → shape46 → shape47 → shape48 (returndocuments "Error Response")
- **Type:** return (ERROR with decompression)

**FALSE Path:**
- **Destination:** shape39 (documentproperties)
- **Termination:** shape39 → shape40 → shape36 (returndocuments "Error Response")
- **Type:** return (ERROR without decompression)

**Pattern:** Conditional Logic (Optional Processing)  
**Description:** Checks if Oracle Fusion error response is GZIP compressed. If yes, decompress before extracting error message. Otherwise, extract error message directly.

**Convergence Point:** None (both paths lead to error returns but different shapes)  
**Early Exit:** Both paths are error exits

**Proof of Data Source:**
- JSON Reference: shape44 decision configuration, decisionvalue[0].valueType = "track", trackparameter.propertyId = "dynamicdocument.DDP_RespHeader"
- This property is populated by HTTP operation shape33 via responseHeaderMapping (Content-Encoding header → DDP_RespHeader)

**Business Logic:** HTTP operation must execute FIRST, check status (shape2), then if error, check if response is compressed to properly extract error message.

### Subprocess Decision: Attachment_Check (subprocess shape4)

**Shape ID:** shape4 (in subprocess a85945c5-3004-42b9-80b1-104f465cd1fb)  
**Comparison:** equals  
**Value 1:** process.DPP_HasAttachment (process property)  
**Value 2:** "Y" (static)

**Data Source:** PROCESS_PROPERTY (written by main process shape38)  
**Decision Type:** POST-OPERATION (depends on main process setting the property)  
**Actual Execution Order:** Main process shape38 writes property → Subprocess called → shape4 (Decision) → Route based on attachment requirement

**TRUE Path (Attachment = Y):**
- **Destination:** shape11 (message - build email body with attachment)
- **Termination:** shape11 → shape14 → shape15 → shape6 → shape3 → shape5 (stop continue=true)
- **Type:** stop (success, return to main process)

**FALSE Path (Attachment = N):**
- **Destination:** shape23 (message - build email body without attachment)
- **Termination:** shape23 → shape22 → shape20 → shape7 → shape9 (stop continue=true)
- **Type:** stop (success, return to main process)

**Pattern:** Conditional Logic (Optional Processing)  
**Description:** Determines whether to send email with or without attachment based on configuration.

**Convergence Point:** None (both paths lead to Stop but different shapes/operations)  
**Early Exit:** None (both paths complete successfully)

**Proof of Data Source:**
- JSON Reference: subprocess shape4 decision configuration, decisionvalue[0].valueType = "process", processparameter.processproperty = "DPP_HasAttachment"
- This property is written by main process shape38 from defined parameter

**Business Logic:** Main process must set DPP_HasAttachment BEFORE calling subprocess. Subprocess routes to appropriate email operation based on this flag.

---

## 11. Branch Shape Analysis (Step 8)

✅ **MANDATORY STEP - BLOCKING - REQUIRED BEFORE STEP 10**

### Self-Check Results

✅ **Classification completed: YES**  
✅ **Assumption check: NO (analyzed dependencies)**  
✅ **Properties extracted: YES**  
✅ **Dependency graph shown: YES**  
✅ **Topological sort applied: N/A (parallel paths)**

### Branch 1: Error Handling Branch (shape20)

**Shape ID:** shape20  
**Number of Paths:** 2  
**Location:** Catch path of Try/Catch (shape17)

#### Path Properties Analysis (Step 1)

**Path 1 (identifier "1"):**
- **Destination:** shape19 (documentproperties - ErrorMsg)
- **Properties READ:** meta.base.catcherrorsmessage (track property, not process property)
- **Properties WRITTEN:** process.DPP_ErrorMessage

**Path 2 (identifier "2"):**
- **Destination:** shape41 (map)
- **Properties READ:** None directly (uses current document)
- **Properties WRITTEN:** None

#### Dependency Graph (Step 2)

```
Path 1 dependencies: None
Path 2 dependencies: None
```

**Analysis:** No data dependencies between paths. Path 1 writes process.DPP_ErrorMessage, but Path 2 does not read it. Path 2 uses a map function (PropertyGet) to read process.DPP_ErrorMessage, which could be written by Path 1, but the paths are parallel.

#### Classification (Step 3)

**Classification:** PARALLEL  
**Reason:** 
- No data dependencies between paths (Path 2 map function reads DPP_ErrorMessage which may have been written by error extraction in shape19 or shape39 or shape46, but not necessarily from Path 1)
- No API calls in branch paths (only documentproperties, map, processcall shapes)

**Proof:**
- Path 1: documentproperties (shape19) → processcall (shape21)
- Path 2: map (shape41) → returndocuments (shape43)
- No HTTP/SOAP/REST operations in branch paths
- No explicit data dependency (Path 2 map reads DPP_ErrorMessage via function, which is independent execution)

#### Topological Sort (Step 4)

**Not Applicable** - Paths are parallel, no sequential execution order required.

#### Path Termination (Step 5)

**Path 1 Terminal:** shape21 (processcall - subprocess call, terminal in main process)  
**Path 2 Terminal:** shape43 (returndocuments "Error Response")

#### Convergence Points (Step 6)

**Convergence:** None  
**Execution Continues From:** None (paths are terminal)

#### Execution Continuation (Step 7)

**Execution Pattern:** Both paths execute in parallel (branch splits execution). Each path terminates independently.
- Path 1: Sends error notification email via subprocess
- Path 2: Returns error response to caller

**Note:** In Boomi, branch shapes with no dependencies execute paths in parallel. Both error handling actions happen simultaneously.

#### Complete Analysis (Step 8)

```json
{
  "shapeId": "shape20",
  "numPaths": 2,
  "classification": "PARALLEL",
  "dependencyOrder": null,
  "pathTerminals": {
    "path1": "shape21 (processcall)",
    "path2": "shape43 (returndocuments)"
  },
  "convergencePoints": [],
  "executionContinuesFrom": null
}
```

---

## 12. Execution Order (Step 9)

✅ **MANDATORY STEP - BLOCKING - REQUIRED BEFORE STEP 10**

### Self-Check Results

✅ **Business logic verified FIRST: YES**  
✅ **Operation analysis complete: YES**  
✅ **Business logic execution order identified: YES**  
✅ **Data dependencies checked FIRST: YES**  
✅ **Operation response analysis used: YES** (references Step 1c)  
✅ **Decision analysis used: YES** (references Step 7)  
✅ **Dependency graph used: YES** (references Step 4)  
✅ **Branch analysis used: YES** (references Step 8)  
✅ **Property dependency verification: YES**  
✅ **Topological sort applied: N/A** (branch is parallel)

### Step 0: Business Logic Flow

#### Operation Analysis

**Operation 1: Create Leave Oracle Fusion OP (Entry Point)**
- **Purpose:** WebServices Server Listen - Accepts leave request from D365
- **What it produces:** Input document (leave request JSON)
- **Dependent operations:** All subsequent operations depend on this input
- **Business flow:** Receives leave request → Triggers main process execution

**Operation 2: Leave Oracle Fusion Create (Downstream)**
- **Purpose:** HTTP POST to Oracle Fusion absences API
- **What it produces:** 
  - HTTP response (Oracle leave entry data)
  - Response headers (including Content-Encoding)
  - Track properties: meta.base.applicationstatuscode, meta.base.applicationstatusmessage
- **Dependent operations:** 
  - Decision shape2 (reads meta.base.applicationstatuscode)
  - Decision shape44 (reads dynamicdocument.DDP_RespHeader)
  - Map shape34 (reads response body if success)
  - Error extraction shapes (read error message if failure)
- **Business flow:** Creates leave entry in Oracle Fusion → Returns Oracle response → Route based on success/failure

**Subprocess Operations: Email Notifications**
- **Purpose:** Send error notification emails
- **What it produces:** Email sent to configured recipients
- **Dependent operations:** None (terminal operation)
- **Business flow:** Called on errors to notify support team

#### Business Logic Execution Order

**CRITICAL RULE:** Oracle Fusion Create API MUST execute BEFORE any decisions check response status.

**Main Success Flow:**
1. Receive leave request from D365 (shape1)
2. Extract and store execution metadata (shape38) - MUST execute FIRST (provides properties for email subprocess)
3. Transform request data (shape29) - Depends on input document
4. Set Oracle API URL (shape8) - Required for HTTP call
5. Log request payload (shape49)
6. Call Oracle Fusion Create API (shape33) - MUST execute BEFORE decisions check response
7. Check HTTP status code (shape2) - Checks RESPONSE from step 6
8. Map Oracle response to D365 format (shape34) - Uses RESPONSE from step 6
9. Return success response (shape35)

**Error Flow 1: Try/Catch Exception**
1-2. Same as success flow
3. Exception occurs in Try block
4. Extract error message (shape19)
5. Branch executes two parallel actions:
   - Path 1: Send error notification email (shape21 subprocess)
   - Path 2: Return error response (shape41 map → shape43)

**Error Flow 2: HTTP Non-20x (No GZIP)**
1-6. Same as success flow
7. Check HTTP status code (shape2) - FALSE path (non-20x status)
8. Check for GZIP compression (shape44) - FALSE path (not compressed)
9. Extract error message from Oracle response (shape39)
10. Map error to D365 format (shape40)
11. Return error response (shape36)

**Error Flow 3: HTTP Non-20x (GZIP)**
1-6. Same as success flow
7. Check HTTP status code (shape2) - FALSE path (non-20x status)
8. Check for GZIP compression (shape44) - TRUE path (compressed)
9. Decompress GZIP response (shape45)
10. Extract error message (shape46)
11. Map error to D365 format (shape47)
12. Return error response (shape48)

**Proof of Business Logic:**
- **Data Dependency Chain:** shape33 (HTTP call) produces response → shape2 (Decision) checks response status → Route to success/error handling
- **Operation Response Analysis (Step 1c):** Shows shape33 produces track properties used by shape2 and shape44
- **Decision Analysis (Step 7):** Shows shape2 and shape44 are POST-OPERATION decisions checking RESPONSE data

### Actual Execution Sequence (Based on Dependencies and Control Flow)

**Referenced Steps:**
- **Step 4 (Data Dependency Graph):** Property dependency chains documented
- **Step 5 (Control Flow Graph):** Dragpoint connections documented
- **Step 7 (Decision Analysis):** Decision data sources and types analyzed
- **Step 8 (Branch Analysis):** Branch classification completed

#### Main Process Execution Order

1. **shape1** (start - WebServices Server Listen)
   - **READS:** None
   - **WRITES:** Input document (from D365)
   
2. **shape38** (documentproperties - Input_details)
   - **READS:** Execution properties, Current document, Defined parameters
   - **WRITES:** process.DPP_Process_Name, process.DPP_AtomName, process.DPP_Payload, process.DPP_ExecutionID, process.DPP_File_Name, process.DPP_Subject, process.To_Email, process.DPP_HasAttachment

3. **shape17** (catcherrors - Try/Catch wrapper)
   - **READS:** None
   - **SPLITS:** Try path (shape29) | Catch path (shape20)

#### Try Path Execution

4. **shape29** (map - Leave Create Map)
   - **READS:** Input document fields
   - **WRITES:** Transformed document (D365 → Oracle format)
   - **Map:** c426b4d6-2aff-450e-b43b-59956c4dbc96

5. **shape8** (documentproperties - set URL)
   - **READS:** Defined parameter Resource_Path
   - **WRITES:** dynamicdocument.URL

6. **shape49** (notify - log payload)
   - **READS:** Current document
   - **WRITES:** Log entry

7. **shape33** (connectoraction - Leave Oracle Fusion Create) **[DOWNSTREAM - System Layer]**
   - **READS:** dynamicdocument.URL, Current document (request body)
   - **WRITES:** Response document, meta.base.applicationstatuscode, dynamicdocument.DDP_RespHeader (from Content-Encoding header)
   - **HTTP:** POST to Oracle Fusion
   - **Expected:** 20* status codes
   - **Errors:** 400, 401, 404, 500

8. **shape2** (decision - HTTP Status 20 check)
   - **READS:** meta.base.applicationstatuscode
   - **CONDITION:** Wildcard match "20*"
   - **TRUE PATH:** Success flow (shape34)
   - **FALSE PATH:** Error flow (shape44)

#### Success Path (shape2 TRUE)

9a. **shape34** (map - Oracle Fusion Leave Response Map)
   - **READS:** Response document (personAbsenceEntryId)
   - **WRITES:** D365 response format (with success defaults)
   - **Map:** e4fd3f59-edb5-43a1-aeae-143b600a064e

10a. **shape35** (returndocuments - Success Response)
   - **READS:** Mapped response document
   - **RETURNS:** HTTP 200 with success response
   - **TERMINAL:** Process ends

#### Error Path (shape2 FALSE) - Check GZIP

9b. **shape44** (decision - Check Response Content Type)
   - **READS:** dynamicdocument.DDP_RespHeader
   - **CONDITION:** Equals "gzip"
   - **TRUE PATH:** GZIP decompression flow (shape45)
   - **FALSE PATH:** Direct error extraction (shape39)

#### Error Path - GZIP = TRUE

10b. **shape45** (dataprocess - Decompress GZIP)
   - **READS:** Current document (compressed response)
   - **WRITES:** Decompressed document
   - **Script:** Groovy GZIP decompression

11b. **shape46** (documentproperties - error msg)
   - **READS:** meta.base.applicationstatusmessage (from decompressed response)
   - **WRITES:** process.DPP_ErrorMessage

12b. **shape47** (map - Leave Error Map)
   - **READS:** process.DPP_ErrorMessage (via PropertyGet function)
   - **WRITES:** D365 error response format
   - **Map:** f46b845a-7d75-41b5-b0ad-c41a6a8e9b12

13b. **shape48** (returndocuments - Error Response)
   - **READS:** Mapped error response
   - **RETURNS:** HTTP 400 (or Oracle error code) with error response
   - **TERMINAL:** Process ends

#### Error Path - GZIP = FALSE

10c. **shape39** (documentproperties - error msg)
   - **READS:** meta.base.applicationstatusmessage
   - **WRITES:** process.DPP_ErrorMessage

11c. **shape40** (map - Leave Error Map)
   - **READS:** process.DPP_ErrorMessage (via PropertyGet function)
   - **WRITES:** D365 error response format
   - **Map:** f46b845a-7d75-41b5-b0ad-c41a6a8e9b12

12c. **shape36** (returndocuments - Error Response)
   - **READS:** Mapped error response
   - **RETURNS:** HTTP 400 (or Oracle error code) with error response
   - **TERMINAL:** Process ends

#### Catch Path Execution (Exception Handling)

4d. **shape20** (branch - Error Handling Branch)
   - **SPLITS:** Path 1 (shape19) | Path 2 (shape41)
   - **Execution:** PARALLEL (both paths execute simultaneously)

**Path 1: Email Notification**

5d. **shape19** (documentproperties - ErrorMsg)
   - **READS:** meta.base.catcherrorsmessage
   - **WRITES:** process.DPP_ErrorMessage

6d. **shape21** (processcall - Email Subprocess)
   - **READS:** process.DPP_Process_Name, process.DPP_AtomName, process.DPP_Payload, process.DPP_ExecutionID, process.DPP_File_Name, process.DPP_Subject, process.To_Email, process.DPP_HasAttachment, process.DPP_ErrorMessage
   - **Subprocess:** a85945c5-3004-42b9-80b1-104f465cd1fb
   - **Action:** Sends error notification email
   - **TERMINAL:** Subprocess returns (main process continues)

**Path 2: Error Response**

5e. **shape41** (map - Leave Error Map)
   - **READS:** process.DPP_ErrorMessage (via PropertyGet function)
   - **WRITES:** D365 error response format
   - **Map:** f46b845a-7d75-41b5-b0ad-c41a6a8e9b12

6e. **shape43** (returndocuments - Error Response)
   - **READS:** Mapped error response
   - **RETURNS:** HTTP 500 with error response
   - **TERMINAL:** Process ends

### Dependency Verification

**Property Dependency Verification:**

✅ **process.DPP_ErrorMessage:**
- Written by: shape19 (catch path), shape39 (HTTP error no GZIP), shape46 (HTTP error GZIP)
- Read by: map f46b845a via PropertyGet function (shapes 40, 41, 47)
- **Verification:** All writes occur BEFORE map reads (sequential execution in each path)

✅ **dynamicdocument.URL:**
- Written by: shape8
- Read by: shape33 (HTTP operation)
- **Verification:** shape8 executes BEFORE shape33 (sequential execution)

✅ **meta.base.applicationstatuscode:**
- Written by: shape33 (HTTP response)
- Read by: shape2 (decision)
- **Verification:** shape33 executes BEFORE shape2 (sequential execution)

✅ **dynamicdocument.DDP_RespHeader:**
- Written by: shape33 (HTTP response header mapping)
- Read by: shape44 (decision)
- **Verification:** shape33 executes BEFORE shape44 (sequential execution)

✅ **Email-related properties (DPP_Process_Name, DPP_AtomName, etc.):**
- Written by: shape38
- Read by: shape21 subprocess
- **Verification:** shape38 executes BEFORE shape17 (which leads to shape21) (sequential execution)

**All property reads happen AFTER property writes.** ✅

---

## 13. Sequence Diagram (Step 10)

✅ **MANDATORY FORMAT - CREATED AFTER STEPS 4, 5, 7, 8, 9**

**📋 NOTE:** Detailed request/response JSON examples are documented in:
- **Section 7: HTTP Status Codes and Return Path Responses** - For response JSON with populated fields for return paths
- **Section 16: Request/Response JSON Examples** - For detailed request/response JSON examples

**Pre-Creation Validation:**
- ✅ Step 4 (Data Dependency Graph) - COMPLETE and DOCUMENTED
- ✅ Step 5 (Control Flow Graph) - COMPLETE and DOCUMENTED
- ✅ Step 7 (Decision Analysis) - COMPLETE and DOCUMENTED
- ✅ Step 8 (Branch Analysis) - COMPLETE and DOCUMENTED
- ✅ Step 9 (Execution Order) - COMPLETE and DOCUMENTED

**References:**
- Based on dependency graph in Step 4 (Section 8)
- Based on decision analysis in Step 7 (Section 10)
- Based on control flow graph in Step 5 (Section 9)
- Based on branch analysis in Step 8 (Section 11)
- Based on execution order in Step 9 (Section 12)

### Main Process Flow

```
START (shape1 - WebServices Server Listen)
 |
 ├─→ shape38: Input_details (documentproperties)
 |   └─→ WRITES: process.DPP_Process_Name, process.DPP_AtomName, process.DPP_Payload, 
 |                process.DPP_ExecutionID, process.DPP_File_Name, process.DPP_Subject,
 |                process.To_Email, process.DPP_HasAttachment
 |
 ├─→ shape17: Try/Catch Wrapper (catcherrors)
 |   |
 |   ├─→ TRY PATH:
 |   |   |
 |   |   ├─→ shape29: Leave Create Map (map c426b4d6)
 |   |   |   └─→ READS: Input document (D365 leave request)
 |   |   |   └─→ WRITES: Transformed document (Oracle Fusion format)
 |   |   |
 |   |   ├─→ shape8: set URL (documentproperties)
 |   |   |   └─→ READS: Defined parameter Resource_Path
 |   |   |   └─→ WRITES: dynamicdocument.URL
 |   |   |
 |   |   ├─→ shape49: Log Payload (notify)
 |   |   |   └─→ READS: Current document
 |   |   |
 |   |   ├─→ shape33: Leave Oracle Fusion Create (Downstream - System Layer)
 |   |   |   └─→ READS: dynamicdocument.URL, Current document
 |   |   |   └─→ WRITES: Response document, meta.base.applicationstatuscode,
 |   |   |                dynamicdocument.DDP_RespHeader
 |   |   |   └─→ HTTP: POST to Oracle Fusion absences API
 |   |   |   └─→ Expected: 20* (200, 201, etc.)
 |   |   |   └─→ Errors: 400, 401, 404, 500
 |   |   |
 |   |   ├─→ Decision (shape2): HTTP Status 20 check
 |   |   |   └─→ READS: meta.base.applicationstatuscode
 |   |   |   └─→ CONDITION: Wildcard match "20*"?
 |   |   |   |
 |   |   |   ├─→ IF TRUE (Success - 20x status):
 |   |   |   |   |
 |   |   |   |   ├─→ shape34: Oracle Fusion Leave Response Map (map e4fd3f59)
 |   |   |   |   |   └─→ READS: Response document (personAbsenceEntryId)
 |   |   |   |   |   └─→ WRITES: D365 success response format
 |   |   |   |   |
 |   |   |   |   └─→ shape35: Return Documents [HTTP: 200] [Success Response] [SUCCESS]
 |   |   |   |       └─→ Response: {"status":"success", "message":"Data successfully sent to Oracle Fusion",
 |   |   |   |                      "personAbsenceEntryId":123456, "success":"true"}
 |   |   |   |
 |   |   |   └─→ IF FALSE (Error - Non-20x status):
 |   |   |       |
 |   |   |       ├─→ Decision (shape44): Check Response Content Type
 |   |   |       |   └─→ READS: dynamicdocument.DDP_RespHeader
 |   |   |       |   └─→ CONDITION: Equals "gzip"?
 |   |   |       |   |
 |   |   |       |   ├─→ IF TRUE (GZIP compressed error):
 |   |   |       |   |   |
 |   |   |       |   |   ├─→ shape45: Decompress GZIP (dataprocess - Groovy script)
 |   |   |       |   |   |   └─→ READS: Current document (compressed)
 |   |   |       |   |   |   └─→ WRITES: Decompressed document
 |   |   |       |   |   |
 |   |   |       |   |   ├─→ shape46: Extract error msg (documentproperties)
 |   |   |       |   |   |   └─→ READS: meta.base.applicationstatusmessage
 |   |   |       |   |   |   └─→ WRITES: process.DPP_ErrorMessage
 |   |   |       |   |   |
 |   |   |       |   |   ├─→ shape47: Leave Error Map (map f46b845a)
 |   |   |       |   |   |   └─→ READS: process.DPP_ErrorMessage (via PropertyGet function)
 |   |   |       |   |   |   └─→ WRITES: D365 error response format
 |   |   |       |   |   |
 |   |   |       |   |   └─→ shape48: Return Documents [HTTP: 400] [Error Response] [EARLY EXIT]
 |   |   |       |   |       └─→ Response: {"status":"failure", "message":"[Oracle error]", "success":"false"}
 |   |   |       |   |
 |   |   |       |   └─→ IF FALSE (Non-GZIP error):
 |   |   |       |       |
 |   |   |       |       ├─→ shape39: Extract error msg (documentproperties)
 |   |   |       |       |   └─→ READS: meta.base.applicationstatusmessage
 |   |   |       |       |   └─→ WRITES: process.DPP_ErrorMessage
 |   |   |       |       |
 |   |   |       |       ├─→ shape40: Leave Error Map (map f46b845a)
 |   |   |       |       |   └─→ READS: process.DPP_ErrorMessage (via PropertyGet function)
 |   |   |       |       |   └─→ WRITES: D365 error response format
 |   |   |       |       |
 |   |   |       |       └─→ shape36: Return Documents [HTTP: 400] [Error Response] [EARLY EXIT]
 |   |   |       |           └─→ Response: {"status":"failure", "message":"[Oracle error]", "success":"false"}
 |   |
 |   └─→ CATCH PATH (Exception occurred):
 |       |
 |       └─→ shape20: Error Handling Branch (branch - 2 paths PARALLEL)
 |           |
 |           ├─→ PATH 1: Email Notification
 |           |   |
 |           |   ├─→ shape19: Extract catch error (documentproperties)
 |           |   |   └─→ READS: meta.base.catcherrorsmessage
 |           |   |   └─→ WRITES: process.DPP_ErrorMessage
 |           |   |
 |           |   └─→ shape21: Email Subprocess Call (processcall a85945c5)
 |           |       └─→ READS: process.DPP_Process_Name, process.DPP_AtomName,
 |           |                  process.DPP_Payload, process.DPP_ExecutionID,
 |           |                  process.DPP_File_Name, process.DPP_Subject,
 |           |                  process.To_Email, process.DPP_HasAttachment,
 |           |                  process.DPP_ErrorMessage
 |           |       └─→ ACTION: Sends error notification email
 |           |       └─→ SUBPROCESS: (Sub) Office 365 Email (see subprocess flow below)
 |           |
 |           └─→ PATH 2: Error Response (executes in PARALLEL with PATH 1)
 |               |
 |               ├─→ shape41: Leave Error Map (map f46b845a)
 |               |   └─→ READS: process.DPP_ErrorMessage (via PropertyGet function)
 |               |   └─→ WRITES: D365 error response format
 |               |
 |               └─→ shape43: Return Documents [HTTP: 500] [Error Response] [EARLY EXIT]
 |                   └─→ Response: {"status":"failure", "message":"[Exception message]", "success":"false"}

**Note:** Detailed request/response JSON examples for all operations and return paths are documented in 
Section 7 (HTTP Status Codes and Return Path Responses) and Section 16 (Request/Response JSON Examples).
```

### Subprocess Flow: (Sub) Office 365 Email (a85945c5-3004-42b9-80b1-104f465cd1fb)

```
SUBPROCESS START (called from main process shape21)
 |
 ├─→ shape2: Try/Catch Wrapper (catcherrors)
 |   |
 |   ├─→ TRY PATH:
 |   |   |
 |   |   ├─→ Decision (shape4): Attachment_Check
 |   |   |   └─→ READS: process.DPP_HasAttachment
 |   |   |   └─→ CONDITION: Equals "Y"?
 |   |   |   |
 |   |   |   ├─→ IF TRUE (With Attachment):
 |   |   |   |   |
 |   |   |   |   ├─→ shape11: Build Email Body HTML (message)
 |   |   |   |   |   └─→ READS: process.DPP_Process_Name, process.DPP_AtomName,
 |   |   |   |   |                process.DPP_ExecutionID, process.DPP_ErrorMessage
 |   |   |   |   |   └─→ WRITES: Email body HTML
 |   |   |   |   |
 |   |   |   |   ├─→ shape14: Set Mail Body (documentproperties)
 |   |   |   |   |   └─→ READS: Current document (email HTML)
 |   |   |   |   |   └─→ WRITES: process.DPP_MailBody
 |   |   |   |   |
 |   |   |   |   ├─→ shape15: Set Payload as Attachment (message)
 |   |   |   |   |   └─→ READS: process.DPP_Payload
 |   |   |   |   |   └─→ WRITES: Attachment content
 |   |   |   |   |
 |   |   |   |   ├─→ shape6: Set Mail Properties (documentproperties)
 |   |   |   |   |   └─→ READS: Defined parameter From_Email, process.To_Email,
 |   |   |   |   |                Defined parameter Environment, process.DPP_Subject,
 |   |   |   |   |                process.DPP_MailBody, process.DPP_File_Name
 |   |   |   |   |   └─→ WRITES: connector.mail.fromAddress, connector.mail.toAddress,
 |   |   |   |   |                connector.mail.subject, connector.mail.body,
 |   |   |   |   |                connector.mail.filename
 |   |   |   |   |
 |   |   |   |   ├─→ shape3: Send Email with Attachment (Downstream - System Layer)
 |   |   |   |   |   └─→ READS: Mail connector properties
 |   |   |   |   |   └─→ OPERATION: af07502a (Email w Attachment)
 |   |   |   |   |   └─→ ACTION: Send email via Office 365 SMTP
 |   |   |   |   |
 |   |   |   |   └─→ shape5: Stop (continue=true) [SUCCESS RETURN]
 |   |   |   |
 |   |   |   └─→ IF FALSE (Without Attachment):
 |   |   |       |
 |   |   |       ├─→ shape23: Build Email Body HTML (message)
 |   |   |       |   └─→ READS: process.DPP_Process_Name, process.DPP_AtomName,
 |   |   |       |                process.DPP_ExecutionID, process.DPP_ErrorMessage
 |   |   |       |   └─→ WRITES: Email body HTML
 |   |   |       |
 |   |   |       ├─→ shape22: Set Mail Body (documentproperties)
 |   |   |       |   └─→ READS: Current document (email HTML)
 |   |   |       |   └─→ WRITES: process.DPP_MailBody
 |   |   |       |
 |   |   |       ├─→ shape20: Set Mail Properties (documentproperties)
 |   |   |       |   └─→ READS: Defined parameter From_Email, process.To_Email,
 |   |   |       |                Defined parameter Environment, process.DPP_Subject,
 |   |   |       |                process.DPP_MailBody
 |   |   |       |   └─→ WRITES: connector.mail.fromAddress, connector.mail.toAddress,
 |   |   |       |                connector.mail.subject, connector.mail.body
 |   |   |       |
 |   |   |       ├─→ shape7: Send Email without Attachment (Downstream - System Layer)
 |   |   |       |   └─→ READS: Mail connector properties
 |   |   |       |   └─→ OPERATION: 15a72a21 (Email W/O Attachment)
 |   |   |       |   └─→ ACTION: Send email via Office 365 SMTP
 |   |   |       |
 |   |   |       └─→ shape9: Stop (continue=true) [SUCCESS RETURN]
 |   |
 |   └─→ CATCH PATH (Exception in email):
 |       |
 |       └─→ shape10: Throw Exception (exception)
 |           └─→ READS: meta.base.catcherrorsmessage
 |           └─→ ACTION: Throw exception with error message
 |           └─→ [EXCEPTION - Propagates to main process]

SUBPROCESS END (returns to main process)
```

---

## 14. Subprocess Analysis (Step 7a)

### Subprocess: (Sub) Office 365 Email (a85945c5-3004-42b9-80b1-104f465cd1fb)

**Subprocess ID:** a85945c5-3004-42b9-80b1-104f465cd1fb  
**Subprocess Name:** (Sub) Office 365 Email  
**Purpose:** Send error notification emails (with or without attachment)

### Internal Flow

**Entry Point:** shape1 (start - passthrough)

**Main Decision:** shape4 (Attachment_Check)
- Checks process.DPP_HasAttachment
- TRUE path: Send email with attachment (shapes 11→14→15→6→3→5)
- FALSE path: Send email without attachment (shapes 23→22→20→7→9)

**Error Handling:** shape2 (Try/Catch)
- Try path: Normal email flow
- Catch path: shape10 (throw exception)

### Return Paths

| Return Type | Shape | Condition | Returns To |
|-------------|-------|-----------|------------|
| SUCCESS | shape5 | Email sent with attachment | Main process (continues) |
| SUCCESS | shape9 | Email sent without attachment | Main process (continues) |
| EXCEPTION | shape10 | Error sending email | Main process (exception propagates) |

**Return Path Mapping to Main Process:**

The subprocess is called by shape21 in the main process. Since the processcall configuration shows `abort="true"` and `wait="true"`, the subprocess is synchronous and waits for completion. No explicit return path labels are defined, so the subprocess returns to the main process after completion (Stop with continue=true) or throws an exception.

### Properties Written by Subprocess

| Property Name | Written By Shape(s) | Value |
|---------------|---------------------|-------|
| process.DPP_MailBody | shape14, shape22 | Email body HTML content |

### Properties Read by Subprocess (from Main Process)

| Property Name | Read By Shape(s) | Usage |
|---------------|------------------|-------|
| process.DPP_HasAttachment | shape4 | Decision: with/without attachment |
| process.DPP_Process_Name | shape11, shape23 | Email body parameter |
| process.DPP_AtomName | shape11, shape23 | Email body parameter |
| process.DPP_ExecutionID | shape11, shape23 | Email body parameter |
| process.DPP_ErrorMessage | shape11, shape23 | Email body parameter |
| process.DPP_Payload | shape15 | Email attachment content |
| process.To_Email | shape6, shape20 | Mail To address |
| process.DPP_Subject | shape6, shape20 | Mail subject |
| process.DPP_File_Name | shape6 | Mail attachment filename |

### Operations Used

| Operation ID | Operation Name | Type | Purpose |
|--------------|----------------|------|---------|
| af07502a-fafd-4976-a691-45d51a33b549 | Email w Attachment | mail | Send email with attachment (shape3) |
| 15a72a21-9b57-49a1-a8ed-d70367146644 | Email W/O Attachment | mail | Send email without attachment (shape7) |

### Business Logic

The subprocess implements conditional email sending:
1. Checks if attachment is required (DPP_HasAttachment = "Y")
2. Builds HTML email body with execution details and error message
3. Sets mail properties (from, to, subject, body, filename)
4. Sends email via Office 365 SMTP (with or without attachment based on flag)
5. Returns to main process on success or throws exception on failure

**Note:** Both paths (with/without attachment) use the same email body template showing process name, environment, execution ID, and error details.

---

## 15. System Layer Identification

### Downstream System Layer Calls

This process makes calls to the following external systems (System Layer):

#### 1. Oracle Fusion HCM

**System:** Oracle Fusion Cloud - Human Capital Management  
**Purpose:** Create leave/absence entries  
**Operation:** Leave Oracle Fusion Create (6e8920fd-af5a-430b-a1d9-9fde7ac29a12)  
**Connection:** aa1fcb29-d146-4425-9ea6-b9698090f60e (Oracle Fusion)  
**Endpoint:** https://iaaxey-dev3.fa.ocs.oraclecloud.com:443  
**API Path:** hcmRestApi/resources/11.13.18.05/absences  
**Method:** POST  
**Authentication:** Basic Auth (INTEGRATION.USER@al-ghurair.com)  
**Request Format:** JSON (application/json)  
**Response Format:** JSON (application/json)  
**Used in Shape:** shape33

**Request Fields:**
- personNumber (from employeeNumber)
- absenceType
- employer
- startDate, endDate
- absenceStatusCd, approvalStatusCd
- startDateDuration, endDateDuration

**Response Fields:**
- personAbsenceEntryId (primary response field)
- Plus 70+ additional Oracle fields (full leave entry data)

#### 2. Office 365 Email (SMTP)

**System:** Microsoft Office 365  
**Purpose:** Send error notification emails  
**Operations:**
- Email w Attachment (af07502a-fafd-4976-a691-45d51a33b549)
- Email W/O Attachment (15a72a21-9b57-49a1-a8ed-d70367146644)  
**Connection:** 00eae79b-2303-4215-8067-dcc299e42697 (Office 365 Email)  
**Endpoint:** smtp-mail.outlook.com:587  
**Authentication:** SMTP AUTH (Boomi.Dev.failures@al-ghurair.com)  
**Protocol:** SMTP with TLS  
**Used in Subprocess Shapes:** shape3, shape7

**Email Fields:**
- From: Configured via defined parameter (Boomi.Dev.failures@al-ghurair.com)
- To: Configured via defined parameter (BoomiIntegrationTeam@al-ghurair.com)
- Subject: Dynamic (process name + "has errors to report")
- Body: HTML with execution details and error message
- Attachment: Optional (input payload if DPP_HasAttachment = "Y")

### System Layer Boundary

**Process Layer (This Process):**
- Receives leave request from D365 (WebServices Server Listen)
- Transforms data between D365 and Oracle formats
- Orchestrates error handling and notifications
- Returns response to D365

**System Layer (External Calls):**
- Oracle Fusion HCM API (leave creation)
- Office 365 SMTP (email notifications)

**Experience Layer (Caller):**
- D365 (Dynamics 365) - calls this process to sync leave data

---

## 16. Request/Response JSON Examples

### Process Layer Entry Point

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
  "personAbsenceEntryId": 300000123456789,
  "success": "true"
}
```

**Error Response - Try/Catch Exception (HTTP 500):**

```json
{
  "status": "failure",
  "message": "Error occurred during processing: [specific error details from exception]",
  "success": "false"
}
```

**Error Response - HTTP Non-20x (HTTP 400):**

```json
{
  "status": "failure",
  "message": "Oracle Fusion API Error: Invalid absence type or required field missing",
  "success": "false"
}
```

**Error Response - HTTP Non-20x with GZIP (HTTP 400):**

```json
{
  "status": "failure",
  "message": "Oracle Fusion API Error (decompressed): Validation error - Employee not found",
  "success": "false"
}
```

### Downstream System Layer Calls

#### Oracle Fusion HCM - Leave Create API

**Request JSON (to Oracle Fusion):**

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

**Success Response JSON (from Oracle Fusion - HTTP 200):**

```json
{
  "personAbsenceEntryId": 300000123456789,
  "absenceCaseId": null,
  "absenceEntryBasicFlag": false,
  "absencePatternCd": null,
  "absenceStatusCd": "SUBMITTED",
  "absenceTypeId": 300000004567890,
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
  "createdBy": "INTEGRATION.USER",
  "creationDate": "2024-03-24T10:30:15.000Z",
  "diseaseCode": null,
  "duration": 2.0,
  "employeeShiftFlag": false,
  "endDate": "2024-03-25",
  "endDateDuration": 1.0,
  "endDateTime": null,
  "endTime": null,
  "establishmentDate": null,
  "frequency": null,
  "initialReportById": null,
  "initialTimelyNotifyFlag": null,
  "lastUpdateDate": "2024-03-24T10:30:15.000Z",
  "lastUpdateLogin": null,
  "lastUpdatedBy": "INTEGRATION.USER",
  "lateNotifyFlag": null,
  "legalEntityId": 300000001234567,
  "legislationCode": "AE",
  "legislativeDataGroupId": 300000001234567,
  "notificationDate": null,
  "objectVersionNumber": 1,
  "openEndedFlag": false,
  "overridden": null,
  "periodOfIncapToWorkFlag": null,
  "periodOfServiceId": 300000009876543,
  "personId": 300000002345678,
  "plannedEndDate": null,
  "processingStatus": null,
  "projectId": null,
  "singleDayFlag": false,
  "source": "Integration",
  "splCondition": null,
  "startDate": "2024-03-24",
  "startDateDuration": 1.0,
  "startDateTime": null,
  "startTime": null,
  "submittedDate": "2024-03-24",
  "timelinessOverrideDate": null,
  "unitOfMeasure": "D",
  "userMode": null,
  "personNumber": "9000604",
  "absenceType": "Sick Leave",
  "employer": "Al Ghurair Investment LLC",
  "absenceReason": null,
  "absenceDispStatus": "Submitted",
  "assignmentId": "300000003456789",
  "dataSecurityPersonId": 300000002345678,
  "effectiveEndDate": null,
  "effectiveStartDate": "2024-03-24",
  "ObjectVersionNumber": 1,
  "agreementName": null,
  "paymentDetail": null,
  "assignmentName": "Employee Assignment",
  "assignmentNumber": "E9000604",
  "unitOfMeasureMeaning": "Days",
  "formattedDuration": "2 Days",
  "absenceDispStatusMeaning": "Submitted",
  "absenceUpdatableFlag": "Y",
  "ApprovalDatetime": null,
  "allowAssignmentSelectionFlag": false,
  "links": [
    {
      "rel": "self",
      "href": "https://iaaxey-dev3.fa.ocs.oraclecloud.com:443/hcmRestApi/resources/11.13.18.05/absences/300000123456789",
      "name": "absences",
      "kind": "item",
      "properties": {
        "changeIndicator": "ACED00057..."
      }
    },
    {
      "rel": "canonical",
      "href": "https://iaaxey-dev3.fa.ocs.oraclecloud.com:443/hcmRestApi/resources/11.13.18.05/absences/300000123456789",
      "name": "absences",
      "kind": "item"
    }
  ]
}
```

**Error Response JSON (from Oracle Fusion - HTTP 400):**

```json
{
  "title": "Bad Request",
  "status": 400,
  "detail": "Validation error occurred",
  "o:errorDetails": [
    {
      "detail": "The employee number provided does not exist in the system.",
      "o:errorCode": "PER_INVALID_EMPLOYEE"
    }
  ]
}
```

**Error Response JSON (from Oracle Fusion - HTTP 401):**

```json
{
  "title": "Unauthorized",
  "status": 401,
  "detail": "Authentication failed. Invalid credentials provided."
}
```

---

## 17. Function Exposure Decision Table

✅ **MANDATORY STEP - PREVENTS FUNCTION EXPLOSION**

### Function Exposure Analysis

This section determines which functions should be exposed at the Process Layer vs delegated to System Layer.

| Function Name | Current Location | Expose at Process Layer? | Reasoning | System Layer Delegation |
|---------------|------------------|--------------------------|-----------|------------------------|
| Create Leave in Oracle Fusion | Main Process (orchestration) | ✅ YES | **Core business process** - Orchestrates D365→Oracle leave sync with error handling and notifications | Oracle Fusion System Layer API handles actual leave creation |
| Transform D365 to Oracle Format | Main Process (map c426b4d6) | ❌ NO - Internal Only | **Data transformation logic** - Simple field mapping (employeeNumber→personNumber, statusCode→statusCd) | No - Keep as internal mapping within Process Layer |
| Transform Oracle Response to D365 | Main Process (map e4fd3f59) | ❌ NO - Internal Only | **Response transformation** - Extracts personAbsenceEntryId + adds success flags | No - Keep as internal mapping within Process Layer |
| Error Response Mapping | Main Process (map f46b845a) | ❌ NO - Internal Only | **Error transformation** - Maps error message to D365 response format | No - Keep as internal mapping within Process Layer |
| Send Error Notification Email | Subprocess (a85945c5) | ❌ NO - System Layer Only | **System integration** - Email notification is pure infrastructure concern | ✅ YES - Office 365 Email System Layer handles email sending |
| GZIP Decompression | Main Process (shape45) | ❌ NO - Internal Only | **Technical processing** - Handles compressed Oracle responses | No - Keep as internal processing within Process Layer |

### Process Layer Exposure Decision

**✅ EXPOSE THIS FUNCTION:**

**Function Name:** Create Leave in Oracle Fusion  
**Exposure Type:** Process Layer HTTP/SOAP API  
**Rationale:**
- This is a complete business process that orchestrates leave data synchronization between D365 and Oracle Fusion
- Encapsulates business logic: error handling, response mapping, notification on errors
- Provides a stable contract to D365 (Experience Layer)
- Abstracts Oracle Fusion API complexity from callers
- Handles multiple error scenarios with appropriate responses

**API Contract:**

- **Endpoint:** WebServices Server (WSS) - Leave Create
- **Method:** POST
- **Input:** D365 Leave Create JSON
- **Output:** Leave Response JSON (success/failure with personAbsenceEntryId)
- **Error Handling:** Returns structured error responses for all failure scenarios

### System Layer Dependencies

This Process Layer function depends on the following System Layer APIs:

| System Layer API | Purpose | Required? | Error Handling |
|------------------|---------|-----------|----------------|
| Oracle Fusion - Absences API | Create leave entry | ✅ YES | Returns error response to caller if Oracle fails |
| Office 365 - Email SMTP | Send error notifications | ❌ NO (notification only) | Subprocess throws exception if email fails, but main process continues to return error response |

### Recommendation

**Process Layer Function to Expose:**
- **HCM_Leave_Create** (this process) - Expose as Process Layer API for D365 integration

**System Layer Functions (Already Exposed):**
- **Oracle Fusion Absences API** - System Layer (external system)
- **Office 365 Email SMTP** - System Layer (infrastructure)

**Internal Components (Do Not Expose):**
- Data transformation maps (c426b4d6, e4fd3f59, f46b845a)
- GZIP decompression logic (shape45)
- Error message extraction logic (shape19, shape39, shape46)

### Migration Impact

**Azure Function Design:**
- Create **single Process Layer Azure Function**: `HCM_Leave_Create`
- This function will:
  - Accept D365 leave request (HTTP POST)
  - Transform to Oracle format
  - Call Oracle Fusion System Layer API (via HTTP client)
  - Handle success/error responses
  - Send email notification on errors (via Office 365 System Layer API)
  - Return structured response to D365

**No Function Explosion:** By following this analysis, we create **1 Process Layer function** instead of potentially creating 5-6 separate functions for each transformation/error handling step.

---

## VALIDATION CHECKLIST

### Data Dependencies ✅
- [x] All property WRITES identified
- [x] All property READS identified
- [x] Dependency graph built
- [x] Execution order satisfies all dependencies (no read-before-write)

### Decision Analysis ✅
- [x] ALL decision shapes inventoried (2 main process, 1 subprocess)
- [x] BOTH TRUE and FALSE paths traced to termination
- [x] Pattern type identified for each decision
- [x] Early exits identified and documented
- [x] Convergence points identified (none found)
- [x] Data source analysis complete (POST-OPERATION decisions)

### Branch Analysis ✅
- [x] Each branch classified as parallel or sequential (1 branch - PARALLEL)
- [x] Classification based on analysis, not assumption
- [x] Each path traced to terminal point
- [x] Convergence points identified (none)
- [x] Execution continuation point determined

### Sequence Diagram ✅
- [x] Format follows required structure
- [x] Each operation shows READS and WRITES
- [x] Decisions show both TRUE and FALSE paths
- [x] HTTP operations marked as (Downstream - System Layer)
- [x] Early exits marked [EARLY EXIT]
- [x] Subprocess internal flows documented
- [x] Subprocess return paths mapped to main process

### Subprocess Analysis ✅
- [x] Subprocess analyzed (internal flow traced)
- [x] Return paths identified (success paths via Stop continue=true)
- [x] Return path labels mapped to main process
- [x] Properties written by subprocess documented
- [x] Properties read by subprocess from main process documented

### Input/Output Structure Analysis ✅
- [x] Entry point operation identified (8f709c2b - WSS)
- [x] Request profile identified and loaded (febfa3e1)
- [x] Request profile structure analyzed (JSON)
- [x] Array vs single object detected (single object)
- [x] ALL request fields extracted (9 fields)
- [x] Request field mapping table generated
- [x] Response profile identified and loaded (f4ca3a70)
- [x] Response profile structure analyzed
- [x] ALL response fields extracted (4 fields)
- [x] Response field mapping table generated
- [x] Document processing behavior determined (single execution)

### HTTP Status Codes and Return Paths ✅
- [x] All return paths documented with HTTP status codes (4 return paths)
- [x] Response JSON examples provided for each return path
- [x] Populated fields documented for each return path
- [x] Decision conditions leading to each return documented
- [x] Error codes and success codes documented
- [x] Downstream operation HTTP status codes documented

### Map Analysis ✅
- [x] ALL map files identified and loaded (3 maps)
- [x] Field mappings extracted from each map
- [x] Profile vs map field name discrepancies documented
- [x] Map field names marked as AUTHORITATIVE
- [x] Scripting functions analyzed (PropertyGet in error map)
- [x] Static values identified and documented
- [x] Process property mappings documented

### Function Exposure Decision ✅
- [x] Function exposure decision table complete
- [x] Process Layer exposure identified (1 function to expose)
- [x] System Layer dependencies documented
- [x] Migration impact analyzed
- [x] Function explosion prevented

---

## EXTRACTION COMPLETE ✅

**Phase 1 Status:** COMPLETE  
**All Mandatory Steps:** COMPLETED  
**All Self-Checks:** PASSED (all YES answers)  
**Ready for Phase 2:** ✅ YES

**Summary:**
- Process receives leave requests from D365
- Transforms data to Oracle Fusion format
- Calls Oracle Fusion absences API to create leave entry
- Handles success (returns personAbsenceEntryId) and error responses
- Sends email notifications on errors
- Returns structured response to D365

**Next Step:** Proceed to Phase 2 (Code Generation) with this complete analysis.
