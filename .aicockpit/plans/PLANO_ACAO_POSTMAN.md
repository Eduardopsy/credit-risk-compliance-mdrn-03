# 🚀 PLANO DE AÇÃO EXECUTIVO
## Correção da Coleção Postman - Credit Risk Compliance

**Versão:** 1.0  
**Data:** 01 de Setembro, 2026  
**Urgência:** CRÍTICA ⛔

---

## 📌 RESUMO CRÍTICO

A análise descobriu **2 endpoints críticos** mencionados na coleção Postman que **NÃO EXISTEM NO CÓDIGO**:

```
❌ GET /api/v1/transactions/{id}
❌ GET /api/v1/transactions?page=X&pageSize=Y
```

**Impacto:** Qualquer pessoa que importar esta coleção e executar contra o servidor terá 2 testes falhando.

---

## 🎯 DECISÃO NECESSÁRIA (BLOQUEADORA)

Antes de proceder com qualquer melhoria, é necessário **tomar uma decisão executiva**:

### Opção A: Implementar os Endpoints
```
✅ Vantagem: Coleção estará completa
⏱️ Tempo: ~2 horas (análise + implementação)
📝 Ação: Implementar GET routes em TransactionEndpoints.cs
```

### Opção B: Remover da Coleção
```
✅ Vantagem: Rápido, sem mudanças no código
⏱️ Tempo: ~30 minutos
📝 Ação: Remover 2 requests da coleção + documentação
```

### Recomendação: **OPÇÃO A (Implementar)**
Motivo: Endpoints de GET são padrão RESTful e úteis para validação.

---

## 📋 PLANO DE AÇÃO EM FASES

### FASE 1: VALIDAÇÃO (Hoje - 30 min)
**Objetivo:** Confirmar os problemas

```bash
# 1. Executar a coleção contra o servidor real
npm install -g newman
cd postman
newman run Credit-Risk-Compliance-Collection.postman_collection.json \
  -e Credit-Risk-Compliance-Environment.postman_environment.json \
  --reporters cli

# 2. Documentar erros exatos
# Esperado: 2 failures em "Get Transaction" e "List Transactions"
```

### FASE 2: IMPLEMENTAÇÃO (Próximos 2-3 dias)
**Objetivo:** Adicionar os endpoints faltando

#### Step 1: Adicionar GET endpoints ao TransactionEndpoints.cs

```csharp
// Adicionar estas rotas em MapTransactionEndpoints()

group.MapGet("/{id:guid}", async (
    Guid id,
    GetTransactionByIdQueryHandler handler,
    CancellationToken ct) =>
{
    var result = await handler.HandleAsync(new GetTransactionByIdQuery(id), ct);
    return result.IsSuccess
        ? Results.Ok(result.Value)
        : Results.NotFound();
})
.WithName("GetTransactionById")
.Produces<TransactionDto>()
.Produces(StatusCodes.Status404NotFound);

group.MapGet("/", async (
    [FromQuery] int page,
    [FromQuery] int pageSize,
    ListTransactionsQueryHandler handler,
    CancellationToken ct) =>
{
    var query = new ListTransactionsQuery(page <= 0 ? 1 : page, pageSize <= 0 ? 10 : pageSize);
    var result = await handler.HandleAsync(query, ct);
    return Results.Ok(result.Value);
})
.WithName("ListTransactions")
.Produces<PagedResult<TransactionDto>>();
```

#### Step 2: Criar Query Handlers
- `GetTransactionByIdQueryHandler`
- `ListTransactionsQueryHandler`

#### Step 3: Testar localmente
```bash
# Executar testes contra servidor local
./run-postman-tests.sh --run
```

#### Step 4: Validar cobertura
- [ ] GET /api/v1/transactions/{id} - 200 OK
- [ ] GET /api/v1/transactions?page=1&pageSize=10 - 200 OK com paginação

---

### FASE 3: MELHORIAS (Próxima semana)
**Objetivo:** Aumentar qualidade dos testes

#### Melhoria 1: Variáveis de Environment
Adicionar ao `Credit-Risk-Compliance-Environment.postman_environment.json`:

```json
{
  "key": "currentUserRole",
  "value": "desk-operator",
  "type": "string"
},
{
  "key": "testTimeout",
  "value": "10000",
  "type": "number"
},
{
  "key": "retryCount",
  "value": "3",
  "type": "number"
}
```

