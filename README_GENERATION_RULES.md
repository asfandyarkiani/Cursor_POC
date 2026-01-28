# README GENERATION RULES (For BOOMI_EXTRACTION_RULES.mdc)

**Purpose:** Standardized README.md generation for System Layer projects  
**When:** After Phase 2 (Code Generation) is complete  
**Source:** BOOMI_EXTRACTION_PHASE1.md (Section 13: Sequence Diagram)

---

## MANDATORY README.md GENERATION (PHASE 2 - FINAL STEP)

**🛑 CRITICAL:** README.md MUST be generated as the FINAL step of Phase 2, AFTER all code is complete and committed.

**Source of Truth:** BOOMI_EXTRACTION_PHASE1.md Section 13 (Sequence Diagram)

**Purpose:** Document the implemented System Layer for Process Layer consumers and operations teams.

---

## SECTION 1: TITLE & METADATA (MANDATORY)

**Format:**
```markdown
# [ProjectName] - System Layer

**System of Record:** [SOR_NAME] ([VENDOR_NAME])  
**Integration Type:** [REST/SOAP/FTP/SFTP/DATABASE/SMTP]  
**Authentication:** [Session-based/Token-based/Credentials-per-request/API-Key/OAuth2/None]  
**Framework:** .NET 8, Azure Functions v4
```

**Rules:**
- Title format: `[ProjectName] - System Layer` (MANDATORY suffix)
- SOR: Full name + vendor in parentheses
- Integration Type: Specify protocol/method
- Authentication: Specify method + how it's handled (middleware/per-request)
- Framework: Always ".NET 8, Azure Functions v4"

**Example:**
```markdown
# CAFM Management System - System Layer

**System of Record:** CAFM (Computer-Aided Facility Management) - FSI Concept  
**Integration Type:** SOAP/XML over HTTP  
**Authentication:** Session-based (Login/Logout via middleware)  
**Framework:** .NET 8, Azure Functions v4
```

---

## SECTION 2: OVERVIEW (MANDATORY)

**Format:**
```markdown
## OVERVIEW

[1-2 sentence description of what this System Layer does]

**Key Operations:**
1. **[Operation1]** - [Purpose/description]
2. **[Operation2]** - [Purpose/description]
[List ALL Azure Functions exposed to Process Layer]
```

**Rules:**
- Brief description (1-2 sentences)
- List ALL Azure Functions (not internal atomic handlers)
- Use bold for operation names
- One-line purpose per operation

---

## SECTION 3: ARCHITECTURE (MANDATORY)

**Format:**
```markdown
## ARCHITECTURE

### API-Led Architecture Layers

```
┌─────────────────────────────────────────────────────────────┐
│ PROCESS LAYER (Orchestration)                               │
│ - Business logic and workflows                              │
│ - Orchestrates multiple System Layer APIs                   │
│ - Implements [key business pattern from Phase 1]            │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│ SYSTEM LAYER (This Project)                                 │
│ - Unlocks data from [SOR_NAME] system                       │
│ - Handles [SOR]-specific authentication ([auth_type])       │
│ - Transforms data to/from [format] format                   │
│ - Exposes atomic operations as Azure Functions              │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│ [SOR_NAME] SYSTEM - System of Record                        │
│ - [Key capability 1]                                         │
│ - [Key capability 2]                                         │
│ - [Key capability 3]                                         │
└─────────────────────────────────────────────────────────────┘
```
```

**Rules:**
- Use ASCII box drawing (┌─┐│└┘)
- Three layers: Process → System → SOR
- Process Layer: List business patterns (check-before-create, etc.)
- System Layer: List SOR-specific responsibilities
- SOR: List key capabilities

---

## SECTION 4: SEQUENCE DIAGRAM (MANDATORY - CRITICAL)

**🚨 MOST IMPORTANT SECTION - MUST BE ACCURATE**

**Source:** BOOMI_EXTRACTION_PHASE1.md Section 13 (Sequence Diagram)

**Format:** ONE sequence diagram per Azure Function

### Template for Each Function

```markdown
### [OperationName] Operation

**📋 BASED ON:** BOOMI_EXTRACTION_PHASE1.md Section 13 (Sequence Diagram)  
**Error Handling:** Derived from Boomi [pattern name] (shape references)

```
Process Layer
     |
     | [HTTP_METHOD] [route]
     | [request JSON example]
     ↓
