# UPDATED MAIN PROMPT v9 - Changes for Single Source of Truth Approach

**Date:** 2026-01-27  
**Change:** Reflect new approach where Phase 1 is complete technical specification  
**Impact:** Clarifies Phase 1 requirements and README simplification

---

## CHANGES TO MAIN PROMPT

### SECTION: MANDATORY WORKFLOW

**CURRENT (v8):**
```
PHASE 1: BOOMI EXTRACTION & ANALYSIS (BLOCKING - NO CODE YET)
1. Load and analyze ALL JSON files from Boomi_Processes folder
2. Create BOOMI_EXTRACTION_PHASE1.md with ALL mandatory sections:
   - Operations Inventory
   - Input Structure Analysis (Step 1a) - BLOCKING
   - Response Structure Analysis (Step 1b) - BLOCKING
   - Operation Response Analysis (Step 1c) - BLOCKING
   - Map Analysis (Step 1d) - BLOCKING
   - HTTP Status Codes and Return Paths (Step 1e) - BLOCKING
   - Process Properties Analysis (Steps 2-3)
   - Data Dependency Graph (Step 4) - BLOCKING
   - Control Flow Graph (Step 5)
   - Decision Shape Analysis (Step 7) - BLOCKING
   - Branch Shape Analysis (Step 8) - BLOCKING
   - Execution Order (Step 9) - BLOCKING
   - Sequence Diagram (Step 10) - BLOCKING
   - Function Exposure Decision Table - BLOCKING
3. Answer ALL self-check questions (must be YES)
4. Commit BOOMI_EXTRACTION_PHASE1.md
5. STOP - Do NOT proceed to Phase 2 until Phase 1 document is complete
```

**UPDATED (v9):**
```
PHASE 1: BOOMI EXTRACTION & ANALYSIS (BLOCKING - NO CODE YET)
1. Load and analyze ALL JSON files from Boomi_Processes folder
2. Create BOOMI_EXTRACTION_PHASE1.md with ALL mandatory sections:
   - Operations Inventory
   - Input Structure Analysis (Step 1a) - BLOCKING
   - Response Structure Analysis (Step 1b) - BLOCKING
   - Operation Response Analysis (Step 1c) - BLOCKING
   - Map Analysis (Step 1d) - BLOCKING
   - HTTP Status Codes and Return Paths (Step 1e) - BLOCKING
   - Process Properties Analysis (Steps 2-3)
   - Data Dependency Graph (Step 4) - BLOCKING
   - Control Flow Graph (Step 5)
   - Decision Shape Analysis (Step 7) - BLOCKING
   - Branch Shape Analysis (Step 8) - BLOCKING
   - Execution Order (Step 9) - BLOCKING
   - **Sequence Diagram (Step 10) - BLOCKING - COMPLETE TECHNICAL SPECIFICATION:**
     * **Section 13.1: Operation Classification Table (NEW - MANDATORY)**
       - Classify EACH operation (AUTHENTICATION, BEST-EFFORT LOOKUP, MAIN OPERATION, CONDITIONAL, CLEANUP)
       - Error handling for EACH operation (throw vs continue)
       - Reason for EACH classification (decision shapes, convergence, operation type)
       - Boomi references for EACH operation (shape numbers, patterns)
     * **Section 13.2: Enhanced Sequence Diagram (UPDATED - MANDATORY)**
       - Operation classification for EACH operation
       - Error handling specification for EACH operation (If fails → action)
       - Result specification for EACH operation (populated or empty / throws)
       - Boomi references for EACH operation (shape numbers, convergence)
       - Code generation hints for EACH operation (if/else structure, exception types)
   - Function Exposure Decision Table - BLOCKING
3. Answer ALL self-check questions (must be YES)
4. **VERIFY: Section 13 is COMPLETE (operation classification + error handling for ALL operations)**
5. Commit BOOMI_EXTRACTION_PHASE1.md
6. STOP - Do NOT proceed to Phase 2 until Phase 1 document is complete AND verified
```

**Key Changes:**
- ✅ Added Section 13.1: Operation Classification Table (MANDATORY)
- ✅ Added Section 13.2: Enhanced Sequence Diagram requirements
- ✅ Added verification step: Section 13 must be COMPLETE
- ✅ Emphasized: Phase 1 is COMPLETE technical specification

---

### SECTION: PHASE 2 CODE GENERATION

