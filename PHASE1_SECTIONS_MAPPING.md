# PHASE 1 SECTIONS MAPPING - What's Used for Code Generation

**Purpose:** Clarify which Phase 1 sections are CRITICAL for code generation vs analysis/documentation only.

---

## SECTION CLASSIFICATION

### 🔴 CRITICAL FOR CODE GENERATION (MUST READ)

These sections are MANDATORY inputs for code generation. Developers MUST read these to generate correct code.

| Section | Title | Purpose for Code Generation | What Developer Needs |
|---|---|---|---|
| **2** | Input Structure Analysis | **DTO structure** (ReqDTO fields, nested objects, arrays) | Field names, data types, required fields → Generate ReqDTO classes |
| **3** | Response Structure Analysis | **DTO structure** (ResDTO fields) | Field names, data types → Generate ResDTO classes |
| **13.1** | Operation Classification Table | **Error handling strategy** for EACH operation | Classification (throw vs continue) → Implement error handling |
| **13.2** | Enhanced Sequence Diagram | **Complete execution flow** with error handling | Operation order, error handling, result variables → Generate Handler code |
| **18** | Function Exposure Decision Table | **Which operations to create** (Functions vs Atomic Handlers) | Which operations are Azure Functions, which are internal → Generate Functions/Handlers/Atomics |

**Code Generation Workflow:**
```
Read Section 2 → Generate ReqDTO (field names, types, validation)
Read Section 3 → Generate ResDTO (field names, types, Map() method)
Read Section 18 → Determine which operations to create (Functions vs Atomics)
Read Section 13.1 → Classify each operation (error handling strategy)
Read Section 13.2 → Generate Handler code (exact flow, error handling, results)
```

---

### 🟡 IMPORTANT FOR UNDERSTANDING (SHOULD READ)

These sections provide context and help understand the flow, but are not directly used for code generation.

| Section | Title | Purpose | What Developer Gets |
|---|---|---|---|
| **1** | Operations Inventory | Lists all operations | Overview of what operations exist |
| **4** | Operation Response Analysis | What each operation produces | Understanding of data dependencies |
| **5** | Map Analysis | SOAP field mappings | Field names for SOAP envelopes (if SOAP) |
| **6** | HTTP Status Codes | Return paths and status codes | Understanding of success/error scenarios |
| **7** | Process Properties | Properties written/read | Understanding of data flow |
| **8** | Data Dependency Graph | Dependencies between operations | Understanding of execution order |
| **9** | Control Flow Graph | Dragpoint connections | Understanding of Boomi flow |
| **10** | Decision Analysis | Decision shapes and paths | Understanding of conditional logic |
| **11** | Branch Analysis | Branch paths and convergence | Understanding of parallel/sequential execution |
| **12** | Execution Order | Business logic flow | Understanding of why operations execute in order |
| **15** | Critical Patterns | Boomi patterns identified | Understanding of patterns (check-before-create, etc.) |

**These provide CONTEXT but are not directly translated to code.**

---

### 🟢 DOCUMENTATION ONLY (OPTIONAL READ)

These sections are for documentation, verification, and understanding. Not needed for code generation.

| Section | Title | Purpose | Usage |
|---|---|---|---|
| **16** | HTTP Status Codes and Return Paths | Detailed return path analysis | Understanding of error responses |
| **17** | Request/Response JSON Examples | Example JSON | Understanding of data format |
| **19** | Request/Response JSON Examples | More examples | Documentation |
| **14** | Subprocess Analysis | Subprocess internal flows | Understanding of subprocesses |
| **20** | Self-Check Validation | Validation checklist | Quality assurance |
| **21** | Phase 1 Completion Checklist | Completion verification | Quality assurance |
| **22** | Ready for Phase 2 | Readiness confirmation | Workflow gate |

**These are for verification, not code generation.**

---

## DETAILED MAPPING: SECTION → CODE COMPONENT

### Section 2 (Input Structure) → ReqDTO

**What's in Section 2:**
- Request profile structure
- Field names and data types
- Array detection (minOccurs, maxOccurs)
- Nested objects
- Field mapping table (Boomi → Azure)

