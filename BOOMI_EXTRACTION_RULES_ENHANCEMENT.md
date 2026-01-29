# BOOMI EXTRACTION RULES - CRITICAL ENHANCEMENTS

## Problem Identified

The current BOOMI_EXTRACTION_RULES.mdc has a critical gap that led to incorrect SOAP envelope generation:

**Issue:** The rulebook instructs to extract field names from **profiles**, but doesn't mandate analyzing **maps** to determine actual field names used in SOAP requests.

**Impact:** 
- Profiles define SOAP schema (all possible fields with technical names like BDET_FKEY_CAT_SEQ)
- Maps show actual field names used in transformation (simpler names like CategoryId)
- Without map analysis, SOAP envelopes use wrong field names

**Example:**
- Profile says: `BDET_FKEY_CAT_SEQ`
- Map shows: `CategoryId` (actual field name used)
- Without map analysis → Used `BDET_FKEY_CAT_SEQ` → WRONG ❌
- With map analysis → Used `CategoryId` → CORRECT ✅

---

## Proposed Enhancements

### ENHANCEMENT 1: Add Step 1d - Map Analysis (MANDATORY)

**Location:** Insert after Step 1c (Operation Response Analysis), before Step 2

**New Step:**

```markdown
### STEP 1d: Map Analysis (MANDATORY - FIELD NAME VERIFICATION)

**🛑 EXPLICIT ENFORCEMENT - CRITICAL STEP:**

**YOU CANNOT PROCEED TO PHASE 2 (SOAP ENVELOPE CREATION)** until this step is complete and documented.

**🚨 CRITICAL:** This step identifies ACTUAL field names used in SOAP requests, which may differ from profile field names.

**REQUIRED OUTPUT**: You MUST create a section in Phase 1 document titled:
```
## Map Analysis (Step 1d)
```

This section MUST be created BEFORE proceeding to Phase 2.

**VALIDATION CHECKPOINT**: 
- If Phase 1 document does NOT contain section "Map Analysis (Step 1d)" → Step 1d NOT complete → **CANNOT PROCEED TO PHASE 2**

```
map_analysis = []

FOR EACH map_file in map_files:
  map_json = load_map(map_file)
  map_id = map_json.componentId
  
  # Identify source and target profiles
  from_profile_id = map_json.Map.fromProfile
  to_profile_id = map_json.Map.toProfile
  
  # Determine if this is a SOAP request map (maps to operation request profile)
  is_soap_request_map = check_if_soap_request_map(to_profile_id, operations_by_id)
  
  IF is_soap_request_map:
    # This map shows actual field names used in SOAP request
    soap_field_mappings = []
    
    FOR EACH mapping in map_json.Map.Mappings.Mapping[]:
      # Extract target field name (actual SOAP field name)
      target_name_path = mapping.toNamePath
      target_field_name = extract_field_name_from_path(target_name_path)
      
      # Extract source field name (input field)
      IF mapping.fromType == "profile":
        source_name_path = mapping.fromNamePath
        source_field_name = extract_field_name_from_path(source_name_path)
      ELIF mapping.fromType == "function":
        # This is a process property or function result
        function_step = find_function_step(map_json, mapping.fromFunction)
        source_field_name = get_function_source(function_step)
      ELIF mapping.fromType == "static":
        source_field_name = "STATIC_VALUE"
      
      soap_field_mappings.append({
        "sourceField": source_field_name,
        "targetField": target_field_name,
        "targetPath": target_name_path,
        "mappingType": mapping.fromType
      })
    
    map_analysis.append({
      "mapId": map_id,
      "mapName": map_json.name,
      "fromProfile": from_profile_id,
      "toProfile": to_profile_id,
      "isSOAPRequest": True,
      "fieldMappings": soap_field_mappings
    })

FUNCTION check_if_soap_request_map(profile_id, operations):
  """
  Check if profile is used as request profile for any SOAP operation
  """
  FOR EACH operation in operations.values():
    IF operation.requestProfile == profile_id:
      IF operation.subType in ["http", "soap", "wss"]:
        RETURN True
  RETURN False

