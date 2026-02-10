# BOOMI EXTRACTION PHASE 1 - HCM Leave Create Process

**Process Name:** HCM_Leave Create  
**Process ID:** ca69f858-785f-4565-ba1f-b4edc6cca05b  
**Purpose:** Sync leave data between D365 and Oracle HCM  
**System of Record:** Oracle Fusion HCM  
**Integration Type:** REST API (JSON)

---

## 1. Operations Inventory

### Main Process Operations

| Operation ID | Operation Name | Type | Sub Type | Purpose |
|---|---|---|---|---|
| 8f709c2b-e63f-4d5f-9374-2932ed70415d | Create Leave Oracle Fusion OP | connector-action | wss | Entry point - Web Service Server Listen |
| 6e8920fd-af5a-430b-a1d9-9fde7ac29a12 | Leave Oracle Fusion Create | connector-action | http | HTTP POST to Oracle Fusion HCM API |

### Subprocess Operations (Email Notification)

| Operation ID | Operation Name | Type | Sub Type | Purpose |
|---|---|---|---|---|
| af07502a-fafd-4976-a691-45d51a33b549 | Email w Attachment | connector-action | mail | Send email with attachment |
| 15a72a21-9b57-49a1-a8ed-d70367146644 | Email W/O Attachment | connector-action | mail | Send email without attachment |

### Connections

| Connection ID | Connection Name | Type | URL/Host | Authentication |
|---|---|---|---|---|
| aa1fcb29-d146-4425-9ea6-b9698090f60e | Oracle Fusion | http | https://iaaxey-dev3.fa.ocs.oraclecloud.com:443 | Basic Auth (INTEGRATION.USER@al-ghurair.com) |
| 00eae79b-2303-4215-8067-dcc299e42697 | Office 365 Email | mail | smtp-mail.outlook.com:587 | SMTP Auth (Boomi.Dev.failures@al-ghurair.com) |

---

## 2. Input Structure Analysis (Step 1a)

### Entry Point Operation
- **Operation ID:** 8f709c2b-e63f-4d5f-9374-2932ed70415d
- **Operation Name:** Create Leave Oracle Fusion OP
- **Operation Type:** connector-action (wss - Web Service Server)
- **Input Type:** singlejson
- **Request Profile ID:** febfa3e1-f719-4ee8-ba57-cdae34137ab3

### Request Profile Structure
- **Profile ID:** febfa3e1-f719-4ee8-ba57-cdae34137ab3
- **Profile Name:** D365 Leave Create JSON Profile
- **Profile Type:** profile.json
- **Root Structure:** Root/Object/[fields]
- **Array Detection:** ❌ NO - Single object structure
- **Input Type:** singlejson

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
- **Behavior:** single_document
- **Description:** Boomi processes single document (singlejson input type)
- **Execution Pattern:** single_execution
- **Session Management:** one_session_per_execution

### All Input Fields Extracted

| Boomi Field Path | Boomi Field Name | Data Type | Required | Is Mappable | Parent Path |
|---|---|---|---|---|---|
| Root/Object/employeeNumber | employeeNumber | number | Yes | true | Root/Object |
| Root/Object/absenceType | absenceType | character | Yes | true | Root/Object |
| Root/Object/employer | employer | character | Yes | true | Root/Object |
| Root/Object/startDate | startDate | character | Yes | true | Root/Object |
| Root/Object/endDate | endDate | character | Yes | true | Root/Object |
| Root/Object/absenceStatusCode | absenceStatusCode | character | Yes | true | Root/Object |
| Root/Object/approvalStatusCode | approvalStatusCode | character | Yes | true | Root/Object |
| Root/Object/startDateDuration | startDateDuration | number | Yes | true | Root/Object |
| Root/Object/endDateDuration | endDateDuration | number | Yes | true | Root/Object |

### Field Mapping Table (Boomi → Azure DTO)

| Boomi Field Path | Boomi Field Name | Data Type | Required | Azure DTO Property | Notes |
|---|---|---|---|---|---|
| Root/Object/employeeNumber | employeeNumber | number | Yes | EmployeeNumber | Tracked field (hr_employee_id) |
| Root/Object/absenceType | absenceType | character | Yes | AbsenceType | Leave type |
| Root/Object/employer | employer | character | Yes | Employer | Employer name |
| Root/Object/startDate | startDate | character | Yes | StartDate | Leave start date |
| Root/Object/endDate | endDate | character | Yes | EndDate | Leave end date |
| Root/Object/absenceStatusCode | absenceStatusCode | character | Yes | AbsenceStatusCode | Absence status |
| Root/Object/approvalStatusCode | approvalStatusCode | character | Yes | ApprovalStatusCode | Approval status |
| Root/Object/startDateDuration | startDateDuration | number | Yes | StartDateDuration | Duration for start date |
| Root/Object/endDateDuration | endDateDuration | number | Yes | EndDateDuration | Duration for end date |

---

## 3. Response Structure Analysis (Step 1b)

### Response Profile (Success Path)
- **Profile ID:** 316175c7-0e45-4869-9ac6-5f9d69882a62
- **Profile Name:** Oracle Fusion Leave Response JSON Profile
- **Profile Type:** profile.json
- **Structure:** Root/Object/[fields]

### Response Structure (Oracle Fusion API Response)

The Oracle Fusion API returns a comprehensive leave absence entry object with the following structure:

```json
{
  "personAbsenceEntryId": 123456,
  "absenceCaseId": "ABC123",
  "absenceEntryBasicFlag": true,
  "absencePatternCd": "PATTERN_CODE",
  "absenceStatusCd": "SUBMITTED",
  "absenceTypeId": 789,
  "absenceTypeReasonId": "REASON_ID",
  "agreementId": "AGREEMENT_ID",
  "approvalStatusCd": "APPROVED",
  "authStatusUpdateDate": "2024-03-24T10:30:00Z",
  "personNumber": "9000604",
  "absenceType": "Sick Leave",
  "employer": "Al Ghurair Investment LLC",
  "links": [
    {
      "rel": "self",
      "href": "https://...",
      "name": "personAbsenceEntries",
      "kind": "item"
    }
  ]
}
```

### Response Field Mapping Table

| Boomi Field Path | Boomi Field Name | Data Type | Azure DTO Property | Notes |
|---|---|---|---|---|
| Root/Object/personAbsenceEntryId | personAbsenceEntryId | number | PersonAbsenceEntryId | Primary identifier |
| Root/Object/absenceCaseId | absenceCaseId | character | AbsenceCaseId | Case identifier |
| Root/Object/absenceStatusCd | absenceStatusCd | character | AbsenceStatusCd | Status code |
| Root/Object/absenceTypeId | absenceTypeId | number | AbsenceTypeId | Type identifier |
| Root/Object/approvalStatusCd | approvalStatusCd | character | ApprovalStatusCd | Approval status |
| Root/Object/personNumber | personNumber | character | PersonNumber | Employee number |
| Root/Object/absenceType | absenceType | character | AbsenceType | Leave type |
| Root/Object/employer | employer | character | Employer | Employer name |
| Root/Object/startDate | startDate | character | StartDate | Leave start date |
| Root/Object/endDate | endDate | character | EndDate | Leave end date |
| Root/Object/startDateDuration | startDateDuration | number | StartDateDuration | Duration for start date |
| Root/Object/endDateDuration | endDateDuration | number | EndDateDuration | Duration for end date |
| Root/Object/links | links | array | Links | HATEOAS links array |

### Process Layer Response Profile
- **Profile ID:** f4ca3a70-114a-4601-bad8-44a3eb20e2c0
- **Profile Name:** Leave D365 Response
- **Profile Type:** profile.json

### Process Layer Response Structure

```json
{
  "leaveResponse": {
    "status": "success",
    "message": "Data successfully sent to Oracle Fusion",
    "personAbsenceEntryId": 123456,
    "success": "true"
  }
}
```

### Process Layer Response Fields

| Field Name | Data Type | Source | Notes |
|---|---|---|---|
| status | character | Default value | "success" or "failure" |
| message | character | Default or error message | Success: "Data successfully sent to Oracle Fusion", Error: DPP_ErrorMessage |
| personAbsenceEntryId | number | Oracle Fusion API response | Mapped from API response |
| success | character | Default value | "true" or "false" |

---

## 4. Operation Response Analysis (Step 1c)

### Operation: Leave Oracle Fusion Create (6e8920fd-af5a-430b-a1d9-9fde7ac29a12)

**Operation Details:**
- **Type:** connector-action (http)
- **Method:** POST
- **Response Profile:** NONE (responseProfileType: NONE)
- **Response Handling:** returnResponses: true, returnErrors: true
- **Response Header Mapping:** Content-Encoding → DDP_RespHeader

**Response Data Extraction:**
- **No explicit response profile** - Operation configured with responseProfileType: NONE
- **Response header extracted:** Content-Encoding header mapped to process property `dynamicdocument.DDP_RespHeader`
- **Response body:** Returned as-is (JSON from Oracle Fusion API)

**Extracted Fields:**
- **DDP_RespHeader** (extracted by shape8 - not used, only for gzip check in shape44)
- **Response body content** - Used in map shape34 (success path) to extract personAbsenceEntryId

**Data Consumers:**
- **shape2 (Decision):** Checks HTTP status code (meta.base.applicationstatuscode) - POST-OPERATION decision
- **shape44 (Decision):** Checks Content-Encoding header (dynamicdocument.DDP_RespHeader) - POST-OPERATION decision
- **shape34 (Map):** Maps Oracle Fusion response to Process Layer response (success path)
- **shape39/shape46/shape47 (Map):** Maps error message to Process Layer response (error paths)

