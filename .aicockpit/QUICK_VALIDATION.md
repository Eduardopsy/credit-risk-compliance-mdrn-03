# Validação Rápida do Setup — Cartão de Referência

## 🚀 Use AGORA (copie e cole)

```bash
# Validação automática completa
.aicockpit/validate-setup.sh
```

---

## ✅ O que será validado

| Componente | Status | O que testa |
|-----------|--------|-----------|
| **Estrutura** | ✓/✗ | .env, logs/, build/, .git/ |
| **Variáveis** | ✓/✗ | POSTGRES_*, REDIS_*, RABBITMQ_*, KEYCLOAK_* |
| **Permissões** | ✓/✗ | read/write em .env, logs/, build/ |
| **Dependências** | ✓/✗ | docker, docker-compose, dotnet |
| **Serviços** | ⚠/✓ | postgres, redis, rabbitmq, keycloak |

---

## 📊 Resultados Esperados

### Sucesso (0 falhas)
```
✅ Setup está OK!
```
→ Pronto para compilar

### Avisos (Docker offline)
```
✅ Setup está OK!

Próximos passos:
1. docker-compose up -d
2. sleep 60
3. docker-compose ps
```
→ Inicie services depois

### Falha (problemas)
```
❌ Existem problemas!

Dicas:
1. Verifique .env.example existe
2. Verifique permissões
3. Re-execute setup
```
→ Veja troubleshooting

---

## 🔍 Verificações Manuais Rápidas

```bash
# Arquivo .env
ls -lh .env && wc -l .env

# Diretórios
ls -ld logs build

# Variáveis principais
grep "POSTGRES_USER\|REDIS_PASSWORD\|KEYCLOAK_" .env | head -5

# Sistema
docker --version && docker-compose --version && dotnet --version

# Status Docker
docker ps --filter "name=crcl-" --format "table {{.Names}}\t{{.Status}}"
```

---

## 🛠️ Troubleshooting Rápido

| Problema | Solução |
|----------|---------|
| .env não existe | `cp ../../.env.example .env` |
| .env não editável | `chmod u+w .env` |
| Docker não instalado | Instale: https://docs.docker.com/get-docker/ |
| Services offline | `docker-compose up -d` (do repo root) |
| Setup falhou | Re-execute: `$REPO_PATH/.aicockpit/setup-script.sh` |

---

## 📚 Documentação

- **Rápida**: Este arquivo (< 2 min)
- **Completa**: `.aicockpit/VALIDATION_GUIDE.md` (5-10 min)
- **Detalhada**: `.aicockpit/SETUP_SCRIPT.md` (referência)

---

## ✨ Passo-a-Passo

1. **Agora**: `.aicockpit/validate-setup.sh` ← você está aqui
2. **Próximo**: Revisar `.env` se necessário
3. **Depois**: `docker-compose up -d` (do repo root)
4. **Então**: `dotnet build`
5. **Finalmente**: Desenvolver!

---

**Versão**: 1.0.0 | **Criado**: 2026-08-20