FUNCTION extract_field_name_from_path(name_path):
  """
  Extract field name from path like "Envelope/Body/CreateBreakdownTask/breakdownTaskDto/CategoryId"
  Returns: "CategoryId"
  """
  parts = name_path.split("/")
  RETURN parts[-1]  # Last part is field name

FUNCTION get_function_source(function_step):
  """
  Identify source of function result (process property, static value, etc.)
  """
  IF function_step.type == "PropertyGet":
    RETURN function_step.Inputs.Input[0].default  # Process property name
  ELIF function_step.type == "DefinedProcessPropertyGet":
    RETURN function_step.Configuration.DefinedProcessProperty.propertyName
  ELIF function_step.type == "Scripting":
    RETURN "SCRIPTED_VALUE"
  ELSE:
    RETURN "FUNCTION_RESULT"

OUTPUT: Complete map analysis showing actual SOAP field names used
```

**REQUIRED DOCUMENTATION IN PHASE 1:**

You MUST document in Phase 1 section "Map Analysis (Step 1d)":

1. **Map Inventory:**
   - For each map: Map ID, name, source profile, target profile
   - Identify which maps are SOAP request maps

2. **SOAP Request Field Mappings:**
   - For each SOAP request map: Complete list of field mappings
   - Source field (input) → Target field (SOAP request)
   - **CRITICAL:** Target field name is the ACTUAL field name used in SOAP request

3. **Profile vs Map Comparison:**
   - For each operation: Compare profile field names vs map field names
   - **PROOF:** Show where profile says "BDET_FKEY_CAT_SEQ" but map uses "CategoryId"
   - Document ALL discrepancies

4. **Field Name Authority:**
   - **RULE:** Map field names are AUTHORITATIVE for SOAP envelopes
   - **RULE:** Profile field names are for schema reference only
   - **CRITICAL:** ALWAYS use map field names when creating SOAP envelopes

**IF MAP ANALYSIS IS MISSING → Step 1d NOT complete → CANNOT PROCEED TO PHASE 2**

**EXAMPLE DOCUMENTATION:**

```markdown
## Map Analysis (Step 1d)

### Map: CreateBreakdownTask (390614fd-ae1d-496d-8a79-f320c8663049)

**From Profile:** af096014 (EQ+_CAFM_Create_Request)  
**To Profile:** 362c3ec8 (CreateBreakdownTask Request)  
**Type:** SOAP Request Map

**Field Mappings:**

| Source Field | Target Field (SOAP) | Mapping Type | Notes |
|---|---|---|---|
| reporterName | ReporterName | profile | Direct mapping |
| reporterEmail | BDET_EMAIL | profile | Direct mapping |
| reporterPhoneNumber | Phone | profile | Direct mapping |
| serviceRequestNumber | CallId | profile | Direct mapping |
| description | LongDescription | profile | Direct mapping |
| DPP_SessionId | sessionId | function (process property) | From process property |
| DPP_CategoryId | CategoryId | function (process property) | From process property |
| DDP_DisciplineId | DisciplineId | function (process property) | From process property |
| DPP_PriorityId | PriorityId | function (process property) | From process property |
| DPP_InstructionId | InstructionId | function (process property) | From process property |
| DPP_BuildingID | BuildingId | function (process property) | From process property |
| DPP_LocationID | LocationId | function (process property) | From process property |
| scheduledDate + scheduledTimeStart | ScheduledDateUtc | function (scripting) | Date formatting script |
| raisedDateUtc | RaisedDateUtc | function (scripting) | Date formatting script |
| ContractId | ContractId | function (defined property) | From defined property |
| BDET_CALLER_SOURCE_ID | BDET_CALLER_SOURCE_ID | function (defined property) | From defined property |

**CRITICAL FINDING:**
- Profile 362c3ec8 defines field "BDET_FKEY_CAT_SEQ"
- Map 390614fd uses field "CategoryId"
- **AUTHORITATIVE:** Use "CategoryId" in SOAP envelope (from map, NOT profile)

