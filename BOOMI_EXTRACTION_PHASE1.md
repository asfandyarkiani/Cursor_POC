# BOOMI EXTRACTION PHASE 1 - HCM Leave Create Process

**Process Name:** HCM_Leave Create  
**Process ID:** ca69f858-785f-4565-ba1f-b4edc6cca05b  
**Process Type:** Web Service Server (Listen)  
**Business Domain:** Human Resources / Leave Management  
**Purpose:** Sync leave data between D365 and Oracle HCM Fusion  
**Version:** 29  
**Last Modified:** 2024-11-04T08:54:39Z

---

## TABLE OF CONTENTS
1. Operations Inventory
2. Input Structure Analysis (Step 1a) - BLOCKING
3. Response Structure Analysis (Step 1b) - BLOCKING
4. Operation Response Analysis (Step 1c) - BLOCKING
5. Map Analysis (Step 1d) - BLOCKING
6. HTTP Status Codes and Return Paths (Step 1e) - BLOCKING
7. Process Properties Analysis (Steps 2-3)
8. Data Dependency Graph (Step 4) - BLOCKING
9. Control Flow Graph (Step 5)
10. Decision Shape Analysis (Step 7) - BLOCKING
11. Branch Shape Analysis (Step 8) - BLOCKING
12. Execution Order (Step 9) - BLOCKING
13. Sequence Diagram (Step 10) - BLOCKING
14. Subprocess Analysis (Step 7a)
15. Critical Patterns Identified
16. System Layer Identification
17. Function Exposure Decision Table - BLOCKING
18. Validation Checklist

---

## 1. OPERATIONS INVENTORY

### Main Process Operations

| Operation ID | Operation Name | Type | SubType | Purpose |
|---|---|---|---|---|
| 8f709c2b-e63f-4d5f-9374-2932ed70415d | Create Leave Oracle Fusion OP | connector-action | wss | Entry point - Web Service Server Listen operation |
| 6e8920fd-af5a-430b-a1d9-9fde7ac29a12 | Leave Oracle Fusion Create | connector-action | http | HTTP POST to Oracle Fusion HCM |
| af07502a-fafd-4976-a691-45d51a33b549 | Email w Attachment | connector-action | mail | Send email with attachment |
| 15a72a21-9b57-49a1-a8ed-d70367146644 | Email W/O Attachment | connector-action | mail | Send email without attachment |

### Connection Details

| Connection ID | Connection Name | Type | URL/Host |
|---|---|---|---|
| aa1fcb29-d146-4425-9ea6-b9698090f60e | Oracle Fusion | http | https://iaaxey-dev3.fa.ocs.oraclecloud.com:443 |
| 00eae79b-2303-4215-8067-dcc299e42697 | Office 365 Email | mail | smtp-mail.outlook.com:587 |

---

## 2. INPUT STRUCTURE ANALYSIS (Step 1a) - BLOCKING

### ✅ Self-Check Results
- ✅ Request profile identified from entry operation: **YES**
- ✅ Profile structure analyzed (JSON or XML): **YES** (JSON)
- ✅ Array vs single object detected: **YES** (Single Object)
- ✅ Array cardinality documented (minOccurs, maxOccurs): **YES** (N/A - Single Object)
- ✅ ALL fields extracted (including nested): **YES**
- ✅ Field paths documented (full Boomi paths): **YES**
- ✅ Field mapping table generated (Boomi → Azure DTO): **YES**
- ✅ Document processing behavior determined: **YES**
- ✅ Input structure documented in Phase 1 document: **YES**

### Request Profile Structure
- **Profile ID:** febfa3e1-f719-4ee8-ba57-cdae34137ab3
- **Profile Name:** D365 Leave Create JSON Profile
- **Profile Type:** profile.json
- **Root Structure:** Root/Object
- **Array Detection:** ❌ NO - Single Object
- **Input Type:** singlejson

### Entry Point Operation Details
- **Operation ID:** 8f709c2b-e63f-4d5f-9374-2932ed70415d
- **Operation Name:** Create Leave Oracle Fusion OP
- **Operation Type:** connector-action (wss - Web Service Server)
- **Request Profile:** febfa3e1-f719-4ee8-ba57-cdae34137ab3
- **Input Type:** singlejson
- **Output Type:** singlejson
- **Object Name:** leave

### Input Format (JSON Structure)

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

### Document Processing Behavior
- **Boomi Processing:** Single document processing (inputType: singlejson)
- **Azure Function Requirement:** Must accept single leave request object
- **Implementation Pattern:** Process single leave request, validate fields, call Oracle Fusion HCM REST API
- **Execution Pattern:** single_execution
- **Session Management:** one_session_per_execution

### Field Count
- **Total Fields:** 9 fields (all top-level, no nested structures)

### Complete Field Mapping Table (Boomi → Azure DTO)

| Boomi Field Path | Boomi Field Name | Data Type | Required | Azure DTO Property | Usage in Process |
|---|---|---|---|---|---|
| Root/Object/employeeNumber | employeeNumber | number | Yes | EmployeeNumber | Tracked field, mapped to personNumber for Oracle Fusion |
| Root/Object/absenceType | absenceType | character | Yes | AbsenceType | Mapped directly to Oracle Fusion request |
| Root/Object/employer | employer | character | Yes | Employer | Mapped directly to Oracle Fusion request |
| Root/Object/startDate | startDate | character | Yes | StartDate | Mapped directly to Oracle Fusion request |
| Root/Object/endDate | endDate | character | Yes | EndDate | Mapped directly to Oracle Fusion request |
| Root/Object/absenceStatusCode | absenceStatusCode | character | Yes | AbsenceStatusCode | Mapped to absenceStatusCd for Oracle Fusion |
| Root/Object/approvalStatusCode | approvalStatusCode | character | Yes | ApprovalStatusCode | Mapped to approvalStatusCd for Oracle Fusion |
| Root/Object/startDateDuration | startDateDuration | number | Yes | StartDateDuration | Mapped directly to Oracle Fusion request |
| Root/Object/endDateDuration | endDateDuration | number | Yes | EndDateDuration | Mapped directly to Oracle Fusion request |

---

## 3. RESPONSE STRUCTURE ANALYSIS (Step 1b) - BLOCKING

### ✅ Self-Check Results
- ✅ Response profile identified from entry operation: **YES**
- ✅ Response structure analyzed: **YES** (JSON)
- ✅ Response field mapping table generated: **YES**

### Response Profile Structure
- **Profile ID:** f4ca3a70-114a-4601-bad8-44a3eb20e2c0
- **Profile Name:** Leave D365 Response
- **Profile Type:** profile.json
- **Root Structure:** leaveResponse/Object

### Response Format (JSON Structure)

```json
{
  "status": "success",
  "message": "Data successfully sent to Oracle Fusion",
  "personAbsenceEntryId": 300000123456789,
  "success": "true"
}
```

### Response Field Mapping Table (Boomi → Azure DTO)

| Boomi Field Path | Boomi Field Name | Data Type | Azure DTO Property | Populated By |
|---|---|---|---|---|
| leaveResponse/Object/status | status | character | Status | Map defaults (success/failure) |
| leaveResponse/Object/message | message | character | Message | Map defaults or error message |
| leaveResponse/Object/personAbsenceEntryId | personAbsenceEntryId | number | PersonAbsenceEntryId | Oracle Fusion response (success path) |
| leaveResponse/Object/success | success | character | Success | Map defaults (true/false) |

---

## 4. OPERATION RESPONSE ANALYSIS (Step 1c) - BLOCKING

### ✅ Self-Check Results
- ✅ Operation Response Inventory completed: **YES**
- ✅ Response structures analyzed for all operations: **YES**
- ✅ Extracted fields documented: **YES**
- ✅ Data consumers identified: **YES**
- ✅ Business logic implications documented: **YES**

### Operation: Leave Oracle Fusion Create (6e8920fd-af5a-430b-a1d9-9fde7ac29a12)

**Response Profile:** NONE (responseProfileType: NONE)  
**Response Type:** Raw JSON response from Oracle Fusion HCM REST API  
**Expected Response:** Oracle Fusion Leave Response (profile: 316175c7-0e45-4869-9ac6-5f9d69882a62)

**Extracted Fields (via Track Properties):**
- **meta.base.applicationstatuscode** - HTTP status code from Oracle Fusion response
- **dynamicdocument.DDP_RespHeader** - Content-Encoding header (for gzip detection)

**Response Header Mapping:**
- Content-Encoding → dynamicdocument.DDP_RespHeader

**Business Logic Implications:**
1. Decision shape2 checks HTTP status code (meta.base.applicationstatuscode) to determine success/failure
2. Decision shape44 checks Content-Encoding header to determine if response is gzipped
3. If status is 20* (success), process continues to map success response
4. If status is NOT 20* (error), process handles error response

**Data Consumers:**
- **Decision shape2** - Reads meta.base.applicationstatuscode (HTTP Status 20* check)
- **Decision shape44** - Reads dynamicdocument.DDP_RespHeader (Content-Encoding check)
- **Map shape34** - Maps Oracle Fusion response to D365 response format (success path)
- **Map shape40/shape47** - Maps error message to D365 response format (error path)

**Actual Execution Order:**
```
Operation (Leave Oracle Fusion Create) 
  → Extracts HTTP status code (meta.base.applicationstatuscode)
  → Extracts Content-Encoding header (dynamicdocument.DDP_RespHeader)
  → Decision checks status code
  → Routes to success or error path based on status
```

### Operation: Email w Attachment (af07502a-fafd-4976-a691-45d51a33b549)

**Response Profile:** NONE  
**Purpose:** Send email notification (error notification with attachment)  
**Extracted Fields:** None  
**Data Consumers:** None (terminal operation in subprocess)

### Operation: Email W/O Attachment (15a72a21-9b57-49a1-a8ed-d70367146644)

**Response Profile:** NONE  
**Purpose:** Send email notification (error notification without attachment)  
**Extracted Fields:** None  
**Data Consumers:** None (terminal operation in subprocess)

---

## 5. MAP ANALYSIS (Step 1d) - BLOCKING

### ✅ Self-Check Results
- ✅ All map files analyzed: **YES**
- ✅ SOAP request maps identified: **YES** (N/A - HTTP/REST operations only)
- ✅ Field mappings extracted: **YES**
- ✅ Scripting functions analyzed: **YES**

### Map Inventory