**What Developer Generates:**
```csharp
// From Section 2 field mapping table
public class CreateBreakdownTaskReqDTO : IRequestSysDTO
{
    public string ReporterName { get; set; } = string.Empty; // From field mapping
    public string ReporterEmail { get; set; } = string.Empty; // From field mapping
    public TicketDetailsDTO? TicketDetails { get; set; } // Nested object from structure
    
    public void ValidateAPIRequestParameters()
    {
        // Required fields from Section 2 (allowEmpty="false")
        if (string.IsNullOrWhiteSpace(ServiceRequestNumber))
            errors.Add("ServiceRequestNumber is required.");
    }
}

// Nested object from Section 2 structure
public class TicketDetailsDTO
{
    public string ScheduledDate { get; set; } = string.Empty;
    public string RaisedDateUtc { get; set; } = string.Empty; // Required field
}
```

---

### Section 3 (Response Structure) → ResDTO

**What's in Section 3:**
- Response profile structure
- Field names and data types
- Response field mapping table

**What Developer Generates:**
```csharp
// From Section 3 field mapping table
public class CreateBreakdownTaskResDTO
{
    public string BreakdownTaskId { get; set; } = string.Empty; // From field mapping
    public string CallId { get; set; } = string.Empty; // From field mapping
    public string Status { get; set; } = string.Empty; // From field mapping
    
    public static CreateBreakdownTaskResDTO Map(CreateBreakdownTaskApiResDTO apiResponse)
    {
        // Map ApiResDTO → ResDTO
        return new CreateBreakdownTaskResDTO
        {
            BreakdownTaskId = apiResponse.BreakdownTaskId ?? string.Empty,
            CallId = apiResponse.CallId ?? string.Empty,
            Status = "Success"
        };
    }
}
```

---

### Section 13.1 (Classification Table) → Error Handling Strategy

**What's in Section 13.1:**
- Classification for EACH operation
- Error handling for EACH operation (throw vs continue)
- Reason for classification
- Boomi references

**What Developer Gets:**
```
Operation: GetLocationsByDto
Classification: BEST-EFFORT LOOKUP
Error Handling: Log warning, set empty, continue
Reason: Branch convergence, no decision checks
Boomi Reference: Branch path 3 converges at shape6
```

**What Developer Implements:**
```csharp
// From Section 13.1: Classification = BEST-EFFORT LOOKUP
// Error Handling: Log warning, set empty, continue

HttpResponseSnapshot locationResponse = await GetLocationsByDtoFromDownstream(request, sessionId);

string locationId = string.Empty;
string buildingId = string.Empty;

if (!locationResponse.IsSuccessStatusCode)
{
    _logger.Warn($"GetLocationsByDto failed - Continuing with empty values"); // Log warning
    // Continue with empty (don't throw)
}
else
{
    // Extract values
    locationId = Extract(...);
    buildingId = Extract(...);
}

// Continue to next operation (don't throw exception)
```

---

### Section 13.2 (Enhanced Sequence Diagram) → Handler Implementation

**What's in Section 13.2:**
- Complete execution flow
- Classification for EACH operation
- Error handling for EACH operation
- Result for EACH operation
- Code hints for EACH operation

**What Developer Gets:**
```
├─→ Path 3: GetLocationsByDto (shape23-24-25) (Downstream - SOAP) - (BEST-EFFORT LOOKUP)
|    └─→ READS: [process.DPP_SessionId, propertyName, unitCode from input]
|    └─→ WRITES: [process.DPP_LocationID, process.DPP_BuildingID]
|    └─→ HTTP: [Expected: 200, Error: 404/500]
|    └─→ ERROR HANDLING: If fails → Log warning, set empty values, CONTINUE
|    └─→ RESULT: locationId, buildingId (populated or empty)
|    └─→ BOOMI: Branch path 3 (shape23-24-25) converges at shape6 (no decision checks)
|    └─→ CODE: if (!response.IsSuccessStatusCode) { _logger.Warn(...); locationId = string.Empty; } else { extract(...); }
```

