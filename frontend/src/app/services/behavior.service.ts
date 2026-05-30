import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../environments/environment';
import { BehaviorEntry, BehaviorEntryPayload } from '../models/wellness.models';

@Injectable({ providedIn: 'root' })
export class BehaviorService {
  private readonly http = inject(HttpClient);

  getAll() {
    return this.http.get<BehaviorEntry[]>(`${environment.apiUrl}/behavior`);
  }

  create(payload: BehaviorEntryPayload) {
    return this.http.post<BehaviorEntry>(`${environment.apiUrl}/behavior`, payload);
  }

  update(id: string, payload: BehaviorEntryPayload) {
    return this.http.put<BehaviorEntry>(`${environment.apiUrl}/behavior/${id}`, payload);
  }

  delete(id: string) {
    return this.http.delete<void>(`${environment.apiUrl}/behavior/${id}`);
  }
}
