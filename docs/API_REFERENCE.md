# API Reference - Credit Risk Compliance System

## Credit Analysis API (Port 5001)

### Credit Proposals

#### Create Proposal
```http
POST /api/credit-analysis/proposals
Content-Type: application/json

{
  "customerId": "550e8400-e29b-41d4-a716-446655440000",
  "customerDocument": "12345678901",
  "customerDocumentType": "CPF",
  "requestedLimit": 50000.00,
  "proposalType": "Individual",
  "createdBy": "user@example.com"
}

Response 201 Created:
{
  "proposalId": "660e8400-e29b-41d4-a716-446655440001",
  "status": "Received",
  "requestedLimit": 50000.00,
  "createdAt": "2026-08-26T15:53:06Z"
}
```

#### Get Proposal
```http
GET /api/credit-analysis/proposals/{proposalId}

Response 200 OK:
{
  "proposalId": "660e8400-e29b-41d4-a716-446655440001",
  "customerId": "550e8400-e29b-41d4-a716-446655440000",
  "status": "Evaluated",
  "requestedLimit": 50000.00,
  "approvedLimit": 45000.00,
  "riskRating": "B",
  "requiresManualReview": false,
  "createdAt": "2026-08-26T15:53:06Z",
  "evaluatedAt": "2026-08-26T15:53:15Z"
}
```

#### List Proposals
```http
GET /api/credit-analysis/proposals?status=Evaluated&page=1&pageSize=20

Response 200 OK:
{
  "proposals": [
    { ... proposal object ... }
  ],
  "total": 150,
  "page": 1,
  "pageSize": 20
}
```

### Credit Scoring

#### Get Scoring Result
```http
GET /api/credit-analysis/proposals/{proposalId}/scoring

Response 200 OK:
{
  "proposalId": "660e8400-e29b-41d4-a716-446655440001",
  "bureauScore": 750,
  "riskRating": "B",
  "approvedLimit": 45000.00,
  "requiresManualReview": false,
  "scoringDetails": {
    "monthlyIncome": 10000.00,
    "debtRatio": 0.35,
    "scoreThresholds": {
      "ratingA": ">=750",
      "ratingB": ">=650",
      "ratingC": ">=500",
      "ratingD": ">=350",
      "ratingE": "<350"
    }
  }
}
```

---

## Compliance API (Port 5002)

### Transactions

#### Ingest Transaction
```http
POST /api/compliance/transactions
Content-Type: application/json

{
  "customerId": "550e8400-e29b-41d4-a716-446655440000",
  "customerDocument": "12345678901",
  "amount": 5000.00,
  "transactionType": "Transfer",
  "channel": "Online",
  "transactionDate": "2026-08-26T15:50:00Z"
}

Response 201 Created:
{
  "transactionId": "770e8400-e29b-41d4-a716-446655440002",
  "customerId": "550e8400-e29b-41d4-a716-446655440000",
  "amount": 5000.00,
  "status": "Received",
  "createdAt": "2026-08-26T15:53:06Z"
}
```

#### Get Transaction
```http
GET /api/compliance/transactions/{transactionId}

Response 200 OK:
{
  "transactionId": "770e8400-e29b-41d4-a716-446655440002",
  "customerId": "550e8400-e29b-41d4-a716-446655440000",
  "amount": 5000.00,
  "transactionType": "Transfer",
  "channel": "Online",
  "status": "Flagged",
  "flags": ["PepMatch"],
  "createdAt": "2026-08-26T15:53:06Z"
}
```

#### List Transactions
```http
GET /api/compliance/transactions?customerId=550e8400-e29b-41d4-a716-446655440000&page=1

Response 200 OK:
{
  "transactions": [
    { ... transaction object ... }
  ],
  "total": 25,
  "page": 1,
  "pageSize": 20
}
```

### Alerts

