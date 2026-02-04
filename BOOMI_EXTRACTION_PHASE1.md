# BOOMI EXTRACTION PHASE 1: HCM Leave Create Process

## Process Overview
**Process Name:** HCM_Leave Create  
**Process ID:** ca69f858-785f-4565-ba1f-b4edc6cca05b  
**Description:** This Process will sync the Leave data between D365 and Oracle HCM  
**System of Record (SOR):** Oracle Fusion HCM  
**Business Domain:** Human Resource (HCM - Leaves)

---

## Operations Inventory

### Operation 1: Create Leave Oracle Fusion OP (WebService Listen)
- **ID:** 8f709c2b-e63f-4d5f-9374-2932ed70415d
- **Type:** WebService Server Listen (WSS - HTTP Trigger)
- **Action:** Listen
- **Purpose:** HTTP entry point that receives leave creation requests
- **Input Profile:** febfa3e1-f719-4ee8-ba57-cdae34137ab3 (D365 Leave Create JSON Profile)
- **Output Profile:** singlejson
- **Object Name:** leave

### Operation 2: Leave Oracle Fusion Create (HTTP POST)
- **ID:** 6e8920fd-af5a-430b-a1d9-9fde7ac29a12
- **Type:** HTTP Send
- **Action:** POST
- **Connection:** aa1fcb29-d146-4425-9ea6-b9698090f60e (Oracle Fusion)
- **Purpose:** Create leave absence record in Oracle Fusion HCM
- **Method:** POST
- **Content-Type:** application/json
- **URL:** Dynamic (set via document property from Resource_Path)
- **Authentication:** Basic Auth (via connection)
- **Response Header Mapping:** Content-Encoding → DDP_RespHeader

### Operation 3: Email w Attachment (Email Send)
- **ID:** af07502a-fafd-4976-a691-45d51a33b549
- **Type:** Mail Send
- **Connection:** 00eae79b-2303-4215-8067-dcc299e42697 (Office 365 Email)
- **Purpose:** Send error notification email with attachment
- **Body Content-Type:** text/html
- **Data Content-Type:** text/plain
- **Disposition:** attachment

### Operation 4: Email W/O Attachment (Email Send)
- **ID:** 15a72a21-9b57-49a1-a8ed-d70367146644
- **Type:** Mail Send
- **Connection:** 00eae79b-2303-4215-8067-dcc299e42697 (Office 365 Email)
- **Purpose:** Send error notification email without attachment
- **Body Content-Type:** text/plain
- **Data Content-Type:** text/html
- **Disposition:** inline

---

## Step 1a: Input Structure Analysis (BLOCKING)

### HTTP Trigger Input (Operation 8f709c2b-e63f-4d5f-9374-2932ed70415d)
**Profile:** febfa3e1-f719-4ee8-ba57-cdae34137ab3 (D365 Leave Create JSON Profile)

**Input JSON Structure:**
```json
{
  "employeeNumber": <number>,
  "absenceType": "<string>",
  "employer": "<string>",
  "startDate": "<string>",
  "endDate": "<string>",
  "absenceStatusCode": "<string>",
  "approvalStatusCode": "<string>",
  "startDateDuration": <number>,
  "endDateDuration": <number>
}
```

**Field Definitions:**
- `employeeNumber` (number, required): Employee identifier from D365
- `absenceType` (string, required): Type of leave/absence
- `employer` (string, required): Employer organization name
- `startDate` (string, required): Leave start date (format: YYYY-MM-DD)
- `endDate` (string, required): Leave end date (format: YYYY-MM-DD)
- `absenceStatusCode` (string, required): Status code of the absence (e.g., "SUBMITTED")
- `approvalStatusCode` (string, required): Approval status code (e.g., "APPROVED")
- `startDateDuration` (number, required): Duration for start date (typically 1 for full day)
- `endDateDuration` (number, required): Duration for end date (typically 1 for full day)

**Sample Input:**
```json
{
  "employeeNumber": 9000604,
  "absenceType": "Sick Leave",
  "employer": "Al Ghurair Investment LLC",
  "startDate": "2024-03-24",
  "endDate": "2024-03-25",
  "absenceStatusCode": "SUBMITTED",
  "approvalStatusCode": "APPROVED",
  "startDateDuration": 1,
  "endDateDuration": 1
}
```

---

## Step 1b: Response Structure Analysis (BLOCKING)

### Success Response (HTTP 20x)
**Profile:** 316175c7-0e45-4869-9ac6-5f9d69882a62 (Oracle Fusion Leave Response JSON Profile)

**Oracle Fusion HCM Response Structure (Partial - Key Fields):**
```json
{
  "personAbsenceEntryId": <number>,
  "absenceCaseId": "<string>",
  "absenceTypeId": <number>,
  "absenceStatusCd": "<string>",
  "approvalStatusCd": "<string>",
  "personNumber": "<string>",
  "absenceType": "<string>",
  "employer": "<string>",
  "startDate": "<string>",
  "endDate": "<string>",
  "startDateDuration": <number>,
  "endDateDuration": <number>",
  "duration": <number>,
  ...
  "links": [...]
}
```

**Mapped Success Response to Caller:**
**Profile:** f4ca3a70-114a-4601-bad8-44a3eb20e2c0 (Leave D365 Response)
```json
{
  "status": "success",
  "message": "Data successfully sent to Oracle Fusion",
  "personAbsenceEntryId": <number>,
  "success": "true"
}
```

### Error Response (HTTP Non-20x or Exception)
**Profile:** f4ca3a70-114a-4601-bad8-44a3eb20e2c0 (Leave D365 Response)
```json
{
  "status": "failure",
  "message": "<error_message_from_DPP_ErrorMessage>",
  "success": "false"
}
```

**Error Scenarios:**
1. **Try/Catch Exception:** Any exception during processing → error response + email notification
2. **HTTP Status Non-20x:** Oracle Fusion returns non-success status → error response + email notification
3. **GZIP Encoded Error:** Response is GZIP encoded (Content-Encoding: gzip) → decompress → error response + email notification

---

## Step 1c: Operation Response Analysis (BLOCKING)

### Operation 6e8920fd-af5a-430b-a1d9-9fde7ac29a12 (Leave Oracle Fusion Create)
**HTTP Method:** POST  
**Return Errors:** true  
**Return Responses:** true  

**Success Response (HTTP 20x):**
- **Status Codes:** 200, 201, 202, 203, 204, 205, 206, 207, 208, 209 (any 20x)
- **Content-Type:** application/json
- **Body:** Oracle Fusion absence entry object (see Step 1b)
- **Key Field:** `personAbsenceEntryId` (unique identifier for created absence)

**Error Response (HTTP Non-20x):**
- **Status Codes:** 4xx, 5xx
- **Content-Type:** application/json or text/plain
- **Body:** Error message/details
- **Possible Encoding:** GZIP (Content-Encoding: gzip) - requires decompression

**Response Header Captured:**
- `Content-Encoding` → mapped to `DDP_RespHeader` document property

---

## Step 1d: Map Analysis (BLOCKING)

