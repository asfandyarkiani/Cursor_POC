# BOOMI EXTRACTION PHASE 1 - Create Work Order from EQ+ to CAFM

**Process Name:** Create Work Order from EQ+ to CAFM  
**Process ID:** cf0ab01d-2ce4-4588-8265-54fc4290368a  
**System of Record (SOR):** CAFM (FSI Evolution)  
**Integration Type:** SOAP/XML  
**Authentication:** Session-based (Login/Logout)  
**Document Processing:** Array input (workOrder array) - Each array element triggers separate process execution

---

## 1. OPERATIONS INVENTORY

Based on analysis of all operation files, the following operations are identified:

| # | Operation ID | Operation Name | Type | Purpose | Request Profile | Response Profile |
|---|---|---|---|---|---|---|
| 1 | de68dad0-be76-4ec8-9857-4e5cf2a7bd4c | EQ+_CAFM_Create | wss | Entry point (Web Service) | af096014 | 9e542ed5 |
| 2 | c20e5991-4d70-47f7-8e25-847df3e5eb6d | Login | http/SOAP | Authentication | 60e1aeea | 992136d3 |
| 3 | 381a025b-f3b9-4597-9902-3be49715c978 | Session logout | http/SOAP | Logout | 6b3afee8 | NONE |
| 4 | 442683cb-b984-499e-b7bb-075c826905aa | GetLocationsByDto_CAFM | http/SOAP | Get location details | 589e623c | 3aa0f5c5 |
| 5 | dc3b6b85-848d-471d-8c76-ed3b7dea0fbd | GetInstructionSetsByDto_CAFM | http/SOAP | Get instruction sets | 589e623c | 5c2f13dd |
| 6 | 33dac20f-ea09-471c-91c3-91b39bc3b172 | CreateBreakdownTask Order EQ+ | http/SOAP | Create work order | 362c3ec8 | dbcca2ef |
| 7 | 52166afd-a020-4de9-b49e-55400f1c0a7a | GetBreakdownTasksByDto_CAFM | http/SOAP | Check task exists | 589e623c | 23f4cc6e |
| 8 | c52c74c2-95e3-4cba-990e-3ce4746836a2 | CreateEvent_CAFM | http/SOAP | Create recurring event | 589e623c | 1570c9d2 |
| 9 | af07502a-fafd-4976-a691-45d51a33b549 | Office 365 Email (with attachment) | mail/SMTP | Send email | - | - |
| 10 | 15a72a21-9b57-49a1-a8ed-d70367146644 | Office 365 Email (without attachment) | mail/SMTP | Send email | - | - |

**Total Operations:** 10 (7 SOAP, 2 SMTP, 1 Web Service entry point)

---

## 2. INPUT STRUCTURE ANALYSIS (Step 1a)

### Request Profile Structure

**Profile ID:** af096014-313f-4565-9091-2bdd56eb46df  
**Profile Name:** EQ+_CAFM_Create_Request  
**Profile Type:** profile.json  
**Root Structure:** Root/Object/workOrder/Array/ArrayElement1/Object/...  
**Array Detection:** ✅ YES - workOrder is an array  
**Array Cardinality:**
- minOccurs: 0
- maxOccurs: -1 (unlimited)

**Input Type:** singlejson

### Input Format

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

### Document Processing Behavior

**Boomi Processing:** Each array element triggers separate process execution (inputType="singlejson" with array)  
**Azure Function Requirement:** Must accept array and process each work order element  
**Implementation Pattern:** Loop through array, process each work order, aggregate results

### Field Count

**Total Fields:** 19 fields (10 root-level + 9 nested in ticketDetails object)

---

## 3. RESPONSE STRUCTURE ANALYSIS (Step 1b)

### Response Profile Structure

**Profile ID:** 9e542ed5-2c65-4af8-b0c6-821cbc58ca31  
**Profile Name:** EQ+_CAFM_Create_Response  
**Profile Type:** profile.json  
**Root Structure:** Root/Object/workOrder/Array/ArrayElement1/Object/...  
**Array Detection:** ✅ YES - workOrder is an array (matches request)

### Response Format

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

### Response Field Mapping

| Boomi Field Path | Boomi Field Name | Data Type | Required | Azure DTO Property |
|---|---|---|---|---|
| Root/Object/workOrder/Array/ArrayElement1/Object/cafmSRNumber | cafmSRNumber | character | Yes | CafmSRNumber |
| Root/Object/workOrder/Array/ArrayElement1/Object/sourceSRNumber | sourceSRNumber | character | No | SourceSRNumber |
| Root/Object/workOrder/Array/ArrayElement1/Object/sourceOrgId | sourceOrgId | character | No | SourceOrgId |
| Root/Object/workOrder/Array/ArrayElement1/Object/status | status | character | No | Status |
| Root/Object/workOrder/Array/ArrayElement1/Object/message | message | character | No | Message |

**Total Response Fields:** 5 fields

---

## 4. OPERATION RESPONSE ANALYSIS (Step 1c)

### Operation 1: Login (Authenticate)

**Operation ID:** c20e5991-4d70-47f7-8e25-847df3e5eb6d  
**Response Profile ID:** 992136d3-da44-4f22-994b-f7181624215b  
**Extracted Fields:**
- SessionId → Written to process.DPP_SessionId (by shape8 in FsiLogin subprocess)

**Consumers:**
- All subsequent SOAP operations (GetLocationsByDto, GetInstructionSetsByDto, CreateBreakdownTask, GetBreakdownTasksByDto, CreateEvent)

### Operation 2: GetLocationsByDto

**Operation ID:** 442683cb-b984-499e-b7bb-075c826905aa  
**Response Profile ID:** 3aa0f5c5-8c95-4023-aba9-9d78dd6ade96  
**Extracted Fields:**
- BuildingId → Written to process.DPP_BuildingID (by shape25)
- LocationId → Written to process.DPP_LocationID (by shape25)

**Consumers:**
- CreateBreakdownTask operation (map function 6, 7)

### Operation 3: GetInstructionSetsByDto

**Operation ID:** dc3b6b85-848d-471d-8c76-ed3b7dea0fbd  
**Response Profile ID:** 5c2f13dd-3e51-4a7c-867b-c801aaa35562  
**Extracted Fields:**
- IN_FKEY_CAT_SEQ → Written to process.DPP_CategoryId (by shape28)
- IN_FKEY_LAB_SEQ → Written to process.DDP_DisciplineId (by shape28)
- IN_FKEY_PRI_SEQ → Written to process.DPP_PriorityId (by shape28)
- IN_SEQ → Written to process.DPP_InstructionId (by shape28)

**Consumers:**
- CreateBreakdownTask operation (map functions 2, 3, 4, 5)

**Key Insight:** ⚠️ **CRITICAL** - GetInstructionSetsByDto returns 4 IDs (CategoryId, DisciplineId, PriorityId, InstructionId), not just InstructionId. This single API call provides all category/discipline/priority identifiers needed for CreateBreakdownTask.

### Operation 4: CreateBreakdownTask

**Operation ID:** 33dac20f-ea09-471c-91c3-91b39bc3b172  
**Response Profile ID:** dbcca2ef-55cc-48e0-9329-1e8db4ada0c8  
**Extracted Fields:**
- PrimaryKeyId (BreakdownTaskId) → Written to process.DPP_BreakdownTaskId (by shape42)

**Consumers:**
- CreateEvent operation (if recurrence = "Y")
- Response mapping to cafmSRNumber

### Operation 5: GetBreakdownTasksByDto

**Operation ID:** 52166afd-a020-4de9-b49e-55400f1c0a7a  
**Response Profile ID:** 23f4cc6e-f46c-47fe-ad9d-6dc191adefb9  
**Extracted Fields:**
- PrimaryKeyId → Written to process.DPP_BreakdownTaskId (by shape55)

**Consumers:**
- Decision shape56 checks if task exists
- If exists, skip creation (early exit)

### Operation 6: CreateEvent

**Operation ID:** c52c74c2-95e3-4cba-990e-3ce4746836a2  
**Response Profile ID:** 1570c9d2-0588-410d-ad3d-bdc7d5c0ec9a  
**Extracted Fields:**
- PrimaryKeyId (EventId) → Written to process.DPP_EventId (by shape35)

**Consumers:**
- None (final operation in conditional path)

---

## 5. MAP ANALYSIS (Step 1d)

### SOAP Request Maps Inventory

| Map ID | Map Name | From Profile | To Profile | Operation |
|---|---|---|---|---|
| 390614fd | CreateBreakdownTask EQ+_to_CAFM_Create | af096014 | 362c3ec8 | CreateBreakdownTask |

### Map: CreateBreakdownTask (390614fd)

**From Profile:** af096014 (EQ+_CAFM_Create_Request)  
**To Profile:** 362c3ec8 (CreateBreakdownTask Request)  
**Type:** SOAP Request Map

**Element Names (CRITICAL):**
- Operation Element: CreateBreakdownTask
- DTO Element: breakdownTaskDto
- **RULE:** Use "breakdownTaskDto" in SOAP envelope, NOT generic "dto"

**Namespace Prefixes (CRITICAL):**
Based on message shape analysis (shape26, shape23):
- xmlns:ns="http://www.fsi.co.uk/services/evolution/04/09"
- xmlns:fsi1="http://schemas.datacontract.org/2004/07/Fsi.Concept.Contracts.Entities.ServiceModel"
- xmlns:fsi2="http://schemas.datacontract.org/2004/07/Fsi.Concept.Tasks.Contracts.Entities"

**Field Mappings:**

| Source Field | Source Type | Target Field (SOAP) | Profile Field Name | Discrepancy? |
|---|---|---|---|---|
| reporterName | profile | ReporterName | ReporterName | ✅ Match |
| reporterEmail | profile | BDET_EMAIL | BDET_EMAIL | ✅ Match |
| reporterPhoneNumber | profile | Phone | Phone | ✅ Match |
| serviceRequestNumber | profile | CallId | CallId | ✅ Match |
| DPP_SessionId | function (process property) | sessionId | sessionId | ✅ Match |
| DPP_CategoryId | function (process property) | CategoryId | BDET_FKEY_CAT_SEQ | ❌ DIFFERENT |
| DDP_DisciplineId | function (process property) | DisciplineId | BDET_FKEY_LAB_SEQ | ❌ DIFFERENT |
| DPP_PriorityId | function (process property) | PriorityId | BDET_FKEY_PRI_SEQ | ❌ DIFFERENT |
| DPP_BuildingID | function (process property) | BuildingId | BuildingId | ✅ Match |
| DPP_LocationID | function (process property) | LocationId | LocationId | ✅ Match |
| DPP_InstructionId | function (process property) | InstructionId | IN_SEQ | ❌ DIFFERENT |
| description | profile | LongDescription | LongDescription | ✅ Match |
| scheduledDate + scheduledTimeStart | function (scripting) | ScheduledDateUtc | ScheduledDateUtc | ✅ Match |
| raisedDateUtc | function (scripting) | RaisedDateUtc | RaisedDateUtc | ✅ Match |

**Scripting Functions:**

| Function | Input | Output | Logic |
|---|---|---|---|
| Function 11 | scheduledDate, scheduledTimeStart | ScheduledDateUtc | Combine date+time, format to ISO with .0208713Z suffix |
| Function 13 | raisedDateUtc | RaisedDateUtc | Format to ISO with .0208713Z suffix |
| Function 14 | Static "1" | ContractId | Returns "1" |
| Function 15 | sourceOrgId | BDET_CALLER_SOURCE_ID | Pass through |

**Profile vs Map Discrepancies:**

| Profile Field Name | Map Field Name (ACTUAL) | Authority | Use in SOAP |
|---|---|---|---|
| BDET_FKEY_CAT_SEQ | CategoryId | ✅ MAP | CategoryId |
| BDET_FKEY_LAB_SEQ | DisciplineId | ✅ MAP | DisciplineId |
| BDET_FKEY_PRI_SEQ | PriorityId | ✅ MAP | PriorityId |
| IN_SEQ | InstructionId | ✅ MAP | InstructionId |

