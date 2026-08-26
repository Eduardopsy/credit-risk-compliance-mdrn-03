# Plano Final: run-script.sh para Worktree backend-unit-tests

## Contexto

O projeto **Credit Risk Compliance Lab** é uma aplicação .NET 10 com arquitetura de microsserviços. O Agent Manager do AI Cockpit permite que o botão "Run" execute um script customizado (`run-script.sh`) para iniciar o ambiente de desenvolvimento.

**Estado Atual**:
- ✅ setup-script.sh implementado e testado
- ✅ Worktree backend-unit-tests totalmente configurado
- ⏳ run-script.sh genérico apenas com template
- ❌ Necessário: implementar run-script.sh otimizado

## Objetivo

Criar um **run-script.sh** que, quando botão "Run" é clicado no Agent Manager:
1. ✅ Valida pré-requisitos
2. ✅ Inicia Docker services (se offline)
3. ✅ Compila solução .NET completa
4. ✅ Inicia 3 APIs em sequência com logs separados
5. ✅ Fornece URLs de acesso e instruções
6. ✅ Limpa recursos corretamente ao parar (Ctrl+C)

## Decisões Finalizadas

### ✅ 1. Modo de Inicialização
- **Decisão**: Sempre iniciar os 3 APIs (IAM, Credit Analysis, Compliance)
- **Motivo**: Worktree é para testes completos
- **Sem seleção por modo**: Fixo para backend-unit-tests

### ✅ 2. Gestão de Docker Compose
- **Decisão**: PARAR docker-compose ao sair (Ctrl+C)
- **Motivo**: Libera recursos, environment limpo
- **Limpeza**: `docker-compose down` na saída

### ✅ 3. Build Mode
- **Decisão**: Build completo (`dotnet build CreditRiskComplianceLab.sln`)
- **Motivo**: Mais seguro, garante tudo compilado
- **Alternativa não escolhida**: Build seletivo (mais rápido mas menos seguro)

### ✅ 4. Logging
- **Decisão**: Arquivos separados por API
- **Formato**: `logs/iam.log`, `logs/credit.log`, `logs/compliance.log`
- **Motivo**: Mais fácil de debugar, logs claros
- **Alternativa não escolhida**: Logs agregados (menos legível)

### ✅ 5. Frontend
- **Decisão**: Agora focar apenas em APIs .NET
- **Motivo**: Backend primeiro, frontend na Fase 2
- **Futuro**: Integrar Blazor WASM quando needed

### ✅ 6. Startup
- **Decisão**: Sequencial (não paralelo)
- **Tempo**: ~30-45 segundos
- **Motivo**: Mais fácil debugar, logs sequenciais claros
- **Alternativa não escolhida**: Paralelo (mais rápido mas logs misturados)

## Arquitetura de Implementação

### 1. Pré-flight Checks
```
✓ .env existe e é válido
✓ Docker instalado
✓ Docker Compose instalado
✓ .NET SDK disponível (8.0.130+)
✓ Solução .NET pode ser compilada
```

### 2. Docker Services Management
```
✓ Verificar se services já estão rodando
✓ Se offline: docker-compose up -d
✓ Health check loop (timeout 120s):
  - postgres (pg_isready)
  - redis (redis-cli ping)
  - rabbitmq (docker-compose exec)
  - keycloak (tcp check)
✓ Validar readiness antes de prosseguir
```

### 3. Build Compilation
```
✓ Build completo: dotnet build CreditRiskComplianceLab.sln
✓ Log para stdout + arquivo
✓ Verificar exit code
✓ Se falha: erro + instruções
```

### 4. Sequential Startup (3 APIs)

**1️⃣ IAM API**
```bash
Port: 5001
Project: src/modules/iam/CreditRisk.IAM.Api/CreditRisk.IAM.Api.csproj
Command: dotnet run --urls "http://localhost:5001"
Log: logs/iam.log
```

**2️⃣ Credit Analysis API**
```bash
Port: 5002
Project: src/modules/credit-analysis/CreditRisk.CreditAnalysis.Api/CreditRisk.CreditAnalysis.Api.csproj
Command: dotnet run --urls "http://localhost:5002"
Log: logs/credit.log
```

**3️⃣ Compliance API**
```bash
Port: 5003
Project: src/modules/compliance/CreditRisk.Compliance.Api/CreditRisk.Compliance.Api.csproj
Command: dotnet run --urls "http://localhost:5003"
Log: logs/compliance.log
```

### 5. Process Tracking
```
✓ Rastrear PIDs em .run-pids
✓ Um PID por linha
✓ Usar para cleanup later
```

