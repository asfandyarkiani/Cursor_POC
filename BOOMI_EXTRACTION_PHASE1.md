# BOOMI EXTRACTION PHASE 1 - HCM Leave Create

**Process Name:** HCM_Leave Create  
**Process ID:** ca69f858-785f-4565-ba1f-b4edc6cca05b  
**Description:** This Process will sync the Leave data between D365 and Oracle HCM  
**System of Record (SOR):** Oracle Fusion HCM  
**Business Domain:** Human Resource Management (HCM) - Leaves  

---

## OPERATIONS INVENTORY

### External Operations (Calls to SOR)

| Operation ID | Operation Name | Type | Method | Connection | Purpose |
|--------------|----------------|------|--------|------------|---------|
| 6e8920fd-af5a-430b-a1d9-9fde7ac29a12 | Leave Oracle Fusion Create | HTTP | POST | Oracle Fusion (aa1fcb29-d146-4425-9ea6-b9698090f60e) | Create absence/leave record in Oracle Fusion HCM |

### Entry Point Operations

| Operation ID | Operation Name | Type | Purpose |
|--------------|----------------|------|---------|
| 8f709c2b-e63f-4d5f-9374-2932ed70415d | Create Leave Oracle Fusion OP | WebService (Listen) | HTTP entry point accepting leave creation requests from D365 |

### Supporting Operations (Email Notifications - NOT System Layer)

| Operation ID | Operation Name | Type | Purpose |
|--------------|----------------|------|---------|
| af07502a-fafd-4976-a691-45d51a33b549 | Email w Attachment | Mail | Send error notification email with attachment |
| 15a72a21-9b57-49a1-a8ed-d70367146644 | Email W/O Attachment | Mail | Send error notification email without attachment |

**Note:** Email operations are cross-cutting concerns handled by error notification subprocess - NOT part of System Layer implementation.

---

## STEP 1a: INPUT STRUCTURE ANALYSIS (BLOCKING)

### Entry Point: Create Leave Oracle Fusion OP (8f709c2b-e63f-4d5f-9374-2932ed70415d)

**Profile:** D365 Leave Create JSON Profile (febfa3e1-f719-4ee8-ba57-cdae34137ab3)

**Input Structure:**
```json
{
  "employeeNumber": 9000604,           // number - Employee ID from D365
  "absenceType": "Sick Leave",         // string - Type of leave
  "employer": "Al Ghurair Investment LLC", // string - Employer name
  "startDate": "2024-03-24",           // string (date) - Leave start date
  "endDate": "2024-03-25",             // string (date) - Leave end date
  "absenceStatusCode": "SUBMITTED",    // string - Status of absence
  "approvalStatusCode": "APPROVED",    // string - Approval status
  "startDateDuration": 1,              // number - Duration on start date
  "endDateDuration": 1                 // number - Duration on end date
}
```

**Field Mapping:**
- `employeeNumber` → Maps to `personNumber` in Oracle HCM request
- `absenceType` → Maps to `absenceType` in Oracle HCM request
- `employer` → Maps to `employer` in Oracle HCM request
- `startDate` → Maps to `startDate` in Oracle HCM request
- `endDate` → Maps to `endDate` in Oracle HCM request
- `absenceStatusCode` → Maps to `absenceStatusCd` in Oracle HCM request
- `approvalStatusCode` → Maps to `approvalStatusCd` in Oracle HCM request
- `startDateDuration` → Maps to `startDateDuration` in Oracle HCM request
- `endDateDuration` → Maps to `endDateDuration` in Oracle HCM request

**Validation Requirements:**
- All fields are required for leave creation
- Date format: YYYY-MM-DD
- Employee number must be valid

---

## STEP 1b: RESPONSE STRUCTURE ANALYSIS (BLOCKING)

### Oracle Fusion HCM Leave Create Response

**Profile:** Oracle Fusion Leave Response JSON Profile (316175c7-0e45-4869-9ac6-5f9d69882a62)

**Success Response Structure (HTTP 20x):**
```json
{
  "personAbsenceEntryId": 123456,      // number - Unique ID of created absence entry
  "absenceCaseId": "ABC123",           // string - Case ID
  "absenceType": "Sick Leave",         // string - Type of absence
  "employer": "Al Ghurair Investment LLC", // string - Employer
  "startDate": "2024-03-24",           // string - Start date
  "endDate": "2024-03-25",             // string - End date
  "absenceStatusCd": "SUBMITTED",      // string - Status code
  "approvalStatusCd": "APPROVED",      // string - Approval status
  "duration": 2,                       // number - Total duration
  "personNumber": "9000604",           // string - Employee number
  // ... many other Oracle HCM fields ...
}
```

**Key Response Fields:**
- `personAbsenceEntryId` - Primary identifier of created leave record (CRITICAL)
- `absenceStatusCd` - Status of the absence
- `approvalStatusCd` - Approval status
- `duration` - Total leave duration

**Error Response Structure (HTTP non-20x or gzip compressed):**
- Error responses may be gzip compressed (Content-Encoding: gzip)
- Error message extracted from `meta.base.applicationstatusmessage` tracking property
- HTTP status code from `meta.base.applicationstatuscode` tracking property

---

## STEP 1c: OPERATION RESPONSE ANALYSIS (BLOCKING)

### Operation: Leave Oracle Fusion Create (6e8920fd-af5a-430b-a1d9-9fde7ac29a12)

**HTTP Configuration:**
- **Method:** POST
- **Content-Type:** application/json
- **URL:** Dynamic (set via document property `dynamicdocument.URL`)
  - Base URL: `https://iaaxey-dev3.fa.ocs.oraclecloud.com:443` (from connection)
  - Resource Path: `hcmRestApi/resources/11.13.18.05/absences` (from process property)
  - Full URL: `{BaseURL}/{ResourcePath}`
- **Authentication:** Basic Authentication
  - Username: `INTEGRATION.USER@al-ghurair.com`
  - Password: Encrypted (stored in connection)
- **Return Errors:** true
- **Return Responses:** true
- **Follow Redirects:** false

**Response Handling:**
- **Response Header Mapping:**
  - `Content-Encoding` header → `DDP_RespHeader` document property
  - Used to detect gzip compression
- **Success Criteria:** HTTP status code 20* (200-209)
- **Error Handling:**
  - Non-20* status codes → Error path
  - Gzip compressed responses → Decompress before processing
  - Error message captured in tracking property

**Tracked Fields:**
- Input: `hr_employee_id` (from employeeNumber field)
- Response: HTTP status code, status message

---

## STEP 1d: MAP ANALYSIS (BLOCKING)

### Map 1: Leave Create Map (c426b4d6-2aff-450e-b43b-59956c4dbc96)
**From:** D365 Leave Create JSON Profile (febfa3e1-f719-4ee8-ba57-cdae34137ab3)  
**To:** HCM Leave Create JSON Profile (a94fa205-c740-40a5-9fda-3d018611135a)

**Field Mappings:**
```
employeeNumber       → personNumber
absenceType          → absenceType
employer             → employer
startDate            → startDate
endDate              → endDate
absenceStatusCode    → absenceStatusCd
approvalStatusCode   → approvalStatusCd
startDateDuration    → startDateDuration
endDateDuration      → endDateDuration
```

**Purpose:** Transform D365 leave request to Oracle HCM format (field name changes only)

