# PHASE 1: BOOMI PROCESS EXTRACTION ANALYSIS
## Create Work Order from EQ+ to CAFM

**Process Name:** Create Work Order from EQ+ to CAFM  
**Process ID:** cf0ab01d-2ce4-4588-8265-54fc4290368a  
**System of Record:** FSI CAFM (Facilities System International)  
**Integration Type:** SOAP/XML + SMTP Email

---

## 1. Operations Inventory

### Main Process Operations
| Operation ID | Operation Name | Type | Purpose |
|---|---|---|---|
| de68dad0-be76-4ec8-9857-4e5cf2a7bd4c | EQ+_CAFM_Create | wss (Listen) | Entry point - receives work order requests |
| 33dac20f-ea09-471c-91c3-91b39bc3b172 | CreateBreakdownTask Order EQ+ | http/SOAP | Creates breakdown task in CAFM |
| 442683cb-b984-499e-b7bb-075c826905aa | GetLocationsByDto_CAFM | http/SOAP | Retrieves location details by unit code |
| dc3b6b85-848d-471d-8c76-ed3b7dea0fbd | GetInstructionSetsByDto_CAFM | http/SOAP | Retrieves instruction sets by description |
| 52166afd-a020-4de9-b49e-55400f1c0a7a | CreateEvent/Link task CAFM | http/SOAP | Creates event and links to task (recurring) |
| c52c74c2-95e3-4cba-990e-3ce4746836a2 | GetBreakdownTasksByDto_CAFM | http/SOAP | Retrieves existing breakdown tasks by CallId |

### Subprocess Operations
| Subprocess ID | Subprocess Name | Operation ID | Operation Name | Type | Purpose |
|---|---|---|---|---|---|
| 3d9db79d-15d0-4472-9f47-375ad9ab1ed2 | FsiLogin | c20e5991-4d70-47f7-8e25-847df3e5eb6d | Login | http/SOAP | Authenticates and returns SessionId |
| b44c26cb-ecd5-4677-a752-434fe68f2e2b | FsiLogout | 381a025b-f3b9-4597-9902-3be49715c978 | Session logout | http/SOAP | Logs out session |
| a85945c5-3004-42b9-80b1-104f465cd1fb | Office 365 Email | af07502a-fafd-4976-a691-45d51a33b549 | Email w Attachment | mail/SMTP | Sends email with attachment |
| a85945c5-3004-42b9-80b1-104f465cd1fb | Office 365 Email | 15a72a21-9b57-49a1-a8ed-d70367146644 | Email W/O Attachment | mail/SMTP | Sends email without attachment |

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
- **Boomi Processing:** Each array element triggers separate process execution
- **Azure Function Requirement:** Must accept array and process each element
- **Implementation Pattern:** Loop through array, process each work order, aggregate results

---

## 3. Field Mapping Analysis (Step 1a Continued)

### Request Field Mapping

| Boomi Field Path | Boomi Field Name | Data Type | Required | Azure DTO Property | Notes |
|---|---|---|---|---|---|
| Root/Object/workOrder/Array/ArrayElement1/Object/reporterName | reporterName | character | No | ReporterName | Reporter contact info |
| Root/Object/workOrder/Array/ArrayElement1/Object/reporterEmail | reporterEmail | character | No | ReporterEmail | Reporter contact info |
| Root/Object/workOrder/Array/ArrayElement1/Object/reporterPhoneNumber | reporterPhoneNumber | character | No | ReporterPhoneNumber | Reporter contact info |
| Root/Object/workOrder/Array/ArrayElement1/Object/description | description | character | No | Description | Work order description |
| Root/Object/workOrder/Array/ArrayElement1/Object/serviceRequestNumber | serviceRequestNumber | character | No | ServiceRequestNumber | Unique identifier from source |
| Root/Object/workOrder/Array/ArrayElement1/Object/propertyName | propertyName | character | No | PropertyName | Property/building name |
| Root/Object/workOrder/Array/ArrayElement1/Object/unitCode | unitCode | character | No | UnitCode | Unit/location code |
| Root/Object/workOrder/Array/ArrayElement1/Object/categoryName | categoryName | character | No | CategoryName | Work order category |
| Root/Object/workOrder/Array/ArrayElement1/Object/subCategory | subCategory | character | No | SubCategory | Work order sub-category |
| Root/Object/workOrder/Array/ArrayElement1/Object/technician | technician | character | No | Technician | Assigned technician |
| Root/Object/workOrder/Array/ArrayElement1/Object/sourceOrgId | sourceOrgId | character | No | SourceOrgId | Source organization ID |
| Root/Object/workOrder/Array/ArrayElement1/Object/ticketDetails/Object/status | status | character | No | Status | Ticket status |
| Root/Object/workOrder/Array/ArrayElement1/Object/ticketDetails/Object/subStatus | subStatus | character | No | SubStatus | Ticket sub-status |
| Root/Object/workOrder/Array/ArrayElement1/Object/ticketDetails/Object/priority | priority | character | No | Priority | Ticket priority |
| Root/Object/workOrder/Array/ArrayElement1/Object/ticketDetails/Object/scheduledDate | scheduledDate | character | No | ScheduledDate | Scheduled date |
| Root/Object/workOrder/Array/ArrayElement1/Object/ticketDetails/Object/scheduledTimeStart | scheduledTimeStart | character | No | ScheduledTimeStart | Scheduled start time |
| Root/Object/workOrder/Array/ArrayElement1/Object/ticketDetails/Object/scheduledTimeEnd | scheduledTimeEnd | character | No | ScheduledTimeEnd | Scheduled end time |
| Root/Object/workOrder/Array/ArrayElement1/Object/ticketDetails/Object/recurrence | recurrence | character | No | Recurrence | Recurrence flag (Y/N) |
| Root/Object/workOrder/Array/ArrayElement1/Object/ticketDetails/Object/oldCAFMSRnumber | oldCAFMSRnumber | character | No | OldCAFMSRNumber | Old CAFM service request number |
| Root/Object/workOrder/Array/ArrayElement1/Object/ticketDetails/Object/raisedDateUtc | raisedDateUtc | character | Yes | RaisedDateUtc | Ticket raised date (UTC) |

---

## 4. Response Structure Analysis (Step 1b)

### Response Profile Structure
- **Profile ID:** 9e542ed5-2c65-4af8-b0c6-821cbc58ca31
- **Profile Name:** EQ+_CAFM_Create_Response (assumed from operation)
- **Profile Type:** profile.json
- **Response Type:** singlejson

### Response Field Mapping
Process Layer expects standard response with:
- Success/failure status
- Created task IDs
- Error messages (if any)

---

## 5. Operation Response Analysis (Step 1c)

### Operation: Login (c20e5991-4d70-47f7-8e25-847df3e5eb6d)
- **Response Profile:** 992136d3-da44-4f22-994b-f7181624215b (Login_Response)
- **Response Structure:** SOAP XML - Envelope/Body/AuthenticateResponse/AuthenticateResult/SessionId
- **Extracted Fields:**
  - SessionId (extracted by shape8, written to process.DPP_SessionId)
- **Consumers:** All subsequent SOAP operations (GetLocationsByDto, GetInstructionSetsByDto, CreateBreakdownTask, CreateEvent, GetBreakdownTasksByDto)
- **Business Logic:** Login MUST execute FIRST - all operations depend on SessionId

### Operation: GetLocationsByDto (442683cb-b984-499e-b7bb-075c826905aa)
- **Response Profile:** 3aa0f5c5-8c95-4023-aba9-9d78dd6ade96
- **Response Structure:** SOAP XML - LocationDto with BuildingId, LocationId
- **Extracted Fields:**
  - BuildingId (extracted by shape25, written to process.DPP_BuildingID)
  - LocationId (extracted by shape25, written to process.DPP_LocationID)
