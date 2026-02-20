# BOOMI EXTRACTION PHASE 1 - HCM Leave Create Process

**Process Name:** HCM_Leave Create  
**Process ID:** ca69f858-785f-4565-ba1f-b4edc6cca05b  
**Description:** This Process will sync the Leave data between D365 and Oracle HCM  
**Extraction Date:** 2025-02-19  
**Business Domain:** Human Resource (HCM)

---

## 1. Operations Inventory

### Main Process Operations

| Operation ID | Operation Name | Type | Sub-Type | Purpose |
|--------------|---------------|------|----------|---------|
| 8f709c2b-e63f-4d5f-9374-2932ed70415d | Create Leave Oracle Fusion OP | connector-action | wss | Web Service Server - Entry point for leave creation requests |
| 6e8920fd-af5a-430b-a1d9-9fde7ac29a12 | Leave Oracle Fusion Create | connector-action | http | HTTP POST to Oracle Fusion HCM to create leave absence |
| af07502a-fafd-4976-a691-45d51a33b549 | Email w Attachment | connector-action | mail | Send email with attachment (error notification) |
| 15a72a21-9b57-49a1-a8ed-d70367146644 | Email W/O Attachment | connector-action | mail | Send email without attachment (error notification) |

### Subprocess Operations

**Subprocess:** (Sub) Office 365 Email (a85945c5-3004-42b9-80b1-104f465cd1fb)

| Operation ID | Operation Name | Type | Sub-Type | Purpose |
|--------------|---------------|------|----------|---------|
| af07502a-fafd-4976-a691-45d51a33b549 | Email w Attachment | connector-action | mail | Send email with attachment |
| 15a72a21-9b57-49a1-a8ed-d70367146644 | Email W/O Attachment | connector-action | mail | Send email without attachment |

---

## 2. Input Structure Analysis (Step 1a)

### Entry Point Operation

**Operation ID:** 8f709c2b-e63f-4d5f-9374-2932ed70415d  
**Operation Name:** Create Leave Oracle Fusion OP  
**Operation Type:** Web Services Server (wss)  
**Input Type:** singlejson  
**Request Profile ID:** febfa3e1-f719-4ee8-ba57-cdae34137ab3  
**Request Profile Name:** D365 Leave Create JSON Profile

### Request Profile Structure

**Profile Type:** profile.json  
**Root Element:** Root  
**Structure Path:** Root/Object  
**Array Detection:** ❌ NO - Single object input  
**Array Cardinality:** N/A (single object)

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

### Document Processing Behavior

- **Behavior:** single_document
- **Description:** Boomi processes single document per execution
- **Execution Pattern:** single_execution
- **Session Management:** one_session_per_execution
- **Azure Function Requirement:** Accept single leave request object

### Request Field Mapping

| Boomi Field Path | Boomi Field Name | Data Type | Required | Azure DTO Property | Notes |
|------------------|------------------|-----------|----------|-------------------|-------|
| Root/Object/employeeNumber | employeeNumber | number | Yes | EmployeeNumber | Employee identifier |
| Root/Object/absenceType | absenceType | character | Yes | AbsenceType | Type of leave (e.g., Sick Leave) |
| Root/Object/employer | employer | character | Yes | Employer | Employer name |
| Root/Object/startDate | startDate | character | Yes | StartDate | Leave start date (YYYY-MM-DD) |
| Root/Object/endDate | endDate | character | Yes | EndDate | Leave end date (YYYY-MM-DD) |
| Root/Object/absenceStatusCode | absenceStatusCode | character | Yes | AbsenceStatusCode | Status code (e.g., SUBMITTED) |
| Root/Object/approvalStatusCode | approvalStatusCode | character | Yes | ApprovalStatusCode | Approval status (e.g., APPROVED) |
| Root/Object/startDateDuration | startDateDuration | number | Yes | StartDateDuration | Duration for start date (days) |
| Root/Object/endDateDuration | endDateDuration | number | Yes | EndDateDuration | Duration for end date (days) |

**Total Fields:** 9 fields (all flat structure, no nested objects)

---

## 3. Response Structure Analysis (Step 1b)

### Response Profile

**Response Profile ID:** febfa3e1-f719-4ee8-ba57-cdae34137ab3 (same as request - bidirectional)  
**Response Profile Name:** D365 Leave Create JSON Profile  
**Profile Type:** profile.json  
**Output Type:** singlejson

### Actual Response Profile Used

The process uses a different response profile for the final response:

**Response Profile ID:** f4ca3a70-114a-4601-bad8-44a3eb20e2c0  
**Response Profile Name:** Leave D365 Response  
**Profile Type:** profile.json

### Response Structure

**Root Element:** leaveResponse  
**Structure Path:** leaveResponse/Object

### Response Format

```json
{
  "status": "success",
  "message": "Data successfully sent to Oracle Fusion",
  "personAbsenceEntryId": 12345,
  "success": "true"
}
```

### Response Field Mapping

| Boomi Field Path | Boomi Field Name | Data Type | Azure DTO Property | Notes |
|------------------|------------------|-----------|-------------------|-------|
| leaveResponse/Object/status | status | character | Status | Response status (success/failure) |
| leaveResponse/Object/message | message | character | Message | Response message |
| leaveResponse/Object/personAbsenceEntryId | personAbsenceEntryId | number | PersonAbsenceEntryId | Oracle Fusion absence entry ID |
| leaveResponse/Object/success | success | character | Success | Success flag (true/false) |

**Total Response Fields:** 4 fields

---

## 4. Operation Response Analysis (Step 1c)

### Operation: Leave Oracle Fusion Create (6e8920fd-af5a-430b-a1d9-9fde7ac29a12)

**Operation Type:** HTTP POST  
**Response Profile:** NONE (responseProfileType: NONE)  
**Response Content Type:** application/json  
**Response Handling:** Raw JSON response from Oracle Fusion HCM

**Expected Response Structure (Oracle Fusion HCM):**

Based on the response profile (316175c7-0e45-4869-9ac6-5f9d69882a62 - Oracle Fusion Leave Response JSON Profile), the Oracle API returns a comprehensive absence entry object with 80+ fields including:

**Key Response Fields:**
- personAbsenceEntryId (number) - Primary identifier
- absenceCaseId (character)
- absenceStatusCd (character)
- approvalStatusCd (character)
- personNumber (character)
- absenceType (character)
- employer (character)
- startDate (character)
- endDate (character)
- startDateDuration (number)
- endDateDuration (number)
- duration (number)
- links (array) - HATEOAS links

**Extracted Fields:**

The process extracts the following field from the Oracle Fusion response:

| Field Name | Extracted By | Written To Property | Data Type |
|------------|--------------|---------------------|-----------|
| personAbsenceEntryId | shape34 (map) | N/A (mapped directly) | number |

**Data Consumers:**

The personAbsenceEntryId field is:
1. Extracted by map shape34 (e4fd3f59-edb5-43a1-aeae-143b600a064e)
2. Mapped to the final response (leaveResponse/Object/personAbsenceEntryId)
3. Returned to the caller in the success response

**Business Logic Implications:**

- Oracle Fusion Create operation (shape33) MUST execute successfully before response mapping (shape34)
- If HTTP status is 20* (success), extract personAbsenceEntryId and return success response
- If HTTP status is NOT 20*, return error response with error message

**Response Header Mapping:**

The operation captures the Content-Encoding header:
- Header: Content-Encoding
- Target Property: DDP_RespHeader (dynamicdocument.DDP_RespHeader)
- Used for: Detecting gzip compression in response

---

## 5. Map Analysis (Step 1d)

### Map Inventory

| Map ID | Map Name | From Profile | To Profile | Purpose |
|--------|----------|--------------|------------|---------|
| c426b4d6-2aff-450e-b43b-59956c4dbc96 | Leave Create Map | febfa3e1-f719-4ee8-ba57-cdae34137ab3 | a94fa205-c740-40a5-9fda-3d018611135a | Transform D365 request to Oracle Fusion format |
| e4fd3f59-edb5-43a1-aeae-143b600a064e | Oracle Fusion Leave Response Map | 316175c7-0e45-4869-9ac6-5f9d69882a62 | f4ca3a70-114a-4601-bad8-44a3eb20e2c0 | Map Oracle response to D365 response |
| f46b845a-7d75-41b5-b0ad-c41a6a8e9b12 | Leave Error Map | 23d7a2e9-5cb0-4e9c-9e4b-3154834bad0d | f4ca3a70-114a-4601-bad8-44a3eb20e2c0 | Map error messages to D365 response |

### Map 1: Leave Create Map (c426b4d6-2aff-450e-b43b-59956c4dbc96)

**Purpose:** Transform incoming D365 leave request to Oracle Fusion HCM format

**From Profile:** febfa3e1-f719-4ee8-ba57-cdae34137ab3 (D365 Leave Create JSON Profile)  
**To Profile:** a94fa205-c740-40a5-9fda-3d018611135a (HCM Leave Create JSON Profile)

**Field Mappings:**

| Source Field | Target Field | Transformation | Notes |
|--------------|--------------|----------------|-------|
| employeeNumber | personNumber | Direct mapping | Employee identifier renamed |
| absenceType | absenceType | Direct mapping | No change |
| employer | employer | Direct mapping | No change |
| startDate | startDate | Direct mapping | No change |
| endDate | endDate | Direct mapping | No change |
| absenceStatusCode | absenceStatusCd | Direct mapping | Field name shortened |
| approvalStatusCode | approvalStatusCd | Direct mapping | Field name shortened |
| startDateDuration | startDateDuration | Direct mapping | No change |
| endDateDuration | endDateDuration | Direct mapping | No change |

**Profile vs Map Field Name Comparison:**

| Profile Field Name (Source) | Profile Field Name (Target) | Map Field Name (Actual) | Discrepancy? |
|------------------------------|----------------------------|------------------------|--------------|
| employeeNumber | personNumber | personNumber | ✅ Match |
| absenceStatusCode | absenceStatusCd | absenceStatusCd | ✅ Match |
| approvalStatusCode | approvalStatusCd | approvalStatusCd | ✅ Match |

**No scripting functions or complex transformations in this map.**

### Map 2: Oracle Fusion Leave Response Map (e4fd3f59-edb5-43a1-aeae-143b600a064e)

