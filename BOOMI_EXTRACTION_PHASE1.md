# BOOMI EXTRACTION PHASE 1 - HCM Leave Create

**Process Name:** HCM_Leave Create  
**Process ID:** ca69f858-785f-4565-ba1f-b4edc6cca05b  
**Description:** This Process will sync the Leave data between D365 and Oracle HCM  
**Analysis Date:** 2026-02-20  
**Business Domain:** Human Resource (HCM)

---

## 1. Operations Inventory

### Main Process Operations

| Operation ID | Operation Name | Type | SubType | Purpose |
|---|---|---|---|---|
| 8f709c2b-e63f-4d5f-9374-2932ed70415d | Create Leave Oracle Fusion OP | connector-action | wss | Web Service Server Listen - Entry point |
| 6e8920fd-af5a-430b-a1d9-9fde7ac29a12 | Leave Oracle Fusion Create | connector-action | http | HTTP POST to Oracle Fusion HCM API |
| af07502a-fafd-4976-a691-45d51a33b549 | Email w Attachment | connector-action | mail | Send email with attachment |
| 15a72a21-9b57-49a1-a8ed-d70367146644 | Email W/O Attachment | connector-action | mail | Send email without attachment |

### Subprocess Operations

**Subprocess ID:** a85945c5-3004-42b9-80b1-104f465cd1fb  
**Subprocess Name:** (Sub) Office 365 Email  
**Purpose:** Email notification subprocess for error handling

---

## 2. Input Structure Analysis (Step 1a)

### Entry Point Identification

**Entry Operation:** shape1 (START shape)  
**Operation ID:** 8f709c2b-e63f-4d5f-9374-2932ed70415d  
**Operation Name:** Create Leave Oracle Fusion OP  
**Operation Type:** connector-action (wss - Web Services Server)  
**JSON Reference:** Lines 46-56 in process_root_ca69f858-785f-4565-ba1f-b4edc6cca05b.json

### Request Profile Structure

**Profile ID:** febfa3e1-f719-4ee8-ba57-cdae34137ab3  
**Profile Name:** D365 Leave Create JSON Profile  
**Profile Type:** profile.json  
**Input Type:** singlejson  
**JSON Reference:** Line 38 in operation_8f709c2b-e63f-4d5f-9374-2932ed70415d.json

### Profile Structure Analysis

**Root Structure:** Root/Object  
**Array Detection:** ❌ NO - Single object structure  
**Array Cardinality:** N/A (not an array)

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

### Complete Field Inventory

| Key | Field Name | Full Path | Data Type | Required | Mappable |
|---|---|---|---|---|---|
| 3 | employeeNumber | Root/Object/employeeNumber | number | Yes | Yes |
| 4 | absenceType | Root/Object/absenceType | character | Yes | Yes |
| 5 | employer | Root/Object/employer | character | Yes | Yes |
| 6 | startDate | Root/Object/startDate | character | Yes | Yes |
| 7 | endDate | Root/Object/endDate | character | Yes | Yes |
| 8 | absenceStatusCode | Root/Object/absenceStatusCode | character | Yes | Yes |
| 9 | approvalStatusCode | Root/Object/approvalStatusCode | character | Yes | Yes |
| 10 | startDateDuration | Root/Object/startDateDuration | number | Yes | Yes |
| 11 | endDateDuration | Root/Object/endDateDuration | number | Yes | Yes |

**Total Fields:** 9 fields (all flat structure, no nested objects)

### Document Processing Behavior

**Input Type:** singlejson  
**Structure Type:** Single object (not array)  
**Processing Behavior:** Single document processing  
**Execution Pattern:** One execution per request  
**Session Management:** One session per execution

---

## 3. Response Structure Analysis (Step 1b)

### Response Profile Structure

**Profile ID:** f4ca3a70-114a-4601-bad8-44a3eb20e2c0  
**Profile Name:** Leave D365 Response  
**Profile Type:** profile.json  
**Output Type:** singlejson  
**JSON Reference:** Operation configuration does not specify explicit response profile, but maps target this profile

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

### Complete Response Field Inventory

| Key | Field Name | Full Path | Data Type | Mappable |
|---|---|---|---|---|
| 4 | status | leaveResponse/Object/status | character | Yes |
| 5 | message | leaveResponse/Object/message | character | Yes |
| 6 | personAbsenceEntryId | leaveResponse/Object/personAbsenceEntryId | number | Yes |
| 7 | success | leaveResponse/Object/success | character | Yes |

**Total Response Fields:** 4 fields

---

## 4. Operation Response Analysis (Step 1c)

### Operation: Leave Oracle Fusion Create (6e8920fd-af5a-430b-a1d9-9fde7ac29a12)

**Operation Type:** HTTP POST  
**Response Profile:** NONE (responseProfileType: "NONE")  
**Return Responses:** true  
**Return Errors:** true  
**JSON Reference:** Lines 32-42 in operation_6e8920fd-af5a-430b-a1d9-9fde7ac29a12.json

**Response Header Mapping:**
- Header Field: "Content-Encoding"
- Target Property: "DDP_RespHeader" (Dynamic Document Property)
- JSON Reference: Lines 54-62 in operation_6e8920fd-af5a-430b-a1d9-9fde7ac29a12.json

**Extracted Fields:**
- **DDP_RespHeader** (from HTTP response header "Content-Encoding")
  - Extracted by: Operation configuration (automatic header mapping)
  - Written to: dynamicdocument.DDP_RespHeader
  - Consumers: shape44 (Decision: Check Response Content Type)

**Track Properties Available After Operation:**
- **meta.base.applicationstatuscode** - HTTP status code from Oracle Fusion API
  - Consumers: shape2 (Decision: HTTP Status 20 check)
- **meta.base.applicationstatusmessage** - HTTP response message
  - Consumers: shape39, shape46 (Error message extraction)

**Business Logic Implications:**
- Operation produces HTTP status code → Decision shape2 checks if status is 20* (success)
- Operation produces response header → Decision shape44 checks if Content-Encoding is "gzip"
- If status is NOT 20*, error path is triggered
- If Content-Encoding is "gzip", response must be decompressed

**Data Flow:**
```
Operation (Leave Oracle Fusion Create)
  ↓ Produces
  - meta.base.applicationstatuscode (HTTP status)
  - meta.base.applicationstatusmessage (HTTP message)
  - dynamicdocument.DDP_RespHeader (Content-Encoding header)
  - Response body (JSON from Oracle Fusion)
  ↓ Consumed by
  - shape2 (Decision: HTTP Status 20 check) - Reads meta.base.applicationstatuscode
  - shape44 (Decision: Check Response Content Type) - Reads dynamicdocument.DDP_RespHeader
  - shape39, shape46 (Error handling) - Reads meta.base.applicationstatusmessage
```

### Operation: Email w Attachment (af07502a-fafd-4976-a691-45d51a33b549)

**Operation Type:** Mail Send  
**Response Profile:** NONE  
**Purpose:** Send error notification email with payload attachment  
**JSON Reference:** Lines 24-42 in operation_af07502a-fafd-4976-a691-45d51a33b549.json

**No response data extracted** - Mail operations do not return data

### Operation: Email W/O Attachment (15a72a21-9b57-49a1-a8ed-d70367146644)

**Operation Type:** Mail Send  
**Response Profile:** NONE  
**Purpose:** Send error notification email without attachment  
**JSON Reference:** Lines 24-42 in operation_15a72a21-9b57-49a1-a8ed-d70367146644.json

**No response data extracted** - Mail operations do not return data

---

## 5. Map Analysis (Step 1d)

### Map Inventory

| Map ID | Map Name | From Profile | To Profile | Type |
|---|---|---|---|---|
| c426b4d6-2aff-450e-b43b-59956c4dbc96 | Leave Create Map | febfa3e1-f719-4ee8-ba57-cdae34137ab3 | a94fa205-c740-40a5-9fda-3d018611135a | Request transformation |
| e4fd3f59-edb5-43a1-aeae-143b600a064e | Oracle Fusion Leave Response Map | 316175c7-0e45-4869-9ac6-5f9d69882a62 | f4ca3a70-114a-4601-bad8-44a3eb20e2c0 | Response transformation |
| f46b845a-7d75-41b5-b0ad-c41a6a8e9b12 | Leave Error Map | 23d7a2e9-5cb0-4e9c-9e4b-3154834bad0d | f4ca3a70-114a-4601-bad8-44a3eb20e2c0 | Error response transformation |

### Map 1: Leave Create Map (c426b4d6-2aff-450e-b43b-59956c4dbc96)

**Purpose:** Transform D365 request to Oracle Fusion HCM request format  
**From Profile:** febfa3e1-f719-4ee8-ba57-cdae34137ab3 (D365 Leave Create JSON Profile)  
**To Profile:** a94fa205-c740-40a5-9fda-3d018611135a (HCM Leave Create JSON Profile)  
**Type:** HTTP Request Transformation (NOT SOAP - this is REST API)

**Field Mappings:**

| Source Field | Source Path | Target Field | Target Path | Transformation |
|---|---|---|---|---|
| employeeNumber | Root/Object/employeeNumber | personNumber | Root/Object/personNumber | Direct mapping (field name change) |
| absenceType | Root/Object/absenceType | absenceType | Root/Object/absenceType | Direct mapping |
| employer | Root/Object/employer | employer | Root/Object/employer | Direct mapping |
| startDate | Root/Object/startDate | startDate | Root/Object/startDate | Direct mapping |
| endDate | Root/Object/endDate | endDate | Root/Object/endDate | Direct mapping |
| absenceStatusCode | Root/Object/absenceStatusCode | absenceStatusCd | Root/Object/absenceStatusCd | Direct mapping (field name change) |
| approvalStatusCode | Root/Object/approvalStatusCode | approvalStatusCd | Root/Object/approvalStatusCd | Direct mapping (field name change) |
| startDateDuration | Root/Object/startDateDuration | startDateDuration | Root/Object/startDateDuration | Direct mapping |
| endDateDuration | Root/Object/endDateDuration | endDateDuration | Root/Object/endDateDuration | Direct mapping |

**Profile vs Map Field Name Comparison:**

| D365 Field Name (Source) | Oracle Fusion Field Name (Target) | Authority | Notes |
|---|---|---|---|
| employeeNumber | personNumber | ✅ MAP | Field name changed for Oracle Fusion API |
| absenceStatusCode | absenceStatusCd | ✅ MAP | Field name abbreviated |
| approvalStatusCode | approvalStatusCd | ✅ MAP | Field name abbreviated |

**Scripting Functions:** None  
**Static Values:** None  
**Process Properties Used:** None

**CRITICAL RULE:** This is a REST API (not SOAP), so no SOAP envelope is required. The map transforms JSON to JSON.

### Map 2: Oracle Fusion Leave Response Map (e4fd3f59-edb5-43a1-aeae-143b600a064e)

**Purpose:** Transform Oracle Fusion response to D365 response format  
**From Profile:** 316175c7-0e45-4869-9ac6-5f9d69882a62 (Oracle Fusion Leave Response JSON Profile)  
**To Profile:** f4ca3a70-114a-4601-bad8-44a3eb20e2c0 (Leave D365 Response)  
**Type:** Success Response Transformation

**Field Mappings:**

| Source Field | Source Path | Target Field | Target Path | Transformation |
|---|---|---|---|---|
| personAbsenceEntryId | Root/Object/personAbsenceEntryId | personAbsenceEntryId | leaveResponse/Object/personAbsenceEntryId | Direct mapping |

**Default Values (Static):**

| Target Field | Target Key | Default Value | Purpose |
|---|---|---|---|
| status | 4 | "success" | Success indicator |
| message | 5 | "Data successfully sent to Oracle Fusion" | Success message |
| success | 7 | "true" | Boolean success flag |

**JSON Reference:** Lines 47-67 in map_e4fd3f59-edb5-43a1-aeae-143b600a064e.json

### Map 3: Leave Error Map (f46b845a-7d75-41b5-b0ad-c41a6a8e9b12)

**Purpose:** Transform error details to D365 error response format  
**From Profile:** 23d7a2e9-5cb0-4e9c-9e4b-3154834bad0d (Dummy FF Profile)  
**To Profile:** f4ca3a70-114a-4601-bad8-44a3eb20e2c0 (Leave D365 Response)  
**Type:** Error Response Transformation

**Field Mappings:**

