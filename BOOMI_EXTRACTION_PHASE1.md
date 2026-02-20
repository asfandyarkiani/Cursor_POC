# BOOMI EXTRACTION PHASE 1: HCM Leave Create Process

**Process Name:** HCM_Leave Create  
**Process ID:** ca69f858-785f-4565-ba1f-b4edc6cca05b  
**Version:** 29  
**Description:** This Process will sync the Leave data between D365 and Oracle HCM  
**Business Domain:** Human Resources (HCM)  
**Last Modified:** 2024-11-04T08:54:39Z  
**Modified By:** Rajesh.Muppala@al-ghurair.com

---

## 1. Operations Inventory

### Main Process Operations

| Operation ID | Operation Name | Type | Sub-Type | Purpose |
|---|---|---|---|---|
| 8f709c2b-e63f-4d5f-9374-2932ed70415d | Create Leave Oracle Fusion OP | connector-action | wss | Entry point - Web Service Server Listen operation |
| 6e8920fd-af5a-430b-a1d9-9fde7ac29a12 | Leave Oracle Fusion Create | connector-action | http | HTTP POST to Oracle Fusion HCM API |
| af07502a-fafd-4976-a691-45d51a33b549 | Email w Attachment | connector-action | mail | Send email with attachment (subprocess) |
| 15a72a21-9b57-49a1-a8ed-d70367146644 | Email W/O Attachment | connector-action | mail | Send email without attachment (subprocess) |

### Subprocess Operations

**Subprocess ID:** a85945c5-3004-42b9-80b1-104f465cd1fb  
**Subprocess Name:** (Sub) Office 365 Email  
**Purpose:** Email notification handler with attachment logic

---

## 2. Input Structure Analysis (Step 1a)

### Entry Point Operation

**Operation ID:** 8f709c2b-e63f-4d5f-9374-2932ed70415d  
**Operation Name:** Create Leave Oracle Fusion OP  
**Operation Type:** connector-action (wss - Web Service Server)  
**Action Type:** Listen  
**Input Type:** singlejson  
**Output Type:** singlejson

### Request Profile Structure

**Profile ID:** febfa3e1-f719-4ee8-ba57-cdae34137ab3  
**Profile Name:** D365 Leave Create JSON Profile  
**Profile Type:** profile.json  
**Root Element:** Root  
**Structure Path:** Root/Object/...

### Array Detection

**Is Array:** ❌ NO - Single object structure  
**Array Cardinality:** N/A (single object)

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

**Behavior:** Single document processing  
**Description:** Boomi processes single leave request document per execution  
**Execution Pattern:** Single execution per request  
**Session Management:** One session per execution

### Request Fields Inventory

| Field Name | Field Path | Data Type | Required | Mappable | Notes |
|---|---|---|---|---|---|
| employeeNumber | Root/Object/employeeNumber | number | Yes | Yes | Tracked field (hr_employee_id) |
| absenceType | Root/Object/absenceType | character | Yes | Yes | Leave type (Sick Leave, Annual Leave, etc.) |
| employer | Root/Object/employer | character | Yes | Yes | Legal entity name |
| startDate | Root/Object/startDate | character | Yes | Yes | Leave start date (YYYY-MM-DD) |
| endDate | Root/Object/endDate | character | Yes | Yes | Leave end date (YYYY-MM-DD) |
| absenceStatusCode | Root/Object/absenceStatusCode | character | Yes | Yes | Status code (SUBMITTED, APPROVED, etc.) |
| approvalStatusCode | Root/Object/approvalStatusCode | character | Yes | Yes | Approval status code |
| startDateDuration | Root/Object/startDateDuration | number | Yes | Yes | Duration for start date (0-1) |
| endDateDuration | Root/Object/endDateDuration | number | Yes | Yes | Duration for end date (0-1) |

**Total Fields:** 9 fields

### Field Mapping (Boomi → Azure DTO)

| Boomi Field Path | Boomi Field Name | Data Type | Azure DTO Property | Notes |
|---|---|---|---|---|
| Root/Object/employeeNumber | employeeNumber | number | EmployeeNumber | Used in map to personNumber |
| Root/Object/absenceType | absenceType | character | AbsenceType | Direct mapping |
| Root/Object/employer | employer | character | Employer | Direct mapping |
| Root/Object/startDate | startDate | character | StartDate | Date format YYYY-MM-DD |
| Root/Object/endDate | endDate | character | EndDate | Date format YYYY-MM-DD |
| Root/Object/absenceStatusCode | absenceStatusCode | character | AbsenceStatusCode | Maps to absenceStatusCd |
| Root/Object/approvalStatusCode | approvalStatusCode | character | ApprovalStatusCode | Maps to approvalStatusCd |
| Root/Object/startDateDuration | startDateDuration | number | StartDateDuration | Numeric (0-1) |
| Root/Object/endDateDuration | endDateDuration | number | EndDateDuration | Numeric (0-1) |

---

## 3. Response Structure Analysis (Step 1b)

### Response Profile Structure

**Profile ID:** f4ca3a70-114a-4601-bad8-44a3eb20e2c0  
**Profile Name:** Leave D365 Response  
**Profile Type:** profile.json  
**Root Element:** leaveResponse  
**Structure Path:** leaveResponse/Object/...

### Response Format (JSON Structure)

```json
{
  "status": "success",
  "message": "Data successfully sent to Oracle Fusion",
  "personAbsenceEntryId": 12345,
  "success": "true"
}
```

### Response Fields Inventory

| Field Name | Field Path | Data Type | Mappable | Notes |
|---|---|---|---|---|
| status | leaveResponse/Object/status | character | Yes | Success/failure indicator |
| message | leaveResponse/Object/message | character | Yes | Response message |
| personAbsenceEntryId | leaveResponse/Object/personAbsenceEntryId | number | Yes | Oracle HCM absence entry ID |
| success | leaveResponse/Object/success | character | Yes | Boolean flag (true/false) |

**Total Fields:** 4 fields

### Response Field Mapping (Boomi → Azure DTO)

| Boomi Field Path | Boomi Field Name | Data Type | Azure DTO Property | Notes |
|---|---|---|---|---|
| leaveResponse/Object/status | status | character | Status | "success" or "failure" |
| leaveResponse/Object/message | message | character | Message | Descriptive message |
| leaveResponse/Object/personAbsenceEntryId | personAbsenceEntryId | number | PersonAbsenceEntryId | Oracle HCM ID |
| leaveResponse/Object/success | success | character | Success | "true" or "false" |

---

## 4. Operation Response Analysis (Step 1c)

### Operation: Leave Oracle Fusion Create (6e8920fd-af5a-430b-a1d9-9fde7ac29a12)

**Operation Type:** HTTP POST  
**Response Profile:** NONE (responseProfileType: NONE)  
**Return Errors:** true  
**Return Responses:** true  
**Data Content Type:** application/json

**Response Header Mapping:**
- Header: Content-Encoding
- Target Property: DDP_RespHeader (dynamic document property)

**Extracted Fields:**
- **DDP_RespHeader** - Extracted from HTTP response header "Content-Encoding"
  - Written by: Response header mapping (automatic)
  - Used by: Decision shape2 (checks if response is gzip compressed)

**Response Data:**
- Profile ID: 316175c7-0e45-4869-9ac6-5f9d69882a62 (Oracle Fusion Leave Response JSON Profile)
- Response contains: personAbsenceEntryId, absenceStatusCd, approvalStatusCd, and many other Oracle HCM fields
- **Track Property:** meta.base.applicationstatuscode (HTTP status code from response)
  - Used by: Decision shape2 (checks if status code matches "20*")

**Consumers:**
1. **Decision shape2** - Checks meta.base.applicationstatuscode (HTTP status code)
   - Data Source: TRACK_PROPERTY (from operation response)
   - Dependency: shape33 (Operation) MUST execute BEFORE shape2 (Decision)

2. **Decision shape44** - Checks dynamicdocument.DDP_RespHeader (Content-Encoding header)
   - Data Source: TRACK_PROPERTY (from operation response header)
   - Dependency: shape33 (Operation) MUST execute BEFORE shape44 (Decision)

**Business Logic:**
- Operation executes HTTP POST to Oracle Fusion HCM
- Response includes HTTP status code and Content-Encoding header
- Decisions check response metadata to determine success/failure path
- **CRITICAL:** Operation MUST execute FIRST, then decisions check response

---

## 5. Map Analysis (Step 1d)

### Map Inventory

| Map ID | Map Name | From Profile | To Profile | Type |
|---|---|---|---|---|
| c426b4d6-2aff-450e-b43b-59956c4dbc96 | Leave Create Map | febfa3e1-f719-4ee8-ba57-cdae34137ab3 | a94fa205-c740-40a5-9fda-3d018611135a | Request transformation |
| e4fd3f59-edb5-43a1-aeae-143b600a064e | Oracle Fusion Leave Response Map | 316175c7-0e45-4869-9ac6-5f9d69882a62 | f4ca3a70-114a-4601-bad8-44a3eb20e2c0 | Success response |
| f46b845a-7d75-41b5-b0ad-c41a6a8e9b12 | Leave Error Map | 23d7a2e9-5cb0-4e9c-9e4b-3154834bad0d | f4ca3a70-114a-4601-bad8-44a3eb20e2c0 | Error response |

### Map: Leave Create Map (c426b4d6-2aff-450e-b43b-59956c4dbc96)

**From Profile:** febfa3e1-f719-4ee8-ba57-cdae34137ab3 (D365 Leave Create JSON Profile)  
**To Profile:** a94fa205-c740-40a5-9fda-3d018611135a (HCM Leave Create JSON Profile)  
**Type:** HTTP Request transformation (D365 → Oracle HCM)

**Purpose:** Transform D365 leave request to Oracle Fusion HCM format

**Field Mappings:**

| Source Field | Source Path | Target Field | Target Path | Transformation |
|---|---|---|---|---|
| employeeNumber | Root/Object/employeeNumber | personNumber | Root/Object/personNumber | Direct mapping |
| absenceType | Root/Object/absenceType | absenceType | Root/Object/absenceType | Direct mapping |
| employer | Root/Object/employer | employer | Root/Object/employer | Direct mapping |
| startDate | Root/Object/startDate | startDate | Root/Object/startDate | Direct mapping |
| endDate | Root/Object/endDate | endDate | Root/Object/endDate | Direct mapping |
| absenceStatusCode | Root/Object/absenceStatusCode | absenceStatusCd | Root/Object/absenceStatusCd | Field name change |
| approvalStatusCode | Root/Object/approvalStatusCode | approvalStatusCd | Root/Object/approvalStatusCd | Field name change |
| startDateDuration | Root/Object/startDateDuration | startDateDuration | Root/Object/startDateDuration | Direct mapping |
| endDateDuration | Root/Object/endDateDuration | endDateDuration | Root/Object/endDateDuration | Direct mapping |

**Profile vs Map Field Name Comparison:**

| Source Profile Field | Target Profile Field | Map Target Field | Discrepancy? |
|---|---|---|---|
| absenceStatusCode | absenceStatusCd | absenceStatusCd | ✅ Match |
| approvalStatusCode | approvalStatusCd | approvalStatusCd | ✅ Match |

**Scripting Functions:** None

**Static Values:** None

**Process Property Mappings:** None

**Element Names:** Standard JSON object structure (no SOAP elements)

**Authority:** Map field names are AUTHORITATIVE for HTTP request body

### Map: Oracle Fusion Leave Response Map (e4fd3f59-edb5-43a1-aeae-143b600a064e)

**From Profile:** 316175c7-0e45-4869-9ac6-5f9d69882a62 (Oracle Fusion Leave Response JSON Profile)  
**To Profile:** f4ca3a70-114a-4601-bad8-44a3eb20e2c0 (Leave D365 Response)  
**Type:** Success response transformation

**Purpose:** Transform Oracle Fusion HCM response to D365 response format

**Field Mappings:**

| Source Field | Source Path | Target Field | Target Path | Transformation |
|---|---|---|---|---|
| personAbsenceEntryId | Root/Object/personAbsenceEntryId | personAbsenceEntryId | leaveResponse/Object/personAbsenceEntryId | Direct mapping |

**Default Values:**

| Target Field | Default Value | Purpose |
|---|---|---|
| status | "success" | Success indicator |
| message | "Data successfully sent to Oracle Fusion" | Success message |
| success | "true" | Boolean success flag |

### Map: Leave Error Map (f46b845a-7d75-41b5-b0ad-c41a6a8e9b12)