[OperationName]API (Function)
     |
     | [Authentication attribute if applicable]
     ↓
[AuthenticationMiddleware if applicable]
     |
     ├─→ [AuthenticateAtomicHandler if applicable]
     |    └─→ [Protocol]: [Operation] ([parameters])
     |    └─→ Response: [fields]
     |    └─→ Store in RequestContext
     |
     ↓ Continue to function
     |
I[Domain]Mgmt (Service Interface)
     ↓
[Domain]MgmtService
     ↓
[OperationName]Handler
     |
     ├─→ Read [context_data] from RequestContext
     |
     ├─→ STEP 1: [AtomicHandler1] (OPERATION_CLASSIFICATION)
     |    └─→ [Protocol]: [Operation] ([parameters])
     |    └─→ Response: [fields]
     |    └─→ Error Handling: If fails → [Log warning/Throw exception], [set empty/stop], [CONTINUE/STOP]
     |    └─→ Result: [variables] (populated or empty / throws exception)
     |
     ├─→ STEP 2: [AtomicHandler2] (OPERATION_CLASSIFICATION)
     |    └─→ [Protocol]: [Operation] ([parameters])
     |    └─→ Response: [fields]
     |    └─→ Error Handling: If fails → [action]
     |    └─→ Result: [variables] (populated or empty / throws exception)
     |
     [Repeat for ALL atomic handlers]
     |
     ├─→ STEP N: Conditional [Operation] (if applicable)
     |    └─→ IF [condition]:
     |         └─→ [AtomicHandler]
     |              └─→ [Protocol]: [Operation]
     |              └─→ Response: [fields]
     |              └─→ Error Handling: [action]
     |    └─→ ELSE:
     |         └─→ Skip [operation]
     |
     ├─→ Deserialize [protocol] response → [Operation]ApiResDTO
     ├─→ Map ApiResDTO → [Operation]ResDTO
     └─→ Return BaseResponseDTO
     |
     ↓ After function completes
     |
[AuthenticationMiddleware cleanup if applicable]
     |
     └─→ [LogoutAtomicHandler if applicable]
          └─→ Clear RequestContext
     |
     ↓
Response to Process Layer
[response JSON example]
```
```

### OPERATION_CLASSIFICATION Values

**MANDATORY:** Specify classification for EACH atomic handler call:

| Classification | When to Use | Error Handling |
|---|---|---|
| `(AUTHENTICATION)` | Login/auth operations | Throw exception (required for all ops) |
| `(BEST-EFFORT LOOKUP)` | Lookup/enrichment operations that can fail | Log warning, set empty, CONTINUE |
| `(MAIN OPERATION)` | Primary business operation | Throw exception (operation must succeed) |
| `(CONDITIONAL)` | Operations executed conditionally | Log warning if fails, CONTINUE |
| `(CLEANUP)` | Logout/cleanup operations | Log error, CONTINUE (non-critical) |

### Error Handling Specification (MANDATORY)

**For EACH atomic handler, specify:**

**Pattern 1: BEST-EFFORT (Continue on Failure)**
```
└─→ Error Handling: If fails → Log warning, set empty values, CONTINUE
└─→ Result: [variables] (populated or empty)
```

**Pattern 2: MUST-SUCCEED (Throw on Failure)**
```
└─→ Error Handling: If fails → Throw exception (operation must succeed)
└─→ Result: [variables] (throws exception on failure)
```

**Pattern 3: CONDITIONAL (Non-Critical)**
```
└─→ Error Handling: If fails → Log warning, CONTINUE (non-critical)
└─→ Result: Operation attempted, may have failed
```

### How to Determine Error Handling from Boomi

**Step 1: Check for Decision Shapes After Operation**
```
IF decision shape exists after operation AND checks status code:
    └─→ Operation has explicit error handling
    └─→ Trace TRUE/FALSE paths to determine behavior
ELSE:
    └─→ Operation continues regardless of result
    └─→ Classification: BEST-EFFORT (continue on failure)
```

**Step 2: Check Branch Convergence**
```
IF operation is in branch path AND paths converge (stop with continue=true):
    └─→ Operation continues to convergence regardless of result
    └─→ Classification: BEST-EFFORT (continue on failure)
ELSE:
    └─→ Check if operation is critical (main operation, auth, etc.)
    └─→ Classification: MAIN OPERATION or AUTHENTICATION (throw on failure)
```

