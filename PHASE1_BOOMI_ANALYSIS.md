# PHASE 1: BOOMI PROCESS ANALYSIS - Create Work Order CAFM

## Process Overview
- **Process Name:** Create Work Order from EQ+ to CAFM
- **Process ID:** cf0ab01d-2ce4-4588-8265-54fc4290368a
- **Type:** Web Service Listener (singlejson input)
- **Target System:** CAFM (FSI Evolution) - SOAP-based integration
- **Authentication:** Session-based (Login → Operations → Logout)

---

## 1. Operations Inventory

### Third-Party System Operations (CAFM/FSI - System Layer)

| Operation ID | Operation Name | Type | Method | Purpose |
|---|---|---|---|---|
| c20e5991-4d70-47f7-8e25-847df3e5eb6d | Login | HTTP/SOAP | POST | Authenticate to CAFM system, obtain SessionId |
| 381a025b-f3b9-4597-9902-3be49715c978 | Session logout | HTTP/SOAP | POST | Logout from CAFM session |
| 442683cb-b984-499e-b7bb-075c826905aa | GetLocationsByDto_CAFM | HTTP/SOAP | POST | Get location details by unit code/barcode |
| dc3b6b85-848d-471d-8c76-ed3b7dea0fbd | GetInstructionSetsByDto_CAFM | HTTP/SOAP | POST | Get instruction sets by subcategory description |
| c52c74c2-95e3-4cba-990e-3ce4746836a2 | GetBreakdownTasksByDto_CAFM | HTTP/SOAP | POST | Get existing breakdown tasks by CallId/SR number |
| 33dac20f-ea09-471c-91c3-91b39bc3b172 | CreateBreakdownTask Order EQ+ | HTTP/SOAP | POST | Create breakdown task in CAFM |
| 52166afd-a020-4de9-b49e-55400f1c0a7a | CreateEvent/Link task CAFM | HTTP/SOAP | POST | Create event and link to task (recurring tasks) |

### Non-System Layer Operations

| Operation ID | Operation Name | Type | Classification |
|---|---|---|---|
| af07502a-fafd-4976-a691-45d51a33b549 | Email w Attachment | SMTP | Separate System Layer (Email/SMTP) |
| 15a72a21-9b57-49a1-a8ed-d70367146644 | Email W/O Attachment | SMTP | Separate System Layer (Email/SMTP) |

---

## 2. Input Structure Analysis (Step 1a)

### Request Profile Structure
- **Profile ID:** af096014-313f-4565-9091-2bdd56eb46df
- **Profile Name:** EQ+_CAFM_Create_Request
- **Profile Type:** profile.json
- **Root Structure:** Root/Object/workOrder/Array/ArrayElement1/Object/...
- **Array Detection:** ✅ YES - workOrder is an array
- **Array Cardinality:** 
  - minOccurs: 0
  - maxOccurs: -1 (unlimited)
- **Input Type:** singlejson

### Input Format (JSON Structure)
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
- **Boomi Processing:** Input type is "singlejson" with array - Boomi processes entire array as single document (NOT splitting)
- **Azure Function Requirement:** Must accept array of work orders and process each element
- **Implementation Pattern:** Loop through array, process each work order, aggregate results

---

## 3. Response Structure Analysis (Step 1b)

### Response Profile Structure
- **Profile ID:** 9e542ed5-2c65-4af8-b0c6-821cbc58ca31 (referenced in operation de68dad0-be76-4ec8-9857-4e5cf2a7bd4c)
- **Profile Type:** profile.json (singlejson output)
- **Purpose:** Return success/failure status for each work order processed

---

## 4. Operation Response Analysis (Step 1c)

### Operation: Login (c20e5991-4d70-47f7-8e25-847df3e5eb6d)
- **Response Profile:** 992136d3-da44-4f22-994b-f7181624215b (Login_Response)
- **Response Structure:** SOAP XML response
- **Extracted Fields:**
  - SessionId (extracted by shape8 in FsiLogin subprocess, written to process.DPP_SessionId)
- **Consumers:** ALL subsequent CAFM operations (GetLocationsByDto, GetInstructionSetsByDto, GetBreakdownTasksByDto, CreateBreakdownTask, CreateEvent, Logout)
- **Business Logic:** Login MUST execute FIRST - produces SessionId required by all operations

### Operation: GetLocationsByDto (442683cb-b984-499e-b7bb-075c826905aa)
- **Response Profile:** 3aa0f5c5-8c95-4023-aba9-9d78dd6ade96
- **Response Structure:** SOAP XML with LocationDto array
- **Extracted Fields:**
  - BuildingId (extracted by shape25, written to process.DPP_BuildingID)
  - LocationId (extracted by shape25, written to process.DPP_LocationID)
- **Consumers:** CreateBreakdownTask operation (uses BuildingId and LocationId)
- **Business Logic:** GetLocationsByDto executes in branch path 3, produces location IDs for task creation

### Operation: GetInstructionSetsByDto (dc3b6b85-848d-471d-8c76-ed3b7dea0fbd)
- **Response Profile:** 5c2f13dd-3e51-4a7c-867b-c801aaa35562
- **Response Structure:** SOAP XML with FINFILEDto
- **Extracted Fields:**
  - IN_FKEY_CAT_SEQ (extracted by shape28, written to process.DPP_CategoryId)
  - IN_FKEY_LAB_SEQ (extracted by shape28, written to process.DDP_DisciplineId)
  - IN_FKEY_PRI_SEQ (extracted by shape28, written to process.DPP_PriorityId)
  - IN_SEQ (extracted by shape28, written to process.DPP_InstructionId)
- **Consumers:** CreateBreakdownTask operation (uses CategoryId, DisciplineId, PriorityId, InstructionId)
- **Business Logic:** GetInstructionSetsByDto executes in branch path 4, produces instruction set IDs for task creation

### Operation: GetBreakdownTasksByDto (c52c74c2-95e3-4cba-990e-3ce4746836a2)
- **Response Profile:** 1570c9d2-0588-410d-ad3d-bdc7d5c0ec9a
- **Response Structure:** SOAP XML with BreakdownTaskDtoV3 array
- **Extracted Fields:**
  - CallId (extracted by shape55, written to process.DPP_CallId)
- **Consumers:** Decision shape56 checks if CallId is empty to determine if task exists
- **Business Logic:** GetBreakdownTasksByDto executes in branch path 2, checks if task already exists

### Operation: CreateBreakdownTask (33dac20f-ea09-471c-91c3-91b39bc3b172)
- **Response Profile:** dbcca2ef-55cc-48e0-9329-1e8db4ada0c8
- **Response Structure:** SOAP XML with BreakdownTaskDtoV3
- **Extracted Fields:**
  - TaskId (extracted by shape42, written to process.DPP_TaskId)