**From Profile:** 23d7a2e9-5cb0-4e9c-9e4b-3154834bad0d (Dummy FF Profile)  
**To Profile:** f4ca3a70-114a-4601-bad8-44a3eb20e2c0 (Leave D365 Response)  
**Type:** Error response transformation

**Purpose:** Transform error information to D365 error response format

**Field Mappings:**

| Source Type | Source | Target Field | Target Path | Transformation |
|---|---|---|---|---|
| function | PropertyGet(DPP_ErrorMessage) | message | leaveResponse/Object/message | Get error message from process property |

**Function Analysis:**

**Function 1: PropertyGet**
- **Type:** PropertyGet (Get Dynamic Process Property)
- **Input:** Property Name = "DPP_ErrorMessage"
- **Output:** Result (error message text)
- **Purpose:** Retrieve error message from process property DPP_ErrorMessage

**Default Values:**

| Target Field | Default Value | Purpose |
|---|---|---|
| status | "failure" | Failure indicator |
| success | "false" | Boolean failure flag |

**Authority:** Map uses process property DPP_ErrorMessage for error message content

---

## 6. HTTP Status Codes and Return Path Responses (Step 1e)

### Return Path Inventory

| Return Shape ID | Return Label | User Label | HTTP Status Code | Path Type |
|---|---|---|---|---|
| shape35 | Success Response | Success Response | 200 | Success |
| shape36 | Error Response | Error Response | 400 | Error (HTTP failure) |
| shape43 | Error Response | Error Response | 500 | Error (Try/Catch exception) |
| shape48 | Error Response | Error Response | 400 | Error (GZIP decompression failure) |

### Return Path 1: Success Response (shape35)

**Return Label:** "Success Response"  
**Return Shape ID:** shape35  
**HTTP Status Code:** 200  
**Path Type:** Success

**Decision Conditions Leading to Return:**
- Decision shape2: meta.base.applicationstatuscode matches "20*" → TRUE path

**Populated Response Fields:**

| Field Name | Field Path | Source | Populated By | Value Type |
|---|---|---|---|---|
| status | leaveResponse/Object/status | map_default | Map e4fd3f59 | Static: "success" |
| message | leaveResponse/Object/message | map_default | Map e4fd3f59 | Static: "Data successfully sent to Oracle Fusion" |
| personAbsenceEntryId | leaveResponse/Object/personAbsenceEntryId | operation_response | Operation 6e8920fd (Oracle HCM API) | Dynamic from Oracle response |
| success | leaveResponse/Object/success | map_default | Map e4fd3f59 | Static: "true" |

**Response JSON Example:**

```json
{
  "status": "success",
  "message": "Data successfully sent to Oracle Fusion",
  "personAbsenceEntryId": 300100123456789,
  "success": "true"
}
```

### Return Path 2: Error Response - HTTP Failure (shape36)

**Return Label:** "Error Response"  
**Return Shape ID:** shape36  
**HTTP Status Code:** 400  
**Path Type:** Error

**Decision Conditions Leading to Return:**
- Decision shape2: meta.base.applicationstatuscode does NOT match "20*" → FALSE path
- Decision shape44: dynamicdocument.DDP_RespHeader does NOT equal "gzip" → FALSE path

**Populated Response Fields:**

| Field Name | Field Path | Source | Populated By | Value Type |
|---|---|---|---|---|
| status | leaveResponse/Object/status | map_default | Map f46b845a | Static: "failure" |
| message | leaveResponse/Object/message | process_property | DPP_ErrorMessage (from track property) | Dynamic from meta.base.applicationstatusmessage |
| success | leaveResponse/Object/success | map_default | Map f46b845a | Static: "false" |

**Response JSON Example:**

```json
{
  "status": "failure",
  "message": "HTTP 400: Bad Request - Invalid absence type",
  "success": "false"
}
```

### Return Path 3: Error Response - Try/Catch Exception (shape43)

**Return Label:** "Error Response"  
**Return Shape ID:** shape43  
**HTTP Status Code:** 500  
**Path Type:** Error

**Decision Conditions Leading to Return:**
- Try/Catch shape17: Exception caught in Try block → Catch path (shape20 branch path 2)

**Populated Response Fields:**

| Field Name | Field Path | Source | Populated By | Value Type |
|---|---|---|---|---|
| status | leaveResponse/Object/status | map_default | Map f46b845a | Static: "failure" |
| message | leaveResponse/Object/message | process_property | DPP_ErrorMessage (from catch error) | Dynamic from meta.base.catcherrorsmessage |
| success | leaveResponse/Object/success | map_default | Map f46b845a | Static: "false" |

**Response JSON Example:**

```json
{
  "status": "failure",
  "message": "Connection timeout to Oracle Fusion HCM API",
  "success": "false"
}
```

### Return Path 4: Error Response - GZIP Decompression Failure (shape48)

**Return Label:** "Error Response"  
**Return Shape ID:** shape48  
**HTTP Status Code:** 400  
**Path Type:** Error

**Decision Conditions Leading to Return:**
- Decision shape2: meta.base.applicationstatuscode matches "20*" → TRUE path
- Decision shape44: dynamicdocument.DDP_RespHeader equals "gzip" → TRUE path
- Data process shape45: GZIP decompression executed
- Document properties shape46: Error message captured from meta.base.applicationstatusmessage

**Populated Response Fields:**

| Field Name | Field Path | Source | Populated By | Value Type |
|---|---|---|---|---|
| status | leaveResponse/Object/status | map_default | Map f46b845a | Static: "failure" |
| message | leaveResponse/Object/message | process_property | DPP_ErrorMessage (from decompression error) | Dynamic from meta.base.applicationstatusmessage |
| success | leaveResponse/Object/success | map_default | Map f46b845a | Static: "false" |

**Response JSON Example:**

```json
{
  "status": "failure",
  "message": "Failed to decompress GZIP response from Oracle Fusion",
  "success": "false"
}
```

### Downstream Operations HTTP Status Codes

#### Operation: Leave Oracle Fusion Create (6e8920fd-af5a-430b-a1d9-9fde7ac29a12)

**Expected Success Codes:** 200, 201  
**Error Status Codes:** 400, 401, 403, 404, 500, 502, 503, 504  
**Error Handling Strategy:** Return error response to caller with error message

**Success Scenario (HTTP 20*):**
- Status Code: 200 or 201
- Response may be GZIP compressed (Content-Encoding: gzip)
- Response body contains Oracle HCM absence entry details

**Error Scenarios:**
- **HTTP 400:** Bad Request - Invalid input data
- **HTTP 401:** Unauthorized - Authentication failure
- **HTTP 403:** Forbidden - Insufficient permissions
- **HTTP 404:** Not Found - Resource not found
- **HTTP 500:** Internal Server Error - Oracle HCM system error
- **HTTP 502/503/504:** Gateway/Service errors

---

## 7. Process Properties Analysis (Steps 2-3)

### Property WRITES

| Property Name | Property ID | Written By Shape(s) | Source | Value Type |
|---|---|---|---|---|
| DPP_Process_Name | process.DPP_Process_Name | shape38 | Execution property | Process Name |
| DPP_AtomName | process.DPP_AtomName | shape38 | Execution property | Atom Name |
| DPP_Payload | process.DPP_Payload | shape38 | Current document | Request payload |
| DPP_ExecutionID | process.DPP_ExecutionID | shape38 | Execution property | Execution Id |
| DPP_File_Name | process.DPP_File_Name | shape38 | Concatenation | ProcessName + Timestamp + ".txt" |
| DPP_Subject | process.DPP_Subject | shape38 | Concatenation | AtomName + " (" + ProcessName + " ) has errors to report" |
| To_Email | process.To_Email | shape38 | Defined parameter | Email recipient address |
| DPP_HasAttachment | process.DPP_HasAttachment | shape38 | Defined parameter | "Y" or "N" |
| DPP_ErrorMessage | process.DPP_ErrorMessage | shape19, shape39, shape46 | Track property | Error message from catch/response |
| dynamicdocument.URL | dynamicdocument.URL | shape8 | Defined parameter | Oracle Fusion API resource path |

### Property READS

| Property Name | Property ID | Read By Shape(s) | Usage Context |
|---|---|---|---|
| dynamicdocument.URL | dynamicdocument.URL | shape33 (operation) | HTTP request path element |
| DPP_ErrorMessage | process.DPP_ErrorMessage | Subprocess shape21 (via map f46b845a) | Error message in response |
| DPP_Process_Name | process.DPP_Process_Name | Subprocess shape21 | Email body content |
| DPP_AtomName | process.DPP_AtomName | Subprocess shape21 | Email body content |
| DPP_ExecutionID | process.DPP_ExecutionID | Subprocess shape21 | Email body content |
| DPP_File_Name | process.DPP_File_Name | Subprocess shape21 | Email attachment filename |
| DPP_Subject | process.DPP_Subject | Subprocess shape21 | Email subject |
| To_Email | process.To_Email | Subprocess shape21 | Email recipient |
| DPP_HasAttachment | process.DPP_HasAttachment | Subprocess shape21 (decision shape4) | Attachment decision |
| DPP_MailBody | process.DPP_MailBody | Subprocess shape21 (shape6, shape20) | Email body content |
| DPP_Payload | process.DPP_Payload | Subprocess shape21 (shape15) | Email attachment content |

### Property Dependency Chains

**Chain 1: Error Handling Properties**
- shape38 WRITES DPP_Process_Name, DPP_AtomName, DPP_ExecutionID, DPP_Payload, DPP_File_Name, DPP_Subject, To_Email, DPP_HasAttachment
- shape19/shape39/shape46 WRITE DPP_ErrorMessage
- Subprocess shape21 READS all these properties
- **Dependency:** shape38 and error shapes MUST execute BEFORE subprocess shape21

**Chain 2: URL Configuration**
- shape8 WRITES dynamicdocument.URL
- shape33 (operation) READS dynamicdocument.URL
- **Dependency:** shape8 MUST execute BEFORE shape33

---

## 8. Data Dependency Graph (Step 4)

### Dependency Relationships

**Dependency 1: URL Configuration → HTTP Operation**
- **Writer:** shape8 (set URL)
- **Property:** dynamicdocument.URL
- **Reader:** shape33 (Leave Oracle Fusion Create operation)
- **Reasoning:** HTTP operation requires URL to be set before execution
- **Execution Order:** shape8 → shape33

**Dependency 2: HTTP Operation → Status Check Decision**
- **Writer:** shape33 (operation response - automatic)
- **Property:** meta.base.applicationstatuscode (track property)
- **Reader:** shape2 (HTTP Status 20 check decision)
- **Reasoning:** Decision checks HTTP status code from operation response
- **Execution Order:** shape33 → shape2

**Dependency 3: HTTP Operation → Content-Encoding Check Decision**
- **Writer:** shape33 (operation response header - automatic)
- **Property:** dynamicdocument.DDP_RespHeader
- **Reader:** shape44 (Check Response Content Type decision)
- **Reasoning:** Decision checks Content-Encoding header from operation response
- **Execution Order:** shape33 → shape44

**Dependency 4: Error Capture → Error Response**
- **Writer:** shape19, shape39, shape46 (error message capture)
- **Property:** process.DPP_ErrorMessage
- **Reader:** Map f46b845a (Leave Error Map)
- **Reasoning:** Error map uses DPP_ErrorMessage to populate response message
- **Execution Order:** shape19/shape39/shape46 → map f46b845a

**Dependency 5: Initial Properties → Subprocess**
- **Writer:** shape38 (Input_details)
- **Properties:** DPP_Process_Name, DPP_AtomName, DPP_Payload, DPP_ExecutionID, DPP_File_Name, DPP_Subject, To_Email, DPP_HasAttachment
- **Reader:** Subprocess shape21 (Office 365 Email)
- **Reasoning:** Subprocess requires these properties for email notification
- **Execution Order:** shape38 → shape21 (if error path triggered)

### Dependency Graph Visualization

```
shape38 (Input_details)
 ├─→ WRITES: DPP_Process_Name, DPP_AtomName, DPP_Payload, DPP_ExecutionID, DPP_File_Name, DPP_Subject, To_Email, DPP_HasAttachment
 └─→ Required by: Subprocess shape21 (if error occurs)

shape8 (set URL)
 ├─→ WRITES: dynamicdocument.URL
 └─→ Required by: shape33 (HTTP operation)

shape33 (Leave Oracle Fusion Create - HTTP operation)
 ├─→ WRITES: meta.base.applicationstatuscode (automatic - track property)
 ├─→ WRITES: dynamicdocument.DDP_RespHeader (automatic - response header)
 ├─→ Required by: shape2 (HTTP Status 20 check)
 └─→ Required by: shape44 (Check Response Content Type)

shape19/shape39/shape46 (ErrorMsg capture)
 ├─→ WRITES: process.DPP_ErrorMessage
 └─→ Required by: Map f46b845a (error response)
```