### Map 1: Leave Create Map (c426b4d6-2aff-450e-b43b-59956c4dbc96)
**From Profile:** febfa3e1-f719-4ee8-ba57-cdae34137ab3 (D365 Leave Create JSON Profile)  
**To Profile:** a94fa205-c740-40a5-9fda-3d018611135a (HCM Leave Create JSON Profile)

**Field Mappings:**
```
employeeNumber       → personNumber
absenceType          → absenceType (direct)
employer             → employer (direct)
startDate            → startDate (direct)
endDate              → endDate (direct)
absenceStatusCode    → absenceStatusCd
approvalStatusCode   → approvalStatusCd
startDateDuration    → startDateDuration (direct)
endDateDuration      → endDateDuration (direct)
```

**Purpose:** Transform D365 input format to Oracle Fusion HCM API format  
**Key Transformation:** `employeeNumber` → `personNumber` (field name change)

### Map 2: Oracle Fusion Leave Response Map (e4fd3f59-edb5-43a1-aeae-143b600a064e)
**From Profile:** 316175c7-0e45-4869-9ac6-5f9d69882a62 (Oracle Fusion Leave Response JSON Profile)  
**To Profile:** f4ca3a70-114a-4601-bad8-44a3eb20e2c0 (Leave D365 Response)

**Field Mappings:**
```
personAbsenceEntryId → personAbsenceEntryId
(static) "success"   → status
(static) "Data successfully sent to Oracle Fusion" → message
(static) "true"      → success
```

**Purpose:** Transform Oracle Fusion success response to standardized response format

### Map 3: Leave Error Map (f46b845a-7d75-41b5-b0ad-c41a6a8e9b12)
**From Profile:** 23d7a2e9-5cb0-4e9c-9e4b-3154834bad0d (Dummy FF Profile)  
**To Profile:** f4ca3a70-114a-4601-bad8-44a3eb20e2c0 (Leave D365 Response)

**Field Mappings:**
```
DPP_ErrorMessage (from process property) → message
(static) "failure"                       → status
(static) "false"                         → success
```

**Purpose:** Transform error details to standardized error response format  
**Function Used:** Get Dynamic Process Property (PropertyGet) to retrieve `DPP_ErrorMessage`

---

## Step 1e: HTTP Status Codes and Return Paths (BLOCKING)

### HTTP Status Code Decision (shape2)
**Decision Shape:** "HTTP Status 20 check"  
**Comparison:** Wildcard match  
**Property Checked:** `meta.base.applicationstatuscode` (Base - Application Status Code)  
**Pattern:** `20*` (any status code starting with 20)

**Return Paths:**

#### Path 1: Success (HTTP 20x) → shape2.dragpoint1 (True)
**Flow:** shape2 → shape34 (Map: Oracle Fusion Leave Response Map) → shape35 (Return: Success Response)  
**HTTP Status:** 200, 201, 202, 203, 204, 205, 206, 207, 208, 209  
**Response:** Success JSON with `personAbsenceEntryId`

#### Path 2: Error (HTTP Non-20x) → shape2.dragpoint2 (False)
**Flow:** shape2 → shape44 (Decision: Check Response Content Type)  
**HTTP Status:** 400, 401, 403, 404, 500, 502, 503, 504, etc.

##### Path 2a: GZIP Encoded Error → shape44.dragpoint1 (True)
**Condition:** `DDP_RespHeader` == "gzip"  
**Flow:** shape44 → shape45 (Data Process: GZIP Decompression) → shape46 (Set Error Message) → shape47 (Map: Leave Error Map) → shape48 (Return: Error Response)  
**Groovy Script (shape45):**
```groovy
import java.util.zip.GZIPInputStream
0.upto(dataContext.getDataCount()-1) {
     dataContext.storeStream(new GZIPInputStream(dataContext.getStream(it)), dataContext.getProperties(it))
}
```

##### Path 2b: Plain Error → shape44.dragpoint2 (False)
**Condition:** `DDP_RespHeader` != "gzip"  
**Flow:** shape44 → shape39 (Set Error Message) → shape40 (Map: Leave Error Map) → shape36 (Return: Error Response)

#### Path 3: Exception (Try/Catch) → shape17.dragpoint2 (Catch)
**Flow:** shape17 (Catch) → shape20 (Branch: 2 branches)  
**Branch 1:** shape20 → shape19 (Set Error Message) → shape21 (Process Call: Email Subprocess) → (End)  
**Branch 2:** shape20 → shape41 (Map: Leave Error Map) → shape43 (Return: Error Response)  
**Error Message Source:** `meta.base.catcherrorsmessage` (Base - Try/Catch Message)

**Summary of HTTP Status Handling:**
- **20x:** Success response with `personAbsenceEntryId`
- **Non-20x (GZIP):** Decompress → Error response + Email notification
- **Non-20x (Plain):** Error response + Email notification
- **Exception:** Error response + Email notification (via subprocess)

---

## Steps 2-3: Process Properties Analysis

### Process Properties WRITES (Set Operations)

#### shape38: Input_details (Document Properties)
**Location:** After Start (shape1)  
**Properties Set:**
1. `process.DPP_Process_Name` ← Execution Property: "Process Name"
2. `process.DPP_AtomName` ← Execution Property: "Atom Name"
3. `process.DPP_Payload` ← Current document (input payload)
4. `process.DPP_ExecutionID` ← Execution Property: "Execution Id"
5. `process.DPP_File_Name` ← Concatenation: Process Name + Current DateTime (yyyy-MM-dd'T'HH:mm:ss.SSS'Z') + ".txt"
6. `process.DPP_Subject` ← Concatenation: Atom Name + " (" + Process Name + " ) has errors to report"
7. `process.To_Email` ← Defined Process Parameter: PP_HCM_LeaveCreate_Properties.To_Email
8. `process.DPP_HasAttachment` ← Defined Process Parameter: PP_HCM_LeaveCreate_Properties.DPP_HasAttachment

#### shape8: set URL (Document Property)
**Location:** After Map (shape29)  
**Property Set:**
1. `dynamicdocument.URL` ← Defined Process Parameter: PP_HCM_LeaveCreate_Properties.Resource_Path

#### shape19: ErrorMsg (Document Property)
**Location:** In Catch block (Branch 1)  
**Property Set:**
1. `process.DPP_ErrorMessage` ← Track Parameter: "meta.base.catcherrorsmessage" (Base - Try/Catch Message)

#### shape39: error msg (Document Property)
**Location:** In Error path (Non-20x, Non-GZIP)  
**Property Set:**
1. `process.DPP_ErrorMessage` ← Track Parameter: "meta.base.applicationstatusmessage" (Base - Application Status Message)

#### shape46: error msg (Document Property)
**Location:** In Error path (Non-20x, GZIP)  
**Property Set:**
1. `process.DPP_ErrorMessage` ← Current document (decompressed error message)

### Process Properties READS (Get Operations)

#### Defined Process Parameters (PP_HCM_LeaveCreate_Properties)
**Component ID:** e22c04db-7dfa-4b1b-9d88-77365b0fdb18

**Parameters:**
1. **Resource_Path** (c2d1a1e8-4bcb-435b-b1d2-bb10a209bcc0)
   - Default: "hcmRestApi/resources/11.13.18.05/absences"
   - Used in: shape8 (set URL)
   - Purpose: Oracle Fusion HCM API endpoint path