**Profile vs Map Discrepancies:**

| Profile Field Name | Map Field Name (ACTUAL) | Use in SOAP Envelope |
|---|---|---|
| BDET_FKEY_CAT_SEQ | CategoryId | ✅ CategoryId |
| BDET_FKEY_LAB_SEQ | DisciplineId | ✅ DisciplineId |
| BDET_FKEY_PRI_SEQ | PriorityId | ✅ PriorityId |
| BDET_FKEY_BLD_SEQ | BuildingId | ✅ BuildingId |
| BDET_FKEY_LOC_SEQ | LocationId | ✅ LocationId |
| BDET_CONTACT_NAME | ReporterName | ✅ ReporterName |
| BDET_CONTACT_EMAIL | BDET_EMAIL | ✅ BDET_EMAIL |
| BDET_CONTACT_PHONE | Phone | ✅ Phone |
| BDET_COMMENTS | LongDescription | ✅ LongDescription |
| BDET_RAISED_DATE | RaisedDateUtc | ✅ RaisedDateUtc |
| BDET_SCHEDULED_DATE | ScheduledDateUtc | ✅ ScheduledDateUtc |
| IN_SEQ | InstructionId | ✅ InstructionId |

**Conclusion:** ALWAYS use map field names, NOT profile field names, when creating SOAP envelopes.
```
```

---

### ENHANCEMENT 2: Add Map Analysis to Validation Checklist

**Location:** Section "📋 VALIDATION CHECKLIST (MANDATORY)"

**Add to checklist:**

```markdown
### Map Analysis (NEW)
- [ ] ALL map files identified and loaded
- [ ] SOAP request maps identified (maps to operation request profiles)
- [ ] Field mappings extracted from each map
- [ ] Profile vs map field name discrepancies documented
- [ ] Map field names marked as AUTHORITATIVE for SOAP envelopes
- [ ] Scripting functions analyzed (date formatting, concatenation, etc.)
- [ ] Static values identified and documented
- [ ] Process property mappings documented
```

---

### ENHANCEMENT 3: Update "NEVER ASSUME" Section

**Location:** Section "🚫 NEVER ASSUME"

**Add new rule:**

```markdown
18. **NEVER use profile field names for SOAP envelopes without checking maps** - Profiles define schema, maps show actual usage
    - **SELF-CHECK:** Did I analyze maps to verify field names? (Answer: YES/NO)
    - **CRITICAL:** Map field names are AUTHORITATIVE, profile field names are reference only

19. **NEVER skip map analysis** - Maps reveal actual field names, data transformations, and scripting logic
    - **SELF-CHECK:** Did I check ALL maps for SOAP request operations? (Answer: YES/NO)

20. **NEVER assume profile field names match SOAP request field names** - They often differ (BDET_FKEY_CAT_SEQ vs CategoryId)
    - **SELF-CHECK:** Did I compare profile vs map field names? (Answer: YES/NO)
```

---

### ENHANCEMENT 4: Add Map Analysis to Phase 1 Document Structure

**Location:** Section "📋 PHASE 1 DOCUMENT STRUCTURE (MANDATORY SECTIONS)"

**Update section list:**

```markdown
**Every Phase 1 document MUST include these sections in this exact order:**

1. **Operations Inventory** - All operations listed
2. **Process Properties Analysis** - All properties WRITTEN and READ
3. **Map Analysis (Step 1d)** - ⚠️ **NEW MANDATORY SECTION** - SOAP request field mappings
   - **🛑 CRITICAL**: This section MUST exist before Phase 2 (SOAP envelope creation)
   - **🛑 CRITICAL**: Must show profile vs map field name discrepancies
4. **Decision Shape Analysis (Step 7)** - All decisions with TRUE/FALSE paths
   - **🛑 CRITICAL**: This section MUST exist before creating Sequence Diagram
   ...
```

---

### ENHANCEMENT 5: Add Map Analysis Section Template

**Location:** After Section 14 (Field Mapping Analysis)