### Independent Operations

- **Subprocess shape21** - Independent (only executes on error paths, uses properties written earlier)
- **Map operations** - Dependent on data availability

### Critical Dependency Chains

**Chain 1: Happy Path**
```
shape38 → shape8 → shape33 → shape2 → shape34 → shape35
(Input properties) → (URL setup) → (HTTP call) → (Status check) → (Success map) → (Success return)
```

**Chain 2: HTTP Error Path**
```
shape38 → shape8 → shape33 → shape2 → shape44 → shape39 → shape40 → shape36
(Input properties) → (URL setup) → (HTTP call) → (Status check FALSE) → (Content check) → (Error capture) → (Error map) → (Error return)
```

**Chain 3: Try/Catch Error Path**
```
shape38 → shape17 (Try) → [Exception] → shape20 (branch) → shape19 → shape21 (subprocess) → shape43
(Input properties) → (Try/Catch) → (Error) → (Branch) → (Error capture) → (Email notification) → (Error return)
```

---

## 9. Control Flow Graph (Step 5)

### Control Flow Connections (Main Process)

**Total Shapes:** 14 shapes in main process  
**Total Connections:** 17 dragpoint connections

| From Shape | Shape Type | To Shape | Identifier | Text | Purpose |
|---|---|---|---|---|---|
| shape1 | start | shape38 | default | - | Entry point to input properties |
| shape38 | documentproperties | shape17 | default | - | Input properties to Try/Catch |
| shape17 | catcherrors | shape29 | default | Try | Try path - normal flow |
| shape17 | catcherrors | shape20 | error | Catch | Catch path - error handling |
| shape29 | map | shape8 | default | - | Map to URL setup |
| shape8 | documentproperties | shape49 | default | - | URL setup to notify |
| shape49 | notify | shape33 | default | - | Notify to HTTP operation |
| shape33 | connectoraction | shape2 | default | - | HTTP operation to status check |
| shape2 | decision | shape34 | true | True | HTTP 20* success path |
| shape2 | decision | shape44 | false | False | HTTP non-20* error path |
| shape34 | map | shape35 | default | - | Success map to success return |
| shape44 | decision | shape45 | true | True | GZIP detected path |
| shape44 | decision | shape39 | false | False | Non-GZIP error path |
| shape45 | dataprocess | shape46 | default | - | GZIP decompress to error capture |
| shape46 | documentproperties | shape47 | default | - | Error capture to error map |
| shape47 | map | shape48 | default | - | Error map to error return |
| shape39 | documentproperties | shape40 | default | - | Error capture to error map |
| shape40 | map | shape36 | default | - | Error map to error return |
| shape20 | branch | shape19 | 1 | 1 | Branch path 1 - error message |
| shape20 | branch | shape41 | 2 | 2 | Branch path 2 - error response |
| shape19 | documentproperties | shape21 | default | - | Error message to subprocess |
| shape41 | map | shape43 | default | - | Error map to error return |

### Control Flow Connections (Subprocess a85945c5)

**Total Shapes:** 10 shapes in subprocess  
**Total Connections:** 9 dragpoint connections

| From Shape | Shape Type | To Shape | Identifier | Text | Purpose |
|---|---|---|---|---|---|
| shape1 | start | shape2 | default | - | Entry point to Try/Catch |
| shape2 | catcherrors | shape4 | default | Try | Try path - normal flow |
| shape2 | catcherrors | shape10 | error | Catch | Catch path - throw exception |
| shape4 | decision | shape11 | true | True | Has attachment = Y |
| shape4 | decision | shape23 | false | False | Has attachment = N |
| shape11 | message | shape14 | default | - | Mail body to set mail body property |
| shape14 | documentproperties | shape15 | default | - | Set mail body to payload message |
| shape15 | message | shape6 | default | - | Payload to mail properties |
| shape6 | documentproperties | shape3 | default | - | Mail properties to email operation |
| shape3 | connectoraction | shape5 | default | - | Email operation to stop |
| shape23 | message | shape22 | default | - | Mail body to set mail body property |
| shape22 | documentproperties | shape20 | default | - | Set mail body to mail properties |
| shape20 | documentproperties | shape7 | default | - | Mail properties to email operation |
| shape7 | connectoraction | shape9 | default | - | Email operation to stop |

### Reverse Flow Mapping (Step 6)

**Convergence Points Identified:**

**Main Process:**
- **No convergence points** - All paths lead to terminal shapes (Return Documents)

**Subprocess:**
- **No convergence points** - Decision paths lead to separate email operations, both terminate at Stop shapes

### Incoming Connections Analysis

| Target Shape | Incoming From | Count | Convergence? |
|---|---|---|---|
| shape38 | shape1 | 1 | No |
| shape17 | shape38 | 1 | No |
| shape29 | shape17 | 1 | No |
| shape8 | shape29 | 1 | No |
| shape49 | shape8 | 1 | No |
| shape33 | shape49 | 1 | No |
| shape2 | shape33 | 1 | No |
| shape34 | shape2 | 1 | No |
| shape44 | shape2 | 1 | No |
| shape35 | shape34 | 1 | No (terminal) |
| shape36 | shape40 | 1 | No (terminal) |
| shape43 | shape41 | 1 | No (terminal) |
| shape48 | shape47 | 1 | No (terminal) |
| shape19 | shape20 | 1 | No |
| shape41 | shape20 | 1 | No |
| shape20 | shape17 | 1 | No |
| shape21 | shape19 | 1 | No (terminal - subprocess) |

**Conclusion:** No convergence points detected. All decision/branch paths lead to independent terminal shapes.

---

## 10. Decision Shape Analysis (Step 7)

### Decision Data Source Analysis

✅ **Decision data sources identified: YES**

✅ **Decision types classified: YES**

✅ **Execution order verified: YES**

✅ **All decision paths traced: YES**

✅ **Decision patterns identified: YES**

✅ **Paths traced to termination: YES**

### Decision Inventory

#### Decision 1: shape2 - HTTP Status 20 check

**Shape ID:** shape2  
**User Label:** "HTTP Status 20 check"  
**Comparison Type:** wildcard  
**Location:** Main process (after HTTP operation)

**Decision Values:**
- **Value 1:** meta.base.applicationstatuscode (track property)
  - Type: track
  - Property: Base - Application Status Code
- **Value 2:** "20*" (static)
  - Type: static
  - Pattern: Wildcard match for HTTP 200-299 status codes

**Data Source:** TRACK_PROPERTY (from operation response)  
**Data Source Detail:** HTTP status code from shape33 (Leave Oracle Fusion Create operation)  
**Decision Type:** POST-OPERATION  
**Reasoning:** Checks response data from HTTP operation, must execute after operation

**Actual Execution Order:** 
```
shape33 (HTTP Operation) → Response (HTTP status code) → shape2 (Decision) → Route based on status
```

**TRUE Path (HTTP 20*):**
- **Destination:** shape34 (Oracle Fusion Leave Response Map)
- **Termination:** shape35 (Success Response - Return Documents)
- **Type:** Success path
- **HTTP Status:** 200

**FALSE Path (HTTP non-20*):**
- **Destination:** shape44 (Check Response Content Type decision)
- **Termination:** shape36 or shape48 (Error Response - Return Documents)
- **Type:** Error path
- **HTTP Status:** 400

**Pattern:** Error Check (Success vs Failure based on HTTP status code)

**Convergence:** No convergence - paths lead to different terminal shapes

**Early Exit:** Both paths terminate (TRUE → Success return, FALSE → Error return)

**Business Logic:**
- If HTTP status is 20* (success) → Map Oracle response to D365 format → Return success
- If HTTP status is not 20* (error) → Check if response is GZIP compressed → Extract error → Return error

#### Decision 2: shape44 - Check Response Content Type

**Shape ID:** shape44  
**User Label:** "Check Response Content Type"  
**Comparison Type:** equals  
**Location:** Main process (FALSE path of shape2)

**Decision Values:**
- **Value 1:** dynamicdocument.DDP_RespHeader (track property)
  - Type: track
  - Property: Dynamic Document Property - DDP_RespHeader
  - Source: Content-Encoding HTTP response header
- **Value 2:** "gzip" (static)
  - Type: static
  - Value: "gzip"

**Data Source:** TRACK_PROPERTY (from operation response header)  
**Data Source Detail:** Content-Encoding header from shape33 (Leave Oracle Fusion Create operation)  
**Decision Type:** POST-OPERATION  
**Reasoning:** Checks response header from HTTP operation, must execute after operation

**Actual Execution Order:** 
```
shape33 (HTTP Operation) → Response (Content-Encoding header) → shape44 (Decision) → Route based on encoding
```

**TRUE Path (Content-Encoding = gzip):**
- **Destination:** shape45 (GZIP decompression data process)
- **Termination:** shape48 (Error Response - Return Documents)
- **Type:** GZIP decompression path
- **HTTP Status:** 400

**FALSE Path (Content-Encoding ≠ gzip):**
- **Destination:** shape39 (error msg - capture error message)
- **Termination:** shape36 (Error Response - Return Documents)
- **Type:** Direct error path
- **HTTP Status:** 400

**Pattern:** Conditional Logic (GZIP decompression vs direct error)

**Convergence:** No convergence - paths lead to different terminal shapes

**Early Exit:** Both paths terminate with error returns

**Business Logic:**
- If response is GZIP compressed → Decompress → Extract error message → Return error
- If response is not GZIP → Extract error message directly → Return error

#### Decision 3: shape4 - Attachment_Check (Subprocess)

**Shape ID:** shape4 (in subprocess a85945c5)  
**User Label:** "Attachment_Check"  
**Comparison Type:** equals  
**Location:** Subprocess (Office 365 Email)

**Decision Values:**
- **Value 1:** DPP_HasAttachment (process property)
  - Type: process
  - Property: process.DPP_HasAttachment
- **Value 2:** "Y" (static)
  - Type: static
  - Value: "Y"

**Data Source:** PROCESS_PROPERTY (from main process)  
**Data Source Detail:** DPP_HasAttachment written by shape38 in main process  
**Decision Type:** PRE-FILTER  
**Reasoning:** Checks input parameter to determine email operation type

**Actual Execution Order:** 
```
Main process shape38 writes DPP_HasAttachment → Subprocess reads property → shape4 (Decision) → Route to appropriate email operation
```

**TRUE Path (Has attachment = Y):**
- **Destination:** shape11 (Mail_Body message with attachment)
- **Termination:** shape5 (Stop with continue=true)
- **Type:** Email with attachment path
- **Operations:** shape3 (Email w Attachment operation)

**FALSE Path (Has attachment ≠ Y):**
- **Destination:** shape23 (Mail_Body message without attachment)
- **Termination:** shape9 (Stop with continue=true)
- **Type:** Email without attachment path
- **Operations:** shape7 (Email W/O Attachment operation)

**Pattern:** Conditional Logic (Optional Processing - attachment vs no attachment)

**Convergence:** No convergence - paths lead to separate Stop shapes (both continue=true)

**Early Exit:** No early exit - both paths complete successfully

**Business Logic:**
- If attachment required (Y) → Build email with attachment → Send via shape3
- If no attachment (N) → Build email without attachment → Send via shape7

### Decision Pattern Summary

| Decision | Pattern Type | Data Source | Execution Type | Early Exit? |
|---|---|---|---|---|
| shape2 | Error Check | TRACK_PROPERTY | POST-OPERATION | Yes (both paths terminate) |
| shape44 | Conditional Logic | TRACK_PROPERTY | POST-OPERATION | Yes (both paths terminate) |
| shape4 | Conditional Logic | PROCESS_PROPERTY | PRE-FILTER | No (both paths continue) |

---

## 11. Branch Shape Analysis (Step 8)

### Branch Shape Inventory

**Total Branch Shapes:** 1 (shape20 in main process)

✅ **Classification completed: YES**

✅ **Assumption check: NO (analyzed dependencies)**

✅ **Properties extracted: YES**

✅ **Dependency graph built: YES**

✅ **Topological sort applied: N/A (no dependencies between paths)**

### Branch: shape20 (Error Handler Branch)

**Shape ID:** shape20  
**User Label:** (none)  
**Location:** Main process - Catch path of Try/Catch shape17  
**Number of Paths:** 2

