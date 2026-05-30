import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../environments/environment';
import { MoodEntry, MoodEntryPayload } from '../models/wellness.models';

@Injectable({ providedIn: 'root' })
export class MoodService {
  private readonly http = inject(HttpClient);

  getAll() {
    return this.http.get<MoodEntry[]>(`${environment.apiUrl}/mood`);
  }

  create(payload: MoodEntryPayload) {
    return this.http.post<MoodEntry>(`${environment.apiUrl}/mood`, payload);
  }

  update(id: string, payload: MoodEntryPayload) {
    return this.http.put<MoodEntry>(`${environment.apiUrl}/mood/${id}`, payload);
  }

  delete(id: string) {
    return this.http.delete<void>(`${environment.apiUrl}/mood/${id}`);
  }
}