**Add new section:**

```markdown
### Section 15: Map Analysis (MANDATORY - NEW)

**Must Include:**
- Map inventory (all map files with IDs and names)
- SOAP request map identification (which maps target operation request profiles)
- Complete field mappings for each SOAP request map
- Profile vs map field name comparison table
- Scripting function analysis (date formatting, concatenation, etc.)
- Static value identification
- Process property mappings

**Example Format:**
```markdown
## 15. Map Analysis (Step 1d)

### SOAP Request Maps Inventory

| Map ID | Map Name | From Profile | To Profile | Operation |
|---|---|---|---|---|
| 390614fd | CreateBreakdownTask EQ+_to_CAFM_Create | af096014 | 362c3ec8 | CreateBreakdownTask |

### Map: CreateBreakdownTask (390614fd)

**Field Mappings:**

| Source Field | Source Type | Target Field (SOAP) | Profile Field Name | Discrepancy? |
|---|---|---|---|---|
| reporterName | profile | ReporterName | ReporterName | ✅ Match |
| reporterEmail | profile | BDET_EMAIL | BDET_EMAIL | ✅ Match |
| DPP_CategoryId | function | CategoryId | BDET_FKEY_CAT_SEQ | ❌ DIFFERENT |
| DDP_DisciplineId | function | DisciplineId | BDET_FKEY_LAB_SEQ | ❌ DIFFERENT |

**Scripting Functions:**

| Function | Input | Output | Logic |
|---|---|---|---|
| Function 11 | scheduledDate, scheduledTimeStart | ScheduledDateUtc | Combine date+time, format to ISO with .0208713Z suffix |
| Function 13 | raisedDateUtc | RaisedDateUtc | Format to ISO with .0208713Z suffix |

**Profile vs Map Discrepancies:**

| Profile Field Name | Map Field Name (ACTUAL) | Authority | Use in SOAP |
|---|---|---|---|
| BDET_FKEY_CAT_SEQ | CategoryId | ✅ MAP | CategoryId |
| BDET_FKEY_LAB_SEQ | DisciplineId | ✅ MAP | DisciplineId |

**CRITICAL RULE:** Map field names are AUTHORITATIVE. Use map field names in SOAP envelopes, NOT profile field names.
```

**Validation:**
- [ ] Section 15 (Map Analysis - Step 1d) present
- [ ] All SOAP request maps analyzed
- [ ] Field mappings extracted
- [ ] Profile vs map discrepancies documented
- [ ] Scripting functions analyzed
- [ ] Map field names marked as authoritative
```

---

### ENHANCEMENT 6: Add Pre-Phase 2 Validation Gate

**Location:** Before Phase 2 section (or at end of Phase 1)

**Add validation gate:**

```markdown
## 🛑 PRE-PHASE 2 VALIDATION GATE (MANDATORY)

**YOU CANNOT PROCEED TO PHASE 2 (CODE GENERATION)** until ALL of the following are complete:

### Phase 1 Completion Checklist

**Input/Output Analysis:**
- [ ] Step 1a (Input Structure Analysis) - COMPLETE and DOCUMENTED
- [ ] Step 1b (Response Structure Analysis) - COMPLETE and DOCUMENTED
- [ ] Step 1c (Operation Response Analysis) - COMPLETE and DOCUMENTED
- [ ] **Step 1d (Map Analysis) - COMPLETE and DOCUMENTED** ⚠️ **NEW MANDATORY**

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
- [ ] **Section 15 (Map Analysis) - COMPLETE** ⚠️ **NEW MANDATORY**

**Self-Check Questions:**

1. ❓ Did I analyze ALL map files? (Answer: YES/NO)
   - **REQUIRED:** YES
   - If NO → **STOP** → Analyze all maps → Document in Phase 1

2. ❓ Did I identify SOAP request maps? (Answer: YES/NO)
   - **REQUIRED:** YES
   - If NO → **STOP** → Identify SOAP request maps → Document in Phase 1

3. ❓ Did I extract actual field names from maps? (Answer: YES/NO)
   - **REQUIRED:** YES
   - If NO → **STOP** → Extract field names from maps → Document in Phase 1

4. ❓ Did I compare profile field names vs map field names? (Answer: YES/NO)
   - **REQUIRED:** YES
   - If NO → **STOP** → Compare and document discrepancies → Document in Phase 1

5. ❓ Did I mark map field names as AUTHORITATIVE? (Answer: YES/NO)
   - **REQUIRED:** YES
   - If NO → **STOP** → Mark map field names as authoritative → Document in Phase 1

6. ❓ Did I analyze scripting functions in maps? (Answer: YES/NO)
   - **REQUIRED:** YES (if maps contain scripting)
   - If NO → **STOP** → Analyze scripting functions → Document in Phase 1

**IF ANY ANSWER IS NO → STOP → COMPLETE THAT STEP → THEN PROCEED TO PHASE 2**

**VALIDATION:** Phase 1 document MUST contain Section 15 (Map Analysis) with complete field mapping analysis before proceeding to Phase 2.
```

