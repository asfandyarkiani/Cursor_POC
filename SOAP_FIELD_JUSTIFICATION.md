# SOAP FIELD JUSTIFICATION
## Evidence-Based Field Name Verification

---

## Question

**On what basis was `<fsi1:LongDescription>{{LongDescription}}</fsi1:LongDescription>` added to CreateBreakdownTask SOAP envelope?**

---

## Answer

**Basis:** Boomi map `390614fd-ae1d-496d-8a79-f320c8663049` (CreateBreakdownTask EQ+_to_CAFM_Create)

**Evidence:** Line 148-152 in map_390614fd-ae1d-496d-8a79-f320c8663049.json

```json
{
  "$": {
    "fromKey": "52",
    "fromKeyPath": "*[@key='1']/*[@key='46']/*[@key='47']/*[@key='48']/*[@key='49']/*[@key='50']/*[@key='52']",
    "fromNamePath": "Root/Object/workOrder/Array/ArrayElement1/Object/description",
    "fromType": "profile",
    "toKey": "275",
    "toKeyPath": "*[@key='1']/*[@key='153']/*[@key='155']/*[@key='161']/*[@key='275']",
    "toNamePath": "Envelope/Body/CreateBreakdownTask/breakdownTaskDto/LongDescription",
    "toType": "profile"
  }
}
```

**Mapping:**
- **Source:** `description` (from input profile)
- **Target:** `LongDescription` (in SOAP request)
- **Path:** `Envelope/Body/CreateBreakdownTask/breakdownTaskDto/LongDescription`

**Conclusion:** `LongDescription` is **CORRECT** ✅ - directly from Boomi map.

---

## Complete Field-by-Field Justification

### CreateBreakdownTask SOAP Envelope - All Fields with Evidence

| # | SOAP Field | Placeholder | Boomi Map Evidence | Map Line | Source Field |
|---|---|---|---|---|---|
| 1 | SessionId | {{SessionId}} | Function 1 → PropertyGet(DPP_SessionId) | 263-289 | DPP_SessionId (process property) |
| 2 | BDET_CALLER_SOURCE_ID | {{BdetCallerSourceId}} | Function 15 → DefinedPropertyGet(BDET_CALLER_SOURCE_ID) | 697-728 | BDET_CALLER_SOURCE_ID (defined property) |
| 3 | BDET_EMAIL | {{ReporterEmail}} | Profile mapping: reporterEmail → BDET_EMAIL | 43-53 | reporterEmail (input) |
| 4 | BuildingId | {{BuildingId}} | Function 6 → PropertyGet(DPP_BuildingID) | 456-494 | DPP_BuildingID (process property) |
| 5 | CallId | {{CallId}} | Profile mapping: serviceRequestNumber → CallId | 67-77 | serviceRequestNumber (input) |
| 6 | CategoryId | {{CategoryId}} | Function 2 → PropertyGet(DPP_CategoryId) | 291-330 | DPP_CategoryId (process property) |
| 7 | ContractId | {{ContractId}} | Function 14 → DefinedPropertyGet(ContractId) | 665-696 | ContractId (defined property) |
| 8 | DisciplineId | {{DisciplineId}} | Function 3 → PropertyGet(DDP_DisciplineId) | 332-373 | DDP_DisciplineId (process property) |
| 9 | InstructionId | {{InstructionId}} | Function 5 → PropertyGet(DPP_InstructionId) | 415-454 | DPP_InstructionId (process property) |
| 10 | LocationId | {{LocationId}} | Function 7 → PropertyGet(DPP_LocationID) | 495-533 | DPP_LocationID (process property) |
| 11 | LongDescription | {{LongDescription}} | Profile mapping: description → LongDescription | 145-155 | description (input) |
| 12 | Phone | {{ReporterPhone}} | Profile mapping: reporterPhoneNumber → Phone | 55-65 | reporterPhoneNumber (input) |
| 13 | PriorityId | {{PriorityId}} | Function 4 → PropertyGet(DPP_PriorityId) | 375-413 | DPP_PriorityId (process property) |
| 14 | RaisedDateUtc | {{RaisedDateUtc}} | Function 13 → Scripting(raisedDateUtc) | 608-664 | raisedDateUtc (input, scripted) |
| 15 | ReporterName | {{ReporterName}} | Profile mapping: reporterName → ReporterName | 30-41 | reporterName (input) |
| 16 | ScheduledDateUtc | {{ScheduledDateUtc}} | Function 11 → Scripting(scheduledDate+scheduledTimeStart) | 535-607 | scheduledDate + scheduledTimeStart (scripted) |

