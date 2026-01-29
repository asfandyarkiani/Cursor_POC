# PHASE 1: BOOMI PROCESS ANALYSIS
## Create Work Order from EQ+ to CAFM

---

## 1. OPERATIONS INVENTORY

### Main Process Operations
| Operation ID | Name | Type | Purpose |
|---|---|---|---|
| de68dad0-be76-4ec8-9857-4e5cf2a7bd4c | EQ+_CAFM_Create | wss (Web Service Listen) | Entry point - receives work order requests |
| 442683cb-b984-499e-b7bb-075c826905aa | GetLocationsByDto_CAFM | http/SOAP | Get location details from CAFM |
| dc3b6b85-848d-471d-8c76-ed3b7dea0fbd | GetInstructionSetsByDto_CAFM | http/SOAP | Get instruction sets from CAFM |
| 33dac20f-ea09-471c-91c3-91b39bc3b172 | CreateBreakdownTask Order EQ+ | http/SOAP | Create breakdown task in CAFM |
| c52c74c2-95e3-4cba-990e-3ce4746836a2 | GetBreakdownTasksByDto_CAFM | http/SOAP | Get breakdown tasks from CAFM |
| 52166afd-a020-4de9-b49e-55400f1c0a7a | CreateEvent/Link task CAFM | http/SOAP | Create event and link to task in CAFM |

### Subprocess Operations
| Subprocess ID | Name | Type | Operations |
|---|---|---|---|
| 3d9db79d-15d0-4472-9f47-375ad9ab1ed2 | FsiLogin | process | c20e5991-4d70-47f7-8e25-847df3e5eb6d (Login - SOAP) |
| b44c26cb-ecd5-4677-a752-434fe68f2e2b | FsiLogout | process | 381a025b-f3b9-4597-9902-3be49715c978 (Session logout - SOAP) |
| a85945c5-3004-42b9-80b1-104f465cd1fb | Office 365 Email | process | af07502a-fafd-4976-a691-45d51a33b549 (Email w Attachment), 15a72a21-9b57-49a1-a8ed-d70367146644 (Email W/O Attachment) |

---

## 2. SYSTEM LAYER CLASSIFICATION

### Third-Party Systems Identified

**System 1: CAFM (FSI Evolution)**
- **Type**: SOAP API
- **Authentication**: Session-based (Login → SessionId → Operations → Logout)
- **Operations**:
  1. Login (Authenticate) - Returns SessionId
  2. Logout - Terminates session
  3. GetLocationsByDto - Get location details by barcode
  4. GetInstructionSetsByDto - Get instruction sets by description
  5. CreateBreakdownTask - Create breakdown task
  6. GetBreakdownTasksByDto - Get breakdown tasks by CallId
  7. CreateEvent - Create event and link to task

**System 2: Office 365 Email (SMTP)**
- **Type**: SMTP/Mail connector
- **Operations**:
  1. Send Email with Attachment
  2. Send Email without Attachment

### System Layer Functions Required

Based on the Function Exposure Decision Process, I will create **1 Azure Function** for CAFM operations:

**CreateWorkOrderAPI** - Main orchestration function that:
- Handles session-based authentication via middleware (Login/Logout)
- Orchestrates internal CAFM operations (GetLocations, GetInstructionSets, CreateBreakdownTask, CreateEvent, GetBreakdownTasks)
- All operations are same SOR (CAFM) → Handler orchestrates internally
- Process Layer will call this single function

**Email operations** will be handled separately if needed (typically Process Layer responsibility for notifications).

---

## 3. INPUT STRUCTURE ANALYSIS (Step 1a)

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
- **Boomi Processing:** Each array element triggers separate process execution (inputType: singlejson with array)
- **Azure Function Requirement:** Must accept array and process each work order element
- **Implementation Pattern:** Loop through array, process each work order, aggregate results

---

## 4. RESPONSE STRUCTURE ANALYSIS (Step 1b)

### Response Profile Structure
- **Profile ID:** 9e542ed5-2c65-4af8-b0c6-821cbc58ca31
- **Profile Name:** EQ+_CAFM_Create_Response
- **Profile Type:** profile.json
- **Root Structure:** Root/Object/workOrder/Array/ArrayElement1/Object/...
- **Array Detection:** ✅ YES - workOrder is an array (matches request structure)

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

---

## 5. OPERATION RESPONSE ANALYSIS (Step 1c)

### Operation: Login (c20e5991-4d70-47f7-8e25-847df3e5eb6d)
- **Response Profile:** 992136d3-da44-4f22-994b-f7181624215b (Login_Response)
- **Extracted Fields:**
  - SessionId (extracted by shape8, written to process.DPP_SessionId)
- **Consumers:** ALL subsequent CAFM operations (GetLocationsByDto, GetInstructionSetsByDto, CreateBreakdownTask, GetBreakdownTasksByDto, CreateEvent)
- **Business Logic:** Login MUST execute FIRST - all CAFM operations require SessionId

### Operation: GetLocationsByDto (442683cb-b984-499e-b7bb-075c826905aa)
- **Response Profile:** 3aa0f5c5-8c95-4023-aba9-9d78dd6ade96
- **Extracted Fields:**
  - BuildingId (extracted by shape25, written to process.DPP_BuildingID)
  - LocationId (extracted by shape25, written to process.DPP_LocationID)
- **Consumers:** CreateBreakdownTask operation (uses BuildingID and LocationID)
- **Business Logic:** GetLocationsByDto MUST execute BEFORE CreateBreakdownTask

### Operation: GetInstructionSetsByDto (dc3b6b85-848d-471d-8c76-ed3b7dea0fbd)
- **Response Profile:** 5c2f13dd-3e51-4a7c-867b-c801aaa35562
- **Extracted Fields:**
  - IN_FKEY_CAT_SEQ (CategoryId) - extracted by shape28, written to process.DPP_CategoryId
  - IN_FKEY_LAB_SEQ (DisciplineId) - extracted by shape28, written to process.DDP_DisciplineId
  - IN_FKEY_PRI_SEQ (PriorityId) - extracted by shape28, written to process.DPP_PriorityId
  - IN_SEQ (InstructionId) - extracted by shape28, written to process.DPP_InstructionId
- **Consumers:** CreateBreakdownTask operation (uses CategoryId, DisciplineId, PriorityId, InstructionId)
- **Business Logic:** GetInstructionSetsByDto MUST execute BEFORE CreateBreakdownTask

### Operation: CreateBreakdownTask (33dac20f-ea09-471c-91c3-91b39bc3b172)
- **Response Profile:** dbcca2ef-55cc-48e0-9329-1e8db4ada0c8
- **Extracted Fields:**
  - TaskId (extracted by shape42, written to process.DPP_TaskId)
- **Consumers:** CreateEvent operation (uses TaskId for linking)
- **Business Logic:** CreateBreakdownTask MUST execute BEFORE CreateEvent (if recurrence = Y)

### Operation: GetBreakdownTasksByDto (c52c74c2-95e3-4cba-990e-3ce4746836a2)
- **Response Profile:** 1570c9d2-0588-410d-ad3d-bdc7d5c0ec9a
- **Extracted Fields:**
  - CallId (checked by decision shape55)
- **Consumers:** Decision shape55 checks if CallId is empty
- **Business Logic:** GetBreakdownTasksByDto executes to check if task already exists

### Operation: CreateEvent (52166afd-a020-4de9-b49e-55400f1c0a7a)
- **Response Profile:** 449782a0-4b04-4a7a-aa5c-aa9265fd2614
- **Extracted Fields:** None explicitly extracted
- **Consumers:** None
- **Business Logic:** CreateEvent executes ONLY if recurrence = Y (conditional)