**Path 1 (identifier "1"):**
- **Start Shape:** shape19 (ErrorMsg - document properties)
- **Path Sequence:** shape19 → shape21 (subprocess)
- **Terminal:** shape21 (subprocess - no explicit return, subprocess handles termination)

**Path 2 (identifier "2"):**
- **Start Shape:** shape41 (map)
- **Path Sequence:** shape41 → shape43 (Return Documents)
- **Terminal:** shape43 (Error Response - Return Documents)

### Step 1: Properties Analysis

**Path 1 Properties:**
- **READS:**
  - process.DPP_ErrorMessage (read by subprocess via map)
  - process.DPP_Process_Name (read by subprocess)
  - process.DPP_AtomName (read by subprocess)
  - process.DPP_ExecutionID (read by subprocess)
  - process.DPP_File_Name (read by subprocess)
  - process.DPP_Subject (read by subprocess)
  - process.To_Email (read by subprocess)
  - process.DPP_HasAttachment (read by subprocess)
  - process.DPP_Payload (read by subprocess)
  - process.DPP_MailBody (read by subprocess)
- **WRITES:**
  - process.DPP_ErrorMessage (written by shape19)

**Path 2 Properties:**
- **READS:**
  - process.DPP_ErrorMessage (read by map f46b845a)
- **WRITES:**
  - None

### Step 2: Dependency Graph Between Paths

**Path Dependencies:**
- Path 2 reads process.DPP_ErrorMessage
- Path 1 writes process.DPP_ErrorMessage (via shape19)
- **Dependency:** Path 1 MUST execute BEFORE Path 2

**Dependency Proof:**
```
Path 1 (shape19) WRITES process.DPP_ErrorMessage
Path 2 (shape41 → map f46b845a) READS process.DPP_ErrorMessage
Therefore: Path 1 → Path 2 (sequential execution)
```

### Step 3: Classification

**Classification:** SEQUENTIAL

**Reasoning:**
1. Path 2 depends on Path 1 (reads property written by Path 1)
2. Path 1 contains subprocess call (shape21) which is an operation
3. **CRITICAL:** Branch does NOT contain API calls in Path 2, but has data dependency
4. Data dependency requires sequential execution

**API Call Detection:**
- Path 1: Contains subprocess shape21 (which internally has email operations)
- Path 2: No API calls (only map and return)
- **Result:** Data dependency enforces sequential execution

### Step 4: Topological Sort Order

**Dependency Order:** Path 1 → Path 2

**Topological Sort:**
1. Path 1 has no incoming dependencies (writes DPP_ErrorMessage)
2. Path 2 depends on Path 1 (reads DPP_ErrorMessage)
3. Sorted order: [Path 1, Path 2]

**Execution Sequence:**
```
1. Execute Path 1: shape19 → shape21 (subprocess)
2. Execute Path 2: shape41 → shape43 (error return)
```

### Step 5: Path Termination

**Path 1 Termination:**
- **Terminal Shape:** shape21 (subprocess call)
- **Terminal Type:** Subprocess (no explicit return in main process)
- **Subprocess Termination:** Stop shapes (shape5, shape9) with continue=true

**Path 2 Termination:**
- **Terminal Shape:** shape43 (Return Documents)
- **Terminal Type:** Return Documents (Error Response)

### Step 6: Convergence Points

**Convergence Analysis:**
- Path 1 terminates at subprocess (no return to main process after subprocess)
- Path 2 terminates at Return Documents
- **Convergence:** None - paths do not rejoin

### Step 7: Execution Continuation

**Execution Continues From:** None

**Reasoning:**
- Path 1 calls subprocess which terminates with Stop (continue=true), but main process does not continue after subprocess in this error path
- Path 2 terminates with Return Documents
- Both paths are terminal error handlers

### Step 8: Complete Branch Analysis Summary

**Branch shape20 Analysis:**
- **Classification:** SEQUENTIAL
- **Dependency Order:** Path 1 → Path 2
- **Path 1:** Error message capture → Email notification (subprocess)
- **Path 2:** Error response mapping → Return error
- **Convergence:** None
- **Continuation:** None (both paths terminate)
- **Purpose:** Error handling with dual response (email notification + error return)

**Critical Note:** This branch is unusual - Path 1 calls subprocess for email notification, Path 2 returns error response. Both paths execute sequentially, suggesting Path 1 sends notification, then Path 2 returns error to caller.

---

## 12. Execution Order (Step 9)

### Business Logic Verification (Step 0 - MANDATORY FIRST)

✅ **Business logic verified FIRST: YES**

✅ **Operation analysis complete: YES**

✅ **Business logic execution order identified: YES**

✅ **Data dependencies checked FIRST: YES**

✅ **Operation response analysis used: YES** (Reference: Section 4 - Operation Response Analysis)

✅ **Decision analysis used: YES** (Reference: Section 10 - Decision Shape Analysis)

✅ **Dependency graph used: YES** (Reference: Section 8 - Data Dependency Graph)

✅ **Branch analysis used: YES** (Reference: Section 11 - Branch Shape Analysis)

✅ **Property dependency verification: YES** (All reads happen after writes)

✅ **Topological sort applied: YES** (For branch shape20)

### Business Logic Flow Analysis

#### Operation 1: shape33 (Leave Oracle Fusion Create)

**Purpose:** Create leave/absence entry in Oracle Fusion HCM system

**What it does:**
- Sends HTTP POST request to Oracle Fusion HCM REST API
- Endpoint: hcmRestApi/resources/11.13.18.05/absences
- Request body: Transformed leave data (personNumber, absenceType, dates, durations, etc.)
- Authentication: Basic Auth (INTEGRATION.USER@al-ghurair.com)

**What it produces:**
- HTTP status code (meta.base.applicationstatuscode)
- Response header Content-Encoding (dynamicdocument.DDP_RespHeader)
- Response body (Oracle HCM absence entry details)

**Dependent Operations:**
- Decision shape2 (checks HTTP status code)
- Decision shape44 (checks Content-Encoding header)

**Business Flow Position:** MUST execute FIRST (produces data needed by all subsequent decisions)

**Proof:** 
- shape2 reads meta.base.applicationstatuscode → shape33 writes this (automatic track property)
- shape44 reads dynamicdocument.DDP_RespHeader → shape33 writes this (response header mapping)
- Therefore: shape33 MUST execute BEFORE shape2 and shape44

#### Operation 2: Subprocess shape21 (Office 365 Email)

**Purpose:** Send error notification email to integration team

**What it does:**
- Sends email via Office 365 SMTP
- Email contains: Process name, environment, execution ID, error details
- Conditional: With or without attachment based on DPP_HasAttachment

**What it produces:**
- Email sent to recipients
- No data returned to main process (Stop with continue=true)

**Dependent Operations:**
- None (terminal operation in error path)

**Business Flow Position:** Executes ONLY on error path (Try/Catch exception)

**Proof:**
- Subprocess is called from shape21 in branch path 1 (error handler)
- Subprocess reads properties written by shape38 (DPP_Process_Name, DPP_AtomName, etc.)
- Subprocess terminates with Stop (continue=true) - no return to main process

### Operations That MUST Execute First

1. **shape38 (Input_details)** - MUST execute FIRST
   - Writes ALL process properties needed by subprocess and operations
   - No dependencies on other operations

2. **shape8 (set URL)** - MUST execute BEFORE shape33
   - Writes dynamicdocument.URL needed by HTTP operation

3. **shape33 (Leave Oracle Fusion Create)** - MUST execute BEFORE decisions
   - Produces HTTP status code and response headers needed by shape2 and shape44

### Execution Order Derivation

**Based on dependency graph (Section 8), decision analysis (Section 10), and branch analysis (Section 11):**

#### Main Flow (Happy Path)

```
1. shape1 (START)
2. shape38 (Input_details) - WRITES: DPP_Process_Name, DPP_AtomName, DPP_Payload, DPP_ExecutionID, DPP_File_Name, DPP_Subject, To_Email, DPP_HasAttachment
3. shape17 (Try/Catch) - TRY path
4. shape29 (Leave Create Map) - Transform D365 → Oracle HCM format
5. shape8 (set URL) - WRITES: dynamicdocument.URL
6. shape49 (notify) - Log request payload
7. shape33 (Leave Oracle Fusion Create - HTTP POST) - WRITES: meta.base.applicationstatuscode, dynamicdocument.DDP_RespHeader
8. shape2 (HTTP Status 20 check) - READS: meta.base.applicationstatuscode
   - IF TRUE (HTTP 20*):
     9a. shape34 (Oracle Fusion Leave Response Map)
     10a. shape35 (Success Response - Return Documents) [HTTP 200] [SUCCESS]
   - IF FALSE (HTTP non-20*):
     9b. shape44 (Check Response Content Type) - READS: dynamicdocument.DDP_RespHeader
       - IF TRUE (gzip):
         10b. shape45 (GZIP decompression)
         11b. shape46 (error msg) - WRITES: DPP_ErrorMessage
         12b. shape47 (Leave Error Map) - READS: DPP_ErrorMessage
         13b. shape48 (Error Response - Return Documents) [HTTP 400] [ERROR]
       - IF FALSE (not gzip):
         10c. shape39 (error msg) - WRITES: DPP_ErrorMessage
         11c. shape40 (Leave Error Map) - READS: DPP_ErrorMessage
         12c. shape36 (Error Response - Return Documents) [HTTP 400] [ERROR]
```

#### Error Flow (Try/Catch Exception)

```
1. shape1 (START)
2. shape38 (Input_details) - WRITES: DPP_Process_Name, DPP_AtomName, DPP_Payload, DPP_ExecutionID, DPP_File_Name, DPP_Subject, To_Email, DPP_HasAttachment
3. shape17 (Try/Catch) - CATCH path (exception occurred)
4. shape20 (Branch - 2 paths) - SEQUENTIAL execution
   - Path 1 (identifier "1"):
     5a. shape19 (ErrorMsg) - WRITES: DPP_ErrorMessage (from meta.base.catcherrorsmessage)
     6a. shape21 (Subprocess: Office 365 Email) - READS: All DPP properties
         SUBPROCESS INTERNAL FLOW:
         - shape1 (START)
         - shape2 (Try/Catch)
         - shape4 (Attachment_Check) - READS: DPP_HasAttachment
           - IF TRUE (Y):
             - shape11 (Mail_Body with attachment)
             - shape14 (set_MailBody) - WRITES: DPP_MailBody
             - shape15 (payload) - READS: DPP_Payload
             - shape6 (set_Mail_Properties) - READS: To_Email, DPP_Subject, DPP_MailBody, DPP_File_Name
             - shape3 (Email w Attachment)
             - shape5 (Stop continue=true)
           - IF FALSE (N):
             - shape23 (Mail_Body without attachment)
             - shape22 (set_MailBody) - WRITES: DPP_MailBody
             - shape20 (set_Mail_Properties) - READS: To_Email, DPP_Subject, DPP_MailBody
             - shape7 (Email W/O Attachment)
             - shape9 (Stop continue=true)
   - Path 2 (identifier "2"):
     5b. shape41 (Leave Error Map) - READS: DPP_ErrorMessage
     6b. shape43 (Error Response - Return Documents) [HTTP 500] [ERROR]
```

### Dependency Verification

**Reference to Step 4 (Data Dependency Graph):**

1. **shape8 → shape33:**
   - shape33 reads dynamicdocument.URL
   - shape8 writes dynamicdocument.URL
   - ✅ Verified: shape8 executes BEFORE shape33

2. **shape33 → shape2:**
   - shape2 reads meta.base.applicationstatuscode
   - shape33 writes meta.base.applicationstatuscode (automatic)
   - ✅ Verified: shape33 executes BEFORE shape2

3. **shape33 → shape44:**
   - shape44 reads dynamicdocument.DDP_RespHeader
   - shape33 writes dynamicdocument.DDP_RespHeader (automatic)
   - ✅ Verified: shape33 executes BEFORE shape44

4. **shape19 → shape41 (branch path dependency):**
   - shape41 (map) reads process.DPP_ErrorMessage
   - shape19 writes process.DPP_ErrorMessage
   - ✅ Verified: shape19 (Path 1) executes BEFORE shape41 (Path 2)

5. **shape38 → subprocess shape21:**
   - Subprocess reads DPP_Process_Name, DPP_AtomName, DPP_ExecutionID, DPP_Payload, DPP_File_Name, DPP_Subject, To_Email, DPP_HasAttachment
   - shape38 writes all these properties
   - ✅ Verified: shape38 executes BEFORE subprocess shape21

### Branch Execution Order

**Reference to Step 8 (Branch Analysis):**

