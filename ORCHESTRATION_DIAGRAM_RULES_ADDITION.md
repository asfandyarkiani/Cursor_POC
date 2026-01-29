# ORCHESTRATION DIAGRAM RULES - ADD TO BOOMI_EXTRACTION_RULES.mdc

**Purpose:** Rules for generating Process Layer ↔ System Layer orchestration diagrams in Phase 1  
**Location:** Add this section AFTER "README.md GENERATION CHECKLIST" and BEFORE "Document Version" footer  
**Version:** 2.6 Addition

---

## 🛑 POST-PHASE 1: ORCHESTRATION DIAGRAM GENERATION (MANDATORY)

**🚨 CRITICAL:** After completing Phase 1 extraction (Sections 1-19), you MUST generate a Process Layer ↔ System Layer orchestration diagram as **Section 20** in BOOMI_EXTRACTION_PHASE1.md.

**WHEN TO CREATE:** After Section 18 (Function Exposure Decision Table) is complete

**WHY MANDATORY:** 
- Shows how Process Layer orchestrates System Layer APIs
- Documents decision ownership (which decisions belong to which layer)
- Clarifies layer responsibilities and boundaries
- Provides visual representation of API-Led Architecture implementation
- Essential for Process Layer developers to understand how to call System Layer APIs

---

### STEP 11: CREATE ORCHESTRATION DIAGRAM (MANDATORY)

**🛑 EXPLICIT ENFORCEMENT:**

**YOU CANNOT DECLARE PHASE 1 COMPLETE** until this section is created and documented.

**REQUIRED OUTPUT**: You MUST create a section in Phase 1 document titled:
```
## 20. Process Layer ↔ System Layer Orchestration Diagram
```

This section MUST be created AFTER Section 18 (Function Exposure Decision Table) is complete.

**VALIDATION CHECKPOINT**: 
- If Phase 1 document does NOT contain section "Process Layer ↔ System Layer Orchestration Diagram" → Phase 1 NOT complete → **CANNOT PROCEED TO PHASE 2**

---

### 20.1 ORCHESTRATION DIAGRAM STRUCTURE (MANDATORY SECTIONS)

The orchestration diagram MUST include these sections in this order:

#### Section 20.1: Overview

**Required Content:**
- Brief description of orchestration pattern
- Key principle statement (System Layer = Lego blocks, Process Layer = orchestrator)
- Summary of layer responsibilities

**Template:**
```markdown
## 20. Process Layer ↔ System Layer Orchestration Diagram

### 20.1 Overview

This diagram shows how Process Layer orchestrates System Layer APIs to implement the complete business workflow extracted from the Boomi process.

**Key Principle:** System Layer provides atomic "Lego block" APIs. Process Layer orchestrates them based on business logic.

**Layer Responsibilities:**
- **Process Layer:** Business orchestration, decision-making, batch processing, error aggregation
- **System Layer:** Atomic operations, SOR abstraction, authentication management, SOAP/REST handling
```

#### Section 20.2: Complete Orchestration Flow

**Required Content:**
- ASCII diagram showing complete flow from Process Layer to System Layer to SOR
- Show ALL System Layer API calls identified in Section 18
- Show business decisions (from Section 10) and which layer owns them
- Show data flow between layers
- Mark authentication middleware interception points

**Template Structure:**
```
Process Layer Function
    │
    │ Business Decision 1: [Decision from Section 10]
    ↓
System Layer API #1: [Function from Section 18]
    │
    │ [CustomAuthentication] Middleware
    │   ├─→ Login
    │   ├─→ Execute Function
    │   └─→ Logout (finally)
    │
    │ Handler orchestrates:
    │   ├─→ Internal Operation 1 (Atomic Handler)
    │   ├─→ Internal Operation 2 (Atomic Handler)
    │   └─→ Main Operation (Atomic Handler)
    ↓
Returns to Process Layer
    │
    │ Business Decision 2: [Decision from Section 10]
    ↓
System Layer API #2: [Function from Section 18]
    [... repeat for all operations ...]
```

**CRITICAL RULES:**
1. Show ALL System Layer Functions from Section 18
2. Show ALL business decisions from Section 10
3. Mark which layer owns each decision (Process vs System)
4. Show authentication middleware interception
5. Show internal operations (Atomic Handlers not exposed as Functions)
6. Show data flow direction (request → response)

#### Section 20.3: Operation-Level Orchestration

**Required Content:**
- Detailed diagram for EACH System Layer Function
- Show request/response structure
- Show middleware authentication flow
- Show internal handler orchestration (if any)
- Show error handling at each level

**Template for EACH Function:**
```markdown
### Operation N: [Function Name]

[ASCII diagram showing:]
- Process Layer call with request structure
- System Layer Function entry
- Middleware authentication (if [CustomAuthentication] applied)
- Service → Handler → Atomic Handler(s) flow
- Internal operations (if Handler orchestrates multiple Atomic Handlers)
- Response structure
- Return to Process Layer
- Process Layer decision based on response
```

**CRITICAL RULES:**
1. Create subsection for EACH System Layer Function from Section 18
2. Show complete request/response JSON structure
3. Show middleware flow (if authentication required)
4. Show internal orchestration (if Handler calls multiple Atomic Handlers)
5. Show error handling (throw vs continue)
6. Reference Section 13.1 (classification) and Section 13.2 (sequence diagram)

#### Section 20.4: System Layer Internal Orchestration

**Required Content:**
- Diagram showing Handler internal orchestration (when Handler calls multiple Atomic Handlers)
- Show best-effort lookup pattern (if applicable)
- Show sequential execution of internal operations
- Show error handling for each internal operation

**When to Include:**
- If ANY Handler orchestrates multiple Atomic Handlers (same-SOR pattern)
- If best-effort lookup pattern identified (Section 15)
- If Handler has conditional logic (simple if/else for same-SOR operations)

**Template:**
```markdown
### [Handler Name] Internal Flow

Handler orchestrates [N] internal operations (same SOR: [SOR_NAME]):

[ASCII diagram showing:]
- STEP 1: [Internal Operation 1] (CLASSIFICATION)
  - If SUCCESS: Extract data
  - If FAIL: [Error handling from Section 13.1]
- STEP 2: [Internal Operation 2] (CLASSIFICATION)
  - If SUCCESS: Extract data
  - If FAIL: [Error handling from Section 13.1]
- STEP 3: [Main Operation] (CLASSIFICATION)
  - Uses data from STEP 1, 2 (may be empty)
  - If FAIL: Throw exception
```

**CRITICAL RULES:**
1. Show ONLY Handlers that orchestrate multiple Atomic Handlers
2. Show error handling for EACH internal operation (from Section 13.1)
3. Show data flow between internal operations
4. Mark best-effort operations vs main operations
5. Reference Section 13.1 for classification

#### Section 20.5: Authentication Flow

**Required Content:**
- Complete authentication lifecycle diagram
- Show CustomAuthenticationMiddleware flow (if session/token-based auth)
- Show RequestContext usage
- Show login → execute → logout (finally) pattern

**When to Include:**
- If authentication pattern identified in Section 15
- If session-based or token-based authentication used

**Template:**
```markdown
### Authentication Flow (Session-Based)

[ASCII diagram showing:]
- Process Layer calls System Layer API
- CustomAuthenticationMiddleware intercepts
- BEFORE function execution:
  - Retrieve credentials from KeyVault
  - Call AuthenticateAtomicHandler
  - Extract SessionId/Token
  - Store in RequestContext
- EXECUTE function:
  - All operations use RequestContext.GetSessionId()
- AFTER function execution (finally):
  - Call LogoutAtomicHandler
  - Clear RequestContext
- Return to Process Layer
```

**CRITICAL RULES:**
1. Show complete lifecycle (before → execute → finally)
2. Show RequestContext usage (SetSessionId, GetSessionId)
3. Show KeyVault credential retrieval
4. Show finally block (logout always executes)
5. Reference Section 15 (authentication pattern)