2. **To_Email** (71c90f6e-4f86-49f3-8aef-bae6668c73f9)
   - Default: "BoomiIntegrationTeam@al-ghurair.com"
   - Used in: shape38 (Input_details)
   - Purpose: Email recipient for error notifications

3. **DPP_HasAttachment** (a717867b-67f4-4a72-be2d-c99c73309fdb)
   - Default: "Y"
   - Allowed Values: "Y", "N"
   - Used in: shape38 (Input_details)
   - Purpose: Flag to determine if email should have attachment

#### Subprocess Parameters (PP_Office365_Email)
**Component ID:** 0feff13f-8a2c-438a-b3c7-1909e2a7f533

**Parameters (used in subprocess a85945c5-3004-42b9-80b1-104f465cd1fb):**
1. **From_Email** (804b6d40-eeee-4ac0-ab4b-94e1aca9f4b7)
   - Default: "Boomi.Dev.failures@al-ghurair.com"
2. **Environment** (d8fc7496-ec2c-4308-a4ca-e9f49c0c9500)
   - Default: "DEV Failure :"
   - Allowed Values: "DEV Failure :", "QA Failure :", "PROD Failure :"

### Dynamic Process Properties (Internal State)
1. `process.DPP_Process_Name` - Process name for logging/email
2. `process.DPP_AtomName` - Atom name for logging/email
3. `process.DPP_Payload` - Original input payload for email attachment
4. `process.DPP_ExecutionID` - Execution ID for tracking
5. `process.DPP_File_Name` - Generated filename for email attachment
6. `process.DPP_Subject` - Email subject line
7. `process.To_Email` - Email recipient
8. `process.DPP_HasAttachment` - Email attachment flag
9. `process.DPP_ErrorMessage` - Error message for response/email
10. `dynamicdocument.URL` - Dynamic URL for HTTP call

### Connection Properties

#### Oracle Fusion Connection (aa1fcb29-d146-4425-9ea6-b9698090f60e)
- **Base URL:** https://iaaxey-dev3.fa.ocs.oraclecloud.com:443
- **Authentication:** Basic Auth
- **User:** INTEGRATION.USER@al-ghurair.com
- **Password:** (encrypted)

#### Office 365 Email Connection (00eae79b-2303-4215-8067-dcc299e42697)
- **Host:** smtp-mail.outlook.com
- **Port:** 587
- **Use SSL:** false
- **Use TLS:** true
- **Use SMTP AUTH:** true
- **User:** Boomi.Dev.failures@al-ghurair.com
- **Password:** (encrypted)

---

## Step 4: Data Dependency Graph (BLOCKING)

### Node Definitions
```
[INPUT] = HTTP Trigger Input (D365 Leave JSON)
[PROP_INIT] = Process Properties Initialization (shape38)
[MAP_REQUEST] = Leave Create Map (shape29)
[SET_URL] = Set URL Property (shape8)
[HTTP_CALL] = Oracle Fusion Create Leave API (shape33)
[STATUS_CHECK] = HTTP Status 20x Check (shape2)
[MAP_SUCCESS] = Oracle Fusion Leave Response Map (shape34)
[RETURN_SUCCESS] = Return Success Response (shape35)
[GZIP_CHECK] = Check Response Content Type (shape44)
[GZIP_DECOMPRESS] = GZIP Decompression (shape45)
[SET_ERROR_GZIP] = Set Error Message from Decompressed (shape46)
[MAP_ERROR_GZIP] = Leave Error Map (shape47)
[RETURN_ERROR_GZIP] = Return Error Response (shape48)
[SET_ERROR_PLAIN] = Set Error Message from Status (shape39)
[MAP_ERROR_PLAIN] = Leave Error Map (shape40)
[RETURN_ERROR_PLAIN] = Return Error Response (shape36)
[CATCH] = Exception Handler (shape17 catch)
[BRANCH] = Branch 2 ways (shape20)
[SET_ERROR_CATCH] = Set Error Message from Exception (shape19)
[EMAIL_SUBPROCESS] = Email Notification Subprocess (shape21)
[MAP_ERROR_CATCH] = Leave Error Map (shape41)
[RETURN_ERROR_CATCH] = Return Error Response (shape43)
```

### Data Flow Dependencies

#### Happy Path (Success)
```
[INPUT] → [PROP_INIT] → [MAP_REQUEST] → [SET_URL] → [HTTP_CALL] → [STATUS_CHECK] → [MAP_SUCCESS] → [RETURN_SUCCESS]
```

**Data Transformations:**
1. `[INPUT]` provides: employeeNumber, absenceType, employer, startDate, endDate, absenceStatusCode, approvalStatusCode, startDateDuration, endDateDuration
2. `[PROP_INIT]` captures: Process metadata, input payload, email settings
3. `[MAP_REQUEST]` transforms: employeeNumber→personNumber, absenceStatusCode→absenceStatusCd, approvalStatusCode→approvalStatusCd
4. `[SET_URL]` sets: dynamicdocument.URL = Resource_Path
5. `[HTTP_CALL]` sends: Mapped JSON to Oracle Fusion HCM API
6. `[STATUS_CHECK]` evaluates: HTTP status code (20x?)
7. `[MAP_SUCCESS]` extracts: personAbsenceEntryId from Oracle response
8. `[RETURN_SUCCESS]` returns: {status: "success", message: "...", personAbsenceEntryId: <id>, success: "true"}

#### Error Path 1 (HTTP Non-20x, GZIP Encoded)
```
[INPUT] → [PROP_INIT] → [MAP_REQUEST] → [SET_URL] → [HTTP_CALL] → [STATUS_CHECK] → [GZIP_CHECK] → [GZIP_DECOMPRESS] → [SET_ERROR_GZIP] → [MAP_ERROR_GZIP] → [RETURN_ERROR_GZIP]
```

**Data Transformations:**
1. `[STATUS_CHECK]` detects: HTTP status != 20x
2. `[GZIP_CHECK]` detects: Content-Encoding == "gzip"
3. `[GZIP_DECOMPRESS]` decompresses: GZIP response body
4. `[SET_ERROR_GZIP]` sets: DPP_ErrorMessage = decompressed error text
5. `[MAP_ERROR_GZIP]` transforms: DPP_ErrorMessage → message, status="failure", success="false"
6. `[RETURN_ERROR_GZIP]` returns: Error response JSON

#### Error Path 2 (HTTP Non-20x, Plain)
```
[INPUT] → [PROP_INIT] → [MAP_REQUEST] → [SET_URL] → [HTTP_CALL] → [STATUS_CHECK] → [GZIP_CHECK] → [SET_ERROR_PLAIN] → [MAP_ERROR_PLAIN] → [RETURN_ERROR_PLAIN]
```

**Data Transformations:**
1. `[STATUS_CHECK]` detects: HTTP status != 20x
2. `[GZIP_CHECK]` detects: Content-Encoding != "gzip"
3. `[SET_ERROR_PLAIN]` sets: DPP_ErrorMessage = meta.base.applicationstatusmessage
4. `[MAP_ERROR_PLAIN]` transforms: DPP_ErrorMessage → message, status="failure", success="false"
5. `[RETURN_ERROR_PLAIN]` returns: Error response JSON

