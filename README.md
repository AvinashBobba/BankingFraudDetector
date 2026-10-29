# Banking Fraud Detector
A learning project built manually in small, explainable increments. The first feature detects suspicious sequences of investor account events using deterministic rules. Agentic investigation and graph analytics follow once the underlying evidence is available.

## Increment history

These entries reflect the actual Git history. Pending work is listed separately.

| Commit | Increment | What changed |
| --- | --- | --- |
| `a05761a` | Initial host | Added the Aspire AppHost and solution. |
| `be2fc5d` | Domain | Added the independent Domain class library. |
| `19fa1ce` | Application | Added Application with a reference to Domain. |
| `188b4ce` | Infrastructure | Added Infrastructure and inward project references. |
| `41bf35e` | API | Added the HTTP API and basic health endpoint. |
| `41bf5cc` | Orchestration | Connected the API to the Aspire AppHost. |
| `1048c96` | Event model | Added the account activity event domain model. |
| `235ad79` | Storage contract | Added the Application event repository interface. |
| `573b607` | Naming | Renamed the account event type. |
| `2cc1a10` | EF Core model | Added the SQL Server provider, context and event mapping. |

| `1a866e0` | Database migration | Configured SQL Server, added EF tooling and applied InitialCreate. |
| `9789d5e` | Event ingestion | Added repository, recording use case, controllers and HTTP requests; corrected Persistence folder naming. |
| `5dfaa67` | Risk assessment | Added event querying, ATO-001, assessment use case/controller, domain tests and HTTP scenarios. |
| `e5f7897` | Assessment persistence | Added extensible rules, PAY-001, saved assessments and findings, POST/GET endpoints and corrective table mapping migration. |

For each future increment, append its commit hash, purpose and validation after committing. Keep planned work separate from completed work.

## Current architecture

```text
Aspire AppHost -> API
API -> Application, Infrastructure
Infrastructure -> Application, Domain
Application -> Domain
AI -> Application
Domain -> no other solution projects
```

- Domain: account events and, later, fraud rules and risk concepts.
- Application: use cases and interfaces required by those use cases.
- Infrastructure: EF Core persistence and external integrations.
- API: HTTP endpoints and dependency registration.
- AppHost: starts and connects application processes. It does not orchestrate AI reasoning.

## Database setup

The current development configuration points to SQL Server instance `Avinash_Bobba` using Windows authentication. LocalDB is optional: an existing SQL Server instance can be used with the same EF Core SQL Server provider. SSMS is a management application, not the database engine itself.

To use the existing instance, run this query in SSMS and use the returned server name in `ConnectionStrings:FraudDatabase`:

```sql
SELECT SERVERPROPERTY('ServerName') AS ServerName,
       SERVERPROPERTY('Edition') AS Edition;
```

Example development connection string (replace the server placeholder; double backslashes in JSON for named instances):

```text
Server=<server-name>;Database=BankingFraudDetector;Trusted_Connection=True;TrustServerCertificate=True
```

Windows authentication uses the identity running the API or EF tooling. That identity needs permission to create the database, or access to an existing database created for this application. TrustServerCertificate is a local development setting.

## Migration lifecycle

1. Configure the chosen database server.
2. Install matching EF Core runtime, design package and CLI tool versions.
3. Generate `InitialCreate` into Infrastructure; use API as the startup project.
4. Review the generated migration before applying it.
5. Apply it with `dotnet ef database update`.
6. Commit migration files and the model snapshot.

Normal application runs reuse the database. Create a new migration only when the EF model/schema changes. Inserting events does not require a migration. Do not delete the database or migration history on startup, and do not mix EnsureCreated with this migration workflow.

## Planned increments

1. Add Application-level assessment tests using an in-memory test double.
2. Add additional fraud rules and define how their scores combine.
3. Define continuous-detection lookback; basic assessment persistence is implemented.
4. Add an Angular analyst interface.
5. Add local Neo4j, shared-identifier queries and graph data science exercises.
6. Implement the scaffolded `BankingFraudDetector.AI` project with Microsoft Agent Framework agents, workflow orchestration, tool registration and local Ollama model access. It will reference Application; Domain and Application will remain independent of the framework.
7. Add `BankingFraudDetector.McpServer` as a separate executable exposing selected Application use cases through MCP. Its composition root will register Infrastructure. The AI project will consume tools through an MCP client; it will not reference the server project directly.
8. Add analyst feedback, labelled synthetic scenarios and evaluation of detection and agent outputs.
9. Add Microsoft Entra login to Angular, API token validation and role/policy authorization.

Initial AI tools will be narrow operations such as GetAccountEvents, AssessAccountRisk and FindLinkedAccounts. Direct function tools can come first; MCP adds a reusable protocol boundary. Tools should return structured evidence. Analyst-approved actions will be introduced with explicit approval controls.

Ollama is the local model runtime; Microsoft Agent Framework coordinates agents and tools. Select and evaluate a model that supports tool calling. LLM investigation supplements the deterministic risk assessment; it should not silently overwrite rule results.

## Running the application

```powershell
dotnet build BankingFraudDetector.slnx
dotnet run --project BankingFraudDetector/BankingFraudDetector.csproj
```

Open the Aspire dashboard URL printed in the terminal, select the fraud-api HTTPS endpoint and visit `/health`. Currently this confirms HTTP responsiveness only; it does not test database connectivity.

## References