| Map ID | Map Name | From Profile | To Profile | Type | Purpose |
|---|---|---|---|---|---|
| c426b4d6-2aff-450e-b43b-59956c4dbc96 | Leave Create Map | febfa3e1-f719-4ee8-ba57-cdae34137ab3 | a94fa205-c740-40a5-9fda-3d018611135a | Data Transform | Transform D365 request to Oracle Fusion request |
| e4fd3f59-edb5-43a1-aeae-143b600a064e | Oracle Fusion Leave Response Map | 316175c7-0e45-4869-9ac6-5f9d69882a62 | f4ca3a70-114a-4601-bad8-44a3eb20e2c0 | Response Transform | Transform Oracle Fusion response to D365 response (success) |
| f46b845a-7d75-41b5-b0ad-c41a6a8e9b12 | Leave Error Map | 23d7a2e9-5cb0-4e9c-9e4b-3154834bad0d | f4ca3a70-114a-4601-bad8-44a3eb20e2c0 | Error Transform | Transform error message to D365 response (failure) |

### Map 1: Leave Create Map (c426b4d6-2aff-450e-b43b-59956c4dbc96)

**Type:** Data Transformation (JSON to JSON)  
**From Profile:** febfa3e1-f719-4ee8-ba57-cdae34137ab3 (D365 Leave Create JSON Profile)  
**To Profile:** a94fa205-c740-40a5-9fda-3d018611135a (HCM Leave Create JSON Profile)  
**Purpose:** Transform D365 request format to Oracle Fusion HCM REST API request format

**Field Mappings:**

| Source Field (D365) | Target Field (Oracle Fusion) | Mapping Type | Transformation |
|---|---|---|---|
| employeeNumber | personNumber | Direct | Field renamed |
| absenceType | absenceType | Direct | No change |
| employer | employer | Direct | No change |
| startDate | startDate | Direct | No change |
| endDate | endDate | Direct | No change |
| absenceStatusCode | absenceStatusCd | Direct | Field renamed (Code → Cd) |
| approvalStatusCode | approvalStatusCd | Direct | Field renamed (Code → Cd) |
| startDateDuration | startDateDuration | Direct | No change |
| endDateDuration | endDateDuration | Direct | No change |

**Profile vs Map Field Name Comparison:**

| D365 Field Name | Oracle Fusion Profile Field Name | Notes |
|---|---|---|
| employeeNumber | personNumber | ✅ Renamed for Oracle Fusion API |
| absenceStatusCode | absenceStatusCd | ✅ Shortened field name |
| approvalStatusCode | approvalStatusCd | ✅ Shortened field name |

**Scripting Functions:** None

**Static Values:** None

**CRITICAL NOTES:**
- This is a straightforward field-to-field mapping
- No complex transformations or scripting required
- Field name changes are minimal (employeeNumber → personNumber, Code → Cd suffix)

### Map 2: Oracle Fusion Leave Response Map (e4fd3f59-edb5-43a1-aeae-143b600a064e)

**Type:** Response Transformation (JSON to JSON)  
**From Profile:** 316175c7-0e45-4869-9ac6-5f9d69882a62 (Oracle Fusion Leave Response JSON Profile)  
**To Profile:** f4ca3a70-114a-4601-bad8-44a3eb20e2c0 (Leave D365 Response)  
**Purpose:** Transform Oracle Fusion success response to D365 response format

**Field Mappings:**

| Source Field (Oracle Fusion) | Target Field (D365) | Mapping Type | Transformation |
|---|---|---|---|
| personAbsenceEntryId | personAbsenceEntryId | Direct | Extracted from Oracle Fusion response |

**Default Values (Static):**

| Target Field | Static Value | Purpose |
|---|---|---|
| status | "success" | Indicates successful operation |
| message | "Data successfully sent to Oracle Fusion" | Success message |
| success | "true" | Boolean flag as string |

**CRITICAL NOTES:**
- Only extracts personAbsenceEntryId from Oracle Fusion response
- All other response fields are populated with static success values
- This map is used only when Oracle Fusion returns HTTP 20* status

### Map 3: Leave Error Map (f46b845a-7d75-41b5-b0ad-c41a6a8e9b12)

**Type:** Error Response Transformation (Flat File to JSON)  
**From Profile:** 23d7a2e9-5cb0-4e9c-9e4b-3154834bad0d (Dummy FF Profile)  
**To Profile:** f4ca3a70-114a-4601-bad8-44a3eb20e2c0 (Leave D365 Response)  
**Purpose:** Transform error message to D365 error response format

**Field Mappings:**

| Source | Target Field (D365) | Mapping Type | Transformation |
|---|---|---|---|
| process.DPP_ErrorMessage | message | Function (PropertyGet) | Get error message from process property |

**Scripting Functions:**

| Function ID | Function Type | Input | Output | Logic |
|---|---|---|---|---|
| 1 | PropertyGet | Property Name: "DPP_ErrorMessage" | Result (error message) | Retrieves error message from process property |

**Default Values (Static):**

| Target Field | Static Value | Purpose |
|---|---|---|
| status | "failure" | Indicates failed operation |
| success | "false" | Boolean flag as string |

**CRITICAL NOTES:**
- This map is used for ALL error scenarios (HTTP errors, try/catch errors, validation errors)
- Error message is dynamically populated from process.DPP_ErrorMessage property
- process.DPP_ErrorMessage is populated by:
  - shape19 (from meta.base.catcherrorsmessage) - Try/Catch errors
  - shape39 (from meta.base.applicationstatusmessage) - HTTP error responses
  - shape46 (from current document - gzip decompression errors)

**VALIDATION:**
- ✅ All maps analyzed and documented
- ✅ Field mappings extracted from JSON
- ✅ Scripting functions identified
- ✅ Static values documented
- ✅ No SOAP operations (all HTTP/REST) - No namespace prefix analysis required

---

## 6. HTTP STATUS CODES AND RETURN PATHS (Step 1e) - BLOCKING

### ✅ Self-Check Results
- ✅ HTTP status codes extracted for all return paths: **YES**
- ✅ Response JSON documented for each return path: **YES**
- ✅ Populated fields documented for each return path: **YES**
- ✅ Downstream operation HTTP status codes documented: **YES**

### Return Path Analysis

#### Return Path 1: Success Response (shape35)

**Return Label:** "Success Response"  
**Return Shape ID:** shape35  
**HTTP Status Code:** 200 (Success)  
**Decision Conditions Leading to Return:**
- Decision shape2: meta.base.applicationstatuscode matches "20*" → TRUE path

**Populated Response Fields:**

| Field Name | Field Path | Source | Populated By |
|---|---|---|---|
| status | leaveResponse/Object/status | static | Map default value: "success" |
| message | leaveResponse/Object/message | static | Map default value: "Data successfully sent to Oracle Fusion" |
| personAbsenceEntryId | leaveResponse/Object/personAbsenceEntryId | operation_response | Leave Oracle Fusion Create operation response |
| success | leaveResponse/Object/success | static | Map default value: "true" |

**Response JSON Example (HTTP 200 - Success):**

```json
{
  "status": "success",
  "message": "Data successfully sent to Oracle Fusion",
  "personAbsenceEntryId": 300000123456789,
  "success": "true"
}
```

#### Return Path 2: Error Response - HTTP Non-20* Status (shape36)

**Return Label:** "Error Response"  
**Return Shape ID:** shape36  
**HTTP Status Code:** 400/500 (based on Oracle Fusion response)  
**Decision Conditions Leading to Return:**
- Decision shape2: meta.base.applicationstatuscode does NOT match "20*" → FALSE path
- Decision shape44: dynamicdocument.DDP_RespHeader does NOT equal "gzip" → FALSE path

**Populated Response Fields:**

| Field Name | Field Path | Source | Populated By |
|---|---|---|---|
| status | leaveResponse/Object/status | static | Map default value: "failure" |
| message | leaveResponse/Object/message | process_property | process.DPP_ErrorMessage (from meta.base.applicationstatusmessage) |
| success | leaveResponse/Object/success | static | Map default value: "false" |

**Response JSON Example (HTTP 400 - Oracle Fusion Error):**

```json
{
  "status": "failure",
  "message": "Bad Request - Invalid absenceType value",
  "success": "false"
}
```

#### Return Path 3: Error Response - Gzipped Response Error (shape48)

**Return Label:** "Error Response"  
**Return Shape ID:** shape48  
**HTTP Status Code:** 500 (Internal Server Error)  
**Decision Conditions Leading to Return:**
- Decision shape2: meta.base.applicationstatuscode does NOT match "20*" → FALSE path
- Decision shape44: dynamicdocument.DDP_RespHeader equals "gzip" → TRUE path
- Gzip decompression attempted (shape45 - Custom Scripting)

**Populated Response Fields:**

| Field Name | Field Path | Source | Populated By |
|---|---|---|---|
| status | leaveResponse/Object/status | static | Map default value: "failure" |
| message | leaveResponse/Object/message | process_property | process.DPP_ErrorMessage (from current document after gzip decompression) |
| success | leaveResponse/Object/success | static | Map default value: "false" |

**Response JSON Example (HTTP 500 - Gzip Error):**

```json
{
  "status": "failure",
  "message": "Error decompressing gzip response from Oracle Fusion",
  "success": "false"
}
```

#### Return Path 4: Error Response - Try/Catch Exception (shape43)

**Return Label:** "Error Response"  
**Return Shape ID:** shape43  
**HTTP Status Code:** 500 (Internal Server Error)  
**Decision Conditions Leading to Return:**
- Try/Catch error occurred (shape17 - catcherrors) → CATCH path
- shape20 (branch) → Path 2

**Populated Response Fields:**

| Field Name | Field Path | Source | Populated By |
|---|---|---|---|
| status | leaveResponse/Object/status | static | Map default value: "failure" |
| message | leaveResponse/Object/message | process_property | process.DPP_ErrorMessage (from meta.base.catcherrorsmessage) |
| success | leaveResponse/Object/success | static | Map default value: "false" |

**Response JSON Example (HTTP 500 - Process Error):**

```json
{
  "status": "failure",
  "message": "Process execution error: Invalid JSON format in request",
  "success": "false"
}
```

### Downstream Operations HTTP Status Codes

| Operation Name | Operation Type | Expected Success Codes | Error Codes | Error Handling |
|---|---|---|---|---|
| Leave Oracle Fusion Create | HTTP POST | 200, 201 | 400, 401, 404, 500 | Check HTTP status code, route to error path on non-20* |
| Email w Attachment | Mail Send | N/A | N/A | Send email notification (error notification in subprocess) |
| Email W/O Attachment | Mail Send | N/A | N/A | Send email notification (error notification in subprocess) |

