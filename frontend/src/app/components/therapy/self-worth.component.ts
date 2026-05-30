import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import {
  SelfWorthEntry,
  TherapyWorksheetsApiService
} from '../../services/therapy-worksheets-api.service';

@Component({
  selector: 'app-self-worth',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './self-worth.component.html',
  styleUrls: ['./worksheet.component.css']
})
export class SelfWorthComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly therapyWorksheets = inject(TherapyWorksheetsApiService);

  protected readonly saving = signal(false);
  protected readonly message = signal<string | null>(null);
  protected readonly entries = signal<SelfWorthEntry[]>([]);
  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly editingId = signal<string | null>(null);

  protected readonly form = this.fb.nonNullable.group({
    identifyTrigger: ['', [Validators.required]],
    impactAnalysis: ['', [Validators.required]],
    realityCheck: ['', [Validators.required]],
    definesSelfWorth: ['', [Validators.required]],
    doesNotDefineSelfWorth: ['', [Validators.required]],
    actsOfCare: ['', [Validators.required]]
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
      ? this.therapyWorksheets.updateSelfWorth(id, payload)
      : this.therapyWorksheets.createSelfWorth(payload);

    req$.pipe(finalize(() => this.saving.set(false))).subscribe({
      next: () => {
        this.message.set(id ? 'Updated 💎' : 'Saved with kindness 💎');
        this.editingId.set(null);
        this.form.reset();
        this.loadEntries();
      },
      error: (err) =>
        this.message.set(err?.error?.message ?? 'Could not save.')
    });
  }

  protected editEntry(entry: SelfWorthEntry): void {
    this.editingId.set(entry.id);
    this.message.set(null);
    this.form.patchValue(entry);
  }

  protected deleteEntry(id: string): void {
    if (!confirm('Delete this worksheet?')) return;
    this.therapyWorksheets.deleteSelfWorth(id).subscribe({
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
      .getSelfWorthEntries()
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (rows) => this.entries.set(rows),
        error: () => this.error.set('Unable to load saved worksheets.')
      });
  }
}
