# BOOMI EXTRACTION PHASE 1 - Late Login Process

**Process Name:** Late_Login  
**Process ID:** 62132bc2-8e9b-4132-bf0c-c7108d8310bf  
**Domain:** Automotive / Buddy APP 2.0 / LateLogin  
**Description:** This process is to raise a late login request by driver  
**SOR:** Microsoft Dynamics 365 Finance & Operations (D365)  
**Date:** 2026-02-03

---

## Operations Inventory

### External Operations (API Calls to SORs)

| Operation ID | Name | Type | Connection | Method | Purpose |
|-------------|------|------|------------|--------|---------|
| 4e6e256a-8cb5-4eb4-a05c-3bac6a77788c | D365_Drivers_Latelogin | HTTP POST | ce8996ca-cdb1-4bb8-aea7-eed845f4dccc (D365_Connection) | POST | Submit late login request to D365 |
| 3d8b036c-d715-4853-a52c-1f6386417924 | D365_Token_Connector Operation | HTTP POST | 2c8c173d-fc9d-41f1-a349-6e7f3fac0c32 (D365_Token_URL) | POST | Get OAuth2 token from Azure AD |

### Entry Point Operation

| Operation ID | Name | Type | Purpose |
|-------------|------|------|---------|
| 32dddc43-22e2-42a0-9e20-440fc3f1e6ab | Latelogin Web Services Server Connector Operation | Web Services Server (WSS) | HTTP entry point - receives late login request from mobile app |

### Email Operations (Cross-Cutting Concern)

| Operation ID | Name | Type | Connection | Purpose |
|-------------|------|------|------------|---------|
| af07502a-fafd-4976-a691-45d51a33b549 | Email w Attachment | Mail Send | 00eae79b-2303-4215-8067-dcc299e42697 (Office 365 Email) | Send error notification email with attachment |
| 15a72a21-9b57-49a1-a8ed-d70367146644 | Email W/O Attachment | Mail Send | 00eae79b-2303-4215-8067-dcc299e42697 (Office 365 Email) | Send error notification email without attachment |

---

## Input Structure Analysis (Step 1a)

### Entry Point Operation
- **Operation ID:** 32dddc43-22e2-42a0-9e20-440fc3f1e6ab
- **Type:** connector-action (subType: wss - Web Services Server)
- **Input Type:** singlejson
- **Request Profile ID:** b854b007-1bc1-410e-96fd-fae1b46454c3
- **Profile Name:** Driver_Input JSON

### Input Profile Structure

**Profile Type:** profile.json (JSON Profile)

**Root Element:** `driverCreateRequest`

**Structure:**
```
driverCreateRequest (Object)
└── driver (Object)
    ├── id (string) - Driver ID [REQUIRED, MAPPED]
    ├── partnerDriverId (string) [OPTIONAL]
    ├── rtaId (string) [OPTIONAL]
    ├── firstName (string) [REQUIRED]
    ├── lastName (string) [REQUIRED]
    ├── careemId (string) [OPTIONAL]
    ├── fullName (string) [OPTIONAL]
    ├── status (string) [OPTIONAL]
    ├── trafficFileNumber (string) [OPTIONAL]
    ├── shift (string) [OPTIONAL]
    ├── dateOfBirth (string) [OPTIONAL]
    ├── nationality (string) [OPTIONAL]
    ├── gender (string) [OPTIONAL]
    ├── rtaPermit (Array) [OPTIONAL]
    │   └── ArrayElement1 (Object, minOccurs: 0, maxOccurs: -1)
    │       ├── number (string)
    │       ├── issuedDate (string)
    │       └── expiryDate (string)
    ├── license (Array) [OPTIONAL]
    │   └── ArrayElement1 (Object, minOccurs: 0, maxOccurs: -1)
    │       ├── number (string)
    │       ├── issuedDate (string)
    │       ├── expiryDate (string)
    │       └── issuingAuthority (string)
    ├── address (Array) [REQUIRED]
    │   └── ArrayElement1 (Object, minOccurs: 0, maxOccurs: -1)
    │       ├── address1 (string)
    │       ├── address2 (string)
    │       ├── state (string)
    │       ├── poBox (string)
    │       ├── city (string)
    │       ├── emirate (string)
    │       ├── country (string)
    │       └── zipCode (string)
    ├── email (Array) [REQUIRED]
    │   └── ArrayElement1 (Object, minOccurs: 0, maxOccurs: -1)
    │       └── id (string)
    ├── telephones (Array) [REQUIRED]
    │   └── ArrayElement1 (Object, minOccurs: 0, maxOccurs: -1)
    │       └── mobileNumber (string)
    ├── login (Array) [REQUIRED] *** CRITICAL FOR LATE LOGIN ***
    │   └── ArrayElement1 (Object, minOccurs: 0, maxOccurs: -1)
    │       ├── dateTime (string) [REQUIRED] - Late login date/time
    │       ├── reason (string) [OPTIONAL] - Reason code for late login
    │       └── remarks (string) [OPTIONAL] - Additional remarks
    ├── communication (Array) [REQUIRED]
    │   └── ArrayElement1 (Object, minOccurs: 0, maxOccurs: -1)
    │       └── type (string)
    └── consent (Object) [REQUIRED]
        └── email (string)
```

### Key Fields for Late Login Process

The following fields are CRITICAL for the late login functionality:

1. **driver.id** (key: 5) - Driver identifier (mapped to driverId in D365 request)
2. **driver.login[0].dateTime** (key: 34) - Late login date/time (mapped to requestDateTime in D365 request)
3. **driver.login[0].reason** (key: 78) - Reason code (mapped to reasonCode in D365 request)
4. **driver.login[0].remarks** (key: 79) - Remarks (mapped to remarks in D365 request)

### Query Parameters

The process also accepts query parameters:
- **query_driver-id** - Driver ID passed as query parameter (default: "1123")
- **inheader_company-code** - Company code passed in HTTP header

### Input Cardinality

- **Is Array:** No (single object expected)
- **Min Occurs:** 1
- **Max Occurs:** 1

### Field Paths (JSON Paths)

```
driverCreateRequest/Object/driver/Object/id
driverCreateRequest/Object/driver/Object/login/Array/ArrayElement1/Object/dateTime
driverCreateRequest/Object/driver/Object/login/Array/ArrayElement1/Object/reason
driverCreateRequest/Object/driver/Object/login/Array/ArrayElement1/Object/remarks
```

### Tracked Fields

The entry operation tracks the following field:
- **driver-id** (fieldId: 39434) - Mapped from `driverCreateRequest/Object/driver/Object/id` (elementId: 5)

---

## Response Structure Analysis (Step 1b)

### Entry Point Response

**Response Profile ID:** bc9f24e9-854f-45cb-9871-2abe44b8963d  
**Profile Name:** LateLogin Boomi Response  
**Profile Type:** profile.json (JSON Profile)

**Response Structure:**
```
lateLoginResponse (Object)
└── messages (Array)
    └── ArrayElement1 (Object, minOccurs: 0, maxOccurs: -1)
        ├── id (string) [OPTIONAL]
        ├── status (string) [REQUIRED] - Success/failure status
        └── message (string) [REQUIRED] - Response message
```

### Response Mapping Source

The response is mapped from D365 API response using map: **83a4eb50-423d-4d55-9c17-a548120a93de** (Late Login Response)

