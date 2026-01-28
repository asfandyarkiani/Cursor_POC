# PROCESS LAYER ↔ SYSTEM LAYER ORCHESTRATION DIAGRAM

**Process:** Create Work Order from EQ+ to CAFM  
**Based on:** BOOMI_EXTRACTION_PHASE1.md  
**Date:** 2026-01-28

---

## OVERVIEW

This diagram shows how Process Layer orchestrates System Layer APIs to implement the complete business workflow extracted from the Boomi process.

**Key Principle:** System Layer provides atomic "Lego block" APIs. Process Layer orchestrates them based on business logic.

---

## COMPLETE ORCHESTRATION FLOW

```
┌─────────────────────────────────────────────────────────────────────────────────┐
│                              PROCESS LAYER                                       │
│                     (Business Orchestration & Decision Logic)                    │
└─────────────────────────────────────────────────────────────────────────────────┘
                                      │
                                      │ HTTP POST
                                      │ { workOrder: [...] }
                                      ↓
┌─────────────────────────────────────────────────────────────────────────────────┐
│                         PROCESS LAYER FUNCTION                                   │
│                    CreateWorkOrderFromEQPlusToCAFM                              │
└─────────────────────────────────────────────────────────────────────────────────┘
                                      │
                                      │ Receives array of work orders
                                      │ Loops through each work order
                                      ↓
                    ┌─────────────────────────────────┐
                    │   FOR EACH work order in array  │
                    └─────────────────────────────────┘
                                      │
                                      ↓
        ╔═════════════════════════════════════════════════════════════╗
        ║              BUSINESS DECISION 1: Check Existence            ║
        ║          "Should we create or skip this work order?"         ║
        ╚═════════════════════════════════════════════════════════════╝
                                      │
                                      │ Call System Layer API
                                      ↓
        ┌─────────────────────────────────────────────────────────────┐
        │                    SYSTEM LAYER API #1                       │
        │         POST /cafm/breakdown-tasks/check                    │
        │                                                              │
        │  Request: { serviceRequestNumber: "EQ-2025-001234" }       │
        │                                                              │
        │  [CustomAuthentication] Middleware:                         │
        │    1. Login to CAFM → Get SessionId                        │
        │    2. Execute function                                      │
        │    3. Logout from CAFM (finally)                           │
        │                                                              │
        │  Handler: GetBreakdownTasksByDtoHandler                    │
        │    → GetBreakdownTasksByDtoAtomicHandler                   │
        │       → SOAP: GetBreakdownTasksByDto                       │
        │          (sessionId, callId)                               │
        │                                                              │
        │  Response: {                                                │
        │    taskExists: true/false,                                 │
        │    existingTaskId: "CAFM-2025-XXX" or ""                  │
        │  }                                                          │
        └─────────────────────────────────────────────────────────────┘
                                      │
                                      │ Returns to Process Layer
                                      ↓
        ╔═════════════════════════════════════════════════════════════╗
        ║         PROCESS LAYER DECISION: Evaluate Response           ║
        ╚═════════════════════════════════════════════════════════════╝
                                      │
                    ┌─────────────────┴─────────────────┐
                    │                                   │
            IF taskExists = TRUE              IF taskExists = FALSE
                    │                                   │
                    ↓                                   ↓
        ┌───────────────────────┐         ┌───────────────────────┐
        │  EARLY EXIT           │         │  CONTINUE TO CREATE   │
        │  Return existing task │         │                       │
        │  Skip creation        │         │  Proceed to Step 2    │
        └───────────────────────┘         └───────────────────────┘
                                                      │
                                                      ↓
        ╔═════════════════════════════════════════════════════════════╗
        ║           BUSINESS DECISION 2: Create Work Order            ║
        ║       "Task doesn't exist, proceed with creation"           ║
        ╚═════════════════════════════════════════════════════════════╝
                                      │
                                      │ Call System Layer API
                                      ↓
        ┌─────────────────────────────────────────────────────────────┐
        │                    SYSTEM LAYER API #2                       │
        │       POST /cafm/breakdown-tasks/create                     │
        │                                                              │
        │  Request: {                                                 │
        │    reporterName: "John Doe",                               │
        │    reporterEmail: "john.doe@example.com",                  │
        │    description: "AC not working",                          │
        │    serviceRequestNumber: "EQ-2025-001234",                 │
        │    propertyName: "Al Ghurair Tower",                       │
        │    unitCode: "UNIT-301",                                   │
        │    categoryName: "HVAC",                                   │
        │    subCategory: "Air Conditioning",                        │
        │    sourceOrgId: "ORG-001",                                 │
        │    priority: "High",                                       │
        │    scheduledDate: "2025-02-25",                            │
        │    scheduledTimeStart: "11:05:41",                         │
        │    raisedDateUtc: "2025-02-24T10:30:00Z",                 │
        │    recurrence: "N"                                         │
        │  }                                                          │
        │                                                              │
        │  [CustomAuthentication] Middleware:                         │
        │    1. Login to CAFM → Get SessionId                        │
        │    2. Execute function                                      │
        │    3. Logout from CAFM (finally)                           │
        │                                                              │
        │  Handler: CreateBreakdownTaskHandler                       │
        │    │                                                         │
        │    ├─→ STEP 1: GetLocationsByDto (BEST-EFFORT)            │
        │    │   → GetLocationsByDtoAtomicHandler                    │
        │    │      → SOAP: GetLocationsByDto                        │
        │    │         (sessionId, propertyName, unitCode)           │
        │    │   → If SUCCESS: Extract LocationId, BuildingId        │
        │    │   → If FAIL: Log warning, set empty, CONTINUE         │
        │    │                                                         │
        │    ├─→ STEP 2: GetInstructionSetsByDto (BEST-EFFORT)      │
        │    │   → GetInstructionSetsByDtoAtomicHandler              │
        │    │      → SOAP: GetInstructionSetsByDto                  │
        │    │         (sessionId, categoryName, subCategory)        │
        │    │   → If SUCCESS: Extract InstructionId                 │
        │    │   → If FAIL: Log warning, set empty, CONTINUE         │
        │    │                                                         │
        │    ├─→ STEP 3: Format Dates                                │
        │    │   → Combine scheduledDate + scheduledTimeStart        │
        │    │   → Format to ISO with .0208713Z suffix               │
        │    │                                                         │
        │    └─→ STEP 4: CreateBreakdownTask (MAIN OPERATION)        │
        │        → CreateBreakdownTaskAtomicHandler                   │
        │           → SOAP: CreateBreakdownTask                       │
        │              (sessionId, all fields, lookup IDs)            │
        │        → If SUCCESS: Extract TaskId                         │
        │        → If FAIL: Throw exception (STOP)                    │
        │                                                              │
        │  Response: {                                                │
        │    taskId: "CAFM-2025-12345",                              │
        │    status: "Created",                                       │
        │    serviceRequestNumber: "EQ-2025-001234",                 │
        │    sourceOrgId: "ORG-001"                                  │
        │  }                                                          │
        └─────────────────────────────────────────────────────────────┘
                                      │
                                      │ Returns to Process Layer
                                      ↓
        ╔═════════════════════════════════════════════════════════════╗
        ║      BUSINESS DECISION 3: Should Link Recurring Event?      ║
        ║              "Check recurrence flag from input"             ║
        ╚═════════════════════════════════════════════════════════════╝
                                      │
                    ┌─────────────────┴─────────────────┐
                    │                                   │
        IF recurrence = "Y"                  IF recurrence ≠ "Y"
                    │                                   │
                    ↓                                   ↓
        ┌───────────────────────┐         ┌───────────────────────┐
        │  LINK RECURRING EVENT │         │  SKIP EVENT LINKING   │
        │  Call System Layer #3 │         │  Return success       │
        └───────────────────────┘         └───────────────────────┘
                    │
                    │ Call System Layer API
                    ↓
        ┌─────────────────────────────────────────────────────────────┐
        │                    SYSTEM LAYER API #3                       │
        │            POST /cafm/events/create                         │
        │                                                              │
        │  Request: {                                                 │
        │    taskId: "CAFM-2025-12345",                              │
        │    eventType: "Recurring"                                  │
        │  }                                                          │
        │                                                              │
        │  [CustomAuthentication] Middleware:                         │
        │    1. Login to CAFM → Get SessionId                        │
        │    2. Execute function                                      │
        │    3. Logout from CAFM (finally)                           │
        │                                                              │
        │  Handler: CreateEventHandler                               │
        │    → CreateEventAtomicHandler                              │
        │       → SOAP: CreateEvent                                  │
        │          (sessionId, taskId, eventType)                    │
        │                                                              │
        │  Response: {                                                │
        │    eventId: "EVENT-2025-98765",                            │
        │    status: "Linked"                                        │
        │  }                                                          │
        └─────────────────────────────────────────────────────────────┘
                                      │
                                      │ Returns to Process Layer
                                      ↓
        ┌─────────────────────────────────────────────────────────────┐
        │              PROCESS LAYER: Aggregate Results                │
        │                                                              │
        │  Combine responses from all System Layer calls:             │
        │    - Task existence check result                            │
        │    - Created task ID                                        │
        │    - Event ID (if created)                                  │
        │                                                              │
        │  Build final response for Experience Layer/Client          │
        └─────────────────────────────────────────────────────────────┘
                                      │
                                      ↓
                              RETURN TO CLIENT
```