**Business Logic Implications:**
- **Operation MUST execute FIRST** before any decisions can check response status or headers
- **Decision shape2** depends on HTTP status code from this operation
- **Decision shape44** depends on Content-Encoding header from this operation
- **Success/Error mapping** depends on response data from this operation

### Operation: Email w Attachment (af07502a-fafd-4976-a691-45d51a33b549)

**Operation Details:**
- **Type:** connector-action (mail)
- **Purpose:** Send error notification email with attachment
- **Response Profile:** NONE
- **Data Consumers:** NONE (terminal operation in subprocess)

### Operation: Email W/O Attachment (15a72a21-9b57-49a1-a8ed-d70367146644)

**Operation Details:**
- **Type:** connector-action (mail)
- **Purpose:** Send error notification email without attachment
- **Response Profile:** NONE
- **Data Consumers:** NONE (terminal operation in subprocess)

---

## 5. Map Analysis (Step 1d)

### Map Inventory

| Map ID | Map Name | From Profile | To Profile | Purpose |
|---|---|---|---|---|
| c426b4d6-2aff-450e-b43b-59956c4dbc96 | Leave Create Map | febfa3e1-f719-4ee8-ba57-cdae34137ab3 | a94fa205-c740-40a5-9fda-3d018611135a | Transform D365 request to Oracle Fusion format |
| e4fd3f59-edb5-43a1-aeae-143b600a064e | Oracle Fusion Leave Response Map | 316175c7-0e45-4869-9ac6-5f9d69882a62 | f4ca3a70-114a-4601-bad8-44a3eb20e2c0 | Map Oracle response to D365 response (success) |
| f46b845a-7d75-41b5-b0ad-c41a6a8e9b12 | Leave Error Map | 23d7a2e9-5cb0-4e9c-9e4b-3154834bad0d | f4ca3a70-114a-4601-bad8-44a3eb20e2c0 | Map error message to D365 response (error) |

### Map 1: Leave Create Map (c426b4d6-2aff-450e-b43b-59956c4dbc96)

**Type:** REST API Request Transformation  
**From Profile:** febfa3e1-f719-4ee8-ba57-cdae34137ab3 (D365 Leave Create JSON Profile)  
**To Profile:** a94fa205-c740-40a5-9fda-3d018611135a (HCM Leave Create JSON Profile)

**Field Mappings:**

| Source Field | Source Type | Target Field | Transformation | Notes |
|---|---|---|---|---|
| employeeNumber | profile | personNumber | Direct mapping | Employee identifier |
| absenceType | profile | absenceType | Direct mapping | Leave type |
| employer | profile | employer | Direct mapping | Employer name |
| startDate | profile | startDate | Direct mapping | Leave start date |
| endDate | profile | endDate | Direct mapping | Leave end date |
| absenceStatusCode | profile | absenceStatusCd | Field name change | Status code |
| approvalStatusCode | profile | approvalStatusCd | Field name change | Approval status |
| startDateDuration | profile | startDateDuration | Direct mapping | Duration |
| endDateDuration | profile | endDateDuration | Direct mapping | Duration |

**Field Name Discrepancies:**

| Source Field Name | Target Field Name | Authority | Use in Request |
|---|---|---|---|
| absenceStatusCode | absenceStatusCd | ✅ MAP | absenceStatusCd |
| approvalStatusCode | approvalStatusCd | ✅ MAP | approvalStatusCd |

**CRITICAL RULE:** Map field names are AUTHORITATIVE for Oracle Fusion API requests.

### Map 2: Oracle Fusion Leave Response Map (e4fd3f59-edb5-43a1-aeae-143b600a064e)

**Type:** Success Response Transformation  
**From Profile:** 316175c7-0e45-4869-9ac6-5f9d69882a62 (Oracle Fusion Leave Response JSON Profile)  
**To Profile:** f4ca3a70-114a-4601-bad8-44a3eb20e2c0 (Leave D365 Response)

**Field Mappings:**

| Source Field | Source Type | Target Field | Transformation | Notes |
|---|---|---|---|---|
| personAbsenceEntryId | profile | personAbsenceEntryId | Direct mapping | Primary ID from Oracle response |
| (static) | default | status | Default value: "success" | Success indicator |
| (static) | default | message | Default value: "Data successfully sent to Oracle Fusion" | Success message |
| (static) | default | success | Default value: "true" | Boolean success flag |

### Map 3: Leave Error Map (f46b845a-7d75-41b5-b0ad-c41a6a8e9b12)

**Type:** Error Response Transformation  
**From Profile:** 23d7a2e9-5cb0-4e9c-9e4b-3154834bad0d (Dummy FF Profile)  
**To Profile:** f4ca3a70-114a-4601-bad8-44a3eb20e2c0 (Leave D365 Response)

**Field Mappings:**

| Source Field | Source Type | Target Field | Transformation | Notes |
|---|---|---|---|---|
| DPP_ErrorMessage | function (PropertyGet) | message | Process property | Error message from catch or API failure |
| (static) | default | status | Default value: "failure" | Failure indicator |
| (static) | default | success | Default value: "false" | Boolean failure flag |

**Function Analysis:**

| Function Key | Type | Input | Output | Logic |
|---|---|---|---|---|
| 1 | PropertyGet | Property Name: "DPP_ErrorMessage" | Result | Retrieves error message from process property |

---

## 6. HTTP Status Codes and Return Path Responses (Step 1e)

### Return Path 1: Success Response (shape35)

**Return Label:** "Success Response"  
**Return Shape ID:** shape35  
**HTTP Status Code:** 200  
**Decision Conditions Leading to Return:**
- Decision shape2: meta.base.applicationstatuscode wildcard matches "20*" → TRUE path

**Populated Response Fields:**

| Field Name | Field Path | Source | Populated By |
|---|---|---|---|
| status | leaveResponse/Object/status | default | Map default value: "success" |
| message | leaveResponse/Object/message | default | Map default value: "Data successfully sent to Oracle Fusion" |
| personAbsenceEntryId | leaveResponse/Object/personAbsenceEntryId | operation_response | Leave Oracle Fusion Create (shape33) |
| success | leaveResponse/Object/success | default | Map default value: "true" |

**Response JSON Example:**

```json
{
  "leaveResponse": {
    "status": "success",
    "message": "Data successfully sent to Oracle Fusion",
    "personAbsenceEntryId": 123456,
    "success": "true"
  }
}
```

### Return Path 2: Error Response - API Failure (shape36)

**Return Label:** "Error Response"  
**Return Shape ID:** shape36  
**HTTP Status Code:** 400 (inferred from API failure)  
**Decision Conditions Leading to Return:**
- Decision shape2: meta.base.applicationstatuscode wildcard matches "20*" → FALSE path
- Decision shape44: dynamicdocument.DDP_RespHeader equals "gzip" → FALSE path

**Populated Response Fields:**

| Field Name | Field Path | Source | Populated By |
|---|---|---|---|
| status | leaveResponse/Object/status | default | Map default value: "failure" |
| message | leaveResponse/Object/message | process_property | DPP_ErrorMessage (from meta.base.applicationstatusmessage) |
| success | leaveResponse/Object/success | default | Map default value: "false" |

**Response JSON Example:**

```json
{
  "leaveResponse": {
    "status": "failure",
    "message": "Oracle Fusion API returned error: [error details]",
    "success": "false"
  }
}
```

### Return Path 3: Error Response - GZIP Content (shape48)

**Return Label:** "Error Response"  
**Return Shape ID:** shape48  
**HTTP Status Code:** 400 (inferred from content encoding issue)  
**Decision Conditions Leading to Return:**
- Decision shape2: meta.base.applicationstatuscode wildcard matches "20*" → FALSE path
- Decision shape44: dynamicdocument.DDP_RespHeader equals "gzip" → TRUE path
- After GZIP decompression (shape45)

**Populated Response Fields:**

| Field Name | Field Path | Source | Populated By |
|---|---|---|---|
| status | leaveResponse/Object/status | default | Map default value: "failure" |
| message | leaveResponse/Object/message | process_property | DPP_ErrorMessage (from decompressed response) |
| success | leaveResponse/Object/success | default | Map default value: "false" |

**Response JSON Example:**

```json
{
  "leaveResponse": {
    "status": "failure",
    "message": "[Decompressed error message from Oracle Fusion]",
    "success": "false"
  }
}
```

### Return Path 4: Error Response - Catch Block (shape43)

**Return Label:** "Error Response"  
**Return Shape ID:** shape43  
**HTTP Status Code:** 500 (inferred from exception)  
**Decision Conditions Leading to Return:**
- Try/Catch (shape17) → Catch path (error identifier)

**Populated Response Fields:**

| Field Name | Field Path | Source | Populated By |
|---|---|---|---|
| status | leaveResponse/Object/status | default | Map default value: "failure" |
| message | leaveResponse/Object/message | process_property | DPP_ErrorMessage (from meta.base.catcherrorsmessage) |
| success | leaveResponse/Object/success | default | Map default value: "false" |

**Response JSON Example:**

```json
{
  "leaveResponse": {
    "status": "failure",
    "message": "[Exception message from try/catch]",
    "success": "false"
  }
}
```

### Downstream Operations HTTP Status Codes

| Operation Name | Expected Success Codes | Error Codes | Error Handling |
|---|---|---|---|
| Leave Oracle Fusion Create | 200, 201, 202 (20*) | 400, 401, 403, 404, 500 | Check HTTP status, throw exception on non-20* |