**Total Fields:** 16 - **ALL have Boomi map evidence**

---

## Detailed Evidence for Each Field

### Field 1: SessionId

**Boomi Map Evidence (Lines 263-289):**
```json
{
  "$": {
    "cacheEnabled": "true",
    "cacheOption": "none",
    "category": "ProcessProperty",
    "key": "1",
    "name": "Get Dynamic Process Property",
    "type": "PropertyGet"
  },
  "Inputs": {
    "Input": [
      {
        "$": {
          "default": "DPP_SessionId",
          "key": "1",
          "name": "Property Name"
        }
      }
    ]
  }
}
```

**Mapping (Lines 79-87):**
```json
{
  "$": {
    "fromFunction": "1",
    "fromKey": "3",
    "fromType": "function",
    "toKey": "157",
    "toNamePath": "Envelope/Body/CreateBreakdownTask/sessionId"
  }
}
```

**Conclusion:** ✅ SessionId from process property DPP_SessionId

---

### Field 2: BDET_CALLER_SOURCE_ID

**Boomi Map Evidence (Lines 697-728):**
```json
{
  "$": {
    "cacheEnabled": "true",
    "category": "ProcessProperty",
    "key": "15",
    "name": "Get Process Property",
    "type": "DefinedProcessPropertyGet"
  },
  "Configuration": {
    "DefinedProcessProperty": {
      "$": {
        "componentId": "13a690a7-c480-4c39-8eeb-fe3ecb030bf5",
        "componentName": "PP_Create_Work Order",
        "propertyKey": "5a18fe55-1d7b-4a5c-89e2-3dec92373ce9",
        "propertyName": "BDET_CALLER_SOURCE_ID"
      }
    }
  }
}
```

**Mapping (Lines 233-243):**
```json
{
  "$": {
    "fromFunction": "15",
    "fromKey": "1",
    "fromType": "function",
    "toKey": "397",
    "toNamePath": "Envelope/Body/CreateBreakdownTask/breakdownTaskDto/BDET_CALLER_SOURCE_ID"
  }
}
```

**Conclusion:** ✅ BDET_CALLER_SOURCE_ID from defined process property

---

### Field 3: BDET_EMAIL

**Boomi Map Evidence (Lines 43-53):**
```json
{
  "$": {
    "fromKey": "73",
    "fromKeyPath": "*[@key='1']/*[@key='46']/*[@key='47']/*[@key='48']/*[@key='49']/*[@key='50']/*[@key='73']",
    "fromNamePath": "Root/Object/workOrder/Array/ArrayElement1/Object/reporterEmail",
    "fromType": "profile",
    "toKey": "399",
    "toKeyPath": "*[@key='1']/*[@key='153']/*[@key='155']/*[@key='161']/*[@key='399']",
    "toNamePath": "Envelope/Body/CreateBreakdownTask/breakdownTaskDto/BDET_EMAIL",
    "toType": "profile"
  }
}
```

**Conclusion:** ✅ BDET_EMAIL from input field reporterEmail

---

### Field 4: BuildingId

