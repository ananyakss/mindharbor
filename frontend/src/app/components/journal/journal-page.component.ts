import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize } from 'rxjs';
import { JournalEntryModel } from '../../models/wellness.models';
import { JournalEntriesService } from '../../services/journal-entries.service';
import { MlService } from '../../services/ml.service';

@Component({
  selector: 'app-journal-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './journal-page.component.html',
  styleUrls: ['./journal-page.component.css']
})
export class JournalPageComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly journalApi = inject(JournalEntriesService);
  private readonly mlService = inject(MlService);

  protected readonly entries = signal<JournalEntryModel[]>([]);
  protected readonly loading = signal(false);
  protected readonly saving = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly editingId = signal<string | null>(null);
  protected readonly predictionError = signal<string | null>(null);

  protected form = this.fb.nonNullable.group({
    entryText: ['', [Validators.required, Validators.maxLength(20000)]],
    allowAnalysisConsent: [false]
  });

  ngOnInit(): void {
    this.load();
  }

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const v = this.form.getRawValue();
    const payload = {
      entryText: v.entryText.trim(),
      allowAnalysisConsent: v.allowAnalysisConsent
    };

    this.saving.set(true);
    this.error.set(null);
    this.predictionError.set(null);
    const id = this.editingId();
    const req$ = id ? this.journalApi.update(id, payload) : this.journalApi.create(payload);

    req$
      .pipe(finalize(() => this.saving.set(false)))
      .subscribe({
        next: (savedEntry) => {
          this.reset();
          if (payload.allowAnalysisConsent) {
            this.persistTextPrediction(payload.entryText, savedEntry.id);
          } else {
            this.load();
          }
        },
        error: () => this.error.set('Could not save journal entry.')
      });
  }

  protected edit(entry: JournalEntryModel): void {
    this.editingId.set(entry.id);
    this.form.reset({
      entryText: entry.entryText,
      allowAnalysisConsent: entry.allowAnalysisConsent
    });
  }

  protected delete(id: string): void {
    if (!confirm('Delete this journal entry?')) return;
    this.journalApi.delete(id).subscribe({
      next: () => this.load(),
      error: () => this.error.set('Could not delete.')
    });
  }

  protected cancelEdit(): void {
    this.reset();
  }

  protected sentimentRiskLabel(sentiment?: string | null): string | null {
    if (!sentiment) return null;
    const value = sentiment.toLowerCase();
    if (value === 'positive') return 'LOW RISK';
    if (value === 'negative') return 'HIGH RISK';
    return null;
  }

  private load(): void {
    this.loading.set(true);
    this.error.set(null);
    this.journalApi
      .getAll()
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (rows) => this.entries.set(rows),
        error: () => this.error.set('Unable to load journal.')
      });
  }

  private reset(): void {
    this.editingId.set(null);
    this.form.reset({ entryText: '', allowAnalysisConsent: false });
  }

  private persistTextPrediction(entryText: string, entryId: string): void {
    this.mlService.predictText(entryText, entryId).subscribe({
      next: () => {
        this.predictionError.set(null);
        this.load();
      },
      error: (err) => {
        this.predictionError.set(err?.error?.message ?? 'Text analysis is unavailable right now.');
        this.load();
      }
    });
  }
}