### Map 2: Oracle Fusion Leave Response Map (e4fd3f59-edb5-43a1-aeae-143b600a064e)
**From:** Oracle Fusion Leave Response JSON Profile (316175c7-0e45-4869-9ac6-5f9d69882a62)  
**To:** Leave D365 Response (f4ca3a70-114a-4601-bad8-44a3eb20e2c0)

**Field Mappings:**
```
personAbsenceEntryId → personAbsenceEntryId (extracted from Oracle response)
```

**Default Values:**
```
status  = "success"
message = "Data successfully sent to Oracle Fusion"
success = "true"
```

**Purpose:** Transform Oracle HCM success response to D365 response format

### Map 3: Leave Error Map (f46b845a-7d75-41b5-b0ad-c41a6a8e9b12)
**From:** Dummy FF Profile (23d7a2e9-5cb0-4e9c-9e4b-3154834bad0d)  
**To:** Leave D365 Response (f4ca3a70-114a-4601-bad8-44a3eb20e2c0)

**Field Mappings:**
```
DPP_ErrorMessage (process property) → message
```

**Default Values:**
```
status  = "failure"
success = "false"
```

**Purpose:** Transform error message to D365 error response format

**CRITICAL FIELD NAMES FOR SOAP/HTTP ENVELOPES:**
- Request to Oracle: `personNumber`, `absenceType`, `employer`, `startDate`, `endDate`, `absenceStatusCd`, `approvalStatusCd`, `startDateDuration`, `endDateDuration`
- Response from Oracle: `personAbsenceEntryId`, `absenceStatusCd`, `approvalStatusCd`, `duration`
- Response to D365: `status`, `message`, `personAbsenceEntryId`, `success`

---

## STEP 1e: HTTP STATUS CODES AND RETURN PATHS (BLOCKING)

### Decision Point: HTTP Status 20* Check (shape2)

**Decision Logic:**
```
IF meta.base.applicationstatuscode matches "20*" (wildcard)
  THEN → Success Path (shape34)
  ELSE → Error Path (shape44)
```

**Success Path (HTTP 20*):**
1. Map Oracle response to D365 success format (shape34 - map e4fd3f59)
2. Return success response (shape35)

**Error Path (HTTP non-20*):**
1. Check if response is gzip compressed (shape44)
   - IF Content-Encoding == "gzip" THEN decompress (shape45)
   - ELSE proceed to error mapping
2. Extract error message (shape39 or shape46)
3. Map to D365 error format (shape40 or shape47 - map f46b845a)
4. Return error response (shape36 or shape48)

**HTTP Status Code Handling:**
- **20x (200-209):** Success - Extract `personAbsenceEntryId` and return success
- **4xx (Client Errors):** Error - Return error message from Oracle
- **5xx (Server Errors):** Error - Return error message from Oracle

**Return Paths:**
- **Success Response (shape35):** Returns to caller with success status
- **Error Response (shape36, shape43, shape48):** Returns to caller with error status

**Try/Catch Error Handling (shape17):**
- **Try Path:** Normal execution flow
- **Catch Path:** Captures exceptions and routes to error notification subprocess (shape21)
  - Error message stored in `DPP_ErrorMessage` process property
  - Email notification sent via subprocess (a85945c5-3004-42b9-80b1-104f465cd1fb)

---

## STEPS 2-3: PROCESS PROPERTIES ANALYSIS

### Process Properties WRITTEN (Set/Assigned)

| Shape | Property Name | Source | Purpose |
|-------|---------------|--------|---------|
| shape38 | DPP_Process_Name | Execution: Process Name | Track process name for logging |
| shape38 | DPP_AtomName | Execution: Atom Name | Track atom name for logging |
| shape38 | DPP_Payload | Current document | Store input payload for error reporting |
| shape38 | DPP_ExecutionID | Execution: Execution Id | Track execution ID for logging |
| shape38 | DPP_File_Name | Process Name + Timestamp + ".txt" | Generate filename for error attachment |
| shape38 | DPP_Subject | Atom Name + " (" + Process Name + ") has errors to report" | Generate email subject |
| shape38 | To_Email | PP_HCM_LeaveCreate_Properties.To_Email | Set recipient email address |
| shape38 | DPP_HasAttachment | PP_HCM_LeaveCreate_Properties.DPP_HasAttachment | Set attachment flag |
| shape8 | dynamicdocument.URL | PP_HCM_LeaveCreate_Properties.Resource_Path | Set Oracle HCM API endpoint |
| shape19 | DPP_ErrorMessage | Track: meta.base.catcherrorsmessage | Store try/catch error message |
| shape39 | DPP_ErrorMessage | Track: meta.base.applicationstatusmessage | Store HTTP error message |
| shape46 | DPP_ErrorMessage | Current document | Store decompressed error message |

### Process Properties READ (Consumed)

| Shape | Property Name | Used For |
|-------|---------------|----------|
| shape8 | PP_HCM_LeaveCreate_Properties.Resource_Path | Build Oracle HCM API URL |
| shape38 | PP_HCM_LeaveCreate_Properties.To_Email | Email notification recipient |
| shape38 | PP_HCM_LeaveCreate_Properties.DPP_HasAttachment | Email attachment flag |
| shape21 (subprocess) | DPP_Process_Name | Email notification content |
| shape21 (subprocess) | DPP_AtomName | Email notification content |
| shape21 (subprocess) | DPP_Payload | Email attachment content |
| shape21 (subprocess) | DPP_ExecutionID | Email notification content |
| shape21 (subprocess) | DPP_ErrorMessage | Email notification content |
| shape21 (subprocess) | DPP_File_Name | Email attachment filename |
| shape21 (subprocess) | DPP_Subject | Email subject line |
| shape21 (subprocess) | To_Email | Email recipient |
| shape21 (subprocess) | DPP_HasAttachment | Email attachment flag |

### Defined Process Properties (Configuration)

**Component:** PP_HCM_LeaveCreate_Properties (e22c04db-7dfa-4b1b-9d88-77365b0fdb18)

| Property Key | Property Name | Default Value | Purpose |
|--------------|---------------|---------------|---------|
| c2d1a1e8-4bcb-435b-b1d2-bb10a209bcc0 | Resource_Path | hcmRestApi/resources/11.13.18.05/absences | Oracle HCM API resource path |
| 71c90f6e-4f86-49f3-8aef-bae6668c73f9 | To_Email | BoomiIntegrationTeam@al-ghurair.com | Error notification recipient |
| a717867b-67f4-4a72-be2d-c99c73309fdb | DPP_HasAttachment | Y | Include payload as attachment in error email |

**Component:** PP_Office365_Email (0feff13f-8a2c-438a-b3c7-1909e2a7f533) - Used by subprocess

| Property Key | Property Name | Default Value | Purpose |
|--------------|---------------|---------------|---------|
| 804b6d40-eeee-4ac0-ab4b-94e1aca9f4b7 | From_Email | Boomi.Dev.failures@al-ghurair.com | Email sender address |
| 600acadb-ee02-4369-af85-ee70af380b6c | To_Email | Rajesh.Muppala@al-ghurair.com;mohan.jonnalagadda@al-ghurair.com | Email recipients |
| 2fa6ce9e-437a-44cc-b44f-5c7e61052f41 | HasAttachment | Y | Include attachment flag |
| 3ca9f307-cecb-4d1e-b9ec-007839509ed7 | EmailBody | (empty) | Email body content |
| d8fc7496-ec2c-4308-a4ca-e9f49c0c9500 | Environment | DEV Failure : | Environment prefix for email subject |

