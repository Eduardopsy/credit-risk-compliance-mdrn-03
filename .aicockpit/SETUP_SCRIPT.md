# Setup Script — Documentação

## Visão Geral

O `setup-script.sh` é um script bash que executa automaticamente quando um novo worktree é criado ou importado no Agent Manager. Ele configura o ambiente do worktree, preparando variáveis de ambiente, estrutura de diretórios e validações de dependências.

**Localização**: `.aicockpit/setup-script.sh`

**Timeout**: 5 minutos (limite do Agent Manager)

## Responsabilidades

O script executa as seguintes tarefas, em ordem:

### 1. Validação de Caminhos
- Verifica se `WORKTREE_PATH` existe e é um diretório
- Verifica se `REPO_PATH` existe e é um diretório
- Extrai identificadores únicos do worktree

### 2. Validação de Docker
- Verifica instalação de `docker` e `docker-compose`
- Verifica se o daemon Docker está rodando
- Retorna erro se algum pré-requisito não for atendido

### 3. Configuração de `.env`
- **Se não existe**: Copia `.env.example` do repo root
- **Se existe**: Valida presença de variáveis obrigatórias
- **Valida**: Presença de `POSTGRES_*`, `REDIS_*`, `RABBITMQ_*`, `KEYCLOAK_*`

### 4. Estrutura de Diretórios
- Cria `logs/` para saída de serviços
- Cria `build/` para artefatos de compilação

### 5. Verificação de Estado do Worktree
- Extrai branch Git atual
- Verifica versão do .NET SDK instalado

### 6. Health Checks de Serviços (Graceful)
- Verifica se containers Docker estão rodando (postgres, redis, rabbitmq, keycloak)
- **Não falha** se serviços não estiverem disponíveis
- Sugere próximos passos se necessário

## Uso

### Uso Automático (Agent Manager)

Quando você cria ou importa um worktree via Agent Manager UI:

1. User clica **"+"** ou **"Import"**
2. Especifica branch/caminho
3. Agent Manager cria worktree em `.aicockpit/worktrees/<name>/`
4. **Setup script executa automaticamente**
5. Agent Manager inicia sessão do agente

### Uso Manual (para testes)

```bash
export WORKTREE_PATH="/path/to/worktree"
export REPO_PATH="/path/to/repo"

.aicockpit/setup-script.sh
```

## Variáveis de Ambiente

### Entrada (do Agent Manager)

| Variável | Descrição | Exemplo |
|----------|-----------|---------|
| `WORKTREE_PATH` | Caminho absoluto do worktree | `/home/user/.aicockpit/worktrees/backend-unit-tests` |
| `REPO_PATH` | Caminho absoluto do repo root | `/home/user/projects/credit-risk-lab` |

### Configuração (arquivo `.env`)

O script copia e valida as seguintes variáveis obrigatórias:

```
POSTGRES_USER
POSTGRES_PASSWORD
POSTGRES_DB
POSTGRES_CONNECTION_STRING

REDIS_PASSWORD
REDIS_CONNECTION_STRING

RABBITMQ_USER
RABBITMQ_PASSWORD

KEYCLOAK_ADMIN_USER
KEYCLOAK_ADMIN_PASSWORD
```

## Outputs

### Sucesso (exit code: 0)

```
╔════════════════════════════════════════════════════════════════════╗
║  AI Cockpit Agent Manager — Worktree Setup                         ║
║  Credit Risk Compliance Lab                                        ║
╚════════════════════════════════════════════════════════════════════╝

[✓] Worktree setup completed successfully!

NEXT STEPS
─────────────────────────────────────────────────────────────────

[INFO] 1. Review .env configuration: ...
[INFO] 2. Start Docker services: docker-compose up -d
[INFO] 3. Run the dev environment: Use 'Run' button in Agent Manager
[INFO] 4. View logs: tail -f .../logs/*.log
```

### Falha (exit code: 1)

```
[ERROR] Setup FAILED

TROUBLESHOOTING
─────────────────────────────────────────────────────────────────

[INFO] • Docker not installed? Visit: https://docs.docker.com/get-docker/
[INFO] • .env.example missing? Ensure repo root contains .env.example
[INFO] • Directory not creatable? Check permissions
```

## Logs