| Source | Source Type | Target Field | Target Path | Transformation |
|---|---|---|---|---|
| DPP_ErrorMessage | Process Property (Function) | message | leaveResponse/Object/message | Get process property value |

**Function Analysis:**

| Function Key | Function Type | Input | Output | Logic |
|---|---|---|---|---|
| 1 | PropertyGet | Property Name: "DPP_ErrorMessage" | Result | Retrieves error message from process property |

**JSON Reference:** Lines 45-88 in map_f46b845a-7d75-41b5-b0ad-c41a6a8e9b12.json

**Default Values (Static):**

| Target Field | Target Key | Default Value | Purpose |
|---|---|---|---|
| status | 4 | "failure" | Failure indicator |
| success | 7 | "false" | Boolean failure flag |

**CRITICAL RULE:** Error map uses process property DPP_ErrorMessage to populate error response message field.

---

## 6. Process Properties Analysis (Steps 2-3)

### Property WRITES (Step 2)

| Property Name | Written By Shape | Source Type | Source Value | JSON Reference |
|---|---|---|---|---|
| process.DPP_Process_Name | shape38 | execution | Process Name | Lines 514-530 |
| process.DPP_AtomName | shape38 | execution | Atom Name | Lines 537-555 |
| process.DPP_Payload | shape38 | current | Current document | Lines 562-574 |
| process.DPP_ExecutionID | shape38 | execution | Execution Id | Lines 581-598 |
| process.DPP_File_Name | shape38 | concatenated | Process Name + Timestamp + ".txt" | Lines 605-647 |
| process.DPP_Subject | shape38 | concatenated | Atom Name + " (" + Process Name + " ) has errors to report" | Lines 653-706 |
| process.To_Email | shape38 | definedparameter | PP_HCM_LeaveCreate_Properties.To_Email | Lines 713-733 |
| process.DPP_HasAttachment | shape38 | definedparameter | PP_HCM_LeaveCreate_Properties.DPP_HasAttachment | Lines 739-760 |
| process.DPP_ErrorMessage | shape19 | track | meta.base.catcherrorsmessage | Lines 239-265 (Try/Catch error path) |
| process.DPP_ErrorMessage | shape39 | track | meta.base.applicationstatusmessage | Lines 786-813 (HTTP error path) |
| process.DPP_ErrorMessage | shape46 | current | meta.base.applicationstatusmessage | Lines 976-1000 (GZIP error path) |
| dynamicdocument.URL | shape8 | definedparameter | PP_HCM_LeaveCreate_Properties.Resource_Path | Lines 146-172 |

### Property READS (Step 3)

| Property Name | Read By Shape | Shape Type | Usage Context | JSON Reference |
|---|---|---|---|---|
| dynamicdocument.URL | shape33 (Operation) | connectoraction | HTTP operation URL path element | operation_6e8920fd-af5a-430b-a1d9-9fde7ac29a12.json, lines 45-52 |
| process.DPP_HasAttachment | shape4 (Subprocess) | decision | Check if email should have attachment | subprocess lines 148-158 |
| process.To_Email | shape6, shape20 (Subprocess) | documentproperties | Mail To Address | subprocess lines 262-278, 732-748 |
| process.DPP_Subject | shape6, shape20 (Subprocess) | documentproperties | Mail Subject | subprocess lines 289-318, 760-789 |
| process.DPP_MailBody | shape6, shape20 (Subprocess) | documentproperties | Mail Body | subprocess lines 326-344, 798-815 |
| process.DPP_File_Name | shape6 (Subprocess) | documentproperties | Mail File Name | subprocess lines 351-369 |
| process.DPP_Process_Name | shape11, shape23 (Subprocess) | message | Email body parameter {1} | subprocess lines 493-504, 850-860 |
| process.DPP_AtomName | shape11, shape23 (Subprocess) | message | Email body parameter {2} | subprocess lines 506-516, 862-872 |
| process.DPP_ExecutionID | shape11, shape23 (Subprocess) | message | Email body parameter {3} | subprocess lines 518-528, 874-884 |
| process.DPP_ErrorMessage | shape11, shape23 (Subprocess) | message | Email body parameter {4} | subprocess lines 530-540, 886-896 |
| process.DPP_Payload | shape15 (Subprocess) | message | Email attachment content {1} | subprocess lines 618-629 |

### Property Dependency Summary

**Properties Written BEFORE Subprocess Call:**
- process.DPP_Process_Name (shape38)
- process.DPP_AtomName (shape38)
- process.DPP_Payload (shape38)
- process.DPP_ExecutionID (shape38)
- process.DPP_File_Name (shape38)
- process.DPP_Subject (shape38)
- process.To_Email (shape38)
- process.DPP_HasAttachment (shape38)
- process.DPP_ErrorMessage (shape19, shape39, or shape46 - depending on error path)

**Properties Read BY Subprocess:**
- All properties listed above are read by subprocess shapes

**Validation:** ✅ All property reads happen AFTER property writes (shape38 writes before subprocess reads)

---

## 7. Data Dependency Graph (Step 4)

### Dependency Chains

**Chain 1: Input Properties → Try/Catch → Error Handling**
```
shape38 (Input_details) WRITES:
  - process.DPP_Process_Name
  - process.DPP_AtomName
  - process.DPP_Payload
  - process.DPP_ExecutionID
  - process.DPP_File_Name
  - process.DPP_Subject
  - process.To_Email
  - process.DPP_HasAttachment
  ↓
shape17 (Try/Catch) - No dependencies
  ↓ [Try Path]
  shape29 (Map) → shape8 (set URL) → shape49 (Notify) → shape33 (HTTP Operation)
  ↓ [Catch Path]
  shape20 (Branch) → shape19 (ErrorMsg) or shape41 (Error Map)
```

**Chain 2: HTTP Operation → Status Check → Response/Error Handling**
```
shape33 (Leave Oracle Fusion Create) PRODUCES:
  - meta.base.applicationstatuscode
  - meta.base.applicationstatusmessage
  - dynamicdocument.DDP_RespHeader
  ↓
shape2 (Decision: HTTP Status 20 check) READS:
  - meta.base.applicationstatuscode
  ↓ [TRUE Path]
  shape34 (Success Map) → shape35 (Success Response)
  ↓ [FALSE Path]
  shape44 (Decision: Check Response Content Type) READS:
    - dynamicdocument.DDP_RespHeader
    ↓ [TRUE Path - GZIP]
    shape45 (Decompress) → shape46 (Extract Error) → shape47 (Error Map) → shape48 (Error Response)
    ↓ [FALSE Path - Not GZIP]
    shape39 (Extract Error) → shape40 (Error Map) → shape36 (Error Response)
```

**Chain 3: Error Properties → Subprocess Email**
```
shape19 (ErrorMsg) WRITES:
  - process.DPP_ErrorMessage (from meta.base.catcherrorsmessage)
  ↓
shape21 (Subprocess: Office 365 Email) READS:
  - process.DPP_ErrorMessage
  - process.DPP_Process_Name
  - process.DPP_AtomName
  - process.DPP_ExecutionID
  - process.DPP_File_Name
  - process.DPP_Subject
  - process.To_Email
  - process.DPP_HasAttachment
  - process.DPP_Payload (for attachment)
```

**OR**

```
shape39 (error msg) WRITES:
  - process.DPP_ErrorMessage (from meta.base.applicationstatusmessage)
  ↓
shape40 (Error Map) READS:
  - process.DPP_ErrorMessage
  ↓
shape36 (Error Response) - Returns error
```

**OR**

```
shape46 (error msg) WRITES:
  - process.DPP_ErrorMessage (from meta.base.applicationstatusmessage)
  ↓
shape47 (Error Map) READS:
  - process.DPP_ErrorMessage
  ↓
shape48 (Error Response) - Returns error
```

### Independent Operations

**No independent operations** - All operations have dependencies on properties or prior operations.

### Critical Dependencies

1. **shape38 MUST execute BEFORE shape17** - Writes all required properties
2. **shape33 MUST execute BEFORE shape2** - Produces HTTP status code
3. **shape2 MUST execute BEFORE shape34 or shape44** - Routes based on status
4. **shape19 MUST execute BEFORE shape21** - Writes error message for subprocess
5. **shape39 MUST execute BEFORE shape40** - Writes error message for error map
6. **shape46 MUST execute BEFORE shape47** - Writes error message for error map

---

## 8. Control Flow Graph (Step 5)

### Control Flow Mapping

| From Shape | To Shape | Identifier | Text | JSON Reference |
|---|---|---|---|---|
| shape1 (START) | shape38 | default | - | Lines 58-66 |
| shape38 (Input_details) | shape17 | default | - | Lines 765-772 |
| shape17 (Try/Catch) | shape29 | default | Try | Lines 205-212 |
| shape17 (Try/Catch) | shape20 | error | Catch | Lines 215-223 |
| shape29 (Map) | shape8 | default | - | Lines 388-395 |
| shape8 (set URL) | shape49 | default | - | Lines 175-182 |
| shape49 (Notify) | shape33 | default | - | Lines 1134-1141 |
| shape33 (HTTP Operation) | shape2 | default | - | Lines 423-430 |
| shape2 (Decision: HTTP Status 20 check) | shape34 | true | True | Lines 112-120 |
| shape2 (Decision: HTTP Status 20 check) | shape44 | false | False | Lines 122-130 |
| shape34 (Success Map) | shape35 | default | - | Lines 450-457 |
| shape35 (Success Response) | - | - | [TERMINAL] | No dragpoints |
| shape44 (Decision: Check Response Content Type) | shape45 | true | True | Lines 941-949 |
| shape44 (Decision: Check Response Content Type) | shape39 | false | False | Lines 951-959 |
| shape45 (Decompress GZIP) | shape46 | default | - | Lines 1045-1052 |
| shape46 (error msg) | shape47 | default | - | Lines 1003-1010 |
| shape47 (Error Map) | shape48 | default | - | Lines 1072-1079 |
| shape48 (Error Response) | - | - | [TERMINAL] | No dragpoints |
| shape39 (error msg) | shape40 | default | - | Lines 815-822 |
| shape40 (Error Map) | shape36 | default | - | Lines 842-849 |
| shape36 (Error Response) | - | - | [TERMINAL] | No dragpoints |
| shape20 (Branch) | shape19 | 1 | 1 | Lines 296-304 |
| shape20 (Branch) | shape41 | 2 | 2 | Lines 306-315 |
| shape19 (ErrorMsg) | shape21 | default | - | Lines 267-274 |
| shape21 (Subprocess Call) | - | - | [TERMINAL] | No dragpoints |
| shape41 (Error Map) | shape43 | default | - | Lines 869-876 |
| shape43 (Error Response) | - | - | [TERMINAL] | No dragpoints |

### Connection Summary

- **Total Shapes:** 14 main process shapes + 1 subprocess
- **Total Connections:** 21 dragpoint connections
- **Shapes with Multiple Outgoing Connections:**
  - shape17 (Try/Catch): 2 paths (Try, Catch)
  - shape2 (Decision): 2 paths (True, False)
  - shape44 (Decision): 2 paths (True, False)
  - shape20 (Branch): 2 paths (1, 2)

### Reverse Flow Mapping (Step 6)

| Target Shape | Incoming From Shapes | Convergence Point? |
|---|---|---|
| shape38 | shape1 | No |
| shape17 | shape38 | No |
| shape29 | shape17 | No |
| shape8 | shape29 | No |
| shape49 | shape8 | No |
| shape33 | shape49 | No |
| shape2 | shape33 | No |
| shape34 | shape2 | No |
| shape35 | shape34 | No (Terminal) |
| shape44 | shape2 | No |
| shape45 | shape44 | No |
| shape46 | shape45 | No |
| shape47 | shape46 | No |
| shape48 | shape47 | No (Terminal) |
| shape39 | shape44 | No |
| shape40 | shape39 | No |
| shape36 | shape40 | No (Terminal) |
| shape20 | shape17 | No |
| shape19 | shape20 | No |
| shape21 | shape19 | No (Terminal) |
| shape41 | shape20 | No |
| shape43 | shape41 | No (Terminal) |

**Convergence Points:** None - All paths lead to terminal points (Return Documents or Subprocess Call)

---

## 9. Decision Shape Analysis (Step 7)

### Decision Inventory

#### Decision 1: shape2 - HTTP Status 20 check

**Shape ID:** shape2  
**User Label:** "HTTP Status 20 check"  
**Comparison Type:** wildcard  
**JSON Reference:** Lines 71-133 in process_root