**Branch shape20:**
- Classification: SEQUENTIAL
- Dependency Order: Path 1 → Path 2
- Reasoning: Path 2 reads DPP_ErrorMessage written by Path 1
- Execution: Path 1 (error notification) → Path 2 (error return)

### Decision Path Execution

**Reference to Step 7 (Decision Analysis):**

**Decision shape2 (HTTP Status 20 check):**
- TRUE path: shape34 → shape35 (success return)
- FALSE path: shape44 → (shape45 → shape46 → shape47 → shape48) OR (shape39 → shape40 → shape36)
- No convergence - paths terminate independently

**Decision shape44 (Check Response Content Type):**
- TRUE path: shape45 → shape46 → shape47 → shape48 (GZIP decompression error)
- FALSE path: shape39 → shape40 → shape36 (direct error)
- No convergence - paths terminate independently

**Decision shape4 (Attachment_Check - subprocess):**
- TRUE path: shape11 → shape14 → shape15 → shape6 → shape3 → shape5 (email with attachment)
- FALSE path: shape23 → shape22 → shape20 → shape7 → shape9 (email without attachment)
- No convergence - paths terminate at separate Stop shapes

### Complete Execution Order

**Main Process Flow:**

```
START (shape1)
 ↓
Input_details (shape38) [WRITES: All DPP properties]
 ↓
Try/Catch (shape17)
 ├─→ TRY PATH:
 |    ↓
 |   Leave Create Map (shape29) [Transform D365 → Oracle HCM]
 |    ↓
 |   set URL (shape8) [WRITES: dynamicdocument.URL]
 |    ↓
 |   notify (shape49) [Log payload]
 |    ↓
 |   Leave Oracle Fusion Create (shape33) [HTTP POST - Downstream]
 |   [WRITES: meta.base.applicationstatuscode, dynamicdocument.DDP_RespHeader]
 |    ↓
 |   HTTP Status 20 check (shape2) [READS: meta.base.applicationstatuscode]
 |    ├─→ IF TRUE (HTTP 20*):
 |    |    ↓
 |    |   Oracle Fusion Leave Response Map (shape34)
 |    |    ↓
 |    |   Success Response (shape35) [HTTP 200] [SUCCESS RETURN]
 |    |
 |    └─→ IF FALSE (HTTP non-20*):
 |         ↓
 |        Check Response Content Type (shape44) [READS: dynamicdocument.DDP_RespHeader]
 |         ├─→ IF TRUE (gzip):
 |         |    ↓
 |         |   GZIP decompression (shape45)
 |         |    ↓
 |         |   error msg (shape46) [WRITES: DPP_ErrorMessage]
 |         |    ↓
 |         |   Leave Error Map (shape47) [READS: DPP_ErrorMessage]
 |         |    ↓
 |         |   Error Response (shape48) [HTTP 400] [ERROR RETURN]
 |         |
 |         └─→ IF FALSE (not gzip):
 |              ↓
 |             error msg (shape39) [WRITES: DPP_ErrorMessage]
 |              ↓
 |             Leave Error Map (shape40) [READS: DPP_ErrorMessage]
 |              ↓
 |             Error Response (shape36) [HTTP 400] [ERROR RETURN]
 |
 └─→ CATCH PATH (Exception):
      ↓
     Branch (shape20) [SEQUENTIAL: Path 1 → Path 2]
      ├─→ Path 1:
      |    ↓
      |   ErrorMsg (shape19) [WRITES: DPP_ErrorMessage from meta.base.catcherrorsmessage]
      |    ↓
      |   Subprocess: Office 365 Email (shape21) [READS: All DPP properties]
      |   [Email notification sent to integration team]
      |
      └─→ Path 2:
           ↓
          Leave Error Map (shape41) [READS: DPP_ErrorMessage]
           ↓
          Error Response (shape43) [HTTP 500] [ERROR RETURN]
```

### Execution Order Summary

**Sequence:**
1. START → Input properties setup
2. Try/Catch wrapper
3. TRY: Map → URL setup → Notify → HTTP operation → Status decision → Success/Error response
4. CATCH: Branch (sequential) → Error notification (subprocess) → Error response

**Critical Dependencies:**
- shape8 MUST execute BEFORE shape33 (URL required for HTTP call)
- shape33 MUST execute BEFORE shape2 (status code required for decision)
- shape33 MUST execute BEFORE shape44 (response header required for decision)
- shape19 MUST execute BEFORE shape41 (error message required for error map)

**Parallel Execution:** None - all operations execute sequentially

**Sequential Execution:** All paths are sequential due to data dependencies and API calls

---

## 13. Sequence Diagram (Step 10)

**Based on:**
- Dependency graph in Step 4 (Section 8)
- Decision analysis in Step 7 (Section 10)
- Control flow graph in Step 5 (Section 9)
- Branch analysis in Step 8 (Section 11)
- Execution order in Step 9 (Section 12)

**📋 NOTE:** Detailed request/response JSON examples are documented in:
- **Section 6: HTTP Status Codes and Return Path Responses** - For response JSON with populated fields for return paths
- **Section 17: Request/Response JSON Examples** - For detailed request/response JSON examples

### Main Process Sequence

```
START (shape1)
 |
 ├─→ Input_details (shape38)
 |    └─→ WRITES: [process.DPP_Process_Name, process.DPP_AtomName, process.DPP_Payload, 
 |                  process.DPP_ExecutionID, process.DPP_File_Name, process.DPP_Subject, 
 |                  process.To_Email, process.DPP_HasAttachment]
 |    └─→ SOURCE: [Execution properties, current document, defined parameters]
 |
 ├─→ Try/Catch (shape17)
 |    |
 |    ├─→ TRY PATH:
 |    |    |
 |    |    ├─→ Leave Create Map (shape29)
 |    |    |    └─→ Transform: D365 format → Oracle HCM format
 |    |    |    └─→ Field mappings: employeeNumber→personNumber, absenceStatusCode→absenceStatusCd, etc.
 |    |    |
 |    |    ├─→ set URL (shape8)
 |    |    |    └─→ WRITES: [dynamicdocument.URL]
 |    |    |    └─→ VALUE: "hcmRestApi/resources/11.13.18.05/absences"
 |    |    |
 |    |    ├─→ notify (shape49)
 |    |    |    └─→ Log request payload (INFO level)
 |    |    |
 |    |    ├─→ Operation: Leave Oracle Fusion Create (shape33) [Downstream - Oracle Fusion HCM]
 |    |    |    └─→ READS: [dynamicdocument.URL]
 |    |    |    └─→ WRITES: [meta.base.applicationstatuscode, dynamicdocument.DDP_RespHeader]
 |    |    |    └─→ HTTP: POST {URL}/hcmRestApi/resources/11.13.18.05/absences
 |    |    |    └─→ AUTH: Basic (INTEGRATION.USER@al-ghurair.com)
 |    |    |    └─→ Expected: [200, 201]
 |    |    |    └─→ Error: [400, 401, 403, 404, 500, 502, 503, 504]
 |    |    |
 |    |    ├─→ Decision: HTTP Status 20 check (shape2)
 |    |    |    └─→ READS: [meta.base.applicationstatuscode]
 |    |    |    └─→ CONDITION: applicationstatuscode matches "20*"?
 |    |    |    |
 |    |    |    ├─→ IF TRUE (HTTP 20* - Success):
 |    |    |    |    |
 |    |    |    |    ├─→ Oracle Fusion Leave Response Map (shape34)
 |    |    |    |    |    └─→ READS: [personAbsenceEntryId from Oracle response]
 |    |    |    |    |    └─→ WRITES: [status="success", message="Data successfully sent to Oracle Fusion", success="true"]
 |    |    |    |    |
 |    |    |    |    └─→ Success Response (shape35) [HTTP 200] [SUCCESS RETURN]
 |    |    |    |         └─→ Response: {"status": "success", "message": "Data successfully sent to Oracle Fusion", 
 |    |    |    |                        "personAbsenceEntryId": 300100123456789, "success": "true"}
 |    |    |    |
 |    |    |    └─→ IF FALSE (HTTP non-20* - Error):
 |    |    |         |
 |    |    |         └─→ Decision: Check Response Content Type (shape44)
 |    |    |              └─→ READS: [dynamicdocument.DDP_RespHeader]
 |    |    |              └─→ CONDITION: DDP_RespHeader equals "gzip"?
 |    |    |              |
 |    |    |              ├─→ IF TRUE (Response is GZIP compressed):
 |    |    |              |    |
 |    |    |              |    ├─→ GZIP decompression (shape45)
 |    |    |              |    |    └─→ Decompress GZIP response using Groovy script
 |    |    |              |    |
 |    |    |              |    ├─→ error msg (shape46)
 |    |    |              |    |    └─→ WRITES: [process.DPP_ErrorMessage]
 |    |    |              |    |    └─→ SOURCE: meta.base.applicationstatusmessage
 |    |    |              |    |
 |    |    |              |    ├─→ Leave Error Map (shape47)
 |    |    |              |    |    └─→ READS: [process.DPP_ErrorMessage]
 |    |    |              |    |    └─→ WRITES: [status="failure", success="false"]
 |    |    |              |    |
 |    |    |              |    └─→ Error Response (shape48) [HTTP 400] [ERROR RETURN]
 |    |    |              |         └─→ Response: {"status": "failure", "message": "<error from Oracle>", "success": "false"}
 |    |    |              |
 |    |    |              └─→ IF FALSE (Response is NOT GZIP):
 |    |    |                   |
 |    |    |                   ├─→ error msg (shape39)
 |    |    |                   |    └─→ WRITES: [process.DPP_ErrorMessage]
 |    |    |                   |    └─→ SOURCE: meta.base.applicationstatusmessage
 |    |    |                   |
 |    |    |                   ├─→ Leave Error Map (shape40)
 |    |    |                   |    └─→ READS: [process.DPP_ErrorMessage]
 |    |    |                   |    └─→ WRITES: [status="failure", success="false"]
 |    |    |                   |
 |    |    |                   └─→ Error Response (shape36) [HTTP 400] [ERROR RETURN]
 |    |    |                        └─→ Response: {"status": "failure", "message": "<error from Oracle>", "success": "false"}
 |    |
 |    └─→ CATCH PATH (Exception in Try block):
 |         |
 |         └─→ Branch (shape20) [SEQUENTIAL: Path 1 → Path 2]
 |              |
 |              ├─→ Path 1 (identifier "1"):
 |              |    |
 |              |    ├─→ ErrorMsg (shape19)
 |              |    |    └─→ WRITES: [process.DPP_ErrorMessage]
 |              |    |    └─→ SOURCE: meta.base.catcherrorsmessage
 |              |    |
 |              |    └─→ Subprocess: Office 365 Email (shape21)
 |              |         └─→ READS: [process.DPP_Process_Name, process.DPP_AtomName, 
 |              |                     process.DPP_ExecutionID, process.DPP_ErrorMessage,
 |              |                     process.DPP_File_Name, process.DPP_Subject, 
 |              |                     process.To_Email, process.DPP_HasAttachment,
 |              |                     process.DPP_Payload]
 |              |         └─→ SUBPROCESS INTERNAL FLOW:
 |              |              |
 |              |              ├─→ START (shape1)
 |              |              |
 |              |              ├─→ Try/Catch (shape2)
 |              |              |    |
 |              |              |    ├─→ TRY PATH:
 |              |              |    |    |
 |              |              |    |    └─→ Decision: Attachment_Check (shape4)
 |              |              |    |         └─→ READS: [process.DPP_HasAttachment]
 |              |              |    |         └─→ CONDITION: DPP_HasAttachment equals "Y"?
 |              |              |    |         |
 |              |              |    |         ├─→ IF TRUE (Has attachment):
 |              |              |    |         |    |
 |              |              |    |         |    ├─→ Mail_Body (shape11)
 |              |              |    |         |    |    └─→ Build HTML email body with execution details
 |              |              |    |         |    |    └─→ READS: [process.DPP_Process_Name, process.DPP_AtomName, 
 |              |              |    |         |    |                process.DPP_ExecutionID, process.DPP_ErrorMessage]
 |              |              |    |         |    |
 |              |              |    |         |    ├─→ set_MailBody (shape14)
 |              |              |    |         |    |    └─→ WRITES: [process.DPP_MailBody]
 |              |              |    |         |    |    └─→ SOURCE: Current document (HTML email body)
 |              |              |    |         |    |
 |              |              |    |         |    ├─→ payload (shape15)
 |              |              |    |         |    |    └─→ READS: [process.DPP_Payload]
 |              |              |    |         |    |    └─→ Set current document to payload (attachment)
 |              |              |    |         |    |
 |              |              |    |         |    ├─→ set_Mail_Properties (shape6)
 |              |              |    |         |    |    └─→ WRITES: [connector.mail.fromAddress, connector.mail.toAddress,
 |              |              |    |         |    |                  connector.mail.subject, connector.mail.body,
 |              |              |    |         |    |                  connector.mail.filename]
 |              |              |    |         |    |    └─→ READS: [process.To_Email, process.DPP_Subject, 
 |              |              |    |         |    |                 process.DPP_MailBody, process.DPP_File_Name]
 |              |              |    |         |    |
 |              |              |    |         |    ├─→ Operation: Email w Attachment (shape3) [Downstream - Office 365]
 |              |              |    |         |    |    └─→ READS: [connector.mail.* properties]
 |              |              |    |         |    |    └─→ Send email with attachment
 |              |              |    |         |    |
 |              |              |    |         |    └─→ Stop (shape5) [continue=true] [SUBPROCESS SUCCESS RETURN]
 |              |              |    |         |
 |              |              |    |         └─→ IF FALSE (No attachment):
 |              |              |    |              |
 |              |              |    |              ├─→ Mail_Body (shape23)
 |              |              |    |              |    └─→ Build HTML email body with execution details
 |              |              |    |              |    └─→ READS: [process.DPP_Process_Name, process.DPP_AtomName,
 |              |              |    |              |                process.DPP_ExecutionID, process.DPP_ErrorMessage]
 |              |              |    |              |
 |              |              |    |              ├─→ set_MailBody (shape22)
 |              |              |    |              |    └─→ WRITES: [process.DPP_MailBody]
 |              |              |    |              |    └─→ SOURCE: Current document (HTML email body)
 |              |              |    |              |
 |              |              |    |              ├─→ set_Mail_Properties (shape20)
 |              |              |    |              |    └─→ WRITES: [connector.mail.fromAddress, connector.mail.toAddress,
 |              |              |    |              |                  connector.mail.subject, connector.mail.body]
 |              |              |    |              |    └─→ READS: [process.To_Email, process.DPP_Subject, process.DPP_MailBody]
 |              |              |    |              |
 |              |              |    |              ├─→ Operation: Email W/O Attachment (shape7) [Downstream - Office 365]
 |              |              |    |              |    └─→ READS: [connector.mail.* properties]
 |              |              |    |              |    └─→ Send email without attachment
 |              |              |    |              |
 |              |              |    |              └─→ Stop (shape9) [continue=true] [SUBPROCESS SUCCESS RETURN]
 |              |              |    |
 |              |              |    └─→ CATCH PATH:
 |              |              |         |
 |              |              |         └─→ Exception (shape10) [SUBPROCESS EXCEPTION]
 |              |              |              └─→ Throw exception with catch error message
 |              |              |
 |              |              └─→ END SUBPROCESS
 |              |
 |              └─→ Path 2 (identifier "2"):
 |                   |
 |                   ├─→ Leave Error Map (shape41)
 |                   |    └─→ READS: [process.DPP_ErrorMessage]
 |                   |    └─→ WRITES: [status="failure", success="false"]
 |                   |
 |                   └─→ Error Response (shape43) [HTTP 500] [ERROR RETURN]
 |                        └─→ Response: {"status": "failure", "message": "<exception message>", "success": "false"}
 |
 └─→ END PROCESS
```