---

## 6. PROCESS PROPERTIES ANALYSIS (Steps 2-3)

### Properties WRITTEN
| Property Name | Written By | Source |
|---|---|---|
| process.DPP_SessionId | shape8 (FsiLogin subprocess) | Login response - SessionId |
| process.DPP_BuildingID | shape25 | GetLocationsByDto response - BuildingId |
| process.DPP_LocationID | shape25 | GetLocationsByDto response - LocationId |
| process.DPP_CategoryId | shape28 | GetInstructionSetsByDto response - IN_FKEY_CAT_SEQ |
| process.DDP_DisciplineId | shape28 | GetInstructionSetsByDto response - IN_FKEY_LAB_SEQ |
| process.DPP_PriorityId | shape28 | GetInstructionSetsByDto response - IN_FKEY_PRI_SEQ |
| process.DPP_InstructionId | shape28 | GetInstructionSetsByDto response - IN_SEQ |
| process.DPP_TaskId | shape42 | CreateBreakdownTask response - TaskId |
| process.DPP_CafmError | shape11 (FsiLogin subprocess) | Static error message |
| process.DPP_ErrorMessage | shape21 | Try/Catch error message |

### Properties READ
| Property Name | Read By | Usage |
|---|---|---|
| process.DPP_SessionId | shape23, shape26, shape50, shape34 (message shapes) | Used in all CAFM SOAP requests |
| process.DPP_BuildingID | (via map) | Used in CreateBreakdownTask request |
| process.DPP_LocationID | (via map) | Used in CreateBreakdownTask request |
| process.DPP_CategoryId | (via map) | Used in CreateBreakdownTask request |
| process.DDP_DisciplineId | (via map) | Used in CreateBreakdownTask request |
| process.DPP_PriorityId | (via map) | Used in CreateBreakdownTask request |
| process.DPP_InstructionId | (via map) | Used in CreateBreakdownTask request |
| process.DPP_TaskId | shape34 (CreateEvent message) | Used in CreateEvent request |
| process.To_Email | Email subprocess | Email recipient |
| process.DPP_HasAttachment | Email subprocess decision | Determines email operation |

---

## 7. DATA DEPENDENCY GRAPH (Step 4)

### Dependency Chains

**Chain 1: Authentication**
```
FsiLogin (shape5) → WRITES process.DPP_SessionId
  ↓
ALL CAFM operations READ process.DPP_SessionId
  ↓
FsiLogout (shape13) READS process.DPP_SessionId
```

**Chain 2: Location Lookup**
```
GetLocationsByDto (shape24) → WRITES process.DPP_BuildingID, process.DPP_LocationID
  ↓
CreateBreakdownTask (shape11) READS process.DPP_BuildingID, process.DPP_LocationID
```

**Chain 3: Instruction Sets Lookup**
```
GetInstructionSetsByDto (shape27) → WRITES process.DPP_CategoryId, process.DDP_DisciplineId, process.DPP_PriorityId, process.DPP_InstructionId
  ↓
CreateBreakdownTask (shape11) READS process.DPP_CategoryId, process.DDP_DisciplineId, process.DPP_PriorityId, process.DPP_InstructionId
```

**Chain 4: Task Creation**
```
CreateBreakdownTask (shape11) → WRITES process.DPP_TaskId
  ↓
CreateEvent (shape35) READS process.DPP_TaskId (only if recurrence = Y)
```

**Chain 5: Task Existence Check**
```
GetBreakdownTasksByDto (shape53) → Returns CallId
  ↓
Decision (shape55) checks if CallId is empty
  ↓
If empty → Create new task (shape7 → shape11)
If not empty → Task exists, skip creation (shape57 → stop)
```

### Dependency Summary
- **FsiLogin** must execute FIRST (all operations depend on SessionId)
- **GetLocationsByDto** and **GetInstructionSetsByDto** must execute BEFORE **CreateBreakdownTask**
- **CreateBreakdownTask** must execute BEFORE **CreateEvent** (if recurrence = Y)
- **GetBreakdownTasksByDto** determines if task creation is needed (check-before-create pattern)

---

## 8. CONTROL FLOW GRAPH (Step 5)

### Main Process Flow
```
shape1 (start) → shape2 (Input_details)
shape2 → shape3 (Try/Catch)
shape3 (Try path) → shape4 (Branch - 6 paths)
shape3 (Catch path) → shape20 (Branch - 3 paths for error handling)

Branch shape4 (6 paths):
  Path 1: shape5 (FsiLogin subprocess) → shape6 (Stop if Login_Error)
  Path 2: shape50 (GetBreakdownTasksByDto message) → shape51 → shape52 → shape53 (GetBreakdownTasksByDto operation)
  Path 3: shape23 (GetLocationsByDto message) → shape40 → shape43 → shape24 (GetLocationsByDto operation)
  Path 4: shape26 (GetInstructionSetsByDto message) → shape39 → shape44 → shape27 (GetInstructionSetsByDto operation)
  Path 5: shape31 (Decision: recurrence != Y?) → shape7 (CreateBreakdownTask) OR shape32 (Branch for CreateEvent)
  Path 6: shape13 (FsiLogout subprocess)

shape53 (GetBreakdownTasksByDto) → shape54 → shape55 (Decision: CallId empty?)
  If TRUE (empty): shape57 (Stop - skip creation)
  If FALSE (not empty): shape58 (Branch - 2 paths) → shape61 (Map) → shape56 (Return)

shape24 (GetLocationsByDto) → shape38 → shape25 (Extract BuildingID, LocationID) → shape30 (Stop)

shape27 (GetInstructionSetsByDto) → shape37 → shape28 (Extract CategoryId, DisciplineId, PriorityId, InstructionId) → shape29 (Stop)

shape31 (Decision: recurrence != Y?):
  If TRUE (not Y): shape7 (Map) → shape9 → shape8 → shape11 (CreateBreakdownTask) → shape42 (Extract TaskId) → shape46 → shape12 (Decision: 20*?)
  If FALSE (Y): shape32 (Branch - 2 paths)
    Path 1: shape7 (Map) → shape9 → shape8 → shape11 (CreateBreakdownTask)
    Path 2: shape34 (CreateEvent message) → shape41 → shape45 → shape35 (CreateEvent operation) → shape36 → shape33 (Stop)

shape12 (Decision: 20*?):
  If TRUE: shape16 (Map success) → shape15 (Return)
  If FALSE: shape47 (Decision: 50*?)
    If TRUE: shape17 (Map server error) → shape15 (Return)
    If FALSE: shape48 (Set error message) → shape49 (Map error) → shape15 (Return)

Error handling (shape20 - 3 paths):
  Path 1: shape21 (Set error message) → shape19 (Email subprocess)
  Path 2: shape18 (Return failure)
  Path 3: shape22 (Exception)
```

### Reverse Flow Mapping (Convergence Points)
- **shape15** (Return Documents): Reached by multiple paths (shape16, shape17, shape49)
- **shape7** (Map for CreateBreakdownTask): Reached by shape31 (TRUE path) and shape32 (path 1)
- **No other convergence points** - most paths terminate independently

---

## 9. DECISION SHAPE ANALYSIS (Step 7)

### Decision Data Source Analysis