---

## STEP 4: DATA DEPENDENCY GRAPH (BLOCKING)

### Data Flow Diagram

```
[D365 Input] 
    ↓
[shape38: Store Input Properties]
    ↓ (DPP_Payload, DPP_Process_Name, etc.)
[shape17: Try/Catch Block]
    ↓ (Try Path)
[shape29: Map D365 → Oracle HCM] (map c426b4d6)
    ↓ (Transformed Request)
[shape8: Set Dynamic URL]
    ↓ (dynamicdocument.URL = Resource_Path)
[shape49: Log Request]
    ↓
[shape33: HTTP POST to Oracle HCM] (operation 6e8920fd)
    ↓ (HTTP Response + Status Code)
[shape2: Decision - HTTP Status 20*?]
    ↓                               ↓
  YES (20*)                       NO (non-20*)
    ↓                               ↓
[shape34: Map Success Response]  [shape44: Decision - Gzip?]
    ↓                               ↓                    ↓
[shape35: Return Success]        YES (gzip)          NO (plain)
                                    ↓                    ↓
                                [shape45: Decompress] [shape39: Extract Error]
                                    ↓                    ↓
                                [shape46: Extract Error] [shape40: Map Error]
                                    ↓                    ↓
                                [shape47: Map Error]  [shape36: Return Error]
                                    ↓
                                [shape48: Return Error]

[shape17: Try/Catch Block]
    ↓ (Catch Path - Exception)
[shape20: Branch]
    ↓                    ↓
[shape19: Store Error] [shape41: Map Error]
    ↓                    ↓
[shape21: Email Subprocess] [shape43: Return Error]
```

### Data Dependencies

**Input Dependencies:**
- `employeeNumber` (D365) → Required for Oracle HCM request
- `absenceType` (D365) → Required for Oracle HCM request
- `startDate`, `endDate` (D365) → Required for Oracle HCM request
- `absenceStatusCode`, `approvalStatusCode` (D365) → Required for Oracle HCM request
- `startDateDuration`, `endDateDuration` (D365) → Required for Oracle HCM request

**Configuration Dependencies:**
- `Resource_Path` (PP_HCM_LeaveCreate_Properties) → Required to build API URL
- `To_Email` (PP_HCM_LeaveCreate_Properties) → Required for error notifications
- Oracle Fusion connection credentials → Required for authentication

**Output Dependencies:**
- `personAbsenceEntryId` (Oracle HCM) → Returned to D365 on success
- `status`, `message`, `success` → Returned to D365 on success/error

**Error Handling Dependencies:**
- `DPP_ErrorMessage` → Captured from exceptions or HTTP errors
- `DPP_Payload` → Stored for error reporting
- Email subprocess properties → Required for error notifications

---

## STEP 5: CONTROL FLOW GRAPH

### Main Process Flow

```
START (shape1)
    ↓
[shape38: Input_details - Store execution properties]
    ↓
[shape17: Try/Catch Error Handler]
    ├─ Try Path →
    │   ↓
    │   [shape29: Map D365 → Oracle HCM]
    │   ↓
    │   [shape8: Set URL from Resource_Path]
    │   ↓
    │   [shape49: Notify/Log request]
    │   ↓
    │   [shape33: HTTP POST to Oracle Fusion]
    │   ↓
    │   [shape2: Decision - HTTP Status 20*?]
    │   ├─ TRUE (20*) →
    │   │   ↓
    │   │   [shape34: Map success response]
    │   │   ↓
    │   │   [shape35: Return Success Response]
    │   │   ↓
    │   │   END
    │   │
    │   └─ FALSE (non-20*) →
    │       ↓
    │       [shape44: Decision - Content-Encoding == gzip?]
    │       ├─ TRUE (gzip) →
    │       │   ↓
    │       │   [shape45: Decompress gzip response]
    │       │   ↓
    │       │   [shape46: Extract error message]
    │       │   ↓
    │       │   [shape47: Map error response]
    │       │   ↓
    │       │   [shape48: Return Error Response]
    │       │   ↓
    │       │   END
    │       │
    │       └─ FALSE (plain) →
    │           ↓
    │           [shape39: Extract error message]
    │           ↓
    │           [shape40: Map error response]
    │           ↓
    │           [shape36: Return Error Response]
    │           ↓
    │           END
    │
    └─ Catch Path (Exception) →
        ↓
        [shape20: Branch (2 parallel paths)]
        ├─ Branch 1 →
        │   ↓
        │   [shape19: Store error message in DPP_ErrorMessage]
        │   ↓
        │   [shape21: Call Email Subprocess]
        │   ↓
        │   END (Abort process)
        │
        └─ Branch 2 →
            ↓
            [shape41: Map error response]
            ↓
            [shape43: Return Error Response]
            ↓
            END
```

### Control Flow Nodes

**Entry Point:** shape1 (Start)

**Sequential Execution:**
1. shape38 → shape17 → shape29 → shape8 → shape49 → shape33 → shape2

**Decision Points:**
1. **shape2:** HTTP Status 20* check
   - TRUE → shape34 → shape35 (Success path)
   - FALSE → shape44 (Error path)

2. **shape44:** Content-Encoding gzip check
   - TRUE → shape45 → shape46 → shape47 → shape48 (Gzip error path)
   - FALSE → shape39 → shape40 → shape36 (Plain error path)

3. **shape17:** Try/Catch error handler
   - Try → Normal flow (shape29)
   - Catch → Error handling (shape20)

**Parallel Execution:**
- **shape20:** Branch shape (2 parallel paths)
  - Branch 1: shape19 → shape21 (Email notification)
  - Branch 2: shape41 → shape43 (Error response)

**Exit Points:**
- shape35 (Success Response)
- shape36 (Error Response - plain)
- shape43 (Error Response - exception)
- shape48 (Error Response - gzip)
- shape21 (Abort after email notification)

---

## STEP 6: REVERSE FLOW MAPPING

### Reverse Mapping: Output to Input

**Success Response Fields:**
```
personAbsenceEntryId (Output) ← personAbsenceEntryId (Oracle Response)
                                ← Created by Oracle HCM after successful POST
                                ← Input: employeeNumber, absenceType, startDate, endDate, etc.

status (Output) ← Default value "success"
                ← Condition: HTTP status code 20*

message (Output) ← Default value "Data successfully sent to Oracle Fusion"
                 ← Condition: HTTP status code 20*

success (Output) ← Default value "true"
                 ← Condition: HTTP status code 20*
```

**Error Response Fields:**
```
status (Output) ← Default value "failure"
                ← Condition: HTTP status code non-20* OR exception

message (Output) ← DPP_ErrorMessage process property
                 ← Source 1: meta.base.applicationstatusmessage (HTTP error)
                 ← Source 2: meta.base.catcherrorsmessage (Exception)
                 ← Source 3: Decompressed gzip response

success (Output) ← Default value "false"
                 ← Condition: HTTP status code non-20* OR exception

personAbsenceEntryId (Output) ← Not populated on error
```

### Reverse Mapping: Oracle HCM Request Fields