**CRITICAL RULE:** Map field names are AUTHORITATIVE. Use map field names in SOAP envelopes, NOT profile field names.

---

## 6. HTTP STATUS CODES AND RETURN PATH RESPONSES (Step 1e)

### Return Path 1: Success Return

**Return Label:** "Return Documents"  
**Return Shape ID:** shape15  
**HTTP Status Code:** 200  
**Decision Conditions Leading to Return:**
- CreateBreakdownTask succeeded (decision shape12 TRUE path)

**Populated Response Fields:**

| Field Name | Field Path | Source | Populated By |
|---|---|---|---|
| cafmSRNumber | Root/Object/workOrder/.../cafmSRNumber | operation_response | CreateBreakdownTask (PrimaryKeyId) |
| sourceSRNumber | Root/Object/workOrder/.../sourceSRNumber | input_field | serviceRequestNumber from request |
| sourceOrgId | Root/Object/workOrder/.../sourceOrgId | input_field | sourceOrgId from request |
| status | Root/Object/workOrder/.../status | static | "Success" |
| message | Root/Object/workOrder/.../message | static | "Work order created successfully" |

**Response JSON Example:**

```json
{
  "workOrder": [
    {
      "cafmSRNumber": "CAFM-2025-12345",
      "sourceSRNumber": "EQ-2025-001",
      "sourceOrgId": "ORG-123",
      "status": "Success",
      "message": "Work order created successfully"
    }
  ]
}
```

### Return Path 2: Failure Return (Catch Block)

**Return Label:** "Failure"  
**Return Shape ID:** shape18  
**HTTP Status Code:** 400  
**Decision Conditions Leading to Return:**
- Any exception in try block (shape3 catch path)

**Error Code:** CAFM_CREATE_FAILURE  
**Populated Response Fields:**

| Field Name | Field Path | Source | Populated By |
|---|---|---|---|
| status | Root/Object/workOrder/.../status | static | "Failure" |
| message | Root/Object/workOrder/.../message | process_property | DPP_ErrorMessage (catch error message) |
| sourceSRNumber | Root/Object/workOrder/.../sourceSRNumber | input_field | serviceRequestNumber from request |
| sourceOrgId | Root/Object/workOrder/.../sourceOrgId | input_field | sourceOrgId from request |

**Response JSON Example:**

```json
{
  "workOrder": [
    {
      "cafmSRNumber": "",
      "sourceSRNumber": "EQ-2025-001",
      "sourceOrgId": "ORG-123",
      "status": "Failure",
      "message": "CAFM Log In Failed. CAFM Log In API Responded with Blank Response"
    }
  ]
}
```

### Downstream Operations HTTP Status Codes

| Operation Name | Expected Success Codes | Error Codes | Error Handling |
|---|---|---|---|
| Login (Authenticate) | 200 | 401, 500 | Throw exception (decision shape4 checks 20*) |
| GetLocationsByDto | 200 | 404, 500 | Continue with empty (branch convergence at shape6) |
| GetInstructionSetsByDto | 200 | 404, 500 | Continue with empty (branch convergence at shape6) |
| GetBreakdownTasksByDto | 200 | 404, 500 | Continue (check-before-create pattern) |
| CreateBreakdownTask | 200 | 400, 500 | Throw exception (decision shape12 checks 20*) |
| CreateEvent | 200 | 400, 500 | Continue (conditional operation) |
| Logout | 200 | 500 | Continue (cleanup operation) |

---

## 7. PROCESS PROPERTIES ANALYSIS (Steps 2-3)

### Properties WRITTEN

| Property Name | Written By | Source | Value |
|---|---|---|---|
| process.DPP_Process_Name | shape2 | Execution property | Process Name |
| process.DPP_AtomName | shape2 | Execution property | Atom Name |
| process.DPP_Payload | shape2 | Current document | Request payload |
| process.DPP_ExecutionID | shape2 | Execution property | Execution Id |
| process.DPP_File_Name | shape2 | Defined parameter + date | File_Name + timestamp + .txt |
| process.DPP_Subject | shape2 | Execution properties | Email subject |
| process.DPP_HasAttachment | shape2 | Defined parameter | Has_Attachment flag |
| process.To_Email | shape2 | Defined parameter | To_Email address |
| process.DPP_SourseSRNumber | shape2 | Input profile | serviceRequestNumber |
| process.DPP_sourseORGId | shape2 | Input profile | sourceOrgId |
| process.DPP_SessionId | shape8 (FsiLogin subprocess) | Login response | SessionId from Authenticate |
| process.DPP_BuildingID | shape25 | GetLocationsByDto response | BuildingId |
| process.DPP_LocationID | shape25 | GetLocationsByDto response | LocationId |
| process.DPP_CategoryId | shape28 | GetInstructionSetsByDto response | IN_FKEY_CAT_SEQ |
| process.DDP_DisciplineId | shape28 | GetInstructionSetsByDto response | IN_FKEY_LAB_SEQ |
| process.DPP_PriorityId | shape28 | GetInstructionSetsByDto response | IN_FKEY_PRI_SEQ |
| process.DPP_InstructionId | shape28 | GetInstructionSetsByDto response | IN_SEQ |
| process.DPP_BreakdownTaskId | shape42 or shape55 | CreateBreakdownTask or GetBreakdownTasksByDto | PrimaryKeyId |
| process.DPP_EventId | shape35 | CreateEvent response | PrimaryKeyId |
| process.DPP_ErrorMessage | shape21 | Catch errors message | Error details |
| process.DPP_CafmError | shape11 (FsiLogin subprocess) | Static | Login error message |

### Properties READ

| Property Name | Read By | Usage |
|---|---|---|
| process.DPP_SessionId | All SOAP operations | Authentication session |
| process.DPP_CategoryId | CreateBreakdownTask (map function 2) | CategoryId field |
| process.DDP_DisciplineId | CreateBreakdownTask (map function 3) | DisciplineId field |
| process.DPP_PriorityId | CreateBreakdownTask (map function 4) | PriorityId field |
| process.DPP_InstructionId | CreateBreakdownTask (map function 5) | InstructionId field |
| process.DPP_BuildingID | CreateBreakdownTask (map function 6) | BuildingId field |
| process.DPP_LocationID | CreateBreakdownTask (map function 7) | LocationId field |
| process.DPP_BreakdownTaskId | CreateEvent, Response mapping | Event linking, response |
| process.DPP_ErrorMessage | Email subprocess | Error notification |
| process.To_Email | Email subprocess | Email recipient |
| process.DPP_HasAttachment | Email subprocess decision | Attachment check |

---

## 8. DATA DEPENDENCY GRAPH (Step 4)

### Dependency Chains

**Chain 1: Authentication → All Operations**
- FsiLogin (shape5) WRITES process.DPP_SessionId
- GetLocationsByDto (shape23-24-25) READS process.DPP_SessionId
- GetInstructionSetsByDto (shape26-27-28) READS process.DPP_SessionId
- GetBreakdownTasksByDto (shape50-51-55) READS process.DPP_SessionId
- CreateBreakdownTask (shape7-8-11) READS process.DPP_SessionId
- CreateEvent (shape32-33-35) READS process.DPP_SessionId
- FsiLogout (shape13) READS process.DPP_SessionId

**Chain 2: Lookups → CreateBreakdownTask**
- GetLocationsByDto (shape25) WRITES process.DPP_BuildingID, process.DPP_LocationID
- GetInstructionSetsByDto (shape28) WRITES process.DPP_CategoryId, process.DDP_DisciplineId, process.DPP_PriorityId, process.DPP_InstructionId
- CreateBreakdownTask (map functions) READS all above properties

**Chain 3: CreateBreakdownTask → CreateEvent**
- CreateBreakdownTask (shape42) WRITES process.DPP_BreakdownTaskId
- CreateEvent (shape32) READS process.DPP_BreakdownTaskId

### Execution Order Requirements

**MANDATORY ORDER:**
1. FsiLogin MUST execute FIRST (provides SessionId for all operations)
2. GetLocationsByDto and GetInstructionSetsByDto MUST execute BEFORE CreateBreakdownTask (provide required IDs)
3. CreateBreakdownTask MUST execute BEFORE CreateEvent (provides BreakdownTaskId)
4. FsiLogout MUST execute LAST (cleanup)

---

## 9. CONTROL FLOW GRAPH (Step 5)

### Shape Connections

| From Shape | To Shape | Identifier | Type |
|---|---|---|---|
| shape1 (start) | shape2 | default | Extract input |
| shape2 | shape3 | default | Try/catch block |
| shape3 | shape4 | default (try) | Branch shape |
| shape3 | shape20 | error (catch) | Error handling branch |
| shape4 | shape5 | 1 | FsiLogin subprocess |
| shape4 | shape50 | 2 | GetBreakdownTasksByDto path |
| shape4 | shape23 | 3 | GetLocationsByDto path |
| shape4 | shape26 | 4 | GetInstructionSetsByDto path |
| shape4 | shape31 | 5 | Check recurrence path |
| shape4 | shape13 | 6 | FsiLogout path |
| shape5 | shape6 | default | Convergence (stop continue=true) |
| shape23-24-25 | shape30 | default | GetLocationsByDto → convergence |
| shape26-27-28 | shape29 | default | GetInstructionSetsByDto → convergence |
| shape50-51-55 | shape56 | default | GetBreakdownTasksByDto → decision |
| shape31 | shape32 | false | Recurrence check (if Y, create event) |
| shape31 | shape30 | true | Skip event creation |
| shape6 (convergence) | shape7 | default | Map to CreateBreakdownTask |
| shape7-8-9-11 | shape12 | default | CreateBreakdownTask → decision |
| shape12 | shape16 | true | Success path |
| shape12 | shape47 | false | Error path |
| shape16 | shape15 | default | Return success |
| shape47 | shape17 | default | Return failure |
| shape20 | shape21 | 1 | Set error message |
| shape20 | shape18 | 2 | Return failure |
| shape20 | shape22 | 3 | Throw exception |
| shape21 | shape19 | default | Email subprocess |

### Convergence Points

| Shape | Type | Purpose |
|---|---|---|
| shape6 | stop (continue=true) | Branch paths converge before CreateBreakdownTask |
| shape30 | stop (continue=true) | GetLocationsByDto and recurrence paths converge |
| shape29 | stop (continue=true) | GetInstructionSetsByDto path converges |

---

## 10. DECISION SHAPE ANALYSIS (Step 7)

### Decision Data Source Analysis

**Decision 1: shape4 (FsiLogin subprocess) - Login Status Check**
- **Shape ID:** shape4 (in FsiLogin subprocess)
- **Comparison:** regex "20*"
- **Data Source:** TRACK_PROPERTY (meta.base.applicationstatuscode from Login response)
- **Decision Type:** POST-OPERATION (checks response from Login)
- **Actual Execution Order:** Login → Check Response → Decision → Route
- **TRUE Path:** shape8 → Extract SessionId → Stop (continue=true) [SUCCESS]
- **FALSE Path:** shape11 → Set error property → shape6 → Map error → shape7 → Return [ERROR]
- **Pattern:** Authentication validation (continue if valid, error if invalid)
- **Early Exit:** FALSE path returns error (Login_Error)

**Decision 2: shape12 - CreateBreakdownTask Status Check**
- **Shape ID:** shape12
- **Comparison:** regex "20*"
- **Data Source:** TRACK_PROPERTY (meta.base.applicationstatuscode from CreateBreakdownTask response)
- **Decision Type:** POST-OPERATION (checks response from CreateBreakdownTask)
- **Actual Execution Order:** CreateBreakdownTask → Check Response → Decision → Route
- **TRUE Path:** shape16 → Map success response → shape15 → Return Documents [SUCCESS]
- **FALSE Path:** shape47 → Map error response → shape17 → Return Documents [FAILURE]
- **Pattern:** Main operation validation (success vs failure)
- **Early Exit:** Both paths terminate (return documents)