- **Consumers:** CreateEvent operation (uses TaskId to link event to task)
- **Business Logic:** CreateBreakdownTask executes in branch path 5, produces TaskId for event linking

---

## 5. Process Properties Analysis (Steps 2-3)

### Properties WRITTEN

| Property Name | Written By Shape | Source | Purpose |
|---|---|---|---|
| process.DPP_SessionId | shape8 (FsiLogin subprocess) | Login response (SessionId) | Auth token for all operations |
| process.DPP_BuildingID | shape25 | GetLocationsByDto response | Building identifier |
| process.DPP_LocationID | shape25 | GetLocationsByDto response | Location identifier |
| process.DPP_CategoryId | shape28 | GetInstructionSetsByDto response | Category identifier |
| process.DDP_DisciplineId | shape28 | GetInstructionSetsByDto response | Discipline/Labor identifier |
| process.DPP_PriorityId | shape28 | GetInstructionSetsByDto response | Priority identifier |
| process.DPP_InstructionId | shape28 | GetInstructionSetsByDto response | Instruction set identifier |
| process.DPP_TaskId | shape42 | CreateBreakdownTask response | Created task identifier |
| process.DPP_CallId | shape55 | GetBreakdownTasksByDto response | Existing task CallId |
| process.DPP_ErrorMessage | shape21, shape11 (email subprocess) | Error messages | Error tracking |
| process.DPP_CafmError | shape11 (FsiLogin subprocess) | Login error message | Login failure tracking |
| process.DPP_CafmResponse | shape48 | Static error message | API timeout/blank response |

### Properties READ

| Property Name | Read By Shape | Operation | Purpose |
|---|---|---|---|
| process.DPP_SessionId | shape23, shape26, shape50, shape34, shape5 (logout subprocess) | All CAFM operations | Auth token in SOAP requests |
| process.DPP_BuildingID | (via map to CreateBreakdownTask) | CreateBreakdownTask | Building reference |
| process.DPP_LocationID | (via map to CreateBreakdownTask) | CreateBreakdownTask | Location reference |
| process.DPP_CategoryId | (via map to CreateBreakdownTask) | CreateBreakdownTask | Category reference |
| process.DDP_DisciplineId | (via map to CreateBreakdownTask) | CreateBreakdownTask | Discipline reference |
| process.DPP_PriorityId | (via map to CreateBreakdownTask) | CreateBreakdownTask | Priority reference |
| process.DPP_InstructionId | (via map to CreateBreakdownTask) | CreateBreakdownTask | Instruction reference |
| process.DPP_TaskId | shape34 | CreateEvent | Task identifier for linking |
| process.DPP_CallId | shape56 (decision) | Decision logic | Check if task exists |

---

## 6. Data Dependency Graph (Step 4)

### Dependency Chains

**Chain 1: Authentication Flow**
```
FsiLogin subprocess (shape5) → Writes process.DPP_SessionId
  ↓
ALL CAFM operations READ process.DPP_SessionId
  ↓
FsiLogout subprocess (shape13) READS process.DPP_SessionId
```
**Proof:** Login MUST execute BEFORE all operations (all operations use SessionId in SOAP requests)

**Chain 2: Location Data Flow (Branch Path 3)**
```
GetLocationsByDto (shape24) → Writes process.DPP_BuildingID, process.DPP_LocationID
  ↓
CreateBreakdownTask (shape11) READS these properties via map
```
**Proof:** GetLocationsByDto MUST execute BEFORE CreateBreakdownTask in path 3

**Chain 3: Instruction Set Data Flow (Branch Path 4)**
```
GetInstructionSetsByDto (shape27) → Writes process.DPP_CategoryId, process.DDP_DisciplineId, process.DPP_PriorityId, process.DPP_InstructionId
  ↓
CreateBreakdownTask (shape11) READS these properties via map
```
**Proof:** GetInstructionSetsByDto MUST execute BEFORE CreateBreakdownTask in path 4

**Chain 4: Task Creation Flow (Branch Path 5)**
```
CreateBreakdownTask (shape11) → Writes process.DPP_TaskId
  ↓
Decision shape31 checks recurrence field
  ↓
If recurrence = "Y" → CreateEvent (shape35) READS process.DPP_TaskId
```
**Proof:** CreateBreakdownTask MUST execute BEFORE CreateEvent

**Chain 5: Task Existence Check (Branch Path 2)**
```
GetBreakdownTasksByDto (shape53) → Writes process.DPP_CallId
  ↓
Decision shape56 checks if process.DPP_CallId is empty
  ↓
If empty → Continue to CreateBreakdownTask
If not empty → Skip CreateBreakdownTask (task already exists)
```
**Proof:** GetBreakdownTasksByDto MUST execute BEFORE decision and CreateBreakdownTask

---

## 7. Control Flow Graph (Step 5)

### Main Process Flow

```
shape1 (start) → shape2 (Input_details)
shape2 → shape3 (catcherrors - Try/Catch wrapper)
shape3 (Try path) → shape4 (branch - 6 paths)
shape3 (Catch path) → shape20 (branch - error handling)

Branch shape4 (6 paths):
  Path 1 → shape5 (processcall FsiLogin)
  Path 2 → shape50 (GetBreakdownTasksByDto request)
  Path 3 → shape23 (GetLocationsByDto request)
  Path 4 → shape26 (GetInstructionSetsByDto request)
  Path 5 → shape31 (decision - recurrence check)
  Path 6 → shape13 (processcall FsiLogout)

Path 1 (Login):
  shape5 → shape6 (stop continue=true) [SUCCESS]
  shape5 (Login_Error return) → shape6 (stop) [EARLY EXIT]

Path 2 (GetBreakdownTasksByDto):
  shape50 → shape51 → shape52 → shape53 (GetBreakdownTasksByDto API)
  shape53 → shape54 → shape55 (extract CallId)
  shape55 → shape56 (decision: CallId empty?)
    TRUE (empty - task doesn't exist) → Continue to path 5 (CreateBreakdownTask)
    FALSE (has value - task exists) → shape17 (map error) → shape15 (return)

Path 3 (GetLocationsByDto):
  shape23 → shape40 → shape43 → shape24 (GetLocationsByDto API)
  shape24 → shape38 → shape25 (extract BuildingId, LocationId)
  shape25 → shape30 (stop continue=true)

Path 4 (GetInstructionSetsByDto):
  shape26 → shape39 → shape44 → shape27 (GetInstructionSetsByDto API)
  shape27 → shape37 → shape28 (extract CategoryId, DisciplineId, PriorityId, InstructionId)
  shape28 → shape29 (stop continue=true)

Path 5 (CreateBreakdownTask):
  shape31 (decision: recurrence != "Y"?)
    TRUE (not recurring) → shape7 (map request) → shape9 → shape8 → shape11 (CreateBreakdownTask API)
    FALSE (recurring) → shape32 (branch - 2 paths)
      Path 1 → shape7 (CreateBreakdownTask)
      Path 2 → shape34 (CreateEvent request) → shape41 → shape45 → shape35 (CreateEvent API) → shape36 → shape33 (stop)
  
  After CreateBreakdownTask:
  shape11 → shape42 (extract TaskId) → shape46 → shape12 (decision: status 20*?)
    TRUE (success) → shape16 (map success) → shape15 (return)
    FALSE (error) → shape47 (decision: status 50*?)
      TRUE (server error) → shape17 (map error) → shape15 (return)
      FALSE (other error) → shape48 (set error message) → shape49 (map error) → shape15 (return)

Path 6 (Logout):
  shape13 (processcall FsiLogout) → (no explicit return path)

Error Handling (Catch path):
  shape20 (branch - 3 paths):
    Path 1 → shape21 (set error message) → shape19 (processcall Email) → (no return)
    Path 2 → shape18 (return Failure)
    Path 3 → shape22 (exception - throw error)
```

