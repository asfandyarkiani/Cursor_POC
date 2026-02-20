# BOOMI EXTRACTION PHASE 1: HCM Leave Create Process

**Process Name:** HCM_Leave Create  
**Process ID:** ca69f858-785f-4565-ba1f-b4edc6cca05b  
**Version:** 29  
**Description:** This Process will sync the Leave data between D365 and Oracle HCM  
**Business Domain:** Human Resource (HCM)  
**Date Analyzed:** 2026-02-18

---

## 1. Operations Inventory

### Main Process Operations

| Operation ID | Operation Name | Type | SubType | Purpose |
|---|---|---|---|---|
| 8f709c2b-e63f-4d5f-9374-2932ed70415d | Create Leave Oracle Fusion OP | connector-action | wss | Entry point - Web Service Server Listen |
| 6e8920fd-af5a-430b-a1d9-9fde7ac29a12 | Leave Oracle Fusion Create | connector-action | http | Create leave in Oracle Fusion HCM |
| af07502a-fafd-4976-a691-45d51a33b549 | Email w Attachment | connector-action | mail | Send email with attachment |
| 15a72a21-9b57-49a1-a8ed-d70367146644 | Email W/O Attachment | connector-action | mail | Send email without attachment |

### Subprocess Operations

**Subprocess:** (Sub) Office 365 Email (a85945c5-3004-42b9-80b1-104f465cd1fb)
- Contains email sending logic with attachment check
- Uses operations: af07502a-fafd-4976-a691-45d51a33b549 (with attachment), 15a72a21-9b57-49a1-a8ed-d70367146644 (without attachment)

### Connections

| Connection ID | Connection Name | Type | Purpose |
|---|---|---|---|
| aa1fcb29-d146-4425-9ea6-b9698090f60e | Oracle Fusion | http | Oracle Fusion HCM REST API connection |
| 00eae79b-2303-4215-8067-dcc299e42697 | Office 365 Email | mail | Office 365 SMTP email connection |

---

## 2. Input Structure Analysis (Step 1a)

### Entry Point Identification

**Entry Operation:** shape1 (START shape)  
**Operation ID:** 8f709c2b-e63f-4d5f-9374-2932ed70415d  
**Operation Name:** Create Leave Oracle Fusion OP  
**Operation Type:** connector-action (wss - Web Service Server)  
**Action Type:** Listen

### Request Profile Structure

**Profile ID:** febfa3e1-f719-4ee8-ba57-cdae34137ab3  
**Profile Name:** D365 Leave Create JSON Profile  
**Profile Type:** profile.json  
**Input Type:** singlejson  
**Root Structure:** Root/Object

### Input Format

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

### Array Detection

**Is Array:** NO  
**Structure Type:** Single JSON Object  
**Root Element:** Root/Object

### Document Processing Behavior

**Behavior:** single_document  
**Description:** Boomi processes single JSON document per execution  
**Execution Pattern:** single_execution  
**Session Management:** one_session_per_execution

### Field Extraction

**Total Fields:** 9 fields (all at root level)

| Field Name | Full Path | Data Type | Required | Mappable | Notes |
|---|---|---|---|---|---|
| employeeNumber | Root/Object/employeeNumber | number | Yes | Yes | Employee identifier (tracked field) |
| absenceType | Root/Object/absenceType | character | Yes | Yes | Type of absence/leave |
| employer | Root/Object/employer | character | Yes | Yes | Employer name |
| startDate | Root/Object/startDate | character | Yes | Yes | Leave start date |
| endDate | Root/Object/endDate | character | Yes | Yes | Leave end date |
| absenceStatusCode | Root/Object/absenceStatusCode | character | Yes | Yes | Absence status code |
| approvalStatusCode | Root/Object/approvalStatusCode | character | Yes | Yes | Approval status code |
| startDateDuration | Root/Object/startDateDuration | number | Yes | Yes | Start date duration |
| endDateDuration | Root/Object/endDateDuration | number | Yes | Yes | End date duration |

### Azure DTO Mapping (Process Layer)

**Process Layer DTO Name:** LeaveCreateRequestDto

| Boomi Field Path | Boomi Field Name | Data Type | Required | Azure DTO Property | C# Type |
|---|---|---|---|---|---|
| Root/Object/employeeNumber | employeeNumber | number | Yes | EmployeeNumber | int |
| Root/Object/absenceType | absenceType | character | Yes | AbsenceType | string |
| Root/Object/employer | employer | character | Yes | Employer | string |
| Root/Object/startDate | startDate | character | Yes | StartDate | string |
| Root/Object/endDate | endDate | character | Yes | EndDate | string |
| Root/Object/absenceStatusCode | absenceStatusCode | character | Yes | AbsenceStatusCode | string |
| Root/Object/approvalStatusCode | approvalStatusCode | character | Yes | ApprovalStatusCode | string |
| Root/Object/startDateDuration | startDateDuration | number | Yes | StartDateDuration | int |
| Root/Object/endDateDuration | endDateDuration | number | Yes | EndDateDuration | int |

---

## 3. Response Structure Analysis (Step 1b)

### Response Profile Structure

**Profile ID:** f4ca3a70-114a-4601-bad8-44a3eb20e2c0  
**Profile Name:** Leave D365 Response  
**Profile Type:** profile.json  
**Root Structure:** leaveResponse/Object

### Response Format

```json
{
  "leaveResponse": {
    "status": "success",
    "message": "Data successfully sent to Oracle Fusion",
    "personAbsenceEntryId": 12345,
    "success": "true"
  }
}
```

### Response Field Extraction

**Total Fields:** 4 fields

| Field Name | Full Path | Data Type | Mappable | Notes |
|---|---|---|---|---|
| status | leaveResponse/Object/status | character | Yes | Success/failure status |
| message | leaveResponse/Object/message | character | Yes | Response message |
| personAbsenceEntryId | leaveResponse/Object/personAbsenceEntryId | number | Yes | Created absence entry ID from Oracle |
| success | leaveResponse/Object/success | character | Yes | Boolean success indicator |

### Azure DTO Mapping (Process Layer Response)

**Process Layer Response DTO Name:** LeaveCreateResponseDto

| Boomi Field Path | Boomi Field Name | Data Type | Azure DTO Property | C# Type |
|---|---|---|---|---|
| leaveResponse/Object/status | status | character | Status | string |
| leaveResponse/Object/message | message | character | Message | string |
| leaveResponse/Object/personAbsenceEntryId | personAbsenceEntryId | number | PersonAbsenceEntryId | long |
| leaveResponse/Object/success | success | character | Success | bool |

---

## 4. Operation Response Analysis (Step 1c)

### Operation: Leave Oracle Fusion Create (6e8920fd-af5a-430b-a1d9-9fde7ac29a12)

**Operation Type:** HTTP POST  
**Purpose:** Create leave absence entry in Oracle Fusion HCM  
**Request Profile:** NONE (request built from map)  
**Response Profile:** NONE (response profile not explicitly defined in operation)  
**Method:** POST  
**Content Type:** application/json  
**Return Errors:** true  
**Return Responses:** true

**Response Header Mapping:**
- Header: Content-Encoding → Target Property: DDP_RespHeader

**Expected Response Structure:**

Based on profile_316175c7-0e45-4869-9ac6-5f9d69882a62 (Oracle Fusion Leave Response JSON Profile), the operation returns:

```json
{
  "personAbsenceEntryId": 12345,
  "absenceCaseId": "...",
  "absenceTypeId": 123,
  "personId": 456,
  "startDate": "2024-03-24",
  "endDate": "2024-03-25",
  "absenceStatusCd": "SUBMITTED",
  "approvalStatusCd": "APPROVED",
  "duration": 2,
  "links": [...]
}
```

**Extracted Fields:**

The operation response is NOT explicitly extracted to process properties. The response document flows through the process and is mapped to the final response profile.

**Track Properties Written:**
- `meta.base.applicationstatuscode` - HTTP status code from Oracle Fusion API
- `dynamicdocument.DDP_RespHeader` - Content-Encoding header value

**Consumers:**
- shape2 (Decision: HTTP Status 20 check) - Reads `meta.base.applicationstatuscode`
- shape44 (Decision: Check Response Content Type) - Reads `dynamicdocument.DDP_RespHeader`

**Business Logic:**
- Operation executes HTTP POST to Oracle Fusion HCM
- Response status code determines success/failure path
- If status is 20x → Success path (map response and return)
- If status is NOT 20x → Error path (extract error message and return)

---

## 5. Map Analysis (Step 1d)

### Map Inventory

| Map ID | Map Name | From Profile | To Profile | Type |
|---|---|---|---|---|
| c426b4d6-2aff-450e-b43b-59956c4dbc96 | Leave Create Map | febfa3e1-f719-4ee8-ba57-cdae34137ab3 | a94fa205-c740-40a5-9fda-3d018611135a | Request Map (D365 → Oracle Fusion) |
| e4fd3f59-edb5-43a1-aeae-143b600a064e | Oracle Fusion Leave Response Map | 316175c7-0e45-4869-9ac6-5f9d69882a62 | f4ca3a70-114a-4601-bad8-44a3eb20e2c0 | Success Response Map |
| f46b845a-7d75-41b5-b0ad-c41a6a8e9b12 | Leave Error Map | 23d7a2e9-5cb0-4e9c-9e4b-3154834bad0d | f4ca3a70-114a-4601-bad8-44a3eb20e2c0 | Error Response Map |

### Map 1: Leave Create Map (c426b4d6-2aff-450e-b43b-59956c4dbc96)

**Purpose:** Transform D365 leave request to Oracle Fusion HCM format  
**From Profile:** febfa3e1-f719-4ee8-ba57-cdae34137ab3 (D365 Leave Create JSON Profile)  
**To Profile:** a94fa205-c740-40a5-9fda-3d018611135a (HCM Leave Create JSON Profile)  
**Used By:** shape29 (Map shape in main process)

**Field Mappings:**

| Source Field (D365) | Source Path | Target Field (Oracle) | Target Path | Transformation |
|---|---|---|---|---|
| employeeNumber | Root/Object/employeeNumber | personNumber | Root/Object/personNumber | Direct mapping (number) |
| absenceType | Root/Object/absenceType | absenceType | Root/Object/absenceType | Direct mapping |
| employer | Root/Object/employer | employer | Root/Object/employer | Direct mapping |
| startDate | Root/Object/startDate | startDate | Root/Object/startDate | Direct mapping |
| endDate | Root/Object/endDate | endDate | Root/Object/endDate | Direct mapping |
| absenceStatusCode | Root/Object/absenceStatusCode | absenceStatusCd | Root/Object/absenceStatusCd | Field name change |
| approvalStatusCode | Root/Object/approvalStatusCode | approvalStatusCd | Root/Object/approvalStatusCd | Field name change |
| startDateDuration | Root/Object/startDateDuration | startDateDuration | Root/Object/startDateDuration | Direct mapping |
| endDateDuration | Root/Object/endDateDuration | endDateDuration | Root/Object/endDateDuration | Direct mapping |