#### Error Path 3 (Exception)
```
[INPUT] → [PROP_INIT] → [CATCH] → [BRANCH]
  Branch 1: [BRANCH] → [SET_ERROR_CATCH] → [EMAIL_SUBPROCESS] → (End)
  Branch 2: [BRANCH] → [MAP_ERROR_CATCH] → [RETURN_ERROR_CATCH]
```

**Data Transformations:**
1. `[CATCH]` captures: Exception during processing
2. `[BRANCH]` splits: Parallel error handling
3. Branch 1:
   - `[SET_ERROR_CATCH]` sets: DPP_ErrorMessage = meta.base.catcherrorsmessage
   - `[EMAIL_SUBPROCESS]` sends: Error notification email
4. Branch 2:
   - `[MAP_ERROR_CATCH]` transforms: DPP_ErrorMessage → message, status="failure", success="false"
   - `[RETURN_ERROR_CATCH]` returns: Error response JSON

### Field-Level Dependencies

#### Input → Oracle Fusion Request
```
INPUT.employeeNumber      → MAP_REQUEST → personNumber      → HTTP_CALL
INPUT.absenceType         → MAP_REQUEST → absenceType       → HTTP_CALL
INPUT.employer            → MAP_REQUEST → employer          → HTTP_CALL
INPUT.startDate           → MAP_REQUEST → startDate         → HTTP_CALL
INPUT.endDate             → MAP_REQUEST → endDate           → HTTP_CALL
INPUT.absenceStatusCode   → MAP_REQUEST → absenceStatusCd   → HTTP_CALL
INPUT.approvalStatusCode  → MAP_REQUEST → approvalStatusCd  → HTTP_CALL
INPUT.startDateDuration   → MAP_REQUEST → startDateDuration → HTTP_CALL
INPUT.endDateDuration     → MAP_REQUEST → endDateDuration   → HTTP_CALL
```

#### Oracle Fusion Response → Success Response
```
HTTP_CALL.response.personAbsenceEntryId → MAP_SUCCESS → personAbsenceEntryId → RETURN_SUCCESS
(static) "success"                      → MAP_SUCCESS → status                → RETURN_SUCCESS
(static) "Data successfully sent..."    → MAP_SUCCESS → message               → RETURN_SUCCESS
(static) "true"                         → MAP_SUCCESS → success               → RETURN_SUCCESS
```

#### Error Handling → Error Response
```
CATCH.meta.base.catcherrorsmessage           → SET_ERROR_CATCH  → DPP_ErrorMessage → MAP_ERROR_CATCH  → message → RETURN_ERROR_CATCH
HTTP_CALL.meta.base.applicationstatusmessage → SET_ERROR_PLAIN  → DPP_ErrorMessage → MAP_ERROR_PLAIN  → message → RETURN_ERROR_PLAIN
HTTP_CALL.response (decompressed)            → SET_ERROR_GZIP   → DPP_ErrorMessage → MAP_ERROR_GZIP   → message → RETURN_ERROR_GZIP
(static) "failure"                           → MAP_ERROR_*      → status           → RETURN_ERROR_*
(static) "false"                             → MAP_ERROR_*      → success          → RETURN_ERROR_*
```

---

## Step 5: Control Flow Graph

### Shape Flow Diagram
```
START (shape1: HTTP Trigger)
  ↓
shape38 (Document Properties: Input_details)
  ↓
shape17 (Try/Catch: catchAll=true)
  ├─[TRY]──→ shape29 (Map: Leave Create Map)
  │           ↓
  │         shape8 (Document Property: set URL)
  │           ↓
  │         shape49 (Notify: Log payload)
  │           ↓
  │         shape33 (Connector: HTTP POST to Oracle Fusion)
  │           ↓
  │         shape2 (Decision: HTTP Status 20 check)
  │           ├─[TRUE: 20*]──→ shape34 (Map: Oracle Fusion Leave Response Map)
  │           │                  ↓
  │           │                shape35 (Return: Success Response)
  │           │                  ↓
  │           │                END
  │           │
  │           └─[FALSE: Non-20*]──→ shape44 (Decision: Check Response Content Type)
  │                                   ├─[TRUE: gzip]──→ shape45 (Data Process: GZIP Decompress)
  │                                   │                  ↓
  │                                   │                shape46 (Document Property: error msg)
  │                                   │                  ↓
  │                                   │                shape47 (Map: Leave Error Map)
  │                                   │                  ↓
  │                                   │                shape48 (Return: Error Response)
  │                                   │                  ↓
  │                                   │                END
  │                                   │
  │                                   └─[FALSE: plain]──→ shape39 (Document Property: error msg)
  │                                                        ↓
  │                                                      shape40 (Map: Leave Error Map)
  │                                                        ↓
  │                                                      shape36 (Return: Error Response)
  │                                                        ↓
  │                                                      END
  │
  └─[CATCH]──→ shape20 (Branch: numBranches=2)
                ├─[Branch 1]──→ shape19 (Document Property: ErrorMsg)
                │                ↓
                │              shape21 (Process Call: Email Subprocess)
                │                ↓
                │              END
                │
                └─[Branch 2]──→ shape41 (Map: Leave Error Map)
                                 ↓
                               shape43 (Return: Error Response)
                                 ↓
                               END
```

### Control Flow Paths

#### Path 1: Success Flow
**Sequence:** shape1 → shape38 → shape17[TRY] → shape29 → shape8 → shape49 → shape33 → shape2[TRUE] → shape34 → shape35 → END  
**Condition:** HTTP POST succeeds with 20x status  
**Result:** Success response with personAbsenceEntryId

#### Path 2: Error Flow (Non-20x, GZIP)
**Sequence:** shape1 → shape38 → shape17[TRY] → shape29 → shape8 → shape49 → shape33 → shape2[FALSE] → shape44[TRUE] → shape45 → shape46 → shape47 → shape48 → END  
**Condition:** HTTP POST returns non-20x with Content-Encoding: gzip  
**Result:** Error response with decompressed error message

#### Path 3: Error Flow (Non-20x, Plain)
**Sequence:** shape1 → shape38 → shape17[TRY] → shape29 → shape8 → shape49 → shape33 → shape2[FALSE] → shape44[FALSE] → shape39 → shape40 → shape36 → END  
**Condition:** HTTP POST returns non-20x without GZIP encoding  
**Result:** Error response with status message

#### Path 4: Exception Flow
**Sequence:** shape1 → shape38 → shape17[CATCH] → shape20 → [Branch 1: shape19 → shape21 → END] + [Branch 2: shape41 → shape43 → END]  
**Condition:** Any exception during Try block  
**Result:** Error response + Email notification (parallel)

### Loop Detection
**Result:** NO LOOPS DETECTED  
**Reasoning:** All paths are linear or branching; no shape points back to a previous shape. The process is strictly forward-flowing with decision branches but no iteration.

---

## Step 7: Decision Shape Analysis (BLOCKING)