**Boomi Map Evidence (Lines 456-494):**
```json
{
  "$": {
    "cacheEnabled": "true",
    "category": "ProcessProperty",
    "key": "6",
    "name": "Get Dynamic Process Property",
    "type": "PropertyGet"
  },
  "Inputs": {
    "Input": [
      {
        "$": {
          "default": "DPP_BuildingID",
          "key": "1",
          "name": "Property Name"
        }
      }
    ]
  }
}
```

**Mapping (Lines 112-120):**
```json
{
  "$": {
    "fromFunction": "6",
    "fromKey": "3",
    "fromType": "function",
    "toKey": "195",
    "toNamePath": "Envelope/Body/CreateBreakdownTask/breakdownTaskDto/BuildingId"
  }
}
```

**Conclusion:** ✅ BuildingId from process property DPP_BuildingID

---

### Field 5: CallId

**Boomi Map Evidence (Lines 67-77):**
```json
{
  "$": {
    "fromKey": "53",
    "fromKeyPath": "*[@key='1']/*[@key='46']/*[@key='47']/*[@key='48']/*[@key='49']/*[@key='50']/*[@key='53']",
    "fromNamePath": "Root/Object/workOrder/Array/ArrayElement1/Object/serviceRequestNumber",
    "fromType": "profile",
    "toKey": "403",
    "toKeyPath": "*[@key='1']/*[@key='153']/*[@key='155']/*[@key='161']/*[@key='403']",
    "toNamePath": "Envelope/Body/CreateBreakdownTask/breakdownTaskDto/CallId",
    "toType": "profile"
  }
}
```

**Conclusion:** ✅ CallId from input field serviceRequestNumber

---

### Field 6: CategoryId

**Boomi Map Evidence (Lines 291-330):**
```json
{
  "$": {
    "cacheEnabled": "true",
    "category": "ProcessProperty",
    "key": "2",
    "name": "Get Dynamic Process Property",
    "type": "PropertyGet"
  },
  "Inputs": {
    "Input": [
      {
        "$": {
          "default": "DPP_CategoryId",
          "key": "1",
          "name": "Property Name"
        }
      }
    ]
  }
}
```

**Mapping (Lines 89-98):**
```json
{
  "$": {
    "fromFunction": "2",
    "fromKey": "3",
    "fromType": "function",
    "toKey": "201",
    "toNamePath": "Envelope/Body/CreateBreakdownTask/breakdownTaskDto/CategoryId"
  }
}
```

**Conclusion:** ✅ CategoryId from process property DPP_CategoryId

---

### Field 7: ContractId

**Boomi Map Evidence (Lines 665-696):**
```json
{
  "$": {
    "cacheEnabled": "true",
    "category": "ProcessProperty",
    "key": "14",
    "name": "Get Process Property",
    "type": "DefinedProcessPropertyGet"
  },
  "Configuration": {
    "DefinedProcessProperty": {
      "$": {
        "componentId": "13a690a7-c480-4c39-8eeb-fe3ecb030bf5",
        "componentName": "PP_Create_Work Order",
        "propertyKey": "8a8d5c37-de99-427a-802a-729c8aac8145",
        "propertyName": "ContractId"
      }
    }
  }
}
```

**Mapping (Lines 223-231):**
```json
{
  "$": {
    "fromFunction": "14",
    "fromKey": "1",
    "fromType": "function",
    "toKey": "207",
    "toNamePath": "Envelope/Body/CreateBreakdownTask/breakdownTaskDto/ContractId"
  }
}
```

**Conclusion:** ✅ ContractId from defined process property

---

### Field 8: DisciplineId

**Boomi Map Evidence (Lines 332-373):**
```json
{
  "$": {
    "cacheEnabled": "true",
    "category": "ProcessProperty",
    "enabled": "true",
    "key": "3",
    "name": "Get Dynamic Process Property",
    "type": "PropertyGet"
  },
  "Inputs": {
    "Input": [
      {
        "$": {
          "default": "DDP_DisciplineId",
          "key": "1",
          "name": "Property Name"
        }
      }
    ]
  }
}
```