```
personNumber (Oracle Request) ← employeeNumber (D365 Input)
absenceType (Oracle Request) ← absenceType (D365 Input)
employer (Oracle Request) ← employer (D365 Input)
startDate (Oracle Request) ← startDate (D365 Input)
endDate (Oracle Request) ← endDate (D365 Input)
absenceStatusCd (Oracle Request) ← absenceStatusCode (D365 Input)
approvalStatusCd (Oracle Request) ← approvalStatusCode (D365 Input)
startDateDuration (Oracle Request) ← startDateDuration (D365 Input)
endDateDuration (Oracle Request) ← endDateDuration (D365 Input)
```

### Reverse Mapping: Configuration Properties

```
dynamicdocument.URL (HTTP Request) ← PP_HCM_LeaveCreate_Properties.Resource_Path
                                    ← Connection Base URL: https://iaaxey-dev3.fa.ocs.oraclecloud.com:443
                                    ← Full URL: {BaseURL}/{Resource_Path}

To_Email (Error Notification) ← PP_HCM_LeaveCreate_Properties.To_Email
DPP_HasAttachment (Error Notification) ← PP_HCM_LeaveCreate_Properties.DPP_HasAttachment
```

---

## STEP 7: DECISION SHAPE ANALYSIS (BLOCKING)

### Decision 1: HTTP Status 20* Check (shape2)

**Location:** After HTTP POST to Oracle Fusion (shape33)

**Decision Logic:**
```
Property: meta.base.applicationstatuscode
Comparison: wildcard match
Pattern: "20*"
```

**Branches:**
- **TRUE:** HTTP status code matches 20* (200-209)
  - **Action:** Proceed to success response mapping (shape34)
  - **Destination:** shape34 → shape35 (Return Success Response)
  
- **FALSE:** HTTP status code does NOT match 20* (4xx, 5xx, etc.)
  - **Action:** Proceed to error handling (shape44)
  - **Destination:** shape44 (Check Content-Encoding)

**Business Logic:**
- Oracle Fusion HCM returns HTTP 20x for successful leave creation
- Any non-20x status indicates an error (validation failure, server error, etc.)
- This decision determines success vs. error response path

**System Layer Responsibility:**
- ✅ YES - This is HTTP status code checking (technical concern)
- This decision belongs in System Layer as it's SOR-specific error detection

### Decision 2: Content-Encoding Gzip Check (shape44)

**Location:** Error path after HTTP Status 20* check fails

**Decision Logic:**
```
Property: dynamicdocument.DDP_RespHeader (Content-Encoding header)
Comparison: equals
Value: "gzip"
```

**Branches:**
- **TRUE:** Response is gzip compressed
  - **Action:** Decompress gzip response (shape45)
  - **Destination:** shape45 → shape46 → shape47 → shape48 (Return Error Response)
  
- **FALSE:** Response is plain text/JSON
  - **Action:** Extract error message directly (shape39)
  - **Destination:** shape39 → shape40 → shape36 (Return Error Response)

**Business Logic:**
- Oracle Fusion HCM may return gzip-compressed error responses
- Decompression is required before error message extraction
- This is a technical concern for proper error handling

**System Layer Responsibility:**
- ✅ YES - This is response format handling (technical concern)
- This decision belongs in System Layer as it's SOR-specific response processing

### Decision 3: Try/Catch Error Handler (shape17)

**Location:** Wraps entire main process flow

**Decision Logic:**
```
Catch All Exceptions: true
Retry Count: 0
```

**Branches:**
- **Try Path (default):** Normal execution flow
  - **Action:** Execute main process logic (shape29 → shape8 → shape49 → shape33 → shape2)
  - **Destination:** Normal flow with success/error responses
  
- **Catch Path (error):** Exception occurred
  - **Action:** Handle exception and send error notification
  - **Destination:** shape20 (Branch) → shape19/shape21 (Email) + shape41/shape43 (Error Response)

**Business Logic:**
- Catches any unexpected exceptions (network failures, parsing errors, etc.)
- Sends error notification email to integration team
- Returns error response to caller

**System Layer Responsibility:**
- ✅ YES - Exception handling is System Layer responsibility
- Email notification is cross-cutting concern (handled by middleware/subprocess)

### Self-Check Questions (MANDATORY)

**Q1: Have I identified ALL decision shapes in the process?**
✅ YES - Identified 3 decision points:
1. shape2 (HTTP Status 20* check)
2. shape44 (Content-Encoding gzip check)
3. shape17 (Try/Catch error handler)

**Q2: For EACH decision, have I documented the comparison logic?**
✅ YES - Documented comparison logic for all 3 decisions:
1. shape2: wildcard match on "20*"
2. shape44: equals comparison on "gzip"
3. shape17: catch all exceptions

**Q3: For EACH decision, have I documented BOTH branches (true/false or try/catch)?**
✅ YES - Documented both branches for all decisions with destinations and actions

**Q4: Have I determined WHERE each decision belongs (System Layer vs Process Layer)?**
✅ YES - All 3 decisions belong in System Layer:
1. HTTP status checking is SOR-specific technical concern
2. Response decompression is SOR-specific technical concern
3. Exception handling is System Layer responsibility

**Q5: Can I explain WHY each decision belongs where it does?**
✅ YES - Explained reasoning:
- shape2: HTTP status codes are SOR-specific (Oracle HCM uses 20x for success)
- shape44: Response format handling is SOR-specific (Oracle HCM may gzip compress errors)
- shape17: Exception handling is System Layer responsibility (technical failures)

---

## STEP 7a: SUBPROCESS ANALYSIS

### Subprocess: (Sub) Office 365 Email (a85945c5-3004-42b9-80b1-104f465cd1fb)

**Purpose:** Send error notification emails to integration team

**Invocation:** Called from shape21 (Catch path of Try/Catch error handler)

**Invocation Parameters:**
- `abort: true` - Process aborts after subprocess completes
- `wait: true` - Wait for subprocess to complete before continuing

**Input Properties (from parent process):**
- `DPP_Process_Name` - Process name for email content
- `DPP_AtomName` - Atom name for email content
- `DPP_ExecutionID` - Execution ID for email content
- `DPP_ErrorMessage` - Error message for email content
- `DPP_Payload` - Input payload for email attachment
- `DPP_File_Name` - Filename for email attachment
- `DPP_Subject` - Email subject line
- `To_Email` - Email recipient address
- `DPP_HasAttachment` - Flag to include attachment

**Subprocess Flow:**
1. **Start** (shape1) - Receives email content from parent process
2. **Try/Catch** (shape2) - Wraps email sending logic
3. **Decision** (shape4) - Check if attachment is required (DPP_HasAttachment == "Y")
   - **TRUE:** Send email with attachment (shape11 → shape14 → shape15 → shape6 → shape3)
   - **FALSE:** Send email without attachment (shape23 → shape22 → shape20 → shape7)
4. **Stop** (shape5 or shape9) - Email sent successfully
5. **Catch Path** (shape10) - Exception occurred, throw exception

**Email Operations:**
- **With Attachment:** operation af07502a-fafd-4976-a691-45d51a33b549 (Email w Attachment)
- **Without Attachment:** operation 15a72a21-9b57-49a1-a8ed-d70367146644 (Email W/O Attachment)

**Connection:** Office 365 Email (00eae79b-2303-4215-8067-dcc299e42697)
- SMTP Host: smtp-mail.outlook.com
- Port: 587
- Use TLS: true
- Authentication: SMTP AUTH with username/password

**System Layer Responsibility:**
- ❌ NO - Email notification is NOT part of System Layer
- This is a cross-cutting concern handled by error notification infrastructure
- System Layer should throw exceptions; middleware/infrastructure handles notifications

