# 🚀 Guia Completo - Coleção Postman Atualizada

## ✅ O Que Foi Atualizado

A coleção `Credit-Risk-Compliance-Simple.postman_collection.json` agora inclui:

✅ Autenticação JWT automática
✅ Scripts pré-requisito para extrair tokens
✅ Variáveis de ambiente para armazenar IDs
✅ Fluxo completo de testes
✅ 20+ endpoints prontos para usar

---

## 📥 Como Importar

### Opção 1: Postman Desktop
1. Abra **Postman Desktop**
2. Clique em **"Import"** (canto superior esquerdo)
3. Selecione **"Upload Files"**
4. Escolha: `postman/Credit-Risk-Compliance-Simple.postman_collection.json`
5. Clique em **"Import"**
6. Pronto! ✅

### Opção 2: Postman Web
1. Vá para **https://web.postman.co**
2. Clique em **"Import"**
3. Selecione **"Upload Files"**
4. Escolha o arquivo `.json`
5. Clique em **"Import"**

---

## 🔄 Fluxo de Teste Recomendado

### Passo 1: Setup & Health Checks (✅ Nenhuma autenticação necessária)

Execute na ordem:
1. `🏥 Health - IAM (5000)` → 200 OK
2. `🏥 Health - Credit Analysis (5001)` → 200 OK
3. `🏥 Health - Compliance (5002)` → 200 OK
4. `🏥 Health - Bureau Mock (8081)` → 200 OK

**Resultado esperado**: Todos retornam 200 OK

---

### Passo 2: Authentication (🔐 Obtenha o JWT Token)

Execute na ordem:

#### 1️⃣ Criar Usuário
```
POST http://localhost:5000/api/v1/users

Request Body:
{
  "email": "analyst@example.com",
  "fullName": "Compliance Analyst",
  "role": "compliance-analyst",
  "temporaryPassword": "TestPass123!"
}

Response (201 Created):
{
  "id": "...",
  "email": "analyst@example.com",
  "fullName": "Compliance Analyst",
  "role": "complianceanalyst",
  "isActive": true,
  "createdAt": "..."
}
```

#### 2️⃣ Fazer Login (IMPORTANTE!)
```
POST http://localhost:5000/api/v1/auth/login

Request Body:
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

✅ **O token é automaticamente extraído e armazenado** na variável `{{accessToken}}`

#### 3️⃣ Verificar Token (Opcional)
```
GET http://localhost:5000/api/v1/users/me

Headers:
Authorization: Bearer {{accessToken}}

Response (200 OK):
{
  "id": "...",
  "email": "analyst@example.com",
  "fullName": "Compliance Analyst",
  ...
}
```

---

### Passo 3: Credit Analysis (📊 Criar e Gerenciar Propostas)

#### 1️⃣ Criar Proposta
```
POST http://localhost:5001/api/v1/proposals

Headers:
Authorization: Bearer {{accessToken}}
Content-Type: application/json

Request Body:
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
  "createdAt": "..."
}
```

✅ **O ID da proposta é automaticamente extraído** e armazenado em `{{proposalId}}`

#### 2️⃣ Listar Propostas
```
GET http://localhost:5001/api/v1/proposals

Headers:
Authorization: Bearer {{accessToken}}

Response (200 OK):
[
  {
    "id": "550e8400...",
    "applicantName": "John Doe",
    ...
  },
  {
    "id": "660e8400...",
    "applicantName": "Jane Smith",
    ...
  }
]
```

#### 3️⃣ Obter Proposta Específica
```
GET http://localhost:5001/api/v1/proposals/{{proposalId}}

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

### Passo 4: Bureau Mock Integration (🏦 Consultar Dados Externos)

#### 1️⃣ Consultar Histórico de Crédito
```
POST http://localhost:8081/query

Content-Type: application/json

Request Body:
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
  "createdAt": "..."
}
```

#### 2️⃣ Obter Estatísticas
```
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

### Passo 5: Compliance & AML (⚖️ Verificações de Conformidade)

#### 1️⃣ Criar Verificação de Conformidade
```
POST http://localhost:5002/api/v1/compliance/checks

Headers:
Authorization: Bearer {{accessToken}}
Content-Type: application/json

Request Body:
{
  "proposalId": "{{proposalId}}",
  "applicantDocument": "12345678901",
  "applicantName": "John Doe"
}