**Field Name Discrepancies:**

| D365 Field Name | Oracle Field Name | Authority | Notes |
|---|---|---|---|
| absenceStatusCode | absenceStatusCd | ✅ MAP | Map transforms "Code" to "Cd" |
| approvalStatusCode | approvalStatusCd | ✅ MAP | Map transforms "Code" to "Cd" |

**Scripting Functions:** None

**Static Values:** None

**Process Properties:** None used in this map

### Map 2: Oracle Fusion Leave Response Map (e4fd3f59-edb5-43a1-aeae-143b600a064e)

**Purpose:** Transform Oracle Fusion success response to D365 response format  
**From Profile:** 316175c7-0e45-4869-9ac6-5f9d69882a62 (Oracle Fusion Leave Response JSON Profile)  
**To Profile:** f4ca3a70-114a-4601-bad8-44a3eb20e2c0 (Leave D365 Response)  
**Used By:** shape34 (Map shape - success path)

**Field Mappings:**

| Source Field (Oracle) | Source Path | Target Field (D365) | Target Path | Transformation |
|---|---|---|---|---|
| personAbsenceEntryId | Root/Object/personAbsenceEntryId | personAbsenceEntryId | leaveResponse/Object/personAbsenceEntryId | Direct mapping |

**Default Values (Static):**

| Target Field | Target Key | Static Value | Notes |
|---|---|---|---|
| status | 4 | "success" | Success indicator |
| message | 5 | "Data successfully sent to Oracle Fusion" | Success message |
| success | 7 | "true" | Boolean success flag |

### Map 3: Leave Error Map (f46b845a-7d75-41b5-b0ad-c41a6a8e9b12)

**Purpose:** Transform error information to D365 error response format  
**From Profile:** 23d7a2e9-5cb0-4e9c-9e4b-3154834bad0d (Dummy FF Profile)  
**To Profile:** f4ca3a70-114a-4601-bad8-44a3eb20e2c0 (Leave D365 Response)  
**Used By:** shape40, shape41, shape47 (Error path map shapes)

**Field Mappings:**

| Source | Source Type | Target Field | Target Path | Transformation |
|---|---|---|---|---|
| DPP_ErrorMessage | function (PropertyGet) | message | leaveResponse/Object/message | Process property → message field |

**Function Analysis:**

**Function 1: PropertyGet**
- **Type:** PropertyGet (Get Dynamic Process Property)
- **Input:** Property Name = "DPP_ErrorMessage"
- **Output:** Result (key=3)
- **Purpose:** Retrieve error message from process property

**Default Values (Static):**

| Target Field | Target Key | Static Value | Notes |
|---|---|---|---|
| status | 4 | "failure" | Failure indicator |
| success | 7 | "false" | Boolean failure flag |

**CRITICAL RULE:** Map field names are AUTHORITATIVE. Use map field names in HTTP requests, NOT profile field names.

---

## 6. HTTP Status Codes and Return Path Responses (Step 1e)

### Return Path Inventory

| Return Shape ID | Return Label | HTTP Status Code | Path Type | Conditions |
|---|---|---|---|---|
| shape35 | Success Response | 200 | SUCCESS | HTTP status 20* (success) |
| shape36 | Error Response | 400 | ERROR | HTTP status NOT 20* AND response content type is NOT gzip |
| shape43 | Error Response | 500 | ERROR | Try/Catch error (exception in main flow) |
| shape48 | Error Response | 400 | ERROR | HTTP status NOT 20* AND response content type is gzip |

### Return Path 1: Success Response (shape35)

**Return Label:** "Success Response"  
**Return Shape ID:** shape35  
**HTTP Status Code:** 200  
**Path Type:** SUCCESS

**Decision Conditions Leading to Return:**
1. Decision shape2 (HTTP Status 20 check): `meta.base.applicationstatuscode` matches wildcard "20*" → TRUE path

**Populated Response Fields:**

| Field Name | Field Path | Source | Populated By | Value Type |
|---|---|---|---|---|
| status | leaveResponse/Object/status | static (map default) | Map e4fd3f59 | "success" |
| message | leaveResponse/Object/message | static (map default) | Map e4fd3f59 | "Data successfully sent to Oracle Fusion" |
| personAbsenceEntryId | leaveResponse/Object/personAbsenceEntryId | operation_response | Operation 6e8920fd (Oracle Fusion Create) | From Oracle API response |
| success | leaveResponse/Object/success | static (map default) | Map e4fd3f59 | "true" |

**Response JSON Example:**

```json
{
  "leaveResponse": {
    "status": "success",
    "message": "Data successfully sent to Oracle Fusion",
    "personAbsenceEntryId": 300100559876543,
    "success": "true"
  }
}
```

### Return Path 2: Error Response - Non-Gzip (shape36)

**Return Label:** "Error Response"  
**Return Shape ID:** shape36  
**HTTP Status Code:** 400  
**Path Type:** ERROR

**Decision Conditions Leading to Return:**
1. Decision shape2 (HTTP Status 20 check): `meta.base.applicationstatuscode` does NOT match "20*" → FALSE path
2. Decision shape44 (Check Response Content Type): `dynamicdocument.DDP_RespHeader` does NOT equal "gzip" → FALSE path

**Populated Response Fields:**

| Field Name | Field Path | Source | Populated By | Value Type |
|---|---|---|---|---|
| status | leaveResponse/Object/status | static (map default) | Map f46b845a | "failure" |
| message | leaveResponse/Object/message | process_property | process.DPP_ErrorMessage (from meta.base.applicationstatusmessage) | Error message from Oracle API |
| success | leaveResponse/Object/success | static (map default) | Map f46b845a | "false" |

**Response JSON Example:**

```json
{
  "leaveResponse": {
    "status": "failure",
    "message": "Oracle Fusion API error: Invalid absence type",
    "success": "false"
  }
}
```

### Return Path 3: Error Response - Try/Catch (shape43)

**Return Label:** "Error Response"  
**Return Shape ID:** shape43  
**HTTP Status Code:** 500  
**Path Type:** ERROR

**Decision Conditions Leading to Return:**
1. Try/Catch shape17: Error occurs in Try block → Catch path (shape20 branch)
2. Branch shape20: Path 2 → shape41 (error map) → shape43 (return)

**Populated Response Fields:**

| Field Name | Field Path | Source | Populated By | Value Type |
|---|---|---|---|---|
| status | leaveResponse/Object/status | static (map default) | Map f46b845a | "failure" |
| message | leaveResponse/Object/message | process_property | process.DPP_ErrorMessage (from meta.base.catcherrorsmessage) | Exception message |
| success | leaveResponse/Object/success | static (map default) | Map f46b845a | "false" |

**Response JSON Example:**

```json
{
  "leaveResponse": {
    "status": "failure",
    "message": "Process execution error: Connection timeout",
    "success": "false"
  }
}
```

### Return Path 4: Error Response - Gzip (shape48)

**Return Label:** "Error Response"  
**Return Shape ID:** shape48  
**HTTP Status Code:** 400  
**Path Type:** ERROR

**Decision Conditions Leading to Return:**
1. Decision shape2 (HTTP Status 20 check): `meta.base.applicationstatuscode` does NOT match "20*" → FALSE path
2. Decision shape44 (Check Response Content Type): `dynamicdocument.DDP_RespHeader` equals "gzip" → TRUE path
3. Decompress gzip response (shape45 - dataprocess)
4. Extract error message (shape46)
5. Map error (shape47) → Return (shape48)

**Populated Response Fields:**

| Field Name | Field Path | Source | Populated By | Value Type |
|---|---|---|---|---|
| status | leaveResponse/Object/status | static (map default) | Map f46b845a | "failure" |
| message | leaveResponse/Object/message | process_property | process.DPP_ErrorMessage (from meta.base.applicationstatusmessage after decompression) | Error message from decompressed Oracle API response |
| success | leaveResponse/Object/success | static (map default) | Map f46b845a | "false" |

**Response JSON Example:**

```json
{
  "leaveResponse": {
    "status": "failure",
    "message": "Oracle Fusion API error: Validation failed - Invalid employee number",
    "success": "false"
  }
}
```

### Downstream Operations HTTP Status Codes

| Operation Name | Operation ID | Expected Success Codes | Error Codes | Error Handling |
|---|---|---|---|---|
| Leave Oracle Fusion Create | 6e8920fd-af5a-430b-a1d9-9fde7ac29a12 | 200, 201 | 400, 401, 403, 404, 500 | Return error response with message |

---

## 7. Process Properties Analysis (Steps 2-3)

### Property WRITES

| Property Name | Written By Shape(s) | Source | Notes |
|---|---|---|---|
| process.DPP_Process_Name | shape38 | Execution property (Process Name) | Process name for logging |
| process.DPP_AtomName | shape38 | Execution property (Atom Name) | Atom name for logging |
| process.DPP_Payload | shape38 | Current document | Input payload for error reporting |
| process.DPP_ExecutionID | shape38 | Execution property (Execution Id) | Execution ID for logging |
| process.DPP_File_Name | shape38 | Execution property + date + ".txt" | File name for email attachment |
| process.DPP_Subject | shape38 | Atom Name + Process Name + " has errors to report" | Email subject |
| process.To_Email | shape38 | Defined parameter (PP_HCM_LeaveCreate_Properties.To_Email) | Email recipient |
| process.DPP_HasAttachment | shape38 | Defined parameter (PP_HCM_LeaveCreate_Properties.DPP_HasAttachment) | Attachment flag |
| dynamicdocument.URL | shape8 | Defined parameter (PP_HCM_LeaveCreate_Properties.Resource_Path) | Oracle Fusion API resource path |
| process.DPP_ErrorMessage | shape19 | Track property (meta.base.catcherrorsmessage) | Try/Catch error message |
| process.DPP_ErrorMessage | shape39 | Track property (meta.base.applicationstatusmessage) | HTTP error message (non-gzip) |
| process.DPP_ErrorMessage | shape46 | Current document (meta.base.applicationstatusmessage) | HTTP error message (gzip decompressed) |

### Property READS

| Property Name | Read By Shape(s) | Shape Type | Usage |
|---|---|---|---|
| dynamicdocument.URL | shape33 (operation 6e8920fd) | connectoraction | HTTP request URL path element |
| process.DPP_ErrorMessage | Map f46b845a (Function 1) | map | Error message for response |
| process.To_Email | Subprocess shape6, shape20 | documentproperties | Email recipient (subprocess) |
| process.DPP_Subject | Subprocess shape6, shape20 | documentproperties | Email subject (subprocess) |
| process.DPP_MailBody | Subprocess shape6, shape20 | documentproperties | Email body (subprocess) |
| process.DPP_File_Name | Subprocess shape6 | documentproperties | Email attachment filename (subprocess) |
| process.DPP_Process_Name | Subprocess shape11, shape23 | message | Process name in email body (subprocess) |
| process.DPP_AtomName | Subprocess shape11, shape23 | message | Atom name in email body (subprocess) |
| process.DPP_ExecutionID | Subprocess shape11, shape23 | message | Execution ID in email body (subprocess) |
| process.DPP_ErrorMessage | Subprocess shape11, shape23 | message | Error message in email body (subprocess) |
| process.DPP_Payload | Subprocess shape15 | message | Payload for email attachment (subprocess) |
| DPP_HasAttachment | Subprocess shape4 | decision | Check if email has attachment (subprocess) |

