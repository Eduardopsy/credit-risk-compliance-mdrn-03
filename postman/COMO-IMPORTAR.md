# 📥 COMO IMPORTAR A COLEÇÃO POSTMAN

## 🎯 VERSÕES DISPONÍVEIS

### ✅ 1. **Credit-Risk-Compliance-Simple.postman_collection.json** (RECOMENDADO)
- ✅ Versão simplificada e testada
- ✅ 4 folders principais
- ✅ 9 endpoints funcionais
- ✅ Sem erros de variáveis
- 📦 **COMECE COM ESTA!**

### ✅ 2. Credit-Risk-Compliance-FIXED.postman_collection.json
- 19 endpoints
- Todas as variáveis substituídas por valores reais
- Mais completa

### ✅ 3. Credit-Risk-Compliance.postman_collection.json (Original)
- 19 endpoints
- Versão com variáveis ambiente ({{testEmail}}, etc)
- Requer configuração de ambiente no Postman

---

## 🚀 PASSO A PASSO PARA IMPORTAR

### Opção 1: Via Desktop Postman

1. **Abra Postman Desktop**
2. **Clique em "Import"** (canto superior esquerdo)
3. **Selecione "Upload Files"**
4. **Escolha o arquivo:**
   ```
   postman/Credit-Risk-Compliance-Simple.postman_collection.json
   ```
5. **Clique em "Import"**
6. **Pronto! ✅**

### Opção 2: Via Postman Web

1. **Vá para https://web.postman.co**
2. **Faça login** na sua conta
3. **Clique em "Import"**
4. **Escolha "Upload Files"**
5. **Selecione o arquivo .json**
6. **Clique em "Import"**

### Opção 3: Drag & Drop

1. **Abra Postman**
2. **Arraste o arquivo** `.json` para a janela do Postman
3. **Clique em "Import Collection"**

---

## ✅ DEPOIS DE IMPORTAR

### 1️⃣ Teste os Health Checks

```
GET http://localhost:5000/health ✅
GET http://localhost:5001/health ✅
GET http://localhost:5002/health ✅
GET http://localhost:5003/health ✅
GET http://localhost:8081/health ✅
```

Todos devem retornar `200 OK`

### 2️⃣ Teste Criar Usuário

```
POST http://localhost:5000/api/v1/users

Body:
{
  "email": "testoperator@example.com",
  "fullName": "Test Operator",
  "role": "desk-operator",
  "temporaryPassword": "TestPass123!"
}

Response (201 Created):
{
  "id": "...",
  "email": "testoperator@example.com",
  "fullName": "Test Operator",
  "role": "deskoperator",
  "isActive": true,
  "createdAt": "..."
}
```

### 3️⃣ Teste Credit Analysis

```
POST http://localhost:5001/api/v1/proposals

Body:
{
  "applicantName": "John Doe",
  "applicantDocument": "12345678901",
  "requestedAmount": 10000.00,
  "tenor": 12
}
```

### 4️⃣ Teste Bureau Mock

```
POST http://localhost:8081/query

Body:
{
  "document": "12345678901",
  "documentType": "CPF"
}
```

---

## 🆘 SOLUÇÃO DE PROBLEMAS

### ❌ "Failed to import collection"

**Solução:**
1. Use a versão `Simple` (mais confiável)
2. Verifique se o arquivo `.json` não está corrompido
3. Tente importar via web (postman.co)

### ❌ "Collection format not recognized"

**Solução:**
1. Certifique-se de que é um arquivo `.json` válido
2. Tente a versão `FIXED` em vez da `Simple`

### ❌ Endpoints retornam erro 500

**Solução:**
1. Verifique se as migrações foram executadas:
   ```bash
   # IAM
   dotnet ef database update \
     -p src/modules/iam/CreditRisk.IAM.Infrastructure \
     -s src/modules/iam/CreditRisk.IAM.Api \
     -c IamDbContext
   
   # Credit Analysis
   dotnet ef database update \
     -p src/modules/credit-analysis/CreditRisk.CreditAnalysis.Infrastructure \
     -s src/modules/credit-analysis/CreditRisk.CreditAnalysis.Api \
     -c CreditAnalysisDbContext
   ```

2. Verifique se os serviços estão rodando:
   ```bash
   curl http://localhost:5000/health
   curl http://localhost:5001/health
   ```

---

## 📊 O QUE FUNCIONA

| Endpoint | Método | Status |
|----------|--------|--------|
| /health | GET | ✅ 200 OK |
| /api/v1/users | POST | ✅ 201 Created |
| /api/v1/proposals | POST | ✅ 201 Created |
| /query (Bureau) | POST | ✅ 200 OK |

---

## 💡 DICAS

1. **Organize as requisições** por pasta (já estão)
2. **Use pré-scripts** para extrair IDs automaticamente
3. **Configure variáveis de ambiente** se quiser reutilizar valores
4. **Salve testes** para automatizar validações

---

## 📞 SUPORTE

Se o import ainda não funcionar:

1. **Verifique a versão do Postman**
   - Atualize para a versão mais recente
   
2. **Tente a versão Web**
   - https://web.postman.co
   
3. **Importe manualmente**
   - Crie uma nova coleção
   - Adicione os endpoints um a um

---

**Pronto para usar! Escolha a versão `Simple` e aproveite! 🚀**