**What Developer Implements:**
```csharp
public async Task<BaseResponseDTO> HandleAsync(CreateBreakdownTaskReqDTO request)
{
    string? sessionId = RequestContext.GetSessionId(); // From READS
    
    // Step 1: GetLocationsByDto - (BEST-EFFORT LOOKUP)
    HttpResponseSnapshot locationResponse = await GetLocationsByDtoFromDownstream(request, sessionId);
    
    string locationId = string.Empty; // RESULT: (populated or empty)
    string buildingId = string.Empty;
    
    if (!locationResponse.IsSuccessStatusCode) // ERROR HANDLING: If fails
    {
        _logger.Warn("GetLocationsByDto failed - Continuing with empty values"); // Log warning
        // Continue with empty (don't throw) - CONTINUE
    }
    else
    {
        locationId = Extract(...); // Extract if success
        buildingId = Extract(...);
    }
    
    // Continue to Step 2 (from sequence diagram)
    // Step 2: GetInstructionSetsByDto - (BEST-EFFORT LOOKUP)
    // ... (same pattern)
    
    // Step 3: CreateBreakdownTask - (MAIN OPERATION)
    HttpResponseSnapshot createResponse = await CreateBreakdownTaskInDownstream(...);
    
    if (!createResponse.IsSuccessStatusCode) // ERROR HANDLING: If fails
    {
        throw new DownStreamApiFailureException(...); // Throw exception - STOP
    }
    
    // ... rest of implementation
}
```

**Developer follows Section 13.2 line by line to generate Handler code.**

---

### Section 18 (Function Exposure) → Which Components to Create

**What's in Section 18:**
- Decision table for EACH operation
- Classification: Azure Function OR Atomic Handler
- Reasoning for each decision

**What Developer Gets:**
```
Operation: GetBreakdownTasksByDto
Independent Invoke? YES
Conclusion: Azure Function
Reasoning: Process Layer needs to check existence independently

Operation: GetLocationsByDto
Independent Invoke? NO
Conclusion: Atomic Handler (Internal)
Reasoning: Internal lookup for CreateBreakdownTask
```

**What Developer Creates:**
```
✅ Create: GetBreakdownTasksByDtoAPI.cs (Azure Function)
✅ Create: GetBreakdownTasksByDtoHandler.cs (Handler)
✅ Create: GetBreakdownTasksByDtoAtomicHandler.cs (Atomic Handler)

✅ Create: GetLocationsByDtoAtomicHandler.cs (Atomic Handler - internal only)
❌ DON'T Create: GetLocationsByDtoAPI.cs (NOT an Azure Function)
```

---

## WHAT'S MISSING IN CURRENT PROMPT

**The prompt doesn't clearly state:**

1. ❌ Which sections are CRITICAL for code generation (2, 3, 13.1, 13.2, 18)
2. ❌ Which sections are CONTEXT only (4, 7, 8, 9, 10, 11, 12, 15)
3. ❌ Which sections are DOCUMENTATION only (14, 16, 17, 19, 20, 21, 22)
4. ❌ How to use each section (Section 2 → ReqDTO, Section 13.1 → Error handling, etc.)
5. ❌ What to read in what order (Section 2 → 3 → 18 → 13.1 → 13.2)

**This causes:**
- Developers might read ALL sections (overwhelming)
- Developers might skip critical sections (like 13.1)
- Developers might not understand what each section is for
- Ambiguity about which sections to use for code generation

---

## WHAT SHOULD BE ADDED TO PROMPT

### NEW SECTION: "PHASE 1 SECTIONS FOR CODE GENERATION"