Response (201 Created):
{
  "id": "770e8400-e29b-41d4-a716-446655440002",
  "proposalId": "550e8400-e29b-41d4-a716-446655440000",
  "applicantDocument": "12345678901",
  "applicantName": "John Doe",
  "status": "pending",
  "checks": {
    "pepScreening": "pending",
    "sanctionList": "clean",
    "amlCheck": "pending"
  },
  "createdAt": "..."
}
```

#### 2️⃣ Listar Verificações
```
GET http://localhost:5002/api/v1/compliance/checks

Headers:
Authorization: Bearer {{accessToken}}

Response (200 OK):
[
  {
    "id": "770e8400...",
    "proposalId": "550e8400...",
    "status": "pending",
    ...
  }
]
```

---

## 🔐 Entendendo as Variáveis

A coleção usa 3 variáveis automáticas:

| Variável | Conteúdo | Definida em |
|----------|----------|-----------|
| `{{accessToken}}` | JWT Bearer Token | Login endpoint |
| `{{proposalId}}` | UUID da proposta | Create Proposal |
| `{{userId}}` | UUID do usuário | Create User |

**Como funcionam:**
1. Quando você executa "Login", o script extrai o `accessToken`
2. Nas próximas requisições, o token é automaticamente incluído no header `Authorization: Bearer {{accessToken}}`
3. Quando cria uma proposta, o `proposalId` é extraído e armazenado
4. Usar `{{proposalId}}` nas requisições subsequentes

---

## 📊 Ordem Recomendada de Execução

```
1. Setup & Health Checks
   ├─ Health IAM (5000)
   ├─ Health Credit Analysis (5001)
   ├─ Health Compliance (5002)
   └─ Health Bureau (8081)

2. Authentication
   ├─ Create User
   ├─ Login (⚠️ IMPORTANTE - gera token)
   └─ Verify Token (opcional)

3. Credit Analysis
   ├─ Create Proposal (extrai proposalId)
   ├─ Get Proposals
   └─ Get Proposal by ID (usa {{proposalId}})

4. Bureau Mock
   ├─ Query Credit History
   └─ Get Statistics

5. Compliance
   ├─ Create Compliance Check (usa {{proposalId}})
   └─ Get Compliance Checks
```

---

## 🆘 Troubleshooting

### ❌ 401 Unauthorized

**Causa**: Token JWT não foi extraído

**Solução**:
1. Execute "Login" novamente
2. Verifique se a resposta contém `accessToken`
3. Verifique se o token foi armazenado em `{{accessToken}}`

### ❌ 404 Not Found (Proposta não encontrada)

**Causa**: `{{proposalId}}` está vazio

**Solução**:
1. Execute "Create Proposal" primeiro
2. Verifique se a resposta contém `id`
3. Execute "Get Proposal by ID" depois

### ❌ 500 Internal Server Error

**Causa**: Banco de dados ou serviço indisponível

**Solução**:
1. Verifique health checks
2. Reinicie os serviços se necessário
3. Verifique se as migrações foram aplicadas

---

## 💡 Dicas e Boas Práticas

1. **Sempre execute Health Checks primeiro** para garantir que os serviços estão rodando
2. **Execute Login antes de testar endpoints autenticados** para obter um token válido
3. **Use as variáveis** `{{accessToken}}`, `{{proposalId}}`, etc. em vez de copiar valores
4. **Salve a coleção** regularmente se fizer customizações
5. **Abra o console do Postman** (View → Show Postman Console) para ver logs de scripts

---

## 📚 Documentação Técnica

Para análise técnica detalhada, consulte:
- `postman/ERRO-500-RESOLVIDO.md` - Como o erro 500 foi resolvido
- `postman/COMO-IMPORTAR.md` - Guia de importação
- `postman/SOLUCAO-ERRO-500.md` - Diagnóstico completo

---

## ✨ Resumo

✅ Coleção completa com 20+ endpoints
✅ Autenticação JWT automática
✅ Scripts pré-requisito para fluxo automático
✅ Variáveis para armazenar dados dinamicamente
✅ Pronta para importar e usar imediatamente

**Importe `Credit-Risk-Compliance-Simple.postman_collection.json` e comece a testar! 🚀**

---

Última atualização: 2026-09-02
Versão: 2.0
