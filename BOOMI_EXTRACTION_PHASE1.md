# BOOMI EXTRACTION PHASE 1 - HCM Leave Create Process

**Process Name:** HCM_Leave Create  
**Process ID:** ca69f858-785f-4565-ba1f-b4edc6cca05b  
**Description:** This Process will sync the Leave data between D365 and Oracle HCM  
**Version:** 29  
**Last Modified:** 2024-11-04T08:54:39Z

---

## 1. Operations Inventory

### Main Process Operations

| Operation ID | Operation Name | Type | Sub-Type | Purpose |
|---|---|---|---|---|
| 8f709c2b-e63f-4d5f-9374-2932ed70415d | Create Leave Oracle Fusion OP | connector-action | wss | Web Service Server Listen - Entry point for leave creation requests |
| 6e8920fd-af5a-430b-a1d9-9fde7ac29a12 | Leave Oracle Fusion Create | connector-action | http | HTTP POST to Oracle Fusion HCM to create leave absence entry |

### Subprocess Operations (a85945c5-3004-42b9-80b1-104f465cd1fb - Office 365 Email)

| Operation ID | Operation Name | Type | Sub-Type | Purpose |
|---|---|---|---|---|
| af07502a-fafd-4976-a691-45d51a33b549 | Email w Attachment | connector-action | mail | Send email with attachment |
| 15a72a21-9b57-49a1-a8ed-d70367146644 | Email W/O Attachment | connector-action | mail | Send email without attachment |

---

## 2. Input Structure Analysis (Step 1a)

### Entry Point Operation
- **Operation ID:** 8f709c2b-e63f-4d5f-9374-2932ed70415d
- **Operation Name:** Create Leave Oracle Fusion OP
- **Type:** Web Services Server Listen (wss)
- **Input Type:** singlejson
- **Request Profile ID:** febfa3e1-f719-4ee8-ba57-cdae34137ab3
- **Request Profile Name:** D365 Leave Create JSON Profile

### Request Profile Structure

**Profile Type:** profile.json  
**Root Structure:** Root/Object  
**Array Detection:** ❌ NO - Single object structure  
**Input Type:** singlejson  

### Request Profile Fields

| Field Name | Data Type | Required | Boomi Path | Azure DTO Property | Notes |
|---|---|---|---|---|---|
| employeeNumber | number | Yes | Root/Object/employeeNumber | EmployeeNumber | Employee identifier from D365 |
| absenceType | character | Yes | Root/Object/absenceType | AbsenceType | Type of leave (e.g., "Sick Leave") |
| employer | character | Yes | Root/Object/employer | Employer | Employer name |
| startDate | character | Yes | Root/Object/startDate | StartDate | Leave start date (format: YYYY-MM-DD) |
| endDate | character | Yes | Root/Object/endDate | EndDate | Leave end date (format: YYYY-MM-DD) |
| absenceStatusCode | character | Yes | Root/Object/absenceStatusCode | AbsenceStatusCode | Status code (e.g., "SUBMITTED") |
| approvalStatusCode | character | Yes | Root/Object/approvalStatusCode | ApprovalStatusCode | Approval status (e.g., "APPROVED") |
| startDateDuration | number | Yes | Root/Object/startDateDuration | StartDateDuration | Duration for start date (e.g., 1 = full day) |
| endDateDuration | number | Yes | Root/Object/endDateDuration | EndDateDuration | Duration for end date (e.g., 1 = full day) |

### Document Processing Behavior

**Behavior:** Single document processing  
**Description:** Boomi processes each request as a single document execution  
**Execution Pattern:** single_execution  
**Session Management:** one_session_per_execution  

### Input JSON Example

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

## 3. Response Structure Analysis (Step 1b)

### Response Profile
- **Response Profile ID:** febfa3e1-f719-4ee8-ba57-cdae34137ab3 (same as request - bidirectional)
- **Response Profile Name:** D365 Leave Create JSON Profile (used for response via map to f4ca3a70-114a-4601-bad8-44a3eb20e2c0)
- **Actual Response Profile ID:** f4ca3a70-114a-4601-bad8-44a3eb20e2c0
- **Actual Response Profile Name:** Leave D365 Response

### Response Profile Structure

**Profile Type:** profile.json  
**Root Structure:** leaveResponse/Object  
**Array Detection:** ❌ NO - Single object structure  

### Response Profile Fields

| Field Name | Data Type | Boomi Path | Azure DTO Property | Notes |
|---|---|---|---|---|
| status | character | leaveResponse/Object/status | Status | Response status ("success" or "failure") |
| message | character | leaveResponse/Object/message | Message | Response message |
| personAbsenceEntryId | number | leaveResponse/Object/personAbsenceEntryId | PersonAbsenceEntryId | Oracle Fusion absence entry ID (returned on success) |
| success | character | leaveResponse/Object/success | Success | Boolean string ("true" or "false") |

### Response JSON Examples

**Success Response:**
```json
{
  "status": "success",
  "message": "Data successfully sent to Oracle Fusion",
  "personAbsenceEntryId": 300000123456789,
  "success": "true"
}
```

**Error Response:**
```json
{
  "status": "failure",
  "message": "[Error message from Oracle Fusion or process error]",
  "success": "false"
}
```

---

## 4. Operation Response Analysis (Step 1c)

### Operation: Leave Oracle Fusion Create (6e8920fd-af5a-430b-a1d9-9fde7ac29a12)

**Response Profile ID:** None explicitly defined (HTTP operation with returnErrors=true, returnResponses=true)  
**Response Type:** HTTP response with JSON body  
**Response Content Type:** application/json  

**Response Header Mapping:**
- Header: Content-Encoding → Dynamic Document Property: DDP_RespHeader

**Response Structure:**
Based on profile 316175c7-0e45-4869-9ac6-5f9d69882a62 (Oracle Fusion Leave Response JSON Profile), the operation returns extensive leave data including:

**Key Response Fields Extracted:**
- personAbsenceEntryId (mapped to final response)
- absenceStatusCd
- approvalStatusCd
- startDate, endDate
- startDateDuration, endDateDuration
- personNumber
- absenceType
- employer
- Plus 60+ additional Oracle Fusion fields

**Extracted Fields:**
- **personAbsenceEntryId** - Extracted by map shape34 (e4fd3f59-edb5-43a1-aeae-143b600a064e), written to final response profile

**Consumers:**
- shape34 (map) reads the Oracle Fusion response and maps personAbsenceEntryId to the final response

**Business Logic Implications:**
- Oracle Fusion Create operation MUST execute before shape34 (response mapping)
- Decision shape2 checks HTTP status code from this operation to determine success/failure path

---

## 5. Map Analysis (Step 1d)

### Map Inventory

| Map ID | Map Name | From Profile | To Profile | Purpose |
|---|---|---|---|---|
| c426b4d6-2aff-450e-b43b-59956c4dbc96 | Leave Create Map | febfa3e1-f719-4ee8-ba57-cdae34137ab3 | a94fa205-c740-40a5-9fda-3d018611135a | Transform D365 request to Oracle Fusion request |
| e4fd3f59-edb5-43a1-aeae-143b600a064e | Oracle Fusion Leave Response Map | 316175c7-0e45-4869-9ac6-5f9d69882a62 | f4ca3a70-114a-4601-bad8-44a3eb20e2c0 | Transform Oracle Fusion response to D365 response (success) |
| f46b845a-7d75-41b5-b0ad-c41a6a8e9b12 | Leave Error Map | 23d7a2e9-5cb0-4e9c-9e4b-3154834bad0d | f4ca3a70-114a-4601-bad8-44a3eb20e2c0 | Transform error to D365 response (failure) |

### Map 1: Leave Create Map (c426b4d6-2aff-450e-b43b-59956c4dbc96)

**Purpose:** Transform D365 leave request to Oracle Fusion HCM format  
**From Profile:** febfa3e1-f719-4ee8-ba57-cdae34137ab3 (D365 Leave Create JSON Profile)  
**To Profile:** a94fa205-c740-40a5-9fda-3d018611135a (HCM Leave Create JSON Profile)  

**Field Mappings:**

| Source Field | Target Field | Transformation | Notes |
|---|---|---|---|
| employeeNumber | personNumber | Direct mapping | Employee ID |
| absenceType | absenceType | Direct mapping | Leave type |
| employer | employer | Direct mapping | Employer name |
| startDate | startDate | Direct mapping | Start date |
| endDate | endDate | Direct mapping | End date |
| absenceStatusCode | absenceStatusCd | Direct mapping | Status code (field name change) |
| approvalStatusCode | approvalStatusCd | Direct mapping | Approval status (field name change) |
| startDateDuration | startDateDuration | Direct mapping | Start duration |
| endDateDuration | endDateDuration | Direct mapping | End duration |

**Field Name Discrepancies:**
- absenceStatusCode → absenceStatusCd (shortened)
- approvalStatusCode → approvalStatusCd (shortened)
- employeeNumber → personNumber (different terminology)

### Map 2: Oracle Fusion Leave Response Map (e4fd3f59-edb5-43a1-aeae-143b600a064e)

**Purpose:** Transform Oracle Fusion success response to D365 response format  
**From Profile:** 316175c7-0e45-4869-9ac6-5f9d69882a62 (Oracle Fusion Leave Response JSON Profile)  
**To Profile:** f4ca3a70-114a-4601-bad8-44a3eb20e2c0 (Leave D365 Response)  

**Field Mappings:**

| Source Field | Target Field | Source Type | Notes |
|---|---|---|---|
| personAbsenceEntryId | personAbsenceEntryId | profile | Oracle Fusion generated ID |
| (static) | status | default | "success" |
| (static) | message | default | "Data successfully sent to Oracle Fusion" |
| (static) | success | default | "true" |

**Default Values:**
- status: "success"
- message: "Data successfully sent to Oracle Fusion"
- success: "true"

### Map 3: Leave Error Map (f46b845a-7d75-41b5-b0ad-c41a6a8e9b12)