**Implementation Note:**
- System Layer will NOT implement email functionality
- System Layer will throw exceptions with error details
- ExceptionHandlerMiddleware will handle error responses
- Email notifications (if needed) should be handled by Process Layer or separate monitoring infrastructure

---

## STEP 8: BRANCH SHAPE ANALYSIS (BLOCKING)

### Branch 1: Error Handling Branch (shape20)

**Location:** Catch path of Try/Catch error handler (shape17)

**Number of Branches:** 2 (parallel execution)

**Branch Configuration:**
```
numBranches: 2
```

**Branch 1 (Identifier: 1):**
- **Destination:** shape19 (Store error message in DPP_ErrorMessage)
- **Flow:** shape19 → shape21 (Call Email Subprocess)
- **Purpose:** Send error notification email to integration team
- **Action:** 
  1. Extract error message from `meta.base.catcherrorsmessage`
  2. Store in `DPP_ErrorMessage` process property
  3. Call email subprocess with error details
  4. Abort process after email sent

**Branch 2 (Identifier: 2):**
- **Destination:** shape41 (Map error response)
- **Flow:** shape41 → shape43 (Return Error Response)
- **Purpose:** Return error response to caller (D365)
- **Action:**
  1. Map error message to D365 error response format
  2. Return error response with status="failure", success="false"

**Parallel Execution:**
- Both branches execute in parallel
- Branch 1: Sends error notification email (async)
- Branch 2: Returns error response to caller (sync)
- Process aborts after Branch 1 completes (abort=true in subprocess call)

**Business Logic:**
- When an exception occurs, two actions are needed:
  1. Notify integration team via email (Branch 1)
  2. Return error response to caller (Branch 2)
- Both actions happen in parallel for efficiency

**System Layer Responsibility:**
- ✅ Branch 2 (Error Response) - System Layer responsibility
- ❌ Branch 1 (Email Notification) - NOT System Layer responsibility
  - Email notification is cross-cutting concern
  - System Layer should throw exceptions; middleware handles notifications

**Implementation Note:**
- System Layer will implement Branch 2 (error response)
- System Layer will NOT implement Branch 1 (email notification)
- ExceptionHandlerMiddleware will handle error responses
- Email notifications (if needed) should be handled separately

### Self-Check Questions (MANDATORY)

**Q1: Have I identified ALL branch shapes in the process?**
✅ YES - Identified 1 branch shape: shape20 (Error Handling Branch)

**Q2: For EACH branch, have I documented the number of branches?**
✅ YES - shape20 has 2 branches (parallel execution)

**Q3: For EACH branch, have I documented ALL branch destinations?**
✅ YES - Documented both branch destinations:
- Branch 1: shape19 → shape21 (Email notification)
- Branch 2: shape41 → shape43 (Error response)

**Q4: Have I determined if branches execute in parallel or sequence?**
✅ YES - Branches execute in PARALLEL (both start from shape20 simultaneously)

**Q5: Have I explained the business purpose of each branch?**
✅ YES - Explained business purpose:
- Branch 1: Notify integration team of error
- Branch 2: Return error response to caller

---

## STEP 9: EXECUTION ORDER (BLOCKING)

### Execution Sequence

**Phase 1: Initialization (Sequential)**
```
1. shape1 (Start)
   ↓
2. shape38 (Input_details - Store execution properties)
   - Store: DPP_Process_Name, DPP_AtomName, DPP_Payload, DPP_ExecutionID, DPP_File_Name, DPP_Subject, To_Email, DPP_HasAttachment
   ↓
3. shape17 (Try/Catch Error Handler - Enter Try block)
```

**Phase 2: Main Processing (Sequential - Try Path)**
```
4. shape29 (Map D365 → Oracle HCM)
   - Transform: employeeNumber → personNumber, absenceStatusCode → absenceStatusCd, etc.
   ↓
5. shape8 (Set Dynamic URL)
   - Set: dynamicdocument.URL = Resource_Path
   ↓
6. shape49 (Notify/Log request)
   - Log current document
   ↓
7. shape33 (HTTP POST to Oracle Fusion)
   - Method: POST
   - URL: {BaseURL}/{Resource_Path}
   - Auth: Basic (INTEGRATION.USER@al-ghurair.com)
   - Body: Transformed leave request
   - Response: Oracle HCM response + HTTP status code
```

**Phase 3: Success Path (Sequential - If HTTP 20*)**
```
8a. shape2 (Decision - HTTP Status 20* check)
    - Condition: meta.base.applicationstatuscode matches "20*"
    - Result: TRUE
    ↓
9a. shape34 (Map success response)
    - Extract: personAbsenceEntryId from Oracle response
    - Set defaults: status="success", message="Data successfully sent to Oracle Fusion", success="true"
    ↓
10a. shape35 (Return Success Response)
     - Return to caller with success response
     ↓
END
```

**Phase 3: Error Path - Plain Response (Sequential - If HTTP non-20* and NOT gzip)**
```
8b. shape2 (Decision - HTTP Status 20* check)
    - Condition: meta.base.applicationstatuscode matches "20*"
    - Result: FALSE
    ↓
9b. shape44 (Decision - Content-Encoding gzip check)
    - Condition: dynamicdocument.DDP_RespHeader equals "gzip"
    - Result: FALSE
    ↓
10b. shape39 (Extract error message)
     - Extract: meta.base.applicationstatusmessage
     - Store: DPP_ErrorMessage
     ↓
11b. shape40 (Map error response)
     - Set: status="failure", message=DPP_ErrorMessage, success="false"
     ↓
12b. shape36 (Return Error Response)
     - Return to caller with error response
     ↓
END
```

**Phase 3: Error Path - Gzip Response (Sequential - If HTTP non-20* and gzip)**
```
8c. shape2 (Decision - HTTP Status 20* check)
    - Condition: meta.base.applicationstatuscode matches "20*"
    - Result: FALSE
    ↓
9c. shape44 (Decision - Content-Encoding gzip check)
    - Condition: dynamicdocument.DDP_RespHeader equals "gzip"
    - Result: TRUE
    ↓
10c. shape45 (Decompress gzip response)
     - Decompress: GZIPInputStream
     ↓
11c. shape46 (Extract error message)
     - Extract: Current document (decompressed)
     - Store: DPP_ErrorMessage
     ↓
12c. shape47 (Map error response)
     - Set: status="failure", message=DPP_ErrorMessage, success="false"
     ↓
13c. shape48 (Return Error Response)
     - Return to caller with error response
     ↓
END
```

**Phase 3: Exception Path (Parallel - If Exception in Try block)**
```
8d. shape17 (Try/Catch Error Handler - Enter Catch block)
    - Exception occurred in Try block
    ↓
9d. shape20 (Branch - 2 parallel paths)
    ↓                                    ↓
Branch 1 (Parallel)                  Branch 2 (Parallel)
    ↓                                    ↓
10d-1. shape19 (Store error message) 10d-2. shape41 (Map error response)
       - Extract: meta.base.catcherrorsmessage    - Set: status="failure", message=DPP_ErrorMessage, success="false"
       - Store: DPP_ErrorMessage         ↓
       ↓                              11d-2. shape43 (Return Error Response)
11d-1. shape21 (Call Email Subprocess)            - Return to caller with error response
       - Subprocess: (Sub) Office 365 Email       ↓
       - abort=true, wait=true            END
       ↓
END (Process aborts)
```

### Execution Dependencies

