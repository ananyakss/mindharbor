import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import {
  SelfValidationEntry,
  TherapyWorksheetsApiService
} from '../../services/therapy-worksheets-api.service';

@Component({
  selector: 'app-self-validation',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './self-validation.component.html',
  styleUrls: ['./worksheet.component.css']
})
export class SelfValidationComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly therapyWorksheets = inject(TherapyWorksheetsApiService);

  protected readonly saving = signal(false);
  protected readonly message = signal<string | null>(null);
  protected readonly entries = signal<SelfValidationEntry[]>([]);
  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly editingId = signal<string | null>(null);

  protected readonly form = this.fb.nonNullable.group({
    step2: this.fb.nonNullable.group({
      whatHappened: ['', [Validators.required]],
      howIFelt: ['', [Validators.required]],
      whatINeeded: ['', [Validators.required]]
    }),
    step3: this.fb.nonNullable.group({
      acknowledgeFeeling: ['', [Validators.required]],
      separateIdentity: ['', [Validators.required]],
      reframeNarrative: ['', [Validators.required]]
    }),
    step4: this.fb.nonNullable.group({
      externalInfluences: ['', [Validators.required]],
      boundaries: ['', [Validators.required]]
    }),
    step5: this.fb.nonNullable.group({
      immediateAction: ['', [Validators.required]],
      futurePrevention: ['', [Validators.required]],
      alternativePerspectives: ['', [Validators.required]]
    }),
    notes: ['', []]
  });

  ngOnInit(): void {
    this.loadEntries();
  }

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const v = this.form.getRawValue();
    const payload = { ...v, notes: v.notes ?? '' };
    const id = this.editingId();

    this.saving.set(true);
    this.message.set(null);
    this.error.set(null);

    const req$ = id
      ? this.therapyWorksheets.updateSelfValidation(id, payload)
      : this.therapyWorksheets.createSelfValidation(payload);

    req$.pipe(finalize(() => this.saving.set(false))).subscribe({
      next: () => {
        this.message.set(
          id
            ? 'Updated 💛 Your past entries are below.'
            : 'Saved with care 💛 You can start a fresh worksheet anytime.'
        );
        this.editingId.set(null);
        this.resetForm();
        this.loadEntries();
      },
      error: (err) =>
        this.message.set(err?.error?.message ?? 'Could not save — please try again.')
    });
  }

  protected editEntry(entry: SelfValidationEntry): void {
    this.editingId.set(entry.id);
    this.message.set(null);
    this.form.patchValue({
      step2: entry.step2,
      step3: entry.step3,
      step4: entry.step4,
      step5: entry.step5,
      notes: entry.notes ?? ''
    });
  }

  protected deleteEntry(id: string): void {
    if (!confirm('Delete this worksheet?')) return;
    this.therapyWorksheets.deleteSelfValidation(id).subscribe({
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
    this.resetForm();
    this.message.set(null);
  }

  private loadEntries(): void {
    this.loading.set(true);
    this.error.set(null);
    this.therapyWorksheets
      .getSelfValidationEntries()
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (rows) => this.entries.set(rows),
        error: () => this.error.set('Unable to load saved worksheets.')
      });
  }

  private resetForm(): void {
    this.form.reset({
      step2: { whatHappened: '', howIFelt: '', whatINeeded: '' },
      step3: { acknowledgeFeeling: '', separateIdentity: '', reframeNarrative: '' },
      step4: { externalInfluences: '', boundaries: '' },
      step5: { immediateAction: '', futurePrevention: '', alternativePerspectives: '' },
      notes: ''
    });
  }
}