**Decision shape4 (FsiLogin subprocess):**
- **Data Source:** TRACK_PROPERTY (meta.base.applicationstatuscode from Login operation response)
- **Type:** POST_OPERATION (checks Login response status)
- **Actual Execution Order:** Login operation → Check response status → Decision routes
- **TRUE Path:** Extract SessionId → Continue (shape8 → shape9)
- **FALSE Path:** Set error → Return error (shape11 → shape6 → shape7)
- **Pattern:** Error Check (Success vs Failure)
- **Early Exit:** FALSE path leads to Return Documents (Login_Error)

**Decision shape31 (Is recurrence not Y?):**
- **Data Source:** INPUT (checks input profile field: recurrence)
- **Type:** PRE_FILTER (checks input data)
- **Actual Execution Order:** Decision checks input → Routes to appropriate path
- **TRUE Path:** Create breakdown task only (shape7 → CreateBreakdownTask)
- **FALSE Path:** Create breakdown task + CreateEvent (shape32 branch)
- **Pattern:** Conditional Logic (Optional Processing - recurring task handling)
- **Convergence:** Both paths eventually converge at shape7 (CreateBreakdownTask)

**Decision shape55 (CallId empty?):**
- **Data Source:** RESPONSE (checks GetBreakdownTasksByDto response - CallId field)
- **Type:** POST_OPERATION (checks operation response)
- **Actual Execution Order:** GetBreakdownTasksByDto → Check response CallId → Decision routes
- **TRUE Path:** CallId is empty → Task doesn't exist → Skip creation (shape57 stop)
- **FALSE Path:** CallId has value → Task exists → Continue with creation (shape58 branch → shape61 → shape56 return)
- **Pattern:** Existence Check (check-before-create - if exists, skip)
- **Early Exit:** TRUE path leads to Stop (skip creation)

**Decision shape12 (20*?):**
- **Data Source:** TRACK_PROPERTY (meta.base.applicationstatuscode from CreateBreakdownTask response)
- **Type:** POST_OPERATION (checks CreateBreakdownTask response status)
- **Actual Execution Order:** CreateBreakdownTask → Check response status → Decision routes
- **TRUE Path:** Success (20*) → Map success response (shape16 → shape15)
- **FALSE Path:** Not success → Check if server error (shape47)
- **Pattern:** Error Check (Success vs Failure)

**Decision shape47 (50*?):**
- **Data Source:** TRACK_PROPERTY (meta.base.applicationstatuscode from CreateBreakdownTask response)
- **Type:** POST_OPERATION (checks CreateBreakdownTask response status)
- **Actual Execution Order:** CreateBreakdownTask → Check status → Decision routes
- **TRUE Path:** Server error (50*) → Map server error (shape17 → shape15)
- **FALSE Path:** Other error → Set error message (shape48 → shape49 → shape15)
- **Pattern:** Error Check (Server Error vs Other Error)

### Self-Check Results
- ✅ Decision data sources identified: YES
- ✅ Decision types classified: YES (POST_OPERATION for shape4, shape12, shape47; PRE_FILTER for shape31; POST_OPERATION for shape55)
- ✅ Execution order verified: YES
- ✅ All decision paths traced: YES
- ✅ Decision patterns identified: YES (Error Check, Conditional Logic, Existence Check)
- ✅ Paths traced to termination: YES

---

## 10. BRANCH SHAPE ANALYSIS (Step 8)

### Branch shape4 (6 paths - Main Operations)

**Properties Analysis:**
- **Path 1 (FsiLogin):** WRITES process.DPP_SessionId
- **Path 2 (GetBreakdownTasksByDto):** READS process.DPP_SessionId
- **Path 3 (GetLocationsByDto):** READS process.DPP_SessionId, WRITES process.DPP_BuildingID, process.DPP_LocationID
- **Path 4 (GetInstructionSetsByDto):** READS process.DPP_SessionId, WRITES process.DPP_CategoryId, process.DDP_DisciplineId, process.DPP_PriorityId, process.DPP_InstructionId
- **Path 5 (CreateBreakdownTask decision):** READS process.DPP_SessionId, process.DPP_BuildingID, process.DPP_LocationID, process.DPP_CategoryId, process.DDP_DisciplineId, process.DPP_PriorityId, process.DPP_InstructionId
- **Path 6 (FsiLogout):** READS process.DPP_SessionId

**Dependency Graph:**
```
Path 1 (FsiLogin) → Path 2, 3, 4, 5, 6 (all depend on SessionId)
Path 3 (GetLocationsByDto) → Path 5 (depends on BuildingID, LocationID)
Path 4 (GetInstructionSetsByDto) → Path 5 (depends on CategoryId, DisciplineId, PriorityId, InstructionId)
Path 5 (CreateBreakdownTask) → Path 6 (Logout after task creation)
```

**Classification:** SEQUENTIAL (all paths contain API calls + data dependencies)

**Topological Sort Order:**
1. Path 1: FsiLogin (produces SessionId)
2. Path 2: GetBreakdownTasksByDto (checks existence)
3. Path 3: GetLocationsByDto (produces BuildingID, LocationID)
4. Path 4: GetInstructionSetsByDto (produces CategoryId, DisciplineId, PriorityId, InstructionId)
5. Path 5: CreateBreakdownTask (consumes all IDs from paths 3 & 4)
6. Path 6: FsiLogout (cleanup)

**Path Termination:**
- Path 1: shape6 (Stop if error) or continues
- Path 2: shape57 (Stop if exists) or shape56 (Return if not exists)
- Path 3: shape30 (Stop)
- Path 4: shape29 (Stop)
- Path 5: shape15 (Return Documents)
- Path 6: No explicit termination (subprocess returns)

**Convergence Points:** None - each path terminates independently or continues to next operation

**Execution Continues From:** N/A - sequential execution, no convergence

### Branch shape20 (3 paths - Error Handling)

**Properties Analysis:**
- **Path 1:** WRITES process.DPP_ErrorMessage, calls Email subprocess
- **Path 2:** Returns failure immediately
- **Path 3:** Throws exception

**Classification:** SEQUENTIAL (all paths contain operations)

**Path Termination:**
- Path 1: Email subprocess (no explicit termination)
- Path 2: shape18 (Return failure)
- Path 3: shape22 (Exception)

### Branch shape32 (2 paths - Recurring Task Handling)

**Properties Analysis:**
- **Path 1:** Continues to CreateBreakdownTask (shape7)
- **Path 2:** READS process.DPP_TaskId, calls CreateEvent

**Dependency Graph:**
```
Path 1 (CreateBreakdownTask) → Path 2 (CreateEvent depends on TaskId)
```

**Classification:** SEQUENTIAL (Path 2 depends on Path 1 - TaskId)

**Topological Sort Order:**
1. Path 1: CreateBreakdownTask (produces TaskId)
2. Path 2: CreateEvent (consumes TaskId)

**Path Termination:**
- Path 1: Continues to shape7 (CreateBreakdownTask flow)
- Path 2: shape33 (Stop)

### Branch shape58 (2 paths - Task Exists Handling)

**Properties Analysis:**
- **Path 1:** Maps response to return format
- **Path 2:** Stops immediately (task exists, skip)

**Classification:** PARALLEL (no data dependencies, simple routing)

**Path Termination:**
- Path 1: shape56 (Return Documents)
- Path 2: shape59 (Stop)

### Self-Check Results
- ✅ Classification completed: YES
- ✅ Assumption check: NO (analyzed dependencies)
- ✅ Properties extracted: YES
- ✅ Dependency graph built: YES
- ✅ Topological sort applied: YES

---

## 11. EXECUTION ORDER (Step 9)

### Business Logic Flow (Step 0 - MANDATORY FIRST)

**Operation Analysis:**