**Sequential Dependencies:**
1. shape38 MUST complete before shape17 (store properties before processing)
2. shape29 MUST complete before shape8 (transform before setting URL)
3. shape8 MUST complete before shape33 (set URL before HTTP call)
4. shape33 MUST complete before shape2 (HTTP call before status check)
5. shape2 MUST complete before shape34/shape44 (status check before response handling)
6. shape44 MUST complete before shape45/shape39 (gzip check before decompression/extraction)

**Parallel Dependencies:**
- shape20 branches (shape19/shape21 and shape41/shape43) execute in PARALLEL
- No dependencies between parallel branches

**Conditional Dependencies:**
- shape34 executes ONLY IF shape2 result is TRUE (HTTP 20*)
- shape44 executes ONLY IF shape2 result is FALSE (HTTP non-20*)
- shape45 executes ONLY IF shape44 result is TRUE (gzip)
- shape39 executes ONLY IF shape44 result is FALSE (plain)
- shape20 executes ONLY IF exception occurs in Try block

### Self-Check Questions (MANDATORY)

**Q1: Have I documented the COMPLETE execution order from start to end?**
✅ YES - Documented complete execution order from shape1 (Start) to all END points (shape35, shape36, shape43, shape48)

**Q2: Have I identified ALL conditional branches and their execution paths?**
✅ YES - Identified all conditional branches:
- shape2: TRUE → shape34/shape35 (Success), FALSE → shape44 (Error)
- shape44: TRUE → shape45/shape46/shape47/shape48 (Gzip), FALSE → shape39/shape40/shape36 (Plain)
- shape17: Try → Normal flow, Catch → shape20 (Exception)

**Q3: Have I identified ALL parallel execution paths?**
✅ YES - Identified parallel execution in shape20:
- Branch 1: shape19 → shape21 (Email notification)
- Branch 2: shape41 → shape43 (Error response)

**Q4: For EACH shape, have I documented what it does and what data it processes?**
✅ YES - Documented action and data for each shape in execution sequence

**Q5: Can I trace the execution path for success, error, and exception scenarios?**
✅ YES - Documented 4 execution paths:
1. Success path (HTTP 20*)
2. Error path - Plain response (HTTP non-20*, NOT gzip)
3. Error path - Gzip response (HTTP non-20*, gzip)
4. Exception path (Try/Catch exception)

---

## STEP 10: SEQUENCE DIAGRAM (BLOCKING)

### Sequence Diagram: HCM Leave Create Process

```
Actor: D365 (Caller)
System: HCM Leave Create Process (Boomi)
SOR: Oracle Fusion HCM

=== SUCCESS SCENARIO ===

D365                    HCM Process                 Oracle Fusion HCM
 |                           |                              |
 |---(1) POST Leave Request->|                              |
 |   {employeeNumber,        |                              |
 |    absenceType,           |                              |
 |    startDate, endDate,    |                              |
 |    ...}                   |                              |
 |                           |                              |
 |                           |---(2) Store Properties----->|
 |                           |    (DPP_Payload,             |
 |                           |     DPP_Process_Name, etc.)  |
 |                           |                              |
 |                           |---(3) Map D365 → Oracle----->|
 |                           |    (employeeNumber →         |
 |                           |     personNumber, etc.)      |
 |                           |                              |
 |                           |---(4) Set Dynamic URL------->|
 |                           |    (Resource_Path)           |
 |                           |                              |
 |                           |---(5) POST /absences-------->|
 |                           |    {personNumber,            |
 |                           |     absenceType,             |
 |                           |     startDate, endDate,      |
 |                           |     absenceStatusCd,         |
 |                           |     approvalStatusCd,        |
 |                           |     startDateDuration,       |
 |                           |     endDateDuration}         |
 |                           |                              |
 |                           |<---(6) HTTP 200 OK-----------|
 |                           |    {personAbsenceEntryId,    |
 |                           |     absenceStatusCd,         |
 |                           |     approvalStatusCd,        |
 |                           |     duration, ...}           |
 |                           |                              |
 |                           |---(7) Check HTTP Status----->|
 |                           |    (20* → TRUE)              |
 |                           |                              |
 |                           |---(8) Map Success Response-->|
 |                           |    (Extract                  |
 |                           |     personAbsenceEntryId)    |
 |                           |                              |
 |<--(9) Success Response----|                              |
 |   {status: "success",     |                              |
 |    message: "Data         |                              |
 |     successfully sent",   |                              |
 |    personAbsenceEntryId,  |                              |
 |    success: "true"}       |                              |
 |                           |                              |

=== ERROR SCENARIO (HTTP 4xx/5xx - Plain Response) ===

D365                    HCM Process                 Oracle Fusion HCM
 |                           |                              |
 |---(1) POST Leave Request->|                              |
 |                           |                              |
 |                           |---(2-4) Same as success----->|
 |                           |                              |
 |                           |---(5) POST /absences-------->|
 |                           |                              |
 |                           |<---(6) HTTP 400 Bad Request--|
 |                           |    {error: "Invalid          |
 |                           |     employee number"}        |
 |                           |                              |
 |                           |---(7) Check HTTP Status----->|
 |                           |    (20* → FALSE)             |
 |                           |                              |
 |                           |---(8) Check Content-Encoding>|
 |                           |    (gzip → FALSE)            |
 |                           |                              |
 |                           |---(9) Extract Error Message->|
 |                           |    (applicationstatusmessage)|
 |                           |                              |
 |                           |---(10) Map Error Response--->|
 |                           |                              |
 |<--(11) Error Response-----|                              |
 |   {status: "failure",     |                              |
 |    message: "Invalid      |                              |
 |     employee number",     |                              |
 |    success: "false"}      |                              |
 |                           |                              |

=== ERROR SCENARIO (HTTP 4xx/5xx - Gzip Response) ===

D365                    HCM Process                 Oracle Fusion HCM
 |                           |                              |
 |---(1) POST Leave Request->|                              |
 |                           |                              |
 |                           |---(2-4) Same as success----->|
 |                           |                              |
 |                           |---(5) POST /absences-------->|
 |                           |                              |
 |                           |<---(6) HTTP 500 Server Error-|
 |                           |    Content-Encoding: gzip    |
 |                           |    [gzip compressed error]   |
 |                           |                              |
 |                           |---(7) Check HTTP Status----->|
 |                           |    (20* → FALSE)             |
 |                           |                              |
 |                           |---(8) Check Content-Encoding>|
 |                           |    (gzip → TRUE)             |
 |                           |                              |
 |                           |---(9) Decompress Gzip------->|
 |                           |                              |
 |                           |---(10) Extract Error Message>|
 |                           |                              |
 |                           |---(11) Map Error Response--->|
 |                           |                              |
 |<--(12) Error Response-----|                              |
 |   {status: "failure",     |                              |
 |    message: "Internal     |                              |
 |     server error",        |                              |
 |    success: "false"}      |                              |
 |                           |                              |

=== EXCEPTION SCENARIO (Network Failure, Timeout, etc.) ===

D365                    HCM Process                 Email Service
 |                           |                              |
 |---(1) POST Leave Request->|                              |
 |                           |                              |
 |                           |---(2-4) Same as success----->|
 |                           |                              |
 |                           |---(5) POST /absences-------->|
 |                           |    [Network Timeout]         |
 |                           |                              |
 |                           |---(6) Exception Caught------>|
 |                           |    (Try/Catch)               |
 |                           |                              |
 |                           |---(7) Branch (Parallel)----->|
 |                           |    ├─ Branch 1: Email        |
 |                           |    └─ Branch 2: Error Resp   |
 |                           |                              |
 |                           |---(8) Store Error Message--->|
 |                           |    (DPP_ErrorMessage)        |
 |                           |                              |
 |                           |---(9) Send Email------------>|
 |                           |    {Subject, Body,           |
 |                           |     Attachment: Payload}     |
 |                           |                              |
 |                           |---(10) Map Error Response--->|
 |                           |                              |
 |<--(11) Error Response-----|                              |
 |   {status: "failure",     |                              |
 |    message: "Network      |                              |
 |     timeout occurred",    |                              |
 |    success: "false"}      |                              |
 |                           |                              |
 |                           |---(12) Process Aborts------->|
 |                           |                              |
```