### Decision 1: HTTP Status 20 check (shape2)
**Shape ID:** shape2  
**User Label:** "HTTP Status 20 check"  
**Comparison Type:** wildcard  
**Decision Logic:**
- **Left Operand:** Track Parameter `meta.base.applicationstatuscode` (Base - Application Status Code)
- **Right Operand:** Static value "20*"
- **Evaluation:** Wildcard match - checks if HTTP status code starts with "20"

**Branches:**
1. **TRUE (shape2.dragpoint1):** HTTP status is 20x (success)
   - **Target:** shape34 (Map: Oracle Fusion Leave Response Map)
   - **Purpose:** Process successful Oracle Fusion response
   
2. **FALSE (shape2.dragpoint2):** HTTP status is NOT 20x (error)
   - **Target:** shape44 (Decision: Check Response Content Type)
   - **Purpose:** Handle error response (check if GZIP encoded)

**Business Logic:** This decision determines if the Oracle Fusion HCM API call was successful based on HTTP status code. Any 20x status (200, 201, 202, etc.) is considered success; all other statuses (4xx, 5xx) are errors.

**System Layer Implication:** This is a technical HTTP status check, NOT business logic. The System Layer should expose the raw HTTP response and let the Process Layer decide how to handle different status codes. However, basic error handling (throwing exceptions for non-2xx) is acceptable in System Layer.

### Decision 2: Check Response Content Type (shape44)
**Shape ID:** shape44  
**User Label:** "Check Response Content Type"  
**Comparison Type:** equals  
**Decision Logic:**
- **Left Operand:** Track Parameter `dynamicdocument.DDP_RespHeader` (Dynamic Document Property - DDP_RespHeader)
- **Right Operand:** Static value "gzip"
- **Evaluation:** Exact match - checks if Content-Encoding header is "gzip"

**Branches:**
1. **TRUE (shape44.dragpoint1):** Response is GZIP encoded
   - **Target:** shape45 (Data Process: GZIP Decompression)
   - **Purpose:** Decompress GZIP response before processing
   
2. **FALSE (shape44.dragpoint2):** Response is NOT GZIP encoded
   - **Target:** shape39 (Document Property: error msg)
   - **Purpose:** Process plain text error response

**Business Logic:** This decision handles technical response encoding. If Oracle Fusion returns a GZIP-compressed error response, it must be decompressed before extracting the error message.

**System Layer Implication:** This is a technical concern (response encoding). The System Layer Atomic Handler should handle GZIP decompression transparently if the response is compressed, so the Handler/Service doesn't need to deal with encoding.

### Decision 3: Attachment_Check (shape4 in Subprocess a85945c5-3004-42b9-80b1-104f465cd1fb)
**Shape ID:** shape4 (in subprocess)  
**User Label:** "Attachment_Check"  
**Comparison Type:** equals  
**Decision Logic:**
- **Left Operand:** Process Parameter `DPP_HasAttachment`
- **Right Operand:** Static value "Y"
- **Evaluation:** Exact match - checks if email should have attachment

**Branches:**
1. **TRUE (shape4.dragpoint1):** Attachment required
   - **Target:** shape11 (Message: Mail_Body with attachment)
   - **Purpose:** Send email with payload attachment
   
2. **FALSE (shape4.dragpoint2):** No attachment
   - **Target:** shape23 (Message: Mail_Body without attachment)
   - **Purpose:** Send email without attachment

**Business Logic:** This decision determines email format based on configuration. This is part of the error notification subprocess.

**System Layer Implication:** Email notification is a cross-cutting concern and should NOT be in System Layer. This belongs to Process Layer or a separate notification service.

### Self-Check Questions

**Q1: Have I identified ALL decision shapes in the main process and subprocesses?**  
**A:** YES - Identified 3 decision shapes:
- shape2 (HTTP Status 20 check) in main process
- shape44 (Check Response Content Type) in main process
- shape4 (Attachment_Check) in subprocess a85945c5-3004-42b9-80b1-104f465cd1fb

**Q2: For EACH decision, have I documented the comparison type, operands, and branch targets?**  
**A:** YES - All decisions documented with:
- Comparison type (wildcard, equals)
- Left and right operands
- Branch targets (TRUE/FALSE dragpoints)
- Purpose of each branch

**Q3: Have I identified which decisions are business logic vs. technical checks?**  
**A:** YES - Categorized:
- **Technical Checks:** shape2 (HTTP status), shape44 (GZIP encoding)
- **Configuration Check:** shape4 (Email attachment flag)
- **Business Logic:** NONE in this process (all decisions are technical/configuration)

**Q4: Have I determined which decisions belong in System Layer vs. Process Layer?**  
**A:** YES - Analysis:
- shape2 (HTTP status check): System Layer can throw exceptions for non-2xx, but should expose raw response
- shape44 (GZIP check): System Layer Atomic Handler should handle transparently
- shape4 (Email attachment): NOT System Layer - belongs to Process Layer or notification service

**Q5: Can I explain WHY each decision is placed in its designated layer?**  
**A:** YES - Reasoning:
- **shape2:** HTTP status checking is acceptable in System Layer for basic error handling, but detailed status code logic belongs to Process Layer
- **shape44:** Response encoding/decoding is a technical concern that System Layer should handle transparently
- **shape4:** Email notification logic is business workflow orchestration, NOT data access - belongs to Process Layer

---

## Step 8: Branch Shape Analysis (BLOCKING)

### Branch 1: Error Handling Branch (shape20)
**Shape ID:** shape20  
**User Label:** (none)  
**Location:** In Catch block (shape17 error path)  
**Number of Branches:** 2  
**Branch Type:** Parallel execution

**Branches:**

#### Branch 1 (shape20.dragpoint1)
**Identifier:** "1"  
**Target:** shape19 (Document Property: ErrorMsg)  
**Flow:** shape19 → shape21 (Process Call: Email Subprocess) → END  
**Purpose:** Send error notification email  
**Data Processed:** 
- Sets `DPP_ErrorMessage` from `meta.base.catcherrorsmessage`
- Calls email subprocess with error details
- Does NOT return response (notification only)

#### Branch 2 (shape20.dragpoint2)
**Identifier:** "2"  
**Target:** shape41 (Map: Leave Error Map)  
**Flow:** shape41 → shape43 (Return: Error Response) → END  
**Purpose:** Return error response to caller  
**Data Processed:**
- Maps `DPP_ErrorMessage` to standardized error response
- Returns JSON error response

**Business Logic:** When an exception occurs, the process performs two parallel actions:
1. Send email notification to support team
2. Return error response to the caller

**Parallelization:** Both branches execute simultaneously. The process does not wait for email to complete before returning the error response.

**System Layer Implication:** This branching logic is orchestration (do two things in parallel). In System Layer:
- Email notification should NOT be in System Layer (cross-cutting concern)
- Error response should be returned via exception handling
- Parallel execution is Process Layer responsibility

### Self-Check Questions

**Q1: Have I identified ALL branch shapes in the main process and subprocesses?**  
**A:** YES - Identified 1 branch shape:
- shape20 (Error Handling Branch) in main process
- No branch shapes in subprocess (subprocess has decision shapes but no branch shapes)

