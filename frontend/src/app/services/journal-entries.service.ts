import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../environments/environment';
import { JournalEntryModel, JournalEntryPayload } from '../models/wellness.models';

@Injectable({ providedIn: 'root' })
export class JournalEntriesService {
  private readonly http = inject(HttpClient);

  getAll() {
    return this.http.get<JournalEntryModel[]>(`${environment.apiUrl}/journal`);
  }

  create(payload: JournalEntryPayload) {
    return this.http.post<JournalEntryModel>(`${environment.apiUrl}/journal`, payload);
  }

  update(id: string, payload: JournalEntryPayload) {
    return this.http.put<JournalEntryModel>(`${environment.apiUrl}/journal/${id}`, payload);
  }

  delete(id: string) {
    return this.http.delete<void>(`${environment.apiUrl}/journal/${id}`);
  }
}