---

## DETAILED LAYER RESPONSIBILITIES

### PROCESS LAYER RESPONSIBILITIES

**Business Orchestration:**
1. ✅ **Loop through work order array** (Boomi: inputType="singlejson" with array)
2. ✅ **Decision: Check existence** (Call GetBreakdownTasksByDto → Evaluate result → Skip or proceed)
3. ✅ **Decision: Link recurring event** (Check recurrence flag → Call CreateEvent if "Y")
4. ✅ **Aggregate results** (Combine responses from multiple System Layer calls)
5. ✅ **Error handling** (Catch System Layer exceptions, send email notifications)
6. ✅ **Response mapping** (Map System Layer responses to Experience Layer format)

**What Process Layer DOES:**
- Receives array of work orders from Experience Layer
- Loops through each work order (batch processing)
- Calls System Layer APIs in sequence based on business logic
- Makes cross-operation decisions (if exists skip, if recurrence link)
- Aggregates results from multiple System Layer calls
- Handles errors and sends notifications

**What Process Layer DOES NOT DO:**
- Does NOT call CAFM SOAP APIs directly (System Layer does this)
- Does NOT handle CAFM authentication (System Layer middleware does this)
- Does NOT build SOAP envelopes (System Layer does this)
- Does NOT manage SessionId lifecycle (System Layer middleware does this)

### SYSTEM LAYER RESPONSIBILITIES

**Atomic Operations:**
1. ✅ **GetBreakdownTasksByDto API** - Check if task exists (returns taskExists boolean)
2. ✅ **CreateBreakdownTask API** - Create work order with internal lookups (best-effort pattern)
3. ✅ **CreateEvent API** - Link recurring event to task

**What System Layer DOES:**
- Provides 3 independent Azure Functions (callable by Process Layer)
- Handles CAFM session-based authentication (middleware: login → execute → logout)
- Builds SOAP envelopes from templates
- Executes SOAP operations (GetBreakdownTasksByDto, GetLocationsByDto, GetInstructionSetsByDto, CreateBreakdownTask, CreateEvent)
- Performs best-effort lookups internally (GetLocationsByDto, GetInstructionSetsByDto)
- Deserializes SOAP responses to JSON DTOs
- Returns standardized BaseResponseDTO to Process Layer

**What System Layer DOES NOT DO:**
- Does NOT decide when to call which operation (Process Layer decides)
- Does NOT loop through work order arrays (Process Layer does this)
- Does NOT make cross-operation decisions (if exists skip, if recurrence link)
- Does NOT aggregate results from multiple operations (Process Layer does this)
- Does NOT send email notifications (Process Layer does this)

---

## OPERATION-LEVEL ORCHESTRATION

### Operation 1: Check Task Existence