- **Consumers:** CreateBreakdownTask operation uses BuildingId and LocationId
- **Business Logic:** GetLocationsByDto MUST execute BEFORE CreateBreakdownTask

### Operation: GetInstructionSetsByDto (dc3b6b85-848d-471d-8c76-ed3b7dea0fbd)
- **Response Profile:** 5c2f13dd-3e51-4a7c-867b-c801aaa35562
- **Response Structure:** SOAP XML - FINFILEDto with CategoryId, DisciplineId, PriorityId, InstructionId
- **Extracted Fields:**
  - IN_FKEY_CAT_SEQ (extracted by shape28, written to process.DPP_CategoryId)
  - IN_FKEY_LAB_SEQ (extracted by shape28, written to process.DDP_DisciplineId)
  - IN_FKEY_PRI_SEQ (extracted by shape28, written to process.DPP_PriorityId)
  - IN_SEQ (extracted by shape28, written to process.DPP_InstructionId)
- **Consumers:** CreateBreakdownTask operation uses these IDs
- **Business Logic:** GetInstructionSetsByDto MUST execute BEFORE CreateBreakdownTask

### Operation: CreateBreakdownTask (33dac20f-ea09-471c-91c3-91b39bc3b172)
- **Response Profile:** dbcca2ef-55cc-48e0-9329-1e8db4ada0c8
- **Response Structure:** SOAP XML - CreateBreakdownTaskResult/TaskId
- **Extracted Fields:**
  - TaskId (extracted by shape42, written to process.DPP_TaskId)
- **Consumers:** CreateEvent operation uses TaskId (if recurrence=Y)
- **Business Logic:** CreateBreakdownTask MUST execute BEFORE CreateEvent (for recurring tasks)

### Operation: GetBreakdownTasksByDto (c52c74c2-95e3-4cba-990e-3ce4746836a2)
- **Response Profile:** 1570c9d2-0588-410d-ad3d-bdc7d5c0ec9a
- **Response Structure:** SOAP XML - BreakdownTaskDtoV3/CallId
- **Extracted Fields:**
  - CallId (checked by shape55 decision)
- **Consumers:** Decision shape55 checks if CallId is empty (task exists check)
- **Business Logic:** GetBreakdownTasksByDto checks if task already exists - if exists, skip creation

---

## 6. Process Properties Analysis (Steps 2-3)

### Properties WRITTEN
| Property Name | Written By | Source |
|---|---|---|
| process.DPP_SessionId | shape8 (FsiLogin subprocess) | Login response SessionId |
| process.DPP_BuildingID | shape25 | GetLocationsByDto response BuildingId |
| process.DPP_LocationID | shape25 | GetLocationsByDto response LocationId |
| process.DPP_CategoryId | shape28 | GetInstructionSetsByDto response IN_FKEY_CAT_SEQ |
| process.DDP_DisciplineId | shape28 | GetInstructionSetsByDto response IN_FKEY_LAB_SEQ |
| process.DPP_PriorityId | shape28 | GetInstructionSetsByDto response IN_FKEY_PRI_SEQ |
| process.DPP_InstructionId | shape28 | GetInstructionSetsByDto response IN_SEQ |
| process.DPP_TaskId | shape42 | CreateBreakdownTask response TaskId |
| process.DPP_Process_Name | shape2 | Execution property "Process Name" |
| process.DPP_AtomName | shape2 | Execution property "Atom Name" |
| process.DPP_ExecutionID | shape2 | Execution property "Execution Id" |
| process.DPP_Payload | shape2 | Current document (input payload) |
| process.DPP_File_Name | shape2 | Defined parameter + timestamp |
| process.DPP_Subject | shape2 | Email subject (for error notification) |
| process.DPP_HasAttachment | shape2 | Defined parameter Has_Attachment |
| process.To_Email | shape2 | Defined parameter To_Email |
| process.DPP_ErrorMessage | shape21 | Try/Catch error message |
| process.DPP_CafmError | shape11 (FsiLogin) | Static: "CAFM Log In Failed" |
| process.DPP_CafmResponse | shape48 | Static: "No Response From CAFM API" |

### Properties READ
| Property Name | Read By | Usage |
|---|---|---|
| process.DPP_SessionId | shape23, shape26, shape34 (message shapes) | Used in all SOAP requests after login |
| process.DPP_BuildingID | (implied in CreateBreakdownTask) | Used in task creation |
| process.DPP_LocationID | (implied in CreateBreakdownTask) | Used in task creation |
| process.DPP_CategoryId | (implied in CreateBreakdownTask) | Used in task creation |
| process.DDP_DisciplineId | (implied in CreateBreakdownTask) | Used in task creation |
| process.DPP_PriorityId | (implied in CreateBreakdownTask) | Used in task creation |
| process.DPP_InstructionId | (implied in CreateBreakdownTask) | Used in task creation |
| process.DPP_TaskId | shape34 (CreateEvent message) | Used in CreateEvent for recurring tasks |
| process.DPP_HasAttachment | shape4 (Email subprocess decision) | Determines email with/without attachment |
| process.To_Email | shape6, shape20 (Email subprocess) | Email recipient |
| process.DPP_Subject | shape6, shape20 (Email subprocess) | Email subject |
| process.DPP_MailBody | shape6, shape20 (Email subprocess) | Email body content |
| process.DPP_File_Name | shape6 (Email subprocess) | Attachment filename |
| process.DPP_ErrorMessage | shape11, shape23 (Email subprocess) | Error details for notification |

---

## 7. Data Dependency Graph (Step 4)

### Dependency Chains

**Chain 1: Authentication**
```
FsiLogin (shape5) → Writes process.DPP_SessionId
  ↓
All SOAP operations READ process.DPP_SessionId
  - GetLocationsByDto (shape23)
  - GetInstructionSetsByDto (shape26)
  - GetBreakdownTasksByDto (shape50)
  - CreateBreakdownTask (shape7)
  - CreateEvent (shape34)
```

**Chain 2: Location Lookup**
```
GetLocationsByDto (shape24) → Writes process.DPP_BuildingID, process.DPP_LocationID
  ↓
CreateBreakdownTask (shape11) READS BuildingID, LocationID
```

**Chain 3: Instruction Sets Lookup**
```
GetInstructionSetsByDto (shape27) → Writes process.DPP_CategoryId, DDP_DisciplineId, DPP_PriorityId, DPP_InstructionId
  ↓
CreateBreakdownTask (shape11) READS CategoryId, DisciplineId, PriorityId, InstructionId
```

**Chain 4: Task Creation for Recurring**
```
CreateBreakdownTask (shape11) → Writes process.DPP_TaskId
  ↓
CreateEvent (shape35) READS DPP_TaskId (only if recurrence=Y)
```

**Chain 5: Check Before Create**
```
GetBreakdownTasksByDto (shape53) → Returns CallId
  ↓
Decision (shape55) checks if CallId is empty
  - If empty (TRUE) → Task doesn't exist → Stop (skip creation)
  - If not empty (FALSE) → Task exists → Continue to update/link
```

### Dependency Summary
1. **FsiLogin MUST execute FIRST** - All operations depend on SessionId
2. **GetLocationsByDto and GetInstructionSetsByDto are PARALLEL** - No dependencies between them, but both must complete before CreateBreakdownTask
3. **CreateBreakdownTask depends on:** SessionId + BuildingID + LocationID + CategoryId + DisciplineId + PriorityId + InstructionId
4. **CreateEvent depends on:** SessionId + TaskId (only for recurring tasks)
5. **GetBreakdownTasksByDto is CHECK operation** - Determines if task already exists
6. **FsiLogout executes LAST** - After all operations complete

---

## 8. Control Flow Graph (Step 5)

### Main Process Control Flow