**Step 3: Check Operation Type**
```
IF operation is authentication (Login):
    └─→ Classification: AUTHENTICATION (throw on failure)
ELIF operation is main business operation (Create, Update, Delete):
    └─→ Classification: MAIN OPERATION (throw on failure)
ELIF operation is lookup/enrichment:
    └─→ Classification: BEST-EFFORT LOOKUP (continue on failure)
ELIF operation is cleanup (Logout):
    └─→ Classification: CLEANUP (continue on failure)
```

---

## SECTION 5: FOLDER STRUCTURE (MANDATORY)

**Format:**
```markdown
## FOLDER STRUCTURE

```
[ProjectName]/
├── Abstractions/
│   └── I[Domain]Mgmt.cs                         # Service interface
├── Implementations/[VendorName]/
│   ├── Services/
│   │   └── [Domain]MgmtService.cs               # Service implementation
│   ├── Handlers/
│   │   ├── [Operation1]Handler.cs               # [Description]
│   │   └── [Operation2]Handler.cs               # [Description]
│   └── AtomicHandlers/
│       ├── [Auth]AtomicHandler.cs               # [Description]
│       ├── [Operation1]AtomicHandler.cs         # [Description]
│       └── [Lookup]AtomicHandler.cs             # Internal lookup
├── DTO/
│   ├── [Operation1]DTO/
│   │   ├── [Operation1]ReqDTO.cs
│   │   └── [Operation1]ResDTO.cs
│   ├── AtomicHandlerDTOs/
│   │   └── [...]HandlerReqDTO.cs
│   └── DownstreamDTOs/
│       └── [...]ApiResDTO.cs
├── Functions/
│   └── [Operation]API.cs
├── ConfigModels/
│   ├── AppConfigs.cs
│   └── KeyVaultConfigs.cs
├── Constants/
│   ├── ErrorConstants.cs
│   ├── InfoConstants.cs
│   └── OperationNames.cs
├── Helper/
│   ├── [SOAPHelper.cs / RestApiHelper.cs]
│   ├── [CustomSoapClient.cs / CustomRestClient.cs]
│   ├── KeyVaultReader.cs (if KeyVault)
│   └── RequestContext.cs (if session/token auth)
├── Attributes/ (if session/token auth)
│   └── CustomAuthenticationAttribute.cs
├── Middleware/ (if session/token auth)
│   └── CustomAuthenticationMiddleware.cs
├── SoapEnvelopes/ (if SOAP)
│   └── [Operation].xml
├── Program.cs
├── host.json
├── appsettings.json (placeholder)
├── appsettings.{env}.json
└── [ProjectName].csproj
```
```

**Rules:**
- Show ACTUAL folder structure (not template)
- Include comments for each component
- Mark conditional folders: (if SOAP), (if KeyVault), (if auth)
- Use tree structure with proper indentation

---

## SECTION 6: AZURE FUNCTIONS EXPOSED (MANDATORY)

**Format:** One subsection per Azure Function

```markdown
## AZURE FUNCTIONS EXPOSED

### N. [OperationName]

**Route:** `[METHOD] [route_pattern]`

**Purpose:** [What this function does - 1 sentence]

**Request:**
```json
[Complete request JSON with realistic field names and values]
```

**Response:**
```json
[Complete response JSON with realistic field names and values]
```

**Authentication:** [Auth_type] ([how it's handled])

**Internal Operations:** (if handler orchestrates multiple atomics)
1. [InternalOp1] ([classification] - [error handling strategy])
2. [InternalOp2] ([classification] - [error handling strategy])
3. [MainOp] ([classification] - [error handling strategy])

**Process Layer Usage:** (if applicable)
```
[Step-by-step guide showing how Process Layer uses this function]
```
```

**Rules:**
- List ALL Azure Functions (from Function Exposure Decision Table)
- Use realistic examples (actual field names, values from Phase 1)
- Show complete JSON (not partial)
- Specify internal operations with error handling strategy
- Include Process Layer usage guide for complex patterns (check-before-create, etc.)

---

## SECTION 7-10: AUTHENTICATION, CONFIGURATION, DEPLOYMENT, PROCESS LAYER INTEGRATION