**Mapping (Lines 157-165):**
```json
{
  "$": {
    "fromFunction": "3",
    "fromKey": "3",
    "fromType": "function",
    "toKey": "233",
    "toNamePath": "Envelope/Body/CreateBreakdownTask/breakdownTaskDto/DisciplineId"
  }
}
```

**Conclusion:** ✅ DisciplineId from process property DDP_DisciplineId

---

### Field 9: InstructionId

**Boomi Map Evidence (Lines 415-454):**
```json
{
  "$": {
    "cacheEnabled": "true",
    "category": "ProcessProperty",
    "key": "5",
    "name": "Get Dynamic Process Property",
    "type": "PropertyGet"
  },
  "Inputs": {
    "Input": [
      {
        "$": {
          "default": "DPP_InstructionId",
          "key": "1",
          "name": "Property Name"
        }
      }
    ]
  }
}
```

**Mapping (Lines 133-142):**
```json
{
  "$": {
    "fromFunction": "5",
    "fromKey": "3",
    "fromType": "function",
    "toKey": "251",
    "toNamePath": "Envelope/Body/CreateBreakdownTask/breakdownTaskDto/InstructionId"
  }
}
```

**Conclusion:** ✅ InstructionId from process property DPP_InstructionId

---

### Field 10: LocationId

**Boomi Map Evidence (Lines 495-533):**
```json
{
  "$": {
    "cacheEnabled": "true",
    "category": "ProcessProperty",
    "key": "7",
    "name": "Get Dynamic Process Property",
    "type": "PropertyGet"
  },
  "Inputs": {
    "Input": [
      {
        "$": {
          "default": "DPP_LocationID",
          "key": "1",
          "name": "Property Name"
        }
      }
    ]
  }
}
```

**Mapping (Lines 122-131):**
```json
{
  "$": {
    "fromFunction": "7",
    "fromKey": "3",
    "fromType": "function",
    "toKey": "271",
    "toNamePath": "Envelope/Body/CreateBreakdownTask/breakdownTaskDto/LocationId"
  }
}
```

**Conclusion:** ✅ LocationId from process property DPP_LocationID

---

### Field 11: LongDescription ⭐ (Your Question)

**Boomi Map Evidence (Lines 145-155):**
```json
{
  "$": {
    "fromKey": "52",
    "fromKeyPath": "*[@key='1']/*[@key='46']/*[@key='47']/*[@key='48']/*[@key='49']/*[@key='50']/*[@key='52']",
    "fromNamePath": "Root/Object/workOrder/Array/ArrayElement1/Object/description",
    "fromType": "profile",
    "toKey": "275",
    "toKeyPath": "*[@key='1']/*[@key='153']/*[@key='155']/*[@key='161']/*[@key='275']",
    "toNamePath": "Envelope/Body/CreateBreakdownTask/breakdownTaskDto/LongDescription",
    "toType": "profile"
  }
}
```

**Breakdown:**
- **Source Field:** `description` (key 52 in input profile)
- **Source Path:** `Root/Object/workOrder/Array/ArrayElement1/Object/description`
- **Target Field:** `LongDescription` (key 275 in SOAP request profile)
- **Target Path:** `Envelope/Body/CreateBreakdownTask/breakdownTaskDto/LongDescription`
- **Mapping Type:** Direct profile-to-profile mapping

**Conclusion:** ✅ **LongDescription is CORRECT** - Boomi map explicitly shows `description` → `LongDescription`

**Why "LongDescription" and not "description"?**
- CAFM API expects field name "LongDescription" (per SOAP schema)
- Boomi map performs field name transformation: input "description" → SOAP "LongDescription"
- This is common in SOAP APIs (descriptive field names in API schema)

---

### Field 12: Phone