**Source Profile:** a30116a0-e900-4e68-9a74-7bd1dca4e7fb (D365_Drivers_LateLogin_Response)

**Mapping:**
- `Root/Object/Messages/Array/ArrayElement1/Object/success` → `lateLoginResponse/Object/messages/Array/ArrayElement1/Object/status`
- `Root/Object/Messages/Array/ArrayElement1/Object/message` → `lateLoginResponse/Object/messages/Array/ArrayElement1/Object/message`

---

## Operation Response Analysis (Step 1c)

### Operation: D365_Drivers_Latelogin (4e6e256a-8cb5-4eb4-a05c-3bac6a77788c)

**Response Profile ID:** a30116a0-e900-4e68-9a74-7bd1dca4e7fb  
**Profile Name:** D365_Drivers_LateLogin_Response  
**Profile Type:** profile.json

**Response Structure:**
```
Root (Object)
├── $id (string)
└── Messages (Array)
    └── ArrayElement1 (Object, minOccurs: 0, maxOccurs: -1)
        ├── $id (string)
        ├── success (string) - "true" or "false"
        ├── message (string) - Success/error message
        ├── reference (string) - Reference ID
        └── InputReference (string) - Input reference
```

**HTTP Status Codes:**
- **Success:** 200 OK (when D365 returns success: true)
- **Error:** Handled by Try/Catch block (returns error via email subprocess)

### Operation: D365_Token_Connector Operation (3d8b036c-d715-4853-a52c-1f6386417924)

**Response Profile ID:** 3c7211fa-5ae0-4b22-a9f6-f812d62ef13d  
**Profile Name:** D365_Token_Details JSON  
**Profile Type:** profile.json

**Response Structure:**
```
Root (Object)
├── token_type (string) - "Bearer"
├── expires_in (string) - Token expiration in seconds
├── ext_expires_in (string) - Extended expiration
├── expires_on (string) - Unix timestamp
├── not_before (string) - Unix timestamp
├── resource (string) - Resource URL
└── access_token (string) - OAuth2 access token [CRITICAL]
```

**HTTP Status Codes:**
- **Success:** 200 OK (Azure AD returns token)
- **Error:** Handled by Try/Catch block in D365_Token_Process subprocess

---

## Map Analysis (Step 1d)

### Map 1: D365_Driver_Latelogin_Map (ed7d9977-13cc-4ff0-9b85-8a84f58434ca)

**Purpose:** Transform input driver data to D365 late login request format

**From Profile:** 01486775-2098-416b-8ba7-51697722edc9 (Driver_Input JSON)  
**To Profile:** b0745f83-34b1-49a0-9058-e5ec0987cc73 (D365_Driver_Latelogin Request)

**Target Structure (D365 Request):**
```
Root (Object)
└── params (Object)
    ├── driverId (string)
    ├── RequestNo (string) [OPTIONAL]
    ├── requestDateTime (string) [OPTIONAL]
    ├── companyCode (string) [OPTIONAL]
    ├── remarks (string) [OPTIONAL]
    └── reasonCode (string) [OPTIONAL]
```

**Field Mappings:**

| Source Field | Source Path | Target Field | Target Path | Transformation |
|-------------|-------------|--------------|-------------|----------------|
| driver.id | driverCreateRequest/Object/driver/Object/id | driverId | Root/Object/params/Object/driverId | Direct mapping |
| DPP_companyCode (Process Property) | - | companyCode | Root/Object/params/Object/companyCode | Get Dynamic Process Property |
| driver.login[0].reason | driverCreateRequest/Object/driver/Object/login/Array/ArrayElement1/Object/reason | reasonCode | Root/Object/params/Object/reasonCode | Direct mapping |
| driver.login[0].remarks | driverCreateRequest/Object/driver/Object/login/Array/ArrayElement1/Object/remarks | remarks | Root/Object/params/Object/remarks | Direct mapping |
| driver.login[0].dateTime | driverCreateRequest/Object/driver/Object/login/Array/ArrayElement1/Object/dateTime | requestDateTime | Root/Object/params/Object/requestDateTime | PopulateDate function (custom) |

**Functions Used:**
1. **Get Dynamic Process Property** (PropertyGet) - Retrieves DPP_companyCode
2. **PopulateDate** (userdefined, id: e4a0b690-18cc-49bb-a7c6-48a04ec5c6fc) - Transforms dateTime format

**CRITICAL FIELD NAMES (AUTHORITATIVE):**
- Use `driverId` (NOT driver_id or DriverId)
- Use `requestDateTime` (NOT request_date_time or RequestDateTime)
- Use `companyCode` (NOT company_code or CompanyCode)
- Use `reasonCode` (NOT reason_code or ReasonCode)
- Use `remarks` (NOT Remarks)

### Map 2: Late Login Response (83a4eb50-423d-4d55-9c17-a548120a93de)

**Purpose:** Transform D365 response to Boomi response format

**From Profile:** a30116a0-e900-4e68-9a74-7bd1dca4e7fb (D365_Drivers_LateLogin_Response)  
**To Profile:** bc9f24e9-854f-45cb-9871-2abe44b8963d (LateLogin Boomi Response)

**Field Mappings:**

| Source Field | Target Field |
|-------------|--------------|
| Root/Object/Messages/Array/ArrayElement1/Object/success | lateLoginResponse/Object/messages/Array/ArrayElement1/Object/status |
| Root/Object/Messages/Array/ArrayElement1/Object/message | lateLoginResponse/Object/messages/Array/ArrayElement1/Object/message |

### Map 3: Error Map (2cb99dad-a13b-4b9d-aa62-0fe4720ab418)

**Purpose:** Map error message to error response format

**From Profile:** f7e100ff-4833-4cdd-96f2-6da3715bf852 (Dummy - flat file)  
**To Profile:** 6b87a796-4d2b-4e7d-887f-47f2a98542e0 (DriverPartner Get Response Common)

**Field Mappings:**

| Source Field | Target Field |
|-------------|--------------|
| DPP_ErrorMessage (Process Property via PropertyGet function) | partnerView/Object/message |

**Default Values:**
- `partnerView/Object/success` = "false" (hardcoded)

---

## HTTP Status Codes and Return Paths (Step 1e)

### Success Path (HTTP 200)

**Condition:** D365 API returns success response with `success: true`

**Return Path:**
1. Map D365 response → Boomi response (map: 83a4eb50-423d-4d55-9c17-a548120a93de)
2. Return documents via shape7 (Success) with label "Success"
3. HTTP Status: 200 OK
4. Response Body: `{ "lateLoginResponse": { "messages": [{ "status": "true", "message": "..." }] } }`

### Error Paths

#### Error Path 1: Try/Catch Error (Branch 1)

**Condition:** Exception occurs in Try block (shape4)

**Return Path:**
1. Catch error → shape14 (Branch with 3 paths)
2. Branch Path 1 (shape14.dragpoint1):
   - Set DPP_ErrorMessage from catch error message (shape17)
   - Call subprocess: (Sub) Office 365 Email (shape15) - Send error email with attachment
   - **NO RETURN** - subprocess ends, no return to main process

**HTTP Status:** Process throws exception, no HTTP response returned

#### Error Path 2: Failure Response (Branch 2)

