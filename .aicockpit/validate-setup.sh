#!/bin/bash
################################################################################
# Worktree Setup Validation Script
# Verifica se o setup-script.sh executou corretamente
################################################################################

set -euo pipefail

WORKTREE_PATH="$(pwd)"
REPO_PATH=$(git rev-parse --show-toplevel 2>/dev/null || echo "?")

# Colors
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

PASSED=0
FAILED=0
WARNINGS=0

echo "╔════════════════════════════════════════════════════════════════════╗"
echo "║  Worktree Setup Validation                                        ║"
echo "║  $(basename "$WORKTREE_PATH")                                              ║"
echo "╚════════════════════════════════════════════════════════════════════╝"
echo ""

# ────────────────────────────────────────────────────────────────────────────
check_file() {
    local name="$1"
    local path="$2"
    
    if [[ -f "$path" ]]; then
        local size=$(du -h "$path" | cut -f1)
        echo -e "${GREEN}✓${NC} $name ($size)"
        ((PASSED++))
    else
        echo -e "${RED}✗${NC} $name (NOT FOUND)"
        ((FAILED++))
    fi
}

check_dir() {
    local name="$1"
    local path="$2"
    
    if [[ -d "$path" ]]; then
        echo -e "${GREEN}✓${NC} $name"
        ((PASSED++))
    else
        echo -e "${RED}✗${NC} $name (NOT FOUND)"
        ((FAILED++))
    fi
}

check_env_var() {
    local var="$1"
    local env_file="$2"
    
    if grep -q "^${var}=" "$env_file" 2>/dev/null; then
        echo -e "${GREEN}✓${NC} $var"
        ((PASSED++))
    else
        echo -e "${RED}✗${NC} $var (MISSING)"
        ((FAILED++))
    fi
}

check_command() {
    local name="$1"
    local cmd="$2"
    
    if command -v "$cmd" &>/dev/null; then
        local version=$($cmd --version 2>/dev/null | head -1 || echo "installed")
        echo -e "${GREEN}✓${NC} $name ($version)"
        ((PASSED++))
    else
        echo -e "${RED}✗${NC} $name (NOT INSTALLED)"
        ((FAILED++))
    fi
}

# ────────────────────────────────────────────────────────────────────────────
echo -e "${BLUE}1. ESTRUTURA DE DIRETÓRIOS${NC}"
echo "────────────────────────────────────────────────────────────────────"

check_file ".env" "$WORKTREE_PATH/.env"
check_dir "logs/" "$WORKTREE_PATH/logs"
check_dir "build/" "$WORKTREE_PATH/build"
check_dir ".git/" "$WORKTREE_PATH/.git"

echo ""

# ────────────────────────────────────────────────────────────────────────────
echo -e "${BLUE}2. VARIÁVEIS DE AMBIENTE${NC}"
echo "────────────────────────────────────────────────────────────────────"

ENV_FILE="$WORKTREE_PATH/.env"
required_vars=(
    "POSTGRES_USER"
    "POSTGRES_PASSWORD"
    "POSTGRES_DB"
    "REDIS_PASSWORD"
    "RABBITMQ_USER"
    "RABBITMQ_PASSWORD"
    "KEYCLOAK_ADMIN_USER"
    "KEYCLOAK_ADMIN_PASSWORD"
)

for var in "${required_vars[@]}"; do
    check_env_var "$var" "$ENV_FILE"
done

echo ""

# ────────────────────────────────────────────────────────────────────────────
echo -e "${BLUE}3. PERMISSÕES${NC}"
echo "────────────────────────────────────────────────────────────────────"

if [[ -r "$WORKTREE_PATH/.env" ]]; then
    echo -e "${GREEN}✓${NC} .env readable"
    ((PASSED++))
else
    echo -e "${RED}✗${NC} .env not readable"
    ((FAILED++))
fi

if [[ -w "$WORKTREE_PATH/.env" ]]; then
    echo -e "${GREEN}✓${NC} .env writable"
    ((PASSED++))
else
    echo -e "${RED}✗${NC} .env not writable"
    ((FAILED++))
fi

echo ""

# ────────────────────────────────────────────────────────────────────────────
echo -e "${BLUE}4. DEPENDÊNCIAS DO SISTEMA${NC}"
echo "────────────────────────────────────────────────────────────────────"

check_command "docker" "docker"
check_command "docker-compose" "docker-compose"
check_command "dotnet" "dotnet"

echo ""

# ────────────────────────────────────────────────────────────────────────────
echo -e "${BLUE}5. SERVIÇOS DOCKER${NC}"
echo "────────────────────────────────────────────────────────────────────"

services=("postgres" "redis" "rabbitmq" "keycloak")
running=0

for service in "${services[@]}"; do
    if docker ps --filter "name=crcl-$service" --quiet 2>/dev/null | grep -q .; then
        echo -e "${GREEN}✓${NC} crcl-$service (running)"
        ((PASSED++))
        ((running++))
    else
        echo -e "${YELLOW}⚠${NC} crcl-$service (offline)"
        ((WARNINGS++))
    fi
done

echo ""

# ────────────────────────────────────────────────────────────────────────────
echo "╔════════════════════════════════════════════════════════════════════╗"
echo "║  RESULTADO                                                         ║"
echo "╚════════════════════════════════════════════════════════════════════╝"
echo ""

echo -e "Testes OK:     ${GREEN}$PASSED${NC}"
echo -e "Testes FALHOS: ${RED}$FAILED${NC}"
echo -e "Avisos:        ${YELLOW}$WARNINGS${NC}"

echo ""

if [[ $FAILED -eq 0 ]]; then
    echo -e "${GREEN}✅ Setup está OK!${NC}"
    if [[ $running -lt ${#services[@]} ]]; then
        echo ""
        echo "Próximos passos:"
        echo "1. Iniciar Docker services:"
        echo "   cd $REPO_PATH && docker-compose up -d"
        echo ""
        echo "2. Aguardar ~60 segundos"
        echo ""
        echo "3. Verificar status:"
        echo "   docker-compose ps"
    fi
else
    echo -e "${RED}❌ Existem problemas!${NC}"
    echo ""
    echo "Dicas:"
    echo "1. Verifique permissões: chmod u+w .env"
    echo "2. Verifique .env.example existe no repo root"
    echo "3. Re-execute setup:"
    echo "   export WORKTREE_PATH=\"\$(pwd)\""
    echo "   export REPO_PATH=\"$REPO_PATH\""
    echo "   \$REPO_PATH/.aicockpit/setup-script.sh"
fi

echo ""
exit $FAILED