#### Section 20.6: Error Handling Flows

**Required Content:**
- Error handling scenarios for EACH operation classification
- Show exception flow (throw → middleware catch → return error)
- Show best-effort pattern (log warning → continue)
- Show cleanup pattern (log error → continue)

**Template:**
```markdown
### Error Handling Flows

#### Scenario 1: Authentication Failure

[ASCII diagram showing:]
- Process Layer calls System Layer
- Middleware login fails
- Throw DownStreamApiFailureException
- ExceptionHandlerMiddleware catches
- Returns BaseResponseDTO with error (HTTP 401)
- Process Layer receives error response
- Process Layer decides: Log, notify, continue or stop

#### Scenario 2: Best-Effort Lookup Failure

[ASCII diagram showing:]
- Handler calls lookup Atomic Handler
- Lookup fails (404, 500)
- Log warning
- Set empty value
- CONTINUE to main operation
- Main operation uses empty value (SOR validates)

#### Scenario 3: Main Operation Failure

[ASCII diagram showing:]
- Handler calls main operation Atomic Handler
- Operation fails (400, 500)
- Throw DownStreamApiFailureException
- ExceptionHandlerMiddleware catches
- Returns BaseResponseDTO with error
- Process Layer receives error response
- Process Layer decides: Log, notify, continue or stop
```

**CRITICAL RULES:**
1. Create scenario for EACH operation classification (AUTHENTICATION, BEST-EFFORT, MAIN, CONDITIONAL, CLEANUP)
2. Show exception flow for throw scenarios
3. Show continue flow for best-effort/cleanup scenarios
4. Show Process Layer response handling
5. Reference Section 13.1 for classification

#### Section 20.7: Data Flow Diagram

**Required Content:**
- Complete architecture diagram showing all layers
- Experience Layer → Process Layer → System Layer → SOR
- Show Azure resources (Function Apps, KeyVault, Redis, App Insights)
- Show HTTP/SOAP protocols

**Template:**
```markdown
### Data Flow Diagram

[ASCII diagram showing:]
┌─────────────────────────────────────┐
│      EXPERIENCE LAYER                │
│   (Mobile, Web, IoT)                 │
└─────────────────────────────────────┘
              │ HTTPS
              ↓
┌─────────────────────────────────────┐
│      PROCESS LAYER                   │
│   Azure Function App                 │
│   - Business orchestration           │
│   - Decision logic                   │
│   - Batch processing                 │
└─────────────────────────────────────┘
              │ HTTPS (Internal)
              ↓
┌─────────────────────────────────────┐
│      SYSTEM LAYER                    │
│   Azure Function App                 │
│   - Atomic operations                │
│   - SOR abstraction                  │
│   - Authentication middleware        │
└─────────────────────────────────────┘
              │ SOAP/REST
              ↓
┌─────────────────────────────────────┐
│   SYSTEM OF RECORD (SOR)             │
│   External API                       │
└─────────────────────────────────────┘

Shared Services:
- Azure KeyVault (credentials)
- Azure Redis Cache (session/data caching)
- Application Insights (monitoring)
```

**CRITICAL RULES:**
1. Show all 3 layers (Experience, Process, System) + SOR
2. Show protocols (HTTPS, SOAP/REST)
3. Show Azure resources (KeyVault, Redis, App Insights)
4. Show data flow direction

#### Section 20.8: Decision Ownership Matrix

**Required Content:**
- Table showing ALL decision points from Section 10
- For each decision: Owner (Process Layer or System Layer)
- For each decision: Rationale (why that layer owns it)
- For each decision: Implementation approach

**Template:**
```markdown
### Decision Ownership Matrix

| Decision Point | Owner | Rationale | Implementation |
|---|---|---|---|
| [Decision from Section 10] | Process Layer | [Why PL owns] | [How implemented] |
| [Decision from Section 10] | System Layer | [Why SL owns] | [How implemented] |
```

**CRITICAL RULES:**
1. Include ALL decisions from Section 10
2. Assign owner based on decision type:
   - Cross-operation decisions (if X exists skip Y) → Process Layer
   - Conditional operations (if flag do X) → Process Layer
   - Error handling decisions (if status 20* vs 50*) → System Layer
   - Same-SOR simple decisions (if lookup fails continue) → System Layer
3. Provide rationale for EACH assignment
4. Show implementation approach (API call, if/else, exception handling)

#### Section 20.9: Layer Responsibilities Summary

**Required Content:**
- What Process Layer DOES
- What Process Layer DOES NOT DO
- What System Layer DOES
- What System Layer DOES NOT DO

**Template:**
```markdown
### Layer Responsibilities Summary

#### Process Layer Responsibilities

**What Process Layer DOES:**
- [List from analysis]

**What Process Layer DOES NOT DO:**
- [List from analysis]

#### System Layer Responsibilities

**What System Layer DOES:**
- [List from analysis]

**What System Layer DOES NOT DO:**
- [List from analysis]
```

**CRITICAL RULES:**
1. List MUST be based on actual operations from Section 1
2. Include specific examples from this process
3. Show clear boundaries between layers
4. Reference API-Led Architecture principles

#### Section 20.10: Benefits of This Architecture

**Required Content:**
- Separation of concerns
- Reusability examples
- Maintainability scenarios
- Testability approach

**Template:**
```markdown
### Benefits of This Architecture

#### 1. Separation of Concerns
[Explain with examples from this process]

#### 2. Reusability
[Show how System Layer APIs can be reused by other processes]

#### 3. Maintainability
[Show change scenarios and impact isolation]

#### 4. Testability
[Show how each layer can be tested independently]
```

#### Section 20.11: Reference Mapping

**Required Content:**
- Boomi shapes → Azure components mapping
- Phase 1 sections → Code files mapping

**Template:**
```markdown
### Reference Mapping

#### Boomi Shapes → Azure Components

| Boomi Component | Azure Component | Layer | File |
|---|---|---|---|
| shape[N] ([Operation]) | [Component Name] | System/Process | [File path] |

#### Phase 1 Sections → Code Components

| Phase 1 Section | Code Component | File |
|---|---|---|
| Section [N] ([Title]) | [Component Name] | [File path] |
```

**CRITICAL RULES:**
1. Map ALL operations from Section 1 to Azure components
2. Map ALL decisions from Section 10 to Process Layer logic
3. Map ALL Phase 1 sections to code files
4. Show layer assignment (Process vs System)

---

### 20.2 ORCHESTRATION DIAGRAM GENERATION ALGORITHM

**Execute this algorithm AFTER Section 18 is complete:**

