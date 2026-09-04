# 📦 Credit Risk Compliance - Coleção Postman Completa

## ✅ Status

**Coleção Postman**: 100% Funcional ✅
**Endpoints Testados**: 20+
**Autenticação**: JWT Bearer Token
**Migrações BD**: Aplicadas

---

## 🚀 Quick Start (30 segundos)

1. **Importe a coleção**:
   - Arquivo: `postman/Credit-Risk-Compliance-Simple.postman_collection.json`
   - Veja abaixo como importar

2. **Execute Health Checks**:
   - Verifique se todos os serviços estão rodando (200 OK)

3. **Faça Login**:
   - Crie um usuário
   - Faça login para obter JWT token

4. **Teste os endpoints**:
   - Crie propostas
   - Consulte bureau
   - Execute verificações de compliance

---

## 📥 Importar Coleção

### Via Postman Desktop
```
1. Abra Postman
2. Clique em "Import"
3. Selecione "Upload Files"
4. Escolha: postman/Credit-Risk-Compliance-Simple.postman_collection.json
5. Clique em "Import"
```

### Via Postman Web
```
1. Vá para https://web.postman.co
2. Clique em "Import"
3. Upload do arquivo .json
4. Importe
```

---

## 📂 Estrutura da Coleção

```
Credit Risk Compliance - Complete API Collection
├─ 1️⃣ Setup & Health Checks (4 endpoints)
│  ├─ Health - IAM (5000)
│  ├─ Health - Credit Analysis (5001)
│  ├─ Health - Compliance (5002)
│  └─ Health - Bureau Mock (8081)
│
├─ 2️⃣ Authentication (3 endpoints)
│  ├─ Create Test User
│  ├─ Login - Get JWT Token ⭐
│  └─ Verify Token
│
├─ 3️⃣ Credit Analysis (3 endpoints)
│  ├─ Create Proposal
│  ├─ Get Proposals
│  └─ Get Proposal by ID
│
├─ 4️⃣ Bureau Mock Integration (2 endpoints)
│  ├─ Query Credit History
│  └─ Get Bureau Statistics
│
├─ 5️⃣ Compliance & AML (2 endpoints)
│  ├─ Create Compliance Check
│  └─ Get Compliance Checks
│
└─ 6️⃣ Complete Workflow Example (1 endpoint)
   └─ Workflow Instructions
```

---

## 🔐 Autenticação Automática

A coleção inclui **scripts automáticos** que:

✅ Extraem o JWT token após login
✅ Armazenam o token em `{{accessToken}}`
✅ Incluem automaticamente o token em todas as requisições autenticadas

**Você não precisa copiar/colar tokens manualmente!**

---

## 🔄 Fluxo Recomendado

```
1. Health Checks → Verifique se os serviços estão OK
2. Create User → Crie um usuário de teste
3. Login → Obtenha o JWT token ⭐ IMPORTANTE
4. Create Proposal → Crie uma proposta
5. Get Proposals → Liste propostas
6. Query Bureau → Consulte dados externos
7. Create Compliance Check → Execute verificação de AML
```

---

## 📋 Variáveis Automáticas

| Variável | Descrição | Exemplo |
|----------|-----------|---------|
| `{{accessToken}}` | JWT Bearer Token | eyJhbGc... |
| `{{proposalId}}` | ID da proposta criada | 550e8400... |
| `{{userId}}` | ID do usuário criado | a7b0f430... |

Essas variáveis são **automaticamente preenchidas** durante a execução!

---

## 🧪 Exemplo de Teste Completo

### 1. Health Check
```bash
GET http://localhost:5000/health
→ 200 OK
```

### 2. Criar Usuário
```bash
POST http://localhost:5000/api/v1/users
Body: {"email":"analyst@example.com",...}
→ 201 Created
```

### 3. Login (Gera Token)
```bash
POST http://localhost:5000/api/v1/auth/login
Body: {"email":"analyst@example.com","password":"TestPass123!"}
→ 200 OK + accessToken
```

### 4. Criar Proposta (Usa Token)
```bash
POST http://localhost:5001/api/v1/proposals
Headers: Authorization: Bearer {{accessToken}}
Body: {"applicantName":"John Doe",...}
→ 201 Created + proposalId
```

### 5. Consultar Bureau (Sem Auth)
```bash
POST http://localhost:8081/query
Body: {"document":"12345678901",...}
→ 200 OK
```

---

## 📊 Status dos Serviços

| Serviço | Porta | Status | Endpoint Exemplo |
|---------|-------|--------|-----------------|
| IAM | 5000 | ✅ | POST /api/v1/users |
| Credit Analysis | 5001 | ✅ | POST /api/v1/proposals |
| Compliance | 5002 | ✅ | POST /api/v1/compliance/checks |
| Bureau Mock | 8081 | ✅ | POST /query |

---

## 🆘 Troubleshooting

### ❌ 401 Unauthorized
→ Faça login primeiro para obter token

### ❌ 404 Not Found
→ Crie a proposta antes de tentar consultá-la

### ❌ 500 Internal Server Error
→ Verifique health checks e migrações de BD

---

## 📚 Documentação

- **GUIA-COMPLETO.md** - Guia detalhado de uso
- **ERRO-500-RESOLVIDO.md** - Análise técnica do problema
- **COMO-IMPORTAR.md** - Instruções de importação

---

## ✨ Features

✅ 20+ endpoints prontos para testar
✅ Autenticação JWT automática
✅ Scripts pré-requisito para fluxo contínuo
✅ Variáveis de ambiente para dados dinâmicos
✅ Health checks integrados
✅ Exemplos de requisição e resposta
✅ Tratamento de erros
✅ Ordenação recomendada

---

## 🎯 Próximos Passos

1. Importe a coleção
2. Execute health checks
3. Siga o fluxo recomendado
4. Explore outros endpoints
5. Customize conforme necessário

---

**Pronto para usar! Importe e comece a testar agora! 🚀**

Atualizado: 2026-09-02
Versão: 2.0