```
┌─────────────────────────────────────────────────────────────────┐
│                        PROCESS LAYER                             │
│                                                                  │
│  Function: CreateWorkOrderFromEQPlusToCAFM                      │
│  Step 1: Check if task exists                                   │
│                                                                  │
│  foreach (workOrder in request.WorkOrders) {                   │
│    // Call System Layer to check existence                      │
│    var checkRequest = new {                                     │
│      serviceRequestNumber = workOrder.ServiceRequestNumber     │
│    };                                                            │
│  }                                                               │
└─────────────────────────────────────────────────────────────────┘
                            │
                            │ HTTP POST
                            │ /cafm/breakdown-tasks/check
                            ↓
┌─────────────────────────────────────────────────────────────────┐
│                       SYSTEM LAYER API                           │
│              GetBreakdownTasksByDtoAPI                          │
│                                                                  │
│  [CustomAuthentication] ← Middleware intercepts                 │
│    ├─→ Login to CAFM (AuthenticateAtomicHandler)               │
│    │   → SOAP: Authenticate(username, password)                 │
│    │   → Extract SessionId                                       │
│    │   → Store in RequestContext                                 │
│    │                                                              │
│    ├─→ Execute Function                                          │
│    │   → Service: IWorkOrderMgmt.GetBreakdownTasksByDto()       │
│    │      → Handler: GetBreakdownTasksByDtoHandler              │
│    │         → AtomicHandler: GetBreakdownTasksByDtoAtomicHandler│
│    │            → SOAP: GetBreakdownTasksByDto                   │
│    │               (sessionId, callId)                           │
│    │            → Deserialize SOAP response                      │
│    │            → Return task list                               │
│    │                                                              │
│    └─→ Logout from CAFM (finally)                               │
│        → LogoutAtomicHandler                                     │
│           → SOAP: LogOut(sessionId)                             │
│                                                                  │
│  Response: {                                                     │
│    message: "Successfully retrieved...",                        │
│    data: {                                                       │
│      tasks: [...],                                              │
│      taskExists: true/false,                                    │
│      existingTaskId: "CAFM-2025-XXX"                           │
│    }                                                             │
│  }                                                               │
└─────────────────────────────────────────────────────────────────┘
                            │
                            │ Returns JSON
                            ↓
┌─────────────────────────────────────────────────────────────────┐
│                        PROCESS LAYER                             │
│                                                                  │
│  Evaluate response:                                             │
│    if (response.data.taskExists) {                              │
│      // Task exists - skip creation                             │
│      results.Add(new {                                          │
│        cafmSRNumber = response.data.existingTaskId,            │
│        sourceSRNumber = workOrder.ServiceRequestNumber,        │
│        status = "true",                                         │
│        message = "Task already exists"                          │
│      });                                                         │
│      continue; // Skip to next work order                       │
│    }                                                             │
│    else {                                                        │
│      // Task doesn't exist - proceed to creation                │
│      // Continue to Operation 2                                 │
│    }                                                             │
└─────────────────────────────────────────────────────────────────┘
```

### Operation 2: Create Work Order

```
┌─────────────────────────────────────────────────────────────────┐
│                        PROCESS LAYER                             │
│                                                                  │
│  Function: CreateWorkOrderFromEQPlusToCAFM                      │
│  Step 2: Create work order (task doesn't exist)                 │
│                                                                  │
│  var createRequest = new {                                      │
│    reporterName = workOrder.ReporterName,                       │
│    reporterEmail = workOrder.ReporterEmail,                     │
│    description = workOrder.Description,                         │
│    serviceRequestNumber = workOrder.ServiceRequestNumber,       │
│    propertyName = workOrder.PropertyName,                       │
│    unitCode = workOrder.UnitCode,                               │
│    categoryName = workOrder.CategoryName,                       │
│    subCategory = workOrder.SubCategory,                         │
│    sourceOrgId = workOrder.SourceOrgId,                         │
│    priority = workOrder.TicketDetails.Priority,                 │
│    scheduledDate = workOrder.TicketDetails.ScheduledDate,       │
│    scheduledTimeStart = workOrder.TicketDetails.ScheduledTimeStart,│
│    raisedDateUtc = workOrder.TicketDetails.RaisedDateUtc,      │
│    recurrence = workOrder.TicketDetails.Recurrence              │
│  };                                                              │
└─────────────────────────────────────────────────────────────────┘
                            │
                            │ HTTP POST
                            │ /cafm/breakdown-tasks/create
                            ↓
┌─────────────────────────────────────────────────────────────────┐
│                       SYSTEM LAYER API                           │
│              CreateBreakdownTaskAPI                             │
│                                                                  │
│  [CustomAuthentication] ← Middleware intercepts                 │
│    ├─→ Login to CAFM (AuthenticateAtomicHandler)               │
│    │   → SOAP: Authenticate(username, password)                 │
│    │   → Extract SessionId                                       │
│    │   → Store in RequestContext                                 │
│    │                                                              │
│    ├─→ Execute Function                                          │
│    │   → Service: IWorkOrderMgmt.CreateBreakdownTask()          │
│    │      → Handler: CreateBreakdownTaskHandler                 │
│    │         │                                                    │
│    │         ├─→ INTERNAL STEP 1: GetLocationsByDto             │
│    │         │   (BEST-EFFORT LOOKUP)                            │
│    │         │   → GetLocationsByDtoAtomicHandler                │
│    │         │      → SOAP: GetLocationsByDto                    │
│    │         │         (sessionId, propertyName, unitCode)       │
│    │         │   → If SUCCESS: locationId, buildingId            │
│    │         │   → If FAIL: Log warning, empty values, CONTINUE  │
│    │         │                                                    │
│    │         ├─→ INTERNAL STEP 2: GetInstructionSetsByDto       │
│    │         │   (BEST-EFFORT LOOKUP)                            │
│    │         │   → GetInstructionSetsByDtoAtomicHandler          │
│    │         │      → SOAP: GetInstructionSetsByDto              │
│    │         │         (sessionId, categoryName, subCategory)    │
│    │         │   → If SUCCESS: instructionId                     │
│    │         │   → If FAIL: Log warning, empty value, CONTINUE   │
│    │         │                                                    │
│    │         ├─→ INTERNAL STEP 3: Format Dates                   │
│    │         │   → Combine scheduledDate + scheduledTimeStart    │
│    │         │   → Format to ISO: .0208713Z suffix               │
│    │         │                                                    │
│    │         └─→ INTERNAL STEP 4: CreateBreakdownTask            │
│    │             (MAIN OPERATION)                                 │
│    │             → CreateBreakdownTaskAtomicHandler              │
│    │                → SOAP: CreateBreakdownTask                  │
│    │                   (sessionId, all fields, lookup IDs)       │
│    │                → Uses: locationId, buildingId, instructionId│
│    │                   (may be empty if lookups failed)          │
│    │             → If SUCCESS: Extract TaskId                    │
│    │             → If FAIL: Throw exception                      │
│    │                                                              │
│    └─→ Logout from CAFM (finally)                               │
│        → LogoutAtomicHandler                                     │
│           → SOAP: LogOut(sessionId)                             │
│           → If FAIL: Log error, CONTINUE                         │
│                                                                  │
│  Response: {                                                     │
│    message: "Breakdown task created successfully...",           │
│    data: {                                                       │
│      taskId: "CAFM-2025-12345",                                │
│      status: "Created",                                         │
│      serviceRequestNumber: "EQ-2025-001234",                   │
│      sourceOrgId: "ORG-001"                                    │
│    }                                                             │
│  }                                                               │
└─────────────────────────────────────────────────────────────────┘
                            │
                            │ Returns JSON
                            ↓
┌─────────────────────────────────────────────────────────────────┐
│                        PROCESS LAYER                             │
│                                                                  │
│  Store created task ID:                                         │
│    createdTaskId = response.data.taskId;                        │
│                                                                  │
│  Check recurrence flag:                                         │
│    if (workOrder.TicketDetails.Recurrence == "Y") {            │
│      // Proceed to Operation 3 (CreateEvent)                    │
│    }                                                             │
│    else {                                                        │
│      // Skip event linking                                      │
│      // Add to results and continue                             │
│    }                                                             │
└─────────────────────────────────────────────────────────────────┘
```