```
shape1 (START - Entry Point)
  ↓
shape2 (Set Process Properties)
  ↓
shape3 (Try/Catch)
  ├─ Try (default) → shape4 (Branch - 6 paths)
  └─ Catch (error) → shape20 (Branch - 3 paths for error handling)
```

### Branch shape4 (6 paths - SEQUENTIAL due to data dependencies)

**Path 1:** Login subprocess
```
shape5 (ProcessCall: FsiLogin)
  ├─ Success → shape6 (Stop continue=true)
  └─ Login_Error → shape6 (Stop continue=true)
```

**Path 2:** Check if task exists
```
shape50 (Message: GetBreakdownTasksByDto request)
  ↓
shape51 (Set URL/SOAPAction)
  ↓
shape52 (Notify)
  ↓
shape53 (ConnectorAction: GetBreakdownTasksByDto)
  ↓
shape54 (Notify)
  ↓
shape55 (Decision: CallId equals "")
  ├─ TRUE (empty = not exists) → shape57 (Stop continue=true) [EARLY EXIT]
  └─ FALSE (has value = exists) → shape58 (Branch - 2 paths)
```

**Path 3:** Get Location Details
```
shape23 (Message: GetLocationsByDto request)
  ↓
shape40 (Set URL/SOAPAction)
  ↓
shape43 (Notify)
  ↓
shape24 (ConnectorAction: GetLocationsByDto)
  ↓
shape38 (Notify)
  ↓
shape25 (Extract BuildingID, LocationID)
  ↓
shape30 (Stop continue=true)
```

**Path 4:** Get Instruction Sets
```
shape26 (Message: GetInstructionSetsByDto request)
  ↓
shape39 (Set URL/SOAPAction)
  ↓
shape44 (Notify)
  ↓
shape27 (ConnectorAction: GetInstructionSetsByDto)
  ↓
shape37 (Notify)
  ↓
shape28 (Extract CategoryId, DisciplineId, PriorityId, InstructionId)
  ↓
shape29 (Stop continue=true)
```

**Path 5:** Create Task (Recurring vs Non-Recurring)
```
shape31 (Decision: recurrence not equals "Y")
  ├─ TRUE (not recurring) → shape7 (Map) → shape9 (Notify) → shape8 (Set URL/SOAPAction) → shape11 (CreateBreakdownTask) → shape42 (Extract TaskId) → shape46 (Notify) → shape12 (Decision: 20*?)
  └─ FALSE (recurring) → shape32 (Branch - 2 paths)
      ├─ Path 1: shape7 (same as above)
      └─ Path 2: shape34 (CreateEvent request) → shape41 (Set URL/SOAPAction) → shape45 (Notify) → shape35 (CreateEvent) → shape36 (Notify) → shape33 (Stop)
```

**Path 6:** Logout subprocess
```
shape13 (ProcessCall: FsiLogout)
```

### Decision shape12 (After CreateBreakdownTask)
```
Decision: HTTP status code matches "20*"?
  ├─ TRUE (success) → shape16 (Map success response) → shape15 (Return Documents)
  └─ FALSE (error) → shape47 (Decision: status matches "50*"?)
      ├─ TRUE (server error) → shape17 (Map error response) → shape15 (Return Documents)
      └─ FALSE (other error) → shape48 (Set error message) → shape49 (Map error response) → shape15 (Return Documents)
```

### Error Handling Branch (shape20 - Catch path)
```
shape20 (Branch - 3 paths)
  ├─ Path 1: shape21 (Set error message) → shape19 (Email subprocess)
  ├─ Path 2: shape18 (Return Documents - Failure)
  └─ Path 3: shape22 (Exception - throw error)
```

---

## 9. Decision Shape Analysis (Step 7)

### ✅ Decision data sources identified: YES
### ✅ Decision types classified: YES
### ✅ Execution order verified: YES
### ✅ All decision paths traced: YES
### ✅ Decision patterns identified: YES
### ✅ Paths traced to termination: YES

### Decision 1: FsiLogin subprocess (shape4 in subprocess)
- **Shape ID:** shape4 (subprocess 3d9db79d-15d0-4472-9f47-375ad9ab1ed2)
- **Comparison:** regex match on meta.base.applicationstatuscode
- **Value 1:** Track property (HTTP status code)
- **Value 2:** Static "20*"
- **Data Source:** RESPONSE (from Login API call)
- **Decision Type:** POST-OPERATION (checks API response status)
- **Actual Execution Order:** Login API → Check Response Status → Decision routes
- **TRUE Path:** shape8 (Extract SessionId) → shape9 (Stop continue=true) [SUCCESS]
- **FALSE Path:** shape11 (Set error property) → shape6 (Map error) → shape7 (Return "Login_Error") [ERROR RETURN]
- **Pattern:** Error Check (Success vs Failure)
- **Convergence:** None (paths terminate differently)
- **Early Exit:** FALSE path returns "Login_Error" to main process

### Decision 2: Email Attachment Check (shape4 in Email subprocess)
- **Shape ID:** shape4 (subprocess a85945c5-3004-42b9-80b1-104f465cd1fb)
- **Comparison:** equals
- **Value 1:** Process property DPP_HasAttachment
- **Value 2:** Static "Y"
- **Data Source:** PROCESS_PROPERTY (set by main process from input)
- **Decision Type:** PRE-FILTER (checks input data)
- **Actual Execution Order:** Decision → Route to appropriate email operation
- **TRUE Path:** shape11 (Build email body) → shape14 (Set mail body) → shape15 (Set payload) → shape6 (Set mail properties) → shape3 (Send email WITH attachment) → shape5 (Stop)
- **FALSE Path:** shape23 (Build email body) → shape22 (Set mail body) → shape20 (Set mail properties) → shape7 (Send email WITHOUT attachment) → shape9 (Stop)
- **Pattern:** Conditional Logic (Optional Processing)
- **Convergence:** Both paths converge at Stop (success)
- **Early Exit:** None

### Decision 3: Recurrence Check (shape31)
- **Shape ID:** shape31
- **Comparison:** notequals
- **Value 1:** Profile element "recurrence" from input
- **Value 2:** Static "Y"
- **Data Source:** INPUT (from request profile)
- **Decision Type:** PRE-FILTER (checks input data)
- **Actual Execution Order:** Decision → Route based on recurrence flag
- **TRUE Path (not recurring):** shape7 (Map) → ... → shape11 (CreateBreakdownTask) → shape42 (Extract TaskId) → shape46 (Notify) → shape12 (Status check decision)
- **FALSE Path (recurring):** shape32 (Branch - 2 paths)
  - Path 1: Same as TRUE path (CreateBreakdownTask)
  - Path 2: shape34 (CreateEvent) → shape41 (Set props) → shape45 (Notify) → shape35 (CreateEvent API) → shape36 (Notify) → shape33 (Stop)
- **Pattern:** Conditional Logic (Recurring task handling)
- **Convergence:** Both paths eventually reach shape12 decision
- **Early Exit:** None

### Decision 4: CreateBreakdownTask Status Check (shape12)
- **Shape ID:** shape12
- **Comparison:** regex match
- **Value 1:** Track property meta.base.applicationstatuscode
- **Value 2:** Static "20*"
- **Data Source:** RESPONSE (from CreateBreakdownTask API)
- **Decision Type:** POST-OPERATION (checks API response status)
- **Actual Execution Order:** CreateBreakdownTask API → Check Response Status → Decision routes
- **TRUE Path (success 2xx):** shape16 (Map success response) → shape15 (Return Documents)
- **FALSE Path (error):** shape47 (Decision: 50*?)
- **Pattern:** Error Check (Success vs Failure)
- **Convergence:** All paths converge at shape15 (Return Documents)
- **Early Exit:** None (all paths return)