**Q2: For EACH branch, have I documented the number of branches and their targets?**  
**A:** YES - Documented:
- shape20: 2 branches
  - Branch 1 → shape19 (Email notification path)
  - Branch 2 → shape41 (Error response path)

**Q3: Have I identified which branches represent parallel execution vs. sequential alternatives?**  
**A:** YES - Analysis:
- shape20: Parallel execution (both branches execute simultaneously)
- No sequential alternative branches (those would be decision shapes, not branch shapes)

**Q4: Have I determined the purpose of each branch path?**  
**A:** YES - Purposes:
- Branch 1: Error notification (email to support team)
- Branch 2: Error response (return to caller)

**Q5: Have I identified which branching logic belongs in System Layer vs. Process Layer?**  
**A:** YES - Analysis:
- shape20 branching: Process Layer responsibility
  - Reason: Orchestrates multiple actions (email + response)
  - System Layer should only throw exception with error details
  - Process Layer decides whether to send notifications, log, retry, etc.

---

## Step 9: Execution Order (BLOCKING)

### Main Process Execution Sequence

#### Initialization Phase (Always Executes)
1. **shape1** (Start: HTTP Trigger) - Receives leave creation request
2. **shape38** (Document Properties: Input_details) - Captures process metadata and input

#### Try Block (Normal Execution Path)
3. **shape17** (Try/Catch: Start Try block)
4. **shape29** (Map: Leave Create Map) - Transform D365 input to Oracle Fusion format
5. **shape8** (Document Property: set URL) - Set dynamic URL for API call
6. **shape49** (Notify: Log payload) - Log request payload
7. **shape33** (Connector: HTTP POST to Oracle Fusion) - Call Oracle Fusion HCM API
8. **shape2** (Decision: HTTP Status 20 check) - Evaluate HTTP response status

#### Success Path (Conditional: HTTP 20x)
9. **shape34** (Map: Oracle Fusion Leave Response Map) - Extract personAbsenceEntryId
10. **shape35** (Return: Success Response) - Return success JSON
11. **END**

#### Error Path 1 (Conditional: HTTP Non-20x, GZIP)
9. **shape44** (Decision: Check Response Content Type) - Check if GZIP encoded
10. **shape45** (Data Process: GZIP Decompress) - Decompress response
11. **shape46** (Document Property: error msg) - Set error message from decompressed content
12. **shape47** (Map: Leave Error Map) - Create error response
13. **shape48** (Return: Error Response) - Return error JSON
14. **END**

#### Error Path 2 (Conditional: HTTP Non-20x, Plain)
9. **shape44** (Decision: Check Response Content Type) - Check if GZIP encoded
10. **shape39** (Document Property: error msg) - Set error message from status
11. **shape40** (Map: Leave Error Map) - Create error response
12. **shape36** (Return: Error Response) - Return error JSON
13. **END**

#### Exception Path (Conditional: Any Exception in Try Block)
3. **shape17** (Try/Catch: Catch block triggered)
4. **shape20** (Branch: Split into 2 parallel branches)

**Branch 1 (Parallel):**
5. **shape19** (Document Property: ErrorMsg) - Set error message from exception
6. **shape21** (Process Call: Email Subprocess) - Send error notification
7. **END**

**Branch 2 (Parallel):**
5. **shape41** (Map: Leave Error Map) - Create error response
6. **shape43** (Return: Error Response) - Return error JSON
7. **END**

### Execution Order Summary

**Guaranteed Execution (All Paths):**
1. shape1 (Start)
2. shape38 (Input_details)
3. shape17 (Try/Catch entry)

**Conditional Execution:**
- **Success Path:** shape29 → shape8 → shape49 → shape33 → shape2[TRUE] → shape34 → shape35
- **Error Path (GZIP):** shape29 → shape8 → shape49 → shape33 → shape2[FALSE] → shape44[TRUE] → shape45 → shape46 → shape47 → shape48
- **Error Path (Plain):** shape29 → shape8 → shape49 → shape33 → shape2[FALSE] → shape44[FALSE] → shape39 → shape40 → shape36
- **Exception Path:** shape17[CATCH] → shape20 → [shape19 → shape21] + [shape41 → shape43]

**Parallel Execution Points:**
- shape20 branches execute simultaneously (email notification + error response)

### Self-Check Questions

**Q1: Have I documented the execution order for ALL possible paths through the process?**  
**A:** YES - Documented 4 complete paths:
- Success path (HTTP 20x)
- Error path with GZIP encoding
- Error path without GZIP encoding
- Exception path with parallel branches

**Q2: Have I identified which shapes ALWAYS execute vs. which are conditional?**  
**A:** YES - Identified:
- **Always:** shape1, shape38, shape17 (entry)
- **Conditional:** All other shapes depend on success/error/exception conditions

**Q3: Have I identified any parallel execution points?**  
**A:** YES - Identified 1 parallel execution:
- shape20 branches (email notification + error response execute simultaneously)

**Q4: Can I trace the data flow through each execution path?**  
**A:** YES - Each path traced with data transformations:
- Success: Input → Map → HTTP → Response → Map → Success JSON
- Error (GZIP): Input → Map → HTTP → Decompress → Error JSON
- Error (Plain): Input → Map → HTTP → Error JSON
- Exception: Input → Exception → Email + Error JSON (parallel)

**Q5: Have I identified any loops or recursive calls?**  
**A:** YES - Verified NO LOOPS exist:
- All paths are linear or branching
- No shape points back to a previous shape
- No recursive subprocess calls

---

## Step 10: Sequence Diagram (BLOCKING)

### Actors and Systems
- **Caller (D365):** External system initiating leave creation
- **Boomi Process:** HCM_Leave Create process (orchestration)
- **Oracle Fusion HCM:** System of Record for leave/absence data
- **Email System:** Office 365 for error notifications

### Sequence Diagram (Success Path)

```
Caller(D365)    Boomi Process           Oracle Fusion HCM
    |                |                         |
    |--POST /leave-->|                         |
    |   (JSON)       |                         |
    |                |                         |
    |                |--[shape38: Capture]     |
    |                |   Input metadata        |
    |                |                         |
    |                |--[shape29: Map]         |
    |                |   D365→Oracle format    |
    |                |                         |
    |                |--[shape8: Set URL]      |
    |                |   Resource_Path         |
    |                |                         |
    |                |--POST /absences-------->|
    |                |   (Mapped JSON)         |
    |                |                         |
    |                |<--HTTP 201 Created------|
    |                |   {personAbsenceEntryId}|
    |                |                         |
    |                |--[shape2: Check 20x]    |
    |                |   Status=201 ✓          |
    |                |                         |
    |                |--[shape34: Map]         |
    |                |   Extract entryId       |
    |                |                         |
    |<--HTTP 200-----|                         |
    |   Success JSON |                         |
    |   {status,id}  |                         |
```

### Sequence Diagram (Error Path - GZIP)