**Purpose:** Transform error information to D365 response format  
**From Profile:** 23d7a2e9-5cb0-4e9c-9e4b-3154834bad0d (Dummy FF Profile)  
**To Profile:** f4ca3a70-114a-4601-bad8-44a3eb20e2c0 (Leave D365 Response)  

**Field Mappings:**

| Source Field | Target Field | Source Type | Notes |
|---|---|---|---|
| process.DPP_ErrorMessage | message | function (PropertyGet) | Error message from process property |
| (static) | status | default | "failure" |
| (static) | success | default | "false" |

**Function Analysis:**
- **Function 1:** PropertyGet - Retrieves process.DPP_ErrorMessage property
  - Input: Property Name = "DPP_ErrorMessage"
  - Output: Result (mapped to message field)

**Default Values:**
- status: "failure"
- success: "false"

**CRITICAL RULE:** Map field names are AUTHORITATIVE for the transformation logic.

---

## 6. HTTP Status Codes and Return Path Responses (Step 1e)

### Return Path Analysis

#### Return Path 1: Success Response (shape35)

**Return Label:** "Success Response"  
**Return Shape ID:** shape35  
**HTTP Status Code:** 200  
**Decision Conditions Leading to Return:**
- Decision shape2: HTTP Status "20*" check → TRUE path

**Populated Response Fields:**

| Field Name | Source | Populated By | Value |
|---|---|---|---|
| status | default (map) | shape34 (Oracle Fusion Leave Response Map) | "success" |
| message | default (map) | shape34 (Oracle Fusion Leave Response Map) | "Data successfully sent to Oracle Fusion" |
| personAbsenceEntryId | operation response | shape33 (Leave Oracle Fusion Create) | Oracle Fusion generated ID |
| success | default (map) | shape34 (Oracle Fusion Leave Response Map) | "true" |

**Response JSON Example:**
```json
{
  "status": "success",
  "message": "Data successfully sent to Oracle Fusion",
  "personAbsenceEntryId": 300000123456789,
  "success": "true"
}
```

#### Return Path 2: Error Response - HTTP Non-20x with gzip encoding (shape48)

**Return Label:** "Error Response"  
**Return Shape ID:** shape48  
**HTTP Status Code:** 400 (inferred from non-20x status)  
**Decision Conditions Leading to Return:**
- Decision shape2: HTTP Status "20*" check → FALSE path
- Decision shape44: Response Content-Encoding equals "gzip" → TRUE path
- Groovy script shape45 decompresses gzip response

**Populated Response Fields:**

| Field Name | Source | Populated By | Value |
|---|---|---|---|
| status | default (map) | shape47 (Leave Error Map) | "failure" |
| message | process property | shape46 → shape47 | Error message from decompressed response (stored in process.DPP_ErrorMessage) |
| success | default (map) | shape47 (Leave Error Map) | "false" |

**Response JSON Example:**
```json
{
  "status": "failure",
  "message": "[Decompressed error message from Oracle Fusion]",
  "success": "false"
}
```

#### Return Path 3: Error Response - HTTP Non-20x without gzip (shape36)

**Return Label:** "Error Response"  
**Return Shape ID:** shape36  
**HTTP Status Code:** 400 (inferred from non-20x status)  
**Decision Conditions Leading to Return:**
- Decision shape2: HTTP Status "20*" check → FALSE path
- Decision shape44: Response Content-Encoding equals "gzip" → FALSE path

**Populated Response Fields:**

| Field Name | Source | Populated By | Value |
|---|---|---|---|
| status | default (map) | shape40 (Leave Error Map) | "failure" |
| message | track property | shape39 → shape40 | Error message from meta.base.applicationstatusmessage |
| success | default (map) | shape40 (Leave Error Map) | "false" |

**Response JSON Example:**
```json
{
  "status": "failure",
  "message": "[Error message from Oracle Fusion HTTP response]",
  "success": "false"
}
```

#### Return Path 4: Error Response - Try/Catch Exception (shape43)

**Return Label:** "Error Response"  
**Return Shape ID:** shape43  
**HTTP Status Code:** 500 (inferred from exception)  
**Decision Conditions Leading to Return:**
- Try/Catch shape17: Error caught in Catch path
- Branch shape20: Path 2

**Populated Response Fields:**

| Field Name | Source | Populated By | Value |
|---|---|---|---|
| status | default (map) | shape41 (Leave Error Map) | "failure" |
| message | process property | shape19 → shape41 | Error message from meta.base.catcherrorsmessage |
| success | default (map) | shape41 (Leave Error Map) | "false" |

**Response JSON Example:**
```json
{
  "status": "failure",
  "message": "[Exception message from try/catch]",
  "success": "false"
}
```

### Downstream Operation HTTP Status Codes

#### Operation: Leave Oracle Fusion Create (6e8920fd-af5a-430b-a1d9-9fde7ac29a12)

**Expected Success Codes:** 200, 201  
**Error Status Codes:** 400, 404, 500  
**Error Handling:** returnErrors=true, returnResponses=true - Process continues with error response, decision shape2 checks status code  

**Response Header Handling:**
- Content-Encoding header mapped to DDP_RespHeader
- If "gzip", response is decompressed using Groovy script

---

## 7. Process Properties Analysis (Steps 2-3)

### Process Properties WRITTEN

| Property ID | Property Name | Written By Shape(s) | Source | Purpose |
|---|---|---|---|---|
| process.DPP_Process_Name | DPP_Process_Name | shape38 | Execution property: Process Name | Store process name for error email |
| process.DPP_AtomName | DPP_AtomName | shape38 | Execution property: Atom Name | Store atom name for error email |
| process.DPP_Payload | DPP_Payload | shape38 | Current document | Store input payload for error email attachment |
| process.DPP_ExecutionID | DPP_ExecutionID | shape38 | Execution property: Execution Id | Store execution ID for error email |
| process.DPP_File_Name | DPP_File_Name | shape38 | Concatenation: Process Name + Date + ".txt" | Generate filename for error email attachment |
| process.DPP_Subject | DPP_Subject | shape38 | Concatenation: Atom Name + " (" + Process Name + " ) has errors to report" | Generate email subject |
| process.To_Email | To_Email | shape38 | Defined property: PP_HCM_LeaveCreate_Properties.To_Email | Email recipient for error notifications |
| process.DPP_HasAttachment | DPP_HasAttachment | shape38 | Defined property: PP_HCM_LeaveCreate_Properties.DPP_HasAttachment | Flag indicating if email should have attachment |
| process.DPP_ErrorMessage | DPP_ErrorMessage | shape19, shape39, shape46 | Track property: meta.base.catcherrorsmessage or meta.base.applicationstatusmessage | Store error message for response |
| dynamicdocument.URL | URL | shape8 | Defined property: PP_HCM_LeaveCreate_Properties.Resource_Path | Oracle Fusion API resource path |
| process.DPP_MailBody | DPP_MailBody | shape14 (subprocess), shape22 (subprocess) | Current document (HTML email body) | Store email body content |

### Process Properties READ

| Property ID | Property Name | Read By Shape(s) | Purpose |
|---|---|---|---|
| dynamicdocument.URL | URL | shape33 (operation) | Used as path parameter in HTTP operation |
| process.To_Email | To_Email | shape6 (subprocess), shape20 (subprocess) | Email recipient address |
| process.DPP_Subject | DPP_Subject | shape6 (subprocess), shape20 (subprocess) | Email subject line |
| process.DPP_MailBody | DPP_MailBody | shape6 (subprocess), shape20 (subprocess) | Email body content |
| process.DPP_File_Name | DPP_File_Name | shape6 (subprocess) | Email attachment filename |
| process.DPP_Process_Name | DPP_Process_Name | shape11 (subprocess), shape23 (subprocess) | Used in email body template |
| process.DPP_AtomName | DPP_AtomName | shape11 (subprocess), shape23 (subprocess) | Used in email body template |
| process.DPP_ExecutionID | DPP_ExecutionID | shape11 (subprocess), shape23 (subprocess) | Used in email body template |
| process.DPP_ErrorMessage | DPP_ErrorMessage | shape11 (subprocess), shape23 (subprocess), map f46b845a | Used in email body template and error response |
| process.DPP_Payload | DPP_Payload | shape15 (subprocess) | Used as email attachment content |
| process.DPP_HasAttachment | DPP_HasAttachment | shape4 (subprocess decision) | Determines email operation path |

### Defined Process Properties

#### PP_HCM_LeaveCreate_Properties (e22c04db-7dfa-4b1b-9d88-77365b0fdb18)

| Property Key | Property Label | Type | Default Value | Purpose |
|---|---|---|---|---|
| c2d1a1e8-4bcb-435b-b1d2-bb10a209bcc0 | Resource_Path | string | hcmRestApi/resources/11.13.18.05/absences | Oracle Fusion API endpoint path |
| 71c90f6e-4f86-49f3-8aef-bae6668c73f9 | To_Email | string | BoomiIntegrationTeam@al-ghurair.com | Error notification email recipient |
| a717867b-67f4-4a72-be2d-c99c73309fdb | DPP_HasAttachment | string | Y | Flag for email attachment (Y/N) |

#### PP_Office365_Email (0feff13f-8a2c-438a-b3c7-1909e2a7f533)

| Property Key | Property Label | Type | Default Value | Purpose |
|---|---|---|---|---|
| 804b6d40-eeee-4ac0-ab4b-94e1aca9f4b7 | From_Email | string | Boomi.Dev.failures@al-ghurair.com | Email sender address |
| 600acadb-ee02-4369-af85-ee70af380b6c | To_Email | string | Rajesh.Muppala@al-ghurair.com;mohan.jonnalagadda@al-ghurair.com | Default email recipients |
| 2fa6ce9e-437a-44cc-b44f-5c7e61052f41 | HasAttachment | string | Y | Default attachment flag |
| 3ca9f307-cecb-4d1e-b9ec-007839509ed7 | EmailBody | string | (empty) | Default email body |
| d8fc7496-ec2c-4308-a4ca-e9f49c0c9500 | Environment | string | DEV Failure : | Environment prefix for email subject |

---

## 8. Data Dependency Graph (Step 4)

### Dependency Chains

