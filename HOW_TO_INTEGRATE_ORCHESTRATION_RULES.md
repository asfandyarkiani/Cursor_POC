# HOW TO INTEGRATE ORCHESTRATION DIAGRAM RULES

**Purpose:** Instructions for adding orchestration diagram rules to BOOMI_EXTRACTION_RULES.mdc  
**Source File:** ORCHESTRATION_DIAGRAM_RULES_ADDITION.md  
**Target File:** .cursor/rules/BOOMI_EXTRACTION_RULES.mdc  
**Date:** 2026-01-28

---

## INTEGRATION STEPS

### Step 1: Locate Insertion Point

**Open file:** `.cursor/rules/BOOMI_EXTRACTION_RULES.mdc`

**Find this section** (near end of file, around line 4287):
```markdown
### README.md GENERATION CHECKLIST (SIMPLIFIED)

**Before generating README.md:**
...
**After README.md generation:**
...
---

**Document Version:** 2.5
```

**Insertion point:** AFTER "README.md GENERATION CHECKLIST" section and BEFORE "Document Version" footer

### Step 2: Insert New Content

**Copy entire content from:** `ORCHESTRATION_DIAGRAM_RULES_ADDITION.md`

**Insert at:** Line ~4310 (after README.md checklist, before Document Version footer)

**What to insert:**
- Complete section starting with: `## 🛑 POST-PHASE 1: ORCHESTRATION DIAGRAM GENERATION (MANDATORY)`
- Ending with: `**END OF ORCHESTRATION DIAGRAM RULES**`

### Step 3: Update Document Version Footer

**Find this footer** (currently at line ~4311):
```markdown
**Document Version:** 2.5  
**Purpose:** Prescriptive guide for extracting execution sequence from ANY Boomi process  
**Key Changes:** 
- Version 2.0: Made rules explicit, mandatory, and non-negotiable
- Version 2.1: Added mandatory Input/Output Structure Analysis (Contract Verification)
- Version 2.2: Added mandatory Map Analysis (Step 1d) - Critical for SOAP envelope field name verification
- Version 2.3: Added mandatory HTTP Status Codes and Return Path Responses (Step 1e) - Critical for error handling and response mapping; Added Request/Response JSON Examples section
- Version 2.4: Added README.md Generation rules (Post-Phase 2) - Critical for documenting error handling behavior with operation classification system
- Version 2.5: **MAJOR CHANGE** - Moved ALL technical details to Phase 1 Section 13. README.md is now a simple user guide (150-250 lines). Sequence diagrams with operation classification, error handling, and Boomi references are ONLY in Phase 1. README references Phase 1 for technical details. This eliminates duplication and makes Phase 1 the single source of truth for code generation.
```

**Replace with:**
```markdown
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

### Step 4: Update Table of Contents

**Find the Table of Contents** (near top of file, around line 53):
```markdown
## 📋 MANDATORY EXTRACTION WORKFLOW