**Boomi Map Evidence (Lines 55-65):**
```json
{
  "$": {
    "fromKey": "74",
    "fromKeyPath": "*[@key='1']/*[@key='46']/*[@key='47']/*[@key='48']/*[@key='49']/*[@key='50']/*[@key='74']",
    "fromNamePath": "Root/Object/workOrder/Array/ArrayElement1/Object/reporterPhoneNumber",
    "fromType": "profile",
    "toKey": "411",
    "toKeyPath": "*[@key='1']/*[@key='153']/*[@key='155']/*[@key='161']/*[@key='411']",
    "toNamePath": "Envelope/Body/CreateBreakdownTask/breakdownTaskDto/Phone",
    "toType": "profile"
  }
}
```

**Conclusion:** ✅ Phone from input field reporterPhoneNumber

---

### Field 13: PriorityId

**Boomi Map Evidence (Lines 375-413):**
```json
{
  "$": {
    "cacheEnabled": "true",
    "category": "ProcessProperty",
    "key": "4",
    "name": "Get Dynamic Process Property",
    "type": "PropertyGet"
  },
  "Inputs": {
    "Input": [
      {
        "$": {
          "default": "DPP_PriorityId",
          "key": "1",
          "name": "Property Name"
        }
      }
    ]
  }
}
```

**Mapping (Lines 100-109):**
```json
{
  "$": {
    "fromFunction": "4",
    "fromKey": "3",
    "fromType": "function",
    "toKey": "305",
    "toNamePath": "Envelope/Body/CreateBreakdownTask/breakdownTaskDto/PriorityId"
  }
}
```

**Conclusion:** ✅ PriorityId from process property DPP_PriorityId

---

### Field 14: RaisedDateUtc

**Boomi Map Evidence (Lines 608-664):**
```json
{
  "$": {
    "cacheEnabled": "true",
    "category": "Scripting",
    "enabled": "true",
    "key": "13",
    "name": "Scripting",
    "type": "Scripting"
  },
  "Inputs": {
    "Input": {
      "$": {
        "key": "3",
        "name": "raisedDateUtc"
      }
    }
  },
  "Outputs": {
    "Output": {
      "$": {
        "key": "4",
        "name": "RaisedDateUtc"
      }
    }
  },
  "Configuration": {
    "Scripting": {
      "$": {
        "language": "javascript"
      },
      "ScriptToExecute": {
        "_": "raisedDateUtc;\ndate = new Date(raisedDateUtc);\nformattedDate = date.toISOString();\nRaisedDateUtc = formattedDate.replace(/(\\.\d{3})Z$/, \".0208713Z\");"
      }
    }
  }
}
```

**Mapping (Lines 201-220):**
```json
{
  "$": {
    "fromFunction": "13",
    "fromKey": "4",
    "fromType": "function",
    "toKey": "307",
    "toNamePath": "Envelope/Body/CreateBreakdownTask/breakdownTaskDto/RaisedDateUtc"
  }
}
```

**Script Logic:**
1. Input: raisedDateUtc
2. Convert to Date object
3. Format to ISO string
4. Replace milliseconds with ".0208713Z"
5. Output: RaisedDateUtc

**Conclusion:** ✅ RaisedDateUtc from input raisedDateUtc with scripting transformation

---

### Field 15: ReporterName

**Boomi Map Evidence (Lines 30-41):**
```json
{
  "$": {
    "fromKey": "72",
    "fromKeyPath": "*[@key='1']/*[@key='46']/*[@key='47']/*[@key='48']/*[@key='49']/*[@key='50']/*[@key='72']",
    "fromNamePath": "Root/Object/workOrder/Array/ArrayElement1/Object/reporterName",
    "fromType": "profile",
    "toKey": "415",
    "toKeyPath": "*[@key='1']/*[@key='153']/*[@key='155']/*[@key='161']/*[@key='415']",
    "toNamePath": "Envelope/Body/CreateBreakdownTask/breakdownTaskDto/ReporterName",
    "toType": "profile"
  }
}
```