**Chain 1: Input Properties Setup → HTTP Operation**
```
shape38 (Input_details) 
  WRITES: process.DPP_Process_Name, process.DPP_AtomName, process.DPP_Payload, 
          process.DPP_ExecutionID, process.DPP_File_Name, process.DPP_Subject, 
          process.To_Email, process.DPP_HasAttachment
  ↓
shape8 (set URL)
  WRITES: dynamicdocument.URL
  ↓
shape33 (Leave Oracle Fusion Create)
  READS: dynamicdocument.URL
```

**Chain 2: Error Handling → Email Subprocess**
```
shape19 (ErrorMsg - Catch path)
  WRITES: process.DPP_ErrorMessage
  READS: meta.base.catcherrorsmessage
  ↓
shape21 (ProcessCall: Office 365 Email subprocess)
  READS: process.DPP_ErrorMessage, process.To_Email, process.DPP_Subject, 
         process.DPP_MailBody, process.DPP_File_Name, process.DPP_Payload, 
         process.DPP_HasAttachment
```

**Chain 3: HTTP Error Response → Error Response**
```
shape39 (error msg - HTTP error path)
  WRITES: process.DPP_ErrorMessage
  READS: meta.base.applicationstatusmessage
  ↓
shape40 (Leave Error Map)
  READS: process.DPP_ErrorMessage (via map function)
  ↓
shape36 (Error Response)
```

**Chain 4: Gzip Decompression → Error Response**
```
shape46 (error msg - gzip path)
  WRITES: process.DPP_ErrorMessage
  READS: meta.base.applicationstatusmessage (current document after decompression)
  ↓
shape47 (Leave Error Map)
  READS: process.DPP_ErrorMessage (via map function)
  ↓
shape48 (Error Response)
```

**Chain 5: Subprocess Email Body Generation**
```
shape11 or shape23 (Mail_Body - subprocess)
  READS: process.DPP_Process_Name, process.DPP_AtomName, process.DPP_ExecutionID, process.DPP_ErrorMessage
  ↓
shape14 or shape22 (set_MailBody - subprocess)
  WRITES: process.DPP_MailBody
  READS: current document (HTML email body)
  ↓
shape15 (payload - subprocess, only with attachment)
  READS: process.DPP_Payload
  ↓
shape6 or shape20 (set_Mail_Properties - subprocess)
  READS: process.DPP_MailBody, process.To_Email, process.DPP_Subject, process.DPP_File_Name
  ↓
shape3 or shape7 (Email operation - subprocess)
```

### Independent Operations
- shape29 (map) - No property dependencies (transforms input document)
- shape34 (map) - No property dependencies (transforms Oracle Fusion response)
- shape49 (notify) - Reads current document for logging

### Property Summary

**Properties Creating Dependencies:**
1. **dynamicdocument.URL** - Written by shape8, read by shape33 (HTTP operation)
2. **process.DPP_ErrorMessage** - Written by shape19/shape39/shape46, read by subprocess and error maps
3. **process.DPP_MailBody** - Written by shape14/shape22 (subprocess), read by shape6/shape20 (subprocess)
4. **process.To_Email, process.DPP_Subject, process.DPP_File_Name** - Written by shape38, read by subprocess
5. **process.DPP_Process_Name, process.DPP_AtomName, process.DPP_ExecutionID, process.DPP_Payload** - Written by shape38, read by subprocess

---

## 9. Control Flow Graph (Step 5)

### Main Process Control Flow

**Shape Connections (Dragpoints):**

| From Shape | To Shape | Identifier | Description |
|---|---|---|---|
| shape1 (start) | shape38 | default | Start → Input properties setup |
| shape38 (Input_details) | shape17 | default | Input setup → Try/Catch wrapper |
| shape17 (catcherrors) | shape29 | default | Try path → Map input |
| shape17 (catcherrors) | shape20 | error | Catch path → Branch (error handling) |
| shape29 (map) | shape8 | default | Map → Set URL property |
| shape8 (set URL) | shape49 | default | Set URL → Notify (log) |
| shape49 (notify) | shape33 | default | Notify → HTTP operation |
| shape33 (connectoraction) | shape2 | default | HTTP operation → HTTP status decision |
| shape2 (decision) | shape34 | true | HTTP 20x → Success map |
| shape2 (decision) | shape44 | false | HTTP non-20x → Content-Type decision |
| shape34 (map) | shape35 | default | Success map → Success response |
| shape44 (decision) | shape45 | true | gzip encoding → Decompress |
| shape44 (decision) | shape39 | false | No gzip → Extract error message |
| shape45 (dataprocess) | shape46 | default | Decompress → Extract error message |
| shape46 (documentproperties) | shape47 | default | Extract error → Error map |
| shape47 (map) | shape48 | default | Error map → Error response |
| shape39 (documentproperties) | shape40 | default | Extract error → Error map |
| shape40 (map) | shape36 | default | Error map → Error response |
| shape20 (branch) | shape19 | 1 | Branch path 1 → Extract error message |
| shape20 (branch) | shape41 | 2 | Branch path 2 → Error map |
| shape19 (documentproperties) | shape21 | default | Extract error → Email subprocess |
| shape41 (map) | shape43 | default | Error map → Error response |

### Subprocess Control Flow (Office 365 Email - a85945c5-3004-42b9-80b1-104f465cd1fb)

| From Shape | To Shape | Identifier | Description |
|---|---|---|---|
| shape1 (start) | shape2 | default | Start → Try/Catch wrapper |
| shape2 (catcherrors) | shape4 | default | Try path → Attachment check decision |
| shape2 (catcherrors) | shape10 | error | Catch path → Exception |
| shape4 (decision) | shape11 | true | Has attachment → Generate email body |
| shape4 (decision) | shape23 | false | No attachment → Generate email body |
| shape11 (message) | shape14 | default | Email body → Set mail body property |
| shape14 (documentproperties) | shape15 | default | Set property → Payload message |
| shape15 (message) | shape6 | default | Payload → Set mail properties |
| shape6 (documentproperties) | shape3 | default | Set properties → Send email with attachment |
| shape3 (connectoraction) | shape5 | default | Send email → Stop (success) |
| shape23 (message) | shape22 | default | Email body → Set mail body property |
| shape22 (documentproperties) | shape20 | default | Set property → Set mail properties |
| shape20 (documentproperties) | shape7 | default | Set properties → Send email without attachment |
| shape7 (connectoraction) | shape9 | default | Send email → Stop (success) |

### Connection Summary
- **Main Process:** 21 shapes with 20 connections
- **Subprocess:** 13 shapes with 12 connections
- **Shapes with Multiple Outgoing Connections:**
  - shape17 (Try/Catch): 2 paths (default, error)
  - shape2 (Decision): 2 paths (true, false)
  - shape44 (Decision): 2 paths (true, false)
  - shape20 (Branch): 2 paths (1, 2)
  - shape2 (subprocess Try/Catch): 2 paths (default, error)
  - shape4 (subprocess Decision): 2 paths (true, false)

### Reverse Flow Mapping (Step 6)

**Convergence Points:**
- **None in main process** - Each path terminates independently with Return Documents
- **None in subprocess** - Each path terminates independently with Stop

**Multiple Incoming Connections:**
- No shapes have multiple incoming connections (no convergence points)

---

## 10. Decision Shape Analysis (Step 7)

### Decision Inventory

#### Decision 1: HTTP Status 20 check (shape2)

**Shape ID:** shape2  
**Comparison Type:** wildcard  
**Value 1:** meta.base.applicationstatuscode (track property)  
**Value 2:** "20*" (static)  

**Data Source:** TRACK_PROPERTY (meta.base.applicationstatuscode)  
**Decision Type:** POST_OPERATION (checks HTTP response status from shape33)  
**Actual Execution Order:** shape33 (HTTP operation) → Response received → shape2 (Decision) → Route based on status  

**TRUE Path:**
- **Destination:** shape34 (Oracle Fusion Leave Response Map)
- **Termination:** shape35 (Return Documents - Success Response) [HTTP 200]
- **Pattern:** Success path - Maps Oracle Fusion response to success response

**FALSE Path:**
- **Destination:** shape44 (Check Response Content Type decision)
- **Termination:** Multiple terminations:
  - shape48 (Return Documents - Error Response) [HTTP 400] - gzip path
  - shape36 (Return Documents - Error Response) [HTTP 400] - non-gzip path
- **Pattern:** Error path - Handles HTTP error responses

**Pattern Type:** Error Check (Success vs Failure)  
**Convergence Point:** None (paths terminate independently)  
**Early Exit:** No (both paths return responses)  

**Business Logic:**
- If Oracle Fusion returns HTTP 20x (success), extract personAbsenceEntryId and return success response
- If Oracle Fusion returns non-20x (error), extract error message and return failure response

#### Decision 2: Check Response Content Type (shape44)

**Shape ID:** shape44  
**Comparison Type:** equals  
**Value 1:** dynamicdocument.DDP_RespHeader (track property - Content-Encoding header)  
**Value 2:** "gzip" (static)  

**Data Source:** TRACK_PROPERTY (dynamicdocument.DDP_RespHeader)  
**Decision Type:** POST_OPERATION (checks HTTP response header from shape33)  
**Actual Execution Order:** shape33 (HTTP operation) → Response received → shape2 (Decision - FALSE path) → shape44 (Decision) → Route based on encoding  

**TRUE Path:**
- **Destination:** shape45 (Custom Scripting - Decompress gzip)
- **Termination:** shape48 (Return Documents - Error Response) [HTTP 400]
- **Pattern:** Decompress gzip response, extract error message, return error response

**FALSE Path:**
- **Destination:** shape39 (error msg - Extract error from response)
- **Termination:** shape36 (Return Documents - Error Response) [HTTP 400]
- **Pattern:** Extract error message directly, return error response

**Pattern Type:** Conditional Logic (Handle gzip vs non-gzip error responses)  
**Convergence Point:** None (paths terminate independently)  
**Early Exit:** No (both paths return error responses)  

**Business Logic:**
- If Oracle Fusion error response is gzip-compressed, decompress before extracting error message
- If Oracle Fusion error response is not compressed, extract error message directly