### Defined Process Properties

**Component:** PP_HCM_LeaveCreate_Properties (e22c04db-7dfa-4b1b-9d88-77365b0fdb18)

| Property Key | Property Name | Type | Default Value | Used By |
|---|---|---|---|---|
| c2d1a1e8-4bcb-435b-b1d2-bb10a209bcc0 | Resource_Path | string | "hcmRestApi/resources/11.13.18.05/absences" | shape8 → dynamicdocument.URL |
| 71c90f6e-4f86-49f3-8aef-bae6668c73f9 | To_Email | string | "BoomiIntegrationTeam@al-ghurair.com" | shape38 → process.To_Email |
| a717867b-67f4-4a72-be2d-c99c73309fdb | DPP_HasAttachment | string | "Y" | shape38 → process.DPP_HasAttachment |

**Component:** PP_Office365_Email (0feff13f-8a2c-438a-b3c7-1909e2a7f533)

| Property Key | Property Name | Type | Default Value | Used By |
|---|---|---|---|---|
| 804b6d40-eeee-4ac0-ab4b-94e1aca9f4b7 | From_Email | string | "Boomi.Dev.failures@al-ghurair.com" | Subprocess (email from address) |
| d8fc7496-ec2c-4308-a4ca-e9f49c0c9500 | Environment | string | "DEV Failure :" | Subprocess (email subject prefix) |

---

## 8. Data Dependency Graph (Step 4)

### Property Dependency Chains

**Chain 1: Logging Properties (Written Early, Read in Error Paths)**

```
shape38 (Input_details) WRITES:
  - process.DPP_Process_Name
  - process.DPP_AtomName
  - process.DPP_Payload
  - process.DPP_ExecutionID
  - process.DPP_File_Name
  - process.DPP_Subject
  - process.To_Email
  - process.DPP_HasAttachment

READ BY (in subprocess a85945c5):
  - Subprocess shape11, shape23 (message shapes) READ: DPP_Process_Name, DPP_AtomName, DPP_ExecutionID, DPP_ErrorMessage
  - Subprocess shape15 (message shape) READ: DPP_Payload
  - Subprocess shape6, shape20 (documentproperties) READ: To_Email, DPP_Subject, DPP_MailBody, DPP_File_Name
  - Subprocess shape4 (decision) READ: DPP_HasAttachment

DEPENDENCY: shape38 MUST execute BEFORE subprocess shape21 (if error path is taken)
```

**Chain 2: URL Configuration**

```
shape8 (set URL) WRITES:
  - dynamicdocument.URL (from defined parameter Resource_Path)

READ BY:
  - shape33 (operation 6e8920fd - Leave Oracle Fusion Create) READS: dynamicdocument.URL

DEPENDENCY: shape8 MUST execute BEFORE shape33
```

**Chain 3: Error Message Flow**

```
WRITE PATH 1 (Try/Catch Error):
  shape19 (ErrorMsg) WRITES:
    - process.DPP_ErrorMessage (from meta.base.catcherrorsmessage)

WRITE PATH 2 (HTTP Error - Non-Gzip):
  shape39 (error msg) WRITES:
    - process.DPP_ErrorMessage (from meta.base.applicationstatusmessage)

WRITE PATH 3 (HTTP Error - Gzip):
  shape46 (error msg) WRITES:
    - process.DPP_ErrorMessage (from meta.base.applicationstatusmessage after decompression)

READ BY:
  - Map f46b845a (Function 1: PropertyGet) READS: process.DPP_ErrorMessage
  - Subprocess shape11, shape23 (message shapes) READ: DPP_ErrorMessage

DEPENDENCY: Error message must be written BEFORE error map executes
```

### Dependency Summary

**Independent Operations:**
- shape38 (Input_details) - No dependencies, executes first
- shape17 (Try/Catch) - Wraps main flow, no property dependencies

**Dependent Operations:**
- shape8 (set URL) depends on: None (uses defined parameter)
- shape33 (HTTP operation) depends on: shape8 (needs dynamicdocument.URL)
- shape2 (HTTP Status check) depends on: shape33 (needs meta.base.applicationstatuscode)
- Error map shapes (shape40, shape41, shape47) depend on: Error message writers (shape19, shape39, shape46)
- Subprocess shape21 depends on: shape38 (needs all logging properties if executed)

---

## 9. Control Flow Graph (Step 5)

### Main Process Control Flow

**Total Shapes:** 14 shapes  
**Total Connections:** 18 dragpoint connections

| Source Shape | Shape Type | Destination Shape(s) | Identifier | Notes |
|---|---|---|---|---|
| shape1 (START) | start | shape38 | default | Entry point |
| shape38 (Input_details) | documentproperties | shape17 | default | Set logging properties |
| shape17 (Try/Catch) | catcherrors | shape29 (Try), shape20 (Catch) | default, error | Error handling wrapper |
| shape29 (Map) | map | shape8 | default | Transform D365 → Oracle |
| shape8 (set URL) | documentproperties | shape49 | default | Set API URL |
| shape49 (Notify) | notify | shape33 | default | Log request |
| shape33 (HTTP Operation) | connectoraction | shape2 | default | Call Oracle Fusion API |
| shape2 (HTTP Status check) | decision | shape34 (TRUE), shape44 (FALSE) | true, false | Check HTTP status 20* |
| shape34 (Success Map) | map | shape35 | default | Map success response |
| shape35 (Success Response) | returndocuments | NONE | - | Terminal (success) |
| shape44 (Content Type check) | decision | shape45 (TRUE), shape39 (FALSE) | true, false | Check if gzip |
| shape45 (Decompress) | dataprocess | shape46 | default | Decompress gzip response |
| shape46 (Extract error) | documentproperties | shape47 | default | Extract error message |
| shape47 (Error Map) | map | shape48 | default | Map error response |
| shape48 (Error Response) | returndocuments | NONE | - | Terminal (error - gzip) |
| shape39 (Extract error) | documentproperties | shape40 | default | Extract error message |
| shape40 (Error Map) | map | shape36 | default | Map error response |
| shape36 (Error Response) | returndocuments | NONE | - | Terminal (error - non-gzip) |
| shape20 (Branch) | branch | shape19 (Path 1), shape41 (Path 2) | 1, 2 | Error handling branch |
| shape19 (ErrorMsg) | documentproperties | shape21 | default | Extract catch error message |
| shape21 (Email Subprocess) | processcall | NONE | - | Send error email (subprocess) |
| shape41 (Error Map) | map | shape43 | default | Map error response |
| shape43 (Error Response) | returndocuments | NONE | - | Terminal (catch error) |

### Subprocess Control Flow (a85945c5-3004-42b9-80b1-104f465cd1fb)

**Total Shapes:** 10 shapes  
**Total Connections:** 11 dragpoint connections

| Source Shape | Shape Type | Destination Shape(s) | Identifier | Notes |
|---|---|---|---|---|
| shape1 (START) | start | shape2 | default | Subprocess entry |
| shape2 (Try/Catch) | catcherrors | shape4 (Try), shape10 (Catch) | default, error | Error handling |
| shape4 (Attachment_Check) | decision | shape11 (TRUE), shape23 (FALSE) | true, false | Check DPP_HasAttachment == "Y" |
| shape11 (Mail_Body) | message | shape14 | default | Build email body with attachment |
| shape14 (set_MailBody) | documentproperties | shape15 | default | Store email body |
| shape15 (payload) | message | shape6 | default | Add payload as attachment |
| shape6 (set_Mail_Properties) | documentproperties | shape3 | default | Set email properties (with attachment) |
| shape3 (Email) | connectoraction | shape5 | default | Send email with attachment |
| shape5 (Stop) | stop | NONE | - | Terminal (success) |
| shape23 (Mail_Body) | message | shape22 | default | Build email body without attachment |
| shape22 (set_MailBody) | documentproperties | shape20 | default | Store email body |
| shape20 (set_Mail_Properties) | documentproperties | shape7 | default | Set email properties (no attachment) |
| shape7 (Email) | connectoraction | shape9 | default | Send email without attachment |
| shape9 (Stop) | stop | NONE | - | Terminal (success) |
| shape10 (Exception) | exception | NONE | - | Terminal (error) |

### Reverse Flow Mapping (Step 6)

**Convergence Points:** NONE identified in main process or subprocess

**Multiple Incoming Connections:**

Main Process:
- NONE (no convergence - all paths terminate independently)

Subprocess:
- NONE (decision paths terminate independently at different Stop shapes)

---

## 10. Decision Shape Analysis (Step 7)

### Decision Inventory

**Total Decisions:** 3 (2 in main process, 1 in subprocess)

### Decision 1: HTTP Status 20 check (shape2)

**Shape ID:** shape2  
**Location:** Main process  
**User Label:** "HTTP Status 20 check"

**Comparison:**
- **Type:** wildcard
- **Value 1:** `meta.base.applicationstatuscode` (track property - HTTP status code from operation)
- **Value 2:** "20*" (static - matches 200, 201, 202, etc.)

**Data Source Analysis:**
- **Data Source:** TRACK_PROPERTY (meta.base.applicationstatuscode)
- **Source Operation:** shape33 (operation 6e8920fd - Leave Oracle Fusion Create)
- **Data Type:** HTTP response status code

**Decision Type Classification:**
- **Type:** POST_OPERATION
- **Reasoning:** Checks track property (meta.base.applicationstatuscode) which is set by HTTP operation response

**Actual Execution Order:**
```
Operation shape33 (HTTP POST) → Response received → Track property set → Decision shape2 checks status → Route based on result
```

**Paths:**

**TRUE Path (Status is 20x - Success):**
- **Destination:** shape34 (Map - Oracle Fusion Leave Response Map)
- **Termination:** shape35 (Return Documents - "Success Response")
- **Type:** SUCCESS path
- **Actions:** Map Oracle response → Return success response

**FALSE Path (Status is NOT 20x - Error):**
- **Destination:** shape44 (Decision - Check Response Content Type)
- **Termination:** shape36 or shape48 (Return Documents - "Error Response")
- **Type:** ERROR path
- **Actions:** Check if response is gzip → Decompress if needed → Extract error → Map error → Return error response

**Pattern:** Error Check (Success vs Failure based on HTTP status)

**Convergence:** NONE (paths terminate independently)

**Early Exit:** Both paths terminate (return documents)

### Decision 2: Check Response Content Type (shape44)

**Shape ID:** shape44  
**Location:** Main process (FALSE path of shape2)  
**User Label:** "Check Response Content Type"

**Comparison:**
- **Type:** equals
- **Value 1:** `dynamicdocument.DDP_RespHeader` (document property - Content-Encoding header)
- **Value 2:** "gzip" (static)

