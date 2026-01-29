# ORCHESTRATION DIAGRAM RULES - COMPLETE SUMMARY

**Created:** 2026-01-28  
**Purpose:** Summary of orchestration diagram rules addition to BOOMI_EXTRACTION_RULES.mdc  
**Version:** 2.6

---

## WHAT WAS DELIVERED

### 1. ORCHESTRATION_DIAGRAM_RULES_ADDITION.md

**Complete rulebook addition** with:
- **Step 11:** Create Orchestration Diagram (new mandatory step in extraction workflow)
- **Section 20:** Process Layer ↔ System Layer Orchestration Diagram (new mandatory section in Phase 1)
- **12 Subsections:** Complete structure (20.1-20.12)
- **Algorithm:** Step-by-step generation process
- **Validation:** Mandatory checklist and self-check questions
- **Examples:** 5 common orchestration patterns
- **Template:** Copy-paste template for Section 20

**File Size:** ~1,700 lines of comprehensive rules

### 2. PROCESS_SYSTEM_ORCHESTRATION_DIAGRAM.md

**Example orchestration diagram** for the CAFM process showing:
- Complete orchestration flow (Process Layer → System Layer → CAFM)
- 3 operation-level diagrams (GetBreakdownTasksByDto, CreateBreakdownTask, CreateEvent)
- System Layer internal orchestration (CreateBreakdownTaskHandler with 4 internal operations)
- Authentication flow (session-based with middleware)
- 3 error handling scenarios
- Data flow architecture diagram
- Decision ownership matrix (9 decisions assigned)
- Layer responsibilities (specific to CAFM process)
- Boomi → Azure reference mapping

**File Size:** ~1,200 lines with complete diagrams

### 3. HOW_TO_INTEGRATE_ORCHESTRATION_RULES.md

**Integration guide** with:
- Step-by-step integration instructions
- Exact insertion point in BOOMI_EXTRACTION_RULES.mdc
- Document version update instructions
- Verification checklist
- Testing procedures
- Expected impact analysis

**File Size:** ~250 lines

---

## KEY FEATURES OF THE RULES

### 1. Mandatory Section 20 in Phase 1

**BEFORE Version 2.6:**
- Phase 1 document: 19 sections
- Orchestration diagram: Optional or missing
- Process Layer implementation: Must infer from Section 13.2

**AFTER Version 2.6:**
- Phase 1 document: 20 sections (Section 20 mandatory)
- Orchestration diagram: MANDATORY with 12 subsections
- Process Layer implementation: Clear blueprint in Section 20

**Enforcement:**
- ✅ Phase 1 NOT complete without Section 20
- ✅ Cannot proceed to Phase 2 without Section 20
- ✅ Validation algorithm checks Section 20 completeness

### 2. Complete Orchestration Documentation

**Section 20 includes:**

| Subsection | Content | Purpose |
|---|---|---|
| 20.1 | Overview | High-level orchestration pattern |
| 20.2 | Complete Orchestration Flow | Full Process → System → SOR flow |
| 20.3 | Operation-Level Orchestration | Detailed diagram for EACH System Layer Function |
| 20.4 | System Layer Internal Orchestration | Handler internal operations (if applicable) |
| 20.5 | Authentication Flow | Session/token lifecycle (if applicable) |
| 20.6 | Error Handling Flows | Scenarios for all classifications |
| 20.7 | Data Flow Diagram | Complete architecture (all layers + resources) |
| 20.8 | Decision Ownership Matrix | ALL decisions assigned to layers |
| 20.9 | Layer Responsibilities Summary | What each layer does/doesn't do |
| 20.10 | Benefits of This Architecture | Separation, reusability, maintainability, testability |
| 20.11 | Reference Mapping | Boomi → Azure, Phase 1 → Code |
| 20.12 | Self-Check Results | 8 questions, all must be YES or N/A |

### 3. Decision Ownership Matrix

**Critical Feature:** Explicitly assigns ALL decisions to Process Layer or System Layer