```
Caller(D365)    Boomi Process           Oracle Fusion HCM    Email System
    |                |                         |                   |
    |--POST /leave-->|                         |                   |
    |                |                         |                   |
    |                |--[shape38: Capture]     |                   |
    |                |--[shape29: Map]         |                   |
    |                |--[shape8: Set URL]      |                   |
    |                |                         |                   |
    |                |--POST /absences-------->|                   |
    |                |                         |                   |
    |                |<--HTTP 400 Bad Request--|                   |
    |                |   Content-Encoding:gzip |                   |
    |                |   (GZIP error body)     |                   |
    |                |                         |                   |
    |                |--[shape2: Check 20x]    |                   |
    |                |   Status=400 ✗          |                   |
    |                |                         |                   |
    |                |--[shape44: Check GZIP]  |                   |
    |                |   Encoding=gzip ✓       |                   |
    |                |                         |                   |
    |                |--[shape45: Decompress]  |                   |
    |                |   GZIP→Plain text       |                   |
    |                |                         |                   |
    |                |--[shape46: Set Error]   |                   |
    |                |   DPP_ErrorMessage      |                   |
    |                |                         |                   |
    |                |--[shape47: Map Error]   |                   |
    |                |                         |                   |
    |<--HTTP 200-----|                         |                   |
    |   Error JSON   |                         |                   |
    |   {status:fail}|                         |                   |
```

### Sequence Diagram (Exception Path)

```
Caller(D365)    Boomi Process           Oracle Fusion HCM    Email System
    |                |                         |                   |
    |--POST /leave-->|                         |                   |
    |                |                         |                   |
    |                |--[shape38: Capture]     |                   |
    |                |--[shape29: Map]         |                   |
    |                |--[shape8: Set URL]      |                   |
    |                |                         |                   |
    |                |--POST /absences-------->|                   |
    |                |                         X (Timeout/Error)   |
    |                |                         |                   |
    |                |--[shape17: CATCH]       |                   |
    |                |   Exception caught      |                   |
    |                |                         |                   |
    |                |--[shape20: BRANCH]      |                   |
    |                |   Split 2 ways          |                   |
    |                |                         |                   |
    |                |--[Branch 1]-------------|------------------>|
    |                |   shape19: Set Error    |                   |
    |                |   shape21: Email Sub    |--Send Email----->|
    |                |                         |                   |
    |                |--[Branch 2]             |                   |
    |                |   shape41: Map Error    |                   |
    |                |                         |                   |
    |<--HTTP 200-----|                         |                   |
    |   Error JSON   |                         |                   |
    |   {status:fail}|                         |                   |
```

### Key Observations from Sequence Diagrams

1. **Synchronous HTTP Call:** The process makes a synchronous POST request to Oracle Fusion HCM and waits for response

2. **Error Handling Strategy:**
   - HTTP errors (non-20x) → Return error response to caller
   - GZIP errors → Decompress before processing
   - Exceptions → Return error response + Send email notification (parallel)

3. **Response Transformation:**
   - Success: Oracle response → Standardized success JSON
   - Error: Error details → Standardized error JSON

4. **Email Notification:** Only triggered on exceptions (not on HTTP errors)

5. **Parallel Processing:** Exception handling branches into email notification and error response simultaneously

### Self-Check Questions

**Q1: Does my sequence diagram reference the Data Dependency Graph (Step 4)?**  
**A:** YES - The sequence diagram shows data transformations that match the dependency graph:
- Input → Map → HTTP Call → Response → Map → Output (matches dependency graph nodes)

**Q2: Does my sequence diagram reference the Control Flow Graph (Step 5)?**  
**A:** YES - The sequence follows the control flow paths:
- Success path: shape1 → shape38 → shape29 → shape8 → shape33 → shape2 → shape34 → shape35
- Error paths: Match the control flow branches for GZIP and plain errors
- Exception path: Match the control flow with parallel branches

**Q3: Does my sequence diagram reference the Decision Shape Analysis (Step 7)?**  
**A:** YES - The sequence shows decision points:
- shape2 (HTTP Status 20 check): Shown as "Status=201 ✓" or "Status=400 ✗"
- shape44 (GZIP check): Shown as "Encoding=gzip ✓"

**Q4: Does my sequence diagram reference the Branch Shape Analysis (Step 8)?**  
**A:** YES - The exception path shows:
- shape20 branching into 2 parallel paths (email + error response)

**Q5: Does my sequence diagram reference the Execution Order (Step 9)?**  
**A:** YES - The sequence follows the documented execution order:
- Initialization: shape1 → shape38
- Try block: shape29 → shape8 → shape33 → shape2
- Conditional paths: Success, Error (GZIP), Error (Plain), Exception

**Q6: Have I created separate sequence diagrams for different execution paths?**  
**A:** YES - Created 3 sequence diagrams:
- Success path (HTTP 20x)
- Error path with GZIP encoding
- Exception path with parallel branches

**Q7: Do my sequence diagrams show all actors and systems involved?**  
**A:** YES - All actors identified:
- Caller (D365): Initiates request
- Boomi Process: Orchestrates flow
- Oracle Fusion HCM: System of Record
- Email System: Error notifications

---

## Function Exposure Decision Table (BLOCKING)

### Decision Process for Azure Functions

Based on the Boomi process analysis, I need to determine which operations should be exposed as Azure Functions vs. internal Atomic Handlers.

| Operation | Independent Invoke? | Decision Before/After? | Same SOR? | Internal Lookup? | Conclusion | Reasoning |
|-----------|-------------------|----------------------|-----------|-----------------|------------|-----------|
| **Create Leave in Oracle Fusion** | YES | NO | N/A (single operation) | NO | **Azure Function** | Process Layer needs to invoke this independently to create leave records. This is a complete business operation. |
| **Email Notification** | NO | YES (after exception) | NO (different system) | NO | **NOT System Layer** | Email is a cross-cutting concern triggered by exception. This belongs to Process Layer or separate notification service. NOT part of System Layer. |
| **GZIP Decompression** | NO | YES (after HTTP error check) | YES (same response) | YES | **Internal Helper** | Technical concern - decompressing response. Should be handled transparently in Atomic Handler, not exposed as separate function. |
| **Map D365 to Oracle Format** | NO | YES (before HTTP call) | YES | YES | **Internal Transformation** | Data transformation for Oracle Fusion API. Part of the Atomic Handler's request preparation. |
| **Extract personAbsenceEntryId** | NO | YES (after HTTP success) | YES | YES | **Internal Transformation** | Response parsing. Part of the Atomic Handler's response handling. |

### Verification Questions

**Q1: Identified ALL decision points?**  
**A:** YES - Identified:
- HTTP status check (shape2)
- GZIP encoding check (shape44)
- Exception handling (shape17)
- Email attachment check (shape4 in subprocess)

**Q2: WHERE each decision belongs?**  
**A:** YES - Determined:
- **System Layer Handler:** HTTP status check (throw exception for non-2xx)
- **System Layer Atomic Handler:** GZIP decompression (transparent handling)
- **Process Layer:** Email notification decision
- **Process Layer:** Exception handling orchestration

**Q3: "if X exists, skip Y" checked?**  
**A:** YES - No such patterns exist in this process. All operations are sequential or error-handling branches.

**Q4: "if flag=X, do Y" checked?**  
**A:** YES - Identified:
- shape4 (Email attachment flag): Process Layer concern, NOT System Layer