### STEP 1: Load JSON Files and Build Lookup Tables
### STEP 1a: Input Structure Analysis
### STEP 1b: Response Structure Analysis
...
### STEP 10: Create Sequence Diagram
```

**Add new step:**
```markdown
### STEP 11: Create Orchestration Diagram (MANDATORY - NEW)
```

### Step 5: Update Phase 1 Document Structure Section

**Find section:** "📋 PHASE 1 DOCUMENT STRUCTURE (MANDATORY SECTIONS)" (around line 3500)

**Add to the list:**
```markdown
19. Validation Checklist
20. **Process Layer ↔ System Layer Orchestration Diagram** ⚠️ **NEW MANDATORY SECTION**
```

---

## VERIFICATION AFTER INTEGRATION

### Verify File Structure

**Check that BOOMI_EXTRACTION_RULES.mdc now has:**

1. ✅ All original content (Steps 1-10)
2. ✅ README.md generation checklist
3. ✅ **NEW:** Post-Phase 1 Orchestration Diagram Generation section
4. ✅ **NEW:** Step 11 in workflow
5. ✅ Updated Document Version to 2.6
6. ✅ Updated Key Changes with Version 2.6 entry

### Verify Section Numbering

**Ensure sections are numbered correctly:**
- Sections 1-19: Original sections (unchanged)
- **Section 20: Process Layer ↔ System Layer Orchestration Diagram (NEW)**
- Section 20.1-20.12: Subsections (NEW)

### Verify References

**Ensure all references are correct:**
- Section 20 references Section 10 (Decision Analysis)
- Section 20 references Section 13.1 (Operation Classification)
- Section 20 references Section 13.2 (Sequence Diagram)
- Section 20 references Section 15 (Critical Patterns)
- Section 20 references Section 18 (Function Exposure Decision)

---

## TESTING THE INTEGRATION

### Test 1: Extract New Process

**After integrating rules, extract a new Boomi process and verify:**

1. ✅ Phase 1 document has Section 20
2. ✅ Section 20 has all 12 subsections
3. ✅ All System Layer Functions from Section 18 shown in Section 20.3
4. ✅ All decisions from Section 10 assigned in Section 20.8
5. ✅ Authentication flow shown (if applicable)
6. ✅ Error handling flows shown for all classifications
7. ✅ Self-check results all YES (or N/A)

### Test 2: Verify Completeness

**Check that agent cannot proceed to Phase 2 without Section 20:**

1. ✅ Agent creates Sections 1-19
2. ✅ Agent attempts to proceed to Phase 2
3. ✅ Validation fails: "Section 20 missing"
4. ✅ Agent creates Section 20
5. ✅ Validation passes
6. ✅ Agent proceeds to Phase 2

### Test 3: Verify Quality

**Check that Section 20 is useful:**

1. ✅ Process Layer developer can understand orchestration flow
2. ✅ Decision ownership is clear
3. ✅ Error handling patterns are clear
4. ✅ Layer boundaries are clear
5. ✅ Examples are specific to the process (not generic)

---

## EXPECTED IMPACT

### On Phase 1 Extraction

**Before Version 2.6:**
- Phase 1 document: 19 sections
- Orchestration diagram: Optional or missing
- Decision ownership: Unclear
- Layer boundaries: Implicit

**After Version 2.6:**
- Phase 1 document: 20 sections (Section 20 mandatory)
- Orchestration diagram: MANDATORY in Section 20
- Decision ownership: Explicitly documented in Section 20.8
- Layer boundaries: Clearly defined in Section 20.9

### On Process Layer Implementation

**Before Version 2.6:**
- Process Layer developers: Must infer orchestration from Section 13.2
- Decision ownership: Must guess which layer owns which decision
- API calling pattern: Must figure out from code

**After Version 2.6:**
- Process Layer developers: Clear blueprint in Section 20
- Decision ownership: Explicitly documented in Section 20.8
- API calling pattern: Shown in Section 20.2 and 20.3

### On System Layer Implementation

**Before Version 2.6:**
- System Layer implementation: Based on Section 13.2 only
- Internal orchestration: Implicit from sequence diagram
- Error handling: Must infer from classification

**After Version 2.6:**
- System Layer implementation: Verified against Section 20
- Internal orchestration: Explicitly shown in Section 20.4
- Error handling: Documented in Section 20.6 with scenarios

---

## MAINTENANCE

### When to Update Section 20 Rules

**Update orchestration diagram rules when:**
1. New operation classification type added (currently 5 types)
2. New authentication pattern added (currently session-based, token-based, credentials-per-request)
3. New error handling pattern added
4. New orchestration pattern identified
5. API-Led Architecture principles change

### Version History

- **Version 2.6:** Added mandatory orchestration diagram section
- **Future versions:** Will add new patterns, classifications, or requirements

---

## SUMMARY

**What This Integration Adds:**

1. ✅ **Section 20 (Orchestration Diagram)** - New mandatory section in Phase 1 documents
2. ✅ **Step 11 (Create Orchestration Diagram)** - New step in extraction workflow
3. ✅ **12 Subsections** - Complete orchestration documentation structure
4. ✅ **Decision Ownership Matrix** - Explicitly assigns all decisions to layers
5. ✅ **Layer Responsibilities** - Documents what each layer does/doesn't do
6. ✅ **Error Handling Flows** - Shows error scenarios for all classifications
7. ✅ **Reference Mapping** - Maps Boomi components to Azure components
8. ✅ **Validation Rules** - Ensures Section 20 is complete before Phase 2
9. ✅ **Self-Check Questions** - 8 mandatory questions (all must be YES or N/A)
10. ✅ **Quality Criteria** - Ensures Section 20 is useful and clear

**Impact:**
- Process Layer developers have clear implementation blueprint
- Decision ownership explicitly documented
- Layer boundaries clearly defined
- API-Led Architecture principles demonstrated
- Orchestration patterns documented with examples

**Next Steps:**
1. Integrate rules into .cursor/rules/BOOMI_EXTRACTION_RULES.mdc
2. Update document version to 2.6
3. Test with new Boomi process extraction
4. Verify Section 20 is generated correctly
5. Verify Process Layer developers can use Section 20 effectively

---

**END OF INTEGRATION GUIDE**