**Format:** Standard sections documenting:
- Authentication flow (if applicable)
- Configuration structure (AppConfigs, KeyVault)
- Deployment process (environment files, CI/CD)
- Process Layer integration patterns

**Rules:**
- Include ONLY if applicable to the project
- Use actual configuration keys (not placeholders)
- Reference Boomi patterns (check-before-create, etc.)

---

## SECTION 11: ERROR HANDLING STRATEGY (MANDATORY - CRITICAL)

**🚨 CRITICAL SECTION - MUST ACCURATELY REFLECT BOOMI BEHAVIOR**

**Format:**
```markdown
## ERROR HANDLING STRATEGY

### Operation Classification

**MUST-SUCCEED Operations (Throw on Failure):**

| Operation | Reason | Action on Failure |
|---|---|---|
| [Op1] | [Why must succeed] | Throw exception ([where]) |
| [Op2] | [Why must succeed] | Throw exception ([where]) |

**BEST-EFFORT Operations (Continue on Failure):**

| Operation | Reason | Action on Failure |
|---|---|---|
| [Op1] | [Why best-effort] | Log warning, set empty, continue |
| [Op2] | [Why best-effort] | Log warning, set empty, continue |

### Rationale: [Boomi Pattern Name]

**From Boomi Analysis:**
- [Key finding 1 from BOOMI_EXTRACTION_PHASE1.md]
- [Key finding 2 - reference shape numbers]
- [Key finding 3 - reference decision shapes or convergence]

**Azure Implementation:**
```csharp
// [Pattern description]
[Code example showing the pattern - 10-15 lines]
```

**Benefits:**
- ✅ [Benefit 1]
- ✅ [Benefit 2]
- ✅ [Benefit 3]
```

**Rules:**
- Classify ALL operations (MUST-SUCCEED vs BEST-EFFORT)
- Reference Boomi analysis (shape numbers, patterns)
- Provide code example showing implementation
- Explain rationale (branch convergence, decision shapes, etc.)

**How to Classify Operations:**

**MUST-SUCCEED (Throw on Failure):**
1. Authentication operations (Login) - Required for all operations
2. Main business operations (Create, Update, Delete) - Primary purpose of function
3. Operations with decision shapes checking status codes - Explicit error handling in Boomi

**BEST-EFFORT (Continue on Failure):**
1. Lookup/enrichment operations in branch paths with convergence - No decision shapes after operation
2. Operations where branch paths converge (stop with continue=true) - All paths complete regardless of results
3. Optional operations (conditional execution, non-critical)

**CLEANUP (Continue on Failure):**
1. Logout operations - Cleanup only, non-critical
2. Notification operations - Best-effort, don't fail process

---

## SECTION 12: ERROR HANDLING (MANDATORY)

**Format:**
```markdown
## ERROR HANDLING

### Exception Types

| Exception | HTTP Status | When |
|---|---|---|
| NoRequestBodyException | 400 | Request body is null/empty |
| RequestValidationFailureException | 400 | Request validation fails |
| DownStreamApiFailureException | Varies | [SOR] API call fails |
| NoResponseBodyException | 500 | [SOR] returns empty response |
| BaseException | 500 | [Context-specific error] |

### Error Response Format

```json
{
  "message": "[Error message]",
  "errorCode": "[ERROR_CODE]",
  "data": null,
  "errorDetails": {
    "errors": [
      {
        "stepName": "[ClassName.cs / MethodName]",
        "stepError": "[Detailed error message]"
      }
    ]
  }
}
```

### Error Codes

| Error Code | Message |
|---|---|
| [XXX_YYYYYY_0001] | [Error message] |
[List ALL error codes from Constants/ErrorConstants.cs]
```

**Rules:**
- List ALL exception types used in the project
- Show complete error response format (BaseResponseDTO structure)
- List ALL error codes from ErrorConstants.cs
- Use actual error code format (AAA_AAAAAA_DDDD for System Layer)

---

## SECTIONS 13-19: SUPPORTING SECTIONS (MANDATORY)

**Required Sections:**
- **PERFORMANCE** - Timing tracking, example breakdown
- **DEVELOPMENT** - Prerequisites, build, run, test (with curl examples)
- **DEPENDENCIES** - Framework projects, NuGet packages
- **MONITORING** - Application Insights, Live Metrics
- **FUTURE ENHANCEMENTS** - Based on TODO placeholders, missing features
- **SUPPORT** - Documentation links (Phase 1, Compliance Report, Rulebooks)
- **VERSION & STATUS** - Version number, date, status