**Decision 3: shape31 - Recurrence Check**
- **Shape ID:** shape31
- **Comparison:** notequals "Y"
- **Data Source:** INPUT (ticketDetails.recurrence from request)
- **Decision Type:** PRE-FILTER (checks input data, but executes after lookups)
- **Actual Execution Order:** After branch convergence → Decision → Route
- **TRUE Path:** shape30 → Convergence (skip event creation) [SKIP]
- **FALSE Path:** shape32 → CreateEvent operation [CREATE EVENT]
- **Pattern:** Conditional operation (create event only if recurring)
- **Early Exit:** TRUE path skips event creation

**Decision 4: shape56 - Task Exists Check**
- **Shape ID:** shape56
- **Comparison:** equals "" (empty)
- **Data Source:** PROCESS_PROPERTY (process.DPP_BreakdownTaskId from GetBreakdownTasksByDto)
- **Decision Type:** POST-OPERATION (checks response from GetBreakdownTasksByDto)
- **Actual Execution Order:** GetBreakdownTasksByDto → Extract BreakdownTaskId → Decision → Route
- **TRUE Path:** shape57 → Convergence (task not found, proceed to create) [CONTINUE]
- **FALSE Path:** shape58 → Map existing task → shape59 → Return Documents [EARLY EXIT]
- **Pattern:** Check-before-create (if exists, return; if not, create)
- **Early Exit:** FALSE path returns existing task

**Decision 5: shape4 (Email subprocess) - Attachment Check**
- **Shape ID:** shape4 (in Office 365 Email subprocess)
- **Comparison:** equals "Y"
- **Data Source:** PROCESS_PROPERTY (DPP_HasAttachment)
- **Decision Type:** PRE-FILTER (checks input flag)
- **Actual Execution Order:** Decision → Route to appropriate email operation
- **TRUE Path:** shape11 → Build email with attachment → shape14-15-6-3 → Send [WITH ATTACHMENT]
- **FALSE Path:** shape23 → Build email without attachment → shape22-20-7 → Send [WITHOUT ATTACHMENT]
- **Pattern:** Conditional email attachment
- **Early Exit:** None (both paths send email)

### Decision Summary

**✅ Decision data sources identified: YES**  
**✅ Decision types classified: YES**  
**✅ Execution order verified: YES**  
**✅ All decision paths traced: YES**  
**✅ Decision patterns identified: YES**  
**✅ Paths traced to termination: YES**

---

## 11. BRANCH SHAPE ANALYSIS (Step 8)

### Branch 1: shape4 (Main Branch - 6 Paths)

**Shape ID:** shape4  
**Number of Paths:** 6  
**Location:** After try block, before operations

#### Properties Analysis

**Path 1: FsiLogin (shape5)**
- **READS:** [] (no process properties)
- **WRITES:** [process.DPP_SessionId]
- **PROOF:** Subprocess shape5 calls FsiLogin, which extracts SessionId (shape8 in subprocess)

**Path 2: GetBreakdownTasksByDto (shape50-51-55)**
- **READS:** [process.DPP_SessionId]
- **WRITES:** [process.DPP_BreakdownTaskId]
- **PROOF:** Message shape50 uses {1} = DPP_SessionId, documentproperties shape55 writes DPP_BreakdownTaskId

**Path 3: GetLocationsByDto (shape23-24-25)**
- **READS:** [process.DPP_SessionId, input:unitCode]
- **WRITES:** [process.DPP_BuildingID, process.DPP_LocationID]
- **PROOF:** Message shape23 uses {1} = DPP_SessionId, {2} = unitCode, documentproperties shape25 writes BuildingID and LocationID

**Path 4: GetInstructionSetsByDto (shape26-27-28)**
- **READS:** [process.DPP_SessionId, input:subCategory]
- **WRITES:** [process.DPP_CategoryId, process.DDP_DisciplineId, process.DPP_PriorityId, process.DPP_InstructionId]
- **PROOF:** Message shape26 uses {1} = DPP_SessionId, {2} = subCategory, documentproperties shape28 writes 4 IDs

**Path 5: Recurrence Check (shape31)**
- **READS:** [input:ticketDetails.recurrence]
- **WRITES:** [] (decision only)
- **PROOF:** Decision shape31 compares ticketDetails.recurrence to "Y"

**Path 6: FsiLogout (shape13)**
- **READS:** [process.DPP_SessionId]
- **WRITES:** []
- **PROOF:** Subprocess shape13 calls FsiLogout, which uses DPP_SessionId

#### Dependency Graph

```
Path 1 (FsiLogin) → WRITES DPP_SessionId
 ↓
Paths 2, 3, 4, 6 → READ DPP_SessionId
 ↓
Path 2 (GetBreakdownTasksByDto) → WRITES DPP_BreakdownTaskId
Path 3 (GetLocationsByDto) → WRITES DPP_BuildingID, DPP_LocationID
Path 4 (GetInstructionSetsByDto) → WRITES DPP_CategoryId, DDP_DisciplineId, DPP_PriorityId, DPP_InstructionId
 ↓
CreateBreakdownTask → READS all above properties
```

#### Classification

**🚨 CRITICAL RULE: ALL API CALLS ARE SEQUENTIAL**

**Classification:** SEQUENTIAL

**Reasoning:**
- Path 1 (FsiLogin) contains API call (SOAP authentication) → SEQUENTIAL
- Path 2 (GetBreakdownTasksByDto) contains API call (SOAP query) → SEQUENTIAL
- Path 3 (GetLocationsByDto) contains API call (SOAP lookup) → SEQUENTIAL
- Path 4 (GetInstructionSetsByDto) contains API call (SOAP lookup) → SEQUENTIAL
- Path 6 (FsiLogout) contains API call (SOAP logout) → SEQUENTIAL
- **RULE:** If ANY path contains API calls, classification is ALWAYS SEQUENTIAL (no parallel API calls in Azure Functions)

**Dependency Order (Topological Sort):**
1. Path 1 (FsiLogin) - Provides SessionId
2. Path 2 (GetBreakdownTasksByDto) - Uses SessionId, provides BreakdownTaskId (for check-before-create)
3. Path 3 (GetLocationsByDto) - Uses SessionId, provides BuildingID/LocationID
4. Path 4 (GetInstructionSetsByDto) - Uses SessionId, provides CategoryId/DisciplineId/PriorityId/InstructionId
5. Path 5 (Recurrence Check) - Decision only (no API call)
6. Path 6 (FsiLogout) - Uses SessionId, cleanup operation

#### Path Termination

| Path | Terminal Shape | Type |
|---|---|---|
| Path 1 | shape6 (stop continue=true) | Convergence |
| Path 2 | shape56 (decision) → shape57 or shape58 | Decision (check-before-create) |
| Path 3 | shape30 (stop continue=true) | Convergence |
| Path 4 | shape29 (stop continue=true) | Convergence |
| Path 5 | shape30 or shape32 | Decision (conditional event) |
| Path 6 | shape13 (stop continue=true) | Convergence |

#### Convergence Points

**shape6 (stop continue=true):** Branch paths converge here before CreateBreakdownTask

**Execution Continues From:** shape6 → shape7 (Map to CreateBreakdownTask)

### Branch 2: shape20 (Error Branch - 3 Paths)

**Shape ID:** shape20  
**Number of Paths:** 3  
**Location:** Catch block error handling

**Path 1:** shape21 → Set error message → shape19 (Email subprocess)  
**Path 2:** shape18 → Return failure  
**Path 3:** shape22 → Throw exception

**Classification:** SEQUENTIAL (error handling paths)

### Self-Check Results

✅ Classification completed: YES  
✅ Assumption check: NO (analyzed dependencies)  
✅ Properties extracted: YES  
✅ Dependency graph built: YES  
✅ Topological sort applied: YES (Path 1 → Path 2 → Path 3 → Path 4 → Path 5 → Path 6)

---

## 12. EXECUTION ORDER (Step 9)

### Business Logic Flow (Step 0 - MUST BE FIRST)

**Operation Analysis:**

1. **FsiLogin (Subprocess shape5):**
   - **Purpose:** Authentication - Establishes session with CAFM system
   - **Produces:** SessionId (written to process.DPP_SessionId)
   - **Dependent Operations:** ALL subsequent SOAP operations (6 operations depend on SessionId)
   - **Business Flow:** FsiLogin MUST execute FIRST (provides required SessionId for all downstream operations)

2. **GetBreakdownTasksByDto (shape50-51-55):**
   - **Purpose:** Check if work order already exists in CAFM
   - **Produces:** BreakdownTaskId (if found)
   - **Dependent Operations:** Decision shape56 checks result
   - **Business Flow:** Executes AFTER FsiLogin (needs SessionId), BEFORE CreateBreakdownTask (check-before-create pattern)

3. **GetLocationsByDto (shape23-24-25):**
   - **Purpose:** Lookup location and building IDs from unit code
   - **Produces:** BuildingId, LocationId
   - **Dependent Operations:** CreateBreakdownTask uses these IDs
   - **Business Flow:** Executes AFTER FsiLogin (needs SessionId), BEFORE CreateBreakdownTask (provides required IDs)

4. **GetInstructionSetsByDto (shape26-27-28):**
   - **Purpose:** Lookup category, discipline, priority, and instruction IDs from subCategory
   - **Produces:** CategoryId, DisciplineId, PriorityId, InstructionId
   - **Dependent Operations:** CreateBreakdownTask uses these IDs
   - **Business Flow:** Executes AFTER FsiLogin (needs SessionId), BEFORE CreateBreakdownTask (provides required IDs)

5. **CreateBreakdownTask (shape7-8-9-11):**
   - **Purpose:** Create work order in CAFM system
   - **Produces:** BreakdownTaskId (CAFM work order number)
   - **Dependent Operations:** CreateEvent (if recurrence = "Y"), Response mapping
   - **Business Flow:** Executes AFTER lookups (uses their IDs), main business operation

6. **CreateEvent (shape32-33-35):**
   - **Purpose:** Create recurring event for work order
   - **Produces:** EventId
   - **Dependent Operations:** None (optional operation)
   - **Business Flow:** Executes AFTER CreateBreakdownTask (needs BreakdownTaskId), ONLY if recurrence = "Y"

7. **FsiLogout (Subprocess shape13):**
   - **Purpose:** Close session with CAFM system
   - **Produces:** None
   - **Dependent Operations:** None
   - **Business Flow:** Executes LAST (cleanup operation, uses SessionId)

### Execution Order List

Based on dependency graph and business logic:

1. **START** (shape1)
2. **Extract Input Details** (shape2) - WRITES: DPP_Process_Name, DPP_Payload, etc.
3. **TRY Block Start** (shape3)
4. **BRANCH** (shape4) - 6 paths - SEQUENTIAL EXECUTION:
   - **Path 1:** FsiLogin (subprocess shape5) → WRITES: DPP_SessionId
   - **Path 2:** GetBreakdownTasksByDto (shape50-51-55) → WRITES: DPP_BreakdownTaskId → Decision (shape56)
     - **If task exists (FALSE):** shape58 → Map existing → shape59 → Return [EARLY EXIT]
     - **If task not found (TRUE):** shape57 → Continue to convergence
   - **Path 3:** GetLocationsByDto (shape23-24-25) → WRITES: DPP_BuildingID, DPP_LocationID
   - **Path 4:** GetInstructionSetsByDto (shape26-27-28) → WRITES: DPP_CategoryId, DDP_DisciplineId, DPP_PriorityId, DPP_InstructionId
   - **Path 5:** Recurrence Check (shape31) → Decision
     - **If recurrence notequals "Y" (TRUE):** shape30 → Convergence (skip event)
     - **If recurrence equals "Y" (FALSE):** shape32 → CreateEvent (shape32-33-35) → WRITES: DPP_EventId
   - **Path 6:** FsiLogout (subprocess shape13) → Cleanup