1. **FsiLogin (shape5 subprocess)**
   - **Purpose:** Authentication - Establishes session with CAFM
   - **Produces:** SessionId (process.DPP_SessionId)
   - **Dependent Operations:** ALL CAFM operations (GetLocationsByDto, GetInstructionSetsByDto, CreateBreakdownTask, GetBreakdownTasksByDto, CreateEvent, FsiLogout)
   - **Business Flow:** FsiLogin MUST execute FIRST (produces SessionId required by all operations)

2. **GetBreakdownTasksByDto (shape53)**
   - **Purpose:** Check if task already exists in CAFM
   - **Produces:** CallId (checked by decision shape55)
   - **Dependent Operations:** Decision shape55 determines if task creation is needed
   - **Business Flow:** GetBreakdownTasksByDto executes AFTER login, BEFORE task creation decision

3. **GetLocationsByDto (shape24)**
   - **Purpose:** Get location details (BuildingId, LocationId) from CAFM
   - **Produces:** BuildingID, LocationID (process.DPP_BuildingID, process.DPP_LocationID)
   - **Dependent Operations:** CreateBreakdownTask (uses BuildingID, LocationID)
   - **Business Flow:** GetLocationsByDto MUST execute BEFORE CreateBreakdownTask

4. **GetInstructionSetsByDto (shape27)**
   - **Purpose:** Get instruction set details (CategoryId, DisciplineId, PriorityId, InstructionId)
   - **Produces:** CategoryId, DisciplineId, PriorityId, InstructionId
   - **Dependent Operations:** CreateBreakdownTask (uses all IDs)
   - **Business Flow:** GetInstructionSetsByDto MUST execute BEFORE CreateBreakdownTask

5. **CreateBreakdownTask (shape11)**
   - **Purpose:** Create breakdown task in CAFM
   - **Produces:** TaskId (process.DPP_TaskId)
   - **Dependent Operations:** CreateEvent (uses TaskId if recurrence = Y)
   - **Business Flow:** CreateBreakdownTask executes AFTER GetLocationsByDto and GetInstructionSetsByDto, BEFORE CreateEvent

6. **CreateEvent (shape35)**
   - **Purpose:** Create event and link to task (for recurring tasks only)
   - **Produces:** None
   - **Dependent Operations:** None
   - **Business Flow:** CreateEvent executes ONLY if recurrence = Y, AFTER CreateBreakdownTask

7. **FsiLogout (shape13 subprocess)**
   - **Purpose:** Terminate CAFM session
   - **Produces:** None
   - **Dependent Operations:** None
   - **Business Flow:** FsiLogout executes LAST (cleanup after all operations)

### Execution Order List

**Based on dependency graph (Step 4), decision analysis (Step 7), control flow graph (Step 5), and branch analysis (Step 8):**

```
START (shape1)
  ↓
Input_details (shape2) - Extract input properties
  ↓
Try/Catch (shape3)
  ↓
Branch (shape4) - 6 paths SEQUENTIAL (API calls + dependencies):
  ↓
Path 1: FsiLogin (shape5 subprocess)
  ├─→ Decision: Login status 20*?
  │   ├─→ TRUE: Extract SessionId (shape8) → Continue (shape9)
  │   └─→ FALSE: Set error (shape11) → Return Login_Error (shape6 → shape7)
  ↓
Path 2: GetBreakdownTasksByDto (shape50 → shape53)
  ├─→ Decision (shape55): CallId empty?
  │   ├─→ TRUE: Task doesn't exist → Skip creation (shape57 stop)
  │   └─→ FALSE: Task exists → Continue (shape58 branch → shape61 map → shape56 return)
  ↓
Path 3: GetLocationsByDto (shape23 → shape24)
  ├─→ Extract BuildingID, LocationID (shape25) → Stop (shape30)
  ↓
Path 4: GetInstructionSetsByDto (shape26 → shape27)
  ├─→ Extract CategoryId, DisciplineId, PriorityId, InstructionId (shape28) → Stop (shape29)
  ↓
Path 5: Decision (shape31): recurrence != Y?
  ├─→ TRUE (not recurring): CreateBreakdownTask only (shape7 → shape11)
  │   └─→ Decision (shape12): 20*? → Map response → Return (shape15)
  └─→ FALSE (recurring): Branch (shape32 - 2 paths SEQUENTIAL):
      ├─→ Path 1: CreateBreakdownTask (shape7 → shape11)
      └─→ Path 2: CreateEvent (shape34 → shape35) → Stop (shape33)
  ↓
Path 6: FsiLogout (shape13 subprocess)
  ↓
END
```

### Dependency Verification

**Reference to Step 4 (Data Dependency Graph):**

1. **FsiLogin → ALL operations:**
   - FsiLogin writes process.DPP_SessionId
   - GetBreakdownTasksByDto, GetLocationsByDto, GetInstructionSetsByDto, CreateBreakdownTask, CreateEvent, FsiLogout all read process.DPP_SessionId
   - **Proof:** FsiLogin MUST execute BEFORE all other CAFM operations

2. **GetLocationsByDto → CreateBreakdownTask:**
   - GetLocationsByDto writes process.DPP_BuildingID, process.DPP_LocationID
   - CreateBreakdownTask reads process.DPP_BuildingID, process.DPP_LocationID
   - **Proof:** GetLocationsByDto MUST execute BEFORE CreateBreakdownTask

3. **GetInstructionSetsByDto → CreateBreakdownTask:**
   - GetInstructionSetsByDto writes process.DPP_CategoryId, process.DDP_DisciplineId, process.DPP_PriorityId, process.DPP_InstructionId
   - CreateBreakdownTask reads all these properties
   - **Proof:** GetInstructionSetsByDto MUST execute BEFORE CreateBreakdownTask

4. **CreateBreakdownTask → CreateEvent:**
   - CreateBreakdownTask writes process.DPP_TaskId
   - CreateEvent reads process.DPP_TaskId
   - **Proof:** CreateBreakdownTask MUST execute BEFORE CreateEvent

### Branch Execution Order

**Reference to Step 8 (Branch Analysis):**

Branch shape4 (6 paths) - SEQUENTIAL execution based on dependencies:
1. Path 1 (FsiLogin) - Produces SessionId
2. Path 2 (GetBreakdownTasksByDto) - Checks existence
3. Path 3 (GetLocationsByDto) - Produces BuildingID, LocationID
4. Path 4 (GetInstructionSetsByDto) - Produces CategoryId, DisciplineId, PriorityId, InstructionId
5. Path 5 (CreateBreakdownTask decision) - Consumes all IDs from paths 3 & 4
6. Path 6 (FsiLogout) - Cleanup

### Decision Path Tracing

**Decision shape31 (recurrence != Y?):**
- **TRUE path:** shape7 (Map) → shape9 (Notify) → shape8 (Set URL/SOAPAction) → shape11 (CreateBreakdownTask) → shape42 (Extract TaskId) → shape46 (Notify) → shape12 (Decision 20*?)
- **FALSE path:** shape32 (Branch) → Path 1: shape7 (CreateBreakdownTask) + Path 2: shape34 (CreateEvent) → shape33 (Stop)
- **Convergence:** Both paths converge at shape7 (CreateBreakdownTask)

**Decision shape55 (CallId empty?):**
- **TRUE path:** shape57 (Stop) - Task doesn't exist, skip creation
- **FALSE path:** shape58 (Branch) → shape61 (Map) → shape56 (Return) - Task exists, return response
- **No convergence:** Paths terminate independently

