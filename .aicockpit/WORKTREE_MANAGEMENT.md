# Agent Manager Worktree Management — Guia Completo

## Visão Geral

O **Credit Risk Compliance Lab** usa o AI Cockpit **Agent Manager** para gerenciar múltiplos worktrees (branches paralelos). Cada worktree funciona de forma isolada, permitindo desenvolvimento paralelo sem conflitos.

O fluxo completo:

```
┌─────────────────────────────────────────────────────────────────┐
│ 1. User cria novo worktree via Agent Manager UI                │
│    (ou importa um worktree existente)                           │
└─────────────────────────────────────────────────────────────────┘
                               ↓
┌─────────────────────────────────────────────────────────────────┐
│ 2. Agent Manager cria diretório em .aicockpit/worktrees/<name>/ │
│    e copia branch para lá                                       │
└─────────────────────────────────────────────────────────────────┘
                               ↓
┌─────────────────────────────────────────────────────────────────┐
│ 3. Agent Manager executa setup-script.sh                        │
│    (configura ambiente, .env, diretórios)                       │
└─────────────────────────────────────────────────────────────────┘
                               ↓
┌─────────────────────────────────────────────────────────────────┐
│ 4. Agent Manager inicia sessão do agente                        │
│    (pronto para desenvolvimento)                                │
└─────────────────────────────────────────────────────────────────┘
                               ↓
┌─────────────────────────────────────────────────────────────────┐
│ 5. User clica "Run" para iniciar dev environment               │
│    (executa run-script.sh)                                      │
└─────────────────────────────────────────────────────────────────┘
```

## Scripts Principais

### 1. setup-script.sh

**Executado**: Uma única vez, quando worktree é criado/importado

**Responsabilidades**:
- ✓ Validar Docker instalado
- ✓ Copiar `.env` do repo root
- ✓ Criar estrutura de diretórios (`logs/`, `build/`)
- ✓ Validar variáveis de ambiente obrigatórias
- ✓ Health check de serviços Docker (graceful)
- ✓ Exibir próximos passos

**Status**: Implementado ✓

**Localização**: `.aicockpit/setup-script.sh`

**Documentação**: `.aicockpit/SETUP_SCRIPT.md`

### 2. run-script.sh

**Executado**: Quando user clica "Run" no Agent Manager

**Responsabilidades** (a implementar):
- [ ] Iniciar Docker Compose services (se offline)
- [ ] Compilar projetos .NET
- [ ] Iniciar dev servers
- [ ] Exibir URLs de acesso

**Status**: Template genérico (versão atualizada em desenvolvimento)

**Localização**: `.aicockpit/run-script`

## Estrutura de Worktrees

```
.aicockpit/
├── setup-script.sh                   # ← Executa ao criar worktree
├── run-script                        # ← Executa ao clicar "Run"
├── agent-manager.json                # ← Estado do Agent Manager
│
└── worktrees/
    ├── backend-unit-tests/           # ← Worktree exemplo
    │   ├── .git/                     # Git checkout da branch
    │   ├── .env                      # Copiado de .env.example
    │   ├── logs/                     # Criado pelo setup-script
    │   ├── build/                    # Criado pelo setup-script
    │   ├── src/                      # Código da branch
    │   └── ...
    │
    └── feature-xyz/                  # ← Outro worktree
        ├── .git/
        ├── .env
        ├── logs/
        ├── build/
        └── ...
```

## Fluxo de Desenvolvimento Paralelo

### Cenário: 2 desenvolvedores, 2 features em paralelo

**Developer A** (feature/aml-rules):
```bash
1. Cria worktree "aml-rules" via Agent Manager
   → setup-script.sh executa automaticamente
   → .env copiado, ambiente pronto

2. Clica "Run"
   → run-script.sh inicia dev environment
   → Pode compilar, testar, debugar

3. Faz commits, cria PR
   → Agent Manager integra worktree
```

**Developer B** (feature/credit-scoring):
```bash
1. Cria worktree "credit-scoring" em paralelo
   → setup-script.sh executa (isolado de A)
   → Seu próprio .env, logs/, build/

2. Clica "Run"
   → run-script.sh inicia seu dev environment
   → Não interfere com Developer A

3. Ambos desenvolvem em paralelo
   → Compartilham mesmos serviços Docker (postgres, redis, etc.)
   → Não compartilham build output, logs, ou estado local
```

## Integração com Agent Manager

### agent-manager.json

Rastreia estado de worktrees:

```json
{
  "worktrees": {
    "wt-1787233880629-2": {
      "branch": "backend-unit-tests",
      "path": ".../backend-unit-tests",
      "parentBranch": "master",
      "createdAt": "2026-08-20T13:51:20.629Z",
      "label": "backend-unit-tests",
      "branchOwned": true
    }
  },
  "sessions": {
    "ses_fe08f342effeuJ56BL3W5eMAha": {
      "worktreeId": "wt-1787233880629-2",
      "createdAt": "2026-08-20T13:51:21.102Z"
    }
  }
}
```