```markdown
## 📋 PHASE 1 SECTIONS FOR CODE GENERATION (CRITICAL MAPPING)

**🚨 CRITICAL:** Not all Phase 1 sections are used for code generation. This table shows which sections to read and what to generate from each.

### CRITICAL SECTIONS (MUST READ FOR CODE GENERATION)

| Section | Title | Read For | Generate |
|---|---|---|---|
| **2** | Input Structure Analysis | Request DTO structure | **ReqDTO classes** (field names, types, nested objects, validation) |
| **3** | Response Structure Analysis | Response DTO structure | **ResDTO classes** (field names, types, Map() method) |
| **5** | Map Analysis | SOAP field mappings | **SOAP envelopes** (field names, element names, namespaces) |
| **13.1** | Operation Classification Table | Error handling strategy | **Handler error handling** (throw vs continue for EACH operation) |
| **13.2** | Enhanced Sequence Diagram | Complete execution flow | **Handler implementation** (operation order, error handling, results) |
| **18** | Function Exposure Decision Table | Which operations to expose | **Functions, Handlers, Atomic Handlers** (which to create) |

### CONTEXT SECTIONS (READ FOR UNDERSTANDING, NOT CODE GENERATION)

| Section | Title | Purpose | Usage |
|---|---|---|---|
| **1** | Operations Inventory | Overview of operations | Understanding what exists |
| **4** | Operation Response Analysis | Data dependencies | Understanding why operations execute in order |
| **7** | Process Properties | Properties read/written | Understanding data flow |
| **8** | Data Dependency Graph | Dependencies | Understanding execution order reasoning |
| **9** | Control Flow Graph | Dragpoint connections | Understanding Boomi flow |
| **10** | Decision Analysis | Decision shapes | Understanding conditional logic |
| **11** | Branch Analysis | Branch convergence | Understanding parallel/sequential |
| **12** | Execution Order | Business logic flow | Understanding why operations ordered this way |
| **15** | Critical Patterns | Boomi patterns | Understanding patterns (check-before-create, etc.) |

### DOCUMENTATION SECTIONS (OPTIONAL READ)

| Section | Title | Purpose | Usage |
|---|---|---|---|
| **6** | HTTP Status Codes | Return paths | Understanding error scenarios |
| **14** | Subprocess Analysis | Subprocess flows | Understanding subprocesses |
| **16-17** | JSON Examples | Example data | Understanding data format |
| **19-22** | Validation/Checklist | Quality assurance | Verification only |

---

## CODE GENERATION WORKFLOW (STEP-BY-STEP)

### Step 1: Determine What to Create (Section 18)

**Read:** Section 18 (Function Exposure Decision Table)

**Extract:**
- Which operations are Azure Functions
- Which operations are Atomic Handlers (internal)
- Reasoning for each

**Generate:**
- List of Functions to create
- List of Handlers to create
- List of Atomic Handlers to create

**Example:**
```
Section 18 says:
- GetBreakdownTasksByDto: Azure Function
- CreateBreakdownTask: Azure Function
- GetLocationsByDto: Atomic Handler (internal)
- GetInstructionSetsByDto: Atomic Handler (internal)

Generate:
✅ GetBreakdownTasksByDtoAPI.cs (Function)
✅ CreateBreakdownTaskAPI.cs (Function)
✅ GetLocationsByDtoAtomicHandler.cs (Atomic - internal)
✅ GetInstructionSetsByDtoAtomicHandler.cs (Atomic - internal)
```

---

### Step 2: Generate DTOs (Sections 2 & 3)

**Read:** Section 2 (Input Structure Analysis)

**Extract:**
- Field mapping table (Boomi field → Azure property)
- Required fields (allowEmpty="false")
- Nested objects (ticketDetails)
- Array detection (workOrder array)

**Generate:**
```csharp
// From Section 2 field mapping table
public class CreateBreakdownTaskReqDTO : IRequestSysDTO
{
    // Field 1 from mapping table
    public string ReporterName { get; set; } = string.Empty;
    
    // Field 2 from mapping table
    public string ReporterEmail { get; set; } = string.Empty;
    
    // Nested object from structure analysis
    public TicketDetailsDTO? TicketDetails { get; set; }
    