**Data Source Analysis:**
- **Data Source:** TRACK_PROPERTY (dynamicdocument.DDP_RespHeader)
- **Source Operation:** shape33 (operation 6e8920fd - response header mapping)
- **Data Type:** HTTP response header value

**Decision Type Classification:**
- **Type:** POST_OPERATION
- **Reasoning:** Checks document property set by HTTP operation response header mapping

**Actual Execution Order:**
```
Operation shape33 (HTTP POST) → Response received → Header mapped to DDP_RespHeader → Decision shape44 checks header → Route based on result
```

**Paths:**

**TRUE Path (Response is gzip compressed):**
- **Destination:** shape45 (DataProcess - Decompress gzip)
- **Termination:** shape48 (Return Documents - "Error Response")
- **Type:** ERROR path with decompression
- **Actions:** Decompress gzip → Extract error message → Map error → Return error response

**FALSE Path (Response is NOT gzip):**
- **Destination:** shape39 (DocumentProperties - Extract error message)
- **Termination:** shape36 (Return Documents - "Error Response")
- **Type:** ERROR path without decompression
- **Actions:** Extract error message → Map error → Return error response

**Pattern:** Conditional Logic (Handle compressed vs uncompressed error responses)

**Convergence:** NONE (paths terminate independently)

**Early Exit:** Both paths terminate (return documents)

### Decision 3: Attachment_Check (Subprocess shape4)

**Shape ID:** shape4  
**Location:** Subprocess a85945c5-3004-42b9-80b1-104f465cd1fb  
**User Label:** "Attachment_Check"

**Comparison:**
- **Type:** equals
- **Value 1:** `process.DPP_HasAttachment` (process property)
- **Value 2:** "Y" (static)

**Data Source Analysis:**
- **Data Source:** PROCESS_PROPERTY (DPP_HasAttachment)
- **Source:** Main process shape38 (written from defined parameter)
- **Data Type:** String flag ("Y" or "N")

**Decision Type Classification:**
- **Type:** PRE_FILTER
- **Reasoning:** Checks process property set by main process before subprocess execution

**Actual Execution Order:**
```
Main process shape38 writes DPP_HasAttachment → Subprocess called → Decision shape4 checks flag → Route to appropriate email path
```

**Paths:**

**TRUE Path (Has attachment = "Y"):**
- **Destination:** shape11 (Message - Build email body with attachment)
- **Termination:** shape5 (Stop - continue=true - SUCCESS)
- **Type:** Email with attachment path
- **Actions:** Build email body → Set body property → Add payload attachment → Set mail properties → Send email with attachment → Stop (success)

**FALSE Path (Has attachment != "Y"):**
- **Destination:** shape23 (Message - Build email body without attachment)
- **Termination:** shape9 (Stop - continue=true - SUCCESS)
- **Type:** Email without attachment path
- **Actions:** Build email body → Set body property → Set mail properties → Send email without attachment → Stop (success)

**Pattern:** Conditional Logic (Optional Processing - with/without attachment)

**Convergence:** NONE (paths terminate independently at different Stop shapes)

**Early Exit:** NONE (both paths complete successfully)

### Decision Analysis Self-Check

✅ **Decision data sources identified:** YES
- shape2: TRACK_PROPERTY (meta.base.applicationstatuscode)
- shape44: TRACK_PROPERTY (dynamicdocument.DDP_RespHeader)
- Subprocess shape4: PROCESS_PROPERTY (DPP_HasAttachment)

✅ **Decision types classified:** YES
- shape2: POST_OPERATION (checks HTTP response status)
- shape44: POST_OPERATION (checks HTTP response header)
- Subprocess shape4: PRE_FILTER (checks input parameter)

✅ **Execution order verified:** YES
- shape2 executes AFTER shape33 (HTTP operation)
- shape44 executes AFTER shape33 (HTTP operation)
- Subprocess shape4 executes AFTER main process shape38 (property write)

✅ **All decision paths traced:** YES
- shape2: TRUE → shape34 → shape35 (terminal), FALSE → shape44 → ... → shape36/shape48 (terminal)
- shape44: TRUE → shape45 → shape46 → shape47 → shape48 (terminal), FALSE → shape39 → shape40 → shape36 (terminal)
- Subprocess shape4: TRUE → shape11 → ... → shape5 (terminal), FALSE → shape23 → ... → shape9 (terminal)

✅ **Decision patterns identified:** YES
- shape2: Error Check (Success vs Failure)
- shape44: Conditional Logic (Compressed vs Uncompressed)
- Subprocess shape4: Conditional Logic (With/Without Attachment)

✅ **Paths traced to termination:** YES
- All paths traced to Return Documents or Stop shapes

---

## 11. Branch Shape Analysis (Step 8)

### Branch Inventory

**Total Branches:** 1 (in main process error path)

### Branch 1: Error Handling Branch (shape20)

**Shape ID:** shape20  
**Location:** Main process (Catch path of shape17)  
**User Label:** (none)  
**Number of Paths:** 2

**Branch Paths:**

**Path 1 (Identifier: "1"):**
- **Destination:** shape19 (DocumentProperties - ErrorMsg)
- **Flow:** shape19 → shape21 (ProcessCall - Email Subprocess)
- **Terminal:** Subprocess execution (no explicit return to main process)
- **Purpose:** Extract catch error message → Send error email

**Path 2 (Identifier: "2"):**
- **Destination:** shape41 (Map - Leave Error Map)
- **Flow:** shape41 → shape43 (Return Documents - Error Response)
- **Terminal:** shape43 (Return Documents)
- **Purpose:** Map error response → Return error to caller

### Properties Analysis (Step 1)

**Path 1 Properties:**

WRITES:
- process.DPP_ErrorMessage (shape19 - from meta.base.catcherrorsmessage)

READS:
- process.To_Email (subprocess - for email recipient)
- process.DPP_Subject (subprocess - for email subject)
- process.DPP_Process_Name (subprocess - for email body)
- process.DPP_AtomName (subprocess - for email body)
- process.DPP_ExecutionID (subprocess - for email body)
- process.DPP_ErrorMessage (subprocess - for email body)
- process.DPP_Payload (subprocess - for email attachment)
- process.DPP_File_Name (subprocess - for attachment filename)
- process.DPP_HasAttachment (subprocess - for attachment check)

**Path 2 Properties:**

WRITES:
- NONE

READS:
- process.DPP_ErrorMessage (Map f46b845a - Function 1: PropertyGet)

### Dependency Graph (Step 2)

**Dependency Analysis:**

```
Path 1 (shape19 → shape21):
  - WRITES: process.DPP_ErrorMessage
  - READS: All logging properties (written by shape38)

Path 2 (shape41 → shape43):
  - READS: process.DPP_ErrorMessage
  - DEPENDS ON: Path 1 writing process.DPP_ErrorMessage? NO - Path 2 can use error message from shape19 if Path 1 executed first

CRITICAL: Path 2 READS process.DPP_ErrorMessage which Path 1 WRITES
```

**Dependency Graph:**
```
Path 1 (shape19) → Path 2 (shape41)
Reasoning: Path 2 reads process.DPP_ErrorMessage which Path 1 writes
```

### Classification (Step 3)

**API Call Detection:**
- Path 1: Contains API call (subprocess shape3/shape7 - email operation)
- Path 2: NO API calls (only map and return)

**Classification:** SEQUENTIAL

**Reasoning:**
1. Path 1 contains API calls (email operations in subprocess) → SEQUENTIAL execution required
2. Path 2 depends on Path 1 (reads process.DPP_ErrorMessage written by Path 1)
3. Both conditions require SEQUENTIAL execution

### Topological Sort Order (Step 4)

**Dependency Order:**
```
Path 1 (shape19 → shape21) → Path 2 (shape41 → shape43)
```

**Execution Sequence:**
1. Execute Path 1: Extract error message → Send error email (subprocess)
2. Execute Path 2: Map error response → Return error to caller

### Path Termination (Step 5)

| Path | First Shape | Terminal Shape | Terminal Type |
|---|---|---|---|
| Path 1 | shape19 | Subprocess (no explicit return) | Subprocess execution |
| Path 2 | shape41 | shape43 | Return Documents |

### Convergence Points (Step 6)

**Convergence:** NONE

**Reasoning:** Path 1 calls subprocess (no return to main process), Path 2 returns documents. Paths do not converge.

### Execution Continuation (Step 7)

**Execution Continues From:** NONE

**Reasoning:** Path 2 terminates with Return Documents. Path 1 calls subprocess but does not return to main process flow.

### Branch Analysis Self-Check

✅ **Classification completed:** YES - SEQUENTIAL

✅ **Assumption check:** NO (analyzed dependencies)
- Checked data dependencies: Path 2 reads property written by Path 1
- Checked API calls: Path 1 contains email API calls

✅ **Properties extracted:** YES
- Path 1 WRITES: process.DPP_ErrorMessage
- Path 1 READS: All logging properties
- Path 2 READS: process.DPP_ErrorMessage

✅ **Dependency graph built:** YES
- Path 1 → Path 2 (property dependency)

✅ **Topological sort applied:** YES
- Execution order: Path 1 → Path 2

---

## 12. Execution Order (Step 9)

### Business Logic Flow (Step 0 - MUST BE FIRST)

**Process Purpose:** Sync leave data from D365 to Oracle Fusion HCM

**Operations Analysis:**

**Operation 1: Create Leave Oracle Fusion OP (8f709c2b-e63f-4d5f-9374-2932ed70415d)**
- **Purpose:** Entry point - Receives leave request from D365 via web service
- **Produces:** Input document (leave request JSON)
- **Dependent Operations:** ALL subsequent operations depend on input data

**Operation 2: Leave Oracle Fusion Create (6e8920fd-af5a-430b-a1d9-9fde7ac29a12)**
- **Purpose:** Create leave absence entry in Oracle Fusion HCM via REST API
- **Produces:** 
  - HTTP status code (meta.base.applicationstatuscode)
  - Response document (Oracle Fusion leave response)
  - Response headers (dynamicdocument.DDP_RespHeader)
- **Dependent Operations:** 
  - Decision shape2 (checks HTTP status)
  - Decision shape44 (checks response header)
  - Map shape34 (maps success response)
  - Error handling shapes (shape39, shape45, shape46)

**Operation 3: Email w Attachment (af07502a-fafd-4976-a691-45d51a33b549)**
- **Purpose:** Send error notification email with payload attachment (subprocess)
- **Produces:** Email sent (no data returned to main process)
- **Dependent Operations:** NONE (terminal operation in error path)

**Operation 4: Email W/O Attachment (15a72a21-9b57-49a1-a8ed-d70367146644)**
- **Purpose:** Send error notification email without attachment (subprocess)
- **Produces:** Email sent (no data returned to main process)
- **Dependent Operations:** NONE (terminal operation in error path)

**Business Flow:**