### Operation 3: Link Recurring Event (Conditional)

```
┌─────────────────────────────────────────────────────────────────┐
│                        PROCESS LAYER                             │
│                                                                  │
│  Function: CreateWorkOrderFromEQPlusToCAFM                      │
│  Step 3: Link recurring event (only if recurrence = "Y")        │
│                                                                  │
│  if (workOrder.TicketDetails.Recurrence == "Y") {              │
│    var eventRequest = new {                                     │
│      taskId = createdTaskId,                                    │
│      eventType = "Recurring"                                    │
│    };                                                            │
│  }                                                               │
└─────────────────────────────────────────────────────────────────┘
                            │
                            │ HTTP POST
                            │ /cafm/events/create
                            ↓
┌─────────────────────────────────────────────────────────────────┐
│                       SYSTEM LAYER API                           │
│                   CreateEventAPI                                │
│                                                                  │
│  [CustomAuthentication] ← Middleware intercepts                 │
│    ├─→ Login to CAFM                                            │
│    ├─→ Execute Function                                          │
│    │   → Service: IWorkOrderMgmt.CreateEvent()                  │
│    │      → Handler: CreateEventHandler                         │
│    │         → AtomicHandler: CreateEventAtomicHandler          │
│    │            → SOAP: CreateEvent                             │
│    │               (sessionId, taskId, eventType)               │
│    └─→ Logout from CAFM (finally)                               │
│                                                                  │
│  Response: {                                                     │
│    message: "Recurring event created...",                       │
│    data: {                                                       │
│      eventId: "EVENT-2025-98765",                              │
│      status: "Linked",                                          │
│      taskId: "CAFM-2025-12345"                                 │
│    }                                                             │
│  }                                                               │
└─────────────────────────────────────────────────────────────────┘
                            │
                            │ Returns JSON
                            ↓
┌─────────────────────────────────────────────────────────────────┐
│                        PROCESS LAYER                             │
│                                                                  │
│  Add to results:                                                │
│    results.Add(new {                                            │
│      cafmSRNumber = createdTaskId,                             │
│      sourceSRNumber = workOrder.ServiceRequestNumber,          │
│      sourceOrgId = workOrder.SourceOrgId,                      │
│      status = "true",                                           │
│      message = "Work order created with recurring event"        │
│    });                                                           │
│                                                                  │
│  Continue to next work order in loop                            │
└─────────────────────────────────────────────────────────────────┘
```

---

## SYSTEM LAYER INTERNAL ORCHESTRATION

### CreateBreakdownTask Handler Internal Flow

**Key Pattern:** System Layer Handler orchestrates SAME-SOR operations internally (all operations are CAFM)

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                    SYSTEM LAYER HANDLER                                      │
│              CreateBreakdownTaskHandler.HandleAsync()                       │
└─────────────────────────────────────────────────────────────────────────────┘
                                    │
                                    │ SessionId from RequestContext
                                    ↓
        ┌───────────────────────────────────────────────────────┐
        │  STEP 1: Get Locations (BEST-EFFORT)                  │
        │                                                        │
        │  if (!string.IsNullOrWhiteSpace(propertyName)) {      │
        │    response = GetLocationsByDto(sessionId, ...);      │
        │                                                        │
        │    if (!response.IsSuccessStatusCode) {               │
        │      _logger.Warn("Lookup failed - Continue");        │
        │      locationId = string.Empty;                       │
        │      buildingId = string.Empty;                       │
        │    }                                                   │
        │    else {                                              │
        │      // Extract IDs                                    │
        │      locationId = apiResponse.LocationId;             │
        │      buildingId = apiResponse.BuildingId;             │
        │    }                                                   │
        │  }                                                     │
        └───────────────────────────────────────────────────────┘
                                    │
                                    ↓
        ┌───────────────────────────────────────────────────────┐
        │  STEP 2: Get Instructions (BEST-EFFORT)               │
        │                                                        │
        │  if (!string.IsNullOrWhiteSpace(categoryName)) {      │
        │    response = GetInstructionSetsByDto(sessionId, ...);│
        │                                                        │
        │    if (!response.IsSuccessStatusCode) {               │
        │      _logger.Warn("Lookup failed - Continue");        │
        │      instructionId = string.Empty;                    │
        │    }                                                   │
        │    else {                                              │
        │      // Extract ID                                     │
        │      instructionId = apiResponse.InstructionId;       │
        │    }                                                   │
        │  }                                                     │
        └───────────────────────────────────────────────────────┘
                                    │
                                    ↓
        ┌───────────────────────────────────────────────────────┐
        │  STEP 3: Format Dates                                 │
        │                                                        │
        │  scheduledDateUtc = FormatScheduledDate(              │
        │    request.ScheduledDate,                             │
        │    request.ScheduledTimeStart                         │
        │  );                                                    │
        │  // Combines: "2025-02-25" + "T" + "11:05:41" + "Z"  │
        │  // Formats: "2025-02-25T11:05:41.0208713Z"          │
        │                                                        │
        │  raisedDateUtc = FormatRaisedDate(                    │
        │    request.RaisedDateUtc                              │
        │  );                                                    │
        │  // Formats: "2025-02-24T10:30:00.0208713Z"          │
        └───────────────────────────────────────────────────────┘
                                    │
                                    ↓
        ┌───────────────────────────────────────────────────────┐
        │  STEP 4: Create Breakdown Task (MAIN OPERATION)       │
        │                                                        │
        │  response = CreateBreakdownTask(                       │
        │    sessionId,                                          │
        │    request,                                            │
        │    locationId,      // May be empty                   │
        │    buildingId,      // May be empty                   │
        │    instructionId,   // May be empty                   │
        │    scheduledDateUtc,                                   │
        │    raisedDateUtc                                       │
        │  );                                                    │
        │                                                        │
        │  if (!response.IsSuccessStatusCode) {                 │
        │    // THROW EXCEPTION - Main operation must succeed   │
        │    throw DownStreamApiFailureException(...);          │
        │  }                                                     │
        │  else {                                                │
        │    // Extract TaskId                                   │
        │    taskId = apiResponse.TaskId;                       │
        │    return BaseResponseDTO with taskId;                │
        │  }                                                     │
        └───────────────────────────────────────────────────────┘
                                    │
                                    │ Returns to Function
                                    ↓
                            Return to Process Layer