**Rules:**
- Include ALL sections (even if brief)
- Use actual values (not placeholders)
- Reference actual files (BOOMI_EXTRACTION_PHASE1.md, etc.)

---

## CRITICAL RULES FOR SEQUENCE DIAGRAMS

### Rule 1: Show ALL Components (MANDATORY)

**MUST include:**
- Process Layer (caller)
- Azure Function (entry point)
- Middleware (if authentication)
- Service Interface
- Service Implementation
- Handler
- **ALL Atomic Handlers** (with complete details)
- Response back to Process Layer

**❌ DON'T:**
- Skip atomic handlers
- Show only happy path
- Omit error handling
- Use generic descriptions

### Rule 2: Show Error Handling for EACH Atomic Handler (MANDATORY)

**For EACH atomic handler call, MUST specify:**

**Line 1: Operation Classification**
```
├─→ STEP N: [AtomicHandlerName] (OPERATION_CLASSIFICATION)
```

**Line 2-3: Protocol and Response**
```
|    └─→ [Protocol]: [Operation] ([parameters])
|    └─→ Response: [fields]
```

**Line 4: Error Handling (CRITICAL)**
```
|    └─→ Error Handling: If fails → [Log warning/Throw exception], [set empty/stop], [CONTINUE/STOP]
```

**Line 5: Result**
```
|    └─→ Result: [variables] (populated or empty / throws exception)
```

**Example (BEST-EFFORT):**
```
├─→ STEP 1: GetLocationsByDtoAtomicHandler (BEST-EFFORT LOOKUP)
|    └─→ SOAP: GetLocationsByDto (sessionId + propertyName + unitCode)
|    └─→ Response: LocationId, BuildingId
|    └─→ Error Handling: If fails → Log warning, set empty values, CONTINUE
|    └─→ Result: locationId, buildingId (populated or empty)
```

**Example (MAIN OPERATION):**
```
├─→ STEP 3: CreateBreakdownTaskAtomicHandler (MAIN OPERATION)
|    └─→ SOAP: CreateBreakdownTask (sessionId + all fields + lookup IDs)
|    └─→ Uses: locationId, buildingId, instructionId (may be empty if lookups failed)
|    └─→ Response: BreakdownTaskId, Status
|    └─→ Error Handling: If fails → Throw exception (main operation must succeed)
|    └─→ Result: breakdownTaskId (throws exception on failure)
```

### Rule 3: Show Conditional Logic (MANDATORY)

**For conditional operations:**
```
├─→ STEP N: Conditional [Operation]
|    └─→ IF [condition]:
|         └─→ [AtomicHandler]
|              └─→ [Protocol]: [Operation]
|              └─→ Response: [fields]
|              └─→ Error Handling: [action]
|    └─→ ELSE:
|         └─→ Skip [operation]
```

**Rules:**
- Show BOTH IF and ELSE branches
- Specify condition clearly
- Show what happens in each branch

### Rule 4: Reference Boomi Analysis (MANDATORY)

**At top of each sequence diagram:**
```markdown
**📋 BASED ON:** BOOMI_EXTRACTION_PHASE1.md Section 13 (Sequence Diagram)  
**Error Handling:** Derived from Boomi [pattern_name] ([shape_references])
```

**Examples:**
- "Derived from Boomi branch convergence pattern (shape6)"
- "Derived from Boomi check-before-create pattern (decision shape55)"
- "Derived from Boomi conditional execution pattern (decision shape31)"

---

## VALIDATION CHECKLIST (MANDATORY BEFORE FINALIZING)

**Before creating README.md, verify:**

### Phase 1 Verification
- [ ] BOOMI_EXTRACTION_PHASE1.md Section 13 (Sequence Diagram) exists and is complete
- [ ] All decision shapes analyzed (Section 10)
- [ ] All branch shapes analyzed (Section 11)
- [ ] Error handling behavior verified from Boomi JSON (decision shapes, convergence)

### Sequence Diagram Completeness
- [ ] ALL Azure Functions have sequence diagrams
- [ ] ALL atomic handlers shown in sequence diagrams
- [ ] EACH atomic handler has operation classification
- [ ] EACH atomic handler has error handling specification
- [ ] EACH atomic handler has result specification
- [ ] Conditional logic shown with IF/ELSE
- [ ] Boomi pattern references included