**Condition:** Catch error occurs, branch path 2 selected

**Return Path:**
1. Map error message to response (shape20 - map: 2cb99dad-a13b-4b9d-aa62-0fe4720ab418)
2. Return documents via shape21 (Failure) with label "Failure"
3. HTTP Status: 500 Internal Server Error (inferred)
4. Response Body: `{ "partnerView": { "success": false, "message": "<error_message>" } }`

#### Error Path 3: Exception Throw (Branch 3)

**Condition:** Catch error occurs, branch path 3 selected

**Return Path:**
1. Throw exception (shape8) with message from catch error
2. **NO RETURN** - exception terminates process
3. HTTP Status: 500 Internal Server Error

### D365 Token Subprocess Error Handling

**Subprocess:** D365_Token_Process (48d4409f-b042-4e7a-9d80-a74743f84b1a)

**Error Paths:**
1. **Branch Path 1:** Set DPP_ErrorMessage → Call email subprocess → NO RETURN
2. **Branch Path 2:** Throw exception with detailed error message → NO RETURN

### Return Path Summary

| Path | Condition | Return Shape | HTTP Status | Response Profile |
|------|-----------|--------------|-------------|------------------|
| Success | D365 success | shape7 (Success) | 200 OK | bc9f24e9-854f-45cb-9871-2abe44b8963d |
| Failure | Catch error (branch 2) | shape21 (Failure) | 500 | 6b87a796-4d2b-4e7d-887f-47f2a98542e0 |
| Error (branch 1) | Catch error (branch 1) | NO RETURN (email sent) | N/A | N/A |
| Error (branch 3) | Catch error (branch 3) | NO RETURN (exception) | 500 | N/A |

---

## Process Properties Analysis (Steps 2-3)

### Step 2: Property WRITES

| Shape | Property Name | Property Type | Source | Value |
|-------|--------------|---------------|--------|-------|
| shape5 | DDP_driverId | Dynamic Document Property | query_driver-id (query parameter) | Query parameter value (default: "1123") |
| shape5 | URL | Dynamic Document Property | PP_Drivers_Latelogin.Resource_Path | "api/services/AGIMobileAppGroup/AGIMobileAppCarsTaxiIntegrationServices/lateloginrequest" |
| shape5 | DPP_companyCode | Dynamic Process Property | inheader_company-code (HTTP header) | HTTP header value |
| shape16 | DPP_Process_Name | Dynamic Process Property | Execution Property: Process Name | "Late_Login" |
| shape16 | DPP_AtomName | Dynamic Process Property | Execution Property: Atom Name | Atom name |
| shape16 | DPP_Payload | Dynamic Process Property | Current document | Input payload |
| shape16 | DPP_ExecutionID | Dynamic Process Property | Execution Property: Execution Id | Execution ID |
| shape16 | DPP_File_Name | Dynamic Process Property | PP_Drivers_Latelogin.File_Name + Current Date + ".txt" | "Driver_Latelogin_<timestamp>.txt" |
| shape16 | DPP_Subject | Dynamic Process Property | Atom Name + " (" + Process Name + " ) has errors to report" | Error email subject |
| shape16 | DPP_HasAttachment | Dynamic Process Property | Static value | "N" |
| shape16 | To_Email | Dynamic Process Property | PP_Drivers_Latelogin.To_Email | "BoomiIntegrationTeam@al-ghurair.com" |
| shape17 | DPP_ErrorMessage | Dynamic Process Property | meta.base.catcherrorsmessage | Catch error message |
| shape24 | Authorization | Dynamic Document Property | "Bearer " + DPP_D365_Token | Bearer token for D365 API |

### Step 3: Property READS

| Shape | Property Name | Property Type | Used In | Purpose |
|-------|--------------|---------------|---------|---------|
| shape10 (Map) | DPP_companyCode | Dynamic Process Property | Map function (Get Dynamic Process Property) | Map to D365 request companyCode field |
| shape24 | DPP_D365_Token | Dynamic Process Property | Set Authorization header | Bearer token for D365 API call |
| shape20 (Error Map) | DPP_ErrorMessage | Dynamic Process Property | Map function (Get Dynamic Process Property) | Map to error response message field |

### Data Dependency Analysis

**CRITICAL DEPENDENCIES:**

1. **DPP_D365_Token MUST be set BEFORE shape24**
   - Written by: subprocess D365_Token_Process (shape22)
   - Read by: shape24 (set Authorization header)
   - **ENFORCEMENT:** shape22 MUST execute before shape24

2. **DPP_companyCode MUST be set BEFORE shape10 (Map)**
   - Written by: shape5 (from HTTP header)
   - Read by: shape10 (map function)
   - **ENFORCEMENT:** shape5 MUST execute before shape10

3. **DPP_ErrorMessage MUST be set BEFORE shape20 (Error Map)**
   - Written by: shape17 (from catch error)
   - Read by: shape20 (map function)
   - **ENFORCEMENT:** shape17 MUST execute before shape20

---

## Data Dependency Graph (Step 4)

### Node Definitions

```
START (shape1) - Entry point
SET_INPUT (shape5) - Set input details (DDP_driverId, URL, DPP_companyCode)
SET_PROPS (shape16) - Set process properties (DPP_Process_Name, DPP_AtomName, etc.)
TRY_CATCH (shape4) - Try/Catch error handler
MAP_REQUEST (shape10) - Map input to D365 request
NOTIFY (shape25) - Notify shape (log payload)
BRANCH_TOKEN (shape23) - Branch: Check if token exists
GET_TOKEN (shape22) - Subprocess: Get D365 token
SET_TOKEN (shape24) - Set Authorization header
CALL_D365 (shape6) - HTTP POST to D365 late login API
MAP_RESPONSE (shape19) - Map D365 response to Boomi response
SUCCESS (shape7) - Return success response
BRANCH_ERROR (shape14) - Branch: Error handling (3 paths)
SET_ERROR (shape17) - Set error message
EMAIL_ERROR (shape15) - Subprocess: Send error email
MAP_ERROR (shape20) - Map error to response
FAILURE (shape21) - Return failure response
THROW_ERROR (shape8) - Throw exception
```

### Edges (Dependencies)

```
START → SET_INPUT (dragpoint)
SET_INPUT → SET_PROPS (dragpoint)
SET_PROPS → TRY_CATCH (dragpoint)

TRY_CATCH (Try path) → MAP_REQUEST (dragpoint)
TRY_CATCH (Catch path) → BRANCH_ERROR (dragpoint)

MAP_REQUEST → NOTIFY (dragpoint)
NOTIFY → BRANCH_TOKEN (dragpoint)

BRANCH_TOKEN (Path 1) → GET_TOKEN (dragpoint)
BRANCH_TOKEN (Path 2) → SET_TOKEN (dragpoint)

GET_TOKEN → SET_TOKEN (data dependency: DPP_D365_Token)
SET_TOKEN → CALL_D365 (dragpoint)

CALL_D365 → MAP_RESPONSE (dragpoint)
MAP_RESPONSE → SUCCESS (dragpoint)

BRANCH_ERROR (Path 1) → SET_ERROR (dragpoint)
SET_ERROR → EMAIL_ERROR (dragpoint)

BRANCH_ERROR (Path 2) → MAP_ERROR (dragpoint)
MAP_ERROR → FAILURE (dragpoint)

BRANCH_ERROR (Path 3) → THROW_ERROR (dragpoint)
```