### Subprocess Decision: Attachment_Check (shape4 - subprocess)

**Shape ID:** shape4 (subprocess a85945c5-3004-42b9-80b1-104f465cd1fb)  
**Comparison Type:** equals  
**Value 1:** process.DPP_HasAttachment (process property)  
**Value 2:** "Y" (static)  

**Data Source:** PROCESS_PROPERTY (process.DPP_HasAttachment)  
**Decision Type:** PRE_FILTER (checks input property to determine email operation)  
**Actual Execution Order:** Decision → Route to appropriate email operation  

**TRUE Path:**
- **Destination:** shape11 (Mail_Body - Generate HTML email body)
- **Termination:** shape5 (Stop - continue=true) [SUCCESS RETURN]
- **Pattern:** Generate email with attachment (includes payload file)

**FALSE Path:**
- **Destination:** shape23 (Mail_Body - Generate HTML email body)
- **Termination:** shape9 (Stop - continue=true) [SUCCESS RETURN]
- **Pattern:** Generate email without attachment

**Pattern Type:** Conditional Logic (Email with/without attachment)  
**Convergence Point:** None (paths terminate independently with Stop)  
**Early Exit:** No (both paths complete successfully)  

**Business Logic:**
- If DPP_HasAttachment = "Y", send email with payload as attachment
- If DPP_HasAttachment ≠ "Y", send email without attachment

### Self-Check Results

✅ **Decision data sources identified:** YES
- shape2: TRACK_PROPERTY (meta.base.applicationstatuscode)
- shape44: TRACK_PROPERTY (dynamicdocument.DDP_RespHeader)
- shape4 (subprocess): PROCESS_PROPERTY (process.DPP_HasAttachment)

✅ **Decision types classified:** YES
- shape2: POST_OPERATION (checks HTTP response status)
- shape44: POST_OPERATION (checks HTTP response header)
- shape4 (subprocess): PRE_FILTER (checks input property)

✅ **Execution order verified:** YES
- shape2: HTTP operation → Response → Decision → Route
- shape44: HTTP operation → Response → shape2 (FALSE) → Decision → Route
- shape4 (subprocess): Decision → Route to email operation

✅ **All decision paths traced:** YES
- shape2: TRUE → shape34 → shape35 (Success), FALSE → shape44 → shape48/shape36 (Error)
- shape44: TRUE → shape45 → shape46 → shape47 → shape48 (Error), FALSE → shape39 → shape40 → shape36 (Error)
- shape4 (subprocess): TRUE → shape11 → ... → shape5 (Stop), FALSE → shape23 → ... → shape9 (Stop)

✅ **Decision patterns identified:** YES
- shape2: Error Check (Success vs Failure)
- shape44: Conditional Logic (Handle gzip vs non-gzip)
- shape4 (subprocess): Conditional Logic (Email with/without attachment)

---

## 11. Branch Shape Analysis (Step 8)

### Branch 1: Error Handling Branch (shape20)

**Shape ID:** shape20  
**Number of Paths:** 2  
**Location:** Catch error path in main process  

### Properties Analysis

**Path 1 (Branch identifier "1"):**
- **Destination:** shape19 (ErrorMsg - documentproperties)
- **Properties READ:** meta.base.catcherrorsmessage (track property)
- **Properties WRITTEN:** process.DPP_ErrorMessage
- **Subsequent Operations:** shape21 (ProcessCall - Email subprocess)

**Path 2 (Branch identifier "2"):**
- **Destination:** shape41 (Leave Error Map)
- **Properties READ:** process.DPP_ErrorMessage (via map function PropertyGet)
- **Properties WRITTEN:** None (map operation)
- **Subsequent Operations:** shape43 (Return Documents - Error Response)

### Dependency Graph

**Dependencies:**
- Path 2 reads process.DPP_ErrorMessage
- Path 1 writes process.DPP_ErrorMessage
- **Therefore:** Path 1 MUST execute BEFORE Path 2

**Dependency Chain:**
```
Path 1: shape19 (writes process.DPP_ErrorMessage) → shape21 (subprocess)
  ↓ (dependency)
Path 2: shape41 (reads process.DPP_ErrorMessage via map) → shape43 (Return Documents)
```

### Classification

**Classification:** SEQUENTIAL  
**Reason:** Path 2 depends on Path 1 (reads property written by Path 1)  

**API Call Detection:** No API calls in branch paths (only documentproperties, map, processcall, returndocuments)

### Topological Sort Order

**Execution Order:** Path 1 → Path 2

**Topological Sort Proof:**
1. Path 1 has no dependencies (incoming edges = 0)
2. Add Path 1 to sorted list
3. Remove Path 1 from graph
4. Path 2 now has no dependencies (Path 1 removed)
5. Add Path 2 to sorted list
6. All paths sorted: [Path 1, Path 2]

### Path Termination

**Path 1 Termination:**
- **Terminal Shape:** shape21 (ProcessCall - Email subprocess)
- **Termination Type:** Subprocess call (continues after subprocess completes)
- **Note:** Subprocess sends error notification email, then main process continues

**Path 2 Termination:**
- **Terminal Shape:** shape43 (Return Documents - Error Response)
- **Termination Type:** Return Documents [HTTP 500]

### Convergence Points

**Convergence:** None  
**Reason:** Path 1 calls subprocess (non-terminating), Path 2 returns documents (terminating). Paths do not converge.

### Execution Continuation

**Execution Continues From:** None  
**Reason:** Path 2 terminates with Return Documents. Path 1 calls subprocess but does not have explicit continuation after subprocess returns.

### Self-Check Results

✅ **Classification completed:** YES  
✅ **Assumption check:** NO (analyzed dependencies)  
✅ **Properties extracted:** YES
- Path 1: READS meta.base.catcherrorsmessage, WRITES process.DPP_ErrorMessage
- Path 2: READS process.DPP_ErrorMessage (via map function)

✅ **Dependency graph built:** YES
```
Path 1 (shape19) writes process.DPP_ErrorMessage
  ↓ (dependency)
Path 2 (shape41) reads process.DPP_ErrorMessage
```

✅ **Topological sort applied:** YES
- Execution Order: Path 1 → Path 2

---

## 12. Subprocess Analysis (Step 7a)

### Subprocess: Office 365 Email (a85945c5-3004-42b9-80b1-104f465cd1fb)

**Subprocess ID:** a85945c5-3004-42b9-80b1-104f465cd1fb  
**Subprocess Name:** (Sub) Office 365 Email  
**Purpose:** Send error notification emails with or without attachments  

### Internal Flow

**Start → Try/Catch → Decision (Attachment Check) → Branch to Email Operations → Stop**

**Flow Diagram:**
```
shape1 (start)
  ↓
shape2 (catcherrors - Try/Catch wrapper)
  ↓ (default - Try path)
shape4 (decision - Attachment_Check: process.DPP_HasAttachment equals "Y"?)
  ↓ TRUE                                    ↓ FALSE
shape11 (message - Mail_Body HTML)          shape23 (message - Mail_Body HTML)
  ↓                                          ↓
shape14 (set_MailBody property)              shape22 (set_MailBody property)
  ↓                                          ↓
shape15 (message - payload)                  shape20 (set_Mail_Properties)
  ↓                                          ↓
shape6 (set_Mail_Properties)                 shape7 (Email W/O Attachment)
  ↓                                          ↓
shape3 (Email w Attachment)                  shape9 (Stop - continue=true)
  ↓
shape5 (Stop - continue=true)

shape2 (Catch path - error)
  ↓
shape10 (exception - throws exception with catch error message)
```

### Return Paths

**Return Path 1: Success (Implicit)**
- **Label:** SUCCESS (implicit via Stop with continue=true)
- **Shape ID:** shape5 or shape9
- **Condition:** Email sent successfully (with or without attachment)
- **Path:** Normal execution completes with Stop (continue=true)

**Return Path 2: Exception**
- **Label:** ERROR (implicit via exception shape)
- **Shape ID:** shape10
- **Condition:** Error caught in Try/Catch (email send failure)
- **Path:** Catch path → Exception thrown

### Main Process Mapping

**ProcessCall Shape:** shape21  
**Subprocess ID:** a85945c5-3004-42b9-80b1-104f465cd1fb  
**Return Path Mapping:** None explicitly defined (subprocess uses Stop with continue=true for success, exception for error)  

**Behavior:**
- Subprocess executes completely
- On success (Stop with continue=true), main process continues (no explicit next shape after shape21)
- On exception, exception propagates to main process (already in catch path, so exception is handled by outer error handling)

### Properties Written by Subprocess

| Property ID | Written By Shape | Purpose |
|---|---|---|
| process.DPP_MailBody | shape14, shape22 | Store generated HTML email body |
| connector.mail.fromAddress | shape6, shape20 | Set email sender address |
| connector.mail.toAddress | shape6, shape20 | Set email recipient address |
| connector.mail.subject | shape6, shape20 | Set email subject line |
| connector.mail.body | shape6, shape20 | Set email body content |
| connector.mail.filename | shape6 | Set email attachment filename (only with attachment path) |

### Properties Read by Subprocess (from Main Process)

| Property ID | Read By Shape | Purpose |
|---|---|---|
| process.DPP_HasAttachment | shape4 | Determine email operation path (with/without attachment) |
| process.DPP_Process_Name | shape11, shape23 | Used in email body template |
| process.DPP_AtomName | shape11, shape23 | Used in email body template |
| process.DPP_ExecutionID | shape11, shape23 | Used in email body template |
| process.DPP_ErrorMessage | shape11, shape23 | Used in email body template |
| process.DPP_Payload | shape15 | Used as email attachment content |
| process.To_Email | shape6, shape20 | Email recipient address |
| process.DPP_Subject | shape6, shape20 | Email subject line |
| process.DPP_File_Name | shape6 | Email attachment filename |

### Subprocess Execution Pattern

**Execution:** Synchronous (wait=true, abort=true)  
**Error Handling:** Exception propagates to main process  
**Return Behavior:** Continues after Stop (continue=true), no explicit return path mapping  

---

## 13. Execution Order (Step 9)