```
STEP 1: Extract System Layer Functions
────────────────────────────────────────
Read Section 18 (Function Exposure Decision Table)

FOR EACH operation marked as "Function":
    system_layer_functions.append({
        "name": operation.name,
        "purpose": operation.purpose,
        "route": generate_route(operation.name),
        "classification": get_classification_from_section_13_1(operation),
        "error_handling": get_error_handling_from_section_13_1(operation)
    })

OUTPUT: List of System Layer Functions to expose

STEP 2: Extract Business Decisions
────────────────────────────────────
Read Section 10 (Decision Shape Analysis)

FOR EACH decision shape:
    IF decision is cross-operation (if X exists skip Y):
        decision_owner = "Process Layer"
        rationale = "Cross-operation business logic"
    ELIF decision is conditional operation (if flag do X):
        decision_owner = "Process Layer"
        rationale = "Business rule based on input flag"
    ELIF decision is error handling (if status 20* vs 50*):
        decision_owner = "System Layer"
        rationale = "Error handling for downstream API"
    ELIF decision is same-SOR simple logic (if lookup fails continue):
        decision_owner = "System Layer"
        rationale = "Same-SOR orchestration with simple if/else"
    
    business_decisions.append({
        "decision": decision.description,
        "owner": decision_owner,
        "rationale": rationale,
        "boomi_reference": decision.shape_id
    })

OUTPUT: Decision ownership matrix

STEP 3: Create Complete Orchestration Flow Diagram
────────────────────────────────────────────────────
Based on Section 12 (Execution Order) and Section 18 (Function Exposure):

FOR EACH System Layer Function:
    diagram += "Process Layer"
    diagram += "  │"
    diagram += "  │ Business Decision: [From Section 10]"
    diagram += "  ↓"
    diagram += "System Layer API: [Function name]"
    diagram += "  │"
    diagram += "  │ [CustomAuthentication] Middleware (if applicable)"
    diagram += "  │   ├─→ Login"
    diagram += "  │   ├─→ Execute Function"
    diagram += "  │   └─→ Logout (finally)"
    diagram += "  │"
    diagram += "  │ Handler: [Handler name from Section 13.2]"
    
    IF Handler orchestrates multiple Atomic Handlers:
        diagram += "  │   ├─→ Internal Operation 1: [Atomic Handler]"
        diagram += "  │   │   └─→ [Classification from Section 13.1]"
        diagram += "  │   │   └─→ [Error handling from Section 13.1]"
        diagram += "  │   ├─→ Internal Operation 2: [Atomic Handler]"
        diagram += "  │   └─→ Main Operation: [Atomic Handler]"
    ELSE:
        diagram += "  │   └─→ Atomic Handler: [Name]"
    
    diagram += "  ↓"
    diagram += "Returns to Process Layer"
    diagram += "  │"
    diagram += "  │ Process Layer evaluates response"
    diagram += "  │ [Next decision or operation]"

OUTPUT: Complete orchestration flow diagram

STEP 4: Create Operation-Level Diagrams
─────────────────────────────────────────
FOR EACH System Layer Function:
    Create detailed diagram showing:
    - Process Layer call with full request structure
    - System Layer Function entry
    - Middleware flow (if applicable)
    - Handler → Atomic Handler(s) flow
    - SOAP/REST call to SOR
    - Response deserialization
    - Return to Process Layer
    - Process Layer decision based on response

OUTPUT: Operation-level orchestration diagrams

STEP 5: Create System Layer Internal Orchestration Diagram
────────────────────────────────────────────────────────────
FOR EACH Handler that orchestrates multiple Atomic Handlers:
    Create diagram showing:
    - Handler entry
    - STEP 1: Internal Operation 1
      - Classification from Section 13.1
      - Error handling from Section 13.1
      - Result (populated or empty)
    - STEP 2: Internal Operation 2
      - Classification from Section 13.1
      - Error handling from Section 13.1
      - Result (populated or empty)
    - STEP 3: Main Operation
      - Uses results from STEP 1, 2 (may be empty)
      - Classification from Section 13.1
      - Error handling from Section 13.1

OUTPUT: Internal orchestration diagrams

STEP 6: Create Authentication Flow Diagram
────────────────────────────────────────────
IF authentication pattern identified in Section 15:
    Create diagram showing:
    - Process Layer call
    - Middleware interception
    - BEFORE: Login → Extract SessionId → Store in RequestContext
    - EXECUTE: Function uses RequestContext.GetSessionId()
    - AFTER (finally): Logout → Clear RequestContext
    - Return to Process Layer

OUTPUT: Authentication flow diagram

STEP 7: Create Error Handling Flow Diagrams
─────────────────────────────────────────────
FOR EACH operation classification in Section 13.1:
    Create error scenario diagram:
    - AUTHENTICATION: Show throw exception → middleware catch → return error
    - BEST-EFFORT LOOKUP: Show log warning → set empty → continue
    - MAIN OPERATION: Show throw exception → middleware catch → return error
    - CONDITIONAL: Show throw exception → middleware catch → return error
    - CLEANUP: Show log error → continue (no throw)

OUTPUT: Error handling flow diagrams

STEP 8: Create Data Flow Diagram
──────────────────────────────────
Create architecture diagram showing:
- Experience Layer (top)
- Process Layer (middle)
- System Layer (middle-lower)
- System of Record (bottom)
- Shared Services (side: KeyVault, Redis, App Insights)
- Protocols (HTTPS, SOAP/REST)
- Data flow direction

OUTPUT: Data flow architecture diagram

STEP 9: Create Decision Ownership Matrix
──────────────────────────────────────────
Based on Section 10 (Decision Analysis):

FOR EACH decision:
    Determine owner:
    - IF decision is cross-operation → Process Layer
    - IF decision is conditional operation → Process Layer
    - IF decision is error handling → System Layer
    - IF decision is same-SOR simple logic → System Layer
    
    Add to matrix:
    - Decision point
    - Owner (Process Layer or System Layer)
    - Rationale (why that layer)
    - Implementation (how it's implemented)

OUTPUT: Decision ownership matrix table

STEP 10: Create Layer Responsibilities Summary
────────────────────────────────────────────────
Based on Section 1 (Operations Inventory) and Section 18 (Function Exposure):

Process Layer DOES:
- List all business orchestration activities
- List all decision-making activities
- List all aggregation activities

Process Layer DOES NOT DO:
- List all SOR-specific activities
- List all technical activities (SOAP, auth)

System Layer DOES:
- List all SOR abstraction activities
- List all technical activities (SOAP, auth, deserialization)
- List all atomic operations

System Layer DOES NOT DO:
- List all business orchestration activities
- List all cross-operation decisions

OUTPUT: Layer responsibilities summary

STEP 11: Create Benefits Section
──────────────────────────────────
Document benefits with examples from this process:
- Separation of concerns (example: CAFM changes only affect System Layer)
- Reusability (example: CreateBreakdownTask can be reused by other processes)
- Maintainability (example: Business logic changes only affect Process Layer)
- Testability (example: System Layer tested with mock SOAP responses)

OUTPUT: Benefits section with concrete examples

STEP 12: Create Reference Mapping
───────────────────────────────────
Map Boomi components to Azure components:
- Read Section 1 (Operations Inventory)
- Read Section 18 (Function Exposure Decision)
- Map each Boomi operation to Azure component
- Map each decision shape to Process Layer logic
- Map each subprocess to middleware or atomic handler

Map Phase 1 sections to code files:
- Section 2 (Input Structure) → ReqDTO files
- Section 3 (Response Structure) → ResDTO files
- Section 5 (Map Analysis) → SOAP envelope files
- Section 13.1 (Classification) → Handler error handling
- Section 13.2 (Sequence Diagram) → Handler orchestration

OUTPUT: Reference mapping tables
```

---

### 20.3 MANDATORY CONTENT CHECKLIST

Before declaring Section 20 complete, verify:

- [ ] Section 20.1: Overview present
- [ ] Section 20.2: Complete orchestration flow diagram present
- [ ] Section 20.3: Operation-level diagrams for ALL System Layer Functions
- [ ] Section 20.4: System Layer internal orchestration (if applicable)
- [ ] Section 20.5: Authentication flow diagram (if applicable)
- [ ] Section 20.6: Error handling flow diagrams (all scenarios)
- [ ] Section 20.7: Data flow diagram present
- [ ] Section 20.8: Decision ownership matrix present
- [ ] Section 20.9: Layer responsibilities summary present
- [ ] Section 20.10: Benefits section present
- [ ] Section 20.11: Reference mapping present

**IF ANY MISSING → Section 20 NOT complete → Phase 1 NOT complete → CANNOT PROCEED TO PHASE 2**

---

### 20.4 ORCHESTRATION DIAGRAM VALIDATION RULES

#### Rule 1: All System Layer Functions Must Be Shown

**Validation:**
```
system_layer_functions_in_section_18 = Extract from Section 18 (Function Exposure Decision)
system_layer_functions_in_section_20 = Extract from Section 20 (Orchestration Diagram)

IF system_layer_functions_in_section_18 != system_layer_functions_in_section_20:
    ERROR: "Section 20 missing System Layer Functions"
    ACTION: Add missing functions to orchestration diagram
```