**Purpose:** Map Oracle Fusion response to D365 response format

**From Profile:** 316175c7-0e45-4869-9ac6-5f9d69882a62 (Oracle Fusion Leave Response JSON Profile)  
**To Profile:** f4ca3a70-114a-4601-bad8-44a3eb20e2c0 (Leave D365 Response)

**Field Mappings:**

| Source Field | Target Field | Transformation | Notes |
|--------------|--------------|----------------|-------|
| personAbsenceEntryId | personAbsenceEntryId | Direct mapping | Oracle absence entry ID |

**Default Values (Static):**

| Target Field | Default Value | Purpose |
|--------------|---------------|---------|
| status | "success" | Success status indicator |
| message | "Data successfully sent to Oracle Fusion" | Success message |
| success | "true" | Success flag |

### Map 3: Leave Error Map (f46b845a-7d75-41b5-b0ad-c41a6a8e9b12)

**Purpose:** Map error messages to D365 response format

**From Profile:** 23d7a2e9-5cb0-4e9c-9e4b-3154834bad0d (Dummy FF Profile)  
**To Profile:** f4ca3a70-114a-4601-bad8-44a3eb20e2c0 (Leave D365 Response)

**Field Mappings:**

| Source Field | Source Type | Target Field | Transformation | Notes |
|--------------|-------------|--------------|----------------|-------|
| process.DPP_ErrorMessage | Process Property (function) | message | PropertyGet function | Retrieves error message from process property |

**Function Analysis:**

**Function 1: Get Dynamic Process Property**
- **Type:** PropertyGet
- **Input:** Property Name = "DPP_ErrorMessage"
- **Output:** Result (error message text)
- **Purpose:** Retrieve error message captured during exception handling

**Default Values (Static):**

| Target Field | Default Value | Purpose |
|--------------|---------------|---------|
| status | "failure" | Failure status indicator |
| success | "false" | Failure flag |

**CRITICAL RULE:** Map field names are AUTHORITATIVE. All field transformations are direct mappings with no complex scripting.

---

## 6. HTTP Status Codes and Return Path Responses (Step 1e)

### Return Path Inventory

The process has **4 return paths** with different HTTP status codes and response structures:

### Return Path 1: Success Response (shape35)

**Return Label:** "Success Response"  
**Return Shape ID:** shape35  
**HTTP Status Code:** 200  
**Decision Conditions Leading to Return:**
- Decision shape2: HTTP Status Code matches "20*" (wildcard) → TRUE path
- Oracle Fusion API returned successful response (2xx status)

**Populated Response Fields:**

| Field Name | Field Path | Source | Populated By |
|------------|-----------|--------|--------------|
| status | leaveResponse/Object/status | static (map default) | shape34 (Oracle Fusion Leave Response Map) |
| message | leaveResponse/Object/message | static (map default) | shape34 (Oracle Fusion Leave Response Map) |
| personAbsenceEntryId | leaveResponse/Object/personAbsenceEntryId | operation_response | shape33 (Leave Oracle Fusion Create) |
| success | leaveResponse/Object/success | static (map default) | shape34 (Oracle Fusion Leave Response Map) |

**Response JSON Example:**

```json
{
  "status": "success",
  "message": "Data successfully sent to Oracle Fusion",
  "personAbsenceEntryId": 300000123456789,
  "success": "true"
}
```

### Return Path 2: Error Response - Try/Catch Error (shape43)

**Return Label:** "Error Response"  
**Return Shape ID:** shape43  
**HTTP Status Code:** 400  
**Decision Conditions Leading to Return:**
- Try/Catch block (shape17) catches exception in default path
- Error occurs during map transformation (shape29) or HTTP call setup

**Populated Response Fields:**

| Field Name | Field Path | Source | Populated By |
|------------|-----------|--------|--------------|
| status | leaveResponse/Object/status | static (map default) | shape41 (Leave Error Map) |
| message | leaveResponse/Object/message | process_property | shape19 (ErrorMsg) writes process.DPP_ErrorMessage from meta.base.catcherrorsmessage |
| success | leaveResponse/Object/success | static (map default) | shape41 (Leave Error Map) |

**Response JSON Example:**

```json
{
  "status": "failure",
  "message": "Error during transformation: Invalid date format",
  "success": "false"
}
```

### Return Path 3: Error Response - HTTP Non-2xx with Gzip (shape48)

**Return Label:** "Error Response"  
**Return Shape ID:** shape48  
**HTTP Status Code:** 400  
**Decision Conditions Leading to Return:**
- Decision shape2: HTTP Status Code does NOT match "20*" → FALSE path
- Decision shape44: Content-Encoding header equals "gzip" → TRUE path
- Oracle Fusion returned error response with gzip compression

**Populated Response Fields:**

| Field Name | Field Path | Source | Populated By |
|------------|-----------|--------|--------------|
| status | leaveResponse/Object/status | static (map default) | shape47 (Leave Error Map) |
| message | leaveResponse/Object/message | process_property | shape46 (error msg) writes process.DPP_ErrorMessage from decompressed response |
| success | leaveResponse/Object/success | static (map default) | shape47 (Leave Error Map) |

**Response JSON Example:**

```json
{
  "status": "failure",
  "message": "Oracle Fusion Error: Person not found for employee number 9000604",
  "success": "false"
}
```

**Special Processing:**
- shape45: Custom Groovy script decompresses gzip response using GZIPInputStream
- Decompressed error message is captured in process.DPP_ErrorMessage

### Return Path 4: Error Response - HTTP Non-2xx without Gzip (shape36)

**Return Label:** "Error Response"  
**Return Shape ID:** shape36  
**HTTP Status Code:** 400  
**Decision Conditions Leading to Return:**
- Decision shape2: HTTP Status Code does NOT match "20*" → FALSE path
- Decision shape44: Content-Encoding header does NOT equal "gzip" → FALSE path
- Oracle Fusion returned error response without compression

**Populated Response Fields:**

| Field Name | Field Path | Source | Populated By |
|------------|-----------|--------|--------------|
| status | leaveResponse/Object/status | static (map default) | shape40 (Leave Error Map) |
| message | leaveResponse/Object/message | process_property | shape39 (error msg) writes process.DPP_ErrorMessage from meta.base.applicationstatusmessage |
| success | leaveResponse/Object/success | static (map default) | shape40 (Leave Error Map) |

**Response JSON Example:**

```json
{
  "status": "failure",
  "message": "Oracle Fusion Error: Invalid absence type code",
  "success": "false"
}
```

### Downstream Operations HTTP Status Codes

| Operation Name | Expected Success Codes | Error Codes | Error Handling |
|----------------|----------------------|-------------|----------------|
| Leave Oracle Fusion Create | 200, 201 | 400, 401, 404, 500 | Return error response with message |

**Error Code Mapping:**
- **400:** Bad Request - Invalid input data
- **401:** Unauthorized - Authentication failed
- **404:** Not Found - Employee or resource not found
- **500:** Internal Server Error - Oracle Fusion system error

---

## 7. Request/Response JSON Examples (Step 1e - Detailed)

### Process Layer Entry Point