### Reverse Flow Mapping (Convergence Points)

| Shape ID | Incoming Connections | Convergence Type |
|---|---|---|
| shape15 | shape16, shape17, shape49 | Final return point (multiple error/success paths) |
| shape7 | shape31 (TRUE), shape32 (Path 1) | CreateBreakdownTask entry (recurring decision) |

---

## 8. Decision Shape Analysis (Step 7)

### ✅ Decision Data Sources Identified: YES
### ✅ Decision Types Classified: YES
### ✅ Execution Order Verified: YES
### ✅ All Decision Paths Traced: YES
### ✅ Decision Patterns Identified: YES
### ✅ Paths Traced to Termination: YES

### Decision 1: Login Success Check (FsiLogin subprocess - shape4)
- **Shape ID:** shape4 (in subprocess 3d9db79d-15d0-4472-9f47-375ad9ab1ed2)
- **Comparison:** regex match on meta.base.applicationstatuscode == "20*"
- **Data Source:** TRACK_PROPERTY (response status code from Login operation)
- **Decision Type:** POST-OPERATION (checks Login response)
- **Actual Execution Order:** Login Operation → Response → Decision → Route
- **TRUE Path:** shape8 (extract SessionId) → shape9 (stop continue=true) [SUCCESS]
- **FALSE Path:** shape11 (set error property) → shape6 (map error) → shape7 (return Login_Error) [EARLY EXIT]
- **Pattern:** Error Check (Success vs Failure) - POST-OPERATION
- **Convergence:** None (FALSE path exits early)
- **Early Exit:** YES (FALSE path returns Login_Error)

### Decision 2: Task Existence Check (shape56)
- **Shape ID:** shape56
- **Comparison:** equals check on process.DPP_CallId == "" (empty)
- **Data Source:** PROCESS_PROPERTY (written by GetBreakdownTasksByDto response extraction)
- **Decision Type:** POST-OPERATION (checks GetBreakdownTasksByDto response data)
- **Actual Execution Order:** GetBreakdownTasksByDto Operation → Extract CallId → Decision → Route
- **TRUE Path:** Continue to path 5 (CreateBreakdownTask) - task doesn't exist
- **FALSE Path:** shape17 (map error "Task already exists") → shape15 (return) [EARLY EXIT]
- **Pattern:** Check-Before-Create (Create if Not Found) - POST-OPERATION
- **Convergence:** None (FALSE path exits early)
- **Early Exit:** YES (FALSE path returns without creating task)

### Decision 3: Recurrence Check (shape31)
- **Shape ID:** shape31
- **Comparison:** notequals check on recurrence field != "Y"
- **Data Source:** INPUT (profile field from request: ticketDetails.recurrence)
- **Decision Type:** PRE-FILTER (checks input data)
- **Actual Execution Order:** Decision → Route to CreateBreakdownTask paths
- **TRUE Path:** shape7 (CreateBreakdownTask only) - not recurring
- **FALSE Path:** shape32 (branch - both CreateBreakdownTask AND CreateEvent) - recurring
- **Pattern:** Conditional Logic (Optional Processing) - PRE-FILTER
- **Convergence:** Both paths eventually reach shape15 (return)
- **Early Exit:** NO (both paths continue to completion)

### Decision 4: CreateBreakdownTask Success Check (shape12)
- **Shape ID:** shape12
- **Comparison:** regex match on meta.base.applicationstatuscode == "20*"
- **Data Source:** TRACK_PROPERTY (response status code from CreateBreakdownTask operation)
- **Decision Type:** POST-OPERATION (checks CreateBreakdownTask response)
- **Actual Execution Order:** CreateBreakdownTask Operation → Response → Decision → Route
- **TRUE Path:** shape16 (map success response) → shape15 (return)
- **FALSE Path:** shape47 (check if 50* server error) → Further error handling
- **Pattern:** Error Check (Success vs Failure) - POST-OPERATION
- **Convergence:** shape15 (all paths converge at return)
- **Early Exit:** NO (all paths reach return)

### Decision 5: Server Error Check (shape47)
- **Shape ID:** shape47
- **Comparison:** wildcard match on meta.base.applicationstatuscode == "50*"
- **Data Source:** TRACK_PROPERTY (response status code from CreateBreakdownTask operation)
- **Decision Type:** POST-OPERATION (checks CreateBreakdownTask response)
- **Actual Execution Order:** CreateBreakdownTask Operation → Response → Decision shape12 (FALSE) → Decision shape47 → Route
- **TRUE Path:** shape17 (map server error) → shape15 (return)
- **FALSE Path:** shape48 (set timeout error message) → shape49 (map error) → shape15 (return)
- **Pattern:** Error Classification (Server Error vs Timeout) - POST-OPERATION
- **Convergence:** shape15 (all paths converge at return)
- **Early Exit:** NO (all paths reach return)

### Decision 6: Email Attachment Check (subprocess a85945c5-3004-42b9-80b1-104f465cd1fb - shape4)
- **Shape ID:** shape4 (in email subprocess)
- **Comparison:** equals check on process.DPP_HasAttachment == "Y"
- **Data Source:** PROCESS_PROPERTY (set by main process from input)
- **Decision Type:** PRE-FILTER (checks input-derived property)
- **Actual Execution Order:** Decision → Route to appropriate email operation
- **TRUE Path:** shape11 (build email body with attachment) → shape14 → shape15 → shape6 (set mail properties) → shape3 (send email with attachment)
- **FALSE Path:** shape23 (build email body without attachment) → shape22 → shape20 (set mail properties) → shape7 (send email without attachment)
- **Pattern:** Conditional Logic (Optional Processing) - PRE-FILTER
- **Convergence:** Both paths reach stop (shape5 or shape9)
- **Early Exit:** NO (both paths complete successfully)