    public void ValidateAPIRequestParameters()
    {
        // Required fields from Section 2
        if (string.IsNullOrWhiteSpace(ServiceRequestNumber))
            errors.Add("ServiceRequestNumber is required.");
        
        if (TicketDetails == null)
            errors.Add("TicketDetails is required.");
        else if (string.IsNullOrWhiteSpace(TicketDetails.RaisedDateUtc))
            errors.Add("TicketDetails.RaisedDateUtc is required.");
    }
}
```

**Read:** Section 3 (Response Structure Analysis)

**Generate:**
```csharp
// From Section 3 field mapping table
public class CreateBreakdownTaskResDTO
{
    public string BreakdownTaskId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    
    public static CreateBreakdownTaskResDTO Map(CreateBreakdownTaskApiResDTO apiResponse)
    {
        return new CreateBreakdownTaskResDTO
        {
            BreakdownTaskId = apiResponse.BreakdownTaskId ?? string.Empty,
            Status = "Success"
        };
    }
}
```

---

### Step 3: Generate SOAP Envelopes (Section 5 - if SOAP)

**Read:** Section 5 (Map Analysis)

**Extract:**
- Field mappings (source → target)
- Element names (breakdownTaskDto, locationDto, etc.)
- Namespace prefixes
- Scripting functions (date formatting)

**Generate:**
```xml
<!-- From Section 5: Element name = "breakdownTaskDto" -->
<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" xmlns:ns="...">
  <soapenv:Body>
    <ns:CreateBreakdownTask>
      <ns:sessionId>{{SessionId}}</ns:sessionId>
      <ns:breakdownTaskDto>
        <!-- Field names from Section 5 field mappings -->
        <ns:ReporterName>{{ReporterName}}</ns:ReporterName>
        <ns:BDET_EMAIL>{{ReporterEmail}}</ns:BDET_EMAIL>
        <ns:CategoryId>{{CategoryId}}</ns:CategoryId>
      </ns:breakdownTaskDto>
    </ns:CreateBreakdownTask>
  </soapenv:Body>
</soapenv:Envelope>
```

---

### Step 4: Determine Error Handling (Section 13.1)

**Read:** Section 13.1 (Operation Classification Table)

**For EACH operation, extract:**
- Classification (5 types)
- Error handling (throw vs continue)
- Reason
- Boomi reference

**Example:**
```
Operation: GetLocationsByDto
Classification: BEST-EFFORT LOOKUP
Error Handling: Log warning, set empty, continue
Reason: Branch convergence, no decision checks
Boomi Reference: Branch path 3 converges at shape6
```

**Use in Handler:**
```csharp
// Classification: BEST-EFFORT LOOKUP
// Error Handling: Log warning, set empty, continue

if (!locationResponse.IsSuccessStatusCode)
{
    _logger.Warn("GetLocationsByDto failed - Continuing with empty values"); // Log warning
    locationId = string.Empty; // Set empty
    buildingId = string.Empty;
    // Continue (don't throw)
}
else
{
    // Extract values
}

// Continue to next operation
```

---

### Step 5: Generate Handler Code (Section 13.2)

**Read:** Section 13.2 (Enhanced Sequence Diagram)

**For EACH operation in diagram, extract:**
- Classification
- Error handling specification
- Result specification
- Code hint

**Follow diagram line by line:**

```
From diagram:
├─→ STEP 1: GetLocationsByDtoAtomicHandler (BEST-EFFORT LOOKUP)
|    └─→ ERROR HANDLING: If fails → Log warning, set empty values, CONTINUE
|    └─→ RESULT: locationId, buildingId (populated or empty)
|    └─→ CODE: if (!response.IsSuccessStatusCode) { _logger.Warn(...); locationId = string.Empty; } else { extract(...); }

Generate:
// Step 1: GetLocationsByDto (continue even if fails)
HttpResponseSnapshot locationResponse = await GetLocationsByDtoFromDownstream(request, sessionId);

string locationId = string.Empty;
string buildingId = string.Empty;

if (!locationResponse.IsSuccessStatusCode)
{
    _logger.Warn($"GetLocationsByDto failed: {locationResponse.StatusCode} - Continuing with empty location IDs");
}
else
{
    GetLocationsByDtoApiResDTO? locationData = SOAPHelper.DeserializeSoapResponse<GetLocationsByDtoApiResDTO>(locationResponse.Content!);
    
    if (locationData == null)
    {
        _logger.Warn("GetLocationsByDto returned empty response - Continuing with empty location IDs");
    }
    else
    {
        locationId = locationData.LocationId ?? string.Empty;
        buildingId = locationData.BuildingId ?? string.Empty;
    }
}

