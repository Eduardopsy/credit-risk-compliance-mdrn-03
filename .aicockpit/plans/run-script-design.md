# Plano: run-script.sh para Worktree backend-unit-tests

## Contexto

O projeto **Credit Risk Compliance Lab** é uma aplicação .NET 10 com arquitetura de microsserviços. O Agent Manager do AI Cockpit permite que o botão "Run" execute um script customizado (`run-script.sh`) quando o desenvolvedor clica nele.

Atualmente:
- `run-script` genérico existe mas apenas com comentários
- Existe `run-services.sh` no repo root que inicia tudo (services + 3 APIs)
- O worktree `backend-unit-tests` está totalmente configurado
- Necessário: script específico para o worktree que execute de forma otimizada

## Objetivo

Criar um **run-script.sh** que:
1. Execute automaticamente quando botão "Run" é clicado no Agent Manager
2. Prepare ambiente (Docker services, compilação)
3. Inicie aplicações .NET em background
4. Forneça URLs de acesso e health checks
5. Seja idempotente e seguro (não duplique processos)
6. Limpe recursos corretamente ao parar

## Requisitos de Projeto

### Stack Tecnológico
- **Runtime**: .NET 10 (C# 14)
- **Arquitetura**: Clean Architecture + DDD + Microsserviços
- **Banco**: PostgreSQL
- **Cache**: Redis
- **Message Broker**: RabbitMQ
- **Identity**: Keycloak
- **Observability**: OpenTelemetry + Prometheus + Grafana + Seq

### Módulos API
| Módulo | Projeto | Porta Sugerida |
|--------|---------|----------------|
| IAM | CreditRisk.IAM.Api | 5001 |
| Credit Analysis | CreditRisk.CreditAnalysis.Api | 5002 |
| Compliance | CreditRisk.Compliance.Api | 5003 |
| Operations Hub | CreditRisk.Operations.Server | 5004 |

### Workers (Background)
- CreditRisk.CreditAnalysis.Worker
- CreditRisk.Compliance.Worker

### Frontend
- CreditRisk.Operations.Client (Blazor WASM)

## Arquitetura da Solução

### Run-Script Responsibilities

#### 1. **Pre-flight Checks**
- Validar `.env` existe e é válido
- Verificar Docker instalado
- Verificar .NET SDK disponível
- Verificar `docker-compose` pronto

#### 2. **Docker Services Management**
- Verificar se Docker services já estão rodando
- Se offline: iniciar `docker-compose up -d`
- Health check: aguardar readiness (postgres, redis, rabbitmq, keycloak)
- Timeout: 120 segundos com retry

#### 3. **Build & Compilation**
- Compilar solução: `dotnet build` (modo Release)
- Ou compilar apenas projects necessários (otimização)
- Verificar sucesso antes de prosseguir

#### 4. **Process Management**
- Iniciar APIs em background (detached)
- Iniciar Workers em background (detached)
- Rastrear PIDs em `.run-pids`
- Implementar trap para cleanup ao sair

#### 5. **Logging & Feedback**
- Logs estruturados em `logs/run-$(date +%s).log`
- Exibir URLs de acesso em stdout
- Health check endpoints
- Status de cada serviço
- Instruções de parada

#### 6. **Signal Handling**
- Implementar SIGINT trap (Ctrl+C)
- Limpar processos background
- Encerrar docker-compose se iniciado pelo script

### Escopo Excluído
- Não fazer setup de novo worktree (já foi feito)
- Não modificar .env (apenas validar)
- Não fazer git operations
- Não executar testes automaticamente (apenas compile)

## Detalhes de Implementação

### Pre-flight Checks

```bash
1. Validar .env existe
   └─ Se não, erro + instruções

2. Validar docker disponível
   └─ Se não, erro + instalação guide

3. Validar .NET SDK
   └─ Se não, erro + setup guide

4. Carregar .env variables
   └─ Necessário para conexões
```

### Docker Services Lifecycle

```bash
# Status Check
docker-compose ps

# Se todos offline
docker-compose up -d

# Health loop (timeout 120s)
for i in {1..60}; do
  if docker-compose exec postgres pg_isready; then
    break
  fi
  sleep 2
done

# Validar readiness
docker-compose ps → todos "Up"
```

### Build Process

```bash
# Opcão 1: Build completo (mais seguro)
dotnet build CreditRiskComplianceLab.sln

# Opcão 2: Build seletivo (mais rápido)
dotnet build src/modules/iam/CreditRisk.IAM.Api/
dotnet build src/modules/credit-analysis/CreditRisk.CreditAnalysis.Api/
dotnet build src/modules/compliance/CreditRisk.Compliance.Api/
```

### Process Startup

```bash
# Rastrear PIDs
pids_file=".aicockpit/worktrees/backend-unit-tests/.run-pids"

# IAM API
dotnet run --project ... --urls "http://localhost:5001" &
echo $! >> $pids_file

# Credit Analysis API
dotnet run --project ... --urls "http://localhost:5002" &
echo $! >> $pids_file

# Compliance API
dotnet run --project ... --urls "http://localhost:5003" &
echo $! >> $pids_file

# Workers (se aplicável)
dotnet run --project ... &
echo $! >> $pids_file
```

### Cleanup Strategy

```bash
# Na saída (Ctrl+C ou conclusão)
cleanup() {
  echo "Stopping services..."
  
  # Kill PIDs
  while IFS= read -r pid; do
    kill $pid 2>/dev/null || true
  done < $pids_file
  
  # Opcional: parar docker-compose
  # docker-compose down
  
  exit 0
}

trap cleanup SIGINT
```

## Variações por Worktree

### Backend-Unit-Tests (Atual)
- Inicia: IAM API + Credit API + Compliance API
- Não inicia workers (para testes isolados)
- Portas: 5001, 5002, 5003

### Possíveis Futuros
- **Credit-Feature**: Apenas Credit Analysis + Compliance
- **Full-Stack**: Tudo + Frontend
- **Workers-Only**: Apenas workers para processamento

→ Script pode aceitar modo: `run-script.sh [iam|credit|compliance|full|workers]`

## Estrutura do Script

```bash
#!/bin/bash
set -euo pipefail

# ┌─────────────────────────────────────────────────┐
# │ 1. CONFIGURATION & ENVIRONMENT                  │
# └─────────────────────────────────────────────────┘

# ┌─────────────────────────────────────────────────┐
# │ 2. UTILITY FUNCTIONS                            │
# └─────────────────────────────────────────────────┘

# ┌─────────────────────────────────────────────────┐
# │ 3. PRE-FLIGHT CHECKS                            │
# └─────────────────────────────────────────────────┘

# ┌─────────────────────────────────────────────────┐
# │ 4. DOCKER SERVICES MANAGEMENT                   │
# └─────────────────────────────────────────────────┘

# ┌─────────────────────────────────────────────────┐
# │ 5. BUILD COMPILATION                            │
# └─────────────────────────────────────────────────┘

# ┌─────────────────────────────────────────────────┐
# │ 6. PROCESS STARTUP                              │
# └─────────────────────────────────────────────────┘

# ┌─────────────────────────────────────────────────┐
# │ 7. LOGGING & FEEDBACK                           │
# └─────────────────────────────────────────────────┘

# ┌─────────────────────────────────────────────────┐
# │ 8. SIGNAL HANDLING & CLEANUP                    │
# └─────────────────────────────────────────────────┘

# ┌─────────────────────────────────────────────────┐
# │ 9. MAIN EXECUTION LOOP                          │
# └─────────────────────────────────────────────────┘
```

## Casos de Uso

### Caso 1: Primeira Execução do Run
1. User clica "Run" no Agent Manager
2. run-script executa
3. Valida .env, Docker
4. Inicia docker-compose
5. Compila código
6. Inicia 3 APIs
7. Exibe URLs
8. Aguarda Ctrl+C

### Caso 2: Reexecução (Services já rodando)
1. User clica "Run" novamente
2. run-script detecta services já up
3. Pula inicialização de docker-compose
4. Verifica se .NET processes ainda existem
5. Se mortos: reinicia
6. Se vivos: aguarda entrada do user

### Caso 3: Parada Limpa
1. User pressiona Ctrl+C
2. Signal handler executa cleanup
3. Mata processos background
4. Deixa docker-compose rodando (para próxima sessão)
5. Exibe resumo
6. Sai com exit 0

## Melhorias Futuras (Fase 2)

1. **Process Monitoring**: Verificar se processos ainda estão vivos
2. **Log Aggregation**: Agregar logs de todas as APIs
3. **Health Dashboard**: Exibir status de readiness
4. **Port Derivation**: Usar portas derivadas do worktree name
5. **Per-Worktree Docker**: Iniciar docker-compose isolado
6. **Parallel Startup**: Iniciar APIs em paralelo (mais rápido)
7. **Debug Mode**: Opção para attach debugger
8. **Frontend Integration**: Incluir frontend build e serve

## Checklist de Validação

- [ ] Script criado em `.aicockpit/run-script.sh`
- [ ] Script é executável (`chmod +x`)
- [ ] Testa com worktree `backend-unit-tests`
- [ ] Agent Manager consegue executar
- [ ] Docker services iniciam corretamente
- [ ] Todos os 3 APIs iniciam
- [ ] URLs exibidas corretamente
- [ ] Health checks funcionam
- [ ] Ctrl+C para corretamente
- [ ] Processes são limpas
- [ ] Idempotente (executar 2x funciona)
- [ ] Logs estruturados
- [ ] Documentado com comentários

## Questões para o User

1. **Modo de Inicialização**: 
   - Sempre iniciar os 3 APIs?
   - Ou permitir seleção por modo (iam | credit | full)?

2. **Docker Compose**:
   - Parar docker-compose ao sair (Ctrl+C)?
   - Ou deixar rodando para próxima sessão?

3. **Build Mode**:
   - Build completo (safer)?
   - Ou build seletivo (faster)?
   - Ou incremental (detectar mudanças)?

4. **Logging**:
   - Arquivos de log separados por API?
   - Ou agregados em um único log?

5. **Frontend**:
   - Incluir frontend Blazor no futuro?
   - Agora só focar em APIs .NET?

---

**Versão**: 1.0.0  
**Criado**: 2026-08-20  
**Status**: Planning Phase