---

## 9. Branch Shape Analysis (Step 8)

### ✅ Classification Completed: YES
### ✅ Assumption Check: NO (analyzed dependencies AND API calls)
### ✅ Properties Extracted: YES
### ✅ Dependency Graph Built: YES
### ✅ Topological Sort Applied: YES (for sequential branches)

### Branch 1: Main Process Branch (shape4) - 6 Paths

**Branch Shape ID:** shape4
**Number of Paths:** 6
**Location:** After Try/Catch wrapper, before all operations

#### Properties Analysis (Step 1)

**Path 1 (Login):**
- READS: None (uses defined parameters for username/password)
- WRITES: process.DPP_SessionId

**Path 2 (GetBreakdownTasksByDto):**
- READS: process.DPP_SessionId, input.serviceRequestNumber
- WRITES: process.DPP_CallId

**Path 3 (GetLocationsByDto):**
- READS: process.DPP_SessionId, input.unitCode
- WRITES: process.DPP_BuildingID, process.DPP_LocationID

**Path 4 (GetInstructionSetsByDto):**
- READS: process.DPP_SessionId, input.subCategory
- WRITES: process.DPP_CategoryId, process.DDP_DisciplineId, process.DPP_PriorityId, process.DPP_InstructionId

**Path 5 (CreateBreakdownTask + optional CreateEvent):**
- READS: process.DPP_SessionId, process.DPP_BuildingID, process.DPP_LocationID, process.DPP_CategoryId, process.DDP_DisciplineId, process.DPP_PriorityId, process.DPP_InstructionId, input fields
- WRITES: process.DPP_TaskId

**Path 6 (Logout):**
- READS: process.DPP_SessionId
- WRITES: None

#### Dependency Graph (Step 2)

```
Path 1 (Login) → Path 2, Path 3, Path 4, Path 5, Path 6
  (All paths depend on Path 1 because they read process.DPP_SessionId)

Path 3 (GetLocationsByDto) → Path 5
  (Path 5 reads process.DPP_BuildingID, process.DPP_LocationID written by Path 3)

Path 4 (GetInstructionSetsByDto) → Path 5
  (Path 5 reads process.DPP_CategoryId, process.DDP_DisciplineId, process.DPP_PriorityId, process.DPP_InstructionId written by Path 4)

Path 5 (CreateBreakdownTask) → Path 6
  (Path 6 should execute after task creation completes)
```

**Proof:**
- Path 2 reads process.DPP_SessionId which Path 1 writes → Path 2 depends on Path 1
- Path 3 reads process.DPP_SessionId which Path 1 writes → Path 3 depends on Path 1
- Path 4 reads process.DPP_SessionId which Path 1 writes → Path 4 depends on Path 1
- Path 5 reads process.DPP_SessionId which Path 1 writes → Path 5 depends on Path 1
- Path 5 reads process.DPP_BuildingID which Path 3 writes → Path 5 depends on Path 3
- Path 5 reads process.DPP_CategoryId which Path 4 writes → Path 5 depends on Path 4
- Path 6 reads process.DPP_SessionId which Path 1 writes → Path 6 depends on Path 1

#### Classification (Step 3)

**Classification:** SEQUENTIAL

**Reasoning:**
1. **API Calls Present:** ALL paths contain SOAP API calls (connectoraction operations)
2. **🚨 CRITICAL RULE:** ALL API calls are SEQUENTIAL - there is NO concept of parallel API calls in Azure Functions migration
3. **Data Dependencies:** Multiple data dependencies exist between paths (SessionId, BuildingId, LocationId, CategoryId, etc.)
4. **Conclusion:** Branch is SEQUENTIAL due to both API calls AND data dependencies

#### Topological Sort Order (Step 4)

**Execution Order (Sequential):**
```
1. Path 1 (Login) - MUST execute FIRST (produces SessionId for all operations)
2. Path 2 (GetBreakdownTasksByDto) - Check if task exists
3. Path 3 (GetLocationsByDto) - Get location IDs (parallel with Path 4 in terms of data dependencies, but SEQUENTIAL due to API calls)
4. Path 4 (GetInstructionSetsByDto) - Get instruction set IDs (parallel with Path 3 in terms of data dependencies, but SEQUENTIAL due to API calls)
5. Path 5 (CreateBreakdownTask + optional CreateEvent) - MUST execute AFTER paths 3 and 4 (consumes their output)
6. Path 6 (Logout) - MUST execute LAST (cleanup)
```

**Note:** Paths 2, 3, 4 have no data dependencies on each other (only on Path 1), but they execute SEQUENTIALLY because all contain API calls. In Azure Functions, all API calls are sequential.

#### Path Termination (Step 5)

- **Path 1:** shape6 (stop continue=true) - Success return OR early exit on Login_Error
- **Path 2:** shape56 decision → shape17 (early exit if task exists) OR continue to Path 5
- **Path 3:** shape30 (stop continue=true)
- **Path 4:** shape29 (stop continue=true)
- **Path 5:** shape15 (return documents) via shape12/shape47 decisions
- **Path 6:** No explicit termination (subprocess ends)

#### Convergence Points (Step 6)

**Convergence at shape15:** All success/error paths from Path 5 (CreateBreakdownTask) converge at shape15 (return documents)

**Convergence at shape7:** Recurrence decision paths converge at shape7 (CreateBreakdownTask entry point)

---

### Branch 2: Error Handling Branch (shape20) - 3 Paths

**Branch Shape ID:** shape20
**Number of Paths:** 3
**Location:** Catch error handler

#### Properties Analysis

**Path 1 (Send Error Email):**
- READS: process.DPP_ErrorMessage (from catch error message)
- WRITES: None

**Path 2 (Return Failure):**
- READS: None
- WRITES: None

**Path 3 (Throw Exception):**
- READS: None
- WRITES: None

#### Classification

**Classification:** SEQUENTIAL (API calls present in Path 1 - email operation)

**Reasoning:** Path 1 contains email SMTP operation (API call), therefore branch executes sequentially

#### Topological Sort Order

**Execution Order:**
```
1. Path 1 (Send Error Email) - Notify via email
2. Path 2 (Return Failure) - Return failure response
3. Path 3 (Throw Exception) - Throw exception
```

**Note:** In practice, only ONE path executes based on error handling logic

---

### Branch 3: Recurrence Branch (shape32) - 2 Paths