**CURRENT (v8):**
```
PHASE 2: CODE GENERATION (ONLY AFTER PHASE 1 COMPLETE)
1. Verify Phase 1 document exists and is complete
2. Generate System Layer code based on Phase 1 analysis
3. Commit code incrementally (5-8 commits for logical units)
4. STOP - Do NOT proceed to Phase 3 until code is complete
```

**UPDATED (v9):**
```
PHASE 2: CODE GENERATION (ONLY AFTER PHASE 1 COMPLETE)
1. Verify Phase 1 document exists and is complete
2. **Verify Phase 1 Section 13 has operation classification for ALL operations**
3. Generate System Layer code using Phase 1 Section 13 as PRIMARY BLUEPRINT:
   - For EACH operation: Read classification from Section 13.1
   - For EACH operation: Read error handling from Section 13.2
   - For EACH operation: Implement EXACT behavior specified (throw vs continue)
   - **NO ASSUMPTIONS:** All error handling is explicit in Phase 1
   - **NO GUESSING:** Classification tells you exactly what to do
4. Commit code incrementally (5-8 commits for logical units)
5. **Generate README.md as SIMPLE user guide (150-250 lines):**
   - Quick start examples
   - API reference (routes, simple request/response)
   - Configuration guide
   - Deployment guide
   - **Reference Phase 1 for technical details (do NOT duplicate)**
6. STOP - Do NOT proceed to Phase 3 until code AND README are complete
```

**Key Changes:**
- ✅ Added verification: Phase 1 Section 13 has operation classification
- ✅ Added guidance: Use Phase 1 Section 13 as PRIMARY BLUEPRINT
- ✅ Added emphasis: NO ASSUMPTIONS, NO GUESSING
- ✅ Added README generation: SIMPLE user guide (150-250 lines)
- ✅ Added: README references Phase 1 (do NOT duplicate)

---

### SECTION: FINAL OUTPUT

**CURRENT (v8):**
```
FINAL OUTPUT (to user):
- Clear list of files changed/added with brief rationale per file
- README with ASCII sequence diagram
- Confirmation that changes are additive and non-breaking
```

**UPDATED (v9):**
```
FINAL OUTPUT (to user):
- Clear list of files changed/added with brief rationale per file
- **README.md as SIMPLE user guide (150-250 lines):**
  * Quick start examples (how to call APIs)
  * API reference (routes, simple request/response examples)
  * Configuration guide (what to configure)
  * Deployment guide (how to deploy)
  * **References BOOMI_EXTRACTION_PHASE1.md for complete technical details**
  * **Does NOT duplicate Phase 1 content (no detailed sequence diagrams, no operation classification)**
- Confirmation that changes are additive and non-breaking
- **Confirmation that code matches Phase 1 Section 13 specification exactly**
```

**Key Changes:**
- ✅ Clarified: README is SIMPLE user guide (not technical spec)
- ✅ Added: README references Phase 1 for technical details
- ✅ Added: README does NOT duplicate Phase 1 content
- ✅ Added: Confirmation code matches Phase 1 specification

---

### NEW SECTION TO ADD: CRITICAL PRINCIPLES

**ADD THIS NEW SECTION (after "Non-negotiable priority"):**