#### List Alerts
```http
GET /api/compliance/alerts?status=Open&severity=High&page=1

Response 200 OK:
{
  "alerts": [
    {
      "alertId": "880e8400-e29b-41d4-a716-446655440003",
      "transactionId": "770e8400-e29b-41d4-a716-446655440002",
      "customerId": "550e8400-e29b-41d4-a716-446655440000",
      "alertType": "PepMatch",
      "severity": "High",
      "transactionAmount": 5000.00,
      "status": "Open",
      "createdAt": "2026-08-26T15:53:10Z"
    }
  ],
  "total": 42,
  "page": 1,
  "pageSize": 20
}
```

#### Get Alert Details
```http
GET /api/compliance/alerts/{alertId}

Response 200 OK:
{
  "alertId": "880e8400-e29b-41d4-a716-446655440003",
  "transactionId": "770e8400-e29b-41d4-a716-446655440002",
  "customerId": "550e8400-e29b-41d4-a716-446655440000",
  "alertType": "PepMatch",
  "severity": "High",
  "transactionAmount": 5000.00,
  "status": "Open",
  "reviewedBy": null,
  "createdAt": "2026-08-26T15:53:10Z"
}
```

#### Review Alert
```http
PUT /api/compliance/alerts/{alertId}/review
Content-Type: application/json

{
  "status": "Resolved",
  "notes": "Customer verified, not actually PEP"
}

Response 200 OK:
{
  "alertId": "880e8400-e29b-41d4-a716-446655440003",
  "status": "Resolved",
  "reviewedBy": "officer@example.com",
  "reviewedAt": "2026-08-26T15:55:00Z"
}
```

---

## Operations Server - SignalR Hub

### Hub Connection
```
URL: ws://localhost:5003/hub/operations
Protocol: WebSocket with SignalR
Authentication: Bearer token (JWT)
```

### Available Channels

#### ReceiveAmlAlert
Broadcasts when AML alert is created (Low, Medium, High severity).

```javascript
connection.on("ReceiveAmlAlert", (alert) => {
  console.log(`Alert: ${alert.alertType} - ${alert.severity}`);
  // alert: AmlAlertNotification
  // {
  //   alertId: string (UUID)
  //   transactionId: string (UUID)
  //   customerId: string (UUID)
  //   alertType: string ("PepMatch", "Structuring", "LargeTransaction")
  //   severity: string ("Low", "Medium", "High")
  //   transactionAmount: number
  //   createdAt: string (ISO 8601)
  //   description: string
  // }
});
```

#### ReceiveUrgentAlert
Broadcasts when CRITICAL severity alert is created (immediate escalation).

```javascript
connection.on("ReceiveUrgentAlert", (alert) => {
  console.warn(`URGENT: ${alert.alertType}`);
  playAlertSound();
  highlightAlert(alert);
});
```

#### ReceiveTransactionFlagged
Broadcasts when transaction is flagged (PEP match or high AML alert).

```javascript
connection.on("ReceiveTransactionFlagged", (notification) => {
  // notification: TransactionFlaggedNotification
  // {
  //   transactionId: string (UUID)
  //   customerId: string (UUID)
  //   reason: string
  //   alertTypes: string[] (["PepMatch", "Structuring"])
  //   flaggedAt: string (ISO 8601)
  // }
  blockTransaction(notification.transactionId);
});
```

#### ReceiveDashboardUpdate
Broadcasts dashboard statistics update (every alert creation).

```javascript
connection.on("ReceiveDashboardUpdate", (update) => {
  // update: DashboardUpdateNotification
  // {
  //   newAlertsCount: number
  //   pendingAlertsCount: number
  //   criticalAlertsCount: number
  //   updatedAt: string (ISO 8601)
  // }
  updateDashboardStats(update);
});
```

### Example Client Implementation

