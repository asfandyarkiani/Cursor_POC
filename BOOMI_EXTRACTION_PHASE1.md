# BOOMI EXTRACTION PHASE 1 - Create Work Order from EQ+ to CAFM

**Process Name:** Create Work Order from EQ+ to CAFM  
**Process ID:** cf0ab01d-2ce4-4588-8265-54fc4290368a  
**System of Record (SOR):** CAFM (FSI - Facilities System International)  
**Integration Type:** SOAP/XML over HTTP  
**Authentication:** Session-based (Login → SessionId → Logout)  
**Extraction Date:** 2026-01-28  
**Version:** 90 (Modified: 2025-11-13)

---

## TABLE OF CONTENTS

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
14. [Subprocess Analysis](#14-subprocess-analysis)
15. [Critical Patterns Identified](#15-critical-patterns-identified)
16. [System Layer Identification](#16-system-layer-identification)
17. [Request/Response JSON Examples](#17-requestresponse-json-examples)
18. [Function Exposure Decision Table](#18-function-exposure-decision-table)
19. [Validation Checklist](#19-validation-checklist)

---

## 1. OPERATIONS INVENTORY

### 1.1 Main Process Entry Point

**Operation:** EQ+_CAFM_Create (Web Service Server Listen)  
**Operation ID:** de68dad0-be76-4ec8-9857-4e5cf2a7bd4c  
**Type:** wss (Web Service Server)  
**Input Type:** singlejson  
**Request Profile:** af096014-313f-4565-9091-2bdd56eb46df (EQ+_CAFM_Create_Request)  
**Response Profile:** 9e542ed5-2c65-4af8-b0c6-821cbc58ca31 (EQ+_CAFM_Create_Response)

### 1.2 CAFM SOAP Operations

| Operation Name | Operation ID | Type | Method | Request Profile | Response Profile |
|---|---|---|---|---|---|
| Login | c20e5991-4d70-47f7-8e25-847df3e5eb6d | http/SOAP | POST | 60e1aeea-348e-4505-8e49-e823a6a82194 | 992136d3-da44-4f22-994b-f7181624215b |
| GetBreakdownTasksByDto | c52c74c2-95e3-4cba-990e-3ce4746836a2 | http/SOAP | POST | 004838f5-51d7-4438-a693-aa82bdef7181 | 1570c9d2-0588-410d-ad3d-bdc7d5c0ec9a |
| GetLocationsByDto | 442683cb-b984-499e-b7bb-075c826905aa | http/SOAP | POST | 589e623c-b91f-4d3c-a5aa-3c767033abc5 | 3aa0f5c5-8c95-4023-aba9-9d78dd6ade96 |
| GetInstructionSetsByDto | dc3b6b85-848d-471d-8c76-ed3b7dea0fbd | http/SOAP | POST | 589e623c-b91f-4d3c-a5aa-3c767033abc5 | 5c2f13dd-3e51-4a7c-867b-c801aaa35562 |
| CreateBreakdownTask | 33dac20f-ea09-471c-91c3-91b39bc3b172 | http/SOAP | POST | 362c3ec8-053c-4694-8a26-cdb931e6a411 | dbcca2ef-55cc-48e0-9329-1e8db4ada0c8 |
| CreateEvent/Link task | 52166afd-a020-4de9-b49e-55400f1c0a7a | http/SOAP | POST | 23f4cc6e-f46c-47fe-ad9d-6dc191adefb9 | 449782a0-4b04-4a7a-aa5c-aa9265fd2614 |
| Session logout | 381a025b-f3b9-4597-9902-3be49715c978 | http/SOAP | POST | 6b3afee8-54cf-4310-83ff-038ddcdc3f9a | NONE |

### 1.3 Email Operations

| Operation Name | Operation ID | Type | Purpose |
|---|---|---|---|
| Email W/O Attachment | 15a72a21-9b57-49a1-a8ed-d70367146644 | mail | Send email without attachment |
| Email w Attachment | af07502a-fafd-4976-a691-45d51a33b549 | mail | Send email with attachment |

### 1.4 Subprocesses

| Subprocess Name | Subprocess ID | Purpose |
|---|---|---|
| FsiLogin | 3d9db79d-15d0-4472-9f47-375ad9ab1ed2 | Authenticate with CAFM, get SessionId |
| FsiLogout | b44c26cb-ecd5-4677-a752-434fe68f2e2b | Logout from CAFM session |
| Office 365 Email | a85945c5-3004-42b9-80b1-104f465cd1fb | Send email notification |

---

## 2. INPUT STRUCTURE ANALYSIS (Step 1a)

**🛑 VALIDATION CHECKPOINT:** This section is COMPLETE and MANDATORY before Phase 2.

### 2.1 Request Profile Structure

**Profile ID:** af096014-313f-4565-9091-2bdd56eb46df  
**Profile Name:** EQ+_CAFM_Create_Request  
**Profile Type:** profile.json  
**Root Structure:** Root/Object/workOrder/Array/ArrayElement1/Object/...  
**Array Detection:** ✅ YES - workOrder is an array  
**Array Cardinality:**
- minOccurs: 0
- maxOccurs: -1 (unlimited)

**Input Type:** singlejson

### 2.2 Input Format

```json
{
  "workOrder": [
    {
      "reporterName": "string",
      "reporterEmail": "string",
      "reporterPhoneNumber": "string",
      "description": "string",
      "serviceRequestNumber": "string",
      "propertyName": "string",
      "unitCode": "string",
      "categoryName": "string",
      "subCategory": "string",
      "technician": "string",
      "sourceOrgId": "string",
      "ticketDetails": {
        "status": "string",
        "subStatus": "string",
        "priority": "string",
        "scheduledDate": "string",
        "scheduledTimeStart": "string",
        "scheduledTimeEnd": "string",
        "recurrence": "string",
        "oldCAFMSRnumber": "string",
        "raisedDateUtc": "string"
      }
    }
  ]
}
```

### 2.3 Document Processing Behavior

**Boomi Processing:** Each array element triggers separate process execution (inputType="singlejson" with array)  
**Azure Function Requirement:** Must accept array and process each element  
**Implementation Pattern:** Loop through array, process each work order, aggregate results

### 2.4 Field Mapping Table

| Boomi Field Path | Boomi Field Name | Data Type | Required | Azure DTO Property | Notes |
|---|---|---|---|---|---|
| Root/Object/workOrder/Array/ArrayElement1/Object/reporterName | reporterName | character | No | ReporterName | Reporter contact name |
| Root/Object/workOrder/Array/ArrayElement1/Object/reporterEmail | reporterEmail | character | No | ReporterEmail | Reporter email address |
| Root/Object/workOrder/Array/ArrayElement1/Object/reporterPhoneNumber | reporterPhoneNumber | character | No | ReporterPhoneNumber | Reporter phone |
| Root/Object/workOrder/Array/ArrayElement1/Object/description | description | character | No | Description | Work order description |
| Root/Object/workOrder/Array/ArrayElement1/Object/serviceRequestNumber | serviceRequestNumber | character | No | ServiceRequestNumber | EQ+ SR number |
| Root/Object/workOrder/Array/ArrayElement1/Object/propertyName | propertyName | character | No | PropertyName | Property name for location lookup |
| Root/Object/workOrder/Array/ArrayElement1/Object/unitCode | unitCode | character | No | UnitCode | Unit code for location lookup |
| Root/Object/workOrder/Array/ArrayElement1/Object/categoryName | categoryName | character | No | CategoryName | Category for instruction lookup |
| Root/Object/workOrder/Array/ArrayElement1/Object/subCategory | subCategory | character | No | SubCategory | Sub-category for instruction lookup |
| Root/Object/workOrder/Array/ArrayElement1/Object/technician | technician | character | No | Technician | Assigned technician |
| Root/Object/workOrder/Array/ArrayElement1/Object/sourceOrgId | sourceOrgId | character | No | SourceOrgId | Source organization ID |
| Root/Object/workOrder/Array/ArrayElement1/Object/ticketDetails/Object/status | status | character | No | Status | Ticket status |
| Root/Object/workOrder/Array/ArrayElement1/Object/ticketDetails/Object/subStatus | subStatus | character | No | SubStatus | Ticket sub-status |
| Root/Object/workOrder/Array/ArrayElement1/Object/ticketDetails/Object/priority | priority | character | No | Priority | Priority level |
| Root/Object/workOrder/Array/ArrayElement1/Object/ticketDetails/Object/scheduledDate | scheduledDate | character | No | ScheduledDate | Scheduled date (YYYY-MM-DD) |
| Root/Object/workOrder/Array/ArrayElement1/Object/ticketDetails/Object/scheduledTimeStart | scheduledTimeStart | character | No | ScheduledTimeStart | Scheduled start time (HH:mm:ss) |
| Root/Object/workOrder/Array/ArrayElement1/Object/ticketDetails/Object/scheduledTimeEnd | scheduledTimeEnd | character | No | ScheduledTimeEnd | Scheduled end time |
| Root/Object/workOrder/Array/ArrayElement1/Object/ticketDetails/Object/recurrence | recurrence | character | No | Recurrence | Recurrence flag (Y/N) |
| Root/Object/workOrder/Array/ArrayElement1/Object/ticketDetails/Object/oldCAFMSRnumber | oldCAFMSRnumber | character | No | OldCAFMSRnumber | Old CAFM SR number |
| Root/Object/workOrder/Array/ArrayElement1/Object/ticketDetails/Object/raisedDateUtc | raisedDateUtc | character | Yes | RaisedDateUtc | Raised date in UTC |

**Total Fields:** 20 (including nested ticketDetails object with 9 fields)

---

## 3. RESPONSE STRUCTURE ANALYSIS (Step 1b)

**🛑 VALIDATION CHECKPOINT:** This section is COMPLETE and MANDATORY before Phase 2.

### 3.1 Response Profile Structure

**Profile ID:** 9e542ed5-2c65-4af8-b0c6-821cbc58ca31  
**Profile Name:** EQ+_CAFM_Create_Response  
**Profile Type:** profile.json  
**Root Structure:** Root/Object/workOrder/Array/ArrayElement1/Object/...  
**Array Detection:** ✅ YES - workOrder is an array (matches request structure)

### 3.2 Response Format

```json
{
  "workOrder": [
    {
      "cafmSRNumber": "string",
      "sourceSRNumber": "string",
      "sourceOrgId": "string",
      "status": "string",
      "message": "string"
    }
  ]
}
```

### 3.3 Response Field Mapping Table

| Boomi Field Path | Boomi Field Name | Data Type | Azure DTO Property | Notes |
|---|---|---|---|---|
| Root/Object/workOrder/Array/ArrayElement1/Object/cafmSRNumber | cafmSRNumber | character | CafmSRNumber | Created CAFM task ID |
| Root/Object/workOrder/Array/ArrayElement1/Object/sourceSRNumber | sourceSRNumber | character | SourceSRNumber | Original EQ+ SR number |
| Root/Object/workOrder/Array/ArrayElement1/Object/sourceOrgId | sourceOrgId | character | SourceOrgId | Source organization ID |
| Root/Object/workOrder/Array/ArrayElement1/Object/status | status | character | Status | Success/failure status |
| Root/Object/workOrder/Array/ArrayElement1/Object/message | message | character | Message | Success/error message |

**Total Fields:** 5

---

## 4. OPERATION RESPONSE ANALYSIS (Step 1c)

**🛑 VALIDATION CHECKPOINT:** This section is COMPLETE and MANDATORY before Step 7.

### 4.1 Login Operation (Subprocess: FsiLogin)

**Operation ID:** c20e5991-4d70-47f7-8e25-847df3e5eb6d  
**Operation Name:** Login  
**Response Profile ID:** 992136d3-da44-4f22-994b-f7181624215b  
**Response Structure:** XML - Envelope/Body/AuthenticateResponse/AuthenticateResult/SessionId

**Extracted Fields:**
- SessionId (extracted by shape8 in FsiLogin subprocess, written to process.DPP_SessionId)

**Consumers:**
- All downstream CAFM operations (GetBreakdownTasksByDto, GetLocationsByDto, GetInstructionSetsByDto, CreateBreakdownTask, CreateEvent, Logout)
- All operations READ process.DPP_SessionId

**Business Logic:** Login MUST execute FIRST (produces SessionId required by all operations)

### 4.2 GetBreakdownTasksByDto Operation

**Operation ID:** c52c74c2-95e3-4cba-990e-3ce4746836a2  
**Operation Name:** GetBreakdownTasksByDto_CAFM  
**Response Profile ID:** 1570c9d2-0588-410d-ad3d-bdc7d5c0ec9a  
**Response Structure:** XML - SOAP response with task list

**Extracted Fields:**
- Task list (checked by decision shape55 to determine if task exists)

**Consumers:**
- Decision shape55 checks if response is empty (task exists check)
- If task exists → Early exit (return existing task)
- If task not exists → Continue to create

**Business Logic:** Check-before-create pattern - GetBreakdownTasksByDto executes BEFORE CreateBreakdownTask

### 4.3 GetLocationsByDto Operation

**Operation ID:** 442683cb-b984-499e-b7bb-075c826905aa  
**Operation Name:** GetLocationsByDto_CAFM  
**Response Profile ID:** 3aa0f5c5-8c95-4023-aba9-9d78dd6ade96  
**Response Structure:** XML - SOAP response with location data

**Extracted Fields:**
- LocationId (extracted by shape25, written to process.DPP_LocationID)
- BuildingId (extracted by shape25, written to process.DPP_BuildingID)

**Consumers:**
- CreateBreakdownTask operation reads process.DPP_LocationID and process.DPP_BuildingID

**Business Logic:** Lookup operation - executes in branch path, converges regardless of result (best-effort)

### 4.4 GetInstructionSetsByDto Operation

**Operation ID:** dc3b6b85-848d-471d-8c76-ed3b7dea0fbd  
**Operation Name:** GetInstructionSetsByDto_CAFM  
**Response Profile ID:** 5c2f13dd-3e51-4a7c-867b-c801aaa35562  
**Response Structure:** XML - SOAP response with instruction data

**Extracted Fields:**
- InstructionId (extracted by shape28, written to process.DPP_InstructionId)

**Consumers:**
- CreateBreakdownTask operation reads process.DPP_InstructionId

**Business Logic:** Lookup operation - executes in branch path, converges regardless of result (best-effort)

### 4.5 CreateBreakdownTask Operation

**Operation ID:** 33dac20f-ea09-471c-91c3-91b39bc3b172  
**Operation Name:** CreateBreakdownTask Order EQ+  
**Response Profile ID:** dbcca2ef-55cc-48e0-9329-1e8db4ada0c8  
**Response Structure:** XML - Envelope/Body/CreateBreakdownTaskResponse/CreateBreakdownTaskResult/TaskId

**Extracted Fields:**
- TaskId (extracted by map_1bd2c72b, mapped to response cafmSRNumber)

**Consumers:**
- CreateEvent operation (if recurrence=Y)
- Response mapping (success path)

**Business Logic:** Main operation - MUST succeed, throws exception on failure (decision shape12 checks status 20*)

### 4.6 CreateEvent Operation

**Operation ID:** 52166afd-a020-4de9-b49e-55400f1c0a7a  
**Operation Name:** CreateEvent/Link task CAFM  
**Response Profile ID:** 449782a0-4b04-4a7a-aa5c-aa9265fd2614  
**Response Structure:** XML - SOAP response

**Extracted Fields:**
- None (operation result not extracted)

**Consumers:**
- None (final operation in conditional path)

**Business Logic:** Conditional operation - executes ONLY if recurrence=Y (decision shape31)

---

## 5. MAP ANALYSIS (Step 1d)

**🛑 VALIDATION CHECKPOINT:** This section is COMPLETE and MANDATORY before Phase 2.

### 5.1 SOAP Request Maps Inventory

| Map ID | Map Name | From Profile | To Profile | Operation |
|---|---|---|---|---|
| 390614fd-ae1d-496d-8a79-f320c8663049 | CreateBreakdownTask EQ+_to_CAFM_Create | af096014-313f-4565-9091-2bdd56eb46df | 362c3ec8-053c-4694-8a26-cdb931e6a411 | CreateBreakdownTask |

### 5.2 Map: CreateBreakdownTask (390614fd-ae1d-496d-8a79-f320c8663049)

**From Profile:** af096014-313f-4565-9091-2bdd56eb46df (EQ+_CAFM_Create_Request)  
**To Profile:** 362c3ec8-053c-4694-8a26-cdb931e6a411 (CreateBreakdownTask Request)  
**Type:** SOAP Request Map

**Element Names (CRITICAL):**
- Operation Element: CreateBreakdownTask
- DTO Element: breakdownTaskDto
- **RULE:** Use "breakdownTaskDto" in SOAP envelope, NOT generic "dto"

**Namespace Prefixes (CRITICAL):**
- Based on CAFM FSI SOAP API structure
- Namespace: http://www.fsi.co.uk/services/evolution/04/09
- Prefix: ns (for FSI operations)

**Field Mappings:**

| Source Field | Source Type | Target Field (SOAP) | Profile Field Name | Discrepancy? |
|---|---|---|---|---|
| reporterName | profile | ReporterName | ReporterName | ✅ Match |
| reporterEmail | profile | BDET_EMAIL | BDET_EMAIL | ✅ Match |
| reporterPhoneNumber | profile | Phone | Phone | ✅ Match |
| serviceRequestNumber | profile | CallId | CallId | ✅ Match |
| DPP_SessionId | function (process property) | sessionId | sessionId | ✅ Match |
| DPP_CategoryId | function (process property) | CategoryId | CategoryId | ✅ Match |
| DPP_DisciplineId | function (process property) | DisciplineId | DisciplineId | ✅ Match |
| DPP_PriorityId | function (process property) | PriorityId | PriorityId | ✅ Match |
| DPP_BuildingID | function (process property) | BuildingId | BuildingId | ✅ Match |
| DPP_LocationID | function (process property) | LocationId | LocationId | ✅ Match |
| DPP_InstructionId | function (process property) | InstructionId | InstructionId | ✅ Match |
| description | profile | LongDescription | LongDescription | ✅ Match |
| scheduledDate + scheduledTimeStart | function (scripting) | ScheduledDateUtc | ScheduledDateUtc | ✅ Match |
| raisedDateUtc | function (scripting) | RaisedDateUtc | RaisedDateUtc | ✅ Match |
| ContractId | function (defined property) | ContractId | ContractId | ✅ Match |
| BDET_CALLER_SOURCE_ID | function (defined property) | BDET_CALLER_SOURCE_ID | BDET_CALLER_SOURCE_ID | ✅ Match |

**Scripting Functions:**

| Function | Input | Output | Logic |
|---|---|---|---|
| Function 11 | scheduledDate, scheduledTimeStart | ScheduledDateUtc | Combine date+time, format to ISO with .0208713Z suffix |
| Function 13 | raisedDateUtc | RaisedDateUtc | Format to ISO with .0208713Z suffix |

**Date Formatting Logic:**
```javascript
// Function 11: Combine scheduledDate + scheduledTimeStart
fullDateTime = scheduledDate + "T" + scheduledTimeStart + "Z";
var date = new Date(fullDateTime);
var formattedDate = date.toISOString();
var ScheduledDateUtc = formattedDate.replace(/(\.\d{3})Z$/, ".0208713Z");

// Function 13: Format raisedDateUtc
date = new Date(raisedDateUtc);
formattedDate = date.toISOString();
RaisedDateUtc = formattedDate.replace(/(\.\d{3})Z$/, ".0208713Z");
```

**Profile vs Map Comparison:**
- All field names match between profile and map
- No discrepancies found
- Map field names are AUTHORITATIVE for SOAP envelopes

**CRITICAL RULE:** Use map field names in SOAP envelopes (all fields match profile in this case)

---

## 6. HTTP STATUS CODES AND RETURN PATH RESPONSES (Step 1e)

**🛑 VALIDATION CHECKPOINT:** This section is COMPLETE and MANDATORY before Phase 2.

### 6.1 Return Path 1: Success Return

**Return Label:** "Return Documents"  
**Return Shape ID:** shape15  
**HTTP Status Code:** 200  
**Decision Conditions Leading to Return:**
- Decision shape12: meta.base.applicationstatuscode regex "20*" → TRUE path (success)
- OR Decision shape47: meta.base.applicationstatuscode regex "50*" → TRUE path (partial success)

**Populated Response Fields:**

| Field Name | Field Path | Source | Populated By |
|---|---|---|---|---|
| cafmSRNumber | Root/Object/workOrder/Array/ArrayElement1/Object/cafmSRNumber | operation_response | CreateBreakdownTask (TaskId) |
| sourceSRNumber | Root/Object/workOrder/Array/ArrayElement1/Object/sourceSRNumber | process_property | DPP_SourseSRNumber (from input) |
| sourceOrgId | Root/Object/workOrder/Array/ArrayElement1/Object/sourceOrgId | process_property | DPP_sourseORGId (from input) |
| status | Root/Object/workOrder/Array/ArrayElement1/Object/status | static | "true" (from map default) |
| message | Root/Object/workOrder/Array/ArrayElement1/Object/message | static | "Work order created successfully" |

**Response JSON Example:**
```json
{
  "workOrder": [
    {
      "cafmSRNumber": "CAFM-2025-12345",
      "sourceSRNumber": "EQ-2025-001",
      "sourceOrgId": "ORG-123",
      "status": "true",
      "message": "Work order created successfully"
    }
  ]
}
```

### 6.2 Return Path 2: Duplicate Task Return

**Return Label:** "Return Documents"  
**Return Shape ID:** shape56  
**HTTP Status Code:** 200  
**Decision Conditions Leading to Return:**
- GetBreakdownTasksByDto returns existing task
- Decision shape55: Task list not empty → FALSE path (task exists)

**Populated Response Fields:**

| Field Name | Field Path | Source | Populated By |
|---|---|---|---|---|
| cafmSRNumber | Root/Object/workOrder/Array/ArrayElement1/Object/cafmSRNumber | operation_response | GetBreakdownTasksByDto (existing TaskId) |
| sourceSRNumber | Root/Object/workOrder/Array/ArrayElement1/Object/sourceSRNumber | process_property | DPP_SourseSRNumber |
| sourceOrgId | Root/Object/workOrder/Array/ArrayElement1/Object/sourceOrgId | process_property | DPP_sourseORGId |
| status | Root/Object/workOrder/Array/ArrayElement1/Object/status | static | "true" |
| message | Root/Object/workOrder/Array/ArrayElement1/Object/message | process_property | DPP_Currentdata |

**Response JSON Example:**
```json
{
  "workOrder": [
    {
      "cafmSRNumber": "CAFM-2025-EXISTING",
      "sourceSRNumber": "EQ-2025-001",
      "sourceOrgId": "ORG-123",
      "status": "true",
      "message": "Task already exists in CAFM"
    }
  ]
}
```

### 6.3 Return Path 3: Error Return

**Return Label:** "Failure"  
**Return Shape ID:** shape18  
**HTTP Status Code:** 400  
**Decision Conditions Leading to Return:**
- Try/Catch error path (shape3 catch block)
- Any exception in main flow

**Populated Response Fields:**

| Field Name | Field Path | Source | Populated By |
|---|---|---|---|---|
| sourceSRNumber | Root/Object/workOrder/Array/ArrayElement1/Object/sourceSRNumber | process_property | DPP_SourseSRNumber |
| sourceOrgId | Root/Object/workOrder/Array/ArrayElement1/Object/sourceOrgId | process_property | DPP_sourseORGId |
| status | Root/Object/workOrder/Array/ArrayElement1/Object/status | static | "false" |
| message | Root/Object/workOrder/Array/ArrayElement1/Object/message | process_property | DPP_Currentdata (error message) |

**Response JSON Example:**
```json
{
  "workOrder": [
    {
      "cafmSRNumber": "",
      "sourceSRNumber": "EQ-2025-001",
      "sourceOrgId": "ORG-123",
      "status": "false",
      "message": "CAFM Log In Failed. CAFM Log In API Responded with Blank Response"
    }
  ]
}
```

### 6.4 Downstream Operations HTTP Status Codes

| Operation Name | Expected Success Codes | Error Codes | Error Handling |
|---|---|---|---|
| Login (FsiLogin) | 200 (regex 20*) | 400, 500 | Throw exception (required for all operations) |
| GetBreakdownTasksByDto | 200 | 404, 500 | Check if empty (existence check) |
| GetLocationsByDto | 200 | 404, 500 | Continue with empty (best-effort lookup) |
| GetInstructionSetsByDto | 200 | 404, 500 | Continue with empty (best-effort lookup) |
| CreateBreakdownTask | 200, 201 (regex 20*) | 400, 500 (regex 50*) | Throw exception on 50*, partial success on 20* |
| CreateEvent | 200 | 400, 500 | Log warning, continue (conditional operation) |
| Logout (FsiLogout) | 200 | 500 | Log error, continue (cleanup operation) |

---

## 7. PROCESS PROPERTIES ANALYSIS (Steps 2-3)

### 7.1 Properties WRITTEN

| Property Name | Written By Shape(s) | Source | Value |
|---|---|---|---|
| process.DPP_Process_Name | shape2 | execution | Process Name |
| process.DPP_AtomName | shape2 | execution | Atom Name |
| process.DPP_Payload | shape2 | current | Input document |
| process.DPP_ExecutionID | shape2 | execution | Execution Id |
| process.DPP_File_Name | shape2 | defined parameter + date + static | File name with timestamp |
| process.DPP_Subject | shape2 | execution + static | Email subject |
| process.DPP_HasAttachment | shape2 | defined parameter | Has_Attachment flag |
| process.To_Email | shape2 | defined parameter | Email recipient |
| process.DPP_SourseSRNumber | shape2 | profile | serviceRequestNumber |
| process.DPP_sourseORGId | shape2 | profile | sourceOrgId |
| process.DPP_SessionId | shape8 (FsiLogin subprocess) | profile | SessionId from Login response |
| process.DPP_LocationID | shape25 | profile | LocationId from GetLocationsByDto |
| process.DPP_BuildingID | shape25 | profile | BuildingId from GetLocationsByDto |
| process.DPP_InstructionId | shape28 | profile | InstructionId from GetInstructionSetsByDto |
| process.DPP_CategoryId | shape40 | profile | CategoryId (from input categoryName lookup) |
| process.DPP_DisciplineId | shape39 | profile | DisciplineId (from input subCategory lookup) |
| process.DPP_PriorityId | shape8 | profile | PriorityId (from input priority lookup) |
| process.DPP_CafmError | shape11 (FsiLogin subprocess) | static | "CAFM Log In Failed..." |
| process.DPP_ErrorMessage | shape21 | track | Catch error message |
| process.DPP_MailBody | shape14, shape22 (Email subprocess) | current | Email body HTML |
| process.DPP_Currentdata | shape48 | track | Current error/success message |

### 7.2 Properties READ

| Property Name | Read By Shape(s) | Usage |
|---|---|---|
| process.DPP_SessionId | shape5 (FsiLogout), all CAFM operations | Authentication for all CAFM API calls |
| process.DPP_LocationID | map_390614fd (CreateBreakdownTask) | BuildingId field in SOAP request |
| process.DPP_BuildingID | map_390614fd (CreateBreakdownTask) | LocationId field in SOAP request |
| process.DPP_InstructionId | map_390614fd (CreateBreakdownTask) | InstructionId field in SOAP request |
| process.DPP_CategoryId | map_390614fd (CreateBreakdownTask) | CategoryId field in SOAP request |
| process.DPP_DisciplineId | map_390614fd (CreateBreakdownTask) | DisciplineId field in SOAP request |
| process.DPP_PriorityId | map_390614fd (CreateBreakdownTask) | PriorityId field in SOAP request |
| process.DPP_SourseSRNumber | map_1bd2c72b, map_59c7be49 | Response mapping (sourceSRNumber) |
| process.DPP_sourseORGId | map_1bd2c72b, map_59c7be49 | Response mapping (sourceOrgId) |
| process.DPP_Currentdata | map_59c7be49 | Error response message |
| process.DPP_HasAttachment | shape4 (Email subprocess) | Determine email with/without attachment |
| process.To_Email | shape6, shape20 (Email subprocess) | Email recipient |
| process.DPP_MailBody | shape6, shape20 (Email subprocess) | Email body content |
| process.DPP_Subject | shape6, shape11, shape20, shape23 (Email subprocess) | Email subject |
| process.DPP_File_Name | shape6 (Email subprocess) | Attachment filename |
| process.DPP_Process_Name | shape11, shape23 (Email subprocess) | Email body (process name) |
| process.DPP_AtomName | shape11, shape23 (Email subprocess) | Email body (atom name) |
| process.DPP_ExecutionID | shape11, shape23 (Email subprocess) | Email body (execution ID) |
| process.DPP_ErrorMessage | shape11, shape23 (Email subprocess) | Email body (error details) |

---

## 8. DATA DEPENDENCY GRAPH (Step 4)

**🛑 VALIDATION CHECKPOINT:** This section is COMPLETE and MANDATORY before Step 8.

### 8.1 Dependency Chains

**Chain 1: Authentication → All Operations**
```
FsiLogin (shape5 subprocess)
  └─ WRITES: process.DPP_SessionId
  └─ CONSUMERS:
     ├─ GetBreakdownTasksByDto (shape53) - READS DPP_SessionId
     ├─ GetLocationsByDto (shape24) - READS DPP_SessionId
     ├─ GetInstructionSetsByDto (shape27) - READS DPP_SessionId
     ├─ CreateBreakdownTask (shape11) - READS DPP_SessionId
     ├─ CreateEvent (shape35) - READS DPP_SessionId
     └─ FsiLogout (shape13 subprocess) - READS DPP_SessionId
```

**Dependency Rule:** FsiLogin MUST execute BEFORE all CAFM operations

**Chain 2: Lookups → CreateBreakdownTask**
```
GetLocationsByDto (shape24)
  └─ WRITES: process.DPP_LocationID, process.DPP_BuildingID
  └─ CONSUMER: CreateBreakdownTask (shape11) - READS DPP_LocationID, DPP_BuildingID

GetInstructionSetsByDto (shape27)
  └─ WRITES: process.DPP_InstructionId
  └─ CONSUMER: CreateBreakdownTask (shape11) - READS DPP_InstructionId
```

**Dependency Rule:** Lookups execute BEFORE CreateBreakdownTask (but can be parallel to each other)

**Chain 3: CreateBreakdownTask → CreateEvent**
```
CreateBreakdownTask (shape11)
  └─ WRITES: TaskId (in response, mapped to cafmSRNumber)
  └─ CONSUMER: CreateEvent (shape35) - Uses TaskId for linking
```

**Dependency Rule:** CreateBreakdownTask MUST execute BEFORE CreateEvent

### 8.2 Independent Operations

- GetLocationsByDto and GetInstructionSetsByDto are independent (no data dependencies between them)
- GetBreakdownTasksByDto is independent of lookups (executed in separate branch path)

### 8.3 Property Summary

**Critical Dependencies:**
1. DPP_SessionId: Required by ALL CAFM operations → FsiLogin MUST be FIRST
2. DPP_LocationID, DPP_BuildingID, DPP_InstructionId: Required by CreateBreakdownTask → Lookups BEFORE Create
3. TaskId: Required by CreateEvent → CreateBreakdownTask BEFORE CreateEvent

---

## 9. CONTROL FLOW GRAPH (Step 5)

### 9.1 Control Flow Map

**Start → shape2 (Input_details)**
- shape2 → shape3 (Try/Catch)

**Try Block (shape3 default path) → shape4 (Branch - 6 paths)**

**Branch Path 1:** shape4 → shape5 (FsiLogin subprocess)
- shape5 return paths:
  - Success: Continue to shape6 (stop with continue=true)
  - Login_Error: shape7 (map) → shape6 (stop)

**Branch Path 2:** shape4 → shape50 (GetBreakdownTasksByDto)
- shape50 → shape51 → shape52 → shape53 → shape54 → shape55 (decision)
  - TRUE: shape57 (stop with continue=true)
  - FALSE: shape58 (branch)
    - Path 1: shape61 (map) → shape56 (return - task exists)
    - Path 2: shape59 (stop with continue=true)

**Branch Path 3:** shape4 → shape23 (GetLocationsByDto)
- shape23 → shape40 → shape43 → shape24 → shape38 → shape25 → shape30 (stop with continue=true)

**Branch Path 4:** shape4 → shape26 (GetInstructionSetsByDto)
- shape26 → shape39 → shape44 → shape27 → shape37 → shape28 → shape29 (stop with continue=true)

**Branch Path 5:** shape4 → shape31 (decision - recurrence check)
- TRUE (not Y): shape7 → shape9 → shape8 → shape11 → shape42 → shape46 → shape12 (decision)
- FALSE (is Y): shape32 (branch - 2 paths)
  - Path 1: shape7 → ... → shape11 → ... → shape12 (same as TRUE path)
  - Path 2: shape34 (CreateEvent) → shape41 → shape45 → shape35 → shape36 → shape33 (stop)

**Branch Path 6:** shape4 → shape13 (FsiLogout subprocess)

**Branch Convergence:** shape6 (stop with continue=true) - All branch paths converge here

**After Convergence:** shape6 → shape7 (map CreateBreakdownTask) → ... → shape12 (decision 20*?)
- TRUE: shape16 (map success) → shape15 (return success)
- FALSE: shape47 (decision 50*?)
  - TRUE: shape17 (map partial) → shape15 (return)
  - FALSE: shape48 → shape49 (map error) → shape15 (return)

**Catch Block (shape3 error path) → shape20 (Branch - 3 paths)**
- Path 1: shape21 → shape19 (Email subprocess)
- Path 2: shape18 (return failure)
- Path 3: shape22 (exception)

### 9.2 Connection Summary

- Total shapes: 58
- Total connections: ~70+
- Shapes with multiple outgoing connections:
  - shape3 (Try/Catch): 2 paths
  - shape4 (Branch): 6 paths
  - shape12 (Decision): 2 paths
  - shape20 (Branch): 3 paths
  - shape31 (Decision): 2 paths
  - shape32 (Branch): 2 paths
  - shape47 (Decision): 2 paths
  - shape55 (Decision): 2 paths
  - shape58 (Branch): 2 paths

---

## 10. DECISION SHAPE ANALYSIS (Step 7)

**🛑 VALIDATION CHECKPOINT:** This section is COMPLETE and MANDATORY before Step 8.

### 10.1 Self-Check Results

✅ Decision data sources identified: YES  
✅ Decision types classified: YES  
✅ Execution order verified: YES  
✅ All decision paths traced: YES  
✅ Decision patterns identified: YES  
✅ Paths traced to termination: YES

### 10.2 Decision Inventory

#### Decision 1: shape4 (FsiLogin subprocess) - Status Code Check

**Location:** FsiLogin subprocess (shape4 in subprocess_3d9db79d)  
**Comparison:** regex  
**Value1:** meta.base.applicationstatuscode (TRACK_PROPERTY)  
**Value2:** "20*" (static)  
**Data Source:** TRACK_PROPERTY (response from Login operation)  
**Decision Type:** POST-OPERATION (checks Login response)  
**Actual Execution Order:** Login operation → Check response status → Decision routes

**TRUE Path:** shape8 → Extract SessionId → shape9 (stop with continue=true) [SUCCESS RETURN]  
**FALSE Path:** shape11 → Set error property → shape6 (map) → shape7 (return "Login_Error") [ERROR RETURN]

**Pattern:** Error Check (Success vs Failure)  
**Convergence:** No (paths terminate differently)  
**Early Exit:** FALSE path returns "Login_Error" label

#### Decision 2: shape55 - Task Existence Check

**Location:** Main process (branch path 2)  
**Comparison:** (implicit - checking if response is empty)  
**Data Source:** RESPONSE (from GetBreakdownTasksByDto operation)  
**Decision Type:** POST-OPERATION (checks operation response)  
**Actual Execution Order:** GetBreakdownTasksByDto → Check if tasks found → Decision routes

**TRUE Path:** shape57 (stop with continue=true) - No tasks found, continue to create  
**FALSE Path:** shape58 (branch) → shape61 (map) → shape56 (return existing task) [EARLY EXIT]

**Pattern:** Existence Check (Return if Found, Create if Not Found)  
**Convergence:** No (FALSE path returns early)  
**Early Exit:** FALSE path returns existing task

#### Decision 3: shape31 - Recurrence Check

**Location:** Main process (branch path 5)  
**Comparison:** notequals  
**Value1:** ticketDetails.recurrence (INPUT - from request profile)  
**Value2:** "Y" (static)  
**Data Source:** INPUT (checks input data)  
**Decision Type:** PRE-FILTER (checks input, but business logic requires CreateBreakdownTask FIRST)  
**Actual Execution Order:** CreateBreakdownTask → Check recurrence → Decision routes (CreateEvent or skip)

**TRUE Path (recurrence != "Y"):** shape7 → CreateBreakdownTask → shape12 (decision) → Return  
**FALSE Path (recurrence == "Y"):** shape32 (branch)
  - Path 1: shape7 → CreateBreakdownTask → shape12 (decision) → Return
  - Path 2: shape34 → CreateEvent → shape33 (stop)

**Pattern:** Conditional Logic (Optional Processing - CreateEvent only if recurrence=Y)  
**Convergence:** Paths rejoin at shape12 (after CreateBreakdownTask)  
**Early Exit:** No (both paths continue to CreateBreakdownTask)

**CRITICAL NOTE:** Although decision checks INPUT data (recurrence field), business logic requires CreateBreakdownTask to execute FIRST, then CreateEvent executes conditionally based on recurrence flag.

#### Decision 4: shape12 - CreateBreakdownTask Status Check

**Location:** Main process (after CreateBreakdownTask)  
**Comparison:** regex  
**Value1:** meta.base.applicationstatuscode (TRACK_PROPERTY)  
**Value2:** "20*" (static)  
**Data Source:** TRACK_PROPERTY (response from CreateBreakdownTask operation)  
**Decision Type:** POST-OPERATION (checks CreateBreakdownTask response)  
**Actual Execution Order:** CreateBreakdownTask → Check response status → Decision routes

**TRUE Path (20*):** shape16 (map success) → shape15 (return success) [SUCCESS]  
**FALSE Path (not 20*):** shape47 (decision - check 50*)

**Pattern:** Error Check (Success vs Partial/Failure)  
**Convergence:** No (paths terminate at return)  
**Early Exit:** Both paths return (success or error)

#### Decision 5: shape47 - Partial Success Check

**Location:** Main process (after CreateBreakdownTask failure)  
**Comparison:** regex  
**Value1:** meta.base.applicationstatuscode (TRACK_PROPERTY)  
**Value2:** "50*" (static)  
**Data Source:** TRACK_PROPERTY (response from CreateBreakdownTask operation)  
**Decision Type:** POST-OPERATION (checks CreateBreakdownTask response)  
**Actual Execution Order:** CreateBreakdownTask → Check status 20* (FALSE) → Check status 50* → Decision routes

**TRUE Path (50*):** shape17 (map partial) → shape15 (return partial success)  
**FALSE Path (not 50*):** shape48 → shape49 (map error) → shape15 (return error)

**Pattern:** Error Check (Partial Success vs Complete Failure)  
**Convergence:** No (paths terminate at return)  
**Early Exit:** Both paths return

### 10.3 Decision Patterns Summary

| Decision | Pattern Type | Data Source | Execution Timing |
|---|---|---|---|
| shape4 (FsiLogin) | Error Check | TRACK_PROPERTY | POST-OPERATION (after Login) |
| shape55 | Existence Check | RESPONSE | POST-OPERATION (after GetBreakdownTasksByDto) |
| shape31 | Conditional Logic | INPUT | PRE-FILTER (but CreateBreakdownTask executes first) |
| shape12 | Error Check | TRACK_PROPERTY | POST-OPERATION (after CreateBreakdownTask) |
| shape47 | Error Check | TRACK_PROPERTY | POST-OPERATION (after CreateBreakdownTask) |

---

## 11. BRANCH SHAPE ANALYSIS (Step 8)

**🛑 VALIDATION CHECKPOINT:** This section is COMPLETE and MANDATORY before Step 9.

### 11.1 Self-Check Results

✅ Classification completed: YES  
✅ Assumption check: NO (analyzed dependencies)  
✅ Properties extracted: YES  
✅ Dependency graph built: YES  
✅ Topological sort applied: YES (for sequential paths)

### 11.2 Branch Shape: shape4 (Main Branch - 6 Paths)

**Shape ID:** shape4  
**Number of Paths:** 6  
**Location:** After Try block start (shape3)

**API Call Detection:** ✅ YES - All paths contain SOAP API calls (connectoraction shapes)  
**Classification:** SEQUENTIAL (API calls present - ALL API calls are sequential)

#### Path Properties Analysis

**Path 1 (FsiLogin):**
- READS: [] (uses defined parameters for username/password)
- WRITES: [process.DPP_SessionId]
- Contains API: ✅ YES (Login SOAP call)

**Path 2 (GetBreakdownTasksByDto):**
- READS: [process.DPP_SessionId]
- WRITES: [] (response checked by decision, not extracted to property)
- Contains API: ✅ YES (GetBreakdownTasksByDto SOAP call)

**Path 3 (GetLocationsByDto):**
- READS: [process.DPP_SessionId]
- WRITES: [process.DPP_LocationID, process.DPP_BuildingID]
- Contains API: ✅ YES (GetLocationsByDto SOAP call)

**Path 4 (GetInstructionSetsByDto):**
- READS: [process.DPP_SessionId]
- WRITES: [process.DPP_InstructionId]
- Contains API: ✅ YES (GetInstructionSetsByDto SOAP call)

**Path 5 (Recurrence Decision → CreateBreakdownTask):**
- READS: [process.DPP_SessionId, process.DPP_LocationID, process.DPP_BuildingID, process.DPP_InstructionId, process.DPP_CategoryId, process.DPP_DisciplineId, process.DPP_PriorityId]
- WRITES: [TaskId in response]
- Contains API: ✅ YES (CreateBreakdownTask SOAP call, optionally CreateEvent SOAP call)

**Path 6 (FsiLogout):**
- READS: [process.DPP_SessionId]
- WRITES: []
- Contains API: ✅ YES (Logout SOAP call)

#### Dependency Graph

```
Path 1 (FsiLogin) MUST execute FIRST
  └─ Produces: DPP_SessionId
  └─ Required by: Paths 2, 3, 4, 5, 6

Paths 2, 3, 4 can execute in parallel (after Path 1)
  └─ Path 2: Independent (existence check)
  └─ Path 3: Produces DPP_LocationID, DPP_BuildingID
  └─ Path 4: Produces DPP_InstructionId
  └─ All consumed by Path 5

Path 5 depends on Paths 1, 3, 4
  └─ READS: DPP_SessionId (from Path 1)
  └─ READS: DPP_LocationID, DPP_BuildingID (from Path 3)
  └─ READS: DPP_InstructionId (from Path 4)

Path 6 (FsiLogout) can execute after Path 5 (or anytime after Path 1)
```

#### Classification

**Classification:** SEQUENTIAL  
**Reason:** All paths contain API calls (SOAP operations) - API calls are ALWAYS sequential

**🚨 CRITICAL RULE:** Even though Paths 3 and 4 have no data dependencies between them, they execute sequentially because they contain API calls. There is NO concept of parallel API calls in Azure Functions migration.

#### Topological Sort Order (Sequential Execution)

Based on data dependencies AND API call sequential rule:

```
1. Path 1 (FsiLogin) - FIRST (produces DPP_SessionId)
2. Path 2 (GetBreakdownTasksByDto) - After Path 1 (needs DPP_SessionId)
3. Path 3 (GetLocationsByDto) - After Path 1 (needs DPP_SessionId)
4. Path 4 (GetInstructionSetsByDto) - After Path 1 (needs DPP_SessionId)
5. Path 5 (CreateBreakdownTask + optional CreateEvent) - After Paths 1, 3, 4 (needs all lookup results)
6. Path 6 (FsiLogout) - After Path 5 (cleanup)
```

**Note:** Paths 2, 3, 4 could technically execute in parallel (no dependencies between them), but since they contain API calls, they execute sequentially in the order above.

#### Path Termination

- Path 1: shape6 (stop with continue=true) - Converges
- Path 2: shape57 or shape59 (stop with continue=true) - Converges, OR shape56 (return early exit)
- Path 3: shape30 (stop with continue=true) - Converges
- Path 4: shape29 (stop with continue=true) - Converges
- Path 5: shape12 → shape15 (return) - Terminates at return
- Path 6: shape13 subprocess returns, continues to convergence

#### Convergence Points

**Primary Convergence:** shape6 (stop with continue=true)
- All branch paths (1, 3, 4, 6) converge here
- Path 2 converges here OR returns early (if task exists)
- Path 5 does NOT converge (continues to CreateBreakdownTask after branch)

**Execution Continues From:** shape6 → Proceeds to CreateBreakdownTask logic (shape7)

### 11.3 Branch Shape: shape32 (Recurrence Branch - 2 Paths)

**Shape ID:** shape32  
**Number of Paths:** 2  
**Location:** Inside decision shape31 FALSE path (recurrence == "Y")

**Path 1:** shape7 → CreateBreakdownTask → shape12 (decision) → Return  
**Path 2:** shape34 → CreateEvent → shape33 (stop)

**Classification:** SEQUENTIAL (both paths contain API calls)  
**Execution Order:**
1. Path 1 (CreateBreakdownTask) - FIRST (creates task)
2. Path 2 (CreateEvent) - SECOND (links event to created task)

**Convergence:** Paths do NOT converge (Path 1 returns, Path 2 stops)

### 11.4 Branch Shape: shape20 (Error Branch - 3 Paths)

**Shape ID:** shape20  
**Number of Paths:** 3  
**Location:** Catch block (shape3 error path)

**Path 1:** shape21 (set error message) → shape19 (Email subprocess)  
**Path 2:** shape18 (return failure)  
**Path 3:** shape22 (exception)

**Classification:** PARALLEL (error handling paths, no API calls in paths themselves)  
**Execution:** All paths execute (email + return + exception)

### 11.5 Branch Shape: shape58 (Task Exists Branch - 2 Paths)

**Shape ID:** shape58  
**Number of Paths:** 2  
**Location:** Inside decision shape55 FALSE path (task exists)

**Path 1:** shape61 (map) → shape56 (return existing task) [EARLY EXIT]  
**Path 2:** shape59 (stop with continue=true) - Converges

**Classification:** CONDITIONAL (based on decision shape55 result)  
**Execution:** Only one path executes (either return existing OR continue)

---

## 12. EXECUTION ORDER (Step 9)

**🛑 VALIDATION CHECKPOINT:** This section is COMPLETE and MANDATORY before Step 10.

### 12.1 Self-Check Results

✅ Business logic verified FIRST: YES  
✅ Operation analysis complete: YES  
✅ Business logic execution order identified: YES  
✅ Data dependencies checked FIRST: YES  
✅ Operation response analysis used: YES (references Section 4)  
✅ Decision analysis used: YES (references Section 10)  
✅ Dependency graph used: YES (references Section 8)  
✅ Branch analysis used: YES (references Section 11)  
✅ Property dependency verification: YES  
✅ Topological sort applied: YES

### 12.2 Business Logic Flow

**Operation 1: FsiLogin (Authenticate)**
- **Purpose:** Establishes session with CAFM system
- **Produces:** SessionId (process.DPP_SessionId)
- **Dependent Operations:** ALL downstream CAFM operations (GetBreakdownTasksByDto, GetLocationsByDto, GetInstructionSetsByDto, CreateBreakdownTask, CreateEvent, FsiLogout)
- **Business Flow:** MUST execute FIRST - all operations require SessionId

**Operation 2: GetBreakdownTasksByDto (Check Existence)**
- **Purpose:** Check if work order already exists in CAFM
- **Produces:** Task list (checked by decision, not stored in property)
- **Dependent Operations:** None (result determines early exit or continue)
- **Business Flow:** Executes after FsiLogin - if task exists, skip creation and return early

**Operation 3: GetLocationsByDto (Lookup Location/Building)**
- **Purpose:** Lookup LocationId and BuildingId based on propertyName and unitCode
- **Produces:** DPP_LocationID, DPP_BuildingID
- **Dependent Operations:** CreateBreakdownTask (consumes LocationId and BuildingId)
- **Business Flow:** Best-effort lookup - executes before CreateBreakdownTask, continues with empty if fails

**Operation 4: GetInstructionSetsByDto (Lookup Instructions)**
- **Purpose:** Lookup InstructionId based on categoryName and subCategory
- **Produces:** DPP_InstructionId
- **Dependent Operations:** CreateBreakdownTask (consumes InstructionId)
- **Business Flow:** Best-effort lookup - executes before CreateBreakdownTask, continues with empty if fails

**Operation 5: CreateBreakdownTask (Create Work Order)**
- **Purpose:** Create breakdown task/work order in CAFM
- **Produces:** TaskId (mapped to cafmSRNumber in response)
- **Dependent Operations:** CreateEvent (if recurrence=Y)
- **Business Flow:** Main operation - MUST succeed, uses lookup results (LocationId, BuildingId, InstructionId may be empty)

**Operation 6: CreateEvent (Link Recurring Event)**
- **Purpose:** Link recurring event to created task
- **Produces:** None
- **Dependent Operations:** None
- **Business Flow:** Conditional operation - executes ONLY if recurrence=Y, logs warning if fails (task already created)

**Operation 7: FsiLogout (Cleanup)**
- **Purpose:** End CAFM session
- **Produces:** None
- **Dependent Operations:** None
- **Business Flow:** Cleanup operation - executes after all operations, logs error if fails (non-critical)

### 12.3 Dependency Verification

**Reference:** Section 8 (Data Dependency Graph)

**Dependency Chain 1: Authentication**
```
FsiLogin (shape5) WRITES process.DPP_SessionId
  └─ GetBreakdownTasksByDto (shape53) READS process.DPP_SessionId
  └─ GetLocationsByDto (shape24) READS process.DPP_SessionId
  └─ GetInstructionSetsByDto (shape27) READS process.DPP_SessionId
  └─ CreateBreakdownTask (shape11) READS process.DPP_SessionId
  └─ CreateEvent (shape35) READS process.DPP_SessionId
  └─ FsiLogout (shape13) READS process.DPP_SessionId
```

**Proof:** FsiLogin MUST execute BEFORE all CAFM operations

**Dependency Chain 2: Lookups → CreateBreakdownTask**
```
GetLocationsByDto (shape24) WRITES process.DPP_LocationID, process.DPP_BuildingID
  └─ CreateBreakdownTask (shape11) READS process.DPP_LocationID, process.DPP_BuildingID

GetInstructionSetsByDto (shape27) WRITES process.DPP_InstructionId
  └─ CreateBreakdownTask (shape11) READS process.DPP_InstructionId
```

**Proof:** Lookups MUST execute BEFORE CreateBreakdownTask

**Dependency Chain 3: CreateBreakdownTask → CreateEvent**
```
CreateBreakdownTask (shape11) produces TaskId
  └─ CreateEvent (shape35) uses TaskId for linking
```

**Proof:** CreateBreakdownTask MUST execute BEFORE CreateEvent

### 12.4 Branch Execution Order

**Reference:** Section 11 (Branch Analysis)

Branch shape4 has 6 paths with sequential execution (API calls present):

**Execution Order:**
1. Path 1 (FsiLogin) - FIRST (produces DPP_SessionId)
2. Path 2 (GetBreakdownTasksByDto) - After Path 1 (needs DPP_SessionId, checks existence)
3. Path 3 (GetLocationsByDto) - After Path 1 (needs DPP_SessionId, lookup)
4. Path 4 (GetInstructionSetsByDto) - After Path 1 (needs DPP_SessionId, lookup)
5. Path 5 (CreateBreakdownTask + CreateEvent) - After Paths 1, 3, 4 (needs lookup results)
6. Path 6 (FsiLogout) - After Path 5 (cleanup)

**Topological Sort Applied:** YES (based on data dependencies and API call sequential rule)

### 12.5 Decision Path Tracing

**Decision shape55 (Task Existence):**
- TRUE path: Continue to create (shape57 → convergence)
- FALSE path: Return existing task (shape58 → shape61 → shape56 return) [EARLY EXIT]
- Convergence: Only TRUE path converges

**Decision shape31 (Recurrence):**
- TRUE path (not Y): CreateBreakdownTask only (shape7 → ... → shape12 → return)
- FALSE path (is Y): CreateBreakdownTask + CreateEvent (shape32 branch)
- Convergence: Both paths execute CreateBreakdownTask

**Decision shape12 (CreateBreakdownTask Status):**
- TRUE path (20*): Success (shape16 → shape15 return)
- FALSE path (not 20*): Check 50* (shape47 decision)
- Termination: Both paths return

### 12.6 Complete Execution Order List

**Based on dependency graph, branch analysis, and decision paths:**

```
1. START (shape1)
2. Extract Input Details (shape2) - WRITES: DPP_Process_Name, DPP_AtomName, DPP_Payload, DPP_ExecutionID, DPP_File_Name, DPP_Subject, DPP_HasAttachment, To_Email, DPP_SourseSRNumber, DPP_sourseORGId
3. TRY Block Start (shape3)
4. BRANCH Start (shape4) - 6 paths, SEQUENTIAL execution
5. Path 1: FsiLogin (shape5 subprocess) - AUTHENTICATION
   - WRITES: DPP_SessionId
   - Decision shape4 (in subprocess): Check status 20*
     - TRUE: Extract SessionId → Continue
     - FALSE: Set error → Return "Login_Error"
6. Path 2: GetBreakdownTasksByDto (shape50-53) - EXISTENCE CHECK
   - READS: DPP_SessionId
   - Decision shape55: Check if tasks found
     - TRUE: Continue to create
     - FALSE: Return existing task [EARLY EXIT]
7. Path 3: GetLocationsByDto (shape23-25) - BEST-EFFORT LOOKUP
   - READS: DPP_SessionId
   - WRITES: DPP_LocationID, DPP_BuildingID
   - Converges at shape6 (no decision checks)
8. Path 4: GetInstructionSetsByDto (shape26-28) - BEST-EFFORT LOOKUP
   - READS: DPP_SessionId
   - WRITES: DPP_InstructionId
   - Converges at shape6 (no decision checks)
9. CONVERGENCE (shape6 - stop with continue=true)
10. Path 5: Decision shape31 (Recurrence check)
    - TRUE (not Y): CreateBreakdownTask only
    - FALSE (is Y): Branch shape32
      - Path 1: CreateBreakdownTask
      - Path 2: CreateEvent (conditional)
11. CreateBreakdownTask (shape11) - MAIN OPERATION
    - READS: DPP_SessionId, DPP_LocationID, DPP_BuildingID, DPP_InstructionId, DPP_CategoryId, DPP_DisciplineId, DPP_PriorityId
    - WRITES: TaskId (in response)
    - Decision shape12: Check status 20*
      - TRUE: Success → Return
      - FALSE: Decision shape47: Check status 50*
        - TRUE: Partial success → Return
        - FALSE: Error → Return
12. CreateEvent (shape35) - CONDITIONAL (only if recurrence=Y)
    - READS: DPP_SessionId, TaskId
    - Executes after CreateBreakdownTask
13. Path 6: FsiLogout (shape13 subprocess) - CLEANUP
    - READS: DPP_SessionId
    - Executes after all operations
14. Return Documents (shape15) - SUCCESS

CATCH Block (shape3 error path):
15. BRANCH (shape20) - 3 error paths
    - Path 1: Set error message (shape21) → Email subprocess (shape19)
    - Path 2: Return failure (shape18)
    - Path 3: Exception (shape22)
```

---

## 13. SEQUENCE DIAGRAM (Step 10)

**🛑 VALIDATION CHECKPOINT:** This section is COMPLETE and MANDATORY. Code generation uses ONLY this section.

**Purpose:** This sequence diagram is the PRIMARY BLUEPRINT for code generation. It contains ALL technical details including operation classification and error handling for EACH operation.

**📋 NOTE:** Detailed request/response JSON examples are documented in Section 17 (Request/Response JSON Examples).

**Based on:**
- Dependency graph in Step 4 (Section 8)
- Decision analysis in Step 7 (Section 10)
- Control flow graph in Step 5 (Section 9)
- Branch analysis in Step 8 (Section 11)
- Execution order in Step 9 (Section 12)

### 13.1 Operation Classification Table (MANDATORY)

**Algorithm Applied:** BOOMI_EXTRACTION_RULES.mdc STEP 10.1

| Operation | Shape(s) | Decision After? | Branch Convergence? | Operation Type | Classification | Error Handling | Reason | Boomi Reference |
|---|---|---|---|---|---|---|---|---|
| FsiLogin | shape5 (subprocess) | Yes (shape4 in subprocess checks 20*) | No | Authentication | AUTHENTICATION | Throw exception | Required for all operations | Subprocess FsiLogin with decision shape4 checking status "20*" |
| GetBreakdownTasksByDto | shape50-53 | Yes (shape55 checks empty) | No | Existence check | MAIN OPERATION | Check if exists | Existence check pattern | Decision shape55 with early exit on FALSE |
| GetLocationsByDto | shape23-25 | No | Yes (shape6) | Lookup | BEST-EFFORT LOOKUP | Log warning, set empty, continue | Branch path 3 converges at shape6 (no decision checks) | Branch path 3 (shape23-24-25) converges at shape6 |
| GetInstructionSetsByDto | shape26-28 | No | Yes (shape6) | Lookup | BEST-EFFORT LOOKUP | Log warning, set empty, continue | Branch path 4 converges at shape6 (no decision checks) | Branch path 4 (shape26-27-28) converges at shape6 |
| CreateBreakdownTask | shape11 | Yes (shape12 checks 20*) | No | Main operation | MAIN OPERATION | Throw exception | Primary business operation | Main operation with decision shape12 checking status "20*" |
| CreateEvent | shape34-35 | No | No | Conditional | CONDITIONAL | Log warning, continue | Optional, task already created | Conditional execution (decision shape31, FALSE path) |
| FsiLogout | shape13 (subprocess) | No | No | Cleanup | CLEANUP | Log error, continue | Cleanup only, non-critical | Subprocess FsiLogout |

### 13.2 Enhanced Sequence Diagram (COMPLETE TECHNICAL SPECIFICATION)

```
START (shape1)
 |
 ├─→ Extract Input Details (shape2)
 | └─→ WRITES: [process.DPP_Process_Name, process.DPP_AtomName, process.DPP_Payload, process.DPP_ExecutionID, process.DPP_File_Name, process.DPP_Subject, process.DPP_HasAttachment, process.To_Email, process.DPP_SourseSRNumber, process.DPP_sourseORGId]
 |
 ├─→ TRY Block Start (shape3)
 |
 ├─→ BRANCH (shape4) - 6 paths - SEQUENTIAL EXECUTION (API calls present)
 | |
 | ├─→ Path 1: FsiLogin (subprocess shape5) (Downstream) - (AUTHENTICATION)
 | | └─→ SUBPROCESS INTERNAL FLOW:
 | |   ├─→ START (shape1)
 | |   ├─→ Set URL and SOAPAction (shape3)
 | |   ├─→ Build SOAP Envelope (shape5)
 | |   ├─→ Login Operation (shape2) (Downstream)
 | |   | └─→ SOAP: Authenticate (username, password from KeyVault)
 | |   | └─→ HTTP: [Expected: 200, Error: 401/500]
 | |   ├─→ Decision (shape4): Status Code regex "20*"?
 | |   | ├─→ IF TRUE (Success) → Extract SessionId (shape8)
 | |   | | └─→ WRITES: [process.DPP_SessionId]
 | |   | | └─→ Stop (shape9, continue=true) [SUCCESS RETURN]
 | |   | └─→ IF FALSE (Failure) → Set Error (shape11)
 | |   |   └─→ WRITES: [process.DPP_CafmError = "CAFM Log In Failed..."]
 | |   |   └─→ Map Error (shape6) → Return Documents (shape7) [ERROR RETURN: "Login_Error"]
 | |   └─→ END SUBPROCESS
 | | └─→ READS: []
 | | └─→ WRITES: [process.DPP_SessionId]
 | | └─→ HTTP: [Expected: 200, Error: 401/500]
 | | └─→ ERROR HANDLING: If fails → Throw exception (required for all operations)
 | | └─→ RESULT: sessionId (throws exception on failure)
 | | └─→ BOOMI: Subprocess FsiLogin with decision shape4 checking status "20*"
 | |
 | ├─→ Path 2: GetBreakdownTasksByDto (shape50-53) (Downstream) - (MAIN OPERATION - Existence Check)
 | | └─→ READS: [process.DPP_SessionId, process.DPP_SourseSRNumber]
 | | └─→ WRITES: [] (response checked by decision, not stored)
 | | └─→ HTTP: [Expected: 200, Error: 404/500]
 | | └─→ ERROR HANDLING: Check if task exists
 | | | └─→ If task exists (response not empty) → Return existing task [EARLY EXIT]
 | | | └─→ If task not exists (response empty) → Continue to create
 | | └─→ RESULT: Task existence determined (early exit or continue)
 | | └─→ BOOMI: Branch path 2 (shape50-53) with decision shape55 checking empty response
 | | └─→ Decision (shape55): Task list empty?
 | |   ├─→ IF TRUE (Empty - task not exists) → Continue (shape57) → Convergence
 | |   └─→ IF FALSE (Not empty - task exists) → Branch (shape58)
 | |     ├─→ Path 1: Map existing task (shape61) → Return Documents (shape56) [EARLY EXIT]
 | |     | └─→ Response: { cafmSRNumber: "existing-id", status: "true", message: "Task already exists" }
 | |     └─→ Path 2: Continue (shape59) → Convergence
 | |
 | ├─→ Path 3: GetLocationsByDto (shape23-25) (Downstream) - (BEST-EFFORT LOOKUP)
 | | └─→ READS: [process.DPP_SessionId, process.DPP_Payload (propertyName, unitCode)]
 | | └─→ WRITES: [process.DPP_LocationID, process.DPP_BuildingID]
 | | └─→ HTTP: [Expected: 200, Error: 404/500]
 | | └─→ ERROR HANDLING: If fails → Log warning, set empty values, CONTINUE
 | | └─→ RESULT: locationId, buildingId (populated or empty)
 | | └─→ BOOMI: Branch path 3 (shape23-24-25) converges at shape6 (no decision checks)
 | |
 | ├─→ Path 4: GetInstructionSetsByDto (shape26-28) (Downstream) - (BEST-EFFORT LOOKUP)
 | | └─→ READS: [process.DPP_SessionId, process.DPP_Payload (categoryName, subCategory)]
 | | └─→ WRITES: [process.DPP_InstructionId]
 | | └─→ HTTP: [Expected: 200, Error: 404/500]
 | | └─→ ERROR HANDLING: If fails → Log warning, set empty value, CONTINUE
 | | └─→ RESULT: instructionId (populated or empty)
 | | └─→ BOOMI: Branch path 4 (shape26-27-28) converges at shape6 (no decision checks)
 | |
 | └─→ Path 6: FsiLogout (subprocess shape13) (Downstream) - (CLEANUP)
 |   └─→ READS: [process.DPP_SessionId]
 |   └─→ WRITES: []
 |   └─→ HTTP: [Expected: 200, Error: 500]
 |   └─→ ERROR HANDLING: If fails → Log error, CONTINUE (cleanup only, non-critical)
 |   └─→ RESULT: (no variables set)
 |   └─→ BOOMI: Subprocess FsiLogout
 |
 ├─→ CONVERGENCE (shape6 - stop with continue=true)
 |
 ├─→ Path 5: Decision (shape31): ticketDetails.recurrence notequals "Y"?
 | ├─→ IF TRUE (Not recurring) → CreateBreakdownTask only
 | | └─→ Map CreateBreakdownTask Request (shape7)
 | | └─→ Build SOAP Request (shape8)
 | | └─→ CreateBreakdownTask (shape11) (Downstream) - (MAIN OPERATION)
 | |   └─→ READS: [process.DPP_SessionId, DPP_LocationID, DPP_BuildingID, DPP_InstructionId, DPP_CategoryId, DPP_DisciplineId, DPP_PriorityId, input fields]
 | |   └─→ WRITES: [TaskId in response]
 | |   └─→ HTTP: [Expected: 200, 201 (regex 20*), Error: 400, 500 (regex 50*)]
 | |   └─→ ERROR HANDLING: If fails → Throw exception (main operation must succeed)
 | |   └─→ RESULT: breakdownTaskId (throws exception on failure)
 | |   └─→ BOOMI: Main operation (shape11) with decision shape12 checking status "20*"
 | |   └─→ Decision (shape12): Status Code regex "20*"?
 | |     ├─→ IF TRUE (Success) → Map Success Response (shape16) → Return Documents (shape15) [HTTP: 200] [SUCCESS]
 | |     └─→ IF FALSE (Not 20*) → Decision (shape47): Status Code regex "50*"?
 | |       ├─→ IF TRUE (Partial Success) → Map Partial Response (shape17) → Return Documents (shape15) [HTTP: 200] [PARTIAL]
 | |       └─→ IF FALSE (Error) → Set Error (shape48) → Map Error Response (shape49) → Return Documents (shape15) [HTTP: 400] [ERROR]
 | |
 | └─→ IF FALSE (Recurring - recurrence == "Y") → BRANCH (shape32) - 2 paths
 |   ├─→ Path 1: CreateBreakdownTask (same as TRUE path above)
 |   | └─→ Map CreateBreakdownTask Request (shape7)
 |   | └─→ Build SOAP Request (shape8)
 |   | └─→ CreateBreakdownTask (shape11) (Downstream) - (MAIN OPERATION)
 |   |   └─→ [Same as above - shape11 details]
 |   |
 |   └─→ Path 2: CreateEvent (shape34-35) (Downstream) - (CONDITIONAL)
 |     └─→ READS: [process.DPP_SessionId, TaskId from CreateBreakdownTask]
 |     └─→ WRITES: []
 |     └─→ HTTP: [Expected: 200, Error: 400/500]
 |     └─→ ERROR HANDLING: If fails → Log warning, CONTINUE (task already created)
 |     └─→ RESULT: eventId (may be empty if failed)
 |     └─→ BOOMI: Conditional execution (decision shape31, FALSE path, branch shape32 path 2)

CATCH Block (shape3 error path):
 |
 └─→ BRANCH (shape20) - 3 error paths
   ├─→ Path 1: Set Error Message (shape21) → Email Subprocess (shape19)
   | └─→ Send error notification email
   ├─→ Path 2: Return Failure (shape18) [HTTP: 400] [ERROR]
   | └─→ Response: { status: "false", message: "Error message" }
   └─→ Path 3: Exception (shape22)
     └─→ Throw exception with error details

**Note:** Detailed request/response JSON examples for all operations and return paths are documented in Section 17 (Request/Response JSON Examples).
```

**CRITICAL RULES FOLLOWED:**
1. ✅ Each operation shows READS and WRITES
2. ✅ Each operation shows HTTP status codes
3. ✅ Each operation shows CLASSIFICATION (5 types)
4. ✅ Each operation shows ERROR HANDLING (exact action if fails)
5. ✅ Each operation shows RESULT (variables set)
6. ✅ Each operation shows BOOMI REFERENCE (shape numbers)
7. ✅ Downstream operations marked (Downstream)
8. ✅ Decisions show both TRUE and FALSE paths
9. ✅ Return paths show HTTP status codes
10. ✅ Check-before-create pattern shown (GetBreakdownTasksByDto → Decision → Create)
11. ✅ Early exits marked [EARLY EXIT]
12. ✅ Conditional execution marked [Only if recurrence=Y]
13. ✅ Detailed JSON examples in Section 17

---

## 14. SUBPROCESS ANALYSIS

### 14.1 FsiLogin Subprocess (3d9db79d-15d0-4472-9f47-375ad9ab1ed2)

**Purpose:** Authenticate with CAFM system and obtain SessionId

**Internal Flow:**
```
START (shape1)
  → Set URL and SOAPAction properties (shape3)
  → Build SOAP Envelope (shape5)
  → Login Operation (shape2) - HTTP POST with SOAP
  → Decision (shape4): Check status code regex "20*"
    ├─→ TRUE: Extract SessionId (shape8) → Stop (shape9, continue=true) [SUCCESS]
    └─→ FALSE: Set Error Property (shape11) → Map Error (shape6) → Return Documents (shape7) [ERROR: "Login_Error"]
```

**Return Paths:**
1. **Success:** Stop (shape9, continue=true) - SessionId extracted to process.DPP_SessionId
2. **Login_Error:** Return Documents (shape7) - Error message in process.DPP_CafmError

**Properties Written:**
- process.DPP_SessionId (on success)
- process.DPP_CafmError (on failure)

**Properties Read:**
- Defined parameters: FSI_Username, FSI_Password, Resourcepath_Login, soapaction_login

**Main Process Mapping:**
- Success return → Continue to next branch path
- "Login_Error" return → shape7 (map) → shape6 (stop/convergence)

### 14.2 FsiLogout Subprocess (b44c26cb-ecd5-4677-a752-434fe68f2e2b)

**Purpose:** End CAFM session

**Internal Flow:**
```
START (shape1)
  → Build SOAP Envelope (shape5)
  → Set URL and SOAPAction properties (shape4)
  → Logout Operation (shape2) - HTTP POST with SOAP
  → Stop (shape3, continue=true) [SUCCESS]
```

**Return Paths:**
1. **Success:** Stop (shape3, continue=true) - No properties written

**Properties Written:** None

**Properties Read:**
- process.DPP_SessionId (from main process)
- Defined parameters: Resourcepath_logout, soapaction_logout

**Main Process Mapping:**
- Always returns success (stop with continue=true)
- Continues to convergence point

### 14.3 Office 365 Email Subprocess (a85945c5-3004-42b9-80b1-104f465cd1fb)

**Purpose:** Send email notification (error reporting)

**Internal Flow:**
```
START (shape1)
  → Try/Catch Block (shape2)
    ├─→ Try Path:
    |   ├─→ Decision (shape4): DPP_HasAttachment equals "Y"?
    |   | ├─→ TRUE: Build Email Body (shape11) → Set Body Property (shape14) → Build Payload (shape15) → Set Mail Properties (shape6) → Send Email with Attachment (shape3)
    |   | └─→ FALSE: Build Email Body (shape23) → Set Body Property (shape22) → Set Mail Properties (shape20) → Send Email without Attachment (shape7)
    |   └─→ Stop (shape5, continue=true)
    └─→ Catch Path: Exception (shape10)
```

**Return Paths:**
1. **Success:** Stop (shape5, continue=true)

**Properties Written:**
- process.DPP_MailBody (email HTML content)

**Properties Read:**
- process.DPP_HasAttachment (determines attachment path)
- process.To_Email (recipient)
- process.DPP_Subject (email subject)
- process.DPP_Process_Name (for email body)
- process.DPP_AtomName (for email body)
- process.DPP_ExecutionID (for email body)
- process.DPP_ErrorMessage (for email body)
- process.DPP_File_Name (attachment filename)
- process.DPP_Payload (attachment content)
- Defined parameters: From_Email, Environment

---

## 15. CRITICAL PATTERNS IDENTIFIED

### 15.1 Pattern 1: Session-Based Authentication

**Pattern:** Login → Store SessionId → Use in all operations → Logout

**Boomi Implementation:**
- Subprocess FsiLogin (shape5) → Extract SessionId → Store in process.DPP_SessionId
- All CAFM operations read process.DPP_SessionId
- Subprocess FsiLogout (shape13) → End session

**Azure Implementation:**
- CustomAuthenticationMiddleware with AuthenticateAtomicHandler + LogoutAtomicHandler
- RequestContext.SetSessionId() / RequestContext.GetSessionId()
- Middleware handles login/logout lifecycle
- Functions marked with [CustomAuthentication] attribute

### 15.2 Pattern 2: Check-Before-Create

**Pattern:** Check if entity exists → If exists, return existing → If not exists, create

**Boomi Implementation:**
- GetBreakdownTasksByDto (shape50-53) → Check response
- Decision shape55: If tasks found (not empty) → Return existing task (shape56) [EARLY EXIT]
- If tasks not found (empty) → Continue to create (shape57 → convergence)

**Azure Implementation:**
- GetBreakdownTasksByDto operation in Handler
- Check response: if (!string.IsNullOrEmpty(existingTaskId)) → Return existing
- Else → Continue to CreateBreakdownTask

### 15.3 Pattern 3: Best-Effort Lookups with Branch Convergence

**Pattern:** Execute lookups in parallel → Converge regardless of result → Use lookup results (may be empty)

**Boomi Implementation:**
- Branch shape4 paths 3 and 4 (GetLocationsByDto, GetInstructionSetsByDto)
- Both paths converge at shape6 (stop with continue=true)
- No decision shapes check lookup results
- CreateBreakdownTask uses lookup results (may be empty)

**Azure Implementation:**
- Execute lookups sequentially (API calls are sequential)
- If lookup fails → Log warning, set empty value, CONTINUE
- CreateBreakdownTask uses lookup results (locationId, buildingId, instructionId may be empty)
- CAFM system validates required fields (not System Layer)

### 15.4 Pattern 4: Conditional Operation Based on Input Flag

**Pattern:** Check input flag → If flag set, execute additional operation → If not set, skip

**Boomi Implementation:**
- Decision shape31: Check ticketDetails.recurrence notequals "Y"
- TRUE (not Y): CreateBreakdownTask only
- FALSE (is Y): Branch shape32
  - Path 1: CreateBreakdownTask
  - Path 2: CreateEvent (link recurring event)

**Azure Implementation:**
- Check recurrence flag in Handler
- Always execute CreateBreakdownTask
- If recurrence == "Y" → Execute CreateEvent
- If CreateEvent fails → Log warning, CONTINUE (task already created)

### 15.5 Pattern 5: Try/Catch with Error Email Notification

**Pattern:** Wrap main logic in try/catch → On error, send email notification + return error

**Boomi Implementation:**
- Try/Catch shape3
- Try path: Main business logic
- Catch path: Branch shape20
  - Path 1: Set error message → Email subprocess
  - Path 2: Return failure
  - Path 3: Exception

**Azure Implementation:**
- ExceptionHandlerMiddleware catches all exceptions
- On exception → Log error, return BaseResponseDTO with error details
- Email notification handled separately (not in System Layer - Process Layer responsibility)

### 15.6 Pattern 6: Date Formatting with Scripting

**Pattern:** Combine date and time fields → Format to ISO with custom suffix

**Boomi Implementation:**
- Map function 11: Combine scheduledDate + scheduledTimeStart → Format to ISO → Replace milliseconds with ".0208713Z"
- Map function 13: Format raisedDateUtc → ISO → Replace milliseconds with ".0208713Z"

**Azure Implementation:**
- DateTimeExtensions or private method in Handler
- Combine date and time: DateTime.Parse($"{scheduledDate}T{scheduledTimeStart}Z")
- Format to ISO: .ToString("yyyy-MM-ddTHH:mm:ss.0208713Z")

---

## 16. SYSTEM LAYER IDENTIFICATION

### 16.1 Third-Party Systems

**System 1: CAFM (FSI - Facilities System International)**
- **Type:** SOAP/XML API
- **Authentication:** Session-based (Login → SessionId → Logout)
- **Operations:**
  - Authenticate (Login)
  - GetBreakdownTasksByDto (Check existence)
  - GetLocationsByDto (Lookup location/building)
  - GetInstructionSetsByDto (Lookup instructions)
  - CreateBreakdownTask (Create work order)
  - CreateEvent (Link recurring event)
  - LogOut (End session)
- **System Layer Project:** sys-cafm-mgmt (or sys-fsi-mgmt)

**System 2: Email (Office 365 SMTP)**
- **Type:** SMTP/Email
- **Authentication:** SMTP AUTH (username/password)
- **Operations:**
  - Send email with/without attachment
- **System Layer Project:** sys-email-mgmt (or use existing email service)

### 16.2 System Layer Repositories

**Primary System Layer:** sys-cafm-mgmt (or sys-fsi-mgmt)
- Handles all CAFM SOAP operations
- Session-based authentication with middleware
- Exposes operations for Process Layer to orchestrate

**Secondary System Layer:** sys-email-mgmt (OPTIONAL)
- Email operations typically handled by shared email service
- May not need separate System Layer (Process Layer can call shared email service directly)

### 16.3 System Layer Scope

**In Scope (System Layer):**
- CAFM authentication (Login/Logout)
- CAFM SOAP operations (GetBreakdownTasksByDto, GetLocationsByDto, GetInstructionSetsByDto, CreateBreakdownTask, CreateEvent)
- SOAP envelope construction
- Session management via middleware
- Response deserialization

**Out of Scope (Process Layer):**
- Business orchestration (when to call which operation)
- Cross-SOR decisions (if EQ+ has X, call CAFM Y)
- Email notification logic (error handling, when to send)
- Input validation (field-level business rules)
- Response aggregation (combining multiple operation results)

---

## 17. REQUEST/RESPONSE JSON EXAMPLES

### 17.1 Process Layer Entry Point

**Request JSON Example:**
```json
{
  "workOrder": [
    {
      "reporterName": "John Doe",
      "reporterEmail": "john.doe@example.com",
      "reporterPhoneNumber": "+971-50-123-4567",
      "description": "Air conditioning not working in office 301",
      "serviceRequestNumber": "EQ-2025-001234",
      "propertyName": "Al Ghurair Tower",
      "unitCode": "UNIT-301",
      "categoryName": "HVAC",
      "subCategory": "Air Conditioning",
      "technician": "Tech-Ahmed",
      "sourceOrgId": "ORG-ALGHURAIR-001",
      "ticketDetails": {
        "status": "Open",
        "subStatus": "Pending Assignment",
        "priority": "High",
        "scheduledDate": "2025-02-25",
        "scheduledTimeStart": "11:05:41",
        "scheduledTimeEnd": "13:05:41",
        "recurrence": "N",
        "oldCAFMSRnumber": "",
        "raisedDateUtc": "2025-02-24T10:30:00Z"
      }
    }
  ]
}
```

**Response JSON Examples:**

**Success Response (HTTP 200):**
```json
{
  "workOrder": [
    {
      "cafmSRNumber": "CAFM-2025-12345",
      "sourceSRNumber": "EQ-2025-001234",
      "sourceOrgId": "ORG-ALGHURAIR-001",
      "status": "true",
      "message": "Work order created successfully"
    }
  ]
}
```

**Task Exists Response (HTTP 200):**
```json
{
  "workOrder": [
    {
      "cafmSRNumber": "CAFM-2025-EXISTING",
      "sourceSRNumber": "EQ-2025-001234",
      "sourceOrgId": "ORG-ALGHURAIR-001",
      "status": "true",
      "message": "Task already exists in CAFM"
    }
  ]
}
```

**Error Response (HTTP 400):**
```json
{
  "workOrder": [
    {
      "cafmSRNumber": "",
      "sourceSRNumber": "EQ-2025-001234",
      "sourceOrgId": "ORG-ALGHURAIR-001",
      "status": "false",
      "message": "CAFM Log In Failed. CAFM Log In API Responded with Blank Response"
    }
  ]
}
```

### 17.2 Downstream System Layer Calls

#### Operation: Authenticate (Login)

**Request SOAP Envelope:**
```xml
<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" xmlns:ns="http://www.fsi.co.uk/services/evolution/04/09">
  <soapenv:Header/>
  <soapenv:Body>
    <ns:Authenticate>
      <ns:loginName>fsi_username</ns:loginName>
      <ns:password>fsi_password</ns:password>
    </ns:Authenticate>
  </soapenv:Body>
</soapenv:Envelope>
```

**Response SOAP Envelope (Success - HTTP 200):**
```xml
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
  <soap:Body>
    <AuthenticateResponse xmlns="http://www.fsi.co.uk/services/evolution/04/09">
      <AuthenticateResult>
        <SessionId>abc123-session-id-xyz789</SessionId>
      </AuthenticateResult>
    </AuthenticateResponse>
  </soap:Body>
</soap:Envelope>
```

**Response SOAP Envelope (Error - HTTP 401):**
```xml
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
  <soap:Body>
    <soap:Fault>
      <faultcode>soap:Client</faultcode>
      <faultstring>Authentication failed</faultstring>
    </soap:Fault>
  </soap:Body>
</soap:Envelope>
```

#### Operation: GetBreakdownTasksByDto

**Request SOAP Envelope:**
```xml
<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" xmlns:ns="http://www.fsi.co.uk/services/evolution/04/09">
  <soapenv:Header/>
  <soapenv:Body>
    <ns:GetBreakdownTasksByDto>
      <ns:sessionId>abc123-session-id-xyz789</ns:sessionId>
      <ns:breakdownTaskDto>
        <ns:CallId>EQ-2025-001234</ns:CallId>
      </ns:breakdownTaskDto>
    </ns:GetBreakdownTasksByDto>
  </soapenv:Body>
</soapenv:Envelope>
```

**Response SOAP Envelope (Success - Task Exists - HTTP 200):**
```xml
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
  <soap:Body>
    <GetBreakdownTasksByDtoResponse xmlns="http://www.fsi.co.uk/services/evolution/04/09">
      <GetBreakdownTasksByDtoResult>
        <BreakdownTask>
          <TaskId>CAFM-2025-EXISTING</TaskId>
          <CallId>EQ-2025-001234</CallId>
          <Status>Open</Status>
        </BreakdownTask>
      </GetBreakdownTasksByDtoResult>
    </GetBreakdownTasksByDtoResponse>
  </soap:Body>
</soap:Envelope>
```

**Response SOAP Envelope (Success - No Tasks - HTTP 200):**
```xml
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
  <soap:Body>
    <GetBreakdownTasksByDtoResponse xmlns="http://www.fsi.co.uk/services/evolution/04/09">
      <GetBreakdownTasksByDtoResult>
        <!-- Empty result -->
      </GetBreakdownTasksByDtoResult>
    </GetBreakdownTasksByDtoResponse>
  </soap:Body>
</soap:Envelope>
```

#### Operation: GetLocationsByDto

**Request SOAP Envelope:**
```xml
<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" xmlns:ns="http://www.fsi.co.uk/services/evolution/04/09">
  <soapenv:Header/>
  <soapenv:Body>
    <ns:GetLocationsByDto>
      <ns:sessionId>abc123-session-id-xyz789</ns:sessionId>
      <ns:locationDto>
        <ns:PropertyName>Al Ghurair Tower</ns:PropertyName>
        <ns:UnitCode>UNIT-301</ns:UnitCode>
      </ns:locationDto>
    </ns:GetLocationsByDto>
  </soapenv:Body>
</soapenv:Envelope>
```

**Response SOAP Envelope (Success - HTTP 200):**
```xml
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
  <soap:Body>
    <GetLocationsByDtoResponse xmlns="http://www.fsi.co.uk/services/evolution/04/09">
      <GetLocationsByDtoResult>
        <Location>
          <LocationId>LOC-12345</LocationId>
          <BuildingId>BLD-67890</BuildingId>
          <PropertyName>Al Ghurair Tower</PropertyName>
          <UnitCode>UNIT-301</UnitCode>
        </Location>
      </GetLocationsByDtoResult>
    </GetLocationsByDtoResponse>
  </soap:Body>
</soap:Envelope>
```

**Response SOAP Envelope (Error - HTTP 404):**
```xml
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
  <soap:Body>
    <soap:Fault>
      <faultcode>soap:Server</faultcode>
      <faultstring>Location not found</faultstring>
    </soap:Fault>
  </soap:Body>
</soap:Envelope>
```

#### Operation: GetInstructionSetsByDto

**Request SOAP Envelope:**
```xml
<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" xmlns:ns="http://www.fsi.co.uk/services/evolution/04/09">
  <soapenv:Header/>
  <soapenv:Body>
    <ns:GetInstructionSetsByDto>
      <ns:sessionId>abc123-session-id-xyz789</ns:sessionId>
      <ns:instructionDto>
        <ns:CategoryName>HVAC</ns:CategoryName>
        <ns:SubCategory>Air Conditioning</ns:SubCategory>
      </ns:instructionDto>
    </ns:GetInstructionSetsByDto>
  </soapenv:Body>
</soapenv:Envelope>
```

**Response SOAP Envelope (Success - HTTP 200):**
```xml
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
  <soap:Body>
    <GetInstructionSetsByDtoResponse xmlns="http://www.fsi.co.uk/services/evolution/04/09">
      <GetInstructionSetsByDtoResult>
        <Instruction>
          <InstructionId>INST-98765</InstructionId>
          <CategoryName>HVAC</CategoryName>
          <SubCategory>Air Conditioning</SubCategory>
        </Instruction>
      </GetInstructionSetsByDtoResult>
    </GetInstructionSetsByDtoResponse>
  </soap:Body>
</soap:Envelope>
```

#### Operation: CreateBreakdownTask

**Request SOAP Envelope:**
```xml
<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" xmlns:ns="http://www.fsi.co.uk/services/evolution/04/09">
  <soapenv:Header/>
  <soapenv:Body>
    <ns:CreateBreakdownTask>
      <ns:sessionId>abc123-session-id-xyz789</ns:sessionId>
      <ns:breakdownTaskDto>
        <ns:ReporterName>John Doe</ns:ReporterName>
        <ns:BDET_EMAIL>john.doe@example.com</ns:BDET_EMAIL>
        <ns:Phone>+971-50-123-4567</ns:Phone>
        <ns:CallId>EQ-2025-001234</ns:CallId>
        <ns:CategoryId>CAT-123</ns:CategoryId>
        <ns:DisciplineId>DISC-456</ns:DisciplineId>
        <ns:PriorityId>PRI-789</ns:PriorityId>
        <ns:BuildingId>BLD-67890</ns:BuildingId>
        <ns:LocationId>LOC-12345</ns:LocationId>
        <ns:InstructionId>INST-98765</ns:InstructionId>
        <ns:LongDescription>Air conditioning not working in office 301</ns:LongDescription>
        <ns:ScheduledDateUtc>2025-02-25T11:05:41.0208713Z</ns:ScheduledDateUtc>
        <ns:RaisedDateUtc>2025-02-24T10:30:00.0208713Z</ns:RaisedDateUtc>
        <ns:ContractId>CONTRACT-001</ns:ContractId>
        <ns:BDET_CALLER_SOURCE_ID>EQ_PLUS</ns:BDET_CALLER_SOURCE_ID>
      </ns:breakdownTaskDto>
    </ns:CreateBreakdownTask>
  </soapenv:Body>
</soapenv:Envelope>
```

**Response SOAP Envelope (Success - HTTP 200):**
```xml
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
  <soap:Body>
    <CreateBreakdownTaskResponse xmlns="http://www.fsi.co.uk/services/evolution/04/09">
      <CreateBreakdownTaskResult>
        <TaskId>CAFM-2025-12345</TaskId>
        <Status>Created</Status>
      </CreateBreakdownTaskResult>
    </CreateBreakdownTaskResponse>
  </soap:Body>
</soap:Envelope>
```

**Response SOAP Envelope (Error - HTTP 400):**
```xml
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
  <soap:Body>
    <soap:Fault>
      <faultcode>soap:Client</faultcode>
      <faultstring>Invalid request: Missing required field CategoryId</faultstring>
    </soap:Fault>
  </soap:Body>
</soap:Envelope>
```

#### Operation: CreateEvent

**Request SOAP Envelope:**
```xml
<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" xmlns:ns="http://www.fsi.co.uk/services/evolution/04/09">
  <soapenv:Header/>
  <soapenv:Body>
    <ns:CreateEvent>
      <ns:sessionId>abc123-session-id-xyz789</ns:sessionId>
      <ns:eventDto>
        <ns:TaskId>CAFM-2025-12345</ns:TaskId>
        <ns:EventType>Recurring</ns:EventType>
      </ns:eventDto>
    </ns:CreateEvent>
  </soapenv:Body>
</soapenv:Envelope>
```

**Response SOAP Envelope (Success - HTTP 200):**
```xml
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
  <soap:Body>
    <CreateEventResponse xmlns="http://www.fsi.co.uk/services/evolution/04/09">
      <CreateEventResult>
        <EventId>EVENT-2025-98765</EventId>
        <Status>Linked</Status>
      </CreateEventResult>
    </CreateEventResponse>
  </soap:Body>
</soap:Envelope>
```

#### Operation: Logout

**Request SOAP Envelope:**
```xml
<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" xmlns:ns="http://www.fsi.co.uk/services/evolution/04/09">
  <soapenv:Header/>
  <soapenv:Body>
    <ns:LogOut>
      <ns:sessionId>abc123-session-id-xyz789</ns:sessionId>
    </ns:LogOut>
  </soapenv:Body>
</soapenv:Envelope>
```

**Response:** No response body (HTTP 200 on success)

---

## 18. FUNCTION EXPOSURE DECISION TABLE

**🛑 VALIDATION CHECKPOINT:** This section is COMPLETE and MANDATORY before Phase 2.

### 18.1 Decision Process

**STEP 1: Create Decision Table**

| Operation | Independent Invoke? | Decision Before/After? | Same SOR? | Internal Lookup? | Conclusion | Reasoning |
|---|---|---|---|---|---|---|
| Login | NO | AFTER (shape4 checks 20*) | N/A | N/A | Atomic (Middleware) | Auth only, handled by middleware |
| GetBreakdownTasksByDto | YES | AFTER (shape55 checks empty) | YES | NO | **Function** | Process Layer decides skip/proceed based on existence |
| GetLocationsByDto | NO | None | YES | YES | Atomic | Internal lookup for CreateBreakdownTask |
| GetInstructionSetsByDto | NO | None | YES | YES | Atomic | Internal lookup for CreateBreakdownTask |
| CreateBreakdownTask | YES | AFTER (shape12 checks 20*) | YES | NO | **Function** | Complete business operation |
| CreateEvent | NO | None | YES | NO | Atomic | Conditional operation (only if recurrence=Y) |
| Logout | NO | None | N/A | N/A | Atomic (Middleware) | Cleanup, handled by middleware |

### 18.2 Question Answers

**Q1: Can Process Layer invoke independently?**
- GetBreakdownTasksByDto: YES (existence check - PL decides skip/proceed)
- CreateBreakdownTask: YES (main operation - PL calls to create)
- Others: NO (internal operations)

**Q2: Decision/conditional logic present?**
- GetBreakdownTasksByDto: YES (decision shape55 checks if task exists - PL decides)
- CreateBreakdownTask: YES (decision shape12 checks status - but error handling, not business logic)
- Others: NO

**Q2a: Is decision same SOR?**
- GetBreakdownTasksByDto: YES (all operations CAFM) - BUT decision is existence check that PL should control
- CreateBreakdownTask: YES (all operations CAFM) - BUT decision is error handling

**Q3: Only field extraction/lookup for another operation?**
- GetLocationsByDto: YES (lookup for CreateBreakdownTask)
- GetInstructionSetsByDto: YES (lookup for CreateBreakdownTask)
- CreateEvent: NO (complete operation, but conditional)

**Q4: Complete business operation Process Layer needs?**
- GetBreakdownTasksByDto: YES (existence check - PL decides based on result)
- CreateBreakdownTask: YES (main create operation)

### 18.3 Verification

1. ❓ Identified ALL decision points? **YES**
   - Decision shape55 (task exists) - PL should decide
   - Decision shape31 (recurrence) - PL should decide
   - Decision shape12 (status check) - Error handling, not business logic

2. ❓ WHERE each decision belongs? **YES**
   - shape55 (task exists) → Process Layer (existence check pattern)
   - shape31 (recurrence) → Process Layer (conditional operation)
   - shape12 (status check) → System Layer (error handling)

3. ❓ "if X exists, skip Y" checked? **YES**
   - GetBreakdownTasksByDto → If task exists, skip CreateBreakdownTask → Process Layer decides

4. ❓ "if flag=X, do Y" checked? **YES**
   - If recurrence=Y, do CreateEvent → Process Layer decides

5. ❓ Can explain WHY each operation type? **YES**
   - GetBreakdownTasksByDto: Function (PL needs to check existence independently)
   - CreateBreakdownTask: Function (main operation PL calls)
   - Lookups: Atomic (internal enrichment for CreateBreakdownTask)
   - CreateEvent: Atomic (conditional, orchestrated by PL)

6. ❓ Avoided pattern-matching? **YES** (analyzed decision points systematically)

7. ❓ If 1 Function, NO decision shapes? **N/A** (2 Functions)

### 18.4 Summary

**I will create 2 Azure Functions for CAFM:**

1. **GetBreakdownTasksByDtoAPI** - Check if work order exists in CAFM
2. **CreateBreakdownTaskAPI** - Create work order in CAFM

**Reasoning:**

Per Rule 1066, business decisions → Process Layer when:
- "if X exists, skip Y" → Process Layer decides (GetBreakdownTasksByDto result determines skip/proceed)
- "if flag=X, do Y" → Process Layer decides (recurrence flag determines CreateEvent execution)

**Functions:**
- **GetBreakdownTasksByDtoAPI:** Process Layer calls to check existence, decides whether to proceed with creation
- **CreateBreakdownTaskAPI:** Process Layer calls to create work order (after existence check passes)

**Internal Operations (Atomic Handlers):**
- AuthenticateAtomicHandler (for middleware)
- GetLocationsByDtoAtomicHandler (internal lookup)
- GetInstructionSetsByDtoAtomicHandler (internal lookup)
- CreateEventAtomicHandler (conditional operation)
- LogoutAtomicHandler (for middleware)

**Authentication Method:** Session-based with CustomAuthenticationMiddleware

**Process Layer Orchestration:**
```
1. Call GetBreakdownTasksByDtoAPI (check existence)
2. If task exists → Return existing task
3. If task not exists:
   a. Call CreateBreakdownTaskAPI (creates task with internal lookups)
   b. If recurrence=Y → Call CreateBreakdownTaskAPI again with CreateEvent flag (or separate CreateEventAPI)
```

**Note:** CreateEvent could be exposed as separate Function OR handled as parameter in CreateBreakdownTaskAPI. Recommend separate Function for clarity: **CreateEventAPI**.

**REVISED: 3 Azure Functions:**
1. **GetBreakdownTasksByDtoAPI** - Check existence
2. **CreateBreakdownTaskAPI** - Create work order
3. **CreateEventAPI** - Link recurring event

---

## 19. VALIDATION CHECKLIST

### Data Dependencies
- [x] All property WRITES identified (Section 7.1)
- [x] All property READS identified (Section 7.2)
- [x] Dependency graph built (Section 8)
- [x] Execution order satisfies all dependencies (Section 12)

### Decision Analysis
- [x] ALL decision shapes inventoried (Section 10)
- [x] BOTH TRUE and FALSE paths traced to termination (Section 10)
- [x] Pattern type identified for each decision (Section 10.3)
- [x] Early exits identified and documented (Section 10)
- [x] Convergence points identified (Section 10)

### Branch Analysis
- [x] Each branch classified as parallel or sequential (Section 11)
- [x] API calls checked in branch paths (Section 11.2)
- [x] Dependency graph built (Section 11.2)
- [x] Topological sort applied (Section 11.2)
- [x] Each path traced to terminal point (Section 11.2)
- [x] Convergence points identified (Section 11.2)

### Sequence Diagram
- [x] Format follows required structure (Section 13.2)
- [x] Each operation shows READS and WRITES (Section 13.2)
- [x] Each operation shows classification (Section 13.1)
- [x] Each operation shows error handling (Section 13.2)
- [x] Each operation shows result (Section 13.2)
- [x] Each operation shows Boomi reference (Section 13.2)
- [x] Decisions show both TRUE and FALSE paths (Section 13.2)
- [x] Check-before-create pattern shown (Section 13.2)
- [x] Early exits marked (Section 13.2)
- [x] Conditional execution marked (Section 13.2)

### Subprocess Analysis
- [x] ALL subprocesses analyzed (Section 14)
- [x] Return paths identified (Section 14)
- [x] Properties written/read documented (Section 14)

### Input/Output Structure Analysis
- [x] Step 1a (Input Structure Analysis) complete (Section 2)
- [x] Step 1b (Response Structure Analysis) complete (Section 3)
- [x] Step 1c (Operation Response Analysis) complete (Section 4)
- [x] Step 1d (Map Analysis) complete (Section 5)
- [x] Step 1e (HTTP Status Codes) complete (Section 6)
- [x] Field mapping tables complete (Sections 2, 3)
- [x] Array detection documented (Section 2.1)
- [x] Document processing behavior determined (Section 2.3)

### Function Exposure Decision
- [x] Decision table complete (Section 18.1)
- [x] All 4 questions answered (Section 18.2)
- [x] Reasoning documented (Section 18.4)
- [x] Summary provided (Section 18.4)

---

## PHASE 1 COMPLETION STATEMENT

✅ **Phase 1 extraction COMPLETE with operation classification.**

✅ **Section 13 is COMPLETE technical specification with error handling for ALL operations.**

✅ **Ready for code generation without assumptions.**

**Verification Question:** Can a developer generate code from Section 13 without making assumptions?
- **Answer:** YES
- **Proof:** Section 13.1 classifies ALL operations with explicit error handling
- **Proof:** Section 13.2 shows error handling for EACH operation (throw vs continue)
- **Proof:** All behavior is explicit (no ambiguity)

---

**END OF PHASE 1 EXTRACTION DOCUMENT**

**Next Steps:**
1. Commit this document
2. Proceed to Phase 2 (Code Generation)
3. Generate System Layer code using Section 13 as PRIMARY BLUEPRINT
