# Plano de Sincronização e Ajuste de setup.md e docs/specs/*.md

## 1. Objetivo
Atualizar `setup.md` e todos os arquivos de especificação em `docs/specs/` (`SPEC-01` a `SPEC-07`) para consolidar todas as correções de bugs, ajustes de arquitetura, configurações de runtime (.NET 8), mapeamento de portas, modelos de Outbox Pattern, serialização AOT, migrações de banco de dados e contratos de API/Postman identificados ao longo do ciclo de vida do projeto.
Desta forma, ao copiar apenas `setup.md` e a pasta `docs/specs/` para um diretório vazio, um agente autônomo será capaz de implementar todo o sistema sem necessidade de correções manuais.

---

## 2. Diagnóstico das Divergências e Correções a Consolidar

### 2.1 Runtime, SDK e Build Properties
- **Divergência:** `setup.md` e alguns specs mencionavam .NET 10 / C# 14, enquanto a implementação funcional real utiliza .NET 8 (LTS) com C# 12/13.
- **Ajuste:** Padronizar em `setup.md`, `Directory.Build.props`, `global.json` e todos os specs o uso do .NET 8 (`net8.0`).

### 2.2 Portas e Bindings de Rede (Kestrel)
- **Divergência:** Sem a configuração explícita de `ListenAnyIP`, o `CreateSlimBuilder` escuta apenas em `127.0.0.1`, falhando em containers.
- **Portas Reais do Sistema:**
  - `5000`: IAM API (`CreditRisk.IAM.Api`)
  - `5001`: Credit Analysis API (`CreditRisk.CreditAnalysis.Api`)
  - `5002`: Compliance API (`CreditRisk.Compliance.Api`)
  - `5003`: Operations Server / SignalR Hub (`CreditRisk.Operations.Server`)
  - `8080`: Keycloak IAM Provider
  - `8081`: Bureau Mock Service (`CreditRisk.BureauMock.Service`)
  - `5432`: PostgreSQL Database
  - `6379`: Redis Cache / Token Store
  - `5672` / `15672`: RabbitMQ Broker & Management UI

### 2.3 Outbox Pattern e EF Core Migrations
- **Divergência:** Falta da tabela `outbox_messages` gerava erro 500 no `POST /api/v1/users` e nos endpoints de criação.
- **Ajuste:** 
  - `IamDbContext`, `CreditAnalysisDbContext` e `ComplianceDbContext` devem conter explicitamente `public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();`.
  - Configuração `OutboxMessageConfiguration` padronizada em todos os módulos de infraestrutura.
  - Documentação do passo a passo de migração EF Core (`dotnet ef database update`) ou migração automática na inicialização.

### 2.4 Serialização JSON e Native AOT
- **Divergência:** Faltavam tipos registrados em `JsonSerializerContext` (como `ReviewAlertRequest`, `PagedResult<T>`, `ProblemDetails`, `Dictionary<string, string[]>`), e faltava a diretiva explícita `using System.Text.Json.Serialization;`.
- **Ajuste:** Documentar todos os DTOs nos contextos `IamApiJsonContext`, `CreditAnalysisApiJsonContext`, `ComplianceApiJsonContext`, `WorkerJsonContext` e `OperationsServerJsonContext`.

### 2.5 Observabilidade e Tratamento de Nulos no OTLP
- **Divergência:** `new Uri(configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]!)` lançava `ArgumentNullException` quando a variável de ambiente não estava presente.
- **Ajuste:** Consolidar em `SPEC-01` e `setup.md` a verificação de nulidade com registro condicional dos exporters OTLP.

### 2.6 Endpoints de Usuário e Autenticação (IAM)
- **Divergência:**
  - `POST /api/v1/users` é público (auto-registro / setup de usuários), recebendo `{ email, fullName, role, temporaryPassword }`.
  - `GET /api/v1/users/{id:guid}` é restrito (`RequiresAdministrator`).
  - `GET /api/v1/users/me` **não existe** (deve ser removido de qualquer menção/teste).
  - `POST /api/v1/auth/login` recebe `{ email, password, totpCode }` e retorna `{ accessToken, refreshToken, expiresIn, tokenType, roles }`.

