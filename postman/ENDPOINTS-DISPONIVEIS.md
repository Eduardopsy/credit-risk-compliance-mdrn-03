# 📋 Endpoints Disponíveis - Referência Completa

## ✅ Endpoints Implementados

### 🏥 Health Checks (Sem Autenticação)

| Método | Endpoint | Porta | Status |
|--------|----------|-------|--------|
| GET | `/health` | 5000 (IAM) | ✅ |
| GET | `/health` | 5001 (Credit Analysis) | ✅ |
| GET | `/health` | 5002 (Compliance) | ✅ |
| GET | `/health` | 8081 (Bureau Mock) | ✅ |

---

### 👤 User Management (IAM)

| Método | Endpoint | Auth | Status | Descrição |
|--------|----------|------|--------|-----------|
| POST | `/api/v1/users` | ❌ | ✅ | Criar novo usuário |
| GET | `/api/v1/users/{id:guid}` | ✅ Admin | ✅ | Obter usuário por ID |

**Exemplo - Criar Usuário:**
```bash
POST http://localhost:5000/api/v1/users

Body:
{
  "email": "analyst@example.com",
  "fullName": "Compliance Analyst",
  "role": "compliance-analyst",
  "temporaryPassword": "TestPass123!"
}

Response (201 Created):
{
  "id": "ca900956-869e-441d-9359-c63c5cf60aea",
  "email": "analyst@example.com",
  "fullName": "Compliance Analyst",
  "role": "complianceanalyst",
  "isActive": true,
  "createdAt": "2026-09-02T13:27:40.3074362+00:00"
}
```

**Exemplo - Obter Usuário por ID:**
```bash
GET http://localhost:5000/api/v1/users/ca900956-869e-441d-9359-c63c5cf60aea

Headers:
Authorization: Bearer {{accessToken}}

Response (200 OK):
{
  "id": "ca900956-869e-441d-9359-c63c5cf60aea",
  "email": "analyst@example.com",
  "fullName": "Compliance Analyst",
  "role": "complianceanalyst",
  "isActive": true,
  "createdAt": "2026-09-02T13:27:40.3074362+00:00"
}
```

---

### 🔐 Authentication (IAM)

| Método | Endpoint | Auth | Status | Descrição |
|--------|----------|------|--------|-----------|
| POST | `/api/v1/auth/login` | ❌ | ✅ | Fazer login e obter JWT |
| POST | `/api/v1/auth/logout` | ✅ | ✅ | Fazer logout |

**Exemplo - Login:**
```bash
POST http://localhost:5000/api/v1/auth/login

Body:
{
  "email": "analyst@example.com",
  "password": "TestPass123!",
  "totpCode": "000000"
}

Response (200 OK):
{
  "accessToken": "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCIsImtpZCI6I...",
  "refreshToken": "...",
  "expiresIn": 3600
}
```

**Exemplo - Logout:**
```bash
POST http://localhost:5000/api/v1/auth/logout

Headers:
Authorization: Bearer {{accessToken}}

Response (204 No Content):
```

---

### 📊 Credit Analysis

| Método | Endpoint | Auth | Status | Descrição |
|--------|----------|------|--------|-----------|
| POST | `/api/v1/proposals` | ✅ | ✅ | Criar proposta |
| GET | `/api/v1/proposals` | ✅ | ✅ | Listar propostas |
| GET | `/api/v1/proposals/{id:guid}` | ✅ | ✅ | Obter proposta por ID |

**Exemplo - Criar Proposta:**
```bash
POST http://localhost:5001/api/v1/proposals

Headers:
Authorization: Bearer {{accessToken}}
Content-Type: application/json

Body:
{
  "applicantName": "John Doe",
  "applicantDocument": "12345678901",
  "requestedAmount": 10000.00,
  "tenor": 12
}

Response (201 Created):
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "applicantName": "John Doe",
  "applicantDocument": "12345678901",
  "requestedAmount": 10000.00,
  "tenor": 12,
  "status": "submitted",
  "createdAt": "2026-09-02T13:27:40.3074362+00:00"
}
```

**Exemplo - Listar Propostas:**
```bash
GET http://localhost:5001/api/v1/proposals

Headers:
Authorization: Bearer {{accessToken}}

Response (200 OK):
[
  {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "applicantName": "John Doe",
    ...
  }
]
```

**Exemplo - Obter Proposta por ID:**
```bash
GET http://localhost:5001/api/v1/proposals/550e8400-e29b-41d4-a716-446655440000

Headers:
Authorization: Bearer {{accessToken}}

Response (200 OK):
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "applicantName": "John Doe",
  ...
}
```