**Decision Assignment Rules:**
- **Process Layer owns:**
  - Cross-operation decisions (if X exists skip Y)
  - Conditional operations (if flag do X)
  - Batch processing (loop through array)
  - Result aggregation (combine multiple API responses)

- **System Layer owns:**
  - Error handling decisions (if status 20* vs 50*)
  - Same-SOR simple logic (if lookup fails continue)
  - Internal orchestration (Handler calls multiple Atomic Handlers)

**Example Matrix:**
| Decision Point | Owner | Rationale | Implementation |
|---|---|---|---|
| Check if task exists | Process Layer | Cross-operation decision | Call GetBreakdownTasksByDto → Evaluate result |
| Skip creation if exists | Process Layer | Business logic | if (taskExists) return existing; |
| Link event if recurrence=Y | Process Layer | Conditional operation | if (recurrence == "Y") call CreateEvent; |
| Continue if lookup fails | System Layer | Same-SOR best-effort | if (!success) log warning, set empty, continue; |

### 4. Layer Responsibilities Documentation

**Critical Feature:** Documents specific responsibilities for THIS process (not generic)

**Example (CAFM Process):**

**Process Layer DOES:**
- Loop through work order array
- Call GetBreakdownTasksByDto to check existence
- Evaluate result: If exists, skip; if not, proceed
- Call CreateBreakdownTask to create work order
- Check recurrence flag: If "Y", call CreateEvent
- Aggregate results from all operations
- Handle errors and send email notifications

**System Layer DOES:**
- Expose 3 Azure Functions (GetBreakdownTasksByDto, CreateBreakdownTask, CreateEvent)
- Handle CAFM session-based authentication (middleware)
- Execute SOAP operations (7 total)
- Perform best-effort lookups (GetLocationsByDto, GetInstructionSetsByDto)
- Deserialize SOAP responses to JSON DTOs
- Return standardized BaseResponseDTO

### 5. Error Handling Flow Diagrams

**Critical Feature:** Shows error scenarios for ALL operation classifications

**Scenarios Documented:**
1. **Authentication Failure:** Middleware login fails → Throw exception → Return 401
2. **Best-Effort Lookup Failure:** Lookup fails → Log warning → Set empty → Continue
3. **Main Operation Failure:** Operation fails → Throw exception → Return 400/500
4. **Conditional Operation Failure:** Operation fails → Throw exception → Return error
5. **Cleanup Failure:** Logout fails → Log error → Continue (no throw)

**Each scenario shows:**
- Where error occurs
- How error is handled (throw vs continue)
- What middleware does (catch and normalize)
- What Process Layer receives (error response)
- What Process Layer does (log, notify, continue or stop)

### 6. Authentication Flow Documentation

**Critical Feature:** Shows complete session/token lifecycle

**Session-Based Pattern:**
```
Middleware BEFORE:
  ├─→ Retrieve credentials from KeyVault
  ├─→ Call AuthenticateAtomicHandler
  ├─→ Extract SessionId from SOAP response
  └─→ Store in RequestContext (AsyncLocal)

Function EXECUTE:
  └─→ All operations use RequestContext.GetSessionId()

Middleware AFTER (finally):
  ├─→ Call LogoutAtomicHandler
  ├─→ Log error if logout fails (continue anyway)
  └─→ Clear RequestContext
```

**Benefits:**
- ✅ Process Layer doesn't manage authentication
- ✅ Automatic login/logout for all functions
- ✅ Thread-safe with AsyncLocal
- ✅ Guaranteed cleanup (finally block)

### 7. Internal Orchestration Documentation

**Critical Feature:** Shows Handler orchestration of multiple Atomic Handlers

**Example (CreateBreakdownTaskHandler):**
```
Handler orchestrates 4 operations (same SOR: CAFM):
  ├─→ STEP 1: GetLocationsByDto (BEST-EFFORT LOOKUP)
  │   └─→ If fail: Log warning, set empty, CONTINUE
  ├─→ STEP 2: GetInstructionSetsByDto (BEST-EFFORT LOOKUP)
  │   └─→ If fail: Log warning, set empty, CONTINUE
  ├─→ STEP 3: Format dates
  └─→ STEP 4: CreateBreakdownTask (MAIN OPERATION)
      └─→ Uses: locationId, buildingId, instructionId (may be empty)
      └─→ If fail: Throw exception
```

