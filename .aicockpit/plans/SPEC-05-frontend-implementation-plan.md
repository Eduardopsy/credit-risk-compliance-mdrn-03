# Plano Detalhado de Implementação: SPEC-05 — Front-End (Blazor WebAssembly)

> **Documento de Referência:** `docs/specs/SPEC-05-frontend.md`  
> **Dependências:** `SPEC-01-architecture-core.md`, `SPEC-02-backend.md`, `SPEC-04-integration.md`  
> **Status:** Pronto para Implementação Física e Completa

---

## 1. Visão Geral e Arquitetura da Solução

A frente **SPEC-05** contempla o front-end moderno em **Blazor WebAssembly (.NET 8)** com a biblioteca **MudBlazor (v7.15.0)**, autenticação via **OIDC (Keycloak)**, atualizações de status em tempo real via **SignalR Client**, e gerenciamento de estado desacoplado e reativo através de **`AppStateService`**.

A aplicação é dividida em dois projetos principais:
1. **`CreditRisk.Operations.Client`** (Blazor WebAssembly SPA):
   - Localização: `src/modules/operations/CreditRisk.Operations.Client/`
   - Páginas: Dashboards (Risk, Compliance, Operations), Gestão de Propostas (List, Detail, Create), Gestão de Alertas AML (List, Detail) e Visualizador de Auditoria (Audit Log).
   - Componentes compartilhados e atômicos (RiskRatingBadge, AlertSeverityChip, ProposalStatusStepper, ConnectionStatusIndicator).
   - Comunicação com backend via `ApiClient` tipado (injetando JWT bearer via `BaseAddressAuthorizationMessageHandler`) e `OperationsHubClient` (SignalR com backoff exponencial: 0s, 2s, 5s, 10s, 30s).
2. **`CreditRisk.Operations.Server`** (Host ASP.NET Core & SignalR Hub):
   - Localização: `src/servers/CreditRisk.Operations.Server/`
   - Hospedagem estática e roteamento fallback do Blazor WebAssembly (`UseBlazorFrameworkFiles()`).
   - SignalR Hub (`OperationsHub`) mapeado para `/hubs/operations` com suporte aos grupos de roles (`role:desk-operator`, `role:compliance-analyst`, `role:administrator`).
   - Consumidores MassTransit (`AmlAlertCreatedEventConsumer`, `CreditLimitApprovedEventConsumer`, `TransactionFlaggedEventConsumer`) transmitindo eventos recebidos do RabbitMQ para os grupos do SignalR.

---

## 2. Inventário de Arquivos e Estrutura Física

### 2.1 Projeto `CreditRisk.Operations.Client`

```
src/modules/operations/CreditRisk.Operations.Client/
├── CreditRisk.Operations.Client.csproj
├── Program.cs
├── App.razor
├── _Imports.razor
├── wwwroot/
│   ├── index.html
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   └── css/
│       └── app.css
├── Models/
│   ├── CreditProposalViewModel.cs
│   ├── AmlAlertViewModel.cs
│   ├── DashboardMetricsViewModel.cs
│   ├── CreateProposalRequest.cs
│   ├── ProposalAcceptedResponse.cs
│   ├── ReviewAlertRequest.cs
│   ├── AuditLogEntryViewModel.cs
│   ├── OperationsMetricsViewModel.cs
│   └── Common/
│       └── PagedResult.cs
├── State/
│   └── AppStateService.cs
├── Services/
│   ├── ApiClient.cs
│   ├── OperationsHubClient.cs
│   └── NotificationService.cs
├── Components/
│   ├── RiskRatingBadge.razor
│   ├── AlertSeverityChip.razor
│   ├── ProposalStatusStepper.razor
│   └── ConnectionStatusIndicator.razor
├── Shared/
│   ├── MainLayout.razor
│   ├── MainLayout.razor.cs
│   ├── NavMenu.razor
│   ├── RedirectToLogin.razor
│   └── NotAuthorized.razor
└── Pages/
    ├── Authentication.razor
    ├── Dashboard/
    │   ├── RiskDashboard.razor
    │   ├── RiskDashboard.razor.cs
    │   ├── ComplianceDashboard.razor
    │   ├── ComplianceDashboard.razor.cs
    │   ├── OperationsDashboard.razor
    │   └── OperationsDashboard.razor.cs
    ├── Proposals/
    │   ├── ProposalList.razor
    │   ├── ProposalList.razor.cs
    │   ├── ProposalDetail.razor
    │   ├── ProposalDetail.razor.cs
    │   ├── CreateProposal.razor
    │   └── CreateProposal.razor.cs
    ├── Alerts/
    │   ├── AlertList.razor
    │   ├── AlertList.razor.cs
    │   ├── AlertDetail.razor
    │   └── AlertDetail.razor.cs
    └── AuditLog/
        ├── AuditLogViewer.razor
        └── AuditLogViewer.razor.cs
```