// Continue to Step 2 (from diagram)
```

**Developer follows diagram sequentially, implementing each step with exact error handling specified.**

---

## READING ORDER FOR CODE GENERATION

**Recommended order:**

```
1. Section 18 (Function Exposure Decision Table)
   └─→ Determine: Which operations to create (Functions vs Atomics)
   └─→ Output: List of components to generate

2. Section 2 (Input Structure Analysis)
   └─→ Generate: ReqDTO classes with field names, types, validation

3. Section 3 (Response Structure Analysis)
   └─→ Generate: ResDTO classes with field names, types, Map() method

4. Section 5 (Map Analysis) - if SOAP
   └─→ Generate: SOAP envelope XML templates with correct field names

5. Section 13.1 (Operation Classification Table)
   └─→ Understand: Error handling strategy for EACH operation

6. Section 13.2 (Enhanced Sequence Diagram)
   └─→ Generate: Handler code following diagram line by line
   └─→ For EACH operation: Implement error handling as specified

7. Context Sections (4, 7-12, 15) - Optional
   └─→ Read for understanding (not code generation)
```

---

## WHAT TO ADD TO MAIN PROMPT

### Addition 1: Section Mapping Table

**Add after "MANDATORY WORKFLOW":**

```markdown
## 📋 PHASE 1 SECTIONS MAPPING (WHAT TO USE FOR CODE GENERATION)

**🚨 CRITICAL:** Phase 1 has 22 sections. Only 6 sections are CRITICAL for code generation.

### CRITICAL FOR CODE GENERATION (MUST READ)

| Section | Title | Generate From This |
|---|---|---|
| **2** | Input Structure Analysis | ReqDTO classes (fields, validation) |
| **3** | Response Structure Analysis | ResDTO classes (fields, Map() method) |
| **5** | Map Analysis | SOAP envelopes (if SOAP) |
| **13.1** | Operation Classification Table | Error handling strategy (throw vs continue) |
| **13.2** | Enhanced Sequence Diagram | Handler implementation (complete flow) |
| **18** | Function Exposure Decision Table | Which components to create (Functions vs Atomics) |

### CONTEXT SECTIONS (READ FOR UNDERSTANDING)

Sections 1, 4, 7-12, 15: Provide context but not directly used for code generation.

### DOCUMENTATION SECTIONS (OPTIONAL)

Sections 6, 14, 16-17, 19-22: Documentation and verification only.

**Code Generation Workflow:**
```
Read Section 18 → Determine what to create
Read Section 2 → Generate ReqDTO
Read Section 3 → Generate ResDTO
Read Section 5 → Generate SOAP envelopes (if SOAP)
Read Section 13.1 → Understand error handling strategy
Read Section 13.2 → Generate Handler code (line by line)
```
```

---

### Addition 2: Phase 1 Completeness Verification

**Add to PHASE 1 workflow (before "Commit"):**

```markdown
**🛑 PHASE 1 COMPLETENESS VERIFICATION (BLOCKING):**

Before committing Phase 1, verify these CRITICAL sections are complete:

- [ ] **Section 2:** Field mapping table complete (all request fields mapped)
- [ ] **Section 3:** Field mapping table complete (all response fields mapped)
- [ ] **Section 5:** Map analysis complete (if SOAP - field names, element names)
- [ ] **Section 13.1:** Operation classification table complete:
  - [ ] ALL operations classified (5 types)
  - [ ] Error handling specified for EACH operation
  - [ ] Reason provided for EACH classification
  - [ ] Boomi references included for EACH operation
- [ ] **Section 13.2:** Enhanced sequence diagram complete:
  - [ ] Classification shown for EACH operation
  - [ ] Error handling shown for EACH operation
  - [ ] Result shown for EACH operation
  - [ ] Boomi references shown for EACH operation
  - [ ] Code hints shown for EACH operation
- [ ] **Section 18:** Function Exposure Decision Table complete (all operations classified)