**Q5: Can explain WHY each operation type?**  
**A:** YES - Reasoning:
- **Create Leave (Azure Function):** Complete business operation that Process Layer needs to invoke independently
- **Email Notification (NOT System Layer):** Cross-cutting concern, orchestration logic
- **GZIP Decompression (Internal Helper):** Technical concern, transparent to caller
- **Data Mapping (Internal):** Request/response transformation, part of Atomic Handler

**Q6: Avoided pattern-matching?**  
**A:** YES - Analyzed based on:
- Can Process Layer invoke independently? (YES → Function)
- Is it a complete business operation? (YES → Function)
- Is it internal data transformation? (YES → Internal)
- Is it cross-system orchestration? (YES → Process Layer)

**Q7: If 1 Function, NO decision shapes?**  
**A:** YES - Verified:
- Creating 1 Azure Function (CreateLeave)
- Decision shapes (shape2, shape44) are technical error handling, NOT business decisions
- Business decisions (if any) would belong to Process Layer

### Function Exposure Summary

**I will create 1 Azure Function for Oracle Fusion HCM:**
- **CreateLeaveAbsence** - Creates a leave/absence record in Oracle Fusion HCM

**Because:**
- This is a complete business operation that Process Layer needs to invoke independently
- It represents a single atomic operation against the Oracle Fusion HCM SOR
- Per Rule 1066, business decisions (like "if employee exists, check status, then create") would belong to Process Layer
- This process has NO business decisions - only technical error handling

**Functions:**
- **CreateLeaveAbsence:** Accepts leave details (employee, dates, type, status), calls Oracle Fusion HCM API, returns personAbsenceEntryId or error

**Internal (Atomic Handlers, NOT Functions):**
- GZIP decompression (transparent in Atomic Handler)
- Request mapping (D365 format → Oracle format)
- Response parsing (extract personAbsenceEntryId)

**NOT System Layer:**
- Email notification (Process Layer or notification service)
- Exception orchestration (Process Layer)

**Authentication:**
- Basic Auth (credentials stored in configuration)
- Username: INTEGRATION.USER@al-ghurair.com
- Password: Stored in Azure Key Vault (referenced in appsettings)

---

## Summary and Conclusions

### Process Summary
The HCM Leave Create process is a **System Layer integration** that creates leave/absence records in Oracle Fusion HCM. It receives leave data from D365 (or other callers), transforms it to Oracle Fusion format, makes an HTTP POST call to the Oracle Fusion HCM API, and returns a standardized response.

### Key Findings

1. **Single SOR Operation:** This process interacts with ONE System of Record (Oracle Fusion HCM)

2. **No Business Logic:** The process contains NO business decisions - only technical error handling (HTTP status checks, GZIP decompression)

3. **Synchronous HTTP Call:** Makes a single synchronous POST request to Oracle Fusion HCM API endpoint `/hcmRestApi/resources/11.13.18.05/absences`

4. **Error Handling Strategy:**
   - HTTP 20x → Success response with personAbsenceEntryId
   - HTTP Non-20x → Error response (with GZIP decompression if needed)
   - Exception → Error response + Email notification (parallel)

5. **Email Notification:** Only triggered on exceptions (NOT on HTTP errors). This is a cross-cutting concern that should NOT be in System Layer.

### System Layer Implementation Plan

**Azure Function to Create:**
1. **CreateLeaveAbsence** (HTTP-triggered Function)
   - **Route:** POST /api/hcm/leaves
   - **Input:** LeaveCreateRequestDTO (from D365 format)
   - **Output:** LeaveCreateResponseDTO (standardized response)
   - **Atomic Handler:** CreateLeaveAbsenceAtomicHandler (calls Oracle Fusion HCM API)

**Architecture Layers:**
- **Function:** CreateLeaveAbsenceFunction (HTTP entry point)
- **Service:** ILeaveManagement (interface), LeaveManagementService (implementation)
- **Handler:** CreateLeaveAbsenceHandler (orchestrates Atomic Handler)
- **Atomic Handler:** CreateLeaveAbsenceAtomicHandler (single HTTP POST to Oracle Fusion)

**Configuration:**
- Oracle Fusion HCM base URL: https://iaaxey-dev3.fa.ocs.oraclecloud.com:443
- API endpoint: /hcmRestApi/resources/11.13.18.05/absences
- Authentication: Basic Auth (username/password from Key Vault)

**Error Handling:**
- HTTP 2xx → Return success response
- HTTP 4xx/5xx → Throw HttpClientException with status code and message
- Timeout/Network errors → Throw HttpClientException
- GZIP responses → Decompress transparently in Atomic Handler

**NOT Implementing in System Layer:**
- Email notification (Process Layer responsibility)
- Exception orchestration (Process Layer responsibility)
- Parallel branching (Process Layer responsibility)

### Phase 1 Completion Checklist

- [x] Operations Inventory (4 operations identified)
- [x] Step 1a: Input Structure Analysis (D365 Leave JSON)
- [x] Step 1b: Response Structure Analysis (Oracle Fusion response + standardized response)
- [x] Step 1c: Operation Response Analysis (HTTP POST with error handling)
- [x] Step 1d: Map Analysis (3 maps: request, success response, error response)
- [x] Step 1e: HTTP Status Codes and Return Paths (3 paths: success, error GZIP, error plain, exception)
- [x] Steps 2-3: Process Properties Analysis (10 dynamic properties, 3 defined parameters, 2 connections)
- [x] Step 4: Data Dependency Graph (4 paths with field-level dependencies)
- [x] Step 5: Control Flow Graph (4 execution paths, no loops)
- [x] Step 7: Decision Shape Analysis (3 decisions: HTTP status, GZIP check, email attachment)
- [x] Step 8: Branch Shape Analysis (1 branch: parallel error handling)
- [x] Step 9: Execution Order (4 complete paths documented)
- [x] Step 10: Sequence Diagram (3 diagrams: success, error GZIP, exception)
- [x] Function Exposure Decision Table (1 Azure Function, reasoning provided)

### Self-Check: All Questions Answered YES

**Step 1a-1e:** ✓ All input/output structures documented  
**Steps 2-3:** ✓ All property reads/writes identified  
**Step 4:** ✓ Data dependency graph complete  
**Step 5:** ✓ Control flow graph complete, no loops  
**Step 7:** ✓ All decision shapes analyzed, layer assignments justified  
**Step 8:** ✓ All branch shapes analyzed, parallelization identified  
**Step 9:** ✓ Execution order documented for all paths  
**Step 10:** ✓ Sequence diagrams reference all prior steps  
**Function Exposure:** ✓ Decision table complete, 1 Function justified  

---

## Ready for Phase 2: Code Generation

This Phase 1 extraction document is now complete and ready to be committed. Phase 2 code generation can proceed based on this analysis.

**Next Steps:**
1. Commit this BOOMI_EXTRACTION_PHASE1.md document
2. Proceed to Phase 2: Generate System Layer code for Oracle Fusion HCM
3. Create 1 Azure Function: CreateLeaveAbsence
4. Implement layered architecture: Function → Service → Handler → Atomic Handler
5. Configure authentication, error handling, and HTTP client
6. Proceed to Phase 3: Compliance audit
7. Proceed to Phase 4: Build validation