### Sequence Diagram Notes

**Success Scenario Steps:**
1. D365 sends leave creation request
2. Process stores execution properties (logging, error handling)
3. Process transforms D365 request to Oracle HCM format
4. Process sets dynamic URL from configuration
5. Process sends HTTP POST to Oracle Fusion HCM
6. Oracle returns HTTP 200 with personAbsenceEntryId
7. Process checks HTTP status (20* → TRUE)
8. Process maps Oracle response to D365 format
9. Process returns success response to D365

**Error Scenario Steps (Plain Response):**
1-5. Same as success scenario
6. Oracle returns HTTP 4xx/5xx with error message
7. Process checks HTTP status (20* → FALSE)
8. Process checks Content-Encoding (gzip → FALSE)
9. Process extracts error message from response
10. Process maps error to D365 format
11. Process returns error response to D365

**Error Scenario Steps (Gzip Response):**
1-7. Same as error scenario (plain)
8. Process checks Content-Encoding (gzip → TRUE)
9. Process decompresses gzip response
10. Process extracts error message from decompressed response
11. Process maps error to D365 format
12. Process returns error response to D365

**Exception Scenario Steps:**
1-5. Same as success scenario
6. Exception occurs (network timeout, parsing error, etc.)
7. Process catches exception and branches to parallel paths
8. Process stores error message in DPP_ErrorMessage
9. Process sends error notification email to integration team
10. Process maps error to D365 format
11. Process returns error response to D365
12. Process aborts after email sent

---

## FUNCTION EXPOSURE DECISION TABLE (BLOCKING)

### Decision Process: Which Operations Should Be Azure Functions?

| Operation | Independent Invoke? | Decision Before/After? | Same SOR? | Internal Lookup? | Conclusion | Reasoning |
|-----------|-------------------|----------------------|-----------|-----------------|-----------|-----------|
| Create Leave in Oracle HCM | YES | NO | N/A | NO | **Azure Function** | Process Layer needs to invoke leave creation independently. This is a complete business operation that creates a leave record in Oracle HCM. No decision logic present - straight transformation and HTTP POST. |

### Decision Analysis

**Q1: Can Process Layer invoke independently?**
- ✅ YES - Process Layer needs to create leave records in Oracle HCM independently
- This is a complete business operation that Process Layer would orchestrate
- Result: Proceed to Q2

**Q2: Decision/conditional logic present?**
- ❌ NO - No business decision logic present in the process
- The process performs:
  1. Data transformation (D365 format → Oracle HCM format)
  2. HTTP POST to Oracle HCM
  3. Response handling (success/error)
- No conditional logic like "if employee exists, skip creation" or "if status=X, do Y"
- All decision points are technical (HTTP status check, gzip check, exception handling)
- Result: Proceed to Q3

**Q3: Only field extraction/lookup for another operation?**
- ❌ NO - This is NOT a lookup operation
- This operation CREATES a new leave record in Oracle HCM
- This is a complete business operation, not a helper lookup
- Result: Proceed to Q4

**Q4: Complete business operation Process Layer needs?**
- ✅ YES - This is a complete business operation
- Process Layer needs to create leave records in Oracle HCM
- This operation:
  - Accepts leave data from D365
  - Transforms to Oracle HCM format
  - Creates leave record in Oracle HCM
  - Returns success/error response
- This is a reusable "Lego block" that Process Layer can use
- Result: **Azure Function**

### Verification Questions

**Q1: Identified ALL decision points?**
✅ YES - Identified 3 decision points:
1. shape2: HTTP Status 20* check (TECHNICAL - belongs in System Layer)
2. shape44: Content-Encoding gzip check (TECHNICAL - belongs in System Layer)
3. shape17: Try/Catch exception handling (TECHNICAL - belongs in System Layer)

**Q2: WHERE each decision belongs?**
✅ YES - All decisions belong in System Layer:
- HTTP status checking is SOR-specific technical concern
- Response decompression is SOR-specific technical concern
- Exception handling is System Layer responsibility

**Q3: "if X exists, skip Y" checked?**
✅ YES - No such pattern exists in this process
- No check-before-create logic
- No conditional skip logic
- Straight transformation and HTTP POST

**Q4: "if flag=X, do Y" checked?**
✅ YES - No such pattern exists in this process
- No flag-based conditional logic
- No business rule decisions
- Only technical decisions (HTTP status, gzip, exceptions)

**Q5: Can explain WHY each operation type?**
✅ YES - Explained reasoning:
- **Create Leave** = Azure Function because:
  - Process Layer needs to invoke independently
  - Complete business operation (create leave record)
  - No business decision logic (only technical decisions)
  - Reusable "Lego block" for Process Layer

**Q6: Avoided pattern-matching?**
✅ YES - Decision based on analysis, not pattern-matching
- Analyzed the actual operation logic
- Identified technical vs. business decisions
- Determined Process Layer needs

**Q7: If 1 Function, NO decision shapes?**
✅ YES - Verified:
- 1 Azure Function (Create Leave)
- 3 decision shapes, but ALL are technical (HTTP status, gzip, exceptions)
- NO business decision shapes
- All decisions belong in System Layer Handler

### Summary

**I will create 1 Azure Function for Oracle Fusion HCM:**

**Function Name:** `CreateLeave`

**Purpose:** Create absence/leave record in Oracle Fusion HCM

**Reasoning:**
- Process Layer needs to invoke leave creation independently
- This is a complete business operation that creates a leave record
- No business decision logic present (only technical decisions)
- All decision points are technical concerns:
  - HTTP status checking (SOR-specific)
  - Response decompression (SOR-specific)
  - Exception handling (System Layer responsibility)

**Per Rule 1066:**
- Business decisions → Process Layer when operations span multiple SORs
- This process has NO cross-SOR business decisions
- All decisions are same-SOR technical concerns (Oracle HCM HTTP status, response format)
- Handler orchestrates internal Atomic Handlers with simple if/else for technical decisions

**Function Responsibilities:**
- Accept leave creation request from Process Layer
- Transform D365 format to Oracle HCM format
- Call Oracle HCM REST API to create leave record
- Handle HTTP status codes and response formats (gzip decompression)
- Return success response with personAbsenceEntryId or error response

**Internal Components (NOT exposed as Functions):**
- **CreateLeaveAtomicHandler:** Single HTTP POST to Oracle HCM /absences endpoint
- **Handler:** Orchestrates transformation, HTTP call, response handling
- **Service:** Abstraction boundary that delegates to Handler

**Authentication:**
- Basic Authentication (username/password from configuration)
- No session/token-based auth → NO middleware needed
- Credentials added directly in Atomic Handler HTTP headers