---

## 7. Process Properties Analysis (Steps 2-3)

### Process Properties WRITTEN

| Property Name | Written By Shape(s) | Source | Purpose |
|---|---|---|---|
| process.DPP_Process_Name | shape38 | Execution property: "Process Name" | Process name for logging |
| process.DPP_AtomName | shape38 | Execution property: "Atom Name" | Atom name for logging |
| process.DPP_Payload | shape38 | Current document | Request payload for email attachment |
| process.DPP_ExecutionID | shape38 | Execution property: "Execution Id" | Execution ID for tracking |
| process.DPP_File_Name | shape38 | Concatenation: Process Name + Timestamp + ".txt" | Email attachment filename |
| process.DPP_Subject | shape38 | Concatenation: Atom Name + " (" + Process Name + " ) has errors to report" | Email subject |
| process.To_Email | shape38 | Defined property: "To_Email" | Email recipient |
| process.DPP_HasAttachment | shape38 | Defined property: "DPP_HasAttachment" | Flag for email attachment |
| process.DPP_ErrorMessage | shape19, shape39, shape46 | Track property: meta.base.catcherrorsmessage or meta.base.applicationstatusmessage | Error message |
| dynamicdocument.URL | shape8 | Defined property: "Resource_Path" | API resource path |
| dynamicdocument.DDP_RespHeader | (operation response) | Response header: Content-Encoding | Response content encoding |

### Process Properties READ

| Property Name | Read By Shape(s) | Purpose |
|---|---|---|---|
| process.DPP_HasAttachment | shape4 (subprocess decision) | Determine if email should have attachment |
| process.To_Email | shape6, shape20 (subprocess) | Email recipient address |
| process.DPP_Subject | shape6, shape20 (subprocess) | Email subject |
| process.DPP_MailBody | shape6, shape20 (subprocess) | Email body content |
| process.DPP_File_Name | shape6 (subprocess) | Email attachment filename |
| process.DPP_Process_Name | shape11, shape23 (subprocess message) | Process name in email body |
| process.DPP_AtomName | shape11, shape23 (subprocess message) | Atom name in email body |
| process.DPP_ExecutionID | shape11, shape23 (subprocess message) | Execution ID in email body |
| process.DPP_ErrorMessage | shape11, shape23 (subprocess message), map_f46b845a (error map) | Error details in email and response |
| dynamicdocument.URL | operation 6e8920fd (HTTP operation) | API endpoint URL |
| dynamicdocument.DDP_RespHeader | shape44 (decision) | Check response content encoding |

---

## 8. Data Dependency Graph (Step 4)

### Dependency Graph

**Property: process.DPP_ErrorMessage**
- **Writers:** shape19 (catch path), shape39 (API error path), shape46 (gzip error path)
- **Readers:** shape11 (subprocess message), shape23 (subprocess message), map_f46b845a (error map)
- **Dependency Chain:** shape19/shape39/shape46 → subprocess shapes (shape11/shape23) → map_f46b845a

**Property: dynamicdocument.URL**
- **Writer:** shape8
- **Reader:** operation 6e8920fd (HTTP operation - shape33)
- **Dependency Chain:** shape8 MUST execute BEFORE shape33

**Property: dynamicdocument.DDP_RespHeader**
- **Writer:** operation 6e8920fd (response header mapping)
- **Reader:** shape44 (decision)
- **Dependency Chain:** shape33 (operation) MUST execute BEFORE shape44 (decision)

**Property: process.DPP_Payload**
- **Writer:** shape38
- **Reader:** shape15 (subprocess message)
- **Dependency Chain:** shape38 MUST execute BEFORE subprocess (shape21)

**Property: process.DPP_File_Name**
- **Writer:** shape38
- **Reader:** shape6 (subprocess)
- **Dependency Chain:** shape38 MUST execute BEFORE subprocess (shape21)

**Property: process.DPP_Subject**
- **Writer:** shape38
- **Reader:** shape6, shape20 (subprocess)
- **Dependency Chain:** shape38 MUST execute BEFORE subprocess (shape21)

**Property: process.To_Email**
- **Writer:** shape38
- **Reader:** shape6, shape20 (subprocess)
- **Dependency Chain:** shape38 MUST execute BEFORE subprocess (shape21)

**Property: process.DPP_HasAttachment**
- **Writer:** shape38
- **Reader:** shape4 (subprocess decision)
- **Dependency Chain:** shape38 MUST execute BEFORE subprocess (shape21)

**Property: process.DPP_Process_Name, process.DPP_AtomName, process.DPP_ExecutionID**
- **Writer:** shape38
- **Readers:** shape11, shape23 (subprocess messages)
- **Dependency Chain:** shape38 MUST execute BEFORE subprocess (shape21)

### Dependency Chains Summary

**Main Process Chain:**
1. shape38 (write properties) → shape17 (try/catch) → shape29 (map) → shape8 (set URL) → shape49 (notify) → shape33 (HTTP operation) → shape2 (decision on status)

**Success Path Chain:**
2. shape2 (TRUE) → shape34 (map response) → shape35 (return success)

**Error Path Chain (Non-20* Status):**
3. shape2 (FALSE) → shape44 (check gzip) → shape39/shape46 (set error) → shape40/shape47 (map error) → shape36/shape48 (return error)

**Catch Path Chain:**
4. shape17 (catch) → shape20 (branch) → shape19/shape41 (set error) → shape21/shape43 (subprocess/return error)

### Independent Operations
- **NONE** - All operations have dependencies on property writes from shape38

---

## 9. Control Flow Graph (Step 5)

### Control Flow Map

**shape1 (start):**
- → shape38 (Input_details)

**shape38 (Input_details):**
- → shape17 (catcherrors)

**shape17 (catcherrors):**
- → shape29 (Try - default path)
- → shape20 (Catch - error path)

**shape29 (map):**
- → shape8 (set URL)

**shape8 (set URL):**
- → shape49 (notify)

**shape49 (notify):**
- → shape33 (HTTP operation)

**shape33 (HTTP operation):**
- → shape2 (HTTP Status 20 check)

**shape2 (HTTP Status 20 check):**
- → shape34 (TRUE - success path)
- → shape44 (FALSE - error path)

**shape34 (map response):**
- → shape35 (Success Response - return)

**shape44 (Check Response Content Type):**
- → shape45 (TRUE - gzip path)
- → shape39 (FALSE - non-gzip error path)

**shape45 (dataprocess - decompress gzip):**
- → shape46 (error msg)

**shape46 (error msg):**
- → shape47 (map)

**shape47 (map):**
- → shape48 (Error Response - return)

**shape39 (error msg):**
- → shape40 (map)

**shape40 (map):**
- → shape36 (Error Response - return)

**shape20 (branch - catch path):**
- → shape19 (path 1 - ErrorMsg)
- → shape41 (path 2 - map)

**shape19 (ErrorMsg):**
- → shape21 (subprocess)

**shape41 (map):**
- → shape43 (Error Response - return)

**shape21 (subprocess):**
- (terminal - no outgoing dragpoints)

### Connection Summary
- **Total Shapes:** 15 (main process) + 11 (subprocess) = 26
- **Total Connections:** 18 (main process) + 10 (subprocess) = 28
- **Shapes with Multiple Outgoing Connections:**
  - shape17 (catcherrors): 2 paths (try, catch)
  - shape2 (decision): 2 paths (true, false)
  - shape44 (decision): 2 paths (true, false)
  - shape20 (branch): 2 paths (1, 2)
  - shape4 (subprocess decision): 2 paths (true, false)

### Reverse Flow Mapping (Step 6)

**Convergence Points:**
- **NONE in main process** - All paths lead to separate return statements

**Terminal Points:**
- shape35 (Success Response - return)
- shape36 (Error Response - return)
- shape43 (Error Response - return)
- shape48 (Error Response - return)

---

## 10. Decision Shape Analysis (Step 7)

### ✅ Decision Data Sources Identified: YES
### ✅ Decision Types Classified: YES
### ✅ Execution Order Verified: YES
### ✅ All Decision Paths Traced: YES
### ✅ Decision Patterns Identified: YES

### Decision 1: shape2 - HTTP Status 20 check

**Shape ID:** shape2  
**Comparison:** wildcard  
**Value 1:** meta.base.applicationstatuscode (track property)  
**Value 2:** "20*" (static)

**Data Source:** TRACK_PROPERTY (meta.base.applicationstatuscode)  
**Decision Type:** POST-OPERATION  
**Actual Execution Order:** Operation shape33 (HTTP call) → Response → Decision shape2 → Route based on status

**TRUE Path:**
- **Destination:** shape34 (map response)
- **Termination:** shape35 (Return Documents - Success Response)
- **Type:** return

**FALSE Path:**
- **Destination:** shape44 (Check Response Content Type)
- **Termination:** shape36 or shape48 (Return Documents - Error Response)
- **Type:** return

**Pattern:** Error Check (Success vs Failure)  
**Convergence Point:** NONE  
**Early Exit:** Both paths terminate (return documents)

**Business Logic:**
- If HTTP status code matches "20*" (200, 201, 202, etc.) → Success path
- If HTTP status code is NOT "20*" (400, 500, etc.) → Error path
- Operation MUST execute FIRST, then check response status

### Decision 2: shape44 - Check Response Content Type

**Shape ID:** shape44  
**Comparison:** equals  
**Value 1:** dynamicdocument.DDP_RespHeader (track property)  
**Value 2:** "gzip" (static)