**Branch Shape ID:** shape32
**Number of Paths:** 2
**Location:** Inside Path 5, after recurrence decision (FALSE - recurring task)

#### Properties Analysis

**Path 1 (CreateBreakdownTask only):**
- READS: process.DPP_SessionId, location/instruction properties
- WRITES: process.DPP_TaskId

**Path 2 (CreateBreakdownTask + CreateEvent):**
- READS: process.DPP_SessionId, process.DPP_TaskId, location/instruction properties
- WRITES: process.DPP_TaskId (from CreateBreakdownTask)

#### Classification

**Classification:** SEQUENTIAL (API calls present in both paths)

**Reasoning:** Both paths contain SOAP API calls, therefore execute sequentially

#### Topological Sort Order

**Execution Order:**
```
1. Path 1 (CreateBreakdownTask only) - Create task without event
2. Path 2 (CreateBreakdownTask + CreateEvent) - Create task AND link event
```

**Note:** Both paths converge at shape7 (CreateBreakdownTask entry), then Path 2 additionally executes CreateEvent

---

## 10. Execution Order (Step 9)

### ✅ Business Logic Verified FIRST: YES
### ✅ Operation Analysis Complete: YES
### ✅ Business Logic Execution Order Identified: YES
### ✅ Data Dependencies Checked FIRST: YES
### ✅ Operation Response Analysis Used: YES (referenced Step 1c)
### ✅ Decision Analysis Used: YES (referenced Step 7)
### ✅ Dependency Graph Used: YES (referenced Step 4)
### ✅ Branch Analysis Used: YES (referenced Step 8)
### ✅ Property Dependency Verification: YES
### ✅ Topological Sort Applied: YES

### Business Logic Flow (Step 0 - FIRST)

**Operation 1: Login**
- **Purpose:** Authentication - Establishes session with CAFM system
- **Outputs:** SessionId (process.DPP_SessionId)
- **Dependent Operations:** ALL subsequent CAFM operations (GetLocationsByDto, GetInstructionSetsByDto, GetBreakdownTasksByDto, CreateBreakdownTask, CreateEvent, Logout)
- **Business Flow:** Login MUST execute FIRST (produces SessionId needed by all operations)

**Operation 2: GetBreakdownTasksByDto**
- **Purpose:** Check if breakdown task already exists for given service request number
- **Outputs:** CallId (process.DPP_CallId)
- **Dependent Operations:** Decision shape56 uses CallId to determine if task exists
- **Business Flow:** Check operation executes AFTER login, BEFORE create operation (check-before-create pattern)

**Operation 3: GetLocationsByDto**
- **Purpose:** Retrieve location details (BuildingId, LocationId) by unit code
- **Outputs:** BuildingId (process.DPP_BuildingID), LocationId (process.DPP_LocationID)
- **Dependent Operations:** CreateBreakdownTask uses these IDs
- **Business Flow:** Lookup operation executes AFTER login, BEFORE create operation (enrichment pattern)

**Operation 4: GetInstructionSetsByDto**
- **Purpose:** Retrieve instruction set details (CategoryId, DisciplineId, PriorityId, InstructionId) by subcategory
- **Outputs:** CategoryId, DisciplineId, PriorityId, InstructionId (process properties)
- **Dependent Operations:** CreateBreakdownTask uses these IDs
- **Business Flow:** Lookup operation executes AFTER login, BEFORE create operation (enrichment pattern)

**Operation 5: CreateBreakdownTask**
- **Purpose:** Create breakdown task in CAFM system with enriched data
- **Outputs:** TaskId (process.DPP_TaskId)
- **Dependent Operations:** CreateEvent uses TaskId (if recurring task)
- **Business Flow:** Create operation executes AFTER all lookups complete, uses data from GetLocationsByDto and GetInstructionSetsByDto

**Operation 6: CreateEvent**
- **Purpose:** Create event and link to task (for recurring tasks only)
- **Outputs:** None
- **Dependent Operations:** None
- **Business Flow:** Conditional operation executes ONLY if recurrence = "Y", AFTER CreateBreakdownTask completes

**Operation 7: Logout**
- **Purpose:** Terminate CAFM session
- **Outputs:** None
- **Dependent Operations:** None
- **Business Flow:** Cleanup operation executes LAST, after all operations complete

### Actual Execution Order (Based on Dependencies and Business Logic)

**Referenced Analysis:**
- Step 1c (Operation Response Analysis): Identified what each operation produces and consumes
- Step 4 (Data Dependency Graph): Showed dependency chains (Login → All Ops, GetLocations → CreateTask, GetInstructions → CreateTask)
- Step 7 (Decision Analysis): Identified check-before-create pattern, recurrence conditional, error checks
- Step 8 (Branch Analysis): Classified branch as SEQUENTIAL due to API calls and dependencies

**Execution Sequence:**

```
1. START (shape1)
2. Input Details Extraction (shape2)
3. Try/Catch Wrapper (shape3)
4. Branch (shape4) - SEQUENTIAL execution:
   
   4.1. Path 1: Login (shape5 - processcall FsiLogin)
        → FsiLogin subprocess executes:
           → Build SOAP envelope with username/password
           → Call Login API (shape2 in subprocess)
           → Decision: Check status 20*? (shape4 in subprocess)
              → TRUE: Extract SessionId (shape8) → Stop (SUCCESS)
              → FALSE: Set error (shape11) → Map error (shape6) → Return Login_Error [EARLY EXIT]
        → If Login_Error: Stop execution (shape6) [EARLY EXIT]
        → If Success: Continue to next paths
   
   4.2. Path 2: GetBreakdownTasksByDto (shape50-53)
        → Build SOAP envelope with SessionId + serviceRequestNumber
        → Call GetBreakdownTasksByDto API (shape53)
        → Extract CallId (shape55)
        → Decision: CallId empty? (shape56)
           → TRUE (empty - task doesn't exist): Continue to Path 5
           → FALSE (has value - task exists): Map error → Return [EARLY EXIT]
   
   4.3. Path 3: GetLocationsByDto (shape23-24)
        → Build SOAP envelope with SessionId + unitCode
        → Call GetLocationsByDto API (shape24)
        → Extract BuildingId, LocationId (shape25)
        → Stop (shape30)
   
   4.4. Path 4: GetInstructionSetsByDto (shape26-27)
        → Build SOAP envelope with SessionId + subCategory
        → Call GetInstructionSetsByDto API (shape27)
        → Extract CategoryId, DisciplineId, PriorityId, InstructionId (shape28)
        → Stop (shape29)
   
   4.5. Path 5: CreateBreakdownTask + Optional CreateEvent (shape31)
        → Decision: recurrence != "Y"? (shape31)
           → TRUE (not recurring):
              → Map CreateBreakdownTask request (shape7)
              → Call CreateBreakdownTask API (shape11)
              → Extract TaskId (shape42)
              → Decision: status 20*? (shape12)
                 → TRUE: Map success → Return (shape15)
                 → FALSE: Check 50*? (shape47)
                    → TRUE: Map server error → Return (shape15)
                    → FALSE: Set timeout error → Map error → Return (shape15)
           
           → FALSE (recurring):
              → Branch (shape32) - SEQUENTIAL:
                 → Path 1: CreateBreakdownTask only
                 → Path 2: CreateBreakdownTask + CreateEvent
                    → Build CreateEvent SOAP envelope with SessionId + TaskId + oldCAFMSRnumber
                    → Call CreateEvent API (shape35)
                    → Stop (shape33)
   
   4.6. Path 6: Logout (shape13 - processcall FsiLogout)
        → FsiLogout subprocess executes:
           → Build SOAP envelope with SessionId
           → Call Logout API (shape2 in subprocess)
           → Stop

5. Return Documents (shape15) - Final response

CATCH (Error Path):
   → Branch (shape20) - 3 paths:
      → Path 1: Send error email (shape21 → shape19 - processcall Email)
      → Path 2: Return failure (shape18)
      → Path 3: Throw exception (shape22)
```