5. **CONVERGENCE** (shape6 - stop continue=true)
6. **Map to CreateBreakdownTask** (shape7)
7. **Build SOAP Request** (shape8-9)
8. **CreateBreakdownTask** (shape11) → WRITES: DPP_BreakdownTaskId
9. **Decision: CreateBreakdownTask Status** (shape12)
   - **If success (TRUE):** shape16 → Map success → shape15 → Return [SUCCESS]
   - **If failure (FALSE):** shape47 → Map error → shape17 → Return [FAILURE]
10. **CATCH Block** (shape20) - 3 error paths
    - **Path 1:** shape21 → Set error → shape19 (Email subprocess)
    - **Path 2:** shape18 → Return failure
    - **Path 3:** shape22 → Throw exception

### Dependency Verification

**Reference to Step 4 (Data Dependency Graph):**

- ✅ FsiLogin executes FIRST → Provides SessionId
- ✅ GetBreakdownTasksByDto reads SessionId → FsiLogin must execute before
- ✅ GetLocationsByDto reads SessionId → FsiLogin must execute before
- ✅ GetInstructionSetsByDto reads SessionId → FsiLogin must execute before
- ✅ CreateBreakdownTask reads SessionId, BuildingID, LocationID, CategoryId, DisciplineId, PriorityId, InstructionId → All lookups must execute before
- ✅ CreateEvent reads SessionId, BreakdownTaskId → CreateBreakdownTask must execute before
- ✅ FsiLogout reads SessionId → Executes last (cleanup)

**PROOF:** All property reads happen after property writes. No read-before-write violations.

### Branch Execution Order

**Reference to Step 8 (Branch Analysis):**

Branch shape4 paths execute in topological sort order:
1. Path 1 (FsiLogin) - No dependencies, provides SessionId
2. Path 2 (GetBreakdownTasksByDto) - Depends on SessionId from Path 1
3. Path 3 (GetLocationsByDto) - Depends on SessionId from Path 1
4. Path 4 (GetInstructionSetsByDto) - Depends on SessionId from Path 1
5. Path 5 (Recurrence Check) - Decision only, no dependencies
6. Path 6 (FsiLogout) - Depends on SessionId from Path 1, executes last

**PROOF:** Topological sort order ensures all dependencies satisfied.

### Decision Path Tracing

**Reference to Step 7 (Decision Analysis):**

**Decision 1 (FsiLogin status):**
- TRUE path: Extract SessionId → Continue
- FALSE path: Set error → Return [EARLY EXIT]

**Decision 2 (CreateBreakdownTask status):**
- TRUE path: Map success → Return [SUCCESS]
- FALSE path: Map error → Return [FAILURE]

**Decision 3 (Recurrence check):**
- TRUE path: Skip event creation → Convergence
- FALSE path: CreateEvent → Continue

**Decision 4 (Task exists check):**
- TRUE path: Task not found → Continue to create
- FALSE path: Task exists → Return existing [EARLY EXIT]

### Self-Check Results

✅ Business logic verified FIRST: YES  
✅ Operation analysis complete: YES  
✅ Business logic execution order identified: YES  
✅ Data dependencies checked FIRST: YES  
✅ Operation response analysis used: YES (Step 1c)  
✅ Decision analysis used: YES (Step 7)  
✅ Dependency graph used: YES (Step 4)  
✅ Branch analysis used: YES (Step 8)  
✅ Property dependency verification: YES  
✅ Topological sort applied: YES

---

## 13. SEQUENCE DIAGRAM (Step 10)

**Based on:**
- Section 8 (Data Dependency Graph)
- Section 9 (Control Flow Graph)
- Section 10 (Decision Analysis)
- Section 11 (Branch Analysis)
- Section 12 (Execution Order)

### 13.1 Operation Classification Table

| Operation | Shape(s) | Decision After? | Branch Convergence? | Operation Type | Classification | Error Handling | Reason | Boomi Reference |
|---|---|---|---|---|---|---|---|---|
| Login (Authenticate) | shape5 (subprocess) | Yes (shape4 checks 20*) | No | Authentication | AUTHENTICATION | Throw exception | Required for all operations | Subprocess FsiLogin with decision shape4 checking status "20*" |
| GetBreakdownTasksByDto | shape50-51-55 | Yes (shape56 checks empty) | No | Check existence | MAIN OPERATION | Throw exception | Check-before-create pattern | Decision shape56 checks if task exists |
| GetLocationsByDto | shape23-24-25 | No | Yes (shape6) | Lookup | BEST-EFFORT LOOKUP | Log warning, set empty, continue | Branch convergence, no decision checks | Branch path 3 converges at shape6 (no decision checks status) |
| GetInstructionSetsByDto | shape26-27-28 | No | Yes (shape6) | Lookup | BEST-EFFORT LOOKUP | Log warning, set empty, continue | Branch convergence, no decision checks | Branch path 4 converges at shape6 (no decision checks status) |
| CreateBreakdownTask | shape11 | Yes (shape12 checks 20*) | No | Main operation | MAIN OPERATION | Throw exception | Primary business operation | Main operation with decision shape12 checking status "20*" |
| CreateEvent | shape32-33-35 | No | No | Conditional | CONDITIONAL | Log warning, continue | Optional, task already created | Conditional execution (decision shape31, FALSE path) |
| Logout | shape13 (subprocess) | No | No | Cleanup | CLEANUP | Log error, continue | Cleanup only, non-critical | Subprocess FsiLogout |

### 13.2 Enhanced Sequence Diagram

```
START (shape1)
 |
 ├─→ Extract Input Details (shape2)
 | └─→ WRITES: [process.DPP_Process_Name, process.DPP_Payload, process.DPP_ExecutionID, process.DPP_File_Name, 
 | process.DPP_Subject, process.DPP_HasAttachment, process.To_Email, process.DPP_SourseSRNumber, 
 | process.DPP_sourseORGId]
 |
 ├─→ TRY Block Start (shape3)
 |
 ├─→ BRANCH (shape4) - 6 paths - SEQUENTIAL EXECUTION
 | |
 | ├─→ Path 1: FsiLogin (subprocess shape5) (Downstream) - (AUTHENTICATION)
 | | └─→ READS: []
 | | └─→ WRITES: [process.DPP_SessionId]
 | | └─→ HTTP: [Expected: 200, Error: 401/500]
 | | └─→ ERROR HANDLING: If fails → Throw exception (required for all operations)
 | | └─→ RESULT: sessionId (throws exception on failure)
 | | └─→ BOOMI: Subprocess FsiLogin with decision shape4 checking status "20*"
 | | └─→ INTERNAL FLOW:
 | | ├─→ START
 | | ├─→ Set URL and SOAPAction (shape3)
 | | ├─→ Build SOAP Request (shape5) - Uses FSI_Username, FSI_Password from config
 | | ├─→ SOAP: Authenticate (shape2)
 | | ├─→ Decision (shape4): Status Code "20*"?
 | | | ├─→ IF TRUE → Extract SessionId (shape8) → Stop (continue=true) [SUCCESS]
 | | | └─→ IF FALSE → Set error (shape11) → Map error (shape6) → Return "Login_Error" [ERROR]
 | |
 | ├─→ Path 2: GetBreakdownTasksByDto (shape50-51-55) (Downstream) - (MAIN OPERATION)
 | | └─→ READS: [process.DPP_SessionId, input:serviceRequestNumber]
 | | └─→ WRITES: [process.DPP_BreakdownTaskId]
 | | └─→ HTTP: [Expected: 200, Error: 404/500]
 | | └─→ ERROR HANDLING: If fails → Throw exception (check-before-create pattern)
 | | └─→ RESULT: breakdownTaskId (populated or empty)
 | | └─→ BOOMI: Check-before-create pattern (shape50-51-55 → decision shape56)
 | | └─→ Decision (shape56): BreakdownTaskId equals "" (empty)?
 | | | ├─→ IF TRUE (not found) → Continue to create (shape57 → convergence)
 | | | └─→ IF FALSE (exists) → Map existing task (shape58) → Return (shape59) [EARLY EXIT]
 | |
 | ├─→ Path 3: GetLocationsByDto (shape23-24-25) (Downstream) - (BEST-EFFORT LOOKUP)
 | | └─→ READS: [process.DPP_SessionId, input:unitCode]
 | | └─→ WRITES: [process.DPP_BuildingID, process.DPP_LocationID]
 | | └─→ HTTP: [Expected: 200, Error: 404/500]
 | | └─→ ERROR HANDLING: If fails → Log warning, set empty values, CONTINUE
 | | └─→ RESULT: buildingId, locationId (populated or empty)
 | | └─→ BOOMI: Branch path 3 (shape23-24-25) converges at shape6 (no decision checks status)
 | |
 | ├─→ Path 4: GetInstructionSetsByDto (shape26-27-28) (Downstream) - (BEST-EFFORT LOOKUP)
 | | └─→ READS: [process.DPP_SessionId, input:subCategory]
 | | └─→ WRITES: [process.DPP_CategoryId, process.DDP_DisciplineId, process.DPP_PriorityId, process.DPP_InstructionId]
 | | └─→ HTTP: [Expected: 200, Error: 404/500]
 | | └─→ ERROR HANDLING: If fails → Log warning, set empty values, CONTINUE
 | | └─→ RESULT: categoryId, disciplineId, priorityId, instructionId (populated or empty)
 | | └─→ BOOMI: Branch path 4 (shape26-27-28) converges at shape6 (no decision checks status)
 | |
 | ├─→ Path 5: Recurrence Check (shape31) (Decision Only)
 | | └─→ READS: [input:ticketDetails.recurrence]
 | | └─→ WRITES: []
 | | └─→ Decision: recurrence notequals "Y"?
 | | | ├─→ IF TRUE (not recurring) → Skip event creation (shape30 → convergence)
 | | | └─→ IF FALSE (recurring) → CreateEvent (shape32-33-35) (Downstream) - (CONDITIONAL)
 | | | └─→ READS: [process.DPP_SessionId, process.DPP_BreakdownTaskId]
 | | | └─→ WRITES: [process.DPP_EventId]
 | | | └─→ HTTP: [Expected: 200, Error: 400/500]
 | | | └─→ ERROR HANDLING: If fails → Log warning, CONTINUE (task already created)
 | | | └─→ RESULT: eventId (may be empty if failed)
 | | | └─→ BOOMI: Conditional execution (decision shape31, FALSE path)
 | |
 | └─→ Path 6: FsiLogout (subprocess shape13) (Downstream) - (CLEANUP)
 | └─→ READS: [process.DPP_SessionId]
 | └─→ WRITES: []
 | └─→ HTTP: [Expected: 200, Error: 500]
 | └─→ ERROR HANDLING: If fails → Log error, CONTINUE (cleanup only, non-critical)
 | └─→ RESULT: (no variables set)
 | └─→ BOOMI: Subprocess FsiLogout
 |
 ├─→ CONVERGENCE (shape6 - stop with continue=true)
 |
 ├─→ Map to CreateBreakdownTask (shape7)
 | └─→ Transforms input + lookup results to SOAP request
 |
 ├─→ Set URL and SOAPAction (shape8)
 |
 ├─→ Build SOAP Request (shape9 - notify/log current data)
 |
 ├─→ CreateBreakdownTask (shape11) (Downstream) - (MAIN OPERATION)
 | └─→ READS: [process.DPP_SessionId, process.DPP_CategoryId, process.DDP_DisciplineId, process.DPP_PriorityId, 
 | process.DPP_InstructionId, process.DPP_BuildingID, process.DPP_LocationID, input:reporterName, 
 | input:reporterEmail, input:reporterPhoneNumber, input:serviceRequestNumber, input:description, 
 | input:ticketDetails.scheduledDate, input:ticketDetails.scheduledTimeStart, input:ticketDetails.raisedDateUtc]
 | └─→ WRITES: [process.DPP_BreakdownTaskId]
 | └─→ HTTP: [Expected: 200, Error: 400/500]
 | └─→ ERROR HANDLING: If fails → Throw exception (main operation must succeed)
 | └─→ RESULT: breakdownTaskId (throws exception on failure)
 | └─→ BOOMI: Main operation (shape11) with decision shape12 checking status "20*"
 |
 ├─→ Decision (shape12): CreateBreakdownTask Status "20*"?
 | ├─→ IF TRUE (success) → Map success response (shape16) → Return Documents (shape15) [HTTP: 200] [SUCCESS]
 | | └─→ Response: { "cafmSRNumber": "CAFM-2025-12345", "sourceSRNumber": "EQ-2025-001", 
 | | "sourceOrgId": "ORG-123", "status": "Success", "message": "Work order created successfully" }
 | |
 | └─→ IF FALSE (failure) → Map error response (shape47) → Return Documents (shape17) [HTTP: 400] [FAILURE]
 | └─→ Response: { "cafmSRNumber": "", "sourceSRNumber": "EQ-2025-001", "sourceOrgId": "ORG-123", 
 | "status": "Failure", "message": "Error details from CAFM" }
 |
 └─→ CATCH Block (shape3 error path):
 |
 └─→ BRANCH (shape20) - 3 error paths
 ├─→ Path 1: Set Error Message (shape21) → Email Subprocess (shape19)
 ├─→ Path 2: Return Failure (shape18) [HTTP: 400] [ERROR]
 └─→ Path 3: Throw Exception (shape22)
```