### 2.2 Projeto `CreditRisk.Operations.Server`

```
src/servers/CreditRisk.Operations.Server/
├── CreditRisk.Operations.Server.csproj
├── Program.cs
├── appsettings.json
├── appsettings.Development.json
├── Hubs/
│   └── OperationsHub.cs
├── Consumers/
│   ├── AmlAlertCreatedEventConsumer.cs
│   ├── CreditLimitApprovedEventConsumer.cs
│   └── TransactionFlaggedEventConsumer.cs
└── Serialization/
    └── OperationsServerJsonContext.cs
```

---

## 3. Fases de Execução Passo a Passo

### Fase 1: Criação e Configuração do Projeto `CreditRisk.Operations.Client`

1. **Criar o Diretório e o `.csproj`:**
   - Referenciar `Microsoft.AspNetCore.Components.WebAssembly` (8.0.11), `Microsoft.AspNetCore.Components.WebAssembly.Authentication` (8.0.11), `Microsoft.AspNetCore.SignalR.Client` (8.0.11), `MudBlazor` (7.15.0), `FluentValidation` (11.11.0).
   - Adicionar project reference para `../../shared/CreditRisk.Shared.Contracts/CreditRisk.Shared.Contracts.csproj`.
   - Incluir na solução `CreditRiskComplianceLab.sln` na pasta de solução `modules/operations`.

2. **Configuração de Inicialização (`Program.cs`):**
   - Configurar `AddOidcAuthentication` com binding de `"Oidc"` e escopos `openid`, `profile`, `email`, `roles`.
   - Configurar `AddHttpClient("crcl-api")` com `BaseAddressAuthorizationMessageHandler`.
   - Configurar MudBlazor com `AddMudServices()` com as configurações de `SnackbarConfiguration` especificadas no SPEC-05.
   - Registrar serviços Scoped: `AppStateService`, `OperationsHubClient`, `ApiClient`, `NotificationService`.

3. **Arquivos Estáticos (`wwwroot`):**
   - `index.html`: incluir fontes Material Design e Roboto, MudBlazor CSS e JS (`_content/MudBlazor/MudBlazor.min.css` e `_content/MudBlazor/MudBlazor.min.js`), e o runtime do Blazor `_framework/blazor.webassembly.js`.
   - `appsettings.json` e `appsettings.Development.json`: configurar `ApiBaseUrl`, `HubUrl` e configurações OIDC para Keycloak (`Authority`, `ClientId`, `RedirectUri`, `PostLogoutRedirectUri`).

4. **Raiz do Blazor (`App.razor`, `_Imports.razor`):**
   - `_Imports.razor`: importar namespaces comuns do Blazor, MudBlazor, Models, State, Services e Shared.
   - `App.razor`: configurar `CascadingAuthenticationState`, `Router` com `AuthorizeRouteView`, `NotAuthorized` handler e `RedirectToLogin`.

---

### Fase 2: Modelos, Estado Global e Serviços do Cliente

1. **ViewModels e Requisições (`Models/`):**
   - `CreditProposalViewModel`: Id, CustomerId, CustomerDocument, CustomerName, RequestedLimit, ApprovedLimit, RiskRating, Status, CreatedAt, EvaluatedAt.
   - `AmlAlertViewModel`: Id, TransactionId, CustomerId, CustomerDocument, AlertType, Severity, TransactionAmount, Description, Status, CreatedAt, IsRead, ReviewerComments.
   - `DashboardMetricsViewModel`: ActiveProposals, PendingReview, ApprovedToday, RejectedToday, RatingACount, RatingBCount, RatingCCount, RatingDCount, RatingECount.
   - `OperationsMetricsViewModel`: SystemHealth, TotalProposalsProcessed, TotalAlertsRaised, ActiveOperators, AverageProcessingTimeMs.
   - `CreateProposalRequest`: CustomerDocument, CustomerDocumentType, CustomerName, CustomerEmail, MonthlyIncome, RequestedLimit, BureauConsentGiven.
   - `ProposalAcceptedResponse`: ProposalId, Status, Message, ReceivedAt.
   - `ReviewAlertRequest`: Decision, Notes, ReviewedBy.
   - `AuditLogEntryViewModel`: Id, Action, EntityType, EntityId, PerformedBy, Timestamp, Details.
   - `PagedResult<T>`: Items, TotalCount, Page, PageSize, TotalPages, HasNextPage, HasPreviousPage.