**Data Source:** TRACK_PROPERTY (dynamicdocument.DDP_RespHeader - from operation response header)  
**Decision Type:** POST-OPERATION  
**Actual Execution Order:** Operation shape33 (HTTP call) → Response Header Extracted → Decision shape44 → Route based on Content-Encoding

**TRUE Path:**
- **Destination:** shape45 (dataprocess - decompress gzip)
- **Termination:** shape48 (Return Documents - Error Response)
- **Type:** return

**FALSE Path:**
- **Destination:** shape39 (error msg)
- **Termination:** shape36 (Return Documents - Error Response)
- **Type:** return

**Pattern:** Conditional Logic (Content Encoding Handling)  
**Convergence Point:** NONE  
**Early Exit:** Both paths terminate (return documents)

**Business Logic:**
- If Content-Encoding header equals "gzip" → Decompress response, then return error
- If Content-Encoding header is NOT "gzip" → Return error directly
- This decision only executes when HTTP status is NOT "20*" (error scenario)

### Decision 3: shape4 - Attachment_Check (Subprocess)

**Shape ID:** shape4 (in subprocess a85945c5-3004-42b9-80b1-104f465cd1fb)  
**Comparison:** equals  
**Value 1:** process.DPP_HasAttachment (process property)  
**Value 2:** "Y" (static)

**Data Source:** PROCESS_PROPERTY (DPP_HasAttachment - set by main process shape38)  
**Decision Type:** POST-OPERATION (depends on main process property)  
**Actual Execution Order:** Main process shape38 writes property → Subprocess reads property → Decision shape4 → Route based on flag

**TRUE Path:**
- **Destination:** shape11 (Mail_Body message with attachment)
- **Termination:** shape5 (Stop - continue=true)
- **Type:** continue

**FALSE Path:**
- **Destination:** shape23 (Mail_Body message without attachment)
- **Termination:** shape9 (Stop - continue=true)
- **Type:** continue

**Pattern:** Conditional Logic (Optional Processing)  
**Convergence Point:** Both paths converge at Stop (continue=true)  
**Early Exit:** NONE (both paths continue)

**Business Logic:**
- If DPP_HasAttachment equals "Y" → Send email with attachment (operation af07502a)
- If DPP_HasAttachment is NOT "Y" → Send email without attachment (operation 15a72a21)
- Both paths result in email being sent, difference is attachment presence

### Decision Patterns Summary

| Pattern Type | Decision Shapes | Description |
|---|---|---|
| Error Check (Success vs Failure) | shape2 | HTTP status code check - routes to success or error path |
| Conditional Logic (Content Encoding) | shape44 | GZIP content handling - decompress if needed |
| Conditional Logic (Optional Processing) | shape4 | Email attachment flag - with or without attachment |

---

## 11. Branch Shape Analysis (Step 8)

### ✅ Classification Completed: YES
### ✅ Assumption Check: NO (analyzed dependencies)
### ✅ Properties Extracted: YES
### ✅ Dependency Graph Built: YES
### ✅ Topological Sort Applied: N/A (no API calls in branch paths)

### Branch Shape: shape20 (Catch Error Path)

**Shape ID:** shape20  
**Number of Paths:** 2  
**Location:** Catch path of try/catch (shape17)

**Path 1: shape19 (ErrorMsg)**
- **Shapes:** shape19 → shape21 (subprocess)
- **Properties READ:** meta.base.catcherrorsmessage
- **Properties WRITTEN:** process.DPP_ErrorMessage
- **Terminal:** shape21 (subprocess - no return, throws exception)

**Path 2: shape41 (map)**
- **Shapes:** shape41 → shape43 (return)
- **Properties READ:** process.DPP_ErrorMessage (from map function)
- **Properties WRITTEN:** NONE
- **Terminal:** shape43 (Return Documents - Error Response)

**Properties Analysis:**

| Path | Properties READ | Properties WRITTEN | API Calls |
|---|---|---|---|
| Path 1 | meta.base.catcherrorsmessage | process.DPP_ErrorMessage | NO (subprocess call) |
| Path 2 | process.DPP_ErrorMessage | NONE | NO |

**Dependency Graph:**
- Path 2 reads process.DPP_ErrorMessage
- Path 1 writes process.DPP_ErrorMessage
- **Therefore:** Path 1 depends on Path 2? NO - Path 2 reads what Path 1 writes
- **Correct Dependency:** Path 1 MUST execute BEFORE Path 2

**Classification:** SEQUENTIAL  
**Reasoning:** Path 2 reads process.DPP_ErrorMessage which Path 1 writes. No API calls in either path, but data dependency exists.

**Topological Sort Order:**
1. Path 1 (shape19 → shape21) - Writes DPP_ErrorMessage
2. Path 2 (shape41 → shape43) - Reads DPP_ErrorMessage

**Path Termination:**
- Path 1: shape21 (subprocess - throws exception, no return)
- Path 2: shape43 (Return Documents)

**Convergence Points:** NONE

**Execution Continues From:** NONE (Path 1 throws exception, Path 2 returns)

---

## 12. Subprocess Analysis (Step 7a)

### Subprocess: (Sub) Office 365 Email (a85945c5-3004-42b9-80b1-104f465cd1fb)

**Subprocess ID:** a85945c5-3004-42b9-80b1-104f465cd1fb  
**Called By:** shape21 (main process catch path)

### Internal Flow

**START (shape1):**
- → shape2 (catcherrors)

**shape2 (catcherrors):**
- → shape4 (Try - Attachment_Check decision)
- → shape10 (Catch - exception)

**shape4 (Attachment_Check decision):**
- → shape11 (TRUE - Mail_Body with attachment)
- → shape23 (FALSE - Mail_Body without attachment)

**shape11 (Mail_Body with attachment):**
- → shape14 (set_MailBody)

**shape14 (set_MailBody):**
- → shape15 (payload message)

**shape15 (payload):**
- → shape6 (set_Mail_Properties)

**shape6 (set_Mail_Properties):**
- → shape3 (Email w Attachment operation)

**shape3 (Email w Attachment):**
- → shape5 (Stop - continue=true)

**shape23 (Mail_Body without attachment):**
- → shape22 (set_MailBody)

**shape22 (set_MailBody):**
- → shape20 (set_Mail_Properties)

**shape20 (set_Mail_Properties):**
- → shape7 (Email W/O Attachment operation)

**shape7 (Email W/O Attachment):**
- → shape9 (Stop - continue=true)

**shape10 (exception):**
- (terminal - throws exception)

### Return Paths

| Return Label | Shape ID | Path | Type |
|---|---|---|---|
| SUCCESS (implicit) | shape5, shape9 | Stop (continue=true) | Success return |
| ERROR (exception) | shape10 | Exception shape | Exception thrown |

### Main Process Mapping
- **No explicit return path mapping** - Subprocess uses Stop (continue=true) for success
- **Exception path** - Throws exception if email operations fail

### Properties Written by Subprocess
- **process.DPP_MailBody** (shape14, shape22) - Email body content

### Properties Read by Subprocess from Main Process
- **process.DPP_HasAttachment** - Determines email attachment presence
- **process.To_Email** - Email recipient
- **process.DPP_Subject** - Email subject
- **process.DPP_Payload** - Email attachment content
- **process.DPP_File_Name** - Email attachment filename
- **process.DPP_Process_Name** - Process name for email body
- **process.DPP_AtomName** - Atom name for email body
- **process.DPP_ExecutionID** - Execution ID for email body
- **process.DPP_ErrorMessage** - Error details for email body

### Subprocess Business Logic
1. **Entry:** Receives email content and properties from main process
2. **Decision:** Check if attachment is required (DPP_HasAttachment = "Y")
3. **TRUE Path:** Build email with attachment → Send via operation af07502a → Success
4. **FALSE Path:** Build email without attachment → Send via operation 15a72a21 → Success
5. **Error Handling:** If any error occurs, throw exception (shape10)

---

## 13. Execution Order (Step 9)

### ✅ Business Logic Verified FIRST: YES
### ✅ Operation Analysis Complete: YES
### ✅ Business Logic Execution Order Identified: YES
### ✅ Data Dependencies Checked FIRST: YES
### ✅ Operation Response Analysis Used: YES (Section 4)
### ✅ Decision Analysis Used: YES (Section 10)
### ✅ Dependency Graph Used: YES (Section 8)
### ✅ Branch Analysis Used: YES (Section 11)
### ✅ Property Dependency Verification: YES

### Business Logic Flow (Step 0 - FIRST)

**Operation: Leave Oracle Fusion Create (shape33)**
- **Purpose:** Create leave absence entry in Oracle Fusion HCM
- **What it does:** Sends HTTP POST request to Oracle Fusion API with leave data
- **What it produces:**
  - HTTP status code (meta.base.applicationstatuscode)
  - Response body (JSON with personAbsenceEntryId)
  - Response header (Content-Encoding → dynamicdocument.DDP_RespHeader)
- **Dependent Operations:** Decision shape2 (checks HTTP status), Decision shape44 (checks Content-Encoding)
- **Business Flow:** This operation MUST execute FIRST because all subsequent decisions depend on its response

**Subprocess: Office 365 Email (shape21)**
- **Purpose:** Send error notification email to support team
- **What it does:** Sends email with error details (with or without attachment based on flag)
- **What it produces:** Email sent (no return data to main process)
- **Dependent Operations:** NONE (terminal operation)
- **Business Flow:** Executes ONLY on error paths (catch or API failure)