**HTTP Operation Configuration:**
- **Method:** POST
- **Data Content Type:** application/json
- **Return Errors:** true
- **Return Responses:** true
- **Follow Redirects:** false

**Error Handling Strategy:**
1. Check HTTP status code (meta.base.applicationstatuscode)
2. If 20* → Success path (map response, return HTTP 200)
3. If NOT 20* → Error path:
   - Check if response is gzipped (Content-Encoding header)
   - If gzipped → Decompress → Extract error message
   - If not gzipped → Extract error message directly
   - Map error response, return HTTP 400/500

---

## 7. PROCESS PROPERTIES ANALYSIS (Steps 2-3)

### Process Properties WRITTEN (Step 2)

| Property Name | Property Type | Written By (Shape ID) | Source | Value Example |
|---|---|---|---|---|
| process.DPP_Process_Name | Dynamic | shape38 | Execution property (Process Name) | "HCM_Leave Create" |
| process.DPP_AtomName | Dynamic | shape38 | Execution property (Atom Name) | "Cloud - Development" |
| process.DPP_Payload | Dynamic | shape38 | Current document | `{"employeeNumber": 9000604, ...}` |
| process.DPP_ExecutionID | Dynamic | shape38 | Execution property (Execution Id) | "execution-12345-67890" |
| process.DPP_File_Name | Dynamic | shape38 | Concatenation (Process Name + Timestamp + ".txt") | "HCM_Leave Create_2024-11-04T10:30:00.123Z.txt" |
| process.DPP_Subject | Dynamic | shape38 | Concatenation (Atom Name + Process Name + static text) | "Cloud - Development (HCM_Leave Create) has errors to report" |
| process.To_Email | Dynamic | shape38 | Defined parameter (PP_HCM_LeaveCreate_Properties.To_Email) | "BoomiIntegrationTeam@al-ghurair.com" |
| process.DPP_HasAttachment | Dynamic | shape38 | Defined parameter (PP_HCM_LeaveCreate_Properties.DPP_HasAttachment) | "Y" |
| process.DPP_ErrorMessage | Dynamic | shape19, shape39, shape46 | Track property (error messages) | Error message text |
| dynamicdocument.URL | Dynamic Document | shape8 | Defined parameter (PP_HCM_LeaveCreate_Properties.Resource_Path) | "hcmRestApi/resources/11.13.18.05/absences" |
| dynamicdocument.DDP_RespHeader | Dynamic Document | (Response header mapping) | HTTP Response Header (Content-Encoding) | "gzip" or empty |

**Defined Process Properties (External Components):**

**Component: PP_HCM_LeaveCreate_Properties (e22c04db-7dfa-4b1b-9d88-77365b0fdb18)**
- Resource_Path: "hcmRestApi/resources/11.13.18.05/absences" (Oracle Fusion API endpoint path)
- To_Email: "BoomiIntegrationTeam@al-ghurair.com" (Error notification email)
- DPP_HasAttachment: "Y" (Email attachment flag)

**Component: PP_Office365_Email (0feff13f-8a2c-438a-b3c7-1909e2a7f533)**
- From_Email: "Boomi.Dev.failures@al-ghurair.com"
- To_Email: "Rajesh.Muppala@al-ghurair.com;mohan.jonnalagadda@al-ghurair.com"
- HasAttachment: "Y"
- EmailBody: (dynamic)
- Environment: "DEV Failure :"

### Process Properties READ (Step 3)

| Property Name | Property Type | Read By (Shape ID) | Usage |
|---|---|---|---|
| dynamicdocument.URL | Dynamic Document | Operation shape33 (HTTP operation) | Used as path parameter for Oracle Fusion API call |
| dynamicdocument.DDP_RespHeader | Dynamic Document | Decision shape44 | Check if response is gzipped |
| process.DPP_ErrorMessage | Dynamic | Map f46b845a (Function 1 - PropertyGet) | Extract error message for error response |
| process.DPP_Process_Name | Dynamic | Subprocess shape11, shape23 (message parameters) | Used in email body |
| process.DPP_AtomName | Dynamic | Subprocess shape11, shape23 (message parameters) | Used in email body |
| process.DPP_ExecutionID | Dynamic | Subprocess shape11, shape23 (message parameters) | Used in email body |
| process.DPP_Payload | Dynamic | Subprocess shape15 (message parameter) | Used as email attachment |
| process.DPP_File_Name | Dynamic | Subprocess shape6 (connector.mail.filename) | Email attachment filename |
| process.DPP_Subject | Dynamic | Subprocess shape6, shape20 (connector.mail.subject) | Email subject line |
| process.To_Email | Dynamic | Subprocess shape6, shape20 (connector.mail.toAddress) | Email recipient |
| process.DPP_MailBody | Dynamic | Subprocess shape6, shape20 (connector.mail.body) | Email body content |
| process.DPP_HasAttachment | Dynamic | Subprocess Decision shape4 | Check if email should have attachment |

**Track Properties READ:**

| Track Property | Read By (Shape ID) | Usage |
|---|---|---|
| meta.base.applicationstatuscode | Decision shape2 | Check HTTP status code (20* pattern) |
| meta.base.catcherrorsmessage | shape19 (documentproperties) | Extract try/catch error message |
| meta.base.applicationstatusmessage | shape39 (documentproperties) | Extract HTTP error response message |

---

## 8. DATA DEPENDENCY GRAPH (Step 4) - BLOCKING

### ✅ Self-Check Results
- ✅ Dependency graph built: **YES**
- ✅ All property writes identified: **YES**
- ✅ All property reads identified: **YES**
- ✅ Dependency chains documented: **YES**

### Dependency Graph

```
PROPERTY: dynamicdocument.URL
  WRITER: shape8 (set URL)
  READERS: shape33 (Leave Oracle Fusion Create operation)
  DEPENDENCY: shape8 MUST execute BEFORE shape33

PROPERTY: process.DPP_Process_Name
  WRITER: shape38 (Input_details)
  READERS: Subprocess shape11, shape23 (email body)
  DEPENDENCY: shape38 MUST execute BEFORE subprocess call (shape21)

PROPERTY: process.DPP_AtomName
  WRITER: shape38 (Input_details)
  READERS: Subprocess shape11, shape23 (email body)
  DEPENDENCY: shape38 MUST execute BEFORE subprocess call (shape21)

PROPERTY: process.DPP_Payload
  WRITER: shape38 (Input_details)
  READERS: Subprocess shape15 (email attachment)
  DEPENDENCY: shape38 MUST execute BEFORE subprocess call (shape21)

PROPERTY: process.DPP_ExecutionID
  WRITER: shape38 (Input_details)
  READERS: Subprocess shape11, shape23 (email body)
  DEPENDENCY: shape38 MUST execute BEFORE subprocess call (shape21)

PROPERTY: process.DPP_File_Name
  WRITER: shape38 (Input_details)
  READERS: Subprocess shape6 (email filename)
  DEPENDENCY: shape38 MUST execute BEFORE subprocess call (shape21)

PROPERTY: process.DPP_Subject
  WRITER: shape38 (Input_details)
  READERS: Subprocess shape6, shape20 (email subject)
  DEPENDENCY: shape38 MUST execute BEFORE subprocess call (shape21)

PROPERTY: process.To_Email
  WRITER: shape38 (Input_details)
  READERS: Subprocess shape6, shape20 (email recipient)
  DEPENDENCY: shape38 MUST execute BEFORE subprocess call (shape21)

PROPERTY: process.DPP_HasAttachment
  WRITER: shape38 (Input_details)
  READERS: Subprocess Decision shape4 (attachment check)
  DEPENDENCY: shape38 MUST execute BEFORE subprocess call (shape21)

PROPERTY: process.DPP_ErrorMessage
  WRITER: shape19 (from catcherrorsmessage), shape39 (from applicationstatusmessage), shape46 (from current)
  READERS: Map f46b845a Function 1 (PropertyGet)
  DEPENDENCIES:
    - shape19 MUST execute BEFORE shape41 (error map)
    - shape39 MUST execute BEFORE shape40 (error map)
    - shape46 MUST execute BEFORE shape47 (error map)

TRACK PROPERTY: meta.base.applicationstatuscode
  WRITER: Operation shape33 (Leave Oracle Fusion Create)
  READERS: Decision shape2 (HTTP Status 20* check)
  DEPENDENCY: shape33 MUST execute BEFORE shape2

TRACK PROPERTY: dynamicdocument.DDP_RespHeader
  WRITER: Operation shape33 (response header mapping)
  READERS: Decision shape44 (Content-Encoding check)
  DEPENDENCY: shape33 MUST execute BEFORE shape44
```

### Complete Dependency Chains

**Chain 1: Main Process Flow (Success Path)**
```
shape38 (Input_details - writes all process properties)
  → shape17 (Try/Catch wrapper)
  → shape29 (Map - D365 to Oracle Fusion)
  → shape8 (set URL - writes dynamicdocument.URL)
  → shape49 (Notify - log request)
  → shape33 (Leave Oracle Fusion Create - writes meta.base.applicationstatuscode, dynamicdocument.DDP_RespHeader)
  → shape2 (Decision - reads meta.base.applicationstatuscode)
  → shape34 (Map - Oracle Fusion response to D365 response)
  → shape35 (Return Documents - Success Response)
```

**Chain 2: Error Path - HTTP Non-20* Status**
```
shape33 (Leave Oracle Fusion Create - writes meta.base.applicationstatuscode)
  → shape2 (Decision - reads meta.base.applicationstatuscode - FALSE path)
  → shape44 (Decision - reads dynamicdocument.DDP_RespHeader - FALSE path)
  → shape39 (ErrorMsg - writes process.DPP_ErrorMessage from meta.base.applicationstatusmessage)
  → shape40 (Map - reads process.DPP_ErrorMessage)
  → shape36 (Return Documents - Error Response)
```

**Chain 3: Error Path - Try/Catch Exception**
```
shape17 (Try/Catch - catches error)
  → shape20 (Branch)
  → shape19 (ErrorMsg - writes process.DPP_ErrorMessage from meta.base.catcherrorsmessage)
  → shape21 (Subprocess call - reads all process properties written by shape38)
  → Subprocess execution (email notification)
  → shape41 (Map - reads process.DPP_ErrorMessage)
  → shape43 (Return Documents - Error Response)
```

### Independent Operations (No Dependencies)
- shape1 (START) - Entry point, no dependencies
- shape38 (Input_details) - Writes properties, reads execution properties (always available)

---

## 9. CONTROL FLOW GRAPH (Step 5)