2. **Gerenciamento de Estado (`AppStateService.cs`):**
   - Armazenar `_pendingAlerts` (limite de 100), `_recentProposals` (limite de 50).
   - Evento `StateChanged` reativo para notificar componentes via `InvokeAsync(StateHasChanged)`.
   - Métodos: `AddAlert`, `MarkAlertRead`, `SetConnectionStatus`, `UpsertProposal`.
   - Indicador de `ConnectionStatus` e contadores agregados (`UnreadAlertCount`).

3. **Cliente SignalR (`OperationsHubClient.cs`):**
   - Injetar `IAccessTokenProvider` para inclusão do token JWT no cabeçalho WebSockets.
   - Configurar `WithAutomaticReconnect` com o backoff prescrito: 0s, 2s, 5s, 10s, 30s.
   - Handlers de ciclo de vida (`Reconnecting`, `Reconnected`, `Closed`) atualizando o `AppStateService`.
   - Subscrições de eventos:
     - `AmlAlertReceived` -> mapeia para `AmlAlertViewModel` e chama `AppState.AddAlert`.
     - `CreditLimitApproved` -> mapeia para `CreditProposalViewModel` e chama `AppState.UpsertProposal`.
     - `TransactionFlagged` -> adiciona notificação/alerta correspondente.
   - Invocação de `JoinRoleGroup(userRole)` ao conectar.
   - Implementação de `IAsyncDisposable`.

4. **Cliente HTTP Tipado (`ApiClient.cs`):**
   - Endpoints de Propostas: `GetProposalAsync`, `GetProposalsAsync`, `CreateProposalAsync`.
   - Endpoints de Alertas: `GetAlertsAsync`, `GetAlertAsync`, `ReviewAlertAsync`.
   - Endpoints de Dashboard: `GetDashboardMetricsAsync`, `GetOperationsMetricsAsync`.
   - Endpoints de Auditoria: `GetAuditLogsAsync`.

5. **Serviço de Notificação (`NotificationService.cs`):**
   - Integração com `ISnackbar` do MudBlazor para exibição de toasts de alertas críticos, notificações de aprovação e erros de conexão.

---

### Fase 3: Componentes de UI Compartilhados e Layout

1. **Componentes:**
   - `ConnectionStatusIndicator.razor`: `MudChip` com cores e ícones dinâmicos baseados no estado SignalR (Verde/Wifi para Connected, Amarelo/WifiFind para Reconnecting, Vermelho/WifiOff para Disconnected).
   - `RiskRatingBadge.razor`: Exibição visual da classificação de risco (A = Verde escuro, B = Verde claro, C = Amarelo/Aviso, D = Laranja, E = Vermelho/Erro).
   - `AlertSeverityChip.razor`: Chip estilizado por severidade de alerta AML (Critical = Error, High = Warning, Medium = Info, Low = Default).
   - `ProposalStatusStepper.razor`: Visualizador de progresso da esteira de crédito (Submitted -> UnderReview -> Evaluated -> Approved/Rejected).

2. **Layout e Navegação:**
   - `MainLayout.razor` & `MainLayout.razor.cs`: `MudThemeProvider`, `MudDialogProvider`, `MudSnackbarProvider`. AppBar com `ConnectionStatusIndicator`, contador de alertas não lidos, menu do usuário autenticado e Drawer lateral retrátil.
   - `NavMenu.razor`: Links baseados em perfil (`AuthorizeView` com roles `desk-operator`, `compliance-analyst`, `administrator`) para Risk Dashboard, Compliance Dashboard, Operations Dashboard, Proposals, Alerts e Audit Log.
   - `RedirectToLogin.razor` e `NotAuthorized.razor`: Tratamento elegante de autenticação e permissões de acesso com mensagens explicativas e redirecionamento para o fluxo OIDC.
   - `Authentication.razor`: Rota `/authentication/{action}` para callbacks de login e logout OIDC do Blazor WASM.