```
1. Receive leave request from D365 (Entry operation)
2. Set logging properties (for error reporting)
3. Transform D365 format → Oracle Fusion format
4. Set Oracle Fusion API URL
5. Call Oracle Fusion API to create leave
6. Check HTTP status code:
   - If 20x (success) → Map success response → Return to D365
   - If NOT 20x (error) → Check if response is compressed → Extract error → Return error to D365
7. On any exception → Send error email → Return error to D365
```

**Operations that MUST execute FIRST:**
1. shape38 (Input_details) - Writes all logging properties needed for error reporting
2. shape8 (set URL) - Writes dynamicdocument.URL needed for HTTP operation
3. shape33 (HTTP operation) - Produces status code and response data needed for decisions

**Operations that execute AFTER:**
1. shape2 (Decision) - Executes AFTER shape33 (needs HTTP status code)
2. shape44 (Decision) - Executes AFTER shape33 (needs response header)
3. All error handling shapes - Execute AFTER shape33 (need error response)
4. shape21 (Subprocess) - Executes AFTER shape19 (needs error message property)

### Execution Order List

**Main Process Execution Order:**

```
1. shape1 (START) - Entry point
2. shape38 (Input_details) - Write logging properties
3. shape17 (Try/Catch) - Begin error handling wrapper
   TRY BLOCK:
   4. shape29 (Map) - Transform D365 → Oracle Fusion format
   5. shape8 (set URL) - Set Oracle Fusion API URL
   6. shape49 (Notify) - Log request (INFO level)
   7. shape33 (HTTP Operation) - Call Oracle Fusion API [DOWNSTREAM SYSTEM CALL]
   8. shape2 (Decision) - Check HTTP status 20*
      
      IF TRUE (Status 20x - Success):
      9a. shape34 (Map) - Map Oracle response to D365 response format
      10a. shape35 (Return Documents) - Return success response [HTTP: 200] [SUCCESS]
      
      IF FALSE (Status NOT 20x - Error):
      9b. shape44 (Decision) - Check if response is gzip compressed
         
         IF TRUE (Response is gzip):
         10b. shape45 (DataProcess) - Decompress gzip response
         11b. shape46 (DocumentProperties) - Extract error message
         12b. shape47 (Map) - Map error response
         13b. shape48 (Return Documents) - Return error response [HTTP: 400] [ERROR]
         
         IF FALSE (Response is NOT gzip):
         10c. shape39 (DocumentProperties) - Extract error message
         11c. shape40 (Map) - Map error response
         12c. shape36 (Return Documents) - Return error response [HTTP: 400] [ERROR]
   
   CATCH BLOCK (if any error in Try):
   14. shape20 (Branch) - Error handling branch [SEQUENTIAL]
       Path 1 (shape19 → shape21):
       15a. shape19 (DocumentProperties) - Extract catch error message
       16a. shape21 (ProcessCall) - Call email subprocess [SUBPROCESS]
       
       Path 2 (shape41 → shape43):
       15b. shape41 (Map) - Map error response
       16b. shape43 (Return Documents) - Return error response [HTTP: 500] [ERROR]
```

**Subprocess Execution Order (a85945c5-3004-42b9-80b1-104f465cd1fb):**

```
SUBPROCESS: (Sub) Office 365 Email
1. shape1 (START) - Subprocess entry
2. shape2 (Try/Catch) - Error handling wrapper
   TRY BLOCK:
   3. shape4 (Decision) - Check DPP_HasAttachment == "Y"
      
      IF TRUE (Has attachment):
      4a. shape11 (Message) - Build email body with error details
      5a. shape14 (DocumentProperties) - Store email body in DPP_MailBody
      6a. shape15 (Message) - Add payload as attachment
      7a. shape6 (DocumentProperties) - Set mail properties (from, to, subject, body, filename)
      8a. shape3 (ConnectorAction) - Send email with attachment [DOWNSTREAM SYSTEM CALL]
      9a. shape5 (Stop) - Success return (continue=true)
      
      IF FALSE (No attachment):
      4b. shape23 (Message) - Build email body with error details
      5b. shape22 (DocumentProperties) - Store email body in DPP_MailBody
      6b. shape20 (DocumentProperties) - Set mail properties (from, to, subject, body)
      7b. shape7 (ConnectorAction) - Send email without attachment [DOWNSTREAM SYSTEM CALL]
      8b. shape9 (Stop) - Success return (continue=true)
   
   CATCH BLOCK (if any error in Try):
   10. shape10 (Exception) - Throw exception with catch error message [TERMINAL]
```

### Dependency Verification

**Reference to Step 4 (Data Dependency Graph):**

✅ **Property dependency verification:** YES

**Verified Dependencies:**
1. shape8 writes dynamicdocument.URL → shape33 reads dynamicdocument.URL → shape8 MUST execute BEFORE shape33
2. shape38 writes logging properties → subprocess reads logging properties → shape38 MUST execute BEFORE shape21
3. shape19 writes process.DPP_ErrorMessage → shape41 (map) reads process.DPP_ErrorMessage → shape19 MUST execute BEFORE shape41 (Path 1 → Path 2)
4. shape33 produces meta.base.applicationstatuscode → shape2 reads it → shape33 MUST execute BEFORE shape2
5. shape33 produces dynamicdocument.DDP_RespHeader → shape44 reads it → shape33 MUST execute BEFORE shape44

**All property reads happen AFTER property writes:** ✅ VERIFIED

### Branch Execution Order

**Reference to Step 8 (Branch Analysis):**

✅ **Branch analysis used:** YES

**Branch shape20 (Error Handling Branch):**
- Classification: SEQUENTIAL
- Dependency Order: Path 1 → Path 2
- Reasoning: Path 1 contains API calls (email) AND Path 2 reads property written by Path 1

**Execution:**
1. Path 1 (shape19 → shape21): Extract error → Send email
2. Path 2 (shape41 → shape43): Map error → Return error response

### Decision Path Tracing

**Reference to Step 7 (Decision Analysis):**

✅ **Decision analysis used:** YES

**Decision shape2 (HTTP Status check):**
- TRUE path: shape34 → shape35 (Success return)
- FALSE path: shape44 → ... → shape36/shape48 (Error return)
- Convergence: NONE (paths terminate independently)

**Decision shape44 (Content Type check):**
- TRUE path: shape45 → shape46 → shape47 → shape48 (Gzip error return)
- FALSE path: shape39 → shape40 → shape36 (Non-gzip error return)
- Convergence: NONE (paths terminate independently)

**Subprocess Decision shape4 (Attachment check):**
- TRUE path: shape11 → ... → shape5 (Email with attachment)
- FALSE path: shape23 → ... → shape9 (Email without attachment)
- Convergence: NONE (paths terminate at different Stop shapes)

### Execution Order Self-Check

✅ **Business logic verified FIRST:** YES (documented in Step 0 above)

✅ **Operation analysis complete:** YES
- All operations identified with purposes and outputs

✅ **Business logic execution order identified:** YES
- Operations that must execute first documented

✅ **Data dependencies checked FIRST:** YES (before following dragpoints)

✅ **Operation response analysis used:** YES (reference to Step 1c)

✅ **Decision analysis used:** YES (reference to Step 7)

✅ **Dependency graph used:** YES (reference to Step 4)

✅ **Branch analysis used:** YES (reference to Step 8)

✅ **Property dependency verification:** YES
- All dependencies verified and documented

✅ **Topological sort applied:** YES
- Branch shape20: Path 1 → Path 2 (sequential order)

---

## 13. Sequence Diagram (Step 10)

**📋 NOTE:** This diagram shows the execution flow based on:
- **Dependency graph** (Step 4) - Property dependencies
- **Control flow graph** (Step 5) - Dragpoint connections
- **Decision analysis** (Step 7) - Decision paths and data sources
- **Branch analysis** (Step 8) - Branch classification and execution order
- **Execution order** (Step 9) - Business logic and actual execution sequence

Detailed request/response JSON examples are documented in Section 16 (HTTP Status Codes and Return Path Responses) and Section 17 (Request/Response JSON Examples).

### Main Process Flow