```

**Key Points:**
- ✅ **Same SOR:** All operations (GetLocationsByDto, GetInstructionSetsByDto, CreateBreakdownTask) are CAFM
- ✅ **Simple orchestration:** Sequential calls with simple if/else (best-effort pattern)
- ✅ **No cross-SOR:** Handler does NOT call other System Layers
- ✅ **Internal lookups:** GetLocationsByDto and GetInstructionSetsByDto are internal (NOT exposed as Functions)
- ✅ **Best-effort:** Lookups may fail, but creation continues (CAFM validates required fields)

---

## DATA FLOW DIAGRAM

```
┌──────────────────────────────────────────────────────────────────────────────┐
│                           EXPERIENCE LAYER                                    │
│                        (Mobile App, Web Portal)                               │
└──────────────────────────────────────────────────────────────────────────────┘
                                      │
                                      │ HTTP POST
                                      │ Array of work orders
                                      ↓
┌──────────────────────────────────────────────────────────────────────────────┐
│                            PROCESS LAYER                                      │
│                   CreateWorkOrderFromEQPlusToCAFM                            │
│                                                                               │
│  Input: { workOrder: [                                                       │
│    {                                                                          │
│      serviceRequestNumber: "EQ-2025-001234",                                │
│      reporterName: "John Doe",                                              │
│      description: "AC not working",                                         │
│      propertyName: "Al Ghurair Tower",                                      │
│      unitCode: "UNIT-301",                                                  │
│      categoryName: "HVAC",                                                  │
│      subCategory: "Air Conditioning",                                       │
│      ticketDetails: {                                                        │
│        priority: "High",                                                    │
│        scheduledDate: "2025-02-25",                                         │
│        scheduledTimeStart: "11:05:41",                                      │
│        raisedDateUtc: "2025-02-24T10:30:00Z",                              │
│        recurrence: "Y"                                                      │
│      }                                                                       │
│    }                                                                          │
│  ]}                                                                           │
│                                                                               │
│  ┌─────────────────────────────────────────────────────────────┐            │
│  │  LOOP: foreach (workOrder in request.WorkOrders)            │            │
│  └─────────────────────────────────────────────────────────────┘            │
│                                                                               │
│  ┌─────────────────────────────────────────────────────────────┐            │
│  │  DECISION 1: Check Existence                                │            │
│  │  ↓                                                           │            │
│  │  Call System Layer: GetBreakdownTasksByDto ────────┐       │            │
│  │  ↓                                                  │        │            │
│  │  if (taskExists) {                                 │        │            │
│  │    Return existing task [EARLY EXIT]              │        │            │
│  │  }                                                  │        │            │
│  │  else {                                             │        │            │
│  │    Continue to DECISION 2                          │        │            │
│  │  }                                                  │        │            │
│  └─────────────────────────────────────────────────────────────┘            │
│                                      │                                        │
│  ┌─────────────────────────────────────────────────────────────┐            │
│  │  DECISION 2: Create Work Order                              │            │
│  │  ↓                                                           │            │
│  │  Call System Layer: CreateBreakdownTask ───────────┐       │            │
│  │    (Internal: GetLocationsByDto, GetInstructionSetsByDto)  │            │
│  │  ↓                                                  │        │            │
│  │  Store createdTaskId                               │        │            │
│  └─────────────────────────────────────────────────────────────┘            │
│                                      │                                        │
│  ┌─────────────────────────────────────────────────────────────┐            │
│  │  DECISION 3: Link Recurring Event                           │            │
│  │  ↓                                                           │            │
│  │  if (recurrence == "Y") {                          │        │            │
│  │    Call System Layer: CreateEvent ─────────────────┐       │            │
│  │  }                                                  │        │            │
│  │  else {                                             │        │            │
│  │    Skip event linking                              │        │            │
│  │  }                                                  │        │            │
│  └─────────────────────────────────────────────────────────────┘            │
│                                                                               │
│  Output: { workOrder: [                                                       │
│    {                                                                          │
│      cafmSRNumber: "CAFM-2025-12345",                                        │
│      sourceSRNumber: "EQ-2025-001234",                                       │
│      sourceOrgId: "ORG-001",                                                 │
│      status: "true",                                                         │
│      message: "Work order created successfully"                              │
│    }                                                                          │
│  ]}                                                                           │
└──────────────────────────────────────────────────────────────────────────────┘
                                      │
                                      │ HTTP Response
                                      ↓
┌──────────────────────────────────────────────────────────────────────────────┐
│                           EXPERIENCE LAYER                                    │
│                        (Mobile App, Web Portal)                               │
│                                                                               │
│  Display results to user:                                                    │
│    "Work order CAFM-2025-12345 created successfully"                         │
└──────────────────────────────────────────────────────────────────────────────┘
```

---

## AUTHENTICATION FLOW (SESSION-BASED)

```
┌─────────────────────────────────────────────────────────────────┐
│                        PROCESS LAYER                             │
│                                                                  │
│  Calls System Layer API (any of the 3 functions)               │
└─────────────────────────────────────────────────────────────────┘
                            │
                            │ HTTP POST
                            ↓
