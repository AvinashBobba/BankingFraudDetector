import { Component, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Assessment, FraudApi, PathResult, errorMessage } from './fraud-api';

@Component({ selector: 'app-root', imports: [FormsModule, DatePipe], templateUrl: './app.html', styleUrl: './app.css' })
export class App {
  private api = inject(FraudApi);
  private revision = 0;
  private selection = 0;
  accountInput = '22222222-2222-2222-2222-222222222222';
  account = signal('');
  from = '2026-10-29T00:00';
  to = '2026-10-30T00:00';
  loading = signal(false); saving = signal(false); thinking = signal(false); pathLoading = signal(false);
  error = signal(''); graphError = signal(''); detailError = signal(''); aiError = signal(''); pathError = signal('');
  history = signal<Assessment[]>([]); selected = signal<Assessment | null>(null);
  direct = signal<{accountId: string; sharedDeviceId: string}[]>([]);
  connected = signal<{accountId: string}[]>([]); path = signal<PathResult | null>(null);
  summary = signal(''); model = signal('');
  short(id: string) { return id.slice(0, 8) + '…' + id.slice(-4); }
  private validId(id: string) { return /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(id) && !/^0{8}-0{4}-0{4}-0{4}-0{12}$/.test(id); }
  async load() {
    const id = this.accountInput.trim().toLowerCase();
    if (!this.validId(id)) { this.error.set('Enter a nonempty account GUID.'); return; }
    const rev = ++this.revision; ++this.selection;
    this.account.set(id); this.loading.set(true); this.error.set(''); this.graphError.set(''); this.detailError.set('');
    this.history.set([]); this.selected.set(null); this.direct.set([]); this.connected.set([]); this.path.set(null); this.summary.set(''); this.aiError.set(''); this.pathError.set('');
    const [history, direct, connected] = await Promise.allSettled([this.api.history(id), this.api.direct(id), this.api.connected(id)]);
    if (rev !== this.revision) return;
    if (history.status === 'fulfilled') this.history.set(history.value); else this.error.set(errorMessage(history.reason));
    if (direct.status === 'fulfilled') this.direct.set(direct.value);
    if (connected.status === 'fulfilled') this.connected.set(connected.value.connectedAccounts);
    const failures = [direct, connected].filter(r => r.status === 'rejected') as PromiseRejectedResult[];
    if (failures.length) this.graphError.set(errorMessage(failures[0].reason));
    this.loading.set(false);
    if (this.history().length) await this.select(this.history()[0].id);
  }
  async select(id: string) {
    const token = ++this.selection; this.selected.set(null); this.summary.set(''); this.aiError.set(''); this.detailError.set('');
    try { const detail = await this.api.assessment(id); if (token === this.selection) this.selected.set(detail); }
    catch (e) { if (token === this.selection) this.detailError.set(errorMessage(e)); }
  }
  async run() {
    const start = new Date(this.from), end = new Date(this.to);
    if (!this.account() || !Number.isFinite(start.getTime()) || !Number.isFinite(end.getTime()) || start >= end) { this.error.set('Choose a valid start and end time, with start before end.'); return; }
    this.saving.set(true); this.error.set(''); const rev = this.revision;
    try {
      const result = await this.api.run(this.account(), start.toISOString(), end.toISOString());
      if (rev !== this.revision) return;
      const history = await this.api.history(this.account()); if (rev !== this.revision) return; this.history.set(history);
      if (rev === this.revision) await this.select(result.assessmentId);
    } catch(e) { if (rev === this.revision) this.error.set(errorMessage(e)); }
    finally { this.saving.set(false); }
  }
  async investigate() {
    const item = this.selected(); if (!item) return;
    const token = this.selection; this.thinking.set(true); this.aiError.set(''); this.summary.set('');
    try { const result = await this.api.investigate(item.id); if (token === this.selection) { this.summary.set(result.summary); this.model.set(result.model); } }
    catch(e) { if (token === this.selection) this.aiError.set(errorMessage(e)); }
    finally { this.thinking.set(false); }
  }
  async explain(target: string) {
    const rev = this.revision; this.pathLoading.set(true); this.path.set(null); this.pathError.set('');
    try { const result = await this.api.path(this.account(), target); if(rev === this.revision) this.path.set(result); }
    catch(e) { if(rev === this.revision) this.pathError.set(errorMessage(e)); }
    finally { this.pathLoading.set(false); }
  }
}