---

### Fase 4: Páginas Principais (com Code-Behind)

1. **Risk Dashboard (`Pages/Dashboard/RiskDashboard.razor` e `.cs`):**
   - Rota: `@page "/risk-dashboard"`, autorização: `[Authorize(Roles = "desk-operator,compliance-analyst,administrator")]`.
   - Cards de KPI: Propostas Ativas, Pendentes de Revisão, Aprovadas Hoje, Rejeitadas Hoje.
   - Gráfico `MudChart` Donut: Distribuição de Ratings A a E.
   - Tabela `MudDataGrid` de Propostas Recentes (do `AppStateService.RecentProposals`) com navegação ao clicar na linha.
   - Auto-refresh de 60s via timer e subscrição ao `AppState.StateChanged`.

2. **Compliance Dashboard (`Pages/Dashboard/ComplianceDashboard.razor` e `.cs`):**
   - Rota: `@page "/compliance-dashboard"`, autorização: `[Authorize(Roles = "compliance-analyst,administrator")]`.
   - Badge com `UnreadAlertCount`.
   - Indicador de conexão SignalR.
   - `MudDataGrid` de alertas pendentes (`AppState.PendingAlerts`) com ordenação, filtros e botão para revisar.
   - Painel lateral com contadores e estatísticas de alertas das últimas 24h por severidade.

3. **Operations Dashboard (`Pages/Dashboard/OperationsDashboard.razor` e `.cs`):**
   - Rota: `@page "/operations-dashboard"`, autorização: `[Authorize(Roles = "administrator")]`.
   - Indicadores de saúde do sistema, taxa de processamento, throughput de eventos e visão executiva de tempos médios.

4. **Nova Proposta (`Pages/Proposals/CreateProposal.razor` e `.cs`):**
   - Rota: `@page "/proposals/create"`, autorização: `[Authorize(Roles = "desk-operator,administrator")]`.
   - Formulário com `MudForm` e validação via `CreateProposalRequestClientValidator` (`FluentValidation`).
   - Validação de CPF/CNPJ, Nome completo, E-mail, Renda Mensal, Limite Solicitado (R$ 1 a R$ 500.000), e consentimento explícito do bureau (LGPD Art. 7).
   - Bloqueio de submissão dupla com `_isSubmitting` e feedback via `ISnackbar`.

5. **Lista e Detalhes de Propostas (`Pages/Proposals/`):**
   - `ProposalList.razor` / `.cs`: Tabela paginada (20 itens/página) com máscaras para dados sensíveis (`529.***.***.25`), filtros por status e data.
   - `ProposalDetail.razor` / `.cs`: Detalhes completos da proposta com `ProposalStatusStepper`, visualização do Score e Limite Aprovado.

6. **Lista e Detalhes de Alertas AML (`Pages/Alerts/`):**
   - `AlertList.razor` / `.cs`: Grid de alertas com paginação, filtros por severidade e status.
   - `AlertDetail.razor` / `.cs`: Detalhes do alerta, transação vinculada, histórico e formulário para decisão (Dismiss / Escalate / Confirm Fraud) enviando `ReviewAlertRequest`.

7. **Visualizador de Logs de Auditoria (`Pages/AuditLog/`):**
   - `AuditLogViewer.razor` / `.cs`: Consulta paginada dos registros de auditoria e conformidade com filtro por entidade e operador.

---

### Fase 5: Atualização e Sincronização do `CreditRisk.Operations.Server`

1. **Ajustes no `.csproj`:**
   - Adicionar referências aos pacotes:
     - `Microsoft.AspNetCore.Components.WebAssembly.Server` (8.0.11)
     - `Microsoft.AspNetCore.SignalR.StackExchangeRedis` (8.0.11)
     - `FluentValidation` (11.11.0)
   - Referenciar o projeto client:
     - `<ProjectReference Include="../../modules/operations/CreditRisk.Operations.Client/CreditRisk.Operations.Client.csproj" />`