### Critical Flow Patterns

**Pattern 1: Try/Catch Error Handling**
- Try block wraps main business logic (map → HTTP operation → response handling)
- Catch block handles exceptions (connection errors, timeouts, etc.)
- Catch path: Send email notification → Return error response

**Pattern 2: HTTP Status Code Routing**
- Decision shape2 checks HTTP status code from operation response
- TRUE path (20*): Success flow → Return success response
- FALSE path (non-20*): Error flow → Check GZIP → Extract error → Return error response

**Pattern 3: GZIP Response Handling**
- Decision shape44 checks if error response is GZIP compressed
- TRUE path: Decompress GZIP → Extract error → Return error
- FALSE path: Extract error directly → Return error

**Pattern 4: Sequential Branch Error Handling**
- Branch shape20 has 2 sequential paths
- Path 1: Send email notification (subprocess)
- Path 2: Return error response to caller
- Both paths execute sequentially (notification first, then return)

---

## 14. Subprocess Analysis (Step 7a)

### Subprocess: Office 365 Email (a85945c5-3004-42b9-80b1-104f465cd1fb)

**Subprocess ID:** a85945c5-3004-42b9-80b1-104f465cd1fb  
**Subprocess Name:** (Sub) Office 365 Email  
**Called By:** shape21 (main process - error path)  
**Purpose:** Send error notification email via Office 365

### Internal Flow

**Start:** shape1 (start)  
**End:** shape5 or shape9 (Stop with continue=true)

**Flow Sequence:**
```
START (shape1)
 ↓
Try/Catch (shape2)
 ├─→ TRY PATH:
 |    ↓
 |   Decision: Attachment_Check (shape4)
 |    ├─→ IF TRUE (DPP_HasAttachment = "Y"):
 |    |    ↓
 |    |   Mail_Body (shape11) → set_MailBody (shape14) → payload (shape15) → 
 |    |   set_Mail_Properties (shape6) → Email w Attachment (shape3) → Stop (shape5)
 |    |
 |    └─→ IF FALSE (DPP_HasAttachment ≠ "Y"):
 |         ↓
 |        Mail_Body (shape23) → set_MailBody (shape22) → set_Mail_Properties (shape20) → 
 |        Email W/O Attachment (shape7) → Stop (shape9)
 |
 └─→ CATCH PATH:
      ↓
     Exception (shape10) [Throw exception]
```

### Return Paths

**Return Path 1: Success (implicit)**
- **Label:** (implicit success - Stop with continue=true)
- **Shape:** shape5 or shape9 (Stop)
- **Condition:** Email sent successfully
- **Main Process Mapping:** No explicit return path mapping (subprocess terminates successfully)

**Return Path 2: Exception**
- **Label:** (exception thrown)
- **Shape:** shape10 (Exception)
- **Condition:** Error in email sending
- **Main Process Mapping:** Exception propagates to main process

### Properties Written by Subprocess

| Property Name | Written By | Source | Value |
|---|---|---|---|
| process.DPP_MailBody | shape14, shape22 | current document | HTML email body |

### Properties Read by Subprocess (from Main Process)

| Property Name | Read By | Usage |
|---|---|---|
| process.DPP_HasAttachment | shape4 (decision) | Determine email operation type |
| process.DPP_Process_Name | shape11, shape23 (message) | Email body content |
| process.DPP_AtomName | shape11, shape23 (message) | Email body content |
| process.DPP_ExecutionID | shape11, shape23 (message) | Email body content |
| process.DPP_ErrorMessage | shape11, shape23 (message) | Email body content |
| process.DPP_Payload | shape15 (message) | Email attachment content |
| process.To_Email | shape6, shape20 (documentproperties) | Email recipient |
| process.DPP_Subject | shape6, shape20 (documentproperties) | Email subject |
| process.DPP_MailBody | shape6, shape20 (documentproperties) | Email body |
| process.DPP_File_Name | shape6 (documentproperties) | Attachment filename |

### Subprocess Operations

**Operation 1: shape3 (Email w Attachment)**
- **Operation ID:** af07502a-fafd-4976-a691-45d51a33b549
- **Type:** connector-action (mail)
- **Purpose:** Send email with attachment via Office 365
- **Configuration:**
  - Body Content Type: text/html
  - Data Content Type: text/plain
  - Disposition: attachment

**Operation 2: shape7 (Email W/O Attachment)**
- **Operation ID:** 15a72a21-9b57-49a1-a8ed-d70367146644
- **Type:** connector-action (mail)
- **Purpose:** Send email without attachment via Office 365
- **Configuration:**
  - Body Content Type: text/plain
  - Data Content Type: text/html
  - Disposition: inline

### Subprocess Error Handling

**Try/Catch:** shape2 wraps email logic
- **Try Path:** Build email → Send email → Stop (success)
- **Catch Path:** shape10 (Exception) - Throw exception with catch error message

**Exception Message:** "{1}" (parameter 0 = meta.base.catcherrorsmessage)

### Main Process Integration

**Call Site:** shape21 (processcall shape)  
**Wait:** true (main process waits for subprocess to complete)  
**Abort:** true (abort main process if subprocess fails)  
**Return Paths:** None (subprocess terminates with Stop or Exception)

**Integration Flow:**
1. Main process calls subprocess shape21
2. Subprocess executes email notification logic
3. If successful: Stop (continue=true) - subprocess completes, main process continues (but no dragpoint from shape21, so main process also terminates)
4. If error: Exception thrown - main process catches exception (but subprocess is called from Catch path, so exception propagates)

**Critical Note:** Subprocess shape21 has no dragpoints in main process, meaning after subprocess completes, main process does not continue. This is expected for error notification - subprocess sends email, then main process terminates via Path 2 of branch shape20.

---

## 15. Critical Patterns Identified

### Pattern 1: Try/Catch with Dual Error Response

**Identification:**
- Try/Catch shape17 wraps main business logic
- Catch path leads to branch shape20 with 2 sequential paths
- Path 1: Send email notification (subprocess)
- Path 2: Return error response to caller

**Execution Rule:**
- If exception occurs in Try block → Execute Catch path
- Catch path: Sequential execution of Path 1 (notification) then Path 2 (return)
- **CRITICAL:** Path 1 MUST execute BEFORE Path 2 (Path 2 reads error message written by Path 1)

**Business Logic:**
- Notify integration team of error via email
- Return error response to caller
- Both actions occur sequentially

### Pattern 2: HTTP Status Code Decision with GZIP Handling

**Identification:**
- Decision shape2 checks HTTP status code from operation response
- FALSE path (non-20*) leads to another decision shape44
- Decision shape44 checks if error response is GZIP compressed
- TRUE path: Decompress GZIP → Extract error
- FALSE path: Extract error directly

**Execution Rule:**
- HTTP operation MUST execute BEFORE decision shape2
- Decision shape2 routes based on status code
- If error (non-20*) → Check GZIP compression
- Handle GZIP decompression if needed

**Business Logic:**
- Oracle Fusion HCM may return GZIP compressed error responses
- Decompress if needed to extract error message
- Return error message to caller

### Pattern 3: Subprocess with Conditional Email Operations

**Identification:**
- Subprocess has decision shape4 checking DPP_HasAttachment
- TRUE path: Email with attachment (operation shape3)
- FALSE path: Email without attachment (operation shape7)
- Both paths terminate at Stop (continue=true)

**Execution Rule:**
- Decision determines which email operation to use
- Attachment logic: If Y → shape3, else → shape7
- Both operations send email via Office 365 SMTP

**Business Logic:**
- Error notification includes execution details
- Optionally attach payload file
- Email sent to integration team for troubleshooting

### Pattern 4: Property-Based Configuration

**Identification:**
- shape38 writes multiple process properties at start
- Properties used throughout process (URL, email settings, error messages)
- Defined parameters provide configuration values

**Execution Rule:**
- shape38 MUST execute FIRST (writes all configuration properties)
- All subsequent operations depend on these properties

**Business Logic:**
- Centralized property initialization
- Configuration-driven execution (URL, email recipients, etc.)
- Execution metadata captured for troubleshooting

---

## 16. Validation Checklist

### Data Dependencies
- [x] All property WRITES identified (Section 7)
- [x] All property READS identified (Section 7)
- [x] Dependency graph built (Section 8)
- [x] Execution order satisfies all dependencies (Section 12)
- [x] No read-before-write violations detected

### Decision Analysis
- [x] ALL decision shapes inventoried (3 decisions: shape2, shape44, shape4)
- [x] BOTH TRUE and FALSE paths traced to termination (Section 10)
- [x] Pattern type identified for each decision (Section 10)
- [x] Early exits identified and documented (shape2, shape44 - all paths terminate)
- [x] Convergence points identified (None - Section 9)
- [x] Decision data sources identified (Section 10)
- [x] Decision types classified (POST-OPERATION, PRE-FILTER - Section 10)
- [x] Actual execution order verified (Section 10)

### Branch Analysis
- [x] Branch classified as sequential (Section 11)
- [x] API call detection performed (subprocess contains email operations)
- [x] Self-check: Checked for API calls (Answer: YES)
- [x] Self-check: Classified, not assumed (Answer: Classified)
- [x] Dependency order built using topological sort (Section 11)
- [x] Each path traced to terminal point (Section 11)
- [x] Convergence points identified (None)
- [x] Execution continuation point determined (None - paths terminate)