**Note:** Detailed request/response JSON examples for all operations and return paths are documented in Section 16 (HTTP Status Codes and Return Path Responses) and Section 17 (Request/Response JSON Examples).

---

## 14. SUBPROCESS ANALYSIS

### Subprocess 1: FsiLogin (3d9db79d-15d0-4472-9f47-375ad9ab1ed2)

**Purpose:** Authenticate with CAFM system and obtain session ID

**Internal Flow:**
1. START (shape1)
2. Set URL and SOAPAction (shape3) - From defined parameters
3. Build SOAP Request (shape5) - Uses FSI_Username, FSI_Password
4. Execute Login (shape2 - connectoraction) - operation c20e5991
5. Decision (shape4): Status Code "20*"?
   - TRUE → Extract SessionId (shape8) → Stop (continue=true) [SUCCESS]
   - FALSE → Set error (shape11) → Map error (shape6) → Return "Login_Error" [ERROR]

**Return Paths:**
- **Success:** Stop (continue=true) - SessionId available in process.DPP_SessionId
- **Error:** Return Documents ("Login_Error") - Error message in process.DPP_CafmError

**Properties Written:**
- process.DPP_SessionId (on success)
- process.DPP_CafmError (on failure)

**Properties Read:**
- Defined parameters: FSI_Username, FSI_Password, Resourcepath_Login, soapaction_login

### Subprocess 2: FsiLogout (b44c26cb-ecd5-4677-a752-434fe68f2e2b)

**Purpose:** Close session with CAFM system

**Internal Flow:**
1. START (shape1)
2. Build SOAP Request (shape5) - Uses process.DPP_SessionId
3. Set URL and SOAPAction (shape4) - From defined parameters
4. Execute Logout (shape2 - connectoraction) - operation 381a025b
5. Stop (continue=true) [SUCCESS]

**Return Paths:**
- **Success:** Stop (continue=true) - Session closed

**Properties Written:**
- None

**Properties Read:**
- process.DPP_SessionId
- Defined parameters: Resourcepath_logout, soapaction_logout

### Subprocess 3: Office 365 Email (a85945c5-3004-42b9-80b1-104f465cd1fb)

**Purpose:** Send email notification (success or error)

**Internal Flow:**
1. START (shape1)
2. TRY Block (shape2)
3. Decision (shape4): DPP_HasAttachment equals "Y"?
   - TRUE → Build email with attachment (shape11-14-15-6-3) → Send (shape3)
   - FALSE → Build email without attachment (shape23-22-20-7) → Send (shape7)
4. Stop (continue=true) [SUCCESS]
5. CATCH Block (shape10) → Throw exception

**Return Paths:**
- **Success:** Stop (continue=true) - Email sent
- **Error:** Exception thrown

**Properties Read:**
- process.DPP_HasAttachment
- process.To_Email
- process.DPP_Subject
- process.DPP_MailBody
- process.DPP_Payload
- process.DPP_File_Name
- process.DPP_Process_Name
- process.DPP_AtomName
- process.DPP_ExecutionID
- process.DPP_ErrorMessage

---

## 15. CRITICAL PATTERNS IDENTIFIED

### Pattern 1: Session-Based Authentication

**Pattern:** Login → Store SessionId → Execute Operations → Logout (finally)

**Boomi Implementation:**
- Subprocess FsiLogin (shape5) executes first
- Decision shape4 checks login status (20*)
- SessionId extracted to process.DPP_SessionId (shape8)
- All SOAP operations use SessionId
- Subprocess FsiLogout (shape13) executes last

**Azure Implementation:**
- CustomAuthenticationMiddleware intercepts Function calls
- BEFORE: AuthenticateAtomicHandler → Extract SessionId → Store in RequestContext
- EXECUTE: All operations use RequestContext.GetSessionId()
- AFTER (finally): LogoutAtomicHandler → Clear RequestContext

### Pattern 2: Check-Before-Create

**Pattern:** Check if entity exists → Decision → Create only if not found

**Boomi Implementation:**
- GetBreakdownTasksByDto (shape50-51-55) checks if task exists
- Decision shape56: BreakdownTaskId equals "" (empty)?
  - FALSE (exists) → Map existing task (shape58) → Return (shape59) [EARLY EXIT]
  - TRUE (not found) → Continue to create (shape57 → convergence → CreateBreakdownTask)

**Azure Implementation:**
- GetBreakdownTasksByDtoHandler checks existence
- If task found → Return existing task (early exit)
- If not found → Continue to CreateBreakdownTaskHandler

### Pattern 3: Best-Effort Lookup with Branch Convergence

**Pattern:** Lookup operations in branch paths that converge regardless of success/failure

**Boomi Implementation:**
- Branch shape4 has multiple lookup paths (GetLocationsByDto, GetInstructionSetsByDto)
- Paths converge at shape6 (stop continue=true)
- NO decision shapes check lookup status after convergence
- CreateBreakdownTask proceeds with whatever values are available (populated or empty)

**Azure Implementation:**
- GetLocationsByDtoAtomicHandler: If fails → Log warning, set empty, CONTINUE
- GetInstructionSetsByDtoAtomicHandler: If fails → Log warning, set empty, CONTINUE
- CreateBreakdownTaskHandler uses lookup results (may be empty)
- CAFM system validates required fields (better error messages from SOR)

**Benefits:**
- ✅ Resilient: Lookup failures don't stop main operation
- ✅ Validation at right place: CAFM validates required fields
- ✅ Accurate errors: Error from CAFM (not generic lookup error)
- ✅ Matches Boomi: Same behavior as original process

### Pattern 4: Conditional Operation

**Pattern:** Execute operation only if condition met

**Boomi Implementation:**
- Decision shape31: ticketDetails.recurrence notequals "Y"?
  - TRUE (not recurring) → Skip event creation (shape30 → convergence)
  - FALSE (recurring) → CreateEvent (shape32-33-35)

**Azure Implementation:**
- Check recurrence flag in Handler
- If "Y" → CreateEventAtomicHandler
- If not "Y" → Skip event creation
- Continue regardless of event creation result

### Pattern 5: Date Formatting with Scripting

**Pattern:** Combine date and time fields, format to ISO with specific suffix

**Boomi Implementation:**
- Map function 11: Combine scheduledDate + scheduledTimeStart → Format to ISO with .0208713Z suffix
- Map function 13: Format raisedDateUtc → ISO with .0208713Z suffix

**Azure Implementation:**
- DateTimeExtensions or Handler logic to combine and format dates
- Append ".0208713Z" suffix to ISO formatted dates

---

## 16. SYSTEM LAYER IDENTIFICATION

### Third-Party Systems

| System | Type | Operations | Authentication |
|---|---|---|---|
| **CAFM (FSI Evolution)** | SOAP/XML | Authenticate, Logout, GetLocationsByDto, GetInstructionSetsByDto, GetBreakdownTasksByDto, CreateBreakdownTask, CreateEvent | Session-based (Login/Logout) |
| **Office 365 Email** | SMTP | Send email (with/without attachment) | SMTP AUTH |

### System Layer Projects

**Project 1: sys-cafm-mgmt** (CAFM System Layer)
- **SOR:** CAFM (FSI Evolution)
- **Operations:** 7 SOAP operations
- **Authentication:** Session-based middleware
- **Folder:** Create new repository or use existing sys-cafm-mgmt

**Project 2: sys-email-mgmt** (Email System Layer - OPTIONAL)
- **SOR:** Office 365 Email
- **Operations:** Send email
- **Authentication:** SMTP AUTH
- **Note:** Email is typically handled by Process Layer or shared email service. For this implementation, we'll focus on CAFM System Layer only.

---

## 17. REQUEST/RESPONSE JSON EXAMPLES

### Process Layer Entry Point

**Request JSON Example:**

```json
{
  "workOrder": [
    {
      "reporterName": "John Doe",
      "reporterEmail": "john.doe@example.com",
      "reporterPhoneNumber": "+971-50-1234567",
      "description": "Air conditioning not working in office",
      "serviceRequestNumber": "EQ-2025-001",
      "propertyName": "Building A",
      "unitCode": "A-101",
      "categoryName": "HVAC",
      "subCategory": "Air Conditioning",
      "technician": "Tech Team 1",
      "sourceOrgId": "ORG-123",
      "ticketDetails": {
        "status": "Open",
        "subStatus": "Pending",
        "priority": "High",
        "scheduledDate": "2025-01-30",
        "scheduledTimeStart": "09:00",
        "scheduledTimeEnd": "11:00",
        "recurrence": "N",
        "oldCAFMSRnumber": "",
        "raisedDateUtc": "2025-01-28T10:30:00"
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
      "sourceSRNumber": "EQ-2025-001",
      "sourceOrgId": "ORG-123",
      "status": "Success",
      "message": "Work order created successfully"
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
      "sourceSRNumber": "EQ-2025-001",
      "sourceOrgId": "ORG-123",
      "status": "Failure",
      "message": "CAFM Log In Failed. CAFM Log In API Responded with Blank Response"
    }
  ]
}
```

### Downstream System Layer Calls

**Operation: Authenticate (Login)**

**Request SOAP:**

```xml
<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" xmlns:ns="http://www.fsi.co.uk/services/evolution/04/09">
   <soapenv:Header/>
   <soapenv:Body>
      <ns:Authenticate>
         <ns:loginName>{{FSI_Username}}</ns:loginName>
         <ns:password>{{FSI_Password}}</ns:password>
      </ns:Authenticate>
   </soapenv:Body>
</soapenv:Envelope>
```

**Response SOAP (Success - HTTP 200):**

```xml
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
   <soap:Body>
      <AuthenticateResponse xmlns="http://www.fsi.co.uk/services/evolution/04/09">
         <AuthenticateResult>
            <SessionId>abc123xyz789</SessionId>
         </AuthenticateResult>
      </AuthenticateResponse>
   </soap:Body>
</soap:Envelope>
```

**Response SOAP (Error - HTTP 401):**

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

**Operation: GetLocationsByDto**

**Request SOAP:**

```xml
<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/"
                  xmlns:ns="http://www.fsi.co.uk/services/evolution/04/09"
                  xmlns:fsi1="http://schemas.datacontract.org/2004/07/Fsi.Concept.Contracts.Entities.ServiceModel">
    <soapenv:Header/>
    <soapenv:Body>
        <ns:GetLocationsByDto>
            <ns:sessionId>{{SessionId}}</ns:sessionId>
            <ns:locationDto>
                <fsi1:BarCode>{{UnitCode}}</fsi1:BarCode>
            </ns:locationDto>
        </ns:GetLocationsByDto>
    </soapenv:Body>
</soapenv:Envelope>
```

**Response SOAP (Success - HTTP 200):**

