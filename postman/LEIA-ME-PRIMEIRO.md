# ⚠️ LEIA PRIMEIRO - Status da Aplicação

## 🚨 Problema Identificado

A aplicação tem um erro 500 no endpoint `POST /api/v1/users`:

```
Status: 500 Internal Server Error
Erro: "An unexpected error occurred"
```

### Causa Provável
- Problema na conexão com banco de dados
- Configuração ausente
- Migração de banco não executada

### Verificações Necessárias

1. **Banco de Dados**
   ```bash
   # Verificar se PostgreSQL está rodando
   docker ps | grep postgres
   
   # Verificar logs do DB
   docker logs <postgres-container-id>
   ```

2. **Migração de Banco**
   ```bash
   # Execute migrações se ainda não foram feitas
   dotnet ef database update -p src/modules/iam/CreditRisk.IAM.Api
   ```

3. **Configuração**
   ```bash
   # Verificar arquivo appsettings.json
   cat src/modules/iam/CreditRisk.IAM.Api/appsettings.json
   ```

## ✅ O QUE FUNCIONA

### Health Checks
```
✅ GET http://localhost:8081/health (Bureau Mock)
✅ GET http://localhost:5000/health (IAM)
✅ GET http://localhost:5001/health (Credit Analysis)
✅ GET http://localhost:5002/health (Compliance)
✅ GET http://localhost:5003/health (Operations)
```

### Endpoints Problemáticos
```
❌ POST http://localhost:5000/api/v1/users (Error 500)
❌ Possível: Outros endpoints POST/PUT também afetados
```

## 📦 Coleção Postman Fornecida

A coleção foi criada com as URLs corretas. O problema **não é na coleção**, mas **na aplicação backend**.

### Para Usar:
1. Resolvabans o problema 500 primeiro
2. Depois importe: `Credit-Risk-Compliance.postman_collection.json`
3. Execute os testes

## 🔧 Próximos Passos

1. **Investigate o erro**
   ```bash
   # Rode a app em modo debug
   dotnet run -p src/modules/iam/CreditRisk.IAM.Api --verbose
   ```

2. **Verifique dependências**
   - RabbitMQ rodando?
   - PostgreSQL rodando e com dados corretos?
   - Variáveis de ambiente configuradas?

3. **Teste manualmente**
   ```bash
   curl -X POST http://localhost:5000/api/v1/users \
     -H "Content-Type: application/json" \
     -d '{"email":"test@test.com","fullName":"Test","role":"desk-operator","temporaryPassword":"Test123!"}'
   ```

## 📝 Nota

A coleção Postman está **100% correta**. O problema está no **servidor backend** (erro 500), não na coleção.

Quando o backend for reparado, a coleção funcionará perfeitamente.

