# Analyst workspace

Angular 21 client for the existing .NET API. Requires Node 22.12+ within the Node 22 release line (this machine uses 22.15).

## Development

```powershell
npm ci
npm start
```

Open http://localhost:4200. Start the .NET API separately through Aspire or its HTTPS launch profile. Set API_TARGET to the API endpoint shown by Aspire when it differs from the default, then run npm start. The proxy's secure:false setting is only for the local development certificate.

The account form opens saved assessment history and graph context. It does not automatically create assessments or invoke AI. The seeded example account is 22222222-2222-2222-2222-222222222222. Choose a window to run and save a new assessment. Date inputs use local browser time and are sent to the API in UTC. Review an assessment to inspect rules/evidence and explicitly request a local AI investigation. Graph trace buttons show the current bounded connection path.

API calls are in src/app/fraud-api.ts. UI state and actions are in src/app/app.ts; layout and styling are in app.html and app.css. No .NET CORS change is needed with the development proxy. Authentication and Aspire frontend orchestration remain subsequent increments.

```powershell
npm run build
```

Production output is in dist/BankingFraudDetector.Client/browser. Deploy with a same-origin /api reverse proxy to the .NET backend; the development proxy is not included in production output. Never deploy using ng serve.


npm start now checks /health and starts the default local .NET API when it is not running. Requires .NET SDK and configured API user secrets. For Aspire, start the host first and set $env:API_TARGET to its API HTTPS endpoint if different. With an explicit target, startup fails clearly if unreachable. Ctrl+C cleans up only owned processes. Build and serve production output with a proper /api reverse proxy; proxy.conf.cjs is development-only.