### Actual Business Flow

**Main Flow:**
1. **shape38 (Input_details)** - Extract input properties, set up email properties
   - **Produces:** All process properties (DPP_Process_Name, DPP_AtomName, DPP_Payload, DPP_ExecutionID, DPP_File_Name, DPP_Subject, To_Email, DPP_HasAttachment)
   - **Operations that depend on this:** ALL subsequent operations

2. **shape17 (Try/Catch)** - Wrap main operation in error handler
   - **Try Path:** Continue to shape29
   - **Catch Path:** Branch to error handling (shape20)

3. **shape29 (map)** - Transform D365 request to Oracle Fusion format
   - **Produces:** Mapped request body
   - **Operations that depend on this:** shape8 (needs mapped data)

4. **shape8 (set URL)** - Set dynamic URL for HTTP operation
   - **Produces:** dynamicdocument.URL property
   - **Operations that depend on this:** shape33 (HTTP operation needs URL)

5. **shape49 (notify)** - Log request payload (INFO level)
   - **Produces:** Log entry
   - **Operations that depend on this:** NONE

6. **shape33 (HTTP operation)** - Call Oracle Fusion API
   - **Produces:** HTTP status code, response body, response headers
   - **Operations that depend on this:** shape2 (checks status), shape44 (checks headers), shape34 (maps response)
   - **CRITICAL:** This operation MUST execute BEFORE any decisions can check response

7. **shape2 (Decision)** - Check HTTP status code
   - **Depends on:** shape33 (needs HTTP status code)
   - **Routes to:** Success path (shape34) OR Error path (shape44)

**Success Path:**
8. **shape34 (map)** - Map Oracle Fusion response to D365 response
   - **Produces:** Success response with personAbsenceEntryId
9. **shape35 (return)** - Return success response

**Error Path (Non-20* Status):**
8. **shape44 (Decision)** - Check Content-Encoding header
   - **Depends on:** shape33 (needs response header)
   - **Routes to:** GZIP path (shape45) OR Non-GZIP path (shape39)

**GZIP Path:**
9. **shape45 (decompress)** - Decompress GZIP response
10. **shape46 (error msg)** - Extract error message
11. **shape47 (map)** - Map error to D365 response
12. **shape48 (return)** - Return error response

**Non-GZIP Path:**
9. **shape39 (error msg)** - Extract error message
10. **shape40 (map)** - Map error to D365 response
11. **shape36 (return)** - Return error response

**Catch Path (Exception):**
8. **shape20 (branch)** - Branch to parallel error handling
   - **Path 1:** shape19 → shape21 (subprocess - send email)
   - **Path 2:** shape41 → shape43 (return error)
   - **Execution:** Sequential (Path 1 writes DPP_ErrorMessage, Path 2 reads it)

### Dependency Verification

**Reference to Step 4 (Data Dependency Graph):**

**Dependency Chain 1: URL Setup**
- shape8 writes dynamicdocument.URL
- shape33 (HTTP operation) reads dynamicdocument.URL
- **Proof:** shape8 MUST execute BEFORE shape33

**Dependency Chain 2: Response Status Check**
- shape33 produces meta.base.applicationstatuscode
- shape2 reads meta.base.applicationstatuscode
- **Proof:** shape33 MUST execute BEFORE shape2

**Dependency Chain 3: Response Header Check**
- shape33 produces dynamicdocument.DDP_RespHeader
- shape44 reads dynamicdocument.DDP_RespHeader
- **Proof:** shape33 MUST execute BEFORE shape44

**Dependency Chain 4: Error Message Propagation**
- shape19 writes process.DPP_ErrorMessage (catch path)
- shape41 reads process.DPP_ErrorMessage (via map function)
- **Proof:** In branch shape20, Path 1 (shape19) MUST execute BEFORE Path 2 (shape41)

**Dependency Chain 5: Subprocess Properties**
- shape38 writes all email properties (To_Email, DPP_Subject, DPP_HasAttachment, etc.)
- Subprocess (shape21) reads these properties
- **Proof:** shape38 MUST execute BEFORE shape21

### Branch Execution Order

**Reference to Step 8 (Branch Analysis):**

**Branch shape20 (Catch Path):**
- **Classification:** SEQUENTIAL
- **Topological Sort Order:** Path 1 (shape19) → Path 2 (shape41)
- **Reasoning:** Path 2 reads process.DPP_ErrorMessage which Path 1 writes

### Decision Path Tracing

**Reference to Step 7 (Decision Analysis):**

**Decision shape2 (HTTP Status Check):**
- **TRUE Path:** shape34 → shape35 (Success Response)
- **FALSE Path:** shape44 → (shape45 → shape46 → shape47 → shape48) OR (shape39 → shape40 → shape36) (Error Response)
- **Convergence:** NONE (separate returns)

**Decision shape44 (Content Encoding Check):**
- **TRUE Path:** shape45 → shape46 → shape47 → shape48 (GZIP Error Response)
- **FALSE Path:** shape39 → shape40 → shape36 (Non-GZIP Error Response)
- **Convergence:** NONE (separate returns)

**Decision shape4 (Attachment Check - Subprocess):**
- **TRUE Path:** shape11 → shape14 → shape15 → shape6 → shape3 → shape5 (Email with attachment)
- **FALSE Path:** shape23 → shape22 → shape20 → shape7 → shape9 (Email without attachment)
- **Convergence:** Both paths converge at Stop (continue=true)

### Complete Execution Order

**Main Process:**
1. shape1 (START)
2. shape38 (Input_details) - Write all properties
3. shape17 (Try/Catch wrapper)
4. **TRY PATH:**
   - shape29 (map) - Transform request
   - shape8 (set URL) - Set API endpoint
   - shape49 (notify) - Log request
   - shape33 (HTTP operation) - Call Oracle Fusion API
   - shape2 (Decision: HTTP Status 20 check)
   - **IF TRUE (20* status):**
     - shape34 (map response) - Map success response
     - shape35 (Return Documents - Success) [HTTP: 200]
   - **IF FALSE (non-20* status):**
     - shape44 (Decision: Check Response Content Type)
     - **IF TRUE (gzip):**
       - shape45 (decompress gzip)
       - shape46 (error msg)
       - shape47 (map error)
       - shape48 (Return Documents - Error) [HTTP: 400]
     - **IF FALSE (non-gzip):**
       - shape39 (error msg)
       - shape40 (map error)
       - shape36 (Return Documents - Error) [HTTP: 400]
5. **CATCH PATH:**
   - shape20 (branch) - Sequential execution
   - **Path 1 (FIRST):** shape19 (ErrorMsg) → shape21 (subprocess - send email)
   - **Path 2 (SECOND):** shape41 (map error) → shape43 (Return Documents - Error) [HTTP: 500]

**Subprocess (shape21 - Office 365 Email):**
1. shape1 (START)
2. shape2 (Try/Catch wrapper)
3. **TRY PATH:**
   - shape4 (Decision: Attachment_Check)
   - **IF TRUE (HasAttachment = "Y"):**
     - shape11 (Mail_Body message)
     - shape14 (set_MailBody)
     - shape15 (payload message)
     - shape6 (set_Mail_Properties)
     - shape3 (Email w Attachment operation)
     - shape5 (Stop - continue=true)
   - **IF FALSE (HasAttachment != "Y"):**
     - shape23 (Mail_Body message)
     - shape22 (set_MailBody)
     - shape20 (set_Mail_Properties)
     - shape7 (Email W/O Attachment operation)
     - shape9 (Stop - continue=true)
4. **CATCH PATH:**
   - shape10 (exception) - Throw exception

---

## 14. Sequence Diagram (Step 10)

**📋 NOTE:** This diagram shows the execution flow based on:
- **Dependency graph in Step 4** (property dependencies)
- **Decision analysis in Step 7** (decision data sources and execution order)
- **Control flow graph in Step 5** (dragpoint connections)
- **Branch analysis in Step 8** (branch path ordering)
- **Execution order in Step 9** (complete business logic flow)

**📋 NOTE:** Detailed request/response JSON examples are documented in:
- **Section 6: HTTP Status Codes and Return Path Responses** - For response JSON with populated fields for return paths
- **Section 15: Request/Response JSON Examples** - For detailed request/response JSON examples