#### Rule 2: All Business Decisions Must Be Assigned to a Layer

**Validation:**
```
decisions_in_section_10 = Extract from Section 10 (Decision Analysis)
decisions_in_section_20_matrix = Extract from Section 20.8 (Decision Ownership Matrix)

IF decisions_in_section_10 != decisions_in_section_20_matrix:
    ERROR: "Section 20 missing decision assignments"
    ACTION: Add missing decisions to decision ownership matrix
```

#### Rule 3: Authentication Flow Must Match Section 15 Pattern

**Validation:**
```
auth_pattern_in_section_15 = Extract from Section 15 (Critical Patterns)

IF auth_pattern_in_section_15 == "Session-Based Authentication":
    REQUIRE: Section 20.5 shows complete session lifecycle
    REQUIRE: Login → Store SessionId → Execute → Logout (finally)
    REQUIRE: RequestContext usage shown
```

#### Rule 4: Error Handling Must Match Section 13.1 Classification

**Validation:**
```
FOR EACH operation in Section 13.1:
    classification = operation.classification
    error_handling_in_section_20 = Extract from Section 20.6
    
    IF classification == "AUTHENTICATION":
        REQUIRE: Section 20.6 shows throw exception scenario
    ELIF classification == "BEST-EFFORT LOOKUP":
        REQUIRE: Section 20.6 shows log warning, set empty, continue scenario
    ELIF classification == "MAIN OPERATION":
        REQUIRE: Section 20.6 shows throw exception scenario
    ELIF classification == "CONDITIONAL":
        REQUIRE: Section 20.6 shows throw exception scenario
    ELIF classification == "CLEANUP":
        REQUIRE: Section 20.6 shows log error, continue scenario
```

#### Rule 5: Internal Orchestration Must Match Section 13.2

**Validation:**
```
handlers_with_multiple_atomics = Extract from Section 13.2 (Sequence Diagram)

FOR EACH handler in handlers_with_multiple_atomics:
    REQUIRE: Section 20.4 shows internal orchestration diagram for this handler
    REQUIRE: Diagram shows all internal operations
    REQUIRE: Diagram shows error handling for each internal operation
    REQUIRE: Diagram references Section 13.1 for classification
```

---

### 20.5 COMMON MISTAKES TO AVOID

#### Mistake 1: Creating Orchestration Diagram Before Section 18

**❌ WRONG:**
```
Create Section 20 (Orchestration Diagram) before Section 18 (Function Exposure Decision)
```

**✅ CORRECT:**
```
1. Complete Section 18 (Function Exposure Decision Table)
2. Know which operations are Functions vs Atomic Handlers
3. THEN create Section 20 (Orchestration Diagram)
```

**Why:** Section 20 shows how Process Layer calls System Layer Functions. You must know which operations are Functions first.

#### Mistake 2: Not Showing Business Decisions

**❌ WRONG:**
```
Orchestration diagram shows only System Layer API calls, no business decisions
```

**✅ CORRECT:**
```
Show business decisions BETWEEN System Layer API calls:
- Process Layer calls API #1
- Process Layer evaluates response
- Process Layer makes decision (skip or proceed)
- Process Layer calls API #2 (based on decision)
```

**Why:** Process Layer orchestration IS the business decisions. Must show decision points.

#### Mistake 3: Not Showing Internal Orchestration

**❌ WRONG:**
```
Show Handler as black box: "Handler → Result"
```

**✅ CORRECT:**
```
Show Handler internal orchestration:
- Handler entry
- STEP 1: Internal Operation 1 (BEST-EFFORT) → If fail, log warning, continue
- STEP 2: Internal Operation 2 (BEST-EFFORT) → If fail, log warning, continue
- STEP 3: Main Operation (MAIN) → If fail, throw exception
```

**Why:** Shows same-SOR orchestration pattern and best-effort lookup pattern.

#### Mistake 4: Not Showing Authentication Middleware

**❌ WRONG:**
```
Process Layer → System Layer Function → Handler
```

**✅ CORRECT:**
```
Process Layer → System Layer Function
  ↓
[CustomAuthentication] Middleware intercepts
  ├─→ Login (before function)
  ├─→ Execute Function
  └─→ Logout (finally, after function)
  ↓
Handler → Atomic Handler
```

**Why:** Authentication is critical part of System Layer. Must show middleware interception.

#### Mistake 5: Not Assigning Decision Ownership

**❌ WRONG:**
```
Decision ownership matrix missing or incomplete
```

**✅ CORRECT:**
```
Decision Ownership Matrix with ALL decisions from Section 10:
- Decision 1: Check existence → Process Layer (cross-operation decision)
- Decision 2: Recurrence flag → Process Layer (conditional operation)
- Decision 3: Status code check → System Layer (error handling)
```

**Why:** Clarifies which layer makes which decisions. Essential for Process Layer implementation.

#### Mistake 6: Generic Layer Responsibilities

**❌ WRONG:**
```
Process Layer: "Orchestrates business logic"
System Layer: "Calls external APIs"
```

**✅ CORRECT:**
```
Process Layer DOES:
- Loop through work order array (batch processing)
- Call GetBreakdownTasksByDto to check existence
- Evaluate result: If exists, skip creation; if not, proceed
- Call CreateBreakdownTask to create work order
- Check recurrence flag: If "Y", call CreateEvent; if not, skip
- Aggregate results from all operations
- Handle errors and send email notifications

System Layer DOES:
- Expose 3 Azure Functions (GetBreakdownTasksByDto, CreateBreakdownTask, CreateEvent)
- Handle CAFM session-based authentication (middleware)
- Execute SOAP operations (7 operations total)
- Perform best-effort lookups (GetLocationsByDto, GetInstructionSetsByDto)
- Deserialize SOAP responses to JSON DTOs
- Return standardized BaseResponseDTO
```

**Why:** Specific examples from THIS process. Not generic descriptions.

---

### 20.6 ORCHESTRATION DIAGRAM EXAMPLES

#### Example 1: Simple Sequential Calls

**Boomi Pattern:** Operation A → Operation B → Operation C (no decisions)

**Orchestration Diagram:**
```
Process Layer
    │
    │ Call System Layer: OperationA
    ↓
System Layer: OperationA API
    │ Handler → Atomic Handler → SOR
    ↓
Returns to Process Layer
    │
    │ Call System Layer: OperationB
    ↓
System Layer: OperationB API
    │ Handler → Atomic Handler → SOR
    ↓
Returns to Process Layer
    │
    │ Call System Layer: OperationC
    ↓
System Layer: OperationC API
    │ Handler → Atomic Handler → SOR
    ↓
Returns to Process Layer
```

**Decision Ownership:** No decisions (sequential execution)

#### Example 2: Check-Before-Create Pattern

**Boomi Pattern:** GetEntity → Decision (exists?) → If not exists, CreateEntity

**Orchestration Diagram:**
```
Process Layer
    │
    │ DECISION 1: Check Existence
    │ Call System Layer: GetEntity
    ↓
System Layer: GetEntityAPI
    │ Handler → Atomic Handler → SOR
    │ Response: { entityExists: true/false, entityId: "..." }
    ↓
Returns to Process Layer
    │
    │ DECISION 1: Evaluate Response
    │ if (entityExists) {
    │   Return existing entity [EARLY EXIT]
    │ }
    │ else {
    │   Proceed to create
    │ }
    ↓
    │ DECISION 2: Create Entity
    │ Call System Layer: CreateEntity
    ↓
System Layer: CreateEntityAPI
    │ Handler → Atomic Handler → SOR
    │ Response: { entityId: "NEW-123", status: "Created" }
    ↓
Returns to Process Layer
```

**Decision Ownership:**
- Decision 1 (Check existence): Process Layer (cross-operation decision)
- Decision 2 (Create entity): Process Layer (based on Decision 1 result)