---

### ENHANCEMENT 7: Update Critical Principles

**Location:** Section "🚨 CRITICAL PRINCIPLES (NEVER VIOLATE)"

**Add new principle:**

```markdown
8. **Map field names are AUTHORITATIVE for SOAP envelopes** - Profiles define schema, maps show actual usage. When profile and map field names differ, ALWAYS use map field names.
   - **ENFORCEMENT**: You MUST analyze maps (Step 1d) before creating SOAP envelopes
   - **VALIDATION**: All SOAP envelopes must use field names from maps, not profiles
   - **EXAMPLE:** Profile says "BDET_FKEY_CAT_SEQ", map uses "CategoryId" → Use "CategoryId"
```

---

### ENHANCEMENT 8: Add Map Analysis Algorithm

**Location:** After Step 1c, before Step 2

**Add detailed algorithm:**

```markdown
### STEP 1d: Map Analysis Algorithm (DETAILED)

**Purpose:** Extract actual field names used in SOAP requests by analyzing Boomi maps.

**Algorithm:**

```
# Step 1: Load all map files
maps_by_id = {}
FOR EACH file in directory:
  IF file matches pattern: map_*.json:
    map_json = load_json(file)
    map_id = map_json.componentId
    maps_by_id[map_id] = map_json

# Step 2: Identify SOAP request maps
soap_request_maps = []
FOR EACH map in maps_by_id.values():
  to_profile_id = map.Map.toProfile
  
  # Check if target profile is used as request profile for SOAP operation
  FOR EACH operation in operations_by_id.values():
    IF operation.requestProfile == to_profile_id:
      IF operation.subType in ["http", "soap", "wss"]:
        soap_request_maps.append({
          "mapId": map.componentId,
          "mapName": map.name,
          "operationId": operation.componentId,
          "operationName": operation.name,
          "fromProfile": map.Map.fromProfile,
          "toProfile": to_profile_id
        })
        BREAK

# Step 3: Extract field mappings from each SOAP request map
FOR EACH soap_map in soap_request_maps:
  map_json = maps_by_id[soap_map.mapId]
  field_mappings = []
  
  FOR EACH mapping in map_json.Map.Mappings.Mapping[]:
    # Extract target field name (SOAP request field)
    target_path = mapping.toNamePath
    # Example: "Envelope/Body/CreateBreakdownTask/breakdownTaskDto/CategoryId"
    target_parts = target_path.split("/")
    target_field_name = target_parts[-1]  # "CategoryId"
    target_element = target_parts[-2]  # "breakdownTaskDto"
    
    # Extract source information
    source_info = extract_source_info(mapping, map_json)
    
    field_mappings.append({
      "sourceField": source_info.field_name,
      "sourceType": source_info.type,
      "targetField": target_field_name,
      "targetElement": target_element,
      "targetPath": target_path
    })
  
  soap_map.field_mappings = field_mappings