┌─────────────────────────────────────────────────────────────────┐
│                       SYSTEM LAYER                               │
│              CustomAuthenticationMiddleware                     │
│                                                                  │
│  ┌──────────────────────────────────────────────────┐          │
│  │  1. BEFORE Function Execution                     │          │
│  │     (Middleware Invoke - Before next())           │          │
│  └──────────────────────────────────────────────────┘          │
│                            │                                     │
│                            ↓                                     │
│  ┌──────────────────────────────────────────────────┐          │
│  │  Check if function has [CustomAuthentication]    │          │
│  │  attribute using reflection                       │          │
│  └──────────────────────────────────────────────────┘          │
│                            │                                     │
│                            ↓                                     │
│  ┌──────────────────────────────────────────────────┐          │
│  │  Retrieve FSI credentials from KeyVault          │          │
│  │    - FSI_Username                                 │          │
│  │    - FSI_Password                                 │          │
│  └──────────────────────────────────────────────────┘          │
│                            │                                     │
│                            ↓                                     │
│  ┌──────────────────────────────────────────────────┐          │
│  │  Call AuthenticateAtomicHandler                   │          │
│  │    → SOAP: Authenticate(username, password)       │          │
│  │    → Response: SessionId                          │          │
│  └──────────────────────────────────────────────────┘          │
│                            │                                     │
│                            ↓                                     │
│  ┌──────────────────────────────────────────────────┐          │
│  │  if (!response.IsSuccessStatusCode) {            │          │
│  │    throw DownStreamApiFailureException;          │          │
│  │  }                                                │          │
│  │  else {                                           │          │
│  │    Extract SessionId from SOAP response          │          │
│  │    RequestContext.SetSessionId(sessionId);       │          │
│  │  }                                                │          │
│  └──────────────────────────────────────────────────┘          │
│                            │                                     │
│                            ↓                                     │
│  ┌──────────────────────────────────────────────────┐          │
│  │  2. EXECUTE Function                              │          │
│  │     await next(context);                          │          │
│  │                                                    │          │
│  │  Function → Service → Handler → Atomic Handlers   │          │
│  │  All operations use RequestContext.GetSessionId() │          │
│  └──────────────────────────────────────────────────┘          │
│                            │                                     │
│                            ↓                                     │
│  ┌──────────────────────────────────────────────────┐          │
│  │  3. AFTER Function Execution (finally block)     │          │
│  │     (Always executes, even on exception)          │          │
│  └──────────────────────────────────────────────────┘          │
│                            │                                     │
│                            ↓                                     │
│  ┌──────────────────────────────────────────────────┐          │
│  │  Call LogoutAtomicHandler                         │          │
│  │    → SOAP: LogOut(sessionId)                      │          │
│  │                                                    │          │
│  │  if (!response.IsSuccessStatusCode) {            │          │
│  │    _logger.Error("Logout failed - Continue");    │          │
│  │  }                                                │          │
│  │  else {                                           │          │
│  │    _logger.Info("Logout successful");            │          │
│  │  }                                                │          │
│  │                                                    │          │
│  │  RequestContext.Clear();                          │          │
│  └──────────────────────────────────────────────────┘          │
│                            │                                     │
│                            ↓                                     │
│                    Return to Process Layer                       │
└─────────────────────────────────────────────────────────────────┘
```

**Key Benefits:**
- ✅ **Automatic:** Process Layer doesn't manage authentication
- ✅ **Guaranteed:** Login and logout always execute correctly
- ✅ **Thread-safe:** RequestContext uses AsyncLocal<T>
- ✅ **Cleanup:** Logout in finally block (executes even on exception)

---

## ERROR HANDLING FLOW

### Scenario 1: Authentication Failure

```
Process Layer
    │
    │ Call System Layer API
    ↓
System Layer: CustomAuthenticationMiddleware
    │
    ├─→ Login to CAFM
    │   → If FAIL: Throw DownStreamApiFailureException
    │      → ExceptionHandlerMiddleware catches
    │         → Returns BaseResponseDTO with error
    │            → HTTP 401
    ↓
Process Layer receives error response
    │
    ├─→ Log error
    ├─→ Add to failed results
    ├─→ Continue to next work order (or stop based on business rules)
    └─→ Send email notification (error reporting)
```

### Scenario 2: Best-Effort Lookup Failure

```
Process Layer
    │
    │ Call CreateBreakdownTask API
    ↓
