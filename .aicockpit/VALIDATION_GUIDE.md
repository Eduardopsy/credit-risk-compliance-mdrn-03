# Como Verificar se o Setup Funcionou Adequadamente

## Verificação Rápida (2 minutos)

### 1️⃣ Executar script de validação

```bash
# Do diretório do worktree
cd .aicockpit/worktrees/backend-unit-tests
.aicockpit/validate-setup.sh
```

Ou se estiver em outro diretório:

```bash
.aicockpit/validate-setup.sh
```

### 2️⃣ Verificar arquivo `.env`

```bash
# Listar variáveis do .env
grep -v "^#" .env | grep -v "^$" | sort

# Verificar tamanho
ls -lh .env

# Contar linhas
wc -l .env
```

### 3️⃣ Verificar diretórios criados

```bash
# Listar estrutura
ls -la

# Deve ver:
# -rw-rw-r-- ... .env
# drwxrwxr-x ... logs/
# drwxrwxr-x ... build/
```

---

## Verificação Detalhada (5 minutos)

### 📁 Estrutura de Diretórios

```bash
# Verificar se arquivos/diretórios existem
test -f .env && echo "✓ .env existe" || echo "✗ .env MISSING"
test -d logs && echo "✓ logs/ existe" || echo "✗ logs/ MISSING"
test -d build && echo "✓ build/ existe" || echo "✗ build/ MISSING"
test -d .git && echo "✓ .git/ existe" || echo "✗ .git/ MISSING"
```

### 📋 Variáveis de Ambiente

```bash
# Verificar variáveis obrigatórias
vars=(POSTGRES_USER POSTGRES_PASSWORD POSTGRES_DB REDIS_PASSWORD)
for var in "${vars[@]}"; do
  grep -q "^${var}=" .env && echo "✓ $var" || echo "✗ $var MISSING"
done
```

### 🔒 Permissões

```bash
# Verificar permissões de arquivo
[ -r .env ] && echo "✓ .env readable" || echo "✗ .env not readable"
[ -w .env ] && echo "✓ .env writable" || echo "✗ .env not writable"
[ -w logs ] && echo "✓ logs/ writable" || echo "✗ logs/ not writable"
```

### 🔧 Dependências

```bash
# Verificar Docker
docker --version
docker-compose --version

# Verificar .NET
dotnet --version

# Verificar git
git --version
```

### 🐳 Serviços Docker

```bash
# Verificar se containers estão rodando
docker ps --filter "name=crcl-" --format "table {{.Names}}\t{{.Status}}"

# Se nenhum aparecer, é esperado (services podem estar offline)
# Inicie com: docker-compose up -d (do repo root)
```

---

## Checklist Completo de Validação

### ✅ Estrutura

- [ ] `.env` existe (5-6 KB)
- [ ] `logs/` existe (vazio)
- [ ] `build/` existe (vazio)
- [ ] `.git/` existe

### ✅ Variáveis de Ambiente

- [ ] `POSTGRES_USER` = `crcl_user`
- [ ] `POSTGRES_PASSWORD` definido
- [ ] `POSTGRES_DB` = `credit_risk_db`
- [ ] `REDIS_PASSWORD` definido
- [ ] `RABBITMQ_USER` = `crcl_broker`
- [ ] `RABBITMQ_PASSWORD` definido
- [ ] `KEYCLOAK_ADMIN_USER` = `admin`
- [ ] `KEYCLOAK_ADMIN_PASSWORD` definido

### ✅ Permissões

- [ ] `.env` é legível
- [ ] `.env` é editável
- [ ] `logs/` é editável
- [ ] `build/` é editável

### ✅ Sistema

- [ ] Docker instalado (`docker --version`)
- [ ] Docker Compose instalado (`docker-compose --version`)
- [ ] .NET SDK instalado (`dotnet --version`)
- [ ] Git instalado (`git --version`)

### ⚠️ Serviços Docker (opcional para agora)

- [ ] PostgreSQL rodando (`docker ps | grep postgres`)
- [ ] Redis rodando (`docker ps | grep redis`)
- [ ] RabbitMQ rodando (`docker ps | grep rabbitmq`)
- [ ] Keycloak rodando (`docker ps | grep keycloak`)

---

## Interpretando Resultados do validate-setup.sh

### ✅ Se todos os testes passaram