- [EF Core migrations](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
- [SQL Server LocalDB](https://learn.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb)
- [Microsoft Agent Framework tools](https://learn.microsoft.com/en-us/agent-framework/agents/tools/)


## Event ingestion increment

Implemented the EF event repository and Application recording use case, registered dependencies, and exposed POST /api/account-events through AccountEventsController. Moved the responsiveness endpoint to HealthController and added HTTP sample requests. Corrected the Persistence folder spelling. No schema change was needed. Validation: API build passed; existing NU1902/NU1903 dependency warnings remain. HTTP samples are provided for manual execution.


## Current risk assessment behavior

GET /api/account-events queries account history within an inclusive-start, exclusive-end window. GET /api/risk-assessments/{accountId} evaluates that history without storing an assessment. ATO-001 matches password reset, then MFA enrolment, then contact change with strictly increasing timestamps within 30 minutes (boundary included). It contributes 80 points once per evaluation and returns three evidence event IDs. Current thresholds are Low below 40, Medium from 40, High from 80. No match means no implemented pattern was found; it does not prove safety. Sequences beginning before the query window are excluded.

Validation completed in the preceding increment: all nine domain cases passed; API built to a separate verification output because the running API locked normal outputs. Existing dependency warnings remain. HTTP scenarios are manual requests, not automated tests. In BankingFraudDetector.Api.http, set hostAddress from Aspire, send the three risk-scenario POSTs, then the assessment GET; expect 80 / High / ATO-001. The shortened window expects 0 / Low, and reversed boundaries expect 400. Each repeated POST inserts another event.

Additional Application tests are deferred at the user's request. Existing domain tests remain in the solution.


## Latest increment: assessment history and AI boundary

- Added account assessment history, returning the newest 50 summaries through `GET /api/assessments/accounts/{accountId}`.
- Added the AI class library referencing Application, plus `IAssessmentInvestigator` and `InvestigationSummary` contracts. The AI boundary was subsequently implemented in the local Ollama investigation increment below.
- `PAY-001` detects bank details changed followed by a sale request within 60 minutes and contributes 60 points. Scores add across matching rules; both current rules produce 140 points / High. Points are not probabilities.
- `POST /api/assessments` runs and saves an assessment; `GET /api/assessments/{assessmentId}` returns its stored findings and evidence IDs.
- `AlignAssessmentFindingMapping` corrects the original singular finding table name and applies configured field lengths. Keep all migrations; do not recreate the database.
- Earlier live verification saved and retrieved an assessment with score 80, High, ATO-001 and three evidence IDs. The newest history endpoint still needs manual HTTP verification.

## First-time setup on another machine

Install the .NET 10 SDK, the Aspire tooling required by the AppHost, and SQL Server (or LocalDB). Configure `ConnectionStrings:FraudDatabase` for your own instance in the API development settings; the checked-in value is the original developer's local Windows-authenticated server, not a portable server address. No SQL password is included.

```powershell
dotnet restore BankingFraudDetector.slnx
dotnet tool restore
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet ef database update --project BankingFraudDetector.Infrastructure --startup-project BankingFraudDetector.Api
dotnet run --project BankingFraudDetector/BankingFraudDetector.csproj
```

Database update creates the database if missing and applies only outstanding migrations. Normal API startup neither drops nor recreates it. Local database contents and downloaded model weights are not part of this repository.

Use `BankingFraudDetector.Api/BankingFraudDetector.Api.http` for manual scenarios, setting its host address to the API endpoint shown in Aspire. Start with the event POST requests before assessing the corresponding accounts.

## AI and remaining roadmap

Ollama and qwen3:4b-instruct are now installed on the original development machine. The intended starting model is `qwen3:4b-instruct`. A fresh environment needs its own Ollama installation and model download. Model and tool-call behavior must be verified before wiring agents.

Remaining features: live tool-call evaluation, a separate MCP server, Angular analyst UI, local Neo4j and graph data science, and Microsoft Entra authentication/authorization. These are planned capabilities, not completed integrations. Use synthetic account data during development.

Known dependency issue: restore reports NU1902/NU1903 for transitive `System.Security.Cryptography.Pkcs` 9.0.11. It has not been resolved in this increment.


Latest validation: the complete solution built successfully in Release configuration. Existing NU1902/NU1903 warnings remain. Additional unit tests were deferred as requested.

## Local Ollama investigation increment

- Added Microsoft.Agents.AI 1.26.0 and OllamaSharp 5.5.0, configuration validation through OllamaOptions, and dependency registration.
- POST /api/assessments/{assessmentId}/investigations returns an AI explanation of a saved assessment. Missing assessments return 404; downstream timeouts return 504 when the caller has not cancelled.
- The client uses a configurable timeout (300 seconds by default). Local generation and multiple tool-call rounds may be slow.
- AssessmentInvestigationTools keeps tool implementations separate from agent orchestration. GetAssessmentEvidence exposes the loaded findings; GetAccountActivityEvents queries events for the assessment's fixed account and window.
- AIFunctionFactory exposes only the two registered tool definitions. The model requests calls and Agent Framework executes the C# methods. The code rejects an investigation if the evidence tool was never called; instructions alone do not guarantee call order or factual accuracy.
- Current activity can contain events inserted after the saved assessment. It does not change the saved score. Summaries are returned without persistence.
- Start Ollama with qwen3:4b-instruct available, restart the API, and use the investigation POST in the HTTP file with an existing assessment ID. Breakpoints in tool methods reveal model selections.
- No database migration was needed. Live model/tool behavior has not been verified by the agent in this increment; the earlier request exceeded the original 100-second timeout.