### ✅ Self-Check Results
- ✅ All dragpoints extracted from JSON: **YES**
- ✅ All toShape references documented: **YES**
- ✅ Decision TRUE/FALSE paths identified: **YES**
- ✅ Branch paths documented: **YES**
- ✅ Control flow graph is complete: **YES**

### Control Flow Map (Dragpoint Connections)

| Source Shape | Shape Type | Dragpoint Identifier | Destination Shape (toShape) | Notes |
|---|---|---|---|---|
| shape1 | start | default | shape38 | Entry point → Input details |
| shape38 | documentproperties | default | shape17 | Input details → Try/Catch wrapper |
| shape17 | catcherrors | default (Try) | shape29 | Try path |
| shape17 | catcherrors | error (Catch) | shape20 | Catch path |
| shape29 | map | default | shape8 | Map D365 to Oracle Fusion |
| shape8 | documentproperties | default | shape49 | Set URL → Notify |
| shape49 | notify | default | shape33 | Notify → HTTP Call |
| shape33 | connectoraction | default | shape2 | HTTP Call → Decision (status check) |
| shape2 | decision | true | shape34 | HTTP 20* → Success map |
| shape2 | decision | false | shape44 | HTTP non-20* → Content-Encoding check |
| shape34 | map | default | shape35 | Success map → Return success |
| shape35 | returndocuments | (terminal) | - | Success response (terminal) |
| shape44 | decision | true | shape45 | Content-Encoding = gzip → Decompress |
| shape44 | decision | false | shape39 | Content-Encoding ≠ gzip → Extract error |
| shape45 | dataprocess | default | shape46 | Gzip decompress → Extract error |
| shape46 | documentproperties | default | shape47 | Error message → Error map |
| shape47 | map | default | shape48 | Error map → Return error |
| shape48 | returndocuments | (terminal) | - | Error response (terminal) |
| shape39 | documentproperties | default | shape40 | Error message → Error map |
| shape40 | map | default | shape36 | Error map → Return error |
| shape36 | returndocuments | (terminal) | - | Error response (terminal) |
| shape20 | branch | 1 | shape19 | Branch path 1 → Error message |
| shape20 | branch | 2 | shape41 | Branch path 2 → Error map |
| shape19 | documentproperties | default | shape21 | Error message → Subprocess call |
| shape21 | processcall | (no dragpoints) | - | Subprocess call (terminal - continues via subprocess return) |
| shape41 | map | default | shape43 | Error map → Return error |
| shape43 | returndocuments | (terminal) | - | Error response (terminal) |

**shape28 (message)** - Disconnected test data shape (toShape: "unset"), not used in execution flow

### Reverse Flow Mapping (Convergence Points)

| Target Shape | Incoming Connections (Sources) | Convergence Type |
|---|---|---|
| shape38 | shape1 | Single entry point |
| shape17 | shape38 | Single path |
| shape29 | shape17 (Try path) | Single path (Try branch) |
| shape8 | shape29 | Single path |
| shape49 | shape8 | Single path |
| shape33 | shape49 | Single path |
| shape2 | shape33 | Single path |
| shape34 | shape2 (TRUE path) | Decision branch |
| shape44 | shape2 (FALSE path) | Decision branch |
| shape35 | shape34 | Single path (terminal) |
| shape45 | shape44 (TRUE path) | Decision branch |
| shape39 | shape44 (FALSE path) | Decision branch |
| shape46 | shape45 | Single path |
| shape47 | shape46 | Single path |
| shape48 | shape47 | Single path (terminal) |
| shape40 | shape39 | Single path |
| shape36 | shape40 | Single path (terminal) |
| shape20 | shape17 (Catch path) | Single path (Catch branch) |
| shape19 | shape20 (path 1) | Branch path |
| shape41 | shape20 (path 2) | Branch path |
| shape21 | shape19 | Single path |
| shape43 | shape41 | Single path (terminal) |

**No Convergence Points:** All paths are divergent (decisions, branches) with no rejoining. Each path leads to its own terminal Return Documents shape.

### Connection Summary
- **Total Shapes:** 23 shapes (excluding shape28 - unused test data)
- **Total Dragpoints:** 22 connections
- **Decision Shapes:** 2 (shape2, shape44)
- **Branch Shapes:** 1 (shape20)
- **Try/Catch Shapes:** 1 (shape17)
- **Terminal Shapes:** 4 Return Documents shapes (shape35, shape36, shape43, shape48)

---

## 10. DECISION SHAPE ANALYSIS (Step 7) - BLOCKING

### ✅ Self-Check Results
- ✅ Decision data sources identified: **YES**
- ✅ Decision types classified: **YES**
- ✅ Execution order verified: **YES**
- ✅ All decision paths traced: **YES**
- ✅ Decision patterns identified: **YES**

### Decision Inventory

#### Decision 1: HTTP Status 20* Check (shape2)

**Shape ID:** shape2  
**User Label:** "HTTP Status 20 check"  
**Comparison Type:** wildcard  
**Value 1:** meta.base.applicationstatuscode (track property)  
**Value 2:** "20*" (static)

**Data Source:** TRACK_PROPERTY (HTTP response status code from Oracle Fusion operation)  
**Decision Type:** POST-OPERATION (checks response data from previous operation)  
**Actual Execution Order:** Operation (shape33) → Extract HTTP Status → Decision → Route to Success/Error

**Business Logic:**
- This decision checks the HTTP status code returned by Oracle Fusion HCM REST API
- If status is 20* (200, 201, etc.) → Success path
- If status is NOT 20* (400, 404, 500, etc.) → Error path

**TRUE Path:**
- **Destination:** shape34 (Oracle Fusion Leave Response Map)
- **Path:** shape34 → shape35 (Return Documents - Success Response)
- **Termination:** Return Documents (HTTP 200)

**FALSE Path:**
- **Destination:** shape44 (Check Response Content Type)
- **Path:** shape44 → [Decision branches to shape45 or shape39]
- **Termination:** Return Documents (HTTP 400/500) via shape36 or shape48

**Pattern:** Error Check (Success vs Failure) - Routes to success or error handling based on HTTP status code  
**Convergence Point:** None (paths diverge to different terminal points)  
**Early Exit:** None (both paths eventually terminate)

#### Decision 2: Check Response Content Type (shape44)

**Shape ID:** shape44  
**User Label:** "Check Response Content Type"  
**Comparison Type:** equals  
**Value 1:** dynamicdocument.DDP_RespHeader (track property)  
**Value 2:** "gzip" (static)

**Data Source:** PROCESS_PROPERTY (Response header from Oracle Fusion operation)  
**Decision Type:** POST-OPERATION (checks response header data from previous operation)  
**Actual Execution Order:** Operation (shape33) → Extract Content-Encoding Header → Decision shape2 (FALSE) → Decision shape44 → Route to Gzip/Non-Gzip Error Handling

**Business Logic:**
- This decision checks if the Oracle Fusion error response is gzip-compressed
- If Content-Encoding = "gzip" → Decompress response before extracting error message
- If Content-Encoding ≠ "gzip" → Extract error message directly

**TRUE Path (Gzipped Response):**
- **Destination:** shape45 (Custom Scripting - Gzip Decompression)
- **Path:** shape45 → shape46 (error msg) → shape47 (error map) → shape48 (Return Documents - Error Response)
- **Termination:** Return Documents (HTTP 500)

**FALSE Path (Non-Gzipped Response):**
- **Destination:** shape39 (ErrorMsg - Extract from applicationstatusmessage)
- **Path:** shape39 → shape40 (error map) → shape36 (Return Documents - Error Response)
- **Termination:** Return Documents (HTTP 400/500)

**Pattern:** Conditional Logic (Optional Processing) - Determines if gzip decompression is needed  
**Convergence Point:** None (both paths lead to error response, but different terminal shapes)  
**Early Exit:** None (both paths eventually terminate with error response)

### Decision Patterns Summary

| Pattern Type | Decision Shapes | Description |
|---|---|---|
| Error Check (Success vs Failure) | shape2 | Routes to success or error path based on HTTP status code |
| Conditional Logic (Optional Processing) | shape44 | Determines if gzip decompression is needed for error response |

### Decision Data Source Analysis

**Decision shape2:**
- **Data Source:** TRACK_PROPERTY (meta.base.applicationstatuscode)
- **Produced By:** Operation shape33 (Leave Oracle Fusion Create)
- **Type:** POST-OPERATION
- **Business Logic:** Check HTTP response status code to determine success/failure
- **Execution Flow:** Operation → Decision → Route based on status

**Decision shape44:**
- **Data Source:** PROCESS_PROPERTY (dynamicdocument.DDP_RespHeader)
- **Produced By:** Operation shape33 (response header mapping)
- **Type:** POST-OPERATION
- **Business Logic:** Check if error response is gzip-compressed
- **Execution Flow:** Operation → Decision shape2 (FALSE) → Decision shape44 → Route based on Content-Encoding

---

## 11. BRANCH SHAPE ANALYSIS (Step 8) - BLOCKING

### ✅ Self-Check Results
- ✅ All branches classified: **YES**
- ✅ Assumption check: **NO** (analyzed dependencies)
- ✅ Properties extracted for each path: **YES**
- ✅ Dependency graph built: **YES**
- ✅ Topological sort applied: **N/A** (paths are independent)

### Branch Shape Analysis

#### Branch 1: Error Handling Branch (shape20)

**Shape ID:** shape20  
**User Label:** (none)  
**Number of Paths:** 2  
**Location:** Catch error path (after shape17 Try/Catch)

**Path Analysis:**