**Decision shape12 (20*?):**
- **TRUE path:** shape16 (Map success) → shape15 (Return)
- **FALSE path:** shape47 (Decision 50*?) → shape17 (Map server error) OR shape48 (Set error) → shape49 (Map error) → shape15 (Return)
- **Convergence:** All paths converge at shape15 (Return Documents)

### Self-Check Results
- ✅ Business logic verified FIRST: YES
- ✅ Operation analysis complete: YES
- ✅ Business logic execution order identified: YES
- ✅ Data dependencies checked FIRST: YES
- ✅ Operation response analysis used: YES (referenced Step 1c)
- ✅ Decision analysis used: YES (referenced Step 7)
- ✅ Dependency graph used: YES (referenced Step 4)
- ✅ Branch analysis used: YES (referenced Step 8)
- ✅ Property dependency verification: YES
- ✅ Topological sort applied: YES

---

## 12. SEQUENCE DIAGRAM (Step 10)

**Based on dependency graph (Step 4), decision analysis (Step 7), control flow graph (Step 5), branch analysis (Step 8), and execution order (Step 9):**

```
START (HTTP Request)
 |
 ├─→ Extract Input Properties (shape2)
 |   └─→ WRITES: DPP_Process_Name, DPP_AtomName, DPP_Payload, DPP_ExecutionID, DPP_File_Name, DPP_Subject, DPP_HasAttachment, To_Email, DPP_SourseSRNumber, DPP_sourseORGId
 |
 ├─→ Try/Catch Block (shape3)
 |   |
 |   ├─→ TRY PATH:
 |   |   |
 |   |   ├─→ Branch (shape4) - 6 paths SEQUENTIAL:
 |   |   |
 |   |   ├─→ Path 1: FsiLogin Subprocess (shape5)
 |   |   |   ├─→ Build SOAP request (username, password)
 |   |   |   ├─→ Operation: Authenticate (c20e5991-4d70-47f7-8e25-847df3e5eb6d)
 |   |   |   |   └─→ READS: FSI_Username, FSI_Password
 |   |   |   |   └─→ RESPONSE: SessionId, OperationResult
 |   |   |   ├─→ Decision: Status 20*?
 |   |   |   |   ├─→ TRUE: Extract SessionId (shape8)
 |   |   |   |   |   └─→ WRITES: process.DPP_SessionId
 |   |   |   |   |   └─→ Stop (continue=true) → SUCCESS RETURN
 |   |   |   |   └─→ FALSE: Set error (shape11)
 |   |   |   |       └─→ WRITES: process.DPP_CafmError
 |   |   |   |       └─→ Return Documents (Login_Error) → ERROR RETURN [EARLY EXIT]
 |   |   |   └─→ Main Process Decision: Login_Error returned?
 |   |   |       └─→ TRUE: Stop (shape6) [EARLY EXIT]
 |   |   |
 |   |   ├─→ Path 2: GetBreakdownTasksByDto (shape50 → shape53)
 |   |   |   ├─→ Build SOAP request
 |   |   |   |   └─→ READS: process.DPP_SessionId, serviceRequestNumber (input)
 |   |   |   ├─→ Operation: GetBreakdownTasksByDto (c52c74c2-95e3-4cba-990e-3ce4746836a2)
 |   |   |   |   └─→ RESPONSE: CallId, BreakdownTaskDtoV3
 |   |   |   ├─→ Decision (shape55): CallId equals "" (empty)?
 |   |   |   |   ├─→ TRUE: Task doesn't exist → Stop (shape57) [SKIP CREATION]
 |   |   |   |   └─→ FALSE: Task exists → Branch (shape58)
 |   |   |   |       ├─→ Path 1: Map response (shape61) → Return (shape56)
 |   |   |   |       └─→ Path 2: Stop (shape59)
 |   |   |
 |   |   ├─→ Path 3: GetLocationsByDto (shape23 → shape24)
 |   |   |   ├─→ Build SOAP request
 |   |   |   |   └─→ READS: process.DPP_SessionId, unitCode (input)
 |   |   |   ├─→ Operation: GetLocationsByDto (442683cb-b984-499e-b7bb-075c826905aa)
 |   |   |   |   └─→ RESPONSE: BuildingId, LocationId, LocationDto
 |   |   |   ├─→ Extract properties (shape25)
 |   |   |   |   └─→ WRITES: process.DPP_BuildingID, process.DPP_LocationID
 |   |   |   └─→ Stop (shape30)
 |   |   |
 |   |   ├─→ Path 4: GetInstructionSetsByDto (shape26 → shape27)
 |   |   |   ├─→ Build SOAP request
 |   |   |   |   └─→ READS: process.DPP_SessionId, subCategory (input)
 |   |   |   ├─→ Operation: GetInstructionSetsByDto (dc3b6b85-848d-471d-8c76-ed3b7dea0fbd)
 |   |   |   |   └─→ RESPONSE: IN_FKEY_CAT_SEQ, IN_FKEY_LAB_SEQ, IN_FKEY_PRI_SEQ, IN_SEQ
 |   |   |   ├─→ Extract properties (shape28)
 |   |   |   |   └─→ WRITES: process.DPP_CategoryId, process.DDP_DisciplineId, process.DPP_PriorityId, process.DPP_InstructionId
 |   |   |   └─→ Stop (shape29)
 |   |   |
 |   |   ├─→ Path 5: Decision (shape31): recurrence != Y?
 |   |   |   ├─→ TRUE (not recurring): CreateBreakdownTask only
 |   |   |   |   ├─→ Map request (shape7)
 |   |   |   |   ├─→ Notify (shape9)
 |   |   |   |   ├─→ Set URL/SOAPAction (shape8)
 |   |   |   |   ├─→ Operation: CreateBreakdownTask (33dac20f-ea09-471c-91c3-91b39bc3b172)
 |   |   |   |   |   └─→ READS: process.DPP_SessionId, process.DPP_BuildingID, process.DPP_LocationID, process.DPP_CategoryId, process.DDP_DisciplineId, process.DPP_PriorityId, process.DPP_InstructionId, input fields
 |   |   |   |   |   └─→ RESPONSE: TaskId
 |   |   |   |   ├─→ Extract TaskId (shape42)
 |   |   |   |   |   └─→ WRITES: process.DPP_TaskId
 |   |   |   |   ├─→ Notify (shape46)
 |   |   |   |   └─→ Decision (shape12): Status 20*?
 |   |   |   |       ├─→ TRUE: Map success (shape16) → Return (shape15)
 |   |   |   |       └─→ FALSE: Decision (shape47): Status 50*?
 |   |   |   |           ├─→ TRUE: Map server error (shape17) → Return (shape15)
 |   |   |   |           └─→ FALSE: Set error (shape48) → Map error (shape49) → Return (shape15)
 |   |   |   |
 |   |   |   └─→ FALSE (recurring): Branch (shape32) - 2 paths SEQUENTIAL:
 |   |   |       ├─→ Path 1: CreateBreakdownTask (shape7 → shape11) [Same as above]
 |   |   |       └─→ Path 2: CreateEvent (shape34 → shape35)
 |   |   |           ├─→ Build SOAP request
 |   |   |           |   └─→ READS: process.DPP_SessionId, process.DPP_TaskId, oldCAFMSRnumber (input)
 |   |   |           ├─→ Operation: CreateEvent (52166afd-a020-4de9-b49e-55400f1c0a7a)
 |   |   |           |   └─→ RESPONSE: Event created
 |   |   |           └─→ Stop (shape33)
 |   |   |
 |   |   └─→ Path 6: FsiLogout Subprocess (shape13)
 |   |       ├─→ Build SOAP request
 |   |       |   └─→ READS: process.DPP_SessionId
 |   |       └─→ Operation: Logout (381a025b-f3b9-4597-9902-3be49715c978)
 |   |
 |   └─→ CATCH PATH (shape20 - Error Handling):
 |       ├─→ Path 1: Set error message (shape21) → Email subprocess (shape19)
 |       ├─→ Path 2: Return failure (shape18)
 |       └─→ Path 3: Exception (shape22)
 |
 └─→ END
```