# Step 4: Compare with profile field names
FOR EACH soap_map in soap_request_maps:
  profile_id = soap_map.toProfile
  profile_json = profiles_by_id[profile_id]
  
  discrepancies = []
  
  FOR EACH field_mapping in soap_map.field_mappings:
    target_field = field_mapping.targetField
    
    # Find this field in profile
    profile_field = find_field_in_profile(profile_json, target_field)
    
    IF profile_field is None:
      # Field in map but not in profile - unusual but possible
      discrepancies.append({
        "mapField": target_field,
        "profileField": "NOT_FOUND",
        "issue": "Map uses field not in profile"
      })
    ELSE:
      # Check if field names differ
      IF profile_field.technical_name != target_field:
        discrepancies.append({
          "mapField": target_field,
          "profileField": profile_field.technical_name,
          "issue": "Profile uses technical name, map uses simplified name"
        })
  
  soap_map.discrepancies = discrepancies

# Step 5: Analyze scripting functions
FOR EACH soap_map in soap_request_maps:
  map_json = maps_by_id[soap_map.mapId]
  
  IF map_json.Map.Functions exists:
    scripting_functions = []
    
    FOR EACH function_step in map_json.Map.Functions.FunctionStep[]:
      IF function_step.type == "Scripting":
        scripting_functions.append({
          "functionKey": function_step.key,
          "inputs": extract_function_inputs(function_step),
          "outputs": extract_function_outputs(function_step),
          "script": function_step.Configuration.Scripting.ScriptToExecute,
          "language": function_step.Configuration.Scripting.language
        })
    
    soap_map.scripting_functions = scripting_functions

# Step 6: Document findings
OUTPUT: Complete map analysis with field mappings, discrepancies, and scripting functions
```

**FUNCTION extract_source_info(mapping, map_json):**
```
IF mapping.fromType == "profile":
  # Direct field mapping from input profile
  source_path = mapping.fromNamePath
  source_parts = source_path.split("/")
  field_name = source_parts[-1]
  RETURN {
    "field_name": field_name,
    "type": "profile",
    "path": source_path
  }

ELIF mapping.fromType == "function":
  # Mapping from function result (process property, scripting, etc.)
  function_key = mapping.fromFunction
  function_step = find_function_step(map_json, function_key)
  
  IF function_step.type == "PropertyGet":
    # Process property
    property_name = function_step.Inputs.Input[0].default
    RETURN {
      "field_name": property_name,
      "type": "process_property",
      "property": property_name
    }
  
  ELIF function_step.type == "DefinedProcessPropertyGet":
    # Defined process property
    property_name = function_step.Configuration.DefinedProcessProperty.propertyName
    RETURN {
      "field_name": property_name,
      "type": "defined_property",
      "property": property_name
    }
  
  ELIF function_step.type == "Scripting":
    # Scripting function
    output_name = function_step.Outputs.Output.name
    RETURN {
      "field_name": output_name,
      "type": "scripting",
      "script": function_step.Configuration.Scripting.ScriptToExecute
    }
  
  ELSE:
    RETURN {
      "field_name": "FUNCTION_RESULT",
      "type": function_step.type
    }

ELIF mapping.fromType == "static":
  # Static value
  RETURN {
    "field_name": "STATIC_VALUE",
    "type": "static"
  }

ELSE:
  RETURN {
    "field_name": "UNKNOWN",
    "type": mapping.fromType
  }
```

---

### ENHANCEMENT 9: Add Element Name Verification

**Location:** Step 1d (Map Analysis)

**Add element name extraction:**

```markdown
### Element Name Verification (CRITICAL)

**Problem:** SOAP request element names (dto, breakdownTaskDto, locationDto) must match profile exactly.

**Algorithm:**