#### Melhoria 2: Testes de Autorização Negada
Adicionar nova folder "Negative Tests" com:
- [ ] CreateProposal com role "compliance-analyst" (esperado: 403)
- [ ] ListAlerts com role "desk-operator" (esperado: 403)
- [ ] GetUserById sem BearerToken (esperado: 401)

#### Melhoria 3: Testes de Paginação
Expandir ListAlerts e ListTransactions com:
- [ ] page=2, pageSize=5
- [ ] Validar estrutura: `{ items: [...], pageNumber: 2, pageSize: 5, totalItems: X }`

#### Melhoria 4: Documentação
- [ ] Sincronizar TEST_SCENARIOS.md com endpoints reais
- [ ] Atualizar POSTMAN_GUIDE.md com troubleshooting
- [ ] Adicionar seção de limitations

---

## 📊 IMPACTO

### Antes da Correção
```
✅ 19 tests passando
❌ 2 tests falhando
📊 95% de cobertura
🚫 Bloqueado para produção
```

### Depois da Correção
```
✅ 21 tests passando
✅ 0 tests falhando
📊 100% de cobertura
✅ Pronto para produção
```

---

## ⏱️ ESTIMATIVA DE TEMPO

| Fase | Tarefa | Tempo | Prioridade |
|------|--------|-------|-----------|
| 1 | Validação | 30 min | 🔴 CRÍTICA |
| 2 | Implementar GET /transactions/{id} | 1.5 h | 🔴 CRÍTICA |
| 2 | Implementar GET /transactions | 1 h | 🔴 CRÍTICA |
| 2 | Testes e validação | 30 min | 🔴 CRÍTICA |
| 3 | Variáveis de environment | 30 min | 🟡 ALTA |
| 3 | Testes de autorização | 1 h | 🟡 ALTA |
| 3 | Testes de paginação | 30 min | 🟡 ALTA |
| 3 | Documentação | 1 h | 🟡 ALTA |

**Total Fase Crítica:** ~3.5 horas  
**Total Fases de Melhoria:** ~3.5 horas  
**Total:** ~7 horas

---

## ✅ CHECKLIST DE EXECUÇÃO

### FASE 1: VALIDAÇÃO
- [ ] Clone/Pull do repositório
- [ ] Instalar Newman: `npm install -g newman`
- [ ] Executar coleção
- [ ] Documentar falhas exatas
- [ ] Compartilhar resultados

### FASE 2: IMPLEMENTAÇÃO
- [ ] Criar branch: `feature/add-transaction-queries`
- [ ] Implementar GetTransactionByIdQueryHandler
- [ ] Implementar ListTransactionsQueryHandler
- [ ] Adicionar rotas em TransactionEndpoints.cs
- [ ] Testes unitários (se aplicável)
- [ ] Testar localmente
- [ ] Criar PR para revisão
- [ ] Merge após aprovação
- [ ] Re-testar coleção Postman

### FASE 3: MELHORIAS
- [ ] Adicionar variáveis de environment
- [ ] Criar testes de autorização negada
- [ ] Expandir testes de paginação
- [ ] Atualizar documentação
- [ ] Testar suite completa
- [ ] Documentar como parte do workflow

---

## 🔗 ARQUIVOS RELACIONADOS

- **Relatório Completo:** `postman/RELATORIO_ANALISE_COMPLETO.md`
- **Coleção Atual:** `postman/Credit-Risk-Compliance-Collection.postman_collection.json`
- **Environment:** `postman/Credit-Risk-Compliance-Environment.postman_environment.json`
- **Código Afetado:** `src/modules/compliance/CreditRisk.Compliance.Api/Endpoints/TransactionEndpoints.cs`

---

## 📞 PRÓXIMOS PASSOS IMEDIATOS

1. **HOJE:** Executar Fase 1 (Validação)
2. **Amanhã:** Decisão sobre Opção A vs B
3. **Próximos 2-3 dias:** Executar Fase 2 se Opção A
4. **Próxima semana:** Executar Fase 3 (Melhorias)

---

## 📝 NOTAS IMPORTANTES

⚠️ **NÃO fazer commit direto** - Sempre criar PR para revisão  
⚠️ **Sincronizar documentação** - Qualquer mudança no código requer atualização de docs  
✅ **Testar localmente primeiro** - Antes de push  
✅ **Manter retrocompatibilidade** - Endpoints novos não devem quebrar existentes

---

**Preparado por:** AI Cockpit  
**Status:** Aguardando Decision Point na Opção A vs B