**If ANY section incomplete → STOP → Complete it → THEN commit Phase 1**

**Verification Question:** Can a developer generate code from Section 13 without making assumptions?
- If YES → Phase 1 is complete
- If NO → Section 13 is incomplete (add missing details)
```

---

### Addition 3: Code Generation Instructions

**Add to PHASE 2 workflow:**

```markdown
**CODE GENERATION INSTRUCTIONS (USE PHASE 1 SECTIONS):**

**Step 1: Read Section 18 (Function Exposure Decision Table)**
- Determine which operations are Azure Functions
- Determine which operations are Atomic Handlers (internal)
- Create list of components to generate

**Step 2: Generate DTOs (Sections 2 & 3)**
- Read Section 2 field mapping table → Generate ReqDTO classes
- Read Section 3 field mapping table → Generate ResDTO classes
- Include nested objects, validation, Map() methods

**Step 3: Generate SOAP Envelopes (Section 5 - if SOAP)**
- Read Section 5 field mappings → Generate XML templates
- Use element names from Section 5 (not generic "dto")
- Use field names from Section 5 (map field names, not profile names)

**Step 4: Generate Handlers (Sections 13.1 & 13.2)**
- Read Section 13.1 → Understand error handling for EACH operation
- Read Section 13.2 → Generate Handler code line by line:
  * For EACH operation: Check classification
  * For EACH operation: Implement error handling as specified
  * For EACH operation: Set result variables as specified
  * Use code hints provided in diagram

**Step 5: Verify Code Matches Phase 1**
- Error handling matches Section 13.1 classification
- Handler flow matches Section 13.2 sequence diagram
- DTOs match Sections 2 & 3 field mappings
- SOAP envelopes match Section 5 field names

**NO ASSUMPTIONS ALLOWED:**
- ❌ Don't assume error handling (read from Section 13.1)
- ❌ Don't assume operation order (read from Section 13.2)
- ❌ Don't assume field names (read from Sections 2, 3, 5)
- ✅ Read Phase 1, implement exactly as specified
```

---

## UPDATED PROMPT STRUCTURE

**Add these sections to main prompt:**

1. **"PHASE 1 SECTIONS MAPPING"** (NEW)
   - Table showing which sections are critical for code generation
   - What to generate from each section
   - Reading order

2. **"PHASE 1 COMPLETENESS VERIFICATION"** (NEW)
   - Checklist for critical sections (2, 3, 5, 13.1, 13.2, 18)
   - Verification question: Can generate code without assumptions?

3. **"CODE GENERATION INSTRUCTIONS"** (NEW)
   - Step-by-step: Which section to read, what to generate
   - Verification: Code matches Phase 1
   - NO ASSUMPTIONS rule

4. **Update PHASE 1 workflow** (EXISTING)
   - Add Section 13.1 and 13.2 requirements
   - Add completeness verification

5. **Update PHASE 2 workflow** (EXISTING)
   - Add code generation instructions
   - Add README as simple user guide

6. **Update FINAL OUTPUT** (EXISTING)
   - Clarify README is simple user guide
   - Add confirmation code matches Phase 1

---

## SUMMARY

**YES, the main prompt MUST be updated to include:**

1. ✅ **Section mapping table** - Which sections are critical for code generation
2. ✅ **What to generate from each section** - Section 2 → ReqDTO, Section 13.1 → Error handling, etc.
3. ✅ **Reading order** - Which sections to read in what order
4. ✅ **Completeness verification** - Checklist for critical sections
5. ✅ **Code generation instructions** - Step-by-step using Phase 1 sections
6. ✅ **NO ASSUMPTIONS rule** - All behavior explicit in Phase 1

**Without these additions:**
- Developers don't know which sections to read
- Developers don't know what to generate from each section
- Developers might skip critical sections (like 13.1)
- Ambiguity remains about error handling

**With these additions:**
- Clear mapping: Section X → Generate Y
- Clear workflow: Read sections in order, generate components
- No ambiguity: All sections have clear purpose
- No assumptions: All behavior explicit

---

**END OF PHASE 1 SECTIONS MAPPING**