```
FOR EACH soap_request_map:
  # Extract element name from map target path
  # Example: "Envelope/Body/CreateBreakdownTask/breakdownTaskDto/CategoryId"
  target_path = soap_request_map.field_mappings[0].targetPath
  path_parts = target_path.split("/")
  
  # Element structure: Envelope/Body/{OperationName}/{ElementName}/{FieldName}
  operation_element = path_parts[2]  # "CreateBreakdownTask"
  dto_element = path_parts[3]  # "breakdownTaskDto"
  
  soap_request_map.operationElement = operation_element
  soap_request_map.dtoElement = dto_element

# Document element names
OUTPUT: For each SOAP operation, document the element name used (dto, breakdownTaskDto, locationDto, etc.)
```

**REQUIRED DOCUMENTATION:**

```markdown
### Element Names (CRITICAL)

| Operation | Element Name | Example Path |
|---|---|---|
| CreateBreakdownTask | breakdownTaskDto | Envelope/Body/CreateBreakdownTask/breakdownTaskDto/CategoryId |
| GetLocationsByDto | locationDto | Envelope/Body/GetLocationsByDto/locationDto/BarCode |
| GetInstructionSetsByDto | dto | Envelope/Body/GetInstructionSetsByDto/dto/IN_DESCRIPTION |

**RULE:** Element names MUST match exactly in SOAP envelopes. Do NOT assume generic "dto" for all operations.
```
```

---

### ENHANCEMENT 10: Add Namespace Prefix Verification

**Location:** Step 1d (Map Analysis)

**Add namespace analysis:**

```markdown
### Namespace Prefix Analysis (CRITICAL)

**Problem:** SOAP fields use namespace prefixes (fsi1:, fsi2:) that must match Boomi message shapes.

**Algorithm:**

```
FOR EACH soap_request_map:
  # Check Boomi message shapes that use this operation
  operation_id = soap_request_map.operationId
  
  # Find message shape that builds SOAP request for this operation
  message_shape = find_message_shape_for_operation(operation_id, process_json)
  
  IF message_shape exists:
    # Extract SOAP envelope from message shape
    soap_envelope = message_shape.message.msgTxt
    
    # Extract namespace declarations
    namespaces = extract_namespace_declarations(soap_envelope)
    # Example: {"fsi1": "http://schemas.datacontract.org/2004/07/Fsi.Concept.Contracts.Entities.ServiceModel"}
    
    # Extract field namespace prefixes
    field_prefixes = extract_field_prefixes(soap_envelope)
    # Example: {"BarCode": "fsi1", "IN_DESCRIPTION": "fsi2"}
    
    soap_request_map.namespaces = namespaces
    soap_request_map.field_prefixes = field_prefixes

OUTPUT: Namespace declarations and field prefixes for each SOAP operation
```

**REQUIRED DOCUMENTATION:**

```markdown
### Namespace Prefixes

| Operation | Namespace Declarations | Field Prefix Examples |
|---|---|---|
| GetLocationsByDto | fsi1: Fsi.Concept.Contracts.Entities.ServiceModel | BarCode → fsi1: |
| GetInstructionSetsByDto | fsi2: Fsi.Concept.Tasks.Contracts.Entities | IN_DESCRIPTION → fsi2: |
| CreateBreakdownTask | fsi1: Fsi.Concept.Contracts.Entities.ServiceModel | CategoryId → fsi1: |

**RULE:** Namespace prefixes MUST match Boomi message shapes. Check message shape SOAP envelope for exact namespace declarations and field prefixes.
```
```

---

## Implementation Priority

### P0 (Critical - Must Add)
1. **Step 1d: Map Analysis** - Prevents wrong field names in SOAP envelopes
2. **Pre-Phase 2 Validation Gate** - Ensures map analysis complete before code generation
3. **Update "NEVER ASSUME" section** - Adds map analysis rules

### P1 (High - Should Add)
4. **Element Name Verification** - Ensures correct element names (dto vs breakdownTaskDto)
5. **Namespace Prefix Verification** - Ensures correct namespace prefixes
6. **Update Phase 1 Document Structure** - Adds Section 15 (Map Analysis)

### P2 (Medium - Nice to Have)
7. **Scripting Function Analysis** - Documents data transformations
8. **Update Validation Checklist** - Adds map analysis checks

---

## Example: How Enhancement Would Have Prevented My Mistake

### Without Step 1d (What Happened)

```
1. Read profile 362c3ec8 (CreateBreakdownTask request)
2. See field: BDET_FKEY_CAT_SEQ
3. Create SOAP envelope with: <fsi1:BDET_FKEY_CAT_SEQ>{{CategoryId}}</fsi1:BDET_FKEY_CAT_SEQ>
4. ❌ WRONG - CAFM API doesn't recognize BDET_FKEY_CAT_SEQ
```

### With Step 1d (What Should Happen)

```
1. Read profile 362c3ec8 (CreateBreakdownTask request)
2. See field: BDET_FKEY_CAT_SEQ (profile field name)
3. ⚠️ MANDATORY: Analyze map 390614fd
4. Map shows: DPP_CategoryId → CategoryId (actual SOAP field name)
5. ✅ CORRECT: Create SOAP envelope with: <fsi1:CategoryId>{{CategoryId}}</fsi1:CategoryId>
6. ✅ CAFM API recognizes CategoryId
```

---

## Recommended Changes to BOOMI_EXTRACTION_RULES.mdc

### Change 1: Add Step 1d After Step 1c

**Insert after line ~330 (after Step 1c - Operation Response Analysis):**

```markdown
### STEP 1d: Map Analysis (MANDATORY - FIELD NAME VERIFICATION)