System Layer: CreateBreakdownTaskHandler
    │
    ├─→ GetLocationsByDto (internal)
    │   → If FAIL:
    │      → Log warning
    │      → Set locationId = empty
    │      → Set buildingId = empty
    │      → CONTINUE (don't throw)
    │
    ├─→ GetInstructionSetsByDto (internal)
    │   → If FAIL:
    │      → Log warning
    │      → Set instructionId = empty
    │      → CONTINUE (don't throw)
    │
    └─→ CreateBreakdownTask (main operation)
        → Uses lookup results (may be empty)
        → CAFM validates required fields
        → If CAFM validation fails: Returns error
        → If CAFM validation passes: Creates task
    ↓
System Layer returns result to Process Layer
    │
    ├─→ If SUCCESS: Process Layer continues
    └─→ If FAIL: Process Layer handles error
```

**Why This Pattern:**
- ✅ **Resilient:** Lookup failures don't stop main operation
- ✅ **Validation at right place:** CAFM validates required fields (not System Layer)
- ✅ **Accurate errors:** Error from CAFM indicates actual problem (not generic lookup error)
- ✅ **Matches Boomi:** Branch convergence pattern (paths converge regardless of lookup results)

### Scenario 3: Main Operation Failure

```
Process Layer
    │
    │ Call CreateBreakdownTask API
    ↓
System Layer: CreateBreakdownTaskHandler
    │
    └─→ CreateBreakdownTask (main operation)
        → If FAIL: Throw DownStreamApiFailureException
           → ExceptionHandlerMiddleware catches
              → Returns BaseResponseDTO with error
                 → HTTP 400/500
    ↓
Process Layer receives error response
    │
    ├─→ Log error
    ├─→ Add to failed results
    ├─→ Continue to next work order (or stop based on business rules)
    └─→ Send email notification (error reporting)
```

---

## SEQUENCE COMPARISON: BOOMI vs AZURE

### BOOMI Process Flow

```
1. Receive work order array (inputType="singlejson")
2. TRY Block Start
3. BRANCH (6 paths - executed sequentially):
   Path 1: FsiLogin → Extract SessionId
   Path 2: GetBreakdownTasksByDto → Decision (exists?) → Return if exists
   Path 3: GetLocationsByDto → Extract LocationId, BuildingId → Converge
   Path 4: GetInstructionSetsByDto → Extract InstructionId → Converge
   Path 5: Decision (recurrence?) → CreateBreakdownTask → Decision (status?) → Return
   Path 6: FsiLogout
4. CONVERGENCE → Continue to CreateBreakdownTask
5. CreateBreakdownTask → Decision (status 20*?) → Return success
6. CATCH Block → Send email → Return error
```

### AZURE Process Layer Flow

```
1. Receive work order array
2. Loop through each work order:
   
   a. Call System Layer: GetBreakdownTasksByDto
      → If task exists: Return existing, continue to next work order
      → If task not exists: Continue to step b
   
   b. Call System Layer: CreateBreakdownTask
      → System Layer internally:
         - GetLocationsByDto (best-effort)
         - GetInstructionSetsByDto (best-effort)
         - Format dates
         - CreateBreakdownTask (main operation)
      → If success: Store taskId, continue to step c
      → If fail: Log error, add to failed results, continue to next work order
   
   c. Check recurrence flag:
      → If recurrence = "Y": Call System Layer: CreateEvent
      → If recurrence ≠ "Y": Skip event linking
   
   d. Add to results (success or partial success)

3. Return aggregated results to Experience Layer
4. On errors: Send email notification (Process Layer handles)
```

**Key Differences:**
- **Boomi:** Single execution per array element (automatic splitting)
- **Azure:** Process Layer loops through array explicitly
- **Boomi:** Branch paths with convergence
- **Azure:** Sequential System Layer API calls with business decisions
- **Boomi:** Try/Catch with email in catch block
- **Azure:** Process Layer handles errors and email notifications

---

## API CONTRACTS

### Process Layer → System Layer

**All calls use standard HTTP POST with JSON:**

```
POST https://[system-layer-base-url]/api/cafm/[operation]
Content-Type: application/json

Request Body: { ... }

Response: {
  "message": "string",
  "errorCode": "string | null",
  "data": { ... },
  "errorDetails": { ... }
}
```

### System Layer → CAFM (FSI)

**All calls use SOAP/XML over HTTP:**

```
POST https://[cafm-base-url]/api/evolution/[operation]
Content-Type: text/xml
SOAPAction: http://www.fsi.co.uk/services/evolution/04/09/[Operation]

Request Body: <soapenv:Envelope>...</soapenv:Envelope>

Response: <soap:Envelope>...</soap:Envelope>
```

---

## DEPLOYMENT ARCHITECTURE

```
┌──────────────────────────────────────────────────────────────────┐
│                       AZURE CLOUD                                 │
│                                                                   │
│  ┌────────────────────────────────────────────────────────┐     │
│  │              EXPERIENCE LAYER                           │     │
│  │         (Mobile App, Web Portal, IoT)                   │     │
│  └────────────────────────────────────────────────────────┘     │
│                            │                                      │
│                            │ HTTPS                                │
│                            ↓                                      │
│  ┌────────────────────────────────────────────────────────┐     │
│  │              PROCESS LAYER                              │     │
│  │        Azure Function App (.NET 8)                      │     │
│  │                                                          │     │
│  │  Functions:                                             │     │
│  │    - CreateWorkOrderFromEQPlusToCAFM                    │     │
│  │    - [Other process functions]                          │     │
│  │                                                          │     │
│  │  Orchestrates:                                          │     │
│  │    - Business decisions (check existence, recurrence)   │     │
│  │    - System Layer API calls                             │     │
│  │    - Error handling and notifications                   │     │
│  └────────────────────────────────────────────────────────┘     │
│                            │                                      │
│                            │ HTTPS (Internal)                     │
│                            ↓                                      │
│  ┌────────────────────────────────────────────────────────┐     │
│  │              SYSTEM LAYER (CAFMSystem)                  │     │
│  │        Azure Function App (.NET 8)                      │     │
│  │                                                          │     │
│  │  Functions:                                             │     │
│  │    - GetBreakdownTasksByDtoAPI                          │     │
│  │    - CreateBreakdownTaskAPI                             │     │
│  │    - CreateEventAPI                                     │     │
│  │                                                          │     │
│  │  Middleware:                                            │     │
│  │    - CustomAuthenticationMiddleware                     │     │
│  │      (Login → SessionId → Logout)                       │     │
│  │                                                          │     │
│  │  Internal Operations:                                   │     │
│  │    - GetLocationsByDto (best-effort lookup)             │     │
│  │    - GetInstructionSetsByDto (best-effort lookup)       │     │
│  └────────────────────────────────────────────────────────┘     │
│                            │                                      │
│                            │ SOAP/XML over HTTPS                  │
│                            ↓                                      │
│  ┌────────────────────────────────────────────────────────┐     │
│  │         SYSTEM OF RECORD (CAFM/FSI)                     │     │
│  │      External SOAP API (On-Premise or Cloud)            │     │
│  │                                                          │     │
│  │  Operations:                                            │     │
│  │    - Authenticate (Login)                               │     │
│  │    - GetBreakdownTasksByDto                             │     │
│  │    - GetLocationsByDto                                  │     │
│  │    - GetInstructionSetsByDto                            │     │
│  │    - CreateBreakdownTask                                │     │
│  │    - CreateEvent                                        │     │
│  │    - LogOut                                              │     │
│  └────────────────────────────────────────────────────────┘     │
│                                                                   │
│  ┌────────────────────────────────────────────────────────┐     │
│  │              SHARED SERVICES                            │     │
│  │                                                          │     │
│  │  - Azure KeyVault (FSI credentials)                     │     │
│  │  - Azure Redis Cache (session caching)                  │     │
│  │  - Application Insights (monitoring)                    │     │
│  └────────────────────────────────────────────────────────┘     │
└──────────────────────────────────────────────────────────────────┘
```

---

## DECISION OWNERSHIP MATRIX

| Decision Point | Owner | Rationale | Implementation |
|---|---|---|---|
| **Loop through work order array** | Process Layer | Business logic (batch processing) | foreach loop in Process Layer function |
| **Check if task exists** | Process Layer | Business decision (skip or proceed) | Call GetBreakdownTasksByDto → Evaluate result |
| **Skip creation if exists** | Process Layer | Business logic (avoid duplicates) | if (taskExists) return existing; |
| **Execute lookups** | System Layer | Same-SOR operations (all CAFM) | Handler orchestrates internally |
| **Continue if lookup fails** | System Layer | Best-effort pattern (branch convergence) | if (!success) log warning, set empty, continue; |
| **Create breakdown task** | System Layer | Atomic operation (single SOR) | CreateBreakdownTaskAtomicHandler |
| **Link recurring event** | Process Layer | Business decision (conditional based on flag) | if (recurrence == "Y") call CreateEvent; |
| **Send error notifications** | Process Layer | Business logic (error handling) | Catch exceptions, send email |
| **Aggregate results** | Process Layer | Business logic (response composition) | Combine responses from multiple calls |

---

## WHY THIS SEPARATION?

### System Layer: Atomic "Lego Blocks"

**GetBreakdownTasksByDto:**
- ✅ **Reusable:** Any process can check task existence
- ✅ **Atomic:** Single responsibility (check existence)
- ✅ **Independent:** Process Layer decides what to do with result

**CreateBreakdownTask:**
- ✅ **Reusable:** Any process can create tasks
- ✅ **Encapsulated:** Hides CAFM complexity (SOAP, authentication, lookups)
- ✅ **Resilient:** Best-effort lookups (doesn't fail on lookup errors)

**CreateEvent:**
- ✅ **Reusable:** Any process can link events
- ✅ **Atomic:** Single responsibility (link event)
- ✅ **Independent:** Process Layer decides when to call

### Process Layer: Business Orchestration

**CreateWorkOrderFromEQPlusToCAFM:**
- ✅ **Business logic:** Check existence → Skip or create → Link event if recurring
- ✅ **Orchestration:** Calls multiple System Layer APIs in sequence
- ✅ **Decision-making:** Evaluates results and decides next steps
- ✅ **Error handling:** Catches errors, sends notifications, aggregates results
- ✅ **Batch processing:** Loops through work order array

---

## BENEFITS OF THIS ARCHITECTURE

### 1. Separation of Concerns

**System Layer:**
- Unlocks data from CAFM
- Insulates Process Layer from CAFM complexity
- Handles SOAP/XML transformations
- Manages authentication lifecycle

**Process Layer:**
- Implements business workflow
- Makes cross-operation decisions
- Handles batch processing
- Aggregates results

### 2. Reusability

**Example:** Another process needs to create CAFM tasks

```
Process Layer: "Create Emergency Work Order"
    │
    ├─→ Call System Layer: CreateBreakdownTask (REUSE)
    └─→ Different business logic, same System Layer API
```

**No need to recreate:** CAFM integration, SOAP envelopes, authentication

### 3. Maintainability

**CAFM API changes (e.g., new field required):**
- ✅ **Update:** System Layer only (CreateBreakdownTask DTO + SOAP envelope)
- ✅ **No impact:** Process Layer unchanged (contract remains same)

**Business logic changes (e.g., skip creation if priority < High):**
- ✅ **Update:** Process Layer only (add decision logic)
- ✅ **No impact:** System Layer unchanged (atomic operations remain same)

### 4. Testability

**System Layer:**
- Test each API independently
- Mock CAFM SOAP responses
- Verify SOAP envelope construction

**Process Layer:**
- Test business orchestration
- Mock System Layer API responses
- Verify decision logic and error handling

---

## REFERENCE MAPPING

### Boomi Shapes → Azure Components

| Boomi Component | Azure Component | Layer | File |
|---|---|---|---|
| shape5 (FsiLogin subprocess) | CustomAuthenticationMiddleware | System | Middleware/CustomAuthenticationMiddleware.cs |
| shape53 (GetBreakdownTasksByDto) | GetBreakdownTasksByDtoAPI | System | Functions/GetBreakdownTasksByDtoAPI.cs |
| shape24 (GetLocationsByDto) | GetLocationsByDtoAtomicHandler | System | Implementations/FSI/AtomicHandlers/GetLocationsByDtoAtomicHandler.cs |
| shape27 (GetInstructionSetsByDto) | GetInstructionSetsByDtoAtomicHandler | System | Implementations/FSI/AtomicHandlers/GetInstructionSetsByDtoAtomicHandler.cs |
| shape11 (CreateBreakdownTask) | CreateBreakdownTaskAPI | System | Functions/CreateBreakdownTaskAPI.cs |
| shape35 (CreateEvent) | CreateEventAPI | System | Functions/CreateEventAPI.cs |
| shape13 (FsiLogout subprocess) | CustomAuthenticationMiddleware | System | Middleware/CustomAuthenticationMiddleware.cs |
| shape55 (Decision: task exists?) | Process Layer if/else | Process | [To be implemented] |
| shape31 (Decision: recurrence?) | Process Layer if/else | Process | [To be implemented] |
| shape4 (Branch: 6 paths) | Process Layer sequential calls | Process | [To be implemented] |
| shape20 (Error branch: email) | Process Layer error handling | Process | [To be implemented] |

### Phase 1 Sections → Code Components

| Phase 1 Section | Code Component | File |
|---|---|---|
| Section 2 (Input Structure) | CreateBreakdownTaskReqDTO | DTO/CreateBreakdownTaskDTO/CreateBreakdownTaskReqDTO.cs |
| Section 3 (Response Structure) | CreateBreakdownTaskResDTO | DTO/CreateBreakdownTaskDTO/CreateBreakdownTaskResDTO.cs |
| Section 5 (Map Analysis) | CreateBreakdownTask.xml | SoapEnvelopes/CreateBreakdownTask.xml |
| Section 13.1 (Classification) | Handler error handling | Implementations/FSI/Handlers/*.cs |
| Section 13.2 (Sequence Diagram) | Handler orchestration | Implementations/FSI/Handlers/CreateBreakdownTaskHandler.cs |

---

## SUMMARY

**Process Layer Role:**
- 🎯 **Orchestrator:** Calls System Layer APIs in sequence based on business logic
- 🎯 **Decision Maker:** Evaluates results and decides next steps (skip, proceed, link event)
- 🎯 **Aggregator:** Combines responses from multiple System Layer calls
- 🎯 **Error Handler:** Catches errors, sends notifications, manages batch processing

**System Layer Role:**
- 🔧 **API Provider:** Exposes 3 independent Azure Functions (Lego blocks)
- 🔧 **SOR Abstraction:** Hides CAFM complexity (SOAP, authentication, lookups)
- 🔧 **Resilience:** Best-effort lookups (continues on failure)
- 🔧 **Authentication:** Automatic session management via middleware

**Key Principle:** System Layer provides atomic operations. Process Layer orchestrates them based on business needs.

---

**END OF ORCHESTRATION DIAGRAM**