```
✓ Testes OK:     25
✗ Testes FALHOS: 0
⚠ Avisos:        0

✅ Setup está OK!
```

**Significado**: Setup executou perfeitamente. Você pode prosseguir para compilar e testar.

### ⚠️ Se tem avisos (Docker services offline)

```
✓ Testes OK:     21
✗ Testes FALHOS: 0
⚠ Avisos:        4

✅ Setup está OK!

Próximos passos:
1. Iniciar Docker services:
   cd /repo && docker-compose up -d
```

**Significado**: Setup OK, mas services Docker não estão rodando (esperado). Inicie-os com `docker-compose up -d`.

### ❌ Se tem falhas

```
✓ Testes OK:     15
✗ Testes FALHOS: 3
⚠ Avisos:        2

❌ Existem problemas!
```

**Significado**: Algo não funcionou. Veja troubleshooting abaixo.

---

## Troubleshooting

### Problema: ".env not found"

```bash
# Verificar se existe
ls -la .env

# Se não existe, copiar manualmente
cp ../../.env.example .env

# Ou re-executar setup
export WORKTREE_PATH="$(pwd)"
export REPO_PATH="$(git rev-parse --show-toplevel)"
$REPO_PATH/.aicockpit/setup-script.sh
```

### Problema: ".env not readable/writable"

```bash
# Verificar permissões
ls -l .env

# Corrigir
chmod u+rw .env
chmod go-rwx .env  # Segurança
```

### Problema: "logs/ or build/ not found"

```bash
# Criar manualmente
mkdir -p logs build

# Ou re-executar setup
$REPO_PATH/.aicockpit/setup-script.sh
```

### Problema: "Variáveis faltando no .env"

```bash
# Verificar quais faltam
grep "POSTGRES_USER" .env
grep "REDIS_PASSWORD" .env

# Adicionar manualmente se necessário
echo "POSTGRES_USER=crcl_user" >> .env
```

### Problema: "Docker não está rodando"

```bash
# Verificar status
docker ps
# Se falhar, inicie Docker

# No Linux
sudo systemctl start docker

# No Mac
open /Applications/Docker.app

# No Windows
# Procure 'Docker Desktop' e clique
```

### Problema: "Services não estão rodando"

É NORMAL durante setup. Para iniciar:

```bash
cd /path/to/repo
docker-compose up -d

# Aguardar ~60 segundos
sleep 60

# Verificar status
docker-compose ps
```

---

## Verificação Pós-Setup (antes de compilar)

Após confirmar que tudo OK:

### 1️⃣ Revisar .env

```bash
nano .env

# Procure por CHANGE_ME e substitua com valores reais
# (ou deixe os defaults para desenvolvimento local)
```

### 2️⃣ Verificar Git branch

```bash
git status
git branch -a

# Deve estar na branch correta do worktree
```

### 3️⃣ Testar build .NET

```bash
# Compilar solução
dotnet build CreditRiskComplianceLab.sln

# Ou compilar um projeto
dotnet build src/modules/iam/CreditRisk.IAM.Api/
```

### 4️⃣ Verificar conectividade com serviços

```bash
# Depois que docker-compose up está rodando

# Test PostgreSQL
psql -h localhost -U crcl_user -d credit_risk_db -c "SELECT 1;"

# Test Redis
redis-cli -h localhost ping

# Test RabbitMQ
curl -v http://localhost:15672/api/health/checks
```

---

## Commands Rápidos de Referência

```bash
# Validar setup
.aicockpit/validate-setup.sh

# Ver .env
cat .env | grep -v "^#" | grep -v "^$"

# Ver estrutura
ls -lhR

# Verificar Docker
docker ps
docker-compose ps

# Compilar
dotnet build

# Iniciar services
docker-compose up -d

# Parar services
docker-compose down

# Ver logs
docker-compose logs -f postgres
docker-compose logs -f redis
```

---

## Resultado Esperado

Se tudo funcionou corretamente, você deve ter:

1. ✅ Arquivo `.env` com todas variáveis configuradas
2. ✅ Diretórios `logs/` e `build/` prontos para uso
3. ✅ Git branch confirmada
4. ✅ Docker e .NET detectados
5. ✅ Pronto para: compilar → testar → desenvolver

---

**Versão**: 1.0.0  
**Criado**: 2026-08-20  
**Para**: Credit Risk Compliance Lab — AI Cockpit Agent Manager