**Endpoint:** Web Service Server (wss) - "leave" object  
**Method:** POST (implied by operationType: EXECUTE)  
**Content-Type:** application/json

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
  "status": "success",
  "message": "Data successfully sent to Oracle Fusion",
  "personAbsenceEntryId": 300000123456789,
  "success": "true"
}
```

**Error Response - Validation Error (HTTP 400):**

```json
{
  "status": "failure",
  "message": "Error during transformation: Invalid date format for startDate",
  "success": "false"
}
```

**Error Response - Oracle Fusion Error (HTTP 400):**

```json
{
  "status": "failure",
  "message": "Oracle Fusion Error: Person not found for employee number 9000604",
  "success": "false"
}
```

### Downstream System Layer Calls

#### Operation: Leave Oracle Fusion Create

**Endpoint:** Oracle Fusion HCM REST API  
**Base URL:** Configured in connection (aa1fcb29-d146-4425-9ea6-b9698090f60e)  
**Resource Path:** hcmRestApi/resources/11.13.18.05/absences (from process property)  
**Method:** POST  
**Content-Type:** application/json

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

**Response JSON (Success - HTTP 200):**

```json
{
  "personAbsenceEntryId": 300000123456789,
  "absenceCaseId": "ABS-2024-001",
  "absenceStatusCd": "SUBMITTED",
  "approvalStatusCd": "APPROVED",
  "absenceTypeId": 300000001234567,
  "personId": 300000987654321,
  "personNumber": "9000604",
  "absenceType": "Sick Leave",
  "employer": "Al Ghurair Investment LLC",
  "startDate": "2024-03-24",
  "endDate": "2024-03-25",
  "startDateDuration": 1,
  "endDateDuration": 1,
  "duration": 2,
  "createdBy": "BOOMI_INTEGRATION",
  "creationDate": "2024-03-24T10:30:00Z",
  "lastUpdateDate": "2024-03-24T10:30:00Z",
  "links": [
    {
      "rel": "self",
      "href": "https://fusion.oracle.com/hcmRestApi/resources/11.13.18.05/absences/300000123456789",
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
  "detail": "Person not found for employee number 9000604",
  "o:errorCode": "PERSON_NOT_FOUND"
}
```

**Response JSON (Error - HTTP 401):**

```json
{
  "title": "Unauthorized",
  "status": 401,
  "detail": "Authentication credentials were missing or incorrect"
}
```

---

## 8. Process Properties Analysis (Steps 2-3)

### Process Properties WRITTEN

| Property ID | Property Name | Written By Shape | Source | Purpose |
|-------------|---------------|------------------|--------|---------|
| process.DPP_Process_Name | Dynamic Process Property - DPP_Process_Name | shape38 | Execution Property (Process Name) | Captures process name for logging |
| process.DPP_AtomName | Dynamic Process Property - DPP_AtomName | shape38 | Execution Property (Atom Name) | Captures atom name for logging |
| process.DPP_Payload | Dynamic Process Property - DPP_Payload | shape38 | Current document | Captures input payload for logging |
| process.DPP_ExecutionID | Dynamic Process Property - DPP_ExecutionID | shape38 | Execution Property (Execution Id) | Captures execution ID for logging |
| process.DPP_File_Name | Dynamic Process Property - DPP_File_Name | shape38 | Concatenation (Process Name + Timestamp + ".txt") | Generates filename for error attachment |
| process.DPP_Subject | Dynamic Process Property - DPP_Subject | shape38 | Concatenation (Atom Name + " (" + Process Name + ") has errors to report") | Generates email subject |
| process.To_Email | Dynamic Process Property - To_Email | shape38 | Defined Process Property (PP_HCM_LeaveCreate_Properties.To_Email) | Email recipient address |
| process.DPP_HasAttachment | Dynamic Process Property - DPP_HasAttachment | shape38 | Defined Process Property (PP_HCM_LeaveCreate_Properties.DPP_HasAttachment) | Flag for email attachment |
| dynamicdocument.URL | Dynamic Document Property - URL | shape8 | Defined Process Property (PP_HCM_LeaveCreate_Properties.Resource_Path) | Oracle Fusion API resource path |
| process.DPP_ErrorMessage | Dynamic Process Property - DPP_ErrorMessage | shape19 | Track Property (meta.base.catcherrorsmessage) | Error message from try/catch |
| process.DPP_ErrorMessage | Dynamic Process Property - DPP_ErrorMessage | shape39 | Track Property (meta.base.applicationstatusmessage) | Error message from HTTP response |
| process.DPP_ErrorMessage | Dynamic Process Property - DPP_ErrorMessage | shape46 | Current document (after gzip decompression) | Error message from decompressed HTTP response |

### Process Properties READ

| Property ID | Property Name | Read By Shape | Usage | Purpose |
|-------------|---------------|---------------|-------|---------|
| process.DPP_Payload | Dynamic Process Property - DPP_Payload | Subprocess shape21 (via shape15) | Message parameter | Pass payload to email subprocess |
| process.DPP_Process_Name | Dynamic Process Property - DPP_Process_Name | Subprocess shape21 (via shape11, shape23) | Message parameter | Pass process name to email body |
| process.DPP_AtomName | Dynamic Process Property - DPP_AtomName | Subprocess shape21 (via shape11, shape23) | Message parameter | Pass atom name to email body |
| process.DPP_ExecutionID | Dynamic Process Property - DPP_ExecutionID | Subprocess shape21 (via shape11, shape23) | Message parameter | Pass execution ID to email body |
| process.DPP_ErrorMessage | Dynamic Process Property - DPP_ErrorMessage | Subprocess shape21 (via shape11, shape23) | Message parameter | Pass error message to email body |
| process.DPP_Subject | Dynamic Process Property - DPP_Subject | Subprocess shape21 (via shape6, shape20) | Connector property (connector.mail.subject) | Email subject line |
| process.To_Email | Dynamic Process Property - To_Email | Subprocess shape21 (via shape6, shape20) | Connector property (connector.mail.toAddress) | Email recipient |
| process.DPP_MailBody | Dynamic Process Property - DPP_MailBody | Subprocess shape21 (via shape6, shape20) | Connector property (connector.mail.body) | Email body content |
| process.DPP_File_Name | Dynamic Process Property - DPP_File_Name | Subprocess shape21 (via shape6) | Connector property (connector.mail.filename) | Email attachment filename |
| process.DPP_HasAttachment | Dynamic Process Property - DPP_HasAttachment | Subprocess shape21 (via shape4) | Decision comparison | Determine if email has attachment |
| process.DPP_ErrorMessage | Dynamic Process Property - DPP_ErrorMessage | shape41, shape40, shape47 (via map function) | Map function input | Error message for response mapping |

### Defined Process Properties

**Component:** PP_HCM_LeaveCreate_Properties (e22c04db-7dfa-4b1b-9d88-77365b0fdb18)

| Property Key | Property Label | Type | Default Value | Purpose |
|--------------|----------------|------|---------------|---------|
| c2d1a1e8-4bcb-435b-b1d2-bb10a209bcc0 | Resource_Path | string | hcmRestApi/resources/11.13.18.05/absences | Oracle Fusion API resource path |
| 71c90f6e-4f86-49f3-8aef-bae6668c73f9 | To_Email | string | BoomiIntegrationTeam@al-ghurair.com | Error notification email recipient |
| a717867b-67f4-4a72-be2d-c99c73309fdb | DPP_HasAttachment | string | Y | Flag for email attachment (Y/N) |

**Component:** PP_Office365_Email (0feff13f-8a2c-438a-b3c7-1909e2a7f533)

| Property Key | Property Label | Type | Default Value | Purpose |
|--------------|----------------|------|---------------|---------|
| 804b6d40-eeee-4ac0-ab4b-94e1aca9f4b7 | From_Email | string | Boomi.Dev.failures@al-ghurair.com | Email sender address |
| d8fc7496-ec2c-4308-a4ca-e9f49c0c9500 | Environment | string | DEV Failure : | Environment prefix for email subject |

---

## 9. Data Dependency Graph (Step 4)

### Property Dependency Chains

**Chain 1: Input Logging Properties**

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
    ↓
Subprocess shape21 (via shape11, shape15, shape6, shape4, shape20) READS these properties
```

**Dependency:** shape38 MUST execute BEFORE subprocess shape21

**Chain 2: URL Configuration**

```
shape8 (set URL) WRITES:
  - dynamicdocument.URL
    ↓
shape33 (Leave Oracle Fusion Create) READS dynamicdocument.URL (implicitly via HTTP operation)
```

**Dependency:** shape8 MUST execute BEFORE shape33

**Chain 3: Error Message Handling**

```
shape19 (ErrorMsg) WRITES:
  - process.DPP_ErrorMessage (from meta.base.catcherrorsmessage)
    ↓
shape21 (subprocess) READS process.DPP_ErrorMessage
    ↓
shape41 (Leave Error Map) READS process.DPP_ErrorMessage
```

**Dependency:** shape19 MUST execute BEFORE shape21 AND shape41

**Chain 4: HTTP Error Message Handling (Non-Gzip)**

```
shape39 (error msg) WRITES:
  - process.DPP_ErrorMessage (from meta.base.applicationstatusmessage)
    ↓
shape40 (Leave Error Map) READS process.DPP_ErrorMessage
```

**Dependency:** shape39 MUST execute BEFORE shape40

**Chain 5: HTTP Error Message Handling (Gzip)**

```
shape46 (error msg) WRITES:
  - process.DPP_ErrorMessage (from current document after decompression)
    ↓
shape47 (Leave Error Map) READS process.DPP_ErrorMessage
```

**Dependency:** shape46 MUST execute BEFORE shape47

### Independent Operations

The following operations have NO data dependencies on other operations:
- shape38 (Input_details) - Reads from execution context only
- shape29 (Leave Create Map) - Reads from input document only
- shape33 (Leave Oracle Fusion Create) - Depends only on shape8 for URL

### Summary of Dependencies

| Writer Shape | Property Written | Reader Shapes | Dependency |
|--------------|------------------|---------------|------------|
| shape38 | process.DPP_Process_Name | shape11, shape23 (subprocess) | shape38 → subprocess |
| shape38 | process.DPP_AtomName | shape11, shape23 (subprocess) | shape38 → subprocess |
| shape38 | process.DPP_Payload | shape15 (subprocess) | shape38 → subprocess |
| shape38 | process.DPP_ExecutionID | shape11, shape23 (subprocess) | shape38 → subprocess |
| shape38 | process.DPP_File_Name | shape6 (subprocess) | shape38 → subprocess |
| shape38 | process.DPP_Subject | shape6, shape20 (subprocess) | shape38 → subprocess |
| shape38 | process.To_Email | shape6, shape20 (subprocess) | shape38 → subprocess |
| shape38 | process.DPP_HasAttachment | shape4 (subprocess) | shape38 → subprocess |
| shape8 | dynamicdocument.URL | shape33 | shape8 → shape33 |
| shape19 | process.DPP_ErrorMessage | shape21, shape41 | shape19 → shape21, shape41 |
| shape39 | process.DPP_ErrorMessage | shape40 | shape39 → shape40 |
| shape46 | process.DPP_ErrorMessage | shape47 | shape46 → shape47 |

---

## 10. Control Flow Graph (Step 5)

### Main Process Control Flow

| From Shape | Shape Type | To Shape(s) | Identifier | Notes |
|------------|------------|-------------|------------|-------|
| shape1 (start) | start | shape38 | default | Entry point |
| shape38 (Input_details) | documentproperties | shape17 | default | Set logging properties |
| shape17 (catcherrors) | catcherrors | shape29 (Try), shape20 (Catch) | default, error | Try/Catch block |
| shape29 (map) | map | shape8 | default | Transform request |
| shape8 (set URL) | documentproperties | shape49 | default | Set API URL |
| shape49 (notify) | notify | shape33 | default | Log payload |
| shape33 (HTTP call) | connectoraction | shape2 | default | Call Oracle Fusion API |
| shape2 (decision) | decision | shape34 (True), shape44 (False) | true, false | Check HTTP status 20* |
| shape34 (map) | map | shape35 | default | Map success response |
| shape35 (return) | returndocuments | - | - | Return success [HTTP 200] |
| shape44 (decision) | decision | shape45 (True), shape39 (False) | true, false | Check gzip encoding |
| shape45 (decompress) | dataprocess | shape46 | default | Decompress gzip response |
| shape46 (error msg) | documentproperties | shape47 | default | Extract error message |
| shape47 (map) | map | shape48 | default | Map error response |
| shape48 (return) | returndocuments | - | - | Return error [HTTP 400] |
| shape39 (error msg) | documentproperties | shape40 | default | Extract error message |
| shape40 (map) | map | shape36 | default | Map error response |
| shape36 (return) | returndocuments | - | - | Return error [HTTP 400] |
| shape20 (branch) | branch | shape19 (Path 1), shape41 (Path 2) | 1, 2 | Error handling branch |
| shape19 (ErrorMsg) | documentproperties | shape21 | default | Extract catch error |
| shape21 (subprocess) | processcall | - | - | Send error email |
| shape41 (map) | map | shape43 | default | Map error response |
| shape43 (return) | returndocuments | - | - | Return error [HTTP 400] |

### Subprocess Control Flow (Office 365 Email)

| From Shape | Shape Type | To Shape(s) | Identifier | Notes |
|------------|------------|-------------|------------|-------|
| shape1 (start) | start | shape2 | default | Subprocess entry |
| shape2 (catcherrors) | catcherrors | shape4 (Try), shape10 (Catch) | default, error | Try/Catch block |
| shape4 (decision) | decision | shape11 (True), shape23 (False) | true, false | Check attachment flag |
| shape11 (message) | message | shape14 | default | Build email body (with attachment) |
| shape14 (set_MailBody) | documentproperties | shape15 | default | Store email body |
| shape15 (payload) | message | shape6 | default | Attach payload |
| shape6 (set_Mail_Properties) | documentproperties | shape3 | default | Set email properties |
| shape3 (Email) | connectoraction | shape5 | default | Send email with attachment |
| shape5 (stop) | stop | - | continue=true | Subprocess success return |
| shape23 (message) | message | shape22 | default | Build email body (no attachment) |
| shape22 (set_MailBody) | documentproperties | shape20 | default | Store email body |
| shape20 (set_Mail_Properties) | documentproperties | shape7 | default | Set email properties |
| shape7 (Email) | connectoraction | shape9 | default | Send email without attachment |
| shape9 (stop) | stop | - | continue=true | Subprocess success return |
| shape10 (exception) | exception | - | - | Throw exception on email failure |

### Convergence Points

**Main Process:**
- No convergence points (all paths lead to return documents)

**Subprocess:**
- No convergence points (all paths lead to stop or exception)

### Total Connections

**Main Process:**
- Total Shapes: 19
- Total Connections: 20
- Branches: 1 (shape20 - 2 paths)
- Decisions: 2 (shape2, shape44)
- Return Documents: 4 (shape35, shape36, shape43, shape48)

**Subprocess:**
- Total Shapes: 13
- Total Connections: 11
- Branches: 0
- Decisions: 1 (shape4)
- Stop Shapes: 2 (shape5, shape9)
- Exception Shapes: 1 (shape10)

---

## 11. Decision Shape Analysis (Step 7)

### ✅ Self-Check Results

- ✅ **Decision data sources identified:** YES
- ✅ **Decision types classified:** YES
- ✅ **Execution order verified:** YES
- ✅ **All decision paths traced:** YES
- ✅ **Decision patterns identified:** YES
- ✅ **Paths traced to termination:** YES

### Decision 1: HTTP Status 20* Check (shape2)

**Shape ID:** shape2  
**User Label:** "HTTP Status 20 check"  
**Comparison Type:** wildcard  
**Location:** Main process, after HTTP call (shape33)

**Decision Values:**
- **Value 1:** Track Property - meta.base.applicationstatuscode (HTTP response status code)
- **Value 2:** Static - "20*" (wildcard pattern for 2xx success codes)

**Data Source Analysis:**
- **Data Source:** TRACK_PROPERTY (meta.base.applicationstatuscode)
- **Data Source Type:** RESPONSE (from HTTP operation shape33)
- **Decision Type:** POST_OPERATION
- **Reasoning:** This decision checks the HTTP status code from the Oracle Fusion API response, which is set by the HTTP operation (shape33). Therefore, it is a POST-OPERATION decision.

**Actual Execution Order:**
- Operation shape33 (Leave Oracle Fusion Create) → HTTP Response → Decision shape2 → Route based on status code

**Decision Paths:**

**TRUE Path (HTTP 20*):**
- **Destination:** shape34 (Oracle Fusion Leave Response Map)
- **Termination:** shape35 (Return Documents - Success Response)
- **Termination Type:** return
- **HTTP Status:** 200
- **Path Flow:** shape34 (map) → shape35 (return)

**FALSE Path (HTTP NOT 20*):**
- **Destination:** shape44 (Check Response Content Type)
- **Termination:** Multiple returns (shape36, shape48)
- **Termination Type:** return (after nested decision)
- **HTTP Status:** 400
- **Path Flow:** shape44 (decision) → [shape45 → shape46 → shape47 → shape48] OR [shape39 → shape40 → shape36]

**Pattern Type:** Error Check (Success vs Failure)

**Convergence:** No convergence - paths lead to different return documents

**Early Exit:** No - both paths eventually return, but through different routes

**Business Logic:**
- If Oracle Fusion returns 2xx status (success), extract personAbsenceEntryId and return success response
- If Oracle Fusion returns non-2xx status (error), check if response is gzip compressed and return error response

### Decision 2: Check Response Content Type (shape44)

**Shape ID:** shape44  
**User Label:** "Check Response Content Type"  
**Comparison Type:** equals  
**Location:** Main process, in FALSE path of shape2

**Decision Values:**
- **Value 1:** Track Property - dynamicdocument.DDP_RespHeader (Content-Encoding header)
- **Value 2:** Static - "gzip"

**Data Source Analysis:**
- **Data Source:** TRACK_PROPERTY (dynamicdocument.DDP_RespHeader)
- **Data Source Type:** RESPONSE (from HTTP operation shape33 - response header mapping)
- **Decision Type:** POST_OPERATION
- **Reasoning:** This decision checks the Content-Encoding response header captured by the HTTP operation (shape33). It determines if the error response is gzip compressed.

**Actual Execution Order:**
- Operation shape33 → HTTP Response → Decision shape2 (FALSE) → Decision shape44 → Route based on compression

**Decision Paths:**

**TRUE Path (gzip):**
- **Destination:** shape45 (Custom Scripting - decompress)
- **Termination:** shape48 (Return Documents - Error Response)
- **Termination Type:** return
- **HTTP Status:** 400
- **Path Flow:** shape45 (decompress) → shape46 (error msg) → shape47 (map) → shape48 (return)

**FALSE Path (NOT gzip):**
- **Destination:** shape39 (error msg)
- **Termination:** shape36 (Return Documents - Error Response)
- **Termination Type:** return
- **HTTP Status:** 400
- **Path Flow:** shape39 (error msg) → shape40 (map) → shape36 (return)

**Pattern Type:** Conditional Logic (Optional Processing)

**Convergence:** No convergence - paths lead to different return documents (though both are error responses)

**Early Exit:** No - both paths return error responses

**Business Logic:**
- If Oracle Fusion error response is gzip compressed, decompress it first before extracting error message
- If Oracle Fusion error response is not compressed, extract error message directly
- Both paths result in error response to caller

### Decision 3: Attachment Check (shape4 - Subprocess)

**Shape ID:** shape4  
**User Label:** "Attachment_Check"  
**Comparison Type:** equals  
**Location:** Subprocess (Office 365 Email), after try block

**Decision Values:**
- **Value 1:** Process Property - process.DPP_HasAttachment
- **Value 2:** Static - "Y"

**Data Source Analysis:**
- **Data Source:** PROCESS_PROPERTY (process.DPP_HasAttachment)
- **Data Source Type:** INPUT (set by main process shape38 from defined process property)
- **Decision Type:** PRE_FILTER
- **Reasoning:** This decision checks a process property that was set at the beginning of the main process (shape38). It determines which email operation to use based on whether an attachment is needed.

**Actual Execution Order:**
- Main process shape38 writes process.DPP_HasAttachment → Subprocess shape4 reads property → Route to appropriate email operation

**Decision Paths:**

**TRUE Path (Has Attachment = "Y"):**
- **Destination:** shape11 (Mail_Body message)
- **Termination:** shape5 (Stop - continue=true)
- **Termination Type:** stop (success return)
- **Path Flow:** shape11 (message) → shape14 (set_MailBody) → shape15 (payload) → shape6 (set_Mail_Properties) → shape3 (Email w Attachment) → shape5 (stop)

**FALSE Path (Has Attachment != "Y"):**
- **Destination:** shape23 (Mail_Body message)
- **Termination:** shape9 (Stop - continue=true)
- **Termination Type:** stop (success return)
- **Path Flow:** shape23 (message) → shape22 (set_MailBody) → shape20 (set_Mail_Properties) → shape7 (Email W/O Attachment) → shape9 (stop)

**Pattern Type:** Conditional Logic (Optional Processing)

**Convergence:** No convergence - paths lead to different stop shapes (though both return successfully)

**Early Exit:** No - both paths complete successfully

**Business Logic:**
- If attachment is required (Y), use Email w Attachment operation (shape3) with payload as attachment
- If attachment is not required (N), use Email W/O Attachment operation (shape7) without payload attachment
- Both paths send error notification email to configured recipients

### Decision Summary

| Decision | Type | Data Source | Pattern | Early Exit |
|----------|------|-------------|---------|------------|
| shape2 (HTTP Status 20* check) | POST_OPERATION | TRACK_PROPERTY (HTTP status) | Error Check | No |
| shape44 (Check Response Content Type) | POST_OPERATION | TRACK_PROPERTY (HTTP header) | Conditional Logic | No |
| shape4 (Attachment_Check) | PRE_FILTER | PROCESS_PROPERTY (input) | Conditional Logic | No |

---

## 12. Branch Shape Analysis (Step 8)

### ✅ Self-Check Results

- ✅ **Classification completed:** YES
- ✅ **Assumption check:** NO (analyzed dependencies)
- ✅ **Properties extracted:** YES
- ✅ **Dependency graph built:** YES
- ✅ **Topological sort applied:** YES (for sequential branches)

### Branch 1: Error Handling Branch (shape20)

**Shape ID:** shape20  
**User Label:** (none)  
**Location:** Main process, in Catch path of Try/Catch block (shape17)  
**Number of Paths:** 2

**Branch Paths:**

**Path 1 (identifier: "1"):**
- **Destination:** shape19 (ErrorMsg)
- **Path Flow:** shape19 → shape21 (subprocess)
- **Terminal:** Subprocess (no explicit return in main process)

**Path 2 (identifier: "2"):**
- **Destination:** shape41 (Leave Error Map)
- **Path Flow:** shape41 → shape43 (Return Documents)
- **Terminal:** shape43 (Return Documents - Error Response)

### Step 1: Properties Analysis

**Path 1 Properties:**

**READS:**
- None (shape19 reads from track property meta.base.catcherrorsmessage, not process property)

**WRITES:**
- process.DPP_ErrorMessage (by shape19)

**Path 2 Properties:**

**READS:**
- process.DPP_ErrorMessage (by shape41 via map function)

**WRITES:**
- None

### Step 2: Dependency Graph

```
Path 1 (shape19 → shape21) WRITES process.DPP_ErrorMessage
    ↓
Path 2 (shape41 → shape43) READS process.DPP_ErrorMessage
```

**Dependency:** Path 2 depends on Path 1 because it reads process.DPP_ErrorMessage which Path 1 writes.

### Step 3: Classification

**Classification:** SEQUENTIAL

**Reasoning:** Path 2 reads process.DPP_ErrorMessage which Path 1 writes. Therefore, Path 1 MUST execute BEFORE Path 2.

**API Call Detection:** Path 1 contains subprocess call (shape21) which may include email operations (API calls). Even without data dependency, this would make the branch sequential.

### Step 4: Topological Sort Order

**Execution Order:** Path 1 → Path 2

**Topological Sort:**
1. Path 1 has no dependencies (incoming edges = 0) → Execute first
2. Path 2 depends on Path 1 → Execute after Path 1 completes

### Step 5: Path Termination

**Path 1 Terminal:** Subprocess shape21 (no explicit return in main process path)  
**Path 2 Terminal:** shape43 (Return Documents - Error Response)

### Step 6: Convergence Points

**Convergence:** No convergence point identified. Path 1 calls subprocess and doesn't explicitly return. Path 2 returns error response.

**Note:** This branch structure is unusual. Typically, branches converge at a later point. In this case, Path 1 calls a subprocess for error notification (email), while Path 2 prepares and returns the error response. Both paths execute sequentially, with Path 1 sending the error notification and Path 2 returning the error to the caller.

### Step 7: Execution Continuation

**Execution Continues From:** shape43 (Return Documents) - Path 2 terminal

**Note:** After Path 1 completes (subprocess returns), Path 2 executes and returns the error response, terminating the main process.

### Step 8: Complete Branch Analysis Documentation

**Branch Shape ID:** shape20  
**Number of Paths:** 2  
**Classification:** SEQUENTIAL  
**Dependency Order:** Path 1 → Path 2  
**Path Terminals:**
- Path 1: Subprocess (shape21)
- Path 2: Return Documents (shape43)
**Convergence Points:** None  
**Execution Continues From:** shape43 (Return Documents)

**Business Logic:**
- When a try/catch error occurs, the process:
  1. Extracts error message from catch block (Path 1 - shape19)
  2. Sends error notification email via subprocess (Path 1 - shape21)
  3. Maps error message to response format (Path 2 - shape41)
  4. Returns error response to caller (Path 2 - shape43)

---

## 13. Execution Order (Step 9)

### ✅ Self-Check Results

- ✅ **Business logic verified FIRST:** YES
- ✅ **Operation analysis complete:** YES
- ✅ **Business logic execution order identified:** YES
- ✅ **Data dependencies checked FIRST:** YES
- ✅ **Operation response analysis used:** YES (Step 1c)
- ✅ **Decision analysis used:** YES (Step 7)
- ✅ **Dependency graph used:** YES (Step 4)
- ✅ **Branch analysis used:** YES (Step 8)
- ✅ **Property dependency verification:** YES
- ✅ **Topological sort applied:** YES (for sequential branch)

### Business Logic Flow (Step 0 - MUST BE FIRST)

#### Operation 1: Create Leave Oracle Fusion OP (8f709c2b-e63f-4d5f-9374-2932ed70415d)

**Purpose:** Web Service Server entry point - Receives leave creation requests from D365

**Outputs:**
- Input document (leave request JSON)
- No response data (entry point operation)

**Dependent Operations:** All subsequent operations depend on this entry point

**Business Flow:**
- This is the entry point for the integration
- Receives leave request from D365
- Triggers the entire leave creation process

#### Operation 2: Leave Oracle Fusion Create (6e8920fd-af5a-430b-a1d9-9fde7ac29a12)

**Purpose:** HTTP POST to Oracle Fusion HCM - Creates absence entry in Oracle Fusion

**Outputs:**
- HTTP response with personAbsenceEntryId (on success)
- HTTP status code (meta.base.applicationstatuscode)
- HTTP response message (meta.base.applicationstatusmessage)
- Response header Content-Encoding (dynamicdocument.DDP_RespHeader)

**Dependent Operations:**
- Decision shape2 (checks HTTP status code)
- Decision shape44 (checks Content-Encoding header)
- Map shape34 (maps success response)
- Map shape40, shape47 (map error responses)

**Business Flow:**
- This operation MUST execute FIRST among downstream operations
- Produces HTTP status code that determines success/error path
- Produces personAbsenceEntryId that is returned to caller on success
- Produces error messages that are returned to caller on failure

#### Operation 3: Email w Attachment (af07502a-fafd-4976-a691-45d51a33b549)

**Purpose:** Send error notification email with payload attachment

**Outputs:**
- Email sent (no data returned to main process)

**Dependent Operations:** None (terminal operation in subprocess)

**Business Flow:**
- Executes only when error occurs AND attachment flag is "Y"
- Sends error notification to configured email recipients
- Includes error details and original payload as attachment

#### Operation 4: Email W/O Attachment (15a72a21-9b57-49a1-a8ed-d70367146644)

**Purpose:** Send error notification email without attachment

**Outputs:**
- Email sent (no data returned to main process)

**Dependent Operations:** None (terminal operation in subprocess)

**Business Flow:**
- Executes only when error occurs AND attachment flag is "N"
- Sends error notification to configured email recipients
- Includes error details in email body only

### Execution Order Analysis

Based on the business logic, data dependencies, decision analysis, and branch analysis, the execution order is:

#### Main Success Path (HTTP 20*)

```
START (shape1)
  ↓
shape38 (Input_details) - WRITES: process.DPP_Process_Name, process.DPP_AtomName, process.DPP_Payload, 
                                   process.DPP_ExecutionID, process.DPP_File_Name, process.DPP_Subject,
                                   process.To_Email, process.DPP_HasAttachment
  ↓
shape17 (Try/Catch) - TRY PATH
  ↓
shape29 (Leave Create Map) - Transform D365 request to Oracle Fusion format
  ↓
shape8 (set URL) - WRITES: dynamicdocument.URL
  ↓
shape49 (notify) - Log payload
  ↓
shape33 (Leave Oracle Fusion Create) - READS: dynamicdocument.URL
                                      - PRODUCES: HTTP status 20*, personAbsenceEntryId
  ↓
shape2 (Decision: HTTP Status 20* check) - READS: meta.base.applicationstatuscode
  ↓ TRUE (HTTP 20*)
shape34 (Oracle Fusion Leave Response Map) - Extract personAbsenceEntryId, set success status
  ↓
shape35 (Return Documents) - SUCCESS RESPONSE [HTTP 200]
```

**Dependency Verification:**
- shape38 → subprocess (writes properties read by subprocess)
- shape8 → shape33 (writes URL read by HTTP operation)
- shape33 → shape2 (produces status code checked by decision)
- shape33 → shape34 (produces personAbsenceEntryId mapped to response)

#### Error Path 1: Try/Catch Error (Exception during transformation or setup)

```
START (shape1)
  ↓
shape38 (Input_details) - WRITES: process properties
  ↓
shape17 (Try/Catch) - TRY PATH
  ↓
shape29 (Leave Create Map) - ERROR OCCURS
  ↓
shape17 (Try/Catch) - CATCH PATH
  ↓
shape20 (Branch) - SEQUENTIAL EXECUTION
  ↓
Path 1: shape19 (ErrorMsg) - WRITES: process.DPP_ErrorMessage (from meta.base.catcherrorsmessage)
        ↓
        shape21 (Subprocess: Office 365 Email) - READS: process properties
          ↓ (Subprocess Internal Flow)
          shape1 (start) → shape2 (Try/Catch) → shape4 (Decision: Attachment_Check)
            ↓ TRUE (Has Attachment = "Y")
            shape11 (Mail_Body) → shape14 (set_MailBody) → shape15 (payload) 
            → shape6 (set_Mail_Properties) → shape3 (Email w Attachment) → shape5 (stop)
  ↓
Path 2: shape41 (Leave Error Map) - READS: process.DPP_ErrorMessage
        ↓
        shape43 (Return Documents) - ERROR RESPONSE [HTTP 400]
```

**Dependency Verification:**
- shape38 → shape21 (subprocess reads properties written by shape38)
- shape19 → shape21 (subprocess reads error message written by shape19)
- shape19 → shape41 (map reads error message written by shape19)
- Path 1 → Path 2 (sequential branch execution)

#### Error Path 2: HTTP Non-2xx with Gzip Compression

```
START (shape1)
  ↓
shape38 (Input_details) - WRITES: process properties
  ↓
shape17 (Try/Catch) - TRY PATH
  ↓
shape29 (Leave Create Map) → shape8 (set URL) → shape49 (notify)
  ↓
shape33 (Leave Oracle Fusion Create) - PRODUCES: HTTP status 400, gzip compressed error response
  ↓
shape2 (Decision: HTTP Status 20* check) - READS: meta.base.applicationstatuscode
  ↓ FALSE (HTTP NOT 20*)
shape44 (Decision: Check Response Content Type) - READS: dynamicdocument.DDP_RespHeader
  ↓ TRUE (gzip)
shape45 (Custom Scripting) - Decompress gzip response using GZIPInputStream
  ↓
shape46 (error msg) - WRITES: process.DPP_ErrorMessage (from decompressed response)
  ↓
shape47 (Leave Error Map) - READS: process.DPP_ErrorMessage
  ↓
shape48 (Return Documents) - ERROR RESPONSE [HTTP 400]
```

**Dependency Verification:**
- shape33 → shape2 (produces status code checked by decision)
- shape33 → shape44 (produces Content-Encoding header checked by decision)
- shape45 → shape46 (decompresses response before extracting error message)
- shape46 → shape47 (writes error message read by map)

#### Error Path 3: HTTP Non-2xx without Gzip Compression

```
START (shape1)
  ↓
shape38 (Input_details) - WRITES: process properties
  ↓
shape17 (Try/Catch) - TRY PATH
  ↓
shape29 (Leave Create Map) → shape8 (set URL) → shape49 (notify)
  ↓
shape33 (Leave Oracle Fusion Create) - PRODUCES: HTTP status 400, non-compressed error response
  ↓
shape2 (Decision: HTTP Status 20* check) - READS: meta.base.applicationstatuscode
  ↓ FALSE (HTTP NOT 20*)
shape44 (Decision: Check Response Content Type) - READS: dynamicdocument.DDP_RespHeader
  ↓ FALSE (NOT gzip)
shape39 (error msg) - WRITES: process.DPP_ErrorMessage (from meta.base.applicationstatusmessage)
  ↓
shape40 (Leave Error Map) - READS: process.DPP_ErrorMessage
  ↓
shape36 (Return Documents) - ERROR RESPONSE [HTTP 400]
```

**Dependency Verification:**
- shape33 → shape2 (produces status code checked by decision)
- shape33 → shape44 (produces Content-Encoding header checked by decision)
- shape39 → shape40 (writes error message read by map)

### Execution Order Summary

**Critical Dependencies:**
1. shape38 MUST execute FIRST (writes all logging properties)
2. shape8 MUST execute BEFORE shape33 (writes URL for HTTP operation)
3. shape33 MUST execute BEFORE shape2 (produces status code for decision)
4. shape19 MUST execute BEFORE shape21 AND shape41 (writes error message)
5. Path 1 MUST execute BEFORE Path 2 in branch shape20 (sequential branch)

**Parallel Execution:** None - All operations execute sequentially

**Branch Execution:** shape20 (sequential) - Path 1 → Path 2

---

## 14. Sequence Diagram (Step 10)

**📋 NOTE:** Detailed request/response JSON examples are documented in:
- **Section 6: HTTP Status Codes and Return Path Responses** - For response JSON with populated fields for return paths
- **Section 7: Request/Response JSON Examples** - For detailed request/response JSON examples

### Main Success Path

```
START (shape1)
  |
  ├─→ shape38: Input_details (Document Properties)
  |   └─→ WRITES: process.DPP_Process_Name, process.DPP_AtomName, process.DPP_Payload,
  |               process.DPP_ExecutionID, process.DPP_File_Name, process.DPP_Subject,
  |               process.To_Email, process.DPP_HasAttachment
  |
  ├─→ shape17: Try/Catch Block
  |   |
  |   ├─→ TRY PATH:
  |   |   |
  |   |   ├─→ shape29: Leave Create Map
  |   |   |   └─→ Transform: D365 request → Oracle Fusion format
  |   |   |   └─→ Field mappings: employeeNumber→personNumber, absenceStatusCode→absenceStatusCd
  |   |   |
  |   |   ├─→ shape8: set URL (Document Properties)
  |   |   |   └─→ WRITES: dynamicdocument.URL
  |   |   |   └─→ VALUE: "hcmRestApi/resources/11.13.18.05/absences"
  |   |   |
  |   |   ├─→ shape49: notify (Notify)
  |   |   |   └─→ Log payload for debugging
  |   |   |
  |   |   ├─→ shape33: Leave Oracle Fusion Create (Downstream HTTP POST)
  |   |   |   └─→ READS: dynamicdocument.URL
  |   |   |   └─→ PRODUCES: HTTP status code, personAbsenceEntryId (on success)
  |   |   |   └─→ HTTP: [Expected: 200/201, Error: 400/401/404/500]
  |   |   |
  |   |   ├─→ shape2: Decision - HTTP Status 20* check
  |   |   |   └─→ READS: meta.base.applicationstatuscode
  |   |   |   └─→ COMPARISON: wildcard match "20*"
  |   |   |   |
  |   |   |   ├─→ IF TRUE (HTTP 20*):
  |   |   |   |   |
  |   |   |   |   ├─→ shape34: Oracle Fusion Leave Response Map
  |   |   |   |   |   └─→ Extract: personAbsenceEntryId from Oracle response
  |   |   |   |   |   └─→ Set defaults: status="success", message="Data successfully sent to Oracle Fusion", success="true"
  |   |   |   |   |
  |   |   |   |   └─→ shape35: Return Documents [HTTP 200] [SUCCESS]
  |   |   |   |       └─→ Response: { status, message, personAbsenceEntryId, success }
  |   |   |   |
  |   |   |   └─→ IF FALSE (HTTP NOT 20*):
  |   |   |       |
  |   |   |       ├─→ shape44: Decision - Check Response Content Type
  |   |   |           └─→ READS: dynamicdocument.DDP_RespHeader
  |   |   |           └─→ COMPARISON: equals "gzip"
  |   |   |           |
  |   |   |           ├─→ IF TRUE (gzip):
  |   |   |           |   |
  |   |   |           |   ├─→ shape45: Custom Scripting (Decompress)
  |   |   |           |   |   └─→ Groovy script: GZIPInputStream decompression
  |   |   |           |   |
  |   |   |           |   ├─→ shape46: error msg (Document Properties)
  |   |   |           |   |   └─→ WRITES: process.DPP_ErrorMessage
  |   |   |           |   |   └─→ SOURCE: Current document (decompressed error response)
  |   |   |           |   |
  |   |   |           |   ├─→ shape47: Leave Error Map
  |   |   |           |   |   └─→ READS: process.DPP_ErrorMessage
  |   |   |           |   |   └─→ Set defaults: status="failure", success="false"
  |   |   |           |   |
  |   |   |           |   └─→ shape48: Return Documents [HTTP 400] [ERROR]
  |   |   |           |       └─→ Response: { status, message, success }
  |   |   |           |
  |   |   |           └─→ IF FALSE (NOT gzip):
  |   |   |               |
  |   |   |               ├─→ shape39: error msg (Document Properties)
  |   |   |               |   └─→ WRITES: process.DPP_ErrorMessage
  |   |   |               |   └─→ SOURCE: meta.base.applicationstatusmessage
  |   |   |               |
  |   |   |               ├─→ shape40: Leave Error Map
  |   |   |               |   └─→ READS: process.DPP_ErrorMessage
  |   |   |               |   └─→ Set defaults: status="failure", success="false"
  |   |   |               |
  |   |   |               └─→ shape36: Return Documents [HTTP 400] [ERROR]
  |   |   |                   └─→ Response: { status, message, success }
  |   |
  |   └─→ CATCH PATH (Exception during transformation or setup):
  |       |
  |       ├─→ shape20: Branch (SEQUENTIAL - 2 paths)
  |           |
  |           ├─→ Path 1: Error Notification
  |           |   |
  |           |   ├─→ shape19: ErrorMsg (Document Properties)
  |           |   |   └─→ WRITES: process.DPP_ErrorMessage
  |           |   |   └─→ SOURCE: meta.base.catcherrorsmessage
  |           |   |
  |           |   └─→ shape21: ProcessCall - (Sub) Office 365 Email
  |           |       └─→ READS: process.DPP_Process_Name, process.DPP_AtomName, process.DPP_Payload,
  |           |                  process.DPP_ExecutionID, process.DPP_ErrorMessage, process.DPP_Subject,
  |           |                  process.To_Email, process.DPP_HasAttachment, process.DPP_File_Name
  |           |       |
  |           |       └─→ SUBPROCESS INTERNAL FLOW:
  |           |           |
  |           |           ├─→ shape1: START
  |           |           |
  |           |           ├─→ shape2: Try/Catch Block
  |           |           |   |
  |           |           |   ├─→ TRY PATH:
  |           |           |   |   |
  |           |           |   |   ├─→ shape4: Decision - Attachment_Check
  |           |           |   |   |   └─→ READS: process.DPP_HasAttachment
  |           |           |   |   |   └─→ COMPARISON: equals "Y"
  |           |           |   |   |   |
  |           |           |   |   |   ├─→ IF TRUE (Has Attachment):
  |           |           |   |   |   |   |
  |           |           |   |   |   |   ├─→ shape11: Mail_Body (Message)
  |           |           |   |   |   |   |   └─→ Build HTML email body with error details
  |           |           |   |   |   |   |   └─→ READS: process.DPP_Process_Name, process.DPP_AtomName,
  |           |           |   |   |   |   |            process.DPP_ExecutionID, process.DPP_ErrorMessage
  |           |           |   |   |   |   |
  |           |           |   |   |   |   ├─→ shape14: set_MailBody (Document Properties)
  |           |           |   |   |   |   |   └─→ WRITES: process.DPP_MailBody
  |           |           |   |   |   |   |
  |           |           |   |   |   |   ├─→ shape15: payload (Message)
  |           |           |   |   |   |   |   └─→ READS: process.DPP_Payload
  |           |           |   |   |   |   |   └─→ Set payload as email attachment
  |           |           |   |   |   |   |
  |           |           |   |   |   |   ├─→ shape6: set_Mail_Properties (Document Properties)
  |           |           |   |   |   |   |   └─→ READS: process.To_Email, process.DPP_Subject, process.DPP_MailBody,
  |           |           |   |   |   |   |            process.DPP_File_Name
  |           |           |   |   |   |   |   └─→ WRITES: connector.mail.fromAddress, connector.mail.toAddress,
  |           |           |   |   |   |   |              connector.mail.subject, connector.mail.body, connector.mail.filename
  |           |           |   |   |   |   |
  |           |           |   |   |   |   ├─→ shape3: Email w Attachment (Downstream MAIL)
  |           |           |   |   |   |   |   └─→ Send email with payload attachment
  |           |           |   |   |   |   |   └─→ HTTP: [Expected: 200, Error: 500]
  |           |           |   |   |   |   |
  |           |           |   |   |   |   └─→ shape5: Stop (continue=true) [SUCCESS RETURN]
  |           |           |   |   |   |
  |           |           |   |   |   └─→ IF FALSE (No Attachment):
  |           |           |   |   |       |
  |           |           |   |   |       ├─→ shape23: Mail_Body (Message)
  |           |           |   |   |       |   └─→ Build HTML email body with error details
  |           |           |   |   |       |   └─→ READS: process.DPP_Process_Name, process.DPP_AtomName,
  |           |           |   |   |       |            process.DPP_ExecutionID, process.DPP_ErrorMessage
  |           |           |   |   |       |
  |           |           |   |   |       ├─→ shape22: set_MailBody (Document Properties)
  |           |           |   |   |       |   └─→ WRITES: process.DPP_MailBody
  |           |           |   |   |       |
  |           |           |   |   |       ├─→ shape20: set_Mail_Properties (Document Properties)
  |           |           |   |   |       |   └─→ READS: process.To_Email, process.DPP_Subject, process.DPP_MailBody
  |           |           |   |   |       |   └─→ WRITES: connector.mail.fromAddress, connector.mail.toAddress,
  |           |           |   |   |       |              connector.mail.subject, connector.mail.body
  |           |           |   |   |       |
  |           |           |   |   |       ├─→ shape7: Email W/O Attachment (Downstream MAIL)
  |           |           |   |   |       |   └─→ Send email without attachment
  |           |           |   |   |       |   └─→ HTTP: [Expected: 200, Error: 500]
  |           |           |   |   |       |
  |           |           |   |   |       └─→ shape9: Stop (continue=true) [SUCCESS RETURN]
  |           |           |   |
  |           |           |   └─→ CATCH PATH:
  |           |           |       |
  |           |           |       └─→ shape10: Exception
  |           |           |           └─→ READS: meta.base.catcherrorsmessage
  |           |           |           └─→ Throw exception [SUBPROCESS FAILURE]
  |           |           |
  |           |           └─→ END SUBPROCESS
  |           |
  |           └─→ Path 2: Error Response
  |               |
  |               ├─→ shape41: Leave Error Map
  |               |   └─→ READS: process.DPP_ErrorMessage
  |               |   └─→ Set defaults: status="failure", success="false"
  |               |
  |               └─→ shape43: Return Documents [HTTP 400] [ERROR]
  |                   └─→ Response: { status, message, success }
```

**Note:** Detailed request/response JSON examples for all operations and return paths are documented in Section 6 (HTTP Status Codes and Return Path Responses) and Section 7 (Request/Response JSON Examples).

### Sequence Diagram References

This sequence diagram is based on:
- **Step 4 (Data Dependency Graph):** Property dependencies verified (shape38 → subprocess, shape8 → shape33, shape19 → shape21/shape41)
- **Step 5 (Control Flow Graph):** Dragpoint connections mapped from JSON
- **Step 7 (Decision Shape Analysis):** Decision paths traced (shape2, shape44, shape4)
- **Step 8 (Branch Shape Analysis):** Sequential branch execution (shape20: Path 1 → Path 2)
- **Step 9 (Execution Order):** Business logic verified, dependencies confirmed

---

## 15. Subprocess Analysis (Step 7a)

### Subprocess: (Sub) Office 365 Email

**Subprocess ID:** a85945c5-3004-42b9-80b1-104f465cd1fb  
**Subprocess Name:** (Sub) Office 365 Email  
**Purpose:** Send error notification emails with or without attachments

### Internal Flow

```
START (shape1)
  ↓
shape2 (Try/Catch)
  ↓ TRY PATH
shape4 (Decision: Attachment_Check)
  ├─→ TRUE (Has Attachment = "Y")
  |   ↓
  |   shape11 (Mail_Body) → shape14 (set_MailBody) → shape15 (payload) 
  |   → shape6 (set_Mail_Properties) → shape3 (Email w Attachment) → shape5 (Stop - SUCCESS)
  |
  └─→ FALSE (No Attachment)
      ↓
      shape23 (Mail_Body) → shape22 (set_MailBody) → shape20 (set_Mail_Properties) 
      → shape7 (Email W/O Attachment) → shape9 (Stop - SUCCESS)
  
CATCH PATH:
  shape10 (Exception) - FAILURE
```

### Return Paths

| Return Label | Shape ID | Path | Condition |
|--------------|----------|------|-----------|
| SUCCESS | shape5 | TRUE path (with attachment) | Email sent successfully with attachment |
| SUCCESS | shape9 | FALSE path (no attachment) | Email sent successfully without attachment |
| EXCEPTION | shape10 | CATCH path | Email send failure |

### Main Process Mapping

The subprocess does NOT use explicit return path labels. It uses:
- **Stop (continue=true):** shape5, shape9 - Indicates successful completion
- **Exception:** shape10 - Indicates failure

**Main Process Handling:**
- The main process calls the subprocess via shape21 (processcall)
- The subprocess returns control to the main process after completion
- No explicit return path mapping in main process (returnpaths: "")
- After subprocess returns, main process continues to Path 2 of branch shape20

### Properties Written by Subprocess

| Property ID | Property Name | Written By | Source |
|-------------|---------------|------------|--------|
| process.DPP_MailBody | Dynamic Process Property - DPP_MailBody | shape14, shape22 | Current document (HTML email body) |
| connector.mail.fromAddress | Mail - From Address | shape6, shape20 | Defined Process Property (PP_Office365_Email.From_Email) |
| connector.mail.toAddress | Mail - To Address | shape6, shape20 | Process Property (process.To_Email) |
| connector.mail.subject | Mail - Subject | shape6, shape20 | Concatenation (Environment + process.DPP_Subject) |
| connector.mail.body | Mail - Body | shape6, shape20 | Process Property (process.DPP_MailBody) |
| connector.mail.filename | Mail - File Name | shape6 | Process Property (process.DPP_File_Name) |

### Properties Read by Subprocess (from Main Process)

| Property ID | Property Name | Read By | Usage |
|-------------|---------------|---------|-------|
| process.DPP_HasAttachment | Dynamic Process Property - DPP_HasAttachment | shape4 | Decision comparison |
| process.DPP_Process_Name | Dynamic Process Property - DPP_Process_Name | shape11, shape23 | Message parameter |
| process.DPP_AtomName | Dynamic Process Property - DPP_AtomName | shape11, shape23 | Message parameter |
| process.DPP_ExecutionID | Dynamic Process Property - DPP_ExecutionID | shape11, shape23 | Message parameter |
| process.DPP_ErrorMessage | Dynamic Process Property - DPP_ErrorMessage | shape11, shape23 | Message parameter |
| process.DPP_Payload | Dynamic Process Property - DPP_Payload | shape15 | Message parameter (attachment) |
| process.To_Email | Dynamic Process Property - To_Email | shape6, shape20 | Mail property |
| process.DPP_Subject | Dynamic Process Property - DPP_Subject | shape6, shape20 | Mail property |
| process.DPP_File_Name | Dynamic Process Property - DPP_File_Name | shape6 | Mail property |

### Error Paths

**Error Handling:**
- If email send fails (shape3 or shape7), the try/catch block (shape2) catches the exception
- Exception shape (shape10) throws exception with error message
- Main process would need to handle subprocess exception (not explicitly shown in main process)

**Business Logic:**
- Subprocess is called only when an error occurs in main process
- Subprocess sends error notification email to configured recipients
- If attachment flag is "Y", includes original payload as attachment
- If attachment flag is "N", sends email body only
- Email body includes: Process Name, Environment, Execution ID, Error Details

---

## 16. Critical Patterns Identified

### Pattern 1: Try/Catch with Error Notification

**Identification:** Try/Catch block (shape17) with error handling branch (shape20) that sends email notification

**Components:**
- Try block: shape29 (map) → shape8 (set URL) → shape49 (notify) → shape33 (HTTP call)
- Catch block: shape20 (branch) → Path 1 (error notification) → Path 2 (error response)

**Execution Rule:**
- If any error occurs during transformation or HTTP call setup, catch block executes
- Error message is captured from meta.base.catcherrorsmessage
- Error notification email is sent via subprocess
- Error response is returned to caller

**Pattern Type:** Error handling with notification

### Pattern 2: HTTP Status Code Decision with Multiple Error Paths

**Identification:** Decision shape2 checks HTTP status code, with nested decision shape44 for error response handling

**Components:**
- Decision shape2: Check HTTP status 20* (success vs error)
- TRUE path: Map success response and return
- FALSE path: Decision shape44 checks gzip compression
  - TRUE path (gzip): Decompress → Extract error → Map error → Return
  - FALSE path (no gzip): Extract error → Map error → Return

**Execution Rule:**
- If Oracle Fusion returns 2xx status, extract success data and return
- If Oracle Fusion returns non-2xx status, check if response is compressed
- Decompress if needed, then extract error message and return error response

**Pattern Type:** Conditional error handling with optional processing (decompression)

### Pattern 3: Sequential Branch for Error Notification and Response

**Identification:** Branch shape20 with 2 sequential paths

**Components:**
- Path 1: Extract error message → Send email notification (subprocess)
- Path 2: Map error message to response → Return error response

**Execution Rule:**
- Path 1 MUST execute BEFORE Path 2 (data dependency)
- Path 1 sends error notification to configured recipients
- Path 2 returns error response to caller
- Both paths execute sequentially (not parallel)

**Pattern Type:** Sequential error handling with notification and response

### Pattern 4: Subprocess with Conditional Email Attachment

**Identification:** Subprocess shape21 with decision shape4 for attachment handling

**Components:**
- Decision shape4: Check attachment flag
- TRUE path: Build email with attachment → Send email with attachment
- FALSE path: Build email without attachment → Send email without attachment

**Execution Rule:**
- If attachment flag is "Y", include original payload as email attachment
- If attachment flag is "N", send email body only
- Both paths send error notification email

**Pattern Type:** Conditional subprocess execution

### Pattern 5: Property-Based Configuration

**Identification:** Multiple defined process properties for configuration

**Components:**
- PP_HCM_LeaveCreate_Properties: Resource_Path, To_Email, DPP_HasAttachment
- PP_Office365_Email: From_Email, Environment

**Execution Rule:**
- Configuration properties are read at runtime
- Properties control: API endpoint, email recipients, attachment flag, email sender, environment prefix
- Properties are environment-specific (can be overridden per environment)

**Pattern Type:** Configuration management

---

## 17. Validation Checklist

### Data Dependencies
- [x] All property WRITES identified (12 properties written)
- [x] All property READS identified (11 properties read)
- [x] Dependency graph built (5 dependency chains documented)
- [x] Execution order satisfies all dependencies (verified in Step 9)

### Decision Analysis
- [x] ALL decision shapes inventoried (3 decisions: shape2, shape44, shape4)
- [x] BOTH TRUE and FALSE paths traced to termination
- [x] Pattern type identified for each decision
- [x] Early exits identified and documented (none - all paths return)
- [x] Convergence points identified (none)

### Branch Analysis
- [x] Each branch classified as parallel or sequential (1 branch: shape20 - SEQUENTIAL)
- [x] API call detection performed (subprocess contains email operations)
- [x] Dependency order built using topological sort (Path 1 → Path 2)
- [x] Each path traced to terminal point
- [x] Convergence points identified (none)
- [x] Execution continuation point determined (shape43)

### Sequence Diagram
- [x] Format follows required structure
- [x] Each operation shows READS and WRITES
- [x] Decisions show both TRUE and FALSE paths
- [x] Try/Catch patterns shown correctly
- [x] Sequential branch execution shown correctly
- [x] Subprocess internal flows documented
- [x] Subprocess return paths mapped to main process

### Subprocess Analysis
- [x] Subprocess analyzed (internal flow traced)
- [x] Return paths identified (success: shape5, shape9; failure: shape10)
- [x] Return path labels mapped to main process (implicit via stop/exception)
- [x] Properties written by subprocess documented (6 properties)
- [x] Properties read by subprocess from main process documented (9 properties)

### Edge Cases
- [x] Nested decisions analyzed (shape2 → shape44)
- [x] Try/Catch error paths documented
- [x] Sequential branch with data dependency documented
- [x] Subprocess with conditional execution documented

### Property Extraction Completeness
- [x] All property patterns searched (${}, %%, {})
- [x] Message parameters checked for process properties
- [x] Operation headers/path parameters checked
- [x] Decision track properties identified (meta.*)
- [x] Document properties that read other properties identified

### Input/Output Structure Analysis (CONTRACT VERIFICATION)
- [x] Entry point operation identified (8f709c2b-e63f-4d5f-9374-2932ed70415d)
- [x] Request profile identified and loaded (febfa3e1-f719-4ee8-ba57-cdae34137ab3)
- [x] Request profile structure analyzed (JSON, single object)
- [x] Array vs single object detected (single object)
- [x] ALL request fields extracted (9 fields)
- [x] Request field paths documented
- [x] Request field mapping table generated
- [x] Response profile identified and loaded (f4ca3a70-114a-4601-bad8-44a3eb20e2c0)
- [x] Response profile structure analyzed (JSON, single object)
- [x] ALL response fields extracted (4 fields)
- [x] Response field mapping table generated
- [x] Document processing behavior determined (single_document)
- [x] Input/Output structure documented in Phase 1 document

### HTTP Status Codes and Return Path Responses
- [x] All return paths documented with HTTP status codes (4 return paths)
- [x] Response JSON examples provided for each return path
- [x] Populated fields documented for each return path
- [x] Decision conditions leading to each return documented
- [x] Downstream operation HTTP status codes documented
- [x] Error handling strategy documented

### Request/Response JSON Examples
- [x] Process Layer entry point request JSON example provided
- [x] Process Layer response JSON examples provided (success and error)
- [x] Downstream System Layer request JSON examples provided
- [x] Downstream System Layer response JSON examples provided

### Map Analysis
- [x] ALL map files identified and loaded (3 maps)
- [x] Field mappings extracted from each map
- [x] Profile vs map field name discrepancies documented (none found)
- [x] Map field names marked as AUTHORITATIVE
- [x] Scripting functions analyzed (1 function in error map)
- [x] Static values identified and documented (default values in maps)
- [x] Process property mappings documented (error message property)
- [x] Map Analysis documented in Phase 1 document

---

## 18. System Layer Identification

### Third-Party Systems

#### System 1: Oracle Fusion HCM

**System Type:** Cloud ERP - Human Capital Management  
**API Type:** REST API  
**Authentication:** Basic Auth / OAuth (configured in connection)  
**Base URL:** Configured in connection (aa1fcb29-d146-4425-9ea6-b9698090f60e)  
**Resource Path:** hcmRestApi/resources/11.13.18.05/absences

**Operations:**
- **Create Absence Entry:** POST to absences endpoint
  - Request: Leave absence details (personNumber, absenceType, dates, durations, status codes)
  - Response: Complete absence entry object with personAbsenceEntryId

**Error Handling:**
- HTTP 400: Bad Request (invalid input data)
- HTTP 401: Unauthorized (authentication failed)
- HTTP 404: Not Found (employee or resource not found)
- HTTP 500: Internal Server Error (Oracle system error)
- Response may be gzip compressed (Content-Encoding: gzip)

#### System 2: Office 365 Email (SMTP)

**System Type:** Email Service  
**Protocol:** SMTP  
**Authentication:** SMTP AUTH  
**Connection:** Configured in connection (00eae79b-2303-4215-8067-dcc299e42697)

**Operations:**
- **Send Email with Attachment:** Send error notification with payload attachment
- **Send Email without Attachment:** Send error notification without attachment

**Configuration:**
- From Address: Boomi.Dev.failures@al-ghurair.com
- To Address: BoomiIntegrationTeam@al-ghurair.com (configurable)
- Subject: Environment prefix + error details
- Body: HTML formatted error details

### Integration Pattern

**Pattern:** Synchronous Request-Response with Error Notification

**Flow:**
1. D365 sends leave creation request to Boomi (Web Service Server)
2. Boomi transforms request to Oracle Fusion format
3. Boomi calls Oracle Fusion HCM API (synchronous HTTP POST)
4. Oracle Fusion processes request and returns response
5. Boomi checks HTTP status code:
   - If 2xx: Extract personAbsenceEntryId and return success response to D365
   - If non-2xx: Extract error message, send email notification, return error response to D365
6. If any error occurs during transformation or setup: Send email notification and return error response to D365

**Error Notification:**
- Sent to configured email recipients (BoomiIntegrationTeam@al-ghurair.com)
- Includes: Process Name, Environment, Execution ID, Error Details
- Optional: Original payload as attachment

---

## 19. Function Exposure Decision Table

### Process Layer Function Exposure

**Process Name:** HCM_Leave Create  
**Business Domain:** Human Resource (HCM)  
**Integration Type:** Synchronous API (Web Service Server)

| Function | Expose as Azure Function? | Reasoning | Recommended Approach |
|----------|---------------------------|-----------|---------------------|
| Create Leave in Oracle Fusion | ✅ YES | This is the core business function - creating leave absence entries in Oracle Fusion HCM from D365 requests | **Process Layer Function:** `CreateLeaveInOracleFusion` |

### Function Exposure Analysis

**Function:** Create Leave in Oracle Fusion

**Expose:** ✅ YES

**Reasoning:**
1. **Business Value:** This is a complete business process that creates leave absence entries in Oracle Fusion HCM
2. **Reusability:** This function can be reused by any system that needs to create leave entries in Oracle Fusion
3. **Encapsulation:** The function encapsulates the entire leave creation workflow including:
   - Input validation and transformation
   - Oracle Fusion API call
   - Error handling with email notifications
   - Response mapping
4. **API-Led Architecture:** This is a Process Layer function that orchestrates System Layer calls (Oracle Fusion API)
5. **Single Responsibility:** The function has a single, well-defined purpose: Create leave absence entry

**Recommended Azure Function:**

**Function Name:** `CreateLeaveInOracleFusion`  
**HTTP Trigger:** POST  
**Route:** `/api/hcm/leaves`  
**Request DTO:** `CreateLeaveRequestDto`  
**Response DTO:** `CreateLeaveResponseDto`

**Function Signature:**
```csharp
[FunctionName("CreateLeaveInOracleFusion")]
public async Task<IActionResult> Run(
    [HttpTrigger(AuthorizationLevel.Function, "post", Route = "hcm/leaves")] HttpRequest req,
    ILogger log)
```

**System Layer Functions Called:**
1. `OracleFusionHcmClient.CreateAbsenceEntry()` - Calls Oracle Fusion HCM API to create absence entry
2. `EmailNotificationClient.SendErrorNotification()` - Sends error notification email (only on error)

### Function Explosion Prevention

**Total Functions Exposed:** 1 Process Layer function

**Rationale:**
- This process has a single, well-defined business function: Create leave in Oracle Fusion
- The error notification is a supporting function, not a separate business function
- The subprocess (Office 365 Email) is a reusable utility, not a business function
- No function explosion risk - this is a focused, single-purpose integration

**System Layer Functions (Not Exposed as Separate Azure Functions):**
- Oracle Fusion HCM API calls → Implemented as `OracleFusionHcmClient` class
- Email notifications → Implemented as `EmailNotificationClient` class

---

## 20. Summary

### Process Overview

**Process:** HCM_Leave Create  
**Purpose:** Synchronize leave data between D365 and Oracle HCM  
**Pattern:** Synchronous Request-Response with Error Notification

### Key Characteristics

1. **Entry Point:** Web Service Server (wss) - receives leave creation requests from D365
2. **Downstream System:** Oracle Fusion HCM REST API - creates absence entries
3. **Error Handling:** Try/Catch with email notification to configured recipients
4. **Response Handling:** Multiple return paths based on HTTP status code and error conditions
5. **Subprocess:** Office 365 Email - sends error notification emails with or without attachments

### Execution Paths

1. **Success Path:** Transform request → Call Oracle Fusion API → Map success response → Return success (HTTP 200)
2. **HTTP Error Path (Gzip):** Call Oracle Fusion API → Decompress gzip response → Extract error → Map error → Return error (HTTP 400)
3. **HTTP Error Path (No Gzip):** Call Oracle Fusion API → Extract error → Map error → Return error (HTTP 400)
4. **Try/Catch Error Path:** Error during transformation/setup → Send email notification → Map error → Return error (HTTP 400)

### Critical Dependencies

1. shape38 MUST execute FIRST (writes logging properties)
2. shape8 MUST execute BEFORE shape33 (writes URL for HTTP operation)
3. shape33 MUST execute BEFORE shape2 (produces status code for decision)
4. shape19 MUST execute BEFORE shape21 AND shape41 (writes error message)
5. Branch shape20: Path 1 MUST execute BEFORE Path 2 (sequential execution)

### Data Flow

**Input:** D365 leave request (9 fields: employeeNumber, absenceType, employer, dates, durations, status codes)  
**Output:** Success response (4 fields: status, message, personAbsenceEntryId, success) OR Error response (3 fields: status, message, success)

### Azure Function Recommendation

**Function Name:** `CreateLeaveInOracleFusion`  
**Layer:** Process Layer  
**System Layer Clients:** OracleFusionHcmClient, EmailNotificationClient

---

**END OF PHASE 1 EXTRACTION**
