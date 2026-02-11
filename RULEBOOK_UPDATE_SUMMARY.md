# BOOMI EXTRACTION RULEBOOK UPDATE SUMMARY
## Version 2.1 → 2.2

---

## File Location

**Updated File:** `/workspace/BOOMI_EXTRACTION_RULES_v2.2_UPDATED.mdc`

**Download this file and replace:** `.cursor/rules/BOOMI_EXTRACTION_RULES.mdc` in your repository

---

## What Changed

### Major Addition: Step 1d (Map Analysis)

**Location:** Inserted after Step 1c (Operation Response Analysis), before Step 2

**Purpose:** Extract ACTUAL SOAP field names from Boomi maps (not profiles)

**Why Critical:** 
- Profiles define SOAP schema with technical field names (BDET_FKEY_CAT_SEQ)
- Maps show actual field names used in SOAP requests (CategoryId)
- Without map analysis → Use wrong field names → SOAP requests fail

**What Step 1d Does:**
1. Loads all map files (map_*.json)
2. Identifies SOAP request maps (maps targeting operation request profiles)
3. Extracts field mappings (source → target field names)
4. Compares profile field names vs map field names
5. Documents discrepancies (Profile: BDET_FKEY_CAT_SEQ, Map: CategoryId)
6. Marks map field names as AUTHORITATIVE
7. Analyzes scripting functions (date formatting, etc.)
8. Extracts element names (dto vs breakdownTaskDto)
9. Extracts namespace declarations from message shapes

---

## Changes by Section

### 1. Critical Principles (Line ~13)

**Added Principle #8:**
```
8. Map field names are AUTHORITATIVE for SOAP envelopes
   - Profiles define schema, maps show actual usage
   - When profile and map differ, ALWAYS use map field names
   - Example: Profile says "BDET_FKEY_CAT_SEQ", map uses "CategoryId" → Use "CategoryId"
```

---

### 2. Mandatory Extraction Workflow (Line ~47)

**Added Step 1d after Step 1c (Line ~432):**
- Complete algorithm for map analysis
- Field mapping extraction
- Profile vs map comparison
- Scripting function analysis
- Element name extraction
- Namespace declaration extraction

**Algorithm includes:**
- `extract_source_info()` function
- `find_function_step()` function
- `extract_namespace_declarations()` function

---

### 3. NEVER ASSUME Section (Line ~1857)

**Added 5 new rules (18-22):**

18. Never use profile field names for SOAP envelopes without checking maps
19. Never skip map analysis
20. Never assume profile field names match SOAP request field names
21. Never assume element names (dto vs breakdownTaskDto)
22. Never skip scripting function analysis

---

### 4. Validation Checklist (Line ~1780)

**Added new section:**
```
### Map Analysis (NEW - MANDATORY)
- [ ] ALL map files identified and loaded
- [ ] SOAP request maps identified
- [ ] Field mappings extracted
- [ ] Profile vs map discrepancies documented
- [ ] Map field names marked as AUTHORITATIVE
- [ ] Scripting functions analyzed
- [ ] Element names extracted
- [ ] Namespace declarations extracted
```

---

### 5. Pre-Phase 2 Validation Gate (NEW - Line ~1855)

**Added complete validation gate before Phase 2:**
- Checklist of all Phase 1 steps (1a, 1b, 1c, **1d**, 2-10)
- 8 mandatory self-check questions for map analysis
- Blocks code generation until map analysis complete

**Critical enforcement:**
- Step 1d must be complete before Phase 2
- All self-check questions must be YES
- Phase 1 Section 3 (Map Analysis) must exist

---

### 6. Phase 1 Document Structure (Line ~2195)

**Updated section list:**
- Added Section 3: Map Analysis (NEW MANDATORY)
- Renumbered: Input Structure Analysis → Section 14 (was 13)
- Renumbered: Field Mapping Analysis → Section 15 (was 14)

