# CORRECTION SUMMARY - Lookup Operations Error Handling

**Date:** 2026-01-27  
**Issue:** Incorrect error handling for lookup operations  
**Status:** ✅ FIXED

---

## ISSUE IDENTIFIED

**Question:** Why throw an exception when GetLocationsByDto fails instead of simply continuing?

**Original (INCORRECT) Implementation:**
```csharp
HttpResponseSnapshot locationResponse = await GetLocationsByDtoFromDownstream(request, sessionId);

if (!locationResponse.IsSuccessStatusCode)
{
    throw new DownStreamApiFailureException(...); // ❌ STOPS entire operation
}

GetLocationsByDtoApiResDTO? locationData = Deserialize(...);

if (locationData == null)
{
    throw new NoResponseBodyException(...); // ❌ STOPS entire operation
}

// Continue to GetInstructionSetsByDto...
```

**Problem:**
- Lookup failure stops the entire work order creation
- User can't create work order even if lookup data is optional
- Doesn't match Boomi behavior

---

## ROOT CAUSE ANALYSIS

### Boomi Process Behavior

**Branch Structure (shape4):**
```
Path 1: FsiLogin → Writes SessionId
Path 2: Lookup Subprocess → Writes CategoryId, DisciplineId, PriorityId
Path 3: GetLocationsByDto → Writes LocationID, BuildingID
Path 4: GetInstructionSetsByDto → Writes InstructionId
  ↓
CONVERGENCE (shape6 - stop with continue=true)
  ↓
Continue to CreateBreakdownTask
```

**Key Findings:**
1. ✅ **NO decision shapes check status codes after lookup operations**
2. ✅ **Path flow:** message → connectoraction → documentproperties → stop (continue=true)
3. ✅ **Branch convergence:** ALL paths converge at shape6 regardless of individual results
4. ✅ **Continuation:** Process continues to CreateBreakdownTask with whatever properties are available

**Boomi Behavior When Lookup Fails:**
- SOAP call returns 404/500 error
- Response is empty/null
- documentproperties shape tries to extract fields → Gets empty values
- Properties are set to empty string
- Path continues to convergence (shape6)
- Process continues to CreateBreakdownTask
- CreateBreakdownTask uses empty values (CAFM may accept or reject)

**Boomi Behavior When Lookup Succeeds:**
- SOAP call returns 200
- Response contains data
- documentproperties shape extracts fields
- Properties are set with actual values
- Path continues to convergence (shape6)
- Process continues to CreateBreakdownTask
- CreateBreakdownTask uses populated values

---

## CORRECTED IMPLEMENTATION

**Fixed (CORRECT) Implementation:**
```csharp
// Step 1: Get Location IDs (continue even if fails)
HttpResponseSnapshot locationResponse = await GetLocationsByDtoFromDownstream(request, sessionId);

string locationId = string.Empty;
string buildingId = string.Empty;

if (!locationResponse.IsSuccessStatusCode)
{
    _logger.Warn($"GetLocationsByDto failed: {locationResponse.StatusCode} - Continuing with empty location IDs");
    // ✅ Continue with empty values
}
else
{
    GetLocationsByDtoApiResDTO? locationData = SOAPHelper.DeserializeSoapResponse<GetLocationsByDtoApiResDTO>(locationResponse.Content!);
    
    if (locationData == null)
    {
        _logger.Warn("GetLocationsByDto returned empty response - Continuing with empty location IDs");
        // ✅ Continue with empty values
    }
    else
    {
        locationId = locationData.LocationId ?? string.Empty;
        buildingId = locationData.BuildingId ?? string.Empty;
        _logger.Info($"Location IDs retrieved: LocationId={locationId}, BuildingId={buildingId}");
    }
}

// ✅ Continue to Step 2 regardless of Step 1 result
// Step 2: Get Instruction Set ID (continue even if fails)
HttpResponseSnapshot instructionResponse = await GetInstructionSetsByDtoFromDownstream(request, sessionId);

string instructionId = string.Empty;

if (!instructionResponse.IsSuccessStatusCode)
{
    _logger.Warn($"GetInstructionSetsByDto failed: {instructionResponse.StatusCode} - Continuing with empty instruction ID");
    // ✅ Continue with empty value
}
else
{
    GetInstructionSetsByDtoApiResDTO? instructionData = SOAPHelper.DeserializeSoapResponse<GetInstructionSetsByDtoApiResDTO>(instructionResponse.Content!);
    
    if (instructionData == null)
    {
        _logger.Warn("GetInstructionSetsByDto returned empty response - Continuing with empty instruction ID");
        // ✅ Continue with empty value
    }
    else
    {
        instructionId = instructionData.InstructionId ?? string.Empty;
        _logger.Info($"Instruction ID retrieved: InstructionId={instructionId}");
    }
}

// ✅ Continue to Step 3 regardless of Step 2 result
// Step 3: Create Breakdown Task (with whatever lookup data we have)
HttpResponseSnapshot createResponse = await CreateBreakdownTaskInDownstream(
    request, 
    sessionId, 
    locationId,      // ✅ May be empty if lookup failed
    buildingId,      // ✅ May be empty if lookup failed
    instructionId    // ✅ May be empty if lookup failed
);

// ✅ CreateBreakdownTask validates and fails if CAFM requires missing fields
if (!createResponse.IsSuccessStatusCode)
{
    throw new DownStreamApiFailureException(...); // ✅ Throw here (main operation)
}
```