### 6. Feedback & Logging
```
Stdout:
  ✓ Status de cada etapa
  ✓ URLs de acesso
  ✓ Health check endpoints
  ✓ Instruções de parada

Files:
  ✓ logs/iam.log → saída IAM API
  ✓ logs/credit.log → saída Credit Analysis API
  ✓ logs/compliance.log → saída Compliance API
  ✓ .run-pids → rastreamento de PIDs
```

### 7. Signal Handling & Cleanup
```
Ctrl+C (SIGINT):
  ✓ Chamar cleanup handler
  ✓ Kill todos os PIDs em .run-pids
  ✓ docker-compose down (parar services)
  ✓ Limpar .run-pids
  ✓ Exibir resumo
  ✓ Exit 0
```

## Estrutura do Script

```bash
#!/bin/bash
set -euo pipefail

# ┌─────────────────────────────────────────────────┐
# │ 1. CONFIGURATION & ENVIRONMENT                  │
# │    - Variáveis globais                          │
# │    - Paths                                      │
# │    - Colors para output                         │
# └─────────────────────────────────────────────────┘

# ┌─────────────────────────────────────────────────┐
# │ 2. UTILITY FUNCTIONS                            │
# │    - log_info, log_warn, log_error, log_success │
# │    - check_command, check_file                  │
# │    - cleanup handler                            │
# └─────────────────────────────────────────────────┘

# ┌─────────────────────────────────────────────────┐
# │ 3. SETUP SIGNAL HANDLERS                        │
# │    - trap cleanup SIGINT                        │
# └─────────────────────────────────────────────────┘

# ┌─────────────────────────────────────────────────┐
# │ 4. PRE-FLIGHT CHECKS                            │
# │    - Validar .env                               │
# │    - Validar Docker                             │
# │    - Validar .NET SDK                           │
# │    - Load variables do .env                     │
# └─────────────────────────────────────────────────┘

# ┌─────────────────────────────────────────────────┐
# │ 5. DOCKER SERVICES MANAGEMENT                   │
# │    - Verificar status                           │
# │    - Iniciar se offline                         │
# │    - Health checks                              │
# │    - Timeout handling                           │
# └─────────────────────────────────────────────────┘

# ┌─────────────────────────────────────────────────┐
# │ 6. BUILD COMPILATION                            │
# │    - dotnet build CreditRiskComplianceLab.sln   │
# │    - Verificar sucesso                          │
# │    - Log output                                 │
# └─────────────────────────────────────────────────┘

# ┌─────────────────────────────────────────────────┐
# │ 7. SEQUENTIAL API STARTUP                       │
# │    - IAM API (5001)                             │
# │    - Credit Analysis API (5002)                 │
# │    - Compliance API (5003)                      │
# │    - Rastrear PIDs                              │
# │    - Aguardar readiness                         │
# └─────────────────────────────────────────────────┘

# ┌─────────────────────────────────────────────────┐
# │ 8. DISPLAY FEEDBACK                             │
# │    - URLs de acesso                             │
# │    - Health endpoints                           │
# │    - Instruções                                 │
# │    - Log files locations                        │
# └─────────────────────────────────────────────────┘

# ┌─────────────────────────────────────────────────┐
# │ 9. MAIN LOOP                                    │
# │    - Aguardar Ctrl+C                            │
# │    - Cleanup automático ao sair                 │
# └─────────────────────────────────────────────────┘
```

## Fluxo de Execução

### Caso 1: Primeira Execução
```
User clica "Run"
    ↓
run-script.sh inicia
    ↓
[Validações] .env ✓, Docker ✓, .NET ✓
    ↓
[Docker] docker-compose up -d (se offline)
    ↓
[Health] Aguardar postgres, redis, rabbitmq, keycloak (max 120s)
    ↓
[Build] dotnet build CreditRiskComplianceLab.sln
    ↓
[IAM] dotnet run ... (porta 5001, logs/iam.log)
    ↓
[Credit] dotnet run ... (porta 5002, logs/credit.log)
    ↓
[Compliance] dotnet run ... (porta 5003, logs/compliance.log)
    ↓
[Feedback] Exibir URLs + instruções
    ↓
[Wait] Aguardar Ctrl+C
```

### Caso 2: Reexecução (Services já rodando)
```
User clica "Run" novamente
    ↓
run-script.sh inicia
    ↓
[Validações] OK
    ↓
[Docker] docker-compose ps → todos "Up"
    ↓
[Skip Docker] Services já ok, pula restart
    ↓
[Build] dotnet build (verificar mudanças)
    ↓
[Startup] Mesmas 3 APIs
    ↓
[Feedback] URLs + instruções
```