---

## SELF-CHECK VALIDATION (MANDATORY)

### Phase 1 Completion Checklist

**Document Structure:**
- ✅ Operations Inventory - Complete
- ✅ Step 1a: Input Structure Analysis - Complete
- ✅ Step 1b: Response Structure Analysis - Complete
- ✅ Step 1c: Operation Response Analysis - Complete
- ✅ Step 1d: Map Analysis - Complete
- ✅ Step 1e: HTTP Status Codes and Return Paths - Complete
- ✅ Steps 2-3: Process Properties Analysis - Complete
- ✅ Step 4: Data Dependency Graph - Complete
- ✅ Step 5: Control Flow Graph - Complete
- ✅ Step 6: Reverse Flow Mapping - Complete
- ✅ Step 7: Decision Shape Analysis - Complete (with self-checks)
- ✅ Step 7a: Subprocess Analysis - Complete
- ✅ Step 8: Branch Shape Analysis - Complete (with self-checks)
- ✅ Step 9: Execution Order - Complete (with self-checks)
- ✅ Step 10: Sequence Diagram - Complete
- ✅ Function Exposure Decision Table - Complete

**Self-Check Questions:**

**Q1: Have I analyzed ALL JSON files provided?**
✅ YES - Analyzed all 18 JSON files:
- process_root_ca69f858-785f-4565-ba1f-b4edc6cca05b.json
- component_0feff13f-8a2c-438a-b3c7-1909e2a7f533.json (PP_Office365_Email)
- component_e22c04db-7dfa-4b1b-9d88-77365b0fdb18.json (PP_HCM_LeaveCreate_Properties)
- connection_00eae79b-2303-4215-8067-dcc299e42697.json (Office 365 Email)
- connection_aa1fcb29-d146-4425-9ea6-b9698090f60e.json (Oracle Fusion)
- operation_8f709c2b-e63f-4d5f-9374-2932ed70415d.json (Create Leave Oracle Fusion OP)
- operation_6e8920fd-af5a-430b-a1d9-9fde7ac29a12.json (Leave Oracle Fusion Create)
- operation_15a72a21-9b57-49a1-a8ed-d70367146644.json (Email W/O Attachment)
- operation_af07502a-fafd-4976-a691-45d51a33b549.json (Email w Attachment)
- subprocess_a85945c5-3004-42b9-80b1-104f465cd1fb.json ((Sub) Office 365 Email)
- map_c426b4d6-2aff-450e-b43b-59956c4dbc96.json (Leave Create Map)
- map_e4fd3f59-edb5-43a1-aeae-143b600a064e.json (Oracle Fusion Leave Response Map)
- map_f46b845a-7d75-41b5-b0ad-c41a6a8e9b12.json (Leave Error Map)
- profile_febfa3e1-f719-4ee8-ba57-cdae34137ab3.json (D365 Leave Create JSON Profile)
- profile_a94fa205-c740-40a5-9fda-3d018611135a.json (HCM Leave Create JSON Profile)
- profile_316175c7-0e45-4869-9ac6-5f9d69882a62.json (Oracle Fusion Leave Response JSON Profile)
- profile_f4ca3a70-114a-4601-bad8-44a3eb20e2c0.json (Leave D365 Response)
- profile_23d7a2e9-5cb0-4e9c-9e4b-3154834bad0d.json (Dummy FF Profile)

**Q2: Have I completed ALL mandatory extraction steps (Steps 1a-1e, 2-10)?**
✅ YES - Completed all mandatory steps:
- Step 1a: Input Structure Analysis ✅
- Step 1b: Response Structure Analysis ✅
- Step 1c: Operation Response Analysis ✅
- Step 1d: Map Analysis ✅
- Step 1e: HTTP Status Codes and Return Paths ✅
- Steps 2-3: Process Properties Analysis ✅
- Step 4: Data Dependency Graph ✅
- Step 5: Control Flow Graph ✅
- Step 6: Reverse Flow Mapping ✅
- Step 7: Decision Shape Analysis ✅
- Step 7a: Subprocess Analysis ✅
- Step 8: Branch Shape Analysis ✅
- Step 9: Execution Order ✅
- Step 10: Sequence Diagram ✅

**Q3: Have I answered ALL self-check questions in Steps 7, 8, and 9?**
✅ YES - Answered all self-check questions:
- Step 7 (Decision Shape Analysis): 5 questions answered with YES
- Step 8 (Branch Shape Analysis): 5 questions answered with YES
- Step 9 (Execution Order): 5 questions answered with YES

**Q4: Have I completed the Function Exposure Decision Table?**
✅ YES - Completed decision table:
- Identified 1 operation: Create Leave in Oracle HCM
- Answered all 4 decision questions (Q1-Q4)
- Answered all 7 verification questions
- Provided summary with reasoning

**Q5: Have I identified the System of Record (SOR)?**
✅ YES - SOR: Oracle Fusion HCM
- Connection: aa1fcb29-d146-4425-9ea6-b9698090f60e
- Base URL: https://iaaxey-dev3.fa.ocs.oraclecloud.com:443
- Resource Path: hcmRestApi/resources/11.13.18.05/absences

**Q6: Have I identified which operations belong in System Layer vs. Process Layer?**
✅ YES - Identified:
- **System Layer:** Create Leave in Oracle HCM (1 Azure Function)
- **Process Layer:** Business orchestration (not implemented in this phase)
- **NOT System Layer:** Email notifications (cross-cutting concern)

**Q7: Have I documented field names for SOAP/HTTP envelopes?**
✅ YES - Documented in Step 1d (Map Analysis):
- Request fields: personNumber, absenceType, employer, startDate, endDate, absenceStatusCd, approvalStatusCd, startDateDuration, endDateDuration
- Response fields: personAbsenceEntryId, absenceStatusCd, approvalStatusCd, duration
- D365 response fields: status, message, personAbsenceEntryId, success

**Q8: Have I created sequence diagrams for ALL scenarios?**
✅ YES - Created sequence diagrams for:
1. Success scenario (HTTP 20*)
2. Error scenario - Plain response (HTTP non-20*, NOT gzip)
3. Error scenario - Gzip response (HTTP non-20*, gzip)
4. Exception scenario (Network failure, timeout, etc.)

**Q9: Can I proceed to Phase 2 (Code Generation)?**
✅ YES - Phase 1 is complete:
- All mandatory sections present
- All self-check questions answered with YES
- Function Exposure Decision Table complete
- Ready to generate System Layer code

---

## PHASE 1 COMPLETION SUMMARY

**Status:** ✅ COMPLETE

**System of Record:** Oracle Fusion HCM

**Azure Functions to Create:** 1
- `CreateLeave` - Create absence/leave record in Oracle Fusion HCM

**Key Findings:**
1. Single external API call to Oracle Fusion HCM (POST /absences)
2. No business decision logic (only technical decisions)
3. No authentication middleware needed (Basic Auth per-request)
4. Handler orchestrates internal Atomic Handlers with simple if/else for technical decisions
5. Email notifications are cross-cutting concerns (NOT System Layer)

**Next Steps:**
- Proceed to Phase 2: Code Generation
- Create System Layer project: sys-oraclefusionhcm-mgmt
- Implement CreateLeave Azure Function with Handler and Atomic Handler
- Follow System-Layer-Rules.mdc for architecture and file placement

**Ready for Phase 2:** ✅ YES