```
## 🚨 CRITICAL PRINCIPLE: SINGLE SOURCE OF TRUTH

**Phase 1 is the COMPLETE technical specification for code generation.**

### Phase 1 Section 13 Requirements (MANDATORY)

**Section 13.1: Operation Classification Table**
- MUST classify EACH operation using algorithm in BOOMI_EXTRACTION_RULES.mdc STEP 10.1
- MUST specify error handling for EACH operation (throw vs continue)
- MUST provide reason for EACH classification (decision shapes, convergence, operation type)
- MUST include Boomi references for EACH operation (shape numbers, patterns)

**Section 13.2: Enhanced Sequence Diagram**
- MUST show classification for EACH operation: (AUTHENTICATION), (BEST-EFFORT LOOKUP), (MAIN OPERATION), (CONDITIONAL), (CLEANUP)
- MUST show error handling for EACH operation: If fails → [exact action], [CONTINUE/STOP]
- MUST show result for EACH operation: [variables] (populated or empty / throws exception)
- MUST show Boomi references for EACH operation: [shape numbers, convergence points, decision shapes]
- MUST show code hints for EACH operation: [implementation pattern]

**Verification Before Phase 2:**
- [ ] Section 13.1 complete (all operations classified)
- [ ] Section 13.2 complete (all operations have error handling)
- [ ] No ambiguity (developer can generate code without assumptions)
- [ ] All self-checks answered YES

**If Phase 1 Section 13 is incomplete → STOP → Complete it → THEN proceed to Phase 2**

### Code Generation from Phase 1

**PRIMARY INPUT:** BOOMI_EXTRACTION_PHASE1.md Section 13 (ONLY)

**For EACH operation:**
1. Read classification from Section 13.1
2. Read error handling from Section 13.2
3. Implement EXACT behavior:
   - (AUTHENTICATION) → Throw exception if fails
   - (BEST-EFFORT LOOKUP) → Log warning, set empty, continue if fails
   - (MAIN OPERATION) → Throw exception if fails
   - (CONDITIONAL) → Log warning, continue if fails
   - (CLEANUP) → Log error, continue if fails

**NO ASSUMPTIONS ALLOWED:**
- ❌ Don't assume operations throw exceptions
- ❌ Don't assume operations continue on failure
- ✅ Read classification from Phase 1
- ✅ Implement EXACT error handling specified

### README.md Generation

**Purpose:** SIMPLE user guide for Process Layer developers and operations teams

**Length:** 150-250 lines (NOT 700+ lines)

**Content:**
- ✅ Quick start (how to call APIs with curl examples)
- ✅ API reference (routes, simple request/response examples)
- ✅ Configuration guide (what to configure, where secrets go)
- ✅ Deployment guide (how to deploy, environment files)
- ✅ Common errors (user-friendly explanations)
- ✅ Support section (reference Phase 1 for technical details)

**Content NOT in README (already in Phase 1):**
- ❌ Detailed sequence diagrams with error handling
- ❌ Operation classification tables
- ❌ Error handling strategy with code examples
- ❌ Complete folder structure
- ❌ Architecture deep-dive
- ❌ Boomi pattern explanations

**Rule:** README references Phase 1 for technical details (does NOT duplicate).
```

---

### SECTION: DELIVERABLES

**CURRENT (v8):**
```
Deliverables (in order):
1. BOOMI_EXTRACTION_PHASE1.md (committed FIRST)
2. System Layer code files (committed incrementally)
3. RULEBOOK_COMPLIANCE_REPORT.md (committed AFTER code)
4. Build validation results (in compliance report)
```

**UPDATED (v9):**
```
Deliverables (in order):
1. **BOOMI_EXTRACTION_PHASE1.md (committed FIRST) - COMPLETE TECHNICAL SPECIFICATION:**
   - ALL mandatory sections (Steps 1a-1e, 2-10)
   - **Section 13.1: Operation Classification Table (MANDATORY)**
   - **Section 13.2: Enhanced Sequence Diagram with error handling for ALL operations (MANDATORY)**
   - Function Exposure Decision Table
   - **VERIFIED: Complete enough for code generation without assumptions**

2. System Layer code files (committed incrementally)
   - Generated from Phase 1 Section 13 (PRIMARY BLUEPRINT)
   - Error handling matches Phase 1 specification EXACTLY
   - No assumptions made (all behavior explicit in Phase 1)

3. **README.md (SIMPLE user guide - 150-250 lines):**
   - Quick start, API reference, configuration, deployment
   - References Phase 1 for technical details
   - Does NOT duplicate Phase 1 content

4. RULEBOOK_COMPLIANCE_REPORT.md (committed AFTER code)

5. Build validation results (in compliance report)
```

**Key Changes:**
- ✅ Emphasized: Phase 1 is COMPLETE technical specification
- ✅ Added: Section 13.1 and 13.2 requirements
- ✅ Added: Code generated from Phase 1 (no assumptions)
- ✅ Added: README as simple user guide (with length limit)
- ✅ Added: README references Phase 1 (does NOT duplicate)

---

### SECTION: PHASE 1 OUTPUT

**CURRENT (v8):**
```
PHASE 1 OUTPUT:
- Create BOOMI_EXTRACTION_PHASE1.md with all mandatory sections
- Commit this document BEFORE any code generation
- Provide summary: "Phase 1 extraction complete, ready for code generation"
```