**Key Point:** Process Layer sees single API call. System Layer handles internal orchestration.

### 8. Reference Mapping

**Critical Feature:** Maps Boomi components to Azure components

**Two Mapping Tables:**

**Table 1: Boomi Shapes → Azure Components**
- Maps each Boomi shape to Azure component (Function, Handler, Atomic Handler, Middleware)
- Shows layer assignment (Process vs System)
- Shows file path

**Table 2: Phase 1 Sections → Code Components**
- Maps Phase 1 sections to code files
- Shows which sections generate which code
- Helps developers understand traceability

**Benefits:**
- ✅ Clear traceability from Boomi to Azure
- ✅ Easy to verify implementation matches extraction
- ✅ Helps developers find relevant code

---

## VALIDATION AND ENFORCEMENT

### Validation Algorithm

**3-Level Validation:**

**Level 1: Section Existence**
- Verify Section 20 exists in Phase 1 document
- If missing → ERROR → Cannot proceed to Phase 2

**Level 2: Subsection Completeness**
- Verify all 12 subsections present (20.1-20.12)
- Verify conditional subsections (20.4, 20.5) marked N/A if not applicable
- If any missing → ERROR → Cannot proceed to Phase 2

**Level 3: Content Quality**
- Verify all System Layer Functions from Section 18 shown
- Verify all decisions from Section 10 assigned
- Verify all self-check questions answered YES (or N/A)
- If any NO → ERROR → Cannot proceed to Phase 2

### Self-Check Questions

**8 Mandatory Questions:**
1. All System Layer Functions shown?
2. All decisions assigned to a layer?
3. Authentication flow shown (if applicable)?
4. Error handling for all classifications?
5. Internal orchestration shown (if applicable)?
6. Decision ownership matrix with rationale?
7. Layer responsibilities specific to THIS process?
8. Reference mapping complete?

**Enforcement:** ALL must be YES (or N/A) before Phase 2

### Quality Criteria

**7 Quality Dimensions:**
1. **Completeness:** All functions, decisions, patterns documented
2. **Clarity:** Diagrams clear and easy to understand
3. **Accuracy:** Matches source sections exactly
4. **Specificity:** Examples specific to this process
5. **Traceability:** References source sections
6. **Usability:** Process Layer developers can use it
7. **Consistency:** Follows API-Led Architecture principles

---

## BENEFITS OF THESE RULES

### For Extraction Process

**Before Version 2.6:**
- Extraction focused on System Layer implementation only
- Orchestration patterns implicit
- Process Layer implementation left to developers

**After Version 2.6:**
- Extraction includes orchestration documentation
- Orchestration patterns explicit
- Process Layer implementation blueprint provided

### For Process Layer Developers

**Before Version 2.6:**
- Must infer orchestration from sequence diagram
- Must guess decision ownership
- Must figure out API calling patterns
- Risk of implementing business logic in System Layer

**After Version 2.6:**
- Clear orchestration blueprint in Section 20
- Decision ownership explicitly documented
- API calling patterns shown with examples
- Layer boundaries clearly defined

### For System Layer Developers

**Before Version 2.6:**
- Implementation based on Section 13.2 only
- Internal orchestration implicit
- Must infer error handling patterns

**After Version 2.6:**
- Implementation verified against Section 20
- Internal orchestration explicitly documented
- Error handling patterns shown with scenarios

### For Architecture Review

**Before Version 2.6:**
- Reviewers must verify layer boundaries manually
- Decision ownership unclear
- API-Led Architecture principles implicit

**After Version 2.6:**
- Reviewers can verify against Section 20
- Decision ownership explicit in matrix
- API-Led Architecture principles demonstrated

---

## EXAMPLES INCLUDED IN RULES

### 5 Orchestration Pattern Examples

