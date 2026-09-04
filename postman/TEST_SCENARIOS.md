# Test Scenarios - Credit Risk Compliance System

Documentação detalhada de todos os cenários de teste na coleção Postman.

## 📋 Índice

1. [Setup & Health Checks](#1-setup--health-checks)
2. [Authentication Flow](#2-authentication-flow)
3. [Credit Analysis Flow](#3-credit-analysis-flow)
4. [Compliance Flow](#4-compliance-flow)
5. [Bureau Mock Tests](#5-bureau-mock-tests)
6. [Error Cases](#6-error-cases)
7. [Variable Dependencies](#7-variable-dependencies)

---

## 1. Setup & Health Checks

### 1.1 Health - Bureau Mock (8081)

**Purpose:** Verify Bureau Mock service is running and healthy

**Request:**
```http
GET http://localhost:8081/health
```

**Expected Response (200):**
```json
{
  "status": "healthy"
}
```

**Test Assertions:**
- ✅ Status code is 200
- ✅ Response contains "healthy" status

**Prerequisites:** Bureau Mock service must be running

**Next Step:** Proceed to IAM health check

---

### 1.2 Health - IAM (5000)

**Purpose:** Verify IAM API service is running

**Request:**
```http
GET http://localhost:5000/health
```

**Expected Response (200):**
```
Healthy
```

**Test Assertions:**
- ✅ Status code is 200
- ✅ Response contains "Healthy"

**Prerequisites:** IAM service must be running

---

### 1.3 Health - Credit Analysis (5001)

**Purpose:** Verify Credit Analysis API is running

**Request:**
```http
GET http://localhost:5001/health
```

**Expected Response (200):**
```
Healthy
```

**Test Assertions:**
- ✅ Status code is 200
- ✅ Response contains "Healthy"

---

### 1.4 Health - Compliance (5002)

**Purpose:** Verify Compliance API is running

**Request:**
```http
GET http://localhost:5002/health
```

**Expected Response (200):**
```
Healthy
```

---

### 1.5 Health - Operations Server (5003)

**Purpose:** Verify Operations Server is running

**Request:**
```http
GET http://localhost:5003/health
```

**Expected Response (200):**
```
Healthy
```

**Success Criteria:** All 5 health checks return 200

---

## 2. Authentication Flow

### 2.1 Create User

**Purpose:** Create a new user account for subsequent tests

**Request:**
```http
POST http://localhost:5000/api/v1/users
Content-Type: application/json

{
  "username": "test_operator_2026-08-27T09:41:59Z",
  "email": "test_operator_2026-08-27T09:41:59Z@example.com",
  "password": "SecurePass123!",
  "fullName": "Test Operator",
  "roles": ["desk-operator"]
}
```

**Pre-request Script:**
- Generates unique timestamp
- Sets `newUserId` variable with UUID

**Expected Response (200/201):**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "username": "test_operator_...",
  "email": "test_operator_...@example.com"
}
```

**Test Assertions:**
- ✅ Status code is 200 or 201
- ✅ Response contains `id` or `userId`
- ✅ Email matches request

**Variables Set:**
- `userId` - extracted from response
- `loginEmail` - from request body
- `loginPassword` - "SecurePass123!"

**Next Step:** Login with created credentials

---

### 2.2 Login

**Purpose:** Authenticate user and obtain JWT token

**Request:**
```http
POST http://localhost:5000/api/v1/auth/login
Content-Type: application/json

{
  "email": "test_operator_2026-08-27T09:41:59Z@example.com",
  "password": "SecurePass123!"
}
```

**Expected Response (200):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresIn": 3600
}
```

**Test Assertions:**
- ✅ Status code is 200
- ✅ Response contains `token` or `accessToken`
- ✅ Token is a string

**Variables Set:**
- `bearerToken` - extracted from response

**Usage:** Token used in subsequent authenticated requests

**Next Step:** Get user details with token

---

### 2.3 Get User

**Purpose:** Verify user data using JWT token

**Request:**
```http
GET http://localhost:5000/api/v1/users/550e8400-e29b-41d4-a716-446655440000
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Expected Response (200):**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "username": "test_operator_...",
  "email": "test_operator_...@example.com",
  "fullName": "Test Operator",
  "roles": ["desk-operator"]
}
```

**Test Assertions:**
- ✅ Status code is 200
- ✅ Response contains user data
- ✅ User ID matches created user

**Dependencies:**
- Requires `userId` variable
- Requires `bearerToken` variable

**Next Step:** Logout to invalidate token

---

### 2.4 Logout

**Purpose:** Invalidate JWT token

**Request:**
```http
POST http://localhost:5000/api/v1/auth/logout
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Expected Response (200):**
```json
{
  "message": "Logout successful"
}
```

**Test Assertions:**
- ✅ Status code is 200

**Note:** Token becomes invalid after logout

**Success Criteria:** Authentication flow completed successfully

---

## 3. Credit Analysis Flow

### 3.1 Create Proposal

**Purpose:** Create a new credit proposal for analysis

**Request:**
```http
POST http://localhost:5001/api/v1/proposals
Content-Type: application/json

{
  "customerDocument": "12345678901",
  "customerDocumentType": "CPF",
  "customerName": "João Silva",
  "customerEmail": "joao@example.com",
  "monthlyIncome": 10000.00,
  "requestedLimit": 50000.00,
  "proposalType": "Individual",
  "bureauConsentGiven": true,
  "bureauConsentIpAddress": "192.168.1.1"
}
```

**Pre-request Script:**
- Generates unique customer ID
- Sets document to "12345678901"

**Expected Response (202 Accepted):**
```json
{
  "proposalId": "660e8400-e29b-41d4-a716-446655440001",
  "status": "Received",
  "requestedLimit": 50000.00,
  "createdAt": "2026-08-27T09:41:59Z"
}
```

**Test Assertions:**
- ✅ Status code is 202
- ✅ Response contains `proposalId`
- ✅ Status is "Received"

**Variables Set:**
- `proposalId` - extracted from response

**Processing:**
- Proposal queued for background processing
- Bureau query initiated asynchronously
- Status will change to "Evaluated" after bureau response

**Next Step:** Check proposal status with Get Proposal

---

### 3.2 Get Proposal

**Purpose:** Retrieve proposal details and current status

**Request:**
```http
GET http://localhost:5001/api/v1/proposals/660e8400-e29b-41d4-a716-446655440001
```

**Expected Response (200):**
```json
{
  "id": "660e8400-e29b-41d4-a716-446655440001",
  "status": "Evaluated",
  "requestedLimit": 50000.00,
  "approvedLimit": 45000.00,
  "riskRating": "B",
  "requiresManualReview": false,
  "createdAt": "2026-08-27T09:41:59Z",
  "evaluatedAt": "2026-08-27T09:42:05Z"
}
```

**Possible Status Values:**
- `Received` - Initial status after creation
- `Evaluated` - Bureau query completed, scoring applied
- `Approved` - Loan approved by system
- `Rejected` - Loan rejected (high risk)

**Risk Ratings (based on bureau score):**
- `A` - Score ≥ 750 (Excellent)
- `B` - Score ≥ 650 (Good)
- `C` - Score ≥ 500 (Fair)
- `D` - Score ≥ 350 (Poor)
- `E` - Score < 350 (Very Poor)

**Test Assertions:**
- ✅ Status code is 200
- ✅ Response contains proposal ID matching request
- ✅ Status is valid (one of: Received, Evaluated, Approved, Rejected)

**Dependencies:**
- Requires `proposalId` variable

**Note:** Status may still be "Received" if background processing hasn't completed
- Wait 2-3 seconds and retry if needed

**Next Step:** Submit proposal for evaluation (optional)

---

### 3.3 Submit Proposal

**Purpose:** Explicitly submit proposal for credit evaluation

**Request:**
```http
PUT http://localhost:5001/api/v1/proposals/660e8400-e29b-41d4-a716-446655440001/submit
Content-Type: application/json
```

**Expected Response (200):**
```json
{
  "id": "660e8400-e29b-41d4-a716-446655440001",
  "status": "Evaluated",
  "requestedLimit": 50000.00,
  "approvedLimit": 45000.00,
  "riskRating": "B"
}
```

**Test Assertions:**
- ✅ Status code is 200
- ✅ Status changed to "Evaluated", "Approved", or "Rejected"

**Note:** May fail if proposal already submitted

**Success Criteria:** Credit Analysis flow completed

---

## 4. Compliance Flow

### 4.1 Ingest Transaction (Normal)

**Purpose:** Ingest a normal transaction for AML screening

**Request:**
```http
POST http://localhost:5002/api/v1/transactions
Content-Type: application/json

{
  "customerId": "550e8400-e29b-41d4-a716-446655440000",
  "customerDocument": "12345678901",
  "amount": 5000.00,
  "transactionType": "Transfer",
  "channel": "Online",
  "transactionDate": "2026-08-27T09:41:59Z"
}
```

**Pre-request Script:**
- Generates unique customer ID
- Uses current ISO timestamp

**Expected Response (201 Created):**
```json
{
  "id": "770e8400-e29b-41d4-a716-446655440002",
  "customerId": "550e8400-e29b-41d4-a716-446655440000",
  "amount": 5000.00,
  "status": "Received",
  "createdAt": "2026-08-27T09:41:59Z"
}
```

**AML Rules Evaluated:**
- ✅ Large Transaction: 5000 < 100000 → PASS
- ✅ Structuring: Single transaction → PASS
- ✅ PEP Screening: Document doesn't start with "000" → PASS

**Expected Result:** No alerts generated

**Variables Set:**
- `transactionId` - extracted from response

**Next Step:** Ingest large transaction to trigger alert

---

### 4.2 Ingest Transaction (Large - Flagged)

**Purpose:** Ingest a large transaction (>100k BRL) to trigger LargeTransaction alert

**Request:**
```http
POST http://localhost:5002/api/v1/transactions
Content-Type: application/json

{
  "customerId": "550e8400-e29b-41d4-a716-446655440001",
  "customerDocument": "98765432101",
  "amount": 150000.00,
  "transactionType": "Transfer",
  "channel": "Online",
  "transactionDate": "2026-08-27T09:41:59Z"
}
```

**Expected Response (201 Created):**
```json
{
  "id": "770e8400-e29b-41d4-a716-446655440003",
  "customerId": "550e8400-e29b-41d4-a716-446655440001",
  "amount": 150000.00,
  "status": "Received",
  "createdAt": "2026-08-27T09:41:59Z"
}
```

**AML Rules Evaluated:**
- ❌ Large Transaction: 150000 > 100000 → **ALERT TRIGGERED**
- Alert Type: `LargeTransaction`
- Severity: `High`

**Expected Alert:**
```json
{
  "id": "alert-123",
  "transactionId": "770e8400-e29b-41d4-a716-446655440003",
  "type": "LargeTransaction",
  "severity": "High",
  "status": "Pending"
}
```

**Variables Set:**
- `largeTransactionId` - extracted from response

**Background Processing:**
- Outbox service detects new transaction
- AML rules engine evaluates (5s poll cycle)
- Alert created and broadcast via SignalR
- Compliance team notified

**Next Step:** Check transaction details and list alerts

---

### 4.3 Get Transaction

**Purpose:** Retrieve transaction details including any AML flags

**Request:**
```http
GET http://localhost:5002/api/v1/transactions/770e8400-e29b-41d4-a716-446655440002
```

**Expected Response (200):**
```json
{
  "id": "770e8400-e29b-41d4-a716-446655440002",
  "customerId": "550e8400-e29b-41d4-a716-446655440000",
  "amount": 5000.00,
  "transactionType": "Transfer",
  "channel": "Online",
  "status": "Processed",
  "flags": [],
  "createdAt": "2026-08-27T09:41:59Z"
}
```

**Response Fields:**
- `id` - Transaction ID
- `customerId` - Customer UUID
- `amount` - Transaction amount in BRL
- `status` - Processed, Flagged, etc.
- `flags` - Array of applicable flags (e.g., ["LargeTransaction"])

**Test Assertions:**
- ✅ Status code is 200
- ✅ Response contains transaction data
- ✅ Amount matches request

**Dependencies:**
- Requires `transactionId` variable

**Next Step:** List all transactions

---

### 4.4 List Transactions

**Purpose:** List all transactions with pagination

**Request:**
```http
GET http://localhost:5002/api/v1/transactions?page=1&pageSize=10
```

**Expected Response (200):**
```json
{
  "items": [
    {
      "id": "770e8400-e29b-41d4-a716-446655440002",
      "customerId": "550e8400-e29b-41d4-a716-446655440000",
      "amount": 5000.00,
      "status": "Processed"
    },
    {
      "id": "770e8400-e29b-41d4-a716-446655440003",
      "customerId": "550e8400-e29b-41d4-a716-446655440001",
      "amount": 150000.00,
      "status": "Flagged"
    }
  ],
  "total": 2,
  "page": 1,
  "pageSize": 10
}
```

**Query Parameters:**
- `page` - Page number (1-based)
- `pageSize` - Items per page (1-100)
- `customerId` (optional) - Filter by customer
- `status` (optional) - Filter by status

**Test Assertions:**
- ✅ Status code is 200
- ✅ Response contains array of transactions
- ✅ Pagination info included

**Next Step:** List AML alerts

---

### 4.5 List Alerts

**Purpose:** List all AML alerts generated from transactions

**Request:**
```http
GET http://localhost:5002/api/v1/alerts?page=1&pageSize=10
```

**Expected Response (200):**
```json
{
  "items": [
    {
      "id": "880e8400-e29b-41d4-a716-446655440000",
      "transactionId": "770e8400-e29b-41d4-a716-446655440003",
      "type": "LargeTransaction",
      "severity": "High",
      "status": "Pending",
      "description": "Transaction amount 150000 exceeds limit of 100000",
      "createdAt": "2026-08-27T09:41:59Z"
    }
  ],
  "total": 1,
  "page": 1,
  "pageSize": 10
}
```

**Alert Types:**
- `LargeTransaction` - Amount > 100,000 BRL (Severity: High)
- `Structuring` - 5+ transactions < 10k in 7 days (Severity: Medium)
- `UnusualFrequency` - 15+ transactions in 24h (Severity: Medium)
- `PepScreening` - Customer document matches PEP list (Severity: High)

**Alert Statuses:**
- `Pending` - Awaiting review
- `Resolved` - Compliance team reviewed
- `Rejected` - False positive marked

**Test Assertions:**
- ✅ Status code is 200
- ✅ Response contains array of alerts
- ✅ Alerts include transaction references

**Variables Set (via test script):**
- `alertId` - First alert's ID (if available)

**Next Step:** Review alert

---

### 4.6 Review Alert

**Purpose:** Review and update AML alert status

**Request:**
```http
PUT http://localhost:5002/api/v1/alerts/880e8400-e29b-41d4-a716-446655440000/review
Content-Type: application/json
Authorization: Bearer {{bearerToken}}

{
  "newStatus": "Resolved"
}
```

**Request Body Options:**
```json
{
  "newStatus": "Resolved"  // Compliance team reviewed and resolved
}
```

**Expected Response (200):**
```json
{
  "id": "880e8400-e29b-41d4-a716-446655440000",
  "transactionId": "770e8400-e29b-41d4-a716-446655440003",
  "type": "LargeTransaction",
  "severity": "High",
  "status": "Resolved",
  "reviewedAt": "2026-08-27T09:42:10Z",
  "reviewedBy": "test_operator"
}
```

**Error Cases:**
- `404 Not Found` - Alert ID doesn't exist
- `400 Bad Request` - Invalid status value
- `401 Unauthorized` - No authentication token

**Test Assertions:**
- ✅ Status code is 200 or 404
- ✅ Status updated to "Resolved"

**Success Criteria:** Compliance flow completed

---

## 5. Bureau Mock Tests

### 5.1 Query Bureau (Success)

**Purpose:** Query Bureau Mock service for credit information

**Request:**
```http
POST http://localhost:8081/query
Content-Type: application/json

{
  "document": "12345678901"
}
```

**Expected Response (200):**
```json
{
  "document": "12345678901",
  "score": 750,
  "rating": "B",
  "inquiries": 2,
  "defaults": 0
}
```

**Score Range:**
- 750-1000: Excellent (A)
- 650-749: Good (B)
- 500-649: Fair (C)
- 350-499: Poor (D)
- 0-349: Very Poor (E)

**Test Assertions:**
- ✅ Status code is 200
- ✅ Response contains score (0-1000)
- ✅ Response contains rating

**Integration:** Used during Credit Proposal evaluation

**Next Step:** Test error handling

---

### 5.2 Query Bureau Error (500)

**Purpose:** Test error handling when Bureau service returns 500

**Request:**
```http
POST http://localhost:8081/query/error/500
Content-Type: application/json
```

**Expected Response (500):**
```json
{
  "error": "Internal Server Error",
  "statusCode": 500
}
```

**Test Assertions:**
- ✅ Status code is 500

**Graceful Degradation:**
When Bureau Mock returns 500:
1. Credit Analysis API catches the error
2. Default score applied (no bureau data)
3. Proposal still processed with conservative estimate
4. Logging recorded for monitoring

**Next Step:** Test 503 Unavailable

---

### 5.3 Query Bureau Error (503)

**Purpose:** Test error handling when Bureau service is unavailable

**Request:**
```http
POST http://localhost:8081/query/error/503
Content-Type: application/json
```

**Expected Response (503):**
```json
{
  "error": "Service Unavailable",
  "statusCode": 503
}
```

**Test Assertions:**
- ✅ Status code is 503

**Graceful Degradation:**
When Bureau service unavailable:
1. Retry logic applied (exponential backoff)
2. After max retries, fallback to conservative scoring
3. Proposal processed with risk mitigation
4. Alert sent to operations team

**Success Criteria:** Bureau Mock tests completed

---

## 6. Error Cases

### 6.1 Create Proposal - Invalid Document

**Request:**
```http
POST http://localhost:5001/api/v1/proposals
Content-Type: application/json

{
  "customerDocument": "invalid",
  "customerDocumentType": "CPF"
}
```

**Expected Response (400 Bad Request):**
```json
{
  "error": "Invalid CPF format"
}
```

### 6.2 Get Non-existent Proposal

**Request:**
```http
GET http://localhost:5001/api/v1/proposals/00000000-0000-0000-0000-000000000000
```

**Expected Response (404 Not Found):**
```json
{
  "error": "Proposal not found"
}
```

### 6.3 Login - Invalid Credentials

**Request:**
```http
POST http://localhost:5000/api/v1/auth/login
Content-Type: application/json

{
  "email": "nonexistent@example.com",
  "password": "wrongpassword"
}
```

**Expected Response (401 Unauthorized):**
```json
{
  "error": "Invalid credentials"
}
```

### 6.4 Access Without Token

**Request:**
```http
GET http://localhost:5000/api/v1/users/550e8400-e29b-41d4-a716-446655440000
```

**Expected Response (401 Unauthorized):**
```json
{
  "error": "Unauthorized"
}
```

---

## 7. Variable Dependencies

### Dependency Graph

```
Setup & Health Checks (no dependencies)
        ↓
Authentication
├─ timestamp → used by all flows
├─ userId
├─ bearerToken → required by Get User, Review Alert
└─ loginEmail, loginPassword
        ↓
Credit Analysis
├─ proposalCustomerId
├─ proposalDocument
└─ proposalId
        ↓
Compliance
├─ transactionCustomerId
├─ transactionDocument
├─ transactionId
├─ largeTransactionId
└─ alertId
        ↓
Bureau Mock
└─ (no variable dependencies)
```

### Variable Extraction Points

| Variable | Extracted From | Used By |
|----------|----------------|---------|
| `timestamp` | Create User (pre-request) | Login, all flows |
| `userId` | Create User (test script) | Get User |
| `bearerToken` | Login (test script) | Get User, Review Alert |
| `proposalId` | Create Proposal (test script) | Get Proposal, Submit Proposal |
| `transactionId` | Ingest Transaction (test script) | Get Transaction |
| `alertId` | List Alerts (test script) | Review Alert |

### Important Notes

1. **Sequential Execution Required:** Run folders in order for variable dependencies to work
2. **Variable Reset:** Environment variables persist across runs; clear them before running again
3. **Token Expiration:** JWT tokens may expire; re-run Login to refresh
4. **Database State:** Some tests modify database state; may need cleanup between runs

---

## 🎯 Test Execution Checklist

- [ ] All 5 health checks pass
- [ ] User created successfully
- [ ] Login returns token
- [ ] Get User uses token correctly
- [ ] Proposal created with valid ID
- [ ] Proposal status is Received/Evaluated
- [ ] Transaction ingested successfully
- [ ] Large transaction creates alert
- [ ] Alert list shows generated alerts
- [ ] Alert can be reviewed and resolved
- [ ] Bureau Mock responds to query
- [ ] Bureau Mock error cases handled

---

## 📊 Expected Test Summary

**Total Requests:** 25  
**Total Assertions:** 60+  
**Estimated Runtime:** 10-15 seconds

**Pass Rate Target:** 100%

---

**Last Updated:** 2026-08-27  
**Collection Version:** 1.0.0
