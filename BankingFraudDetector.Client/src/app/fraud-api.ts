import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';

export interface Assessment {
  id: string; accountId: string; assessedAtUtc: string; fromInclusiveUtc: string;
  toExclusiveUtc: string; totalScore: number; riskBand: string; ruleSetVersion: string;
  findings?: { id: string; ruleCode: string; score: number; explanation: string; evidenceEventIds: string[] }[];
}
export interface PathResult {
  found: boolean; maxHops: number; limitation: string;
  path: { nodes: { type: string; identifier: string }[] } | null;
}
@Injectable({ providedIn: 'root' })
export class FraudApi {
  private http = inject(HttpClient);
  history(account: string) { return firstValueFrom(this.http.get<Assessment[]>(`/api/assessments/accounts/${account}`)); }
  assessment(id: string) { return firstValueFrom(this.http.get<Assessment>(`/api/assessments/${id}`)); }
  run(accountId: string, fromInclusive: string, toExclusive: string) {
    return firstValueFrom(this.http.post<{ assessmentId: string }>('/api/assessments', { accountId, fromInclusive, toExclusive }));
  }
  direct(account: string) { return firstValueFrom(this.http.get<{ accountId: string; sharedDeviceId: string }[]>(`/api/accounts/${account}/graph/shared-devices`)); }
  connected(account: string) { return firstValueFrom(this.http.get<{ connectedAccounts: { accountId: string }[] }>(`/api/accounts/${account}/graph/connected-accounts`)); }
  path(account: string, target: string) { return firstValueFrom(this.http.get<PathResult>(`/api/accounts/${account}/graph/paths/${target}`)); }
  investigate(id: string) { return firstValueFrom(this.http.post<{ summary: string; model: string }>(`/api/assessments/${id}/investigations`, {})); }
}
export function errorMessage(error: unknown): string {
  if (error instanceof HttpErrorResponse) {
    if (error.error && typeof error.error === 'object' && (error.error.detail || error.error.title)) {
      return error.error.detail || error.error.title;
    }
    if (error.status === 0 || error.status === 502) return 'Cannot reach the API. Check that it is running and the development proxy target is correct.';
    return error.error?.detail || error.error?.title || `Request failed (${error.status}). Check the API logs for details.`;
  }
  return 'The request could not be completed. Please try again.';
}


