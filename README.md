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

Current uncommitted increment: database connection configuration, Infrastructure dependency registration, EF Core 10.0.14 local tooling and Design package, and the InitialCreate migration. Applied migration `20261029144956_InitialCreate` successfully to `Avinash_Bobba`, database `BankingFraudDetector`; created AccountEvents, its account/time index and the migration history entry. The API build succeeded. Restore reported existing NU1902/NU1903 warnings for transitive System.Security.Cryptography.Pkcs 9.0.11. This README is also new.

For each future increment, append its commit hash, purpose and validation after committing. Keep planned work separate from completed work.

## Current architecture

```text
Aspire AppHost -> API
API -> Application, Infrastructure
Infrastructure -> Application, Domain
Application -> Domain
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

1. Complete database registration and generate/apply the initial migration.
2. Implement the event repository and event ingestion use case/API endpoint.
3. Add deterministic event-sequence detection, time windows, risk bands and evidence.
4. Add an Angular analyst interface.
5. Add local Neo4j, shared-identifier queries and graph data science exercises.
6. Add `BankingFraudDetector.AI` for Microsoft Agent Framework agents, workflow orchestration, tool registration and local Ollama model access. It will reference Application; Domain and Application will remain independent of the framework.
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