[Insert complete Step 1d algorithm from Enhancement 1]
```

### Change 2: Update Validation Checklist

**Add to "📋 VALIDATION CHECKLIST (MANDATORY)" section:**

```markdown
### Map Analysis (NEW - MANDATORY)
- [ ] ALL map files identified and loaded
- [ ] SOAP request maps identified
- [ ] Field mappings extracted from each map
- [ ] Profile vs map field name discrepancies documented
- [ ] Map field names marked as AUTHORITATIVE
- [ ] Scripting functions analyzed (if present)
- [ ] Element names extracted and documented
- [ ] Namespace prefixes verified
```

### Change 3: Update "NEVER ASSUME" Section

**Add rules 18-20 from Enhancement 3**

### Change 4: Update Phase 1 Document Structure

**Add Section 15 (Map Analysis) to mandatory sections list**

### Change 5: Add Pre-Phase 2 Validation Gate

**Insert before any "Phase 2" references:**

```markdown
## 🛑 PRE-PHASE 2 VALIDATION GATE (MANDATORY)

[Insert complete validation gate from Enhancement 6]
```

### Change 6: Update Critical Principles

**Add principle #8 about map field names being authoritative**

---

## Testing the Enhancement

### Test Case 1: Profile with Technical Names

**Scenario:** Profile uses BDET_FKEY_CAT_SEQ, map uses CategoryId

**Without Enhancement:**
- Agent reads profile → Uses BDET_FKEY_CAT_SEQ → WRONG ❌

**With Enhancement:**
- Agent reads profile → MANDATORY Step 1d → Analyzes map → Uses CategoryId → CORRECT ✅

### Test Case 2: Element Name Variations

**Scenario:** Different operations use different element names (dto, breakdownTaskDto, locationDto)

**Without Enhancement:**
- Agent assumes generic "dto" for all → WRONG ❌

**With Enhancement:**
- Agent analyzes map paths → Extracts element names → Uses correct names → CORRECT ✅

### Test Case 3: Scripting Functions

**Scenario:** Map uses JavaScript to format dates

**Without Enhancement:**
- Agent passes dates as-is → Format mismatch → WRONG ❌

**With Enhancement:**
- Agent analyzes scripting functions → Replicates date formatting → CORRECT ✅

---

## Conclusion

**Current Rulebook:** Good for process flow analysis, but missing critical map analysis step.

**Enhancement Needed:** Add Step 1d (Map Analysis) as MANDATORY step before Phase 2.

**Impact:** Prevents field name mismatches, element name errors, and missing data transformations.

**Priority:** P0 (Critical) - Should be added immediately to prevent future mistakes.

---

**END OF ENHANCEMENT PROPOSAL**
