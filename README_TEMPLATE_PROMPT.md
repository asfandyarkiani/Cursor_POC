# INTELLIGENT PROMPT: System Layer README.md Generation Template

**Purpose:** Generate comprehensive README.md documentation for System Layer projects following a standardized format.

**When to Use:** After completing System Layer code generation (Phase 2), create README.md using this template structure.

---

## PROMPT TEMPLATE

```
Generate a comprehensive README.md file for the System Layer project with the following structure:

# [PROJECT NAME] - System Layer

**System of Record:** [SOR_NAME] ([VENDOR])  
**Integration Type:** [REST/SOAP/FTP/DATABASE/etc.]  
**Authentication:** [Session-based/Token-based/Credentials-per-request/API-Key/None]  
**Framework:** .NET 8, Azure Functions v4

---

## OVERVIEW

[Brief description of what this System Layer does]

**Key Operations:**
1. **[Operation1]** - [Purpose]
2. **[Operation2]** - [Purpose]
[List all Azure Functions exposed to Process Layer]

---

## ARCHITECTURE

### API-Led Architecture Layers

```
┌─────────────────────────────────────────────────────────────┐
│ PROCESS LAYER (Orchestration)                               │
│ - Business logic and workflows                              │
│ - Orchestrates multiple System Layer APIs                   │
│ - Implements [key business patterns]                        │
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

---

## SEQUENCE DIAGRAM

### [Operation1] Operation

```
Process Layer
     |
     | POST [route]
     | [request JSON example]
     ↓
[Operation1]API (Function)
     |
     | [Authentication attribute if applicable]
     ↓
[AuthenticationMiddleware if applicable]
     |
     ├─→ [AuthenticateAtomicHandler if applicable]
     |    └─→ [Auth method]: [details]
     |    └─→ Response: [auth response]
     |    └─→ Store in RequestContext
     |
     ↓ Continue to function
     |
I[Domain]Mgmt (Service Interface)
     ↓
[Domain]MgmtService
     ↓
[Operation1]Handler
     |
     ├─→ Read [context data] from RequestContext
     |
     ├─→ [AtomicHandler1]
     |    └─→ [Protocol]: [Operation] ([parameters])
     |    └─→ Response: [response fields]
     |
     ├─→ Deserialize [protocol] response → [Operation1]ApiResDTO
     ├─→ Map ApiResDTO → [Operation1]ResDTO
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

**CRITICAL: For operations with multiple atomic handlers, show ALL steps:**

```
[OperationN]Handler
     |
     ├─→ Read [context] from RequestContext
     |
     ├─→ STEP 1: [AtomicHandler1] (BEST-EFFORT LOOKUP / MAIN OPERATION / etc.)
     |    └─→ [Protocol]: [Operation] ([parameters])
     |    └─→ Response: [fields]
     |    └─→ Error Handling: If fails → [Log warning/Throw exception], [action], [CONTINUE/STOP]
     |    └─→ Result: [variables] (populated or empty)
     |
     ├─→ STEP 2: [AtomicHandler2] (BEST-EFFORT LOOKUP / MAIN OPERATION / etc.)
     |    └─→ [Protocol]: [Operation] ([parameters])
     |    └─→ Response: [fields]
     |    └─→ Error Handling: If fails → [action]
     |    └─→ Result: [variables] (populated or empty)
     |
     ├─→ STEP 3: [AtomicHandler3] (MAIN OPERATION)
     |    └─→ [Any preprocessing like date formatting]
     |    └─→ [Protocol]: [Operation] ([parameters])
     |    └─→ Uses: [variables from previous steps] (may be empty if lookups failed)
     |    └─→ Response: [fields]
     |    └─→ Error Handling: If fails → Throw exception (main operation must succeed)
     |
     ├─→ STEP 4: Conditional [Operation] (if applicable)
     |    └─→ IF [condition]:
     |         └─→ [AtomicHandler]
     |              └─→ [Protocol]: [Operation]
     |              └─→ Response: [fields]
     |    └─→ ELSE:
     |         └─→ Skip [operation]
     |
     ├─→ Map ApiResDTO → [Operation]ResDTO
     └─→ Return BaseResponseDTO