```xml
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
   <soap:Body>
      <GetLocationsByDtoResponse xmlns="http://www.fsi.co.uk/services/evolution/04/09">
         <GetLocationsByDtoResult>
            <LocationDto>
               <BuildingId>456</BuildingId>
               <LocationId>789</LocationId>
            </LocationDto>
         </GetLocationsByDtoResult>
      </GetLocationsByDtoResponse>
   </soap:Body>
</soap:Envelope>
```

**Operation: CreateBreakdownTask**

**Request SOAP (abbreviated - full version has 50+ fields):**

```xml
<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" xmlns:ns="http://www.fsi.co.uk/services/evolution/04/09">
    <soapenv:Header/>
    <soapenv:Body>
        <ns:CreateBreakdownTask>
            <ns:sessionId>{{SessionId}}</ns:sessionId>
            <ns:breakdownTaskDto>
                <ReporterName>{{ReporterName}}</ReporterName>
                <BDET_EMAIL>{{ReporterEmail}}</BDET_EMAIL>
                <Phone>{{ReporterPhoneNumber}}</Phone>
                <CallId>{{ServiceRequestNumber}}</CallId>
                <CategoryId>{{CategoryId}}</CategoryId>
                <DisciplineId>{{DisciplineId}}</DisciplineId>
                <PriorityId>{{PriorityId}}</PriorityId>
                <InstructionId>{{InstructionId}}</InstructionId>
                <BuildingId>{{BuildingId}}</BuildingId>
                <LocationId>{{LocationId}}</LocationId>
                <LongDescription>{{Description}}</LongDescription>
                <ScheduledDateUtc>{{ScheduledDateUtc}}</ScheduledDateUtc>
                <RaisedDateUtc>{{RaisedDateUtc}}</RaisedDateUtc>
                <ContractId>1</ContractId>
                <BDET_CALLER_SOURCE_ID>{{SourceOrgId}}</BDET_CALLER_SOURCE_ID>
            </ns:breakdownTaskDto>
        </ns:CreateBreakdownTask>
    </soapenv:Body>
</soapenv:Envelope>
```

**Response SOAP (Success - HTTP 200):**

```xml
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
   <soap:Body>
      <CreateBreakdownTaskResponse xmlns="http://www.fsi.co.uk/services/evolution/04/09">
         <CreateBreakdownTaskResult>
            <PrimaryKeyId>12345</PrimaryKeyId>
         </CreateBreakdownTaskResult>
      </CreateBreakdownTaskResponse>
   </soap:Body>
</soap:Envelope>
```

---

## 18. FUNCTION EXPOSURE DECISION TABLE

### Decision Process

For EACH operation, answer 5 questions:

**Q1:** Can Process Layer invoke independently?  
**Q2:** Decision/conditional logic present?  
**Q2a:** Is decision same SOR (all operations in if/else same System Layer)?  
**Q3:** Only field extraction/lookup for another operation?  
**Q4:** Complete business operation Process Layer needs?

### Decision Table

| Operation | Q1: Independent? | Q2: Decision? | Q2a: Same SOR? | Q3: Lookup? | Q4: Complete Op? | Conclusion | Reasoning |
|---|---|---|---|---|---|---|---|
| Login (Authenticate) | NO | None | N/A | N/A | N/A | **Atomic (Middleware)** | Auth handled by middleware |
| Logout | NO | None | N/A | N/A | N/A | **Atomic (Middleware)** | Auth handled by middleware |
| GetBreakdownTasksByDto | YES | YES (check exists) | YES (same SOR) | NO | YES | **Function** | Check-before-create: PL decides skip/proceed |
| GetLocationsByDto | NO | None | YES | YES | NO | **Atomic (Internal)** | Lookup for CreateBreakdownTask (same SOR) |
| GetInstructionSetsByDto | NO | None | YES | YES | NO | **Atomic (Internal)** | Lookup for CreateBreakdownTask (same SOR) |
| CreateBreakdownTask | YES | None | N/A | NO | YES | **Function** | Main operation PL calls |
| CreateEvent | YES | YES (conditional) | N/A | NO | YES | **Function** | PL decides based on recurrence flag |
| Office 365 Email | NO | None | N/A | N/A | N/A | **Not System Layer** | Email handled separately |

### Summary

**I will create 3 Azure Functions for CAFM System Layer:**

1. **GetBreakdownTasksByDtoAPI** - Check if work order exists (check-before-create pattern)
2. **CreateBreakdownTaskAPI** - Create work order (main operation)
3. **CreateEventAPI** - Create recurring event (conditional operation)

**Because:**
- GetBreakdownTasksByDto has decision shape (check-before-create) → Process Layer decides skip/proceed
- CreateBreakdownTask is main operation → Process Layer calls independently
- CreateEvent has conditional logic (recurrence flag) → Process Layer decides based on flag

**Per Rule 1066:** Business decisions → Process Layer when operations can be invoked independently with conditional logic.

**Functions Purposes:**
- GetBreakdownTasksByDtoAPI: Check work order existence (returns existing task or empty)
- CreateBreakdownTaskAPI: Create work order with all details (orchestrates internal lookups)
- CreateEventAPI: Create recurring event for work order

**Internal Atomic Handlers:**
- AuthenticateAtomicHandler (middleware)
- LogoutAtomicHandler (middleware)
- GetLocationsByDtoAtomicHandler (internal lookup)
- GetInstructionSetsByDtoAtomicHandler (internal lookup)

**Auth Method:** Session-based middleware (CustomAuthenticationMiddleware with AuthenticateAtomicHandler + LogoutAtomicHandler)

---

## 19. VALIDATION CHECKLIST

### Data Dependencies
- [x] All property WRITES identified (21 properties)
- [x] All property READS identified (11 properties)
- [x] Dependency graph built (3 chains)
- [x] Execution order satisfies all dependencies (no read-before-write)

### Decision Analysis
- [x] ALL decision shapes inventoried (5 decisions)
- [x] BOTH TRUE and FALSE paths traced to termination
- [x] Pattern type identified for each decision
- [x] Early exits identified and documented (2 early exits)
- [x] Convergence points identified (3 convergence points)

### Branch Analysis
- [x] Each branch classified as parallel or sequential
- [x] **CRITICAL:** Branch contains API calls → Classification is SEQUENTIAL
- [x] **SELF-CHECK:** Did I check for API calls in branch paths? (YES)
- [x] **SELF-CHECK:** Did I classify or assume? (Classified)
- [x] Dependency order built using topological sort
- [x] Each path traced to terminal point
- [x] Convergence points identified
- [x] Execution continuation point determined

### Sequence Diagram
- [x] Format follows required structure
- [x] Each operation shows READS and WRITES
- [x] Decisions show both TRUE and FALSE paths
- [x] Check-before-create patterns shown correctly
- [x] **SELF-CHECK:** Did I verify check happens BEFORE create? (YES)
- [x] **CROSS-VALIDATION:** Sequence diagram matches control flow graph
- [x] **CROSS-VALIDATION:** Execution order matches dependency graph
- [x] Early exits marked [EARLY EXIT]
- [x] Conditional execution marked
- [x] Subprocess internal flows documented
- [x] Subprocess return paths mapped

### Input/Output Structure Analysis
- [x] Entry point operation identified (de68dad0)
- [x] Request profile identified and loaded (af096014)
- [x] Request profile structure analyzed (JSON)
- [x] Array vs single object detected (Array)
- [x] Array cardinality documented (minOccurs: 0, maxOccurs: -1)
- [x] ALL request fields extracted (19 fields)
- [x] Request field paths documented
- [x] Request field mapping table generated
- [x] Response profile identified and loaded (9e542ed5)
- [x] Response profile structure analyzed (JSON)
- [x] ALL response fields extracted (5 fields)
- [x] Response field mapping table generated
- [x] Document processing behavior determined (array splitting)

### Map Analysis
- [x] ALL map files identified and loaded (6 maps)
- [x] SOAP request maps identified (1 map: CreateBreakdownTask)
- [x] Field mappings extracted from map
- [x] Profile vs map field name discrepancies documented
- [x] Map field names marked as AUTHORITATIVE
- [x] Scripting functions analyzed (date formatting)
- [x] Static values identified
- [x] Process property mappings documented
- [x] Element names extracted (breakdownTaskDto)
- [x] Namespace prefixes verified

### HTTP Status Codes and Return Paths
- [x] All return paths documented with HTTP status codes (2 paths)
- [x] Response JSON examples provided for each return path
- [x] Populated fields documented for each return path
- [x] Decision conditions leading to each return documented
- [x] Error codes and success codes documented
- [x] Downstream operation HTTP status codes documented

### Function Exposure Decision
- [x] Decision table completed for ALL operations (10 operations)
- [x] All 5 questions answered for EACH operation
- [x] Reasoning documented for EACH decision
- [x] All verification questions YES or N/A
- [x] Summary written (3 Functions + reasoning)
- [x] No internal lookups as Functions
- [x] No Login/Logout as Functions
- [x] Business decisions assigned to Process Layer

---

## 20. PROCESS LAYER ↔ SYSTEM LAYER ORCHESTRATION DIAGRAM

### 20.1 Overview

This diagram shows how Process Layer orchestrates CAFM System Layer APIs to implement the complete work order creation workflow extracted from the Boomi process.

**Key Principle:** System Layer provides atomic "Lego block" APIs. Process Layer orchestrates them based on business logic.

**Layer Responsibilities:**
- **Process Layer:** Business orchestration (check-before-create, conditional event creation, batch processing), decision-making, error aggregation
- **System Layer:** Atomic operations (CAFM abstraction), authentication management (session-based middleware), SOAP handling

**Based on:**
- Section 10 (Decision Analysis): 5 key decisions (login status, task exists, create status, recurrence, attachment)
- Section 13.1 (Operation Classification): 3 Functions + 4 Atomic Handlers
- Section 13.2 (Sequence Diagram): Complete execution flow with error handling
- Section 15 (Critical Patterns): Session-based auth, check-before-create, best-effort lookup, conditional operation
- Section 18 (Function Exposure Decision): 3 Functions + 4 Atomic Handlers

### 20.2 Complete Orchestration Flow

```
Process Layer
 │
 │ Receive array of work orders
 │
 ├─→ LOOP: foreach (workOrder in workOrders)
 │ │
 │ ├─→ DECISION 1: Check Existence
 │ │ └─→ Call System Layer: GetBreakdownTasksByDtoAPI
 │ │ └─→ if (taskExists) return existing task [EARLY EXIT]
 │ │ └─→ else proceed to create
 │ │
 │ ├─→ DECISION 2: Create Work Order
 │ │ └─→ Call System Layer: CreateBreakdownTaskAPI
 │ │ └─→ Handler orchestrates internal lookups (GetLocationsByDto, GetInstructionSetsByDto)
 │ │
 │ ├─→ DECISION 3: Check Recurrence Flag
 │ │ └─→ if (recurrence == "Y") Call System Layer: CreateEventAPI
 │ │ └─→ else skip event creation
 │ │
 │ └─→ Add to results (success or skip)
 │
 └─→ Return aggregated results

Authentication: CustomAuthenticationMiddleware handles login/logout automatically
```

### 20.3 Operation-Level Orchestration

#### Operation 1: GetBreakdownTasksByDtoAPI

```
Process Layer
 │
 │ Call System Layer: GetBreakdownTasksByDtoAPI
 │ Request: { "serviceRequestNumber": "EQ-2025-001" }
 ↓
System Layer: GetBreakdownTasksByDtoAPI
 │
 │ [CustomAuthentication] Middleware intercepts
 │ ├─→ BEFORE: AuthenticateAtomicHandler → Extract SessionId → Store in RequestContext
 │ ├─→ EXECUTE: Function
 │ └─→ AFTER (finally): LogoutAtomicHandler → Clear RequestContext
 │
 │ IBreakdownTaskMgmt (Service Interface)
 ↓
 BreakdownTaskMgmtService
 ↓
 GetBreakdownTasksByDtoHandler
 │
 ├─→ Read SessionId from RequestContext
 ├─→ GetBreakdownTasksByDtoAtomicHandler
 | └─→ SOAP: GetBreakdownTasksByDto (sessionId + serviceRequestNumber)
 | └─→ Response: { "PrimaryKeyId": "12345" } or empty
 |
 └─→ Map ApiResDTO → GetBreakdownTasksByDtoResDTO
 |
 ↓
Returns to Process Layer
 │
 │ Process Layer evaluates response
 │ if (taskExists) return existing [EARLY EXIT]
 │ else proceed to create
```