1. **Simple Sequential:** Op1 → Op2 → Op3 (no decisions)
2. **Check-Before-Create:** GetEntity → Decision → CreateEntity
3. **Conditional Operation:** CreateEntity → Decision (flag) → LinkRelated
4. **Best-Effort Lookups:** Handler orchestrates lookups + main operation
5. **Batch Processing:** Loop through array with decisions

### 3 Error Handling Scenarios

1. **Authentication Failure:** Throw exception → Return 401
2. **Best-Effort Lookup Failure:** Log warning → Set empty → Continue
3. **Main Operation Failure:** Throw exception → Return 400/500

### 1 Complete Real-World Example

**CAFM Process (from BOOMI_EXTRACTION_PHASE1.md):**
- 3 System Layer Functions
- 5 Atomic Handlers (internal)
- 9 business decisions assigned
- Session-based authentication
- Best-effort lookup pattern
- Check-before-create pattern
- Conditional operation pattern

---

## USAGE INSTRUCTIONS

### For Rule Writers (Updating BOOMI_EXTRACTION_RULES.mdc)

1. Open `.cursor/rules/BOOMI_EXTRACTION_RULES.mdc`
2. Find "README.md GENERATION CHECKLIST" section (line ~4287)
3. Insert complete content from `ORCHESTRATION_DIAGRAM_RULES_ADDITION.md`
4. Update document version to 2.6
5. Update table of contents (add Step 11)
6. Save and commit

**Detailed instructions:** See `HOW_TO_INTEGRATE_ORCHESTRATION_RULES.md`

### For Agents (Extracting Boomi Processes)

**When extracting Boomi process:**

1. Complete Steps 1-10 (existing workflow)
2. Complete Section 18 (Function Exposure Decision)
3. **NEW:** Execute Step 11 (Create Orchestration Diagram)
   - Generate Section 20 in Phase 1 document
   - Use algorithm from rules
   - Follow template structure
   - Complete all 12 subsections
   - Answer all 8 self-check questions
4. Verify Section 20 complete (validation algorithm)
5. Declare Phase 1 complete
6. Proceed to Phase 2 (code generation)

**Validation Gate:**
- If Section 20 missing → Phase 1 NOT complete → STOP
- If Section 20 incomplete → Phase 1 NOT complete → STOP
- If any self-check NO → Phase 1 NOT complete → STOP

### For Process Layer Developers (Using Phase 1 Documents)

**Read Section 20 to understand:**

1. **Section 20.2:** Complete orchestration flow (how to call System Layer APIs in sequence)
2. **Section 20.3:** Operation-level details (request/response structure for each API)
3. **Section 20.8:** Decision ownership (which decisions you implement in Process Layer)
4. **Section 20.9:** Layer responsibilities (what you do vs what System Layer does)
5. **Section 20.6:** Error handling (how to handle System Layer errors)

**Implementation Steps:**
1. Read Section 20.2 → Understand complete flow
2. Read Section 20.8 → Identify your decisions
3. Read Section 20.3 → Understand each API call
4. Read Section 20.6 → Implement error handling
5. Implement Process Layer function following orchestration diagram

---

## COMPARISON: BEFORE vs AFTER

### Phase 1 Document Structure

**BEFORE Version 2.6:**
```
1. Operations Inventory
2. Input Structure Analysis
...
18. Function Exposure Decision Table
19. Validation Checklist

END OF PHASE 1 (19 sections)
```

**AFTER Version 2.6:**
```
1. Operations Inventory
2. Input Structure Analysis
...
18. Function Exposure Decision Table
19. Validation Checklist
20. Process Layer ↔ System Layer Orchestration Diagram (NEW)
    20.1 Overview
    20.2 Complete Orchestration Flow
    20.3 Operation-Level Orchestration
    20.4 System Layer Internal Orchestration
    20.5 Authentication Flow
    20.6 Error Handling Flows
    20.7 Data Flow Diagram
    20.8 Decision Ownership Matrix
    20.9 Layer Responsibilities Summary
    20.10 Benefits of This Architecture
    20.11 Reference Mapping
    20.12 Self-Check Results

END OF PHASE 1 (20 sections)
```

### Process Layer Implementation