### Data Dependencies (Property Flow)

```
SET_INPUT writes: DDP_driverId, URL, DPP_companyCode
  ↓ (data dependency)
MAP_REQUEST reads: DPP_companyCode

GET_TOKEN writes: DPP_D365_Token (via subprocess)
  ↓ (data dependency)
SET_TOKEN reads: DPP_D365_Token

SET_ERROR writes: DPP_ErrorMessage
  ↓ (data dependency)
MAP_ERROR reads: DPP_ErrorMessage
```

### Topological Sort (Execution Order)

**Main Flow (Success Path):**
1. START
2. SET_INPUT (writes: DDP_driverId, URL, DPP_companyCode)
3. SET_PROPS (writes: DPP_Process_Name, DPP_AtomName, etc.)
4. TRY_CATCH (start try block)
5. MAP_REQUEST (reads: DPP_companyCode)
6. NOTIFY
7. BRANCH_TOKEN (decision)
8. GET_TOKEN (if token not exists) OR skip (if token exists)
9. SET_TOKEN (reads: DPP_D365_Token)
10. CALL_D365
11. MAP_RESPONSE
12. SUCCESS (return)

**Error Flow (Catch Path):**
1. BRANCH_ERROR (3-way branch)
2. **Path 1:** SET_ERROR → EMAIL_ERROR (no return)
3. **Path 2:** MAP_ERROR → FAILURE (return)
4. **Path 3:** THROW_ERROR (no return)

---

## Control Flow Graph (Step 5)

### Nodes

```
N1: START (shape1)
N2: SET_INPUT (shape5)
N3: SET_PROPS (shape16)
N4: TRY_CATCH (shape4)
N5: MAP_REQUEST (shape10)
N6: NOTIFY (shape25)
N7: BRANCH_TOKEN (shape23)
N8: GET_TOKEN (shape22)
N9: SET_TOKEN (shape24)
N10: CALL_D365 (shape6)
N11: MAP_RESPONSE (shape19)
N12: SUCCESS (shape7)
N13: BRANCH_ERROR (shape14)
N14: SET_ERROR (shape17)
N15: EMAIL_ERROR (shape15)
N16: MAP_ERROR (shape20)
N17: FAILURE (shape21)
N18: THROW_ERROR (shape8)
```

### Edges (Control Flow)

```
N1 → N2 (unconditional)
N2 → N3 (unconditional)
N3 → N4 (unconditional)
N4 → N5 (try path)
N4 → N13 (catch path)
N5 → N6 (unconditional)
N6 → N7 (unconditional)
N7 → N8 (branch path 1: token not exists)
N7 → N9 (branch path 2: token exists)
N8 → N9 (unconditional - after subprocess)
N9 → N10 (unconditional)
N10 → N11 (unconditional)
N11 → N12 (unconditional)
N13 → N14 (branch path 1)
N13 → N16 (branch path 2)
N13 → N18 (branch path 3)
N14 → N15 (unconditional)
N16 → N17 (unconditional)
```

### Terminal Nodes (Exit Points)

```
N12: SUCCESS (return documents - success)
N15: EMAIL_ERROR (subprocess ends - no return)
N17: FAILURE (return documents - failure)
N18: THROW_ERROR (exception - no return)
```

---

## Decision Shape Analysis (Step 7)

### Decision 1: BRANCH_TOKEN (shape23)

**Type:** Branch Shape (2 paths)

**Purpose:** Check if D365 OAuth2 token already exists in process property DPP_D365_Token

**Paths:**
- **Path 1 (shape23.dragpoint1):** Token NOT exists → GET_TOKEN subprocess (shape22)
- **Path 2 (shape23.dragpoint2):** Token exists → Skip to SET_TOKEN (shape24)

**Analysis:**
- **Is this a business decision?** NO - This is an optimization check
- **Does this affect data flow?** YES - Determines whether to call token subprocess
- **Are both paths valid?** YES - Both paths converge at SET_TOKEN (shape24)
- **Early exit?** NO - Both paths continue to SET_TOKEN

**Execution Order:**
1. BRANCH_TOKEN evaluates condition
2. **IF** token not exists: GET_TOKEN → SET_TOKEN
3. **ELSE**: SET_TOKEN directly
4. Both paths converge at SET_TOKEN

**Trace to Termination:**
- **Path 1:** BRANCH_TOKEN → GET_TOKEN → SET_TOKEN → CALL_D365 → MAP_RESPONSE → SUCCESS
- **Path 2:** BRANCH_TOKEN → SET_TOKEN → CALL_D365 → MAP_RESPONSE → SUCCESS

**Self-Check Questions:**
- ✅ **Q1:** Did I identify ALL decision shapes? YES - shape23 is the only decision shape in main flow
- ✅ **Q2:** Did I trace BOTH TRUE and FALSE paths to termination? YES - Both paths traced to SUCCESS
- ✅ **Q3:** Did I identify early exits? YES - No early exits in this decision
- ✅ **Q4:** Did I document business logic? YES - Token existence check documented
- ✅ **Q5:** Did I verify data dependencies? YES - DPP_D365_Token dependency verified

### Decision 2: BRANCH_ERROR (shape14) - Error Handling

**Type:** Branch Shape (3 paths)

**Purpose:** Handle errors from Try/Catch block with different error handling strategies

**Paths:**
- **Path 1 (shape14.dragpoint1):** SET_ERROR → EMAIL_ERROR (send error email, no return)
- **Path 2 (shape14.dragpoint2):** MAP_ERROR → FAILURE (return error response)
- **Path 3 (shape14.dragpoint3):** THROW_ERROR (throw exception, no return)

**Analysis:**
- **Is this a business decision?** NO - This is error handling branching
- **Does this affect data flow?** YES - Determines error handling strategy
- **Are all paths valid?** YES - All 3 paths are valid error handling strategies
- **Early exit?** YES - Paths 1 and 3 do not return to caller

**Execution Order:**
1. BRANCH_ERROR evaluates condition (likely based on error type or configuration)
2. **Path 1:** SET_ERROR → EMAIL_ERROR → END (no return)
3. **Path 2:** MAP_ERROR → FAILURE → RETURN
4. **Path 3:** THROW_ERROR → END (exception thrown)

**Trace to Termination:**
- **Path 1:** BRANCH_ERROR → SET_ERROR → EMAIL_ERROR → [EARLY EXIT - NO RETURN]
- **Path 2:** BRANCH_ERROR → MAP_ERROR → FAILURE → [RETURN FAILURE RESPONSE]
- **Path 3:** BRANCH_ERROR → THROW_ERROR → [EARLY EXIT - EXCEPTION THROWN]

**Self-Check Questions:**
- ✅ **Q1:** Did I identify ALL decision shapes? YES - shape14 is error handling branch
- ✅ **Q2:** Did I trace ALL 3 paths to termination? YES - All 3 paths traced
- ✅ **Q3:** Did I identify early exits? YES - Paths 1 and 3 are early exits (no return)
- ✅ **Q4:** Did I document business logic? YES - Error handling strategies documented
- ✅ **Q5:** Did I verify data dependencies? YES - DPP_ErrorMessage dependency verified

### Decision Summary