**New section order:**
1. Operations Inventory
2. Process Properties Analysis
3. **Map Analysis (Step 1d)** ⚠️ NEW
4. Decision Shape Analysis (Step 7)
5. Data Dependency Graph (Step 4)
6. Control Flow Graph (Step 5)
7. Branch Shape Analysis (Step 8)
8. Execution Order (Step 9)
9. Sequence Diagram (Step 10)
10. Subprocess Analysis
11. Critical Patterns Identified
12. Validation Checklist
13. System Layer Identification
14. Input Structure Analysis (renumbered from 13)
15. Field Mapping Analysis (renumbered from 14)

---

### 7. Version Number (Line ~2313)

**Updated:** 2.1 → 2.2

**Changelog:**
```
- Version 2.0: Made rules explicit, mandatory, and non-negotiable
- Version 2.1: Added mandatory Input/Output Structure Analysis (Contract Verification)
- Version 2.2: Added Step 1d (Map Analysis) - CRITICAL for SOAP field name accuracy
  * Maps show ACTUAL field names used in SOAP requests
  * Profiles show SCHEMA field names (technical names)
  * Map field names are AUTHORITATIVE (use in SOAP envelopes)
  * Prevents field name mismatches (CategoryId vs BDET_FKEY_CAT_SEQ)
  * Mandatory before Phase 2 (SOAP envelope creation)
```

---

## Why This Update Matters

### Problem Solved

**Before v2.2:**
- Agent reads profile → Uses BDET_FKEY_CAT_SEQ → WRONG ❌
- No mandate to check maps
- SOAP envelopes use wrong field names
- API calls fail

**After v2.2:**
- Agent reads profile → MANDATORY Step 1d → Analyzes map → Uses CategoryId → CORRECT ✅
- Map analysis is blocking step before Phase 2
- SOAP envelopes use correct field names
- API calls succeed

### Real-World Impact

**CAFM Implementation Example:**
- Without Step 1d: Used 12 wrong field names in CreateBreakdownTask
- With Step 1d: Would have used correct field names from the start
- Saved: Hours of debugging and fixing

### Prevented Errors

**Step 1d would have prevented:**
1. ❌ Using BDET_FKEY_CAT_SEQ instead of CategoryId
2. ❌ Using BDET_CONTACT_NAME instead of ReporterName
3. ❌ Using BDET_COMMENTS instead of LongDescription
4. ❌ Using dto instead of breakdownTaskDto
5. ❌ Missing namespace declarations
6. ❌ Missing date formatting logic

---

## How to Use Updated Rulebook

### For Agents/Developers

**When extracting Boomi process:**

1. Complete Steps 1a, 1b, 1c (as before)
2. **NEW:** Complete Step 1d (Map Analysis) - MANDATORY
   - Load all map files
   - Identify SOAP request maps
   - Extract field mappings
   - Compare profile vs map field names
   - Document discrepancies
   - Mark map field names as AUTHORITATIVE
3. Complete Steps 2-10 (as before)
4. **GATE:** Verify Phase 1 Section 3 (Map Analysis) exists before Phase 2
5. Create SOAP envelopes using map field names (NOT profile field names)

### Key Rules to Remember

**Rule 1:** Map field names are AUTHORITATIVE  
**Rule 2:** Profile field names are schema reference only  
**Rule 3:** ALWAYS check maps before creating SOAP envelopes  
**Rule 4:** Document profile vs map discrepancies  
**Rule 5:** Replicate scripting function logic (date formatting)

---

## Validation Checklist

**Before using updated rulebook, verify:**
- [ ] File downloaded: BOOMI_EXTRACTION_RULES_v2.2_UPDATED.mdc
- [ ] File placed in: .cursor/rules/BOOMI_EXTRACTION_RULES.mdc
- [ ] Old version backed up (optional)
- [ ] Version number shows 2.2
- [ ] Step 1d section exists (after Step 1c)
- [ ] Pre-Phase 2 Validation Gate exists
- [ ] Section 3 (Map Analysis) in Phase 1 Document Structure