### Business Logic Flow (Step 0 - MUST BE FIRST)

#### Operation Analysis

**Operation 1: Create Leave Oracle Fusion OP (8f709c2b-e63f-4d5f-9374-2932ed70415d)**
- **Purpose:** Entry point - Receives leave creation request from D365
- **Outputs:** Input document (leave request JSON)
- **Dependent Operations:** All downstream operations depend on this input
- **Business Flow:** Entry point MUST execute FIRST (receives request)

**Operation 2: Leave Oracle Fusion Create (6e8920fd-af5a-430b-a1d9-9fde7ac29a12)**
- **Purpose:** Create absence entry in Oracle Fusion HCM via HTTP POST
- **Outputs:** 
  - HTTP response with personAbsenceEntryId (on success)
  - HTTP status code (meta.base.applicationstatuscode)
  - Response headers (Content-Encoding → DDP_RespHeader)
- **Dependent Operations:** 
  - Decision shape2 checks HTTP status code
  - Decision shape44 checks Content-Encoding header
  - Map shape34 extracts personAbsenceEntryId from response
- **Business Flow:** Oracle Fusion Create MUST execute BEFORE decisions and response mapping

**Operation 3: Email w Attachment / Email W/O Attachment (subprocess operations)**
- **Purpose:** Send error notification emails
- **Outputs:** Email sent (no data returned to main process)
- **Dependent Operations:** None (terminal operation in error path)
- **Business Flow:** Email operations execute AFTER error is captured and properties are set

#### Business Logic Execution Order

1. **Entry Point:** Receive leave creation request from D365
2. **Input Setup:** Extract execution metadata (process name, atom name, execution ID, payload)
3. **Transform Request:** Map D365 request to Oracle Fusion format
4. **Set API URL:** Configure Oracle Fusion API endpoint
5. **Call Oracle Fusion:** POST leave absence entry to Oracle Fusion HCM
6. **Check HTTP Status:** Determine if Oracle Fusion call succeeded (20x) or failed (non-20x)
7. **Success Path:** Extract personAbsenceEntryId, map to success response, return to D365
8. **Error Path:** Extract error message (decompress if gzip), map to error response, return to D365
9. **Exception Path:** Capture exception, send error notification email, return error response to D365

### Execution Order (Based on Business Logic and Data Dependencies)

**Based on dependency graph in Step 4, decision analysis in Step 7, control flow graph in Step 5, branch analysis in Step 8.**

```
START (shape1)
  ↓
shape38 (Input_details - documentproperties)
  WRITES: process.DPP_Process_Name, process.DPP_AtomName, process.DPP_Payload, 
          process.DPP_ExecutionID, process.DPP_File_Name, process.DPP_Subject, 
          process.To_Email, process.DPP_HasAttachment
  ↓
TRY/CATCH (shape17) - Try path:
  ↓
  shape29 (Leave Create Map)
    Transforms D365 request to Oracle Fusion format
    ↓
  shape8 (set URL - documentproperties)
    WRITES: dynamicdocument.URL
    ↓
  shape49 (notify - log current document)
    READS: current document
    ↓
  shape33 (Leave Oracle Fusion Create - HTTP POST)
    READS: dynamicdocument.URL
    Produces: HTTP response, status code, headers
    ↓
  DECISION (shape2 - HTTP Status 20 check)
    READS: meta.base.applicationstatuscode
    ↓
    IF TRUE (HTTP 20x - Success):
      ↓
      shape34 (Oracle Fusion Leave Response Map)
        Transforms Oracle Fusion response to D365 response format
        Maps personAbsenceEntryId
        Sets default values: status="success", message="Data successfully sent to Oracle Fusion", success="true"
        ↓
      shape35 (Return Documents - Success Response) [HTTP 200]
        Returns success response to D365
    
    IF FALSE (HTTP non-20x - Error):
      ↓
      DECISION (shape44 - Check Response Content Type)
        READS: dynamicdocument.DDP_RespHeader (Content-Encoding header)
        ↓
        IF TRUE (gzip encoding):
          ↓
          shape45 (Custom Scripting - Decompress gzip)
            Decompresses gzip response using Groovy script
            ↓
          shape46 (error msg - documentproperties)
            WRITES: process.DPP_ErrorMessage
            READS: current document (decompressed error message)
            ↓
          shape47 (Leave Error Map)
            READS: process.DPP_ErrorMessage (via map function PropertyGet)
            Sets default values: status="failure", success="false"
            ↓
          shape48 (Return Documents - Error Response) [HTTP 400]
            Returns error response to D365
        
        IF FALSE (no gzip encoding):
          ↓
          shape39 (error msg - documentproperties)
            WRITES: process.DPP_ErrorMessage
            READS: meta.base.applicationstatusmessage
            ↓
          shape40 (Leave Error Map)
            READS: process.DPP_ErrorMessage (via map function PropertyGet)
            Sets default values: status="failure", success="false"
            ↓
          shape36 (Return Documents - Error Response) [HTTP 400]
            Returns error response to D365

TRY/CATCH (shape17) - Catch path (Exception):
  ↓
  BRANCH (shape20) - Sequential execution (Path 1 → Path 2):
    ↓
    Path 1:
      ↓
      shape19 (ErrorMsg - documentproperties)
        WRITES: process.DPP_ErrorMessage
        READS: meta.base.catcherrorsmessage
        ↓
      shape21 (ProcessCall - Office 365 Email subprocess)
        READS: process.DPP_ErrorMessage, process.To_Email, process.DPP_Subject, 
               process.DPP_MailBody, process.DPP_File_Name, process.DPP_Payload, 
               process.DPP_HasAttachment
        ↓
        SUBPROCESS INTERNAL FLOW:
          ↓
          shape1 (start - subprocess)
            ↓
          TRY/CATCH (shape2 - subprocess) - Try path:
            ↓
            DECISION (shape4 - Attachment_Check)
              READS: process.DPP_HasAttachment
              ↓
              IF TRUE (Has attachment):
                ↓
                shape11 (Mail_Body - message)
                  READS: process.DPP_Process_Name, process.DPP_AtomName, 
                         process.DPP_ExecutionID, process.DPP_ErrorMessage
                  Generates HTML email body
                  ↓
                shape14 (set_MailBody - documentproperties)
                  WRITES: process.DPP_MailBody
                  READS: current document
                  ↓
                shape15 (payload - message)
                  READS: process.DPP_Payload
                  ↓
                shape6 (set_Mail_Properties - documentproperties)
                  READS: process.DPP_MailBody, process.To_Email, 
                         process.DPP_Subject, process.DPP_File_Name
                  WRITES: connector.mail.* properties
                  ↓
                shape3 (Email w Attachment - mail operation)
                  Sends email with attachment
                  ↓
                shape5 (Stop - continue=true) [SUCCESS RETURN]
              
              IF FALSE (No attachment):
                ↓
                shape23 (Mail_Body - message)
                  READS: process.DPP_Process_Name, process.DPP_AtomName, 
                         process.DPP_ExecutionID, process.DPP_ErrorMessage
                  Generates HTML email body
                  ↓
                shape22 (set_MailBody - documentproperties)
                  WRITES: process.DPP_MailBody
                  READS: current document
                  ↓
                shape20 (set_Mail_Properties - documentproperties)
                  READS: process.DPP_MailBody, process.To_Email, process.DPP_Subject
                  WRITES: connector.mail.* properties
                  ↓
                shape7 (Email W/O Attachment - mail operation)
                  Sends email without attachment
                  ↓
                shape9 (Stop - continue=true) [SUCCESS RETURN]
          
          TRY/CATCH (shape2 - subprocess) - Catch path:
            ↓
            shape10 (exception)
              Throws exception with catch error message
        
        END SUBPROCESS
    
    Path 2 (executes AFTER Path 1 due to dependency):
      ↓
      shape41 (Leave Error Map)
        READS: process.DPP_ErrorMessage (via map function PropertyGet)
        Sets default values: status="failure", success="false"
        ↓
      shape43 (Return Documents - Error Response) [HTTP 500]
        Returns error response to D365

END
```

### Dependency Verification

**Reference to Step 4 (Data Dependency Graph):**

1. **shape8 → shape33:** shape8 writes dynamicdocument.URL, shape33 reads it
   - **Proof:** shape8 MUST execute BEFORE shape33

2. **shape19 → shape21:** shape19 writes process.DPP_ErrorMessage, shape21 (subprocess) reads it
   - **Proof:** shape19 MUST execute BEFORE shape21

3. **shape19 → shape41:** shape19 writes process.DPP_ErrorMessage, shape41 reads it (via map function)
   - **Proof:** shape19 (Path 1) MUST execute BEFORE shape41 (Path 2)

4. **shape38 → subprocess:** shape38 writes multiple properties, subprocess reads them
   - **Proof:** shape38 MUST execute BEFORE subprocess is called

5. **shape39 → shape40:** shape39 writes process.DPP_ErrorMessage, shape40 reads it (via map function)
   - **Proof:** shape39 MUST execute BEFORE shape40

6. **shape46 → shape47:** shape46 writes process.DPP_ErrorMessage, shape47 reads it (via map function)
   - **Proof:** shape46 MUST execute BEFORE shape47

### Branch Execution Order

**Reference to Step 8 (Branch Analysis):**

**Branch shape20 (Error Handling Branch):**
- **Classification:** SEQUENTIAL
- **Topological Sort Order:** Path 1 → Path 2
- **Reason:** Path 2 depends on Path 1 (reads process.DPP_ErrorMessage written by Path 1)
- **Execution:** Path 1 (shape19 → shape21) executes FIRST, then Path 2 (shape41 → shape43) executes

### Decision Path Tracing

**Reference to Step 7 (Decision Analysis):**

**Decision shape2 (HTTP Status 20 check):**
- **TRUE path:** shape34 → shape35 (Success Response)
- **FALSE path:** shape44 → [shape45 → shape46 → shape47 → shape48] OR [shape39 → shape40 → shape36] (Error Response)

**Decision shape44 (Check Response Content Type):**
- **TRUE path:** shape45 → shape46 → shape47 → shape48 (gzip decompression)
- **FALSE path:** shape39 → shape40 → shape36 (direct error extraction)