**UPDATED (v9):**
```
PHASE 1 OUTPUT:
- Create BOOMI_EXTRACTION_PHASE1.md with all mandatory sections
- **CRITICAL: Section 13 must be COMPLETE technical specification:**
  * Section 13.1: Operation Classification Table
    - Classify EACH operation (use algorithm in BOOMI_EXTRACTION_RULES.mdc STEP 10.1)
    - Error handling for EACH operation
    - Reason for EACH classification
    - Boomi references for EACH operation
  * Section 13.2: Enhanced Sequence Diagram
    - Classification for EACH operation: (AUTHENTICATION), (BEST-EFFORT LOOKUP), etc.
    - Error handling for EACH operation: If fails → [action], [CONTINUE/STOP]
    - Result for EACH operation: [variables] (populated or empty / throws)
    - Boomi references for EACH operation: [shape numbers, convergence]
    - Code hints for EACH operation: [implementation pattern]
- **VERIFY: Developer can generate code from Section 13 without making assumptions**
- Commit this document BEFORE any code generation
- Provide summary: "Phase 1 extraction complete with operation classification. Section 13 is complete technical specification. Ready for code generation."
```

**Key Changes:**
- ✅ Added: Section 13 must be COMPLETE technical specification
- ✅ Added: Section 13.1 requirements (classification table)
- ✅ Added: Section 13.2 requirements (enhanced sequence diagram)
- ✅ Added: Verification step (can generate code without assumptions)
- ✅ Updated: Summary message to confirm completeness

---

### SECTION: PHASE 2 OUTPUT

**CURRENT (v8):**
```
PHASE 2 OUTPUT:
- Create/modify only the necessary files inside the existing repo structure
- Commit incrementally (5-8 commits for logical units of work)
- Provide progress updates as you commit each unit
```

**UPDATED (v9):**
```
PHASE 2 OUTPUT:
- **Verify Phase 1 Section 13 is complete (operation classification + error handling for ALL operations)**
- Generate System Layer code using Phase 1 Section 13 as PRIMARY BLUEPRINT:
  * For EACH operation: Read classification from Section 13.1
  * For EACH operation: Read error handling from Section 13.2
  * For EACH operation: Implement EXACT behavior (throw vs continue)
  * NO ASSUMPTIONS: All error handling is explicit in Phase 1
  * NO GUESSING: Classification tells you exactly what to do
- Commit code incrementally (5-8 commits for logical units of work)
- **Generate README.md as SIMPLE user guide (150-250 lines):**
  * Quick start (how to call APIs with curl examples)
  * API reference (routes, simple request/response examples)
  * Configuration guide (what to configure, where secrets go)
  * Deployment guide (how to deploy, environment files)
  * Common errors (user-friendly explanations)
  * Support section (reference Phase 1 for technical details)
  * **Do NOT duplicate Phase 1 content (no detailed sequence diagrams, no operation classification)**
- Provide progress updates as you commit each unit
```

**Key Changes:**
- ✅ Added: Verification step (Phase 1 Section 13 complete)
- ✅ Added: Use Phase 1 Section 13 as PRIMARY BLUEPRINT
- ✅ Added: For EACH operation instructions
- ✅ Added: NO ASSUMPTIONS, NO GUESSING
- ✅ Added: README generation requirements (simple user guide)
- ✅ Added: README length limit (150-250 lines)
- ✅ Added: Do NOT duplicate Phase 1 content

---

### SECTION: FINAL OUTPUT

**CURRENT (v8):**
```
FINAL OUTPUT (to user):
- Clear list of files changed/added with brief rationale per file
- README with ASCII sequence diagram
- Confirmation that changes are additive and non-breaking
```

**UPDATED (v9):**
```
FINAL OUTPUT (to user):
- Clear list of files changed/added with brief rationale per file
- **README.md as SIMPLE user guide (150-250 lines):**
  * Quick start examples (how to call APIs)
  * API reference (routes, simple request/response examples)
  * Configuration guide (what to configure)
  * Deployment guide (how to deploy)
  * **References BOOMI_EXTRACTION_PHASE1.md Section 13 for complete technical details**
  * **Does NOT duplicate Phase 1 content (no detailed sequence diagrams, no operation classification)**
- Confirmation that changes are additive and non-breaking
- **Confirmation that code matches Phase 1 Section 13 specification exactly (no assumptions made)**
```

**Key Changes:**
- ✅ Clarified: README is SIMPLE user guide (not technical spec)
- ✅ Added: README length limit (150-250 lines)
- ✅ Added: README references Phase 1 for technical details
- ✅ Added: README does NOT duplicate Phase 1 content
- ✅ Added: Confirmation code matches Phase 1 specification

---

## NEW SECTION TO ADD: INPUTS FOR CODE GENERATION

**ADD THIS NEW SECTION (before "Deliverables"):**