---

### 🏦 Bureau Mock Integration

| Método | Endpoint | Auth | Status | Descrição |
|--------|----------|------|--------|-----------|
| POST | `/query` | ❌ | ✅ | Consultar histórico de crédito |
| GET | `/statistics` | ❌ | ✅ | Obter estatísticas |

**Exemplo - Consultar Crédito:**
```bash
POST http://localhost:8081/query

Content-Type: application/json

Body:
{
  "document": "12345678901",
  "documentType": "CPF"
}

Response (200 OK):
{
  "document": "12345678901",
  "documentType": "CPF",
  "status": "success",
  "creditScore": 750,
  "totalDebt": 5000.00,
  "defaultHistory": false,
  "inquiries": 2,
  "createdAt": "2026-09-02T13:27:40.3074362+00:00"
}
```

**Exemplo - Estatísticas:**
```bash
GET http://localhost:8081/statistics

Response (200 OK):
{
  "totalQueries": 42,
  "successRate": 98.5,
  "averageResponseTime": 145,
  "queriesByDocumentType": {
    "CPF": 38,
    "CNPJ": 4
  }
}
```

---

### ⚖️ Compliance & AML

| Método | Endpoint | Auth | Status | Descrição |
|--------|----------|------|--------|-----------|
| POST | `/api/v1/compliance/checks` | ✅ | ✅ | Criar verificação |
| GET | `/api/v1/compliance/checks` | ✅ | ✅ | Listar verificações |

**Exemplo - Criar Verificação:**
```bash
POST http://localhost:5002/api/v1/compliance/checks

Headers:
Authorization: Bearer {{accessToken}}
Content-Type: application/json

Body:
{
  "proposalId": "550e8400-e29b-41d4-a716-446655440000",
  "applicantDocument": "12345678901",
  "applicantName": "John Doe"
}

Response (201 Created):
{
  "id": "770e8400-e29b-41d4-a716-446655440002",
  "proposalId": "550e8400-e29b-41d4-a716-446655440000",
  "status": "pending",
  "checks": {
    "pepScreening": "pending",
    "sanctionList": "clean",
    "amlCheck": "pending"
  }
}
```

**Exemplo - Listar Verificações:**
```bash
GET http://localhost:5002/api/v1/compliance/checks

Headers:
Authorization: Bearer {{accessToken}}

Response (200 OK):
[
  {
    "id": "770e8400-e29b-41d4-a716-446655440002",
    "proposalId": "550e8400-e29b-41d4-a716-446655440000",
    "status": "pending",
    ...
  }
]
```

---

## ❌ Endpoints NÃO Existentes

Os seguintes endpoints **NÃO existem** no código:

- ❌ `GET /api/v1/users/me` - Não implementado
- ❌ `GET /api/v1/users` (listar todos) - Não implementado
- ❌ `PUT /api/v1/users/{id}` - Não implementado
- ❌ `DELETE /api/v1/users/{id}` - Não implementado

---

## 📊 Resumo

| Tipo | Total | Implementados | Faltando |
|------|-------|---------------|----------|
| Health Checks | 4 | 4 | 0 |
| User Management | 4 | 2 | 2 |
| Authentication | 2 | 2 | 0 |
| Credit Analysis | 3 | 3 | 0 |
| Bureau Mock | 2 | 2 | 0 |
| Compliance | 2 | 2 | 0 |
| **TOTAL** | **17** | **15** | **2** |

---

## 🔐 Autenticação

### Endpoints SEM Autenticação (❌)
- POST /api/v1/users
- POST /api/v1/auth/login
- GET /health (todos)
- POST /query
- GET /statistics

### Endpoints COM Autenticação (✅)
- GET /api/v1/users/{id}
- POST /api/v1/auth/logout
- POST /api/v1/proposals
- GET /api/v1/proposals
- GET /api/v1/proposals/{id}
- POST /api/v1/compliance/checks
- GET /api/v1/compliance/checks

---

## 🎯 Fluxo Recomendado

```
1. GET /health (todos) → Verificar serviços
2. POST /api/v1/users → Criar usuário
3. POST /api/v1/auth/login → Obter token
4. POST /api/v1/proposals → Criar proposta
5. GET /api/v1/proposals → Listar propostas
6. POST /query (Bureau) → Consultar crédito
7. POST /api/v1/compliance/checks → Criar check
```

---

**Última atualização: 2026-09-02**