```typescript
import * as signalR from "@microsoft/signalr";

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/hub/operations", {
        accessTokenFactory: () => localStorage.getItem("token") || ""
    })
    .withAutomaticReconnect([0, 2000, 10000]) // Retry intervals
    .withHubProtocol(new signalR.JsonHubProtocol())
    .build();

connection.on("ReceiveAmlAlert", (alert: AmlAlertNotification) => {
    addAlertToList(alert);
    if (alert.severity === "Critical") {
        playUrgentAlertSound();
    }
});

connection.on("ReceiveUrgentAlert", (alert: AmlAlertNotification) => {
    showUrgentAlertModal(alert);
    notifyComplianceOfficers(alert);
});

connection.on("ReceiveTransactionFlagged", (tx: TransactionFlaggedNotification) => {
    markTransactionAsBlocked(tx.transactionId);
    updateTransactionStatus(tx);
});

connection.on("ReceiveDashboardUpdate", (update: DashboardUpdateNotification) => {
    document.getElementById("alertCount").textContent = update.newAlertsCount;
    document.getElementById("pendingCount").textContent = update.pendingAlertsCount;
    document.getElementById("criticalCount").textContent = update.criticalAlertsCount;
});

connection.onreconnecting((error) => {
    console.log(`Reconnecting due to error: ${error.message}`);
});

connection.onreconnected((connectionId) => {
    console.log(`Successfully reconnected with connection id: ${connectionId}`);
});

await connection.start();
```

---

## Error Responses

### 400 Bad Request
```json
{
  "error": "ValidationError",
  "message": "Request validation failed",
  "details": [
    {
      "field": "requestedLimit",
      "message": "Must be greater than 0"
    }
  ]
}
```

### 404 Not Found
```json
{
  "error": "NotFound",
  "message": "Proposal with ID '660e8400-e29b-41d4-a716-446655440001' not found"
}
```

### 500 Internal Server Error
```json
{
  "error": "InternalServerError",
  "message": "An unexpected error occurred",
  "traceId": "0HN1GC7JIJ2QV:00000001"
}
```

---

## Rate Limiting

All APIs implement rate limiting:
- **Credit Analysis API:** 100 requests/minute per client
- **Compliance API:** 100 requests/minute per client
- **SignalR:** 10 messages/second per connection

Rate limit headers:
```
X-RateLimit-Limit: 100
X-RateLimit-Remaining: 87
X-RateLimit-Reset: 1630000000
```

---

## Authentication & Authorization

### Credit Analysis API
- **Public endpoints:** None (all require authentication)
- **Required role:** `credit_analyst` or `admin`

### Compliance API
- **Public endpoints:** None (all require authentication)
- **Required role:** `compliance_officer` or `admin`

### Operations Server (SignalR)
- **Required role:** `compliance_officer` or `admin`
- **Token:** Bearer JWT in Authorization header or query parameter

---

## Pagination

All list endpoints support pagination:
```
GET /api/endpoint?page=2&pageSize=50

Query Parameters:
- page: Page number (1-indexed, default: 1)
- pageSize: Results per page (min: 10, max: 100, default: 20)

Response:
{
  "data": [...],
  "page": 2,
  "pageSize": 50,
  "total": 250,
  "totalPages": 5
}
```

---

## Filtering

Supported filter operations:
```
GET /api/compliance/alerts?status=Open&severity=High&createdAfter=2026-08-20

Filter Operators:
- Equals: status=Open
- Not equals: status!=Resolved
- Greater than: amount>5000
- Less than: amount<100000
- Contains: alertType~PepMatch
- Date range: createdAfter=2026-08-20&createdBefore=2026-08-26
```

---

## Status Codes

- **200 OK:** Successful GET/PUT request
- **201 Created:** Successful POST request (resource created)
- **204 No Content:** Successful DELETE request
- **400 Bad Request:** Invalid request format or validation error
- **401 Unauthorized:** Missing or invalid authentication
- **403 Forbidden:** Insufficient permissions
- **404 Not Found:** Resource does not exist
- **409 Conflict:** Resource already exists or state conflict
- **429 Too Many Requests:** Rate limit exceeded
- **500 Internal Server Error:** Server error
- **503 Service Unavailable:** Service temporarily unavailable