```
START (shape1)
 |
 ├─→ shape38: Set Input Details (DocumentProperties)
 |   └─→ WRITES: [process.DPP_Process_Name, process.DPP_AtomName, process.DPP_Payload,
 |                 process.DPP_ExecutionID, process.DPP_File_Name, process.DPP_Subject,
 |                 process.To_Email, process.DPP_HasAttachment]
 |   └─→ SOURCE: [Execution properties, Current document, Defined parameters]
 |
 ├─→ shape17: Try/Catch Error Handler
 |   |
 |   ├─→ TRY BLOCK:
 |   |   |
 |   |   ├─→ shape29: Map - Leave Create Map
 |   |   |   └─→ TRANSFORM: D365 format → Oracle Fusion format
 |   |   |   └─→ FIELD MAPPINGS: employeeNumber→personNumber, absenceStatusCode→absenceStatusCd,
 |   |   |                        approvalStatusCode→approvalStatusCd
 |   |   |
 |   |   ├─→ shape8: Set URL (DocumentProperties)
 |   |   |   └─→ WRITES: [dynamicdocument.URL]
 |   |   |   └─→ VALUE: "hcmRestApi/resources/11.13.18.05/absences"
 |   |   |
 |   |   ├─→ shape49: Notify (Log Request)
 |   |   |   └─→ LOG LEVEL: INFO
 |   |   |   └─→ MESSAGE: Current document
 |   |   |
 |   |   ├─→ shape33: Leave Oracle Fusion Create (HTTP POST) [DOWNSTREAM SYSTEM CALL]
 |   |   |   └─→ READS: [dynamicdocument.URL]
 |   |   |   └─→ WRITES: [meta.base.applicationstatuscode, dynamicdocument.DDP_RespHeader]
 |   |   |   └─→ HTTP: [Expected: 200/201, Error: 400/401/403/404/500]
 |   |   |   └─→ ENDPOINT: POST {base_url}/{dynamicdocument.URL}
 |   |   |   └─→ CONTENT-TYPE: application/json
 |   |   |
 |   |   ├─→ shape2: Decision - HTTP Status 20 check
 |   |   |   └─→ CONDITION: meta.base.applicationstatuscode wildcard matches "20*"?
 |   |   |   └─→ DATA SOURCE: TRACK_PROPERTY (from operation response)
 |   |   |   └─→ TYPE: POST_OPERATION
 |   |   |   |
 |   |   |   ├─→ IF TRUE (Status 20x - Success):
 |   |   |   |   |
 |   |   |   |   ├─→ shape34: Map - Oracle Fusion Leave Response Map
 |   |   |   |   |   └─→ TRANSFORM: Oracle response → D365 response format
 |   |   |   |   |   └─→ MAPPINGS: personAbsenceEntryId → personAbsenceEntryId
 |   |   |   |   |   └─→ DEFAULTS: status="success", message="Data successfully sent to Oracle Fusion", success="true"
 |   |   |   |   |
 |   |   |   |   └─→ shape35: Return Documents - Success Response [HTTP: 200] [SUCCESS]
 |   |   |   |       └─→ RESPONSE: {"status": "success", "message": "...", "personAbsenceEntryId": 123, "success": "true"}
 |   |   |   |
 |   |   |   └─→ IF FALSE (Status NOT 20x - Error):
 |   |   |       |
 |   |   |       ├─→ shape44: Decision - Check Response Content Type
 |   |   |       |   └─→ CONDITION: dynamicdocument.DDP_RespHeader equals "gzip"?
 |   |   |       |   └─→ DATA SOURCE: TRACK_PROPERTY (from operation response header)
 |   |   |       |   └─→ TYPE: POST_OPERATION
 |   |   |       |   |
 |   |   |       |   ├─→ IF TRUE (Response is gzip):
 |   |   |       |   |   |
 |   |   |       |   |   ├─→ shape45: DataProcess - Decompress Gzip
 |   |   |       |   |   |   └─→ SCRIPT: Groovy - GZIPInputStream decompression
 |   |   |       |   |   |
 |   |   |       |   |   ├─→ shape46: Extract Error Message (DocumentProperties)
 |   |   |       |   |   |   └─→ WRITES: [process.DPP_ErrorMessage]
 |   |   |       |   |   |   └─→ SOURCE: Current document (meta.base.applicationstatusmessage)
 |   |   |       |   |   |
 |   |   |       |   |   ├─→ shape47: Map - Leave Error Map
 |   |   |       |   |   |   └─→ READS: [process.DPP_ErrorMessage]
 |   |   |       |   |   |   └─→ DEFAULTS: status="failure", success="false"
 |   |   |       |   |   |
 |   |   |       |   |   └─→ shape48: Return Documents - Error Response [HTTP: 400] [ERROR]
 |   |   |       |   |       └─→ RESPONSE: {"status": "failure", "message": "...", "success": "false"}
 |   |   |       |   |
 |   |   |       |   └─→ IF FALSE (Response is NOT gzip):
 |   |   |       |       |
 |   |   |       |       ├─→ shape39: Extract Error Message (DocumentProperties)
 |   |   |       |       |   └─→ WRITES: [process.DPP_ErrorMessage]
 |   |   |       |       |   └─→ SOURCE: Track property (meta.base.applicationstatusmessage)
 |   |   |       |       |
 |   |   |       |       ├─→ shape40: Map - Leave Error Map
 |   |   |       |       |   └─→ READS: [process.DPP_ErrorMessage]
 |   |   |       |       |   └─→ DEFAULTS: status="failure", success="false"
 |   |   |       |       |
 |   |   |       |       └─→ shape36: Return Documents - Error Response [HTTP: 400] [ERROR]
 |   |   |       |           └─→ RESPONSE: {"status": "failure", "message": "...", "success": "false"}
 |   |   |
 |   └─→ CATCH BLOCK (if exception in Try):
 |       |
 |       ├─→ shape20: Branch - Error Handling [SEQUENTIAL - 2 paths]
 |           |
 |           ├─→ PATH 1 (Identifier: "1") - Send Error Email:
 |           |   |
 |           |   ├─→ shape19: Extract Catch Error Message (DocumentProperties)
 |           |   |   └─→ WRITES: [process.DPP_ErrorMessage]
 |           |   |   └─→ SOURCE: Track property (meta.base.catcherrorsmessage)
 |           |   |
 |           |   └─→ shape21: ProcessCall - Email Subprocess [SUBPROCESS CALL]
 |           |       └─→ SUBPROCESS: (Sub) Office 365 Email (a85945c5-3004-42b9-80b1-104f465cd1fb)
 |           |       └─→ READS: [process.To_Email, process.DPP_Subject, process.DPP_Process_Name,
 |           |                   process.DPP_AtomName, process.DPP_ExecutionID, process.DPP_ErrorMessage,
 |           |                   process.DPP_Payload, process.DPP_File_Name, process.DPP_HasAttachment]
 |           |       └─→ INTERNAL FLOW: See Subprocess Flow below
 |           |
 |           └─→ PATH 2 (Identifier: "2") - Return Error Response:
 |               |
 |               ├─→ shape41: Map - Leave Error Map
 |               |   └─→ READS: [process.DPP_ErrorMessage]
 |               |   └─→ DEFAULTS: status="failure", success="false"
 |               |
 |               └─→ shape43: Return Documents - Error Response [HTTP: 500] [ERROR]
 |                   └─→ RESPONSE: {"status": "failure", "message": "...", "success": "false"}
```

### Subprocess Flow: (Sub) Office 365 Email (a85945c5-3004-42b9-80b1-104f465cd1fb)

```
SUBPROCESS START (shape1)
 |
 ├─→ shape2: Try/Catch Error Handler
 |   |
 |   ├─→ TRY BLOCK:
 |   |   |
 |   |   ├─→ shape4: Decision - Attachment_Check
 |   |   |   └─→ CONDITION: process.DPP_HasAttachment equals "Y"?
 |   |   |   └─→ DATA SOURCE: PROCESS_PROPERTY (from main process)
 |   |   |   └─→ TYPE: PRE_FILTER
 |   |   |   |
 |   |   |   ├─→ IF TRUE (Has attachment = "Y"):
 |   |   |   |   |
 |   |   |   |   ├─→ shape11: Build Email Body with Attachment (Message)
 |   |   |   |   |   └─→ READS: [process.DPP_Process_Name, process.DPP_AtomName,
 |   |   |   |   |                process.DPP_ExecutionID, process.DPP_ErrorMessage]
 |   |   |   |   |   └─→ TEMPLATE: HTML email with execution details table
 |   |   |   |   |
 |   |   |   |   ├─→ shape14: Set Mail Body (DocumentProperties)
 |   |   |   |   |   └─→ WRITES: [process.DPP_MailBody]
 |   |   |   |   |   └─→ SOURCE: Current document (email HTML)
 |   |   |   |   |
 |   |   |   |   ├─→ shape15: Add Payload Attachment (Message)
 |   |   |   |   |   └─→ READS: [process.DPP_Payload]
 |   |   |   |   |   └─→ OUTPUT: Payload document as attachment
 |   |   |   |   |
 |   |   |   |   ├─→ shape6: Set Mail Properties (DocumentProperties)
 |   |   |   |   |   └─→ WRITES: [connector.mail.fromAddress, connector.mail.toAddress,
 |   |   |   |   |                connector.mail.subject, connector.mail.body, connector.mail.filename]
 |   |   |   |   |   └─→ READS: [process.To_Email, process.DPP_Subject, process.DPP_MailBody,
 |   |   |   |   |                process.DPP_File_Name]
 |   |   |   |   |
 |   |   |   |   ├─→ shape3: Send Email with Attachment [DOWNSTREAM SYSTEM CALL]
 |   |   |   |   |   └─→ OPERATION: af07502a-fafd-4976-a691-45d51a33b549 (Email w Attachment)
 |   |   |   |   |   └─→ CONNECTOR: Office 365 Email (SMTP)
 |   |   |   |   |   └─→ HTTP: [Expected: 250 (SMTP), Error: 5xx]
 |   |   |   |   |
 |   |   |   |   └─→ shape5: Stop (continue=true) [SUBPROCESS SUCCESS RETURN]
 |   |   |   |
 |   |   |   └─→ IF FALSE (Has attachment != "Y"):
 |   |   |       |
 |   |   |       ├─→ shape23: Build Email Body without Attachment (Message)
 |   |   |       |   └─→ READS: [process.DPP_Process_Name, process.DPP_AtomName,
 |   |   |       |                process.DPP_ExecutionID, process.DPP_ErrorMessage]
 |   |   |       |   └─→ TEMPLATE: HTML email with execution details table
 |   |   |       |
 |   |   |       ├─→ shape22: Set Mail Body (DocumentProperties)
 |   |   |       |   └─→ WRITES: [process.DPP_MailBody]
 |   |   |       |   └─→ SOURCE: Current document (email HTML)
 |   |   |       |
 |   |   |       ├─→ shape20: Set Mail Properties (DocumentProperties)
 |   |   |       |   └─→ WRITES: [connector.mail.fromAddress, connector.mail.toAddress,
 |   |   |       |                connector.mail.subject, connector.mail.body]
 |   |   |       |   └─→ READS: [process.To_Email, process.DPP_Subject, process.DPP_MailBody]
 |   |   |       |
 |   |   |       ├─→ shape7: Send Email without Attachment [DOWNSTREAM SYSTEM CALL]
 |   |   |       |   └─→ OPERATION: 15a72a21-9b57-49a1-a8ed-d70367146644 (Email W/O Attachment)
 |   |   |       |   └─→ CONNECTOR: Office 365 Email (SMTP)
 |   |   |       |   └─→ HTTP: [Expected: 250 (SMTP), Error: 5xx]
 |   |   |       |
 |   |   |       └─→ shape9: Stop (continue=true) [SUBPROCESS SUCCESS RETURN]
 |   |   |
 |   └─→ CATCH BLOCK (if exception in Try):
 |       |
 |       ├─→ shape20: Branch - Error Handling [SEQUENTIAL - 2 paths]
 |           |
 |           ├─→ PATH 1 (Executed FIRST):
 |           |   |
 |           |   ├─→ shape19: Extract Catch Error Message (DocumentProperties)
 |           |   |   └─→ WRITES: [process.DPP_ErrorMessage]
 |           |   |   └─→ SOURCE: Track property (meta.base.catcherrorsmessage)
 |           |   |
 |           |   └─→ shape21: Call Email Subprocess [SUBPROCESS CALL]
 |           |       └─→ SUBPROCESS: (Sub) Office 365 Email
 |           |       └─→ READS: [All logging properties]
 |           |       └─→ ACTION: Send error notification email
 |           |       └─→ RETURN: No explicit return to main process
 |           |
 |           └─→ PATH 2 (Executed SECOND):
 |               |
 |               ├─→ shape41: Map - Leave Error Map
 |               |   └─→ READS: [process.DPP_ErrorMessage]
 |               |   └─→ DEFAULTS: status="failure", success="false"
 |               |
 |               └─→ shape43: Return Documents - Error Response [HTTP: 500] [ERROR]
 |                   └─→ RESPONSE: {"status": "failure", "message": "...", "success": "false"}
```

**CRITICAL EXECUTION RULES:**

1. **Property Dependencies:**
   - shape8 MUST execute BEFORE shape33 (provides URL)
   - shape38 MUST execute BEFORE shape21 (provides logging properties)
   - shape19 MUST execute BEFORE shape41 (provides error message)

2. **Decision Execution:**
   - shape2 executes AFTER shape33 (checks HTTP status from operation)
   - shape44 executes AFTER shape33 (checks response header from operation)

3. **Branch Execution:**
   - shape20 paths execute SEQUENTIALLY: Path 1 → Path 2
   - Path 1 contains API calls (email) → SEQUENTIAL required
   - Path 2 depends on Path 1 (reads error message written by Path 1)

4. **Error Handling:**
   - Try/Catch wraps main business logic
   - Catch block executes only if exception occurs in Try block
   - Error paths return error response to caller

---

## 14. Subprocess Analysis (Step 7a)

