import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../environments/environment';
import { TherapyWorksheetSavedEntry } from '../models/wellness.models';

export type SelfValidationPayload = {
  step2: { whatHappened: string; howIFelt: string; whatINeeded: string };
  step3: { acknowledgeFeeling: string; separateIdentity: string; reframeNarrative: string };
  step4: { externalInfluences: string; boundaries: string };
  step5: { immediateAction: string; futurePrevention: string; alternativePerspectives: string };
  notes: string;
};

export type SelfConfidencePayload = {
  identifyTrigger: string;
  supportingEvidence: string;
  rationalizeDoubt: string;
  actionPlan: string;
};

export type SelfWorthPayload = {
  identifyTrigger: string;
  impactAnalysis: string;
  realityCheck: string;
  definesSelfWorth: string;
  doesNotDefineSelfWorth: string;
  actsOfCare: string;
};

export type SchemaAbandonmentPayload = {
  trigger: string;
  coreBelief: string;
  emotionalResponse: string;
  copingBehavior: string;
  healthierAlternativeBelief: string;
  whenUnsupported: string;
  expectFromOthers: string;
  whatActuallyHappened: string;
};

export type SchemaFailurePayload = {
  trigger: string;
  coreBelief: string;
  emotionalResponse: string;
  copingBehavior: string;
  healthierAlternativeBelief: string;
  evidenceSupports: string;
  evidenceContradicts: string;
};

export type SchemaPeoplePleasingPayload = {
  trigger: string;
  coreBelief: string;
  emotionalResponse: string;
  copingBehavior: string;
  healthierAlternativeBelief: string;
  whenIgnoreNeeds: string;
  afraidWillHappen: string;
};

export type SchemaSuppressionPayload = {
  trigger: string;
  coreBelief: string;
  emotionalResponse: string;
  copingBehavior: string;
  healthierAlternativeBelief: string;
  emotionsAvoided: string;
  whySuppress: string;
};

export type SelfValidationEntry = SelfValidationPayload & TherapyWorksheetSavedEntry;
export type SelfConfidenceEntry = SelfConfidencePayload & TherapyWorksheetSavedEntry;
export type SelfWorthEntry = SelfWorthPayload & TherapyWorksheetSavedEntry;
export type SchemaAbandonmentEntry = SchemaAbandonmentPayload & TherapyWorksheetSavedEntry;
export type SchemaFailureEntry = SchemaFailurePayload & TherapyWorksheetSavedEntry;
export type SchemaPeoplePleasingEntry = SchemaPeoplePleasingPayload & TherapyWorksheetSavedEntry;
export type SchemaSuppressionEntry = SchemaSuppressionPayload & TherapyWorksheetSavedEntry;

@Injectable({ providedIn: 'root' })
export class TherapyWorksheetsApiService {
  private readonly http = inject(HttpClient);
  private readonly base = environment.apiUrl;

  getSelfValidationEntries() {
    return this.http.get<SelfValidationEntry[]>(`${this.base}/self-validation`);
  }

  createSelfValidation(payload: SelfValidationPayload) {
    return this.http.post<SelfValidationEntry>(`${this.base}/self-validation`, payload);
  }

  updateSelfValidation(id: string, payload: SelfValidationPayload) {
    return this.http.put<SelfValidationEntry>(`${this.base}/self-validation/${id}`, payload);
  }

  deleteSelfValidation(id: string) {
    return this.http.delete<void>(`${this.base}/self-validation/${id}`);
  }

  getSelfConfidenceEntries() {
    return this.http.get<SelfConfidenceEntry[]>(`${this.base}/self-confidence`);
  }

  createSelfConfidence(payload: SelfConfidencePayload) {
    return this.http.post<SelfConfidenceEntry>(`${this.base}/self-confidence`, payload);
  }

  updateSelfConfidence(id: string, payload: SelfConfidencePayload) {
    return this.http.put<SelfConfidenceEntry>(`${this.base}/self-confidence/${id}`, payload);
  }

  deleteSelfConfidence(id: string) {
    return this.http.delete<void>(`${this.base}/self-confidence/${id}`);
  }

  getSelfWorthEntries() {
    return this.http.get<SelfWorthEntry[]>(`${this.base}/self-worth`);
  }

  createSelfWorth(payload: SelfWorthPayload) {
    return this.http.post<SelfWorthEntry>(`${this.base}/self-worth`, payload);
  }

  updateSelfWorth(id: string, payload: SelfWorthPayload) {
    return this.http.put<SelfWorthEntry>(`${this.base}/self-worth/${id}`, payload);
  }

  deleteSelfWorth(id: string) {
    return this.http.delete<void>(`${this.base}/self-worth/${id}`);
  }

  getSchemaAbandonmentEntries() {
    return this.http.get<SchemaAbandonmentEntry[]>(`${this.base}/schema-abandonment`);
  }

  createSchemaAbandonment(payload: SchemaAbandonmentPayload) {
    return this.http.post<SchemaAbandonmentEntry>(`${this.base}/schema-abandonment`, payload);
  }

  updateSchemaAbandonment(id: string, payload: SchemaAbandonmentPayload) {
    return this.http.put<SchemaAbandonmentEntry>(`${this.base}/schema-abandonment/${id}`, payload);
  }

  deleteSchemaAbandonment(id: string) {
    return this.http.delete<void>(`${this.base}/schema-abandonment/${id}`);
  }

  getSchemaFailureEntries() {
    return this.http.get<SchemaFailureEntry[]>(`${this.base}/schema-failure`);
  }

  createSchemaFailure(payload: SchemaFailurePayload) {
    return this.http.post<SchemaFailureEntry>(`${this.base}/schema-failure`, payload);
  }

  updateSchemaFailure(id: string, payload: SchemaFailurePayload) {
    return this.http.put<SchemaFailureEntry>(`${this.base}/schema-failure/${id}`, payload);
  }

  deleteSchemaFailure(id: string) {
    return this.http.delete<void>(`${this.base}/schema-failure/${id}`);
  }

  getSchemaPeoplePleasingEntries() {
    return this.http.get<SchemaPeoplePleasingEntry[]>(`${this.base}/schema-people-pleasing`);
  }

  createSchemaPeoplePleasing(payload: SchemaPeoplePleasingPayload) {
    return this.http.post<SchemaPeoplePleasingEntry>(`${this.base}/schema-people-pleasing`, payload);
  }

  updateSchemaPeoplePleasing(id: string, payload: SchemaPeoplePleasingPayload) {
    return this.http.put<SchemaPeoplePleasingEntry>(`${this.base}/schema-people-pleasing/${id}`, payload);
  }

  deleteSchemaPeoplePleasing(id: string) {
    return this.http.delete<void>(`${this.base}/schema-people-pleasing/${id}`);
  }

  getSchemaSuppressionEntries() {
    return this.http.get<SchemaSuppressionEntry[]>(`${this.base}/schema-suppression`);
  }

  createSchemaSuppression(payload: SchemaSuppressionPayload) {
    return this.http.post<SchemaSuppressionEntry>(`${this.base}/schema-suppression`, payload);
  }

  updateSchemaSuppression(id: string, payload: SchemaSuppressionPayload) {
    return this.http.put<SchemaSuppressionEntry>(`${this.base}/schema-suppression/${id}`, payload);
  }

  deleteSchemaSuppression(id: string) {
    return this.http.delete<void>(`${this.base}/schema-suppression/${id}`);
  }
}