```
START (shape1)
 |
 ├─→ shape38: Input_details (Extract Properties)
 |   └─→ WRITES: [process.DPP_Process_Name, process.DPP_AtomName, process.DPP_Payload, 
 |                 process.DPP_ExecutionID, process.DPP_File_Name, process.DPP_Subject, 
 |                 process.To_Email, process.DPP_HasAttachment]
 |
 ├─→ shape17: Try/Catch Wrapper
 |   |
 |   ├─→ TRY PATH:
 |   |   |
 |   |   ├─→ shape29: Map (Leave Create Map)
 |   |   |   └─→ READS: [Input profile fields]
 |   |   |   └─→ WRITES: [Mapped request body]
 |   |   |
 |   |   ├─→ shape8: Set URL (Document Property)
 |   |   |   └─→ READS: [Defined property: Resource_Path]
 |   |   |   └─→ WRITES: [dynamicdocument.URL]
 |   |   |
 |   |   ├─→ shape49: Notify (Log Request)
 |   |   |   └─→ READS: [Current document]
 |   |   |   └─→ INFO: Log request payload
 |   |   |
 |   |   ├─→ shape33: Leave Oracle Fusion Create (Downstream)
 |   |   |   └─→ READS: [dynamicdocument.URL, mapped request body]
 |   |   |   └─→ WRITES: [meta.base.applicationstatuscode, dynamicdocument.DDP_RespHeader, response body]
 |   |   |   └─→ HTTP: [Expected: 200/201/202 (20*), Error: 400/401/403/404/500]
 |   |   |   └─→ METHOD: POST
 |   |   |   └─→ URL: {dynamicdocument.URL}
 |   |   |
 |   |   ├─→ shape2: Decision (HTTP Status 20 check)
 |   |   |   └─→ READS: [meta.base.applicationstatuscode]
 |   |   |   └─→ CONDITION: applicationstatuscode wildcard "20*"
 |   |   |   |
 |   |   |   ├─→ IF TRUE (Status 20*) → SUCCESS PATH:
 |   |   |   |   |
 |   |   |   |   ├─→ shape34: Map (Oracle Fusion Leave Response Map)
 |   |   |   |   |   └─→ READS: [Oracle Fusion response body]
 |   |   |   |   |   └─→ WRITES: [Success response with personAbsenceEntryId]
 |   |   |   |   |
 |   |   |   |   └─→ shape35: Return Documents [HTTP: 200] [SUCCESS]
 |   |   |   |       └─→ Response: { "status": "success", "message": "Data successfully sent to Oracle Fusion", 
 |   |   |   |                      "personAbsenceEntryId": 123456, "success": "true" }
 |   |   |   |
 |   |   |   └─→ IF FALSE (Status NOT 20*) → ERROR PATH:
 |   |   |       |
 |   |   |       ├─→ shape44: Decision (Check Response Content Type)
 |   |   |       |   └─→ READS: [dynamicdocument.DDP_RespHeader]
 |   |   |       |   └─→ CONDITION: DDP_RespHeader equals "gzip"
 |   |   |       |   |
 |   |   |       |   ├─→ IF TRUE (gzip) → GZIP ERROR PATH:
 |   |   |       |   |   |
 |   |   |       |   |   ├─→ shape45: DataProcess (Decompress GZIP)
 |   |   |       |   |   |   └─→ READS: [Response body]
 |   |   |       |   |   |   └─→ WRITES: [Decompressed response]
 |   |   |       |   |   |   └─→ SCRIPT: GZIPInputStream decompression
 |   |   |       |   |   |
 |   |   |       |   |   ├─→ shape46: ErrorMsg (Extract Error)
 |   |   |       |   |   |   └─→ READS: [meta.base.applicationstatusmessage]
 |   |   |       |   |   |   └─→ WRITES: [process.DPP_ErrorMessage]
 |   |   |       |   |   |
 |   |   |       |   |   ├─→ shape47: Map (Leave Error Map)
 |   |   |       |   |   |   └─→ READS: [process.DPP_ErrorMessage]
 |   |   |       |   |   |   └─→ WRITES: [Error response]
 |   |   |       |   |   |
 |   |   |       |   |   └─→ shape48: Return Documents [HTTP: 400] [ERROR]
 |   |   |       |   |       └─→ Response: { "status": "failure", "message": "[error details]", "success": "false" }
 |   |   |       |   |
 |   |   |       |   └─→ IF FALSE (non-gzip) → NON-GZIP ERROR PATH:
 |   |   |       |       |
 |   |   |       |       ├─→ shape39: ErrorMsg (Extract Error)
 |   |   |       |       |   └─→ READS: [meta.base.applicationstatusmessage]
 |   |   |       |       |   └─→ WRITES: [process.DPP_ErrorMessage]
 |   |   |       |       |
 |   |   |       |       ├─→ shape40: Map (Leave Error Map)
 |   |   |       |       |   └─→ READS: [process.DPP_ErrorMessage]
 |   |   |       |       |   └─→ WRITES: [Error response]
 |   |   |       |       |
 |   |   |       |       └─→ shape36: Return Documents [HTTP: 400] [ERROR]
 |   |   |       |           └─→ Response: { "status": "failure", "message": "[error details]", "success": "false" }
 |   |   |
 |   └─→ CATCH PATH (Exception):
 |       |
 |       ├─→ shape20: Branch (2 paths - SEQUENTIAL)
 |       |   |
 |       |   ├─→ PATH 1 (FIRST):
 |       |   |   |
 |       |   |   ├─→ shape19: ErrorMsg (Extract Catch Message)
 |       |   |   |   └─→ READS: [meta.base.catcherrorsmessage]
 |       |   |   |   └─→ WRITES: [process.DPP_ErrorMessage]
 |       |   |   |
 |       |   |   └─→ shape21: ProcessCall (Office 365 Email Subprocess)
 |       |   |       └─→ SUBPROCESS INTERNAL FLOW:
 |       |   |           |
 |       |   |           ├─→ START (shape1)
 |       |   |           |
 |       |   |           ├─→ shape2: Try/Catch Wrapper (Subprocess)
 |       |   |           |   |
 |       |   |           |   ├─→ TRY PATH:
 |       |   |           |   |   |
 |       |   |           |   |   ├─→ shape4: Decision (Attachment_Check)
 |       |   |           |   |   |   └─→ READS: [process.DPP_HasAttachment]
 |       |   |           |   |   |   └─→ CONDITION: DPP_HasAttachment equals "Y"
 |       |   |           |   |   |   |
 |       |   |           |   |   |   ├─→ IF TRUE (Has Attachment):
 |       |   |           |   |   |   |   |
 |       |   |           |   |   |   |   ├─→ shape11: Mail_Body (Build HTML Email)
 |       |   |           |   |   |   |   |   └─→ READS: [process.DPP_Process_Name, process.DPP_AtomName, 
 |       |   |           |   |   |   |   |              process.DPP_ExecutionID, process.DPP_ErrorMessage]
 |       |   |           |   |   |   |   |
 |       |   |           |   |   |   |   ├─→ shape14: Set_MailBody
 |       |   |           |   |   |   |   |   └─→ WRITES: [process.DPP_MailBody]
 |       |   |           |   |   |   |   |
 |       |   |           |   |   |   |   ├─→ shape15: Payload Message
 |       |   |           |   |   |   |   |   └─→ READS: [process.DPP_Payload]
 |       |   |           |   |   |   |   |
 |       |   |           |   |   |   |   ├─→ shape6: Set_Mail_Properties
 |       |   |           |   |   |   |   |   └─→ READS: [Defined properties: From_Email, To_Email, Environment]
 |       |   |           |   |   |   |   |   └─→ READS: [process.To_Email, process.DPP_Subject, process.DPP_MailBody, process.DPP_File_Name]
 |       |   |           |   |   |   |   |   └─→ WRITES: [connector.mail.fromAddress, connector.mail.toAddress, 
 |       |   |           |   |   |   |   |              connector.mail.subject, connector.mail.body, connector.mail.filename]
 |       |   |           |   |   |   |   |
 |       |   |           |   |   |   |   ├─→ shape3: Email w Attachment (Downstream)
 |       |   |           |   |   |   |   |   └─→ READS: [connector.mail.* properties]
 |       |   |           |   |   |   |   |   └─→ SMTP: Send email with attachment
 |       |   |           |   |   |   |   |
 |       |   |           |   |   |   |   └─→ shape5: Stop (continue=true) [SUCCESS RETURN]
 |       |   |           |   |   |   |
 |       |   |           |   |   |   └─→ IF FALSE (No Attachment):
 |       |   |           |   |   |       |
 |       |   |           |   |   |       ├─→ shape23: Mail_Body (Build HTML Email)
 |       |   |           |   |   |       |   └─→ READS: [process.DPP_Process_Name, process.DPP_AtomName, 
 |       |   |           |   |   |       |              process.DPP_ExecutionID, process.DPP_ErrorMessage]
 |       |   |           |   |   |       |
 |       |   |           |   |   |       ├─→ shape22: Set_MailBody
 |       |   |           |   |   |       |   └─→ WRITES: [process.DPP_MailBody]
 |       |   |           |   |   |       |
 |       |   |           |   |   |       ├─→ shape20: Set_Mail_Properties
 |       |   |           |   |   |       |   └─→ READS: [Defined properties: From_Email, To_Email, Environment]
 |       |   |           |   |   |       |   └─→ READS: [process.To_Email, process.DPP_Subject, process.DPP_MailBody]
 |       |   |           |   |   |       |   └─→ WRITES: [connector.mail.fromAddress, connector.mail.toAddress, 
 |       |   |           |   |   |       |              connector.mail.subject, connector.mail.body]
 |       |   |           |   |   |       |
 |       |   |           |   |   |       ├─→ shape7: Email W/O Attachment (Downstream)
 |       |   |           |   |   |       |   └─→ READS: [connector.mail.* properties]
 |       |   |           |   |   |       |   └─→ SMTP: Send email without attachment
 |       |   |           |   |   |       |
 |       |   |           |   |   |       └─→ shape9: Stop (continue=true) [SUCCESS RETURN]
 |       |   |           |   |
 |       |   |           |   └─→ CATCH PATH:
 |       |   |           |       |
 |       |   |           |       └─→ shape10: Exception (Throw Exception) [ERROR RETURN]
 |       |   |
 |       |   └─→ PATH 2 (SECOND):
 |       |       |
 |       |       ├─→ shape41: Map (Leave Error Map)
 |       |       |   └─→ READS: [process.DPP_ErrorMessage]
 |       |       |   └─→ WRITES: [Error response]
 |       |       |
 |       |       └─→ shape43: Return Documents [HTTP: 500] [ERROR]
 |       |           └─→ Response: { "status": "failure", "message": "[exception message]", "success": "false" }
 |
 └─→ END
```

