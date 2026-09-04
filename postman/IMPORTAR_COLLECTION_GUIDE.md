# 📦 COMO IMPORTAR A COLEÇÃO NO POSTMAN

## 🎯 ARQUIVO GERADO

**Nome:** `Credit-Risk-Compliance-Collection-Updated.postman_collection.json`  
**Localização:** `postman/`  
**Tamanho:** 11.8 KB  
**Versão:** 2.0  

---

## 📋 CONTEÚDO DA COLEÇÃO

### ✅ O que você vai importar:

```
Credit Risk Compliance System - Complete
├─ 1️⃣  Setup & Health Checks (5 requests)
│  ├─ Health - Bureau Mock (8081)
│  ├─ Health - IAM (5000)
│  ├─ Health - Credit Analysis (5001)
│  ├─ Health - Compliance (5002)
│  └─ Health - Operations Server (5003)
│
├─ 2️⃣  Authentication (IAM) (4 requests)
│  ├─ Create Test User
│  ├─ Login as Test User
│  ├─ Get User Details
│  └─ Logout
│
├─ 3️⃣  Credit Analysis (3 requests)
│  ├─ Create Proposal
│  ├─ Get Proposal
│  └─ Submit Proposal
│
├─ 4️⃣  Compliance (4 requests)
│  ├─ Ingest Transaction (Normal)
│  ├─ Ingest Transaction (Large - Triggers Alert)
│  ├─ List Alerts
│  └─ Review Alert
│
└─ 5️⃣  Bureau Mock (3 requests)
   ├─ Query Bureau (Success)
   ├─ Query Bureau Error (500)
   └─ Query Bureau Error (503)

TOTAL: 19 Requests prontos para usar
```

### 📊 Variáveis Incluídas:

```
- baseUrl_Bureau (http://localhost:8081)
- baseUrl_IAM (http://localhost:5000)
- baseUrl_CreditAnalysis (http://localhost:5001)
- baseUrl_Compliance (http://localhost:5002)
- baseUrl_Operations (http://localhost:5003)
- bearerToken (auto-extraído do login)
- userId (auto-extraído)
- proposalId (auto-extraído)
- transactionId (auto-extraído)
- alertId (auto-extraído)
- timestamp (auto-gerado)
```

---

## 🚀 COMO IMPORTAR (PASSO A PASSO)

### Método 1: Importação via Interface (Recomendado)

#### **Passo 1:** Abra o Postman

```
Clique em Postman desktop ou acesse https://web.postman.co
```

#### **Passo 2:** Clique em "Import"

```
Menu superior esquerdo:
Arquivo → Import
OU
Clique em "Import" no centro da tela
```

#### **Passo 3:** Selecione o arquivo

```
Navegue até:
/seu-projeto/postman/Credit-Risk-Compliance-Collection-Updated.postman_collection.json

OU arraste o arquivo para a janela do Postman
```

#### **Passo 4:** Confirme a importação

```
Clique em "Import"
A coleção aparecerá na sua sidebar esquerda
```

### Método 2: Importação via CLI (Para Automatização)

```bash
# Se tiver Postman CLI instalado
postman collection import Credit-Risk-Compliance-Collection-Updated.postman_collection.json

# Para usar com Newman
npm install -g newman
newman run Credit-Risk-Compliance-Collection-Updated.postman_collection.json \
  -e Credit-Risk-Compliance-Environment.postman_environment.json
```

---

## 📚 COMO USAR A COLEÇÃO

### IMPORTANTE: Ordem de Execução Recomendada

Sempre execute nesta ordem para evitar erros de variáveis não definidas:

```
1. Setup & Health Checks (verifica se tudo está rodando)
   ↓
2. Authentication (faz login e extrai token)
   ↓
3. Credit Analysis (usa o token)
   ↓
4. Compliance (usa o token)
   ↓
5. Bureau Mock (testes de integração)
```

### Opção A: Executar Tudo no GUI (Mais Fácil)

1. **Selecione a pasta "Setup & Health Checks"**
   ```
   Clique com botão direito → Run folder
   ```

2. **Observe os resultados**
   ```
   Todos os 5 health checks devem passar
   ```

3. **Repita para cada folder em ordem**
   ```
   Authentication → Credit Analysis → Compliance → Bureau Mock
   ```

4. **Verifique o histórico**
   ```
   Abra "Test Results" para ver sumário de testes
   ```

### Opção B: Executar com Newman (CLI - Automatizado)

```bash
# 1. Instale Newman se ainda não tiver
npm install -g newman

# 2. Navegue até a pasta postman
cd postman

# 3. Execute a coleção
newman run Credit-Risk-Compliance-Collection-Updated.postman_collection.json \
  -e Credit-Risk-Compliance-Environment.postman_environment.json \
  --reporters cli,html \
  --reporter-html-export report.html

# 4. Visualize o relatório
open report.html
```

---

## ⚙️ CONFIGURAÇÃO NECESSÁRIA

### Pré-requisitos

1. **Postman instalado**
   - Desktop: https://www.postman.com/downloads/
   - Web: https://web.postman.co