### Decision 5: Server Error Check (shape47)
- **Shape ID:** shape47
- **Comparison:** wildcard match
- **Value 1:** Track property meta.base.applicationstatuscode
- **Value 2:** Static "50*"
- **Data Source:** RESPONSE (from CreateBreakdownTask API)
- **Decision Type:** POST-OPERATION (checks API response status)
- **Actual Execution Order:** (After shape12 FALSE path) → Check if server error → Decision routes
- **TRUE Path (server error 5xx):** shape17 (Map error response) → shape15 (Return Documents)
- **FALSE Path (other error):** shape48 (Set error message) → shape49 (Map error response) → shape15 (Return Documents)
- **Pattern:** Error Classification (Server Error vs Other Error)
- **Convergence:** Both paths converge at shape15 (Return Documents)
- **Early Exit:** None (all paths return)

### Decision 6: Task Exists Check (shape55)
- **Shape ID:** shape55
- **Comparison:** equals
- **Value 1:** Profile element "CallId" from GetBreakdownTasksByDto response
- **Value 2:** Static "" (empty string)
- **Data Source:** RESPONSE (from GetBreakdownTasksByDto API)
- **Decision Type:** POST-OPERATION (checks API response data)
- **Actual Execution Order:** GetBreakdownTasksByDto API → Check Response CallId → Decision routes
- **TRUE Path (empty = task doesn't exist):** shape57 (Stop continue=true) [EARLY EXIT - Skip creation]
- **FALSE Path (has value = task exists):** shape58 (Branch - 2 paths)
  - Path 1: shape61 (Map) → shape56 (Return Documents)
  - Path 2: shape59 (Stop continue=false) [Process termination]
- **Pattern:** Check-Before-Create (Skip if exists)
- **Convergence:** None (paths terminate differently)
- **Early Exit:** TRUE path stops (skips creation)

---

## 10. Branch Shape Analysis (Step 8)

### ✅ Classification completed: YES
### ✅ Assumption check: NO (analyzed dependencies)
### ✅ Properties extracted: YES
### ✅ Dependency graph built: YES
### ✅ Topological sort applied: YES

### Branch shape4 (Main Process - 6 paths)

**Classification:** SEQUENTIAL (paths have data dependencies AND contain API calls)

**Path Analysis:**

**Path 1 (Login):**
- **Shapes:** shape5 (ProcessCall: FsiLogin)
- **WRITES:** process.DPP_SessionId
- **READS:** None
- **API Calls:** YES (Login SOAP API)

**Path 2 (Check Task Exists):**
- **Shapes:** shape50 → shape51 → shape52 → shape53 → shape54 → shape55 → shape57/shape58
- **WRITES:** None (only checks)
- **READS:** process.DPP_SessionId
- **API Calls:** YES (GetBreakdownTasksByDto SOAP API)

**Path 3 (Get Location):**
- **Shapes:** shape23 → shape40 → shape43 → shape24 → shape38 → shape25 → shape30
- **WRITES:** process.DPP_BuildingID, process.DPP_LocationID
- **READS:** process.DPP_SessionId
- **API Calls:** YES (GetLocationsByDto SOAP API)

**Path 4 (Get Instruction Sets):**
- **Shapes:** shape26 → shape39 → shape44 → shape27 → shape37 → shape28 → shape29
- **WRITES:** process.DPP_CategoryId, DDP_DisciplineId, DPP_PriorityId, DPP_InstructionId
- **READS:** process.DPP_SessionId
- **API Calls:** YES (GetInstructionSetsByDto SOAP API)

**Path 5 (Create Task):**
- **Shapes:** shape31 → shape7/shape32 → shape9 → shape8 → shape11 → shape42 → shape46 → shape12 → shape16/shape47
- **WRITES:** process.DPP_TaskId
- **READS:** process.DPP_SessionId, process.DPP_BuildingID, process.DPP_LocationID, process.DPP_CategoryId, DDP_DisciplineId, DPP_PriorityId, DPP_InstructionId
- **API Calls:** YES (CreateBreakdownTask SOAP API, optionally CreateEvent SOAP API)

**Path 6 (Logout):**
- **Shapes:** shape13 (ProcessCall: FsiLogout)
- **WRITES:** None
- **READS:** process.DPP_SessionId
- **API Calls:** YES (Logout SOAP API)

**Dependency Graph:**
```
Path 1 (Login) → Writes SessionId
  ↓
Path 2 (Check) depends on SessionId
Path 3 (GetLocation) depends on SessionId
Path 4 (GetInstructionSets) depends on SessionId
  ↓
Path 5 (CreateTask) depends on SessionId + BuildingID + LocationID + CategoryId + DisciplineId + PriorityId + InstructionId
  ↓
Path 6 (Logout) depends on SessionId
```

**Topological Sort Order:**
1. Path 1 (Login) - MUST execute first
2. Path 2 (Check) - Can execute after login
3. Path 3 (GetLocation) - Can execute after login (parallel with Path 4)
4. Path 4 (GetInstructionSets) - Can execute after login (parallel with Path 3)
5. Path 5 (CreateTask) - MUST execute after Paths 3 and 4 complete
6. Path 6 (Logout) - MUST execute last

**Path Terminals:**
- Path 1: shape6 (Stop continue=true)
- Path 2: shape57 (Stop continue=true) OR shape58 (Branch → shape56/shape59)
- Path 3: shape30 (Stop continue=true)
- Path 4: shape29 (Stop continue=true)
- Path 5: shape15 (Return Documents)
- Path 6: No explicit terminal (subprocess completes)

**Convergence Points:**
- shape15 (Return Documents) - Final convergence point for all success/error paths

**Execution Continues From:** shape15 (Return Documents) - Process ends

---

### Branch shape20 (Error Handling - 3 paths)

**Classification:** SEQUENTIAL (contains API calls)

**Path 1:** Send error email
- **Shapes:** shape21 (Set error message) → shape19 (ProcessCall: Email)
- **API Calls:** YES (SMTP email)

**Path 2:** Return failure
- **Shapes:** shape18 (Return Documents - Failure)
- **API Calls:** NO

**Path 3:** Throw exception
- **Shapes:** shape22 (Exception)
- **API Calls:** NO

**Topological Sort Order:** Sequential execution (Path 1 → Path 2 → Path 3)

---

### Branch shape32 (Recurring Task - 2 paths)

**Classification:** SEQUENTIAL (contains API calls)

**Path 1:** Create breakdown task only
- **Shapes:** shape7 → shape9 → shape8 → shape11 (CreateBreakdownTask) → ...
- **API Calls:** YES (CreateBreakdownTask SOAP API)

**Path 2:** Create event and link to task
- **Shapes:** shape34 → shape41 → shape45 → shape35 (CreateEvent) → shape36 → shape33
- **API Calls:** YES (CreateEvent SOAP API)

**Dependency:** Path 2 depends on Path 1 (needs TaskId from CreateBreakdownTask)

**Topological Sort Order:** Path 1 → Path 2 (sequential)

---

### Branch shape58 (Task Exists Handling - 2 paths)

**Classification:** SEQUENTIAL (no API calls, but business logic routing)

**Path 1:** Map existing task response
- **Shapes:** shape61 (Map) → shape56 (Return Documents)
- **API Calls:** NO

**Path 2:** Stop process
- **Shapes:** shape59 (Stop continue=false)
- **API Calls:** NO

**Topological Sort Order:** Path 1 → Path 2 (sequential)

---

## 11. Execution Order (Step 9)

### ✅ Business logic verified FIRST: YES
### ✅ Operation analysis complete: YES
### ✅ Business logic execution order identified: YES
### ✅ Data dependencies checked FIRST: YES
### ✅ Operation response analysis used: YES (Step 1c)
### ✅ Decision analysis used: YES (Step 7)
### ✅ Dependency graph used: YES (Step 4)
### ✅ Branch analysis used: YES (Step 8)
### ✅ Property dependency verification: YES
### ✅ Topological sort applied: YES

### Business Logic Flow (Step 0)

**Operation 1: FsiLogin (Authentication)**
- **Purpose:** Establishes session with CAFM system
- **Produces:** SessionId (written to process.DPP_SessionId)
- **Dependent Operations:** ALL subsequent SOAP operations (GetLocationsByDto, GetInstructionSetsByDto, GetBreakdownTasksByDto, CreateBreakdownTask, CreateEvent, FsiLogout)
- **Business Flow:** FsiLogin MUST execute FIRST (produces SessionId required by all operations)

**Operation 2: GetBreakdownTasksByDto (Check Exists)**
- **Purpose:** Checks if breakdown task already exists for given CallId
- **Produces:** CallId (checked by decision shape55)
- **Dependent Operations:** Decision determines if CreateBreakdownTask should execute
- **Business Flow:** GetBreakdownTasksByDto executes AFTER Login, BEFORE CreateBreakdownTask (check-before-create pattern)

**Operation 3: GetLocationsByDto (Lookup)**
- **Purpose:** Retrieves location details (BuildingId, LocationId) by unit code
- **Produces:** BuildingId, LocationId (written to process.DPP_BuildingID, process.DPP_LocationID)
- **Dependent Operations:** CreateBreakdownTask uses BuildingId and LocationId
- **Business Flow:** GetLocationsByDto MUST execute BEFORE CreateBreakdownTask

**Operation 4: GetInstructionSetsByDto (Lookup)**
- **Purpose:** Retrieves instruction set details (CategoryId, DisciplineId, PriorityId, InstructionId) by description
- **Produces:** CategoryId, DisciplineId, PriorityId, InstructionId (written to process properties)
- **Dependent Operations:** CreateBreakdownTask uses these IDs
- **Business Flow:** GetInstructionSetsByDto MUST execute BEFORE CreateBreakdownTask

**Operation 5: CreateBreakdownTask (Create)**
- **Purpose:** Creates breakdown task in CAFM system
- **Produces:** TaskId (written to process.DPP_TaskId)
- **Dependent Operations:** CreateEvent uses TaskId (if recurring)
- **Business Flow:** CreateBreakdownTask executes AFTER GetLocationsByDto and GetInstructionSetsByDto complete, BEFORE CreateEvent (for recurring tasks)

**Operation 6: CreateEvent (Link Recurring)**
- **Purpose:** Creates event and links to task (for recurring tasks only)
- **Produces:** Event created
- **Dependent Operations:** None
- **Business Flow:** CreateEvent executes AFTER CreateBreakdownTask (only if recurrence=Y)

**Operation 7: FsiLogout (Cleanup)**
- **Purpose:** Logs out session from CAFM system
- **Produces:** None
- **Dependent Operations:** None
- **Business Flow:** FsiLogout executes LAST (after all operations complete)

### Actual Execution Order

**Based on dependency graph (Step 4) and branch analysis (Step 8):**

```
1. START (shape1) - Entry point
2. Set Process Properties (shape2) - Initialize execution context
3. Try/Catch (shape3) - Error handling wrapper
4. Branch (shape4) - 6 parallel paths (SEQUENTIAL due to dependencies)
   
   SEQUENTIAL EXECUTION ORDER:
   
   4.1. Path 1: FsiLogin subprocess (shape5)
        - MUST execute FIRST
        - Produces: SessionId
        - Decision: Check login status (20*?)
          - SUCCESS: Extract SessionId → Continue
          - FAILURE: Return "Login_Error" → Stop
   
   4.2. Path 2: GetBreakdownTasksByDto (shape50-53)
        - Depends on: SessionId
        - Produces: CallId (for existence check)
        - Decision (shape55): CallId equals ""?
          - TRUE (empty = not exists): Stop [EARLY EXIT - Skip creation]
          - FALSE (has value = exists): Continue to update/link
   
   4.3. Path 3: GetLocationsByDto (shape23-24) [PARALLEL with Path 4]
        - Depends on: SessionId
        - Produces: BuildingId, LocationId
        - No dependencies with Path 4
   
   4.4. Path 4: GetInstructionSetsByDto (shape26-27) [PARALLEL with Path 3]
        - Depends on: SessionId
        - Produces: CategoryId, DisciplineId, PriorityId, InstructionId
        - No dependencies with Path 3
   
   4.5. Path 5: CreateBreakdownTask (shape7-11)
        - Depends on: SessionId, BuildingId, LocationId, CategoryId, DisciplineId, PriorityId, InstructionId
        - MUST execute AFTER Paths 3 and 4 complete
        - Decision (shape31): recurrence not equals "Y"?
          - TRUE (not recurring): Create task only
          - FALSE (recurring): Branch (shape32)
            - Path 1: Create task
            - Path 2: CreateEvent (shape34-35) - Link recurring task
        - Produces: TaskId
        - Decision (shape12): Status code 20*?
          - TRUE: Map success → Return
          - FALSE: Decision (shape47): Status code 50*?
            - TRUE: Map server error → Return
            - FALSE: Set error message → Map error → Return
   
   4.6. Path 6: FsiLogout subprocess (shape13)
        - Depends on: SessionId
        - MUST execute LAST (cleanup)

5. Return Documents (shape15) - Final response
```

### Dependency Verification (References Step 4)

**Verified Dependencies:**
- ✅ Path 1 (Login) writes SessionId → Paths 2-6 read SessionId → Path 1 MUST execute before Paths 2-6
- ✅ Path 3 (GetLocation) writes BuildingId, LocationId → Path 5 (CreateTask) reads them → Path 3 MUST execute before Path 5
- ✅ Path 4 (GetInstructionSets) writes CategoryId, DisciplineId, PriorityId, InstructionId → Path 5 (CreateTask) reads them → Path 4 MUST execute before Path 5
- ✅ Path 5 (CreateTask) writes TaskId → CreateEvent (within Path 5) reads TaskId → CreateTask MUST execute before CreateEvent
- ✅ All operations read SessionId → Login MUST execute first
- ✅ CreateTask reads data from Paths 3 and 4 → Paths 3 and 4 MUST execute before Path 5

**Branch Execution Order (References Step 8):**
- Branch shape4: SEQUENTIAL execution (Path 1 → Path 2 → [Path 3 || Path 4] → Path 5 → Path 6)
- Branch shape32: SEQUENTIAL execution (Path 1 → Path 2)
- Branch shape58: SEQUENTIAL execution (Path 1 → Path 2)

---

## 12. Sequence Diagram (Step 10)

**Based on dependency graph in Step 4, control flow graph in Step 5, decision analysis in Step 7, branch analysis in Step 8, and execution order in Step 9.**

```
START (Entry Point)
 |
 ├─→ Set Process Properties (shape2)
 |    └─→ WRITES: DPP_Process_Name, DPP_AtomName, DPP_ExecutionID, DPP_Payload, DPP_File_Name, DPP_Subject, DPP_HasAttachment, To_Email, DPP_SourseSRNumber, DPP_sourseORGId
 |
 ├─→ Try/Catch (shape3)
 |    |
 |    ├─→ TRY PATH:
 |    |    |
 |    |    ├─→ Branch (shape4) - 6 SEQUENTIAL paths
 |    |    |
 |    |    ├─→ PATH 1: FsiLogin Subprocess (shape5) [MUST EXECUTE FIRST]
 |    |    |    └─→ SUBPROCESS INTERNAL FLOW:
 |    |    |         ├─→ START
 |    |    |         ├─→ Set URL/SOAPAction (shape3)
 |    |    |         ├─→ Build SOAP Request (shape5)
 |    |    |         ├─→ Operation: Login (shape2)
 |    |    |         |    └─→ SOAP: Authenticate(username, password)
 |    |    |         ├─→ Decision (shape4): Status Code "20*"?
 |    |    |         |    ├─→ IF TRUE (Success):
 |    |    |         |    |    └─→ Extract SessionId (shape8)
 |    |    |         |    |         └─→ WRITES: process.DPP_SessionId
 |    |    |         |    |         └─→ Stop (continue=true) [SUCCESS RETURN]
 |    |    |         |    |
 |    |    |         |    └─→ IF FALSE (Failure):
 |    |    |         |         └─→ Set Error Property (shape11)
 |    |    |         |              └─→ WRITES: process.DPP_CafmError = "CAFM Log In Failed"
 |    |    |         |              └─→ Map Error (shape6)
 |    |    |         |              └─→ Return Documents ("Login_Error") [ERROR RETURN]
 |    |    |         |
 |    |    |         └─→ END SUBPROCESS
 |    |    |    |
 |    |    |    └─→ Main Process Decision: Subprocess returned "Login_Error"?
 |    |    |         ├─→ IF TRUE → Stop (shape6) [EARLY EXIT]
 |    |    |         └─→ IF FALSE → Continue (SessionId available)
 |    |    |
 |    |    ├─→ PATH 2: GetBreakdownTasksByDto (shape50-53) [CHECK OPERATION]
 |    |    |    └─→ READS: process.DPP_SessionId
 |    |    |    └─→ Build SOAP Request (shape50)
 |    |    |    └─→ Set URL/SOAPAction (shape51)
 |    |    |    └─→ Operation: GetBreakdownTasksByDto (shape53)
 |    |    |         └─→ SOAP: GetBreakdownTasksByDto(sessionId, serviceRequestNumber)
 |    |    |    └─→ Decision (shape55): CallId equals ""?
 |    |    |         ├─→ IF TRUE (empty = task doesn't exist):
 |    |    |         |    └─→ Stop (shape57) [EARLY EXIT - Skip creation]
 |    |    |         |
 |    |    |         └─→ IF FALSE (task exists):
 |    |    |              └─→ Branch (shape58)
 |    |    |                   ├─→ Path 1: Map response (shape61) → Return (shape56)
 |    |    |                   └─→ Path 2: Stop (shape59)
 |    |    |
 |    |    ├─→ PATH 3: GetLocationsByDto (shape23-24) [PARALLEL with PATH 4]
 |    |    |    └─→ READS: process.DPP_SessionId
 |    |    |    └─→ Build SOAP Request (shape23)
 |    |    |    └─→ Set URL/SOAPAction (shape40)
 |    |    |    └─→ Operation: GetLocationsByDto (shape24)
 |    |    |         └─→ SOAP: GetLocationsByDto(sessionId, unitCode)
 |    |    |    └─→ Extract Properties (shape25)
 |    |    |         └─→ WRITES: process.DPP_BuildingID, process.DPP_LocationID
 |    |    |    └─→ Stop (shape30)
 |    |    |
 |    |    ├─→ PATH 4: GetInstructionSetsByDto (shape26-27) [PARALLEL with PATH 3]
 |    |    |    └─→ READS: process.DPP_SessionId
 |    |    |    └─→ Build SOAP Request (shape26)
 |    |    |    └─→ Set URL/SOAPAction (shape39)
 |    |    |    └─→ Operation: GetInstructionSetsByDto (shape27)
 |    |    |         └─→ SOAP: GetInstructionSetsByDto(sessionId, subCategory)
 |    |    |    └─→ Extract Properties (shape28)
 |    |    |         └─→ WRITES: process.DPP_CategoryId, process.DDP_DisciplineId, process.DPP_PriorityId, process.DPP_InstructionId
 |    |    |    └─→ Stop (shape29)
 |    |    |
 |    |    ├─→ PATH 5: CreateBreakdownTask (shape7-11) [MUST EXECUTE AFTER PATH 3 & 4]
 |    |    |    └─→ READS: process.DPP_SessionId, process.DPP_BuildingID, process.DPP_LocationID, process.DPP_CategoryId, process.DDP_DisciplineId, process.DPP_PriorityId, process.DPP_InstructionId
 |    |    |    |
 |    |    |    └─→ Decision (shape31): recurrence not equals "Y"?
 |    |    |         |
 |    |    |         ├─→ IF TRUE (not recurring):
 |    |    |         |    └─→ Map Request (shape7)
 |    |    |         |    └─→ Set URL/SOAPAction (shape8)
 |    |    |         |    └─→ Operation: CreateBreakdownTask (shape11)
 |    |    |         |         └─→ SOAP: CreateBreakdownTask(sessionId, dto)
 |    |    |         |    └─→ Extract TaskId (shape42)
 |    |    |         |         └─→ WRITES: process.DPP_TaskId
 |    |    |         |
 |    |    |         └─→ IF FALSE (recurring):
 |    |    |              └─→ Branch (shape32) - 2 SEQUENTIAL paths
 |    |    |                   |
 |    |    |                   ├─→ Path 1: CreateBreakdownTask (same as above)
 |    |    |                   |
 |    |    |                   └─→ Path 2: CreateEvent (shape34-35)
 |    |    |                        └─→ READS: process.DPP_SessionId, process.DPP_TaskId
 |    |    |                        └─→ Build SOAP Request (shape34)
 |    |    |                        └─→ Set URL/SOAPAction (shape41)
 |    |    |                        └─→ Operation: CreateEvent (shape35)
 |    |    |                             └─→ SOAP: CreateEvent(sessionId, taskId, comments)
 |    |    |                        └─→ Stop (shape33)
 |    |    |    |
 |    |    |    └─→ Decision (shape12): Status Code "20*"?
 |    |    |         |
 |    |    |         ├─→ IF TRUE (Success):
 |    |    |         |    └─→ Map Success Response (shape16)
 |    |    |         |    └─→ Return Documents (shape15)
 |    |    |         |
 |    |    |         └─→ IF FALSE (Error):
 |    |    |              └─→ Decision (shape47): Status Code "50*"?
 |    |    |                   |
 |    |    |                   ├─→ IF TRUE (Server Error):
 |    |    |                   |    └─→ Map Error Response (shape17)
 |    |    |                   |    └─→ Return Documents (shape15)
 |    |    |                   |
 |    |    |                   └─→ IF FALSE (Other Error):
 |    |    |                        └─→ Set Error Message (shape48)
 |    |    |                        └─→ Map Error Response (shape49)
 |    |    |                        └─→ Return Documents (shape15)
 |    |    |
 |    |    └─→ PATH 6: FsiLogout Subprocess (shape13) [MUST EXECUTE LAST]
 |    |         └─→ SUBPROCESS INTERNAL FLOW:
 |    |              ├─→ START
 |    |              ├─→ Build SOAP Request (shape5)
 |    |              ├─→ Set URL/SOAPAction (shape4)
 |    |              ├─→ Operation: Logout (shape2)
 |    |              |    └─→ SOAP: LogOut(sessionId)
 |    |              └─→ Stop (continue=true)
 |    |
 |    └─→ CATCH PATH (Error Handling):
 |         └─→ Branch (shape20) - 3 paths
 |              ├─→ Path 1: Set error message (shape21) → Email subprocess (shape19)
 |              ├─→ Path 2: Return Documents "Failure" (shape18)
 |              └─→ Path 3: Throw Exception (shape22)
 |
 └─→ END
```

---

## 13. Subprocess Analysis

### Subprocess 1: FsiLogin (3d9db79d-15d0-4472-9f47-375ad9ab1ed2)

**Internal Flow:**
```
START (shape1)
  ↓
Set URL/SOAPAction (shape3)
  └─→ WRITES: dynamicdocument.URL, dynamicdocument.SOAPAction
  ↓
Build SOAP Request (shape5)
  └─→ Message: Authenticate(username, password)
  ↓
Operation: Login (shape2)
  └─→ SOAP API Call
  ↓
Decision (shape4): Status Code "20*"?
  ├─→ TRUE (Success):
  |    └─→ Extract SessionId (shape8)
  |         └─→ WRITES: process.DPP_SessionId
  |         └─→ Stop (continue=true) [SUCCESS RETURN]
  |
  └─→ FALSE (Failure):
       └─→ Set Error (shape11)
            └─→ WRITES: process.DPP_CafmError
            └─→ Map Error (shape6)
            └─→ Return Documents ("Login_Error") [ERROR RETURN]
```

**Return Paths:**
- **SUCCESS:** Stop (continue=true) - SessionId available in main process
- **ERROR:** Return Documents ("Login_Error") - Main process receives error label

**Properties Written:**
- process.DPP_SessionId (on success)
- process.DPP_CafmError (on failure)

**Properties Read from Main Process:**
- None (uses defined parameters: FSI_Username, FSI_Password)

---

### Subprocess 2: FsiLogout (b44c26cb-ecd5-4677-a752-434fe68f2e2b)

**Internal Flow:**
```
START (shape1)
  ↓
Build SOAP Request (shape5)
  └─→ Message: LogOut(sessionId)
  └─→ READS: process.DPP_SessionId
  ↓
Set URL/SOAPAction (shape4)
  └─→ WRITES: dynamicdocument.URL, dynamicdocument.SOAPAction
  ↓
Operation: Logout (shape2)
  └─→ SOAP API Call
  ↓
Stop (continue=true)
```

**Return Paths:**
- **SUCCESS:** Stop (continue=true) - Logout complete

**Properties Written:**
- None

**Properties Read from Main Process:**
- process.DPP_SessionId

---

### Subprocess 3: Office 365 Email (a85945c5-3004-42b9-80b1-104f465cd1fb)

**Internal Flow:**
```
START (shape1)
  ↓
Try/Catch (shape2)
  |
  ├─→ TRY PATH:
  |    └─→ Decision (shape4): DPP_HasAttachment equals "Y"?
  |         |
  |         ├─→ TRUE (with attachment):
  |         |    └─→ Build Email Body (shape11)
  |         |    └─→ Set Mail Body Property (shape14)
  |         |    └─→ Set Payload (shape15)
  |         |    └─→ Set Mail Properties (shape6)
  |         |         └─→ WRITES: connector.mail.fromAddress, connector.mail.toAddress, connector.mail.subject, connector.mail.body, connector.mail.filename
  |         |    └─→ Operation: Send Email with Attachment (shape3)
  |         |    └─→ Stop (continue=true)
  |         |
  |         └─→ FALSE (without attachment):
  |              └─→ Build Email Body (shape23)
  |              └─→ Set Mail Body Property (shape22)
  |              └─→ Set Mail Properties (shape20)
  |                   └─→ WRITES: connector.mail.fromAddress, connector.mail.toAddress, connector.mail.subject, connector.mail.body
  |              └─→ Operation: Send Email without Attachment (shape7)
  |              └─→ Stop (continue=true)
  |
  └─→ CATCH PATH:
       └─→ Exception (shape10)
            └─→ Throw error with Try/Catch message
```

**Return Paths:**
- **SUCCESS:** Stop (continue=true) - Email sent

**Properties Written:**
- None (uses connector properties)

**Properties Read from Main Process:**
- process.DPP_HasAttachment
- process.To_Email
- process.DPP_Subject
- process.DPP_MailBody
- process.DPP_File_Name (if attachment)
- process.DPP_Process_Name
- process.DPP_AtomName
- process.DPP_ExecutionID
- process.DPP_ErrorMessage

---

## 14. Critical Patterns Identified

### Pattern 1: Session-Based Authentication
- **Subprocess:** FsiLogin
- **Pattern:** Login → Extract SessionId → Use in all subsequent calls → Logout
- **Implementation:** Middleware with AuthenticateAtomicHandler + LogoutAtomicHandler + RequestContext

### Pattern 2: Check-Before-Create
- **Operation:** GetBreakdownTasksByDto → Decision → CreateBreakdownTask
- **Pattern:** Check if task exists → If exists, skip creation → If not exists, create
- **Implementation:** Handler orchestrates: GetBreakdownTasksByDto atomic handler → Check response → If empty, skip creation

### Pattern 3: Parallel Lookups with Sequential Aggregation
- **Operations:** GetLocationsByDto (Path 3) || GetInstructionSetsByDto (Path 4) → CreateBreakdownTask (Path 5)
- **Pattern:** Two independent lookups execute in parallel → Results aggregated → Create operation uses aggregated data
- **Implementation:** Handler orchestrates: GetLocationsByDto + GetInstructionSetsByDto (parallel) → CreateBreakdownTask (sequential)

### Pattern 4: Conditional Recurring Task Handling
- **Decision:** recurrence not equals "Y"?
- **Pattern:** If recurring → Create task + Create event/link → If not recurring → Create task only
- **Implementation:** Handler orchestrates: Check recurrence flag → CreateBreakdownTask → If recurring, CreateEvent

### Pattern 5: Error Notification via Email
- **Subprocess:** Office 365 Email
- **Pattern:** On error → Send email notification with error details
- **Implementation:** Separate email subprocess (NOT System Layer - Process Layer responsibility)

---

## 15. System Layer Identification

### Third-Party Systems Identified

**System 1: FSI CAFM (Facilities System International)**
- **Type:** SOAP/XML API
- **Connection ID:** 6ccb44cd-d2c6-4e29-8631-7710e22b239f
- **Operations:**
  1. Authenticate (Login)
  2. LogOut
  3. GetLocationsByDto
  4. GetInstructionSetsByDto
  5. CreateBreakdownTask
  6. CreateEvent
  7. GetBreakdownTasksByDto

**System 2: Office 365 Email (SMTP)**
- **Type:** SMTP/Email
- **Connection ID:** 00eae79b-2303-4215-8067-dcc299e42697
- **Operations:**
  1. Send Email with Attachment
  2. Send Email without Attachment

---

## 16. System Layer Classification (Function Exposure Decision)

### ✅ Decision Table Completed: YES

| Operation | Independent Invoke? | Decision Before/After? | Same SOR? | Internal Lookup? | Conclusion | Reasoning |
|---|---|---|---|---|---|---|
| **FSI CAFM Operations:** |
| Login | NO | AFTER (status check) | N/A | N/A | **Atomic (Middleware)** | Auth lifecycle - middleware handles |
| Logout | NO | None | N/A | N/A | **Atomic (Middleware)** | Auth lifecycle - middleware handles |
| GetLocationsByDto | NO | None | YES | YES - lookup | **Atomic (Internal)** | Lookup for CreateBreakdownTask |
| GetInstructionSetsByDto | NO | None | YES | YES - lookup | **Atomic (Internal)** | Lookup for CreateBreakdownTask |
| GetBreakdownTasksByDto | NO | BEFORE (check exists) | YES | YES - check | **Atomic (Internal)** | Check-before-create pattern |
| CreateBreakdownTask | YES | AFTER (status check) | YES | NO | **Azure Function** | Complete operation PL calls |
| CreateEvent | NO | None | YES | NO | **Atomic (Internal)** | Conditional (recurring only) |
| **Email Operations:** |
| Send Email (with/without attachment) | YES | None | N/A | N/A | **Azure Function** | Complete operation PL calls |

### Summary

**I will create 2 Azure Functions for FSI CAFM:**
1. **CreateWorkOrderAPI** - Main function that orchestrates:
   - Internal: GetBreakdownTasksByDto (check exists)
   - Internal: GetLocationsByDto (lookup)
   - Internal: GetInstructionSetsByDto (lookup)
   - Internal: CreateBreakdownTask (create)
   - Internal: CreateEvent (conditional - recurring only)
   - Middleware: Login/Logout (session management)

2. **SendEmailAPI** - Email notification function (separate SOR)

**Per Rule 1066:** Business decisions → Process Layer when operations span different SORs or involve complex orchestration. In this case:
- **Same SOR (FSI CAFM):** All CAFM operations orchestrated internally by CreateWorkOrderAPI Handler
- **Different SOR (Email):** Separate SendEmailAPI Function for email operations
- **Check-before-create:** Handler orchestrates internally (same SOR)
- **Parallel lookups:** Handler orchestrates internally (same SOR)
- **Conditional recurring:** Handler orchestrates internally (same SOR)

**Functions:**
- CreateWorkOrderAPI: Accepts work order request, orchestrates all CAFM operations, returns success/error response
- SendEmailAPI: Accepts email request, sends email with/without attachment, returns success/error response

**Internal Atomic Handlers:**
- AuthenticateAtomicHandler (for middleware)
- LogoutAtomicHandler (for middleware)
- GetLocationsByDtoAtomicHandler
- GetInstructionSetsByDtoAtomicHandler
- GetBreakdownTasksByDtoAtomicHandler
- CreateBreakdownTaskAtomicHandler
- CreateEventAtomicHandler

**Auth Method:** Session-based with middleware (CustomAuthenticationMiddleware)

---

## 17. Validation Checklist

### Data Dependencies
- [x] All property WRITES identified
- [x] All property READS identified
- [x] Dependency graph built
- [x] Execution order satisfies all dependencies (no read-before-write)

### Decision Analysis
- [x] ALL decision shapes inventoried (6 decisions)
- [x] BOTH TRUE and FALSE paths traced to termination
- [x] Pattern type identified for each decision
- [x] Early exits identified and documented (shape55 TRUE path, FsiLogin error return)
- [x] Convergence points identified (shape15 Return Documents)

### Branch Analysis
- [x] Each branch classified as parallel or sequential
- [x] Branch shape4: SEQUENTIAL (data dependencies + API calls)
- [x] Branch shape20: SEQUENTIAL (API calls)
- [x] Branch shape32: SEQUENTIAL (data dependencies + API calls)
- [x] Branch shape58: SEQUENTIAL (business logic routing)
- [x] Dependency order built using topological sort
- [x] Each path traced to terminal point
- [x] Convergence points identified
- [x] Execution continuation point determined

### Sequence Diagram
- [x] Format follows required structure
- [x] Each operation shows READS and WRITES
- [x] Decisions show both TRUE and FALSE paths
- [x] Check-before-create pattern shown correctly (GetBreakdownTasksByDto → Decision → Skip/Continue)
- [x] Early exits marked [EARLY EXIT]
- [x] Conditional execution marked (CreateEvent only if recurrence=Y)
- [x] Subprocess internal flows documented
- [x] Subprocess return paths mapped to main process

### Subprocess Analysis
- [x] ALL subprocesses analyzed (FsiLogin, FsiLogout, Office 365 Email)
- [x] Return paths identified (success and error)
- [x] Return path labels mapped to main process shapes
- [x] Properties written by subprocess documented
- [x] Properties read by subprocess from main process documented

### Input/Output Structure Analysis
- [x] Entry point operation identified (de68dad0-be76-4ec8-9857-4e5cf2a7bd4c)
- [x] Request profile identified (af096014-313f-4565-9091-2bdd56eb46df)
- [x] Request profile structure analyzed (JSON)
- [x] Array vs single object detected (Array: workOrder)
- [x] Array cardinality documented (minOccurs: 0, maxOccurs: -1)
- [x] ALL request fields extracted (19 fields including nested)
- [x] Request field paths documented
- [x] Request field mapping table generated
- [x] Response profile identified
- [x] Response structure analyzed
- [x] Document processing behavior determined (array splitting)

---

## 18. System Layer Implementation Summary

### Repository Naming Convention
**Target Repository:** `sys-fsi-cafm-mgmt` (System Layer for FSI CAFM)

### Azure Functions to Create
1. **CreateWorkOrderAPI** - Creates work order in CAFM system
2. **SendEmailAPI** - Sends email notifications (separate SOR)

### Interfaces to Create
1. **IWorkOrderMgmt** - Work order management interface
2. **IEmailMgmt** - Email management interface

### Services to Create
1. **WorkOrderMgmtService** - Implements IWorkOrderMgmt
2. **EmailMgmtService** - Implements IEmailMgmt

### Handlers to Create
1. **CreateWorkOrderHandler** - Orchestrates work order creation
2. **SendEmailHandler** - Handles email sending

### Atomic Handlers to Create
1. **AuthenticateAtomicHandler** - SOAP Login
2. **LogoutAtomicHandler** - SOAP Logout
3. **GetLocationsByDtoAtomicHandler** - SOAP GetLocationsByDto
4. **GetInstructionSetsByDtoAtomicHandler** - SOAP GetInstructionSetsByDto
5. **GetBreakdownTasksByDtoAtomicHandler** - SOAP GetBreakdownTasksByDto
6. **CreateBreakdownTaskAtomicHandler** - SOAP CreateBreakdownTask
7. **CreateEventAtomicHandler** - SOAP CreateEvent
8. **SendEmailWithAttachmentAtomicHandler** - SMTP Send with attachment
9. **SendEmailWithoutAttachmentAtomicHandler** - SMTP Send without attachment

### Middleware Components
1. **CustomAuthenticationAttribute** - Marks functions requiring auth
2. **CustomAuthenticationMiddleware** - Handles login/logout lifecycle
3. **RequestContext** - AsyncLocal storage for SessionId

### DTOs to Create
**HandlerDTOs:**
- CreateWorkOrderReqDTO, CreateWorkOrderResDTO
- SendEmailReqDTO, SendEmailResDTO

**AtomicHandlerDTOs:**
- AuthenticateHandlerReqDTO
- LogoutHandlerReqDTO
- GetLocationsByDtoHandlerReqDTO
- GetInstructionSetsByDtoHandlerReqDTO
- GetBreakdownTasksByDtoHandlerReqDTO
- CreateBreakdownTaskHandlerReqDTO
- CreateEventHandlerReqDTO
- SendEmailHandlerReqDTO

**DownstreamDTOs (ApiResDTO):**
- AuthenticateApiResDTO
- GetLocationsByDtoApiResDTO
- GetInstructionSetsByDtoApiResDTO
- GetBreakdownTasksByDtoApiResDTO
- CreateBreakdownTaskApiResDTO
- CreateEventApiResDTO

### Helpers to Create
1. **CustomSoapClient.cs** - SOAP HTTP client wrapper
2. **SOAPHelper.cs** - SOAP envelope utilities
3. **CustomSmtpClient.cs** - SMTP client wrapper (for email)

### SOAP Envelopes to Create
1. **Authenticate.xml**
2. **Logout.xml**
3. **GetLocationsByDto.xml**
4. **GetInstructionSetsByDto.xml**
5. **GetBreakdownTasksByDto.xml**
6. **CreateBreakdownTask.xml**
7. **CreateEvent.xml**

### Configuration
**AppConfigs properties:**
- BaseUrl (FSI CAFM base URL)
- LoginResourcePath
- LogoutResourcePath
- GetLocationsByDtoResourcePath
- GetInstructionSetsByDtoResourcePath
- GetBreakdownTasksByDtoResourcePath
- CreateBreakdownTaskResourcePath
- CreateEventResourcePath
- LoginSoapAction
- LogoutSoapAction
- GetLocationsByDtoSoapAction
- GetInstructionSetsByDtoSoapAction
- GetBreakdownTasksByDtoSoapAction
- CreateBreakdownTaskSoapAction
- CreateEventSoapAction
- FsiUsername (from KeyVault)
- FsiPassword (from KeyVault)
- SmtpHost
- SmtpPort
- SmtpUsername (from KeyVault)
- SmtpPassword (from KeyVault)
- SmtpFromEmail
- TimeoutSeconds (default: 50)
- RetryCount (default: 0)

---

**END OF PHASE 1 ANALYSIS**