### Error Handling Accuracy
- [ ] Operation classification table complete (MUST-SUCCEED vs BEST-EFFORT)
- [ ] Rationale references Boomi analysis (shape numbers, patterns)
- [ ] Code example shows actual implementation
- [ ] Classification matches Boomi behavior (verified from JSON)

### Content Accuracy
- [ ] Request/response examples use actual field names (from Phase 1)
- [ ] Folder structure matches actual project structure
- [ ] Configuration examples show actual config keys
- [ ] Error codes match ErrorConstants.cs
- [ ] All sections present (19 sections)

---

## COMMON MISTAKES TO AVOID

### Mistake 1: Generic Sequence Diagrams

**❌ WRONG:**
```
Handler
 ↓
AtomicHandler
 └─→ API Call
 └─→ Response
```

**✅ CORRECT:**
```
GetLocationsByDtoHandler
 |
 ├─→ STEP 1: GetLocationsByDtoAtomicHandler (BEST-EFFORT LOOKUP)
 |    └─→ SOAP: GetLocationsByDto (sessionId + propertyName + unitCode)
 |    └─→ Response: LocationId, BuildingId
 |    └─→ Error Handling: If fails → Log warning, set empty values, CONTINUE
 |    └─→ Result: locationId, buildingId (populated or empty)
```

### Mistake 2: Missing Error Handling

**❌ WRONG:**
```
├─→ GetLocationsByDtoAtomicHandler
|    └─→ SOAP: GetLocationsByDto
|    └─→ Response: LocationId, BuildingId
```

**✅ CORRECT:**
```
├─→ STEP 1: GetLocationsByDtoAtomicHandler (BEST-EFFORT LOOKUP)
|    └─→ SOAP: GetLocationsByDto (sessionId + propertyName + unitCode)
|    └─→ Response: LocationId, BuildingId
|    └─→ Error Handling: If fails → Log warning, set empty values, CONTINUE
|    └─→ Result: locationId, buildingId (populated or empty)
```

### Mistake 3: Assuming Error Handling Without Verification

**❌ WRONG:**
- Assume all operations throw exceptions on failure
- Don't check Boomi JSON for decision shapes
- Don't verify branch convergence behavior

**✅ CORRECT:**
- Check Boomi JSON for decision shapes after each operation
- Verify branch convergence (stop with continue=true)
- Classify operations based on Boomi behavior
- Document rationale with shape references

### Mistake 4: Creating README Before Code

**❌ WRONG:**
- Create README.md during code generation
- Use README as blueprint for code
- README documents planned implementation (not actual)

**✅ CORRECT:**
- Create README.md AFTER code is complete
- Use BOOMI_EXTRACTION_PHASE1.md as blueprint for code
- README documents actual implementation
- README references Phase 1 analysis

---

## GENERATION WORKFLOW (MANDATORY ORDER)

### Step 1: Verify Phase 1 Complete
- [ ] BOOMI_EXTRACTION_PHASE1.md exists
- [ ] Section 13 (Sequence Diagram) complete
- [ ] All decision shapes analyzed
- [ ] All branch shapes analyzed
- [ ] Error handling behavior verified

### Step 2: Verify Code Complete
- [ ] All code files committed
- [ ] All components implemented
- [ ] Error handling implemented correctly (throw vs continue)

### Step 3: Generate README.md
- [ ] Use Phase 1 Section 13 as source for sequence diagrams
- [ ] Verify error handling from Boomi analysis
- [ ] Classify ALL operations (MUST-SUCCEED vs BEST-EFFORT)
- [ ] Show error handling for EACH atomic handler
- [ ] Include ALL 19 sections

### Step 4: Validate README.md
- [ ] Sequence diagrams match Phase 1 analysis
- [ ] Error handling matches code implementation
- [ ] Operation classification verified from Boomi JSON
- [ ] All sections complete

### Step 5: Commit README.md
- [ ] Commit with descriptive message
- [ ] Reference Phase 1 analysis in commit message

---

## EXAMPLE PROMPT FOR AGENT

**When agent reaches README generation step:**