| Decision | Type | Paths | Early Exit? | Business Logic? |
|----------|------|-------|-------------|-----------------|
| BRANCH_TOKEN (shape23) | Branch | 2 | NO | NO (optimization) |
| BRANCH_ERROR (shape14) | Branch | 3 | YES (paths 1, 3) | NO (error handling) |

---

## Branch Shape Analysis (Step 8)

### Branch 1: BRANCH_TOKEN (shape23)

**Configuration:**
```json
{
  "numBranches": "2"
}
```

**Paths:**
1. **Path 1 (identifier: "1"):** shape23 → shape22 (GET_TOKEN subprocess)
2. **Path 2 (identifier: "2"):** shape23 → shape24 (SET_TOKEN)

**Analysis:**

**Step 1: Identify operations in each path**
- **Path 1:** GET_TOKEN (subprocess: D365_Token_Process) - Contains HTTP POST to Azure AD token endpoint
- **Path 2:** SET_TOKEN (document properties) - No API calls

**Step 2: Check for API calls**
- **Path 1:** YES - HTTP POST to Azure AD (operation: 3d8b036c-d715-4853-a52c-1f6386417924)
- **Path 2:** NO - Only document property manipulation

**Step 3: Apply Rule 7 (ALL API CALLS ARE SEQUENTIAL)**
- **Path 1 contains API call** → This branch is SEQUENTIAL
- Even though Path 2 has no data dependency on Path 1, the presence of API call in Path 1 makes this SEQUENTIAL

**Step 4: Determine execution order**
- **Execution Order:** Path 1 → Path 2 (SEQUENTIAL)
- **Reason:** Path 1 contains API call (token retrieval), which must execute sequentially

**Step 5: Check data dependencies**
- **Path 1 writes:** DPP_D365_Token (via subprocess)
- **Path 2 reads:** DPP_D365_Token (in shape24)
- **Data Dependency:** Path 1 MUST execute before Path 2 (if Path 1 is taken)

**Classification:** SEQUENTIAL (due to API call in Path 1)

**Convergence Point:** shape24 (SET_TOKEN)

**Self-Check Questions:**
- ✅ **Q1:** Did I identify ALL operations in each path? YES
- ✅ **Q2:** Did I check for API calls? YES - Path 1 has HTTP POST
- ✅ **Q3:** Did I apply Rule 7? YES - API call makes it SEQUENTIAL
- ✅ **Q4:** Did I check data dependencies? YES - DPP_D365_Token dependency verified
- ✅ **Q5:** Did I determine execution order? YES - Path 1 → Path 2

### Branch 2: BRANCH_ERROR (shape14)

**Configuration:**
```json
{
  "numBranches": "3"
}
```

**Paths:**
1. **Path 1 (identifier: "1"):** shape14 → shape17 (SET_ERROR) → shape15 (EMAIL_ERROR subprocess)
2. **Path 2 (identifier: "2"):** shape14 → shape20 (MAP_ERROR) → shape21 (FAILURE)
3. **Path 3 (identifier: "3"):** shape14 → shape8 (THROW_ERROR)

**Analysis:**

**Step 1: Identify operations in each path**
- **Path 1:** SET_ERROR (document properties) → EMAIL_ERROR (subprocess: Office 365 Email) - Contains SMTP send
- **Path 2:** MAP_ERROR (map) → FAILURE (return documents) - No API calls
- **Path 3:** THROW_ERROR (exception) - No API calls

**Step 2: Check for API calls**
- **Path 1:** YES - SMTP send email (operations: af07502a-fafd-4976-a691-45d51a33b549 or 15a72a21-9b57-49a1-a8ed-d70367146644)
- **Path 2:** NO - Only mapping and return
- **Path 3:** NO - Only exception throw

**Step 3: Apply Rule 7 (ALL API CALLS ARE SEQUENTIAL)**
- **Path 1 contains API call** → This branch is SEQUENTIAL
- Even though paths have no data dependencies on each other, the presence of API call makes this SEQUENTIAL

**Step 4: Determine execution order**
- **Execution Order:** Path 1 → Path 2 → Path 3 (SEQUENTIAL)
- **Reason:** Path 1 contains API call (email send), which must execute sequentially
- **Note:** In practice, only ONE path executes (branch selects one path based on error type)

**Step 5: Check data dependencies**
- **Path 1:** Reads DPP_ErrorMessage (written by shape17)
- **Path 2:** Reads DPP_ErrorMessage (in map function)
- **Path 3:** Reads meta.base.catcherrorsmessage (from catch block)
- **No cross-path dependencies** - Each path is independent

**Classification:** SEQUENTIAL (due to API call in Path 1)

**Convergence Point:** NONE - All paths are terminal (no return or exception)

**Self-Check Questions:**
- ✅ **Q1:** Did I identify ALL operations in each path? YES
- ✅ **Q2:** Did I check for API calls? YES - Path 1 has SMTP send
- ✅ **Q3:** Did I apply Rule 7? YES - API call makes it SEQUENTIAL
- ✅ **Q4:** Did I check data dependencies? YES - No cross-path dependencies
- ✅ **Q5:** Did I determine execution order? YES - Path 1 → Path 2 → Path 3 (but only one executes)

### Branch Summary

| Branch | Paths | API Calls? | Classification | Execution Order | Convergence |
|--------|-------|-----------|----------------|-----------------|-------------|
| BRANCH_TOKEN (shape23) | 2 | YES (Path 1) | SEQUENTIAL | Path 1 → Path 2 | shape24 (SET_TOKEN) |
| BRANCH_ERROR (shape14) | 3 | YES (Path 1) | SEQUENTIAL | Path 1 → Path 2 → Path 3 | NONE (terminal paths) |

---

## Execution Order (Step 9)

### Main Process Flow (Success Path)

**Execution Sequence:**

1. **START** (shape1) - Entry point, receives HTTP request
2. **SET_INPUT** (shape5) - Set input details:
   - DDP_driverId ← query_driver-id
   - URL ← PP_Drivers_Latelogin.Resource_Path
   - DPP_companyCode ← inheader_company-code
3. **SET_PROPS** (shape16) - Set process properties:
   - DPP_Process_Name ← "Late_Login"
   - DPP_AtomName ← Atom Name
   - DPP_Payload ← Current document
   - DPP_ExecutionID ← Execution Id
   - DPP_File_Name ← "Driver_Latelogin_<timestamp>.txt"
   - DPP_Subject ← Error email subject
   - DPP_HasAttachment ← "N"
   - To_Email ← "BoomiIntegrationTeam@al-ghurair.com"
4. **TRY_CATCH** (shape4) - Start try block
5. **MAP_REQUEST** (shape10) - Map input to D365 request:
   - Uses map: ed7d9977-13cc-4ff0-9b85-8a84f58434ca
   - Reads: DPP_companyCode
   - Transforms: driver data → D365 params object
6. **NOTIFY** (shape25) - Log payload (INFO level)
7. **BRANCH_TOKEN** (shape23) - Check if token exists:
   - **IF** token not exists: → GET_TOKEN (shape22)
   - **ELSE**: → SET_TOKEN (shape24)
8. **GET_TOKEN** (shape22) - [CONDITIONAL] Subprocess: D365_Token_Process
   - Calls Azure AD token endpoint
   - Writes: DPP_D365_Token