#### Operation 2: CreateBreakdownTaskAPI

```
Process Layer
 │
 │ Call System Layer: CreateBreakdownTaskAPI
 │ Request: { "reporterName": "John Doe", "serviceRequestNumber": "EQ-2025-001", ... }
 ↓
System Layer: CreateBreakdownTaskAPI
 │
 │ [CustomAuthentication] Middleware intercepts
 │ ├─→ BEFORE: AuthenticateAtomicHandler → Extract SessionId → Store in RequestContext
 │ ├─→ EXECUTE: Function
 │ └─→ AFTER (finally): LogoutAtomicHandler → Clear RequestContext
 │
 │ IBreakdownTaskMgmt (Service Interface)
 ↓
 BreakdownTaskMgmtService
 ↓
 CreateBreakdownTaskHandler
 │
 ├─→ Read SessionId from RequestContext
 │
 ├─→ INTERNAL STEP 1: GetLocationsByDtoAtomicHandler (BEST-EFFORT LOOKUP)
 │ | └─→ SOAP: GetLocationsByDto (sessionId + unitCode)
 │ | └─→ Response: BuildingId, LocationId
 │ | └─→ If SUCCESS: buildingId = "456", locationId = "789"
 │ | └─→ If FAIL: Log warning, buildingId = empty, locationId = empty, CONTINUE
 │ |
 │ ├─→ INTERNAL STEP 2: GetInstructionSetsByDtoAtomicHandler (BEST-EFFORT LOOKUP)
 │ | └─→ SOAP: GetInstructionSetsByDto (sessionId + subCategory)
 │ | └─→ Response: CategoryId, DisciplineId, PriorityId, InstructionId
 │ | └─→ If SUCCESS: categoryId = "123", disciplineId = "456", priorityId = "789", instructionId = "101"
 │ | └─→ If FAIL: Log warning, all IDs = empty, CONTINUE
 │ |
 │ ├─→ INTERNAL STEP 3: CreateBreakdownTaskAtomicHandler (MAIN OPERATION)
 │ | └─→ Format dates (ScheduledDateUtc, RaisedDateUtc)
 │ | └─→ SOAP: CreateBreakdownTask (sessionId + all fields + lookup IDs)
 │ | └─→ Uses: buildingId, locationId, categoryId, disciplineId, priorityId, instructionId (may be empty)
 │ | └─→ Response: PrimaryKeyId (BreakdownTaskId)
 │ | └─→ If FAIL: Throw exception (main operation must succeed)
 │ |
 │ └─→ Map ApiResDTO → CreateBreakdownTaskResDTO
 |
 ↓
Returns to Process Layer
 │
 │ Process Layer stores breakdownTaskId
 │ Process Layer checks recurrence flag
```

#### Operation 3: CreateEventAPI

```
Process Layer
 │
 │ if (recurrence == "Y") Call System Layer: CreateEventAPI
 │ Request: { "breakdownTaskId": "12345" }
 ↓
System Layer: CreateEventAPI
 │
 │ [CustomAuthentication] Middleware intercepts
 │ ├─→ BEFORE: AuthenticateAtomicHandler → Extract SessionId → Store in RequestContext
 │ ├─→ EXECUTE: Function
 │ └─→ AFTER (finally): LogoutAtomicHandler → Clear RequestContext
 │
 │ IBreakdownTaskMgmt (Service Interface)
 ↓
 BreakdownTaskMgmtService
 ↓
 CreateEventHandler
 │
 ├─→ Read SessionId from RequestContext
 ├─→ CreateEventAtomicHandler
 | └─→ SOAP: CreateEvent (sessionId + breakdownTaskId)
 | └─→ Response: { "PrimaryKeyId": "67890" }
 | └─→ If FAIL: Log warning, CONTINUE (task already created)
 |
 └─→ Map ApiResDTO → CreateEventResDTO
 |
 ↓
Returns to Process Layer
 │
 │ Process Layer continues (event creation optional)
```

### 20.4 System Layer Internal Orchestration

#### CreateBreakdownTaskHandler Internal Flow

Handler orchestrates 3 internal operations (same SOR: CAFM):

```
Handler Entry
 │
 ├─→ STEP 1: GetLocationsByDtoAtomicHandler (BEST-EFFORT LOOKUP)
 │ | └─→ SOAP: GetLocationsByDto (sessionId + unitCode)
 │ | └─→ Classification: BEST-EFFORT LOOKUP
 │ | └─→ Error Handling: If fails → Log warning, set empty values, CONTINUE
 │ | └─→ Result: buildingId, locationId (populated or empty)
 │ |
 │ ├─→ STEP 2: GetInstructionSetsByDtoAtomicHandler (BEST-EFFORT LOOKUP)
 │ | └─→ SOAP: GetInstructionSetsByDto (sessionId + subCategory)
 │ | └─→ Classification: BEST-EFFORT LOOKUP
 │ | └─→ Error Handling: If fails → Log warning, set empty values, CONTINUE
 │ | └─→ Result: categoryId, disciplineId, priorityId, instructionId (populated or empty)
 │ |
 │ └─→ STEP 3: CreateBreakdownTaskAtomicHandler (MAIN OPERATION)
 | └─→ SOAP: CreateBreakdownTask (sessionId + all fields + lookup IDs)
 | └─→ Uses: buildingId, locationId, categoryId, disciplineId, priorityId, instructionId (may be empty)
 | └─→ Classification: MAIN OPERATION
 | └─→ Error Handling: If fails → Throw exception (main operation must succeed)
 | └─→ Result: breakdownTaskId (throws exception on failure)
```

**Key Point:** Process Layer sees single API call (CreateBreakdownTaskAPI). System Layer Handler handles internal orchestration of lookups + main operation.

### 20.5 Authentication Flow

```
Process Layer calls System Layer API
 │
 ↓
CustomAuthenticationMiddleware intercepts
 │
 ├─→ BEFORE function execution:
 │ | ├─→ Retrieve credentials from KeyVault (FSI_Username, FSI_Password)
 │ | ├─→ Call AuthenticateAtomicHandler
 │ | | └─→ SOAP: Authenticate (username, password)
 │ | | └─→ Response: SessionId
 │ | ├─→ Extract SessionId from SOAP response
 │ | └─→ Store in RequestContext.SetSessionId(sessionId)
 │ |
 │ ├─→ EXECUTE function:
 │ | └─→ All operations use RequestContext.GetSessionId()
 │ |
 │ └─→ AFTER function execution (finally block):
 | ├─→ Call LogoutAtomicHandler
 | | └─→ SOAP: Logout (sessionId)
 | └─→ Clear RequestContext
 |
 ↓
Response to Process Layer
```

### 20.6 Error Handling Flows

#### Scenario 1: Authentication Failure

```
Process Layer calls System Layer
 │
 ↓
CustomAuthenticationMiddleware
 │
 ├─→ AuthenticateAtomicHandler
 │ └─→ SOAP: Authenticate
 │ └─→ Response: HTTP 401 or empty SessionId
 │
 ├─→ Throw DownStreamApiFailureException
 │
 ↓
ExceptionHandlerMiddleware catches
 │
 └─→ Returns BaseResponseDTO with error (HTTP 401)
 |
 ↓
Process Layer receives error response
 │
 └─→ Process Layer decides: Log, notify, skip this work order, continue to next
```

#### Scenario 2: Best-Effort Lookup Failure

```
CreateBreakdownTaskHandler
 │
 ├─→ GetLocationsByDtoAtomicHandler
 │ └─→ SOAP: GetLocationsByDto
 │ └─→ Response: HTTP 404 (location not found)
 │
 ├─→ Check: if (!response.IsSuccessStatusCode)
 │ | └─→ Log warning: "Location lookup failed - Continuing with empty values"
 │ | └─→ buildingId = string.Empty
 │ | └─→ locationId = string.Empty
 │ | └─→ CONTINUE (do not throw)
 │ |
 │ └─→ else
 | └─→ Extract buildingId, locationId from response
 |
 ├─→ GetInstructionSetsByDtoAtomicHandler
 │ └─→ (Same pattern: fail → log warning → set empty → continue)
 │
 ├─→ CreateBreakdownTaskAtomicHandler
 │ └─→ SOAP: CreateBreakdownTask (with all fields, some may be empty)
 │ └─→ CAFM system validates required fields
 │ └─→ If CAFM rejects: Returns error (better error message from SOR)
 │ └─→ If CAFM accepts: Returns BreakdownTaskId
 |
 └─→ Result: Either success or error from CAFM (not generic lookup error)
```

**Benefits:**
- ✅ Resilient: Lookup failures don't stop main operation
- ✅ Validation at right place: CAFM validates required fields
- ✅ Accurate errors: Error from CAFM (not generic "lookup failed")
- ✅ Matches Boomi: Same behavior as original process (branch convergence pattern)

#### Scenario 3: Main Operation Failure

```
CreateBreakdownTaskHandler
 │
 ├─→ CreateBreakdownTaskAtomicHandler
 │ └─→ SOAP: CreateBreakdownTask
 │ └─→ Response: HTTP 400 (validation error from CAFM)
 │
 ├─→ Check: if (!response.IsSuccessStatusCode)
 │ └─→ Throw DownStreamApiFailureException
 |
 ↓
ExceptionHandlerMiddleware catches
 │
 └─→ Returns BaseResponseDTO with error (HTTP 400)
 |
 ↓
Process Layer receives error response
 │
 └─→ Process Layer decides: Log, notify, mark this work order as failed, continue to next
```

#### Scenario 4: Conditional Operation Failure

```
CreateEventHandler
 │
 ├─→ CreateEventAtomicHandler
 │ └─→ SOAP: CreateEvent
 │ └─→ Response: HTTP 500 (server error)
 │
 ├─→ Check: if (!response.IsSuccessStatusCode)
 │ | └─→ Log warning: "Event creation failed - Work order already created successfully"
 │ | └─→ eventId = string.Empty
 │ | └─→ CONTINUE (do not throw)
 │ |
 │ └─→ else
 | └─→ Extract eventId from response
 |
 └─→ Return success (with or without eventId)
```

**Rationale:** Work order already created successfully. Event creation is optional enhancement. Failure doesn't invalidate work order creation.

#### Scenario 5: Cleanup Operation Failure

```
CustomAuthenticationMiddleware (finally block)
 │
 ├─→ LogoutAtomicHandler
 │ └─→ SOAP: Logout
 │ └─→ Response: HTTP 500 (server error)
 │
 ├─→ try { logout(...); }
 │ └─→ catch (ex) { _logger.Error(ex, "Logout failed - Session may remain active"); }
 │
 └─→ CONTINUE (do not throw, do not fail the request)
```

**Rationale:** Cleanup only. Session will expire naturally. Logout failure shouldn't fail the entire request.

### 20.7 Data Flow Diagram

```
┌─────────────────────────────────────┐
│ EXPERIENCE LAYER │
│ (Mobile, Web, IoT) │
└─────────────────────────────────────┘
 │ HTTPS
 ↓
┌─────────────────────────────────────┐
│ PROCESS LAYER │
│ Azure Function App │
│ - Batch processing (loop array) │
│ - Check-before-create decision │
│ - Conditional event creation │
│ - Error aggregation │
└─────────────────────────────────────┘
 │ HTTPS (Internal)
 ↓
┌─────────────────────────────────────┐
│ SYSTEM LAYER (CAFM) │
│ Azure Function App │
│ - 3 Functions exposed │
│ - Session-based auth middleware │
│ - Internal lookup orchestration │
│ - SOAP envelope handling │
└─────────────────────────────────────┘
 │ SOAP/XML
 ↓
┌─────────────────────────────────────┐
│ SYSTEM OF RECORD │
│ CAFM (FSI Evolution) │
└─────────────────────────────────────┘

Shared Services:
- Azure KeyVault (FSI credentials)
- Azure Redis Cache (session caching)
- Application Insights (monitoring)
```