### Subprocess: (Sub) Office 365 Email (a85945c5-3004-42b9-80b1-104f465cd1fb)

**Subprocess ID:** a85945c5-3004-42b9-80b1-104f465cd1fb  
**Subprocess Name:** (Sub) Office 365 Email  
**Purpose:** Send error notification email via Office 365 SMTP  
**Called By:** Main process shape21 (ProcessCall)

### Internal Flow

**Entry Point:** shape1 (START - passthroughaction)

**Flow Structure:**
1. Try/Catch error handler (shape2)
2. Decision: Check if attachment needed (shape4)
3. TRUE path: Build email with attachment → Send → Stop (success)
4. FALSE path: Build email without attachment → Send → Stop (success)
5. CATCH path: Throw exception

### Return Paths

| Return Label | Shape ID | Shape Type | Condition | Return Type |
|---|---|---|---|---|
| (implicit success) | shape5 | stop (continue=true) | Attachment path success | SUCCESS |
| (implicit success) | shape9 | stop (continue=true) | No attachment path success | SUCCESS |
| (exception) | shape10 | exception | Error in Try block | ERROR |

**Main Process Mapping:**

The subprocess does NOT have explicit return paths defined in the ProcessCall configuration (returnpaths is empty). The subprocess executes and returns implicitly:
- Success: Stop shapes with continue=true (shape5, shape9)
- Error: Exception shape (shape10) - throws exception to main process

### Properties Written by Subprocess

| Property Name | Written By | Source | Notes |
|---|---|---|---|
| process.DPP_MailBody | shape14, shape22 | Current document (email HTML) | Email body content |
| connector.mail.fromAddress | shape6, shape20 | Defined parameter (PP_Office365_Email.From_Email) | Email from address |
| connector.mail.toAddress | shape6, shape20 | Process property (process.To_Email) | Email recipient |
| connector.mail.subject | shape6, shape20 | Defined parameter + process property | Email subject |
| connector.mail.body | shape6, shape20 | Process property (process.DPP_MailBody) | Email body |
| connector.mail.filename | shape6 | Process property (process.DPP_File_Name) | Attachment filename |

### Properties Read by Subprocess (from Main Process)

| Property Name | Read By | Usage |
|---|---|---|
| process.DPP_HasAttachment | shape4 | Decision condition (check if attachment needed) |
| process.To_Email | shape6, shape20 | Email recipient address |
| process.DPP_Subject | shape6, shape20 | Email subject line |
| process.DPP_Process_Name | shape11, shape23 | Email body content (process name) |
| process.DPP_AtomName | shape11, shape23 | Email body content (atom name) |
| process.DPP_ExecutionID | shape11, shape23 | Email body content (execution ID) |
| process.DPP_ErrorMessage | shape11, shape23 | Email body content (error details) |
| process.DPP_Payload | shape15 | Email attachment content |
| process.DPP_File_Name | shape6 | Email attachment filename |
| process.DPP_MailBody | shape6, shape20 | Email body (after building) |

### Subprocess Execution Pattern

**Invocation:** Synchronous (wait=true, abort=true)
- Main process waits for subprocess to complete
- If subprocess throws exception, main process aborts

**Return Behavior:**
- Success: Subprocess completes with Stop (continue=true), main process continues (but no explicit next shape)
- Error: Subprocess throws exception, caught by main process Try/Catch

---

## 15. Critical Patterns Identified

### Pattern 1: Try/Catch Error Handling with Branching

**Location:** Main process shape17 (Try/Catch)

**Pattern:**
```
Try/Catch:
  Try Block:
    - Main business logic (map, set URL, call API, check status, return)
  Catch Block:
    - Branch with 2 paths (SEQUENTIAL):
      - Path 1: Send error email notification
      - Path 2: Return error response to caller
```

**Implementation:**
- Try block contains main business flow
- Catch block handles exceptions
- Branch in catch block ensures both error notification AND error response

### Pattern 2: HTTP Status Check with Error Response Handling

**Location:** Main process shape2 (Decision - HTTP Status 20 check)

**Pattern:**
```
HTTP Operation → Check Status Code:
  If 20x (Success):
    - Map success response
    - Return success to caller
  If NOT 20x (Error):
    - Check if response is compressed (gzip)
    - Decompress if needed
    - Extract error message
    - Map error response
    - Return error to caller
```

**Implementation:**
- POST_OPERATION decision (checks operation response)
- Handles both compressed and uncompressed error responses
- Extracts error message from Oracle Fusion API response

### Pattern 3: Conditional Email with/without Attachment

**Location:** Subprocess shape4 (Decision - Attachment_Check)

**Pattern:**
```
Decision: Has Attachment?
  If YES:
    - Build email body
    - Add payload as attachment
    - Set mail properties (including filename)
    - Send email with attachment
  If NO:
    - Build email body
    - Set mail properties (no filename)
    - Send email without attachment
```

**Implementation:**
- PRE_FILTER decision (checks input parameter)
- Two separate email operations (with/without attachment)
- Both paths terminate successfully (Stop with continue=true)

### Pattern 4: Property-Based Configuration

**Location:** shape8 (set URL), shape38 (Input_details)

**Pattern:**
```
Set Dynamic Properties:
  - Read from Defined Process Properties (configuration)
  - Write to Document/Process Properties
  - Use in subsequent operations
```

**Implementation:**
- Defined Process Properties act as configuration
- Document/Process Properties act as runtime variables
- Decouples configuration from operation logic

### Pattern 5: Error Notification with Logging

**Location:** shape38 (Input_details) + Subprocess shape21

**Pattern:**
```
Capture Execution Context:
  - Process Name
  - Atom Name
  - Execution ID
  - Input Payload
  - Error Message

Send Error Email:
  - Build HTML email with execution details
  - Attach payload (if enabled)
  - Send to configured recipients
```

**Implementation:**
- Logging properties captured early (shape38)
- Available for error reporting throughout process
- Subprocess handles email formatting and sending

---

## 16. Validation Checklist

### Data Dependencies
- [x] All property WRITES identified (11 properties written)
- [x] All property READS identified (12 properties read)
- [x] Dependency graph built (3 dependency chains documented)
- [x] Execution order satisfies all dependencies (verified - no read-before-write)

### Decision Analysis
- [x] ALL decision shapes inventoried (3 decisions: shape2, shape44, subprocess shape4)
- [x] BOTH TRUE and FALSE paths traced to termination (all paths traced)
- [x] Pattern type identified for each decision (Error Check, Conditional Logic)
- [x] Early exits identified and documented (all return paths documented)
- [x] Convergence points identified (NONE - all paths terminate independently)

### Branch Analysis
- [x] Each branch classified as parallel or sequential (shape20: SEQUENTIAL)
- [x] **CRITICAL:** API calls checked (Path 1 contains email API calls → SEQUENTIAL)
- [x] **SELF-CHECK:** Did I check for API calls? **YES**
- [x] **SELF-CHECK:** Did I classify or assume? **Classified** (analyzed dependencies and API calls)
- [x] Dependency order built using topological sort (Path 1 → Path 2)
- [x] Each path traced to terminal point (Path 1: subprocess, Path 2: shape43)
- [x] Convergence points identified (NONE)
- [x] Execution continuation point determined (NONE - paths terminate)

### Sequence Diagram
- [x] Format follows required structure (Operation → Decision → Operation)
- [x] Each operation shows READS and WRITES
- [x] Decisions show both TRUE and FALSE paths
- [x] **CROSS-VALIDATION:** Sequence diagram matches control flow graph (Step 5)
- [x] **CROSS-VALIDATION:** Execution order matches dependency graph (Step 4)
- [x] Early exits marked [ERROR] or [SUCCESS]
- [x] Subprocess internal flows documented
- [x] Subprocess return paths mapped to main process

### Subprocess Analysis
- [x] ALL subprocesses analyzed (1 subprocess: a85945c5-3004-42b9-80b1-104f465cd1fb)
- [x] Internal flow traced (Start → Try/Catch → Decision → Email operations → Stop)
- [x] Return paths identified (implicit success via Stop, error via Exception)
- [x] Return path labels mapped to main process (no explicit mapping - implicit returns)
- [x] Properties written by subprocess documented (7 properties)
- [x] Properties read by subprocess from main process documented (12 properties)

### Edge Cases
- [x] Nested branches/decisions analyzed (Decision shape44 nested in FALSE path of shape2)
- [x] Loops identified (NONE)
- [x] Property chains traced (3 chains documented)
- [x] Circular dependencies detected and resolved (NONE)
- [x] Try/Catch error paths documented (2 Try/Catch blocks: main process, subprocess)

### Property Extraction Completeness
- [x] All property patterns searched (${}, %%, {})
- [x] Message parameters checked for process properties (shape11, shape15, shape23 checked)
- [x] Operation headers/path parameters checked (dynamicdocument.URL checked)
- [x] Decision track properties identified (meta.base.applicationstatuscode, dynamicdocument.DDP_RespHeader)
- [x] Document properties that read other properties identified (all documented)

### Input/Output Structure Analysis (CONTRACT VERIFICATION)
- [x] Entry point operation identified (8f709c2b-e63f-4d5f-9374-2932ed70415d)
- [x] Request profile identified and loaded (febfa3e1-f719-4ee8-ba57-cdae34137ab3)
- [x] Request profile structure analyzed (JSON - single object)
- [x] Array vs single object detected (Single object)
- [x] Array cardinality documented (N/A - not an array)
- [x] ALL request fields extracted (9 fields)
- [x] Request field paths documented (all paths: Root/Object/fieldName)
- [x] Request field mapping table generated (Boomi → Azure DTO)
- [x] Response profile identified and loaded (f4ca3a70-114a-4601-bad8-44a3eb20e2c0)
- [x] Response profile structure analyzed (JSON - single object with wrapper)
- [x] ALL response fields extracted (4 fields)
- [x] Response field mapping table generated (Boomi → Azure DTO)
- [x] Document processing behavior determined (single_document)
- [x] Input/Output structure documented in Phase 1 document (Sections 2 & 3)

### HTTP Status Codes and Return Path Responses
- [x] Section 6 (HTTP Status Codes and Return Path Responses - Step 1e) present
- [x] All return paths documented with HTTP status codes (4 return paths)
- [x] Response JSON examples provided for each return path
- [x] Populated fields documented for each return path (source and populated by)
- [x] Decision conditions leading to each return documented
- [x] Error codes and success codes documented (status field values)
- [x] Downstream operation HTTP status codes documented (operation 6e8920fd)
- [x] Error handling strategy documented (return error response with message)

### Map Analysis
- [x] ALL map files identified and loaded (3 maps)
- [x] Request maps identified (map c426b4d6 - D365 → Oracle Fusion)
- [x] Field mappings extracted from each map (all mappings documented)
- [x] Profile vs map field name discrepancies documented (absenceStatusCode→absenceStatusCd)
- [x] Map field names marked as AUTHORITATIVE
- [x] Scripting functions analyzed (NONE in request map)
- [x] Static values identified and documented (map defaults)
- [x] Process property mappings documented (DPP_ErrorMessage in error map)
- [x] Map Analysis documented in Phase 1 document (Section 5)