Todos os logs incluem timestamps:

```
[INFO] 2026-08-20 11:15:28 Copying .env from repo root...
[WARN] 2026-08-20 11:15:29 Service 'postgres' not running
[ERROR] 2026-08-20 11:15:30 Docker not found
[✓] Worktree setup completed successfully!
```

### Níveis de Log

- `[INFO]` — Informações operacionais
- `[WARN]` — Avisos (não impedem conclusão)
- `[ERROR]` — Erros críticos (impedem conclusão)
- `[✓]` — Sucesso / operação concluída

## Tratamento de Erros

| Cenário | Comportamento |
|---------|---------------|
| `WORKTREE_PATH` não existe | Retorna erro 1, exibe troubleshooting |
| `REPO_PATH` não existe | Retorna erro 1, exibe troubleshooting |
| Docker não instalado | Retorna erro 1, link para instalação |
| Docker daemon offline | Continua (graceful degradation) |
| `.env.example` não existe | Retorna erro 1, instruções manuais |
| Diretório não criável | Retorna erro 1, verifica permissões |
| Serviços Docker offline | Continua (aviso + próximos passos) |

## Características Especiais

### Idempotência

O script é seguro executar múltiplas vezes:

- Se `.env` já existe, valida sem sobrescrever
- Se `logs/` e `build/` existem, pula criação
- Retorna sucesso (exit 0) em ambas execuções

### Graceful Degradation

Se Docker não está rodando ou serviços não estão up:

- Setup **continua** (não falha)
- Loga avisos explicativos
- Sugere próximos passos
- Permite que user inicie serviços depois

### Isolamento de Worktree

Cada worktree:

- Tem seu próprio `.env` (copiado do repo)
- Tem seus próprios `logs/` e `build/`
- Usa serviços Docker compartilhados (mesmo host)
- Não interfere com outros worktrees

## Integração com Agent Manager

### Arquivo de Configuração

Agent Manager procura por scripts nesta ordem:

**Linux/macOS:**
1. `.aicockpit/setup-script` (sem extensão)
2. `.aicockpit/setup-script.sh`

**Windows:**
1. `.aicockpit/setup-script.ps1`
2. `.aicockpit/setup-script.cmd`
3. `.aicockpit/setup-script.bat`

### Ciclo de Vida

```
User cria worktree
         ↓
Agent Manager cria diretório
         ↓
Agent Manager copia .env.* (se existir)
         ↓
Agent Manager executa setup-script.sh  ← AQUI
         ↓
Agent Manager aguarda sucesso/falha
         ↓
Agent Manager inicia sessão do agente
```

## Troubleshooting

### "Docker not found"

```bash
# Instale Docker
curl -fsSL https://get.docker.com | sh

# Ou veja: https://docs.docker.com/get-docker/
```

### ".env.example not found"

O arquivo `.env.example` deve existir no repo root:

```bash
# Verifique
ls -la /path/to/repo/.env.example

# Se não existe, crie a partir de setup.md
cp infra/templates/.env.example .env.example
```

### "Services not running"

Serviços Docker não estão em execução. Para iniciar:

```bash
cd /path/to/repo
docker-compose up -d
```

Verifique status:

```bash
docker-compose ps
```

### Script timeout (> 5 min)

Se o script leva mais de 5 minutos:

1. Verifique Docker daemon lentidão (`docker ps` demora?)
2. Verifique I/O do disco (`du -sh /path/to/worktree`)
3. Reduza passos opcionais em futuras versões

## Próximas Execuções do Setup

Você pode re-executar o setup manualmente a qualquer momento:

```bash
export WORKTREE_PATH="$(pwd)"
export REPO_PATH="$(git rev-parse --show-toplevel)"

.aicockpit/setup-script.sh
```

Ou dentro de um worktree:

```bash
cd /path/to/worktree
/path/to/repo/.aicockpit/setup-script.sh
```

## Melhorias Futuras

- [ ] Port derivation (portas isoladas por worktree)
- [ ] Per-worktree docker-compose
- [ ] Database seeding
- [ ] Certificate generation local
- [ ] Health endpoint `/health/setup`
- [ ] Metrics e telemetry

---

**Versão**: 1.0.0  
**Criado**: 2026-08-20  
**Status**: Production