### 20.8 Decision Ownership Matrix

| Decision Point | Owner | Rationale | Implementation |
|---|---|---|---|
| Check if work order exists (GetBreakdownTasksByDto) | Process Layer | Cross-operation decision (if exists skip, else create) | Call GetBreakdownTasksByDtoAPI, evaluate response, decide next step |
| Create work order (CreateBreakdownTask) | Process Layer | Main operation invoked independently | Call CreateBreakdownTaskAPI |
| Check recurrence flag | Process Layer | Conditional operation (if flag create event) | Check recurrence field, conditionally call CreateEventAPI |
| Internal lookup error handling (GetLocationsByDto, GetInstructionSetsByDto) | System Layer | Same-SOR best-effort pattern | Handler orchestrates: if lookup fails → log warning → set empty → continue to main operation |
| Login status check | System Layer | Authentication validation | Middleware: if login fails → throw exception |
| CreateBreakdownTask status check | System Layer | Main operation validation | Handler: if create fails → throw exception |
| CreateEvent status check | System Layer | Conditional operation validation | Handler: if event fails → log warning → continue |
| Logout error handling | System Layer | Cleanup operation | Middleware finally: if logout fails → log error → continue |

### 20.9 Layer Responsibilities Summary

#### Process Layer Responsibilities

**What Process Layer DOES:**
- Loop through work order array (batch processing)
- Call GetBreakdownTasksByDtoAPI to check if work order exists
- Evaluate result: If exists, skip creation and return existing; if not, proceed
- Call CreateBreakdownTaskAPI to create work order
- Check recurrence flag: If "Y", call CreateEventAPI; if not, skip
- Aggregate results from all work orders (success, skip, error)
- Handle errors and send email notifications
- Implement business workflow orchestration

**What Process Layer DOES NOT DO:**
- Make direct SOAP calls to CAFM
- Handle CAFM session authentication
- Transform SOAP envelopes
- Perform internal lookups (GetLocationsByDto, GetInstructionSetsByDto)
- Deserialize SOAP responses
- Manage CAFM-specific error codes

#### System Layer Responsibilities

**What System Layer DOES:**
- Expose 3 Azure Functions (GetBreakdownTasksByDto, CreateBreakdownTask, CreateEvent)
- Handle CAFM session-based authentication (middleware: login before, logout after)
- Execute SOAP operations (7 operations total: Authenticate, Logout, GetBreakdownTasksByDto, GetLocationsByDto, GetInstructionSetsByDto, CreateBreakdownTask, CreateEvent)
- Perform best-effort lookups (GetLocationsByDto, GetInstructionSetsByDto) - internal to CreateBreakdownTaskHandler
- Build SOAP envelopes with correct namespaces and field names
- Deserialize SOAP responses to JSON DTOs
- Return standardized BaseResponseDTO
- Handle SOAP-specific errors and status codes

**What System Layer DOES NOT DO:**
- Batch processing (loop through arrays)
- Cross-operation business decisions (check-before-create)
- Conditional operation decisions (recurrence flag check)
- Email notifications
- Aggregate results from multiple work orders
- Implement business workflows

### 20.10 Benefits of This Architecture

#### 1. Separation of Concerns

**Example from this process:**
- **CAFM changes:** If CAFM API changes (new required field, different authentication), only System Layer needs update
- **Business logic changes:** If check-before-create logic changes (e.g., check multiple systems), only Process Layer needs update
- **Impact isolation:** CAFM SOAP envelope changes don't affect Process Layer (System Layer abstracts SOAP complexity)

#### 2. Reusability

**Example from this process:**
- **CreateBreakdownTaskAPI** can be reused by:
  - Other work order creation processes (different sources: mobile app, web portal, IoT devices)
  - Bulk work order creation processes
  - Work order update processes (if they need to create first)
- **GetBreakdownTasksByDtoAPI** can be reused by:
  - Work order query processes
  - Work order status check processes
  - Work order update processes (check before update)

#### 3. Maintainability

**Change Scenarios:**
- **Scenario 1:** CAFM requires new field (e.g., "CostCenter")
  - **Impact:** System Layer only (add field to CreateBreakdownTask SOAP envelope)
  - **No Impact:** Process Layer (just passes new field through)
- **Scenario 2:** Business rule changes (e.g., check 2 systems before create)
  - **Impact:** Process Layer only (call 2 System Layer APIs)
  - **No Impact:** System Layer (same atomic operations)

#### 4. Testability

**Testing Approach:**
- **System Layer:** Test with mock SOAP responses (no CAFM connection needed)
- **Process Layer:** Test with mock System Layer API responses (no CAFM connection needed)
- **Integration:** Test System Layer + CAFM (isolated from Process Layer)
- **End-to-End:** Test all layers together

### 20.11 Reference Mapping

#### Boomi Shapes → Azure Components

| Boomi Component | Azure Component | Layer | File |
|---|---|---|---|
| shape5 (FsiLogin subprocess) | AuthenticateAtomicHandler + CustomAuthenticationMiddleware | System | Middleware/CustomAuthenticationMiddleware.cs, AtomicHandlers/AuthenticateAtomicHandler.cs |
| shape13 (FsiLogout subprocess) | LogoutAtomicHandler + CustomAuthenticationMiddleware | System | AtomicHandlers/LogoutAtomicHandler.cs |
| shape50-51-55 (GetBreakdownTasksByDto) | GetBreakdownTasksByDtoAPI + Handler + Atomic | System | Functions/GetBreakdownTasksByDtoAPI.cs |
| shape23-24-25 (GetLocationsByDto) | GetLocationsByDtoAtomicHandler (internal) | System | AtomicHandlers/GetLocationsByDtoAtomicHandler.cs |
| shape26-27-28 (GetInstructionSetsByDto) | GetInstructionSetsByDtoAtomicHandler (internal) | System | AtomicHandlers/GetInstructionSetsByDtoAtomicHandler.cs |
| shape7-8-9-11 (CreateBreakdownTask) | CreateBreakdownTaskAPI + Handler + Atomic | System | Functions/CreateBreakdownTaskAPI.cs |
| shape32-33-35 (CreateEvent) | CreateEventAPI + Handler + Atomic | System | Functions/CreateEventAPI.cs |
| shape56 (Decision: task exists) | Process Layer logic | Process | (Not implemented - Process Layer responsibility) |
| shape31 (Decision: recurrence) | Process Layer logic | Process | (Not implemented - Process Layer responsibility) |

#### Phase 1 Sections → Code Components

| Phase 1 Section | Code Component | File |
|---|---|---|
| Section 2 (Input Structure) | CreateBreakdownTaskReqDTO | DTO/CreateBreakdownTaskDTO/CreateBreakdownTaskReqDTO.cs |
| Section 3 (Response Structure) | CreateBreakdownTaskResDTO | DTO/CreateBreakdownTaskDTO/CreateBreakdownTaskResDTO.cs |
| Section 5 (Map Analysis) | CreateBreakdownTask.xml | SoapEnvelopes/CreateBreakdownTask.xml |
| Section 13.1 (Classification) | Handler error handling | Handlers/*.cs |
| Section 13.2 (Sequence Diagram) | Handler orchestration | Handlers/CreateBreakdownTaskHandler.cs |
| Section 15 (Authentication Pattern) | CustomAuthenticationMiddleware | Middleware/CustomAuthenticationMiddleware.cs |
| Section 15 (Best-Effort Pattern) | CreateBreakdownTaskHandler internal orchestration | Handlers/CreateBreakdownTaskHandler.cs |
| Section 18 (Function Exposure) | 3 Functions + 4 Atomic Handlers | Functions/, AtomicHandlers/ |

### 20.12 Self-Check Results

✅ All System Layer Functions shown: YES (3 Functions: GetBreakdownTasksByDto, CreateBreakdownTask, CreateEvent)  
✅ All decisions assigned: YES (8 decisions assigned to Process or System Layer)  
✅ Authentication flow shown: YES (Session-based with middleware)  
✅ Error handling scenarios complete: YES (5 scenarios: auth, best-effort, main op, conditional, cleanup)  
✅ Internal orchestration shown: YES (CreateBreakdownTaskHandler orchestrates 2 internal lookups)  
✅ Decision ownership matrix complete: YES (8 decisions with rationale)  
✅ Layer responsibilities documented: YES (specific examples from this process)  
✅ Reference mapping complete: YES (Boomi shapes → Azure components, Phase 1 sections → Code files)

**Section 20 Status:** ✅ COMPLETE

---

## 21. PHASE 1 COMPLETION STATUS

### Critical Sections Verification

- [x] **Section 2:** Input Structure Analysis - COMPLETE (19 fields mapped)
- [x] **Section 3:** Response Structure Analysis - COMPLETE (5 fields mapped)
- [x] **Section 5:** Map Analysis - COMPLETE (1 SOAP map analyzed)
- [x] **Section 13.1:** Operation Classification Table - COMPLETE (7 operations classified)
- [x] **Section 13.2:** Enhanced Sequence Diagram - COMPLETE (all operations with error handling)
- [x] **Section 18:** Function Exposure Decision Table - COMPLETE (3 Functions identified)
- [x] **Section 20:** Orchestration Diagram - COMPLETE (all subsections present)

### Verification Question

**Can a developer generate code from Section 13 without making assumptions?**

**Answer:** ✅ YES

**Proof:**
- Section 13.1 specifies error handling for EACH operation (throw vs continue)
- Section 13.2 shows classification for EACH operation (AUTHENTICATION, BEST-EFFORT LOOKUP, MAIN OPERATION, CONDITIONAL, CLEANUP)
- Section 13.2 shows result for EACH operation (variables: populated or empty / throws exception)
- Section 13.2 shows Boomi references for EACH operation (shape numbers, convergence points)
- No ambiguity: Developer knows exactly what to do for each operation

### Self-Check Questions (ALL YES)

1. ❓ Did I analyze ALL map files? **YES** (6 maps loaded, 1 SOAP request map analyzed)
2. ❓ Did I identify SOAP request maps? **YES** (CreateBreakdownTask map identified)
3. ❓ Did I extract actual field names from maps? **YES** (CategoryId, DisciplineId, PriorityId, InstructionId)
4. ❓ Did I compare profile field names vs map field names? **YES** (Discrepancies documented)
5. ❓ Did I mark map field names as AUTHORITATIVE? **YES** (Section 5)
6. ❓ Did I analyze scripting functions in maps? **YES** (Date formatting functions 11, 13)
7. ❓ Did I extract element names from maps? **YES** (breakdownTaskDto)
8. ❓ Did I verify namespace prefixes from message shapes? **YES** (ns, fsi1, fsi2)
9. ❓ Did I extract HTTP status codes for all return paths? **YES** (2 return paths documented)
10. ❓ Did I document response JSON for each return path? **YES** (Success and Failure examples)
11. ❓ Did I document populated fields for each return path? **YES** (Section 6)
12. ❓ Did I extract HTTP status codes for downstream operations? **YES** (Section 6 table)
13. ❓ Did I create request/response JSON examples? **YES** (Section 17)

### Phase 1 Status

**Status:** ✅ COMPLETE  
**Ready for Phase 2:** ✅ YES  
**Reason:** All mandatory sections complete, operation classification table complete, enhanced sequence diagram complete with error handling for all operations, no assumptions needed for code generation.

---

**END OF PHASE 1 DOCUMENT**

**Document Version:** 1.0  
**Created:** 2026-01-28  
**Purpose:** Complete technical specification for CAFM System Layer code generation  
**Total Sections:** 21  
**Total Operations:** 10 (7 SOAP, 2 SMTP, 1 Web Service)  
**Functions to Create:** 3 (GetBreakdownTasksByDto, CreateBreakdownTask, CreateEvent)  
**Atomic Handlers:** 4 (Authenticate, Logout, GetLocationsByDto, GetInstructionSetsByDto)