### Dependency Verification

**Referenced Step 4 (Data Dependency Graph):**
- Login produces SessionId → ALL operations consume SessionId → Login MUST execute FIRST ✅
- GetLocationsByDto produces BuildingId, LocationId → CreateBreakdownTask consumes → GetLocationsByDto MUST execute BEFORE CreateBreakdownTask ✅
- GetInstructionSetsByDto produces CategoryId, DisciplineId, PriorityId, InstructionId → CreateBreakdownTask consumes → GetInstructionSetsByDto MUST execute BEFORE CreateBreakdownTask ✅
- CreateBreakdownTask produces TaskId → CreateEvent consumes → CreateBreakdownTask MUST execute BEFORE CreateEvent ✅

### Branch Execution Order

**Referenced Step 8 (Branch Analysis):**
- Main branch (shape4) classified as SEQUENTIAL due to API calls and data dependencies
- Topological sort order: Path 1 → Path 2 → Path 3 → Path 4 → Path 5 → Path 6
- Paths 3 and 4 have no data dependencies on each other, but execute sequentially due to API call rule

---

## 11. Sequence Diagram (Step 10)

**Based on dependency graph in Step 4, decision analysis in Step 7, control flow graph in Step 5, branch analysis in Step 8, and execution order in Step 9:**

```
START (Process Layer calls System Layer Function)
 |
 ├─→ TRY:
 |    |
 |    ├─→ Operation 1: FsiLogin (Subprocess)
 |    |    └─→ READS: FSI_Username, FSI_Password (from config)
 |    |    └─→ WRITES: process.DPP_SessionId
 |    |    └─→ Decision: Login status 20*?
 |    |         ├─→ TRUE: Extract SessionId → Continue
 |    |         └─→ FALSE: Return Login_Error [EARLY EXIT]
 |    |
 |    ├─→ Operation 2: GetBreakdownTasksByDto
 |    |    └─→ READS: process.DPP_SessionId, input.serviceRequestNumber
 |    |    └─→ WRITES: process.DPP_CallId
 |    |    └─→ Decision: CallId empty?
 |    |         ├─→ TRUE (task doesn't exist): Continue
 |    |         └─→ FALSE (task exists): Return "Task already exists" [EARLY EXIT]
 |    |
 |    ├─→ Operation 3: GetLocationsByDto
 |    |    └─→ READS: process.DPP_SessionId, input.unitCode
 |    |    └─→ WRITES: process.DPP_BuildingID, process.DPP_LocationID
 |    |
 |    ├─→ Operation 4: GetInstructionSetsByDto
 |    |    └─→ READS: process.DPP_SessionId, input.subCategory
 |    |    └─→ WRITES: process.DPP_CategoryId, process.DDP_DisciplineId, process.DPP_PriorityId, process.DPP_InstructionId
 |    |
 |    ├─→ Decision: recurrence != "Y"? [PRE-FILTER - checks input data]
 |    |    ├─→ TRUE (not recurring):
 |    |    |    └─→ Operation 5a: CreateBreakdownTask
 |    |    |         └─→ READS: process.DPP_SessionId, process.DPP_BuildingID, process.DPP_LocationID, 
 |    |    |                     process.DPP_CategoryId, process.DDP_DisciplineId, process.DPP_PriorityId, 
 |    |    |                     process.DPP_InstructionId, input fields
 |    |    |         └─→ WRITES: process.DPP_TaskId
 |    |    |         └─→ Decision: CreateBreakdownTask status 20*?
 |    |    |              ├─→ TRUE: Return Success
 |    |    |              └─→ FALSE: Check 50*?
 |    |    |                   ├─→ TRUE: Return Server Error
 |    |    |                   └─→ FALSE: Return Timeout Error
 |    |    |
 |    |    └─→ FALSE (recurring):
 |    |         └─→ Operation 5b: CreateBreakdownTask (same as 5a)
 |    |         └─→ Operation 6: CreateEvent [Only if recurring]
 |    |              └─→ READS: process.DPP_SessionId, process.DPP_TaskId, input.oldCAFMSRnumber
 |    |              └─→ WRITES: None
 |    |
 |    └─→ Operation 7: FsiLogout (Subprocess)
 |         └─→ READS: process.DPP_SessionId
 |         └─→ WRITES: None
 |
 └─→ CATCH (Error Path):
      └─→ Branch (3 paths - SEQUENTIAL):
           ├─→ Path 1: Send Error Email (Operation: Office365 Email)
           ├─→ Path 2: Return Failure
           └─→ Path 3: Throw Exception
 |
END (Return to Process Layer)
```

---

## 12. Subprocess Analysis

### Subprocess 1: FsiLogin (3d9db79d-15d0-4472-9f47-375ad9ab1ed2)

**Purpose:** Authenticate to CAFM system and obtain SessionId

**Internal Flow:**
```
START (shape1)
 → Set dynamic properties (URL, SOAPAction) (shape3)
 → Build SOAP envelope with username/password (shape5)
 → Call Login API (shape2 - operation c20e5991-4d70-47f7-8e25-847df3e5eb6d)
 → Decision: Check status 20*? (shape4)
    ├─→ TRUE: Extract SessionId (shape8) → Stop (continue=true) [SUCCESS RETURN]
    └─→ FALSE: Set error message (shape11) → Map error (shape6) → Return "Login_Error" [ERROR RETURN]
```

**Return Paths:**
- **SUCCESS:** Stop (continue=true) - SessionId available in process.DPP_SessionId
- **ERROR:** Return Documents with label "Login_Error" - Error message in process.DPP_CafmError