**O setup-script não modifica este arquivo** — Agent Manager gerencia completamente.

## Variáveis de Ambiente

### Fluxo de `.env`

```
1. Repo root contém: .env.example (committed)
   └─ Template com todas as variáveis

2. Agent Manager copia .env.* antes de setup
   └─ Se houver .env.local, copia para worktree

3. setup-script.sh executa
   └─ Se worktree não tem .env:
      ├─ Copia .env.example → .env
      └─ Valida variáveis obrigatórias

4. Worktree tem seu próprio .env (não committed)
   └─ User configura passwords/secrets localmente
```

### Variáveis Validadas

O setup-script valida presença de:

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

Se alguma faltar, é logada como aviso (não é erro fatal).

## Dependências Externas

### Docker Compose Services

Todos os worktrees compartilham mesmos serviços:

```
┌──────────────────────────────────────────────────┐
│ Docker Compose (repo root)                       │
│                                                  │
│  crcl-postgres       ← Compartilhado            │
│  crcl-redis          ← Compartilhado            │
│  crcl-rabbitmq       ← Compartilhado            │
│  crcl-keycloak       ← Compartilhado            │
│  crcl-prometheus     ← Compartilhado            │
│  crcl-grafana        ← Compartilhado            │
│  crcl-seq            ← Compartilhado            │
│                                                  │
└──────────────────────────────────────────────────┘
```

Cada worktree acessa mesmos serviços via hostname (docker network).

### Por que compartilhar?

- ✓ Economia de recursos (não rodam múltiplas instâncias)
- ✓ Banco de dados único (dados consistentes)
- ✓ Cache unificado (Redis compartilhado)
- ✓ Simplicidade (um docker-compose.yml)

### Trade-off

- ✗ Não há isolamento de dados entre worktrees
- ✗ Se um worktree corromper DB, afeta todos

**Solução futura**: Per-worktree docker-compose (Fase 2)

## Checklist de Implementação

- [x] setup-script.sh implementado
- [x] setup-script.sh testado (idempotente)
- [x] .env.example criado
- [x] Documentação SETUP_SCRIPT.md
- [ ] run-script.sh otimizado para .NET
- [ ] Teste de novo worktree (criação real)
- [ ] Teste de worktree import
- [ ] Documentação em AGENTS.md (se necessário)
- [ ] Per-worktree docker-compose (Fase 2)
- [ ] Port derivation (Fase 2)

## Como Usar

### Criar novo worktree

```
1. Abra VS Code com projeto
2. Clique em Agent Manager (sidebar)
3. Clique "+" (novo worktree)
4. Especifique branch (ex: "feature/aml-rules")
5. Agent Manager cria worktree
   → setup-script.sh executa
   → Ambiente pronto
6. Clique "Run" para iniciar dev
```

### Importar worktree existente

```
1. Agent Manager → worktrees
2. Clique "..." (menu)
3. "Import" → selecione caminho local
4. Agent Manager importa
   → setup-script.sh executa
   → Ambiente reconhecido
```

### Executar setup manualmente

```bash
export WORKTREE_PATH="/path/to/worktree"
export REPO_PATH="/path/to/repo"

.aicockpit/setup-script.sh
```

## Troubleshooting

### Setup falha com "Docker not found"

```bash
# Instale Docker
curl -fsSL https://get.docker.com | sh

# Verifique
docker --version
docker-compose --version
```

### Setup falha com ".env.example not found"

```bash
# Verifique arquivo existe no repo root
ls -la /path/to/repo/.env.example

# Se não existe, crie manualmente
cat > .env.example << 'EOF'
POSTGRES_USER=user
POSTGRES_PASSWORD=password
...
EOF
```

### Setup completa mas "Services not running"

É esperado! Docker services podem não estar rodando durante setup.

Para iniciar serviços:

```bash
cd /path/to/repo
docker-compose up -d
```

Verifique status:

```bash
docker-compose ps
```

### Worktree não aparece em Agent Manager

1. Verifique em `.aicockpit/agent-manager.json`
2. Verifique diretório existe
3. Verifique `.git/` existe (é um checkout git)
4. Reinicie VS Code

## Próximas Melhorias

### Fase 2: Per-Worktree Isolation

- [ ] Gerar `docker-compose.worktree.yml` por worktree
- [ ] Port mapping derivado de worktree name
- [ ] Database por worktree (isolamento completo)
- [ ] Redis database por worktree

### Fase 2: run-script.sh Otimizado

- [ ] Detectar .NET projects
- [ ] Compilar com `dotnet build`
- [ ] Iniciar múltiplos serviços em paralelo
- [ ] Health check endpoints
- [ ] Exibir URLs de acesso

### Fase 2: Health Endpoint

- [ ] Criar `/health/setup` em cada API
- [ ] Validar estado pós-setup
- [ ] Telemetry e métricas

---

**Versão**: 1.0.0  
**Criado**: 2026-08-20  
**Status**: Production