**Conclusion:** ✅ ReporterName from input field reporterName

---

### Field 16: ScheduledDateUtc

**Boomi Map Evidence (Lines 535-607):**
```json
{
  "$": {
    "cacheEnabled": "true",
    "category": "Scripting",
    "key": "11",
    "name": "Scripting",
    "type": "Scripting"
  },
  "Inputs": {
    "Input": [
      {
        "$": {
          "key": "1",
          "name": "scheduledDate"
        }
      },
      {
        "$": {
          "key": "2",
          "name": "scheduledTimeStart"
        }
      }
    ]
  },
  "Outputs": {
    "Output": {
      "$": {
        "key": "4",
        "name": "ScheduledDateUtc"
      }
    }
  },
  "Configuration": {
    "Scripting": {
      "$": {
        "language": "javascript"
      },
      "ScriptToExecute": {
        "_": "scheduledDate = \"2025-02-25\";\nscheduledTimeStart = \"11:05:41\";\nfullDateTime = scheduledDate + \"T\" + scheduledTimeStart + \"Z\";\nvar date = new Date(fullDateTime);\nvar formattedDate = date.toISOString();\nvar ScheduledDateUtc = formattedDate.replace(/(\\.\d{3})Z$/, \".0208713Z\");"
      }
    }
  }
}
```

**Mapping (Lines 189-198):**
```json
{
  "$": {
    "fromFunction": "11",
    "fromKey": "4",
    "fromType": "function",
    "toKey": "315",
    "toNamePath": "Envelope/Body/CreateBreakdownTask/breakdownTaskDto/ScheduledDateUtc"
  }
}
```

**Script Logic:**
1. Input: scheduledDate, scheduledTimeStart
2. Combine: scheduledDate + "T" + scheduledTimeStart + "Z"
3. Convert to Date object
4. Format to ISO string
5. Replace milliseconds with ".0208713Z"
6. Output: ScheduledDateUtc

**Conclusion:** ✅ ScheduledDateUtc from scheduledDate + scheduledTimeStart with scripting transformation

---

## Summary: LongDescription Justification

**Question:** On what basis was LongDescription added?

**Answer:** Based on Boomi map `390614fd-ae1d-496d-8a79-f320c8663049`, lines 145-155.

**Evidence:**
```json
{
  "fromNamePath": "Root/Object/workOrder/Array/ArrayElement1/Object/description",
  "toNamePath": "Envelope/Body/CreateBreakdownTask/breakdownTaskDto/LongDescription"
}
```

**Proof:**
- ✅ Boomi map explicitly maps `description` → `LongDescription`
- ✅ Target path shows exact SOAP field name: `LongDescription`
- ✅ Not assumed - extracted from map JSON
- ✅ Map is authoritative source for SOAP field names

**Why "LongDescription" instead of "description"?**
- CAFM API schema uses "LongDescription" as field name
- Boomi performs field name transformation (input "description" → SOAP "LongDescription")
- This is standard practice in SOAP APIs (descriptive/semantic field names)

---

## Verification Method

**For EVERY field in CreateBreakdownTask SOAP envelope:**

1. ✅ Located in Boomi map `390614fd`
2. ✅ Extracted `toNamePath` (shows SOAP field name)
3. ✅ Verified against map JSON (not assumed)
4. ✅ Documented evidence (line numbers, JSON snippets)

**Result:** 16/16 fields have explicit Boomi map evidence - **100% justified**

---

## Key Insight

**The question highlights the importance of:**
- Documenting evidence for EVERY field
- Not assuming field names
- Always referencing Boomi map line numbers
- Providing proof, not just assertions

**This is exactly why Step 1d (Map Analysis) is critical** - it forces explicit documentation of evidence for each field.

---

**END OF JUSTIFICATION**