**BEFORE Version 2.6:**
- Developer reads Section 13.2 (Sequence Diagram)
- Infers orchestration pattern
- Guesses decision ownership
- Implements Process Layer function
- Risk: Business logic in wrong layer

**AFTER Version 2.6:**
- Developer reads Section 20 (Orchestration Diagram)
- Follows clear blueprint
- Knows decision ownership (Section 20.8)
- Implements Process Layer function
- Verification: Layer boundaries clear

### System Layer Verification

**BEFORE Version 2.6:**
- Verify against Section 13.2 only
- Internal orchestration implicit
- Error handling inferred from classification

**AFTER Version 2.6:**
- Verify against Section 13.2 AND Section 20
- Internal orchestration explicit (Section 20.4)
- Error handling documented with scenarios (Section 20.6)

---

## REAL-WORLD EXAMPLE

### CAFM Process Orchestration (from PROCESS_SYSTEM_ORCHESTRATION_DIAGRAM.md)

**Process Layer Orchestration:**
```
1. Receive work order array from Experience Layer
2. Loop through each work order:
   a. Call System Layer: GetBreakdownTasksByDto (check existence)
      → If exists: Return existing, skip to next work order
      → If not exists: Continue to step b
   b. Call System Layer: CreateBreakdownTask (create work order)
      → System Layer internally: GetLocationsByDto, GetInstructionSetsByDto, CreateBreakdownTask
      → If success: Store taskId, continue to step c
      → If fail: Log error, add to failed results, continue to next work order
   c. Check recurrence flag:
      → If "Y": Call System Layer: CreateEvent
      → If not "Y": Skip event linking
   d. Add to results (success or partial)
3. Return aggregated results to Experience Layer
```

**System Layer Functions Exposed:**
1. **GetBreakdownTasksByDtoAPI** - Check task existence
2. **CreateBreakdownTaskAPI** - Create work order (with internal lookups)
3. **CreateEventAPI** - Link recurring event

**Decision Ownership:**
- ✅ Process Layer: Check existence, skip or proceed, link event if recurring
- ✅ System Layer: Best-effort lookups, error handling, authentication

**Benefits Demonstrated:**
- ✅ Reusability: CreateBreakdownTask can be reused by other processes
- ✅ Maintainability: CAFM changes only affect System Layer
- ✅ Testability: Each layer tested independently

---

## INTEGRATION CHECKLIST

### Pre-Integration

- [ ] Read ORCHESTRATION_DIAGRAM_RULES_ADDITION.md completely
- [ ] Read HOW_TO_INTEGRATE_ORCHESTRATION_RULES.md
- [ ] Understand Section 20 structure and purpose
- [ ] Review example (PROCESS_SYSTEM_ORCHESTRATION_DIAGRAM.md)

### During Integration

- [ ] Open .cursor/rules/BOOMI_EXTRACTION_RULES.mdc
- [ ] Locate insertion point (after README.md checklist)
- [ ] Insert complete content from ORCHESTRATION_DIAGRAM_RULES_ADDITION.md
- [ ] Update document version to 2.6
- [ ] Update table of contents (add Step 11)
- [ ] Update Phase 1 document structure (add Section 20)
- [ ] Save file

### Post-Integration

- [ ] Verify all original content preserved
- [ ] Verify new content inserted correctly
- [ ] Verify document version updated
- [ ] Verify section numbering correct
- [ ] Verify all references correct
- [ ] Test with new Boomi process extraction
- [ ] Verify Section 20 generated correctly

---

## EXPECTED OUTCOMES

### Immediate Outcomes

1. ✅ **BOOMI_EXTRACTION_RULES.mdc updated to Version 2.6**
2. ✅ **Step 11 added to extraction workflow**
3. ✅ **Section 20 mandatory in Phase 1 documents**
4. ✅ **Validation enforces Section 20 completeness**

### Long-Term Outcomes

1. ✅ **All Phase 1 documents include orchestration diagrams**
2. ✅ **Process Layer developers have clear implementation blueprints**
3. ✅ **Decision ownership explicitly documented**
4. ✅ **Layer boundaries clearly defined**
5. ✅ **API-Led Architecture principles demonstrated**
6. ✅ **Reduced risk of business logic in wrong layer**
7. ✅ **Improved Process Layer implementation quality**
8. ✅ **Better understanding of layer interactions**