---

## 17. Request/Response JSON Examples

### Process Layer Entry Point

**Request JSON Example:**

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
    "personAbsenceEntryId": 300100559876543,
    "success": "true"
  }
}
```

**Error Response - API Error (HTTP 400):**

```json
{
  "leaveResponse": {
    "status": "failure",
    "message": "Oracle Fusion API error: Invalid absence type",
    "success": "false"
  }
}
```

**Error Response - Process Exception (HTTP 500):**

```json
{
  "leaveResponse": {
    "status": "failure",
    "message": "Process execution error: Connection timeout",
    "success": "false"
  }
}
```

### Downstream System Layer Calls

**Operation: Leave Oracle Fusion Create (6e8920fd-af5a-430b-a1d9-9fde7ac29a12)**

**Request JSON:**

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

**Request Details:**
- **Method:** POST
- **URL:** {base_url}/hcmRestApi/resources/11.13.18.05/absences
- **Content-Type:** application/json
- **Authentication:** Basic Auth (INTEGRATION.USER@al-ghurair.com)

**Response JSON (Success - HTTP 200/201):**

```json
{
  "personAbsenceEntryId": 300100559876543,
  "absenceCaseId": "ABS-2024-001",
  "absenceTypeId": 123,
  "personId": 456789,
  "startDate": "2024-03-24",
  "endDate": "2024-03-25",
  "absenceStatusCd": "SUBMITTED",
  "approvalStatusCd": "APPROVED",
  "duration": 2,
  "startDateDuration": 1,
  "endDateDuration": 1,
  "personNumber": "9000604",
  "absenceType": "Sick Leave",
  "employer": "Al Ghurair Investment LLC",
  "createdBy": "INTEGRATION.USER",
  "creationDate": "2024-03-24T10:30:00Z",
  "lastUpdateDate": "2024-03-24T10:30:00Z",
  "lastUpdatedBy": "INTEGRATION.USER",
  "links": [
    {
      "rel": "self",
      "href": "https://iaaxey-dev3.fa.ocs.oraclecloud.com/hcmRestApi/resources/11.13.18.05/absences/300100559876543",
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
  "detail": "Invalid absence type provided",
  "o:errorCode": "VALIDATION_ERROR",
  "o:errorDetails": [
    {
      "type": "http://www.w3.org/Protocols/rfc2616/rfc2616-sec10.html#sec10.4.1",
      "title": "Bad Request",
      "status": 400,
      "detail": "The absence type 'Sick Leave' is not valid for this employee",
      "o:errorPath": "absenceType"
    }
  ]
}
```

**Response JSON (Error - HTTP 401):**

```json
{
  "title": "Unauthorized",
  "status": 401,
  "detail": "Authentication failed"
}
```

**Response JSON (Error - HTTP 500):**

```json
{
  "title": "Internal Server Error",
  "status": 500,
  "detail": "An internal error occurred while processing the request"
}
```

**Operation: Email w Attachment (af07502a-fafd-4976-a691-45d51a33b549)**

**Request:** Email message with HTML body and attachment

**Email Details:**
- **From:** Boomi.Dev.failures@al-ghurair.com
- **To:** BoomiIntegrationTeam@al-ghurair.com
- **Subject:** DEV Failure : {AtomName} ({ProcessName}) has errors to report
- **Body:** HTML table with execution details
- **Attachment:** Input payload (filename: {ProcessName}_{timestamp}.txt)

**Response:** SMTP response (250 OK or error)

**Operation: Email W/O Attachment (15a72a21-9b57-49a1-a8ed-d70367146644)**

**Request:** Email message with HTML body (no attachment)

**Email Details:**
- **From:** Boomi.Dev.failures@al-ghurair.com
- **To:** BoomiIntegrationTeam@al-ghurair.com
- **Subject:** DEV Failure : {AtomName} ({ProcessName}) has errors to report
- **Body:** HTML table with execution details

**Response:** SMTP response (250 OK or error)

---

## 18. System Layer Identification

### Downstream Systems

**System 1: Oracle Fusion HCM**
- **Connection:** aa1fcb29-d146-4425-9ea6-b9698090f60e (Oracle Fusion)
- **Base URL:** https://iaaxey-dev3.fa.ocs.oraclecloud.com:443
- **Authentication:** Basic Auth (INTEGRATION.USER@al-ghurair.com)
- **API Type:** REST API
- **Operations:**
  - Create Leave: POST /hcmRestApi/resources/11.13.18.05/absences
- **System Layer Function:** CreateLeaveInOracleFusion

**System 2: Office 365 Email (SMTP)**
- **Connection:** 00eae79b-2303-4215-8067-dcc299e42697 (Office 365 Email)
- **SMTP Host:** smtp-mail.outlook.com
- **SMTP Port:** 587
- **Authentication:** SMTP AUTH (Boomi.Dev.failures@al-ghurair.com)
- **TLS:** Enabled
- **Operations:**
  - Send Email with Attachment
  - Send Email without Attachment
- **System Layer Function:** SendEmailNotification

### System Layer API Requirements

**System API 1: Oracle Fusion HCM Leave API**
- **Function Name:** CreateLeaveInOracleFusion
- **Method:** POST
- **Endpoint:** /hcmRestApi/resources/11.13.18.05/absences
- **Request DTO:** OracleFusionLeaveRequestDto
- **Response DTO:** OracleFusionLeaveResponseDto
- **Error Handling:** Return HTTP status codes (400, 401, 404, 500)

**System API 2: Email Notification API**
- **Function Name:** SendEmailNotification
- **Method:** SMTP Send
- **Request DTO:** EmailNotificationRequestDto
- **Response:** SMTP status
- **Error Handling:** Throw exception on SMTP error

---

## 19. Function Exposure Decision Table

### Process Layer Functions

| Function Name | HTTP Method | Route | Purpose | Expose? | Reasoning |
|---|---|---|---|---|---|
| CreateLeaveFromD365 | POST | /api/hcm/leave/create | Create leave in Oracle Fusion from D365 request | ✅ YES | Primary business function - orchestrates D365 → Oracle Fusion sync |

### System Layer Functions

| Function Name | HTTP Method | Route | Purpose | Expose? | Reasoning |
|---|---|---|---|---|---|
| CreateLeaveInOracleFusion | POST | /api/oracle-fusion/leave/create | Create leave in Oracle Fusion HCM | ✅ YES | Reusable System API - can be used by other processes |
| SendEmailNotification | POST | /api/notifications/email/send | Send email notification | ✅ YES | Reusable System API - generic email sending |

### Internal Helper Functions (NOT Exposed)

| Function Name | Purpose | Expose? | Reasoning |
|---|---|---|---|
| DecompressGzipResponse | Decompress gzip HTTP response | ❌ NO | Internal utility - part of error handling logic |
| BuildErrorResponse | Build error response DTO | ❌ NO | Internal utility - response mapping logic |
| ExtractErrorMessage | Extract error message from track properties | ❌ NO | Internal utility - error handling logic |

### Function Explosion Prevention

**CRITICAL RULES:**
1. **ONE Process Layer function per Boomi process** - CreateLeaveFromD365
2. **System Layer functions are reusable** - CreateLeaveInOracleFusion, SendEmailNotification
3. **Internal utilities are NOT exposed** - Decompression, error mapping, etc.
4. **Error handling is INTERNAL** - Not exposed as separate functions

**Validation:**
- [x] Process Layer: 1 function (CreateLeaveFromD365)
- [x] System Layer: 2 functions (CreateLeaveInOracleFusion, SendEmailNotification)
- [x] Internal utilities: 3 (not exposed)
- [x] Total exposed functions: 3 (prevents function explosion)

---

## 20. Pre-Phase 2 Validation Gate

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
- [x] Section 17 (Request/Response JSON Examples) - COMPLETE

**Self-Check Questions:**

1. ❓ Did I analyze ALL map files? **YES** (3 maps analyzed)
2. ❓ Did I identify request maps? **YES** (map c426b4d6 - D365 → Oracle)
3. ❓ Did I extract actual field names from maps? **YES** (all field mappings documented)
4. ❓ Did I compare profile field names vs map field names? **YES** (discrepancies documented)
5. ❓ Did I mark map field names as AUTHORITATIVE? **YES** (documented in Section 5)
6. ❓ Did I analyze scripting functions in maps? **YES** (NONE found in request map, PropertyGet in error map)
7. ❓ Did I extract HTTP status codes for all return paths? **YES** (4 return paths documented)
8. ❓ Did I document response JSON for each return path? **YES** (all documented in Section 6)
9. ❓ Did I document populated fields for each return path? **YES** (all documented)
10. ❓ Did I extract HTTP status codes for downstream operations? **YES** (operation 6e8920fd documented)
11. ❓ Did I create request/response JSON examples? **YES** (Section 17)

**ALL ANSWERS: YES** ✅

### VALIDATION: Phase 1 Document Complete

✅ **All mandatory sections present**  
✅ **All self-check questions answered with YES**  
✅ **Function Exposure Decision Table complete**  
✅ **Sequence diagram references all prior steps**

**PHASE 1 EXTRACTION COMPLETE - READY FOR PHASE 2**

---

## 21. Summary and Key Findings

### Process Overview

The HCM Leave Create process is a **synchronous integration** that receives leave requests from D365 and creates leave absence entries in Oracle Fusion HCM. The process includes comprehensive error handling with email notifications and proper error response mapping.

### Key Characteristics

1. **Input:** Single JSON object (D365 leave request)
2. **Output:** JSON response with status, message, and personAbsenceEntryId
3. **Downstream System:** Oracle Fusion HCM REST API
4. **Error Handling:** Try/Catch with email notification and error response
5. **Response Handling:** Checks HTTP status and handles compressed/uncompressed error responses

### Critical Dependencies

1. **URL Configuration:** shape8 must execute before shape33 (HTTP operation)
2. **Logging Properties:** shape38 must execute before error paths (provides context for email)
3. **Error Message:** Error extraction shapes must execute before error map
4. **HTTP Status:** shape33 must execute before decisions (shape2, shape44)

### Execution Patterns

1. **Happy Path:** Map → Set URL → Call API → Check Status (20x) → Map Success → Return Success
2. **Error Path (API Error):** Map → Set URL → Call API → Check Status (NOT 20x) → Check Compression → Extract Error → Map Error → Return Error
3. **Error Path (Exception):** Try/Catch → Catch → Branch (Send Email + Return Error)

### System Layer Functions Required

1. **CreateLeaveInOracleFusion** - POST to Oracle Fusion HCM absences API
2. **SendEmailNotification** - Send email via Office 365 SMTP

### Process Layer Function

1. **CreateLeaveFromD365** - Orchestrates D365 → Oracle Fusion leave sync with error handling

---

**Document Status:** ✅ COMPLETE  
**Phase 1 Validation:** ✅ PASSED  
**Ready for Phase 2:** ✅ YES