**Properties Written:**
- process.DPP_SessionId (on success)
- process.DPP_CafmError (on failure)

**Main Process Mapping:**
- Return label "Login_Error" → shape6 (stop) in main process [EARLY EXIT]
- Success (no explicit return label) → Continue to next branch path

---

### Subprocess 2: FsiLogout (b44c26cb-ecd5-4677-a752-434fe68f2e2b)

**Purpose:** Terminate CAFM session

**Internal Flow:**
```
START (shape1)
 → Build SOAP envelope with SessionId (shape5)
 → Set dynamic properties (URL, SOAPAction) (shape4)
 → Call Logout API (shape2 - operation 381a025b-f3b9-4597-9902-3be49715c978)
 → Stop (continue=true)
```

**Return Paths:**
- **SUCCESS:** Stop (continue=true) - Logout completes

**Properties Read:**
- process.DPP_SessionId

**Main Process Mapping:**
- No explicit return paths - subprocess completes and main process ends

---

### Subprocess 3: Office 365 Email (a85945c5-3004-42b9-80b1-104f465cd1fb)

**Purpose:** Send email notification (with or without attachment)

**Internal Flow:**
```
START (shape1)
 → Try/Catch wrapper (shape2)
    ├─→ TRY:
    |    → Decision: HasAttachment == "Y"? (shape4)
    |       ├─→ TRUE: Build email body with attachment (shape11) → Set mail properties (shape6) → Send email with attachment (shape3) → Stop
    |       └─→ FALSE: Build email body without attachment (shape23) → Set mail properties (shape20) → Send email without attachment (shape7) → Stop
    |
    └─→ CATCH: Throw exception (shape10)
```

**Return Paths:**
- **SUCCESS:** Stop (continue=true) - Email sent

**Properties Read:**
- process.DPP_HasAttachment
- process.To_Email
- process.DPP_Subject
- process.DPP_MailBody
- process.DPP_File_Name (if attachment)

**Main Process Mapping:**
- Called from error handling path (shape19) - sends error notification email

---

## 13. Critical Patterns Identified

### Pattern 1: Check-Before-Create (Task Existence)
**Identification:** GetBreakdownTasksByDto → Decision (CallId empty?) → CreateBreakdownTask (if empty)
**Execution Rule:** Check operation MUST execute BEFORE create operation
**Sequence:**
```
Operation_Check: GetBreakdownTasksByDto
 └─→ WRITES: process.DPP_CallId
 |
 ├─→ Decision: CallId equals "" (empty)?
 |    ├─→ TRUE (empty = task NOT exists) → Continue to CreateBreakdownTask
 |    └─→ FALSE (has value = task EXISTS) → Return "Task already exists" [EARLY EXIT]
```

### Pattern 2: Session-Based Authentication
**Identification:** FsiLogin subprocess → All operations use SessionId → FsiLogout subprocess
**Execution Rule:** Login MUST execute FIRST, Logout MUST execute LAST
**Sequence:**
```
FsiLogin (produces SessionId)
 → All CAFM operations (consume SessionId)
 → FsiLogout (cleanup SessionId)
```

### Pattern 3: Data Enrichment (Parallel Lookups)
**Identification:** GetLocationsByDto + GetInstructionSetsByDto → Both produce data → CreateBreakdownTask consumes both
**Execution Rule:** Both lookups MUST execute BEFORE CreateBreakdownTask, but have no dependencies on each other
**Sequence:**
```
GetLocationsByDto (produces BuildingId, LocationId)
  ↓ (sequential due to API calls)
GetInstructionSetsByDto (produces CategoryId, DisciplineId, PriorityId, InstructionId)
  ↓
CreateBreakdownTask (consumes all enrichment data)
```

### Pattern 4: Conditional Processing (Recurrence)
**Identification:** Decision checks recurrence field → If "Y" execute additional CreateEvent operation
**Execution Rule:** Decision is PRE-FILTER (checks input), CreateEvent is conditional
**Sequence:**
```
Decision: recurrence != "Y"?
 ├─→ TRUE (not recurring): CreateBreakdownTask only
 └─→ FALSE (recurring): CreateBreakdownTask + CreateEvent (link task to recurring event)
```

### Pattern 5: Error Handling with Email Notification
**Identification:** Try/Catch wrapper → On error, send email notification
**Execution Rule:** Email subprocess called only on errors
**Sequence:**
```
TRY: All operations
CATCH: Error occurs
 → Branch (3 paths):
    → Path 1: Send error email (Office365 Email subprocess)
    → Path 2: Return failure response
    → Path 3: Throw exception
```

---

## 14. Validation Checklist

### Data Dependencies
- [x] All property WRITES identified
- [x] All property READS identified
- [x] Dependency graph built
- [x] Execution order satisfies all dependencies (no read-before-write)

### Decision Analysis
- [x] ALL decision shapes inventoried (6 decisions)
- [x] BOTH TRUE and FALSE paths traced to termination
- [x] Pattern type identified for each decision
- [x] Early exits identified and documented (Login_Error, Task exists)
- [x] Convergence points identified (shape15, shape7)

### Branch Analysis
- [x] Each branch classified as sequential (API calls present)
- [x] API calls checked in branch paths (all paths have SOAP operations)
- [x] Dependency order built using topological sort
- [x] Each path traced to terminal point
- [x] Convergence points identified
- [x] Execution continuation point determined

### Sequence Diagram
- [x] Format follows required structure
- [x] Each operation shows READS and WRITES
- [x] Decisions show both TRUE and FALSE paths
- [x] Check-before-create pattern shown correctly
- [x] Early exits marked [EARLY EXIT]
- [x] Conditional execution marked [Only if recurring]
- [x] Subprocess internal flows documented

### Subprocess Analysis
- [x] ALL subprocesses analyzed (FsiLogin, FsiLogout, Office365 Email)
- [x] Return paths identified (Login_Error for FsiLogin)
- [x] Return path labels mapped to main process shapes
- [x] Properties written by subprocess documented
- [x] Properties read by subprocess from main process documented

### Input/Output Structure Analysis
- [x] Entry point operation identified (de68dad0-be76-4ec8-9857-4e5cf2a7bd4c)
- [x] Request profile identified (af096014-313f-4565-9091-2bdd56eb46df)
- [x] Request profile structure analyzed (JSON with workOrder array)
- [x] Array detection completed (workOrder is array, minOccurs=0, maxOccurs=-1)
- [x] ALL request fields extracted
- [x] Document processing behavior determined (singlejson - process entire array)

---

## 15. System Layer Identification

### CAFM/FSI System Layer Operations