**CRITICAL NOTES:**
1. **shape33 (HTTP operation) MUST execute BEFORE shape2 (decision)** - Decision checks response status
2. **shape33 MUST execute BEFORE shape44 (decision)** - Decision checks response header
3. **shape8 MUST execute BEFORE shape33** - Operation needs URL property
4. **shape38 MUST execute BEFORE ALL operations** - All operations depend on properties written here
5. **Branch shape20 executes SEQUENTIALLY** - Path 1 writes DPP_ErrorMessage, Path 2 reads it
6. **Subprocess shape21 reads properties from main process** - Main process MUST write properties before subprocess

---

## 15. Critical Patterns Identified

### Pattern 1: Try/Catch with Error Notification

**Identification:**
- Try/Catch wrapper (shape17) around main operation
- Catch path branches to error handling (shape20)
- One branch path sends email notification (shape21 - subprocess)
- Other branch path returns error response (shape41 → shape43)

**Execution Rule:**
- Try path executes main operation
- If exception occurs, catch path executes
- Catch path SEQUENTIALLY: (1) Send email notification, (2) Return error response

**Business Logic:**
- Ensures error notifications are sent before returning error to caller
- Provides visibility into integration failures

### Pattern 2: HTTP Status Code Decision (Success vs Error)

**Identification:**
- Decision shape2 checks HTTP status code (meta.base.applicationstatuscode)
- TRUE path (20*) → Success response mapping
- FALSE path (non-20*) → Error response handling

**Execution Rule:**
- HTTP operation MUST execute FIRST
- Decision checks response status
- Routes to appropriate response mapping

**Business Logic:**
- Differentiates successful API calls (20x) from failures (4xx, 5xx)
- Enables different response handling for success vs error

### Pattern 3: Content Encoding Handling (GZIP Decompression)

**Identification:**
- Decision shape44 checks Content-Encoding header (dynamicdocument.DDP_RespHeader)
- TRUE path (gzip) → Decompress response before error mapping
- FALSE path (non-gzip) → Direct error mapping

**Execution Rule:**
- Only executes on error path (HTTP status NOT 20*)
- Decompresses GZIP content if needed
- Extracts error message from decompressed content

**Business Logic:**
- Handles Oracle Fusion API responses that may be GZIP-compressed
- Ensures error messages are readable regardless of compression

### Pattern 4: Subprocess with Conditional Execution (Email Attachment)

**Identification:**
- Subprocess (shape21) called from catch path
- Internal decision (shape4) checks DPP_HasAttachment flag
- TRUE path → Send email with attachment (operation af07502a)
- FALSE path → Send email without attachment (operation 15a72a21)

**Execution Rule:**
- Main process writes DPP_HasAttachment property
- Subprocess reads property and routes accordingly
- Both paths result in email being sent

**Business Logic:**
- Flexible email notification with optional attachment
- Attachment contains request payload for debugging

---

## 16. Validation Checklist

### Data Dependencies
- [x] All property WRITES identified (Section 7)
- [x] All property READS identified (Section 7)
- [x] Dependency graph built (Section 8)
- [x] Execution order satisfies all dependencies (no read-before-write) (Section 13)

### Decision Analysis
- [x] ALL decision shapes inventoried (Section 10)
- [x] BOTH TRUE and FALSE paths traced to termination (Section 10)
- [x] Pattern type identified for each decision (Section 10)
- [x] Early exits identified and documented (All paths terminate with return)
- [x] Convergence points identified (NONE - separate returns)

### Branch Analysis
- [x] Each branch classified as parallel or sequential (Section 11)
- [x] Classification based on dependency analysis (NOT assumption)
- [x] Properties extracted for each path (Section 11)
- [x] Dependency graph built (Section 11)
- [x] Topological sort applied (Section 11 - Path 1 before Path 2)
- [x] Each path traced to terminal point (Section 11)
- [x] Convergence points identified (NONE)
- [x] Execution continuation point determined (NONE)

### Sequence Diagram
- [x] Format follows required structure (Section 14)
- [x] Each operation shows READS and WRITES (Section 14)
- [x] Decisions show both TRUE and FALSE paths (Section 14)
- [x] Sequence diagram references Step 4 (dependency graph) (Section 14)
- [x] Sequence diagram references Step 5 (control flow graph) (Section 14)
- [x] Sequence diagram references Step 7 (decision analysis) (Section 14)
- [x] Sequence diagram references Step 8 (branch analysis) (Section 14)
- [x] Sequence diagram references Step 9 (execution order) (Section 14)
- [x] Early exits marked [ERROR] (Section 14)
- [x] HTTP status codes documented (Section 14)

### Subprocess Analysis
- [x] ALL subprocesses analyzed (Section 12)
- [x] Internal flow traced (Section 12)
- [x] Return paths identified (Section 12)
- [x] Return path labels mapped to main process shapes (Section 12)
- [x] Properties written by subprocess documented (Section 12)
- [x] Properties read by subprocess from main process documented (Section 12)

### Input/Output Structure Analysis
- [x] Entry point operation identified (Section 2)
- [x] Request profile identified and loaded (Section 2)
- [x] Request profile structure analyzed (Section 2)
- [x] Array vs single object detected (Section 2 - Single object)
- [x] Document processing behavior determined (Section 2)
- [x] ALL request fields extracted (Section 2)
- [x] Request field mapping table generated (Section 2)
- [x] Response profile identified and loaded (Section 3)
- [x] Response profile structure analyzed (Section 3)
- [x] ALL response fields extracted (Section 3)
- [x] Response field mapping table generated (Section 3)

### HTTP Status Codes and Return Path Responses
- [x] Section 6 present (HTTP Status Codes and Return Path Responses)
- [x] All return paths documented with HTTP status codes (Section 6)
- [x] Response JSON examples provided for each return path (Section 6)
- [x] Populated fields documented for each return path (Section 6)
- [x] Decision conditions leading to each return documented (Section 6)
- [x] Downstream operation HTTP status codes documented (Section 6)

### Map Analysis
- [x] ALL map files identified and loaded (Section 5)
- [x] Field mappings extracted from each map (Section 5)
- [x] Profile vs map field name discrepancies documented (Section 5)
- [x] Map field names marked as AUTHORITATIVE (Section 5)
- [x] Static values identified and documented (Section 5)

---

## 17. System Layer Identification

### Third-Party Systems

| System Name | System Type | Integration Type | Operations | Authentication |
|---|---|---|---|---|
| **Oracle Fusion HCM** | SOR (System of Record) | REST API (JSON) | Create Leave Absence | Basic Auth (credentials-per-request) |
| **Office 365 Email (SMTP)** | Notification System | SMTP | Send Email (with/without attachment) | SMTP Auth (credentials-per-request) |

### System Layer Projects Required

**Project 1: Oracle Fusion HCM System Layer**
- **Repository Name:** sys-oraclefusionhcm-mgmt (or sys-hcm-mgmt)
- **Purpose:** Manage Oracle Fusion HCM operations (Leave management)
- **Operations to Expose:**
  - CreateLeave (POST to /hcmRestApi/resources/11.13.18.05/absences)

**Project 2: Email Notification System Layer**
- **Repository Name:** sys-email-mgmt (or sys-smtp-mgmt)
- **Purpose:** Send email notifications via Office 365 SMTP
- **Operations to Expose:**
  - SendEmail (with optional attachment)

**Note:** Email notification is a cross-cutting concern. If sys-email-mgmt already exists, use it. If not, this should be created as a reusable System Layer for all email operations across the enterprise.

---

## 18. Request/Response JSON Examples

