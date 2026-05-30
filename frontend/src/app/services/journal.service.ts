import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../environments/environment';
import {
  CbtEntry,
  CbtEntryPayload,
  DailyJournalEntry,
  DailyJournalPayload,
  ErpEntry,
  ErpEntryPayload
} from '../models/journal.models';

@Injectable({ providedIn: 'root' })
export class JournalService {
  private readonly http = inject(HttpClient);

  getDailyEntries() {
    return this.http.get<DailyJournalEntry[]>(`${environment.apiUrl}/daily`);
  }

  createDailyEntry(payload: DailyJournalPayload) {
    return this.http.post<DailyJournalEntry>(`${environment.apiUrl}/daily`, payload);
  }

  updateDailyEntry(id: string, payload: DailyJournalPayload) {
    return this.http.put<DailyJournalEntry>(`${environment.apiUrl}/daily/${id}`, payload);
  }

  deleteDailyEntry(id: string) {
    return this.http.delete<void>(`${environment.apiUrl}/daily/${id}`);
  }

  getCbtEntries() {
    return this.http.get<CbtEntry[]>(`${environment.apiUrl}/cbt`);
  }

  createCbtEntry(payload: CbtEntryPayload) {
    return this.http.post<CbtEntry>(`${environment.apiUrl}/cbt`, payload);
  }

  updateCbtEntry(id: string, payload: CbtEntryPayload) {
    return this.http.put<CbtEntry>(`${environment.apiUrl}/cbt/${id}`, payload);
  }

  deleteCbtEntry(id: string) {
    return this.http.delete<void>(`${environment.apiUrl}/cbt/${id}`);
  }

  getErpEntries() {
    return this.http.get<ErpEntry[]>(`${environment.apiUrl}/erp`);
  }

  createErpEntry(payload: ErpEntryPayload) {
    return this.http.post<ErpEntry>(`${environment.apiUrl}/erp`, payload);
  }

  updateErpEntry(id: string, payload: ErpEntryPayload) {
    return this.http.put<ErpEntry>(`${environment.apiUrl}/erp/${id}`, payload);
  }

  deleteErpEntry(id: string) {
    return this.http.delete<void>(`${environment.apiUrl}/erp/${id}`);
  }
}