#### Example 3: Handler with Internal Orchestration

**Boomi Pattern:** Branch (Path 1: GetTypeId, Path 2: GetCategoryId) → Converge → CreateEntity

**Orchestration Diagram:**
```
Process Layer
    │
    │ Call System Layer: CreateEntity
    ↓
System Layer: CreateEntityAPI
    │
    │ Handler: CreateEntityHandler
    │   │
    │   ├─→ INTERNAL STEP 1: GetTypeId (BEST-EFFORT LOOKUP)
    │   │   → GetTypeIdAtomicHandler → SOR
    │   │   → If SUCCESS: typeId = "TYPE-123"
    │   │   → If FAIL: Log warning, typeId = empty, CONTINUE
    │   │
    │   ├─→ INTERNAL STEP 2: GetCategoryId (BEST-EFFORT LOOKUP)
    │   │   → GetCategoryIdAtomicHandler → SOR
    │   │   → If SUCCESS: categoryId = "CAT-456"
    │   │   → If FAIL: Log warning, categoryId = empty, CONTINUE
    │   │
    │   └─→ INTERNAL STEP 3: CreateEntity (MAIN OPERATION)
    │       → CreateEntityAtomicHandler → SOR
    │       → Uses: typeId, categoryId (may be empty)
    │       → If FAIL: Throw exception
    │
    │ Response: { entityId: "ENT-789", status: "Created" }
    ↓
Returns to Process Layer
```

**Decision Ownership:**
- Internal lookup error handling: System Layer (same-SOR, best-effort pattern)
- Main operation error handling: System Layer (throws exception)

**Key Point:** System Layer Handler orchestrates internal operations (same SOR). Process Layer sees single API call.

---

### 20.7 INTEGRATION WITH EXISTING SECTIONS

**Section 20 (Orchestration Diagram) MUST reference:**

1. **Section 1 (Operations Inventory):** List of all operations
2. **Section 10 (Decision Analysis):** All decision points and their patterns
3. **Section 13.1 (Operation Classification):** Error handling for each operation
4. **Section 13.2 (Sequence Diagram):** Complete execution flow
5. **Section 15 (Critical Patterns):** Authentication, best-effort, check-before-create patterns
6. **Section 18 (Function Exposure Decision):** Which operations are Functions vs Atomic Handlers

**References Format:**
```markdown
**Based on:**
- Section 10 (Decision Analysis): Decision shape55 (task exists check)
- Section 13.1 (Operation Classification): GetLocationsByDto is BEST-EFFORT LOOKUP
- Section 13.2 (Sequence Diagram): Complete execution order
- Section 15 (Critical Patterns): Best-effort lookup pattern
- Section 18 (Function Exposure Decision): 3 Functions + 5 Atomic Handlers
```

---

### 20.8 PHASE 1 DOCUMENT STRUCTURE (UPDATED)

**🚨 CRITICAL ENFORCEMENT: Section 20 is NOW MANDATORY**

**Updated Phase 1 Document Structure:**

1. Operations Inventory
2. Input Structure Analysis (Step 1a)
3. Response Structure Analysis (Step 1b)
4. Operation Response Analysis (Step 1c)
5. Map Analysis (Step 1d)
6. HTTP Status Codes and Return Path Responses (Step 1e)
7. Process Properties Analysis (Steps 2-3)
8. Data Dependency Graph (Step 4)
9. Control Flow Graph (Step 5)
10. Decision Shape Analysis (Step 7)
11. Branch Shape Analysis (Step 8)
12. Execution Order (Step 9)
13. Sequence Diagram (Step 10)
14. Subprocess Analysis
15. Critical Patterns Identified
16. System Layer Identification
17. Request/Response JSON Examples
18. Function Exposure Decision Table
19. Validation Checklist
20. **Process Layer ↔ System Layer Orchestration Diagram** ⚠️ **NEW MANDATORY SECTION**

**🛑 ENFORCEMENT CHECKPOINT:**

Before declaring Phase 1 complete, you MUST verify:
- [ ] Section 20 (Orchestration Diagram) exists
- [ ] Section 20 has all 11 subsections (20.1-20.11)
- [ ] All System Layer Functions from Section 18 shown in orchestration diagram
- [ ] All business decisions from Section 10 assigned to a layer
- [ ] Authentication flow shown (if applicable)
- [ ] Error handling flows shown for all classifications
- [ ] Data flow diagram present
- [ ] Decision ownership matrix complete
- [ ] Layer responsibilities specific to this process
- [ ] Reference mapping complete

**IF ANY MISSING → Section 20 NOT complete → Phase 1 NOT complete → CANNOT PROCEED TO PHASE 2**

---

### 20.9 SELF-CHECK QUESTIONS (MANDATORY)

**Before declaring Section 20 complete, answer these questions:**

1. ❓ Did I show ALL System Layer Functions from Section 18? (Answer: YES/NO)
   - **REQUIRED OUTPUT:** Show answer in Phase 1 Section 20: "✅ All System Layer Functions shown: YES"
   - If NO → **STOP** → Add missing functions → Then proceed

2. ❓ Did I assign ALL decisions from Section 10 to a layer? (Answer: YES/NO)
   - **REQUIRED OUTPUT:** Show answer in Phase 1 Section 20: "✅ All decisions assigned: YES"
   - If NO → **STOP** → Complete decision ownership matrix → Then proceed

3. ❓ Did I show authentication flow (if session/token-based)? (Answer: YES/NO/N/A)
   - **REQUIRED OUTPUT:** Show answer in Phase 1 Section 20: "✅ Authentication flow shown: YES/N/A"
   - If NO and authentication exists → **STOP** → Add authentication flow → Then proceed

4. ❓ Did I show error handling for ALL operation classifications? (Answer: YES/NO)
   - **REQUIRED OUTPUT:** Show answer in Phase 1 Section 20: "✅ Error handling scenarios complete: YES"
   - If NO → **STOP** → Add missing error scenarios → Then proceed

5. ❓ Did I show internal orchestration for Handlers with multiple Atomic Handlers? (Answer: YES/NO/N/A)
   - **REQUIRED OUTPUT:** Show answer in Phase 1 Section 20: "✅ Internal orchestration shown: YES/N/A"
   - If NO and applicable → **STOP** → Add internal orchestration diagrams → Then proceed

6. ❓ Did I create decision ownership matrix with rationale? (Answer: YES/NO)
   - **REQUIRED OUTPUT:** Show answer in Phase 1 Section 20: "✅ Decision ownership matrix complete: YES"
   - If NO → **STOP** → Complete matrix with rationale → Then proceed

7. ❓ Did I document layer responsibilities specific to THIS process? (Answer: YES/NO)
   - **REQUIRED OUTPUT:** Show answer in Phase 1 Section 20: "✅ Layer responsibilities documented: YES"
   - If NO → **STOP** → Add specific examples from this process → Then proceed

8. ❓ Did I create reference mapping (Boomi → Azure)? (Answer: YES/NO)
   - **REQUIRED OUTPUT:** Show answer in Phase 1 Section 20: "✅ Reference mapping complete: YES"
   - If NO → **STOP** → Complete reference mapping → Then proceed

**🛑 IF ANY ANSWER IS NO (and not N/A) → Section 20 NOT complete → STOP → Complete missing items → Then proceed**

---

### 20.10 ORCHESTRATION DIAGRAM QUALITY CRITERIA

**Section 20 is considered COMPLETE when:**

1. ✅ **Completeness:** All System Layer Functions shown, all decisions assigned, all patterns documented
2. ✅ **Clarity:** Diagrams are clear and easy to understand (ASCII format with proper indentation)
3. ✅ **Accuracy:** Diagrams match Sections 10, 13, 15, 18 exactly (no contradictions)
4. ✅ **Specificity:** Examples are specific to THIS process (not generic descriptions)
5. ✅ **Traceability:** All diagrams reference source sections (Section 10, 13.1, 13.2, etc.)
6. ✅ **Usability:** Process Layer developers can understand how to call System Layer APIs
7. ✅ **Consistency:** Decision ownership consistent with API-Led Architecture principles