### Process Layer Entry Point

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
  "leaveResponse": {
    "status": "success",
    "message": "Data successfully sent to Oracle Fusion",
    "personAbsenceEntryId": 123456,
    "success": "true"
  }
}
```

**Error Response - API Failure (HTTP 400):**

```json
{
  "leaveResponse": {
    "status": "failure",
    "message": "Oracle Fusion API returned error: Invalid absence type",
    "success": "false"
  }
}
```

**Error Response - Exception (HTTP 500):**

```json
{
  "leaveResponse": {
    "status": "failure",
    "message": "Exception occurred during processing: Connection timeout",
    "success": "false"
  }
}
```

### Downstream System Layer Calls

**Operation: Leave Oracle Fusion Create (shape33)**

**Request JSON (to Oracle Fusion HCM API):**

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

**Response JSON (Success - HTTP 200):**

```json
{
  "personAbsenceEntryId": 123456,
  "absenceCaseId": "ABC123",
  "absenceEntryBasicFlag": true,
  "absencePatternCd": "PATTERN_CODE",
  "absenceStatusCd": "SUBMITTED",
  "absenceTypeId": 789,
  "absenceTypeReasonId": "REASON_ID",
  "agreementId": "AGREEMENT_ID",
  "approvalStatusCd": "APPROVED",
  "authStatusUpdateDate": "2024-03-24T10:30:00Z",
  "personNumber": "9000604",
  "absenceType": "Sick Leave",
  "employer": "Al Ghurair Investment LLC",
  "startDate": "2024-03-24",
  "endDate": "2024-03-25",
  "startDateDuration": 1,
  "endDateDuration": 1,
  "links": [
    {
      "rel": "self",
      "href": "https://iaaxey-dev3.fa.ocs.oraclecloud.com:443/hcmRestApi/resources/11.13.18.05/absences/123456",
      "name": "personAbsenceEntries",
      "kind": "item"
    }
  ]
}
```

**Response JSON (Error - HTTP 400):**

```json
{
  "title": "Bad Request",
  "detail": "Invalid absence type code",
  "o:errorCode": "VALIDATION_ERROR",
  "status": "400"
}
```

**Response JSON (Error - HTTP 401):**

```json
{
  "title": "Unauthorized",
  "detail": "Authentication failed",
  "o:errorCode": "AUTH_ERROR",
  "status": "401"
}
```

**Response JSON (Error - HTTP 500):**

```json
{
  "title": "Internal Server Error",
  "detail": "An unexpected error occurred",
  "o:errorCode": "INTERNAL_ERROR",
  "status": "500"
}
```

---

## 19. Function Exposure Decision Table

### ✅ MANDATORY: Function Exposure Decision Process (BLOCKING)

**STEP 1: Create Decision Table for EACH Operation**

| Operation | Independent Invoke? | Decision Before/After? | Same SOR? | Internal Lookup? | Conclusion | Reasoning |
|---|---|---|---|---|---|---|
| Leave Oracle Fusion Create (shape33) | YES | YES (AFTER - shape2 checks status) | N/A | NO | **Azure Function** | Process Layer needs to invoke this independently to create leave in Oracle Fusion. Decision shape2 is POST-OPERATION (checks response status), but this is error handling, NOT business logic. Process Layer will handle success/error responses. |
| Email Notification (shape21 - subprocess) | NO | YES (shape4 checks attachment flag) | N/A | NO | **NOT Azure Function** | This is error notification only, triggered by main process on errors. Process Layer does NOT need to invoke this independently. This is a cross-cutting concern that should be handled by System Layer internally or by a separate Email System Layer. |

**STEP 2: Answer 5 Questions for EACH Operation**

**Operation: Leave Oracle Fusion Create**
- **Q1:** Can Process Layer invoke independently? **YES** - Process Layer needs to create leave entries in Oracle Fusion
- **Q2:** Decision/conditional logic present? **YES** - Decision shape2 checks HTTP status
- **Q2a:** Is decision same SOR? **YES** - Decision checks response from same Oracle Fusion API call
- **Conclusion:** **1 Azure Function** - CreateLeave
- **Reasoning:** Decision shape2 is POST-OPERATION error handling (checks if API succeeded). This is NOT business logic (like "if entity exists, skip create"). The decision simply routes to success or error response mapping. Process Layer will receive either success or error response and handle accordingly. Handler can orchestrate internally: make API call → check status → map response.

**Operation: Email Notification (Subprocess)**
- **Q1:** Can Process Layer invoke independently? **NO** - This is internal error notification
- **Conclusion:** **NOT Azure Function** - Internal subprocess
- **Reasoning:** Email notification is triggered by main process errors. Process Layer does NOT need to invoke this. This is a System Layer internal concern or should be handled by a separate Email System Layer if email operations need to be exposed.

**STEP 3: Verification (7 Questions - ALL YES or N/A)**

1. ❓ Identified ALL decision points? **YES** - shape2 (status check), shape44 (content encoding), shape4 (attachment flag - subprocess)
2. ❓ WHERE each decision belongs? **YES** - shape2 and shape44 are POST-OPERATION error handling (System Layer), shape4 is subprocess internal logic
3. ❓ "if X exists, skip Y" checked? **N/A** - No existence check pattern in this process
4. ❓ "if flag=X, do Y" checked? **YES** - shape4 checks attachment flag (subprocess internal, NOT exposed as Function)
5. ❓ Can explain WHY each operation type? **YES** - CreateLeave is main business operation (Function), Email is internal notification (NOT Function)
6. ❓ Avoided pattern-matching? **YES** - Analyzed each operation individually
7. ❓ If 1 Function, NO decision shapes? **NO** - 1 Function, but decisions are POST-OPERATION error handling (allowed in Handler)

**STEP 4: Summary (MANDATORY)**

"I will create **1 Azure Function** for Oracle Fusion HCM: **CreateLeaveAPI**. Because the only independent operation Process Layer needs is creating leave entries in Oracle Fusion. Decision shape2 checks HTTP status (POST-OPERATION error handling), which is allowed in System Layer Handler - Handler will make API call, check status, and map to success or error response. Decision shape44 checks Content-Encoding (GZIP handling) - also POST-OPERATION error handling in Handler. Per Rule 1066, business decisions → Process Layer, but these are NOT business decisions - they are error handling and response transformation decisions that belong in System Layer Handler. The subprocess (Email Notification) is internal error notification, NOT exposed as Function. Process Layer does NOT need to invoke email independently - it's triggered by System Layer errors.

**Functions:** CreateLeaveAPI (exposes Oracle Fusion leave creation)  
**Internal:** Email notification subprocess (shape21), GZIP decompression (shape45), error mapping (shape39/shape40/shape41/shape46/shape47)  
**Auth:** Credentials-per-request (Basic Auth for Oracle Fusion, SMTP Auth for Email)"

### Function Exposure Summary

**Azure Functions to Create:**
1. **CreateLeaveAPI** - Create leave absence entry in Oracle Fusion HCM

**Internal Operations (NOT Functions):**
- Email notification (subprocess) - Triggered by System Layer on errors
- GZIP decompression - Handler internal logic
- Error response mapping - Handler internal logic
- Success response mapping - Handler internal logic

**Authentication Approach:**
- **Oracle Fusion HCM:** Credentials-per-request (Basic Auth) - NO middleware needed
- **Email SMTP:** Credentials-per-request (SMTP Auth) - NO middleware needed

---

## 20. Self-Check Questions - Final Validation

### BOOMI Extraction Rules Compliance

1. ❓ Did I analyze ALL map files? **YES** - 3 maps analyzed (Section 5)
2. ❓ Did I extract actual field names from maps? **YES** - Field mappings documented (Section 5)
3. ❓ Did I compare profile field names vs map field names? **YES** - Discrepancies documented (Section 5)
4. ❓ Did I mark map field names as AUTHORITATIVE? **YES** - Noted in Section 5
5. ❓ Did I extract HTTP status codes for all return paths? **YES** - Section 6
6. ❓ Did I document response JSON for each return path? **YES** - Section 6
7. ❓ Did I document populated fields for each return path? **YES** - Section 6
8. ❓ Did I extract HTTP status codes for downstream operations? **YES** - Section 6
9. ❓ Did I create request/response JSON examples? **YES** - Section 18
10. ❓ Did I verify business logic FIRST before following dragpoints? **YES** - Section 13 Step 0
11. ❓ Did I identify what each operation does and what it produces? **YES** - Section 4 and Section 13
12. ❓ Did I identify which operations MUST execute first based on business logic? **YES** - Section 13
13. ❓ Did I check data dependencies FIRST before following dragpoints? **YES** - Section 8 and Section 13
14. ❓ Did I use operation response analysis from Step 1c? **YES** - Referenced in Section 13
15. ❓ Did I use decision analysis from Step 7? **YES** - Referenced in Section 13 and Section 14
16. ❓ Did I use dependency graph from Step 4? **YES** - Referenced in Section 13 and Section 14
17. ❓ Did I use branch analysis from Step 8? **YES** - Referenced in Section 13 and Section 14
18. ❓ Did I verify all property reads happen after property writes? **YES** - Section 8 and Section 13
19. ❓ Did I classify each branch as parallel or sequential? **YES** - Section 11
20. ❓ Did I assume branches are parallel? **NO** - Analyzed dependencies (Section 11)
21. ❓ Did I extract properties read/written by each path? **YES** - Section 11
22. ❓ Did I build dependency graph between paths? **YES** - Section 11
23. ❓ Did I apply topological sort if sequential? **YES** - Section 11
24. ❓ Did I identify data source for EVERY decision? **YES** - Section 10
25. ❓ Did I classify each decision type? **YES** - Section 10
26. ❓ Did I verify actual execution order for decisions? **YES** - Section 10
27. ❓ Did I trace BOTH TRUE and FALSE paths for EVERY decision? **YES** - Section 10
28. ❓ Did I identify the pattern for each decision? **YES** - Section 10
29. ❓ Did I trace paths to termination? **YES** - Section 10

### ALL SELF-CHECK ANSWERS: YES

---

## 21. Phase 1 Completion Status

**Status:** ✅ COMPLETE

**All Mandatory Sections Present:**
- [x] Section 1: Operations Inventory
- [x] Section 2: Input Structure Analysis (Step 1a)
- [x] Section 3: Response Structure Analysis (Step 1b)
- [x] Section 4: Operation Response Analysis (Step 1c)
- [x] Section 5: Map Analysis (Step 1d)
- [x] Section 6: HTTP Status Codes and Return Path Responses (Step 1e)
- [x] Section 7: Process Properties Analysis (Steps 2-3)
- [x] Section 8: Data Dependency Graph (Step 4)
- [x] Section 9: Control Flow Graph (Step 5)
- [x] Section 10: Decision Shape Analysis (Step 7)
- [x] Section 11: Branch Shape Analysis (Step 8)
- [x] Section 12: Subprocess Analysis (Step 7a)
- [x] Section 13: Execution Order (Step 9)
- [x] Section 14: Sequence Diagram (Step 10)
- [x] Section 15: Critical Patterns Identified
- [x] Section 16: Validation Checklist
- [x] Section 17: System Layer Identification
- [x] Section 18: Request/Response JSON Examples
- [x] Section 19: Function Exposure Decision Table

**All Self-Check Questions Answered:** YES

**Ready for Phase 2:** ✅ YES

---

**END OF PHASE 1 EXTRACTION**