**Decision Values:**
- **Value 1:** meta.base.applicationstatuscode (track property)
- **Value 2:** "20*" (static - matches any 20x status code)

**Data Source Analysis:**
- **Data Source:** TRACK_PROPERTY (meta.base.applicationstatuscode)
- **Source Operation:** shape33 (Leave Oracle Fusion Create HTTP operation)
- **Data Type:** HTTP status code from Oracle Fusion API response
- **Classification:** POST-OPERATION (checks response from HTTP operation)

**Decision Type Classification:**
- **Type:** POST-OPERATION
- **Reasoning:** Checks HTTP status code from operation response (meta.base.applicationstatuscode is populated by HTTP connector after operation executes)

**Actual Execution Order:**
```
shape33 (HTTP Operation: Leave Oracle Fusion Create)
  ↓ Produces
  meta.base.applicationstatuscode (HTTP status code)
  ↓ Then
shape2 (Decision: Check if status is 20*)
  ↓ Routes based on status
  TRUE → Success path
  FALSE → Error path
```

**Dragpoints:**
- **TRUE Path:** → shape34 (Success Map)
- **FALSE Path:** → shape44 (Decision: Check Response Content Type)

**Path Tracing:**

**TRUE Path Termination:**
```
shape34 (Map: Oracle Fusion Leave Response Map)
  ↓
shape35 (Return Documents: Success Response)
  [TERMINAL - Return Documents]
```

**FALSE Path Termination:**
```
shape44 (Decision: Check Response Content Type)
  ↓ [TRUE - GZIP]
  shape45 (Decompress) → shape46 (error msg) → shape47 (Error Map) → shape48 (Return Documents: Error Response)
  [TERMINAL - Return Documents]
  ↓ [FALSE - Not GZIP]
  shape39 (error msg) → shape40 (Error Map) → shape36 (Return Documents: Error Response)
  [TERMINAL - Return Documents]
```

**Pattern Type:** Error Check (Success vs Failure)  
**Convergence Point:** None (paths terminate separately)  
**Early Exit:** Both paths terminate (TRUE → Success Return, FALSE → Error Return)

#### Decision 2: shape44 - Check Response Content Type

**Shape ID:** shape44  
**User Label:** "Check Response Content Type"  
**Comparison Type:** equals  
**JSON Reference:** Lines 899-962 in process_root

**Decision Values:**
- **Value 1:** dynamicdocument.DDP_RespHeader (track property - actually dynamic document property)
- **Value 2:** "gzip" (static)

**Data Source Analysis:**
- **Data Source:** TRACK_PROPERTY (dynamicdocument.DDP_RespHeader)
- **Source Operation:** shape33 (Leave Oracle Fusion Create HTTP operation)
- **Data Type:** HTTP response header "Content-Encoding"
- **Classification:** POST-OPERATION (checks response header from HTTP operation)