**Decision shape4 (subprocess - Attachment_Check):**
- **TRUE path:** shape11 → shape14 → shape15 → shape6 → shape3 → shape5 (Email with attachment)
- **FALSE path:** shape23 → shape22 → shape20 → shape7 → shape9 (Email without attachment)

### Self-Check Results

✅ **Business logic verified FIRST:** YES
- Analyzed what each operation does and what it produces
- Identified Oracle Fusion Create as the core operation that produces data consumed by decisions and response mapping

✅ **Operation analysis complete:** YES
- Operation 1: Entry point (receives request)
- Operation 2: Oracle Fusion Create (produces HTTP response, status code, headers)
- Operation 3: Email operations (send error notifications)

✅ **Business logic execution order identified:** YES
- Entry → Input Setup → Transform → Set URL → Call Oracle Fusion → Check Status → Success/Error Path

✅ **Data dependencies checked FIRST:** YES
- Verified all property reads happen after property writes
- Documented dependency chains in Step 4

✅ **Operation response analysis used:** YES
- Referenced Step 1c for Oracle Fusion response structure and extracted fields

✅ **Decision analysis used:** YES
- Referenced Step 7 for decision data sources, types, and path tracing

✅ **Dependency graph used:** YES
- Referenced Step 4 for property dependency chains

✅ **Branch analysis used:** YES
- Referenced Step 8 for branch classification and topological sort order

✅ **Property dependency verification:** YES
- All property reads happen after property writes (verified in dependency chains)

✅ **Topological sort applied:** YES
- Branch shape20: Path 1 → Path 2 (sequential execution)

---

## 14. Sequence Diagram (Step 10)

**Based on execution order in Step 9, dependency graph in Step 4, decision analysis in Step 7, control flow graph in Step 5, and branch analysis in Step 8.**

**📋 NOTE:** Detailed request/response JSON examples are documented in Section 6 (HTTP Status Codes and Return Path Responses) and Section 3 (Response Structure Analysis).

```
START (shape1)
 |
 ├─→ shape38: Input_details (documentproperties)
 |   └─→ WRITES: process.DPP_Process_Name, process.DPP_AtomName, process.DPP_Payload, 
 |                process.DPP_ExecutionID, process.DPP_File_Name, process.DPP_Subject, 
 |                process.To_Email, process.DPP_HasAttachment
 |   └─→ READS: Execution properties (Process Name, Atom Name, Execution Id), Current document
 |
 ├─→ TRY/CATCH (shape17)
 |   |
 |   ├─→ TRY PATH:
 |   |   |
 |   |   ├─→ shape29: Leave Create Map (map)
 |   |   |   └─→ Transforms D365 request to Oracle Fusion format
 |   |   |   └─→ Field mappings: employeeNumber→personNumber, absenceStatusCode→absenceStatusCd, etc.
 |   |   |
 |   |   ├─→ shape8: set URL (documentproperties)
 |   |   |   └─→ WRITES: dynamicdocument.URL
 |   |   |   └─→ READS: Defined property PP_HCM_LeaveCreate_Properties.Resource_Path
 |   |   |
 |   |   ├─→ shape49: notify (log)
 |   |   |   └─→ READS: Current document
 |   |   |   └─→ Logs transformed request for monitoring
 |   |   |
 |   |   ├─→ shape33: Leave Oracle Fusion Create (HTTP POST) [Downstream]
 |   |   |   └─→ READS: dynamicdocument.URL
 |   |   |   └─→ WRITES: HTTP response, meta.base.applicationstatuscode, dynamicdocument.DDP_RespHeader
 |   |   |   └─→ HTTP: [Expected: 200/201, Error: 400/404/500]
 |   |   |   └─→ Endpoint: {base_url}/hcmRestApi/resources/11.13.18.05/absences
 |   |   |   └─→ Method: POST
 |   |   |   └─→ Content-Type: application/json
 |   |   |
 |   |   ├─→ DECISION (shape2): HTTP Status 20 check
 |   |   |   └─→ READS: meta.base.applicationstatuscode
 |   |   |   └─→ Comparison: wildcard match "20*"
 |   |   |   |
 |   |   |   ├─→ IF TRUE (HTTP 20x - Success):
 |   |   |   |   |
 |   |   |   |   ├─→ shape34: Oracle Fusion Leave Response Map (map)
 |   |   |   |   |   └─→ READS: Oracle Fusion response (profile 316175c7)
 |   |   |   |   |   └─→ Maps: personAbsenceEntryId → personAbsenceEntryId
 |   |   |   |   |   └─→ Sets defaults: status="success", message="Data successfully sent to Oracle Fusion", success="true"
 |   |   |   |   |
 |   |   |   |   └─→ shape35: Return Documents [HTTP 200] [SUCCESS]
 |   |   |   |       └─→ Response: {"status":"success", "message":"Data successfully sent to Oracle Fusion", 
 |   |   |   |                      "personAbsenceEntryId":300000123456789, "success":"true"}
 |   |   |   |
 |   |   |   └─→ IF FALSE (HTTP non-20x - Error):
 |   |   |       |
 |   |   |       ├─→ DECISION (shape44): Check Response Content Type
 |   |   |       |   └─→ READS: dynamicdocument.DDP_RespHeader (Content-Encoding header)
 |   |   |       |   └─→ Comparison: equals "gzip"
 |   |   |       |   |
 |   |   |       |   ├─→ IF TRUE (gzip encoding):
 |   |   |       |   |   |
 |   |   |       |   |   ├─→ shape45: Custom Scripting - Decompress gzip (dataprocess)
 |   |   |       |   |   |   └─→ READS: Current document (gzip compressed response)
 |   |   |       |   |   |   └─→ Groovy script: GZIPInputStream decompression
 |   |   |       |   |   |   └─→ WRITES: Decompressed response to current document
 |   |   |       |   |   |
 |   |   |       |   |   ├─→ shape46: error msg (documentproperties)
 |   |   |       |   |   |   └─→ WRITES: process.DPP_ErrorMessage
 |   |   |       |   |   |   └─→ READS: Current document (decompressed error message)
 |   |   |       |   |   |
 |   |   |       |   |   ├─→ shape47: Leave Error Map (map)
 |   |   |       |   |   |   └─→ READS: process.DPP_ErrorMessage (via PropertyGet function)
 |   |   |       |   |   |   └─→ Sets defaults: status="failure", success="false"
 |   |   |       |   |   |
 |   |   |       |   |   └─→ shape48: Return Documents [HTTP 400] [ERROR]
 |   |   |       |   |       └─→ Response: {"status":"failure", "message":"[Decompressed error]", "success":"false"}
 |   |   |       |   |
 |   |   |       |   └─→ IF FALSE (no gzip encoding):
 |   |   |       |       |
 |   |   |       |       ├─→ shape39: error msg (documentproperties)
 |   |   |       |       |   └─→ WRITES: process.DPP_ErrorMessage
 |   |   |       |       |   └─→ READS: meta.base.applicationstatusmessage
 |   |   |       |       |
 |   |   |       |       ├─→ shape40: Leave Error Map (map)
 |   |   |       |       |   └─→ READS: process.DPP_ErrorMessage (via PropertyGet function)
 |   |   |       |       |   └─→ Sets defaults: status="failure", success="false"
 |   |   |       |       |
 |   |   |       |       └─→ shape36: Return Documents [HTTP 400] [ERROR]
 |   |   |       |           └─→ Response: {"status":"failure", "message":"[Error from Oracle]", "success":"false"}
 |   |
 |   └─→ CATCH PATH (Exception):
 |       |
 |       ├─→ BRANCH (shape20) - Sequential Execution (Path 1 → Path 2):
 |       |   |
 |       |   ├─→ PATH 1:
 |       |   |   |
 |       |   |   ├─→ shape19: ErrorMsg (documentproperties)
 |       |   |   |   └─→ WRITES: process.DPP_ErrorMessage
 |       |   |   |   └─→ READS: meta.base.catcherrorsmessage
 |       |   |   |
 |       |   |   └─→ shape21: ProcessCall - Office 365 Email Subprocess
 |       |   |       └─→ READS: process.DPP_ErrorMessage, process.To_Email, process.DPP_Subject, 
 |       |   |                  process.DPP_MailBody, process.DPP_File_Name, process.DPP_Payload, 
 |       |   |                  process.DPP_HasAttachment
 |       |   |       |
 |       |   |       └─→ SUBPROCESS INTERNAL FLOW:
 |       |   |           |
 |       |   |           ├─→ shape1: start (subprocess)
 |       |   |           |
 |       |   |           ├─→ TRY/CATCH (shape2 - subprocess)
 |       |   |           |   |
 |       |   |           |   ├─→ TRY PATH:
 |       |   |           |   |   |
 |       |   |           |   |   ├─→ DECISION (shape4): Attachment_Check
 |       |   |           |   |   |   └─→ READS: process.DPP_HasAttachment
 |       |   |           |   |   |   └─→ Comparison: equals "Y"
 |       |   |           |   |   |   |
 |       |   |           |   |   |   ├─→ IF TRUE (Has attachment):
 |       |   |           |   |   |   |   |
 |       |   |           |   |   |   |   ├─→ shape11: Mail_Body (message)
 |       |   |           |   |   |   |   |   └─→ READS: process.DPP_Process_Name, process.DPP_AtomName, 
 |       |   |           |   |   |   |   |              process.DPP_ExecutionID, process.DPP_ErrorMessage
 |       |   |           |   |   |   |   |   └─→ Generates HTML email body with error details table
 |       |   |           |   |   |   |   |
 |       |   |           |   |   |   |   ├─→ shape14: set_MailBody (documentproperties)
 |       |   |           |   |   |   |   |   └─→ WRITES: process.DPP_MailBody
 |       |   |           |   |   |   |   |   └─→ READS: Current document (HTML email body)
 |       |   |           |   |   |   |   |
 |       |   |           |   |   |   |   ├─→ shape15: payload (message)
 |       |   |           |   |   |   |   |   └─→ READS: process.DPP_Payload
 |       |   |           |   |   |   |   |   └─→ Sets current document to payload for attachment
 |       |   |           |   |   |   |   |
 |       |   |           |   |   |   |   ├─→ shape6: set_Mail_Properties (documentproperties)
 |       |   |           |   |   |   |   |   └─→ READS: process.DPP_MailBody, process.To_Email, 
 |       |   |           |   |   |   |   |              process.DPP_Subject, process.DPP_File_Name
 |       |   |           |   |   |   |   |   └─→ WRITES: connector.mail.fromAddress, connector.mail.toAddress, 
 |       |   |           |   |   |   |   |               connector.mail.subject, connector.mail.body, 
 |       |   |           |   |   |   |   |               connector.mail.filename
 |       |   |           |   |   |   |   |
 |       |   |           |   |   |   |   ├─→ shape3: Email w Attachment (mail operation) [Downstream]
 |       |   |           |   |   |   |   |   └─→ READS: connector.mail.* properties
 |       |   |           |   |   |   |   |   └─→ Sends email with payload as attachment
 |       |   |           |   |   |   |   |   └─→ HTTP: [Expected: 200, Error: 500]
 |       |   |           |   |   |   |   |
 |       |   |           |   |   |   |   └─→ shape5: Stop (continue=true) [SUCCESS RETURN]
 |       |   |           |   |   |   |
 |       |   |           |   |   |   └─→ IF FALSE (No attachment):
 |       |   |           |   |   |       |
 |       |   |           |   |   |       ├─→ shape23: Mail_Body (message)
 |       |   |           |   |   |       |   └─→ READS: process.DPP_Process_Name, process.DPP_AtomName, 
 |       |   |           |   |   |       |              process.DPP_ExecutionID, process.DPP_ErrorMessage
 |       |   |           |   |   |       |   └─→ Generates HTML email body with error details table
 |       |   |           |   |   |       |
 |       |   |           |   |   |       ├─→ shape22: set_MailBody (documentproperties)
 |       |   |           |   |   |       |   └─→ WRITES: process.DPP_MailBody
 |       |   |           |   |   |       |   └─→ READS: Current document (HTML email body)
 |       |   |           |   |   |       |
 |       |   |           |   |   |       ├─→ shape20: set_Mail_Properties (documentproperties)
 |       |   |           |   |   |       |   └─→ READS: process.DPP_MailBody, process.To_Email, process.DPP_Subject
 |       |   |           |   |   |       |   └─→ WRITES: connector.mail.fromAddress, connector.mail.toAddress, 
 |       |   |           |   |   |       |               connector.mail.subject, connector.mail.body
 |       |   |           |   |   |       |
 |       |   |           |   |   |       ├─→ shape7: Email W/O Attachment (mail operation) [Downstream]
 |       |   |           |   |   |       |   └─→ READS: connector.mail.* properties
 |       |   |           |   |   |       |   └─→ Sends email without attachment
 |       |   |           |   |   |       |   └─→ HTTP: [Expected: 200, Error: 500]
 |       |   |           |   |   |       |
 |       |   |           |   |   |       └─→ shape9: Stop (continue=true) [SUCCESS RETURN]
 |       |   |           |   |
 |       |   |           |   └─→ CATCH PATH (Exception):
 |       |   |           |       |
 |       |   |           |       └─→ shape10: exception
 |       |   |           |           └─→ READS: meta.base.catcherrorsmessage
 |       |   |           |           └─→ Throws exception with catch error message
 |       |   |           |
 |       |   |           └─→ END SUBPROCESS
 |       |   |
 |       |   └─→ PATH 2 (executes AFTER Path 1 due to dependency):
 |       |       |
 |       |       ├─→ shape41: Leave Error Map (map)
 |       |       |   └─→ READS: process.DPP_ErrorMessage (via PropertyGet function)
 |       |       |   └─→ Sets defaults: status="failure", success="false"
 |       |       |
 |       |       └─→ shape43: Return Documents [HTTP 500] [ERROR]
 |       |           └─→ Response: {"status":"failure", "message":"[Exception message]", "success":"false"}
 |
END
```