**Path 1:**
- **Destination:** shape19 (ErrorMsg)
- **Operations:** shape19 (documentproperties) → shape21 (processcall - subprocess email notification)
- **Properties READ:** None (shape19 reads track property meta.base.catcherrorsmessage)
- **Properties WRITTEN:** process.DPP_ErrorMessage (by shape19)
- **Terminal:** Subprocess call (shape21) - process terminates after email sent
- **API Calls:** None (subprocess contains email operation, but that's in subprocess context)

**Path 2:**
- **Destination:** shape41 (Map - Leave Error Map)
- **Operations:** shape41 (map) → shape43 (returndocuments)
- **Properties READ:** process.DPP_ErrorMessage (by map function PropertyGet)
- **Properties WRITTEN:** None
- **Terminal:** Return Documents (shape43)
- **API Calls:** None

**Dependency Analysis:**

**Path 1 → Path 2 Dependency Check:**
- Path 2 reads process.DPP_ErrorMessage
- Path 1 writes process.DPP_ErrorMessage (via shape19)
- **Conclusion:** Path 2 DEPENDS on Path 1 (reads property written by Path 1)

**Dependency Graph:**
```
Path 1 (shape19) → writes process.DPP_ErrorMessage
Path 2 (shape41) → reads process.DPP_ErrorMessage
Therefore: Path 1 MUST execute BEFORE Path 2
```

**Classification:** **SEQUENTIAL**  
**Reason:** Path 2 depends on Path 1 (data dependency: process.DPP_ErrorMessage)  
**API Call Check:** No API calls in either path (subprocess email is separate context)

**Topological Sort Order:**
```
Path 1 (shape19 → shape21) → Path 2 (shape41 → shape43)
```

**Execution Flow:**
1. Execute Path 1: shape19 writes process.DPP_ErrorMessage → shape21 (subprocess call - email notification)
2. Execute Path 2: shape41 reads process.DPP_ErrorMessage → map error response → shape43 (return error)

**Path Termination:**
- **Path 1:** Subprocess call (shape21) - no explicit return, process continues
- **Path 2:** Return Documents (shape43) - terminal

**Convergence Points:** None (Path 2 is the continuation after Path 1)

**Execution Continues From:** Path 2 (shape41) - final error response return

### Branch Classification Summary

| Branch Shape | Number of Paths | Classification | Reason | Execution Order |
|---|---|---|---|---|
| shape20 | 2 | SEQUENTIAL | Path 2 depends on Path 1 (reads process.DPP_ErrorMessage written by Path 1) | Path 1 → Path 2 |

---

## 12. EXECUTION ORDER (Step 9) - BLOCKING

### ✅ Self-Check Results
- ✅ Business logic verified FIRST: **YES**
- ✅ Operation analysis complete: **YES**
- ✅ Business logic execution order identified: **YES**
- ✅ Data dependencies checked FIRST: **YES**
- ✅ Operation response analysis used: **YES** (Step 1c)
- ✅ Decision analysis used: **YES** (Step 7)
- ✅ Dependency graph used: **YES** (Step 4)
- ✅ Branch analysis used: **YES** (Step 8)
- ✅ Property dependency verification: **YES**

### Business Logic Flow (Step 0 - MUST BE FIRST)

#### Operation Analysis

**Operation 1: Create Leave Oracle Fusion OP (shape1)**
- **Purpose:** Entry point - Receives leave request from D365 via Web Service
- **Outputs:** Request document (JSON)
- **Dependent Operations:** All subsequent operations (entire process depends on this)
- **Business Flow:** Receives leave request → Triggers process execution

**Operation 2: Leave Oracle Fusion Create (shape33)**
- **Purpose:** HTTP POST to Oracle Fusion HCM REST API to create absence/leave record
- **Outputs:** 
  - HTTP response (JSON) containing personAbsenceEntryId
  - meta.base.applicationstatuscode (HTTP status code)
  - dynamicdocument.DDP_RespHeader (Content-Encoding header)
- **Dependent Operations:** 
  - Decision shape2 (checks HTTP status code)
  - Decision shape44 (checks Content-Encoding header)
  - Map shape34 (maps success response)
  - Map shape39/shape40 (maps error response)
- **Business Flow:** Call Oracle Fusion API → Receive response → Extract status and headers → Decision checks status

**Operation 3: Email w Attachment / Email W/O Attachment (subprocess)**
- **Purpose:** Send error notification email to integration team
- **Outputs:** None (terminal operation)
- **Dependent Operations:** None
- **Business Flow:** Send email notification on error → Process terminates

#### Business Logic Execution Order

**Primary Flow (Success Path):**
```
1. Receive leave request (shape1 - START)
2. Extract input details and set process properties (shape38)
3. Try/Catch wrapper begins (shape17 - Try path)
4. Map D365 request to Oracle Fusion format (shape29)
5. Set Oracle Fusion API URL (shape8)
6. Log request (shape49 - Notify)
7. Call Oracle Fusion HCM API (shape33)
   → PRODUCES: HTTP status code, response headers, response body
8. Check HTTP status code (shape2)
   → IF 20* (Success):
     9a. Map Oracle Fusion response to D365 format (shape34)
     10a. Return success response (shape35) [HTTP 200]
```

**Error Flow (HTTP Non-20* Status, Non-Gzipped Response):**
```
8. Check HTTP status code (shape2)
   → IF NOT 20* (Error):
     9b. Check Content-Encoding header (shape44)
       → IF NOT gzip:
         10b. Extract error message from applicationstatusmessage (shape39)
         11b. Map error to D365 format (shape40)
         12b. Return error response (shape36) [HTTP 400/500]
```

**Error Flow (HTTP Non-20* Status, Gzipped Response):**
```
8. Check HTTP status code (shape2)
   → IF NOT 20* (Error):
     9b. Check Content-Encoding header (shape44)
       → IF gzip:
         10c. Decompress gzip response (shape45 - Custom Scripting)
         11c. Extract error message (shape46)
         12c. Map error to D365 format (shape47)
         13c. Return error response (shape48) [HTTP 500]
```

**Error Flow (Try/Catch Exception):**
```
3. Try/Catch wrapper catches exception (shape17 - Catch path)
4. Branch to error handling (shape20)
5. Path 1: Extract error message (shape19)
6. Path 1: Call email notification subprocess (shape21)
7. Path 2: Map error to D365 format (shape41)
8. Path 2: Return error response (shape43) [HTTP 500]
```

#### Data Dependencies MUST BE RESPECTED

**Critical Dependencies:**
1. **shape38 MUST execute BEFORE all operations** (writes all process properties used throughout)
2. **shape8 MUST execute BEFORE shape33** (writes dynamicdocument.URL used by HTTP operation)
3. **shape33 MUST execute BEFORE shape2** (produces HTTP status code checked by decision)
4. **shape33 MUST execute BEFORE shape44** (produces Content-Encoding header checked by decision)
5. **shape19 MUST execute BEFORE shape41** (writes process.DPP_ErrorMessage read by error map)

### Derived Execution Order (Respecting Data Dependencies)

**Main Process Flow (Success Path):**

```
START (shape1)
  ↓
Input Details (shape38) - WRITES: All process properties
  ↓
Try/Catch Begin (shape17 - Try path)
  ↓
Map D365 to Oracle Fusion (shape29)
  ↓
Set URL (shape8) - WRITES: dynamicdocument.URL
  ↓
Notify/Log (shape49)
  ↓
HTTP Call to Oracle Fusion (shape33) - WRITES: meta.base.applicationstatuscode, dynamicdocument.DDP_RespHeader, response body
  ↓
DECISION: HTTP Status 20* check (shape2) - READS: meta.base.applicationstatuscode
  ↓
  ├─ TRUE (20*) → Map Success Response (shape34) → Return Success (shape35) [HTTP 200]
  │
  └─ FALSE (non-20*) → DECISION: Content-Encoding check (shape44) - READS: dynamicdocument.DDP_RespHeader
      ↓
      ├─ TRUE (gzip) → Decompress (shape45) → Extract Error (shape46) → Map Error (shape47) → Return Error (shape48) [HTTP 500]
      │
      └─ FALSE (not gzip) → Extract Error (shape39) → Map Error (shape40) → Return Error (shape36) [HTTP 400/500]
```

**Error Flow (Try/Catch Exception):**

```
Try/Catch Exception (shape17 - Catch path)
  ↓
Branch (shape20) - SEQUENTIAL EXECUTION (Path 1 → Path 2)
  ↓
Path 1: Extract Error Message (shape19) - WRITES: process.DPP_ErrorMessage
  ↓
Path 1: Call Email Notification Subprocess (shape21)
  ↓
Path 2: Map Error (shape41) - READS: process.DPP_ErrorMessage
  ↓
Path 2: Return Error (shape43) [HTTP 500]
```

### Execution Order Verification

**Dependency Verification:**

| Operation | Reads Properties | Writes Properties | Must Execute After |
|---|---|---|---|
| shape38 | Execution properties | All process properties | shape1 (START) |
| shape29 | Input document | Mapped document | shape17 (Try begins) |
| shape8 | Defined parameter | dynamicdocument.URL | shape29 |
| shape33 | dynamicdocument.URL | meta.base.applicationstatuscode, dynamicdocument.DDP_RespHeader | shape8, shape49 |
| shape2 | meta.base.applicationstatuscode | None | shape33 |
| shape44 | dynamicdocument.DDP_RespHeader | None | shape33, shape2 (FALSE path) |
| shape34 | Response document | Mapped response | shape2 (TRUE path) |
| shape19 | meta.base.catcherrorsmessage | process.DPP_ErrorMessage | shape17 (Catch path), shape20 (Path 1) |
| shape41 | process.DPP_ErrorMessage | Mapped error | shape19 (Path 1 complete) |

**All Dependencies Satisfied:** ✅ YES

---

## 13. SEQUENCE DIAGRAM (Step 10) - BLOCKING

### ✅ Pre-Creation Validation
- ✅ Step 4 (Data Dependency Graph) COMPLETE: **YES**
- ✅ Step 5 (Control Flow Graph) COMPLETE: **YES**
- ✅ Step 7 (Decision Analysis) COMPLETE: **YES**
- ✅ Step 8 (Branch Analysis) COMPLETE: **YES**
- ✅ Step 9 (Execution Order) COMPLETE: **YES**

### Sequence Diagram

**📋 NOTE:** Detailed request/response JSON examples are documented in Section 6 (HTTP Status Codes and Return Path Responses).

Based on execution order analysis in Step 9, control flow in Step 5, decision analysis in Step 7, branch analysis in Step 8, and dependency graph in Step 4.

```
START (shape1 - Web Service Server Listen)
 |
 ├─→ shape38: Input_details (documentproperties)
 |   └─→ WRITES: process.DPP_Process_Name, process.DPP_AtomName, process.DPP_Payload,
 |              process.DPP_ExecutionID, process.DPP_File_Name, process.DPP_Subject,
 |              process.To_Email, process.DPP_HasAttachment
 |
 ├─→ shape17: Try/Catch Wrapper (catcherrors)
 |   |
 |   ├─→ TRY PATH:
 |   |   |
 |   |   ├─→ shape29: Map D365 to Oracle Fusion (map)
 |   |   |   └─→ Transforms: D365 Leave Create JSON → HCM Leave Create JSON
 |   |   |
 |   |   ├─→ shape8: set URL (documentproperties)
 |   |   |   └─→ WRITES: dynamicdocument.URL
 |   |   |   └─→ READS: Defined parameter (PP_HCM_LeaveCreate_Properties.Resource_Path)
 |   |   |
 |   |   ├─→ shape49: Notify (notify)
 |   |   |   └─→ READS: current document (for logging)
 |   |   |   └─→ Logs request payload
 |   |   |
 |   |   ├─→ shape33: Leave Oracle Fusion Create (Downstream - HTTP POST)
 |   |   |   └─→ READS: dynamicdocument.URL
 |   |   |   └─→ WRITES: meta.base.applicationstatuscode, dynamicdocument.DDP_RespHeader
 |   |   |   └─→ HTTP: [Expected: 200/201, Error: 400/401/404/500]
 |   |   |   └─→ Target: https://iaaxey-dev3.fa.ocs.oraclecloud.com:443/hcmRestApi/resources/11.13.18.05/absences
 |   |   |   └─→ Method: POST
 |   |   |   └─→ Auth: Basic (INTEGRATION.USER@al-ghurair.com)
 |   |   |
 |   |   ├─→ shape2: Decision - HTTP Status 20* check (decision)
 |   |   |   └─→ READS: meta.base.applicationstatuscode
 |   |   |   └─→ Comparison: wildcard match "20*"
 |   |   |
 |   |   ├─→ IF TRUE (HTTP 20* - Success):
 |   |   |   |
 |   |   |   ├─→ shape34: Map Oracle Fusion Response to D365 (map)
 |   |   |   |   └─→ Transforms: Oracle Fusion Leave Response → Leave D365 Response
 |   |   |   |   └─→ Extracts: personAbsenceEntryId
 |   |   |   |   └─→ Sets defaults: status="success", message="Data successfully sent to Oracle Fusion", success="true"
 |   |   |   |
 |   |   |   └─→ shape35: Return Documents [HTTP: 200] [Success] [SUCCESS RESPONSE]
 |   |   |       └─→ Response: {"status": "success", "message": "Data successfully sent to Oracle Fusion", 
 |   |   |                      "personAbsenceEntryId": 300000123456789, "success": "true"}
 |   |   |
 |   |   └─→ IF FALSE (HTTP non-20* - Error):
 |   |       |
 |   |       ├─→ shape44: Decision - Check Response Content Type (decision)
 |   |       |   └─→ READS: dynamicdocument.DDP_RespHeader
 |   |       |   └─→ Comparison: equals "gzip"
 |   |       |
 |   |       ├─→ IF TRUE (Content-Encoding = gzip):
 |   |       |   |
 |   |       |   ├─→ shape45: Custom Scripting - Gzip Decompression (dataprocess)
 |   |       |   |   └─→ Script: GZIPInputStream decompression (Groovy)
 |   |       |   |
 |   |       |   ├─→ shape46: error msg (documentproperties)
 |   |       |   |   └─→ WRITES: process.DPP_ErrorMessage
 |   |       |   |   └─→ READS: current document (decompressed error response)
 |   |       |   |
 |   |       |   ├─→ shape47: Map Error to D365 (map)
 |   |       |   |   └─→ READS: process.DPP_ErrorMessage
 |   |       |   |   └─→ Sets: status="failure", success="false", message=DPP_ErrorMessage
 |   |       |   |
 |   |       |   └─→ shape48: Return Documents [HTTP: 500] [Error Code: ERR_GZIP] [ERROR RESPONSE]
 |   |       |       └─→ Response: {"status": "failure", "message": "Error decompressing gzip response", "success": "false"}
 |   |       |
 |   |       └─→ IF FALSE (Content-Encoding ≠ gzip):
 |   |           |
 |   |           ├─→ shape39: error msg (documentproperties)
 |   |           |   └─→ WRITES: process.DPP_ErrorMessage
 |   |           |   └─→ READS: meta.base.applicationstatusmessage
 |   |           |
 |   |           ├─→ shape40: Map Error to D365 (map)
 |   |           |   └─→ READS: process.DPP_ErrorMessage
 |   |           |   └─→ Sets: status="failure", success="false", message=DPP_ErrorMessage
 |   |           |
 |   |           └─→ shape36: Return Documents [HTTP: 400/500] [Error Code: ERR_HTTP] [ERROR RESPONSE]
 |   |               └─→ Response: {"status": "failure", "message": "Bad Request - Invalid absenceType value", "success": "false"}
 |   |
 |   └─→ CATCH PATH (Exception):
 |       |
 |       ├─→ shape20: Branch (branch) - SEQUENTIAL EXECUTION
 |       |   └─→ Classification: SEQUENTIAL (Path 2 depends on Path 1)
 |       |   └─→ Based on branch analysis in Step 8
 |       |
 |       ├─→ Path 1:
 |       |   |
 |       |   ├─→ shape19: ErrorMsg (documentproperties)
 |       |   |   └─→ WRITES: process.DPP_ErrorMessage
 |       |   |   └─→ READS: meta.base.catcherrorsmessage
 |       |   |
 |       |   └─→ shape21: Call Email Notification Subprocess (processcall)
 |       |       └─→ SUBPROCESS: (Sub) Office 365 Email (a85945c5-3004-42b9-80b1-104f465cd1fb)
 |       |       └─→ READS: process.DPP_Process_Name, process.DPP_AtomName, process.DPP_ExecutionID,
 |       |                 process.DPP_ErrorMessage, process.DPP_Payload, process.DPP_File_Name,
 |       |                 process.DPP_Subject, process.To_Email, process.DPP_HasAttachment
 |       |       └─→ See Section 14 for subprocess internal flow
 |       |
 |       └─→ Path 2: (Executes AFTER Path 1 due to data dependency)
 |           |
 |           ├─→ shape41: Map Error to D365 (map)
 |           |   └─→ READS: process.DPP_ErrorMessage
 |           |   └─→ Sets: status="failure", success="false", message=DPP_ErrorMessage
 |           |
 |           └─→ shape43: Return Documents [HTTP: 500] [Error Code: ERR_EXCEPTION] [ERROR RESPONSE]
 |               └─→ Response: {"status": "failure", "message": "Process execution error: Invalid JSON format", "success": "false"}
```

**CRITICAL RULES APPLIED:**
1. ✅ Each operation shows READS and WRITES
2. ✅ Downstream operation marked: shape33 (HTTP POST to Oracle Fusion)
3. ✅ Decisions show both TRUE and FALSE paths
4. ✅ Each return path shows HTTP status code and error/success codes
5. ✅ Sequence diagram references Steps 4, 5, 7, 8, 9 for reasoning
6. ✅ Try/Catch paths documented
7. ✅ Branch paths documented as SEQUENTIAL with execution order
8. ✅ Data dependencies respected (shape8 before shape33, shape19 before shape41)

---

## 14. SUBPROCESS ANALYSIS (Step 7a)

### Subprocess: (Sub) Office 365 Email (a85945c5-3004-42b9-80b1-104f465cd1fb)

**Subprocess Name:** (Sub) Office 365 Email  
**Subprocess ID:** a85945c5-3004-42b9-80b1-104f465cd1fb  
**Type:** process (reusable subprocess)  
**Purpose:** Send email notification (with or without attachment) via Office 365 SMTP  
**Called By:** shape21 (processcall) in main process

### Subprocess Internal Flow

```
START (shape1 - Email content)
 |
 ├─→ shape2: Try/Catch Wrapper (catcherrors)
 |   |
 |   ├─→ TRY PATH:
 |   |   |
 |   |   ├─→ shape4: Decision - Attachment_Check (decision)
 |   |   |   └─→ READS: process.DPP_HasAttachment
 |   |   |   └─→ Comparison: equals "Y"
 |   |   |
 |   |   ├─→ IF TRUE (Has Attachment):
 |   |   |   |
 |   |   |   ├─→ shape11: Mail_Body (message)
 |   |   |   |   └─→ Builds HTML email body with execution details
 |   |   |   |   └─→ READS: process.DPP_Process_Name, process.DPP_AtomName, 
 |   |   |   |             process.DPP_ExecutionID, process.DPP_ErrorMessage
 |   |   |   |
 |   |   |   ├─→ shape14: set_MailBody (documentproperties)
 |   |   |   |   └─→ WRITES: process.DPP_MailBody
 |   |   |   |   └─→ READS: current document (HTML body)
 |   |   |   |
 |   |   |   ├─→ shape15: payload (message)
 |   |   |   |   └─→ READS: process.DPP_Payload
 |   |   |   |   └─→ Creates email attachment content
 |   |   |   |
 |   |   |   ├─→ shape6: set_Mail_Properties (documentproperties)
 |   |   |   |   └─→ WRITES: connector.mail.fromAddress, connector.mail.toAddress,
 |   |   |   |             connector.mail.subject, connector.mail.body, connector.mail.filename
 |   |   |   |   └─→ READS: Defined parameters (PP_Office365_Email properties),
 |   |   |   |             process.To_Email, process.DPP_Subject, process.DPP_MailBody,
 |   |   |   |             process.DPP_File_Name
 |   |   |   |
 |   |   |   ├─→ shape3: Email (connectoraction - mail send with attachment)
 |   |   |   |   └─→ Operation: Email w Attachment (af07502a-fafd-4976-a691-45d51a33b549)
 |   |   |   |   └─→ Connection: Office 365 Email (00eae79b-2303-4215-8067-dcc299e42697)
 |   |   |   |   └─→ Disposition: attachment
 |   |   |   |
 |   |   |   └─→ shape5: Stop (continue=true) [SUCCESS RETURN]
 |   |   |
 |   |   └─→ IF FALSE (No Attachment):
 |   |       |
 |   |       ├─→ shape23: Mail_Body (message)
 |   |       |   └─→ Builds HTML email body (same as shape11)
 |   |       |
 |   |       ├─→ shape22: set_MailBody (documentproperties)
 |   |       |   └─→ WRITES: process.DPP_MailBody
 |   |       |
 |   |       ├─→ shape20: set_Mail_Properties (documentproperties)
 |   |       |   └─→ WRITES: connector.mail.fromAddress, connector.mail.toAddress,
 |   |       |             connector.mail.subject, connector.mail.body
 |   |       |
 |   |       ├─→ shape7: Email (connectoraction - mail send without attachment)
 |   |       |   └─→ Operation: Email W/O Attachment (15a72a21-9b57-49a1-a8ed-d70367146644)
 |   |       |   └─→ Connection: Office 365 Email (00eae79b-2303-4215-8067-dcc299e42697)
 |   |       |   └─→ Disposition: inline
 |   |       |
 |   |       └─→ shape9: Stop (continue=true) [SUCCESS RETURN]
 |   |
 |   └─→ CATCH PATH (Exception):
 |       |
 |       └─→ shape10: Exception (exception)
 |           └─→ Throws exception with error message
 |           └─→ READS: meta.base.catcherrorsmessage
 |           └─→ Stops process execution
```

### Subprocess Return Paths

| Return Label | Path | Terminal Shape | Condition |
|---|---|---|---|
| SUCCESS | Try path → TRUE branch | shape5 (Stop - continue=true) | Email sent successfully with attachment |
| SUCCESS | Try path → FALSE branch | shape9 (Stop - continue=true) | Email sent successfully without attachment |
| ERROR | Catch path | shape10 (Exception) | Email send failed (throws exception) |

### Subprocess Properties

**Properties READ (from main process):**
- process.DPP_Process_Name
- process.DPP_AtomName
- process.DPP_ExecutionID
- process.DPP_ErrorMessage
- process.DPP_Payload
- process.DPP_File_Name
- process.DPP_Subject
- process.To_Email
- process.DPP_HasAttachment

**Properties WRITTEN (local subprocess scope):**
- process.DPP_MailBody
- connector.mail.fromAddress
- connector.mail.toAddress
- connector.mail.subject
- connector.mail.body
- connector.mail.filename (only if has attachment)

### Main Process Mapping

**Return Path Mapping:**
- Subprocess has no explicit return paths defined in main process (shape21 processcall has empty returnpaths)
- Subprocess execution is synchronous (wait=true)
- If exception occurs in subprocess, main process catch block handles it
- After subprocess completes successfully, main process continues to Path 2 (shape41)

### Email Content Details

**Email Subject:**
```
[Environment] ([Atom Name] (Process Name) has errors to report)
Example: "DEV Failure : (Cloud - Development (HCM_Leave Create) has errors to report)"
```

**Email Body (HTML):**
```html
<!DOCTYPE html>
<html>
  <head>
    <meta http-equiv="Content-Type" content="text/html" charset="us-ascii" />
    <meta name="viewport" content="width=device-width" />
  </head>
  <body>
    <pre>
      <font color="black">
        <h1 style="font-size:15px;"><u>Execution Details:</u></h1>
        <table border="1">
          <tr>
            <th scope="row"><b><font size="2">Process Name</font></b></th>
            <td>{DPP_Process_Name}</td>
          </tr>
          <tr>
            <th scope="row"><b><font size="2">Environment</font></b></th>
            <td>{DPP_AtomName}</td>
          </tr>
          <tr>
            <th scope="row"><b><font size="2">Execution ID</font></b></th>
            <td>{DPP_ExecutionID}</td>
          </tr>
          <tr>
            <th scope="row"><b><font size="2">Error Details</font></b></th>
            <td>{DPP_ErrorMessage}</td>
          </tr>
        </table>
      </font>
    </pre>
    <text>P.S: This is system generated email.</text>
  </body>
</html>
```

**Email Attachment (if DPP_HasAttachment = "Y"):**
- **Filename:** `{DPP_File_Name}` (e.g., "HCM_Leave Create_2024-11-04T10:30:00.123Z.txt")
- **Content:** `{DPP_Payload}` (original request JSON)
- **Content Type:** text/plain

---

## 15. CRITICAL PATTERNS IDENTIFIED

### Pattern 1: Try/Catch Error Handling with Email Notification

**Pattern Type:** Exception Handling with Notification  
**Shapes Involved:** shape17 (catcherrors), shape20 (branch), shape19 (documentproperties), shape21 (processcall)

**Description:**
- Main process flow wrapped in Try/Catch block
- If exception occurs, process branches to:
  - Path 1: Send email notification to integration team
  - Path 2: Return error response to caller
- Email notification includes execution details and original payload

**Business Logic:**
- Ensures integration team is notified of process failures
- Provides diagnostic information (error message, execution ID, payload)
- Returns proper error response to caller

### Pattern 2: HTTP Status Code Based Routing

**Pattern Type:** Error Check (Success vs Failure)  
**Shapes Involved:** shape2 (decision), shape34 (success map), shape44 (decision), shape39/shape40 (error maps)

**Description:**
- After calling Oracle Fusion API, check HTTP status code
- If 20* (success) → Map success response
- If non-20* (error) → Check if response is gzipped → Extract/decompress error → Map error response

**Business Logic:**
- Separates success and error handling paths
- Handles gzip-compressed error responses
- Ensures proper error messages are returned to caller

### Pattern 3: Sequential Branch Path Execution with Data Dependency

**Pattern Type:** Sequential Branch (Data Dependency)  
**Shapes Involved:** shape20 (branch), Path 1 (shape19 → shape21), Path 2 (shape41 → shape43)

**Description:**
- Branch has 2 paths, but they must execute sequentially
- Path 1 writes process.DPP_ErrorMessage (used for email notification)
- Path 2 reads process.DPP_ErrorMessage (used for error response mapping)
- Path 2 cannot execute until Path 1 completes

**Business Logic:**
- Ensures error message is available for both email notification and response
- Path 1 sends email notification (non-blocking for caller perspective)
- Path 2 returns error response to caller

### Pattern 4: Dynamic URL Construction

**Pattern Type:** Configuration-Driven Operation  
**Shapes Involved:** shape8 (documentproperties), shape33 (connectoraction)

**Description:**
- URL path is constructed dynamically from defined process property
- Base URL comes from connection configuration
- Resource path comes from process property
- Full URL = Base URL + Resource Path

**Business Logic:**
- Allows environment-specific configuration (DEV, QA, PROD)
- Base URL can be different per environment (connection override)
- Resource path can be different per API version (process property override)

### Pattern 5: Conditional Email Attachment

**Pattern Type:** Conditional Processing (Attachment vs No Attachment)  
**Shapes Involved:** Subprocess shape4 (decision), shape3 (email w/ attachment), shape7 (email w/o attachment)

**Description:**
- Subprocess checks if email should have attachment (process.DPP_HasAttachment)
- If "Y" → Build HTML body + Attach payload → Send email with attachment
- If not "Y" → Build HTML body → Send email without attachment

**Business Logic:**
- Provides flexibility to send error notifications with or without payload
- Reduces email size if attachment not needed
- Supports different notification scenarios

---

## 16. SYSTEM LAYER IDENTIFICATION

### Third-Party Systems Identified

#### System 1: Oracle Fusion HCM

**System Name:** Oracle Fusion Human Capital Management (HCM)  
**System Type:** Cloud ERP - HCM Module  
**Connection Type:** HTTP REST API  
**Authentication:** Basic Authentication

**Connection Details:**
- **Base URL:** https://iaaxey-dev3.fa.ocs.oraclecloud.com:443
- **API Endpoint:** /hcmRestApi/resources/11.13.18.05/absences
- **Method:** POST
- **Content Type:** application/json
- **User:** INTEGRATION.USER@al-ghurair.com
- **Environment:** DEV3

**API Details:**
- **API Name:** Oracle Fusion HCM REST API
- **Resource:** Absences
- **Version:** 11.13.18.05
- **Operation:** Create Absence/Leave Record

**Request Format (JSON):**
```json
{
  "personNumber": 9000604,
  "absenceType": "Sick Leave",
  "employer": "Al Ghurair Investment LLC",
  "startDate": "2024-03-24",
  "endDate": "2024-03-25",
  "absenceStatusCd": "SUBMITTED",
  "approvalStatusCd": "APPROVED",
  "startDateDuration": 1,
  "endDateDuration": 1
}
```

**Response Format (JSON - Success):**
```json
{
  "personAbsenceEntryId": 300000123456789,
  "absenceCaseId": "...",
  "absenceType": "Sick Leave",
  "startDate": "2024-03-24",
  "endDate": "2024-03-25",
  "absenceStatusCd": "SUBMITTED",
  "approvalStatusCd": "APPROVED",
  "startDateDuration": 1,
  "endDateDuration": 1,
  ...
}
```

**Response Format (JSON - Error):**
```json
{
  "errorCode": "INVALID_REQUEST",
  "errorMessage": "Bad Request - Invalid absenceType value",
  ...
}
```

**Expected Status Codes:**
- **Success:** 200 (OK), 201 (Created)
- **Error:** 400 (Bad Request), 401 (Unauthorized), 404 (Not Found), 500 (Internal Server Error)

**System Layer API Required:** YES

**Proposed System Layer API:**
- **API Name:** OracleFusionHCM.Absences.Create
- **HTTP Method:** POST
- **Endpoint:** /api/system/oraclefusionhcm/absences
- **Purpose:** Create absence/leave record in Oracle Fusion HCM
- **Request DTO:** CreateAbsenceRequest
- **Response DTO:** CreateAbsenceResponse

#### System 2: Office 365 Email (SMTP)

**System Name:** Office 365 Email  
**System Type:** Email Service (SMTP)  
**Connection Type:** SMTP  
**Authentication:** SMTP AUTH

**Connection Details:**
- **Host:** smtp-mail.outlook.com
- **Port:** 587
- **Use SSL:** false
- **Use TLS:** true
- **Use SMTP AUTH:** true
- **User:** Boomi.Dev.failures@al-ghurair.com

**Operation:** Send Email (with or without attachment)

**System Layer API Required:** NO (Common notification service, not business-specific)

**Rationale:**
- Email notification is a cross-cutting concern, not specific to HCM domain
- Can be handled by shared notification service/utility
- Not a core business system requiring System Layer abstraction

### System Layer APIs Summary

| System | API Required | API Name | Purpose | HTTP Method | Endpoint |
|---|---|---|---|---|---|
| Oracle Fusion HCM | YES | OracleFusionHCM.Absences.Create | Create absence/leave record | POST | /api/system/oraclefusionhcm/absences |
| Office 365 Email | NO | N/A | Send email notification | N/A | N/A |

---

## 17. FUNCTION EXPOSURE DECISION TABLE - BLOCKING

### ✅ Self-Check Results
- ✅ Function Exposure Decision Table created: **YES**
- ✅ All operations evaluated for function exposure: **YES**
- ✅ Reusability analysis complete: **YES**
- ✅ Function explosion prevention applied: **YES**

### Process Layer Function Analysis

**Process Name:** HCM_Leave Create  
**Business Domain:** Human Resources / Leave Management  
**Business Capability:** Leave/Absence Management

**Current Boomi Trigger:** Web Service Server (Listen) - API endpoint exposed  
**Azure Function Exposure:** **YES** - This should be exposed as an Azure Function

**Rationale:**
1. **Entry Point:** This is a web service server operation, designed to be called by external systems (D365)
2. **Business Process:** Encapsulates business logic for leave creation (validation, transformation, orchestration)
3. **API Contract:** Has defined request/response profiles (JSON)
4. **Reusability:** Could be called by multiple consumers (D365, mobile app, web portal)
5. **API-Led Architecture:** Fits Process Layer definition (orchestrates System Layer APIs)

### Function Exposure Decision

| Process | Expose as Function? | Function Name | Trigger Type | Justification |
|---|---|---|---|---|
| HCM_Leave Create | **YES** | CreateLeaveRequest | HTTP Trigger | Entry point process, orchestrates leave creation between D365 and Oracle Fusion |

### Subprocess Exposure Decision

| Subprocess | Expose as Function? | Justification |
|---|---|---|
| (Sub) Office 365 Email | **NO** | Internal utility subprocess, not a business capability. Should be replaced with shared notification service. |

### Process Layer Function Definition

**Function Name:** CreateLeaveRequest  
**HTTP Method:** POST  
**Route:** /api/process/hcm/leave/create  
**Request DTO:** CreateLeaveRequestDto  
**Response DTO:** CreateLeaveResponseDto

**Request DTO Structure:**
```csharp
public class CreateLeaveRequestDto
{
    public int EmployeeNumber { get; set; }
    public string AbsenceType { get; set; }
    public string Employer { get; set; }
    public string StartDate { get; set; }
    public string EndDate { get; set; }
    public string AbsenceStatusCode { get; set; }
    public string ApprovalStatusCode { get; set; }
    public int StartDateDuration { get; set; }
    public int EndDateDuration { get; set; }
}
```

**Response DTO Structure:**
```csharp
public class CreateLeaveResponseDto
{
    public string Status { get; set; }
    public string Message { get; set; }
    public long? PersonAbsenceEntryId { get; set; }
    public bool Success { get; set; }
}
```

### Function Explosion Prevention

**✅ CRITICAL RULE:** Only expose processes that represent business capabilities, not internal utilities or technical operations.

**Processes NOT to Expose:**
- Subprocesses (utilities, reusable components)
- Scheduled processes (batch jobs, data synchronization)
- Internal transformation processes (data mapping, validation)
- Technical utility processes (error handling, logging, notification)

**This Process:** ✅ APPROVED for function exposure (represents business capability: Create Leave Request)

---

## 18. VALIDATION CHECKLIST

### Data Dependencies
- [x] All property WRITES identified
- [x] All property READS identified
- [x] Dependency graph built
- [x] Execution order satisfies all dependencies (no read-before-write)

### Decision Analysis
- [x] ALL decision shapes inventoried (2 decisions)
- [x] BOTH TRUE and FALSE paths traced to termination
- [x] Pattern type identified for each decision
- [x] Early exits identified and documented (none)
- [x] Convergence points identified (none - all paths diverge)
- [x] Decision data sources identified (TRACK_PROPERTY, PROCESS_PROPERTY)
- [x] Decision types classified (POST-OPERATION)

### Branch Analysis
- [x] Each branch classified as parallel or sequential (1 branch - SEQUENTIAL)
- [x] **SELF-CHECK:** Did I check for API calls in branch paths? **YES** (No API calls in branch paths)
- [x] **SELF-CHECK:** Did I classify or assume? **Classified** (analyzed data dependencies)
- [x] If sequential: dependency_order built using topological sort (Path 1 → Path 2)
- [x] Each path traced to terminal point
- [x] Convergence points identified (none)
- [x] Execution continuation point determined (Path 2 is final path)

### Sequence Diagram
- [x] Format follows required structure (Operation → Decision → Operation)
- [x] Each operation shows READS and WRITES
- [x] Decisions show both TRUE and FALSE paths
- [x] **CROSS-VALIDATION:** Sequence diagram matches control flow graph from Step 5
- [x] **CROSS-VALIDATION:** Execution order matches dependency graph from Step 4
- [x] Early exits marked (none in this process)
- [x] Conditional execution marked (decision branches)
- [x] Subprocess internal flows documented
- [x] Subprocess return paths mapped to main process
- [x] References to Steps 4, 5, 7, 8, 9 included

### Subprocess Analysis
- [x] ALL subprocesses analyzed (1 subprocess: Office 365 Email)
- [x] Internal flow traced
- [x] Return paths identified (success and error)
- [x] Return path labels mapped to main process shapes
- [x] Properties written by subprocess documented
- [x] Properties read by subprocess from main process documented

### Edge Cases
- [x] Nested branches/decisions analyzed (none)
- [x] Loops identified (none)
- [x] Property chains traced (all dependencies traced)
- [x] Circular dependencies detected and resolved (none detected)
- [x] Try/Catch error paths documented

### Property Extraction Completeness
- [x] All property patterns searched (${}, %%, {})
- [x] Message parameters checked for process properties
- [x] Operation headers/path parameters checked
- [x] Decision track properties identified (meta.*)
- [x] Document properties that read other properties identified

### Input/Output Structure Analysis (CONTRACT VERIFICATION)
- [x] Entry point operation identified
- [x] Request profile identified and loaded
- [x] Request profile structure analyzed (JSON)
- [x] Array vs single object detected (single object)
- [x] Array cardinality documented (N/A - single object)
- [x] ALL request fields extracted (9 fields)
- [x] Request field paths documented
- [x] Request field mapping table generated (Boomi → Azure DTO)
- [x] Response profile identified and loaded
- [x] Response profile structure analyzed
- [x] ALL response fields extracted (4 fields)
- [x] Response field mapping table generated
- [x] Document processing behavior determined (single document)
- [x] Input/Output structure documented in Phase 1 document

### HTTP Status Codes and Return Path Responses
- [x] Section 6 present
- [x] All return paths documented with HTTP status codes (4 return paths)
- [x] Response JSON examples provided for each return path
- [x] Populated fields documented for each return path (source and populated by)
- [x] Decision conditions leading to each return documented
- [x] Error codes and success codes documented
- [x] Downstream operation HTTP status codes documented (expected success and error codes)
- [x] Error handling strategy documented

### Map Analysis
- [x] ALL map files identified and loaded (3 maps)
- [x] SOAP request maps identified (N/A - all HTTP/REST operations)
- [x] Field mappings extracted from each map
- [x] Profile vs map field name discrepancies documented (minimal - field renaming only)
- [x] Scripting functions analyzed (1 function: PropertyGet in error map)
- [x] Static values identified and documented (success/failure status defaults)
- [x] Process property mappings documented
- [x] Map Analysis documented in Phase 1 document (Section 5)

### Function Exposure Decision Table
- [x] Function Exposure Decision Table created (Section 17)
- [x] All operations evaluated for function exposure
- [x] Reusability analysis complete
- [x] Function explosion prevention applied
- [x] Decision: Expose as Azure Function (YES - CreateLeaveRequest)

---

## PHASE 1 COMPLETION SUMMARY

### ✅ ALL MANDATORY STEPS COMPLETED

**Step 1a: Input Structure Analysis** ✅ COMPLETE  
**Step 1b: Response Structure Analysis** ✅ COMPLETE  
**Step 1c: Operation Response Analysis** ✅ COMPLETE  
**Step 1d: Map Analysis** ✅ COMPLETE  
**Step 1e: HTTP Status Codes and Return Paths** ✅ COMPLETE  
**Steps 2-3: Process Properties Analysis** ✅ COMPLETE  
**Step 4: Data Dependency Graph** ✅ COMPLETE  
**Step 5: Control Flow Graph** ✅ COMPLETE  
**Step 6: Reverse Flow Mapping** ✅ COMPLETE  
**Step 7: Decision Shape Analysis** ✅ COMPLETE  
**Step 7a: Subprocess Analysis** ✅ COMPLETE  
**Step 8: Branch Shape Analysis** ✅ COMPLETE  
**Step 9: Execution Order** ✅ COMPLETE  
**Step 10: Sequence Diagram** ✅ COMPLETE  

### ✅ ALL SELF-CHECK QUESTIONS ANSWERED YES

**Step 1a Self-Checks:** All YES ✅  
**Step 1b Self-Checks:** All YES ✅  
**Step 1c Self-Checks:** All YES ✅  
**Step 1d Self-Checks:** All YES ✅  
**Step 1e Self-Checks:** All YES ✅  
**Step 7 Self-Checks:** All YES ✅  
**Step 8 Self-Checks:** All YES ✅ (assumption check: NO - analyzed dependencies)  
**Step 9 Self-Checks:** All YES ✅  
**Function Exposure Self-Checks:** All YES ✅

### ✅ ALL VALIDATION CHECKLISTS COMPLETE

**Data Dependencies:** All items checked ✅  
**Decision Analysis:** All items checked ✅  
**Branch Analysis:** All items checked ✅  
**Sequence Diagram:** All items checked ✅  
**Subprocess Analysis:** All items checked ✅  
**Edge Cases:** All items checked ✅  
**Property Extraction:** All items checked ✅  
**Input/Output Structure:** All items checked ✅  
**HTTP Status Codes:** All items checked ✅  
**Map Analysis:** All items checked ✅  
**Function Exposure:** All items checked ✅

---

## NEXT STEPS

**Phase 1 Extraction:** ✅ COMPLETE - All mandatory sections documented  
**Phase 2: Code Generation:** ⏳ READY TO PROCEED

**Critical Information Extracted:**
1. ✅ Input/Output contracts verified (request/response structures)
2. ✅ Process flow analyzed (sequence diagram created)
3. ✅ Data dependencies identified (dependency graph built)
4. ✅ Error handling patterns documented (try/catch, decision routing)
5. ✅ System Layer API identified (Oracle Fusion HCM Absences API)
6. ✅ Process Layer function exposure decided (CreateLeaveRequest)
7. ✅ Subprocess flow analyzed (email notification)
8. ✅ All maps analyzed (field transformations documented)
9. ✅ HTTP status codes and return paths documented

**Phase 2 will generate:**
- Process Layer Azure Function (CreateLeaveRequest)
- System Layer API (OracleFusionHCM.Absences.Create)
- DTOs (request/response models)
- Service classes (orchestration logic)
- Error handling (try/catch, status code routing)
- Configuration (Oracle Fusion connection settings)

---

**Document Status:** ✅ PHASE 1 COMPLETE - Ready for Phase 2 Code Generation  
**Document Version:** 1.0  
**Created Date:** 2025-02-11  
**Total Sections:** 18  
**Total Pages:** Comprehensive extraction document

---

**END OF PHASE 1 EXTRACTION DOCUMENT**