2. **Serviços rodando localmente**
   ```bash
   ./start-all-services.sh
   # Verifique se estas portas estão livres:
   # 8081 - Bureau Mock
   # 5000 - IAM
   # 5001 - Credit Analysis
   # 5002 - Compliance
   # 5003 - Operations Server
   ```

3. **NodeJS/npm (opcional, para Newman)**
   ```bash
   npm install -g newman
   ```

---

## 🔍 TESTE RÁPIDO (Verificação em 1 minuto)

Após importar, faça este teste rápido:

```
1. Clique em "Setup & Health Checks"
2. Clique no botão "Run" (ou ▶️ Run)
3. Execute apenas 1 request: "Health - IAM (5000)"
4. Esperado: Status 200 OK
5. Se passou ✅ = Tudo certo!
```

---

## ❓ TROUBLESHOOTING

### Erro: "404 Not Found"

**Causa:** A coleção menciona 2 endpoints que não existem no código:
- `GET /api/v1/transactions/{id}`
- `GET /api/v1/transactions`

**Solução:** Consulte `ANALISE_README.md` para decisão (Opção A ou B)

### Erro: "Connection refused"

**Causa:** Serviços não estão rodando

**Solução:**
```bash
./start-all-services.sh
# Aguarde ~30 segundos para inicialização
```

### Erro: "401 Unauthorized"

**Causa:** Token JWT expirou ou não foi extraído

**Solução:**
```
1. Sempre execute "Create Test User" primeiro
2. Depois execute "Login as Test User"
3. Depois os outros requests
```

### Erro: "Variables not found"

**Causa:** As variáveis não foram definidas

**Solução:**
```
1. Verifique a aba "Variables" no Postman
2. Certifique-se que os valores estão preenchidos
3. Se vazios, rode os requests de setup primeiro
```

---

## 📊 TESTE COMPLETO (Execução Total)

Tempo estimado: **2-3 minutos**

```
✅ Setup & Health Checks ............. 5 requests (30 segundos)
✅ Authentication (IAM) .............. 4 requests (30 segundos)
✅ Credit Analysis ................... 3 requests (20 segundos)
✅ Compliance ....................... 4 requests (30 segundos)
✅ Bureau Mock ...................... 3 requests (20 segundos)
────────────────────────────────────────────────────
TOTAL: 19 requests (2-3 minutos)
```

### Resultado Esperado

```
✅ 19/19 testes passando (100%)
✅ Todas as respostas com status correto
✅ Variáveis extraídas automaticamente
✅ Fluxo completo funcionando
```

---

## 💾 SALVAR A COLEÇÃO NO POSTMAN

Depois de importar, a coleção fica salva em sua conta Postman.

**Para compartilhar com o time:**

1. Clique em "Share" (ícone de compartilhamento)
2. Selecione membros do time
3. Clique em "Share"

**Para exportar novamente:**

1. Clique no menu (⋮) da coleção
2. Selecione "Export"
3. Escolha v2.1 como formato

---

## 🎓 RECURSOS ADICIONAIS

### Documentos Relacionados

- **ANALISE_README.md** - Resumo da análise (leia se tiver problemas)
- **ANALISE_VISUAL_RESUMIDA.md** - Problemas identificados
- **RELATORIO_ANALISE_COMPLETO.md** - Análise técnica detalhada
- **PLANO_ACAO_POSTMAN.md** - Plano de ação para correções

### Postman Learning Resources

- Postman Docs: https://learning.postman.com/
- Variables Guide: https://learning.postman.com/docs/sending-requests/variables/
- Tests Guide: https://learning.postman.com/docs/writing-scripts/test-scripts/

---

## ✅ CHECKLIST DE SETUP

```
[ ] Postman instalado
[ ] Arquivo JSON baixado
[ ] Serviços rodando (./start-all-services.sh)
[ ] Arquivo importado no Postman
[ ] Health check passando (porta 5000)
[ ] Login funcionando (bearerToken extraído)
[ ] Coleção salva em sua conta Postman
[ ] Pronto para usar!
```

---

## 🚀 PRÓXIMOS PASSOS

1. **HOJE:** Importar a coleção
2. **Executar health checks** para verificar se tudo está rodando
3. **Testar autenticação** para confirmar que JWT está funcionando
4. **Executar coleção completa** para validar todos os endpoints
5. **Consultar documentação** se encontrar problemas

---

## 📞 SUPORTE

Se encontrar problemas:

1. Consulte `ANALISE_README.md` na pasta `postman/`
2. Verifique se os serviços estão rodando
3. Confirme que as portas estão corretas
4. Verifique o console de erro do Postman (View → Show Postman Console)

---

## 📝 INFORMAÇÕES DO ARQUIVO

```
Nome: Credit-Risk-Compliance-Collection-Updated.postman_collection.json
Versão: 2.0
Tamanho: 11.8 KB
Folders: 5
Requests: 19
Variáveis: 11
Testes: 50+
Data: 02 de Setembro, 2026
```

---

**Pronto para importar e usar! 🎉**

Abra seu Postman e importe o arquivo agora mesmo!