**Note:** Detailed request/response JSON examples for all operations and return paths are documented in Section 6 (HTTP Status Codes and Return Path Responses) and Section 3 (Response Structure Analysis).

---

## 15. System Layer Identification

### Third-Party Systems

#### System 1: Oracle Fusion HCM

**System Name:** Oracle Fusion HCM  
**Purpose:** Human Capital Management system - Manages employee leave/absence entries  
**Connection Type:** HTTP REST API  
**Endpoint:** {base_url}/hcmRestApi/resources/11.13.18.05/absences  
**Authentication:** Basic Auth (configured in connection aa1fcb29-d146-4425-9ea6-b9698090f60e)  
**Operations:**
- POST /absences - Create absence entry

**Data Exchanged:**
- **Request:** Leave absence entry (personNumber, absenceType, employer, startDate, endDate, absenceStatusCd, approvalStatusCd, startDateDuration, endDateDuration)
- **Response:** Created absence entry with personAbsenceEntryId and extensive metadata

#### System 2: Office 365 Email (SMTP)

**System Name:** Office 365 Email  
**Purpose:** Email notification system for error alerts  
**Connection Type:** SMTP  
**Connection ID:** 00eae79b-2303-4215-8067-dcc299e42697  
**Authentication:** SMTP AUTH with username/password  
**Operations:**
- Send email with attachment
- Send email without attachment

**Data Exchanged:**
- **Email Content:** HTML formatted error details (process name, environment, execution ID, error message)
- **Attachments:** Input payload (JSON) when errors occur

---

## 16. Critical Patterns Identified

### Pattern 1: Try/Catch Error Handling with Email Notification

**Identification:**
- Try/Catch wrapper (shape17) around main business logic
- Catch path branches to error handling (shape20)
- Error message captured and sent via email subprocess (shape21)
- Error response returned to caller (shape43)

**Execution Rule:**
- Try path executes main business logic
- If exception occurs, catch path captures error message, sends email notification, and returns error response

### Pattern 2: HTTP Status Code Decision with Multiple Error Paths

**Identification:**
- Decision shape2 checks HTTP status code (20* wildcard)
- Success path (TRUE) maps response and returns success
- Error path (FALSE) branches to additional decision (shape44) to handle gzip compression
- Multiple error response paths converge to return error response

**Execution Rule:**
- Oracle Fusion HTTP operation MUST execute BEFORE decision
- Decision routes to success or error path based on HTTP status
- Error path handles gzip decompression if needed before extracting error message

### Pattern 3: Subprocess with Conditional Email Operations

**Identification:**
- Subprocess (shape21) called from catch path
- Subprocess decision (shape4) checks attachment flag
- Two parallel paths: email with attachment vs email without attachment
- Both paths converge to Stop (continue=true)

**Execution Rule:**
- Subprocess executes synchronously (wait=true)
- Decision determines email operation based on attachment flag
- Subprocess returns to main process after email sent

### Pattern 4: Sequential Branch Execution with Property Dependency

**Identification:**
- Branch shape20 with 2 paths
- Path 1 writes process.DPP_ErrorMessage
- Path 2 reads process.DPP_ErrorMessage (via map function)
- Topological sort required: Path 1 → Path 2

**Execution Rule:**
- Path 1 MUST execute BEFORE Path 2 (data dependency)
- Path 1 sends email notification (non-terminating)
- Path 2 returns error response (terminating)

### Pattern 5: Map-Based Response Transformation with Default Values

**Identification:**
- Success map (shape34) transforms Oracle Fusion response to D365 format
- Error maps (shape40, shape41, shape47) transform error messages to D365 format
- Maps use default values for status, message, and success fields
- Maps use PropertyGet function to read process properties

**Execution Rule:**
- Maps execute after data is available (Oracle Fusion response or error message)
- Default values provide consistent response structure
- PropertyGet function enables reading process properties within maps

---

## 17. Validation Checklist

### Data Dependencies
- [x] All property WRITES identified (Section 7)
- [x] All property READS identified (Section 7)
- [x] Dependency graph built (Section 8)
- [x] Execution order satisfies all dependencies (Section 13)

### Decision Analysis
- [x] ALL decision shapes inventoried (Section 10)
- [x] BOTH TRUE and FALSE paths traced to termination (Section 10)
- [x] Pattern type identified for each decision (Section 10)
- [x] Early exits identified and documented (Section 10)
- [x] Convergence points identified (Section 10)

### Branch Analysis
- [x] Each branch classified as parallel or sequential (Section 11)
- [x] API call detection performed (Section 11)
- [x] Classification based on analysis, not assumption (Section 11)
- [x] If sequential: dependency_order built using topological sort (Section 11)
- [x] Each path traced to terminal point (Section 11)
- [x] Convergence points identified (Section 11)
- [x] Execution continuation point determined (Section 11)

### Sequence Diagram
- [x] Format follows required structure (Section 14)
- [x] Each operation shows READS and WRITES (Section 14)
- [x] Decisions show both TRUE and FALSE paths (Section 14)
- [x] Check-before-create patterns shown correctly (N/A - no check-before-create pattern)
- [x] Cross-validation: Sequence diagram matches control flow graph (Section 9)
- [x] Cross-validation: Execution order matches dependency graph (Section 8)
- [x] Early exits marked (N/A - all paths return responses)
- [x] Conditional execution marked (Section 14)
- [x] Subprocess internal flows documented (Section 14)
- [x] Subprocess return paths mapped to main process (Section 12)

### Subprocess Analysis
- [x] ALL subprocesses analyzed (Section 12)
- [x] Return paths identified (Section 12)
- [x] Return path labels mapped to main process shapes (Section 12)
- [x] Properties written by subprocess documented (Section 12)
- [x] Properties read by subprocess from main process documented (Section 12)

### Edge Cases
- [x] Nested branches/decisions analyzed (N/A - no nested branches)
- [x] Loops identified (N/A - no loops)
- [x] Property chains traced (Section 8)
- [x] Circular dependencies detected and resolved (None found)
- [x] Try/Catch error paths documented (Section 14)

