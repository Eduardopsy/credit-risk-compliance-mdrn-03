# Plano: setup-script.sh para Gerenciamento de Worktrees

## Contexto

O projeto **Credit Risk Compliance Lab** é uma aplicação .NET 10 com arquitetura de microsserviços, containerizada com Docker Compose. O projeto utiliza o **Agent Manager** do AI Cockpit para gerenciar múltiplos worktrees (checkouts de branches separadas para trabalho paralelo).

Atualmente:
- Existe 1 worktree ativo: `backend-unit-tests` na branch `backend-unit-tests`
- O `agent-manager.json` rastreia estado, sessões e ordem de worktrees
- Há um `run-script` genérico (não otimizado para múltiplos worktrees)
- Não existe um `setup-script.sh` — é necessário criar um

## Objetivo

Criar um **setup-script.sh** que:
1. Execute uma única vez quando um worktree é criado/importado
2. Configure variáveis de ambiente isoladas por worktree
3. Prepare recursos locais (arquivos `.env`, certificados, etc.)
4. Evite conflitos de portas/recursos entre worktrees paralelos
5. Verifique readiness de dependências externas (Docker Compose)
6. Forneça feedback claro sobre configuração

## Requisitos de Projeto

### Contexto Técnico
- **Runtime**: .NET 10 LTS
- **Linguagem**: C# 14
- **Frontend**: Blazor WebAssembly
- **ORM**: Entity Framework Core + Dapper
- **Message Broker**: RabbitMQ
- **Database**: PostgreSQL
- **Cache**: Redis
- **Identity Provider**: Keycloak
- **Observability**: OpenTelemetry + Prometheus + Grafana + Seq
- **Containerização**: Docker Compose (local dev) / Kubernetes (prod)

## Arquitetura da Solução

### Setup Script Responsibilities

#### 1. **Inicialização de Ambiente**
- Recebe `WORKTREE_PATH` (caminho do worktree) e `REPO_PATH` (raiz do repo)
- Valida e cria estrutura de diretórios necessários

#### 2. **Gestão de Arquivo `.env`**
- Se `.env` não existe: copia de `.env.example`
- Se `.env` existe: valida variáveis obrigatórias
- Não altera arquivo existente (idempotente)

#### 3. **Validação de Dependências**
- Verificar se Docker e Docker Compose estão instalados
- Verificar se daemon está rodando
- Verificar readiness de serviços principais

#### 4. **Preparação de Recursos Locais**
- Criar diretórios para logs, build output
- Configurar permissões de arquivo

#### 5. **Health Checks e Feedback**
- Retornar status 0 (sucesso) ou 1 (falha)
- Imprimir relatório de inicialização
- Fornecer próximos passos

## Implementação

O script será implementado com:

1. **Validação de Docker** - Verifica instalação e daemon
2. **Configuração de `.env`** - Copia e valida arquivo de ambiente
3. **Estrutura de Diretórios** - Cria logs/, build/
4. **Health Checks** - Verifica serviços Docker (graceful)
5. **Logging & Feedback** - Timestamps, níveis, resumo final

## Funcionalidades Principais

- Logs com timestamps (INFO, WARN, ERROR, SUCCESS)
- Idempotente (executar 2x não quebra nada)
- Timeout respeitado (< 5 min)
- Graceful degradation (não falha se Docker services estão down)
- Mensagens claras de erro e próximos passos

## Casos de Uso

1. **Novo worktree**: Agent Manager cria → Setup executa → Ambiente pronto
2. **Importar worktree**: User importa → Setup executa → Ambiente validado
3. **Worktree paralelo**: Múltiplos worktrees usam mesmos serviços Docker

## Integração com Agent Manager

- **Executa após**: Criação/importação de worktree
- **Recebe**: `WORKTREE_PATH`, `REPO_PATH` como env vars
- **Completa em**: < 5 minutos (timeout Agent Manager)
- **Não modifica**: `agent-manager.json` (Agent Manager gerencia)

## Melhorias Futuras (Fase 2)

1. Port derivation para isolação de portas
2. Per-worktree docker-compose
3. Database seeding por worktree
4. Certificate generation local
5. Health endpoint `/health/setup`
6. Telemetry e métricas de setup

## Próximos Passos

1. Implementar script em `.aicockpit/setup-script.sh`
2. Testar com worktree existente
3. Validar Agent Manager consegue executar
4. Documentar em AGENTS.md se necessário
5. Iterar baseado em feedback