```

[Repeat for each Azure Function exposed]

---

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
│       ├── [Operation2]AtomicHandler.cs         # [Description]
│       └── [LookupN]AtomicHandler.cs            # Internal lookup
├── DTO/
│   ├── [Operation1]DTO/
│   │   ├── [Operation1]ReqDTO.cs
│   │   └── [Operation1]ResDTO.cs
│   ├── AtomicHandlerDTOs/
│   │   ├── [Operation1]HandlerReqDTO.cs
│   │   └── [...]HandlerReqDTO.cs
│   └── DownstreamDTOs/
│       ├── [Operation1]ApiResDTO.cs
│       └── [...]ApiResDTO.cs
├── Functions/
│   ├── [Operation1]API.cs
│   └── [Operation2]API.cs
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
│   ├── [Operation1].xml
│   └── [...]xml
├── Program.cs
├── host.json
├── appsettings.json (placeholder)
├── appsettings.dev.json
├── appsettings.qa.json
├── appsettings.prod.json
└── [ProjectName].csproj
```

---

## AZURE FUNCTIONS EXPOSED

### [For each Azure Function, include:]

### N. [OperationName]

**Route:** `[METHOD] [route_pattern]`

**Purpose:** [What this function does]

**Request:**
```json
[Complete request JSON example with realistic values]
```

**Response:**
```json
[Complete response JSON example]
```

