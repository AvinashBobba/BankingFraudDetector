# Banking Fraud Detector
A learning project built manually in small, explainable increments. The first feature detects suspicious sequences of investor account events using deterministic rules. Agentic investigation and graph analytics follow once the underlying evidence is available.
<img width="1699" height="864" alt="image" src="https://github.com/user-attachments/assets/36f03683-b957-419f-93c8-7504f784d6ef" />

<img width="1706" height="598" alt="image" src="https://github.com/user-attachments/assets/0f3511a5-9157-40c4-aee2-802376e23f89" />

<img width="1700" height="862" alt="image" src="https://github.com/user-attachments/assets/5ae3096e-bc34-40bd-9639-5bfca8d2719e" />

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

## MCP integration increment

Added .NET stdio MCP server/client projects using ModelContextProtocol 2.3.0. The server exposes saved assessment evidence through Application; logs use stderr to keep stdout reserved for MCP. The console client lists tools and invokes the evidence tool with a saved assessment ID.

The AI evidence tool now calls AssessmentMcpGateway, which launches the compiled server DLL, discovers the evidence tool, forwards the fixed assessment ID, checks errors and disposes the connection. Activity queries and the initial assessment lookup still run in-process. The server receives the database connection explicitly and a minimal environment. Configure Mcp:ServerDllPath for your machine and build the server before an investigation.

This is a learning integration: a server process starts per evidence call. For our internal production path, direct Application queries remain the recommended default; MCP is useful as a reusable interface for other clients. No migration is required. End-to-end MCP/model execution has not been verified by the agent in this increment.

## Local Neo4j: destructive development reset

Docker Compose starts Neo4j Community with a named data volume and loopback-only ports. Copy .env.example to .env and choose a password, or use the generated local .env if already present. The actual .env is ignored by Git. Open http://localhost:7474 and sign in as neo4j with that password.

IMPORTANT: per the requested development behavior, EVERY container start deletes all graph nodes and relationships and inserts the synthetic graph again. This includes stop/start, restart, and container recreation. Constraints remain and are created if absent. This setup is not suitable for graphs whose data must be retained. SQL Server is unaffected.

```powershell
docker compose up -d --wait
docker compose stop neo4j
docker compose start --wait neo4j
docker compose restart neo4j
```

The startup wrapper waits for Neo4j, runs dev/neo4j/reset-and-seed.cypher and marks readiness only after successful seeding. Wait for healthy status before querying; restart returns before seeding necessarily completes. A failed seed leaves the container unhealthy. docker compose up on an already-running container does not reset it; a real start/restart does.

Seeds: three Account nodes, two Device nodes and three USED_DEVICE relationships. Accounts 2222... and 3333... share synthetic-device-shared; account 4444... uses synthetic-device-separate. View them with:

```cypher
MATCH (a:Account)-[r:USED_DEVICE]->(d:Device)
RETURN a, r, d;
```

Neo4j driver integration and graph data science are subsequent manual steps; no EF migration is involved here. Changing the .env password does not change credentials already stored in the named volume.

Verified locally: initial startup, container restart, and force recreation all reached healthy status. After restart/recreation a temporary ResetVerification node was removed and the graph contained exactly five nodes and three USED_DEVICE relationships. Shell syntax and Git whitespace checks passed.

## Shared-device investigation and GDS increment

Implemented IAccountGraph through Neo4jAccountGraph using a parameterized shared-device query. Added FindLinkedAccounts, GET /api/accounts/{accountId}/graph/shared-devices, manual HTTP samples and AccountGraphTools. The investigator now has a third function tool for linked accounts. Graph results are current context, not proof of fraud, and do not alter saved rule scores.

The API requires Neo4j:Uri, Neo4j:Username and Neo4j:Password. Store the password in API user secrets (the Compose .env is not loaded by the API). The driver is singleton; each graph query opens a session with a 15-second transaction timeout. Cancellation is checked around the operation, not passed to every driver operation.

Enabled graph-data-science through NEO4J_PLUGINS. Verified the running server reports GDS 2026.10.1. The full Release solution build passed; existing NU1902/NU1903 warnings remain. No new unit tests or live agent calls were run. No EF migration was required. The existing destructive graph reset on every start remains enabled.

### Next manual exercise: connected components

Run these statements separately in Neo4j Browser. Dropping the projection removes only its in-memory copy, not database nodes.

```cypher
CALL gds.graph.drop('fraud-device-graph', false);
```

```cypher
CALL gds.graph.project(
  'fraud-device-graph',
  ['Account', 'Device'],
  {USED_DEVICE: {orientation: 'UNDIRECTED'}}
)
YIELD graphName, nodeCount, relationshipCount
RETURN graphName, nodeCount, relationshipCount;
```

The seed projects five nodes and six directed adjacency entries (three stored relationships projected in both directions).

```cypher
CALL gds.wcc.stream('fraud-device-graph')
YIELD nodeId, componentId
WITH componentId, gds.util.asNode(nodeId) AS node
RETURN componentId,
       collect(coalesce(node.accountId, node.deviceId)) AS members,
       count(*) AS nodeCount
ORDER BY nodeCount DESC;
```

Expect two components with three and two nodes respectively. Component IDs can change across projections; they are not stable business identifiers. WCC identifies connected components, not fraudulent communities. Stream mode does not write results to stored nodes. The projection is an in-memory snapshot: rebuild after graph changes or a Neo4j restart. This exercise is documented for manual execution and has not been run by the agent.

## Connected-component investigation increment