2. **Ajustes no `OperationsHub.cs`:**
   - Implementar métodos `JoinRoleGroup(string role)` mapeando para `role:desk-operator`, `role:compliance-analyst`, `role:administrator`.
   - Limpeza de grupos no `OnDisconnectedAsync`.
   - Proteger o Hub com `[Authorize]`.

3. **Consumidores MassTransit (`Consumers/`):**
   - `AmlAlertCreatedEventConsumer`: consome `AmlAlertCreatedEvent` e despacha para grupos `role:compliance-analyst` e `role:administrator` via `Clients.Group(...).SendAsync("AmlAlertReceived", ...)`.
   - `CreditLimitApprovedEventConsumer`: consome `CreditLimitApprovedEvent` e despacha para grupos `role:desk-operator` e `role:administrator` via `Clients.Group(...).SendAsync("CreditLimitApproved", ...)`.
   - `TransactionFlaggedEventConsumer`: consome `TransactionFlaggedEvent` e despacha `ReceiveTransactionFlagged` ou `AmlAlertReceived`.

4. **Pipeline HTTP e Hospedagem Blazor (`Program.cs`):**
   - Configurar autenticação JWT para SignalR (permitir passagem de token de acesso via query string para WebSocket `/hubs/operations`).
   - Adicionar suporte a Redis backplane para SignalR quando configurado.
   - Adicionar `app.UseBlazorFrameworkFiles()` antes de `app.UseStaticFiles()`.
   - Mapear fallback: `app.MapFallbackToFile("index.html")`.
   - Mapear Hub com rota prescrita: `app.MapHub<OperationsHub>("/hubs/operations")`.

---

### Fase 6: Solução, Compilação e Validação

1. **Adicionar Projetos à Solução `CreditRiskComplianceLab.sln`:**
   - `dotnet sln add src/modules/operations/CreditRisk.Operations.Client/CreditRisk.Operations.Client.csproj`
   - Atualizar a referência de `CreditRisk.Operations.Server.csproj` caso necessário.

2. **Compilação e Verificação:**
   - Executar `dotnet build CreditRiskComplianceLab.sln` garantindo 0 erros e 0 warnings.
   - Testar o publish: `dotnet publish src/modules/operations/CreditRisk.Operations.Client/ -c Release`.
   - Validar geração de `index.html` e assets em `_framework`.

---

## 4. Matriz de Rastreabilidade com os Critérios de Aceite (DoD)

| Critério de Aceite (SPEC-05 §9) | Componente Responsável | Verificação |
|---|---|---|
| `dotnet build` sucede em Client e Server com 0 warnings | Solução geral / `.csproj` | `dotnet build` sem erros/avisos |
| `Program.cs` com OIDC Keycloak, MudBlazor, AppState, HubClient, ApiClient | `CreditRisk.Operations.Client/Program.cs` | Inspeção de DI e serviços registrados |
| 4 Dashboards existem com rotas `@page` e `[Authorize]` | `RiskDashboard`, `ComplianceDashboard`, `OperationsDashboard`, `AuditLogViewer` | Checagem de atributos e routes |
| `CreateProposal` valida CPF/CNPJ, campos obrigatórios e consentimento LGPD | `CreateProposal.razor` & Validator | Validação FluentValidation client-side |
| `AppStateService.StateChanged` reage e notifica componentes | `AppStateService.cs` & Dashboards | Inscrições e render updates |
| `OperationsHubClient` com auto-reconnect backoff (0, 2, 5, 10, 30s) | `OperationsHubClient.cs` | Configuração do HubConnectionBuilder |
| `ConnectionStatusIndicator` reflete Connected, Reconnecting, Disconnected | `ConnectionStatusIndicator.razor` | Chip MudBlazor com ícones e cores dinâmicas |
| `MudDataGrid` com paginação padrão de 20 linhas | Tabelas de propostas e alertas | Propriedades `RowsPerPage="20"` |
| Dados sensíveis (CPF/CNPJ) mascarados na UI | Grid / Helpers de formatação | `529.***.***.25` |
| Redirecionamento unauthenticated para Keycloak e bloqueio por roles | `App.razor`, `MainLayout`, `NotAuthorized.razor` | `AuthorizeRouteView` e autorização por roles |
| `appsettings.json` com chaves de OIDC e API | `wwwroot/appsettings.json` | Configuração JSON completa |