9. **SET_TOKEN** (shape24) - Set Authorization header:
   - Authorization ← "Bearer " + DPP_D365_Token
10. **CALL_D365** (shape6) - HTTP POST to D365 late login API:
    - URL: D365_Connection + URL
    - Headers: Authorization
    - Body: D365 request (from MAP_REQUEST)
11. **MAP_RESPONSE** (shape19) - Map D365 response to Boomi response:
    - Uses map: 83a4eb50-423d-4d55-9c17-a548120a93de
    - Transforms: D365 response → lateLoginResponse
12. **SUCCESS** (shape7) - Return success response (HTTP 200)

### Error Flow (Catch Path)

**Execution Sequence:**

1. **BRANCH_ERROR** (shape14) - 3-way branch (error handling)
2. **Path 1:**
   - **SET_ERROR** (shape17) - Set DPP_ErrorMessage ← catch error message
   - **EMAIL_ERROR** (shape15) - Subprocess: (Sub) Office 365 Email
   - **[EARLY EXIT]** - No return to caller
3. **Path 2:**
   - **MAP_ERROR** (shape20) - Map error to response:
     - Uses map: 2cb99dad-a13b-4b9d-aa62-0fe4720ab418
     - Reads: DPP_ErrorMessage
     - Transforms: error → partnerView response
   - **FAILURE** (shape21) - Return failure response (HTTP 500)
4. **Path 3:**
   - **THROW_ERROR** (shape8) - Throw exception with catch error message
   - **[EARLY EXIT]** - Exception terminates process

### Subprocess: D365_Token_Process (48d4409f-b042-4e7a-9d80-a74743f84b1a)

**Execution Sequence:**

1. **START** (shape1) - Subprocess entry
2. **TRY_CATCH** (shape12) - Start try block
3. **MESSAGE** (shape4) - Build token request body:
   - Format: "grant_type={1}&client_id={2}&client_secret={3}&resource={4}"
   - Values from: D365_Token_PP (component: d73ae535-5d2e-4208-bdb6-1e6f278e4597)
4. **CALL_TOKEN_API** (shape2) - HTTP POST to Azure AD token endpoint:
   - URL: https://login.microsoftonline.com/.../oauth2/token
   - Body: Token request (from MESSAGE)
5. **SET_TOKEN** (shape8) - Set DPP_D365_Token:
   - DPP_D365_Token ← "Bearer " + access_token
6. **STOP** (shape3) - Subprocess ends (continue: true)
7. **[ERROR PATH]** BRANCH_ERROR (shape15) - 2-way branch:
   - **Path 1:** SET_ERROR (shape16) → EMAIL_ERROR (shape13) → [NO RETURN]
   - **Path 2:** THROW_ERROR (shape14) → [EXCEPTION]

### Subprocess: (Sub) Office 365 Email (a85945c5-3004-42b9-80b1-104f465cd1fb)

**Execution Sequence:**

1. **START** (shape1) - Subprocess entry
2. **TRY_CATCH** (shape2) - Start try block
3. **DECISION** (shape4) - Check DPP_HasAttachment:
   - **IF** DPP_HasAttachment == "Y": → EMAIL_WITH_ATTACHMENT
   - **ELSE**: → EMAIL_WITHOUT_ATTACHMENT
4. **EMAIL_WITH_ATTACHMENT** (shape11, shape14, shape15, shape6, shape3):
   - Build email body (HTML format)
   - Set DPP_MailBody
   - Set payload
   - Set mail properties (from, to, subject, body, filename)
   - Send email (operation: af07502a-fafd-4976-a691-45d51a33b549)
   - STOP
5. **EMAIL_WITHOUT_ATTACHMENT** (shape23, shape22, shape20, shape7, shape9):
   - Build email body (HTML format)
   - Set DPP_MailBody
   - Set mail properties (from, to, subject, body)
   - Send email (operation: 15a72a21-9b57-49a1-a8ed-d70367146644)
   - STOP
6. **[ERROR PATH]** THROW_ERROR (shape10) - Throw exception

### Data Dependencies Verification

**Verification:**

1. ✅ **DPP_companyCode** written (shape5) BEFORE read (shape10 map)
2. ✅ **DPP_D365_Token** written (shape22 subprocess) BEFORE read (shape24)
3. ✅ **DPP_ErrorMessage** written (shape17) BEFORE read (shape20 map)
4. ✅ **All property writes occur before property reads**

### Self-Check Questions

- ✅ **Q1:** Did I list ALL shapes in execution order? YES - All shapes documented
- ✅ **Q2:** Did I verify data dependencies? YES - All dependencies verified
- ✅ **Q3:** Did I document conditional paths? YES - Branches and decisions documented
- ✅ **Q4:** Did I mark early exits? YES - Early exits marked with [EARLY EXIT]
- ✅ **Q5:** Did I include subprocesses? YES - All subprocesses documented
- ✅ **Q6:** Did I verify topological sort? YES - Execution order respects dependencies

---

## Sequence Diagram (Step 10)

