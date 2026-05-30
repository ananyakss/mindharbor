import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import {
  SelfConfidenceEntry,
  TherapyWorksheetsApiService
} from '../../services/therapy-worksheets-api.service';

@Component({
  selector: 'app-self-confidence',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './self-confidence.component.html',
  styleUrls: ['./worksheet.component.css']
})
export class SelfConfidenceComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly therapyWorksheets = inject(TherapyWorksheetsApiService);

  protected readonly saving = signal(false);
  protected readonly message = signal<string | null>(null);
  protected readonly entries = signal<SelfConfidenceEntry[]>([]);
  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly editingId = signal<string | null>(null);

  protected readonly form = this.fb.nonNullable.group({
    identifyTrigger: ['', [Validators.required]],
    supportingEvidence: ['', [Validators.required]],
    rationalizeDoubt: ['', [Validators.required]],
    actionPlan: ['', [Validators.required]]
  });

  ngOnInit(): void {
    this.loadEntries();
  }

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const payload = this.form.getRawValue();
    const id = this.editingId();

    this.saving.set(true);
    this.message.set(null);
    this.error.set(null);

    const req$ = id
      ? this.therapyWorksheets.updateSelfConfidence(id, payload)
      : this.therapyWorksheets.createSelfConfidence(payload);

    req$.pipe(finalize(() => this.saving.set(false))).subscribe({
      next: () => {
        this.message.set(id ? 'Updated 🌟' : 'Saved! 🌟');
        this.editingId.set(null);
        this.form.reset();
        this.loadEntries();
      },
      error: (err) =>
        this.message.set(err?.error?.message ?? 'Could not save.')
    });
  }

  protected editEntry(entry: SelfConfidenceEntry): void {
    this.editingId.set(entry.id);
    this.message.set(null);
    this.form.patchValue({
      identifyTrigger: entry.identifyTrigger,
      supportingEvidence: entry.supportingEvidence,
      rationalizeDoubt: entry.rationalizeDoubt,
      actionPlan: entry.actionPlan
    });
  }

  protected deleteEntry(id: string): void {
    if (!confirm('Delete this worksheet?')) return;
    this.therapyWorksheets.deleteSelfConfidence(id).subscribe({
      next: () => {
        if (this.editingId() === id) {
          this.cancelEdit();
        }
        this.loadEntries();
      },
      error: () => this.error.set('Could not delete entry.')
    });
  }

  protected cancelEdit(): void {
    this.editingId.set(null);
    this.form.reset();
    this.message.set(null);
  }

  private loadEntries(): void {
    this.loading.set(true);
    this.error.set(null);
    this.therapyWorksheets
      .getSelfConfidenceEntries()
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (rows) => this.entries.set(rows),
        error: () => this.error.set('Unable to load saved worksheets.')
      });
  }
}
