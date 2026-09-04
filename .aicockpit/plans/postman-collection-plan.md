# Plano: Coleção Postman para Testes Funcionais

## Objetivo
Criar uma coleção Postman completa e importável para testar todos os serviços da plataforma Credit Risk Compliance, incluindo:
- Autenticação (IAM)
- Análise de Crédito
- Conformidade/AML
- Health checks
- SignalR WebSocket (Operations Hub)

## Contexto
- Sistema com 5 APIs rodando em portas 5000-5003, 8081
- APIs utilizam JWT Bearer Token para autenticação
- Variáveis de ambiente necessárias: base URLs, tokens, IDs
- Documentação API já existe em docs/API_REFERENCE.md

## Escopo

### 1. Estrutura da Coleção Postman
```
Credit Risk Compliance System
├── Setup & Health Checks
│   ├── Health - Bureau Mock (8081)
│   ├── Health - IAM (5000)
│   ├── Health - Credit Analysis (5001)
│   ├── Health - Compliance (5002)
│   └── Health - Operations (5003)
├── Authentication (IAM - Port 5000)
│   ├── Create User
│   ├── Login
│   ├── Get User
│   └── Logout
├── Credit Analysis (Port 5001)
│   ├── Create Proposal
│   ├── Get Proposal
│   ├── Submit Proposal
│   └── Get Scoring Result
├── Compliance (Port 5002)
│   ├── Ingest Transaction
│   ├── Get Transaction
│   ├── List Transactions
│   ├── List Alerts
│   └── Review Alert
├── Bureau Mock (Port 8081)
│   ├── Query (Simulate credit bureau)
│   └── Query Error (Test error handling)
└── Operations WebSocket (Port 5003)
    └── Connect to SignalR Hub (documentation)
```

### 2. Variáveis de Ambiente

**Global Variables:**
- `baseUrl_Bureau`: http://localhost:8081
- `baseUrl_IAM`: http://localhost:5000
- `baseUrl_CreditAnalysis`: http://localhost:5001
- `baseUrl_Compliance`: http://localhost:5002
- `baseUrl_Operations`: http://localhost:5003
- `bearerToken`: (set after login)
- `userId`: (generated/set)
- `proposalId`: (generated/set)
- `transactionId`: (generated/set)
- `alertId`: (generated/set)

### 3. Endpoints a Incluir

#### IAM (5000)
- `POST /api/v1/auth/login` - Login com username/password
- `POST /api/v1/auth/logout` - Logout
- `POST /api/v1/users` - Create User
- `GET /api/v1/users/{id}` - Get User

#### Credit Analysis (5001)
- `POST /api/v1/proposals` - Create Proposal
- `GET /api/v1/proposals/{id}` - Get Proposal by ID
- `PUT /api/v1/proposals/{id}/submit` - Submit Proposal
- `GET /health` - Health check

#### Compliance (5002)
- `POST /api/v1/transactions` - Ingest Transaction
- `GET /api/v1/transactions/{id}` - Get Transaction
- `GET /api/v1/transactions?customerId=...&page=...` - List Transactions
- `GET /api/v1/alerts` - List Alerts
- `PUT /api/v1/alerts/{id}/review` - Review Alert
- `GET /health` - Health check

#### Bureau Mock (8081)
- `POST /query` - Query bureau with document
- `POST /query/error/{statusCode}` - Test error handling
- `GET /health` - Health check

### 4. Fluxos de Teste

#### Fluxo 1: Autenticação Completa
1. Create User (admin role)
2. Login com credenciais criadas
3. Extract JWT token para variável
4. Get User (utilizar token)
5. Logout

#### Fluxo 2: Análise de Crédito Completa
1. Create Proposal com dados válidos
2. Extract proposalId
3. Get Proposal (verificar status Received)
4. Submit Proposal
5. Verify scoring result

#### Fluxo 3: Conformidade Completa
1. Ingest Transaction com dados normais
2. Ingest Transaction com valor > 100k (Large Transaction)
3. Multiple transactions < 10k in 7 days (Structuring)
4. Extract transactionIds e alertIds
5. List Transactions
6. List Alerts
7. Review Alert

#### Fluxo 4: Casos de Erro
1. Create Proposal com documento inválido
2. Query Bureau (sucesso) vs Query Bureau Error
3. Get non-existent proposal
4. Get non-existent transaction

### 5. Dados de Teste

**Test Users:**
```json
{
  "username": "test_operator",
  "email": "operator@example.com",
  "password": "SecurePass123!",
  "roles": ["desk-operator"]
}
{
  "username": "test_analyst",
  "email": "analyst@example.com",
  "password": "SecurePass123!",
  "roles": ["compliance-analyst"]
}
```

**Test Proposals:**
- Document: 12345678901, Type: CPF
- Monthly Income: 10000
- Requested Limit: 50000
- Bureau Consent: true

**Test Transactions:**
- Normal: 5000 BRL
- Large: 150000 BRL
- Structuring: 9000 BRL x 5 (multiple)
- PEP Document: "000" prefix

### 6. Request/Response Examples

**Estrutura com pre-request scripts:**
- Auto-generate UUIDs
- Set timestamps
- Token management
- Variable extraction via regex

**Estrutura com tests:**
- Status code assertions (200, 201, 400, 404)
- Response body validation (schema validation via Ajv)
- Variable assignments (ids, tokens)
- Performance checks (< 1s response time)

### 7. Documentação na Coleção

Cada request incluirá:
- Descrição clara do que testa
- Expected status code
- Headers necessários
- Body examples
- Response schema examples

### 8. Arquivo de Saída

**Formato:** `Credit-Risk-Compliance-Collection.postman_collection.json`

**Estrutura JSON Postman v2.1:**
```json
{
  "info": {
    "name": "Credit Risk Compliance System",
    "description": "Complete functional test collection",
    "version": "1.0.0",
    "schema": "https://schema.getpostman.com/json/collection/v2.1.0/collection.json"
  },
  "item": [...],
  "variable": [...],
  "event": [...]
}
```

## Implementação

### Passo 1: Gerar estrutura base (JSON)
- Criar structure.json com pasta hierarchy
- Definir todas as variáveis

### Passo 2: Popuar requests básicas
- Health checks (todos os serviços)
- Authentication endpoints
- CRUD endpoints

### Passo 3: Adicionar pre-request scripts
- UUID generation
- Timestamp setup
- Token management

### Passo 4: Adicionar test scripts
- Status code checks
- Response validation
- Variable extraction

### Passo 5: Validação
- Importar no Postman
- Executar todos os requests
- Verify all tests pass
- Create environment file template

## Deliverables

1. **Credit-Risk-Compliance-Collection.postman_collection.json** (main collection)
2. **Postman-Environment-Template.json** (environment file template)
3. **POSTMAN_GUIDE.md** (how to import and use)
4. **Postman_Test_Results.md** (documentation of test coverage)

## Benefícios

✅ Testes funcionais automatizados
✅ Documentação executável
✅ Fácil onboarding para novos desenvolvedores
✅ Integração com CI/CD (Newman)
✅ Rastreamento de casos de teste
✅ Validação de contratos API

## Perguntas para o Usuário

1. **Incluir testes de performance?** (tempos de resposta mínimos/máximos)
2. **Incluir testes de carga/stress?** (ou apenas testes funcionais)
3. **Versionar a coleção?** (v1.0, v1.1, etc.)
4. **Incluir testes de segurança?** (validação de autenticação, CORS, etc.)
5. **Incluir WebSocket tests**? (SignalR connection to Operations Hub)
6. **CI/CD integration?** (scripts para rodar via Newman)