```
┌─────────────┐
│ Mobile App  │
└──────┬──────┘
       │
       │ HTTP POST /latelogin?driver-id=1123
       │ Headers: company-code
       │ Body: { driverCreateRequest: { driver: { id, login: [{ dateTime, reason, remarks }] } } }
       ▼
┌──────────────────────────────────────────────────────────────────────────────┐
│ Late_Login Process (Boomi)                                                   │
├──────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  1. START (shape1) - Receive HTTP request                                   │
│     │                                                                        │
│     ▼                                                                        │
│  2. SET_INPUT (shape5) - Set input properties                               │
│     │ WRITE: DDP_driverId ← query_driver-id                                 │
│     │ WRITE: URL ← "api/services/.../lateloginrequest"                      │
│     │ WRITE: DPP_companyCode ← inheader_company-code                        │
│     ▼                                                                        │
│  3. SET_PROPS (shape16) - Set process properties                            │
│     │ WRITE: DPP_Process_Name, DPP_AtomName, DPP_Payload, etc.              │
│     ▼                                                                        │
│  4. TRY_CATCH (shape4) - Start try block                                    │
│     │                                                                        │
│     ├─── TRY PATH ───────────────────────────────────────────────────────┐  │
│     │                                                                      │  │
│     │  5. MAP_REQUEST (shape10) - Map input to D365 request               │  │
│     │     │ READ: DPP_companyCode                                         │  │
│     │     │ Map: driver.id → params.driverId                              │  │
│     │     │ Map: driver.login[0].dateTime → params.requestDateTime        │  │
│     │     │ Map: driver.login[0].reason → params.reasonCode               │  │
│     │     │ Map: driver.login[0].remarks → params.remarks                 │  │
│     │     ▼                                                                │  │
│     │  6. NOTIFY (shape25) - Log payload                                  │  │
│     │     ▼                                                                │  │
│     │  7. BRANCH_TOKEN (shape23) - Check if token exists                  │  │
│     │     │                                                                │  │
│     │     ├─── Path 1: Token NOT exists ──────────────────────┐           │  │
│     │     │                                                    │           │  │
│     │     │  8. GET_TOKEN (shape22) - Subprocess              │           │  │
│     │     │     │                                              │           │  │
│     │     │     ┌────────────────────────────────────────────┐│           │  │
│     │     │     │ D365_Token_Process Subprocess              ││           │  │
│     │     │     ├────────────────────────────────────────────┤│           │  │
│     │     │     │ 1. START                                   ││           │  │
│     │     │     │ 2. TRY_CATCH                               ││           │  │
│     │     │     │ 3. MESSAGE - Build token request           ││           │  │
│     │     │     │    Body: grant_type=...&client_id=...      ││           │  │
│     │     │     │ 4. CALL_TOKEN_API (HTTP POST)              ││           │  │
│     │     │     │    ├──────────────────────────────────────►││           │  │
│     │     │     │    │ Azure AD Token Endpoint               ││           │  │
│     │     │     │    │ POST /oauth2/token                    ││           │  │
│     │     │     │    ◄──────────────────────────────────────┤│           │  │
│     │     │     │    Response: { access_token, ... }        ││           │  │
│     │     │     │ 5. SET_TOKEN                               ││           │  │
│     │     │     │    WRITE: DPP_D365_Token ← "Bearer " +    ││           │  │
│     │     │     │           access_token                     ││           │  │
│     │     │     │ 6. STOP (continue: true)                   ││           │  │
│     │     │     │                                            ││           │  │
│     │     │     │ [ERROR PATH]                               ││           │  │
│     │     │     │ BRANCH_ERROR → SET_ERROR → EMAIL_ERROR     ││           │  │
│     │     │     │             OR THROW_ERROR                 ││           │  │
│     │     │     └────────────────────────────────────────────┘│           │  │
│     │     │     │                                              │           │  │
│     │     │     └──────────────────────────────────────────────┘           │  │
│     │     │                                                                │  │
│     │     └─── Path 2: Token exists ────────────────────────────────────┐ │  │
│     │                                                                    │ │  │
│     │  9. SET_TOKEN (shape24) - Set Authorization header ◄──────────────┴─┘  │
│     │     │ READ: DPP_D365_Token                                             │
│     │     │ WRITE: Authorization ← "Bearer " + DPP_D365_Token                │
│     │     ▼                                                                  │
│     │  10. CALL_D365 (shape6) - HTTP POST to D365                           │
│     │      │                                                                 │
│     │      ├─────────────────────────────────────────────────────────────►  │
│     │      │ Microsoft Dynamics 365 F&O                                     │
│     │      │ POST /api/services/.../lateloginrequest                        │
│     │      │ Headers: Authorization: Bearer <token>                         │
│     │      │ Body: { params: { driverId, requestDateTime, ... } }           │
│     │      ◄─────────────────────────────────────────────────────────────┤  │
│     │      Response: { Messages: [{ success, message, ... }] }              │
│     │      │                                                                 │
│     │      ▼                                                                 │
│     │  11. MAP_RESPONSE (shape19) - Map D365 response to Boomi response     │
│     │      │ Map: Messages[0].success → messages[0].status                  │
│     │      │ Map: Messages[0].message → messages[0].message                 │
│     │      ▼                                                                 │
│     │  12. SUCCESS (shape7) - Return success response                       │
│     │      │ HTTP 200 OK                                                    │
│     │      │ Body: { lateLoginResponse: { messages: [...] } }               │
│     │      │                                                                 │
│     └──────┴─────────────────────────────────────────────────────────────┘  │
│                                                                              │
│     ├─── CATCH PATH ────────────────────────────────────────────────────┐   │
│     │                                                                    │   │
│     │  13. BRANCH_ERROR (shape14) - 3-way error handling branch         │   │
│     │      │                                                             │   │
│     │      ├─── Path 1: Send Error Email ──────────────────────┐        │   │
│     │      │                                                    │        │   │
│     │      │  14. SET_ERROR (shape17)                          │        │   │
│     │      │      │ WRITE: DPP_ErrorMessage ← catch error      │        │   │
│     │      │      ▼                                             │        │   │
│     │      │  15. EMAIL_ERROR (shape15) - Subprocess           │        │   │
│     │      │      │                                             │        │   │
│     │      │      ┌──────────────────────────────────────────┐ │        │   │
│     │      │      │ (Sub) Office 365 Email Subprocess        │ │        │   │
│     │      │      ├──────────────────────────────────────────┤ │        │   │
│     │      │      │ 1. START                                 │ │        │   │
│     │      │      │ 2. TRY_CATCH                             │ │        │   │
│     │      │      │ 3. DECISION - Check DPP_HasAttachment    │ │        │   │
│     │      │      │    IF "Y": Send with attachment          │ │        │   │
│     │      │      │    ELSE: Send without attachment         │ │        │   │
│     │      │      │ 4. Build email body (HTML)               │ │        │   │
│     │      │      │ 5. Set mail properties                   │ │        │   │
│     │      │      │ 6. Send email (SMTP)                     │ │        │   │
│     │      │      │    ├────────────────────────────────────►│ │        │   │
│     │      │      │    │ Office 365 SMTP                     │ │        │   │
│     │      │      │    │ To: BoomiIntegrationTeam@...        │ │        │   │
│     │      │      │    │ Subject: <error subject>            │ │        │   │
│     │      │      │    │ Body: <error details>               │ │        │   │
│     │      │      │    ◄────────────────────────────────────┤│ │        │   │
│     │      │      │ 7. STOP                                  │ │        │   │
│     │      │      └──────────────────────────────────────────┘ │        │   │
│     │      │      │                                             │        │   │
│     │      │      │ [EARLY EXIT - NO RETURN]                   │        │   │
│     │      │      └─────────────────────────────────────────────┘        │   │
│     │      │                                                             │   │
│     │      ├─── Path 2: Return Failure Response ──────────────────┐     │   │
│     │      │                                                       │     │   │
│     │      │  16. MAP_ERROR (shape20) - Map error to response     │     │   │
│     │      │      │ READ: DPP_ErrorMessage                        │     │   │
│     │      │      │ Map: DPP_ErrorMessage → partnerView.message   │     │   │
│     │      │      │ Default: partnerView.success ← "false"        │     │   │
│     │      │      ▼                                                │     │   │
│     │      │  17. FAILURE (shape21) - Return failure response     │     │   │
│     │      │      │ HTTP 500 Internal Server Error                │     │   │
│     │      │      │ Body: { partnerView: { success: false, ... } }│     │   │
│     │      │      │                                                │     │   │
│     │      │      └────────────────────────────────────────────────┘     │   │
│     │      │                                                             │   │
│     │      └─── Path 3: Throw Exception ──────────────────────────┐     │   │
│     │                                                              │     │   │
│     │         18. THROW_ERROR (shape8) - Throw exception          │     │   │
│     │             │ Message: catch error message                  │     │   │
│     │             │ [EARLY EXIT - EXCEPTION THROWN]               │     │   │
│     │             └───────────────────────────────────────────────┘     │   │
│     │                                                                    │   │
│     └────────────────────────────────────────────────────────────────────┘   │
│                                                                              │
└──────────────────────────────────────────────────────────────────────────────┘
       │
       ▼
┌──────────────┐
│  Mobile App  │
│  (Response)  │
└──────────────┘
```

### Sequence Diagram Notes