### Caso 3: Parada (Ctrl+C)
```
User pressiona Ctrl+C
    ↓
Signal handler (SIGINT)
    ↓
[Cleanup] Kill PIDs das 3 APIs
    ↓
[Cleanup] docker-compose down
    ↓
[Cleanup] Remover .run-pids
    ↓
[Feedback] Exibir resumo
    ↓
[Exit] exit 0
```

## Saída Esperada

### Stdout (Console)
```
╔════════════════════════════════════════════════════════════╗
║  Credit Risk Compliance Lab — Dev Environment             ║
║  Worktree: backend-unit-tests                             ║
╚════════════════════════════════════════════════════════════╝

[INFO] Validating prerequisites...
  ✓ .env found and valid
  ✓ Docker available (v29.1.3)
  ✓ .NET SDK available (v8.0.130)

[INFO] Docker services...
  ✓ Services already running (4/4 healthy)

[INFO] Building solution...
  → dotnet build CreditRiskComplianceLab.sln
  ✓ Build successful

[INFO] Starting APIs (sequential)...
  
  [1/3] IAM API
    → http://localhost:5001
    → logs: logs/iam.log
    ✓ Started (PID: 12345)
    
  [2/3] Credit Analysis API
    → http://localhost:5002
    → logs: logs/credit.log
    ✓ Started (PID: 12346)
    
  [3/3] Compliance API
    → http://localhost:5003
    → logs: logs/compliance.log
    ✓ Started (PID: 12347)

[SUCCESS] All APIs running!

URLs:
  • IAM API:              http://localhost:5001
  • Credit Analysis API:  http://localhost:5002
  • Compliance API:       http://localhost:5003

Health Endpoints:
  • IAM:              http://localhost:5001/health
  • Credit Analysis:  http://localhost:5002/health
  • Compliance:       http://localhost:5003/health

Logs:
  • IAM:              logs/iam.log
  • Credit Analysis:  logs/credit.log
  • Compliance:       logs/compliance.log

Press Ctrl+C to stop all services and cleanup.
```

### Ao Pressionar Ctrl+C
```
[INFO] Stopping services...
  ✓ Stopped IAM API (PID 12345)
  ✓ Stopped Credit Analysis API (PID 12346)
  ✓ Stopped Compliance API (PID 12347)

[INFO] Stopping Docker services...
  ✓ docker-compose down completed

[SUCCESS] Cleanup complete. Goodbye!
```

## Arquivos Gerados/Modificados

### Criados
- `.aicockpit/run-script.sh` (implementação)
- `logs/iam.log` (IAM API output)
- `logs/credit.log` (Credit Analysis API output)
- `logs/compliance.log` (Compliance API output)
- `.run-pids` (PID tracking)

### Não Modificados
- `.env` (apenas validado)
- `docker-compose.yml` (executado mas não modificado)
- `CreditRiskComplianceLab.sln` (compilado mas não modificado)

## Melhorias Futuras (Fase 2)

1. **Process Monitoring**: Verificar se processos ainda estão vivos
2. **Log Aggregation**: Opção de agregar logs em único arquivo
3. **Health Dashboard**: Exibir status contínuo
4. **Port Derivation**: Portas derivadas do worktree name
5. **Parallel Startup**: Iniciar APIs em paralelo (mais rápido)
6. **Debug Mode**: Opção `--debug` para debugger attachment
7. **Frontend Integration**: Incluir Blazor WASM build/serve
8. **Worker Support**: Iniciar CreditAnalysis.Worker e Compliance.Worker

## Checklist de Validação

- [ ] Script criado em `.aicockpit/run-script.sh`
- [ ] Script é executável (`chmod +x`)
- [ ] Testa com worktree `backend-unit-tests`
- [ ] Agent Manager consegue executar
- [ ] Docker services iniciam corretamente
- [ ] Build completo funciona
- [ ] Todos os 3 APIs iniciam em sequência
- [ ] Logs separados por API gerados
- [ ] URLs exibidas corretamente
- [ ] Health endpoints funcionam
- [ ] Ctrl+C para corretamente (cleanup)
- [ ] docker-compose down executado na saída
- [ ] Processes limpas
- [ ] Idempotente (executar 2x funciona)
- [ ] Documentado com inline comments
- [ ] README.md ou SETUP.md atualizado

## Próximos Passos (Implementação)

1. ✅ Plano finalizado com todas decisões
2. ⏳ Implementar run-script.sh
3. ⏳ Testar com worktree backend-unit-tests
4. ⏳ Validar com Agent Manager
5. ⏳ Documentação de uso
6. ⏳ Fase 2: Melhorias listadas acima

---

**Versão**: 1.0.0  
**Data**: 2026-08-20  
**Status**: ✅ Ready for Implementation  
**Responsável**: Jorge Eduardo  
**Worktree**: backend-unit-tests