---

## COMPARISON: BEFORE vs AFTER

### Before (INCORRECT)

**Behavior:**
- GetLocationsByDto fails → Throw exception → Stop
- GetInstructionSetsByDto never called
- CreateBreakdownTask never called
- User gets error: "Failed to get locations"

**Problems:**
- ❌ Too strict: Lookup failure stops entire operation
- ❌ Poor UX: Can't create work order even if location is optional
- ❌ Doesn't match Boomi: Boomi continues with empty values
- ❌ Wrong error location: Error says "GetLocationsByDto failed" instead of "CreateBreakdownTask failed due to missing LocationId"

### After (CORRECT)

**Behavior:**
- GetLocationsByDto fails → Log warning → Set empty values → Continue
- GetInstructionSetsByDto called (may succeed or fail)
- CreateBreakdownTask called with available data
- If CAFM requires LocationId → CreateBreakdownTask fails with proper error
- If CAFM accepts empty LocationId → Work order created successfully

**Benefits:**
- ✅ Resilient: Lookup failures don't stop operation
- ✅ Best-effort: Try to get data, but continue if unavailable
- ✅ Proper error location: If LocationId required, CreateBreakdownTask fails (not lookup)
- ✅ Matches Boomi: Same behavior as original process
- ✅ Better UX: User gets work order created (if possible) or proper error from CAFM

---

## WHICH OPERATIONS SHOULD THROW vs CONTINUE?

### Operations That MUST Succeed (Throw on Failure)

| Operation | Reason | Action on Failure |
|---|---|---|
| **Login (FsiLogin)** | Required for all operations | Throw exception (auth required) |
| **CreateBreakdownTask** | Main operation | Throw exception (operation failed) |

### Operations That Are Best-Effort (Continue on Failure)

| Operation | Reason | Action on Failure |
|---|---|---|
| **GetLocationsByDto** | Enrichment/lookup | Log warning, set empty, continue |
| **GetInstructionSetsByDto** | Enrichment/lookup | Log warning, set empty, continue |
| **Lookup Subprocess** | Enrichment/lookup | Log warning, set empty, continue |
| **CreateEvent** | Optional linking | Log warning, continue (task already created) |
| **Logout (FsiLogout)** | Cleanup only | Log error, continue (non-critical) |

---

## VALIDATION STRATEGY

### Where Validation Happens

**Lookup Operations:**
- ✅ Validate request parameters (SessionId required)
- ✅ Call SOAP API
- ✅ Log warning if fails
- ✅ Set empty values
- ✅ Continue to next operation
- ❌ DON'T throw exception
- ❌ DON'T stop process

**CreateBreakdownTask:**
- ✅ Validate request parameters (SessionId, CallId, RaisedDateUtc required)
- ✅ Call SOAP API with all fields (populated or empty)
- ✅ CAFM validates required fields
- ✅ If CAFM rejects (missing required fields) → Returns 400/500 error
- ✅ Throw DownStreamApiFailureException (proper error from CAFM)
- ✅ User gets meaningful error: "CreateBreakdownTask failed: Missing required field LocationId"