1. **Main Flow (Success Path):**
   - Receives HTTP request with driver late login details
   - Sets input properties (driver ID, company code, URL)
   - Maps input to D365 request format
   - Checks if OAuth2 token exists (optimization)
   - If token not exists, calls D365_Token_Process subprocess to get token
   - Sets Authorization header with Bearer token
   - Calls D365 late login API
   - Maps D365 response to Boomi response format
   - Returns HTTP 200 OK with success response

2. **Error Flow (Catch Path):**
   - Catches any exception in try block
   - Branches to one of 3 error handling paths:
     - **Path 1:** Send error email to integration team (no return to caller)
     - **Path 2:** Map error to response and return HTTP 500
     - **Path 3:** Throw exception (no return to caller)

3. **Subprocesses:**
   - **D365_Token_Process:** Gets OAuth2 token from Azure AD
   - **(Sub) Office 365 Email:** Sends error notification email

4. **External Systems:**
   - **Azure AD Token Endpoint:** OAuth2 token generation
   - **Microsoft Dynamics 365 F&O:** Late login request submission
   - **Office 365 SMTP:** Error notification emails

---

## Function Exposure Decision Table (MANDATORY - BLOCKING)

### Decision Table

| Operation | Independent Invoke? | Decision Before/After? | Same SOR? | Internal Lookup? | Conclusion | Reasoning |
|-----------|-------------------|----------------------|-----------|------------------|------------|-----------|
| D365_Drivers_Latelogin (4e6e256a-8cb5-4eb4-a05c-3bac6a77788c) | YES | NO | N/A | NO | Azure Function | Process Layer needs to invoke late login independently. This is a complete business operation. |
| D365_Token_Connector (3d8b036c-d715-4853-a52c-1f6386417924) | NO | YES (before D365 call) | YES (D365) | YES | Atomic Handler (Internal) | Token retrieval is an internal lookup for authentication. Process Layer should NOT invoke this directly. This is handled by middleware/authentication. |
| Email w Attachment (af07502a-fafd-4976-a691-45d51a33b549) | NO | NO | N/A | NO | Atomic Handler (Internal) | Email is a cross-cutting concern handled by error handling middleware. Process Layer should NOT invoke this directly. |
| Email W/O Attachment (15a72a21-9b57-49a1-a8ed-d70367146644) | NO | NO | N/A | NO | Atomic Handler (Internal) | Email is a cross-cutting concern handled by error handling middleware. Process Layer should NOT invoke this directly. |

### Verification Questions

1. **Identified ALL decision points?** YES
   - Token retrieval decision (BRANCH_TOKEN - shape23)
   - Error handling decision (BRANCH_ERROR - shape14)
   - Email attachment decision (DECISION - shape4 in email subprocess)

2. **WHERE each decision belongs?**
   - **Token retrieval:** System Layer (authentication concern) - Handled by middleware
   - **Error handling:** System Layer (error handling concern) - Handled by middleware
   - **Email attachment:** System Layer (error notification concern) - Handled by middleware

3. **"if X exists, skip Y" checked?** YES
   - "if token exists, skip GET_TOKEN" → System Layer decision (optimization)

4. **"if flag=X, do Y" checked?** YES
   - "if DPP_HasAttachment=Y, send with attachment" → System Layer decision (error handling)

5. **Can explain WHY each operation type?** YES
   - **D365_Drivers_Latelogin:** Azure Function because Process Layer needs to invoke late login independently
   - **D365_Token_Connector:** Atomic Handler because it's an internal authentication lookup
   - **Email operations:** Atomic Handlers because they're cross-cutting error handling concerns

6. **Avoided pattern-matching?** YES - Analyzed each operation based on business requirements, not patterns

7. **If 1 Function, NO decision shapes?** NO - We have 1 Function (D365_Drivers_Latelogin), but decisions exist for authentication and error handling (which are System Layer concerns, not business decisions)

### Summary

**I will create 1 Azure Function for D365 (Microsoft Dynamics 365 F&O):**

**Function Name:** `SubmitDriverLateLoginRequest`

**Reasoning:**
- **Decision Points:** Token retrieval (BRANCH_TOKEN) and error handling (BRANCH_ERROR) are System Layer concerns, not business decisions.
- **Per Rule 1066:** Business decisions belong to Process Layer when they involve cross-SOR orchestration or complex business workflows. The decisions in this process are:
  - Token existence check → System Layer (authentication optimization)
  - Error handling strategy → System Layer (error handling concern)
  - Email attachment flag → System Layer (error notification concern)
- **None of these are business decisions** that Process Layer should make.

**Function Purpose:**
- Submit driver late login request to D365 F&O
- Accept driver ID, late login date/time, reason code, remarks, and company code
- Return success/failure response from D365

**Internal Operations (Atomic Handlers):**
- `AuthenticateD365AtomicHandler` - Get OAuth2 token from Azure AD (internal authentication)
- `SubmitLateLoginAtomicHandler` - HTTP POST to D365 late login API

**Authentication:**
- OAuth2 Bearer token authentication
- Token retrieval handled by `CustomAuthenticationMiddleware` (per System Layer Rules)
- Token caching/reuse handled by middleware

**Error Handling:**
- Email notifications handled by `ExceptionHandlerMiddleware` (per System Layer Rules)
- No email operations exposed as Functions

---

## Self-Check Questions (MANDATORY)

### Phase 1 Completion Checklist

- ✅ **Q1: Operations Inventory Complete?** YES - All operations documented (5 operations total)
- ✅ **Q2: Input Structure Analysis (Step 1a) Complete?** YES - Entry point and input profile fully analyzed
- ✅ **Q3: Response Structure Analysis (Step 1b) Complete?** YES - Response profiles documented
- ✅ **Q4: Operation Response Analysis (Step 1c) Complete?** YES - All operation responses analyzed
- ✅ **Q5: Map Analysis (Step 1d) Complete?** YES - All 3 maps analyzed, field names documented
- ✅ **Q6: HTTP Status Codes (Step 1e) Complete?** YES - All return paths and status codes documented
- ✅ **Q7: Property WRITES/READS (Steps 2-3) Complete?** YES - All property writes and reads documented
- ✅ **Q8: Data Dependency Graph (Step 4) Complete?** YES - Dependency graph and topological sort complete
- ✅ **Q9: Control Flow Graph (Step 5) Complete?** YES - All nodes and edges documented
- ✅ **Q10: Decision Shape Analysis (Step 7) Complete?** YES - All decisions analyzed with self-checks
- ✅ **Q11: Branch Shape Analysis (Step 8) Complete?** YES - All branches analyzed with self-checks
- ✅ **Q12: Execution Order (Step 9) Complete?** YES - Complete execution sequence documented
- ✅ **Q13: Sequence Diagram (Step 10) Complete?** YES - Full sequence diagram created
- ✅ **Q14: Function Exposure Decision Table Complete?** YES - Decision table and verification complete
- ✅ **Q15: All self-check questions answered with YES?** YES - All questions answered affirmatively

### Validation Checkpoint

**Before proceeding to Phase 2, verify:**

- ✅ BOOMI_EXTRACTION_PHASE1.md exists and is committed
- ✅ All mandatory sections present (Steps 1a-1e, 2-10)
- ✅ All self-check questions answered with YES
- ✅ Function Exposure Decision Table complete
- ✅ Sequence diagram references all prior steps

**PHASE 1 EXTRACTION COMPLETE - READY FOR PHASE 2 CODE GENERATION**

---

## End of Phase 1 Document