### Property Extraction Completeness
- [x] All property patterns searched (Section 7)
- [x] Message parameters checked for process properties (Section 7)
- [x] Operation headers/path parameters checked (Section 7)
- [x] Decision track properties identified (Section 10)
- [x] Document properties that read other properties identified (Section 7)

### Input/Output Structure Analysis
- [x] Entry point operation identified (Section 2)
- [x] Request profile identified and loaded (Section 2)
- [x] Request profile structure analyzed (Section 2)
- [x] Array vs single object detected (Section 2)
- [x] Array cardinality documented (N/A - single object)
- [x] ALL request fields extracted (Section 2)
- [x] Request field paths documented (Section 2)
- [x] Request field mapping table generated (Section 2)
- [x] Response profile identified and loaded (Section 3)
- [x] Response profile structure analyzed (Section 3)
- [x] ALL response fields extracted (Section 3)
- [x] Response field mapping table generated (Section 3)
- [x] Document processing behavior determined (Section 2)
- [x] Input/Output structure documented (Sections 2-3)

### HTTP Status Codes and Return Path Responses
- [x] Section 6 present (HTTP Status Codes and Return Path Responses)
- [x] All return paths documented with HTTP status codes (Section 6)
- [x] Response JSON examples provided for each return path (Section 6)
- [x] Populated fields documented for each return path (Section 6)
- [x] Decision conditions leading to each return documented (Section 6)
- [x] Error codes and success codes documented (Section 6)
- [x] Downstream operation HTTP status codes documented (Section 6)
- [x] Error handling strategy documented (Section 6)

### Map Analysis
- [x] ALL map files identified and loaded (Section 5)
- [x] Field mappings extracted from each map (Section 5)
- [x] Profile vs map field name discrepancies documented (Section 5)
- [x] Map field names marked as AUTHORITATIVE (Section 5)
- [x] Scripting functions analyzed (Section 5)
- [x] Static values identified and documented (Section 5)
- [x] Process property mappings documented (Section 5)
- [x] Map Analysis documented (Section 5)

---

## 18. Function Exposure Decision Table

### Process Layer Function

**Function Name:** CreateLeaveAbsence  
**HTTP Method:** POST  
**Route:** /api/hcm/leave/create  
**Purpose:** Create leave absence entry in Oracle Fusion HCM from D365 request  

**Expose as Azure Function:** ✅ YES

**Reasoning:**
1. **Entry Point:** Process has Web Service Server Listen operation (wss) - designed to receive external requests
2. **Business Logic:** Orchestrates leave creation between D365 and Oracle Fusion HCM
3. **Error Handling:** Comprehensive error handling with email notifications and structured error responses
4. **Response Contract:** Well-defined response structure (success/failure with message and ID)
5. **Reusability:** Can be called by any system needing to create leave entries in Oracle Fusion HCM

**Request DTO:** CreateLeaveAbsenceRequest
```csharp
public class CreateLeaveAbsenceRequest
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

**Response DTO:** CreateLeaveAbsenceResponse
```csharp
public class CreateLeaveAbsenceResponse
{
    public string Status { get; set; }
    public string Message { get; set; }
    public long? PersonAbsenceEntryId { get; set; }
    public string Success { get; set; }
}
```

### System Layer Functions

#### Function 1: CreateOracleFusionLeaveAbsence

**Function Name:** CreateOracleFusionLeaveAbsence  
**HTTP Method:** POST  
**Route:** /api/oracle-fusion/absences  
**Purpose:** Create absence entry in Oracle Fusion HCM  

**Expose as Azure Function:** ✅ YES

**Reasoning:**
1. **System API:** Encapsulates Oracle Fusion HCM REST API call
2. **Reusability:** Can be used by other processes needing to create absences in Oracle Fusion
3. **Insulation:** Insulates consumers from Oracle Fusion API changes
4. **Error Handling:** Handles HTTP errors, gzip decompression, and provides structured responses

**Request DTO:** OracleFusionLeaveRequest
```csharp
public class OracleFusionLeaveRequest
{
    public int PersonNumber { get; set; }
    public string AbsenceType { get; set; }
    public string Employer { get; set; }
    public string StartDate { get; set; }
    public string EndDate { get; set; }
    public string AbsenceStatusCd { get; set; }
    public string ApprovalStatusCd { get; set; }
    public int StartDateDuration { get; set; }
    public int EndDateDuration { get; set; }
}
```

**Response DTO:** OracleFusionLeaveResponse
```csharp
public class OracleFusionLeaveResponse
{
    public long PersonAbsenceEntryId { get; set; }
    public string AbsenceStatusCd { get; set; }
    public string ApprovalStatusCd { get; set; }
    public string StartDate { get; set; }
    public string EndDate { get; set; }
    public int StartDateDuration { get; set; }
    public int EndDateDuration { get; set; }
    public int PersonNumber { get; set; }
    public string AbsenceType { get; set; }
    public string Employer { get; set; }
    // Plus 60+ additional Oracle Fusion fields (optional)
}
```

#### Function 2: SendErrorNotificationEmail

**Function Name:** SendErrorNotificationEmail  
**HTTP Method:** POST  
**Route:** /api/notifications/email/error  
**Purpose:** Send error notification email via Office 365 SMTP  

**Expose as Azure Function:** ✅ YES

**Reasoning:**
1. **Reusability:** Common error notification pattern used across multiple processes
2. **Decoupling:** Separates email sending logic from business logic
3. **Configurability:** Supports attachments and configurable recipients
4. **Monitoring:** Centralized error notification mechanism

**Request DTO:** SendErrorNotificationRequest
```csharp
public class SendErrorNotificationRequest
{
    public string ProcessName { get; set; }
    public string Environment { get; set; }
    public string ExecutionId { get; set; }
    public string ErrorMessage { get; set; }
    public string ToEmail { get; set; }
    public string Subject { get; set; }
    public bool HasAttachment { get; set; }
    public string AttachmentContent { get; set; }
    public string AttachmentFileName { get; set; }
}
```

**Response DTO:** SendErrorNotificationResponse
```csharp
public class SendErrorNotificationResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }
}
```

### Summary

**Total Functions to Expose:** 3
- **Process Layer:** 1 (CreateLeaveAbsence)
- **System Layer:** 2 (CreateOracleFusionLeaveAbsence, SendErrorNotificationEmail)

**Rationale:**
- All functions represent reusable, well-defined business capabilities
- Clear separation between Process Layer (orchestration) and System Layer (system integration)
- Each function has well-defined contracts (request/response DTOs)
- Functions follow API-Led Architecture principles (System, Process, Experience layers)

---

## 19. PHASE 1 COMPLETION SUMMARY

### Extraction Status: ✅ COMPLETE

All mandatory extraction steps have been completed and documented:

1. ✅ **Operations Inventory** - Section 1
2. ✅ **Input Structure Analysis (Step 1a)** - Section 2
3. ✅ **Response Structure Analysis (Step 1b)** - Section 3
4. ✅ **Operation Response Analysis (Step 1c)** - Section 4
5. ✅ **Map Analysis (Step 1d)** - Section 5
6. ✅ **HTTP Status Codes and Return Paths (Step 1e)** - Section 6
7. ✅ **Process Properties Analysis (Steps 2-3)** - Section 7
8. ✅ **Data Dependency Graph (Step 4)** - Section 8
9. ✅ **Control Flow Graph (Step 5)** - Section 9
10. ✅ **Reverse Flow Mapping (Step 6)** - Section 9
11. ✅ **Decision Shape Analysis (Step 7)** - Section 10
12. ✅ **Subprocess Analysis (Step 7a)** - Section 12
13. ✅ **Branch Shape Analysis (Step 8)** - Section 11
14. ✅ **Execution Order (Step 9)** - Section 13
15. ✅ **Sequence Diagram (Step 10)** - Section 14
16. ✅ **System Layer Identification** - Section 15
17. ✅ **Critical Patterns Identified** - Section 16
18. ✅ **Validation Checklist** - Section 17
19. ✅ **Function Exposure Decision Table** - Section 18

### Key Findings

**Process Complexity:**
- **Main Process:** 21 shapes with 20 connections
- **Subprocess:** 13 shapes with 12 connections
- **Total Operations:** 4 (2 main process, 2 subprocess)
- **Decisions:** 3 (2 main process, 1 subprocess)
- **Branches:** 1 (error handling branch with sequential execution)
- **Try/Catch Blocks:** 2 (1 main process, 1 subprocess)

**Data Flow:**
- **Input:** D365 leave creation request (9 fields)
- **Transformation:** Map D365 request to Oracle Fusion format (field name changes)
- **HTTP Call:** POST to Oracle Fusion HCM absences API
- **Response Handling:** Success (extract personAbsenceEntryId) or Error (extract error message, handle gzip)
- **Output:** Structured response (status, message, personAbsenceEntryId, success)

**Error Handling:**
- **Try/Catch:** Wraps main business logic
- **HTTP Status Check:** Routes to success or error path based on HTTP 20x
- **Gzip Handling:** Decompresses gzip-encoded error responses
- **Email Notification:** Sends error details via Office 365 email (with/without attachment)
- **Error Response:** Returns structured error response to caller

**Critical Dependencies:**
- **dynamicdocument.URL:** Written by shape8, read by shape33 (HTTP operation)
- **process.DPP_ErrorMessage:** Written by shape19/shape39/shape46, read by subprocess and error maps
- **Branch shape20:** Sequential execution (Path 1 → Path 2) due to property dependency

### Ready for Phase 2: Code Generation

All prerequisites for code generation are complete:
- ✅ Input/Output contracts defined
- ✅ Data transformations documented
- ✅ Execution order verified
- ✅ Error handling patterns identified
- ✅ System Layer APIs identified
- ✅ Function exposure decisions made

**Next Steps:**
- Generate Process Layer Azure Function: CreateLeaveAbsence
- Generate System Layer Azure Functions: CreateOracleFusionLeaveAbsence, SendErrorNotificationEmail
- Implement DTOs, services, and HTTP clients
- Implement error handling and logging
- Configure Azure Function bindings and settings

---

**END OF PHASE 1 EXTRACTION DOCUMENT**