**Result:**
- Validation happens at the right place (CreateBreakdownTask, not lookups)
- Error messages are accurate (CAFM's validation error, not generic lookup error)
- User understands what's actually required

---

## BOOMI PATTERN: BRANCH CONVERGENCE

### Understanding Branch Convergence

**Pattern:**
```
BRANCH (multiple paths)
  ↓ Path 1 (may succeed or fail)
  ↓ Path 2 (may succeed or fail)
  ↓ Path 3 (may succeed or fail)
  ↓
CONVERGENCE (stop with continue=true)
  ↓
Continue to next operation
```

**Meaning:**
- Each branch path executes independently
- Paths may succeed (set properties) or fail (properties remain empty)
- ALL paths converge at convergence point
- Process continues regardless of individual path results
- Next operation uses whatever properties are available

**Azure Implementation:**
- Execute each lookup sequentially
- If lookup fails: Log warning, set empty, continue
- If lookup succeeds: Extract data, set properties, continue
- After all lookups: Continue to main operation
- Main operation validates and fails if required data missing

---

## FILES UPDATED

### 1. CreateBreakdownTaskHandler.cs

**Changes:**
- Removed `throw new DownStreamApiFailureException()` for lookup failures
- Removed `throw new NoResponseBodyException()` for empty lookup responses
- Added `_logger.Warn()` for lookup failures
- Set `locationId`, `buildingId`, `instructionId` to empty string on failure
- Continue to CreateBreakdownTask with available data

**Lines Changed:** 38 insertions, 38 deletions

### 2. BOOMI_EXTRACTION_PHASE1.md

**Changes:**
- Added critical error handling behavior note in Section 13 (Sequence Diagram)
- Updated Section 4 (Operation Response Analysis) with error handling details
- Updated Section 6 (HTTP Status Codes) with best-effort vs must-succeed classification
- Updated Section 15 (Critical Patterns) with Azure implementation example

**Lines Changed:** 104 insertions, 15 deletions

---

## IMPACT ANALYSIS

### Functional Impact

**Before Fix:**
- Lookup failure → Entire operation fails
- User can't create work order if lookup fails
- Error message: "Failed to get locations from CAFM system"

**After Fix:**
- Lookup failure → Operation continues with empty values
- User can create work order (if CAFM accepts empty lookup fields)
- Error message (if CAFM rejects): "Failed to create breakdown task: Missing required field LocationId" (more accurate)

### Error Handling Flow

**Scenario 1: All Lookups Succeed**
```
GetLocationsByDto → Success (LocationId=101, BuildingId=202)
GetInstructionSetsByDto → Success (InstructionId=303)
CreateBreakdownTask → Success (Work order created)
Result: ✅ Success
```

**Scenario 2: GetLocationsByDto Fails, GetInstructionSetsByDto Succeeds**
```
GetLocationsByDto → Fails (404) → Log warning → locationId="", buildingId=""
GetInstructionSetsByDto → Success (InstructionId=303)
CreateBreakdownTask → Calls CAFM with empty location IDs
  └─→ If CAFM accepts empty location: Success ✅
  └─→ If CAFM requires location: Returns 400 error → Throw exception ❌
Result: Depends on CAFM validation rules
```

**Scenario 3: All Lookups Fail**
```
GetLocationsByDto → Fails (404) → Log warning → locationId="", buildingId=""
GetInstructionSetsByDto → Fails (404) → Log warning → instructionId=""
CreateBreakdownTask → Calls CAFM with empty lookup IDs
  └─→ If CAFM accepts empty lookups: Success ✅
  └─→ If CAFM requires lookups: Returns 400 error → Throw exception ❌
Result: Depends on CAFM validation rules
```

### Benefits

✅ **Resilient:** System doesn't fail prematurely on lookup errors  
✅ **Flexible:** CAFM system controls validation (not System Layer)  
✅ **Accurate errors:** Error messages reflect actual problem (missing required field in CreateBreakdownTask, not lookup failure)  
✅ **Matches Boomi:** Same behavior as original Boomi process (branch convergence pattern)  
✅ **Better UX:** User gets work order created if possible, or meaningful error from CAFM

---

## COMMITS

**Total Commits for This Fix:** 3

1. **cb5c1a4:** Initial refactor (removed pyramid of doom, used guard clauses)
2. **d51bdc5:** Critical fix (lookup operations continue on failure, not throw)
3. **89f23f6:** Updated Phase 1 document (clarified error handling behavior)

---

## CONCLUSION

**✅ The flow is now CORRECT and matches the Boomi extraction analysis.**

**Key Takeaway:**
- Lookup operations are **best-effort enrichment**
- They should **continue even if they fail** (set empty values)
- Validation happens at **CreateBreakdownTask level** (CAFM validates required fields)
- This matches Boomi's **branch convergence pattern** (all paths converge regardless of results)

**Updated Documents:**
- ✅ `CreateBreakdownTaskHandler.cs` - Fixed implementation
- ✅ `BOOMI_EXTRACTION_PHASE1.md` - Clarified error handling behavior
- ✅ `CORRECTION_SUMMARY.md` - This document

---

**END OF CORRECTION SUMMARY**