---

## 13. SUBPROCESS ANALYSIS

### Subprocess: FsiLogin (3d9db79d-15d0-4472-9f47-375ad9ab1ed2)

**Internal Flow:**
```
START (shape1)
  ↓
Set URL/SOAPAction (shape3)
  ↓
Build SOAP request (shape5) - Authenticate envelope with username/password
  ↓
Operation: Login (shape2 - c20e5991-4d70-47f7-8e25-847df3e5eb6d)
  ↓
Decision (shape4): Status 20*?
  ├─→ TRUE: Extract SessionId (shape8) → Stop (continue=true) [SUCCESS RETURN]
  └─→ FALSE: Set error (shape11) → Map error (shape6) → Return Documents (Login_Error) [ERROR RETURN]
```

**Return Paths:**
- **SUCCESS:** Stop (continue=true) - SessionId available in process.DPP_SessionId
- **ERROR:** Return Documents (Login_Error) - Main process handles error

**Properties Written:** process.DPP_SessionId (on success), process.DPP_CafmError (on failure)

**Properties Read:** FSI_Username, FSI_Password (from defined parameters)

### Subprocess: FsiLogout (b44c26cb-ecd5-4677-a752-434fe68f2e2b)

**Internal Flow:**
```
START (shape1)
  ↓
Build SOAP request (shape5) - LogOut envelope with sessionId
  ↓
Set URL/SOAPAction (shape4)
  ↓
Operation: Session logout (shape2 - 381a025b-f3b9-4597-9902-3be49715c978)
  ↓
Stop (continue=true) [SUCCESS RETURN]
```

**Return Paths:**
- **SUCCESS:** Stop (continue=true) - Logout completed

**Properties Read:** process.DPP_SessionId

### Subprocess: Office 365 Email (a85945c5-3004-42b9-80b1-104f465cd1fb)

**Internal Flow:**
```
START (shape1)
  ↓
Try/Catch (shape2)
  ├─→ TRY: Decision (shape4): DPP_HasAttachment equals Y?
  |   ├─→ TRUE: Build email with attachment (shape11) → Set mail properties (shape6) → Email operation (shape3 - af07502a-fafd-4976-a691-45d51a33b549)
  |   └─→ FALSE: Build email without attachment (shape23) → Set mail properties (shape20) → Email operation (shape7 - 15a72a21-9b57-49a1-a8ed-d70367146644)
  └─→ CATCH: Exception (shape10)
  ↓
Stop (continue=true) [SUCCESS RETURN]
```

**Return Paths:**
- **SUCCESS:** Stop (continue=true) - Email sent

**Properties Read:** process.To_Email, process.DPP_Subject, process.DPP_MailBody, process.DPP_File_Name, process.DPP_HasAttachment

---

## 14. CRITICAL PATTERNS IDENTIFIED

### Pattern 1: Check-Before-Create (GetBreakdownTasksByDto → Decision → CreateBreakdownTask)
- **Identification:** GetBreakdownTasksByDto checks if task exists (by CallId)
- **Decision:** shape55 checks if CallId is empty
- **If empty (TRUE):** Task doesn't exist → Skip creation (shape57 stop)
- **If not empty (FALSE):** Task exists → Continue with creation (shape58 → shape61 → shape56)
- **Execution Rule:** Check operation MUST execute BEFORE create operation

### Pattern 2: Topological Sort for Branch Paths (Branch shape4)
- **6 paths with data dependencies**
- **Dependency graph:**
  - Path 1 (FsiLogin) → All other paths (SessionId dependency)
  - Path 3 (GetLocationsByDto) → Path 5 (BuildingID, LocationID dependency)
  - Path 4 (GetInstructionSetsByDto) → Path 5 (CategoryId, DisciplineId, PriorityId, InstructionId dependency)
  - Path 5 (CreateBreakdownTask) → Path 6 (Logout after creation)
- **Topological sort order:** Path 1 → Path 2 → Path 3 → Path 4 → Path 5 → Path 6
- **All paths contain API calls → SEQUENTIAL execution (no parallel API calls)**