### 2.7 Injeção de Dependências e Cache Distribuído
- **Divergência:** Compliance API exigia `IDistributedCache` não registrado para o `PepScreeningService`.
- **Ajuste:** Especificar o registro de Redis / `AddStackExchangeRedisCache` em `AddComplianceInfrastructure`.

### 2.8 Coleção Postman e Automação de Testes
- **Ajuste:** Especificar a estrutura e os scripts pós-requisição da coleção Postman (extração automática de `accessToken`, `userId`, `proposalId`) e o script de execução via curl/newman.

---

## 3. Plano de Arquivos a Serem Atualizados

### Fase 1: `setup.md`
- Atualizar tabela de tecnologias e versões (.NET 8, C# 12/13).
- Atualizar inventário de serviços e portas (5000, 5001, 5002, 5003, 8081, 8080, etc.).
- Incluir as regras estritas de Native AOT, Kestrel `ListenAnyIP`, Outbox Pattern em todos os módulos, migrações de banco de dados e contratos de API reais.

### Fase 2: `docs/specs/SPEC-01-architecture-core.md`
- Ajustar `global.json`, `Directory.Build.props`, `Directory.Packages.props`.
- Incluir `OutboxMessage`, `IOutboxRepository`, `IOutboxProcessor` no `CreditRisk.Shared.Kernel`.
- Sincronizar `ObservabilityExtensions.cs` com o null-check seguro de OTLP.
- Confirmar implementações de `Cpf`, `Cnpj`, `MoneyAmount`, `Result<T>`, `PagedResult<T>`, `Guard`, `ValidationBehavior`.

### Fase 3: `docs/specs/SPEC-02-backend.md`
- Atualizar o `Program.cs` e `ServiceCollectionExtensions` dos 3 módulos de API (IAM, Credit Analysis, Compliance).
- Ajustar contratos de rota e DTOs (remover `/users/me`, ajustar `POST /users` público e `GET /users/{id}` admin).
- Adicionar mapeamento de `OutboxMessage` em `IamDbContext`, `CreditAnalysisDbContext` e `ComplianceDbContext`.
- Configurar Kestrel em todas as APIs (`ListenAnyIP`).
- Garantir `IDistributedCache` registrado no Compliance.
- Atualizar os contextos `JsonSerializerContext` com todos os DTOs e types.

### Fase 4: `docs/specs/SPEC-03-backend-unit-tests.md`
- Sincronizar todos os testes unitários, builders e fakes manuais com os contratos reais (CQRS, CancellationToken, Result<T>).
- Alinhar com `coverage.runsettings` e exclusões AOT.

### Fase 5: `docs/specs/SPEC-04-integration.md`
- Especificar o `CreditRisk.BureauMock.Service` na porta 8081 (`/query`, `/health`).
- Documentar os consumers de MassTransit e a implementação do `OutboxProcessor`.
- Atualizar portas e configurações dos Workers.

### Fase 6: `docs/specs/SPEC-05-frontend.md` e `docs/specs/SPEC-06-frontend-unit-tests.md`
- Sincronizar URLs da API e SignalR hub com a porta 5003 (`CreditRisk.Operations.Server`).
- Garantir alinhamento de DTOs e modelos de visualização (ViewModels).

### Fase 7: `docs/specs/SPEC-07-devops-infrastructure.md`
- Sincronizar `docker-compose.yml`, `init-db.sql`, `seed-data.sql`, `realm-export.json`.
- Alinhar mapeamento de portas e variáveis de ambiente com o ambiente funcional testado.
- Incluir comandos de migração EF Core automática no fluxo de inicialização.

---

## 4. Critérios de Validação da Documentação
- Todas as especificações em `docs/specs/` e `setup.md` são consistentes entre si.
- Nenhum tipo, rota ou configuração fictícia/inexistente é referenciada.
- Todas as 5 portas dos serviços de backend (5000, 5001, 5002, 5003, 8081) e infraestrutura estão perfeitamente documentadas.
- O código de exemplo fornecido compila diretamente no .NET 8.