```
Generate README.md for [ProjectName] System Layer project.

INPUTS (MANDATORY - READ THESE FIRST):
1. BOOMI_EXTRACTION_PHASE1.md Section 13 (Sequence Diagram) - Use as blueprint
2. BOOMI_EXTRACTION_PHASE1.md Section 10 (Decision Analysis) - For error handling
3. BOOMI_EXTRACTION_PHASE1.md Section 11 (Branch Analysis) - For convergence patterns
4. BOOMI_EXTRACTION_PHASE1.md Section 6 (HTTP Status Codes) - For error codes
5. Implemented code files - For actual structure, operation names

REQUIREMENTS:
1. Follow README_GENERATION_RULES format (19 sections)
2. Create sequence diagram for EACH Azure Function
3. For EACH atomic handler in sequence diagrams:
   - Specify operation classification (BEST-EFFORT LOOKUP / MAIN OPERATION / etc.)
   - Show error handling (If fails → action, CONTINUE/STOP)
   - Show result (populated or empty / throws exception)
4. Include Error Handling Strategy section with:
   - Operation classification table
   - Rationale from Boomi analysis (reference shape numbers)
   - Code example showing implementation
5. Use realistic examples (actual field names from Phase 1)
6. Reference Boomi patterns (branch convergence, check-before-create, etc.)

CRITICAL:
- Verify error handling from Boomi JSON (check for decision shapes, convergence)
- Don't assume operations throw exceptions - verify from Phase 1
- Show complete flow (don't skip components)
- Classify operations accurately (MUST-SUCCEED vs BEST-EFFORT)

OUTPUT: Complete README.md file with all 19 sections.
```

---

## INTEGRATION WITH BOOMI_EXTRACTION_RULES.mdc

**Where to Add:** After Section "## 21. PHASE 1 COMPLETION CHECKLIST"

**New Section Number:** Section 22 or 23

**Section Title:** "## 22. README.md GENERATION (PHASE 2 - FINAL STEP)"

**Content:** Include all rules above

**Cross-References:**
- Reference Section 13 (Sequence Diagram) as source
- Reference Section 10 (Decision Analysis) for error handling
- Reference Section 11 (Branch Analysis) for convergence patterns
- Reference Section 6 (HTTP Status Codes) for error codes

---

## SUGGESTED ADDITION TO BOOMI_EXTRACTION_RULES.mdc

**Insert this section after "## 21. PHASE 1 COMPLETION CHECKLIST":**

