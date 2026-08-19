import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import {
  BehaviorPredictionResult,
  TextPredictionResult
} from '../models/wellness.models';
import { environment } from '../environments/environment';

export interface BehaviorPredictionInput {
  sleep: number;
  steps: number;
  diet: number;
  social: number;
  hobbies: number;
  stress: number;
}

@Injectable({ providedIn: 'root' })
export class MlService {
  private readonly http = inject(HttpClient);

  predictBehavior(payload: BehaviorPredictionInput) {
    return this.http.post<BehaviorPredictionResult>(`${environment.apiUrl}/predict/behavior`, payload);
  }

  predictText(journalText: string) {
    return this.http.post<TextPredictionResult>(`${environment.apiUrl}/predict/text`, { journalText });
  }
}