**System of Record:** CAFM (FSI Evolution)
**Integration Type:** SOAP/XML
**Authentication:** Session-based (Login/Logout)

**System Layer Functions to Create:**

1. **AuthenticateCAFM** - Login to CAFM system (Middleware - Internal Atomic Handler)
2. **LogoutCAFM** - Logout from CAFM system (Middleware - Internal Atomic Handler)
3. **GetLocationsByDto** - Get location details by unit code
4. **GetInstructionSetsByDto** - Get instruction sets by subcategory
5. **GetBreakdownTasksByDto** - Get existing breakdown tasks by service request number
6. **CreateBreakdownTask** - Create breakdown task in CAFM
7. **CreateEvent** - Create event and link to task (for recurring tasks)

**Total System Layer Functions:** 5 Azure Functions (excluding auth handlers which are middleware-internal)

**Middleware Required:** YES - Session-based authentication requires CustomAuthenticationMiddleware with AuthenticateAtomicHandler and LogoutAtomicHandler

---

## 16. Field Mapping Analysis

### Request Field Mapping (Input Profile: af096014-313f-4565-9091-2bdd56eb46df)

| Boomi Field Path | Boomi Field Name | Data Type | Required | Azure DTO Property | Notes |
|---|---|---|---|---|---|
| Root/Object/workOrder/Array/ArrayElement1/Object/reporterName | reporterName | character | No | ReporterName | Reporter contact info |
| Root/Object/workOrder/Array/ArrayElement1/Object/reporterEmail | reporterEmail | character | No | ReporterEmail | Reporter contact info |
| Root/Object/workOrder/Array/ArrayElement1/Object/reporterPhoneNumber | reporterPhoneNumber | character | No | ReporterPhoneNumber | Reporter contact info |
| Root/Object/workOrder/Array/ArrayElement1/Object/description | description | character | No | Description | Work order description |
| Root/Object/workOrder/Array/ArrayElement1/Object/serviceRequestNumber | serviceRequestNumber | character | No | ServiceRequestNumber | Unique SR identifier |
| Root/Object/workOrder/Array/ArrayElement1/Object/propertyName | propertyName | character | No | PropertyName | Property name |
| Root/Object/workOrder/Array/ArrayElement1/Object/unitCode | unitCode | character | No | UnitCode | Unit/location barcode |
| Root/Object/workOrder/Array/ArrayElement1/Object/categoryName | categoryName | character | No | CategoryName | Category name |
| Root/Object/workOrder/Array/ArrayElement1/Object/subCategory | subCategory | character | No | SubCategory | Subcategory description |
| Root/Object/workOrder/Array/ArrayElement1/Object/technician | technician | character | No | Technician | Assigned technician |
| Root/Object/workOrder/Array/ArrayElement1/Object/sourceOrgId | sourceOrgId | character | No | SourceOrgId | Source organization ID |
| Root/Object/workOrder/Array/ArrayElement1/Object/ticketDetails/Object/status | status | character | No | Status | Ticket status |
| Root/Object/workOrder/Array/ArrayElement1/Object/ticketDetails/Object/subStatus | subStatus | character | No | SubStatus | Ticket sub-status |
| Root/Object/workOrder/Array/ArrayElement1/Object/ticketDetails/Object/priority | priority | character | No | Priority | Priority level |
| Root/Object/workOrder/Array/ArrayElement1/Object/ticketDetails/Object/scheduledDate | scheduledDate | character | No | ScheduledDate | Scheduled date |
| Root/Object/workOrder/Array/ArrayElement1/Object/ticketDetails/Object/scheduledTimeStart | scheduledTimeStart | character | No | ScheduledTimeStart | Start time |
| Root/Object/workOrder/Array/ArrayElement1/Object/ticketDetails/Object/scheduledTimeEnd | scheduledTimeEnd | character | No | ScheduledTimeEnd | End time |
| Root/Object/workOrder/Array/ArrayElement1/Object/ticketDetails/Object/recurrence | recurrence | character | No | Recurrence | Recurrence flag (Y/N) |
| Root/Object/workOrder/Array/ArrayElement1/Object/ticketDetails/Object/oldCAFMSRnumber | oldCAFMSRnumber | character | No | OldCAFMSRnumber | Previous CAFM SR number |
| Root/Object/workOrder/Array/ArrayElement1/Object/ticketDetails/Object/raisedDateUtc | raisedDateUtc | character | Yes | RaisedDateUtc | Raised date (UTC) |

### Response Field Mapping

**GetLocationsByDto Response:**
- BuildingId (number) → BuildingId
- LocationId (number) → LocationId
- PrimaryKeyId (number) → PrimaryKeyId

**GetInstructionSetsByDto Response:**
- IN_FKEY_CAT_SEQ (number) → CategoryId
- IN_FKEY_LAB_SEQ (number) → DisciplineId
- IN_FKEY_PRI_SEQ (number) → PriorityId
- IN_SEQ (number) → InstructionId

**GetBreakdownTasksByDto Response:**
- CallId (string) → CallId
- TaskId (number) → TaskId

**CreateBreakdownTask Response:**
- TaskId (number) → TaskId
- PrimaryKeyId (number) → PrimaryKeyId
- FilterQueryId (number) → FilterQueryId

**CreateEvent Response:**
- EventId (number) → EventId (if applicable)

---

## 17. Summary

### System Layer Classification

**CAFM/FSI System Layer:**
- **Total Operations:** 7 SOAP operations
- **Azure Functions to Create:** 5 (GetLocationsByDto, GetInstructionSetsByDto, GetBreakdownTasksByDto, CreateBreakdownTask, CreateEvent)
- **Middleware Components:** CustomAuthenticationMiddleware, AuthenticateAtomicHandler (Login), LogoutAtomicHandler (Logout), RequestContext
- **Integration Type:** SOAP/XML
- **Authentication:** Session-based (middleware-managed)

**Execution Pattern:**
1. Middleware handles Login (obtains SessionId)
2. Process Layer calls System Layer Functions independently
3. Each Function uses SessionId from RequestContext
4. Middleware handles Logout in finally block

**Key Architectural Decisions:**
- Login/Logout are NOT Azure Functions - handled by middleware (internal Atomic Handlers)
- GetLocationsByDto, GetInstructionSetsByDto, GetBreakdownTasksByDto are separate Functions (Process Layer can call independently)
- CreateBreakdownTask is separate Function (main operation)
- CreateEvent is separate Function (conditional operation for recurring tasks)
- Check-before-create logic (GetBreakdownTasksByDto → decision) is Process Layer responsibility
- Data enrichment orchestration (GetLocations + GetInstructions → CreateTask) is Process Layer responsibility

---

**PHASE 1 COMPLETE - Ready for Phase 2 (System Layer Code Generation)**