```markdown
---

## 22. README.md GENERATION (PHASE 2 - FINAL STEP)

**🛑 CRITICAL:** README.md MUST be generated as the FINAL step of Phase 2, AFTER all code is complete and committed.

**Purpose:** Document the implemented System Layer for Process Layer consumers and operations teams.

**Source of Truth:** BOOMI_EXTRACTION_PHASE1.md Section 13 (Sequence Diagram)

### 22.1 WHEN TO GENERATE

**Timing:** AFTER Phase 2 code generation is complete

**Prerequisites:**
- [ ] BOOMI_EXTRACTION_PHASE1.md complete and verified
- [ ] All code files implemented and committed
- [ ] Error handling verified from Boomi JSON (decision shapes, convergence)
- [ ] Operation classification determined (MUST-SUCCEED vs BEST-EFFORT)

### 22.2 MANDATORY SECTIONS (19 Total)

[Insert Section 1-19 content from above]

### 22.3 SEQUENCE DIAGRAM REQUIREMENTS (CRITICAL)

**🚨 MOST IMPORTANT SECTION**

**For EACH Azure Function, create sequence diagram showing:**

1. **Complete Component Flow:**
   - Process Layer → Function → Middleware → Service → Handler → ALL Atomic Handlers → Response

2. **Operation Classification (MANDATORY for EACH atomic handler):**
   - `(AUTHENTICATION)` - Auth operations (throw on failure)
   - `(BEST-EFFORT LOOKUP)` - Lookup operations (continue on failure)
   - `(MAIN OPERATION)` - Primary operations (throw on failure)
   - `(CONDITIONAL)` - Conditional operations (continue on failure)
   - `(CLEANUP)` - Cleanup operations (continue on failure)

3. **Error Handling Specification (MANDATORY for EACH atomic handler):**
   ```
   └─→ Error Handling: If fails → [Log warning/Throw exception], [set empty/stop], [CONTINUE/STOP]
   └─→ Result: [variables] (populated or empty / throws exception)
   ```

4. **Conditional Logic (if applicable):**
   ```
   └─→ IF [condition]:
        └─→ [AtomicHandler]
   └─→ ELSE:
        └─→ Skip [operation]
   ```

5. **Boomi Pattern Reference:**
   ```
   **📋 BASED ON:** BOOMI_EXTRACTION_PHASE1.md Section 13 (Sequence Diagram)  
   **Error Handling:** Derived from Boomi [pattern_name] ([shape_references])
   ```

### 22.4 HOW TO DETERMINE ERROR HANDLING FROM BOOMI

**Algorithm:**

```
FOR EACH atomic handler operation:
    
    Step 1: Check for Decision Shapes After Operation
    IF decision shape exists AND checks status code:
        └─→ Operation has explicit error handling
        └─→ Trace TRUE/FALSE paths
        └─→ Classification: Based on path behavior
    ELSE:
        └─→ Go to Step 2
    
    Step 2: Check Branch Convergence
    IF operation is in branch path:
        IF branch paths converge (stop with continue=true):
            └─→ Operation continues to convergence regardless of result
            └─→ Classification: BEST-EFFORT LOOKUP
            └─→ Error Handling: Log warning, set empty, CONTINUE
        ELSE:
            └─→ Go to Step 3
    ELSE:
        └─→ Go to Step 3
    
    Step 3: Check Operation Type
    IF operation is authentication (Login):
        └─→ Classification: AUTHENTICATION
        └─→ Error Handling: Throw exception (required for all ops)
    ELIF operation is main business operation (Create, Update, Delete):
        └─→ Classification: MAIN OPERATION
        └─→ Error Handling: Throw exception (operation must succeed)
    ELIF operation is cleanup (Logout):
        └─→ Classification: CLEANUP
        └─→ Error Handling: Log error, CONTINUE (non-critical)
    ELIF operation is conditional/optional:
        └─→ Classification: CONDITIONAL
        └─→ Error Handling: Log warning, CONTINUE (non-critical)
    ELSE:
        └─→ Classification: BEST-EFFORT LOOKUP
        └─→ Error Handling: Log warning, set empty, CONTINUE
```

### 22.5 VALIDATION CHECKLIST

**Before finalizing README.md:**

- [ ] All Azure Functions have sequence diagrams
- [ ] ALL atomic handlers shown (not skipped)
- [ ] EACH atomic handler has operation classification
- [ ] EACH atomic handler has error handling specification
- [ ] EACH atomic handler has result specification
- [ ] Conditional logic shown with IF/ELSE
- [ ] Boomi pattern references included
- [ ] Operation classification table complete
- [ ] Error handling rationale references Boomi analysis
- [ ] Code examples show actual implementation
- [ ] Request/response examples use actual field names
- [ ] All 19 sections present

### 22.6 COMMON MISTAKES

**Mistake 1: Creating README Before Code**
- ❌ README created during code generation
- ✅ README created AFTER code is complete

**Mistake 2: Generic Sequence Diagrams**
- ❌ Missing atomic handlers, no error handling
- ✅ Complete flow with error handling for each step

**Mistake 3: Assuming Error Handling**
- ❌ Assume all operations throw exceptions
- ✅ Verify from Boomi JSON (decision shapes, convergence)

**Mistake 4: Missing Operation Classification**
- ❌ No classification specified
- ✅ BEST-EFFORT LOOKUP / MAIN OPERATION / etc.

---

**END OF README GENERATION RULES**
```

**This section should be added to BOOMI_EXTRACTION_RULES.mdc after Section 21.**

---

## SUMMARY

**What This Provides:**
1. ✅ Standardized README.md format (19 sections)
2. ✅ Sequence diagram template with error handling
3. ✅ Operation classification system (MUST-SUCCEED vs BEST-EFFORT)
4. ✅ Algorithm for determining error handling from Boomi
5. ✅ Validation checklist
6. ✅ Common mistakes to avoid
7. ✅ Integration instructions for BOOMI_EXTRACTION_RULES.mdc

**Key Innovation:**
- **Operation classification** in sequence diagrams (BEST-EFFORT LOOKUP, MAIN OPERATION, etc.)
- **Error handling specification** for each atomic handler (throw vs continue)
- **Algorithm for determining** error handling from Boomi JSON
- **Boomi pattern references** (branch convergence, check-before-create, etc.)

This ensures future README files accurately document error handling behavior from the start!