**Quality Checklist:**
- [ ] Can a Process Layer developer understand the complete orchestration flow?
- [ ] Are all business decisions clearly assigned to Process Layer or System Layer?
- [ ] Is authentication flow clear (if applicable)?
- [ ] Are error handling patterns clear for each operation?
- [ ] Is internal orchestration clear (if applicable)?
- [ ] Are layer boundaries clear (what each layer does/doesn't do)?
- [ ] Are benefits of architecture explained with examples?

**IF ANY CHECKLIST ITEM IS NO → Section 20 needs improvement → Revise → Then proceed**

---

### 20.11 ORCHESTRATION DIAGRAM TEMPLATE (COPY THIS)

**Use this template when creating Section 20:**

```markdown
## 20. Process Layer ↔ System Layer Orchestration Diagram

### 20.1 Overview

This diagram shows how Process Layer orchestrates System Layer APIs to implement the complete business workflow extracted from the Boomi process.

**Key Principle:** System Layer provides atomic "Lego block" APIs. Process Layer orchestrates them based on business logic.

**Based on:**
- Section 10 (Decision Analysis): [List key decisions]
- Section 13.1 (Operation Classification): [List classifications]
- Section 13.2 (Sequence Diagram): [Reference execution flow]
- Section 15 (Critical Patterns): [List patterns]
- Section 18 (Function Exposure Decision): [N] Functions + [M] Atomic Handlers

---

### 20.2 Complete Orchestration Flow

[ASCII diagram showing:]
- Process Layer function entry
- Loop through data (if batch processing)
- Business Decision 1: [From Section 10]
  - Call System Layer API #1
  - Evaluate response
  - Decide next step
- Business Decision 2: [From Section 10]
  - Call System Layer API #2
  - Evaluate response
  - Decide next step
- [Repeat for all decisions and API calls]
- Return aggregated results

---

### 20.3 Operation-Level Orchestration

#### Operation 1: [Function Name from Section 18]

[ASCII diagram showing:]
- Process Layer call with request structure
- System Layer Function entry
- Middleware authentication (if applicable)
- Service → Handler → Atomic Handler flow
- SOAP/REST call to SOR
- Response deserialization
- Return to Process Layer
- Process Layer decision based on response

[Repeat for EACH System Layer Function]

---

### 20.4 System Layer Internal Orchestration

[For EACH Handler that orchestrates multiple Atomic Handlers:]

#### [Handler Name] Internal Flow

[ASCII diagram showing:]
- Handler entry
- STEP 1: [Internal Operation 1] ([Classification from Section 13.1])
  - Error handling: [From Section 13.1]
  - Result: [Variables set]
- STEP 2: [Internal Operation 2] ([Classification from Section 13.1])
  - Error handling: [From Section 13.1]
  - Result: [Variables set]
- STEP 3: [Main Operation] ([Classification from Section 13.1])
  - Uses: Results from STEP 1, 2 (may be empty)
  - Error handling: [From Section 13.1]
  - Result: [Variables set]

---

### 20.5 Authentication Flow

[If session/token-based authentication:]

[ASCII diagram showing:]
- Process Layer calls System Layer
- CustomAuthenticationMiddleware intercepts
- BEFORE: Login → Extract SessionId/Token → Store in RequestContext
- EXECUTE: Function uses RequestContext.GetSessionId()
- AFTER (finally): Logout → Clear RequestContext
- Return to Process Layer

---

### 20.6 Error Handling Flows

#### Scenario 1: Authentication Failure
[Diagram]

#### Scenario 2: Best-Effort Lookup Failure
[Diagram]

#### Scenario 3: Main Operation Failure
[Diagram]

[Include scenario for EACH operation classification from Section 13.1]

---

### 20.7 Data Flow Diagram

[ASCII diagram showing:]
- Experience Layer
- Process Layer
- System Layer
- System of Record
- Shared Services (KeyVault, Redis, App Insights)
- Protocols (HTTPS, SOAP/REST)
- Data flow direction

---

### 20.8 Decision Ownership Matrix

| Decision Point | Owner | Rationale | Implementation |
|---|---|---|---|
| [Decision from Section 10] | Process/System Layer | [Why] | [How] |

[Include ALL decisions from Section 10]

---

### 20.9 Layer Responsibilities Summary

#### Process Layer Responsibilities

**What Process Layer DOES:**
- [List specific to this process]

**What Process Layer DOES NOT DO:**
- [List specific to this process]

#### System Layer Responsibilities

**What System Layer DOES:**
- [List specific to this process]

**What System Layer DOES NOT DO:**
- [List specific to this process]

---

### 20.10 Benefits of This Architecture

#### 1. Separation of Concerns
[Explain with examples from this process]

#### 2. Reusability
[Show how System Layer APIs can be reused]

#### 3. Maintainability
[Show change scenarios and impact isolation]

#### 4. Testability
[Show how each layer can be tested independently]

---

### 20.11 Reference Mapping

#### Boomi Shapes → Azure Components

| Boomi Component | Azure Component | Layer | File |
|---|---|---|---|
| shape[N] ([Operation]) | [Component Name] | System/Process | [File path] |

#### Phase 1 Sections → Code Components

| Phase 1 Section | Code Component | File |
|---|---|---|
| Section [N] ([Title]) | [Component Name] | [File path] |

---

### 20.12 Self-Check Results

✅ All System Layer Functions shown: YES  
✅ All decisions assigned: YES  
✅ Authentication flow shown: YES/N/A  
✅ Error handling scenarios complete: YES  
✅ Internal orchestration shown: YES/N/A  
✅ Decision ownership matrix complete: YES  
✅ Layer responsibilities documented: YES  
✅ Reference mapping complete: YES

**Section 20 Status:** ✅ COMPLETE

---

**END OF SECTION 20**
```

---

### 20.12 PHASE 1 COMPLETION VALIDATION (UPDATED)

**🛑 EXPLICIT ENFORCEMENT - FINAL GATE:**

**YOU CANNOT PROCEED TO PHASE 2 (CODE GENERATION)** until ALL of the following are complete:

**Input/Output Analysis:**
- [ ] Step 1a (Input Structure Analysis) - COMPLETE and DOCUMENTED
- [ ] Step 1b (Response Structure Analysis) - COMPLETE and DOCUMENTED
- [ ] Step 1c (Operation Response Analysis) - COMPLETE and DOCUMENTED
- [ ] Step 1d (Map Analysis) - COMPLETE and DOCUMENTED
- [ ] Step 1e (HTTP Status Codes and Return Path Responses) - COMPLETE and DOCUMENTED

**Process Flow Analysis:**
- [ ] Step 2 (Property Writes) - COMPLETE and DOCUMENTED
- [ ] Step 3 (Property Reads) - COMPLETE and DOCUMENTED
- [ ] Step 4 (Data Dependency Graph) - COMPLETE and DOCUMENTED
- [ ] Step 5 (Control Flow Graph) - COMPLETE and DOCUMENTED
- [ ] Step 6 (Reverse Flow Mapping) - COMPLETE and DOCUMENTED
- [ ] Step 7 (Decision Analysis) - COMPLETE and DOCUMENTED
- [ ] Step 8 (Branch Analysis) - COMPLETE and DOCUMENTED
- [ ] Step 9 (Execution Order) - COMPLETE and DOCUMENTED
- [ ] Step 10 (Sequence Diagram) - COMPLETE and DOCUMENTED

**Contract Verification:**
- [ ] Section 13 (Input Structure Analysis) - COMPLETE
- [ ] Section 14 (Field Mapping Analysis) - COMPLETE
- [ ] Section 15 (Map Analysis) - COMPLETE
- [ ] Section 16 (HTTP Status Codes and Return Path Responses) - COMPLETE
- [ ] Section 17 (Request/Response JSON Examples) - COMPLETE
- [ ] Section 18 (Function Exposure Decision Table) - COMPLETE

**Orchestration Diagram (NEW - MANDATORY):**
- [ ] **Section 20 (Orchestration Diagram) - COMPLETE** ⚠️ **NEW MANDATORY**
- [ ] Section 20.1 (Overview) - COMPLETE
- [ ] Section 20.2 (Complete Orchestration Flow) - COMPLETE
- [ ] Section 20.3 (Operation-Level Orchestration) - COMPLETE
- [ ] Section 20.4 (System Layer Internal Orchestration) - COMPLETE (if applicable)
- [ ] Section 20.5 (Authentication Flow) - COMPLETE (if applicable)
- [ ] Section 20.6 (Error Handling Flows) - COMPLETE
- [ ] Section 20.7 (Data Flow Diagram) - COMPLETE
- [ ] Section 20.8 (Decision Ownership Matrix) - COMPLETE
- [ ] Section 20.9 (Layer Responsibilities Summary) - COMPLETE
- [ ] Section 20.10 (Benefits of This Architecture) - COMPLETE
- [ ] Section 20.11 (Reference Mapping) - COMPLETE
- [ ] Section 20.12 (Self-Check Results) - ALL YES (or N/A)

**Self-Check Questions (Section 20):**

1. ❓ Did I show ALL System Layer Functions from Section 18? (Answer: YES/NO)
   - **REQUIRED:** YES
   - If NO → **STOP** → Add missing functions → Document in Section 20

2. ❓ Did I assign ALL decisions from Section 10 to a layer? (Answer: YES/NO)
   - **REQUIRED:** YES
   - If NO → **STOP** → Complete decision ownership matrix → Document in Section 20

3. ❓ Did I show authentication flow (if applicable)? (Answer: YES/NO/N/A)
   - **REQUIRED:** YES (if auth exists) or N/A (if no auth)
   - If NO and auth exists → **STOP** → Add authentication flow → Document in Section 20

4. ❓ Did I show error handling for ALL classifications? (Answer: YES/NO)
   - **REQUIRED:** YES
   - If NO → **STOP** → Add missing error scenarios → Document in Section 20

5. ❓ Did I show internal orchestration (if applicable)? (Answer: YES/NO/N/A)
   - **REQUIRED:** YES (if Handler orchestrates multiple Atomic Handlers) or N/A
   - If NO and applicable → **STOP** → Add internal orchestration → Document in Section 20

6. ❓ Did I document layer responsibilities specific to THIS process? (Answer: YES/NO)
   - **REQUIRED:** YES
   - If NO → **STOP** → Add specific examples → Document in Section 20

7. ❓ Did I create reference mapping (Boomi → Azure)? (Answer: YES/NO)
   - **REQUIRED:** YES
   - If NO → **STOP** → Complete reference mapping → Document in Section 20

8. ❓ Can a Process Layer developer understand how to call System Layer APIs? (Answer: YES/NO)
   - **REQUIRED:** YES
   - If NO → **STOP** → Improve clarity of orchestration diagrams → Document in Section 20

**🛑 IF ANY ANSWER IS NO (and not N/A) → Section 20 NOT complete → Phase 1 NOT complete → CANNOT PROCEED TO PHASE 2**

---

### 20.13 WHY ORCHESTRATION DIAGRAM IS MANDATORY

**Problem Without Orchestration Diagram:**
- ❌ Process Layer developers don't know how to call System Layer APIs
- ❌ Unclear which decisions belong to which layer
- ❌ Unclear how to handle errors from System Layer
- ❌ Unclear what each layer is responsible for
- ❌ Difficult to understand API-Led Architecture implementation
- ❌ Risk of implementing business logic in wrong layer

**Solution With Orchestration Diagram:**
- ✅ Clear visual representation of layer interaction
- ✅ Decision ownership explicitly assigned
- ✅ Error handling patterns documented
- ✅ Layer responsibilities clearly defined
- ✅ API-Led Architecture principles demonstrated
- ✅ Process Layer implementation guide provided

**Impact:**
- **Without Section 20:** Process Layer developers must guess how to orchestrate System Layer APIs
- **With Section 20:** Process Layer developers have clear blueprint for implementation

---

### 20.14 ORCHESTRATION DIAGRAM EXAMPLES BY PATTERN

#### Pattern 1: Simple Sequential (No Decisions)

**Boomi:** Op1 → Op2 → Op3 (sequential, no branches, no decisions)

**Orchestration Diagram:**
```
Process Layer: Sequential Execution
  ├─→ Call System Layer: Op1 API
  ├─→ Call System Layer: Op2 API
  └─→ Call System Layer: Op3 API

Decision Ownership: None (no decisions)
```

#### Pattern 2: Check-Before-Create

**Boomi:** GetEntity → Decision (exists?) → If not exists, CreateEntity

**Orchestration Diagram:**
```
Process Layer: Check-Before-Create Pattern
  ├─→ DECISION 1: Check Existence
  │   └─→ Call System Layer: GetEntity API
  │   └─→ Evaluate: if (exists) return, else proceed
  │
  └─→ DECISION 2: Create Entity
      └─→ Call System Layer: CreateEntity API

Decision Ownership:
- Decision 1 (Check existence): Process Layer (cross-operation)
- Decision 2 (Create entity): Process Layer (based on Decision 1)
```

#### Pattern 3: Conditional Operation

**Boomi:** CreateEntity → Decision (flag?) → If flag=Y, LinkRelated

**Orchestration Diagram:**
```
Process Layer: Conditional Operation Pattern
  ├─→ Call System Layer: CreateEntity API
  │   └─→ Store: createdEntityId
  │
  └─→ DECISION: Check Flag
      ├─→ If flag = "Y": Call System Layer: LinkRelated API
      └─→ If flag ≠ "Y": Skip linking

Decision Ownership:
- Decision (Check flag): Process Layer (conditional operation)
```

#### Pattern 4: Best-Effort Lookups with Main Operation

**Boomi:** Branch (GetTypeId, GetCategoryId) → Converge → CreateEntity

**Orchestration Diagram:**
```
Process Layer: Single API Call
  └─→ Call System Layer: CreateEntity API

System Layer: Internal Orchestration (Same SOR)
  ├─→ STEP 1: GetTypeId (BEST-EFFORT)
  │   └─→ If fail: Log warning, typeId = empty, CONTINUE
  ├─→ STEP 2: GetCategoryId (BEST-EFFORT)
  │   └─→ If fail: Log warning, categoryId = empty, CONTINUE
  └─→ STEP 3: CreateEntity (MAIN OPERATION)
      └─→ Uses: typeId, categoryId (may be empty)
      └─→ If fail: Throw exception

Decision Ownership:
- Internal lookup error handling: System Layer (same-SOR, best-effort)
- Main operation error handling: System Layer (throws exception)

Key Point: Process Layer sees single API call. System Layer handles internal orchestration.
```

#### Pattern 5: Batch Processing with Decisions

**Boomi:** Array input → For each element: Check → Decision → Create

**Orchestration Diagram:**
```
Process Layer: Batch Processing Pattern
  │
  │ Receive array of items
  │
  ├─→ LOOP: foreach (item in items)
  │   │
  │   ├─→ DECISION 1: Check Existence
  │   │   └─→ Call System Layer: CheckEntity API
  │   │   └─→ if (exists) skip, else proceed
  │   │
  │   ├─→ DECISION 2: Create Entity
  │   │   └─→ Call System Layer: CreateEntity API
  │   │
  │   └─→ Add to results (success or skip)
  │
  └─→ Return aggregated results

Decision Ownership:
- Loop through array: Process Layer (batch processing)
- Decision 1 (Check existence): Process Layer (cross-operation)
- Decision 2 (Create entity): Process Layer (based on Decision 1)
```

---

### 20.15 INTEGRATION WITH CODE GENERATION (PHASE 2)

**How Section 20 is Used in Phase 2:**

**Step 1: Generate Process Layer Code (NOT in current task, but documented for future)**
- Read Section 20.2 (Complete Orchestration Flow) → Generate Process Layer function
- Read Section 20.8 (Decision Ownership Matrix) → Implement business decisions
- Read Section 20.3 (Operation-Level Orchestration) → Generate System Layer API calls

**Step 2: Verify System Layer Code**
- Read Section 20.3 (Operation-Level Orchestration) → Verify System Layer Functions match
- Read Section 20.4 (System Layer Internal Orchestration) → Verify Handler orchestration
- Read Section 20.5 (Authentication Flow) → Verify middleware implementation

**Step 3: Verify Layer Boundaries**
- Read Section 20.9 (Layer Responsibilities) → Verify no business logic in System Layer
- Read Section 20.8 (Decision Ownership Matrix) → Verify decisions in correct layer

**Key Point:** Section 20 serves as blueprint for BOTH Process Layer and System Layer implementation.

---

### 20.16 VALIDATION ALGORITHM

**Execute this algorithm before declaring Phase 1 complete:**

```
STEP 1: Verify Section 20 Exists
─────────────────────────────────
IF Phase 1 document does NOT contain "## 20. Process Layer ↔ System Layer Orchestration Diagram":
    ERROR: "Section 20 missing"
    ACTION: Create Section 20 using template
    STOP: Cannot proceed to Phase 2

STEP 2: Verify All Subsections Present
────────────────────────────────────────
required_subsections = [
    "20.1 Overview",
    "20.2 Complete Orchestration Flow",
    "20.3 Operation-Level Orchestration",
    "20.4 System Layer Internal Orchestration",
    "20.5 Authentication Flow",
    "20.6 Error Handling Flows",
    "20.7 Data Flow Diagram",
    "20.8 Decision Ownership Matrix",
    "20.9 Layer Responsibilities Summary",
    "20.10 Benefits of This Architecture",
    "20.11 Reference Mapping",
    "20.12 Self-Check Results"
]

FOR EACH subsection in required_subsections:
    IF subsection NOT in Section 20:
        IF subsection is conditional (20.4, 20.5) AND not applicable:
            MARK: N/A
        ELSE:
            ERROR: f"Subsection {subsection} missing"
            ACTION: Add missing subsection
            STOP: Cannot proceed to Phase 2

STEP 3: Verify System Layer Functions Coverage
────────────────────────────────────────────────
functions_in_section_18 = Extract from Section 18
functions_in_section_20_3 = Extract from Section 20.3

IF functions_in_section_18 != functions_in_section_20_3:
    ERROR: "Section 20.3 missing System Layer Functions"
    ACTION: Add missing operation-level diagrams
    STOP: Cannot proceed to Phase 2

STEP 4: Verify Decision Coverage
──────────────────────────────────
decisions_in_section_10 = Extract from Section 10
decisions_in_section_20_8 = Extract from Section 20.8

IF decisions_in_section_10 != decisions_in_section_20_8:
    ERROR: "Section 20.8 missing decision assignments"
    ACTION: Add missing decisions to matrix
    STOP: Cannot proceed to Phase 2

STEP 5: Verify Self-Check Results
───────────────────────────────────
self_check_results = Extract from Section 20.12

FOR EACH question in self_check_results:
    IF answer is NO (and not N/A):
        ERROR: f"Self-check question {question} answered NO"
        ACTION: Complete missing item
        STOP: Cannot proceed to Phase 2

STEP 6: Final Validation
─────────────────────────
IF all validations pass:
    Phase 1 Status: COMPLETE
    Can proceed to Phase 2: YES
ELSE:
    Phase 1 Status: INCOMPLETE
    Can proceed to Phase 2: NO
    ACTION: Complete missing items, re-run validation
```

---

### 20.17 SUMMARY OF CHANGES TO BOOMI_EXTRACTION_RULES.mdc

**Version 2.6 Changes:**

1. ✅ **Added Section 20 (Orchestration Diagram) as MANDATORY**
   - Must be created AFTER Section 18 is complete
   - Must be created BEFORE Phase 2 code generation
   - 12 mandatory subsections (20.1-20.12)

2. ✅ **Added Step 11 (Create Orchestration Diagram) to workflow**
   - Executes after Step 10 (Sequence Diagram)
   - Generates Section 20 in Phase 1 document

3. ✅ **Updated Phase 1 Completion Validation**
   - Added Section 20 to completion checklist
   - Added self-check questions for Section 20
   - Added validation algorithm for Section 20

4. ✅ **Added Orchestration Diagram Template**
   - Complete template for Section 20
   - Examples for common patterns
   - Integration with code generation

5. ✅ **Added Quality Criteria**
   - Completeness, clarity, accuracy, specificity, traceability, usability, consistency
   - Quality checklist for Section 20

**Impact:**
- Phase 1 documents will now include orchestration diagrams
- Process Layer developers have clear blueprint for implementation
- Decision ownership explicitly documented
- Layer boundaries clearly defined

---

### 20.18 ENFORCEMENT SUMMARY

**🚨 CRITICAL ENFORCEMENT POINTS:**

1. **Section 20 is MANDATORY** - Cannot proceed to Phase 2 without it
2. **Create AFTER Section 18** - Must know which operations are Functions first
3. **All 12 subsections required** - Cannot skip any subsection (except 20.4, 20.5 if N/A)
4. **All self-checks must be YES** - Cannot proceed if any answer is NO (except N/A)
5. **Must reference source sections** - Section 20 must reference Sections 10, 13, 15, 18
6. **Must be specific to this process** - Not generic descriptions, actual examples from this process
7. **Must show decision ownership** - ALL decisions from Section 10 assigned to a layer

**Validation Gate:**
```
IF Section 20 NOT complete:
    Phase 1 Status: INCOMPLETE
    Can proceed to Phase 2: NO
    Action Required: Complete Section 20
ELSE:
    Phase 1 Status: COMPLETE
    Can proceed to Phase 2: YES
```

---

**END OF ORCHESTRATION DIAGRAM RULES**

**Add this section to BOOMI_EXTRACTION_RULES.mdc AFTER "README.md GENERATION CHECKLIST" section and BEFORE "Document Version" footer.**

**Update Document Version footer to:**
```
**Document Version:** 2.6  
**Purpose:** Prescriptive guide for extracting execution sequence from ANY Boomi process  
**Key Changes:** 
- Version 2.0: Made rules explicit, mandatory, and non-negotiable
- Version 2.1: Added mandatory Input/Output Structure Analysis (Contract Verification)
- Version 2.2: Added mandatory Map Analysis (Step 1d) - Critical for SOAP envelope field name verification
- Version 2.3: Added mandatory HTTP Status Codes and Return Path Responses (Step 1e) - Critical for error handling and response mapping; Added Request/Response JSON Examples section
- Version 2.4: Added README.md Generation rules (Post-Phase 2) - Critical for documenting error handling behavior with operation classification system
- Version 2.5: **MAJOR CHANGE** - Moved ALL technical details to Phase 1 Section 13. README.md is now a simple user guide (150-250 lines). Sequence diagrams with operation classification, error handling, and Boomi references are ONLY in Phase 1. README references Phase 1 for technical details. This eliminates duplication and makes Phase 1 the single source of truth for code generation.
- Version 2.6: **MANDATORY ORCHESTRATION DIAGRAM** - Added Section 20 (Process Layer ↔ System Layer Orchestration Diagram) as mandatory section in Phase 1. Shows how Process Layer orchestrates System Layer APIs, documents decision ownership, clarifies layer responsibilities. Includes 12 mandatory subsections with complete orchestration flows, operation-level diagrams, authentication flow, error handling scenarios, data flow diagram, decision ownership matrix, and reference mapping. Phase 1 is NOT complete without Section 20.
```