```
## 📋 INPUTS FOR CODE GENERATION (SINGLE SOURCE OF TRUTH)

### Primary Input (MANDATORY)

**BOOMI_EXTRACTION_PHASE1.md Section 13 (ONLY)**

This is the SINGLE SOURCE OF TRUTH for code generation. It contains:

**Section 13.1: Operation Classification Table**
- Classification for EACH operation (5 types)
- Error handling for EACH operation (throw vs continue)
- Reason for EACH classification
- Boomi references for EACH operation

**Section 13.2: Enhanced Sequence Diagram**
- Complete execution flow
- Classification for EACH operation
- Error handling for EACH operation (If fails → action)
- Result for EACH operation (variables set)
- Boomi references for EACH operation
- Code hints for EACH operation

**Developers generate code directly from Section 13 without making assumptions.**

### Secondary Inputs (Reference Only)

**Rulebooks (Architecture Patterns):**
- `.cursor/rules/System-Layer-Rules.mdc` - Folder structure, naming, interfaces
- `.cursor/rules/BOOMI_EXTRACTION_RULES.mdc` - Extraction methodology, README generation

**Boomi JSON Files (Verification Only):**
- Use ONLY for verification if Phase 1 is unclear
- Do NOT generate code directly from JSON files

### What NOT to Use

❌ **README.md** - Generated AFTER code, not before  
❌ **Assumptions** - All behavior is explicit in Phase 1  
❌ **Standard patterns** - Boomi has unique patterns (branch convergence, etc.)  
❌ **Boomi JSON directly** - Already analyzed in Phase 1

### Code Generation Workflow

```
Step 1: Read Phase 1 Section 13.1 (Operation Classification Table)
  └─→ For EACH operation: Note classification and error handling

Step 2: Read Phase 1 Section 13.2 (Enhanced Sequence Diagram)
  └─→ For EACH operation: Read detailed specification

Step 3: Generate Handler code
  └─→ For EACH atomic handler call:
       - Check classification (BEST-EFFORT / MAIN OPERATION / etc.)
       - Implement error handling as specified (throw vs continue)
       - Set result variables as specified (populated or empty)
       - Use code hints provided in Phase 1

Step 4: Verify code matches Phase 1
  └─→ Error handling matches specification
  └─→ No assumptions made
  └─→ All operations implemented as classified

Step 5: Generate README.md (SIMPLE user guide)
  └─→ Quick start, API reference, configuration, deployment
  └─→ Reference Phase 1 for technical details
  └─→ Do NOT duplicate Phase 1 content
```
```

---

## SUMMARY OF CHANGES TO MAIN PROMPT v8 → v9

### Added Sections

1. **CRITICAL PRINCIPLE: SINGLE SOURCE OF TRUTH** (NEW)
   - Phase 1 Section 13 requirements
   - Code generation workflow
   - Inputs for code generation

### Updated Sections

2. **PHASE 1 WORKFLOW**
   - Added Section 13.1 requirements (classification table)
   - Added Section 13.2 requirements (enhanced sequence diagram)
   - Added verification step (Section 13 complete)

3. **PHASE 2 WORKFLOW**
   - Added verification (Section 13 has operation classification)
   - Added guidance (use Phase 1 as PRIMARY BLUEPRINT)
   - Added README generation (simple user guide)
   - Added NO ASSUMPTIONS, NO GUESSING

4. **FINAL OUTPUT**
   - Clarified README as simple user guide (150-250 lines)
   - Added README references Phase 1 (does NOT duplicate)
   - Added confirmation code matches Phase 1

### Key Improvements

✅ **Eliminates ambiguity:** Error handling explicit for each operation  
✅ **Eliminates assumptions:** Classification tells you what to do  
✅ **Single source of truth:** Phase 1 Section 13 is the blueprint  
✅ **Simple README:** User guide, not technical spec  
✅ **No duplication:** Technical details in ONE place (Phase 1)

---

## RECOMMENDATION

**YES, the main prompt should be updated to v9 with these changes.**

**Impact:**
- Future agents will create COMPLETE Phase 1 with operation classification
- Code generation will use Phase 1 as single source of truth
- README will be simple user guide (not technical spec)
- No ambiguity, no assumptions, no errors

**Files to Update:**
- User query instructions (wherever the main prompt is stored)
- Any templates or examples that reference the workflow

**Would you like me to:**
1. Create a clean v9 prompt document with all changes integrated?
2. Create a diff showing exactly what changed from v8 to v9?
3. Create a migration guide for updating existing prompts?

---

**END OF UPDATED MAIN PROMPT v9**
