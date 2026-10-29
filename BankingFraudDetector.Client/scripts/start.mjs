import { spawn, spawnSync } from 'node:child_process';
import https from 'node:https';
import http from 'node:http';
import { fileURLToPath } from 'node:url';
import path from 'node:path';

const root = fileURLToPath(new URL('../', import.meta.url));
const target = new URL(process.env.API_TARGET || 'https://127.0.0.1:7064');
if (!['http:', 'https:'].includes(target.protocol) ||
    !['localhost', '127.0.0.1', '[::1]'].includes(target.hostname)) {
  throw new Error('API_TARGET must be a local HTTP or HTTPS API URL.');
}

function ready() {
  return new Promise(resolve => {
    const transport = target.protocol === 'https:' ? https : http;
    const request = transport.get(new URL('/health', target),
      { rejectUnauthorized: false, timeout: 2000 }, response => {
        response.resume();
        resolve(response.statusCode === 200);
      });
    request.on('timeout', () => request.destroy());
    request.on('error', () => resolve(false));
  });
}

let api;
let frontend;
let stopping = false;
function cleanup(code = 0) {
  if (stopping) return;
  stopping = true;
  for (const child of [frontend, api]) {
    if (!child || child.exitCode !== null) continue;
    if (process.platform === 'win32') {
      spawnSync('taskkill', ['/PID', String(child.pid), '/T', '/F'], { stdio: 'ignore' });
    } else child.kill('SIGTERM');
  }
  process.exit(code);
}
process.on('SIGINT', () => cleanup());
process.on('SIGTERM', () => cleanup());

console.log(`Checking API at ${target.origin} …`);
if (!await ready()) {
  if (process.env.API_TARGET) {
    console.error('Configured API_TARGET is unavailable. Start Aspire/API, then retry npm start.');
    process.exit(1);
  }
  console.log('Starting the local .NET API (HTTPS launch profile) …');
  api = spawn('dotnet', ['run', '--project',
    path.resolve(root, '../BankingFraudDetector.Api/BankingFraudDetector.Api.csproj'),
    '--launch-profile', 'https'], { cwd: root, stdio: 'inherit', windowsHide: true });
  api.on('error', error => { console.error(error.message); cleanup(1); });
  let available = false;
  for (let attempt = 0; attempt < 90; attempt++) {
    if (api.exitCode !== null) break;
    if (await ready()) { available = true; break; }
    await new Promise(resolve => setTimeout(resolve, 1000));
  }
  if (!available) { console.error('API startup failed. Check .NET configuration and user secrets in the output above.'); cleanup(1); }
} else console.log('Using the already-running API; it will not be stopped by this script.');

frontend = spawn(process.execPath,
  [path.join(root, 'node_modules/@angular/cli/bin/ng.js'), 'serve', '--proxy-config',
    path.join(root, 'proxy.conf.cjs')],
  { cwd: root, stdio: 'inherit', windowsHide: true });
frontend.on('error', error => { console.error(error.message); cleanup(1); });
frontend.on('exit', code => cleanup(code ?? 1));
api?.on('exit', code => { if (!stopping) { console.error('The managed API stopped.'); cleanup(code || 1); } });