### Sequence Diagram
- [x] Format follows required structure (Section 13)
- [x] Each operation shows READS and WRITES (Section 13)
- [x] Decisions show both TRUE and FALSE paths (Section 13)
- [x] Early exits marked [ERROR RETURN] or [SUCCESS RETURN] (Section 13)
- [x] Subprocess internal flows documented (Section 13)
- [x] Sequence diagram references all prior steps (Section 13)

### Subprocess Analysis
- [x] Subprocess analyzed (internal flow traced - Section 14)
- [x] Return paths identified (Stop with continue=true, Exception - Section 14)
- [x] Properties written by subprocess documented (Section 14)
- [x] Properties read by subprocess documented (Section 14)

### Property Extraction Completeness
- [x] All property patterns searched (${}, %%, {}, parameters)
- [x] Message parameters checked for process properties (Section 7)
- [x] Operation headers/path parameters checked (Section 4)
- [x] Decision track properties identified (meta.* - Section 10)
- [x] Document properties that read other properties identified (Section 7)

### Input/Output Structure Analysis
- [x] Entry point operation identified (Section 2)
- [x] Request profile identified and loaded (Section 2)
- [x] Request profile structure analyzed (JSON - Section 2)
- [x] Array vs single object detected (Single object - Section 2)
- [x] ALL request fields extracted (9 fields - Section 2)
- [x] Request field mapping table generated (Section 2)
- [x] Response profile identified and loaded (Section 3)
- [x] Response profile structure analyzed (Section 3)
- [x] ALL response fields extracted (4 fields - Section 3)
- [x] Response field mapping table generated (Section 3)
- [x] Document processing behavior determined (Single document - Section 2)

### HTTP Status Codes and Return Path Responses
- [x] Section 6 (HTTP Status Codes and Return Path Responses - Step 1e) present
- [x] All return paths documented with HTTP status codes (4 return paths)
- [x] Response JSON examples provided for each return path (Section 6)
- [x] Populated fields documented for each return path (Section 6)
- [x] Decision conditions leading to each return documented (Section 6)
- [x] Downstream operation HTTP status codes documented (Section 6)

### Map Analysis
- [x] ALL map files identified and loaded (3 maps - Section 5)
- [x] Field mappings extracted from each map (Section 5)
- [x] Profile vs map field name discrepancies documented (Section 5)
- [x] Scripting functions analyzed (PropertyGet function - Section 5)
- [x] Static values identified and documented (Default values - Section 5)
- [x] Process property mappings documented (Section 5)
- [x] Map Analysis documented in Phase 1 document (Section 5)

---

## 17. System Layer Identification

### Downstream Systems

#### System 1: Oracle Fusion HCM

**System Name:** Oracle Fusion HCM  
**Connection ID:** aa1fcb29-d146-4425-9ea6-b9698090f60e  
**Connection Name:** Oracle Fusion  
**Connection Type:** HTTP  
**Authentication:** Basic Auth

**Base URL:** https://iaaxey-dev3.fa.ocs.oraclecloud.com:443  
**Resource Path:** hcmRestApi/resources/11.13.18.05/absences  
**Full Endpoint:** https://iaaxey-dev3.fa.ocs.oraclecloud.com:443/hcmRestApi/resources/11.13.18.05/absences

**Operations:**
- **Operation:** Leave Oracle Fusion Create (6e8920fd-af5a-430b-a1d9-9fde7ac29a12)
  - Method: POST
  - Purpose: Create absence/leave entry in Oracle HCM
  - Request Profile: a94fa205-c740-40a5-9fda-3d018611135a (HCM Leave Create JSON Profile)
  - Response Profile: 316175c7-0e45-4869-9ac6-5f9d69882a62 (Oracle Fusion Leave Response JSON Profile)

**API Type:** REST API  
**Data Format:** JSON  
**Response Handling:** May return GZIP compressed responses

#### System 2: Office 365 Email (SMTP)

**System Name:** Office 365 Email  
**Connection ID:** 00eae79b-2303-4215-8067-dcc299e42697  
**Connection Name:** Office 365 Email  
**Connection Type:** Mail (SMTP)  
**Authentication:** SMTP AUTH

**SMTP Server:** smtp-mail.outlook.com  
**Port:** 587  
**TLS:** true  
**SSL:** false

**Operations:**
- **Operation 1:** Email w Attachment (af07502a-fafd-4976-a691-45d51a33b549)
  - Purpose: Send email with attachment
  - Body Content Type: text/html
  - Data Content Type: text/plain
  - Disposition: attachment

- **Operation 2:** Email W/O Attachment (15a72a21-9b57-49a1-a8ed-d70367146644)
  - Purpose: Send email without attachment
  - Body Content Type: text/plain
  - Data Content Type: text/html
  - Disposition: inline

**API Type:** SMTP Email  
**Data Format:** HTML email body, text attachment

### System Layer API Requirements

**System API 1: Oracle Fusion HCM Leave API**
- **Purpose:** Create leave/absence entries in Oracle Fusion HCM
- **Endpoint:** POST /hcmRestApi/resources/11.13.18.05/absences
- **Authentication:** Basic Auth
- **Request:** JSON (leave details)
- **Response:** JSON (absence entry details) or GZIP compressed JSON

**System API 2: Office 365 Email API**
- **Purpose:** Send email notifications via Office 365 SMTP
- **Protocol:** SMTP over TLS
- **Authentication:** SMTP AUTH
- **Request:** Email message (HTML body, optional attachment)
- **Response:** SMTP status

---

## 18. Request/Response JSON Examples

### Process Layer Entry Point

**Operation:** Create Leave Oracle Fusion OP (8f709c2b-e63f-4d5f-9374-2932ed70415d)

**Request JSON Example (from D365):**

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

**Response JSON Examples:**

**Success Response (HTTP 200):**

```json
{
  "status": "success",
  "message": "Data successfully sent to Oracle Fusion",
  "personAbsenceEntryId": 300100123456789,
  "success": "true"
}
```

**Error Response - HTTP Failure (HTTP 400):**

```json
{
  "status": "failure",
  "message": "HTTP 400: Bad Request - Invalid absence type code",
  "success": "false"
}
```

**Error Response - Try/Catch Exception (HTTP 500):**

```json
{
  "status": "failure",
  "message": "Connection timeout to Oracle Fusion HCM API",
  "success": "false"
}
```

**Error Response - GZIP Decompression Failure (HTTP 400):**

```json
{
  "status": "failure",
  "message": "Failed to decompress GZIP response from Oracle Fusion",
  "success": "false"
}
```

### Downstream System Layer Calls

#### Operation: Leave Oracle Fusion Create (6e8920fd-af5a-430b-a1d9-9fde7ac29a12)

**System:** Oracle Fusion HCM  
**Endpoint:** POST https://iaaxey-dev3.fa.ocs.oraclecloud.com:443/hcmRestApi/resources/11.13.18.05/absences  
**Authentication:** Basic Auth (INTEGRATION.USER@al-ghurair.com)

**Request JSON (after transformation via map c426b4d6):**

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

**Response JSON (Success - HTTP 200/201):**

```json
{
  "personAbsenceEntryId": 300100123456789,
  "absenceCaseId": "ABS-2024-001234",
  "absenceEntryBasicFlag": true,
  "absencePatternCd": "SINGLE_DAY",
  "absenceStatusCd": "SUBMITTED",
  "absenceTypeId": 300000001234567,
  "approvalStatusCd": "APPROVED",
  "personId": 300000009876543,
  "personNumber": "9000604",
  "absenceType": "Sick Leave",
  "employer": "Al Ghurair Investment LLC",
  "startDate": "2024-03-24",
  "endDate": "2024-03-25",
  "startDateDuration": 1,
  "endDateDuration": 1,
  "duration": 2,
  "createdBy": "INTEGRATION.USER",
  "creationDate": "2024-03-24T10:30:00Z",
  "lastUpdateDate": "2024-03-24T10:30:00Z",
  "lastUpdatedBy": "INTEGRATION.USER",
  "links": [
    {
      "rel": "self",
      "href": "https://iaaxey-dev3.fa.ocs.oraclecloud.com:443/hcmRestApi/resources/11.13.18.05/absences/300100123456789",
      "name": "absences",
      "kind": "item"
    }
  ]
}
```

**Response JSON (Error - HTTP 400):**

```json
{
  "title": "Bad Request",
  "status": 400,
  "detail": "Invalid absence type code: Sick Leave",
  "o:errorCode": "INVALID_ABSENCE_TYPE",
  "o:errorPath": "/absences"
}
```

**Response JSON (Error - HTTP 401):**

```json
{
  "title": "Unauthorized",
  "status": 401,
  "detail": "Authentication failed",
  "o:errorCode": "UNAUTHORIZED"
}
```

**Response JSON (Error - HTTP 500):**

```json
{
  "title": "Internal Server Error",
  "status": 500,
  "detail": "An internal error occurred while processing the request",
  "o:errorCode": "INTERNAL_ERROR"
}
```

#### Operation: Email w Attachment (af07502a-fafd-4976-a691-45d51a33b549)

**System:** Office 365 Email (SMTP)  
**Protocol:** SMTP over TLS  
**Server:** smtp-mail.outlook.com:587  
**Authentication:** SMTP AUTH

**Email Content:**

**From:** Boomi.Dev.failures@al-ghurair.com  
**To:** BoomiIntegrationTeam@al-ghurair.com  
**Subject:** DEV Failure : [AtomName] ([ProcessName]) has errors to report

**Body (HTML):**

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
            <td>HCM_Leave Create</td>
          </tr>
          <tr>
            <th scope="row"><b><font size="2">Environment</font></b></th>
            <td>DEV</td>
          </tr>
          <tr>
            <th scope="row"><b><font size="2">Execution ID</font></b></th>
            <td>execution-abc-123-xyz</td>
          </tr>
          <tr>
            <th scope="row"><b><font size="2">Error Details</font></b></th>
            <td>Connection timeout to Oracle Fusion HCM API</td>
          </tr>
        </table>
      </font>
    </pre>
    <text>P.S: This is system generated email.</text>
  </body>