Added ConnectedAccount, FindConnectedAccounts and a WCC-backed IAccountGraph query. GET /api/accounts/{accountId}/graph/connected-accounts returns other accounts in the same component; a missing projection maps to 503 through GraphProjectionUnavailableException. Registered the new use case in dependency injection.

AccountGraphTools now exposes connected-component analysis alongside direct shared-device analysis. Unavailable projection results are explicitly distinguished from no connections. Component membership is snapshot-based context and does not explain an exact path or prove fraud. WCC currently runs per query and needs a scheduled/precomputed approach before use on large graphs.

Extended synthetic seeds with account 5555... connected to 3333... through synthetic-device-bridge. Account 2222... directly shares a device with 3333..., while its component also includes 5555.... The startup seed script now automatically rebuilds fraud-device-graph after seeding; graph readiness follows projection creation. All existing destructive reset behavior remains enabled.

Verified the running projection contains seven nodes and ten projected relationships. The Release solution build passed with existing dependency warnings. Live controller/model behavior was not exercised in this increment. No EF migration was needed. Next: connection-path evidence for indirect links.

## Neo4j MVP scope complete: connection-path evidence

Added IAccountConnectionPaths, FindAccountConnectionPath and a Neo4j implementation returning one shortest USED_DEVICE path within six relationship hops. The query is parameterized, has a 15-second transaction timeout and reads the current database without a GDS projection. Ordered path nodes distinguish Account and Device identifiers. Equal/empty IDs are rejected; no result means no path found within the bound (or missing endpoints), not proof of no relationship.

GET /api/accounts/{accountId}/graph/paths/{targetAccountId} returns found, maxHops and path. AccountGraphTools exposes GetAccountConnectionPath with the source account fixed by the assessment, and the investigator registers it alongside existing tools. Instructions require returned path evidence before describing intermediate connections. API sample requests cover an indirect path, no path and invalid identical accounts.

Validation: full Release build passed with existing dependency warnings. A temporary local API instance was used to verify direct links (3333...), component membership (3333... and 5555...), the ordered four-hop path from 2222... to 5555..., no path to 4444..., and HTTP 400 for identical endpoints. Live model selection of the new tool was not tested. No new unit tests or EF migrations were added.

This closes the current graph MVP scope: destructive local reset/seeding, GDS projection readiness, shared-device lookup, WCC membership, path evidence, API endpoints and agent tools. More graph algorithms, bank/email/IP graph expansion, temporal projections, ingestion synchronization and production-scale analytics scheduling are future requirements, not blockers for moving to Angular. WCC still runs per request on a snapshot, and current graph evidence may differ from saved assessment-time data. The development graph resets on every restart as requested.

## Angular analyst workspace

Added BankingFraudDetector.Client using Angular 21 (compatible with the installed Node 22.15). The local dashboard looks up account assessment history, runs and saves deterministic assessments for a selected time window, displays saved rule findings and evidence IDs, queries direct and component graph links, traces connection paths, and invokes the local AI investigator. It uses live API data with explicit loading, empty and failure states. Model summaries are rendered as text and are not saved. Dates entered in the form use the browser's local time and are converted to UTC for the API.

Start the existing API/Aspire host, then:

```powershell
cd BankingFraudDetector.Client
npm ci
npm start
```

Open http://localhost:4200. The Angular development proxy forwards /api/** to https://localhost:7064; update proxy.conf.cjs to the API HTTPS address shown by Aspire if different, then restart Angular. Certificate verification is disabled only in this local proxy for the development certificate. No .NET CORS changes or database migrations were needed. The client launcher reuses a running API or starts the local API when the default endpoint is unavailable. For deployment, configure a same-origin reverse proxy for /api or an explicit production API configuration; ng serve is development tooling.

Use seeded account 22222222-2222-2222-2222-222222222222 to inspect existing assessments and graph context. Run a new assessment only when intentionally creating another saved record. AI requests can take several minutes and require Ollama and the configured MCP server DLL. Authentication remains a later milestone.

Client validation: Angular production build passed; development server returned HTTP 200. Verified assessment history and detail through the proxy, then direct links, two connected accounts and a five-node indirect path against a temporary current API instance. The pre-existing API on 7064 was stale and must be restarted to use the latest graph changes. Temporary verification servers were stopped and the proxy target restored to 7064. No additional .NET source changes were made for the dashboard. Live AI generation, saving a new assessment through the browser and visual browser QA were not performed.


## Dashboard startup and connection fix

Run npm start from BankingFraudDetector.Client. The launcher probes the API /health endpoint before serving Angular. It reuses an already-running API; if the default local endpoint is unavailable, it runs the API with its HTTPS launch profile and waits for readiness. Ctrl+C stops only processes launched by this script, leaving a pre-existing API alone. This starts the API directly, not the Aspire host. Start Neo4j separately with docker compose up -d --wait and ensure API user secrets are configured.

For an existing Aspire-managed API with a different endpoint:

```powershell
$env:API_TARGET = "https://localhost:YOUR_API_PORT"
npm start
```

With an explicit API_TARGET the launcher requires that endpoint to be running and does not start another API. Remove the variable with Remove-Item Env:API_TARGET to restore defaults. All browser calls remain same-origin /api requests; proxy.conf.cjs chooses the target, defaulting to https://127.0.0.1:7064. Development certificate validation is bypassed only for local startup/proxy checks. No .NET CORS changes were introduced.

The observed error was reproduced as ECONNREFUSED from the development proxy when no API listener was available. Verified the repaired launcher with an already-running API, Angular HTTP responses, three history records, two component members and a five-node connection path through port 4200. The auto-start branch is provided but was not exercised against the user's already-running API. Failed history loading is distinguished from an empty history in the UI. Live browser visual QA and AI generation remain unverified.