**Decision Type Classification:**
- **Type:** POST-OPERATION
- **Reasoning:** Checks HTTP response header value (DDP_RespHeader is populated by HTTP connector's responseHeaderMapping after operation executes)

**Actual Execution Order:**
```
shape33 (HTTP Operation: Leave Oracle Fusion Create)
  ↓ Produces
  dynamicdocument.DDP_RespHeader (Content-Encoding header)
  ↓ Then
shape2 (Decision: Check HTTP status)
  ↓ FALSE Path (status NOT 20*)
shape44 (Decision: Check if Content-Encoding is "gzip")
  ↓ Routes based on encoding
  TRUE → Decompress GZIP response
  FALSE → Use response as-is
```

**Dragpoints:**
- **TRUE Path:** → shape45 (Decompress GZIP)
- **FALSE Path:** → shape39 (Extract error message)

**Path Tracing:**

**TRUE Path Termination:**
```
shape45 (Data Process: Decompress GZIP)
  ↓
shape46 (Extract error message from decompressed response)
  ↓
shape47 (Map: Leave Error Map)
  ↓
shape48 (Return Documents: Error Response)
  [TERMINAL - Return Documents]
```

**FALSE Path Termination:**
```
shape39 (Extract error message from response)
  ↓
shape40 (Map: Leave Error Map)
  ↓
shape36 (Return Documents: Error Response)
  [TERMINAL - Return Documents]
```

**Pattern Type:** Conditional Logic (Data Processing - Handle GZIP compression)  
**Convergence Point:** None (both paths terminate at different Return Documents shapes)  
**Early Exit:** Both paths terminate with error response

### Decision Patterns Summary

1. **Error Check Pattern** (shape2): Check HTTP status code → Success or Error
2. **Conditional Processing Pattern** (shape44): Check response encoding → Decompress if GZIP

### Self-Check Results

- ✅ **Decision data sources identified:** YES
  - shape2: TRACK_PROPERTY (meta.base.applicationstatuscode)
  - shape44: TRACK_PROPERTY (dynamicdocument.DDP_RespHeader)

- ✅ **Decision types classified:** YES
  - shape2: POST-OPERATION (checks HTTP response status)
  - shape44: POST-OPERATION (checks HTTP response header)

- ✅ **Execution order verified:** YES
  - shape2 executes AFTER shape33 (HTTP operation)
  - shape44 executes AFTER shape2 (in FALSE path)

- ✅ **All decision paths traced:** YES
  - shape2: TRUE → shape34 → shape35 (Success Return)
  - shape2: FALSE → shape44 → [shape45 → shape46 → shape47 → shape48] OR [shape39 → shape40 → shape36] (Error Returns)
  - shape44: TRUE → shape45 → shape46 → shape47 → shape48 (Error Return with GZIP decompression)
  - shape44: FALSE → shape39 → shape40 → shape36 (Error Return without decompression)

- ✅ **Decision patterns identified:** YES
  - Error Check (Success vs Failure)
  - Conditional Processing (GZIP handling)

- ✅ **Paths traced to termination:** YES
  - All paths terminate at Return Documents shapes

---

## 10. Branch Shape Analysis (Step 8)

### Branch: shape20 - Error Handling Branch

**Shape ID:** shape20  
**Number of Paths:** 2  
**Location:** Catch path of Try/Catch (shape17)  
**JSON Reference:** Lines 279-317 in process_root

### Step 1: Properties Analysis

**Path 1 (Branch identifier "1" → shape19):**
- **Reads:** 
  - meta.base.catcherrorsmessage (track property - from Try/Catch)
- **Writes:**
  - process.DPP_ErrorMessage (shape19)

**Path 2 (Branch identifier "2" → shape41):**
- **Reads:**
  - process.DPP_ErrorMessage (used by map function in shape41's map)
- **Writes:**
  - None

**JSON Proof:**
- Path 1 reads: Lines 255-261 in shape19 configuration
- Path 1 writes: Lines 246 in shape19 configuration
- Path 2 reads: Lines 31-37 in map_f46b845a-7d75-41b5-b0ad-c41a6a8e9b12.json (map function reads DPP_ErrorMessage)

### Step 2: Dependency Graph

```
Path 1 (shape19) WRITES process.DPP_ErrorMessage
  ↓
Path 2 (shape41) READS process.DPP_ErrorMessage
```

**Dependency:** Path 2 depends on Path 1 (reads property that Path 1 writes)

### Step 3: Classification

**Classification:** SEQUENTIAL  
**Reasoning:** Path 2 reads process.DPP_ErrorMessage which Path 1 writes, therefore Path 2 depends on Path 1

**API Call Detection:**
- Path 1: No API calls (documentproperties shape only)
- Path 2: No API calls (map shape only)
- **Result:** No API calls, but data dependency exists

### Step 4: Topological Sort Order

**Dependency Graph:**
```
Path 1 (shape19) → Path 2 (shape41)
```

**Topological Sort:**
1. Path 1 (shape19) - No incoming dependencies
2. Path 2 (shape41) - Depends on Path 1

**Execution Order:** Path 1 → Path 2

### Step 5: Path Termination

**Path 1 Termination:**
```
shape19 (ErrorMsg)
  ↓
shape21 (Subprocess Call: Office 365 Email)
  [TERMINAL - Subprocess execution, no return to main process]
```

**Path 2 Termination:**
```
shape41 (Error Map)
  ↓
shape43 (Return Documents: Error Response)
  [TERMINAL - Return Documents]
```

### Step 6: Convergence Points

**Convergence Points:** None - Each path terminates independently

### Step 7: Execution Continuation

**Execution Continues From:** None - Both paths terminate (subprocess call or return documents)

### Step 8: Complete Analysis Documentation

**Branch Analysis Summary:**
- **Shape ID:** shape20
- **Number of Paths:** 2
- **Classification:** SEQUENTIAL
- **Dependency Order:** Path 1 → Path 2
- **Path 1 Terminal:** Subprocess call (shape21)
- **Path 2 Terminal:** Return Documents (shape43)
- **Convergence Points:** None
- **Execution Continuation:** None (both paths terminate)

### Self-Check Results

- ✅ **Classification completed:** YES - SEQUENTIAL
- ✅ **Assumption check:** NO (analyzed dependencies) - Did NOT assume parallel
- ✅ **Properties extracted:** YES
  - Path 1 reads: meta.base.catcherrorsmessage
  - Path 1 writes: process.DPP_ErrorMessage
  - Path 2 reads: process.DPP_ErrorMessage
- ✅ **Dependency graph built:** YES - Path 2 depends on Path 1
- ✅ **Topological sort applied:** YES - Path 1 → Path 2

---

## 11. Subprocess Analysis (Step 7a)

### Subprocess: (Sub) Office 365 Email (a85945c5-3004-42b9-80b1-104f465cd1fb)

**Called By:** shape21 (main process, catch error path)  
**Purpose:** Send error notification email via Office 365

### Internal Flow Analysis

**Subprocess Shapes:**

| Shape ID | Shape Type | User Label | Purpose |
|---|---|---|---|
| shape1 | start | Email content | Entry point |
| shape2 | catcherrors | - | Try/Catch wrapper |
| shape4 | decision | Attachment_Check | Check if email should have attachment |
| shape11 | message | Mail_Body | Build email body HTML (with attachment) |
| shape14 | documentproperties | set_MailBody | Store email body in property |
| shape15 | message | payload | Create attachment content |
| shape6 | documentproperties | set_Mail_Properties | Set mail properties (with attachment) |
| shape3 | connectoraction | Email | Send email with attachment |
| shape5 | stop | - | Success return (continue=true) |
| shape23 | message | Mail_Body | Build email body HTML (without attachment) |
| shape22 | documentproperties | set_MailBody | Store email body in property |
| shape20 | documentproperties | set_Mail_Properties | Set mail properties (without attachment) |
| shape7 | connectoraction | Email | Send email without attachment |
| shape9 | stop | - | Success return (continue=true) |
| shape10 | exception | - | Throw exception on error |

### Subprocess Control Flow

```
START (shape1)
  ↓
Try/Catch (shape2)
  ↓ [Try Path]
  Decision (shape4): DPP_HasAttachment equals "Y"?
    ↓ [TRUE - Has Attachment]
    shape11 (Build Email Body HTML)
      ↓
    shape14 (Store Email Body in process.DPP_MailBody)
      ↓
    shape15 (Create Attachment from process.DPP_Payload)
      ↓
    shape6 (Set Mail Properties: From, To, Subject, Body, FileName)
      ↓
    shape3 (Send Email with Attachment)
      ↓
    shape5 (Stop: continue=true) [SUCCESS RETURN]
    
    ↓ [FALSE - No Attachment]
    shape23 (Build Email Body HTML)
      ↓
    shape22 (Store Email Body in process.DPP_MailBody)
      ↓
    shape20 (Set Mail Properties: From, To, Subject, Body)
      ↓
    shape7 (Send Email without Attachment)
      ↓
    shape9 (Stop: continue=true) [SUCCESS RETURN]
  
  ↓ [Catch Path]
  shape10 (Exception: Throw error with meta.base.catcherrorsmessage) [ERROR RETURN]
```

### Return Paths

| Return Label | Return Type | Shape ID | Condition | Path |
|---|---|---|---|---|
| SUCCESS | Stop (continue=true) | shape5 | Email with attachment sent successfully | Try → TRUE → shape3 → shape5 |
| SUCCESS | Stop (continue=true) | shape9 | Email without attachment sent successfully | Try → FALSE → shape7 → shape9 |
| ERROR | Exception | shape10 | Any error in Try block | Catch → shape10 |

**Main Process Mapping:**
- Subprocess has NO explicit returnpaths configuration (Lines 335-336: returnpaths is empty)
- Subprocess uses Stop(continue=true) for success returns
- Subprocess uses Exception for error returns

### Properties Read by Subprocess (from Main Process)

| Property Name | Read By Shapes | Usage |
|---|---|---|---|
| process.DPP_HasAttachment | shape4 | Decision: Check if attachment needed |
| process.To_Email | shape6, shape20 | Mail To Address |
| process.DPP_Subject | shape6, shape20 | Mail Subject |
| process.DPP_MailBody | shape6, shape20 | Mail Body (after being set by shape14/shape22) |
| process.DPP_File_Name | shape6 | Mail File Name (attachment) |
| process.DPP_Process_Name | shape11, shape23 | Email body parameter {1} |
| process.DPP_AtomName | shape11, shape23 | Email body parameter {2} |
| process.DPP_ExecutionID | shape11, shape23 | Email body parameter {3} |
| process.DPP_ErrorMessage | shape11, shape23 | Email body parameter {4} |
| process.DPP_Payload | shape15 | Attachment content {1} |

### Properties Written by Subprocess

| Property Name | Written By Shape | Source |
|---|---|---|---|
| process.DPP_MailBody | shape14, shape22 | Current document (HTML email body) |

**Note:** process.DPP_MailBody is written and read within subprocess (internal property)

### Subprocess Execution Pattern

**Execution:** Synchronous (wait=true, abort=true)  
**Return Behavior:**
- Success: Stop(continue=true) - Subprocess completes, main process does NOT continue (no dragpoints from shape21)
- Error: Exception thrown - Propagates to main process

---

## 12. Execution Order (Step 9)

### Step 0: Business Logic Verification (MUST DO FIRST)

#### Business Logic Flow Analysis

**Operation 1: shape33 - Leave Oracle Fusion Create (HTTP POST)**

**Purpose:** Create leave/absence record in Oracle Fusion HCM system  
**What it does:** Sends leave request data to Oracle Fusion REST API  
**What it produces:**
- HTTP status code (meta.base.applicationstatuscode)
- HTTP response message (meta.base.applicationstatusmessage)
- HTTP response header Content-Encoding (dynamicdocument.DDP_RespHeader)
- Response body (JSON with personAbsenceEntryId if successful)

**Dependent Operations:**
- shape2 (Decision: HTTP Status 20 check) - Reads meta.base.applicationstatuscode
- shape44 (Decision: Check Response Content Type) - Reads dynamicdocument.DDP_RespHeader
- shape39, shape46 (Error message extraction) - Reads meta.base.applicationstatusmessage

**Business Flow:**
```
1. Receive leave request from D365 (shape1 START)
2. Capture execution context (shape38 - process name, execution ID, payload)
3. Transform D365 format to Oracle Fusion format (shape29 - map)
4. Set Oracle Fusion API URL (shape8)
5. Log request (shape49 - notify)
6. Call Oracle Fusion API to create leave (shape33 - HTTP POST)
7. Check if API call succeeded (shape2 - check status 20*)
   - If SUCCESS (20*): Transform response and return success
   - If FAILURE (not 20*): Check if response is compressed, extract error, return error
```

**Operation 2: shape21 - Office 365 Email Subprocess**

**Purpose:** Send error notification email to support team  
**What it does:** Sends email with error details and optional payload attachment  
**What it produces:** None (email sent, no data returned to main process)  
**Dependent Operations:** None (terminal operation)

**Business Flow:**
```
1. Receive error context from main process (properties)
2. Check if attachment needed (shape4 - decision)
3. Build email body with error details (shape11 or shape23)
4. Set mail properties (shape6 or shape20)
5. Send email (shape3 or shape7)
6. Complete (shape5 or shape9 - stop continue=true)
```

### Operations That MUST Execute First

1. **shape38 (Input_details) MUST execute FIRST**
   - Produces: All execution context properties required by subprocess
   - Reason: Subprocess reads these properties, so they must be written first

2. **shape33 (HTTP Operation) MUST execute BEFORE shape2**
   - Produces: meta.base.applicationstatuscode
   - Reason: shape2 decision checks this status code

3. **shape2 (Decision) MUST execute BEFORE shape44**
   - Reason: shape44 is in FALSE path of shape2

4. **shape19 (ErrorMsg) MUST execute BEFORE shape21**
   - Produces: process.DPP_ErrorMessage
   - Reason: Subprocess reads this property in email body

### Actual Business Flow

```
Main Process Business Flow:
1. START → Receive leave request from D365
2. Capture execution context (properties)
3. Try/Catch wrapper begins
4. [Try Path]:
   a. Transform request (D365 → Oracle Fusion format)
   b. Set Oracle Fusion API URL
   c. Log request
   d. Call Oracle Fusion API (HTTP POST)
   e. Check HTTP status code
      - If 20* (Success): Transform response → Return success
      - If NOT 20* (Error): Check response encoding → Extract error → Return error
5. [Catch Path]:
   a. Branch to handle error
      - Path 1: Extract error message → Send email notification (subprocess)
      - Path 2: Map error → Return error response
```

### Self-Check Results

- ✅ **Business logic verified FIRST:** YES
  - Documented operation purposes, outputs, and dependencies above

- ✅ **Operation analysis complete:** YES
  - shape33: Creates leave in Oracle Fusion, produces HTTP status/headers/response
  - shape21: Sends error notification email, no return data

- ✅ **Business logic execution order identified:** YES
  - shape38 MUST execute FIRST (produces properties)
  - shape33 MUST execute BEFORE shape2 (produces status code)
  - shape19 MUST execute BEFORE shape21 (produces error message)

- ✅ **Data dependencies checked FIRST:** YES
  - Verified in Step 4 (Data Dependency Graph)

- ✅ **Operation response analysis used:** YES
  - Referenced Step 1c for operation outputs

- ✅ **Decision analysis used:** YES
  - Referenced Step 7 for decision execution order

- ✅ **Dependency graph used:** YES
  - Referenced Step 4 for property dependencies

- ✅ **Branch analysis used:** YES
  - Referenced Step 8 for branch path execution order

- ✅ **Property dependency verification:** YES
  - All property reads happen after property writes:
    - shape38 writes properties → subprocess reads properties ✅
    - shape33 produces status → shape2 reads status ✅
    - shape19 writes DPP_ErrorMessage → shape21 reads DPP_ErrorMessage ✅

- ✅ **Topological sort applied:** YES
  - Branch shape20: Path 1 → Path 2 (sequential execution)

### Execution Order List

**Based on dependency graph (Step 4), control flow (Step 5), decision analysis (Step 7), and branch analysis (Step 8):**

```
1. shape1 (START) - Entry point
2. shape38 (Input_details) - WRITES all execution context properties
3. shape17 (Try/Catch) - Wrapper for error handling
4. [Try Path]:
   4a. shape29 (Map: Leave Create Map) - Transform D365 → Oracle Fusion format
   4b. shape8 (set URL) - WRITES dynamicdocument.URL
   4c. shape49 (Notify) - Log request
   4d. shape33 (HTTP Operation: Leave Oracle Fusion Create) - PRODUCES status/headers/response
   4e. shape2 (Decision: HTTP Status 20 check) - READS meta.base.applicationstatuscode
       [TRUE Path - Success]:
       4e1. shape34 (Map: Oracle Fusion Leave Response Map) - Transform response
       4e2. shape35 (Return Documents: Success Response) [TERMINAL]
       [FALSE Path - Error]:
       4e3. shape44 (Decision: Check Response Content Type) - READS dynamicdocument.DDP_RespHeader
            [TRUE Path - GZIP]:
            4e3a. shape45 (Decompress GZIP)
            4e3b. shape46 (error msg) - WRITES process.DPP_ErrorMessage
            4e3c. shape47 (Error Map) - READS process.DPP_ErrorMessage
            4e3d. shape48 (Return Documents: Error Response) [TERMINAL]
            [FALSE Path - Not GZIP]:
            4e3e. shape39 (error msg) - WRITES process.DPP_ErrorMessage
            4e3f. shape40 (Error Map) - READS process.DPP_ErrorMessage
            4e3g. shape36 (Return Documents: Error Response) [TERMINAL]
5. [Catch Path]:
   5a. shape20 (Branch) - 2 paths, SEQUENTIAL execution
       [Path 1]:
       5a1. shape19 (ErrorMsg) - WRITES process.DPP_ErrorMessage
       5a2. shape21 (Subprocess: Office 365 Email) - READS all properties, sends email [TERMINAL]
       [Path 2]:
       5a3. shape41 (Error Map) - READS process.DPP_ErrorMessage
       5a4. shape43 (Return Documents: Error Response) [TERMINAL]
```

### Dependency Verification

**Reference to Step 4 (Data Dependency Graph):**

1. **shape38 → subprocess (shape21)**
   - shape38 writes: process.DPP_Process_Name, process.DPP_AtomName, process.DPP_Payload, etc.
   - subprocess reads: All these properties
   - **Verified:** shape38 executes BEFORE subprocess ✅

2. **shape33 → shape2**
   - shape33 produces: meta.base.applicationstatuscode
   - shape2 reads: meta.base.applicationstatuscode
   - **Verified:** shape33 executes BEFORE shape2 ✅

3. **shape19 → shape21**
   - shape19 writes: process.DPP_ErrorMessage
   - shape21 reads: process.DPP_ErrorMessage
   - **Verified:** shape19 executes BEFORE shape21 ✅

4. **shape39 → shape40**
   - shape39 writes: process.DPP_ErrorMessage
   - shape40 reads: process.DPP_ErrorMessage
   - **Verified:** shape39 executes BEFORE shape40 ✅

5. **shape46 → shape47**
   - shape46 writes: process.DPP_ErrorMessage
   - shape47 reads: process.DPP_ErrorMessage
   - **Verified:** shape46 executes BEFORE shape47 ✅

### Branch Execution Order

**Reference to Step 8 (Branch Analysis):**

**Branch shape20:**
- Classification: SEQUENTIAL
- Execution Order: Path 1 (shape19 → shape21) → Path 2 (shape41 → shape43)
- Reasoning: Path 2 reads process.DPP_ErrorMessage which Path 1 writes

**Note:** However, upon closer inspection of the control flow, Path 1 and Path 2 actually terminate independently:
- Path 1 → shape19 → shape21 (subprocess call, no return)
- Path 2 → shape41 → shape43 (return documents)

**Correction:** These paths execute in parallel (no actual data dependency between them in execution, as shape21 is terminal and doesn't affect Path 2). The dependency graph shows Path 2 could read DPP_ErrorMessage, but since Path 1 terminates at subprocess call, they don't actually execute sequentially in practice.

**Revised Classification:** PARALLEL (both paths terminate independently, no actual execution dependency)

### Decision Path Tracing

**Decision shape2 (HTTP Status 20 check):**
- **TRUE Path:** shape34 → shape35 (Success Response) [TERMINAL]
- **FALSE Path:** shape44 → [shape45 → shape46 → shape47 → shape48] OR [shape39 → shape40 → shape36] (Error Response) [TERMINAL]

**Decision shape44 (Check Response Content Type):**
- **TRUE Path:** shape45 → shape46 → shape47 → shape48 (Error Response with GZIP decompression) [TERMINAL]
- **FALSE Path:** shape39 → shape40 → shape36 (Error Response without decompression) [TERMINAL]

**Convergence Points:** None - All paths terminate at Return Documents

---

## 13. Sequence Diagram (Step 10)

**📋 NOTE:** This diagram shows the technical execution flow. Detailed request/response JSON examples are documented in Section 16 (HTTP Status Codes and Return Path Responses) and Section 17 (Request/Response JSON Examples).

**References:**
- Based on dependency graph in Step 4 (Data Dependency Graph)
- Based on control flow graph in Step 5 (Control Flow Graph)
- Based on decision analysis in Step 7 (Decision Shape Analysis)
- Based on branch analysis in Step 8 (Branch Shape Analysis)
- Based on execution order in Step 9 (Execution Order)

```
START (shape1)
 |
 ├─→ shape38: Input_details (Document Properties)
 |   └─→ WRITES: [process.DPP_Process_Name, process.DPP_AtomName, process.DPP_Payload, 
 |                 process.DPP_ExecutionID, process.DPP_File_Name, process.DPP_Subject,
 |                 process.To_Email, process.DPP_HasAttachment, dynamicdocument.URL]
 |
 ├─→ shape17: Try/Catch Wrapper
 |   |
 |   ├─→ [TRY PATH]:
 |   |   |
 |   |   ├─→ shape29: Map (Leave Create Map)
 |   |   |   └─→ Transform: D365 format → Oracle Fusion format
 |   |   |   └─→ Field mappings: employeeNumber→personNumber, absenceStatusCode→absenceStatusCd, etc.
 |   |   |
 |   |   ├─→ shape8: set URL (Document Properties)
 |   |   |   └─→ WRITES: [dynamicdocument.URL = "hcmRestApi/resources/11.13.18.05/absences"]
 |   |   |
 |   |   ├─→ shape49: Notify (Log request)
 |   |   |   └─→ INFO: Log current document
 |   |   |
 |   |   ├─→ shape33: Leave Oracle Fusion Create (Downstream HTTP POST)
 |   |   |   └─→ READS: [dynamicdocument.URL, current document (transformed request)]
 |   |   |   └─→ WRITES: [meta.base.applicationstatuscode, meta.base.applicationstatusmessage,
 |   |   |                 dynamicdocument.DDP_RespHeader, response body]
 |   |   |   └─→ HTTP: POST to Oracle Fusion HCM API
 |   |   |   └─→ Endpoint: https://iaaxey-dev3.fa.ocs.oraclecloud.com:443/hcmRestApi/resources/11.13.18.05/absences
 |   |   |   └─→ Expected: [200, 201], Error: [400, 401, 404, 500]
 |   |   |
 |   |   ├─→ shape2: Decision - HTTP Status 20 check
 |   |   |   └─→ READS: [meta.base.applicationstatuscode]
 |   |   |   └─→ Condition: meta.base.applicationstatuscode wildcard matches "20*"?
 |   |   |   |
 |   |   |   ├─→ IF TRUE (Status 20* - Success):
 |   |   |   |   |
 |   |   |   |   ├─→ shape34: Map (Oracle Fusion Leave Response Map)
 |   |   |   |   |   └─→ Transform: Oracle Fusion response → D365 response format
 |   |   |   |   |   └─→ Extract: personAbsenceEntryId
 |   |   |   |   |   └─→ Set defaults: status="success", message="Data successfully sent to Oracle Fusion", success="true"
 |   |   |   |   |
 |   |   |   |   └─→ shape35: Return Documents [HTTP: 200] [SUCCESS]
 |   |   |   |       └─→ Response: {"leaveResponse": {"status": "success", "message": "...", "personAbsenceEntryId": 12345, "success": "true"}}
 |   |   |   |
 |   |   |   └─→ IF FALSE (Status NOT 20* - Error):
 |   |   |       |
 |   |   |       ├─→ shape44: Decision - Check Response Content Type
 |   |   |           └─→ READS: [dynamicdocument.DDP_RespHeader]
 |   |   |           └─→ Condition: dynamicdocument.DDP_RespHeader equals "gzip"?
 |   |   |           |
 |   |   |           ├─→ IF TRUE (GZIP compressed):
 |   |   |           |   |
 |   |   |           |   ├─→ shape45: Data Process (Decompress GZIP)
 |   |   |           |   |   └─→ Groovy script: Decompress GZIP response
 |   |   |           |   |
 |   |   |           |   ├─→ shape46: error msg (Document Properties)
 |   |   |           |   |   └─→ WRITES: [process.DPP_ErrorMessage]
 |   |   |           |   |   └─→ READS: [meta.base.applicationstatusmessage]
 |   |   |           |   |
 |   |   |           |   ├─→ shape47: Map (Leave Error Map)
 |   |   |           |   |   └─→ READS: [process.DPP_ErrorMessage]
 |   |   |           |   |   └─→ Set: status="failure", success="false", message=DPP_ErrorMessage
 |   |   |           |   |
 |   |   |           |   └─→ shape48: Return Documents [HTTP: 400/500] [ERROR] [EARLY EXIT]
 |   |   |           |       └─→ Response: {"leaveResponse": {"status": "failure", "message": "...", "success": "false"}}
 |   |   |           |
 |   |   |           └─→ IF FALSE (Not GZIP):
 |   |   |               |
 |   |   |               ├─→ shape39: error msg (Document Properties)
 |   |   |               |   └─→ WRITES: [process.DPP_ErrorMessage]
 |   |   |               |   └─→ READS: [meta.base.applicationstatusmessage]
 |   |   |               |
 |   |   |               ├─→ shape40: Map (Leave Error Map)
 |   |   |               |   └─→ READS: [process.DPP_ErrorMessage]
 |   |   |               |   └─→ Set: status="failure", success="false", message=DPP_ErrorMessage
 |   |   |               |
 |   |   |               └─→ shape36: Return Documents [HTTP: 400/500] [ERROR] [EARLY EXIT]
 |   |   |                   └─→ Response: {"leaveResponse": {"status": "failure", "message": "...", "success": "false"}}
 |   |
 |   └─→ [CATCH PATH]:
 |       |
 |       ├─→ shape20: Branch (2 paths - PARALLEL execution)
 |           |
 |           ├─→ [Path 1]:
 |           |   |
 |           |   ├─→ shape19: ErrorMsg (Document Properties)
 |           |   |   └─→ WRITES: [process.DPP_ErrorMessage]
 |           |   |   └─→ READS: [meta.base.catcherrorsmessage]
 |           |   |
 |           |   └─→ shape21: ProcessCall - Office 365 Email Subprocess
 |           |       └─→ READS: [process.DPP_ErrorMessage, process.DPP_Process_Name, process.DPP_AtomName,
 |           |                   process.DPP_ExecutionID, process.DPP_File_Name, process.DPP_Subject,
 |           |                   process.To_Email, process.DPP_HasAttachment, process.DPP_Payload]
 |           |       |
 |           |       └─→ SUBPROCESS INTERNAL FLOW:
 |           |           |
 |           |           ├─→ START (shape1)
 |           |           |
 |           |           ├─→ Try/Catch (shape2)
 |           |           |   |
 |           |           |   ├─→ [Try Path]:
 |           |           |   |   |
 |           |           |   |   ├─→ shape4: Decision - Attachment_Check
 |           |           |   |   |   └─→ READS: [process.DPP_HasAttachment]
 |           |           |   |   |   └─→ Condition: process.DPP_HasAttachment equals "Y"?
 |           |           |   |   |   |
 |           |           |   |   |   ├─→ IF TRUE (Has Attachment):
 |           |           |   |   |   |   |
 |           |           |   |   |   |   ├─→ shape11: Message (Build Email Body HTML)
 |           |           |   |   |   |   |   └─→ READS: [process.DPP_Process_Name, process.DPP_AtomName,
 |           |           |   |   |   |   |               process.DPP_ExecutionID, process.DPP_ErrorMessage]
 |           |           |   |   |   |   |
 |           |           |   |   |   |   ├─→ shape14: set_MailBody (Document Properties)
 |           |           |   |   |   |   |   └─→ WRITES: [process.DPP_MailBody]
 |           |           |   |   |   |   |
 |           |           |   |   |   |   ├─→ shape15: Message (Create Attachment)
 |           |           |   |   |   |   |   └─→ READS: [process.DPP_Payload]
 |           |           |   |   |   |   |
 |           |           |   |   |   |   ├─→ shape6: set_Mail_Properties (Document Properties)
 |           |           |   |   |   |   |   └─→ WRITES: [connector.mail.fromAddress, connector.mail.toAddress,
 |           |           |   |   |   |   |                 connector.mail.subject, connector.mail.body, connector.mail.filename]
 |           |           |   |   |   |   |   └─→ READS: [process.To_Email, process.DPP_Subject, process.DPP_MailBody, process.DPP_File_Name]
 |           |           |   |   |   |   |
 |           |           |   |   |   |   ├─→ shape3: Email (Send Email with Attachment)
 |           |           |   |   |   |   |   └─→ Operation: af07502a-fafd-4976-a691-45d51a33b549
 |           |           |   |   |   |   |
 |           |           |   |   |   |   └─→ shape5: Stop (continue=true) [SUCCESS RETURN]
 |           |           |   |   |   |
 |           |           |   |   |   └─→ IF FALSE (No Attachment):
 |           |           |   |   |       |
 |           |           |   |   |       ├─→ shape23: Message (Build Email Body HTML)
 |           |           |   |   |       |   └─→ READS: [process.DPP_Process_Name, process.DPP_AtomName,
 |           |           |   |   |       |               process.DPP_ExecutionID, process.DPP_ErrorMessage]
 |           |           |   |   |       |
 |           |           |   |   |       ├─→ shape22: set_MailBody (Document Properties)
 |           |           |   |   |       |   └─→ WRITES: [process.DPP_MailBody]
 |           |           |   |   |       |
 |           |           |   |   |       ├─→ shape20: set_Mail_Properties (Document Properties)
 |           |           |   |   |       |   └─→ WRITES: [connector.mail.fromAddress, connector.mail.toAddress,
 |           |           |   |   |       |                 connector.mail.subject, connector.mail.body]
 |           |           |   |   |       |   └─→ READS: [process.To_Email, process.DPP_Subject, process.DPP_MailBody]
 |           |           |   |   |       |
 |           |           |   |   |       ├─→ shape7: Email (Send Email without Attachment)
 |           |           |   |   |       |   └─→ Operation: 15a72a21-9b57-49a1-a8ed-d70367146644
 |           |           |   |   |       |
 |           |           |   |   |       └─→ shape9: Stop (continue=true) [SUCCESS RETURN]
 |           |           |   |
 |           |           |   └─→ [Catch Path]:
 |           |           |       |
 |           |           |       └─→ shape10: Exception (Throw error)
 |           |           |           └─→ READS: [meta.base.catcherrorsmessage]
 |           |           |           └─→ [ERROR RETURN]
 |           |           |
 |           |           └─→ END SUBPROCESS
 |           |
 |           └─→ [Path 2]:
 |               |
 |               ├─→ shape41: Map (Leave Error Map)
 |               |   └─→ READS: [process.DPP_ErrorMessage]
 |               |   └─→ Set: status="failure", success="false", message=DPP_ErrorMessage
 |               |
 |               └─→ shape43: Return Documents [HTTP: 400/500] [ERROR] [EARLY EXIT]
 |                   └─→ Response: {"leaveResponse": {"status": "failure", "message": "...", "success": "false"}}
 |
 └─→ END
```

**Note:** Detailed request/response JSON examples for all operations and return paths are documented in Section 16 (HTTP Status Codes and Return Path Responses) and Section 17 (Request/Response JSON Examples).

---

## 14. HTTP Status Codes and Return Path Responses (Step 1e)

### Return Path Analysis

#### Return Path 1: Success Response (shape35)

**Return Label:** "Success Response"  
**Return Shape ID:** shape35  
**HTTP Status Code:** 200  
**Decision Conditions Leading to Return:**
- shape2 (HTTP Status 20 check): meta.base.applicationstatuscode matches "20*" → TRUE path

**Populated Response Fields:**

| Field Name | Field Path | Source | Populated By | Value Origin |
|---|---|---|---|---|
| status | leaveResponse/Object/status | static (map default) | shape34 (Map) | Default value: "success" |
| message | leaveResponse/Object/message | static (map default) | shape34 (Map) | Default value: "Data successfully sent to Oracle Fusion" |
| personAbsenceEntryId | leaveResponse/Object/personAbsenceEntryId | operation_response | shape33 (HTTP Operation) | Oracle Fusion API response field |
| success | leaveResponse/Object/success | static (map default) | shape34 (Map) | Default value: "true" |

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

#### Return Path 2: Error Response - Not GZIP (shape36)

**Return Label:** "Error Response"  
**Return Shape ID:** shape36  
**HTTP Status Code:** 400 (client error) or 500 (server error)  
**Decision Conditions Leading to Return:**
- shape2 (HTTP Status 20 check): meta.base.applicationstatuscode does NOT match "20*" → FALSE path
- shape44 (Check Response Content Type): dynamicdocument.DDP_RespHeader does NOT equal "gzip" → FALSE path

**Populated Response Fields:**

| Field Name | Field Path | Source | Populated By | Value Origin |
|---|---|---|---|---|
| status | leaveResponse/Object/status | static (map default) | shape40 (Map) | Default value: "failure" |
| message | leaveResponse/Object/message | process_property | shape39 → shape40 (Map) | process.DPP_ErrorMessage (from meta.base.applicationstatusmessage) |
| success | leaveResponse/Object/success | static (map default) | shape40 (Map) | Default value: "false" |

**Response JSON Example:**

```json
{
  "leaveResponse": {
    "status": "failure",
    "message": "400 Bad Request - Invalid absence type",
    "success": "false"
  }
}
```

#### Return Path 3: Error Response - GZIP (shape48)

**Return Label:** "Error Response"  
**Return Shape ID:** shape48  
**HTTP Status Code:** 400 (client error) or 500 (server error)  
**Decision Conditions Leading to Return:**
- shape2 (HTTP Status 20 check): meta.base.applicationstatuscode does NOT match "20*" → FALSE path
- shape44 (Check Response Content Type): dynamicdocument.DDP_RespHeader equals "gzip" → TRUE path

**Populated Response Fields:**

| Field Name | Field Path | Source | Populated By | Value Origin |
|---|---|---|---|---|
| status | leaveResponse/Object/status | static (map default) | shape47 (Map) | Default value: "failure" |
| message | leaveResponse/Object/message | process_property | shape46 → shape47 (Map) | process.DPP_ErrorMessage (from meta.base.applicationstatusmessage, after GZIP decompression) |
| success | leaveResponse/Object/success | static (map default) | shape47 (Map) | Default value: "false" |

**Response JSON Example:**

```json
{
  "leaveResponse": {
    "status": "failure",
    "message": "500 Internal Server Error - Service unavailable",
    "success": "false"
  }
}
```

#### Return Path 4: Error Response - Catch Path (shape43)

**Return Label:** "Error Response"  
**Return Shape ID:** shape43  
**HTTP Status Code:** 500 (internal server error)  
**Decision Conditions Leading to Return:**
- shape17 (Try/Catch): Exception caught in Try block → Catch path
- shape20 (Branch): Path 2 executed

**Populated Response Fields:**

| Field Name | Field Path | Source | Populated By | Value Origin |
|---|---|---|---|---|
| status | leaveResponse/Object/status | static (map default) | shape41 (Map) | Default value: "failure" |
| message | leaveResponse/Object/message | process_property | shape19 → shape41 (Map) | process.DPP_ErrorMessage (from meta.base.catcherrorsmessage) |
| success | leaveResponse/Object/success | static (map default) | shape41 (Map) | Default value: "false" |

**Response JSON Example:**

```json
{
  "leaveResponse": {
    "status": "failure",
    "message": "Error in Map: Leave Create Map - Invalid field mapping",
    "success": "false"
  }
}
```

**Note:** Catch path also triggers subprocess email notification (shape21) in parallel with error response return.

### Downstream Operations HTTP Status Codes

#### Operation: Leave Oracle Fusion Create (shape33)

**Operation ID:** 6e8920fd-af5a-430b-a1d9-9fde7ac29a12  
**Operation Name:** Leave Oracle Fusion Create  
**Type:** HTTP POST  
**Endpoint:** https://iaaxey-dev3.fa.ocs.oraclecloud.com:443/hcmRestApi/resources/11.13.18.05/absences

**Expected Success Codes:** 200, 201  
**Error Codes:** 400, 401, 404, 500

**Error Handling Strategy:**
- **returnErrors:** true (return error responses to process)
- **returnResponses:** true (return success responses to process)
- **Handling:** Check status code with decision shape2, route to success or error path

**HTTP Status Code Routing:**
- **20* (200, 201, etc.):** Success path → Transform response → Return success
- **4xx/5xx:** Error path → Check if GZIP → Extract error → Return error

#### Operation: Email w Attachment (shape3 - Subprocess)

**Operation ID:** af07502a-fafd-4976-a691-45d51a33b549  
**Operation Name:** Email w Attachment  
**Type:** Mail Send

**Expected Success Codes:** N/A (Mail operation)  
**Error Codes:** N/A  
**Error Handling Strategy:** Exception thrown on error (caught by subprocess Try/Catch)

#### Operation: Email W/O Attachment (shape7 - Subprocess)

**Operation ID:** 15a72a21-9b57-49a1-a8ed-d70367146644  
**Operation Name:** Email W/O Attachment  
**Type:** Mail Send

**Expected Success Codes:** N/A (Mail operation)  
**Error Codes:** N/A  
**Error Handling Strategy:** Exception thrown on error (caught by subprocess Try/Catch)

---

## 15. Request/Response JSON Examples

### Process Layer Entry Point

**Entry Operation:** Create Leave Oracle Fusion OP (8f709c2b-e63f-4d5f-9374-2932ed70415d)  
**Method:** Web Service Server Listen (REST API endpoint)  
**Content Type:** application/json

#### Request JSON Example (D365 Format)

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

**Error Response - HTTP Error (HTTP 400/500):**

```json
{
  "leaveResponse": {
    "status": "failure",
    "message": "400 Bad Request - Invalid absence type",
    "success": "false"
  }
}
```

**Error Response - Catch Exception (HTTP 500):**

```json
{
  "leaveResponse": {
    "status": "failure",
    "message": "Error in Map: Leave Create Map - Invalid field mapping",
    "success": "false"
  }
}
```

### Downstream System Layer Calls

#### Operation: Leave Oracle Fusion Create (shape33)

**Operation ID:** 6e8920fd-af5a-430b-a1d9-9fde7ac29a12  
**Method:** POST  
**Endpoint:** https://iaaxey-dev3.fa.ocs.oraclecloud.com:443/hcmRestApi/resources/11.13.18.05/absences  
**Content Type:** application/json  
**Authentication:** Basic Auth (INTEGRATION.USER@al-ghurair.com)

**Request JSON (Oracle Fusion Format):**

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
  "absenceCaseId": "ABS-12345",
  "absenceEntryBasicFlag": true,
  "absencePatternCd": "PATTERN1",
  "absenceStatusCd": "SUBMITTED",
  "absenceTypeId": 300000001234567,
  "absenceTypeReasonId": "300000001234568",
  "agreementId": "300000001234569",
  "approvalStatusCd": "APPROVED",
  "authStatusUpdateDate": "2024-03-24T10:30:00Z",
  "bandDtlId": "300000001234570",
  "blockedLeaveCandidate": "N",
  "certificationAuthFlag": "N",
  "childEventTypeCd": null,
  "comments": "Sick leave request",
  "conditionStartDate": "2024-03-24",
  "confirmedDate": "2024-03-24T10:30:00Z",
  "consumedByAgreement": "N",
  "createdBy": "INTEGRATION.USER",
  "creationDate": "2024-03-24T10:30:00Z",
  "diseaseCode": null,
  "duration": 2,
  "employeeShiftFlag": false,
  "endDate": "2024-03-25",
  "endDateDuration": 1,
  "endDateTime": "2024-03-25T23:59:59Z",
  "endTime": "23:59:59",
  "establishmentDate": "2024-03-24",
  "frequency": "ONCE",
  "initialReportById": "300000001234571",
  "initialTimelyNotifyFlag": "Y",
  "lastUpdateDate": "2024-03-24T10:30:00Z",
  "lastUpdateLogin": "300000001234572",
  "lastUpdatedBy": "INTEGRATION.USER",
  "lateNotifyFlag": "N",
  "legalEntityId": 300000001234573,
  "legislationCode": "AE",
  "legislativeDataGroupId": 300000001234574,
  "notificationDate": "2024-03-24",
  "objectVersionNumber": 1,
  "openEndedFlag": false,
  "overridden": "N",
  "personAbsenceEntryId": 300000123456789,
  "periodOfIncapToWorkFlag": "N",
  "periodOfServiceId": 300000001234575,
  "personId": 300000001234576,
  "plannedEndDate": "2024-03-25",
  "processingStatus": "COMPLETE",
  "projectId": null,
  "singleDayFlag": false,
  "source": "D365",
  "splCondition": null,
  "startDate": "2024-03-24",
  "startDateDuration": 1,
  "startDateTime": "2024-03-24T00:00:00Z",
  "startTime": "00:00:00",
  "submittedDate": "2024-03-24T10:30:00Z",
  "timelinessOverrideDate": null,
  "unitOfMeasure": "D",
  "userMode": "EMPLOYEE",
  "personNumber": "9000604",
  "absenceType": "Sick Leave",
  "employer": "Al Ghurair Investment LLC",
  "absenceReason": null,
  "absenceDispStatus": "SUBMITTED",
  "assignmentId": "300000001234577",
  "dataSecurityPersonId": 300000001234576,
  "effectiveEndDate": null,
  "effectiveStartDate": "2024-03-24",
  "ObjectVersionNumber": 1,
  "agreementName": "Standard Agreement",
  "paymentDetail": null,
  "assignmentName": "E9000604 Assignment",
  "assignmentNumber": "E9000604",
  "unitOfMeasureMeaning": "Days",
  "formattedDuration": "2 Days",
  "absenceDispStatusMeaning": "Submitted",
  "absenceUpdatableFlag": "Y",
  "ApprovalDatetime": "2024-03-24T10:30:00Z",
  "allowAssignmentSelectionFlag": false,
  "links": [
    {
      "rel": "self",
      "href": "https://iaaxey-dev3.fa.ocs.oraclecloud.com:443/hcmRestApi/resources/11.13.18.05/absences/300000123456789",
      "name": "absences",
      "kind": "item",
      "properties": {
        "changeIndicator": "ACED0005737200136A6176612E7574696C2E41727261794C6973747881D21D99C7619D03000149000473697A65787000000001770400000001737200116A6176612E6C616E672E496E746567657212E2A0A4F781873802000149000576616C7565787200106A6176612E6C616E672E4E756D62657286AC951D0B94E08B02000078700000000178"
      }
    }
  ]
}
```

**Response JSON (Error - HTTP 400):**

```json
{
  "title": "Bad Request",
  "status": 400,
  "detail": "Invalid absence type provided",
  "o:errorCode": "VALIDATION_ERROR",
  "o:errorPath": "absenceType"
}
```

**Response JSON (Error - HTTP 401):**

```json
{
  "title": "Unauthorized",
  "status": 401,
  "detail": "Authentication failed"
}
```

**Response JSON (Error - HTTP 500):**

```json
{
  "title": "Internal Server Error",
  "status": 500,
  "detail": "An unexpected error occurred while processing the request"
}
```

#### Return Path 3: Error Response - GZIP (shape48)

**Return Label:** "Error Response"  
**Return Shape ID:** shape48  
**HTTP Status Code:** 400 (client error) or 500 (server error)  
**Decision Conditions Leading to Return:**
- shape2 (HTTP Status 20 check): meta.base.applicationstatuscode does NOT match "20*" → FALSE path
- shape44 (Check Response Content Type): dynamicdocument.DDP_RespHeader equals "gzip" → TRUE path

**Populated Response Fields:**

| Field Name | Field Path | Source | Populated By | Value Origin |
|---|---|---|---|---|
| status | leaveResponse/Object/status | static (map default) | shape47 (Map) | Default value: "failure" |
| message | leaveResponse/Object/message | process_property | shape46 → shape47 (Map) | process.DPP_ErrorMessage (from meta.base.applicationstatusmessage, after GZIP decompression) |
| success | leaveResponse/Object/success | static (map default) | shape47 (Map) | Default value: "false" |

**Response JSON Example:**

```json
{
  "leaveResponse": {
    "status": "failure",
    "message": "400 Bad Request - Invalid absence type (decompressed from GZIP)",
    "success": "false"
  }
}
```

#### Return Path 5: Error Response - Catch Exception (shape43)

**Return Label:** "Error Response"  
**Return Shape ID:** shape43  
**HTTP Status Code:** 500 (internal server error)  
**Decision Conditions Leading to Return:**
- shape17 (Try/Catch): Exception caught in Try block → Catch path
- shape20 (Branch): Path 2 executed

**Populated Response Fields:**

| Field Name | Field Path | Source | Populated By | Value Origin |
|---|---|---|---|---|
| status | leaveResponse/Object/status | static (map default) | shape41 (Map) | Default value: "failure" |
| message | leaveResponse/Object/message | process_property | shape19 → shape41 (Map) | process.DPP_ErrorMessage (from meta.base.catcherrorsmessage) |
| success | leaveResponse/Object/success | static (map default) | shape41 (Map) | Default value: "false" |

**Response JSON Example:**

```json
{
  "leaveResponse": {
    "status": "failure",
    "message": "Error in Map: Leave Create Map - Invalid field mapping",
    "success": "false"
  }
}
```

**Note:** This path also triggers subprocess email notification (shape21 - Path 1 of branch), but subprocess does not return data to main process.

### HTTP Status Code Summary

| Return Path | Shape ID | HTTP Status | Success/Error | Trigger Condition |
|---|---|---|---|---|
| Success Response | shape35 | 200 | Success | HTTP status 20* |
| Error Response (Not GZIP) | shape36 | 400/500 | Error | HTTP status NOT 20* AND Content-Encoding NOT gzip |
| Error Response (GZIP) | shape48 | 400/500 | Error | HTTP status NOT 20* AND Content-Encoding is gzip |
| Error Response (Catch) | shape43 | 500 | Error | Exception in Try block |

---

## 16. Field Mapping Analysis

### Request Field Mapping (D365 → Oracle Fusion)

**Source Profile:** febfa3e1-f719-4ee8-ba57-cdae34137ab3 (D365 Leave Create JSON Profile)  
**Target Profile:** a94fa205-c740-40a5-9fda-3d018611135a (HCM Leave Create JSON Profile)  
**Map:** c426b4d6-2aff-450e-b43b-59956c4dbc96 (Leave Create Map)

| Boomi Field Path (D365) | Boomi Field Name | Data Type | Required | Azure DTO Property | Oracle Fusion Field | Notes |
|---|---|---|---|---|---|---|
| Root/Object/employeeNumber | employeeNumber | number | Yes | EmployeeNumber | personNumber | Field name changed |
| Root/Object/absenceType | absenceType | character | Yes | AbsenceType | absenceType | Direct mapping |
| Root/Object/employer | employer | character | Yes | Employer | employer | Direct mapping |
| Root/Object/startDate | startDate | character | Yes | StartDate | startDate | Direct mapping |
| Root/Object/endDate | endDate | character | Yes | EndDate | endDate | Direct mapping |
| Root/Object/absenceStatusCode | absenceStatusCode | character | Yes | AbsenceStatusCode | absenceStatusCd | Field name abbreviated |
| Root/Object/approvalStatusCode | approvalStatusCode | character | Yes | ApprovalStatusCode | approvalStatusCd | Field name abbreviated |
| Root/Object/startDateDuration | startDateDuration | number | Yes | StartDateDuration | startDateDuration | Direct mapping |
| Root/Object/endDateDuration | endDateDuration | number | Yes | EndDateDuration | endDateDuration | Direct mapping |

### Response Field Mapping (Oracle Fusion → D365)

**Source Profile:** 316175c7-0e45-4869-9ac6-5f9d69882a62 (Oracle Fusion Leave Response JSON Profile)  
**Target Profile:** f4ca3a70-114a-4601-bad8-44a3eb20e2c0 (Leave D365 Response)  
**Map:** e4fd3f59-edb5-43a1-aeae-143b600a064e (Oracle Fusion Leave Response Map)

| Boomi Field Path (Oracle Fusion) | Boomi Field Name | Data Type | Azure DTO Property | D365 Response Field | Notes |
|---|---|---|---|---|---|
| Root/Object/personAbsenceEntryId | personAbsenceEntryId | number | PersonAbsenceEntryId | personAbsenceEntryId | Direct mapping |
| - | - | - | Status | status | Static value: "success" |
| - | - | - | Message | message | Static value: "Data successfully sent to Oracle Fusion" |
| - | - | - | Success | success | Static value: "true" |

### Error Response Field Mapping

**Source Profile:** 23d7a2e9-5cb0-4e9c-9e4b-3154834bad0d (Dummy FF Profile)  
**Target Profile:** f4ca3a70-114a-4601-bad8-44a3eb20e2c0 (Leave D365 Response)  
**Map:** f46b845a-7d75-41b5-b0ad-c41a6a8e9b12 (Leave Error Map)

| Source | Source Type | Azure DTO Property | D365 Response Field | Notes |
|---|---|---|---|---|
| process.DPP_ErrorMessage | Process Property | Message | message | Error message from exception or HTTP error |
| - | Static | Status | status | Static value: "failure" |
| - | Static | Success | success | Static value: "false" |

---

## 17. Critical Patterns Identified

### Pattern 1: Try/Catch Error Handling

**Location:** shape17 (main process)  
**Pattern:** Wrap main business logic in Try/Catch, route errors to notification and error response

**Implementation:**
```
Try/Catch (shape17)
  ├─→ [Try Path]: Execute business logic (map, HTTP call, decision, response)
  └─→ [Catch Path]: Handle errors (branch to email notification and error response)
```

**Business Rule:** Any exception in Try block triggers:
1. Email notification to support team (with payload attachment)
2. Error response to caller

### Pattern 2: HTTP Status Code Routing

**Location:** shape2 (Decision: HTTP Status 20 check)  
**Pattern:** Check HTTP status code after API call, route to success or error path

**Implementation:**
```
HTTP Operation (shape33)
  ↓ Produces meta.base.applicationstatuscode
Decision (shape2): Check if status is 20*
  ├─→ TRUE (20*): Success path → Transform response → Return success
  └─→ FALSE (not 20*): Error path → Extract error → Return error
```

**Business Rule:** Only 20x status codes are considered successful, all others are errors

### Pattern 3: GZIP Response Handling

**Location:** shape44 (Decision: Check Response Content Type)  
**Pattern:** Check if HTTP response is GZIP compressed, decompress if needed before processing

**Implementation:**
```
Decision (shape44): Check if Content-Encoding is "gzip"
  ├─→ TRUE (GZIP): Decompress (shape45) → Extract error → Return error
  └─→ FALSE (Not GZIP): Extract error directly → Return error
```

**Business Rule:** Oracle Fusion API may return GZIP compressed error responses, must decompress before extracting error message

### Pattern 4: Dual Error Notification

**Location:** shape20 (Branch in Catch path)  
**Pattern:** On exception, both send email notification AND return error response

**Implementation:**
```
Branch (shape20) - 2 paths execute in parallel:
  ├─→ Path 1: Extract error → Send email notification (subprocess)
  └─→ Path 2: Map error → Return error response
```

**Business Rule:** Errors trigger both user notification (email) and system response (API error response)

### Pattern 5: Subprocess Email Notification

**Location:** shape21 (ProcessCall to Office 365 Email subprocess)  
**Pattern:** Reusable email notification subprocess with attachment support

**Implementation:**
```
Subprocess: Office 365 Email
  ├─→ Decision: Check if attachment needed (DPP_HasAttachment = "Y")
  ├─→ TRUE: Build email with attachment → Send
  └─→ FALSE: Build email without attachment → Send
```

**Business Rule:** Email notifications include execution details (process name, atom, execution ID, error message) and optional payload attachment

---

## 18. System Layer Identification

### Downstream Systems

#### System 1: Oracle Fusion HCM

**System Type:** Oracle Fusion Cloud HCM (Human Capital Management)  
**Connection:** aa1fcb29-d146-4425-9ea6-b9698090f60e (Oracle Fusion)  
**Base URL:** https://iaaxey-dev3.fa.ocs.oraclecloud.com:443  
**Authentication:** Basic Auth (INTEGRATION.USER@al-ghurair.com)  
**API Type:** REST API

**Operations:**
- **Leave Oracle Fusion Create** (6e8920fd-af5a-430b-a1d9-9fde7ac29a12)
  - Method: POST
  - Endpoint: /hcmRestApi/resources/11.13.18.05/absences
  - Purpose: Create absence/leave record in Oracle Fusion HCM

**System Layer Classification:** System API (unlocks data from Oracle Fusion HCM system of record)

#### System 2: Office 365 Email (SMTP)

**System Type:** Microsoft Office 365 Email Service  
**Connection:** 00eae79b-2303-4215-8067-dcc299e42697 (Office 365 Email)  
**SMTP Host:** smtp-mail.outlook.com  
**SMTP Port:** 587  
**Authentication:** SMTP AUTH with TLS (Boomi.Dev.failures@al-ghurair.com)  
**API Type:** SMTP

**Operations:**
- **Email w Attachment** (af07502a-fafd-4976-a691-45d51a33b549)
  - Purpose: Send email with attachment
- **Email W/O Attachment** (15a72a21-9b57-49a1-a8ed-d70367146644)
  - Purpose: Send email without attachment

**System Layer Classification:** Notification Service (not a System API, but a support service)

### Process Layer Classification

**Process Name:** HCM_Leave Create  
**Layer:** Process Layer API  
**Business Domain:** Human Resource (HCM)  
**Purpose:** Orchestrate leave creation from D365 to Oracle Fusion HCM

**Responsibilities:**
1. Accept leave request from D365 (Experience Layer or another Process Layer)
2. Transform D365 format to Oracle Fusion format
3. Call Oracle Fusion HCM System API to create leave
4. Handle success/error responses
5. Return standardized response to caller
6. Send error notifications to support team

**API Led Architecture Classification:**
- **Layer:** Process Layer
- **Orchestrates:** Oracle Fusion HCM System API
- **Business Logic:** Leave creation orchestration, error handling, notification
- **Data Transformation:** D365 format ↔ Oracle Fusion format

---

## 19. Validation Checklist

### Data Dependencies
- ✅ All property WRITES identified (Step 2)
- ✅ All property READS identified (Step 3)
- ✅ Dependency graph built (Step 4)
- ✅ Execution order satisfies all dependencies (no read-before-write) (Step 9)

### Decision Analysis
- ✅ ALL decision shapes inventoried (2 decisions: shape2, shape44)
- ✅ BOTH TRUE and FALSE paths traced to termination (Step 7)
- ✅ Pattern type identified for each decision (Error Check, Conditional Processing)
- ✅ Early exits identified and documented (All paths terminate at Return Documents)
- ✅ Convergence points identified (None - all paths terminate separately)
- ✅ Decision data source analysis complete (Step 7)
- ✅ Decision type classification complete (Step 7)
- ✅ Actual execution order verified (Step 7)

### Branch Analysis
- ✅ Each branch classified as parallel or sequential (shape20: SEQUENTIAL → PARALLEL correction)
- ✅ API call detection performed (No API calls in branch paths)
- ✅ Dependency graph built for branch paths (Step 8)
- ✅ Topological sort applied (Step 8)
- ✅ Path termination traced (Step 8)
- ✅ Convergence points identified (None)
- ✅ Execution continuation point determined (None - paths terminate)

### Sequence Diagram
- ✅ Format follows required structure (Operation → Decision → Operation)
- ✅ Each operation shows READS and WRITES
- ✅ Decisions show both TRUE and FALSE paths
- ✅ Early exits marked [EARLY EXIT]
- ✅ Subprocess internal flows documented
- ✅ Sequence diagram references all prior steps (Steps 4, 5, 7, 8, 9)

### Subprocess Analysis
- ✅ ALL subprocesses analyzed (1 subprocess: Office 365 Email)
- ✅ Internal flow traced (Step 7a)
- ✅ Return paths identified (Success: Stop continue=true, Error: Exception)
- ✅ Properties written by subprocess documented (process.DPP_MailBody - internal)
- ✅ Properties read by subprocess from main process documented (Step 7a)

### Property Extraction Completeness
- ✅ All property patterns searched (${}, %%, {})
- ✅ Message parameters checked for process properties
- ✅ Operation headers/path parameters checked
- ✅ Decision track properties identified (meta.base.applicationstatuscode, dynamicdocument.DDP_RespHeader)
- ✅ Document properties that read other properties identified

### Input/Output Structure Analysis (CONTRACT VERIFICATION)
- ✅ Entry point operation identified (8f709c2b-e63f-4d5f-9374-2932ed70415d)
- ✅ Request profile identified and loaded (febfa3e1-f719-4ee8-ba57-cdae34137ab3)
- ✅ Request profile structure analyzed (JSON)
- ✅ Array vs single object detected (Single object)
- ✅ Array cardinality documented (N/A - not array)
- ✅ ALL request fields extracted (9 fields)
- ✅ Request field paths documented (Step 1a)
- ✅ Request field mapping table generated (Section 16)
- ✅ Response profile identified and loaded (f4ca3a70-114a-4601-bad8-44a3eb20e2c0)
- ✅ Response profile structure analyzed (JSON)
- ✅ ALL response fields extracted (4 fields)
- ✅ Response field mapping table generated (Section 16)
- ✅ Document processing behavior determined (Single document processing)
- ✅ Input/Output structure documented in Phase 1 document (Sections 2, 3)

### HTTP Status Codes and Return Path Responses
- ✅ Section 14 (HTTP Status Codes and Return Path Responses - Step 1e) present
- ✅ All return paths documented with HTTP status codes (5 return paths)
- ✅ Response JSON examples provided for each return path
- ✅ Populated fields documented for each return path (source and populated by)
- ✅ Decision conditions leading to each return documented
- ✅ Error codes and success codes documented for each return path
- ✅ Downstream operation HTTP status codes documented (shape33: 200/201 success, 400/401/404/500 error)
- ✅ Error handling strategy documented (returnErrors=true, returnResponses=true)

### Request/Response JSON Examples
- ✅ Section 15 (Request/Response JSON Examples) present
- ✅ Process Layer entry point request JSON example provided
- ✅ Process Layer response JSON examples provided (all 5 return paths)
- ✅ Downstream System Layer request JSON examples provided (Oracle Fusion API)
- ✅ Downstream System Layer response JSON examples provided (success and error scenarios)

### Map Analysis
- ✅ ALL map files identified and loaded (3 maps)
- ✅ HTTP request maps identified (c426b4d6-2aff-450e-b43b-59956c4dbc96)
- ✅ Field mappings extracted from each map (Step 1d)
- ✅ Profile vs map field name discrepancies documented (employeeNumber→personNumber, etc.)
- ✅ Map field names marked as AUTHORITATIVE for HTTP requests
- ✅ Scripting functions analyzed (None in this process)
- ✅ Static values identified and documented (Map defaults: status, message, success)
- ✅ Process property mappings documented (DPP_ErrorMessage used in error map)
- ✅ Map Analysis documented in Phase 1 document (Section 5)

### Edge Cases
- ✅ Nested branches/decisions analyzed (None)
- ✅ Loops identified (None)
- ✅ Property chains traced (Step 4)
- ✅ Circular dependencies detected and resolved (None)
- ✅ Try/Catch error paths documented (shape17, subprocess shape2)

---

## 20. Self-Check Questions (FINAL VALIDATION)

### Extraction Completeness

1. ❓ **Did I analyze ALL map files?** YES
   - Analyzed 3 maps: c426b4d6-2aff-450e-b43b-59956c4dbc96, e4fd3f59-edb5-43a1-aeae-143b600a064e, f46b845a-7d75-41b5-b0ad-c41a6a8e9b12

2. ❓ **Did I identify HTTP request maps?** YES
   - Map c426b4d6-2aff-450e-b43b-59956c4dbc96 transforms D365 request to Oracle Fusion request

3. ❓ **Did I extract actual field names from maps?** YES
   - Documented in Section 5 (Map Analysis)

4. ❓ **Did I compare profile field names vs map field names?** YES
   - employeeNumber → personNumber
   - absenceStatusCode → absenceStatusCd
   - approvalStatusCode → approvalStatusCd

5. ❓ **Did I mark map field names as AUTHORITATIVE?** YES
   - Documented in Section 5

6. ❓ **Did I analyze scripting functions in maps?** YES
   - Map f46b845a-7d75-41b5-b0ad-c41a6a8e9b12 uses PropertyGet function (no scripting)
   - No scripting functions in other maps

7. ❓ **Did I extract element names from maps?** N/A
   - This is REST API (not SOAP), no SOAP elements

8. ❓ **Did I verify namespace prefixes from message shapes?** N/A
   - This is REST API (not SOAP), no SOAP namespaces

9. ❓ **Did I extract HTTP status codes for all return paths?** YES
   - Documented in Section 14 (5 return paths with status codes)

10. ❓ **Did I document response JSON for each return path?** YES
    - Documented in Sections 14 and 15

11. ❓ **Did I document populated fields for each return path?** YES
    - Documented in Section 14 with source and populated by

12. ❓ **Did I extract HTTP status codes for downstream operations?** YES
    - shape33: Expected 200/201, Error 400/401/404/500

13. ❓ **Did I create request/response JSON examples?** YES
    - Documented in Section 15

14. ❓ **Did I verify business logic FIRST before following dragpoints?** YES
    - Documented in Step 0 of Section 12 (Execution Order)

15. ❓ **Did I identify what each operation does and what it produces?** YES
    - Documented in Section 12, Step 0

16. ❓ **Did I identify which operations MUST execute first based on business logic?** YES
    - shape38 MUST execute FIRST (produces properties)
    - shape33 MUST execute BEFORE shape2 (produces status code)

17. ❓ **Did I check data dependencies FIRST before following dragpoints?** YES
    - Documented in Step 4, verified in Step 9

18. ❓ **Did I use operation response analysis from Step 1c?** YES
    - Referenced in Step 9

19. ❓ **Did I use decision analysis from Step 7?** YES
    - Referenced in Step 9

20. ❓ **Did I use dependency graph from Step 4?** YES
    - Referenced in Step 9

21. ❓ **Did I use branch analysis from Step 8?** YES
    - Referenced in Step 9

22. ❓ **Did I verify all property reads happen after property writes?** YES
    - Verified in Step 9

23. ❓ **Did I follow topological sort order for sequential branches?** YES
    - Branch shape20 analyzed, but corrected to PARALLEL (paths terminate independently)

### Mandatory Sections Present

- ✅ Section 1: Operations Inventory
- ✅ Section 2: Input Structure Analysis (Step 1a)
- ✅ Section 3: Response Structure Analysis (Step 1b)
- ✅ Section 4: Operation Response Analysis (Step 1c)
- ✅ Section 5: Map Analysis (Step 1d)
- ✅ Section 6: Process Properties Analysis (Steps 2-3)
- ✅ Section 7: Data Dependency Graph (Step 4)
- ✅ Section 8: Control Flow Graph (Step 5)
- ✅ Section 9: Decision Shape Analysis (Step 7)
- ✅ Section 10: Branch Shape Analysis (Step 8)
- ✅ Section 11: Subprocess Analysis (Step 7a)
- ✅ Section 12: Execution Order (Step 9)
- ✅ Section 13: Sequence Diagram (Step 10)
- ✅ Section 14: HTTP Status Codes and Return Path Responses (Step 1e)
- ✅ Section 15: Request/Response JSON Examples
- ✅ Section 16: Field Mapping Analysis
- ✅ Section 17: Critical Patterns Identified
- ✅ Section 18: System Layer Identification
- ✅ Section 19: Validation Checklist
- ✅ Section 20: Self-Check Questions

---

## 21. Function Exposure Decision Table

### Process Layer Function Exposure Analysis

**Process Name:** HCM_Leave Create  
**Business Domain:** Human Resource (HCM)  
**Business Capability:** Leave/Absence Management

### Should This Process Be Exposed as a Process Layer Function?

**Analysis:**

| Criteria | Evaluation | Score |
|---|---|---|
| **Orchestrates multiple System APIs** | No - Only calls Oracle Fusion HCM API (single system) | ❌ |
| **Contains business logic** | Yes - Error handling, response transformation, notification | ✅ |
| **Transforms data between systems** | Yes - D365 format → Oracle Fusion format | ✅ |
| **Reusable across multiple consumers** | Yes - Can be called by D365, other HCM processes, or Experience APIs | ✅ |
| **Encapsulates business domain logic** | Yes - Leave creation business rules | ✅ |
| **Provides single view of entity** | No - Does not aggregate data from multiple systems | ❌ |

**Decision:** ✅ **YES - EXPOSE AS PROCESS LAYER FUNCTION**

**Reasoning:**
1. **Business Logic Encapsulation:** Contains error handling, response transformation, and notification logic
2. **Reusability:** Can be called by multiple consumers (D365, other HCM processes, Experience APIs)
3. **Domain Boundary:** Encapsulates HCM leave creation business capability
4. **Transformation Logic:** Transforms between D365 and Oracle Fusion formats
5. **Single System Orchestration:** While it only calls one System API, it adds value through error handling and standardized response format

**Function Signature:**

```
POST /api/hcm/leave/create
Content-Type: application/json

Request:
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

Response (Success - 200):
{
  "leaveResponse": {
    "status": "success",
    "message": "Data successfully sent to Oracle Fusion",
    "personAbsenceEntryId": 300000123456789,
    "success": "true"
  }
}

Response (Error - 400/500):
{
  "leaveResponse": {
    "status": "failure",
    "message": "Error message details",
    "success": "false"
  }
}
```

### Alternative: Should This Be a System Layer Function Instead?

**Analysis:**

| Criteria | Evaluation | Score |
|---|---|---|
| **Directly wraps single system of record** | Yes - Wraps Oracle Fusion HCM API | ✅ |
| **No orchestration logic** | No - Contains error handling and notification | ❌ |
| **Minimal transformation** | No - Transforms D365 format to Oracle Fusion format | ❌ |
| **1:1 mapping to system API** | No - Adds error handling and notification logic | ❌ |

**Decision:** ❌ **NO - NOT A SYSTEM LAYER FUNCTION**

**Reasoning:**
- Contains business logic beyond simple system wrapping
- Adds error notification capability
- Transforms between different formats (not just wrapping Oracle API)

### Recommended Architecture

**Layer:** Process Layer  
**Function Name:** CreateLeave  
**Namespace:** HCM.Leave  
**Full Path:** /api/hcm/leave/create

**Dependencies:**
- **System Layer:** Oracle Fusion HCM System API (to be created separately)
  - Function: CreateAbsence
  - Endpoint: POST /api/systems/oracle-fusion-hcm/absences
  - Purpose: Direct wrapper for Oracle Fusion HCM REST API

**Refactoring Recommendation:**

**Current Architecture:**
```
Process Layer (HCM_Leave Create)
  └─→ Directly calls Oracle Fusion HCM API
```

**Recommended Architecture:**
```
Process Layer (HCM.Leave.CreateLeave)
  └─→ System Layer (OracleFusionHCM.Absence.CreateAbsence)
      └─→ Oracle Fusion HCM API
```

**Benefits:**
1. **Separation of Concerns:** System Layer handles Oracle Fusion API specifics, Process Layer handles business logic
2. **Reusability:** System Layer function can be reused by other Process Layer functions
3. **Maintainability:** Changes to Oracle Fusion API only affect System Layer
4. **Testability:** System Layer can be mocked for Process Layer testing

**Implementation Plan:**
1. Create System Layer function: OracleFusionHCM.Absence.CreateAbsence
2. Refactor Process Layer function: HCM.Leave.CreateLeave to call System Layer
3. Move Oracle Fusion API connection and operation to System Layer
4. Keep error handling and notification logic in Process Layer

---

## 22. Phase 1 Extraction Complete

### Summary

**Process Analyzed:** HCM_Leave Create  
**Total Shapes:** 14 main process shapes + 12 subprocess shapes  
**Total Operations:** 4 operations (1 entry point, 1 HTTP, 2 email)  
**Total Decisions:** 2 decisions (HTTP status check, GZIP check) + 1 subprocess decision (attachment check)  
**Total Branches:** 1 branch (error handling)  
**Total Subprocesses:** 1 subprocess (Office 365 Email)  
**Total Maps:** 3 maps (request transform, success response, error response)  
**Total Profiles:** 5 profiles (request, response, Oracle response, error, dummy)

### Key Findings

1. **Process Layer Function:** Should be exposed as Process Layer API
2. **Single System Orchestration:** Calls Oracle Fusion HCM API only
3. **Error Handling:** Comprehensive error handling with Try/Catch, status checks, and email notifications
4. **GZIP Support:** Handles GZIP compressed error responses from Oracle Fusion
5. **Dual Error Notification:** Sends email to support team AND returns error response to caller
6. **No SOAP:** This is a REST API integration (not SOAP)

### Next Steps

**Phase 1 Complete:** ✅ All mandatory sections documented  
**Ready for Phase 2:** ✅ Code generation can proceed

**Recommended Actions:**
1. Create System Layer function for Oracle Fusion HCM API
2. Create Process Layer function for HCM Leave Create
3. Implement error handling and notification logic
4. Implement GZIP decompression for error responses
5. Implement email notification subprocess

---

**Document Version:** 1.0  
**Analysis Complete:** 2026-02-20  
**Analyst:** Cloud Agent  
**Status:** ✅ PHASE 1 EXTRACTION COMPLETE - READY FOR CODE GENERATION