</html>
```

**Attachment:**
- **Filename:** HCM_Leave Create_2024-03-24T10:30:00.123Z.txt
- **Content:** Original request payload (JSON)

**SMTP Response (Success):**
- Status: 250 OK (email accepted)

**SMTP Response (Error):**
- Status: 550 (mailbox unavailable) or other SMTP error codes

#### Operation: Email W/O Attachment (15a72a21-9b57-49a1-a8ed-d70367146644)

**System:** Office 365 Email (SMTP)  
**Protocol:** SMTP over TLS  
**Server:** smtp-mail.outlook.com:587  
**Authentication:** SMTP AUTH

**Email Content:** Same as "Email w Attachment" but without attachment file

**SMTP Response:** Same as "Email w Attachment"

---

## 19. Function Exposure Decision Table

### Process Layer Function Exposure

**Process Name:** HCM Leave Create  
**Business Domain:** Human Resources (HCM)  
**Source System:** D365 (Dynamics 365)  
**Target System:** Oracle Fusion HCM

**Should this process be exposed as a Process Layer Azure Function?**

| Criteria | Evaluation | Score |
|---|---|---|
| **Business Logic Orchestration** | ✅ YES - Orchestrates leave creation between D365 and Oracle HCM | +1 |
| **Multiple System Integration** | ✅ YES - Integrates D365 (source) with Oracle HCM (target) | +1 |
| **Data Transformation** | ✅ YES - Transforms D365 leave format to Oracle HCM format | +1 |
| **Error Handling** | ✅ YES - Comprehensive error handling with notifications | +1 |
| **Business Domain Entity** | ✅ YES - Leave/Absence is a core HR business entity | +1 |
| **Reusability** | ✅ YES - Leave creation is reusable across HR processes | +1 |
| **Single System CRUD** | ❌ NO - Not a simple CRUD operation | 0 |
| **Pure Data Passthrough** | ❌ NO - Includes transformation and error handling | 0 |

**Total Score:** 6/6 (Strong candidate for Process Layer)

**DECISION:** ✅ **EXPOSE AS PROCESS LAYER FUNCTION**

**Function Name:** `CreateLeaveEntry`  
**HTTP Trigger:** POST /api/hcm/leave/create  
**Layer:** Process Layer

**Reasoning:**
1. **Business Orchestration:** Process orchestrates leave creation workflow between D365 and Oracle HCM
2. **Multi-System Integration:** Integrates two systems (D365 as source, Oracle HCM as target)
3. **Data Transformation:** Transforms leave data format (field name changes, data mapping)
4. **Error Handling:** Comprehensive error handling with email notifications
5. **Business Entity:** Leave/Absence is a core HR business domain entity
6. **Reusability:** Leave creation logic can be reused by multiple HR processes

**Process Layer Responsibilities:**
- Accept leave request from D365 (or other Experience Layer)
- Transform to Oracle HCM format
- Call Oracle HCM System Layer API
- Handle errors and send notifications
- Return standardized response

### System Layer Function Requirements

**System API 1: Oracle Fusion HCM Leave API**

**Should Oracle HCM operation be exposed as System Layer function?**

| Criteria | Evaluation | Score |
|---|---|---|
| **Direct System Access** | ✅ YES - Direct HTTP call to Oracle HCM REST API | +1 |
| **Single System Operation** | ✅ YES - Only interacts with Oracle HCM | +1 |
| **CRUD Operation** | ✅ YES - Creates absence entry in Oracle HCM | +1 |
| **Reusability** | ✅ YES - Can be reused by other HR processes | +1 |
| **System Abstraction** | ✅ YES - Abstracts Oracle HCM API complexity | +1 |

**Total Score:** 5/5 (Strong candidate for System Layer)

**DECISION:** ✅ **EXPOSE AS SYSTEM LAYER FUNCTION**

**Function Name:** `CreateOracleHcmAbsence`  
**HTTP Trigger:** POST /api/system/oracle-hcm/absences  
**Layer:** System Layer

**Reasoning:**
1. **System Abstraction:** Abstracts Oracle Fusion HCM REST API
2. **Single Responsibility:** Only creates absence entries in Oracle HCM
3. **Reusability:** Can be called by multiple Process Layer functions (leave create, leave update, etc.)
4. **System Insulation:** Insulates callers from Oracle HCM API changes

**System API 2: Office 365 Email API**

**Should email operations be exposed as System Layer functions?**

| Criteria | Evaluation | Score |
|---|---|---|
| **Direct System Access** | ✅ YES - Direct SMTP call to Office 365 | +1 |
| **Single System Operation** | ✅ YES - Only interacts with Office 365 SMTP | +1 |
| **Reusability** | ✅ YES - Email notification is reusable | +1 |
| **Already Exists** | ✅ YES - Common subprocess used across processes | +1 |

**Total Score:** 4/4 (Strong candidate for System Layer)

**DECISION:** ✅ **EXPOSE AS SYSTEM LAYER FUNCTION** (or use existing email service)

**Function Name:** `SendEmailNotification`  
**HTTP Trigger:** POST /api/system/email/send  
**Layer:** System Layer (or Common Service)

**Reasoning:**
1. **Common Service:** Email notification is a common service used across many processes
2. **Reusability:** Can be called by any Process Layer function needing email notifications
3. **System Abstraction:** Abstracts Office 365 SMTP complexity

**Alternative:** Use existing Azure Communication Services or SendGrid integration

### Function Exposure Summary

**Process Layer Functions:** 1
- CreateLeaveEntry (HCM Leave Create)

**System Layer Functions:** 2
- CreateOracleHcmAbsence (Oracle Fusion HCM)
- SendEmailNotification (Office 365 Email - or use existing service)

**Function Explosion Prevention:**
- ✅ Only expose functions that provide business value
- ✅ System Layer functions are reusable across multiple Process Layer functions
- ✅ Email notification can be a shared service (not process-specific)

---

## 20. Edge Cases and Special Handling

### Edge Case 1: GZIP Compressed Error Responses

**Detection:** Decision shape44 checks Content-Encoding header for "gzip"

**Handling:**
- If GZIP detected → shape45 (data process) decompresses using Groovy script
- Groovy script: `new GZIPInputStream(dataContext.getStream(it))`
- After decompression → Extract error message → Return error response

**Azure Function Implementation:**
- Check response Content-Encoding header
- If "gzip" → Decompress using GZipStream
- Extract error message from decompressed content

### Edge Case 2: Try/Catch with Dual Error Response

**Detection:** Branch shape20 in Catch path with 2 sequential paths

**Handling:**
- Path 1: Send email notification (subprocess)
- Path 2: Return error response to caller
- Sequential execution ensures notification sent before return

**Azure Function Implementation:**
- Catch block: Try to send email notification (best effort)
- Always return error response to caller
- Log if email notification fails

### Edge Case 3: Subprocess Without Explicit Return Path Mapping

**Detection:** Subprocess shape21 has no return path mapping in main process

**Handling:**
- Subprocess terminates with Stop (continue=true)
- Main process does not continue after subprocess (no dragpoints from shape21)
- Branch path 2 handles the actual return to caller

**Azure Function Implementation:**
- Call email service asynchronously or synchronously
- Do not wait for email response (fire-and-forget or log errors)
- Continue to return error response to caller

### Edge Case 4: Multiple Error Return Shapes

**Detection:** 3 different error return shapes (shape36, shape43, shape48)

**Handling:**
- shape36: HTTP error (non-GZIP)
- shape43: Try/Catch exception
- shape48: GZIP decompression error
- All use same error response profile but different HTTP status codes

**Azure Function Implementation:**
- Standardize error response format
- Use appropriate HTTP status codes:
  - 400: Bad Request (HTTP errors from Oracle)
  - 500: Internal Server Error (exceptions, connection errors)
- Include error message from Oracle or exception

---

## 21. Pre-Phase 2 Validation Gate

### Phase 1 Completion Checklist

**Input/Output Analysis:**
- [x] Step 1a (Input Structure Analysis) - COMPLETE and DOCUMENTED (Section 2)
- [x] Step 1b (Response Structure Analysis) - COMPLETE and DOCUMENTED (Section 3)
- [x] Step 1c (Operation Response Analysis) - COMPLETE and DOCUMENTED (Section 4)
- [x] Step 1d (Map Analysis) - COMPLETE and DOCUMENTED (Section 5)
- [x] Step 1e (HTTP Status Codes and Return Path Responses) - COMPLETE and DOCUMENTED (Section 6)

**Process Flow Analysis:**
- [x] Step 2 (Property Writes) - COMPLETE and DOCUMENTED (Section 7)
- [x] Step 3 (Property Reads) - COMPLETE and DOCUMENTED (Section 7)
- [x] Step 4 (Data Dependency Graph) - COMPLETE and DOCUMENTED (Section 8)
- [x] Step 5 (Control Flow Graph) - COMPLETE and DOCUMENTED (Section 9)
- [x] Step 6 (Reverse Flow Mapping) - COMPLETE and DOCUMENTED (Section 9)
- [x] Step 7 (Decision Analysis) - COMPLETE and DOCUMENTED (Section 10)
- [x] Step 7a (Subprocess Analysis) - COMPLETE and DOCUMENTED (Section 14)
- [x] Step 8 (Branch Analysis) - COMPLETE and DOCUMENTED (Section 11)
- [x] Step 9 (Execution Order) - COMPLETE and DOCUMENTED (Section 12)
- [x] Step 10 (Sequence Diagram) - COMPLETE and DOCUMENTED (Section 13)

**Contract Verification:**
- [x] Section 2 (Input Structure Analysis) - COMPLETE
- [x] Section 3 (Response Structure Analysis) - COMPLETE
- [x] Section 5 (Map Analysis) - COMPLETE
- [x] Section 6 (HTTP Status Codes and Return Path Responses) - COMPLETE
- [x] Section 18 (Request/Response JSON Examples) - COMPLETE

**Self-Check Questions:**

1. ❓ Did I analyze ALL map files? **YES** (3 maps analyzed)
2. ❓ Did I extract actual field names from maps? **YES** (Section 5)
3. ❓ Did I compare profile field names vs map field names? **YES** (Section 5)
4. ❓ Did I mark map field names as AUTHORITATIVE? **YES** (Section 5)
5. ❓ Did I analyze scripting functions in maps? **YES** (PropertyGet function - Section 5)
6. ❓ Did I extract HTTP status codes for all return paths? **YES** (4 return paths - Section 6)
7. ❓ Did I document response JSON for each return path? **YES** (Section 6)
8. ❓ Did I document populated fields for each return path? **YES** (Section 6)
9. ❓ Did I extract HTTP status codes for downstream operations? **YES** (Section 6)
10. ❓ Did I create request/response JSON examples? **YES** (Section 18)

**ALL SELF-CHECK ANSWERS: YES**

### Validation Status

✅ **PHASE 1 EXTRACTION COMPLETE**

**All mandatory sections present:**
- ✅ Operations Inventory (Section 1)
- ✅ Input Structure Analysis (Section 2)
- ✅ Response Structure Analysis (Section 3)
- ✅ Operation Response Analysis (Section 4)
- ✅ Map Analysis (Section 5)
- ✅ HTTP Status Codes and Return Paths (Section 6)
- ✅ Process Properties Analysis (Section 7)
- ✅ Data Dependency Graph (Section 8)
- ✅ Control Flow Graph (Section 9)
- ✅ Decision Shape Analysis (Section 10)
- ✅ Branch Shape Analysis (Section 11)
- ✅ Execution Order (Section 12)
- ✅ Sequence Diagram (Section 13)
- ✅ Subprocess Analysis (Section 14)
- ✅ Critical Patterns (Section 15)
- ✅ Validation Checklist (Section 16)
- ✅ System Layer Identification (Section 17)
- ✅ Request/Response JSON Examples (Section 18)
- ✅ Function Exposure Decision Table (Section 19)

**All self-check questions answered: YES**

**Sequence diagram references all prior steps: YES**

**Ready to proceed to Phase 2: ✅ YES**

---

## 22. Summary and Key Findings

### Process Overview

**Business Purpose:** Synchronize leave/absence data from D365 to Oracle Fusion HCM

**Integration Pattern:** Point-to-point integration with error notification

**Key Operations:**
1. Transform D365 leave request to Oracle HCM format
2. POST leave data to Oracle Fusion HCM REST API
3. Handle success/error responses
4. Send email notification on errors

### Critical Dependencies

1. **URL Configuration → HTTP Operation**
   - shape8 sets URL → shape33 uses URL for HTTP request

2. **HTTP Operation → Status Check**
   - shape33 executes HTTP POST → shape2 checks status code

3. **Error Capture → Error Response**
   - Error shapes write DPP_ErrorMessage → Error maps read DPP_ErrorMessage

4. **Branch Sequential Execution**
   - Path 1 writes DPP_ErrorMessage → Path 2 reads DPP_ErrorMessage

### Execution Paths

**Path 1: Success (HTTP 20*)**
```
Input → Map → URL setup → HTTP POST → Status check (TRUE) → Success map → Return 200
```

**Path 2: HTTP Error (non-20*, not GZIP)**
```
Input → Map → URL setup → HTTP POST → Status check (FALSE) → Content check (FALSE) → Error capture → Error map → Return 400
```

**Path 3: HTTP Error (non-20*, GZIP)**
```
Input → Map → URL setup → HTTP POST → Status check (FALSE) → Content check (TRUE) → GZIP decompress → Error capture → Error map → Return 400
```

**Path 4: Exception (Try/Catch)**
```
Input → Exception → Branch (sequential) → Path 1: Error notification (email) → Path 2: Error map → Return 500
```

### Recommended Azure Functions

**Process Layer:**
- **Function:** CreateLeaveEntry
- **Trigger:** HTTP POST
- **Route:** /api/hcm/leave/create
- **Purpose:** Orchestrate leave creation between D365 and Oracle HCM

**System Layer:**
- **Function 1:** CreateOracleHcmAbsence
- **Trigger:** HTTP POST
- **Route:** /api/system/oracle-hcm/absences
- **Purpose:** Create absence entry in Oracle Fusion HCM

- **Function 2:** SendEmailNotification (or use existing service)
- **Trigger:** HTTP POST
- **Route:** /api/system/email/send
- **Purpose:** Send email notifications via Office 365

### Next Steps

1. ✅ Phase 1 extraction complete
2. ⏭️ Ready for Phase 2: Code generation
3. ⏭️ Generate Process Layer Azure Function (CreateLeaveEntry)
4. ⏭️ Generate System Layer Azure Functions (CreateOracleHcmAbsence, SendEmailNotification)
5. ⏭️ Generate DTOs based on field mappings
6. ⏭️ Implement error handling patterns
7. ⏭️ Implement GZIP decompression logic
8. ⏭️ Implement email notification integration

---

**Document Version:** 1.0  
**Created:** 2026-02-20  
**Status:** ✅ COMPLETE - Ready for Phase 2