### Pattern 3: Early Exit Detection
- **FsiLogin subprocess:** If login fails (status != 20*), return Login_Error → Main process stops (shape6)
- **GetBreakdownTasksByDto:** If CallId is empty (task doesn't exist), stop (shape57) - skip creation

### Pattern 4: Subprocess Return Paths
- **FsiLogin:** Returns "Login_Error" label if authentication fails → Main process routes to shape6 (Stop)
- **FsiLogout:** No explicit return paths - always succeeds
- **Office 365 Email:** No explicit return paths - always succeeds (try/catch handles errors)

### Pattern 5: Conditional Logic (Recurrence Handling)
- **Decision shape31:** Checks if recurrence != Y
- **If TRUE (not recurring):** Create breakdown task only (shape7 → shape11)
- **If FALSE (recurring):** Create breakdown task + CreateEvent (shape32 branch)
- **Both paths converge at shape7 (CreateBreakdownTask)**

---

## 15. VALIDATION CHECKLIST

### Data Dependencies
- [x] All property WRITES identified (10 properties)
- [x] All property READS identified (10 properties)
- [x] Dependency graph built (4 chains)
- [x] Execution order satisfies all dependencies (no read-before-write)

### Decision Analysis
- [x] ALL decision shapes inventoried (5 decisions)
- [x] BOTH TRUE and FALSE paths traced to termination
- [x] Pattern type identified for each decision
- [x] Early exits identified and documented (2 early exits)
- [x] Convergence points identified (shape15, shape7)

### Branch Analysis
- [x] Each branch classified as parallel or sequential
- [x] Branch shape4: SEQUENTIAL (API calls + dependencies)
- [x] Branch shape20: SEQUENTIAL (error handling operations)
- [x] Branch shape32: SEQUENTIAL (TaskId dependency)
- [x] Branch shape58: PARALLEL (no dependencies, simple routing)
- [x] Dependency order built using topological sort
- [x] Each path traced to terminal point
- [x] Convergence points identified
- [x] Execution continuation point determined

### Sequence Diagram
- [x] Format follows required structure
- [x] Each operation shows READS and WRITES
- [x] Decisions show both TRUE and FALSE paths
- [x] Check-before-create patterns shown correctly
- [x] Sequence diagram matches control flow graph (Step 5)
- [x] Execution order matches dependency graph (Step 4)
- [x] Early exits marked [EARLY EXIT]
- [x] Conditional execution marked [Only if condition X]
- [x] Subprocess internal flows documented
- [x] Subprocess return paths mapped to main process

### Subprocess Analysis
- [x] ALL subprocesses analyzed (3 subprocesses)
- [x] Return paths identified (success and error)
- [x] Return path labels mapped to main process shapes
- [x] Properties written by subprocess documented
- [x] Properties read by subprocess from main process documented

### Input/Output Structure Analysis
- [x] Entry point operation identified (de68dad0-be76-4ec8-9857-4e5cf2a7bd4c)
- [x] Request profile identified and loaded (af096014-313f-4565-9091-2bdd56eb46df)
- [x] Request profile structure analyzed (JSON)
- [x] Array vs single object detected (Array)
- [x] Array cardinality documented (minOccurs: 0, maxOccurs: -1)
- [x] ALL request fields extracted (14 fields + nested ticketDetails with 9 fields)
- [x] Request field paths documented
- [x] Response profile identified and loaded (9e542ed5-2c65-4af8-b0c6-821cbc58ca31)
- [x] Response profile structure analyzed (JSON array)
- [x] ALL response fields extracted (5 fields)
- [x] Document processing behavior determined (array splitting)

---

## 16. FIELD MAPPING ANALYSIS

### Request Field Mapping

| Boomi Field Path | Boomi Field Name | Data Type | Required | Azure DTO Property | Notes |
|---|---|---|---|---|---|
| Root/Object/workOrder/Array/ArrayElement1/Object/reporterName | reporterName | character | No | ReporterName | Reporter contact info |
| Root/Object/workOrder/Array/ArrayElement1/Object/reporterEmail | reporterEmail | character | No | ReporterEmail | Reporter contact info |
| Root/Object/workOrder/Array/ArrayElement1/Object/reporterPhoneNumber | reporterPhoneNumber | character | No | ReporterPhoneNumber | Reporter contact info |
| Root/Object/workOrder/Array/ArrayElement1/Object/description | description | character | No | Description | Work order description |
| Root/Object/workOrder/Array/ArrayElement1/Object/serviceRequestNumber | serviceRequestNumber | character | No | ServiceRequestNumber | Unique identifier from source |
| Root/Object/workOrder/Array/ArrayElement1/Object/propertyName | propertyName | character | No | PropertyName | Property name |
| Root/Object/workOrder/Array/ArrayElement1/Object/unitCode | unitCode | character | No | UnitCode | Used in GetLocationsByDto |
| Root/Object/workOrder/Array/ArrayElement1/Object/categoryName | categoryName | character | No | CategoryName | Category name |
| Root/Object/workOrder/Array/ArrayElement1/Object/subCategory | subCategory | character | No | SubCategory | Used in GetInstructionSetsByDto |
| Root/Object/workOrder/Array/ArrayElement1/Object/technician | technician | character | No | Technician | Assigned technician |
| Root/Object/workOrder/Array/ArrayElement1/Object/sourceOrgId | sourceOrgId | character | No | SourceOrgId | Source organization ID |
| Root/Object/workOrder/Array/ArrayElement1/Object/ticketDetails/Object/status | status | character | No | Status | Ticket status |
| Root/Object/workOrder/Array/ArrayElement1/Object/ticketDetails/Object/subStatus | subStatus | character | No | SubStatus | Ticket sub-status |
| Root/Object/workOrder/Array/ArrayElement1/Object/ticketDetails/Object/priority | priority | character | No | Priority | Ticket priority |
| Root/Object/workOrder/Array/ArrayElement1/Object/ticketDetails/Object/scheduledDate | scheduledDate | character | No | ScheduledDate | Scheduled date |
| Root/Object/workOrder/Array/ArrayElement1/Object/ticketDetails/Object/scheduledTimeStart | scheduledTimeStart | character | No | ScheduledTimeStart | Scheduled start time |
| Root/Object/workOrder/Array/ArrayElement1/Object/ticketDetails/Object/scheduledTimeEnd | scheduledTimeEnd | character | No | ScheduledTimeEnd | Scheduled end time |
| Root/Object/workOrder/Array/ArrayElement1/Object/ticketDetails/Object/recurrence | recurrence | character | No | Recurrence | Recurrence flag (Y/N) |
| Root/Object/workOrder/Array/ArrayElement1/Object/ticketDetails/Object/oldCAFMSRnumber | oldCAFMSRnumber | character | No | OldCAFMSRnumber | Old CAFM SR number |
| Root/Object/workOrder/Array/ArrayElement1/Object/ticketDetails/Object/raisedDateUtc | raisedDateUtc | character | Yes | RaisedDateUtc | Raised date (UTC) |

### Response Field Mapping

| Boomi Field Path | Boomi Field Name | Data Type | Azure DTO Property | Notes |
|---|---|---|---|---|
| Root/Object/workOrder/Array/ArrayElement1/Object/cafmSRNumber | cafmSRNumber | character | CafmSRNumber | CAFM service request number (required) |
| Root/Object/workOrder/Array/ArrayElement1/Object/sourceSRNumber | sourceSRNumber | character | SourceSRNumber | Source SR number |
| Root/Object/workOrder/Array/ArrayElement1/Object/sourceOrgId | sourceOrgId | character | SourceOrgId | Source organization ID |
| Root/Object/workOrder/Array/ArrayElement1/Object/status | status | character | Status | Processing status |
| Root/Object/workOrder/Array/ArrayElement1/Object/message | message | character | Message | Status message |

---

## 17. FUNCTION EXPOSURE DECISION TABLE

| Operation | Independent Invoke? | Decision Before/After? | Same SOR? | Internal Lookup? | Conclusion | Reasoning |
|---|---|---|---|---|---|---|
| FsiLogin | NO | YES (AFTER - status check) | N/A | N/A | Atomic (Middleware) | Session-based auth - handled by middleware |
| FsiLogout | NO | None | N/A | N/A | Atomic (Middleware) | Session cleanup - handled by middleware |
| GetLocationsByDto | NO | None | YES (CAFM) | YES - lookup | Atomic | Internal lookup for BuildingID/LocationID |
| GetInstructionSetsByDto | NO | None | YES (CAFM) | YES - lookup | Atomic | Internal lookup for CategoryId/DisciplineId/PriorityId/InstructionId |
| CreateBreakdownTask | YES | YES (check-before-create) | YES (CAFM) | NO | **Function** | Main operation - Handler orchestrates GetLocations, GetInstructionSets, CreateBreakdownTask internally (same SOR) |
| GetBreakdownTasksByDto | NO | YES (AFTER - existence check) | YES (CAFM) | YES - check | Atomic | Internal check for task existence |
| CreateEvent | NO | YES (BEFORE - recurrence check) | YES (CAFM) | NO | Atomic | Conditional operation (only if recurrence = Y) - Handler orchestrates internally |
| SendEmail | NO | None | N/A | N/A | Skip | Process Layer responsibility (notification) |

### Decision Summary

**I will create 1 Azure Function for CAFM:**

**CreateWorkOrderAPI** - Main work order creation function

**Reasoning:**
- All CAFM operations are same SOR (FSI Evolution CAFM system)
- GetLocationsByDto and GetInstructionSetsByDto are internal lookups (field extraction for CreateBreakdownTask)
- GetBreakdownTasksByDto is internal check (existence verification)
- CreateEvent is conditional operation (only if recurrence = Y) - Handler orchestrates internally
- Per Rule 1066, business decisions → Process Layer when operations span different SORs
- Since all operations are same SOR, Handler orchestrates internally with simple if/else and check-before-create patterns

**Functions:**
- **CreateWorkOrderAPI:** Creates work order in CAFM (orchestrates GetLocations, GetInstructionSets, check existence, CreateBreakdownTask, conditionally CreateEvent)

**Internal (Atomic Handlers):**
- AuthenticateAtomicHandler (for middleware)
- LogoutAtomicHandler (for middleware)
- GetLocationsByDtoAtomicHandler
- GetInstructionSetsByDtoAtomicHandler
- CreateBreakdownTaskAtomicHandler
- GetBreakdownTasksByDtoAtomicHandler
- CreateEventAtomicHandler

**Auth Method:** Session-based (middleware with AuthenticateAtomicHandler + LogoutAtomicHandler)

---

## 18. SYSTEM LAYER IMPLEMENTATION PLAN

### Repository Identification
- **Target SOR:** CAFM (FSI Evolution)
- **Expected Repository:** sys-cafm-mgmt OR sys-fsi-mgmt
- **Action:** Check if repository exists, if not create new System Layer project

### Components to Create

**1. Middleware Components (Session-based auth):**
- `Attributes/CustomAuthenticationAttribute.cs`
- `Middleware/CustomAuthenticationMiddleware.cs`
- `Middleware/RequestContext.cs`
- `Implementations/FSI/AtomicHandlers/AuthenticateAtomicHandler.cs`
- `Implementations/FSI/AtomicHandlers/LogoutAtomicHandler.cs`

**2. SOAP Components:**
- `Helper/CustomSoapClient.cs` (YOU MUST CREATE - NOT in Framework)
- `Helper/SOAPHelper.cs`
- `SoapEnvelopes/Authenticate.xml`
- `SoapEnvelopes/Logout.xml`
- `SoapEnvelopes/GetLocationsByDto.xml`
- `SoapEnvelopes/GetInstructionSetsByDto.xml`
- `SoapEnvelopes/CreateBreakdownTask.xml`
- `SoapEnvelopes/GetBreakdownTasksByDto.xml`
- `SoapEnvelopes/CreateEvent.xml`

**3. Azure Function:**
- `Functions/CreateWorkOrderAPI.cs`

**4. Service & Abstraction:**
- `Abstractions/IWorkOrderMgmt.cs`
- `Implementations/FSI/Services/WorkOrderMgmtService.cs`

**5. Handler:**
- `Implementations/FSI/Handlers/CreateWorkOrderHandler.cs`

**6. Atomic Handlers:**
- `Implementations/FSI/AtomicHandlers/GetLocationsByDtoAtomicHandler.cs`
- `Implementations/FSI/AtomicHandlers/GetInstructionSetsByDtoAtomicHandler.cs`
- `Implementations/FSI/AtomicHandlers/CreateBreakdownTaskAtomicHandler.cs`
- `Implementations/FSI/AtomicHandlers/GetBreakdownTasksByDtoAtomicHandler.cs`
- `Implementations/FSI/AtomicHandlers/CreateEventAtomicHandler.cs`

**7. DTOs:**
- `DTO/HandlerDTOs/CreateWorkOrderDTO/CreateWorkOrderReqDTO.cs`
- `DTO/HandlerDTOs/CreateWorkOrderDTO/CreateWorkOrderResDTO.cs`
- `DTO/AtomicHandlerDTOs/AuthenticationRequestDTO.cs`
- `DTO/AtomicHandlerDTOs/LogoutRequestDTO.cs`
- `DTO/AtomicHandlerDTOs/GetLocationsByDtoHandlerReqDTO.cs`
- `DTO/AtomicHandlerDTOs/GetInstructionSetsByDtoHandlerReqDTO.cs`
- `DTO/AtomicHandlerDTOs/CreateBreakdownTaskHandlerReqDTO.cs`
- `DTO/AtomicHandlerDTOs/GetBreakdownTasksByDtoHandlerReqDTO.cs`
- `DTO/AtomicHandlerDTOs/CreateEventHandlerReqDTO.cs`
- `DTO/DownstreamDTOs/AuthenticationResponseDTO.cs`
- `DTO/DownstreamDTOs/GetLocationsByDtoApiResDTO.cs`
- `DTO/DownstreamDTOs/GetInstructionSetsByDtoApiResDTO.cs`
- `DTO/DownstreamDTOs/CreateBreakdownTaskApiResDTO.cs`
- `DTO/DownstreamDTOs/GetBreakdownTasksByDtoApiResDTO.cs`
- `DTO/DownstreamDTOs/CreateEventApiResDTO.cs`

**8. Configuration:**
- `ConfigModels/AppConfigs.cs` (extend)
- `Constants/ErrorConstants.cs`
- `Constants/InfoConstants.cs`
- `Constants/OperationNames.cs`
- `appsettings.json`, `appsettings.dev.json`, `appsettings.qa.json`, `appsettings.prod.json`

**9. Program.cs:**
- Register all components in correct order
- Register middleware: ExecutionTimingMiddleware → ExceptionHandlerMiddleware → CustomAuthenticationMiddleware

---

## 19. SELF-CHECK VALIDATION

### Step 1a (Input Structure Analysis)
- [x] Section "Input Structure Analysis (Step 1a)" exists and is complete
- [x] Request profile analyzed with array detection
- [x] All fields extracted and documented
- [x] Field mapping table created

### Step 1b (Response Structure Analysis)
- [x] Section "Response Structure Analysis (Step 1b)" exists and is complete
- [x] Response profile analyzed
- [x] All response fields extracted and documented

### Step 1c (Operation Response Analysis)
- [x] Section "Operation Response Analysis (Step 1c)" exists and is complete
- [x] All operations with response profiles analyzed
- [x] Extracted fields documented
- [x] Data consumers identified

### Step 4 (Data Dependency Graph)
- [x] Section "Data Dependency Graph (Step 4)" exists and is complete
- [x] All dependencies documented with proof
- [x] Dependency chains shown

### Step 5 (Control Flow Graph)
- [x] Section "Control Flow Graph (Step 5)" exists and is complete
- [x] All dragpoint connections mapped
- [x] Reverse flow mapping completed

### Step 7 (Decision Analysis)
- [x] Section "Decision Shape Analysis (Step 7)" exists and is complete
- [x] All self-check answers shown with YES
- [x] Decision data source analysis completed
- [x] Decision type classification completed
- [x] Actual execution order verified

### Step 8 (Branch Analysis)
- [x] Section "Branch Shape Analysis (Step 8)" exists and is complete
- [x] All self-check answers shown with YES
- [x] Dependencies analyzed
- [x] Classification completed (SEQUENTIAL for API-containing branches)
- [x] Topological sort applied

### Step 9 (Execution Order)
- [x] Section "Execution Order (Step 9)" exists and is complete
- [x] All self-check answers shown with YES
- [x] Business logic verified FIRST
- [x] Dependency graph referenced
- [x] Branch analysis referenced

### Step 10 (Sequence Diagram)
- [x] Section "Sequence Diagram (Step 10)" exists and is complete
- [x] References all prerequisite sections (Steps 4, 5, 7, 8, 9)
- [x] Shows operations with READS/WRITES
- [x] Shows decision paths

---

## 20. CONCLUSION

**Phase 1 Analysis Complete:**
- Total operations analyzed: 10 (6 main + 4 subprocess)
- Third-party systems identified: 2 (CAFM FSI Evolution - SOAP, Office 365 Email - SMTP)
- System Layer Functions to generate: 1 (CreateWorkOrderAPI)
- Authentication pattern: Session-based (middleware)
- All validation checklists completed
- Ready to proceed to Phase 2 (System Layer Implementation)

**Key Architectural Decisions:**
1. **Single Function:** CreateWorkOrderAPI (all CAFM operations same SOR)
2. **Handler Orchestration:** Handler orchestrates 5 Atomic Handlers internally (GetLocations, GetInstructionSets, CheckTaskExists, CreateBreakdownTask, CreateEvent)
3. **Middleware Auth:** CustomAuthenticationMiddleware handles Login/Logout lifecycle
4. **Check-Before-Create:** GetBreakdownTasksByDto checks existence before creation
5. **Conditional Logic:** CreateEvent only executes if recurrence = Y
6. **Email Handling:** Skipped (Process Layer responsibility for notifications)

---

**END OF PHASE 1 ANALYSIS**