---

## Testing the Update

### Test Case: CreateBreakdownTask

**Scenario:** Profile uses BDET_FKEY_CAT_SEQ, map uses CategoryId

**Without Step 1d (v2.1):**
```
1. Read profile 362c3ec8
2. See field: BDET_FKEY_CAT_SEQ
3. Create SOAP: <fsi1:BDET_FKEY_CAT_SEQ>{{CategoryId}}</fsi1:BDET_FKEY_CAT_SEQ>
4. ❌ WRONG - CAFM API doesn't recognize BDET_FKEY_CAT_SEQ
```

**With Step 1d (v2.2):**
```
1. Read profile 362c3ec8
2. See field: BDET_FKEY_CAT_SEQ (profile field name)
3. ⚠️ MANDATORY: Complete Step 1d (Map Analysis)
4. Analyze map 390614fd
5. Map shows: DPP_CategoryId → CategoryId (actual SOAP field name)
6. Create SOAP: <fsi1:CategoryId>{{CategoryId}}</fsi1:CategoryId>
7. ✅ CORRECT - CAFM API recognizes CategoryId
```

---

## Migration Guide

### If You Have Existing Extractions

**Option 1: Re-extract with v2.2**
- Use updated rulebook for new extractions
- Ensures 100% accuracy

**Option 2: Verify Existing Extractions**
- Run Step 1d on existing extractions
- Compare SOAP envelopes against maps
- Fix any discrepancies found

### If Starting New Extraction

**Follow v2.2 workflow:**
1. Steps 1a, 1b, 1c (profiles)
2. **Step 1d (maps)** ← NEW MANDATORY
3. Steps 2-10 (process flow)
4. Pre-Phase 2 gate validation
5. Phase 2 (code generation)

---

## FAQ

### Q1: Why wasn't map analysis in v2.1?

**A:** V2.1 focused on profile analysis (contract verification). Map analysis importance was discovered during CAFM implementation when field name mismatches were found.

### Q2: Is Step 1d really mandatory?

**A:** YES - absolutely mandatory for SOAP integrations. Without it, SOAP envelopes will use wrong field names and API calls will fail.

### Q3: What if my process doesn't use SOAP?

**A:** Step 1d still useful for REST APIs (maps show field transformations), but less critical (REST typically uses profile field names directly).

### Q4: Can I skip Step 1d if I'm confident?

**A:** NO - confidence doesn't replace verification. Step 1d is blocking gate before Phase 2. No exceptions.

### Q5: How long does Step 1d take?

**A:** 10-20 minutes per SOAP operation (load map, extract mappings, compare with profile, document discrepancies).

---

## Support

**Questions about the update?**
- Review BOOMI_EXTRACTION_RULES_ENHANCEMENT.md for detailed rationale
- Review SOAP_FIELD_JUSTIFICATION.md for real-world example
- Review VERIFICATION_REPORT.md for errors prevented by Step 1d

**Issues with the updated rulebook?**
- Check that Step 1d section exists after Step 1c
- Verify Pre-Phase 2 Validation Gate exists
- Confirm version number shows 2.2

---

## Summary

**Updated:** BOOMI_EXTRACTION_RULES.mdc v2.1 → v2.2  
**Key Addition:** Step 1d (Map Analysis) - MANDATORY before Phase 2  
**Impact:** Prevents SOAP envelope field name errors  
**Priority:** P0 (Critical) - Use immediately for all new extractions  
**File Location:** /workspace/BOOMI_EXTRACTION_RULES_v2.2_UPDATED.mdc

**Download this file and replace .cursor/rules/BOOMI_EXTRACTION_RULES.mdc in your repository.**

---

**END OF UPDATE SUMMARY**