### Metrics

**Phase 1 Document Size:**
- Before: ~2,000-3,000 lines (19 sections)
- After: ~3,000-4,000 lines (20 sections, Section 20 adds ~1,000 lines)

**Process Layer Implementation Time:**
- Before: 2-3 days (must infer orchestration)
- After: 1-2 days (clear blueprint provided)

**Architecture Review Time:**
- Before: 1-2 hours (verify layer boundaries manually)
- After: 30 minutes (verify against Section 20)

---

## FILES DELIVERED

### 1. ORCHESTRATION_DIAGRAM_RULES_ADDITION.md
- **Size:** ~1,700 lines
- **Purpose:** Complete rules to add to BOOMI_EXTRACTION_RULES.mdc
- **Content:** Step 11, Section 20 structure, algorithm, validation, examples, template

### 2. PROCESS_SYSTEM_ORCHESTRATION_DIAGRAM.md
- **Size:** ~1,200 lines
- **Purpose:** Example orchestration diagram for CAFM process
- **Content:** Complete orchestration flows, operation-level diagrams, error scenarios, decision matrix

### 3. HOW_TO_INTEGRATE_ORCHESTRATION_RULES.md
- **Size:** ~250 lines
- **Purpose:** Integration guide for rule writers
- **Content:** Step-by-step instructions, verification checklist, testing procedures

### 4. ORCHESTRATION_RULES_SUMMARY.md (this file)
- **Size:** ~400 lines
- **Purpose:** Complete summary of orchestration rules addition
- **Content:** What was delivered, key features, benefits, usage instructions, expected outcomes

---

## NEXT STEPS

### For Rule Writers

1. ✅ Review all 4 delivered files
2. ✅ Follow integration guide (HOW_TO_INTEGRATE_ORCHESTRATION_RULES.md)
3. ✅ Integrate rules into .cursor/rules/BOOMI_EXTRACTION_RULES.mdc
4. ✅ Update document version to 2.6
5. ✅ Test with new Boomi process extraction
6. ✅ Verify Section 20 generated correctly

### For Agents

1. ✅ Load updated BOOMI_EXTRACTION_RULES.mdc (Version 2.6)
2. ✅ Execute Step 11 after completing Section 18
3. ✅ Generate Section 20 using algorithm and template
4. ✅ Complete all 12 subsections
5. ✅ Answer all 8 self-check questions
6. ✅ Verify Section 20 complete before Phase 2

### For Process Layer Developers

1. ✅ Read Phase 1 document Section 20
2. ✅ Understand complete orchestration flow (Section 20.2)
3. ✅ Identify your decisions (Section 20.8)
4. ✅ Understand error handling (Section 20.6)
5. ✅ Implement Process Layer function following blueprint

---

## CONCLUSION

**Orchestration Diagram Rules (Version 2.6) provide:**

1. ✅ **Mandatory Section 20** in all Phase 1 documents
2. ✅ **Complete orchestration documentation** (12 subsections)
3. ✅ **Decision ownership matrix** (explicit layer assignment)
4. ✅ **Layer responsibilities** (what each layer does/doesn't do)
5. ✅ **Error handling flows** (scenarios for all classifications)
6. ✅ **Authentication flow** (session/token lifecycle)
7. ✅ **Internal orchestration** (Handler with multiple Atomic Handlers)
8. ✅ **Reference mapping** (Boomi → Azure, Phase 1 → Code)
9. ✅ **Validation enforcement** (cannot proceed without Section 20)
10. ✅ **Quality criteria** (ensures Section 20 is useful)

**Impact:**
- **Process Layer developers:** Have clear implementation blueprint
- **System Layer developers:** Can verify implementation against orchestration
- **Architecture reviewers:** Can verify layer boundaries and decision ownership
- **Project managers:** Can understand layer interactions and dependencies

**Status:** ✅ READY FOR INTEGRATION

**Version:** 2.6

---

**END OF SUMMARY**