**Authentication:** [Session-based/Token-based/None] ([how it's handled])

**Internal Operations:** (if handler orchestrates multiple atomics)
1. [InternalOp1] ([description] - [error handling strategy])
2. [InternalOp2] ([description] - [error handling strategy])
3. [MainOp] ([description] - [error handling strategy])

**Process Layer Usage:** (if applicable)
```
[Step-by-step guide for Process Layer on how to use this function]
```

---

## AUTHENTICATION

### [Authentication Type]

**Middleware:** [MiddlewareName] (if applicable)

**Flow:**
```
[Step-by-step authentication flow]
1. [Step 1]
2. [Step 2]
3. [Step 3]
[Include storage mechanism, lifecycle, cleanup]
```

**Benefits:**
- ✅ [Benefit 1]
- ✅ [Benefit 2]
- ✅ [Benefit 3]

---

## CONFIGURATION

### AppConfigs (appsettings.json)

```json
{
  "AppConfigs": {
    "[SOR]BaseUrl": "[example_url]",
    "[SOR][Operation]Url": "[example_url]",
    "[Config1]": "[value]",
    "TimeoutSeconds": 50,
    "RetryCount": 0
  }
}
```

### KeyVault Secrets (if applicable)

```json
{
  "KeyVault": {
    "Url": "https://your-keyvault.vault.azure.net/",
    "Secrets": {
      "[SecretKey1]": "[SecretName1]",
      "[SecretKey2]": "[SecretName2]"
    }
  }
}
```

**Secrets Required:**
- `[SecretName1]`: [Description]
- `[SecretName2]`: [Description]

---

## DEPLOYMENT

### Environment Files

- `appsettings.json` - Placeholder (CI/CD replaces with environment-specific)
- `appsettings.dev.json` - Development environment
- `appsettings.qa.json` - QA environment
- `appsettings.prod.json` - Production environment

**CI/CD Process:**
1. Pipeline detects environment (dev/qa/prod)
2. Copies `appsettings.{env}.json` content → `appsettings.json`
3. Deploys to Azure Functions

### Azure Resources Required

- Azure Functions App (.NET 8, Isolated Worker)
- [Azure Key Vault (if using secrets)]
- [Azure Redis Cache (if using caching)]
- Application Insights (for monitoring)

---

## PROCESS LAYER INTEGRATION

### [Key Integration Pattern]

**Scenario:** [Business scenario description]

**Flow:**
```
1. [Process Layer step 1]
   └─→ Request: [example]
   └─→ Response: [example]

2. [Process Layer step 2]
   └─→ [Action based on step 1]
   
[Complete workflow showing how Process Layer uses System Layer functions]
```

**Benefits:**
- ✅ [Benefit 1]
- ✅ [Benefit 2]
- ✅ [Benefit 3]

---

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

### Rationale: [Pattern Name from Boomi]

**From Boomi Analysis:**
- [Key finding 1 from Phase 1 extraction]
- [Key finding 2 from Phase 1 extraction]
- [Key finding 3 from Phase 1 extraction]

**Azure Implementation:**
```csharp
// [Pattern description]
[Code example showing the pattern]
```

**Benefits:**
- ✅ [Benefit 1]
- ✅ [Benefit 2]
- ✅ [Benefit 3]

---

## ERROR HANDLING

### Exception Types

| Exception | HTTP Status | When |
|---|---|---|
| NoRequestBodyException | 400 | Request body is null/empty |
| RequestValidationFailureException | 400 | Request validation fails |
| DownStreamApiFailureException | Varies | [SOR] API call fails |
| NoResponseBodyException | 500 | [SOR] returns empty response |
| [CustomException] | [Status] | [When] |

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
| [XXX_YYYYYY_0002] | [Error message] |
[List all error codes from ErrorConstants.cs]

---

## PERFORMANCE

### Timing Tracking

**Mechanism:** Stopwatch + ResponseHeaders.DSTimeBreakDown

**Example Response Headers:**
```
SYSTotalTime: [total]ms
DSTimeBreakDown: [OP1]:[ms1],[OP2]:[ms2],[OP3]:[ms3]
DSAggregatedTime: [total]ms
```

**Breakdown:**
- [OPERATION1]: [ms]ms ([description])
- [OPERATION2]: [ms]ms ([description])
[List all operations with typical timing]

---

## DEVELOPMENT

### Prerequisites

- .NET 8 SDK
- Azure Functions Core Tools v4
- Visual Studio 2022 or VS Code with Azure Functions extension

### Build

```bash
dotnet restore
dotnet build
```

### Run Locally

```bash
func start
```

### Test

```bash
# [Operation1]
curl -X POST http://localhost:7071/api/[route] \
  -H "Content-Type: application/json" \
  -d '[request_json]'

# [Operation2]
curl -X POST http://localhost:7071/api/[route] \
  -H "Content-Type: application/json" \
  -d @test-[operation]-request.json
```

---

## DEPENDENCIES

### Framework Projects

- `Framework/Core/Core/Core.csproj` - Core framework (exceptions, middlewares, extensions)
- `Framework/Cache/Cache.csproj` - Caching framework (Redis)

### NuGet Packages

- Microsoft.Azure.Functions.Worker ([version])
- Microsoft.Azure.Functions.Worker.Extensions.Http ([version])
- Microsoft.ApplicationInsights.WorkerService ([version])
- [List all major packages with versions]

---

## MONITORING

### Application Insights

**Metrics Tracked:**
- Request duration (SYSTotalTime)
- Downstream operation timing (DSTimeBreakDown)
- Success/failure rates
- Exception counts
- [Auth-specific metrics if applicable]

**Logs:**
- Function entry/exit
- Handler start/completion
- Atomic handler operations
- [Protocol] request/response status
- [Auth lifecycle if applicable]

### Live Metrics

**Enabled:** Yes (host.json: enableLiveMetricsFilters: true)

**Real-time Monitoring:**
- Active requests
- Failed requests
- Server response time
- Dependency calls ([SOR] operations)

---

## FUTURE ENHANCEMENTS

### Potential Additions

1. **[Enhancement1]**
   - [Description]
   - [Implementation notes]

2. **[Enhancement2]**
   - [Description]
   - [Implementation notes]

[List 3-5 potential future enhancements based on TODO placeholders or missing features]

---

## SUPPORT

### Documentation

- `BOOMI_EXTRACTION_PHASE1.md` - Complete Boomi process analysis
- `RULEBOOK_COMPLIANCE_REPORT.md` - Architecture compliance audit
- `.cursor/rules/System-Layer-Rules.mdc` - System Layer architecture rules
- `.cursor/rules/BOOMI_EXTRACTION_RULES.mdc` - Boomi extraction methodology

### Contact

For questions or issues, refer to the architecture documentation or contact the development team.

---

**Version:** 1.0  
**Last Updated:** [DATE]  
**Status:** ✅ Production Ready (pending build validation)
```

---

## CRITICAL REQUIREMENTS FOR SEQUENCE DIAGRAMS

### 1. Show Complete Component Flow

**MANDATORY Elements:**
- Process Layer (caller)
- Azure Function (entry point)
- Middleware (if authentication)
- Service Interface
- Service Implementation
- Handler
- ALL Atomic Handlers (with details)
- Response back to Process Layer

### 2. Show Error Handling Strategy

**For EACH atomic handler call, specify:**
- Operation classification: `(BEST-EFFORT LOOKUP)` or `(MAIN OPERATION)` or `(CONDITIONAL)` or `(CLEANUP)`
- Error handling: `If fails → [Log warning/Throw exception], [set empty/stop], [CONTINUE/STOP]`
- Result: `[variables] (populated or empty)` or `(throws exception)`

**Example:**
```
├─→ STEP 1: GetLocationsByDtoAtomicHandler (BEST-EFFORT LOOKUP)
|    └─→ SOAP: GetLocationsByDto (sessionId + propertyName + unitCode)
|    └─→ Response: LocationId, BuildingId
|    └─→ Error Handling: If fails → Log warning, set empty values, CONTINUE
|    └─→ Result: locationId, buildingId (populated or empty)
```

### 3. Show Conditional Logic

**For conditional operations:**
```
├─→ STEP N: Conditional [Operation]
|    └─→ IF [condition]:
|         └─→ [AtomicHandler]
|              └─→ [Details]
|    └─→ ELSE:
|         └─→ Skip [operation]
```

### 4. Reference Boomi Analysis

**Add note at top of sequence diagram:**
```
**📋 BASED ON:** BOOMI_EXTRACTION_PHASE1.md Section 13 (Sequence Diagram)
**Error Handling:** Derived from Boomi branch convergence pattern (shape6)
```

---

## SECTION ORDERING (MANDATORY)

1. **Title & Metadata** (SOR, Integration Type, Auth, Framework)
2. **OVERVIEW** (What it does, key operations)
3. **ARCHITECTURE** (API-Led layers diagram)
4. **SEQUENCE DIAGRAM** (One per Azure Function - COMPLETE with error handling)
5. **FOLDER STRUCTURE** (Tree view with comments)
6. **AZURE FUNCTIONS EXPOSED** (Detailed API docs per function)
7. **AUTHENTICATION** (If applicable - flow, benefits)
8. **CONFIGURATION** (AppConfigs, KeyVault)
9. **DEPLOYMENT** (Environment files, CI/CD, Azure resources)
10. **PROCESS LAYER INTEGRATION** (Key patterns, workflows)
11. **ERROR HANDLING STRATEGY** (Operation classification, rationale, code examples)
12. **ERROR HANDLING** (Exception types, error response format, error codes)
13. **PERFORMANCE** (Timing tracking, example breakdown)
14. **DEVELOPMENT** (Prerequisites, build, run, test)
15. **DEPENDENCIES** (Framework projects, NuGet packages)
16. **MONITORING** (Application Insights, Live Metrics)
17. **FUTURE ENHANCEMENTS** (Potential additions)
18. **SUPPORT** (Documentation links, contact)
19. **Version & Status** (Version, date, status)

---

## CONTENT GUIDELINES

### What to Include

✅ **Complete sequence diagrams** showing ALL components in the flow  
✅ **Error handling strategy** for each operation (throw vs continue)  
✅ **Realistic examples** (JSON requests/responses with actual field names)  
✅ **Process Layer integration patterns** (how to use the APIs)  
✅ **Configuration guide** (what to configure, where secrets go)  
✅ **Deployment instructions** (environment files, CI/CD process)  
✅ **Operation classification** (must-succeed vs best-effort)  
✅ **Boomi analysis references** (link back to Phase 1 extraction)

### What to Avoid

❌ **Generic placeholders** (use actual operation names, field names)  
❌ **Incomplete flows** (don't skip atomic handlers or error handling)  
❌ **Assumptions about error handling** (verify from Boomi analysis)  
❌ **Missing conditional logic** (show IF/ELSE for conditional operations)  
❌ **Vague descriptions** (be specific about what each component does)

---

## VALIDATION CHECKLIST

Before finalizing README.md, verify:

- [ ] All Azure Functions documented with complete sequence diagrams
- [ ] Error handling strategy shown for EACH atomic handler call
- [ ] Operation classification specified (BEST-EFFORT vs MAIN OPERATION)
- [ ] Conditional logic shown with IF/ELSE
- [ ] Request/response examples are realistic (not generic)
- [ ] Folder structure matches actual project structure
- [ ] Configuration examples show actual config keys
- [ ] Error codes listed match ErrorConstants.cs
- [ ] Sequence diagrams reference BOOMI_EXTRACTION_PHASE1.md
- [ ] Error handling rationale references Boomi patterns (branch convergence, etc.)

---

## EXAMPLE USAGE

**Input to Agent:**
```
Generate README.md for CAFMManagementSystem System Layer project.

Context:
- SOR: CAFM (FSI Concept)
- Integration: SOAP/XML
- Auth: Session-based (Login/Logout via middleware)
- Functions: GetBreakdownTasksByDto, CreateBreakdownTask
- Reference: BOOMI_EXTRACTION_PHASE1.md Section 13 for sequence diagram
- Error Handling: Lookup operations are best-effort (continue on failure), main operations throw on failure

Follow README_TEMPLATE_PROMPT.md format.
```

**Expected Output:**
- Complete README.md with all sections
- Sequence diagrams showing error handling for each step
- Operation classification (BEST-EFFORT vs MAIN OPERATION)
- Boomi analysis references
- Realistic examples

---

## KEY INSIGHTS FROM CAFM IMPLEMENTATION

### What Made This README Effective

1. **Sequence Diagrams with Error Handling:**
   - Not just happy path - shows what happens on failure
   - Specifies: BEST-EFFORT LOOKUP vs MAIN OPERATION
   - Shows: "If fails → Log warning, set empty, CONTINUE"

2. **Operation Classification Table:**
   - MUST-SUCCEED vs BEST-EFFORT
   - Rationale for each classification
   - Action on failure for each operation

3. **Boomi Pattern References:**
   - "Branch Convergence Pattern"
   - Links back to Phase 1 analysis
   - Explains WHY the error handling works this way

4. **Process Layer Integration:**
   - Shows how Process Layer uses the APIs
   - Documents check-before-create pattern
   - Step-by-step workflow

5. **Realistic Examples:**
   - Actual field names (serviceRequestNumber, breakdownTaskId)
   - Actual values (EQ-2025-001, CAFM-2025-12345)
   - Complete JSON structures (not partial)

### What Could Be Improved

1. **Generate AFTER Phase 1 verification** (not during code generation)
2. **Use Phase 1 sequence diagram as source** (not independent creation)
3. **Validate error handling against Boomi JSON** (check for decision shapes)
4. **Cross-reference line numbers** (link to specific code locations)

---

## PROMPT FOR FUTURE AGENTS

**When generating README.md for a System Layer project, use this prompt:**

```
Create README.md for [ProjectName] System Layer following this structure:

INPUTS:
- BOOMI_EXTRACTION_PHASE1.md (Section 13: Sequence Diagram - use as blueprint)
- Implemented code files (for actual folder structure, operation names)
- Phase 1 Section 6 (HTTP Status Codes) for error handling strategy
- Phase 1 Section 15 (Critical Patterns) for Boomi patterns

REQUIREMENTS:
1. Title section with SOR, integration type, auth method, framework
2. Overview with key operations list
3. Architecture diagram (API-Led layers: Process → System → SOR)
4. Sequence diagrams (ONE per Azure Function):
   - Show complete flow: Process Layer → Function → Middleware → Service → Handler → Atomic Handlers → Response
   - For EACH atomic handler: Specify operation classification (BEST-EFFORT vs MAIN OPERATION)
   - For EACH atomic handler: Show error handling (If fails → action, CONTINUE/STOP)
   - For EACH atomic handler: Show result (populated or empty)
   - Include conditional logic with IF/ELSE
5. Folder structure (tree view with comments)
6. Azure Functions Exposed (detailed API docs per function with request/response examples)
7. Authentication section (if applicable - flow, benefits)
8. Configuration section (AppConfigs, KeyVault)
9. Deployment section (environment files, CI/CD, Azure resources)
10. Process Layer Integration (key patterns, workflows)
11. Error Handling Strategy (operation classification table, rationale from Boomi, code examples)
12. Error Handling (exception types, error response format, error codes list)
13. Performance (timing tracking, example breakdown)
14. Development (prerequisites, build, run, test with curl examples)
15. Dependencies (Framework projects, NuGet packages)
16. Monitoring (Application Insights, Live Metrics)
17. Future Enhancements (based on TODO placeholders)
18. Support (documentation links)
19. Version & Status

CRITICAL:
- Use Phase 1 sequence diagram as source of truth
- Verify error handling from Boomi analysis (decision shapes, branch convergence)
- Show operation classification for ALL atomic handlers
- Include realistic examples (actual field names, values)
- Reference Boomi patterns (branch convergence, check-before-create, etc.)

OUTPUT: Complete README.md file following the template structure above.
```

---

**END OF TEMPLATE PROMPT**
