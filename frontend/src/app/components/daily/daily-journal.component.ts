import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize } from 'rxjs';
import { DailyJournalEntry, DailyJournalPayload } from '../../models/journal.models';
import { JournalService } from '../../services/journal.service';

type DailyFormGroup = ReturnType<DailyJournalComponent['createForm']>;

@Component({
  selector: 'app-daily-journal',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './daily-journal.component.html',
  styleUrls: ['./daily-journal.component.css']
})
export class DailyJournalComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly journalService = inject(JournalService);

  protected readonly moods = ['happy', 'content', 'neutral', 'tired', 'anxious', 'sad', 'angry'];

  protected readonly entries = signal<DailyJournalEntry[]>([]);
  protected readonly loading = signal(false);
  protected readonly saving = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly editingId = signal<string | null>(null);

  protected form: DailyFormGroup = this.createForm();

  ngOnInit(): void {
    this.loadEntries();
  }

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const payload = this.buildPayload();
    const entryId = this.editingId();

    this.saving.set(true);
    this.error.set(null);

    const request$ = entryId
      ? this.journalService.updateDailyEntry(entryId, payload)
      : this.journalService.createDailyEntry(payload);

    request$
      .pipe(finalize(() => this.saving.set(false)))
      .subscribe({
        next: () => {
          this.resetForm();
          this.loadEntries();
        },
        error: () => this.error.set('Unable to save entry. Please try again.')
      });
  }

  protected editEntry(entry: DailyJournalEntry): void {
    this.editingId.set(entry.id);
    this.form.reset({
      date: this.toInputDate(entry.date),
      entryText: entry.entryText,
      sleepHours: entry.sleepHours,
      waterGlasses: entry.waterGlasses,
      foodNotes: entry.foodNotes,
      mood: entry.mood,
      moodEmoji: entry.moodEmoji ?? '',
      gratitude1: entry.gratitude[0] ?? '',
      gratitude2: entry.gratitude[1] ?? '',
      gratitude3: entry.gratitude[2] ?? ''
    });
  }

  protected deleteEntry(id: string): void {
    if (!confirm('Delete this entry?')) {
      return;
    }

    this.journalService.deleteDailyEntry(id).subscribe({
      next: () => this.loadEntries(),
      error: () => this.error.set('Unable to delete entry right now.')
    });
  }

  protected cancelEdit(): void {
    this.resetForm();
  }

  private loadEntries(): void {
    this.loading.set(true);
    this.error.set(null);

    this.journalService
      .getDailyEntries()
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (entries) => this.entries.set(entries),
        error: () => this.error.set('Unable to load daily entries. Please refresh.')
      });
  }

  private createForm() {
    const today = this.toInputDate(new Date().toISOString());
    return this.fb.nonNullable.group({
      date: [today, [Validators.required]],
      entryText: ['', [Validators.required, Validators.maxLength(2000)]],
      sleepHours: [7, [Validators.required, Validators.min(0), Validators.max(24)]],
      waterGlasses: [8, [Validators.required, Validators.min(0), Validators.max(30)]],
      foodNotes: [''],
      mood: ['', [Validators.required]],
      moodEmoji: [''],
      gratitude1: [''],
      gratitude2: [''],
      gratitude3: ['']
    });
  }

  private resetForm(): void {
    this.editingId.set(null);
    this.form = this.createForm();
  }

  private buildPayload(): DailyJournalPayload {
    const value = this.form.getRawValue();
    const gratitude = [value.gratitude1, value.gratitude2, value.gratitude3]
      .map((item) => item.trim())
      .filter((item) => !!item)
      .slice(0, 3);

    return {
      date: new Date(value.date).toISOString(),
      entryText: value.entryText,
      sleepHours: Number(value.sleepHours),
      waterGlasses: Number(value.waterGlasses),
      foodNotes: value.foodNotes,
      mood: value.mood,
      moodEmoji: value.moodEmoji?.trim() || null,
      gratitude
    };
  }

  private toInputDate(value: string): string {
    const date = new Date(value);
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }
}


